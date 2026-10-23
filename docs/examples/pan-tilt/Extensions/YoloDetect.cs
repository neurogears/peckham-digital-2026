using Bonsai;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;

namespace YoloDetect
{
    /// <summary>
    /// The ONNX Runtime execution provider used to run the model. Values from <see cref="Cuda"/>
    /// upwards require the GPU build of the ONNX Runtime native library.
    /// </summary>
    public enum ExecutionProvider
    {
        Cpu,
        Cuda,
        TensorRT
    }

    /// <summary>
    /// Runs a YOLO detection model exported to ONNX on each image and emits the detected objects.
    /// Each image is letterboxed to the model input size, converted to planar RGB in the unit range
    /// directly into the input tensor, run through the model, and the raw [1, 4 + classes, anchors]
    /// output is decoded with a confidence threshold followed by class-agnostic non-maximum
    /// suppression. Bounding boxes are reported in the pixel coordinates of the input image.
    /// </summary>
    [Combinator]
    [DefaultProperty("ModelPath")]
    [WorkflowElementCategory(ElementCategory.Transform)]
    [Description("Runs a YOLO object detection model exported to ONNX on each image and emits the detected objects in image coordinates.")]
    public class YoloDetect
    {
        private ExecutionProvider _executionProvider = ExecutionProvider.Cpu;
        private float _confidenceThreshold = 0.5f;
        private float _iouThreshold = 0.5f;

        [FileNameFilter("ONNX models (*.onnx)|*.onnx|All files (*.*)|*.*")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [Description("The path to the ONNX model file.")]
        public string ModelPath { get; set; }

        [Description("The execution provider used to run the model. CUDA and TensorRT require the GPU build of ONNX Runtime.")]
        public ExecutionProvider ExecutionProvider
        {
            get { return _executionProvider; }
            set { _executionProvider = value; }
        }

        [Description("The index of the GPU used by the CUDA and TensorRT execution providers.")]
        public int DeviceId { get; set; }

        [Range(0, 1)]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The minimum class score for a candidate box to be kept.")]
        public float ConfidenceThreshold
        {
            get { return _confidenceThreshold; }
            set { _confidenceThreshold = value; }
        }

        [Range(0, 1)]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The intersection-over-union above which a box overlapping a more confident one is suppressed.")]
        public float IouThreshold
        {
            get { return _iouThreshold; }
            set { _iouThreshold = value; }
        }

        public IObservable<ObjectDetectionCollection> Process(IObservable<IplImage> source)
        {
            return Observable.Using(
                () => new YoloDetector(ModelPath, ExecutionProvider, DeviceId),
                detector => source.Select(image => detector.Detect(image, ConfidenceThreshold, IouThreshold)));
        }
    }

    /// <summary>
    /// Owns the inference session and the buffers of one subscription. The input tensor is backed by a
    /// pinned array over which one Mat per colour plane is mapped, so the letterboxed image is scaled
    /// into tensor memory by OpenCV without an intermediate floating-point image.
    /// </summary>
    sealed class YoloDetector : IDisposable
    {
        const int DefaultInputSize = 640;
        const int BoxFields = 4;
        const byte PaddingValue = 114;
        static readonly int ClassCount = Enum.GetValues(typeof(ObjectClass)).Length;

        readonly InferenceSession session;
        readonly NamedOnnxValue[] inputs;
        readonly GCHandle inputHandle;
        readonly int inputWidth;
        readonly int inputHeight;
        readonly Mat[] planes;
        readonly IplImage[] planeTemp;
        readonly IplImage letterbox;
        readonly List<Candidate> candidates = new List<Candidate>();
        readonly List<Candidate> kept = new List<Candidate>();
        readonly object gate = new object();
        IplImage colorTemp;
        bool disposed;

        struct Candidate
        {
            public float X1, Y1, X2, Y2, Score;
            public int Class;
        }

        struct PreprocessResult
        {
            public float Scale;
            public int PadX;
            public int PadY;
        }

        public YoloDetector(string modelPath, ExecutionProvider provider, int deviceId)
        {
            if (string.IsNullOrEmpty(modelPath))
                throw new InvalidOperationException("A valid ONNX model path must be specified.");

            using (var options = new SessionOptions())
            {
                if (provider == ExecutionProvider.TensorRT) options.AppendExecutionProvider_Tensorrt(deviceId);
                if (provider >= ExecutionProvider.Cuda) options.AppendExecutionProvider_CUDA(deviceId);
                session = new InferenceSession(modelPath, options);
            }

            try
            {
                var input = session.InputMetadata.First();
                var dimensions = input.Value.Dimensions;
                if (dimensions.Length != 4 || dimensions[1] != 3)
                {
                    throw new InvalidOperationException(
                        string.Format("The model input '{0}' must have shape [1, 3, height, width], but has shape [{1}].", input.Key, string.Join(", ", dimensions)));
                }

                inputHeight = dimensions[2] > 0 ? dimensions[2] : DefaultInputSize;
                inputWidth = dimensions[3] > 0 ? dimensions[3] : DefaultInputSize;
                var planeLength = inputWidth * inputHeight;
                var buffer = new float[3 * planeLength];
                inputHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                var tensor = new DenseTensor<float>(buffer, new[] { 1, 3, inputHeight, inputWidth });
                inputs = new[] { NamedOnnxValue.CreateFromTensor(input.Key, tensor) };

                var basePointer = inputHandle.AddrOfPinnedObject();
                planes = new Mat[3];
                for (int i = 0; i < planes.Length; i++)
                {
                    planes[i] = new Mat(inputHeight, inputWidth, Depth.F32, 1, basePointer + i * planeLength * sizeof(float));
                }

                var size = new Size(inputWidth, inputHeight);
                planeTemp = new IplImage[3];
                for (int i = 0; i < planeTemp.Length; i++)
                {
                    planeTemp[i] = new IplImage(size, IplDepth.U8, 1);
                }
                letterbox = new IplImage(size, IplDepth.U8, 3);
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        public ObjectDetectionCollection Detect(IplImage image, float confidenceThreshold, float iouThreshold)
        {
            var detections = new ObjectDetectionCollection(image);
            if (image == null || image.Width == 0 || image.Height == 0) return detections;
            if (image.Depth != IplDepth.U8)
                throw new InvalidOperationException("The input image must have 8-bit unsigned pixels.");

            // Inference and disposal are mutually exclusive, and frames arriving after disposal are
            // dropped, so a frame still in flight when the subscription ends cannot reach the native
            // session or the pinned input buffer once they have been released.
            lock (gate)
            {
                if (disposed) return detections;

                var preprocessResult = Preprocess(image);
                using (var results = session.Run(inputs))
                {
                    var output = results.First().AsTensor<float>() as DenseTensor<float>;
                    if (output == null)
                        throw new InvalidOperationException("The model output is not a dense single-precision tensor.");

                    Decode(output, image.Size, preprocessResult.Scale, preprocessResult.PadX, preprocessResult.PadY, confidenceThreshold, iouThreshold, detections);
                }
            }

            return detections;
        }

        PreprocessResult Preprocess(IplImage image)
        {
            var frame = EnsureColorFormat(image);
            var scale = Math.Min((float)inputWidth / frame.Width, (float)inputHeight / frame.Height);
            var width = Math.Max(1, (int)Math.Round(frame.Width * scale));
            var height = Math.Max(1, (int)Math.Round(frame.Height * scale));
            var padX = (inputWidth - width) / 2;
            var padY = (inputHeight - height) / 2;

            letterbox.Set(Scalar.All(PaddingValue));
            using (var region = letterbox.GetSubRect(new Rect(padX, padY, width, height)))
            {
                CV.Resize(frame, region, SubPixelInterpolation.Linear);
            }

            // Frames are BGR; the model expects RGB planes scaled to the unit range.
            CV.Split(letterbox, planeTemp[0], planeTemp[1], planeTemp[2], null);
            for (int i = 0; i < planes.Length; i++)
            {
                CV.ConvertScale(planeTemp[2 - i], planes[i], 1.0 / byte.MaxValue, 0);
            }

            return new PreprocessResult { Scale = scale, PadX = padX, PadY = padY };
        }

        IplImage EnsureColorFormat(IplImage image)
        {
            if (image.Channels == 3) return image;
            ColorConversion conversion;
            switch (image.Channels)
            {
                case 1: conversion = ColorConversion.Gray2Bgr; break;
                case 4: conversion = ColorConversion.Bgra2Bgr; break;
                default: throw new InvalidOperationException(string.Format("Images with {0} channels are not supported.", image.Channels));
            }

            if (colorTemp == null)
            {
                colorTemp = new IplImage(image.Size, IplDepth.U8, 3);
            }
            else if (colorTemp.Size != image.Size)
            {
                colorTemp.Dispose();
                colorTemp = new IplImage(image.Size, IplDepth.U8, 3);
            }

            CV.CvtColor(image, colorTemp, conversion);
            return colorTemp;
        }

        void Decode(
            DenseTensor<float> output,
            Size imageSize,
            float scale,
            int padX,
            int padY,
            float confidenceThreshold,
            float iouThreshold,
            ObjectDetectionCollection detections)
        {
            // Ultralytics exports [1, 4 + classes, anchors]; a transposed [1, anchors, 4 + classes]
            // layout is handled by swapping the strides.
            var dimensions = output.Dimensions.ToArray();
            if (dimensions.Length != 3 || dimensions[0] != 1)
                throw new InvalidOperationException(string.Format("Unexpected model output shape [{0}].", string.Join(", ", dimensions.ToArray())));

            int fields, anchors, fieldStride, anchorStride;
            if (dimensions[1] == BoxFields + ClassCount)
            {
                fields = dimensions[1]; anchors = dimensions[2]; fieldStride = anchors; anchorStride = 1;
            }
            else if (dimensions[2] == BoxFields + ClassCount)
            {
                fields = dimensions[2]; anchors = dimensions[1]; fieldStride = 1; anchorStride = fields;
            }
            else
            {
                throw new InvalidOperationException(
                    string.Format("The model output has {0} classes but {1} were expected.", Math.Max(dimensions[1], dimensions[2]) - BoxFields, ClassCount));
            }

            var data = output.Buffer.Span.ToArray();
            candidates.Clear();
            for (int i = 0; i < anchors; i++)
            {
                var anchorOffset = i * anchorStride;
                var bestScore = 0f;
                var bestClass = 0;
                for (int c = BoxFields; c < fields; c++)
                {
                    var score = data[c * fieldStride + anchorOffset];
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestClass = c - BoxFields;
                    }
                }
                if (bestScore < confidenceThreshold) continue;

                var cx = data[0 * fieldStride + anchorOffset];
                var cy = data[1 * fieldStride + anchorOffset];
                var w = data[2 * fieldStride + anchorOffset];
                var h = data[3 * fieldStride + anchorOffset];
                var candidate = new Candidate
                {
                    X1 = Clamp((cx - w / 2 - padX) / scale, 0, imageSize.Width),
                    Y1 = Clamp((cy - h / 2 - padY) / scale, 0, imageSize.Height),
                    X2 = Clamp((cx + w / 2 - padX) / scale, 0, imageSize.Width),
                    Y2 = Clamp((cy + h / 2 - padY) / scale, 0, imageSize.Height),
                    Score = bestScore,
                    Class = bestClass
                };
                if (candidate.X2 > candidate.X1 && candidate.Y2 > candidate.Y1)
                {
                    candidates.Add(candidate);
                }
            }

            candidates.Sort((a, b) => b.Score.CompareTo(a.Score));
            kept.Clear();
            foreach (var candidate in candidates)
            {
                var suppressed = false;
                foreach (var keep in kept)
                {
                    if (IntersectionOverUnion(candidate, keep) > iouThreshold)
                    {
                        suppressed = true;
                        break;
                    }
                }
                if (suppressed) continue;

                kept.Add(candidate);
                var box = new BoundingBox(candidate.X1, candidate.Y1, candidate.X2 - candidate.X1, candidate.Y2 - candidate.Y1);
                detections.Add(new ObjectDetection(box, candidate.Score, (ObjectClass)candidate.Class));
            }
        }

        static float Clamp(float value, float min, float max)
        {
            return value < min ? min : value > max ? max : value;
        }

        static float IntersectionOverUnion(Candidate a, Candidate b)
        {
            var width = Math.Min(a.X2, b.X2) - Math.Max(a.X1, b.X1);
            var height = Math.Min(a.Y2, b.Y2) - Math.Max(a.Y1, b.Y1);
            if (width <= 0 || height <= 0) return 0;
            var intersection = width * height;
            var union = (a.X2 - a.X1) * (a.Y2 - a.Y1) + (b.X2 - b.X1) * (b.Y2 - b.Y1) - intersection;
            return union > 0 ? intersection / union : 0;
        }

        public void Dispose()
        {
            lock (gate)
            {
                if (disposed) return;
                disposed = true;
                Release();
            }
        }

        void Release()
        {
            if (session != null)
                session.Dispose();

            if (planes != null)
            {
                foreach (var plane in planes)
                {
                    if (plane != null)
                        plane.Dispose();
                }
            }

            if (planeTemp != null)
            {
                foreach (var plane in planeTemp)
                {
                    if (plane != null)
                        plane.Dispose();
                }
            }
            if (letterbox != null)
                letterbox.Dispose();
            if (colorTemp != null)
                colorTemp.Dispose();
            if (inputHandle.IsAllocated) inputHandle.Free();
        }
    }
}

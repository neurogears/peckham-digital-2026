using System;
using System.Windows.Forms;
using Bonsai;
using Bonsai.Design;
using Bonsai.Vision;
using Bonsai.Vision.Design;
using OpenCV.Net;

[assembly: TypeVisualizer(typeof(YoloDetect.ObjectDetectionCollectionVisualizer), Target = typeof(YoloDetect.ObjectDetectionCollection))]
[assembly: TypeVisualizer(typeof(YoloDetect.ObjectDetectionOverlay), Target = typeof(MashupSource<ImageMashupVisualizer, YoloDetect.ObjectDetectionCollectionVisualizer>))]

namespace YoloDetect
{

    /// <summary>
    /// Shows the image carried by each detection collection with the detected objects drawn over it.
    /// Bounding boxes and labels can be toggled from the status strip, which opens on right-click,
    /// and both settings are saved with the visualizer layout.
    /// </summary>
    public class ObjectDetectionCollectionVisualizer : IplImageVisualizer
    {
        IplImage canvas;
        ToolStripButton boxesButton;
        ToolStripButton labelsButton;
        bool _drawBoundingBoxes = true;
        bool _drawLabels = true;

        /// <summary>
        /// Whether to draw the bounding box of each detection.
        /// </summary>
        public bool DrawBoundingBoxes
        {
            get { return _drawBoundingBoxes; }
            set { _drawBoundingBoxes = value; }
        }

        /// <summary>
        /// Whether to draw the class and confidence of each detection.
        /// </summary>
        public bool DrawLabels
        {
            get { return _drawLabels; }
            set { _drawLabels = value; }
        }

        public override void Load(IServiceProvider provider)
        {
            base.Load(provider);
            boxesButton = CreateToggle("Bounding Boxes", DrawBoundingBoxes, value => DrawBoundingBoxes = value);
            labelsButton = CreateToggle("Labels", DrawLabels, value => DrawLabels = value);
            StatusStrip.Items.Add(boxesButton);
            StatusStrip.Items.Add(labelsButton);
        }

        static ToolStripButton CreateToggle(string text, bool initialValue, Action<bool> setValue)
        {
            var button = new ToolStripButton(text) { CheckOnClick = true, Checked = initialValue };
            button.CheckedChanged += (sender, e) => setValue(button.Checked);
            return button;
        }

        public override void Show(object value)
        {
            var detections = value as ObjectDetectionCollection;
            if (detections == null) return;
            var image = detections.Image;
            if (image == null) return;

            canvas = IplImageHelper.EnsureColorCopy(canvas, image);
            ObjectDetectionDrawing.Draw(canvas, detections, DrawBoundingBoxes, DrawLabels);
            base.Show(canvas);
        }

        public override void Unload()
        {
            base.Unload();
            canvas = null;
            boxesButton = null;
            labelsButton = null;
        }
    }

    /// <summary>
    /// Draws detections over the image of another image visualizer when the detections are dropped
    /// onto it as a mashup.
    /// </summary>
    public class ObjectDetectionOverlay : DialogTypeVisualizer
    {
        ImageMashupVisualizer visualizer;
        bool _drawBoundingBoxes = true;
        bool _drawLabels = true;

        public bool DrawBoundingBoxes
        {
            get { return _drawBoundingBoxes; }
            set { _drawBoundingBoxes = value; }
        }

        public bool DrawLabels
        {
            get { return _drawLabels; }
            set { _drawLabels = value; }
        }

        public override void Load(IServiceProvider provider)
        {
            visualizer = (ImageMashupVisualizer)provider.GetService(typeof(MashupVisualizer));
        }

        public override void Show(object value)
        {
            if (visualizer == null) return;
            var image = visualizer.VisualizerImage;
            if (image != null && value is ObjectDetectionCollection)
            {
                ObjectDetectionDrawing.Draw(image, (ObjectDetectionCollection)value, DrawBoundingBoxes, DrawLabels);
            }
        }

        public override void Unload()
        {
            visualizer = null;
        }
    }

    /// <summary>
    /// Draws detections onto an image, with line widths and text scaled to the image height and a
    /// distinct colour per object class.
    /// </summary>
    static class ObjectDetectionDrawing
    {
        const float ReferenceHeight = 480;

        public static void Draw(IplImage image, ObjectDetectionCollection detections, bool drawBoxes, bool drawLabels)
        {
            if (image == null || detections == null || detections.Count == 0) return;
            if (!drawBoxes && !drawLabels) return;

            var unit = Math.Max(1, (int)Math.Ceiling(image.Height / ReferenceHeight));
            var font = new Font(FontFace.HersheySimplex, 0.5 * unit, 0.5 * unit, 0, unit, LineFlags.AntiAliased);
            foreach (var detection in detections)
            {
                var box = detection.BoundingBox;
                var color = image.Channels == 1 ? Scalar.Real(255) : ClassColor(detection.ObjectClass);
                var rect = new Rect(
                    (int)Math.Round(box.X),
                    (int)Math.Round(box.Y),
                    (int)Math.Round(box.Width),
                    (int)Math.Round(box.Height));
                if (drawBoxes)
                {
                    CV.Rectangle(image, rect, color, 2 * unit);
                }

                if (drawLabels)
                {
                    var label = string.Format("{0} {1:F2}", detection.ObjectClass, detection.Confidence);
                    var origin = new Point(rect.X, Math.Max(rect.Y - 4 * unit, 12 * unit));
                    CV.PutText(image, label, origin, font, color);
                }
            }
        }

        /// <summary>
        /// Returns a saturated colour for the class, spread around the hue circle so neighbouring
        /// class indices get clearly different colours.
        /// </summary>
        static Scalar ClassColor(ObjectClass objectClass)
        {
            var hue = (int)objectClass * 137.508 % 360;
            var sector = (int)(hue / 60) % 6;
            var fraction = hue / 60 - Math.Floor(hue / 60);
            var rising = 255 * fraction;
            var falling = 255 * (1 - fraction);
            double r, g, b;
            switch (sector)
            {
                case 0: r = 255; g = rising; b = 0; break;
                case 1: r = falling; g = 255; b = 0; break;
                case 2: r = 0; g = 255; b = rising; break;
                case 3: r = 0; g = falling; b = 255; break;
                case 4: r = rising; g = 0; b = 255; break;
                default: r = 255; g = 0; b = falling; break;
            }
            return Scalar.Rgb(r, g, b);
        }
    }
}

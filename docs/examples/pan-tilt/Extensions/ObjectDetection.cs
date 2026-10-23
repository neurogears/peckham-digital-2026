using System.Collections.ObjectModel;
using OpenCV.Net;

namespace YoloDetect
{
    /// <summary>
    /// An axis-aligned box in image pixel coordinates, given by its top-left corner and size.
    /// </summary>
    public struct BoundingBox
    {
        public BoundingBox(float x, float y, float width, float height)
            : this()
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public float X { get; private set; }

        public float Y { get; private set; }

        public float Width { get; private set; }

        public float Height { get; private set; }

        /// <summary>
        /// The center of the box, in image pixel coordinates.
        /// </summary>
        public Point2f Center
        {
            get { return new Point2f(X + Width / 2, Y + Height / 2); }
        }

        public override string ToString()
        {
            return string.Format("({0:F0}, {1:F0}, {2:F0}, {3:F0})", X, Y, Width, Height);
        }
    }

    /// <summary>
    /// A single detected object: its bounding box, confidence score and class.
    /// </summary>
    public sealed class ObjectDetection
    {
        public ObjectDetection(BoundingBox boundingBox, float confidence, ObjectClass objectClass)
        {
            BoundingBox = boundingBox;
            Confidence = confidence;
            ObjectClass = objectClass;
        }

        public BoundingBox BoundingBox { get; private set; }

        public float Confidence { get; private set; }

        public ObjectClass ObjectClass { get; private set; }

        public override string ToString()
        {
            return string.Format("{0} {1:F2} {2}", ObjectClass, Confidence, BoundingBox);
        }
    }

    /// <summary>
    /// The detections found in one image, ordered by decreasing confidence. The source image is
    /// carried alongside the detections so they can be visualized without a separate image stream.
    /// </summary>
    public class ObjectDetectionCollection : Collection<ObjectDetection>
    {
        public ObjectDetectionCollection(IplImage image)
        {
            Image = image;
        }

        /// <summary>
        /// The image in which the objects were detected.
        /// </summary>
        public IplImage Image { get; private set; }
    }
}

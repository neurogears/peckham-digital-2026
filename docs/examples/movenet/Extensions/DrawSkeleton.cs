using Bonsai;
using Bonsai.TensorFlow.MoveNet;
using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Reactive.Linq;

[Combinator]
[Description("Draws every body part of a pose as a circle on its source image and joins the parts along the limbs.")]
[WorkflowElementCategory(ElementCategory.Transform)]
public class DrawSkeleton
{
    // The seventeen MoveNet keypoints and their connected parts.
    static readonly string[][] Links =
    {
        new[] { "nose", "left_eye" },
        new[] { "nose", "right_eye" },
        new[] { "left_eye", "left_ear" },
        new[] { "right_eye", "right_ear" },
        new[] { "nose", "left_shoulder" },
        new[] { "nose", "right_shoulder" },
        new[] { "left_shoulder", "right_shoulder" },
        new[] { "left_shoulder", "left_elbow" },
        new[] { "left_elbow", "left_wrist" },
        new[] { "right_shoulder", "right_elbow" },
        new[] { "right_elbow", "right_wrist" },
        new[] { "left_shoulder", "left_hip" },
        new[] { "right_shoulder", "right_hip" },
        new[] { "left_hip", "right_hip" },
        new[] { "left_hip", "left_knee" },
        new[] { "left_knee", "left_ankle" },
        new[] { "right_hip", "right_knee" },
        new[] { "right_knee", "right_ankle" }
    };

    static readonly Scalar LeftColor = Scalar.Rgb(255, 140, 0);
    static readonly Scalar RightColor = Scalar.Rgb(0, 200, 255);
    static readonly Scalar CenterColor = Scalar.Rgb(255, 255, 0);

    public DrawSkeleton()
    {
        Radius = 6;
        Thickness = 3;
    }

    [Description("The radius of the circle drawn on each body part, in pixels.")]
    public int Radius { get; set; }

    [Description("The thickness of the lines joining body parts, in pixels.")]
    public int Thickness { get; set; }

    public IObservable<IplImage> Process(IObservable<Pose> source)
    {
        return source.Select(pose =>
        {
            var output = pose.Image.Clone();
            foreach (var link in Links)
            {
                BodyPart from, to;
                if (TryGetPart(pose, link[0], out from) && TryGetPart(pose, link[1], out to))
                {
                    CV.Line(output, new Point(from.Position), new Point(to.Position), ColorFor(link[0], link[1]), Thickness, LineFlags.AntiAliased);
                }
            }

            foreach (var part in pose)
            {
                if (IsVisible(part))
                {
                    CV.Circle(output, new Point(part.Position), Radius, ColorFor(part.Name, part.Name), -1, LineFlags.AntiAliased);
                }
            }
            return output;
        });
    }

    static bool TryGetPart(Pose pose, string name, out BodyPart part)
    {
        if (pose.Contains(name))
        {
            part = pose[name];
            return IsVisible(part);
        }
        part = null;
        return false;
    }

    static bool IsVisible(BodyPart part)
    {
        return !float.IsNaN(part.Position.X);
    }

    // Left side parts in one colour, right side in another, anything crossing the middle in a third.
    static Scalar ColorFor(string a, string b)
    {
        var left = a.StartsWith("left_") && b.StartsWith("left_");
        var right = a.StartsWith("right_") && b.StartsWith("right_");
        if (left) return LeftColor;
        if (right) return RightColor;
        return CenterColor;
    }
}

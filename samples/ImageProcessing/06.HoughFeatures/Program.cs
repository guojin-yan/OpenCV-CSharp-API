using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.ImgProc;
using JYPPX.OpenCvSharp.Samples.Common;
using CoreCv2 = JYPPX.OpenCvSharp.Core.Cv2;
using ImgProcCv2 = JYPPX.OpenCvSharp.ImgProc.Cv2;

namespace JYPPX.OpenCvSharp.Samples.ImageProcessing.HoughFeatures
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            string outputDirectory = SampleSupport.GetOutputDirectory(args, "hough-features");
            using (var lineImage = new Mat(SampleSupport.PanelHeight, SampleSupport.PanelWidth, MatType.CV_8UC1, new Scalar(0)))
            using (var circleImage = new Mat(SampleSupport.PanelHeight, SampleSupport.PanelWidth, MatType.CV_8UC1, new Scalar(0)))
            {
                ImgProcCv2.Line(lineImage, new Point(50, 100), new Point(560, 100), new Scalar(255), 3);
                ImgProcCv2.Line(lineImage, new Point(70, 300), new Point(500, 150), new Scalar(255), 4);
                ImgProcCv2.Circle(circleImage, new Point(470, 225), 66, new Scalar(255), 3);
                HoughLine[] lines = ImgProcCv2.HoughLines(lineImage, 1.0, Math.PI / 180.0, 100);
                Vec4i[] segments = ImgProcCv2.HoughLinesP(lineImage, 1.0, Math.PI / 180.0, 60, 80, 12);
                HoughCircle[] circles = ImgProcCv2.HoughCircles(circleImage, HoughModes.Gradient, 1.0, 40, 80, 15, 40, 90);

                using (Mat combined = CoreCv2.Max(lineImage, circleImage))
                using (Mat panel = CoreCv2.Merge(new[] { combined, combined, combined }))
                {
                    foreach (Vec4i segment in segments)
                    {
                        ImgProcCv2.Line(panel, new Point(segment.V0, segment.V1), new Point(segment.V2, segment.V3),
                            new Scalar(52, 226, 164), 3, LineTypes.AntiAlias);
                    }
                    foreach (HoughCircle circle in circles)
                    {
                        ImgProcCv2.Circle(panel, new Point((int)circle.X, (int)circle.Y), (int)circle.Radius,
                            new Scalar(92, 224, 255), 3, LineTypes.AntiAlias);
                    }
                    SampleSupport.AddPanelLabel(panel, "HOUGH LINES + CIRCLES", new Scalar(92, 224, 255));
                    SampleSupport.AddMetric(panel, "Lines " + lines.Length + "  Segments " + segments.Length + "  Circles " + circles.Length);
                    SampleSupport.WritePng(outputDirectory, "hough-features.png", panel);
                }

                SampleSupport.WriteSummary("Hough features", outputDirectory,
                    "lines=" + lines.Length + ", segments=" + segments.Length + ", circles=" + circles.Length);
            }
        }
    }
}

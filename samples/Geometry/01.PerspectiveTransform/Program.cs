using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.ImgProc;
using JYPPX.OpenCvSharp.Samples.Common;
using ImgProcCv2 = JYPPX.OpenCvSharp.ImgProc.Cv2;

namespace JYPPX.OpenCvSharp.Samples.Geometry.PerspectiveTransform
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            string outputDirectory = SampleSupport.GetOutputDirectory(args, "perspective-transform");
            using (Mat source = SampleSupport.CreateSourceImage())
            {
                Point2f[] sourceQuad =
                {
                    new Point2f(40, 80), new Point2f(600, 80),
                    new Point2f(600, 330), new Point2f(40, 330)
                };
                Point2f[] destinationQuad =
                {
                    new Point2f(95, 55), new Point2f(560, 95),
                    new Point2f(605, 315), new Point2f(45, 285)
                };
                using (Mat transform = ImgProcCv2.GetPerspectiveTransform(sourceQuad, destinationQuad))
                using (var warped = new Mat())
                {
                    ImgProcCv2.WarpPerspective(source, warped, transform, source.Size,
                        InterpolationFlags.Linear, BorderTypes.Constant, new Scalar(20, 22, 26));
                    for (int i = 0; i < destinationQuad.Length; i++)
                    {
                        Point2f current = destinationQuad[i];
                        Point2f next = destinationQuad[(i + 1) % destinationQuad.Length];
                        ImgProcCv2.Line(warped, new Point((int)current.X, (int)current.Y), new Point((int)next.X, (int)next.Y),
                            new Scalar(92, 224, 255), 3, LineTypes.AntiAlias);
                    }
                    SampleSupport.AddPanelLabel(warped, "PERSPECTIVE TRANSFORM", new Scalar(92, 224, 255));
                    SampleSupport.AddMetric(warped, "Homography  3x3");
                    SampleSupport.WritePng(outputDirectory, "perspective-transform.png", warped);
                    SampleSupport.WriteSummary("Perspective transform", outputDirectory,
                        "matrix=" + transform.Rows + "x" + transform.Cols + ", destinationPoints=4");
                }
            }
        }
    }
}

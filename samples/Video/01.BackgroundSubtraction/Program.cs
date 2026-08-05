using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.ImgProc;
using JYPPX.OpenCvSharp.Samples.Common;
using JYPPX.OpenCvSharp.Video;
using CoreCv2 = JYPPX.OpenCvSharp.Core.Cv2;
using ImgProcCv2 = JYPPX.OpenCvSharp.ImgProc.Cv2;

namespace JYPPX.OpenCvSharp.Samples.Video.BackgroundSubtraction
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            string outputDirectory = SampleSupport.GetOutputDirectory(args, "background-subtraction");
            using (BackgroundSubtractorMOG2 subtractor = BackgroundSubtractorMOG2.Create(history: 24, varThreshold: 14, detectShadows: false))
            using (var mask = new Mat())
            {
                Mat? finalFrame = null;
                try
                {
                    for (int frameIndex = 0; frameIndex < 30; frameIndex++)
                    {
                        using (var frame = new Mat(SampleSupport.PanelHeight, SampleSupport.PanelWidth, MatType.CV_8UC3,
                            new Scalar(28, 32, 38)))
                        {
                            ImgProcCv2.Rectangle(frame, new Rect(0, 0, frame.Cols, 76), new Scalar(40, 46, 54), -1);
                            ImgProcCv2.Rectangle(frame, new Rect(30 + (frameIndex * 12), 150, 92, 72),
                                new Scalar(72, 210, 252), -1);
                            ImgProcCv2.Circle(frame, new Point(520, 240), 52, new Scalar(70, 170, 98), -1);
                            subtractor.Apply(frame, mask, frameIndex < 8 ? 0.35 : 0.02);
                            if (frameIndex == 29) finalFrame = frame.Clone();
                        }
                    }

                    using (var cleaned = new Mat())
                    using (Mat kernel = ImgProcCv2.GetStructuringElement(MorphShapes.Ellipse, new Size(7, 7)))
                    {
                        ImgProcCv2.MorphologyEx(mask, cleaned, MorphTypes.Open, kernel);
                        int foregroundPixels = CoreCv2.CountNonZero(cleaned);
                        using (Mat maskBgr = CoreCv2.Merge(new[] { cleaned, cleaned, cleaned }))
                        using (var highlighted = new Mat())
                        {
                            CoreCv2.BitwiseAnd(finalFrame!, maskBgr, highlighted);
                            SampleSupport.AddPanelLabel(highlighted, "MOG2 BACKGROUND SUBTRACTION", new Scalar(72, 210, 252));
                            SampleSupport.AddMetric(highlighted, "Foreground  " + foregroundPixels);
                            SampleSupport.WritePng(outputDirectory, "foreground-mask.png", cleaned);
                            SampleSupport.WritePng(outputDirectory, "background-subtraction.png", highlighted);
                        }
                        SampleSupport.WriteSummary("MOG2 background subtraction", outputDirectory,
                            "frames=30, foregroundPixels=" + foregroundPixels + ", history=" + subtractor.History);
                    }
                }
                finally
                {
                    finalFrame?.Dispose();
                }
            }
        }
    }
}

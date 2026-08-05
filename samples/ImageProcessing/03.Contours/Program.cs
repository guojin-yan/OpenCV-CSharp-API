using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.ImgProc;
using JYPPX.OpenCvSharp.Samples.Common;
using ImgProcCv2 = JYPPX.OpenCvSharp.ImgProc.Cv2;

namespace JYPPX.OpenCvSharp.Samples.ImageProcessing.Contours
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            string outputDirectory = SampleSupport.GetOutputDirectory(args, "contours");
            using (Mat source = SampleSupport.CreateSourceImage())
            using (var gray = new Mat())
            using (var binary = new Mat())
            {
                ImgProcCv2.CvtColor(source, gray, ColorConversionCodes.BGR2GRAY);
                ImgProcCv2.Threshold(gray, binary, 82, 255, ThresholdTypes.Binary);
                ImgProcCv2.FindContours(binary, out Point[][] contours, out Vec4i[] hierarchy,
                    RetrievalModes.External, ContourApproximationModes.ApproxSimple);

                using (Mat panel = source.Clone())
                {
                    if (contours.Length > 0)
                    {
                        ImgProcCv2.DrawContours(panel, contours, -1, new Scalar(52, 226, 164), 4,
                            LineTypes.AntiAlias, hierarchy);
                    }
                    SampleSupport.AddPanelLabel(panel, "CONTOURS", new Scalar(52, 226, 164));
                    SampleSupport.AddMetric(panel, "Objects  " + contours.Length);
                    SampleSupport.WritePng(outputDirectory, "contours.png", panel);
                    SampleSupport.WriteSummary("Threshold and contours", outputDirectory,
                        "contours=" + contours.Length);
                }
            }
        }
    }
}

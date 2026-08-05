using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.ImgProc;
using JYPPX.OpenCvSharp.Samples.PublishedPackageSamples;
using ImgProcCv2 = JYPPX.OpenCvSharp.ImgProc.Cv2;

namespace JYPPX.OpenCvSharp.Samples.Case03Contours
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            string outputDirectory = PublishedPackageSampleSupport.GetOutputDirectory(args, "published-package-contours");
            using (Mat source = PublishedPackageSampleSupport.CreateSourceImage())
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
                    PublishedPackageSampleSupport.AddPanelLabel(panel, "03  CONTOURS", new Scalar(52, 226, 164));
                    PublishedPackageSampleSupport.AddMetric(panel, "Objects  " + contours.Length);
                    PublishedPackageSampleSupport.WritePng(outputDirectory, "contours.png", panel);
                    PublishedPackageSampleSupport.WriteSummary("Threshold and contours", outputDirectory,
                        "contours=" + contours.Length);
                }
            }
        }
    }
}

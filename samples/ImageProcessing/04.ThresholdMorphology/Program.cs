using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.ImgProc;
using JYPPX.OpenCvSharp.Samples.Common;
using CoreCv2 = JYPPX.OpenCvSharp.Core.Cv2;
using ImgProcCv2 = JYPPX.OpenCvSharp.ImgProc.Cv2;

namespace JYPPX.OpenCvSharp.Samples.ImageProcessing.ThresholdMorphology
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            string outputDirectory = SampleSupport.GetOutputDirectory(args, "threshold-morphology");
            using (Mat source = SampleSupport.CreateSourceImage())
            using (var gray = new Mat())
            using (var binary = new Mat())
            using (Mat kernel = ImgProcCv2.GetStructuringElement(MorphShapes.Ellipse, new Size(9, 9)))
            using (var opened = new Mat())
            using (var closed = new Mat())
            {
                ImgProcCv2.CvtColor(source, gray, ColorConversionCodes.BGR2GRAY);
                ImgProcCv2.Threshold(gray, binary, 0, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);
                ImgProcCv2.MorphologyEx(binary, opened, MorphTypes.Open, kernel, iterations: 1);
                ImgProcCv2.MorphologyEx(opened, closed, MorphTypes.Close, kernel, iterations: 2);
                int foregroundPixels = CoreCv2.CountNonZero(closed);

                using (Mat panel = CoreCv2.Merge(new[] { closed, closed, closed }))
                {
                    SampleSupport.AddPanelLabel(panel, "THRESHOLD + MORPHOLOGY", new Scalar(92, 224, 255));
                    SampleSupport.AddMetric(panel, "Foreground  " + foregroundPixels);
                    SampleSupport.WritePng(outputDirectory, "binary.png", binary);
                    SampleSupport.WritePng(outputDirectory, "threshold-morphology.png", panel);
                }

                SampleSupport.WriteSummary("Threshold and morphology", outputDirectory,
                    "kernel=ellipse-9x9, foregroundPixels=" + foregroundPixels);
            }
        }
    }
}

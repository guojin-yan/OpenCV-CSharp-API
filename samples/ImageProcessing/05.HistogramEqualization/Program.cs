using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.ImgProc;
using JYPPX.OpenCvSharp.Samples.Common;
using CoreCv2 = JYPPX.OpenCvSharp.Core.Cv2;
using ImgProcCv2 = JYPPX.OpenCvSharp.ImgProc.Cv2;

namespace JYPPX.OpenCvSharp.Samples.ImageProcessing.HistogramEqualization
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            string outputDirectory = SampleSupport.GetOutputDirectory(args, "histogram-equalization");
            using (Mat source = SampleSupport.CreateSourceImage())
            using (var gray = new Mat())
            using (var equalized = new Mat())
            using (var histogram = new Mat())
            {
                ImgProcCv2.CvtColor(source, gray, ColorConversionCodes.BGR2GRAY);
                ImgProcCv2.EqualizeHist(gray, equalized);
                ImgProcCv2.CalcHist(equalized, 0, null, histogram, 256, 0, 256);
                float[] bins = histogram.ToArray<float>();
                float maximum = 1.0F;
                for (int i = 0; i < bins.Length; i++) maximum = Math.Max(maximum, bins[i]);

                using (Mat panel = CoreCv2.Merge(new[] { equalized, equalized, equalized }))
                {
                    int baseline = panel.Rows - 62;
                    for (int x = 1; x < bins.Length; x++)
                    {
                        int x0 = ((x - 1) * (panel.Cols - 1)) / 255;
                        int x1 = (x * (panel.Cols - 1)) / 255;
                        int y0 = baseline - (int)Math.Round((bins[x - 1] / maximum) * 120.0F);
                        int y1 = baseline - (int)Math.Round((bins[x] / maximum) * 120.0F);
                        ImgProcCv2.Line(panel, new Point(x0, y0), new Point(x1, y1), new Scalar(52, 226, 164), 2);
                    }
                    SampleSupport.AddPanelLabel(panel, "HISTOGRAM EQUALIZATION", new Scalar(52, 226, 164));
                    SampleSupport.AddMetric(panel, "Bins  256");
                    SampleSupport.WritePng(outputDirectory, "histogram-equalization.png", panel);
                }

                SampleSupport.WriteSummary("Histogram equalization", outputDirectory,
                    "bins=256, maximumBin=" + maximum);
            }
        }
    }
}

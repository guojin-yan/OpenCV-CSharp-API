using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.ImgProc;
using CoreCv2 = JYPPX.OpenCvSharp.Core.Cv2;
using ImgProcCv2 = JYPPX.OpenCvSharp.ImgProc.Cv2;
using JYPPX.OpenCvSharp.Samples.Common;

namespace JYPPX.OpenCvSharp.Samples.ImageProcessing.ImagePipeline
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            string outputDirectory = SampleSupport.GetOutputDirectory(args, "image-pipeline");
            using (Mat source = SampleSupport.CreateSourceImage())
            using (var gray = new Mat())
            using (var blurred = new Mat())
            using (var edges = new Mat())
            {
                ImgProcCv2.CvtColor(source, gray, ColorConversionCodes.BGR2GRAY);
                ImgProcCv2.GaussianBlur(gray, blurred, new Size(7, 7), 1.4);
                ImgProcCv2.Canny(blurred, edges, 45, 135);
                int edgePixels = CoreCv2.CountNonZero(edges);

                using (Mat panel = CoreCv2.Merge(new[] { edges, edges, edges }))
                {
                    SampleSupport.AddPanelLabel(panel, "IMAGE PIPELINE", new Scalar(52, 226, 164));
                    SampleSupport.AddMetric(panel, "Canny edges  " + edgePixels);
                    SampleSupport.WritePng(outputDirectory, "source.png", source);
                    SampleSupport.WritePng(outputDirectory, "image-pipeline.png", panel);
                }

                SampleSupport.WriteSummary("Image pipeline", outputDirectory, "edgePixels=" + edgePixels);
            }
        }
    }
}

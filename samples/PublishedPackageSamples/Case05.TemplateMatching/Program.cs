using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.ImgProc;
using JYPPX.OpenCvSharp.Samples.PublishedPackageSamples;
using CoreCv2 = JYPPX.OpenCvSharp.Core.Cv2;
using ImgProcCv2 = JYPPX.OpenCvSharp.ImgProc.Cv2;

namespace JYPPX.OpenCvSharp.Samples.Case05TemplateMatching
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            string outputDirectory = PublishedPackageSampleSupport.GetOutputDirectory(args, "published-package-template");
            using (Mat source = PublishedPackageSampleSupport.CreateSourceImage())
            {
                var templateRect = new Rect(346, 106, 122, 168);
                using (Mat templateView = source.SubMat(templateRect))
                using (Mat template = templateView.Clone())
                using (Mat response = ImgProcCv2.MatchTemplate(source, template, TemplateMatchModes.CCoeffNormed))
                {
                    MinMaxLocResult extrema = CoreCv2.MinMaxLoc(response);
                    Point location = extrema.MaxLoc;
                    double confidence = extrema.MaxVal;
                    using (Mat panel = source.Clone())
                    {
                        ImgProcCv2.Rectangle(panel, new Rect(location, template.Size), new Scalar(72, 220, 255), 5,
                            LineTypes.AntiAlias);
                        PublishedPackageSampleSupport.AddPanelLabel(panel, "05  TEMPLATE MATCH", new Scalar(72, 220, 255));
                        PublishedPackageSampleSupport.AddMetric(panel, "Confidence  " + PublishedPackageSampleSupport.Format(confidence));
                        PublishedPackageSampleSupport.WritePng(outputDirectory, "template-match.png", panel);
                        PublishedPackageSampleSupport.WriteSummary("Template matching", outputDirectory,
                            "location=" + location + ", confidence=" + PublishedPackageSampleSupport.Format(confidence));
                    }
                }
            }
        }
    }
}

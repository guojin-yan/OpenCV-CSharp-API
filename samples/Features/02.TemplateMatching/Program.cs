using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.ImgProc;
using JYPPX.OpenCvSharp.Samples.Common;
using CoreCv2 = JYPPX.OpenCvSharp.Core.Cv2;
using ImgProcCv2 = JYPPX.OpenCvSharp.ImgProc.Cv2;

namespace JYPPX.OpenCvSharp.Samples.Features.TemplateMatching
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            string outputDirectory = SampleSupport.GetOutputDirectory(args, "template-matching");
            using (Mat source = SampleSupport.CreateSourceImage())
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
                        SampleSupport.AddPanelLabel(panel, "TEMPLATE MATCH", new Scalar(72, 220, 255));
                        SampleSupport.AddMetric(panel, "Confidence  " + SampleSupport.Format(confidence));
                        SampleSupport.WritePng(outputDirectory, "template-match.png", panel);
                        SampleSupport.WriteSummary("Template matching", outputDirectory,
                            "location=" + location + ", confidence=" + SampleSupport.Format(confidence));
                    }
                }
            }
        }
    }
}

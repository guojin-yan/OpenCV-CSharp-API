using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.ImgProc;
using JYPPX.OpenCvSharp.Samples.PublishedPackageSamples;
using ImgProcCv2 = JYPPX.OpenCvSharp.ImgProc.Cv2;

namespace JYPPX.OpenCvSharp.Samples.Case02ChinesePutText
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            string outputDirectory = PublishedPackageSampleSupport.GetOutputDirectory(args, "published-package-text");
            string fontPath = PublishedPackageSampleSupport.ResolveCjkFontPath(args.Length > 1 ? args[1] : null);
            const string headline = "OpenCV 中文写字";
            const string detail = "图像处理 UTF-8";
            var panel = new Mat(PublishedPackageSampleSupport.PanelHeight,
                PublishedPackageSampleSupport.PanelWidth, MatType.CV_8UC3, new Scalar(31, 35, 42));
            try
            {
                ImgProcCv2.Rectangle(panel, new Rect(32, 92, PublishedPackageSampleSupport.PanelWidth - 64, 192),
                    new Scalar(43, 49, 58), -1);
                Rect bounds;
                Point next;
                using (var fontFace = new FontFace(fontPath))
                {
                    var origin = new Point(54, 145);
                    bounds = ImgProcCv2.GetTextSize(panel.Size, headline, origin, fontFace, 42, weight: 500);
                    next = ImgProcCv2.PutText(panel, headline, origin, new Scalar(92, 224, 255), fontFace, 42, weight: 500);
                    ImgProcCv2.PutText(panel, detail, new Point(54, 224), new Scalar(232, 240, 245), fontFace, 30, weight: 400);
                    ImgProcCv2.Rectangle(panel, bounds, new Scalar(92, 224, 255), 1, LineTypes.AntiAlias);
                }

                PublishedPackageSampleSupport.AddPanelLabel(panel, "02  OPENCV PUTTEXT + UTF-8", new Scalar(92, 224, 255));
                PublishedPackageSampleSupport.AddMetric(panel, "Bounds  " + bounds.Width + "x" + bounds.Height);
                PublishedPackageSampleSupport.WritePng(outputDirectory, "chinese-text.png", panel);
                PublishedPackageSampleSupport.WriteSummary("OpenCV Chinese putText", outputDirectory,
                    "font=" + fontPath + ", bounds=" + bounds + ", next=" + next);
            }
            finally
            {
                panel.Dispose();
            }
        }
    }
}

using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.ImgProc;
using JYPPX.OpenCvSharp.Samples.Common;
using ImgProcCv2 = JYPPX.OpenCvSharp.ImgProc.Cv2;

namespace JYPPX.OpenCvSharp.Samples.ImageProcessing.ChinesePutText
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            string outputDirectory = SampleSupport.GetOutputDirectory(args, "chinese-puttext");
            string fontPath = SampleSupport.ResolveCjkFontPath(args.Length > 1 ? args[1] : null);
            const string headline = "OpenCV 中文写字";
            const string detail = "图像处理 UTF-8";
            var panel = new Mat(SampleSupport.PanelHeight,
                SampleSupport.PanelWidth, MatType.CV_8UC3, new Scalar(31, 35, 42));
            try
            {
                ImgProcCv2.Rectangle(panel, new Rect(32, 92, SampleSupport.PanelWidth - 64, 192),
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

                SampleSupport.AddPanelLabel(panel, "OPENCV PUTTEXT + UTF-8", new Scalar(92, 224, 255));
                SampleSupport.AddMetric(panel, "Bounds  " + bounds.Width + "x" + bounds.Height);
                SampleSupport.WritePng(outputDirectory, "chinese-text.png", panel);
                SampleSupport.WriteSummary("OpenCV Chinese putText", outputDirectory,
                    "font=" + fontPath + ", bounds=" + bounds + ", next=" + next);
            }
            finally
            {
                panel.Dispose();
            }
        }
    }
}

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using JYPPX.OpenCvSharp;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.ImgCodecs;
using JYPPX.OpenCvSharp.ImgProc;
using ImgCodecsCv2 = JYPPX.OpenCvSharp.ImgCodecs.Cv2;
using ImgProcCv2 = JYPPX.OpenCvSharp.ImgProc.Cv2;

namespace JYPPX.OpenCvSharp.Samples.PublishedPackageSamples
{
    internal static class PublishedPackageSampleSupport
    {
        public const int PanelWidth = 640;
        public const int PanelHeight = 360;

        public static string GetOutputDirectory(string[] args, string defaultName)
        {
            string path = args.Length > 0 && !string.IsNullOrWhiteSpace(args[0])
                ? args[0]
                : Path.Combine(Environment.CurrentDirectory, "artifacts", defaultName);
            string fullPath = Path.GetFullPath(path);
            Directory.CreateDirectory(fullPath);
            return fullPath;
        }

        public static string ResolveCjkFontPath(string? requestedPath)
        {
            string? configuredPath = string.IsNullOrWhiteSpace(requestedPath)
                ? Environment.GetEnvironmentVariable("OPENCV_CSHARP_CJK_FONT")
                : requestedPath;
            if (!string.IsNullOrWhiteSpace(configuredPath))
            {
                string fullPath = Path.GetFullPath(configuredPath);
                if (!File.Exists(fullPath))
                {
                    throw new FileNotFoundException("The configured CJK font file was not found.", fullPath);
                }
                return fullPath;
            }

            string fontsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
            string[] candidates =
            {
                Path.Combine(fontsDirectory, "Deng.ttf"),
                Path.Combine(fontsDirectory, "msyh.ttc"),
                Path.Combine(fontsDirectory, "simhei.ttf"),
                Path.Combine(fontsDirectory, "simsun.ttc"),
                "/usr/share/fonts/opentype/noto/NotoSansCJK-Regular.ttc",
                "/usr/share/fonts/opentype/noto/NotoSansCJK-Regular.otf",
                "/usr/share/fonts/truetype/wqy/wqy-zenhei.ttc",
                "/System/Library/Fonts/PingFang.ttc"
            };

            foreach (string candidate in candidates)
            {
                if (!string.IsNullOrWhiteSpace(candidate) && File.Exists(candidate))
                {
                    return Path.GetFullPath(candidate);
                }
            }

            throw new FileNotFoundException(
                "No CJK font was found. Pass a TTF/TTC font as the second argument or set OPENCV_CSHARP_CJK_FONT. " +
                "The font must contain the Chinese glyphs used by the tutorial.");
        }

        public static Mat CreateSourceImage()
        {
            var image = new Mat(PanelHeight, PanelWidth, MatType.CV_8UC3, new Scalar(24, 28, 34));
            ImgProcCv2.Rectangle(image, new Rect(0, 0, PanelWidth, 74), new Scalar(38, 44, 52), -1);
            ImgProcCv2.Rectangle(image, new Rect(44, 112, 228, 184), new Scalar(235, 168, 44), -1);
            ImgProcCv2.Rectangle(image, new Rect(62, 130, 192, 148), new Scalar(248, 194, 73), 4);
            ImgProcCv2.Circle(image, new Point(406, 190), 86, new Scalar(77, 205, 118), -1);
            ImgProcCv2.Circle(image, new Point(406, 190), 52, new Scalar(39, 118, 72), 5);
            ImgProcCv2.Line(image, new Point(500, 290), new Point(602, 118), new Scalar(232, 92, 76), 15, LineTypes.AntiAlias);
            ImgProcCv2.Line(image, new Point(506, 122), new Point(604, 290), new Scalar(232, 92, 76), 15, LineTypes.AntiAlias);
            ImgProcCv2.PutText(image, "OpenCV CSharp API", new Point(42, 49), HersheyFonts.HersheySimplex, 1.05,
                new Scalar(248, 250, 252), 2, LineTypes.AntiAlias);
            ImgProcCv2.PutText(image, "IMAGE", new Point(99, 215), HersheyFonts.HersheyDuplex, 1.15,
                new Scalar(42, 48, 56), 3, LineTypes.AntiAlias);
            ImgProcCv2.PutText(image, "5.0", new Point(366, 207), HersheyFonts.HersheyDuplex, 1.25,
                new Scalar(244, 252, 247), 3, LineTypes.AntiAlias);
            return image;
        }

        public static void AddPanelLabel(Mat panel, string text, Scalar accent)
        {
            ImgProcCv2.Rectangle(panel, new Rect(0, 0, panel.Cols, 62), new Scalar(24, 28, 34), -1);
            ImgProcCv2.Rectangle(panel, new Rect(0, 0, 10, 62), accent, -1);
            ImgProcCv2.PutText(panel, text, new Point(28, 40), HersheyFonts.HersheyDuplex, 0.85,
                new Scalar(248, 250, 252), 2, LineTypes.AntiAlias);
        }

        public static void AddMetric(Mat panel, string text)
        {
            Size textSize = ImgProcCv2.GetTextSize(text, HersheyFonts.HersheySimplex, 0.58, 1, out _);
            int left = Math.Max(16, panel.Cols - textSize.Width - 28);
            ImgProcCv2.Rectangle(panel, new Rect(left - 12, panel.Rows - 50, textSize.Width + 24, 36), new Scalar(24, 28, 34), -1);
            ImgProcCv2.PutText(panel, text, new Point(left, panel.Rows - 25), HersheyFonts.HersheySimplex, 0.58,
                new Scalar(242, 245, 248), 1, LineTypes.AntiAlias);
        }

        public static void WritePng(string outputDirectory, string fileName, Mat image)
        {
            string path = Path.Combine(outputDirectory, fileName);
            if (!ImgCodecsCv2.ImWrite(path, image, new[] { (int)ImwriteFlags.PngCompression, 4 }))
            {
                throw new IOException("OpenCV did not write sample image: " + path);
            }
        }

        public static void WriteSummary(string caseName, string outputDirectory, string details)
        {
            string packageVersion = Assembly.GetEntryAssembly()?
                .GetCustomAttributes<AssemblyMetadataAttribute>()
                .FirstOrDefault(attribute => attribute.Key == "PublishedOpenCvPackageVersion")?.Value ?? "unknown";
            Console.WriteLine("Published packages: " + packageVersion);
            Console.WriteLine("Managed namespace: JYPPX.OpenCvSharp");
            Console.WriteLine("Runtime package: JYPPX.OpenCV.runtime.win-x64");
            Console.WriteLine(OpenCvSharpBuildInfo.GetDisplayString());
            Console.WriteLine("Case: " + caseName);
            Console.WriteLine("Output: " + outputDirectory);
            Console.WriteLine(details);
        }

        public static string Format(double value)
        {
            return value.ToString("0.000", CultureInfo.InvariantCulture);
        }
    }
}

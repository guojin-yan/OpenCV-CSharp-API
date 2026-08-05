using System;
using System.Linq;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.ImgProc;
using JYPPX.OpenCvSharp.Samples.Common;
using ImgProcCv2 = JYPPX.OpenCvSharp.ImgProc.Cv2;

namespace JYPPX.OpenCvSharp.Samples.Geometry.DocumentScanner
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            string outputDirectory = SampleSupport.GetOutputDirectory(args, "document-scanner");
            using Mat scene = CreateDocumentScene();
            using var gray = new Mat();
            using var blurred = new Mat();
            using var edges = new Mat();
            ImgProcCv2.CvtColor(scene, gray, ColorConversionCodes.BGR2GRAY);
            ImgProcCv2.GaussianBlur(gray, blurred, new Size(5, 5), 0);
            ImgProcCv2.Canny(blurred, edges, 55, 150);
            ImgProcCv2.FindContours(edges, out Point[][] contours, out _,
                RetrievalModes.List, ContourApproximationModes.ApproxSimple);

            Point[] documentContour = contours
                .Select(contour => new
                {
                    Area = Math.Abs(ImgProcCv2.ContourArea(contour)),
                    Polygon = ImgProcCv2.ApproxPolyDP(contour, ImgProcCv2.ArcLength(contour, true) * 0.02, true)
                })
                .Where(candidate => candidate.Area > 50000 && candidate.Polygon.Length == 4 && ImgProcCv2.IsContourConvex(candidate.Polygon))
                .OrderByDescending(candidate => candidate.Area)
                .Select(candidate => candidate.Polygon)
                .FirstOrDefault() ?? throw new InvalidOperationException("The synthetic document quadrilateral was not detected.");

            Point2f[] ordered = OrderCorners(documentContour);
            Point2f[] outputCorners =
            {
                new Point2f(0, 0), new Point2f(519, 0),
                new Point2f(519, 359), new Point2f(0, 359)
            };
            using Mat transform = ImgProcCv2.GetPerspectiveTransform(ordered, outputCorners);
            using var scannedColor = new Mat();
            using var scannedGray = new Mat();
            using var scanned = new Mat();
            ImgProcCv2.WarpPerspective(scene, scannedColor, transform, new Size(520, 360));
            ImgProcCv2.CvtColor(scannedColor, scannedGray, ColorConversionCodes.BGR2GRAY);
            ImgProcCv2.AdaptiveThreshold(scannedGray, scanned, 255, AdaptiveThresholdTypes.GaussianC,
                ThresholdTypes.Binary, 21, 8);

            using Mat detected = scene.Clone();
            ImgProcCv2.Polylines(detected, documentContour, true, new Scalar(58, 232, 154), 5, LineTypes.AntiAlias);
            SampleSupport.AddPanelLabel(detected, "DOCUMENT DETECTION", new Scalar(58, 232, 154));
            using Mat scannedPanel = JYPPX.OpenCvSharp.Core.Cv2.Merge(new[] { scanned, scanned, scanned });
            SampleSupport.AddPanelLabel(scannedPanel, "RECTIFIED DOCUMENT", new Scalar(92, 198, 255));
            SampleSupport.AddMetric(scannedPanel, "Contour  " + Math.Round(Math.Abs(ImgProcCv2.ContourArea(documentContour))));

            SampleSupport.WritePng(outputDirectory, "document-detection.png", detected);
            SampleSupport.WritePng(outputDirectory, "document-scan.png", scannedPanel);
            SampleSupport.WriteSummary("Document scanning pipeline", outputDirectory,
                "contours=" + contours.Length + ", detectedCorners=4, output=520x360");
        }

        private static Mat CreateDocumentScene()
        {
            using var document = new Mat(360, 520, MatType.CV_8UC3, new Scalar(246, 246, 242));
            ImgProcCv2.Rectangle(document, new Rect(0, 0, 520, 60), new Scalar(42, 49, 58), -1);
            ImgProcCv2.PutText(document, "COMPUTER VISION REPORT", new Point(28, 39),
                HersheyFonts.HersheyDuplex, 0.78, new Scalar(250, 250, 248), 2, LineTypes.AntiAlias);
            ImgProcCv2.PutText(document, "OpenCV CSharp API", new Point(34, 104),
                HersheyFonts.HersheyDuplex, 0.82, new Scalar(32, 38, 44), 2, LineTypes.AntiAlias);
            for (int row = 0; row < 7; row++)
            {
                int y = 137 + (row * 27);
                ImgProcCv2.Line(document, new Point(36, y), new Point(480 - ((row % 3) * 42), y),
                    new Scalar(125, 130, 134), 2, LineTypes.AntiAlias);
            }
            ImgProcCv2.Rectangle(document, new Rect(350, 255, 126, 67), new Scalar(52, 194, 132), 3);
            ImgProcCv2.PutText(document, "VERIFIED", new Point(363, 296),
                HersheyFonts.HersheyDuplex, 0.62, new Scalar(42, 145, 98), 2, LineTypes.AntiAlias);

            Point2f[] source =
            {
                new Point2f(0, 0), new Point2f(519, 0),
                new Point2f(519, 359), new Point2f(0, 359)
            };
            Point2f[] destination =
            {
                new Point2f(92, 70), new Point2f(566, 43),
                new Point2f(607, 427), new Point2f(46, 445)
            };
            using Mat transform = ImgProcCv2.GetPerspectiveTransform(source, destination);
            var scene = new Mat();
            ImgProcCv2.WarpPerspective(document, scene, transform, new Size(640, 480),
                borderValue: new Scalar(28, 33, 39));
            return scene;
        }

        private static Point2f[] OrderCorners(Point[] points)
        {
            Point topLeft = points.OrderBy(point => point.X + point.Y).First();
            Point bottomRight = points.OrderByDescending(point => point.X + point.Y).First();
            Point topRight = points.OrderByDescending(point => point.X - point.Y).First();
            Point bottomLeft = points.OrderBy(point => point.X - point.Y).First();
            return new[]
            {
                new Point2f(topLeft.X, topLeft.Y), new Point2f(topRight.X, topRight.Y),
                new Point2f(bottomRight.X, bottomRight.Y), new Point2f(bottomLeft.X, bottomLeft.Y)
            };
        }
    }
}

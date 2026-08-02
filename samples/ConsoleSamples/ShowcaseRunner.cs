using System;
using System.Globalization;
using System.IO;
using OpenCvSharp;
using OpenCvSharp.Core;
using OpenCvSharp.Features2D;
using OpenCvSharp.ImgCodecs;
using OpenCvSharp.ImgProc;
using OpenCvSharp.ML;
using CoreCv2 = OpenCvSharp.Core.Cv2;
using Features2DCv2 = OpenCvSharp.Features2D.Cv2;
using ImgCodecsCv2 = OpenCvSharp.ImgCodecs.Cv2;
using ImgProcCv2 = OpenCvSharp.ImgProc.Cv2;

namespace ConsoleSamples
{
    internal static class ShowcaseRunner
    {
        private const int PanelWidth = 640;
        private const int PanelHeight = 360;

        public static void Run(string[] args)
        {
            string command = args.Length > 0 ? args[0].ToLowerInvariant() : "all";
            if (command == "help" || command == "--help" || command == "-h")
            {
                WriteUsage();
                return;
            }

            if (command != "all" && command != "image" && command != "features" &&
                command != "template" && command != "ml")
            {
                throw new ArgumentException("Unknown showcase command: " + command, nameof(args));
            }

            string outputDirectory = args.Length > 1
                ? Path.GetFullPath(args[1])
                : Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "artifacts", "showcase"));
            Directory.CreateDirectory(outputDirectory);

            using (Mat source = CreateSourceImage())
            {
                if (command == "image")
                {
                    using (Mat image = CreateEdgePanel(source, out int edgePixels))
                    {
                        WritePng(outputDirectory, "image-pipeline.png", image);
                        WritePng(outputDirectory, "source.png", source);
                        WriteSummary(command, outputDirectory, "edgePixels=" + edgePixels);
                    }
                    return;
                }

                if (command == "features")
                {
                    using (Mat image = CreateFeaturePanel(source, out int keypointCount, out int descriptorColumns))
                    {
                        WritePng(outputDirectory, "orb-features.png", image);
                        WriteSummary(command, outputDirectory, "keypoints=" + keypointCount + ", descriptorColumns=" + descriptorColumns);
                    }
                    return;
                }

                if (command == "template")
                {
                    using (Mat image = CreateTemplatePanel(source, out Point location, out double confidence))
                    {
                        WritePng(outputDirectory, "template-match.png", image);
                        WriteSummary(command, outputDirectory, "location=" + location + ", confidence=" + Format(confidence));
                    }
                    return;
                }

                if (command == "ml")
                {
                    using (Mat image = CreateMlPanel(out int queryCount))
                    {
                        WritePng(outputDirectory, "knn-classification.png", image);
                        WriteSummary(command, outputDirectory, "queries=" + queryCount + ", k=3");
                    }
                    return;
                }

                RunAll(source, outputDirectory);
            }
        }

        private static void RunAll(Mat source, string outputDirectory)
        {
            using (Mat edgePanel = CreateEdgePanel(source, out int edgePixels))
            using (Mat featurePanel = CreateFeaturePanel(source, out int keypointCount, out int descriptorColumns))
            using (Mat templatePanel = CreateTemplatePanel(source, out Point location, out double confidence))
            using (Mat mlPanel = CreateMlPanel(out int queryCount))
            using (Mat top = CoreCv2.HConcat(new[] { edgePanel, featurePanel }))
            using (Mat bottom = CoreCv2.HConcat(new[] { templatePanel, mlPanel }))
            using (Mat overview = CoreCv2.VConcat(new[] { top, bottom }))
            {
                WritePng(outputDirectory, "source.png", source);
                WritePng(outputDirectory, "image-pipeline.png", edgePanel);
                WritePng(outputDirectory, "orb-features.png", featurePanel);
                WritePng(outputDirectory, "template-match.png", templatePanel);
                WritePng(outputDirectory, "knn-classification.png", mlPanel);
                WritePng(outputDirectory, "showcase-overview.png", overview);

                string details = "edgePixels=" + edgePixels
                    + ", keypoints=" + keypointCount
                    + ", descriptorColumns=" + descriptorColumns
                    + ", match=" + location
                    + ", confidence=" + Format(confidence)
                    + ", mlQueries=" + queryCount;
                WriteSummary("all", outputDirectory, details);
            }
        }

        private static Mat CreateSourceImage()
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

        private static Mat CreateEdgePanel(Mat source, out int edgePixels)
        {
            using (var gray = new Mat())
            using (var blurred = new Mat())
            using (var edges = new Mat())
            {
                ImgProcCv2.CvtColor(source, gray, ColorConversionCodes.BGR2GRAY);
                ImgProcCv2.GaussianBlur(gray, blurred, new Size(7, 7), 1.4);
                ImgProcCv2.Canny(blurred, edges, 45, 135);
                edgePixels = CoreCv2.CountNonZero(edges);
                Mat panel = CoreCv2.Merge(new[] { edges, edges, edges });
                AddPanelLabel(panel, "01  IMAGE PIPELINE", new Scalar(52, 226, 164));
                AddMetric(panel, "Canny edges  " + edgePixels.ToString(CultureInfo.InvariantCulture));
                return panel;
            }
        }

        private static Mat CreateFeaturePanel(Mat source, out int keypointCount, out int descriptorColumns)
        {
            using (ORB orb = ORB.Create(maxFeatures: 320, fastThreshold: 8))
            using (var descriptors = new Mat())
            {
                orb.DetectAndCompute(source, null, out KeyPoint[] keypoints, descriptors);
                var panel = new Mat();
                Features2DCv2.DrawKeypoints(source, keypoints, panel, new Scalar(48, 238, 255), DrawMatchesFlags.DrawRichKeypoints);
                keypointCount = keypoints.Length;
                descriptorColumns = descriptors.Cols;
                AddPanelLabel(panel, "02  ORB FEATURES", new Scalar(48, 238, 255));
                AddMetric(panel, "Keypoints  " + keypointCount.ToString(CultureInfo.InvariantCulture));
                return panel;
            }
        }

        private static Mat CreateTemplatePanel(Mat source, out Point location, out double confidence)
        {
            var templateRect = new Rect(346, 106, 122, 168);
            using (Mat templateView = source.SubMat(templateRect))
            using (Mat template = templateView.Clone())
            using (Mat response = ImgProcCv2.MatchTemplate(source, template, TemplateMatchModes.CCoeffNormed))
            {
                MinMaxLocResult extrema = CoreCv2.MinMaxLoc(response);
                location = extrema.MaxLoc;
                confidence = extrema.MaxVal;
                Mat panel = source.Clone();
                ImgProcCv2.Rectangle(panel, new Rect(location, template.Size), new Scalar(72, 220, 255), 5, LineTypes.AntiAlias);
                AddPanelLabel(panel, "03  TEMPLATE MATCH", new Scalar(72, 220, 255));
                AddMetric(panel, "Confidence  " + Format(confidence));
                return panel;
            }
        }

        private static Mat CreateMlPanel(out int queryCount)
        {
            const int gridWidth = 80;
            const int gridHeight = 45;
            float[] trainingValues =
            {
                0.12F, 0.18F, 0.20F, 0.34F, 0.34F, 0.13F, 0.42F, 0.39F,
                0.62F, 0.62F, 0.72F, 0.78F, 0.82F, 0.56F, 0.88F, 0.84F
            };
            int[] trainingLabels = { 0, 0, 0, 0, 1, 1, 1, 1 };
            queryCount = gridWidth * gridHeight;

            using (var samples = new Mat(8, 2, MatType.CV_32FC1))
            using (var responses = new Mat(8, 1, MatType.CV_32SC1))
            using (var queries = new Mat(queryCount, 2, MatType.CV_32FC1))
            using (var results = new Mat())
            using (KNearest knn = KNearest.Create())
            {
                samples.CopyFrom<float>(trainingValues);
                responses.CopyFrom<int>(trainingLabels);
                float[] queryValues = new float[queryCount * 2];
                for (int y = 0; y < gridHeight; y++)
                {
                    for (int x = 0; x < gridWidth; x++)
                    {
                        int offset = ((y * gridWidth) + x) * 2;
                        queryValues[offset] = x / (float)(gridWidth - 1);
                        queryValues[offset + 1] = y / (float)(gridHeight - 1);
                    }
                }
                queries.CopyFrom<float>(queryValues);

                knn.DefaultK = 3;
                knn.IsClassifierModel = true;
                knn.AlgorithmType = KNearestTypes.BruteForce;
                if (!knn.Train(samples, SampleTypes.RowSample, responses))
                {
                    throw new InvalidOperationException("KNearest showcase training failed.");
                }
                knn.FindNearest(queries, 3, results);

                float[] labels = results.ToArray<float>();
                byte[] pixels = new byte[queryCount * 3];
                for (int i = 0; i < labels.Length; i++)
                {
                    bool secondClass = labels[i] >= 0.5F;
                    pixels[(i * 3)] = secondClass ? (byte)74 : (byte)198;
                    pixels[(i * 3) + 1] = secondClass ? (byte)156 : (byte)91;
                    pixels[(i * 3) + 2] = secondClass ? (byte)232 : (byte)62;
                }

                using (var grid = new Mat(gridHeight, gridWidth, MatType.CV_8UC3))
                {
                    grid.CopyFrom<byte>(pixels);
                    var panel = new Mat();
                    ImgProcCv2.Resize(grid, panel, new Size(PanelWidth, PanelHeight), interpolation: InterpolationFlags.Nearest);
                    for (int i = 0; i < trainingLabels.Length; i++)
                    {
                        int x = (int)Math.Round(trainingValues[i * 2] * (PanelWidth - 1));
                        int y = (int)Math.Round(trainingValues[(i * 2) + 1] * (PanelHeight - 1));
                        Scalar color = trainingLabels[i] == 0 ? new Scalar(255, 225, 220) : new Scalar(220, 250, 255);
                        ImgProcCv2.Circle(panel, new Point(x, y), 10, color, -1, LineTypes.AntiAlias);
                        ImgProcCv2.Circle(panel, new Point(x, y), 10, new Scalar(32, 36, 42), 2, LineTypes.AntiAlias);
                    }
                    AddPanelLabel(panel, "04  KNN CLASSIFICATION", new Scalar(245, 245, 245));
                    AddMetric(panel, "3-nearest  3,600 queries");
                    return panel;
                }
            }
        }

        private static void AddPanelLabel(Mat panel, string text, Scalar accent)
        {
            ImgProcCv2.Rectangle(panel, new Rect(0, 0, panel.Cols, 62), new Scalar(24, 28, 34), -1);
            ImgProcCv2.Rectangle(panel, new Rect(0, 0, 10, 62), accent, -1);
            ImgProcCv2.PutText(panel, text, new Point(28, 40), HersheyFonts.HersheyDuplex, 0.85,
                new Scalar(248, 250, 252), 2, LineTypes.AntiAlias);
        }

        private static void AddMetric(Mat panel, string text)
        {
            Size textSize = ImgProcCv2.GetTextSize(text, HersheyFonts.HersheySimplex, 0.58, 1, out _);
            int left = Math.Max(16, panel.Cols - textSize.Width - 28);
            ImgProcCv2.Rectangle(panel, new Rect(left - 12, panel.Rows - 50, textSize.Width + 24, 36), new Scalar(24, 28, 34), -1);
            ImgProcCv2.PutText(panel, text, new Point(left, panel.Rows - 25), HersheyFonts.HersheySimplex, 0.58,
                new Scalar(242, 245, 248), 1, LineTypes.AntiAlias);
        }

        private static void WritePng(string outputDirectory, string fileName, Mat image)
        {
            string path = Path.Combine(outputDirectory, fileName);
            if (!ImgCodecsCv2.ImWrite(path, image, new[] { (int)ImwriteFlags.PngCompression, 4 }))
            {
                throw new IOException("OpenCV did not write showcase image: " + path);
            }
        }

        private static void WriteSummary(string command, string outputDirectory, string details)
        {
            Console.WriteLine(OpenCvSharpBuildInfo.GetDisplayString());
            Console.WriteLine("Showcase: " + command);
            Console.WriteLine("Output: " + outputDirectory);
            Console.WriteLine(details);
        }

        private static string Format(double value)
        {
            return value.ToString("0.000", CultureInfo.InvariantCulture);
        }

        private static void WriteUsage()
        {
            Console.WriteLine("Usage: dotnet run --project samples/ConsoleSamples/ConsoleSamples.csproj -- showcase [all|image|features|template|ml] [output-directory]");
        }
    }
}

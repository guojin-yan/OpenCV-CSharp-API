using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using JYPPX.OpenCvSharp;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Dnn;
using JYPPX.OpenCvSharp.ImgCodecs;
using JYPPX.OpenCvSharp.ImgProc;
using JYPPX.OpenCvSharp.Samples.Common;
using CoreCv2 = JYPPX.OpenCvSharp.Core.Cv2;
using DnnCv2 = JYPPX.OpenCvSharp.Dnn.Cv2;
using ImgCodecsCv2 = JYPPX.OpenCvSharp.ImgCodecs.Cv2;
using ImgProcCv2 = JYPPX.OpenCvSharp.ImgProc.Cv2;

namespace JYPPX.OpenCvSharp.Samples.DeepLearning.ObjectDetection
{
    internal static class Program
    {
        private const int InputSize = 416;
        private const int ClassCount = 80;
        private const int RegressionBins = 8;
        private const float ConfidenceThreshold = 0.45F;
        private const float NmsThreshold = 0.60F;
        private static readonly int[] Strides = { 8, 16, 32 };
        private static readonly string[] ClassNames =
        {
            "person", "bicycle", "car", "motorcycle", "airplane", "bus", "train", "truck", "boat", "traffic light",
            "fire hydrant", "stop sign", "parking meter", "bench", "bird", "cat", "dog", "horse", "sheep", "cow",
            "elephant", "bear", "zebra", "giraffe", "backpack", "umbrella", "handbag", "tie", "suitcase", "frisbee",
            "skis", "snowboard", "sports ball", "kite", "baseball bat", "baseball glove", "skateboard", "surfboard", "tennis racket", "bottle",
            "wine glass", "cup", "fork", "knife", "spoon", "bowl", "banana", "apple", "sandwich", "orange",
            "broccoli", "carrot", "hot dog", "pizza", "donut", "cake", "chair", "couch", "potted plant", "bed",
            "dining table", "toilet", "tv", "laptop", "mouse", "remote", "keyboard", "cell phone", "microwave", "oven",
            "toaster", "sink", "refrigerator", "book", "clock", "vase", "scissors", "teddy bear", "hair drier", "toothbrush"
        };

        private static void Main(string[] args)
        {
            string outputDirectory = SampleSupport.GetOutputDirectory(args, "object-detection");
            string? assetRoot = args.Length > 1 ? args[1] : null;
            string modelPath = ModelAssetSupport.ResolveFile("nanodet-model", assetRoot);
            string imagePath = ModelAssetSupport.ResolveFile("opencv-messi5-image", assetRoot);

            using Mat image = ImgCodecsCv2.ImRead(imagePath, ImreadModes.Color);
            if (image.Empty)
            {
                throw new InvalidDataException("OpenCV could not read the verified detection image.");
            }

            using Mat letterboxed = CreateLetterbox(image, out LetterboxInfo letterbox);
            var parameters = new Image2BlobParams(
                new Scalar(1.0 / 57.375, 1.0 / 57.12, 1.0 / 58.395),
                mean: new Scalar(103.53, 116.28, 123.675),
                swapRB: true);
            using Mat blob = DnnCv2.BlobFromImage(letterboxed, parameters);
            using Net net = Net.ReadNetFromOnnx(modelPath, DnnEngine.Classic);
            net.SetPreferableBackend(DnnBackend.OpenCV).SetPreferableTarget(DnnTarget.Cpu).SetInput(blob);

            string[] outputNames = net.GetUnconnectedOutLayersNames();
            var stopwatch = Stopwatch.StartNew();
            Mat[] outputs = net.Forward(outputNames);
            stopwatch.Stop();
            Detection[] detections;
            try
            {
                detections = NonMaximumSuppression(Decode(outputs, outputNames, letterbox, image.Size), NmsThreshold).ToArray();
            }
            finally
            {
                foreach (Mat output in outputs)
                {
                    output.Dispose();
                }
            }

            using Mat visualization = image.Clone();
            foreach (Detection detection in detections)
            {
                ImgProcCv2.Rectangle(visualization, detection.Box, new Scalar(54, 224, 134), 2, LineTypes.AntiAlias);
                string label = $"{ClassNames[detection.ClassId]} {detection.Score:0.00}";
                int labelY = Math.Max(18, detection.Box.Y - 7);
                ImgProcCv2.PutText(visualization, label, new Point(detection.Box.X, labelY),
                    HersheyFonts.HersheyDuplex, 0.52, new Scalar(40, 245, 150), 1, LineTypes.AntiAlias);
            }
            ImgProcCv2.Rectangle(visualization, new Rect(0, 0, visualization.Cols, 34), new Scalar(22, 27, 33), -1);
            ImgProcCv2.PutText(visualization, $"NanoDet | detections {detections.Length} | {stopwatch.Elapsed.TotalMilliseconds:0.0} ms",
                new Point(12, 23), HersheyFonts.HersheySimplex, 0.56, new Scalar(245, 248, 250), 1, LineTypes.AntiAlias);
            SampleSupport.WritePng(outputDirectory, "object-detection.png", visualization);

            string classes = detections.Length == 0
                ? "none"
                : string.Join(",", detections.Select(detection => ClassNames[detection.ClassId]).Distinct());
            SampleSupport.WriteSummary("NanoDet object detection", outputDirectory,
                $"detections={detections.Length}, classes={classes}, inferenceMs={stopwatch.Elapsed.TotalMilliseconds:0.0}");
        }

        private static Mat CreateLetterbox(Mat source, out LetterboxInfo info)
        {
            double scale = Math.Min(InputSize / (double)source.Cols, InputSize / (double)source.Rows);
            int width = Math.Max(1, (int)(source.Cols * scale));
            int height = Math.Max(1, (int)(source.Rows * scale));
            int left = (InputSize - width) / 2;
            int top = (InputSize - height) / 2;
            using Mat resized = new Mat();
            ImgProcCv2.Resize(source, resized, new Size(width, height), interpolation: InterpolationFlags.Area);
            Mat result = CoreCv2.CopyMakeBorder(resized, top, InputSize - height - top, left, InputSize - width - left,
                BorderTypes.Constant, new Scalar(0));
            info = new LetterboxInfo(left, top, width, height, source.Cols, source.Rows);
            return result;
        }

        private static List<Detection> Decode(Mat[] outputs, string[] outputNames, LetterboxInfo letterbox, Size imageSize)
        {
            if (outputs.Length != Strides.Length * 2)
            {
                string shapes = string.Join(", ", outputs.Select((output, index) =>
                    $"{outputNames[index]}:dims={output.Dims},rows={output.Rows},cols={output.Cols},values={output.Total.ToUInt64() * (ulong)output.Channels}"));
                throw new InvalidDataException("Unexpected NanoDet output set: " + shapes + ".");
            }

            var detections = new List<Detection>();
            for (int level = 0; level < Strides.Length; level++)
            {
                int stride = Strides[level];
                int side = InputSize / stride;
                int locationCount = side * side;
                float[] classScores = outputs[level * 2].ToArray<float>();
                float[] boxScores = outputs[(level * 2) + 1].ToArray<float>();
                if (classScores.Length != locationCount * ClassCount || boxScores.Length != locationCount * 4 * RegressionBins)
                {
                    throw new InvalidDataException($"Unexpected NanoDet output size at stride {stride}: {classScores.Length}/{boxScores.Length}.");
                }

                for (int location = 0; location < locationCount; location++)
                {
                    int classOffset = location * ClassCount;
                    int classId = 0;
                    float score = classScores[classOffset];
                    for (int classIndex = 1; classIndex < ClassCount; classIndex++)
                    {
                        float candidate = classScores[classOffset + classIndex];
                        if (candidate > score)
                        {
                            score = candidate;
                            classId = classIndex;
                        }
                    }
                    if (score < ConfidenceThreshold)
                    {
                        continue;
                    }

                    int gridX = location % side;
                    int gridY = location / side;
                    float centerX = (gridX * stride) + (0.5F * (stride - 1));
                    float centerY = (gridY * stride) + (0.5F * (stride - 1));
                    int boxOffset = location * 4 * RegressionBins;
                    float left = DecodeDistance(boxScores, boxOffset, stride);
                    float top = DecodeDistance(boxScores, boxOffset + RegressionBins, stride);
                    float right = DecodeDistance(boxScores, boxOffset + (2 * RegressionBins), stride);
                    float bottom = DecodeDistance(boxScores, boxOffset + (3 * RegressionBins), stride);
                    Rect box = letterbox.ToImageRect(centerX - left, centerY - top, centerX + right, centerY + bottom, imageSize);
                    if (box.Width > 1 && box.Height > 1)
                    {
                        detections.Add(new Detection(box, score, classId));
                    }
                }
            }
            return detections;
        }

        private static float DecodeDistance(float[] values, int offset, int stride)
        {
            float maximum = values[offset];
            for (int i = 1; i < RegressionBins; i++)
            {
                maximum = Math.Max(maximum, values[offset + i]);
            }
            double denominator = 0.0;
            double weighted = 0.0;
            for (int i = 0; i < RegressionBins; i++)
            {
                double probability = Math.Exp(values[offset + i] - maximum);
                denominator += probability;
                weighted += probability * i;
            }
            return (float)(weighted / denominator * stride);
        }

        private static IEnumerable<Detection> NonMaximumSuppression(IEnumerable<Detection> source, float threshold)
        {
            var kept = new List<Detection>();
            foreach (Detection candidate in source.OrderByDescending(detection => detection.Score))
            {
                if (kept.All(existing => IntersectionOverUnion(candidate.Box, existing.Box) <= threshold))
                {
                    kept.Add(candidate);
                }
            }
            return kept;
        }

        private static double IntersectionOverUnion(Rect left, Rect right)
        {
            int x1 = Math.Max(left.X, right.X);
            int y1 = Math.Max(left.Y, right.Y);
            int x2 = Math.Min(left.X + left.Width, right.X + right.Width);
            int y2 = Math.Min(left.Y + left.Height, right.Y + right.Height);
            double intersection = Math.Max(0, x2 - x1) * Math.Max(0, y2 - y1);
            double union = (left.Width * left.Height) + (right.Width * right.Height) - intersection;
            return union <= 0.0 ? 0.0 : intersection / union;
        }

        private sealed class Detection
        {
            public Detection(Rect box, float score, int classId)
            {
                Box = box;
                Score = score;
                ClassId = classId;
            }
            public Rect Box { get; }
            public float Score { get; }
            public int ClassId { get; }
        }

        private readonly struct LetterboxInfo
        {
            public LetterboxInfo(int left, int top, int width, int height, int sourceWidth, int sourceHeight)
            {
                Left = left;
                Top = top;
                Width = width;
                Height = height;
                SourceWidth = sourceWidth;
                SourceHeight = sourceHeight;
            }
            private int Left { get; }
            private int Top { get; }
            private int Width { get; }
            private int Height { get; }
            private int SourceWidth { get; }
            private int SourceHeight { get; }

            public Rect ToImageRect(float x1, float y1, float x2, float y2, Size imageSize)
            {
                int left = Math.Clamp((int)Math.Round((x1 - Left) * SourceWidth / Width), 0, imageSize.Width - 1);
                int top = Math.Clamp((int)Math.Round((y1 - Top) * SourceHeight / Height), 0, imageSize.Height - 1);
                int right = Math.Clamp((int)Math.Round((x2 - Left) * SourceWidth / Width), left + 1, imageSize.Width);
                int bottom = Math.Clamp((int)Math.Round((y2 - Top) * SourceHeight / Height), top + 1, imageSize.Height);
                return new Rect(left, top, right - left, bottom - top);
            }
        }
    }
}

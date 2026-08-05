using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using JYPPX.OpenCvSharp;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Dnn;
using JYPPX.OpenCvSharp.ImgCodecs;
using JYPPX.OpenCvSharp.ImgProc;
using JYPPX.OpenCvSharp.Samples.Common;
using DnnCv2 = JYPPX.OpenCvSharp.Dnn.Cv2;
using ImgCodecsCv2 = JYPPX.OpenCvSharp.ImgCodecs.Cv2;
using ImgProcCv2 = JYPPX.OpenCvSharp.ImgProc.Cv2;

namespace JYPPX.OpenCvSharp.Samples.DeepLearning.ImageClassification
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            string outputDirectory = SampleSupport.GetOutputDirectory(args, "image-classification");
            string? assetRoot = args.Length > 1 ? args[1] : null;
            string modelPath = ModelAssetSupport.ResolveFile("mobilenet-v2-model", assetRoot);
            string labelsPath = ModelAssetSupport.ResolveFile("imagenet-1k-labels", assetRoot);
            string imagePath = ModelAssetSupport.ResolveFile("opencv-messi5-image", assetRoot);
            string[] labels = LoadLabels(labelsPath);

            using Mat image = ImgCodecsCv2.ImRead(imagePath, ImreadModes.Color);
            if (image.Empty)
            {
                throw new InvalidDataException("OpenCV could not read the verified classification image.");
            }

            using Mat resized = new Mat();
            ImgProcCv2.Resize(image, resized, new Size(256, 256), interpolation: InterpolationFlags.Area);
            using Mat crop = resized.SubMat(new Rect(16, 16, 224, 224));

            var parameters = new Image2BlobParams(
                new Scalar(1.0 / (255.0 * 0.229), 1.0 / (255.0 * 0.224), 1.0 / (255.0 * 0.225)),
                mean: new Scalar(255.0 * 0.485, 255.0 * 0.456, 255.0 * 0.406),
                swapRB: true);
            using Mat blob = DnnCv2.BlobFromImage(crop, parameters);
            using Net net = Net.ReadNetFromOnnx(modelPath, DnnEngine.Classic);
            net.SetPreferableBackend(DnnBackend.OpenCV).SetPreferableTarget(DnnTarget.Cpu).SetInput(blob);

            var stopwatch = Stopwatch.StartNew();
            using Mat output = net.Forward();
            stopwatch.Stop();
            float[] scores = output.ToArray<float>();
            if (scores.Length != labels.Length)
            {
                throw new InvalidDataException($"MobileNet output/label count mismatch: {scores.Length}/{labels.Length}.");
            }

            Prediction[] predictions = GetTopPredictions(scores, labels, 5);
            using Mat visualization = image.Clone();
            ImgProcCv2.Rectangle(visualization, new Rect(0, 0, visualization.Cols, 148), new Scalar(22, 27, 33), -1);
            ImgProcCv2.PutText(visualization, "MobileNetV2 | ImageNet-1K | OpenCV DNN CPU", new Point(16, 28),
                HersheyFonts.HersheyDuplex, 0.58, new Scalar(92, 210, 255), 1, LineTypes.AntiAlias);
            for (int i = 0; i < predictions.Length; i++)
            {
                Prediction prediction = predictions[i];
                string label = prediction.Label.Length > 48 ? prediction.Label.Substring(0, 48) : prediction.Label;
                string line = $"{i + 1}. {label}  {prediction.Probability:P1}";
                ImgProcCv2.PutText(visualization, line, new Point(18, 52 + (i * 19)),
                    HersheyFonts.HersheySimplex, 0.46, new Scalar(242, 246, 249), 1, LineTypes.AntiAlias);
            }
            SampleSupport.WritePng(outputDirectory, "image-classification.png", visualization);
            SampleSupport.WriteSummary("MobileNetV2 image classification", outputDirectory,
                $"top1={predictions[0].Label}, confidence={SampleSupport.Format(predictions[0].Probability)}, inferenceMs={stopwatch.Elapsed.TotalMilliseconds:0.0}");
        }

        private static string[] LoadLabels(string path)
        {
            var labels = new List<string>(1000);
            foreach (string line in File.ReadLines(path))
            {
                string candidate = line.Trim();
                if (!candidate.StartsWith("\"", StringComparison.Ordinal))
                {
                    continue;
                }
                candidate = candidate.TrimEnd(',');
                string? label = JsonSerializer.Deserialize<string>(candidate);
                if (!string.IsNullOrWhiteSpace(label))
                {
                    labels.Add(label);
                }
            }
            if (labels.Count != 1000)
            {
                throw new InvalidDataException("Expected exactly 1,000 ImageNet labels, found " + labels.Count + ".");
            }
            return labels.ToArray();
        }

        private static Prediction[] GetTopPredictions(float[] scores, string[] labels, int count)
        {
            double maximum = scores.Max();
            double denominator = scores.Sum(score => Math.Exp(score - maximum));
            return Enumerable.Range(0, scores.Length)
                .OrderByDescending(index => scores[index])
                .Take(count)
                .Select(index => new Prediction(labels[index], Math.Exp(scores[index] - maximum) / denominator))
                .ToArray();
        }

        private sealed class Prediction
        {
            public Prediction(string label, double probability)
            {
                Label = label;
                Probability = probability;
            }

            public string Label { get; }
            public double Probability { get; }
        }
    }
}

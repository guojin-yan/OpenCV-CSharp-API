using System;
using System.Diagnostics;
using System.IO;
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

namespace JYPPX.OpenCvSharp.Samples.DeepLearning.HumanSegmentation
{
    internal static class Program
    {
        private const int InputSize = 192;

        private static void Main(string[] args)
        {
            string outputDirectory = SampleSupport.GetOutputDirectory(args, "human-segmentation");
            string? assetRoot = args.Length > 1 ? args[1] : null;
            string modelPath = ModelAssetSupport.ResolveFile("pphumanseg-model", assetRoot);
            string imagePath = ModelAssetSupport.ResolveFile("opencv-messi5-image", assetRoot);

            using Mat image = ImgCodecsCv2.ImRead(imagePath, ImreadModes.Color);
            if (image.Empty)
            {
                throw new InvalidDataException("OpenCV could not read the verified segmentation image.");
            }

            using Mat resized = new Mat();
            ImgProcCv2.Resize(image, resized, new Size(InputSize, InputSize), interpolation: InterpolationFlags.Area);
            var parameters = new Image2BlobParams(new Scalar(2.0 / 255.0), mean: new Scalar(127.5), swapRB: true);
            using Mat blob = DnnCv2.BlobFromImage(resized, parameters);
            using Net net = Net.ReadNetFromOnnx(modelPath, DnnEngine.Classic);
            net.SetPreferableBackend(DnnBackend.OpenCV).SetPreferableTarget(DnnTarget.Cpu).SetInput(blob);

            var stopwatch = Stopwatch.StartNew();
            using Mat output = net.Forward();
            stopwatch.Stop();
            float[] scores = output.ToArray<float>();
            int planeSize = InputSize * InputSize;
            if (scores.Length != planeSize * 2)
            {
                throw new InvalidDataException("Unexpected PPHumanSeg output size: " + scores.Length + ".");
            }

            var maskBytes = new byte[planeSize];
            for (int i = 0; i < planeSize; i++)
            {
                maskBytes[i] = scores[planeSize + i] > scores[i] ? byte.MaxValue : byte.MinValue;
            }

            using Mat maskSmall = new Mat(InputSize, InputSize, MatType.CV_8UC1);
            maskSmall.CopyFrom<byte>(maskBytes);
            using Mat mask = new Mat();
            ImgProcCv2.Resize(maskSmall, mask, image.Size, interpolation: InterpolationFlags.Nearest);
            int foregroundPixels = CoreCv2.CountNonZero(mask);

            using Mat color = new Mat(image.Rows, image.Cols, MatType.CV_8UC3, new Scalar(48, 198, 244));
            using Mat selected = image.Clone();
            CoreCv2.CopyTo(color, selected, mask);
            using Mat visualization = CoreCv2.AddWeighted(image, 0.58, selected, 0.42, 0.0);
            ImgProcCv2.Rectangle(visualization, new Rect(0, 0, visualization.Cols, 36), new Scalar(22, 27, 33), -1);
            double ratio = foregroundPixels / (double)(image.Rows * image.Cols);
            ImgProcCv2.PutText(visualization, $"PPHumanSeg | foreground {ratio:P1} | {stopwatch.Elapsed.TotalMilliseconds:0.0} ms",
                new Point(12, 24), HersheyFonts.HersheySimplex, 0.56, new Scalar(245, 248, 250), 1, LineTypes.AntiAlias);
            SampleSupport.WritePng(outputDirectory, "human-segmentation.png", visualization);
            SampleSupport.WritePng(outputDirectory, "human-mask.png", mask);
            SampleSupport.WriteSummary("PPHumanSeg portrait segmentation", outputDirectory,
                $"foregroundPixels={foregroundPixels}, ratio={SampleSupport.Format(ratio)}, inferenceMs={stopwatch.Elapsed.TotalMilliseconds:0.0}");
        }
    }
}

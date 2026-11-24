using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using JYPPX.OpenCvSharp;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Dnn;
using JYPPX.OpenCvSharp.VideoIO;
using Codec = JYPPX.OpenCvSharp.ImgCodecs.Cv2;

internal static class Program
{
    private const string ModelSha256 = "326793cdb2fc2da739a715c3f3ff71d09779b389ad29e56bbfccc4313e900744";

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static int Main(string[] args)
    {
        try
        {
            Require(args.Length == 2 && (args[0] == "full" || args[0] == "mini"), "Expected profile and output directory.");
            Require(OperatingSystem.IsLinux() && RuntimeInformation.ProcessArchitecture == Architecture.X64, "Expected native Linux x64.");
            string profile = args[0];
            string output = Path.GetFullPath(args[1]);
            Directory.CreateDirectory(output);
            OpenCvSharpBuildInfo.VerifyNativeRuntimeCompatibility();
            Require(OpenCvSharpBuildInfo.OpenCvVersion == "5.1.0" && OpenCvSharpBuildInfo.NuGetPackageVersion == "5.1.0", "Wrong managed package version.");
            Require(OpenCvSharpBuildInfo.GetLoadedNativeAbiVersion() == 1, "Wrong native ABI.");

            object image = CheckImage();
            object video = CheckVideo(output);
            object dnn = CheckDnn(profile);
            var nativeFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.so*")
                .OrderBy(Path.GetFileName, StringComparer.Ordinal)
                .Select(path => new { name = Path.GetFileName(path), bytes = new FileInfo(path).Length, sha256 = Hash(File.ReadAllBytes(path)) }).ToArray();
            Require(nativeFiles.Length > 0, "No package native payload was copied.");
            string[] loadedOpenCvPaths = File.ReadLines("/proc/self/maps")
                .Where(line => line.Contains("libopencv_", StringComparison.Ordinal) || line.Contains("JYPPX.OpenCV.Native", StringComparison.Ordinal))
                .Select(line => line.Substring(line.IndexOf('/'))).Distinct(StringComparer.Ordinal).OrderBy(path => path, StringComparer.Ordinal).ToArray();
            Require(loadedOpenCvPaths.Length > 0 && loadedOpenCvPaths.All(path => Path.GetDirectoryName(path) == AppContext.BaseDirectory.TrimEnd('/')), "OpenCV loaded outside the consumer output.");
            var result = new
            {
                status = "passed", profile, openCvVersion = OpenCvSharpBuildInfo.OpenCvVersion,
                packageVersion = OpenCvSharpBuildInfo.NuGetPackageVersion, nativeAbi = OpenCvSharpBuildInfo.GetLoadedNativeAbiVersion(),
                targetFramework = OpenCvSharpBuildInfo.TargetFramework, runtimeVersion = Environment.Version.ToString(),
                frameworkDescription = RuntimeInformation.FrameworkDescription, architecture = RuntimeInformation.ProcessArchitecture.ToString(),
                managedAssemblySha256 = Hash(File.ReadAllBytes(typeof(Mat).Assembly.Location)),
                image, video, dnn, nativeFiles, loadedOpenCvPaths
            };
            string json = JsonSerializer.Serialize(result);
            File.WriteAllText(Path.Combine(output, "result.json"), json + "\n");
            Console.WriteLine(json);
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            return 1;
        }
    }

    private static object CheckImage()
    {
        using var image = new Mat(24, 32, MatType.CV_8UC3, new Scalar(20, 60, 120));
        byte[] png = Codec.ImEncode(".png", image);
        using Mat decoded = Codec.ImDecode(png, JYPPX.OpenCvSharp.ImgCodecs.ImreadModes.Color);
        Require(decoded.Rows == 24 && decoded.Cols == 32 && decoded.Channels == 3 && decoded.AsReadOnlySpan<byte>().SequenceEqual(image.AsReadOnlySpan<byte>()), "PNG roundtrip changed pixels.");
        return new { pngBytes = png.Length, pngSha256 = Hash(png), exactPixelRoundtrip = true };
    }

    private static object CheckVideo(string directory)
    {
        string path = Path.Combine(directory, "three-frames.avi");
        string writerBackend;
        using (var writer = new VideoWriter())
        using (var frame = new Mat(24, 32, MatType.CV_8UC3))
        {
            Require(writer.Open(path, VideoCaptureAPIs.OpenCVMjpeg, VideoWriter.FourCC("MJPG"), 10.0, frame.Size, true), "MJPG writer failed.");
            writerBackend = writer.GetBackendName();
            Require(writerBackend == "CV_MJPEG", "Unexpected writer backend.");
            for (int index = 0; index < 3; index++)
            {
                frame.SetTo(new Scalar(20 + 10 * index, 60 + 10 * index, 120 + 10 * index));
                Require(writer.Write(frame), "Writer rejected a frame.");
            }
            writer.Release();
        }
        byte[] avi = File.ReadAllBytes(path);
        Require(avi.Length > 12 && System.Text.Encoding.ASCII.GetString(avi, 0, 4) == "RIFF" && System.Text.Encoding.ASCII.GetString(avi, 8, 4) == "AVI ", "Writer did not produce an AVI container.");
        string readerBackend;
        var frames = new List<object>();
        using (var capture = new VideoCapture())
        using (var frame = new Mat())
        {
            Require(capture.Open(path, VideoCaptureAPIs.OpenCVMjpeg), "MJPG reader failed.");
            readerBackend = capture.GetBackendName();
            Require(readerBackend == "CV_MJPEG", "Unexpected reader backend.");
            while (capture.Read(frame))
            {
                int index = frames.Count;
                Require(index < 3 && frame.Rows == 24 && frame.Cols == 32 && frame.Channels == 3, "Wrong video frame count or shape.");
                ReadOnlySpan<byte> pixels = frame.AsReadOnlySpan<byte>();
                int maxError = 0;
                long checksum = 0;
                int[] colors = { 20 + 10 * index, 60 + 10 * index, 120 + 10 * index };
                for (int offset = 0; offset < pixels.Length; offset++)
                {
                    maxError = Math.Max(maxError, Math.Abs(pixels[offset] - colors[offset % 3]));
                    checksum += pixels[offset];
                }
                Require(maxError <= 5, "Decoded frame pixels exceed the MJPG tolerance.");
                frames.Add(new { index, rows = frame.Rows, columns = frame.Cols, channels = frame.Channels, checksum, maxAbsoluteError = maxError });
            }
            Require(frames.Count == 3 && !capture.Read(frame), "Video EOF or frame count mismatch.");
        }
        return new { codec = "MJPG", container = "AVI", writerBackend, readerBackend, fileBytes = avi.Length, fileSha256 = Hash(avi), frameCount = frames.Count, endOfFileVerified = true, pixelTolerance = 5, frames };
    }

    private static object CheckDnn(string profile)
    {
        byte[] model = Convert.FromBase64String(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "identity-opset13.onnx.base64")).Trim());
        Require(model.Length == 147 && Hash(model) == ModelSha256, "ONNX model bytes drifted.");
        if (profile == "mini")
        {
            try
            {
                using var unexpected = Net.ReadNetFromOnnx(model, DnnEngine.Classic);
                throw new InvalidOperationException("Mini unexpectedly accepted an ONNX network.");
            }
            catch (EntryPointNotFoundException exception) when (exception.Message.Contains("jyppx_ocv_dnn_read_net_from_onnx_buffer", StringComparison.Ordinal))
            {
                return new { status = "omitted", modelBytes = model.Length, modelSha256 = ModelSha256, inferenceExecuted = false,
                    reasonCode = "entrypoint-absent", entrypoint = "jyppx_ocv_dnn_read_net_from_onnx_buffer", reason = exception.Message };
            }
        }
        using var net = Net.ReadNetFromOnnx(model, DnnEngine.Classic);
        net.SetPreferableBackend(DnnBackend.OpenCV).SetPreferableTarget(DnnTarget.Cpu);
        string[] names = net.GetUnconnectedOutLayersNames();
        Require(names.Length == 1 && names[0] == "output", "Unexpected ONNX named output.");
        var inferences = new List<object>();
        foreach (float[] values in new[] { new[] { 1F, 2F, 3F, 4F }, new[] { -4F, -3F, -2F, -1F } })
        {
            using var image = new Mat(2, 2, MatType.CV_32FC1);
            image.CopyFrom<float>(values);
            using Mat blob = JYPPX.OpenCvSharp.Dnn.Cv2.BlobFromImage(image, new Image2BlobParams());
            net.SetInput(blob, "input");
            using Mat result = net.Forward("output");
            float[] actual = result.ToArray<float>();
            Require(actual.SequenceEqual(values), "ONNX inference changed the identity output.");
            inferences.Add(new { input = values, output = actual, maxAbsoluteError = 0 });
        }
        return new { status = "passed", modelBytes = model.Length, modelSha256 = ModelSha256, inferenceExecuted = true, engine = "Classic", backend = "OpenCV", target = "Cpu", inputName = "input", outputName = "output", inferences };
    }
}

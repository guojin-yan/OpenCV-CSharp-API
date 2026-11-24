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
using Gui = JYPPX.OpenCvSharp.HighGui.Cv2;
using JYPPX.OpenCvSharp.HighGui;

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

            object highgui = CheckHighGui(profile);
            object image = CheckImage();
            object missingVideo = CheckMissingVideo(output);
            object video = CheckVideo(output);
            object dnn = CheckDnn(profile);
            var nativeFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.so*")
                .OrderBy(Path.GetFileName, StringComparer.Ordinal)
                .Select(path => new { name = Path.GetFileName(path), bytes = new FileInfo(path).Length, sha256 = Hash(File.ReadAllBytes(path)) }).ToArray();
            Require(nativeFiles.Length == (profile == "full" ? 49 : 19), "Wrong candidate native payload count.");
            Require(!nativeFiles.Any(f => f.name.Contains("highgui", StringComparison.Ordinal)), "HighGui payload present.");
            string[] loadedOpenCvPaths = File.ReadLines("/proc/self/maps")
                .Where(line => line.Contains("libopencv_", StringComparison.Ordinal) || line.Contains("JYPPX.OpenCV.Native", StringComparison.Ordinal))
                .Select(line => line.Substring(line.IndexOf('/'))).Distinct(StringComparer.Ordinal).OrderBy(path => path, StringComparer.Ordinal).ToArray();
            Require(loadedOpenCvPaths.Length == (profile == "full" ? 17 : 7) && loadedOpenCvPaths.All(path => Path.GetDirectoryName(path) == AppContext.BaseDirectory.TrimEnd('/')), "OpenCV loaded outside the consumer output.");
            Require(!File.ReadLines("/proc/self/maps").Any(line => new[] { "libgtk", "libgdk", "libQt", "libX11", "libXext", "libwayland", "libopencv_highgui" }.Any(name => line.Contains(name, StringComparison.Ordinal))), "GUI library loaded.");
            var result = new
            {
                status = "passed", profile, openCvVersion = OpenCvSharpBuildInfo.OpenCvVersion,
                packageVersion = OpenCvSharpBuildInfo.NuGetPackageVersion, nativeAbi = OpenCvSharpBuildInfo.GetLoadedNativeAbiVersion(),
                targetFramework = OpenCvSharpBuildInfo.TargetFramework, runtimeVersion = Environment.Version.ToString(),
                frameworkDescription = RuntimeInformation.FrameworkDescription, architecture = RuntimeInformation.ProcessArchitecture.ToString(),
                managedAssemblySha256 = Hash(File.ReadAllBytes(typeof(Mat).Assembly.Location)),
                highgui, image, missingVideo, video, dnn, nativeFiles, loadedOpenCvPaths,
                guiLibrariesAbsent = true, displayUnset = true, nativeStateUsableAfterEachGuiFailure = true,
                backends = VideoIORegistry.GetBackends().Select(backend => new { id = (int)backend, name = VideoIORegistry.GetBackendName(backend), builtIn = VideoIORegistry.IsBackendBuiltIn(backend), available = VideoIORegistry.HasBackend(backend) }).ToArray()
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

    private static object CheckHighGui(string profile)
    {
        foreach (string variable in new[] { "DISPLAY", "WAYLAND_DISPLAY", "LD_LIBRARY_PATH", "LD_PRELOAD", "LD_AUDIT", "OPENCV_CSHARP_OPENCV_RUNTIME_ROOT" })
            Require(Environment.GetEnvironmentVariable(variable) == null, "Forbidden environment variable: " + variable);
        using var image = new Mat(2, 2, MatType.CV_8UC1, new Scalar(11));
        var checks = new List<object>();
        void Reject(string name, Action action)
        {
            Exception? failure = null;
            try { action(); } catch (Exception exception) { failure = exception; }
            Require(failure != null, "GUI call unexpectedly succeeded: " + name);
            string expected = profile == "full" ? "NOT_LINKED" : "entrypoint-absent";
            Require(profile == "full" ? failure is OpenCvException && failure.Message.Contains("NOT_LINKED", StringComparison.OrdinalIgnoreCase)
                : failure is EntryPointNotFoundException, "Unexpected GUI failure: " + failure);
            image.SetTo(new Scalar(11));
            Require(image.AsReadOnlySpan<byte>().ToArray().All(value => value == 11), "Native state broken after GUI failure.");
            checks.Add(new { name, status = expected, exceptionType = failure!.GetType().FullName, nativeRecovery = true });
        }
        Reject("NamedWindow", () => Gui.NamedWindow("headless", WindowFlags.AutoSize));
        Reject("DestroyWindow", () => Gui.DestroyWindow("headless"));
        Reject("ImShow", () => Gui.ImShow("headless", image));
        Reject("CreateTrackbar", () => Gui.CreateTrackbar("position", "headless", 0, 10));
        Reject("GetCurrentUIFramework", () => Gui.GetCurrentUIFramework());
        Reject("WaitKey", () => Gui.WaitKey(1));
        return new { status = "passed", checks };
    }

    private static object CheckMissingVideo(string output)
    {
        string path = Path.Combine(output, "missing-input.avi");
        Require(!File.Exists(path), "Missing-input fixture exists.");
        void Attempt()
        {
            using var capture = new VideoCapture();
            Require(!capture.Open(path, VideoCaptureAPIs.OpenCVMjpeg) && !capture.IsOpened, "Missing file unexpectedly opened.");
            capture.Release();
        }
        Attempt(); // Warm the tested backend before measuring its second open/dispose.
        int tasksBefore = Directory.GetDirectories("/proc/self/task").Length;
        int socketsBefore = Directory.GetFiles("/proc/self/fd").Count(file => new FileInfo(file).LinkTarget?.StartsWith("socket:", StringComparison.Ordinal) == true);
        Attempt();
        int tasksAfter = Directory.GetDirectories("/proc/self/task").Length;
        int socketsAfter = Directory.GetFiles("/proc/self/fd").Count(file => new FileInfo(file).LinkTarget?.StartsWith("socket:", StringComparison.Ordinal) == true);
        Require(tasksBefore == tasksAfter && socketsBefore == socketsAfter, "Missing-file VideoIO left workers or sockets.");
        Require(!Directory.GetFiles("/proc/self/fd").Any(file => new FileInfo(file).LinkTarget == path), "Missing-file handle leaked.");
        return new { status = "returned-false", backend = "CV_MJPEG", attempts = 2, tasksBefore, tasksAfter, socketsBefore, socketsAfter, fileHandleLeaked = false };
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

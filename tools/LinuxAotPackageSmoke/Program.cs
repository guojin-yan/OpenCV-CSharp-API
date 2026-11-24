using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using JYPPX.OpenCvSharp;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Dnn;
using JYPPX.OpenCvSharp.VideoIO;
using Codec = JYPPX.OpenCvSharp.ImgCodecs.Cv2;

internal static class Program
{
    private const string ModelSha256 = "326793cdb2fc2da739a715c3f3ff71d09779b389ad29e56bbfccc4313e900744";
    private const string ModelBase64 = "CAgSEU9wZW5DVi1DU2hhcnAtQVBJOngKIwoFaW5wdXQSBm91dHB1dBoIaWRlbnRpdHkiCElkZW50aXR5Eg5pZGVudGl0eV9ncmFwaFofCgVpbnB1dBIWChQIARIQCgIIAQoCCAEKAggCCgIIAmIgCgZvdXRwdXQSFgoUCAESEAoCCAEKAggBCgIIAgoCCAJCAhAN";

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static int Main(string[] args)
    {
        try
        {
            Require(args.Length == 2 && (args[0] == "full" || args[0] == "mini"), "Expected profile and output directory.");
            Require(OperatingSystem.IsLinux() && RuntimeInformation.ProcessArchitecture == Architecture.X64, "Expected native Linux x64.");
            Require(!RuntimeFeature.IsDynamicCodeSupported && !RuntimeFeature.IsDynamicCodeCompiled, "Expected NativeAOT without JIT.");
            OpenCvSharpBuildInfo.VerifyNativeRuntimeCompatibility();
            Require(OpenCvSharpBuildInfo.OpenCvVersion == "5.1.0" && OpenCvSharpBuildInfo.NuGetPackageVersion == "5.1.0" &&
                OpenCvSharpBuildInfo.GetLoadedNativeAbiVersion() == 1, "Wrong managed/native package version.");
            Require(Environment.Version.ToString() == "10.0.15", "Wrong compiled runtime version.");
            Console.WriteLine("AOT_LINUX_SMOKE_START");
            Console.WriteLine("profile=" + args[0]);
            Console.WriteLine("openCvVersion=" + OpenCvSharpBuildInfo.OpenCvVersion);
            Console.WriteLine("managedPackageVersion=" + OpenCvSharpBuildInfo.NuGetPackageVersion);
            Console.WriteLine("nativeAbi=" + OpenCvSharpBuildInfo.GetLoadedNativeAbiVersion());
            Console.WriteLine("runtimeVersion=" + Environment.Version);
            Console.WriteLine("dynamicCodeSupported=false");
            Console.WriteLine("dynamicCodeCompiled=false");
            using (var source = new Mat(2, 2, MatType.CV_8UC1, new Scalar(7)))
            using (var blurred = new Mat())
            {
                JYPPX.OpenCvSharp.ImgProc.Cv2.Blur(source, blurred, new Size(1, 1));
                byte[] encoded = Codec.ImEncode(".png", blurred);
                using Mat decoded = Codec.ImDecode(encoded, JYPPX.OpenCvSharp.ImgCodecs.ImreadModes.Grayscale);
                Require(encoded.Length == 71 && decoded.Rows == 2 && decoded.Cols == 2 &&
                    decoded.AsReadOnlySpan<byte>().SequenceEqual(source.AsReadOnlySpan<byte>()), "PNG pixels changed.");
                Console.WriteLine("pngBytes=" + encoded.Length);
                Console.WriteLine("pngSha256=" + Convert.ToHexString(SHA256.HashData(encoded)).ToLowerInvariant());
                Console.WriteLine("pngPixelsExact=true");
            }
            string outputDirectory = Path.GetFullPath(args[1]);
            Directory.CreateDirectory(outputDirectory);
            CheckVideo(Path.Combine(outputDirectory, "three-frames.avi"));
            CheckDnn(args[0]);
            string[] paths = File.ReadLines("/proc/self/maps")
                .Where(line => line.Contains("libopencv_", StringComparison.Ordinal) || line.Contains("JYPPX.OpenCV.Native", StringComparison.Ordinal))
                .Select(line => line.Substring(line.IndexOf('/'))).Distinct(StringComparer.Ordinal).OrderBy(path => path, StringComparer.Ordinal).ToArray();
            Require(paths.Length == (args[0] == "full" ? 18 : 7), "Wrong loaded native module closure.");
            foreach (string path in paths)
            {
                Require(Path.GetDirectoryName(path) == AppContext.BaseDirectory.TrimEnd('/'), "Native library loaded outside the application directory.");
                Console.WriteLine("nativeLoaded=" + path);
            }
            Require(!File.ReadLines("/proc/self/maps").Any(line => line.Contains("libcoreclr", StringComparison.Ordinal) ||
                line.Contains("libhostfxr", StringComparison.Ordinal) || line.Contains("libclrjit", StringComparison.Ordinal)), "Unexpected external CLR/JIT.");
            Console.WriteLine("externalClrLoaded=false");
            Console.WriteLine("AOT_LINUX_SMOKE_OK");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            return 1;
        }
    }

    private static void CheckVideo(string path)
    {
        using (var writer = new VideoWriter())
        using (var frame = new Mat(24, 32, MatType.CV_8UC3))
        {
            Require(writer.Open(path, VideoCaptureAPIs.OpenCVMjpeg, VideoWriter.FourCC("MJPG"), 10, frame.Size, true) &&
                writer.GetBackendName() == "CV_MJPEG", "MJPG writer failed.");
            for (int index = 0; index < 3; index++)
            {
                frame.SetTo(new Scalar(20 + 10 * index, 60 + 10 * index, 120 + 10 * index));
                Require(writer.Write(frame), "MJPG frame write failed.");
            }
            writer.Release();
        }
        int count = 0;
        long checksum = 0;
        int maxError = 0;
        using (var capture = new VideoCapture())
        using (var frame = new Mat())
        {
            Require(capture.Open(path, VideoCaptureAPIs.OpenCVMjpeg) && capture.GetBackendName() == "CV_MJPEG", "MJPG reader failed.");
            while (capture.Read(frame))
            {
                Require(count < 3 && frame.Rows == 24 && frame.Cols == 32 && frame.Channels == 3, "Wrong frame shape/count.");
                ReadOnlySpan<byte> pixels = frame.AsReadOnlySpan<byte>();
                int[] colors = { 20 + 10 * count, 60 + 10 * count, 120 + 10 * count };
                for (int index = 0; index < pixels.Length; index++)
                {
                    checksum += pixels[index];
                    maxError = Math.Max(maxError, Math.Abs(pixels[index] - colors[index % 3]));
                }
                count++;
            }
            Require(count == 3 && !capture.Read(frame) && maxError <= 5, "Wrong MJPG pixels/EOF.");
        }
        Console.WriteLine("videoBackend=CV_MJPEG");
        Console.WriteLine("videoFrames=" + count);
        Console.WriteLine("videoChecksum=" + checksum);
        Console.WriteLine("videoMaxError=" + maxError);
        Console.WriteLine("videoEof=true");
        Console.WriteLine("videoSha256=" + Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant());
    }

    private static void CheckDnn(string profile)
    {
        byte[] model = Convert.FromBase64String(ModelBase64);
        Require(model.Length == 147 && Convert.ToHexString(SHA256.HashData(model)).ToLowerInvariant() == ModelSha256, "Wrong ONNX fixture.");
        Console.WriteLine("modelSha256=" + ModelSha256);
        if (profile == "mini")
        {
            try
            {
                using var unexpected = Net.ReadNetFromOnnx(model, DnnEngine.Classic);
                throw new InvalidOperationException("Mini unexpectedly loaded an ONNX network.");
            }
            catch (EntryPointNotFoundException exception) when (exception.Message.Contains("jyppx_ocv_dnn_read_net_from_onnx_buffer", StringComparison.Ordinal))
            {
                Console.WriteLine("dnn=entrypoint-absent");
                Console.WriteLine("dnnInferenceCount=0");
                return;
            }
        }
        using var net = Net.ReadNetFromOnnx(model, DnnEngine.Classic);
        net.SetPreferableBackend(DnnBackend.OpenCV).SetPreferableTarget(DnnTarget.Cpu);
        Require(net.GetUnconnectedOutLayersNames().SequenceEqual(new[] { "output" }), "Wrong named output.");
        foreach (float[] input in new[] { new[] { 1F, 2F, 3F, 4F }, new[] { -4F, -3F, -2F, -1F } })
        {
            using var image = new Mat(2, 2, MatType.CV_32FC1);
            image.CopyFrom<float>(input);
            using Mat blob = JYPPX.OpenCvSharp.Dnn.Cv2.BlobFromImage(image, new Image2BlobParams());
            net.SetInput(blob, "input");
            using Mat result = net.Forward("output");
            Require(result.ToArray<float>().SequenceEqual(input), "Wrong named ONNX output values.");
        }
        Console.WriteLine("dnn=OpenCV-CPU-identity-values-exact");
        Console.WriteLine("dnnInferenceCount=2");
    }
}

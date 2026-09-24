using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using JYPPX.OpenCvSharp;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Dnn;
using JYPPX.OpenCvSharp.VideoIO;
using DnnCv2 = JYPPX.OpenCvSharp.Dnn.Cv2;

internal static class Program
{
    private const int Iterations = 100;
    private const int VideoFrameCount = 3;
    private const int VideoRows = 24;
    private const int VideoColumns = 32;
    private const string IdentityModelSha256 = "326793cdb2fc2da739a715c3f3ff71d09779b389ad29e56bbfccc4313e900744";

    private static int Main()
    {
        try
        {
            OpenCvSharpBuildInfo.VerifyNativeRuntimeCompatibility();
        }
        catch (Exception exception)
        {
            Console.WriteLine(JsonSerializer.Serialize(new { status = "skipped", reason = "native-runtime-required", detail = exception.GetType().Name }));
            return 0;
        }

        try
        {
            string videoPath = Path.Combine(Path.GetTempPath(), "opencv-csharp-scenario-" + Guid.NewGuid().ToString("N") + ".avi");
            try
            {
                VideoBenchmark video = MeasureVideo(videoPath);
                DnnBenchmark dnn = MeasureDnn();
                var result = new
                {
                    status = "measured",
                    targetFramework = OpenCvSharpBuildInfo.TargetFramework,
                    packageVersion = OpenCvSharpBuildInfo.NuGetPackageVersion,
                    openCvVersion = OpenCvSharpBuildInfo.OpenCvVersion,
                    iterations = Iterations,
                    video,
                    dnn,
                    limitations = new[]
                    {
                        "Elapsed ticks and process working-set values are runner-specific regression evidence.",
                        "WorkingSet64 is the Windows process RSS-equivalent used by this benchmark; it is sampled between operations.",
                        "The video path uses a temporary MJPG AVI and exercises file-backed VideoIO, not camera or network backends.",
                        "The DNN path uses the fixed 147-byte identity ONNX fixture and the OpenCV CPU backend."
                    }
                };
                Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                }));
                return 0;
            }
            finally
            {
                if (File.Exists(videoPath)) File.Delete(videoPath);
            }
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            return 1;
        }
    }

    private static VideoBenchmark MeasureVideo(string path)
    {
        using (var writer = new VideoWriter())
        using (var frame = new Mat(VideoRows, VideoColumns, MatType.CV_8UC3))
        {
            if (!writer.Open(path, VideoWriter.FourCC("MJPG"), 10.0, frame.Size, Array.Empty<int>()))
                throw new InvalidOperationException("The MJPG VideoWriter backend could not be opened.");

            for (int index = 0; index < VideoFrameCount; index++)
            {
                frame.SetTo(new Scalar(20 + index, 60 + index, 120 + index));
                if (!writer.Write(frame)) throw new InvalidOperationException("VideoWriter rejected a benchmark frame.");
            }
            writer.Release();
        }

        byte[] fileBytes = File.ReadAllBytes(path);
        string fileSha256 = Convert.ToHexString(SHA256.HashData(fileBytes)).ToLowerInvariant();
        for (int index = 0; index < 5; index++)
            ReadVideo(path, out _, out _, out _);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        long beforeAllocated = GC.GetAllocatedBytesForCurrentThread();
        long baselineWorkingSet = GetWorkingSet();
        long peakWorkingSet = baselineWorkingSet;
        long checksum = 0;
        int framesRead = 0;
        Stopwatch stopwatch = Stopwatch.StartNew();
        for (int iteration = 0; iteration < Iterations; iteration++)
        {
            ReadVideo(path, out int iterationFrames, out long iterationChecksum, out long iterationPeak);
            framesRead = iterationFrames;
            checksum += iterationChecksum;
            if (iterationPeak > peakWorkingSet) peakWorkingSet = iterationPeak;
        }
        stopwatch.Stop();

        if (framesRead != VideoFrameCount || checksum <= 0) throw new InvalidOperationException("Video benchmark did not read the expected frame sequence.");
        return new VideoBenchmark(
            OpenCvSharpBuildInfo.TargetFramework,
            Iterations,
            VideoFrameCount,
            VideoRows,
            VideoColumns,
            "MJPG",
            fileBytes.Length,
            fileSha256,
            new BenchmarkMetric(
                GC.GetAllocatedBytesForCurrentThread() - beforeAllocated,
                stopwatch.ElapsedTicks,
                checksum,
                baselineWorkingSet,
                peakWorkingSet));
    }

    private static void ReadVideo(string path, out int framesRead, out long checksum, out long peakWorkingSet)
    {
        using (var capture = new VideoCapture())
        using (var decoded = new Mat())
        {
            if (!capture.Open(path, VideoCaptureAPIs.Any, Array.Empty<int>()))
                throw new InvalidOperationException("The VideoCapture backend could not open the benchmark AVI.");

            framesRead = 0;
            checksum = 0;
            peakWorkingSet = GetWorkingSet();
            while (capture.Read(decoded))
            {
                ReadOnlySpan<byte> bytes = decoded.AsReadOnlySpan<byte>();
                for (int index = 0; index < bytes.Length; index++) checksum += bytes[index];
                framesRead++;
                long workingSet = GetWorkingSet();
                if (workingSet > peakWorkingSet) peakWorkingSet = workingSet;
            }
            capture.Release();
        }
    }

    private static DnnBenchmark MeasureDnn()
    {
        string modelPath = Path.Combine(AppContext.BaseDirectory, "Model", "identity-opset13.onnx.base64");
        byte[] model = Convert.FromBase64String(File.ReadAllText(modelPath, Encoding.ASCII).Trim());
        string modelSha256 = Convert.ToHexString(SHA256.HashData(model)).ToLowerInvariant();
        if (!string.Equals(modelSha256, IdentityModelSha256, StringComparison.Ordinal))
            throw new InvalidOperationException("The identity ONNX fixture hash drifted.");

        using (var net = Net.ReadNetFromOnnx(model, DnnEngine.Classic))
        using (var image = new Mat(2, 2, MatType.CV_32FC1))
        {
            image.CopyFrom(new[] { 1.0F, 2.0F, 3.0F, 4.0F });
            using (var blob = DnnCv2.BlobFromImage(image, new Image2BlobParams()))
            {
            net.SetPreferableBackend(DnnBackend.OpenCV)
                .SetPreferableTarget(DnnTarget.Cpu)
                .SetInput(blob, "input");
            string[] outputNames = net.GetUnconnectedOutLayersNames();
            if (outputNames.Length == 0) throw new InvalidOperationException("The DNN fixture has no output name.");
            string outputName = outputNames[0];

            for (int index = 0; index < 5; index++)
            {
                net.SetInput(blob, "input");
                using Mat warmup = net.Forward(outputName);
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            long beforeAllocated = GC.GetAllocatedBytesForCurrentThread();
            long baselineWorkingSet = GetWorkingSet();
            long peakWorkingSet = baselineWorkingSet;
            long checksum = 0;
            Stopwatch stopwatch = Stopwatch.StartNew();
            for (int iteration = 0; iteration < Iterations; iteration++)
            {
                net.SetInput(blob, "input");
                using Mat output = net.Forward(outputName);
                float[] values = output.ToArray<float>();
                for (int index = 0; index < values.Length; index++) checksum += checked((long)Math.Round(values[index] * 1000.0F));
                long workingSet = GetWorkingSet();
                if (workingSet > peakWorkingSet) peakWorkingSet = workingSet;
            }
            stopwatch.Stop();

            DnnPerfProfile profile = net.GetPerfProfile();
            DnnMemoryConsumption memory = net.GetMemoryConsumption(
                new[] { new[] { 1, 1, 2, 2 } },
                new[] { MatType.CV_32F });
            if (checksum <= 0 || profile.TickCount < 0)
                throw new InvalidOperationException("DNN benchmark returned an invalid checksum or profile.");

            return new DnnBenchmark(
                OpenCvSharpBuildInfo.TargetFramework,
                Iterations,
                model.Length,
                modelSha256,
                outputName,
                checksum,
                profile.TickCount,
                profile.LayerCount,
                memory.WeightsBytes,
                memory.BlobBytes,
                new BenchmarkMetric(
                    GC.GetAllocatedBytesForCurrentThread() - beforeAllocated,
                    stopwatch.ElapsedTicks,
                    checksum,
                    baselineWorkingSet,
                    peakWorkingSet));
            }
        }
    }

    private static long GetWorkingSet()
    {
        using (Process process = Process.GetCurrentProcess())
        {
            process.Refresh();
            return process.WorkingSet64;
        }
    }

    private sealed class BenchmarkMetric
    {
        public BenchmarkMetric(long managedAllocatedBytes, long elapsedTicks, long checksum, long baselineWorkingSetBytes, long peakWorkingSetBytes)
        {
            ManagedAllocatedBytes = managedAllocatedBytes;
            ElapsedTicks = elapsedTicks;
            Checksum = checksum;
            BaselineWorkingSetBytes = baselineWorkingSetBytes;
            PeakWorkingSetBytes = peakWorkingSetBytes;
        }

        public long ManagedAllocatedBytes { get; }
        public long ElapsedTicks { get; }
        public long Checksum { get; }
        public long BaselineWorkingSetBytes { get; }
        public long PeakWorkingSetBytes { get; }
    }

    private sealed class VideoBenchmark
    {
        public VideoBenchmark(string targetFramework, int iterations, int frameCount, int rows, int columns, string codec, int fileBytes, string fileSha256, BenchmarkMetric metric)
        {
            TargetFramework = targetFramework;
            Iterations = iterations;
            FrameCount = frameCount;
            Rows = rows;
            Columns = columns;
            Codec = codec;
            FileBytes = fileBytes;
            FileSha256 = fileSha256;
            Metric = metric;
        }

        public string TargetFramework { get; }
        public int Iterations { get; }
        public int FrameCount { get; }
        public int Rows { get; }
        public int Columns { get; }
        public string Codec { get; }
        public int FileBytes { get; }
        public string FileSha256 { get; }
        public BenchmarkMetric Metric { get; }
    }

    private sealed class DnnBenchmark
    {
        public DnnBenchmark(string targetFramework, int iterations, int modelBytes, string modelSha256, string outputName, long outputChecksum, long profileTickCount, int profileLayerCount, ulong weightsBytes, ulong blobBytes, BenchmarkMetric metric)
        {
            TargetFramework = targetFramework;
            Iterations = iterations;
            ModelBytes = modelBytes;
            ModelSha256 = modelSha256;
            OutputName = outputName;
            OutputChecksum = outputChecksum;
            ProfileTickCount = profileTickCount;
            ProfileLayerCount = profileLayerCount;
            WeightsBytes = weightsBytes;
            BlobBytes = blobBytes;
            Metric = metric;
        }

        public string TargetFramework { get; }
        public int Iterations { get; }
        public int ModelBytes { get; }
        public string ModelSha256 { get; }
        public string OutputName { get; }
        public long OutputChecksum { get; }
        public long ProfileTickCount { get; }
        public int ProfileLayerCount { get; }
        public ulong WeightsBytes { get; }
        public ulong BlobBytes { get; }
        public BenchmarkMetric Metric { get; }
    }
}

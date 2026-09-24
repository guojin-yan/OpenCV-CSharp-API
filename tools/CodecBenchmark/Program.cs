using System;
using System.Buffers;
using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using JYPPX.OpenCvSharp;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.ImgCodecs;
using ImgCodecsCv2 = JYPPX.OpenCvSharp.ImgCodecs.Cv2;

internal static class Program
{
    private const int Iterations = 100;
    private const int Rows = 64;
    private const int Columns = 64;

    private static int Main()
    {
        try
        {
            OpenCvSharpBuildInfo.VerifyNativeRuntimeCompatibility();
        }
        catch (Exception exception)
        {
            Console.WriteLine("{\"status\":\"skipped\",\"reason\":\"native-runtime-required\",\"detail\":\"" + Escape(exception.GetType().Name) + "\"}");
            return 0;
        }

        using (Mat image = new Mat(Rows, Columns, MatType.CV_8UC3))
        {
            byte[] input = CreateInput(image.ByteLength);
            image.CopyFrom(input);
            string inputHash = Convert.ToHexString(SHA256.HashData(input)).ToLowerInvariant();

            MeasureArray(image, out long arrayAllocations, out long arrayTicks, out int arrayBytes);
            MeasureWriter(image, out long writerAllocations, out long writerTicks, out int writerBytes);

            string json = "{\"status\":\"measured\",\"targetFramework\":\"" + Escape(OpenCvSharpBuildInfo.TargetFramework) +
                "\",\"packageVersion\":\"" + Escape(OpenCvSharpBuildInfo.NuGetPackageVersion) +
                "\",\"openCvVersion\":\"" + Escape(OpenCvSharpBuildInfo.OpenCvVersion) +
                "\",\"iterations\":" + Iterations.ToString(CultureInfo.InvariantCulture) +
                ",\"rows\":" + Rows.ToString(CultureInfo.InvariantCulture) +
                ",\"columns\":" + Columns.ToString(CultureInfo.InvariantCulture) +
                ",\"inputBytes\":" + input.Length.ToString(CultureInfo.InvariantCulture) +
                ",\"inputSha256\":\"" + inputHash +
                "\",\"byteArray\":{" +
                "\"managedAllocatedBytes\":" + arrayAllocations.ToString(CultureInfo.InvariantCulture) +
                ",\"encodedBytes\":" + arrayBytes.ToString(CultureInfo.InvariantCulture) +
                ",\"elapsedTicks\":" + arrayTicks.ToString(CultureInfo.InvariantCulture) + "}," +
                "\"bufferWriter\":{" +
                "\"managedAllocatedBytes\":" + writerAllocations.ToString(CultureInfo.InvariantCulture) +
                ",\"encodedBytes\":" + writerBytes.ToString(CultureInfo.InvariantCulture) +
                ",\"elapsedTicks\":" + writerTicks.ToString(CultureInfo.InvariantCulture) + "}}";
            Console.WriteLine(json);
        }

        return 0;
    }

    private static byte[] CreateInput(int length)
    {
        var input = new byte[length];
        for (int i = 0; i < input.Length; i++)
        {
            input[i] = (byte)((i * 37 + 19) % 256);
        }
        return input;
    }

    private static void MeasureArray(Mat image, out long allocated, out long ticks, out int encodedBytes)
    {
        for (int i = 0; i < 5; i++) _ = ImgCodecsCv2.ImEncode(".png", image);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        long before = GC.GetAllocatedBytesForCurrentThread();
        var stopwatch = Stopwatch.StartNew();
        encodedBytes = 0;
        for (int i = 0; i < Iterations; i++)
        {
            byte[] encoded = ImgCodecsCv2.ImEncode(".png", image);
            encodedBytes = encoded.Length;
        }
        stopwatch.Stop();
        allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        ticks = stopwatch.ElapsedTicks;
    }

    private static void MeasureWriter(Mat image, out long allocated, out long ticks, out int encodedBytes)
    {
        for (int i = 0; i < 5; i++)
        {
            var warmup = new ArrayBufferWriter<byte>();
            ImgCodecsCv2.ImEncodeTo(".png", image, warmup);
        }
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        long before = GC.GetAllocatedBytesForCurrentThread();
        var stopwatch = Stopwatch.StartNew();
        encodedBytes = 0;
        for (int i = 0; i < Iterations; i++)
        {
            var destination = new ArrayBufferWriter<byte>();
            ImgCodecsCv2.ImEncodeTo(".png", image, destination);
            encodedBytes = destination.WrittenCount;
        }
        stopwatch.Stop();
        allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        ticks = stopwatch.ElapsedTicks;
    }

    private static string Escape(string value)
    {
        return value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal);
    }
}

using System;
using System.Buffers;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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
            MeasureReusedWriter(
                image,
                out long reusedWriterAllocations,
                out long reusedWriterTicks,
                out int reusedWriterBytes,
                out int reusedWriterGetSpanCalls,
                out int reusedWriterAdvanceCalls);
            if (reusedWriterGetSpanCalls != Iterations || reusedWriterAdvanceCalls != Iterations)
            {
                throw new InvalidOperationException("The reusable writer must receive exactly one GetSpan and one Advance per iteration.");
            }
            byte[] encoded = ImgCodecsCv2.ImEncode(".png", image);
            string encodedHash = Convert.ToHexString(SHA256.HashData(encoded)).ToLowerInvariant();
            using (var decodeStream = new MemoryStream(encoded, writable: false))
            {
                MeasureDecodeByteArray(encoded, out long decodeArrayAllocations, out long decodeArrayTicks, out int decodeArrayBytes, out long decodeArrayChecksum);
                MeasureDecodeSpan(encoded, out long decodeSpanAllocations, out long decodeSpanTicks, out int decodeSpanBytes, out long decodeSpanChecksum);
                MeasureDecodeStream(encoded, decodeStream, out long decodeStreamAllocations, out long decodeStreamTicks, out int decodeStreamBytes, out long decodeStreamChecksum);

                if (decodeArrayBytes != decodeSpanBytes || decodeArrayBytes != decodeStreamBytes ||
                    decodeArrayChecksum != decodeSpanChecksum || decodeArrayChecksum != decodeStreamChecksum)
                {
                    throw new InvalidOperationException("Codec decode paths returned inconsistent decoded bytes or checksums.");
                }

                string json = "{\"status\":\"measured\",\"targetFramework\":\"" + Escape(OpenCvSharpBuildInfo.TargetFramework) +
                    "\",\"packageVersion\":\"" + Escape(OpenCvSharpBuildInfo.NuGetPackageVersion) +
                    "\",\"openCvVersion\":\"" + Escape(OpenCvSharpBuildInfo.OpenCvVersion) +
                    "\",\"iterations\":" + Iterations.ToString(CultureInfo.InvariantCulture) +
                    ",\"rows\":" + Rows.ToString(CultureInfo.InvariantCulture) +
                    ",\"columns\":" + Columns.ToString(CultureInfo.InvariantCulture) +
                    ",\"inputBytes\":" + input.Length.ToString(CultureInfo.InvariantCulture) +
                    ",\"inputSha256\":\"" + inputHash +
                    "\",\"encodedBytes\":" + encoded.Length.ToString(CultureInfo.InvariantCulture) +
                    ",\"encodedSha256\":\"" + encodedHash +
                    "\",\"encodeByteArray\":{" +
                    "\"managedAllocatedBytes\":" + arrayAllocations.ToString(CultureInfo.InvariantCulture) +
                    ",\"encodedBytes\":" + arrayBytes.ToString(CultureInfo.InvariantCulture) +
                    ",\"elapsedTicks\":" + arrayTicks.ToString(CultureInfo.InvariantCulture) + "}," +
                    "\"encodeBufferWriter\":{" +
                    "\"managedAllocatedBytes\":" + writerAllocations.ToString(CultureInfo.InvariantCulture) +
                    ",\"encodedBytes\":" + writerBytes.ToString(CultureInfo.InvariantCulture) +
                    ",\"elapsedTicks\":" + writerTicks.ToString(CultureInfo.InvariantCulture) + "}," +
                    "\"encodeBufferWriterReused\":{" +
                    "\"managedAllocatedBytes\":" + reusedWriterAllocations.ToString(CultureInfo.InvariantCulture) +
                    ",\"encodedBytes\":" + reusedWriterBytes.ToString(CultureInfo.InvariantCulture) +
                    ",\"elapsedTicks\":" + reusedWriterTicks.ToString(CultureInfo.InvariantCulture) +
                    ",\"getSpanCalls\":" + reusedWriterGetSpanCalls.ToString(CultureInfo.InvariantCulture) +
                    ",\"advanceCalls\":" + reusedWriterAdvanceCalls.ToString(CultureInfo.InvariantCulture) +
                    ",\"writerCapacity\":4096,\"writerReused\":true}," +
                    "\"decodeByteArray\":{" + Metrics(decodeArrayAllocations, decodeArrayTicks, decodeArrayBytes, decodeArrayChecksum) + "}," +
                    "\"decodeSpanWithPreflight\":{" + Metrics(decodeSpanAllocations, decodeSpanTicks, decodeSpanBytes, decodeSpanChecksum) + "}," +
                    "\"decodeStreamWithPreflight\":{" + Metrics(decodeStreamAllocations, decodeStreamTicks, decodeStreamBytes, decodeStreamChecksum) + "}}";
                Console.WriteLine(json);
            }
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

    private static void MeasureReusedWriter(
        Mat image,
        out long allocated,
        out long ticks,
        out int encodedBytes,
        out int getSpanCalls,
        out int advanceCalls)
    {
        var destination = new ReusableWriter(4096);
        for (int i = 0; i < 5; i++)
        {
            destination.Reset();
            ImgCodecsCv2.ImEncodeTo(".png", image, destination);
        }

        destination.ResetCounters();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        long before = GC.GetAllocatedBytesForCurrentThread();
        var stopwatch = Stopwatch.StartNew();
        encodedBytes = 0;
        for (int i = 0; i < Iterations; i++)
        {
            destination.Reset();
            ImgCodecsCv2.ImEncodeTo(".png", image, destination);
            encodedBytes = destination.WrittenCount;
        }

        stopwatch.Stop();
        allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        ticks = stopwatch.ElapsedTicks;
        getSpanCalls = destination.GetSpanCalls;
        advanceCalls = destination.AdvanceCalls;
    }

    private static void MeasureDecodeByteArray(byte[] encoded, out long allocated, out long ticks, out int decodedBytes, out long checksum)
    {
        for (int i = 0; i < 5; i++) { using Mat warmup = ImgCodecsCv2.ImDecode(encoded, ImreadModes.Color); }
        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        long before = GC.GetAllocatedBytesForCurrentThread(); var stopwatch = Stopwatch.StartNew();
        decodedBytes = 0; checksum = 0;
        for (int i = 0; i < Iterations; i++)
        {
            using Mat decoded = ImgCodecsCv2.ImDecode(encoded, ImreadModes.Color);
            decodedBytes = checked((int)decoded.ByteLength);
            checksum += Checksum(decoded);
        }
        stopwatch.Stop(); allocated = GC.GetAllocatedBytesForCurrentThread() - before; ticks = stopwatch.ElapsedTicks;
    }

    private static void MeasureDecodeSpan(byte[] encoded, out long allocated, out long ticks, out int decodedBytes, out long checksum)
    {
        ImageDecodeOptions options = new ImageDecodeOptions();
        for (int i = 0; i < 5; i++) { using Mat warmup = ImgCodecsCv2.ImDecode(encoded.AsSpan(), options, ImreadModes.Color); }
        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        long before = GC.GetAllocatedBytesForCurrentThread(); var stopwatch = Stopwatch.StartNew();
        decodedBytes = 0; checksum = 0;
        for (int i = 0; i < Iterations; i++)
        {
            using Mat decoded = ImgCodecsCv2.ImDecode(encoded.AsSpan(), options, ImreadModes.Color);
            decodedBytes = checked((int)decoded.ByteLength);
            checksum += Checksum(decoded);
        }
        stopwatch.Stop(); allocated = GC.GetAllocatedBytesForCurrentThread() - before; ticks = stopwatch.ElapsedTicks;
    }

    private static void MeasureDecodeStream(byte[] encoded, MemoryStream stream, out long allocated, out long ticks, out int decodedBytes, out long checksum)
    {
        ImageDecodeOptions options = new ImageDecodeOptions();
        for (int i = 0; i < 5; i++) { stream.Position = 0; using Mat warmup = ImgCodecsCv2.ImDecode(stream, options, ImreadModes.Color); }
        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        long before = GC.GetAllocatedBytesForCurrentThread(); var stopwatch = Stopwatch.StartNew();
        decodedBytes = 0; checksum = 0;
        for (int i = 0; i < Iterations; i++)
        {
            stream.Position = 0;
            using Mat decoded = ImgCodecsCv2.ImDecode(stream, options, ImreadModes.Color);
            decodedBytes = checked((int)decoded.ByteLength);
            checksum += Checksum(decoded);
        }
        stopwatch.Stop(); allocated = GC.GetAllocatedBytesForCurrentThread() - before; ticks = stopwatch.ElapsedTicks;
    }

    private static long Checksum(Mat image)
    {
        ReadOnlySpan<byte> bytes = image.AsReadOnlySpan<byte>();
        long checksum = 0;
        for (int i = 0; i < bytes.Length; i++) checksum += bytes[i];
        return checksum;
    }

    private static string Metrics(long allocated, long ticks, int decodedBytes, long checksum)
    {
        return "\"managedAllocatedBytes\":" + allocated.ToString(CultureInfo.InvariantCulture) +
            ",\"decodedBytes\":" + decodedBytes.ToString(CultureInfo.InvariantCulture) +
            ",\"checksum\":" + checksum.ToString(CultureInfo.InvariantCulture) +
            ",\"elapsedTicks\":" + ticks.ToString(CultureInfo.InvariantCulture);
    }

    private static string Escape(string value)
    {
        return value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal);
    }

    private sealed class ReusableWriter : IBufferWriter<byte>
    {
        private readonly byte[] buffer;

        public ReusableWriter(int capacity)
        {
            buffer = new byte[capacity];
        }

        public int WrittenCount { get; private set; }

        public int GetSpanCalls { get; private set; }

        public int AdvanceCalls { get; private set; }

        public void Reset()
        {
            WrittenCount = 0;
        }

        public void ResetCounters()
        {
            GetSpanCalls = 0;
            AdvanceCalls = 0;
        }

        public void Advance(int count)
        {
            AdvanceCalls++;
            if (count < 0 || count > buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            WrittenCount = count;
        }

        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            ValidateSizeHint(sizeHint);
            GetSpanCalls++;
            return buffer;
        }

        public Span<byte> GetSpan(int sizeHint = 0)
        {
            ValidateSizeHint(sizeHint);
            GetSpanCalls++;
            return buffer.AsSpan();
        }

        private void ValidateSizeHint(int sizeHint)
        {
            if (sizeHint < 0 || sizeHint > buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(sizeHint));
            }
        }
    }
}

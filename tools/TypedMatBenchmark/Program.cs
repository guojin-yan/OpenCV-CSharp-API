using System;
using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using JYPPX.OpenCvSharp;
using JYPPX.OpenCvSharp.Core;

internal static class Program
{
    private const int Iterations = 100;
    private const int Rows = 128;
    private const int Columns = 128;

    private static int Main()
    {
        try { OpenCvSharpBuildInfo.VerifyNativeRuntimeCompatibility(); }
        catch (Exception exception)
        {
            Console.WriteLine("{\"status\":\"skipped\",\"reason\":\"native-runtime-required\",\"detail\":\"" + Escape(exception.GetType().Name) + "\"}");
            return 0;
        }

        using (Mat mat = new Mat(Rows, Columns, MatType.CV_8UC1))
        using (Mat roi = mat.SubMat(new Rect(3, 5, Columns - 6, Rows - 10)))
        {
            mat.SetTo(new Scalar(7));
            string inputHash = Convert.ToHexString(SHA256.HashData(mat.ToBytes())).ToLowerInvariant();
            MeasureRowAccessor(roi, out long rowAllocated, out long rowTicks, out long rowChecksum);
            MeasureView(roi, out long viewAllocated, out long viewTicks, out long viewChecksum);
            MeasureReadOnlyView(roi, out long readOnlyAllocated, out long readOnlyTicks, out long readOnlyChecksum);
            string json = "{\"status\":\"measured\",\"targetFramework\":\"" + Escape(OpenCvSharpBuildInfo.TargetFramework) +
                "\",\"iterations\":" + Iterations.ToString(CultureInfo.InvariantCulture) +
                ",\"rows\":" + roi.Rows.ToString(CultureInfo.InvariantCulture) +
                ",\"columns\":" + roi.Cols.ToString(CultureInfo.InvariantCulture) +
                ",\"inputSha256\":\"" + inputHash +
                "\",\"rowAccessor\":{" + Metrics(rowAllocated, rowTicks, rowChecksum) +
                "},\"matView\":{" + Metrics(viewAllocated, viewTicks, viewChecksum) +
                "},\"readOnlyMatView\":{" + Metrics(readOnlyAllocated, readOnlyTicks, readOnlyChecksum) + "}}";
            Console.WriteLine(json);
        }
        return 0;
    }

    private static void MeasureRowAccessor(Mat mat, out long allocated, out long ticks, out long checksum)
    {
        for (int i = 0; i < 3; i++) checksum = SumRows(mat);
        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        long before = GC.GetAllocatedBytesForCurrentThread(); var timer = Stopwatch.StartNew(); checksum = 0;
        for (int i = 0; i < Iterations; i++) checksum += SumRows(mat);
        timer.Stop(); allocated = GC.GetAllocatedBytesForCurrentThread() - before; ticks = timer.ElapsedTicks;
    }

    private static long SumRows(Mat mat)
    {
        MatRowAccessor<byte> rows = mat.AsRows<byte>(); long total = 0;
        for (int row = 0; row < rows.Count; row++) { Span<byte> values = rows[row]; for (int col = 0; col < values.Length; col++) total += values[col]; }
        return total;
    }

    private static void MeasureView(Mat mat, out long allocated, out long ticks, out long checksum)
    {
        for (int i = 0; i < 3; i++) { using MatView<byte> view = mat.AsView<byte>(); checksum = SumView(view); }
        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        long before = GC.GetAllocatedBytesForCurrentThread(); var timer = Stopwatch.StartNew(); checksum = 0;
        for (int i = 0; i < Iterations; i++) { using MatView<byte> view = mat.AsView<byte>(); checksum += SumView(view); }
        timer.Stop(); allocated = GC.GetAllocatedBytesForCurrentThread() - before; ticks = timer.ElapsedTicks;
    }

    private static long SumView(MatView<byte> view)
    {
        long total = 0; for (int row = 0; row < view.Rows; row++) { ReadOnlySpan<byte> values = view.AsReadOnlyRowSpan(row); for (int col = 0; col < values.Length; col++) total += values[col]; } return total;
    }

    private static void MeasureReadOnlyView(Mat mat, out long allocated, out long ticks, out long checksum)
    {
        for (int i = 0; i < 3; i++) { using ReadOnlyMatView<byte> view = mat.AsReadOnlyView<byte>(); checksum = SumReadOnlyView(view); }
        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        long before = GC.GetAllocatedBytesForCurrentThread(); var timer = Stopwatch.StartNew(); checksum = 0;
        for (int i = 0; i < Iterations; i++) { using ReadOnlyMatView<byte> view = mat.AsReadOnlyView<byte>(); checksum += SumReadOnlyView(view); }
        timer.Stop(); allocated = GC.GetAllocatedBytesForCurrentThread() - before; ticks = timer.ElapsedTicks;
    }

    private static long SumReadOnlyView(ReadOnlyMatView<byte> view)
    {
        long total = 0; for (int row = 0; row < view.Rows; row++) { ReadOnlySpan<byte> values = view.AsReadOnlyRowSpan(row); for (int col = 0; col < values.Length; col++) total += values[col]; } return total;
    }

    private static string Metrics(long allocation, long ticks, long checksum) =>
        "\"managedAllocatedBytes\":" + allocation.ToString(CultureInfo.InvariantCulture) + ",\"elapsedTicks\":" + ticks.ToString(CultureInfo.InvariantCulture) + ",\"checksum\":" + checksum.ToString(CultureInfo.InvariantCulture);

    private static string Escape(string value) => value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal);
}

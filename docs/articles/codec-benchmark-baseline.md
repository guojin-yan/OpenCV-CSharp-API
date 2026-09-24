# Codec Benchmark Baseline / 编解码基线

The repository contains a standalone net8.0 harness at `tools/CodecBenchmark`. It uses a fixed 64x64 `CV_8UC3` input, records its SHA256, warms each path five times, then measures 100 iterations of the existing `byte[]` PNG encoder and the preview `ImEncodeTo(..., IBufferWriter<byte>)` path.

仓库提供独立的 net8.0 harness：`tools/CodecBenchmark`。它使用固定的 64x64 `CV_8UC3` 输入并记录 SHA256，分别预热五次，然后测量现有 `byte[]` PNG encoder 和 preview `ImEncodeTo(..., IBufferWriter<byte>)` 路径各 100 次。

Run with a factual native runtime directory:

```powershell
dotnet run --project .\tools\CodecBenchmark\CodecBenchmark.csproj -c Release -p:OpenCvNativeRuntimeDir=C:\path\to\native-runtime
```

The JSON output records managed allocation bytes from `GC.GetAllocatedBytesForCurrentThread`, encoded byte length, and elapsed stopwatch ticks for each path. It is a regression baseline for the selected runner and input; it is not a cross-machine performance claim. Without a matching native runtime the harness emits a single `skipped` JSON object and does not fabricate measurements.

JSON 输出记录 `GC.GetAllocatedBytesForCurrentThread` 的托管分配字节、编码长度和每条路径的 stopwatch ticks。它是指定 runner 与固定输入的回归基线，不是跨机器性能声明。没有匹配 native runtime 时，harness 只输出一个 `skipped` JSON 对象，不伪造测量数据。

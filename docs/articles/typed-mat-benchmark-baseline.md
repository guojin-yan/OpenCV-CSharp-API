# Typed Mat Benchmark Baseline / Typed Mat 基线

`tools/TypedMatBenchmark` is a standalone net8.0/net10.0 harness for the conditional typed-view preview. It creates a fixed 128x128 `CV_8UC1` matrix and a non-contiguous ROI, then performs 100 iterations over the same rows using `MatRowAccessor<byte>`, `MatView<byte>`, and `ReadOnlyMatView<byte>`. It records the input hash, managed allocations from `GC.GetAllocatedBytesForCurrentThread`, stopwatch ticks, and a checksum for each path.

`tools/TypedMatBenchmark` 是一个独立的 net8.0/net10.0 harness，用于条件性 typed-view preview。它创建固定的 128x128 `CV_8UC1` 矩阵和非连续 ROI，用 `MatRowAccessor<byte>`、`MatView<byte>` 和 `ReadOnlyMatView<byte>` 对同一组行执行 100 次循环，并记录输入 hash、`GC.GetAllocatedBytesForCurrentThread` 托管分配、stopwatch ticks 以及每条路径的 checksum。

Run it with a factual native runtime directory:

```powershell
dotnet run --project .\tools\TypedMatBenchmark\TypedMatBenchmark.csproj -c Release -p:OpenCvNativeRuntimeDir=C:\path\to\native-runtime
```

The harness emits `measured` JSON only when the native runtime is verified. Without it, the guard emits `skipped` and never creates synthetic performance data. Results are runner-specific regression evidence and do not stabilize the public MatView contract by themselves.

只有在 native runtime 验证成功时，harness 才输出 `measured` JSON。没有 runtime 时输出 `skipped`，不会生成合成性能数据。结果是 runner-specific 回归证据，不能单独将 MatView 晋升为稳定 public contract。

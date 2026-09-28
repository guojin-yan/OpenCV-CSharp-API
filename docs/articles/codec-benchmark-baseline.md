# Codec Benchmark Baseline / 编解码基线

The repository contains a standalone net8.0 harness at `tools/CodecBenchmark`. It uses a fixed 64x64 `CV_8UC3` input, records its SHA256, warms each path five times, then measures 100 iterations of the existing `byte[]` PNG encoder, the preview `ImEncodeTo(..., IBufferWriter<byte>)` path, and three decode paths: byte array, `ReadOnlySpan<byte>` with preflight, and seekable `Stream` with preflight. The decode paths must report identical decoded-byte counts and checksums.

仓库提供独立的 net8.0 harness：`tools/CodecBenchmark`。它使用固定的 64x64 `CV_8UC3` 输入并记录 SHA256，分别预热五次，然后测量现有 `byte[]` PNG encoder、preview `ImEncodeTo(..., IBufferWriter<byte>)`，以及 byte array、带 preflight 的 `ReadOnlySpan<byte>`、带 preflight 的 seekable `Stream` 三条 decode 路径各 100 次。三条 decode 路径必须报告相同的 decoded-byte 数量和 checksum。

Run with a factual native runtime directory:

```powershell
dotnet run --project .\tools\CodecBenchmark\CodecBenchmark.csproj -c Release -p:OpenCvNativeRuntimeDir=C:\path\to\native-runtime
```

The JSON output records managed allocation bytes from `GC.GetAllocatedBytesForCurrentThread`, encoded byte length, and elapsed stopwatch ticks for each path. It is a regression baseline for the selected runner and input; it is not a cross-machine performance claim. Without a matching native runtime the harness emits a single `skipped` JSON object and does not fabricate measurements.

JSON 输出记录 `GC.GetAllocatedBytesForCurrentThread` 的托管分配字节、编码长度和每条路径的 stopwatch ticks。它是指定 runner 与固定输入的回归基线，不是跨机器性能声明。没有匹配 native runtime 时，harness 只输出一个 `skipped` JSON 对象，不伪造测量数据。

The measured Windows x64 evidence is recorded in [`codec-typed-mat-benchmark-evidence.json`](../../packaging/performance/codec-typed-mat-benchmark-evidence.json). [`Measure-CodecTypedMatBenchmark.ps1`](../../scripts/Measure-CodecTypedMatBenchmark.ps1) binds it to one clean source commit, managed assembly hashes for net8.0/net10.0, the exact runtime NuGet package, and all 18 DLL hashes. The matching Video/DNN measurements and fixed regression limits are described in [PERF-001 Regression Baseline](perf-001-regression-baseline.md).

已测量的 Windows x64 证据记录在 [`codec-typed-mat-benchmark-evidence.json`](../../packaging/performance/codec-typed-mat-benchmark-evidence.json) 中。[`Measure-CodecTypedMatBenchmark.ps1`](../../scripts/Measure-CodecTypedMatBenchmark.ps1) 将证据绑定到单一干净源码提交、net8.0/net10.0 managed assembly hash、准确的 runtime NuGet 包和全部 18 个 DLL hash。配对的 Video/DNN 测量与固定回归阈值见 [PERF-001 性能回归基线](perf-001-regression-baseline.md)。

# Video And DNN Benchmark Baseline / Video 与 DNN 基线

`tools/ScenarioBenchmark` measures two native-backed workflows that were missing from the 5.0.1 performance baseline: a fixed three-frame MJPG AVI round trip through `VideoWriter` and `VideoCapture`, and 100 CPU OpenCV DNN forwards through the 147-byte identity ONNX fixture. Each scenario records the fixed input identity, managed allocations, elapsed stopwatch ticks, output checksum, and sampled process working-set baseline/peak.

`WorkingSet64` is the Windows process RSS-equivalent used by this evidence. It is sampled between operations and is a regression signal for the selected runner, not a portable memory guarantee. The video path exercises file-backed VideoIO and does not claim camera or network backend support. The DNN path exercises the OpenCV CPU backend and does not claim GPU acceleration or model coverage.

在提供真实 full native runtime 的 Windows x64 runner 上运行：

```powershell
dotnet run --project .\tools\ScenarioBenchmark\ScenarioBenchmark.csproj -c Release `
  -p:OpenCvNativeRuntimeDir=C:\path\to\native-runtime
```

没有匹配的 native runtime 时，工具只输出 `status=skipped`；有 runtime 但 VideoIO 或 DNN 场景不能执行时，工具失败，不会生成部分测量。结构化证据见 [`video-dnn-benchmark-evidence.json`](../../packaging/performance/video-dnn-benchmark-evidence.json)，由 [`Test-VideoDnnBenchmarkEvidence.ps1`](../../scripts/Test-VideoDnnBenchmarkEvidence.ps1) 校验。

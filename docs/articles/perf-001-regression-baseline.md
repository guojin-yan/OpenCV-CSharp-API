# PERF-001 Regression Baseline / PERF-001 性能回归基线

`Measure-CodecTypedMatBenchmark.ps1` and `Measure-VideoDnnBenchmark.ps1` create the two evidence sets from one clean source commit, Windows x64 runner, .NET SDK, and exact full runtime package. Each script verifies the 18 DLL files against the `win-x64` payload inside the supplied NuGet package. Codec and typed Mat evidence covers the fixed PNG encode/decode and Mat-view inputs on net8.0/net10.0; the paired Video/DNN evidence covers the fixed MJPG file and identity ONNX model on net10.0.

Run both measurement scripts without changing source between runs:

```powershell
$runtimeDir = 'C:\path\to\runtimes\win-x64\native'
$runtimePackage = 'C:\path\to\jyppx.opencv.runtime.win-x64.5.0.0.nupkg'
pwsh -NoProfile -File .\scripts\Measure-CodecTypedMatBenchmark.ps1 `
  -OpenCvNativeRuntimeDir $runtimeDir -NativeRuntimePackagePath $runtimePackage
pwsh -NoProfile -File .\scripts\Measure-VideoDnnBenchmark.ps1 `
  -OpenCvNativeRuntimeDir $runtimeDir -NativeRuntimePackagePath $runtimePackage
```

The first reviewed measurement can be made into an immutable baseline with `New-PerformanceRegressionBaseline.ps1`. Routine checks use `Test-PerformanceRegressionBaseline.ps1`, which requires the same measured source commit (or a descendant), exact package and payload hashes, runner identity, fixed workload hashes, and all 100-iteration scenarios. Per-metric limits are elapsed ticks +20%, managed allocated bytes +10%, and sampled peak working set +10%. A changed runner, package, native DLL, input, metric set, or threshold fails closed instead of comparing unlike measurements. Refreshing the baseline is an explicit reviewed evidence change.

Elapsed ticks and working-set samples are machine-specific regression signals, not cross-machine guarantees. Peak working set is sampled process working set on Windows, and the Video/DNN scenarios exercise file-backed VideoIO and CPU inference only.

---

`Measure-CodecTypedMatBenchmark.ps1` 与 `Measure-VideoDnnBenchmark.ps1` 必须在同一干净源码提交、Windows x64 runner、.NET SDK 和完整 runtime 包上生成两组证据。每个脚本都会将 runtime 目录中的 18 个 DLL 与 NuGet 包内 `win-x64` payload 逐项校验。Codec 与 Typed Mat 记录固定 PNG 编解码和 Mat view 输入，并覆盖 net8.0/net10.0；Video/DNN 记录固定 MJPG 文件与 identity ONNX 模型，并运行于 net10.0。

两次测量之间不要改动源码。`New-PerformanceRegressionBaseline.ps1` 用于显式创建首个经审阅的不可变基线；常规检查使用 `Test-PerformanceRegressionBaseline.ps1`。比较要求源码提交相同或为其后代，并严格匹配 package/payload hash、runner、固定 workload hash 与 100 次迭代场景。单项阈值为 elapsed ticks 增长 20%、托管分配增长 10%、采样 peak working set 增长 10%。runner、包、native DLL、输入、指标集合或阈值变化都会停止比较，避免把不相同的测量混为一谈。刷新基线需要显式审阅证据变更。

Elapsed ticks 和 working-set 采样只用于同机器回归信号，不构成跨机器保证。Peak working set 是 Windows 进程 working set 采样；Video/DNN 场景只覆盖文件 VideoIO 和 CPU inference。

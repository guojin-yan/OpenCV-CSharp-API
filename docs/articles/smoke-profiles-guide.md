# Smoke Profiles Guide



Linked smoke profiles assume the managed package `JYPPX.OpenCV.CSharp.API` and the matching full `JYPPX.OpenCV.runtime.<rid>` or mini `JYPPX.OpenCV.runtime.<rid>.mini` package use the same normalized NuGet package version. Choose the runtime package for the target RID/profile under smoke.

linked smoke profile 默认 managed 主包 `JYPPX.OpenCV.CSharp.API` 与匹配的 full `JYPPX.OpenCV.runtime.<rid>` 或 mini `JYPPX.OpenCV.runtime.<rid>.mini` 包使用相同的四段 package version 元数据。请选择 smoke 目标 target RID/profile 对应的 runtime 包。

For Linux smoke, choose the distro-specific Linux RID package that matches the runtime output, such as `JYPPX.OpenCV.runtime.ubuntu.22.04-x64`, `JYPPX.OpenCV.runtime.ubuntu.24.04-x64`, `JYPPX.OpenCV.runtime.debian.12-x64`, or `JYPPX.OpenCV.runtime.alpine.3.20-x64`. These package IDs intentionally differ from generic `linux-x64`; external consumer projects should set `RuntimeIdentifierGraphPath` to `packaging/runtime/runtime-distro-rid-graph.json` or an equivalent copied graph before restore when the selected distro RID is project-defined.

Linux smoke 应选择与 runtime 输出匹配的 distro-specific Linux RID package，例如 `JYPPX.OpenCV.runtime.ubuntu.22.04-x64`、`JYPPX.OpenCV.runtime.ubuntu.24.04-x64`、`JYPPX.OpenCV.runtime.debian.12-x64` 或 `JYPPX.OpenCV.runtime.alpine.3.20-x64`。这些 package ID 有意区别于通用 `linux-x64`；外部 consumer project 使用项目自定义 distro RID 时，如果 SDK 默认不认识所选 RID，应在 restore 前把 `RuntimeIdentifierGraphPath` 指向 `packaging/runtime/runtime-distro-rid-graph.json` 或复制后的等效 graph。

If no matching runtime package is available yet, use `Build-OpenCV.ps1` and `Stage-Runtime.ps1` to prepare a local native runtime, then pass that output to tests or samples with `OpenCvNativeRuntimeDir`.

如果 no matching runtime package is available yet，请用 `Build-OpenCV.ps1` 和 `Stage-Runtime.ps1` 准备 local native runtime，然后通过 `OpenCvNativeRuntimeDir` 把该输出传给测试或样例。

Build and staging fallback details are in the [Linked Runtime Build Guide](linked-runtime-build-guide.md); end-to-end linked validation commands are in the [Linked Runtime Smoke Guide](linked-runtime-smoke-guide.md).

构建和 staging fallback 细节见 [Linked Runtime Build Guide](linked-runtime-build-guide.md)；端到端 linked 验证命令见 [Linked Runtime Smoke Guide](linked-runtime-smoke-guide.md)。

## Default Tests / 默认测试

```powershell
dotnet test .\tests\OpenCvSharp.Tests\OpenCvSharp.Tests.csproj -c Release -f net10.0
```

Default tests leave smoke environment variables unset. They cover managed object shape, argument validation, disposed-state behavior, enum/value contracts, and no-OpenCV `NOT_LINKED` boundaries. They must not require downloads, cameras, GUI windows, online resources, real model files, or known process-fatal linked paths.

默认测试不设置 smoke 环境变量。它们覆盖 managed 对象形状、参数校验、disposed-state 行为、枚举/数值契约，以及 no-OpenCV 下的 `NOT_LINKED` 边界。默认测试不能依赖下载、摄像头、GUI 窗口、在线资源、真实模型文件或已知可能导致进程级崩溃的 linked 路径。

## Ordinary Native Smoke / 普通 Native Smoke

```powershell
$env:OPENCV_CSHARP_NATIVE_SMOKE='1'
Remove-Item Env:\OPENCV_CSHARP_UNSTABLE_NATIVE_SMOKE -ErrorAction SilentlyContinue
# Clear the legacy compatibility alias too so ordinary smoke cannot inherit unstable mode.
Remove-Item Env:\OPENCV_CSHARP_BIOINSPIRED_NATIVE_SMOKE -ErrorAction SilentlyContinue
dotnet test .\tests\OpenCvSharp.Tests\OpenCvSharp.Tests.csproj -c Release -f net10.0
```



## Console Samples / Console 样例

```powershell
dotnet build .\samples\ConsoleSamples\ConsoleSamples.csproj -c Release
dotnet run --project .\samples\ConsoleSamples\ConsoleSamples.csproj -c Release --no-build
```

The default console sample runs stable synthetic paths. Extended sample coverage is opt-in:

```powershell
$env:OPENCV_CSHARP_CONSOLE_EXTENDED='1'
dotnet run --project .\samples\ConsoleSamples\ConsoleSamples.csproj -c Release --no-build
```

The extended path may touch more optional contrib modules. BioInspired remains skipped unless `OPENCV_CSHARP_UNSTABLE_NATIVE_SMOKE=1` is also set.

默认 Console sample 运行稳定的合成数据路径。扩展样例覆盖需要显式设置 `OPENCV_CSHARP_CONSOLE_EXTENDED=1`。扩展路径可能触及更多 optional contrib 模块；BioInspired 仍会跳过，除非同时设置 `OPENCV_CSHARP_UNSTABLE_NATIVE_SMOKE=1`。

## HighGUI Smoke / HighGUI Smoke

```powershell
$env:OPENCV_CSHARP_HIGHGUI_SMOKE='1'
dotnet test .\tests\OpenCvSharp.Tests\OpenCvSharp.Tests.csproj -c Release -f net10.0 --filter FullyQualifiedName~HighGui
```

HighGUI smoke opens native GUI windows and should run only on an interactive desktop with the required platform GUI dependencies. Headless CI should leave `OPENCV_CSHARP_HIGHGUI_SMOKE` unset.

HighGUI smoke 会打开 native GUI 窗口，只应在具备交互式桌面和所需平台 GUI 依赖的机器上运行。无头 CI 应保持 `OPENCV_CSHARP_HIGHGUI_SMOKE` 未设置。

## Model-Backed Smoke / 模型驱动 Smoke

Some native paths require caller-provided assets. Keep them out of default tests and ordinary synthetic smoke unless the paths are explicitly configured:

- DNN forward/profile smoke uses `OPENCV_CSHARP_DNN_MODEL`, `OPENCV_CSHARP_DNN_CONFIG`, and optionally `OPENCV_CSHARP_DNN_FRAMEWORK`.
- BRISQUE quality smoke uses `OPENCV_CSHARP_BRISQUE_MODEL` and `OPENCV_CSHARP_BRISQUE_RANGE`.
- Face and cascade-backed smoke can use `OPENCV_CSHARP_FACE_CASCADE`, `OPENCV_CSHARP_FACE_DETECTOR_MODEL`, and `OPENCV_CSHARP_FACE_RECOGNIZER_MODEL`.
- Stitching sample input uses `OPENCV_CSHARP_STITCHING_IMAGES`.

部分 native 路径需要调用方提供资产。除非显式配置路径，否则它们不应进入默认测试或普通合成 smoke：

- DNN forward/profile smoke 使用 `OPENCV_CSHARP_DNN_MODEL`、`OPENCV_CSHARP_DNN_CONFIG`，并可选使用 `OPENCV_CSHARP_DNN_FRAMEWORK`。
- BRISQUE quality smoke 使用 `OPENCV_CSHARP_BRISQUE_MODEL` 和 `OPENCV_CSHARP_BRISQUE_RANGE`。
- Face 和 cascade 相关 smoke 可使用 `OPENCV_CSHARP_FACE_CASCADE`、`OPENCV_CSHARP_FACE_DETECTOR_MODEL` 和 `OPENCV_CSHARP_FACE_RECOGNIZER_MODEL`。
- Stitching 样例输入使用 `OPENCV_CSHARP_STITCHING_IMAGES`。

## Unstable Native Smoke / 不稳定 Native Smoke

```powershell
.\scripts\Run-UnstableSmoke.ps1 -Framework net10.0 -Filter 'FullyQualifiedName~BioInspired'
```

`OPENCV_CSHARP_UNSTABLE_NATIVE_SMOKE=1` enables experimental linked paths that can expose local runtime crashes, aborts, or access violations. The current unstable profile includes BioInspired linked object, metadata, Retina, tone-mapping, and transient segmentation paths. Do not enable this profile in default CI or broad `dotnet test` runs.

`scripts\Run-UnstableSmoke.ps1` is the preferred manual diagnostic entry. It sets `OPENCV_CSHARP_UNSTABLE_NATIVE_SMOKE=1` for the child `dotnet test` process, clears the older existing-workflow unstable compatibility alias for that child process, defaults to a filtered BioInspired run, clears both ordinary native smoke environment switches (the neutral variable and the existing-workflow compatibility alias) unless `-IncludeOrdinaryNativeSmoke` is supplied, and writes a diagnostic testhost log. Its neutral `-ProjectPath` parameter accepts a repository-relative or absolute test project path; the default is the version-neutral project `tests\OpenCvSharp.Tests\OpenCvSharp.Tests.csproj`. The neutral `-DiagLog` parameter also accepts a repository-relative or absolute diagnostic path; its default remains `artifacts\unstable-smoke-testhost.log`. Pass `-IncludeOrdinaryNativeSmoke` only when deliberately combining ordinary native smoke with an unstable diagnostic filter.

`OPENCV_CSHARP_UNSTABLE_NATIVE_SMOKE=1` 会启用实验性 linked 路径，可能暴露本地 runtime crash、abort 或 access violation。当前 unstable profile 包括 BioInspired linked object、metadata、Retina、tone-mapping 和 transient segmentation 路径。不要在默认 CI 或宽泛的 `dotnet test` 中启用该 profile。

`scripts\Run-UnstableSmoke.ps1` 是推荐的手动诊断入口。它对子 `dotnet test` 进程设置 `OPENCV_CSHARP_UNSTABLE_NATIVE_SMOKE=1`，并为该子进程清除旧的既有 workflow unstable 兼容别名；默认运行过滤后的 BioInspired 测试，并在未传入 `-IncludeOrdinaryNativeSmoke` 时清除 ordinary native smoke 的中性环境变量及其既有 workflow 兼容别名，然后写出 testhost 诊断日志。中性的 `-ProjectPath` 参数可以接受仓库相对或绝对测试项目路径，默认值是版本中立的项目 `tests\OpenCvSharp.Tests\OpenCvSharp.Tests.csproj`。中性的 `-DiagLog` 参数也可以接受仓库相对或绝对诊断路径，默认值仍为 `artifacts\unstable-smoke-testhost.log`。只有在明确需要把普通 native smoke 与 unstable 诊断过滤器组合时，才传入 `-IncludeOrdinaryNativeSmoke`。

## Related Guide / 相关文档

See [Linked Runtime Smoke Guide](linked-runtime-smoke-guide.md) for module-level smoke notes and factual runtime DLL expectations for the current packaged runtime identity, OpenCV 5.0.0.

模块级 smoke 说明，以及当前打包 runtime 身份 OpenCV 5.0.0 的事实性 runtime DLL 预期见 [Linked Runtime Smoke Guide](linked-runtime-smoke-guide.md)。

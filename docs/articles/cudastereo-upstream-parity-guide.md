# CudaStereo Upstream Map / CudaStereo 上游映射

This guide records the scoped OpenCV 5.0.0 contrib `cudastereo` declaration closure and exact build prerequisite evidence. It does not claim CUDA support or repository-wide OpenCV parity.

本指南记录 OpenCV 5.0.0 contrib `cudastereo` 声明范围及精确构建前提证据，不宣称本包支持 CUDA，也不宣称 repository-wide OpenCV parity。

## Scope / 范围

The map covers parser-emitted declarations from `opencv2/cudastereo.hpp`. Callable declarations are classified against the current native ABI and managed public API; absence of a CudaStereo wrapper is recorded as an intentional omission, not as implemented parity.

映射覆盖 parser 从 `opencv2/cudastereo.hpp` 输出的声明。Callable 会与当前 native ABI 和 managed public API 对照；当前没有 CudaStereo wrapper 的声明会明确记录为 intentional omission，而非已实现 parity。

The guard hashes the upstream module `CMakeLists.txt`. OpenCV disables this module on iOS/WinRT and when neither CUDA nor CUDA stubs are enabled. It depends on `opencv_geometry` and `opencv_stereo`, optionally `opencv_cudev`; first-class CUDA builds link the CUDA runtime. GPU matrix/stream ownership, asynchronous disparity transport, and parameter-rich belief-propagation/SGM algorithm state remain outside this package's contract.

专项 guard 将 upstream `CMakeLists.txt` 的 hash 作为证据。OpenCV 在 iOS/WinRT 上禁用该模块，并在 CUDA 与 CUDA stubs 均未启用时禁用。该模块依赖 `opencv_geometry` 和 `opencv_stereo`，可选依赖 `opencv_cudev`；first-class CUDA 构建链接 CUDA runtime。GPU matrix/stream 生命周期、异步视差数据传输及参数丰富的 belief-propagation/SGM 算法状态不属于本包契约。

## Verification / 验证

```powershell
pwsh -NoProfile -File .\scripts\Test-CudaStereoUpstreamMap.ps1
pwsh -NoProfile -File .\scripts\Test-UpstreamMapCatalog.ps1
```

The focused guard verifies parser closure, declaration partition, exact module build gate, CMake dependency evidence, absence of native/managed CudaStereo wrapper evidence, and the prohibition on repository-wide parity claims.

专项 guard 检查 parser closure、声明分区、模块构建门控、CMake 依赖证据、native/managed CudaStereo wrapper 缺失，以及禁止 repository-wide parity 声明。

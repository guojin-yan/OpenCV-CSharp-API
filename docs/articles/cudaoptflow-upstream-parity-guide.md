# CudaOptFlow Upstream Map / CudaOptFlow 上游映射

This guide records the scoped OpenCV 5.0.0 contrib `cudaoptflow` declaration closure and its exact build prerequisite evidence. It does not claim CUDA support or repository-wide OpenCV parity.

本指南记录 OpenCV 5.0.0 contrib `cudaoptflow` 声明范围及其精确构建前提证据，不宣称本包支持 CUDA，也不宣称 repository-wide OpenCV parity。

## Scope / 范围

The map covers `opencv2/cudaoptflow.hpp` through the parser-backed public declaration closure. All callable declarations are intentionally omitted because the current package has no native CudaOptFlow wrapper or managed public owner.

映射通过 parser 覆盖 `opencv2/cudaoptflow.hpp` 的公共声明闭包。当前包没有 native CudaOptFlow wrapper 或 managed public owner，因此所有 callable 都明确标为 intentional omission。

The guard hashes the upstream module `CMakeLists.txt` as compatibility evidence. OpenCV disables the module on iOS/WinRT and when neither CUDA nor CUDA stubs are enabled. The module depends on `opencv_video`, `opencv_optflow`, `opencv_cudaarithm`, `opencv_cudawarping`, `opencv_cudaimgproc`, and `opencv_cudafeatures2d`, with optional `opencv_cudalegacy`; first-class CUDA builds link the CUDA runtime, and CUDA 10+ may download NVIDIA Optical Flow SDK 2.0 headers. `GpuMat`/`Stream` ownership, asynchronous execution, flow-field transport, algorithm state, and SDK provenance remain outside this package's contract.

专项 guard 将 upstream 模块 `CMakeLists.txt` 的 hash 作为兼容性证据。OpenCV 在 iOS/WinRT 上禁用该模块，并在 CUDA 与 CUDA stubs 均未启用时禁用。该模块依赖 `opencv_video`、`opencv_optflow`、`opencv_cudaarithm`、`opencv_cudawarping`、`opencv_cudaimgproc` 和 `opencv_cudafeatures2d`，可选依赖 `opencv_cudalegacy`；first-class CUDA 构建链接 CUDA runtime，CUDA 10+ 可能下载 NVIDIA Optical Flow SDK 2.0 headers。`GpuMat`/`Stream` 生命周期、异步执行、flow field 传输、算法状态和 SDK 来源不属于本包契约。

## Verification / 验证

```powershell
pwsh -NoProfile -File .\scripts\Test-CudaOptFlowUpstreamMap.ps1
pwsh -NoProfile -File .\scripts\Test-UpstreamMapCatalog.ps1
```

The focused guard verifies parser closure, declaration partition, exact module build gate, CMake dependency/SDK evidence, absence of native/managed CudaOptFlow wrapper evidence, and the prohibition on repository-wide parity claims.

专项 guard 检查 parser closure、声明分区、模块构建门控、CMake 依赖/SDK 证据、native/managed CudaOptFlow wrapper 证据缺失，以及禁止 repository-wide parity 声明。

# CudaFeatures2D Upstream Map / CudaFeatures2D 上游映射

This guide records the scoped OpenCV 5.0.0 contrib `cudafeatures2d` declaration closure and its exact build prerequisite evidence. It does not claim CUDA support or repository-wide OpenCV parity.

本指南记录 OpenCV 5.0.0 contrib `cudafeatures2d` 声明范围及其精确构建前提证据，不宣称本包支持 CUDA，也不宣称 repository-wide OpenCV parity。

## Scope / 范围

The map covers `opencv2/cudafeatures2d.hpp`: 64 parser declarations, consisting of 58 callable declarations and six class metadata rows. All callables are intentionally omitted because the current package has no native CudaFeatures2D wrapper or managed public owner.

映射覆盖 `opencv2/cudafeatures2d.hpp`：64 项 parser 声明，包括 58 个 callable 和 6 个 class metadata。当前包没有 native CudaFeatures2D wrapper 或 managed public owner，因此所有 callable 都明确标为 intentional omission。

The guard hashes the upstream module `CMakeLists.txt` as compatibility evidence. OpenCV disables the module on iOS/WinRT and when neither CUDA nor CUDA stubs are enabled. The module depends on `opencv_features`, `opencv_cudafilters`, and `opencv_cudawarping`; first-class CUDA builds link the CUDA runtime. `GpuMat`/`Stream` ownership, asynchronous execution, keypoint/descriptor transport, and matcher state remain outside this package's contract.

专项 guard 将 upstream 模块 `CMakeLists.txt` 的 hash 作为兼容性证据。OpenCV 在 iOS/WinRT 上禁用该模块，并在 CUDA 与 CUDA stubs 均未启用时禁用。该模块依赖 `opencv_features`、`opencv_cudafilters` 和 `opencv_cudawarping`；first-class CUDA 构建链接 CUDA runtime。`GpuMat`/`Stream` 生命周期、异步执行、关键点/描述子传输和 matcher 状态均不属于本包契约。

## Verification / 验证

```powershell
pwsh -NoProfile -File .\scripts\Test-CudaFeatures2DUpstreamMap.ps1
pwsh -NoProfile -File .\scripts\Test-UpstreamMapCatalog.ps1
```

The focused guard verifies the parser closure, declaration partition, exact module build gate, CMake dependency evidence, absence of native/managed CudaFeatures2D wrapper evidence, and the prohibition on repository-wide parity claims.

专项 guard 检查 parser closure、声明分区、模块构建门控、CMake 依赖证据、native/managed CudaFeatures2D wrapper 证据缺失，以及禁止 repository-wide parity 声明。

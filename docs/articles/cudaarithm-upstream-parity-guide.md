# CudaArithm Upstream Map / CudaArithm 上游映射

This guide records the scoped OpenCV 5.0.0 contrib `cudaarithm` declaration closure and the exact build prerequisite evidence. It does not claim CUDA support or repository-wide OpenCV parity.

本指南记录 OpenCV 5.0.0 contrib `cudaarithm` 声明范围及其精确构建前提证据，不宣称本包支持 CUDA，也不宣称 repository-wide OpenCV parity。

## Scope / 范围

The map covers `opencv2/cudaarithm.hpp`: 89 parser declarations, consisting of 86 callable declarations and three class metadata rows (`LookUpTable`, `DFT`, and `Convolution`). All callables are intentionally omitted because the current package has no native CudaArithm wrapper or managed public owner.

映射覆盖 `opencv2/cudaarithm.hpp`：89 项 parser 声明，包括 86 个 callable 和三个 class metadata（`LookUpTable`、`DFT`、`Convolution`）。当前包没有 native CudaArithm wrapper 或 managed public owner，因此所有 callable 都明确标为 intentional omission。

The guard hashes the upstream module `CMakeLists.txt` as compatibility evidence. OpenCV disables the module on iOS/WinRT and when neither CUDA nor CUDA stubs are enabled. Real acceleration depends on CUDA and NPP; the module may additionally use `opencv_cudev`, cuBLAS/cuBLASLt, and cuFFT. `GpuMat` and `Stream` ownership, asynchronous execution, and runtime availability remain outside this package's contract.

专项 guard 将 upstream 模块 `CMakeLists.txt` 的 hash 作为兼容性证据。OpenCV 在 iOS/WinRT 上禁用该模块，并在 CUDA 与 CUDA stubs 均未启用时禁用。真实加速依赖 CUDA 与 NPP；模块还可能使用 `opencv_cudev`、cuBLAS/cuBLASLt 和 cuFFT。`GpuMat`/`Stream` 生命周期、异步执行和 runtime 可用性均不属于本包契约。

## Verification / 验证

```powershell
pwsh -NoProfile -File .\scripts\Test-CudaArithmUpstreamMap.ps1
pwsh -NoProfile -File .\scripts\Test-UpstreamMapCatalog.ps1
```

The focused guard verifies the parser closure, declaration partition, exact module build gate, CMake dependency evidence, absence of native/managed CudaArithm wrapper evidence, and the prohibition on repository-wide parity claims.

专项 guard 检查 parser closure、声明分区、模块构建门控、CMake 依赖证据、native/managed CudaArithm wrapper 证据缺失，以及禁止 repository-wide parity 声明。

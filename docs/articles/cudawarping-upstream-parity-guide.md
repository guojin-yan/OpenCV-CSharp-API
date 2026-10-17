# CudaWarping Upstream Map / CudaWarping 上游映射

This guide records the scoped OpenCV 5.0.0 contrib `cudawarping` declaration closure and its exact build prerequisite evidence. It does not claim CUDA support or repository-wide OpenCV parity.

本指南记录 OpenCV 5.0.0 contrib `cudawarping` 声明范围及其精确构建前提证据，不宣称本包支持 CUDA，也不宣称 repository-wide OpenCV parity。

## Scope / 范围

The map covers `opencv2/cudawarping.hpp`: 13 parser declarations, all callable functions in `cv.cuda`; there are no class or enum metadata rows. All callables are intentionally omitted because the current package has no native CudaWarping wrapper or managed public owner.

映射覆盖 `opencv2/cudawarping.hpp`：13 项 parser 声明，均为 `cv.cuda` 中的 callable function；没有 class 或 enum metadata。当前包没有 native CudaWarping wrapper 或 managed public owner，因此所有 callable 都明确标为 intentional omission。

The guard hashes upstream `modules/cudawarping/CMakeLists.txt` as compatibility evidence. OpenCV disables the module on iOS/WinRT and when neither CUDA nor CUDA stubs are enabled. The module requires `opencv_core`, `opencv_imgproc`, and `opencv_geometry`, optionally uses `opencv_cudev`, and first-class CUDA-language builds link CUDA runtime and NPP image-warping libraries. `GpuMat`/`Stream` ownership, asynchronous execution, map/output lifetime, and actual CUDA runtime availability remain outside this package's contract.

专项 guard 将 upstream 模块 `CMakeLists.txt` 的 hash 作为兼容性证据。OpenCV 在 iOS/WinRT 上禁用该模块，并在 CUDA 与 CUDA stubs 均未启用时禁用。该模块依赖 `opencv_core`、`opencv_imgproc` 和 `opencv_geometry`，可选使用 `opencv_cudev`；first-class CUDA-language 构建会链接 CUDA runtime 与 NPP image-warping 库。`GpuMat`/`Stream` 生命周期、异步执行、map/output 所有权及实际 CUDA runtime 可用性均不属于本包契约。

## Verification / 验证

```powershell
pwsh -NoProfile -File .\scripts\Test-CudaWarpingUpstreamMap.ps1
pwsh -NoProfile -File .\scripts\Test-UpstreamMapCatalog.ps1
```

The focused guard verifies the parser closure, declaration partition, exact module build gate, CMake dependency evidence, absence of native/managed CudaWarping wrapper evidence, and the prohibition on repository-wide parity claims.

专项 guard 检查 parser closure、声明分区、模块构建门控、CMake 依赖证据、native/managed CudaWarping wrapper 证据缺失，以及禁止 repository-wide parity 声明。

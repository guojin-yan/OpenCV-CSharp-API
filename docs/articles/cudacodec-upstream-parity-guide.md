# CudaCodec Upstream Map / CudaCodec 上游映射

This guide records the scoped OpenCV 5.0.0 contrib `cudacodec` declaration closure and its exact build prerequisite evidence. It does not claim CUDA codec support or repository-wide OpenCV parity.

本指南记录 OpenCV 5.0.0 contrib `cudacodec` 声明范围及其精确构建前提证据，不宣称本包支持 CUDA codec，也不宣称 repository-wide OpenCV parity。

## Scope / 范围

The map covers `opencv2/cudacodec.hpp`: 45 parser declarations, consisting of 23 callable declarations, nine class metadata rows, and 13 enum metadata rows. All callables are intentionally omitted because the current package has no native CudaCodec wrapper or managed public owner.

映射覆盖 `opencv2/cudacodec.hpp`：45 项 parser 声明，包括 23 个 callable、9 个 class metadata 和 13 个 enum metadata。当前包没有 native CudaCodec wrapper 或 managed public owner，因此所有 callable 都明确标为 intentional omission。

The guard hashes the upstream module `CMakeLists.txt` as compatibility evidence. OpenCV disables the module on iOS, Apple, and WinRT, and when neither CUDA nor CUDA stubs are enabled. The module requires `opencv_core`, `opencv_videoio`, `opencv_cudaarithm`, and `opencv_cudawarping`, optionally uses `opencv_cudev` and `opencv_cudaimgproc`, and links CUDA driver/runtime plus optional NVIDIA Video Codec SDK libraries (`nvcuvid`, `nvencodeapi`/`nvidia-encode`). `VideoReader`/`VideoWriter`, callback, `GpuMat`/`Stream`, asynchronous execution, and codec ownership remain outside this package's contract.

专项 guard 将 upstream 模块 `CMakeLists.txt` 的 hash 作为兼容性证据。OpenCV 在 iOS、Apple 和 WinRT 上禁用该模块，并在 CUDA 与 CUDA stubs 均未启用时禁用。该模块依赖 `opencv_core`、`opencv_videoio`、`opencv_cudaarithm` 和 `opencv_cudawarping`，可选使用 `opencv_cudev` 与 `opencv_cudaimgproc`，并链接 CUDA driver/runtime 及可选 NVIDIA Video Codec SDK 库（`nvcuvid`、`nvencodeapi`/`nvidia-encode`）。`VideoReader`/`VideoWriter`、callback、`GpuMat`/`Stream` 生命周期、异步执行和 codec ownership 均不属于本包契约。

## Verification / 验证

```powershell
pwsh -NoProfile -File .\scripts\Test-CudaCodecUpstreamMap.ps1
pwsh -NoProfile -File .\scripts\Test-UpstreamMapCatalog.ps1
```

The focused guard verifies the parser closure, declaration partition, exact module build gate, CMake dependency evidence, absence of native/managed CudaCodec wrapper evidence, and the prohibition on repository-wide parity claims.

专项 guard 检查 parser closure、声明分区、模块构建门控、CMake 依赖证据、native/managed CudaCodec wrapper 证据缺失，以及禁止 repository-wide parity 声明。

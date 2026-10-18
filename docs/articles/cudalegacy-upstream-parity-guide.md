# CudaLegacy upstream map / CudaLegacy 上游映射

This evidence map scopes the OpenCV 5.0.0 `opencv2/cudalegacy.hpp` compatibility umbrella and its seven parser-visible `opencv2/cudalegacy/*.hpp` public headers. The parser closure contains 221 unique declarations: 191 callable declarations, 25 class metadata declarations, and 5 enum metadata declarations.

本映射覆盖 OpenCV 5.0.0 的 `opencv2/cudalegacy.hpp` 兼容 umbrella 以及七个 parser 可见的 `opencv2/cudalegacy/*.hpp` public header。闭包共 221 个唯一声明：191 个 callable、25 个 class metadata、5 个 enum metadata。

All callable declarations are intentional omissions. The current package has no CudaLegacy native wrapper or managed owner, and the upstream module is disabled unless `HAVE_CUDA` is enabled. The CMake contract also records the legacy NCV/NPP staging and first-class `CUDA::cudart` dependency; this map does not claim CUDA support or repository-wide parity.

所有 callable 声明均为 intentional omission。当前包没有 CudaLegacy native wrapper 或 managed owner，上游模块只有在启用 `HAVE_CUDA` 时才构建。CMake 证据同时记录 legacy NCV/NPP staging 与 first-class `CUDA::cudart` 依赖；本映射不宣称 CUDA 支持或 repository-wide parity。

Run the deterministic contract check with:

```powershell
pwsh -NoProfile -File scripts/Test-CudaLegacyUpstreamMap.ps1
```

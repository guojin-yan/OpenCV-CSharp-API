# Julia Upstream Map / Julia 上游映射

This guide records the scoped OpenCV 5.0.0 contrib `julia` parser/header boundary. It is evidence for the public header closure, not a claim of repository-wide contrib parity.

本指南记录固定 OpenCV 5.0.0 contrib `julia` 的 parser/header 边界证据，不宣称整个 contrib 仓库已经 parity。

## Scope / 范围

The map covers `opencv-source/opencv_contrib-5.0.0/modules/julia/include/opencv2/julia.hpp`. The parser emits one callable declaration, `cv.julia.initJulia`, which initializes bindings for an external Julia/libcxxwrap-julia/CxxWrap environment. The current package has no Julia native wrapper or managed public owner, so the declaration is intentionally omitted and no ABI/API parity is inferred.

映射覆盖 `opencv-source/opencv_contrib-5.0.0/modules/julia/include/opencv2/julia.hpp`。parser 发出一个 callable 声明 `cv.julia.initJulia`，用于初始化外部 Julia/libcxxwrap-julia/CxxWrap 环境。当前包没有 Julia native wrapper 或 managed public owner，因此该声明明确 intentional omission，不推断 ABI/API parity。

## Verification / 验证

```powershell
pwsh -NoProfile -File .\scripts\Test-JuliaUpstreamMap.ps1
pwsh -NoProfile -File .\scripts\Test-UpstreamMapCatalog.ps1
```

The focused guard verifies the pinned header/include closure, the one intentionally omitted callable, zero native/managed evidence, the external Julia binding dependency, and the prohibition on repository-wide parity claims.

专项 guard 会检查固定 header/include closure、一个 intentional omission callable、native/managed 证据为零、外部 Julia binding 依赖，以及禁止 repository-wide parity 声明。

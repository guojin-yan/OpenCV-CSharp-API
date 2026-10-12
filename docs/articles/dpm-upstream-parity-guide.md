# DPM Upstream Map / DPM 上游映射

This guide records the scoped OpenCV 5.0.0 contrib `dpm` parser/header boundary. It is evidence for the public header closure, not a claim of repository-wide contrib parity.

本指南记录固定 OpenCV 5.0.0 contrib `dpm` 的 parser/header 边界证据，不宣称整个 contrib 仓库已经 parity。

## Scope / 范围

The map covers `opencv-source/opencv_contrib-5.0.0/modules/dpm/include/opencv2/dpm.hpp`. The parser emits two class declarations (`DPMDetector` and `DPMDetector.ObjectDetection`) and no callable declarations. The current package has no DPM native wrapper or managed public owner, so the class declarations remain non-callable metadata and no ABI/API parity is inferred.

映射覆盖 `opencv-source/opencv_contrib-5.0.0/modules/dpm/include/opencv2/dpm.hpp`。parser 发出两个 class 声明（`DPMDetector` 与 `DPMDetector.ObjectDetection`），没有 callable 声明。当前包没有 DPM native wrapper 或 managed public owner，因此 class 声明保留为 non-callable metadata，不推断 ABI/API parity。

## Verification / 验证

```powershell
pwsh -NoProfile -File .\scripts\Test-DpmUpstreamMap.ps1
pwsh -NoProfile -File .\scripts\Test-UpstreamMapCatalog.ps1
```

The focused guard verifies the pinned header/include closure, the two metadata declarations, zero callable/native/managed evidence, and the prohibition on repository-wide parity claims.

专项 guard 会检查固定 header/include closure、两个 metadata 声明、callable/native/managed 证据为零，以及禁止 repository-wide parity 声明。

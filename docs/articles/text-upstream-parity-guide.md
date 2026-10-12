# Text Upstream Map / Text 上游映射

This guide records the scoped OpenCV 5.0.0 contrib `text` parser/header boundary. It is evidence for the public header closure, not a claim of repository-wide contrib parity.

本指南记录固定 OpenCV 5.0.0 contrib `text` 的 parser/header 边界证据，不宣称整个 contrib 仓库已经 parity。

## Scope / 范围

The map covers `opencv2/text.hpp` and its parser-emitted `text/` closure: 47 declarations across ER filtering, OCR, SWT text detection, and CNN text detection. The closure contains 30 callables and 17 non-callable metadata declarations; all callables are intentionally omitted because the current package has no native Text wrapper or managed public owner. OCRTesseract is conditionally compiled against external Tesseract, and detector/OCR families require caller-selected classifier or model data.

映射覆盖 `opencv2/text.hpp` 及其 parser 发出的 `text/` closure：ER filtering、OCR、SWT text detection 与 CNN text detection 共 47 个声明，其中 30 个 callable、17 个 metadata 声明。当前包没有 native Text wrapper 或 managed public owner，因此所有 callable 均为 intentional omission；OCRTesseract 还依赖外部 Tesseract，检测与 OCR family 需要调用方选择 classifier/model 数据。

## Verification / 验证

```powershell
pwsh -NoProfile -File .\scripts\Test-TextUpstreamMap.ps1
pwsh -NoProfile -File .\scripts\Test-UpstreamMapCatalog.ps1
```

The focused guard verifies the pinned four-header closure, the 47-declaration partition, external Tesseract/model dependency rationale, zero native/managed evidence, and the prohibition on repository-wide parity claims.

专项 guard 会检查固定四个 header 的 closure、47 个声明的 partition、外部 Tesseract/model 依赖理由、native/managed 证据为零，以及禁止 repository-wide parity 声明。

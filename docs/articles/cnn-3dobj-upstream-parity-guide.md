# Cnn3DObj Upstream Map / Cnn3DObj 上游映射

This guide records the scoped OpenCV 5.0.0 contrib `cnn_3dobj` evidence map. It is a parser/header boundary record, not a claim of repository-wide contrib parity.

本指南记录固定 OpenCV 5.0.0 contrib `cnn_3dobj` 的 parser/header 证据范围，不宣称整个 contrib 仓库已经 parity。

## Scope / 范围

The map covers the parser-emitted declarations from `opencv-source/opencv_contrib-5.0.0/modules/cnn_3dobj/include/opencv2/cnn_3dobj.hpp`: two class metadata rows (`icoSphere` and `descriptorExtractor`) and five callable rows from the `icoSphere` surface.

映射覆盖 `opencv-source/opencv_contrib-5.0.0/modules/cnn_3dobj/include/opencv2/cnn_3dobj.hpp` 的 parser 声明：两个类元数据行（`icoSphere`、`descriptorExtractor`）和 `icoSphere` 的五个 callable 行。

All five callable rows are `intentionally-omitted`. The module requires an external Caffe triplet build and caller-selected network/mean/model files; this repository has no Cnn3DObj native ABI entrypoint or managed public owner, so it does not infer a wrapper or model execution contract.

五个 callable 行全部为 `intentionally-omitted`。该模块依赖外部 Caffe triplet 构建以及调用方提供的 network/mean/model 文件；本仓库没有 Cnn3DObj native ABI entrypoint 或 managed owner，因此不推断 wrapper 或模型执行契约。

## Verification / 验证

```powershell
pwsh -NoProfile -File .\scripts\Test-Cnn3DObjUpstreamMap.ps1
pwsh -NoProfile -File .\scripts\Test-UpstreamMapCatalog.ps1
```

The focused guard verifies the pinned header/parser hashes, the 7/5/2 declaration partition, five intentional omissions, zero native/managed evidence, the external Caffe/model boundary, and the prohibition on repository-wide parity claims.

专项 guard 会检查固定 header/parser hash、7/5/2 声明分区、五个 intentional omission、native/managed 证据为零、外部 Caffe/model 边界，以及禁止 repository-wide parity 声明。

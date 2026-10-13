# DNN Object Detection Upstream Parity Guide

This guide records the scoped OpenCV 5.0.0 contrib `dnn_objdetect` evidence map. It is a source-scope and wrapper-boundary record, not a claim of repository-wide upstream parity.

## Scope

OpenCV 5.0.0 has no `dnn_objdetect.hpp` umbrella header; the map therefore covers the public `opencv2/core_detect.hpp` closure. The official parser cannot consume the anonymous `typedef struct object` spelling in this header, so the extractor uses a temporary parser-only normalization, records `parserMode=non-wrapper-public-declarations` and the normalization description, and hashes the untouched upstream header. The resulting public inventory is two class metadata rows (`object` and `InferBbox`) plus two callable rows (`InferBbox` construction and `filter`), with the eight public data fields retained in class metadata. Protected prediction/NMS helpers are intentionally excluded from the public map.

Both callable rows are intentionally omitted. The current package has no DnnObjDetect native ABI entrypoint or managed public owner; caller-selected network/model data, backend policy, and detection output ownership remain outside the current ABI/API and deterministic smoke contract. The map reports zero missing and zero native/managed evidence.

## Dependency and ownership boundary

The module is model-driven. Its documented examples require caller-selected DNN model files and network outputs; no model assets or managed/native DnnObjDetect wrapper are bundled. This dependency is recorded in `compatibility/dnn_objdetect-upstream-classifications.json`.

## Verification

Run the focused guard:

```powershell
pwsh -NoProfile -File .\scripts\Test-DnnObjDetectUpstreamMap.ps1
```

The guard verifies the pinned source/parser hashes, parser normalization marker, four-declaration public inventory, eight public data fields, exclusion of protected helpers, two intentional omissions, zero missing rows, zero native/managed evidence, the model dependency note, and the prohibition on repository-wide parity claims.

中文说明：`dnn_objdetect` 在 OpenCV 5.0.0 中没有独立 umbrella header，因此映射固定到 `core_detect.hpp`。由于官方 parser 无法直接处理匿名 `typedef struct object`，提取器只在临时副本中做 parser 兼容归一化，并保留原始头文件哈希；公共范围包含 `object`、`InferBbox`、构造函数和 `filter`，受保护的预测/NMS helper 不计入公共映射。该模块依赖调用方模型数据，当前包没有 native/managed owner，两个 callable 均明确标记为 intentionally-omitted。

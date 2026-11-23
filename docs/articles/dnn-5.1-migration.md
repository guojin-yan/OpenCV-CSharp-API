# DNN compatibility with OpenCV 5.1

The 5.1 runtime keeps the published managed enum values and native ABI 1. OpenCV changed its internal enum values, so the wrapper translates them at every model-reader boundary.

5.1 运行时保留既有 managed 枚举数值与 native ABI 1；上游枚举发生变化时，在原生边界转换。

| Managed selection | ABI value | OpenCV 5.1 request |
| --- | --- | --- |
| `DnnEngine.Auto` | 3 | `ENGINE_AUTO` (0) |
| `DnnEngine.Classic` | 1 | `ENGINE_OPENCV` (1) |
| `DnnEngine.New` | 2 | `ENGINE_OPENCV` (1) |
| `DnnEngine.Ort` | 4 | `ENGINE_ORT` (2) |

The engine translation covers generic readers and ONNX/TFLite path and buffer readers. OpenCV decides whether ORT is available and may fall back to its own engine. A successful import on a build without ORT does not verify the ORT backend. Native smoke checks all projections independently of this fallback, and compile-time assertions bind them to the linked upstream enum definitions. OpenCV 5.0 builds retain their original values.

`Net.ModelFormat` translates the upstream 5.1 TFLite value 2 to the existing `DnnModelFormat.TensorFlowLite` value 3. `TensorFlow` remains value 2 for binary compatibility; its importer was removed upstream and the retained overloads report an explicit exception. Generic and ONNX values remain 0 and 1.

`New` 与 `Classic` 均映射到 5.1 统一 OpenCV engine，`Ort` 保留显式 ORT 请求；上游可因构建配置回退。TFLite 格式由上游的 2 转换到既有公开值 3，避免误报为 TensorFlow。路径、数组、Span 和通用 reader 均有真实推理回归；408 字节的合成 TFLite fixture 执行 `output = input + 0`，不依赖外部权重或 TensorFlow。

## Source mapping

The versioned [5.1 raw map](https://github.com/guojin-yan/OpenCV-CSharp-API/blob/opencv5.x/compatibility/opencv-5.1.0/dnn-upstream-raw.json) uses the official OpenCV commit `e4f78fbcf1a773fbed69e14500e8b734963eb585` and its own parser. It covers the same three-header parser slice as the historical DNN map: `dict.hpp`, `dnn.hpp`, and `utils/inference_engine.hpp`, with `CV_VERSION_MAJOR=5` and `OPENCV_ABI_COMPATIBILITY=500`.

There are 185 declarations: 161 callables, 15 classes and nine enums. Classifications are 68 implemented, 85 intentionally omitted, two unsupported, six upstream conditional, 24 metadata, and zero unexplained missing. Twelve identities were added, nine removed, and 173 retained; 18 retained identities have changed properties, qualifiers or documentation. The delta retains the exact before/after declarations. Historical artifacts remain bound to OpenCV 5.0.0.

This is a parser slice, not complete DNN C++ coverage. It does not enumerate the complete `layer.hpp`/`dnn.inl.hpp`/template surface, establish optional backend availability or certify all 73 historical maps against 5.1.

5.1 映射单独保存完整声明差异，按精确 identity 关联旧证据，不沿用发生偏移的 ordinal。`Layer.outputNameToIndex` 移到 `LayerInfo` 基类后，现有 owned Layer wrapper 继续调用继承的方法。新增的非默认插值参数/属性、TF32、显式 KV-cache 容量预留、ONNX 外部数据 mmap 和多段 Tokenizer 均有具体省略理由。现有预处理保持 `INTER_LINEAR`；ONNX PBTXT 导出仍在触及上游 exporter 前明确拒绝，以保留已加载 graph。

Regenerate with an explicit Python interpreter:

```powershell
pwsh -NoProfile -File scripts/Generate-Dnn51UpstreamMap.ps1 -RegenerateRaw -InitializeClassification -PythonPath <python>
pwsh -NoProfile -File scripts/Test-Dnn51UpstreamMap.ps1
```

Routine semantic checks use .NET 10 and the pinned source files; they require no Python, TensorFlow or ORT download. Source hashes normalize CRLF to official LF without trimming, so Windows checkouts and downloaded source files agree. Twenty-two negative fixtures reject changed identities, source scope/hashes, lost class properties/classifications, false evidence, unsupported feature promotion and incomplete deltas.

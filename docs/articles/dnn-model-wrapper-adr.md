# DNN Model Wrapper ADR / DNN 模型 Wrapper ADR

## Status

Research and preview design for 5.0.1. The stable surface remains `Net`, `Mat`, named I/O, shape helpers, and independently owned forward outputs. Classification, detection, and segmentation wrappers are not stable public APIs yet.

## Model provenance

A wrapper instance must carry or receive model provenance: framework, model byte hash, optional config byte hash, license/source identity, input names, output names, preprocessing revision, and expected output schema. The library must not download model files, silently choose preprocessing, or infer a license from a filename.

模型 wrapper 必须携带或接收 provenance：framework、model 字节 hash、可选 config 字节 hash、license/source identity、input/output 名称、预处理版本和期望输出 schema。库不能下载模型、静默选择预处理，或从文件名推断许可证。

## Ownership and batching

- The wrapper owns the `Net` and disposes it exactly once.
- Input `Mat` values are borrowed for each call. Outputs are newly owned `Mat` or managed result objects and remain valid after the call until disposed.
- Batch inputs must be explicit. A wrapper cannot silently resize, reorder, normalize, or clone inputs without recording that policy.
- Partial output conversion releases every native and managed result already created.
- Async inference is outside this wrapper ADR until native cancellation and backend threading are separately closed.

## Shape and output contracts

Each wrapper declares accepted rank, dimensions, channel order, depth, batch range, and dynamic dimensions. Runtime output validation checks names, count, rank, dimensions, and numeric type before constructing result objects. Unknown or backend-dependent output shapes remain a structured error; they are not converted to empty detections.

Classification results carry labels, scores, and class-index policy. Detection results carry boxes, scores, class ids, and NMS policy. Segmentation results carry the mask owner, class mapping, and resize/interpolation policy. These result types must not share a loose dictionary shape.

## Backend and model limits

`OpenCvCapabilities` backend/target probes are diagnostic inputs only. A wrapper may select a backend/target, but model execution evidence must use fixed model bytes and record the selected runtime, backend, target, output schema, and failure reason. CUDA/OpenCL/NPU claims remain separate from model-wrapper success.

## Promotion gate

A wrapper can move from preview to stable only after a deterministic fixture, model provenance record, input/output ownership tests, malformed output tests, batch/shape validation, at least one CPU execution path, and documented backend limitations are complete. Accuracy is an application/model evaluation concern and is not inferred from a successful native call. Until then, consumers should use `Net` directly and own preprocessing/postprocessing.

wrapper 在 deterministic fixture、model provenance、输入输出 ownership、异常输出、batch/shape validation、至少一个 CPU 执行路径和 backend 限制文档完成前，不得从 preview 晋升 stable。准确率属于应用和模型评估，不能从 native 调用成功推断。此之前消费者应直接使用 `Net` 并自行负责预处理/后处理。

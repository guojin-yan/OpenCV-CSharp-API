# DNN Quantization And Backend Diagnostics ADR / DNN 量化与 Backend 诊断 ADR

## Status

Research and preview design for 5.0.1. `OpenCvCapabilities.DnnBackends` remains the current backend/target enumeration surface. It does not claim model execution, layer compatibility, or quantized arithmetic support.

## Diagnostic layers

Diagnostics are separated into four layers:

1. **Declared**: backend/target names, model metadata, or quantization fields present in the model graph.
2. **Available**: the linked runtime reports a backend/target or accepts a metadata query.
3. **Verified**: a fixed model fixture executes a deterministic probe on that backend/target and returns validated outputs.
4. **Model failure**: a layer, tensor, shape, dtype, scale, zero-point, or target rejected during load/finalization/execution, with a structured reason and layer identity when OpenCV exposes one.

`DnnTarget.Cuda`, `OpenCL`, `Cann`, `TimVx`, and similar names remain target requests. They are not evidence of quantized or accelerated execution.

## Quantization facts

A future report may expose per-tensor or per-layer dtype, scale, zero-point, signedness, and axis only when the model/runtime can prove the fact. Unknown fields remain unknown; the report must not invent a default scale or zero-point. Quantization metadata is separate from the final `Mat` depth because OpenCV may convert tensors during preprocessing or output retrieval.

For integer input, the report must state whether scale and zero-point were supplied by the model, by preprocessing parameters, or were absent. Per-channel parameters require an axis and a length check. Mismatched scale/zero-point lengths fail closed before a wrapper result is constructed.

## Backend and unsupported-layer diagnostics

- Backend selection and target selection are recorded independently.
- Layer diagnostics include layer name/id when available, operation type when available, stage (`load`, `finalize`, `forward`), and a non-sensitive reason.
- A backend that returns a target list but rejects the model is `Available` plus a model failure; it is not `Verified`.
- A CPU fallback is reported separately from the requested backend. Silent fallback cannot be represented as successful GPU/NPU verification.
- Diagnostics must not parse unstable `getBuildInformation()` text as the sole source. Native errors may be preserved as bounded diagnostic text, but paths, environment variables, and model bytes are excluded from the default report.

## Ownership and fixture rules

Reports own copied strings and numeric arrays. They never retain a `Net`, `Mat`, native pointer, or model buffer. Deterministic fixtures must pin model bytes, license/source, preprocessing parameters, expected output shape, and output hash. A successful identity model only proves the wrapper path; it does not prove arbitrary quantized operator coverage.

## Promotion gate

A stable quantization/backend diagnostic API requires one CPU fixture, one backend failure fixture, one known quantized fixture with scale/zero-point facts, malformed metadata rejection, output ownership tests, and evidence on every supported modern TFM. Until then, keep this surface as report design and internal/preview helpers; direct `Net` and `OpenCvCapabilities` remain the supported APIs.

稳定量化/backend 诊断 API 需要 CPU fixture、backend failure fixture、带 scale/zero-point 事实的量化 fixture、malformed metadata 拒绝、输出 ownership 测试，以及所有支持现代 TFM 的证据。在此之前保持 report design 和 internal/preview helper；支持 API 仍是 `Net` 与 `OpenCvCapabilities`。

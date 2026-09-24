# DNN Named I/O And Dynamic Shape ADR / DNN 命名 I/O 与动态 Shape ADR

## Status

Accepted for the 5.0.1 P1 preview boundary. The existing `Net.SetInput`, `SetInputsNames`, `SetInputShape`, `Forward(string)`, `Forward(string[])`, and `ForwardAndRetrieve` methods remain the implementation baseline. This ADR does not add a model wrapper or claim backend execution support.

## Owner model

- `Net` owns its native network handle and is disposable.
- `SetInput` borrows the caller's `Mat` only for the native call; the network does not retain the managed wrapper as an owner contract.
- `Forward(string)` returns one independently owned `Mat`; `Forward(string[])` returns one independently owned `Mat` per requested name; `ForwardAndRetrieve` returns independently owned nested groups. Every returned matrix remains valid after the network call and must be disposed by the caller.
- A failed multi-output or nested retrieval releases every native handle already produced. Managed conversion failures release both converted and still-native handles.

## Named input and output rules

1. Names are UTF-8, reject embedded nulls, and preserve caller order in packed buffers.
2. Empty output names are allowed only for the single-output `Forward(string)` default path. A multi-output name array must be non-null, non-empty, and contain no null elements.
3. `SetInputsNames` accepts an empty array as an explicit reset because OpenCV owns that operation's semantics; null elements remain invalid.
4. Output arrays preserve the requested order. Native count/fill mismatches are errors and cannot be silently truncated.
5. A backend target enum or a layer name does not prove model execution. Model bytes, framework, preprocessing, and output shape remain application-owned evidence.

## Dynamic shape rules

- `SetInputShape` validates a non-null, non-empty shape with at most ten dimensions. Each dimension must be positive or the documented OpenCV dynamic sentinel; zero is allowed only where OpenCV's loader explicitly defines it as dynamic.
- The managed layer does not invent a replacement shape for an unresolved dynamic dimension. Shape inference methods return the values reported by OpenCV, including dynamic or empty values when the native runtime cannot resolve them.
- `SetInputShape` is graph/network mutation and must occur before the affected forward call. It is not thread-safe with concurrent inference on the same `Net`.
- `GetLayerShapes` and FLOPS helpers copy returned shape arrays into independently owned managed arrays. They do not retain caller arrays or native pointers.
- A shape/type count mismatch, negative dimension, excessive dimensionality, or native count/fill change fails before returning a partial result.

## Error and rollback contract

Managed null, empty-name, array-length, shape, type, and layer-id validation runs before native entry. Native backend, unsupported-layer, and `NOT_LINKED` failures remain `OpenCvException`/`NativeException` facts; they are not converted to a generic shape error. On any failure after native allocation begins, all partially created output handles are released.

托管层的 null、空名称、数组长度、shape、type 和 layer id 校验在进入 native 前执行。native backend、unsupported layer 和 `NOT_LINKED` 失败继续作为 `OpenCvException`/`NativeException` 事实返回，不伪装成通用 shape 错误。native 分配开始后发生失败时，所有已创建的部分输出句柄都必须释放。

## Fixture and promotion boundary

The deterministic identity ONNX fixture may prove named I/O, owned output, and shape validation. It does not prove CUDA/OpenCL/NPU execution, arbitrary model compatibility, dynamic-shape support for every framework, or production accuracy. A stable model wrapper requires fixed model bytes, license/source/hash, preprocessing metadata, output ownership, batch policy, and backend-specific evidence; until then the current Net methods remain the preview surface.

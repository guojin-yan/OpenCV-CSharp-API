# DNN Structured Parity Guide

This guide covers the parser-selected OpenCV 5.0.0 DNN network, layer, model-loading, preprocessing, shape, memory, and profiling families exposed by `JYPPX.OpenCvSharp.Dnn`. The measured scope is module-specific and does not claim repository-wide OpenCV C++ parity.

本指南说明 `JYPPX.OpenCvSharp.Dnn` 对 OpenCV 5.0.0 DNN 网络、层、模型加载、预处理、形状、内存和性能分析接口的结构化绑定。该范围仅针对已测量的 DNN 模块，不代表整个 OpenCV C++ API 已全部覆盖。

## Model Loading / 模型加载

`Net` supports path and in-memory loading for ONNX, TensorFlow, TensorFlow Lite, OpenVINO Model Optimizer, and the general OpenCV `ReadNet` dispatcher. `byte[]` overloads are available on every target framework; `ReadOnlySpan<byte>` overloads are available on modern targets. Model buffers are pinned only for the duration of the native call and are never retained.

`Net` 支持从路径或内存加载 ONNX、TensorFlow、TensorFlow Lite、OpenVINO Model Optimizer，并提供 OpenCV 通用 `ReadNet` 分派入口。所有目标框架都有 `byte[]` 重载，现代目标框架还提供 `ReadOnlySpan<byte>` 重载。模型缓冲区只在 native 调用期间固定，调用结束后不会被保留。

Paths and names use strict UTF-8. Null strings, embedded null characters, and invalid UTF-16 are rejected before the native call. On Windows, the native boundary converts UTF-8 paths without replacement characters.

路径和名称使用严格 UTF-8。空引用、内嵌空字符和非法 UTF-16 会在 native 调用前被拒绝。Windows native 边界转换 UTF-8 路径时不允许替换字符。

`DnnEngine.Auto` is OpenCV 5 value `3`; `Classic`, `New`, and optional `Ort` are `1`, `2`, and `4`. The engine is a semantic choice, not merely metadata. Classic supports forwarding to named intermediate layers. New is currently CPU-oriented and may reject that operation. KV cache is restricted to a New-engine graph because OpenCV's cache manager requires `mainGraph`; the wrapper returns a managed `OpenCvException` for Classic instead of allowing an upstream null dereference.

`DnnEngine.Auto` 对应 OpenCV 5 的值 `3`；`Classic`、`New` 和可选的 `Ort` 分别为 `1`、`2`、`4`。engine 会改变实际行为，而不仅是元数据。Classic 支持 forward 到命名中间层；New 当前主要面向 CPU，并可能拒绝该操作。KV cache 仅适用于具有 `mainGraph` 的 New engine；Classic 调用会得到托管 `OpenCvException`，不会进入 OpenCV 上游的空指针路径。

## Preprocessing / 预处理

`Image2BlobParams` records per-channel scale, output size, mean, RGB swap, output depth, NCHW/NHWC layout, crop or letterbox mode, and border value. Output depth is `CV_32F` or `CV_8U`; `CV_8U` requires unit scale and zero mean. Input images must follow OpenCV's one-, three-, or four-channel rules.

`Image2BlobParams` 保存逐通道 scale、输出尺寸、mean、RGB 交换、输出深度、NCHW/NHWC 布局、裁剪或 letterbox 模式以及边界值。输出深度只能是 `CV_32F` 或 `CV_8U`；`CV_8U` 要求单位 scale 和零 mean。输入图像必须符合 OpenCV 的 1、3 或 4 通道规则。

`BlobRectToImageRect` and `BlobRectsToImageRects` use the configured blob size to project coordinates back to the original image. OpenCV 5 only performs the conversion branch when blob size differs from image size; callers should not use a same-size call to infer an identity transform.

`BlobRectToImageRect` 和 `BlobRectsToImageRects` 使用参数中的 blob size 将坐标映射回原图。OpenCV 5 只有在 blob size 与图像尺寸不同时才进入转换分支，因此不能用同尺寸调用推断恒等映射。

## Network Workflow / 网络流程

```csharp
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Dnn;
using DnnCv2 = JYPPX.OpenCvSharp.Dnn.Cv2;

byte[] model = File.ReadAllBytes("model.onnx");
using Net net = Net.ReadNetFromOnnx(model, DnnEngine.Classic);
using Mat image = new Mat(224, 224, MatType.CV_8UC3);
using Mat blob = DnnCv2.BlobFromImage(
    image,
    new Image2BlobParams(
        new Scalar(1.0 / 255.0),
        new Size(224, 224),
        swapRB: true));

net.SetPreferableBackend(DnnBackend.OpenCV)
   .SetPreferableTarget(DnnTarget.Cpu)
   .SetProfilingMode(DnnProfilingMode.Detailed)
   .SetInput(blob)
   .FinalizeNetwork();

string[] outputNames = net.GetUnconnectedOutLayersNames();
using Mat output = net.Forward(outputNames[0]);
Console.WriteLine($"dims={output.Dims}, values={output.Total}");
```

`FinalizeNetwork` pays backend setup cost at a predictable point and reports configuration failures before the first forward. If it is omitted, OpenCV finalizes lazily.

`FinalizeNetwork` 可在可预测的位置完成 backend 初始化，并在第一次 forward 前报告配置失败；省略时 OpenCV 会延迟初始化。

## Ownership / 所有权

- `Net` owns one opaque native handle and uses `SafeHandle` dangerous references around every native call. Repeated disposal is safe; calls after disposal throw `ObjectDisposedException`.
- `Layer` owns an independent ref-counted `cv::Ptr<Layer>` wrapper. It can outlive the parent `Net` and must be disposed separately.
- `Forward`, `Forward(string[])`, `ForwardAndRetrieve`, `GetParam`, and `ImagesFromBlob` return independently owned `Mat` wrappers. Dispose every returned Mat, including every element in nested results.
- Input Mats and parameter Mats are caller-owned. The native call does not retain a managed interior pointer.
- `Dump` returns an owned UTF-8 result handle that is copied and released before returning the managed string.

- `Net` 拥有一个 opaque native handle，每次 native 调用期间通过 `SafeHandle` dangerous reference 保持存活。重复释放安全；释放后的调用抛出 `ObjectDisposedException`。
- `Layer` 拥有独立引用计数的 `cv::Ptr<Layer>` 包装，可在父 `Net` 释放后继续存在，并需要单独释放。
- `Forward`、`Forward(string[])`、`ForwardAndRetrieve`、`GetParam` 和 `ImagesFromBlob` 返回独立拥有的 `Mat`。嵌套结果中的每个 Mat 都必须释放。
- 输入 Mat 和参数 Mat 由调用方拥有。native 调用不会保留托管内部指针。
- `Dump` 返回 owned UTF-8 result handle；托管层复制文本后立即释放该 handle。

## Shapes And Collections / 形状与集合

Multi-input shape APIs accept one shape and one OpenCV Mat type per network input. Shape/type counts must match. A shape has at most ten dimensions, matching OpenCV 5 `MatShape::MAX_DIMS`; negative dimensions remain available for dynamic-shape contracts. Packed offsets and values use checked arithmetic.

多输入 shape API 要求每个网络输入对应一个 shape 和一个 OpenCV Mat type，二者数量必须一致。每个 shape 最多十维，与 OpenCV 5 `MatShape::MAX_DIMS` 一致；负维度仍可用于动态 shape 契约。offset 和 value 打包使用 checked 算术。

Variable outputs use two-stage count/fill calls. Managed code requires exact count and byte-count agreement between stages. Native partial allocation failures release every already-created Mat, and managed conversion failures release both converted and still-native handles.

变长输出使用两阶段 count/fill。托管层要求两个阶段的元素数和字节数完全一致。native 部分分配失败会释放已创建的全部 Mat；托管转换失败也会释放已经转换和尚未转换的 handle。

## Profiling And Controls / 性能分析与控制

`GetPerfProfile` returns tick counts and numeric per-layer timings. `GetDetailedPerfProfile` returns three equal-length UTF-8 columns: names, formatted times, and invocation counts. Timing values depend on backend and host; tests should assert structure and relationships rather than exact duration.

`GetPerfProfile` 返回 tick 数和逐层数值耗时。`GetDetailedPerfProfile` 返回三列等长 UTF-8 数据：名称、格式化耗时和调用次数。耗时取决于 backend 与主机，测试应验证结构和关系，而不是固定时间。

Fusion, Winograd, tracing, and profiling are graph/backend controls. Backend availability must be queried with `Cv2.GetAvailableTargets`; enum existence does not prove a GPU or optional backend is present on the current machine.

Fusion、Winograd、tracing 和 profiling 都是图或 backend 控制项。应使用 `Cv2.GetAvailableTargets` 查询实际可用 target；枚举存在并不代表当前机器具有 GPU 或可选 backend。

## Deterministic Evidence / 确定性证据

Focused tests and the default ConsoleSamples workflow use `tests/OpenCvSharp.Tests/Dnn/Fixtures/identity-opset13.onnx.base64`. It is a 147-byte ONNX opset-13 Identity graph with input and output shape `[1,1,2,2]`. The decoded model SHA256 is `326793cdb2fc2da739a715c3f3ff71d09779b389ad29e56bbfccc4313e900744`. It is generated for this repository, requires no download, and produces `[1,2,3,4]` for the matching test input.

focused tests 与默认 ConsoleSamples 使用 `tests/OpenCvSharp.Tests/Dnn/Fixtures/identity-opset13.onnx.base64`。它是本仓库生成的 147 字节 ONNX opset-13 Identity 图，输入输出 shape 为 `[1,1,2,2]`，解码后 SHA256 为 `326793cdb2fc2da739a715c3f3ff71d09779b389ad29e56bbfccc4313e900744`。该模型无需下载，并对测试输入确定性输出 `[1,2,3,4]`。

The DNN module is full-profile only. Mini packages preserve compatibility entrypoint behavior but report `NOT_LINKED`; they are not DNN execution evidence.

DNN 仅属于 full profile。mini package 保留兼容入口行为，但返回 `NOT_LINKED`，不能作为 DNN 执行证据。

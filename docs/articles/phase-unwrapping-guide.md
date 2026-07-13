# Phase Unwrapping Guide / Phase Unwrapping 指南

`OpenCvSharp.PhaseUnwrapping` wraps the OpenCV contrib `phase_unwrapping` module through opaque native handles and caller-owned `Mat` inputs and outputs.

`OpenCvSharp.PhaseUnwrapping` 通过 opaque native handle 以及调用方持有的 `Mat` 输入/输出封装 OpenCV contrib `phase_unwrapping` 模块。

## Scope / 范围

- Parameters: `HistogramPhaseUnwrappingParams`.
- Factory helpers: `HistogramPhaseUnwrapping.Create` and `PhaseUnwrappingCv2.CreateHistogramPhaseUnwrapping`.
- Base operation: `PhaseUnwrappingObject.UnwrapPhaseMap`.
- Histogram output: `HistogramPhaseUnwrapping.GetInverseReliabilityMap`.

- 参数：`HistogramPhaseUnwrappingParams`。
- 工厂 helper：`HistogramPhaseUnwrapping.Create` 与 `PhaseUnwrappingCv2.CreateHistogramPhaseUnwrapping`。
- 基类操作：`PhaseUnwrappingObject.UnwrapPhaseMap`。
- 直方图输出：`HistogramPhaseUnwrapping.GetInverseReliabilityMap`。

## Runtime / 运行时

`phase_unwrapping` is an optional OpenCV contrib module. Runtime staging includes the factual OpenCV 5.0.0 runtime artifact `opencv_phase_unwrapping500.dll` when the module is built. If a runtime lacks it, the managed API shape remains stable and linked calls report `NOT_LINKED`.

`phase_unwrapping` 是可选 OpenCV contrib 模块。构建该模块时 runtime staging 会包含事实性 OpenCV 5.0.0 runtime 产物 `opencv_phase_unwrapping500.dll`。如果某个 runtime 缺少它，managed API 形状仍保持稳定，linked 调用会报告 `NOT_LINKED`。

## Input Notes / 输入说明

Histogram phase unwrapping expects wrapped and unwrapped phase maps as single-channel 32-bit floating-point matrices. The optional shadow mask stays caller-owned. `GetInverseReliabilityMap` reports the inverse reliability map computed by the previous unwrap call.

直方图相位展开期望包裹相位图和展开结果为单通道 32 位浮点矩阵。可选 shadow mask 仍由调用方持有。`GetInverseReliabilityMap` 返回上一次展开调用计算得到的反可靠性图。

```csharp
using OpenCvSharp.Core;
using OpenCvSharp.PhaseUnwrapping;

using Mat wrapped = new Mat(8, 8, MatType.CV_32FC1);
wrapped.CopyFrom(new float[64]);

using HistogramPhaseUnwrapping unwrapper = HistogramPhaseUnwrapping.Create(
    width: 8,
    height: 8,
    histThresh: HistogramPhaseUnwrappingParams.Default.HistThresh);

using Mat unwrapped = unwrapper.UnwrapPhaseMap(wrapped);
using Mat inverseReliability = unwrapper.GetInverseReliabilityMap();
```

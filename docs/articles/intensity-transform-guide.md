# Intensity Transform Guide / Intensity Transform 指南

`OpenCvSharp.IntensityTransform` wraps the first OpenCV contrib `intensity_transform` image-enhancement functions through caller-owned `Mat` inputs and outputs.

`OpenCvSharp.IntensityTransform` 通过调用方持有的 `Mat` 输入/输出封装第一批 OpenCV contrib `intensity_transform` 图像增强函数。

## Scope / 范围

- Logarithmic transform: `IntensityTransformCv2.LogTransform`.
- Gamma correction: `IntensityTransformCv2.GammaCorrection`.
- Autoscaling: `IntensityTransformCv2.Autoscaling`.
- Contrast stretching: `IntensityTransformCv2.ContrastStretching`.
- BIMEF low-light enhancement: `IntensityTransformCv2.Bimef` overloads with automatic or explicit exposure ratio.

- 对数变换：`IntensityTransformCv2.LogTransform`。
- Gamma 校正：`IntensityTransformCv2.GammaCorrection`。
- 自动缩放：`IntensityTransformCv2.Autoscaling`。
- 对比度拉伸：`IntensityTransformCv2.ContrastStretching`。
- BIMEF 低照度增强：`IntensityTransformCv2.Bimef`，包含自动或显式曝光比例重载。

## Runtime / 运行时

`intensity_transform` is an optional OpenCV contrib module. Runtime staging includes the factual OpenCV 5.0.0 runtime artifact `opencv_intensity_transform500.dll` when the module is built. If a runtime lacks it, the managed API shape remains stable and linked calls report `NOT_LINKED`.

`intensity_transform` 是可选 OpenCV contrib 模块。构建该模块时 runtime staging 会包含事实性 OpenCV 5.0.0 runtime 产物 `opencv_intensity_transform500.dll`。如果某个 runtime 缺少它，managed API 形状仍保持稳定，linked 调用会报告 `NOT_LINKED`。

BIMEF also depends on OpenCV being built with EIGEN support. A runtime can contain the factual OpenCV 5.0.0 runtime artifact `opencv_intensity_transform500.dll` while BIMEF still reports an Eigen-required OpenCV exception.

BIMEF 还依赖启用 EIGEN 的 OpenCV 构建。即使 runtime 包含事实性 OpenCV 5.0.0 runtime 产物 `opencv_intensity_transform500.dll`，BIMEF 仍可能返回需要 Eigen 的 OpenCV 异常。

## Input Notes / 输入说明

The basic transforms are useful on tiny generated grayscale matrices for smoke tests. BIMEF is intended for color low-light images. The wrappers validate null matrices, gamma positivity, byte-range contrast points, and finite BIMEF parameters before crossing the native boundary.

基础变换适合在 tiny 合成灰度矩阵上做 smoke。BIMEF 面向彩色低照度图像。封装层在进入 native 边界前验证空矩阵、gamma 正数、对比度点位字节范围，以及 BIMEF 参数是否有限。

```csharp
using OpenCvSharp.Core;
using OpenCvSharp.IntensityTransform;

using Mat gray = new Mat(8, 8, MatType.CV_8UC1, new Scalar(32));
using Mat log = IntensityTransformCv2.LogTransform(gray);
using Mat gamma = IntensityTransformCv2.GammaCorrection(gray, 1.2F);
using Mat scaled = IntensityTransformCv2.Autoscaling(gray);
using Mat stretched = IntensityTransformCv2.ContrastStretching(gray, 16, 0, 192, 255);

using Mat bgr = new Mat(8, 8, MatType.CV_8UC3, new Scalar(16, 32, 64));
using Mat enhanced = IntensityTransformCv2.Bimef(bgr, mu: 0.5F);
```

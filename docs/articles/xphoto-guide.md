# XPhoto Guide / XPhoto 指南

`JYPPX.OpenCvSharp.XPhoto` wraps the first optional contrib `xphoto` white balance and enhancement utilities.

`JYPPX.OpenCvSharp.XPhoto` 封装第一批可选 contrib `xphoto` 白平衡与增强工具。

## Scope / 范围

- White balancers: `WhiteBalancer`, `SimpleWB`, `GrayworldWB`, and `LearningBasedWB`.
- White-balance properties for input/output range, saturation threshold, histogram bins, and range maximum.
- Functions: `ApplyChannelGains`, `DctDenoising`, `Bm3dDenoising`, `Inpaint`, and `OilPainting`.
- Enums: `Bm3dSteps`, `InpaintTypes`, and `TransformTypes`.

- 白平衡器：`WhiteBalancer`、`SimpleWB`、`GrayworldWB` 和 `LearningBasedWB`。
- 白平衡属性：输入/输出范围、饱和度阈值、直方图 bin 数和 range 最大值。
- 函数：`ApplyChannelGains`、`DctDenoising`、`Bm3dDenoising`、`Inpaint` 和 `OilPainting`。
- 枚举：`Bm3dSteps`、`InpaintTypes` 与 `TransformTypes`。

## Runtime / 运行时

`xphoto` is an optional OpenCV contrib module. Runtime staging should include the factual OpenCV 5.0.0 runtime artifact `opencv_xphoto500.dll` when the module is built. If it is missing, the managed API shape remains stable and calls report `NOT_LINKED`.

`xphoto` 是可选 OpenCV contrib 模块。构建该模块时，runtime staging 应包含事实性 OpenCV 5.0.0 runtime 产物 `opencv_xphoto500.dll`。如果缺少该 DLL，managed API 形状仍保持稳定，调用会报告 `NOT_LINKED`。

## Input Notes / 输入说明

XPhoto algorithms are sensitive to channel count, depth, and parameter ranges. BM3D expects grayscale 8-bit or 16-bit input in OpenCV's implementation. White-balancer outputs can vary substantially across real photographs, illuminants, and saturation thresholds.

XPhoto 算法对通道数、位深和参数范围较敏感。OpenCV 的 BM3D 实现期望灰度 8-bit 或 16-bit 输入。白平衡输出会随真实照片、光源和饱和度阈值明显变化。

`Inpaint` requires a same-size `CV_8UC1` mask. Non-zero mask pixels are valid source content; zero pixels are reconstructed. Shiftmap supports one to four source channels at `CV_8U`, `CV_8S`, `CV_16U`, `CV_16S`, `CV_32S`, `CV_32F`, or `CV_64F` depth, while FSR supports grayscale or BGR input at `CV_8U`, `CV_16U`, `CV_32F`, or `CV_64F` depth. FSR expects conventional values of 0–255, 0–65535, and 0–1 for those respective depths; out-of-range image content is rejected by OpenCV.

`Inpaint` 要求与源图像同尺寸的 `CV_8UC1` 掩码。掩码非零位置是有效源内容，零位置会被重建。Shiftmap 支持 1 至 4 个源通道以及 `CV_8U`、`CV_8S`、`CV_16U`、`CV_16S`、`CV_32S`、`CV_32F` 或 `CV_64F` 位深；FSR 支持 `CV_8U`、`CV_16U`、`CV_32F` 或 `CV_64F` 位深的灰度或 BGR 输入。FSR 对应位深的常规像素范围分别为 0–255、0–65535 和 0–1；超出范围的图像内容由 OpenCV 拒绝。

```csharp
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.XPhoto;

using Mat color = new Mat(8, 8, MatType.CV_8UC3, new Scalar(10, 20, 30));
using SimpleWB whiteBalancer = XPhotoCv2.CreateSimpleWB();
whiteBalancer.P = 1.0F;
using Mat balanced = whiteBalancer.BalanceWhite(color);

using Mat gains = XPhotoCv2.ApplyChannelGains(color, 1.0F, 1.1F, 0.9F);

using Mat gray = new Mat(128, 128, MatType.CV_8UC1, new Scalar(127));
using Mat validMask = new Mat(128, 128, MatType.CV_8UC1, new Scalar(255));
using Mat restored = XPhotoCv2.Inpaint(gray, validMask, InpaintTypes.FsrFast);
```

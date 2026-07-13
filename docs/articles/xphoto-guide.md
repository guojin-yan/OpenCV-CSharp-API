# XPhoto Guide / XPhoto 指南

`OpenCvSharp.XPhoto` wraps the first optional contrib `xphoto` white balance and enhancement utilities.

`OpenCvSharp.XPhoto` 封装第一批可选 contrib `xphoto` 白平衡与增强工具。

## Scope / 范围

- White balancers: `WhiteBalancer`, `SimpleWB`, `GrayworldWB`, and `LearningBasedWB`.
- White-balance properties for input/output range, saturation threshold, histogram bins, and range maximum.
- Functions: `ApplyChannelGains`, `DctDenoising`, `Bm3dDenoising`, and `OilPainting`.
- Enums: `Bm3dSteps` and `TransformTypes`.

- 白平衡器：`WhiteBalancer`、`SimpleWB`、`GrayworldWB` 和 `LearningBasedWB`。
- 白平衡属性：输入/输出范围、饱和度阈值、直方图 bin 数和 range 最大值。
- 函数：`ApplyChannelGains`、`DctDenoising`、`Bm3dDenoising` 和 `OilPainting`。
- 枚举：`Bm3dSteps` 与 `TransformTypes`。

## Runtime / 运行时

`xphoto` is an optional OpenCV contrib module. Runtime staging should include the factual OpenCV 5.0.0 runtime artifact `opencv_xphoto500.dll` when the module is built. If it is missing, the managed API shape remains stable and calls report `NOT_LINKED`.

`xphoto` 是可选 OpenCV contrib 模块。构建该模块时，runtime staging 应包含事实性 OpenCV 5.0.0 runtime 产物 `opencv_xphoto500.dll`。如果缺少该 DLL，managed API 形状仍保持稳定，调用会报告 `NOT_LINKED`。

## Input Notes / 输入说明

XPhoto algorithms are sensitive to channel count, depth, and parameter ranges. BM3D expects grayscale 8-bit or 16-bit input in OpenCV's implementation. White-balancer outputs can vary substantially across real photographs, illuminants, and saturation thresholds.

XPhoto 算法对通道数、位深和参数范围较敏感。OpenCV 的 BM3D 实现期望灰度 8-bit 或 16-bit 输入。白平衡输出会随真实照片、光源和饱和度阈值明显变化。

```csharp
using OpenCvSharp.Core;
using OpenCvSharp.XPhoto;

using Mat color = new Mat(8, 8, MatType.CV_8UC3, new Scalar(10, 20, 30));
using SimpleWB whiteBalancer = XPhotoCv2.CreateSimpleWB();
whiteBalancer.P = 1.0F;
using Mat balanced = whiteBalancer.BalanceWhite(color);

using Mat gains = XPhotoCv2.ApplyChannelGains(color, 1.0F, 1.1F, 0.9F);
```

# Fuzzy Guide / Fuzzy 指南

`OpenCvSharp.Fuzzy` wraps the first model-free OpenCV contrib `fuzzy` F-transform and image inpainting helpers.

`OpenCvSharp.Fuzzy` 封装第一批无需模型文件的 OpenCV contrib `fuzzy` F-transform 与图像修复 helper。

## Scope / 范围

- Kernel creation from predefined functions or caller-provided function matrices.
- Image helpers: `FuzzyCv2.Filter` and `FuzzyCv2.Inpaint`.
- F0 helpers: `FT02DComponents`, `FT02DInverseFT`, `FT02DProcess`, `FT02DIteration`, `FT02DFLProcess`, and `FT02DFLProcessFloat`.
- F1 helpers: `FT12DComponents`, `FT12DPolynomial`, `FT12DCreatePolynomMatrixVertical`, `FT12DCreatePolynomMatrixHorizontal`, `FT12DInverseFT`, and `FT12DProcess`.
- Enum mappings: `FuzzyFunctionType.Linear`, `FuzzyFunctionType.Sinus`, and `FuzzyInpaintAlgorithm`.

- 通过预定义函数或调用方函数矩阵创建 kernel。
- 图像 helper：`FuzzyCv2.Filter` 与 `FuzzyCv2.Inpaint`。
- F0 helper：`FT02DComponents`、`FT02DInverseFT`、`FT02DProcess`、`FT02DIteration`、`FT02DFLProcess` 和 `FT02DFLProcessFloat`。
- F1 helper：`FT12DComponents`、`FT12DPolynomial`、`FT12DCreatePolynomMatrixVertical`、`FT12DCreatePolynomMatrixHorizontal`、`FT12DInverseFT` 和 `FT12DProcess`。
- 枚举映射：`FuzzyFunctionType.Linear`、`FuzzyFunctionType.Sinus` 和 `FuzzyInpaintAlgorithm`。

## Runtime / 运行时

`fuzzy` is an optional OpenCV contrib module. Runtime staging includes the factual OpenCV 5.0.0 runtime artifact `opencv_fuzzy500.dll` when the module is built. If a runtime lacks it, the managed API shape remains stable and linked calls report `NOT_LINKED`.

`fuzzy` 是可选 OpenCV contrib 模块。构建该模块时 runtime staging 会包含事实性 OpenCV 5.0.0 runtime 产物 `opencv_fuzzy500.dll`。如果某个 runtime 缺少它，managed API 形状仍保持稳定，linked 调用会报告 `NOT_LINKED`。

## Input Notes / 输入说明

Most F0/F1 helpers expect matrix and kernel channel counts to match. The optimized F0 linear helpers expect 3-channel input. Tiny smoke checks output shape and call paths; it does not measure inpaint or transform quality.

多数 F0/F1 helper 要求 matrix 与 kernel 通道数匹配。优化的 F0 linear helper 期望 3 通道输入。tiny smoke 只检查输出形状和调用路径，不衡量修复或变换质量。

```csharp
using OpenCvSharp.Core;
using OpenCvSharp.Fuzzy;

using Mat image = new Mat(8, 8, MatType.CV_8UC3, new Scalar(20, 40, 80));
using Mat mask = new Mat(8, 8, MatType.CV_8UC1, new Scalar(0));
using Mat kernel = FuzzyCv2.CreateKernel(FuzzyFunctionType.Linear, radius: 2, channels: image.Channels);

using Mat filtered = FuzzyCv2.Filter(image, kernel);
using Mat inpainted = FuzzyCv2.Inpaint(
    image,
    mask,
    radius: 2,
    functionType: FuzzyFunctionType.Linear,
    algorithm: FuzzyInpaintAlgorithm.OneStep);
```

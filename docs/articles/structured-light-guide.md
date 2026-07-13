# Structured Light Guide / Structured Light 指南

`OpenCvSharp.StructuredLight` wraps the first model-free OpenCV contrib `structured_light` surfaces: Gray-code and sinusoidal pattern generation plus selected decode/phase helpers.

`OpenCvSharp.StructuredLight` 封装第一批无需模型文件的 OpenCV contrib `structured_light` 能力：Gray-code 和正弦图案生成，以及部分解码/相位 helper。

## Scope / 范围

- Base pattern object: `StructuredLightPattern.Generate`.
- Gray-code: `GrayCodePatternParams`, `GrayCodePattern.Create`, `NumberOfPatternImages`, threshold setters, `GetImagesForShadowMasks`, and `GetProjPixel`.
- Sinusoidal: `SinusoidalPatternParams`, `SinusoidalPatternMethod`, `SinusoidalPattern.Create`, `ComputePhaseMap`, `UnwrapPhaseMap`, and `ComputeDataModulationTerm`.
- Factory helpers: `StructuredLightCv2.CreateGrayCodePattern` and `StructuredLightCv2.CreateSinusoidalPattern`.

- 基类图案对象：`StructuredLightPattern.Generate`。
- Gray-code：`GrayCodePatternParams`、`GrayCodePattern.Create`、`NumberOfPatternImages`、阈值 setter、`GetImagesForShadowMasks` 与 `GetProjPixel`。
- 正弦：`SinusoidalPatternParams`、`SinusoidalPatternMethod`、`SinusoidalPattern.Create`、`ComputePhaseMap`、`UnwrapPhaseMap` 与 `ComputeDataModulationTerm`。
- 工厂 helper：`StructuredLightCv2.CreateGrayCodePattern` 与 `StructuredLightCv2.CreateSinusoidalPattern`。

## Runtime / 运行时

`structured_light` is an optional OpenCV contrib module. Runtime staging includes the factual OpenCV 5.0.0 runtime artifact `opencv_structured_light500.dll` when the module is built. If a runtime lacks it, the managed API shape remains stable and linked calls report `NOT_LINKED`.

`structured_light` 是可选 OpenCV contrib 模块。构建该模块时 runtime staging 会包含事实性 OpenCV 5.0.0 runtime 产物 `opencv_structured_light500.dll`。如果某个 runtime 缺少它，managed API 形状仍保持稳定，linked 调用会报告 `NOT_LINKED`。

## Input Notes / 输入说明

Pattern generation returns newly owned `Mat[]` values; dispose each returned matrix after use. Gray-code `GetProjPixel` expects captured pattern images. Passing generated projector images is useful as a smoke check, but real projector-camera calibration requires images captured by a camera. Sinusoidal phase helpers also expect captured pattern images and a shadow mask.

图案生成返回新持有的 `Mat[]`；使用后需要逐个释放。Gray-code `GetProjPixel` 期望输入相机采集到的图案图像。传入生成的投影图案适合作为 smoke 检查，但真实投影仪-相机标定需要使用相机采集图像。正弦相位 helper 同样期望采集图像和 shadow mask。

```csharp
using OpenCvSharp.Core;
using OpenCvSharp.StructuredLight;

using GrayCodePattern gray = GrayCodePattern.Create(16, 8);
Mat[] grayImages = gray.Generate();
try
{
    gray.GetImagesForShadowMasks(out Mat black, out Mat white);
    using (black)
    using (white)
    {
        bool found = gray.GetProjPixel(grayImages, 0, 0, out Point projectorPixel);
    }
}
finally
{
    foreach (Mat image in grayImages)
    {
        image.Dispose();
    }
}

using SinusoidalPattern sinusoidal = SinusoidalPattern.Create(new SinusoidalPatternParams
{
    Width = 24,
    Height = 16,
    NbrOfPeriods = 4,
    Method = SinusoidalPatternMethod.Psp
});

Mat[] sinusoidalImages = sinusoidal.Generate();
foreach (Mat image in sinusoidalImages)
{
    image.Dispose();
}
```

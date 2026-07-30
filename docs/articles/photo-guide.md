# Photo Guide

`OpenCvSharp.Photo` exposes OpenCV `photo` module functions and objects: inpainting, fast non-local means and TV-L1 denoising, chromatic-aberration correction, seamless editing, edge-preserving stylization, tonemap operators, the main HDR align/calibrate/merge workflow, CPU color correction models, and stateful live-wire contours. These APIs require the factual OpenCV 5.0.0 runtime artifact `opencv_photo500.dll`. See [Photo HDR Workflow Guide](photo-hdr-workflow-guide.md), [Photo Color Correction Model Guide](photo-ccm-guide.md), [Photo Intelligent Scissors Guide](photo-intelligent-scissors-guide.md), and [Photo TV-L1 And Chromatic Aberration Guide](photo-tvl1-chromatic-aberration-guide.md) for ownership, state, and data contracts.

`OpenCvSharp.Photo` 暴露 OpenCV `photo` 模块函数和对象：图像修复、单帧和多帧 fast non-local means 去噪、seamless editing、边缘保持风格化，以及 tone mapping 对象。这些 API 需要事实性 OpenCV 5.0.0 runtime 产物 `opencv_photo500.dll`。

## Covered APIs / 已覆盖接口

- `PhotoCv2.Inpaint`
- `PhotoCv2.FastNlMeansDenoising`
- `PhotoCv2.FastNlMeansDenoisingMulti`
- `PhotoCv2.FastNlMeansDenoisingColored`
- `PhotoCv2.FastNlMeansDenoisingColoredMulti`
- `PhotoCv2.DenoiseTvl1`
- `PhotoCv2.CorrectChromaticAberration`
- `PhotoCv2.LoadChromaticAberrationParams` and `ChromaticAberrationParameters`
- `PhotoCv2.Decolor`
- `PhotoCv2.SeamlessClone`
- `PhotoCv2.ColorChange`
- `PhotoCv2.IlluminationChange`
- `PhotoCv2.TextureFlattening`
- `PhotoCv2.EdgePreservingFilter`
- `PhotoCv2.DetailEnhance`
- `PhotoCv2.PencilSketch`
- `PhotoCv2.Stylization`
- `Tonemap`, `TonemapDrago`, `TonemapReinhard`, and `TonemapMantiuk`
- `AlignMTB`, `CalibrateDebevec`, and `CalibrateRobertson`
- `MergeDebevec`, `MergeMertens`, and `MergeRobertson`
- `PhotoCv2.GammaCorrection` and `ColorCorrectionModel`
- `IntelligentScissorsMB`
- `CcmType`, `InitialMethodType`, `ColorCheckerType`, `ColorSpace`, `LinearizationType`, and `DistanceType`
- `InpaintMethod`
- `SeamlessCloneFlags`, `EdgePreservingFilterFlags`

- `PhotoCv2.Inpaint`
- `PhotoCv2.FastNlMeansDenoising`
- `PhotoCv2.FastNlMeansDenoisingMulti`
- `PhotoCv2.FastNlMeansDenoisingColored`
- `PhotoCv2.FastNlMeansDenoisingColoredMulti`
- `PhotoCv2.Decolor`
- `PhotoCv2.SeamlessClone`
- `PhotoCv2.ColorChange`
- `PhotoCv2.IlluminationChange`
- `PhotoCv2.TextureFlattening`
- `PhotoCv2.EdgePreservingFilter`
- `PhotoCv2.DetailEnhance`
- `PhotoCv2.PencilSketch`
- `PhotoCv2.Stylization`
- `Tonemap`、`TonemapDrago`、`TonemapReinhard` 和 `TonemapMantiuk`
- `InpaintMethod`
- `SeamlessCloneFlags`、`EdgePreservingFilterFlags`

## Inpainting And Denoising / 图像修复与去噪

```csharp
using OpenCvSharp.Core;
using OpenCvSharp.Photo;

namespace PhotoFunctionSample
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat src = new Mat(64, 64, MatType.CV_8UC1, new Scalar(32)))
            using (Mat mask = new Mat(64, 64, MatType.CV_8UC1, new Scalar(0)))
            using (Mat repaired = PhotoCv2.Inpaint(src, mask, 3.0, InpaintMethod.Telea))
            using (Mat denoised = PhotoCv2.FastNlMeansDenoising(src))
            {
                System.Console.WriteLine("Inpaint=" + repaired.Size + ", denoise=" + denoised.Size);
            }
        }
    }
}
```

`Inpaint`, `FastNlMeansDenoising`, and `FastNlMeansDenoisingColored` provide returning `Mat` overloads for simple single-output use. Output-`Mat` overloads remain available when callers want to reuse a destination matrix. `FastNlMeansDenoising` also has a per-channel `float[]` overload and a `ReadOnlySpan<float>` overload on modern target frameworks.

`Inpaint`、`FastNlMeansDenoising` 与 `FastNlMeansDenoisingColored` 为简单单输出用法提供返回 `Mat` 的重载。调用方需要复用目标矩阵时，仍可使用 output-`Mat` 重载。`FastNlMeansDenoising` 还提供按通道 `float[]` 强度重载，并在现代目标框架上提供 `ReadOnlySpan<float>` 重载。

## Tonemap Objects / Tone Mapping 对象

```csharp
using OpenCvSharp.Core;
using OpenCvSharp.Photo;

namespace TonemapSample
{
    internal static class Program
    {
        private static void Main()
        {
            using (TonemapDrago tonemap = PhotoCv2.CreateTonemapDrago(1.0F, 1.0F, 0.85F))
            using (Mat hdr = new Mat(4, 4, MatType.CV_32FC3, new Scalar(0.25, 0.5, 0.75)))
            using (Mat ldr = tonemap.Process(hdr))
            {
                tonemap.Bias = 0.8F;
                System.Console.WriteLine("Tonemap=" + ldr.Size + ", gamma=" + tonemap.Gamma);
            }
        }
    }
}
```

The `Tonemap` base handle owns the native `cv::Ptr<cv::Tonemap>` through an opaque C ABI handle. Derived properties such as Drago bias, Reinhard adaptation, and Mantiuk scale are accessed through dedicated functions; no `cv::Ptr` crosses the ABI.

`Tonemap` 基类句柄通过 opaque C ABI 句柄持有 native `cv::Ptr<cv::Tonemap>`。Drago bias、Reinhard adaptation 和 Mantiuk scale 等派生属性通过专用函数访问；`cv::Ptr` 不会穿过 ABI。

## Runtime Notes / 运行时说明

Photo APIs require the factual OpenCV 5.0.0 runtime artifact `opencv_photo500.dll` in addition to the core `Mat` runtime. Multi-frame denoise uses a short-lived array of `Mat` handles and does not expose OpenCV vector layouts through the ABI. Real results depend on valid image types and ranges; tonemap operators usually expect floating-point HDR input. Default tests avoid external images and only run native smoke when `OPENCV_CSHARP_NATIVE_SMOKE` is enabled. The older `OPENCV5SHARP_NATIVE_SMOKE` name remains accepted only as an existing-smoke-workflow compatibility alias.

Photo API 除 core `Mat` runtime 外还需要事实性 OpenCV 5.0.0 runtime 产物 `opencv_photo500.dll`。多帧去噪使用短生命周期的 `Mat` 句柄数组，不会通过 ABI 暴露 OpenCV vector 布局。真实效果取决于有效的图像类型和数值范围；tone mapping 算子通常期望浮点 HDR 输入。默认测试不依赖外部图像，只有设置 `OPENCV_CSHARP_NATIVE_SMOKE` 时才运行 native smoke。旧的 `OPENCV5SHARP_NATIVE_SMOKE` 名称仍仅作为既有 smoke workflow 的兼容别名使用。

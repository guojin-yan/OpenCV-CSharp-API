# BioInspired Guide / BioInspired 指南

`OpenCvSharp.BioInspired` wraps the first OpenCV contrib `bioinspired` Retina, fast tone-mapping, and transient-area segmentation surfaces.

`OpenCvSharp.BioInspired` 封装第一批 OpenCV contrib `bioinspired` Retina、fast tone mapping 和 transient-area segmentation 能力。

## Scope / 范围

- Factories: `BioInspiredCv2.CreateRetina`, `CreateRetinaFastToneMapping`, and `CreateTransientAreasSegmentationModule`.
- Objects: `Retina`, `RetinaFastToneMapping`, and `TransientAreasSegmentationModule`.
- Value types: `RetinaParvoParameters`, `RetinaMagnoParameters`, `RetinaParameters`, `SegmentationParameters`, and `RetinaColorSamplingMethod`.
- Outputs use caller-owned `Mat` overloads and new-`Mat` convenience overloads.

- 工厂：`BioInspiredCv2.CreateRetina`、`CreateRetinaFastToneMapping` 和 `CreateTransientAreasSegmentationModule`。
- 对象：`Retina`、`RetinaFastToneMapping` 和 `TransientAreasSegmentationModule`。
- 值类型：`RetinaParvoParameters`、`RetinaMagnoParameters`、`RetinaParameters`、`SegmentationParameters` 和 `RetinaColorSamplingMethod`。
- 输出同时提供调用方持有 `Mat` 重载和返回新 `Mat` 的便利重载。

## Runtime / 运行时

`bioinspired` is an optional OpenCV contrib module. Runtime staging includes the factual OpenCV 5.0.0 runtime artifact `opencv_bioinspired500.dll` when the module is built. If a runtime lacks it, the managed API shape remains stable and linked calls report `NOT_LINKED`.

`bioinspired` 是可选 OpenCV contrib 模块。构建该模块时 runtime staging 会包含事实性 OpenCV 5.0.0 runtime 产物 `opencv_bioinspired500.dll`。如果某个 runtime 缺少它，managed API 形状仍保持稳定，linked 调用会报告 `NOT_LINKED`。

The local OpenCV 5.0.0 headers expose `Retina.Run(InputArray)`, `Retina.ApplyFastToneMapping(InputArray, OutputArray)`, `RetinaFastToneMapping.ApplyFastToneMapping(InputArray, OutputArray)`, and `TransientAreasSegmentationModule.Run(InputArray, int)`. Those algorithm calls are available in the managed API, but linked smoke is intentionally treated as unstable for this module: default tests and ordinary `OPENCV_CSHARP_NATIVE_SMOKE=1` runs do not create linked BioInspired objects, and `OPENCV_CSHARP_UNSTABLE_NATIVE_SMOKE=1` is required for object, metadata, Retina, tone, and transient smoke. The older `OPENCV5SHARP_UNSTABLE_NATIVE_SMOKE=1` name remains accepted only as an existing-smoke-workflow compatibility alias.

本地 OpenCV 5.0.0 header 暴露 `Retina.Run(InputArray)`、`Retina.ApplyFastToneMapping(InputArray, OutputArray)`、`RetinaFastToneMapping.ApplyFastToneMapping(InputArray, OutputArray)` 和 `TransientAreasSegmentationModule.Run(InputArray, int)`。这些算法调用在 managed API 中可用，但本模块的 linked smoke 被有意视作不稳定路径：默认测试和普通 `OPENCV_CSHARP_NATIVE_SMOKE=1` 都不会创建 linked BioInspired 对象；object、metadata、Retina、tone 和 transient smoke 都需要额外设置 `OPENCV_CSHARP_UNSTABLE_NATIVE_SMOKE=1`。旧的 `OPENCV5SHARP_UNSTABLE_NATIVE_SMOKE=1` 名称仍仅作为既有 smoke workflow 的兼容别名使用。

## Example / 示例

The following example demonstrates real algorithm calls. Keep it out of default CI smoke unless you are validating a linked BioInspired runtime explicitly.

下面示例演示真实算法调用。除非正在显式验证 linked BioInspired runtime，否则不要把它放进默认 CI smoke。

```csharp
using OpenCvSharp.BioInspired;
using OpenCvSharp.Core;
using OpenCvSharp.ImgProc;

internal static class Program
{
    private static void Main()
    {
        using Mat image = new Mat(32, 32, MatType.CV_8UC3, new Scalar(20, 30, 40));
        Cv2.Rectangle(image, new Rect(8, 8, 12, 12), new Scalar(210, 80, 120), -1);

        using Retina retina = BioInspiredCv2.CreateRetina(image.Size);
        retina.Run(image);
        using Mat parvo = retina.GetParvo();
        using Mat magno = retina.GetMagno();

        using RetinaFastToneMapping toneMapping = BioInspiredCv2.CreateRetinaFastToneMapping(image.Size);
        using Mat toneMapped = toneMapping.Apply(image);

        using TransientAreasSegmentationModule segmentation = BioInspiredCv2.CreateTransientAreasSegmentationModule(image.Size);
        segmentation.Run(image);
        using Mat segmented = segmentation.GetSegmentationPicture();
    }
}
```

Retina and transient segmentation are stateful models. For video-like workflows, feed frames in sequence and clear buffers when starting a new sequence.

Retina 和 transient segmentation 是有状态模型。类视频工作流应按顺序输入帧；开始新序列时可清除内部缓冲区。

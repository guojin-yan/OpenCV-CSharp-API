# XImgProc Run-Length Morphology Guide / XImgProc Run-Length Morphology 指南

`XImgProcRlCv2` wraps OpenCV contrib `cv::ximgproc::rl` helpers for run-length encoded binary morphology.

`XImgProcRlCv2` 封装 OpenCV contrib `cv::ximgproc::rl` 中面向 run-length encoded 二值形态学的 helper。

## Scope / 范围

- `Threshold`: converts a single-channel image to an RLE image.
- `Dilate`, `Erode`, and `MorphologyEx`.
- `GetStructuringElement`.
- `Paint`.
- `IsRLMorphologyPossible`.
- `CreateRLEImage` from `Point3i` run triples.

- `Threshold`：将单通道图像转换为 RLE 图像。
- `Dilate`、`Erode` 和 `MorphologyEx`。
- `GetStructuringElement`。
- `Paint`。
- `IsRLMorphologyPossible`。
- 基于 `Point3i` run 三元组的 `CreateRLEImage`。

## Input Notes / 输入说明

OpenCV stores RLE images in normal `Mat` objects containing `Point3i` rows. For `CreateRLEImage`, each run uses OpenCV's `(column begin, column end, row)` convention. Only `ThresholdTypes.Binary` and `ThresholdTypes.BinaryInv` are supported by OpenCV's RLE threshold helper.

OpenCV 将 RLE 图像存储在普通 `Mat` 中，其中包含 `Point3i` 行。`CreateRLEImage` 的每个 run 使用 OpenCV 的 `(column begin, column end, row)` 约定。OpenCV 的 RLE threshold helper 只支持 `ThresholdTypes.Binary` 和 `ThresholdTypes.BinaryInv`。

## Example / 示例

```csharp
using OpenCvSharp.Core;
using OpenCvSharp.ImgProc;
using OpenCvSharp.XImgProc;

namespace XImgProcRleExample
{
    internal static class Program
    {
        private static void Main()
        {
            using Mat image = new Mat(16, 16, MatType.CV_8UC1, new Scalar(0));
            Cv2.Rectangle(image, new Rect(4, 4, 8, 8), new Scalar(255), -1);

            using Mat rl = XImgProcRlCv2.Threshold(image, 100.0, ThresholdTypes.Binary);
            using Mat kernel = XImgProcRlCv2.GetStructuringElement(MorphShapes.Rect, new Size(3, 3));
            using Mat opened = XImgProcRlCv2.MorphologyEx(rl, MorphTypes.Open, kernel);

            using Mat painted = new Mat(image.Rows, image.Cols, MatType.CV_8UC1, new Scalar(0));
            XImgProcRlCv2.Paint(painted, opened, new Scalar(255));

            using Mat fromRuns = XImgProcRlCv2.CreateRLEImage(
                new[] { new Point3i(1, 4, 1), new Point3i(1, 4, 2) },
                new Size(16, 16));
        }
    }
}
```

## Smoke / Smoke

Default tests validate null arguments and empty run input. Linked smoke uses a tiny binary image, conservative kernel, paint output, and explicit run triples.

默认测试校验 null 参数和空 run 输入。linked smoke 使用 tiny 二值图、保守 kernel、paint 输出和显式 run 三元组。

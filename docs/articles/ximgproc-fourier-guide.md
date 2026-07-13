# XImgProc Fourier Guide / XImgProc Fourier 指南

`OpenCvSharp.XImgProc` wraps OpenCV contrib Fourier descriptor utilities for closed contours.

`OpenCvSharp.XImgProc` 封装 OpenCV contrib 中面向闭合轮廓的 Fourier descriptor 工具。

## Scope / 范围

- `XImgProcCv2.FourierDescriptor`.
- `XImgProcCv2.TransformFD`.
- `XImgProcCv2.ContourSampling`.
- `ContourFitting` with `CtrSize`, `FDSize`, and `EstimateTransformation`.

- `XImgProcCv2.FourierDescriptor`。
- `XImgProcCv2.TransformFD`。
- `XImgProcCv2.ContourSampling`。
- `ContourFitting`，包含 `CtrSize`、`FDSize` 和 `EstimateTransformation`。

## Input Notes / 输入说明

Contour inputs are caller-owned `Mat` values. You can use `Calib3D.Cv2.ToPointMat(Point2f[])` to build a two-channel point matrix. OpenCV requires valid closed-contour data; for explicit `nbFD`, use values within the OpenCV constraint for the selected `nbElt`. `ContourFitting` also requires `FDSize` to fit the sampled contour size.

轮廓输入是调用方持有的 `Mat`。可使用 `Calib3D.Cv2.ToPointMat(Point2f[])` 构建二通道点矩阵。OpenCV 要求输入是有效闭合轮廓；显式设置 `nbFD` 时，取值必须满足所选 `nbElt` 的 OpenCV 约束。`ContourFitting` 的 `FDSize` 也必须适配重采样轮廓大小。

## Example / 示例

```csharp
using OpenCvSharp.Calib3D;
using OpenCvSharp.Core;
using OpenCvSharp.XImgProc;

namespace XImgProcFourierExample
{
    internal static class Program
    {
        private static void Main()
        {
            using Mat contour = Cv2.ToPointMat(new[]
            {
                new Point2f(0.0F, 0.0F),
                new Point2f(16.0F, 0.0F),
                new Point2f(16.0F, 16.0F),
                new Point2f(0.0F, 16.0F)
            });

            using Mat sampled = XImgProcCv2.ContourSampling(contour, 8);
            using Mat descriptor = XImgProcCv2.FourierDescriptor(contour, nbElt: 8, nbFD: 4);
            using ContourFitting fitting = XImgProcCv2.CreateContourFitting(8, 3);
            using Mat transform = fitting.EstimateTransformation(sampled, sampled, out double distance);
            using Mat transformed = XImgProcCv2.TransformFD(sampled, transform, fdContour: false);
        }
    }
}
```

## Smoke / Smoke

Linked smoke uses a tiny square contour and conservative descriptor sizes. It verifies ABI shape and object lifetime rather than contour matching quality.

linked smoke 使用 tiny 方形轮廓和保守 descriptor 大小，验证 ABI 形状与对象生命周期，不衡量轮廓匹配质量。

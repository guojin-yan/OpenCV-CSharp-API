# XImgProc Sparse Interpolation Guide / XImgProc 稀疏插值指南

`OpenCvSharp.XImgProc` wraps OpenCV contrib sparse match interpolation as reusable managed objects.

`OpenCvSharp.XImgProc` 将 OpenCV contrib 稀疏匹配插值能力封装为可复用 managed 对象。

## Scope / 范围

- `SparseMatchInterpolator`: common base class with `Interpolate` overloads for `Mat` point sets and managed `Point2f[]` arrays.
- `EdgeAwareInterpolator`: edge-aware local interpolation with `K`, `Sigma`, `Lambda`, FGS post-processing options, and optional cost-map input.
- `RICInterpolator`: robust interpolation of correspondences with superpixel, model, refinement, FGS, and variational-refinement controls.

- `SparseMatchInterpolator`：通用基类，提供基于 `Mat` 点集和 managed `Point2f[]` 数组的 `Interpolate` 重载。
- `EdgeAwareInterpolator`：边缘感知局部插值，包含 `K`、`Sigma`、`Lambda`、FGS 后处理选项和可选 cost map 输入。
- `RICInterpolator`：robust interpolation of correspondences，包含 superpixel、模型、refinement、FGS 和 variational refinement 控制项。

## Point Sets / 点集

The native ABI does not expose `std::vector<Point2f>`. Managed point arrays are converted to caller-owned `Mat` values before crossing the boundary, and dense flow is written into a caller-owned `Mat`, commonly `CV_32FC2`.

native ABI 不暴露 `std::vector<Point2f>`。managed 点数组会在跨边界前转换为调用方持有的 `Mat`，dense flow 写入调用方持有的 `Mat`，常见输出类型为 `CV_32FC2`。

The two point arrays must be non-empty and have the same length.

两个点数组必须非空且长度一致。

## Example / 示例

```csharp
using OpenCvSharp.Core;
using OpenCvSharp.XImgProc;

namespace XImgProcSparseInterpolationExample
{
    internal static class Program
    {
        private static void Main()
        {
            using Mat from = new Mat(32, 32, MatType.CV_8UC3, new Scalar(24, 48, 72));
            using Mat to = from.Clone();

            Point2f[] fromPoints =
            {
                new Point2f(2.0F, 2.0F),
                new Point2f(28.0F, 2.0F),
                new Point2f(2.0F, 28.0F),
                new Point2f(28.0F, 28.0F)
            };

            Point2f[] toPoints =
            {
                new Point2f(3.0F, 2.0F),
                new Point2f(29.0F, 2.0F),
                new Point2f(3.0F, 28.0F),
                new Point2f(29.0F, 28.0F)
            };

            using EdgeAwareInterpolator edgeAware = XImgProcCv2.CreateEdgeAwareInterpolator();
            edgeAware.UsePostProcessing = false;
            using Mat denseFlow = edgeAware.Interpolate(from, fromPoints, to, toPoints);

            using RICInterpolator ric = XImgProcCv2.CreateRICInterpolator();
            ric.UseGlobalSmootherFilter = false;
            ric.UseVariationalRefinement = false;
            using Mat ricFlow = ric.Interpolate(from, fromPoints, to, toPoints);
        }
    }
}
```

## Smoke / Smoke

Linked smoke uses tiny 3-channel synthetic images and four sparse matches. The check asserts output shape and object lifetime only; interpolation quality depends on image content, point distribution, and parameter scale.

linked smoke 使用 tiny 三通道合成图和四个稀疏匹配点。该检查只断言输出形状和对象生命周期；插值质量取决于图像内容、点分布和参数尺度。

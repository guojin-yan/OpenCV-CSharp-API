# ImgProc Segmentation, Contours, and Features Guide

This guide shows the current image-analysis flow built around segmentation, connected components, contours, image moments, histogram equalization, and corner responses.

本文档展示当前围绕分割、连通域、轮廓、图像矩、直方图均衡化和角点响应构建的图像分析流程。

## Pipeline / 流水线

The managed surface stays close to OpenCV C++ names while using C# conventions:

托管表层接口尽量贴近 OpenCV C++ 命名，同时遵循 C# 使用习惯：

```csharp
using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Geometry;
using JYPPX.OpenCvSharp.ImgProc;
using ImgProcCv2 = JYPPX.OpenCvSharp.ImgProc.Cv2;

namespace Demo
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat src = new Mat(8, 8, MatType.CV_8UC1))
            using (Mat equalized = new Mat())
            using (Mat binary = new Mat())
            using (Mat labels = new Mat())
            using (Mat stats = new Mat())
            using (Mat centroids = new Mat())
            using (Mat distance = new Mat())
            using (Mat distanceLabels = new Mat())
            using (Mat contourCanvas = new Mat(8, 8, MatType.CV_8UC1))
            using (Mat cornerResponse = new Mat())
            {
                src.CopyFrom(new byte[]
                {
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 40, 60, 60, 40, 0, 0, 0,
                    0, 60, 180, 180, 60, 0, 0, 0,
                    0, 60, 180, 180, 60, 0, 0, 0,
                    0, 40, 60, 60, 40, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 220, 0,
                    0, 0, 0, 0, 0, 0, 220, 0,
                    0, 0, 0, 0, 0, 0, 0, 0
                });

                ImgProcCv2.EqualizeHist(src, equalized);
                ImgProcCv2.AdaptiveThreshold(equalized, binary, 255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.Binary, 3, 2);
                ImgProcCv2.DistanceTransform(binary, distance, distanceLabels, DistanceTypes.L2, DistanceTransformMasks.Mask3);

                int componentCount = ImgProcCv2.ConnectedComponentsWithStats(binary, labels, stats, centroids);
                ImgProcCv2.FindContours(binary, out Point[][] contours, out Vec4i[] hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
                ImgProcCv2.DrawContours(contourCanvas, contours, -1, new Scalar(255), hierarchy: hierarchy);

                Moments moments = ImgProcCv2.Moments(binary, true);
                double[] hu = ImgProcCv2.HuMoments(moments);
                ImgProcCv2.CornerHarris(binary, cornerResponse, 3, 3, 0.04);

                Console.WriteLine($"components={componentCount}");
                Console.WriteLine($"contours={contours.Length}, hierarchy={hierarchy.Length}");
                Console.WriteLine($"m00={moments.M00}, hu0={hu[0]}");
                Console.WriteLine($"corner={cornerResponse.Rows}x{cornerResponse.Cols}");
            }
        }
    }
}
```

## Implemented APIs / 已实现 API

- Segmentation: `AdaptiveThreshold`, `DistanceTransform`, `FloodFill`.
- Statistics: `ConnectedComponents`, `ConnectedComponentsWithAlgorithm`, `ConnectedComponentsWithStats`, `ConnectedComponentsWithStatsWithAlgorithm`.
- Integral images: `Integral`, `Integral2`, `Integral3`.
- Contours: `FindContours`, `DrawContours`.
- Moments: `Moments(Mat)`, `Moments(Point[])`, `HuMoments`.
- Histogram and features: `EqualizeHist`, `CornerHarris`, `CornerMinEigenVal`, `CornerEigenValsAndVecs`, `PreCornerDetect`.

- 分割：`AdaptiveThreshold`、`DistanceTransform`、`FloodFill`。
- 统计：`ConnectedComponents`、`ConnectedComponentsWithAlgorithm`、`ConnectedComponentsWithStats`、`ConnectedComponentsWithStatsWithAlgorithm`。
- 积分图：`Integral`、`Integral2`、`Integral3`。
- 轮廓：`FindContours`、`DrawContours`。
- 矩：`Moments(Mat)`、`Moments(Point[])`、`HuMoments`。
- 直方图与特征：`EqualizeHist`、`CornerHarris`、`CornerMinEigenVal`、`CornerEigenValsAndVecs`、`PreCornerDetect`。

## ABI Notes / ABI 说明

- Native contour output uses a count/fill pattern: the first call queries contour count and total point count, and the second call fills flattened point buffers, contour lengths, and hierarchy values.
- `Moments` crosses the C ABI as a fixed 24-element `double` buffer, then becomes a managed value object.
- `FloodFill` has separate native entries for mask and non-mask overloads.
- `ConnectedComponentsWithStats` writes labels, stats, and centroids into caller-owned `Mat` instances.
- Modern .NET targets use span-based point paths where available; older frameworks keep array fallbacks.

- native 轮廓输出使用 count/fill 两次调用模式：第一次查询轮廓数量和总点数，第二次填充展平点缓冲、每条轮廓长度和层级数组。
- `Moments` 通过固定 24 元素 `double` 缓冲跨越 C ABI，然后在 managed 层变成值对象。
- `FloodFill` 对带 mask 和不带 mask 的重载使用独立 native 入口。
- `ConnectedComponentsWithStats` 将 labels、stats 和 centroids 写入调用方持有的 `Mat`。
- 现代 .NET 目标在可用处使用基于 span 的点集路径；老框架保留数组 fallback。

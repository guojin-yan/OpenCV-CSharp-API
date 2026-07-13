# Features2D Blob And Region Detectors Guide / Features2D 斑点与区域检测指南

This guide covers blob and region detectors. The current implemented objects are `MSER` and `SimpleBlobDetector`.

本指南说明斑点与区域检测器。当前已实现对象包括 `MSER` 和 `SimpleBlobDetector`。

## MSER Object Model / MSER 对象模型

`MSER` inherits from `Feature2D`, so it can be used anywhere a keypoint detector is expected. It also adds `DetectRegions`, which returns `MserRegion` objects containing managed point arrays and bounding boxes.

`MserRegion` is a pure managed region container. Its constructor rejects null point arrays with `ArgumentNullException`, clones the input points, and stores the supplied `BoundingBox`. The `Points` property returns a caller-owned clone, while `PointCount` and `HasPoints` report the stored region size without exposing the internal array.

`MSER` 继承自 `Feature2D`，因此可以在需要关键点检测器的地方使用。它还新增 `DetectRegions`，返回包含 managed 点数组和边界框的 `MserRegion` 对象。

`MserRegion` 是纯 managed 区域容器。构造函数会用 `ArgumentNullException` 拒绝空点数组，克隆输入点集，并保存传入的 `BoundingBox`。`Points` 属性返回调用方拥有的克隆，`PointCount` 和 `HasPoints` 会报告已保存区域大小而不暴露内部数组。

```csharp
using System;
using OpenCvSharp.Core;
using OpenCvSharp.Features2D;

namespace Samples
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat image = new Mat(160, 160, MatType.CV_8UC1, new Scalar(0)))
            using (MSER mser = MSER.Create(delta: 6, minArea: 20, maxArea: 30000))
            {
                MserRegion[] regions = mser.DetectRegions(image);
                KeyPoint[] keypoints = mser.Detect(image);
                Console.WriteLine("regions=" + regions.Length + ", keypoints=" + keypoints.Length);
            }
        }
    }
}
```

## Tunable Parameters / 可调参数

The managed properties mirror the OpenCV MSER knobs while following C# casing:

managed 属性映射 OpenCV MSER 的主要参数，同时使用 C# 命名规范：

- `Delta`
- `MinArea`
- `MaxArea`
- `MaxVariation`
- `MinDiversity`
- `MaxEvolution`
- `AreaThreshold`
- `MinMargin`
- `EdgeBlurSize`
- `Pass2Only`

```csharp
mser.Delta = 7;
mser.MinArea = 32;
mser.MaxArea = 18000;
mser.Pass2Only = false;
```

## ABI Shape / ABI 形状

OpenCV returns region point sets through C++ containers. The native wrapper converts them to POD buffers: region offsets, flattened points, and bounding rectangles. The managed wrapper then reconstructs `MserRegion[]`.

OpenCV 通过 C++ 容器返回区域点集。native 封装会把它们转换为 POD 缓冲：区域 offset、扁平化点数组和边界矩形。managed 封装再重建 `MserRegion[]`。

This keeps STL, `cv::Ptr<>`, and OpenCV object layouts out of the C ABI.

这样可以避免 STL、`cv::Ptr<>` 和 OpenCV 对象布局穿过 C ABI。

## SimpleBlobDetector / SimpleBlobDetector

`SimpleBlobDetector` returns blob centers as `KeyPoint[]` and uses `SimpleBlobDetectorParams` to configure area, color, circularity, inertia, and convexity filters. When `CollectContours` is enabled before detection, `GetBlobContours()` returns the cached blob contours as managed `Point[][]`.

`SimpleBlobDetector` 将斑点中心作为 `KeyPoint[]` 返回，并使用 `SimpleBlobDetectorParams` 配置面积、颜色、圆度、惯性比和凸性过滤。检测前启用 `CollectContours` 后，`GetBlobContours()` 会以 managed `Point[][]` 返回缓存的斑点轮廓。

See [SimpleBlobDetector Guide](features2d-simpleblob-guide.md) for parameter examples.

参数示例见 [SimpleBlobDetector Guide](features2d-simpleblob-guide.md)。

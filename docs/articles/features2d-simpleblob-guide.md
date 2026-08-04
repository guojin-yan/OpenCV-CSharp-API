# SimpleBlobDetector Guide / SimpleBlobDetector 使用指南

`SimpleBlobDetector` wraps OpenCV `cv::SimpleBlobDetector` and uses `SimpleBlobDetectorParams` as a managed parameter object. The native ABI receives a project-owned POD structure instead of the C++ `Params` layout.

`SimpleBlobDetector` 封装 OpenCV `cv::SimpleBlobDetector`，并使用 `SimpleBlobDetectorParams` 作为 managed 参数对象。native ABI 接收项目自有的 POD 结构，而不是 C++ `Params` 布局。

## Parameter Object / 参数对象

`SimpleBlobDetectorParams` starts with OpenCV default values. You can clone or copy it before changing values for a specific detector.

`SimpleBlobDetectorParams` 使用 OpenCV 默认值初始化。可以在为某个检测器调整参数前复制或克隆它。

`Clone()` and the copy constructor create independent managed parameter objects, so later edits to one instance do not mutate the other. The copy constructor rejects a null source with `ArgumentNullException`. `SimpleBlobDetector.Create(SimpleBlobDetectorParams)` also rejects null parameters before native dispatch, and assigning `null` to `SimpleBlobDetector.Parameters` is invalid.

`Clone()` 和复制构造函数会创建独立的 managed 参数对象，因此之后修改其中一个实例不会影响另一个实例。复制构造函数会用 `ArgumentNullException` 拒绝空源对象。`SimpleBlobDetector.Create(SimpleBlobDetectorParams)` 也会在进入 native 前拒绝空参数，向 `SimpleBlobDetector.Parameters` 赋值 `null` 同样是非法的。

```csharp
using System;
using JYPPX.OpenCvSharp.Features2D;

namespace Samples
{
    internal static class Program
    {
        private static void Main()
        {
            SimpleBlobDetectorParams parameters = new SimpleBlobDetectorParams();
            parameters.MinArea = 20.0F;
            parameters.MaxArea = 2500.0F;
            parameters.FilterByCircularity = false;

            SimpleBlobDetectorParams copy = parameters.Clone();
            Console.WriteLine(copy);
        }
    }
}
```

## Detect Blobs / 检测斑点

For bright backgrounds with dark blobs, keep `FilterByColor = true` and `BlobColor = 0`.

对于亮背景上的暗斑点，可以保持 `FilterByColor = true` 和 `BlobColor = 0`。

```csharp
using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Features2D;
using ImgProcCv2 = JYPPX.OpenCvSharp.ImgProc.Cv2;

namespace Samples
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat image = new Mat(160, 160, MatType.CV_8UC1, new Scalar(255)))
            {
                ImgProcCv2.Circle(image, new Point(40, 40), 12, new Scalar(0), -1);
                ImgProcCv2.Circle(image, new Point(112, 48), 16, new Scalar(0), -1);

                SimpleBlobDetectorParams parameters = new SimpleBlobDetectorParams
                {
                    ThresholdStep = 5.0F,
                    MinThreshold = 0.0F,
                    MaxThreshold = 255.0F,
                    MinRepeatability = 1,
                    FilterByColor = true,
                    BlobColor = 0,
                    FilterByArea = true,
                    MinArea = 20.0F,
                    MaxArea = 2500.0F,
                    FilterByCircularity = false,
                    FilterByInertia = false,
                    FilterByConvexity = false
                };

                using (SimpleBlobDetector detector = SimpleBlobDetector.Create(parameters))
                {
                    KeyPoint[] keypoints = detector.Detect(image);
                    Console.WriteLine("blobs=" + keypoints.Length);
                }
            }
        }
    }
}
```

## Collected Contours / 收集轮廓

OpenCV can cache the contour for each detected blob when `CollectContours` is enabled. Call `Detect` first, then read the cached contours with `GetBlobContours()`.

启用 `CollectContours` 后，OpenCV 可以缓存每个斑点的轮廓。先调用 `Detect`，再通过 `GetBlobContours()` 读取缓存轮廓。

```csharp
using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Features2D;
using ImgProcCv2 = JYPPX.OpenCvSharp.ImgProc.Cv2;

namespace Samples
{
    internal static class Program
    {
        private static void Main()
        {
            SimpleBlobDetectorParams parameters = new SimpleBlobDetectorParams
            {
                CollectContours = true,
                FilterByColor = true,
                BlobColor = 0,
                FilterByArea = true,
                MinArea = 20.0F,
                MaxArea = 2500.0F
            };

            using (Mat image = new Mat(160, 160, MatType.CV_8UC1, new Scalar(255)))
            using (SimpleBlobDetector detector = SimpleBlobDetector.Create(parameters))
            {
                ImgProcCv2.Circle(image, new Point(40, 40), 12, new Scalar(0), -1);
                ImgProcCv2.Circle(image, new Point(112, 48), 16, new Scalar(0), -1);

                KeyPoint[] keypoints = detector.Detect(image);
                Point[][] contours = detector.GetBlobContours();
                Console.WriteLine("blobs=" + keypoints.Length + ", contours=" + contours.Length);
            }
        }
    }
}
```

## Feature2D Base / Feature2D 基类

`SimpleBlobDetector` inherits from `Feature2D`, so it participates in the same batch-detect model as ORB, SIFT, FAST, GFTT, and MSER.

`SimpleBlobDetector` 继承自 `Feature2D`，因此与 ORB、SIFT、FAST、GFTT 和 MSER 使用同一套批量检测模型。

```csharp
Feature2D feature = SimpleBlobDetector.Create(parameters);
KeyPoint[][] batch = feature.Detect(new[] { image1, image2 });
```

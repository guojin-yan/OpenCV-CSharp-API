# ORB Guide / ORB 使用指南

`ORB` wraps OpenCV `cv::ORB` as a disposable managed object. The property names follow .NET casing while keeping OpenCV terminology: `MaxFeatures`, `ScaleFactor`, `NLevels`, `EdgeThreshold`, `WtaK`, `ScoreType`, `PatchSize`, and `FastThreshold`.

`ORB` 将 OpenCV `cv::ORB` 封装为可释放的 managed 对象。属性名称采用 .NET 大小写规范，同时保留 OpenCV 术语：`MaxFeatures`、`ScaleFactor`、`NLevels`、`EdgeThreshold`、`WtaK`、`ScoreType`、`PatchSize`、`FastThreshold`。

`ScoreType` accepts only `OrbScoreType.HarrisScore` and `OrbScoreType.FastScore`. Unknown enum values throw `ArgumentOutOfRangeException` before native ORB creation or before setting the property.

`ScoreType` 只接受 `OrbScoreType.HarrisScore` 和 `OrbScoreType.FastScore`。未知枚举值会在创建 native ORB 前或设置属性前抛出 `ArgumentOutOfRangeException`。

## Example / 示例

```csharp
using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Features2D;
using JYPPX.OpenCvSharp.ImgProc;
using Features2DCv2 = JYPPX.OpenCvSharp.Features2D.Cv2;
using ImgProcCv2 = JYPPX.OpenCvSharp.ImgProc.Cv2;

namespace Samples
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat image = new Mat(64, 64, MatType.CV_8UC1))
            using (Mat descriptors = new Mat())
            using (Mat drawing = new Mat())
            using (ORB orb = ORB.Create(maxFeatures: 128, fastThreshold: 8))
            {
                image.SetTo(new Scalar(0));
                ImgProcCv2.Rectangle(image, new Rect(8, 8, 24, 24), new Scalar(255), -1);
                ImgProcCv2.Circle(image, new Point(44, 44), 10, new Scalar(255), -1);

                KeyPoint[] keypoints = orb.Detect(image);
                KeyPoint[] keptKeypoints = orb.Compute(image, keypoints, descriptors);

                Features2DCv2.DrawKeypoints(
                    image,
                    keptKeypoints,
                    drawing,
                    new Scalar(0, 255, 0),
                    DrawMatchesFlags.DrawRichKeypoints);

                Console.WriteLine("keypoints=" + keptKeypoints.Length + ", descriptors=" + descriptors.Rows);
            }
        }
    }
}
```

## Native Availability / native 可用性

If the runtime package was built without OpenCV `features`, `ORB.Create()` throws `OpenCvException` instead of silently degrading. This is intentional: feature detection depends on native OpenCV implementation and must not pretend to run with an incomplete backend.

如果 runtime 包未包含 OpenCV `features`，`ORB.Create()` 会抛出 `OpenCvException`，不会静默降级。这是刻意设计：特征检测依赖 native OpenCV 实现，不能在后端不完整时假装执行成功。

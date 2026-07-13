# Video Background Subtractor Guide

`OpenCvSharp.Video` now wraps OpenCV background subtraction through `BackgroundSubtractor`, `BackgroundSubtractorMOG2`, and `BackgroundSubtractorKNN`. These objects live behind opaque native handles and require the factual OpenCV 5.0.0 runtime artifact `opencv_video500.dll` in a linked runtime.

`OpenCvSharp.Video` 现在通过 `BackgroundSubtractor`、`BackgroundSubtractorMOG2` 和 `BackgroundSubtractorKNN` 封装 OpenCV 背景减除能力。这些对象由 native opaque handle 持有，在 linked runtime 中需要事实性 OpenCV 5.0.0 runtime 产物 `opencv_video500.dll`。

## Covered APIs / 已覆盖接口

- `BackgroundSubtractor.Apply`
- `BackgroundSubtractor.ApplyWithKnownForeground`
- `BackgroundSubtractor.GetBackgroundImage`
- `BackgroundSubtractorMOG2.Create`
- `BackgroundSubtractorKNN.Create`
- MOG2 properties: `History`, `NMixtures`, `DetectShadows`, `ShadowValue`, `ShadowThreshold`, `BackgroundRatio`, `VarThreshold`, `VarThresholdGen`, `VarInit`, `VarMin`, `VarMax`, `ComplexityReductionThreshold`
- KNN properties: `History`, `NSamples`, `KnnSamples`, `DetectShadows`, `ShadowValue`, `ShadowThreshold`, `Dist2Threshold`

- `BackgroundSubtractor.Apply`
- `BackgroundSubtractor.ApplyWithKnownForeground`
- `BackgroundSubtractor.GetBackgroundImage`
- `BackgroundSubtractorMOG2.Create`
- `BackgroundSubtractorKNN.Create`
- MOG2 属性：`History`、`NMixtures`、`DetectShadows`、`ShadowValue`、`ShadowThreshold`、`BackgroundRatio`、`VarThreshold`、`VarThresholdGen`、`VarInit`、`VarMin`、`VarMax`、`ComplexityReductionThreshold`
- KNN 属性：`History`、`NSamples`、`KnnSamples`、`DetectShadows`、`ShadowValue`、`ShadowThreshold`、`Dist2Threshold`

## Tiny Synthetic Frames / 小型合成帧

```csharp
using OpenCvSharp.Core;
using OpenCvSharp.ImgProc;
using OpenCvSharp.Video;
using ImgProcCv2 = OpenCvSharp.ImgProc.Cv2;

namespace VideoBackgroundSample
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat frame0 = new Mat(32, 32, MatType.CV_8UC1, new Scalar(0)))
            using (Mat frame1 = new Mat(32, 32, MatType.CV_8UC1, new Scalar(0)))
            using (BackgroundSubtractorMOG2 mog2 = BackgroundSubtractorMOG2.Create(history: 16))
            using (BackgroundSubtractorKNN knn = BackgroundSubtractorKNN.Create(history: 16))
            {
                ImgProcCv2.Rectangle(frame1, new Rect(8, 8, 12, 12), new Scalar(255), -1);

                using (Mat mog2Mask = mog2.Apply(frame0))
                using (Mat knnMask = knn.Apply(frame1))
                {
                    System.Console.WriteLine("MOG2=" + mog2Mask.Size + ", KNN=" + knnMask.Size);
                }
            }
        }
    }
}
```

`ApplyWithKnownForeground` maps to the OpenCV overload that accepts a known foreground mask. In C#, the returning helper is named separately to avoid an overload conflict with the output-`Mat` form.

`ApplyWithKnownForeground` 对应 OpenCV 中接收已知前景掩码的重载。C# 中返回 `Mat` 的 helper 使用独立名称，以避免和输出 `Mat` 形式发生重载冲突。

## ABI And Runtime / ABI 与运行时

The native ABI exports base and derived handles separately but never exposes `cv::Ptr`, `cv::InputArray`, `cv::OutputArray`, STL containers, or C++ object layout. Stub builds keep the exported functions and report `NOT_LINKED`.

native ABI 分别导出基类和派生类 handle，但不会暴露 `cv::Ptr`、`cv::InputArray`、`cv::OutputArray`、STL 容器或 C++ 对象布局。stub build 保留导出函数并返回 `NOT_LINKED`。

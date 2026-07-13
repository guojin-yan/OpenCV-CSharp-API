# Video Motion Guide

`OpenCvSharp.Video` exposes OpenCV `video` module motion APIs: sparse Lucas-Kanade optical flow, dense Farneback optical flow, optical-flow pyramids, mean-shift tracking, CamShift tracking, background subtraction objects, and `KalmanFilter`. These APIs require the factual OpenCV 5.0.0 runtime artifact `opencv_video500.dll` when the native wrapper is linked to OpenCV.

`OpenCvSharp.Video` 暴露 OpenCV `video` 模块运动能力：稀疏 Lucas-Kanade 光流、密集 Farneback 光流、光流金字塔、mean-shift 跟踪、CamShift 跟踪、背景减除对象和 `KalmanFilter`。当 native wrapper 链接真实 OpenCV 时，这些 API 需要事实性 OpenCV 5.0.0 runtime 产物 `opencv_video500.dll`。

## Covered APIs / 已覆盖接口

- `Cv2.CalcOpticalFlowPyrLK`
- `Cv2.CalcOpticalFlowFarneback`
- `Cv2.BuildOpticalFlowPyramid`
- `Cv2.MeanShift`
- `Cv2.CamShift`
- `OpticalFlowFlags`
- `OpticalFlowPyramidResult`
- `MeanShiftResult`
- `CamShiftResult`
- `BackgroundSubtractor`, `BackgroundSubtractorMOG2`, `BackgroundSubtractorKNN`
- `KalmanFilter`

- `Cv2.CalcOpticalFlowPyrLK`
- `Cv2.CalcOpticalFlowFarneback`
- `Cv2.BuildOpticalFlowPyramid`
- `Cv2.MeanShift`
- `Cv2.CamShift`
- `OpticalFlowFlags`
- `OpticalFlowPyramidResult`
- `MeanShiftResult`
- `CamShiftResult`
- `BackgroundSubtractor`、`BackgroundSubtractorMOG2`、`BackgroundSubtractorKNN`
- `KalmanFilter`

## Optical Flow / 光流

```csharp
using OpenCvSharp.Core;
using VideoCv2 = OpenCvSharp.Video.Cv2;

namespace VideoMotionSample
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat prev = new Mat(32, 32, MatType.CV_8UC1, new Scalar(0)))
            using (Mat next = new Mat(32, 32, MatType.CV_8UC1, new Scalar(0)))
            using (Mat flow = new Mat())
            {
                Point2f[] points = { new Point2f(12.0F, 12.0F) };
                Point2f[] tracked = VideoCv2.CalcOpticalFlowPyrLK(prev, next, points, out byte[] status, out float[] err);

                VideoCv2.CalcOpticalFlowFarneback(prev, next, flow, 0.5, 1, 5, 1, 5, 1.1);
                System.Console.WriteLine("LK=" + tracked.Length + ", status=" + status.Length + ", flow=" + flow.Size);
            }
        }
    }
}
```

The native ABI accepts flat point buffers and caller-owned output arrays. It does not expose `std::vector`, `cv::InputArray`, or `cv::OutputArray`.

native ABI 使用平铺点缓冲和调用方持有的输出数组，不暴露 `std::vector`、`cv::InputArray` 或 `cv::OutputArray`。

## Tracking Windows / 跟踪窗口

```csharp
using OpenCvSharp.Core;
using VideoCv2 = OpenCvSharp.Video.Cv2;

namespace VideoShiftSample
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat probability = new Mat(64, 64, MatType.CV_8UC1, new Scalar(0)))
            {
                Rect window = new Rect(8, 8, 24, 24);
                MeanShiftResult meanShift = VideoCv2.MeanShift(probability, window, TermCriteria.ByCountAndEpsilon(10, 1.0));
                CamShiftResult camShift = VideoCv2.CamShift(probability, window, TermCriteria.ByCountAndEpsilon(10, 1.0));

                System.Console.WriteLine("MeanShift iterations=" + meanShift.Iterations + ", CamShift angle=" + camShift.Box.Angle);
            }
        }
    }
}
```

## Runtime Notes / 运行时说明

Default tests cover enum values, result shape, parameter validation, and no-OpenCV `NOT_LINKED` behavior. Real optical-flow behavior requires the factual OpenCV 5.0.0 runtime artifact `opencv_video500.dll` and valid image data. Linked smoke is guarded by `OPENCV_CSHARP_NATIVE_SMOKE=1`; the older `OPENCV5SHARP_NATIVE_SMOKE=1` name remains accepted only as an existing-smoke-workflow compatibility alias.

默认测试覆盖枚举值、结果形状、参数校验和 no-OpenCV 下的 `NOT_LINKED` 行为。真实光流效果需要事实性 OpenCV 5.0.0 runtime 产物 `opencv_video500.dll` 并提供有效图像数据。linked smoke 通过 `OPENCV_CSHARP_NATIVE_SMOKE=1` 保护；旧的 `OPENCV5SHARP_NATIVE_SMOKE=1` 名称仍仅作为既有 smoke workflow 的兼容别名使用。

For background subtraction details, see [Video Background Subtractor Guide](video-background-subtractor-guide.md). Motion-template functions were rechecked against the local OpenCV 5.0.0 public headers and are documented in [Video Motion Template Guide](video-motion-template-guide.md).

背景减除对象详见 [Video Background Subtractor Guide](video-background-subtractor-guide.md)。motion-template 函数已按本地 OpenCV 5.0.0 public header 重新核对，结论见 [Video Motion Template Guide](video-motion-template-guide.md)。

# XImgProc Disparity Guide / XImgProc Disparity 指南

`JYPPX.OpenCvSharp.XImgProc` includes the second-batch disparity helpers from OpenCV contrib `ximgproc`.

`JYPPX.OpenCvSharp.XImgProc` 已包含 OpenCV contrib `ximgproc` 第二批 disparity 相关能力。

## Scope / 范围

- `DisparityFilter`: base wrapper for disparity post-processing filters.
- `DisparityWLSFilter`: weighted least-squares disparity filter with `Lambda`, `SigmaColor`, `LrcThreshold`, `DepthDiscontinuityRadius`, `ROI`, and confidence-map output.
- `StereoMatcher`: owned generic adapter returned by the XImgProc right-matcher factories.
- `XImgProcCv2.CreateDisparityWLSFilterGeneric`: creates a WLS filter without sharing ownership with a stereo matcher.
- `XImgProcCv2.CreateDisparityWLSFilter`: creates a confidence-enabled WLS filter from `StereoBM`, `StereoSGBM`, or an owned `StereoMatcher`.
- `XImgProcCv2.CreateRightMatcher`: creates an owned right-view matcher from `StereoBM`, `StereoSGBM`, or another owned `StereoMatcher`.
- `XImgProcCv2.GetDisparityVis`: creates a display-friendly disparity image.
- `XImgProcCv2.ComputeMSE` and `ComputeBadPixelPercent`: compute ROI-scoped disparity metrics.

- `DisparityFilter`：disparity 后处理滤波器基类。
- `DisparityWLSFilter`：weighted least-squares disparity filter，包含 `Lambda`、`SigmaColor`、`LrcThreshold`、`DepthDiscontinuityRadius`、`ROI` 和 confidence map 输出。
- `StereoMatcher`：由 XImgProc 右 matcher 工厂返回并独立持有所有权的通用 adapter。
- `XImgProcCv2.CreateDisparityWLSFilterGeneric`：创建不共享 stereo matcher 所有权的 WLS 滤波器。
- `XImgProcCv2.CreateDisparityWLSFilter`：从 `StereoBM`、`StereoSGBM` 或 owned `StereoMatcher` 创建启用置信度的 WLS 滤波器。
- `XImgProcCv2.CreateRightMatcher`：从 `StereoBM`、`StereoSGBM` 或另一个 owned `StereoMatcher` 创建 owned 右视图 matcher。
- `XImgProcCv2.GetDisparityVis`：生成便于显示的 disparity 图。
- `XImgProcCv2.ComputeMSE` 与 `ComputeBadPixelPercent`：计算 ROI 内 disparity 指标。

## Matcher Bridge / Matcher Bridge

`StereoBM` and `StereoSGBM` remain sealed, independently owned public wrappers. `StereoMatcher` is not a new base class for them and has no public constructor or independent `Create` method. It is an owned adapter for matcher results returned by `CreateRightMatcher`.

The native bridge copies the source `cv::Ptr<cv::StereoMatcher>` reference-counted ownership into an independent wrapper. It does not borrow, move, or invalidate the source matcher's pointer. The source matcher, returned right matcher, and WLS filter can therefore be disposed independently.

`StereoBM` 和 `StereoSGBM` 继续保持 sealed、独立持有所有权的公开 wrapper。`StereoMatcher` 不是它们的新基类，也没有公开构造函数或独立的 `Create` 方法；它只表示 `CreateRightMatcher` 返回的 owned matcher adapter。

native bridge 会把源 `cv::Ptr<cv::StereoMatcher>` 的引用计数所有权复制到独立 wrapper 中，不会借用、移动或使源 matcher 的指针失效。因此源 matcher、返回的右 matcher 和 WLS filter 可以相互独立地释放。

## Right Matcher Rules / 右 Matcher 规则

- A right matcher created from `StereoBM` accepts only `CV_8UC1`.
- A right matcher created from `StereoSGBM` accepts `CV_8UC1` and `CV_8UC3`.
- Creating another right matcher from an owned `StereoMatcher` preserves that input capability.
- OpenCV computes the right minimum disparity as `-left.MinDisparity - left.NumDisparities + 1`.
- OpenCV copies `NumDisparities` and `BlockSize`, but it does not copy every shared property. The factory resets `SpeckleWindowSize` and `Disp12MaxDiff` and leaves `SpeckleRange` at the concrete matcher's constructor default. Set these properties explicitly on the returned adapter when required.

- 从 `StereoBM` 创建的右 matcher 只接受 `CV_8UC1`。
- 从 `StereoSGBM` 创建的右 matcher 接受 `CV_8UC1` 和 `CV_8UC3`。
- 从 owned `StereoMatcher` 再创建右 matcher 时，会保留该输入能力。
- OpenCV 按 `-left.MinDisparity - left.NumDisparities + 1` 计算右 matcher 的最小视差。
- OpenCV 会复制 `NumDisparities` 和 `BlockSize`，但不会复制所有共享属性。工厂会重置 `SpeckleWindowSize` 和 `Disp12MaxDiff`，`SpeckleRange` 则保持具体 matcher 构造时的默认值；需要时应在返回的 adapter 上显式设置。

## Input Notes / 输入说明

Stereo inputs must be non-empty rectified images with identical size and type. Disparity maps commonly use `CV_16SC1` values scaled by 16. The guide image is caller-owned and should match the filtered disparity size. For confidence-enabled filtering, pass both left and right disparity maps and the right-view image. Pass an explicit `Rect` ROI when evaluating metrics or filtering a known valid area.

stereo 输入必须是非空、已校正并且尺寸与类型完全一致的图像。Disparity map 常见类型为 `CV_16SC1`，数值按 16 缩放。guide 图由调用方持有，尺寸应与被滤波 disparity 一致。使用 confidence-enabled filter 时，应同时传入左右 disparity map 和右视图图像。评估指标或只滤波有效区域时，请传入明确的 `Rect` ROI。

## Example / 示例

```csharp
using JYPPX.OpenCvSharp.Calib3D;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.XImgProc;

namespace XImgProcDisparityExample
{
    internal static class Program
    {
        private static void Main()
        {
            const int blockSize = 3;
            using Mat left = new Mat(48, 96, MatType.CV_8UC1, new Scalar(96));
            using Mat right = left.Clone();
            using StereoSGBM leftMatcher = StereoSGBM.Create(
                minDisparity: 0,
                numDisparities: 16,
                blockSize: blockSize,
                p1: 8 * blockSize * blockSize,
                p2: 32 * blockSize * blockSize);
            using StereoMatcher rightMatcher = XImgProcCv2.CreateRightMatcher(leftMatcher);
            using DisparityWLSFilter wls = XImgProcCv2.CreateDisparityWLSFilter(leftMatcher);
            using Mat leftDisparity = leftMatcher.Compute(left, right);
            using Mat rightDisparity = rightMatcher.Compute(right, left);
            using Mat filtered = new Mat();

            wls.Lambda = 8000.0;
            wls.SigmaColor = 1.5;
            wls.Filter(
                leftDisparity,
                left,
                filtered,
                rightDisparity,
                new Rect(0, 0, left.Cols, left.Rows),
                right);
            using Mat visual = XImgProcCv2.GetDisparityVis(filtered);

            double mse = XImgProcCv2.ComputeMSE(leftDisparity, filtered, new Rect(0, 0, left.Cols, left.Rows));
            double bad = XImgProcCv2.ComputeBadPixelPercent(leftDisparity, filtered, new Rect(0, 0, left.Cols, left.Rows));
        }
    }
}
```

## Smoke / Smoke

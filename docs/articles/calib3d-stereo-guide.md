# Calib3D StereoBM Guide

`StereoBM` wraps OpenCV `cv::StereoBM` as a managed disposable object. The C ABI owns a small opaque handle around `cv::Ptr<cv::StereoBM>`, and the public C# class exposes OpenCV-style properties with .NET naming.

`StereoBM` 将 OpenCV `cv::StereoBM` 封装成 managed 可释放对象。C ABI 持有一个围绕 `cv::Ptr<cv::StereoBM>` 的不透明句柄，公开 C# 类则以 .NET 命名方式暴露接近 OpenCV 的属性。

## Object Model / 对象模型

- Create an instance with `StereoBM.Create`.
- Configure integer properties such as `NumDisparities`, `BlockSize`, `TextureThreshold`, and `UniquenessRatio`.
- Use `ROI1` and `ROI2` to pass valid regions from stereo rectification.
- Call `Compute` with rectified single-channel left/right images.
- Dispose the object when it is no longer needed.

- 使用 `StereoBM.Create` 创建实例。
- 配置 `NumDisparities`、`BlockSize`、`TextureThreshold`、`UniquenessRatio` 等整数属性。
- 通过 `ROI1` 和 `ROI2` 传入双目校正后的有效区域。
- 对已校正的单通道左右图调用 `Compute`。
- 使用完成后释放对象。

## Example / 示例

```csharp
using JYPPX.OpenCvSharp.Calib3D;
using JYPPX.OpenCvSharp.Core;

namespace StereoBMSample
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat left = new Mat(120, 160, MatType.CV_8UC1))
            using (Mat right = new Mat(120, 160, MatType.CV_8UC1))
            using (StereoBM stereo = StereoBM.Create(numDisparities: 16, blockSize: 9))
            {
                left.SetTo(new Scalar(40));
                right.SetTo(new Scalar(40));

                stereo.MinDisparity = 0;
                stereo.NumDisparities = 16;
                stereo.BlockSize = 9;
                stereo.PreFilterType = StereoBMPreFilterType.XSobel;
                stereo.TextureThreshold = 10;
                stereo.UniquenessRatio = 15;
                stereo.SpeckleWindowSize = 0;
                stereo.SpeckleRange = 0;

                using (Mat disparity = stereo.Compute(left, right))
                {
                    System.Console.WriteLine("Disparity rows: " + disparity.Rows);
                }
            }
        }
    }
}
```

## Disparity Scale / 视差缩放

OpenCV `StereoBM` returns fixed-point disparity values. `StereoBM.DispShift` and `StereoBM.DispScale` are exposed to mirror OpenCV constants and make conversion explicit.

OpenCV `StereoBM` 返回定点视差值。`StereoBM.DispShift` 和 `StereoBM.DispScale` 暴露了对应 OpenCV 常量，方便显式转换。

```csharp
using JYPPX.OpenCvSharp.Calib3D;
using JYPPX.OpenCvSharp.Core;

namespace DisparityScaleSample
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat left = new Mat(32, 32, MatType.CV_8UC1))
            using (Mat right = new Mat(32, 32, MatType.CV_8UC1))
            using (StereoBM stereo = StereoBM.Create(16, 9))
            using (Mat disparity = stereo.Compute(left, right))
            {
                double scale = 1.0 / StereoBM.DispScale;
                System.Console.WriteLine("Scale for floating disparity: " + scale);
                System.Console.WriteLine("Disparity type: " + disparity.Type);
            }
        }
    }
}
```

## Rectification Flow / 校正流程

For a complete stereo pipeline, use `Calib3D.Cv2.StereoCalibrate`, then `StereoRectify` or `StereoRectifyUncalibrated` before block matching. The resulting valid pixel ROIs can be assigned to `ROI1` and `ROI2`.

完整双目流程中，应先使用 `Calib3D.Cv2.StereoCalibrate`，再使用 `StereoRectify` 或 `StereoRectifyUncalibrated` 进行校正，最后执行块匹配。校正得到的有效像素 ROI 可以赋值给 `ROI1` 和 `ROI2`。

```csharp
using JYPPX.OpenCvSharp.Calib3D;
using JYPPX.OpenCvSharp.Core;

namespace StereoROIConfigurationSample
{
    internal static class Program
    {
        private static void Main()
        {
            using (StereoBM stereo = StereoBM.Create(16, 9))
            {
                stereo.ROI1 = new Rect(0, 0, 320, 240);
                stereo.ROI2 = new Rect(0, 0, 320, 240);

                System.Console.WriteLine("ROI width: " + stereo.ROI1.Width);
            }
        }
    }
}
```

## Runtime Notes / 运行时说明

The native wrapper links OpenCV `stereo` and must be distributed with the matching OpenCV runtime libraries for the selected RID. The runtime package naming remains `JYPPX.OpenCV.runtime.<rid>`, and the OpenCV runtime identity is carried by four-part package version metadata such as `5.0.0.0`.

native 封装链接 OpenCV `stereo`，必须随目标 RID 分发匹配的 OpenCV runtime 库。runtime 包名仍保持 `JYPPX.OpenCV.runtime.<rid>`，OpenCV runtime 身份由四段 package version 元数据承载，例如 `5.0.0.0`。

`StereoBM` is a native stateful object. Keep one instance per configuration or thread-local workflow unless a higher-level synchronization layer is added by the caller.

`StereoBM` 是有 native 状态的对象。建议按配置或线程局部流程持有实例，除非调用方自行增加更高层同步。

# Stitching Stitcher Guide

`JYPPX.OpenCvSharp.Stitching` wraps the high-level OpenCV `cv::Stitcher` object. The wrapper focuses on the stable public `Stitcher` surface: mode/status enums, pipeline scale properties, wave correction, stitch/estimate/compose calls, component indices, camera parameters, and result mask access.

`JYPPX.OpenCvSharp.Stitching` 封装 OpenCV 高层 `cv::Stitcher` 对象。当前 wrapper 聚焦稳定的 public `Stitcher` 接口：mode/status 枚举、pipeline 尺度属性、波形校正、stitch/estimate/compose 调用、component 索引、相机参数和 result mask 访问。

## Covered APIs / 已覆盖接口

- `Stitcher.Create`
- `Stitcher.EstimateTransform`
- `Stitcher.ComposePanorama`
- `Stitcher.Stitch`
- `Stitcher.GetComponent`
- `Stitcher.GetCameras`
- `Stitcher.GetResultMask`
- `RegistrationResol`, `SeamEstimationResol`, `CompositingResol`, `PanoConfidenceThresh`
- `WaveCorrection`, `InterpolationFlags`, `WaveCorrectKind`, `WorkScale`
- `StitcherMode`, `StitcherStatus`, `WaveCorrectKind`, `StitcherCameraParams`

- `Stitcher.Create`
- `Stitcher.EstimateTransform`
- `Stitcher.ComposePanorama`
- `Stitcher.Stitch`
- `Stitcher.GetComponent`
- `Stitcher.GetCameras`
- `Stitcher.GetResultMask`
- `RegistrationResol`、`SeamEstimationResol`、`CompositingResol`、`PanoConfidenceThresh`
- `WaveCorrection`、`InterpolationFlags`、`WaveCorrectKind`、`WorkScale`
- `StitcherMode`、`StitcherStatus`、`WaveCorrectKind`、`StitcherCameraParams`

## Minimal Call Shape / 最小调用形态

```csharp
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Stitching;

namespace StitchingSample
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat first = new Mat(32, 48, MatType.CV_8UC3, new Scalar(0)))
            using (Mat second = new Mat(32, 48, MatType.CV_8UC3, new Scalar(0)))
            using (Mat pano = new Mat())
            using (Stitcher stitcher = Stitcher.Create(StitcherMode.Panorama))
            {
                stitcher.RegistrationResol = 0.2;
                StitcherStatus status = stitcher.Stitch(new[] { first, second }, pano);

                System.Console.WriteLine("status=" + status + ", pano=" + pano.Size);
            }
        }
    }
}
```

Tiny synthetic images are useful for ABI and status checks, but they are not a promise that OpenCV will produce a successful panorama. Real stitching depends on overlap, feature quality, camera motion, and the selected OpenCV pipeline.

小型合成图像适合验证 ABI 和状态返回，但不代表 OpenCV 一定能拼接成功。真实拼接效果依赖重叠区域、特征质量、相机运动和所选 OpenCV pipeline。

## ABI Notes / ABI 说明

The C ABI stores `cv::Ptr<cv::Stitcher>` behind an opaque handle. Image and mask arrays cross the boundary as caller-owned arrays of opaque `Mat` handles. `std::vector<cv::Mat>`, `InputArrayOfArrays`, and `cv::Ptr` stay inside native code. Camera parameter output uses count/fill calls and returns owned `Mat` handles for rotation and translation matrices.

C ABI 将 `cv::Ptr<cv::Stitcher>` 保存在 opaque handle 后面。图像和掩码数组通过调用方持有的 opaque `Mat` handle 数组跨边界传递。`std::vector<cv::Mat>`、`InputArrayOfArrays` 和 `cv::Ptr` 留在 native 代码内部。相机参数输出使用 count/fill 调用，并为旋转和平移矩阵返回 owned `Mat` handle。

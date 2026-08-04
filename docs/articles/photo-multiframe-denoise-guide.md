# Photo Multi-Frame Denoise Guide

`PhotoCv2.FastNlMeansDenoisingMulti` and `PhotoCv2.FastNlMeansDenoisingColoredMulti` denoise a selected frame from a short temporal sequence. The managed API accepts `Mat[]` on all frameworks and `ReadOnlySpan<Mat>` on modern .NET targets.

`PhotoCv2.FastNlMeansDenoisingMulti` 和 `PhotoCv2.FastNlMeansDenoisingColoredMulti` 会从短时序图像序列中选取一帧进行去噪。managed API 在所有框架上支持 `Mat[]`，并在现代 .NET 目标上支持 `ReadOnlySpan<Mat>`。

## APIs / API

- `PhotoCv2.FastNlMeansDenoisingMulti(Mat[] srcImages, ...)`
- `PhotoCv2.FastNlMeansDenoisingMulti(Mat[] srcImages, ..., float[] h, ...)`
- `PhotoCv2.FastNlMeansDenoisingColoredMulti(Mat[] srcImages, ...)`
- Modern .NET span overloads for `ReadOnlySpan<Mat>` and `ReadOnlySpan<float>`.

- `PhotoCv2.FastNlMeansDenoisingMulti(Mat[] srcImages, ...)`
- `PhotoCv2.FastNlMeansDenoisingMulti(Mat[] srcImages, ..., float[] h, ...)`
- `PhotoCv2.FastNlMeansDenoisingColoredMulti(Mat[] srcImages, ...)`
- 现代 .NET 的 `ReadOnlySpan<Mat>` 和 `ReadOnlySpan<float>` 重载。

## Example / 示例

```csharp
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Photo;

namespace PhotoMultiFrameDenoiseSample
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat frame0 = new Mat(64, 64, MatType.CV_8UC1, new Scalar(30)))
            using (Mat frame1 = new Mat(64, 64, MatType.CV_8UC1, new Scalar(32)))
            using (Mat frame2 = new Mat(64, 64, MatType.CV_8UC1, new Scalar(31)))
            using (Mat denoised = new Mat())
            {
                PhotoCv2.FastNlMeansDenoisingMulti(
                    new[] { frame0, frame1, frame2 },
                    denoised,
                    imgToDenoiseIndex: 1,
                    temporalWindowSize: 3);
            }
        }
    }
}
```

## Runtime Notes / 运行时说明

Multi-frame denoise requires the factual OpenCV 5.0.0 runtime artifact `opencv_photo500.dll`. The C ABI receives a short-lived array of `jyppx_ocv_mat*` handles and validates each element before converting to OpenCV `std::vector<cv::Mat>` internally.

多帧去噪需要事实性 OpenCV 5.0.0 runtime 产物 `opencv_photo500.dll`。C ABI 接收短生命周期的 `jyppx_ocv_mat*` 句柄数组，并在内部转换为 OpenCV `std::vector<cv::Mat>` 前验证每个元素。

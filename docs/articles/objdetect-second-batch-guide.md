# ObjDetect Second Batch Guide

`OpenCvSharp.ObjDetect` now includes the second main-module graphical-code batch from OpenCV 5.0.0: `BarcodeDetector`, `QRCodeDetectorAruco`, and `QRCodeEncoder`. These APIs live in the main OpenCV `objdetect` module and are staged with the factual OpenCV 5.0.0 runtime artifact `opencv_objdetect500.dll`.

`OpenCvSharp.ObjDetect` 现在包含 OpenCV 5.0.0 主线图形码第二批能力：`BarcodeDetector`、`QRCodeDetectorAruco` 和 `QRCodeEncoder`。这些 API 属于 OpenCV 主仓库 `objdetect` 模块，并随事实性 OpenCV 5.0.0 runtime 产物 `opencv_objdetect500.dll` 暂存。

## Covered APIs / 已覆盖接口

- `BarcodeDetector`: default creation, super-resolution model path creation, `Detect`, `Decode`, `DecodeWithType`, `DetectAndDecode`, `DetectAndDecodeWithType`, `DownsamplingThreshold`, `GradientThreshold`, `GetDetectorScales`, and `SetDetectorScales`.
- `BarcodeDecodeResult`: success flag, decoded strings, decoded barcode type names, and points matrix.
- `QRCodeDetectorAruco`: default and parameterized creation, `GetDetectorParameters`, `SetDetectorParameters`, `Detect`, `Decode`, `DetectAndDecode`, `DetectMulti`, `DecodeMulti`, and `DetectAndDecodeMulti`.
- `QRCodeDetectorArucoParams`: flattened OpenCV parameter fields used by the local OpenCV 5.0.0 header.
- `QRCodeEncoder`: default and parameterized creation, `Encode`, and `EncodeStructuredAppend`.
- `QRCodeEncoderParams`, `QRCodeEncoderCorrectionLevel`, and `QRCodeEncoderEncodeMode`.

- `BarcodeDetector`：默认创建、超分辨率模型路径创建、`Detect`、`Decode`、`DecodeWithType`、`DetectAndDecode`、`DetectAndDecodeWithType`、`DownsamplingThreshold`、`GradientThreshold`、`GetDetectorScales` 和 `SetDetectorScales`。
- `BarcodeDecodeResult`：成功标志、解码字符串、条形码类型名称和 points 矩阵。
- `QRCodeDetectorAruco`：默认和带参数创建、`GetDetectorParameters`、`SetDetectorParameters`、`Detect`、`Decode`、`DetectAndDecode`、`DetectMulti`、`DecodeMulti` 和 `DetectAndDecodeMulti`。
- `QRCodeDetectorArucoParams`：按本地 OpenCV 5.0.0 头文件平铺的参数字段。
- `QRCodeEncoder`：默认和带参数创建、`Encode` 和 `EncodeStructuredAppend`。
- `QRCodeEncoderParams`、`QRCodeEncoderCorrectionLevel` 和 `QRCodeEncoderEncodeMode`。

## Barcode Detection / 条形码检测

`BarcodeDetector` can be created without a model. A super-resolution ONNX model path can also be supplied when the application wants OpenCV's super-resolution barcode path.

`BarcodeDetector` 可在不提供模型的情况下创建。如果应用需要 OpenCV 的超分辨率条形码路径，也可以提供 ONNX 模型路径。

```csharp
using OpenCvSharp.Core;
using OpenCvSharp.ImgCodecs;
using OpenCvSharp.ObjDetect;

namespace ObjDetectBarcodeSample
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat image = Cv2.ImRead("barcode.png", ImreadModes.Color))
            using (BarcodeDetector detector = BarcodeDetector.Create())
            {
                detector.DownsamplingThreshold = 512.0;
                detector.GradientThreshold = 64.0;
                detector.SetDetectorScales(new[] { 0.01f, 0.03f, 0.06f, 0.08f });

                BarcodeDecodeResult result = detector.DetectAndDecodeWithType(image);
                System.Console.WriteLine("Success=" + result.Success + ", count=" + result.DecodedInfo.Length);

                if (result.Points != null)
                {
                    result.Points.Dispose();
                }
            }
        }
    }
}
```

`DetectAndDecode` and `DetectAndDecodeWithType` allocate a points `Mat` when none is supplied. Dispose `BarcodeDecodeResult.Points` after use in that ownership shape.

`DetectAndDecode` 和 `DetectAndDecodeWithType` 在未传入 points 时会分配一个 `Mat`。这种所有权形态下，使用后应释放 `BarcodeDecodeResult.Points`。

## ArUco QR Detection / ArUco 二维码检测

`QRCodeDetectorAruco` is a QR detector backed by OpenCV's ArUco marker detection code. It is separate from the existing `QRCodeDetector` and exposes the OpenCV 5.0.0 ArUco QR parameter structure through `QRCodeDetectorArucoParams`.

`QRCodeDetectorAruco` 是基于 OpenCV ArUco 标记检测代码的二维码检测器。它独立于已有 `QRCodeDetector`，并通过 `QRCodeDetectorArucoParams` 暴露 OpenCV 5.0.0 的 ArUco QR 参数结构。

```csharp
using OpenCvSharp.Core;
using OpenCvSharp.ImgCodecs;
using OpenCvSharp.ObjDetect;

namespace ObjDetectQrArucoSample
{
    internal static class Program
    {
        private static void Main()
        {
            QRCodeDetectorArucoParams parameters = QRCodeDetectorArucoParams.Default;

            using (Mat image = Cv2.ImRead("qr.png", ImreadModes.Color))
            using (QRCodeDetectorAruco detector = QRCodeDetectorAruco.Create(parameters))
            using (Mat points = new Mat())
            {
                bool found = detector.Detect(image, points);
                string decoded = detector.Decode(image, points);

                System.Console.WriteLine("Found=" + found + ", decoded=" + decoded);
            }
        }
    }
}
```

Multi-code methods return `QRCodeMultiDecodeResult`, matching the first QR detector wrapper's ownership pattern for decoded strings and points.

多二维码方法返回 `QRCodeMultiDecodeResult`，其解码字符串和 points 的所有权模式与第一批二维码检测器保持一致。

## QR Encoding / 二维码编码

`QRCodeEncoder` creates QR code images from strings. Parameters expose QR version, error-correction level, encode mode, and structured append count.

`QRCodeEncoder` 可将字符串编码为二维码图像。参数暴露二维码版本、纠错级别、编码模式和结构化追加数量。

```csharp
using OpenCvSharp.Core;
using OpenCvSharp.ObjDetect;

namespace ObjDetectQrEncoderSample
{
    internal static class Program
    {
        private static void Main()
        {
            var parameters = new QRCodeEncoderParams(
                0,
                QRCodeEncoderCorrectionLevel.M,
                QRCodeEncoderEncodeMode.Auto,
                1);

            using (QRCodeEncoder encoder = QRCodeEncoder.Create(parameters))
            using (Mat qrcode = encoder.Encode("OpenCvSharp"))
            {
                System.Console.WriteLine("QR size=" + qrcode.Size);
            }
        }
    }
}
```

`EncodeStructuredAppend` returns an array of `Mat` instances and transfers ownership to managed code. Dispose each matrix after use.

`EncodeStructuredAppend` 返回 `Mat` 数组，并把所有权转交给 managed 层。使用后需要逐个释放矩阵。

## Runtime Notes / 运行时说明

These second-batch objects require the factual OpenCV 5.0.0 runtime artifact `opencv_objdetect500.dll`. Barcode super-resolution requires a compatible user-supplied model path; barcode and QR decoding require real images with sufficient quality. The default tests do not download images, barcode samples, or models.

第二批对象需要事实性 OpenCV 5.0.0 runtime 产物 `opencv_objdetect500.dll`。条形码超分辨率需要用户提供兼容模型路径；条形码和二维码解码需要真实且质量足够的图像。默认测试不会下载图像、条形码样本或模型。

When native OpenCV is absent, the exported ABI remains present and returns the defined `NOT_LINKED` status.

当缺少 native OpenCV 时，导出的 ABI 仍保持存在，并返回定义好的 `NOT_LINKED` 状态。

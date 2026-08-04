# ObjDetect Guide

`JYPPX.OpenCvSharp.ObjDetect` wraps OpenCV object-detection APIs that are present in the main OpenCV `objdetect` module. The first guide focuses on QR code detection/decoding and DNN-based face detection/recognition through stable native handles; the second-batch guide covers barcode, ArUco QR, and QR encoder objects.

`JYPPX.OpenCvSharp.ObjDetect` 封装 OpenCV 主仓库 `objdetect` 模块中的目标检测 API。本指南聚焦二维码检测/解码，以及通过稳定 native 句柄调用的 DNN 人脸检测与识别对象；第二批指南覆盖条形码、ArUco 二维码和二维码编码器对象。

## Covered APIs / 已覆盖接口

- `QRCodeDetector`: constructor, `Create`, `SetEpsX`, `SetEpsY`, `SetUseAlignmentMarkers`, `Detect`, `Decode`, `DetectAndDecode`, `DecodeCurved`, `DetectAndDecodeCurved`, `DetectMulti`, `DecodeMulti`, `DetectAndDecodeMulti`, and `GetEncoding`.
- `QRCodeMultiDecodeResult`: decoded strings, success flag, and points matrix ownership.
- `QRCodeEncoderECIEncodings`: OpenCV QR ECI encoding values.
- See [ObjDetect Second Batch Guide](objdetect-second-batch-guide.md) for `BarcodeDetector`, `QRCodeDetectorAruco`, and `QRCodeEncoder`.
- See [ChArUco Guide](charuco-guide.md) and [MCC Checker Guide](mcc-checker-guide.md) for board/checker object chains.
- `FaceDetectorYN`: path and buffer `Create` overloads, `InputSize`, `ScoreThreshold`, `NMSThreshold`, `TopK`, `Detect`, and `ToFaceDetections`.
- `FaceDetection`: rectangle, five landmarks, and confidence score from the OpenCV `N x 15` output row.
- `FaceRecognizerSF`: path and buffer `Create` overloads, `AlignCrop`, `Feature`, and `Match`.
- `DnnBackend`, `DnnTarget`, and `FaceRecognizerSFDistanceType` enums.
- Modern .NET fast paths: `ReadOnlySpan<byte>` model/config overloads for `netcoreapp3.1+`, plus source-generated P/Invoke on `net7.0+`.

- `QRCodeDetector`：构造、`Create`、`SetEpsX`、`SetEpsY`、`SetUseAlignmentMarkers`、`Detect`、`Decode`、`DetectAndDecode`、`DecodeCurved`、`DetectAndDecodeCurved`、`DetectMulti`、`DecodeMulti`、`DetectAndDecodeMulti` 和 `GetEncoding`。
- `QRCodeMultiDecodeResult`：解码字符串、成功标志和 points 矩阵所有权。
- `QRCodeEncoderECIEncodings`：OpenCV QR ECI 编码值。
- `BarcodeDetector`、`QRCodeDetectorAruco` 和 `QRCodeEncoder` 见 [ObjDetect Second Batch Guide](objdetect-second-batch-guide.md)。
- board/checker 对象链见 [ChArUco Guide](charuco-guide.md) 和 [MCC Checker Guide](mcc-checker-guide.md)。
- `FaceDetectorYN`：路径和缓冲区 `Create` 重载、`InputSize`、`ScoreThreshold`、`NMSThreshold`、`TopK`、`Detect` 和 `ToFaceDetections`。
- `FaceDetection`：来自 OpenCV `N x 15` 输出行的矩形、五个人脸关键点和置信度。
- `FaceRecognizerSF`：路径和缓冲区 `Create` 重载、`AlignCrop`、`Feature` 和 `Match`。
- `DnnBackend`、`DnnTarget` 和 `FaceRecognizerSFDistanceType` 枚举。
- 现代 .NET 快速路径：`netcoreapp3.1+` 的 `ReadOnlySpan<byte>` 模型/配置重载，以及 `net7.0+` 的 source-generated P/Invoke。

## QR Code Detection / 二维码检测

```csharp
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.ImgCodecs;
using JYPPX.OpenCvSharp.ObjDetect;

namespace ObjDetectQrSample
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat image = Cv2.ImRead("qr.png", ImreadModes.Color))
            using (QRCodeDetector detector = QRCodeDetector.Create())
            using (Mat points = new Mat())
            {
                string decoded = detector.SetEpsX(0.2)
                    .SetEpsY(0.2)
                    .SetUseAlignmentMarkers(true)
                    .DetectAndDecode(image, points);

                System.Console.WriteLine("Decoded=" + decoded);
            }
        }
    }
}
```

`DetectAndDecodeMulti` returns a `QRCodeMultiDecodeResult`. If you do not pass a points matrix, the result owns the returned `Mat`; dispose it when the points are no longer needed.

`DetectAndDecodeMulti` 返回 `QRCodeMultiDecodeResult`。如果没有传入 points 矩阵，结果对象会持有返回的 `Mat`；不再需要顶点时应释放它。

## Face Detection / 人脸检测

`FaceDetectorYN` follows OpenCV `cv::FaceDetectorYN`. It requires a model file or model buffer supported by OpenCV DNN, usually ONNX. Detection writes an OpenCV `N x 15` matrix:

`FaceDetectorYN` 对齐 OpenCV `cv::FaceDetectorYN`。它需要 OpenCV DNN 支持的模型文件或模型缓冲区，通常是 ONNX。检测结果写入 OpenCV `N x 15` 矩阵：

- `x`, `y`, `width`, `height`
- right eye, left eye, nose tip, right mouth corner, left mouth corner
- score

- `x`、`y`、`width`、`height`
- 右眼、左眼、鼻尖、右嘴角、左嘴角
- 分数

```csharp
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.ImgCodecs;
using JYPPX.OpenCvSharp.ObjDetect;

namespace ObjDetectFaceSample
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat image = Cv2.ImRead("face.jpg", ImreadModes.Color))
            using (FaceDetectorYN detector = FaceDetectorYN.Create("face_detection_yunet.onnx", string.Empty, new Size(320, 320)))
            using (Mat faces = detector.Detect(image, out int result))
            {
                FaceDetection[] detections = FaceDetectorYN.ToFaceDetections(faces);
                System.Console.WriteLine("Result=" + result + ", faces=" + detections.Length);
            }
        }
    }
}
```

For `netcoreapp3.1+`, model buffers can be passed as `ReadOnlySpan<byte>` to avoid extra array ownership work in higher-level code. Older target frameworks keep `byte[]` overloads with the same API shape.

对于 `netcoreapp3.1+`，模型缓冲区可通过 `ReadOnlySpan<byte>` 传入，方便上层代码减少额外数组所有权处理。旧目标框架保留 `byte[]` 重载，并维持一致的 API 形状。

## Face Recognition / 人脸识别

`FaceRecognizerSF` wraps `cv::FaceRecognizerSF`. The usual workflow is:

`FaceRecognizerSF` 封装 `cv::FaceRecognizerSF`。常见流程如下：

1. Detect a face with `FaceDetectorYN`.
2. Pass the face row to `AlignCrop`.
3. Pass the aligned image to `Feature`.
4. Compare two features with `Match`.

1. 使用 `FaceDetectorYN` 检测人脸。
2. 将人脸结果行传给 `AlignCrop`。
3. 将对齐后图像传给 `Feature`。
4. 使用 `Match` 比较两个人脸特征。

```csharp
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.ImgCodecs;
using JYPPX.OpenCvSharp.ObjDetect;

namespace ObjDetectRecognizerSample
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat image = Cv2.ImRead("face.jpg", ImreadModes.Color))
            using (FaceDetectorYN detector = FaceDetectorYN.Create("face_detection_yunet.onnx", string.Empty, new Size(320, 320)))
            using (FaceRecognizerSF recognizer = FaceRecognizerSF.Create("face_recognition_sface.onnx", string.Empty))
            using (Mat faces = detector.Detect(image, out int result))
            {
                if (result > 0 && faces.Rows > 0)
                {
                    using (Mat faceRow = faces.Row(0))
                    using (Mat aligned = recognizer.AlignCrop(image, faceRow))
                    using (Mat feature = recognizer.Feature(aligned))
                    {
                        double selfScore = recognizer.Match(feature, feature, FaceRecognizerSFDistanceType.Cosine);
                        System.Console.WriteLine("Self score=" + selfScore);
                    }
                }
            }
        }
    }
}
```

Real recognition requires a valid image, a valid face row from `FaceDetectorYN`, and a compatible SFace model.

真实识别需要有效图像、来自 `FaceDetectorYN` 的有效人脸结果行，以及兼容的 SFace 模型。

## Runtime Notes / 运行时说明

ObjDetect runtime packages must stage `opencv_objdetect500` and `opencv_dnn500` in addition to the existing core/image modules. QR, barcode, QR encoder, ArUco, ChArUco, and non-DNN MCC checker APIs use `objdetect`, while `FaceDetectorYN`, `FaceRecognizerSF`, and future DNN-assisted MCC APIs require OpenCV built with DNN support and external model files supplied by the user.

包含 ObjDetect 的 runtime 包除现有 core/image 模块外，还必须暂存 `opencv_objdetect500` 和 `opencv_dnn500`。二维码、条形码、二维码编码器、ArUco、ChArUco 和非 DNN MCC checker API 使用 `objdetect`，而 `FaceDetectorYN`、`FaceRecognizerSF` 以及后续 DNN 辅助 MCC API 需要 OpenCV 启用 DNN，并由用户提供外部模型文件。



## OpenCV 5.0.0 Boundary / OpenCV 5.0.0 边界

The local OpenCV 5.0.0 main `objdetect` headers expose `QRCodeDetector`, `BarcodeDetector`, `QRCodeDetectorAruco`, `QRCodeEncoder`, `FaceDetectorYN`, `FaceRecognizerSF`, and other graphical-code APIs. The older cascade and HOG workflows are not part of this main-module surface in the local 5.0.0 tree; related APIs found under contrib `xobjdetect` are documented in [XObjDetect Guide](xobjdetect-guide.md).

本地 OpenCV 5.0.0 主仓库 `objdetect` 头文件暴露 `QRCodeDetector`、`BarcodeDetector`、`QRCodeDetectorAruco`、`QRCodeEncoder`、`FaceDetectorYN`、`FaceRecognizerSF` 和其他 graphical-code 相关 API。旧版级联分类器与 HOG 工作流不属于本地 5.0.0 主模块接口面；在 contrib `xobjdetect` 下发现的相关 API 见 [XObjDetect Guide](xobjdetect-guide.md)。

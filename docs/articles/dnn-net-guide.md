# DNN Net Guide

`OpenCvSharp.Dnn` exposes the OpenCV DNN surface: `Net`, path and buffer model loading, input setup, forward passes, multi-output forward, layer-name and metadata queries, profiling helpers, FLOPS helpers, and blob helpers. These APIs require the factual OpenCV 5.0.0 runtime artifact `opencv_dnn500.dll` in linked runtime packages.

`OpenCvSharp.Dnn` 暴露 OpenCV DNN 接口：`Net`、模型路径和缓冲区加载、输入设置、forward、多输出 forward、层名称和元数据查询、profile helper、FLOPS helper 和 blob 辅助函数。这些 API 在 linked runtime 包中需要事实性 OpenCV 5.0.0 runtime 产物 `opencv_dnn500.dll`。

## Covered APIs / 已覆盖接口

- `Net.CreateEmpty`
- `Net.ReadNet` from model/config/framework paths
- `Net.ReadNet` from `byte[]` and modern `ReadOnlySpan<byte>` buffers
- `Net.Empty`
- `Net.SetPreferableBackend`
- `Net.SetPreferableTarget`
- `Net.SetInput`
- `Net.Forward`
- `Net.Forward(string[] outputNames)`
- `Net.GetLayerNames`
- `Net.GetUnconnectedOutLayersNames`
- `Net.GetUnconnectedOutLayers`
- `Net.GetLayerTypes`
- `Net.GetPerfProfile`
- `Net.GetFLOPS`
- `Cv2.BlobFromImage`
- `Cv2.BlobFromImages`
- `Cv2.ImagesFromBlob`
- `DnnBackend`, `DnnTarget`, and `DnnEngine`

- `Net.CreateEmpty`
- 通过模型/配置/framework 路径调用 `Net.ReadNet`
- 通过 `byte[]` 和现代 `ReadOnlySpan<byte>` 缓冲调用 `Net.ReadNet`
- `Net.Empty`
- `Net.SetPreferableBackend`
- `Net.SetPreferableTarget`
- `Net.SetInput`
- `Net.Forward`
- `Net.Forward(string[] outputNames)`
- `Net.GetLayerNames`
- `Net.GetUnconnectedOutLayersNames`
- `Net.GetUnconnectedOutLayers`
- `Net.GetLayerTypes`
- `Net.GetPerfProfile`
- `Net.GetFLOPS`
- `Cv2.BlobFromImage`
- `Cv2.BlobFromImages`
- `Cv2.ImagesFromBlob`
- `DnnBackend`、`DnnTarget` 和 `DnnEngine`

## Blob Helpers / Blob 辅助函数

```csharp
using OpenCvSharp.Core;
using DnnCv2 = OpenCvSharp.Dnn.Cv2;

namespace DnnBlobSample
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat image = new Mat(224, 224, MatType.CV_8UC3, new Scalar(32, 64, 96)))
            using (Mat blob = DnnCv2.BlobFromImage(image, 1.0, new Size(224, 224), new Scalar(0), swapRB: true))
            {
                Mat[] images = DnnCv2.ImagesFromBlob(blob);
                try
                {
                    System.Console.WriteLine("Blob=" + blob.Size + ", images=" + images.Length);
                }
                finally
                {
                    for (int i = 0; i < images.Length; i++)
                    {
                        images[i].Dispose();
                    }
                }
            }
        }
    }
}
```

## Model Forward / 模型 Forward

```csharp
using OpenCvSharp.Core;
using OpenCvSharp.Dnn;

namespace DnnForwardSample
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat image = new Mat(224, 224, MatType.CV_8UC3, new Scalar(0)))
            using (Mat blob = Cv2.BlobFromImage(image, 1.0, new Size(224, 224)))
            using (Net net = Net.ReadNet("model.onnx"))
            {
                net.SetPreferableBackend(DnnBackend.OpenCV);
                net.SetPreferableTarget(DnnTarget.Cpu);
                net.SetInput(blob);

                using (Mat output = net.Forward())
                {
                    System.Console.WriteLine("Output=" + output.Size + ", layers=" + net.GetLayerNames().Length);
                }
            }
        }
    }
}
```

Default tests do not download or require a model. Real model smoke can be enabled with `OPENCV_CSHARP_DNN_MODEL`, `OPENCV_CSHARP_DNN_CONFIG`, and optionally `OPENCV_CSHARP_DNN_FRAMEWORK`. The older `OPENCV5SHARP_DNN_*` names remain accepted only as existing-smoke-workflow compatibility aliases.

默认测试不会下载或要求模型。真实模型 smoke 可按需通过 `OPENCV_CSHARP_DNN_MODEL`、`OPENCV_CSHARP_DNN_CONFIG`，以及可选的 `OPENCV_CSHARP_DNN_FRAMEWORK` 启用。旧的 `OPENCV5SHARP_DNN_*` 名称仍仅作为既有 smoke workflow 的兼容别名使用。

Advanced metadata, profile, FLOPS, and multi-output examples are covered in [DNN Net Advanced Guide](dnn-net-advanced-guide.md).

高级元数据、profile、FLOPS 和多输出示例见 [DNN Net Advanced Guide](dnn-net-advanced-guide.md)。

## ABI Notes / ABI 说明

`Net` is held behind an opaque native handle. String arrays use count/fill calls with offsets and caller-owned UTF-8 buffers. Mat arrays use caller-owned handle buffers. No `cv::Ptr`, STL container, `cv::InputArray`, or `cv::OutputArray` crosses the C ABI.

`Net` 由 opaque native 句柄持有。字符串数组使用 count/fill 调用、offsets 和调用方持有的 UTF-8 缓冲。Mat 数组使用调用方持有的句柄缓冲。`cv::Ptr`、STL 容器、`cv::InputArray` 和 `cv::OutputArray` 都不会穿过 C ABI。

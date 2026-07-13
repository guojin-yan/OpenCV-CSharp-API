# DNN Net Advanced Guide

`OpenCvSharp.Dnn.Net` now includes the second DNN batch: multi-output `Forward`, layer and net metadata, performance profile timings, FLOPS helpers, input-shape helpers, and convenience model readers for selected OpenCV loaders.

`OpenCvSharp.Dnn.Net` 现在包含 DNN 第二批能力：多输出 `Forward`、layer/net 元数据、性能剖析耗时、FLOPS helper、输入形状 helper，以及部分 OpenCV loader 的便捷读取入口。

## Covered APIs / 已覆盖接口

- `Net.ReadNetFromOnnx`
- `Net.ReadNetFromTensorflow`
- `Net.ReadNetFromTFLite`
- `Net.ReadNetFromModelOptimizer`
- `Net.Forward(string[] outputNames)`
- `Net.GetLayerId`
- `Net.GetUnconnectedOutLayers`
- `Net.SetInputsNames`
- `Net.SetInputShape`
- `Net.GetFLOPS`
- `Net.GetLayerFLOPS`
- `Net.GetPerfProfile`
- `Net.GetLayerTypes`
- `Net.GetLayersCountByType`

- `Net.ReadNetFromOnnx`
- `Net.ReadNetFromTensorflow`
- `Net.ReadNetFromTFLite`
- `Net.ReadNetFromModelOptimizer`
- `Net.Forward(string[] outputNames)`
- `Net.GetLayerId`
- `Net.GetUnconnectedOutLayers`
- `Net.SetInputsNames`
- `Net.SetInputShape`
- `Net.GetFLOPS`
- `Net.GetLayerFLOPS`
- `Net.GetPerfProfile`
- `Net.GetLayerTypes`
- `Net.GetLayersCountByType`

For Caffe and Darknet style path loading, use the existing general `Net.ReadNet(model, config, framework)` entry and pass the framework string expected by OpenCV.

对于 Caffe 和 Darknet 风格的路径加载，请使用已有通用入口 `Net.ReadNet(model, config, framework)`，并传入 OpenCV 期望的 framework 字符串。

## Multi-Output Forward / 多输出 Forward

```csharp
using OpenCvSharp.Core;
using OpenCvSharp.Dnn;

namespace DnnAdvancedSample
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat image = new Mat(224, 224, MatType.CV_8UC3, new Scalar(0)))
            using (Mat blob = Cv2.BlobFromImage(image, 1.0, new Size(224, 224)))
            using (Net net = Net.ReadNetFromOnnx("model.onnx"))
            {
                net.SetInput(blob);
                string[] outputNames = net.GetUnconnectedOutLayersNames();
                Mat[] outputs = outputNames.Length == 0 ? new Mat[0] : net.Forward(outputNames);
                try
                {
                    DnnPerfProfile profile = net.GetPerfProfile();
                    System.Console.WriteLine("outputs=" + outputs.Length + ", ticks=" + profile.TickCount);
                }
                finally
                {
                    for (int i = 0; i < outputs.Length; i++)
                    {
                        outputs[i].Dispose();
                    }
                }
            }
        }
    }
}
```

Default tests do not download or require a model. Real model smoke remains opt-in through `OPENCV_CSHARP_DNN_MODEL`, `OPENCV_CSHARP_DNN_CONFIG`, and optionally `OPENCV_CSHARP_DNN_FRAMEWORK`. The older `OPENCV5SHARP_DNN_*` names remain accepted only as existing-smoke-workflow compatibility aliases.

默认测试不会下载或依赖模型。真实模型 smoke 仍通过 `OPENCV_CSHARP_DNN_MODEL`、`OPENCV_CSHARP_DNN_CONFIG`，以及可选的 `OPENCV_CSHARP_DNN_FRAMEWORK` 显式启用。旧的 `OPENCV5SHARP_DNN_*` 名称仍仅作为既有 smoke workflow 的兼容别名使用。

Empty `Net` metadata is runtime-specific. The runtime for the current packaged runtime identity, OpenCV 5.0.0, may report internal `_input` output names or layer types even when `Net.Empty` is true, so smoke tests validate that metadata calls are stable rather than requiring all metadata arrays to be empty.

空 `Net` 的 metadata 具有 runtime 差异。当前打包 runtime 身份 OpenCV 5.0.0 对应的 runtime 即使在 `Net.Empty` 为 true 时也可能报告内部 `_input` output name 或 layer type，因此 smoke 测试验证 metadata 调用稳定，而不是要求所有 metadata 数组必须为空。

## ABI Notes / ABI 说明

Output names are packed into caller-owned UTF-8 buffers with offsets. Native forward returns owned `Mat` handles in a caller-provided handle buffer. Performance timings and layer ids use caller-owned numeric arrays. No STL container, `cv::Ptr`, `cv::InputArray`, or `cv::OutputArray` crosses the exported C ABI.

输出名称通过调用方持有的 UTF-8 缓冲和 offsets 传入。native forward 通过调用方提供的 handle 缓冲返回 owned `Mat` handle。性能耗时和 layer id 使用调用方持有的数值数组。导出的 C ABI 不穿透 STL 容器、`cv::Ptr`、`cv::InputArray` 或 `cv::OutputArray`。

# 17 ONNX Inference / ONNX 推理

The DNN example loads the checked-in ONNX opset 13 identity model from a base64 fixture, creates a blob, binds input `input`, executes a CPU forward pass, and prints the four output values.

本案例从 base64 夹具加载仓库内的 ONNX opset 13 identity 模型，创建 blob，绑定 `input`，执行 CPU 前向推理并输出四个结果值。

```powershell
dotnet run --project .\samples\DeepLearning\01.OnnxInference\OnnxInference.csproj -c Release -- .\artifacts\tutorial-17
```

The tiny identity graph keeps the example deterministic and offline. Replace the fixture with a production model only after documenting its input shape, preprocessing, output names, and runtime backend requirements.

## Pipeline / 流程

The base64 file is decoded before any native call, so the sample proves an in-memory model boundary rather than a path-dependent loader. `BlobFromImage` creates the four-dimensional input blob, `SetInput` binds the declared ONNX input name, and `Forward` returns an owned `Mat` whose values are copied into the summary and visualization.

先解码 base64 文件再调用 native API，因此案例验证的是内存模型边界，而不是依赖路径的加载器。`BlobFromImage` 创建四维输入 blob，`SetInput` 绑定 ONNX 输入名，`Forward` 返回由 `Mat` 持有的结果，再复制数值到摘要和可视化面板。

# 06 KNN Classification / KNN 分类

This case combines the Full runtime's required OpenCV ML module with image visualization. It trains a two-class K-nearest model, predicts 3,600 query samples in one native call, and renders the decision surface. Mini and older packages without the corrected Full/ML boundary produce a `NOT_LINKED` diagnostic panel instead of hiding the capability mismatch.

该案例把 Full runtime 必需的 OpenCV ML 模块与图像可视化组合起来：训练二分类 K 近邻模型，在一次 native 调用中预测 3,600 个 query，并绘制决策面。Mini 和尚未修正 Full/ML 边界的旧包会生成 `NOT_LINKED` 诊断面板，明确指出能力不匹配。

![KNN classification output](../images/showcase/knn-classification.png)

## Run / 运行

[`MachineLearning/01.KnnClassification/Program.cs`](https://github.com/guojin-yan/OpenCV-CSharp-API/blob/opencv5.x/samples/MachineLearning/01.KnnClassification/Program.cs) owns training data preparation, batch query generation, model training, prediction, decision-surface rendering, and explicit runtime capability handling.

[`MachineLearning/01.KnnClassification/Program.cs`](https://github.com/guojin-yan/OpenCV-CSharp-API/blob/opencv5.x/samples/MachineLearning/01.KnnClassification/Program.cs) 自行完成训练数据准备、批量查询生成、模型训练、预测、决策面绘制和明确的 runtime 能力处理。

```powershell
dotnet run --project .\samples\MachineLearning\01.KnnClassification\KnnClassification.csproj -c Release -- .\artifacts\tutorial-06
```

## Core Flow / 核心流程

```csharp
using var samples = new Mat(8, 2, MatType.CV_32FC1);
using var responses = new Mat(8, 1, MatType.CV_32SC1);
using var queries = new Mat(3600, 2, MatType.CV_32FC1);
using var results = new Mat();
using KNearest knn = KNearest.Create();

samples.CopyFrom<float>(trainingValues);
responses.CopyFrom<int>(trainingLabels);
queries.CopyFrom<float>(queryValues);
knn.DefaultK = 3;
knn.IsClassifierModel = true;
knn.Train(samples, SampleTypes.RowSample, responses);
knn.FindNearest(queries, 3, results);
float[] labels = results.ToArray<float>();
```

Production code should control feature normalization, label type, train/test separation, validation metrics, and persisted model provenance. Batch prediction avoids a managed/native transition for every sample.

生产代码需要控制特征归一化、标签类型、训练/测试划分、验证指标和持久化模型来源。批量预测可以避免每个样本都发生一次 managed/native 切换。

This case needs the latest Full runtime, whose release contract now requires ML. The public `5.0.0-preview.1` `win-x64` package can report `NOT_LINKED` because it predates that corrected boundary; update the runtime rather than treating the marker as an application error. Continue with [ML Guide](ml-guide.md) for SVM, trees, boosting, EM, logistic regression, and neural-network APIs.

该案例需要最新的 Full runtime；当前发布契约已经把 ML 列为必需模块。公开的 `5.0.0-preview.1` `win-x64` 包早于该修复，可能返回 `NOT_LINKED`；此时应更新 runtime，而不是把标记误判为应用错误。SVM、trees、boosting、EM、logistic regression 和神经网络 API 见 [ML Guide](ml-guide.md)。

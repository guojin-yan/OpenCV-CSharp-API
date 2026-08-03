# 06 KNN Classification / KNN 分类

The final case combines OpenCV ML with image visualization. It trains a two-class K-nearest model, predicts 3,600 query samples in one native call, and renders the decision surface.

最后一个案例把 OpenCV ML 与图像可视化组合起来：训练二分类 K 近邻模型，在一次 native 调用中预测 3,600 个 query，并绘制决策面。

![KNN classification output](../images/showcase/knn-classification.png)

## Run / 运行

```powershell
dotnet run --project .\samples\ConsoleSamples\ConsoleSamples.csproj -c Release `
  -p:OpenCvNativeRuntimeDir=E:\path\to\full-runtime `
  -- tutorial ml .\artifacts\tutorial-06
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

This case requires a full runtime. Continue with [ML Guide](ml-guide.md) for SVM, trees, boosting, EM, logistic regression, and neural-network APIs.

该案例需要 full runtime。SVM、trees、boosting、EM、logistic regression 和神经网络 API 见 [ML Guide](ml-guide.md)。

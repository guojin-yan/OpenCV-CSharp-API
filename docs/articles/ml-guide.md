# ML Guide / ML 指南

`OpenCvSharp.ML` wraps the first OpenCV 5.0.0 machine-learning objects from the local contrib `ml` tree.

`OpenCvSharp.ML` 封装第一批来自本地 OpenCV 5.0.0 contrib `ml` 树的机器学习对象。

## Scope / 范围

- Training data: `TrainData` from in-memory matrices or CSV files.
- Model base: `StatModel` state, training, prediction, error calculation, save, and clear.
- Models: `KNearest`, `SVM`, and `NormalBayesClassifier`.
- Parameter grids: `ParamGrid` and `SVM.GetDefaultGrid`.
- Enums: sample layout, variable type, model flags, KNN algorithm, SVM type, SVM kernel, and SVM parameter ids.

- 训练数据：从内存矩阵或 CSV 文件创建 `TrainData`。
- 模型基类：`StatModel` 状态、训练、预测、误差计算、保存和清理。
- 模型：`KNearest`、`SVM` 和 `NormalBayesClassifier`。
- 参数网格：`ParamGrid` 与 `SVM.GetDefaultGrid`。
- 枚举：样本布局、变量类型、模型标志、KNN 算法、SVM 类型、SVM 核函数和 SVM 参数 id。

## Runtime / 运行时

In this local OpenCV 5.0.0 source layout, `ml` is provided by the contrib tree, not the main OpenCV module tree. A linked runtime should include the factual OpenCV 5.0.0 runtime artifact `opencv_ml500.dll`. If the module is not linked, the exported ABI remains present and managed calls report `NOT_LINKED`.

在当前本地 OpenCV 5.0.0 源码布局中，`ml` 来自 contrib 树，而不是 OpenCV 主仓库模块树。linked runtime 应包含事实性 OpenCV 5.0.0 runtime 产物 `opencv_ml500.dll`。如果模块未链接，导出的 ABI 仍存在，managed 调用会报告 `NOT_LINKED`。

## Data Notes / 数据说明

OpenCV ML training samples usually use `CV_32F`. For row samples, every row is one sample and every column is one variable. Response type and classifier/regression settings affect training behavior, so keep `responses` aligned with the model type.

OpenCV ML 训练样本通常使用 `CV_32F`。使用 row samples 时，每一行是一条样本，每一列是一个变量。`responses` 类型以及分类/回归设置会影响训练行为，因此要让响应数据与模型类型保持一致。

`TrainData` exposes the stable data surface that can be represented through this C ABI. String and vector-like outputs stay inside native code and are copied through count/fill style APIs.

`TrainData` 只暴露可通过当前 C ABI 稳定表达的数据面。字符串和类似 vector 的输出留在 native 内部，并通过 count/fill 风格 API 复制出来。

`SVM.TrainAuto` can be slower than tiny smoke examples because OpenCV searches parameter grids. Samples and default tests use direct training on small matrices.

`SVM.TrainAuto` 会搜索参数网格，因此可能比 tiny smoke 示例慢。示例和默认测试使用小矩阵直接训练。

```csharp
using OpenCvSharp.Core;
using OpenCvSharp.ML;

using Mat samples = new Mat(4, 2, MatType.CV_32FC1);
samples.CopyFrom<float>(new float[]
{
    0.0F, 0.0F,
    0.0F, 1.0F,
    5.0F, 5.0F,
    6.0F, 5.0F
});

using Mat responses = new Mat(4, 1, MatType.CV_32SC1);
responses.CopyFrom<int>(new[] { 0, 0, 1, 1 });

using KNearest knn = KNearest.Create();
knn.DefaultK = 1;
knn.Train(samples, SampleTypes.RowSample, responses);

using Mat query = new Mat(1, 2, MatType.CV_32FC1);
query.CopyFrom<float>(new[] { 0.1F, 0.2F });
float predicted = knn.Predict(query);
```

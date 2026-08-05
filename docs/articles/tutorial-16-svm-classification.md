# 16 SVM Classification / SVM 分类

This example trains an RBF support-vector classifier on two small feature clusters, predicts a dense grid, and renders the learned decision surface with the training points.

本案例在两个小型特征簇上训练 RBF 支持向量分类器，预测密集网格，并将决策面和训练点一起可视化。

```powershell
dotnet run --project .\samples\MachineLearning\02.SvmClassification\SvmClassification.csproj -c Release -- .\artifacts\tutorial-16
```

OpenCV ML is an optional native module in some runtime profiles. A `NOT_LINKED` runtime produces a diagnostic image and summary so capability discovery is explicit and automation-friendly.

## Pipeline / 流程

Training samples are row-major `CV_32FC1` values and responses are integer class labels. The case sets `CSvc`, an RBF kernel, `C=2`, `Gamma=3`, and a bounded termination criterion, then predicts a 80x45 grid in one call. Training points are drawn on top of the decision surface to make the separation easy to inspect.

训练样本是按行排列的 `CV_32FC1` 数据，响应为整数类别标签。案例配置 `CSvc`、RBF 核、`C=2`、`Gamma=3` 和有上限的终止条件，再一次性预测 80x45 网格，并把训练点绘制在决策面上。

# Shape Guide / Shape 指南

`OpenCvSharp.Shape` wraps model-free OpenCV contrib `shape` utilities behind stable managed objects and caller-owned `Mat` inputs.

`OpenCvSharp.Shape` 通过稳定的 managed object 和调用方持有的 `Mat` 输入封装 OpenCV contrib `shape` 的无模型工具。

## Scope / 范围

- Static helper: `ShapeCv2.EMDL1`.
- Histogram cost extractors: `NormHistogramCostExtractor`, `EMDHistogramCostExtractor`, `ChiHistogramCostExtractor`, and `EMDL1HistogramCostExtractor`.
- Shared histogram operations: `BuildCostMatrix`, `NDummies`, and `DefaultCost`.
- Shape distance extractors: `ShapeContextDistanceExtractor`, `HausdorffDistanceExtractor`, and base `ShapeDistanceExtractor.ComputeDistance`.
- Hausdorff properties: `DistanceFlag` and `RankProportion`.

- 静态 helper：`ShapeCv2.EMDL1`。
- 直方图代价提取器：`NormHistogramCostExtractor`、`EMDHistogramCostExtractor`、`ChiHistogramCostExtractor` 与 `EMDL1HistogramCostExtractor`。
- 共享直方图操作：`BuildCostMatrix`、`NDummies` 与 `DefaultCost`。
- 形状距离提取器：`ShapeContextDistanceExtractor`、`HausdorffDistanceExtractor` 以及基类 `ShapeDistanceExtractor.ComputeDistance`。
- Hausdorff 属性：`DistanceFlag` 与 `RankProportion`。

## Runtime / 运行时

`shape` is an optional OpenCV contrib module. Runtime staging includes the factual OpenCV 5.0.0 runtime artifact `opencv_shape500.dll` when the module is built; if a runtime lacks it, the managed API shape remains stable and linked calls report `NOT_LINKED`.

`shape` 是可选 OpenCV contrib 模块。构建该模块时 runtime staging 会包含事实性 OpenCV 5.0.0 runtime 产物 `opencv_shape500.dll`；如果某个 runtime 缺少它，managed API 形状仍保持稳定，linked 调用会报告 `NOT_LINKED`。

## Input Notes / 输入说明

`EMDL1` and histogram cost extractors operate on caller-owned floating-point descriptor/signature matrices. Distance extractors operate on contour matrices, commonly `CV_32FC2` point sequences. Tiny smoke checks verify call paths and output shape only; real matching quality depends on descriptor and contour construction.

`EMDL1` 与直方图代价提取器使用调用方持有的浮点 descriptor/signature 矩阵。距离提取器使用轮廓矩阵，常见为 `CV_32FC2` 点序列。tiny smoke 只验证调用路径和输出形状；真实匹配质量取决于 descriptor 与 contour 的构造方式。

```csharp
using OpenCvSharp.Core;
using OpenCvSharp.Shape;

using Mat first = new Mat(3, 1, MatType.CV_32FC1);
using Mat second = new Mat(3, 1, MatType.CV_32FC1);
first.CopyFrom(new float[] { 0.2F, 0.3F, 0.5F });
second.CopyFrom(new float[] { 0.1F, 0.4F, 0.5F });

float emdL1 = ShapeCv2.EMDL1(first, second);

using NormHistogramCostExtractor extractor = ShapeCv2.CreateNormHistogramCostExtractor(NormTypes.L2, 2, 0.25F);
using Mat cost = extractor.BuildCostMatrix(first, second);

using Mat contour1 = new Mat(4, 1, MatType.CV_32FC2);
using Mat contour2 = new Mat(4, 1, MatType.CV_32FC2);
contour1.CopyFrom(new float[] { 0, 0, 1, 0, 1, 1, 0, 1 });
contour2.CopyFrom(new float[] { 0.2F, 0, 1.2F, 0, 1.2F, 1, 0.2F, 1 });

using HausdorffDistanceExtractor hausdorff = ShapeCv2.CreateHausdorffDistanceExtractor(NormTypes.L2, 0.6F);
float contourDistance = hausdorff.ComputeDistance(contour1, contour2);
```

# HFS Guide / HFS 指南

`JYPPX.OpenCvSharp.Hfs` wraps the first OpenCV contrib `hfs` Hierarchical Feature Selection segmentation surface through an opaque `HfsSegment` handle.

`JYPPX.OpenCvSharp.Hfs` 通过 opaque `HfsSegment` 句柄封装第一批 OpenCV contrib `hfs` Hierarchical Feature Selection 分割能力。

## Scope / 范围

- Parameter value object: `HfsSegmentParams`.
- Factory helpers: `HfsSegment.Create` and `HfsCv2.CreateHfsSegment`.
- Seven HFS properties: `SegEgbThresholdI`, `MinRegionSizeI`, `SegEgbThresholdII`, `MinRegionSizeII`, `SpatialWeight`, `SlicSpixelSize`, and `NumSlicIter`.
- Segmentation calls: `PerformSegmentCpu` and guarded `PerformSegmentGpu`.

- 参数值对象：`HfsSegmentParams`。
- 工厂 helper：`HfsSegment.Create` 与 `HfsCv2.CreateHfsSegment`。
- 七个 HFS 属性：`SegEgbThresholdI`、`MinRegionSizeI`、`SegEgbThresholdII`、`MinRegionSizeII`、`SpatialWeight`、`SlicSpixelSize` 和 `NumSlicIter`。
- 分割调用：`PerformSegmentCpu` 与受运行时支持约束的 `PerformSegmentGpu`。

## Runtime / 运行时

`hfs` is an optional OpenCV contrib module. Runtime staging includes the factual OpenCV 5.0.0 runtime artifact `opencv_hfs500.dll` when the module is built. If a runtime lacks it, the managed API shape remains stable and linked calls report `NOT_LINKED`.

`hfs` 是可选 OpenCV contrib 模块。构建该模块时 runtime staging 会包含事实性 OpenCV 5.0.0 runtime 产物 `opencv_hfs500.dll`。如果某个 runtime 缺少它，managed API 形状仍保持稳定，linked 调用会报告 `NOT_LINKED`。

The default smoke path uses CPU segmentation only. GPU segmentation depends on how the local OpenCV runtime was built and should be treated as optional.

默认 smoke 路径只使用 CPU 分割。GPU 分割取决于本地 OpenCV runtime 的构建方式，应视为可选能力。

## Input Notes / 输入说明

OpenCV HFS expects the segmenter size to match the input image size. The CPU smoke uses tiny synthetic `CV_8UC3` BGR images and verifies output shape only, not segmentation quality.

OpenCV HFS 期望 segmenter 尺寸与输入图像尺寸一致。CPU smoke 使用 tiny 合成 `CV_8UC3` BGR 图像，并只验证输出形状，不衡量分割质量。

```csharp
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Hfs;

using Mat image = new Mat(32, 32, MatType.CV_8UC3, new Scalar(20, 40, 80));
using HfsSegment segment = HfsCv2.CreateHfsSegment(32, 32);

using Mat drawn = segment.PerformSegmentCpu(image, draw: true);
using Mat labels = segment.PerformSegmentCpu(image, draw: false);
```

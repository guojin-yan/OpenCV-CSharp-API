# OptFlow Guide / OptFlow 指南

`OpenCvSharp.OptFlow` wraps the first OpenCV 5.0.0 contrib `optflow` objects and motion-template helpers.

`OpenCvSharp.OptFlow` 封装第一批 OpenCV 5.0.0 contrib `optflow` 对象和 motion-template 辅助函数。

## Scope / 范围

- Dense flow base: `DenseOpticalFlow` with `Calc` and `CollectGarbage`.
- Sparse flow base: `SparseOpticalFlow` with Mat-based point-set input/output.
- Objects: `DualTVL1OpticalFlow`, `DenseRLOFOpticalFlow`, `SparseRLOFOpticalFlow`, and `RLOFOpticalFlowParameter`.
- Factories and helpers: DeepFlow, SimpleFlow, Farneback, SparseToDense, SimpleFlow static overloads, dense/sparse RLOF static overloads.
- Motion templates: `UpdateMotionHistory`, `CalcMotionGradient`, `CalcGlobalOrientation`, and `SegmentMotion`.

- 密集光流基类：带 `Calc` 与 `CollectGarbage` 的 `DenseOpticalFlow`。
- 稀疏光流基类：用 `Mat` 表达点集输入/输出的 `SparseOpticalFlow`。
- 对象：`DualTVL1OpticalFlow`、`DenseRLOFOpticalFlow`、`SparseRLOFOpticalFlow` 和 `RLOFOpticalFlowParameter`。
- 工厂与 helper：DeepFlow、SimpleFlow、Farneback、SparseToDense、SimpleFlow 静态重载、dense/sparse RLOF 静态重载。
- Motion template：`UpdateMotionHistory`、`CalcMotionGradient`、`CalcGlobalOrientation` 和 `SegmentMotion`。

## Runtime / 运行时

`optflow` is an optional OpenCV contrib module. A linked runtime should include the factual OpenCV 5.0.0 runtime artifact `opencv_optflow500.dll`. Several algorithms can also use the staged contrib `ximgproc` module, which is now exposed directly through `OpenCvSharp.XImgProc`. If the module is not linked, the exported ABI remains present and managed calls report `NOT_LINKED`.

`optflow` 是可选 OpenCV contrib 模块。linked runtime 应包含事实性 OpenCV 5.0.0 runtime 产物 `opencv_optflow500.dll`。部分算法也可以使用已暂存的 contrib `ximgproc` 模块，该模块现在也通过 `OpenCvSharp.XImgProc` 直接暴露。如果模块未链接，导出的 ABI 仍存在，managed 调用会报告 `NOT_LINKED`。

## Data Notes / 数据说明

Dense optical-flow output is usually a two-channel floating matrix, commonly `CV_32FC2`, with one flow vector per pixel. Sparse RLOF uses caller-owned `Mat` values for point sets, status, and error output so the C ABI never exposes `std::vector<Point2f>`.

密集光流输出通常是双通道浮点矩阵，常见为 `CV_32FC2`，每个像素对应一个光流向量。Sparse RLOF 使用调用方持有的 `Mat` 表达点集、状态和误差输出，因此 C ABI 不暴露 `std::vector<Point2f>`。

RLOF cross support regions are sensitive to channel count and image content. Tiny smoke tests verify the linked call path and shape, not real tracking quality.

RLOF cross support region 对输入通道数和图像内容敏感。tiny smoke 只验证 linked 调用路径和输出形状，不代表真实跟踪质量。

```csharp
using OpenCvSharp.Core;
using OpenCvSharp.ImgProc;
using OpenCvSharp.OptFlow;

using Mat first = new Mat(24, 24, MatType.CV_8UC3, new Scalar(20, 40, 60));
using Mat second = first.Clone();
using Mat flow = new Mat();

Cv2.Rectangle(second, new Rect(6, 5, 8, 9), new Scalar(200, 30, 80), -1);
OptFlowCv2.CalcOpticalFlowSparseToDense(first, second, flow, gridStep: 4, k: 8, usePostProc: false);

using DualTVL1OpticalFlow tvl1 = DualTVL1OpticalFlow.Create(nscales: 2, warps: 1);
using Mat gray0 = new Mat();
using Mat gray1 = new Mat();
using Mat tvl1Flow = new Mat();
Cv2.CvtColor(first, gray0, ColorConversionCodes.BGR2GRAY);
Cv2.CvtColor(second, gray1, ColorConversionCodes.BGR2GRAY);
tvl1.Calc(gray0, gray1, tvl1Flow);
```

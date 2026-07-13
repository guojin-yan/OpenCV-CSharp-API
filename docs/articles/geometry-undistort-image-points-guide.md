# Geometry Undistort Image Points Guide

## Overview

`OpenCvSharp.Calib3D.Cv2.UndistortImagePoints` exposes OpenCV's
`cv::undistortImagePoints` utility for correcting distorted image-point positions while keeping
the result in pixel coordinates.

`OpenCvSharp.Calib3D.Cv2.UndistortImagePoints` 封装 OpenCV 的
`cv::undistortImagePoints` 工具，用于校正畸变像点，并始终将结果保留在像素坐标系中。

The operation is equivalent to calling `UndistortPoints` with:

- no rectification transform,
- the original camera matrix as the projection matrix,
- the same termination criteria.

该操作等价于调用 `UndistortPoints`，并固定使用：

- 空校正变换；
- 原相机矩阵作为投影矩阵；
- 相同的终止条件。

## Coordinate Behavior

Input points are distorted pixel coordinates. Output points are undistorted pixel coordinates
using the original focal lengths and principal point. This differs from `UndistortPoints`
without a projection matrix, which returns normalized camera coordinates.

输入点是畸变像素坐标；输出点使用原焦距和主点，仍为无畸变像素坐标。它与未提供投影矩阵的
`UndistortPoints` 不同，后者返回归一化相机坐标。

## Supported Inputs

Point matrices may use:

- `CV_32F` or `CV_64F` depth;
- `N x 2` or `2 x N` single-channel layout;
- `N x 1` or `1 x N` two-channel layout.

点矩阵支持：

- `CV_32F` 或 `CV_64F` 深度；
- `N x 2` 或 `2 x N` 单通道布局；
- `N x 1` 或 `1 x N` 双通道布局。

The camera matrix must be a floating-point, single-channel `3 x 3` matrix. Distortion
coefficients may be empty for zero distortion or contain 4, 5, 8, 12, or 14 floating-point
values.

相机矩阵必须是浮点、单通道 `3 x 3` 矩阵。畸变系数可以为空以表示零畸变，也可以包含
4、5、8、12 或 14 个浮点值。

## Ownership And Precision

The caller-owned overload writes to an existing output `Mat`. The owned overload allocates and
returns an independent `Mat`, disposing it if validation or native execution fails. Output point
depth matches the source point depth.

caller-owned 重载写入已有输出 `Mat`。owned 重载分配并返回独立 `Mat`，若验证或 native
执行失败会释放该对象。输出点深度与源点深度一致。

Managed `Point2f[]` and modern `ReadOnlySpan<Point2f>` overloads create an owned
single-precision point matrix. All input Mats and managed point collections remain unchanged.

托管 `Point2f[]` 和现代 `ReadOnlySpan<Point2f>` 重载会创建拥有所有权的单精度点矩阵。
所有输入 Mat 和托管点集合均保持不变。

## Termination Criteria

The OpenCV default is five maximum iterations with epsilon metadata `0.01`. A custom
`TermCriteria` can request count-based, epsilon-based, or combined stopping behavior. Enabled
counts must be positive and enabled epsilon values must be finite and positive.

OpenCV 默认使用最多五次迭代，并携带 `0.01` 的 epsilon 元数据。自定义 `TermCriteria`
可以选择次数、epsilon 或组合终止方式；启用的次数必须为正，启用的 epsilon 必须是有限正数。

## Example

```csharp
using Mat cameraMatrix = CreateCameraMatrix();
using Mat distCoeffs = CreateDistortionCoefficients();

Point2f[] distorted =
{
    new Point2f(96.0F, 72.0F),
    new Point2f(320.0F, 240.0F),
    new Point2f(548.0F, 391.0F)
};

using Mat undistorted = OpenCvSharp.Calib3D.Cv2.UndistortImagePoints(
    distorted,
    cameraMatrix,
    distCoeffs);
```

The returned points are ready for pixel-space measurement, drawing, or comparison against
undistorted image content.

返回点可直接用于像素空间测量、绘制，或与去畸变图像内容进行比较。

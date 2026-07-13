# Stereo Disparity Utilities Guide

## Overview

`OpenCvSharp.Calib3D.Cv2` exposes four OpenCV stereo disparity utilities:

- `FilterSpeckles`
- `GetValidDisparityROI`
- `ValidateDisparity`
- `ReprojectImageTo3D`

`OpenCvSharp.Calib3D.Cv2` 提供四个 OpenCV 双目视差工具：

- `FilterSpeckles`
- `GetValidDisparityROI`
- `ValidateDisparity`
- `ReprojectImageTo3D`

These functions complement `StereoBM` and prepare the project for a later `StereoSGBM` wrapper.

这些函数补充 `StereoBM` 的视差处理能力，并为后续 `StereoSGBM` 封装做好准备。

## Fixed-Point Disparity Scale

OpenCV stereo matchers commonly return `CV_16SC1` disparity maps with four fractional bits. The
stored value is therefore the real disparity multiplied by `StereoBM.DispScale`, which is 16.

OpenCV 双目匹配器通常返回带四个小数位的 `CV_16SC1` 视差图，因此存储值等于真实视差乘以
`StereoBM.DispScale`，即 16。

`FilterSpeckles` and `ValidateDisparity` operate on this stored scale. `ReprojectImageTo3D` does
not divide fixed-point values automatically; convert the disparity map to floating point and scale
by `1.0 / 16.0` first when real disparity units are required.

`FilterSpeckles` 和 `ValidateDisparity` 使用该存储尺度。`ReprojectImageTo3D` 不会自动除以
16；需要真实视差单位时，应先将视差图转换为浮点并乘以 `1.0 / 16.0`。

## Speckle Filtering

`FilterSpeckles` modifies a single-channel `CV_8U` or `CV_16S` disparity image in place. It uses
four-connected regions and replaces regions whose size is at most `maxSpeckleSize`.

`FilterSpeckles` 原地修改单通道 `CV_8U` 或 `CV_16S` 视差图。它使用四连通区域，并替换大小
不超过 `maxSpeckleSize` 的区域。

OpenCV rounds `newValue` and `maxDifference` to integers. An optional caller-owned buffer can be
reused across calls; OpenCV may resize it. The buffer must not alias the disparity image.

OpenCV 会将 `newValue` 和 `maxDifference` 四舍五入为整数。可选的调用方缓冲区可以跨调用
复用，OpenCV 可能调整其大小。缓冲区不得与视差图别名。

## Valid Disparity ROI

`GetValidDisparityROI` combines the valid ROIs of two rectified images with the minimum disparity,
disparity count, and matching block size. It preserves OpenCV's exact integer arithmetic and
returns an empty `Rect` when no positive valid area remains.

`GetValidDisparityROI` 根据两幅校正图像的有效 ROI、最小视差、视差数量和匹配块大小计算结果。
它保留 OpenCV 的精确整数运算；没有正面积时返回空 `Rect`。

## Left-Right Validation

`ValidateDisparity` modifies a `CV_16SC1` disparity map in place. The cost map must have identical
dimensions and use `CV_16SC1` or `CV_32SC1`. The cost map remains unchanged.

`ValidateDisparity` 原地修改 `CV_16SC1` 视差图。代价图必须具有相同尺寸，并使用
`CV_16SC1` 或 `CV_32SC1`；代价图保持不变。

The invalid disparity sentinel is:

```text
(minDisparity - 1) * 16
```

无效视差标记为：

```text
(minDisparity - 1) * 16
```

## Reprojection To 3D

`ReprojectImageTo3D` accepts single-channel disparity images with these types:

- `CV_8U`
- `CV_16S`
- `CV_32S`
- `CV_32F`

`ReprojectImageTo3D` 支持以下单通道视差类型：

- `CV_8U`
- `CV_16S`
- `CV_32S`
- `CV_32F`

The Q matrix must be single-channel `4 x 4`. For each pixel, OpenCV applies:

```text
[X, Y, Z, W]^T = Q * [x, y, disparity, 1]^T
```

and divides X, Y, and Z by W.

Q 矩阵必须是单通道 `4 x 4`。OpenCV 对每个像素执行上述齐次变换，并将 X、Y、Z 除以 W。

The default output is `CV_32FC3`. Explicit output depths are `CV_16S`, `CV_32S`, and `CV_32F`;
the output always has three channels and the same rows and columns as the disparity map.

默认输出为 `CV_32FC3`。显式输出深度支持 `CV_16S`、`CV_32S` 和 `CV_32F`；输出始终为三通道，
并与视差图具有相同行列数。

When `handleMissingValues` is true, pixels containing the global minimum disparity receive
`Z = 10000`.

当 `handleMissingValues` 为 true 时，包含全局最小视差的像素会得到 `Z = 10000`。

## Ownership And Mutation

The caller-owned reprojection overload writes into an existing output Mat. The owned overload
returns a new Mat and disposes it if validation or native execution fails.

caller-owned 重投影重载写入已有输出 Mat。owned 重载返回新 Mat，并在验证或 native 执行失败时
释放它。

Mutation boundaries:

- `FilterSpeckles`: disparity and optional buffer may change
- `ValidateDisparity`: disparity changes; cost does not
- `GetValidDisparityROI`: no input mutation
- `ReprojectImageTo3D`: disparity and Q remain unchanged

修改边界：

- `FilterSpeckles`：视差图和可选缓冲区可能改变
- `ValidateDisparity`：视差图改变，代价图不变
- `GetValidDisparityROI`：不修改输入
- `ReprojectImageTo3D`：视差图和 Q 保持不变

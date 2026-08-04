# StereoSGBM Guide

## Overview / 概述

`JYPPX.OpenCvSharp.Calib3D.StereoSGBM` wraps the mainline OpenCV `cv::StereoSGBM`
algorithm as a sealed, disposable managed object. It is a parallel companion to the
existing `StereoBM` wrapper rather than a new managed `StereoMatcher` inheritance
hierarchy, which preserves the published `StereoBM` API.

`JYPPX.OpenCvSharp.Calib3D.StereoSGBM` 将 OpenCV 主线的 `cv::StereoSGBM` 算法封装为
密封、可释放的 managed 对象。它与现有 `StereoBM` 并列，而不是引入新的 managed
`StereoMatcher` 继承体系，从而保持已发布的 `StereoBM` API 不变。

Supported rectified input types are exactly:

- `CV_8UC1` grayscale images
- `CV_8UC3` color images

支持的校正后输入类型严格为：

- `CV_8UC1` 灰度图像
- `CV_8UC3` 彩色图像

The left and right images must have identical dimensions and types. `Compute` does not
modify either input. The caller-owned output must not alias an input because OpenCV may
resize or replace its storage.

左右图像必须具有相同尺寸和类型。`Compute` 不修改任何输入。调用方提供的输出不得与输入
别名，因为 OpenCV 可能调整或替换其存储。

## Basic Usage / 基本用法

```csharp
using JYPPX.OpenCvSharp.Calib3D;
using JYPPX.OpenCvSharp.Core;

using var left = new Mat(240, 320, MatType.CV_8UC1);
using var right = new Mat(240, 320, MatType.CV_8UC1);

const int blockSize = 3;
using StereoSGBM matcher = StereoSGBM.Create(
    minDisparity: 0,
    numDisparities: 64,
    blockSize: blockSize,
    p1: 8 * blockSize * blockSize,
    p2: 32 * blockSize * blockSize,
    disp12MaxDiff: 1,
    preFilterCap: 31,
    uniquenessRatio: 10,
    speckleWindowSize: 100,
    speckleRange: 2,
    mode: StereoSGBMMode.SGBM);

using Mat disparity = matcher.Compute(left, right);
```

The owned overload creates and returns a new `Mat`. The caller-owned overload writes into
an existing `Mat`:

owned 重载创建并返回新的 `Mat`。caller-owned 重载写入现有 `Mat`：

```csharp
using var disparity = new Mat();
matcher.Compute(left, right, disparity);
```

Both overloads produce a `CV_16SC1` result with the same rows and columns as the inputs.
The owned overload disposes its allocated output if validation or native execution fails.

两个重载都生成与输入行列数相同的 `CV_16SC1` 结果。若验证或 native 执行失败，owned
重载会释放它创建的输出。

## Fixed-Point Disparity / 定点视差

StereoSGBM stores disparity with four fractional bits:

StereoSGBM 使用四个小数位存储视差：

```text
real disparity = stored CV_16S value / StereoSGBM.DispScale
StereoSGBM.DispScale = 16
```

Invalid and border pixels may contain the sentinel associated with the configured minimum
disparity, commonly `(MinDisparity - 1) * 16`. Do not interpret the invalid border as a
measured negative disparity.

无效像素和边界像素可能包含与最小视差对应的标记值，通常为
`(MinDisparity - 1) * 16`。不要把无效边界误认为实际测得的负视差。

When real disparity units are required by a later operation, convert the map to floating
point and multiply by `1.0 / StereoSGBM.DispScale`.

后续操作需要真实视差单位时，应将视差图转换为浮点，并乘以
`1.0 / StereoSGBM.DispScale`。

## Search And Smoothness Parameters / 搜索与平滑参数

- `MinDisparity` selects the first disparity in the search interval.
- `NumDisparities` is normally positive and divisible by 16.
- `BlockSize` is normally odd and at least 1.
- `P1` penalizes disparity changes of plus or minus one.
- `P2` penalizes larger disparity changes and should normally be greater than `P1`.

- `MinDisparity` 指定搜索区间的起始视差。
- `NumDisparities` 通常为正数且可被 16 整除。
- `BlockSize` 通常为不小于 1 的奇数。
- `P1` 惩罚正负 1 的视差变化。
- `P2` 惩罚更大的视差变化，通常应大于 `P1`。

Common penalty recommendations are:

常用惩罚参数建议为：

```text
P1 = 8  * channels * BlockSize * BlockSize
P2 = 32 * channels * BlockSize * BlockSize
```

These are recommendations, not additional managed validation rules. Factory arguments and
property setters preserve OpenCV's permissive behavior, while OpenCV may reject unsuitable
combinations when computation begins.

这些是建议值，并不是额外的 managed 验证规则。工厂参数和属性 setter 保留 OpenCV 的
宽容行为；不合适的参数组合可能在计算开始时由 OpenCV 拒绝。

## Matching Filters / 匹配过滤

- `PreFilterCap` clips the pre-filtered image derivatives.
- `UniquenessRatio` rejects ambiguous matches whose best cost is insufficiently distinct.
- A non-positive `Disp12MaxDiff` disables the left-right consistency check.
- `SpeckleWindowSize = 0` disables speckle filtering.
- A positive `SpeckleRange` is internally multiplied by the disparity scale of 16.

- `PreFilterCap` 限制预滤波图像导数。
- `UniquenessRatio` 在最佳代价不够独特时拒绝歧义匹配。
- 非正的 `Disp12MaxDiff` 禁用左右一致性检查。
- `SpeckleWindowSize = 0` 禁用斑点过滤。
- 正的 `SpeckleRange` 在内部会乘以视差缩放因子 16。

## Modes / 模式

`StereoSGBMMode` exposes all four OpenCV modes:

`StereoSGBMMode` 暴露全部四种 OpenCV 模式：

- `SGBM = 0`: the standard single-pass mode.
- `HH = 1`: the full two-pass Hirschmuller variant; it can consume substantially more memory.
- `SGBM3Way = 2`: the three-way mode.
- `HH4 = 3`: the four-path full-scale mode.

- `SGBM = 0`：标准单遍模式。
- `HH = 1`：完整双遍 Hirschmuller 变体，可能消耗明显更多内存。
- `SGBM3Way = 2`：三路模式。
- `HH4 = 3`：四路径全尺度模式。

Unknown enum values are rejected by `Create` and the `Mode` setter before native execution.

`Create` 和 `Mode` setter 会在 native 执行前拒绝未知枚举值。

## Color Input / 彩色输入

For `CV_8UC3` input, include the channel count in the common `P1` and `P2`
recommendations. StereoSGBM reads all three channels but still returns one single-channel
disparity map.

对于 `CV_8UC3` 输入，计算常用 `P1` 和 `P2` 建议值时应包含通道数。StereoSGBM
读取三个通道，但仍返回单通道视差图。

## Post-Processing And 3D / 后处理与三维重建

The Round 968 disparity utilities can be used directly with StereoSGBM output:

Round 968 的视差工具可以直接处理 StereoSGBM 输出：

- `Cv2.GetValidDisparityROI` identifies the geometrically valid matching region.
- `Cv2.ValidateDisparity` applies left-right consistency validation to `CV_16SC1` maps.
- `Cv2.FilterSpeckles` removes small connected disparity regions in place.
- `Cv2.ReprojectImageTo3D` reprojects disparity using a `4 x 4` Q matrix.

- `Cv2.GetValidDisparityROI` 计算几何上有效的匹配区域。
- `Cv2.ValidateDisparity` 对 `CV_16SC1` 视差图执行左右一致性验证。
- `Cv2.FilterSpeckles` 原地移除较小的连通视差区域。
- `Cv2.ReprojectImageTo3D` 使用 `4 x 4` Q 矩阵重投影视差。

Convert fixed-point disparity to real floating-point disparity before reprojection when the Q
matrix expects real disparity units.

当 Q 矩阵要求真实视差单位时，应在重投影前把定点视差转换为浮点真实视差。

## Lifetime / 生命周期

`StereoSGBM` owns a native OpenCV matcher through a dedicated safe handle. Dispose the matcher
when it is no longer needed. Accessing properties or calling `Compute` after disposal throws
`ObjectDisposedException`.

`StereoSGBM` 通过独立的安全句柄持有 native OpenCV 匹配器。使用完成后应释放 matcher。
释放后访问属性或调用 `Compute` 会抛出 `ObjectDisposedException`。

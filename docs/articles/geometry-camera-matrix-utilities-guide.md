# Geometry Camera Matrix Utilities Guide

Round 966 adds managed support for constructing the default new camera matrix and for calculating
the inner and outer rectangles of an undistorted image plane.

Round 966 增加了默认新相机矩阵构建，以及无畸变图像平面内接/外接矩形计算支持。

## Default New Camera Matrix / 默认新相机矩阵

`Cv2.GetDefaultNewCameraMatrix` accepts a single-channel `3 x 3` camera matrix with `CV_32F` or
`CV_64F` depth. The output is always an independently owned `3 x 3 CV_64FC1` Mat.

`Cv2.GetDefaultNewCameraMatrix` 接受单通道 `3 x 3` 相机矩阵，深度为 `CV_32F` 或
`CV_64F`。输出始终是独立拥有的 `3 x 3 CV_64FC1` Mat。

When `centerPrincipalPoint` is `false`:

- all camera-matrix values are preserved;
- `CV_32F` input is converted to `CV_64F`;
- `CV_64F` input remains `CV_64F`;
- the image size is ignored and may be left as the default empty `Size`.

当 `centerPrincipalPoint` 为 `false` 时：

- 所有相机矩阵数值保持不变；
- `CV_32F` 输入会转换为 `CV_64F`；
- `CV_64F` 输入保持 `CV_64F`；
- 图像尺寸不会参与计算，可以保留默认的空 `Size`。

```csharp
using JYPPX.OpenCvSharp.Calib3D;
using JYPPX.OpenCvSharp.Core;
using Calib3DCv2 = JYPPX.OpenCvSharp.Calib3D.Cv2;

using Mat cameraMatrix = GetCameraMatrix();
using Mat newCameraMatrix =
    Calib3DCv2.GetDefaultNewCameraMatrix(cameraMatrix);
```

The owned result does not alias the input Mat, even though the upstream C++ implementation may
return a shallow Mat header for an uncentered `CV_64F` input. Mutating or disposing the managed
result therefore does not mutate or invalidate the input.

即使上游 C++ 实现在输入为未居中的 `CV_64F` 时可能返回浅层 Mat 头，托管 owned 结果也不会与
输入 Mat 别名。因此修改或释放托管结果不会修改或使输入失效。

## Centered Principal Point / 主点居中

When `centerPrincipalPoint` is `true`, the image width and height must both be positive. The output
preserves the focal lengths and all matrix elements except the principal point:

当 `centerPrincipalPoint` 为 `true` 时，图像宽度和高度都必须为正。输出保留焦距以及除主点
之外的全部矩阵元素：

```text
cx = (imageWidth  - 1) * 0.5
cy = (imageHeight - 1) * 0.5
```

```csharp
using Mat centered = Calib3DCv2.GetDefaultNewCameraMatrix(
    cameraMatrix,
    new Size(640, 480),
    centerPrincipalPoint: true);
```

For a `640 x 480` image, the centered principal point is exactly `(319.5, 239.5)`.

对于 `640 x 480` 图像，居中主点精确为 `(319.5, 239.5)`。

## Caller-Owned Output / 调用方持有输出

The caller-owned overload writes into an existing Mat:

caller-owned 重载会写入已有 Mat：

```csharp
using var output = new Mat();

Calib3DCv2.GetDefaultNewCameraMatrix(
    cameraMatrix,
    output,
    new Size(640, 480),
    centerPrincipalPoint: true);
```

The input and output Mats must not alias. The API never disposes caller-owned output Mats. The
owned overload disposes its newly allocated Mat if validation or native execution fails.

输入和输出 Mat 不得别名。API 不会释放 caller-owned 输出 Mat。owned 重载在验证或 native
执行失败时会释放新分配的 Mat。

## Undistort Rectangles / 去畸变矩形

`Cv2.GetUndistortRectangles` samples 32 points along the source-image border using the same
`9 x 9` conceptual grid as OpenCV 5.0.0. It undistorts those border points and returns:

`Cv2.GetUndistortRectangles` 使用与 OpenCV 5.0.0 相同的概念性 `9 x 9` 网格，在源图像边界
采样 32 个点，对这些边界点去畸变并返回：

- `inner`: the maximal inscribed `Rect2d`;
- `outer`: the minimal bounding `Rect2d`.

- `inner`：最大内接 `Rect2d`；
- `outer`：最小外接 `Rect2d`。

The camera matrix must be `3 x 3`. Distortion coefficients may be an empty Mat for zero distortion,
or a single-channel floating-point vector containing 4, 5, 8, 12, or 14 values.

相机矩阵必须为 `3 x 3`。畸变系数可以为空 Mat（表示零畸变），也可以是包含 4、5、8、12 或
14 个值的单通道浮点向量。

## Normalized Coordinates / 归一化坐标

When `newCameraMatrix` is omitted, OpenCV returns normalized undistorted coordinates:

未提供 `newCameraMatrix` 时，OpenCV 返回归一化无畸变坐标：

```csharp
Calib3DCv2.GetUndistortRectangles(
    cameraMatrix,
    distCoeffs,
    new Size(640, 480),
    out Rect2d normalizedInner,
    out Rect2d normalizedOuter);
```

For zero distortion and camera matrix
`fx=600`, `fy=610`, `cx=320`, `cy=240`, the rectangle is:

对于零畸变以及 `fx=600`、`fy=610`、`cx=320`、`cy=240` 的相机矩阵，矩形为：

```text
X      = -320 / 600
Y      = -240 / 610
Width  =  639 / 600
Height =  479 / 610
```

With zero distortion, `inner` and `outer` are equal because the mapping is affine.

零畸变时映射为仿射变换，因此 `inner` 与 `outer` 相等。

## Pixel And Projected Coordinates / 像素与投影坐标

Supplying a `3 x 3` new camera matrix or a `3 x 4` projection matrix returns rectangles in that
projected image plane:

提供 `3 x 3` 新相机矩阵或 `3 x 4` 投影矩阵时，返回矩形位于对应投影图像平面：

```csharp
Calib3DCv2.GetUndistortRectangles(
    cameraMatrix,
    distCoeffs,
    new Size(640, 480),
    out Rect2d pixelInner,
    out Rect2d pixelOuter,
    newCameraMatrix: cameraMatrix);
```

With zero distortion and the original camera matrix supplied as `newCameraMatrix`, both rectangles
are exactly:

零畸变且将原相机矩阵作为 `newCameraMatrix` 时，两个矩形都精确为：

```text
Rect2d(0, 0, imageWidth - 1, imageHeight - 1)
```

For a `640 x 480` image this is `Rect2d(0, 0, 639, 479)`, not
`Rect2d(0, 0, 640, 480)`, because OpenCV samples pixel-center coordinates from zero through
`width - 1` and `height - 1`.

对于 `640 x 480` 图像，结果是 `Rect2d(0, 0, 639, 479)`，而不是
`Rect2d(0, 0, 640, 480)`，因为 OpenCV 采样的像素中心坐标范围是从零到 `width - 1` 和
`height - 1`。

## Optional Rectification / 可选校正

The optional `r` input is a single-channel `3 x 3` floating-point rectification matrix. The
optional `newCameraMatrix` input is a single-channel `3 x 3` camera matrix or `3 x 4` projection
matrix. Null or empty optional Mats preserve the upstream omitted-input behavior.

可选 `r` 输入是单通道 `3 x 3` 浮点校正矩阵。可选 `newCameraMatrix` 输入是单通道
`3 x 3` 相机矩阵或 `3 x 4` 投影矩阵。空引用或空 Mat 会保留上游省略输入时的行为。

## Immutability And Precision / 不变性与精度

Camera, distortion, rectification, and projection Mats remain unchanged. Both returned rectangles
preserve all eight upstream `double` values without integer rounding.

相机、畸变、校正和投影 Mat 都保持不变。返回的两个矩形会保留上游全部八个 `double` 分量，
不会进行整数舍入。

## Validation Summary / 验证摘要

The managed API rejects:

托管 API 会拒绝：

- null, disposed, empty, non-`3 x 3`, multi-channel, or unsupported-depth camera matrices;
- aliased default-camera input and output Mats;
- non-positive centered or undistort image sizes;
- malformed distortion vectors;
- malformed optional rectification matrices;
- malformed optional camera/projection matrices.

- 空引用、已释放、空、非 `3 x 3`、多通道或深度不受支持的相机矩阵；
- 默认相机矩阵 API 中互相别名的输入和输出 Mat；
- 非正的居中尺寸或去畸变图像尺寸；
- 格式错误的畸变向量；
- 格式错误的可选校正矩阵；
- 格式错误的可选相机/投影矩阵。

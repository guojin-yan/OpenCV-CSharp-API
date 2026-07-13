# Geometry Fisheye Projection And Pose Guide

Round 962 adds direct managed support for fisheye projection, point distortion and undistortion,
new-camera-matrix estimation, PnP, and PnP RANSAC.

Round 962 增加了鱼眼投影、点畸变与去畸变、新相机矩阵估计、PnP 和 PnP RANSAC 的直接托管支持。

## Fisheye Versus Pinhole / 鱼眼与针孔投影

The fisheye APIs use the four-coefficient angle model `D = [k1, k2, k3, k4]`. They are separate
from the normal pinhole `ProjectPoints`, `SolvePnP`, and `SolvePnPRansac` APIs, whose distortion
coefficients have different meanings.

鱼眼 API 使用四系数角度模型 `D = [k1, k2, k3, k4]`。它们与普通针孔
`ProjectPoints`、`SolvePnP` 和 `SolvePnPRansac` API 相互独立，后者的畸变系数语义不同。

Do not pass pinhole distortion coefficients to the fisheye methods, and do not treat fisheye image
points as pinhole observations.

不要把针孔畸变系数传给鱼眼方法，也不要把鱼眼像点当作针孔观测点处理。

## Projection / 三维点投影

`Cv2.FisheyeProjectPoints` accepts object points, a Rodrigues rotation vector, a translation
vector, a `3 x 3` camera matrix, and exactly four fisheye distortion values.

`Cv2.FisheyeProjectPoints` 接收物点、Rodrigues 旋转向量、平移向量、`3 x 3` 相机矩阵以及恰好
四个鱼眼畸变值。

The output follows the library point-matrix convention: `N x 1` with two channels. A supplied
Jacobian is caller-owned and is written as `2N x 15`. The columns follow the upstream fisheye
parameter order for rotation, translation, focal lengths, principal point, four distortion
coefficients, and skew alpha.

输出遵循库的点矩阵约定：`N x 1`、双通道。调用方提供的 Jacobian 由调用方持有，输出形状为
`2N x 15`。列顺序遵循上游鱼眼参数顺序，包括旋转、平移、焦距、主点、四个畸变系数以及斜切
参数 alpha。

Managed validation rejects non-floating point object points, rotation vectors, translation vectors,
camera matrices, and fisheye distortion vectors before the native call. Caller-owned `imagePoints`
and optional `jacobian` outputs must not alias any input matrix or each other.

托管层会在 native 调用前拒绝非浮点的物点、旋转向量、平移向量、相机矩阵和鱼眼畸变向量。
调用方持有的 `imagePoints` 与可选 `jacobian` 输出不得与任何输入矩阵或彼此使用相同别名。

```csharp
using OpenCvSharp.Calib3D;
using OpenCvSharp.Core;
using Calib3DCv2 = OpenCvSharp.Calib3D.Cv2;

using var imagePoints = new Mat();
using var jacobian = new Mat();

Calib3DCv2.FisheyeProjectPoints(
    objectPointMat,
    rvec,
    tvec,
    cameraMatrix,
    distCoeffs,
    imagePoints,
    alpha: 0.0,
    jacobian: jacobian);

Console.WriteLine($"{imagePoints.Rows}x{imagePoints.Cols}");
Console.WriteLine($"{jacobian.Rows}x{jacobian.Cols}");
```

Array and `ReadOnlySpan<Point3f>` overloads convert through the established point-marshalling
helpers and return an owned image-point matrix.

数组和 `ReadOnlySpan<Point3f>` 重载复用既有点封送 helper，并返回由调用方持有的像点矩阵。

## Normalized And Pixel Coordinates / 归一化与像素坐标

The two distortion modes intentionally use different input coordinate systems:

两种畸变模式有意使用不同的输入坐标系：

- `FisheyeDistortPoints` treats input points as normalized coordinates under an identity camera.
- `FisheyeDistortPointsWithCameraMatrix` treats input points as undistorted pixels described by
  `undistortedCameraMatrix`.
- Both methods return distorted pixel coordinates under `cameraMatrix`.

- `FisheyeDistortPoints` 把输入点视为单位相机下的归一化坐标。
- `FisheyeDistortPointsWithCameraMatrix` 把输入点视为由 `undistortedCameraMatrix` 描述的无畸变像素。
- 两种方法都返回 `cameraMatrix` 下的畸变像素坐标。

The explicit `WithCameraMatrix` name avoids an ambiguous C# overload made only from four `Mat`
parameters. It maps directly to the upstream `Kundistorted` overload.

显式的 `WithCameraMatrix` 名称避免了仅由四个 `Mat` 参数组成的 C# 重载歧义，并直接映射到上游
`Kundistorted` 重载。

```csharp
using Mat distortedNormalized =
    Calib3DCv2.FisheyeDistortPoints(
        normalizedPoints,
        cameraMatrix,
        distCoeffs);

using Mat distortedPixels =
    Calib3DCv2.FisheyeDistortPointsWithCameraMatrix(
        undistortedPixelPoints,
        undistortedCameraMatrix,
        cameraMatrix,
        distCoeffs);
```

The `Kundistorted` overload performs its normalization in double precision, so its returned point
matrix may be `CV_64FC2` even when managed input points are `Point2f`.

`Kundistorted` 重载会以双精度执行归一化，因此即使托管输入为 `Point2f`，返回点矩阵也可能是
`CV_64FC2`。

## Undistortion And Rectification / 去畸变与校正

`FisheyeUndistortPoints` removes fisheye distortion and optionally applies rectification `R` and
projection `P`.

`FisheyeUndistortPoints` 去除鱼眼畸变，并可选应用校正矩阵 `R` 与投影矩阵 `P`。

- Without `P`, output points are normalized camera coordinates.
- With a `3 x 3` or `3 x 4` `P`, output points are expressed in that projected pixel coordinate
  system.
- `R` may be empty, a `3 x 3` rotation matrix, or a supported three-value Rodrigues vector.
- The default criteria are `CountOrEps`, 10 iterations, and epsilon `1e-8`.

- 未提供 `P` 时，输出为归一化相机坐标。
- 提供 `3 x 3` 或 `3 x 4` 的 `P` 时，输出位于该投影像素坐标系。
- `R` 可以为空、`3 x 3` 旋转矩阵或受支持的三元素 Rodrigues 向量。
- 默认终止条件为 `CountOrEps`、10 次迭代和 `1e-8` epsilon。

```csharp
using Mat normalized = Calib3DCv2.FisheyeUndistortPoints(
    distortedPixels,
    cameraMatrix,
    distCoeffs);

using Mat rectifiedPixels = Calib3DCv2.FisheyeUndistortPoints(
    distortedPixels,
    cameraMatrix,
    distCoeffs,
    r: rectification,
    p: newCameraMatrix);
```

For the caller-owned overload, named arguments make the output parameter explicit:

caller-owned 重载可通过命名参数明确输出矩阵：

```csharp
Calib3DCv2.FisheyeUndistortPoints(
    distorted: distortedPixels,
    undistorted: outputPoints,
    cameraMatrix: cameraMatrix,
    distCoeffs: distCoeffs,
    r: rectification,
    p: newCameraMatrix);
```

## New Camera Matrix / 新相机矩阵

`FisheyeEstimateNewCameraMatrixForUndistortRectify` estimates a `3 x 3` matrix for an undistorted or
rectified image.

`FisheyeEstimateNewCameraMatrixForUndistortRectify` 为去畸变或校正图像估计 `3 x 3` 相机矩阵。

- `balance` is in `[0, 1]`. Lower values crop more aggressively; higher values retain more field
  of view.
- `fovScale` must be positive and scales the resulting field of view.
- Omitting `newSize` passes an empty size to OpenCV and preserves upstream behavior.
- `R` is optional and uses the same rectification validation as point undistortion.

- `balance` 位于 `[0, 1]`。较低值更积极地裁剪，较高值保留更多视场。
- `fovScale` 必须为正数，用于缩放输出视场。
- 省略 `newSize` 时会向 OpenCV 传入空尺寸，保留上游行为。
- `R` 可选，并与点去畸变使用相同的校正验证规则。

```csharp
using Mat newCameraMatrix =
    Calib3DCv2.FisheyeEstimateNewCameraMatrixForUndistortRectify(
        cameraMatrix,
        distCoeffs,
        imageSize,
        r: rectification,
        balance: 0.5,
        newSize: imageSize,
        fovScale: 1.0);
```

## Fisheye PnP / 鱼眼 PnP

`FisheyeSolvePnP` and `FisheyeSolvePnPRansac` solve pose using fisheye observations. They validate
matching 3D and 2D point counts, the `3 x 3` camera matrix, four distortion values, supported
`SolvePnPFlags`, and fisheye termination criteria before native execution.

`FisheyeSolvePnP` 和 `FisheyeSolvePnPRansac` 使用鱼眼观测点求解位姿。进入 native 层前会验证
三维与二维点数匹配、`3 x 3` 相机矩阵、四个畸变值、受支持的 `SolvePnPFlags` 以及鱼眼终止条件。

`rvec` and `tvec` are caller-owned input-output matrices when `useExtrinsicGuess` is true. RANSAC
also validates a positive iteration count and reprojection threshold plus confidence strictly
between zero and one.

当 `useExtrinsicGuess` 为 `true` 时，`rvec` 与 `tvec` 是调用方持有的输入输出矩阵。RANSAC 还会验证
正的迭代次数、正的重投影阈值，以及严格位于零与一之间的置信度。

```csharp
using var rvec = new Mat();
using var tvec = new Mat();

bool solved = Calib3DCv2.FisheyeSolvePnP(
    objectPoints,
    imagePoints,
    cameraMatrix,
    distCoeffs,
    rvec,
    tvec);
```

```csharp
using var ransacRvec = new Mat();
using var ransacTvec = new Mat();
using var inliers = new Mat();

bool robustSolved = Calib3DCv2.FisheyeSolvePnPRansac(
    objectPoints,
    imagePointsWithOutliers,
    cameraMatrix,
    distCoeffs,
    ransacRvec,
    ransacTvec,
    iterationsCount: 300,
    reprojectionError: 2.0F,
    confidence: 0.999,
    inliers: inliers);
```

The optional `inliers` output contains zero-based point indices in a single-channel integer matrix.

可选 `inliers` 输出是单通道整数矩阵，包含从零开始的点索引。

Array and `ReadOnlySpan<Point3f>` / `ReadOnlySpan<Point2f>` overloads preserve the same behavior as
the Mat overloads.

数组以及 `ReadOnlySpan<Point3f>` / `ReadOnlySpan<Point2f>` 重载与 Mat 重载保持相同行为。

## Ownership / 所有权

Caller-owned overloads write into supplied matrices and never dispose them. Owned-output overloads
return a newly allocated matrix. If native execution fails, every partially allocated owned output
is disposed before the exception is rethrown.

caller-owned 重载写入调用方提供的矩阵，且不会释放它们。owned-output 重载返回新分配的矩阵。
如果 native 执行失败，重新抛出异常前会释放所有已部分分配的 owned 输出。

Always dispose successful owned matrices with `using` or an equivalent `try/finally`.

成功获得的 owned 矩阵必须通过 `using` 或等效的 `try/finally` 释放。

## Deferred Affine Overload / 延后支持的 Affine 重载

OpenCV also exposes a C++-only fisheye projection overload that accepts `Affine3d`. It is not marked
as a managed-bindable wrapper declaration, and this library currently has no `Affine3d` managed
type. Round 962 intentionally leaves that overload unsupported instead of introducing a partial or
versioned type.

OpenCV 还提供一个仅限 C++、接收 `Affine3d` 的鱼眼投影重载。它没有标记为可托管绑定的 wrapper
声明，并且本库目前没有 `Affine3d` 托管类型。Round 962 有意暂不支持该重载，而不是引入不完整或
带版本号的类型。

## Runtime Notes / 运行时说明

The linked implementation calls the OpenCV 5.0.0 Geometry fisheye functions. Project-owned API,
ABI, assembly, package, file, and namespace names remain version-neutral.

链接实现调用 OpenCV 5.0.0 Geometry 鱼眼函数。项目自有 API、ABI、程序集、包、文件和命名空间名称
保持版本中立。

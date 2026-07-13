# Geometry Homogeneous And Epipolar Utilities Guide

Round 963 adds direct managed support for homogeneous point conversion, optimal epipolar
correspondence correction, and Sampson distance.

Round 963 增加了齐次点转换、最优极线对应点校正和 Sampson 距离的直接托管支持。

## Coordinates / 坐标表示

`Cv2.ConvertPointsToHomogeneous` appends a final coordinate of one:

`Cv2.ConvertPointsToHomogeneous` 在末尾追加一个值为一的坐标：

```text
(x, y)       -> (x, y, 1)
(x, y, z)    -> (x, y, z, 1)
```

`Cv2.ConvertPointsFromHomogeneous` performs perspective division:

`Cv2.ConvertPointsFromHomogeneous` 执行透视除法：

```text
(x, y, w)       -> (x / w, y / w)
(x, y, z, w)    -> (x / w, y / w, z / w)
```

OpenCV 5.0.0 uses a scale of one when the final coordinate is zero or numerically close to zero.
Therefore `(x, y, 0)` produces `(x, y)`, not infinities, NaN values, or zero coordinates. This
guide documents the linked upstream implementation rather than assuming mathematical division by
zero behavior.

当末坐标为零或在数值上接近零时，OpenCV 5.0.0 使用比例因子一。因此 `(x, y, 0)` 输出
`(x, y)`，而不是无穷、NaN 或全零坐标。这里记录的是实际链接的上游实现行为，而不是数学上的
除零推断。

## Point Matrices / 点矩阵

The Mat overloads accept the point layouts recognized by OpenCV `Mat::checkVector`:

Mat 重载接受 OpenCV `Mat::checkVector` 识别的点布局：

- `N x 1` or `1 x N` multi-channel vectors, such as `CV_32FC2`.
- `N x D` single-channel matrices, where `D` is the point component count.
- Two or three source components for conversion to homogeneous coordinates.
- Three or four source components for conversion from homogeneous coordinates.

- `N x 1` 或 `1 x N` 的多通道向量，例如 `CV_32FC2`。
- `N x D` 的单通道矩阵，其中 `D` 是每个点的分量数。
- 转换到齐次坐标时，源点可包含两个或三个分量。
- 从齐次坐标转换时，源点可包含三个或四个分量。

A transposed single-channel `D x N` matrix is not a point vector under these rules. Convert or
transpose it to `N x D` before calling the API.

转置后的单通道 `D x N` 矩阵不符合这些规则。调用前应将其转换或转置为 `N x D`。

OpenCV writes conversion results as `N x 1` multi-channel matrices, regardless of whether the
accepted input was a row vector, column vector, or scalar-component matrix.

无论输入是行向量、列向量还是标量分量矩阵，OpenCV 都会把转换结果写成 `N x 1` 多通道矩阵。

## Depth Selection / 深度选择

Conversion sources may use `CV_32S`, `CV_32F`, or `CV_64F`. The optional `dtype` argument accepts:

转换源可使用 `CV_32S`、`CV_32F` 或 `CV_64F`。可选 `dtype` 参数接受：

- `-1` to use the upstream default.
- `MatType.CV_32F` for single-precision output.
- `MatType.CV_64F` for double-precision output.

- `-1`，使用上游默认规则。
- `MatType.CV_32F`，输出单精度结果。
- `MatType.CV_64F`，输出双精度结果。

For conversion to homogeneous coordinates, `-1` preserves the input depth, including integer
input. For conversion from homogeneous coordinates, `-1` produces `CV_32F` from `CV_32S` or
`CV_32F`, and `CV_64F` from `CV_64F`.

转换到齐次坐标时，`-1` 保留输入深度，包括整数输入。转换回欧氏坐标时，`CV_32S` 或
`CV_32F` 输入在 `-1` 下输出 `CV_32F`，`CV_64F` 输入输出 `CV_64F`。

```csharp
using OpenCvSharp.Calib3D;
using OpenCvSharp.Core;
using Calib3DCv2 = OpenCvSharp.Calib3D.Cv2;

Point2f[] points =
{
    new Point2f(2.0F, 4.0F),
    new Point2f(-3.0F, 6.0F)
};

using Mat homogeneous32 =
    Calib3DCv2.ConvertPointsToHomogeneous(points);

using Mat homogeneous64 =
    Calib3DCv2.ConvertPointsToHomogeneous(
        points,
        MatType.CV_64F);

using Mat roundTrip =
    Calib3DCv2.ConvertPointsFromHomogeneous(homogeneous32);
```

`Point2f[]` and `ReadOnlySpan<Point2f>` provide the unambiguous 2D-to-3D conversion.
`Point3f[]` and `ReadOnlySpan<Point3f>` support both 3D-to-4D conversion and homogeneous
3D-to-Euclidean-2D conversion. Four-component conversion remains Mat-based because the public API
does not expose a dedicated four-dimensional point abstraction.

`Point2f[]` 和 `ReadOnlySpan<Point2f>` 提供无歧义的二维到三维转换。`Point3f[]` 和
`ReadOnlySpan<Point3f>` 同时支持三维到四维以及齐次三维到欧氏二维转换。四分量转换保持
Mat 形式，因为公开 API 没有专用四维点抽象。

## Correct Matches / 对应点校正

`Cv2.CorrectMatches` implements OpenCV's optimal triangulation correction. The `3 x 3`
fundamental matrix maps points from image 1 to epipolar lines in image 2:

`Cv2.CorrectMatches` 实现 OpenCV 的最优三角化校正。`3 x 3` 基础矩阵把图像一中的点映射到
图像二中的极线：

```text
l2 = F * p1
p2^T * F * p1 = 0
```

The point inputs must:

输入点必须满足：

- Be non-empty `CV_32FC2` or `CV_64FC2` row or column vectors.
- Have identical size, orientation, depth, and point count.
- Use the same floating-point depth as each other.

- 是非空 `CV_32FC2` 或 `CV_64FC2` 行向量或列向量。
- 具有完全相同的大小、方向、深度和点数。
- 两组点使用相同浮点深度。

The corrected outputs preserve the corresponding input shape and type. They minimize the sum of
squared geometric correction distances subject to the epipolar constraint. The method does not
directly minimize Sampson distance.

校正输出保留对应输入的形状和类型。它们在满足极线约束的前提下，最小化两幅图像中几何校正
距离平方和。该方法并不直接最小化 Sampson 距离。

```csharp
using Mat fundamentalMatrix = CreateFundamentalMatrix();

Calib3DCv2.CorrectMatches(
    fundamentalMatrix,
    points1,
    points2,
    out Mat corrected1,
    out Mat corrected2);

using (corrected1)
using (corrected2)
{
    // Verify corrected2[i]^T * F * corrected1[i] is near zero.
}
```

The Mat overload also supports caller-owned output matrices. Array and
`ReadOnlySpan<Point2f>` overloads allocate two owned result matrices.

Mat 重载也支持调用方持有的输出矩阵。数组和 `ReadOnlySpan<Point2f>` 重载会分配两个 owned
结果矩阵。

## Two-Camera Essential Matrix / 双相机本质矩阵

Round 976 adds managed overloads for OpenCV's two-camera essential-matrix and pose-recovery
workflow. Use these overloads when corresponding image points come from cameras with different
intrinsics or different distortion coefficients:

Round 976 增加了 OpenCV 双相机本质矩阵和位姿恢复工作流的托管重载。当两组对应图像点来自
内参或畸变系数不同的相机时，应使用这些重载：

```csharp
using Mat essential = Calib3DCv2.FindEssentialMat(
    points1,
    points2,
    cameraMatrix1,
    distCoeffs1,
    cameraMatrix2,
    distCoeffs2,
    RobustEstimationAlgorithms.RANSAC,
    0.999,
    1.0,
    mask);

RecoverPoseResult pose = Calib3DCv2.RecoverPose(
    points1,
    points2,
    cameraMatrix1,
    distCoeffs1,
    cameraMatrix2,
    distCoeffs2,
    recoveredEssential,
    rotation,
    translation,
    RobustEstimationAlgorithms.RANSAC,
    0.999,
    1.0,
    mask);
```

`FindEssentialMat` returns an owned `3 x 3` essential matrix. `RecoverPose` writes the essential
matrix, rotation, and translation direction into caller-owned matrices and returns a
`RecoverPoseResult` containing the inlier count. The optional mask follows OpenCV's
input/output inlier-mask behavior for pose recovery.

`FindEssentialMat` 返回 owned 的 `3 x 3` 本质矩阵。`RecoverPose` 将本质矩阵、旋转矩阵和
平移方向写入调用方持有的矩阵，并返回包含内点数量的 `RecoverPoseResult`。可选 mask 遵循
OpenCV 位姿恢复中的输入输出内点掩码行为。

## Sampson Distance / Sampson 距离

`Cv2.SampsonDistance` evaluates the first-order approximation to geometric reprojection error:

`Cv2.SampsonDistance` 计算几何重投影误差的一阶近似：

```text
                         (p2^T F p1)^2
d = ---------------------------------------------------------
    (F p1)x^2 + (F p1)y^2 + (F^T p2)x^2 + (F^T p2)y^2
```

The Mat overload intentionally requires:

Mat 重载有意要求：

- Each point is a `3 x 1`, single-channel `CV_64F` homogeneous vector.
- The fundamental matrix is `3 x 3`, single-channel `CV_64F`.

- 每个点都是 `3 x 1`、单通道 `CV_64F` 齐次向量。
- 基础矩阵是 `3 x 3`、单通道 `CV_64F`。

The `Point2d` convenience overload constructs temporary `[x, y, 1]` vectors and disposes them
before returning.

`Point2d` 便捷重载会构造临时 `[x, y, 1]` 向量，并在返回前释放。

```csharp
double distance = Calib3DCv2.SampsonDistance(
    new Point2d(10.0, 20.0),
    new Point2d(12.0, 23.0),
    fundamentalMatrix);
```

A distance near zero indicates that the correspondence nearly satisfies the epipolar constraint.
It is an approximation and should not be interpreted as the exact geometric correction returned by
`CorrectMatches`.

接近零的距离表示对应点近似满足极线约束。该值是一阶近似，不应被解释为 `CorrectMatches` 返回的
精确几何校正量。

## Ownership / 所有权

Caller-owned overloads write into supplied matrices and never dispose them. Owned-output
conversion overloads return a new Mat. The owned `CorrectMatches` overload returns two matrices
through `out` parameters.

caller-owned 重载写入调用方提供的矩阵，且不会释放它们。owned-output 转换重载返回新的 Mat。
owned `CorrectMatches` 重载通过两个 `out` 参数返回矩阵。

If native execution fails, every matrix allocated by an owned-output overload is disposed before
the exception is rethrown. Successful owned outputs remain the caller's responsibility and should
be enclosed in `using`.

如果 native 执行失败，owned-output 重载分配的每个矩阵都会在重新抛出异常前释放。成功返回的
owned 输出仍由调用方负责，应放入 `using` 中。

## Intentionally Omitted API / 有意省略的 API

OpenCV also contains the obsolete `convertPointsHomogeneous` helper. It is not marked as a
managed-bindable wrapper API and has been replaced upstream by the two direction-specific
functions. The managed library intentionally exposes only `ConvertPointsToHomogeneous` and
`ConvertPointsFromHomogeneous`.

OpenCV 还包含已废弃的 `convertPointsHomogeneous` helper。它没有标记为可托管绑定的 wrapper
API，并已被上游的两个方向明确函数替代。托管库有意只公开
`ConvertPointsToHomogeneous` 和 `ConvertPointsFromHomogeneous`。

## Runtime Notes / 运行时说明

The linked implementation calls the OpenCV 5.0.0 Geometry functions. Project-owned API, ABI,
assembly, package, file, and namespace names remain version-neutral.

链接实现调用 OpenCV 5.0.0 Geometry 函数。项目自有 API、ABI、程序集、包、文件和命名空间名称
保持版本中立。

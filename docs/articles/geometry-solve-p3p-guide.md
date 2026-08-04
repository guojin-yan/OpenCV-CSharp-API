# Geometry SolveP3P Guide

Round 972 adds direct P3P and AP3P pose solving to `JYPPX.OpenCvSharp.Calib3D.Cv2`.

Round 972 在 `JYPPX.OpenCvSharp.Calib3D.Cv2` 中增加直接 P3P 和 AP3P 位姿求解。

## Problem And Inputs / 问题与输入

`SolveP3P` estimates camera pose from matched 3D object points and 2D image points. The input must
contain exactly three or four correspondences:

`SolveP3P` 根据匹配的三维物点和二维像点估计相机位姿。输入必须正好包含三对或四对对应点：

- object points: 3D points in the object coordinate system
- image points: corresponding 2D pixel coordinates
- camera matrix: a single-channel `3 x 3` `CV_32F` or `CV_64F` matrix
- distortion coefficients: an empty `Mat`, or a single-channel vector with 4, 5, 8, 12, or 14 values

- 物点：物体坐标系中的三维点
- 像点：与物点对应的二维像素坐标
- 相机矩阵：单通道 `3 x 3`、深度为 `CV_32F` 或 `CV_64F` 的矩阵
- 畸变系数：空 `Mat`，或包含 4、5、8、12、14 个值的单通道向量

Point matrices may use a multi-channel vector layout such as `N x 1 CV_32FC3`, or a
single-channel scalar layout such as `N x 3 CV_64FC1`. Point depths must be `CV_32F` or
`CV_64F`.

点矩阵可以使用 `N x 1 CV_32FC3` 等多通道向量布局，也可以使用 `N x 3 CV_64FC1`
等单通道标量布局。点深度必须为 `CV_32F` 或 `CV_64F`。

## P3P And AP3P / P3P 与 AP3P

Only these flags are accepted:

仅接受以下标志：

| Flag | Description / 说明 |
| --- | --- |
| `SolvePnPFlags.P3P` | Classic perspective-three-point solver / 经典透视三点求解器 |
| `SolvePnPFlags.AP3P` | Algebraic P3P solver / 代数 P3P 求解器 |

Other `SolvePnPFlags` values are rejected before native execution. Use `SolvePnP`,
`SolvePnPGeneric`, or the appropriate refinement API for other pose methods.

其他 `SolvePnPFlags` 会在进入原生层前被拒绝。其他位姿方法应使用 `SolvePnP`、
`SolvePnPGeneric` 或相应的细化 API。

## Solutions And Ordering / 解与排序

P3P can return zero through four pose solutions. The wrapper preserves the upstream ordering:
solutions are sorted by reprojection error from lowest to highest.

P3P 可以返回零到四组位姿解。封装保留上游顺序：解按照重投影误差从低到高排序。

Outputs are packed by row:

输出按行打包：

```text
rvecs: N x 3 CV_64FC1
tvecs: N x 3 CV_64FC1
```

Row `i` in `rvecs` and row `i` in `tvecs` form one pose solution. The wrapper never reorders
solutions, converts them to float, or synthesizes a pose when no solution is found.

`rvecs` 的第 `i` 行和 `tvecs` 的第 `i` 行组成同一组位姿解。封装不会重新排序解、
不会把结果降为 float，也不会在无解时合成位姿。

## Caller-Owned Outputs / 调用方持有输出

Use the caller-owned overload when output matrices already belong to a larger processing pipeline:

当输出矩阵已经属于更大的处理流程时，可以使用调用方持有输出的重载：

```csharp
using JYPPX.OpenCvSharp.Calib3D;
using JYPPX.OpenCvSharp.Core;
using Calib3DCv2 = JYPPX.OpenCvSharp.Calib3D.Cv2;

Point3f[] objectPoints =
{
    new Point3f(-0.6F, -0.5F, 0.2F),
    new Point3f(0.7F, -0.4F, 0.0F),
    new Point3f(0.5F, 0.8F, 0.3F),
    new Point3f(-0.8F, 0.6F, 0.9F)
};

Point2f[] imagePoints =
{
    new Point2f(233.2F, 132.8F),
    new Point2f(453.7F, 147.0F),
    new Point2f(389.4F, 359.0F),
    new Point2f(196.2F, 298.4F)
};

using Mat cameraMatrix = Mat.Eye(3, 3, MatType.CV_64FC1);
cameraMatrix.SetValue(0, 820.0);
cameraMatrix.SetValue(2, 320.0);
cameraMatrix.SetValue(4, 790.0);
cameraMatrix.SetValue(5, 240.0);

using var distCoeffs = new Mat();
using var rvecs = new Mat();
using var tvecs = new Mat();

int solutionCount = Calib3DCv2.SolveP3P(
    objectPoints,
    imagePoints,
    cameraMatrix,
    distCoeffs,
    rvecs,
    tvecs,
    SolvePnPFlags.AP3P);
```

`rvecs` and `tvecs` must be different matrices and must not alias any input matrix.

`rvecs` 与 `tvecs` 必须是不同矩阵，也不能与任何输入矩阵别名。

## Owned Result / Owned 结果

The owned overload returns `SolvePnPGenericResult` because its packed pose representation already
matches P3P:

owned 重载返回 `SolvePnPGenericResult`，因为它现有的位姿打包形式已经与 P3P 匹配：

```csharp
SolvePnPGenericResult result = Calib3DCv2.SolveP3P(
    objectPoints,
    imagePoints,
    cameraMatrix,
    distCoeffs,
    SolvePnPFlags.P3P);

try
{
    int count = result.SolutionCount;
    Mat rotations = result.Rvecs;
    Mat translations = result.Tvecs;
    bool hasErrorMatrix = result.ReprojectionError != null; // false
}
finally
{
    result.Rvecs.Dispose();
    result.Tvecs.Dispose();
}
```

`ReprojectionError` is always `null` for this API. The result structure does not own an automatic
disposal mechanism, so callers must dispose `Rvecs` and `Tvecs`.

此 API 的 `ReprojectionError` 始终为 `null`。结果结构不会自动释放资源，因此调用方必须
释放 `Rvecs` 和 `Tvecs`。

## Arrays And Spans / 数组与 Span

The API provides equivalent entry points for:

API 提供以下等价入口：

- `Mat` object and image points
- `Point3f[]` plus `Point2f[]`
- `ReadOnlySpan<Point3f>` plus `ReadOnlySpan<Point2f>` on supported target frameworks

- `Mat` 物点和像点
- `Point3f[]` 与 `Point2f[]`
- 支持的目标框架上的 `ReadOnlySpan<Point3f>` 与 `ReadOnlySpan<Point2f>`

Array and Span inputs must be non-empty, have matching lengths, and contain exactly three or four
point pairs.

数组和 Span 输入必须非空、长度匹配，并且正好包含三对或四对点。

## Scope / 范围

This API intentionally does not add a second pose-result type, expose `OutputArrayOfArrays`, or
accept arbitrary `SolvePnPFlags`. The native boundary exposes one version-neutral entry point:

此 API 有意不增加第二种位姿结果类型，不暴露 `OutputArrayOfArrays`，也不接受任意
`SolvePnPFlags`。原生边界只增加一个版本中立入口：

```text
jyppx_ocv_calib3d_solve_p3p
```

# Geometry ProjectPoints Separated Jacobians Guide

Round 975 adds caller-owned and owned-result APIs for the six separated `ProjectPoints`
Jacobian blocks while preserving the existing projected-point and combined-Jacobian APIs.

Round 975 增加 `ProjectPoints` 六个分离 Jacobian 块的调用方持有与结果持有 API，同时保留
现有投影点和组合 Jacobian API。

## Inputs / 输入

`objectPoints` accepts these non-empty layouts:

`objectPoints` 接受以下非空布局：

```text
N x 1 or 1 x N, three channels
N x 3 or 3 x N, one channel
```

The depth must be `CV_32F` or `CV_64F`. A square `3 x 3` single-channel matrix follows the
`N x 3` interpretation, matching the existing combined-Jacobian wrapper.

深度必须为 `CV_32F` 或 `CV_64F`。单通道方形 `3 x 3` 矩阵按 `N x 3` 解释，与现有组合
Jacobian 包装保持一致。

`rvec` may be a `3 x 3` rotation matrix, a `1 x 3` or `3 x 1` single-channel vector, or a
`1 x 1` three-channel vector. `tvec` accepts the same three-value vector layouts but not a
rotation matrix. Both must use floating-point depth.

`rvec` 可以是 `3 x 3` 旋转矩阵、`1 x 3` 或 `3 x 1` 单通道向量，或 `1 x 1` 三通道向量。
`tvec` 接受相同的三标量向量布局，但不接受旋转矩阵。两者都必须使用浮点深度。

`cameraMatrix` must be a single-channel `3 x 3` floating-point matrix. `distCoeffs` may be
empty or contain 4, 5, 8, 12, or 14 floating-point values in a row or column vector. An empty
matrix is normalized to five zero coefficients, so `dpdk` has five columns.

`cameraMatrix` 必须是单通道 `3 x 3` 浮点矩阵。`distCoeffs` 可以为空，或以行/列向量包含
4、5、8、12 或 14 个浮点值。空矩阵会规范化为五个零系数，因此 `dpdk` 有五列。

## Outputs / 输出

For `N` object points, `imagePoints` is an `N x 1` two-channel matrix with the same depth as
`objectPoints`. All derivative matrices are single-channel `CV_64F`:

对于 `N` 个物点，`imagePoints` 是与 `objectPoints` 深度相同的 `N x 1` 双通道矩阵。
全部导数矩阵都是单通道 `CV_64F`：

```text
dpdr : 2N x 3    projection derivative with respect to rotation
dpdt : 2N x 3    projection derivative with respect to translation
dpdf : 2N x 2    projection derivative with respect to fx and fy
dpdc : 2N x 2    projection derivative with respect to cx and cy
dpdk : 2N x K    projection derivative with respect to K distortion values
dpdo : 2N x 3N   projection derivative with respect to object coordinates
```

The first five blocks concatenate exactly into the existing combined Jacobian:

前五个块可精确拼接为现有组合 Jacobian：

```text
jacobian = [ dpdr | dpdt | dpdf | dpdc | dpdk ]
```

Rows are flattened as `(u0, v0, u1, v1, ...)`. The `dpdo` columns are flattened as
`(x0, y0, z0, x1, y1, z1, ...)`. Each projected point depends only on the three coordinates
of the matching object point, so all off-point `dpdo` blocks are zero.

行按 `(u0, v0, u1, v1, ...)` 展平。`dpdo` 列按
`(x0, y0, z0, x1, y1, z1, ...)` 展平。每个投影点仅依赖对应物点的三个坐标，因此
`dpdo` 中非对应物点的块为零。

## Caller-Owned Outputs / 调用方持有输出

Use the extended overload when output matrices belong to an existing pipeline:

当输出矩阵属于现有处理流程时，使用扩展重载：

```csharp
using OpenCvSharp.Calib3D;
using OpenCvSharp.Core;
using Calib3DCv2 = OpenCvSharp.Calib3D.Cv2;

using var imagePoints = new Mat();
using var dpdr = new Mat();
using var dpdt = new Mat();
using var dpdf = new Mat();
using var dpdc = new Mat();
using var dpdk = new Mat();
using var dpdo = new Mat();

Calib3DCv2.ProjectPoints(
    objectPoints,
    rvec,
    tvec,
    cameraMatrix,
    distCoeffs,
    imagePoints,
    dpdr,
    dpdt,
    dpdf,
    dpdc,
    dpdk,
    dpdo);
```

No output may alias an input or another output. Native execution may resize and replace existing
output storage.

任何输出都不得与输入或其他输出别名。原生执行可能调整并替换现有输出存储。

## Owned Result / 拥有所有权的结果

`ProjectPointsWithDerivatives` returns a `ProjectPointsDerivativesResult` containing all seven
owned matrices:

`ProjectPointsWithDerivatives` 返回包含全部七个自有矩阵的
`ProjectPointsDerivativesResult`：

```csharp
ProjectPointsDerivativesResult result =
    Calib3DCv2.ProjectPointsWithDerivatives(
        objectPoints,
        rvec,
        tvec,
        cameraMatrix,
        distCoeffs);

using Mat imagePoints = result.ImagePoints;
using Mat dpdr = result.DpDr;
using Mat dpdt = result.DpDt;
using Mat dpdf = result.DpDf;
using Mat dpdc = result.DpDc;
using Mat dpdk = result.DpDk;
using Mat dpdo = result.DpDo;
```

The result is a lightweight value and is not itself disposable. The caller must dispose every
returned `Mat`. If validation or native execution fails before return, the API disposes all seven
temporary outputs.

结果是轻量值，本身不可释放。调用方必须释放每个返回的 `Mat`。如果校验或原生执行在返回前
失败，API 会释放全部七个临时输出。

## Fixed Aspect Ratio / 固定宽高比

`aspectRatio` must be finite. When it is greater than the native floating-point epsilon, the
projection uses `fx = fy * aspectRatio`, and the focal-length derivative block is adjusted for
that constraint. The separated first-five-block concatenation remains identical to the combined
Jacobian produced with the same value.

`aspectRatio` 必须为有限值。当它大于原生浮点 epsilon 时，投影使用
`fx = fy * aspectRatio`，焦距导数块也会按该约束调整。使用相同参数时，分离 API 的前五个
块仍与组合 Jacobian 完全一致。

## Finite-Difference Verification / 有限差分验证

For each scalar parameter `x`, compare the matching analytic column with:

对于每个标量参数 `x`，将对应解析列与以下中心有限差分比较：

```text
(project(x + epsilon) - project(x - epsilon)) / (2 * epsilon)
```

Verify rotation, translation, `fx/fy`, `cx/cy`, every supplied distortion coefficient, and every
object-point coordinate. Use `CV_64F`, nondegenerate positive depths, small perturbations, and
restore each value after evaluating the two projections.

应验证旋转、平移、`fx/fy`、`cx/cy`、每个已提供的畸变系数以及每个物点坐标。建议使用
`CV_64F`、非退化正深度和小扰动，并在两次投影后恢复被扰动值。

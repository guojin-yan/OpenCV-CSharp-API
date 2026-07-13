# Geometry Translation Estimation Guide

Round 964 adds direct managed support for robust pure-translation estimation in three and two
dimensions.

Round 964 增加了三维和二维纯平移稳健估计的直接托管支持。

## Translation Model / 平移模型

Both APIs estimate the translation that maps each source point to its corresponding destination
point:

两个 API 都估计把每个源点映射到对应目标点的平移：

```text
destination = source + translation
```

The estimators model translation only. They do not estimate rotation, scale, affine deformation,
or perspective transformation.

估计器只建模平移，不估计旋转、尺度、仿射形变或透视变换。

## 3D Estimation / 三维估计

`Cv2.EstimateTranslation3D` uses RANSAC and requires at least four matched 3D points. The source
and destination collections must contain the same number of points.

`Cv2.EstimateTranslation3D` 使用 RANSAC，并要求至少四对匹配的三维点。源点和目标点集合的
点数必须相同。

```csharp
using OpenCvSharp.Calib3D;
using OpenCvSharp.Core;
using Calib3DCv2 = OpenCvSharp.Calib3D.Cv2;

Point3f[] source =
{
    new Point3f(0.0F, 0.0F, 0.0F),
    new Point3f(1.0F, 0.0F, 0.0F),
    new Point3f(0.0F, 1.0F, 0.0F),
    new Point3f(0.0F, 0.0F, 1.0F)
};

Point3f[] destination =
{
    new Point3f(2.0F, -3.0F, 4.0F),
    new Point3f(3.0F, -3.0F, 4.0F),
    new Point3f(2.0F, -2.0F, 4.0F),
    new Point3f(2.0F, -3.0F, 5.0F)
};

bool found = Calib3DCv2.EstimateTranslation3D(
    source,
    destination,
    out Mat translation,
    out Mat inliers);

using (translation)
using (inliers)
{
    if (found)
    {
        double x = translation.At<double>(0, 0);
        double y = translation.At<double>(0, 1);
        double z = translation.At<double>(0, 2);
    }
}
```

The linked OpenCV 5.0.0 implementation returns the translation as a
`1 x 3`, single-channel `CV_64F` matrix. This observed runtime shape differs from an upstream
comment that describes a `3 x 1` model. The wrapper preserves actual linked behavior and does not
transpose the result.

链接的 OpenCV 5.0.0 实现把平移返回为 `1 x 3`、单通道 `CV_64F` 矩阵。这个实际运行时形状
与上游注释中描述的 `3 x 1` 模型不同。封装保留真实链接行为，不对结果进行转置。

The return value is `true` when a model is found. A caller-owned overload accepts existing
translation and inlier matrices. Owned-output overloads allocate both matrices and return them
through `out` parameters.

找到模型时返回值为 `true`。caller-owned 重载接受已有的平移矩阵和内点矩阵。owned-output
重载分配两个矩阵，并通过 `out` 参数返回。

## 2D Estimation / 二维估计

`Cv2.EstimateTranslation2D` requires at least one matched 2D point and supports
`RobustEstimationAlgorithms.RANSAC` and `RobustEstimationAlgorithms.LMEDS`.

`Cv2.EstimateTranslation2D` 至少需要一对匹配的二维点，并支持
`RobustEstimationAlgorithms.RANSAC` 和 `RobustEstimationAlgorithms.LMEDS`。

```csharp
Point2f[] source =
{
    new Point2f(0.0F, 0.0F),
    new Point2f(5.0F, 1.0F),
    new Point2f(-2.0F, 4.0F)
};

Point2f[] destination =
{
    new Point2f(1.5F, -2.0F),
    new Point2f(6.5F, -1.0F),
    new Point2f(-0.5F, 2.0F)
};

using Mat inliers = new Mat();
Point2d translation = Calib3DCv2.EstimateTranslation2D(
    source,
    destination,
    inliers,
    RobustEstimationAlgorithms.RANSAC);
```

The result is a double-precision `Point2d`. If upstream estimation fails, the method returns
`Point2d(double.NaN, double.NaN)` rather than throwing merely because no model was found.

结果是双精度 `Point2d`。如果上游估计失败，该方法返回
`Point2d(double.NaN, double.NaN)`，不会仅因为未找到模型而抛出异常。

`maxIters` must be positive, `refineIters` must be non-negative, the threshold must be positive,
and confidence must be strictly between zero and one. Other robust-estimation enum values are
rejected before native execution.

`maxIters` 必须为正数，`refineIters` 必须为非负数，阈值必须为正数，置信度必须严格位于零和
一之间。其他稳健估计算法枚举值会在进入 native 调用前被拒绝。

## Point Matrices / 点矩阵

Mat overloads accept point layouts recognized by OpenCV `Mat::checkVector`:

Mat 重载接受 OpenCV `Mat::checkVector` 识别的点布局：

- `N x 1` or `1 x N` multi-channel point vectors.
- `N x 2` single-channel matrices for 2D points.
- `N x 3` single-channel matrices for 3D points.
- Numeric source depths supported by the wrapper, including integer inputs.

- `N x 1` 或 `1 x N` 的多通道点向量。
- 表示二维点的 `N x 2` 单通道矩阵。
- 表示三维点的 `N x 3` 单通道矩阵。
- 封装支持的数值源深度，包括整数输入。

Source and destination point counts must match. The estimator reads the inputs without modifying
their values, type, shape, or orientation.

源点和目标点的点数必须相同。估计器只读取输入，不修改其值、类型、形状或方向。

## Inlier Masks / 内点掩码

When supplied, the inlier output is an `N x 1`, single-channel `CV_8U` mask. A nonzero entry marks
a correspondence accepted by the robust estimator. Pass `null` when the mask is not needed.

提供内点输出时，结果是 `N x 1`、单通道 `CV_8U` 掩码。非零项表示对应点被稳健估计器接受。
不需要掩码时可传入 `null`。

The mask is caller-owned for every overload that receives a `Mat? inliers` argument. The API does
not dispose a supplied matrix.

对于所有接收 `Mat? inliers` 参数的重载，掩码都由调用方持有。API 不会释放传入的矩阵。

## Arrays And Spans / 数组与 Span

`Point3f[]` and `ReadOnlySpan<Point3f>` overloads are available for 3D estimation.
`Point2f[]` and `ReadOnlySpan<Point2f>` overloads are available for 2D estimation. These overloads
marshal points through temporary Mat instances and dispose those temporary matrices before
returning.

三维估计提供 `Point3f[]` 和 `ReadOnlySpan<Point3f>` 重载。二维估计提供 `Point2f[]` 和
`ReadOnlySpan<Point2f>` 重载。这些重载通过临时 Mat 封送点，并在返回前释放临时矩阵。

Span overloads do not retain references to the supplied memory after the call completes.

Span 重载在调用完成后不会保留对输入内存的引用。

## Ownership And Failure / 所有权与失败处理

Caller-owned overloads write into supplied output matrices and never dispose them. Successful
owned 3D outputs are the caller's responsibility and should be enclosed in `using`.

caller-owned 重载写入调用方提供的输出矩阵，且不会释放它们。成功返回的 owned 三维输出由
调用方负责，应放入 `using` 中。

If native execution throws, the owned 3D overload disposes both allocated output matrices before
rethrowing the exception. Invalid point layouts, mismatched counts, invalid thresholds, invalid
confidence values, and unsupported methods are rejected before native execution.

如果 native 执行抛出异常，owned 三维重载会在重新抛出异常前释放两个已分配的输出矩阵。
无效点布局、点数不一致、无效阈值、无效置信度和不支持的方法会在进入 native 调用前被拒绝。

## Runtime Notes / 运行时说明

The linked implementation calls the OpenCV 5.0.0 Geometry translation-estimation functions.
Project-owned API, ABI, assembly, package, file, and namespace names remain version-neutral.

链接实现调用 OpenCV 5.0.0 Geometry 平移估计函数。项目自有 API、ABI、程序集、包、文件和
命名空间保持版本中立。

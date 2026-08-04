# Geometry Affine Estimation Guide

Round 971 adds the classic Geometry affine-estimation family to
`JYPPX.OpenCvSharp.Calib3D.Cv2`.

Round 971 在 `JYPPX.OpenCvSharp.Calib3D.Cv2` 中增加经典 Geometry 仿射估计 API。

## Transform Families / 变换类型

The four entry-point families solve different models:

四组入口解决不同的变换模型：

| API | Model | Minimum matches | Output |
| --- | --- | ---: | --- |
| `EstimateAffine3D` with transform and inliers | General 3D affine transform using RANSAC | 4 | `3 x 4 CV_64FC1` |
| `EstimateAffine3D` with `out double scale` | 3D similarity transform using Umeyama | 3 | `3 x 4 CV_64FC1` plus scale |
| `EstimateAffine2D` | General 2D affine transform | 3 | `2 x 3 CV_64FC1` |
| `EstimateAffinePartial2D` | Uniform scale, rotation, and translation | 2 | `2 x 3 CV_64FC1` |

The full 2D and 3D affine estimators can represent non-uniform scale and shear. The partial 2D
estimator has four degrees of freedom: one uniform scale, one rotation angle, X translation, and Y
translation.

完整二维和三维仿射估计器可以表示非均匀缩放和剪切。部分二维估计器只有四个自由度：统一
缩放、旋转角、X 平移和 Y 平移。

## Robust 3D Affine / 鲁棒三维仿射

The RANSAC overload writes the transform into a caller-owned `Mat` and optionally writes an inlier
mask. An owned-output overload returns both matrices through `out` parameters.

RANSAC 重载把变换写入调用方持有的 `Mat`，并可选写入内点掩码。owned-output 重载通过
`out` 参数返回两个矩阵。

```csharp
using JYPPX.OpenCvSharp.Calib3D;
using JYPPX.OpenCvSharp.Core;
using Calib3DCv2 = JYPPX.OpenCvSharp.Calib3D.Cv2;

Point3f[] source =
{
    new Point3f(0.0F, 0.0F, 0.0F),
    new Point3f(1.0F, 0.0F, 0.0F),
    new Point3f(0.0F, 1.0F, 0.0F),
    new Point3f(0.0F, 0.0F, 1.0F),
    new Point3f(1.0F, 2.0F, 3.0F)
};

Point3f[] destination =
{
    new Point3f(2.0F, -1.0F, 3.0F),
    new Point3f(3.2F, -1.3F, 3.05F),
    new Point3f(2.1F, -0.1F, 2.75F),
    new Point3f(1.8F, -0.85F, 4.1F),
    new Point3f(2.8F, 0.95F, 5.85F)
};

bool found = Calib3DCv2.EstimateAffine3D(
    source,
    destination,
    out Mat transform,
    out Mat inliers,
    ransacThreshold: 0.1,
    confidence: 0.99);

using (transform)
using (inliers)
{
    if (found)
    {
        double a11 = transform.At<double>(0, 0);
        byte firstMatchIsInlier = inliers.At<byte>(0, 0);
    }
}
```

On success, the transform is:

成功时，变换矩阵为：

```text
[ a11 a12 a13 b1 ]
[ a21 a22 a23 b2 ]
[ a31 a32 a33 b3 ]
```

The return value is `false` when upstream cannot find a model. The wrapper does not replace failure
with an identity or zero transform.

上游无法找到模型时返回 `false`。封装不会用单位矩阵或零矩阵替代失败结果。

## Umeyama 3D Similarity / Umeyama 三维相似变换

The Umeyama overload returns a rotation and translation matrix plus the uniform scale:

Umeyama 重载返回旋转和平移矩阵，并单独返回统一缩放：

```text
destination = scale * R * source + t
```

The `3 x 3` block in the returned `3 x 4` matrix is `R`, not `scale * R`. The fourth column is the
translation `t`.

返回的 `3 x 4` 矩阵中，`3 x 3` 块是 `R`，不是 `scale * R`；第四列是平移 `t`。

```csharp
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
    new Point3f(2.0F, -1.5F, 4.0F),
    new Point3f(0.5F, -3.0F, 4.0F),
    new Point3f(2.0F, -3.0F, 5.5F)
};

using Mat similarity = Calib3DCv2.EstimateAffine3D(
    source,
    destination,
    out double scale,
    forceRotation: true);
```

With `forceRotation: true`, the returned `R` always has rotation orientation and never represents a
reflection. Set it to `false` when mapping between left-handed and right-handed coordinate systems
and a reflection is valid.

`forceRotation: true` 时，返回的 `R` 始终是旋转，不会表示反射。在左右手坐标系之间映射且
允许反射时，可以设为 `false`。

## Full 2D Affine / 完整二维仿射

`EstimateAffine2D` estimates six coefficients:

`EstimateAffine2D` 估计六个系数：

```text
x' = a11 * x + a12 * y + b1
y' = a21 * x + a22 * y + b2
```

```csharp
Point2f[] source =
{
    new Point2f(0.0F, 0.0F),
    new Point2f(1.0F, 0.0F),
    new Point2f(0.0F, 1.0F),
    new Point2f(2.0F, 1.0F)
};

Point2f[] destination =
{
    new Point2f(2.0F, -1.0F),
    new Point2f(3.2F, -1.2F),
    new Point2f(2.3F, -0.1F),
    new Point2f(4.7F, -0.5F)
};

using Mat inliers = new Mat();
using Mat transform = Calib3DCv2.EstimateAffine2D(
    source,
    destination,
    inliers,
    RobustEstimationAlgorithms.RANSAC,
    ransacReprojThreshold: 0.1);
```

Only `RobustEstimationAlgorithms.RANSAC` and `RobustEstimationAlgorithms.LMEDS` are accepted. If
upstream cannot estimate a transform, the method returns an empty owned `Mat`; it never synthesizes
an identity transform.

仅接受 `RobustEstimationAlgorithms.RANSAC` 和 `RobustEstimationAlgorithms.LMEDS`。如果上游
无法估计变换，方法返回空的 owned `Mat`，不会合成单位矩阵。

## Partial 2D Affine / 部分二维仿射

`EstimateAffinePartial2D` returns:

`EstimateAffinePartial2D` 返回：

```text
[  s*cos(theta)  -s*sin(theta)  tx ]
[  s*sin(theta)   s*cos(theta)  ty ]
```

Use the partial estimator when anisotropic scale and shear are not valid for the application. It
requires only two matched points, although additional correspondences improve robust estimation.

当应用不允许非均匀缩放和剪切时，应使用部分仿射估计器。它最少需要两对匹配点，但更多
对应点有助于稳健估计。

## Robust Options / 稳健参数

For the 2D estimators:

对于二维估计器：

- `ransacReprojThreshold` must be positive and finite.
- `confidence` must be strictly between zero and one.
- `maxIters` must be positive.
- `refineIters` must be non-negative.
- `RANSAC` and `LMEDS` are the only supported methods.

- `ransacReprojThreshold` 必须为有限正数。
- `confidence` 必须严格位于零和一之间。
- `maxIters` 必须为正数。
- `refineIters` 必须为非负数。
- 仅支持 `RANSAC` 和 `LMEDS`。

The robust 3D estimator validates a positive finite `ransacThreshold` and confidence strictly
between zero and one.

鲁棒三维估计器验证有限正数 `ransacThreshold`，并要求置信度严格位于零和一之间。

## Inlier Masks / 内点掩码

Robust 3D, full 2D, and partial 2D overloads can write an `N x 1`, single-channel `CV_8UC1` mask.
A nonzero value marks an accepted correspondence. Pass `null` when the mask is not required.

鲁棒三维、完整二维和部分二维重载都可以写入 `N x 1`、单通道 `CV_8UC1` 掩码。非零值
表示对应点被接受。不需要掩码时传入 `null`。

Every `Mat? inliers` argument is caller-owned. The API writes into it but never disposes it. The
owned robust 3D overload allocates both transform and mask and disposes both if native execution
throws before returning.

所有 `Mat? inliers` 参数都由调用方持有。API 会写入但不会释放它。owned 鲁棒三维重载分配
变换和掩码；如果 native 执行在返回前抛出异常，两个矩阵都会被释放。

## Mat, Array, And Span Inputs / Mat、数组与 Span 输入

Mat overloads accept point layouts recognized by OpenCV `Mat::checkVector`, including `N x 1` or
`1 x N` multi-channel vectors and single-channel `N x 2` or `N x 3` matrices. Source and
destination point counts must match.

Mat 重载接受 OpenCV `Mat::checkVector` 识别的点布局，包括 `N x 1` 或 `1 x N` 多通道向量，
以及单通道 `N x 2` 或 `N x 3` 矩阵。源点和目标点数量必须相同。

Managed overloads are available for `Point2f[]`, `Point3f[]`, `ReadOnlySpan<Point2f>`, and
`ReadOnlySpan<Point3f>` where applicable. Temporary point matrices are disposed before the call
returns, and Span memory is not retained.

适用位置提供 `Point2f[]`、`Point3f[]`、`ReadOnlySpan<Point2f>` 和
`ReadOnlySpan<Point3f>` 重载。临时点矩阵会在返回前释放，也不会保留 Span 内存。

Input matrices and output matrices must not alias. The managed boundary rejects source-transform,
destination-transform, source-mask, destination-mask, and transform-mask aliasing before native
execution.

输入矩阵和输出矩阵不得别名。托管边界会在进入 native 前拒绝源点-变换、目标点-变换、
源点-掩码、目标点-掩码以及变换-掩码别名。

## Scope / 范围

This round exposes the classic RANSAC and LMEDS overloads. It does not expose the separate
`estimateAffine2D` overload that accepts C++ `UsacParams`, and it does not add a managed
`UsacParams` type.

本轮只公开经典 RANSAC 和 LMEDS 重载，不公开接收 C++ `UsacParams` 的独立
`estimateAffine2D` 重载，也不增加托管 `UsacParams` 类型。

The linked implementation calls the factual OpenCV 5.0.0 Geometry functions. Project-owned API,
ABI, package, assembly, file, and namespace names remain version-neutral.

链接实现调用事实上的 OpenCV 5.0.0 Geometry 函数。项目自有 API、ABI、包、程序集、文件和
命名空间保持版本中立。

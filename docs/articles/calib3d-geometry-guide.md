# Calib3D Geometry Guide

Local OpenCV 5.0.0 moves many classic `calib3d` geometry APIs into the `geometry` and `stereo` modules while keeping compatibility headers. JYPPX.OpenCvSharp exposes the first managed package under `JYPPX.OpenCvSharp.Calib3D` and keeps the C ABI stable through `jyppx_ocv_calib3d_*` exports.

本地 OpenCV 5.0.0 将许多经典 `calib3d` 几何 API 移到 `geometry` 和 `stereo` 模块，同时保留兼容头文件。JYPPX.OpenCvSharp 将第一批托管接口放在 `JYPPX.OpenCvSharp.Calib3D` 下，并通过 `jyppx_ocv_calib3d_*` 导出保持 C ABI 稳定。

## Covered APIs / 已覆盖接口

- Rotation and projection: `Rodrigues`, `RQDecomp3x3`, `DecomposeProjectionMatrix`, `ComposeRT`, `ProjectPoints`.
- Pose estimation: `SolvePnP`, `SolvePnPRansac`, `SolvePnPGeneric`, `SolvePnPRefineLM`, `SolvePnPRefineVVS`.
- Multi-view geometry: `FindHomography`, `FindFundamentalMat`, `FindEssentialMat`, `DecomposeEssentialMat`, `RecoverPose`, `ComputeCorrespondEpilines`, `TriangulatePoints`.
- Rectification: `UndistortPoints`, `InitUndistortRectifyMap`, `StereoRectify`, `StereoRectifyUncalibrated`.
- Camera utilities: `GetOptimalNewCameraMatrix`, `CalibrationMatrixValues`.
- Helpers: `Point3f`, `ToPointMat(Point2f[])`, `ToPointMat(Point3f[])`, and modern `ReadOnlySpan<T>` overloads on newer target frameworks.

- 旋转与投影：`Rodrigues`、`RQDecomp3x3`、`DecomposeProjectionMatrix`、`ComposeRT`、`ProjectPoints`。
- 位姿估计：`SolvePnP`、`SolvePnPRansac`、`SolvePnPGeneric`、`SolvePnPRefineLM`、`SolvePnPRefineVVS`。
- 多视图几何：`FindHomography`、`FindFundamentalMat`、`FindEssentialMat`、`DecomposeEssentialMat`、`RecoverPose`、`ComputeCorrespondEpilines`、`TriangulatePoints`。
- 校正：`UndistortPoints`、`InitUndistortRectifyMap`、`StereoRectify`、`StereoRectifyUncalibrated`。
- 相机工具：`GetOptimalNewCameraMatrix`、`CalibrationMatrixValues`。
- 辅助类型：`Point3f`、`ToPointMat(Point2f[])`、`ToPointMat(Point3f[])`，以及新目标框架上的 `ReadOnlySpan<T>` 重载。

## Point Projection Ownership / 点投影输出所有权

`ProjectPoints` is available in caller-owned and owned-output forms. The caller-owned form writes an `N x 1` two-channel image-point matrix into `imagePoints` and can also fill the combined OpenCV Jacobian. The owned-output form allocates the image-point matrix and returns it directly for the common projection-only case.

`ProjectPoints` 提供调用方拥有输出和方法拥有输出两种形式。调用方拥有输出的形式会把二通道像点的 `N x 1` 矩阵写入 `imagePoints`，也可以填充 OpenCV 合并雅可比矩阵。方法拥有输出的形式会针对常见的仅投影场景分配像点矩阵并直接返回。

Object points must be non-empty floating-point matrices in `N x 1` or `1 x N` three-channel layout, or `N x 3` / `3 x N` single-channel layout. `rvec` must be a `3 x 3` rotation matrix or a three-value rotation vector, `tvec` must be a three-value vector, and `cameraMatrix` must be a single-channel floating-point `3 x 3` matrix. `distCoeffs` can be empty for zero distortion or contain 4, 5, 8, 12, or 14 floating-point values. The caller-owned outputs are validated not to alias inputs or each other before the native call.

物点必须是非空浮点矩阵，可使用三通道 `N x 1` 或 `1 x N` 布局，也可使用单通道 `N x 3` 或 `3 x N` 布局。`rvec` 必须是 `3 x 3` 旋转矩阵或三值旋转向量，`tvec` 必须是三值向量，`cameraMatrix` 必须是浮点单通道 `3 x 3` 矩阵。`distCoeffs` 可为空以表示零畸变，也可以包含 4、5、8、12 或 14 个浮点值。进入 native 调用前会校验调用方拥有的输出不得与输入或彼此别名相同。

```csharp
using (Mat objectPoints = Calib3DCv2.ToPointMat(new[]
{
    new Point3f(1.0F, 2.0F, 1.0F),
    new Point3f(2.0F, 4.0F, 2.0F)
}))
using (Mat rvec = new Mat(3, 1, MatType.CV_64FC1))
using (Mat tvec = new Mat(3, 1, MatType.CV_64FC1))
using (Mat camera = Mat.Eye(3, 3, MatType.CV_64FC1))
using (var zeroDistortion = new Mat())
{
    using Mat imagePoints = Calib3DCv2.ProjectPoints(
        objectPoints,
        rvec,
        tvec,
        camera,
        zeroDistortion);
    System.Console.WriteLine("Projected points: " + imagePoints.Rows);
}
```

## Corresponding Epilines Ownership / 对应极线输出所有权

`ComputeCorrespondEpilines` is available in caller-owned and owned-output forms. The caller-owned form writes an `N x 1` matrix of three-channel line coefficients into `lines`. The owned-output form allocates that matrix and returns it directly.

`ComputeCorrespondEpilines` 提供调用方拥有输出和方法拥有输出两种形式。调用方拥有输出的形式会把三通道直线系数的 `N x 1` 矩阵写入 `lines`。方法拥有输出的形式会分配该矩阵并直接返回。

`whichImage` must be `1` or `2`. The fundamental matrix must be a single-channel `3 x 3` matrix with `CV_32F` or `CV_64F` depth. Point inputs must be non-empty 2- or 3-component point matrices with `CV_32S`, `CV_32F`, or `CV_64F` depth. The output matrix is validated not to alias `points` or `fundamental` before the native call. Returned line coefficients are `(a, b, c)` for `a*x + b*y + c = 0` and are normalized so `a^2 + b^2 = 1`.

`whichImage` 必须为 `1` 或 `2`。基础矩阵必须是单通道 `3 x 3` 矩阵，深度为 `CV_32F` 或 `CV_64F`。点输入必须是非空的 2 或 3 分量点矩阵，深度为 `CV_32S`、`CV_32F` 或 `CV_64F`。进入 native 调用前会校验输出矩阵不得与 `points` 或 `fundamental` 别名相同。返回的直线系数为 `(a, b, c)`，表示 `a*x + b*y + c = 0`，并按 `a^2 + b^2 = 1` 归一化。

```csharp
using (Mat points = new Mat(2, 1, MatType.CV_32FC2))
using (Mat fundamental = new Mat(3, 3, MatType.CV_64FC1))
{
    points.SetValue(0, new Point2f(10.0F, 20.0F));
    points.SetValue(1, new Point2f(12.0F, 23.0F));
    fundamental.CopyFrom(new[]
    {
        0.0, 0.0, 0.0,
        0.0, 0.0, -1.0,
        0.0, 1.0, 0.0
    });

    using Mat lines = Calib3DCv2.ComputeCorrespondEpilines(points, 1, fundamental);
    Point3f firstLine = lines.GetValue<Point3f>(0);
    System.Console.WriteLine($"Line: {firstLine.X}, {firstLine.Y}, {firstLine.Z}");
}
```

## Triangulation Ownership / 三角化输出所有权

`TriangulatePoints` is available in caller-owned and owned-output forms. The caller-owned form writes a homogeneous `4 x N` point matrix into `points4D`. The owned-output form allocates that matrix and returns it directly.

`TriangulatePoints` 提供调用方拥有输出和方法拥有输出两种形式。调用方拥有输出的形式会把齐次 `4 x N` 点矩阵写入 `points4D`。方法拥有输出的形式会分配该矩阵并直接返回。

Projection matrices must be single-channel `3 x 4` matrices with `CV_32F` or `CV_64F` depth. Image point inputs must contain matching counts and can be row/column vectors of two-channel points or single-channel `2 x N` matrices. The output matrix is validated not to alias any input before the native call.

投影矩阵必须是单通道 `3 x 4` 矩阵，深度为 `CV_32F` 或 `CV_64F`。图像点输入必须数量匹配，可以是二通道点的行/列向量，也可以是单通道 `2 x N` 矩阵。进入 native 调用前会校验输出矩阵不得与任一输入矩阵别名相同。

```csharp
using (Mat projection1 = new Mat(3, 4, MatType.CV_32FC1))
using (Mat projection2 = new Mat(3, 4, MatType.CV_32FC1))
using (Mat points1 = new Mat(2, 3, MatType.CV_32FC1))
using (Mat points2 = new Mat(2, 3, MatType.CV_32FC1))
{
    projection1.CopyFrom(new[]
    {
        1.0F, 0.0F, 0.0F, 0.0F,
        0.0F, 1.0F, 0.0F, 0.0F,
        0.0F, 0.0F, 1.0F, 0.0F
    });
    projection2.CopyFrom(new[]
    {
        1.0F, 0.0F, 0.0F, -1.0F,
        0.0F, 1.0F, 0.0F, 0.0F,
        0.0F, 0.0F, 1.0F, 0.0F
    });

    using Mat points4D = Calib3DCv2.TriangulatePoints(
        projection1,
        projection2,
        points1,
        points2);
    System.Console.WriteLine("Triangulated columns: " + points4D.Cols);
}
```

## Point Undistortion Ownership / 点去畸变输出所有权

`UndistortPoints` is available in caller-owned and owned-output forms. The caller-owned form writes ideal undistorted point coordinates into `dst` and accepts optional `r`, `p`, and `criteria` parameters. The owned-output form allocates the point matrix and returns it directly for the common `src`, `cameraMatrix`, `distCoeffs` case.

`UndistortPoints` 提供调用方拥有输出和方法拥有输出两种形式。调用方拥有输出的形式会把理想无畸变点坐标写入 `dst`，并接受可选的 `r`、`p` 与 `criteria` 参数。方法拥有输出的形式会针对常用的 `src`、`cameraMatrix`、`distCoeffs` 场景分配点矩阵并直接返回。

Input points must be non-empty `CV_32F` or `CV_64F` two-component point matrices: `2 x N` or `N x 2` single-channel matrices, or row/column vectors of two-channel points. `cameraMatrix` must be a floating-point single-channel `3 x 3` matrix. `distCoeffs` can be empty for zero distortion, or a floating-point single-channel vector with 4, 5, 8, 12, or 14 values. Optional `r` can be empty, a `3 x 3` matrix, or a three-value vector. Optional `p` can be empty, `3 x 3`, or `3 x 4`. The output matrix is validated not to alias any input before the native call.

输入点必须是非空的 `CV_32F` 或 `CV_64F` 二维点矩阵：单通道 `2 x N` 或 `N x 2` 矩阵，或二通道点的行/列向量。`cameraMatrix` 必须是浮点单通道 `3 x 3` 矩阵。`distCoeffs` 可为空以表示零畸变，也可以是包含 4、5、8、12 或 14 个值的浮点单通道向量。可选 `r` 可以为空、`3 x 3` 矩阵或三值向量。可选 `p` 可以为空、`3 x 3` 或 `3 x 4`。进入 native 调用前会校验输出矩阵不得与任一输入矩阵别名相同。

Without `p`, output points are normalized camera coordinates. Supplying `p` projects the undistorted points into that camera or projection matrix. Use `UndistortImagePoints` when you specifically want pixel coordinates under the original camera matrix.

未提供 `p` 时，输出点为归一化相机坐标。提供 `p` 后，无畸变点会投影到该相机或投影矩阵下。如果明确需要原相机矩阵下的像素坐标，请使用 `UndistortImagePoints`。

```csharp
using (Mat points = Calib3DCv2.ToPointMat(new[]
{
    new Point2f(320.0F, 240.0F),
    new Point2f(940.0F, 855.0F)
}))
using (Mat camera = new Mat(3, 3, MatType.CV_64FC1))
using (var zeroDistortion = new Mat())
{
    camera.CopyFrom(new[]
    {
        620.0, 0.0, 320.0,
        0.0, 615.0, 240.0,
        0.0, 0.0, 1.0
    });

    using Mat normalized = Calib3DCv2.UndistortPoints(points, camera, zeroDistortion);
    using var pixels = new Mat();
    Calib3DCv2.UndistortPoints(points, pixels, camera, zeroDistortion, p: camera);
    System.Console.WriteLine("Normalized points: " + normalized.Rows);
    System.Console.WriteLine("Pixel-space points: " + pixels.Rows);
}
```

## Rectification Map Ownership / 校正映射所有权

`InitUndistortRectifyMap` is available in two ownership forms:

- Caller-owned: pass `map1` and `map2` to be populated by the method.
- Owned result: call the overload without output maps and receive an `UndistortRectifyMapResult` containing `Map1` and `Map2`.

`InitUndistortRectifyMap` 提供两种所有权形式：

- 调用方拥有输出：传入 `map1` 和 `map2`，由方法填充。
- 结果对象拥有输出：调用不带输出映射参数的重载，并接收包含 `Map1` 和 `Map2` 的 `UndistortRectifyMapResult`。

The map type must be one of `MatType.CV_16SC2`, `MatType.CV_32FC1`, or `MatType.CV_32FC2`. `cameraMatrix`, `distCoeffs`, `r`, `newCameraMatrix`, `size`, and output maps are validated before the native call. The caller-owned form requires two distinct output `Mat` instances; aliasing `map1` and `map2` is rejected so the native call cannot overwrite one output through the other.

映射类型必须是 `MatType.CV_16SC2`、`MatType.CV_32FC1` 或 `MatType.CV_32FC2`。`cameraMatrix`、`distCoeffs`、`r`、`newCameraMatrix`、`size` 和输出映射会在进入 native 调用前完成校验。调用方拥有输出的形式要求 `map1` 与 `map2` 是两个不同的 `Mat` 实例；如果二者别名相同会被拒绝，避免 native 调用通过一个输出覆盖另一个输出。

```csharp
using (Mat cameraMatrix = Mat.Eye(3, 3, MatType.CV_64FC1))
using (Mat distCoeffs = new Mat(1, 5, MatType.CV_64FC1))
using (Mat r = Mat.Eye(3, 3, MatType.CV_64FC1))
using (Mat newCameraMatrix = cameraMatrix.Clone())
{
    distCoeffs.SetTo(new Scalar(0));

    UndistortRectifyMapResult maps = Calib3DCv2.InitUndistortRectifyMap(
        cameraMatrix,
        distCoeffs,
        r,
        newCameraMatrix,
        new Size(640, 480),
        MatType.CV_32FC1);

    using (maps.Map1)
    using (maps.Map2)
    {
        System.Console.WriteLine(maps);
    }
}
```

## Example / 示例

```csharp
using JYPPX.OpenCvSharp.Calib3D;
using JYPPX.OpenCvSharp.Core;
using Calib3DCv2 = JYPPX.OpenCvSharp.Calib3D.Cv2;

internal static class Program
{
    private static void Main()
    {
        Point3f[] objectPoints =
        {
            new Point3f(-1, -1, 0),
            new Point3f(1, -1, 0),
            new Point3f(1, 1, 0),
            new Point3f(-1, 1, 0)
        };

        Point2f[] imagePoints =
        {
            new Point2f(100, 100),
            new Point2f(200, 100),
            new Point2f(200, 200),
            new Point2f(100, 200)
        };

        using (Mat cameraMatrix = Mat.Eye(3, 3, MatType.CV_64FC1))
        using (Mat distCoeffs = new Mat(1, 5, MatType.CV_64FC1))
        using (Mat rvec = new Mat())
        using (Mat tvec = new Mat())
        {
            cameraMatrix.SetValue(0, 100.0);
            cameraMatrix.SetValue(2, 150.0);
            cameraMatrix.SetValue(4, 100.0);
            cameraMatrix.SetValue(5, 150.0);
            distCoeffs.SetTo(new Scalar(0));

            bool solved = Calib3DCv2.SolvePnP(
                objectPoints,
                imagePoints,
                cameraMatrix,
                distCoeffs,
                rvec,
                tvec,
                flags: SolvePnPFlags.IPPE);

            using (Mat projected = Calib3DCv2.ProjectPoints(objectPoints, rvec, tvec, cameraMatrix, distCoeffs))
            {
                System.Console.WriteLine("Solved: " + solved + ", projected rows: " + projected.Rows);
            }
        }
    }
}
```

## Runtime Notes / 运行时说明

The runtime package must include `opencv_geometry500`, `opencv_stereo500`, `opencv_objdetect500`, and `opencv_imgproc500` in addition to `opencv_core500`. Packaging automation stages these as required modules for Calib3D.

runtime 包除 `opencv_core500` 外，还必须包含 `opencv_geometry500`、`opencv_stereo500`、`opencv_objdetect500` 和 `opencv_imgproc500`。打包自动化已将这些模块作为 Calib3D 所需模块暂存。

On `netcoreapp3.1` and newer targets, point-array helpers use span-based memory reinterpretation to reduce temporary copies before filling `Mat`. Older .NET Framework targets keep the same public API and use a compatible array flattening fallback.

在 `netcoreapp3.1` 及更新目标框架上，点数组辅助方法使用基于 Span 的内存重解释，减少填充 `Mat` 前的临时拷贝。旧 .NET Framework 目标框架保持相同公共 API，并使用兼容的数组展平 fallback。

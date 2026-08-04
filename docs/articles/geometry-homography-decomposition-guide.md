# Geometry Homography Decomposition Guide

Round 965 adds managed support for planar homography decomposition and positive-depth solution
filtering.

Round 965 增加了平面单应矩阵分解和基于正深度约束的解筛选支持。

## Homography Model / 单应模型

`Cv2.DecomposeHomographyMat` decomposes a calibrated planar homography into one to four motion
solutions. Each solution contains:

`Cv2.DecomposeHomographyMat` 把经过相机内参标定的平面单应矩阵分解为一到四组运动解。每组解
包含：

- A `3 x 3 CV_64F` rotation matrix.
- A `3 x 1 CV_64F` normalized translation vector.
- A `3 x 1 CV_64F` plane-normal vector.

- 一个 `3 x 3 CV_64F` 旋转矩阵。
- 一个 `3 x 1 CV_64F` 归一化平移向量。
- 一个 `3 x 1 CV_64F` 平面法向量。

Both the homography and camera matrix must be single-channel `3 x 3` matrices with `CV_32F` or
`CV_64F` depth. The camera matrix supplies the focal lengths and principal point used to normalize
the homography.

单应矩阵和相机矩阵都必须是单通道 `3 x 3` 矩阵，深度为 `CV_32F` 或 `CV_64F`。相机矩阵
提供焦距和主点，用于对单应矩阵进行归一化。

## Multiple Solutions / 多组解

A planar homography does not generally identify one unique camera motion. The returned arrays
preserve OpenCV's exact solution order and may contain sign-opposite translation and normal pairs.

平面单应矩阵通常不能唯一确定一组相机运动。返回数组保留 OpenCV 的精确解顺序，并可能包含
平移和法向同时反号的成对解。

The translation vector is normalized by the unknown scene depth. Its direction is meaningful, but
its absolute magnitude is not a metric distance unless additional scene scale is known.

平移向量会按未知场景深度进行归一化。其方向有意义，但在没有额外场景尺度时，其绝对长度不是
真实距离。

```csharp
using JYPPX.OpenCvSharp.Calib3D;
using JYPPX.OpenCvSharp.Core;
using Calib3DCv2 = JYPPX.OpenCvSharp.Calib3D.Cv2;

using Mat homography = GetHomography();
using Mat cameraMatrix = GetCameraMatrix();

int solutionCount = Calib3DCv2.DecomposeHomographyMat(
    homography,
    cameraMatrix,
    out Mat[] rotations,
    out Mat[] translations,
    out Mat[] normals);

try
{
    for (int i = 0; i < solutionCount; ++i)
    {
        Mat rotation = rotations[i];
        Mat normalizedTranslation = translations[i];
        Mat planeNormal = normals[i];
    }
}
finally
{
    DisposeAll(rotations);
    DisposeAll(translations);
    DisposeAll(normals);
}
```

The owned-output overload returns arrays whose lengths exactly equal the returned solution count.

owned-output 重载返回的三个数组长度都与返回的解数量完全相同。

## Caller-Owned Outputs / 调用方持有输出

The caller-owned decomposition overload requires three arrays with capacity for four Mats. Every
entry must be non-null, undisposed, and distinct from every other output entry. OpenCV writes only
the first `solutionCount` entries. Remaining entries stay owned and unchanged in ownership by the
caller.

caller-owned 分解重载要求三个数组都至少容纳四个 Mat。每个元素都必须非空、未释放，并且不能
与任何其他输出元素别名。OpenCV 只写入前 `solutionCount` 个元素。其余元素继续由调用方持有，
API 不会改变其所有权。

The API never disposes caller-owned output Mats. The owned overload allocates four temporary Mats
per output family, trims successful results to exact-length arrays, disposes unused Mats, and
disposes every temporary Mat if decomposition or trimming fails.

API 不会释放 caller-owned 输出 Mat。owned 重载会为每组输出分配四个临时 Mat，成功后裁剪为
精确长度数组并释放未使用的 Mat；如果分解或裁剪失败，则释放所有临时 Mat。

## Positive-Depth Filtering / 正深度筛选

`Cv2.FilterHomographyDecompByVisibleRefpoints` removes solutions that place any selected reference
point behind either camera. It receives matching rotation and normal arrays and returns indices
into those original arrays.

`Cv2.FilterHomographyDecompByVisibleRefpoints` 会移除使任一选定参考点位于任一相机后方的解。
它接收数量匹配的旋转和法向数组，并返回这些原始数组中的索引。

The before and after point matrices must be `CV_32FC2` row or column vectors with equal, nonzero
point counts. Inputs are rectified normalized image points, not raw distorted pixel coordinates.

前后点矩阵必须是 `CV_32FC2` 行向量或列向量，点数相等且非零。输入应是经过校正的归一化图像
点，而不是带畸变的原始像素坐标。

```csharp
using Mat beforePoints = Calib3DCv2.ToPointMat(before);
using Mat afterPoints = Calib3DCv2.ToPointMat(after);
using Mat possibleSolutions =
    Calib3DCv2.FilterHomographyDecompByVisibleRefpoints(
        rotations,
        normals,
        beforePoints,
        afterPoints);

int[] indices = possibleSolutions.Empty
    ? Array.Empty<int>()
    : possibleSolutions.ToArray<int>();
```

The result is a single-channel `CV_32S` vector. Its values preserve OpenCV's exact ordering. An
empty valid result is represented by an empty Mat.

结果是单通道 `CV_32S` 向量，其值保留 OpenCV 的精确顺序。没有可行解时，结果表示为空 Mat。

## Point Masks / 点掩码

An optional single-channel `CV_8U` or `CV_8S` row or column vector can select which correspondences
participate in filtering. Its element count must equal the point count. Zero entries are ignored;
nonzero entries participate in every solution's positive-depth check.

可选的单通道 `CV_8U` 或 `CV_8S` 行向量或列向量可选择参与筛选的对应点。掩码元素数量必须与
点数相同。零元素会被忽略，非零元素会参与每组解的正深度检查。

```csharp
using Mat mask = CreateVisibilityMask();
using Mat possibleSolutions =
    Calib3DCv2.FilterHomographyDecompByVisibleRefpoints(
        rotations,
        normals,
        beforePoints,
        afterPoints,
        mask);
```

The rotations, normals, point matrices, and mask are read-only inputs and remain unchanged.

旋转、法向、点矩阵和掩码都是只读输入，调用后保持不变。

## Caller-Owned Filter Result / 调用方持有筛选结果

The caller-owned filtering overload writes into a supplied Mat and does not dispose it. Because the
owned overload uses its fifth Mat argument as the optional point mask, use the
`possibleSolutions:` named argument when calling the caller-owned overload without a mask:

caller-owned 筛选重载写入调用方提供的 Mat，且不会释放它。由于 owned 重载把第五个 Mat 参数
用作可选点掩码，不带掩码调用 caller-owned 重载时应使用 `possibleSolutions:` 命名参数：

```csharp
using Mat possibleSolutions = new Mat();

Calib3DCv2.FilterHomographyDecompByVisibleRefpoints(
    rotations,
    normals,
    beforePoints,
    afterPoints,
    possibleSolutions: possibleSolutions);
```

When a mask is supplied, pass both output and mask explicitly:

提供掩码时，应同时明确传入输出和掩码：

```csharp
Calib3DCv2.FilterHomographyDecompByVisibleRefpoints(
    rotations,
    normals,
    beforePoints,
    afterPoints,
    possibleSolutions,
    mask);
```

## Arrays And Spans / 数组与 Span

`Point2f[]` and `ReadOnlySpan<Point2f>` overloads create temporary `CV_32FC2` point Mats, call the
same native filter, and dispose both temporary Mats before returning. They require nonempty,
equal-length before and after collections.

`Point2f[]` 和 `ReadOnlySpan<Point2f>` 重载会创建临时 `CV_32FC2` 点 Mat，调用同一个 native
筛选器，并在返回前释放两个临时 Mat。前后集合必须非空且长度相同。

The Span overload does not retain references to the supplied memory after the call completes.

Span 重载在调用完成后不会保留对输入内存的引用。

## Validation And Ownership / 验证与所有权

Filtering accepts one to four rotation/normal pairs. Rotations must be numeric, single-channel
`3 x 3` matrices. Normals must be numeric, single-channel matrices containing exactly three
elements. Counts must match.

筛选接受一到四组旋转/法向。旋转必须是数值型、单通道 `3 x 3` 矩阵。法向必须是数值型、
单通道且恰好包含三个元素的矩阵。两组数组数量必须相同。

Owned filtering results are the caller's responsibility and should be enclosed in `using`. If the
native call fails, the owned overload disposes its allocated result before rethrowing.

owned 筛选结果由调用方负责，应放入 `using` 中。如果 native 调用失败，owned 重载会先释放已
分配的结果再重新抛出异常。

## Runtime Notes / 运行时说明

The linked implementation calls the OpenCV 5.0.0 Geometry homography-decomposition functions.
Project-owned API, ABI, assembly, package, file, directory, and namespace names remain
version-neutral.

链接实现调用 OpenCV 5.0.0 Geometry 单应分解函数。项目自有 API、ABI、程序集、包、文件、
目录和命名空间保持版本中立。

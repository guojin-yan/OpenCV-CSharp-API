# Calib3D Fisheye Calibration Guide

Round 961 adds direct managed support for single-camera and stereo fisheye calibration through
`Cv2.FisheyeCalibrate`, `Cv2.FisheyeStereoCalibrate`, and
`Cv2.FisheyeStereoCalibrateExtended`.

Round 961 增加了鱼眼单目和双目标定的直接托管支持，包括
`Cv2.FisheyeCalibrate`、`Cv2.FisheyeStereoCalibrate` 和
`Cv2.FisheyeStereoCalibrateExtended`。

## Fisheye Versus Pinhole / 鱼眼与针孔模型

The fisheye model is intended for lenses with a very wide field of view. It models the distorted
angle with four coefficients, `D = [k1, k2, k3, k4]`, instead of the radial and tangential
coefficient layout used by the normal pinhole calibration API.

鱼眼模型适用于超广角镜头。它使用四个系数 `D = [k1, k2, k3, k4]` 对视线角度进行畸变建模，
而不是使用普通针孔标定 API 的径向与切向畸变系数布局。

Use `FisheyeCalibrate` and `FisheyeStereoCalibrate` only when observations were produced by the
fisheye projection model. Do not pass a pinhole distortion vector to these methods.

只有观测数据符合鱼眼投影模型时才应使用 `FisheyeCalibrate` 和 `FisheyeStereoCalibrate`。
不要向这些方法传入针孔模型的畸变向量。

## Input Shape / 输入形状

Single-camera input uses one group per calibration view:

单目标定输入按标定视图分组：

```csharp
Point3f[][] objectPoints; // [view][point]
Point2f[][] imagePoints;  // [view][point]
Size imageSize;
```

Stereo calibration uses matching object, left-image, and right-image groups:

双目标定使用相互匹配的物点、左图像点和右图像点组：

```csharp
Point3f[][] objectPoints;  // [view][point]
Point2f[][] imagePoints1;  // [view][point]
Point2f[][] imagePoints2;  // [view][point]
Size imageSize;
```

Every top-level array and every nested group must be non-null and non-empty. Corresponding groups
must contain the same number of points. Different views may contain different point counts, and
the image width and height must both be positive.

每个顶层数组和嵌套点组都必须非空。对应视图的点数必须一致；不同视图之间允许使用不同点数。
图像宽度和高度都必须为正数。

## Single-Camera Results / 单目结果

The caller-owned overload writes:

caller-owned 重载写入：

- `cameraMatrix`: `3 x 3`.
- `distCoeffs`: exactly four values, either `4 x 1` or `1 x 4`.
- `rvecs`: `viewCount x 3`, type `CV_64F`.
- `tvecs`: `viewCount x 3`, type `CV_64F`.
- The return value is the RMS reprojection error.

- `cameraMatrix`：`3 x 3`。
- `distCoeffs`：恰好四个值，可为 `4 x 1` 或 `1 x 4`。
- `rvecs`：`viewCount x 3`，类型为 `CV_64F`。
- `tvecs`：`viewCount x 3`，类型为 `CV_64F`。
- 返回值为 RMS 重投影误差。

The owned overload returns the same outputs in `CalibrationResult`.

owned 重载通过 `CalibrationResult` 返回相同输出。

## Stereo Results / 双目结果

`FisheyeStereoCalibrate` returns or writes:

`FisheyeStereoCalibrate` 返回或写入：

- Two `3 x 3` camera matrices.
- Two four-coefficient fisheye distortion vectors.
- `R`: a `3 x 3` rotation from camera 1 to camera 2.
- `T`: a `3 x 1` translation from camera 1 to camera 2.
- The RMS reprojection error.

- 两个 `3 x 3` 相机矩阵。
- 两个四系数鱼眼畸变向量。
- `R`：从相机 1 到相机 2 的 `3 x 3` 旋转矩阵。
- `T`：从相机 1 到相机 2 的 `3 x 1` 平移向量。
- RMS 重投影误差。

`FisheyeStereoCalibrateExtended` additionally writes `rvecs` and `tvecs` as
`viewCount x 3 CV_64F`. These are the calibration-board poses in the coordinate system of camera
1, not extra camera-to-camera transforms. Compact and extended calls use the same intrinsic and
stereo-extrinsic model.

`FisheyeStereoCalibrateExtended` 还会写入 `viewCount x 3 CV_64F` 的 `rvecs` 与 `tvecs`。
它们表示第一台相机坐标系中的标定板位姿，并不是额外的相机间变换。紧凑版和扩展版使用相同的
内参与双目外参模型。

## Flags / 标定标志

Supported single-camera flags are:

单目标定支持：

- `UseIntrinsicGuess`
- `FixPrincipalPoint`
- `FixFocalLength`
- `FixK1`, `FixK2`, `FixK3`, `FixK4`
- `RecomputeExtrinsic`
- `CheckCond`
- `FixSkew`

Stereo calibration supports the same flags plus `FixIntrinsic`. Pinhole-only and unrelated flags
are rejected before native execution.

双目标定支持上述标志以及 `FixIntrinsic`。针孔模型专用和其他无关标志会在进入 native 层前被拒绝。

Important fisheye behavior: `FixK1` through `FixK4` set the corresponding coefficient to zero and
keep it fixed. They do not preserve a nonzero value supplied by the caller. To use nonzero
distortion values as an initial estimate, use `UseIntrinsicGuess` without the corresponding
`FixK` flags.

重要的鱼眼语义：`FixK1` 至 `FixK4` 会把对应系数设为零并固定，并不会保留调用方提供的非零值。
如果要把非零畸变系数作为初值，请使用 `UseIntrinsicGuess`，且不要添加对应的 `FixK` 标志。

The default termination criteria are `CountOrEps`, 100 iterations, and `DBL_EPSILON`.

默认终止条件为 `CountOrEps`、100 次迭代和 `DBL_EPSILON`。

## Intrinsic Modes / 内参模式

`UseIntrinsicGuess` treats `K` and `D` as input-output values. Use a caller-owned overload and
provide a valid `3 x 3` camera matrix plus a four-value distortion matrix. The owned single-camera
overload rejects this flag because it has no initial matrices to consume.

`UseIntrinsicGuess` 会把 `K` 和 `D` 作为输入输出值。此时应使用 caller-owned 重载，并提供有效的
`3 x 3` 相机矩阵与四元素畸变矩阵。owned 单目重载没有可供读取的初始矩阵，因此会拒绝此标志。

Caller-owned stereo overloads default to `FixIntrinsic`, matching OpenCV. Both cameras must then
receive valid initial intrinsics. Owned stereo overloads default to joint calibration with
`CalibrationFlags.None`; because they allocate empty matrices, they reject both `UseIntrinsicGuess`
and `FixIntrinsic`.

caller-owned 双目重载默认使用 `FixIntrinsic`，与 OpenCV 一致，此时两台相机都必须提供有效初始内参。
owned 双目重载默认以 `CalibrationFlags.None` 执行联合标定；由于它们分配空矩阵，因此会拒绝
`UseIntrinsicGuess` 和 `FixIntrinsic`。

## Caller-Owned Example / Caller-Owned 示例

```csharp
using JYPPX.OpenCvSharp.Calib3D;
using JYPPX.OpenCvSharp.Core;
using Calib3DCv2 = JYPPX.OpenCvSharp.Calib3D.Cv2;

using var cameraMatrix = Mat.Eye(3, 3, MatType.CV_64FC1);
using var distCoeffs = Mat.Zeros(4, 1, MatType.CV_64FC1);
using var rvecs = new Mat();
using var tvecs = new Mat();

double rms = Calib3DCv2.FisheyeCalibrate(
    objectPoints,
    imagePoints,
    imageSize,
    cameraMatrix,
    distCoeffs,
    rvecs,
    tvecs,
    CalibrationFlags.UseIntrinsicGuess |
    CalibrationFlags.RecomputeExtrinsic |
    CalibrationFlags.FixSkew);

Console.WriteLine(rms);
Console.WriteLine($"{rvecs.Rows}x{rvecs.Cols}");
```

With deterministic input point arrays, the overload performs no hidden randomization; repeated
calls with the same OpenCV build and inputs use the same data and settings.

当输入点数组固定时，该重载不会引入隐藏随机过程；在相同 OpenCV 构建和输入下，重复调用使用完全
相同的数据与设置。

## Owned Stereo Example / Owned 双目示例

```csharp
FisheyeStereoCalibrationExtendedResult result =
    Calib3DCv2.FisheyeStereoCalibrateExtended(
        objectPoints,
        imagePoints1,
        imagePoints2,
        imageSize,
        CalibrationFlags.RecomputeExtrinsic |
        CalibrationFlags.CheckCond |
        CalibrationFlags.FixSkew);

try
{
    Console.WriteLine(result.Calibration.ReprojectionError);
    Console.WriteLine($"{result.Calibration.R.Rows}x{result.Calibration.R.Cols}");
    Console.WriteLine($"{result.Calibration.T.Rows}x{result.Calibration.T.Cols}");
    Console.WriteLine(result.ViewCount);
}
finally
{
    result.Calibration.CameraMatrix1.Dispose();
    result.Calibration.DistCoeffs1.Dispose();
    result.Calibration.CameraMatrix2.Dispose();
    result.Calibration.DistCoeffs2.Dispose();
    result.Calibration.R.Dispose();
    result.Calibration.T.Dispose();
    result.Rvecs.Dispose();
    result.Tvecs.Dispose();
}
```

## Ownership / 所有权

Caller-owned overloads write into supplied `Mat` objects and never dispose them. Owned-result
overloads allocate every returned matrix, transfer ownership to the result, and clean up all
partial allocations if calibration fails. The caller must dispose every matrix in a successful
owned result.

caller-owned 重载写入调用方提供的 `Mat`，且不会释放它们。owned-result 重载会分配所有返回矩阵，
将所有权交给结果对象，并在标定失败时清理全部中间分配。调用方必须释放成功结果中的每个矩阵。

## Runtime Notes / 运行时说明

The linked implementation uses the OpenCV 5.0.0 calibration module. Project-owned API, ABI,
assembly, package, file, and namespace names remain version-neutral; the OpenCV major version is
represented by package versions plus explicitly preserved loader/build-info compatibility names for existing consumers.

链接实现使用 OpenCV 5.0.0 calibration 模块。项目自有的 API、ABI、程序集、包、文件和命名空间名称
保持版本中立；OpenCV 主版本通过包版本和面向既有消费者明确保留的 loader/build-info 兼容名称表达。

# Calib3D Stereo Calibration Guide

Round 55 adds full stereo calibration entry points next to the existing stereo rectification and `StereoBM` APIs. Round 959 adds camera-pair registration for cameras whose intrinsics are already calibrated. The wrappers accept grouped point arrays, write extrinsic matrices into `Mat` outputs, and return the RMS reprojection error.

Round 55 在已有双目校正和 `StereoBM` API 旁补齐了完整双目标定入口。Round 959 增加了针对已完成内参标定相机的相机对注册。封装接收分组点数组，将外参矩阵写入 `Mat` 输出，并返回 RMS 重投影误差。

## APIs / API

- `Cv2.StereoCalibrate(...)`
- `Cv2.StereoCalibrateExtended(...)`
- `Cv2.RegisterCameras(...)`
- `Cv2.RegisterCamerasExtended(...)`
- `Cv2.StereoRectify(...)`
- `Cv2.StereoRectifyUncalibrated(...)`
- `Cv2.Rectify3Collinear(...)`
- `CameraModel`
- `CameraRegistrationResult`
- `CameraRegistrationExtendedResult`
- `StereoCalibrationResult`
- `StereoCalibrationExtendedResult`
- `StereoRectifyResult`
- `StereoRectifyUncalibratedResult`
- `Rectify3CollinearResult`

- `Cv2.StereoCalibrate(...)`
- `Cv2.StereoCalibrateExtended(...)`
- `Cv2.RegisterCameras(...)`
- `Cv2.RegisterCamerasExtended(...)`
- `Cv2.StereoRectify(...)`
- `Cv2.StereoRectifyUncalibrated(...)`
- `Cv2.Rectify3Collinear(...)`
- `CameraModel`
- `CameraRegistrationResult`
- `CameraRegistrationExtendedResult`
- `StereoCalibrationResult`
- `StereoCalibrationExtendedResult`
- `StereoRectifyResult`
- `StereoRectifyUncalibratedResult`
- `Rectify3CollinearResult`

The default stereo flags follow local OpenCV 5.0.0: `CalibrationFlags.FixIntrinsic` with `TermCriteria.CountOrEps, 100, 1e-6`.

默认双目标定参数跟随本地 OpenCV 5.0.0：`CalibrationFlags.FixIntrinsic`，终止条件为 `TermCriteria.CountOrEps, 100, 1e-6`。

## Example / 示例

```csharp
using System;
using OpenCvSharp.Calib3D;
using OpenCvSharp.Core;
using Calib3DCv2 = OpenCvSharp.Calib3D.Cv2;

namespace StereoCalibrationSample
{
    internal static class Program
    {
        private static void Main()
        {
            Point3f[][] objectPoints =
            {
                new[] { new Point3f(0, 0, 0), new Point3f(1, 0, 0), new Point3f(0, 1, 0), new Point3f(1, 1, 0) }
            };
            Point2f[][] left =
            {
                new[] { new Point2f(100, 100), new Point2f(200, 100), new Point2f(100, 200), new Point2f(200, 200) }
            };
            Point2f[][] right =
            {
                new[] { new Point2f(96, 100), new Point2f(196, 100), new Point2f(96, 200), new Point2f(196, 200) }
            };

            StereoCalibrationResult result = Calib3DCv2.StereoCalibrate(objectPoints, left, right, new Size(640, 480));
            try
            {
                Console.WriteLine(result.ReprojectionError);
                Console.WriteLine(result.R.Rows + "x" + result.R.Cols);
            }
            finally
            {
                result.CameraMatrix1.Dispose();
                result.DistCoeffs1.Dispose();
                result.CameraMatrix2.Dispose();
                result.DistCoeffs2.Dispose();
                result.R.Dispose();
                result.T.Dispose();
                result.E.Dispose();
                result.F.Dispose();
            }
        }
    }
}
```

## Camera Registration / 相机注册

Use `StereoCalibrate` when the calibration process must estimate or refine the two camera matrices and distortion coefficients together. Use `RegisterCameras` when both cameras are already calibrated and only the relative camera transform must be estimated. Registration treats the supplied camera matrices and distortion coefficients as read-only inputs and does not include them in the owned result.

当标定过程需要联合估计或细化两台相机的相机矩阵与畸变系数时，应使用 `StereoCalibrate`。当两台相机已经分别完成标定、只需估计相机间相对变换时，应使用 `RegisterCameras`。注册 API 将传入的相机矩阵和畸变系数视为只读输入，因此 owned result 不会拥有这些内参矩阵。

Each camera receives its own `Point3f[][]` object-point groups and `Point2f[][]` image-point groups. Both cameras must provide the same number of frames, and each camera's object/image point counts must match within a frame. Camera 1 and camera 2 may use different point counts in the same frame. Because each observation uses absolute target coordinates, the cameras can be registered even when their fields of view do not overlap.

每台相机分别接收自己的 `Point3f[][]` 物点组和 `Point2f[][]` 像点组。两台相机必须提供相同帧数，并且每台相机在每帧内的物点数与像点数必须一致；但相机 1 与相机 2 在同一帧中可以使用不同数量的点。由于每组观测使用绝对标定目标坐标，即使两台相机的视场不重叠，也可以完成注册。

Select `CameraModel.Pinhole` or `CameraModel.Fisheye` independently for each camera. Pinhole-pinhole, fisheye-fisheye, and mixed pinhole-fisheye pairs are supported. The compact result contains `R` (`3 x 3`), `T` (`3 x 1`), `E` (`3 x 3`), `F` (`3 x 3`), and `PerViewErrors` (`N x 2`). The extended result additionally contains packed `Rvecs` and `Tvecs` matrices with shape `N x 3`.

可为两台相机分别选择 `CameraModel.Pinhole` 或 `CameraModel.Fisheye`。支持针孔-针孔、鱼眼-鱼眼以及针孔-鱼眼混合组合。基础结果包含 `R`（`3 x 3`）、`T`（`3 x 1`）、`E`（`3 x 3`）、`F`（`3 x 3`）和 `PerViewErrors`（`N x 2`）；扩展结果还包含打包为 `N x 3` 的 `Rvecs` 与 `Tvecs`。

`CalibrationFlags.UseExtrinsicGuess` makes `R` and `T` input-output matrices. Use the caller-owned overload when supplying this flag so the initial matrices can be provided. Owned-result overloads reject the flag because they allocate empty `R` and `T` outputs.

设置 `CalibrationFlags.UseExtrinsicGuess` 后，`R` 与 `T` 会成为输入输出矩阵。需要使用该标志时，请调用 caller-owned 重载并提供初始矩阵。owned-result 重载会分配空的 `R` 与 `T`，因此会明确拒绝该标志。

```csharp
CameraRegistrationResult registration = Calib3DCv2.RegisterCameras(
    objectPoints1,
    objectPoints2,
    imagePoints1,
    imagePoints2,
    cameraMatrix1,
    distCoeffs1,
    CameraModel.Pinhole,
    cameraMatrix2,
    distCoeffs2,
    CameraModel.Fisheye);

try
{
    Console.WriteLine(registration.ReprojectionError);
    Console.WriteLine(registration.R.Rows + "x" + registration.R.Cols);
    Console.WriteLine(registration.PerViewErrors.Rows + "x" + registration.PerViewErrors.Cols);
}
finally
{
    registration.R.Dispose();
    registration.T.Dispose();
    registration.E.Dispose();
    registration.F.Dispose();
    registration.PerViewErrors.Dispose();
}
```

Every matrix returned by an owned-result overload is caller-disposable. The input camera matrices and distortion coefficients remain owned by the caller and are not disposed by registration.

owned-result 重载返回的每个矩阵都必须由调用方释放。输入的相机矩阵和畸变系数仍由调用方持有，注册过程不会释放它们。

## Stereo Rectification Ownership / 双目校正所有权

`StereoRectify` is available in caller-owned and owned-result forms. The caller-owned form writes `R1`, `R2`, `P1`, `P2`, and `Q` into supplied `Mat` outputs and returns `ValidPixROI1`/`ValidPixROI2` through `out` parameters. The owned-result form allocates those five matrices and returns them in `StereoRectifyResult`.

`StereoRectify` 提供调用方拥有输出和结果对象拥有输出两种形式。调用方拥有输出的形式会把 `R1`、`R2`、`P1`、`P2` 和 `Q` 写入传入的 `Mat`，并通过 `out` 参数返回 `ValidPixROI1`/`ValidPixROI2`。结果对象拥有输出的形式会分配这五个矩阵，并通过 `StereoRectifyResult` 返回。

The managed wrapper validates camera matrices, distortion coefficients, positive source image size, non-negative optional new image size, `3 x 3` rotation, `3 x 1` or `1 x 3` translation, and distinct caller-owned output matrices before the native call.

managed 封装会在 native 调用前校验相机矩阵、畸变系数、正数原始图像尺寸、非负可选新图像尺寸、`3 x 3` 旋转、`3 x 1` 或 `1 x 3` 平移，以及调用方拥有输出形式中的各输出矩阵不能互为别名。

```csharp
StereoRectifyResult rectification = Calib3DCv2.StereoRectify(
    cameraMatrix1,
    distCoeffs1,
    cameraMatrix2,
    distCoeffs2,
    new Size(640, 480),
    r,
    t);

try
{
    Console.WriteLine(rectification.R1.Rows + "x" + rectification.R1.Cols);
    Console.WriteLine(rectification.P2.Rows + "x" + rectification.P2.Cols);
    Console.WriteLine(rectification.ValidPixROI1);
}
finally
{
    rectification.R1.Dispose();
    rectification.R2.Dispose();
    rectification.P1.Dispose();
    rectification.P2.Dispose();
    rectification.Q.Dispose();
}
```

## Uncalibrated Rectification Ownership / 未标定校正所有权

`StereoRectifyUncalibrated` is available in caller-owned and owned-result forms. The caller-owned form writes `H1` and `H2` into supplied homography matrices and returns the OpenCV success flag as `bool`. The owned-result form allocates both homographies and returns `StereoRectifyUncalibratedResult`, which contains `Success`, `H1`, and `H2`.

`StereoRectifyUncalibrated` 提供调用方拥有输出和结果对象拥有输出两种形式。调用方拥有输出的形式会把 `H1` 和 `H2` 写入传入的单应矩阵，并以 `bool` 返回 OpenCV 的成功标志。结果对象拥有输出的形式会分配两个单应矩阵，并返回包含 `Success`、`H1` 和 `H2` 的 `StereoRectifyUncalibratedResult`。

The managed wrapper validates `CV_32FC2` point matrices, matching point counts, a `3 x 3` fundamental matrix, positive image size, and distinct caller-owned homography outputs before the native call.

managed 封装会在 native 调用前校验 `CV_32FC2` 点矩阵、匹配的点数量、`3 x 3` 基础矩阵、正数图像尺寸，以及调用方拥有输出形式中的两个单应矩阵不能互为别名。

```csharp
StereoRectifyUncalibratedResult uncalibrated = Calib3DCv2.StereoRectifyUncalibrated(
    points1,
    points2,
    fundamental,
    new Size(640, 480));

try
{
    Console.WriteLine(uncalibrated.Success);
    Console.WriteLine(uncalibrated.H1.Rows + "x" + uncalibrated.H1.Cols);
}
finally
{
    uncalibrated.H1.Dispose();
    uncalibrated.H2.Dispose();
}
```

## Runtime Notes / 运行时说明

`StereoCalibrate`, `RegisterCameras`, and `RegisterCamerasExtended` require the factual OpenCV 5.0.0 runtime artifact `opencv_calib500.dll`. `StereoRectify`, `StereoRectifyUncalibrated`, `Rectify3Collinear`, and `StereoBM` use the stereo/calib split from local OpenCV 5.0.0 and require the staged factual OpenCV 5.0.0 runtime artifact `opencv_stereo500.dll` plus its dependencies.

`StereoCalibrate`、`RegisterCameras` 和 `RegisterCamerasExtended` 需要事实性 OpenCV 5.0.0 runtime 产物 `opencv_calib500.dll`。`StereoRectify`、`StereoRectifyUncalibrated`、`Rectify3Collinear` 和 `StereoBM` 遵循本地 OpenCV 5.0.0 的 stereo/calib 拆分，需要暂存事实性 OpenCV 5.0.0 runtime 产物 `opencv_stereo500.dll` 及其依赖。

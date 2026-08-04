# Calib3D Multiview Calibration Guide

Round 960 adds `Cv2.CalibrateMultiview` and `Cv2.CalibrateMultiviewExtended` for calibrating a fixed rig of two or more cameras. The API estimates one intrinsic model per camera, camera poses relative to camera 0, and one target pose per visible frame.

Round 960 增加了 `Cv2.CalibrateMultiview` 与 `Cv2.CalibrateMultiviewExtended`，用于标定由两台或更多相机组成的固定相机系统。API 会估计每台相机的内参模型、相对相机 0 的相机位姿，以及每个可见帧中的标定目标位姿。

## Choosing The API / API 选择

- Use `StereoCalibrate` to jointly calibrate one stereo pair with matching observations.
- Use `RegisterCameras` when two cameras are already calibrated and only their relative pose is needed.
- Use `CalibrateMultiview` when a fixed rig contains two or more cameras and observations may be missing by camera or frame.

- 使用 `StereoCalibrate` 联合标定一对具有匹配观测的双目相机。
- 当两台相机已经完成内参标定、只需求相对位姿时，使用 `RegisterCameras`。
- 当固定系统包含两台或更多相机，并且不同相机或帧可能缺少观测时，使用 `CalibrateMultiview`。

## Input Shape / 输入形状

The managed input is camera-major:

托管输入采用相机优先布局：

```csharp
Point3f[][] objectPoints;   // [frame][point]
Point2f[][][] imagePoints;  // [camera][frame][point]
Size[] imageSizes;          // [camera]
bool[][] detectionMask;     // [camera][frame]
CameraModel[] cameraModels; // [camera]
```

Every frame has one non-empty object-point group. A visible camera/frame image group must have the same length as that frame's object-point group. An invisible group may be empty.

每一帧都必须包含一个非空物点组。可见的相机/帧像点组长度必须与该帧物点组一致；不可见组可以为空。

## Visibility And Missing Points / 可见性与缺失点

`detectionMask[camera][frame]` states whether that camera observes the target in that frame. Every camera must observe at least one frame. Two cameras are connected when they share at least one visible frame, and the complete camera-overlap graph must be connected. Direct overlap between every pair is not required; a chain such as camera 0 to camera 1 to camera 2 is valid.

`detectionMask[camera][frame]` 表示该相机是否在该帧观测到标定目标。每台相机至少需要一个可见帧。两台相机只要共享至少一个可见帧就形成连接，整个相机重叠图必须连通；并不要求每对相机都直接重叠，例如相机 0 到相机 1、再到相机 2 的链式连接是有效的。

Within an otherwise visible frame, unobserved pattern points may use invalid image coordinates such as `(-1, -1)`. The wrapper preserves these sentinels exactly. OpenCV may include the invalid coordinates in reported RMS values, so compare errors only when datasets use the same missing-point convention.

在整体可见的帧中，未观测到的局部标定点可以使用 `(-1, -1)` 等无效像点坐标。封装会原样保留这些哨兵值。OpenCV 可能会把无效坐标计入报告的 RMS，因此只有在数据集使用相同缺失点约定时才应直接比较误差。

A frame may be invisible to every camera. In the extended result, its `Rvecs0[frame]` and `Tvecs0[frame]` entries are empty `Mat` objects, and every camera's corresponding `PerFrameErrors` entry is `-1`.

某一帧可以对所有相机都不可见。扩展结果中该帧的 `Rvecs0[frame]` 与 `Tvecs0[frame]` 会是空 `Mat`，并且所有相机在 `PerFrameErrors` 中对应的值都是 `-1`。

## Camera Models / 相机模型

Each camera uses `CameraModel.Pinhole` or `CameraModel.Fisheye`. Normal pairwise registration supports pinhole, fisheye, and supported mixed-model rigs. `CalibrationFlags.StereoRegistration` cannot initialize a mixed pinhole/fisheye rig, so the managed API rejects that combination before the native call.

每台相机使用 `CameraModel.Pinhole` 或 `CameraModel.Fisheye`。普通成对注册支持针孔、鱼眼以及受支持的混合模型系统。`CalibrationFlags.StereoRegistration` 无法初始化针孔/鱼眼混合系统，因此托管 API 会在 native 调用前拒绝该组合。

## Guesses And Flags / 初值与标志

`CalibrationFlags.UseIntrinsicGuess` treats every camera matrix and distortion matrix as input-output. `CalibrationFlags.UseExtrinsicGuess` treats the camera-relative rotation and translation arrays as input-output. Use the caller-owned overloads for either flag so the initial matrices can be supplied.

`CalibrationFlags.UseIntrinsicGuess` 会把每台相机的相机矩阵与畸变矩阵作为输入输出；`CalibrationFlags.UseExtrinsicGuess` 会把相机相对旋转和平移数组作为输入输出。使用任一标志时，应调用 caller-owned 重载以提供初始矩阵。

Owned-result overloads allocate empty output matrices and therefore reject intrinsic and extrinsic guess flags explicitly. Per-camera intrinsic flags are supplied through `CalibrationFlags[] flagsForIntrinsics`. The default termination criteria are `CountOrEps`, 100 iterations, and `DBL_EPSILON`.

owned-result 重载会分配空输出矩阵，因此会明确拒绝内参和外参初值标志。每台相机的内参标定标志通过 `CalibrationFlags[] flagsForIntrinsics` 提供。默认终止条件为 `CountOrEps`、100 次迭代和 `DBL_EPSILON`。

## Result Shapes / 结果形状

`MultiviewCalibrationResult` contains:

- `CameraMatrices`: one `3 x 3` matrix per camera.
- `DistCoeffs`: one distortion matrix per camera.
- `RotationVectors`: one `3 x 1` rotation vector per camera, relative to camera 0.
- `TranslationVectors`: one `3 x 1` translation vector per camera, relative to camera 0.
- Camera 0 rotation and translation are zero.

`MultiviewCalibrationResult` 包含：

- `CameraMatrices`：每台相机一个 `3 x 3` 矩阵。
- `DistCoeffs`：每台相机一个畸变矩阵。
- `RotationVectors`：每台相机一个相对相机 0 的 `3 x 1` 旋转向量。
- `TranslationVectors`：每台相机一个相对相机 0 的 `3 x 1` 平移向量。
- 相机 0 的旋转与平移为零。

`MultiviewCalibrationExtendedResult` additionally contains:

- `InitializationPairs`: `(cameraCount - 1) x 2`, type `CV_32S`.
- `Rvecs0` and `Tvecs0`: one camera-0 target pose entry per frame.
- `PerFrameErrors`: `cameraCount x frameCount`, type `CV_64F`; invisible entries are `-1`.

`MultiviewCalibrationExtendedResult` 还包含：

- `InitializationPairs`：`(cameraCount - 1) x 2`，类型为 `CV_32S`。
- `Rvecs0` 与 `Tvecs0`：每帧一个相机 0 参考系下的目标位姿。
- `PerFrameErrors`：`cameraCount x frameCount`，类型为 `CV_64F`；不可见项为 `-1`。

## Example / 示例

```csharp
using JYPPX.OpenCvSharp.Calib3D;
using JYPPX.OpenCvSharp.Core;
using Calib3DCv2 = JYPPX.OpenCvSharp.Calib3D.Cv2;

MultiviewCalibrationExtendedResult result =
    Calib3DCv2.CalibrateMultiviewExtended(
        objectPoints,
        imagePoints,
        imageSizes,
        detectionMask,
        cameraModels);

try
{
    Console.WriteLine(result.Calibration.ReprojectionError);
    Console.WriteLine(result.Calibration.CameraCount);
    Console.WriteLine(result.InitializationPairs.Rows + "x" + result.InitializationPairs.Cols);
    Console.WriteLine(result.PerFrameErrors.Rows + "x" + result.PerFrameErrors.Cols);
}
finally
{
    foreach (Mat value in result.Calibration.CameraMatrices) value.Dispose();
    foreach (Mat value in result.Calibration.DistCoeffs) value.Dispose();
    foreach (Mat value in result.Calibration.RotationVectors) value.Dispose();
    foreach (Mat value in result.Calibration.TranslationVectors) value.Dispose();
    result.InitializationPairs.Dispose();
    foreach (Mat value in result.Rvecs0) value.Dispose();
    foreach (Mat value in result.Tvecs0) value.Dispose();
    result.PerFrameErrors.Dispose();
}
```

Every matrix returned by an owned-result overload is owned by the caller and must be disposed. Caller-owned overloads never dispose supplied matrices.

owned-result 重载返回的每个矩阵都由调用方持有并必须释放。caller-owned 重载不会释放调用方提供的矩阵。

## Runtime Notes / 运行时说明

The linked implementation is provided by the OpenCV 5.0.0 calibration module and requires the staged factual OpenCV 5.0.0 runtime artifact `opencv_calib500.dll` plus its dependencies on Windows. Project-owned API, ABI, package, and namespace names remain version-neutral.

链接实现由 OpenCV 5.0.0 calibration 模块提供；在 Windows 上需要暂存事实性 OpenCV 5.0.0 runtime 产物 `opencv_calib500.dll` 及其依赖。项目自有的 API、ABI、包和命名空间名称保持版本中立。

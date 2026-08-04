using System;

namespace JYPPX.OpenCvSharp.Calib3D
{
    /// <summary>
    /// Calibration flags shared by camera, stereo, fisheye, and multi-view calibration APIs.
    /// 相机、双目、鱼眼和多视图标定 API 共享的标定标志。
    /// </summary>
    [Flags]
    public enum CalibrationFlags
    {
        /// <summary>
        /// No calibration flag.
        /// 不使用标定标志。
        /// </summary>
        None = 0,

        /// <summary>
        /// Uses user-provided intrinsics as an initial estimate.
        /// 使用用户提供的内参作为初值。
        /// </summary>
        UseIntrinsicGuess = 0x00001,

        /// <summary>
        /// Keeps the input aspect ratio fixed.
        /// 固定输入宽高比。
        /// </summary>
        FixAspectRatio = 0x00002,

        /// <summary>
        /// Keeps the principal point fixed.
        /// 固定主点。
        /// </summary>
        FixPrincipalPoint = 0x00004,

        /// <summary>
        /// Sets tangential distortion to zero.
        /// 将切向畸变设为零。
        /// </summary>
        ZeroTangentDist = 0x00008,

        /// <summary>
        /// Keeps focal length fixed.
        /// 固定焦距。
        /// </summary>
        FixFocalLength = 0x00010,

        /// <summary>
        /// Keeps radial distortion coefficient K1 fixed. Fisheye calibration sets K1 to zero and fixes it.
        /// 固定径向畸变系数 K1；鱼眼标定会将 K1 设为零并固定。
        /// </summary>
        FixK1 = 0x00020,

        /// <summary>
        /// Keeps radial distortion coefficient K2 fixed. Fisheye calibration sets K2 to zero and fixes it.
        /// 固定径向畸变系数 K2；鱼眼标定会将 K2 设为零并固定。
        /// </summary>
        FixK2 = 0x00040,

        /// <summary>
        /// Keeps radial distortion coefficient K3 fixed. Fisheye calibration sets K3 to zero and fixes it.
        /// 固定径向畸变系数 K3；鱼眼标定会将 K3 设为零并固定。
        /// </summary>
        FixK3 = 0x00080,

        /// <summary>
        /// Fixes intrinsic parameters during stereo or multi-camera calibration.
        /// 在双目或多相机标定中固定内参。
        /// </summary>
        FixIntrinsic = 0x00100,

        /// <summary>
        /// Enforces the same focal length for stereo cameras.
        /// 强制双目相机使用相同焦距。
        /// </summary>
        SameFocalLength = 0x00200,

        /// <summary>
        /// Moves principal points to the same pixel coordinates in rectified stereo views.
        /// 将校正后双目视图中的主点移动到相同像素坐标。
        /// </summary>
        ZeroDisparity = 0x00400,

        /// <summary>
        /// Keeps radial distortion coefficient K4 fixed. Fisheye calibration sets K4 to zero and fixes it.
        /// 固定径向畸变系数 K4；鱼眼标定会将 K4 设为零并固定。
        /// </summary>
        FixK4 = 0x00800,

        /// <summary>
        /// Keeps radial distortion coefficient K5 fixed.
        /// 固定径向畸变系数 K5。
        /// </summary>
        FixK5 = 0x01000,

        /// <summary>
        /// Keeps radial distortion coefficient K6 fixed.
        /// 固定径向畸变系数 K6。
        /// </summary>
        FixK6 = 0x02000,

        /// <summary>
        /// Enables the rational distortion model.
        /// 启用有理畸变模型。
        /// </summary>
        RationalModel = 0x04000,

        /// <summary>
        /// Enables the thin-prism distortion model.
        /// 启用薄棱镜畸变模型。
        /// </summary>
        ThinPrismModel = 0x08000,

        /// <summary>
        /// Keeps thin-prism coefficients S1 through S4 fixed.
        /// 固定薄棱镜系数 S1 到 S4。
        /// </summary>
        FixS1S2S3S4 = 0x10000,

        /// <summary>
        /// Uses LU decomposition for solving.
        /// 使用 LU 分解求解。
        /// </summary>
        UseLU = 1 << 17,

        /// <summary>
        /// Disables Schur complement optimization.
        /// 禁用 Schur 补优化。
        /// </summary>
        DisableSchurComplement = 1 << 18,

        /// <summary>
        /// Enables the tilted sensor model.
        /// 启用倾斜传感器模型。
        /// </summary>
        TiltedModel = 0x40000,

        /// <summary>
        /// Keeps tilted sensor coefficients tauX and tauY fixed.
        /// 固定倾斜传感器系数 tauX 和 tauY。
        /// </summary>
        FixTauXTauY = 0x80000,

        /// <summary>
        /// Uses QR decomposition for solving.
        /// 使用 QR 分解求解。
        /// </summary>
        UseQR = 0x100000,

        /// <summary>
        /// Fixes tangential distortion coefficients.
        /// 固定切向畸变系数。
        /// </summary>
        FixTangentDist = 0x200000,

        /// <summary>
        /// Uses user-provided extrinsics as an initial estimate.
        /// 使用用户提供的外参作为初值。
        /// </summary>
        UseExtrinsicGuess = 1 << 22,

        /// <summary>
        /// Recomputes board extrinsics during fisheye calibration.
        /// 在鱼眼标定中重新计算标定板外参。
        /// </summary>
        RecomputeExtrinsic = 1 << 23,

        /// <summary>
        /// Checks condition numbers during fisheye calibration.
        /// 在鱼眼标定中检查条件数。
        /// </summary>
        CheckCond = 1 << 24,

        /// <summary>
        /// Keeps fisheye skew fixed.
        /// 固定鱼眼模型偏斜系数。
        /// </summary>
        FixSkew = 1 << 25,

        /// <summary>
        /// Uses stereo registration initialization for multi-view calibration.
        /// 多视图标定中使用双目配准方式初始化。
        /// </summary>
        StereoRegistration = 1 << 26
    }
}

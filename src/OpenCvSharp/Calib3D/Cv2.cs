using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Calib3D
{
    /// <summary>
    /// Provides OpenCV calibration, pose, and multi-view geometry functions.
    /// 提供 OpenCV 相机标定、位姿估计和多视图几何函数。
    /// </summary>
    public static unsafe partial class Cv2
    {
        private static readonly TermCriteria DefaultCalibrationCriteria = TermCriteria.ByCountAndEpsilon(500, 2.2204460492503131E-16);
        private static readonly TermCriteria DefaultStereoCalibrationCriteria = TermCriteria.ByCountAndEpsilon(100, 1e-6);

        /// <summary>
        /// Converts a rotation vector to a rotation matrix, or a rotation matrix to a rotation vector.
        /// 将旋转向量转换为旋转矩阵，或将旋转矩阵转换为旋转向量。
        /// </summary>
        /// <param name="src">The input rotation vector or matrix. 输入旋转向量或矩阵。</param>
        /// <param name="dst">The output rotation matrix or vector. 输出旋转矩阵或向量。</param>
        /// <param name="jacobian">The optional output Jacobian matrix. 可选输出雅可比矩阵。</param>
        /// <exception cref="ArgumentNullException">Thrown when a required matrix is null. 当必需矩阵为空时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static void Rodrigues(Mat src, Mat dst, Mat? jacobian = null)
        {
            ThrowIfNull(src, nameof(src));
            ThrowIfNull(dst, nameof(dst));

            NativeException.ThrowIfError(NativeMethods.Calib3DRodrigues(
                src.NativeHandle,
                dst.NativeHandle,
                GetNativeHandleOrZero(jacobian)));
        }

        /// <summary>
        /// Converts a rotation vector to a rotation matrix, or a rotation matrix to a rotation vector.
        /// 将旋转向量转换为旋转矩阵，或将旋转矩阵转换为旋转向量。
        /// </summary>
        /// <param name="src">The input rotation vector or matrix. 输入旋转向量或矩阵。</param>
        /// <returns>The output rotation matrix or vector. 输出旋转矩阵或向量。</returns>
        public static Mat Rodrigues(Mat src)
        {
            ThrowIfNull(src, nameof(src));

            var dst = new Mat();
            try
            {
                Rodrigues(src, dst);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Computes the RQ decomposition of a 3x3 matrix.
        /// 计算 3x3 矩阵的 RQ 分解。
        /// </summary>
        /// <param name="src">The input 3x3 matrix. 输入 3x3 矩阵。</param>
        /// <param name="mtxR">The output upper-triangular matrix. 输出上三角矩阵。</param>
        /// <param name="mtxQ">The output orthogonal matrix. 输出正交矩阵。</param>
        /// <param name="qx">The optional X-axis rotation matrix. 可选 X 轴旋转矩阵。</param>
        /// <param name="qy">The optional Y-axis rotation matrix. 可选 Y 轴旋转矩阵。</param>
        /// <param name="qz">The optional Z-axis rotation matrix. 可选 Z 轴旋转矩阵。</param>
        /// <returns>The Euler angles in degrees. 欧拉角，单位为度。</returns>
        public static RQDecomp3x3Result RQDecomp3x3(Mat src, Mat mtxR, Mat mtxQ, Mat? qx = null, Mat? qy = null, Mat? qz = null)
        {
            ThrowIfNull(src, nameof(src));
            ThrowIfNull(mtxR, nameof(mtxR));
            ThrowIfNull(mtxQ, nameof(mtxQ));

            NativeException.ThrowIfError(NativeMethods.Calib3DRQDecomp3x3(
                src.NativeHandle,
                mtxR.NativeHandle,
                mtxQ.NativeHandle,
                GetNativeHandleOrZero(qx),
                GetNativeHandleOrZero(qy),
                GetNativeHandleOrZero(qz),
                out double eulerX,
                out double eulerY,
                out double eulerZ));
            return new RQDecomp3x3Result(eulerX, eulerY, eulerZ);
        }

        /// <summary>
        /// Decomposes a projection matrix into camera, rotation, translation, and optional Euler components.
        /// 将投影矩阵分解为相机矩阵、旋转矩阵、平移向量和可选欧拉分量。
        /// </summary>
        /// <param name="projMatrix">The input 3x4 projection matrix. 输入 3x4 投影矩阵。</param>
        /// <param name="cameraMatrix">The output camera matrix. 输出相机矩阵。</param>
        /// <param name="rotMatrix">The output rotation matrix. 输出旋转矩阵。</param>
        /// <param name="transVect">The output homogeneous translation vector. 输出齐次平移向量。</param>
        /// <param name="rotMatrixX">The optional X-axis rotation matrix. 可选 X 轴旋转矩阵。</param>
        /// <param name="rotMatrixY">The optional Y-axis rotation matrix. 可选 Y 轴旋转矩阵。</param>
        /// <param name="rotMatrixZ">The optional Z-axis rotation matrix. 可选 Z 轴旋转矩阵。</param>
        /// <param name="eulerAngles">The optional Euler angles matrix. 可选欧拉角矩阵。</param>
        public static void DecomposeProjectionMatrix(
            Mat projMatrix,
            Mat cameraMatrix,
            Mat rotMatrix,
            Mat transVect,
            Mat? rotMatrixX = null,
            Mat? rotMatrixY = null,
            Mat? rotMatrixZ = null,
            Mat? eulerAngles = null)
        {
            ThrowIfNull(projMatrix, nameof(projMatrix));
            ThrowIfNull(cameraMatrix, nameof(cameraMatrix));
            ThrowIfNull(rotMatrix, nameof(rotMatrix));
            ThrowIfNull(transVect, nameof(transVect));

            NativeException.ThrowIfError(NativeMethods.Calib3DDecomposeProjectionMatrix(
                projMatrix.NativeHandle,
                cameraMatrix.NativeHandle,
                rotMatrix.NativeHandle,
                transVect.NativeHandle,
                GetNativeHandleOrZero(rotMatrixX),
                GetNativeHandleOrZero(rotMatrixY),
                GetNativeHandleOrZero(rotMatrixZ),
                GetNativeHandleOrZero(eulerAngles)));
        }

        /// <summary>
        /// Composes two rotation and translation transforms.
        /// 组合两个旋转和平移变换。
        /// </summary>
        /// <param name="rvec1">The first rotation vector. 第一个旋转向量。</param>
        /// <param name="tvec1">The first translation vector. 第一个平移向量。</param>
        /// <param name="rvec2">The second rotation vector. 第二个旋转向量。</param>
        /// <param name="tvec2">The second translation vector. 第二个平移向量。</param>
        /// <param name="rvec3">The output composed rotation vector. 输出组合旋转向量。</param>
        /// <param name="tvec3">The output composed translation vector. 输出组合平移向量。</param>
        public static void ComposeRT(Mat rvec1, Mat tvec1, Mat rvec2, Mat tvec2, Mat rvec3, Mat tvec3)
        {
            ThrowIfNull(rvec1, nameof(rvec1));
            ThrowIfNull(tvec1, nameof(tvec1));
            ThrowIfNull(rvec2, nameof(rvec2));
            ThrowIfNull(tvec2, nameof(tvec2));
            ThrowIfNull(rvec3, nameof(rvec3));
            ThrowIfNull(tvec3, nameof(tvec3));

            NativeException.ThrowIfError(NativeMethods.Calib3DComposeRT(
                rvec1.NativeHandle,
                tvec1.NativeHandle,
                rvec2.NativeHandle,
                tvec2.NativeHandle,
                rvec3.NativeHandle,
                tvec3.NativeHandle));
        }

        /// <summary>
        /// Projects 3D points to an image plane.
        /// 将三维点投影到图像平面。
        /// </summary>
        /// <param name="objectPoints">The input 3D object points. 输入三维物点。</param>
        /// <param name="rvec">The rotation vector. 旋转向量。</param>
        /// <param name="tvec">The translation vector. 平移向量。</param>
        /// <param name="cameraMatrix">The camera intrinsic matrix. 相机内参矩阵。</param>
        /// <param name="distCoeffs">The distortion coefficients. 畸变系数。</param>
        /// <param name="imagePoints">The output 2D image points. 输出二维图像点。</param>
        /// <param name="jacobian">The optional output Jacobian matrix. 可选输出雅可比矩阵。</param>
        /// <param name="aspectRatio">The optional fixed aspect ratio. 可选固定宽高比。</param>
        public static void ProjectPoints(
            Mat objectPoints,
            Mat rvec,
            Mat tvec,
            Mat cameraMatrix,
            Mat distCoeffs,
            Mat imagePoints,
            Mat? jacobian = null,
            double aspectRatio = 0)
        {
            ThrowIfNull(objectPoints, nameof(objectPoints));
            ThrowIfNull(rvec, nameof(rvec));
            ThrowIfNull(tvec, nameof(tvec));
            ThrowIfNull(cameraMatrix, nameof(cameraMatrix));
            ThrowIfNull(distCoeffs, nameof(distCoeffs));
            ThrowIfNull(imagePoints, nameof(imagePoints));

            _ = ValidateProjectPointsObjectPoints(objectPoints);
            ValidateProjectPointsRotation(rvec, nameof(rvec));
            ValidateProjectPointsTranslation(tvec, nameof(tvec));
            ValidateCameraUtilityMatrix(cameraMatrix, nameof(cameraMatrix));
            ValidateProjectPointsDistortion(distCoeffs);
            ValidateProjectPointsAspectRatio(aspectRatio);
            ValidateProjectPointsOutputs(
                new[] { objectPoints, rvec, tvec, cameraMatrix, distCoeffs },
                imagePoints,
                jacobian);

            NativeException.ThrowIfError(NativeMethods.Calib3DProjectPoints(
                objectPoints.NativeHandle,
                rvec.NativeHandle,
                tvec.NativeHandle,
                cameraMatrix.NativeHandle,
                distCoeffs.NativeHandle,
                imagePoints.NativeHandle,
                GetNativeHandleOrZero(jacobian),
                aspectRatio));
        }

        /// <summary>
        /// Projects 3D points to an image plane and returns a new matrix.
        /// 将三维点投影到图像平面并返回新矩阵。
        /// </summary>
        /// <param name="objectPoints">The input 3D object points. 输入三维物点。</param>
        /// <param name="rvec">The rotation vector. 旋转向量。</param>
        /// <param name="tvec">The translation vector. 平移向量。</param>
        /// <param name="cameraMatrix">The camera intrinsic matrix. 相机内参矩阵。</param>
        /// <param name="distCoeffs">The distortion coefficients. 畸变系数。</param>
        /// <param name="aspectRatio">The optional fixed aspect ratio. 可选固定宽高比。</param>
        /// <returns>The projected image points. 投影后的图像点。</returns>
        public static Mat ProjectPoints(Mat objectPoints, Mat rvec, Mat tvec, Mat cameraMatrix, Mat distCoeffs, double aspectRatio = 0)
        {
            var imagePoints = new Mat();
            try
            {
                ProjectPoints(objectPoints, rvec, tvec, cameraMatrix, distCoeffs, imagePoints, null, aspectRatio);
                return imagePoints;
            }
            catch
            {
                imagePoints.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Solves an object pose from 3D-2D correspondences.
        /// 根据 3D-2D 对应点求解物体位姿。
        /// </summary>
        /// <param name="objectPoints">The input 3D object points. 输入三维物点。</param>
        /// <param name="imagePoints">The input 2D image points. 输入二维像点。</param>
        /// <param name="cameraMatrix">The camera intrinsic matrix. 相机内参矩阵。</param>
        /// <param name="distCoeffs">The distortion coefficients. 畸变系数。</param>
        /// <param name="rvec">The output rotation vector. 输出旋转向量。</param>
        /// <param name="tvec">The output translation vector. 输出平移向量。</param>
        /// <param name="useExtrinsicGuess">Whether to use the supplied <paramref name="rvec"/> and <paramref name="tvec"/> as an initial guess. 是否使用传入的 <paramref name="rvec"/> 和 <paramref name="tvec"/> 作为初值。</param>
        /// <param name="flags">The PnP method. PnP 方法。</param>
        /// <returns><c>true</c> if a pose was found; otherwise, <c>false</c>. 如果找到位姿则返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        public static bool SolvePnP(
            Mat objectPoints,
            Mat imagePoints,
            Mat cameraMatrix,
            Mat distCoeffs,
            Mat rvec,
            Mat tvec,
            bool useExtrinsicGuess = false,
            SolvePnPFlags flags = SolvePnPFlags.Iterative)
        {
            ThrowIfNull(objectPoints, nameof(objectPoints));
            ThrowIfNull(imagePoints, nameof(imagePoints));
            ThrowIfNull(cameraMatrix, nameof(cameraMatrix));
            ThrowIfNull(distCoeffs, nameof(distCoeffs));
            ThrowIfNull(rvec, nameof(rvec));
            ThrowIfNull(tvec, nameof(tvec));

            NativeException.ThrowIfError(NativeMethods.Calib3DSolvePnP(
                objectPoints.NativeHandle,
                imagePoints.NativeHandle,
                cameraMatrix.NativeHandle,
                distCoeffs.NativeHandle,
                rvec.NativeHandle,
                tvec.NativeHandle,
                useExtrinsicGuess ? 1 : 0,
                (int)flags,
                out int solved));
            return solved != 0;
        }

        /// <summary>
        /// Solves an object pose from 3D-2D correspondences using RANSAC.
        /// 使用 RANSAC 根据 3D-2D 对应点求解物体位姿。
        /// </summary>
        /// <param name="objectPoints">The input 3D object points. 输入三维物点。</param>
        /// <param name="imagePoints">The input 2D image points. 输入二维像点。</param>
        /// <param name="cameraMatrix">The camera intrinsic matrix. 相机内参矩阵。</param>
        /// <param name="distCoeffs">The distortion coefficients. 畸变系数。</param>
        /// <param name="rvec">The output rotation vector. 输出旋转向量。</param>
        /// <param name="tvec">The output translation vector. 输出平移向量。</param>
        /// <param name="useExtrinsicGuess">Whether to use the supplied vectors as an initial guess. 是否使用传入向量作为初值。</param>
        /// <param name="iterationsCount">The number of RANSAC iterations. RANSAC 迭代次数。</param>
        /// <param name="reprojectionError">The inlier reprojection threshold. 内点重投影阈值。</param>
        /// <param name="confidence">The confidence value. 置信度。</param>
        /// <param name="inliers">The optional output inlier indices. 可选输出内点索引。</param>
        /// <param name="flags">The PnP method. PnP 方法。</param>
        /// <returns><c>true</c> if a pose was found; otherwise, <c>false</c>. 如果找到位姿则返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        public static bool SolvePnPRansac(
            Mat objectPoints,
            Mat imagePoints,
            Mat cameraMatrix,
            Mat distCoeffs,
            Mat rvec,
            Mat tvec,
            bool useExtrinsicGuess = false,
            int iterationsCount = 100,
            float reprojectionError = 8.0F,
            double confidence = 0.99,
            Mat? inliers = null,
            SolvePnPFlags flags = SolvePnPFlags.Iterative)
        {
            ThrowIfNull(objectPoints, nameof(objectPoints));
            ThrowIfNull(imagePoints, nameof(imagePoints));
            ThrowIfNull(cameraMatrix, nameof(cameraMatrix));
            ThrowIfNull(distCoeffs, nameof(distCoeffs));
            ThrowIfNull(rvec, nameof(rvec));
            ThrowIfNull(tvec, nameof(tvec));

            NativeException.ThrowIfError(NativeMethods.Calib3DSolvePnPRansac(
                objectPoints.NativeHandle,
                imagePoints.NativeHandle,
                cameraMatrix.NativeHandle,
                distCoeffs.NativeHandle,
                rvec.NativeHandle,
                tvec.NativeHandle,
                useExtrinsicGuess ? 1 : 0,
                iterationsCount,
                reprojectionError,
                confidence,
                GetNativeHandleOrZero(inliers),
                (int)flags,
                out int solved));
            return solved != 0;
        }

        /// <summary>
        /// Finds a perspective homography between two point sets.
        /// 在两组点之间估计透视单应矩阵。
        /// </summary>
        /// <param name="srcPoints">The source points. 源点。</param>
        /// <param name="dstPoints">The destination points. 目标点。</param>
        /// <param name="method">The estimation method. 估计方法。</param>
        /// <param name="ransacReprojThreshold">The RANSAC reprojection threshold. RANSAC 重投影阈值。</param>
        /// <param name="mask">The optional output inlier mask. 可选输出内点掩码。</param>
        /// <param name="maxIters">The maximum number of iterations. 最大迭代次数。</param>
        /// <param name="confidence">The confidence value. 置信度。</param>
        /// <returns>The homography matrix. 单应矩阵。</returns>
        public static Mat FindHomography(
            Mat srcPoints,
            Mat dstPoints,
            RobustEstimationAlgorithms method = RobustEstimationAlgorithms.LeastSquares,
            double ransacReprojThreshold = 3,
            Mat? mask = null,
            int maxIters = 2000,
            double confidence = 0.995)
        {
            ThrowIfNull(srcPoints, nameof(srcPoints));
            ThrowIfNull(dstPoints, nameof(dstPoints));

            NativeException.ThrowIfError(NativeMethods.Calib3DFindHomography(
                srcPoints.NativeHandle,
                dstPoints.NativeHandle,
                (int)method,
                ransacReprojThreshold,
                GetNativeHandleOrZero(mask),
                maxIters,
                confidence,
                out IntPtr homography));
            return new Mat(homography);
        }

        /// <summary>
        /// Finds a fundamental matrix from corresponding points.
        /// 根据对应点估计基础矩阵。
        /// </summary>
        /// <param name="points1">The first point set. 第一组点。</param>
        /// <param name="points2">The second point set. 第二组点。</param>
        /// <param name="method">The estimation method. 估计方法。</param>
        /// <param name="ransacReprojThreshold">The RANSAC reprojection threshold. RANSAC 重投影阈值。</param>
        /// <param name="confidence">The confidence value. 置信度。</param>
        /// <param name="maxIters">The maximum number of iterations. 最大迭代次数。</param>
        /// <param name="mask">The optional output inlier mask. 可选输出内点掩码。</param>
        /// <returns>The fundamental matrix. 基础矩阵。</returns>
        public static Mat FindFundamentalMat(
            Mat points1,
            Mat points2,
            FundamentalMatMethods method = FundamentalMatMethods.RANSAC,
            double ransacReprojThreshold = 3.0,
            double confidence = 0.99,
            int maxIters = 1000,
            Mat? mask = null)
        {
            ThrowIfNull(points1, nameof(points1));
            ThrowIfNull(points2, nameof(points2));

            NativeException.ThrowIfError(NativeMethods.Calib3DFindFundamentalMat(
                points1.NativeHandle,
                points2.NativeHandle,
                (int)method,
                ransacReprojThreshold,
                confidence,
                maxIters,
                GetNativeHandleOrZero(mask),
                out IntPtr fundamental));
            return new Mat(fundamental);
        }

        /// <summary>
        /// Finds an essential matrix using a camera intrinsic matrix.
        /// 使用相机内参矩阵估计本质矩阵。
        /// </summary>
        /// <param name="points1">The first point set. 第一组点。</param>
        /// <param name="points2">The second point set. 第二组点。</param>
        /// <param name="cameraMatrix">The camera intrinsic matrix. 相机内参矩阵。</param>
        /// <param name="method">The robust estimation method. 鲁棒估计方法。</param>
        /// <param name="prob">The confidence probability. 置信概率。</param>
        /// <param name="threshold">The RANSAC threshold. RANSAC 阈值。</param>
        /// <param name="maxIters">The maximum number of iterations. 最大迭代次数。</param>
        /// <param name="mask">The optional output inlier mask. 可选输出内点掩码。</param>
        /// <returns>The essential matrix. 本质矩阵。</returns>
        public static Mat FindEssentialMat(
            Mat points1,
            Mat points2,
            Mat cameraMatrix,
            RobustEstimationAlgorithms method = RobustEstimationAlgorithms.RANSAC,
            double prob = 0.999,
            double threshold = 1.0,
            int maxIters = 1000,
            Mat? mask = null)
        {
            ThrowIfNull(points1, nameof(points1));
            ThrowIfNull(points2, nameof(points2));
            ThrowIfNull(cameraMatrix, nameof(cameraMatrix));

            NativeException.ThrowIfError(NativeMethods.Calib3DFindEssentialMat(
                points1.NativeHandle,
                points2.NativeHandle,
                cameraMatrix.NativeHandle,
                (int)method,
                prob,
                threshold,
                maxIters,
                GetNativeHandleOrZero(mask),
                out IntPtr essential));
            return new Mat(essential);
        }

        /// <summary>
        /// Finds an essential matrix using separate intrinsics and distortion coefficients for each camera.
        /// 使用两台相机各自的内参矩阵和畸变系数估计本质矩阵。
        /// </summary>
        /// <param name="points1">The first point set. 第一组点。</param>
        /// <param name="points2">The second point set. 第二组点。</param>
        /// <param name="cameraMatrix1">The first camera intrinsic matrix. 第一台相机内参矩阵。</param>
        /// <param name="distCoeffs1">The first camera distortion coefficients. 第一台相机畸变系数。</param>
        /// <param name="cameraMatrix2">The second camera intrinsic matrix. 第二台相机内参矩阵。</param>
        /// <param name="distCoeffs2">The second camera distortion coefficients. 第二台相机畸变系数。</param>
        /// <param name="method">The robust estimation method. 鲁棒估计方法。</param>
        /// <param name="prob">The confidence probability. 置信概率。</param>
        /// <param name="threshold">The RANSAC threshold. RANSAC 阈值。</param>
        /// <param name="mask">The optional output inlier mask. 可选输出内点掩码。</param>
        /// <returns>The essential matrix. 本质矩阵。</returns>
        public static Mat FindEssentialMat(
            Mat points1,
            Mat points2,
            Mat cameraMatrix1,
            Mat distCoeffs1,
            Mat cameraMatrix2,
            Mat distCoeffs2,
            RobustEstimationAlgorithms method = RobustEstimationAlgorithms.RANSAC,
            double prob = 0.999,
            double threshold = 1.0,
            Mat? mask = null)
        {
            ThrowIfNull(points1, nameof(points1));
            ThrowIfNull(points2, nameof(points2));
            ThrowIfNull(cameraMatrix1, nameof(cameraMatrix1));
            ThrowIfNull(distCoeffs1, nameof(distCoeffs1));
            ThrowIfNull(cameraMatrix2, nameof(cameraMatrix2));
            ThrowIfNull(distCoeffs2, nameof(distCoeffs2));

            NativeException.ThrowIfError(NativeMethods.Calib3DFindEssentialMatTwoCameras(
                points1.NativeHandle,
                points2.NativeHandle,
                cameraMatrix1.NativeHandle,
                distCoeffs1.NativeHandle,
                cameraMatrix2.NativeHandle,
                distCoeffs2.NativeHandle,
                (int)method,
                prob,
                threshold,
                GetNativeHandleOrZero(mask),
                out IntPtr essential));
            return new Mat(essential);
        }

        /// <summary>
        /// Finds an essential matrix using focal length and principal point.
        /// 使用焦距和主点估计本质矩阵。
        /// </summary>
        /// <param name="points1">The first point set. 第一组点。</param>
        /// <param name="points2">The second point set. 第二组点。</param>
        /// <param name="focal">The focal length. 焦距。</param>
        /// <param name="pp">The principal point. 主点。</param>
        /// <param name="method">The robust estimation method. 鲁棒估计方法。</param>
        /// <param name="prob">The confidence probability. 置信概率。</param>
        /// <param name="threshold">The RANSAC threshold. RANSAC 阈值。</param>
        /// <param name="maxIters">The maximum number of iterations. 最大迭代次数。</param>
        /// <param name="mask">The optional output inlier mask. 可选输出内点掩码。</param>
        /// <returns>The essential matrix. 本质矩阵。</returns>
        public static Mat FindEssentialMat(
            Mat points1,
            Mat points2,
            double focal,
            Point2d pp,
            RobustEstimationAlgorithms method = RobustEstimationAlgorithms.RANSAC,
            double prob = 0.999,
            double threshold = 1.0,
            int maxIters = 1000,
            Mat? mask = null)
        {
            ThrowIfNull(points1, nameof(points1));
            ThrowIfNull(points2, nameof(points2));

            NativeException.ThrowIfError(NativeMethods.Calib3DFindEssentialMatFocal(
                points1.NativeHandle,
                points2.NativeHandle,
                focal,
                pp.X,
                pp.Y,
                (int)method,
                prob,
                threshold,
                maxIters,
                GetNativeHandleOrZero(mask),
                out IntPtr essential));
            return new Mat(essential);
        }

        /// <summary>
        /// Decomposes an essential matrix into two possible rotations and one translation direction.
        /// 将本质矩阵分解为两个可能的旋转矩阵和一个平移方向。
        /// </summary>
        /// <param name="essential">The input essential matrix. 输入本质矩阵。</param>
        /// <param name="r1">The first output rotation matrix. 第一个输出旋转矩阵。</param>
        /// <param name="r2">The second output rotation matrix. 第二个输出旋转矩阵。</param>
        /// <param name="t">The output translation direction. 输出平移方向。</param>
        public static void DecomposeEssentialMat(Mat essential, Mat r1, Mat r2, Mat t)
        {
            ThrowIfNull(essential, nameof(essential));
            ThrowIfNull(r1, nameof(r1));
            ThrowIfNull(r2, nameof(r2));
            ThrowIfNull(t, nameof(t));

            NativeException.ThrowIfError(NativeMethods.Calib3DDecomposeEssentialMat(
                essential.NativeHandle,
                r1.NativeHandle,
                r2.NativeHandle,
                t.NativeHandle));
        }

        /// <summary>
        /// Recovers relative camera pose from an essential matrix and point correspondences.
        /// 根据本质矩阵和对应点恢复相对相机位姿。
        /// </summary>
        /// <param name="essential">The input essential matrix. 输入本质矩阵。</param>
        /// <param name="points1">The first point set. 第一组点。</param>
        /// <param name="points2">The second point set. 第二组点。</param>
        /// <param name="cameraMatrix">The camera intrinsic matrix. 相机内参矩阵。</param>
        /// <param name="r">The output rotation matrix. 输出旋转矩阵。</param>
        /// <param name="t">The output translation direction. 输出平移方向。</param>
        /// <param name="mask">The optional input/output inlier mask. 可选输入输出内点掩码。</param>
        /// <returns>The recover-pose metadata. recover-pose 元数据。</returns>
        public static RecoverPoseResult RecoverPose(Mat essential, Mat points1, Mat points2, Mat cameraMatrix, Mat r, Mat t, Mat? mask = null)
        {
            ThrowIfNull(essential, nameof(essential));
            ThrowIfNull(points1, nameof(points1));
            ThrowIfNull(points2, nameof(points2));
            ThrowIfNull(cameraMatrix, nameof(cameraMatrix));
            ThrowIfNull(r, nameof(r));
            ThrowIfNull(t, nameof(t));

            NativeException.ThrowIfError(NativeMethods.Calib3DRecoverPose(
                essential.NativeHandle,
                points1.NativeHandle,
                points2.NativeHandle,
                cameraMatrix.NativeHandle,
                r.NativeHandle,
                t.NativeHandle,
                GetNativeHandleOrZero(mask),
                out int inlierCount));
            return new RecoverPoseResult(inlierCount);
        }

        /// <summary>
        /// Recovers relative camera pose using focal length and principal point.
        /// 使用焦距和主点恢复相对相机位姿。
        /// </summary>
        /// <param name="essential">The input essential matrix. 输入本质矩阵。</param>
        /// <param name="points1">The first point set. 第一组点。</param>
        /// <param name="points2">The second point set. 第二组点。</param>
        /// <param name="r">The output rotation matrix. 输出旋转矩阵。</param>
        /// <param name="t">The output translation direction. 输出平移方向。</param>
        /// <param name="focal">The focal length. 焦距。</param>
        /// <param name="pp">The principal point. 主点。</param>
        /// <param name="mask">The optional input/output inlier mask. 可选输入输出内点掩码。</param>
        /// <returns>The recover-pose metadata. recover-pose 元数据。</returns>
        public static RecoverPoseResult RecoverPose(Mat essential, Mat points1, Mat points2, Mat r, Mat t, double focal, Point2d pp, Mat? mask = null)
        {
            ThrowIfNull(essential, nameof(essential));
            ThrowIfNull(points1, nameof(points1));
            ThrowIfNull(points2, nameof(points2));
            ThrowIfNull(r, nameof(r));
            ThrowIfNull(t, nameof(t));

            NativeException.ThrowIfError(NativeMethods.Calib3DRecoverPoseFocal(
                essential.NativeHandle,
                points1.NativeHandle,
                points2.NativeHandle,
                r.NativeHandle,
                t.NativeHandle,
                focal,
                pp.X,
                pp.Y,
                GetNativeHandleOrZero(mask),
                out int inlierCount));
            return new RecoverPoseResult(inlierCount);
        }

        /// <summary>
        /// Recovers relative camera pose and optionally outputs triangulated points.
        /// 恢复相对相机位姿，并可选输出三角化点。
        /// </summary>
        /// <param name="essential">The input essential matrix. 输入本质矩阵。</param>
        /// <param name="points1">The first point set. 第一组点。</param>
        /// <param name="points2">The second point set. 第二组点。</param>
        /// <param name="cameraMatrix">The camera intrinsic matrix. 相机内参矩阵。</param>
        /// <param name="r">The output rotation matrix. 输出旋转矩阵。</param>
        /// <param name="t">The output translation direction. 输出平移方向。</param>
        /// <param name="distanceThresh">The distance threshold used to reject far points. 用于剔除远点的距离阈值。</param>
        /// <param name="mask">The optional input/output inlier mask. 可选输入输出内点掩码。</param>
        /// <param name="triangulatedPoints">The optional output triangulated points. 可选输出三角化点。</param>
        /// <returns>The recover-pose metadata. recover-pose 元数据。</returns>
        public static RecoverPoseResult RecoverPose(
            Mat essential,
            Mat points1,
            Mat points2,
            Mat cameraMatrix,
            Mat r,
            Mat t,
            double distanceThresh,
            Mat? mask = null,
            Mat? triangulatedPoints = null)
        {
            ThrowIfNull(essential, nameof(essential));
            ThrowIfNull(points1, nameof(points1));
            ThrowIfNull(points2, nameof(points2));
            ThrowIfNull(cameraMatrix, nameof(cameraMatrix));
            ThrowIfNull(r, nameof(r));
            ThrowIfNull(t, nameof(t));

            NativeException.ThrowIfError(NativeMethods.Calib3DRecoverPoseWithDistance(
                essential.NativeHandle,
                points1.NativeHandle,
                points2.NativeHandle,
                cameraMatrix.NativeHandle,
                r.NativeHandle,
                t.NativeHandle,
                distanceThresh,
                GetNativeHandleOrZero(mask),
                GetNativeHandleOrZero(triangulatedPoints),
                out int inlierCount));
            return new RecoverPoseResult(inlierCount);
        }

        /// <summary>
        /// Recovers relative pose using separate intrinsics and distortion coefficients for each camera.
        /// 使用两台相机各自的内参矩阵和畸变系数恢复相对位姿。
        /// </summary>
        /// <param name="points1">The first point set. 第一组点。</param>
        /// <param name="points2">The second point set. 第二组点。</param>
        /// <param name="cameraMatrix1">The first camera intrinsic matrix. 第一台相机内参矩阵。</param>
        /// <param name="distCoeffs1">The first camera distortion coefficients. 第一台相机畸变系数。</param>
        /// <param name="cameraMatrix2">The second camera intrinsic matrix. 第二台相机内参矩阵。</param>
        /// <param name="distCoeffs2">The second camera distortion coefficients. 第二台相机畸变系数。</param>
        /// <param name="essential">The output essential matrix. 输出本质矩阵。</param>
        /// <param name="r">The output rotation matrix. 输出旋转矩阵。</param>
        /// <param name="t">The output translation direction. 输出平移方向。</param>
        /// <param name="method">The robust estimation method. 鲁棒估计方法。</param>
        /// <param name="prob">The confidence probability. 置信概率。</param>
        /// <param name="threshold">The RANSAC threshold. RANSAC 阈值。</param>
        /// <param name="mask">The optional input/output inlier mask. 可选输入输出内点掩码。</param>
        /// <returns>The recover-pose metadata. recover-pose 元数据。</returns>
        public static RecoverPoseResult RecoverPose(
            Mat points1,
            Mat points2,
            Mat cameraMatrix1,
            Mat distCoeffs1,
            Mat cameraMatrix2,
            Mat distCoeffs2,
            Mat essential,
            Mat r,
            Mat t,
            RobustEstimationAlgorithms method = RobustEstimationAlgorithms.RANSAC,
            double prob = 0.999,
            double threshold = 1.0,
            Mat? mask = null)
        {
            ThrowIfNull(points1, nameof(points1));
            ThrowIfNull(points2, nameof(points2));
            ThrowIfNull(cameraMatrix1, nameof(cameraMatrix1));
            ThrowIfNull(distCoeffs1, nameof(distCoeffs1));
            ThrowIfNull(cameraMatrix2, nameof(cameraMatrix2));
            ThrowIfNull(distCoeffs2, nameof(distCoeffs2));
            ThrowIfNull(essential, nameof(essential));
            ThrowIfNull(r, nameof(r));
            ThrowIfNull(t, nameof(t));

            NativeException.ThrowIfError(NativeMethods.Calib3DRecoverPoseTwoCameras(
                points1.NativeHandle,
                points2.NativeHandle,
                cameraMatrix1.NativeHandle,
                distCoeffs1.NativeHandle,
                cameraMatrix2.NativeHandle,
                distCoeffs2.NativeHandle,
                essential.NativeHandle,
                r.NativeHandle,
                t.NativeHandle,
                (int)method,
                prob,
                threshold,
                GetNativeHandleOrZero(mask),
                out int inlierCount));
            return new RecoverPoseResult(inlierCount);
        }

        /// <summary>
        /// Computes corresponding epipolar lines for points in one of the two images.
        /// 计算某一幅图像中点在另一幅图像中的对应极线。
        /// </summary>
        /// <param name="points">The input points. 输入点。</param>
        /// <param name="whichImage">The image index, 1 or 2. 图像编号，1 或 2。</param>
        /// <param name="fundamental">The fundamental matrix. 基础矩阵。</param>
        /// <param name="lines">The output line coefficients. 输出直线系数。</param>
        public static void ComputeCorrespondEpilines(Mat points, int whichImage, Mat fundamental, Mat lines)
        {
            ThrowIfNull(points, nameof(points));
            ThrowIfNull(fundamental, nameof(fundamental));
            ThrowIfNull(lines, nameof(lines));
            ValidateEpilineImageIndex(whichImage, nameof(whichImage));
            ValidateEpilinePointMatrix(points, nameof(points));
            ValidateFundamentalMatrix(fundamental, nameof(fundamental), false);
            ValidateEpilineOutputDoesNotAlias(points, fundamental, lines, nameof(lines));

            NativeException.ThrowIfError(NativeMethods.Calib3DComputeCorrespondEpilines(
                points.NativeHandle,
                whichImage,
                fundamental.NativeHandle,
                lines.NativeHandle));
        }

        /// <summary>
        /// Computes corresponding epipolar lines and returns an owned line coefficient matrix.
        /// 计算对应极线，并返回拥有所有权的直线系数矩阵。
        /// </summary>
        /// <param name="points">The input points. 输入点。</param>
        /// <param name="whichImage">The image index, 1 or 2. 图像编号，1 或 2。</param>
        /// <param name="fundamental">The fundamental matrix. 基础矩阵。</param>
        /// <returns>The owned output line coefficients. 拥有所有权的输出直线系数。</returns>
        public static Mat ComputeCorrespondEpilines(Mat points, int whichImage, Mat fundamental)
        {
            var lines = new Mat();
            try
            {
                ComputeCorrespondEpilines(points, whichImage, fundamental, lines);
                return lines;
            }
            catch
            {
                lines.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Triangulates points from two projection matrices and matching point sets.
        /// 根据两个投影矩阵和匹配点集进行三角化。
        /// </summary>
        /// <param name="projMatr1">The first 3x4 projection matrix. 第一个 3x4 投影矩阵。</param>
        /// <param name="projMatr2">The second 3x4 projection matrix. 第二个 3x4 投影矩阵。</param>
        /// <param name="projPoints1">The first image points. 第一幅图像中的点。</param>
        /// <param name="projPoints2">The second image points. 第二幅图像中的点。</param>
        /// <param name="points4D">The output homogeneous 4D points. 输出齐次 4D 点。</param>
        public static void TriangulatePoints(Mat projMatr1, Mat projMatr2, Mat projPoints1, Mat projPoints2, Mat points4D)
        {
            ThrowIfNull(projMatr1, nameof(projMatr1));
            ThrowIfNull(projMatr2, nameof(projMatr2));
            ThrowIfNull(projPoints1, nameof(projPoints1));
            ThrowIfNull(projPoints2, nameof(projPoints2));
            ThrowIfNull(points4D, nameof(points4D));
            ValidateTriangulationProjectionMatrix(projMatr1, nameof(projMatr1));
            ValidateTriangulationProjectionMatrix(projMatr2, nameof(projMatr2));
            ValidateTriangulationPointMatrix(projPoints1, nameof(projPoints1));
            ValidateTriangulationPointMatrix(projPoints2, nameof(projPoints2));
            ValidateMatchingPointMatrixCount(projPoints1, nameof(projPoints1), projPoints2, nameof(projPoints2));
            ValidateTriangulationOutputDoesNotAlias(projMatr1, projMatr2, projPoints1, projPoints2, points4D, nameof(points4D));

            NativeException.ThrowIfError(NativeMethods.Calib3DTriangulatePoints(
                projMatr1.NativeHandle,
                projMatr2.NativeHandle,
                projPoints1.NativeHandle,
                projPoints2.NativeHandle,
                points4D.NativeHandle));
        }

        /// <summary>
        /// Triangulates points from two projection matrices and returns an owned homogeneous 4D point matrix.
        /// 根据两个投影矩阵进行三角化，并返回拥有所有权的齐次 4D 点矩阵。
        /// </summary>
        /// <param name="projMatr1">The first 3x4 projection matrix. 第一个 3x4 投影矩阵。</param>
        /// <param name="projMatr2">The second 3x4 projection matrix. 第二个 3x4 投影矩阵。</param>
        /// <param name="projPoints1">The first image points. 第一幅图像中的点。</param>
        /// <param name="projPoints2">The second image points. 第二幅图像中的点。</param>
        /// <returns>The owned homogeneous 4D points. 拥有所有权的齐次 4D 点。</returns>
        public static Mat TriangulatePoints(Mat projMatr1, Mat projMatr2, Mat projPoints1, Mat projPoints2)
        {
            var points4D = new Mat();
            try
            {
                TriangulatePoints(projMatr1, projMatr2, projPoints1, projPoints2, points4D);
                return points4D;
            }
            catch
            {
                points4D.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Computes ideal undistorted point coordinates.
        /// 计算理想无畸变点坐标。
        /// </summary>
        /// <param name="src">The input distorted points. 输入畸变点。</param>
        /// <param name="dst">The output undistorted points. 输出无畸变点。</param>
        /// <param name="cameraMatrix">The camera intrinsic matrix. 相机内参矩阵。</param>
        /// <param name="distCoeffs">The distortion coefficients. 畸变系数。</param>
        /// <param name="r">The optional rectification transform. 可选校正变换。</param>
        /// <param name="p">The optional new camera or projection matrix. 可选新相机矩阵或投影矩阵。</param>
        /// <param name="criteria">The iterative undistortion termination criteria. 迭代去畸变终止条件。</param>
        public static void UndistortPoints(Mat src, Mat dst, Mat cameraMatrix, Mat distCoeffs, Mat? r = null, Mat? p = null, TermCriteria? criteria = null)
        {
            ThrowIfNull(src, nameof(src));
            ThrowIfNull(dst, nameof(dst));
            ThrowIfNull(cameraMatrix, nameof(cameraMatrix));
            ThrowIfNull(distCoeffs, nameof(distCoeffs));
            ValidateUndistortPointMatrix(src, nameof(src));
            ValidateCameraUtilityMatrix(cameraMatrix, nameof(cameraMatrix));
            ValidatePinholeDistortionCoefficients(distCoeffs, nameof(distCoeffs));
            ValidateUndistortOptionalRectification(r, nameof(r));
            ValidateCameraUtilityProjection(p, nameof(p));
            ValidateUndistortPointsOutputDoesNotAlias(
                src,
                dst,
                cameraMatrix,
                distCoeffs,
                r,
                p,
                nameof(dst));

            TermCriteria actualCriteria = criteria ?? TermCriteria.ByCountAndEpsilon(10, 1e-8);
            ValidateRegistrationCriteria(actualCriteria, nameof(criteria));
            NativeException.ThrowIfError(NativeMethods.Calib3DUndistortPoints(
                src.NativeHandle,
                dst.NativeHandle,
                cameraMatrix.NativeHandle,
                distCoeffs.NativeHandle,
                GetNativeHandleOrZero(r),
                GetNativeHandleOrZero(p),
                (int)actualCriteria.Type,
                actualCriteria.MaxCount,
                actualCriteria.Epsilon));
        }

        /// <summary>
        /// Computes ideal undistorted point coordinates and returns an owned point matrix.
        /// 计算理想无畸变点坐标，并返回拥有所有权的点矩阵。
        /// </summary>
        /// <param name="src">The input distorted points. 输入畸变点。</param>
        /// <param name="cameraMatrix">The camera intrinsic matrix. 相机内参矩阵。</param>
        /// <param name="distCoeffs">The distortion coefficients. 畸变系数。</param>
        /// <returns>The owned undistorted points. 拥有所有权的无畸变点。</returns>
        public static Mat UndistortPoints(Mat src, Mat cameraMatrix, Mat distCoeffs)
        {
            var dst = new Mat();
            try
            {
                UndistortPoints(src, dst, cameraMatrix, distCoeffs);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Computes undistortion and rectification maps.
        /// 计算去畸变和校正映射。
        /// </summary>
        /// <param name="cameraMatrix">The camera intrinsic matrix. 相机内参矩阵。</param>
        /// <param name="distCoeffs">The distortion coefficients. 畸变系数。</param>
        /// <param name="r">The rectification transform. 校正变换。</param>
        /// <param name="newCameraMatrix">The new camera matrix. 新相机矩阵。</param>
        /// <param name="size">The undistorted image size. 去畸变图像尺寸。</param>
        /// <param name="m1type">The first map type. 第一个映射矩阵类型。</param>
        /// <param name="map1">The first output map. 第一个输出映射。</param>
        /// <param name="map2">The second output map. 第二个输出映射。</param>
        public static void InitUndistortRectifyMap(Mat cameraMatrix, Mat distCoeffs, Mat r, Mat newCameraMatrix, Size size, int m1type, Mat map1, Mat map2)
        {
            ThrowIfNull(cameraMatrix, nameof(cameraMatrix));
            ThrowIfNull(distCoeffs, nameof(distCoeffs));
            ThrowIfNull(r, nameof(r));
            ThrowIfNull(newCameraMatrix, nameof(newCameraMatrix));
            ThrowIfNull(map1, nameof(map1));
            ThrowIfNull(map2, nameof(map2));
            ValidateCameraUtilityMatrix(cameraMatrix, nameof(cameraMatrix));
            ValidatePinholeDistortionCoefficients(distCoeffs, nameof(distCoeffs));
            ValidateCameraUtilityRectification(r, nameof(r));
            ValidateCameraUtilityProjection(newCameraMatrix, nameof(newCameraMatrix));
            ValidatePositiveSize(size, nameof(size));
            ValidateInitUndistortRectifyMapType(m1type, nameof(m1type));
            ValidateDistinctOutputPair(map1, nameof(map1), map2, nameof(map2));

            NativeException.ThrowIfError(NativeMethods.Calib3DInitUndistortRectifyMap(
                cameraMatrix.NativeHandle,
                distCoeffs.NativeHandle,
                r.NativeHandle,
                newCameraMatrix.NativeHandle,
                size.Width,
                size.Height,
                m1type,
                map1.NativeHandle,
                map2.NativeHandle));
        }

        /// <summary>
        /// Computes undistortion and rectification maps and returns owned output matrices.
        /// 计算去畸变和校正映射，并返回拥有所有权的输出矩阵。
        /// </summary>
        /// <param name="cameraMatrix">The camera intrinsic matrix. 相机内参矩阵。</param>
        /// <param name="distCoeffs">The distortion coefficients. 畸变系数。</param>
        /// <param name="r">The rectification transform. 校正变换。</param>
        /// <param name="newCameraMatrix">The new camera matrix. 新相机矩阵。</param>
        /// <param name="size">The undistorted image size. 去畸变图像尺寸。</param>
        /// <param name="m1type">The first map type. 第一个映射矩阵类型。</param>
        /// <returns>The owned undistortion/rectification maps. 拥有所有权的去畸变/校正映射。</returns>
        public static UndistortRectifyMapResult InitUndistortRectifyMap(
            Mat cameraMatrix,
            Mat distCoeffs,
            Mat r,
            Mat newCameraMatrix,
            Size size,
            int m1type)
        {
            var map1 = new Mat();
            var map2 = new Mat();
            try
            {
                InitUndistortRectifyMap(
                    cameraMatrix,
                    distCoeffs,
                    r,
                    newCameraMatrix,
                    size,
                    m1type,
                    map1,
                    map2);
                return new UndistortRectifyMapResult(map1, map2);
            }
            catch
            {
                map1.Dispose();
                map2.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Computes rectification transforms for a calibrated stereo camera.
        /// 计算已标定双目相机的校正变换。
        /// </summary>
        /// <param name="cameraMatrix1">The first camera matrix. 第一个相机矩阵。</param>
        /// <param name="distCoeffs1">The first distortion coefficients. 第一组畸变系数。</param>
        /// <param name="cameraMatrix2">The second camera matrix. 第二个相机矩阵。</param>
        /// <param name="distCoeffs2">The second distortion coefficients. 第二组畸变系数。</param>
        /// <param name="imageSize">The original image size. 原始图像尺寸。</param>
        /// <param name="r">The rotation between cameras. 相机之间的旋转。</param>
        /// <param name="t">The translation between cameras. 相机之间的平移。</param>
        /// <param name="r1">The first output rectification transform. 第一个输出校正变换。</param>
        /// <param name="r2">The second output rectification transform. 第二个输出校正变换。</param>
        /// <param name="p1">The first output projection matrix. 第一个输出投影矩阵。</param>
        /// <param name="p2">The second output projection matrix. 第二个输出投影矩阵。</param>
        /// <param name="q">The output disparity-to-depth mapping matrix. 输出视差到深度映射矩阵。</param>
        /// <param name="flags">The stereo rectification flags. 双目校正标志。</param>
        /// <param name="alpha">The free scaling parameter. 自由缩放参数。</param>
        /// <param name="newImageSize">The optional new image size. 可选新图像尺寸。</param>
        /// <param name="validPixROI1">The first valid-pixel ROI. 第一个有效像素 ROI。</param>
        /// <param name="validPixROI2">The second valid-pixel ROI. 第二个有效像素 ROI。</param>
        public static void StereoRectify(
            Mat cameraMatrix1,
            Mat distCoeffs1,
            Mat cameraMatrix2,
            Mat distCoeffs2,
            Size imageSize,
            Mat r,
            Mat t,
            Mat r1,
            Mat r2,
            Mat p1,
            Mat p2,
            Mat q,
            StereoRectifyFlags flags,
            double alpha,
            Size newImageSize,
            out Rect validPixROI1,
            out Rect validPixROI2)
        {
            ThrowIfNull(cameraMatrix1, nameof(cameraMatrix1));
            ThrowIfNull(distCoeffs1, nameof(distCoeffs1));
            ThrowIfNull(cameraMatrix2, nameof(cameraMatrix2));
            ThrowIfNull(distCoeffs2, nameof(distCoeffs2));
            ThrowIfNull(r, nameof(r));
            ThrowIfNull(t, nameof(t));
            ThrowIfNull(r1, nameof(r1));
            ThrowIfNull(r2, nameof(r2));
            ThrowIfNull(p1, nameof(p1));
            ThrowIfNull(p2, nameof(p2));
            ThrowIfNull(q, nameof(q));
            ValidateCameraUtilityMatrix(cameraMatrix1, nameof(cameraMatrix1));
            ValidatePinholeDistortionCoefficients(distCoeffs1, nameof(distCoeffs1));
            ValidateCameraUtilityMatrix(cameraMatrix2, nameof(cameraMatrix2));
            ValidatePinholeDistortionCoefficients(distCoeffs2, nameof(distCoeffs2));
            ValidatePositiveSize(imageSize, nameof(imageSize));
            ValidateCameraUtilityRectification(r, nameof(r));
            ValidateCameraUtilityTranslation(t, nameof(t));
            ValidateNonNegativeSize(newImageSize, nameof(newImageSize));
            ValidateDistinctOutputSet(
                new[] { r1, r2, p1, p2, q },
                new[] { nameof(r1), nameof(r2), nameof(p1), nameof(p2), nameof(q) });

            NativeException.ThrowIfError(NativeMethods.Calib3DStereoRectify(
                cameraMatrix1.NativeHandle,
                distCoeffs1.NativeHandle,
                cameraMatrix2.NativeHandle,
                distCoeffs2.NativeHandle,
                imageSize.Width,
                imageSize.Height,
                r.NativeHandle,
                t.NativeHandle,
                r1.NativeHandle,
                r2.NativeHandle,
                p1.NativeHandle,
                p2.NativeHandle,
                q.NativeHandle,
                (int)flags,
                alpha,
                newImageSize.Width,
                newImageSize.Height,
                out int roi1X,
                out int roi1Y,
                out int roi1Width,
                out int roi1Height,
                out int roi2X,
                out int roi2Y,
                out int roi2Width,
                out int roi2Height));

            validPixROI1 = new Rect(roi1X, roi1Y, roi1Width, roi1Height);
            validPixROI2 = new Rect(roi2X, roi2Y, roi2Width, roi2Height);
        }

        /// <summary>
        /// Stereo-rectifies with OpenCV's default flags, alpha, and new image size.
        /// 使用 OpenCV 默认标志、alpha 和新图像尺寸执行双目校正。
        /// </summary>
        /// <param name="cameraMatrix1">The first camera matrix. 第一个相机矩阵。</param>
        /// <param name="distCoeffs1">The first distortion coefficients. 第一组畸变系数。</param>
        /// <param name="cameraMatrix2">The second camera matrix. 第二个相机矩阵。</param>
        /// <param name="distCoeffs2">The second distortion coefficients. 第二组畸变系数。</param>
        /// <param name="imageSize">The original image size. 原始图像尺寸。</param>
        /// <param name="r">The rotation between cameras. 相机之间的旋转。</param>
        /// <param name="t">The translation between cameras. 相机之间的平移。</param>
        /// <param name="r1">The first output rectification transform. 第一个输出校正变换。</param>
        /// <param name="r2">The second output rectification transform. 第二个输出校正变换。</param>
        /// <param name="p1">The first output projection matrix. 第一个输出投影矩阵。</param>
        /// <param name="p2">The second output projection matrix. 第二个输出投影矩阵。</param>
        /// <param name="q">The output disparity-to-depth mapping matrix. 输出视差到深度映射矩阵。</param>
        /// <param name="validPixROI1">The first valid-pixel ROI. 第一个有效像素 ROI。</param>
        /// <param name="validPixROI2">The second valid-pixel ROI. 第二个有效像素 ROI。</param>
        public static void StereoRectify(
            Mat cameraMatrix1,
            Mat distCoeffs1,
            Mat cameraMatrix2,
            Mat distCoeffs2,
            Size imageSize,
            Mat r,
            Mat t,
            Mat r1,
            Mat r2,
            Mat p1,
            Mat p2,
            Mat q,
            out Rect validPixROI1,
            out Rect validPixROI2)
        {
            StereoRectify(
                cameraMatrix1,
                distCoeffs1,
                cameraMatrix2,
                distCoeffs2,
                imageSize,
                r,
                t,
                r1,
                r2,
                p1,
                p2,
                q,
                StereoRectifyFlags.ZeroDisparity,
                -1.0,
                new Size(),
                out validPixROI1,
                out validPixROI2);
        }

        /// <summary>
        /// Stereo-rectifies a calibrated camera pair and returns owned output matrices.
        /// 对已标定相机对执行双目校正，并返回拥有所有权的输出矩阵。
        /// </summary>
        /// <param name="cameraMatrix1">The first camera matrix. 第一个相机矩阵。</param>
        /// <param name="distCoeffs1">The first distortion coefficients. 第一组畸变系数。</param>
        /// <param name="cameraMatrix2">The second camera matrix. 第二个相机矩阵。</param>
        /// <param name="distCoeffs2">The second distortion coefficients. 第二组畸变系数。</param>
        /// <param name="imageSize">The original image size. 原始图像尺寸。</param>
        /// <param name="r">The rotation between cameras. 相机之间的旋转。</param>
        /// <param name="t">The translation between cameras. 相机之间的平移。</param>
        /// <param name="flags">The stereo rectification flags. 双目校正标志。</param>
        /// <param name="alpha">The free scaling parameter. 自由缩放参数。</param>
        /// <param name="newImageSize">The optional new image size. 可选新图像尺寸。</param>
        /// <returns>The owned stereo rectification result. 拥有所有权的双目校正结果。</returns>
        public static StereoRectifyResult StereoRectify(
            Mat cameraMatrix1,
            Mat distCoeffs1,
            Mat cameraMatrix2,
            Mat distCoeffs2,
            Size imageSize,
            Mat r,
            Mat t,
            StereoRectifyFlags flags,
            double alpha,
            Size newImageSize)
        {
            var r1 = new Mat();
            var r2 = new Mat();
            var p1 = new Mat();
            var p2 = new Mat();
            var q = new Mat();
            try
            {
                StereoRectify(
                    cameraMatrix1,
                    distCoeffs1,
                    cameraMatrix2,
                    distCoeffs2,
                    imageSize,
                    r,
                    t,
                    r1,
                    r2,
                    p1,
                    p2,
                    q,
                    flags,
                    alpha,
                    newImageSize,
                    out Rect validPixROI1,
                    out Rect validPixROI2);
                return new StereoRectifyResult(r1, r2, p1, p2, q, validPixROI1, validPixROI2);
            }
            catch
            {
                r1.Dispose();
                r2.Dispose();
                p1.Dispose();
                p2.Dispose();
                q.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Stereo-rectifies a calibrated camera pair with OpenCV's default flags, alpha, and new image size, returning owned output matrices.
        /// 使用 OpenCV 默认标志、alpha 和新图像尺寸执行双目校正，并返回拥有所有权的输出矩阵。
        /// </summary>
        /// <param name="cameraMatrix1">The first camera matrix. 第一个相机矩阵。</param>
        /// <param name="distCoeffs1">The first distortion coefficients. 第一组畸变系数。</param>
        /// <param name="cameraMatrix2">The second camera matrix. 第二个相机矩阵。</param>
        /// <param name="distCoeffs2">The second distortion coefficients. 第二组畸变系数。</param>
        /// <param name="imageSize">The original image size. 原始图像尺寸。</param>
        /// <param name="r">The rotation between cameras. 相机之间的旋转。</param>
        /// <param name="t">The translation between cameras. 相机之间的平移。</param>
        /// <returns>The owned stereo rectification result. 拥有所有权的双目校正结果。</returns>
        public static StereoRectifyResult StereoRectify(
            Mat cameraMatrix1,
            Mat distCoeffs1,
            Mat cameraMatrix2,
            Mat distCoeffs2,
            Size imageSize,
            Mat r,
            Mat t)
        {
            return StereoRectify(
                cameraMatrix1,
                distCoeffs1,
                cameraMatrix2,
                distCoeffs2,
                imageSize,
                r,
                t,
                StereoRectifyFlags.ZeroDisparity,
                -1.0,
                new Size());
        }

        /// <summary>
        /// Finds all pose solutions from 3D-2D correspondences.
        /// 根据 3D-2D 对应点查找所有位姿解。
        /// </summary>
        /// <param name="objectPoints">The input 3D object points. 输入三维物点。</param>
        /// <param name="imagePoints">The input 2D image points. 输入二维像点。</param>
        /// <param name="cameraMatrix">The camera intrinsic matrix. 相机内参矩阵。</param>
        /// <param name="distCoeffs">The distortion coefficients. 畸变系数。</param>
        /// <param name="rvecs">The output <c>N x 3</c> rotation-vector matrix. 输出 <c>N x 3</c> 旋转向量矩阵。</param>
        /// <param name="tvecs">The output <c>N x 3</c> translation-vector matrix. 输出 <c>N x 3</c> 平移向量矩阵。</param>
        /// <param name="useExtrinsicGuess">Whether to use the optional pose as an initial guess. 是否使用可选位姿作为初值。</param>
        /// <param name="flags">The PnP method. PnP 方法。</param>
        /// <param name="rvec">The optional initial rotation vector. 可选初始旋转向量。</param>
        /// <param name="tvec">The optional initial translation vector. 可选初始平移向量。</param>
        /// <param name="reprojectionError">The optional output reprojection-error matrix. 可选输出重投影误差矩阵。</param>
        /// <returns>The number of pose solutions. 位姿解数量。</returns>
        public static int SolvePnPGeneric(
            Mat objectPoints,
            Mat imagePoints,
            Mat cameraMatrix,
            Mat distCoeffs,
            Mat rvecs,
            Mat tvecs,
            bool useExtrinsicGuess = false,
            SolvePnPFlags flags = SolvePnPFlags.Iterative,
            Mat? rvec = null,
            Mat? tvec = null,
            Mat? reprojectionError = null)
        {
            ThrowIfNull(objectPoints, nameof(objectPoints));
            ThrowIfNull(imagePoints, nameof(imagePoints));
            ThrowIfNull(cameraMatrix, nameof(cameraMatrix));
            ThrowIfNull(distCoeffs, nameof(distCoeffs));
            ThrowIfNull(rvecs, nameof(rvecs));
            ThrowIfNull(tvecs, nameof(tvecs));

            NativeException.ThrowIfError(NativeMethods.Calib3DSolvePnPGeneric(
                objectPoints.NativeHandle,
                imagePoints.NativeHandle,
                cameraMatrix.NativeHandle,
                distCoeffs.NativeHandle,
                rvecs.NativeHandle,
                tvecs.NativeHandle,
                useExtrinsicGuess ? 1 : 0,
                (int)flags,
                GetNativeHandleOrZero(rvec),
                GetNativeHandleOrZero(tvec),
                GetNativeHandleOrZero(reprojectionError),
                out int solutionCount));
            return solutionCount;
        }

        /// <summary>
        /// Finds all pose solutions and returns owned output matrices.
        /// 查找所有位姿解并返回拥有所有权的输出矩阵。
        /// </summary>
        /// <param name="objectPoints">The input 3D object points. 输入三维物点。</param>
        /// <param name="imagePoints">The input 2D image points. 输入二维像点。</param>
        /// <param name="cameraMatrix">The camera intrinsic matrix. 相机内参矩阵。</param>
        /// <param name="distCoeffs">The distortion coefficients. 畸变系数。</param>
        /// <param name="useExtrinsicGuess">Whether to use the optional pose as an initial guess. 是否使用可选位姿作为初值。</param>
        /// <param name="flags">The PnP method. PnP 方法。</param>
        /// <param name="rvec">The optional initial rotation vector. 可选初始旋转向量。</param>
        /// <param name="tvec">The optional initial translation vector. 可选初始平移向量。</param>
        /// <param name="returnReprojectionError">Whether to request reprojection errors. 是否请求重投影误差。</param>
        /// <returns>The pose solution bundle. 位姿解结果包。</returns>
        public static SolvePnPGenericResult SolvePnPGeneric(
            Mat objectPoints,
            Mat imagePoints,
            Mat cameraMatrix,
            Mat distCoeffs,
            bool useExtrinsicGuess = false,
            SolvePnPFlags flags = SolvePnPFlags.Iterative,
            Mat? rvec = null,
            Mat? tvec = null,
            bool returnReprojectionError = false)
        {
            var rvecs = new Mat();
            var tvecs = new Mat();
            Mat? reprojectionError = returnReprojectionError ? new Mat() : null;
            try
            {
                int solutionCount = SolvePnPGeneric(
                    objectPoints,
                    imagePoints,
                    cameraMatrix,
                    distCoeffs,
                    rvecs,
                    tvecs,
                    useExtrinsicGuess,
                    flags,
                    rvec,
                    tvec,
                    reprojectionError);
                return new SolvePnPGenericResult(solutionCount, rvecs, tvecs, reprojectionError);
            }
            catch
            {
                rvecs.Dispose();
                tvecs.Dispose();
                if (reprojectionError != null)
                {
                    reprojectionError.Dispose();
                }

                throw;
            }
        }

        /// <summary>
        /// Refines a pose with Levenberg-Marquardt optimization.
        /// 使用 Levenberg-Marquardt 优化细化位姿。
        /// </summary>
        /// <param name="objectPoints">The input 3D object points. 输入三维物点。</param>
        /// <param name="imagePoints">The input 2D image points. 输入二维像点。</param>
        /// <param name="cameraMatrix">The camera intrinsic matrix. 相机内参矩阵。</param>
        /// <param name="distCoeffs">The distortion coefficients. 畸变系数。</param>
        /// <param name="rvec">The input-output rotation vector. 输入输出旋转向量。</param>
        /// <param name="tvec">The input-output translation vector. 输入输出平移向量。</param>
        /// <param name="criteria">The stop criteria. 停止条件。</param>
        public static void SolvePnPRefineLM(Mat objectPoints, Mat imagePoints, Mat cameraMatrix, Mat distCoeffs, Mat rvec, Mat tvec, TermCriteria? criteria = null)
        {
            ThrowIfNull(objectPoints, nameof(objectPoints));
            ThrowIfNull(imagePoints, nameof(imagePoints));
            ThrowIfNull(cameraMatrix, nameof(cameraMatrix));
            ThrowIfNull(distCoeffs, nameof(distCoeffs));
            ThrowIfNull(rvec, nameof(rvec));
            ThrowIfNull(tvec, nameof(tvec));

            TermCriteria resolved = criteria ?? new TermCriteria(TermCriteriaTypes.CountOrEps, 20, 1.1920928955078125E-7);
            NativeException.ThrowIfError(NativeMethods.Calib3DSolvePnPRefineLM(
                objectPoints.NativeHandle,
                imagePoints.NativeHandle,
                cameraMatrix.NativeHandle,
                distCoeffs.NativeHandle,
                rvec.NativeHandle,
                tvec.NativeHandle,
                (int)resolved.Type,
                resolved.MaxCount,
                resolved.Epsilon));
        }

        /// <summary>
        /// Refines a pose with virtual visual servoing optimization.
        /// 使用虚拟视觉伺服优化细化位姿。
        /// </summary>
        /// <param name="objectPoints">The input 3D object points. 输入三维物点。</param>
        /// <param name="imagePoints">The input 2D image points. 输入二维像点。</param>
        /// <param name="cameraMatrix">The camera intrinsic matrix. 相机内参矩阵。</param>
        /// <param name="distCoeffs">The distortion coefficients. 畸变系数。</param>
        /// <param name="rvec">The input-output rotation vector. 输入输出旋转向量。</param>
        /// <param name="tvec">The input-output translation vector. 输入输出平移向量。</param>
        /// <param name="criteria">The stop criteria. 停止条件。</param>
        /// <param name="vvsLambda">The VVS gain. VVS 增益。</param>
        public static void SolvePnPRefineVVS(Mat objectPoints, Mat imagePoints, Mat cameraMatrix, Mat distCoeffs, Mat rvec, Mat tvec, TermCriteria? criteria = null, double vvsLambda = 1.0)
        {
            ThrowIfNull(objectPoints, nameof(objectPoints));
            ThrowIfNull(imagePoints, nameof(imagePoints));
            ThrowIfNull(cameraMatrix, nameof(cameraMatrix));
            ThrowIfNull(distCoeffs, nameof(distCoeffs));
            ThrowIfNull(rvec, nameof(rvec));
            ThrowIfNull(tvec, nameof(tvec));

            TermCriteria resolved = criteria ?? new TermCriteria(TermCriteriaTypes.CountOrEps, 20, 1.1920928955078125E-7);
            NativeException.ThrowIfError(NativeMethods.Calib3DSolvePnPRefineVVS(
                objectPoints.NativeHandle,
                imagePoints.NativeHandle,
                cameraMatrix.NativeHandle,
                distCoeffs.NativeHandle,
                rvec.NativeHandle,
                tvec.NativeHandle,
                (int)resolved.Type,
                resolved.MaxCount,
                resolved.Epsilon,
                vvsLambda));
        }

        /// <summary>
        /// Finds chessboard corners for a calibration pattern.
        /// 查找标定棋盘格角点。
        /// </summary>
        /// <param name="image">The source image. 源图像。</param>
        /// <param name="patternSize">The number of inner corners per chessboard row and column. 棋盘格每行每列的内角点数量。</param>
        /// <param name="corners">The output corner matrix. 输出角点矩阵。</param>
        /// <param name="flags">Detection flags. 检测标志。</param>
        /// <returns><c>true</c> if the pattern was found; otherwise, <c>false</c>. 找到图案时返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        public static bool FindChessboardCorners(Mat image, Size patternSize, Mat corners, ChessboardFlags flags = ChessboardFlags.Default)
        {
            ThrowIfNull(image, nameof(image));
            ThrowIfNull(corners, nameof(corners));
            ValidatePatternSize(patternSize, nameof(patternSize));

            NativeException.ThrowIfError(NativeMethods.Calib3DFindChessboardCorners(
                image.NativeHandle,
                patternSize.Width,
                patternSize.Height,
                corners.NativeHandle,
                (int)flags,
                out int found));
            return found != 0;
        }

        /// <summary>
        /// Finds chessboard corners and returns a new corner matrix.
        /// 查找棋盘格角点并返回新的角点矩阵。
        /// </summary>
        /// <param name="image">The source image. 源图像。</param>
        /// <param name="patternSize">The number of inner corners per chessboard row and column. 棋盘格每行每列的内角点数量。</param>
        /// <param name="flags">Detection flags. 检测标志。</param>
        /// <param name="corners">The detected corners. 检测到的角点。</param>
        /// <returns><c>true</c> if the pattern was found; otherwise, <c>false</c>. 找到图案时返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        public static bool FindChessboardCorners(Mat image, Size patternSize, ChessboardFlags flags, out Mat corners)
        {
            corners = new Mat();
            try
            {
                return FindChessboardCorners(image, patternSize, corners, flags);
            }
            catch
            {
                corners.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Quickly checks whether an image contains a chessboard of the requested size.
        /// 快速检查图像是否包含指定尺寸的棋盘格。
        /// </summary>
        /// <param name="image">The source image. 源图像。</param>
        /// <param name="patternSize">The number of inner corners per chessboard row and column. 棋盘格每行每列的内角点数量。</param>
        /// <returns><c>true</c> if a chessboard may be present; otherwise, <c>false</c>. 可能存在棋盘格时返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        public static bool CheckChessboard(Mat image, Size patternSize)
        {
            ThrowIfNull(image, nameof(image));
            ValidatePatternSize(patternSize, nameof(patternSize));

            NativeException.ThrowIfError(NativeMethods.Calib3DCheckChessboard(
                image.NativeHandle,
                patternSize.Width,
                patternSize.Height,
                out int found));
            return found != 0;
        }

        /// <summary>Finds chessboard corners using the sector-based detector.</summary>
        public static bool FindChessboardCornersSB(Mat image, Size patternSize, Mat corners, ChessboardFlags flags = (ChessboardFlags)0)
        {
            ThrowIfNull(image, nameof(image));
            ThrowIfNull(corners, nameof(corners));
            ValidatePatternSize(patternSize, nameof(patternSize));
            NativeException.ThrowIfError(NativeMethods.Calib3DFindChessboardCornersSB(
                image.NativeHandle, patternSize.Width, patternSize.Height, corners.NativeHandle, (int)flags, out int found));
            return found != 0;
        }

        /// <summary>Finds chessboard corners and writes sector-detector metadata.</summary>
        public static bool FindChessboardCornersSB(Mat image, Size patternSize, Mat corners, Mat meta, ChessboardFlags flags = (ChessboardFlags)0)
        {
            ThrowIfNull(image, nameof(image));
            ThrowIfNull(corners, nameof(corners));
            ThrowIfNull(meta, nameof(meta));
            ValidatePatternSize(patternSize, nameof(patternSize));
            NativeException.ThrowIfError(NativeMethods.Calib3DFindChessboardCornersSBWithMeta(
                image.NativeHandle, patternSize.Width, patternSize.Height, corners.NativeHandle, (int)flags, meta.NativeHandle, out int found));
            return found != 0;
        }

        /// <summary>Estimates chessboard edge sharpness and optionally writes per-edge profiles.</summary>
        public static Scalar EstimateChessboardSharpness(Mat image, Size patternSize, Mat corners, float riseDistance = 0.8F, bool vertical = false, Mat? sharpness = null)
        {
            ThrowIfNull(image, nameof(image));
            ThrowIfNull(corners, nameof(corners));
            ValidatePatternSize(patternSize, nameof(patternSize));
            NativeException.ThrowIfError(NativeMethods.Calib3DEstimateChessboardSharpness(
                image.NativeHandle, patternSize.Width, patternSize.Height, corners.NativeHandle,
                riseDistance, vertical ? 1 : 0, sharpness?.NativeHandle ?? IntPtr.Zero,
                out double value0, out double value1, out double value2, out double value3));
            return new Scalar(value0, value1, value2, value3);
        }

        /// <summary>Refines a four-quad chessboard corner set in place.</summary>
        public static bool Find4QuadCornerSubpix(Mat image, Mat corners, Size regionSize)
        {
            ThrowIfNull(image, nameof(image));
            ThrowIfNull(corners, nameof(corners));
            ValidatePatternSize(regionSize, nameof(regionSize));
            NativeException.ThrowIfError(NativeMethods.Calib3DFind4QuadCornerSubpix(
                image.NativeHandle, corners.NativeHandle, regionSize.Width, regionSize.Height, out int found));
            return found != 0;
        }

        /// <summary>
        /// Finds centers of a circles-grid calibration pattern.
        /// 查找圆点阵列标定图案的圆心。
        /// </summary>
        /// <param name="image">The source image. 源图像。</param>
        /// <param name="patternSize">The pattern size. 图案尺寸。</param>
        /// <param name="centers">The output center matrix. 输出圆心矩阵。</param>
        /// <param name="flags">Detection flags. 检测标志。</param>
        /// <returns><c>true</c> if the pattern was found; otherwise, <c>false</c>. 找到图案时返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        public static bool FindCirclesGrid(Mat image, Size patternSize, Mat centers, CirclesGridFlags flags = CirclesGridFlags.SymmetricGrid)
        {
            ThrowIfNull(image, nameof(image));
            ThrowIfNull(centers, nameof(centers));
            ValidatePatternSize(patternSize, nameof(patternSize));

            NativeException.ThrowIfError(NativeMethods.Calib3DFindCirclesGrid(
                image.NativeHandle,
                patternSize.Width,
                patternSize.Height,
                centers.NativeHandle,
                (int)flags,
                out int found));
            return found != 0;
        }

        /// <summary>
        /// Finds circle-grid centers and returns a new center matrix.
        /// 查找圆点阵列圆心并返回新的圆心矩阵。
        /// </summary>
        /// <param name="image">The source image. 源图像。</param>
        /// <param name="patternSize">The pattern size. 图案尺寸。</param>
        /// <param name="flags">Detection flags. 检测标志。</param>
        /// <param name="centers">The detected centers. 检测到的圆心。</param>
        /// <returns><c>true</c> if the pattern was found; otherwise, <c>false</c>. 找到图案时返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        public static bool FindCirclesGrid(Mat image, Size patternSize, CirclesGridFlags flags, out Mat centers)
        {
            centers = new Mat();
            try
            {
                return FindCirclesGrid(image, patternSize, centers, flags);
            }
            catch
            {
                centers.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Draws detected chessboard or circles-grid corners.
        /// 绘制检测到的棋盘格或圆点阵列角点。
        /// </summary>
        /// <param name="image">The image to draw on. 要绘制的图像。</param>
        /// <param name="patternSize">The pattern size. 图案尺寸。</param>
        /// <param name="corners">The detected corners or centers. 检测到的角点或圆心。</param>
        /// <param name="patternWasFound">Whether the pattern was found. 是否找到图案。</param>
        public static void DrawChessboardCorners(Mat image, Size patternSize, Mat corners, bool patternWasFound)
        {
            ThrowIfNull(image, nameof(image));
            ThrowIfNull(corners, nameof(corners));
            ValidatePatternSize(patternSize, nameof(patternSize));

            NativeException.ThrowIfError(NativeMethods.Calib3DDrawChessboardCorners(
                image.NativeHandle,
                patternSize.Width,
                patternSize.Height,
                corners.NativeHandle,
                patternWasFound ? 1 : 0));
        }

        /// <summary>
        /// Computes an optimal new camera matrix for undistortion.
        /// 为去畸变计算最优新相机矩阵。
        /// </summary>
        /// <param name="cameraMatrix">The original camera matrix. 原始相机矩阵。</param>
        /// <param name="distCoeffs">The distortion coefficients. 畸变系数。</param>
        /// <param name="imageSize">The original image size. 原始图像尺寸。</param>
        /// <param name="alpha">The free scaling parameter. 自由缩放参数。</param>
        /// <param name="newImageSize">The new image size. 新图像尺寸。</param>
        /// <param name="centerPrincipalPoint">Whether to force the principal point to the image center. 是否强制主点位于图像中心。</param>
        /// <returns>The new camera matrix and valid-pixel ROI. 新相机矩阵和有效像素 ROI。</returns>
        public static OptimalNewCameraMatrixResult GetOptimalNewCameraMatrix(Mat cameraMatrix, Mat distCoeffs, Size imageSize, double alpha, Size? newImageSize = null, bool centerPrincipalPoint = false)
        {
            ThrowIfNull(cameraMatrix, nameof(cameraMatrix));
            ThrowIfNull(distCoeffs, nameof(distCoeffs));

            Size resolvedNewSize = newImageSize ?? new Size();
            NativeException.ThrowIfError(NativeMethods.Calib3DGetOptimalNewCameraMatrix(
                cameraMatrix.NativeHandle,
                distCoeffs.NativeHandle,
                imageSize.Width,
                imageSize.Height,
                alpha,
                resolvedNewSize.Width,
                resolvedNewSize.Height,
                centerPrincipalPoint ? 1 : 0,
                out int roiX,
                out int roiY,
                out int roiWidth,
                out int roiHeight,
                out IntPtr newCameraMatrix));
            return new OptimalNewCameraMatrixResult(new Mat(newCameraMatrix), new Rect(roiX, roiY, roiWidth, roiHeight));
        }

        /// <summary>
        /// Computes useful camera values from an intrinsic camera matrix.
        /// 从相机内参矩阵计算常用相机参数。
        /// </summary>
        /// <param name="cameraMatrix">The camera intrinsic matrix. 相机内参矩阵。</param>
        /// <param name="imageSize">The image size in pixels. 图像像素尺寸。</param>
        /// <param name="apertureWidth">The physical aperture width. 物理孔径宽度。</param>
        /// <param name="apertureHeight">The physical aperture height. 物理孔径高度。</param>
        /// <returns>The computed camera values. 计算出的相机参数。</returns>
        public static CalibrationMatrixValuesResult CalibrationMatrixValues(Mat cameraMatrix, Size imageSize, double apertureWidth, double apertureHeight)
        {
            ThrowIfNull(cameraMatrix, nameof(cameraMatrix));

            NativeException.ThrowIfError(NativeMethods.Calib3DCalibrationMatrixValues(
                cameraMatrix.NativeHandle,
                imageSize.Width,
                imageSize.Height,
                apertureWidth,
                apertureHeight,
                out double fovX,
                out double fovY,
                out double focalLength,
                out double principalPointX,
                out double principalPointY,
                out double aspectRatio));

            return new CalibrationMatrixValuesResult(fovX, fovY, focalLength, new Point2d(principalPointX, principalPointY), aspectRatio);
        }

        /// <summary>
        /// Computes rectification transforms for an uncalibrated stereo camera pair.
        /// 为未标定双目相机对计算校正变换。
        /// </summary>
        /// <param name="points1">The first point set. 第一组点。</param>
        /// <param name="points2">The second point set. 第二组点。</param>
        /// <param name="fundamental">The fundamental matrix. 基础矩阵。</param>
        /// <param name="imageSize">The image size. 图像尺寸。</param>
        /// <param name="h1">The first output rectification transform. 第一个输出校正变换。</param>
        /// <param name="h2">The second output rectification transform. 第二个输出校正变换。</param>
        /// <param name="threshold">The optional outlier rejection threshold. 可选外点剔除阈值。</param>
        /// <returns><c>true</c> if rectification succeeded; otherwise, <c>false</c>. 校正成功时返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        public static bool StereoRectifyUncalibrated(Mat points1, Mat points2, Mat fundamental, Size imageSize, Mat h1, Mat h2, double threshold = 5.0)
        {
            ThrowIfNull(points1, nameof(points1));
            ThrowIfNull(points2, nameof(points2));
            ThrowIfNull(fundamental, nameof(fundamental));
            ThrowIfNull(h1, nameof(h1));
            ThrowIfNull(h2, nameof(h2));
            ValidatePoint2fMatrix(points1, nameof(points1));
            ValidatePoint2fMatrix(points2, nameof(points2));
            ValidateMatchingPointMatrixCount(points1, nameof(points1), points2, nameof(points2));
            ValidateCameraUtilityRectification(fundamental, nameof(fundamental));
            ValidatePositiveSize(imageSize, nameof(imageSize));
            ValidateDistinctOutputPair(h1, nameof(h1), h2, nameof(h2));

            NativeException.ThrowIfError(NativeMethods.Calib3DStereoRectifyUncalibrated(
                points1.NativeHandle,
                points2.NativeHandle,
                fundamental.NativeHandle,
                imageSize.Width,
                imageSize.Height,
                h1.NativeHandle,
                h2.NativeHandle,
                threshold,
                out int success));
            return success != 0;
        }

        /// <summary>
        /// Computes uncalibrated stereo rectification transforms and returns owned output matrices.
        /// 计算未标定双目校正变换，并返回拥有所有权的输出矩阵。
        /// </summary>
        /// <param name="points1">The first point set. 第一组点。</param>
        /// <param name="points2">The second point set. 第二组点。</param>
        /// <param name="fundamental">The fundamental matrix. 基础矩阵。</param>
        /// <param name="imageSize">The image size. 图像尺寸。</param>
        /// <param name="threshold">The optional outlier rejection threshold. 可选外点剔除阈值。</param>
        /// <returns>The owned uncalibrated rectification result. 拥有所有权的未标定校正结果。</returns>
        public static StereoRectifyUncalibratedResult StereoRectifyUncalibrated(
            Mat points1,
            Mat points2,
            Mat fundamental,
            Size imageSize,
            double threshold = 5.0)
        {
            var h1 = new Mat();
            var h2 = new Mat();
            try
            {
                bool success = StereoRectifyUncalibrated(
                    points1,
                    points2,
                    fundamental,
                    imageSize,
                    h1,
                    h2,
                    threshold);
                return new StereoRectifyUncalibratedResult(success, h1, h2);
            }
            catch
            {
                h1.Dispose();
                h2.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Finds camera intrinsics and extrinsics from grouped 3D object points and 2D image points.
        /// 根据分组的三维物点和二维像点估计相机内参和外参。
        /// </summary>
        /// <param name="objectPoints">The calibration target points for each view. 每个视图中的标定目标物点。</param>
        /// <param name="imagePoints">The detected image points for each view. 每个视图中检测到的像点。</param>
        /// <param name="imageSize">The image size used for calibration. 用于标定的图像尺寸。</param>
        /// <param name="cameraMatrix">The input-output camera matrix. 输入输出相机矩阵。</param>
        /// <param name="distCoeffs">The input-output distortion coefficients. 输入输出畸变系数。</param>
        /// <param name="rvecs">The output <c>N x 3</c> packed rotation-vector matrix. 输出 <c>N x 3</c> 打包旋转向量矩阵。</param>
        /// <param name="tvecs">The output <c>N x 3</c> packed translation-vector matrix. 输出 <c>N x 3</c> 打包平移向量矩阵。</param>
        /// <param name="flags">Calibration flags. 标定标志。</param>
        /// <param name="criteria">The stop criteria. 停止条件。</param>
        /// <returns>The RMS reprojection error. RMS 重投影误差。</returns>
        public static double CalibrateCamera(
            Point3f[][] objectPoints,
            Point2f[][] imagePoints,
            Size imageSize,
            Mat cameraMatrix,
            Mat distCoeffs,
            Mat rvecs,
            Mat tvecs,
            CalibrationFlags flags = CalibrationFlags.None,
            TermCriteria? criteria = null)
        {
            ThrowIfNull(cameraMatrix, nameof(cameraMatrix));
            ThrowIfNull(distCoeffs, nameof(distCoeffs));
            ThrowIfNull(rvecs, nameof(rvecs));
            ThrowIfNull(tvecs, nameof(tvecs));
            ValidatePositiveSize(imageSize, nameof(imageSize));

            PrepareCalibrationPointGroups(
                objectPoints,
                imagePoints,
                nameof(objectPoints),
                nameof(imagePoints),
                out int[] objectOffsets,
                out NativeMethods.Calib3DPoint3fNative[] nativeObjectPoints,
                out int[] imageOffsets,
                out NativeMethods.Calib3DPoint2fNative[] nativeImagePoints);
            TermCriteria resolved = criteria ?? DefaultCalibrationCriteria;

            fixed (int* objectOffsetsPtr = objectOffsets)
            fixed (NativeMethods.Calib3DPoint3fNative* objectPointsPtr = nativeObjectPoints)
            fixed (int* imageOffsetsPtr = imageOffsets)
            fixed (NativeMethods.Calib3DPoint2fNative* imagePointsPtr = nativeImagePoints)
            {
                NativeException.ThrowIfError(NativeMethods.Calib3DCalibrateCamera(
                    objectOffsetsPtr,
                    objectPoints.Length,
                    objectPointsPtr,
                    nativeObjectPoints.Length,
                    imageOffsetsPtr,
                    imagePoints.Length,
                    imagePointsPtr,
                    nativeImagePoints.Length,
                    imageSize.Width,
                    imageSize.Height,
                    cameraMatrix.NativeHandle,
                    distCoeffs.NativeHandle,
                    rvecs.NativeHandle,
                    tvecs.NativeHandle,
                    (int)flags,
                    (int)resolved.Type,
                    resolved.MaxCount,
                    resolved.Epsilon,
                    out double reprojectionError));
                return reprojectionError;
            }
        }

        /// <summary>
        /// Calibrates a camera and returns owned output matrices.
        /// 执行相机标定并返回拥有所有权的输出矩阵。
        /// </summary>
        /// <param name="objectPoints">The calibration target points for each view. 每个视图中的标定目标物点。</param>
        /// <param name="imagePoints">The detected image points for each view. 每个视图中检测到的像点。</param>
        /// <param name="imageSize">The image size used for calibration. 用于标定的图像尺寸。</param>
        /// <param name="flags">Calibration flags. 标定标志。</param>
        /// <param name="criteria">The stop criteria. 停止条件。</param>
        /// <returns>The calibration result bundle. 标定结果包。</returns>
        public static CalibrationResult CalibrateCamera(
            Point3f[][] objectPoints,
            Point2f[][] imagePoints,
            Size imageSize,
            CalibrationFlags flags = CalibrationFlags.None,
            TermCriteria? criteria = null)
        {
            var cameraMatrix = new Mat();
            var distCoeffs = new Mat();
            var rvecs = new Mat();
            var tvecs = new Mat();
            try
            {
                double reprojectionError = CalibrateCamera(
                    objectPoints,
                    imagePoints,
                    imageSize,
                    cameraMatrix,
                    distCoeffs,
                    rvecs,
                    tvecs,
                    flags,
                    criteria);
                return new CalibrationResult(reprojectionError, cameraMatrix, distCoeffs, rvecs, tvecs);
            }
            catch
            {
                cameraMatrix.Dispose();
                distCoeffs.Dispose();
                rvecs.Dispose();
                tvecs.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Finds camera intrinsics, extrinsics, and uncertainty estimates from calibration views.
        /// 根据多个标定视图估计相机内外参及不确定度。
        /// </summary>
        /// <param name="objectPoints">The calibration target points for each view. 每个视图中的标定目标物点。</param>
        /// <param name="imagePoints">The detected image points for each view. 每个视图中检测到的像点。</param>
        /// <param name="imageSize">The image size used for calibration. 用于标定的图像尺寸。</param>
        /// <param name="cameraMatrix">The input-output camera matrix. 输入输出相机矩阵。</param>
        /// <param name="distCoeffs">The input-output distortion coefficients. 输入输出畸变系数。</param>
        /// <param name="rvecs">The output <c>N x 3</c> packed rotation-vector matrix. 输出 <c>N x 3</c> 打包旋转向量矩阵。</param>
        /// <param name="tvecs">The output <c>N x 3</c> packed translation-vector matrix. 输出 <c>N x 3</c> 打包平移向量矩阵。</param>
        /// <param name="stdDeviationsIntrinsics">The intrinsic parameter standard deviations. 内参标准差。</param>
        /// <param name="stdDeviationsExtrinsics">The extrinsic parameter standard deviations. 外参标准差。</param>
        /// <param name="perViewErrors">The per-view reprojection errors. 每个视图的重投影误差。</param>
        /// <param name="flags">Calibration flags. 标定标志。</param>
        /// <param name="criteria">The stop criteria. 停止条件。</param>
        /// <returns>The RMS reprojection error. RMS 重投影误差。</returns>
        public static double CalibrateCameraExtended(
            Point3f[][] objectPoints,
            Point2f[][] imagePoints,
            Size imageSize,
            Mat cameraMatrix,
            Mat distCoeffs,
            Mat rvecs,
            Mat tvecs,
            Mat stdDeviationsIntrinsics,
            Mat stdDeviationsExtrinsics,
            Mat perViewErrors,
            CalibrationFlags flags = CalibrationFlags.None,
            TermCriteria? criteria = null)
        {
            ThrowIfNull(cameraMatrix, nameof(cameraMatrix));
            ThrowIfNull(distCoeffs, nameof(distCoeffs));
            ThrowIfNull(rvecs, nameof(rvecs));
            ThrowIfNull(tvecs, nameof(tvecs));
            ThrowIfNull(stdDeviationsIntrinsics, nameof(stdDeviationsIntrinsics));
            ThrowIfNull(stdDeviationsExtrinsics, nameof(stdDeviationsExtrinsics));
            ThrowIfNull(perViewErrors, nameof(perViewErrors));
            ValidatePositiveSize(imageSize, nameof(imageSize));

            PrepareCalibrationPointGroups(
                objectPoints,
                imagePoints,
                nameof(objectPoints),
                nameof(imagePoints),
                out int[] objectOffsets,
                out NativeMethods.Calib3DPoint3fNative[] nativeObjectPoints,
                out int[] imageOffsets,
                out NativeMethods.Calib3DPoint2fNative[] nativeImagePoints);
            TermCriteria resolved = criteria ?? DefaultCalibrationCriteria;

            fixed (int* objectOffsetsPtr = objectOffsets)
            fixed (NativeMethods.Calib3DPoint3fNative* objectPointsPtr = nativeObjectPoints)
            fixed (int* imageOffsetsPtr = imageOffsets)
            fixed (NativeMethods.Calib3DPoint2fNative* imagePointsPtr = nativeImagePoints)
            {
                NativeException.ThrowIfError(NativeMethods.Calib3DCalibrateCameraExtended(
                    objectOffsetsPtr,
                    objectPoints.Length,
                    objectPointsPtr,
                    nativeObjectPoints.Length,
                    imageOffsetsPtr,
                    imagePoints.Length,
                    imagePointsPtr,
                    nativeImagePoints.Length,
                    imageSize.Width,
                    imageSize.Height,
                    cameraMatrix.NativeHandle,
                    distCoeffs.NativeHandle,
                    rvecs.NativeHandle,
                    tvecs.NativeHandle,
                    stdDeviationsIntrinsics.NativeHandle,
                    stdDeviationsExtrinsics.NativeHandle,
                    perViewErrors.NativeHandle,
                    (int)flags,
                    (int)resolved.Type,
                    resolved.MaxCount,
                    resolved.Epsilon,
                    out double reprojectionError));
                return reprojectionError;
            }
        }

        /// <summary>
        /// Calibrates a camera with uncertainty outputs and returns owned output matrices.
        /// 执行带不确定度输出的相机标定，并返回拥有所有权的输出矩阵。
        /// </summary>
        /// <param name="objectPoints">The calibration target points for each view. 每个视图中的标定目标物点。</param>
        /// <param name="imagePoints">The detected image points for each view. 每个视图中检测到的像点。</param>
        /// <param name="imageSize">The image size used for calibration. 用于标定的图像尺寸。</param>
        /// <param name="flags">Calibration flags. 标定标志。</param>
        /// <param name="criteria">The stop criteria. 停止条件。</param>
        /// <returns>The extended calibration result bundle. 扩展标定结果包。</returns>
        public static CalibrationExtendedResult CalibrateCameraExtended(
            Point3f[][] objectPoints,
            Point2f[][] imagePoints,
            Size imageSize,
            CalibrationFlags flags = CalibrationFlags.None,
            TermCriteria? criteria = null)
        {
            var cameraMatrix = new Mat();
            var distCoeffs = new Mat();
            var rvecs = new Mat();
            var tvecs = new Mat();
            var stdDeviationsIntrinsics = new Mat();
            var stdDeviationsExtrinsics = new Mat();
            var perViewErrors = new Mat();
            try
            {
                double reprojectionError = CalibrateCameraExtended(
                    objectPoints,
                    imagePoints,
                    imageSize,
                    cameraMatrix,
                    distCoeffs,
                    rvecs,
                    tvecs,
                    stdDeviationsIntrinsics,
                    stdDeviationsExtrinsics,
                    perViewErrors,
                    flags,
                    criteria);
                var calibration = new CalibrationResult(reprojectionError, cameraMatrix, distCoeffs, rvecs, tvecs);
                return new CalibrationExtendedResult(calibration, stdDeviationsIntrinsics, stdDeviationsExtrinsics, perViewErrors);
            }
            catch
            {
                cameraMatrix.Dispose();
                distCoeffs.Dispose();
                rvecs.Dispose();
                tvecs.Dispose();
                stdDeviationsIntrinsics.Dispose();
                stdDeviationsExtrinsics.Dispose();
                perViewErrors.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Calibrates a stereo camera pair from grouped object and image points.
        /// 根据分组物点和左右图像点标定双目相机。
        /// </summary>
        /// <param name="objectPoints">The calibration target points for each view. 每个视图中的标定目标物点。</param>
        /// <param name="imagePoints1">The first-camera image points for each view. 第一相机每个视图中的像点。</param>
        /// <param name="imagePoints2">The second-camera image points for each view. 第二相机每个视图中的像点。</param>
        /// <param name="cameraMatrix1">The first input-output camera matrix. 第一个输入输出相机矩阵。</param>
        /// <param name="distCoeffs1">The first input-output distortion coefficients. 第一组输入输出畸变系数。</param>
        /// <param name="cameraMatrix2">The second input-output camera matrix. 第二个输入输出相机矩阵。</param>
        /// <param name="distCoeffs2">The second input-output distortion coefficients. 第二组输入输出畸变系数。</param>
        /// <param name="imageSize">The image size used for calibration. 用于标定的图像尺寸。</param>
        /// <param name="r">The output rotation between cameras. 输出相机之间的旋转。</param>
        /// <param name="t">The output translation between cameras. 输出相机之间的平移。</param>
        /// <param name="e">The output essential matrix. 输出本质矩阵。</param>
        /// <param name="f">The output fundamental matrix. 输出基础矩阵。</param>
        /// <param name="flags">Calibration flags. 标定标志。</param>
        /// <param name="criteria">The stop criteria. 停止条件。</param>
        /// <returns>The RMS reprojection error. RMS 重投影误差。</returns>
        public static double StereoCalibrate(
            Point3f[][] objectPoints,
            Point2f[][] imagePoints1,
            Point2f[][] imagePoints2,
            Mat cameraMatrix1,
            Mat distCoeffs1,
            Mat cameraMatrix2,
            Mat distCoeffs2,
            Size imageSize,
            Mat r,
            Mat t,
            Mat e,
            Mat f,
            CalibrationFlags flags = CalibrationFlags.FixIntrinsic,
            TermCriteria? criteria = null)
        {
            ThrowIfNull(cameraMatrix1, nameof(cameraMatrix1));
            ThrowIfNull(distCoeffs1, nameof(distCoeffs1));
            ThrowIfNull(cameraMatrix2, nameof(cameraMatrix2));
            ThrowIfNull(distCoeffs2, nameof(distCoeffs2));
            ThrowIfNull(r, nameof(r));
            ThrowIfNull(t, nameof(t));
            ThrowIfNull(e, nameof(e));
            ThrowIfNull(f, nameof(f));
            ValidatePositiveSize(imageSize, nameof(imageSize));

            PrepareStereoCalibrationPointGroups(
                objectPoints,
                imagePoints1,
                imagePoints2,
                out int[] objectOffsets,
                out NativeMethods.Calib3DPoint3fNative[] nativeObjectPoints,
                out int[] image1Offsets,
                out NativeMethods.Calib3DPoint2fNative[] nativeImagePoints1,
                out int[] image2Offsets,
                out NativeMethods.Calib3DPoint2fNative[] nativeImagePoints2);
            TermCriteria resolved = criteria ?? DefaultStereoCalibrationCriteria;

            fixed (int* objectOffsetsPtr = objectOffsets)
            fixed (NativeMethods.Calib3DPoint3fNative* objectPointsPtr = nativeObjectPoints)
            fixed (int* image1OffsetsPtr = image1Offsets)
            fixed (NativeMethods.Calib3DPoint2fNative* imagePoints1Ptr = nativeImagePoints1)
            fixed (int* image2OffsetsPtr = image2Offsets)
            fixed (NativeMethods.Calib3DPoint2fNative* imagePoints2Ptr = nativeImagePoints2)
            {
                NativeException.ThrowIfError(NativeMethods.Calib3DStereoCalibrate(
                    objectOffsetsPtr,
                    objectPoints.Length,
                    objectPointsPtr,
                    nativeObjectPoints.Length,
                    image1OffsetsPtr,
                    imagePoints1.Length,
                    imagePoints1Ptr,
                    nativeImagePoints1.Length,
                    image2OffsetsPtr,
                    imagePoints2.Length,
                    imagePoints2Ptr,
                    nativeImagePoints2.Length,
                    cameraMatrix1.NativeHandle,
                    distCoeffs1.NativeHandle,
                    cameraMatrix2.NativeHandle,
                    distCoeffs2.NativeHandle,
                    imageSize.Width,
                    imageSize.Height,
                    r.NativeHandle,
                    t.NativeHandle,
                    e.NativeHandle,
                    f.NativeHandle,
                    (int)flags,
                    (int)resolved.Type,
                    resolved.MaxCount,
                    resolved.Epsilon,
                    out double reprojectionError));
                return reprojectionError;
            }
        }

        /// <summary>
        /// Calibrates a stereo camera pair and returns owned output matrices.
        /// 执行双目标定并返回拥有所有权的输出矩阵。
        /// </summary>
        /// <param name="objectPoints">The calibration target points for each view. 每个视图中的标定目标物点。</param>
        /// <param name="imagePoints1">The first-camera image points for each view. 第一相机每个视图中的像点。</param>
        /// <param name="imagePoints2">The second-camera image points for each view. 第二相机每个视图中的像点。</param>
        /// <param name="imageSize">The image size used for calibration. 用于标定的图像尺寸。</param>
        /// <param name="flags">Calibration flags. 标定标志。</param>
        /// <param name="criteria">The stop criteria. 停止条件。</param>
        /// <returns>The stereo calibration result bundle. 双目标定结果包。</returns>
        public static StereoCalibrationResult StereoCalibrate(
            Point3f[][] objectPoints,
            Point2f[][] imagePoints1,
            Point2f[][] imagePoints2,
            Size imageSize,
            CalibrationFlags flags = CalibrationFlags.FixIntrinsic,
            TermCriteria? criteria = null)
        {
            var cameraMatrix1 = new Mat();
            var distCoeffs1 = new Mat();
            var cameraMatrix2 = new Mat();
            var distCoeffs2 = new Mat();
            var r = new Mat();
            var t = new Mat();
            var e = new Mat();
            var f = new Mat();
            try
            {
                double reprojectionError = StereoCalibrate(
                    objectPoints,
                    imagePoints1,
                    imagePoints2,
                    cameraMatrix1,
                    distCoeffs1,
                    cameraMatrix2,
                    distCoeffs2,
                    imageSize,
                    r,
                    t,
                    e,
                    f,
                    flags,
                    criteria);
                return new StereoCalibrationResult(reprojectionError, cameraMatrix1, distCoeffs1, cameraMatrix2, distCoeffs2, r, t, e, f);
            }
            catch
            {
                cameraMatrix1.Dispose();
                distCoeffs1.Dispose();
                cameraMatrix2.Dispose();
                distCoeffs2.Dispose();
                r.Dispose();
                t.Dispose();
                e.Dispose();
                f.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Calibrates a stereo camera pair and returns per-view pose and error outputs.
        /// 标定双目相机，并输出每个视图的位姿和误差。
        /// </summary>
        /// <param name="objectPoints">The calibration target points for each view. 每个视图中的标定目标物点。</param>
        /// <param name="imagePoints1">The first-camera image points for each view. 第一相机每个视图中的像点。</param>
        /// <param name="imagePoints2">The second-camera image points for each view. 第二相机每个视图中的像点。</param>
        /// <param name="cameraMatrix1">The first input-output camera matrix. 第一个输入输出相机矩阵。</param>
        /// <param name="distCoeffs1">The first input-output distortion coefficients. 第一组输入输出畸变系数。</param>
        /// <param name="cameraMatrix2">The second input-output camera matrix. 第二个输入输出相机矩阵。</param>
        /// <param name="distCoeffs2">The second input-output distortion coefficients. 第二组输入输出畸变系数。</param>
        /// <param name="imageSize">The image size used for calibration. 用于标定的图像尺寸。</param>
        /// <param name="r">The output rotation between cameras. 输出相机之间的旋转。</param>
        /// <param name="t">The output translation between cameras. 输出相机之间的平移。</param>
        /// <param name="e">The output essential matrix. 输出本质矩阵。</param>
        /// <param name="f">The output fundamental matrix. 输出基础矩阵。</param>
        /// <param name="rvecs">The output packed per-view rotation vectors. 输出打包的每视图旋转向量。</param>
        /// <param name="tvecs">The output packed per-view translation vectors. 输出打包的每视图平移向量。</param>
        /// <param name="perViewErrors">The per-view reprojection errors. 每个视图的重投影误差。</param>
        /// <param name="flags">Calibration flags. 标定标志。</param>
        /// <param name="criteria">The stop criteria. 停止条件。</param>
        /// <returns>The RMS reprojection error. RMS 重投影误差。</returns>
        public static double StereoCalibrateExtended(
            Point3f[][] objectPoints,
            Point2f[][] imagePoints1,
            Point2f[][] imagePoints2,
            Mat cameraMatrix1,
            Mat distCoeffs1,
            Mat cameraMatrix2,
            Mat distCoeffs2,
            Size imageSize,
            Mat r,
            Mat t,
            Mat e,
            Mat f,
            Mat rvecs,
            Mat tvecs,
            Mat perViewErrors,
            CalibrationFlags flags = CalibrationFlags.FixIntrinsic,
            TermCriteria? criteria = null)
        {
            ThrowIfNull(cameraMatrix1, nameof(cameraMatrix1));
            ThrowIfNull(distCoeffs1, nameof(distCoeffs1));
            ThrowIfNull(cameraMatrix2, nameof(cameraMatrix2));
            ThrowIfNull(distCoeffs2, nameof(distCoeffs2));
            ThrowIfNull(r, nameof(r));
            ThrowIfNull(t, nameof(t));
            ThrowIfNull(e, nameof(e));
            ThrowIfNull(f, nameof(f));
            ThrowIfNull(rvecs, nameof(rvecs));
            ThrowIfNull(tvecs, nameof(tvecs));
            ThrowIfNull(perViewErrors, nameof(perViewErrors));
            ValidatePositiveSize(imageSize, nameof(imageSize));

            PrepareStereoCalibrationPointGroups(
                objectPoints,
                imagePoints1,
                imagePoints2,
                out int[] objectOffsets,
                out NativeMethods.Calib3DPoint3fNative[] nativeObjectPoints,
                out int[] image1Offsets,
                out NativeMethods.Calib3DPoint2fNative[] nativeImagePoints1,
                out int[] image2Offsets,
                out NativeMethods.Calib3DPoint2fNative[] nativeImagePoints2);
            TermCriteria resolved = criteria ?? DefaultStereoCalibrationCriteria;

            fixed (int* objectOffsetsPtr = objectOffsets)
            fixed (NativeMethods.Calib3DPoint3fNative* objectPointsPtr = nativeObjectPoints)
            fixed (int* image1OffsetsPtr = image1Offsets)
            fixed (NativeMethods.Calib3DPoint2fNative* imagePoints1Ptr = nativeImagePoints1)
            fixed (int* image2OffsetsPtr = image2Offsets)
            fixed (NativeMethods.Calib3DPoint2fNative* imagePoints2Ptr = nativeImagePoints2)
            {
                NativeException.ThrowIfError(NativeMethods.Calib3DStereoCalibrateExtended(
                    objectOffsetsPtr,
                    objectPoints.Length,
                    objectPointsPtr,
                    nativeObjectPoints.Length,
                    image1OffsetsPtr,
                    imagePoints1.Length,
                    imagePoints1Ptr,
                    nativeImagePoints1.Length,
                    image2OffsetsPtr,
                    imagePoints2.Length,
                    imagePoints2Ptr,
                    nativeImagePoints2.Length,
                    cameraMatrix1.NativeHandle,
                    distCoeffs1.NativeHandle,
                    cameraMatrix2.NativeHandle,
                    distCoeffs2.NativeHandle,
                    imageSize.Width,
                    imageSize.Height,
                    r.NativeHandle,
                    t.NativeHandle,
                    e.NativeHandle,
                    f.NativeHandle,
                    rvecs.NativeHandle,
                    tvecs.NativeHandle,
                    perViewErrors.NativeHandle,
                    (int)flags,
                    (int)resolved.Type,
                    resolved.MaxCount,
                    resolved.Epsilon,
                    out double reprojectionError));
                return reprojectionError;
            }
        }

        /// <summary>
        /// Calibrates a stereo camera pair with extended outputs and returns owned output matrices.
        /// 执行带扩展输出的双目标定，并返回拥有所有权的输出矩阵。
        /// </summary>
        /// <param name="objectPoints">The calibration target points for each view. 每个视图中的标定目标物点。</param>
        /// <param name="imagePoints1">The first-camera image points for each view. 第一相机每个视图中的像点。</param>
        /// <param name="imagePoints2">The second-camera image points for each view. 第二相机每个视图中的像点。</param>
        /// <param name="imageSize">The image size used for calibration. 用于标定的图像尺寸。</param>
        /// <param name="flags">Calibration flags. 标定标志。</param>
        /// <param name="criteria">The stop criteria. 停止条件。</param>
        /// <returns>The extended stereo calibration result bundle. 扩展双目标定结果包。</returns>
        public static StereoCalibrationExtendedResult StereoCalibrateExtended(
            Point3f[][] objectPoints,
            Point2f[][] imagePoints1,
            Point2f[][] imagePoints2,
            Size imageSize,
            CalibrationFlags flags = CalibrationFlags.FixIntrinsic,
            TermCriteria? criteria = null)
        {
            var cameraMatrix1 = new Mat();
            var distCoeffs1 = new Mat();
            var cameraMatrix2 = new Mat();
            var distCoeffs2 = new Mat();
            var r = new Mat();
            var t = new Mat();
            var e = new Mat();
            var f = new Mat();
            var rvecs = new Mat();
            var tvecs = new Mat();
            var perViewErrors = new Mat();
            try
            {
                double reprojectionError = StereoCalibrateExtended(
                    objectPoints,
                    imagePoints1,
                    imagePoints2,
                    cameraMatrix1,
                    distCoeffs1,
                    cameraMatrix2,
                    distCoeffs2,
                    imageSize,
                    r,
                    t,
                    e,
                    f,
                    rvecs,
                    tvecs,
                    perViewErrors,
                    flags,
                    criteria);
                var calibration = new StereoCalibrationResult(reprojectionError, cameraMatrix1, distCoeffs1, cameraMatrix2, distCoeffs2, r, t, e, f);
                return new StereoCalibrationExtendedResult(calibration, rvecs, tvecs, perViewErrors);
            }
            catch
            {
                cameraMatrix1.Dispose();
                distCoeffs1.Dispose();
                cameraMatrix2.Dispose();
                distCoeffs2.Dispose();
                r.Dispose();
                t.Dispose();
                e.Dispose();
                f.Dispose();
                rvecs.Dispose();
                tvecs.Dispose();
                perViewErrors.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Computes rectification transforms for a horizontal three-camera setup.
        /// 为三相机水平共线系统计算校正变换。
        /// </summary>
        /// <param name="cameraMatrix1">The first camera matrix. 第一个相机矩阵。</param>
        /// <param name="distCoeffs1">The first distortion coefficients. 第一组畸变系数。</param>
        /// <param name="cameraMatrix2">The second camera matrix. 第二个相机矩阵。</param>
        /// <param name="distCoeffs2">The second distortion coefficients. 第二组畸变系数。</param>
        /// <param name="cameraMatrix3">The third camera matrix. 第三个相机矩阵。</param>
        /// <param name="distCoeffs3">The third distortion coefficients. 第三组畸变系数。</param>
        /// <param name="imagePoints1">The first-camera image point groups. 第一相机图像点分组。</param>
        /// <param name="imagePoints3">The third-camera image point groups. 第三相机图像点分组。</param>
        /// <param name="imageSize">The original image size. 原始图像尺寸。</param>
        /// <param name="r12">The rotation from camera 1 to camera 2. 从相机 1 到相机 2 的旋转。</param>
        /// <param name="t12">The translation from camera 1 to camera 2. 从相机 1 到相机 2 的平移。</param>
        /// <param name="r13">The rotation from camera 1 to camera 3. 从相机 1 到相机 3 的旋转。</param>
        /// <param name="t13">The translation from camera 1 to camera 3. 从相机 1 到相机 3 的平移。</param>
        /// <param name="r1">The first output rectification transform. 第一个输出校正变换。</param>
        /// <param name="r2">The second output rectification transform. 第二个输出校正变换。</param>
        /// <param name="r3">The third output rectification transform. 第三个输出校正变换。</param>
        /// <param name="p1">The first output projection matrix. 第一个输出投影矩阵。</param>
        /// <param name="p2">The second output projection matrix. 第二个输出投影矩阵。</param>
        /// <param name="p3">The third output projection matrix. 第三个输出投影矩阵。</param>
        /// <param name="q">The output disparity-to-depth mapping matrix. 输出视差到深度映射矩阵。</param>
        /// <param name="alpha">The free scaling parameter. 自由缩放参数。</param>
        /// <param name="newImageSize">The new rectified image size. 新的校正图像尺寸。</param>
        /// <param name="flags">Stereo rectification flags. 双目校正标志。</param>
        /// <returns>The three-camera rectification result. 三相机校正结果。</returns>
        public static Rectify3CollinearResult Rectify3Collinear(
            Mat cameraMatrix1,
            Mat distCoeffs1,
            Mat cameraMatrix2,
            Mat distCoeffs2,
            Mat cameraMatrix3,
            Mat distCoeffs3,
            Point2f[][] imagePoints1,
            Point2f[][] imagePoints3,
            Size imageSize,
            Mat r12,
            Mat t12,
            Mat r13,
            Mat t13,
            Mat r1,
            Mat r2,
            Mat r3,
            Mat p1,
            Mat p2,
            Mat p3,
            Mat q,
            double alpha,
            Size newImageSize,
            StereoRectifyFlags flags = StereoRectifyFlags.ZeroDisparity)
        {
            ThrowIfNull(cameraMatrix1, nameof(cameraMatrix1));
            ThrowIfNull(distCoeffs1, nameof(distCoeffs1));
            ThrowIfNull(cameraMatrix2, nameof(cameraMatrix2));
            ThrowIfNull(distCoeffs2, nameof(distCoeffs2));
            ThrowIfNull(cameraMatrix3, nameof(cameraMatrix3));
            ThrowIfNull(distCoeffs3, nameof(distCoeffs3));
            ThrowIfNull(r12, nameof(r12));
            ThrowIfNull(t12, nameof(t12));
            ThrowIfNull(r13, nameof(r13));
            ThrowIfNull(t13, nameof(t13));
            ThrowIfNull(r1, nameof(r1));
            ThrowIfNull(r2, nameof(r2));
            ThrowIfNull(r3, nameof(r3));
            ThrowIfNull(p1, nameof(p1));
            ThrowIfNull(p2, nameof(p2));
            ThrowIfNull(p3, nameof(p3));
            ThrowIfNull(q, nameof(q));
            ValidatePositiveSize(imageSize, nameof(imageSize));
            ValidateNonNegativeSize(newImageSize, nameof(newImageSize));

            PreparePoint2fGroupPair(
                imagePoints1,
                imagePoints3,
                nameof(imagePoints1),
                nameof(imagePoints3),
                out int[] image1Offsets,
                out NativeMethods.Calib3DPoint2fNative[] nativeImagePoints1,
                out int[] image3Offsets,
                out NativeMethods.Calib3DPoint2fNative[] nativeImagePoints3);

            fixed (int* image1OffsetsPtr = image1Offsets)
            fixed (NativeMethods.Calib3DPoint2fNative* imagePoints1Ptr = nativeImagePoints1)
            fixed (int* image3OffsetsPtr = image3Offsets)
            fixed (NativeMethods.Calib3DPoint2fNative* imagePoints3Ptr = nativeImagePoints3)
            {
                NativeException.ThrowIfError(NativeMethods.Calib3DRectify3Collinear(
                    cameraMatrix1.NativeHandle,
                    distCoeffs1.NativeHandle,
                    cameraMatrix2.NativeHandle,
                    distCoeffs2.NativeHandle,
                    cameraMatrix3.NativeHandle,
                    distCoeffs3.NativeHandle,
                    image1OffsetsPtr,
                    imagePoints1.Length,
                    imagePoints1Ptr,
                    nativeImagePoints1.Length,
                    image3OffsetsPtr,
                    imagePoints3.Length,
                    imagePoints3Ptr,
                    nativeImagePoints3.Length,
                    imageSize.Width,
                    imageSize.Height,
                    r12.NativeHandle,
                    t12.NativeHandle,
                    r13.NativeHandle,
                    t13.NativeHandle,
                    r1.NativeHandle,
                    r2.NativeHandle,
                    r3.NativeHandle,
                    p1.NativeHandle,
                    p2.NativeHandle,
                    p3.NativeHandle,
                    q.NativeHandle,
                    alpha,
                    newImageSize.Width,
                    newImageSize.Height,
                    (int)flags,
                    out int roi1X,
                    out int roi1Y,
                    out int roi1Width,
                    out int roi1Height,
                    out int roi2X,
                    out int roi2Y,
                    out int roi2Width,
                    out int roi2Height,
                    out float scale));
                return new Rectify3CollinearResult(
                    scale,
                    new Rect(roi1X, roi1Y, roi1Width, roi1Height),
                    new Rect(roi2X, roi2Y, roi2Width, roi2Height));
            }
        }

        /// <summary>
        /// Creates a point matrix from 2D single-precision points.
        /// 从二维单精度点创建点矩阵。
        /// </summary>
        /// <param name="points">The input points. 输入点。</param>
        /// <returns>An <c>N x 1</c> matrix of type <c>CV_32FC2</c>. 类型为 <c>CV_32FC2</c> 的 <c>N x 1</c> 矩阵。</returns>
        public static Mat ToPointMat(Point2f[] points)
        {
            PointSetMarshaller.ValidateNotEmpty(points, nameof(points));

#if NETCOREAPP3_1_OR_GREATER
            return ToPointMat(points.AsSpan());
#else
            var result = new Mat(points.Length, 1, MatType.CV_32FC2);
            float[] values = new float[points.Length * 2];
            for (int i = 0; i < points.Length; i++)
            {
                int offset = i * 2;
                values[offset] = points[i].X;
                values[offset + 1] = points[i].Y;
            }

            result.CopyFrom(ToByteArray(values));
            return result;
#endif
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Creates a point matrix from 2D single-precision points without first flattening to a managed array.
        /// 从二维单精度点创建点矩阵，避免先展平为 managed 数组。
        /// </summary>
        /// <param name="points">The input points. 输入点。</param>
        /// <returns>An <c>N x 1</c> matrix of type <c>CV_32FC2</c>. 类型为 <c>CV_32FC2</c> 的 <c>N x 1</c> 矩阵。</returns>
        public static Mat ToPointMat(ReadOnlySpan<Point2f> points)
        {
            PointSetMarshaller.ValidateNotEmpty(points, nameof(points));
            var result = new Mat(points.Length, 1, MatType.CV_32FC2);
            try
            {
                result.CopyFrom(PointSetMarshaller.AsInterleaved(points));
                return result;
            }
            catch
            {
                result.Dispose();
                throw;
            }
        }
#endif

        /// <summary>
        /// Creates a point matrix from 3D single-precision points.
        /// 从三维单精度点创建点矩阵。
        /// </summary>
        /// <param name="points">The input points. 输入点。</param>
        /// <returns>An <c>N x 1</c> matrix of type <c>CV_32FC3</c>. 类型为 <c>CV_32FC3</c> 的 <c>N x 1</c> 矩阵。</returns>
        public static Mat ToPointMat(Point3f[] points)
        {
            PointSetMarshaller.ValidateNotEmpty(points, nameof(points));

#if NETCOREAPP3_1_OR_GREATER
            return ToPointMat(points.AsSpan());
#else
            var result = new Mat(points.Length, 1, MatType.CV_32FC3);
            float[] values = new float[points.Length * 3];
            for (int i = 0; i < points.Length; i++)
            {
                int offset = i * 3;
                values[offset] = points[i].X;
                values[offset + 1] = points[i].Y;
                values[offset + 2] = points[i].Z;
            }

            result.CopyFrom(ToByteArray(values));
            return result;
#endif
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Creates a point matrix from 3D single-precision points without first flattening to a managed array.
        /// 从三维单精度点创建点矩阵，避免先展平为 managed 数组。
        /// </summary>
        /// <param name="points">The input points. 输入点。</param>
        /// <returns>An <c>N x 1</c> matrix of type <c>CV_32FC3</c>. 类型为 <c>CV_32FC3</c> 的 <c>N x 1</c> 矩阵。</returns>
        public static Mat ToPointMat(ReadOnlySpan<Point3f> points)
        {
            PointSetMarshaller.ValidateNotEmpty(points, nameof(points));
            var result = new Mat(points.Length, 1, MatType.CV_32FC3);
            try
            {
                result.CopyFrom(PointSetMarshaller.AsInterleaved(points));
                return result;
            }
            catch
            {
                result.Dispose();
                throw;
            }
        }
#endif

        /// <summary>
        /// Projects 3D points to an image plane and returns a point matrix.
        /// 将三维点投影到图像平面并返回点矩阵。
        /// </summary>
        /// <param name="objectPoints">The input 3D object points. 输入三维物点。</param>
        /// <param name="rvec">The rotation vector. 旋转向量。</param>
        /// <param name="tvec">The translation vector. 平移向量。</param>
        /// <param name="cameraMatrix">The camera intrinsic matrix. 相机内参矩阵。</param>
        /// <param name="distCoeffs">The distortion coefficients. 畸变系数。</param>
        /// <param name="aspectRatio">The optional fixed aspect ratio. 可选固定宽高比。</param>
        /// <returns>The projected points as an <c>N x 1</c> <c>CV_32FC2</c>/<c>CV_64FC2</c> matrix. 作为 <c>N x 1</c> <c>CV_32FC2</c>/<c>CV_64FC2</c> 矩阵返回的投影点。</returns>
        public static Mat ProjectPoints(Point3f[] objectPoints, Mat rvec, Mat tvec, Mat cameraMatrix, Mat distCoeffs, double aspectRatio = 0)
        {
            using (Mat objectPointMat = ToPointMat(objectPoints))
            {
                return ProjectPoints(objectPointMat, rvec, tvec, cameraMatrix, distCoeffs, aspectRatio);
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Projects 3D points from a span to an image plane and returns a point matrix.
        /// 将 Span 中的三维点投影到图像平面并返回点矩阵。
        /// </summary>
        /// <param name="objectPoints">The input 3D object points. 输入三维物点。</param>
        /// <param name="rvec">The rotation vector. 旋转向量。</param>
        /// <param name="tvec">The translation vector. 平移向量。</param>
        /// <param name="cameraMatrix">The camera intrinsic matrix. 相机内参矩阵。</param>
        /// <param name="distCoeffs">The distortion coefficients. 畸变系数。</param>
        /// <param name="aspectRatio">The optional fixed aspect ratio. 可选固定宽高比。</param>
        /// <returns>The projected points as an <c>N x 1</c> point matrix. 作为 <c>N x 1</c> 点矩阵返回的投影点。</returns>
        public static Mat ProjectPoints(ReadOnlySpan<Point3f> objectPoints, Mat rvec, Mat tvec, Mat cameraMatrix, Mat distCoeffs, double aspectRatio = 0)
        {
            using (Mat objectPointMat = ToPointMat(objectPoints))
            {
                return ProjectPoints(objectPointMat, rvec, tvec, cameraMatrix, distCoeffs, aspectRatio);
            }
        }
#endif

        /// <summary>
        /// Solves pose from managed point arrays.
        /// 根据 managed 点数组求解位姿。
        /// </summary>
        /// <param name="objectPoints">The input 3D object points. 输入三维物点。</param>
        /// <param name="imagePoints">The input 2D image points. 输入二维像点。</param>
        /// <param name="cameraMatrix">The camera intrinsic matrix. 相机内参矩阵。</param>
        /// <param name="distCoeffs">The distortion coefficients. 畸变系数。</param>
        /// <param name="rvec">The output rotation vector. 输出旋转向量。</param>
        /// <param name="tvec">The output translation vector. 输出平移向量。</param>
        /// <param name="useExtrinsicGuess">Whether to use the supplied vectors as an initial guess. 是否使用传入向量作为初值。</param>
        /// <param name="flags">The PnP method. PnP 方法。</param>
        /// <returns><c>true</c> if a pose was found; otherwise, <c>false</c>. 如果找到位姿则返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        public static bool SolvePnP(
            Point3f[] objectPoints,
            Point2f[] imagePoints,
            Mat cameraMatrix,
            Mat distCoeffs,
            Mat rvec,
            Mat tvec,
            bool useExtrinsicGuess = false,
            SolvePnPFlags flags = SolvePnPFlags.Iterative)
        {
            using (Mat objectPointMat = ToPointMat(objectPoints))
            using (Mat imagePointMat = ToPointMat(imagePoints))
            {
                return SolvePnP(objectPointMat, imagePointMat, cameraMatrix, distCoeffs, rvec, tvec, useExtrinsicGuess, flags);
            }
        }

        /// <summary>
        /// Finds all pose solutions from managed point arrays.
        /// 根据 managed 点数组查找所有位姿解。
        /// </summary>
        /// <param name="objectPoints">The input 3D object points. 输入三维物点。</param>
        /// <param name="imagePoints">The input 2D image points. 输入二维像点。</param>
        /// <param name="cameraMatrix">The camera intrinsic matrix. 相机内参矩阵。</param>
        /// <param name="distCoeffs">The distortion coefficients. 畸变系数。</param>
        /// <param name="useExtrinsicGuess">Whether to use the optional pose as an initial guess. 是否使用可选位姿作为初值。</param>
        /// <param name="flags">The PnP method. PnP 方法。</param>
        /// <param name="rvec">The optional initial rotation vector. 可选初始旋转向量。</param>
        /// <param name="tvec">The optional initial translation vector. 可选初始平移向量。</param>
        /// <param name="returnReprojectionError">Whether to request reprojection errors. 是否请求重投影误差。</param>
        /// <returns>The pose solution bundle. 位姿解结果包。</returns>
        public static SolvePnPGenericResult SolvePnPGeneric(
            Point3f[] objectPoints,
            Point2f[] imagePoints,
            Mat cameraMatrix,
            Mat distCoeffs,
            bool useExtrinsicGuess = false,
            SolvePnPFlags flags = SolvePnPFlags.Iterative,
            Mat? rvec = null,
            Mat? tvec = null,
            bool returnReprojectionError = false)
        {
            using (Mat objectPointMat = ToPointMat(objectPoints))
            using (Mat imagePointMat = ToPointMat(imagePoints))
            {
                return SolvePnPGeneric(objectPointMat, imagePointMat, cameraMatrix, distCoeffs, useExtrinsicGuess, flags, rvec, tvec, returnReprojectionError);
            }
        }

        /// <summary>
        /// Refines a pose from managed point arrays with Levenberg-Marquardt optimization.
        /// 使用 Levenberg-Marquardt 优化，根据 managed 点数组细化位姿。
        /// </summary>
        /// <param name="objectPoints">The input 3D object points. 输入三维物点。</param>
        /// <param name="imagePoints">The input 2D image points. 输入二维像点。</param>
        /// <param name="cameraMatrix">The camera intrinsic matrix. 相机内参矩阵。</param>
        /// <param name="distCoeffs">The distortion coefficients. 畸变系数。</param>
        /// <param name="rvec">The input-output rotation vector. 输入输出旋转向量。</param>
        /// <param name="tvec">The input-output translation vector. 输入输出平移向量。</param>
        /// <param name="criteria">The stop criteria. 停止条件。</param>
        public static void SolvePnPRefineLM(Point3f[] objectPoints, Point2f[] imagePoints, Mat cameraMatrix, Mat distCoeffs, Mat rvec, Mat tvec, TermCriteria? criteria = null)
        {
            using (Mat objectPointMat = ToPointMat(objectPoints))
            using (Mat imagePointMat = ToPointMat(imagePoints))
            {
                SolvePnPRefineLM(objectPointMat, imagePointMat, cameraMatrix, distCoeffs, rvec, tvec, criteria);
            }
        }

        /// <summary>
        /// Refines a pose from managed point arrays with virtual visual servoing optimization.
        /// 使用虚拟视觉伺服优化，根据 managed 点数组细化位姿。
        /// </summary>
        /// <param name="objectPoints">The input 3D object points. 输入三维物点。</param>
        /// <param name="imagePoints">The input 2D image points. 输入二维像点。</param>
        /// <param name="cameraMatrix">The camera intrinsic matrix. 相机内参矩阵。</param>
        /// <param name="distCoeffs">The distortion coefficients. 畸变系数。</param>
        /// <param name="rvec">The input-output rotation vector. 输入输出旋转向量。</param>
        /// <param name="tvec">The input-output translation vector. 输入输出平移向量。</param>
        /// <param name="criteria">The stop criteria. 停止条件。</param>
        /// <param name="vvsLambda">The VVS gain. VVS 增益。</param>
        public static void SolvePnPRefineVVS(Point3f[] objectPoints, Point2f[] imagePoints, Mat cameraMatrix, Mat distCoeffs, Mat rvec, Mat tvec, TermCriteria? criteria = null, double vvsLambda = 1.0)
        {
            using (Mat objectPointMat = ToPointMat(objectPoints))
            using (Mat imagePointMat = ToPointMat(imagePoints))
            {
                SolvePnPRefineVVS(objectPointMat, imagePointMat, cameraMatrix, distCoeffs, rvec, tvec, criteria, vvsLambda);
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Solves pose from point spans.
        /// 根据点 Span 求解位姿。
        /// </summary>
        /// <param name="objectPoints">The input 3D object points. 输入三维物点。</param>
        /// <param name="imagePoints">The input 2D image points. 输入二维像点。</param>
        /// <param name="cameraMatrix">The camera intrinsic matrix. 相机内参矩阵。</param>
        /// <param name="distCoeffs">The distortion coefficients. 畸变系数。</param>
        /// <param name="rvec">The output rotation vector. 输出旋转向量。</param>
        /// <param name="tvec">The output translation vector. 输出平移向量。</param>
        /// <param name="useExtrinsicGuess">Whether to use the supplied vectors as an initial guess. 是否使用传入向量作为初值。</param>
        /// <param name="flags">The PnP method. PnP 方法。</param>
        /// <returns><c>true</c> if a pose was found; otherwise, <c>false</c>. 如果找到位姿则返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        public static bool SolvePnP(
            ReadOnlySpan<Point3f> objectPoints,
            ReadOnlySpan<Point2f> imagePoints,
            Mat cameraMatrix,
            Mat distCoeffs,
            Mat rvec,
            Mat tvec,
            bool useExtrinsicGuess = false,
            SolvePnPFlags flags = SolvePnPFlags.Iterative)
        {
            using (Mat objectPointMat = ToPointMat(objectPoints))
            using (Mat imagePointMat = ToPointMat(imagePoints))
            {
                return SolvePnP(objectPointMat, imagePointMat, cameraMatrix, distCoeffs, rvec, tvec, useExtrinsicGuess, flags);
            }
        }

        /// <summary>
        /// Finds all pose solutions from point spans.
        /// 根据点 Span 查找所有位姿解。
        /// </summary>
        /// <param name="objectPoints">The input 3D object points. 输入三维物点。</param>
        /// <param name="imagePoints">The input 2D image points. 输入二维像点。</param>
        /// <param name="cameraMatrix">The camera intrinsic matrix. 相机内参矩阵。</param>
        /// <param name="distCoeffs">The distortion coefficients. 畸变系数。</param>
        /// <param name="useExtrinsicGuess">Whether to use the optional pose as an initial guess. 是否使用可选位姿作为初值。</param>
        /// <param name="flags">The PnP method. PnP 方法。</param>
        /// <param name="rvec">The optional initial rotation vector. 可选初始旋转向量。</param>
        /// <param name="tvec">The optional initial translation vector. 可选初始平移向量。</param>
        /// <param name="returnReprojectionError">Whether to request reprojection errors. 是否请求重投影误差。</param>
        /// <returns>The pose solution bundle. 位姿解结果包。</returns>
        public static SolvePnPGenericResult SolvePnPGeneric(
            ReadOnlySpan<Point3f> objectPoints,
            ReadOnlySpan<Point2f> imagePoints,
            Mat cameraMatrix,
            Mat distCoeffs,
            bool useExtrinsicGuess = false,
            SolvePnPFlags flags = SolvePnPFlags.Iterative,
            Mat? rvec = null,
            Mat? tvec = null,
            bool returnReprojectionError = false)
        {
            using (Mat objectPointMat = ToPointMat(objectPoints))
            using (Mat imagePointMat = ToPointMat(imagePoints))
            {
                return SolvePnPGeneric(objectPointMat, imagePointMat, cameraMatrix, distCoeffs, useExtrinsicGuess, flags, rvec, tvec, returnReprojectionError);
            }
        }

        /// <summary>
        /// Refines a pose from point spans with Levenberg-Marquardt optimization.
        /// 使用 Levenberg-Marquardt 优化，根据点 Span 细化位姿。
        /// </summary>
        /// <param name="objectPoints">The input 3D object points. 输入三维物点。</param>
        /// <param name="imagePoints">The input 2D image points. 输入二维像点。</param>
        /// <param name="cameraMatrix">The camera intrinsic matrix. 相机内参矩阵。</param>
        /// <param name="distCoeffs">The distortion coefficients. 畸变系数。</param>
        /// <param name="rvec">The input-output rotation vector. 输入输出旋转向量。</param>
        /// <param name="tvec">The input-output translation vector. 输入输出平移向量。</param>
        /// <param name="criteria">The stop criteria. 停止条件。</param>
        public static void SolvePnPRefineLM(ReadOnlySpan<Point3f> objectPoints, ReadOnlySpan<Point2f> imagePoints, Mat cameraMatrix, Mat distCoeffs, Mat rvec, Mat tvec, TermCriteria? criteria = null)
        {
            using (Mat objectPointMat = ToPointMat(objectPoints))
            using (Mat imagePointMat = ToPointMat(imagePoints))
            {
                SolvePnPRefineLM(objectPointMat, imagePointMat, cameraMatrix, distCoeffs, rvec, tvec, criteria);
            }
        }

        /// <summary>
        /// Refines a pose from point spans with virtual visual servoing optimization.
        /// 使用虚拟视觉伺服优化，根据点 Span 细化位姿。
        /// </summary>
        /// <param name="objectPoints">The input 3D object points. 输入三维物点。</param>
        /// <param name="imagePoints">The input 2D image points. 输入二维像点。</param>
        /// <param name="cameraMatrix">The camera intrinsic matrix. 相机内参矩阵。</param>
        /// <param name="distCoeffs">The distortion coefficients. 畸变系数。</param>
        /// <param name="rvec">The input-output rotation vector. 输入输出旋转向量。</param>
        /// <param name="tvec">The input-output translation vector. 输入输出平移向量。</param>
        /// <param name="criteria">The stop criteria. 停止条件。</param>
        /// <param name="vvsLambda">The VVS gain. VVS 增益。</param>
        public static void SolvePnPRefineVVS(ReadOnlySpan<Point3f> objectPoints, ReadOnlySpan<Point2f> imagePoints, Mat cameraMatrix, Mat distCoeffs, Mat rvec, Mat tvec, TermCriteria? criteria = null, double vvsLambda = 1.0)
        {
            using (Mat objectPointMat = ToPointMat(objectPoints))
            using (Mat imagePointMat = ToPointMat(imagePoints))
            {
                SolvePnPRefineVVS(objectPointMat, imagePointMat, cameraMatrix, distCoeffs, rvec, tvec, criteria, vvsLambda);
            }
        }
#endif

        private static void PrepareCalibrationPointGroups(
            Point3f[][] objectPoints,
            Point2f[][] imagePoints,
            string objectParameterName,
            string imageParameterName,
            out int[] objectOffsets,
            out NativeMethods.Calib3DPoint3fNative[] nativeObjectPoints,
            out int[] imageOffsets,
            out NativeMethods.Calib3DPoint2fNative[] nativeImagePoints)
        {
            PointSetMarshaller.FlattenPoint3fGroups(objectPoints, objectParameterName, out objectOffsets, out Point3f[] flatObjectPoints);
            PointSetMarshaller.FlattenPoint2fGroups(imagePoints, imageParameterName, out imageOffsets, out Point2f[] flatImagePoints);
            ValidateMatchingCalibrationGroups(objectPoints, imagePoints, objectParameterName, imageParameterName);
            nativeObjectPoints = ToNativePoint3fArray(flatObjectPoints);
            nativeImagePoints = ToNativePoint2fArray(flatImagePoints);
        }

        private static void PrepareStereoCalibrationPointGroups(
            Point3f[][] objectPoints,
            Point2f[][] imagePoints1,
            Point2f[][] imagePoints2,
            out int[] objectOffsets,
            out NativeMethods.Calib3DPoint3fNative[] nativeObjectPoints,
            out int[] image1Offsets,
            out NativeMethods.Calib3DPoint2fNative[] nativeImagePoints1,
            out int[] image2Offsets,
            out NativeMethods.Calib3DPoint2fNative[] nativeImagePoints2)
        {
            PrepareCalibrationPointGroups(
                objectPoints,
                imagePoints1,
                nameof(objectPoints),
                nameof(imagePoints1),
                out objectOffsets,
                out nativeObjectPoints,
                out image1Offsets,
                out nativeImagePoints1);
            PointSetMarshaller.FlattenPoint2fGroups(imagePoints2, nameof(imagePoints2), out image2Offsets, out Point2f[] flatImagePoints2);
            ValidateMatchingPoint2fGroups(imagePoints1, imagePoints2, nameof(imagePoints1), nameof(imagePoints2));
            nativeImagePoints2 = ToNativePoint2fArray(flatImagePoints2);
        }

        private static void PreparePoint2fGroupPair(
            Point2f[][] first,
            Point2f[][] second,
            string firstParameterName,
            string secondParameterName,
            out int[] firstOffsets,
            out NativeMethods.Calib3DPoint2fNative[] nativeFirstPoints,
            out int[] secondOffsets,
            out NativeMethods.Calib3DPoint2fNative[] nativeSecondPoints)
        {
            PointSetMarshaller.FlattenPoint2fGroups(first, firstParameterName, out firstOffsets, out Point2f[] flatFirstPoints);
            PointSetMarshaller.FlattenPoint2fGroups(second, secondParameterName, out secondOffsets, out Point2f[] flatSecondPoints);
            ValidateMatchingPoint2fGroups(first, second, firstParameterName, secondParameterName);
            nativeFirstPoints = ToNativePoint2fArray(flatFirstPoints);
            nativeSecondPoints = ToNativePoint2fArray(flatSecondPoints);
        }

        private static void ValidateMatchingCalibrationGroups(Point3f[][] objectPoints, Point2f[][] imagePoints, string objectParameterName, string imageParameterName)
        {
            if (objectPoints.Length == 0)
            {
                throw new ArgumentException("Point group collection cannot be empty.", objectParameterName);
            }

            if (imagePoints.Length != objectPoints.Length)
            {
                throw new ArgumentException("Point group collections must have the same length.", imageParameterName);
            }

            for (int i = 0; i < objectPoints.Length; i++)
            {
                if (objectPoints[i].Length == 0)
                {
                    throw new ArgumentException("Point groups cannot be empty.", objectParameterName);
                }

                if (imagePoints[i].Length == 0)
                {
                    throw new ArgumentException("Point groups cannot be empty.", imageParameterName);
                }

                if (objectPoints[i].Length != imagePoints[i].Length)
                {
                    throw new ArgumentException("Each image point group must match the corresponding object point group length.", imageParameterName);
                }
            }
        }

        private static void ValidateMatchingPoint2fGroups(Point2f[][] first, Point2f[][] second, string firstParameterName, string secondParameterName)
        {
            if (first.Length == 0)
            {
                throw new ArgumentException("Point group collection cannot be empty.", firstParameterName);
            }

            if (second.Length != first.Length)
            {
                throw new ArgumentException("Point group collections must have the same length.", secondParameterName);
            }

            for (int i = 0; i < first.Length; i++)
            {
                if (first[i].Length == 0)
                {
                    throw new ArgumentException("Point groups cannot be empty.", firstParameterName);
                }

                if (second[i].Length == 0)
                {
                    throw new ArgumentException("Point groups cannot be empty.", secondParameterName);
                }

                if (first[i].Length != second[i].Length)
                {
                    throw new ArgumentException("Point groups at the same index must have matching lengths.", secondParameterName);
                }
            }
        }

        private static NativeMethods.Calib3DPoint2fNative[] ToNativePoint2fArray(Point2f[] points)
        {
            var result = new NativeMethods.Calib3DPoint2fNative[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                result[i] = new NativeMethods.Calib3DPoint2fNative
                {
                    X = points[i].X,
                    Y = points[i].Y
                };
            }

            return result;
        }

        private static NativeMethods.Calib3DPoint3fNative[] ToNativePoint3fArray(Point3f[] points)
        {
            var result = new NativeMethods.Calib3DPoint3fNative[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                result[i] = new NativeMethods.Calib3DPoint3fNative
                {
                    X = points[i].X,
                    Y = points[i].Y,
                    Z = points[i].Z
                };
            }

            return result;
        }

        private static void ValidatePositiveSize(Size size, string parameterName)
        {
            if (size.Width <= 0 || size.Height <= 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Size must be positive.");
            }
        }

        private static void ValidateNonNegativeSize(Size size, string parameterName)
        {
            if (size.Width < 0 || size.Height < 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Size cannot be negative.");
            }
        }

        private static void ValidateCameraUtilityTranslation(Mat value, string parameterName)
        {
            if (!((value.Rows == 3 && value.Cols == 1) ||
                  (value.Rows == 1 && value.Cols == 3)))
            {
                throw new ArgumentException("Translation vector must be 3 x 1 or 1 x 3.", parameterName);
            }
        }

        private static void ValidatePoint2fMatrix(Mat value, string parameterName)
        {
            if (value.Rows <= 0 || value.Cols <= 0)
            {
                throw new ArgumentException("Point matrix cannot be empty.", parameterName);
            }

            if (value.Type != MatType.CV_32FC2)
            {
                throw new ArgumentException("Point matrix must be CV_32FC2.", parameterName);
            }
        }

        private static void ValidateUndistortPointMatrix(Mat value, string parameterName)
        {
            if (value.Empty)
            {
                throw new ArgumentException("Point matrix cannot be empty.", parameterName);
            }
            if (value.Depth != MatType.CV_32F && value.Depth != MatType.CV_64F)
            {
                throw new ArgumentException(
                    "Point matrix depth must be CV_32F or CV_64F.",
                    parameterName);
            }

            bool channelVector =
                value.Channels == 2 &&
                (value.Rows == 1 || value.Cols == 1);
            bool scalarMatrix =
                value.Channels == 1 &&
                (value.Rows == 2 || value.Cols == 2);
            if (!channelVector && !scalarMatrix)
            {
                throw new ArgumentException(
                    "Point matrix must be 2 x N or N x 2 single-channel, or a two-channel vector.",
                    parameterName);
            }
        }

        private static void ValidateUndistortOptionalRectification(Mat? value, string parameterName)
        {
            if (value == null || value.Empty)
            {
                return;
            }
            if (value.Depth != MatType.CV_32F && value.Depth != MatType.CV_64F)
            {
                throw new ArgumentException(
                    "Rectification must use CV_32F or CV_64F depth.",
                    parameterName);
            }

            bool matrix = value.Channels == 1 && value.Rows == 3 && value.Cols == 3;
            bool scalarVector = value.Channels == 1 && value.Rows * value.Cols == 3;
            bool channelVector = value.Channels == 3 && value.Rows * value.Cols == 1;
            if (!matrix && !scalarVector && !channelVector)
            {
                throw new ArgumentException(
                    "Rectification must be a 3 x 3 matrix or a three-value vector.",
                    parameterName);
            }
        }

        private static void ValidateUndistortPointsOutputDoesNotAlias(
            Mat src,
            Mat dst,
            Mat cameraMatrix,
            Mat distCoeffs,
            Mat? r,
            Mat? p,
            string outputParameterName)
        {
            IntPtr outputHandle = dst.NativeHandle;
            if (ReferenceEquals(src, dst) ||
                ReferenceEquals(cameraMatrix, dst) ||
                ReferenceEquals(distCoeffs, dst) ||
                ReferenceEquals(r, dst) ||
                ReferenceEquals(p, dst) ||
                src.NativeHandle == outputHandle ||
                cameraMatrix.NativeHandle == outputHandle ||
                distCoeffs.NativeHandle == outputHandle ||
                (r != null && r.NativeHandle == outputHandle) ||
                (p != null && p.NativeHandle == outputHandle))
            {
                throw new ArgumentException("Output matrix must not alias any input matrix.", outputParameterName);
            }
        }

        private static void ValidateEpilineImageIndex(int whichImage, string parameterName)
        {
            if (whichImage != 1 && whichImage != 2)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    "Image index must be 1 or 2.");
            }
        }

        private static void ValidateEpilinePointMatrix(Mat value, string parameterName)
        {
            if (value.Empty)
            {
                throw new ArgumentException("Point matrix cannot be empty.", parameterName);
            }

            if (value.Depth != MatType.CV_32S &&
                value.Depth != MatType.CV_32F &&
                value.Depth != MatType.CV_64F)
            {
                throw new ArgumentException(
                    "Point matrix depth must be CV_32S, CV_32F, or CV_64F.",
                    parameterName);
            }

            if (!TryGetPointCount(value, 2, out _) &&
                !TryGetPointCount(value, 3, out _))
            {
                throw new ArgumentException(
                    "Point matrix must contain non-empty 2- or 3-component points.",
                    parameterName);
            }
        }

        private static void ValidateEpilineOutputDoesNotAlias(
            Mat points,
            Mat fundamental,
            Mat lines,
            string outputParameterName)
        {
            IntPtr outputHandle = lines.NativeHandle;
            if (ReferenceEquals(points, lines) ||
                ReferenceEquals(fundamental, lines) ||
                points.NativeHandle == outputHandle ||
                fundamental.NativeHandle == outputHandle)
            {
                throw new ArgumentException("Output matrix must not alias any input matrix.", outputParameterName);
            }
        }

        private static void ValidateTriangulationProjectionMatrix(Mat value, string parameterName)
        {
            if (value.Rows != 3 || value.Cols != 4 || value.Channels != 1)
            {
                throw new ArgumentException("Projection matrix must be a single-channel 3 x 4 matrix.", parameterName);
            }

            if (value.Depth != MatType.CV_32F && value.Depth != MatType.CV_64F)
            {
                throw new ArgumentException("Projection matrix must use CV_32F or CV_64F depth.", parameterName);
            }
        }

        private static void ValidateTriangulationPointMatrix(Mat value, string parameterName)
        {
            if (value.Rows <= 0 || value.Cols <= 0)
            {
                throw new ArgumentException("Point matrix cannot be empty.", parameterName);
            }

            if (value.Depth != MatType.CV_32F && value.Depth != MatType.CV_64F)
            {
                throw new ArgumentException("Point matrix must use CV_32F or CV_64F depth.", parameterName);
            }

            bool isTwoChannelVector = value.Channels == 2 && (value.Rows == 1 || value.Cols == 1);
            bool isTwoRowScalarMatrix = value.Channels == 1 && value.Rows == 2;
            if (!isTwoChannelVector && !isTwoRowScalarMatrix)
            {
                throw new ArgumentException(
                    "Point matrix must be a row/column vector of two-channel points or a single-channel 2 x N matrix.",
                    parameterName);
            }
        }

        private static void ValidateMatchingPointMatrixCount(
            Mat first,
            string firstParameterName,
            Mat second,
            string secondParameterName)
        {
            int firstCount = GetPointMatrixCount(first);
            int secondCount = GetPointMatrixCount(second);
            if (firstCount != secondCount)
            {
                throw new ArgumentException(
                    "Point matrices must contain the same number of points.",
                    secondParameterName);
            }
        }

        private static int GetPointMatrixCount(Mat value)
        {
            if (value.Channels == 1 && value.Rows == 2)
            {
                return value.Cols;
            }

            return value.Rows * value.Cols;
        }

        private static void ValidateTriangulationOutputDoesNotAlias(
            Mat projMatr1,
            Mat projMatr2,
            Mat projPoints1,
            Mat projPoints2,
            Mat points4D,
            string outputParameterName)
        {
            IntPtr outputHandle = points4D.NativeHandle;
            if (ReferenceEquals(projMatr1, points4D) ||
                ReferenceEquals(projMatr2, points4D) ||
                ReferenceEquals(projPoints1, points4D) ||
                ReferenceEquals(projPoints2, points4D) ||
                projMatr1.NativeHandle == outputHandle ||
                projMatr2.NativeHandle == outputHandle ||
                projPoints1.NativeHandle == outputHandle ||
                projPoints2.NativeHandle == outputHandle)
            {
                throw new ArgumentException("Output matrix must not alias any input matrix.", outputParameterName);
            }
        }

        private static void ValidateInitUndistortRectifyMapType(int m1type, string parameterName)
        {
            if (m1type != MatType.CV_16SC2 &&
                m1type != MatType.CV_32FC1 &&
                m1type != MatType.CV_32FC2)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    "Map type must be CV_16SC2, CV_32FC1, or CV_32FC2.");
            }
        }

        private static void ValidateDistinctOutputPair(
            Mat first,
            string firstParameterName,
            Mat second,
            string secondParameterName)
        {
            IntPtr firstHandle = first.NativeHandle;
            IntPtr secondHandle = second.NativeHandle;
            if (ReferenceEquals(first, second) || firstHandle == secondHandle)
            {
                throw new ArgumentException(
                    "Output matrices must not alias.",
                    secondParameterName);
            }
        }

        private static void ValidateDistinctOutputSet(Mat[] outputs, string[] parameterNames)
        {
            for (int i = 0; i < outputs.Length; i++)
            {
                for (int j = i + 1; j < outputs.Length; j++)
                {
                    if (ReferenceEquals(outputs[i], outputs[j]) ||
                        outputs[i].NativeHandle == outputs[j].NativeHandle)
                    {
                        throw new ArgumentException(
                            "Output matrices must not alias.",
                            parameterNames[j]);
                    }
                }
            }
        }

        private static IntPtr GetNativeHandleOrZero(Mat? mat)
        {
            return mat == null ? IntPtr.Zero : mat.NativeHandle;
        }

        private static void ThrowIfNull(object? value, string parameterName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        private static void ValidatePatternSize(Size size, string parameterName)
        {
            if (size.Width <= 0 || size.Height <= 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Pattern size must be positive.");
            }
        }

#if !NETCOREAPP3_1_OR_GREATER
        private static byte[] ToByteArray(float[] values)
        {
            byte[] bytes = new byte[values.Length * sizeof(float)];
            Buffer.BlockCopy(values, 0, bytes, 0, bytes.Length);
            return bytes;
        }
#endif
    }
}

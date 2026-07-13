using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Calib3D
{
    public static partial class Cv2
    {
        /// <summary>
        /// Projects 3D points and computes the six separated Jacobian blocks.
        /// 投影三维点并计算六个分离的 Jacobian 块。
        /// </summary>
        /// <param name="objectPoints">The input 3D object points. 输入三维物点。</param>
        /// <param name="rvec">The rotation vector or rotation matrix. 旋转向量或旋转矩阵。</param>
        /// <param name="tvec">The translation vector. 平移向量。</param>
        /// <param name="cameraMatrix">The 3 x 3 camera matrix. 3 x 3 相机矩阵。</param>
        /// <param name="distCoeffs">The distortion coefficients, or an empty matrix for five zero coefficients. 畸变系数；空矩阵表示五个零系数。</param>
        /// <param name="imagePoints">The caller-owned projected image points. 调用方持有的投影像点。</param>
        /// <param name="dpdr">The caller-owned derivative with respect to rotation. 调用方持有的旋转导数。</param>
        /// <param name="dpdt">The caller-owned derivative with respect to translation. 调用方持有的平移导数。</param>
        /// <param name="dpdf">The caller-owned derivative with respect to focal lengths. 调用方持有的焦距导数。</param>
        /// <param name="dpdc">The caller-owned derivative with respect to the principal point. 调用方持有的主点导数。</param>
        /// <param name="dpdk">The caller-owned derivative with respect to distortion coefficients. 调用方持有的畸变系数导数。</param>
        /// <param name="dpdo">The caller-owned derivative with respect to object-point coordinates. 调用方持有的物点坐标导数。</param>
        /// <param name="aspectRatio">The optional fixed aspect ratio. 可选固定宽高比。</param>
        public static void ProjectPoints(
            Mat objectPoints,
            Mat rvec,
            Mat tvec,
            Mat cameraMatrix,
            Mat distCoeffs,
            Mat imagePoints,
            Mat dpdr,
            Mat dpdt,
            Mat dpdf,
            Mat dpdc,
            Mat dpdk,
            Mat dpdo,
            double aspectRatio = 0)
        {
            ThrowIfNull(objectPoints, nameof(objectPoints));
            ThrowIfNull(rvec, nameof(rvec));
            ThrowIfNull(tvec, nameof(tvec));
            ThrowIfNull(cameraMatrix, nameof(cameraMatrix));
            ThrowIfNull(distCoeffs, nameof(distCoeffs));
            ThrowIfNull(imagePoints, nameof(imagePoints));
            ThrowIfNull(dpdr, nameof(dpdr));
            ThrowIfNull(dpdt, nameof(dpdt));
            ThrowIfNull(dpdf, nameof(dpdf));
            ThrowIfNull(dpdc, nameof(dpdc));
            ThrowIfNull(dpdk, nameof(dpdk));
            ThrowIfNull(dpdo, nameof(dpdo));

            int pointCount = ValidateProjectPointsObjectPoints(objectPoints);
            ValidateProjectPointsRotation(rvec, nameof(rvec));
            ValidateProjectPointsTranslation(tvec, nameof(tvec));
            ValidateCameraUtilityMatrix(cameraMatrix, nameof(cameraMatrix));
            ValidateProjectPointsDistortion(distCoeffs);
            ValidateProjectPointsAspectRatio(aspectRatio);
            ValidateProjectPointsDerivativeDimensions(pointCount);
            ValidateProjectPointsDerivativeOutputs(
                new[] { objectPoints, rvec, tvec, cameraMatrix, distCoeffs },
                new[] { imagePoints, dpdr, dpdt, dpdf, dpdc, dpdk, dpdo },
                new[]
                {
                    nameof(imagePoints),
                    nameof(dpdr),
                    nameof(dpdt),
                    nameof(dpdf),
                    nameof(dpdc),
                    nameof(dpdk),
                    nameof(dpdo)
                });

            NativeException.ThrowIfError(
                NativeMethods.Calib3DProjectPointsSeparatedJacobians(
                    objectPoints.NativeHandle,
                    rvec.NativeHandle,
                    tvec.NativeHandle,
                    cameraMatrix.NativeHandle,
                    distCoeffs.NativeHandle,
                    imagePoints.NativeHandle,
                    dpdr.NativeHandle,
                    dpdt.NativeHandle,
                    dpdf.NativeHandle,
                    dpdc.NativeHandle,
                    dpdk.NativeHandle,
                    dpdo.NativeHandle,
                    aspectRatio));
        }

        /// <summary>
        /// Projects 3D points and returns owned image points and separated Jacobian blocks.
        /// 投影三维点并返回拥有所有权的像点和分离 Jacobian 块。
        /// </summary>
        public static ProjectPointsDerivativesResult ProjectPointsWithDerivatives(
            Mat objectPoints,
            Mat rvec,
            Mat tvec,
            Mat cameraMatrix,
            Mat distCoeffs,
            double aspectRatio = 0)
        {
            var imagePoints = new Mat();
            var dpdr = new Mat();
            var dpdt = new Mat();
            var dpdf = new Mat();
            var dpdc = new Mat();
            var dpdk = new Mat();
            var dpdo = new Mat();

            try
            {
                ProjectPoints(
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
                    dpdo,
                    aspectRatio);
                return new ProjectPointsDerivativesResult(
                    imagePoints,
                    dpdr,
                    dpdt,
                    dpdf,
                    dpdc,
                    dpdk,
                    dpdo);
            }
            catch
            {
                DisposeProjectPointsDerivativeOutputs(
                    imagePoints,
                    dpdr,
                    dpdt,
                    dpdf,
                    dpdc,
                    dpdk,
                    dpdo);
                throw;
            }
        }

        private static int ValidateProjectPointsObjectPoints(Mat objectPoints)
        {
            if (objectPoints.Empty || objectPoints.Rows <= 0 || objectPoints.Cols <= 0)
            {
                throw new ArgumentException(
                    "Object-point matrix cannot be empty.",
                    nameof(objectPoints));
            }
            ValidateCameraUtilityFloatingDepth(objectPoints, nameof(objectPoints));

            int pointCount;
            try
            {
                checked
                {
                    if (objectPoints.Channels == 3 &&
                        (objectPoints.Rows == 1 || objectPoints.Cols == 1))
                    {
                        pointCount = objectPoints.Rows * objectPoints.Cols;
                    }
                    else if (objectPoints.Channels == 1 && objectPoints.Cols == 3)
                    {
                        pointCount = objectPoints.Rows;
                    }
                    else if (objectPoints.Channels == 1 && objectPoints.Rows == 3)
                    {
                        pointCount = objectPoints.Cols;
                    }
                    else
                    {
                        throw new ArgumentException(
                            "Object points must use N x 1/1 x N three-channel or N x 3/3 x N single-channel layout.",
                            nameof(objectPoints));
                    }

                    _ = pointCount * 2;
                    _ = pointCount * 3;
                }
            }
            catch (OverflowException exception)
            {
                throw new ArgumentException(
                    "Object-point count exceeds supported derivative dimensions.",
                    nameof(objectPoints),
                    exception);
            }

            if (pointCount <= 0)
            {
                throw new ArgumentException(
                    "Object-point matrix must contain at least one point.",
                    nameof(objectPoints));
            }

            return pointCount;
        }

        private static void ValidateProjectPointsRotation(Mat rvec, string parameterName)
        {
            if (rvec.Empty)
            {
                throw new ArgumentException("Rotation cannot be empty.", parameterName);
            }
            ValidateCameraUtilityFloatingDepth(rvec, parameterName);

            bool matrix = rvec.Channels == 1 && rvec.Rows == 3 && rvec.Cols == 3;
            bool scalarVector = rvec.Channels == 1 &&
                ((rvec.Rows == 1 && rvec.Cols == 3) ||
                 (rvec.Rows == 3 && rvec.Cols == 1));
            bool channelVector = rvec.Channels == 3 && rvec.Rows == 1 && rvec.Cols == 1;
            if (!matrix && !scalarVector && !channelVector)
            {
                throw new ArgumentException(
                    "Rotation must be a 3 x 3 matrix or a three-value vector.",
                    parameterName);
            }
        }

        private static void ValidateProjectPointsTranslation(Mat tvec, string parameterName)
        {
            if (tvec.Empty)
            {
                throw new ArgumentException("Translation vector cannot be empty.", parameterName);
            }
            ValidateCameraUtilityFloatingDepth(tvec, parameterName);

            bool scalarVector = tvec.Channels == 1 &&
                ((tvec.Rows == 1 && tvec.Cols == 3) ||
                 (tvec.Rows == 3 && tvec.Cols == 1));
            bool channelVector = tvec.Channels == 3 && tvec.Rows == 1 && tvec.Cols == 1;
            if (!scalarVector && !channelVector)
            {
                throw new ArgumentException(
                    "Translation must be a three-value vector.",
                    parameterName);
            }
        }

        private static int ValidateProjectPointsDistortion(Mat distCoeffs)
        {
            if (distCoeffs.Empty)
            {
                return 5;
            }
            ValidateCameraUtilityFloatingDepth(distCoeffs, nameof(distCoeffs));
            if (distCoeffs.Rows != 1 && distCoeffs.Cols != 1)
            {
                throw new ArgumentException(
                    "Distortion coefficients must use a single-vector layout.",
                    nameof(distCoeffs));
            }

            int count;
            try
            {
                count = checked(distCoeffs.Rows * distCoeffs.Cols * distCoeffs.Channels);
            }
            catch (OverflowException exception)
            {
                throw new ArgumentException(
                    "Distortion coefficient count exceeds the supported range.",
                    nameof(distCoeffs),
                    exception);
            }

            if (count != 4 && count != 5 && count != 8 && count != 12 && count != 14)
            {
                throw new ArgumentException(
                    "Distortion coefficients must contain 4, 5, 8, 12, or 14 values.",
                    nameof(distCoeffs));
            }
            return count;
        }

        private static void ValidateProjectPointsAspectRatio(double aspectRatio)
        {
            if (double.IsNaN(aspectRatio) || double.IsInfinity(aspectRatio))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(aspectRatio),
                    "Aspect ratio must be finite.");
            }
        }

        private static void ValidateProjectPointsDerivativeDimensions(int pointCount)
        {
            try
            {
                checked
                {
                    _ = pointCount * 2;
                    _ = pointCount * 3;
                }
            }
            catch (OverflowException exception)
            {
                throw new ArgumentException(
                    "ProjectPoints derivative dimensions exceed the supported range.",
                    nameof(pointCount),
                    exception);
            }
        }

        private static void ValidateProjectPointsDerivativeOutputs(
            Mat[] inputs,
            Mat[] outputs,
            string[] outputNames)
        {
            var inputHandles = new IntPtr[inputs.Length];
            for (int index = 0; index < inputs.Length; index++)
            {
                inputHandles[index] = inputs[index].NativeHandle;
            }

            var outputHandles = new IntPtr[outputs.Length];
            for (int index = 0; index < outputs.Length; index++)
            {
                outputHandles[index] = outputs[index].NativeHandle;
                for (int inputIndex = 0; inputIndex < inputs.Length; inputIndex++)
                {
                    if (ProjectPointsDerivativeMatsAlias(
                        outputs[index],
                        outputHandles[index],
                        inputs[inputIndex],
                        inputHandles[inputIndex]))
                    {
                        throw new ArgumentException(
                            "ProjectPoints derivative outputs must not alias any input matrix.",
                            outputNames[index]);
                    }
                }

                for (int previous = 0; previous < index; previous++)
                {
                    if (ProjectPointsDerivativeMatsAlias(
                        outputs[index],
                        outputHandles[index],
                        outputs[previous],
                        outputHandles[previous]))
                    {
                        throw new ArgumentException(
                            "ProjectPoints derivative outputs must not alias each other.",
                            outputNames[index]);
                    }
                }
            }
        }

        private static void ValidateProjectPointsOutputs(
            Mat[] inputs,
            Mat imagePoints,
            Mat? jacobian)
        {
            var inputHandles = new IntPtr[inputs.Length];
            for (int index = 0; index < inputs.Length; index++)
            {
                inputHandles[index] = inputs[index].NativeHandle;
            }

            IntPtr imagePointsHandle = imagePoints.NativeHandle;
            for (int inputIndex = 0; inputIndex < inputs.Length; inputIndex++)
            {
                if (ProjectPointsDerivativeMatsAlias(
                    imagePoints,
                    imagePointsHandle,
                    inputs[inputIndex],
                    inputHandles[inputIndex]))
                {
                    throw new ArgumentException(
                        "ProjectPoints image output must not alias any input matrix.",
                        nameof(imagePoints));
                }
            }

            if (jacobian is null)
            {
                return;
            }

            IntPtr jacobianHandle = jacobian.NativeHandle;
            for (int inputIndex = 0; inputIndex < inputs.Length; inputIndex++)
            {
                if (ProjectPointsDerivativeMatsAlias(
                    jacobian,
                    jacobianHandle,
                    inputs[inputIndex],
                    inputHandles[inputIndex]))
                {
                    throw new ArgumentException(
                        "ProjectPoints jacobian output must not alias any input matrix.",
                        nameof(jacobian));
                }
            }

            if (ProjectPointsDerivativeMatsAlias(
                jacobian,
                jacobianHandle,
                imagePoints,
                imagePointsHandle))
            {
                throw new ArgumentException(
                    "ProjectPoints outputs must not alias each other.",
                    nameof(jacobian));
            }
        }

        private static bool ProjectPointsDerivativeMatsAlias(
            Mat first,
            IntPtr firstHandle,
            Mat second,
            IntPtr secondHandle)
        {
            return ReferenceEquals(first, second) || firstHandle == secondHandle;
        }

        private static void DisposeProjectPointsDerivativeOutputs(params Mat[] outputs)
        {
            for (int index = 0; index < outputs.Length; index++)
            {
                outputs[index].Dispose();
            }
        }
    }
}

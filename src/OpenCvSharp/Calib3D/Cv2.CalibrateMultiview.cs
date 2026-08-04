using System;
using System.Collections.Generic;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Calib3D
{
    public static unsafe partial class Cv2
    {
        private static readonly TermCriteria DefaultMultiviewCalibrationCriteria =
            new TermCriteria(TermCriteriaTypes.CountOrEps, 100, 2.2204460492503131E-16);

        /// <summary>
        /// Calibrates a fixed multi-camera system.
        /// 标定固定的多相机系统。
        /// </summary>
        public static double CalibrateMultiview(
            Point3f[][] objectPoints,
            Point2f[][][] imagePoints,
            Size[] imageSizes,
            bool[][] detectionMask,
            CameraModel[] cameraModels,
            Mat[] cameraMatrices,
            Mat[] distCoeffs,
            Mat[] rotationVectors,
            Mat[] translationVectors,
            CalibrationFlags[]? flagsForIntrinsics = null,
            CalibrationFlags flags = CalibrationFlags.None,
            TermCriteria? criteria = null)
        {
            return CalibrateMultiviewCore(
                objectPoints,
                imagePoints,
                imageSizes,
                detectionMask,
                cameraModels,
                cameraMatrices,
                distCoeffs,
                rotationVectors,
                translationVectors,
                null,
                null,
                null,
                null,
                flagsForIntrinsics,
                flags,
                criteria,
                false);
        }

        /// <summary>
        /// Calibrates a fixed multi-camera system and returns owned outputs.
        /// 标定固定多相机系统并返回拥有所有权的输出。
        /// </summary>
        /// <remarks>The caller must dispose every matrix in every returned array. 调用方必须释放返回数组中的每个矩阵。</remarks>
        public static MultiviewCalibrationResult CalibrateMultiview(
            Point3f[][] objectPoints,
            Point2f[][][] imagePoints,
            Size[] imageSizes,
            bool[][] detectionMask,
            CameraModel[] cameraModels,
            CalibrationFlags[]? flagsForIntrinsics = null,
            CalibrationFlags flags = CalibrationFlags.None,
            TermCriteria? criteria = null)
        {
            RejectOwnedMultiviewGuesses(flags);
            int cameraCount = GetMultiviewCameraCount(imagePoints);
            Mat[] cameraMatrices = CreateOwnedMultiviewMats(cameraCount);
            Mat[] distCoeffs = CreateOwnedMultiviewMats(cameraCount);
            Mat[] rotationVectors = CreateOwnedMultiviewMats(cameraCount);
            Mat[] translationVectors = CreateOwnedMultiviewMats(cameraCount);
            try
            {
                double reprojectionError = CalibrateMultiview(
                    objectPoints,
                    imagePoints,
                    imageSizes,
                    detectionMask,
                    cameraModels,
                    cameraMatrices,
                    distCoeffs,
                    rotationVectors,
                    translationVectors,
                    flagsForIntrinsics,
                    flags,
                    criteria);
                return new MultiviewCalibrationResult(
                    reprojectionError,
                    cameraMatrices,
                    distCoeffs,
                    rotationVectors,
                    translationVectors);
            }
            catch
            {
                DisposeMultiviewMats(cameraMatrices);
                DisposeMultiviewMats(distCoeffs);
                DisposeMultiviewMats(rotationVectors);
                DisposeMultiviewMats(translationVectors);
                throw;
            }
        }

        /// <summary>
        /// Calibrates a fixed multi-camera system and writes extended outputs.
        /// 标定固定多相机系统并写入扩展输出。
        /// </summary>
        public static double CalibrateMultiviewExtended(
            Point3f[][] objectPoints,
            Point2f[][][] imagePoints,
            Size[] imageSizes,
            bool[][] detectionMask,
            CameraModel[] cameraModels,
            Mat[] cameraMatrices,
            Mat[] distCoeffs,
            Mat[] rotationVectors,
            Mat[] translationVectors,
            Mat initializationPairs,
            Mat[] rvecs0,
            Mat[] tvecs0,
            Mat perFrameErrors,
            CalibrationFlags[]? flagsForIntrinsics = null,
            CalibrationFlags flags = CalibrationFlags.None,
            TermCriteria? criteria = null)
        {
            return CalibrateMultiviewCore(
                objectPoints,
                imagePoints,
                imageSizes,
                detectionMask,
                cameraModels,
                cameraMatrices,
                distCoeffs,
                rotationVectors,
                translationVectors,
                initializationPairs,
                rvecs0,
                tvecs0,
                perFrameErrors,
                flagsForIntrinsics,
                flags,
                criteria,
                true);
        }

        /// <summary>
        /// Calibrates a fixed multi-camera system and returns owned extended outputs.
        /// 标定固定多相机系统并返回拥有所有权的扩展输出。
        /// </summary>
        /// <remarks>The caller must dispose every matrix in every returned array. 调用方必须释放返回数组中的每个矩阵。</remarks>
        public static MultiviewCalibrationExtendedResult CalibrateMultiviewExtended(
            Point3f[][] objectPoints,
            Point2f[][][] imagePoints,
            Size[] imageSizes,
            bool[][] detectionMask,
            CameraModel[] cameraModels,
            CalibrationFlags[]? flagsForIntrinsics = null,
            CalibrationFlags flags = CalibrationFlags.None,
            TermCriteria? criteria = null)
        {
            RejectOwnedMultiviewGuesses(flags);
            if (objectPoints == null)
            {
                throw new ArgumentNullException(nameof(objectPoints));
            }
            int cameraCount = GetMultiviewCameraCount(imagePoints);
            int frameCount = objectPoints.Length;
            Mat[] cameraMatrices = CreateOwnedMultiviewMats(cameraCount);
            Mat[] distCoeffs = CreateOwnedMultiviewMats(cameraCount);
            Mat[] rotationVectors = CreateOwnedMultiviewMats(cameraCount);
            Mat[] translationVectors = CreateOwnedMultiviewMats(cameraCount);
            var initializationPairs = new Mat();
            Mat[] rvecs0 = CreateOwnedMultiviewMats(frameCount);
            Mat[] tvecs0 = CreateOwnedMultiviewMats(frameCount);
            var perFrameErrors = new Mat();
            try
            {
                double reprojectionError = CalibrateMultiviewExtended(
                    objectPoints,
                    imagePoints,
                    imageSizes,
                    detectionMask,
                    cameraModels,
                    cameraMatrices,
                    distCoeffs,
                    rotationVectors,
                    translationVectors,
                    initializationPairs,
                    rvecs0,
                    tvecs0,
                    perFrameErrors,
                    flagsForIntrinsics,
                    flags,
                    criteria);
                var calibration = new MultiviewCalibrationResult(
                    reprojectionError,
                    cameraMatrices,
                    distCoeffs,
                    rotationVectors,
                    translationVectors);
                return new MultiviewCalibrationExtendedResult(
                    calibration,
                    initializationPairs,
                    rvecs0,
                    tvecs0,
                    perFrameErrors);
            }
            catch
            {
                DisposeMultiviewMats(cameraMatrices);
                DisposeMultiviewMats(distCoeffs);
                DisposeMultiviewMats(rotationVectors);
                DisposeMultiviewMats(translationVectors);
                initializationPairs.Dispose();
                DisposeMultiviewMats(rvecs0);
                DisposeMultiviewMats(tvecs0);
                perFrameErrors.Dispose();
                throw;
            }
        }

        private static double CalibrateMultiviewCore(
            Point3f[][] objectPoints,
            Point2f[][][] imagePoints,
            Size[] imageSizes,
            bool[][] detectionMask,
            CameraModel[] cameraModels,
            Mat[] cameraMatrices,
            Mat[] distCoeffs,
            Mat[] rotationVectors,
            Mat[] translationVectors,
            Mat? initializationPairs,
            Mat[]? rvecs0,
            Mat[]? tvecs0,
            Mat? perFrameErrors,
            CalibrationFlags[]? flagsForIntrinsics,
            CalibrationFlags flags,
            TermCriteria? criteria,
            bool extended)
        {
            PrepareMultiviewInputs(
                objectPoints,
                imagePoints,
                imageSizes,
                detectionMask,
                cameraModels,
                flagsForIntrinsics,
                flags,
                out int[] objectOffsets,
                out NativeMethods.Calib3DPoint3fNative[] nativeObjectPoints,
                out int[] imageOffsets,
                out NativeMethods.Calib3DPoint2fNative[] nativeImagePoints,
                out int[] imageWidths,
                out int[] imageHeights,
                out byte[] nativeDetectionMask,
                out int[] nativeCameraModels,
                out int[] nativeIntrinsicFlags);

            int cameraCount = imagePoints.Length;
            int frameCount = objectPoints.Length;
            IntPtr[] cameraMatrixHandles = CreateMultiviewHandles(cameraMatrices, cameraCount, nameof(cameraMatrices));
            IntPtr[] distCoeffHandles = CreateMultiviewHandles(distCoeffs, cameraCount, nameof(distCoeffs));
            IntPtr[] rotationHandles = CreateMultiviewHandles(rotationVectors, cameraCount, nameof(rotationVectors));
            IntPtr[] translationHandles = CreateMultiviewHandles(translationVectors, cameraCount, nameof(translationVectors));
            ValidateMultiviewGuessInputs(
                cameraMatrices,
                distCoeffs,
                rotationVectors,
                translationVectors,
                flags);

            IntPtr[] frameRotationHandles = Array.Empty<IntPtr>();
            IntPtr[] frameTranslationHandles = Array.Empty<IntPtr>();
            if (extended)
            {
                ThrowIfNull(initializationPairs, nameof(initializationPairs));
                ThrowIfNull(perFrameErrors, nameof(perFrameErrors));
                frameRotationHandles = CreateMultiviewHandles(rvecs0!, frameCount, nameof(rvecs0));
                frameTranslationHandles = CreateMultiviewHandles(tvecs0!, frameCount, nameof(tvecs0));
            }

            TermCriteria resolved = criteria ?? DefaultMultiviewCalibrationCriteria;
            ValidateRegistrationCriteria(resolved, nameof(criteria));

            fixed (int* objectOffsetsPtr = objectOffsets)
            fixed (NativeMethods.Calib3DPoint3fNative* objectPointsPtr = nativeObjectPoints)
            fixed (int* imageOffsetsPtr = imageOffsets)
            fixed (NativeMethods.Calib3DPoint2fNative* imagePointsPtr = nativeImagePoints)
            fixed (int* imageWidthsPtr = imageWidths)
            fixed (int* imageHeightsPtr = imageHeights)
            fixed (byte* detectionMaskPtr = nativeDetectionMask)
            fixed (int* cameraModelsPtr = nativeCameraModels)
            fixed (IntPtr* cameraMatricesPtr = cameraMatrixHandles)
            fixed (IntPtr* distCoeffsPtr = distCoeffHandles)
            fixed (IntPtr* rotationVectorsPtr = rotationHandles)
            fixed (IntPtr* translationVectorsPtr = translationHandles)
            fixed (int* intrinsicFlagsPtr = nativeIntrinsicFlags)
            fixed (IntPtr* rvecs0Ptr = frameRotationHandles)
            fixed (IntPtr* tvecs0Ptr = frameTranslationHandles)
            {
                int status = extended
                    ? NativeMethods.Calib3DCalibrateMultiviewExtended(
                        objectOffsetsPtr,
                        frameCount,
                        objectPointsPtr,
                        nativeObjectPoints.Length,
                        imageOffsetsPtr,
                        cameraCount,
                        frameCount,
                        imagePointsPtr,
                        nativeImagePoints.Length,
                        imageWidthsPtr,
                        imageHeightsPtr,
                        detectionMaskPtr,
                        cameraModelsPtr,
                        cameraMatricesPtr,
                        distCoeffsPtr,
                        rotationVectorsPtr,
                        translationVectorsPtr,
                        initializationPairs!.NativeHandle,
                        rvecs0Ptr,
                        tvecs0Ptr,
                        perFrameErrors!.NativeHandle,
                        intrinsicFlagsPtr,
                        (int)flags,
                        (int)resolved.Type,
                        resolved.MaxCount,
                        resolved.Epsilon,
                        out double reprojectionError)
                    : NativeMethods.Calib3DCalibrateMultiview(
                        objectOffsetsPtr,
                        frameCount,
                        objectPointsPtr,
                        nativeObjectPoints.Length,
                        imageOffsetsPtr,
                        cameraCount,
                        frameCount,
                        imagePointsPtr,
                        nativeImagePoints.Length,
                        imageWidthsPtr,
                        imageHeightsPtr,
                        detectionMaskPtr,
                        cameraModelsPtr,
                        cameraMatricesPtr,
                        distCoeffsPtr,
                        rotationVectorsPtr,
                        translationVectorsPtr,
                        intrinsicFlagsPtr,
                        (int)flags,
                        (int)resolved.Type,
                        resolved.MaxCount,
                        resolved.Epsilon,
                        out reprojectionError);
                NativeException.ThrowIfError(status);
                return reprojectionError;
            }
        }

        private static void PrepareMultiviewInputs(
            Point3f[][] objectPoints,
            Point2f[][][] imagePoints,
            Size[] imageSizes,
            bool[][] detectionMask,
            CameraModel[] cameraModels,
            CalibrationFlags[]? flagsForIntrinsics,
            CalibrationFlags flags,
            out int[] objectOffsets,
            out NativeMethods.Calib3DPoint3fNative[] nativeObjectPoints,
            out int[] imageOffsets,
            out NativeMethods.Calib3DPoint2fNative[] nativeImagePoints,
            out int[] imageWidths,
            out int[] imageHeights,
            out byte[] nativeDetectionMask,
            out int[] nativeCameraModels,
            out int[] nativeIntrinsicFlags)
        {
            ThrowIfNull(objectPoints, nameof(objectPoints));
            ThrowIfNull(imagePoints, nameof(imagePoints));
            ThrowIfNull(imageSizes, nameof(imageSizes));
            ThrowIfNull(detectionMask, nameof(detectionMask));
            ThrowIfNull(cameraModels, nameof(cameraModels));

            int frameCount = objectPoints.Length;
            int cameraCount = imagePoints.Length;
            if (frameCount == 0)
            {
                throw new ArgumentException("At least one frame is required.", nameof(objectPoints));
            }
            if (cameraCount < 2)
            {
                throw new ArgumentException("At least two cameras are required.", nameof(imagePoints));
            }
            if (imageSizes.Length != cameraCount)
            {
                throw new ArgumentException("Image-size count must match the camera count.", nameof(imageSizes));
            }
            if (detectionMask.Length != cameraCount)
            {
                throw new ArgumentException("Detection-mask row count must match the camera count.", nameof(detectionMask));
            }
            if (cameraModels.Length != cameraCount)
            {
                throw new ArgumentException("Camera-model count must match the camera count.", nameof(cameraModels));
            }

            for (int frame = 0; frame < frameCount; ++frame)
            {
                if (objectPoints[frame] == null || objectPoints[frame].Length == 0)
                {
                    throw new ArgumentException("Object-point frame groups cannot be null or empty.", nameof(objectPoints));
                }
            }

            PointSetMarshaller.FlattenPoint3fGroups(
                objectPoints,
                nameof(objectPoints),
                out objectOffsets,
                out Point3f[] flatObjectPoints);
            nativeObjectPoints = ToNativePoint3fArray(flatObjectPoints);

            var flatImagePoints = new List<Point2f>();
            imageOffsets = new int[cameraCount * frameCount + 1];
            nativeDetectionMask = new byte[cameraCount * frameCount];
            imageWidths = new int[cameraCount];
            imageHeights = new int[cameraCount];
            nativeCameraModels = new int[cameraCount];
            int groupIndex = 0;
            for (int camera = 0; camera < cameraCount; ++camera)
            {
                if (imagePoints[camera] == null || imagePoints[camera].Length != frameCount)
                {
                    throw new ArgumentException("Each camera must contain one image-point group per frame.", nameof(imagePoints));
                }
                if (detectionMask[camera] == null || detectionMask[camera].Length != frameCount)
                {
                    throw new ArgumentException("Each detection-mask row must match the frame count.", nameof(detectionMask));
                }
                ValidatePositiveSize(imageSizes[camera], nameof(imageSizes));
                ValidateCameraModel(cameraModels[camera], nameof(cameraModels));
                imageWidths[camera] = imageSizes[camera].Width;
                imageHeights[camera] = imageSizes[camera].Height;
                nativeCameraModels[camera] = (int)cameraModels[camera];

                int visibleFrames = 0;
                for (int frame = 0; frame < frameCount; ++frame)
                {
                    Point2f[] group = imagePoints[camera][frame];
                    if (group == null)
                    {
                        throw new ArgumentException("Image-point groups cannot be null.", nameof(imagePoints));
                    }
                    bool visible = detectionMask[camera][frame];
                    nativeDetectionMask[camera * frameCount + frame] = visible ? (byte)1 : (byte)0;
                    if (visible)
                    {
                        ++visibleFrames;
                        if (group.Length != objectPoints[frame].Length)
                        {
                            throw new ArgumentException(
                                "Visible image-point groups must match the object-point count.",
                                nameof(imagePoints));
                        }
                    }
                    else if (group.Length != 0 && group.Length != objectPoints[frame].Length)
                    {
                        throw new ArgumentException(
                            "Invisible image-point groups must be empty or match the object-point count.",
                            nameof(imagePoints));
                    }

                    flatImagePoints.AddRange(group);
                    imageOffsets[++groupIndex] = flatImagePoints.Count;
                }
                if (visibleFrames == 0)
                {
                    throw new ArgumentException("Every camera must observe at least one frame.", nameof(detectionMask));
                }
            }
            nativeImagePoints = ToNativePoint2fArray(flatImagePoints.ToArray());
            ValidateMultiviewConnectivity(detectionMask, cameraCount, frameCount);
            ValidateMultiviewCommonFlags(flags, cameraModels);

            CalibrationFlags[] resolvedIntrinsicFlags = flagsForIntrinsics ?? new CalibrationFlags[cameraCount];
            if (resolvedIntrinsicFlags.Length != cameraCount)
            {
                throw new ArgumentException("Intrinsic-flag count must match the camera count.", nameof(flagsForIntrinsics));
            }
            nativeIntrinsicFlags = new int[cameraCount];
            for (int camera = 0; camera < cameraCount; ++camera)
            {
                ValidateKnownCalibrationFlags(resolvedIntrinsicFlags[camera], nameof(flagsForIntrinsics));
                nativeIntrinsicFlags[camera] = (int)resolvedIntrinsicFlags[camera];
            }
        }

        private static void ValidateMultiviewConnectivity(bool[][] mask, int cameraCount, int frameCount)
        {
            var visited = new bool[cameraCount];
            var pending = new Stack<int>();
            visited[0] = true;
            pending.Push(0);
            while (pending.Count > 0)
            {
                int current = pending.Pop();
                for (int candidate = 0; candidate < cameraCount; ++candidate)
                {
                    if (visited[candidate] || candidate == current)
                    {
                        continue;
                    }
                    bool overlaps = false;
                    for (int frame = 0; frame < frameCount; ++frame)
                    {
                        if (mask[current][frame] && mask[candidate][frame])
                        {
                            overlaps = true;
                            break;
                        }
                    }
                    if (overlaps)
                    {
                        visited[candidate] = true;
                        pending.Push(candidate);
                    }
                }
            }
            for (int camera = 0; camera < cameraCount; ++camera)
            {
                if (!visited[camera])
                {
                    throw new ArgumentException("The camera visibility graph must be connected.", nameof(mask));
                }
            }
        }

        private static void ValidateMultiviewCommonFlags(CalibrationFlags flags, CameraModel[] cameraModels)
        {
            const CalibrationFlags supported =
                CalibrationFlags.UseIntrinsicGuess |
                CalibrationFlags.UseExtrinsicGuess |
                CalibrationFlags.StereoRegistration;
            if ((flags & ~supported) != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(flags), "Unsupported multiview calibration flag.");
            }
            if ((flags & CalibrationFlags.StereoRegistration) != 0)
            {
                CameraModel first = cameraModels[0];
                for (int i = 1; i < cameraModels.Length; ++i)
                {
                    if (cameraModels[i] != first)
                    {
                        throw new ArgumentException(
                            "StereoRegistration does not support mixed camera models.",
                            nameof(cameraModels));
                    }
                }
            }
        }

        private static void ValidateKnownCalibrationFlags(CalibrationFlags flags, string parameterName)
        {
            const CalibrationFlags known =
                CalibrationFlags.UseIntrinsicGuess |
                CalibrationFlags.FixAspectRatio |
                CalibrationFlags.FixPrincipalPoint |
                CalibrationFlags.ZeroTangentDist |
                CalibrationFlags.FixFocalLength |
                CalibrationFlags.FixK1 |
                CalibrationFlags.FixK2 |
                CalibrationFlags.FixK3 |
                CalibrationFlags.FixIntrinsic |
                CalibrationFlags.SameFocalLength |
                CalibrationFlags.ZeroDisparity |
                CalibrationFlags.FixK4 |
                CalibrationFlags.FixK5 |
                CalibrationFlags.FixK6 |
                CalibrationFlags.RationalModel |
                CalibrationFlags.ThinPrismModel |
                CalibrationFlags.FixS1S2S3S4 |
                CalibrationFlags.UseLU |
                CalibrationFlags.DisableSchurComplement |
                CalibrationFlags.TiltedModel |
                CalibrationFlags.FixTauXTauY |
                CalibrationFlags.UseQR |
                CalibrationFlags.FixTangentDist |
                CalibrationFlags.UseExtrinsicGuess |
                CalibrationFlags.RecomputeExtrinsic |
                CalibrationFlags.CheckCond |
                CalibrationFlags.FixSkew |
                CalibrationFlags.StereoRegistration;
            if ((flags & ~known) != 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Calibration flags contain an unknown bit.");
            }
        }

        private static IntPtr[] CreateMultiviewHandles(Mat[] values, int expectedCount, string parameterName)
        {
            if (values == null)
            {
                throw new ArgumentNullException(parameterName);
            }
            if (values.Length != expectedCount)
            {
                throw new ArgumentException("Matrix array length is invalid.", parameterName);
            }
            var handles = new IntPtr[expectedCount];
            for (int i = 0; i < expectedCount; ++i)
            {
                if (values[i] == null)
                {
                    throw new ArgumentNullException(parameterName);
                }
                handles[i] = values[i].NativeHandle;
            }
            return handles;
        }

        private static void ValidateMultiviewGuessInputs(
            Mat[] cameraMatrices,
            Mat[] distCoeffs,
            Mat[] rotationVectors,
            Mat[] translationVectors,
            CalibrationFlags flags)
        {
            if ((flags & CalibrationFlags.UseIntrinsicGuess) != 0)
            {
                for (int i = 0; i < cameraMatrices.Length; ++i)
                {
                    if (cameraMatrices[i].Rows != 3 || cameraMatrices[i].Cols != 3 || distCoeffs[i].Empty)
                    {
                        throw new ArgumentException(
                            "UseIntrinsicGuess requires non-empty 3 x 3 camera matrices and distortion matrices.",
                            nameof(cameraMatrices));
                    }
                }
            }
            if ((flags & CalibrationFlags.UseExtrinsicGuess) != 0)
            {
                for (int i = 0; i < rotationVectors.Length; ++i)
                {
                    if (rotationVectors[i].Rows != 3 || rotationVectors[i].Cols != 1 ||
                        translationVectors[i].Rows != 3 || translationVectors[i].Cols != 1)
                    {
                        throw new ArgumentException(
                            "UseExtrinsicGuess requires 3 x 1 rotation and translation vectors.",
                            nameof(rotationVectors));
                    }
                }
            }
        }

        private static void RejectOwnedMultiviewGuesses(CalibrationFlags flags)
        {
            if ((flags & (CalibrationFlags.UseIntrinsicGuess | CalibrationFlags.UseExtrinsicGuess)) != 0)
            {
                throw new ArgumentException(
                    "Use caller-owned overloads to provide intrinsic or extrinsic guesses.",
                    nameof(flags));
            }
        }

        private static int GetMultiviewCameraCount(Point2f[][][] imagePoints)
        {
            if (imagePoints == null)
            {
                throw new ArgumentNullException(nameof(imagePoints));
            }
            return imagePoints.Length;
        }

        private static Mat[] CreateOwnedMultiviewMats(int count)
        {
            var result = new Mat[count];
            int created = 0;
            try
            {
                for (; created < count; ++created)
                {
                    result[created] = new Mat();
                }
                return result;
            }
            catch
            {
                for (int i = 0; i < created; ++i)
                {
                    result[i].Dispose();
                }
                throw;
            }
        }

        private static void DisposeMultiviewMats(Mat[] values)
        {
            for (int i = 0; i < values.Length; ++i)
            {
                values[i]?.Dispose();
            }
        }
    }
}

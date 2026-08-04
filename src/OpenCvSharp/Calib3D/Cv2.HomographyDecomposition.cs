using System;
using System.Collections.Generic;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Calib3D
{
    public static partial class Cv2
    {
        private const int HomographyDecompositionCapacity = 4;

        /// <summary>
        /// Decomposes a planar homography into rotation, normalized translation, and plane-normal solutions.
        /// 将平面单应矩阵分解为旋转、归一化平移和法向解。
        /// </summary>
        /// <param name="homography">The <c>3 x 3</c> homography matrix. <c>3 x 3</c> 单应矩阵。</param>
        /// <param name="cameraMatrix">The <c>3 x 3</c> camera intrinsic matrix. <c>3 x 3</c> 相机内参矩阵。</param>
        /// <param name="rotations">At least four caller-owned output Mats for <c>3 x 3 CV_64F</c> rotations. 至少四个调用方持有的 <c>3 x 3 CV_64F</c> 旋转输出。</param>
        /// <param name="translations">At least four caller-owned output Mats for <c>3 x 1 CV_64F</c> normalized translations. 至少四个调用方持有的 <c>3 x 1 CV_64F</c> 归一化平移输出。</param>
        /// <param name="normals">At least four caller-owned output Mats for <c>3 x 1 CV_64F</c> plane normals. 至少四个调用方持有的 <c>3 x 1 CV_64F</c> 平面法向输出。</param>
        /// <returns>The number of solutions written to the beginning of each output array. 写入每个输出数组起始位置的解数量。</returns>
        public static unsafe int DecomposeHomographyMat(
            Mat homography,
            Mat cameraMatrix,
            Mat[] rotations,
            Mat[] translations,
            Mat[] normals)
        {
            ValidateHomographyMatrix(homography, nameof(homography));
            ValidateHomographyMatrix(cameraMatrix, nameof(cameraMatrix));

            IntPtr[] rotationHandles = CreateHomographyOutputHandles(
                rotations,
                nameof(rotations));
            IntPtr[] translationHandles = CreateHomographyOutputHandles(
                translations,
                nameof(translations));
            IntPtr[] normalHandles = CreateHomographyOutputHandles(
                normals,
                nameof(normals));
            ValidateDistinctHomographyOutputs(
                rotationHandles,
                translationHandles,
                normalHandles);

            fixed (IntPtr* rotationsPtr = rotationHandles)
            fixed (IntPtr* translationsPtr = translationHandles)
            fixed (IntPtr* normalsPtr = normalHandles)
            {
                NativeException.ThrowIfError(
                    NativeMethods.Calib3DDecomposeHomographyMat(
                        homography.NativeHandle,
                        cameraMatrix.NativeHandle,
                        rotationsPtr,
                        translationsPtr,
                        normalsPtr,
                        HomographyDecompositionCapacity,
                        out int solutionCount));
                if (solutionCount < 0 ||
                    solutionCount > HomographyDecompositionCapacity)
                {
                    throw new OpenCvException(
                        "Native homography decomposition returned an invalid solution count.");
                }
                return solutionCount;
            }
        }

        /// <summary>
        /// Decomposes a planar homography and returns exact-length owned solution arrays.
        /// 分解平面单应矩阵并返回精确长度、拥有所有权的解数组。
        /// </summary>
        public static int DecomposeHomographyMat(
            Mat homography,
            Mat cameraMatrix,
            out Mat[] rotations,
            out Mat[] translations,
            out Mat[] normals)
        {
            rotations = Array.Empty<Mat>();
            translations = Array.Empty<Mat>();
            normals = Array.Empty<Mat>();

            Mat[] allocatedRotations = Array.Empty<Mat>();
            Mat[] allocatedTranslations = Array.Empty<Mat>();
            Mat[] allocatedNormals = Array.Empty<Mat>();
            try
            {
                allocatedRotations = CreateOwnedHomographyMats();
                allocatedTranslations = CreateOwnedHomographyMats();
                allocatedNormals = CreateOwnedHomographyMats();

                int solutionCount = DecomposeHomographyMat(
                    homography,
                    cameraMatrix,
                    allocatedRotations,
                    allocatedTranslations,
                    allocatedNormals);

                var exactRotations = new Mat[solutionCount];
                var exactTranslations = new Mat[solutionCount];
                var exactNormals = new Mat[solutionCount];
                Array.Copy(allocatedRotations, exactRotations, solutionCount);
                Array.Copy(allocatedTranslations, exactTranslations, solutionCount);
                Array.Copy(allocatedNormals, exactNormals, solutionCount);

                DisposeHomographyMats(
                    allocatedRotations,
                    solutionCount);
                DisposeHomographyMats(
                    allocatedTranslations,
                    solutionCount);
                DisposeHomographyMats(
                    allocatedNormals,
                    solutionCount);

                rotations = exactRotations;
                translations = exactTranslations;
                normals = exactNormals;
                return solutionCount;
            }
            catch
            {
                DisposeHomographyMats(allocatedRotations, 0);
                DisposeHomographyMats(allocatedTranslations, 0);
                DisposeHomographyMats(allocatedNormals, 0);
                throw;
            }
        }

        /// <summary>
        /// Filters homography decomposition solutions using visible rectified reference points.
        /// 使用可见的校正参考点筛选单应分解解。
        /// </summary>
        /// <param name="rotations">The rotation solution matrices. 旋转解矩阵。</param>
        /// <param name="normals">The plane-normal solution matrices. 平面法向解矩阵。</param>
        /// <param name="beforePoints">Rectified points before applying the homography, as a <c>CV_32FC2</c> vector. 应用单应矩阵前的校正点，类型为 <c>CV_32FC2</c> 向量。</param>
        /// <param name="afterPoints">Rectified points after applying the homography, as a <c>CV_32FC2</c> vector. 应用单应矩阵后的校正点，类型为 <c>CV_32FC2</c> 向量。</param>
        /// <param name="possibleSolutions">The caller-owned <c>CV_32S</c> solution-index vector. 调用方持有的 <c>CV_32S</c> 解索引向量。</param>
        /// <param name="pointsMask">An optional single-channel <c>CV_8U</c> or <c>CV_8S</c> point mask. 可选的单通道 <c>CV_8U</c> 或 <c>CV_8S</c> 点掩码。</param>
        public static unsafe void FilterHomographyDecompByVisibleRefpoints(
            Mat[] rotations,
            Mat[] normals,
            Mat beforePoints,
            Mat afterPoints,
            Mat possibleSolutions,
            Mat? pointsMask = null)
        {
            IntPtr[] rotationHandles = CreateHomographySolutionHandles(
                rotations,
                true,
                nameof(rotations));
            IntPtr[] normalHandles = CreateHomographySolutionHandles(
                normals,
                false,
                nameof(normals));
            if (rotationHandles.Length != normalHandles.Length)
            {
                throw new ArgumentException(
                    "Rotation and normal solution counts must match.",
                    nameof(normals));
            }

            int pointCount = ValidateHomographyVisiblePoints(
                beforePoints,
                nameof(beforePoints));
            int afterPointCount = ValidateHomographyVisiblePoints(
                afterPoints,
                nameof(afterPoints));
            if (pointCount != afterPointCount)
            {
                throw new ArgumentException(
                    "Before and after point counts must match.",
                    nameof(afterPoints));
            }
            ValidateHomographyPointMask(
                pointsMask,
                pointCount,
                nameof(pointsMask));
            ThrowIfNull(possibleSolutions, nameof(possibleSolutions));

            fixed (IntPtr* rotationsPtr = rotationHandles)
            fixed (IntPtr* normalsPtr = normalHandles)
            {
                NativeException.ThrowIfError(
                    NativeMethods.Calib3DFilterHomographyDecompByVisibleRefpoints(
                        rotationsPtr,
                        rotationHandles.Length,
                        normalsPtr,
                        normalHandles.Length,
                        beforePoints.NativeHandle,
                        afterPoints.NativeHandle,
                        possibleSolutions.NativeHandle,
                        GetNativeHandleOrZero(pointsMask)));
            }
        }

        /// <summary>
        /// Filters homography decomposition solutions and returns an owned solution-index Mat.
        /// 筛选单应分解解并返回拥有所有权的解索引 Mat。
        /// </summary>
        public static Mat FilterHomographyDecompByVisibleRefpoints(
            Mat[] rotations,
            Mat[] normals,
            Mat beforePoints,
            Mat afterPoints,
            Mat? pointsMask = null)
        {
            var possibleSolutions = new Mat();
            try
            {
                FilterHomographyDecompByVisibleRefpoints(
                    rotations,
                    normals,
                    beforePoints,
                    afterPoints,
                    possibleSolutions,
                    pointsMask);
                return possibleSolutions;
            }
            catch
            {
                possibleSolutions.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Filters homography decomposition solutions from managed point arrays.
        /// 根据托管点数组筛选单应分解解。
        /// </summary>
        public static Mat FilterHomographyDecompByVisibleRefpoints(
            Mat[] rotations,
            Mat[] normals,
            Point2f[] beforePoints,
            Point2f[] afterPoints,
            Mat? pointsMask = null)
        {
            ValidateHomographyPointArrays(
                beforePoints,
                afterPoints);
            using (Mat beforePointMat = ToPointMat(beforePoints))
            using (Mat afterPointMat = ToPointMat(afterPoints))
            {
                return FilterHomographyDecompByVisibleRefpoints(
                    rotations,
                    normals,
                    beforePointMat,
                    afterPointMat,
                    pointsMask);
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Filters homography decomposition solutions from point spans.
        /// 根据点 Span 筛选单应分解解。
        /// </summary>
        public static Mat FilterHomographyDecompByVisibleRefpoints(
            Mat[] rotations,
            Mat[] normals,
            ReadOnlySpan<Point2f> beforePoints,
            ReadOnlySpan<Point2f> afterPoints,
            Mat? pointsMask = null)
        {
            ValidateHomographyPointSpans(
                beforePoints,
                afterPoints);
            using (Mat beforePointMat = ToPointMat(beforePoints))
            using (Mat afterPointMat = ToPointMat(afterPoints))
            {
                return FilterHomographyDecompByVisibleRefpoints(
                    rotations,
                    normals,
                    beforePointMat,
                    afterPointMat,
                    pointsMask);
            }
        }
#endif

        private static void ValidateHomographyMatrix(
            Mat value,
            string parameterName)
        {
            ThrowIfNull(value, parameterName);
            if (value.Empty)
            {
                throw new ArgumentException(
                    "Matrix cannot be empty.",
                    parameterName);
            }
            if (value.Rows != 3 ||
                value.Cols != 3 ||
                value.Channels != 1)
            {
                throw new ArgumentException(
                    "Matrix must be single-channel 3 x 3.",
                    parameterName);
            }
            if (value.Depth != MatType.CV_32F &&
                value.Depth != MatType.CV_64F)
            {
                throw new ArgumentException(
                    "Matrix depth must be CV_32F or CV_64F.",
                    parameterName);
            }
        }

        private static IntPtr[] CreateHomographyOutputHandles(
            Mat[] values,
            string parameterName)
        {
            if (values == null)
            {
                throw new ArgumentNullException(parameterName);
            }
            if (values.Length < HomographyDecompositionCapacity)
            {
                throw new ArgumentException(
                    "At least four output matrices are required.",
                    parameterName);
            }

            var handles = new IntPtr[HomographyDecompositionCapacity];
            for (int i = 0; i < handles.Length; ++i)
            {
                if (values[i] == null)
                {
                    throw new ArgumentNullException(parameterName);
                }
                handles[i] = values[i].NativeHandle;
            }
            return handles;
        }

        private static void ValidateDistinctHomographyOutputs(
            IntPtr[] rotations,
            IntPtr[] translations,
            IntPtr[] normals)
        {
            var handles = new HashSet<IntPtr>();
            AddDistinctHomographyOutputs(
                handles,
                rotations,
                nameof(rotations));
            AddDistinctHomographyOutputs(
                handles,
                translations,
                nameof(translations));
            AddDistinctHomographyOutputs(
                handles,
                normals,
                nameof(normals));
        }

        private static void AddDistinctHomographyOutputs(
            HashSet<IntPtr> existing,
            IntPtr[] values,
            string parameterName)
        {
            for (int i = 0; i < values.Length; ++i)
            {
                if (!existing.Add(values[i]))
                {
                    throw new ArgumentException(
                        "Homography decomposition output matrices must be distinct.",
                        parameterName);
                }
            }
        }

        private static Mat[] CreateOwnedHomographyMats()
        {
            var result = new Mat[HomographyDecompositionCapacity];
            int created = 0;
            try
            {
                for (; created < result.Length; ++created)
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

        private static void DisposeHomographyMats(
            Mat[] values,
            int firstIndex)
        {
            for (int i = firstIndex; i < values.Length; ++i)
            {
                values[i]?.Dispose();
            }
        }

        private static IntPtr[] CreateHomographySolutionHandles(
            Mat[] values,
            bool rotations,
            string parameterName)
        {
            if (values == null)
            {
                throw new ArgumentNullException(parameterName);
            }
            if (values.Length == 0 ||
                values.Length > HomographyDecompositionCapacity)
            {
                throw new ArgumentException(
                    "One to four homography solutions are required.",
                    parameterName);
            }

            var handles = new IntPtr[values.Length];
            for (int i = 0; i < values.Length; ++i)
            {
                Mat value = values[i] ??
                    throw new ArgumentNullException(parameterName);
                if (value.Empty)
                {
                    throw new ArgumentException(
                        "Solution matrices cannot be empty.",
                        parameterName);
                }
                if (!IsSupportedHomographyNumericDepth(value.Depth) ||
                    value.Channels != 1)
                {
                    throw new ArgumentException(
                        "Solution matrices must be single-channel numeric matrices.",
                        parameterName);
                }
                if (rotations)
                {
                    if (value.Rows != 3 || value.Cols != 3)
                    {
                        throw new ArgumentException(
                            "Rotation solutions must be 3 x 3.",
                            parameterName);
                    }
                }
                else if (value.Rows * value.Cols != 3)
                {
                    throw new ArgumentException(
                        "Normal solutions must contain exactly three elements.",
                        parameterName);
                }

                handles[i] = value.NativeHandle;
            }
            return handles;
        }

        private static bool IsSupportedHomographyNumericDepth(int depth)
        {
            return depth >= MatType.CV_8U &&
                depth <= MatType.CV_16F;
        }

        private static int ValidateHomographyVisiblePoints(
            Mat points,
            string parameterName)
        {
            ThrowIfNull(points, parameterName);
            if (points.Empty)
            {
                throw new ArgumentException(
                    "Point matrix cannot be empty.",
                    parameterName);
            }
            if (points.Type != MatType.CV_32FC2 ||
                (points.Rows != 1 && points.Cols != 1))
            {
                throw new ArgumentException(
                    "Points must be a CV_32FC2 row or column vector.",
                    parameterName);
            }
            return checked(points.Rows * points.Cols);
        }

        private static void ValidateHomographyPointMask(
            Mat? pointsMask,
            int pointCount,
            string parameterName)
        {
            if (pointsMask == null)
            {
                return;
            }
            if (pointsMask.Empty)
            {
                return;
            }
            if (pointsMask.Channels != 1 ||
                (pointsMask.Depth != MatType.CV_8U &&
                 pointsMask.Depth != MatType.CV_8S) ||
                (pointsMask.Rows != 1 && pointsMask.Cols != 1) ||
                checked(pointsMask.Rows * pointsMask.Cols) != pointCount)
            {
                throw new ArgumentException(
                    "Point mask must be a matching CV_8U or CV_8S vector.",
                    parameterName);
            }
        }

        private static void ValidateHomographyPointArrays(
            Point2f[] beforePoints,
            Point2f[] afterPoints)
        {
            if (beforePoints == null)
            {
                throw new ArgumentNullException(nameof(beforePoints));
            }
            if (afterPoints == null)
            {
                throw new ArgumentNullException(nameof(afterPoints));
            }
            if (beforePoints.Length == 0)
            {
                throw new ArgumentException(
                    "Point arrays cannot be empty.",
                    nameof(beforePoints));
            }
            if (beforePoints.Length != afterPoints.Length)
            {
                throw new ArgumentException(
                    "Before and after point counts must match.",
                    nameof(afterPoints));
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        private static void ValidateHomographyPointSpans(
            ReadOnlySpan<Point2f> beforePoints,
            ReadOnlySpan<Point2f> afterPoints)
        {
            if (beforePoints.IsEmpty)
            {
                throw new ArgumentException(
                    "Point spans cannot be empty.",
                    nameof(beforePoints));
            }
            if (beforePoints.Length != afterPoints.Length)
            {
                throw new ArgumentException(
                    "Before and after point counts must match.",
                    nameof(afterPoints));
            }
        }
#endif
    }
}

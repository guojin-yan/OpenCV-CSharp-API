using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.ObjDetect
{
    /// <summary>
    /// ArUco marker detector compatible with OpenCV <c>cv::aruco::ArucoDetector</c>.
    /// 与 OpenCV <c>cv::aruco::ArucoDetector</c> 兼容的 ArUco marker 检测器。
    /// </summary>
    public sealed unsafe class ArucoDetector : IDisposable
    {
        private NativeArucoDetectorHandle handle;
        private bool disposed;

        /// <summary>
        /// Initializes a detector for a predefined dictionary.
        /// 使用预定义字典初始化检测器。
        /// </summary>
        public ArucoDetector(
            PredefinedDictionaryType dictionaryType = PredefinedDictionaryType.Dict4X4_50,
            ArucoDetectorParameters? detectorParameters = null,
            ArucoRefineParameters? refineParameters = null)
            : this(ArucoDictionary.GetPredefinedDictionary(dictionaryType), detectorParameters, refineParameters, ownsDictionary: true)
        {
        }

        /// <summary>
        /// Initializes a detector with an explicit dictionary.
        /// 使用显式字典初始化检测器。
        /// </summary>
        public ArucoDetector(ArucoDictionary dictionary, ArucoDetectorParameters? detectorParameters = null, ArucoRefineParameters? refineParameters = null)
            : this(dictionary, detectorParameters, refineParameters, ownsDictionary: false)
        {
        }

        /// <summary>Initializes a detector that searches multiple dictionaries in one pass.</summary>
        private ArucoDetector(ArucoDictionary[] dictionaries, ArucoDetectorParameters? detectorParameters, ArucoRefineParameters? refineParameters, bool multiDictionary)
        {
            _ = multiDictionary;
            IntPtr[] nativeDictionaries = ToNativeHandles(dictionaries, nameof(dictionaries));
            NativeMethods.ArucoDetectorParamsNative nativeDetector = (detectorParameters ?? new ArucoDetectorParameters()).ToNative();
            NativeMethods.ArucoRefineParamsNative nativeRefine = (refineParameters ?? ArucoRefineParameters.Default).ToNative();
            NativeException.ThrowIfError(NativeMethods.ArucoDetectorCreateMultiDictionary(
                nativeDictionaries, nativeDictionaries.Length, ref nativeDetector, ref nativeRefine, out IntPtr nativeHandle));
            handle = NativeArucoDetectorHandle.FromNativePointer(nativeHandle);
        }

        private ArucoDetector(ArucoDictionary dictionary, ArucoDetectorParameters? detectorParameters, ArucoRefineParameters? refineParameters, bool ownsDictionary)
        {
            ValidateNotNull(dictionary, nameof(dictionary));
            try
            {
                NativeMethods.ArucoDetectorParamsNative nativeDetector = (detectorParameters ?? new ArucoDetectorParameters()).ToNative();
                NativeMethods.ArucoRefineParamsNative nativeRefine = (refineParameters ?? ArucoRefineParameters.Default).ToNative();
                NativeException.ThrowIfError(NativeMethods.ArucoDetectorCreate(dictionary.NativeHandle, ref nativeDetector, ref nativeRefine, out IntPtr nativeHandle));
                handle = NativeArucoDetectorHandle.FromNativePointer(nativeHandle);
            }
            finally
            {
                if (ownsDictionary)
                {
                    dictionary.Dispose();
                }
            }
        }

        /// <summary>Gets whether this detector has been disposed. 获取检测器是否已经释放。</summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        internal IntPtr NativeHandle
        {
            get
            {
                ThrowIfDisposed();
                return handle.DangerousGetHandle();
            }
        }

        /// <summary>Creates a detector with OpenCV defaults. 使用 OpenCV 默认值创建检测器。</summary>
        public static ArucoDetector Create()
        {
            return new ArucoDetector();
        }

        /// <summary>Creates a detector that searches multiple dictionaries in one pass.</summary>
        public static ArucoDetector Create(ArucoDictionary[] dictionaries, ArucoDetectorParameters? detectorParameters = null, ArucoRefineParameters? refineParameters = null)
        {
            return new ArucoDetector(dictionaries, detectorParameters, refineParameters, multiDictionary: true);
        }

        /// <summary>Gets a copy of the detector dictionary. 获取检测器字典副本。</summary>
        public ArucoDictionary GetDictionary()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.ArucoDetectorGetDictionary(NativeHandle, out IntPtr dictionary));
            return new ArucoDictionary(dictionary);
        }

        /// <summary>Sets the detector dictionary. 设置检测器字典。</summary>
        public ArucoDetector SetDictionary(ArucoDictionary dictionary)
        {
            ThrowIfDisposed();
            ValidateNotNull(dictionary, nameof(dictionary));
            NativeException.ThrowIfError(NativeMethods.ArucoDetectorSetDictionary(NativeHandle, dictionary.NativeHandle));
            return this;
        }

        /// <summary>Gets independent copies of all dictionaries searched by this detector.</summary>
        public ArucoDictionary[] GetDictionaries()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.ArucoDetectorGetDictionariesCount(NativeHandle, out int count));
            if (count < 0) throw new OpenCvException("Native dictionary count is negative.");
            var result = new ArucoDictionary[count];
            try
            {
                for (int i = 0; i < count; i++)
                {
                    NativeException.ThrowIfError(NativeMethods.ArucoDetectorGetDictionaryAt(NativeHandle, i, out IntPtr dictionary));
                    result[i] = new ArucoDictionary(dictionary);
                }
                return result;
            }
            catch
            {
                for (int i = 0; i < result.Length; i++) result[i]?.Dispose();
                throw;
            }
        }

        /// <summary>Replaces all dictionaries searched by this detector.</summary>
        public ArucoDetector SetDictionaries(ArucoDictionary[] dictionaries)
        {
            ThrowIfDisposed();
            IntPtr[] nativeDictionaries = ToNativeHandles(dictionaries, nameof(dictionaries));
            NativeException.ThrowIfError(NativeMethods.ArucoDetectorSetDictionaries(NativeHandle, nativeDictionaries, nativeDictionaries.Length));
            return this;
        }

        /// <summary>Gets detector parameters. 获取检测器参数。</summary>
        public ArucoDetectorParameters GetDetectorParameters()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.ArucoDetectorGetDetectorParameters(NativeHandle, out NativeMethods.ArucoDetectorParamsNative native));
            return ArucoDetectorParameters.FromNative(native);
        }

        /// <summary>Sets detector parameters. 设置检测器参数。</summary>
        public ArucoDetector SetDetectorParameters(ArucoDetectorParameters parameters)
        {
            ThrowIfDisposed();
            ValidateNotNull(parameters, nameof(parameters));
            NativeMethods.ArucoDetectorParamsNative native = parameters.ToNative();
            NativeException.ThrowIfError(NativeMethods.ArucoDetectorSetDetectorParameters(NativeHandle, ref native));
            return this;
        }

        /// <summary>Gets refine parameters. 获取细化参数。</summary>
        public ArucoRefineParameters GetRefineParameters()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.ArucoDetectorGetRefineParameters(NativeHandle, out NativeMethods.ArucoRefineParamsNative native));
            return ArucoRefineParameters.FromNative(native);
        }

        /// <summary>Sets refine parameters. 设置细化参数。</summary>
        public ArucoDetector SetRefineParameters(ArucoRefineParameters parameters)
        {
            ThrowIfDisposed();
            NativeMethods.ArucoRefineParamsNative native = parameters.ToNative();
            NativeException.ThrowIfError(NativeMethods.ArucoDetectorSetRefineParameters(NativeHandle, ref native));
            return this;
        }

        /// <summary>Detects ArUco markers in an image. 检测图像中的 ArUco marker。</summary>
        public ArucoDetectionResult DetectMarkers(Mat image)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            return DetectCore(image, withConfidence: false);
        }

        /// <summary>Detects ArUco markers and returns confidence values. 检测 ArUco marker 并返回置信度。</summary>
        public ArucoDetectionResult DetectMarkersWithConfidence(Mat image)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            return DetectCore(image, withConfidence: true);
        }

        /// <summary>Detects markers and reports the dictionary index used for each result.</summary>
        public ArucoMultiDictionaryDetectionResult DetectMarkersMultiDictionary(Mat image)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            NativeException.ThrowIfError(NativeMethods.ArucoDetectorDetectMarkersMultiDictionaryCount(
                NativeHandle, image.NativeHandle,
                out int markerCount, out int cornerPointCount, out int rejectedCount, out int rejectedPointCount));
            ValidateNativeCount(markerCount, nameof(markerCount));
            ValidateNativeCount(cornerPointCount, nameof(cornerPointCount));
            ValidateNativeCount(rejectedCount, nameof(rejectedCount));
            ValidateNativeCount(rejectedPointCount, nameof(rejectedPointCount));

            var cornerOffsets = new int[checked(markerCount + 1)];
            var corners = new NativeMethods.Point2fNative[cornerPointCount];
            var ids = new int[markerCount];
            var dictionaryIndices = new int[markerCount];
            var rejectedOffsets = new int[checked(rejectedCount + 1)];
            var rejected = new NativeMethods.Point2fNative[rejectedPointCount];
            fixed (int* cornerOffsetsPtr = cornerOffsets)
            fixed (NativeMethods.Point2fNative* cornersPtr = corners)
            fixed (int* idsPtr = ids)
            fixed (int* dictionaryIndicesPtr = dictionaryIndices)
            fixed (int* rejectedOffsetsPtr = rejectedOffsets)
            fixed (NativeMethods.Point2fNative* rejectedPtr = rejected)
            {
                NativeException.ThrowIfError(NativeMethods.ArucoDetectorDetectMarkersMultiDictionaryFill(
                    NativeHandle, image.NativeHandle,
                    cornerOffsetsPtr, cornerOffsets.Length, cornersPtr, corners.Length,
                    idsPtr, ids.Length, dictionaryIndicesPtr, dictionaryIndices.Length,
                    rejectedOffsetsPtr, rejectedOffsets.Length, rejectedPtr, rejected.Length,
                    out int writtenMarkers, out int writtenCorners, out int writtenRejected, out int writtenRejectedPoints));
                if (writtenMarkers != markerCount || writtenCorners != cornerPointCount || writtenRejected != rejectedCount || writtenRejectedPoints != rejectedPointCount)
                    throw new OpenCvException("Aruco multi-dictionary detection counts changed during count/fill.");
            }

            var detection = new ArucoDetectionResult(
                PointSetMarshaller.ToPoint2fGroups(cornerOffsets, ToPoint2fArray(corners, corners.Length), markerCount),
                ids,
                PointSetMarshaller.ToPoint2fGroups(rejectedOffsets, ToPoint2fArray(rejected, rejected.Length), rejectedCount),
                Array.Empty<float>());
            return new ArucoMultiDictionaryDetectionResult(detection, dictionaryIndices);
        }

        /// <summary>Draws detected marker borders and optional identifiers into an image.</summary>
        public static void DrawDetectedMarkers(Mat image, Point2f[][] corners, int[]? ids = null, Scalar borderColor = default)
        {
            ValidateNotNull(image, nameof(image));
            PointSetMarshaller.FlattenPoint2fGroups(corners, nameof(corners), out int[] offsets, out Point2f[] flatCorners);
            ids ??= Array.Empty<int>();
            if (ids.Length != 0 && ids.Length != corners.Length) throw new ArgumentException("The id count must be zero or match the marker count.", nameof(ids));
            var nativeCorners = ToNativePoint2fArray(flatCorners);
            Scalar color = borderColor.Equals(default(Scalar)) ? new Scalar(0, 255, 0, 0) : borderColor;
            fixed (int* offsetsPtr = offsets)
            fixed (NativeMethods.Point2fNative* cornersPtr = nativeCorners)
            fixed (int* idsPtr = ids)
            {
                NativeException.ThrowIfError(NativeMethods.ArucoDrawDetectedMarkers(
                    image.NativeHandle, offsetsPtr, corners.Length, cornersPtr, nativeCorners.Length,
                    idsPtr, ids.Length, color.V0, color.V1, color.V2, color.V3));
            }
        }

        /// <summary>
        /// Refines detected ArUco markers using a grid board and rejected candidates.
        /// 使用 GridBoard 和 rejected candidates 细化已检测到的 ArUco marker。
        /// </summary>
        /// <param name="image">The source image. 源图像。</param>
        /// <param name="board">The ArUco grid board. ArUco 网格板。</param>
        /// <param name="detectedCorners">The currently detected marker corners. 当前已检测 marker 的角点。</param>
        /// <param name="detectedIds">The currently detected marker ids. 当前已检测 marker 的 ID。</param>
        /// <param name="rejectedCandidates">The rejected marker candidates. 被拒绝的 marker 候选。</param>
        /// <param name="cameraMatrix">The optional camera matrix. 可选相机矩阵。</param>
        /// <param name="distCoeffs">The optional distortion coefficients. 可选畸变系数。</param>
        /// <returns>The refined marker result. 细化后的 marker 结果。</returns>
        public ArucoRefineResult RefineDetectedMarkers(
            Mat image,
            ArucoGridBoard board,
            Point2f[][] detectedCorners,
            int[] detectedIds,
            Point2f[][] rejectedCandidates,
            Mat? cameraMatrix = null,
            Mat? distCoeffs = null)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(board, nameof(board));
            ValidateNotNull(detectedIds, nameof(detectedIds));

            PointSetMarshaller.FlattenPoint2fGroups(detectedCorners, nameof(detectedCorners), out int[] detectedOffsets, out Point2f[] flatDetectedCorners);
            PointSetMarshaller.FlattenPoint2fGroups(rejectedCandidates, nameof(rejectedCandidates), out int[] rejectedOffsets, out Point2f[] flatRejectedCandidates);
            ValidateDetectedMarkerInputs(detectedCorners, detectedIds, nameof(detectedCorners), nameof(detectedIds));

            NativeMethods.Point2fNative[] nativeDetectedCorners = ToNativePoint2fArray(flatDetectedCorners);
            NativeMethods.Point2fNative[] nativeRejectedCandidates = ToNativePoint2fArray(flatRejectedCandidates);

            fixed (int* detectedOffsetsPtr = detectedOffsets)
            fixed (NativeMethods.Point2fNative* detectedCornersPtr = nativeDetectedCorners)
            fixed (int* detectedIdsPtr = detectedIds)
            fixed (int* rejectedOffsetsPtr = rejectedOffsets)
            fixed (NativeMethods.Point2fNative* rejectedCandidatesPtr = nativeRejectedCandidates)
            {
                NativeException.ThrowIfError(NativeMethods.ArucoDetectorRefineDetectedMarkersCount(
                    NativeHandle,
                    image.NativeHandle,
                    board.NativeHandle,
                    detectedOffsetsPtr,
                    detectedCorners.Length,
                    detectedCornersPtr,
                    nativeDetectedCorners.Length,
                    detectedIdsPtr,
                    detectedIds.Length,
                    rejectedOffsetsPtr,
                    rejectedCandidates.Length,
                    rejectedCandidatesPtr,
                    nativeRejectedCandidates.Length,
                    GetNativeHandleOrZero(cameraMatrix),
                    GetNativeHandleOrZero(distCoeffs),
                    out int refinedMarkerCount,
                    out int refinedCornerPointCount,
                    out int refinedRejectedCount,
                    out int refinedRejectedPointCount,
                    out int recoveredIndexCount));

                var refinedOffsets = new int[Math.Max(refinedMarkerCount, 0) + 1];
                var refinedCorners = new NativeMethods.Point2fNative[Math.Max(refinedCornerPointCount, 0)];
                var refinedIds = new int[Math.Max(refinedMarkerCount, 0)];
                var refinedRejectedOffsets = new int[Math.Max(refinedRejectedCount, 0) + 1];
                var refinedRejected = new NativeMethods.Point2fNative[Math.Max(refinedRejectedPointCount, 0)];
                var recoveredIndices = new int[Math.Max(recoveredIndexCount, 0)];

                fixed (int* refinedOffsetsPtr = refinedOffsets)
                fixed (NativeMethods.Point2fNative* refinedCornersPtr = refinedCorners)
                fixed (int* refinedIdsPtr = refinedIds)
                fixed (int* refinedRejectedOffsetsPtr = refinedRejectedOffsets)
                fixed (NativeMethods.Point2fNative* refinedRejectedPtr = refinedRejected)
                fixed (int* recoveredIndicesPtr = recoveredIndices)
                {
                    NativeException.ThrowIfError(NativeMethods.ArucoDetectorRefineDetectedMarkersFill(
                        NativeHandle,
                        image.NativeHandle,
                        board.NativeHandle,
                        detectedOffsetsPtr,
                        detectedCorners.Length,
                        detectedCornersPtr,
                        nativeDetectedCorners.Length,
                        detectedIdsPtr,
                        detectedIds.Length,
                        rejectedOffsetsPtr,
                        rejectedCandidates.Length,
                        rejectedCandidatesPtr,
                        nativeRejectedCandidates.Length,
                        GetNativeHandleOrZero(cameraMatrix),
                        GetNativeHandleOrZero(distCoeffs),
                        refinedOffsetsPtr,
                        refinedOffsets.Length,
                        refinedCornersPtr,
                        refinedCorners.Length,
                        refinedIdsPtr,
                        refinedIds.Length,
                        refinedRejectedOffsetsPtr,
                        refinedRejectedOffsets.Length,
                        refinedRejectedPtr,
                        refinedRejected.Length,
                        recoveredIndicesPtr,
                        recoveredIndices.Length,
                        out refinedMarkerCount,
                        out refinedCornerPointCount,
                        out refinedRejectedCount,
                        out refinedRejectedPointCount,
                        out recoveredIndexCount));
                }

                int safeMarkerCount = Math.Max(0, Math.Min(refinedMarkerCount, refinedIds.Length));
                int safeRejectedCount = Math.Max(0, Math.Min(refinedRejectedCount, refinedRejectedOffsets.Length - 1));
                Point2f[] managedCorners = ToPoint2fArray(refinedCorners, Math.Max(0, Math.Min(refinedCornerPointCount, refinedCorners.Length)));
                Point2f[] managedRejected = ToPoint2fArray(refinedRejected, Math.Max(0, Math.Min(refinedRejectedPointCount, refinedRejected.Length)));
                return new ArucoRefineResult(
                    PointSetMarshaller.ToPoint2fGroups(refinedOffsets, managedCorners, safeMarkerCount),
                    Trim(refinedIds, safeMarkerCount),
                    PointSetMarshaller.ToPoint2fGroups(refinedRejectedOffsets, managedRejected, safeRejectedCount),
                    Trim(recoveredIndices, Math.Max(0, Math.Min(recoveredIndexCount, recoveredIndices.Length))));
            }
        }

        /// <summary>Releases the native detector. 释放 native 检测器。</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private ArucoDetectionResult DetectCore(Mat image, bool withConfidence)
        {
            int markerCount;
            int cornerPointCount;
            int rejectedCount;
            int rejectedPointCount;
            int status = withConfidence
                ? NativeMethods.ArucoDetectorDetectMarkersWithConfidenceCount(NativeHandle, image.NativeHandle, out markerCount, out cornerPointCount, out rejectedCount, out rejectedPointCount)
                : NativeMethods.ArucoDetectorDetectMarkersCount(NativeHandle, image.NativeHandle, out markerCount, out cornerPointCount, out rejectedCount, out rejectedPointCount);
            NativeException.ThrowIfError(status);

            var cornerOffsets = new int[Math.Max(markerCount, 0) + 1];
            var corners = new NativeMethods.Point2fNative[Math.Max(cornerPointCount, 0)];
            var ids = new int[Math.Max(markerCount, 0)];
            var rejectedOffsets = new int[Math.Max(rejectedCount, 0) + 1];
            var rejected = new NativeMethods.Point2fNative[Math.Max(rejectedPointCount, 0)];
            var confidence = withConfidence ? new float[Math.Max(markerCount, 0)] : Array.Empty<float>();

            fixed (int* cornerOffsetsPtr = cornerOffsets)
            fixed (NativeMethods.Point2fNative* cornersPtr = corners)
            fixed (int* idsPtr = ids)
            fixed (float* confidencePtr = confidence)
            fixed (int* rejectedOffsetsPtr = rejectedOffsets)
            fixed (NativeMethods.Point2fNative* rejectedPtr = rejected)
            {
                if (withConfidence)
                {
                    NativeException.ThrowIfError(NativeMethods.ArucoDetectorDetectMarkersWithConfidenceFill(
                        NativeHandle,
                        image.NativeHandle,
                        cornerOffsetsPtr,
                        cornerOffsets.Length,
                        cornersPtr,
                        corners.Length,
                        idsPtr,
                        ids.Length,
                        confidencePtr,
                        confidence.Length,
                        rejectedOffsetsPtr,
                        rejectedOffsets.Length,
                        rejectedPtr,
                        rejected.Length,
                        out markerCount,
                        out cornerPointCount,
                        out rejectedCount,
                        out rejectedPointCount));
                }
                else
                {
                    NativeException.ThrowIfError(NativeMethods.ArucoDetectorDetectMarkersFill(
                        NativeHandle,
                        image.NativeHandle,
                        cornerOffsetsPtr,
                        cornerOffsets.Length,
                        cornersPtr,
                        corners.Length,
                        idsPtr,
                        ids.Length,
                        rejectedOffsetsPtr,
                        rejectedOffsets.Length,
                        rejectedPtr,
                        rejected.Length,
                        out markerCount,
                        out cornerPointCount,
                        out rejectedCount,
                        out rejectedPointCount));
                }
            }

            int safeMarkerCount = Math.Max(0, Math.Min(markerCount, ids.Length));
            int safeRejectedCount = Math.Max(0, Math.Min(rejectedCount, rejectedOffsets.Length - 1));
            Point2f[] managedCorners = ToPoint2fArray(corners, Math.Max(0, Math.Min(cornerPointCount, corners.Length)));
            Point2f[] managedRejected = ToPoint2fArray(rejected, Math.Max(0, Math.Min(rejectedPointCount, rejected.Length)));
            return new ArucoDetectionResult(
                PointSetMarshaller.ToPoint2fGroups(cornerOffsets, managedCorners, safeMarkerCount),
                Trim(ids, safeMarkerCount),
                PointSetMarshaller.ToPoint2fGroups(rejectedOffsets, managedRejected, safeRejectedCount),
                Trim(confidence, Math.Min(safeMarkerCount, confidence.Length)));
        }

        private static Point2f[] ToPoint2fArray(NativeMethods.Point2fNative[] points, int count)
        {
            if (count <= 0)
            {
                return Array.Empty<Point2f>();
            }

            var result = new Point2f[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = new Point2f(points[i].X, points[i].Y);
            }

            return result;
        }

        private static NativeMethods.Point2fNative[] ToNativePoint2fArray(Point2f[] points)
        {
            if (points.Length == 0)
            {
                return Array.Empty<NativeMethods.Point2fNative>();
            }

            var result = new NativeMethods.Point2fNative[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                result[i] = new NativeMethods.Point2fNative
                {
                    X = points[i].X,
                    Y = points[i].Y
                };
            }

            return result;
        }

        private static IntPtr[] ToNativeHandles(ArucoDictionary[] dictionaries, string parameterName)
        {
            if (dictionaries == null) throw new ArgumentNullException(parameterName);
            if (dictionaries.Length == 0) throw new ArgumentException("At least one dictionary is required.", parameterName);
            var result = new IntPtr[dictionaries.Length];
            for (int i = 0; i < dictionaries.Length; i++)
            {
                if (dictionaries[i] == null) throw new ArgumentException("Dictionaries cannot contain null elements.", parameterName);
                result[i] = dictionaries[i].NativeHandle;
            }
            return result;
        }

        private static void ValidateNativeCount(int value, string name)
        {
            if (value < 0) throw new OpenCvException("Native " + name + " is negative.");
        }

        private static int[] Trim(int[] values, int count)
        {
            if (count <= 0)
            {
                return Array.Empty<int>();
            }

            if (count == values.Length)
            {
                return values;
            }

            var result = new int[count];
            Array.Copy(values, result, count);
            return result;
        }

        private static float[] Trim(float[] values, int count)
        {
            if (count <= 0)
            {
                return Array.Empty<float>();
            }

            if (count == values.Length)
            {
                return values;
            }

            var result = new float[count];
            Array.Copy(values, result, count);
            return result;
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing && handle != null)
                {
                    handle.Dispose();
                }

                disposed = true;
            }
        }

        private static void ValidateNotNull<T>(T value, string parameterName)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        private static void ValidateDetectedMarkerInputs(Point2f[][] detectedCorners, int[] detectedIds, string cornersParameterName, string idsParameterName)
        {
            if (detectedCorners.Length != detectedIds.Length)
            {
                throw new ArgumentException("Detected corner group count must match detected id count.", idsParameterName);
            }

            for (int i = 0; i < detectedCorners.Length; i++)
            {
                if (detectedCorners[i].Length == 0)
                {
                    throw new ArgumentException("Detected corner groups cannot be empty.", cornersParameterName);
                }
            }
        }

        private static IntPtr GetNativeHandleOrZero(Mat? mat)
        {
            return mat == null ? IntPtr.Zero : mat.NativeHandle;
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }
}

using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.ObjDetect
{
    /// <summary>
    /// ChArUco detector compatible with OpenCV <c>cv::aruco::CharucoDetector</c>.
    /// 与 OpenCV <c>cv::aruco::CharucoDetector</c> 兼容的 ChArUco 检测器。
    /// </summary>
    public sealed unsafe class CharucoDetector : IDisposable
    {
        private NativeArucoCharucoDetectorHandle handle;
        private bool disposed;

        /// <summary>
        /// Initializes a ChArUco detector.
        /// 初始化 ChArUco 检测器。
        /// </summary>
        public CharucoDetector(
            CharucoBoard board,
            CharucoParameters? charucoParameters = null,
            ArucoDetectorParameters? detectorParameters = null,
            ArucoRefineParameters? refineParameters = null)
        {
            ValidateNotNull(board, nameof(board));
            NativeMethods.ArucoCharucoParamsNative nativeCharuco = (charucoParameters ?? new CharucoParameters()).ToNative();
            NativeMethods.ArucoDetectorParamsNative nativeDetector = (detectorParameters ?? new ArucoDetectorParameters()).ToNative();
            NativeMethods.ArucoRefineParamsNative nativeRefine = (refineParameters ?? ArucoRefineParameters.Default).ToNative();
            NativeException.ThrowIfError(NativeMethods.ArucoCharucoDetectorCreate(
                board.NativeHandle,
                ref nativeCharuco,
                charucoParameters?.CameraMatrix?.NativeHandle ?? IntPtr.Zero,
                charucoParameters?.DistCoeffs?.NativeHandle ?? IntPtr.Zero,
                ref nativeDetector,
                ref nativeRefine,
                out IntPtr nativeHandle));
            handle = NativeArucoCharucoDetectorHandle.FromNativePointer(nativeHandle);
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

        /// <summary>Gets a copy of the detector board. 获取检测器 board 副本。</summary>
        public CharucoBoard GetBoard()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.ArucoCharucoDetectorGetBoard(NativeHandle, out IntPtr board));
            return new CharucoBoard(board);
        }

        /// <summary>Sets the detector board. 设置检测器 board。</summary>
        public CharucoDetector SetBoard(CharucoBoard board)
        {
            ThrowIfDisposed();
            ValidateNotNull(board, nameof(board));
            NativeException.ThrowIfError(NativeMethods.ArucoCharucoDetectorSetBoard(NativeHandle, board.NativeHandle));
            return this;
        }

        /// <summary>Gets ChArUco parameters. 获取 ChArUco 参数。</summary>
        public CharucoParameters GetCharucoParameters()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.ArucoCharucoDetectorGetCharucoParameters(NativeHandle, out NativeMethods.ArucoCharucoParamsNative native, out IntPtr cameraMatrix, out IntPtr distCoeffs));
            Mat? camera = cameraMatrix == IntPtr.Zero ? null : new Mat(cameraMatrix);
            Mat? dist = distCoeffs == IntPtr.Zero ? null : new Mat(distCoeffs);
            return CharucoParameters.FromNative(native, camera, dist);
        }

        /// <summary>Sets ChArUco parameters. 设置 ChArUco 参数。</summary>
        public CharucoDetector SetCharucoParameters(CharucoParameters parameters)
        {
            ThrowIfDisposed();
            ValidateNotNull(parameters, nameof(parameters));
            NativeMethods.ArucoCharucoParamsNative native = parameters.ToNative();
            NativeException.ThrowIfError(NativeMethods.ArucoCharucoDetectorSetCharucoParameters(
                NativeHandle,
                ref native,
                parameters.CameraMatrix?.NativeHandle ?? IntPtr.Zero,
                parameters.DistCoeffs?.NativeHandle ?? IntPtr.Zero));
            return this;
        }

        /// <summary>Gets marker detector parameters.</summary>
        public ArucoDetectorParameters GetDetectorParameters()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.ArucoCharucoDetectorGetDetectorParameters(NativeHandle, out NativeMethods.ArucoDetectorParamsNative parameters));
            return ArucoDetectorParameters.FromNative(parameters);
        }

        /// <summary>Sets marker detector parameters.</summary>
        public CharucoDetector SetDetectorParameters(ArucoDetectorParameters parameters)
        {
            ThrowIfDisposed();
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
            NativeMethods.ArucoDetectorParamsNative native = parameters.ToNative();
            NativeException.ThrowIfError(NativeMethods.ArucoCharucoDetectorSetDetectorParameters(NativeHandle, ref native));
            return this;
        }

        /// <summary>Gets marker refinement parameters.</summary>
        public ArucoRefineParameters GetRefineParameters()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.ArucoCharucoDetectorGetRefineParameters(NativeHandle, out NativeMethods.ArucoRefineParamsNative parameters));
            return ArucoRefineParameters.FromNative(parameters);
        }

        /// <summary>Sets marker refinement parameters.</summary>
        public CharucoDetector SetRefineParameters(ArucoRefineParameters parameters)
        {
            ThrowIfDisposed();
            NativeMethods.ArucoRefineParamsNative native = parameters.ToNative();
            NativeException.ThrowIfError(NativeMethods.ArucoCharucoDetectorSetRefineParameters(NativeHandle, ref native));
            return this;
        }

        /// <summary>Detects ArUco markers and interpolates ChArUco corners. 检测 ArUco marker 并插值 ChArUco 角点。</summary>
        public CharucoDetectionResult DetectBoard(Mat image)
        {
            return DetectBoard(image, Array.Empty<Point2f[]>(), Array.Empty<int>());
        }

        /// <summary>Interpolates ChArUco corners from supplied marker detections. 根据传入的 marker 检测结果插值 ChArUco 角点。</summary>
        public CharucoDetectionResult DetectBoard(Mat image, Point2f[][] markerCorners, int[] markerIds)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            if (markerCorners == null)
            {
                throw new ArgumentNullException(nameof(markerCorners));
            }

            if (markerIds == null)
            {
                throw new ArgumentNullException(nameof(markerIds));
            }

            if (markerCorners.Length != markerIds.Length)
            {
                throw new ArgumentException("Marker corner group count must match marker id count.", nameof(markerIds));
            }

            PointSetMarshaller.FlattenPoint2fGroups(markerCorners, nameof(markerCorners), out int[] inputOffsets, out Point2f[] inputPoints);
            NativeMethods.Point2fNative[] inputNativePoints = ToNative(inputPoints);

            int charucoCount;
            int markerCount;
            int markerPointCount;
            fixed (int* inputOffsetsPtr = inputOffsets)
            fixed (NativeMethods.Point2fNative* inputPointsPtr = inputNativePoints)
            fixed (int* inputIdsPtr = markerIds)
            {
                NativeException.ThrowIfError(NativeMethods.ArucoCharucoDetectorDetectBoardCount(
                    NativeHandle,
                    image.NativeHandle,
                    inputOffsetsPtr,
                    markerCorners.Length,
                    inputPointsPtr,
                    inputNativePoints.Length,
                    inputIdsPtr,
                    markerIds.Length,
                    out charucoCount,
                    out markerCount,
                    out markerPointCount));
            }

            var nativeCharucoCorners = new NativeMethods.Point2fNative[Math.Max(charucoCount, 0)];
            var charucoIds = new int[Math.Max(charucoCount, 0)];
            var outputMarkerOffsets = new int[Math.Max(markerCount, 0) + 1];
            var nativeMarkerCorners = new NativeMethods.Point2fNative[Math.Max(markerPointCount, 0)];
            var outputMarkerIds = new int[Math.Max(markerCount, 0)];

            fixed (int* inputOffsetsPtr = inputOffsets)
            fixed (NativeMethods.Point2fNative* inputPointsPtr = inputNativePoints)
            fixed (int* inputIdsPtr = markerIds)
            fixed (NativeMethods.Point2fNative* charucoCornersPtr = nativeCharucoCorners)
            fixed (int* charucoIdsPtr = charucoIds)
            fixed (int* outputMarkerOffsetsPtr = outputMarkerOffsets)
            fixed (NativeMethods.Point2fNative* markerCornersPtr = nativeMarkerCorners)
            fixed (int* outputMarkerIdsPtr = outputMarkerIds)
            {
                NativeException.ThrowIfError(NativeMethods.ArucoCharucoDetectorDetectBoardFill(
                    NativeHandle,
                    image.NativeHandle,
                    inputOffsetsPtr,
                    markerCorners.Length,
                    inputPointsPtr,
                    inputNativePoints.Length,
                    inputIdsPtr,
                    markerIds.Length,
                    charucoCornersPtr,
                    nativeCharucoCorners.Length,
                    charucoIdsPtr,
                    charucoIds.Length,
                    outputMarkerOffsetsPtr,
                    outputMarkerOffsets.Length,
                    markerCornersPtr,
                    nativeMarkerCorners.Length,
                    outputMarkerIdsPtr,
                    outputMarkerIds.Length,
                    out charucoCount,
                    out markerCount,
                    out markerPointCount));
            }

            int safeCharucoCount = Math.Max(0, Math.Min(charucoCount, charucoIds.Length));
            int safeMarkerCount = Math.Max(0, Math.Min(markerCount, outputMarkerIds.Length));
            Point2f[] managedCharucoCorners = ToPoint2fArray(nativeCharucoCorners, safeCharucoCount);
            Point2f[] managedMarkerCorners = ToPoint2fArray(nativeMarkerCorners, Math.Max(0, Math.Min(markerPointCount, nativeMarkerCorners.Length)));
            return new CharucoDetectionResult(
                managedCharucoCorners,
                Trim(charucoIds, safeCharucoCount),
                PointSetMarshaller.ToPoint2fGroups(outputMarkerOffsets, managedMarkerCorners, safeMarkerCount),
                Trim(outputMarkerIds, safeMarkerCount));
        }

        /// <summary>Detects ChArUco diamonds and the marker observations used to form them.</summary>
        public CharucoDiamondDetectionResult DetectDiamonds(Mat image)
        {
            return DetectDiamonds(image, Array.Empty<Point2f[]>(), Array.Empty<int>());
        }

        /// <summary>Detects ChArUco diamonds from supplied marker observations.</summary>
        public CharucoDiamondDetectionResult DetectDiamonds(Mat image, Point2f[][] markerCorners, int[] markerIds)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            if (markerCorners == null) throw new ArgumentNullException(nameof(markerCorners));
            if (markerIds == null) throw new ArgumentNullException(nameof(markerIds));
            if (markerCorners.Length != markerIds.Length) throw new ArgumentException("Marker corner group count must match marker id count.", nameof(markerIds));
            PointSetMarshaller.FlattenPoint2fGroups(markerCorners, nameof(markerCorners), out int[] inputOffsets, out Point2f[] inputPoints);
            NativeMethods.Point2fNative[] inputNativePoints = ToNative(inputPoints);

            int diamondCount;
            int diamondPointCount;
            int markerCount;
            int markerPointCount;
            fixed (int* inputOffsetsPtr = inputOffsets)
            fixed (NativeMethods.Point2fNative* inputPointsPtr = inputNativePoints)
            fixed (int* inputIdsPtr = markerIds)
            {
                NativeException.ThrowIfError(NativeMethods.ArucoCharucoDetectorDetectDiamondsCount(
                    NativeHandle, image.NativeHandle,
                    inputOffsetsPtr, markerCorners.Length, inputPointsPtr, inputNativePoints.Length, inputIdsPtr, markerIds.Length,
                    out diamondCount, out diamondPointCount, out markerCount, out markerPointCount));
            }
            ValidateCount(diamondCount, nameof(diamondCount));
            ValidateCount(diamondPointCount, nameof(diamondPointCount));
            ValidateCount(markerCount, nameof(markerCount));
            ValidateCount(markerPointCount, nameof(markerPointCount));

            var diamondOffsets = new int[checked(diamondCount + 1)];
            var diamondPoints = new NativeMethods.Point2fNative[diamondPointCount];
            var diamondIds = new int[checked(diamondCount * 4)];
            var outputMarkerOffsets = new int[checked(markerCount + 1)];
            var outputMarkerPoints = new NativeMethods.Point2fNative[markerPointCount];
            var outputMarkerIds = new int[markerCount];
            fixed (int* inputOffsetsPtr = inputOffsets)
            fixed (NativeMethods.Point2fNative* inputPointsPtr = inputNativePoints)
            fixed (int* inputIdsPtr = markerIds)
            fixed (int* diamondOffsetsPtr = diamondOffsets)
            fixed (NativeMethods.Point2fNative* diamondPointsPtr = diamondPoints)
            fixed (int* diamondIdsPtr = diamondIds)
            fixed (int* outputMarkerOffsetsPtr = outputMarkerOffsets)
            fixed (NativeMethods.Point2fNative* outputMarkerPointsPtr = outputMarkerPoints)
            fixed (int* outputMarkerIdsPtr = outputMarkerIds)
            {
                NativeException.ThrowIfError(NativeMethods.ArucoCharucoDetectorDetectDiamondsFill(
                    NativeHandle, image.NativeHandle,
                    inputOffsetsPtr, markerCorners.Length, inputPointsPtr, inputNativePoints.Length, inputIdsPtr, markerIds.Length,
                    diamondOffsetsPtr, diamondOffsets.Length, diamondPointsPtr, diamondPoints.Length, diamondIdsPtr, diamondIds.Length,
                    outputMarkerOffsetsPtr, outputMarkerOffsets.Length, outputMarkerPointsPtr, outputMarkerPoints.Length, outputMarkerIdsPtr, outputMarkerIds.Length,
                    out int writtenDiamonds, out int writtenDiamondPoints, out int writtenMarkers, out int writtenMarkerPoints));
                if (writtenDiamonds != diamondCount || writtenDiamondPoints != diamondPointCount || writtenMarkers != markerCount || writtenMarkerPoints != markerPointCount)
                    throw new OpenCvException("ChArUco diamond counts changed during count/fill.");
            }

            var managedDiamondIds = new Vec4i[diamondCount];
            for (int i = 0; i < diamondCount; i++)
                managedDiamondIds[i] = new Vec4i(diamondIds[i * 4], diamondIds[i * 4 + 1], diamondIds[i * 4 + 2], diamondIds[i * 4 + 3]);
            return new CharucoDiamondDetectionResult(
                PointSetMarshaller.ToPoint2fGroups(diamondOffsets, ToPoint2fArray(diamondPoints, diamondPoints.Length), diamondCount),
                managedDiamondIds,
                PointSetMarshaller.ToPoint2fGroups(outputMarkerOffsets, ToPoint2fArray(outputMarkerPoints, outputMarkerPoints.Length), markerCount),
                outputMarkerIds);
        }

        /// <summary>Draws detected ChArUco corners and optional identifiers into an image.</summary>
        public static void DrawDetectedCorners(Mat image, Point2f[] corners, int[]? ids = null, Scalar cornerColor = default)
        {
            ValidateNotNull(image, nameof(image));
            if (corners == null) throw new ArgumentNullException(nameof(corners));
            ids ??= Array.Empty<int>();
            if (ids.Length != 0 && ids.Length != corners.Length) throw new ArgumentException("The id count must be zero or match the corner count.", nameof(ids));
            NativeMethods.Point2fNative[] nativeCorners = ToNative(corners);
            Scalar color = cornerColor.Equals(default(Scalar)) ? new Scalar(255, 0, 0, 0) : cornerColor;
            fixed (NativeMethods.Point2fNative* cornersPtr = nativeCorners)
            fixed (int* idsPtr = ids)
            {
                NativeException.ThrowIfError(NativeMethods.ArucoDrawDetectedCornersCharuco(
                    image.NativeHandle, cornersPtr, nativeCorners.Length, idsPtr, ids.Length,
                    color.V0, color.V1, color.V2, color.V3));
            }
        }

        /// <summary>Draws detected ChArUco diamonds and optional four-part identifiers into an image.</summary>
        public static void DrawDetectedDiamonds(Mat image, Point2f[][] diamondCorners, Vec4i[]? diamondIds = null, Scalar borderColor = default)
        {
            ValidateNotNull(image, nameof(image));
            PointSetMarshaller.FlattenPoint2fGroups(diamondCorners, nameof(diamondCorners), out int[] offsets, out Point2f[] flatPoints);
            diamondIds ??= Array.Empty<Vec4i>();
            if (diamondIds.Length != 0 && diamondIds.Length != diamondCorners.Length) throw new ArgumentException("The id count must be zero or match the diamond count.", nameof(diamondIds));
            NativeMethods.Point2fNative[] nativePoints = ToNative(flatPoints);
            var flatIds = new int[checked(diamondIds.Length * 4)];
            for (int i = 0; i < diamondIds.Length; i++)
            {
                flatIds[i * 4] = diamondIds[i].V0;
                flatIds[i * 4 + 1] = diamondIds[i].V1;
                flatIds[i * 4 + 2] = diamondIds[i].V2;
                flatIds[i * 4 + 3] = diamondIds[i].V3;
            }
            Scalar color = borderColor.Equals(default(Scalar)) ? new Scalar(0, 0, 255, 0) : borderColor;
            fixed (int* offsetsPtr = offsets)
            fixed (NativeMethods.Point2fNative* pointsPtr = nativePoints)
            fixed (int* idsPtr = flatIds)
            {
                NativeException.ThrowIfError(NativeMethods.ArucoDrawDetectedDiamonds(
                    image.NativeHandle, offsetsPtr, diamondCorners.Length, pointsPtr, nativePoints.Length,
                    idsPtr, flatIds.Length, color.V0, color.V1, color.V2, color.V3));
            }
        }

        /// <summary>Releases the native detector. 释放 native 检测器。</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private static NativeMethods.Point2fNative[] ToNative(Point2f[] points)
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

        private static void ValidateCount(int count, string name)
        {
            if (count < 0) throw new OpenCvException("Native " + name + " is negative.");
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

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }
}

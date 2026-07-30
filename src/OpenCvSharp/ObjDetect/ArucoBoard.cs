using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.ObjDetect
{
    /// <summary>Represents a generic board of ArUco markers with owned native value semantics.</summary>
    public sealed unsafe class ArucoBoard : IDisposable
    {
        private NativeArucoBoardHandle handle;
        private bool disposed;

        /// <summary>Creates a board from marker object points and marker identifiers.</summary>
        public ArucoBoard(Point3f[][] objectPoints, ArucoDictionary dictionary, int[] ids)
        {
            if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));
            if (ids == null) throw new ArgumentNullException(nameof(ids));
            PointSetMarshaller.FlattenPoint3fGroups(objectPoints, nameof(objectPoints), out int[] offsets, out Point3f[] flatPoints);
            if (objectPoints.Length != ids.Length) throw new ArgumentException("The marker and id counts must match.", nameof(ids));

            NativeMethods.Point3fNative[] nativePoints = ToNative(flatPoints);
            fixed (int* offsetsPtr = offsets)
            fixed (NativeMethods.Point3fNative* pointsPtr = nativePoints)
            fixed (int* idsPtr = ids)
            {
                NativeException.ThrowIfError(NativeMethods.ArucoBoardCreate(
                    offsetsPtr, objectPoints.Length, pointsPtr, nativePoints.Length,
                    dictionary.NativeHandle, idsPtr, ids.Length, out IntPtr nativeHandle));
                handle = NativeArucoBoardHandle.FromNativePointer(nativeHandle);
            }
        }

        /// <summary>Gets whether this board has been disposed.</summary>
        public bool IsDisposed => disposed;

        internal IntPtr NativeHandle
        {
            get
            {
                ThrowIfDisposed();
                return handle.DangerousGetHandle();
            }
        }

        /// <summary>Gets an independent dictionary value owned by the caller.</summary>
        public ArucoDictionary Dictionary
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.ArucoBoardGetDictionary(NativeHandle, out IntPtr dictionary));
                return new ArucoDictionary(dictionary);
            }
        }

        /// <summary>Gets independent copies of marker object-point groups.</summary>
        public Point3f[][] ObjectPoints
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.ArucoBoardGetObjectPointsCount(NativeHandle, out int markerCount, out int pointCount));
                ValidateCount(markerCount, nameof(markerCount));
                ValidateCount(pointCount, nameof(pointCount));
                var offsets = new int[checked(markerCount + 1)];
                var points = new NativeMethods.Point3fNative[pointCount];
                fixed (int* offsetsPtr = offsets)
                fixed (NativeMethods.Point3fNative* pointsPtr = points)
                {
                    NativeException.ThrowIfError(NativeMethods.ArucoBoardGetObjectPointsFill(
                        NativeHandle, offsetsPtr, offsets.Length, pointsPtr, points.Length,
                        out int writtenMarkers, out int writtenPoints));
                    if (writtenMarkers != markerCount || writtenPoints != pointCount)
                        throw new OpenCvException("ArucoBoard object-point count changed during count/fill.");
                }
                return ToGroups(offsets, points);
            }
        }

        /// <summary>Gets a copy of marker identifiers.</summary>
        public int[] Ids
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.ArucoBoardGetIdsCount(NativeHandle, out int count));
                ValidateCount(count, nameof(count));
                var result = new int[count];
                fixed (int* resultPtr = result)
                {
                    NativeException.ThrowIfError(NativeMethods.ArucoBoardGetIdsFill(NativeHandle, resultPtr, result.Length, out int written));
                    if (written != count) throw new OpenCvException("ArucoBoard id count changed during count/fill.");
                }
                return result;
            }
        }

        /// <summary>Gets the maximum board coordinate.</summary>
        public Point3f RightBottomCorner
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.ArucoBoardGetRightBottomCorner(NativeHandle, out NativeMethods.Point3fNative point));
                return new Point3f(point.X, point.Y, point.Z);
            }
        }

        /// <summary>Matches detected image corners to board object points and writes caller-owned matrices.</summary>
        public void MatchImagePoints(Point2f[][] detectedCorners, int[] detectedIds, Mat objectPoints, Mat imagePoints)
        {
            ThrowIfDisposed();
            if (detectedIds == null) throw new ArgumentNullException(nameof(detectedIds));
            if (objectPoints == null) throw new ArgumentNullException(nameof(objectPoints));
            if (imagePoints == null) throw new ArgumentNullException(nameof(imagePoints));
            PointSetMarshaller.FlattenPoint2fGroups(detectedCorners, nameof(detectedCorners), out int[] offsets, out Point2f[] flatPoints);
            if (detectedCorners.Length != detectedIds.Length) throw new ArgumentException("The detected corner and id counts must match.", nameof(detectedIds));
            NativeMethods.Point2fNative[] nativePoints = ToNative(flatPoints);
            fixed (int* offsetsPtr = offsets)
            fixed (NativeMethods.Point2fNative* pointsPtr = nativePoints)
            fixed (int* idsPtr = detectedIds)
            {
                NativeException.ThrowIfError(NativeMethods.ArucoBoardMatchImagePoints(
                    NativeHandle, offsetsPtr, detectedCorners.Length, pointsPtr, nativePoints.Length,
                    idsPtr, detectedIds.Length, objectPoints.NativeHandle, imagePoints.NativeHandle));
            }
        }

        /// <summary>Generates the board image into a caller-owned matrix.</summary>
        public void GenerateImage(Size outSize, Mat image, int marginSize = 0, int borderBits = 1)
        {
            ThrowIfDisposed();
            if (image == null) throw new ArgumentNullException(nameof(image));
            NativeException.ThrowIfError(NativeMethods.ArucoBoardGenerateImage(NativeHandle, outSize.Width, outSize.Height, image.NativeHandle, marginSize, borderBits));
        }

        /// <summary>Generates and returns an owned board image.</summary>
        public Mat GenerateImage(Size outSize, int marginSize = 0, int borderBits = 1)
        {
            var image = new Mat();
            try
            {
                GenerateImage(outSize, image, marginSize, borderBits);
                return image;
            }
            catch
            {
                image.Dispose();
                throw;
            }
        }

        /// <summary>Releases the owned native board.</summary>
        public void Dispose()
        {
            if (!disposed)
            {
                handle?.Dispose();
                disposed = true;
            }
            GC.SuppressFinalize(this);
        }

        private static NativeMethods.Point3fNative[] ToNative(Point3f[] points)
        {
            var result = new NativeMethods.Point3fNative[points.Length];
            for (int i = 0; i < points.Length; i++) result[i] = new NativeMethods.Point3fNative { X = points[i].X, Y = points[i].Y, Z = points[i].Z };
            return result;
        }

        private static NativeMethods.Point2fNative[] ToNative(Point2f[] points)
        {
            var result = new NativeMethods.Point2fNative[points.Length];
            for (int i = 0; i < points.Length; i++) result[i] = new NativeMethods.Point2fNative { X = points[i].X, Y = points[i].Y };
            return result;
        }

        private static Point3f[][] ToGroups(int[] offsets, NativeMethods.Point3fNative[] points)
        {
            var result = new Point3f[offsets.Length - 1][];
            for (int i = 0; i < result.Length; i++)
            {
                int start = offsets[i];
                int end = offsets[i + 1];
                if (start < 0 || end < start || end > points.Length) throw new OpenCvException("Native ArucoBoard offsets are invalid.");
                var group = new Point3f[end - start];
                for (int j = start; j < end; j++) group[j - start] = new Point3f(points[j].X, points[j].Y, points[j].Z);
                result[i] = group;
            }
            return result;
        }

        private static void ValidateCount(int count, string name)
        {
            if (count < 0) throw new OpenCvException("Native " + name + " is negative.");
        }

        private void ThrowIfDisposed()
        {
            if (disposed) throw new ObjectDisposedException(GetType().FullName);
        }
    }
}

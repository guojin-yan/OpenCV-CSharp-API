using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Stitching
{
    /// <summary>Owns a configured low-level spherical projector.</summary>
    public sealed class SphericalProjector : IDisposable
    {
        private NativeSphericalProjectorHandle handle;
        private bool disposed;

        /// <summary>
        /// Creates a projector from exact 3 x 3 CV_32FC1 camera and rotation matrices and an optional 3-vector translation.
        /// </summary>
        public SphericalProjector(float scale, Mat cameraMatrix, Mat rotationMatrix, Mat? translation = null)
        {
            if (float.IsNaN(scale) || float.IsInfinity(scale) || scale <= 0F) throw new ArgumentOutOfRangeException(nameof(scale));
            ValidateMatrix(cameraMatrix, nameof(cameraMatrix), 3, 3);
            ValidateMatrix(rotationMatrix, nameof(rotationMatrix), 3, 3);
            if (translation != null && (translation.Empty || translation.Dims != 2 || translation.Type != MatType.CV_32FC1 ||
                !((translation.Rows == 3 && translation.Cols == 1) || (translation.Rows == 1 && translation.Cols == 3))))
                throw new ArgumentException("Translation must be an exact 3 x 1 or 1 x 3 CV_32FC1 matrix.", nameof(translation));
            NativeException.ThrowIfError(NativeMethods.StitchingSphericalProjectorCreate(
                scale, cameraMatrix.NativeHandle, rotationMatrix.NativeHandle, translation?.NativeHandle ?? IntPtr.Zero, out IntPtr nativeHandle));
            handle = NativeSphericalProjectorHandle.FromNativePointer(nativeHandle);
            GC.KeepAlive(cameraMatrix); GC.KeepAlive(rotationMatrix); GC.KeepAlive(translation);
        }

        /// <summary>Gets whether this projector has been disposed.</summary>
        public bool IsDisposed => disposed;

        internal IntPtr NativeHandle
        {
            get { ThrowIfDisposed(); return handle.DangerousGetHandle(); }
        }

        /// <summary>Maps a source image point to spherical coordinates.</summary>
        public Point2f MapForward(Point2f point)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.StitchingSphericalProjectorMapForward(
                NativeHandle, point.X, point.Y, out float u, out float v));
            return new Point2f(u, v);
        }

        /// <summary>Maps spherical coordinates back to the source image.</summary>
        public Point2f MapBackward(Point2f point)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.StitchingSphericalProjectorMapBackward(
                NativeHandle, point.X, point.Y, out float x, out float y));
            return new Point2f(x, y);
        }

        /// <summary>Releases the owned native projector.</summary>
        public void Dispose()
        {
            if (disposed) return;
            handle.Dispose(); disposed = true; GC.SuppressFinalize(this);
        }

        private static void ValidateMatrix(Mat value, string parameterName, int rows, int cols)
        {
            if (value == null) throw new ArgumentNullException(parameterName);
            if (value.Empty || value.Dims != 2 || value.Rows != rows || value.Cols != cols || value.Type != MatType.CV_32FC1)
                throw new ArgumentException("The matrix must be an exact 3 x 3 CV_32FC1 matrix.", parameterName);
        }

        private void ThrowIfDisposed()
        {
            if (disposed) throw new ObjectDisposedException(GetType().FullName);
        }
    }
}

using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Video
{
    /// <summary>Base class for owned sparse optical-flow algorithms.</summary>
    public abstract unsafe class SparseOpticalFlow : IDisposable
    {
        private readonly NativeSparseOpticalFlowHandle handle;
        private bool disposed;

        /// <summary>Initializes an owned sparse optical-flow wrapper.</summary>
        protected SparseOpticalFlow(IntPtr nativeHandle)
        {
            handle = NativeSparseOpticalFlowHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Gets whether the native algorithm has been released.</summary>
        public bool IsDisposed => disposed;

        /// <summary>Gets the live opaque native handle.</summary>
        protected IntPtr NativeHandle
        {
            get
            {
                ThrowIfDisposed();
                return handle.DangerousGetHandle();
            }
        }

        /// <summary>Calculates sparse flow without caller-supplied initial estimates.</summary>
        public Point2f[] Calc(Mat previousImage, Mat nextImage, Point2f[] previousPoints, out byte[] status, out float[] error)
        {
            if (RequiresInitialFlow)
            {
                throw new InvalidOperationException("The configured optical-flow flags require caller-supplied initial next points.");
            }

            return CalcCore(previousImage, nextImage, previousPoints, null, out status, out error);
        }

        /// <summary>Calculates sparse flow using caller-supplied initial estimates when the algorithm flag requests them.</summary>
        public Point2f[] Calc(Mat previousImage, Mat nextImage, Point2f[] previousPoints, Point2f[] initialNextPoints, out byte[] status, out float[] error)
        {
            ValidateNotNull(initialNextPoints, nameof(initialNextPoints));
            return CalcCore(previousImage, nextImage, previousPoints, initialNextPoints, out status, out error);
        }

        private Point2f[] CalcCore(Mat previousImage, Mat nextImage, Point2f[] previousPoints, Point2f[]? initialNextPoints, out byte[] status, out float[] error)
        {
            ThrowIfDisposed();
            ValidateNotNull(previousImage, nameof(previousImage));
            ValidateNotNull(nextImage, nameof(nextImage));
            ValidateNotNull(previousPoints, nameof(previousPoints));
            if (initialNextPoints != null && initialNextPoints.Length != previousPoints.Length)
            {
                throw new ArgumentException("Initial point count must match previousPoints length.", nameof(initialNextPoints));
            }

            var nativePrevious = new NativeMethods.VideoPoint2fNative[previousPoints.Length];
            var nativeNext = new NativeMethods.VideoPoint2fNative[previousPoints.Length];
            for (int i = 0; i < previousPoints.Length; i++)
            {
                nativePrevious[i] = new NativeMethods.VideoPoint2fNative { X = previousPoints[i].X, Y = previousPoints[i].Y };
                Point2f initial = initialNextPoints == null ? default : initialNextPoints[i];
                nativeNext[i] = new NativeMethods.VideoPoint2fNative { X = initial.X, Y = initial.Y };
            }

            status = new byte[previousPoints.Length];
            error = new float[previousPoints.Length];
            fixed (NativeMethods.VideoPoint2fNative* previousPointer = nativePrevious)
            fixed (NativeMethods.VideoPoint2fNative* nextPointer = nativeNext)
            fixed (byte* statusPointer = status)
            fixed (float* errorPointer = error)
            {
                NativeException.ThrowIfError(NativeMethods.SparseOpticalFlowCalc(
                    NativeHandle, previousImage.NativeHandle, nextImage.NativeHandle,
                    previousPointer, previousPoints.Length, nextPointer, statusPointer, errorPointer));
            }

            var result = new Point2f[previousPoints.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new Point2f(nativeNext[i].X, nativeNext[i].Y);
            }
            return result;
        }

        internal virtual bool RequiresInitialFlow => false;

        /// <summary>Releases the owned native algorithm.</summary>
        public void Dispose()
        {
            if (!disposed)
            {
                handle.Dispose();
                disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>Throws when the algorithm has been disposed.</summary>
        protected void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        /// <summary>Validates a required reference argument.</summary>
        protected static void ValidateNotNull<T>(T value, string parameterName)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }
    }
}

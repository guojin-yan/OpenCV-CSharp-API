using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Video
{
    /// <summary>Base class for owned dense optical-flow algorithms.</summary>
    public abstract class DenseOpticalFlow : IDisposable
    {
        private readonly NativeDenseOpticalFlowHandle handle;
        private bool disposed;

        /// <summary>Initializes an owned dense optical-flow wrapper.</summary>
        protected DenseOpticalFlow(IntPtr nativeHandle)
        {
            handle = NativeDenseOpticalFlowHandle.FromNativePointer(nativeHandle);
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

        /// <summary>Calculates or refines a two-channel dense flow field.</summary>
        public virtual void Calc(Mat first, Mat second, Mat flow)
        {
            ValidateNotNull(first, nameof(first));
            ValidateNotNull(second, nameof(second));
            ValidateNotNull(flow, nameof(flow));
            NativeException.ThrowIfError(NativeMethods.DenseOpticalFlowCalc(NativeHandle, first.NativeHandle, second.NativeHandle, flow.NativeHandle));
        }

        /// <summary>Calculates and returns an independently owned dense flow field.</summary>
        public Mat Calc(Mat first, Mat second)
        {
            ThrowIfDisposed();
            ValidateNotNull(first, nameof(first));
            ValidateNotNull(second, nameof(second));

            var flow = new Mat(first.Rows, first.Cols, MatType.CV_32FC2, new Scalar(0, 0, 0, 0));
            try
            {
                Calc(first, second, flow);
                return flow;
            }
            catch
            {
                flow.Dispose();
                throw;
            }
        }

        /// <summary>Releases cached algorithm buffers while keeping the object usable.</summary>
        public void CollectGarbage()
        {
            NativeException.ThrowIfError(NativeMethods.DenseOpticalFlowCollectGarbage(NativeHandle));
        }

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

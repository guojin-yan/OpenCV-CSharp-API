using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Photo
{
    /// <summary>Base class for exposure-sequence merge algorithms.</summary>
    public abstract class MergeExposures : IDisposable
    {
        private readonly NativeHdrPhotoHandle handle;
        private readonly bool allowAnyDepth;
        private bool disposed;

        internal MergeExposures(NativeHdrPhotoHandle handle, bool allowAnyDepth = false)
        {
            this.handle = handle;
            this.allowAnyDepth = allowAnyDepth;
        }

        /// <summary>Gets whether this merger has been disposed.</summary>
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

        /// <summary>Merges an exposure sequence using times and camera response.</summary>
        public void Process(Mat[] src, Mat dst, Mat times, Mat response)
        {
            ProcessCore(src, dst, times, response, inputMode: 2);
        }

        /// <summary>Merges an exposure sequence and returns a new matrix.</summary>
        public Mat ProcessToMat(Mat[] src, Mat times, Mat response)
        {
            var dst = new Mat();
            try
            {
                Process(src, dst, times, response);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>Releases the native merge algorithm.</summary>
        public void Dispose()
        {
            if (!disposed)
            {
                handle.Dispose();
                disposed = true;
            }
            GC.SuppressFinalize(this);
        }

        internal void ProcessCore(Mat[] src, Mat dst, Mat times, Mat response, int inputMode)
        {
            ThrowIfDisposed();
            HdrPhotoValidation.RequireMat(dst, nameof(dst));
            IntPtr[] srcHandles = HdrPhotoValidation.GetImageHandles(
                src,
                nameof(src),
                requireEightBit: false,
                allowHighDynamicRangeDepth: true,
                requireColor: false,
                allowFourChannels: false,
                allowAnyDepth: allowAnyDepth);
            IntPtr timesHandle = IntPtr.Zero;
            IntPtr responseHandle = IntPtr.Zero;
            if (inputMode >= 1)
            {
                HdrPhotoValidation.ValidateTimes(times, srcHandles.Length, nameof(times));
                timesHandle = times.NativeHandle;
            }
            if (inputMode == 2)
            {
                HdrPhotoValidation.ValidateResponse(response, nameof(response));
                responseHandle = response.NativeHandle;
            }
            NativeException.ThrowIfError(NativeMethods.MergeExposuresProcess(
                NativeHandle,
                srcHandles,
                srcHandles.Length,
                dst.NativeHandle,
                timesHandle,
                responseHandle,
                inputMode));
            GC.KeepAlive(src);
        }

        internal void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }
}

using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Photo
{
    /// <summary>Base class for camera response calibration algorithms.</summary>
    public abstract class CalibrateCRF : IDisposable
    {
        private readonly NativeHdrPhotoHandle handle;
        private bool disposed;

        internal CalibrateCRF(NativeHdrPhotoHandle handle)
        {
            this.handle = handle;
        }

        /// <summary>Gets whether this calibrator has been disposed.</summary>
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

        /// <summary>Recovers the camera response into a caller-owned output matrix.</summary>
        public void Process(Mat[] src, Mat dst, Mat times)
        {
            ThrowIfDisposed();
            HdrPhotoValidation.RequireMat(dst, nameof(dst));
            IntPtr[] srcHandles = HdrPhotoValidation.GetImageHandles(
                src,
                nameof(src),
                requireEightBit: true,
                allowHighDynamicRangeDepth: false,
                requireColor: false,
                allowFourChannels: false,
                allowAnyDepth: false);
            HdrPhotoValidation.ValidateTimes(times, srcHandles.Length, nameof(times));
            NativeException.ThrowIfError(NativeMethods.CalibrateCrfProcess(
                NativeHandle, srcHandles, srcHandles.Length, dst.NativeHandle, times.NativeHandle));
            GC.KeepAlive(src);
        }

        /// <summary>Recovers and returns an independently owned camera-response matrix.</summary>
        public Mat Process(Mat[] src, Mat times)
        {
            var dst = new Mat();
            try
            {
                Process(src, dst, times);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>Releases the native calibration algorithm.</summary>
        public void Dispose()
        {
            if (!disposed)
            {
                handle.Dispose();
                disposed = true;
            }
            GC.SuppressFinalize(this);
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

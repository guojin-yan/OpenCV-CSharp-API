using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Photo
{
    /// <summary>Base class for exposure alignment algorithms.</summary>
    public abstract class AlignExposures : IDisposable
    {
        private readonly NativeHdrPhotoHandle handle;
        private bool disposed;

        internal AlignExposures(NativeHdrPhotoHandle handle)
        {
            this.handle = handle;
        }

        /// <summary>Gets whether this aligner has been disposed.</summary>
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

        /// <summary>Aligns an exposure sequence into caller-owned output matrices.</summary>
        public void Process(Mat[] src, Mat[] dst, Mat times, Mat response)
        {
            ProcessCore(src, dst, times, response, true);
        }

        /// <summary>Aligns an exposure sequence and returns independently owned matrix headers.</summary>
        public Mat[] Process(Mat[] src, Mat times, Mat response)
        {
            if (src == null)
            {
                throw new ArgumentNullException(nameof(src));
            }
            Mat[] dst = HdrPhotoValidation.CreateOutputMats(src.Length);
            try
            {
                Process(src, dst, times, response);
                return dst;
            }
            catch
            {
                HdrPhotoValidation.DisposeAll(dst);
                throw;
            }
        }

        /// <summary>Releases the native alignment algorithm.</summary>
        public void Dispose()
        {
            if (!disposed)
            {
                handle.Dispose();
                disposed = true;
            }
            GC.SuppressFinalize(this);
        }

        internal void ProcessCore(Mat[] src, Mat[] dst, Mat times, Mat response, bool useExtraInputs)
        {
            ThrowIfDisposed();
            IntPtr[] srcHandles = HdrPhotoValidation.GetImageHandles(
                src,
                nameof(src),
                requireEightBit: true,
                allowHighDynamicRangeDepth: false,
                requireColor: true,
                allowFourChannels: true,
                allowAnyDepth: false);
            IntPtr[] dstHandles = HdrPhotoValidation.GetOutputHandles(dst, srcHandles.Length, nameof(dst));
            IntPtr timesHandle = IntPtr.Zero;
            IntPtr responseHandle = IntPtr.Zero;
            if (useExtraInputs)
            {
                HdrPhotoValidation.ValidateTimes(times, srcHandles.Length, nameof(times));
                HdrPhotoValidation.ValidateResponse(response, nameof(response));
                timesHandle = times.NativeHandle;
                responseHandle = response.NativeHandle;
            }
            NativeException.ThrowIfError(NativeMethods.AlignMtbProcess(
                NativeHandle,
                srcHandles,
                dstHandles,
                srcHandles.Length,
                timesHandle,
                responseHandle,
                useExtraInputs ? 1 : 0));
            GC.KeepAlive(src);
            GC.KeepAlive(dst);
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

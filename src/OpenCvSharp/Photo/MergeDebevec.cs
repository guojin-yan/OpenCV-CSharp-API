using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Photo
{
    /// <summary>Debevec high-dynamic-range exposure merger.</summary>
    public sealed class MergeDebevec : MergeExposures
    {
        private MergeDebevec(NativeHdrPhotoHandle handle)
            : base(handle)
        {
        }

        /// <summary>Creates a Debevec merger.</summary>
        public static MergeDebevec Create()
        {
            NativeException.ThrowIfError(NativeMethods.MergeDebevecCreate(out IntPtr native));
            return new MergeDebevec(
                NativeHdrPhotoHandle.FromNativePointer(native, HdrPhotoHandleKind.MergeExposures));
        }

        /// <summary>Merges an exposure sequence using a linear camera response.</summary>
        public void Process(Mat[] src, Mat dst, Mat times)
        {
            ProcessCore(src, dst, times, null!, inputMode: 1);
        }

        /// <summary>Merges an exposure sequence using a linear response and returns a new matrix.</summary>
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
    }
}

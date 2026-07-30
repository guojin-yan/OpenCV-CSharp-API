using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Photo
{
    /// <summary>Robertson high-dynamic-range exposure merger.</summary>
    public sealed class MergeRobertson : MergeExposures
    {
        private MergeRobertson(NativeHdrPhotoHandle handle)
            : base(handle)
        {
        }

        /// <summary>Creates a Robertson merger.</summary>
        public static MergeRobertson Create()
        {
            NativeException.ThrowIfError(NativeMethods.MergeRobertsonCreate(out IntPtr native));
            return new MergeRobertson(
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

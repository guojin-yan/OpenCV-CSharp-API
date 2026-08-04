using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Photo
{
    /// <summary>Mertens exposure-fusion merger.</summary>
    public sealed class MergeMertens : MergeExposures
    {
        private MergeMertens(NativeHdrPhotoHandle handle)
            : base(handle, allowAnyDepth: true)
        {
        }

        /// <summary>Gets or sets the contrast-measure weight.</summary>
        public float ContrastWeight
        {
            get
            {
                NativeException.ThrowIfError(NativeMethods.MergeMertensGetContrastWeight(NativeHandle, out float value));
                return value;
            }
            set { NativeException.ThrowIfError(NativeMethods.MergeMertensSetContrastWeight(NativeHandle, value)); }
        }

        /// <summary>Gets or sets the saturation-measure weight.</summary>
        public float SaturationWeight
        {
            get
            {
                NativeException.ThrowIfError(NativeMethods.MergeMertensGetSaturationWeight(NativeHandle, out float value));
                return value;
            }
            set { NativeException.ThrowIfError(NativeMethods.MergeMertensSetSaturationWeight(NativeHandle, value)); }
        }

        /// <summary>Gets or sets the well-exposedness weight.</summary>
        public float ExposureWeight
        {
            get
            {
                NativeException.ThrowIfError(NativeMethods.MergeMertensGetExposureWeight(NativeHandle, out float value));
                return value;
            }
            set { NativeException.ThrowIfError(NativeMethods.MergeMertensSetExposureWeight(NativeHandle, value)); }
        }

        /// <summary>Creates a Mertens exposure-fusion merger.</summary>
        public static MergeMertens Create(
            float contrastWeight = 1.0F,
            float saturationWeight = 1.0F,
            float exposureWeight = 0.0F)
        {
            NativeException.ThrowIfError(NativeMethods.MergeMertensCreate(
                contrastWeight, saturationWeight, exposureWeight, out IntPtr native));
            return new MergeMertens(
                NativeHdrPhotoHandle.FromNativePointer(native, HdrPhotoHandleKind.MergeExposures));
        }

        /// <summary>Fuses an exposure sequence without times or camera response.</summary>
        public void Process(Mat[] src, Mat dst)
        {
            ProcessCore(src, dst, null!, null!, inputMode: 0);
        }

        /// <summary>Fuses an exposure sequence and returns a new matrix.</summary>
        public Mat Process(Mat[] src)
        {
            var dst = new Mat();
            try
            {
                Process(src, dst);
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

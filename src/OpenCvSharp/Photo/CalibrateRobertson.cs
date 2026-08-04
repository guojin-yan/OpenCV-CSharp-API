using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Photo
{
    /// <summary>Robertson inverse camera-response calibration.</summary>
    public sealed class CalibrateRobertson : CalibrateCRF
    {
        private CalibrateRobertson(NativeHdrPhotoHandle handle)
            : base(handle)
        {
        }

        /// <summary>Gets or sets the maximum solver iteration count.</summary>
        public int MaxIter
        {
            get
            {
                NativeException.ThrowIfError(NativeMethods.CalibrateRobertsonGetMaxIter(NativeHandle, out int value));
                return value;
            }
            set { NativeException.ThrowIfError(NativeMethods.CalibrateRobertsonSetMaxIter(NativeHandle, value)); }
        }

        /// <summary>Gets or sets the convergence threshold.</summary>
        public float Threshold
        {
            get
            {
                NativeException.ThrowIfError(NativeMethods.CalibrateRobertsonGetThreshold(NativeHandle, out float value));
                return value;
            }
            set { NativeException.ThrowIfError(NativeMethods.CalibrateRobertsonSetThreshold(NativeHandle, value)); }
        }

        /// <summary>Creates a Robertson calibrator.</summary>
        public static CalibrateRobertson Create(int maxIter = 30, float threshold = 0.01F)
        {
            NativeException.ThrowIfError(NativeMethods.CalibrateRobertsonCreate(maxIter, threshold, out IntPtr native));
            return new CalibrateRobertson(
                NativeHdrPhotoHandle.FromNativePointer(native, HdrPhotoHandleKind.CalibrateCrf));
        }

        /// <summary>Copies the most recently computed radiance image to a caller-owned matrix.</summary>
        public void GetRadiance(Mat radiance)
        {
            ThrowIfDisposed();
            HdrPhotoValidation.RequireMat(radiance, nameof(radiance));
            NativeException.ThrowIfError(NativeMethods.CalibrateRobertsonGetRadiance(NativeHandle, radiance.NativeHandle));
        }

        /// <summary>Returns an independently owned copy of the most recently computed radiance image.</summary>
        public Mat GetRadiance()
        {
            var radiance = new Mat();
            try
            {
                GetRadiance(radiance);
                return radiance;
            }
            catch
            {
                radiance.Dispose();
                throw;
            }
        }
    }
}

using System;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Photo
{
    /// <summary>Debevec inverse camera-response calibration.</summary>
    public sealed class CalibrateDebevec : CalibrateCRF
    {
        private CalibrateDebevec(NativeHdrPhotoHandle handle)
            : base(handle)
        {
        }

        /// <summary>Gets or sets the smoothness weight.</summary>
        public float Lambda
        {
            get
            {
                NativeException.ThrowIfError(NativeMethods.CalibrateDebevecGetLambda(NativeHandle, out float value));
                return value;
            }
            set { NativeException.ThrowIfError(NativeMethods.CalibrateDebevecSetLambda(NativeHandle, value)); }
        }

        /// <summary>Gets or sets the number of sampled pixel locations.</summary>
        public int Samples
        {
            get
            {
                NativeException.ThrowIfError(NativeMethods.CalibrateDebevecGetSamples(NativeHandle, out int value));
                return value;
            }
            set { NativeException.ThrowIfError(NativeMethods.CalibrateDebevecSetSamples(NativeHandle, value)); }
        }

        /// <summary>Gets or sets whether sample locations are randomized.</summary>
        public bool Random
        {
            get
            {
                NativeException.ThrowIfError(NativeMethods.CalibrateDebevecGetRandom(NativeHandle, out int value));
                return value != 0;
            }
            set { NativeException.ThrowIfError(NativeMethods.CalibrateDebevecSetRandom(NativeHandle, value ? 1 : 0)); }
        }

        /// <summary>Creates a Debevec calibrator.</summary>
        public static CalibrateDebevec Create(int samples = 70, float lambda = 10.0F, bool random = false)
        {
            NativeException.ThrowIfError(NativeMethods.CalibrateDebevecCreate(
                samples, lambda, random ? 1 : 0, out IntPtr native));
            return new CalibrateDebevec(
                NativeHdrPhotoHandle.FromNativePointer(native, HdrPhotoHandleKind.CalibrateCrf));
        }
    }
}

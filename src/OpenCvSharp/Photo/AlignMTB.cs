using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Photo
{
    /// <summary>Median-threshold bitmap exposure alignment.</summary>
    public sealed class AlignMTB : AlignExposures
    {
        private AlignMTB(NativeHdrPhotoHandle handle)
            : base(handle)
        {
        }

        /// <summary>Gets or sets the base-2 maximum shift level.</summary>
        public int MaxBits
        {
            get
            {
                NativeException.ThrowIfError(NativeMethods.AlignMtbGetMaxBits(NativeHandle, out int value));
                return value;
            }
            set { NativeException.ThrowIfError(NativeMethods.AlignMtbSetMaxBits(NativeHandle, value)); }
        }

        /// <summary>Gets or sets the median exclusion range.</summary>
        public int ExcludeRange
        {
            get
            {
                NativeException.ThrowIfError(NativeMethods.AlignMtbGetExcludeRange(NativeHandle, out int value));
                return value;
            }
            set { NativeException.ThrowIfError(NativeMethods.AlignMtbSetExcludeRange(NativeHandle, value)); }
        }

        /// <summary>Gets or sets whether aligned borders are cropped.</summary>
        public bool Cut
        {
            get
            {
                NativeException.ThrowIfError(NativeMethods.AlignMtbGetCut(NativeHandle, out int value));
                return value != 0;
            }
            set { NativeException.ThrowIfError(NativeMethods.AlignMtbSetCut(NativeHandle, value ? 1 : 0)); }
        }

        /// <summary>Creates a median-threshold bitmap aligner.</summary>
        public static AlignMTB Create(int maxBits = 6, int excludeRange = 4, bool cut = true)
        {
            NativeException.ThrowIfError(NativeMethods.AlignMtbCreate(maxBits, excludeRange, cut ? 1 : 0, out IntPtr native));
            return new AlignMTB(NativeHdrPhotoHandle.FromNativePointer(native, HdrPhotoHandleKind.AlignMtb));
        }

        /// <summary>Aligns an exposure sequence without times or response inputs.</summary>
        public void Process(Mat[] src, Mat[] dst)
        {
            ProcessCore(src, dst, null!, null!, false);
        }

        /// <summary>Aligns an exposure sequence without times or response inputs.</summary>
        public Mat[] Process(Mat[] src)
        {
            if (src == null)
            {
                throw new ArgumentNullException(nameof(src));
            }
            Mat[] dst = HdrPhotoValidation.CreateOutputMats(src.Length);
            try
            {
                Process(src, dst);
                return dst;
            }
            catch
            {
                HdrPhotoValidation.DisposeAll(dst);
                throw;
            }
        }

        /// <summary>Calculates the shift needed to align the second grayscale image to the first.</summary>
        public Point CalculateShift(Mat img0, Mat img1)
        {
            ThrowIfDisposed();
            HdrPhotoValidation.ValidateSingleChannelByte(img0, nameof(img0));
            HdrPhotoValidation.ValidateSingleChannelByte(img1, nameof(img1));
            if (img0.Rows != img1.Rows || img0.Cols != img1.Cols)
            {
                throw new ArgumentException("Shift images must have the same size.", nameof(img1));
            }
            NativeException.ThrowIfError(NativeMethods.AlignMtbCalculateShift(
                NativeHandle, img0.NativeHandle, img1.NativeHandle, out int x, out int y));
            return new Point(x, y);
        }

        /// <summary>Shifts a matrix into a distinct caller-owned output.</summary>
        public void ShiftMat(Mat src, Mat dst, Point shift)
        {
            ThrowIfDisposed();
            HdrPhotoValidation.RequireMat(src, nameof(src));
            HdrPhotoValidation.RequireMat(dst, nameof(dst));
            if (src.Empty)
            {
                throw new ArgumentException("Source matrix cannot be empty.", nameof(src));
            }
            HdrPhotoValidation.ValidateDistinct(src, dst, nameof(dst));
            if (Math.Abs((long)shift.X) >= src.Cols || Math.Abs((long)shift.Y) >= src.Rows)
            {
                throw new ArgumentOutOfRangeException(nameof(shift), "Shift must leave a non-empty overlap.");
            }
            NativeException.ThrowIfError(NativeMethods.AlignMtbShiftMat(
                NativeHandle, src.NativeHandle, dst.NativeHandle, shift.X, shift.Y));
        }

        /// <summary>Shifts a matrix and returns a new output matrix.</summary>
        public Mat ShiftMat(Mat src, Point shift)
        {
            var dst = new Mat();
            try
            {
                ShiftMat(src, dst, shift);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>Computes median-threshold and exclusion bitmaps.</summary>
        public void ComputeBitmaps(Mat img, Mat thresholdBitmap, Mat excludeBitmap)
        {
            ThrowIfDisposed();
            HdrPhotoValidation.ValidateSingleChannelByte(img, nameof(img));
            HdrPhotoValidation.RequireMat(thresholdBitmap, nameof(thresholdBitmap));
            HdrPhotoValidation.RequireMat(excludeBitmap, nameof(excludeBitmap));
            HdrPhotoValidation.ValidateDistinct(img, thresholdBitmap, nameof(thresholdBitmap));
            HdrPhotoValidation.ValidateDistinct(img, excludeBitmap, nameof(excludeBitmap));
            HdrPhotoValidation.ValidateDistinct(thresholdBitmap, excludeBitmap, nameof(excludeBitmap));
            NativeException.ThrowIfError(NativeMethods.AlignMtbComputeBitmaps(
                NativeHandle, img.NativeHandle, thresholdBitmap.NativeHandle, excludeBitmap.NativeHandle));
        }
    }
}

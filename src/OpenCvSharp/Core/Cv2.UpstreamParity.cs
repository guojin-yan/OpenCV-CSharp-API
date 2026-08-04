using System;
using JYPPX.OpenCvSharp.ImgProc;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Core
{
    public static partial class Cv2
    {
        /// <summary>Maps an out-of-range coordinate into a one-dimensional border.</summary>
        public static int BorderInterpolate(int p, int len, BorderTypes borderType)
        {
            if (len <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(len), "Length must be positive.");
            }

            NativeException.ThrowIfError(NativeMethods.CoreBorderInterpolate(p, len, (int)borderType, out int value));
            return value;
        }

        /// <summary>Copies a matrix into caller-owned output and extrapolates its border.</summary>
        public static void CopyMakeBorder(
            Mat src, Mat dst, int top, int bottom, int left, int right,
            BorderTypes borderType, Scalar? value = null)
        {
            ValidateMatPair(src, dst);
            ValidateNonNegative(top, nameof(top));
            ValidateNonNegative(bottom, nameof(bottom));
            ValidateNonNegative(left, nameof(left));
            ValidateNonNegative(right, nameof(right));
            Scalar borderValue = value ?? new Scalar(0);
            NativeException.ThrowIfError(NativeMethods.CoreCopyMakeBorder(
                src.NativeHandle, dst.NativeHandle, top, bottom, left, right, (int)borderType,
                borderValue.V0, borderValue.V1, borderValue.V2, borderValue.V3));
        }

        /// <summary>Returns a newly owned bordered matrix.</summary>
        public static Mat CopyMakeBorder(
            Mat src, int top, int bottom, int left, int right,
            BorderTypes borderType, Scalar? value = null)
        {
            return CreateOutput(dst => CopyMakeBorder(src, dst, top, bottom, left, right, borderType, value));
        }

        /// <summary>Reports whether a single-channel matrix contains a non-zero element.</summary>
        public static bool HasNonZero(Mat src)
        {
            ValidateNotNull(src, nameof(src));
            ValidateSingleChannel(src, nameof(src), "HasNonZero");
            NativeException.ThrowIfError(NativeMethods.CoreHasNonZero(src.NativeHandle, out int value));
            return value != 0;
        }

        /// <summary>Writes non-zero element coordinates as a CV_32SC2 matrix.</summary>
        public static void FindNonZero(Mat src, Mat dst)
        {
            ValidateMatPair(src, dst);
            ValidateSingleChannel(src, nameof(src), "FindNonZero");
            NativeException.ThrowIfError(NativeMethods.CoreFindNonZero(src.NativeHandle, dst.NativeHandle));
        }

        /// <summary>Returns newly owned non-zero element coordinates.</summary>
        public static Mat FindNonZero(Mat src)
        {
            return CreateOutput(dst => FindNonZero(src, dst));
        }

        /// <summary>Calculates peak signal-to-noise ratio for equal-shaped, equal-typed matrices.</summary>
        public static double Psnr(Mat src1, Mat src2, double maxValue = 255.0)
        {
            ValidateNotNull(src1, nameof(src1));
            ValidateNotNull(src2, nameof(src2));
            if (!(maxValue > 0.0) || double.IsInfinity(maxValue) || double.IsNaN(maxValue))
            {
                throw new ArgumentOutOfRangeException(nameof(maxValue), "Maximum value must be finite and positive.");
            }
            if (src1.Type != src2.Type || !src1.Size.Equals(src2.Size) || src1.Dims != src2.Dims)
            {
                throw new ArgumentException("PSNR inputs must have matching shape and type.", nameof(src2));
            }

            NativeException.ThrowIfError(NativeMethods.CorePsnr(src1.NativeHandle, src2.NativeHandle, maxValue, out double value));
            return value;
        }

        /// <summary>Writes minimum-value indices along axis 0 or 1.</summary>
        public static void ReduceArgMin(Mat src, Mat dst, int axis, bool lastIndex = false)
        {
            ValidateReduceArg(src, dst, axis);
            NativeException.ThrowIfError(NativeMethods.CoreReduceArgMin(src.NativeHandle, dst.NativeHandle, axis, lastIndex ? 1 : 0));
        }

        /// <summary>Returns newly owned minimum-value indices along an axis.</summary>
        public static Mat ReduceArgMin(Mat src, int axis, bool lastIndex = false)
        {
            return CreateOutput(dst => ReduceArgMin(src, dst, axis, lastIndex));
        }

        /// <summary>Writes maximum-value indices along axis 0 or 1.</summary>
        public static void ReduceArgMax(Mat src, Mat dst, int axis, bool lastIndex = false)
        {
            ValidateReduceArg(src, dst, axis);
            NativeException.ThrowIfError(NativeMethods.CoreReduceArgMax(src.NativeHandle, dst.NativeHandle, axis, lastIndex ? 1 : 0));
        }

        /// <summary>Returns newly owned maximum-value indices along an axis.</summary>
        public static Mat ReduceArgMax(Mat src, int axis, bool lastIndex = false)
        {
            return CreateOutput(dst => ReduceArgMax(src, dst, axis, lastIndex));
        }

        /// <summary>Flips an n-dimensional matrix along the specified positive or negative axis.</summary>
        public static void FlipND(Mat src, Mat dst, int axis)
        {
            ValidateMatPair(src, dst);
            if (axis < -src.Dims || axis >= src.Dims)
            {
                throw new ArgumentOutOfRangeException(nameof(axis));
            }
            NativeException.ThrowIfError(NativeMethods.CoreFlipNd(src.NativeHandle, dst.NativeHandle, axis));
        }

        /// <summary>Returns a newly owned n-dimensional flipped matrix.</summary>
        public static Mat FlipND(Mat src, int axis)
        {
            return CreateOutput(dst => FlipND(src, dst, axis));
        }

        /// <summary>Broadcasts a continuous single-channel source to a CV_32SC1 target shape.</summary>
        public static void Broadcast(Mat src, Mat shape, Mat dst)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(shape, nameof(shape));
            ValidateNotNull(dst, nameof(dst));
            ValidateSingleChannel(src, nameof(src), "Broadcast");
            if (!src.IsContinuous)
            {
                throw new ArgumentException("Broadcast requires a continuous source matrix.", nameof(src));
            }
            if (shape.Type != MatType.CV_32SC1 || shape.Empty)
            {
                throw new ArgumentException("Broadcast shape must be a non-empty CV_32SC1 matrix.", nameof(shape));
            }
            NativeException.ThrowIfError(NativeMethods.CoreBroadcast(src.NativeHandle, shape.NativeHandle, dst.NativeHandle));
        }

        /// <summary>Returns a newly owned broadcast matrix.</summary>
        public static Mat Broadcast(Mat src, Mat shape)
        {
            return CreateOutput(dst => Broadcast(src, shape, dst));
        }

        /// <summary>Copies selected elements into caller-owned output using an 8-bit mask.</summary>
        public static void CopyTo(Mat src, Mat dst, Mat mask)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            ValidateNotNull(mask, nameof(mask));
            if (mask.Type != MatType.CV_8UC1 || !mask.Size.Equals(src.Size))
            {
                throw new ArgumentException("Copy mask must be CV_8UC1 and match the source size.", nameof(mask));
            }
            NativeException.ThrowIfError(NativeMethods.CoreCopyToMask(src.NativeHandle, dst.NativeHandle, mask.NativeHandle));
        }

        /// <summary>Checks finiteness and membership in the half-open interval [minValue,maxValue).</summary>
        public static CheckRangeResult CheckRange(Mat src, double minValue = double.MinValue, double maxValue = double.MaxValue)
        {
            ValidateNotNull(src, nameof(src));
            if (double.IsNaN(minValue) || double.IsNaN(maxValue) || minValue >= maxValue)
            {
                throw new ArgumentException("Check range must be a non-empty half-open interval.", nameof(maxValue));
            }
            NativeException.ThrowIfError(NativeMethods.CoreCheckRange(src.NativeHandle, minValue, maxValue, out int valid, out int x, out int y));
            return new CheckRangeResult(valid != 0, new Point(x, y));
        }

        /// <summary>Writes an 8-bit mask for finite floating-point elements.</summary>
        public static void FiniteMask(Mat src, Mat dst)
        {
            ValidateMatPair(src, dst);
            if ((src.Depth != MatType.CV_32F && src.Depth != MatType.CV_64F) || src.Channels > 4)
            {
                throw new ArgumentException("FiniteMask requires CV_32F or CV_64F input with at most four channels.", nameof(src));
            }
            NativeException.ThrowIfError(NativeMethods.CoreFiniteMask(src.NativeHandle, dst.NativeHandle));
        }

        /// <summary>Returns a newly owned finite-element mask.</summary>
        public static Mat FiniteMask(Mat src)
        {
            return CreateOutput(dst => FiniteMask(src, dst));
        }

        /// <summary>Permutes every dimension of a continuous single-channel matrix into distinct output.</summary>
        public static void TransposeND(Mat src, int[] order, Mat dst)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(order, nameof(order));
            ValidateNotNull(dst, nameof(dst));
            ValidateSingleChannel(src, nameof(src), "TransposeND");
            if (!src.IsContinuous)
            {
                throw new ArgumentException("TransposeND requires a continuous source matrix.", nameof(src));
            }
            ValidatePermutation(order, src.Dims);
            if (src.NativeHandle == dst.NativeHandle)
            {
                throw new ArgumentException("TransposeND does not support in-place output.", nameof(dst));
            }
            NativeException.ThrowIfError(NativeMethods.CoreTransposeNd(src.NativeHandle, order, order.Length, dst.NativeHandle));
        }

        /// <summary>Returns a newly owned n-dimensional transpose.</summary>
        public static Mat TransposeND(Mat src, int[] order)
        {
            return CreateOutput(dst => TransposeND(src, order, dst));
        }

        /// <summary>Sorts a single-channel matrix into caller-owned output.</summary>
        public static void Sort(Mat src, Mat dst, SortFlags flags = SortFlags.EveryRow | SortFlags.Ascending)
        {
            ValidateSort(src, dst, flags);
            NativeException.ThrowIfError(NativeMethods.CoreSort(src.NativeHandle, dst.NativeHandle, (int)flags));
        }

        /// <summary>Returns a newly owned sorted matrix.</summary>
        public static Mat Sort(Mat src, SortFlags flags = SortFlags.EveryRow | SortFlags.Ascending)
        {
            return CreateOutput(dst => Sort(src, dst, flags));
        }

        /// <summary>Writes CV_32S source indices in sorted order.</summary>
        public static void SortIdx(Mat src, Mat dst, SortFlags flags = SortFlags.EveryRow | SortFlags.Ascending)
        {
            ValidateSort(src, dst, flags);
            NativeException.ThrowIfError(NativeMethods.CoreSortIdx(src.NativeHandle, dst.NativeHandle, (int)flags));
        }

        /// <summary>Returns newly owned CV_32S source indices in sorted order.</summary>
        public static Mat SortIdx(Mat src, SortFlags flags = SortFlags.EveryRow | SortFlags.Ascending)
        {
            return CreateOutput(dst => SortIdx(src, dst, flags));
        }

        private static Mat CreateOutput(Action<Mat> operation)
        {
            var dst = new Mat();
            try
            {
                operation(dst);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        private static void ValidateNonNegative(int value, string parameterName)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Value cannot be negative.");
            }
        }

        private static void ValidateSingleChannel(Mat src, string parameterName, string operation)
        {
            if (src.Channels != 1)
            {
                throw new ArgumentException(operation + " requires a single-channel source matrix.", parameterName);
            }
        }

        private static void ValidateReduceArg(Mat src, Mat dst, int axis)
        {
            ValidateMatPair(src, dst);
            ValidateSingleChannel(src, nameof(src), "ReduceArg");
            if (src.Dims != 2 || (axis != 0 && axis != 1))
            {
                throw new ArgumentOutOfRangeException(nameof(axis), "ReduceArg requires a two-dimensional source and axis 0 or 1.");
            }
        }

        private static void ValidatePermutation(int[] order, int dimensions)
        {
            if (order.Length != dimensions)
            {
                throw new ArgumentException("Permutation length must equal the matrix dimensionality.", nameof(order));
            }
            var seen = new bool[dimensions];
            for (int i = 0; i < order.Length; i++)
            {
                int value = order[i];
                if (value < 0 || value >= dimensions || seen[value])
                {
                    throw new ArgumentException("Order must be a complete dimension permutation.", nameof(order));
                }
                seen[value] = true;
            }
        }

        private static void ValidateSort(Mat src, Mat dst, SortFlags flags)
        {
            ValidateMatPair(src, dst);
            ValidateSingleChannel(src, nameof(src), "Sort");
            int value = (int)flags;
            if ((value & ~17) != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(flags));
            }
        }
    }
}

using System;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Core
{
    public static partial class Cv2
    {
        /// <summary>
        /// Calculates the per-element sum of two arrays.
        /// 计算两个数组逐元素相加的结果。
        /// </summary>
        public static void Add(Mat src1, Mat src2, Mat dst, Mat? mask = null, int dtype = -1)
        {
            ValidateMatTriple(src1, src2, dst);
            ValidateMatchingEmptyInputs(src1, src2, "Add");
            ValidateArithmeticArrayOrScalarInputs(src1, src2, dtype, "Add");
            ValidateOperationMask(src1, mask, "Add");
            NativeException.ThrowIfError(NativeMethods.CoreAdd(src1.NativeHandle, src2.NativeHandle, dst.NativeHandle, OptionalHandle(mask), dtype));
        }

        /// <summary>
        /// Calculates the per-element sum of two arrays and returns a new matrix.
        /// 计算两个数组逐元素相加的结果，并返回新矩阵。
        /// </summary>
        public static Mat Add(Mat src1, Mat src2, int dtype = -1)
        {
            var dst = new Mat();
            try
            {
                Add(src1, src2, dst, null, dtype);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Adds a scalar to each array element.
        /// 将标量逐元素加到数组上。
        /// </summary>
        public static void Add(Mat src, Scalar value, Mat dst, Mat? mask = null, int dtype = -1)
        {
            ValidateMatPair(src, dst);
            ValidateOperationMask(src, mask, "Add");
            NativeException.ThrowIfError(NativeMethods.CoreAddScalar(src.NativeHandle, value.V0, value.V1, value.V2, value.V3, dst.NativeHandle, OptionalHandle(mask), dtype));
        }

        /// <summary>
        /// Adds a scalar to each array element and returns a new matrix.
        /// 将标量逐元素加到数组上，并返回新矩阵。
        /// </summary>
        public static Mat Add(Mat src, Scalar value, int dtype = -1)
        {
            var dst = new Mat();
            try
            {
                Add(src, value, dst, null, dtype);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Calculates the per-element difference of two arrays.
        /// 计算两个数组逐元素相减的结果。
        /// </summary>
        public static void Subtract(Mat src1, Mat src2, Mat dst, Mat? mask = null, int dtype = -1)
        {
            ValidateMatTriple(src1, src2, dst);
            ValidateMatchingEmptyInputs(src1, src2, "Subtract");
            ValidateArithmeticArrayOrScalarInputs(src1, src2, dtype, "Subtract");
            ValidateOperationMask(src1, mask, "Subtract");
            NativeException.ThrowIfError(NativeMethods.CoreSubtract(src1.NativeHandle, src2.NativeHandle, dst.NativeHandle, OptionalHandle(mask), dtype));
        }

        /// <summary>
        /// Calculates the per-element difference of two arrays and returns a new matrix.
        /// 计算两个数组逐元素相减的结果，并返回新矩阵。
        /// </summary>
        public static Mat Subtract(Mat src1, Mat src2, int dtype = -1)
        {
            var dst = new Mat();
            try
            {
                Subtract(src1, src2, dst, null, dtype);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Subtracts a scalar from each array element.
        /// 从数组每个元素中减去标量。
        /// </summary>
        public static void Subtract(Mat src, Scalar value, Mat dst, Mat? mask = null, int dtype = -1)
        {
            ValidateMatPair(src, dst);
            ValidateOperationMask(src, mask, "Subtract");
            NativeException.ThrowIfError(NativeMethods.CoreSubtractScalar(src.NativeHandle, value.V0, value.V1, value.V2, value.V3, dst.NativeHandle, OptionalHandle(mask), dtype));
        }

        /// <summary>
        /// Subtracts a scalar from each array element and returns a new matrix.
        /// 从数组每个元素中减去标量，并返回新矩阵。
        /// </summary>
        public static Mat Subtract(Mat src, Scalar value, int dtype = -1)
        {
            var dst = new Mat();
            try
            {
                Subtract(src, value, dst, null, dtype);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Calculates the per-element scaled product of two arrays.
        /// 计算两个数组逐元素相乘并缩放的结果。
        /// </summary>
        public static void Multiply(Mat src1, Mat src2, Mat dst, double scale = 1.0, int dtype = -1)
        {
            ValidateMatTriple(src1, src2, dst);
            ValidateArithmeticArrayInputs(src1, src2, dtype, "Multiply");
            NativeException.ThrowIfError(NativeMethods.CoreMultiply(src1.NativeHandle, src2.NativeHandle, dst.NativeHandle, scale, dtype));
        }

        /// <summary>
        /// Calculates the per-element scaled product of two arrays and returns a new matrix.
        /// 计算两个数组逐元素相乘并缩放的结果，并返回新矩阵。
        /// </summary>
        public static Mat Multiply(Mat src1, Mat src2, double scale = 1.0, int dtype = -1)
        {
            var dst = new Mat();
            try
            {
                Multiply(src1, src2, dst, scale, dtype);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Calculates the per-element scaled quotient of two arrays.
        /// 计算两个数组逐元素相除并缩放的结果。
        /// </summary>
        public static void Divide(Mat src1, Mat src2, Mat dst, double scale = 1.0, int dtype = -1)
        {
            ValidateMatTriple(src1, src2, dst);
            ValidateMatchingEmptyInputs(src1, src2, "Divide");
            ValidateArithmeticArrayInputs(src1, src2, dtype, "Divide");
            NativeException.ThrowIfError(NativeMethods.CoreDivide(src1.NativeHandle, src2.NativeHandle, dst.NativeHandle, scale, dtype));
        }

        /// <summary>
        /// Calculates the per-element scaled quotient of two arrays and returns a new matrix.
        /// 计算两个数组逐元素相除并缩放的结果，并返回新矩阵。
        /// </summary>
        public static Mat Divide(Mat src1, Mat src2, double scale = 1.0, int dtype = -1)
        {
            var dst = new Mat();
            try
            {
                Divide(src1, src2, dst, scale, dtype);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Calculates <c>dst = src1 * alpha + src2</c>.
        /// 计算 <c>dst = src1 * alpha + src2</c>。
        /// </summary>
        public static void ScaleAdd(Mat src1, double alpha, Mat src2, Mat dst)
        {
            ValidateMatTriple(src1, src2, dst);
            ValidateBinaryArrayInputs(src1, src2, "ScaleAdd");
            NativeException.ThrowIfError(NativeMethods.CoreScaleAdd(src1.NativeHandle, alpha, src2.NativeHandle, dst.NativeHandle));
        }

        /// <summary>
        /// Calculates <c>dst = src1 * alpha + src2</c> and returns a new matrix.
        /// 计算 <c>dst = src1 * alpha + src2</c>，并返回新矩阵。
        /// </summary>
        public static Mat ScaleAdd(Mat src1, double alpha, Mat src2)
        {
            var dst = new Mat();
            try
            {
                ScaleAdd(src1, alpha, src2, dst);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Calculates the weighted sum of two arrays.
        /// 计算两个数组的加权和。
        /// </summary>
        public static void AddWeighted(Mat src1, double alpha, Mat src2, double beta, double gamma, Mat dst, int dtype = -1)
        {
            ValidateMatTriple(src1, src2, dst);
            ValidateMatchingEmptyInputs(src1, src2, "AddWeighted");
            ValidateArithmeticArrayInputs(src1, src2, dtype, "AddWeighted");
            NativeException.ThrowIfError(NativeMethods.CoreAddWeighted(src1.NativeHandle, alpha, src2.NativeHandle, beta, gamma, dst.NativeHandle, dtype));
        }

        /// <summary>
        /// Calculates the weighted sum of two arrays and returns a new matrix.
        /// 计算两个数组的加权和，并返回新矩阵。
        /// </summary>
        public static Mat AddWeighted(Mat src1, double alpha, Mat src2, double beta, double gamma, int dtype = -1)
        {
            var dst = new Mat();
            try
            {
                AddWeighted(src1, alpha, src2, beta, gamma, dst, dtype);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Calculates the per-element absolute difference of two arrays.
        /// 计算两个数组逐元素绝对差。
        /// </summary>
        public static void AbsDiff(Mat src1, Mat src2, Mat dst)
        {
            ValidateMatTriple(src1, src2, dst);
            ValidateMatchingEmptyInputs(src1, src2, "AbsDiff");
            ValidateBinaryArrayInputs(src1, src2, "AbsDiff");
            NativeException.ThrowIfError(NativeMethods.CoreAbsDiff(src1.NativeHandle, src2.NativeHandle, dst.NativeHandle));
        }

        /// <summary>
        /// Calculates the per-element absolute difference of two arrays and returns a new matrix.
        /// 计算两个数组逐元素绝对差，并返回新矩阵。
        /// </summary>
        public static Mat AbsDiff(Mat src1, Mat src2)
        {
            var dst = new Mat();
            try
            {
                AbsDiff(src1, src2, dst);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Calculates the per-element absolute difference between an array and a scalar.
        /// 计算数组与标量的逐元素绝对差。
        /// </summary>
        public static void AbsDiff(Mat src, Scalar value, Mat dst)
        {
            ValidateMatPair(src, dst);
            NativeException.ThrowIfError(NativeMethods.CoreAbsDiffScalar(src.NativeHandle, value.V0, value.V1, value.V2, value.V3, dst.NativeHandle));
        }

        /// <summary>
        /// Calculates the per-element absolute difference between an array and a scalar and returns a new matrix.
        /// 计算数组与标量的逐元素绝对差，并返回新矩阵。
        /// </summary>
        public static Mat AbsDiff(Mat src, Scalar value)
        {
            var dst = new Mat();
            try
            {
                AbsDiff(src, value, dst);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Calculates the per-element bitwise conjunction of two arrays.
        /// 计算两个数组逐元素按位与。
        /// </summary>
        public static void BitwiseAnd(Mat src1, Mat src2, Mat dst, Mat? mask = null)
        {
            ValidateMatTriple(src1, src2, dst);
            ValidateBinaryArrayInputs(src1, src2, "BitwiseAnd");
            ValidateOperationMask(src1, mask, "BitwiseAnd");
            NativeException.ThrowIfError(NativeMethods.CoreBitwiseAnd(src1.NativeHandle, src2.NativeHandle, dst.NativeHandle, OptionalHandle(mask)));
        }

        /// <summary>
        /// Calculates the per-element bitwise conjunction of two arrays and returns a new matrix.
        /// 计算两个数组逐元素按位与，并返回新矩阵。
        /// </summary>
        public static Mat BitwiseAnd(Mat src1, Mat src2)
        {
            var dst = new Mat();
            try
            {
                BitwiseAnd(src1, src2, dst);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Calculates the per-element bitwise disjunction of two arrays.
        /// 计算两个数组逐元素按位或。
        /// </summary>
        public static void BitwiseOr(Mat src1, Mat src2, Mat dst, Mat? mask = null)
        {
            ValidateMatTriple(src1, src2, dst);
            ValidateBinaryArrayInputs(src1, src2, "BitwiseOr");
            ValidateOperationMask(src1, mask, "BitwiseOr");
            NativeException.ThrowIfError(NativeMethods.CoreBitwiseOr(src1.NativeHandle, src2.NativeHandle, dst.NativeHandle, OptionalHandle(mask)));
        }

        /// <summary>
        /// Calculates the per-element bitwise disjunction of two arrays and returns a new matrix.
        /// 计算两个数组逐元素按位或，并返回新矩阵。
        /// </summary>
        public static Mat BitwiseOr(Mat src1, Mat src2)
        {
            var dst = new Mat();
            try
            {
                BitwiseOr(src1, src2, dst);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Calculates the per-element bitwise exclusive-or of two arrays.
        /// 计算两个数组逐元素按位异或。
        /// </summary>
        public static void BitwiseXor(Mat src1, Mat src2, Mat dst, Mat? mask = null)
        {
            ValidateMatTriple(src1, src2, dst);
            ValidateBinaryArrayInputs(src1, src2, "BitwiseXor");
            ValidateOperationMask(src1, mask, "BitwiseXor");
            NativeException.ThrowIfError(NativeMethods.CoreBitwiseXor(src1.NativeHandle, src2.NativeHandle, dst.NativeHandle, OptionalHandle(mask)));
        }

        /// <summary>
        /// Calculates the per-element bitwise exclusive-or of two arrays and returns a new matrix.
        /// 计算两个数组逐元素按位异或，并返回新矩阵。
        /// </summary>
        public static Mat BitwiseXor(Mat src1, Mat src2)
        {
            var dst = new Mat();
            try
            {
                BitwiseXor(src1, src2, dst);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Inverts every bit of each array element.
        /// 对数组每个元素逐位取反。
        /// </summary>
        public static void BitwiseNot(Mat src, Mat dst, Mat? mask = null)
        {
            ValidateMatPair(src, dst);
            ValidateOperationMask(src, mask, "BitwiseNot");
            NativeException.ThrowIfError(NativeMethods.CoreBitwiseNot(src.NativeHandle, dst.NativeHandle, OptionalHandle(mask)));
        }

        private static void ValidateOperationMask(Mat src, Mat? mask, string operationName)
        {
            if (mask == null || mask.Empty)
            {
                return;
            }

            int maskType = mask.Type;
            if (maskType != MatType.CV_8UC1 &&
                maskType != MatType.CV_8SC1 &&
                maskType != MatType.CV_BoolC1)
            {
                throw new ArgumentException($"{operationName} mask must be empty, CV_8UC1, CV_8SC1, or CV_BoolC1.", nameof(mask));
            }

            if (mask.Size != src.Size)
            {
                throw new ArgumentException($"{operationName} mask must have the same size as the source matrix.", nameof(mask));
            }
        }

        private static void ValidateMatchingEmptyInputs(Mat src1, Mat src2, string operationName)
        {
            if (src1.Empty != src2.Empty)
            {
                string parameterName = src1.Empty ? nameof(src1) : nameof(src2);
                throw new ArgumentException($"{operationName} requires both source matrices to be empty or both source matrices to be non-empty.", parameterName);
            }
        }

        private static void ValidateBinaryArrayInputs(Mat src1, Mat src2, string operationName)
        {
            if (src1.Size != src2.Size)
            {
                throw new ArgumentException($"{operationName} source matrices must have the same size.", nameof(src2));
            }

            if (src1.Type != src2.Type)
            {
                throw new ArgumentException($"{operationName} source matrices must have the same type.", nameof(src2));
            }
        }

        private static void ValidateArithmeticArrayInputs(Mat src1, Mat src2, int dtype, string operationName)
        {
            if (src1.Size != src2.Size)
            {
                throw new ArgumentException($"{operationName} source matrices must have the same size.", nameof(src2));
            }

            if (src1.Channels != src2.Channels)
            {
                throw new ArgumentException($"{operationName} source matrices must have the same number of channels.", nameof(src2));
            }

            if (src1.Depth != src2.Depth && dtype < 0)
            {
                throw new ArgumentException($"{operationName} source matrices with different depths require an explicit output depth.", nameof(dtype));
            }
        }

        private static void ValidateArithmeticArrayOrScalarInputs(Mat src1, Mat src2, int dtype, string operationName)
        {
            if (src1.Empty || src2.Empty)
            {
                return;
            }

            if (src1.Size == src2.Size && src1.Channels == src2.Channels)
            {
                if (src1.Depth != src2.Depth && dtype < 0)
                {
                    throw new ArgumentException($"{operationName} source matrices with different depths require an explicit output depth.", nameof(dtype));
                }

                return;
            }

            bool src1Scalar = IsOpenCvArithmeticScalarLike(src1, src2);
            bool src2Scalar = IsOpenCvArithmeticScalarLike(src2, src1);
            if (!src1Scalar && !src2Scalar)
            {
                throw new ArgumentException($"{operationName} source matrices must have the same size and channel count unless one source is an OpenCV-compatible scalar matrix.", nameof(src2));
            }
        }

        private static bool IsOpenCvArithmeticScalarLike(Mat candidate, Mat array)
        {
            if (candidate.Dims > 2 || !candidate.IsContinuous)
            {
                return false;
            }

            Size size = candidate.Size;
            if (size.Width != 1 && size.Height != 1)
            {
                return false;
            }

            int channels = array.Channels;
            bool scalarShape = (size.Width == 1 && size.Height == 1) ||
                               (size.Width == channels && size.Height == 1) ||
                               (size.Width == 1 && size.Height == 4 && channels <= 4);

            return scalarShape && (candidate.Type == MatType.CV_32FC1 || candidate.Type == MatType.CV_64FC1);
        }

        /// <summary>
        /// Inverts every bit of each array element and returns a new matrix.
        /// 对数组每个元素逐位取反，并返回新矩阵。
        /// </summary>
        public static Mat BitwiseNot(Mat src)
        {
            var dst = new Mat();
            try
            {
                BitwiseNot(src, dst);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Performs a per-element comparison of two arrays.
        /// 对两个数组执行逐元素比较。
        /// </summary>
        public static void Compare(Mat src1, Mat src2, Mat dst, CmpTypes cmpop)
        {
            ValidateMatTriple(src1, src2, dst);
            ValidateCmpType(cmpop, nameof(cmpop));
            ValidateCompareInputs(src1, src2);
            NativeException.ThrowIfError(NativeMethods.CoreCompare(src1.NativeHandle, src2.NativeHandle, dst.NativeHandle, (int)cmpop));
        }

        private static void ValidateCompareInputs(Mat src1, Mat src2)
        {
            if (src1.Empty != src2.Empty)
            {
                string parameterName = src1.Empty ? nameof(src1) : nameof(src2);
                throw new ArgumentException("Compare requires both source matrices to be empty or both source matrices to be non-empty.", parameterName);
            }

            if (src1.Empty)
            {
                return;
            }

            if (src1.Size == src2.Size && src1.Type == src2.Type)
            {
                return;
            }

            bool src1Scalar = IsOpenCvScalarLike(src1, src2);
            bool src2Scalar = IsOpenCvScalarLike(src2, src1);
            if (src1Scalar == src2Scalar)
            {
                throw new ArgumentException("Compare sources must have the same size and type, or exactly one source must be an OpenCV-compatible scalar matrix.", nameof(src2));
            }
        }

        private static bool IsOpenCvScalarLike(Mat candidate, Mat array)
        {
            if (candidate.Dims > 2 || !candidate.IsContinuous)
            {
                return false;
            }

            Size size = candidate.Size;
            if (size.Width != 1 && size.Height != 1)
            {
                return false;
            }

            int channels = array.Channels;
            return (size.Width == 1 && size.Height == 1) ||
                   (size.Width == 1 && size.Height == channels) ||
                   (size.Width == channels && size.Height == 1) ||
                   (size.Width == 1 && size.Height == 4 && candidate.Type == MatType.CV_64FC1 && channels <= 4);
        }

        private static void ValidateCmpType(CmpTypes value, string parameterName)
        {
            if (value != CmpTypes.EQ &&
                value != CmpTypes.GT &&
                value != CmpTypes.GE &&
                value != CmpTypes.LT &&
                value != CmpTypes.LE &&
                value != CmpTypes.NE)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Unsupported comparison operation.");
            }
        }

        /// <summary>
        /// Performs a per-element comparison of two arrays and returns a new mask matrix.
        /// 对两个数组执行逐元素比较，并返回新的掩码矩阵。
        /// </summary>
        public static Mat Compare(Mat src1, Mat src2, CmpTypes cmpop)
        {
            var dst = new Mat();
            try
            {
                Compare(src1, src2, dst, cmpop);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Calculates the per-element minimum of two arrays.
        /// 计算两个数组逐元素最小值。
        /// </summary>
        public static void Min(Mat src1, Mat src2, Mat dst)
        {
            ValidateMatTriple(src1, src2, dst);
            ValidateBinaryArrayInputs(src1, src2, "Min");
            NativeException.ThrowIfError(NativeMethods.CoreMin(src1.NativeHandle, src2.NativeHandle, dst.NativeHandle));
        }

        /// <summary>
        /// Calculates the per-element minimum of two arrays and returns a new matrix.
        /// 计算两个数组逐元素最小值，并返回新矩阵。
        /// </summary>
        public static Mat Min(Mat src1, Mat src2)
        {
            var dst = new Mat();
            try
            {
                Min(src1, src2, dst);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Calculates the per-element maximum of two arrays.
        /// 计算两个数组逐元素最大值。
        /// </summary>
        public static void Max(Mat src1, Mat src2, Mat dst)
        {
            ValidateMatTriple(src1, src2, dst);
            ValidateBinaryArrayInputs(src1, src2, "Max");
            NativeException.ThrowIfError(NativeMethods.CoreMax(src1.NativeHandle, src2.NativeHandle, dst.NativeHandle));
        }

        /// <summary>
        /// Calculates the per-element maximum of two arrays and returns a new matrix.
        /// 计算两个数组逐元素最大值，并返回新矩阵。
        /// </summary>
        public static Mat Max(Mat src1, Mat src2)
        {
            var dst = new Mat();
            try
            {
                Max(src1, src2, dst);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Checks whether array elements lie between two scalar bounds.
        /// 检查数组元素是否位于两个标量边界之间。
        /// </summary>
        public static void InRange(Mat src, Scalar lowerb, Scalar upperb, Mat dst)
        {
            ValidateMatPair(src, dst);
            ValidateInRangeInput(src, nameof(src));
            NativeException.ThrowIfError(NativeMethods.CoreInRange(
                src.NativeHandle,
                lowerb.V0,
                lowerb.V1,
                lowerb.V2,
                lowerb.V3,
                upperb.V0,
                upperb.V1,
                upperb.V2,
                upperb.V3,
                dst.NativeHandle));
        }

        private static void ValidateInRangeInput(Mat src, string parameterName)
        {
            if (src.Empty)
            {
                throw new ArgumentException("InRange requires a non-empty source matrix.", parameterName);
            }
        }

        /// <summary>
        /// Checks whether array elements lie between two scalar bounds and returns a new mask matrix.
        /// 检查数组元素是否位于两个标量边界之间，并返回新的掩码矩阵。
        /// </summary>
        public static Mat InRange(Mat src, Scalar lowerb, Scalar upperb)
        {
            var dst = new Mat();
            try
            {
                InRange(src, lowerb, upperb, dst);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Replaces NaN values in a floating-point array.
        /// 替换浮点数组中的 NaN 值。
        /// </summary>
        public static void PatchNaNs(Mat src, double value = 0.0)
        {
            ValidateNotNull(src, nameof(src));
            ValidatePatchNaNsInput(src, nameof(src));
            NativeException.ThrowIfError(NativeMethods.CorePatchNaNs(src.NativeHandle, value));
        }

        private static void ValidatePatchNaNsInput(Mat src, string parameterName)
        {
            int depth = src.Depth;
            if (depth != MatType.CV_32F && depth != MatType.CV_64F)
            {
                throw new ArgumentException("PatchNaNs source matrix must have CV_32F or CV_64F depth.", parameterName);
            }
        }
    }
}

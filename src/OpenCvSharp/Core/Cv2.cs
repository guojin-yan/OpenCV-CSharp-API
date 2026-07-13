using System;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Core
{
    /// <summary>
    /// Provides OpenCV core array functions aligned with OpenCV <c>cv</c> free functions.
    /// 提供与 OpenCV <c>cv</c> 自由函数对齐的核心数组函数。
    /// </summary>
    public static partial class Cv2
    {
        private static IntPtr OptionalHandle(Mat? mat)
        {
            return mat == null ? IntPtr.Zero : mat.NativeHandle;
        }

        private static void ValidateNotNull<T>(T value, string parameterName)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        private static void ValidateMatPair(Mat src, Mat dst)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
        }

        private static void ValidateMatTriple(Mat src1, Mat src2, Mat dst)
        {
            ValidateNotNull(src1, nameof(src1));
            ValidateNotNull(src2, nameof(src2));
            ValidateNotNull(dst, nameof(dst));
        }

        private static void ValidatePositive(int value, string parameterName)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Value must be positive.");
            }
        }

        private static void ValidateNonEmpty<T>(T[] values, string parameterName)
        {
            if (values == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            if (values.Length == 0)
            {
                throw new ArgumentException("Array cannot be empty.", parameterName);
            }
        }

        private static Scalar ToScalar(double[] values)
        {
            return new Scalar(values[0], values[1], values[2], values[3]);
        }

#if NETCOREAPP3_1_OR_GREATER
        private static Scalar ToScalar(ReadOnlySpan<double> values)
        {
            return new Scalar(values[0], values[1], values[2], values[3]);
        }
#endif
    }
}

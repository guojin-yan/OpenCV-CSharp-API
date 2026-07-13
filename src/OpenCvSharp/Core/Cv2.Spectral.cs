using System;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Core
{
    public static partial class Cv2
    {
        /// <summary>
        /// Performs a discrete Fourier transform.
        /// 执行离散傅里叶变换。
        /// </summary>
        public static void Dft(Mat src, Mat dst, DftFlags flags = DftFlags.None, int nonzeroRows = 0)
        {
            ValidateMatPair(src, dst);
            ValidateDftFlags(flags, nameof(flags));
            ValidateDftInput(src, flags, nameof(src));
            NativeException.ThrowIfError(NativeMethods.CoreDft(src.NativeHandle, dst.NativeHandle, (int)flags, nonzeroRows));
        }

        /// <summary>
        /// Performs a discrete Fourier transform and returns a new matrix.
        /// 执行离散傅里叶变换并返回新矩阵。
        /// </summary>
        public static Mat Dft(Mat src, DftFlags flags = DftFlags.None, int nonzeroRows = 0)
        {
            var dst = new Mat();
            try
            {
                Dft(src, dst, flags, nonzeroRows);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Performs an inverse discrete Fourier transform.
        /// 执行逆离散傅里叶变换。
        /// </summary>
        public static void Idft(Mat src, Mat dst, DftFlags flags = DftFlags.None, int nonzeroRows = 0)
        {
            ValidateMatPair(src, dst);
            ValidateDftFlags(flags, nameof(flags));
            ValidateDftInput(src, flags, nameof(src));
            NativeException.ThrowIfError(NativeMethods.CoreIdft(src.NativeHandle, dst.NativeHandle, (int)flags, nonzeroRows));
        }

        private static void ValidateDftFlags(DftFlags value, string parameterName)
        {
            const DftFlags allowed = DftFlags.Inverse | DftFlags.Scale | DftFlags.Rows | DftFlags.ComplexOutput | DftFlags.RealOutput | DftFlags.ComplexInput;
            if ((value & ~allowed) != 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Unsupported discrete Fourier transform flags.");
            }

            if ((value & DftFlags.ComplexOutput) != 0 && (value & DftFlags.RealOutput) != 0)
            {
                throw new ArgumentException("Discrete Fourier transforms cannot request both ComplexOutput and RealOutput.", parameterName);
            }
        }

        private static void ValidateDftInput(Mat src, DftFlags flags, string parameterName)
        {
            if (!IsSupportedDftType(src.Type))
            {
                throw new ArgumentException("Discrete Fourier transforms require CV_32FC1, CV_32FC2, CV_64FC1, or CV_64FC2 input.", parameterName);
            }

            if ((flags & DftFlags.ComplexInput) != 0 && src.Channels != 2)
            {
                throw new ArgumentException("Discrete Fourier transforms with ComplexInput require a two-channel source matrix.", parameterName);
            }
        }

        private static bool IsSupportedDftType(int type)
        {
            return type == MatType.CV_32FC1 ||
                type == MatType.CV_32FC2 ||
                type == MatType.CV_64FC1 ||
                type == MatType.CV_64FC2;
        }

        /// <summary>
        /// Performs an inverse discrete Fourier transform and returns a new matrix.
        /// 执行逆离散傅里叶变换并返回新矩阵。
        /// </summary>
        public static Mat Idft(Mat src, DftFlags flags = DftFlags.None, int nonzeroRows = 0)
        {
            var dst = new Mat();
            try
            {
                Idft(src, dst, flags, nonzeroRows);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Performs a discrete cosine transform.
        /// 执行离散余弦变换。
        /// </summary>
        public static void Dct(Mat src, Mat dst, DctFlags flags = DctFlags.None)
        {
            ValidateMatPair(src, dst);
            ValidateDctFlags(flags, nameof(flags));
            ValidateDctInput(src, nameof(src));
            NativeException.ThrowIfError(NativeMethods.CoreDct(src.NativeHandle, dst.NativeHandle, (int)flags));
        }

        /// <summary>
        /// Performs a discrete cosine transform and returns a new matrix.
        /// 执行离散余弦变换并返回新矩阵。
        /// </summary>
        public static Mat Dct(Mat src, DctFlags flags = DctFlags.None)
        {
            var dst = new Mat();
            try
            {
                Dct(src, dst, flags);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Performs an inverse discrete cosine transform.
        /// 执行逆离散余弦变换。
        /// </summary>
        public static void Idct(Mat src, Mat dst, DctFlags flags = DctFlags.None)
        {
            ValidateMatPair(src, dst);
            ValidateDctFlags(flags, nameof(flags));
            ValidateDctInput(src, nameof(src));
            NativeException.ThrowIfError(NativeMethods.CoreIdct(src.NativeHandle, dst.NativeHandle, (int)flags));
        }

        private static void ValidateDctFlags(DctFlags value, string parameterName)
        {
            const DctFlags allowed = DctFlags.Inverse | DctFlags.Rows;
            if ((value & ~allowed) != 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Unsupported discrete cosine transform flags.");
            }
        }

        private static void ValidateDctInput(Mat src, string parameterName)
        {
            int type = src.Type;
            if (type != MatType.CV_32FC1 && type != MatType.CV_64FC1)
            {
                throw new ArgumentException("Discrete cosine transforms require CV_32FC1 or CV_64FC1 input.", parameterName);
            }
        }

        /// <summary>
        /// Performs an inverse discrete cosine transform and returns a new matrix.
        /// 执行逆离散余弦变换并返回新矩阵。
        /// </summary>
        public static Mat Idct(Mat src, DctFlags flags = DctFlags.None)
        {
            var dst = new Mat();
            try
            {
                Idct(src, dst, flags);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Performs per-element multiplication of two Fourier spectrums.
        /// 对两个傅里叶频谱执行逐元素乘法。
        /// </summary>
        public static void MulSpectrums(Mat a, Mat b, Mat c, MulSpectrumsFlags flags = MulSpectrumsFlags.None, bool conjB = false)
        {
            ValidateNotNull(a, nameof(a));
            ValidateNotNull(b, nameof(b));
            ValidateNotNull(c, nameof(c));
            ValidateMulSpectrumsFlags(flags, nameof(flags));
            ValidateSpectrumOperationInputs(a, b);
            NativeException.ThrowIfError(NativeMethods.CoreMulSpectrums(a.NativeHandle, b.NativeHandle, c.NativeHandle, (int)flags, conjB ? 1 : 0));
        }

        /// <summary>
        /// Performs per-element multiplication of two Fourier spectrums and returns a new matrix.
        /// 对两个傅里叶频谱执行逐元素乘法并返回新矩阵。
        /// </summary>
        public static Mat MulSpectrums(Mat a, Mat b, MulSpectrumsFlags flags = MulSpectrumsFlags.None, bool conjB = false)
        {
            var c = new Mat();
            try
            {
                MulSpectrums(a, b, c, flags, conjB);
                return c;
            }
            catch
            {
                c.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Performs per-element division of one Fourier spectrum by another.
        /// 对一个傅里叶频谱逐元素除以另一个频谱。
        /// </summary>
        public static void DivSpectrums(Mat a, Mat b, Mat c, MulSpectrumsFlags flags = MulSpectrumsFlags.None, bool conjB = false)
        {
            ValidateNotNull(a, nameof(a));
            ValidateNotNull(b, nameof(b));
            ValidateNotNull(c, nameof(c));
            ValidateMulSpectrumsFlags(flags, nameof(flags));
            ValidateSpectrumOperationInputs(a, b);
            ValidateDivSpectrumsOutput(a, b, c);
            NativeException.ThrowIfError(NativeMethods.CoreDivSpectrums(a.NativeHandle, b.NativeHandle, c.NativeHandle, (int)flags, conjB ? 1 : 0));
        }

        private static void ValidateMulSpectrumsFlags(MulSpectrumsFlags value, string parameterName)
        {
            const MulSpectrumsFlags allowed = MulSpectrumsFlags.Rows;
            if ((value & ~allowed) != 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Unsupported spectrum operation flags.");
            }
        }

        private static void ValidateSpectrumOperationInputs(Mat a, Mat b)
        {
            if (a.Type != MatType.CV_32FC1 &&
                a.Type != MatType.CV_32FC2 &&
                a.Type != MatType.CV_64FC1 &&
                a.Type != MatType.CV_64FC2)
            {
                throw new ArgumentException("Spectrum operations require CV_32FC1, CV_32FC2, CV_64FC1, or CV_64FC2 input.", nameof(a));
            }

            if (b.Type != a.Type)
            {
                throw new ArgumentException("Spectrum operation inputs must have the same type.", nameof(b));
            }

            if (b.Rows != a.Rows || b.Cols != a.Cols)
            {
                throw new ArgumentException("Spectrum operation inputs must have the same size.", nameof(b));
            }
        }

        private static void ValidateDivSpectrumsOutput(Mat a, Mat b, Mat c)
        {
            IntPtr destinationData = c.Data;
            if (destinationData == IntPtr.Zero)
            {
                return;
            }

            if (destinationData == a.Data || destinationData == b.Data)
            {
                throw new ArgumentException("Spectrum division requires a destination that does not alias either input.", nameof(c));
            }
        }

        /// <summary>
        /// Performs spectrum division and returns a new matrix.
        /// 执行频谱除法并返回新矩阵。
        /// </summary>
        public static Mat DivSpectrums(Mat a, Mat b, MulSpectrumsFlags flags = MulSpectrumsFlags.None, bool conjB = false)
        {
            var c = new Mat();
            try
            {
                DivSpectrums(a, b, c, flags, conjB);
                return c;
            }
            catch
            {
                c.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Returns the optimal DFT size for a vector length.
        /// 返回指定向量长度的最优 DFT 尺寸。
        /// </summary>
        public static int GetOptimalDftSize(int vecSize)
        {
            NativeException.ThrowIfError(NativeMethods.CoreGetOptimalDftSize(vecSize, out int size));
            return size;
        }
    }
}

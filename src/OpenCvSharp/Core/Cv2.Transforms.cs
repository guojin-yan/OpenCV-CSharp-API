using System;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Core
{
    public static partial class Cv2
    {
        /// <summary>
        /// Applies a matrix transform to every array element.
        /// 对数组的每个元素应用矩阵变换。
        /// </summary>
        public static void Transform(Mat src, Mat dst, Mat m)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            ValidateNotNull(m, nameof(m));
            ValidateTransformMatrix(src, m);
            NativeException.ThrowIfError(NativeMethods.CoreTransform(src.NativeHandle, dst.NativeHandle, m.NativeHandle));
        }

        /// <summary>
        /// Applies a matrix transform and returns a new matrix.
        /// 应用矩阵变换并返回新矩阵。
        /// </summary>
        public static Mat Transform(Mat src, Mat m)
        {
            var dst = new Mat();
            try
            {
                Transform(src, dst, m);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Applies a perspective transform to every vector.
        /// 对每个向量应用透视变换。
        /// </summary>
        public static void PerspectiveTransform(Mat src, Mat dst, Mat m)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            ValidateNotNull(m, nameof(m));
            ValidatePerspectiveTransformInputs(src, m);
            NativeException.ThrowIfError(NativeMethods.CorePerspectiveTransform(src.NativeHandle, dst.NativeHandle, m.NativeHandle));
        }

        /// <summary>
        /// Applies a perspective transform and returns a new matrix.
        /// 应用透视变换并返回新矩阵。
        /// </summary>
        public static Mat PerspectiveTransform(Mat src, Mat m)
        {
            var dst = new Mat();
            try
            {
                PerspectiveTransform(src, dst, m);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        private static void ValidateTransformMatrix(Mat src, Mat m)
        {
            int sourceChannels = src.Channels;
            if (m.Cols != sourceChannels && m.Cols != sourceChannels + 1)
            {
                throw new ArgumentException("Transform matrix columns must match the source channel count or source channel count plus one.", nameof(m));
            }
        }

        private static void ValidatePerspectiveTransformInputs(Mat src, Mat m)
        {
            int sourceChannels = src.Channels;
            if (sourceChannels != 2 && sourceChannels != 3)
            {
                throw new ArgumentException("Perspective transform source must have two or three channels.", nameof(src));
            }

            if (m.Rows != sourceChannels + 1 || m.Cols != sourceChannels + 1)
            {
                throw new ArgumentException("Perspective transform matrix must be 3x3 for two-channel sources or 4x4 for three-channel sources.", nameof(m));
            }

            int depth = src.Depth;
            if (depth != MatType.CV_32F && depth != MatType.CV_64F)
            {
                throw new ArgumentException("Perspective transform source depth must be CV_32F or CV_64F.", nameof(src));
            }
        }

        /// <summary>
        /// Calculates vector magnitudes from x and y components.
        /// 根据 x 和 y 分量计算向量幅值。
        /// </summary>
        public static void Magnitude(Mat x, Mat y, Mat magnitude)
        {
            ValidateNotNull(x, nameof(x));
            ValidateNotNull(y, nameof(y));
            ValidateNotNull(magnitude, nameof(magnitude));
            ValidateCartesianCoordinateInputs(x, y);
            NativeException.ThrowIfError(NativeMethods.CoreMagnitude(x.NativeHandle, y.NativeHandle, magnitude.NativeHandle));
        }

        /// <summary>
        /// Calculates vector magnitudes and returns a new matrix.
        /// 计算向量幅值并返回新矩阵。
        /// </summary>
        public static Mat Magnitude(Mat x, Mat y)
        {
            var magnitude = new Mat();
            try
            {
                Magnitude(x, y, magnitude);
                return magnitude;
            }
            catch
            {
                magnitude.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Calculates vector angles from x and y components.
        /// 根据 x 和 y 分量计算向量角度。
        /// </summary>
        public static void Phase(Mat x, Mat y, Mat angle, bool angleInDegrees = false)
        {
            ValidateNotNull(x, nameof(x));
            ValidateNotNull(y, nameof(y));
            ValidateNotNull(angle, nameof(angle));
            ValidateCartesianCoordinateInputs(x, y);
            NativeException.ThrowIfError(NativeMethods.CorePhase(x.NativeHandle, y.NativeHandle, angle.NativeHandle, angleInDegrees ? 1 : 0));
        }

        /// <summary>
        /// Calculates vector angles and returns a new matrix.
        /// 计算向量角度并返回新矩阵。
        /// </summary>
        public static Mat Phase(Mat x, Mat y, bool angleInDegrees = false)
        {
            var angle = new Mat();
            try
            {
                Phase(x, y, angle, angleInDegrees);
                return angle;
            }
            catch
            {
                angle.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Converts Cartesian coordinates to polar coordinates.
        /// 将笛卡尔坐标转换为极坐标。
        /// </summary>
        public static void CartToPolar(Mat x, Mat y, Mat magnitude, Mat angle, bool angleInDegrees = false)
        {
            ValidateNotNull(x, nameof(x));
            ValidateNotNull(y, nameof(y));
            ValidateNotNull(magnitude, nameof(magnitude));
            ValidateNotNull(angle, nameof(angle));
            ValidateDistinctPolarOutputs(magnitude, angle);
            ValidateCartesianCoordinateInputs(x, y);
            NativeException.ThrowIfError(NativeMethods.CoreCartToPolar(x.NativeHandle, y.NativeHandle, magnitude.NativeHandle, angle.NativeHandle, angleInDegrees ? 1 : 0));
        }

        private static void ValidateCartesianCoordinateInputs(Mat x, Mat y)
        {
            if (!y.Size.Equals(x.Size))
            {
                throw new ArgumentException("Cartesian coordinate inputs must have the same size.", nameof(y));
            }

            if (y.Type != x.Type)
            {
                throw new ArgumentException("Cartesian coordinate inputs must have the same type.", nameof(y));
            }

            int depth = x.Depth;
            if (depth != MatType.CV_32F && depth != MatType.CV_64F)
            {
                throw new ArgumentException("Cartesian coordinate inputs must have CV_32F or CV_64F depth.", nameof(x));
            }
        }

        private static void ValidateDistinctPolarOutputs(Mat magnitude, Mat angle)
        {
            if (magnitude.NativeHandle == angle.NativeHandle)
            {
                throw new ArgumentException("Cartesian-to-polar conversion requires distinct magnitude and angle output matrices.", nameof(angle));
            }
        }

        /// <summary>
        /// Converts polar coordinates to Cartesian coordinates.
        /// 将极坐标转换为笛卡尔坐标。
        /// </summary>
        public static void PolarToCart(Mat magnitude, Mat angle, Mat x, Mat y, bool angleInDegrees = false)
        {
            ValidateNotNull(magnitude, nameof(magnitude));
            ValidateNotNull(angle, nameof(angle));
            ValidateNotNull(x, nameof(x));
            ValidateNotNull(y, nameof(y));
            ValidateDistinctCartesianOutputs(x, y);
            ValidatePolarToCartInputs(magnitude, angle);
            NativeException.ThrowIfError(NativeMethods.CorePolarToCart(magnitude.NativeHandle, angle.NativeHandle, x.NativeHandle, y.NativeHandle, angleInDegrees ? 1 : 0));
        }

        private static void ValidateDistinctCartesianOutputs(Mat x, Mat y)
        {
            if (x.NativeHandle == y.NativeHandle)
            {
                throw new ArgumentException("Polar-to-Cartesian conversion requires distinct x and y output matrices.", nameof(y));
            }
        }

        private static void ValidatePolarToCartInputs(Mat magnitude, Mat angle)
        {
            int angleDepth = angle.Depth;
            if (angleDepth != MatType.CV_32F && angleDepth != MatType.CV_64F)
            {
                throw new ArgumentException("Polar angle input must have CV_32F or CV_64F depth.", nameof(angle));
            }

            if (magnitude.Empty)
            {
                return;
            }

            if (magnitude.Type != angle.Type)
            {
                throw new ArgumentException("Polar magnitude input must be empty or have the same type as angle.", nameof(magnitude));
            }

            if (!magnitude.Size.Equals(angle.Size))
            {
                throw new ArgumentException("Polar magnitude input must be empty or have the same size as angle.", nameof(magnitude));
            }
        }

        /// <summary>
        /// Calculates the natural exponent of every array element.
        /// 计算数组每个元素的自然指数。
        /// </summary>
        public static void Exp(Mat src, Mat dst)
        {
            ValidateMatPair(src, dst);
            ValidateFloatingPointMathInput(src);
            NativeException.ThrowIfError(NativeMethods.CoreExp(src.NativeHandle, dst.NativeHandle));
        }

        /// <summary>
        /// Calculates the natural exponent and returns a new matrix.
        /// 计算自然指数并返回新矩阵。
        /// </summary>
        public static Mat Exp(Mat src)
        {
            var dst = new Mat();
            try
            {
                Exp(src, dst);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Calculates the natural logarithm of every array element.
        /// 计算数组每个元素的自然对数。
        /// </summary>
        public static void Log(Mat src, Mat dst)
        {
            ValidateMatPair(src, dst);
            ValidateFloatingPointMathInput(src);
            NativeException.ThrowIfError(NativeMethods.CoreLog(src.NativeHandle, dst.NativeHandle));
        }

        private static void ValidateFloatingPointMathInput(Mat src)
        {
            if (src.Depth != MatType.CV_32F && src.Depth != MatType.CV_64F)
            {
                throw new ArgumentException("Source matrix must have CV_32F or CV_64F depth.", nameof(src));
            }
        }

        /// <summary>
        /// Calculates the natural logarithm and returns a new matrix.
        /// 计算自然对数并返回新矩阵。
        /// </summary>
        public static Mat Log(Mat src)
        {
            var dst = new Mat();
            try
            {
                Log(src, dst);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Calculates the square root of every array element.
        /// 计算数组每个元素的平方根。
        /// </summary>
        public static void Sqrt(Mat src, Mat dst)
        {
            ValidateMatPair(src, dst);
            NativeException.ThrowIfError(NativeMethods.CoreSqrt(src.NativeHandle, dst.NativeHandle));
        }

        /// <summary>
        /// Calculates square roots and returns a new matrix.
        /// 计算平方根并返回新矩阵。
        /// </summary>
        public static Mat Sqrt(Mat src)
        {
            var dst = new Mat();
            try
            {
                Sqrt(src, dst);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Raises every array element to a power.
        /// 计算数组每个元素的指定幂。
        /// </summary>
        public static void Pow(Mat src, double power, Mat dst)
        {
            ValidateMatPair(src, dst);
            NativeException.ThrowIfError(NativeMethods.CorePow(src.NativeHandle, power, dst.NativeHandle));
        }

        /// <summary>
        /// Raises every array element to a power and returns a new matrix.
        /// 计算指定幂并返回新矩阵。
        /// </summary>
        public static Mat Pow(Mat src, double power)
        {
            var dst = new Mat();
            try
            {
                Pow(src, power, dst);
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

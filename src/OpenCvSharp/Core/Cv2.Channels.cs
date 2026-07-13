using System;
using OpenCvSharp.Internal.Interop;

#if NETCOREAPP3_1_OR_GREATER
using System.Runtime.InteropServices;
#endif

namespace OpenCvSharp.Core
{
    public static partial class Cv2
    {
        /// <summary>
        /// Splits a multi-channel matrix into individual single-channel matrices.
        /// 将多通道矩阵拆分为单通道矩阵。
        /// </summary>
        public static Mat[] Split(Mat src)
        {
            ValidateNotNull(src, nameof(src));
            NativeException.ThrowIfError(NativeMethods.CoreSplitCount(src.NativeHandle, out int count));
            if (count <= 0)
            {
                return Array.Empty<Mat>();
            }

#if NETCOREAPP3_1_OR_GREATER
            IntPtr[] handles = new IntPtr[count];
            unsafe
            {
                fixed (IntPtr* handlesPtr = handles)
                {
                    NativeException.ThrowIfError(NativeMethods.CoreSplitFillPtr(src.NativeHandle, handlesPtr, handles.Length, out int written));
                    return CreateMatArray(handles, written);
                }
            }
#else
            IntPtr[] handles = new IntPtr[count];
            NativeException.ThrowIfError(NativeMethods.CoreSplitFill(src.NativeHandle, handles, handles.Length, out int written));
            return CreateMatArray(handles, written);
#endif
        }

        /// <summary>
        /// Merges several single-channel matrices into one multi-channel matrix.
        /// 将多个单通道矩阵合并为一个多通道矩阵。
        /// </summary>
        public static void Merge(Mat[] src, Mat dst)
        {
            ValidateNonEmpty(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            ValidateMergeInputs(src, nameof(src));

#if NETCOREAPP3_1_OR_GREATER
            IntPtr[] handles = new IntPtr[src.Length];
            for (int i = 0; i < src.Length; i++)
            {
                handles[i] = src[i].NativeHandle;
            }

            unsafe
            {
                fixed (IntPtr* handlesPtr = handles)
                {
                    NativeException.ThrowIfError(NativeMethods.CoreMergePtr(handlesPtr, handles.Length, dst.NativeHandle));
                }
            }
#else
            IntPtr[] handles = new IntPtr[src.Length];
            for (int i = 0; i < src.Length; i++)
            {
                handles[i] = src[i].NativeHandle;
            }

            NativeException.ThrowIfError(NativeMethods.CoreMerge(handles, handles.Length, dst.NativeHandle));
#endif
        }

        private static void ValidateMergeInputs(Mat[] src, string parameterName)
        {
            Mat first = src[0];
            if (first == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            if (first.Empty)
            {
                throw new ArgumentException("Merge source matrices must be non-empty.", parameterName);
            }

            Size size = first.Size;
            int depth = first.Depth;
            int channels = 0;

            for (int i = 0; i < src.Length; i++)
            {
                Mat mat = src[i];
                if (mat == null)
                {
                    throw new ArgumentNullException(parameterName);
                }

                if (mat.Empty)
                {
                    throw new ArgumentException("Merge source matrices must be non-empty.", parameterName);
                }

                if (mat.Size != size)
                {
                    throw new ArgumentException("Merge source matrices must have the same size.", parameterName);
                }

                if (mat.Depth != depth)
                {
                    throw new ArgumentException("Merge source matrices must have the same depth.", parameterName);
                }

                channels += mat.Channels;
                if (channels > MatType.ChannelMax)
                {
                    throw new ArgumentException("Merge output channel count cannot exceed OpenCV channel maximum.", parameterName);
                }
            }
        }

        /// <summary>
        /// Merges several single-channel matrices and returns a new multi-channel matrix.
        /// 合并多个单通道矩阵，并返回新的多通道矩阵。
        /// </summary>
        public static Mat Merge(Mat[] src)
        {
            var dst = new Mat();
            try
            {
                Merge(src, dst);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Extracts a single channel from a multi-channel matrix.
        /// 从多通道矩阵中提取单个通道。
        /// </summary>
        public static void ExtractChannel(Mat src, Mat dst, int coi)
        {
            ValidateMatPair(src, dst);
            ValidateExtractChannelIndex(src, coi, nameof(coi));
            NativeException.ThrowIfError(NativeMethods.CoreExtractChannel(src.NativeHandle, dst.NativeHandle, coi));
        }

        private static void ValidateExtractChannelIndex(Mat src, int coi, string parameterName)
        {
            if (coi < 0 || coi >= src.Channels)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Channel index must be within the source matrix channel range.");
            }
        }

        /// <summary>
        /// Extracts a single channel and returns a new matrix.
        /// 提取单个通道，并返回新矩阵。
        /// </summary>
        public static Mat ExtractChannel(Mat src, int coi)
        {
            var dst = new Mat();
            try
            {
                ExtractChannel(src, dst, coi);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Inserts a single-channel matrix into one channel of a multi-channel destination matrix.
        /// 将单通道矩阵插入到多通道目标矩阵的指定通道。
        /// </summary>
        public static void InsertChannel(Mat src, Mat dst, int coi)
        {
            ValidateMatPair(src, dst);
            ValidateInsertChannelInputs(src, dst, coi);
            NativeException.ThrowIfError(NativeMethods.CoreInsertChannel(src.NativeHandle, dst.NativeHandle, coi));
        }

        private static void ValidateInsertChannelInputs(Mat src, Mat dst, int coi)
        {
            if (src.Rows != dst.Rows || src.Cols != dst.Cols)
            {
                throw new ArgumentException("Source and destination matrices must have the same size.", nameof(dst));
            }

            if (src.Depth != dst.Depth)
            {
                throw new ArgumentException("Source and destination matrices must have the same depth.", nameof(dst));
            }

            if (src.Channels != 1)
            {
                throw new ArgumentException("Source matrix must have exactly one channel.", nameof(src));
            }

            if (coi < 0 || coi >= dst.Channels)
            {
                throw new ArgumentOutOfRangeException(nameof(coi), "Channel index must be within the destination matrix channel range.");
            }
        }

        /// <summary>
        /// Remaps channels from source matrices to destination matrices.
        /// 将源矩阵通道重新映射到目标矩阵。
        /// </summary>
        public static void MixChannels(Mat[] src, Mat[] dst, int[] fromTo)
        {
            ValidateNonEmpty(src, nameof(src));
            ValidateNonEmpty(dst, nameof(dst));
            ValidateNonEmpty(fromTo, nameof(fromTo));
            ValidateChannelMappingPairs(fromTo, nameof(fromTo));
            ValidateMixChannelsInputs(src, dst, fromTo);

            IntPtr[] srcHandles = new IntPtr[src.Length];
            IntPtr[] dstHandles = new IntPtr[dst.Length];
            for (int i = 0; i < src.Length; i++)
            {
                srcHandles[i] = src[i].NativeHandle;
            }

            for (int i = 0; i < dst.Length; i++)
            {
                dstHandles[i] = dst[i].NativeHandle;
            }

#if NETCOREAPP3_1_OR_GREATER
            unsafe
            {
                fixed (IntPtr* srcPtr = srcHandles)
                fixed (IntPtr* dstPtr = dstHandles)
                fixed (int* fromToPtr = fromTo)
                {
                    NativeException.ThrowIfError(NativeMethods.CoreMixChannelsPtr(srcPtr, srcHandles.Length, dstPtr, dstHandles.Length, fromToPtr, fromTo.Length / 2));
                }
            }
#else
            NativeException.ThrowIfError(NativeMethods.CoreMixChannels(srcHandles, srcHandles.Length, dstHandles, dstHandles.Length, fromTo, fromTo.Length / 2));
#endif
        }

        private static void ValidateChannelMappingPairs(int[] fromTo, string parameterName)
        {
            if ((fromTo.Length & 1) != 0)
            {
                throw new ArgumentException("Channel mapping array must contain source/destination pairs.", parameterName);
            }
        }

        private static void ValidateMixChannelsInputs(Mat[] src, Mat[] dst, int[] fromTo)
        {
            Mat first = src[0];
            if (first == null)
            {
                throw new ArgumentNullException(nameof(src));
            }

            if (first.Empty)
            {
                throw new ArgumentException("MixChannels source matrices must be non-empty.", nameof(src));
            }

            Size size = first.Size;
            int depth = first.Depth;
            int sourceChannels = 0;
            for (int i = 0; i < src.Length; i++)
            {
                Mat mat = src[i];
                if (mat == null)
                {
                    throw new ArgumentNullException(nameof(src));
                }

                if (mat.Empty)
                {
                    throw new ArgumentException("MixChannels source matrices must be non-empty.", nameof(src));
                }

                if (mat.Size != size)
                {
                    throw new ArgumentException("MixChannels source matrices must have the same size.", nameof(src));
                }

                if (mat.Depth != depth)
                {
                    throw new ArgumentException("MixChannels source matrices must have the same depth.", nameof(src));
                }

                sourceChannels += mat.Channels;
            }

            int destinationChannels = 0;
            for (int i = 0; i < dst.Length; i++)
            {
                Mat mat = dst[i];
                if (mat == null)
                {
                    throw new ArgumentNullException(nameof(dst));
                }

                if (mat.Empty)
                {
                    throw new ArgumentException("MixChannels destination matrices must be pre-allocated.", nameof(dst));
                }

                if (mat.Size != size)
                {
                    throw new ArgumentException("MixChannels destination matrices must have the same size as the first source matrix.", nameof(dst));
                }

                if (mat.Depth != depth)
                {
                    throw new ArgumentException("MixChannels destination matrices must have the same depth as the first source matrix.", nameof(dst));
                }

                destinationChannels += mat.Channels;
            }

            for (int i = 0; i < fromTo.Length; i += 2)
            {
                int sourceChannel = fromTo[i];
                int destinationChannel = fromTo[i + 1];

                if (sourceChannel >= sourceChannels)
                {
                    throw new ArgumentOutOfRangeException(nameof(fromTo), "Source channel index must be within the continuous source channel range.");
                }

                if (destinationChannel < 0 || destinationChannel >= destinationChannels)
                {
                    throw new ArgumentOutOfRangeException(nameof(fromTo), "Destination channel index must be within the continuous destination channel range.");
                }
            }
        }

        /// <summary>
        /// Repeats a matrix multiple times in the vertical and horizontal directions.
        /// 在垂直和水平方向上重复矩阵。
        /// </summary>
        public static void Repeat(Mat src, int ny, int nx, Mat dst)
        {
            ValidateMatPair(src, dst);
            ValidateRepeatInput(src);
            ValidatePositive(ny, nameof(ny));
            ValidatePositive(nx, nameof(nx));
            NativeException.ThrowIfError(NativeMethods.CoreRepeat(src.NativeHandle, ny, nx, dst.NativeHandle));
        }

        private static void ValidateRepeatInput(Mat src)
        {
            if (src.Dims > 2)
            {
                throw new ArgumentException("Repeat source matrix must have at most two dimensions.", nameof(src));
            }
        }

        /// <summary>
        /// Repeats a matrix and returns a new matrix.
        /// 重复矩阵，并返回新矩阵。
        /// </summary>
        public static Mat Repeat(Mat src, int ny, int nx)
        {
            var dst = new Mat();
            try
            {
                Repeat(src, ny, nx, dst);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Flips a matrix around x-axis, y-axis, or both axes.
        /// 围绕 X 轴、Y 轴或双轴翻转矩阵。
        /// </summary>
        public static void Flip(Mat src, Mat dst, int flipCode)
        {
            ValidateMatPair(src, dst);
            ValidateFlipInput(src);
            NativeException.ThrowIfError(NativeMethods.CoreFlip(src.NativeHandle, dst.NativeHandle, flipCode));
        }

        private static void ValidateFlipInput(Mat src)
        {
            if (src.Dims > 2)
            {
                throw new ArgumentException("Flip source matrix must have at most two dimensions.", nameof(src));
            }
        }

        /// <summary>
        /// Flips a matrix and returns a new matrix.
        /// 翻转矩阵，并返回新矩阵。
        /// </summary>
        public static Mat Flip(Mat src, int flipCode)
        {
            var dst = new Mat();
            try
            {
                Flip(src, dst, flipCode);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Rotates a matrix by a predefined right-angle mode.
        /// 按预定义直角模式旋转矩阵。
        /// </summary>
        public static void Rotate(Mat src, Mat dst, RotateFlags rotateCode)
        {
            ValidateMatPair(src, dst);
            ValidateRotateInput(src);
            ValidateRotateFlag(rotateCode, nameof(rotateCode));
            NativeException.ThrowIfError(NativeMethods.CoreRotate(src.NativeHandle, dst.NativeHandle, (int)rotateCode));
        }

        private static void ValidateRotateInput(Mat src)
        {
            if (src.Dims > 2)
            {
                throw new ArgumentException("Rotate source matrix must have at most two dimensions.", nameof(src));
            }
        }

        private static void ValidateRotateFlag(RotateFlags value, string parameterName)
        {
            if (value != RotateFlags.Rotate90Clockwise &&
                value != RotateFlags.Rotate180 &&
                value != RotateFlags.Rotate90Counterclockwise)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Unsupported rotation mode.");
            }
        }

        /// <summary>
        /// Rotates a matrix and returns a new matrix.
        /// 旋转矩阵，并返回新矩阵。
        /// </summary>
        public static Mat Rotate(Mat src, RotateFlags rotateCode)
        {
            var dst = new Mat();
            try
            {
                Rotate(src, dst, rotateCode);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Transposes a matrix.
        /// 转置矩阵。
        /// </summary>
        public static void Transpose(Mat src, Mat dst)
        {
            ValidateMatPair(src, dst);
            ValidateTransposeInput(src);
            NativeException.ThrowIfError(NativeMethods.CoreTranspose(src.NativeHandle, dst.NativeHandle));
        }

        private static void ValidateTransposeInput(Mat src)
        {
            if (src.Dims > 2)
            {
                throw new ArgumentException("Transpose source matrix must have at most two dimensions.", nameof(src));
            }
        }

        /// <summary>
        /// Transposes a matrix and returns a new matrix.
        /// 转置矩阵，并返回新矩阵。
        /// </summary>
        public static Mat Transpose(Mat src)
        {
            var dst = new Mat();
            try
            {
                Transpose(src, dst);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Applies a look-up table transformation.
        /// 应用查找表变换。
        /// </summary>
        public static void Lut(Mat src, Mat lut, Mat dst)
        {
            ValidateMatTriple(src, lut, dst);
            ValidateLutInputs(src, lut);
            NativeException.ThrowIfError(NativeMethods.CoreLut(src.NativeHandle, lut.NativeHandle, dst.NativeHandle));
        }

        private static void ValidateLutInputs(Mat src, Mat lut)
        {
            int sourceChannels = src.Channels;
            int lookupTableChannels = lut.Channels;
            if (lookupTableChannels != sourceChannels && lookupTableChannels != 1)
            {
                throw new ArgumentException("Lookup table must have either one channel or the same channel count as the source matrix.", nameof(lut));
            }

            if (!lut.IsContinuous)
            {
                throw new ArgumentException("Lookup table matrix data must be continuous.", nameof(lut));
            }

            int sourceDepth = src.Depth;
            ulong lookupTableSize = lut.Total.ToUInt64();
            bool isEightBitSource = sourceDepth == MatType.CV_8U || sourceDepth == MatType.CV_8S;
            bool isSixteenBitSource = sourceDepth == MatType.CV_16U || sourceDepth == MatType.CV_16S;
            if (!((lookupTableSize == 256UL && isEightBitSource) || (lookupTableSize == 65536UL && isSixteenBitSource)))
            {
                throw new ArgumentException("Lookup table length must match the source matrix depth.", nameof(lut));
            }
        }

        /// <summary>
        /// Applies a look-up table transformation and returns a new matrix.
        /// 应用查找表变换，并返回新矩阵。
        /// </summary>
        public static Mat Lut(Mat src, Mat lut)
        {
            var dst = new Mat();
            try
            {
                Lut(src, lut, dst);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Converts an array to absolute values with scaling.
        /// 以缩放方式将数组转换为绝对值。
        /// </summary>
        public static void ConvertScaleAbs(Mat src, Mat dst, double alpha = 1.0, double beta = 0.0)
        {
            ValidateMatPair(src, dst);
            NativeException.ThrowIfError(NativeMethods.CoreConvertScaleAbs(src.NativeHandle, dst.NativeHandle, alpha, beta));
        }

        /// <summary>
        /// Converts an array to absolute values with scaling and returns a new matrix.
        /// 以缩放方式将数组转换为绝对值，并返回新矩阵。
        /// </summary>
        public static Mat ConvertScaleAbs(Mat src, double alpha = 1.0, double beta = 0.0)
        {
            var dst = new Mat();
            try
            {
                ConvertScaleAbs(src, dst, alpha, beta);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Completes the symmetric part of a matrix.
        /// 补全矩阵的对称部分。
        /// </summary>
        public static void CompleteSymm(Mat mat, bool lowerToUpper = true)
        {
            ValidateNotNull(mat, nameof(mat));
            ValidateCompleteSymmInput(mat, nameof(mat));
            NativeException.ThrowIfError(NativeMethods.CoreCompleteSymm(mat.NativeHandle, lowerToUpper ? 1 : 0));
        }

        private static void ValidateCompleteSymmInput(Mat mat, string parameterName)
        {
            if (mat.Dims > 2 || mat.Rows != mat.Cols)
            {
                throw new ArgumentException("Matrix must be two-dimensional or less and square.", parameterName);
            }
        }

        /// <summary>
        /// Sets a matrix to an identity matrix.
        /// 将矩阵设置为单位矩阵。
        /// </summary>
        public static void SetIdentity(Mat mat, Scalar value)
        {
            ValidateNotNull(mat, nameof(mat));
            ValidateSetIdentityInput(mat, nameof(mat));
            NativeException.ThrowIfError(NativeMethods.CoreSetIdentity(mat.NativeHandle, value.V0, value.V1, value.V2, value.V3));
        }

        private static void ValidateSetIdentityInput(Mat mat, string parameterName)
        {
            if (mat.Dims > 2)
            {
                throw new ArgumentException("Matrix must be two-dimensional or less.", parameterName);
            }
        }

        /// <summary>
        /// Sets a matrix to an identity matrix with ones on the main diagonal.
        /// 将矩阵设置为主对角线为 1 的单位矩阵。
        /// </summary>
        public static void SetIdentity(Mat mat)
        {
            SetIdentity(mat, new Scalar(1.0));
        }

        private static Mat[] CreateMatArray(IntPtr[] handles, int count)
        {
            Mat[] result = new Mat[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = new Mat(handles[i]);
            }

            return result;
        }
    }
}

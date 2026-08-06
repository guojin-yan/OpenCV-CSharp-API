namespace JYPPX.OpenCvSharp.Core
{
    /// <summary>
    /// Provides OpenCV matrix type constants and helpers.
    /// 提供 OpenCV 矩阵类型常量和辅助方法。
    /// </summary>
    public static partial class MatType
    {
        /// <summary>
        /// Creates an OpenCV matrix type value from a depth and channel count.
        /// 根据深度和通道数创建 OpenCV 矩阵类型值。
        /// </summary>
        /// <param name="depth">The OpenCV element depth. OpenCV 元素深度。</param>
        /// <param name="channels">The channel count. 通道数。</param>
        /// <returns>The encoded OpenCV matrix type. 编码后的 OpenCV 矩阵类型。</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when <paramref name="depth"/> or <paramref name="channels"/> is outside the supported OpenCV matrix type range. 当 <paramref name="depth"/> 或 <paramref name="channels"/> 超出受支持的 OpenCV 矩阵类型范围时抛出。</exception>
        public static int MakeType(int depth, int channels)
        {
            if (depth < 0 || depth >= DepthMax)
            {
                throw new System.ArgumentOutOfRangeException(nameof(depth), "Depth must be in the supported OpenCV matrix depth range.");
            }

            if (channels < 1 || channels > ChannelMax)
            {
                throw new System.ArgumentOutOfRangeException(nameof(channels), "Channel count must be in the supported OpenCV matrix channel range.");
            }

            return Depth(type: depth) + ((channels - 1) << ChannelShift);
        }

        /// <summary>Creates an unsigned 8-bit matrix type with the requested channel count.</summary>
        public static int CV_8UC(int channels) { return MakeType(CV_8U, channels); }
        /// <summary>Creates a signed 8-bit matrix type with the requested channel count.</summary>
        public static int CV_8SC(int channels) { return MakeType(CV_8S, channels); }
        /// <summary>Creates an unsigned 16-bit matrix type with the requested channel count.</summary>
        public static int CV_16UC(int channels) { return MakeType(CV_16U, channels); }
        /// <summary>Creates a signed 16-bit matrix type with the requested channel count.</summary>
        public static int CV_16SC(int channels) { return MakeType(CV_16S, channels); }
        /// <summary>Creates a signed 32-bit matrix type with the requested channel count.</summary>
        public static int CV_32SC(int channels) { return MakeType(CV_32S, channels); }
        /// <summary>Creates a 32-bit floating-point matrix type with the requested channel count.</summary>
        public static int CV_32FC(int channels) { return MakeType(CV_32F, channels); }
        /// <summary>Creates a 64-bit floating-point matrix type with the requested channel count.</summary>
        public static int CV_64FC(int channels) { return MakeType(CV_64F, channels); }
        /// <summary>Creates an IEEE 16-bit floating-point matrix type with the requested channel count.</summary>
        public static int CV_16FC(int channels) { return MakeType(CV_16F, channels); }
        /// <summary>Creates a bfloat16 matrix type with the requested channel count.</summary>
        public static int CV_16BFC(int channels) { return MakeType(CV_16BF, channels); }
        /// <summary>Creates a Boolean matrix type with the requested channel count.</summary>
        public static int CV_BoolC(int channels) { return MakeType(CV_Bool, channels); }
        /// <summary>Creates an unsigned 64-bit matrix type with the requested channel count.</summary>
        public static int CV_64UC(int channels) { return MakeType(CV_64U, channels); }
        /// <summary>Creates a signed 64-bit matrix type with the requested channel count.</summary>
        public static int CV_64SC(int channels) { return MakeType(CV_64S, channels); }
        /// <summary>Creates an unsigned 32-bit matrix type with the requested channel count.</summary>
        public static int CV_32UC(int channels) { return MakeType(CV_32U, channels); }

        /// <summary>
        /// Gets the element depth from an encoded OpenCV matrix type.
        /// 从编码后的 OpenCV 矩阵类型中获取元素深度。
        /// </summary>
        /// <param name="type">The encoded OpenCV matrix type. 编码后的 OpenCV 矩阵类型。</param>
        /// <returns>The OpenCV element depth. OpenCV 元素深度。</returns>
        public static int Depth(int type)
        {
            return type & DepthMask;
        }

        /// <summary>
        /// Gets the channel count from an encoded OpenCV matrix type.
        /// 从编码后的 OpenCV 矩阵类型中获取通道数。
        /// </summary>
        /// <param name="type">The encoded OpenCV matrix type. 编码后的 OpenCV 矩阵类型。</param>
        /// <returns>The channel count. 通道数。</returns>
        public static int Channels(int type)
        {
            return ((type & ChannelMask) >> ChannelShift) + 1;
        }

        /// <summary>
        /// Gets the matrix type bits from an encoded OpenCV matrix flags value.
        /// 从编码后的 OpenCV 矩阵 flags 值中获取矩阵类型位。
        /// </summary>
        /// <param name="flags">The OpenCV matrix flags value. OpenCV 矩阵 flags 值。</param>
        /// <returns>The OpenCV matrix type. OpenCV 矩阵类型。</returns>
        public static int TypeMask(int flags)
        {
            return flags & MatrixTypeMask;
        }
    }
}

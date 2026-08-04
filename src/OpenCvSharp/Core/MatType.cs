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

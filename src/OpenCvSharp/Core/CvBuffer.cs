using System;

namespace JYPPX.OpenCvSharp.Core
{
    /// <summary>
    /// Provides buffer helpers used by image and matrix data paths.
    /// 提供图像和矩阵数据路径使用的缓冲区辅助方法。
    /// </summary>
    public static class CvBuffer
    {
        /// <summary>
        /// Copies bytes from one array to another.
        /// 将字节从一个数组复制到另一个数组。
        /// </summary>
        /// <param name="source">The source buffer. 源缓冲区。</param>
        /// <param name="destination">The destination buffer. 目标缓冲区。</param>
        public static void Copy(byte[] source, byte[] destination)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            if (destination.Length < source.Length)
            {
                throw new ArgumentException("Destination buffer is smaller than the source buffer.", nameof(destination));
            }

#if NETCOREAPP3_1_OR_GREATER
            source.AsSpan().CopyTo(destination.AsSpan());
#else
            Buffer.BlockCopy(source, 0, destination, 0, source.Length);
#endif
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Copies bytes by using span-based fast paths on modern .NET.
        /// 在现代 .NET 上使用基于 Span 的快速路径复制字节。
        /// </summary>
        /// <param name="source">The source span. 源 Span。</param>
        /// <param name="destination">The destination span. 目标 Span。</param>
        public static void Copy(ReadOnlySpan<byte> source, Span<byte> destination)
        {
            if (destination.Length < source.Length)
            {
                throw new ArgumentException("Destination span is smaller than the source span.", nameof(destination));
            }

            source.CopyTo(destination);
        }
#endif
    }
}


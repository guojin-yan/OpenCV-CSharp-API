using System;
using System.Runtime.InteropServices;
using OpenCvSharp.Core;

namespace OpenCvSharp.Internal.Interop
{
    /// <summary>
    /// Provides internal helpers for matrix ownership, slicing, and byte-view access.
    /// 提供矩阵所有权、切片和字节视图访问的内部辅助方法。
    /// </summary>
    internal static class MatViewAdapter
    {
        private const string MatrixDataNotContinuousMessage = "Matrix data is not continuous.";

        internal static int GetByteLength(Mat mat)
        {
            if (mat == null)
            {
                throw new ArgumentNullException(nameof(mat));
            }

            ulong byteLength = mat.Total.ToUInt64() * mat.ElemSize.ToUInt64();
            if (byteLength > int.MaxValue)
            {
                throw new OpenCvException("Matrix byte length is larger than Int32.MaxValue.");
            }

            return (int)byteLength;
        }

        internal static int GetContinuousByteLength(Mat mat)
        {
            if (mat == null)
            {
                throw new ArgumentNullException(nameof(mat));
            }

            if (!mat.IsContinuous)
            {
                throw new OpenCvException(MatrixDataNotContinuousMessage);
            }

            if (mat.Data == IntPtr.Zero)
            {
                return 0;
            }

            return GetByteLength(mat);
        }

#if NETCOREAPP3_1_OR_GREATER
        internal static unsafe Span<byte> AsByteSpan(Mat mat)
        {
            if (mat == null)
            {
                throw new ArgumentNullException(nameof(mat));
            }

            if (!mat.IsContinuous)
            {
                throw new OpenCvException(MatrixDataNotContinuousMessage);
            }

            IntPtr data = mat.Data;
            if (data == IntPtr.Zero)
            {
                return Span<byte>.Empty;
            }

            int byteLength = GetByteLength(mat);
            return new Span<byte>(data.ToPointer(), byteLength);
        }

        internal static unsafe Span<T> AsSpan<T>(Mat mat) where T : unmanaged
        {
            if (mat == null)
            {
                throw new ArgumentNullException(nameof(mat));
            }

            if (!mat.IsContinuous)
            {
                throw new OpenCvException(MatrixDataNotContinuousMessage);
            }

            IntPtr data = mat.Data;
            if (data == IntPtr.Zero)
            {
                return Span<T>.Empty;
            }

            int byteLength = GetByteLength(mat);
            int elementSize = Marshal.SizeOf<T>();
            if (byteLength % elementSize != 0)
            {
                throw new OpenCvException("Matrix element size does not match the requested span type.");
            }

            return new Span<T>(data.ToPointer(), byteLength / elementSize);
        }

        internal static ReadOnlySpan<T> AsReadOnlySpan<T>(Mat mat) where T : unmanaged
        {
            return AsSpan<T>(mat);
        }
#endif
    }
}

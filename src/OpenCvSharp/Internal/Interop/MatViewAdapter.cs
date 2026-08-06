using System;
using System.Runtime.InteropServices;
using JYPPX.OpenCvSharp.Core;

namespace JYPPX.OpenCvSharp.Internal.Interop
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

        internal static int GetRowByteLength(Mat mat)
        {
            if (mat == null)
            {
                throw new ArgumentNullException(nameof(mat));
            }

            if (mat.Dims != 2)
            {
                throw new OpenCvException("Row access requires a two-dimensional matrix.");
            }

            ulong byteLength = checked((ulong)mat.Cols * mat.ElemSize.ToUInt64());
            if (byteLength > int.MaxValue)
            {
                throw new OpenCvException("Matrix row byte length is larger than Int32.MaxValue.");
            }

            return (int)byteLength;
        }

        internal static void CopyTo(Mat mat, byte[] destination)
        {
            int byteLength = GetByteLength(mat);
            if (destination.Length < byteLength)
            {
                throw new ArgumentException("Destination buffer is smaller than the matrix byte length.", nameof(destination));
            }

            if (byteLength == 0)
            {
                return;
            }

            if (mat.IsContinuous)
            {
                Marshal.Copy(mat.Data, destination, 0, byteLength);
                return;
            }

            if (mat.Dims != 2)
            {
                using (Mat contiguous = mat.Clone())
                {
                    CopyTo(contiguous, destination);
                }
                return;
            }

            int rowByteLength = GetRowByteLength(mat);
            for (int row = 0; row < mat.Rows; row++)
            {
                Marshal.Copy(GetRowPointer(mat, row), destination, checked(row * rowByteLength), rowByteLength);
            }
        }

        internal static void CopyFrom(Mat mat, byte[] source)
        {
            int byteLength = GetByteLength(mat);
            if (source.Length < byteLength)
            {
                throw new ArgumentException("Source buffer is smaller than the matrix byte length.", nameof(source));
            }

            if (byteLength == 0)
            {
                return;
            }

            if (mat.IsContinuous)
            {
                Marshal.Copy(source, 0, mat.Data, byteLength);
                return;
            }

            if (mat.Dims != 2)
            {
                using (Mat contiguous = mat.Clone())
                {
                    CopyFrom(contiguous, source);
                    contiguous.CopyTo(mat);
                }
                return;
            }

            int rowByteLength = GetRowByteLength(mat);
            for (int row = 0; row < mat.Rows; row++)
            {
                Marshal.Copy(source, checked(row * rowByteLength), GetRowPointer(mat, row), rowByteLength);
            }
        }

        internal static void CopyRowTo(Mat mat, int row, byte[] destination)
        {
            int rowByteLength = ValidateRowCopy(mat, row, destination.Length, nameof(destination));
            if (rowByteLength != 0)
            {
                Marshal.Copy(GetRowPointer(mat, row), destination, 0, rowByteLength);
            }
        }

        internal static void CopyRowFrom(Mat mat, int row, byte[] source)
        {
            int rowByteLength = ValidateRowCopy(mat, row, source.Length, nameof(source));
            if (rowByteLength != 0)
            {
                Marshal.Copy(source, 0, GetRowPointer(mat, row), rowByteLength);
            }
        }

        internal static unsafe void CopyPixelsTo(Mat mat, IntPtr destination, long destinationStep)
        {
            int rowByteLength = ValidatePixelCopy(mat, destination, destinationStep, nameof(destination), nameof(destinationStep));
            byte* destinationPointer = (byte*)destination.ToPointer();
            for (int row = 0; row < mat.Rows; row++)
            {
                Buffer.MemoryCopy(
                    GetRowPointer(mat, row).ToPointer(),
                    destinationPointer + checked(row * destinationStep),
                    rowByteLength,
                    rowByteLength);
            }
        }

        internal static unsafe void CopyPixelsFrom(Mat mat, IntPtr source, long sourceStep)
        {
            int rowByteLength = ValidatePixelCopy(mat, source, sourceStep, nameof(source), nameof(sourceStep));
            byte* sourcePointer = (byte*)source.ToPointer();
            for (int row = 0; row < mat.Rows; row++)
            {
                Buffer.MemoryCopy(
                    sourcePointer + checked(row * sourceStep),
                    GetRowPointer(mat, row).ToPointer(),
                    rowByteLength,
                    rowByteLength);
            }
        }

        private static int ValidatePixelCopy(Mat mat, IntPtr buffer, long bufferStep, string bufferName, string stepName)
        {
            if (buffer == IntPtr.Zero)
            {
                throw new ArgumentException("Pixel buffer pointer cannot be zero.", bufferName);
            }

            int rowByteLength = GetRowByteLength(mat);
            if (bufferStep == long.MinValue || Math.Abs(bufferStep) < rowByteLength)
            {
                throw new ArgumentOutOfRangeException(stepName, "Pixel buffer step is smaller than the logical matrix row.");
            }

            if (mat.Rows > 1)
            {
                checked
                {
                    _ = (mat.Rows - 1) * bufferStep;
                }
            }

            return rowByteLength;
        }

        private static int ValidateRowCopy(Mat mat, int row, int bufferLength, string parameterName)
        {
            int rowByteLength = GetRowByteLength(mat);
            if ((uint)row >= (uint)mat.Rows)
            {
                throw new ArgumentOutOfRangeException(nameof(row));
            }
            if (bufferLength < rowByteLength)
            {
                throw new ArgumentException("Buffer is smaller than the matrix row byte length.", parameterName);
            }

            return rowByteLength;
        }

        private static IntPtr GetRowPointer(Mat mat, int row)
        {
            ulong offset = checked((ulong)row * mat.Step.ToUInt64());
            if (offset > long.MaxValue)
            {
                throw new OpenCvException("Matrix row offset is larger than Int64.MaxValue.");
            }

            return new IntPtr(checked(mat.Data.ToInt64() + (long)offset));
        }

#if NETCOREAPP3_1_OR_GREATER
        internal static unsafe void CopyTo(Mat mat, Span<byte> destination)
        {
            int byteLength = GetByteLength(mat);
            if (destination.Length < byteLength)
            {
                throw new ArgumentException("Destination span is smaller than the matrix byte length.", nameof(destination));
            }

            if (byteLength == 0)
            {
                return;
            }

            if (mat.IsContinuous)
            {
                AsByteSpan(mat).CopyTo(destination);
                return;
            }

            if (mat.Dims != 2)
            {
                using (Mat contiguous = mat.Clone())
                {
                    CopyTo(contiguous, destination);
                }
                return;
            }

            int rowByteLength = GetRowByteLength(mat);
            for (int row = 0; row < mat.Rows; row++)
            {
                ReadOnlySpan<byte> source = new ReadOnlySpan<byte>(GetRowPointer(mat, row).ToPointer(), rowByteLength);
                source.CopyTo(destination.Slice(checked(row * rowByteLength), rowByteLength));
            }
        }

        internal static unsafe void CopyFrom(Mat mat, ReadOnlySpan<byte> source)
        {
            int byteLength = GetByteLength(mat);
            if (source.Length < byteLength)
            {
                throw new ArgumentException("Source span is smaller than the matrix byte length.", nameof(source));
            }

            if (byteLength == 0)
            {
                return;
            }

            if (mat.IsContinuous)
            {
                source.Slice(0, byteLength).CopyTo(AsByteSpan(mat));
                return;
            }

            if (mat.Dims != 2)
            {
                using (Mat contiguous = mat.Clone())
                {
                    CopyFrom(contiguous, source);
                    contiguous.CopyTo(mat);
                }
                return;
            }

            int rowByteLength = GetRowByteLength(mat);
            for (int row = 0; row < mat.Rows; row++)
            {
                Span<byte> destination = new Span<byte>(GetRowPointer(mat, row).ToPointer(), rowByteLength);
                source.Slice(checked(row * rowByteLength), rowByteLength).CopyTo(destination);
            }
        }

        internal static unsafe void CopyRowTo(Mat mat, int row, Span<byte> destination)
        {
            int rowByteLength = ValidateRowCopy(mat, row, destination.Length, nameof(destination));
            ReadOnlySpan<byte> source = new ReadOnlySpan<byte>(GetRowPointer(mat, row).ToPointer(), rowByteLength);
            source.CopyTo(destination);
        }

        internal static unsafe void CopyRowFrom(Mat mat, int row, ReadOnlySpan<byte> source)
        {
            int rowByteLength = ValidateRowCopy(mat, row, source.Length, nameof(source));
            Span<byte> destination = new Span<byte>(GetRowPointer(mat, row).ToPointer(), rowByteLength);
            source.Slice(0, rowByteLength).CopyTo(destination);
        }

        internal static unsafe Span<byte> AsRowByteSpan(Mat mat, int row)
        {
            int rowByteLength = ValidateRowCopy(mat, row, int.MaxValue, nameof(row));
            if (rowByteLength == 0)
            {
                return Span<byte>.Empty;
            }

            return new Span<byte>(GetRowPointer(mat, row).ToPointer(), rowByteLength);
        }

        internal static Span<T> AsRowSpan<T>(Mat mat, int row) where T : unmanaged
        {
            Span<byte> bytes = AsRowByteSpan(mat, row);
            int elementSize = Marshal.SizeOf<T>();
            if (bytes.Length % elementSize != 0)
            {
                throw new OpenCvException("Matrix row byte length does not match the requested span type.");
            }

            return MemoryMarshal.Cast<byte, T>(bytes);
        }

        internal static MatRowAccessor<T> AsRows<T>(Mat mat) where T : unmanaged
        {
            _ = GetRowByteLength(mat);
            ValidateMatrixElementType<T>(mat);
            if (mat.Rows > 0 && mat.Cols > 0 && mat.Data == IntPtr.Zero)
            {
                throw new OpenCvException("Matrix data pointer is null.");
            }

            return new MatRowAccessor<T>(mat.Data, mat.Step.ToUInt64(), mat.Rows, mat.Cols);
        }

        internal static T GetValue<T>(Mat mat, int row, int column) where T : unmanaged
        {
            ValidateMatrixElementType<T>(mat);
            if ((uint)column >= (uint)mat.Cols)
            {
                throw new ArgumentOutOfRangeException(nameof(column));
            }

            return AsRowSpan<T>(mat, row)[column];
        }

        internal static void SetValue<T>(Mat mat, int row, int column, T value) where T : unmanaged
        {
            ValidateMatrixElementType<T>(mat);
            if ((uint)column >= (uint)mat.Cols)
            {
                throw new ArgumentOutOfRangeException(nameof(column));
            }

            AsRowSpan<T>(mat, row)[column] = value;
        }

        private static void ValidateMatrixElementType<T>(Mat mat) where T : unmanaged
        {
            if (mat.ElemSize.ToUInt64() != (ulong)Marshal.SizeOf<T>())
            {
                throw new OpenCvException("The requested value type size does not match the matrix element size.");
            }
        }

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

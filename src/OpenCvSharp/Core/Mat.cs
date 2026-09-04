using System;
using System.Runtime.InteropServices;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Core
{
    /// <summary>
    /// Represents an OpenCV matrix object compatible with <c>cv::Mat</c>.
    /// 表示与 OpenCV <c>cv::Mat</c> 对应的矩阵对象。
    /// </summary>
    public sealed class Mat : IDisposable
    {
        private NativeMatHandle handle;
        private bool disposed;
        private long viewRevision;

        /// <summary>
        /// Initializes an empty managed matrix placeholder.
        /// 初始化一个空的 managed 矩阵占位对象。
        /// </summary>
        public Mat()
        {
            NativeException.ThrowIfError(NativeMethods.MatCreateEmpty(out IntPtr nativeHandle));
            handle = NativeMatHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>
        /// Initializes a new matrix with the specified row count, column count, and OpenCV type.
        /// 使用指定的行数、列数和 OpenCV 类型初始化新矩阵。
        /// </summary>
        /// <param name="rows">The number of rows. 行数。</param>
        /// <param name="cols">The number of columns. 列数。</param>
        /// <param name="type">The OpenCV matrix type. OpenCV 矩阵类型。</param>
        public Mat(int rows, int cols, int type)
        {
            NativeException.ThrowIfError(NativeMethods.MatCreate(rows, cols, type, out IntPtr nativeHandle));
            handle = NativeMatHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>
        /// Initializes a new matrix with the specified size and OpenCV type.
        /// 使用指定尺寸和 OpenCV 类型初始化新矩阵。
        /// </summary>
        /// <param name="size">The matrix size. 矩阵尺寸。</param>
        /// <param name="type">The OpenCV matrix type. OpenCV 矩阵类型。</param>
        public Mat(Size size, int type)
            : this(size.Height, size.Width, type)
        {
        }

        /// <summary>
        /// Initializes a new matrix and fills it with a scalar value.
        /// 初始化新矩阵，并使用标量值填充。
        /// </summary>
        /// <param name="rows">The number of rows. 行数。</param>
        /// <param name="cols">The number of columns. 列数。</param>
        /// <param name="type">The OpenCV matrix type. OpenCV 矩阵类型。</param>
        /// <param name="value">The fill value. 填充值。</param>
        public Mat(int rows, int cols, int type, Scalar value)
        {
            NativeException.ThrowIfError(NativeMethods.MatCreateWithScalar(
                rows,
                cols,
                type,
                value.V0,
                value.V1,
                value.V2,
                value.V3,
                out IntPtr nativeHandle));
            handle = NativeMatHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>
        /// Initializes a new matrix with the specified size and fills it with a scalar value.
        /// 使用指定尺寸初始化新矩阵，并使用标量值填充。
        /// </summary>
        /// <param name="size">The matrix size. 矩阵尺寸。</param>
        /// <param name="type">The OpenCV matrix type. OpenCV 矩阵类型。</param>
        /// <param name="value">The fill value. 填充值。</param>
        public Mat(Size size, int type, Scalar value)
            : this(size.Height, size.Width, type, value)
        {
        }

        internal Mat(IntPtr nativeHandle)
        {
            handle = NativeMatHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>
        /// Gets a value indicating whether the matrix has been disposed.
        /// 获取矩阵是否已经释放。
        /// </summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        /// <summary>
        /// Gets a value indicating whether the matrix is empty.
        /// 获取矩阵是否为空。
        /// </summary>
        public bool Empty
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.MatEmpty(handle.DangerousGetHandle(), out int empty));
                return empty != 0;
            }
        }

        /// <summary>
        /// Gets the number of dimensions in the matrix.
        /// 获取矩阵维度数量。
        /// </summary>
        public int Dims
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.MatDims(handle.DangerousGetHandle(), out int dims));
                return dims;
            }
        }

        /// <summary>
        /// Gets the number of rows in the matrix.
        /// 获取矩阵行数。
        /// </summary>
        public int Rows
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.MatRows(handle.DangerousGetHandle(), out int rows));
                return rows;
            }
        }

        /// <summary>
        /// Gets the number of columns in the matrix.
        /// 获取矩阵列数。
        /// </summary>
        public int Cols
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.MatCols(handle.DangerousGetHandle(), out int cols));
                return cols;
            }
        }

        /// <summary>
        /// Gets the matrix width, equivalent to <see cref="Cols"/>.
        /// 获取矩阵宽度，等同于 <see cref="Cols"/>。
        /// </summary>
        public int Width
        {
            get { return Cols; }
        }

        /// <summary>
        /// Gets the matrix height, equivalent to <see cref="Rows"/>.
        /// 获取矩阵高度，等同于 <see cref="Rows"/>。
        /// </summary>
        public int Height
        {
            get { return Rows; }
        }

        /// <summary>
        /// Gets the matrix size as width and height.
        /// 获取矩阵尺寸，宽度对应列数，高度对应行数。
        /// </summary>
        public Size Size
        {
            get { return new Size(Cols, Rows); }
        }

        /// <summary>
        /// Gets the number of matrix channels.
        /// 获取矩阵通道数。
        /// </summary>
        public int Channels
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.MatChannels(handle.DangerousGetHandle(), out int channels));
                return channels;
            }
        }

        /// <summary>
        /// Gets the OpenCV element depth.
        /// 获取 OpenCV 元素深度。
        /// </summary>
        public int Depth
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.MatDepth(handle.DangerousGetHandle(), out int depth));
                return depth;
            }
        }

        /// <summary>
        /// Gets the OpenCV matrix type.
        /// 获取 OpenCV 矩阵类型。
        /// </summary>
        public int Type
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.MatType(handle.DangerousGetHandle(), out int type));
                return type;
            }
        }

        /// <summary>
        /// Gets the total number of matrix elements.
        /// 获取矩阵元素总数。
        /// </summary>
        public UIntPtr Total
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.MatTotal(handle.DangerousGetHandle(), out UIntPtr total));
                return total;
            }
        }

        /// <summary>
        /// Gets the size of each matrix element in bytes.
        /// 获取每个矩阵元素的字节大小。
        /// </summary>
        public UIntPtr ElemSize
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.MatElemSize(handle.DangerousGetHandle(), out UIntPtr elemSize));
                return elemSize;
            }
        }

        /// <summary>
        /// Gets the size of one channel element in bytes.
        /// 获取单通道元素的字节大小。
        /// </summary>
        public UIntPtr ElemSize1
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.MatElemSize1(handle.DangerousGetHandle(), out UIntPtr elemSize1));
                return elemSize1;
            }
        }

        /// <summary>
        /// Gets the byte step between adjacent matrix rows.
        /// 获取相邻矩阵行之间的字节跨度。
        /// </summary>
        public UIntPtr Step
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.MatStep(handle.DangerousGetHandle(), out UIntPtr step));
                return step;
            }
        }

        /// <summary>
        /// Gets the row step measured in single-channel elements.
        /// 获取以单通道元素为单位的行跨度。
        /// </summary>
        public UIntPtr Step1
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.MatStep1(handle.DangerousGetHandle(), out UIntPtr step1));
                return step1;
            }
        }

        /// <summary>
        /// Gets the native data pointer of the matrix.
        /// 获取矩阵的 native 数据指针。
        /// </summary>
        public IntPtr Data
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.MatData(handle.DangerousGetHandle(), out IntPtr data));
                return data;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the matrix memory is continuous.
        /// 获取矩阵内存是否连续。
        /// </summary>
        public bool IsContinuous
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.MatIsContinuous(handle.DangerousGetHandle(), out int isContinuous));
                return isContinuous != 0;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this matrix is a submatrix view.
        /// 获取此矩阵是否为子矩阵视图。
        /// </summary>
        public bool IsSubmatrix
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.MatIsSubmatrix(handle.DangerousGetHandle(), out int isSubmatrix));
                return isSubmatrix != 0;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the matrix data pointer is null.
        /// 获取矩阵数据指针是否为空。
        /// </summary>
        public bool DataPointerIsNull
        {
            get { return Data == IntPtr.Zero; }
        }

        /// <summary>
        /// Gets a value indicating whether the matrix has a non-null data pointer.
        /// 获取矩阵是否具有非空数据指针。
        /// </summary>
        public bool HasData
        {
            get { return Data != IntPtr.Zero; }
        }

        /// <summary>
        /// Gets the number of bytes required to store all matrix elements.
        /// 获取存储所有矩阵元素所需的字节数。
        /// </summary>
        /// <exception cref="OpenCvException">Thrown when the byte length is larger than <see cref="int.MaxValue"/>. 当字节长度大于 <see cref="int.MaxValue"/> 时抛出。</exception>
        public int ByteLength
        {
            get
            {
                ThrowIfDisposed();
                return MatViewAdapter.GetByteLength(this);
            }
        }

        /// <summary>
        /// Gets the number of logical payload bytes in one row of a two-dimensional matrix.
        /// The value excludes any padding represented by <see cref="Step"/>.
        /// </summary>
        public int RowByteLength
        {
            get
            {
                ThrowIfDisposed();
                return MatViewAdapter.GetRowByteLength(this);
            }
        }

        /// <summary>
        /// Gets the number of single-channel values in the matrix.
        /// 获取矩阵中的单通道值数量。
        /// </summary>
        public long ValueCount
        {
            get { return checked((long)Total.ToUInt64() * Channels); }
        }

        /// <summary>
        /// Gets a value indicating whether the matrix has one channel.
        /// 获取矩阵是否为单通道。
        /// </summary>
        public bool IsSingleChannel
        {
            get { return Channels == 1; }
        }

        /// <summary>
        /// Gets a value indicating whether the matrix has three channels.
        /// 获取矩阵是否为三通道。
        /// </summary>
        public bool IsThreeChannel
        {
            get { return Channels == 3; }
        }

        /// <summary>
        /// Gets a value indicating whether the matrix has four channels.
        /// 获取矩阵是否为四通道。
        /// </summary>
        public bool IsFourChannel
        {
            get { return Channels == 4; }
        }

        internal IntPtr NativeHandle
        {
            get
            {
                ThrowIfDisposed();
                return handle.DangerousGetHandle();
            }
        }

        internal long ViewRevision
        {
            get
            {
                ThrowIfDisposed();
                return viewRevision;
            }
        }

        /// <summary>
        /// Creates a zero-filled matrix.
        /// 创建填充为零的矩阵。
        /// </summary>
        /// <param name="rows">The number of rows. 行数。</param>
        /// <param name="cols">The number of columns. 列数。</param>
        /// <param name="type">The OpenCV matrix type. OpenCV 矩阵类型。</param>
        /// <returns>A new matrix. 新矩阵。</returns>
        public static Mat Zeros(int rows, int cols, int type)
        {
            NativeException.ThrowIfError(NativeMethods.MatZeros(rows, cols, type, out IntPtr nativeHandle));
            return new Mat(nativeHandle);
        }

        /// <summary>
        /// Creates a zero-filled matrix.
        /// 创建填充为零的矩阵。
        /// </summary>
        /// <param name="size">The matrix size. 矩阵尺寸。</param>
        /// <param name="type">The OpenCV matrix type. OpenCV 矩阵类型。</param>
        /// <returns>A new matrix. 新矩阵。</returns>
        public static Mat Zeros(Size size, int type)
        {
            return Zeros(size.Height, size.Width, type);
        }

        /// <summary>
        /// Creates a one-filled matrix.
        /// 创建填充为一的矩阵。
        /// </summary>
        /// <param name="rows">The number of rows. 行数。</param>
        /// <param name="cols">The number of columns. 列数。</param>
        /// <param name="type">The OpenCV matrix type. OpenCV 矩阵类型。</param>
        /// <returns>A new matrix. 新矩阵。</returns>
        public static Mat Ones(int rows, int cols, int type)
        {
            NativeException.ThrowIfError(NativeMethods.MatOnes(rows, cols, type, out IntPtr nativeHandle));
            return new Mat(nativeHandle);
        }

        /// <summary>
        /// Creates a one-filled matrix.
        /// 创建填充为一的矩阵。
        /// </summary>
        /// <param name="size">The matrix size. 矩阵尺寸。</param>
        /// <param name="type">The OpenCV matrix type. OpenCV 矩阵类型。</param>
        /// <returns>A new matrix. 新矩阵。</returns>
        public static Mat Ones(Size size, int type)
        {
            return Ones(size.Height, size.Width, type);
        }

        /// <summary>
        /// Creates an identity matrix.
        /// 创建单位矩阵。
        /// </summary>
        /// <param name="rows">The number of rows. 行数。</param>
        /// <param name="cols">The number of columns. 列数。</param>
        /// <param name="type">The OpenCV matrix type. OpenCV 矩阵类型。</param>
        /// <returns>A new matrix. 新矩阵。</returns>
        public static Mat Eye(int rows, int cols, int type)
        {
            NativeException.ThrowIfError(NativeMethods.MatEye(rows, cols, type, out IntPtr nativeHandle));
            return new Mat(nativeHandle);
        }

        /// <summary>
        /// Creates an identity matrix.
        /// 创建单位矩阵。
        /// </summary>
        /// <param name="size">The matrix size. 矩阵尺寸。</param>
        /// <param name="type">The OpenCV matrix type. OpenCV 矩阵类型。</param>
        /// <returns>A new matrix. 新矩阵。</returns>
        public static Mat Eye(Size size, int type)
        {
            return Eye(size.Height, size.Width, type);
        }

        /// <summary>
        /// Creates storage for this matrix.
        /// 为此矩阵创建存储。
        /// </summary>
        /// <param name="rows">The number of rows. 行数。</param>
        /// <param name="cols">The number of columns. 列数。</param>
        /// <param name="type">The OpenCV matrix type. OpenCV 矩阵类型。</param>
        public void Create(int rows, int cols, int type)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MatCreateInPlace(NativeHandle, rows, cols, type));
            viewRevision = checked(viewRevision + 1);
        }

        /// <summary>
        /// Creates storage for this matrix.
        /// 为此矩阵创建存储。
        /// </summary>
        /// <param name="size">The matrix size. 矩阵尺寸。</param>
        /// <param name="type">The OpenCV matrix type. OpenCV 矩阵类型。</param>
        public void Create(Size size, int type)
        {
            Create(size.Height, size.Width, type);
        }

        /// <summary>
        /// Creates a deep copy of this matrix.
        /// 创建此矩阵的深拷贝。
        /// </summary>
        /// <returns>A new matrix with independent storage. 具有独立存储的新矩阵。</returns>
        public Mat Clone()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MatClone(NativeHandle, out IntPtr nativeHandle));
            return new Mat(nativeHandle);
        }

        /// <summary>
        /// Copies this matrix into another matrix.
        /// 将此矩阵复制到另一个矩阵。
        /// </summary>
        /// <param name="destination">The destination matrix. 目标矩阵。</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="destination"/> is null. 当 <paramref name="destination"/> 为空时抛出。</exception>
        public void CopyTo(Mat destination)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MatCopyTo(NativeHandle, destination.NativeHandle));
        }

        /// <summary>
        /// Converts this matrix to another type with optional scaling and offset.
        /// 将此矩阵转换为另一种类型，并可选进行缩放和偏移。
        /// </summary>
        /// <param name="destination">The destination matrix. 目标矩阵。</param>
        /// <param name="rtype">The desired output matrix type, or a depth such as <c>MatType.CV_32F</c>. 期望输出矩阵类型，或类似 <c>MatType.CV_32F</c> 的深度。</param>
        /// <param name="alpha">The scale factor. 缩放因子。</param>
        /// <param name="beta">The offset added after scaling. 缩放后添加的偏移。</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="destination"/> is null. 当 <paramref name="destination"/> 为空时抛出。</exception>
        public void ConvertTo(Mat destination, int rtype, double alpha = 1.0, double beta = 0.0)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MatConvertTo(NativeHandle, destination.NativeHandle, rtype, alpha, beta));
        }

        /// <summary>
        /// Converts this matrix to another type and returns the converted matrix.
        /// 将此矩阵转换为另一种类型并返回转换结果。
        /// </summary>
        /// <param name="rtype">The desired output matrix type, or a depth such as <c>MatType.CV_32F</c>. 期望输出矩阵类型，或类似 <c>MatType.CV_32F</c> 的深度。</param>
        /// <param name="alpha">The scale factor. 缩放因子。</param>
        /// <param name="beta">The offset added after scaling. 缩放后添加的偏移。</param>
        /// <returns>The converted matrix. 转换后的矩阵。</returns>
        public Mat ConvertTo(int rtype, double alpha = 1.0, double beta = 0.0)
        {
            var result = new Mat();
            try
            {
                ConvertTo(result, rtype, alpha, beta);
                return result;
            }
            catch
            {
                result.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Creates a copied matrix from this matrix.
        /// 从此矩阵创建一个复制矩阵。
        /// </summary>
        /// <returns>A copied matrix. 复制得到的矩阵。</returns>
        public Mat ToMat()
        {
            return Clone();
        }

        /// <summary>
        /// Sets all matrix elements to the specified scalar value.
        /// 将矩阵全部元素设置为指定标量值。
        /// </summary>
        /// <param name="value">The value to assign. 要设置的值。</param>
        public void SetTo(Scalar value)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MatSetTo(NativeHandle, value.V0, value.V1, value.V2, value.V3));
        }

        /// <summary>
        /// Returns a matrix view for the specified region of interest.
        /// 返回指定感兴趣区域的矩阵视图。
        /// </summary>
        /// <param name="roi">The region of interest. 感兴趣区域。</param>
        /// <returns>A submatrix view sharing the same native data. 共享相同 native 数据的子矩阵视图。</returns>
        public Mat SubMat(Rect roi)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MatSubmat(NativeHandle, roi.X, roi.Y, roi.Width, roi.Height, out IntPtr nativeHandle));
            return new Mat(nativeHandle);
        }

        /// <summary>
        /// Returns a matrix view for a row range.
        /// 返回指定行范围的矩阵视图。
        /// </summary>
        /// <param name="startRow">The inclusive start row. 起始行，包含。</param>
        /// <param name="endRow">The exclusive end row. 结束行，不包含。</param>
        /// <returns>A submatrix view sharing the same native data. 共享相同 native 数据的子矩阵视图。</returns>
        public Mat RowRange(int startRow, int endRow)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MatRowRange(NativeHandle, startRow, endRow, out IntPtr nativeHandle));
            return new Mat(nativeHandle);
        }

        /// <summary>
        /// Returns a matrix view for a single row.
        /// 返回单行矩阵视图。
        /// </summary>
        /// <param name="row">The row index. 行索引。</param>
        /// <returns>A row view. 行视图。</returns>
        public Mat Row(int row)
        {
            return RowRange(row, row + 1);
        }

        /// <summary>
        /// Returns a matrix view for a column range.
        /// 返回指定列范围的矩阵视图。
        /// </summary>
        /// <param name="startCol">The inclusive start column. 起始列，包含。</param>
        /// <param name="endCol">The exclusive end column. 结束列，不包含。</param>
        /// <returns>A submatrix view sharing the same native data. 共享相同 native 数据的子矩阵视图。</returns>
        public Mat ColRange(int startCol, int endCol)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MatColRange(NativeHandle, startCol, endCol, out IntPtr nativeHandle));
            return new Mat(nativeHandle);
        }

        /// <summary>
        /// Returns a matrix view for a single column.
        /// 返回单列矩阵视图。
        /// </summary>
        /// <param name="col">The column index. 列索引。</param>
        /// <returns>A column view. 列视图。</returns>
        public Mat Col(int col)
        {
            return ColRange(col, col + 1);
        }

        /// <summary>
        /// Changes the shape and channel count without copying data when OpenCV can represent it as a view.
        /// 在 OpenCV 可表达为视图时，更改矩阵形状和通道数而不复制数据。
        /// </summary>
        /// <param name="channels">The new channel count, or 0 to keep the existing channel count. 新通道数，0 表示保持不变。</param>
        /// <param name="rows">The new row count, or 0 to infer it. 新行数，0 表示自动推导。</param>
        /// <returns>A reshaped matrix view. 重排后的矩阵视图。</returns>
        public Mat Reshape(int channels, int rows = 0)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MatReshape(NativeHandle, channels, rows, out IntPtr nativeHandle));
            return new Mat(nativeHandle);
        }

        /// <summary>
        /// Copies logical matrix bytes from a managed buffer, honoring row stride for non-continuous matrices.
        /// </summary>
        /// <param name="source">The source buffer. 源缓冲区。</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is null. 当 <paramref name="source"/> 为空时抛出。</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="source"/> is smaller than the matrix byte length. 当 <paramref name="source"/> 小于矩阵字节长度时抛出。</exception>
        public void CopyFrom(byte[] source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            ThrowIfDisposed();
            MatViewAdapter.CopyFrom(this, source);
        }

        /// <summary>
        /// Copies logical matrix bytes into a managed buffer, honoring row stride for non-continuous matrices.
        /// </summary>
        /// <param name="destination">The destination buffer. 目标缓冲区。</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="destination"/> is null. 当 <paramref name="destination"/> 为空时抛出。</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="destination"/> is smaller than the matrix byte length. 当 <paramref name="destination"/> 小于矩阵字节长度时抛出。</exception>
        public void CopyTo(byte[] destination)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            ThrowIfDisposed();
            MatViewAdapter.CopyTo(this, destination);
        }

        /// <summary>
        /// Copies one logical matrix row into a managed buffer without exposing the native data pointer.
        /// </summary>
        public void CopyRowTo(int row, byte[] destination)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            ThrowIfDisposed();
            MatViewAdapter.CopyRowTo(this, row, destination);
        }

        /// <summary>
        /// Copies managed bytes into one logical matrix row without writing row padding.
        /// </summary>
        public void CopyRowFrom(int row, byte[] source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            ThrowIfDisposed();
            MatViewAdapter.CopyRowFrom(this, row, source);
        }

        /// <summary>
        /// Copies logical pixel rows to an unmanaged buffer with an independent row step.
        /// This supports padded and bottom-up UI bitmap buffers without exposing matrix pointer arithmetic.
        /// </summary>
        /// <param name="destination">Pointer to the first byte of logical row zero.</param>
        /// <param name="destinationStep">Bytes between destination rows; may be negative for a bottom-up buffer.</param>
        public void CopyPixelsTo(IntPtr destination, long destinationStep)
        {
            ThrowIfDisposed();
            MatViewAdapter.CopyPixelsTo(this, destination, destinationStep);
        }

        /// <summary>
        /// Copies logical pixel rows from an unmanaged buffer with an independent row step.
        /// This supports padded and bottom-up UI bitmap buffers without exposing matrix pointer arithmetic.
        /// </summary>
        /// <param name="source">Pointer to the first byte of logical row zero.</param>
        /// <param name="sourceStep">Bytes between source rows; may be negative for a bottom-up buffer.</param>
        public void CopyPixelsFrom(IntPtr source, long sourceStep)
        {
            ThrowIfDisposed();
            MatViewAdapter.CopyPixelsFrom(this, source, sourceStep);
        }

        /// <summary>
        /// Copies bytes into a newly allocated managed array.
        /// 将矩阵字节复制到新分配的 managed 数组。
        /// </summary>
        /// <returns>A byte array containing matrix data. 包含矩阵数据的字节数组。</returns>
        public byte[] ToBytes()
        {
            byte[] bytes = new byte[ByteLength];
            CopyTo(bytes);
            return bytes;
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Copies logical matrix bytes from a span, honoring row stride for non-continuous matrices.
        /// </summary>
        /// <param name="source">The source span. 源 Span。</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="source"/> is smaller than the matrix byte length. 当 <paramref name="source"/> 小于矩阵字节长度时抛出。</exception>
        public void CopyFrom(ReadOnlySpan<byte> source)
        {
            ThrowIfDisposed();
            MatViewAdapter.CopyFrom(this, source);
        }

        /// <summary>
        /// Copies logical matrix bytes into a span, honoring row stride for non-continuous matrices.
        /// </summary>
        /// <param name="destination">The destination span. 目标 Span。</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="destination"/> is smaller than the matrix byte length. 当 <paramref name="destination"/> 小于矩阵字节长度时抛出。</exception>
        public void CopyTo(Span<byte> destination)
        {
            ThrowIfDisposed();
            MatViewAdapter.CopyTo(this, destination);
        }

        /// <summary>Copies one logical matrix row into a span without exposing native memory.</summary>
        public void CopyRowTo(int row, Span<byte> destination)
        {
            ThrowIfDisposed();
            MatViewAdapter.CopyRowTo(this, row, destination);
        }

        /// <summary>Copies a span into one logical matrix row without writing row padding.</summary>
        public void CopyRowFrom(int row, ReadOnlySpan<byte> source)
        {
            ThrowIfDisposed();
            MatViewAdapter.CopyRowFrom(this, row, source);
        }

        /// <summary>Gets a writable byte span over one logical row, excluding row padding.</summary>
        public Span<byte> AsRowByteSpan(int row)
        {
            ThrowIfDisposed();
            return MatViewAdapter.AsRowByteSpan(this, row);
        }

        /// <summary>Gets a read-only byte span over one logical row, excluding row padding.</summary>
        public ReadOnlySpan<byte> AsReadOnlyRowByteSpan(int row)
        {
            ThrowIfDisposed();
            return MatViewAdapter.AsRowByteSpan(this, row);
        }

        /// <summary>
        /// Gets a writable typed span over one logical row. The requested type must divide the logical row byte length.
        /// For example, use <see cref="Vec3b"/> with a <c>CV_8UC3</c> matrix.
        /// </summary>
        public Span<T> AsRowSpan<T>(int row) where T : unmanaged
        {
            ThrowIfDisposed();
            return MatViewAdapter.AsRowSpan<T>(this, row);
        }

        /// <summary>Gets a read-only typed span over one logical row.</summary>
        public ReadOnlySpan<T> AsReadOnlyRowSpan<T>(int row) where T : unmanaged
        {
            ThrowIfDisposed();
            return MatViewAdapter.AsRowSpan<T>(this, row);
        }

        /// <summary>
        /// Gets a zero-allocation typed row accessor for a two-dimensional matrix.
        /// The requested type must exactly match one matrix element, such as <see cref="Vec3b"/> for <c>CV_8UC3</c>.
        /// </summary>
        public MatRowAccessor<T> AsRows<T>() where T : unmanaged
        {
            ThrowIfDisposed();
            return MatViewAdapter.AsRows<T>(this);
        }

        /// <summary>
        /// Creates a type-checked borrowed pixel view. This API is a preview and is available on Span-capable target frameworks.
        /// </summary>
        /// <typeparam name="TPixel">A pixel type registered by <see cref="PixelTypeTraits"/>.</typeparam>
        /// <returns>A typed view that does not own or dispose this matrix.</returns>
        public MatView<TPixel> AsView<TPixel>() where TPixel : unmanaged
        {
            ThrowIfDisposed();
            return new MatView<TPixel>(this);
        }

        /// <summary>
        /// Copies typed values from a span into the logical matrix payload, honoring row stride.
        /// 将类型化 Span 中的值按逻辑行复制到矩阵，并正确处理行步长。
        /// </summary>
        /// <typeparam name="T">The unmanaged element type. 非托管元素类型。</typeparam>
        /// <param name="source">The source span. 源 Span。</param>
        public void CopyFrom<T>(ReadOnlySpan<T> source) where T : unmanaged
        {
            int elementSize = Marshal.SizeOf<T>();
            if (ByteLength % elementSize != 0)
            {
                throw new OpenCvException("Matrix element size does not match the requested span type.");
            }

            CopyFrom(MemoryMarshal.AsBytes(source));
        }

        /// <summary>
        /// Copies typed values from the logical matrix payload into a span, honoring row stride.
        /// 将矩阵中的类型化值按逻辑行复制到 Span，并正确处理行步长。
        /// </summary>
        /// <typeparam name="T">The unmanaged element type. 非托管元素类型。</typeparam>
        /// <param name="destination">The destination span. 目标 Span。</param>
        public void CopyTo<T>(Span<T> destination) where T : unmanaged
        {
            int elementSize = Marshal.SizeOf<T>();
            if (ByteLength % elementSize != 0)
            {
                throw new OpenCvException("Matrix element size does not match the requested span type.");
            }

            CopyTo(MemoryMarshal.AsBytes(destination));
        }

        /// <summary>
        /// Gets a span over the continuous matrix byte buffer.
        /// 获取连续矩阵字节缓冲区上的 Span。
        /// </summary>
        /// <returns>A span over matrix bytes. 矩阵字节 Span。</returns>
        /// <exception cref="OpenCvException">Thrown when the matrix is not continuous. 当矩阵不是连续内存时抛出。</exception>
        public Span<byte> AsByteSpan()
        {
            ThrowIfDisposed();
            return MatViewAdapter.AsByteSpan(this);
        }

        /// <summary>
        /// Gets a typed span over the continuous matrix buffer.
        /// 获取连续矩阵缓冲区上的类型化 Span。
        /// </summary>
        /// <typeparam name="T">The unmanaged element type. 非托管元素类型。</typeparam>
        /// <returns>A typed span over matrix data. 矩阵数据上的类型化 Span。</returns>
        public Span<T> AsSpan<T>() where T : unmanaged
        {
            ThrowIfDisposed();
            return MatViewAdapter.AsSpan<T>(this);
        }

        /// <summary>
        /// Gets a read-only typed span over the continuous matrix buffer.
        /// 获取连续矩阵缓冲区上的只读类型化 Span。
        /// </summary>
        /// <typeparam name="T">The unmanaged element type. 非托管元素类型。</typeparam>
        /// <returns>A read-only typed span over matrix data. 矩阵数据上的只读类型化 Span。</returns>
        public ReadOnlySpan<T> AsReadOnlySpan<T>() where T : unmanaged
        {
            ThrowIfDisposed();
            return MatViewAdapter.AsReadOnlySpan<T>(this);
        }

        /// <summary>
        /// Tries to get a byte span over the matrix data.
        /// 尝试获取矩阵数据上的字节 Span。
        /// </summary>
        /// <param name="span">The resulting span. 输出 Span。</param>
        /// <returns><c>true</c> when the matrix is continuous; otherwise <c>false</c>. 矩阵连续时为 <c>true</c>，否则为 <c>false</c>。</returns>
        public bool TryGetByteSpan(out Span<byte> span)
        {
            ThrowIfDisposed();
            if (!IsContinuous)
            {
                span = Span<byte>.Empty;
                return false;
            }

            span = MatViewAdapter.AsByteSpan(this);
            return true;
        }

        /// <summary>
        /// Tries to get a typed span over the matrix data.
        /// 尝试获取矩阵数据上的类型化 Span。
        /// </summary>
        /// <typeparam name="T">The unmanaged element type. 非托管元素类型。</typeparam>
        /// <param name="span">The resulting span. 输出 Span。</param>
        /// <returns><c>true</c> when the matrix is continuous and compatible; otherwise <c>false</c>. 矩阵连续且兼容时为 <c>true</c>，否则为 <c>false</c>。</returns>
        public bool TryGetSpan<T>(out Span<T> span) where T : unmanaged
        {
            ThrowIfDisposed();
            if (!IsContinuous)
            {
                span = Span<T>.Empty;
                return false;
            }

            try
            {
                span = MatViewAdapter.AsSpan<T>(this);
                return true;
            }
            catch (OpenCvException)
            {
                span = Span<T>.Empty;
                return false;
            }
        }

        /// <summary>
        /// Gets a typed value from a continuous matrix.
        /// 从连续矩阵获取类型化值。
        /// </summary>
        /// <typeparam name="T">The unmanaged element type. 非托管元素类型。</typeparam>
        /// <param name="index">The flat element index. 扁平元素索引。</param>
        /// <returns>The value. 值。</returns>
        public T GetValue<T>(int index) where T : unmanaged
        {
            return AsReadOnlySpan<T>()[index];
        }

        /// <summary>
        /// Gets a two-dimensional matrix element. The unmanaged type size must exactly match <see cref="ElemSize"/>.
        /// </summary>
        public T GetValue<T>(int row, int column) where T : unmanaged
        {
            ThrowIfDisposed();
            return MatViewAdapter.GetValue<T>(this, row, column);
        }

        /// <summary>
        /// Sets a typed value in a continuous matrix.
        /// 在连续矩阵中设置类型化值。
        /// </summary>
        /// <typeparam name="T">The unmanaged element type. 非托管元素类型。</typeparam>
        /// <param name="index">The flat element index. 扁平元素索引。</param>
        /// <param name="value">The value to write. 要写入的值。</param>
        public void SetValue<T>(int index, T value) where T : unmanaged
        {
            AsSpan<T>()[index] = value;
        }

        /// <summary>
        /// Sets a two-dimensional matrix element. The unmanaged type size must exactly match <see cref="ElemSize"/>.
        /// </summary>
        public void SetValue<T>(int row, int column, T value) where T : unmanaged
        {
            ThrowIfDisposed();
            MatViewAdapter.SetValue(this, row, column, value);
        }

        /// <summary>
        /// Copies typed matrix values into a new managed array.
        /// 将类型化矩阵值复制到新的 managed 数组。
        /// </summary>
        /// <typeparam name="T">The unmanaged element type. 非托管元素类型。</typeparam>
        /// <returns>A new array containing matrix values. 包含矩阵值的新数组。</returns>
        public T[] ToArray<T>() where T : unmanaged
        {
            int elementSize = Marshal.SizeOf<T>();
            if (ByteLength % elementSize != 0)
            {
                throw new OpenCvException("Matrix element size does not match the requested array type.");
            }

            var result = new T[ByteLength / elementSize];
            CopyTo(result.AsSpan());
            return result;
        }
#endif

        /// <summary>
        /// Releases resources used by this matrix.
        /// 释放此矩阵使用的资源。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return disposed
                ? "{Disposed=True}"
                : "{Rows=" + Rows + ",Cols=" + Cols + ",Type=" + Type + ",Channels=" + Channels + "}";
        }

        private void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing && handle != null)
            {
                handle.Dispose();
            }

            disposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(Mat));
            }
        }
    }
}

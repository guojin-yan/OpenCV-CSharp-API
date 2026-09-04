#if NETCOREAPP3_1_OR_GREATER
using System;

namespace JYPPX.OpenCvSharp.Core
{
    /// <summary>
    /// Provides a type-checked, borrowed view over a two-dimensional <see cref="Mat"/>.
    /// This API is a preview and is available only on Span-capable target frameworks.
    /// </summary>
    /// <typeparam name="TPixel">A pixel type registered by <see cref="PixelTypeTraits"/>.</typeparam>
    public sealed class MatView<TPixel> : IDisposable where TPixel : unmanaged
    {
        private readonly Mat owner;
        private readonly PixelTypeDescriptor descriptor;
        private readonly int rowCount;
        private readonly int columnCount;
        private readonly long ownerRevision;
        private readonly UIntPtr step;
        private readonly IntPtr data;
        private bool disposed;

        /// <summary>
        /// Creates a borrowed typed view over a two-dimensional matrix.
        /// The matrix type must exactly match the registered <typeparamref name="TPixel"/> descriptor.
        /// </summary>
        /// <param name="mat">The matrix whose storage is borrowed.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="mat"/> is <see langword="null"/>.</exception>
        /// <exception cref="NotSupportedException">Thrown when <typeparamref name="TPixel"/> is not registered.</exception>
        /// <exception cref="OpenCvException">Thrown when the matrix is not two-dimensional or its type does not match.</exception>
        public MatView(Mat mat)
        {
            if (mat == null)
            {
                throw new ArgumentNullException(nameof(mat));
            }

            owner = mat;
            descriptor = PixelTypeDescriptor.Get<TPixel>();

            int dimensions = mat.Dims;
            if (dimensions != 2)
            {
                throw new OpenCvException("Typed views require a two-dimensional matrix.");
            }

            int type = mat.Type;
            if (!descriptor.MatchesMatType(type))
            {
                throw new OpenCvException(
                    "The requested pixel type does not match the matrix depth and channel count.");
            }

            rowCount = mat.Rows;
            columnCount = mat.Cols;
            ownerRevision = mat.ViewRevision;
            step = mat.Step;
            data = mat.Data;
            ulong rowByteLength = checked((ulong)columnCount * (ulong)descriptor.ElementSizeBytes);
            if (rowCount > 0 && columnCount > 0 && step.ToUInt64() < rowByteLength)
            {
                throw new OpenCvException("Matrix row stride is smaller than the logical row payload.");
            }
            if (rowCount > 0 && columnCount > 0 && data == IntPtr.Zero)
            {
                throw new OpenCvException("Matrix data pointer is null.");
            }
        }

        /// <summary>Gets the registered storage descriptor for this view.</summary>
        public PixelTypeDescriptor Descriptor
        {
            get
            {
                EnsureUsable();
                return descriptor;
            }
        }

        /// <summary>Gets the number of matrix rows.</summary>
        public int Rows
        {
            get
            {
                EnsureUsable();
                return rowCount;
            }
        }

        /// <summary>Gets the number of typed pixels in each matrix row.</summary>
        public int Columns
        {
            get
            {
                EnsureUsable();
                return columnCount;
            }
        }

        /// <summary>Gets a value indicating whether the matrix payload is continuous.</summary>
        public bool IsContinuous
        {
            get
            {
                EnsureUsable();
                return owner.IsContinuous;
            }
        }

        /// <summary>
        /// Gets a writable span over the complete matrix.
        /// This requires continuous storage; use <see cref="AsRowSpan(int)"/> for an ROI.
        /// </summary>
        public Span<TPixel> AsSpan()
        {
            EnsureUsable();
            return owner.AsSpan<TPixel>();
        }

        /// <summary>Gets a read-only span over the complete continuous matrix.</summary>
        public ReadOnlySpan<TPixel> AsReadOnlySpan()
        {
            EnsureUsable();
            return owner.AsReadOnlySpan<TPixel>();
        }

        /// <summary>Tries to get a writable span over the complete matrix.</summary>
        public bool TryGetSpan(out Span<TPixel> span)
        {
            EnsureUsable();
            return owner.TryGetSpan(out span);
        }

        /// <summary>Gets a writable span over one logical row, including non-contiguous ROIs.</summary>
        public Span<TPixel> AsRowSpan(int row)
        {
            EnsureUsable();
            return owner.AsRowSpan<TPixel>(row);
        }

        /// <summary>Gets a read-only span over one logical row, including non-contiguous ROIs.</summary>
        public ReadOnlySpan<TPixel> AsReadOnlyRowSpan(int row)
        {
            EnsureUsable();
            return owner.AsReadOnlyRowSpan<TPixel>(row);
        }

        /// <summary>Gets a zero-allocation row accessor for the matrix.</summary>
        public MatRowAccessor<TPixel> AsRows()
        {
            EnsureUsable();
            return owner.AsRows<TPixel>();
        }

        /// <summary>Gets one typed pixel at the specified row and column.</summary>
        public TPixel GetValue(int row, int column)
        {
            EnsureUsable();
            return owner.GetValue<TPixel>(row, column);
        }

        /// <summary>Sets one typed pixel at the specified row and column.</summary>
        public void SetValue(int row, int column, TPixel value)
        {
            EnsureUsable();
            owner.SetValue(row, column, value);
        }

        /// <summary>Copies logical matrix pixels into a destination span.</summary>
        public void CopyTo(Span<TPixel> destination)
        {
            EnsureUsable();
            owner.CopyTo(destination);
        }

        /// <summary>Copies source pixels into the logical matrix payload.</summary>
        public void CopyFrom(ReadOnlySpan<TPixel> source)
        {
            EnsureUsable();
            owner.CopyFrom(source);
        }

        /// <summary>Copies logical matrix pixels into a new managed array.</summary>
        public TPixel[] ToArray()
        {
            EnsureUsable();
            return owner.ToArray<TPixel>();
        }

        /// <summary>
        /// Invalidates this view. The borrowed <see cref="Mat"/> is not disposed.
        /// Spans obtained before disposal must not be used after this view or its matrix is disposed.
        /// </summary>
        public void Dispose()
        {
            disposed = true;
        }

        private void EnsureUsable()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(MatView<TPixel>));
            }

            // Revalidate the native header so a later Mat.Create cannot make this view address stale storage.
            if (owner.Dims != 2 ||
                !descriptor.MatchesMatType(owner.Type) ||
                owner.Rows != rowCount ||
                owner.Cols != columnCount ||
                owner.ViewRevision != ownerRevision ||
                owner.Step != step ||
                owner.Data != data)
            {
                throw new InvalidOperationException("The matrix changed after this typed view was created.");
            }
        }
    }
}
#endif

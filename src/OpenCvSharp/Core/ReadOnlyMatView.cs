#if NETCOREAPP3_1_OR_GREATER
using System;

namespace JYPPX.OpenCvSharp.Core
{
    /// <summary>
    /// Provides a type-checked, borrowed read-only view over a two-dimensional <see cref="Mat"/>.
    /// This API is a preview and is available only on Span-capable target frameworks.
    /// </summary>
    /// <typeparam name="TPixel">A pixel type registered by <see cref="PixelTypeTraits"/>.</typeparam>
    public sealed class ReadOnlyMatView<TPixel> : IDisposable where TPixel : unmanaged
    {
        private readonly MatView<TPixel> view;

        /// <summary>Creates a borrowed read-only view over a two-dimensional matrix.</summary>
        /// <param name="mat">The matrix whose storage is borrowed.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="mat"/> is <see langword="null"/>.</exception>
        /// <exception cref="NotSupportedException">Thrown when <typeparamref name="TPixel"/> is not registered.</exception>
        /// <exception cref="OpenCvException">Thrown when the matrix shape or type does not match.</exception>
        public ReadOnlyMatView(Mat mat)
        {
            view = new MatView<TPixel>(mat);
        }

        /// <summary>Gets the registered storage descriptor for this view.</summary>
        public PixelTypeDescriptor Descriptor { get { return view.Descriptor; } }

        /// <summary>Gets the number of matrix rows.</summary>
        public int Rows { get { return view.Rows; } }

        /// <summary>Gets the number of typed pixels in each matrix row.</summary>
        public int Columns { get { return view.Columns; } }

        /// <summary>Gets a value indicating whether the matrix payload is continuous.</summary>
        public bool IsContinuous { get { return view.IsContinuous; } }

        /// <summary>Gets a read-only span over the complete continuous matrix.</summary>
        public ReadOnlySpan<TPixel> AsReadOnlySpan() { return view.AsReadOnlySpan(); }

        /// <summary>Tries to get a read-only span over the complete matrix.</summary>
        public bool TryGetSpan(out ReadOnlySpan<TPixel> span)
        {
            if (view.TryGetSpan(out Span<TPixel> writable))
            {
                span = writable;
                return true;
            }

            span = default(ReadOnlySpan<TPixel>);
            return false;
        }

        /// <summary>Gets a read-only span over one logical row, including non-contiguous ROIs.</summary>
        public ReadOnlySpan<TPixel> AsReadOnlyRowSpan(int row) { return view.AsReadOnlyRowSpan(row); }

        /// <summary>Gets one typed pixel at the specified row and column.</summary>
        public TPixel GetValue(int row, int column) { return view.GetValue(row, column); }

        /// <summary>Copies logical matrix pixels into a destination span.</summary>
        public void CopyTo(Span<TPixel> destination) { view.CopyTo(destination); }

        /// <summary>Copies logical matrix pixels into a new managed array.</summary>
        public TPixel[] ToArray() { return view.ToArray(); }

        /// <summary>Creates an owning deep copy of the viewed matrix or ROI.</summary>
        public Mat Clone() { return view.Clone(); }

        /// <summary>Copies the viewed matrix or ROI into an owning destination matrix.</summary>
        /// <param name="destination">The destination matrix. 目标矩阵。</param>
        public void CopyTo(Mat destination) { view.CopyTo(destination); }

        /// <summary>
        /// Invalidates this view. The borrowed <see cref="Mat"/> is not disposed.
        /// Spans obtained before disposal must not be used after this view or its matrix is disposed.
        /// </summary>
        public void Dispose() { view.Dispose(); }
    }
}
#endif

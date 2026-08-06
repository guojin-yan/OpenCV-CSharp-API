#if NETCOREAPP3_1_OR_GREATER
using System;

namespace JYPPX.OpenCvSharp.Core
{
    /// <summary>
    /// Provides zero-allocation typed row access to a two-dimensional matrix, including non-contiguous views.
    /// </summary>
    /// <typeparam name="T">An unmanaged type whose size exactly matches one matrix element.</typeparam>
    public readonly ref struct MatRowAccessor<T> where T : unmanaged
    {
        private readonly IntPtr data;
        private readonly ulong step;
        private readonly int columns;

        internal MatRowAccessor(IntPtr data, ulong step, int rows, int columns)
        {
            this.data = data;
            this.step = step;
            Count = rows;
            this.columns = columns;
        }

        /// <summary>Gets the number of accessible rows.</summary>
        public int Count { get; }

        /// <summary>Gets the number of typed elements in each row.</summary>
        public int Columns { get { return columns; } }

        /// <summary>Gets a writable span over one logical matrix row.</summary>
        public unsafe Span<T> this[int row]
        {
            get
            {
                if ((uint)row >= (uint)Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(row));
                }

                ulong offset = checked((ulong)row * step);
                if (offset > long.MaxValue)
                {
                    throw new OpenCvException("Matrix row offset is larger than Int64.MaxValue.");
                }

                long address = checked(data.ToInt64() + (long)offset);
                return new Span<T>(new IntPtr(address).ToPointer(), columns);
            }
        }
    }
}
#endif

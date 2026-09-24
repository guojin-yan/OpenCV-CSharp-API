using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal
{
    /// <summary>
    /// Keeps a managed byte owner pinned for a bounded native operation.
    /// This is an internal prototype; it is not a public Mat or codec API.
    /// </summary>
    internal sealed class CodecBufferLease : IDisposable
    {
        private readonly object sync = new object();
        private readonly GCHandle? pinnedOwner;
        private readonly Action? releaseCallback;
        private readonly IntPtr data;
        private readonly long lengthBytes;
        private readonly long stepBytes;
        private readonly int rows;
        private readonly long rowPayloadBytes;
        private int activeOperations;
        private bool disposeRequested;
        private bool released;
        private Exception? releaseException;

        private CodecBufferLease(
            GCHandle? pinnedOwner,
            IntPtr data,
            long lengthBytes,
            long stepBytes,
            int rows,
            long rowPayloadBytes,
            Action? releaseCallback)
        {
            this.pinnedOwner = pinnedOwner;
            this.data = data;
            this.lengthBytes = lengthBytes;
            this.stepBytes = stepBytes;
            this.rows = rows;
            this.rowPayloadBytes = rowPayloadBytes;
            this.releaseCallback = releaseCallback;
        }

        /// <summary>
        /// Pins a byte array and exposes a checked two-dimensional layout.
        /// </summary>
        internal static CodecBufferLease Pin(
            byte[] owner,
            int byteOffset,
            int rows,
            long stepBytes,
            long rowPayloadBytes,
            Action? releaseCallback = null)
        {
            ValidateLayout(owner, byteOffset, rows, stepBytes, rowPayloadBytes);

            GCHandle pinned = default(GCHandle);
            try
            {
                pinned = GCHandle.Alloc(owner, GCHandleType.Pinned);
                IntPtr baseAddress = pinned.AddrOfPinnedObject();
                return new CodecBufferLease(
                    pinned,
                    IntPtr.Add(baseAddress, byteOffset),
                    checked((long)owner.LongLength - byteOffset),
                    stepBytes,
                    rows,
                    rowPayloadBytes,
                    releaseCallback);
            }
            catch
            {
                if (pinned.IsAllocated)
                {
                    pinned.Free();
                }
                throw;
            }
        }

        /// <summary>
        /// Wraps an explicitly owned native pointer for a bounded operation.
        /// The release callback owns the native memory and is invoked once.
        /// </summary>
        internal static CodecBufferLease FromNativePointer(
            IntPtr nativePointer,
            long lengthBytes,
            int rows,
            long stepBytes,
            long rowPayloadBytes,
            Action releaseCallback)
        {
            if (nativePointer == IntPtr.Zero)
            {
                throw new ArgumentNullException(nameof(nativePointer));
            }
            if (releaseCallback == null)
            {
                throw new ArgumentNullException(nameof(releaseCallback));
            }

            ValidateLayout(lengthBytes, rows, stepBytes, rowPayloadBytes, nameof(lengthBytes));
            return new CodecBufferLease(null, nativePointer, lengthBytes, stepBytes, rows, rowPayloadBytes, releaseCallback);
        }

        /// <summary>Gets the first row pointer while the lease is usable.</summary>
        internal IntPtr Data
        {
            get
            {
                EnsureUsable();
                return data;
            }
        }

        /// <summary>Gets the number of bytes from the first row to the end of the owner.</summary>
        internal long LengthBytes
        {
            get
            {
                EnsureUsable();
                return lengthBytes;
            }
        }

        /// <summary>Gets the byte distance between row starts.</summary>
        internal long StepBytes
        {
            get
            {
                EnsureUsable();
                return stepBytes;
            }
        }

        /// <summary>Gets the number of logical rows.</summary>
        internal int Rows
        {
            get
            {
                EnsureUsable();
                return rows;
            }
        }

        /// <summary>Gets the logical payload bytes in each row.</summary>
        internal long RowPayloadBytes
        {
            get
            {
                EnsureUsable();
                return rowPayloadBytes;
            }
        }

        /// <summary>Gets a callback exception captured during release, if any.</summary>
        internal Exception? ReleaseException
        {
            get
            {
                lock (sync)
                {
                    return releaseException;
                }
            }
        }

        /// <summary>Gets a checked pointer to a logical row while the lease is usable.</summary>
        internal IntPtr GetRowPointer(int row)
        {
            EnsureUsable();
            if (row < 0 || row >= rows)
            {
                throw new ArgumentOutOfRangeException(nameof(row));
            }

            return IntPtr.Add(data, checked((int)checked((long)row * stepBytes)));
        }

        /// <summary>
        /// Enters the bounded native-call lifetime. Dispose may be requested during the guard;
        /// the pin is released only after the final guard exits.
        /// </summary>
        internal IDisposable EnterOperation()
        {
            lock (sync)
            {
                if (disposeRequested || released)
                {
                    throw new ObjectDisposedException(nameof(CodecBufferLease));
                }

                activeOperations++;
                return new OperationGuard(this);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            lock (sync)
            {
                if (disposeRequested)
                {
                    return;
                }

                disposeRequested = true;
                if (activeOperations == 0)
                {
                    ReleaseCore();
                }
            }
        }

        private static void ValidateLayout(
            byte[] owner,
            int byteOffset,
            int rows,
            long stepBytes,
            long rowPayloadBytes)
        {
            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner));
            }
            if (owner.Length == 0)
            {
                throw new ArgumentException("The pinned owner cannot be empty.", nameof(owner));
            }
            if (byteOffset < 0 || byteOffset >= owner.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(byteOffset));
            }
            if (rows <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(rows));
            }
            if (rowPayloadBytes <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(rowPayloadBytes));
            }
            if (stepBytes < rowPayloadBytes)
            {
                throw new ArgumentOutOfRangeException(nameof(stepBytes), "The row stride cannot be smaller than the logical row payload.");
            }

            long availableBytes = checked((long)owner.LongLength - byteOffset);
            ValidateLayout(availableBytes, rows, stepBytes, rowPayloadBytes, nameof(owner));
        }

        private static void ValidateLayout(
            long availableBytes,
            int rows,
            long stepBytes,
            long rowPayloadBytes,
            string ownerParameterName)
        {
            if (availableBytes <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(availableBytes));
            }
            if (rows <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(rows));
            }
            if (rowPayloadBytes <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(rowPayloadBytes));
            }
            if (stepBytes < rowPayloadBytes)
            {
                throw new ArgumentOutOfRangeException(nameof(stepBytes), "The row stride cannot be smaller than the logical row payload.");
            }

            long finalRowEnd = checked(checked((long)(rows - 1) * stepBytes) + rowPayloadBytes);
            if (finalRowEnd > availableBytes)
            {
                throw new ArgumentException("The owner is shorter than the requested rows and stride.", ownerParameterName);
            }
        }

        private void EnsureUsable()
        {
            lock (sync)
            {
                if (disposeRequested || released)
                {
                    throw new ObjectDisposedException(nameof(CodecBufferLease));
                }
            }
        }

        private void ExitOperation()
        {
            lock (sync)
            {
                if (activeOperations <= 0)
                {
                    return;
                }

                activeOperations--;
                if (activeOperations == 0 && disposeRequested)
                {
                    ReleaseCore();
                }
            }
        }

        private void ReleaseCore()
        {
            if (released)
            {
                return;
            }

            released = true;
            if (pinnedOwner.HasValue && pinnedOwner.Value.IsAllocated)
            {
                GCHandle handle = pinnedOwner.Value;
                handle.Free();
            }

            if (releaseCallback != null)
            {
                try
                {
                    releaseCallback();
                }
                catch (Exception exception)
                {
                    releaseException = exception;
                }
            }
        }

        private sealed class OperationGuard : IDisposable
        {
            private CodecBufferLease? owner;

            internal OperationGuard(CodecBufferLease owner)
            {
                this.owner = owner;
            }

            public void Dispose()
            {
                CodecBufferLease? value = owner;
                if (value == null)
                {
                    return;
                }

                owner = null;
                value.ExitOperation();
            }
        }
    }
}

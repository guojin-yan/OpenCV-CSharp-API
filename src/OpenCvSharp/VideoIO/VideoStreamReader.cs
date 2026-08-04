using System;
using System.IO;
using System.Runtime.InteropServices;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.VideoIO
{
    /// <summary>
    /// Callback-backed stream reader used by the stream overloads in <see cref="VideoCaptureExtensions"/>.
    /// 用于 <see cref="VideoCaptureExtensions"/> 流重载的回调流读取器。
    /// </summary>
    public sealed class VideoStreamReader : IDisposable
    {
        private readonly ReaderState state;
        private readonly GCHandle stateHandle;
        private readonly NativeMethods.VideoStreamReaderReadCallback readCallback;
        private readonly NativeMethods.VideoStreamReaderSeekCallback seekCallback;
        private readonly NativeMethods.VideoStreamReaderReleaseCallback releaseCallback;
        private NativeVideoStreamReaderHandle handle;
        private bool disposed;

        /// <summary>
        /// Creates a reader over a managed stream.
        /// 基于托管流创建读取器。
        /// </summary>
        /// <param name="stream">The source stream. 源流。</param>
        /// <param name="leaveOpen">Whether to keep the stream open after native release. native 释放后是否保持流打开。</param>
        public VideoStreamReader(Stream stream, bool leaveOpen = false)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            if (!stream.CanRead)
            {
                throw new ArgumentException("The stream must be readable.", nameof(stream));
            }

            state = new ReaderState(stream, leaveOpen);
            stateHandle = GCHandle.Alloc(state);
            readCallback = ReadCallback;
            seekCallback = SeekCallback;
            releaseCallback = ReleaseCallback;
            try
            {
                NativeException.ThrowIfError(NativeMethods.VideoStreamReaderCreate(
                    GCHandle.ToIntPtr(stateHandle),
                    readCallback,
                    seekCallback,
                    releaseCallback,
                    out IntPtr nativeHandle));
                handle = NativeVideoStreamReaderHandle.FromNativePointer(nativeHandle);
            }
            catch
            {
                if (stateHandle.IsAllocated)
                {
                    stateHandle.Free();
                }
                throw;
            }
        }

        internal IntPtr NativeHandle
        {
            get
            {
                ThrowIfDisposed();
                return handle.DangerousGetHandle();
            }
        }

        internal void ThrowIfCallbackFailed()
        {
            ThrowPendingException();
        }

        /// <summary>
        /// Reads bytes through the native stream-reader contract.
        /// 按 native 流读取器契约读取字节。
        /// </summary>
        public int Read(byte[] buffer, int offset, int count)
        {
            ThrowIfDisposed();
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }
            if (offset < 0 || count < 0 || offset > buffer.Length - count)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
            if (count == 0)
            {
                return 0;
            }

            GCHandle pinned = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                NativeException.ThrowIfError(NativeMethods.VideoStreamReaderRead(
                    NativeHandle,
                    IntPtr.Add(pinned.AddrOfPinnedObject(), offset),
                    count,
                    out long bytesRead));
                ThrowPendingException();
                if (bytesRead < 0)
                {
                    return 0;
                }
                return checked((int)Math.Min(bytesRead, count));
            }
            finally
            {
                pinned.Free();
            }
        }

        /// <summary>
        /// Seeks through the native stream-reader contract.
        /// 按 native 流读取器契约定位。
        /// </summary>
        public long Seek(long offset, SeekOrigin origin)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.VideoStreamReaderSeek(NativeHandle, offset, (int)origin, out long position));
            ThrowPendingException();
            return position;
        }

        /// <summary>
        /// Releases the native stream-reader wrapper.
        /// 释放 native 流读取器包装对象。
        /// </summary>
        public void Dispose()
        {
            if (!disposed)
            {
                handle.Dispose();
                disposed = true;
            }
            GC.SuppressFinalize(this);
        }

        private void ThrowPendingException()
        {
            Exception? exception = state.GetPendingException();
            if (exception != null)
            {
                throw new IOException("The managed video stream callback failed.", exception);
            }
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        private static long ReadCallback(IntPtr context, IntPtr buffer, long size)
        {
            ReaderState reader = GetState(context);
            try
            {
                if (size < 0 || size > int.MaxValue)
                {
                    throw new IOException("The native read size is outside the managed stream range.");
                }
                int count = checked((int)size);
                if (count == 0)
                {
                    return 0;
                }
                var bytes = new byte[count];
                int read = reader.Stream.Read(bytes, 0, count);
                Marshal.Copy(bytes, 0, buffer, read);
                return read;
            }
            catch (Exception exception)
            {
                reader.SetPendingException(exception);
                return -1;
            }
        }

        private static long SeekCallback(IntPtr context, long offset, int origin)
        {
            ReaderState reader = GetState(context);
            try
            {
                return reader.Stream.Seek(offset, (SeekOrigin)origin);
            }
            catch (Exception exception)
            {
                reader.SetPendingException(exception);
                return -1;
            }
        }

        private static void ReleaseCallback(IntPtr context)
        {
            GCHandle handle = GCHandle.FromIntPtr(context);
            if (!handle.IsAllocated)
            {
                return;
            }
            var reader = (ReaderState)handle.Target!;
            try
            {
                reader.Dispose();
            }
            catch (Exception exception)
            {
                reader.SetPendingException(exception);
            }
            finally
            {
                handle.Free();
            }
        }

        private static ReaderState GetState(IntPtr context)
        {
            return (ReaderState)GCHandle.FromIntPtr(context).Target!;
        }

        private sealed class ReaderState
        {
            private readonly object gate = new object();
            private Exception? pendingException;
            private readonly bool leaveOpen;

            internal ReaderState(Stream stream, bool leaveOpen)
            {
                Stream = stream;
                this.leaveOpen = leaveOpen;
            }

            internal Stream Stream { get; }

            internal void SetPendingException(Exception exception)
            {
                lock (gate)
                {
                    pendingException ??= exception;
                }
            }

            internal Exception? GetPendingException()
            {
                lock (gate)
                {
                    return pendingException;
                }
            }

            internal void Dispose()
            {
                if (!leaveOpen)
                {
                    Stream.Dispose();
                }
            }
        }
    }
}

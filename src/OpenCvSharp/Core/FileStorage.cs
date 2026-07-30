using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Core
{
    /// <summary>Reads and writes OpenCV XML, YAML, and JSON persistence data.</summary>
    public sealed class FileStorage : IDisposable
    {
        private readonly NativeFileStorageHandle handle;

        /// <summary>Creates a closed storage object.</summary>
        public FileStorage()
        {
            NativeException.ThrowIfError(NativeMethods.CoreFileStorageCreate(out IntPtr value));
            handle = NativeFileStorageHandle.FromNativePointer(value);
        }

        /// <summary>Creates a storage object and attempts to open the source.</summary>
        public FileStorage(string source, FileStorageModes flags, string? encoding = null)
            : this()
        {
            Open(source, flags, encoding);
        }

        /// <summary>Gets whether this managed wrapper has been disposed.</summary>
        public bool IsDisposed { get { return handle.IsClosed; } }

        /// <summary>Gets whether the native storage is open.</summary>
        public bool IsOpened
        {
            get
            {
                NativeException.ThrowIfError(NativeMethods.CoreFileStorageIsOpened(NativeHandle, out int opened));
                return opened != 0;
            }
        }

        /// <summary>Gets the detected or selected persistence format.</summary>
        public FileStorageModes GetFormat()
        {
            NativeException.ThrowIfError(NativeMethods.CoreFileStorageGetFormat(NativeHandle, out int format));
            return (FileStorageModes)format;
        }

        /// <summary>Opens a file path or an in-memory document/format hint.</summary>
        public bool Open(string source, FileStorageModes flags, string? encoding = null)
        {
            ValidateFlags(flags, nameof(flags));
            byte[] sourceUtf8 = CorePersistenceMarshal.Encode(source, nameof(source), true);
            byte[] encodingUtf8 = CorePersistenceMarshal.Encode(encoding ?? string.Empty, nameof(encoding), true);
            NativeException.ThrowIfError(NativeMethods.CoreFileStorageOpen(
                NativeHandle,
                sourceUtf8,
                sourceUtf8.Length,
                (int)flags,
                encodingUtf8,
                encodingUtf8.Length,
                out int opened));
            return opened != 0;
        }

        /// <summary>Releases the opened storage and invalidates nodes previously obtained from it.</summary>
        public void Release()
        {
            NativeException.ThrowIfError(NativeMethods.CoreFileStorageRelease(NativeHandle));
        }

        /// <summary>Releases memory-mode output and returns its UTF-8 document text.</summary>
        public string ReleaseAndGetString()
        {
            NativeException.ThrowIfError(NativeMethods.CoreFileStorageReleaseAndGetString(NativeHandle, out IntPtr value));
            return CorePersistenceMarshal.ReadUtf8Result(value);
        }

        /// <summary>Gets the first top-level node.</summary>
        public FileNode GetFirstTopLevelNode()
        {
            NativeException.ThrowIfError(NativeMethods.CoreFileStorageGetFirstTopLevelNode(NativeHandle, out IntPtr value));
            return new FileNode(value);
        }

        /// <summary>Gets a root node for a document stream.</summary>
        public FileNode Root(int streamIndex = 0)
        {
            if (streamIndex < 0) throw new ArgumentOutOfRangeException(nameof(streamIndex));
            NativeException.ThrowIfError(NativeMethods.CoreFileStorageRoot(NativeHandle, streamIndex, out IntPtr value));
            return new FileNode(value);
        }

        /// <summary>Gets a named node from any root stream.</summary>
        public FileNode this[string name]
        {
            get
            {
                byte[] nameUtf8 = CorePersistenceMarshal.Encode(name, nameof(name), false);
                NativeException.ThrowIfError(NativeMethods.CoreFileStorageGetNode(NativeHandle, nameUtf8, nameUtf8.Length, out IntPtr value));
                return new FileNode(value);
            }
        }

        /// <summary>Writes a named 32-bit integer value, or an unnamed sequence item.</summary>
        public void Write(string name, int value)
        {
            byte[] nameUtf8 = GetWriteName(name);
            EnsureOpened();
            NativeException.ThrowIfError(NativeMethods.CoreFileStorageWriteInt(NativeHandle, nameUtf8, nameUtf8.Length, value));
        }

        /// <summary>Writes a named Boolean value, or an unnamed sequence item.</summary>
        public void Write(string name, bool value)
        {
            byte[] nameUtf8 = GetWriteName(name);
            EnsureOpened();
            NativeException.ThrowIfError(NativeMethods.CoreFileStorageWriteBool(NativeHandle, nameUtf8, nameUtf8.Length, value ? 1 : 0));
        }

        /// <summary>Writes a named 64-bit integer value, or an unnamed sequence item.</summary>
        public void Write(string name, long value)
        {
            byte[] nameUtf8 = GetWriteName(name);
            EnsureOpened();
            NativeException.ThrowIfError(NativeMethods.CoreFileStorageWriteInt64(NativeHandle, nameUtf8, nameUtf8.Length, value));
        }

        /// <summary>Writes a named double-precision value, or an unnamed sequence item.</summary>
        public void Write(string name, double value)
        {
            byte[] nameUtf8 = GetWriteName(name);
            EnsureOpened();
            NativeException.ThrowIfError(NativeMethods.CoreFileStorageWriteDouble(NativeHandle, nameUtf8, nameUtf8.Length, value));
        }

        /// <summary>Writes a named UTF-8 string value, or an unnamed sequence item.</summary>
        public void Write(string name, string value)
        {
            byte[] nameUtf8 = GetWriteName(name);
            byte[] valueUtf8 = CorePersistenceMarshal.Encode(value, nameof(value), true);
            EnsureOpened();
            NativeException.ThrowIfError(NativeMethods.CoreFileStorageWriteString(NativeHandle, nameUtf8, nameUtf8.Length, valueUtf8, valueUtf8.Length));
        }

        /// <summary>Writes a named matrix value, or an unnamed sequence item.</summary>
        public void Write(string name, Mat value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            byte[] nameUtf8 = GetWriteName(name);
            EnsureOpened();
            NativeException.ThrowIfError(NativeMethods.CoreFileStorageWriteMat(NativeHandle, nameUtf8, nameUtf8.Length, value.NativeHandle));
        }

        /// <summary>Writes a named vector of UTF-8 strings, or an unnamed sequence item.</summary>
        public void Write(string name, IReadOnlyList<string> values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            byte[] nameUtf8 = GetWriteName(name);
            var encoded = new byte[values.Count][];
            int total = 0;
            for (int i = 0; i < values.Count; i++)
            {
                if (values[i] == null) throw new ArgumentException("String collections cannot contain null values.", nameof(values));
                encoded[i] = CorePersistenceMarshal.Encode(values[i], nameof(values), true);
                total = checked(total + encoded[i].Length);
            }

            var flattened = new byte[total];
            var offsets = new int[values.Count];
            var lengths = new int[values.Count];
            int offset = 0;
            for (int i = 0; i < encoded.Length; i++)
            {
                offsets[i] = offset;
                lengths[i] = encoded[i].Length;
                Buffer.BlockCopy(encoded[i], 0, flattened, offset, encoded[i].Length);
                offset += encoded[i].Length;
            }

            EnsureOpened();
            NativeException.ThrowIfError(NativeMethods.CoreFileStorageWriteStringVector(
                NativeHandle,
                nameUtf8,
                nameUtf8.Length,
                flattened,
                flattened.Length,
                offsets,
                lengths,
                values.Count));
        }

        /// <summary>Writes a format comment.</summary>
        public void WriteComment(string comment, bool append = false)
        {
            byte[] value = CorePersistenceMarshal.Encode(comment, nameof(comment), true);
            EnsureOpened();
            NativeException.ThrowIfError(NativeMethods.CoreFileStorageWriteComment(NativeHandle, value, value.Length, append ? 1 : 0));
        }

        /// <summary>Begins a named or sequence-item map/sequence structure.</summary>
        public void StartWriteStruct(string name, FileNodeTypes flags, string? typeName = null)
        {
            ValidateStructFlags(flags, nameof(flags));
            byte[] nameUtf8 = GetWriteName(name);
            byte[] typeNameUtf8 = CorePersistenceMarshal.Encode(typeName ?? string.Empty, nameof(typeName), true);
            EnsureOpened();
            NativeException.ThrowIfError(NativeMethods.CoreFileStorageStartWriteStruct(
                NativeHandle,
                nameUtf8,
                nameUtf8.Length,
                (int)flags,
                typeNameUtf8,
                typeNameUtf8.Length));
        }

        /// <summary>Ends the current structure.</summary>
        public void EndWriteStruct()
        {
            EnsureOpened();
            NativeException.ThrowIfError(NativeMethods.CoreFileStorageEndWriteStruct(NativeHandle));
        }

        internal IntPtr NativeHandle
        {
            get
            {
                if (handle.IsClosed || handle.IsInvalid) throw new ObjectDisposedException(nameof(FileStorage));
                return handle.DangerousGetHandle();
            }
        }

        /// <summary>Releases this managed wrapper's native storage handle.</summary>
        public void Dispose() { handle.Dispose(); }

        private byte[] GetWriteName(string name)
        {
            return CorePersistenceMarshal.Encode(name, nameof(name), true);
        }

        private void EnsureOpened()
        {
            if (!IsOpened) throw new InvalidOperationException("FileStorage is not open.");
        }

        private static void ValidateFlags(FileStorageModes flags, string parameterName)
        {
            const int knownMask = 3 | 4 | (7 << 3) | 64;
            int value = (int)flags;
            int operation = value & 3;
            int format = value & (7 << 3);
            if ((value & ~knownMask) != 0 || operation == 3 ||
                (format != 0 && format != 8 && format != 16 && format != 24 && format != 32))
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }

        private static void ValidateStructFlags(FileNodeTypes flags, string parameterName)
        {
            int value = (int)flags;
            int type = value & (int)FileNodeTypes.TypeMask;
            if ((value & ~((int)FileNodeTypes.TypeMask | (int)FileNodeTypes.Flow)) != 0 ||
                (type != (int)FileNodeTypes.Sequence && type != (int)FileNodeTypes.Map))
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }
    }

    internal static class CorePersistenceMarshal
    {
        private static readonly Encoding StrictUtf8 = new UTF8Encoding(false, true);

        internal static byte[] Encode(string value, string parameterName, bool allowEmpty)
        {
            if (value == null) throw new ArgumentNullException(parameterName);
            if (!allowEmpty && value.Length == 0) throw new ArgumentException("Value cannot be empty.", parameterName);
            if (value.IndexOf('\0') >= 0) throw new ArgumentException("Embedded null characters are not supported.", parameterName);
            try { return StrictUtf8.GetBytes(value); }
            catch (EncoderFallbackException exception) { throw new ArgumentException("Value is not valid UTF-16 text.", parameterName, exception); }
        }

        internal static string ReadUtf8Result(IntPtr value)
        {
            using (NativeCoreUtf8ResultHandle result = NativeCoreUtf8ResultHandle.FromNativePointer(value))
            {
                NativeException.ThrowIfError(NativeMethods.CoreUtf8ResultSize(result.DangerousGetHandle(), out UIntPtr sizeValue));
                ulong size = sizeValue.ToUInt64();
                if (size > int.MaxValue) throw new OpenCvException("Native UTF-8 result is larger than Int32.MaxValue.");
                NativeException.ThrowIfError(NativeMethods.CoreUtf8ResultData(result.DangerousGetHandle(), out IntPtr data));
                if (size == 0) return string.Empty;
                if (data == IntPtr.Zero) throw new OpenCvException("Native UTF-8 result data is null.");
                var bytes = new byte[(int)size];
                Marshal.Copy(data, bytes, 0, bytes.Length);
                try { return StrictUtf8.GetString(bytes); }
                catch (DecoderFallbackException exception) { throw new OpenCvException("Native text result is not valid UTF-8.", exception); }
            }
        }
    }
}

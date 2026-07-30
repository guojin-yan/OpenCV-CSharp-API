using System;
using System.Collections;
using System.Collections.Generic;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Core
{
    /// <summary>Represents an owned wrapper around a node borrowed from FileStorage state.</summary>
    public sealed class FileNode : IDisposable, IEnumerable<FileNode>
    {
        private readonly NativeFileNodeHandle handle;

        /// <summary>Creates an empty node.</summary>
        public FileNode()
        {
            NativeException.ThrowIfError(NativeMethods.CoreFileNodeCreate(out IntPtr value));
            handle = NativeFileNodeHandle.FromNativePointer(value);
        }

        internal FileNode(IntPtr value)
        {
            handle = NativeFileNodeHandle.FromNativePointer(value);
        }

        /// <summary>Gets whether this managed wrapper has been disposed.</summary>
        public bool IsDisposed { get { return handle.IsClosed; } }

        /// <summary>Gets whether this node is absent or contains no value.</summary>
        public bool Empty
        {
            get
            {
                NativeException.ThrowIfError(NativeMethods.CoreFileNodeEmpty(NativeHandle, out int value));
                return value != 0;
            }
        }

        /// <summary>Gets the scalar or collection type of this node.</summary>
        public FileNodeTypes Type
        {
            get
            {
                NativeException.ThrowIfError(NativeMethods.CoreFileNodeType(NativeHandle, out int value));
                return (FileNodeTypes)value;
            }
        }

        /// <summary>Gets whether this node is absent.</summary>
        public bool IsNone { get { return Type == FileNodeTypes.None; } }
        /// <summary>Gets whether this node contains a sequence.</summary>
        public bool IsSequence { get { return Type == FileNodeTypes.Sequence; } }
        /// <summary>Gets whether this node contains a map.</summary>
        public bool IsMap { get { return Type == FileNodeTypes.Map; } }
        /// <summary>Gets whether this node contains an integer.</summary>
        public bool IsInteger { get { return Type == FileNodeTypes.Integer; } }
        /// <summary>Gets whether this node contains a floating-point value.</summary>
        public bool IsReal { get { return Type == FileNodeTypes.Real; } }
        /// <summary>Gets whether this node contains a string.</summary>
        public bool IsString { get { return Type == FileNodeTypes.String; } }
        /// <summary>Gets whether this node has a name.</summary>
        public bool IsNamed { get { return Name.Length != 0; } }

        /// <summary>Gets this node's UTF-8 name, or an empty string for an unnamed node.</summary>
        public string Name
        {
            get
            {
                NativeException.ThrowIfError(NativeMethods.CoreFileNodeName(NativeHandle, out IntPtr value));
                return CorePersistenceMarshal.ReadUtf8Result(value);
            }
        }

        /// <summary>Gets the number of elements in a collection node.</summary>
        public int Size
        {
            get
            {
                NativeException.ThrowIfError(NativeMethods.CoreFileNodeSize(NativeHandle, out UIntPtr value));
                ulong size = value.ToUInt64();
                if (size > int.MaxValue) throw new OpenCvException("FileNode size is larger than Int32.MaxValue.");
                return (int)size;
            }
        }

        /// <summary>Gets OpenCV's raw serialized byte-size estimate for this node.</summary>
        public ulong RawSize
        {
            get
            {
                NativeException.ThrowIfError(NativeMethods.CoreFileNodeRawSize(NativeHandle, out UIntPtr value));
                return value.ToUInt64();
            }
        }

        /// <summary>Gets an integer or floating-point node as a double.</summary>
        public double Real
        {
            get
            {
                if (!IsInteger && !IsReal) throw new InvalidOperationException("FileNode does not contain a numeric value.");
                NativeException.ThrowIfError(NativeMethods.CoreFileNodeReal(NativeHandle, out double value));
                return value;
            }
        }

        /// <summary>Gets a string node as managed text.</summary>
        public string String
        {
            get
            {
                if (!IsString) throw new InvalidOperationException("FileNode does not contain a string value.");
                NativeException.ThrowIfError(NativeMethods.CoreFileNodeString(NativeHandle, out IntPtr value));
                return CorePersistenceMarshal.ReadUtf8Result(value);
            }
        }

        /// <summary>Gets the UTF-8 keys of a map node in storage order.</summary>
        public string[] Keys
        {
            get
            {
                if (!IsMap) throw new InvalidOperationException("FileNode is not a map.");
                NativeException.ThrowIfError(NativeMethods.CoreFileNodeKeys(NativeHandle, out IntPtr value));
                using (NativeCoreStringListHandle values = NativeCoreStringListHandle.FromNativePointer(value))
                {
                    NativeException.ThrowIfError(NativeMethods.CoreStringListCount(values.DangerousGetHandle(), out UIntPtr countValue));
                    ulong count = countValue.ToUInt64();
                    if (count > int.MaxValue) throw new OpenCvException("FileNode key count is larger than Int32.MaxValue.");
                    var result = new string[(int)count];
                    for (int i = 0; i < result.Length; i++)
                    {
                        NativeException.ThrowIfError(NativeMethods.CoreStringListGet(values.DangerousGetHandle(), (UIntPtr)(uint)i, out IntPtr item));
                        result[i] = CorePersistenceMarshal.ReadUtf8Result(item);
                    }
                    return result;
                }
            }
        }

        /// <summary>Gets a named child from a map node.</summary>
        public FileNode this[string name]
        {
            get
            {
                if (!IsMap) throw new InvalidOperationException("FileNode is not a map.");
                byte[] nameUtf8 = CorePersistenceMarshal.Encode(name, nameof(name), false);
                NativeException.ThrowIfError(NativeMethods.CoreFileNodeGetNode(NativeHandle, nameUtf8, nameUtf8.Length, out IntPtr value));
                return new FileNode(value);
            }
        }

        /// <summary>Gets a child from a sequence node by zero-based index.</summary>
        public FileNode this[int index]
        {
            get
            {
                if (!IsSequence) throw new InvalidOperationException("FileNode is not a sequence.");
                if (index < 0 || index >= Size) throw new ArgumentOutOfRangeException(nameof(index));
                NativeException.ThrowIfError(NativeMethods.CoreFileNodeAt(NativeHandle, index, out IntPtr value));
                return new FileNode(value);
            }
        }

        /// <summary>Reads this node as an independently owned Mat.</summary>
        public Mat ToMat()
        {
            var result = new Mat();
            try
            {
                NativeException.ThrowIfError(NativeMethods.CoreFileNodeMat(NativeHandle, result.NativeHandle));
                return result;
            }
            catch
            {
                result.Dispose();
                throw;
            }
        }

        /// <summary>Enumerates sequence children. Each returned node is independently disposable.</summary>
        public IEnumerator<FileNode> GetEnumerator()
        {
            if (!IsSequence) throw new InvalidOperationException("FileNode is not a sequence.");
            int count = Size;
            for (int i = 0; i < count; i++) yield return this[i];
        }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        internal IntPtr NativeHandle
        {
            get
            {
                if (handle.IsClosed || handle.IsInvalid) throw new ObjectDisposedException(nameof(FileNode));
                return handle.DangerousGetHandle();
            }
        }

        /// <summary>Releases this managed wrapper's native node handle.</summary>
        public void Dispose() { handle.Dispose(); }
    }
}

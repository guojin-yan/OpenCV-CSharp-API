using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.ImgCodecs
{
    /// <summary>Provides indexed lazy access to a multi-page image. 提供多页图像的索引式延迟访问。</summary>
    public sealed class ImageCollection : IDisposable
    {
        private readonly NativeImgCodecsImageCollectionHandle handle;

        /// <summary>Creates an uninitialized collection. 创建未初始化的集合。</summary>
        public ImageCollection()
        {
            NativeException.ThrowIfError(NativeMethods.ImgCodecsImageCollectionCreate(out IntPtr value));
            handle = NativeImgCodecsImageCollectionHandle.FromNativePointer(value);
        }

        /// <summary>Creates a lazy collection for a multi-page file. 为多页文件创建延迟集合。</summary>
        public ImageCollection(string filename, ImreadModes flags = ImreadModes.Color)
        {
            byte[] path = Cv2.ToNullTerminatedUtf8Value(filename, nameof(filename));
            Cv2.ValidateImreadModeValue(flags, nameof(flags));
            NativeException.ThrowIfError(NativeMethods.ImgCodecsImageCollectionCreateFile(path, (int)flags, out IntPtr value));
            handle = NativeImgCodecsImageCollectionHandle.FromNativePointer(value);
        }

        /// <summary>Gets whether the collection has been disposed. 获取集合是否已释放。</summary>
        public bool IsDisposed { get { return handle.IsClosed; } }

        /// <summary>Gets the number of pages. 获取页面数。</summary>
        public int Count
        {
            get
            {
                NativeException.ThrowIfError(NativeMethods.ImgCodecsImageCollectionSize(NativeHandle, out UIntPtr value));
                ulong count = value.ToUInt64();
                if (count > int.MaxValue) throw new OpenCvException("Image collection count is larger than Int32.MaxValue.");
                return (int)count;
            }
        }

        /// <summary>Gets an independently owned clone of a page. 获取页面的独立拥有克隆。</summary>
        public Mat this[int index]
        {
            get
            {
                if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
                NativeException.ThrowIfError(NativeMethods.ImgCodecsImageCollectionCloneAt(NativeHandle, index, out IntPtr image));
                return new Mat(image);
            }
        }

        /// <summary>Reinitializes the collection for another file. 使用另一个文件重新初始化集合。</summary>
        public void Initialize(string filename, ImreadModes flags = ImreadModes.Color)
        {
            byte[] path = Cv2.ToNullTerminatedUtf8Value(filename, nameof(filename));
            Cv2.ValidateImreadModeValue(flags, nameof(flags));
            NativeException.ThrowIfError(NativeMethods.ImgCodecsImageCollectionInit(NativeHandle, path, (int)flags));
        }

        /// <summary>Releases a decoded page from the native cache. 从原生缓存释放已解码页面。</summary>
        public void ReleaseCache(int index)
        {
            if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            NativeException.ThrowIfError(NativeMethods.ImgCodecsImageCollectionReleaseCache(NativeHandle, index));
        }

        internal IntPtr NativeHandle
        {
            get
            {
                if (handle.IsClosed || handle.IsInvalid) throw new ObjectDisposedException(nameof(ImageCollection));
                return handle.DangerousGetHandle();
            }
        }

        /// <inheritdoc/>
        public void Dispose() { handle.Dispose(); }
    }
}

using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Face
{
    /// <summary>
    /// Minimum Average Correlation Energy filter wrapper.
    /// MACE 最小平均相关能量滤波器包装。
    /// </summary>
    public sealed class MACE : IDisposable
    {
        private NativeFaceMaceHandle handle;
        private bool disposed;

        private MACE(IntPtr nativeHandle)
        {
            handle = NativeFaceMaceHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Gets whether this object has been disposed. 获取对象是否已经释放。</summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        /// <summary>Gets whether the native MACE filter is empty. 获取 native MACE 滤波器是否为空。</summary>
        public bool Empty
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.FaceMaceEmpty(NativeHandle, out int empty));
                return empty != 0;
            }
        }

        private IntPtr NativeHandle
        {
            get
            {
                ThrowIfDisposed();
                return handle.DangerousGetHandle();
            }
        }

        /// <summary>Creates a MACE filter. 创建 MACE 滤波器。</summary>
        public static MACE Create(int imageSize = 64)
        {
            NativeException.ThrowIfError(NativeMethods.FaceMaceCreate(imageSize, out IntPtr nativeHandle));
            return new MACE(nativeHandle);
        }

        /// <summary>Loads a serialized MACE filter. 加载序列化的 MACE 滤波器。</summary>
        public static MACE Load(string filename, string objname = "")
        {
            byte[] nativeFilename = FaceStringConvert.ToNullTerminatedUtf8(filename, nameof(filename));
            byte[] nativeObjname = FaceStringConvert.ToNullTerminatedUtf8(objname, nameof(objname));
            NativeException.ThrowIfError(NativeMethods.FaceMaceLoad(nativeFilename, nativeObjname, out IntPtr nativeHandle));
            return new MACE(nativeHandle);
        }

        /// <summary>Applies a passphrase-derived salt to the filter workflow. 对滤波工作流应用由 passphrase 派生的 salt。</summary>
        public void Salt(string passphrase)
        {
            ThrowIfDisposed();
            byte[] nativePassphrase = FaceStringConvert.ToNullTerminatedUtf8(passphrase, nameof(passphrase));
            NativeException.ThrowIfError(NativeMethods.FaceMaceSalt(NativeHandle, nativePassphrase));
        }

        /// <summary>Trains the filter from positive example images. 使用正样本图像训练滤波器。</summary>
        public void Train(params Mat[] images)
        {
            ThrowIfDisposed();
            ValidateImages(images, nameof(images));
            NativeException.ThrowIfError(NativeMethods.FaceMaceTrain(NativeHandle, FaceRecognizer.ToNativeHandles(images), images.Length));
        }

        /// <summary>Tests whether a query image belongs to the trained class. 测试查询图像是否属于已训练类别。</summary>
        public bool Same(Mat query)
        {
            ThrowIfDisposed();
            FaceRecognizer.ValidateNotNull(query, nameof(query));
            NativeException.ThrowIfError(NativeMethods.FaceMaceSame(NativeHandle, query.NativeHandle, out int same));
            return same != 0;
        }

        /// <summary>Saves this MACE filter to a file. 将 MACE 滤波器保存到文件。</summary>
        public void Save(string path)
        {
            ThrowIfDisposed();
            byte[] nativePath = FaceStringConvert.ToNullTerminatedUtf8(path, nameof(path));
            NativeException.ThrowIfError(NativeMethods.FaceMaceSave(NativeHandle, nativePath));
        }

        /// <summary>Releases native resources. 释放 native 资源。</summary>
        public void Dispose()
        {
            if (!disposed)
            {
                handle.Dispose();
                disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        private static void ValidateImages(Mat[] images, string parameterName)
        {
            if (images == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            if (images.Length == 0)
            {
                throw new ArgumentException("Image array cannot be empty.", parameterName);
            }

            for (int i = 0; i < images.Length; i++)
            {
                if (images[i] == null)
                {
                    throw new ArgumentNullException(parameterName);
                }
            }
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }
}

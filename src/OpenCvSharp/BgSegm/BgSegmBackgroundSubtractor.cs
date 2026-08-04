using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.BgSegm
{
    /// <summary>
    /// Base class for contrib bgsegm background subtractors.
    /// contrib bgsegm 背景减除器基类。
    /// </summary>
    public abstract class BgSegmBackgroundSubtractor : IDisposable
    {
        private NativeBgSegmBackgroundSubtractorHandle handle;
        private bool disposed;

        internal BgSegmBackgroundSubtractor(IntPtr nativeHandle)
        {
            handle = NativeBgSegmBackgroundSubtractorHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Gets whether this object has been disposed. 获取对象是否已经释放。</summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        internal IntPtr NativeHandle
        {
            get
            {
                ThrowIfDisposed();
                return handle.DangerousGetHandle();
            }
        }

        /// <summary>Computes a foreground mask. 计算前景掩码。</summary>
        public void Apply(Mat image, Mat fgmask, double learningRate = -1.0)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(fgmask, nameof(fgmask));
            ValidateApplyImage(image);
            NativeException.ThrowIfError(NativeMethods.BgSegmBackgroundSubtractorApply(NativeHandle, image.NativeHandle, fgmask.NativeHandle, learningRate));
        }

        /// <summary>Computes and returns a foreground mask. 计算并返回前景掩码。</summary>
        public Mat Apply(Mat image, double learningRate = -1.0)
        {
            var fgmask = new Mat();
            try
            {
                Apply(image, fgmask, learningRate);
                return fgmask;
            }
            catch
            {
                fgmask.Dispose();
                throw;
            }
        }

        /// <summary>Computes a foreground mask with a known foreground mask. 使用已知前景掩码计算前景掩码。</summary>
        public void Apply(Mat image, Mat knownForegroundMask, Mat fgmask, double learningRate = -1.0)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(knownForegroundMask, nameof(knownForegroundMask));
            ValidateNotNull(fgmask, nameof(fgmask));
            ValidateApplyImage(image);
            NativeException.ThrowIfError(NativeMethods.BgSegmBackgroundSubtractorApplyWithKnownForeground(NativeHandle, image.NativeHandle, knownForegroundMask.NativeHandle, fgmask.NativeHandle, learningRate));
        }

        /// <summary>Computes and returns a foreground mask with a known foreground mask. 使用已知前景掩码计算并返回前景掩码。</summary>
        public Mat ApplyWithKnownForeground(Mat image, Mat knownForegroundMask, double learningRate = -1.0)
        {
            var fgmask = new Mat();
            try
            {
                Apply(image, knownForegroundMask, fgmask, learningRate);
                return fgmask;
            }
            catch
            {
                fgmask.Dispose();
                throw;
            }
        }

        /// <summary>Writes the current background image estimate. 写入当前背景图像估计。</summary>
        public void GetBackgroundImage(Mat backgroundImage)
        {
            ThrowIfDisposed();
            ValidateNotNull(backgroundImage, nameof(backgroundImage));
            NativeException.ThrowIfError(NativeMethods.BgSegmBackgroundSubtractorGetBackgroundImage(NativeHandle, backgroundImage.NativeHandle));
        }

        /// <summary>Returns the current background image estimate. 返回当前背景图像估计。</summary>
        public Mat GetBackgroundImage()
        {
            var backgroundImage = new Mat();
            try
            {
                GetBackgroundImage(backgroundImage);
                return backgroundImage;
            }
            catch
            {
                backgroundImage.Dispose();
                throw;
            }
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

        internal static void ValidateNotNull<T>(T value, string parameterName)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        /// <summary>Allows concrete subtractors to validate image formats before native calls. 允许具体减除器在 native 调用前校验图像格式。</summary>
        protected virtual void ValidateApplyImage(Mat image)
        {
        }

        /// <summary>Throws when disposed. 已释放时抛出异常。</summary>
        protected void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }
}

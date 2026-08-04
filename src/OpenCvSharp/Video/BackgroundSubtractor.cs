using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Video
{
    /// <summary>
    /// Base class for OpenCV background-subtraction algorithms.
    /// OpenCV 背景减除算法的基类。
    /// </summary>
    public abstract class BackgroundSubtractor : IDisposable
    {
        private NativeBackgroundSubtractorHandle handle;
        private bool disposed;

        /// <summary>
        /// Initializes the managed wrapper from a native background subtractor handle.
        /// 使用 native 背景减除器句柄初始化 managed 包装对象。
        /// </summary>
        protected BackgroundSubtractor(IntPtr nativeHandle)
        {
            handle = NativeBackgroundSubtractorHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>
        /// Gets whether this subtractor has been disposed.
        /// 获取背景减除器是否已经释放。
        /// </summary>
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

        /// <summary>
        /// Computes a foreground mask for an input frame.
        /// 为输入帧计算前景掩码。
        /// </summary>
        public void Apply(Mat image, Mat fgmask, double learningRate = -1.0)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(fgmask, nameof(fgmask));
            NativeException.ThrowIfError(NativeMethods.BackgroundSubtractorApply(
                NativeHandle,
                image.NativeHandle,
                fgmask.NativeHandle,
                learningRate));
        }

        /// <summary>
        /// Computes and returns a foreground mask for an input frame.
        /// 为输入帧计算并返回前景掩码。
        /// </summary>
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

        /// <summary>
        /// Computes a foreground mask while supplying known foreground pixels.
        /// 在提供已知前景像素的情况下计算前景掩码。
        /// </summary>
        public void Apply(Mat image, Mat knownForegroundMask, Mat fgmask, double learningRate = -1.0)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(knownForegroundMask, nameof(knownForegroundMask));
            ValidateNotNull(fgmask, nameof(fgmask));
            NativeException.ThrowIfError(NativeMethods.BackgroundSubtractorApplyWithKnownForeground(
                NativeHandle,
                image.NativeHandle,
                knownForegroundMask.NativeHandle,
                fgmask.NativeHandle,
                learningRate));
        }

        /// <summary>
        /// Computes and returns a foreground mask while supplying known foreground pixels.
        /// 在提供已知前景像素的情况下计算并返回前景掩码。
        /// </summary>
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

        /// <summary>
        /// Writes the current background image estimate.
        /// 写入当前背景图像估计结果。
        /// </summary>
        public void GetBackgroundImage(Mat backgroundImage)
        {
            ThrowIfDisposed();
            ValidateNotNull(backgroundImage, nameof(backgroundImage));
            NativeException.ThrowIfError(NativeMethods.BackgroundSubtractorGetBackgroundImage(
                NativeHandle,
                backgroundImage.NativeHandle));
        }

        /// <summary>
        /// Returns the current background image estimate.
        /// 返回当前背景图像估计结果。
        /// </summary>
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

        /// <summary>
        /// Releases native resources.
        /// 释放 native 资源。
        /// </summary>
        public void Dispose()
        {
            if (!disposed)
            {
                handle.Dispose();
                disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Throws if this object has been disposed.
        /// 如果对象已经释放则抛出异常。
        /// </summary>
        protected void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        /// <summary>
        /// Validates that a reference is not null.
        /// 验证引用不为空。
        /// </summary>
        protected static void ValidateNotNull<T>(T value, string parameterName)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }
    }
}

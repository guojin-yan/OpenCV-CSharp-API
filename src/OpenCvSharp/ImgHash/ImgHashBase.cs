using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.ImgHash
{
    /// <summary>
    /// Base class for OpenCV image hash algorithms.
    /// OpenCV 图像哈希算法基类。
    /// </summary>
    public abstract class ImgHashBase : IDisposable
    {
        private NativeImgHashHandle handle;
        private bool disposed;

        internal ImgHashBase(IntPtr nativeHandle)
        {
            handle = NativeImgHashHandle.FromNativePointer(nativeHandle);
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

        /// <summary>
        /// Computes image hash into <paramref name="output"/>.
        /// 将图像哈希计算到 <paramref name="output"/>。
        /// </summary>
        public void Compute(Mat input, Mat output)
        {
            ThrowIfDisposed();
            ValidateNotNull(input, nameof(input));
            ValidateNotNull(output, nameof(output));
            ValidateInputImage(input, nameof(input));
            NativeException.ThrowIfError(NativeMethods.ImgHashCompute(NativeHandle, input.NativeHandle, output.NativeHandle));
        }

        /// <summary>
        /// Computes image hash as a new matrix.
        /// 计算图像哈希并返回新矩阵。
        /// </summary>
        public Mat Compute(Mat input)
        {
            var output = new Mat();
            try
            {
                Compute(input, output);
                return output;
            }
            catch
            {
                output.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Compares two hash matrices.
        /// 比较两个哈希矩阵。
        /// </summary>
        public double Compare(Mat hashOne, Mat hashTwo)
        {
            ThrowIfDisposed();
            ValidateNotNull(hashOne, nameof(hashOne));
            ValidateNotNull(hashTwo, nameof(hashTwo));
            NativeException.ThrowIfError(NativeMethods.ImgHashCompare(NativeHandle, hashOne.NativeHandle, hashTwo.NativeHandle, out double value));
            return value;
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

        internal static void ValidateInputImage(Mat input, string parameterName)
        {
            ValidateNotNull(input, parameterName);

            if (input.Empty)
            {
                throw new ArgumentException("Image hash input image must not be empty.", parameterName);
            }

            if (input.Type != MatType.CV_8UC1 &&
                input.Type != MatType.CV_8UC3 &&
                input.Type != MatType.CV_8UC4)
            {
                throw new ArgumentException("Image hash input image must be CV_8UC1, CV_8UC3, or CV_8UC4.", parameterName);
            }
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

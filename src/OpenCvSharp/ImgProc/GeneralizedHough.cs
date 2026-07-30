using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.ImgProc
{
    /// <summary>
    /// Finds an arbitrary template with OpenCV's generalized Hough transform.
    /// 使用 OpenCV 广义霍夫变换查找任意模板。
    /// </summary>
    public abstract class GeneralizedHough : IDisposable
    {
        private NativeGeneralizedHoughHandle handle;
        private bool disposed;

        internal GeneralizedHough(IntPtr nativeHandle)
        {
            handle = NativeGeneralizedHoughHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Gets whether this object has been disposed. 获取此对象是否已经释放。</summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        /// <summary>Gets or sets the low Canny threshold. 获取或设置 Canny 低阈值。</summary>
        public int CannyLowThreshold
        {
            get { return GetIntProperty(0); }
            set { ValidateNonNegative(value, nameof(value)); SetIntProperty(0, value); }
        }

        /// <summary>Gets or sets the high Canny threshold. 获取或设置 Canny 高阈值。</summary>
        public int CannyHighThreshold
        {
            get { return GetIntProperty(1); }
            set { ValidateNonNegative(value, nameof(value)); SetIntProperty(1, value); }
        }

        /// <summary>Gets or sets the minimum distance between detected centers. 获取或设置检测中心之间的最小距离。</summary>
        public double MinDistance
        {
            get { return GetDoubleProperty(0); }
            set { ValidateNonNegative(value, nameof(value)); SetDoubleProperty(0, value); }
        }

        /// <summary>Gets or sets the inverse accumulator resolution ratio. 获取或设置累加器分辨率的反比。</summary>
        public double Dp
        {
            get { return GetDoubleProperty(1); }
            set { ValidatePositive(value, nameof(value)); SetDoubleProperty(1, value); }
        }

        /// <summary>Gets or sets the maximum internal buffer size. 获取或设置内部缓冲区最大尺寸。</summary>
        public int MaxBufferSize
        {
            get { return GetIntProperty(2); }
            set { ValidatePositive(value, nameof(value)); SetIntProperty(2, value); }
        }

        internal IntPtr NativeHandle
        {
            get
            {
                ThrowIfDisposed();
                return handle.DangerousGetHandle();
            }
        }

        /// <summary>Sets the grayscale template. 设置灰度模板。</summary>
        public void SetTemplate(Mat template, Point? templateCenter = null)
        {
            ThrowIfDisposed();
            ValidateNotNull(template, nameof(template));
            Point center = templateCenter ?? new Point(-1, -1);
            NativeException.ThrowIfError(NativeMethods.ImgProcGeneralizedHoughSetTemplate(
                NativeHandle,
                template.NativeHandle,
                center.X,
                center.Y));
        }

        /// <summary>Sets a template from precomputed edges and derivatives. 使用预计算边缘和导数设置模板。</summary>
        public void SetTemplate(Mat edges, Mat dx, Mat dy, Point? templateCenter = null)
        {
            ThrowIfDisposed();
            ValidateNotNull(edges, nameof(edges));
            ValidateNotNull(dx, nameof(dx));
            ValidateNotNull(dy, nameof(dy));
            Point center = templateCenter ?? new Point(-1, -1);
            NativeException.ThrowIfError(NativeMethods.ImgProcGeneralizedHoughSetTemplateEdges(
                NativeHandle,
                edges.NativeHandle,
                dx.NativeHandle,
                dy.NativeHandle,
                center.X,
                center.Y));
        }

        /// <summary>Detects template positions in an image. 在图像中检测模板位置。</summary>
        public void Detect(Mat image, Mat positions, Mat? votes = null)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(positions, nameof(positions));
            NativeException.ThrowIfError(NativeMethods.ImgProcGeneralizedHoughDetect(
                NativeHandle,
                image.NativeHandle,
                positions.NativeHandle,
                votes == null ? IntPtr.Zero : votes.NativeHandle));
        }

        /// <summary>Detects template positions and returns the position matrix. 检测模板位置并返回位置矩阵。</summary>
        public Mat Detect(Mat image)
        {
            var positions = new Mat();
            Detect(image, positions);
            return positions;
        }

        /// <summary>Detects positions from precomputed edges and derivatives. 使用预计算边缘和导数检测位置。</summary>
        public void Detect(Mat edges, Mat dx, Mat dy, Mat positions, Mat? votes = null)
        {
            ThrowIfDisposed();
            ValidateNotNull(edges, nameof(edges));
            ValidateNotNull(dx, nameof(dx));
            ValidateNotNull(dy, nameof(dy));
            ValidateNotNull(positions, nameof(positions));
            NativeException.ThrowIfError(NativeMethods.ImgProcGeneralizedHoughDetectEdges(
                NativeHandle,
                edges.NativeHandle,
                dx.NativeHandle,
                dy.NativeHandle,
                positions.NativeHandle,
                votes == null ? IntPtr.Zero : votes.NativeHandle));
        }

        /// <summary>Detects from precomputed edges and returns the position matrix. 使用预计算边缘检测并返回位置矩阵。</summary>
        public Mat DetectEdges(Mat edges, Mat dx, Mat dy)
        {
            var positions = new Mat();
            Detect(edges, dx, dy, positions);
            return positions;
        }

        /// <summary>Releases the native algorithm object. 释放 native 算法对象。</summary>
        public void Dispose()
        {
            if (!disposed)
            {
                handle.Dispose();
                disposed = true;
            }
            GC.SuppressFinalize(this);
        }

        private protected int GetIntProperty(int property)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.ImgProcGeneralizedHoughGetIntProperty(NativeHandle, property, out int value));
            return value;
        }

        private protected void SetIntProperty(int property, int value)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.ImgProcGeneralizedHoughSetIntProperty(NativeHandle, property, value));
        }

        private protected double GetDoubleProperty(int property)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.ImgProcGeneralizedHoughGetDoubleProperty(NativeHandle, property, out double value));
            return value;
        }

        private protected void SetDoubleProperty(int property, double value)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.ImgProcGeneralizedHoughSetDoubleProperty(NativeHandle, property, value));
        }

        private protected static void ValidatePositive(int value, string parameterName)
        {
            if (value <= 0) throw new ArgumentOutOfRangeException(parameterName, "Value must be positive.");
        }

        private protected static void ValidatePositive(double value, string parameterName)
        {
            if (value <= 0 || double.IsNaN(value) || double.IsInfinity(value)) throw new ArgumentOutOfRangeException(parameterName, "Value must be finite and positive.");
        }

        private protected static void ValidateNonNegative(int value, string parameterName)
        {
            if (value < 0) throw new ArgumentOutOfRangeException(parameterName, "Value cannot be negative.");
        }

        private protected static void ValidateNonNegative(double value, string parameterName)
        {
            if (value < 0 || double.IsNaN(value) || double.IsInfinity(value)) throw new ArgumentOutOfRangeException(parameterName, "Value must be finite and non-negative.");
        }

        private static void ValidateNotNull<T>(T value, string parameterName) where T : class
        {
            if (value == null) throw new ArgumentNullException(parameterName);
        }

        private void ThrowIfDisposed()
        {
            if (disposed) throw new ObjectDisposedException(GetType().FullName);
        }
    }
}

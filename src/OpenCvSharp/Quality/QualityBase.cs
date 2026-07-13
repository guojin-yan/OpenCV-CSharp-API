using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Quality
{
    /// <summary>
    /// Base class for OpenCV quality metrics.
    /// OpenCV 图像质量指标基类。
    /// </summary>
    public abstract class QualityBase : IDisposable
    {
        private NativeQualityHandle handle;
        private bool disposed;

        internal QualityBase(NativeQualityHandle handle)
        {
            this.handle = handle;
        }

        /// <summary>Gets whether this quality object has been disposed. 获取质量对象是否已经释放。</summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        /// <summary>Gets whether the native object is empty. 获取 native 对象是否为空。</summary>
        public bool Empty
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.QualityEmpty(NativeHandle, out int empty));
                return empty != 0;
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

        /// <summary>
        /// Computes the quality score for a comparison image.
        /// 计算比较图像的质量分数。
        /// </summary>
        public Scalar Compute(Mat comparison)
        {
            ThrowIfDisposed();
            ValidateNotNull(comparison, nameof(comparison));
            ValidateComputeInput(comparison, nameof(comparison));
            var values = new double[4];
            NativeException.ThrowIfError(NativeMethods.QualityCompute(NativeHandle, comparison.NativeHandle, values, values.Length));
            return ToScalar(values);
        }

        /// <summary>
        /// Writes the most recent quality map into <paramref name="qualityMap"/>.
        /// 将最近一次质量图写入 <paramref name="qualityMap"/>。
        /// </summary>
        public void GetQualityMap(Mat qualityMap)
        {
            ThrowIfDisposed();
            ValidateNotNull(qualityMap, nameof(qualityMap));
            NativeException.ThrowIfError(NativeMethods.QualityGetQualityMap(NativeHandle, qualityMap.NativeHandle));
        }

        /// <summary>
        /// Gets the most recent quality map as a new matrix.
        /// 以新矩阵获取最近一次质量图。
        /// </summary>
        public Mat GetQualityMap()
        {
            var qualityMap = new Mat();
            try
            {
                GetQualityMap(qualityMap);
                return qualityMap;
            }
            catch
            {
                qualityMap.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Clears internal native state.
        /// 清除 native 内部状态。
        /// </summary>
        public void Clear()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.QualityClear(NativeHandle));
        }

        /// <summary>Releases native resources. 释放 native 资源。</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Releases managed and native resources. 释放托管和 native 资源。</summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing && handle != null)
            {
                handle.Dispose();
            }

            disposed = true;
        }

        /// <summary>Throws when the object has been disposed. 对象已释放时抛出异常。</summary>
        protected void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        /// <summary>Validates metric-specific comparison input. 校验指标特定的比较图像输入。</summary>
        protected virtual void ValidateComputeInput(Mat comparison, string parameterName)
        {
        }

        internal static Scalar ToScalar(double[] values)
        {
            return new Scalar(values[0], values[1], values[2], values[3]);
        }

        internal static void ValidateNotNull<T>(T value, string parameterName)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }
    }
}

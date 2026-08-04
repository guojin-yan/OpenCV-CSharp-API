using System;
using System.Globalization;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.ML
{
    /// <summary>
    /// Represents an OpenCV ML parameter grid.
    /// 表示 OpenCV ML 参数网格。
    /// </summary>
    public sealed class ParamGrid : IDisposable
    {
        private NativeMlParamGridHandle handle;
        private bool disposed;

        /// <summary>
        /// Initializes a parameter grid.
        /// 初始化参数网格。
        /// </summary>
        public ParamGrid(double minVal = 0.0, double maxVal = 0.0, double logStep = 1.0)
        {
            NativeException.ThrowIfError(NativeMethods.MlParamGridCreate(minVal, maxVal, logStep, out IntPtr nativeHandle));
            handle = NativeMlParamGridHandle.FromNativePointer(nativeHandle);
        }

        internal ParamGrid(IntPtr nativeHandle)
        {
            handle = NativeMlParamGridHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Gets whether the grid has been disposed. 获取网格是否已释放。</summary>
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

        /// <summary>Gets or sets the minimum value. 获取或设置最小值。</summary>
        public double MinVal
        {
            get
            {
                GetValues(out double minVal, out _, out _);
                return minVal;
            }

            set
            {
                GetValues(out _, out double maxVal, out double logStep);
                SetValues(value, maxVal, logStep);
            }
        }

        /// <summary>Gets or sets the maximum value. 获取或设置最大值。</summary>
        public double MaxVal
        {
            get
            {
                GetValues(out _, out double maxVal, out _);
                return maxVal;
            }

            set
            {
                GetValues(out double minVal, out _, out double logStep);
                SetValues(minVal, value, logStep);
            }
        }

        /// <summary>Gets or sets the logarithmic step. 获取或设置对数步长。</summary>
        public double LogStep
        {
            get
            {
                GetValues(out _, out _, out double logStep);
                return logStep;
            }

            set
            {
                GetValues(out double minVal, out double maxVal, out _);
                SetValues(minVal, maxVal, value);
            }
        }

        /// <summary>
        /// Gets all grid values.
        /// 获取全部网格值。
        /// </summary>
        public void GetValues(out double minVal, out double maxVal, out double logStep)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MlParamGridGet(NativeHandle, out minVal, out maxVal, out logStep));
        }

        /// <summary>
        /// Sets all grid values.
        /// 设置全部网格值。
        /// </summary>
        public void SetValues(double minVal, double maxVal, double logStep)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MlParamGridSet(NativeHandle, minVal, maxVal, logStep));
        }

        /// <summary>
        /// Returns a diagnostic string for the parameter grid.
        /// 返回参数网格的诊断字符串。
        /// </summary>
        public override string ToString()
        {
            return disposed
                ? "{Disposed=True}"
                : "{MinVal=" + MinVal.ToString(CultureInfo.InvariantCulture)
                    + ",MaxVal=" + MaxVal.ToString(CultureInfo.InvariantCulture)
                    + ",LogStep=" + LogStep.ToString(CultureInfo.InvariantCulture) + "}";
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

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(ParamGrid));
            }
        }
    }
}

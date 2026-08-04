using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Plot
{
    /// <summary>
    /// Wraps OpenCV's two-dimensional plot renderer.
    /// 封装 OpenCV 的二维曲线绘制器。
    /// </summary>
    public sealed class Plot2d : IDisposable
    {
        private NativePlot2dHandle handle;
        private bool disposed;

        private Plot2d(NativePlot2dHandle handle)
        {
            this.handle = handle;
        }

        /// <summary>Gets whether this plot object has been disposed. 获取对象是否已经释放。</summary>
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
        /// Creates a plot from a vector of Y values.
        /// 从 Y 值向量创建曲线图。
        /// </summary>
        public static Plot2d Create(Mat data)
        {
            ValidateNotNull(data, nameof(data));
            NativeException.ThrowIfError(NativeMethods.Plot2dCreate(data.NativeHandle, out IntPtr nativeHandle));
            return new Plot2d(NativePlot2dHandle.FromNativePointer(nativeHandle));
        }

        /// <summary>
        /// Creates a plot from vectors of X and Y values.
        /// 从 X 与 Y 值向量创建曲线图。
        /// </summary>
        public static Plot2d Create(Mat dataX, Mat dataY)
        {
            ValidateNotNull(dataX, nameof(dataX));
            ValidateNotNull(dataY, nameof(dataY));
            NativeException.ThrowIfError(NativeMethods.Plot2dCreateXY(dataX.NativeHandle, dataY.NativeHandle, out IntPtr nativeHandle));
            return new Plot2d(NativePlot2dHandle.FromNativePointer(nativeHandle));
        }

        /// <summary>Sets the minimum X value shown by the plot. 设置绘图显示的最小 X 值。</summary>
        public Plot2d SetMinX(double value)
        {
            return SetDouble(value, NativeMethods.Plot2dSetMinX);
        }

        /// <summary>Sets the minimum Y value shown by the plot. 设置绘图显示的最小 Y 值。</summary>
        public Plot2d SetMinY(double value)
        {
            return SetDouble(value, NativeMethods.Plot2dSetMinY);
        }

        /// <summary>Sets the maximum X value shown by the plot. 设置绘图显示的最大 X 值。</summary>
        public Plot2d SetMaxX(double value)
        {
            return SetDouble(value, NativeMethods.Plot2dSetMaxX);
        }

        /// <summary>Sets the maximum Y value shown by the plot. 设置绘图显示的最大 Y 值。</summary>
        public Plot2d SetMaxY(double value)
        {
            return SetDouble(value, NativeMethods.Plot2dSetMaxY);
        }

        /// <summary>Sets the plot line width in pixels. 设置曲线线宽。</summary>
        public Plot2d SetPlotLineWidth(int value)
        {
            return SetInt(value, NativeMethods.Plot2dSetPlotLineWidth);
        }

        /// <summary>Sets whether neighboring plot points are connected by a line. 设置是否用线连接相邻点。</summary>
        public Plot2d SetNeedPlotLine(bool value)
        {
            return SetBool(value, NativeMethods.Plot2dSetNeedPlotLine);
        }

        /// <summary>Sets the curve color. 设置曲线颜色。</summary>
        public Plot2d SetPlotLineColor(Scalar value)
        {
            return SetScalar(value, NativeMethods.Plot2dSetPlotLineColor);
        }

        /// <summary>Sets the plot background color. 设置绘图区背景颜色。</summary>
        public Plot2d SetPlotBackgroundColor(Scalar value)
        {
            return SetScalar(value, NativeMethods.Plot2dSetPlotBackgroundColor);
        }

        /// <summary>Sets the plot axis color. 设置坐标轴颜色。</summary>
        public Plot2d SetPlotAxisColor(Scalar value)
        {
            return SetScalar(value, NativeMethods.Plot2dSetPlotAxisColor);
        }

        /// <summary>Sets the plot grid color. 设置网格颜色。</summary>
        public Plot2d SetPlotGridColor(Scalar value)
        {
            return SetScalar(value, NativeMethods.Plot2dSetPlotGridColor);
        }

        /// <summary>Sets the plot text color. 设置文本颜色。</summary>
        public Plot2d SetPlotTextColor(Scalar value)
        {
            return SetScalar(value, NativeMethods.Plot2dSetPlotTextColor);
        }

        /// <summary>Sets the rendered plot image size. 设置输出绘图图像尺寸。</summary>
        public Plot2d SetPlotSize(int width, int height)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.Plot2dSetPlotSize(NativeHandle, width, height));
            return this;
        }

        /// <summary>Sets whether grid lines are shown. 设置是否显示网格。</summary>
        public Plot2d SetShowGrid(bool value)
        {
            return SetBool(value, NativeMethods.Plot2dSetShowGrid);
        }

        /// <summary>Sets whether text labels are shown. 设置是否显示文本。</summary>
        public Plot2d SetShowText(bool value)
        {
            return SetBool(value, NativeMethods.Plot2dSetShowText);
        }

        /// <summary>Sets the number of grid lines. 设置网格线数量。</summary>
        public Plot2d SetGridLinesNumber(int value)
        {
            return SetInt(value, NativeMethods.Plot2dSetGridLinesNumber);
        }

        /// <summary>Sets whether the plot orientation is inverted. 设置是否反转绘图方向。</summary>
        public Plot2d SetInvertOrientation(bool value)
        {
            return SetBool(value, NativeMethods.Plot2dSetInvertOrientation);
        }

        /// <summary>Sets the point index printed in the text overlay. 设置要在文本层显示坐标的点索引。</summary>
        public Plot2d SetPointIdxToPrint(int value)
        {
            return SetInt(value, NativeMethods.Plot2dSetPointIdxToPrint);
        }

        /// <summary>
        /// Renders the plot into <paramref name="result"/>.
        /// 将曲线图绘制到 <paramref name="result"/>。
        /// </summary>
        public void Render(Mat result)
        {
            ThrowIfDisposed();
            ValidateNotNull(result, nameof(result));
            NativeException.ThrowIfError(NativeMethods.Plot2dRender(NativeHandle, result.NativeHandle));
        }

        /// <summary>
        /// Renders the plot as a new matrix.
        /// 将曲线图绘制为新矩阵。
        /// </summary>
        public Mat Render()
        {
            var result = new Mat();
            try
            {
                Render(result);
                return result;
            }
            catch
            {
                result.Dispose();
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

        private delegate int NativeSetDouble(IntPtr plot, double value);

        private delegate int NativeSetInt(IntPtr plot, int value);

        private delegate int NativeSetScalar(IntPtr plot, double v0, double v1, double v2, double v3);

        private Plot2d SetDouble(double value, NativeSetDouble setter)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(setter(NativeHandle, value));
            return this;
        }

        private Plot2d SetInt(int value, NativeSetInt setter)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(setter(NativeHandle, value));
            return this;
        }

        private Plot2d SetBool(bool value, NativeSetInt setter)
        {
            return SetInt(value ? 1 : 0, setter);
        }

        private Plot2d SetScalar(Scalar value, NativeSetScalar setter)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(setter(NativeHandle, value.V0, value.V1, value.V2, value.V3));
            return this;
        }

        private static void ValidateNotNull<T>(T value, string parameterName)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
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

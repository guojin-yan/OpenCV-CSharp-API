using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.XObjDetect
{
    /// <summary>
    /// Contrib HOG descriptor from OpenCV <c>xobjdetect</c>.
    /// OpenCV <c>xobjdetect</c> contrib HOG 描述子。
    /// </summary>
    public sealed class HOGDescriptor : IDisposable
    {
        private NativeHOGDescriptorHandle handle;
        private bool disposed;

        /// <summary>Initializes a HOG descriptor with OpenCV defaults. 使用 OpenCV 默认参数初始化 HOG 描述子。</summary>
        public HOGDescriptor()
        {
            NativeException.ThrowIfError(NativeMethods.HOGDescriptorCreate(out IntPtr nativeHandle));
            handle = NativeHOGDescriptorHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Initializes a HOG descriptor with full parameter set. 使用完整参数初始化 HOG 描述子。</summary>
        public HOGDescriptor(
            Size winSize,
            Size blockSize,
            Size blockStride,
            Size cellSize,
            int nbins,
            int derivAperture = 1,
            double winSigma = -1,
            HOGDescriptorHistogramNormType histogramNormType = HOGDescriptorHistogramNormType.L2Hys,
            double l2HysThreshold = 0.2,
            bool gammaCorrection = true,
            int nlevels = 64,
            bool signedGradient = false)
        {
            NativeException.ThrowIfError(NativeMethods.HOGDescriptorCreateWithParams(
                winSize.Width,
                winSize.Height,
                blockSize.Width,
                blockSize.Height,
                blockStride.Width,
                blockStride.Height,
                cellSize.Width,
                cellSize.Height,
                nbins,
                derivAperture,
                winSigma,
                (int)histogramNormType,
                l2HysThreshold,
                gammaCorrection ? 1 : 0,
                nlevels,
                signedGradient ? 1 : 0,
                out IntPtr nativeHandle));
            handle = NativeHOGDescriptorHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Initializes a HOG descriptor from a file. 从文件初始化 HOG 描述子。</summary>
        public HOGDescriptor(string filename)
        {
            byte[] path = XObjDetectStringConvert.ToNullTerminatedUtf8(filename, nameof(filename));
            NativeException.ThrowIfError(NativeMethods.HOGDescriptorCreateFromFile(path, out IntPtr nativeHandle));
            handle = NativeHOGDescriptorHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Gets whether this descriptor has been disposed. 获取描述子是否已经释放。</summary>
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

        /// <summary>Gets or sets the detection window size. 获取或设置检测窗口尺寸。</summary>
        public Size WinSize
        {
            get { return new Size((int)GetProperty(0), (int)GetProperty(1)); }
            set { SetProperty(0, value.Width); SetProperty(1, value.Height); }
        }

        /// <summary>Gets or sets the block size. 获取或设置 block 尺寸。</summary>
        public Size BlockSize
        {
            get { return new Size((int)GetProperty(2), (int)GetProperty(3)); }
            set { SetProperty(2, value.Width); SetProperty(3, value.Height); }
        }

        /// <summary>Gets or sets the block stride. 获取或设置 block 步长。</summary>
        public Size BlockStride
        {
            get { return new Size((int)GetProperty(4), (int)GetProperty(5)); }
            set { SetProperty(4, value.Width); SetProperty(5, value.Height); }
        }

        /// <summary>Gets or sets the cell size. 获取或设置 cell 尺寸。</summary>
        public Size CellSize
        {
            get { return new Size((int)GetProperty(6), (int)GetProperty(7)); }
            set { SetProperty(6, value.Width); SetProperty(7, value.Height); }
        }

        /// <summary>Gets or sets the histogram bin count. 获取或设置直方图 bin 数。</summary>
        public int NBins
        {
            get { return (int)GetProperty(8); }
            set { SetProperty(8, value); }
        }

        /// <summary>Gets or sets derivative aperture. 获取或设置导数 aperture。</summary>
        public int DerivAperture
        {
            get { return (int)GetProperty(9); }
            set { SetProperty(9, value); }
        }

        /// <summary>Gets or sets configured window sigma. 获取或设置配置的窗口 sigma。</summary>
        public double WinSigma
        {
            get { return GetProperty(10); }
            set { SetProperty(10, value); }
        }

        /// <summary>Gets or sets histogram normalization type. 获取或设置直方图归一化类型。</summary>
        public HOGDescriptorHistogramNormType HistogramNormType
        {
            get { return (HOGDescriptorHistogramNormType)(int)GetProperty(11); }
            set { SetProperty(11, (int)value); }
        }

        /// <summary>Gets or sets L2-Hys threshold. 获取或设置 L2-Hys 阈值。</summary>
        public double L2HysThreshold
        {
            get { return GetProperty(12); }
            set { SetProperty(12, value); }
        }

        /// <summary>Gets or sets gamma correction flag. 获取或设置 gamma 校正标志。</summary>
        public bool GammaCorrection
        {
            get { return GetProperty(13) != 0.0; }
            set { SetProperty(13, value ? 1.0 : 0.0); }
        }

        /// <summary>Gets or sets the maximum number of scale levels. 获取或设置最大尺度层数。</summary>
        public int NLevels
        {
            get { return (int)GetProperty(14); }
            set { SetProperty(14, value); }
        }

        /// <summary>Gets or sets whether signed gradients are used. 获取或设置是否使用有符号梯度。</summary>
        public bool SignedGradient
        {
            get { return GetProperty(15) != 0.0; }
            set { SetProperty(15, value ? 1.0 : 0.0); }
        }

        /// <summary>Gets the default people detector coefficients. 获取默认行人检测器系数。</summary>
        public static float[] GetDefaultPeopleDetector()
        {
            return GetDetectorVector(NativeMethods.HOGDescriptorGetDefaultPeopleDetectorCount, NativeMethods.HOGDescriptorGetDefaultPeopleDetectorFill);
        }

        /// <summary>Gets the Daimler people detector coefficients. 获取 Daimler 行人检测器系数。</summary>
        public static float[] GetDaimlerPeopleDetector()
        {
            return GetDetectorVector(NativeMethods.HOGDescriptorGetDaimlerPeopleDetectorCount, NativeMethods.HOGDescriptorGetDaimlerPeopleDetectorFill);
        }

        /// <summary>Sets SVM detector coefficients. 设置 SVM 检测器系数。</summary>
        public void SetSVMDetector(float[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.HOGDescriptorSetSVMDetector(NativeHandle, values, values.Length));
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>Sets SVM detector coefficients from a span. 从 Span 设置 SVM 检测器系数。</summary>
        public unsafe void SetSVMDetector(ReadOnlySpan<float> values)
        {
            ThrowIfDisposed();
            fixed (float* valuePtr = values)
            {
                NativeException.ThrowIfError(NativeMethods.HOGDescriptorSetSVMDetector(NativeHandle, valuePtr, values.Length));
            }
        }
#endif

        /// <summary>Checks whether detector vector size matches descriptor size. 检查检测器向量尺寸是否匹配描述子尺寸。</summary>
        public bool CheckDetectorSize()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.HOGDescriptorCheckDetectorSize(NativeHandle, out int result));
            return result != 0;
        }

        /// <summary>Gets descriptor size. 获取描述子尺寸。</summary>
        public UIntPtr GetDescriptorSize()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.HOGDescriptorGetDescriptorSize(NativeHandle, out UIntPtr descriptorSize));
            return descriptorSize;
        }

        /// <summary>Gets computed window sigma. 获取计算后的窗口 sigma。</summary>
        public double GetWinSigma()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.HOGDescriptorGetWinSigma(NativeHandle, out double winSigma));
            return winSigma;
        }

        /// <summary>Runs single-scale HOG detection. 运行单尺度 HOG 检测。</summary>
        public HOGDetectionResult Detect(Mat image, double hitThreshold = 0, Size winStride = default, Size padding = default)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            NativeException.ThrowIfError(NativeMethods.HOGDescriptorDetectCount(NativeHandle, image.NativeHandle, hitThreshold, winStride.Width, winStride.Height, padding.Width, padding.Height, out int count));
            if (count <= 0)
            {
                return new HOGDetectionResult(Array.Empty<Point>(), Array.Empty<double>());
            }

            var raw = new int[count * 2];
            var weights = new double[count];
            NativeException.ThrowIfError(NativeMethods.HOGDescriptorDetectFill(NativeHandle, image.NativeHandle, hitThreshold, winStride.Width, winStride.Height, padding.Width, padding.Height, raw, raw.Length, weights, weights.Length, out int written));
            int resultCount = Math.Min(written, count);
            return new HOGDetectionResult(ToPoints(raw, resultCount), Trim(weights, resultCount));
        }

        /// <summary>Runs multi-scale HOG detection. 运行多尺度 HOG 检测。</summary>
        public HOGDetectionResult DetectMultiScale(Mat image, double hitThreshold = 0, Size winStride = default, Size padding = default, double scale = 1.05, double groupThreshold = 2.0, bool useMeanshiftGrouping = false)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            NativeException.ThrowIfError(NativeMethods.HOGDescriptorDetectMultiScaleCount(NativeHandle, image.NativeHandle, hitThreshold, winStride.Width, winStride.Height, padding.Width, padding.Height, scale, groupThreshold, useMeanshiftGrouping ? 1 : 0, out int count));
            if (count <= 0)
            {
                return new HOGDetectionResult(Array.Empty<Rect>(), Array.Empty<double>());
            }

            var raw = new int[count * 4];
            var weights = new double[count];
            NativeException.ThrowIfError(NativeMethods.HOGDescriptorDetectMultiScaleFill(NativeHandle, image.NativeHandle, hitThreshold, winStride.Width, winStride.Height, padding.Width, padding.Height, scale, groupThreshold, useMeanshiftGrouping ? 1 : 0, raw, raw.Length, weights, weights.Length, out int written));
            int resultCount = Math.Min(written, count);
            return new HOGDetectionResult(ToRectangles(raw, resultCount), Trim(weights, resultCount));
        }

        /// <summary>Releases the native descriptor. 释放 native 描述子。</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private delegate int DetectorCountGetter(out int count);

        private delegate int DetectorFillMethod(float[] values, int valueCapacity, out int count);

        private static float[] GetDetectorVector(DetectorCountGetter countGetter, DetectorFillMethod fill)
        {
            NativeException.ThrowIfError(countGetter(out int count));
            if (count <= 0)
            {
                return Array.Empty<float>();
            }

            var result = new float[count];
            NativeException.ThrowIfError(fill(result, result.Length, out int written));
            if (written != result.Length)
            {
                Array.Resize(ref result, Math.Max(0, Math.Min(written, result.Length)));
            }

            return result;
        }

        private double GetProperty(int propertyId)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.HOGDescriptorGetProperty(NativeHandle, propertyId, out double value));
            return value;
        }

        private void SetProperty(int propertyId, double value)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.HOGDescriptorSetProperty(NativeHandle, propertyId, value));
        }

        private static Point[] ToPoints(int[] raw, int count)
        {
            if (count <= 0)
            {
                return Array.Empty<Point>();
            }

            var result = new Point[count];
            for (int i = 0; i < result.Length; i++)
            {
                int offset = i * 2;
                result[i] = new Point(raw[offset], raw[offset + 1]);
            }

            return result;
        }

        private static Rect[] ToRectangles(int[] raw, int count)
        {
            if (count <= 0)
            {
                return Array.Empty<Rect>();
            }

            var result = new Rect[count];
            for (int i = 0; i < result.Length; i++)
            {
                int offset = i * 4;
                result[i] = new Rect(raw[offset], raw[offset + 1], raw[offset + 2], raw[offset + 3]);
            }

            return result;
        }

        private static double[] Trim(double[] values, int count)
        {
            if (count <= 0)
            {
                return Array.Empty<double>();
            }

            if (count == values.Length)
            {
                return values;
            }

            var result = new double[count];
            Array.Copy(values, result, count);
            return result;
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing && handle != null)
                {
                    handle.Dispose();
                }

                disposed = true;
            }
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

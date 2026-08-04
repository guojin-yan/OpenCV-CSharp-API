using System;
using System.Globalization;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Features2D
{
    /// <summary>
    /// Provides maximally stable extremal region detection compatible with <c>cv::MSER</c>.
    /// 提供与 OpenCV <c>cv::MSER</c> 兼容的最大稳定极值区域检测能力。
    /// </summary>
    public sealed class MSER : Feature2D
    {
        private NativeMserHandle handle;
        private bool disposed;

        private MSER(IntPtr nativeHandle)
        {
            handle = NativeMserHandle.FromNativePointer(nativeHandle);
        }

        /// <inheritdoc/>
        public override bool IsDisposed
        {
            get { return disposed; }
        }

        /// <summary>
        /// Gets or sets the delta used to compare region sizes across intensity levels.
        /// 获取或设置跨灰度级比较区域大小时使用的 delta。
        /// </summary>
        public int Delta
        {
            get { return GetInt(NativeMethods.Features2DMserGetDelta); }
            set { SetInt(NativeMethods.Features2DMserSetDelta, value); }
        }

        /// <summary>
        /// Gets or sets the minimum region area.
        /// 获取或设置最小区域面积。
        /// </summary>
        public int MinArea
        {
            get { return GetInt(NativeMethods.Features2DMserGetMinArea); }
            set { SetInt(NativeMethods.Features2DMserSetMinArea, value); }
        }

        /// <summary>
        /// Gets or sets the maximum region area.
        /// 获取或设置最大区域面积。
        /// </summary>
        public int MaxArea
        {
            get { return GetInt(NativeMethods.Features2DMserGetMaxArea); }
            set { SetInt(NativeMethods.Features2DMserSetMaxArea, value); }
        }

        /// <summary>
        /// Gets or sets the maximum region variation.
        /// 获取或设置最大区域变化率。
        /// </summary>
        public double MaxVariation
        {
            get { return GetDouble(NativeMethods.Features2DMserGetMaxVariation); }
            set { SetDouble(NativeMethods.Features2DMserSetMaxVariation, value); }
        }

        /// <summary>
        /// Gets or sets the minimum diversity used by color MSER.
        /// 获取或设置彩色 MSER 使用的最小多样性。
        /// </summary>
        public double MinDiversity
        {
            get { return GetDouble(NativeMethods.Features2DMserGetMinDiversity); }
            set { SetDouble(NativeMethods.Features2DMserSetMinDiversity, value); }
        }

        /// <summary>
        /// Gets or sets the maximum evolution steps used by color MSER.
        /// 获取或设置彩色 MSER 使用的最大演化步数。
        /// </summary>
        public int MaxEvolution
        {
            get { return GetInt(NativeMethods.Features2DMserGetMaxEvolution); }
            set { SetInt(NativeMethods.Features2DMserSetMaxEvolution, value); }
        }

        /// <summary>
        /// Gets or sets the area threshold used by color MSER.
        /// 获取或设置彩色 MSER 使用的面积阈值。
        /// </summary>
        public double AreaThreshold
        {
            get { return GetDouble(NativeMethods.Features2DMserGetAreaThreshold); }
            set { SetDouble(NativeMethods.Features2DMserSetAreaThreshold, value); }
        }

        /// <summary>
        /// Gets or sets the minimum margin used by color MSER.
        /// 获取或设置彩色 MSER 使用的最小边距。
        /// </summary>
        public double MinMargin
        {
            get { return GetDouble(NativeMethods.Features2DMserGetMinMargin); }
            set { SetDouble(NativeMethods.Features2DMserSetMinMargin, value); }
        }

        /// <summary>
        /// Gets or sets the edge blur aperture size used by color MSER.
        /// 获取或设置彩色 MSER 使用的边缘模糊孔径尺寸。
        /// </summary>
        public int EdgeBlurSize
        {
            get { return GetInt(NativeMethods.Features2DMserGetEdgeBlurSize); }
            set { SetInt(NativeMethods.Features2DMserSetEdgeBlurSize, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether only the second pass is used.
        /// 获取或设置是否仅使用第二阶段处理。
        /// </summary>
        public bool Pass2Only
        {
            get { return GetInt(NativeMethods.Features2DMserGetPass2Only) != 0; }
            set { SetInt(NativeMethods.Features2DMserSetPass2Only, value ? 1 : 0); }
        }

        /// <inheritdoc/>
        public override bool Empty
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.Features2DMserEmpty(NativeHandle, out int empty));
                return empty != 0;
            }
        }

        /// <inheritdoc/>
        public override int DescriptorSize
        {
            get { return GetInt(NativeMethods.Features2DMserDescriptorSize); }
        }

        /// <inheritdoc/>
        public override int DescriptorType
        {
            get { return GetInt(NativeMethods.Features2DMserDescriptorType); }
        }

        /// <inheritdoc/>
        public override NormTypes DefaultNorm
        {
            get { return (NormTypes)GetInt(NativeMethods.Features2DMserDefaultNorm); }
        }

        /// <inheritdoc/>
        public override string DefaultName
        {
            get
            {
                ThrowIfDisposed();
                unsafe
                {
                    return NativeStringMarshaller.GetString(NativeHandle, NativeMethods.Features2DMserDefaultNameLength, NativeMethods.Features2DMserDefaultNameFill);
                }
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
        /// Creates an MSER detector.
        /// 创建 MSER 检测器。
        /// </summary>
        /// <param name="delta">The delta used to compare region sizes. 用于比较区域大小的 delta。</param>
        /// <param name="minArea">The minimum region area. 最小区域面积。</param>
        /// <param name="maxArea">The maximum region area. 最大区域面积。</param>
        /// <param name="maxVariation">The maximum variation. 最大变化率。</param>
        /// <param name="minDiversity">The minimum diversity for color images. 彩色图像使用的最小多样性。</param>
        /// <param name="maxEvolution">The maximum evolution steps for color images. 彩色图像使用的最大演化步数。</param>
        /// <param name="areaThreshold">The area threshold for color images. 彩色图像使用的面积阈值。</param>
        /// <param name="minMargin">The minimum margin for color images. 彩色图像使用的最小边距。</param>
        /// <param name="edgeBlurSize">The edge blur aperture size for color images. 彩色图像使用的边缘模糊孔径尺寸。</param>
        /// <returns>The created MSER detector. 创建的 MSER 检测器。</returns>
        public static MSER Create(
            int delta = 5,
            int minArea = 60,
            int maxArea = 14400,
            double maxVariation = 0.25,
            double minDiversity = 0.2,
            int maxEvolution = 200,
            double areaThreshold = 1.01,
            double minMargin = 0.003,
            int edgeBlurSize = 5)
        {
            NativeException.ThrowIfError(NativeMethods.Features2DMserCreate(
                delta,
                minArea,
                maxArea,
                maxVariation,
                minDiversity,
                maxEvolution,
                areaThreshold,
                minMargin,
                edgeBlurSize,
                out IntPtr nativeHandle));
            return new MSER(nativeHandle);
        }

        /// <inheritdoc/>
        public override void Clear()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.Features2DMserClear(NativeHandle));
        }

        /// <inheritdoc/>
        public override KeyPoint[] Detect(Mat image, Mat? mask = null)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            NativeException.ThrowIfError(NativeMethods.Features2DMserDetectCount(NativeHandle, image.NativeHandle, OptionalHandle(mask), out int keypointCount));
            if (keypointCount <= 0)
            {
                return Array.Empty<KeyPoint>();
            }

            var native = new NativeKeyPoint[keypointCount];
            unsafe
            {
                fixed (NativeKeyPoint* nativePtr = native)
                {
                    NativeException.ThrowIfError(NativeMethods.Features2DMserDetectFill(NativeHandle, image.NativeHandle, OptionalHandle(mask), nativePtr, native.Length, out int writtenCount));
                    return KeyPointMarshaller.FromNative(native, writtenCount);
                }
            }
        }

        /// <summary>
        /// Detects MSER regions and returns point sets with bounding boxes.
        /// 检测 MSER 区域，并返回区域点集与边界框。
        /// </summary>
        /// <param name="image">The input image; OpenCV expects 8UC1, 8UC3, or 8UC4 and at least 3x3. 输入图像；OpenCV 要求 8UC1、8UC3 或 8UC4 且尺寸至少为 3x3。</param>
        /// <returns>The detected MSER regions. 检测到的 MSER 区域。</returns>
        public MserRegion[] DetectRegions(Mat image)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            NativeException.ThrowIfError(NativeMethods.Features2DMserDetectRegionsCount(NativeHandle, image.NativeHandle, out int regionCount, out int totalPointCount));
            if (regionCount <= 0)
            {
                return Array.Empty<MserRegion>();
            }

            int[] offsets = new int[regionCount + 1];
            var points = new NativePoint[Math.Max(totalPointCount, 1)];
            var bboxes = new NativeRect[regionCount];
            unsafe
            {
                fixed (int* offsetsPtr = offsets)
                fixed (NativePoint* pointsPtr = points)
                fixed (NativeRect* bboxesPtr = bboxes)
                {
                    NativeException.ThrowIfError(NativeMethods.Features2DMserDetectRegionsFill(
                        NativeHandle,
                        image.NativeHandle,
                        offsetsPtr,
                        offsets.Length,
                        pointsPtr,
                        points.Length,
                        bboxesPtr,
                        bboxes.Length,
                        out int writtenRegionCount,
                        out int writtenPointCount));
                    return FromNativeRegions(offsets, writtenRegionCount, points, writtenPointCount, bboxes);
                }
            }
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return disposed
                ? "{Disposed=True}"
                : "{Delta=" + Delta
                    + ",MinArea=" + MinArea
                    + ",MaxArea=" + MaxArea
                    + ",MaxVariation=" + MaxVariation.ToString(CultureInfo.InvariantCulture)
                    + ",MinDiversity=" + MinDiversity.ToString(CultureInfo.InvariantCulture)
                    + ",AreaThreshold=" + AreaThreshold.ToString(CultureInfo.InvariantCulture)
                    + ",MinMargin=" + MinMargin.ToString(CultureInfo.InvariantCulture)
                    + ",Pass2Only=" + Pass2Only + "}";
        }

        private delegate int IntGetter(IntPtr handle, out int value);

        private delegate int IntSetter(IntPtr handle, int value);

        private delegate int DoubleGetter(IntPtr handle, out double value);

        private delegate int DoubleSetter(IntPtr handle, double value);

        private int GetInt(IntGetter getter)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(getter(NativeHandle, out int value));
            return value;
        }

        private void SetInt(IntSetter setter, int value)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(setter(NativeHandle, value));
        }

        private double GetDouble(DoubleGetter getter)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(getter(NativeHandle, out double value));
            return value;
        }

        private void SetDouble(DoubleSetter setter, double value)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(setter(NativeHandle, value));
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

        private static IntPtr OptionalHandle(Mat? mat)
        {
            return mat == null ? IntPtr.Zero : mat.NativeHandle;
        }

        private static MserRegion[] FromNativeRegions(int[] offsets, int regionCount, NativePoint[] points, int pointCount, NativeRect[] bboxes)
        {
            var result = new MserRegion[regionCount];
            for (int i = 0; i < regionCount; i++)
            {
                int start = offsets[i];
                int end = offsets[i + 1];
                int length = Math.Max(0, Math.Min(end, pointCount) - start);
                var regionPoints = new Point[length];
                for (int j = 0; j < length; j++)
                {
                    NativePoint point = points[start + j];
                    regionPoints[j] = new Point(point.X, point.Y);
                }

                NativeRect bbox = bboxes[i];
                result[i] = new MserRegion(regionPoints, new Rect(bbox.X, bbox.Y, bbox.Width, bbox.Height));
            }

            return result;
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

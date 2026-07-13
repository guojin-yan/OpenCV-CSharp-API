using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.ObjDetect
{
    /// <summary>
    /// MCC color checker result compatible with OpenCV <c>cv::mcc::CChecker</c>.
    /// 与 OpenCV <c>cv::mcc::CChecker</c> 兼容的 MCC 色卡结果。
    /// </summary>
    public sealed unsafe class CChecker : IDisposable
    {
        private NativeMccCheckerHandle handle;
        private bool disposed;

        /// <summary>
        /// Initializes an empty MCC checker object.
        /// 初始化一个空的 MCC checker 对象。
        /// </summary>
        public CChecker()
        {
            NativeException.ThrowIfError(NativeMethods.MccCheckerCreate(out IntPtr nativeHandle));
            handle = NativeMccCheckerHandle.FromNativePointer(nativeHandle);
        }

        internal CChecker(IntPtr nativeHandle)
        {
            handle = NativeMccCheckerHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Gets whether this checker has been disposed. 获取 checker 是否已经释放。</summary>
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

        /// <summary>Gets or sets the target color chart type. 获取或设置目标色卡类型。</summary>
        public ColorChart Target
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.MccCheckerGetTarget(NativeHandle, out int target));
                return (ColorChart)target;
            }

            set
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.MccCheckerSetTarget(NativeHandle, (int)value));
            }
        }

        /// <summary>Gets or sets the checker matching cost. 获取或设置 checker 匹配代价。</summary>
        public float Cost
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.MccCheckerGetCost(NativeHandle, out float cost));
                return cost;
            }

            set
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.MccCheckerSetCost(NativeHandle, value));
            }
        }

        /// <summary>Gets or sets the checker center. 获取或设置 checker 中心点。</summary>
        public Point2f Center
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.MccCheckerGetCenter(NativeHandle, out NativeMethods.Point2fNative center));
                return new Point2f(center.X, center.Y);
            }

            set
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.MccCheckerSetCenter(
                    NativeHandle,
                    new NativeMethods.Point2fNative { X = value.X, Y = value.Y }));
            }
        }

        /// <summary>Gets the detected checker quadrilateral. 获取检测到的 checker 四边形。</summary>
        public Point2f[] GetBox()
        {
            return GetPointVector(NativeMethods.MccCheckerGetBoxCount, NativeMethods.MccCheckerGetBoxFill);
        }

        /// <summary>Sets the detected checker quadrilateral. 设置检测到的 checker 四边形。</summary>
        public CChecker SetBox(Point2f[] points)
        {
            ThrowIfDisposed();
            if (points == null)
            {
                throw new ArgumentNullException(nameof(points));
            }

            NativeMethods.Point2fNative[] native = ToNative(points);
            fixed (NativeMethods.Point2fNative* nativePtr = native)
            {
                NativeException.ThrowIfError(NativeMethods.MccCheckerSetBox(NativeHandle, nativePtr, native.Length));
            }

            return this;
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>Sets the detected checker quadrilateral from a span. 使用 Span 设置检测到的 checker 四边形。</summary>
        public CChecker SetBox(ReadOnlySpan<Point2f> points)
        {
            ThrowIfDisposed();
            NativeMethods.Point2fNative[] native = ToNative(points);
            fixed (NativeMethods.Point2fNative* nativePtr = native)
            {
                NativeException.ThrowIfError(NativeMethods.MccCheckerSetBox(NativeHandle, nativePtr, native.Length));
            }

            return this;
        }
#endif

        /// <summary>Gets color patch center coordinates. 获取色块中心点坐标。</summary>
        public Point2f[] GetColorCharts()
        {
            return GetPointVector(NativeMethods.MccCheckerGetColorChartsCount, NativeMethods.MccCheckerGetColorChartsFill);
        }

        /// <summary>Gets sampled RGB chart data as an owned matrix. 获取采样 RGB 色卡数据矩阵。</summary>
        public Mat GetChartsRGB(bool getStats = true)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MccCheckerGetChartsRgb(NativeHandle, getStats ? 1 : 0, out IntPtr mat));
            return new Mat(mat);
        }

        /// <summary>Sets sampled RGB chart data from a matrix copy. 从矩阵副本设置采样 RGB 色卡数据。</summary>
        public CChecker SetChartsRGB(Mat chartsRGB)
        {
            ThrowIfDisposed();
            ValidateNotNull(chartsRGB, nameof(chartsRGB));
            NativeException.ThrowIfError(NativeMethods.MccCheckerSetChartsRgb(NativeHandle, chartsRGB.NativeHandle));
            return this;
        }

        /// <summary>Gets sampled YCbCr chart data as an owned matrix. 获取采样 YCbCr 色卡数据矩阵。</summary>
        public Mat GetChartsYCbCr()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MccCheckerGetChartsYCbCr(NativeHandle, out IntPtr mat));
            return new Mat(mat);
        }

        /// <summary>Sets sampled YCbCr chart data from a matrix copy. 从矩阵副本设置采样 YCbCr 色卡数据。</summary>
        public CChecker SetChartsYCbCr(Mat chartsYCbCr)
        {
            ThrowIfDisposed();
            ValidateNotNull(chartsYCbCr, nameof(chartsYCbCr));
            NativeException.ThrowIfError(NativeMethods.MccCheckerSetChartsYCbCr(NativeHandle, chartsYCbCr.NativeHandle));
            return this;
        }

        /// <summary>Releases the native checker. 释放 native checker。</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private Point2f[] GetPointVector(
            PointCountInvoker countInvoker,
            PointFillInvoker fillInvoker)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(countInvoker(NativeHandle, out int count));
            var native = new NativeMethods.Point2fNative[Math.Max(count, 0)];
            fixed (NativeMethods.Point2fNative* nativePtr = native)
            {
                NativeException.ThrowIfError(fillInvoker(NativeHandle, nativePtr, native.Length, out count));
            }

            return ToManaged(native, Math.Max(0, Math.Min(count, native.Length)));
        }

        private delegate int PointCountInvoker(IntPtr checker, out int pointCount);

        private delegate int PointFillInvoker(IntPtr checker, NativeMethods.Point2fNative* points, int pointCapacity, out int pointCount);

        private static NativeMethods.Point2fNative[] ToNative(Point2f[] points)
        {
            if (points.Length == 0)
            {
                return Array.Empty<NativeMethods.Point2fNative>();
            }

            var result = new NativeMethods.Point2fNative[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                result[i] = new NativeMethods.Point2fNative { X = points[i].X, Y = points[i].Y };
            }

            return result;
        }

#if NETCOREAPP3_1_OR_GREATER
        private static NativeMethods.Point2fNative[] ToNative(ReadOnlySpan<Point2f> points)
        {
            if (points.Length == 0)
            {
                return Array.Empty<NativeMethods.Point2fNative>();
            }

            var result = new NativeMethods.Point2fNative[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                result[i] = new NativeMethods.Point2fNative { X = points[i].X, Y = points[i].Y };
            }

            return result;
        }
#endif

        private static Point2f[] ToManaged(NativeMethods.Point2fNative[] points, int count)
        {
            if (count <= 0)
            {
                return Array.Empty<Point2f>();
            }

            var result = new Point2f[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = new Point2f(points[i].X, points[i].Y);
            }

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

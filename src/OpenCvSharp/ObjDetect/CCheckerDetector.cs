using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.ObjDetect
{
    /// <summary>
    /// MCC color checker detector compatible with OpenCV <c>cv::mcc::CCheckerDetector</c>.
    /// 与 OpenCV <c>cv::mcc::CCheckerDetector</c> 兼容的 MCC 色卡检测器。
    /// </summary>
    public sealed unsafe class CCheckerDetector : IDisposable
    {
        private NativeMccCheckerDetectorHandle handle;
        private bool disposed;

        /// <summary>
        /// Initializes an MCC checker detector.
        /// 初始化 MCC checker 检测器。
        /// </summary>
        public CCheckerDetector()
        {
            NativeException.ThrowIfError(NativeMethods.MccCheckerDetectorCreate(out IntPtr nativeHandle));
            handle = NativeMccCheckerDetectorHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Gets whether this detector has been disposed. 获取 detector 是否已经释放。</summary>
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
        public ColorChart ColorChartType
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.MccCheckerDetectorGetColorChartType(NativeHandle, out int chartType));
                return (ColorChart)chartType;
            }

            set
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.MccCheckerDetectorSetColorChartType(NativeHandle, (int)value));
            }
        }

        /// <summary>Runs MCC detection over the full image. 在整张图像上运行 MCC 检测。</summary>
        public bool Process(Mat image, int nc = 1)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            NativeException.ThrowIfError(NativeMethods.MccCheckerDetectorProcess(NativeHandle, image.NativeHandle, nc, out int detected));
            return detected != 0;
        }

        /// <summary>Runs MCC detection inside regions of interest. 在感兴趣区域内运行 MCC 检测。</summary>
        public bool Process(Mat image, Rect[] regionsOfInterest, int nc = 1)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            if (regionsOfInterest == null)
            {
                throw new ArgumentNullException(nameof(regionsOfInterest));
            }

            int[] nativeRois = ToNativeRectArray(regionsOfInterest);
            NativeException.ThrowIfError(NativeMethods.MccCheckerDetectorProcessWithRoi(NativeHandle, image.NativeHandle, nativeRois, regionsOfInterest.Length, nc, out int detected));
            return detected != 0;
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>Runs MCC detection inside regions of interest from a span. 使用 Span 中的 ROI 运行 MCC 检测。</summary>
        public bool Process(Mat image, ReadOnlySpan<Rect> regionsOfInterest, int nc = 1)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            int[] nativeRois = ToNativeRectArray(regionsOfInterest);
            fixed (int* roiPtr = nativeRois)
            {
                NativeException.ThrowIfError(NativeMethods.MccCheckerDetectorProcessWithRoi(NativeHandle, image.NativeHandle, roiPtr, regionsOfInterest.Length, nc, out int detected));
                return detected != 0;
            }
        }
#endif

        /// <summary>Gets the best detected color checker, or null if none was detected. 获取最佳色卡；未检测到时返回 null。</summary>
        public CChecker? GetBestColorChecker()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MccCheckerDetectorGetBestColorChecker(NativeHandle, out IntPtr checker, out int hasChecker));
            return hasChecker == 0 || checker == IntPtr.Zero ? null : new CChecker(checker);
        }

        /// <summary>Gets all detected color checkers. 获取所有检测到的色卡。</summary>
        public CChecker[] GetListColorChecker()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MccCheckerDetectorGetListColorCheckerCount(NativeHandle, out int count));
            if (count <= 0)
            {
                return Array.Empty<CChecker>();
            }

            var handles = new IntPtr[count];
            NativeException.ThrowIfError(NativeMethods.MccCheckerDetectorGetListColorCheckerFill(NativeHandle, handles, handles.Length, out int written));
            int safeCount = Math.Max(0, Math.Min(written, handles.Length));
            var result = new CChecker[safeCount];
            try
            {
                for (int i = 0; i < safeCount; i++)
                {
                    result[i] = new CChecker(handles[i]);
                    handles[i] = IntPtr.Zero;
                }

                return result;
            }
            finally
            {
                for (int i = 0; i < handles.Length; i++)
                {
                    if (handles[i] != IntPtr.Zero)
                    {
                        NativeMethods.MccCheckerReleaseHandle(handles[i]);
                    }
                }
            }
        }

        /// <summary>Draws color checkers on an image. 在图像上绘制色卡。</summary>
        public void Draw(CChecker[] checkers, Mat image, Scalar color = default, int thickness = 2)
        {
            ThrowIfDisposed();
            if (checkers == null)
            {
                throw new ArgumentNullException(nameof(checkers));
            }

            ValidateNotNull(image, nameof(image));
            IntPtr[] handles = ToNativeHandles(checkers);
            Scalar drawColor = color.Equals(default(Scalar)) ? new Scalar(0, 250, 0, 0) : color;
            NativeException.ThrowIfError(NativeMethods.MccCheckerDetectorDraw(
                NativeHandle,
                handles,
                handles.Length,
                image.NativeHandle,
                drawColor.V0,
                drawColor.V1,
                drawColor.V2,
                drawColor.V3,
                thickness));
        }

        /// <summary>Gets reference chart colors as an owned matrix. 获取参考色卡颜色矩阵。</summary>
        public Mat GetRefColors()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MccCheckerDetectorGetRefColors(NativeHandle, out IntPtr mat));
            return new Mat(mat);
        }

        /// <summary>Gets detector parameters. 获取检测参数。</summary>
        public DetectorParametersMCC GetDetectionParams()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MccCheckerDetectorGetDetectionParams(NativeHandle, out NativeMethods.MccDetectorParamsNative native));
            return DetectorParametersMCC.FromNative(native);
        }

        /// <summary>Sets detector parameters. 设置检测参数。</summary>
        public CCheckerDetector SetDetectionParams(DetectorParametersMCC parameters)
        {
            ThrowIfDisposed();
            ValidateNotNull(parameters, nameof(parameters));
            NativeMethods.MccDetectorParamsNative native = parameters.ToNative();
            NativeException.ThrowIfError(NativeMethods.MccCheckerDetectorSetDetectionParams(NativeHandle, ref native));
            return this;
        }

        /// <summary>Releases the native detector. 释放 native detector。</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private static int[] ToNativeRectArray(Rect[] rectangles)
        {
            if (rectangles.Length == 0)
            {
                return Array.Empty<int>();
            }

            var result = new int[checked(rectangles.Length * 4)];
            for (int i = 0; i < rectangles.Length; i++)
            {
                int offset = i * 4;
                result[offset] = rectangles[i].X;
                result[offset + 1] = rectangles[i].Y;
                result[offset + 2] = rectangles[i].Width;
                result[offset + 3] = rectangles[i].Height;
            }

            return result;
        }

#if NETCOREAPP3_1_OR_GREATER
        private static int[] ToNativeRectArray(ReadOnlySpan<Rect> rectangles)
        {
            if (rectangles.Length == 0)
            {
                return Array.Empty<int>();
            }

            var result = new int[checked(rectangles.Length * 4)];
            for (int i = 0; i < rectangles.Length; i++)
            {
                int offset = i * 4;
                result[offset] = rectangles[i].X;
                result[offset + 1] = rectangles[i].Y;
                result[offset + 2] = rectangles[i].Width;
                result[offset + 3] = rectangles[i].Height;
            }

            return result;
        }
#endif

        private static IntPtr[] ToNativeHandles(CChecker[] checkers)
        {
            if (checkers.Length == 0)
            {
                return Array.Empty<IntPtr>();
            }

            var result = new IntPtr[checkers.Length];
            for (int i = 0; i < checkers.Length; i++)
            {
                if (checkers[i] == null)
                {
                    throw new ArgumentNullException(nameof(checkers));
                }

                result[i] = checkers[i].NativeHandle;
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

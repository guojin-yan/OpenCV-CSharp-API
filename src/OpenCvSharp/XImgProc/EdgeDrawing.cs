using System;
using OpenCvSharp.Core;
using OpenCvSharp.ImgProc;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.XImgProc
{
    /// <summary>
    /// EdgeDrawing detector wrapper.
    /// EdgeDrawing 检测器包装。
    /// </summary>
    public sealed class EdgeDrawing : IDisposable
    {
        private NativeXImgProcEdgeDrawingHandle handle;
        private bool disposed;

        private EdgeDrawing(IntPtr nativeHandle)
        {
            handle = NativeXImgProcEdgeDrawingHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Gets whether this detector has been disposed. 获取检测器是否已经释放。</summary>
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

        /// <summary>Gets or sets EdgeDrawing parameters. 获取或设置 EdgeDrawing 参数。</summary>
        public EdgeDrawingParams Params
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.XImgProcEdgeDrawingGetParams(NativeHandle, out NativeMethods.XImgProcEdgeDrawingParamsNative native));
                return EdgeDrawingParams.FromNative(native);
            }

            set
            {
                ThrowIfDisposed();
                NativeMethods.XImgProcEdgeDrawingParamsNative native = value.ToNative();
                NativeException.ThrowIfError(NativeMethods.XImgProcEdgeDrawingSetParams(NativeHandle, ref native));
            }
        }

        /// <summary>Creates an EdgeDrawing detector. 创建 EdgeDrawing 检测器。</summary>
        public static EdgeDrawing Create()
        {
            NativeException.ThrowIfError(NativeMethods.XImgProcEdgeDrawingCreate(out IntPtr nativeHandle));
            return new EdgeDrawing(nativeHandle);
        }

        /// <summary>Detects edge segments from an image. 从图像检测边缘片段。</summary>
        public void DetectEdges(Mat src)
        {
            ThrowIfDisposed();
            XImgProcCv2.ValidateNotNull(src, nameof(src));
            ValidateDetectEdgesSource(src);
            NativeException.ThrowIfError(NativeMethods.XImgProcEdgeDrawingDetectEdges(NativeHandle, src.NativeHandle));
        }

        /// <summary>Copies the edge image into <paramref name="dst"/>. 将边缘图写入 <paramref name="dst"/>。</summary>
        public void GetEdgeImage(Mat dst)
        {
            ThrowIfDisposed();
            XImgProcCv2.ValidateNotNull(dst, nameof(dst));
            NativeException.ThrowIfError(NativeMethods.XImgProcEdgeDrawingGetEdgeImage(NativeHandle, dst.NativeHandle));
        }

        /// <summary>Gets the edge image as a new matrix. 以新矩阵返回边缘图。</summary>
        public Mat GetEdgeImage()
        {
            return CreateOutput(GetEdgeImage);
        }

        /// <summary>Copies the gradient image into <paramref name="dst"/>. 将梯度图写入 <paramref name="dst"/>。</summary>
        public void GetGradientImage(Mat dst)
        {
            ThrowIfDisposed();
            XImgProcCv2.ValidateNotNull(dst, nameof(dst));
            NativeException.ThrowIfError(NativeMethods.XImgProcEdgeDrawingGetGradientImage(NativeHandle, dst.NativeHandle));
        }

        /// <summary>Gets the gradient image as a new matrix. 以新矩阵返回梯度图。</summary>
        public Mat GetGradientImage()
        {
            return CreateOutput(GetGradientImage);
        }

        /// <summary>Gets detected edge segments as grouped points. 以分组点返回检测到的边缘片段。</summary>
        public Point[][] GetSegments()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.XImgProcEdgeDrawingGetSegmentsCount(NativeHandle, out int groupCount, out int pointCount));
            if (groupCount <= 0)
            {
                return Array.Empty<Point[]>();
            }

            int[] offsets = new int[groupCount + 1];
            var nativePoints = new NativeMethods.XImgProcPointNative[pointCount];
            NativeException.ThrowIfError(NativeMethods.XImgProcEdgeDrawingGetSegmentsFill(NativeHandle, offsets, offsets.Length, nativePoints, nativePoints.Length, out int writtenGroups, out int writtenPoints));
            return ToPointGroups(offsets, nativePoints, writtenGroups, writtenPoints);
        }

        /// <summary>Detects line segments into a matrix. 将线段检测结果写入矩阵。</summary>
        public void DetectLines(Mat lines)
        {
            ThrowIfDisposed();
            XImgProcCv2.ValidateNotNull(lines, nameof(lines));
            NativeException.ThrowIfError(NativeMethods.XImgProcEdgeDrawingDetectLines(NativeHandle, lines.NativeHandle));
        }

        /// <summary>Detects line segments as managed values. 以 managed 值返回线段检测结果。</summary>
        public LineSegment[] DetectLines()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.XImgProcEdgeDrawingDetectLinesCount(NativeHandle, out int lineCount));
            if (lineCount <= 0)
            {
                return Array.Empty<LineSegment>();
            }

            var values = new float[lineCount * 4];
            NativeException.ThrowIfError(NativeMethods.XImgProcEdgeDrawingDetectLinesFill(NativeHandle, values, lineCount, out int writtenCount));
            return ToLineSegments(values, writtenCount);
        }

        /// <summary>Gets segment indices corresponding to the last detected lines. 获取最近线段检测对应的 segment 索引。</summary>
        public int[] GetSegmentIndicesOfLines()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.XImgProcEdgeDrawingGetSegmentIndicesOfLinesCount(NativeHandle, out int count));
            if (count <= 0)
            {
                return Array.Empty<int>();
            }

            int[] values = new int[count];
            NativeException.ThrowIfError(NativeMethods.XImgProcEdgeDrawingGetSegmentIndicesOfLinesFill(NativeHandle, values, values.Length, out int writtenCount));
            if (writtenCount == values.Length)
            {
                return values;
            }

            Array.Resize(ref values, writtenCount);
            return values;
        }

        /// <summary>Detects ellipses into a matrix. 将椭圆检测结果写入矩阵。</summary>
        public void DetectEllipses(Mat ellipses)
        {
            ThrowIfDisposed();
            XImgProcCv2.ValidateNotNull(ellipses, nameof(ellipses));
            NativeException.ThrowIfError(NativeMethods.XImgProcEdgeDrawingDetectEllipses(NativeHandle, ellipses.NativeHandle));
        }

        /// <summary>Detects ellipses as managed values. 以 managed 值返回椭圆检测结果。</summary>
        public EdgeDrawingEllipse[] DetectEllipses()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.XImgProcEdgeDrawingDetectEllipsesCount(NativeHandle, out int ellipseCount));
            if (ellipseCount <= 0)
            {
                return Array.Empty<EdgeDrawingEllipse>();
            }

            var values = new double[ellipseCount * 6];
            NativeException.ThrowIfError(NativeMethods.XImgProcEdgeDrawingDetectEllipsesFill(NativeHandle, values, ellipseCount, out int writtenCount));
            return ToEllipses(values, writtenCount);
        }

        /// <summary>Releases native resources. 释放 native 资源。</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private static Mat CreateOutput(Action<Mat> action)
        {
            var dst = new Mat();
            try
            {
                action(dst);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        private static void ValidateDetectEdgesSource(Mat src)
        {
            if (src.Empty)
            {
                throw new ArgumentException("EdgeDrawing source image must not be empty.", nameof(src));
            }

            if (src.Type != MatType.CV_8UC1 &&
                src.Type != MatType.CV_8UC3 &&
                src.Type != MatType.CV_8UC4)
            {
                throw new ArgumentException("EdgeDrawing source image must be CV_8UC1, CV_8UC3, or CV_8UC4.", nameof(src));
            }
        }

        private static Point[][] ToPointGroups(int[] offsets, NativeMethods.XImgProcPointNative[] points, int groupCount, int pointCount)
        {
            var result = new Point[groupCount][];
            for (int i = 0; i < groupCount; i++)
            {
                int start = offsets[i];
                int end = offsets[i + 1];
                if (start < 0 || end < start || end > pointCount)
                {
                    result[i] = Array.Empty<Point>();
                    continue;
                }

                var group = new Point[end - start];
                for (int j = 0; j < group.Length; j++)
                {
                    NativeMethods.XImgProcPointNative point = points[start + j];
                    group[j] = new Point(point.X, point.Y);
                }

                result[i] = group;
            }

            return result;
        }

        private static LineSegment[] ToLineSegments(float[] values, int count)
        {
            var result = new LineSegment[count];
            for (int i = 0; i < count; i++)
            {
                int offset = i * 4;
                result[i] = new LineSegment(values[offset], values[offset + 1], values[offset + 2], values[offset + 3]);
            }

            return result;
        }

        private static EdgeDrawingEllipse[] ToEllipses(double[] values, int count)
        {
            var result = new EdgeDrawingEllipse[count];
            for (int i = 0; i < count; i++)
            {
                int offset = i * 6;
                result[i] = new EdgeDrawingEllipse(values[offset], values[offset + 1], values[offset + 2], values[offset + 3], values[offset + 4], values[offset + 5]);
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

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }
}

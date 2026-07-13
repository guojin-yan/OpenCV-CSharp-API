using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.ImgProc
{
    /// <summary>
    /// Provides OpenCV line segment detection compatible with <c>cv::LineSegmentDetector</c>.
    /// 提供与 OpenCV <c>cv::LineSegmentDetector</c> 兼容的线段检测能力。
    /// </summary>
    public sealed class LineSegmentDetector : IDisposable
    {
        private NativeLineSegmentDetectorHandle handle;
        private bool disposed;

        internal LineSegmentDetector(IntPtr nativeHandle)
        {
            handle = NativeLineSegmentDetectorHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>
        /// Gets a value indicating whether this object has been disposed.
        /// 获取此对象是否已经释放。
        /// </summary>
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
        /// Detects line segments into an OpenCV matrix.
        /// 将检测到的线段写入 OpenCV 矩阵。
        /// </summary>
        /// <param name="image">The grayscale source image. 灰度源图像。</param>
        /// <param name="lines">The output line matrix of <c>Vec4f</c>. 输出 <c>Vec4f</c> 线段矩阵。</param>
        /// <param name="width">Optional output line widths. 可选输出线宽。</param>
        /// <param name="prec">Optional output precisions. 可选输出精度。</param>
        /// <param name="nfa">Optional output false-alarm scores. 可选输出误警评分。</param>
        public void Detect(Mat image, Mat lines, Mat? width = null, Mat? prec = null, Mat? nfa = null)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(lines, nameof(lines));

            NativeException.ThrowIfError(NativeMethods.ImgProcLineSegmentDetectorDetect(
                NativeHandle,
                image.NativeHandle,
                lines.NativeHandle,
                width == null ? IntPtr.Zero : width.NativeHandle,
                prec == null ? IntPtr.Zero : prec.NativeHandle,
                nfa == null ? IntPtr.Zero : nfa.NativeHandle));
        }

        /// <summary>
        /// Detects line segments and returns them as managed values.
        /// 检测线段并以 managed 值返回。
        /// </summary>
        /// <param name="image">The grayscale source image. 灰度源图像。</param>
        /// <returns>The detected line segments. 检测到的线段。</returns>
        public LineSegment[] Detect(Mat image)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));

            NativeException.ThrowIfError(NativeMethods.ImgProcLineSegmentDetectorDetectCount(
                NativeHandle,
                image.NativeHandle,
                out int lineCount));

            if (lineCount <= 0)
            {
                return Array.Empty<LineSegment>();
            }

            var values = new float[lineCount * 4];
            NativeException.ThrowIfError(NativeMethods.ImgProcLineSegmentDetectorDetectFill(
                NativeHandle,
                image.NativeHandle,
                values,
                lineCount,
                out int writtenCount));
            return FromInterleavedLineSegments(values, writtenCount);
        }

        /// <summary>
        /// Draws line segments stored in an OpenCV matrix.
        /// 绘制存储在 OpenCV 矩阵中的线段。
        /// </summary>
        /// <param name="image">The image to draw on. 要绘制的图像。</param>
        /// <param name="lines">The line matrix. 线段矩阵。</param>
        public void DrawSegments(Mat image, Mat lines)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(lines, nameof(lines));
            NativeException.ThrowIfError(NativeMethods.ImgProcLineSegmentDetectorDrawSegments(NativeHandle, image.NativeHandle, lines.NativeHandle));
        }

        /// <summary>
        /// Draws managed line segment values.
        /// 绘制 managed 线段值。
        /// </summary>
        /// <param name="image">The image to draw on. 要绘制的图像。</param>
        /// <param name="lines">The line segments. 线段。</param>
        public void DrawSegments(Mat image, LineSegment[] lines)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            float[] values = ToInterleavedLineSegments(lines, nameof(lines));
            if (lines.Length == 0)
            {
                return;
            }

            NativeException.ThrowIfError(NativeMethods.ImgProcLineSegmentDetectorDrawSegmentsArray(
                NativeHandle,
                image.NativeHandle,
                values,
                lines.Length));
        }

        /// <summary>
        /// Compares two line segment matrices.
        /// 比较两个线段矩阵。
        /// </summary>
        /// <param name="size">The source image size. 源图像尺寸。</param>
        /// <param name="lines1">The first line matrix. 第一组线段矩阵。</param>
        /// <param name="lines2">The second line matrix. 第二组线段矩阵。</param>
        /// <param name="image">Optional output visualization image. 可选输出可视化图像。</param>
        /// <returns>The number of mismatching pixels. 不匹配像素数量。</returns>
        public int CompareSegments(Size size, Mat lines1, Mat lines2, Mat? image = null)
        {
            ThrowIfDisposed();
            ValidateNotNull(lines1, nameof(lines1));
            ValidateNotNull(lines2, nameof(lines2));

            NativeException.ThrowIfError(NativeMethods.ImgProcLineSegmentDetectorCompareSegments(
                NativeHandle,
                size.Width,
                size.Height,
                lines1.NativeHandle,
                lines2.NativeHandle,
                image == null ? IntPtr.Zero : image.NativeHandle,
                out int mismatchCount));
            return mismatchCount;
        }

        /// <summary>
        /// Compares two managed line segment arrays.
        /// 比较两组 managed 线段数组。
        /// </summary>
        /// <param name="size">The source image size. 源图像尺寸。</param>
        /// <param name="lines1">The first line segment array. 第一组线段数组。</param>
        /// <param name="lines2">The second line segment array. 第二组线段数组。</param>
        /// <param name="image">Optional output visualization image. 可选输出可视化图像。</param>
        /// <returns>The number of mismatching pixels. 不匹配像素数量。</returns>
        public int CompareSegments(Size size, LineSegment[] lines1, LineSegment[] lines2, Mat? image = null)
        {
            ThrowIfDisposed();
            float[] values1 = ToInterleavedLineSegments(lines1, nameof(lines1));
            float[] values2 = ToInterleavedLineSegments(lines2, nameof(lines2));

            NativeException.ThrowIfError(NativeMethods.ImgProcLineSegmentDetectorCompareSegmentsArray(
                NativeHandle,
                size.Width,
                size.Height,
                values1,
                lines1.Length,
                values2,
                lines2.Length,
                image == null ? IntPtr.Zero : image.NativeHandle,
                out int mismatchCount));
            return mismatchCount;
        }

        /// <summary>
        /// Releases the native line segment detector.
        /// 释放 native 线段检测器。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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

        private static LineSegment[] FromInterleavedLineSegments(float[] values, int lineCount)
        {
            var result = new LineSegment[lineCount];
            for (int i = 0; i < lineCount; i++)
            {
                int offset = i * 4;
                result[i] = new LineSegment(values[offset], values[offset + 1], values[offset + 2], values[offset + 3]);
            }

            return result;
        }

        private static float[] ToInterleavedLineSegments(LineSegment[] lines, string parameterName)
        {
            if (lines == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            var values = new float[lines.Length * 4];
            for (int i = 0; i < lines.Length; i++)
            {
                int offset = i * 4;
                values[offset] = lines[i].X1;
                values[offset + 1] = lines[i].Y1;
                values[offset + 2] = lines[i].X2;
                values[offset + 3] = lines[i].Y2;
            }

            return values;
        }

        private static void ValidateNotNull<T>(T value, string parameterName)
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

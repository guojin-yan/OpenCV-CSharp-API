using System;
using OpenCvSharp.Core;
using OpenCvSharp.ImgProc;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.XImgProc
{
    /// <summary>
    /// Fast line detector wrapper from ximgproc.
    /// ximgproc 快速线段检测器包装。
    /// </summary>
    public sealed class FastLineDetector : IDisposable
    {
        private NativeXImgProcFastLineDetectorHandle handle;
        private bool disposed;

        private FastLineDetector(IntPtr nativeHandle)
        {
            handle = NativeXImgProcFastLineDetectorHandle.FromNativePointer(nativeHandle);
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

        /// <summary>Creates a fast line detector. 创建快速线段检测器。</summary>
        public static FastLineDetector Create(int lengthThreshold = 10, float distanceThreshold = 1.414213562F, double cannyTh1 = 50.0, double cannyTh2 = 50.0, int cannyApertureSize = 3, bool doMerge = false)
        {
            if (lengthThreshold <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(lengthThreshold), "Length threshold must be greater than zero.");
            }

            if (distanceThreshold <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(distanceThreshold), "Distance threshold must be greater than zero.");
            }

            if (cannyTh1 <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cannyTh1), "Canny threshold 1 must be greater than zero.");
            }

            if (cannyTh2 <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cannyTh2), "Canny threshold 2 must be greater than zero.");
            }

            if (cannyApertureSize < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cannyApertureSize), "Canny aperture size must be greater than or equal to zero.");
            }

            NativeException.ThrowIfError(NativeMethods.XImgProcFastLineDetectorCreate(lengthThreshold, distanceThreshold, cannyTh1, cannyTh2, cannyApertureSize, doMerge ? 1 : 0, out IntPtr nativeHandle));
            return new FastLineDetector(nativeHandle);
        }

        /// <summary>Detects line segments into a matrix. 将检测到的线段写入矩阵。</summary>
        public void Detect(Mat image, Mat lines)
        {
            ThrowIfDisposed();
            XImgProcCv2.ValidateNotNull(image, nameof(image));
            XImgProcCv2.ValidateNotNull(lines, nameof(lines));
            ValidateDetectImage(image, nameof(image));
            NativeException.ThrowIfError(NativeMethods.XImgProcFastLineDetectorDetect(NativeHandle, image.NativeHandle, lines.NativeHandle));
        }

        /// <summary>Detects line segments as managed values. 以 managed 值返回检测到的线段。</summary>
        public LineSegment[] Detect(Mat image)
        {
            ThrowIfDisposed();
            XImgProcCv2.ValidateNotNull(image, nameof(image));
            ValidateDetectImage(image, nameof(image));
            NativeException.ThrowIfError(NativeMethods.XImgProcFastLineDetectorDetectCount(NativeHandle, image.NativeHandle, out int lineCount));
            if (lineCount <= 0)
            {
                return Array.Empty<LineSegment>();
            }

            var values = new float[lineCount * 4];
            NativeException.ThrowIfError(NativeMethods.XImgProcFastLineDetectorDetectFill(NativeHandle, image.NativeHandle, values, lineCount, out int writtenCount));
            return FromInterleavedLineSegments(values, writtenCount);
        }

        /// <summary>Draws line segments stored in a matrix. 绘制矩阵中的线段。</summary>
        public void DrawSegments(Mat image, Mat lines, bool drawArrow = false, Scalar? lineColor = null, int lineThickness = 1)
        {
            ThrowIfDisposed();
            XImgProcCv2.ValidateNotNull(image, nameof(image));
            XImgProcCv2.ValidateNotNull(lines, nameof(lines));
            ValidateDrawSegmentsImage(image, nameof(image));
            ValidateDrawSegmentsLines(lines, nameof(lines));
            Scalar color = lineColor ?? new Scalar(0, 0, 255);
            NativeException.ThrowIfError(NativeMethods.XImgProcFastLineDetectorDrawSegments(
                NativeHandle,
                image.NativeHandle,
                lines.NativeHandle,
                drawArrow ? 1 : 0,
                color.V0,
                color.V1,
                color.V2,
                color.V3,
                lineThickness));
        }

        /// <summary>Draws managed line segments. 绘制 managed 线段。</summary>
        public void DrawSegments(Mat image, LineSegment[] lines, bool drawArrow = false, Scalar? lineColor = null, int lineThickness = 1)
        {
            ThrowIfDisposed();
            XImgProcCv2.ValidateNotNull(image, nameof(image));
            ValidateDrawSegmentsImage(image, nameof(image));
            float[] values = ToInterleavedLineSegments(lines, nameof(lines));
            if (lines.Length == 0)
            {
                return;
            }

            Scalar color = lineColor ?? new Scalar(0, 0, 255);
            NativeException.ThrowIfError(NativeMethods.XImgProcFastLineDetectorDrawSegmentsArray(
                NativeHandle,
                image.NativeHandle,
                values,
                lines.Length,
                drawArrow ? 1 : 0,
                color.V0,
                color.V1,
                color.V2,
                color.V3,
                lineThickness));
        }

        /// <summary>Releases native resources. 释放 native 资源。</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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

        private static void ValidateDetectImage(Mat image, string parameterName)
        {
            if (image.Empty)
            {
                throw new ArgumentException("FastLineDetector input image must not be empty.", parameterName);
            }

            if (image.Type != MatType.CV_8UC1)
            {
                throw new ArgumentException("FastLineDetector input image must be CV_8UC1.", parameterName);
            }
        }

        private static void ValidateDrawSegmentsImage(Mat image, string parameterName)
        {
            if (image.Empty)
            {
                throw new ArgumentException("FastLineDetector draw image must not be empty.", parameterName);
            }

            int channels = image.Channels;
            if (channels != 1 && channels != 3 && channels != 4)
            {
                throw new ArgumentException("FastLineDetector draw image must have 1, 3, or 4 channels.", parameterName);
            }
        }

        private static void ValidateDrawSegmentsLines(Mat lines, string parameterName)
        {
            if (lines.Empty)
            {
                return;
            }

            bool hasVectorChannels = (lines.Rows == 1 || lines.Cols == 1) && lines.Channels == 4;
            bool hasVectorColumns = lines.Cols == 4 && lines.Channels == 1;
            if (lines.Depth != MatType.CV_32F || (!hasVectorChannels && !hasVectorColumns))
            {
                throw new ArgumentException("FastLineDetector lines must be a CV_32F vector of 4-channel line segments.", parameterName);
            }
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

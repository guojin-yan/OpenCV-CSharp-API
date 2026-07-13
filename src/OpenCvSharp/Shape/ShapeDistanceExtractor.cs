using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Shape
{
    /// <summary>
    /// Base class for OpenCV shape distance extractors.
    /// OpenCV 形状距离提取器基类。
    /// </summary>
    public abstract class ShapeDistanceExtractor : IDisposable
    {
        private NativeShapeDistanceExtractorHandle handle;
        private bool disposed;

        internal ShapeDistanceExtractor(NativeShapeDistanceExtractorHandle handle)
        {
            this.handle = handle;
        }

        /// <summary>Gets whether this extractor has been disposed. 获取对象是否已经释放。</summary>
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
        /// Computes the distance between two contour matrices.
        /// 计算两个轮廓矩阵之间的形状距离。
        /// </summary>
        public float ComputeDistance(Mat contour1, Mat contour2)
        {
            ThrowIfDisposed();
            ValidateNotNull(contour1, nameof(contour1));
            ValidateNotNull(contour2, nameof(contour2));
            ValidateContour(contour1, nameof(contour1));
            ValidateContour(contour2, nameof(contour2));
            NativeException.ThrowIfError(NativeMethods.ShapeDistanceExtractorComputeDistance(
                NativeHandle,
                contour1.NativeHandle,
                contour2.NativeHandle,
                out float distance));
            return distance;
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

        internal static void ValidateNotNull<T>(T value, string parameterName)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        private static void ValidateContour(Mat contour, string parameterName)
        {
            if (contour.Channels != 2 || contour.Cols <= 0)
            {
                throw new ArgumentException("Shape distance contours must have two channels and at least one column.", parameterName);
            }
        }

        /// <summary>Throws when this object has been disposed. 对象释放后抛出异常。</summary>
        protected void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }
}

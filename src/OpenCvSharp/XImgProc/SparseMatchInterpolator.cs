using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.XImgProc
{
    /// <summary>
    /// Base class for ximgproc sparse match interpolators.
    /// ximgproc 稀疏匹配插值器基类。
    /// </summary>
    public abstract class SparseMatchInterpolator : IDisposable
    {
        private bool disposed;

        /// <summary>Gets whether this interpolator has been disposed. 获取插值器是否已经释放。</summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        internal abstract IntPtr NativeHandle { get; }

        /// <summary>Interpolates sparse matches into a dense flow matrix. 将稀疏匹配插值为 dense flow 矩阵。</summary>
        public abstract void Interpolate(Mat fromImage, Mat fromPoints, Mat toImage, Mat toPoints, Mat denseFlow);

        /// <summary>Interpolates sparse matches and returns a new dense flow matrix. 插值稀疏匹配并返回新的 dense flow 矩阵。</summary>
        public Mat Interpolate(Mat fromImage, Mat fromPoints, Mat toImage, Mat toPoints)
        {
            var denseFlow = new Mat();
            try
            {
                Interpolate(fromImage, fromPoints, toImage, toPoints, denseFlow);
                return denseFlow;
            }
            catch
            {
                denseFlow.Dispose();
                throw;
            }
        }

        /// <summary>Interpolates sparse matches from managed point arrays. 使用 managed 点数组插值稀疏匹配。</summary>
        public void Interpolate(Mat fromImage, Point2f[] fromPoints, Mat toImage, Point2f[] toPoints, Mat denseFlow)
        {
            ValidatePointPairs(fromPoints, toPoints);
            using (Mat fromMat = OpenCvSharp.Calib3D.Cv2.ToPointMat(fromPoints))
            using (Mat toMat = OpenCvSharp.Calib3D.Cv2.ToPointMat(toPoints))
            {
                Interpolate(fromImage, fromMat, toImage, toMat, denseFlow);
            }
        }

        /// <summary>Interpolates sparse matches from managed point arrays and returns a new dense flow matrix. 使用 managed 点数组插值并返回新的 dense flow 矩阵。</summary>
        public Mat Interpolate(Mat fromImage, Point2f[] fromPoints, Mat toImage, Point2f[] toPoints)
        {
            var denseFlow = new Mat();
            try
            {
                Interpolate(fromImage, fromPoints, toImage, toPoints, denseFlow);
                return denseFlow;
            }
            catch
            {
                denseFlow.Dispose();
                throw;
            }
        }

        /// <summary>Releases native resources. 释放 native 资源。</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal static void ValidatePointPairs(Point2f[] fromPoints, Point2f[] toPoints)
        {
            PointSetMarshaller.ValidateNotEmpty(fromPoints, nameof(fromPoints));
            PointSetMarshaller.ValidateNotEmpty(toPoints, nameof(toPoints));
            if (fromPoints.Length != toPoints.Length)
            {
                throw new ArgumentException("Point arrays must have the same length.", nameof(toPoints));
            }
        }

        internal void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        /// <summary>Disposes derived resources. 释放派生资源。</summary>
        protected virtual void Dispose(bool disposing)
        {
            disposed = true;
        }
    }
}

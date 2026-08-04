using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Shape
{
    /// <summary>
    /// Base class for OpenCV histogram cost extractors.
    /// OpenCV 直方图代价矩阵提取器基类。
    /// </summary>
    public abstract class HistogramCostExtractor : IDisposable
    {
        private NativeShapeHistogramCostExtractorHandle handle;
        private bool disposed;

        internal HistogramCostExtractor(NativeShapeHistogramCostExtractorHandle handle)
        {
            this.handle = handle;
        }

        /// <summary>Gets whether this extractor has been disposed. 获取对象是否已经释放。</summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        /// <summary>Gets or sets the number of dummy bins. 获取或设置 dummy bin 数量。</summary>
        public int NDummies
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.ShapeHistogramCostExtractorGetNDummies(NativeHandle, out int value));
                return value;
            }
            set
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.ShapeHistogramCostExtractorSetNDummies(NativeHandle, value));
            }
        }

        /// <summary>Gets or sets the default matching cost. 获取或设置默认匹配代价。</summary>
        public float DefaultCost
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.ShapeHistogramCostExtractorGetDefaultCost(NativeHandle, out float value));
                return value;
            }
            set
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.ShapeHistogramCostExtractorSetDefaultCost(NativeHandle, value));
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
        /// Builds a cost matrix between two descriptor matrices.
        /// 在两组描述子矩阵之间构建代价矩阵。
        /// </summary>
        public void BuildCostMatrix(Mat descriptors1, Mat descriptors2, Mat costMatrix)
        {
            ThrowIfDisposed();
            ValidateNotNull(descriptors1, nameof(descriptors1));
            ValidateNotNull(descriptors2, nameof(descriptors2));
            ValidateNotNull(costMatrix, nameof(costMatrix));
            NativeException.ThrowIfError(NativeMethods.ShapeHistogramCostExtractorBuildCostMatrix(
                NativeHandle,
                descriptors1.NativeHandle,
                descriptors2.NativeHandle,
                costMatrix.NativeHandle));
        }

        /// <summary>
        /// Builds and returns a cost matrix between two descriptor matrices.
        /// 构建并返回两组描述子矩阵之间的代价矩阵。
        /// </summary>
        public Mat BuildCostMatrix(Mat descriptors1, Mat descriptors2)
        {
            var costMatrix = new Mat();
            try
            {
                BuildCostMatrix(descriptors1, descriptors2, costMatrix);
                return costMatrix;
            }
            catch
            {
                costMatrix.Dispose();
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

        internal static void ValidateNotNull<T>(T value, string parameterName)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
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

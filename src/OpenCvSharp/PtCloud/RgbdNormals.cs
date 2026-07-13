using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.PtCloud
{
    /// <summary>
    /// Computes normals from organized RGB-D point data.
    /// 从有组织的 RGB-D 点数据计算法线。
    /// </summary>
    public sealed class RgbdNormals : IDisposable
    {
        private const int PropertyRows = 0;
        private const int PropertyCols = 1;
        private const int PropertyWindowSize = 2;
        private const int PropertyDepth = 3;
        private const int PropertyMethod = 4;

        private NativeRgbdNormalsHandle handle;
        private bool disposed;

        /// <summary>
        /// Initializes a normal computer.
        /// 初始化法线计算器。
        /// </summary>
        public RgbdNormals(
            int rows = 0,
            int cols = 0,
            int depth = 0,
            Mat? cameraMatrix = null,
            int windowSize = 5,
            float diffThreshold = 50.0F,
            RgbdNormalsMethod method = RgbdNormalsMethod.Fals)
        {
            NativeException.ThrowIfError(NativeMethods.RgbdNormalsCreate(
                rows,
                cols,
                depth,
                cameraMatrix == null ? IntPtr.Zero : cameraMatrix.NativeHandle,
                windowSize,
                diffThreshold,
                (int)method,
                out IntPtr nativeHandle));
            handle = NativeRgbdNormalsHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Gets whether this object has been disposed. 获取对象是否已经释放。</summary>
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

        /// <summary>Gets or sets the configured depth-image row count. 获取或设置配置的深度图行数。</summary>
        public int Rows
        {
            get { return GetIntProperty(PropertyRows); }
            set { SetIntProperty(PropertyRows, value); }
        }

        /// <summary>Gets or sets the configured depth-image column count. 获取或设置配置的深度图列数。</summary>
        public int Cols
        {
            get { return GetIntProperty(PropertyCols); }
            set { SetIntProperty(PropertyCols, value); }
        }

        /// <summary>Gets or sets the local window size. 获取或设置局部窗口尺寸。</summary>
        public int WindowSize
        {
            get { return GetIntProperty(PropertyWindowSize); }
            set { SetIntProperty(PropertyWindowSize, value); }
        }

        /// <summary>Gets the normal output depth. 获取法线输出深度。</summary>
        public int Depth
        {
            get { return GetIntProperty(PropertyDepth); }
        }

        /// <summary>Gets the normal-estimation method. 获取法线估计方法。</summary>
        public RgbdNormalsMethod Method
        {
            get { return (RgbdNormalsMethod)GetIntProperty(PropertyMethod); }
        }

        /// <summary>Creates a normal computer. 创建法线计算器。</summary>
        public static RgbdNormals Create(
            int rows = 0,
            int cols = 0,
            int depth = 0,
            Mat? cameraMatrix = null,
            int windowSize = 5,
            float diffThreshold = 50.0F,
            RgbdNormalsMethod method = RgbdNormalsMethod.Fals)
        {
            return new RgbdNormals(rows, cols, depth, cameraMatrix, windowSize, diffThreshold, method);
        }

        /// <summary>
        /// Computes normals for a points matrix.
        /// 为点矩阵计算法线。
        /// </summary>
        public void Apply(Mat points, Mat normals)
        {
            ThrowIfDisposed();
            ValidateNotNull(points, nameof(points));
            ValidateNotNull(normals, nameof(normals));
            NativeException.ThrowIfError(NativeMethods.RgbdNormalsApply(NativeHandle, points.NativeHandle, normals.NativeHandle));
        }

        /// <summary>
        /// Computes normals and returns a new matrix.
        /// 计算法线并返回新矩阵。
        /// </summary>
        public Mat Apply(Mat points)
        {
            var normals = new Mat();
            try
            {
                Apply(points, normals);
                return normals;
            }
            catch
            {
                normals.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Prepares cached data used by the normal computer.
        /// 准备用于法线计算的缓存数据。
        /// </summary>
        public void Cache()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.RgbdNormalsCache(NativeHandle));
        }

        /// <summary>
        /// Writes the camera matrix into <paramref name="cameraMatrix"/>.
        /// 将相机矩阵写入 <paramref name="cameraMatrix"/>。
        /// </summary>
        public void GetK(Mat cameraMatrix)
        {
            ThrowIfDisposed();
            ValidateNotNull(cameraMatrix, nameof(cameraMatrix));
            NativeException.ThrowIfError(NativeMethods.RgbdNormalsGetK(NativeHandle, cameraMatrix.NativeHandle));
        }

        /// <summary>
        /// Gets the camera matrix as a new matrix.
        /// 以新矩阵获取相机矩阵。
        /// </summary>
        public Mat GetK()
        {
            var cameraMatrix = new Mat();
            try
            {
                GetK(cameraMatrix);
                return cameraMatrix;
            }
            catch
            {
                cameraMatrix.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Sets the camera matrix.
        /// 设置相机矩阵。
        /// </summary>
        public void SetK(Mat cameraMatrix)
        {
            ThrowIfDisposed();
            ValidateNotNull(cameraMatrix, nameof(cameraMatrix));
            NativeException.ThrowIfError(NativeMethods.RgbdNormalsSetK(NativeHandle, cameraMatrix.NativeHandle));
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

        private int GetIntProperty(int propertyId)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.RgbdNormalsGetIntProperty(NativeHandle, propertyId, out int value));
            return value;
        }

        private void SetIntProperty(int propertyId, int value)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.RgbdNormalsSetIntProperty(NativeHandle, propertyId, value));
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(RgbdNormals));
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
    }
}

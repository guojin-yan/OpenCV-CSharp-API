using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.SurfaceMatching
{
    /// <summary>
    /// Iterative Closest Point registration object from OpenCV surface matching.
    /// OpenCV surface matching 的 ICP 配准对象。
    /// </summary>
    public sealed class Icp : IDisposable
    {
        private NativeSurfaceMatchingIcpHandle handle;
        private bool disposed;

        private Icp(NativeSurfaceMatchingIcpHandle handle)
        {
            this.handle = handle;
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

        /// <summary>Creates an ICP object. 创建 ICP 对象。</summary>
        public static Icp Create(
            int iterations = 250,
            float tolerance = 0.005F,
            float rejectionScale = 2.5F,
            int numLevels = 6,
            IcpSamplingType sampleType = IcpSamplingType.Uniform,
            int numMaxCorr = 1)
        {
            ValidatePositive(iterations, nameof(iterations));
            ValidatePositiveFinite(tolerance, nameof(tolerance));
            ValidatePositiveFinite(rejectionScale, nameof(rejectionScale));
            ValidatePositive(numLevels, nameof(numLevels));
            ValidatePositive(numMaxCorr, nameof(numMaxCorr));

            NativeException.ThrowIfError(NativeMethods.SurfaceMatchingIcpCreate(
                iterations,
                tolerance,
                rejectionScale,
                numLevels,
                (int)sampleType,
                numMaxCorr,
                out IntPtr nativeHandle));
            return new Icp(NativeSurfaceMatchingIcpHandle.FromNativePointer(nativeHandle));
        }

        /// <summary>Registers a source model cloud to a destination scene cloud. 将源模型点云配准到目标场景点云。</summary>
        public IcpRegistrationResult RegisterModelToScene(Mat srcPc, Mat dstPc)
        {
            ThrowIfDisposed();
            ValidateNotNull(srcPc, nameof(srcPc));
            ValidateNotNull(dstPc, nameof(dstPc));
            ValidateSourcePointCloud(srcPc, nameof(srcPc));
            double[] pose = new double[16];
            NativeException.ThrowIfError(NativeMethods.SurfaceMatchingIcpRegisterModelToScene(
                NativeHandle,
                srcPc.NativeHandle,
                dstPc.NativeHandle,
                out int resultCode,
                out double residual,
                pose,
                pose.Length));
            return new IcpRegistrationResult(resultCode, residual, pose);
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

        internal static void ValidatePositive(int value, string parameterName)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Value must be positive.");
            }
        }

        internal static void ValidatePositiveFinite(double value, string parameterName)
        {
            if (value <= 0.0 || double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(parameterName, "Value must be a finite positive value.");
            }
        }

        private static void ValidateSourcePointCloud(Mat value, string parameterName)
        {
            if (value.Rows <= 0)
            {
                throw new ArgumentException("Source point cloud must contain at least one row.", parameterName);
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

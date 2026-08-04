using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Face
{
    /// <summary>
    /// Biologically Inspired Features descriptor helper.
    /// 生物启发特征描述子辅助对象。
    /// </summary>
    public sealed class BIF : IDisposable
    {
        private NativeFaceBIFHandle handle;
        private bool disposed;

        private BIF(IntPtr nativeHandle)
        {
            handle = NativeFaceBIFHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Gets whether this object has been disposed. 获取对象是否已经释放。</summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        /// <summary>Gets the number of bands. 获取 band 数量。</summary>
        public int NumBands
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.FaceBIFGetNumBands(NativeHandle, out int value));
                return value;
            }
        }

        /// <summary>Gets the number of rotations. 获取旋转数量。</summary>
        public int NumRotations
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.FaceBIFGetNumRotations(NativeHandle, out int value));
                return value;
            }
        }

        private IntPtr NativeHandle
        {
            get
            {
                ThrowIfDisposed();
                return handle.DangerousGetHandle();
            }
        }

        /// <summary>Creates a BIF descriptor helper. 创建 BIF 描述子辅助对象。</summary>
        public static BIF Create(int numBands = 8, int numRotations = 12)
        {
            ValidateNumBands(numBands, nameof(numBands));
            ValidateNumRotations(numRotations, nameof(numRotations));
            NativeException.ThrowIfError(NativeMethods.FaceBIFCreate(numBands, numRotations, out IntPtr nativeHandle));
            return new BIF(nativeHandle);
        }

        /// <summary>Computes BIF features into <paramref name="features"/>. 将 BIF 特征计算到 <paramref name="features"/>。</summary>
        public void Compute(Mat image, Mat features)
        {
            ThrowIfDisposed();
            FaceRecognizer.ValidateNotNull(image, nameof(image));
            FaceRecognizer.ValidateNotNull(features, nameof(features));
            ValidateComputeImage(image, nameof(image));
            NativeException.ThrowIfError(NativeMethods.FaceBIFCompute(NativeHandle, image.NativeHandle, features.NativeHandle));
        }

        /// <summary>Computes BIF features as a new matrix. 计算 BIF 特征并返回新矩阵。</summary>
        public Mat Compute(Mat image)
        {
            var features = new Mat();
            try
            {
                Compute(image, features);
                return features;
            }
            catch
            {
                features.Dispose();
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

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        private static void ValidateNumBands(int value, string parameterName)
        {
            if (value < 1 || value > 8)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Number of BIF bands must be between 1 and 8.");
            }
        }

        private static void ValidateNumRotations(int value, string parameterName)
        {
            if (value < 1)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Number of BIF rotations must be greater than zero.");
            }
        }

        private static void ValidateComputeImage(Mat value, string parameterName)
        {
            if (value.Type != MatType.CV_32FC1)
            {
                throw new ArgumentException("BIF input image must be a single-channel 32-bit floating point matrix (CV_32FC1).", parameterName);
            }
        }
    }
}

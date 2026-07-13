using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.XStereo
{
    /// <summary>
    /// Binary SGBM stereo matcher from OpenCV xstereo.
    /// OpenCV xstereo 的二值 SGBM 双目 matcher。
    /// </summary>
    public sealed class StereoBinarySGBM : IDisposable
    {
        private NativeXStereoBinarySGBMHandle handle;
        private bool disposed;

        private StereoBinarySGBM(IntPtr nativeHandle)
        {
            handle = NativeXStereoBinarySGBMHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Gets whether this object has been disposed. 获取对象是否已经释放。</summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        /// <summary>Gets or sets minimum disparity. 获取或设置最小视差。</summary>
        public int MinDisparity
        {
            get { return GetInt(NativeMethods.XStereoBinarySGBMGetMinDisparity); }
            set { SetInt(NativeMethods.XStereoBinarySGBMSetMinDisparity, value); }
        }

        /// <summary>Gets or sets disparity level count. 获取或设置视差级数。</summary>
        public int NumDisparities
        {
            get { return GetInt(NativeMethods.XStereoBinarySGBMGetNumDisparities); }
            set { SetInt(NativeMethods.XStereoBinarySGBMSetNumDisparities, value); }
        }

        /// <summary>Gets or sets block size. 获取或设置块大小。</summary>
        public int BlockSize
        {
            get { return GetInt(NativeMethods.XStereoBinarySGBMGetBlockSize); }
            set { SetInt(NativeMethods.XStereoBinarySGBMSetBlockSize, value); }
        }

        /// <summary>Gets or sets speckle window size. 获取或设置 speckle 窗口大小。</summary>
        public int SpeckleWindowSize
        {
            get { return GetInt(NativeMethods.XStereoBinarySGBMGetSpeckleWindowSize); }
            set { SetInt(NativeMethods.XStereoBinarySGBMSetSpeckleWindowSize, value); }
        }

        /// <summary>Gets or sets speckle range. 获取或设置 speckle 范围。</summary>
        public int SpeckleRange
        {
            get { return GetInt(NativeMethods.XStereoBinarySGBMGetSpeckleRange); }
            set { SetInt(NativeMethods.XStereoBinarySGBMSetSpeckleRange, value); }
        }

        /// <summary>Gets or sets max left-right disparity difference. 获取或设置左右一致性最大视差差。</summary>
        public int Disp12MaxDiff
        {
            get { return GetInt(NativeMethods.XStereoBinarySGBMGetDisp12MaxDiff); }
            set { SetInt(NativeMethods.XStereoBinarySGBMSetDisp12MaxDiff, value); }
        }

        /// <summary>Gets or sets pre-filter cap. 获取或设置预滤波裁剪上限。</summary>
        public int PreFilterCap
        {
            get { return GetInt(NativeMethods.XStereoBinarySGBMGetPreFilterCap); }
            set { SetInt(NativeMethods.XStereoBinarySGBMSetPreFilterCap, value); }
        }

        /// <summary>Gets or sets uniqueness ratio. 获取或设置唯一性比例。</summary>
        public int UniquenessRatio
        {
            get { return GetInt(NativeMethods.XStereoBinarySGBMGetUniquenessRatio); }
            set { SetInt(NativeMethods.XStereoBinarySGBMSetUniquenessRatio, value); }
        }

        /// <summary>Gets or sets the first smoothness penalty. 获取或设置第一个平滑惩罚参数。</summary>
        public int P1
        {
            get { return GetInt(NativeMethods.XStereoBinarySGBMGetP1); }
            set { SetInt(NativeMethods.XStereoBinarySGBMSetP1, value); }
        }

        /// <summary>Gets or sets the second smoothness penalty. 获取或设置第二个平滑惩罚参数。</summary>
        public int P2
        {
            get { return GetInt(NativeMethods.XStereoBinarySGBMGetP2); }
            set { SetInt(NativeMethods.XStereoBinarySGBMSetP2, value); }
        }

        /// <summary>Gets or sets SGBM mode. 获取或设置 SGBM 模式。</summary>
        public StereoBinarySGBMMode Mode
        {
            get { return (StereoBinarySGBMMode)GetInt(NativeMethods.XStereoBinarySGBMGetMode); }
            set
            {
                ThrowIfDisposed();
                XStereoCv2.ValidateStereoBinarySGBMMode(value, nameof(value));
                NativeException.ThrowIfError(NativeMethods.XStereoBinarySGBMSetMode(NativeHandle, (int)value));
            }
        }

        /// <summary>Gets or sets speckle removal algorithm. 获取或设置 speckle 去除算法。</summary>
        public StereoSpeckleRemovalAlgorithm SpeckleRemovalTechnique
        {
            get { return (StereoSpeckleRemovalAlgorithm)GetInt(NativeMethods.XStereoBinarySGBMGetSpeckleRemovalTechnique); }
            set
            {
                ThrowIfDisposed();
                XStereoCv2.ValidateStereoSpeckleRemovalAlgorithm(value, nameof(value));
                NativeException.ThrowIfError(NativeMethods.XStereoBinarySGBMSetSpeckleRemovalTechnique(NativeHandle, (int)value));
            }
        }

        /// <summary>Gets or sets binary kernel type. 获取或设置二值核类型。</summary>
        public CensusTransformType BinaryKernelType
        {
            get { return (CensusTransformType)GetInt(NativeMethods.XStereoBinarySGBMGetBinaryKernelType); }
            set
            {
                ThrowIfDisposed();
                XStereoCv2.ValidateCensusTransformType(value, nameof(value));
                NativeException.ThrowIfError(NativeMethods.XStereoBinarySGBMSetBinaryKernelType(NativeHandle, (int)value));
            }
        }

        /// <summary>Gets or sets sub-pixel interpolation method. 获取或设置亚像素插值方式。</summary>
        public StereoSubPixelInterpolationMethod SubPixelInterpolationMethod
        {
            get { return (StereoSubPixelInterpolationMethod)GetInt(NativeMethods.XStereoBinarySGBMGetSubPixelInterpolationMethod); }
            set
            {
                ThrowIfDisposed();
                XStereoCv2.ValidateStereoSubPixelInterpolationMethod(value, nameof(value));
                NativeException.ThrowIfError(NativeMethods.XStereoBinarySGBMSetSubPixelInterpolationMethod(NativeHandle, (int)value));
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

        /// <summary>Creates a StereoBinarySGBM matcher. 创建 StereoBinarySGBM matcher。</summary>
        public static StereoBinarySGBM Create(
            int minDisparity,
            int numDisparities,
            int blockSize,
            int p1 = 100,
            int p2 = 1000,
            int disp12MaxDiff = 1,
            int preFilterCap = 0,
            int uniquenessRatio = 5,
            int speckleWindowSize = 400,
            int speckleRange = 200,
            StereoBinarySGBMMode mode = StereoBinarySGBMMode.Sgbm)
        {
            XStereoCv2.ValidateStereoBinarySGBMMode(mode, nameof(mode));
            NativeException.ThrowIfError(NativeMethods.XStereoBinarySGBMCreate(
                minDisparity,
                numDisparities,
                blockSize,
                p1,
                p2,
                disp12MaxDiff,
                preFilterCap,
                uniquenessRatio,
                speckleWindowSize,
                speckleRange,
                (int)mode,
                out IntPtr nativeHandle));
            return new StereoBinarySGBM(nativeHandle);
        }

        /// <summary>Computes disparity into caller-owned output. 将视差计算到调用方输出矩阵。</summary>
        public void Compute(Mat left, Mat right, Mat disparity)
        {
            ThrowIfDisposed();
            XStereoCv2.ValidateNotNull(left, nameof(left));
            XStereoCv2.ValidateNotNull(right, nameof(right));
            XStereoCv2.ValidateNotNull(disparity, nameof(disparity));
            NativeException.ThrowIfError(NativeMethods.XStereoBinarySGBMCompute(NativeHandle, left.NativeHandle, right.NativeHandle, disparity.NativeHandle));
        }

        /// <summary>Computes disparity and returns a new matrix. 计算视差并返回新矩阵。</summary>
        public Mat Compute(Mat left, Mat right)
        {
            var disparity = new Mat();
            try
            {
                Compute(left, right, disparity);
                return disparity;
            }
            catch
            {
                disparity.Dispose();
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

        private int GetInt(IntGetter getter)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(getter(NativeHandle, out int value));
            return value;
        }

        private void SetInt(IntSetter setter, int value)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(setter(NativeHandle, value));
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        private delegate int IntGetter(IntPtr handle, out int value);

        private delegate int IntSetter(IntPtr handle, int value);
    }
}

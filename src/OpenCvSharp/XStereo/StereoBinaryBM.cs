using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.XStereo
{
    /// <summary>
    /// Binary block-matching stereo matcher from OpenCV xstereo.
    /// OpenCV xstereo 的二值块匹配双目 matcher。
    /// </summary>
    public sealed class StereoBinaryBM : IDisposable
    {
        private NativeXStereoBinaryBMHandle handle;
        private bool disposed;

        private StereoBinaryBM(IntPtr nativeHandle)
        {
            handle = NativeXStereoBinaryBMHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Gets whether this object has been disposed. 获取对象是否已经释放。</summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        /// <summary>Gets or sets the minimum disparity. 获取或设置最小视差。</summary>
        public int MinDisparity
        {
            get { return GetInt(NativeMethods.XStereoBinaryBMGetMinDisparity); }
            set { SetInt(NativeMethods.XStereoBinaryBMSetMinDisparity, value); }
        }

        /// <summary>Gets or sets disparity level count. 获取或设置视差级数。</summary>
        public int NumDisparities
        {
            get { return GetInt(NativeMethods.XStereoBinaryBMGetNumDisparities); }
            set { SetInt(NativeMethods.XStereoBinaryBMSetNumDisparities, value); }
        }

        /// <summary>Gets or sets the block size. 获取或设置块大小。</summary>
        public int BlockSize
        {
            get { return GetInt(NativeMethods.XStereoBinaryBMGetBlockSize); }
            set { SetInt(NativeMethods.XStereoBinaryBMSetBlockSize, value); }
        }

        /// <summary>Gets or sets speckle window size. 获取或设置 speckle 窗口大小。</summary>
        public int SpeckleWindowSize
        {
            get { return GetInt(NativeMethods.XStereoBinaryBMGetSpeckleWindowSize); }
            set { SetInt(NativeMethods.XStereoBinaryBMSetSpeckleWindowSize, value); }
        }

        /// <summary>Gets or sets speckle range. 获取或设置 speckle 范围。</summary>
        public int SpeckleRange
        {
            get { return GetInt(NativeMethods.XStereoBinaryBMGetSpeckleRange); }
            set { SetInt(NativeMethods.XStereoBinaryBMSetSpeckleRange, value); }
        }

        /// <summary>Gets or sets max left-right disparity difference. 获取或设置左右一致性最大视差差。</summary>
        public int Disp12MaxDiff
        {
            get { return GetInt(NativeMethods.XStereoBinaryBMGetDisp12MaxDiff); }
            set { SetInt(NativeMethods.XStereoBinaryBMSetDisp12MaxDiff, value); }
        }

        /// <summary>Gets or sets pre-filter type. 获取或设置预滤波类型。</summary>
        public StereoBinaryBMPreFilterType PreFilterType
        {
            get { return (StereoBinaryBMPreFilterType)GetInt(NativeMethods.XStereoBinaryBMGetPreFilterType); }
            set
            {
                ThrowIfDisposed();
                XStereoCv2.ValidateStereoBinaryBMPreFilterType(value, nameof(value));
                NativeException.ThrowIfError(NativeMethods.XStereoBinaryBMSetPreFilterType(NativeHandle, (int)value));
            }
        }

        /// <summary>Gets or sets pre-filter size. 获取或设置预滤波尺寸。</summary>
        public int PreFilterSize
        {
            get { return GetInt(NativeMethods.XStereoBinaryBMGetPreFilterSize); }
            set { SetInt(NativeMethods.XStereoBinaryBMSetPreFilterSize, value); }
        }

        /// <summary>Gets or sets pre-filter cap. 获取或设置预滤波裁剪上限。</summary>
        public int PreFilterCap
        {
            get { return GetInt(NativeMethods.XStereoBinaryBMGetPreFilterCap); }
            set { SetInt(NativeMethods.XStereoBinaryBMSetPreFilterCap, value); }
        }

        /// <summary>Gets or sets texture threshold. 获取或设置纹理阈值。</summary>
        public int TextureThreshold
        {
            get { return GetInt(NativeMethods.XStereoBinaryBMGetTextureThreshold); }
            set { SetInt(NativeMethods.XStereoBinaryBMSetTextureThreshold, value); }
        }

        /// <summary>Gets or sets uniqueness ratio. 获取或设置唯一性比例。</summary>
        public int UniquenessRatio
        {
            get { return GetInt(NativeMethods.XStereoBinaryBMGetUniquenessRatio); }
            set { SetInt(NativeMethods.XStereoBinaryBMSetUniquenessRatio, value); }
        }

        /// <summary>Gets or sets smaller block size. 获取或设置较小块大小。</summary>
        public int SmallerBlockSize
        {
            get { return GetInt(NativeMethods.XStereoBinaryBMGetSmallerBlockSize); }
            set { SetInt(NativeMethods.XStereoBinaryBMSetSmallerBlockSize, value); }
        }

        /// <summary>Gets or sets the scale factor. 获取或设置缩放因子。</summary>
        public int ScaleFactor
        {
            get { return GetInt(NativeMethods.XStereoBinaryBMGetScaleFactor); }
            set { SetInt(NativeMethods.XStereoBinaryBMSetScaleFactor, value); }
        }

        /// <summary>Gets or sets speckle removal algorithm. 获取或设置 speckle 去除算法。</summary>
        public StereoSpeckleRemovalAlgorithm SpeckleRemovalTechnique
        {
            get { return (StereoSpeckleRemovalAlgorithm)GetInt(NativeMethods.XStereoBinaryBMGetSpeckleRemovalTechnique); }
            set
            {
                ThrowIfDisposed();
                XStereoCv2.ValidateStereoSpeckleRemovalAlgorithm(value, nameof(value));
                NativeException.ThrowIfError(NativeMethods.XStereoBinaryBMSetSpeckleRemovalTechnique(NativeHandle, (int)value));
            }
        }

        /// <summary>Gets or sets whether pre-filtering is used. 获取或设置是否使用预滤波。</summary>
        public bool UsePrefilter
        {
            get { return GetInt(NativeMethods.XStereoBinaryBMGetUsePrefilter) != 0; }
            set { SetInt(NativeMethods.XStereoBinaryBMSetUsePrefilter, value ? 1 : 0); }
        }

        /// <summary>Gets or sets binary kernel type. 获取或设置二值核类型。</summary>
        public CensusTransformType BinaryKernelType
        {
            get { return (CensusTransformType)GetInt(NativeMethods.XStereoBinaryBMGetBinaryKernelType); }
            set
            {
                ThrowIfDisposed();
                XStereoCv2.ValidateCensusTransformType(value, nameof(value));
                NativeException.ThrowIfError(NativeMethods.XStereoBinaryBMSetBinaryKernelType(NativeHandle, (int)value));
            }
        }

        /// <summary>Gets or sets aggregation window size. 获取或设置聚合窗口大小。</summary>
        public int AggregationWindowSize
        {
            get { return GetInt(NativeMethods.XStereoBinaryBMGetAggregationWindowSize); }
            set { SetInt(NativeMethods.XStereoBinaryBMSetAggregationWindowSize, value); }
        }

        internal IntPtr NativeHandle
        {
            get
            {
                ThrowIfDisposed();
                return handle.DangerousGetHandle();
            }
        }

        /// <summary>Creates a StereoBinaryBM matcher. 创建 StereoBinaryBM matcher。</summary>
        public static StereoBinaryBM Create(int numDisparities = 0, int blockSize = 9)
        {
            NativeException.ThrowIfError(NativeMethods.XStereoBinaryBMCreate(numDisparities, blockSize, out IntPtr nativeHandle));
            return new StereoBinaryBM(nativeHandle);
        }

        /// <summary>Computes disparity into caller-owned output. 将视差计算到调用方输出矩阵。</summary>
        public void Compute(Mat left, Mat right, Mat disparity)
        {
            ThrowIfDisposed();
            XStereoCv2.ValidateNotNull(left, nameof(left));
            XStereoCv2.ValidateNotNull(right, nameof(right));
            XStereoCv2.ValidateNotNull(disparity, nameof(disparity));
            NativeException.ThrowIfError(NativeMethods.XStereoBinaryBMCompute(NativeHandle, left.NativeHandle, right.NativeHandle, disparity.NativeHandle));
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

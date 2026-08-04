using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Calib3D
{
    /// <summary>
    /// Semi-global block-matching stereo correspondence algorithm compatible with OpenCV
    /// <c>cv::StereoSGBM</c>.
    /// 与 OpenCV <c>cv::StereoSGBM</c> 兼容的半全局块匹配双目视差算法。
    /// </summary>
    public sealed class StereoSGBM : IDisposable
    {
        /// <summary>
        /// Number of fractional bits used by OpenCV fixed-point disparity maps.
        /// OpenCV 定点视差图使用的小数位数。
        /// </summary>
        public const int DispShift = 4;

        /// <summary>
        /// Scale factor used by OpenCV fixed-point disparity maps.
        /// OpenCV 定点视差图使用的缩放因子。
        /// </summary>
        public const int DispScale = 1 << DispShift;

        private NativeStereoSGBMHandle handle;
        private bool disposed;

        private StereoSGBM(IntPtr nativeHandle)
        {
            handle = NativeStereoSGBMHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>
        /// Gets a value indicating whether this object has been disposed.
        /// 获取此对象是否已经释放。
        /// </summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        /// <summary>Gets or sets the minimum disparity. 获取或设置最小视差。</summary>
        public int MinDisparity
        {
            get { return GetInt(NativeMethods.StereoSGBMGetMinDisparity); }
            set { SetInt(NativeMethods.StereoSGBMSetMinDisparity, value); }
        }

        /// <summary>Gets or sets the disparity search range. 获取或设置视差搜索范围。</summary>
        public int NumDisparities
        {
            get { return GetInt(NativeMethods.StereoSGBMGetNumDisparities); }
            set { SetInt(NativeMethods.StereoSGBMSetNumDisparities, value); }
        }

        /// <summary>Gets or sets the matched block size. 获取或设置匹配块大小。</summary>
        public int BlockSize
        {
            get { return GetInt(NativeMethods.StereoSGBMGetBlockSize); }
            set { SetInt(NativeMethods.StereoSGBMSetBlockSize, value); }
        }

        /// <summary>Gets or sets the speckle filtering window size. 获取或设置斑点过滤窗口大小。</summary>
        public int SpeckleWindowSize
        {
            get { return GetInt(NativeMethods.StereoSGBMGetSpeckleWindowSize); }
            set { SetInt(NativeMethods.StereoSGBMSetSpeckleWindowSize, value); }
        }

        /// <summary>Gets or sets the speckle disparity range. 获取或设置斑点视差范围。</summary>
        public int SpeckleRange
        {
            get { return GetInt(NativeMethods.StereoSGBMGetSpeckleRange); }
            set { SetInt(NativeMethods.StereoSGBMSetSpeckleRange, value); }
        }

        /// <summary>Gets or sets the left-right consistency threshold. 获取或设置左右一致性阈值。</summary>
        public int Disp12MaxDiff
        {
            get { return GetInt(NativeMethods.StereoSGBMGetDisp12MaxDiff); }
            set { SetInt(NativeMethods.StereoSGBMSetDisp12MaxDiff, value); }
        }

        /// <summary>Gets or sets the pre-filter clipping cap. 获取或设置预滤波裁剪上限。</summary>
        public int PreFilterCap
        {
            get { return GetInt(NativeMethods.StereoSGBMGetPreFilterCap); }
            set { SetInt(NativeMethods.StereoSGBMSetPreFilterCap, value); }
        }

        /// <summary>Gets or sets the uniqueness ratio. 获取或设置唯一性比例。</summary>
        public int UniquenessRatio
        {
            get { return GetInt(NativeMethods.StereoSGBMGetUniquenessRatio); }
            set { SetInt(NativeMethods.StereoSGBMSetUniquenessRatio, value); }
        }

        /// <summary>Gets or sets the small disparity-change penalty. 获取或设置小视差变化惩罚。</summary>
        public int P1
        {
            get { return GetInt(NativeMethods.StereoSGBMGetP1); }
            set { SetInt(NativeMethods.StereoSGBMSetP1, value); }
        }

        /// <summary>Gets or sets the large disparity-change penalty. 获取或设置大视差变化惩罚。</summary>
        public int P2
        {
            get { return GetInt(NativeMethods.StereoSGBMGetP2); }
            set { SetInt(NativeMethods.StereoSGBMSetP2, value); }
        }

        /// <summary>Gets or sets the dynamic-programming mode. 获取或设置动态规划模式。</summary>
        public StereoSGBMMode Mode
        {
            get { return (StereoSGBMMode)GetInt(NativeMethods.StereoSGBMGetMode); }
            set
            {
                ValidateMode(value, nameof(value));
                SetInt(NativeMethods.StereoSGBMSetMode, (int)value);
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
        /// Creates a StereoSGBM object.
        /// 创建 StereoSGBM 对象。
        /// </summary>
        public static StereoSGBM Create(
            int minDisparity = 0,
            int numDisparities = 16,
            int blockSize = 3,
            int p1 = 0,
            int p2 = 0,
            int disp12MaxDiff = 0,
            int preFilterCap = 0,
            int uniquenessRatio = 0,
            int speckleWindowSize = 0,
            int speckleRange = 0,
            StereoSGBMMode mode = StereoSGBMMode.SGBM)
        {
            ValidateMode(mode, nameof(mode));
            NativeException.ThrowIfError(NativeMethods.StereoSGBMCreate(
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
            return new StereoSGBM(nativeHandle);
        }

        /// <summary>
        /// Computes a fixed-point disparity map from a rectified stereo pair.
        /// 根据校正后的双目图像对计算定点视差图。
        /// </summary>
        public void Compute(Mat left, Mat right, Mat disparity)
        {
            ThrowIfDisposed();
            ValidateInput(left, nameof(left));
            ValidateInput(right, nameof(right));
            ValidateNotNull(disparity, nameof(disparity));
            if (left.Rows != right.Rows || left.Cols != right.Cols || left.Type != right.Type)
            {
                throw new ArgumentException("Left and right images must have identical size and type.", nameof(right));
            }

            IntPtr leftHandle = left.NativeHandle;
            IntPtr rightHandle = right.NativeHandle;
            IntPtr disparityHandle = disparity.NativeHandle;
            if (ReferenceEquals(left, disparity) || leftHandle == disparityHandle)
            {
                throw new ArgumentException("The output disparity must not alias the left image.", nameof(disparity));
            }
            if (ReferenceEquals(right, disparity) || rightHandle == disparityHandle)
            {
                throw new ArgumentException("The output disparity must not alias the right image.", nameof(disparity));
            }

            NativeException.ThrowIfError(NativeMethods.StereoSGBMCompute(
                NativeHandle,
                leftHandle,
                rightHandle,
                disparityHandle));
        }

        /// <summary>
        /// Computes and returns a fixed-point disparity map.
        /// 计算并返回定点视差图。
        /// </summary>
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

        /// <summary>Releases the native StereoSGBM object. 释放 native StereoSGBM 对象。</summary>
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

        private int GetInt(IntGetter getter)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(getter(NativeHandle, out int value));
            return value;
        }

        private void SetInt(Func<IntPtr, int, int> setter, int value)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(setter(NativeHandle, value));
        }

        private static void ValidateInput(Mat value, string parameterName)
        {
            ValidateNotNull(value, parameterName);
            if (value.Empty)
            {
                throw new ArgumentException("Stereo input image cannot be empty.", parameterName);
            }
            if (value.Type != MatType.CV_8UC1 && value.Type != MatType.CV_8UC3)
            {
                throw new ArgumentException("StereoSGBM input must be CV_8UC1 or CV_8UC3.", parameterName);
            }
        }

        private static void ValidateMode(StereoSGBMMode value, string parameterName)
        {
            if (value != StereoSGBMMode.SGBM &&
                value != StereoSGBMMode.HH &&
                value != StereoSGBMMode.SGBM3Way &&
                value != StereoSGBMMode.HH4)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Unsupported StereoSGBM mode.");
            }
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

        private delegate int IntGetter(IntPtr handle, out int value);
    }
}

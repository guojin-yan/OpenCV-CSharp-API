using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Calib3D
{
    /// <summary>
    /// Owned generic stereo matcher returned by matcher bridge factories.
    /// 由 matcher bridge 工厂返回并独立持有所有权的通用双目匹配器。
    /// </summary>
    public sealed class StereoMatcher : IDisposable
    {
        private NativeStereoMatcherHandle handle;
        private readonly bool supportsColor;
        private bool disposed;

        private StereoMatcher(IntPtr nativeHandle, bool supportsColor)
        {
            handle = NativeStereoMatcherHandle.FromNativePointer(nativeHandle);
            this.supportsColor = supportsColor;
        }

        /// <summary>Gets whether this matcher has been disposed. 获取此 matcher 是否已经释放。</summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        /// <summary>Gets or sets the minimum disparity. 获取或设置最小视差。</summary>
        public int MinDisparity
        {
            get { return GetInt(NativeMethods.StereoMatcherGetMinDisparity); }
            set { SetInt(NativeMethods.StereoMatcherSetMinDisparity, value); }
        }

        /// <summary>Gets or sets the disparity search range. 获取或设置视差搜索范围。</summary>
        public int NumDisparities
        {
            get { return GetInt(NativeMethods.StereoMatcherGetNumDisparities); }
            set { SetInt(NativeMethods.StereoMatcherSetNumDisparities, value); }
        }

        /// <summary>Gets or sets the matched block size. 获取或设置匹配块大小。</summary>
        public int BlockSize
        {
            get { return GetInt(NativeMethods.StereoMatcherGetBlockSize); }
            set { SetInt(NativeMethods.StereoMatcherSetBlockSize, value); }
        }

        /// <summary>Gets or sets the speckle filtering window size. 获取或设置斑点过滤窗口大小。</summary>
        public int SpeckleWindowSize
        {
            get { return GetInt(NativeMethods.StereoMatcherGetSpeckleWindowSize); }
            set { SetInt(NativeMethods.StereoMatcherSetSpeckleWindowSize, value); }
        }

        /// <summary>Gets or sets the speckle disparity range. 获取或设置斑点视差范围。</summary>
        public int SpeckleRange
        {
            get { return GetInt(NativeMethods.StereoMatcherGetSpeckleRange); }
            set { SetInt(NativeMethods.StereoMatcherSetSpeckleRange, value); }
        }

        /// <summary>Gets or sets the left-right consistency threshold. 获取或设置左右一致性阈值。</summary>
        public int Disp12MaxDiff
        {
            get { return GetInt(NativeMethods.StereoMatcherGetDisp12MaxDiff); }
            set { SetInt(NativeMethods.StereoMatcherSetDisp12MaxDiff, value); }
        }

        internal IntPtr NativeHandle
        {
            get
            {
                ThrowIfDisposed();
                return handle.DangerousGetHandle();
            }
        }

        internal bool SupportsColor
        {
            get
            {
                ThrowIfDisposed();
                return supportsColor;
            }
        }

        internal static StereoMatcher FromNativePointer(IntPtr nativeHandle, bool supportsColor)
        {
            return new StereoMatcher(nativeHandle, supportsColor);
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

            NativeException.ThrowIfError(NativeMethods.StereoMatcherCompute(
                NativeHandle,
                leftHandle,
                rightHandle,
                disparityHandle));
        }

        /// <summary>Computes and returns a fixed-point disparity map. 计算并返回定点视差图。</summary>
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

        /// <summary>Releases the native matcher. 释放 native matcher。</summary>
        public void Dispose()
        {
            if (!disposed)
            {
                handle.Dispose();
                disposed = true;
            }

            GC.SuppressFinalize(this);
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

        private void ValidateInput(Mat value, string parameterName)
        {
            ValidateNotNull(value, parameterName);
            if (value.Empty)
            {
                throw new ArgumentException("Stereo input image cannot be empty.", parameterName);
            }

            bool supported = value.Type == MatType.CV_8UC1 ||
                (supportsColor && value.Type == MatType.CV_8UC3);
            if (!supported)
            {
                throw new ArgumentException(
                    supportsColor
                        ? "Stereo matcher input must be CV_8UC1 or CV_8UC3."
                        : "Stereo matcher input must be CV_8UC1.",
                    parameterName);
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

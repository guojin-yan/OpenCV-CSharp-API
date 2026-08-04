using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.ImgProc;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Stitching
{
    /// <summary>
    /// High-level image stitcher compatible with OpenCV <c>cv::Stitcher</c>.
    /// 与 OpenCV <c>cv::Stitcher</c> 兼容的高级图像拼接器。
    /// </summary>
    public sealed class Stitcher : IDisposable
    {
        private const int DoublePropertyRegistrationResol = 0;
        private const int DoublePropertySeamEstimationResol = 1;
        private const int DoublePropertyCompositingResol = 2;
        private const int DoublePropertyPanoConfidenceThresh = 3;
        private const int DoublePropertyWorkScale = 4;

        private const int IntPropertyWaveCorrection = 0;
        private const int IntPropertyInterpolationFlags = 1;
        private const int IntPropertyWaveCorrectKind = 2;

        private NativeStitcherHandle handle;
        private bool disposed;

        /// <summary>
        /// Creates a stitcher with the specified mode.
        /// 使用指定模式创建拼接器。
        /// </summary>
        public Stitcher(StitcherMode mode = StitcherMode.Panorama)
            : this(CreateNative(mode))
        {
        }

        private Stitcher(IntPtr nativeHandle)
        {
            handle = NativeStitcherHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>
        /// Gets whether this stitcher has been disposed.
        /// 获取拼接器是否已经释放。
        /// </summary>
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
        /// Creates a stitcher with the specified mode.
        /// 使用指定模式创建拼接器。
        /// </summary>
        public static Stitcher Create(StitcherMode mode = StitcherMode.Panorama)
        {
            return new Stitcher(mode);
        }

        /// <summary>Gets or sets registration resolution in megapixels. 获取或设置配准分辨率（百万像素）。</summary>
        public double RegistrationResol
        {
            get { return GetDoubleProperty(DoublePropertyRegistrationResol); }
            set { SetDoubleProperty(DoublePropertyRegistrationResol, value); }
        }

        /// <summary>Gets or sets seam estimation resolution in megapixels. 获取或设置接缝估计分辨率（百万像素）。</summary>
        public double SeamEstimationResol
        {
            get { return GetDoubleProperty(DoublePropertySeamEstimationResol); }
            set { SetDoubleProperty(DoublePropertySeamEstimationResol, value); }
        }

        /// <summary>Gets or sets compositing resolution in megapixels. 获取或设置合成分辨率（百万像素）。</summary>
        public double CompositingResol
        {
            get { return GetDoubleProperty(DoublePropertyCompositingResol); }
            set { SetDoubleProperty(DoublePropertyCompositingResol, value); }
        }

        /// <summary>Gets or sets panorama confidence threshold. 获取或设置全景置信度阈值。</summary>
        public double PanoConfidenceThresh
        {
            get { return GetDoubleProperty(DoublePropertyPanoConfidenceThresh); }
            set { SetDoubleProperty(DoublePropertyPanoConfidenceThresh, value); }
        }

        /// <summary>Gets the work scale estimated by OpenCV. 获取 OpenCV 估计的工作尺度。</summary>
        public double WorkScale
        {
            get { return GetDoubleProperty(DoublePropertyWorkScale); }
        }

        /// <summary>Gets or sets whether wave correction is enabled. 获取或设置是否启用波形校正。</summary>
        public bool WaveCorrection
        {
            get { return GetIntProperty(IntPropertyWaveCorrection) != 0; }
            set { SetIntProperty(IntPropertyWaveCorrection, value ? 1 : 0); }
        }

        /// <summary>Gets or sets interpolation flags. 获取或设置插值标志。</summary>
        public InterpolationFlags InterpolationFlags
        {
            get { return (InterpolationFlags)GetIntProperty(IntPropertyInterpolationFlags); }
            set { SetIntProperty(IntPropertyInterpolationFlags, (int)value); }
        }

        /// <summary>Gets or sets wave correction kind. 获取或设置波形校正类型。</summary>
        public WaveCorrectKind WaveCorrectKind
        {
            get { return (WaveCorrectKind)GetIntProperty(IntPropertyWaveCorrectKind); }
            set
            {
                ThrowIfDisposed();
                ValidateWaveCorrectKind(value, nameof(value));
                NativeException.ThrowIfError(NativeMethods.StitcherSetIntProperty(NativeHandle, IntPropertyWaveCorrectKind, (int)value));
            }
        }

        /// <summary>
        /// Estimates image transforms.
        /// 估计图像变换。
        /// </summary>
        public StitcherStatus EstimateTransform(Mat[] images)
        {
            return EstimateTransform(images, null);
        }

        /// <summary>
        /// Estimates image transforms with masks.
        /// 使用掩码估计图像变换。
        /// </summary>
        public StitcherStatus EstimateTransform(Mat[] images, Mat[]? masks)
        {
            ThrowIfDisposed();
            IntPtr[] imageHandles = ToHandleArray(images, nameof(images), allowEmpty: false);
            IntPtr[] maskHandles = ToOptionalMaskHandleArray(masks, imageHandles.Length);
            NativeException.ThrowIfError(NativeMethods.StitcherEstimateTransform(
                NativeHandle,
                imageHandles,
                imageHandles.Length,
                maskHandles,
                maskHandles.Length,
                out int status));
            return (StitcherStatus)status;
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Estimates image transforms from span-backed image input.
        /// 使用 Span 支持的图像输入估计图像变换。
        /// </summary>
        public StitcherStatus EstimateTransform(ReadOnlySpan<Mat> images)
        {
            return EstimateTransform(images.ToArray());
        }

        /// <summary>
        /// Estimates image transforms from span-backed image and mask input.
        /// 使用 Span 支持的图像与掩码输入估计图像变换。
        /// </summary>
        public StitcherStatus EstimateTransform(ReadOnlySpan<Mat> images, ReadOnlySpan<Mat> masks)
        {
            return EstimateTransform(images.ToArray(), masks.ToArray());
        }
#endif

        /// <summary>
        /// Composes a panorama after transform estimation.
        /// 在变换估计后合成全景图。
        /// </summary>
        public StitcherStatus ComposePanorama(Mat pano)
        {
            ThrowIfDisposed();
            ValidateNotNull(pano, nameof(pano));
            NativeException.ThrowIfError(NativeMethods.StitcherComposePanorama(NativeHandle, pano.NativeHandle, out int status));
            return (StitcherStatus)status;
        }

        /// <summary>
        /// Composes a panorama from images after transform estimation.
        /// 在变换估计后使用图像合成全景图。
        /// </summary>
        public StitcherStatus ComposePanorama(Mat[] images, Mat pano)
        {
            ThrowIfDisposed();
            IntPtr[] imageHandles = ToHandleArray(images, nameof(images), allowEmpty: false);
            ValidateNotNull(pano, nameof(pano));
            NativeException.ThrowIfError(NativeMethods.StitcherComposePanoramaImages(
                NativeHandle,
                imageHandles,
                imageHandles.Length,
                pano.NativeHandle,
                out int status));
            return (StitcherStatus)status;
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Composes a panorama from span-backed image input.
        /// 使用 Span 支持的图像输入合成全景图。
        /// </summary>
        public StitcherStatus ComposePanorama(ReadOnlySpan<Mat> images, Mat pano)
        {
            return ComposePanorama(images.ToArray(), pano);
        }
#endif

        /// <summary>
        /// Stitches images into a panorama.
        /// 将图像拼接为全景图。
        /// </summary>
        public StitcherStatus Stitch(Mat[] images, Mat pano)
        {
            return Stitch(images, null, pano);
        }

        /// <summary>
        /// Stitches images into a panorama with masks.
        /// 使用掩码将图像拼接为全景图。
        /// </summary>
        public StitcherStatus Stitch(Mat[] images, Mat[]? masks, Mat pano)
        {
            ThrowIfDisposed();
            IntPtr[] imageHandles = ToHandleArray(images, nameof(images), allowEmpty: false);
            IntPtr[] maskHandles = ToOptionalMaskHandleArray(masks, imageHandles.Length);
            ValidateNotNull(pano, nameof(pano));
            NativeException.ThrowIfError(NativeMethods.StitcherStitch(
                NativeHandle,
                imageHandles,
                imageHandles.Length,
                maskHandles,
                maskHandles.Length,
                pano.NativeHandle,
                out int status));
            return (StitcherStatus)status;
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Stitches span-backed images into a panorama.
        /// 将 Span 支持的图像拼接为全景图。
        /// </summary>
        public StitcherStatus Stitch(ReadOnlySpan<Mat> images, Mat pano)
        {
            return Stitch(images.ToArray(), pano);
        }

        /// <summary>
        /// Stitches span-backed images into a panorama with masks.
        /// 使用 Span 支持的图像和掩码拼接为全景图。
        /// </summary>
        public StitcherStatus Stitch(ReadOnlySpan<Mat> images, ReadOnlySpan<Mat> masks, Mat pano)
        {
            return Stitch(images.ToArray(), masks.ToArray(), pano);
        }
#endif

        /// <summary>
        /// Gets indices of input images used in the panorama.
        /// 获取参与全景图的输入图像索引。
        /// </summary>
        public int[] GetComponent()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.StitcherGetComponentCount(NativeHandle, out int componentCount));
            if (componentCount <= 0)
            {
                return Array.Empty<int>();
            }

            var components = new int[componentCount];
            NativeException.ThrowIfError(NativeMethods.StitcherGetComponentFill(NativeHandle, components, components.Length, out int written));
            return TrimArray(components, written);
        }

        /// <summary>
        /// Gets estimated camera parameters.
        /// 获取估计的相机参数。
        /// </summary>
        public StitcherCameraParams[] GetCameras()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.StitcherGetCamerasCount(NativeHandle, out int cameraCount));
            if (cameraCount <= 0)
            {
                return Array.Empty<StitcherCameraParams>();
            }

            var nativeCameras = new NativeMethods.StitchingCameraParamsNative[cameraCount];
            NativeException.ThrowIfError(NativeMethods.StitcherGetCamerasFill(NativeHandle, nativeCameras, nativeCameras.Length, out int written));
            int count = Math.Max(0, Math.Min(written, nativeCameras.Length));
            var cameras = new StitcherCameraParams[count];
            for (int i = 0; i < count; i++)
            {
                cameras[i] = new StitcherCameraParams(
                    nativeCameras[i].Focal,
                    nativeCameras[i].Aspect,
                    nativeCameras[i].Ppx,
                    nativeCameras[i].Ppy,
                    new Mat(nativeCameras[i].R),
                    new Mat(nativeCameras[i].T));
            }

            return cameras;
        }

        /// <summary>
        /// Writes the panorama result mask.
        /// 写入全景图结果掩码。
        /// </summary>
        public void GetResultMask(Mat resultMask)
        {
            ThrowIfDisposed();
            ValidateNotNull(resultMask, nameof(resultMask));
            NativeException.ThrowIfError(NativeMethods.StitcherGetResultMask(NativeHandle, resultMask.NativeHandle));
        }

        /// <summary>
        /// Returns the panorama result mask.
        /// 返回全景图结果掩码。
        /// </summary>
        public Mat GetResultMask()
        {
            var result = new Mat();
            try
            {
                GetResultMask(result);
                return result;
            }
            catch
            {
                result.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Releases native resources.
        /// 释放 native 资源。
        /// </summary>
        public void Dispose()
        {
            if (!disposed)
            {
                handle.Dispose();
                disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        private static IntPtr CreateNative(StitcherMode mode)
        {
            ValidateStitcherMode(mode, nameof(mode));
            NativeException.ThrowIfError(NativeMethods.StitcherCreate((int)mode, out IntPtr nativeHandle));
            return nativeHandle;
        }

        private double GetDoubleProperty(int propertyId)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.StitcherGetDoubleProperty(NativeHandle, propertyId, out double value));
            return value;
        }

        private void SetDoubleProperty(int propertyId, double value)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.StitcherSetDoubleProperty(NativeHandle, propertyId, value));
        }

        private int GetIntProperty(int propertyId)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.StitcherGetIntProperty(NativeHandle, propertyId, out int value));
            return value;
        }

        private void SetIntProperty(int propertyId, int value)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.StitcherSetIntProperty(NativeHandle, propertyId, value));
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(Stitcher));
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

        private static void ValidateStitcherMode(StitcherMode value, string parameterName)
        {
            if (value != StitcherMode.Panorama && value != StitcherMode.Scans)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Unsupported stitcher mode.");
            }
        }

        private static void ValidateWaveCorrectKind(WaveCorrectKind value, string parameterName)
        {
            if (value != WaveCorrectKind.Horizontal && value != WaveCorrectKind.Vertical && value != WaveCorrectKind.Auto)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Unsupported wave correction kind.");
            }
        }

        private static IntPtr[] ToHandleArray(Mat[] values, string parameterName, bool allowEmpty)
        {
            ValidateNotNull(values, parameterName);
            if (!allowEmpty && values.Length == 0)
            {
                throw new ArgumentException("At least one image is required.", parameterName);
            }

            var handles = new IntPtr[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == null)
                {
                    throw new ArgumentNullException(parameterName);
                }

                handles[i] = values[i].NativeHandle;
            }

            return handles;
        }

        private static IntPtr[] ToOptionalMaskHandleArray(Mat[]? masks, int imageCount)
        {
            if (masks == null)
            {
                return Array.Empty<IntPtr>();
            }

            if (masks.Length != imageCount)
            {
                throw new ArgumentException("Mask count must match image count.", nameof(masks));
            }

            return ToHandleArray(masks, nameof(masks), allowEmpty: true);
        }

        private static int[] TrimArray(int[] values, int count)
        {
            int length = Math.Max(0, Math.Min(count, values.Length));
            if (length == values.Length)
            {
                return values;
            }

            var result = new int[length];
            Array.Copy(values, result, length);
            return result;
        }
    }
}

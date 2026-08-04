using System;
using System.Globalization;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Features2D
{
    /// <summary>
    /// Provides BRISK keypoint detection and binary descriptor extraction compatible with <c>cv::xfeatures2d::BRISK</c>.
    /// 提供与 OpenCV <c>cv::xfeatures2d::BRISK</c> 兼容的 BRISK 关键点检测和二进制描述子提取能力。
    /// </summary>
    public sealed unsafe class BRISK : Feature2D
    {
        private NativeBriskHandle handle;
        private bool disposed;

        private BRISK(IntPtr nativeHandle)
        {
            handle = NativeBriskHandle.FromNativePointer(nativeHandle);
        }

        /// <inheritdoc/>
        public override bool IsDisposed
        {
            get { return disposed; }
        }

        /// <summary>
        /// Gets or sets the AGAST detection threshold score.
        /// 获取或设置 AGAST 检测阈值得分。
        /// </summary>
        public int Threshold
        {
            get { return GetInt(NativeMethods.Features2DBriskGetThreshold); }
            set { SetInt(NativeMethods.Features2DBriskSetThreshold, value); }
        }

        /// <summary>
        /// Gets or sets the number of detection octaves.
        /// 获取或设置检测 octave 数量。
        /// </summary>
        public int Octaves
        {
            get { return GetInt(NativeMethods.Features2DBriskGetOctaves); }
            set { SetInt(NativeMethods.Features2DBriskSetOctaves, value); }
        }

        /// <summary>
        /// Gets or sets the descriptor sampling pattern scale.
        /// 获取或设置描述子采样模式缩放比例。
        /// </summary>
        public float PatternScale
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.Features2DBriskGetPatternScale(NativeHandle, out float value));
                return value;
            }

            set
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.Features2DBriskSetPatternScale(NativeHandle, value));
            }
        }

        /// <inheritdoc/>
        public override bool Empty
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.Features2DBriskEmpty(NativeHandle, out int empty));
                return empty != 0;
            }
        }

        /// <inheritdoc/>
        public override int DescriptorSize
        {
            get { return GetInt(NativeMethods.Features2DBriskDescriptorSize); }
        }

        /// <inheritdoc/>
        public override int DescriptorType
        {
            get { return GetInt(NativeMethods.Features2DBriskDescriptorType); }
        }

        /// <inheritdoc/>
        public override NormTypes DefaultNorm
        {
            get { return (NormTypes)GetInt(NativeMethods.Features2DBriskDefaultNorm); }
        }

        /// <inheritdoc/>
        public override string DefaultName
        {
            get
            {
                ThrowIfDisposed();
                unsafe
                {
                    return NativeStringMarshaller.GetString(NativeHandle, NativeMethods.Features2DBriskDefaultNameLength, NativeMethods.Features2DBriskDefaultNameFill);
                }
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
        /// Creates a BRISK detector and descriptor extractor.
        /// 创建 BRISK 检测器和描述子提取器。
        /// </summary>
        /// <param name="threshold">The AGAST detection threshold score. AGAST 检测阈值得分。</param>
        /// <param name="octaves">The detection octave count; zero means single scale. 检测 octave 数量；0 表示单尺度。</param>
        /// <param name="patternScale">The scale applied to the sampling pattern. 应用于采样模式的缩放比例。</param>
        /// <returns>The created BRISK object. 创建的 BRISK 对象。</returns>
        public static BRISK Create(int threshold = 30, int octaves = 3, float patternScale = 1.0F)
        {
            NativeException.ThrowIfError(NativeMethods.Features2DBriskCreate(threshold, octaves, patternScale, out IntPtr nativeHandle));
            return new BRISK(nativeHandle);
        }

        /// <summary>
        /// Creates a BRISK detector and descriptor extractor with a custom sampling pattern.
        /// 使用自定义采样模式创建 BRISK 检测器和描述子提取器。
        /// </summary>
        public static BRISK Create(float[] radiusList, int[] numberList, float dMax = 5.85F, float dMin = 8.2F, int[]? indexChange = null)
        {
            ValidateNotNull(radiusList, nameof(radiusList));
            ValidateNotNull(numberList, nameof(numberList));
            indexChange = indexChange ?? Array.Empty<int>();
            unsafe
            {
                fixed (float* radiusPtr = radiusList)
                fixed (int* numberPtr = numberList)
                fixed (int* indexPtr = indexChange)
                {
                    return CreatePatternCore(radiusPtr, radiusList.Length, numberPtr, numberList.Length, dMax, dMin, indexPtr, indexChange.Length);
                }
            }
        }

        /// <summary>
        /// Creates a BRISK detector with a custom sampling pattern and explicit detection settings.
        /// 使用自定义采样模式和显式检测设置创建 BRISK 检测器。
        /// </summary>
        public static BRISK Create(int threshold, int octaves, float[] radiusList, int[] numberList, float dMax = 5.85F, float dMin = 8.2F, int[]? indexChange = null)
        {
            ValidateNotNull(radiusList, nameof(radiusList));
            ValidateNotNull(numberList, nameof(numberList));
            indexChange = indexChange ?? Array.Empty<int>();
            unsafe
            {
                fixed (float* radiusPtr = radiusList)
                fixed (int* numberPtr = numberList)
                fixed (int* indexPtr = indexChange)
                {
                    return CreatePatternWithThresholdCore(threshold, octaves, radiusPtr, radiusList.Length, numberPtr, numberList.Length, dMax, dMin, indexPtr, indexChange.Length);
                }
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Creates a BRISK detector and descriptor extractor with span-backed custom sampling pattern data.
        /// 使用 Span 支持的自定义采样模式数据创建 BRISK 检测器和描述子提取器。
        /// </summary>
        public static BRISK Create(ReadOnlySpan<float> radiusList, ReadOnlySpan<int> numberList, float dMax = 5.85F, float dMin = 8.2F, ReadOnlySpan<int> indexChange = default)
        {
            unsafe
            {
                fixed (float* radiusPtr = radiusList)
                fixed (int* numberPtr = numberList)
                fixed (int* indexPtr = indexChange)
                {
                    return CreatePatternCore(radiusPtr, radiusList.Length, numberPtr, numberList.Length, dMax, dMin, indexPtr, indexChange.Length);
                }
            }
        }

        /// <summary>
        /// Creates a BRISK detector with span-backed custom sampling pattern data and explicit detection settings.
        /// 使用 Span 支持的自定义采样模式数据和显式检测设置创建 BRISK 检测器。
        /// </summary>
        public static BRISK Create(int threshold, int octaves, ReadOnlySpan<float> radiusList, ReadOnlySpan<int> numberList, float dMax = 5.85F, float dMin = 8.2F, ReadOnlySpan<int> indexChange = default)
        {
            unsafe
            {
                fixed (float* radiusPtr = radiusList)
                fixed (int* numberPtr = numberList)
                fixed (int* indexPtr = indexChange)
                {
                    return CreatePatternWithThresholdCore(threshold, octaves, radiusPtr, radiusList.Length, numberPtr, numberList.Length, dMax, dMin, indexPtr, indexChange.Length);
                }
            }
        }
#endif

        /// <inheritdoc/>
        public override void Clear()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.Features2DBriskClear(NativeHandle));
        }

        /// <inheritdoc/>
        public override KeyPoint[] Detect(Mat image, Mat? mask = null)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            return Feature2DDescriptorInterop.Detect(NativeHandle, image.NativeHandle, OptionalHandle(mask), NativeMethods.Features2DBriskDetectCount, NativeMethods.Features2DBriskDetectFill);
        }

        /// <summary>
        /// Computes descriptors for caller-provided keypoints and returns the keypoints kept by OpenCV.
        /// 为调用方提供的关键点计算描述子，并返回 OpenCV 保留的关键点。
        /// </summary>
        public KeyPoint[] Compute(Mat image, KeyPoint[] keypoints, Mat descriptors)
        {
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(keypoints, nameof(keypoints));
            ValidateNotNull(descriptors, nameof(descriptors));
            ThrowIfDisposed();
            return Feature2DDescriptorInterop.Compute(NativeHandle, image.NativeHandle, keypoints, descriptors.NativeHandle, NativeMethods.Features2DBriskCompute);
        }

        /// <summary>
        /// Computes descriptors and replaces the keypoint array with the keypoints kept by OpenCV.
        /// 计算描述子，并用 OpenCV 保留的关键点替换关键点数组。
        /// </summary>
        public void Compute(Mat image, ref KeyPoint[] keypoints, Mat descriptors)
        {
            ValidateNotNull(keypoints, nameof(keypoints));
            keypoints = Compute(image, keypoints, descriptors);
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Computes descriptors from a span-backed keypoint sequence.
        /// 从 Span 支持的关键点序列计算描述子。
        /// </summary>
        public KeyPoint[] Compute(Mat image, ReadOnlySpan<KeyPoint> keypoints, Mat descriptors)
        {
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(descriptors, nameof(descriptors));
            ThrowIfDisposed();
            return Feature2DDescriptorInterop.Compute(NativeHandle, image.NativeHandle, keypoints, descriptors.NativeHandle, NativeMethods.Features2DBriskCompute);
        }
#endif

        /// <summary>
        /// Detects keypoints and computes descriptors.
        /// 检测关键点并计算描述子。
        /// </summary>
        public void DetectAndCompute(Mat image, Mat? mask, out KeyPoint[] keypoints, Mat descriptors, bool useProvidedKeypoints = false)
        {
            keypoints = DetectAndCompute(image, mask, Array.Empty<KeyPoint>(), descriptors, useProvidedKeypoints);
        }

        /// <summary>
        /// Detects or reuses keypoints and computes descriptors.
        /// 检测或复用关键点并计算描述子。
        /// </summary>
        public KeyPoint[] DetectAndCompute(Mat image, Mat? mask, KeyPoint[] keypoints, Mat descriptors, bool useProvidedKeypoints = true)
        {
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(keypoints, nameof(keypoints));
            ValidateNotNull(descriptors, nameof(descriptors));
            ThrowIfDisposed();
            return Feature2DDescriptorInterop.DetectAndCompute(
                NativeHandle,
                image.NativeHandle,
                OptionalHandle(mask),
                keypoints,
                descriptors.NativeHandle,
                useProvidedKeypoints,
                NativeMethods.Features2DBriskDetectAndComputeCount,
                NativeMethods.Features2DBriskDetectAndComputeFill);
        }

        /// <summary>
        /// Detects or reuses keypoints, computes descriptors, and replaces the keypoint array with the OpenCV result.
        /// 检测或复用关键点、计算描述子，并用 OpenCV 返回结果替换关键点数组。
        /// </summary>
        public void DetectAndComputeInPlace(Mat image, Mat? mask, ref KeyPoint[] keypoints, Mat descriptors, bool useProvidedKeypoints)
        {
            ValidateNotNull(keypoints, nameof(keypoints));
            keypoints = DetectAndCompute(image, mask, keypoints, descriptors, useProvidedKeypoints);
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Computes descriptors using span-backed provided keypoints.
        /// 使用 Span 支持的已有关键点计算描述子。
        /// </summary>
        public KeyPoint[] DetectAndCompute(Mat image, Mat? mask, ReadOnlySpan<KeyPoint> keypoints, Mat descriptors)
        {
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(descriptors, nameof(descriptors));
            ThrowIfDisposed();
            return Feature2DDescriptorInterop.DetectAndCompute(
                NativeHandle,
                image.NativeHandle,
                OptionalHandle(mask),
                keypoints,
                descriptors.NativeHandle,
                useProvidedKeypoints: true,
                NativeMethods.Features2DBriskDetectAndComputeCount,
                NativeMethods.Features2DBriskDetectAndComputeFill);
        }
#endif

        /// <inheritdoc/>
        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return disposed ? "{Disposed=True}" : "{Threshold=" + Threshold + ",Octaves=" + Octaves + ",PatternScale=" + PatternScale.ToString(CultureInfo.InvariantCulture) + "}";
        }

        private delegate int IntGetter(IntPtr handle, out int value);

        private delegate int IntSetter(IntPtr handle, int value);

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

        private unsafe static BRISK CreatePatternCore(float* radiusList, int radiusCount, int* numberList, int numberCount, float dMax, float dMin, int* indexChange, int indexChangeCount)
        {
            NativeException.ThrowIfError(NativeMethods.Features2DBriskCreatePattern(radiusList, radiusCount, numberList, numberCount, dMax, dMin, indexChange, indexChangeCount, out IntPtr nativeHandle));
            return new BRISK(nativeHandle);
        }

        private unsafe static BRISK CreatePatternWithThresholdCore(int threshold, int octaves, float* radiusList, int radiusCount, int* numberList, int numberCount, float dMax, float dMin, int* indexChange, int indexChangeCount)
        {
            NativeException.ThrowIfError(NativeMethods.Features2DBriskCreatePatternWithThreshold(threshold, octaves, radiusList, radiusCount, numberList, numberCount, dMax, dMin, indexChange, indexChangeCount, out IntPtr nativeHandle));
            return new BRISK(nativeHandle);
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

        private static IntPtr OptionalHandle(Mat? mat)
        {
            return mat == null ? IntPtr.Zero : mat.NativeHandle;
        }

        private static void ValidateNotNull<T>(T value, string parameterName)
            where T : class
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
    }
}

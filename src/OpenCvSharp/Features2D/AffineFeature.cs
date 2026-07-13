using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Features2D
{
    /// <summary>
    /// Provides an affine-invariant wrapper around an existing <see cref="Feature2D"/> backend compatible with <c>cv::AffineFeature</c>.
    /// 提供与 OpenCV <c>cv::AffineFeature</c> 兼容的仿射不变特征封装器，用于包装已有 <see cref="Feature2D"/> 后端。
    /// </summary>
    public sealed class AffineFeature : Feature2D
    {
        private NativeAffineFeatureHandle handle;
        private readonly Feature2D backend;
        private bool disposed;

        private AffineFeature(IntPtr nativeHandle, Feature2D backend)
        {
            handle = NativeAffineFeatureHandle.FromNativePointer(nativeHandle);
            this.backend = backend;
        }

        /// <inheritdoc/>
        public override bool IsDisposed
        {
            get { return disposed; }
        }

        /// <summary>
        /// Gets the backend feature detector/extractor retained by this wrapper.
        /// 获取此封装器持有的后端特征检测器/描述子提取器。
        /// </summary>
        /// <remarks>
        /// Disposing this wrapper does not dispose the backend object.
        /// 释放此封装器不会释放后端对象。
        /// </remarks>
        public Feature2D Backend
        {
            get
            {
                ThrowIfDisposed();
                return backend;
            }
        }

        /// <inheritdoc/>
        public override bool Empty
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.Features2DAffineEmpty(NativeHandle, out int empty));
                return empty != 0;
            }
        }

        /// <inheritdoc/>
        public override int DescriptorSize
        {
            get { return GetInt(NativeMethods.Features2DAffineDescriptorSize); }
        }

        /// <inheritdoc/>
        public override int DescriptorType
        {
            get { return GetInt(NativeMethods.Features2DAffineDescriptorType); }
        }

        /// <inheritdoc/>
        public override NormTypes DefaultNorm
        {
            get { return (NormTypes)GetInt(NativeMethods.Features2DAffineDefaultNorm); }
        }

        /// <inheritdoc/>
        public override string DefaultName
        {
            get
            {
                ThrowIfDisposed();
                unsafe
                {
                    return NativeStringMarshaller.GetString(NativeHandle, NativeMethods.Features2DAffineDefaultNameLength, NativeMethods.Features2DAffineDefaultNameFill);
                }
            }
        }

        /// <summary>
        /// Gets the number of configured affine views.
        /// 获取当前配置的仿射视角数量。
        /// </summary>
        public int ViewCount
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.Features2DAffineGetViewParamsCount(NativeHandle, out int tiltCount, out int rollCount));
                if (tiltCount != rollCount)
                {
                    throw new OpenCvException("AffineFeature view parameter counts differ.");
                }

                return tiltCount;
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
        /// Creates an affine-invariant wrapper using a supported <see cref="Feature2D"/> backend.
        /// 使用受支持的 <see cref="Feature2D"/> 后端创建仿射不变封装器。
        /// </summary>
        /// <param name="backend">The backend detector/extractor. 后端检测器/描述子提取器。</param>
        /// <param name="maxTilt">The highest power index of the tilt factor. 倾斜因子的最大幂索引。</param>
        /// <param name="minTilt">The lowest power index of the tilt factor. 倾斜因子的最小幂索引。</param>
        /// <param name="tiltStep">The tilt sampling step. 倾斜采样步长。</param>
        /// <param name="rotateStepBase">The rotation sampling step base. 旋转采样步长基准。</param>
        /// <returns>The created affine wrapper. 创建的仿射封装器。</returns>
        public static AffineFeature Create(
            Feature2D backend,
            int maxTilt = 5,
            int minTilt = 0,
            float tiltStep = 1.41421356F,
            float rotateStepBase = 72.0F)
        {
            ValidateNotNull(backend, nameof(backend));
            if (backend is ORB orb)
            {
                return Create(orb, maxTilt, minTilt, tiltStep, rotateStepBase);
            }

            if (backend is SIFT sift)
            {
                return Create(sift, maxTilt, minTilt, tiltStep, rotateStepBase);
            }

            if (backend is FastFeatureDetector fast)
            {
                return Create(fast, maxTilt, minTilt, tiltStep, rotateStepBase);
            }

            if (backend is GFTTDetector gftt)
            {
                return Create(gftt, maxTilt, minTilt, tiltStep, rotateStepBase);
            }

            if (backend is MSER mser)
            {
                return Create(mser, maxTilt, minTilt, tiltStep, rotateStepBase);
            }

            if (backend is SimpleBlobDetector simpleBlob)
            {
                return Create(simpleBlob, maxTilt, minTilt, tiltStep, rotateStepBase);
            }

            if (backend is BRISK brisk)
            {
                return Create(brisk, maxTilt, minTilt, tiltStep, rotateStepBase);
            }

            if (backend is KAZE kaze)
            {
                return Create(kaze, maxTilt, minTilt, tiltStep, rotateStepBase);
            }

            if (backend is AKAZE akaze)
            {
                return Create(akaze, maxTilt, minTilt, tiltStep, rotateStepBase);
            }

            throw new NotSupportedException("AffineFeature supports ORB, SIFT, FastFeatureDetector, GFTTDetector, MSER, SimpleBlobDetector, BRISK, KAZE, and AKAZE backends.");
        }

        /// <summary>
        /// Creates an affine-invariant wrapper around an ORB backend.
        /// 围绕 ORB 后端创建仿射不变封装器。
        /// </summary>
        public static AffineFeature Create(ORB backend, int maxTilt = 5, int minTilt = 0, float tiltStep = 1.41421356F, float rotateStepBase = 72.0F)
        {
            ValidateNotNull(backend, nameof(backend));
            return CreateCore(backend, backend.NativeHandle, NativeMethods.Features2DAffineCreateFromOrb, maxTilt, minTilt, tiltStep, rotateStepBase);
        }

        /// <summary>
        /// Creates an affine-invariant wrapper around a SIFT backend.
        /// 围绕 SIFT 后端创建仿射不变封装器。
        /// </summary>
        public static AffineFeature Create(SIFT backend, int maxTilt = 5, int minTilt = 0, float tiltStep = 1.41421356F, float rotateStepBase = 72.0F)
        {
            ValidateNotNull(backend, nameof(backend));
            return CreateCore(backend, backend.NativeHandle, NativeMethods.Features2DAffineCreateFromSift, maxTilt, minTilt, tiltStep, rotateStepBase);
        }

        /// <summary>
        /// Creates an affine-invariant wrapper around a FAST backend.
        /// 围绕 FAST 后端创建仿射不变封装器。
        /// </summary>
        public static AffineFeature Create(FastFeatureDetector backend, int maxTilt = 5, int minTilt = 0, float tiltStep = 1.41421356F, float rotateStepBase = 72.0F)
        {
            ValidateNotNull(backend, nameof(backend));
            return CreateCore(backend, backend.NativeHandle, NativeMethods.Features2DAffineCreateFromFast, maxTilt, minTilt, tiltStep, rotateStepBase);
        }

        /// <summary>
        /// Creates an affine-invariant wrapper around a GFTT backend.
        /// 围绕 GFTT 后端创建仿射不变封装器。
        /// </summary>
        public static AffineFeature Create(GFTTDetector backend, int maxTilt = 5, int minTilt = 0, float tiltStep = 1.41421356F, float rotateStepBase = 72.0F)
        {
            ValidateNotNull(backend, nameof(backend));
            return CreateCore(backend, backend.NativeHandle, NativeMethods.Features2DAffineCreateFromGftt, maxTilt, minTilt, tiltStep, rotateStepBase);
        }

        /// <summary>
        /// Creates an affine-invariant wrapper around an MSER backend.
        /// 围绕 MSER 后端创建仿射不变封装器。
        /// </summary>
        public static AffineFeature Create(MSER backend, int maxTilt = 5, int minTilt = 0, float tiltStep = 1.41421356F, float rotateStepBase = 72.0F)
        {
            ValidateNotNull(backend, nameof(backend));
            return CreateCore(backend, backend.NativeHandle, NativeMethods.Features2DAffineCreateFromMser, maxTilt, minTilt, tiltStep, rotateStepBase);
        }

        /// <summary>
        /// Creates an affine-invariant wrapper around a simple blob detector backend.
        /// 围绕 SimpleBlobDetector 后端创建仿射不变封装器。
        /// </summary>
        public static AffineFeature Create(SimpleBlobDetector backend, int maxTilt = 5, int minTilt = 0, float tiltStep = 1.41421356F, float rotateStepBase = 72.0F)
        {
            ValidateNotNull(backend, nameof(backend));
            return CreateCore(backend, backend.NativeHandle, NativeMethods.Features2DAffineCreateFromSimpleBlob, maxTilt, minTilt, tiltStep, rotateStepBase);
        }

        /// <summary>
        /// Creates an affine-invariant wrapper around a BRISK backend.
        /// 围绕 BRISK 后端创建仿射不变封装器。
        /// </summary>
        public static AffineFeature Create(BRISK backend, int maxTilt = 5, int minTilt = 0, float tiltStep = 1.41421356F, float rotateStepBase = 72.0F)
        {
            ValidateNotNull(backend, nameof(backend));
            return CreateCore(backend, backend.NativeHandle, NativeMethods.Features2DAffineCreateFromBrisk, maxTilt, minTilt, tiltStep, rotateStepBase);
        }

        /// <summary>
        /// Creates an affine-invariant wrapper around a KAZE backend.
        /// 围绕 KAZE 后端创建仿射不变封装器。
        /// </summary>
        public static AffineFeature Create(KAZE backend, int maxTilt = 5, int minTilt = 0, float tiltStep = 1.41421356F, float rotateStepBase = 72.0F)
        {
            ValidateNotNull(backend, nameof(backend));
            return CreateCore(backend, backend.NativeHandle, NativeMethods.Features2DAffineCreateFromKaze, maxTilt, minTilt, tiltStep, rotateStepBase);
        }

        /// <summary>
        /// Creates an affine-invariant wrapper around an AKAZE backend.
        /// 围绕 AKAZE 后端创建仿射不变封装器。
        /// </summary>
        public static AffineFeature Create(AKAZE backend, int maxTilt = 5, int minTilt = 0, float tiltStep = 1.41421356F, float rotateStepBase = 72.0F)
        {
            ValidateNotNull(backend, nameof(backend));
            return CreateCore(backend, backend.NativeHandle, NativeMethods.Features2DAffineCreateFromAkaze, maxTilt, minTilt, tiltStep, rotateStepBase);
        }

        /// <inheritdoc/>
        public override void Clear()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.Features2DAffineClear(NativeHandle));
        }

        /// <inheritdoc/>
        public override KeyPoint[] Detect(Mat image, Mat? mask = null)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            NativeException.ThrowIfError(NativeMethods.Features2DAffineDetectCount(NativeHandle, image.NativeHandle, OptionalHandle(mask), out int keypointCount));
            if (keypointCount <= 0)
            {
                return Array.Empty<KeyPoint>();
            }

            var native = new NativeKeyPoint[keypointCount];
            unsafe
            {
                fixed (NativeKeyPoint* nativePtr = native)
                {
                    NativeException.ThrowIfError(NativeMethods.Features2DAffineDetectFill(NativeHandle, image.NativeHandle, OptionalHandle(mask), nativePtr, native.Length, out int writtenCount));
                    return KeyPointMarshaller.FromNative(native, writtenCount);
                }
            }
        }

        /// <summary>
        /// Sets affine view parameters.
        /// 设置仿射视角参数。
        /// </summary>
        /// <param name="tilts">The tilt values. 倾斜值集合。</param>
        /// <param name="rolls">The roll values; length must match <paramref name="tilts"/>. 旋转值集合；长度必须与 <paramref name="tilts"/> 一致。</param>
        public void SetViewParams(float[] tilts, float[] rolls)
        {
            ValidateNotNull(tilts, nameof(tilts));
            ValidateNotNull(rolls, nameof(rolls));
            if (tilts.Length != rolls.Length)
            {
                throw new ArgumentException("Tilt and roll collections must have the same length.", nameof(rolls));
            }

            unsafe
            {
                if (tilts.Length == 0)
                {
                    SetViewParamsNative(null, 0, null, 0);
                    return;
                }

                fixed (float* tiltsPtr = tilts)
                fixed (float* rollsPtr = rolls)
                {
                    SetViewParamsNative(tiltsPtr, tilts.Length, rollsPtr, rolls.Length);
                }
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Sets affine view parameters from span-backed collections.
        /// 使用 Span 支持的集合设置仿射视角参数。
        /// </summary>
        /// <param name="tilts">The tilt values. 倾斜值集合。</param>
        /// <param name="rolls">The roll values; length must match <paramref name="tilts"/>. 旋转值集合；长度必须与 <paramref name="tilts"/> 一致。</param>
        public void SetViewParams(ReadOnlySpan<float> tilts, ReadOnlySpan<float> rolls)
        {
            if (tilts.Length != rolls.Length)
            {
                throw new ArgumentException("Tilt and roll collections must have the same length.", nameof(rolls));
            }

            unsafe
            {
                if (tilts.IsEmpty)
                {
                    SetViewParamsNative(null, 0, null, 0);
                    return;
                }

                fixed (float* tiltsPtr = tilts)
                fixed (float* rollsPtr = rolls)
                {
                    SetViewParamsNative(tiltsPtr, tilts.Length, rollsPtr, rolls.Length);
                }
            }
        }
#endif

        /// <summary>
        /// Gets affine view parameters.
        /// 获取仿射视角参数。
        /// </summary>
        /// <param name="tilts">The returned tilt values. 返回的倾斜值集合。</param>
        /// <param name="rolls">The returned roll values. 返回的旋转值集合。</param>
        public void GetViewParams(out float[] tilts, out float[] rolls)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.Features2DAffineGetViewParamsCount(NativeHandle, out int tiltCount, out int rollCount));
            if (tiltCount < 0 || rollCount < 0)
            {
                throw new OpenCvException("AffineFeature returned negative view parameter counts.");
            }

            tilts = new float[tiltCount];
            rolls = new float[rollCount];
            if (tiltCount == 0 && rollCount == 0)
            {
                return;
            }

            unsafe
            {
                fixed (float* tiltsPtr = tilts)
                fixed (float* rollsPtr = rolls)
                {
                    NativeException.ThrowIfError(NativeMethods.Features2DAffineGetViewParamsFill(
                        NativeHandle,
                        tiltsPtr,
                        tilts.Length,
                        rollsPtr,
                        rolls.Length,
                        out int writtenTiltCount,
                        out int writtenRollCount));
                    if (writtenTiltCount != tilts.Length)
                    {
                        Array.Resize(ref tilts, Math.Max(0, writtenTiltCount));
                    }

                    if (writtenRollCount != rolls.Length)
                    {
                        Array.Resize(ref rolls, Math.Max(0, writtenRollCount));
                    }
                }
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Fills caller-provided spans with affine view parameters.
        /// 使用调用方提供的 Span 填充仿射视角参数。
        /// </summary>
        /// <param name="tilts">The destination tilt span. 目标倾斜值 Span。</param>
        /// <param name="rolls">The destination roll span. 目标旋转值 Span。</param>
        /// <returns>The number of written view entries. 写入的视角条目数。</returns>
        public int GetViewParams(Span<float> tilts, Span<float> rolls)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.Features2DAffineGetViewParamsCount(NativeHandle, out int tiltCount, out int rollCount));
            if (tiltCount != rollCount)
            {
                throw new OpenCvException("AffineFeature view parameter counts differ.");
            }

            if (tilts.Length < tiltCount)
            {
                throw new ArgumentException("Tilt destination span is too small.", nameof(tilts));
            }

            if (rolls.Length < rollCount)
            {
                throw new ArgumentException("Roll destination span is too small.", nameof(rolls));
            }

            if (tiltCount == 0)
            {
                return 0;
            }

            unsafe
            {
                fixed (float* tiltsPtr = tilts)
                fixed (float* rollsPtr = rolls)
                {
                    NativeException.ThrowIfError(NativeMethods.Features2DAffineGetViewParamsFill(
                        NativeHandle,
                        tiltsPtr,
                        tilts.Length,
                        rollsPtr,
                        rolls.Length,
                        out int writtenTiltCount,
                        out int writtenRollCount));
                    if (writtenTiltCount != writtenRollCount)
                    {
                        throw new OpenCvException("AffineFeature wrote mismatched view parameter counts.");
                    }

                    return writtenTiltCount;
                }
            }
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
            return disposed ? "{Disposed=True}" : "{DefaultName=" + DefaultName + ",ViewCount=" + ViewCount + ",Backend=" + backend.GetType().Name + "}";
        }

        private delegate int IntGetter(IntPtr handle, out int value);

        private delegate int CreateFromBackendMethod(IntPtr backend, int maxTilt, int minTilt, float tiltStep, float rotateStepBase, out IntPtr affine);

        private int GetInt(IntGetter getter)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(getter(NativeHandle, out int value));
            return value;
        }

        private static AffineFeature CreateCore(
            Feature2D backend,
            IntPtr backendHandle,
            CreateFromBackendMethod create,
            int maxTilt,
            int minTilt,
            float tiltStep,
            float rotateStepBase)
        {
            NativeException.ThrowIfError(create(backendHandle, maxTilt, minTilt, tiltStep, rotateStepBase, out IntPtr nativeHandle));
            return new AffineFeature(nativeHandle, backend);
        }

        private unsafe void SetViewParamsNative(float* tilts, int tiltCount, float* rolls, int rollCount)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.Features2DAffineSetViewParams(NativeHandle, tilts, tiltCount, rolls, rollCount));
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

using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Hfs
{
    /// <summary>
    /// Hierarchical Feature Selection image segmentation object.
    /// Hierarchical Feature Selection 图像分割对象。
    /// </summary>
    public sealed class HfsSegment : IDisposable
    {
        private const int FloatPropertySegEgbThresholdI = 0;
        private const int FloatPropertySegEgbThresholdII = 1;
        private const int FloatPropertySpatialWeight = 2;

        private const int IntPropertyMinRegionSizeI = 0;
        private const int IntPropertyMinRegionSizeII = 1;
        private const int IntPropertySlicSpixelSize = 2;
        private const int IntPropertyNumSlicIter = 3;

        private NativeHfsSegmentHandle handle;
        private readonly int height;
        private readonly int width;
        private bool disposed;

        private HfsSegment(NativeHfsSegmentHandle handle, int height, int width)
        {
            this.handle = handle;
            this.height = height;
            this.width = width;
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

        /// <summary>Gets or sets first-stage EGB segmentation threshold. 获取或设置第一阶段 EGB 分割阈值。</summary>
        public float SegEgbThresholdI
        {
            get { return GetFloatProperty(FloatPropertySegEgbThresholdI); }
            set { SetFloatProperty(FloatPropertySegEgbThresholdI, value, nameof(SegEgbThresholdI)); }
        }

        /// <summary>Gets or sets first-stage minimum region size. 获取或设置第一阶段最小区域尺寸。</summary>
        public int MinRegionSizeI
        {
            get { return GetIntProperty(IntPropertyMinRegionSizeI); }
            set { SetIntProperty(IntPropertyMinRegionSizeI, value, nameof(MinRegionSizeI)); }
        }

        /// <summary>Gets or sets second-stage EGB segmentation threshold. 获取或设置第二阶段 EGB 分割阈值。</summary>
        public float SegEgbThresholdII
        {
            get { return GetFloatProperty(FloatPropertySegEgbThresholdII); }
            set { SetFloatProperty(FloatPropertySegEgbThresholdII, value, nameof(SegEgbThresholdII)); }
        }

        /// <summary>Gets or sets second-stage minimum region size. 获取或设置第二阶段最小区域尺寸。</summary>
        public int MinRegionSizeII
        {
            get { return GetIntProperty(IntPropertyMinRegionSizeII); }
            set { SetIntProperty(IntPropertyMinRegionSizeII, value, nameof(MinRegionSizeII)); }
        }

        /// <summary>Gets or sets SLIC spatial weight. 获取或设置 SLIC 空间权重。</summary>
        public float SpatialWeight
        {
            get { return GetFloatProperty(FloatPropertySpatialWeight); }
            set { SetFloatProperty(FloatPropertySpatialWeight, value, nameof(SpatialWeight)); }
        }

        /// <summary>Gets or sets SLIC superpixel size. 获取或设置 SLIC 超像素尺寸。</summary>
        public int SlicSpixelSize
        {
            get { return GetIntProperty(IntPropertySlicSpixelSize); }
            set { SetIntProperty(IntPropertySlicSpixelSize, value, nameof(SlicSpixelSize)); }
        }

        /// <summary>Gets or sets SLIC iteration count. 获取或设置 SLIC 迭代次数。</summary>
        public int NumSlicIter
        {
            get { return GetIntProperty(IntPropertyNumSlicIter); }
            set { SetIntProperty(IntPropertyNumSlicIter, value, nameof(NumSlicIter)); }
        }

        /// <summary>Creates an HFS segmenter. 创建 HFS 分割器。</summary>
        public static HfsSegment Create(HfsSegmentParams parameters)
        {
            parameters.Validate();
            NativeException.ThrowIfError(NativeMethods.HfsSegmentCreate(
                parameters.Height,
                parameters.Width,
                parameters.SegEgbThresholdI,
                parameters.MinRegionSizeI,
                parameters.SegEgbThresholdII,
                parameters.MinRegionSizeII,
                parameters.SpatialWeight,
                parameters.SlicSpixelSize,
                parameters.NumSlicIter,
                out IntPtr nativeHandle));
            return new HfsSegment(NativeHfsSegmentHandle.FromNativePointer(nativeHandle), parameters.Height, parameters.Width);
        }

        /// <summary>Creates an HFS segmenter with OpenCV default algorithm values for the specified image size. 使用指定图像尺寸和 OpenCV 默认算法值创建 HFS 分割器。</summary>
        public static HfsSegment Create(int height, int width)
        {
            return Create(HfsSegmentParams.Default(height, width));
        }

        /// <summary>Performs CPU segmentation into <paramref name="dst"/>. 执行 CPU 分割并写入 <paramref name="dst"/>。</summary>
        public void PerformSegmentCpu(Mat src, Mat dst, bool draw = true)
        {
            ThrowIfDisposed();
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            ValidateSourceSize(src);
            NativeException.ThrowIfError(NativeMethods.HfsSegmentPerformSegmentCpu(NativeHandle, src.NativeHandle, dst.NativeHandle, draw ? 1 : 0));
        }

        /// <summary>Performs CPU segmentation and returns the result. 执行 CPU 分割并返回结果。</summary>
        public Mat PerformSegmentCpu(Mat src, bool draw = true)
        {
            var dst = new Mat();
            try
            {
                PerformSegmentCpu(src, dst, draw);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>Performs GPU segmentation when OpenCV HFS was built with CUDA support. 在 OpenCV HFS 启用 CUDA 时执行 GPU 分割。</summary>
        public void PerformSegmentGpu(Mat src, Mat dst, bool draw = true)
        {
            ThrowIfDisposed();
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            ValidateSourceSize(src);
            NativeException.ThrowIfError(NativeMethods.HfsSegmentPerformSegmentGpu(NativeHandle, src.NativeHandle, dst.NativeHandle, draw ? 1 : 0));
        }

        /// <summary>Performs GPU segmentation and returns the result. 执行 GPU 分割并返回结果。</summary>
        public Mat PerformSegmentGpu(Mat src, bool draw = true)
        {
            var dst = new Mat();
            try
            {
                PerformSegmentGpu(src, dst, draw);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>Releases native resources. 释放 native 资源。</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private float GetFloatProperty(int propertyId)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.HfsSegmentGetFloatProperty(NativeHandle, propertyId, out float value));
            return value;
        }

        private void SetFloatProperty(int propertyId, float value, string parameterName)
        {
            ThrowIfDisposed();
            ValidatePositiveFinite(value, parameterName);
            NativeException.ThrowIfError(NativeMethods.HfsSegmentSetFloatProperty(NativeHandle, propertyId, value));
        }

        private int GetIntProperty(int propertyId)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.HfsSegmentGetIntProperty(NativeHandle, propertyId, out int value));
            return value;
        }

        private void SetIntProperty(int propertyId, int value, string parameterName)
        {
            ThrowIfDisposed();
            ValidatePositive(value, parameterName);
            NativeException.ThrowIfError(NativeMethods.HfsSegmentSetIntProperty(NativeHandle, propertyId, value));
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
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

        private static void ValidateNotNull<T>(T value, string parameterName)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        private void ValidateSourceSize(Mat src)
        {
            if (src.Rows != height || src.Cols != width)
            {
                throw new ArgumentException("HFS source image size must match the segmenter creation size.", nameof(src));
            }
        }

        private static void ValidatePositive(int value, string parameterName)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Value must be positive.");
            }
        }

        private static void ValidatePositiveFinite(float value, string parameterName)
        {
            if (value <= 0.0F || float.IsNaN(value) || float.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(parameterName, "Value must be a finite positive value.");
            }
        }
    }
}

using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.XImgProc
{
    /// <summary>
    /// Selective Search segmentation strategy wrapper.
    /// Selective Search 分割策略包装。
    /// </summary>
    public class SelectiveSearchSegmentationStrategy : IDisposable
    {
        private NativeXImgProcSelectiveSearchStrategyHandle handle;
        private bool disposed;

        internal SelectiveSearchSegmentationStrategy(IntPtr nativeHandle)
        {
            handle = NativeXImgProcSelectiveSearchStrategyHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Gets whether this strategy has been disposed. 获取策略是否已经释放。</summary>
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

        /// <summary>Creates a color-based strategy. 创建基于颜色的策略。</summary>
        public static SelectiveSearchSegmentationStrategy CreateColor()
        {
            NativeException.ThrowIfError(NativeMethods.XImgProcSelectiveSearchStrategyCreateColor(out IntPtr nativeHandle));
            return new SelectiveSearchSegmentationStrategy(nativeHandle);
        }

        /// <summary>Creates a size-based strategy. 创建基于大小的策略。</summary>
        public static SelectiveSearchSegmentationStrategy CreateSize()
        {
            NativeException.ThrowIfError(NativeMethods.XImgProcSelectiveSearchStrategyCreateSize(out IntPtr nativeHandle));
            return new SelectiveSearchSegmentationStrategy(nativeHandle);
        }

        /// <summary>Creates a texture-based strategy. 创建基于纹理的策略。</summary>
        public static SelectiveSearchSegmentationStrategy CreateTexture()
        {
            NativeException.ThrowIfError(NativeMethods.XImgProcSelectiveSearchStrategyCreateTexture(out IntPtr nativeHandle));
            return new SelectiveSearchSegmentationStrategy(nativeHandle);
        }

        /// <summary>Creates a fill-based strategy. 创建基于填充度的策略。</summary>
        public static SelectiveSearchSegmentationStrategy CreateFill()
        {
            NativeException.ThrowIfError(NativeMethods.XImgProcSelectiveSearchStrategyCreateFill(out IntPtr nativeHandle));
            return new SelectiveSearchSegmentationStrategy(nativeHandle);
        }

        /// <summary>Creates a strategy that combines multiple strategies. 创建组合多个策略的策略。</summary>
        public static SelectiveSearchSegmentationStrategyMultiple CreateMultiple()
        {
            NativeException.ThrowIfError(NativeMethods.XImgProcSelectiveSearchStrategyCreateMultiple(out IntPtr nativeHandle));
            return new SelectiveSearchSegmentationStrategyMultiple(nativeHandle);
        }

        /// <summary>Sets the image and region state used by this strategy. 设置该策略使用的图像和区域状态。</summary>
        public void SetImage(Mat image, Mat regions, Mat sizes, int imageId = -1)
        {
            ThrowIfDisposed();
            XImgProcCv2.ValidateNotNull(image, nameof(image));
            XImgProcCv2.ValidateNotNull(regions, nameof(regions));
            XImgProcCv2.ValidateNotNull(sizes, nameof(sizes));
            NativeException.ThrowIfError(NativeMethods.XImgProcSelectiveSearchStrategySetImage(NativeHandle, image.NativeHandle, regions.NativeHandle, sizes.NativeHandle, imageId));
        }

        /// <summary>Gets the merge score between two regions. 获取两个区域之间的合并分数。</summary>
        public float Get(int r1, int r2)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.XImgProcSelectiveSearchStrategyGet(NativeHandle, r1, r2, out float value));
            return value;
        }

        /// <summary>Informs the strategy that two regions have been merged. 通知策略两个区域已合并。</summary>
        public void Merge(int r1, int r2)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.XImgProcSelectiveSearchStrategyMerge(NativeHandle, r1, r2));
        }

        /// <summary>Releases native resources. 释放 native 资源。</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private protected virtual void Dispose(bool disposing)
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

        private protected void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }
}

using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.XImgProc
{
    /// <summary>
    /// Selective Search object proposal segmentation wrapper.
    /// Selective Search 目标候选分割包装。
    /// </summary>
    public sealed class SelectiveSearchSegmentation : IDisposable
    {
        private NativeXImgProcSelectiveSearchSegmentationHandle handle;
        private bool disposed;

        private SelectiveSearchSegmentation(IntPtr nativeHandle)
        {
            handle = NativeXImgProcSelectiveSearchSegmentationHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Gets whether this segmenter has been disposed. 获取分割器是否已经释放。</summary>
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

        /// <summary>Creates a Selective Search segmentation object. 创建 Selective Search 分割对象。</summary>
        public static SelectiveSearchSegmentation Create()
        {
            NativeException.ThrowIfError(NativeMethods.XImgProcSelectiveSearchSegmentationCreate(out IntPtr nativeHandle));
            return new SelectiveSearchSegmentation(nativeHandle);
        }

        /// <summary>Sets the base image used by switch helpers. 设置 switch 辅助方法使用的基础图像。</summary>
        public void SetBaseImage(Mat image)
        {
            ThrowIfDisposed();
            XImgProcCv2.ValidateNotNull(image, nameof(image));
            NativeException.ThrowIfError(NativeMethods.XImgProcSelectiveSearchSegmentationSetBaseImage(NativeHandle, image.NativeHandle));
        }

        /// <summary>Switches to the single-strategy preset. 切换到 single-strategy 预设。</summary>
        public void SwitchToSingleStrategy(int k = 200, float sigma = 0.8F)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.XImgProcSelectiveSearchSegmentationSwitchToSingleStrategy(NativeHandle, k, sigma));
        }

        /// <summary>Switches to the fast Selective Search preset. 切换到 fast Selective Search 预设。</summary>
        public void SwitchToSelectiveSearchFast(int baseK = 150, int incK = 150, float sigma = 0.8F)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.XImgProcSelectiveSearchSegmentationSwitchToFast(NativeHandle, baseK, incK, sigma));
        }

        /// <summary>Switches to the quality Selective Search preset. 切换到 quality Selective Search 预设。</summary>
        public void SwitchToSelectiveSearchQuality(int baseK = 150, int incK = 150, float sigma = 0.8F)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.XImgProcSelectiveSearchSegmentationSwitchToQuality(NativeHandle, baseK, incK, sigma));
        }

        /// <summary>Adds an image to process. 添加待处理图像。</summary>
        public void AddImage(Mat image)
        {
            ThrowIfDisposed();
            XImgProcCv2.ValidateNotNull(image, nameof(image));
            NativeException.ThrowIfError(NativeMethods.XImgProcSelectiveSearchSegmentationAddImage(NativeHandle, image.NativeHandle));
        }

        /// <summary>Clears all images. 清空所有图像。</summary>
        public void ClearImages()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.XImgProcSelectiveSearchSegmentationClearImages(NativeHandle));
        }

        /// <summary>Adds a graph segmentation object. 添加基于图的分割对象。</summary>
        public void AddGraphSegmentation(GraphSegmentation graphSegmentation)
        {
            ThrowIfDisposed();
            XImgProcCv2.ValidateNotNull(graphSegmentation, nameof(graphSegmentation));
            NativeException.ThrowIfError(NativeMethods.XImgProcSelectiveSearchSegmentationAddGraphSegmentation(NativeHandle, graphSegmentation.NativeHandle));
        }

        /// <summary>Clears all graph segmentations. 清空所有图分割对象。</summary>
        public void ClearGraphSegmentations()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.XImgProcSelectiveSearchSegmentationClearGraphSegmentations(NativeHandle));
        }

        /// <summary>Adds a Selective Search strategy. 添加 Selective Search 策略。</summary>
        public void AddStrategy(SelectiveSearchSegmentationStrategy strategy)
        {
            ThrowIfDisposed();
            XImgProcCv2.ValidateNotNull(strategy, nameof(strategy));
            NativeException.ThrowIfError(NativeMethods.XImgProcSelectiveSearchSegmentationAddStrategy(NativeHandle, strategy.NativeHandle));
        }

        /// <summary>Clears all strategies. 清空所有策略。</summary>
        public void ClearStrategies()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.XImgProcSelectiveSearchSegmentationClearStrategies(NativeHandle));
        }

        /// <summary>Processes images and returns proposal rectangles. 处理图像并返回候选矩形。</summary>
        public Rect[] Process()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.XImgProcSelectiveSearchSegmentationProcessCount(NativeHandle, out int count));
            if (count <= 0)
            {
                return Array.Empty<Rect>();
            }

            var nativeRects = new NativeMethods.XImgProcRectNative[count];
            NativeException.ThrowIfError(NativeMethods.XImgProcSelectiveSearchSegmentationProcessFill(NativeHandle, nativeRects, nativeRects.Length, out int writtenCount));
            var result = new Rect[writtenCount];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = NativeXImgProcConvert.ToRect(nativeRects[i]);
            }

            return result;
        }

        /// <summary>Releases native resources. 释放 native 资源。</summary>
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

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }
}

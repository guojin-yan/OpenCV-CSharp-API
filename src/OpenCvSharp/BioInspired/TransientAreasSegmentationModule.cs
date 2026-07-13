using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.BioInspired
{
    /// <summary>
    /// Wrapper for OpenCV TransientAreasSegmentationModule.
    /// OpenCV TransientAreasSegmentationModule 包装。
    /// </summary>
    public sealed class TransientAreasSegmentationModule : IDisposable
    {
        private NativeBioInspiredTransientAreasHandle handle;
        private bool disposed;

        internal TransientAreasSegmentationModule(IntPtr nativeHandle)
        {
            handle = NativeBioInspiredTransientAreasHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Gets whether this object has been disposed. 获取对象是否已经释放。</summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        /// <summary>Gets module input size. 获取模块输入尺寸。</summary>
        public Size Size
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.BioInspiredTransientAreasGetSize(NativeHandle, out int width, out int height));
                return new Size(width, height);
            }
        }

        /// <summary>Gets current segmentation parameters. 获取当前分割参数。</summary>
        public SegmentationParameters Parameters
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.BioInspiredTransientAreasGetParameters(NativeHandle, out NativeBioInspiredSegmentationParameters native));
                return SegmentationParameters.FromNative(native);
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

        /// <summary>Loads a parameter file or applies the default setup. 加载参数文件或应用默认配置。</summary>
        public void Setup(string? segmentationParameterFile = null, bool applyDefaultSetupOnFailure = true)
        {
            ThrowIfDisposed();
            byte[] path = BioInspiredStringConvert.ToOptionalNullTerminatedUtf8(segmentationParameterFile);
            NativeException.ThrowIfError(NativeMethods.BioInspiredTransientAreasSetup(NativeHandle, path, applyDefaultSetupOnFailure ? 1 : 0));
        }

        /// <summary>Applies segmentation parameters. 应用分割参数。</summary>
        public void Setup(SegmentationParameters parameters)
        {
            ThrowIfDisposed();
            NativeBioInspiredSegmentationParameters native = parameters.ToNative();
            NativeException.ThrowIfError(NativeMethods.BioInspiredTransientAreasSetupParameters(NativeHandle, ref native));
        }

        /// <summary>Runs segmentation on one frame. 对一帧运行分割。</summary>
        public void Run(Mat input, int channelIndex = 0)
        {
            ThrowIfDisposed();
            BioInspiredCv2.ValidateNotNull(input, nameof(input));
            if (channelIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(channelIndex), "Channel index must be non-negative.");
            }

            NativeException.ThrowIfError(NativeMethods.BioInspiredTransientAreasRun(NativeHandle, input.NativeHandle, channelIndex));
        }

        /// <summary>Writes segmentation picture into caller-owned output. 将分割图写入调用方输出矩阵。</summary>
        public void GetSegmentationPicture(Mat output)
        {
            ThrowIfDisposed();
            BioInspiredCv2.ValidateNotNull(output, nameof(output));
            NativeException.ThrowIfError(NativeMethods.BioInspiredTransientAreasGetSegmentationPicture(NativeHandle, output.NativeHandle));
        }

        /// <summary>Returns segmentation picture as a new matrix. 返回新的分割图矩阵。</summary>
        public Mat GetSegmentationPicture()
        {
            var output = new Mat();
            try
            {
                GetSegmentationPicture(output);
                return output;
            }
            catch
            {
                output.Dispose();
                throw;
            }
        }

        /// <summary>Clears all internal buffers. 清除全部内部缓冲区。</summary>
        public void ClearAllBuffers()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.BioInspiredTransientAreasClearAllBuffers(NativeHandle));
        }

        /// <summary>Returns the formatted module setup. 返回格式化的模块配置。</summary>
        public string PrintSetup()
        {
            ThrowIfDisposed();
            unsafe
            {
                return NativeStringMarshaller.GetString(NativeHandle, NativeMethods.BioInspiredTransientAreasPrintSetupLength, NativeMethods.BioInspiredTransientAreasPrintSetupFill);
            }
        }

        /// <summary>Writes module setup to a file. 将模块配置写入文件。</summary>
        public void Write(string path)
        {
            ThrowIfDisposed();
            byte[] nativePath = BioInspiredStringConvert.ToNullTerminatedUtf8(path, nameof(path));
            NativeException.ThrowIfError(NativeMethods.BioInspiredTransientAreasWrite(NativeHandle, nativePath));
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

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }
}

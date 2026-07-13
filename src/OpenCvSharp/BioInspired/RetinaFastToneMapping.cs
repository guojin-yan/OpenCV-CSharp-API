using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.BioInspired
{
    /// <summary>
    /// Wrapper for OpenCV RetinaFastToneMapping.
    /// OpenCV RetinaFastToneMapping 包装。
    /// </summary>
    public sealed class RetinaFastToneMapping : IDisposable
    {
        private NativeBioInspiredRetinaFastToneMappingHandle handle;
        private bool disposed;

        internal RetinaFastToneMapping(IntPtr nativeHandle)
        {
            handle = NativeBioInspiredRetinaFastToneMappingHandle.FromNativePointer(nativeHandle);
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

        /// <summary>Configures tone-mapping parameters. 配置 tone mapping 参数。</summary>
        public void Setup(float photoreceptorsNeighborhoodRadius = 3.0f, float ganglionCellsNeighborhoodRadius = 1.0f, float meanLuminanceModulatorK = 1.0f)
        {
            ThrowIfDisposed();
            BioInspiredCv2.ValidatePositive(photoreceptorsNeighborhoodRadius, nameof(photoreceptorsNeighborhoodRadius));
            BioInspiredCv2.ValidatePositive(ganglionCellsNeighborhoodRadius, nameof(ganglionCellsNeighborhoodRadius));
            NativeException.ThrowIfError(NativeMethods.BioInspiredRetinaFastToneMappingSetup(
                NativeHandle,
                photoreceptorsNeighborhoodRadius,
                ganglionCellsNeighborhoodRadius,
                meanLuminanceModulatorK));
        }

        /// <summary>Applies fast tone mapping into caller-owned output. 将 fast tone mapping 写入调用方输出矩阵。</summary>
        public void Apply(Mat input, Mat output)
        {
            ThrowIfDisposed();
            BioInspiredCv2.ValidateNotNull(input, nameof(input));
            BioInspiredCv2.ValidateNotNull(output, nameof(output));
            NativeException.ThrowIfError(NativeMethods.BioInspiredRetinaFastToneMappingApply(NativeHandle, input.NativeHandle, output.NativeHandle));
        }

        /// <summary>Applies fast tone mapping and returns a new matrix. 运行 fast tone mapping 并返回新矩阵。</summary>
        public Mat Apply(Mat input)
        {
            var output = new Mat();
            try
            {
                Apply(input, output);
                return output;
            }
            catch
            {
                output.Dispose();
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

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }
}

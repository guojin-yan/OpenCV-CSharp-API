using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.BioInspired
{
    /// <summary>
    /// Wrapper for OpenCV bioinspired Retina.
    /// OpenCV bioinspired Retina 包装。
    /// </summary>
    public sealed class Retina : IDisposable
    {
        private NativeBioInspiredRetinaHandle handle;
        private bool disposed;

        internal Retina(IntPtr nativeHandle)
        {
            handle = NativeBioInspiredRetinaHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Gets whether this object has been disposed. 获取对象是否已经释放。</summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        /// <summary>Gets the Retina input size. 获取 Retina 输入尺寸。</summary>
        public Size InputSize
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.BioInspiredRetinaGetInputSize(NativeHandle, out int width, out int height));
                return new Size(width, height);
            }
        }

        /// <summary>Gets the Retina output size. 获取 Retina 输出尺寸。</summary>
        public Size OutputSize
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.BioInspiredRetinaGetOutputSize(NativeHandle, out int width, out int height));
                return new Size(width, height);
            }
        }

        /// <summary>Gets the current Retina parameter groups. 获取当前 Retina 参数组。</summary>
        public RetinaParameters Parameters
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.BioInspiredRetinaGetParameters(NativeHandle, out NativeBioInspiredRetinaParvoParameters parvo, out NativeBioInspiredRetinaMagnoParameters magno));
                return new RetinaParameters(RetinaParvoParameters.FromNative(parvo), RetinaMagnoParameters.FromNative(magno));
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
        public void Setup(string? retinaParameterFile = null, bool applyDefaultSetupOnFailure = true)
        {
            ThrowIfDisposed();
            byte[] path = BioInspiredStringConvert.ToOptionalNullTerminatedUtf8(retinaParameterFile);
            NativeException.ThrowIfError(NativeMethods.BioInspiredRetinaSetup(NativeHandle, path, applyDefaultSetupOnFailure ? 1 : 0));
        }

        /// <summary>Applies parvo channel parameters. 应用 parvo 通道参数。</summary>
        public void Setup(RetinaParvoParameters parameters)
        {
            ThrowIfDisposed();
            NativeBioInspiredRetinaParvoParameters native = parameters.ToNative();
            NativeException.ThrowIfError(NativeMethods.BioInspiredRetinaSetupParvo(NativeHandle, ref native));
        }

        /// <summary>Applies magno channel parameters. 应用 magno 通道参数。</summary>
        public void Setup(RetinaMagnoParameters parameters)
        {
            ThrowIfDisposed();
            NativeBioInspiredRetinaMagnoParameters native = parameters.ToNative();
            NativeException.ThrowIfError(NativeMethods.BioInspiredRetinaSetupMagno(NativeHandle, ref native));
        }

        /// <summary>Applies both parvo and magno parameter groups. 应用 parvo 和 magno 参数组。</summary>
        public void Setup(RetinaParameters parameters)
        {
            Setup(parameters.Parvo);
            Setup(parameters.Magno);
        }

        /// <summary>Runs Retina processing on one frame. 对一帧运行 Retina 处理。</summary>
        public void Run(Mat input)
        {
            ThrowIfDisposed();
            BioInspiredCv2.ValidateNotNull(input, nameof(input));
            NativeException.ThrowIfError(NativeMethods.BioInspiredRetinaRun(NativeHandle, input.NativeHandle));
        }

        /// <summary>Applies Retina fast tone mapping into caller-owned output. 将 Retina fast tone mapping 写入调用方输出矩阵。</summary>
        public void ApplyFastToneMapping(Mat input, Mat output)
        {
            ThrowIfDisposed();
            BioInspiredCv2.ValidateNotNull(input, nameof(input));
            BioInspiredCv2.ValidateNotNull(output, nameof(output));
            NativeException.ThrowIfError(NativeMethods.BioInspiredRetinaApplyFastToneMapping(NativeHandle, input.NativeHandle, output.NativeHandle));
        }

        /// <summary>Applies Retina fast tone mapping and returns a new matrix. 运行 Retina fast tone mapping 并返回新矩阵。</summary>
        public Mat ApplyFastToneMapping(Mat input)
        {
            var output = new Mat();
            try
            {
                ApplyFastToneMapping(input, output);
                return output;
            }
            catch
            {
                output.Dispose();
                throw;
            }
        }

        /// <summary>Writes parvo output into caller-owned matrix. 将 parvo 输出写入调用方矩阵。</summary>
        public void GetParvo(Mat output)
        {
            GetOutput(output, NativeMethods.BioInspiredRetinaGetParvo);
        }

        /// <summary>Returns parvo output as a new matrix. 返回新的 parvo 输出矩阵。</summary>
        public Mat GetParvo()
        {
            return GetOutput(NativeMethods.BioInspiredRetinaGetParvo);
        }

        /// <summary>Writes raw parvo output into caller-owned matrix. 将 raw parvo 输出写入调用方矩阵。</summary>
        public void GetParvoRaw(Mat output)
        {
            GetOutput(output, NativeMethods.BioInspiredRetinaGetParvoRaw);
        }

        /// <summary>Returns raw parvo output as a new matrix. 返回新的 raw parvo 输出矩阵。</summary>
        public Mat GetParvoRaw()
        {
            return GetOutput(NativeMethods.BioInspiredRetinaGetParvoRaw);
        }

        /// <summary>Writes magno output into caller-owned matrix. 将 magno 输出写入调用方矩阵。</summary>
        public void GetMagno(Mat output)
        {
            GetOutput(output, NativeMethods.BioInspiredRetinaGetMagno);
        }

        /// <summary>Returns magno output as a new matrix. 返回新的 magno 输出矩阵。</summary>
        public Mat GetMagno()
        {
            return GetOutput(NativeMethods.BioInspiredRetinaGetMagno);
        }

        /// <summary>Writes raw magno output into caller-owned matrix. 将 raw magno 输出写入调用方矩阵。</summary>
        public void GetMagnoRaw(Mat output)
        {
            GetOutput(output, NativeMethods.BioInspiredRetinaGetMagnoRaw);
        }

        /// <summary>Returns raw magno output as a new matrix. 返回新的 raw magno 输出矩阵。</summary>
        public Mat GetMagnoRaw()
        {
            return GetOutput(NativeMethods.BioInspiredRetinaGetMagnoRaw);
        }

        /// <summary>Configures output color saturation. 配置输出颜色饱和度。</summary>
        public void SetColorSaturation(bool saturateColors, float colorSaturationValue)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.BioInspiredRetinaSetColorSaturation(NativeHandle, saturateColors ? 1 : 0, colorSaturationValue));
        }

        /// <summary>Clears Retina internal buffers. 清除 Retina 内部缓冲区。</summary>
        public void ClearBuffers()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.BioInspiredRetinaClearBuffers(NativeHandle));
        }

        /// <summary>Enables or disables moving contours processing. 启用或禁用 moving contours 处理。</summary>
        public void ActivateMovingContoursProcessing(bool activate)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.BioInspiredRetinaActivateMovingContoursProcessing(NativeHandle, activate ? 1 : 0));
        }

        /// <summary>Enables or disables contours processing. 启用或禁用 contours 处理。</summary>
        public void ActivateContoursProcessing(bool activate)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.BioInspiredRetinaActivateContoursProcessing(NativeHandle, activate ? 1 : 0));
        }

        /// <summary>Returns the formatted Retina setup. 返回格式化的 Retina 配置。</summary>
        public string PrintSetup()
        {
            ThrowIfDisposed();
            unsafe
            {
                return NativeStringMarshaller.GetString(NativeHandle, NativeMethods.BioInspiredRetinaPrintSetupLength, NativeMethods.BioInspiredRetinaPrintSetupFill);
            }
        }

        /// <summary>Writes Retina setup to a file. 将 Retina 配置写入文件。</summary>
        public void Write(string path)
        {
            ThrowIfDisposed();
            byte[] nativePath = BioInspiredStringConvert.ToNullTerminatedUtf8(path, nameof(path));
            NativeException.ThrowIfError(NativeMethods.BioInspiredRetinaWrite(NativeHandle, nativePath));
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

        private void GetOutput(Mat output, OutputGetter getter)
        {
            ThrowIfDisposed();
            BioInspiredCv2.ValidateNotNull(output, nameof(output));
            NativeException.ThrowIfError(getter(NativeHandle, output.NativeHandle));
        }

        private Mat GetOutput(OutputGetter getter)
        {
            var output = new Mat();
            try
            {
                GetOutput(output, getter);
                return output;
            }
            catch
            {
                output.Dispose();
                throw;
            }
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        private delegate int OutputGetter(IntPtr retina, IntPtr output);
    }
}

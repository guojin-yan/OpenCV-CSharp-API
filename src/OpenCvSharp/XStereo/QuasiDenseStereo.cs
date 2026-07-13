using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.XStereo
{
    /// <summary>
    /// Quasi-dense stereo matcher from OpenCV xstereo.
    /// OpenCV xstereo 的 quasi-dense stereo matcher。
    /// </summary>
    public sealed class QuasiDenseStereo : IDisposable
    {
        private NativeXStereoQuasiDenseStereoHandle handle;
        private bool disposed;

        private QuasiDenseStereo(IntPtr nativeHandle)
        {
            handle = NativeXStereoQuasiDenseStereoHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Gets whether this object has been disposed. 获取对象是否已经释放。</summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        /// <summary>Gets or sets propagation parameters. 获取或设置传播参数。</summary>
        public PropagationParameters Parameters
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.XStereoQuasiDenseGetParameters(NativeHandle, out NativeXStereoPropagationParameters native));
                return PropagationParameters.FromNative(native);
            }

            set
            {
                ThrowIfDisposed();
                NativeXStereoPropagationParameters native = value.ToNative();
                NativeException.ThrowIfError(NativeMethods.XStereoQuasiDenseSetParameters(NativeHandle, ref native));
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

        /// <summary>Creates a QuasiDenseStereo matcher. 创建 QuasiDenseStereo matcher。</summary>
        public static QuasiDenseStereo Create(Size monoImageSize, string? parameterFilePath = null)
        {
            XStereoCv2.ValidatePositiveSize(monoImageSize, nameof(monoImageSize));
            byte[] nativePath = XStereoStringConvert.ToOptionalNullTerminatedUtf8(parameterFilePath);
            NativeException.ThrowIfError(NativeMethods.XStereoQuasiDenseCreate(monoImageSize.Width, monoImageSize.Height, nativePath, out IntPtr nativeHandle));
            return new QuasiDenseStereo(nativeHandle);
        }

        /// <summary>Loads matcher parameters from a file. 从文件加载 matcher 参数。</summary>
        public int LoadParameters(string parameterFilePath)
        {
            ThrowIfDisposed();
            byte[] nativePath = XStereoStringConvert.ToNullTerminatedUtf8(parameterFilePath, nameof(parameterFilePath));
            NativeException.ThrowIfError(NativeMethods.XStereoQuasiDenseLoadParameters(NativeHandle, nativePath, out int result));
            return result;
        }

        /// <summary>Saves matcher parameters to a file. 将 matcher 参数保存到文件。</summary>
        public int SaveParameters(string parameterFilePath)
        {
            ThrowIfDisposed();
            byte[] nativePath = XStereoStringConvert.ToNullTerminatedUtf8(parameterFilePath, nameof(parameterFilePath));
            NativeException.ThrowIfError(NativeMethods.XStereoQuasiDenseSaveParameters(NativeHandle, nativePath, out int result));
            return result;
        }

        /// <summary>Processes a stereo pair. 处理双目图像对。</summary>
        public void Process(Mat left, Mat right)
        {
            ThrowIfDisposed();
            XStereoCv2.ValidateNotNull(left, nameof(left));
            XStereoCv2.ValidateNotNull(right, nameof(right));
            NativeException.ThrowIfError(NativeMethods.XStereoQuasiDenseProcess(NativeHandle, left.NativeHandle, right.NativeHandle));
        }

        /// <summary>Gets sparse matches. 获取稀疏匹配。</summary>
        public MatchQuasiDense[] GetSparseMatches()
        {
            return GetMatches(NativeMethods.XStereoQuasiDenseGetSparseMatchesCount, NativeMethods.XStereoQuasiDenseGetSparseMatchesFill);
        }

        /// <summary>Gets dense matches. 获取稠密匹配。</summary>
        public MatchQuasiDense[] GetDenseMatches()
        {
            return GetMatches(NativeMethods.XStereoQuasiDenseGetDenseMatchesCount, NativeMethods.XStereoQuasiDenseGetDenseMatchesFill);
        }

        /// <summary>Gets the matching point in the right image. 获取右图中的匹配点。</summary>
        public Point2f GetMatch(int x, int y)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.XStereoQuasiDenseGetMatch(NativeHandle, x, y, out float matchX, out float matchY));
            return new Point2f(matchX, matchY);
        }

        /// <summary>Writes the disparity map into caller-owned output. 将视差图写入调用方输出矩阵。</summary>
        public void GetDisparity(Mat disparity)
        {
            ThrowIfDisposed();
            XStereoCv2.ValidateNotNull(disparity, nameof(disparity));
            NativeException.ThrowIfError(NativeMethods.XStereoQuasiDenseGetDisparity(NativeHandle, disparity.NativeHandle));
        }

        /// <summary>Gets the disparity map as a new matrix. 获取新的视差图矩阵。</summary>
        public Mat GetDisparity()
        {
            var disparity = new Mat();
            try
            {
                GetDisparity(disparity);
                return disparity;
            }
            catch
            {
                disparity.Dispose();
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

        private MatchQuasiDense[] GetMatches(MatchCountGetter countGetter, MatchFillMethod fill)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(countGetter(NativeHandle, out int count));
            if (count <= 0)
            {
                return Array.Empty<MatchQuasiDense>();
            }

            var native = new NativeXStereoMatchQuasiDense[count];
            NativeException.ThrowIfError(fill(NativeHandle, native, native.Length, out int written));
            if (written < 0)
            {
                written = 0;
            }
            if (written > native.Length)
            {
                written = native.Length;
            }

            var result = new MatchQuasiDense[written];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = MatchQuasiDense.FromNative(native[i]);
            }

            return result;
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        private delegate int MatchCountGetter(IntPtr stereo, out int count);

        private delegate int MatchFillMethod(IntPtr stereo, NativeXStereoMatchQuasiDense[] matches, int matchCapacity, out int count);
    }
}

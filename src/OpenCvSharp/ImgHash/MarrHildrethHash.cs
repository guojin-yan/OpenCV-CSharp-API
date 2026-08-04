using System;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.ImgHash
{
    /// <summary>Marr-Hildreth image hash. Marr-Hildreth 图像哈希。</summary>
    public sealed class MarrHildrethHash : ImgHashBase
    {
        private MarrHildrethHash(IntPtr nativeHandle)
            : base(nativeHandle)
        {
        }

        /// <summary>Gets the alpha parameter. 获取 alpha 参数。</summary>
        public float Alpha
        {
            get
            {
                GetKernelParam(out float alpha, out _);
                return alpha;
            }
        }

        /// <summary>Gets the scale parameter. 获取 scale 参数。</summary>
        public float Scale
        {
            get
            {
                GetKernelParam(out _, out float scale);
                return scale;
            }
        }

        /// <summary>Creates a MarrHildrethHash object. 创建 MarrHildrethHash 对象。</summary>
        public static MarrHildrethHash Create(float alpha = 2.0F, float scale = 1.0F)
        {
            NativeException.ThrowIfError(NativeMethods.ImgHashMarrHildrethCreate(alpha, scale, out IntPtr nativeHandle));
            return new MarrHildrethHash(nativeHandle);
        }

        /// <summary>Gets kernel parameters. 获取核参数。</summary>
        public void GetKernelParam(out float alpha, out float scale)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.ImgHashMarrHildrethGet(NativeHandle, out alpha, out scale));
        }

        /// <summary>Sets kernel parameters. 设置核参数。</summary>
        public void SetKernelParam(float alpha, float scale)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.ImgHashMarrHildrethSetKernelParam(NativeHandle, alpha, scale));
        }
    }
}

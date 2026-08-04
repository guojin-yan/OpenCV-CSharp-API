using System;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Photo
{
    /// <summary>
    /// Mantiuk contrast-domain tonemap operator.
    /// Mantiuk 对比度域 tone mapping 算子。
    /// </summary>
    public sealed class TonemapMantiuk : Tonemap
    {
        private TonemapMantiuk(NativeTonemapHandle handle)
            : base(handle)
        {
        }

        /// <summary>Gets or sets contrast scale. 获取或设置对比度缩放值。</summary>
        public float Scale
        {
            get
            {
                NativeException.ThrowIfError(NativeMethods.TonemapMantiukGetScale(NativeHandle, out float scale));
                return scale;
            }

            set
            {
                NativeException.ThrowIfError(NativeMethods.TonemapMantiukSetScale(NativeHandle, value));
            }
        }

        /// <summary>Gets or sets saturation enhancement. 获取或设置饱和度增强值。</summary>
        public float Saturation
        {
            get
            {
                NativeException.ThrowIfError(NativeMethods.TonemapMantiukGetSaturation(NativeHandle, out float saturation));
                return saturation;
            }

            set
            {
                NativeException.ThrowIfError(NativeMethods.TonemapMantiukSetSaturation(NativeHandle, value));
            }
        }

        /// <summary>Creates a Mantiuk tonemap operator. 创建 Mantiuk tone mapping 算子。</summary>
        public static TonemapMantiuk Create(float gamma = 1.0F, float scale = 0.7F, float saturation = 1.0F)
        {
            NativeException.ThrowIfError(NativeMethods.TonemapMantiukCreate(gamma, scale, saturation, out IntPtr nativeHandle));
            return new TonemapMantiuk(NativeTonemapHandle.FromNativePointer(nativeHandle));
        }
    }
}

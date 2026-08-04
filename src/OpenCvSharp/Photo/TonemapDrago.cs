using System;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Photo
{
    /// <summary>
    /// Drago adaptive logarithmic tonemap operator.
    /// Drago 自适应对数 tone mapping 算子。
    /// </summary>
    public sealed class TonemapDrago : Tonemap
    {
        private TonemapDrago(NativeTonemapHandle handle)
            : base(handle)
        {
        }

        /// <summary>Gets or sets saturation enhancement. 获取或设置饱和度增强值。</summary>
        public float Saturation
        {
            get
            {
                NativeException.ThrowIfError(NativeMethods.TonemapDragoGetSaturation(NativeHandle, out float saturation));
                return saturation;
            }

            set
            {
                NativeException.ThrowIfError(NativeMethods.TonemapDragoSetSaturation(NativeHandle, value));
            }
        }

        /// <summary>Gets or sets bias value. 获取或设置 bias 值。</summary>
        public float Bias
        {
            get
            {
                NativeException.ThrowIfError(NativeMethods.TonemapDragoGetBias(NativeHandle, out float bias));
                return bias;
            }

            set
            {
                NativeException.ThrowIfError(NativeMethods.TonemapDragoSetBias(NativeHandle, value));
            }
        }

        /// <summary>Creates a Drago tonemap operator. 创建 Drago tone mapping 算子。</summary>
        public static TonemapDrago Create(float gamma = 1.0F, float saturation = 1.0F, float bias = 0.85F)
        {
            NativeException.ThrowIfError(NativeMethods.TonemapDragoCreate(gamma, saturation, bias, out IntPtr nativeHandle));
            return new TonemapDrago(NativeTonemapHandle.FromNativePointer(nativeHandle));
        }
    }
}

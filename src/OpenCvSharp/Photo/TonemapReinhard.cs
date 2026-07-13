using System;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Photo
{
    /// <summary>
    /// Reinhard global tonemap operator.
    /// Reinhard 全局 tone mapping 算子。
    /// </summary>
    public sealed class TonemapReinhard : Tonemap
    {
        private TonemapReinhard(NativeTonemapHandle handle)
            : base(handle)
        {
        }

        /// <summary>Gets or sets result intensity. 获取或设置结果强度。</summary>
        public float Intensity
        {
            get
            {
                NativeException.ThrowIfError(NativeMethods.TonemapReinhardGetIntensity(NativeHandle, out float intensity));
                return intensity;
            }

            set
            {
                NativeException.ThrowIfError(NativeMethods.TonemapReinhardSetIntensity(NativeHandle, value));
            }
        }

        /// <summary>Gets or sets light adaptation. 获取或设置亮度适应值。</summary>
        public float LightAdaptation
        {
            get
            {
                NativeException.ThrowIfError(NativeMethods.TonemapReinhardGetLightAdaptation(NativeHandle, out float lightAdapt));
                return lightAdapt;
            }

            set
            {
                NativeException.ThrowIfError(NativeMethods.TonemapReinhardSetLightAdaptation(NativeHandle, value));
            }
        }

        /// <summary>Gets or sets color adaptation. 获取或设置颜色适应值。</summary>
        public float ColorAdaptation
        {
            get
            {
                NativeException.ThrowIfError(NativeMethods.TonemapReinhardGetColorAdaptation(NativeHandle, out float colorAdapt));
                return colorAdapt;
            }

            set
            {
                NativeException.ThrowIfError(NativeMethods.TonemapReinhardSetColorAdaptation(NativeHandle, value));
            }
        }

        /// <summary>Creates a Reinhard tonemap operator. 创建 Reinhard tone mapping 算子。</summary>
        public static TonemapReinhard Create(float gamma = 1.0F, float intensity = 0.0F, float lightAdapt = 1.0F, float colorAdapt = 0.0F)
        {
            NativeException.ThrowIfError(NativeMethods.TonemapReinhardCreate(gamma, intensity, lightAdapt, colorAdapt, out IntPtr nativeHandle));
            return new TonemapReinhard(NativeTonemapHandle.FromNativePointer(nativeHandle));
        }
    }
}

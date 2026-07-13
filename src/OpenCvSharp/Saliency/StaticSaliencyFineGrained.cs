using System;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Saliency
{
    /// <summary>
    /// Static fine-grained saliency algorithm.
    /// 静态细粒度显著性算法。
    /// </summary>
    public sealed class StaticSaliencyFineGrained : StaticSaliency
    {
        private StaticSaliencyFineGrained(IntPtr nativeHandle)
            : base(nativeHandle)
        {
        }

        /// <summary>Creates a fine-grained saliency algorithm. 创建细粒度显著性算法。</summary>
        public static StaticSaliencyFineGrained Create()
        {
            NativeException.ThrowIfError(NativeMethods.SaliencyFineGrainedCreate(out IntPtr nativeHandle));
            return new StaticSaliencyFineGrained(nativeHandle);
        }
    }
}

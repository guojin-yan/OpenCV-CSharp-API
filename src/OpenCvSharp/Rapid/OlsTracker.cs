using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Rapid
{
    /// <summary>
    /// RAPID optimal local searching tracker.
    /// RAPID optimal local searching tracker。
    /// </summary>
    public sealed class OlsTracker : RapidTracker
    {
        private OlsTracker(NativeRapidTrackerHandle handle)
            : base(handle)
        {
        }

        /// <summary>Creates an OLS tracker. 创建 OLS tracker。</summary>
        public static OlsTracker Create(Mat pts3d, Mat tris, int histBins = 8, byte sobelThresh = 10)
        {
            RapidCv2.ValidateNotNull(pts3d, nameof(pts3d));
            RapidCv2.ValidateNotNull(tris, nameof(tris));
            RapidCv2.ValidateMatVector(pts3d, 3, MatType.CV_32F, nameof(pts3d));
            RapidCv2.ValidateMatVector(tris, 3, MatType.CV_32S, nameof(tris));
            RapidCv2.ValidatePositive(histBins, nameof(histBins));
            NativeException.ThrowIfError(NativeMethods.RapidOlsTrackerCreate(
                pts3d.NativeHandle,
                tris.NativeHandle,
                histBins,
                sobelThresh,
                out IntPtr nativeHandle));
            return new OlsTracker(NativeRapidTrackerHandle.FromNativePointer(nativeHandle));
        }
    }
}

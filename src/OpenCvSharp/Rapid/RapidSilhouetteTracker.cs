using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Rapid
{
    /// <summary>
    /// Stateful wrapper around the basic RAPID silhouette tracker.
    /// 基础 RAPID 轮廓 tracker 的有状态封装。
    /// </summary>
    public sealed class RapidSilhouetteTracker : RapidTracker
    {
        private RapidSilhouetteTracker(NativeRapidTrackerHandle handle)
            : base(handle)
        {
        }

        /// <summary>Creates a basic RAPID tracker. 创建基础 RAPID tracker。</summary>
        public static RapidSilhouetteTracker Create(Mat pts3d, Mat tris)
        {
            RapidCv2.ValidateNotNull(pts3d, nameof(pts3d));
            RapidCv2.ValidateNotNull(tris, nameof(tris));
            RapidCv2.ValidateMatVector(pts3d, 3, MatType.CV_32F, nameof(pts3d));
            RapidCv2.ValidateMatVector(tris, 3, MatType.CV_32S, nameof(tris));
            NativeException.ThrowIfError(NativeMethods.RapidTrackerCreate(pts3d.NativeHandle, tris.NativeHandle, out IntPtr nativeHandle));
            return new RapidSilhouetteTracker(NativeRapidTrackerHandle.FromNativePointer(nativeHandle));
        }
    }
}

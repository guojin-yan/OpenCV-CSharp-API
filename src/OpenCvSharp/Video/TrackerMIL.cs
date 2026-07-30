using System;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Video
{
    /// <summary>Owned model-free MIL tracker from the main OpenCV Video module.</summary>
    public sealed class TrackerMIL : Tracker
    {
        private TrackerMIL(IntPtr nativeHandle)
            : base(nativeHandle)
        {
        }

        /// <summary>Creates a tracker with OpenCV 5.0.0 defaults.</summary>
        public static TrackerMIL Create()
        {
            return Create(TrackerMILParams.Default);
        }

        /// <summary>Creates a tracker from a copied parameter value.</summary>
        public static TrackerMIL Create(TrackerMILParams parameters)
        {
            NativeMethods.VideoTrackerMilParamsNative native = parameters.ToNative();
            NativeException.ThrowIfError(NativeMethods.VideoTrackerMilCreate(ref native, out IntPtr tracker));
            return new TrackerMIL(tracker);
        }
    }
}

#if !NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_phase_unwrapping_histogram_create")]
        internal static extern int PhaseUnwrappingHistogramCreate(int width, int height, float histThresh, int nbrOfSmallBins, int nbrOfLargeBins, out IntPtr phaseUnwrapping);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_phase_unwrapping_release")]
        internal static extern void PhaseUnwrappingRelease(IntPtr phaseUnwrapping);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_phase_unwrapping_unwrap_phase_map")]
        internal static extern int PhaseUnwrappingUnwrapPhaseMap(IntPtr phaseUnwrapping, IntPtr wrappedPhaseMap, IntPtr unwrappedPhaseMap, IntPtr shadowMask);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_phase_unwrapping_histogram_get_inverse_reliability_map")]
        internal static extern int PhaseUnwrappingHistogramGetInverseReliabilityMap(IntPtr phaseUnwrapping, IntPtr reliabilityMap);
    }
}
#endif

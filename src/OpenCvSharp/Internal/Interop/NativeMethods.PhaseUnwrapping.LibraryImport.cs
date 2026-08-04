#if NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_phase_unwrapping_histogram_create")]
        internal static partial int PhaseUnwrappingHistogramCreate(int width, int height, float histThresh, int nbrOfSmallBins, int nbrOfLargeBins, out IntPtr phaseUnwrapping);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_phase_unwrapping_release")]
        internal static partial void PhaseUnwrappingRelease(IntPtr phaseUnwrapping);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_phase_unwrapping_unwrap_phase_map")]
        internal static partial int PhaseUnwrappingUnwrapPhaseMap(IntPtr phaseUnwrapping, IntPtr wrappedPhaseMap, IntPtr unwrappedPhaseMap, IntPtr shadowMask);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_phase_unwrapping_histogram_get_inverse_reliability_map")]
        internal static partial int PhaseUnwrappingHistogramGetInverseReliabilityMap(IntPtr phaseUnwrapping, IntPtr reliabilityMap);
    }
}
#endif

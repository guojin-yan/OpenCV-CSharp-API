#if !NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_rapid_draw_correspondencies")]
        internal static extern int RapidDrawCorrespondencies(IntPtr bundle, IntPtr cols, IntPtr colors);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_rapid_draw_search_lines")]
        internal static extern int RapidDrawSearchLines(IntPtr img, IntPtr locations, double color0, double color1, double color2, double color3);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_rapid_draw_wireframe")]
        internal static extern int RapidDrawWireframe(IntPtr img, IntPtr pts2d, IntPtr tris, double color0, double color1, double color2, double color3, int lineType, int cullBackface);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_rapid_extract_control_points")]
        internal static extern int RapidExtractControlPoints(int num, int len, IntPtr pts3d, IntPtr rvec, IntPtr tvec, IntPtr cameraMatrix, int imageWidth, int imageHeight, IntPtr tris, IntPtr ctl2d, IntPtr ctl3d);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_rapid_extract_line_bundle")]
        internal static extern int RapidExtractLineBundle(int len, IntPtr ctl2d, IntPtr img, IntPtr bundle, IntPtr srcLocations);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_rapid_find_correspondencies")]
        internal static extern int RapidFindCorrespondencies(IntPtr bundle, IntPtr cols, IntPtr response);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_rapid_convert_correspondencies")]
        internal static extern int RapidConvertCorrespondencies(IntPtr cols, IntPtr srcLocations, IntPtr pts2d, IntPtr pts3d, IntPtr mask);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_rapid_run")]
        internal static extern int RapidRun(IntPtr img, int num, int len, IntPtr pts3d, IntPtr tris, IntPtr cameraMatrix, IntPtr rvec, IntPtr tvec, int computeRmsd, out float ratio, out double rmsd);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_rapid_tracker_create")]
        internal static extern int RapidTrackerCreate(IntPtr pts3d, IntPtr tris, out IntPtr tracker);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_rapid_ols_tracker_create")]
        internal static extern int RapidOlsTrackerCreate(IntPtr pts3d, IntPtr tris, int histBins, int sobelThresh, out IntPtr tracker);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_rapid_tracker_release")]
        internal static extern void RapidTrackerRelease(IntPtr tracker);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_rapid_tracker_compute")]
        internal static extern int RapidTrackerCompute(IntPtr tracker, IntPtr img, int num, int len, IntPtr cameraMatrix, IntPtr rvec, IntPtr tvec, int criteriaType, int criteriaMaxCount, double criteriaEpsilon, out float ratio);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_rapid_tracker_clear_state")]
        internal static extern int RapidTrackerClearState(IntPtr tracker);
    }
}
#endif

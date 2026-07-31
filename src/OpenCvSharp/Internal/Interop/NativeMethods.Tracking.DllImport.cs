#if !NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_tracker_release_handle")]
        internal static extern void TrackingTrackerReleaseHandle(IntPtr tracker);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_tracker_release_handle")]
        internal static extern void TrackingLegacyTrackerReleaseHandle(IntPtr tracker);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_multi_tracker_release_handle")]
        internal static extern void TrackingLegacyMultiTrackerReleaseHandle(IntPtr tracker);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_tracker_init")]
        internal static extern int TrackingTrackerInit(IntPtr tracker, IntPtr image, TrackingRectNative boundingBox);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_tracker_update")]
        internal static extern int TrackingTrackerUpdate(IntPtr tracker, IntPtr image, ref TrackingRectNative boundingBox, out int result);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_tracker_kcf_create_default")]
        internal static extern int TrackingTrackerKcfCreateDefault(out IntPtr tracker);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_tracker_kcf_create")]
        internal static extern int TrackingTrackerKcfCreate(ref TrackingKcfParamsNative parameters, out IntPtr tracker);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_tracker_kcf_get_default_params")]
        internal static extern int TrackingTrackerKcfGetDefaultParams(out TrackingKcfParamsNative parameters);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_tracker_csrt_create_default")]
        internal static extern int TrackingTrackerCsrtCreateDefault(out IntPtr tracker);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_tracker_csrt_create")]
        internal static extern int TrackingTrackerCsrtCreate(ref TrackingCsrtParamsNative parameters, out IntPtr tracker);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_tracker_csrt_get_default_params")]
        internal static extern int TrackingTrackerCsrtGetDefaultParams(out TrackingCsrtParamsNative parameters);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_tracker_csrt_set_initial_mask")]
        internal static extern int TrackingTrackerCsrtSetInitialMask(IntPtr tracker, IntPtr mask);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_tracker_init")]
        internal static extern int TrackingLegacyTrackerInit(IntPtr tracker, IntPtr image, TrackingRect2dNative boundingBox);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_tracker_update")]
        internal static extern int TrackingLegacyTrackerUpdate(IntPtr tracker, IntPtr image, ref TrackingRect2dNative boundingBox, out int result);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_tracker_mosse_create")]
        internal static extern int TrackingLegacyTrackerMosseCreate(out IntPtr tracker);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_tracker_mil_create_default")]
        internal static extern int TrackingLegacyTrackerMilCreateDefault(out IntPtr tracker);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_tracker_mil_create")]
        internal static extern int TrackingLegacyTrackerMilCreate(ref TrackingMilParamsNative parameters, out IntPtr tracker);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_tracker_mil_get_default_params")]
        internal static extern int TrackingLegacyTrackerMilGetDefaultParams(out TrackingMilParamsNative parameters);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_tracker_median_flow_create_default")]
        internal static extern int TrackingLegacyTrackerMedianFlowCreateDefault(out IntPtr tracker);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_tracker_median_flow_create")]
        internal static extern int TrackingLegacyTrackerMedianFlowCreate(ref TrackingMedianFlowParamsNative parameters, out IntPtr tracker);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_tracker_median_flow_get_default_params")]
        internal static extern int TrackingLegacyTrackerMedianFlowGetDefaultParams(out TrackingMedianFlowParamsNative parameters);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_tracker_boosting_create_default")]
        internal static extern int TrackingLegacyTrackerBoostingCreateDefault(out IntPtr tracker);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_tracker_boosting_create")]
        internal static extern int TrackingLegacyTrackerBoostingCreate(ref TrackingBoostingParamsNative parameters, out IntPtr tracker);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_tracker_boosting_get_default_params")]
        internal static extern int TrackingLegacyTrackerBoostingGetDefaultParams(out TrackingBoostingParamsNative parameters);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_tracker_tld_create")]
        internal static extern int TrackingLegacyTrackerTldCreate(out IntPtr tracker);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_tracker_kcf_create_default")]
        internal static extern int TrackingLegacyTrackerKcfCreateDefault(out IntPtr tracker);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_tracker_kcf_create")]
        internal static extern int TrackingLegacyTrackerKcfCreate(ref TrackingKcfParamsNative parameters, out IntPtr tracker);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_tracker_csrt_create_default")]
        internal static extern int TrackingLegacyTrackerCsrtCreateDefault(out IntPtr tracker);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_tracker_csrt_create")]
        internal static extern int TrackingLegacyTrackerCsrtCreate(ref TrackingCsrtParamsNative parameters, out IntPtr tracker);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_tracker_csrt_set_initial_mask")]
        internal static extern int TrackingLegacyTrackerCsrtSetInitialMask(IntPtr tracker, IntPtr mask);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_upgrade")]
        internal static extern int TrackingLegacyUpgrade(IntPtr legacyTracker, out IntPtr tracker);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_multi_tracker_create")]
        internal static extern int TrackingLegacyMultiTrackerCreate(out IntPtr tracker);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_multi_tracker_add")]
        internal static extern int TrackingLegacyMultiTrackerAdd(IntPtr multiTracker, IntPtr tracker, IntPtr image, TrackingRect2dNative boundingBox, out int result);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_multi_tracker_update_count")]
        internal static extern int TrackingLegacyMultiTrackerUpdateCount(IntPtr multiTracker, IntPtr image, out int result, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_multi_tracker_update_fill")]
        internal static extern int TrackingLegacyMultiTrackerUpdateFill(IntPtr multiTracker, IntPtr image, TrackingRect2dNative[] boundingBoxes, int boundingBoxCapacity, out int result, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_multi_tracker_get_objects_count")]
        internal static extern int TrackingLegacyMultiTrackerGetObjectsCount(IntPtr multiTracker, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_multi_tracker_get_objects_fill")]
        internal static extern int TrackingLegacyMultiTrackerGetObjectsFill(IntPtr multiTracker, TrackingRect2dNative[] boundingBoxes, int boundingBoxCapacity, out int count);
    }
}
#endif

#if NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_tracker_release_handle")]
        internal static partial void TrackingTrackerReleaseHandle(IntPtr tracker);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_tracker_release_handle")]
        internal static partial void TrackingLegacyTrackerReleaseHandle(IntPtr tracker);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_multi_tracker_release_handle")]
        internal static partial void TrackingLegacyMultiTrackerReleaseHandle(IntPtr tracker);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_tracker_init")]
        internal static partial int TrackingTrackerInit(IntPtr tracker, IntPtr image, TrackingRectNative boundingBox);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_tracker_update")]
        internal static partial int TrackingTrackerUpdate(IntPtr tracker, IntPtr image, ref TrackingRectNative boundingBox, out int result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_tracker_kcf_create_default")]
        internal static partial int TrackingTrackerKcfCreateDefault(out IntPtr tracker);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_tracker_kcf_create")]
        internal static partial int TrackingTrackerKcfCreate(ref TrackingKcfParamsNative parameters, out IntPtr tracker);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_tracker_kcf_get_default_params")]
        internal static partial int TrackingTrackerKcfGetDefaultParams(out TrackingKcfParamsNative parameters);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_tracker_csrt_create_default")]
        internal static partial int TrackingTrackerCsrtCreateDefault(out IntPtr tracker);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_tracker_csrt_create")]
        internal static partial int TrackingTrackerCsrtCreate(ref TrackingCsrtParamsNative parameters, out IntPtr tracker);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_tracker_csrt_get_default_params")]
        internal static partial int TrackingTrackerCsrtGetDefaultParams(out TrackingCsrtParamsNative parameters);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_tracker_csrt_set_initial_mask")]
        internal static partial int TrackingTrackerCsrtSetInitialMask(IntPtr tracker, IntPtr mask);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_tracker_init")]
        internal static partial int TrackingLegacyTrackerInit(IntPtr tracker, IntPtr image, TrackingRect2dNative boundingBox);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_tracker_update")]
        internal static partial int TrackingLegacyTrackerUpdate(IntPtr tracker, IntPtr image, ref TrackingRect2dNative boundingBox, out int result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_tracker_mosse_create")]
        internal static partial int TrackingLegacyTrackerMosseCreate(out IntPtr tracker);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_tracker_mil_create_default")]
        internal static partial int TrackingLegacyTrackerMilCreateDefault(out IntPtr tracker);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_tracker_mil_create")]
        internal static partial int TrackingLegacyTrackerMilCreate(ref TrackingMilParamsNative parameters, out IntPtr tracker);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_tracker_mil_get_default_params")]
        internal static partial int TrackingLegacyTrackerMilGetDefaultParams(out TrackingMilParamsNative parameters);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_tracker_median_flow_create_default")]
        internal static partial int TrackingLegacyTrackerMedianFlowCreateDefault(out IntPtr tracker);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_tracker_median_flow_create")]
        internal static partial int TrackingLegacyTrackerMedianFlowCreate(ref TrackingMedianFlowParamsNative parameters, out IntPtr tracker);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_tracker_median_flow_get_default_params")]
        internal static partial int TrackingLegacyTrackerMedianFlowGetDefaultParams(out TrackingMedianFlowParamsNative parameters);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_tracker_boosting_create_default")]
        internal static partial int TrackingLegacyTrackerBoostingCreateDefault(out IntPtr tracker);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_tracker_boosting_create")]
        internal static partial int TrackingLegacyTrackerBoostingCreate(ref TrackingBoostingParamsNative parameters, out IntPtr tracker);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_tracker_boosting_get_default_params")]
        internal static partial int TrackingLegacyTrackerBoostingGetDefaultParams(out TrackingBoostingParamsNative parameters);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_tracker_tld_create")]
        internal static partial int TrackingLegacyTrackerTldCreate(out IntPtr tracker);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_tracker_kcf_create_default")]
        internal static partial int TrackingLegacyTrackerKcfCreateDefault(out IntPtr tracker);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_tracker_kcf_create")]
        internal static partial int TrackingLegacyTrackerKcfCreate(ref TrackingKcfParamsNative parameters, out IntPtr tracker);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_tracker_csrt_create_default")]
        internal static partial int TrackingLegacyTrackerCsrtCreateDefault(out IntPtr tracker);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_tracker_csrt_create")]
        internal static partial int TrackingLegacyTrackerCsrtCreate(ref TrackingCsrtParamsNative parameters, out IntPtr tracker);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_tracker_csrt_set_initial_mask")]
        internal static partial int TrackingLegacyTrackerCsrtSetInitialMask(IntPtr tracker, IntPtr mask);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_upgrade")]
        internal static partial int TrackingLegacyUpgrade(IntPtr legacyTracker, out IntPtr tracker);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_multi_tracker_create")]
        internal static partial int TrackingLegacyMultiTrackerCreate(out IntPtr tracker);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_multi_tracker_add")]
        internal static partial int TrackingLegacyMultiTrackerAdd(IntPtr multiTracker, IntPtr tracker, IntPtr image, TrackingRect2dNative boundingBox, out int result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_multi_tracker_update_count")]
        internal static partial int TrackingLegacyMultiTrackerUpdateCount(IntPtr multiTracker, IntPtr image, out int result, out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_multi_tracker_update_fill")]
        internal static partial int TrackingLegacyMultiTrackerUpdateFill(IntPtr multiTracker, IntPtr image, TrackingRect2dNative[] boundingBoxes, int boundingBoxCapacity, out int result, out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_multi_tracker_get_objects_count")]
        internal static partial int TrackingLegacyMultiTrackerGetObjectsCount(IntPtr multiTracker, out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_tracking_legacy_multi_tracker_get_objects_fill")]
        internal static partial int TrackingLegacyMultiTrackerGetObjectsFill(IntPtr multiTracker, TrackingRect2dNative[] boundingBoxes, int boundingBoxCapacity, out int count);
    }
}
#endif

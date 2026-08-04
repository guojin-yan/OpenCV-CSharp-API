#if NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_set_num_threads")]
        internal static partial int CoreSetNumThreads(int threadCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_get_num_threads")]
        internal static partial int CoreGetNumThreads(out int threadCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_get_thread_num")]
        internal static partial int CoreGetThreadNum(out int threadNumber);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_get_build_information")]
        internal static partial int CoreGetBuildInformation(out IntPtr result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_get_tick_count")]
        internal static partial int CoreGetTickCount(out long tickCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_get_tick_frequency")]
        internal static partial int CoreGetTickFrequency(out double tickFrequency);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_tick_meter_create")]
        internal static partial int CoreTickMeterCreate(out IntPtr meter);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_tick_meter_release")]
        internal static partial void CoreTickMeterRelease(IntPtr meter);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_tick_meter_start")]
        internal static partial int CoreTickMeterStart(IntPtr meter);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_tick_meter_stop")]
        internal static partial int CoreTickMeterStop(IntPtr meter);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_tick_meter_get_time_ticks")]
        internal static partial int CoreTickMeterGetTimeTicks(IntPtr meter, out long value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_tick_meter_get_time_micro")]
        internal static partial int CoreTickMeterGetTimeMicro(IntPtr meter, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_tick_meter_get_time_milli")]
        internal static partial int CoreTickMeterGetTimeMilli(IntPtr meter, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_tick_meter_get_time_sec")]
        internal static partial int CoreTickMeterGetTimeSec(IntPtr meter, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_tick_meter_get_last_time_ticks")]
        internal static partial int CoreTickMeterGetLastTimeTicks(IntPtr meter, out long value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_tick_meter_get_last_time_micro")]
        internal static partial int CoreTickMeterGetLastTimeMicro(IntPtr meter, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_tick_meter_get_last_time_milli")]
        internal static partial int CoreTickMeterGetLastTimeMilli(IntPtr meter, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_tick_meter_get_last_time_sec")]
        internal static partial int CoreTickMeterGetLastTimeSec(IntPtr meter, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_tick_meter_get_counter")]
        internal static partial int CoreTickMeterGetCounter(IntPtr meter, out long value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_tick_meter_get_fps")]
        internal static partial int CoreTickMeterGetFps(IntPtr meter, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_tick_meter_get_avg_time_sec")]
        internal static partial int CoreTickMeterGetAvgTimeSec(IntPtr meter, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_tick_meter_get_avg_time_milli")]
        internal static partial int CoreTickMeterGetAvgTimeMilli(IntPtr meter, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_tick_meter_reset")]
        internal static partial int CoreTickMeterReset(IntPtr meter);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_get_cpu_tick_count")]
        internal static partial int CoreGetCpuTickCount(out long tickCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_check_hardware_support")]
        internal static partial int CoreCheckHardwareSupport(int feature, out int supported);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_get_hardware_feature_name")]
        internal static partial int CoreGetHardwareFeatureName(int feature, out IntPtr result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_get_cpu_features_line")]
        internal static partial int CoreGetCpuFeaturesLine(out IntPtr result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_get_number_of_cpus")]
        internal static partial int CoreGetNumberOfCpus(out int cpuCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_get_default_algorithm_hint")]
        internal static partial int CoreGetDefaultAlgorithmHint(out int hint);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_set_use_optimized")]
        internal static partial int CoreSetUseOptimized(int enabled);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_use_optimized")]
        internal static partial int CoreUseOptimized(out int enabled);
    }
}
#endif

#if !NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_set_num_threads")]
        internal static extern int CoreSetNumThreads(int threadCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_get_num_threads")]
        internal static extern int CoreGetNumThreads(out int threadCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_get_thread_num")]
        internal static extern int CoreGetThreadNum(out int threadNumber);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_get_build_information")]
        internal static extern int CoreGetBuildInformation(out IntPtr result);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_get_tick_count")]
        internal static extern int CoreGetTickCount(out long tickCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_get_tick_frequency")]
        internal static extern int CoreGetTickFrequency(out double tickFrequency);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_tick_meter_create")]
        internal static extern int CoreTickMeterCreate(out IntPtr meter);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_tick_meter_release")]
        internal static extern void CoreTickMeterRelease(IntPtr meter);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_tick_meter_start")]
        internal static extern int CoreTickMeterStart(IntPtr meter);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_tick_meter_stop")]
        internal static extern int CoreTickMeterStop(IntPtr meter);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_tick_meter_get_time_ticks")]
        internal static extern int CoreTickMeterGetTimeTicks(IntPtr meter, out long value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_tick_meter_get_time_micro")]
        internal static extern int CoreTickMeterGetTimeMicro(IntPtr meter, out double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_tick_meter_get_time_milli")]
        internal static extern int CoreTickMeterGetTimeMilli(IntPtr meter, out double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_tick_meter_get_time_sec")]
        internal static extern int CoreTickMeterGetTimeSec(IntPtr meter, out double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_tick_meter_get_last_time_ticks")]
        internal static extern int CoreTickMeterGetLastTimeTicks(IntPtr meter, out long value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_tick_meter_get_last_time_micro")]
        internal static extern int CoreTickMeterGetLastTimeMicro(IntPtr meter, out double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_tick_meter_get_last_time_milli")]
        internal static extern int CoreTickMeterGetLastTimeMilli(IntPtr meter, out double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_tick_meter_get_last_time_sec")]
        internal static extern int CoreTickMeterGetLastTimeSec(IntPtr meter, out double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_tick_meter_get_counter")]
        internal static extern int CoreTickMeterGetCounter(IntPtr meter, out long value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_tick_meter_get_fps")]
        internal static extern int CoreTickMeterGetFps(IntPtr meter, out double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_tick_meter_get_avg_time_sec")]
        internal static extern int CoreTickMeterGetAvgTimeSec(IntPtr meter, out double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_tick_meter_get_avg_time_milli")]
        internal static extern int CoreTickMeterGetAvgTimeMilli(IntPtr meter, out double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_tick_meter_reset")]
        internal static extern int CoreTickMeterReset(IntPtr meter);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_get_cpu_tick_count")]
        internal static extern int CoreGetCpuTickCount(out long tickCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_check_hardware_support")]
        internal static extern int CoreCheckHardwareSupport(int feature, out int supported);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_get_hardware_feature_name")]
        internal static extern int CoreGetHardwareFeatureName(int feature, out IntPtr result);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_get_cpu_features_line")]
        internal static extern int CoreGetCpuFeaturesLine(out IntPtr result);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_get_number_of_cpus")]
        internal static extern int CoreGetNumberOfCpus(out int cpuCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_get_default_algorithm_hint")]
        internal static extern int CoreGetDefaultAlgorithmHint(out int hint);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_set_use_optimized")]
        internal static extern int CoreSetUseOptimized(int enabled);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_use_optimized")]
        internal static extern int CoreUseOptimized(out int enabled);
    }
}
#endif

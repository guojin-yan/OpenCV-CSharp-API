using System;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Core
{
    public static partial class Cv2
    {
        /// <summary>Sets the requested OpenCV thread count for subsequent parallel regions.</summary>
        /// <remarks>This is process-global OpenCV state and must not be changed concurrently with OpenCV work.</remarks>
        public static void SetNumThreads(int threadCount)
        {
            NativeException.ThrowIfError(NativeMethods.CoreSetNumThreads(threadCount));
        }

        /// <summary>Gets the thread count currently reported by the configured OpenCV parallel backend.</summary>
        public static int GetNumThreads()
        {
            NativeException.ThrowIfError(NativeMethods.CoreGetNumThreads(out int value));
            return value;
        }

        /// <summary>Gets the current backend-specific parallel thread index.</summary>
        /// <remarks>The value is backend-dependent and can be negative outside a parallel region.</remarks>
        public static int GetThreadNum()
        {
            NativeException.ThrowIfError(NativeMethods.CoreGetThreadNum(out int value));
            return value;
        }

        /// <summary>Gets the OpenCV build configuration text from the loaded native runtime.</summary>
        public static string GetBuildInformation()
        {
            NativeException.ThrowIfError(NativeMethods.CoreGetBuildInformation(out IntPtr value));
            return CorePersistenceMarshal.ReadUtf8Result(value);
        }

        /// <summary>Gets a monotonic OpenCV tick count suitable for interval measurement.</summary>
        public static long GetTickCount()
        {
            NativeException.ThrowIfError(NativeMethods.CoreGetTickCount(out long value));
            return value;
        }

        /// <summary>Gets the number of OpenCV ticks per second.</summary>
        public static double GetTickFrequency()
        {
            NativeException.ThrowIfError(NativeMethods.CoreGetTickFrequency(out double value));
            return value;
        }

        /// <summary>Gets a CPU tick counter when the platform provides one.</summary>
        /// <remarks>CPU ticks are not guaranteed to be monotonic across processor migration and cannot be converted directly to elapsed time.</remarks>
        public static long GetCpuTickCount()
        {
            NativeException.ThrowIfError(NativeMethods.CoreGetCpuTickCount(out long value));
            return value;
        }

        /// <summary>Gets whether the loaded runtime currently enables a CPU feature.</summary>
        public static bool CheckHardwareSupport(CpuFeatures feature)
        {
            ValidateCpuFeature(feature, nameof(feature));
            NativeException.ThrowIfError(NativeMethods.CoreCheckHardwareSupport((int)feature, out int value));
            return value != 0;
        }

        /// <summary>Gets the OpenCV name for a CPU feature, or an empty string for an undefined identifier.</summary>
        public static string GetHardwareFeatureName(CpuFeatures feature)
        {
            ValidateCpuFeature(feature, nameof(feature));
            NativeException.ThrowIfError(NativeMethods.CoreGetHardwareFeatureName((int)feature, out IntPtr value));
            return CorePersistenceMarshal.ReadUtf8Result(value);
        }

        /// <summary>Gets the CPU feature dispatch line compiled into the loaded OpenCV runtime.</summary>
        public static string GetCpuFeaturesLine()
        {
            NativeException.ThrowIfError(NativeMethods.CoreGetCpuFeaturesLine(out IntPtr value));
            return CorePersistenceMarshal.ReadUtf8Result(value);
        }

        /// <summary>Gets the number of logical CPUs available to the current process.</summary>
        public static int GetNumberOfCpus()
        {
            NativeException.ThrowIfError(NativeMethods.CoreGetNumberOfCpus(out int value));
            return value;
        }

        /// <summary>Gets the default algorithm hint selected by the loaded OpenCV build.</summary>
        public static AlgorithmHint GetDefaultAlgorithmHint()
        {
            NativeException.ThrowIfError(NativeMethods.CoreGetDefaultAlgorithmHint(out int value));
            if (value < (int)AlgorithmHint.Default || value > (int)AlgorithmHint.Approximate)
            {
                throw new OpenCvException("Native OpenCV returned an unknown algorithm hint.");
            }
            return (AlgorithmHint)value;
        }

        /// <summary>Enables or disables OpenCV optimized dispatch code.</summary>
        /// <remarks>This is process-global OpenCV state and is only safe when no other OpenCV operation is executing.</remarks>
        public static void SetUseOptimized(bool enabled)
        {
            NativeException.ThrowIfError(NativeMethods.CoreSetUseOptimized(enabled ? 1 : 0));
        }

        /// <summary>Gets whether OpenCV optimized dispatch code is enabled.</summary>
        public static bool UseOptimized()
        {
            NativeException.ThrowIfError(NativeMethods.CoreUseOptimized(out int value));
            return value != 0;
        }

        private static void ValidateCpuFeature(CpuFeatures feature, string parameterName)
        {
            int value = (int)feature;
            if (value < (int)CpuFeatures.None || value > (int)CpuFeatures.MaxFeature)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }
    }
}

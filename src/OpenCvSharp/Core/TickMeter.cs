using System;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Core
{
    /// <summary>Measures elapsed OpenCV tick time across explicit start and stop intervals.</summary>
    public sealed class TickMeter : IDisposable
    {
        private readonly NativeTickMeterHandle handle;

        /// <summary>Initializes an empty timer.</summary>
        public TickMeter()
        {
            NativeException.ThrowIfError(NativeMethods.CoreTickMeterCreate(out IntPtr value));
            handle = NativeTickMeterHandle.FromNativePointer(value);
        }

        /// <summary>Gets whether this timer has been disposed.</summary>
        public bool IsDisposed { get { return handle.IsClosed; } }

        /// <summary>Gets total elapsed ticks across completed intervals.</summary>
        public long TimeTicks { get { return GetInt64(NativeMethods.CoreTickMeterGetTimeTicks); } }

        /// <summary>Gets total elapsed microseconds across completed intervals.</summary>
        public double TimeMicroseconds { get { return GetDouble(NativeMethods.CoreTickMeterGetTimeMicro); } }

        /// <summary>Gets total elapsed milliseconds across completed intervals.</summary>
        public double TimeMilliseconds { get { return GetDouble(NativeMethods.CoreTickMeterGetTimeMilli); } }

        /// <summary>Gets total elapsed seconds across completed intervals.</summary>
        public double TimeSeconds { get { return GetDouble(NativeMethods.CoreTickMeterGetTimeSec); } }

        /// <summary>Gets ticks from the last completed interval.</summary>
        public long LastTimeTicks { get { return GetInt64(NativeMethods.CoreTickMeterGetLastTimeTicks); } }

        /// <summary>Gets microseconds from the last completed interval.</summary>
        public double LastTimeMicroseconds { get { return GetDouble(NativeMethods.CoreTickMeterGetLastTimeMicro); } }

        /// <summary>Gets milliseconds from the last completed interval.</summary>
        public double LastTimeMilliseconds { get { return GetDouble(NativeMethods.CoreTickMeterGetLastTimeMilli); } }

        /// <summary>Gets seconds from the last completed interval.</summary>
        public double LastTimeSeconds { get { return GetDouble(NativeMethods.CoreTickMeterGetLastTimeSec); } }

        /// <summary>Gets the number of completed intervals.</summary>
        public long Counter { get { return GetInt64(NativeMethods.CoreTickMeterGetCounter); } }

        /// <summary>Gets the average number of completed intervals per second.</summary>
        public double FramesPerSecond { get { return GetDouble(NativeMethods.CoreTickMeterGetFps); } }

        /// <summary>Gets average seconds per completed interval.</summary>
        public double AverageTimeSeconds { get { return GetDouble(NativeMethods.CoreTickMeterGetAvgTimeSec); } }

        /// <summary>Gets average milliseconds per completed interval.</summary>
        public double AverageTimeMilliseconds { get { return GetDouble(NativeMethods.CoreTickMeterGetAvgTimeMilli); } }

        /// <summary>Starts a new interval.</summary>
        public void Start()
        {
            WithNativeHandle(nativeHandle => NativeException.ThrowIfError(NativeMethods.CoreTickMeterStart(nativeHandle)));
        }

        /// <summary>Stops the active interval, if any.</summary>
        public void Stop()
        {
            WithNativeHandle(nativeHandle => NativeException.ThrowIfError(NativeMethods.CoreTickMeterStop(nativeHandle)));
        }

        /// <summary>Clears every accumulated interval and elapsed value.</summary>
        public void Reset()
        {
            WithNativeHandle(nativeHandle => NativeException.ThrowIfError(NativeMethods.CoreTickMeterReset(nativeHandle)));
        }

        /// <summary>Releases the owned native timer.</summary>
        public void Dispose()
        {
            handle.Dispose();
            GC.SuppressFinalize(this);
        }

        private delegate void HandleAction(IntPtr nativeHandle);
        private delegate int Int64Getter(IntPtr handle, out long value);
        private delegate int DoubleGetter(IntPtr handle, out double value);

        private void WithNativeHandle(HandleAction action)
        {
            bool addedReference = false;
            try
            {
                if (handle.IsClosed || handle.IsInvalid)
                {
                    throw new ObjectDisposedException(GetType().FullName);
                }

                handle.DangerousAddRef(ref addedReference);
                action(handle.DangerousGetHandle());
            }
            finally
            {
                if (addedReference)
                {
                    handle.DangerousRelease();
                }
            }
        }

        private long GetInt64(Int64Getter getter)
        {
            long value = 0;
            WithNativeHandle(nativeHandle => NativeException.ThrowIfError(getter(nativeHandle, out value)));
            return value;
        }

        private double GetDouble(DoubleGetter getter)
        {
            double value = 0.0;
            WithNativeHandle(nativeHandle => NativeException.ThrowIfError(getter(nativeHandle, out value)));
            return value;
        }
    }
}

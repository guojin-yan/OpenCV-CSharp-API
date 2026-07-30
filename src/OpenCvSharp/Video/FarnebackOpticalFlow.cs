using System;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Video
{
    /// <summary>Stateful Farneback dense optical-flow algorithm.</summary>
    public sealed class FarnebackOpticalFlow : DenseOpticalFlow
    {
        private const int NumLevelsProperty = 0;
        private const int WinSizeProperty = 1;
        private const int NumIterationsProperty = 2;
        private const int PolyNProperty = 3;
        private const int FlagsProperty = 4;
        private const int PyrScaleProperty = 0;
        private const int PolySigmaProperty = 1;

        /// <summary>Creates a Farneback algorithm with the upstream defaults.</summary>
        public FarnebackOpticalFlow(int numLevels = 5, double pyrScale = 0.5, bool fastPyramids = false, int winSize = 13, int numIterations = 10, int polyN = 5, double polySigma = 1.1, OpticalFlowFlags flags = OpticalFlowFlags.None)
            : base(CreateNative(numLevels, pyrScale, fastPyramids, winSize, numIterations, polyN, polySigma, flags))
        {
        }

        /// <summary>Creates a Farneback algorithm with the upstream defaults.</summary>
        public static FarnebackOpticalFlow Create(int numLevels = 5, double pyrScale = 0.5, bool fastPyramids = false, int winSize = 13, int numIterations = 10, int polyN = 5, double polySigma = 1.1, OpticalFlowFlags flags = OpticalFlowFlags.None)
        {
            return new FarnebackOpticalFlow(numLevels, pyrScale, fastPyramids, winSize, numIterations, polyN, polySigma, flags);
        }

        /// <summary>Gets or sets the number of pyramid levels.</summary>
        public int NumLevels { get => GetInt(NumLevelsProperty); set => SetInt(NumLevelsProperty, value); }
        /// <summary>Gets or sets the pyramid scale factor.</summary>
        public double PyrScale { get => GetDouble(PyrScaleProperty); set => SetDouble(PyrScaleProperty, value); }
        /// <summary>Gets or sets whether fast pyramids are used.</summary>
        public bool FastPyramids { get => GetBool(); set => SetBool(value); }
        /// <summary>Gets or sets the averaging window size.</summary>
        public int WinSize { get => GetInt(WinSizeProperty); set => SetInt(WinSizeProperty, value); }
        /// <summary>Gets or sets the iteration count per pyramid level.</summary>
        public int NumIterations { get => GetInt(NumIterationsProperty); set => SetInt(NumIterationsProperty, value); }
        /// <summary>Gets or sets the polynomial neighborhood size.</summary>
        public int PolyN { get => GetInt(PolyNProperty); set => SetInt(PolyNProperty, value); }
        /// <summary>Gets or sets the polynomial Gaussian sigma.</summary>
        public double PolySigma { get => GetDouble(PolySigmaProperty); set => SetDouble(PolySigmaProperty, value); }
        /// <summary>Gets or sets Farneback flow flags.</summary>
        public OpticalFlowFlags Flags { get => (OpticalFlowFlags)GetInt(FlagsProperty); set => SetInt(FlagsProperty, ValidateFlags(value)); }

        private int GetInt(int propertyId) { NativeException.ThrowIfError(NativeMethods.FarnebackOpticalFlowGetIntProperty(NativeHandle, propertyId, out int value)); return value; }
        private void SetInt(int propertyId, int value) { NativeException.ThrowIfError(NativeMethods.FarnebackOpticalFlowSetIntProperty(NativeHandle, propertyId, value)); }
        private double GetDouble(int propertyId) { NativeException.ThrowIfError(NativeMethods.FarnebackOpticalFlowGetDoubleProperty(NativeHandle, propertyId, out double value)); return value; }
        private void SetDouble(int propertyId, double value) { if (double.IsNaN(value) || double.IsInfinity(value)) throw new ArgumentOutOfRangeException(nameof(value)); NativeException.ThrowIfError(NativeMethods.FarnebackOpticalFlowSetDoubleProperty(NativeHandle, propertyId, value)); }
        private bool GetBool() { NativeException.ThrowIfError(NativeMethods.FarnebackOpticalFlowGetBoolProperty(NativeHandle, 0, out int value)); return value != 0; }
        private void SetBool(bool value) { NativeException.ThrowIfError(NativeMethods.FarnebackOpticalFlowSetBoolProperty(NativeHandle, 0, value ? 1 : 0)); }

        private static IntPtr CreateNative(int numLevels, double pyrScale, bool fastPyramids, int winSize, int numIterations, int polyN, double polySigma, OpticalFlowFlags flags)
        {
            if (double.IsNaN(pyrScale) || double.IsInfinity(pyrScale)) throw new ArgumentOutOfRangeException(nameof(pyrScale));
            if (double.IsNaN(polySigma) || double.IsInfinity(polySigma)) throw new ArgumentOutOfRangeException(nameof(polySigma));
            NativeException.ThrowIfError(NativeMethods.FarnebackOpticalFlowCreate(numLevels, pyrScale, fastPyramids ? 1 : 0, winSize, numIterations, polyN, polySigma, ValidateFlags(flags), out IntPtr handle));
            return handle;
        }

        private static int ValidateFlags(OpticalFlowFlags flags)
        {
            const OpticalFlowFlags allowed = OpticalFlowFlags.UseInitialFlow | OpticalFlowFlags.FarnebackGaussian;
            if ((flags & ~allowed) != 0) throw new ArgumentOutOfRangeException(nameof(flags));
            return (int)flags;
        }
    }
}

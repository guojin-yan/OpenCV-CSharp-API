using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Video
{
    /// <summary>Stateful sparse pyramidal Lucas-Kanade optical flow.</summary>
    public sealed class SparsePyrLkOpticalFlow : SparseOpticalFlow
    {
        private const int MaxLevelProperty = 0;
        private const int FlagsProperty = 1;

        /// <summary>Creates a sparse pyramidal LK algorithm with upstream defaults.</summary>
        public SparsePyrLkOpticalFlow(Size? winSize = null, int maxLevel = 3, TermCriteria? criteria = null, OpticalFlowFlags flags = OpticalFlowFlags.None, double minEigThreshold = 1e-4)
            : base(CreateNative(winSize ?? new Size(21, 21), maxLevel, criteria ?? TermCriteria.ByCountAndEpsilon(30, 0.01), flags, minEigThreshold))
        {
        }

        /// <summary>Creates a sparse pyramidal LK algorithm with upstream defaults.</summary>
        public static SparsePyrLkOpticalFlow Create(Size? winSize = null, int maxLevel = 3, TermCriteria? criteria = null, OpticalFlowFlags flags = OpticalFlowFlags.None, double minEigThreshold = 1e-4)
        {
            return new SparsePyrLkOpticalFlow(winSize, maxLevel, criteria, flags, minEigThreshold);
        }

        /// <summary>Gets or sets the LK search-window size.</summary>
        public Size WinSize
        {
            get { NativeException.ThrowIfError(NativeMethods.SparsePyrLKOpticalFlowGetSizeProperty(NativeHandle, out int width, out int height)); return new Size(width, height); }
            set { NativeException.ThrowIfError(NativeMethods.SparsePyrLKOpticalFlowSetSizeProperty(NativeHandle, value.Width, value.Height)); }
        }

        /// <summary>Gets or sets the maximum pyramid level.</summary>
        public int MaxLevel { get => GetInt(MaxLevelProperty); set => SetInt(MaxLevelProperty, value); }
        /// <summary>Gets or sets iteration termination criteria.</summary>
        public TermCriteria Criteria
        {
            get { NativeException.ThrowIfError(NativeMethods.SparsePyrLKOpticalFlowGetTermCriteria(NativeHandle, out int type, out int maxCount, out double epsilon)); return new TermCriteria((TermCriteriaTypes)type, maxCount, epsilon); }
            set { NativeException.ThrowIfError(NativeMethods.SparsePyrLKOpticalFlowSetTermCriteria(NativeHandle, (int)value.Type, value.MaxCount, value.Epsilon)); }
        }
        /// <summary>Gets or sets sparse-LK flags.</summary>
        public OpticalFlowFlags Flags { get => (OpticalFlowFlags)GetInt(FlagsProperty); set => SetInt(FlagsProperty, ValidateFlags(value)); }
        /// <summary>Gets or sets the minimum eigenvalue threshold.</summary>
        public double MinEigThreshold
        {
            get { NativeException.ThrowIfError(NativeMethods.SparsePyrLKOpticalFlowGetMinEigThreshold(NativeHandle, out double value)); return value; }
            set { if (double.IsNaN(value) || double.IsInfinity(value)) throw new ArgumentOutOfRangeException(nameof(value)); NativeException.ThrowIfError(NativeMethods.SparsePyrLKOpticalFlowSetMinEigThreshold(NativeHandle, value)); }
        }

        internal override bool RequiresInitialFlow => (Flags & OpticalFlowFlags.UseInitialFlow) != 0;

        private int GetInt(int propertyId) { NativeException.ThrowIfError(NativeMethods.SparsePyrLKOpticalFlowGetIntProperty(NativeHandle, propertyId, out int value)); return value; }
        private void SetInt(int propertyId, int value) { NativeException.ThrowIfError(NativeMethods.SparsePyrLKOpticalFlowSetIntProperty(NativeHandle, propertyId, value)); }

        private static IntPtr CreateNative(Size winSize, int maxLevel, TermCriteria criteria, OpticalFlowFlags flags, double minEigThreshold)
        {
            if (double.IsNaN(minEigThreshold) || double.IsInfinity(minEigThreshold)) throw new ArgumentOutOfRangeException(nameof(minEigThreshold));
            NativeException.ThrowIfError(NativeMethods.SparsePyrLKOpticalFlowCreate(winSize.Width, winSize.Height, maxLevel, (int)criteria.Type, criteria.MaxCount, criteria.Epsilon, ValidateFlags(flags), minEigThreshold, out IntPtr handle));
            return handle;
        }

        private static int ValidateFlags(OpticalFlowFlags flags)
        {
            const OpticalFlowFlags allowed = OpticalFlowFlags.UseInitialFlow | OpticalFlowFlags.LkGetMinEigenvals;
            if ((flags & ~allowed) != 0) throw new ArgumentOutOfRangeException(nameof(flags));
            return (int)flags;
        }
    }
}

using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Stitching
{
    /// <summary>Base class for camera bundle adjustment methods.</summary>
    public abstract class BundleAdjusterBase : Estimator
    {
        internal BundleAdjusterBase(IntPtr nativeHandle)
            : base(nativeHandle, true)
        {
        }

        /// <summary>Gets or sets an independently copied 3 x 3 CV_8UC1 parameter-refinement mask.</summary>
        public Mat RefinementMask
        {
            get
            {
                var result = new Mat();
                try
                {
                    NativeException.ThrowIfError(NativeMethods.StitchingBundleAdjusterCopyRefinementMask(
                        NativeHandle, result.NativeHandle));
                    return result;
                }
                catch
                {
                    result.Dispose();
                    throw;
                }
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                if (value.Empty || value.Dims != 2 || value.Rows != 3 || value.Cols != 3 ||
                    value.Type != MatType.CV_8UC1)
                {
                    throw new ArgumentException("The refinement mask must be an exact 3 x 3 CV_8UC1 matrix.", nameof(value));
                }
                NativeException.ThrowIfError(NativeMethods.StitchingBundleAdjusterSetRefinementMask(
                    NativeHandle, value.NativeHandle));
                GC.KeepAlive(value);
            }
        }

        /// <summary>Gets or sets the finite pairwise confidence threshold.</summary>
        public double ConfidenceThreshold
        {
            get
            {
                NativeException.ThrowIfError(NativeMethods.StitchingBundleAdjusterGetConfidenceThreshold(
                    NativeHandle, out double value));
                return value;
            }
            set
            {
                if (double.IsNaN(value) || double.IsInfinity(value))
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Confidence threshold must be finite.");
                }
                NativeException.ThrowIfError(NativeMethods.StitchingBundleAdjusterSetConfidenceThreshold(
                    NativeHandle, value));
            }
        }

        /// <summary>Gets or sets the bundle solver termination criteria.</summary>
        public TermCriteria TerminationCriteria
        {
            get
            {
                NativeException.ThrowIfError(NativeMethods.StitchingBundleAdjusterGetTermCriteria(
                    NativeHandle, out int type, out int maxCount, out double epsilon));
                return new TermCriteria((TermCriteriaTypes)type, maxCount, epsilon);
            }
            set
            {
                ValidateTermCriteria(value);
                NativeException.ThrowIfError(NativeMethods.StitchingBundleAdjusterSetTermCriteria(
                    NativeHandle, (int)value.Type, value.MaxCount, value.Epsilon));
            }
        }

        private static void ValidateTermCriteria(TermCriteria value)
        {
            const TermCriteriaTypes allowed = TermCriteriaTypes.CountOrEps;
            if (value.Type == 0 || (value.Type & ~allowed) != 0 || value.MaxCount < 0 ||
                double.IsNaN(value.Epsilon) || double.IsInfinity(value.Epsilon) || value.Epsilon < 0.0 ||
                ((value.Type & TermCriteriaTypes.Count) != 0 && value.MaxCount <= 0) ||
                ((value.Type & TermCriteriaTypes.Eps) != 0 && value.Epsilon <= 0.0))
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Termination criteria must enable a valid positive count and/or epsilon.");
            }
        }
    }

    /// <summary>A bundle adjuster that returns the supplied camera values unchanged.</summary>
    public sealed class NoBundleAdjuster : BundleAdjusterBase
    {
        /// <summary>Creates a no-op bundle adjuster.</summary>
        public NoBundleAdjuster() : base(CreateNative()) { }
        private static IntPtr CreateNative()
        {
            NativeException.ThrowIfError(NativeMethods.StitchingEstimatorCreateNoBundleAdjuster(out IntPtr value));
            return value;
        }
    }

    /// <summary>Minimizes reprojection errors while refining camera parameters.</summary>
    public sealed class BundleAdjusterReproj : BundleAdjusterBase
    {
        /// <summary>Creates a reprojection bundle adjuster.</summary>
        public BundleAdjusterReproj() : base(CreateNative()) { }
        private static IntPtr CreateNative()
        {
            NativeException.ThrowIfError(NativeMethods.StitchingEstimatorCreateBundleAdjusterReproj(out IntPtr value));
            return value;
        }
    }

    /// <summary>Minimizes distances between matched camera rays.</summary>
    public sealed class BundleAdjusterRay : BundleAdjusterBase
    {
        /// <summary>Creates a ray bundle adjuster.</summary>
        public BundleAdjusterRay() : base(CreateNative()) { }
        private static IntPtr CreateNative()
        {
            NativeException.ThrowIfError(NativeMethods.StitchingEstimatorCreateBundleAdjusterRay(out IntPtr value));
            return value;
        }
    }

    /// <summary>Refines all affine transformation parameters.</summary>
    public sealed class BundleAdjusterAffine : BundleAdjusterBase
    {
        /// <summary>Creates a full affine bundle adjuster.</summary>
        public BundleAdjusterAffine() : base(CreateNative()) { }
        private static IntPtr CreateNative()
        {
            NativeException.ThrowIfError(NativeMethods.StitchingEstimatorCreateBundleAdjusterAffine(out IntPtr value));
            return value;
        }
    }

    /// <summary>Refines the four-degree-of-freedom affine transformation.</summary>
    public sealed class BundleAdjusterAffinePartial : BundleAdjusterBase
    {
        /// <summary>Creates a partial affine bundle adjuster.</summary>
        public BundleAdjusterAffinePartial() : base(CreateNative()) { }
        private static IntPtr CreateNative()
        {
            NativeException.ThrowIfError(NativeMethods.StitchingEstimatorCreateBundleAdjusterAffinePartial(out IntPtr value));
            return value;
        }
    }
}

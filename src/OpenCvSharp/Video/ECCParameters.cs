using System;
using OpenCvSharp.Core;
using OpenCvSharp.ImgProc;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Video
{
    /// <summary>Immutable parameters for multiscale ECC registration.</summary>
    public sealed class ECCParameters
    {
        private readonly int[] iterationsPerLevel;

        /// <summary>Initializes ECC parameters with OpenCV 5.0.0 defaults.</summary>
        public ECCParameters(
            MotionType motionType = MotionType.Affine,
            TermCriteria? criteria = null,
            int[]? iterationsPerLevel = null,
            int gaussianFilterSize = 5,
            int levelCount = 4,
            InterpolationFlags interpolation = InterpolationFlags.Linear)
        {
            ValidateMotionType(motionType, nameof(motionType));
            TermCriteria resolvedCriteria = criteria ?? TermCriteria.ByCountAndEpsilon(50, 1e-6);
            ValidateCriteria(resolvedCriteria, nameof(criteria));
            if (gaussianFilterSize < 0 || (gaussianFilterSize != 0 && (gaussianFilterSize & 1) == 0))
            {
                throw new ArgumentOutOfRangeException(nameof(gaussianFilterSize), "The Gaussian filter size must be zero or a positive odd value.");
            }
            if (levelCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(levelCount));
            }
            if (interpolation != InterpolationFlags.Nearest && interpolation != InterpolationFlags.Linear)
            {
                throw new ArgumentOutOfRangeException(nameof(interpolation), "Multiscale ECC supports nearest or linear interpolation only.");
            }

            int[] schedule = iterationsPerLevel == null ? Array.Empty<int>() : (int[])iterationsPerLevel.Clone();
            if (schedule.Length != 0 && schedule.Length != levelCount)
            {
                throw new ArgumentException("The iteration schedule must be empty or contain one value per pyramid level.", nameof(iterationsPerLevel));
            }
            for (int i = 0; i < schedule.Length; i++)
            {
                if (schedule[i] < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(iterationsPerLevel), "Iteration counts cannot be negative.");
                }
            }

            MotionType = motionType;
            Criteria = resolvedCriteria;
            this.iterationsPerLevel = schedule;
            GaussianFilterSize = gaussianFilterSize;
            LevelCount = levelCount;
            Interpolation = interpolation;
        }

        /// <summary>Gets the motion model.</summary>
        public MotionType MotionType { get; }

        /// <summary>Gets the termination criteria.</summary>
        public TermCriteria Criteria { get; }

        /// <summary>Gets an independent copy of the per-level iteration schedule.</summary>
        public int[] IterationsPerLevel => (int[])iterationsPerLevel.Clone();

        /// <summary>Gets the Gaussian smoothing kernel size; zero disables smoothing.</summary>
        public int GaussianFilterSize { get; }

        /// <summary>Gets the number of pyramid levels.</summary>
        public int LevelCount { get; }

        /// <summary>Gets the warp interpolation mode.</summary>
        public InterpolationFlags Interpolation { get; }

        /// <summary>Reads default parameters from the linked OpenCV runtime.</summary>
        public static ECCParameters GetDefaultFromNative()
        {
            NativeException.ThrowIfError(NativeMethods.VideoECCParametersGetDefault(
                out int motionType,
                out int criteriaType,
                out int criteriaMaxCount,
                out double criteriaEpsilon,
                out int gaussianFilterSize,
                out int levelCount,
                out int interpolation));
            return new ECCParameters(
                (MotionType)motionType,
                new TermCriteria((TermCriteriaTypes)criteriaType, criteriaMaxCount, criteriaEpsilon),
                null,
                gaussianFilterSize,
                levelCount,
                (InterpolationFlags)interpolation);
        }

        internal int[] GetIterationSchedule() => iterationsPerLevel;

        internal static void ValidateMotionType(MotionType value, string parameterName)
        {
            if (value < MotionType.Translation || value > MotionType.Homography)
            {
                throw new ArgumentOutOfRangeException(parameterName, value, "Unknown ECC motion type.");
            }
        }

        internal static void ValidateCriteria(TermCriteria value, string parameterName)
        {
            const TermCriteriaTypes valid = TermCriteriaTypes.CountOrEps;
            if ((value.Type & valid) == 0 || (value.Type & ~valid) != 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, "ECC criteria must include count or epsilon without unknown flag bits.");
            }
            if ((value.Type & TermCriteriaTypes.Count) != 0 && value.MaxCount < 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, "The ECC iteration count cannot be negative.");
            }
            if (double.IsNaN(value.Epsilon) || double.IsInfinity(value.Epsilon))
            {
                throw new ArgumentOutOfRangeException(parameterName, "The ECC epsilon must be finite.");
            }
        }
    }
}

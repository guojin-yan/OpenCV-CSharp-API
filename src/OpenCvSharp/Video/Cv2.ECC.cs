using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Video
{
    public static unsafe partial class Cv2
    {
        private static readonly TermCriteria DefaultEccCriteria = TermCriteria.ByCountAndEpsilon(50, 0.001);
        private static readonly TermCriteria DefaultDualMaskEccCriteria = TermCriteria.ByCountAndEpsilon(50, 1e-6);

        /// <summary>Computes the enhanced correlation coefficient between two images.</summary>
        public static double ComputeECC(Mat templateImage, Mat inputImage, Mat? inputMask = null)
        {
            ValidateEccImagePair(templateImage, inputImage, true, true);
            ValidateEccMask(inputMask, templateImage, nameof(inputMask));
            NativeException.ThrowIfError(NativeMethods.VideoComputeECC(
                templateImage.NativeHandle,
                inputImage.NativeHandle,
                inputMask == null ? IntPtr.Zero : inputMask.NativeHandle,
                out double result));
            return result;
        }

        /// <summary>Refines a caller-owned warp matrix with single-mask ECC registration.</summary>
        public static double FindTransformECC(
            Mat templateImage,
            Mat inputImage,
            Mat warpMatrix,
            MotionType motionType = MotionType.Affine,
            TermCriteria? criteria = null,
            Mat? inputMask = null,
            int gaussianFilterSize = 5)
        {
            ValidateEccImagePair(templateImage, inputImage, false, true);
            ValidateNotNull(warpMatrix, nameof(warpMatrix));
            ECCParameters.ValidateMotionType(motionType, nameof(motionType));
            TermCriteria resolvedCriteria = criteria ?? DefaultEccCriteria;
            ECCParameters.ValidateCriteria(resolvedCriteria, nameof(criteria));
            ValidateSingleScaleGaussianFilter(gaussianFilterSize, nameof(gaussianFilterSize));
            ValidateSingleScaleWarp(warpMatrix, motionType, nameof(warpMatrix));
            ValidateEccMask(inputMask, inputImage, nameof(inputMask));

            NativeException.ThrowIfError(NativeMethods.VideoFindTransformECC(
                templateImage.NativeHandle,
                inputImage.NativeHandle,
                warpMatrix.NativeHandle,
                (int)motionType,
                (int)resolvedCriteria.Type,
                resolvedCriteria.MaxCount,
                resolvedCriteria.Epsilon,
                inputMask == null ? IntPtr.Zero : inputMask.NativeHandle,
                gaussianFilterSize,
                out double result));
            return result;
        }

        /// <summary>Allocates and owns an identity-initialized warp for single-mask ECC registration.</summary>
        public static ECCRegistrationResult FindTransformECC(
            Mat templateImage,
            Mat inputImage,
            MotionType motionType = MotionType.Affine,
            TermCriteria? criteria = null,
            Mat? inputMask = null,
            int gaussianFilterSize = 5)
        {
            var warpMatrix = new Mat();
            try
            {
                double score = FindTransformECC(
                    templateImage,
                    inputImage,
                    warpMatrix,
                    motionType,
                    criteria,
                    inputMask,
                    gaussianFilterSize);
                return new ECCRegistrationResult(score, warpMatrix);
            }
            catch
            {
                warpMatrix.Dispose();
                throw;
            }
        }

        /// <summary>Refines a caller-owned warp matrix with template and input masks.</summary>
        public static double FindTransformECCWithMask(
            Mat templateImage,
            Mat inputImage,
            Mat templateMask,
            Mat inputMask,
            Mat warpMatrix,
            MotionType motionType = MotionType.Affine,
            TermCriteria? criteria = null,
            int gaussianFilterSize = 5)
        {
            ValidateEccImagePair(templateImage, inputImage, false, true);
            ValidateNotNull(templateMask, nameof(templateMask));
            ValidateNotNull(inputMask, nameof(inputMask));
            ValidateNotNull(warpMatrix, nameof(warpMatrix));
            ECCParameters.ValidateMotionType(motionType, nameof(motionType));
            TermCriteria resolvedCriteria = criteria ?? DefaultDualMaskEccCriteria;
            ECCParameters.ValidateCriteria(resolvedCriteria, nameof(criteria));
            ValidateSingleScaleGaussianFilter(gaussianFilterSize, nameof(gaussianFilterSize));
            ValidateSingleScaleWarp(warpMatrix, motionType, nameof(warpMatrix));
            ValidateEccMask(templateMask, templateImage, nameof(templateMask));
            ValidateEccMask(inputMask, inputImage, nameof(inputMask));

            NativeException.ThrowIfError(NativeMethods.VideoFindTransformECCWithMask(
                templateImage.NativeHandle,
                inputImage.NativeHandle,
                templateMask.NativeHandle,
                inputMask.NativeHandle,
                warpMatrix.NativeHandle,
                (int)motionType,
                (int)resolvedCriteria.Type,
                resolvedCriteria.MaxCount,
                resolvedCriteria.Epsilon,
                gaussianFilterSize,
                out double result));
            return result;
        }

        /// <summary>Allocates and owns an identity-initialized warp for dual-mask ECC registration.</summary>
        public static ECCRegistrationResult FindTransformECCWithMask(
            Mat templateImage,
            Mat inputImage,
            Mat templateMask,
            Mat inputMask,
            MotionType motionType = MotionType.Affine,
            TermCriteria? criteria = null,
            int gaussianFilterSize = 5)
        {
            var warpMatrix = new Mat();
            try
            {
                double score = FindTransformECCWithMask(
                    templateImage,
                    inputImage,
                    templateMask,
                    inputMask,
                    warpMatrix,
                    motionType,
                    criteria,
                    gaussianFilterSize);
                return new ECCRegistrationResult(score, warpMatrix);
            }
            catch
            {
                warpMatrix.Dispose();
                throw;
            }
        }

        /// <summary>Refines a caller-owned warp matrix with multiscale ECC registration.</summary>
        public static double FindTransformECCMultiScale(
            Mat referenceImage,
            Mat sampleImage,
            Mat warpMatrix,
            ECCParameters? parameters = null,
            Mat? referenceMask = null,
            Mat? sampleMask = null)
        {
            ValidateEccImagePair(referenceImage, sampleImage, false, false);
            ValidateNotNull(warpMatrix, nameof(warpMatrix));
            ECCParameters resolved = parameters ?? new ECCParameters();
            ValidateMultiScaleWarp(warpMatrix, resolved.MotionType, nameof(warpMatrix));
            ValidateEccMask(referenceMask, referenceImage, nameof(referenceMask));
            ValidateEccMask(sampleMask, sampleImage, nameof(sampleMask));
            int[] schedule = resolved.GetIterationSchedule();

            fixed (int* schedulePointer = schedule)
            {
                NativeException.ThrowIfError(NativeMethods.VideoFindTransformECCMultiScale(
                    referenceImage.NativeHandle,
                    sampleImage.NativeHandle,
                    warpMatrix.NativeHandle,
                    (int)resolved.MotionType,
                    (int)resolved.Criteria.Type,
                    resolved.Criteria.MaxCount,
                    resolved.Criteria.Epsilon,
                    schedulePointer,
                    schedule.Length,
                    resolved.GaussianFilterSize,
                    resolved.LevelCount,
                    (int)resolved.Interpolation,
                    referenceMask == null ? IntPtr.Zero : referenceMask.NativeHandle,
                    sampleMask == null ? IntPtr.Zero : sampleMask.NativeHandle,
                    out double result));
                return result;
            }
        }

        /// <summary>Allocates and owns an identity-initialized warp for multiscale ECC registration.</summary>
        public static ECCRegistrationResult FindTransformECCMultiScale(
            Mat referenceImage,
            Mat sampleImage,
            ECCParameters? parameters = null,
            Mat? referenceMask = null,
            Mat? sampleMask = null)
        {
            var warpMatrix = new Mat();
            try
            {
                double score = FindTransformECCMultiScale(
                    referenceImage,
                    sampleImage,
                    warpMatrix,
                    parameters,
                    referenceMask,
                    sampleMask);
                return new ECCRegistrationResult(score, warpMatrix);
            }
            catch
            {
                warpMatrix.Dispose();
                throw;
            }
        }

        private static void ValidateEccImagePair(Mat first, Mat second, bool requireSameSize, bool allowThreeChannels)
        {
            ValidateNotNull(first, nameof(first));
            ValidateNotNull(second, nameof(second));
            if (first.Empty || second.Empty || first.Dims != 2 || second.Dims != 2)
            {
                throw new ArgumentException("ECC images must be non-empty two-dimensional Mats.");
            }
            if (first.Type != second.Type)
            {
                throw new ArgumentException("ECC images must have the same Mat type.");
            }
            if (first.Channels != 1 && (!allowThreeChannels || first.Channels != 3))
            {
                throw new ArgumentException(allowThreeChannels
                    ? "ECC images must have one or three channels."
                    : "Multiscale ECC images must be single-channel.");
            }
            if (first.Depth != MatType.CV_8U && first.Depth != MatType.CV_16U &&
                first.Depth != MatType.CV_32F && first.Depth != MatType.CV_64F)
            {
                throw new ArgumentException("ECC images must use CV_8U, CV_16U, CV_32F, or CV_64F depth.");
            }
            if (requireSameSize && (first.Rows != second.Rows || first.Cols != second.Cols))
            {
                throw new ArgumentException("These ECC inputs must have matching dimensions.");
            }
        }

        private static void ValidateEccMask(Mat? mask, Mat image, string parameterName)
        {
            if (mask == null)
            {
                return;
            }
            _ = mask.NativeHandle;
            if (mask.Empty)
            {
                return;
            }
            if (mask.Dims != 2 || mask.Type != MatType.CV_8UC1 || mask.Rows != image.Rows || mask.Cols != image.Cols)
            {
                throw new ArgumentException("ECC masks must be empty or matching two-dimensional CV_8UC1 Mats.", parameterName);
            }
        }

        private static void ValidateSingleScaleWarp(Mat warpMatrix, MotionType motionType, string parameterName)
        {
            _ = warpMatrix.NativeHandle;
            if (warpMatrix.Empty)
            {
                return;
            }
            if (warpMatrix.Dims != 2 || warpMatrix.Type != MatType.CV_32FC1 ||
                warpMatrix.Cols != 3 || (warpMatrix.Rows != 2 && warpMatrix.Rows != 3) ||
                (motionType == MotionType.Homography && warpMatrix.Rows != 3))
            {
                throw new ArgumentException("Single-scale ECC requires an empty or CV_32FC1 2x3/3x3 warp compatible with the motion type.", parameterName);
            }
        }

        private static void ValidateMultiScaleWarp(Mat warpMatrix, MotionType motionType, string parameterName)
        {
            _ = warpMatrix.NativeHandle;
            if (warpMatrix.Empty)
            {
                return;
            }
            if (warpMatrix.Dims != 2 ||
                (warpMatrix.Type != MatType.CV_32FC1 && warpMatrix.Type != MatType.CV_64FC1) ||
                warpMatrix.Cols != 3 || (warpMatrix.Rows != 2 && warpMatrix.Rows != 3) ||
                (motionType == MotionType.Homography && warpMatrix.Rows != 3))
            {
                throw new ArgumentException("Multiscale ECC requires an empty or floating-point 2x3/3x3 warp compatible with the motion type.", parameterName);
            }
        }

        private static void ValidateSingleScaleGaussianFilter(int value, string parameterName)
        {
            if (value <= 0 || (value & 1) == 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Single-scale ECC requires a positive odd Gaussian filter size.");
            }
        }
    }
}

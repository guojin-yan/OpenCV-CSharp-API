using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Photo
{
    /// <summary>Fits and applies a color correction matrix from measured and reference colors.</summary>
    public sealed class ColorCorrectionModel : IDisposable
    {
        private const double InclusiveUnitUpperBound = 1.0000000000000002;

        private readonly NativeColorCorrectionModelHandle handle;
        private readonly int sampleCount;
        private readonly bool canCompute;
        private bool ready;
        private bool disposed;

        private ColorCorrectionModel(
            NativeColorCorrectionModelHandle handle,
            int sampleCount,
            bool canCompute)
        {
            this.handle = handle;
            this.sampleCount = sampleCount;
            this.canCompute = canCompute;
        }

        /// <summary>Gets whether this model has been disposed.</summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        internal IntPtr NativeHandle
        {
            get
            {
                ThrowIfDisposed();
                return handle.DangerousGetHandle();
            }
        }

        /// <summary>Creates an empty model intended for loading from persistence.</summary>
        public static ColorCorrectionModel Create()
        {
            NativeException.ThrowIfError(NativeMethods.PhotoCcmCreate(out IntPtr native));
            return new ColorCorrectionModel(
                NativeColorCorrectionModelHandle.FromNativePointer(native),
                sampleCount: 0,
                canCompute: false);
        }

        /// <summary>Creates a model from measured colors and a built-in color checker.</summary>
        public static ColorCorrectionModel Create(Mat src, ColorCheckerType colorChecker)
        {
            int count = GetColorCheckerCount(colorChecker, nameof(colorChecker));
            ValidateColorSamples(src, nameof(src), count, requireUnitRange: true);
            NativeException.ThrowIfError(NativeMethods.PhotoCcmCreateColorChecker(
                src.NativeHandle, (int)colorChecker, out IntPtr native));
            return new ColorCorrectionModel(
                NativeColorCorrectionModelHandle.FromNativePointer(native),
                count,
                canCompute: true);
        }

        /// <summary>Creates a model from measured and custom reference colors.</summary>
        public static ColorCorrectionModel Create(
            Mat src,
            Mat colors,
            ColorSpace referenceColorSpace)
        {
            ValidateReferenceColorSpace(referenceColorSpace, nameof(referenceColorSpace));
            int count = ValidateColorSamples(src, nameof(src), requireUnitRange: true);
            ValidateColorSamples(
                colors,
                nameof(colors),
                count,
                requireUnitRange: IsRgbColorSpace(referenceColorSpace));
            NativeException.ThrowIfError(NativeMethods.PhotoCcmCreateReferenceColors(
                src.NativeHandle,
                colors.NativeHandle,
                (int)referenceColorSpace,
                out IntPtr native));
            return new ColorCorrectionModel(
                NativeColorCorrectionModelHandle.FromNativePointer(native),
                count,
                canCompute: true);
        }

        /// <summary>Creates a model from measured colors, custom reference colors, and a colored-patch mask.</summary>
        public static ColorCorrectionModel Create(
            Mat src,
            Mat colors,
            ColorSpace referenceColorSpace,
            Mat coloredPatchesMask)
        {
            ValidateReferenceColorSpace(referenceColorSpace, nameof(referenceColorSpace));
            int count = ValidateColorSamples(src, nameof(src), requireUnitRange: true);
            ValidateColorSamples(
                colors,
                nameof(colors),
                count,
                requireUnitRange: IsRgbColorSpace(referenceColorSpace));
            ValidateColoredPatchesMask(coloredPatchesMask, count, nameof(coloredPatchesMask));
            NativeException.ThrowIfError(NativeMethods.PhotoCcmCreateReferenceColorsMasked(
                src.NativeHandle,
                colors.NativeHandle,
                (int)referenceColorSpace,
                coloredPatchesMask.NativeHandle,
                out IntPtr native));
            return new ColorCorrectionModel(
                NativeColorCorrectionModelHandle.FromNativePointer(native),
                count,
                canCompute: true);
        }

        /// <summary>Sets the nonlinear RGB color space used for measured colors.</summary>
        public void SetColorSpace(ColorSpace colorSpace)
        {
            ThrowIfDisposed();
            int value = (int)colorSpace;
            if (value < (int)ColorSpace.Srgb ||
                value > (int)ColorSpace.Rec2020Rgb ||
                (value & 1) != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(colorSpace), "Model color space must be a supported nonlinear RGB color space.");
            }

            NativeException.ThrowIfError(NativeMethods.PhotoCcmSetColorSpace(NativeHandle, value));
            ready = false;
        }

        /// <summary>Sets the shape of the fitted color correction matrix.</summary>
        public void SetCcmType(CcmType ccmType)
        {
            ThrowIfDisposed();
            ValidateEnumRange((int)ccmType, 0, 1, nameof(ccmType));
            NativeException.ThrowIfError(NativeMethods.PhotoCcmSetCcmType(NativeHandle, (int)ccmType));
            ready = false;
        }

        /// <summary>Sets the color distance used during fitting.</summary>
        public void SetDistance(DistanceType distance)
        {
            ThrowIfDisposed();
            ValidateEnumRange((int)distance, 0, 7, nameof(distance));
            NativeException.ThrowIfError(NativeMethods.PhotoCcmSetDistance(NativeHandle, (int)distance));
            ready = false;
        }

        /// <summary>Sets the method used to linearize measured RGB colors.</summary>
        public void SetLinearization(LinearizationType linearization)
        {
            ThrowIfDisposed();
            ValidateEnumRange((int)linearization, 0, 5, nameof(linearization));
            NativeException.ThrowIfError(NativeMethods.PhotoCcmSetLinearization(NativeHandle, (int)linearization));
            ready = false;
        }

        /// <summary>Sets the positive gamma used by gamma linearization.</summary>
        public void SetLinearizationGamma(double gamma)
        {
            ThrowIfDisposed();
            ValidatePositiveFinite(gamma, nameof(gamma));
            NativeException.ThrowIfError(NativeMethods.PhotoCcmSetLinearizationGamma(NativeHandle, gamma));
            ready = false;
        }

        /// <summary>Sets the positive polynomial degree used by polynomial linearization.</summary>
        public void SetLinearizationDegree(int degree)
        {
            ThrowIfDisposed();
            if (degree <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(degree), "Linearization degree must be positive.");
            }

            NativeException.ThrowIfError(NativeMethods.PhotoCcmSetLinearizationDegree(NativeHandle, degree));
            ready = false;
        }

        /// <summary>Sets the closed source-color interval retained for model fitting.</summary>
        public void SetSaturatedThreshold(double lower, double upper)
        {
            ThrowIfDisposed();
            if (!IsFinite(lower) || !IsFinite(upper) || lower < 0.0 || lower >= upper || upper > 1.0)
            {
                throw new ArgumentOutOfRangeException(nameof(lower), "Saturation thresholds must satisfy 0 <= lower < upper <= 1.");
            }

            NativeException.ThrowIfError(NativeMethods.PhotoCcmSetSaturatedThreshold(NativeHandle, lower, upper));
            ready = false;
        }

        /// <summary>Sets an optional per-sample weight list. The native model keeps an independent copy.</summary>
        public void SetWeightsList(Mat weightsList)
        {
            ThrowIfDisposed();
            if (weightsList == null)
            {
                throw new ArgumentNullException(nameof(weightsList));
            }

            if (!weightsList.Empty)
            {
                if (sampleCount <= 0 || weightsList.Dims != 2 ||
                    weightsList.Type != MatType.CV_64FC1 ||
                    weightsList.Rows != sampleCount || weightsList.Cols != 1)
                {
                    throw new ArgumentException("Weights must be an N x 1 CV_64FC1 matrix matching the model sample count.", nameof(weightsList));
                }

                ValidateFiniteMatrix(weightsList, nameof(weightsList));
            }

            NativeException.ThrowIfError(NativeMethods.PhotoCcmSetWeightsList(NativeHandle, weightsList.NativeHandle));
            ready = false;
        }

        /// <summary>Sets the finite luminance weight exponent.</summary>
        public void SetWeightCoeff(double weightCoeff)
        {
            ThrowIfDisposed();
            ValidateFinite(weightCoeff, nameof(weightCoeff));
            NativeException.ThrowIfError(NativeMethods.PhotoCcmSetWeightCoeff(NativeHandle, weightCoeff));
            ready = false;
        }

        /// <summary>Sets the initial color correction matrix estimation method.</summary>
        public void SetInitialMethod(InitialMethodType initialMethod)
        {
            ThrowIfDisposed();
            ValidateEnumRange((int)initialMethod, 0, 1, nameof(initialMethod));
            NativeException.ThrowIfError(NativeMethods.PhotoCcmSetInitialMethod(NativeHandle, (int)initialMethod));
            ready = false;
        }

        /// <summary>Sets the positive maximum optimization iteration count.</summary>
        public void SetMaxCount(int maxCount)
        {
            ThrowIfDisposed();
            if (maxCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxCount), "Maximum iteration count must be positive.");
            }

            NativeException.ThrowIfError(NativeMethods.PhotoCcmSetMaxCount(NativeHandle, maxCount));
            ready = false;
        }

        /// <summary>Sets the positive finite optimization epsilon.</summary>
        public void SetEpsilon(double epsilon)
        {
            ThrowIfDisposed();
            ValidatePositiveFinite(epsilon, nameof(epsilon));
            NativeException.ThrowIfError(NativeMethods.PhotoCcmSetEpsilon(NativeHandle, epsilon));
            ready = false;
        }

        /// <summary>Sets whether image input is converted from BGR to RGB before correction.</summary>
        public void SetRGB(bool rgb)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.PhotoCcmSetRgb(NativeHandle, rgb ? 1 : 0));
        }

        /// <summary>Fits the model and writes the color correction matrix to a caller-owned output.</summary>
        public void Compute(Mat colorCorrectionMatrix)
        {
            ThrowIfDisposed();
            RequireOutput(colorCorrectionMatrix, nameof(colorCorrectionMatrix));
            if (!canCompute)
            {
                throw new InvalidOperationException("This model has no source samples. Create it from colors before computing, or load it with Read.");
            }

            ready = false;
            NativeException.ThrowIfError(NativeMethods.PhotoCcmCompute(NativeHandle, colorCorrectionMatrix.NativeHandle));
            ready = true;
        }

        /// <summary>Fits the model and returns an independently owned color correction matrix.</summary>
        public Mat Compute()
        {
            return CreateMat(Compute);
        }

        /// <summary>Copies the fitted color correction matrix to a caller-owned output.</summary>
        public void GetColorCorrectionMatrix(Mat colorCorrectionMatrix)
        {
            RequireReady();
            RequireOutput(colorCorrectionMatrix, nameof(colorCorrectionMatrix));
            NativeException.ThrowIfError(NativeMethods.PhotoCcmGetColorCorrectionMatrix(
                NativeHandle, colorCorrectionMatrix.NativeHandle));
        }

        /// <summary>Returns an independently owned copy of the fitted color correction matrix.</summary>
        public Mat GetColorCorrectionMatrix()
        {
            return CreateMat(GetColorCorrectionMatrix);
        }

        /// <summary>Gets the loss from the most recent successful compute or read operation.</summary>
        public double GetLoss()
        {
            RequireReady();
            NativeException.ThrowIfError(NativeMethods.PhotoCcmGetLoss(NativeHandle, out double loss));
            return loss;
        }

        /// <summary>Copies the retained linearized source RGB samples to a caller-owned output.</summary>
        public void GetSrcLinearRGB(Mat srcLinearRgb)
        {
            RequireReady();
            RequireOutput(srcLinearRgb, nameof(srcLinearRgb));
            NativeException.ThrowIfError(NativeMethods.PhotoCcmGetSrcLinearRgb(NativeHandle, srcLinearRgb.NativeHandle));
        }

        /// <summary>Returns an independently owned copy of the retained linearized source RGB samples.</summary>
        public Mat GetSrcLinearRGB()
        {
            return CreateMat(GetSrcLinearRGB);
        }

        /// <summary>Copies the retained linearized reference RGB samples to a caller-owned output.</summary>
        public void GetRefLinearRGB(Mat refLinearRgb)
        {
            RequireReady();
            RequireOutput(refLinearRgb, nameof(refLinearRgb));
            NativeException.ThrowIfError(NativeMethods.PhotoCcmGetRefLinearRgb(NativeHandle, refLinearRgb.NativeHandle));
        }

        /// <summary>Returns an independently owned copy of the retained linearized reference RGB samples.</summary>
        public Mat GetRefLinearRGB()
        {
            return CreateMat(GetRefLinearRGB);
        }

        /// <summary>Copies the fitted sample-selection mask to a caller-owned output.</summary>
        public void GetMask(Mat mask)
        {
            RequireReady();
            RequireOutput(mask, nameof(mask));
            NativeException.ThrowIfError(NativeMethods.PhotoCcmGetMask(NativeHandle, mask.NativeHandle));
        }

        /// <summary>Returns an independently owned copy of the fitted sample-selection mask.</summary>
        public Mat GetMask()
        {
            return CreateMat(GetMask);
        }

        /// <summary>Copies the normalized fitted sample weights to a caller-owned output.</summary>
        public void GetWeights(Mat weights)
        {
            RequireReady();
            RequireOutput(weights, nameof(weights));
            NativeException.ThrowIfError(NativeMethods.PhotoCcmGetWeights(NativeHandle, weights.NativeHandle));
        }

        /// <summary>Returns an independently owned copy of the normalized fitted sample weights.</summary>
        public Mat GetWeights()
        {
            return CreateMat(GetWeights);
        }

        /// <summary>Applies the fitted model to a three-channel image.</summary>
        public void CorrectImage(Mat src, Mat dst, bool isLinear = false)
        {
            RequireReady();
            ValidateCorrectionImage(src, nameof(src));
            RequireOutput(dst, nameof(dst));
            NativeException.ThrowIfError(NativeMethods.PhotoCcmCorrectImage(
                NativeHandle, src.NativeHandle, dst.NativeHandle, isLinear ? 1 : 0));
        }

        /// <summary>Applies the fitted model and returns an independently owned output image.</summary>
        public Mat CorrectImage(Mat src, bool isLinear = false)
        {
            var dst = new Mat();
            try
            {
                CorrectImage(src, dst, isLinear);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>Writes the ready model as a top-level ColorCorrectionModel map.</summary>
        public void Write(FileStorage storage)
        {
            RequireReady();
            if (storage == null)
            {
                throw new ArgumentNullException(nameof(storage));
            }

            NativeException.ThrowIfError(NativeMethods.PhotoCcmWrite(NativeHandle, storage.NativeHandle));
        }

        /// <summary>Loads a model from the inner ColorCorrectionModel map node.</summary>
        public void Read(FileNode node)
        {
            ThrowIfDisposed();
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            ready = false;
            NativeException.ThrowIfError(NativeMethods.PhotoCcmRead(NativeHandle, node.NativeHandle));
            ready = true;
        }

        /// <summary>Releases the native color correction model.</summary>
        public void Dispose()
        {
            if (!disposed)
            {
                handle.Dispose();
                disposed = true;
                ready = false;
            }

            GC.SuppressFinalize(this);
        }

        private static Mat CreateMat(Action<Mat> fill)
        {
            var result = new Mat();
            try
            {
                fill(result);
                return result;
            }
            catch
            {
                result.Dispose();
                throw;
            }
        }

        private static int ValidateColorSamples(
            Mat value,
            string parameterName,
            int expectedCount = -1,
            bool requireUnitRange = false)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            if (value.Empty || value.Dims != 2 || value.Type != MatType.CV_64FC3 ||
                value.Cols != 1 || (expectedCount >= 0 && value.Rows != expectedCount))
            {
                throw new ArgumentException("Color samples must be a non-empty N x 1 CV_64FC3 matrix.", parameterName);
            }

            CheckRangeResult range = requireUnitRange
                ? Cv2.CheckRange(value, 0.0, InclusiveUnitUpperBound)
                : Cv2.CheckRange(value);
            if (!range.IsValid)
            {
                throw new ArgumentException(
                    requireUnitRange
                        ? "Color sample values must be finite and lie in the closed interval [0, 1]."
                        : "Color sample values must be finite.",
                    parameterName);
            }

            return value.Rows;
        }

        private static void ValidateColoredPatchesMask(Mat value, int expectedCount, string parameterName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            if (value.Empty || value.Dims != 2 || value.Type != MatType.CV_8UC1 ||
                value.Rows != expectedCount || value.Cols != 1)
            {
                throw new ArgumentException("Colored-patch mask must be an N x 1 CV_8UC1 matrix matching the sample count.", parameterName);
            }

            if (!Cv2.CheckRange(value, 0.0, 2.0).IsValid)
            {
                throw new ArgumentException("Colored-patch mask values must be binary zero or one.", parameterName);
            }
        }

        private static void ValidateCorrectionImage(Mat value, string parameterName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            int type = value.Type;
            if (value.Empty || value.Dims != 2 ||
                (type != MatType.CV_8UC3 && type != MatType.CV_16UC3 && type != MatType.CV_32FC3))
            {
                throw new ArgumentException("Correction input must be a non-empty CV_8UC3, CV_16UC3, or CV_32FC3 image.", parameterName);
            }
        }

        private static void ValidateFiniteMatrix(Mat value, string parameterName)
        {
            if (!Cv2.CheckRange(value).IsValid)
            {
                throw new ArgumentException("Matrix values must be finite.", parameterName);
            }
        }

        private static int GetColorCheckerCount(ColorCheckerType colorChecker, string parameterName)
        {
            switch (colorChecker)
            {
                case ColorCheckerType.Macbeth:
                case ColorCheckerType.Vinyl:
                    return 24;
                case ColorCheckerType.DigitalSg:
                    return 140;
                default:
                    throw new ArgumentOutOfRangeException(parameterName);
            }
        }

        private static void ValidateReferenceColorSpace(ColorSpace value, string parameterName)
        {
            ValidateEnumRange((int)value, 0, 39, parameterName);
        }

        private static bool IsRgbColorSpace(ColorSpace value)
        {
            return (int)value >= (int)ColorSpace.Srgb && (int)value <= (int)ColorSpace.Rec2020RgbLinear;
        }

        private static void ValidateEnumRange(int value, int minimum, int maximum, string parameterName)
        {
            if (value < minimum || value > maximum)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }

        private static void ValidatePositiveFinite(double value, string parameterName)
        {
            if (!(value > 0.0) || !IsFinite(value))
            {
                throw new ArgumentOutOfRangeException(parameterName, "Value must be finite and positive.");
            }
        }

        private static void ValidateFinite(double value, string parameterName)
        {
            if (!IsFinite(value))
            {
                throw new ArgumentOutOfRangeException(parameterName, "Value must be finite.");
            }
        }

        private static bool IsFinite(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }

        private static void RequireOutput(Mat value, string parameterName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            _ = value.NativeHandle;
        }

        private void RequireReady()
        {
            ThrowIfDisposed();
            if (!ready)
            {
                throw new InvalidOperationException("The color correction model is not ready. Call Compute or Read first.");
            }
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }
}

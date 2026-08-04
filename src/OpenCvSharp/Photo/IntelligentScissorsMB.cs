using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Photo
{
    /// <summary>Computes minimum-cost live-wire contours from image edge features.</summary>
    /// <remarks>Instances are stateful and are not thread-safe.</remarks>
    public sealed class IntelligentScissorsMB : IDisposable
    {
        private const float FloatEpsilon = 1.192092896e-07F;

        private readonly NativeIntelligentScissorsHandle handle;
        private float weightNonEdge = 0.43F;
        private float weightGradientDirection = 0.43F;
        private float weightGradientMagnitude = 0.14F;
        private int width;
        private int height;
        private bool featuresApplied;
        private bool mapBuilt;
        private bool disposed;

        /// <summary>Creates an Intelligent Scissors model with OpenCV's default weights and zero-crossing edge mode.</summary>
        public IntelligentScissorsMB()
        {
            NativeException.ThrowIfError(NativeMethods.PhotoIntelligentScissorsCreate(out IntPtr native));
            handle = NativeIntelligentScissorsHandle.FromNativePointer(native);
        }

        /// <summary>Gets whether this instance has been disposed.</summary>
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

        /// <summary>Sets non-negative feature weights whose sum must be greater than zero.</summary>
        public IntelligentScissorsMB SetWeights(
            float weightNonEdge,
            float weightGradientDirection,
            float weightGradientMagnitude)
        {
            ThrowIfDisposed();
            ValidateNonNegativeFinite(weightNonEdge, nameof(weightNonEdge));
            ValidateNonNegativeFinite(weightGradientDirection, nameof(weightGradientDirection));
            ValidateNonNegativeFinite(weightGradientMagnitude, nameof(weightGradientMagnitude));
            double sum = (double)weightNonEdge + weightGradientDirection + weightGradientMagnitude;
            if (sum <= FloatEpsilon)
            {
                throw new ArgumentOutOfRangeException(nameof(weightNonEdge), "The sum of feature weights must be greater than FLT_EPSILON.");
            }

            NativeException.ThrowIfError(NativeMethods.PhotoIntelligentScissorsSetWeights(
                NativeHandle,
                weightNonEdge,
                weightGradientDirection,
                weightGradientMagnitude));
            this.weightNonEdge = weightNonEdge;
            this.weightGradientDirection = weightGradientDirection;
            this.weightGradientMagnitude = weightGradientMagnitude;
            InvalidateFeatures();
            return this;
        }

        /// <summary>Sets the non-negative gradient-magnitude maximum limit; zero disables thresholding.</summary>
        public IntelligentScissorsMB SetGradientMagnitudeMaxLimit(float gradientMagnitudeThresholdMax = 0.0F)
        {
            ThrowIfDisposed();
            ValidateNonNegativeFinite(gradientMagnitudeThresholdMax, nameof(gradientMagnitudeThresholdMax));
            NativeException.ThrowIfError(NativeMethods.PhotoIntelligentScissorsSetGradientMagnitudeMaxLimit(
                NativeHandle,
                gradientMagnitudeThresholdMax));
            InvalidateFeatures();
            return this;
        }

        /// <summary>Uses the zero-crossing edge feature with an optional non-negative noise threshold.</summary>
        public IntelligentScissorsMB SetEdgeFeatureZeroCrossingParameters(float gradientMagnitudeMinValue = 0.0F)
        {
            ThrowIfDisposed();
            ValidateNonNegativeFinite(gradientMagnitudeMinValue, nameof(gradientMagnitudeMinValue));
            NativeException.ThrowIfError(NativeMethods.PhotoIntelligentScissorsSetEdgeFeatureZeroCrossingParameters(
                NativeHandle,
                gradientMagnitudeMinValue));
            InvalidateFeatures();
            return this;
        }

        /// <summary>Uses Canny edge extraction with OpenCV-compatible aperture settings.</summary>
        public IntelligentScissorsMB SetEdgeFeatureCannyParameters(
            double threshold1,
            double threshold2,
            int apertureSize = 3,
            bool l2Gradient = false)
        {
            ThrowIfDisposed();
            ValidateNonNegativeFinite(threshold1, nameof(threshold1));
            ValidateNonNegativeFinite(threshold2, nameof(threshold2));
            if (apertureSize != -1 && apertureSize != 3 && apertureSize != 5 && apertureSize != 7)
            {
                throw new ArgumentOutOfRangeException(nameof(apertureSize), "Canny aperture size must be -1, 3, 5, or 7.");
            }

            NativeException.ThrowIfError(NativeMethods.PhotoIntelligentScissorsSetEdgeFeatureCannyParameters(
                NativeHandle,
                threshold1,
                threshold2,
                apertureSize,
                l2Gradient ? 1 : 0));
            InvalidateFeatures();
            return this;
        }

        /// <summary>Extracts all features from a non-empty CV_8UC1, CV_8UC3, or CV_8UC4 image.</summary>
        public IntelligentScissorsMB ApplyImage(Mat image)
        {
            ThrowIfDisposed();
            ValidateImage(image, nameof(image), optional: false, out Size size);
            NativeException.ThrowIfError(NativeMethods.PhotoIntelligentScissorsApplyImage(
                NativeHandle,
                image.NativeHandle));
            SetAppliedSize(size);
            return this;
        }

        /// <summary>
        /// Applies custom features. Missing non-zero-weight features are derived from <paramref name="image"/>.
        /// </summary>
        /// <remarks>
        /// Supplied feature Mats are retained by OpenCV through reference counting. Disposing their managed wrappers is safe,
        /// but mutating shared feature storage before <see cref="BuildMap"/> changes the calculation.
        /// </remarks>
        public IntelligentScissorsMB ApplyImageFeatures(
            Mat? nonEdge,
            Mat? gradientDirection,
            Mat? gradientMagnitude,
            Mat? image = null)
        {
            ThrowIfDisposed();
            Size size = default;
            bool hasNonEdge = ValidateOptionalFeature(nonEdge, nameof(nonEdge), MatType.CV_8UC1, ref size);
            bool hasGradientDirection = ValidateOptionalFeature(
                gradientDirection,
                nameof(gradientDirection),
                MatType.CV_32FC2,
                ref size);
            bool hasGradientMagnitude = ValidateOptionalFeature(
                gradientMagnitude,
                nameof(gradientMagnitude),
                MatType.CV_32FC1,
                ref size);
            bool hasImage = ValidateImage(image, nameof(image), optional: true, out Size imageSize);
            MergeSize(ref size, imageSize, hasImage, nameof(image));

            if (size.Width <= 0 || size.Height <= 0)
            {
                throw new ArgumentException("At least one non-empty feature or image is required.");
            }
            if ((!hasNonEdge && weightNonEdge != 0.0F) ||
                (!hasGradientDirection && weightGradientDirection != 0.0F) ||
                (!hasGradientMagnitude && weightGradientMagnitude != 0.0F))
            {
                if (!hasImage)
                {
                    throw new ArgumentException("An image is required to derive each omitted feature with a non-zero weight.", nameof(image));
                }
            }

            NativeException.ThrowIfError(NativeMethods.PhotoIntelligentScissorsApplyImageFeatures(
                NativeHandle,
                OptionalNativeHandle(nonEdge, hasNonEdge),
                OptionalNativeHandle(gradientDirection, hasGradientDirection),
                OptionalNativeHandle(gradientMagnitude, hasGradientMagnitude),
                OptionalNativeHandle(image, hasImage)));
            SetAppliedSize(size);
            return this;
        }

        /// <summary>Builds the optimal-path map from a source point inside the applied image.</summary>
        public void BuildMap(Point sourcePoint)
        {
            ThrowIfDisposed();
            RequireFeatures();
            ValidatePoint(sourcePoint, nameof(sourcePoint));
            NativeException.ThrowIfError(NativeMethods.PhotoIntelligentScissorsBuildMap(
                NativeHandle,
                sourcePoint.X,
                sourcePoint.Y));
            mapBuilt = true;
        }

        /// <summary>Writes the optimal contour into a caller-owned CV_32SC2 Mat.</summary>
        public void GetContour(Point targetPoint, Mat contour, bool backward = false)
        {
            ThrowIfDisposed();
            RequireMap();
            ValidatePoint(targetPoint, nameof(targetPoint));
            if (contour == null)
            {
                throw new ArgumentNullException(nameof(contour));
            }

            NativeException.ThrowIfError(NativeMethods.PhotoIntelligentScissorsGetContour(
                NativeHandle,
                targetPoint.X,
                targetPoint.Y,
                contour.NativeHandle,
                backward ? 1 : 0));
        }

        /// <summary>Returns a new caller-owned CV_32SC2 Mat containing the optimal contour.</summary>
        public Mat GetContour(Point targetPoint, bool backward = false)
        {
            var result = new Mat();
            try
            {
                GetContour(targetPoint, result, backward);
                return result;
            }
            catch
            {
                result.Dispose();
                throw;
            }
        }

        /// <summary>Releases the native model.</summary>
        public void Dispose()
        {
            if (!disposed)
            {
                handle.Dispose();
                disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        private static bool ValidateImage(Mat? value, string parameterName, bool optional, out Size size)
        {
            size = default;
            if (value == null)
            {
                if (optional) return false;
                throw new ArgumentNullException(parameterName);
            }
            if (value.Empty)
            {
                if (optional) return false;
                throw new ArgumentException("Image must not be empty.", parameterName);
            }
            if (value.Dims != 2 ||
                (value.Type != MatType.CV_8UC1 && value.Type != MatType.CV_8UC3 && value.Type != MatType.CV_8UC4))
            {
                throw new ArgumentException("Image must be a two-dimensional CV_8UC1, CV_8UC3, or CV_8UC4 Mat.", parameterName);
            }
            size = value.Size;
            return true;
        }

        private static bool ValidateOptionalFeature(Mat? value, string parameterName, int expectedType, ref Size size)
        {
            if (value == null || value.Empty) return false;
            if (value.Dims != 2 || value.Type != expectedType)
            {
                throw new ArgumentException("Custom feature has an invalid Mat type or dimensionality.", parameterName);
            }
            MergeSize(ref size, value.Size, present: true, parameterName);
            return true;
        }

        private static void MergeSize(ref Size current, Size value, bool present, string parameterName)
        {
            if (!present) return;
            if (current.Width > 0 && current.Height > 0 &&
                (current.Width != value.Width || current.Height != value.Height))
            {
                throw new ArgumentException("All custom features and the optional image must have the same size.", parameterName);
            }
            current = value;
        }

        private static IntPtr OptionalNativeHandle(Mat? value, bool present)
        {
            return present && value != null ? value.NativeHandle : IntPtr.Zero;
        }

        private static void ValidateNonNegativeFinite(double value, string parameterName)
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || value < 0.0)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Value must be finite and non-negative.");
            }
        }

        private void SetAppliedSize(Size size)
        {
            width = size.Width;
            height = size.Height;
            featuresApplied = true;
            mapBuilt = false;
        }

        private void InvalidateFeatures()
        {
            width = 0;
            height = 0;
            featuresApplied = false;
            mapBuilt = false;
        }

        private void RequireFeatures()
        {
            if (!featuresApplied)
            {
                throw new InvalidOperationException("Call ApplyImage or ApplyImageFeatures before BuildMap.");
            }
        }

        private void RequireMap()
        {
            if (!mapBuilt)
            {
                throw new InvalidOperationException("Call BuildMap before GetContour.");
            }
        }

        private void ValidatePoint(Point point, string parameterName)
        {
            if (point.X < 0 || point.Y < 0 || point.X >= width || point.Y >= height)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Point must lie inside the applied image.");
            }
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(IntelligentScissorsMB));
            }
        }
    }
}

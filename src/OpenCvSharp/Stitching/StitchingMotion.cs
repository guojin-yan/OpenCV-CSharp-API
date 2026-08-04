using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Stitching
{
    /// <summary>Camera calibration, wave correction, and match-graph utilities for stitching.</summary>
    public static class StitchingMotion
    {
        /// <summary>Estimates horizontal and vertical focal lengths from an exact 3 x 3 CV_64FC1 homography.</summary>
        public static void FocalsFromHomography(
            Mat homography,
            out double focalX,
            out double focalY,
            out bool focalXEstimated,
            out bool focalYEstimated)
        {
            ValidateHomography(homography, nameof(homography));
            NativeException.ThrowIfError(NativeMethods.StitchingFocalsFromHomography(
                homography.NativeHandle,
                out focalX,
                out focalY,
                out int nativeFocalXEstimated,
                out int nativeFocalYEstimated));
            focalXEstimated = nativeFocalXEstimated != 0;
            focalYEstimated = nativeFocalYEstimated != 0;
            GC.KeepAlive(homography);
        }

        /// <summary>Attempts rotating-camera calibration into caller-owned 3 x 3 CV_64FC1 storage.</summary>
        public static bool CalibrateRotatingCamera(Mat[] homographies, Mat cameraMatrix)
        {
            if (cameraMatrix == null) throw new ArgumentNullException(nameof(cameraMatrix));
            IntPtr[] handles = GetHomographyHandles(homographies);
            NativeException.ThrowIfError(NativeMethods.StitchingCalibrateRotatingCamera(
                handles, handles.Length, cameraMatrix.NativeHandle, out int calibrated));
            GC.KeepAlive(homographies);
            GC.KeepAlive(cameraMatrix);
            return calibrated != 0;
        }

        /// <summary>Attempts rotating-camera calibration and returns independently owned camera storage.</summary>
        public static bool TryCalibrateRotatingCamera(Mat[] homographies, out Mat cameraMatrix)
        {
            cameraMatrix = new Mat();
            try
            {
                return CalibrateRotatingCamera(homographies, cameraMatrix);
            }
            catch
            {
                cameraMatrix.Dispose();
                throw;
            }
        }

        /// <summary>Corrects exact 3 x 3 CV_32FC1 rotation matrices in place.</summary>
        public static void WaveCorrect(Mat[] rotationMatrices, WaveCorrectKind correctionKind)
        {
            if (rotationMatrices == null) throw new ArgumentNullException(nameof(rotationMatrices));
            if (correctionKind < WaveCorrectKind.Horizontal || correctionKind > WaveCorrectKind.Auto)
            {
                throw new ArgumentOutOfRangeException(nameof(correctionKind));
            }
            var handles = new IntPtr[rotationMatrices.Length];
            for (int i = 0; i < rotationMatrices.Length; ++i)
            {
                Mat value = rotationMatrices[i] ?? throw new ArgumentNullException(
                    nameof(rotationMatrices), "The rotation matrix collection contains null.");
                if (value.Empty || value.Dims != 2 || value.Rows != 3 || value.Cols != 3 ||
                    value.Type != MatType.CV_32FC1)
                {
                    throw new ArgumentException(
                        "Every rotation matrix must be an exact 3 x 3 CV_32FC1 matrix.",
                        nameof(rotationMatrices));
                }
                for (int j = 0; j < i; ++j)
                {
                    if (ReferenceEquals(value, rotationMatrices[j]))
                    {
                        throw new ArgumentException("Rotation matrix objects must be distinct.", nameof(rotationMatrices));
                    }
                }
                handles[i] = value.NativeHandle;
            }
            NativeException.ThrowIfError(NativeMethods.StitchingWaveCorrect(
                handles, handles.Length, (int)correctionKind));
            GC.KeepAlive(rotationMatrices);
        }

        /// <summary>Returns an independently owned UTF-8-decoded DOT representation of the match graph.</summary>
        public static string MatchesGraphAsString(
            string[] paths,
            MatchesInfo[] pairwiseMatches,
            float confidenceThreshold)
        {
            if (paths == null) throw new ArgumentNullException(nameof(paths));
            if (paths.Length == 0) throw new ArgumentException("At least one path is required.", nameof(paths));
            if (float.IsNaN(confidenceThreshold) || float.IsInfinity(confidenceThreshold))
            {
                throw new ArgumentOutOfRangeException(nameof(confidenceThreshold), "Confidence threshold must be finite.");
            }
            IntPtr[] matchHandles = StitchingMotionMarshal.GetMatchHandles(
                pairwiseMatches, paths.Length, nameof(pairwiseMatches));
            PackPaths(paths, out byte[] buffer, out int[] offsets);
            NativeException.ThrowIfError(NativeMethods.StitchingMatchesGraphAsString(
                buffer,
                buffer.Length,
                offsets,
                paths.Length,
                offsets.Length,
                matchHandles,
                matchHandles.Length,
                confidenceThreshold,
                out IntPtr result));
            GC.KeepAlive(pairwiseMatches);
            return CorePersistenceMarshal.ReadUtf8Result(result);
        }

        /// <summary>
        /// Copies the largest connected component into independently owned feature and row-major match arrays.
        /// </summary>
        public static int[] LeaveBiggestComponent(
            ImageFeatures[] features,
            MatchesInfo[] pairwiseMatches,
            float confidenceThreshold,
            out ImageFeatures[] componentFeatures,
            out MatchesInfo[] componentMatches)
        {
            componentFeatures = Array.Empty<ImageFeatures>();
            componentMatches = Array.Empty<MatchesInfo>();
            if (float.IsNaN(confidenceThreshold) || float.IsInfinity(confidenceThreshold))
            {
                throw new ArgumentOutOfRangeException(nameof(confidenceThreshold), "Confidence threshold must be finite.");
            }
            IntPtr[] featureHandles = StitchingMotionMarshal.GetFeatureHandles(features, nameof(features));
            IntPtr[] matchHandles = StitchingMotionMarshal.GetMatchHandles(
                pairwiseMatches, featureHandles.Length, nameof(pairwiseMatches));
            int maximumMatchCount = checked(featureHandles.Length * featureHandles.Length);
            ImageFeatures[] allFeatures = CreateFeatureOutputs(featureHandles.Length);
            MatchesInfo[] allMatches = CreateMatchOutputs(maximumMatchCount);
            try
            {
                IntPtr[] outputFeatureHandles = GetFeatureOutputHandles(allFeatures);
                IntPtr[] outputMatchHandles = GetMatchOutputHandles(allMatches);
                var indices = new int[featureHandles.Length];
                NativeException.ThrowIfError(NativeMethods.StitchingLeaveBiggestComponent(
                    featureHandles,
                    featureHandles.Length,
                    matchHandles,
                    matchHandles.Length,
                    confidenceThreshold,
                    outputFeatureHandles,
                    outputFeatureHandles.Length,
                    outputMatchHandles,
                    outputMatchHandles.Length,
                    indices,
                    indices.Length,
                    out int selectedCount));
                if (selectedCount <= 0 || selectedCount > featureHandles.Length)
                {
                    throw new OpenCvException("Native largest-component count is outside the valid range.");
                }
                int selectedMatchCount = checked(selectedCount * selectedCount);
                componentFeatures = new ImageFeatures[selectedCount];
                componentMatches = new MatchesInfo[selectedMatchCount];
                Array.Copy(allFeatures, componentFeatures, selectedCount);
                Array.Copy(allMatches, componentMatches, selectedMatchCount);
                for (int i = 0; i < selectedCount; ++i) allFeatures[i] = null!;
                for (int i = 0; i < selectedMatchCount; ++i) allMatches[i] = null!;
                var selectedIndices = new int[selectedCount];
                Array.Copy(indices, selectedIndices, selectedCount);
                GC.KeepAlive(features);
                GC.KeepAlive(pairwiseMatches);
                return selectedIndices;
            }
            finally
            {
                DisposeFeatures(allFeatures);
                DisposeMatches(allMatches);
            }
        }

        private static IntPtr[] GetHomographyHandles(Mat[] homographies)
        {
            if (homographies == null) throw new ArgumentNullException(nameof(homographies));
            if (homographies.Length == 0)
            {
                throw new ArgumentException("At least one homography is required.", nameof(homographies));
            }
            var handles = new IntPtr[homographies.Length];
            for (int i = 0; i < homographies.Length; ++i)
            {
                ValidateHomography(homographies[i], nameof(homographies));
                handles[i] = homographies[i].NativeHandle;
            }
            return handles;
        }

        private static void ValidateHomography(Mat homography, string parameterName)
        {
            if (homography == null) throw new ArgumentNullException(parameterName);
            if (homography.Empty || homography.Dims != 2 || homography.Rows != 3 || homography.Cols != 3 ||
                homography.Type != MatType.CV_64FC1)
            {
                throw new ArgumentException("A homography must be an exact 3 x 3 CV_64FC1 matrix.", parameterName);
            }
        }

        private static void PackPaths(string[] paths, out byte[] buffer, out int[] offsets)
        {
            offsets = new int[checked(paths.Length + 1)];
            var encoded = new byte[paths.Length][];
            int total = 0;
            for (int i = 0; i < paths.Length; ++i)
            {
                encoded[i] = CorePersistenceMarshal.Encode(paths[i], nameof(paths), true);
                total = checked(total + encoded[i].Length);
                offsets[i + 1] = total;
            }
            buffer = new byte[total];
            for (int i = 0; i < encoded.Length; ++i)
            {
                Buffer.BlockCopy(encoded[i], 0, buffer, offsets[i], encoded[i].Length);
            }
        }

        private static ImageFeatures[] CreateFeatureOutputs(int count)
        {
            var values = new ImageFeatures[count];
            int created = 0;
            try
            {
                for (; created < count; ++created) values[created] = ImageFeatures.CreateEmpty();
                return values;
            }
            catch
            {
                for (int i = 0; i < created; ++i) values[i]?.Dispose();
                throw;
            }
        }

        private static MatchesInfo[] CreateMatchOutputs(int count)
        {
            var values = new MatchesInfo[count];
            int created = 0;
            try
            {
                for (; created < count; ++created) values[created] = new MatchesInfo();
                return values;
            }
            catch
            {
                for (int i = 0; i < created; ++i) values[i]?.Dispose();
                throw;
            }
        }

        private static IntPtr[] GetFeatureOutputHandles(ImageFeatures[] values)
        {
            var handles = new IntPtr[values.Length];
            for (int i = 0; i < values.Length; ++i) handles[i] = values[i].NativeHandle;
            return handles;
        }

        private static IntPtr[] GetMatchOutputHandles(MatchesInfo[] values)
        {
            var handles = new IntPtr[values.Length];
            for (int i = 0; i < values.Length; ++i) handles[i] = values[i].NativeHandle;
            return handles;
        }

        private static void DisposeFeatures(ImageFeatures[] values)
        {
            for (int i = 0; i < values.Length; ++i) values[i]?.Dispose();
        }

        private static void DisposeMatches(MatchesInfo[] values)
        {
            for (int i = 0; i < values.Length; ++i) values[i]?.Dispose();
        }
    }

    internal static class StitchingMotionMarshal
    {
        internal static IntPtr[] GetFeatureHandles(ImageFeatures[] features, string parameterName)
        {
            if (features == null) throw new ArgumentNullException(parameterName);
            if (features.Length == 0) throw new ArgumentException("At least one feature record is required.", parameterName);
            var handles = new IntPtr[features.Length];
            for (int i = 0; i < features.Length; ++i)
            {
                ImageFeatures feature = features[i] ?? throw new ArgumentNullException(
                    parameterName, "The feature collection contains null.");
                if (feature.IsDisposed) throw new ObjectDisposedException(feature.GetType().FullName);
                for (int j = 0; j < i; ++j)
                {
                    if (ReferenceEquals(feature, features[j]))
                    {
                        throw new ArgumentException("Feature record objects must be distinct.", parameterName);
                    }
                }
                handles[i] = feature.NativeHandle;
            }
            return handles;
        }

        internal static IntPtr[] GetMatchHandles(MatchesInfo[] matches, int featureCount, string parameterName)
        {
            if (matches == null) throw new ArgumentNullException(parameterName);
            int required = checked(featureCount * featureCount);
            if (matches.Length != required)
            {
                throw new ArgumentException("Pairwise matches must contain exactly N squared row-major records.", parameterName);
            }
            var handles = new IntPtr[matches.Length];
            for (int i = 0; i < matches.Length; ++i)
            {
                MatchesInfo match = matches[i] ?? throw new ArgumentNullException(
                    parameterName, "The match collection contains null.");
                if (match.IsDisposed) throw new ObjectDisposedException(match.GetType().FullName);
                for (int j = 0; j < i; ++j)
                {
                    if (ReferenceEquals(match, matches[j]))
                    {
                        throw new ArgumentException("Pairwise match objects must be distinct.", parameterName);
                    }
                }
                handles[i] = match.NativeHandle;
            }
            return handles;
        }

        internal static NativeMethods.StitchingCameraParamsNative[] GetCameraValues(
            StitcherCameraParams[]? cameras,
            int featureCount,
            bool required)
        {
            if (cameras == null)
            {
                if (required)
                {
                    throw new ArgumentNullException(nameof(cameras), "This estimator requires initial camera values.");
                }
                return Array.Empty<NativeMethods.StitchingCameraParamsNative>();
            }
            if (cameras.Length != featureCount)
            {
                throw new ArgumentException("Initial camera count must match feature count.", nameof(cameras));
            }
            var result = new NativeMethods.StitchingCameraParamsNative[cameras.Length];
            for (int i = 0; i < cameras.Length; ++i)
            {
                StitcherCameraParams camera = cameras[i] ?? throw new ArgumentNullException(
                    nameof(cameras), "The camera collection contains null.");
                ValidateCamera(camera, nameof(cameras));
                result[i] = new NativeMethods.StitchingCameraParamsNative
                {
                    Focal = camera.Focal,
                    Aspect = camera.Aspect,
                    Ppx = camera.PrincipalPointX,
                    Ppy = camera.PrincipalPointY,
                    R = camera.Rotation.NativeHandle,
                    T = camera.Translation.NativeHandle
                };
            }
            return result;
        }

        internal static StitcherCameraParams[] TakeCameras(
            NativeMethods.StitchingCameraParamsNative[] nativeValues)
        {
            var rotations = new Mat[nativeValues.Length];
            var translations = new Mat[nativeValues.Length];
            try
            {
                for (int i = 0; i < nativeValues.Length; ++i)
                {
                    if (nativeValues[i].R == IntPtr.Zero || nativeValues[i].T == IntPtr.Zero)
                    {
                        throw new OpenCvException("Native camera output contains a null matrix handle.");
                    }
                    rotations[i] = TakeMat(ref nativeValues[i].R);
                    translations[i] = TakeMat(ref nativeValues[i].T);
                }
                var result = new StitcherCameraParams[nativeValues.Length];
                for (int i = 0; i < result.Length; ++i)
                {
                    result[i] = new StitcherCameraParams(
                        nativeValues[i].Focal,
                        nativeValues[i].Aspect,
                        nativeValues[i].Ppx,
                        nativeValues[i].Ppy,
                        rotations[i],
                        translations[i]);
                }
                return result;
            }
            catch
            {
                for (int i = 0; i < rotations.Length; ++i) rotations[i]?.Dispose();
                for (int i = 0; i < translations.Length; ++i) translations[i]?.Dispose();
                throw;
            }
            finally
            {
                for (int i = 0; i < nativeValues.Length; ++i)
                {
                    ReleaseMat(ref nativeValues[i].R);
                    ReleaseMat(ref nativeValues[i].T);
                }
            }
        }

        private static void ValidateCamera(StitcherCameraParams camera, string parameterName)
        {
            if (double.IsNaN(camera.Focal) || double.IsInfinity(camera.Focal) ||
                double.IsNaN(camera.Aspect) || double.IsInfinity(camera.Aspect) ||
                double.IsNaN(camera.PrincipalPointX) || double.IsInfinity(camera.PrincipalPointX) ||
                double.IsNaN(camera.PrincipalPointY) || double.IsInfinity(camera.PrincipalPointY))
            {
                throw new ArgumentException("Camera scalar values must be finite.", parameterName);
            }
            Mat rotation = camera.Rotation;
            Mat translation = camera.Translation;
            if (rotation == null || translation == null || rotation.IsDisposed || translation.IsDisposed ||
                rotation.Empty || rotation.Dims != 2 || rotation.Rows != 3 || rotation.Cols != 3 ||
                rotation.Channels != 1 || (rotation.Depth != MatType.CV_32F && rotation.Depth != MatType.CV_64F) ||
                translation.Empty || translation.Dims != 2 || translation.Rows != 3 || translation.Cols != 1 ||
                translation.Channels != 1 || (translation.Depth != MatType.CV_32F && translation.Depth != MatType.CV_64F))
            {
                throw new ArgumentException(
                    "Camera rotation must be 3 x 3 and translation 3 x 1, both single-channel CV_32F or CV_64F.",
                    parameterName);
            }
        }

        private static Mat TakeMat(ref IntPtr value)
        {
            IntPtr native = value;
            value = IntPtr.Zero;
            return new Mat(native);
        }

        private static void ReleaseMat(ref IntPtr value)
        {
            if (value == IntPtr.Zero) return;
            var mat = new Mat(value);
            value = IntPtr.Zero;
            mat.Dispose();
        }
    }
}

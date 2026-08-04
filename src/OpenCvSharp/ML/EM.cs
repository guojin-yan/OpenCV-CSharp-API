using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.ML
{
    /// <summary>Expectation-maximization model for Gaussian mixtures.</summary>
    public sealed class EM : StatModel
    {
        private const int IntClustersNumber = 0;
        private const int IntCovarianceMatrixType = 1;

        /// <summary>The OpenCV default number of mixture components.</summary>
        public const int DefaultClustersNumber = 5;

        /// <summary>The OpenCV default maximum number of EM iterations.</summary>
        public const int DefaultMaxIterations = 100;

        private EM(IntPtr nativeHandle)
            : base(nativeHandle)
        {
        }

        /// <summary>Gets or sets the number of Gaussian mixture components.</summary>
        public int ClustersNumber
        {
            get { return GetInt(IntClustersNumber); }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                SetInt(IntClustersNumber, value);
            }
        }

        /// <summary>Gets or sets the covariance-matrix constraint.</summary>
        public EMCovarianceMatrixTypes CovarianceMatrixType
        {
            get { return (EMCovarianceMatrixTypes)GetInt(IntCovarianceMatrixType); }
            set
            {
                ValidateCovarianceMatrixType(value, nameof(value));
                SetInt(IntCovarianceMatrixType, (int)value);
            }
        }

        /// <summary>Gets or sets the EM termination criteria.</summary>
        public TermCriteria TermCriteria
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.MlEMGetTermCriteria(NativeHandle, out int type, out int maxCount, out double epsilon));
                return new TermCriteria((TermCriteriaTypes)type, maxCount, epsilon);
            }

            set
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.MlEMSetTermCriteria(NativeHandle, (int)value.Type, value.MaxCount, value.Epsilon));
            }
        }

        /// <summary>Creates an empty expectation-maximization model.</summary>
        public static EM Create()
        {
            NativeException.ThrowIfError(NativeMethods.MlEMCreate(out IntPtr nativeHandle));
            return new EM(nativeHandle);
        }

        /// <summary>Loads a serialized expectation-maximization model.</summary>
        public static EM Load(string filepath, string? nodeName = null)
        {
            byte[] nativePath = MLStringConvert.ToNullTerminatedUtf8(filepath, nameof(filepath));
            byte[] nativeNodeName = MLStringConvert.ToNullTerminatedUtf8(nodeName, nameof(nodeName), allowNull: true);
            NativeException.ThrowIfError(NativeMethods.MlEMLoad(nativePath, nativeNodeName, out IntPtr nativeHandle));
            return new EM(nativeHandle);
        }

        /// <summary>Returns an independent copy of the mixture weights.</summary>
        public Mat GetWeights()
        {
            var dst = new Mat();
            try
            {
                GetWeights(dst);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>Copies the mixture weights into a caller-owned matrix.</summary>
        public void GetWeights(Mat dst)
        {
            ThrowIfDisposed();
            TrainData.ValidateNotNull(dst, nameof(dst));
            NativeException.ThrowIfError(NativeMethods.MlEMGetWeights(NativeHandle, dst.NativeHandle));
        }

        /// <summary>Returns an independent copy of the mixture means.</summary>
        public Mat GetMeans()
        {
            var dst = new Mat();
            try
            {
                GetMeans(dst);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>Copies the mixture means into a caller-owned matrix.</summary>
        public void GetMeans(Mat dst)
        {
            ThrowIfDisposed();
            TrainData.ValidateNotNull(dst, nameof(dst));
            NativeException.ThrowIfError(NativeMethods.MlEMGetMeans(NativeHandle, dst.NativeHandle));
        }

        /// <summary>Returns independent copies of the covariance matrices.</summary>
        public Mat[] GetCovariances()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MlEMGetCovariancesCount(NativeHandle, out int count));
            if (count < 0)
            {
                throw new OpenCvException("Native EM covariance count is negative.");
            }
            if (count == 0)
            {
                return Array.Empty<Mat>();
            }

            var result = new Mat[count];
            var handles = new IntPtr[count];
            try
            {
                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = new Mat();
                    handles[i] = result[i].NativeHandle;
                }

                NativeException.ThrowIfError(NativeMethods.MlEMGetCovariancesFill(NativeHandle, handles, handles.Length, out int written));
                if (written != count)
                {
                    throw new OpenCvException("Native EM covariance count changed during retrieval.");
                }

                return result;
            }
            catch
            {
                DisposeMats(result);
                throw;
            }
        }

        /// <summary>Returns the sample log-likelihood and most-probable mixture component.</summary>
        public EMPredictionResult Predict2(Mat sample, Mat? probabilities = null)
        {
            ThrowIfDisposed();
            TrainData.ValidateNotNull(sample, nameof(sample));
            NativeException.ThrowIfError(NativeMethods.MlEMPredict2(
                NativeHandle,
                sample.NativeHandle,
                probabilities?.NativeHandle ?? IntPtr.Zero,
                out double logLikelihood,
                out int label));
            return new EMPredictionResult(logLikelihood, label);
        }

        /// <summary>Trains the model with automatically initialized parameters.</summary>
        public bool TrainEM(Mat samples, Mat? logLikelihoods = null, Mat? labels = null, Mat? probabilities = null)
        {
            ThrowIfDisposed();
            TrainData.ValidateNotNull(samples, nameof(samples));
            NativeException.ThrowIfError(NativeMethods.MlEMTrainEM(
                NativeHandle,
                samples.NativeHandle,
                logLikelihoods?.NativeHandle ?? IntPtr.Zero,
                labels?.NativeHandle ?? IntPtr.Zero,
                probabilities?.NativeHandle ?? IntPtr.Zero,
                out int result));
            return result != 0;
        }

        /// <summary>Trains the model from initial means and optional covariance and weight estimates.</summary>
        public bool TrainE(
            Mat samples,
            Mat initialMeans,
            Mat[]? initialCovariances = null,
            Mat? initialWeights = null,
            Mat? logLikelihoods = null,
            Mat? labels = null,
            Mat? probabilities = null)
        {
            ThrowIfDisposed();
            TrainData.ValidateNotNull(samples, nameof(samples));
            TrainData.ValidateNotNull(initialMeans, nameof(initialMeans));
            IntPtr[] covarianceHandles = ToNativeHandles(initialCovariances, nameof(initialCovariances));
            NativeException.ThrowIfError(NativeMethods.MlEMTrainE(
                NativeHandle,
                samples.NativeHandle,
                initialMeans.NativeHandle,
                covarianceHandles,
                covarianceHandles.Length,
                initialWeights?.NativeHandle ?? IntPtr.Zero,
                logLikelihoods?.NativeHandle ?? IntPtr.Zero,
                labels?.NativeHandle ?? IntPtr.Zero,
                probabilities?.NativeHandle ?? IntPtr.Zero,
                out int result));
            return result != 0;
        }

        /// <summary>Trains the model from initial posterior probabilities.</summary>
        public bool TrainM(
            Mat samples,
            Mat initialProbabilities,
            Mat? logLikelihoods = null,
            Mat? labels = null,
            Mat? probabilities = null)
        {
            ThrowIfDisposed();
            TrainData.ValidateNotNull(samples, nameof(samples));
            TrainData.ValidateNotNull(initialProbabilities, nameof(initialProbabilities));
            NativeException.ThrowIfError(NativeMethods.MlEMTrainM(
                NativeHandle,
                samples.NativeHandle,
                initialProbabilities.NativeHandle,
                logLikelihoods?.NativeHandle ?? IntPtr.Zero,
                labels?.NativeHandle ?? IntPtr.Zero,
                probabilities?.NativeHandle ?? IntPtr.Zero,
                out int result));
            return result != 0;
        }

        private int GetInt(int propertyId)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MlEMGetInt(NativeHandle, propertyId, out int value));
            return value;
        }

        private void SetInt(int propertyId, int value)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MlEMSetInt(NativeHandle, propertyId, value));
        }

        private static void ValidateCovarianceMatrixType(EMCovarianceMatrixTypes value, string parameterName)
        {
            if (value != EMCovarianceMatrixTypes.Spherical &&
                value != EMCovarianceMatrixTypes.Diagonal &&
                value != EMCovarianceMatrixTypes.Generic)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }

        private static IntPtr[] ToNativeHandles(Mat[]? values, string parameterName)
        {
            if (values == null || values.Length == 0)
            {
                return Array.Empty<IntPtr>();
            }

            var handles = new IntPtr[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == null)
                {
                    throw new ArgumentException("The matrix collection contains a null item.", parameterName);
                }

                handles[i] = values[i].NativeHandle;
            }

            return handles;
        }

        private static void DisposeMats(Mat[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                values[i]?.Dispose();
            }
        }
    }
}

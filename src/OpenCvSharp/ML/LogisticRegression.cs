using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.ML
{
    /// <summary>Logistic-regression classifier.</summary>
    public sealed class LogisticRegression : StatModel
    {
        private const int IntIterations = 0;
        private const int IntRegularization = 1;
        private const int IntTrainingMethod = 2;
        private const int IntMiniBatchSize = 3;

        private LogisticRegression(IntPtr nativeHandle)
            : base(nativeHandle)
        {
        }

        /// <summary>Gets or sets the gradient-descent learning rate.</summary>
        public double LearningRate
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.MlLogisticRegressionGetLearningRate(NativeHandle, out double value));
                return value;
            }
            set
            {
                if (value <= 0.0 || double.IsNaN(value) || double.IsInfinity(value))
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.MlLogisticRegressionSetLearningRate(NativeHandle, value));
            }
        }

        /// <summary>Gets or sets the configured iteration count.</summary>
        public int Iterations
        {
            get { return GetInt(IntIterations); }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                SetInt(IntIterations, value);
            }
        }

        /// <summary>Gets or sets the regularization kind.</summary>
        public LogisticRegressionRegularizationKinds Regularization
        {
            get { return (LogisticRegressionRegularizationKinds)GetInt(IntRegularization); }
            set
            {
                ValidateRegularization(value, nameof(value));
                SetInt(IntRegularization, (int)value);
            }
        }

        /// <summary>Gets or sets the optimization method.</summary>
        public LogisticRegressionTrainingMethods TrainingMethod
        {
            get { return (LogisticRegressionTrainingMethods)GetInt(IntTrainingMethod); }
            set
            {
                ValidateTrainingMethod(value, nameof(value));
                SetInt(IntTrainingMethod, (int)value);
            }
        }

        /// <summary>Gets or sets the sample count used by each mini-batch step.</summary>
        public int MiniBatchSize
        {
            get { return GetInt(IntMiniBatchSize); }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                SetInt(IntMiniBatchSize, value);
            }
        }

        /// <summary>Gets or sets the optimization termination criteria.</summary>
        public TermCriteria TermCriteria
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.MlLogisticRegressionGetTermCriteria(NativeHandle, out int type, out int maxCount, out double epsilon));
                return new TermCriteria((TermCriteriaTypes)type, maxCount, epsilon);
            }
            set
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.MlLogisticRegressionSetTermCriteria(NativeHandle, (int)value.Type, value.MaxCount, value.Epsilon));
            }
        }

        /// <summary>Creates an empty logistic-regression classifier.</summary>
        public static LogisticRegression Create()
        {
            NativeException.ThrowIfError(NativeMethods.MlLogisticRegressionCreate(out IntPtr nativeHandle));
            return new LogisticRegression(nativeHandle);
        }

        /// <summary>Loads a serialized logistic-regression classifier.</summary>
        public static LogisticRegression Load(string filepath, string? nodeName = null)
        {
            byte[] nativePath = MLStringConvert.ToNullTerminatedUtf8(filepath, nameof(filepath));
            byte[] nativeNodeName = MLStringConvert.ToNullTerminatedUtf8(nodeName, nameof(nodeName), allowNull: true);
            NativeException.ThrowIfError(NativeMethods.MlLogisticRegressionLoad(nativePath, nativeNodeName, out IntPtr nativeHandle));
            return new LogisticRegression(nativeHandle);
        }

        /// <summary>Returns an independent copy of the trained parameter matrix.</summary>
        public Mat GetLearntThetas()
        {
            var dst = new Mat();
            try
            {
                GetLearntThetas(dst);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>Copies the trained parameter matrix into a caller-owned matrix.</summary>
        public void GetLearntThetas(Mat dst)
        {
            ThrowIfDisposed();
            TrainData.ValidateNotNull(dst, nameof(dst));
            NativeException.ThrowIfError(NativeMethods.MlLogisticRegressionGetLearntThetas(NativeHandle, dst.NativeHandle));
        }

        private int GetInt(int propertyId)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MlLogisticRegressionGetInt(NativeHandle, propertyId, out int value));
            return value;
        }

        private void SetInt(int propertyId, int value)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MlLogisticRegressionSetInt(NativeHandle, propertyId, value));
        }

        private static void ValidateRegularization(LogisticRegressionRegularizationKinds value, string parameterName)
        {
            if (value != LogisticRegressionRegularizationKinds.Disable &&
                value != LogisticRegressionRegularizationKinds.L1 &&
                value != LogisticRegressionRegularizationKinds.L2)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }

        private static void ValidateTrainingMethod(LogisticRegressionTrainingMethods value, string parameterName)
        {
            if (value != LogisticRegressionTrainingMethods.Batch && value != LogisticRegressionTrainingMethods.MiniBatch)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }
    }
}

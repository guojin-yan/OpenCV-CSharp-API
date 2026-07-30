using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.ML
{
    /// <summary>Linear SVM classifier trained with stochastic gradient descent.</summary>
    public sealed class SVMSGD : StatModel
    {
        private const int IntType = 0;
        private const int IntMarginType = 1;

        private const int FloatMarginRegularization = 0;
        private const int FloatInitialStepSize = 1;
        private const int FloatStepDecreasingPower = 2;

        private SVMSGD(IntPtr nativeHandle)
            : base(nativeHandle)
        {
        }

        /// <summary>Gets or sets the stochastic-gradient variant.</summary>
        public SVMSGDTypes Type
        {
            get { return (SVMSGDTypes)GetInt(IntType); }
            set
            {
                ValidateType(value, nameof(value));
                SetInt(IntType, (int)value);
            }
        }

        /// <summary>Gets or sets the margin constraint.</summary>
        public SVMSGDMarginTypes MarginType
        {
            get { return (SVMSGDMarginTypes)GetInt(IntMarginType); }
            set
            {
                ValidateMarginType(value, nameof(value));
                SetInt(IntMarginType, (int)value);
            }
        }

        /// <summary>Gets or sets the positive margin-regularization parameter.</summary>
        public float MarginRegularization
        {
            get { return GetFloat(FloatMarginRegularization); }
            set
            {
                ValidatePositive(value, nameof(value), allowZero: false);
                SetFloat(FloatMarginRegularization, value);
            }
        }

        /// <summary>Gets or sets the positive initial step size.</summary>
        public float InitialStepSize
        {
            get { return GetFloat(FloatInitialStepSize); }
            set
            {
                ValidatePositive(value, nameof(value), allowZero: false);
                SetFloat(FloatInitialStepSize, value);
            }
        }

        /// <summary>Gets or sets the non-negative step-decreasing power.</summary>
        public float StepDecreasingPower
        {
            get { return GetFloat(FloatStepDecreasingPower); }
            set
            {
                ValidatePositive(value, nameof(value), allowZero: true);
                SetFloat(FloatStepDecreasingPower, value);
            }
        }

        /// <summary>Gets or sets the training termination criteria.</summary>
        public TermCriteria TermCriteria
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.MlSVMSGDGetTermCriteria(NativeHandle, out int type, out int maxCount, out double epsilon));
                return new TermCriteria((TermCriteriaTypes)type, maxCount, epsilon);
            }
            set
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.MlSVMSGDSetTermCriteria(NativeHandle, (int)value.Type, value.MaxCount, value.Epsilon));
            }
        }

        /// <summary>Gets the decision-function shift.</summary>
        public float Shift
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.MlSVMSGDGetShift(NativeHandle, out float value));
                return value;
            }
        }

        /// <summary>Creates an empty stochastic-gradient SVM classifier.</summary>
        public static SVMSGD Create()
        {
            NativeException.ThrowIfError(NativeMethods.MlSVMSGDCreate(out IntPtr nativeHandle));
            return new SVMSGD(nativeHandle);
        }

        /// <summary>Loads a serialized stochastic-gradient SVM classifier.</summary>
        public static SVMSGD Load(string filepath, string? nodeName = null)
        {
            byte[] nativePath = MLStringConvert.ToNullTerminatedUtf8(filepath, nameof(filepath));
            byte[] nativeNodeName = MLStringConvert.ToNullTerminatedUtf8(nodeName, nameof(nodeName), allowNull: true);
            NativeException.ThrowIfError(NativeMethods.MlSVMSGDLoad(nativePath, nativeNodeName, out IntPtr nativeHandle));
            return new SVMSGD(nativeHandle);
        }

        /// <summary>Sets OpenCV's recommended parameters for the selected type and margin.</summary>
        public void SetOptimalParameters(SVMSGDTypes type = SVMSGDTypes.Asgd, SVMSGDMarginTypes marginType = SVMSGDMarginTypes.SoftMargin)
        {
            ValidateType(type, nameof(type));
            ValidateMarginType(marginType, nameof(marginType));
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MlSVMSGDSetOptimalParameters(NativeHandle, (int)type, (int)marginType));
        }

        /// <summary>Returns an independent copy of the decision-function weights.</summary>
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

        /// <summary>Copies the decision-function weights into a caller-owned matrix.</summary>
        public void GetWeights(Mat dst)
        {
            ThrowIfDisposed();
            TrainData.ValidateNotNull(dst, nameof(dst));
            NativeException.ThrowIfError(NativeMethods.MlSVMSGDGetWeights(NativeHandle, dst.NativeHandle));
        }

        private int GetInt(int propertyId)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MlSVMSGDGetInt(NativeHandle, propertyId, out int value));
            return value;
        }

        private void SetInt(int propertyId, int value)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MlSVMSGDSetInt(NativeHandle, propertyId, value));
        }

        private float GetFloat(int propertyId)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MlSVMSGDGetFloat(NativeHandle, propertyId, out float value));
            return value;
        }

        private void SetFloat(int propertyId, float value)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MlSVMSGDSetFloat(NativeHandle, propertyId, value));
        }

        private static void ValidateType(SVMSGDTypes value, string parameterName)
        {
            if (value != SVMSGDTypes.Sgd && value != SVMSGDTypes.Asgd)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }

        private static void ValidateMarginType(SVMSGDMarginTypes value, string parameterName)
        {
            if (value != SVMSGDMarginTypes.SoftMargin && value != SVMSGDMarginTypes.HardMargin)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }

        private static void ValidatePositive(float value, string parameterName, bool allowZero)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || (allowZero ? value < 0.0F : value <= 0.0F))
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }
    }
}

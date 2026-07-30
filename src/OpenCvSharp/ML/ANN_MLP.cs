using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.ML
{
    /// <summary>Artificial neural network multi-layer perceptron model.</summary>
    public sealed class ANN_MLP : StatModel
    {
        private const int IntTrainMethod = 0;
        private const int IntAnnealIterationsPerStep = 1;

        private const int DoubleBackpropWeightScale = 0;
        private const int DoubleBackpropMomentumScale = 1;
        private const int DoubleRpropDW0 = 2;
        private const int DoubleRpropDWPlus = 3;
        private const int DoubleRpropDWMinus = 4;
        private const int DoubleRpropDWMin = 5;
        private const int DoubleRpropDWMax = 6;
        private const int DoubleAnnealInitialT = 7;
        private const int DoubleAnnealFinalT = 8;
        private const int DoubleAnnealCoolingRatio = 9;

        private ANN_MLP(IntPtr nativeHandle)
            : base(nativeHandle)
        {
        }

        /// <summary>Gets the current training algorithm.</summary>
        public ANN_MLPTrainingMethods TrainingMethod
        {
            get { return (ANN_MLPTrainingMethods)GetInt(IntTrainMethod); }
        }

        /// <summary>Gets or sets the back-propagation weight-gradient scale.</summary>
        public double BackpropWeightScale
        {
            get { return GetDouble(DoubleBackpropWeightScale); }
            set { SetDouble(DoubleBackpropWeightScale, value); }
        }

        /// <summary>Gets or sets the back-propagation momentum scale.</summary>
        public double BackpropMomentumScale
        {
            get { return GetDouble(DoubleBackpropMomentumScale); }
            set { SetDouble(DoubleBackpropMomentumScale, value); }
        }

        /// <summary>Gets or sets the initial RPROP update value.</summary>
        public double RpropDW0
        {
            get { return GetDouble(DoubleRpropDW0); }
            set { SetDouble(DoubleRpropDW0, value); }
        }

        /// <summary>Gets or sets the RPROP increase factor.</summary>
        public double RpropDWPlus
        {
            get { return GetDouble(DoubleRpropDWPlus); }
            set { SetDouble(DoubleRpropDWPlus, value); }
        }

        /// <summary>Gets or sets the RPROP decrease factor.</summary>
        public double RpropDWMinus
        {
            get { return GetDouble(DoubleRpropDWMinus); }
            set { SetDouble(DoubleRpropDWMinus, value); }
        }

        /// <summary>Gets or sets the RPROP lower update limit.</summary>
        public double RpropDWMin
        {
            get { return GetDouble(DoubleRpropDWMin); }
            set { SetDouble(DoubleRpropDWMin, value); }
        }

        /// <summary>Gets or sets the RPROP upper update limit.</summary>
        public double RpropDWMax
        {
            get { return GetDouble(DoubleRpropDWMax); }
            set { SetDouble(DoubleRpropDWMax, value); }
        }

        /// <summary>Gets or sets the annealing initial temperature.</summary>
        public double AnnealInitialT
        {
            get { return GetDouble(DoubleAnnealInitialT); }
            set { SetDouble(DoubleAnnealInitialT, value); }
        }

        /// <summary>Gets or sets the annealing final temperature.</summary>
        public double AnnealFinalT
        {
            get { return GetDouble(DoubleAnnealFinalT); }
            set { SetDouble(DoubleAnnealFinalT, value); }
        }

        /// <summary>Gets or sets the annealing cooling ratio.</summary>
        public double AnnealCoolingRatio
        {
            get { return GetDouble(DoubleAnnealCoolingRatio); }
            set { SetDouble(DoubleAnnealCoolingRatio, value); }
        }

        /// <summary>Gets or sets the number of annealing iterations per temperature step.</summary>
        public int AnnealIterationsPerStep
        {
            get { return GetInt(IntAnnealIterationsPerStep); }
            set { SetInt(IntAnnealIterationsPerStep, value); }
        }

        /// <summary>Gets or sets the training termination criteria.</summary>
        public TermCriteria TermCriteria
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.MlAnnMlpGetTermCriteria(NativeHandle, out int type, out int maxCount, out double epsilon));
                return new TermCriteria((TermCriteriaTypes)type, maxCount, epsilon);
            }

            set
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.MlAnnMlpSetTermCriteria(NativeHandle, (int)value.Type, value.MaxCount, value.Epsilon));
            }
        }

        /// <summary>Creates an empty multi-layer perceptron model.</summary>
        public static ANN_MLP Create()
        {
            NativeException.ThrowIfError(NativeMethods.MlAnnMlpCreate(out IntPtr nativeHandle));
            return new ANN_MLP(nativeHandle);
        }

        /// <summary>Loads a serialized multi-layer perceptron model.</summary>
        public static ANN_MLP Load(string filepath)
        {
            byte[] nativePath = MLStringConvert.ToNullTerminatedUtf8(filepath, nameof(filepath));
            NativeException.ThrowIfError(NativeMethods.MlAnnMlpLoad(nativePath, out IntPtr nativeHandle));
            return new ANN_MLP(nativeHandle);
        }

        /// <summary>Sets the training algorithm and its common parameters.</summary>
        public void SetTrainMethod(ANN_MLPTrainingMethods method, double param1 = 0.0, double param2 = 0.0)
        {
            ValidateTrainingMethod(method);
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MlAnnMlpSetTrainMethod(NativeHandle, (int)method, param1, param2));
        }

        /// <summary>Sets the activation function and its parameters.</summary>
        public void SetActivationFunction(ANN_MLPActivationFunctions type, double param1 = 0.0, double param2 = 0.0)
        {
            ValidateActivationFunction(type);
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MlAnnMlpSetActivationFunction(NativeHandle, (int)type, param1, param2));
        }

        /// <summary>Sets the number of neurons in every layer.</summary>
        public void SetLayerSizes(Mat layerSizes)
        {
            ThrowIfDisposed();
            TrainData.ValidateNotNull(layerSizes, nameof(layerSizes));
            NativeException.ThrowIfError(NativeMethods.MlAnnMlpSetLayerSizes(NativeHandle, layerSizes.NativeHandle));
        }

        /// <summary>Returns the layer sizes in a new, independently owned matrix.</summary>
        public Mat GetLayerSizes()
        {
            var dst = new Mat();
            try
            {
                GetLayerSizes(dst);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>Copies the layer sizes into a caller-owned matrix.</summary>
        public void GetLayerSizes(Mat dst)
        {
            ThrowIfDisposed();
            TrainData.ValidateNotNull(dst, nameof(dst));
            NativeException.ThrowIfError(NativeMethods.MlAnnMlpGetLayerSizes(NativeHandle, dst.NativeHandle));
        }

        /// <summary>Returns weights for a layer in a new, independently owned matrix.</summary>
        public Mat GetWeights(int layerIndex)
        {
            var dst = new Mat();
            try
            {
                GetWeights(layerIndex, dst);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>Copies weights for a layer into a caller-owned matrix.</summary>
        public void GetWeights(int layerIndex, Mat dst)
        {
            ThrowIfDisposed();
            TrainData.ValidateNotNull(dst, nameof(dst));
            NativeException.ThrowIfError(NativeMethods.MlAnnMlpGetWeights(NativeHandle, layerIndex, dst.NativeHandle));
        }

        /// <summary>Sets the deterministic seed used by simulated annealing.</summary>
        public void SetAnnealEnergySeed(ulong seed)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MlAnnMlpSetAnnealEnergySeed(NativeHandle, seed));
        }

        /// <summary>Trains the model with ANN-specific training flags.</summary>
        public bool Train(TrainData trainData, ANN_MLPTrainFlags flags)
        {
            return base.Train(trainData, (StatModelFlags)flags);
        }

        private int GetInt(int propertyId)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MlAnnMlpGetInt(NativeHandle, propertyId, out int value));
            return value;
        }

        private void SetInt(int propertyId, int value)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MlAnnMlpSetInt(NativeHandle, propertyId, value));
        }

        private double GetDouble(int propertyId)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MlAnnMlpGetDouble(NativeHandle, propertyId, out double value));
            return value;
        }

        private void SetDouble(int propertyId, double value)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MlAnnMlpSetDouble(NativeHandle, propertyId, value));
        }

        private static void ValidateTrainingMethod(ANN_MLPTrainingMethods method)
        {
            if ((int)method < (int)ANN_MLPTrainingMethods.Backprop || (int)method > (int)ANN_MLPTrainingMethods.Anneal)
            {
                throw new ArgumentOutOfRangeException(nameof(method));
            }
        }

        private static void ValidateActivationFunction(ANN_MLPActivationFunctions type)
        {
            if ((int)type < (int)ANN_MLPActivationFunctions.Identity || (int)type > (int)ANN_MLPActivationFunctions.LeakyRelu)
            {
                throw new ArgumentOutOfRangeException(nameof(type));
            }
        }
    }
}

using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.ML
{
    /// <summary>Decision-tree model and shared base for OpenCV tree ensembles.</summary>
    public class DTrees : StatModel
    {
        private const int IntMaxCategories = 0;
        private const int IntMaxDepth = 1;
        private const int IntMinSampleCount = 2;
        private const int IntCvFolds = 3;
        private const int IntUseSurrogates = 4;
        private const int IntUse1SeRule = 5;
        private const int IntTruncatePrunedTree = 6;

        internal DTrees(IntPtr nativeHandle)
            : base(nativeHandle)
        {
        }

        /// <summary>Gets or sets the maximum number of clustered categories.</summary>
        public int MaxCategories
        {
            get { return GetInt(IntMaxCategories); }
            set { SetInt(IntMaxCategories, value); }
        }

        /// <summary>Gets or sets the maximum tree depth.</summary>
        public int MaxDepth
        {
            get { return GetInt(IntMaxDepth); }
            set { SetInt(IntMaxDepth, value); }
        }

        /// <summary>Gets or sets the minimum number of samples required to split a node.</summary>
        public int MinSampleCount
        {
            get { return GetInt(IntMinSampleCount); }
            set { SetInt(IntMinSampleCount, value); }
        }

        /// <summary>Gets or sets the number of cross-validation folds used for pruning.</summary>
        public int CVFolds
        {
            get { return GetInt(IntCvFolds); }
            set { SetInt(IntCvFolds, value); }
        }

        /// <summary>Gets or sets whether surrogate splits are requested.</summary>
        public bool UseSurrogates
        {
            get { return GetInt(IntUseSurrogates) != 0; }
            set { SetInt(IntUseSurrogates, value ? 1 : 0); }
        }

        /// <summary>Gets or sets whether the one-standard-error pruning rule is used.</summary>
        public bool Use1SERule
        {
            get { return GetInt(IntUse1SeRule) != 0; }
            set { SetInt(IntUse1SeRule, value ? 1 : 0); }
        }

        /// <summary>Gets or sets whether pruned branches are physically removed.</summary>
        public bool TruncatePrunedTree
        {
            get { return GetInt(IntTruncatePrunedTree) != 0; }
            set { SetInt(IntTruncatePrunedTree, value ? 1 : 0); }
        }

        /// <summary>Gets or sets the regression-tree stopping accuracy.</summary>
        public float RegressionAccuracy
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.MlDTreesGetRegressionAccuracy(NativeHandle, out float value));
                return value;
            }

            set
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.MlDTreesSetRegressionAccuracy(NativeHandle, value));
            }
        }

        /// <summary>Creates an empty decision-tree model.</summary>
        public static DTrees Create()
        {
            NativeException.ThrowIfError(NativeMethods.MlDTreesCreate(out IntPtr nativeHandle));
            return new DTrees(nativeHandle);
        }

        /// <summary>Loads a serialized decision-tree model.</summary>
        public static DTrees Load(string filepath, string? nodeName = null)
        {
            byte[] nativePath = MLStringConvert.ToNullTerminatedUtf8(filepath, nameof(filepath));
            byte[] nativeNodeName = MLStringConvert.ToNullTerminatedUtf8(nodeName, nameof(nodeName), allowNull: true);
            NativeException.ThrowIfError(NativeMethods.MlDTreesLoad(nativePath, nativeNodeName, out IntPtr nativeHandle));
            return new DTrees(nativeHandle);
        }

        /// <summary>Returns a deep copy of the current class-prior matrix.</summary>
        public Mat GetPriors()
        {
            var dst = new Mat();
            try
            {
                GetPriors(dst);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>Copies the current class priors into a caller-owned matrix.</summary>
        public void GetPriors(Mat dst)
        {
            ThrowIfDisposed();
            TrainData.ValidateNotNull(dst, nameof(dst));
            NativeException.ThrowIfError(NativeMethods.MlDTreesGetPriors(NativeHandle, dst.NativeHandle));
        }

        /// <summary>
        /// Sets class priors. OpenCV retains a reference-counted view of this matrix until the
        /// property is replaced, the model is trained or cleared, or the model is disposed.
        /// </summary>
        public void SetPriors(Mat priors)
        {
            ThrowIfDisposed();
            TrainData.ValidateNotNull(priors, nameof(priors));
            NativeException.ThrowIfError(NativeMethods.MlDTreesSetPriors(NativeHandle, priors.NativeHandle));
        }

        /// <summary>Predicts with an explicit decision-tree aggregation mode.</summary>
        public float Predict(
            Mat samples,
            DTreesPredictionFlags treeFlags,
            Mat? results = null,
            StatModelFlags flags = StatModelFlags.None)
        {
            ValidatePredictionFlags(treeFlags, nameof(treeFlags));
            return base.Predict(samples, results, (StatModelFlags)((int)flags | (int)treeFlags));
        }

        internal static void ValidatePredictionFlags(DTreesPredictionFlags flags, string parameterName)
        {
            if (flags != DTreesPredictionFlags.Auto &&
                flags != DTreesPredictionFlags.Sum &&
                flags != DTreesPredictionFlags.MaxVote)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }

        private int GetInt(int propertyId)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MlDTreesGetInt(NativeHandle, propertyId, out int value));
            return value;
        }

        private void SetInt(int propertyId, int value)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MlDTreesSetInt(NativeHandle, propertyId, value));
        }
    }
}

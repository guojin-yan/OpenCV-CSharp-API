using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.ML
{
    /// <summary>Random-forest model.</summary>
    public sealed class RTrees : DTrees
    {
        private const int IntCalculateVarImportance = 0;
        private const int IntActiveVarCount = 1;

        private RTrees(IntPtr nativeHandle)
            : base(nativeHandle)
        {
        }

        /// <summary>Gets or sets whether training calculates variable importance.</summary>
        public bool CalculateVarImportance
        {
            get { return GetInt(IntCalculateVarImportance) != 0; }
            set { SetInt(IntCalculateVarImportance, value ? 1 : 0); }
        }

        /// <summary>Gets or sets the number of randomly selected variables at each split.</summary>
        public int ActiveVarCount
        {
            get { return GetInt(IntActiveVarCount); }
            set { SetInt(IntActiveVarCount, value); }
        }

        /// <summary>Gets or sets the forest training termination criteria.</summary>
        public TermCriteria TermCriteria
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.MlRTreesGetTermCriteria(NativeHandle, out int type, out int maxCount, out double epsilon));
                return new TermCriteria((TermCriteriaTypes)type, maxCount, epsilon);
            }

            set
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.MlRTreesSetTermCriteria(NativeHandle, (int)value.Type, value.MaxCount, value.Epsilon));
            }
        }

        /// <summary>Gets the out-of-bag error recorded by the trained forest.</summary>
        public double OobError
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.MlRTreesGetOobError(NativeHandle, out double value));
                return value;
            }
        }

        /// <summary>Creates an empty random-forest model.</summary>
        public new static RTrees Create()
        {
            NativeException.ThrowIfError(NativeMethods.MlRTreesCreate(out IntPtr nativeHandle));
            return new RTrees(nativeHandle);
        }

        /// <summary>Loads a serialized random-forest model.</summary>
        public new static RTrees Load(string filepath, string? nodeName = null)
        {
            byte[] nativePath = MLStringConvert.ToNullTerminatedUtf8(filepath, nameof(filepath));
            byte[] nativeNodeName = MLStringConvert.ToNullTerminatedUtf8(nodeName, nameof(nodeName), allowNull: true);
            NativeException.ThrowIfError(NativeMethods.MlRTreesLoad(nativePath, nativeNodeName, out IntPtr nativeHandle));
            return new RTrees(nativeHandle);
        }

        /// <summary>Returns a deep copy of the variable-importance vector.</summary>
        public Mat GetVarImportance()
        {
            var dst = new Mat();
            try
            {
                GetVarImportance(dst);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>Copies variable importance into a caller-owned matrix.</summary>
        public void GetVarImportance(Mat dst)
        {
            ThrowIfDisposed();
            TrainData.ValidateNotNull(dst, nameof(dst));
            NativeException.ThrowIfError(NativeMethods.MlRTreesGetVarImportance(NativeHandle, dst.NativeHandle));
        }

        /// <summary>Returns individual tree responses or class-vote counts in a new matrix.</summary>
        public Mat GetVotes(
            Mat samples,
            DTreesPredictionFlags treeFlags = DTreesPredictionFlags.Auto,
            StatModelFlags flags = StatModelFlags.None)
        {
            var dst = new Mat();
            try
            {
                GetVotes(samples, dst, treeFlags, flags);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>Copies individual tree responses or class-vote counts into a caller-owned matrix.</summary>
        public void GetVotes(
            Mat samples,
            Mat results,
            DTreesPredictionFlags treeFlags = DTreesPredictionFlags.Auto,
            StatModelFlags flags = StatModelFlags.None)
        {
            ThrowIfDisposed();
            TrainData.ValidateNotNull(samples, nameof(samples));
            TrainData.ValidateNotNull(results, nameof(results));
            ValidatePredictionFlags(treeFlags, nameof(treeFlags));
            NativeException.ThrowIfError(NativeMethods.MlRTreesGetVotes(
                NativeHandle,
                samples.NativeHandle,
                results.NativeHandle,
                (int)flags | (int)treeFlags));
        }

        private int GetInt(int propertyId)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MlRTreesGetInt(NativeHandle, propertyId, out int value));
            return value;
        }

        private void SetInt(int propertyId, int value)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MlRTreesSetInt(NativeHandle, propertyId, value));
        }
    }
}

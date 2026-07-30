using System;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.ML
{
    /// <summary>Boosted decision-tree model.</summary>
    public sealed class Boost : DTrees
    {
        private const int IntBoostType = 0;
        private const int IntWeakCount = 1;

        private Boost(IntPtr nativeHandle)
            : base(nativeHandle)
        {
        }

        /// <summary>Gets or sets the boosting algorithm.</summary>
        public BoostTypes BoostType
        {
            get { return (BoostTypes)GetInt(IntBoostType); }
            set
            {
                ValidateBoostType(value);
                SetInt(IntBoostType, (int)value);
            }
        }

        /// <summary>Gets or sets the number of weak learners.</summary>
        public int WeakCount
        {
            get { return GetInt(IntWeakCount); }
            set { SetInt(IntWeakCount, value); }
        }

        /// <summary>Gets or sets the training weight-trimming threshold.</summary>
        public double WeightTrimRate
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.MlBoostGetWeightTrimRate(NativeHandle, out double value));
                return value;
            }

            set
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.MlBoostSetWeightTrimRate(NativeHandle, value));
            }
        }

        /// <summary>Creates an empty boosted-tree model.</summary>
        public new static Boost Create()
        {
            NativeException.ThrowIfError(NativeMethods.MlBoostCreate(out IntPtr nativeHandle));
            return new Boost(nativeHandle);
        }

        /// <summary>Loads a serialized boosted-tree model.</summary>
        public new static Boost Load(string filepath, string? nodeName = null)
        {
            byte[] nativePath = MLStringConvert.ToNullTerminatedUtf8(filepath, nameof(filepath));
            byte[] nativeNodeName = MLStringConvert.ToNullTerminatedUtf8(nodeName, nameof(nodeName), allowNull: true);
            NativeException.ThrowIfError(NativeMethods.MlBoostLoad(nativePath, nativeNodeName, out IntPtr nativeHandle));
            return new Boost(nativeHandle);
        }

        private static void ValidateBoostType(BoostTypes value)
        {
            if ((int)value < (int)BoostTypes.Discrete || (int)value > (int)BoostTypes.Gentle)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
        }

        private int GetInt(int propertyId)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MlBoostGetInt(NativeHandle, propertyId, out int value));
            return value;
        }

        private void SetInt(int propertyId, int value)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MlBoostSetInt(NativeHandle, propertyId, value));
        }
    }
}

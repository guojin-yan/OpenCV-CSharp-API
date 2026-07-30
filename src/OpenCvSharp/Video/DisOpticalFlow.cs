using System;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Video
{
    /// <summary>DIS optical-flow quality and speed presets.</summary>
    public enum DisOpticalFlowPreset
    {
        /// <summary>Lowest-cost preset.</summary>
        UltraFast = 0,
        /// <summary>Default fast preset.</summary>
        Fast = 1,
        /// <summary>Higher-quality medium preset.</summary>
        Medium = 2
    }

    /// <summary>Dense Inverse Search optical-flow algorithm.</summary>
    public sealed class DisOpticalFlow : DenseOpticalFlow
    {
        /// <summary>Creates a DIS algorithm from a preset.</summary>
        public DisOpticalFlow(DisOpticalFlowPreset preset = DisOpticalFlowPreset.Fast)
            : base(CreateNative(preset))
        {
        }

        /// <summary>Creates a DIS algorithm from a preset.</summary>
        public static DisOpticalFlow Create(DisOpticalFlowPreset preset = DisOpticalFlowPreset.Fast) => new DisOpticalFlow(preset);

        /// <summary>Gets or sets the finest pyramid scale.</summary>
        public int FinestScale { get => GetInt(0); set => SetInt(0, value); }
        /// <summary>Gets or sets the coarsest pyramid scale.</summary>
        public int CoarsestScale { get => GetInt(1); set => SetInt(1, value); }
        /// <summary>Gets or sets the square patch size.</summary>
        public int PatchSize { get => GetInt(2); set => SetInt(2, value); }
        /// <summary>Gets or sets the patch-grid stride.</summary>
        public int PatchStride { get => GetInt(3); set => SetInt(3, value); }
        /// <summary>Gets or sets gradient-descent iterations.</summary>
        public int GradientDescentIterations { get => GetInt(4); set => SetInt(4, value); }
        /// <summary>Gets or sets variational-refinement iterations.</summary>
        public int VariationalRefinementIterations { get => GetInt(5); set => SetInt(5, value); }
        /// <summary>Gets or sets variational smoothness weight.</summary>
        public float VariationalRefinementAlpha { get => GetFloat(0); set => SetFloat(0, value); }
        /// <summary>Gets or sets variational color weight.</summary>
        public float VariationalRefinementDelta { get => GetFloat(1); set => SetFloat(1, value); }
        /// <summary>Gets or sets variational gradient weight.</summary>
        public float VariationalRefinementGamma { get => GetFloat(2); set => SetFloat(2, value); }
        /// <summary>Gets or sets variational robust regularizer.</summary>
        public float VariationalRefinementEpsilon { get => GetFloat(3); set => SetFloat(3, value); }
        /// <summary>Gets or sets patch-mean normalization.</summary>
        public bool UseMeanNormalization { get => GetBool(0); set => SetBool(0, value); }
        /// <summary>Gets or sets spatial propagation.</summary>
        public bool UseSpatialPropagation { get => GetBool(1); set => SetBool(1, value); }

        private int GetInt(int propertyId) { NativeException.ThrowIfError(NativeMethods.DisOpticalFlowGetIntProperty(NativeHandle, propertyId, out int value)); return value; }
        private void SetInt(int propertyId, int value) { NativeException.ThrowIfError(NativeMethods.DisOpticalFlowSetIntProperty(NativeHandle, propertyId, value)); }
        private float GetFloat(int propertyId) { NativeException.ThrowIfError(NativeMethods.DisOpticalFlowGetFloatProperty(NativeHandle, propertyId, out float value)); return value; }
        private void SetFloat(int propertyId, float value) { if (float.IsNaN(value) || float.IsInfinity(value)) throw new ArgumentOutOfRangeException(nameof(value)); NativeException.ThrowIfError(NativeMethods.DisOpticalFlowSetFloatProperty(NativeHandle, propertyId, value)); }
        private bool GetBool(int propertyId) { NativeException.ThrowIfError(NativeMethods.DisOpticalFlowGetBoolProperty(NativeHandle, propertyId, out int value)); return value != 0; }
        private void SetBool(int propertyId, bool value) { NativeException.ThrowIfError(NativeMethods.DisOpticalFlowSetBoolProperty(NativeHandle, propertyId, value ? 1 : 0)); }

        private static IntPtr CreateNative(DisOpticalFlowPreset preset)
        {
            if (preset < DisOpticalFlowPreset.UltraFast || preset > DisOpticalFlowPreset.Medium) throw new ArgumentOutOfRangeException(nameof(preset));
            NativeException.ThrowIfError(NativeMethods.DisOpticalFlowCreate((int)preset, out IntPtr handle));
            return handle;
        }
    }
}

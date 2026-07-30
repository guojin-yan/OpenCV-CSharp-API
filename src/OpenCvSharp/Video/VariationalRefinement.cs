using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Video
{
    /// <summary>Variational refinement for dense optical flow.</summary>
    public sealed class VariationalRefinement : DenseOpticalFlow
    {
        /// <summary>Creates a variational-refinement algorithm.</summary>
        public VariationalRefinement()
            : base(CreateNative())
        {
        }

        /// <summary>Creates a variational-refinement algorithm.</summary>
        public static VariationalRefinement Create() => new VariationalRefinement();

        /// <summary>Gets or sets the outer fixed-point iteration count.</summary>
        public int FixedPointIterations { get => GetInt(0); set => SetInt(0, value); }
        /// <summary>Gets or sets the successive over-relaxation iteration count.</summary>
        public int SorIterations { get => GetInt(1); set => SetInt(1, value); }
        /// <summary>Gets or sets the relaxation factor.</summary>
        public float Omega { get => GetFloat(0); set => SetFloat(0, value); }
        /// <summary>Gets or sets the smoothness weight.</summary>
        public float Alpha { get => GetFloat(1); set => SetFloat(1, value); }
        /// <summary>Gets or sets the color-constancy weight.</summary>
        public float Delta { get => GetFloat(2); set => SetFloat(2, value); }
        /// <summary>Gets or sets the gradient-constancy weight.</summary>
        public float Gamma { get => GetFloat(3); set => SetFloat(3, value); }
        /// <summary>Gets or sets the robust penalty regularizer.</summary>
        public float Epsilon { get => GetFloat(4); set => SetFloat(4, value); }

        /// <summary>Refines separate horizontal and vertical single-channel flow fields in place.</summary>
        public void CalcUV(Mat first, Mat second, Mat flowU, Mat flowV)
        {
            ValidateNotNull(first, nameof(first)); ValidateNotNull(second, nameof(second));
            ValidateNotNull(flowU, nameof(flowU)); ValidateNotNull(flowV, nameof(flowV));
            NativeException.ThrowIfError(NativeMethods.VariationalRefinementCalcUV(NativeHandle, first.NativeHandle, second.NativeHandle, flowU.NativeHandle, flowV.NativeHandle));
        }

        private int GetInt(int propertyId) { NativeException.ThrowIfError(NativeMethods.VariationalRefinementGetIntProperty(NativeHandle, propertyId, out int value)); return value; }
        private void SetInt(int propertyId, int value) { NativeException.ThrowIfError(NativeMethods.VariationalRefinementSetIntProperty(NativeHandle, propertyId, value)); }
        private float GetFloat(int propertyId) { NativeException.ThrowIfError(NativeMethods.VariationalRefinementGetFloatProperty(NativeHandle, propertyId, out float value)); return value; }
        private void SetFloat(int propertyId, float value) { if (float.IsNaN(value) || float.IsInfinity(value)) throw new ArgumentOutOfRangeException(nameof(value)); NativeException.ThrowIfError(NativeMethods.VariationalRefinementSetFloatProperty(NativeHandle, propertyId, value)); }
        private static IntPtr CreateNative() { NativeException.ThrowIfError(NativeMethods.VariationalRefinementCreate(out IntPtr handle)); return handle; }
    }
}

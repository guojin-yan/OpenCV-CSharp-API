using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.ML
{
    /// <summary>
    /// Support vector machine model.
    /// 支持向量机模型。
    /// </summary>
    public sealed class SVM : StatModel
    {
        private const int IntType = 0;
        private const int IntKernelType = 1;

        private const int DoubleGamma = 0;
        private const int DoubleCoef0 = 1;
        private const int DoubleDegree = 2;
        private const int DoubleC = 3;
        private const int DoubleNu = 4;
        private const int DoubleP = 5;

        private SVM(IntPtr nativeHandle)
            : base(nativeHandle)
        {
        }

        /// <summary>Gets or sets the SVM type. 获取或设置 SVM 类型。</summary>
        public SVMTypes Type
        {
            get { return (SVMTypes)GetInt(IntType); }
            set { SetInt(IntType, (int)value); }
        }

        /// <summary>Gets the current kernel type. 获取当前核函数类型。</summary>
        public SVMKernelTypes KernelType
        {
            get { return (SVMKernelTypes)GetInt(IntKernelType); }
        }

        /// <summary>Gets or sets gamma. 获取或设置 gamma。</summary>
        public double Gamma
        {
            get { return GetDouble(DoubleGamma); }
            set { SetDouble(DoubleGamma, value); }
        }

        /// <summary>Gets or sets coef0. 获取或设置 coef0。</summary>
        public double Coef0
        {
            get { return GetDouble(DoubleCoef0); }
            set { SetDouble(DoubleCoef0, value); }
        }

        /// <summary>Gets or sets degree. 获取或设置 degree。</summary>
        public double Degree
        {
            get { return GetDouble(DoubleDegree); }
            set { SetDouble(DoubleDegree, value); }
        }

        /// <summary>Gets or sets C. 获取或设置 C。</summary>
        public double C
        {
            get { return GetDouble(DoubleC); }
            set { SetDouble(DoubleC, value); }
        }

        /// <summary>Gets or sets nu. 获取或设置 nu。</summary>
        public double Nu
        {
            get { return GetDouble(DoubleNu); }
            set { SetDouble(DoubleNu, value); }
        }

        /// <summary>Gets or sets p. 获取或设置 p。</summary>
        public double P
        {
            get { return GetDouble(DoubleP); }
            set { SetDouble(DoubleP, value); }
        }

        /// <summary>Gets or sets the training termination criteria. 获取或设置训练终止条件。</summary>
        public TermCriteria TermCriteria
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.MlSvmGetTermCriteria(NativeHandle, out int type, out int maxCount, out double epsilon));
                return new TermCriteria((TermCriteriaTypes)type, maxCount, epsilon);
            }

            set
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.MlSvmSetTermCriteria(NativeHandle, (int)value.Type, value.MaxCount, value.Epsilon));
            }
        }

        /// <summary>Creates an empty SVM model. 创建空 SVM 模型。</summary>
        public static SVM Create()
        {
            NativeException.ThrowIfError(NativeMethods.MlSvmCreate(out IntPtr nativeHandle));
            return new SVM(nativeHandle);
        }

        /// <summary>Loads a serialized SVM model. 加载序列化 SVM 模型。</summary>
        public static SVM Load(string filepath)
        {
            byte[] nativePath = MLStringConvert.ToNullTerminatedUtf8(filepath, nameof(filepath));
            NativeException.ThrowIfError(NativeMethods.MlSvmLoad(nativePath, out IntPtr nativeHandle));
            return new SVM(nativeHandle);
        }

        /// <summary>Gets the default grid for a parameter. 获取参数的默认网格。</summary>
        public static ParamGrid GetDefaultGrid(SVMParamTypes paramType)
        {
            NativeException.ThrowIfError(NativeMethods.MlSvmGetDefaultGrid((int)paramType, out IntPtr nativeHandle));
            return new ParamGrid(nativeHandle);
        }

        /// <summary>Sets the SVM kernel type. 设置 SVM 核函数类型。</summary>
        public void SetKernel(SVMKernelTypes kernelType)
        {
            SetInt(IntKernelType, (int)kernelType);
        }

        /// <summary>Gets class weights as a new matrix. 以新矩阵获取类别权重。</summary>
        public Mat GetClassWeights()
        {
            var dst = new Mat();
            try
            {
                GetClassWeights(dst);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>Writes class weights into a matrix. 将类别权重写入矩阵。</summary>
        public void GetClassWeights(Mat dst)
        {
            ThrowIfDisposed();
            TrainData.ValidateNotNull(dst, nameof(dst));
            NativeException.ThrowIfError(NativeMethods.MlSvmGetClassWeights(NativeHandle, dst.NativeHandle));
        }

        /// <summary>Sets class weights. 设置类别权重。</summary>
        public void SetClassWeights(Mat weights)
        {
            ThrowIfDisposed();
            TrainData.ValidateNotNull(weights, nameof(weights));
            NativeException.ThrowIfError(NativeMethods.MlSvmSetClassWeights(NativeHandle, weights.NativeHandle));
        }

        /// <summary>
        /// Trains the SVM with automatic parameter selection.
        /// 使用自动参数选择训练 SVM。
        /// </summary>
        public bool TrainAuto(
            Mat samples,
            SampleTypes layout,
            Mat responses,
            int kFold = 10,
            ParamGrid? cGrid = null,
            ParamGrid? gammaGrid = null,
            ParamGrid? pGrid = null,
            ParamGrid? nuGrid = null,
            ParamGrid? coeffGrid = null,
            ParamGrid? degreeGrid = null,
            bool balanced = false)
        {
            ThrowIfDisposed();
            TrainData.ValidateNotNull(samples, nameof(samples));
            TrainData.ValidateNotNull(responses, nameof(responses));
            NativeException.ThrowIfError(NativeMethods.MlSvmTrainAuto(
                NativeHandle,
                samples.NativeHandle,
                (int)layout,
                responses.NativeHandle,
                kFold,
                OptionalHandle(cGrid),
                OptionalHandle(gammaGrid),
                OptionalHandle(pGrid),
                OptionalHandle(nuGrid),
                OptionalHandle(coeffGrid),
                OptionalHandle(degreeGrid),
                balanced ? 1 : 0,
                out int result));
            return result != 0;
        }

        /// <summary>Gets support vectors. 获取支持向量。</summary>
        public Mat GetSupportVectors()
        {
            var dst = new Mat();
            try
            {
                NativeException.ThrowIfError(NativeMethods.MlSvmGetSupportVectors(NativeHandle, dst.NativeHandle));
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>Gets uncompressed support vectors. 获取未压缩支持向量。</summary>
        public Mat GetUncompressedSupportVectors()
        {
            var dst = new Mat();
            try
            {
                NativeException.ThrowIfError(NativeMethods.MlSvmGetUncompressedSupportVectors(NativeHandle, dst.NativeHandle));
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Gets a decision function.
        /// 获取决策函数。
        /// </summary>
        public double GetDecisionFunction(int index, Mat? alpha = null, Mat? svidx = null)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MlSvmGetDecisionFunction(NativeHandle, index, OptionalHandle(alpha), OptionalHandle(svidx), out double rho));
            return rho;
        }

        private int GetInt(int propertyId)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MlSvmGetInt(NativeHandle, propertyId, out int value));
            return value;
        }

        private void SetInt(int propertyId, int value)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MlSvmSetInt(NativeHandle, propertyId, value));
        }

        private double GetDouble(int propertyId)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MlSvmGetDouble(NativeHandle, propertyId, out double value));
            return value;
        }

        private void SetDouble(int propertyId, double value)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MlSvmSetDouble(NativeHandle, propertyId, value));
        }
    }
}

using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.ML
{
    /// <summary>
    /// Base class for OpenCV ML statistical models.
    /// OpenCV ML 统计模型基类。
    /// </summary>
    public abstract class StatModel : IDisposable
    {
        private const int PropertyVarCount = 0;
        private const int PropertyEmpty = 1;
        private const int PropertyIsTrained = 2;
        private const int PropertyIsClassifier = 3;

        private NativeMlModelHandle handle;
        private bool disposed;

        internal StatModel(IntPtr nativeHandle)
        {
            handle = NativeMlModelHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Gets whether this model has been disposed. 获取模型是否已经释放。</summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        /// <summary>Gets whether the native model is empty. 获取 native 模型是否为空。</summary>
        public bool Empty
        {
            get { return GetStatInt(PropertyEmpty) != 0; }
        }

        /// <summary>Gets whether the model has been trained. 获取模型是否已经训练。</summary>
        public bool IsTrained
        {
            get { return GetStatInt(PropertyIsTrained) != 0; }
        }

        /// <summary>Gets whether the model is a classifier. 获取模型是否为分类器。</summary>
        public bool IsClassifier
        {
            get { return GetStatInt(PropertyIsClassifier) != 0; }
        }

        /// <summary>Gets the number of variables. 获取变量数量。</summary>
        public int VarCount
        {
            get { return GetStatInt(PropertyVarCount); }
        }

        internal IntPtr NativeHandle
        {
            get
            {
                ThrowIfDisposed();
                return handle.DangerousGetHandle();
            }
        }

        /// <summary>
        /// Trains this model from prepared training data.
        /// 使用准备好的训练数据训练模型。
        /// </summary>
        public bool Train(TrainData trainData, StatModelFlags flags = StatModelFlags.None)
        {
            ThrowIfDisposed();
            TrainData.ValidateNotNull(trainData, nameof(trainData));
            NativeException.ThrowIfError(NativeMethods.MlStatModelTrainData(NativeHandle, trainData.NativeHandle, (int)flags, out int result));
            return result != 0;
        }

        /// <summary>
        /// Trains this model from sample and response matrices.
        /// 使用样本和响应矩阵训练模型。
        /// </summary>
        public bool Train(Mat samples, SampleTypes layout, Mat responses)
        {
            ThrowIfDisposed();
            TrainData.ValidateNotNull(samples, nameof(samples));
            TrainData.ValidateNotNull(responses, nameof(responses));
            NativeException.ThrowIfError(NativeMethods.MlStatModelTrainSamples(NativeHandle, samples.NativeHandle, (int)layout, responses.NativeHandle, out int result));
            return result != 0;
        }

        /// <summary>
        /// Predicts responses for samples.
        /// 预测样本响应。
        /// </summary>
        public float Predict(Mat samples, Mat? results = null, StatModelFlags flags = StatModelFlags.None)
        {
            ThrowIfDisposed();
            TrainData.ValidateNotNull(samples, nameof(samples));
            NativeException.ThrowIfError(NativeMethods.MlStatModelPredict(NativeHandle, samples.NativeHandle, OptionalHandle(results), (int)flags, out float value));
            return value;
        }

        /// <summary>
        /// Calculates model error for training data.
        /// 计算模型在训练数据上的误差。
        /// </summary>
        public float CalcError(TrainData data, bool test, Mat? responses = null)
        {
            ThrowIfDisposed();
            TrainData.ValidateNotNull(data, nameof(data));
            NativeException.ThrowIfError(NativeMethods.MlStatModelCalcError(NativeHandle, data.NativeHandle, test ? 1 : 0, OptionalHandle(responses), out float value));
            return value;
        }

        /// <summary>
        /// Saves the model to a file.
        /// 将模型保存到文件。
        /// </summary>
        public void Save(string filepath)
        {
            ThrowIfDisposed();
            byte[] nativePath = MLStringConvert.ToNullTerminatedUtf8(filepath, nameof(filepath));
            NativeException.ThrowIfError(NativeMethods.MlStatModelSave(NativeHandle, nativePath));
        }

        /// <summary>
        /// Clears native model state.
        /// 清除 native 模型状态。
        /// </summary>
        public void Clear()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MlStatModelClear(NativeHandle));
        }

        /// <summary>Releases native resources. 释放 native 资源。</summary>
        public void Dispose()
        {
            if (!disposed)
            {
                handle.Dispose();
                disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>Throws when disposed. 已释放时抛出异常。</summary>
        protected void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        private int GetStatInt(int propertyId)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MlStatModelGetInt(NativeHandle, propertyId, out int value));
            return value;
        }

        internal static IntPtr OptionalHandle(Mat? mat)
        {
            return mat == null ? IntPtr.Zero : mat.NativeHandle;
        }

        internal static IntPtr OptionalHandle(ParamGrid? grid)
        {
            return grid == null ? IntPtr.Zero : grid.NativeHandle;
        }
    }
}

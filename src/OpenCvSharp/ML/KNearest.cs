using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.ML
{
    /// <summary>
    /// K-nearest neighbors model.
    /// K 最近邻模型。
    /// </summary>
    public sealed class KNearest : StatModel
    {
        private const int PropertyDefaultK = 0;
        private const int PropertyIsClassifier = 1;
        private const int PropertyEmax = 2;
        private const int PropertyAlgorithmType = 3;

        private KNearest(IntPtr nativeHandle)
            : base(nativeHandle)
        {
        }

        /// <summary>Gets or sets the default neighbor count. 获取或设置默认邻居数量。</summary>
        public int DefaultK
        {
            get { return GetInt(PropertyDefaultK); }
            set { SetInt(PropertyDefaultK, value); }
        }

        /// <summary>Gets or sets whether this is a classifier. 获取或设置是否为分类器。</summary>
        public bool IsClassifierModel
        {
            get { return GetInt(PropertyIsClassifier) != 0; }
            set { SetInt(PropertyIsClassifier, value ? 1 : 0); }
        }

        /// <summary>Gets or sets KDTree Emax. 获取或设置 KDTree Emax。</summary>
        public int Emax
        {
            get { return GetInt(PropertyEmax); }
            set { SetInt(PropertyEmax, value); }
        }

        /// <summary>Gets or sets the algorithm type. 获取或设置算法类型。</summary>
        public KNearestTypes AlgorithmType
        {
            get { return (KNearestTypes)GetInt(PropertyAlgorithmType); }
            set { SetInt(PropertyAlgorithmType, (int)value); }
        }

        /// <summary>Creates an empty KNearest model. 创建空 KNearest 模型。</summary>
        public static KNearest Create()
        {
            NativeException.ThrowIfError(NativeMethods.MlKNearestCreate(out IntPtr nativeHandle));
            return new KNearest(nativeHandle);
        }

        /// <summary>Loads a serialized KNearest model. 加载序列化 KNearest 模型。</summary>
        public static KNearest Load(string filepath)
        {
            byte[] nativePath = MLStringConvert.ToNullTerminatedUtf8(filepath, nameof(filepath));
            NativeException.ThrowIfError(NativeMethods.MlKNearestLoad(nativePath, out IntPtr nativeHandle));
            return new KNearest(nativeHandle);
        }

        /// <summary>
        /// Finds nearest neighbors and predicts responses.
        /// 查找最近邻并预测响应。
        /// </summary>
        public float FindNearest(Mat samples, int k, Mat results, Mat? neighborResponses = null, Mat? dist = null)
        {
            ThrowIfDisposed();
            TrainData.ValidateNotNull(samples, nameof(samples));
            TrainData.ValidateNotNull(results, nameof(results));
            NativeException.ThrowIfError(NativeMethods.MlKNearestFindNearest(
                NativeHandle,
                samples.NativeHandle,
                k,
                results.NativeHandle,
                OptionalHandle(neighborResponses),
                OptionalHandle(dist),
                out float value));
            return value;
        }

        private int GetInt(int propertyId)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MlKNearestGetInt(NativeHandle, propertyId, out int value));
            return value;
        }

        private void SetInt(int propertyId, int value)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.MlKNearestSetInt(NativeHandle, propertyId, value));
        }
    }
}

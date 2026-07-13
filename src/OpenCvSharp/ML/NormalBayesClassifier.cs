using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.ML
{
    /// <summary>
    /// Normal Bayes classifier.
    /// 正态贝叶斯分类器。
    /// </summary>
    public sealed class NormalBayesClassifier : StatModel
    {
        private NormalBayesClassifier(IntPtr nativeHandle)
            : base(nativeHandle)
        {
        }

        /// <summary>Creates an empty classifier. 创建空分类器。</summary>
        public static NormalBayesClassifier Create()
        {
            NativeException.ThrowIfError(NativeMethods.MlNormalBayesClassifierCreate(out IntPtr nativeHandle));
            return new NormalBayesClassifier(nativeHandle);
        }

        /// <summary>Loads a serialized classifier. 加载序列化分类器。</summary>
        public static NormalBayesClassifier Load(string filepath, string? nodeName = null)
        {
            byte[] nativePath = MLStringConvert.ToNullTerminatedUtf8(filepath, nameof(filepath));
            byte[] nativeNodeName = MLStringConvert.ToNullTerminatedUtf8(nodeName, nameof(nodeName), allowNull: true);
            NativeException.ThrowIfError(NativeMethods.MlNormalBayesClassifierLoad(nativePath, nativeNodeName, out IntPtr nativeHandle));
            return new NormalBayesClassifier(nativeHandle);
        }

        /// <summary>
        /// Predicts classes and probabilities.
        /// 预测类别和概率。
        /// </summary>
        public float PredictProb(Mat inputs, Mat outputs, Mat outputProbs, StatModelFlags flags = StatModelFlags.None)
        {
            ThrowIfDisposed();
            TrainData.ValidateNotNull(inputs, nameof(inputs));
            TrainData.ValidateNotNull(outputs, nameof(outputs));
            TrainData.ValidateNotNull(outputProbs, nameof(outputProbs));
            NativeException.ThrowIfError(NativeMethods.MlNormalBayesClassifierPredictProb(
                NativeHandle,
                inputs.NativeHandle,
                outputs.NativeHandle,
                outputProbs.NativeHandle,
                (int)flags,
                out float value));
            return value;
        }
    }
}

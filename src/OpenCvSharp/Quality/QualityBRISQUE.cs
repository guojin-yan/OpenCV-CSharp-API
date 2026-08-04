using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Quality
{
    /// <summary>
    /// BRISQUE no-reference image quality metric.
    /// BRISQUE 无参考图像质量指标。
    /// </summary>
    public sealed class QualityBRISQUE : QualityBase
    {
        /// <summary>Creates a BRISQUE metric from model and range files. 使用模型和 range 文件创建 BRISQUE 指标。</summary>
        public QualityBRISQUE(string modelFilePath, string rangeFilePath)
            : base(CreateHandle(modelFilePath, rangeFilePath))
        {
        }

        /// <summary>Creates a BRISQUE metric from model and range files. 使用模型和 range 文件创建 BRISQUE 指标。</summary>
        public static QualityBRISQUE Create(string modelFilePath, string rangeFilePath)
        {
            return new QualityBRISQUE(modelFilePath, rangeFilePath);
        }

        /// <summary>
        /// Computes BRISQUE score for an image.
        /// 计算图像的 BRISQUE 分数。
        /// </summary>
        public static Scalar Compute(Mat image, string modelFilePath, string rangeFilePath)
        {
            ValidateNotNull(image, nameof(image));
            byte[] model = QualityStringConvert.ToNullTerminatedUtf8(modelFilePath, nameof(modelFilePath));
            byte[] range = QualityStringConvert.ToNullTerminatedUtf8(rangeFilePath, nameof(rangeFilePath));
            ValidateComputeImage(image, nameof(image));
            var values = new double[4];
            NativeException.ThrowIfError(NativeMethods.QualityBRISQUEComputeStatic(image.NativeHandle, model, range, values, values.Length));
            return ToScalar(values);
        }

        /// <summary>
        /// Computes BRISQUE features for an image.
        /// 计算图像的 BRISQUE 特征。
        /// </summary>
        public static void ComputeFeatures(Mat image, Mat features)
        {
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(features, nameof(features));
            ValidateComputeFeaturesImage(image, nameof(image));
            NativeException.ThrowIfError(NativeMethods.QualityBRISQUEComputeFeatures(image.NativeHandle, features.NativeHandle));
        }

        /// <summary>
        /// Computes BRISQUE features and returns a new matrix.
        /// 计算 BRISQUE 特征并返回新矩阵。
        /// </summary>
        public static Mat ComputeFeatures(Mat image)
        {
            var features = new Mat();
            try
            {
                ComputeFeatures(image, features);
                return features;
            }
            catch
            {
                features.Dispose();
                throw;
            }
        }

        private static NativeQualityHandle CreateHandle(string modelFilePath, string rangeFilePath)
        {
            byte[] model = QualityStringConvert.ToNullTerminatedUtf8(modelFilePath, nameof(modelFilePath));
            byte[] range = QualityStringConvert.ToNullTerminatedUtf8(rangeFilePath, nameof(rangeFilePath));
            NativeException.ThrowIfError(NativeMethods.QualityBRISQUECreate(model, range, out IntPtr nativeHandle));
            return NativeQualityHandle.FromNativePointer(nativeHandle);
        }

        private static void ValidateComputeFeaturesImage(Mat image, string parameterName)
        {
            ValidateComputeImage(image, parameterName);
        }

        private static void ValidateComputeImage(Mat image, string parameterName)
        {
            if (image.Empty)
            {
                throw new ArgumentException("BRISQUE input image must be non-empty.", parameterName);
            }

            int channels = image.Channels;
            if (channels != 1 && channels != 3 && channels != 4)
            {
                throw new ArgumentException("BRISQUE input image must have 1, 3, or 4 channels.", parameterName);
            }
        }
    }
}

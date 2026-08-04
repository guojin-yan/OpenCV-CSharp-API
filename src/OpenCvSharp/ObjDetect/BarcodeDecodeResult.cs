using System;
using JYPPX.OpenCvSharp.Core;

namespace JYPPX.OpenCvSharp.ObjDetect
{
    /// <summary>
    /// Barcode decode result.
    /// 条形码解码结果。
    /// </summary>
    public sealed class BarcodeDecodeResult
    {
        /// <summary>
        /// Initializes a barcode decode result.
        /// 初始化条形码解码结果。
        /// </summary>
        public BarcodeDecodeResult(bool success, string[] decodedInfo, string[] decodedTypes, Mat? points)
        {
            string[] normalizedDecodedInfo = Clone(decodedInfo, nameof(decodedInfo), "Decoded info");
            string[] normalizedDecodedTypes = Clone(decodedTypes, nameof(decodedTypes), "Decoded types");
            ValidateTypeCount(normalizedDecodedTypes, normalizedDecodedInfo.Length, nameof(decodedTypes));

            Success = success;
            this.decodedInfo = normalizedDecodedInfo;
            this.decodedTypes = normalizedDecodedTypes;
            Points = points;
        }

        private readonly string[] decodedInfo;
        private readonly string[] decodedTypes;

        /// <summary>Gets whether at least one barcode was decoded. 获取是否至少解码出一个条形码。</summary>
        public bool Success { get; }

        /// <summary>Gets decoded UTF-8 text values. 获取解码出的 UTF-8 文本。</summary>
        public string[] DecodedInfo
        {
            get { return Clone(decodedInfo, nameof(decodedInfo), "Decoded info"); }
        }

        /// <summary>Gets the number of decoded UTF-8 text values. 获取解码出的 UTF-8 文本数量。</summary>
        public int DecodedInfoCount
        {
            get { return decodedInfo.Length; }
        }

        /// <summary>Gets decoded barcode type names. 获取解码出的条形码类型名称。</summary>
        public string[] DecodedTypes
        {
            get { return Clone(decodedTypes, nameof(decodedTypes), "Decoded types"); }
        }

        /// <summary>Gets the number of decoded barcode type names. 获取解码出的条形码类型名称数量。</summary>
        public int DecodedTypeCount
        {
            get { return decodedTypes.Length; }
        }

        /// <summary>Gets detected quadrangle points when supplied by the call. 获取调用产生的检测顶点。</summary>
        public Mat? Points { get; }

        /// <summary>Gets whether detected quadrangle points are available. 获取是否存在检测顶点。</summary>
        public bool HasPoints
        {
            get { return Points != null; }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return
                $"{nameof(BarcodeDecodeResult)}(" +
                $"{nameof(Success)}={Success}, " +
                $"{nameof(DecodedInfo)}={DecodedInfoCount}, " +
                $"{nameof(DecodedTypes)}={DecodedTypeCount}, " +
                $"{nameof(Points)}={FormatMatSize(Points)})";
        }

        private static string FormatMatSize(Mat? mat)
        {
            return mat is null ? "<null>" : $"{mat.Rows}x{mat.Cols}";
        }

        private static string[] Clone(string[] values, string parameterName, string label)
        {
            if (values == null || values.Length == 0)
            {
                return Array.Empty<string>();
            }

            var clone = new string[values.Length];
            Array.Copy(values, clone, clone.Length);
            for (int i = 0; i < clone.Length; i++)
            {
                if (clone[i] == null)
                {
                    throw new ArgumentNullException(parameterName, label + " cannot contain null elements.");
                }
            }

            return clone;
        }

        private static void ValidateTypeCount(string[] decodedTypes, int decodedInfoCount, string parameterName)
        {
            if (decodedTypes.Length != 0 && decodedTypes.Length != decodedInfoCount)
            {
                throw new ArgumentException("Decoded type count must be zero or match the decoded info count.", parameterName);
            }
        }
    }
}

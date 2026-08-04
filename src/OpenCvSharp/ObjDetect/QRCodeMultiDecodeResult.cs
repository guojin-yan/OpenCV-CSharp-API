using System;
using JYPPX.OpenCvSharp.Core;

namespace JYPPX.OpenCvSharp.ObjDetect
{
    /// <summary>
    /// Represents the result of QR multi-code decoding.
    /// 表示多二维码解码结果。
    /// </summary>
    public sealed class QRCodeMultiDecodeResult
    {
        /// <summary>
        /// Initializes a QR multi-code result.
        /// 初始化多二维码结果。
        /// </summary>
        /// <param name="success">Whether OpenCV reported success. OpenCV 是否报告成功。</param>
        /// <param name="decodedInfo">Decoded UTF-8 strings. 解码得到的 UTF-8 字符串。</param>
        /// <param name="points">Detected quadrangle points. 检测到的四边形顶点。</param>
        public QRCodeMultiDecodeResult(bool success, string[] decodedInfo, Mat? points)
        {
            Success = success;
            this.decodedInfo = Clone(decodedInfo, nameof(decodedInfo));
            Points = points;
        }

        private readonly string[] decodedInfo;

        /// <summary>
        /// Gets whether OpenCV reported success.
        /// 获取 OpenCV 是否报告成功。
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Gets decoded UTF-8 strings.
        /// 获取解码得到的 UTF-8 字符串。
        /// </summary>
        public string[] DecodedInfo
        {
            get { return Clone(decodedInfo, nameof(decodedInfo)); }
        }

        /// <summary>
        /// Gets the number of decoded UTF-8 strings.
        /// 获取解码得到的 UTF-8 字符串数量。
        /// </summary>
        public int DecodedInfoCount
        {
            get { return decodedInfo.Length; }
        }

        /// <summary>
        /// Gets detected quadrangle points, or <c>null</c> when not requested.
        /// 获取检测到的四边形顶点；未请求时为 <c>null</c>。
        /// </summary>
        public Mat? Points { get; }

        /// <summary>
        /// Gets whether detected quadrangle points are available.
        /// 获取是否存在检测到的四边形顶点。
        /// </summary>
        public bool HasPoints
        {
            get { return Points != null; }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return
                $"{nameof(QRCodeMultiDecodeResult)}(" +
                $"{nameof(Success)}={Success}, " +
                $"{nameof(DecodedInfo)}={DecodedInfoCount}, " +
                $"{nameof(Points)}={FormatMatSize(Points)})";
        }

        private static string FormatMatSize(Mat? mat)
        {
            return mat is null ? "<null>" : $"{mat.Rows}x{mat.Cols}";
        }

        private static string[] Clone(string[] values, string parameterName)
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
                    throw new ArgumentNullException(parameterName, "Decoded info cannot contain null elements.");
                }
            }

            return clone;
        }
    }
}

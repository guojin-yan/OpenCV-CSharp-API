using System;
using System.Runtime.InteropServices;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.ObjDetect
{
    /// <summary>
    /// Parameters for <see cref="QRCodeEncoder"/>.
    /// <see cref="QRCodeEncoder"/> 的参数。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct QRCodeEncoderParams : IEquatable<QRCodeEncoderParams>
    {
        /// <summary>
        /// Initializes encoder parameters.
        /// 初始化编码器参数。
        /// </summary>
        public QRCodeEncoderParams(int version, QRCodeEncoderCorrectionLevel correctionLevel, QRCodeEncoderEncodeMode mode, int structureNumber)
        {
            if (version < 0 || version > 40)
                throw new ArgumentOutOfRangeException(nameof(version), version, "QR code version must be 0 for automatic selection or between 1 and 40.");
            if (!Enum.IsDefined(typeof(QRCodeEncoderCorrectionLevel), correctionLevel))
                throw new ArgumentOutOfRangeException(nameof(correctionLevel), correctionLevel, "QR code correction level must be a defined value.");
            if (!Enum.IsDefined(typeof(QRCodeEncoderEncodeMode), mode))
                throw new ArgumentOutOfRangeException(nameof(mode), mode, "QR code encode mode must be a defined value.");
            if (structureNumber <= 0)
                throw new ArgumentOutOfRangeException(nameof(structureNumber), structureNumber, "Structured append count must be positive.");

            Version = version;
            CorrectionLevel = correctionLevel;
            Mode = mode;
            StructureNumber = structureNumber;
        }

        /// <summary>Gets the optional QR version. 获取可选二维码版本。</summary>
        public int Version { get; }

        /// <summary>Gets the correction level. 获取纠错级别。</summary>
        public QRCodeEncoderCorrectionLevel CorrectionLevel { get; }

        /// <summary>Gets the encode mode. 获取编码模式。</summary>
        public QRCodeEncoderEncodeMode Mode { get; }

        /// <summary>Gets the structured append count. 获取结构化追加数量。</summary>
        public int StructureNumber { get; }

        /// <summary>
        /// Determines whether two values are equal.
        /// 判断两个值是否相等。
        /// </summary>
        public static bool operator ==(QRCodeEncoderParams left, QRCodeEncoderParams right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two values are not equal.
        /// 判断两个值是否不相等。
        /// </summary>
        public static bool operator !=(QRCodeEncoderParams left, QRCodeEncoderParams right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Gets default OpenCV QR encoder parameters.
        /// 获取 OpenCV 默认二维码编码器参数。
        /// </summary>
        public static QRCodeEncoderParams Default
        {
            get
            {
                NativeException.ThrowIfError(NativeMethods.QRCodeEncoderDefaultParams(out NativeMethods.QRCodeEncoderParamsNative native));
                return FromNative(native);
            }
        }

        internal NativeMethods.QRCodeEncoderParamsNative ToNative()
        {
            return new NativeMethods.QRCodeEncoderParamsNative
            {
                Version = Version,
                CorrectionLevel = (int)CorrectionLevel,
                Mode = (int)Mode,
                StructureNumber = StructureNumber
            };
        }

        internal static QRCodeEncoderParams FromNative(NativeMethods.QRCodeEncoderParamsNative native)
        {
            return new QRCodeEncoderParams(
                native.Version,
                (QRCodeEncoderCorrectionLevel)native.CorrectionLevel,
                (QRCodeEncoderEncodeMode)native.Mode,
                native.StructureNumber);
        }

        /// <inheritdoc/>
        public bool Equals(QRCodeEncoderParams other)
        {
            return Version == other.Version &&
                CorrectionLevel == other.CorrectionLevel &&
                Mode == other.Mode &&
                StructureNumber == other.StructureNumber;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is QRCodeEncoderParams other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hash = Version;
            hash = (hash * 397) ^ (int)CorrectionLevel;
            hash = (hash * 397) ^ (int)Mode;
            hash = (hash * 397) ^ StructureNumber;
            return hash;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{Version=" + Version + ",CorrectionLevel=" + CorrectionLevel + ",Mode=" + Mode + ",StructureNumber=" + StructureNumber + "}";
        }
    }
}

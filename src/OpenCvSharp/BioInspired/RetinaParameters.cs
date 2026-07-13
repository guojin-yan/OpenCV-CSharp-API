using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.BioInspired
{
    /// <summary>
    /// Combined Retina parameter groups.
    /// Retina 参数组集合。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct RetinaParameters : IEquatable<RetinaParameters>
    {
        /// <summary>Initializes combined Retina parameters. 初始化 Retina 参数组集合。</summary>
        public RetinaParameters(RetinaParvoParameters parvo, RetinaMagnoParameters magno)
        {
            Parvo = parvo;
            Magno = magno;
        }

        /// <summary>Gets parvo channel parameters. 获取 parvo 通道参数。</summary>
        public RetinaParvoParameters Parvo { get; }

        /// <summary>Gets magno channel parameters. 获取 magno 通道参数。</summary>
        public RetinaMagnoParameters Magno { get; }

        /// <summary>Determines whether two parameter values are equal. 判断两个参数值是否相等。</summary>
        public static bool operator ==(RetinaParameters left, RetinaParameters right)
        {
            return left.Equals(right);
        }

        /// <summary>Determines whether two parameter values are different. 判断两个参数值是否不同。</summary>
        public static bool operator !=(RetinaParameters left, RetinaParameters right)
        {
            return !left.Equals(right);
        }

        /// <summary>Creates default parameters. 创建默认参数。</summary>
        public static RetinaParameters Default
        {
            get { return new RetinaParameters(RetinaParvoParameters.Default, RetinaMagnoParameters.Default); }
        }

        /// <inheritdoc />
        public bool Equals(RetinaParameters other)
        {
            return Parvo.Equals(other.Parvo) && Magno.Equals(other.Magno);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is RetinaParameters other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (Parvo.GetHashCode() * 397) ^ Magno.GetHashCode();
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                "RetinaParameters(Parvo={0}, Magno={1})",
                Parvo,
                Magno);
        }
    }
}

using System;
using System.Globalization;
using System.Runtime.InteropServices;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.ObjDetect
{
    /// <summary>
    /// Parameters used by ArUco marker refinement.
    /// ArUco marker 细化使用的参数。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct ArucoRefineParameters : IEquatable<ArucoRefineParameters>
    {
        /// <summary>
        /// Initializes refine parameters.
        /// 初始化细化参数。
        /// </summary>
        public ArucoRefineParameters(float minRepDistance, float errorCorrectionRate, bool checkAllOrders)
        {
            MinRepDistance = minRepDistance;
            ErrorCorrectionRate = errorCorrectionRate;
            CheckAllOrders = checkAllOrders;
        }

        /// <summary>Gets minimum reprojection distance. 获取最小重投影距离。</summary>
        public float MinRepDistance { get; }

        /// <summary>Gets allowed error correction rate. 获取允许的纠错比例。</summary>
        public float ErrorCorrectionRate { get; }

        /// <summary>Gets whether all corner orders are checked. 获取是否检查所有角点顺序。</summary>
        public bool CheckAllOrders { get; }

        /// <summary>
        /// Determines whether two values are equal.
        /// 判断两个值是否相等。
        /// </summary>
        public static bool operator ==(ArucoRefineParameters left, ArucoRefineParameters right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two values are not equal.
        /// 判断两个值是否不相等。
        /// </summary>
        public static bool operator !=(ArucoRefineParameters left, ArucoRefineParameters right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Gets OpenCV default refine parameters.
        /// 获取 OpenCV 默认细化参数。
        /// </summary>
        public static ArucoRefineParameters Default
        {
            get
            {
                NativeException.ThrowIfError(NativeMethods.ArucoRefineDefaultParams(out NativeMethods.ArucoRefineParamsNative native));
                return FromNative(native);
            }
        }

        internal NativeMethods.ArucoRefineParamsNative ToNative()
        {
            return new NativeMethods.ArucoRefineParamsNative
            {
                MinRepDistance = MinRepDistance,
                ErrorCorrectionRate = ErrorCorrectionRate,
                CheckAllOrders = CheckAllOrders ? 1 : 0
            };
        }

        internal static ArucoRefineParameters FromNative(NativeMethods.ArucoRefineParamsNative native)
        {
            return new ArucoRefineParameters(native.MinRepDistance, native.ErrorCorrectionRate, native.CheckAllOrders != 0);
        }

        /// <inheritdoc/>
        public bool Equals(ArucoRefineParameters other)
        {
            return MinRepDistance.Equals(other.MinRepDistance) &&
                ErrorCorrectionRate.Equals(other.ErrorCorrectionRate) &&
                CheckAllOrders == other.CheckAllOrders;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is ArucoRefineParameters other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hash = MinRepDistance.GetHashCode();
            hash = (hash * 397) ^ ErrorCorrectionRate.GetHashCode();
            hash = (hash * 397) ^ CheckAllOrders.GetHashCode();
            return hash;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{{MinRepDistance={0},ErrorCorrectionRate={1},CheckAllOrders={2}}}",
                MinRepDistance,
                ErrorCorrectionRate,
                CheckAllOrders);
        }
    }
}

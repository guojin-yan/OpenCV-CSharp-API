using System;
using System.Globalization;

namespace JYPPX.OpenCvSharp.SurfaceMatching
{
    /// <summary>
    /// Result returned by ICP model-to-scene registration.
    /// ICP model-to-scene 配准返回结果。
    /// </summary>
    public sealed class IcpRegistrationResult
    {
        /// <summary>Initializes the result. 初始化结果。</summary>
        public IcpRegistrationResult(int resultCode, double residual, double[] pose)
        {
            ValidatePose(pose, nameof(pose));

            ResultCode = resultCode;
            Residual = residual;
            this.pose = Clone(pose);
        }

        private readonly double[] pose;

        /// <summary>Gets the native ICP result code. 获取 native ICP 结果码。</summary>
        public int ResultCode { get; }

        /// <summary>Gets the residual registration error. 获取配准残差。</summary>
        public double Residual { get; }

        /// <summary>Gets a row-major 4x4 pose matrix. 获取行优先 4x4 pose 矩阵。</summary>
        public double[] Pose
        {
            get { return Clone(pose); }
        }

        /// <summary>Gets the number of pose matrix values. 获取 pose 矩阵值数量。</summary>
        public int PoseLength
        {
            get { return pose.Length; }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{{ResultCode={0},Residual={1},PoseLength={2}}}",
                ResultCode,
                Residual,
                PoseLength);
        }

        private static double[] Clone(double[] values)
        {
            var clone = new double[values.Length];
            Array.Copy(values, clone, clone.Length);
            return clone;
        }

        private static void ValidatePose(double[] values, string parameterName)
        {
            if (values == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            if (values.Length != 16)
            {
                throw new ArgumentException("Array length must be 16.", parameterName);
            }
        }
    }
}

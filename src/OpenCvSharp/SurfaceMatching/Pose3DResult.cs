using System;
using System.Globalization;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.SurfaceMatching
{
    /// <summary>
    /// Flat managed summary of OpenCV <c>Pose3D</c>.
    /// OpenCV <c>Pose3D</c> 的 managed 扁平摘要。
    /// </summary>
    public sealed class Pose3DResult
    {
        /// <summary>
        /// Initializes a managed pose result.
        /// 初始化 managed pose 结果。
        /// </summary>
        public Pose3DResult(
            double alpha,
            double residual,
            ulong modelIndex,
            ulong numVotes,
            double angle,
            double[] translation,
            double[] quaternion,
            double[] pose)
        {
            ValidateLength(translation, nameof(translation), 3);
            ValidateLength(quaternion, nameof(quaternion), 4);
            ValidateLength(pose, nameof(pose), 16);

            Alpha = alpha;
            Residual = residual;
            ModelIndex = modelIndex;
            NumVotes = numVotes;
            Angle = angle;
            this.translation = Clone(translation);
            this.quaternion = Clone(quaternion);
            this.pose = Clone(pose);
        }

        internal Pose3DResult(NativeSurfaceMatchingPose3DResult value)
            : this(
                value.Alpha,
                value.Residual,
                value.ModelIndex,
                value.NumVotes,
                value.Angle,
                new[] { value.T0, value.T1, value.T2 },
                new[] { value.Q0, value.Q1, value.Q2, value.Q3 },
                new[]
                {
                    value.Pose00, value.Pose01, value.Pose02, value.Pose03,
                    value.Pose10, value.Pose11, value.Pose12, value.Pose13,
                    value.Pose20, value.Pose21, value.Pose22, value.Pose23,
                    value.Pose30, value.Pose31, value.Pose32, value.Pose33
                })
        {
        }

        private readonly double[] translation;
        private readonly double[] quaternion;
        private readonly double[] pose;

        /// <summary>Gets the pose alpha value. 获取 pose alpha 值。</summary>
        public double Alpha { get; }

        /// <summary>Gets the pose residual. 获取 pose 残差。</summary>
        public double Residual { get; }

        /// <summary>Gets the model index. 获取模型索引。</summary>
        public ulong ModelIndex { get; }

        /// <summary>Gets the vote count. 获取投票数。</summary>
        public ulong NumVotes { get; }

        /// <summary>Gets the pose angle. 获取 pose 角度。</summary>
        public double Angle { get; }

        /// <summary>Gets the translation vector as three doubles. 获取三元素平移向量。</summary>
        public double[] Translation
        {
            get { return Clone(translation); }
        }

        /// <summary>Gets the number of translation vector values. 获取平移向量值数量。</summary>
        public int TranslationLength
        {
            get { return translation.Length; }
        }

        /// <summary>Gets the quaternion as four doubles. 获取四元素四元数。</summary>
        public double[] Quaternion
        {
            get { return Clone(quaternion); }
        }

        /// <summary>Gets the number of quaternion values. 获取四元数值数量。</summary>
        public int QuaternionLength
        {
            get { return quaternion.Length; }
        }

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
                "{{Alpha={0},Residual={1},ModelIndex={2},NumVotes={3},Angle={4},TranslationLength={5},QuaternionLength={6},PoseLength={7}}}",
                Alpha,
                Residual,
                ModelIndex,
                NumVotes,
                Angle,
                TranslationLength,
                QuaternionLength,
                PoseLength);
        }

        private static void ValidateLength(double[] values, string parameterName, int expectedLength)
        {
            if (values == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            if (values.Length != expectedLength)
            {
                throw new ArgumentException("Array length must be " + expectedLength + ".", parameterName);
            }
        }

        private static double[] Clone(double[] values)
        {
            var clone = new double[values.Length];
            Array.Copy(values, clone, clone.Length);
            return clone;
        }
    }
}

using System;
using OpenCvSharp.Core;

namespace OpenCvSharp.Calib3D
{
    /// <summary>
    /// Owned transform-composition result with all eight Jacobian matrices.
    /// 包含全部八个 Jacobian 矩阵且由调用方持有的变换组合结果。
    /// </summary>
    public readonly struct ComposeRTDerivativesResult
    {
        /// <summary>
        /// Initializes a transform-composition derivative result.
        /// 初始化变换组合导数结果。
        /// </summary>
        public ComposeRTDerivativesResult(
            Mat rvec3,
            Mat tvec3,
            Mat dr3dr1,
            Mat dr3dt1,
            Mat dr3dr2,
            Mat dr3dt2,
            Mat dt3dr1,
            Mat dt3dt1,
            Mat dt3dr2,
            Mat dt3dt2)
        {
            Rvec3 = rvec3 ?? throw new ArgumentNullException(nameof(rvec3));
            Tvec3 = tvec3 ?? throw new ArgumentNullException(nameof(tvec3));
            Dr3Dr1 = dr3dr1 ?? throw new ArgumentNullException(nameof(dr3dr1));
            Dr3Dt1 = dr3dt1 ?? throw new ArgumentNullException(nameof(dr3dt1));
            Dr3Dr2 = dr3dr2 ?? throw new ArgumentNullException(nameof(dr3dr2));
            Dr3Dt2 = dr3dt2 ?? throw new ArgumentNullException(nameof(dr3dt2));
            Dt3Dr1 = dt3dr1 ?? throw new ArgumentNullException(nameof(dt3dr1));
            Dt3Dt1 = dt3dt1 ?? throw new ArgumentNullException(nameof(dt3dt1));
            Dt3Dr2 = dt3dr2 ?? throw new ArgumentNullException(nameof(dt3dr2));
            Dt3Dt2 = dt3dt2 ?? throw new ArgumentNullException(nameof(dt3dt2));

            ValidateVector(Rvec3, nameof(rvec3));
            ValidateVector(Tvec3, nameof(tvec3));
            if (Rvec3.Rows != Tvec3.Rows || Rvec3.Cols != Tvec3.Cols)
            {
                throw new ArgumentException(
                    "The composed rotation and translation vectors must have the same shape.",
                    nameof(tvec3));
            }
            if (Rvec3.Type != Tvec3.Type)
            {
                throw new ArgumentException(
                    "The composed rotation and translation vectors must have the same type.",
                    nameof(tvec3));
            }

            int type = Rvec3.Type;
            ValidateDerivative(Dr3Dr1, type, nameof(dr3dr1));
            ValidateDerivative(Dr3Dt1, type, nameof(dr3dt1));
            ValidateDerivative(Dr3Dr2, type, nameof(dr3dr2));
            ValidateDerivative(Dr3Dt2, type, nameof(dr3dt2));
            ValidateDerivative(Dt3Dr1, type, nameof(dt3dr1));
            ValidateDerivative(Dt3Dt1, type, nameof(dt3dt1));
            ValidateDerivative(Dt3Dr2, type, nameof(dt3dr2));
            ValidateDerivative(Dt3Dt2, type, nameof(dt3dt2));
        }

        /// <summary>Gets the composed rotation vector. 获取组合后的旋转向量。</summary>
        public Mat Rvec3 { get; }

        /// <summary>Gets the composed translation vector. 获取组合后的平移向量。</summary>
        public Mat Tvec3 { get; }

        /// <summary>Gets d(rvec3)/d(rvec1). 获取 d(rvec3)/d(rvec1)。</summary>
        public Mat Dr3Dr1 { get; }

        /// <summary>Gets d(rvec3)/d(tvec1). 获取 d(rvec3)/d(tvec1)。</summary>
        public Mat Dr3Dt1 { get; }

        /// <summary>Gets d(rvec3)/d(rvec2). 获取 d(rvec3)/d(rvec2)。</summary>
        public Mat Dr3Dr2 { get; }

        /// <summary>Gets d(rvec3)/d(tvec2). 获取 d(rvec3)/d(tvec2)。</summary>
        public Mat Dr3Dt2 { get; }

        /// <summary>Gets d(tvec3)/d(rvec1). 获取 d(tvec3)/d(rvec1)。</summary>
        public Mat Dt3Dr1 { get; }

        /// <summary>Gets d(tvec3)/d(tvec1). 获取 d(tvec3)/d(tvec1)。</summary>
        public Mat Dt3Dt1 { get; }

        /// <summary>Gets d(tvec3)/d(rvec2). 获取 d(tvec3)/d(rvec2)。</summary>
        public Mat Dt3Dr2 { get; }

        /// <summary>Gets d(tvec3)/d(tvec2). 获取 d(tvec3)/d(tvec2)。</summary>
        public Mat Dt3Dt2 { get; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{Rvec3=" + Rvec3.Rows + "x" + Rvec3.Cols
                + ",Tvec3=" + Tvec3.Rows + "x" + Tvec3.Cols
                + ",Jacobians=8x3x3}";
        }

        private static void ValidateVector(Mat vector, string parameterName)
        {
            if (!((vector.Rows == 1 && vector.Cols == 3) ||
                (vector.Rows == 3 && vector.Cols == 1)))
            {
                throw new ArgumentException(
                    "Composed vectors must be 1 x 3 or 3 x 1.",
                    parameterName);
            }
            if (vector.Type != MatType.CV_32FC1 &&
                vector.Type != MatType.CV_64FC1)
            {
                throw new ArgumentException(
                    "Composed vectors must be CV_32FC1 or CV_64FC1.",
                    parameterName);
            }
        }

        private static void ValidateDerivative(Mat derivative, int type, string parameterName)
        {
            if (derivative.Rows != 3 || derivative.Cols != 3)
            {
                throw new ArgumentException(
                    "ComposeRT derivative matrices must be 3 x 3.",
                    parameterName);
            }
            if (derivative.Type != type)
            {
                throw new ArgumentException(
                    "ComposeRT derivative matrices must match the composed-vector type.",
                    parameterName);
            }
        }
    }
}

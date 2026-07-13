using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.ObjDetect
{
    /// <summary>
    /// Parameters for ChArUco corner interpolation.
    /// ChArUco 角点插值参数。
    /// </summary>
    public sealed class CharucoParameters
    {
        /// <summary>
        /// Initializes parameters with OpenCV default values.
        /// 使用 OpenCV 默认值初始化参数。
        /// </summary>
        public CharucoParameters()
        {
            NativeException.ThrowIfError(NativeMethods.ArucoCharucoDefaultParams(out NativeMethods.ArucoCharucoParamsNative native));
            CopyFromNative(native);
        }

        /// <summary>
        /// Initializes parameters with scalar values.
        /// 使用标量值初始化参数。
        /// </summary>
        public CharucoParameters(int minMarkers, bool tryRefineMarkers, bool checkMarkers)
        {
            MinMarkers = minMarkers;
            TryRefineMarkers = tryRefineMarkers;
            CheckMarkers = checkMarkers;
        }

        /// <summary>
        /// Initializes parameters by copying another instance.
        /// 通过复制另一个实例初始化参数。
        /// </summary>
        /// <param name="other">The parameters to copy. 要复制的参数。</param>
        public CharucoParameters(CharucoParameters other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            MinMarkers = other.MinMarkers;
            TryRefineMarkers = other.TryRefineMarkers;
            CheckMarkers = other.CheckMarkers;
            CameraMatrix = other.CameraMatrix;
            DistCoeffs = other.DistCoeffs;
        }

        /// <summary>Gets or sets the optional camera matrix. 获取或设置可选相机矩阵。</summary>
        public Mat? CameraMatrix { get; set; }

        /// <summary>Gets or sets the optional distortion coefficients. 获取或设置可选畸变系数。</summary>
        public Mat? DistCoeffs { get; set; }

        /// <summary>Gets or sets the minimum adjacent marker count. 获取或设置最少相邻 marker 数量。</summary>
        public int MinMarkers { get; set; }

        /// <summary>Gets or sets whether marker refinement is attempted. 获取或设置是否尝试 marker 细化。</summary>
        public bool TryRefineMarkers { get; set; }

        /// <summary>Gets or sets whether marker-board consistency is checked. 获取或设置是否检查 marker 与 board 的一致性。</summary>
        public bool CheckMarkers { get; set; }

        internal NativeMethods.ArucoCharucoParamsNative ToNative()
        {
            return new NativeMethods.ArucoCharucoParamsNative
            {
                MinMarkers = MinMarkers,
                TryRefineMarkers = TryRefineMarkers ? 1 : 0,
                CheckMarkers = CheckMarkers ? 1 : 0
            };
        }

        /// <summary>
        /// Creates a shallow copy of this parameter object.
        /// 创建此参数对象的浅拷贝。
        /// </summary>
        /// <remarks>
        /// Matrix references are preserved; ownership remains with the caller.
        /// 矩阵引用会被保留；所有权仍由调用方持有。
        /// </remarks>
        public CharucoParameters Clone()
        {
            return new CharucoParameters(this);
        }

        internal static CharucoParameters FromNative(NativeMethods.ArucoCharucoParamsNative native, Mat? cameraMatrix, Mat? distCoeffs)
        {
            var result = new CharucoParameters(native.MinMarkers, native.TryRefineMarkers != 0, native.CheckMarkers != 0)
            {
                CameraMatrix = cameraMatrix,
                DistCoeffs = distCoeffs
            };
            return result;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return
                $"{nameof(CharucoParameters)}(" +
                $"{nameof(MinMarkers)}={MinMarkers}, " +
                $"{nameof(TryRefineMarkers)}={TryRefineMarkers}, " +
                $"{nameof(CheckMarkers)}={CheckMarkers}, " +
                $"{nameof(CameraMatrix)}={FormatMatSize(CameraMatrix)}, " +
                $"{nameof(DistCoeffs)}={FormatMatSize(DistCoeffs)})";
        }

        private void CopyFromNative(NativeMethods.ArucoCharucoParamsNative native)
        {
            MinMarkers = native.MinMarkers;
            TryRefineMarkers = native.TryRefineMarkers != 0;
            CheckMarkers = native.CheckMarkers != 0;
        }

        private static string FormatMatSize(Mat? mat)
        {
            return mat is null ? "<null>" : $"{mat.Rows}x{mat.Cols}";
        }
    }
}

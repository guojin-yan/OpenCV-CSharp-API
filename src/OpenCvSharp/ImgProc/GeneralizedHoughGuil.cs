using System;

namespace OpenCvSharp.ImgProc
{
    /// <summary>Generalized Hough detector for position, scale, and rotation. 检测位置、缩放和旋转的广义霍夫检测器。</summary>
    public sealed class GeneralizedHoughGuil : GeneralizedHough
    {
        internal GeneralizedHoughGuil(IntPtr nativeHandle) : base(nativeHandle) { }

        /// <summary>Gets or sets the feature angle difference. 获取或设置特征角度差。</summary>
        public double Xi { get { return GetDoubleProperty(2); } set { ValidateNonNegative(value, nameof(value)); SetDoubleProperty(2, value); } }

        /// <summary>Gets or sets the feature table level count. 获取或设置特征表层级数。</summary>
        public int Levels { get { return GetIntProperty(3); } set { ValidatePositive(value, nameof(value)); SetIntProperty(3, value); } }

        /// <summary>Gets or sets the angle equality tolerance. 获取或设置角度相等容差。</summary>
        public double AngleEpsilon { get { return GetDoubleProperty(3); } set { ValidateNonNegative(value, nameof(value)); SetDoubleProperty(3, value); } }

        /// <summary>Gets or sets the minimum detected angle. 获取或设置最小检测角度。</summary>
        public double MinAngle { get { return GetDoubleProperty(4); } set { ValidateNonNegative(value, nameof(value)); SetDoubleProperty(4, value); } }

        /// <summary>Gets or sets the maximum detected angle. 获取或设置最大检测角度。</summary>
        public double MaxAngle { get { return GetDoubleProperty(5); } set { ValidateNonNegative(value, nameof(value)); SetDoubleProperty(5, value); } }

        /// <summary>Gets or sets the angle step. 获取或设置角度步长。</summary>
        public double AngleStep { get { return GetDoubleProperty(6); } set { ValidatePositive(value, nameof(value)); SetDoubleProperty(6, value); } }

        /// <summary>Gets or sets the angle vote threshold. 获取或设置角度投票阈值。</summary>
        public int AngleThreshold { get { return GetIntProperty(5); } set { ValidatePositive(value, nameof(value)); SetIntProperty(5, value); } }

        /// <summary>Gets or sets the minimum detected scale. 获取或设置最小检测缩放。</summary>
        public double MinScale { get { return GetDoubleProperty(7); } set { ValidatePositive(value, nameof(value)); SetDoubleProperty(7, value); } }

        /// <summary>Gets or sets the maximum detected scale. 获取或设置最大检测缩放。</summary>
        public double MaxScale { get { return GetDoubleProperty(8); } set { ValidatePositive(value, nameof(value)); SetDoubleProperty(8, value); } }

        /// <summary>Gets or sets the scale step. 获取或设置缩放步长。</summary>
        public double ScaleStep { get { return GetDoubleProperty(9); } set { ValidatePositive(value, nameof(value)); SetDoubleProperty(9, value); } }

        /// <summary>Gets or sets the scale vote threshold. 获取或设置缩放投票阈值。</summary>
        public int ScaleThreshold { get { return GetIntProperty(6); } set { ValidatePositive(value, nameof(value)); SetIntProperty(6, value); } }

        /// <summary>Gets or sets the position vote threshold. 获取或设置位置投票阈值。</summary>
        public int PositionThreshold { get { return GetIntProperty(7); } set { ValidatePositive(value, nameof(value)); SetIntProperty(7, value); } }
    }
}

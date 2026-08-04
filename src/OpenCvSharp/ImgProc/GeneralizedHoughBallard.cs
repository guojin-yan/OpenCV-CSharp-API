using System;

namespace JYPPX.OpenCvSharp.ImgProc
{
    /// <summary>Generalized Hough detector for translation-only matching. 仅检测平移的广义霍夫检测器。</summary>
    public sealed class GeneralizedHoughBallard : GeneralizedHough
    {
        internal GeneralizedHoughBallard(IntPtr nativeHandle) : base(nativeHandle) { }

        /// <summary>Gets or sets the R-table level count. 获取或设置 R-table 层级数。</summary>
        public int Levels
        {
            get { return GetIntProperty(3); }
            set { ValidatePositive(value, nameof(value)); SetIntProperty(3, value); }
        }

        /// <summary>Gets or sets the center vote threshold. 获取或设置中心投票阈值。</summary>
        public int VotesThreshold
        {
            get { return GetIntProperty(4); }
            set { ValidatePositive(value, nameof(value)); SetIntProperty(4, value); }
        }
    }
}

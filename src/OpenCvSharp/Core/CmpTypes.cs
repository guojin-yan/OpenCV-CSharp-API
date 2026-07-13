namespace OpenCvSharp.Core
{
    /// <summary>
    /// Specifies array comparison operations compatible with OpenCV <c>cv::CmpTypes</c>.
    /// 指定与 OpenCV <c>cv::CmpTypes</c> 兼容的数组比较操作。
    /// </summary>
    public enum CmpTypes
    {
        /// <summary>
        /// Checks whether the first value is equal to the second value.
        /// 判断第一个值是否等于第二个值。
        /// </summary>
        EQ = 0,

        /// <summary>
        /// Checks whether the first value is greater than the second value.
        /// 判断第一个值是否大于第二个值。
        /// </summary>
        GT = 1,

        /// <summary>
        /// Checks whether the first value is greater than or equal to the second value.
        /// 判断第一个值是否大于或等于第二个值。
        /// </summary>
        GE = 2,

        /// <summary>
        /// Checks whether the first value is less than the second value.
        /// 判断第一个值是否小于第二个值。
        /// </summary>
        LT = 3,

        /// <summary>
        /// Checks whether the first value is less than or equal to the second value.
        /// 判断第一个值是否小于或等于第二个值。
        /// </summary>
        LE = 4,

        /// <summary>
        /// Checks whether the first value is different from the second value.
        /// 判断第一个值是否不等于第二个值。
        /// </summary>
        NE = 5
    }
}

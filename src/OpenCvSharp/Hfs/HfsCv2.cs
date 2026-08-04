namespace JYPPX.OpenCvSharp.Hfs
{
    /// <summary>
    /// Entry points for OpenCV HFS functions.
    /// OpenCV HFS 函数入口。
    /// </summary>
    public static class HfsCv2
    {
        /// <summary>Creates an HFS segmenter. 创建 HFS 分割器。</summary>
        public static HfsSegment CreateHfsSegment(HfsSegmentParams parameters)
        {
            return HfsSegment.Create(parameters);
        }

        /// <summary>Creates an HFS segmenter with OpenCV default algorithm values for the specified image size. 使用指定图像尺寸和 OpenCV 默认算法值创建 HFS 分割器。</summary>
        public static HfsSegment CreateHfsSegment(int height, int width)
        {
            return HfsSegment.Create(height, width);
        }
    }
}

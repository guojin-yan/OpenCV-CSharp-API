namespace JYPPX.OpenCvSharp.Stitching
{
    /// <summary>
    /// Stitcher operating mode compatible with OpenCV <c>cv::Stitcher::Mode</c>.
    /// 与 OpenCV <c>cv::Stitcher::Mode</c> 兼容的拼接器运行模式。
    /// </summary>
    public enum StitcherMode
    {
        /// <summary>Panorama mode for perspective camera motion. 适用于透视相机运动的全景模式。</summary>
        Panorama = 0,
        /// <summary>Scans mode for affine-like input. 适用于类仿射输入的扫描模式。</summary>
        Scans = 1
    }
}

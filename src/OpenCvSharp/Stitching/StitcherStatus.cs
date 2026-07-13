namespace OpenCvSharp.Stitching
{
    /// <summary>
    /// Status returned by OpenCV <c>cv::Stitcher</c>.
    /// OpenCV <c>cv::Stitcher</c> 返回的状态。
    /// </summary>
    public enum StitcherStatus
    {
        /// <summary>The operation succeeded. 操作成功。</summary>
        OK = 0,
        /// <summary>More images are required. 需要更多图像。</summary>
        ErrorNeedMoreImages = 1,
        /// <summary>Homography estimation failed. 单应估计失败。</summary>
        ErrorHomographyEstimationFailed = 2,
        /// <summary>Camera parameter adjustment failed. 相机参数调整失败。</summary>
        ErrorCameraParametersAdjustFailed = 3
    }
}

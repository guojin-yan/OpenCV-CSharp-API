namespace OpenCvSharp.ObjDetect
{
    /// <summary>
    /// Face feature distance type compatible with OpenCV <c>cv::FaceRecognizerSF::DisType</c>.
    /// 与 OpenCV <c>cv::FaceRecognizerSF::DisType</c> 兼容的人脸特征距离类型。
    /// </summary>
    public enum FaceRecognizerSFDistanceType
    {
        /// <summary>
        /// Cosine distance.
        /// 余弦距离。
        /// </summary>
        Cosine = 0,

        /// <summary>
        /// L2 norm distance.
        /// L2 范数距离。
        /// </summary>
        NormL2 = 1
    }
}

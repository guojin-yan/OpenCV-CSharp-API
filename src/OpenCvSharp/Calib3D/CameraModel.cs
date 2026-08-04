namespace JYPPX.OpenCvSharp.Calib3D
{
    /// <summary>
    /// Camera projection model used by camera-pair registration.
    /// 相机对注册使用的相机投影模型。
    /// </summary>
    public enum CameraModel
    {
        /// <summary>Pinhole camera model. 针孔相机模型。</summary>
        Pinhole = 0,

        /// <summary>Fisheye camera model. 鱼眼相机模型。</summary>
        Fisheye = 1
    }
}

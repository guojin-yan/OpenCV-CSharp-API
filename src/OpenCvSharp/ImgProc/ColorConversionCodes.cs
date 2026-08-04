namespace JYPPX.OpenCvSharp.ImgProc
{
    /// <summary>
    /// Specifies color conversion operation codes compatible with OpenCV <c>cv::ColorConversionCodes</c>.
    /// 指定与 OpenCV <c>cv::ColorConversionCodes</c> 兼容的颜色转换操作代码。
    /// </summary>
    public enum ColorConversionCodes
    {
        /// <summary>
        /// Converts an image from BGR color space to grayscale, equivalent to <c>cv::COLOR_BGR2GRAY</c>.
        /// 将图像从 BGR 色彩空间转换为灰度图，等价于 <c>cv::COLOR_BGR2GRAY</c>。
        /// </summary>
        BGR2GRAY = 6,

        /// <summary>Converts NV12 YUV planes to RGB.</summary>
        YUV2RGB_NV12 = 90,
        /// <summary>Converts NV12 YUV planes to BGR.</summary>
        YUV2BGR_NV12 = 91,
        /// <summary>Converts NV21 YUV planes to RGB.</summary>
        YUV2RGB_NV21 = 92,
        /// <summary>Converts NV21 YUV planes to BGR.</summary>
        YUV2BGR_NV21 = 93,
        /// <summary>Demosaics a Bayer BG pattern to BGR.</summary>
        BayerBG2BGR = 46,
        /// <summary>Demosaics a Bayer GB pattern to BGR.</summary>
        BayerGB2BGR = 47,
        /// <summary>Demosaics a Bayer RG pattern to BGR.</summary>
        BayerRG2BGR = 48,
        /// <summary>Demosaics a Bayer GR pattern to BGR.</summary>
        BayerGR2BGR = 49,
        /// <summary>Demosaics a Bayer BG pattern to grayscale.</summary>
        BayerBG2GRAY = 86,
        /// <summary>Demosaics a Bayer GB pattern to grayscale.</summary>
        BayerGB2GRAY = 87,
        /// <summary>Demosaics a Bayer RG pattern to grayscale.</summary>
        BayerRG2GRAY = 88,
        /// <summary>Demosaics a Bayer GR pattern to grayscale.</summary>
        BayerGR2GRAY = 89,
        /// <summary>Demosaics a Bayer BG pattern with variable-number gradients.</summary>
        BayerBG2BGR_VNG = 62,
        /// <summary>Demosaics a Bayer GB pattern with variable-number gradients.</summary>
        BayerGB2BGR_VNG = 63,
        /// <summary>Demosaics a Bayer RG pattern with variable-number gradients.</summary>
        BayerRG2BGR_VNG = 64,
        /// <summary>Demosaics a Bayer GR pattern with variable-number gradients.</summary>
        BayerGR2BGR_VNG = 65,
        /// <summary>Demosaics a Bayer BG pattern with edge-aware interpolation.</summary>
        BayerBG2BGR_EA = 135,
        /// <summary>Demosaics a Bayer GB pattern with edge-aware interpolation.</summary>
        BayerGB2BGR_EA = 136,
        /// <summary>Demosaics a Bayer RG pattern with edge-aware interpolation.</summary>
        BayerRG2BGR_EA = 137,
        /// <summary>Demosaics a Bayer GR pattern with edge-aware interpolation.</summary>
        BayerGR2BGR_EA = 138
    }
}

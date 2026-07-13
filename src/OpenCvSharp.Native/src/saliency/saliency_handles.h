#pragma once

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SALIENCY)
#include <opencv2/saliency.hpp>
#include <vector>
#endif

struct jyppx_ocv_saliency_saliency
{
    virtual ~jyppx_ocv_saliency_saliency() = default;

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SALIENCY)
    cv::Ptr<cv::saliency::Saliency> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_saliency_static : jyppx_ocv_saliency_saliency
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SALIENCY)
    cv::Ptr<cv::saliency::StaticSaliency> static_value;
#else
    int placeholder2;
#endif
};

struct jyppx_ocv_saliency_spectral_residual : jyppx_ocv_saliency_static
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SALIENCY)
    cv::Ptr<cv::saliency::StaticSaliencySpectralResidual> concrete;
#else
    int placeholder3;
#endif
};

struct jyppx_ocv_saliency_fine_grained : jyppx_ocv_saliency_static
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SALIENCY)
    cv::Ptr<cv::saliency::StaticSaliencyFineGrained> concrete;
#else
    int placeholder3;
#endif
};

struct jyppx_ocv_saliency_motion_bin_wang : jyppx_ocv_saliency_saliency
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SALIENCY)
    cv::Ptr<cv::saliency::MotionSaliencyBinWangApr2014> concrete;
#else
    int placeholder2;
#endif
};

struct jyppx_ocv_saliency_objectness_bing : jyppx_ocv_saliency_saliency
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SALIENCY)
    cv::Ptr<cv::saliency::ObjectnessBING> concrete;
    std::vector<cv::Vec4i> last_boxes;
    std::vector<float> last_values;
#else
    int placeholder2;
#endif
};

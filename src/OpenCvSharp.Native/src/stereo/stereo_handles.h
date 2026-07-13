#pragma once

#if defined(OPENCV_CSHARP_HAS_OPENCV)
#include <opencv2/stereo.hpp>
#endif

struct jyppx_ocv_stereo_matcher
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    cv::Ptr<cv::StereoMatcher> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_stereo_bm
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    cv::Ptr<cv::StereoBM> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_stereo_sgbm
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    cv::Ptr<cv::StereoSGBM> value;
#else
    int placeholder;
#endif
};

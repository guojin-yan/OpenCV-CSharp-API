#pragma once

#if defined(OPENCV_CSHARP_HAS_OPENCV)
#include <opencv2/photo.hpp>
#endif

struct jyppx_ocv_tonemap
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    cv::Ptr<cv::Tonemap> value;
#else
    int placeholder;
#endif
};

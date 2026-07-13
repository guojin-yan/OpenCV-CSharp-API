#pragma once

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_QUALITY)
#include <opencv2/quality.hpp>
#endif

struct jyppx_ocv_quality
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_QUALITY)
    cv::Ptr<cv::quality::QualityBase> value;
#else
    int placeholder;
#endif
};

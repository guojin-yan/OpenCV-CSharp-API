#pragma once

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_RAPID)
#include <opencv2/rapid.hpp>
#endif

struct jyppx_ocv_rapid_tracker
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_RAPID)
    cv::Ptr<cv::rapid::Tracker> value;
#else
    int placeholder;
#endif
};

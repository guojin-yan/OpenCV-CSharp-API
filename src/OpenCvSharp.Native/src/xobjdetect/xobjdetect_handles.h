#pragma once

#if defined(OPENCV_CSHARP_HAS_OPENCV_XOBJDETECT)
#include <opencv2/xobjdetect.hpp>
#endif

struct jyppx_ocv_cascade_classifier
{
#if defined(OPENCV_CSHARP_HAS_OPENCV_XOBJDETECT)
    cv::CascadeClassifier value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_hog_descriptor
{
#if defined(OPENCV_CSHARP_HAS_OPENCV_XOBJDETECT)
    cv::HOGDescriptor value;
#else
    int placeholder;
#endif
};

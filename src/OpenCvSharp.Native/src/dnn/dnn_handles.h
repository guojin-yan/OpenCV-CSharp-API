#pragma once

#if defined(OPENCV_CSHARP_HAS_OPENCV)
#include <opencv2/dnn.hpp>
#endif

struct jyppx_ocv_dnn_net
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    cv::dnn::Net value;
#else
    int placeholder;
#endif
};

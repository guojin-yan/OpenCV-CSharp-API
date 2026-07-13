#pragma once

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XPHOTO)
#include <opencv2/xphoto.hpp>
#endif

struct jyppx_ocv_xphoto_white_balancer
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XPHOTO)
    cv::Ptr<cv::xphoto::WhiteBalancer> value;
#else
    int placeholder;
#endif
};

#pragma once

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_PTCLOUD)
#include <opencv2/ptcloud.hpp>
#endif

struct jyppx_ocv_rgbd_normals
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_PTCLOUD)
    cv::Ptr<cv::RgbdNormals> value;
#else
    int placeholder;
#endif
};

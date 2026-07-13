#pragma once

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_IMG_HASH)
#include <opencv2/img_hash.hpp>
#endif

struct jyppx_ocv_img_hash
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_IMG_HASH)
    cv::Ptr<cv::img_hash::ImgHashBase> value;
    int kind;
#else
    int placeholder;
#endif
};

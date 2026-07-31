#pragma once

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
#include <opencv2/stitching.hpp>
#endif

struct jyppx_ocv_stitcher
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
    cv::Ptr<cv::Stitcher> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_stitching_exposure_compensator
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
    cv::Ptr<cv::detail::ExposureCompensator> value;
#else
    int placeholder;
#endif
};

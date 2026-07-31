#pragma once

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
#include <opencv2/stitching.hpp>
#include <opencv2/stitching/warpers.hpp>
#include <memory>
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

struct jyppx_ocv_stitching_py_rotation_warper
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
    std::unique_ptr<cv::PyRotationWarper> value;
    bool configured = false;
#else
    int placeholder;
#endif
};

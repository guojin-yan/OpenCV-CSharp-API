#pragma once

#include "open_cv_sharp/core/mat.h"

#if defined(OPENCV_CSHARP_HAS_OPENCV)
#include <opencv2/core.hpp>
#endif

struct jyppx_ocv_mat
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    cv::Mat value;
#else
    int placeholder;
#endif
};

#if defined(OPENCV_CSHARP_HAS_OPENCV)
namespace opencv_csharp_native
{
    cv::Mat& mat_value(jyppx_ocv_mat* mat) noexcept;
    const cv::Mat& mat_value(const jyppx_ocv_mat* mat) noexcept;
}

#endif

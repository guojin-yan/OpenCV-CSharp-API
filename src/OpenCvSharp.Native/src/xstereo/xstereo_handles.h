#pragma once

#include "open_cv_sharp/xstereo/xstereo.h"

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XSTEREO)
#include <opencv2/xstereo.hpp>
#endif

struct jyppx_ocv_xstereo_binary_bm
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XSTEREO)
    cv::Ptr<cv::stereo::StereoBinaryBM> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_xstereo_binary_sgbm
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XSTEREO)
    cv::Ptr<cv::stereo::StereoBinarySGBM> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_xstereo_quasi_dense_stereo
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XSTEREO)
    cv::Ptr<cv::stereo::QuasiDenseStereo> value;
#else
    int placeholder;
#endif
};

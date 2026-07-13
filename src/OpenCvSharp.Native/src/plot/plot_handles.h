#pragma once

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_PLOT)
#include <opencv2/plot.hpp>
#endif

struct jyppx_ocv_plot_2d
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_PLOT)
    cv::Ptr<cv::plot::Plot2d> value;
#else
    int placeholder;
#endif
};

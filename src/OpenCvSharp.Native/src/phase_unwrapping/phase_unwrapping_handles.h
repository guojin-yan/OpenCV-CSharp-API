#pragma once

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_PHASE_UNWRAPPING)
#include <opencv2/phase_unwrapping.hpp>
#include <opencv2/phase_unwrapping/histogramphaseunwrapping.hpp>
#endif

struct jyppx_ocv_phase_unwrapping
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_PHASE_UNWRAPPING)
    cv::Ptr<cv::phase_unwrapping::PhaseUnwrapping> value;
    int kind;
#else
    int placeholder;
#endif
};

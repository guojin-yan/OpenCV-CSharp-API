#pragma once

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STRUCTURED_LIGHT)
#include <opencv2/structured_light.hpp>
#include <opencv2/structured_light/graycodepattern.hpp>
#include <opencv2/structured_light/sinusoidalpattern.hpp>
#endif

struct jyppx_ocv_structured_light_pattern
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STRUCTURED_LIGHT)
    cv::Ptr<cv::structured_light::StructuredLightPattern> value;
    int kind;
#else
    int placeholder;
#endif
};

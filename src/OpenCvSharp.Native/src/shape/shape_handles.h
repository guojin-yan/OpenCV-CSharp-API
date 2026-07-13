#pragma once

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SHAPE)
#include <opencv2/shape.hpp>
#endif

struct jyppx_ocv_shape_histogram_cost_extractor
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SHAPE)
    cv::Ptr<cv::HistogramCostExtractor> value;
    int kind;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_shape_distance_extractor
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SHAPE)
    cv::Ptr<cv::ShapeDistanceExtractor> value;
    int kind;
#else
    int placeholder;
#endif
};

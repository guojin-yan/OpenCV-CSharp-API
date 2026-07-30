#pragma once

#if defined(OPENCV_CSHARP_HAS_OPENCV)
#include <opencv2/dnn.hpp>
#endif

struct jyppx_ocv_dnn_net
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    cv::dnn::Net value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_dnn_layer
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    cv::Ptr<cv::dnn::Layer> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_dnn_mat_groups
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    std::vector<std::vector<cv::Mat>> values;
#else
    int placeholder;
#endif
};

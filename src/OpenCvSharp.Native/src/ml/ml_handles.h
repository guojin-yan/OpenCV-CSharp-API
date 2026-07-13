#pragma once

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
#include <opencv2/ml.hpp>
#endif

struct jyppx_ocv_ml_train_data
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
    cv::Ptr<cv::ml::TrainData> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_ml_param_grid
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
    cv::ml::ParamGrid value;
#else
    double min_val;
    double max_val;
    double log_step;
#endif
};

struct jyppx_ocv_ml_model
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
    cv::Ptr<cv::ml::StatModel> value;
    int kind;
#else
    int placeholder;
#endif
};

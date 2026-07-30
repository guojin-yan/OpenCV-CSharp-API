#pragma once

#if defined(OPENCV_CSHARP_HAS_OPENCV)
#include <opencv2/video.hpp>
#endif

struct jyppx_ocv_kalman_filter
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    cv::KalmanFilter value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_background_subtractor
{
    virtual ~jyppx_ocv_background_subtractor() = default;

#if defined(OPENCV_CSHARP_HAS_OPENCV)
    cv::Ptr<cv::BackgroundSubtractor> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_background_subtractor_mog2 : jyppx_ocv_background_subtractor
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    cv::Ptr<cv::BackgroundSubtractorMOG2> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_background_subtractor_knn : jyppx_ocv_background_subtractor
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    cv::Ptr<cv::BackgroundSubtractorKNN> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_dense_optical_flow
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    cv::Ptr<cv::DenseOpticalFlow> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_sparse_optical_flow
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    cv::Ptr<cv::SparseOpticalFlow> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_video_tracker
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    cv::Ptr<cv::Tracker> value;
    bool initialized = false;
#else
    int placeholder;
#endif
};

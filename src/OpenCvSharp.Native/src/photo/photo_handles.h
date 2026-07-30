#pragma once

#if defined(OPENCV_CSHARP_HAS_OPENCV)
#include <memory>
#include <opencv2/photo.hpp>
#endif

struct jyppx_ocv_tonemap
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    cv::Ptr<cv::Tonemap> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_align_mtb
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    cv::Ptr<cv::AlignMTB> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_calibrate_crf
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    cv::Ptr<cv::CalibrateCRF> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_merge_exposures
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    cv::Ptr<cv::MergeExposures> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_color_correction_model
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    std::unique_ptr<cv::ccm::ColorCorrectionModel> value;
    int sample_count = 0;
    bool ready = false;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_intelligent_scissors_mb
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    std::unique_ptr<cv::segmentation::IntelligentScissorsMB> value;
    int width = 0;
    int height = 0;
    bool features_applied = false;
    bool map_built = false;
#else
    int placeholder;
#endif
};

#pragma once

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BGSEGM)
#include <opencv2/bgsegm.hpp>
#endif

struct jyppx_ocv_bgsegm_background_subtractor
{
    virtual ~jyppx_ocv_bgsegm_background_subtractor() = default;

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BGSEGM)
    cv::Ptr<cv::BackgroundSubtractor> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_bgsegm_background_subtractor_mog : jyppx_ocv_bgsegm_background_subtractor
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BGSEGM)
    cv::Ptr<cv::bgsegm::BackgroundSubtractorMOG> concrete;
#else
    int placeholder2;
#endif
};

struct jyppx_ocv_bgsegm_background_subtractor_gmg : jyppx_ocv_bgsegm_background_subtractor
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BGSEGM)
    cv::Ptr<cv::bgsegm::BackgroundSubtractorGMG> concrete;
#else
    int placeholder2;
#endif
};

struct jyppx_ocv_bgsegm_background_subtractor_cnt : jyppx_ocv_bgsegm_background_subtractor
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BGSEGM)
    cv::Ptr<cv::bgsegm::BackgroundSubtractorCNT> concrete;
#else
    int placeholder2;
#endif
};

struct jyppx_ocv_bgsegm_synthetic_sequence_generator
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BGSEGM)
    cv::Ptr<cv::bgsegm::SyntheticSequenceGenerator> value;
#else
    int placeholder;
#endif
};

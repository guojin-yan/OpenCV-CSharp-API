#pragma once

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SURFACE_MATCHING)
#include <opencv2/surface_matching/icp.hpp>
#include <opencv2/surface_matching/ppf_match_3d.hpp>
#endif

struct jyppx_ocv_surface_matching_icp
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SURFACE_MATCHING)
    cv::ppf_match_3d::ICP value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_surface_matching_ppf_3d_detector
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SURFACE_MATCHING)
    cv::Ptr<cv::ppf_match_3d::PPF3DDetector> value;
#else
    int placeholder;
#endif
};

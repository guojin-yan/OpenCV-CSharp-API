#pragma once

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
#include <opencv2/ximgproc.hpp>
#include <opencv2/ximgproc/deriche_filter.hpp>
#include <opencv2/ximgproc/disparity_filter.hpp>
#include <opencv2/ximgproc/edge_drawing.hpp>
#include <opencv2/ximgproc/edgeboxes.hpp>
#include <opencv2/ximgproc/estimated_covariance.hpp>
#include <opencv2/ximgproc/fourier_descriptors.hpp>
#include <opencv2/ximgproc/paillou_filter.hpp>
#include <opencv2/ximgproc/ridgefilter.hpp>
#include <opencv2/ximgproc/run_length_morphology.hpp>
#include <opencv2/ximgproc/scansegment.hpp>
#include <opencv2/ximgproc/segmentation.hpp>
#include <opencv2/ximgproc/sparse_match_interpolator.hpp>
#endif

struct jyppx_ocv_ximgproc_guided_filter
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
    cv::Ptr<cv::ximgproc::GuidedFilter> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_ximgproc_fast_global_smoother_filter
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
    cv::Ptr<cv::ximgproc::FastGlobalSmootherFilter> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_ximgproc_superpixel_slic
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
    cv::Ptr<cv::ximgproc::SuperpixelSLIC> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_ximgproc_superpixel_seeds
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
    cv::Ptr<cv::ximgproc::SuperpixelSEEDS> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_ximgproc_superpixel_lsc
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
    cv::Ptr<cv::ximgproc::SuperpixelLSC> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_ximgproc_fast_line_detector
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
    cv::Ptr<cv::ximgproc::FastLineDetector> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_ximgproc_disparity_filter
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
    cv::Ptr<cv::ximgproc::DisparityFilter> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_ximgproc_disparity_wls_filter
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
    cv::Ptr<cv::ximgproc::DisparityWLSFilter> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_ximgproc_fast_bilateral_solver_filter
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
    cv::Ptr<cv::ximgproc::FastBilateralSolverFilter> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_ximgproc_sparse_match_interpolator
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
    cv::Ptr<cv::ximgproc::SparseMatchInterpolator> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_ximgproc_edge_aware_interpolator
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
    cv::Ptr<cv::ximgproc::EdgeAwareInterpolator> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_ximgproc_ric_interpolator
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
    cv::Ptr<cv::ximgproc::RICInterpolator> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_ximgproc_edge_drawing
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
    cv::Ptr<cv::ximgproc::EdgeDrawing> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_ximgproc_edge_boxes
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
    cv::Ptr<cv::ximgproc::EdgeBoxes> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_ximgproc_ridge_detection_filter
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
    cv::Ptr<cv::ximgproc::RidgeDetectionFilter> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_ximgproc_contour_fitting
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
    cv::Ptr<cv::ximgproc::ContourFitting> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_ximgproc_scan_segment
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
    cv::Ptr<cv::ximgproc::ScanSegment> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_ximgproc_graph_segmentation
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
    cv::Ptr<cv::ximgproc::segmentation::GraphSegmentation> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_ximgproc_selective_search_strategy
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
    cv::Ptr<cv::ximgproc::segmentation::SelectiveSearchSegmentationStrategy> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_ximgproc_selective_search_segmentation
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
    cv::Ptr<cv::ximgproc::segmentation::SelectiveSearchSegmentation> value;
#else
    int placeholder;
#endif
};

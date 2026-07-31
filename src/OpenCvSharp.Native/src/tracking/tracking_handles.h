#pragma once

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_TRACKING)
#include <opencv2/tracking.hpp>
#include <opencv2/tracking/tracking_legacy.hpp>
#endif

struct jyppx_ocv_tracking_tracker
{
    virtual ~jyppx_ocv_tracking_tracker() = default;

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_TRACKING)
    cv::Ptr<cv::Tracker> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_tracking_tracker_kcf : jyppx_ocv_tracking_tracker
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_TRACKING)
    cv::Ptr<cv::TrackerKCF> concrete;
#else
    int placeholder2;
#endif
};

struct jyppx_ocv_tracking_tracker_csrt : jyppx_ocv_tracking_tracker
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_TRACKING)
    cv::Ptr<cv::TrackerCSRT> concrete;
#else
    int placeholder2;
#endif
};

struct jyppx_ocv_tracking_legacy_tracker
{
    virtual ~jyppx_ocv_tracking_legacy_tracker() = default;

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_TRACKING)
    cv::Ptr<cv::legacy::Tracker> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_tracking_legacy_tracker_mosse : jyppx_ocv_tracking_legacy_tracker
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_TRACKING)
    cv::Ptr<cv::legacy::TrackerMOSSE> concrete;
#else
    int placeholder2;
#endif
};

struct jyppx_ocv_tracking_legacy_tracker_mil : jyppx_ocv_tracking_legacy_tracker
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_TRACKING)
    cv::Ptr<cv::legacy::TrackerMIL> concrete;
#else
    int placeholder2;
#endif
};

struct jyppx_ocv_tracking_legacy_tracker_median_flow : jyppx_ocv_tracking_legacy_tracker
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_TRACKING)
    cv::Ptr<cv::legacy::TrackerMedianFlow> concrete;
#else
    int placeholder2;
#endif
};

struct jyppx_ocv_tracking_legacy_tracker_boosting : jyppx_ocv_tracking_legacy_tracker
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_TRACKING)
    cv::Ptr<cv::legacy::TrackerBoosting> concrete;
#else
    int placeholder2;
#endif
};

struct jyppx_ocv_tracking_legacy_tracker_tld : jyppx_ocv_tracking_legacy_tracker
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_TRACKING)
    cv::Ptr<cv::legacy::TrackerTLD> concrete;
#else
    int placeholder2;
#endif
};

struct jyppx_ocv_tracking_legacy_tracker_kcf : jyppx_ocv_tracking_legacy_tracker
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_TRACKING)
    cv::Ptr<cv::legacy::TrackerKCF> concrete;
#else
    int placeholder2;
#endif
};

struct jyppx_ocv_tracking_legacy_tracker_csrt : jyppx_ocv_tracking_legacy_tracker
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_TRACKING)
    cv::Ptr<cv::legacy::TrackerCSRT> concrete;
#else
    int placeholder2;
#endif
};

struct jyppx_ocv_tracking_legacy_multi_tracker
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_TRACKING)
    cv::Ptr<cv::legacy::MultiTracker> value;
#else
    int placeholder;
#endif
};

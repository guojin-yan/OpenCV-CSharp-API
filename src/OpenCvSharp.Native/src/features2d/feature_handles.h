#pragma once

#if defined(OPENCV_CSHARP_HAS_OPENCV_FEATURES2D)
#include <opencv2/features.hpp>
#include <filesystem>
#endif

#if defined(OPENCV_CSHARP_HAS_OPENCV_XFEATURES2D)
#include <opencv2/xfeatures2d.hpp>
#endif

struct jyppx_ocv_features2d_orb
{
#if defined(OPENCV_CSHARP_HAS_OPENCV_FEATURES2D)
    cv::Ptr<cv::ORB> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_features2d_bf_matcher
{
#if defined(OPENCV_CSHARP_HAS_OPENCV_FEATURES2D)
    cv::Ptr<cv::BFMatcher> value;
    int norm_type;
    bool cross_check;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_features2d_descriptor_matcher
{
#if defined(OPENCV_CSHARP_HAS_OPENCV_FEATURES2D)
    cv::Ptr<cv::DescriptorMatcher> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_features2d_sift
{
#if defined(OPENCV_CSHARP_HAS_OPENCV_FEATURES2D)
    cv::Ptr<cv::SIFT> value;
    int descriptor_type;
    bool enable_precise_upscale;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_features2d_fast
{
#if defined(OPENCV_CSHARP_HAS_OPENCV_FEATURES2D)
    cv::Ptr<cv::FastFeatureDetector> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_features2d_gftt
{
#if defined(OPENCV_CSHARP_HAS_OPENCV_FEATURES2D)
    cv::Ptr<cv::GFTTDetector> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_features2d_flann_matcher
{
#if defined(OPENCV_CSHARP_HAS_OPENCV_FEATURES2D)
    cv::Ptr<cv::FlannBasedMatcher> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_features2d_mser
{
#if defined(OPENCV_CSHARP_HAS_OPENCV_FEATURES2D)
    cv::Ptr<cv::MSER> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_features2d_simple_blob
{
#if defined(OPENCV_CSHARP_HAS_OPENCV_FEATURES2D)
    cv::Ptr<cv::SimpleBlobDetector> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_features2d_brisk
{
#if defined(OPENCV_CSHARP_HAS_OPENCV_XFEATURES2D)
    cv::Ptr<cv::xfeatures2d::BRISK> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_features2d_kaze
{
#if defined(OPENCV_CSHARP_HAS_OPENCV_XFEATURES2D)
    cv::Ptr<cv::xfeatures2d::KAZE> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_features2d_akaze
{
#if defined(OPENCV_CSHARP_HAS_OPENCV_XFEATURES2D)
    cv::Ptr<cv::xfeatures2d::AKAZE> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_features2d_affine
{
#if defined(OPENCV_CSHARP_HAS_OPENCV_FEATURES2D)
    cv::Ptr<cv::AffineFeature> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_features2d_ann_index
{
#if defined(OPENCV_CSHARP_HAS_OPENCV_FEATURES2D)
    cv::Ptr<cv::ANNIndex> value;
    int dimension;
    cv::ANNIndex::Distance distance;
    std::filesystem::path temporary_path;
    std::filesystem::path on_disk_target_path;

    ~jyppx_ocv_features2d_ann_index()
    {
        value.release();
        if (!temporary_path.empty())
        {
            std::error_code ignored;
            std::filesystem::remove(temporary_path, ignored);
        }
    }
#else
    int placeholder;
#endif
};

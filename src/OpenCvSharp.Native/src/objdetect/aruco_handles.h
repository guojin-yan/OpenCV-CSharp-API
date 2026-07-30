#pragma once

#if defined(OPENCV_CSHARP_HAS_OPENCV)
#include <opencv2/objdetect.hpp>
#include <opencv2/objdetect/aruco_board.hpp>
#include <opencv2/objdetect/aruco_detector.hpp>
#include <opencv2/objdetect/aruco_dictionary.hpp>
#include <opencv2/objdetect/charuco_detector.hpp>
#include <opencv2/objdetect/mcc_checker_detector.hpp>
#endif

struct jyppx_ocv_aruco_dictionary
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    cv::aruco::Dictionary value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_aruco_detector
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    cv::aruco::ArucoDetector value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_aruco_board
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    cv::aruco::Board value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_aruco_grid_board
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    cv::aruco::GridBoard value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_aruco_charuco_board
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    cv::aruco::CharucoBoard value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_aruco_charuco_detector
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    cv::aruco::CharucoDetector value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_mcc_checker
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    cv::Ptr<cv::mcc::CChecker> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_mcc_checker_detector
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    cv::Ptr<cv::mcc::CCheckerDetector> value;
#else
    int placeholder;
#endif
};

#pragma once

#if defined(OPENCV_CSHARP_HAS_OPENCV)
#include <opencv2/objdetect.hpp>
#include <opencv2/objdetect/barcode.hpp>
#endif

struct jyppx_ocv_qrcode_detector
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    cv::QRCodeDetector value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_face_detector_yn
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    cv::Ptr<cv::FaceDetectorYN> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_face_recognizer_sf
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    cv::Ptr<cv::FaceRecognizerSF> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_barcode_detector
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    cv::barcode::BarcodeDetector value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_qrcode_detector_aruco
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    cv::QRCodeDetectorAruco value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_qrcode_encoder
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    cv::Ptr<cv::QRCodeEncoder> value;
#else
    int placeholder;
#endif
};

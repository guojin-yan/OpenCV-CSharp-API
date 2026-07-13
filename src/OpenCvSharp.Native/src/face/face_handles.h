#pragma once

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
#include <opencv2/face.hpp>
#include <opencv2/face/bif.hpp>
#include <opencv2/face/facerec.hpp>
#include <opencv2/face/predict_collector.hpp>
#include <opencv2/face/facemark.hpp>
#include <opencv2/face/facemarkLBF.hpp>
#include <opencv2/face/mace.hpp>
#include <opencv2/core.hpp>
#endif

struct jyppx_ocv_face_recognizer
{
    virtual ~jyppx_ocv_face_recognizer() = default;

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
    cv::Ptr<cv::face::FaceRecognizer> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_face_basic_recognizer : jyppx_ocv_face_recognizer
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
    cv::Ptr<cv::face::BasicFaceRecognizer> basic;
#else
    int placeholder2;
#endif
};

struct jyppx_ocv_face_eigen_recognizer : jyppx_ocv_face_basic_recognizer
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
    cv::Ptr<cv::face::EigenFaceRecognizer> concrete;
#else
    int placeholder3;
#endif
};

struct jyppx_ocv_face_fisher_recognizer : jyppx_ocv_face_basic_recognizer
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
    cv::Ptr<cv::face::FisherFaceRecognizer> concrete;
#else
    int placeholder3;
#endif
};

struct jyppx_ocv_face_lbph_recognizer : jyppx_ocv_face_recognizer
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
    cv::Ptr<cv::face::LBPHFaceRecognizer> concrete;
#else
    int placeholder2;
#endif
};

struct jyppx_ocv_face_standard_collector
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
    cv::Ptr<cv::face::StandardCollector> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_face_bif
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
    cv::Ptr<cv::face::BIF> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_face_facemark
{
    virtual ~jyppx_ocv_face_facemark() = default;

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
    cv::Ptr<cv::face::Facemark> value;
    // cached landmark output from the most recent fit() call.
    std::vector<std::vector<cv::Point2f>> last_landmarks;
    // cached face rectangles from the most recent getFaces() call.
    std::vector<cv::Rect> last_faces;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_face_facemark_train : jyppx_ocv_face_facemark
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
    cv::Ptr<cv::face::FacemarkTrain> train_value;
#else
    int placeholder2;
#endif
};

struct jyppx_ocv_face_facemark_lbf : jyppx_ocv_face_facemark_train
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
    cv::Ptr<cv::face::FacemarkLBF> concrete;
    cv::face::FacemarkLBF::Params params;
#else
    int placeholder3;
#endif
};

struct jyppx_ocv_face_mace
{
    virtual ~jyppx_ocv_face_mace() = default;

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
    cv::Ptr<cv::face::MACE> value;
#else
    int placeholder;
#endif
};

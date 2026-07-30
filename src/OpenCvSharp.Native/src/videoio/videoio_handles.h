#pragma once

#if defined(OPENCV_CSHARP_HAS_OPENCV)
#include <opencv2/videoio.hpp>
#endif

struct jyppx_ocv_video_capture
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    cv::VideoCapture value;
    cv::Ptr<cv::IStreamReader> stream_reader;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_video_stream_reader
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    cv::Ptr<cv::IStreamReader> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_video_writer
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    cv::VideoWriter value;
#else
    int placeholder;
#endif
};

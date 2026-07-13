#pragma once

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_LINE_DESCRIPTOR)
#include <opencv2/line_descriptor.hpp>
#endif

struct jyppx_ocv_line_descriptor_binary_descriptor
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_LINE_DESCRIPTOR)
    cv::Ptr<cv::line_descriptor::BinaryDescriptor> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_line_descriptor_binary_descriptor_matcher
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_LINE_DESCRIPTOR)
    cv::Ptr<cv::line_descriptor::BinaryDescriptorMatcher> value;
#else
    int placeholder;
#endif
};

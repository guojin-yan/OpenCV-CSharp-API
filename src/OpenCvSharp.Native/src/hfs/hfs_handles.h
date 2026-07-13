#pragma once

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_HFS)
#include <opencv2/hfs.hpp>
#endif

struct jyppx_ocv_hfs_segment
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_HFS)
    cv::Ptr<cv::hfs::HfsSegment> value;
#else
    int placeholder;
#endif
};

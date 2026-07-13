#pragma once

#include "open_cv_sharp/bioinspired/bioinspired.h"

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BIOINSPIRED)
#include <opencv2/bioinspired.hpp>
#endif

struct jyppx_ocv_bioinspired_retina
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BIOINSPIRED)
    cv::Ptr<cv::bioinspired::Retina> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_bioinspired_retina_fast_tone_mapping
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BIOINSPIRED)
    cv::Ptr<cv::bioinspired::RetinaFastToneMapping> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_bioinspired_transient_areas_segmentation_module
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BIOINSPIRED)
    cv::Ptr<cv::bioinspired::TransientAreasSegmentationModule> value;
#else
    int placeholder;
#endif
};

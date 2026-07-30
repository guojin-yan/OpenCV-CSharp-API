#pragma once

#include "open_cv_sharp/core/persistence.h"

#if defined(OPENCV_CSHARP_HAS_OPENCV)
#include <opencv2/core/persistence.hpp>

namespace opencv_csharp_native
{
    int access_core_file_storage(
        const char* api_name,
        jyppx_ocv_core_file_storage* storage,
        cv::FileStorage** out_value);

    int access_core_file_node(
        const char* api_name,
        const jyppx_ocv_core_file_node* node,
        const cv::FileNode** out_value);
}
#endif

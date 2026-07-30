#pragma once

#include "open_cv_sharp/core/persistence.h"

#include <string>

namespace opencv_csharp_native
{
    int make_core_utf8_result(
        const char* api_name,
        const std::string& value,
        jyppx_ocv_core_utf8_result** out_result);
}

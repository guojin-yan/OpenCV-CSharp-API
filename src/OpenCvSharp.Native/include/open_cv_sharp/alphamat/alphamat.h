#pragma once

#include "open_cv_sharp/core/mat.h"
#include "open_cv_sharp/export.h"
#include "open_cv_sharp/status.h"

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_alphamat_info_flow(
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* trimap,
    jyppx_ocv_mat* result);

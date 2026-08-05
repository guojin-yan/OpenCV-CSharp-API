#pragma once

#include "open_cv_sharp/export.h"

#define OPENCV_CSHARP_NATIVE_ABI_VERSION 1

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_get_native_abi_version(void);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_get_version_major(void);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_get_version_minor(void);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_get_version_revision(void);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API const char* jyppx_ocv_get_version_string(void);

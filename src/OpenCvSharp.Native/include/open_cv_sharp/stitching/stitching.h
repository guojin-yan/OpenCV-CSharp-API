#pragma once

#include "open_cv_sharp/core/mat.h"
#include "open_cv_sharp/export.h"
#include "open_cv_sharp/status.h"

typedef struct jyppx_ocv_stitcher jyppx_ocv_stitcher;

typedef struct jyppx_ocv_stitching_camera_params
{
    double focal;
    double aspect;
    double ppx;
    double ppy;
    jyppx_ocv_mat* r;
    jyppx_ocv_mat* t;
} jyppx_ocv_stitching_camera_params;

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitcher_create(
    int mode,
    jyppx_ocv_stitcher** stitcher);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_stitcher_release_handle(
    jyppx_ocv_stitcher* stitcher);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitcher_get_double_property(
    const jyppx_ocv_stitcher* stitcher,
    int property_id,
    double* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitcher_set_double_property(
    jyppx_ocv_stitcher* stitcher,
    int property_id,
    double value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitcher_get_int_property(
    const jyppx_ocv_stitcher* stitcher,
    int property_id,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitcher_set_int_property(
    jyppx_ocv_stitcher* stitcher,
    int property_id,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitcher_estimate_transform(
    jyppx_ocv_stitcher* stitcher,
    const jyppx_ocv_mat* const* images,
    int image_count,
    const jyppx_ocv_mat* const* masks,
    int mask_count,
    int* status_code);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitcher_compose_panorama(
    jyppx_ocv_stitcher* stitcher,
    jyppx_ocv_mat* pano,
    int* status_code);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitcher_compose_panorama_images(
    jyppx_ocv_stitcher* stitcher,
    const jyppx_ocv_mat* const* images,
    int image_count,
    jyppx_ocv_mat* pano,
    int* status_code);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitcher_stitch(
    jyppx_ocv_stitcher* stitcher,
    const jyppx_ocv_mat* const* images,
    int image_count,
    const jyppx_ocv_mat* const* masks,
    int mask_count,
    jyppx_ocv_mat* pano,
    int* status_code);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitcher_get_component_count(
    const jyppx_ocv_stitcher* stitcher,
    int* component_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitcher_get_component_fill(
    const jyppx_ocv_stitcher* stitcher,
    int* components,
    int component_capacity,
    int* component_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitcher_get_cameras_count(
    const jyppx_ocv_stitcher* stitcher,
    int* camera_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitcher_get_cameras_fill(
    const jyppx_ocv_stitcher* stitcher,
    jyppx_ocv_stitching_camera_params* cameras,
    int camera_capacity,
    int* camera_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitcher_get_result_mask(
    const jyppx_ocv_stitcher* stitcher,
    jyppx_ocv_mat* result_mask);

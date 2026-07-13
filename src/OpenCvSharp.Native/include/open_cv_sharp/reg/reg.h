#pragma once

#include "open_cv_sharp/core/mat.h"
#include "open_cv_sharp/export.h"
#include "open_cv_sharp/status.h"

typedef struct jyppx_ocv_reg_map jyppx_ocv_reg_map;
typedef struct jyppx_ocv_reg_mapper jyppx_ocv_reg_mapper;

enum jyppx_ocv_reg_map_kind
{
    JYPPX_OCV_REG_MAP_KIND_UNKNOWN = 0,
    JYPPX_OCV_REG_MAP_KIND_SHIFT = 1,
    JYPPX_OCV_REG_MAP_KIND_AFFINE = 2,
    JYPPX_OCV_REG_MAP_KIND_PROJEC = 3
};

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_reg_map_release(
    jyppx_ocv_reg_map* map);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_reg_map_get_kind(
    const jyppx_ocv_reg_map* map,
    int* kind);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_reg_map_warp(
    const jyppx_ocv_reg_map* map,
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_reg_map_inverse_warp(
    const jyppx_ocv_reg_map* map,
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_reg_map_inverse_map(
    const jyppx_ocv_reg_map* map,
    jyppx_ocv_reg_map** inverse_map);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_reg_map_compose(
    jyppx_ocv_reg_map* map,
    const jyppx_ocv_reg_map* other);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_reg_map_scale(
    jyppx_ocv_reg_map* map,
    double factor);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_reg_map_shift_create(
    double shift_x,
    double shift_y,
    jyppx_ocv_reg_map** map);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_reg_map_shift_get(
    const jyppx_ocv_reg_map* map,
    double* shift_x,
    double* shift_y);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_reg_map_affine_create(
    double m00,
    double m01,
    double m10,
    double m11,
    double shift_x,
    double shift_y,
    jyppx_ocv_reg_map** map);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_reg_map_affine_get(
    const jyppx_ocv_reg_map* map,
    double* m00,
    double* m01,
    double* m10,
    double* m11,
    double* shift_x,
    double* shift_y);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_reg_map_projec_create(
    double m00,
    double m01,
    double m02,
    double m10,
    double m11,
    double m12,
    double m20,
    double m21,
    double m22,
    jyppx_ocv_reg_map** map);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_reg_map_projec_get(
    const jyppx_ocv_reg_map* map,
    double* m00,
    double* m01,
    double* m02,
    double* m10,
    double* m11,
    double* m12,
    double* m20,
    double* m21,
    double* m22);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_reg_map_projec_normalize(
    jyppx_ocv_reg_map* map);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_reg_mapper_release(
    jyppx_ocv_reg_mapper* mapper);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_reg_mapper_grad_shift_create(
    jyppx_ocv_reg_mapper** mapper);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_reg_mapper_grad_euclid_create(
    jyppx_ocv_reg_mapper** mapper);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_reg_mapper_grad_similar_create(
    jyppx_ocv_reg_mapper** mapper);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_reg_mapper_grad_affine_create(
    jyppx_ocv_reg_mapper** mapper);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_reg_mapper_grad_proj_create(
    jyppx_ocv_reg_mapper** mapper);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_reg_mapper_pyramid_create(
    const jyppx_ocv_reg_mapper* base_mapper,
    jyppx_ocv_reg_mapper** mapper);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_reg_mapper_calculate(
    const jyppx_ocv_reg_mapper* mapper,
    const jyppx_ocv_mat* img1,
    const jyppx_ocv_mat* img2,
    const jyppx_ocv_reg_map* init,
    jyppx_ocv_reg_map** map);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_reg_mapper_get_map(
    const jyppx_ocv_reg_mapper* mapper,
    jyppx_ocv_reg_map** map);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_reg_mapper_pyramid_get_num_levels(
    const jyppx_ocv_reg_mapper* mapper,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_reg_mapper_pyramid_set_num_levels(
    jyppx_ocv_reg_mapper* mapper,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_reg_mapper_pyramid_get_num_iterations_per_scale(
    const jyppx_ocv_reg_mapper* mapper,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_reg_mapper_pyramid_set_num_iterations_per_scale(
    jyppx_ocv_reg_mapper* mapper,
    int value);

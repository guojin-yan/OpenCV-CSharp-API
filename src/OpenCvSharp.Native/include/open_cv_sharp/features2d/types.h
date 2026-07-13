#pragma once

#include <stdint.h>

typedef struct jyppx_ocv_key_point
{
    float x;
    float y;
    float size;
    float angle;
    float response;
    int32_t octave;
    int32_t class_id;
} jyppx_ocv_key_point;

typedef struct jyppx_ocv_dmatch
{
    int32_t query_idx;
    int32_t train_idx;
    int32_t img_idx;
    float distance;
} jyppx_ocv_dmatch;

typedef struct jyppx_ocv_point
{
    int32_t x;
    int32_t y;
} jyppx_ocv_point;

typedef struct jyppx_ocv_rect
{
    int32_t x;
    int32_t y;
    int32_t width;
    int32_t height;
} jyppx_ocv_rect;

typedef struct jyppx_ocv_simple_blob_params
{
    int32_t size;
    float threshold_step;
    float min_threshold;
    float max_threshold;
    int32_t min_repeatability;
    float min_dist_between_blobs;
    int32_t filter_by_color;
    int32_t blob_color;
    int32_t filter_by_area;
    float min_area;
    float max_area;
    int32_t filter_by_circularity;
    float min_circularity;
    float max_circularity;
    int32_t filter_by_inertia;
    float min_inertia_ratio;
    float max_inertia_ratio;
    int32_t filter_by_convexity;
    float min_convexity;
    float max_convexity;
    int32_t collect_contours;
} jyppx_ocv_simple_blob_params;

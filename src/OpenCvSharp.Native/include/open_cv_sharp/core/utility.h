#pragma once

#include "open_cv_sharp/core/persistence.h"
#include "open_cv_sharp/export.h"
#include "open_cv_sharp/status.h"

#include <stdint.h>

typedef struct jyppx_ocv_core_tick_meter jyppx_ocv_core_tick_meter;

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_set_num_threads(int thread_count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_get_num_threads(int* out_thread_count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_get_thread_num(int* out_thread_number);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_get_build_information(
    jyppx_ocv_core_utf8_result** out_result);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_get_tick_count(int64_t* out_tick_count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_get_tick_frequency(double* out_tick_frequency);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_tick_meter_create(
    jyppx_ocv_core_tick_meter** out_meter);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_core_tick_meter_release(
    jyppx_ocv_core_tick_meter* meter);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_tick_meter_start(
    jyppx_ocv_core_tick_meter* meter);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_tick_meter_stop(
    jyppx_ocv_core_tick_meter* meter);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_tick_meter_get_time_ticks(
    const jyppx_ocv_core_tick_meter* meter,
    int64_t* out_value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_tick_meter_get_time_micro(
    const jyppx_ocv_core_tick_meter* meter,
    double* out_value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_tick_meter_get_time_milli(
    const jyppx_ocv_core_tick_meter* meter,
    double* out_value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_tick_meter_get_time_sec(
    const jyppx_ocv_core_tick_meter* meter,
    double* out_value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_tick_meter_get_last_time_ticks(
    const jyppx_ocv_core_tick_meter* meter,
    int64_t* out_value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_tick_meter_get_last_time_micro(
    const jyppx_ocv_core_tick_meter* meter,
    double* out_value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_tick_meter_get_last_time_milli(
    const jyppx_ocv_core_tick_meter* meter,
    double* out_value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_tick_meter_get_last_time_sec(
    const jyppx_ocv_core_tick_meter* meter,
    double* out_value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_tick_meter_get_counter(
    const jyppx_ocv_core_tick_meter* meter,
    int64_t* out_value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_tick_meter_get_fps(
    const jyppx_ocv_core_tick_meter* meter,
    double* out_value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_tick_meter_get_avg_time_sec(
    const jyppx_ocv_core_tick_meter* meter,
    double* out_value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_tick_meter_get_avg_time_milli(
    const jyppx_ocv_core_tick_meter* meter,
    double* out_value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_tick_meter_reset(
    jyppx_ocv_core_tick_meter* meter);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_get_cpu_tick_count(
    int64_t* out_tick_count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_check_hardware_support(
    int feature,
    int* out_supported);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_get_hardware_feature_name(
    int feature,
    jyppx_ocv_core_utf8_result** out_result);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_get_cpu_features_line(
    jyppx_ocv_core_utf8_result** out_result);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_get_number_of_cpus(
    int* out_cpu_count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_get_default_algorithm_hint(
    int* out_hint);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_set_use_optimized(int enabled);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_use_optimized(int* out_enabled);

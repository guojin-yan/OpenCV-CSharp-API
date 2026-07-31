#include "open_cv_sharp/core/mat.h"
#include "open_cv_sharp/core/decomp.h"
#include "open_cv_sharp/core/operations.h"
#include "open_cv_sharp/core/persistence.h"
#include "open_cv_sharp/core/utility.h"
#if !defined(OPENCV_CSHARP_RUNTIME_PROFILE_MINI)
#include "open_cv_sharp/calib3d/calib3d.h"
#include "open_cv_sharp/dnn/dnn.h"
#include "open_cv_sharp/features2d/features2d.h"
#include "open_cv_sharp/ml/ml.h"
#include "open_cv_sharp/objdetect/aruco.h"
#include "open_cv_sharp/objdetect/objdetect.h"
#include "open_cv_sharp/photo/photo.h"
#include "open_cv_sharp/tracking/tracking.h"
#include "open_cv_sharp/video/video.h"
#endif
#include "open_cv_sharp/error.h"
#include "open_cv_sharp/imgcodecs.h"
#include "open_cv_sharp/imgproc.h"
#include "open_cv_sharp/status.h"
#include "open_cv_sharp/videoio/videoio.h"
#include "open_cv_sharp/version.h"

#include <cmath>
#include <cstring>
#include <cstdio>
#include <limits>
#include <string>
#include <vector>

namespace
{
    long long videoio_smoke_read(void*, char* buffer, long long size)
    {
        if (size > 0 && buffer != nullptr)
        {
            buffer[0] = 'v';
            return 1;
        }
        return 0;
    }

    long long videoio_smoke_seek(void*, long long offset, int)
    {
        return offset;
    }

    void videoio_smoke_release(void*)
    {
    }

    int run_videoio_smoke()
    {
        jyppx_ocv_video_capture* capture = nullptr;
        if (jyppx_ocv_video_capture_create(&capture) != OPENCV_CSHARP_STATUS_OK || capture == nullptr)
        {
            return 400;
        }

        int opened = 1;
        if (jyppx_ocv_video_capture_open_index_params(capture, -1, 0, nullptr, 0, &opened) != OPENCV_CSHARP_STATUS_OK || opened != 0)
        {
            jyppx_ocv_video_capture_release_handle(capture);
            return 401;
        }

        if (jyppx_ocv_video_capture_set_exception_mode(capture, 1) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_video_capture_release_handle(capture);
            return 402;
        }

        int exception_mode = 0;
        if (jyppx_ocv_video_capture_get_exception_mode(capture, &exception_mode) != OPENCV_CSHARP_STATUS_OK || exception_mode != 1)
        {
            jyppx_ocv_video_capture_release_handle(capture);
            return 403;
        }

        int fourcc = 0;
        if (jyppx_ocv_video_writer_fourcc('M', 'J', 'P', 'G', &fourcc) != OPENCV_CSHARP_STATUS_OK || fourcc != ('M' | ('J' << 8) | ('P' << 16) | ('G' << 24)))
        {
            jyppx_ocv_video_capture_release_handle(capture);
            return 404;
        }

        int backend_count = 0;
        if (jyppx_ocv_videoio_registry_get_backends_count(&backend_count) != OPENCV_CSHARP_STATUS_OK || backend_count <= 0)
        {
            jyppx_ocv_video_capture_release_handle(capture);
            return 405;
        }
        int backend_values[64] = {};
        int backend_written = 0;
        if (jyppx_ocv_videoio_registry_get_backends_fill(backend_values, 64, &backend_written) != OPENCV_CSHARP_STATUS_OK || backend_written <= 0 || backend_written > 64)
        {
            jyppx_ocv_video_capture_release_handle(capture);
            return 406;
        }

        int context = 1;
        jyppx_ocv_video_stream_reader* reader = nullptr;
        if (jyppx_ocv_video_stream_reader_create(&context, videoio_smoke_read, videoio_smoke_seek, videoio_smoke_release, &reader) != OPENCV_CSHARP_STATUS_OK || reader == nullptr)
        {
            jyppx_ocv_video_capture_release_handle(capture);
            return 407;
        }
        char buffer[4] = {};
        long long bytes_read = 0;
        long long position = 0;
        if (jyppx_ocv_video_stream_reader_read(reader, buffer, 4, &bytes_read) != OPENCV_CSHARP_STATUS_OK || bytes_read != 1 || buffer[0] != 'v' ||
            jyppx_ocv_video_stream_reader_seek(reader, 7, 0, &position) != OPENCV_CSHARP_STATUS_OK || position != 7)
        {
            jyppx_ocv_video_stream_reader_release_handle(reader);
            jyppx_ocv_video_capture_release_handle(capture);
            return 408;
        }
        jyppx_ocv_video_stream_reader_release_handle(reader);
        jyppx_ocv_video_capture_release_handle(capture);
        return 0;
    }

    struct NativeMatHandle
    {
        jyppx_ocv_mat* value = nullptr;

        NativeMatHandle() = default;
        NativeMatHandle(const NativeMatHandle&) = delete;
        NativeMatHandle& operator=(const NativeMatHandle&) = delete;

        ~NativeMatHandle()
        {
            jyppx_ocv_mat_release(value);
        }

        jyppx_ocv_mat* get() const noexcept
        {
            return value;
        }

        jyppx_ocv_mat** out() noexcept
        {
            return &value;
        }
    };

#if !defined(OPENCV_CSHARP_RUNTIME_PROFILE_MINI)
    struct NativeDenseOpticalFlowHandle
    {
        jyppx_ocv_dense_optical_flow* value = nullptr;
        ~NativeDenseOpticalFlowHandle() { jyppx_ocv_dense_optical_flow_release_handle(value); }
    };

    struct NativeSparseOpticalFlowHandle
    {
        jyppx_ocv_sparse_optical_flow* value = nullptr;
        ~NativeSparseOpticalFlowHandle() { jyppx_ocv_sparse_optical_flow_release_handle(value); }
    };

    struct NativeVideoTrackerHandle
    {
        jyppx_ocv_video_tracker* value = nullptr;
        ~NativeVideoTrackerHandle() { jyppx_ocv_video_tracker_release_handle(value); }
    };

    struct NativeMlModelHandle
    {
        jyppx_ocv_ml_model* value = nullptr;
        ~NativeMlModelHandle() { jyppx_ocv_ml_model_release_handle(value); }
    };

    struct NativeMlTrainDataHandle
    {
        jyppx_ocv_ml_train_data* value = nullptr;
        ~NativeMlTrainDataHandle() { jyppx_ocv_ml_train_data_release_handle(value); }
    };

    struct NativeAnnIndexHandle
    {
        jyppx_ocv_features2d_ann_index* value = nullptr;

        NativeAnnIndexHandle() = default;
        NativeAnnIndexHandle(const NativeAnnIndexHandle&) = delete;
        NativeAnnIndexHandle& operator=(const NativeAnnIndexHandle&) = delete;

        ~NativeAnnIndexHandle()
        {
            jyppx_ocv_features2d_ann_index_release(value);
        }
    };

    struct NativeAlignMtbHandle
    {
        jyppx_ocv_align_mtb* value = nullptr;
        ~NativeAlignMtbHandle() { jyppx_ocv_align_mtb_release_handle(value); }
    };

    struct NativeCalibrateCrfHandle
    {
        jyppx_ocv_calibrate_crf* value = nullptr;
        ~NativeCalibrateCrfHandle() { jyppx_ocv_calibrate_crf_release_handle(value); }
    };

    struct NativeMergeExposuresHandle
    {
        jyppx_ocv_merge_exposures* value = nullptr;
        ~NativeMergeExposuresHandle() { jyppx_ocv_merge_exposures_release_handle(value); }
    };

    struct NativeColorCorrectionModelHandle
    {
        jyppx_ocv_color_correction_model* value = nullptr;
        ~NativeColorCorrectionModelHandle() { jyppx_ocv_photo_ccm_release_handle(value); }
    };

    struct NativeIntelligentScissorsHandle
    {
        jyppx_ocv_intelligent_scissors_mb* value = nullptr;
        ~NativeIntelligentScissorsHandle() { jyppx_ocv_photo_intelligent_scissors_release_handle(value); }
    };
#endif

    struct NativeFileStorageHandle
    {
        jyppx_ocv_core_file_storage* value = nullptr;

        NativeFileStorageHandle() = default;
        NativeFileStorageHandle(const NativeFileStorageHandle&) = delete;
        NativeFileStorageHandle& operator=(const NativeFileStorageHandle&) = delete;

        ~NativeFileStorageHandle()
        {
            jyppx_ocv_core_file_storage_release_handle(value);
        }
    };

    struct NativeRngHandle
    {
        jyppx_ocv_rng* value = nullptr;

        NativeRngHandle() = default;
        NativeRngHandle(const NativeRngHandle&) = delete;
        NativeRngHandle& operator=(const NativeRngHandle&) = delete;

        ~NativeRngHandle()
        {
            jyppx_ocv_core_rng_release(value);
        }
    };

    struct NativeFileNodeHandle
    {
        jyppx_ocv_core_file_node* value = nullptr;

        NativeFileNodeHandle() = default;
        NativeFileNodeHandle(const NativeFileNodeHandle&) = delete;
        NativeFileNodeHandle& operator=(const NativeFileNodeHandle&) = delete;

        ~NativeFileNodeHandle()
        {
            jyppx_ocv_core_file_node_release(value);
        }
    };

    struct NativeUtf8ResultHandle
    {
        jyppx_ocv_core_utf8_result* value = nullptr;

        NativeUtf8ResultHandle() = default;
        NativeUtf8ResultHandle(const NativeUtf8ResultHandle&) = delete;
        NativeUtf8ResultHandle& operator=(const NativeUtf8ResultHandle&) = delete;

        ~NativeUtf8ResultHandle()
        {
            jyppx_ocv_core_utf8_result_release(value);
        }
    };

    struct NativeTickMeterHandle
    {
        jyppx_ocv_core_tick_meter* value = nullptr;

        NativeTickMeterHandle() = default;
        NativeTickMeterHandle(const NativeTickMeterHandle&) = delete;
        NativeTickMeterHandle& operator=(const NativeTickMeterHandle&) = delete;

        ~NativeTickMeterHandle()
        {
            jyppx_ocv_core_tick_meter_release(value);
        }
    };

    struct NativeRuntimeStateRestore
    {
        int thread_count = 0;
        int optimized = 0;
        bool initialized = false;

        ~NativeRuntimeStateRestore()
        {
            if (initialized)
            {
                jyppx_ocv_core_set_num_threads(thread_count);
                jyppx_ocv_core_set_use_optimized(optimized);
            }
        }
    };

    struct NativeStringListHandle
    {
        jyppx_ocv_core_string_list* value = nullptr;

        NativeStringListHandle() = default;
        NativeStringListHandle(const NativeStringListHandle&) = delete;
        NativeStringListHandle& operator=(const NativeStringListHandle&) = delete;

        ~NativeStringListHandle()
        {
            jyppx_ocv_core_string_list_release(value);
        }
    };

    bool utf8_result_equals(const jyppx_ocv_core_utf8_result* result, const unsigned char* expected, size_t expected_size)
    {
        size_t size = 0;
        const unsigned char* data = nullptr;
        return jyppx_ocv_core_utf8_result_size(result, &size) == OPENCV_CSHARP_STATUS_OK &&
            size == expected_size &&
            jyppx_ocv_core_utf8_result_data(result, &data) == OPENCV_CSHARP_STATUS_OK &&
            (size == 0 || (data != nullptr && std::memcmp(data, expected, size) == 0));
    }

#if !defined(OPENCV_CSHARP_RUNTIME_PROFILE_MINI)
    struct NativeArucoDictionaryHandle
    {
        jyppx_ocv_aruco_dictionary* value = nullptr;
        ~NativeArucoDictionaryHandle() { jyppx_ocv_aruco_dictionary_release_handle(value); }
    };

    struct NativeArucoBoardHandle
    {
        jyppx_ocv_aruco_board* value = nullptr;
        ~NativeArucoBoardHandle() { jyppx_ocv_aruco_board_release_handle(value); }
    };

    struct NativeArucoDetectorHandle
    {
        jyppx_ocv_aruco_detector* value = nullptr;
        ~NativeArucoDetectorHandle() { jyppx_ocv_aruco_detector_release_handle(value); }
    };

    struct NativeCharucoBoardHandle
    {
        jyppx_ocv_aruco_charuco_board* value = nullptr;
        ~NativeCharucoBoardHandle() { jyppx_ocv_aruco_charuco_board_release_handle(value); }
    };

    struct NativeCharucoDetectorHandle
    {
        jyppx_ocv_aruco_charuco_detector* value = nullptr;
        ~NativeCharucoDetectorHandle() { jyppx_ocv_aruco_charuco_detector_release_handle(value); }
    };

    struct NativeMccDetectorHandle
    {
        jyppx_ocv_mcc_checker_detector* value = nullptr;
        ~NativeMccDetectorHandle() { jyppx_ocv_mcc_checker_detector_release_handle(value); }
    };

    struct NativeQRCodeDetectorArucoHandle
    {
        jyppx_ocv_qrcode_detector_aruco* value = nullptr;
        ~NativeQRCodeDetectorArucoHandle() { jyppx_ocv_qrcode_detector_aruco_release_handle(value); }
    };

    struct NativeSubdiv2DHandle
    {
        jyppx_ocv_calib3d_subdiv2d* value = nullptr;

        NativeSubdiv2DHandle() = default;
        NativeSubdiv2DHandle(const NativeSubdiv2DHandle&) = delete;
        NativeSubdiv2DHandle& operator=(const NativeSubdiv2DHandle&) = delete;

        ~NativeSubdiv2DHandle()
        {
            jyppx_ocv_calib3d_subdiv2d_release(value);
        }
    };

    bool create_video_smoke_frame(int offset, NativeMatHandle& frame)
    {
        if (jyppx_ocv_mat_create_with_scalar(32, 32, 0, 0.0, 0.0, 0.0, 0.0, frame.out()) != OPENCV_CSHARP_STATUS_OK)
        {
            return false;
        }

        unsigned char* data = nullptr;
        size_t step = 0;
        if (jyppx_ocv_mat_data(frame.get(), &data) != OPENCV_CSHARP_STATUS_OK || data == nullptr ||
            jyppx_ocv_mat_step(frame.get(), &step) != OPENCV_CSHARP_STATUS_OK)
        {
            return false;
        }

        for (int y = 7; y < 25; ++y)
        {
            for (int x = 7 + offset; x < 23 + offset; ++x)
            {
                data[(static_cast<size_t>(y) * step) + static_cast<size_t>(x)] = ((x + y) & 1) == 0 ? 255 : 80;
            }
        }
        return true;
    }

    int run_video_optical_flow_object_smoke()
    {
        NativeMatHandle first;
        NativeMatHandle second;
        NativeMatHandle packed_flow;
        NativeMatHandle flow_u;
        NativeMatHandle flow_v;
        if (!create_video_smoke_frame(0, first) || !create_video_smoke_frame(1, second) ||
            jyppx_ocv_mat_create_with_scalar(32, 32, 37, 0.0, 0.0, 0.0, 0.0, packed_flow.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_with_scalar(32, 32, 5, 0.0, 0.0, 0.0, 0.0, flow_u.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_with_scalar(32, 32, 5, 0.0, 0.0, 0.0, 0.0, flow_v.out()) != OPENCV_CSHARP_STATUS_OK)
        {
            return 785;
        }

        NativeDenseOpticalFlowHandle farneback;
        if (jyppx_ocv_farneback_optical_flow_create(3, 0.5, 0, 9, 3, 5, 1.1, 0, &farneback.value) != OPENCV_CSHARP_STATUS_OK || farneback.value == nullptr)
        {
            return 786;
        }

        int int_value = 0;
        double double_value = 0.0;
        if (jyppx_ocv_farneback_optical_flow_set_int_property(farneback.value, 0, 2) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_farneback_optical_flow_get_int_property(farneback.value, 0, &int_value) != OPENCV_CSHARP_STATUS_OK || int_value != 2 ||
            jyppx_ocv_farneback_optical_flow_set_double_property(farneback.value, 1, 1.2) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_farneback_optical_flow_get_double_property(farneback.value, 1, &double_value) != OPENCV_CSHARP_STATUS_OK || std::abs(double_value - 1.2) > 0.000001 ||
            jyppx_ocv_farneback_optical_flow_set_bool_property(farneback.value, 0, 1) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_farneback_optical_flow_get_bool_property(farneback.value, 0, &int_value) != OPENCV_CSHARP_STATUS_OK || int_value != 1 ||
            jyppx_ocv_farneback_optical_flow_set_bool_property(farneback.value, 0, 0) != OPENCV_CSHARP_STATUS_OK)
        {
            return 787;
        }

        int rows = 0;
        int cols = 0;
        int type = 0;
        if (jyppx_ocv_dense_optical_flow_calc(farneback.value, first.get(), second.get(), packed_flow.get()) != OPENCV_CSHARP_STATUS_OK)
        {
            return 788;
        }
        if (jyppx_ocv_mat_rows(packed_flow.get(), &rows) != OPENCV_CSHARP_STATUS_OK || rows != 32)
        {
            return 798;
        }
        if (jyppx_ocv_mat_cols(packed_flow.get(), &cols) != OPENCV_CSHARP_STATUS_OK || cols != 32)
        {
            return 800;
        }
        if (jyppx_ocv_mat_type(packed_flow.get(), &type) != OPENCV_CSHARP_STATUS_OK || type != 37)
        {
            return 801;
        }
        if (jyppx_ocv_dense_optical_flow_collect_garbage(farneback.value) != OPENCV_CSHARP_STATUS_OK)
        {
            return 799;
        }

        if (jyppx_ocv_farneback_optical_flow_get_int_property(farneback.value, 99, &int_value) == OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_variational_refinement_get_int_property(farneback.value, 0, &int_value) == OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_farneback_optical_flow_get_int_property(farneback.value, 0, nullptr) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT)
        {
            return 789;
        }

        NativeDenseOpticalFlowHandle variational;
        float float_value = 0.0F;
        if (jyppx_ocv_variational_refinement_create(&variational.value) != OPENCV_CSHARP_STATUS_OK || variational.value == nullptr ||
            jyppx_ocv_variational_refinement_set_int_property(variational.value, 0, 2) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_variational_refinement_get_int_property(variational.value, 0, &int_value) != OPENCV_CSHARP_STATUS_OK || int_value != 2 ||
            jyppx_ocv_variational_refinement_set_float_property(variational.value, 0, 1.5F) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_variational_refinement_get_float_property(variational.value, 0, &float_value) != OPENCV_CSHARP_STATUS_OK || std::abs(float_value - 1.5F) > 0.000001F)
        {
            return 790;
        }

        if (jyppx_ocv_dense_optical_flow_calc(variational.value, first.get(), second.get(), packed_flow.get()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_variational_refinement_calc_uv(variational.value, first.get(), second.get(), flow_u.get(), flow_v.get()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_dense_optical_flow_collect_garbage(variational.value) != OPENCV_CSHARP_STATUS_OK)
        {
            return 791;
        }

        NativeDenseOpticalFlowHandle dis;
        if (jyppx_ocv_dis_optical_flow_create(0, &dis.value) != OPENCV_CSHARP_STATUS_OK || dis.value == nullptr ||
            jyppx_ocv_dis_optical_flow_set_int_property(dis.value, 0, 1) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_dis_optical_flow_get_int_property(dis.value, 0, &int_value) != OPENCV_CSHARP_STATUS_OK || int_value != 1 ||
            jyppx_ocv_dis_optical_flow_set_float_property(dis.value, 0, 20.0F) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_dis_optical_flow_get_float_property(dis.value, 0, &float_value) != OPENCV_CSHARP_STATUS_OK || std::abs(float_value - 20.0F) > 0.000001F ||
            jyppx_ocv_dis_optical_flow_set_bool_property(dis.value, 1, 0) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_dis_optical_flow_get_bool_property(dis.value, 1, &int_value) != OPENCV_CSHARP_STATUS_OK || int_value != 0)
        {
            return 792;
        }

        if (jyppx_ocv_dense_optical_flow_calc(dis.value, first.get(), second.get(), packed_flow.get()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_dense_optical_flow_collect_garbage(dis.value) != OPENCV_CSHARP_STATUS_OK)
        {
            return 793;
        }

        NativeSparseOpticalFlowHandle sparse;
        if (jyppx_ocv_sparse_pyr_lk_optical_flow_create(9, 9, 2, 3, 15, 0.02, 0, 0.00001, &sparse.value) != OPENCV_CSHARP_STATUS_OK || sparse.value == nullptr)
        {
            return 794;
        }

        int width = 0;
        int height = 0;
        int criteria_type = 0;
        int criteria_count = 0;
        double criteria_epsilon = 0.0;
        if (jyppx_ocv_sparse_pyr_lk_optical_flow_set_size_property(sparse.value, 11, 11) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_sparse_pyr_lk_optical_flow_get_size_property(sparse.value, &width, &height) != OPENCV_CSHARP_STATUS_OK || width != 11 || height != 11 ||
            jyppx_ocv_sparse_pyr_lk_optical_flow_set_int_property(sparse.value, 0, 1) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_sparse_pyr_lk_optical_flow_get_int_property(sparse.value, 0, &int_value) != OPENCV_CSHARP_STATUS_OK || int_value != 1 ||
            jyppx_ocv_sparse_pyr_lk_optical_flow_set_term_criteria(sparse.value, 3, 12, 0.03) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_sparse_pyr_lk_optical_flow_get_term_criteria(sparse.value, &criteria_type, &criteria_count, &criteria_epsilon) != OPENCV_CSHARP_STATUS_OK ||
            criteria_type != 3 || criteria_count != 12 || std::abs(criteria_epsilon - 0.03) > 0.000001 ||
            jyppx_ocv_sparse_pyr_lk_optical_flow_set_min_eig_threshold(sparse.value, 0.00002) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_sparse_pyr_lk_optical_flow_get_min_eig_threshold(sparse.value, &double_value) != OPENCV_CSHARP_STATUS_OK || std::abs(double_value - 0.00002) > 0.000000001)
        {
            return 795;
        }

        const jyppx_ocv_video_point2f previous_points[] = {{10.0F, 10.0F}, {15.0F, 15.0F}, {20.0F, 20.0F}};
        jyppx_ocv_video_point2f next_points[] = {{0.0F, 0.0F}, {0.0F, 0.0F}, {0.0F, 0.0F}};
        unsigned char point_status[3] = {};
        float point_error[3] = {};
        if (jyppx_ocv_sparse_optical_flow_calc(sparse.value, first.get(), second.get(), previous_points, 3, next_points, point_status, point_error) != OPENCV_CSHARP_STATUS_OK)
        {
            return 796;
        }

        if (jyppx_ocv_dense_optical_flow_calc(nullptr, first.get(), second.get(), packed_flow.get()) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT ||
            jyppx_ocv_sparse_optical_flow_calc(nullptr, first.get(), second.get(), previous_points, 3, next_points, point_status, point_error) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT ||
            jyppx_ocv_sparse_optical_flow_calc(sparse.value, first.get(), second.get(), previous_points, -1, next_points, point_status, point_error) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT ||
            jyppx_ocv_farneback_optical_flow_create(3, 0.5, 0, 9, 3, 5, 1.1, 0, nullptr) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT)
        {
            return 797;
        }

        jyppx_ocv_dense_optical_flow_release_handle(nullptr);
        jyppx_ocv_sparse_optical_flow_release_handle(nullptr);
        return 0;
    }

    bool create_video_tracking_frame(int offset, NativeMatHandle& frame)
    {
        if (jyppx_ocv_mat_create_with_scalar(80, 80, 0, 0.0, 0.0, 0.0, 0.0, frame.out()) != OPENCV_CSHARP_STATUS_OK)
        {
            return false;
        }
        unsigned char* data = nullptr;
        size_t step = 0;
        if (jyppx_ocv_mat_data(frame.get(), &data) != OPENCV_CSHARP_STATUS_OK || data == nullptr ||
            jyppx_ocv_mat_step(frame.get(), &step) != OPENCV_CSHARP_STATUS_OK)
        {
            return false;
        }
        for (int y = 0; y < 80; ++y)
        {
            for (int x = 0; x < 80; ++x)
            {
                data[(static_cast<size_t>(y) * step) + static_cast<size_t>(x)] = static_cast<unsigned char>((x * 3 + y * 5) & 31);
            }
        }
        for (int y = 22; y < 42; ++y)
        {
            for (int x = 20 + offset; x < 40 + offset; ++x)
            {
                data[(static_cast<size_t>(y) * step) + static_cast<size_t>(x)] = ((x + y) & 1) == 0 ? 240 : 96;
            }
        }
        return true;
    }

    int run_video_ecc_tracker_mil_smoke()
    {
        NativeMatHandle reference;
        NativeMatHandle mask;
        NativeMatHandle single_warp;
        NativeMatHandle dual_warp;
        NativeMatHandle multiscale_warp;
        if (!create_video_smoke_frame(0, reference) ||
            jyppx_ocv_mat_create_with_scalar(32, 32, 0, 255.0, 0.0, 0.0, 0.0, mask.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(single_warp.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(dual_warp.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(multiscale_warp.out()) != OPENCV_CSHARP_STATUS_OK)
        {
            return 802;
        }

        double score = 0.0;
        if (jyppx_ocv_video_compute_ecc(reference.get(), reference.get(), nullptr, &score) != OPENCV_CSHARP_STATUS_OK ||
            std::abs(score - 1.0) > 0.00001)
        {
            return 803;
        }
        if (jyppx_ocv_video_find_transform_ecc(reference.get(), reference.get(), single_warp.get(), 0, 3, 20, 0.000001, nullptr, 5, &score) != OPENCV_CSHARP_STATUS_OK ||
            score < 0.999 || score > 1.001)
        {
            return 804;
        }
        if (jyppx_ocv_video_find_transform_ecc_with_mask(reference.get(), reference.get(), mask.get(), mask.get(), dual_warp.get(), 0, 3, 20, 0.000001, 5, &score) != OPENCV_CSHARP_STATUS_OK ||
            score < 0.999 || score > 1.001)
        {
            return 805;
        }

        int motion_type = -1;
        int criteria_type = 0;
        int criteria_count = 0;
        double criteria_epsilon = 0.0;
        int gaussian_filter_size = 0;
        int level_count = 0;
        int interpolation = -1;
        if (jyppx_ocv_video_ecc_parameters_get_default(&motion_type, &criteria_type, &criteria_count, &criteria_epsilon, &gaussian_filter_size, &level_count, &interpolation) != OPENCV_CSHARP_STATUS_OK ||
            motion_type != 2 || criteria_type != 3 || criteria_count != 50 || std::abs(criteria_epsilon - 0.000001) > 0.000000001 ||
            gaussian_filter_size != 5 || level_count != 4 || interpolation != 1)
        {
            return 806;
        }
        const int iterations[] = { 4, 4, 4, 4 };
        if (jyppx_ocv_video_find_transform_ecc_multi_scale(reference.get(), reference.get(), multiscale_warp.get(), 0, 3, 20, 0.000001,
            iterations, 4, 5, 4, 1, mask.get(), mask.get(), &score) != OPENCV_CSHARP_STATUS_OK ||
            score < 0.999 || score > 1.001)
        {
            return 807;
        }
        if (jyppx_ocv_video_compute_ecc(nullptr, reference.get(), nullptr, &score) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT ||
            jyppx_ocv_video_find_transform_ecc_multi_scale(reference.get(), reference.get(), multiscale_warp.get(), 0, 3, 20, 0.000001,
                nullptr, 1, 5, 4, 1, nullptr, nullptr, &score) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT ||
            jyppx_ocv_video_ecc_parameters_get_default(nullptr, &criteria_type, &criteria_count, &criteria_epsilon, &gaussian_filter_size, &level_count, &interpolation) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT)
        {
            return 808;
        }

        jyppx_ocv_video_tracker_mil_params parameters{};
        if (jyppx_ocv_video_tracker_mil_get_default_params(&parameters) != OPENCV_CSHARP_STATUS_OK ||
            std::abs(parameters.sampler_init_in_radius - 3.0F) > 0.000001F ||
            parameters.sampler_init_max_neg_num != 65 ||
            std::abs(parameters.sampler_search_win_size - 25.0F) > 0.000001F ||
            parameters.feature_set_num_features != 250)
        {
            return 809;
        }
        parameters.sampler_init_max_neg_num = 20;
        NativeVideoTrackerHandle tracker;
        if (jyppx_ocv_video_tracker_mil_create(&parameters, &tracker.value) != OPENCV_CSHARP_STATUS_OK || tracker.value == nullptr)
        {
            return 810;
        }
        NativeMatHandle first_frame;
        NativeMatHandle second_frame;
        if (!create_video_tracking_frame(0, first_frame) || !create_video_tracking_frame(2, second_frame))
        {
            return 811;
        }
        jyppx_ocv_video_rect box{ 20, 22, 20, 20 };
        int found = 0;
        if (jyppx_ocv_video_tracker_update(tracker.value, second_frame.get(), &box, &found) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT)
        {
            return 812;
        }
        if (jyppx_ocv_video_tracker_init(tracker.value, first_frame.get(), box) != OPENCV_CSHARP_STATUS_OK)
        {
            return 813;
        }
        float tracking_score = 0.0F;
        if (jyppx_ocv_video_tracker_get_tracking_score(tracker.value, &tracking_score) != OPENCV_CSHARP_STATUS_OK ||
            std::abs(tracking_score + 1.0F) > 0.000001F)
        {
            return 814;
        }
        if (jyppx_ocv_video_tracker_update(tracker.value, second_frame.get(), &box, &found) != OPENCV_CSHARP_STATUS_OK ||
            (found != 0 && (box.width <= 0 || box.height <= 0)))
        {
            return 815;
        }
        if (jyppx_ocv_video_tracker_mil_create(nullptr, &tracker.value) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT ||
            jyppx_ocv_video_tracker_mil_get_default_params(nullptr) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT ||
            jyppx_ocv_video_tracker_get_tracking_score(tracker.value, nullptr) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT)
        {
            return 816;
        }
        jyppx_ocv_video_tracker_release_handle(nullptr);
        return 0;
    }

    int run_calib3d_upstream_parity_smoke()
    {
        NativeSubdiv2DHandle subdiv;
        if (jyppx_ocv_calib3d_subdiv2d_create_rect(0, 0, 100, 100, &subdiv.value) != OPENCV_CSHARP_STATUS_OK || subdiv.value == nullptr)
        {
            return 500;
        }

        const jyppx_ocv_calib3d_point2f points[] = {
            {10.0F, 10.0F}, {85.0F, 10.0F}, {85.0F, 85.0F}, {10.0F, 85.0F}, {48.0F, 48.0F}};
        if (jyppx_ocv_calib3d_subdiv2d_insert_points(subdiv.value, points, 5) != OPENCV_CSHARP_STATUS_OK)
        {
            return 501;
        }

        int edge_count = 0;
        if (jyppx_ocv_calib3d_subdiv2d_get_edge_list_count(subdiv.value, &edge_count) != OPENCV_CSHARP_STATUS_OK || edge_count <= 0)
        {
            return 502;
        }
        std::vector<jyppx_ocv_calib3d_vec4f> edges(static_cast<std::size_t>(edge_count));
        if (jyppx_ocv_calib3d_subdiv2d_get_edge_list_fill(subdiv.value, edges.data(), edge_count) != OPENCV_CSHARP_STATUS_OK)
        {
            return 503;
        }

        int triangle_count = 0;
        if (jyppx_ocv_calib3d_subdiv2d_get_triangle_list_count(subdiv.value, &triangle_count) != OPENCV_CSHARP_STATUS_OK || triangle_count <= 0)
        {
            return 504;
        }
        std::vector<jyppx_ocv_calib3d_vec6f> triangles(static_cast<std::size_t>(triangle_count));
        if (jyppx_ocv_calib3d_subdiv2d_get_triangle_list_fill(subdiv.value, triangles.data(), triangle_count) != OPENCV_CSHARP_STATUS_OK)
        {
            return 505;
        }

        int location = -2;
        int located_edge = 0;
        int located_vertex = 0;
        if (jyppx_ocv_calib3d_subdiv2d_locate(subdiv.value, 48.0F, 48.0F, &location, &located_edge, &located_vertex) != OPENCV_CSHARP_STATUS_OK || location != 1 || located_vertex <= 0)
        {
            return 506;
        }

        int nearest_vertex = 0;
        float nearest_x = 0.0F;
        float nearest_y = 0.0F;
        if (jyppx_ocv_calib3d_subdiv2d_find_nearest(subdiv.value, 47.0F, 47.0F, &nearest_vertex, &nearest_x, &nearest_y) != OPENCV_CSHARP_STATUS_OK ||
            nearest_vertex != located_vertex || nearest_x != 48.0F || nearest_y != 48.0F)
        {
            return 507;
        }

        int facet_count = 0;
        int facet_point_count = 0;
        if (jyppx_ocv_calib3d_subdiv2d_get_voronoi_facet_list_count(subdiv.value, &located_vertex, 1, &facet_count, &facet_point_count) != OPENCV_CSHARP_STATUS_OK ||
            facet_count != 1 || facet_point_count < 3)
        {
            return 508;
        }
        std::vector<int> facet_offsets(static_cast<std::size_t>(facet_count + 1));
        std::vector<jyppx_ocv_calib3d_point2f> facet_points(static_cast<std::size_t>(facet_point_count));
        std::vector<jyppx_ocv_calib3d_point2f> facet_centers(static_cast<std::size_t>(facet_count));
        if (jyppx_ocv_calib3d_subdiv2d_get_voronoi_facet_list_fill(
                subdiv.value,
                &located_vertex,
                1,
                facet_offsets.data(),
                facet_count + 1,
                facet_points.data(),
                facet_point_count,
                facet_centers.data(),
                facet_count) != OPENCV_CSHARP_STATUS_OK ||
            facet_offsets[1] != facet_point_count)
        {
            return 509;
        }

        jyppx_ocv_calib3d_usac_params params{};
        if (jyppx_ocv_calib3d_usac_params_get_default(&params) != OPENCV_CSHARP_STATUS_OK ||
            params.max_iterations != 5000 || params.score != 1 || params.final_polisher != 3)
        {
            return 510;
        }

        NativeMatHandle source_points;
        NativeMatHandle destination_points;
        NativeMatHandle mask;
        NativeMatHandle homography;
        if (jyppx_ocv_mat_create(4, 2, 5, source_points.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create(4, 2, 5, destination_points.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(mask.out()) != OPENCV_CSHARP_STATUS_OK)
        {
            return 511;
        }
        unsigned char* source_data = nullptr;
        unsigned char* destination_data = nullptr;
        if (jyppx_ocv_mat_data(source_points.get(), &source_data) != OPENCV_CSHARP_STATUS_OK || source_data == nullptr ||
            jyppx_ocv_mat_data(destination_points.get(), &destination_data) != OPENCV_CSHARP_STATUS_OK || destination_data == nullptr)
        {
            return 512;
        }
        const float source_values[] = {0.0F, 0.0F, 20.0F, 0.0F, 20.0F, 20.0F, 0.0F, 20.0F};
        const float destination_values[] = {3.0F, 4.0F, 23.0F, 4.0F, 23.0F, 24.0F, 3.0F, 24.0F};
        std::memcpy(source_data, source_values, sizeof(source_values));
        std::memcpy(destination_data, destination_values, sizeof(destination_values));
        int homography_status = jyppx_ocv_calib3d_find_homography_usac(
            source_points.get(), destination_points.get(), mask.get(), &params, homography.out());
        if (homography_status != OPENCV_CSHARP_STATUS_OK || homography.get() == nullptr)
        {
            return 513;
        }
        int rows = 0;
        int cols = 0;
        if (jyppx_ocv_mat_rows(homography.get(), &rows) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_cols(homography.get(), &cols) != OPENCV_CSHARP_STATUS_OK || rows != 3 || cols != 3)
        {
            return 514;
        }
        return 0;
    }
#endif

    struct NativeGeneralizedHoughHandle
    {
        jyppx_ocv_generalized_hough* value = nullptr;

        NativeGeneralizedHoughHandle() = default;
        NativeGeneralizedHoughHandle(const NativeGeneralizedHoughHandle&) = delete;
        NativeGeneralizedHoughHandle& operator=(const NativeGeneralizedHoughHandle&) = delete;

        ~NativeGeneralizedHoughHandle()
        {
            jyppx_ocv_imgproc_generalized_hough_release(value);
        }

        jyppx_ocv_generalized_hough* get() const noexcept
        {
            return value;
        }

        jyppx_ocv_generalized_hough** out() noexcept
        {
            return &value;
        }
    };

    struct NativeFontFaceHandle
    {
        jyppx_ocv_font_face* value = nullptr;

        NativeFontFaceHandle() = default;
        NativeFontFaceHandle(const NativeFontFaceHandle&) = delete;
        NativeFontFaceHandle& operator=(const NativeFontFaceHandle&) = delete;

        ~NativeFontFaceHandle()
        {
            jyppx_ocv_imgproc_font_face_release(value);
        }

        jyppx_ocv_font_face* get() const noexcept
        {
            return value;
        }

        jyppx_ocv_font_face** out() noexcept
        {
            return &value;
        }
    };

    int expect_int(int status, const int& actual, int expected, int error_code)
    {
        return status == OPENCV_CSHARP_STATUS_OK && actual == expected ? 0 : error_code;
    }

    int expect_size(int status, const size_t& actual, size_t expected, int error_code)
    {
        return status == OPENCV_CSHARP_STATUS_OK && actual == expected ? 0 : error_code;
    }

    int expect_mat_bytes(jyppx_ocv_mat* mat, const unsigned char* expected, size_t length, int error_code)
    {
        unsigned char* data = nullptr;
        if (jyppx_ocv_mat_data(mat, &data) != OPENCV_CSHARP_STATUS_OK || data == nullptr)
        {
            return error_code;
        }

        for (size_t i = 0; i < length; ++i)
        {
            if (data[i] != expected[i])
            {
                return error_code;
            }
        }

        return 0;
    }

    int fill_mat_bytes(jyppx_ocv_mat* mat, const unsigned char* source, size_t length, int error_code)
    {
        unsigned char* data = nullptr;
        if (jyppx_ocv_mat_data(mat, &data) != OPENCV_CSHARP_STATUS_OK || data == nullptr)
        {
            return error_code;
        }

        for (size_t i = 0; i < length; ++i)
        {
            data[i] = source[i];
        }

        return 0;
    }

    int run_mat_object_api_smoke()
    {
        NativeMatHandle scalar;
        if (jyppx_ocv_mat_create_with_scalar(2, 2, 0, 7.0, 0.0, 0.0, 0.0, scalar.out()) != OPENCV_CSHARP_STATUS_OK || scalar.get() == nullptr)
        {
            return 200;
        }

        int rows = 0;
        int cols = 0;
        int dims = 0;
        int channels = 0;
        int depth = 0;
        int type = 0;
        int is_continuous = 0;
        int is_submatrix = 0;
        size_t total = 0;
        size_t elem_size = 0;
        size_t elem_size1 = 0;
        size_t step = 0;
        size_t step1 = 0;

        if (expect_int(jyppx_ocv_mat_rows(scalar.get(), &rows), rows, 2, 201) != 0)
        {
            return 201;
        }

        if (expect_int(jyppx_ocv_mat_cols(scalar.get(), &cols), cols, 2, 202) != 0)
        {
            return 202;
        }

        if (expect_int(jyppx_ocv_mat_dims(scalar.get(), &dims), dims, 2, 203) != 0)
        {
            return 203;
        }

        if (expect_int(jyppx_ocv_mat_channels(scalar.get(), &channels), channels, 1, 204) != 0)
        {
            return 204;
        }

        if (expect_int(jyppx_ocv_mat_depth(scalar.get(), &depth), depth, 0, 205) != 0)
        {
            return 205;
        }

        if (expect_int(jyppx_ocv_mat_type(scalar.get(), &type), type, 0, 206) != 0)
        {
            return 206;
        }

        if (expect_size(jyppx_ocv_mat_total(scalar.get(), &total), total, 4, 207) != 0)
        {
            return 207;
        }

        if (expect_size(jyppx_ocv_mat_elem_size(scalar.get(), &elem_size), elem_size, 1, 208) != 0)
        {
            return 208;
        }

        if (expect_size(jyppx_ocv_mat_elem_size1(scalar.get(), &elem_size1), elem_size1, 1, 209) != 0)
        {
            return 209;
        }

        if (expect_size(jyppx_ocv_mat_step(scalar.get(), &step), step, 2, 210) != 0)
        {
            return 210;
        }

        if (expect_size(jyppx_ocv_mat_step1(scalar.get(), &step1), step1, 2, 211) != 0)
        {
            return 211;
        }

        if (expect_int(jyppx_ocv_mat_is_continuous(scalar.get(), &is_continuous), is_continuous, 1, 212) != 0)
        {
            return 212;
        }

        if (expect_int(jyppx_ocv_mat_is_submatrix(scalar.get(), &is_submatrix), is_submatrix, 0, 213) != 0)
        {
            return 213;
        }

        const unsigned char scalar_expected[] = { 7, 7, 7, 7 };
        if (expect_mat_bytes(scalar.get(), scalar_expected, 4, 214) != 0)
        {
            return 214;
        }

        NativeMatHandle in_place;
        if (jyppx_ocv_mat_create_empty(in_place.out()) != OPENCV_CSHARP_STATUS_OK || in_place.get() == nullptr)
        {
            return 215;
        }

        if (jyppx_ocv_mat_create_in_place(in_place.get(), 2, 3, 0) != OPENCV_CSHARP_STATUS_OK)
        {
            return 216;
        }

        if (jyppx_ocv_mat_set_to(in_place.get(), 4.0, 0.0, 0.0, 0.0) != OPENCV_CSHARP_STATUS_OK)
        {
            return 217;
        }

        const unsigned char in_place_expected[] = { 4, 4, 4, 4, 4, 4 };
        if (expect_mat_bytes(in_place.get(), in_place_expected, 6, 218) != 0)
        {
            return 218;
        }

        NativeMatHandle zeros;
        NativeMatHandle ones;
        NativeMatHandle eye;
        if (jyppx_ocv_mat_zeros(2, 2, 0, zeros.out()) != OPENCV_CSHARP_STATUS_OK || zeros.get() == nullptr)
        {
            return 219;
        }

        if (jyppx_ocv_mat_ones(2, 2, 0, ones.out()) != OPENCV_CSHARP_STATUS_OK || ones.get() == nullptr)
        {
            return 220;
        }

        if (jyppx_ocv_mat_eye(3, 3, 0, eye.out()) != OPENCV_CSHARP_STATUS_OK || eye.get() == nullptr)
        {
            return 221;
        }

        const unsigned char zeros_expected[] = { 0, 0, 0, 0 };
        const unsigned char ones_expected[] = { 1, 1, 1, 1 };
        const unsigned char eye_expected[] = { 1, 0, 0, 0, 1, 0, 0, 0, 1 };
        if (expect_mat_bytes(zeros.get(), zeros_expected, 4, 222) != 0)
        {
            return 222;
        }

        if (expect_mat_bytes(ones.get(), ones_expected, 4, 223) != 0)
        {
            return 223;
        }

        if (expect_mat_bytes(eye.get(), eye_expected, 9, 224) != 0)
        {
            return 224;
        }

        NativeMatHandle source;
        if (jyppx_ocv_mat_create(3, 4, 0, source.out()) != OPENCV_CSHARP_STATUS_OK || source.get() == nullptr)
        {
            return 225;
        }

        const unsigned char source_initial[] =
        {
            1, 2, 3, 4,
            5, 6, 7, 8,
            9, 10, 11, 12
        };
        if (fill_mat_bytes(source.get(), source_initial, 12, 226) != 0)
        {
            return 226;
        }

        NativeMatHandle clone;
        if (jyppx_ocv_mat_clone(source.get(), clone.out()) != OPENCV_CSHARP_STATUS_OK || clone.get() == nullptr)
        {
            return 227;
        }

        if (jyppx_ocv_mat_set_to(clone.get(), 9.0, 0.0, 0.0, 0.0) != OPENCV_CSHARP_STATUS_OK)
        {
            return 228;
        }

        if (expect_mat_bytes(source.get(), source_initial, 12, 229) != 0)
        {
            return 229;
        }

        const unsigned char clone_expected[] =
        {
            9, 9, 9, 9,
            9, 9, 9, 9,
            9, 9, 9, 9
        };
        if (expect_mat_bytes(clone.get(), clone_expected, 12, 230) != 0)
        {
            return 230;
        }

        NativeMatHandle copied;
        if (jyppx_ocv_mat_create_empty(copied.out()) != OPENCV_CSHARP_STATUS_OK || copied.get() == nullptr)
        {
            return 231;
        }

        if (jyppx_ocv_mat_copy_to(source.get(), copied.get()) != OPENCV_CSHARP_STATUS_OK)
        {
            return 232;
        }

        if (expect_int(jyppx_ocv_mat_rows(copied.get(), &rows), rows, 3, 233) != 0)
        {
            return 233;
        }

        if (expect_int(jyppx_ocv_mat_cols(copied.get(), &cols), cols, 4, 234) != 0)
        {
            return 234;
        }

        if (expect_mat_bytes(copied.get(), source_initial, 12, 235) != 0)
        {
            return 235;
        }

        NativeMatHandle roi;
        if (jyppx_ocv_mat_submat(source.get(), 1, 1, 2, 2, roi.out()) != OPENCV_CSHARP_STATUS_OK || roi.get() == nullptr)
        {
            return 236;
        }

        if (expect_int(jyppx_ocv_mat_rows(roi.get(), &rows), rows, 2, 237) != 0)
        {
            return 237;
        }

        if (expect_int(jyppx_ocv_mat_cols(roi.get(), &cols), cols, 2, 238) != 0)
        {
            return 238;
        }

        if (expect_int(jyppx_ocv_mat_is_submatrix(roi.get(), &is_submatrix), is_submatrix, 1, 239) != 0)
        {
            return 239;
        }

        if (expect_int(jyppx_ocv_mat_is_continuous(roi.get(), &is_continuous), is_continuous, 0, 240) != 0)
        {
            return 240;
        }

        if (jyppx_ocv_mat_set_to(roi.get(), 99.0, 0.0, 0.0, 0.0) != OPENCV_CSHARP_STATUS_OK)
        {
            return 241;
        }

        const unsigned char roi_source_expected[] =
        {
            1, 2, 3, 4,
            5, 99, 99, 8,
            9, 99, 99, 12
        };
        if (expect_mat_bytes(source.get(), roi_source_expected, 12, 242) != 0)
        {
            return 242;
        }

        NativeMatHandle row_range;
        if (jyppx_ocv_mat_row_range(source.get(), 1, 2, row_range.out()) != OPENCV_CSHARP_STATUS_OK || row_range.get() == nullptr)
        {
            return 243;
        }

        if (expect_int(jyppx_ocv_mat_rows(row_range.get(), &rows), rows, 1, 244) != 0)
        {
            return 244;
        }

        if (expect_int(jyppx_ocv_mat_cols(row_range.get(), &cols), cols, 4, 245) != 0)
        {
            return 245;
        }

        if (expect_int(jyppx_ocv_mat_is_submatrix(row_range.get(), &is_submatrix), is_submatrix, 1, 246) != 0)
        {
            return 246;
        }

        if (jyppx_ocv_mat_set_to(row_range.get(), 55.0, 0.0, 0.0, 0.0) != OPENCV_CSHARP_STATUS_OK)
        {
            return 247;
        }

        const unsigned char row_source_expected[] =
        {
            1, 2, 3, 4,
            55, 55, 55, 55,
            9, 99, 99, 12
        };
        if (expect_mat_bytes(source.get(), row_source_expected, 12, 248) != 0)
        {
            return 248;
        }

        NativeMatHandle col_range;
        if (jyppx_ocv_mat_col_range(source.get(), 2, 3, col_range.out()) != OPENCV_CSHARP_STATUS_OK || col_range.get() == nullptr)
        {
            return 249;
        }

        if (expect_int(jyppx_ocv_mat_rows(col_range.get(), &rows), rows, 3, 250) != 0)
        {
            return 250;
        }

        if (expect_int(jyppx_ocv_mat_cols(col_range.get(), &cols), cols, 1, 251) != 0)
        {
            return 251;
        }

        if (expect_int(jyppx_ocv_mat_is_submatrix(col_range.get(), &is_submatrix), is_submatrix, 1, 252) != 0)
        {
            return 252;
        }

        if (jyppx_ocv_mat_set_to(col_range.get(), 44.0, 0.0, 0.0, 0.0) != OPENCV_CSHARP_STATUS_OK)
        {
            return 253;
        }

        const unsigned char col_source_expected[] =
        {
            1, 2, 44, 4,
            55, 55, 44, 55,
            9, 99, 44, 12
        };
        if (expect_mat_bytes(source.get(), col_source_expected, 12, 254) != 0)
        {
            return 254;
        }

        NativeMatHandle reshape_source;
        if (jyppx_ocv_mat_create(2, 3, 0, reshape_source.out()) != OPENCV_CSHARP_STATUS_OK || reshape_source.get() == nullptr)
        {
            return 255;
        }

        const unsigned char reshape_bytes[] = { 1, 2, 3, 4, 5, 6 };
        if (fill_mat_bytes(reshape_source.get(), reshape_bytes, 6, 256) != 0)
        {
            return 256;
        }

        NativeMatHandle reshaped;
        if (jyppx_ocv_mat_reshape(reshape_source.get(), 3, 2, reshaped.out()) != OPENCV_CSHARP_STATUS_OK || reshaped.get() == nullptr)
        {
            return 257;
        }

        if (expect_int(jyppx_ocv_mat_rows(reshaped.get(), &rows), rows, 2, 258) != 0)
        {
            return 258;
        }

        if (expect_int(jyppx_ocv_mat_cols(reshaped.get(), &cols), cols, 1, 259) != 0)
        {
            return 259;
        }

        if (expect_int(jyppx_ocv_mat_channels(reshaped.get(), &channels), channels, 3, 260) != 0)
        {
            return 260;
        }

        if (expect_int(jyppx_ocv_mat_type(reshaped.get(), &type), type, 64, 261) != 0)
        {
            return 261;
        }

        if (expect_size(jyppx_ocv_mat_elem_size(reshaped.get(), &elem_size), elem_size, 3, 262) != 0)
        {
            return 262;
        }

        if (expect_size(jyppx_ocv_mat_step1(reshaped.get(), &step1), step1, 3, 263) != 0)
        {
            return 263;
        }

        if (expect_mat_bytes(reshaped.get(), reshape_bytes, 6, 264) != 0)
        {
            return 264;
        }

        return 0;
    }

    int run_imgproc_filter_transform_api_smoke()
    {
        NativeMatHandle src;
        NativeMatHandle dst;
        if (jyppx_ocv_mat_create(4, 4, 0, src.out()) != OPENCV_CSHARP_STATUS_OK || src.get() == nullptr)
        {
            return 300;
        }

        if (jyppx_ocv_mat_create_empty(dst.out()) != OPENCV_CSHARP_STATUS_OK || dst.get() == nullptr)
        {
            return 301;
        }

        const unsigned char source_pixels[] =
        {
            1, 2, 3, 4,
            5, 6, 7, 8,
            9, 10, 11, 12,
            13, 14, 15, 16
        };
        if (fill_mat_bytes(src.get(), source_pixels, 16, 302) != 0)
        {
            return 302;
        }

        if (jyppx_ocv_imgproc_blur(src.get(), dst.get(), 3, 3, -1, -1, 4) != OPENCV_CSHARP_STATUS_OK)
        {
            return 303;
        }

        if (jyppx_ocv_imgproc_box_filter(src.get(), dst.get(), -1, 3, 3, -1, -1, 1, 4) != OPENCV_CSHARP_STATUS_OK)
        {
            return 304;
        }

        if (jyppx_ocv_imgproc_sqr_box_filter(src.get(), dst.get(), 5, 3, 3, -1, -1, 1, 4) != OPENCV_CSHARP_STATUS_OK)
        {
            return 305;
        }

        if (jyppx_ocv_imgproc_median_blur(src.get(), dst.get(), 3) != OPENCV_CSHARP_STATUS_OK)
        {
            return 306;
        }

        if (jyppx_ocv_imgproc_bilateral_filter(src.get(), dst.get(), 3, 25.0, 25.0, 4) != OPENCV_CSHARP_STATUS_OK)
        {
            return 307;
        }

        NativeMatHandle kernel;
        if (jyppx_ocv_mat_create(1, 1, 5, kernel.out()) != OPENCV_CSHARP_STATUS_OK || kernel.get() == nullptr)
        {
            return 308;
        }

        unsigned char* kernel_data = nullptr;
        if (jyppx_ocv_mat_data(kernel.get(), &kernel_data) != OPENCV_CSHARP_STATUS_OK || kernel_data == nullptr)
        {
            return 309;
        }

        reinterpret_cast<float*>(kernel_data)[0] = 1.0F;
        if (jyppx_ocv_imgproc_filter2d(src.get(), dst.get(), -1, kernel.get(), -1, -1, 0.0, 4) != OPENCV_CSHARP_STATUS_OK)
        {
            return 310;
        }

        NativeMatHandle gaussian_kernel;
        if (jyppx_ocv_imgproc_get_gaussian_kernel(3, 0.0, 6, gaussian_kernel.out()) != OPENCV_CSHARP_STATUS_OK || gaussian_kernel.get() == nullptr)
        {
            return 311;
        }

        if (jyppx_ocv_imgproc_sep_filter2d(src.get(), dst.get(), -1, gaussian_kernel.get(), gaussian_kernel.get(), -1, -1, 0.0, 4) != OPENCV_CSHARP_STATUS_OK)
        {
            return 312;
        }

        NativeMatHandle gabor_kernel;
        if (jyppx_ocv_imgproc_get_gabor_kernel(3, 3, 1.0, 0.0, 2.0, 0.5, 1.5707963267948966, 6, gabor_kernel.out()) != OPENCV_CSHARP_STATUS_OK || gabor_kernel.get() == nullptr)
        {
            return 313;
        }

        NativeMatHandle kx;
        NativeMatHandle ky;
        if (jyppx_ocv_mat_create_empty(kx.out()) != OPENCV_CSHARP_STATUS_OK || kx.get() == nullptr)
        {
            return 314;
        }

        if (jyppx_ocv_mat_create_empty(ky.out()) != OPENCV_CSHARP_STATUS_OK || ky.get() == nullptr)
        {
            return 315;
        }

        if (jyppx_ocv_imgproc_get_deriv_kernels(kx.get(), ky.get(), 1, 0, 3, 0, 5) != OPENCV_CSHARP_STATUS_OK)
        {
            return 316;
        }

        if (jyppx_ocv_imgproc_sobel(src.get(), dst.get(), 3, 1, 0, 3, 1.0, 0.0, 4) != OPENCV_CSHARP_STATUS_OK)
        {
            return 317;
        }

        if (jyppx_ocv_imgproc_scharr(src.get(), dst.get(), 3, 1, 0, 1.0, 0.0, 4) != OPENCV_CSHARP_STATUS_OK)
        {
            return 318;
        }

        if (jyppx_ocv_imgproc_laplacian(src.get(), dst.get(), 3, 1, 1.0, 0.0, 4) != OPENCV_CSHARP_STATUS_OK)
        {
            return 319;
        }

        if (jyppx_ocv_imgproc_canny(src.get(), dst.get(), 16.0, 64.0, 3, 0) != OPENCV_CSHARP_STATUS_OK)
        {
            return 320;
        }

        if (jyppx_ocv_imgproc_pyr_down(src.get(), dst.get(), 0, 0, 4) != OPENCV_CSHARP_STATUS_OK)
        {
            return 321;
        }

        int rows = 0;
        int cols = 0;
        if (expect_int(jyppx_ocv_mat_rows(dst.get(), &rows), rows, 2, 322) != 0)
        {
            return 322;
        }

        if (expect_int(jyppx_ocv_mat_cols(dst.get(), &cols), cols, 2, 323) != 0)
        {
            return 323;
        }

        if (jyppx_ocv_imgproc_pyr_up(dst.get(), dst.get(), 4, 4, 4) != OPENCV_CSHARP_STATUS_OK)
        {
            return 324;
        }

        NativeMatHandle rotation;
        if (jyppx_ocv_imgproc_get_rotation_matrix2d(0.0F, 0.0F, 0.0, 1.0, rotation.out()) != OPENCV_CSHARP_STATUS_OK || rotation.get() == nullptr)
        {
            return 325;
        }

        NativeMatHandle inverse_rotation;
        if (jyppx_ocv_mat_create_empty(inverse_rotation.out()) != OPENCV_CSHARP_STATUS_OK || inverse_rotation.get() == nullptr)
        {
            return 326;
        }

        if (jyppx_ocv_imgproc_invert_affine_transform(rotation.get(), inverse_rotation.get()) != OPENCV_CSHARP_STATUS_OK)
        {
            return 327;
        }

        if (jyppx_ocv_imgproc_warp_affine(src.get(), dst.get(), rotation.get(), 4, 4, 0, 0, 0.0, 0.0, 0.0, 0.0) != OPENCV_CSHARP_STATUS_OK)
        {
            return 328;
        }

        const float affine_src[] = { 0.0F, 0.0F, 3.0F, 0.0F, 0.0F, 3.0F };
        const float affine_dst[] = { 0.0F, 0.0F, 3.0F, 0.0F, 0.0F, 3.0F };
        if (jyppx_ocv_imgproc_get_affine_transform(affine_src, affine_dst, rotation.out()) != OPENCV_CSHARP_STATUS_OK || rotation.get() == nullptr)
        {
            return 329;
        }

        NativeMatHandle perspective;
        const float perspective_src[] = { 0.0F, 0.0F, 3.0F, 0.0F, 3.0F, 3.0F, 0.0F, 3.0F };
        const float perspective_dst[] = { 0.0F, 0.0F, 3.0F, 0.0F, 3.0F, 3.0F, 0.0F, 3.0F };
        if (jyppx_ocv_imgproc_get_perspective_transform(perspective_src, perspective_dst, 0, perspective.out()) != OPENCV_CSHARP_STATUS_OK || perspective.get() == nullptr)
        {
            return 330;
        }

        if (jyppx_ocv_imgproc_warp_perspective(src.get(), dst.get(), perspective.get(), 4, 4, 0, 0, 0.0, 0.0, 0.0, 0.0) != OPENCV_CSHARP_STATUS_OK)
        {
            return 331;
        }

        NativeMatHandle map_x;
        NativeMatHandle map_y;
        if (jyppx_ocv_mat_create(4, 4, 5, map_x.out()) != OPENCV_CSHARP_STATUS_OK || map_x.get() == nullptr)
        {
            return 332;
        }

        if (jyppx_ocv_mat_create(4, 4, 5, map_y.out()) != OPENCV_CSHARP_STATUS_OK || map_y.get() == nullptr)
        {
            return 333;
        }

        unsigned char* map_x_data = nullptr;
        unsigned char* map_y_data = nullptr;
        if (jyppx_ocv_mat_data(map_x.get(), &map_x_data) != OPENCV_CSHARP_STATUS_OK || map_x_data == nullptr)
        {
            return 334;
        }

        if (jyppx_ocv_mat_data(map_y.get(), &map_y_data) != OPENCV_CSHARP_STATUS_OK || map_y_data == nullptr)
        {
            return 335;
        }

        float* map_x_values = reinterpret_cast<float*>(map_x_data);
        float* map_y_values = reinterpret_cast<float*>(map_y_data);
        for (int y = 0; y < 4; ++y)
        {
            for (int x = 0; x < 4; ++x)
            {
                const int index = y * 4 + x;
                map_x_values[index] = static_cast<float>(x);
                map_y_values[index] = static_cast<float>(y);
            }
        }

        if (jyppx_ocv_imgproc_remap(src.get(), dst.get(), map_x.get(), map_y.get(), 0, 0, 0.0, 0.0, 0.0, 0.0) != OPENCV_CSHARP_STATUS_OK)
        {
            return 336;
        }

        if (expect_mat_bytes(dst.get(), source_pixels, 16, 337) != 0)
        {
            return 337;
        }

        NativeMatHandle converted_map1;
        NativeMatHandle converted_map2;
        if (jyppx_ocv_mat_create_empty(converted_map1.out()) != OPENCV_CSHARP_STATUS_OK || converted_map1.get() == nullptr)
        {
            return 338;
        }

        if (jyppx_ocv_mat_create_empty(converted_map2.out()) != OPENCV_CSHARP_STATUS_OK || converted_map2.get() == nullptr)
        {
            return 339;
        }

        if (jyppx_ocv_imgproc_convert_maps(map_x.get(), map_y.get(), converted_map1.get(), converted_map2.get(), 35, 1) != OPENCV_CSHARP_STATUS_OK)
        {
            return 340;
        }

        return 0;
    }

    int run_mini_excluded_features_smoke()
    {
#if defined(OPENCV_CSHARP_RUNTIME_PROFILE_MINI)
        NativeMatHandle image;
        if (jyppx_ocv_mat_create(4, 4, 0, image.out()) != OPENCV_CSHARP_STATUS_OK || image.get() == nullptr)
        {
            return 341;
        }

        int corner_count = -1;
        const int status = jyppx_ocv_imgproc_good_features_to_track_count(
            image.get(),
            nullptr,
            8,
            0.01,
            1.0,
            3,
            3,
            0,
            0.04,
            &corner_count);
        if (status != OPENCV_CSHARP_STATUS_NOT_LINKED || corner_count != 0)
        {
            return 342;
        }

        const char* error = jyppx_ocv_get_last_error();
        return error != nullptr && std::strlen(error) > 0 ? 0 : 343;
#else
        return 0;
#endif
    }

    int run_imgproc_upstream_parity_api_smoke()
    {
        NativeMatHandle gray;
        NativeMatHandle second;
        NativeMatHandle output;
        NativeMatHandle output2;
        NativeMatHandle mask;
        NativeMatHandle weights1;
        NativeMatHandle weights2;
        if (jyppx_ocv_mat_create_with_scalar(32, 32, 0, 32.0, 0.0, 0.0, 0.0, gray.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_with_scalar(32, 32, 0, 192.0, 0.0, 0.0, 0.0, second.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(output.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(output2.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_with_scalar(32, 32, 0, 255.0, 0.0, 0.0, 0.0, mask.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_with_scalar(32, 32, 5, 0.25, 0.0, 0.0, 0.0, weights1.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_with_scalar(32, 32, 5, 0.75, 0.0, 0.0, 0.0, weights2.out()) != OPENCV_CSHARP_STATUS_OK)
        {
            return 346;
        }

        if (jyppx_ocv_imgproc_apply_color_map(gray.get(), output.get(), 2) != OPENCV_CSHARP_STATUS_OK)
        {
            return 347;
        }
        int channels = 0;
        if (jyppx_ocv_mat_channels(output.get(), &channels) != OPENCV_CSHARP_STATUS_OK || channels != 3)
        {
            return 348;
        }

        if (jyppx_ocv_imgproc_blend_linear(gray.get(), second.get(), weights1.get(), weights2.get(), output.get()) != OPENCV_CSHARP_STATUS_OK)
        {
            return 349;
        }
        if (jyppx_ocv_imgproc_stack_blur(gray.get(), output.get(), 3, 3) != OPENCV_CSHARP_STATUS_OK)
        {
            return 350;
        }
        if (jyppx_ocv_imgproc_spatial_gradient(gray.get(), output.get(), output2.get(), 3, 4) != OPENCV_CSHARP_STATUS_OK)
        {
            return 351;
        }

        double threshold = 0.0;
        if (jyppx_ocv_imgproc_threshold_with_mask(gray.get(), second.get(), mask.get(), 64.0, 255.0, 0, &threshold) != OPENCV_CSHARP_STATUS_OK || threshold != 64.0)
        {
            return 352;
        }

        NativeMatHandle drawing;
        if (jyppx_ocv_mat_create_with_scalar(32, 32, 64, 0.0, 0.0, 0.0, 0.0, drawing.out()) != OPENCV_CSHARP_STATUS_OK)
        {
            return 353;
        }
        if (jyppx_ocv_imgproc_draw_marker(drawing.get(), 16, 16, 0.0, 255.0, 0.0, 0.0, 2, 11, 1, 8) != OPENCV_CSHARP_STATUS_OK)
        {
            return 354;
        }
        const int polygon[] = { 4, 4, 24, 4, 16, 24 };
        if (jyppx_ocv_imgproc_fill_convex_poly(drawing.get(), polygon, 3, 255.0, 0.0, 0.0, 0.0, 8, 0) != OPENCV_CSHARP_STATUS_OK)
        {
            return 355;
        }
        double font_scale = 0.0;
        if (jyppx_ocv_imgproc_get_font_scale_from_height(0, 18, 1, &font_scale) != OPENCV_CSHARP_STATUS_OK || font_scale <= 0.0)
        {
            return 356;
        }

        NativeMatHandle bayer;
        if (jyppx_ocv_mat_create_with_scalar(4, 4, 0, 128.0, 0.0, 0.0, 0.0, bayer.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_imgproc_demosaicing(bayer.get(), output.get(), 46, 0) != OPENCV_CSHARP_STATUS_OK)
        {
            return 357;
        }

        NativeMatHandle y_plane;
        NativeMatHandle uv_plane;
        if (jyppx_ocv_mat_create_with_scalar(4, 4, 0, 128.0, 0.0, 0.0, 0.0, y_plane.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_with_scalar(2, 2, 32, 128.0, 128.0, 0.0, 0.0, uv_plane.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_imgproc_cvt_color_two_plane(y_plane.get(), uv_plane.get(), output.get(), 91) != OPENCV_CSHARP_STATUS_OK)
        {
            return 358;
        }

        NativeMatHandle templ;
        NativeMatHandle image;
        NativeMatHandle positions;
        NativeGeneralizedHoughHandle ballard;
        if (jyppx_ocv_mat_create_with_scalar(12, 12, 0, 0.0, 0.0, 0.0, 0.0, templ.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_with_scalar(32, 32, 0, 0.0, 0.0, 0.0, 0.0, image.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(positions.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_imgproc_rectangle_by_rect(templ.get(), 2, 2, 8, 8, 255.0, 0.0, 0.0, 0.0, 1, 8, 0) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_imgproc_rectangle_by_rect(image.get(), 10, 10, 8, 8, 255.0, 0.0, 0.0, 0.0, 1, 8, 0) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_imgproc_generalized_hough_ballard_create(ballard.out()) != OPENCV_CSHARP_STATUS_OK || ballard.get() == nullptr)
        {
            return 359;
        }

        if (jyppx_ocv_imgproc_generalized_hough_set_int_property(ballard.get(), 0, 25) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_imgproc_generalized_hough_set_int_property(ballard.get(), 1, 75) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_imgproc_generalized_hough_set_int_property(ballard.get(), 3, 90) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_imgproc_generalized_hough_set_int_property(ballard.get(), 4, 1) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_imgproc_generalized_hough_set_template(ballard.get(), templ.get(), -1, -1) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_imgproc_generalized_hough_detect(ballard.get(), image.get(), positions.get(), nullptr) != OPENCV_CSHARP_STATUS_OK)
        {
            return 360;
        }
        int property_value = 0;
        if (jyppx_ocv_imgproc_generalized_hough_get_int_property(ballard.get(), 3, &property_value) != OPENCV_CSHARP_STATUS_OK || property_value != 90)
        {
            return 361;
        }

        NativeGeneralizedHoughHandle guil;
        if (jyppx_ocv_imgproc_generalized_hough_guil_create(guil.out()) != OPENCV_CSHARP_STATUS_OK || guil.get() == nullptr ||
            jyppx_ocv_imgproc_generalized_hough_set_double_property(guil.get(), 2, 80.0) != OPENCV_CSHARP_STATUS_OK)
        {
            return 362;
        }
        double double_property = 0.0;
        if (jyppx_ocv_imgproc_generalized_hough_get_double_property(guil.get(), 2, &double_property) != OPENCV_CSHARP_STATUS_OK || double_property != 80.0)
        {
            return 363;
        }

        if (jyppx_ocv_imgproc_generalized_hough_get_int_property(ballard.get(), 99, &property_value) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT ||
            jyppx_ocv_imgproc_draw_marker(nullptr, 0, 0, 0.0, 0.0, 0.0, 0.0, 0, 1, 1, 8) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT)
        {
            return 364;
        }
        return 0;
    }

    int run_imgproc_remaining_parity_api_smoke()
    {
        NativeMatHandle gray;
        NativeMatHandle patch;
        NativeMatHandle polar;
        if (jyppx_ocv_mat_zeros(16, 16, 0, gray.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(patch.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(polar.out()) != OPENCV_CSHARP_STATUS_OK)
        {
            return 365;
        }
        unsigned char* gray_data = nullptr;
        if (jyppx_ocv_mat_data(gray.get(), &gray_data) != OPENCV_CSHARP_STATUS_OK || gray_data == nullptr)
        {
            return 366;
        }
        gray_data[8 * 16 + 8] = 255;
        if (jyppx_ocv_imgproc_get_rect_sub_pix(gray.get(), 5, 5, 8.0F, 8.0F, patch.get(), -1) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_imgproc_warp_polar(gray.get(), polar.get(), 16, 16, 8.0F, 8.0F, 7.0, 1) != OPENCV_CSHARP_STATUS_OK)
        {
            return 367;
        }
        int rows = 0;
        int cols = 0;
        if (jyppx_ocv_mat_rows(patch.get(), &rows) != OPENCV_CSHARP_STATUS_OK || rows != 5 ||
            jyppx_ocv_mat_cols(patch.get(), &cols) != OPENCV_CSHARP_STATUS_OK || cols != 5)
        {
            return 368;
        }

        NativeMatHandle source_float;
        NativeMatHandle second_float;
        NativeMatHandle accumulator;
        NativeMatHandle hanning;
        if (jyppx_ocv_mat_zeros(8, 8, 5, source_float.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_zeros(8, 8, 5, second_float.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_zeros(8, 8, 5, accumulator.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(hanning.out()) != OPENCV_CSHARP_STATUS_OK)
        {
            return 369;
        }
        unsigned char* source_bytes = nullptr;
        unsigned char* second_bytes = nullptr;
        if (jyppx_ocv_mat_data(source_float.get(), &source_bytes) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_data(second_float.get(), &second_bytes) != OPENCV_CSHARP_STATUS_OK ||
            source_bytes == nullptr || second_bytes == nullptr)
        {
            return 370;
        }
        reinterpret_cast<float*>(source_bytes)[2 * 8 + 2] = 1.0F;
        reinterpret_cast<float*>(second_bytes)[3 * 8 + 4] = 1.0F;
        if (jyppx_ocv_imgproc_accumulate(source_float.get(), accumulator.get(), nullptr) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_imgproc_accumulate_square(source_float.get(), accumulator.get(), nullptr) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_imgproc_accumulate_product(source_float.get(), second_float.get(), accumulator.get(), nullptr) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_imgproc_accumulate_weighted(source_float.get(), accumulator.get(), 0.25, nullptr) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_imgproc_create_hanning_window(hanning.get(), 8, 8, 5) != OPENCV_CSHARP_STATUS_OK)
        {
            return 371;
        }
        double shift_x = 0.0;
        double shift_y = 0.0;
        double response = 0.0;
        if (jyppx_ocv_imgproc_phase_correlate(source_float.get(), second_float.get(), hanning.get(), &shift_x, &shift_y, &response) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_imgproc_phase_correlate_iterative(source_float.get(), second_float.get(), 7, 10, &shift_x, &shift_y) != OPENCV_CSHARP_STATUS_OK)
        {
            return 372;
        }

        NativeMatHandle signature1;
        NativeMatHandle signature2;
        NativeMatHandle flow;
        if (jyppx_ocv_mat_create(2, 2, 5, signature1.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create(2, 2, 5, signature2.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(flow.out()) != OPENCV_CSHARP_STATUS_OK)
        {
            return 373;
        }
        unsigned char* signature1_bytes = nullptr;
        unsigned char* signature2_bytes = nullptr;
        if (jyppx_ocv_mat_data(signature1.get(), &signature1_bytes) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_data(signature2.get(), &signature2_bytes) != OPENCV_CSHARP_STATUS_OK)
        {
            return 374;
        }
        const float signature1_values[] = { 1.0F, 0.0F, 1.0F, 1.0F };
        const float signature2_values[] = { 1.0F, 0.25F, 1.0F, 1.25F };
        std::memcpy(signature1_bytes, signature1_values, sizeof(signature1_values));
        std::memcpy(signature2_bytes, signature2_values, sizeof(signature2_values));
        float lower_bound = 0.0F;
        float emd_distance = 0.0F;
        if (jyppx_ocv_imgproc_emd(signature1.get(), signature2.get(), 2, nullptr, 1, &lower_bound, flow.get(), &emd_distance) != OPENCV_CSHARP_STATUS_OK || emd_distance <= 0.0F)
        {
            return 375;
        }

        NativeMatHandle color;
        NativeMatHandle filtered;
        NativeMatHandle markers;
        NativeMatHandle grab_mask;
        NativeMatHandle background_model;
        NativeMatHandle foreground_model;
        if (jyppx_ocv_mat_create_with_scalar(16, 16, 64, 64.0, 96.0, 128.0, 0.0, color.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(filtered.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_zeros(16, 16, 4, markers.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_zeros(16, 16, 0, grab_mask.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(background_model.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(foreground_model.out()) != OPENCV_CSHARP_STATUS_OK)
        {
            return 376;
        }
        unsigned char* color_bytes = nullptr;
        if (jyppx_ocv_mat_data(color.get(), &color_bytes) != OPENCV_CSHARP_STATUS_OK || color_bytes == nullptr)
        {
            return 394;
        }
        for (int y = 0; y < 16; ++y)
        {
            for (int x = 0; x < 16; ++x)
            {
                const bool inside = x >= 3 && x < 13 && y >= 3 && y < 13;
                const int offset = (y * 16 + x) * 3;
                color_bytes[offset] = static_cast<unsigned char>(inside ? 180 + (x % 5) : 10 + x);
                color_bytes[offset + 1] = static_cast<unsigned char>(inside ? 40 + (y % 7) : 15 + y);
                color_bytes[offset + 2] = static_cast<unsigned char>(inside ? 60 + ((x + y) % 9) : 20 + ((x + y) % 5));
            }
        }
        unsigned char* marker_bytes = nullptr;
        if (jyppx_ocv_mat_data(markers.get(), &marker_bytes) != OPENCV_CSHARP_STATUS_OK || marker_bytes == nullptr)
        {
            return 377;
        }
        reinterpret_cast<int*>(marker_bytes)[2 * 16 + 2] = 1;
        reinterpret_cast<int*>(marker_bytes)[13 * 16 + 13] = 2;
        if (jyppx_ocv_imgproc_pyr_mean_shift_filtering(color.get(), filtered.get(), 2.0, 8.0, 0, 3, 5, 1.0) != OPENCV_CSHARP_STATUS_OK)
        {
            return 391;
        }
        if (jyppx_ocv_imgproc_watershed(color.get(), markers.get()) != OPENCV_CSHARP_STATUS_OK)
        {
            return 392;
        }
        if (jyppx_ocv_imgproc_grab_cut(color.get(), grab_mask.get(), 2, 2, 12, 12, background_model.get(), foreground_model.get(), 1, 0) != OPENCV_CSHARP_STATUS_OK)
        {
            std::fprintf(stderr, "GrabCut smoke failed: %s\n", jyppx_ocv_get_last_error());
            return 393;
        }

        NativeMatHandle templ;
        NativeMatHandle match_result;
        if (jyppx_ocv_mat_zeros(3, 3, 0, templ.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(match_result.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_imgproc_match_template(gray.get(), templ.get(), match_result.get(), 5, nullptr) != OPENCV_CSHARP_STATUS_OK)
        {
            return 379;
        }

        NativeMatHandle binary;
        if (jyppx_ocv_mat_zeros(16, 16, 0, binary.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_imgproc_rectangle_by_rect(binary.get(), 3, 3, 8, 8, 255.0, 0.0, 0.0, 0.0, -1, 8, 0) != OPENCV_CSHARP_STATUS_OK)
        {
            return 380;
        }
        int contour_count = 0;
        int point_count = 0;
        int hierarchy_count = 0;
        if (jyppx_ocv_imgproc_find_contours_link_runs_count(binary.get(), 1, &contour_count, &point_count, &hierarchy_count) != OPENCV_CSHARP_STATUS_OK || contour_count <= 0 || point_count <= 0 || hierarchy_count != contour_count)
        {
            return 381;
        }
        int contours_xy[128] = {};
        int contour_lengths[16] = {};
        int hierarchy_values[64] = {};
        int written_contours = 0;
        int written_points = 0;
        int written_hierarchy = 0;
        if (jyppx_ocv_imgproc_find_contours_link_runs_fill(binary.get(), 1, contours_xy, 64, contour_lengths, 16, hierarchy_values, 16, &written_contours, &written_points, &written_hierarchy) != OPENCV_CSHARP_STATUS_OK ||
            written_contours != contour_count || written_points != point_count || written_hierarchy != hierarchy_count)
        {
            return 382;
        }

        NativeMatHandle camera_matrix;
        NativeMatHandle dist_coeffs;
        NativeMatHandle fisheye_coeffs;
        NativeMatHandle rectification;
        NativeMatHandle rectified;
        NativeMatHandle map1;
        NativeMatHandle map2;
        NativeMatHandle rotation_vector;
        NativeMatHandle translation_vector;
        if (jyppx_ocv_mat_eye(3, 3, 6, camera_matrix.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_zeros(1, 5, 6, dist_coeffs.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_zeros(1, 4, 6, fisheye_coeffs.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_eye(3, 3, 6, rectification.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(rectified.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(map1.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(map2.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_zeros(3, 1, 6, rotation_vector.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_zeros(3, 1, 6, translation_vector.out()) != OPENCV_CSHARP_STATUS_OK)
        {
            return 383;
        }
        unsigned char* camera_bytes = nullptr;
        unsigned char* translation_bytes = nullptr;
        if (jyppx_ocv_mat_data(camera_matrix.get(), &camera_bytes) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_data(translation_vector.get(), &translation_bytes) != OPENCV_CSHARP_STATUS_OK)
        {
            return 384;
        }
        double* camera_values = reinterpret_cast<double*>(camera_bytes);
        camera_values[0] = 12.0;
        camera_values[4] = 12.0;
        camera_values[2] = 8.0;
        camera_values[5] = 8.0;
        reinterpret_cast<double*>(translation_bytes)[2] = 2.0;
        if (jyppx_ocv_imgproc_undistort(color.get(), rectified.get(), camera_matrix.get(), dist_coeffs.get(), camera_matrix.get()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_imgproc_init_inverse_rectification_map(camera_matrix.get(), dist_coeffs.get(), rectification.get(), camera_matrix.get(), 16, 16, 5, map1.get(), map2.get()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_imgproc_fisheye_undistort_image(color.get(), rectified.get(), camera_matrix.get(), fisheye_coeffs.get(), camera_matrix.get(), 16, 16) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_imgproc_draw_frame_axes(color.get(), camera_matrix.get(), dist_coeffs.get(), rotation_vector.get(), translation_vector.get(), 0.5F, 1) != OPENCV_CSHARP_STATUS_OK)
        {
            return 385;
        }

        NativeFontFaceHandle font_face;
        const unsigned char sans_name[] = { 's', 'a', 'n', 's', 0 };
        if (jyppx_ocv_imgproc_font_face_create(sans_name, font_face.out()) != OPENCV_CSHARP_STATUS_OK || font_face.get() == nullptr)
        {
            return 386;
        }
        int font_set = 0;
        int name_size = 0;
        if (jyppx_ocv_imgproc_font_face_set(font_face.get(), sans_name, &font_set) != OPENCV_CSHARP_STATUS_OK || font_set == 0 ||
            jyppx_ocv_imgproc_font_face_get_name_size(font_face.get(), &name_size) != OPENCV_CSHARP_STATUS_OK || name_size <= 0)
        {
            return 387;
        }
        unsigned char name_buffer[64] = {};
        int name_written = 0;
        int instance_set = 0;
        int instance_count = 0;
        int instance_result = 0;
        if (jyppx_ocv_imgproc_font_face_get_name_fill(font_face.get(), name_buffer, 64, &name_written) != OPENCV_CSHARP_STATUS_OK || name_written != name_size ||
            jyppx_ocv_imgproc_font_face_set_instance(font_face.get(), nullptr, 0, &instance_set) != OPENCV_CSHARP_STATUS_OK || instance_set == 0 ||
            jyppx_ocv_imgproc_font_face_get_instance_count(font_face.get(), &instance_count, &instance_result) != OPENCV_CSHARP_STATUS_OK || instance_result == 0)
        {
            return 388;
        }
        const unsigned char text[] = { 'O', 'K', 0 };
        int next_x = 0;
        int next_y = 0;
        int text_x = 0;
        int text_y = 0;
        int text_width = 0;
        int text_height = 0;
        if (jyppx_ocv_imgproc_put_text_font_face(color.get(), text, 1, 12, 255.0, 255.0, 255.0, 0.0, font_face.get(), 10, 0, 0, 0, 0, 0, &next_x, &next_y) != OPENCV_CSHARP_STATUS_OK || next_x <= 1 ||
            jyppx_ocv_imgproc_get_text_size_font_face(16, 16, text, 1, 12, font_face.get(), 10, 0, 0, 0, 0, 0, &text_x, &text_y, &text_width, &text_height) != OPENCV_CSHARP_STATUS_OK || text_width <= 0 || text_height <= 0)
        {
            return 389;
        }

        if (jyppx_ocv_imgproc_get_rect_sub_pix(nullptr, 5, 5, 0.0F, 0.0F, patch.get(), -1) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT ||
            jyppx_ocv_imgproc_create_hanning_window(hanning.get(), 1, 8, 5) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT ||
            jyppx_ocv_imgproc_font_face_set_instance(font_face.get(), nullptr, 1, &instance_set) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT)
        {
            return 390;
        }

        return 0;
    }

#if !defined(OPENCV_CSHARP_RUNTIME_PROFILE_MINI)
    int run_imgcodecs_upstream_parity_api_smoke()
    {
        NativeMatHandle first;
        NativeMatHandle second;
        if (jyppx_ocv_mat_create(3, 4, 64, first.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create(2, 5, 64, second.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_set_to(first.get(), 10, 20, 30, 0) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_set_to(second.get(), 40, 50, 60, 0) != OPENCV_CSHARP_STATUS_OK)
        {
            return 400;
        }

        const jyppx_ocv_mat* pages[] = {first.get(), second.get()};
        jyppx_ocv_encoded_buffer* tiff = nullptr;
        if (jyppx_ocv_imgcodecs_imencode_multi(".tiff", pages, 2, nullptr, 0, &tiff) != OPENCV_CSHARP_STATUS_OK || tiff == nullptr)
        {
            return 401;
        }

        const unsigned char* tiff_data = nullptr;
        size_t tiff_size = 0;
        if (jyppx_ocv_encoded_buffer_size(tiff, &tiff_size) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_encoded_buffer_data(tiff, &tiff_data) != OPENCV_CSHARP_STATUS_OK ||
            tiff_data == nullptr || tiff_size == 0)
        {
            jyppx_ocv_encoded_buffer_release(tiff);
            return 402;
        }

        jyppx_ocv_imgcodecs_mat_vector* decoded = nullptr;
        int success = 0;
        if (jyppx_ocv_imgcodecs_imdecode_multi(tiff_data, tiff_size, -1, 0, 0, 0, &decoded, &success) != OPENCV_CSHARP_STATUS_OK ||
            decoded == nullptr || success != 1)
        {
            jyppx_ocv_encoded_buffer_release(tiff);
            return 403;
        }

        size_t page_count = 0;
        NativeMatHandle cloned_page;
        if (jyppx_ocv_imgcodecs_mat_vector_count(decoded, &page_count) != OPENCV_CSHARP_STATUS_OK || page_count != 2 ||
            jyppx_ocv_imgcodecs_mat_vector_clone_at(decoded, 1, cloned_page.out()) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_imgcodecs_mat_vector_release(decoded);
            jyppx_ocv_encoded_buffer_release(tiff);
            return 404;
        }
        jyppx_ocv_imgcodecs_mat_vector_release(decoded);
        jyppx_ocv_encoded_buffer_release(tiff);

        jyppx_ocv_imgcodecs_metadata_result* metadata_result = nullptr;
        jyppx_ocv_encoded_buffer* png = nullptr;
        if (jyppx_ocv_imgcodecs_imencode_with_metadata(".png", first.get(), nullptr, nullptr, 0, nullptr, 0, &png) != OPENCV_CSHARP_STATUS_OK ||
            png == nullptr ||
            jyppx_ocv_encoded_buffer_size(png, &tiff_size) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_encoded_buffer_data(png, &tiff_data) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_imgcodecs_imdecode_with_metadata(tiff_data, tiff_size, -1, &metadata_result) != OPENCV_CSHARP_STATUS_OK ||
            metadata_result == nullptr)
        {
            jyppx_ocv_encoded_buffer_release(png);
            return 405;
        }
        size_t metadata_count = 99;
        if (jyppx_ocv_imgcodecs_metadata_result_count(metadata_result, &metadata_count) != OPENCV_CSHARP_STATUS_OK || metadata_count != 0)
        {
            jyppx_ocv_imgcodecs_metadata_result_release(metadata_result);
            jyppx_ocv_encoded_buffer_release(png);
            return 406;
        }
        jyppx_ocv_imgcodecs_metadata_result_release(metadata_result);
        jyppx_ocv_encoded_buffer_release(png);

        jyppx_ocv_imgcodecs_animation* animation = nullptr;
        if (jyppx_ocv_imgcodecs_animation_create(2, 1, 2, 3, 4, &animation) != OPENCV_CSHARP_STATUS_OK || animation == nullptr)
        {
            return 407;
        }
        const jyppx_ocv_mat* animation_frames[] = {first.get(), first.get()};
        const int durations[] = {40, 80};
        if (jyppx_ocv_imgcodecs_animation_set_frames(animation, animation_frames, durations, 2) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_imgcodecs_animation_release(animation);
            return 408;
        }
        jyppx_ocv_encoded_buffer* gif = nullptr;
        if (jyppx_ocv_imgcodecs_imencode_animation(".gif", animation, nullptr, 0, &gif) != OPENCV_CSHARP_STATUS_OK || gif == nullptr)
        {
            jyppx_ocv_imgcodecs_animation_release(animation);
            return 409;
        }
        jyppx_ocv_imgcodecs_animation* decoded_animation = nullptr;
        if (jyppx_ocv_imgcodecs_animation_create(0, 0, 0, 0, 0, &decoded_animation) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_encoded_buffer_size(gif, &tiff_size) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_encoded_buffer_data(gif, &tiff_data) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_imgcodecs_imdecode_animation(tiff_data, tiff_size, 0, 32767, decoded_animation, &success) != OPENCV_CSHARP_STATUS_OK ||
            success != 1)
        {
            jyppx_ocv_imgcodecs_animation_release(decoded_animation);
            jyppx_ocv_encoded_buffer_release(gif);
            jyppx_ocv_imgcodecs_animation_release(animation);
            return 410;
        }
        size_t frame_count = 0;
        int duration = 0;
        NativeMatHandle cloned_frame;
        if (jyppx_ocv_imgcodecs_animation_frame_count(decoded_animation, &frame_count) != OPENCV_CSHARP_STATUS_OK || frame_count != 2 ||
            jyppx_ocv_imgcodecs_animation_frame_clone_at(decoded_animation, 1, cloned_frame.out(), &duration) != OPENCV_CSHARP_STATUS_OK || duration != 80)
        {
            jyppx_ocv_imgcodecs_animation_release(decoded_animation);
            jyppx_ocv_encoded_buffer_release(gif);
            jyppx_ocv_imgcodecs_animation_release(animation);
            return 411;
        }

        jyppx_ocv_imgcodecs_animation_release(decoded_animation);
        jyppx_ocv_encoded_buffer_release(gif);
        jyppx_ocv_imgcodecs_animation_release(animation);
        return 0;
    }
#endif

    int run_core_upstream_parity_smoke()
    {
        NativeMatHandle src;
        NativeMatHandle dst;
        NativeMatHandle mask;
        if (jyppx_ocv_mat_create(2, 3, 4, src.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(dst.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create(2, 3, 0, mask.out()) != OPENCV_CSHARP_STATUS_OK)
        {
            return 420;
        }

        unsigned char* src_bytes = nullptr;
        unsigned char* mask_bytes = nullptr;
        if (jyppx_ocv_mat_data(src.get(), &src_bytes) != OPENCV_CSHARP_STATUS_OK || src_bytes == nullptr ||
            jyppx_ocv_mat_data(mask.get(), &mask_bytes) != OPENCV_CSHARP_STATUS_OK || mask_bytes == nullptr)
        {
            return 421;
        }
        const int values[] = { 3, 1, 1, 2, 4, 0 };
        std::memcpy(src_bytes, values, sizeof(values));
        const unsigned char mask_values[] = { 255, 0, 255, 0, 255, 0 };
        std::memcpy(mask_bytes, mask_values, sizeof(mask_values));

        int has_non_zero = 0;
        int border_value = -1;
        if (jyppx_ocv_core_has_non_zero(src.get(), &has_non_zero) != OPENCV_CSHARP_STATUS_OK)
        {
            return 422;
        }
        if (has_non_zero != 1)
        {
            return 428;
        }
        if (jyppx_ocv_core_border_interpolate(-1, 3, 4, &border_value) != OPENCV_CSHARP_STATUS_OK || border_value != 1)
        {
            return 429;
        }
        if (jyppx_ocv_core_reduce_arg_min(src.get(), dst.get(), 1, 0) != OPENCV_CSHARP_STATUS_OK)
        {
            return 430;
        }
        unsigned char* dst_bytes = nullptr;
        if (jyppx_ocv_mat_data(dst.get(), &dst_bytes) != OPENCV_CSHARP_STATUS_OK || dst_bytes == nullptr ||
            reinterpret_cast<int*>(dst_bytes)[0] != 1 || reinterpret_cast<int*>(dst_bytes)[1] != 2)
        {
            return 423;
        }

        if (jyppx_ocv_core_sort(src.get(), dst.get(), 16) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_copy_to_mask(src.get(), dst.get(), mask.get()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_find_non_zero(mask.get(), dst.get()) != OPENCV_CSHARP_STATUS_OK)
        {
            return 424;
        }

        double psnr = 0.0;
        int valid = 0;
        int x = -1;
        int y = -1;
        if (jyppx_ocv_core_psnr(src.get(), src.get(), 255.0, &psnr) != OPENCV_CSHARP_STATUS_OK || psnr <= 300.0 ||
            jyppx_ocv_core_check_range(src.get(), 0.0, 5.0, &valid, &x, &y) != OPENCV_CSHARP_STATUS_OK || valid != 1)
        {
            return 425;
        }

        const int order[] = { 1, 0 };
        if (jyppx_ocv_core_flip_nd(src.get(), dst.get(), -1) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_transpose_nd(src.get(), order, 2, dst.get()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_copy_make_border(src.get(), dst.get(), 1, 1, 1, 1, 0, 0.0, 0.0, 0.0, 0.0) != OPENCV_CSHARP_STATUS_OK)
        {
            return 426;
        }

        if (jyppx_ocv_core_has_non_zero(nullptr, &has_non_zero) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT ||
            jyppx_ocv_core_transpose_nd(src.get(), nullptr, 0, dst.get()) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT ||
            jyppx_ocv_core_check_range(src.get(), 1.0, 1.0, &valid, &x, &y) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT)
        {
            return 427;
        }
        return 0;
    }

    int run_core_persistence_smoke()
    {
        constexpr int storage_write_memory_yaml = 1 | 4 | 16;
        constexpr int storage_read_memory_yaml = 4 | 16;
        constexpr int file_node_sequence = 4;
        constexpr int file_node_map = 5;
        const auto* hint = reinterpret_cast<const unsigned char*>("memory.yml");

        NativeFileStorageHandle writer;
        int opened = 0;
        if (jyppx_ocv_core_file_storage_create(&writer.value) != OPENCV_CSHARP_STATUS_OK || writer.value == nullptr ||
            jyppx_ocv_core_file_storage_open(writer.value, hint, 10, storage_write_memory_yaml, nullptr, 0, &opened) != OPENCV_CSHARP_STATUS_OK || opened != 1)
        {
            return 600;
        }

        int format = 0;
        if (jyppx_ocv_core_file_storage_get_format(writer.value, &format) != OPENCV_CSHARP_STATUS_OK || format != 16)
        {
            return 601;
        }

        NativeMatHandle matrix;
        if (jyppx_ocv_mat_create(2, 2, 4, matrix.out()) != OPENCV_CSHARP_STATUS_OK)
        {
            return 602;
        }
        unsigned char* matrix_bytes = nullptr;
        if (jyppx_ocv_mat_data(matrix.get(), &matrix_bytes) != OPENCV_CSHARP_STATUS_OK || matrix_bytes == nullptr)
        {
            return 603;
        }
        const int matrix_values[] = { 2, 4, 6, 8 };
        std::memcpy(matrix_bytes, matrix_values, sizeof(matrix_values));

        const unsigned char utf8_text[] = { 'h', 0xc3, 0xa9, 'l', 'l', 'o', '-', 0xe4, 0xb8, 0xad };
        const unsigned char flattened_values[] = { 'a', 'l', 'p', 'h', 'a', 0xe4, 0xb9, 0x99 };
        const int value_offsets[] = { 0, 5, 5 };
        const int value_lengths[] = { 5, 0, 3 };
        const int empty_offsets[] = { 0, 0 };
        const int empty_lengths[] = { 0, 0 };
        if (jyppx_ocv_core_file_storage_write_comment(writer.value, reinterpret_cast<const unsigned char*>("persistence smoke"), 17, 0) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_file_storage_write_int(writer.value, reinterpret_cast<const unsigned char*>("count"), 5, 7) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_file_storage_write_bool(writer.value, reinterpret_cast<const unsigned char*>("enabled"), 7, 1) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_file_storage_write_int64(writer.value, reinterpret_cast<const unsigned char*>("large"), 5, 1234567890123LL) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_file_storage_write_double(writer.value, reinterpret_cast<const unsigned char*>("score"), 5, 2.5) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_file_storage_write_string(writer.value, reinterpret_cast<const unsigned char*>("text"), 4, utf8_text, static_cast<int>(sizeof(utf8_text))) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_file_storage_write_mat(writer.value, reinterpret_cast<const unsigned char*>("matrix"), 6, matrix.get()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_file_storage_write_string_vector(writer.value, reinterpret_cast<const unsigned char*>("words"), 5, flattened_values, static_cast<int>(sizeof(flattened_values)), value_offsets, value_lengths, 3) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_file_storage_write_string_vector(writer.value, reinterpret_cast<const unsigned char*>("empty_words"), 11, nullptr, 0, empty_offsets, empty_lengths, 2) != OPENCV_CSHARP_STATUS_OK)
        {
            return 604;
        }

        if (jyppx_ocv_core_file_storage_start_write_struct(writer.value, reinterpret_cast<const unsigned char*>("metadata"), 8, file_node_map, nullptr, 0) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_file_storage_write_string(writer.value, reinterpret_cast<const unsigned char*>("owner"), 5, reinterpret_cast<const unsigned char*>("native"), 6) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_file_storage_end_write_struct(writer.value) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_file_storage_start_write_struct(writer.value, reinterpret_cast<const unsigned char*>("values"), 6, file_node_sequence, nullptr, 0) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_file_storage_write_int(writer.value, nullptr, 0, 11) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_file_storage_write_int(writer.value, nullptr, 0, 13) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_file_storage_end_write_struct(writer.value) != OPENCV_CSHARP_STATUS_OK)
        {
            return 605;
        }

        NativeUtf8ResultHandle document;
        if (jyppx_ocv_core_file_storage_release_and_get_string(writer.value, &document.value) != OPENCV_CSHARP_STATUS_OK || document.value == nullptr)
        {
            return 606;
        }
        size_t document_size = 0;
        const unsigned char* document_data = nullptr;
        if (jyppx_ocv_core_utf8_result_size(document.value, &document_size) != OPENCV_CSHARP_STATUS_OK || document_size == 0 ||
            jyppx_ocv_core_utf8_result_data(document.value, &document_data) != OPENCV_CSHARP_STATUS_OK || document_data == nullptr)
        {
            return 607;
        }

        NativeFileStorageHandle reader;
        if (jyppx_ocv_core_file_storage_create(&reader.value) != OPENCV_CSHARP_STATUS_OK ||
            document_size > static_cast<size_t>((std::numeric_limits<int>::max)()) ||
            jyppx_ocv_core_file_storage_open(reader.value, document_data, static_cast<int>(document_size), storage_read_memory_yaml, nullptr, 0, &opened) != OPENCV_CSHARP_STATUS_OK || opened != 1)
        {
            return 608;
        }

        NativeFileNodeHandle count_node;
        NativeFileNodeHandle text_node;
        NativeFileNodeHandle words_node;
        NativeFileNodeHandle matrix_node;
        NativeFileNodeHandle map_node;
        NativeFileNodeHandle sequence_node;
        NativeFileNodeHandle root_node;
        if (jyppx_ocv_core_file_storage_get_node(reader.value, reinterpret_cast<const unsigned char*>("count"), 5, &count_node.value) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_file_storage_get_node(reader.value, reinterpret_cast<const unsigned char*>("text"), 4, &text_node.value) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_file_storage_get_node(reader.value, reinterpret_cast<const unsigned char*>("words"), 5, &words_node.value) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_file_storage_get_node(reader.value, reinterpret_cast<const unsigned char*>("matrix"), 6, &matrix_node.value) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_file_storage_get_node(reader.value, reinterpret_cast<const unsigned char*>("metadata"), 8, &map_node.value) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_file_storage_get_node(reader.value, reinterpret_cast<const unsigned char*>("values"), 6, &sequence_node.value) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_file_storage_root(reader.value, 0, &root_node.value) != OPENCV_CSHARP_STATUS_OK)
        {
            return 609;
        }

        double numeric_value = 0.0;
        int node_type = 0;
        size_t node_size = 0;
        if (jyppx_ocv_core_file_node_real(count_node.value, &numeric_value) != OPENCV_CSHARP_STATUS_OK || numeric_value != 7.0 ||
            jyppx_ocv_core_file_node_type(map_node.value, &node_type) != OPENCV_CSHARP_STATUS_OK || node_type != file_node_map ||
            jyppx_ocv_core_file_node_size(sequence_node.value, &node_size) != OPENCV_CSHARP_STATUS_OK || node_size != 2)
        {
            return 610;
        }

        NativeUtf8ResultHandle text_result;
        NativeUtf8ResultHandle name_result;
        if (jyppx_ocv_core_file_node_string(text_node.value, &text_result.value) != OPENCV_CSHARP_STATUS_OK ||
            !utf8_result_equals(text_result.value, utf8_text, sizeof(utf8_text)) ||
            jyppx_ocv_core_file_node_name(text_node.value, &name_result.value) != OPENCV_CSHARP_STATUS_OK ||
            !utf8_result_equals(name_result.value, reinterpret_cast<const unsigned char*>("text"), 4))
        {
            return 611;
        }

        NativeStringListHandle keys;
        size_t key_count = 0;
        if (jyppx_ocv_core_file_node_keys(root_node.value, &keys.value) != OPENCV_CSHARP_STATUS_OK || keys.value == nullptr ||
            jyppx_ocv_core_string_list_count(keys.value, &key_count) != OPENCV_CSHARP_STATUS_OK || key_count != 10)
        {
            return 612;
        }
        NativeUtf8ResultHandle first_key;
        if (jyppx_ocv_core_string_list_get(keys.value, 0, &first_key.value) != OPENCV_CSHARP_STATUS_OK ||
            !utf8_result_equals(first_key.value, reinterpret_cast<const unsigned char*>("count"), 5))
        {
            return 613;
        }

        NativeFileNodeHandle empty_word;
        NativeFileNodeHandle map_owner;
        NativeFileNodeHandle sequence_item;
        if (jyppx_ocv_core_file_node_at(words_node.value, 1, &empty_word.value) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_file_node_get_node(map_node.value, reinterpret_cast<const unsigned char*>("owner"), 5, &map_owner.value) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_file_node_at(sequence_node.value, 1, &sequence_item.value) != OPENCV_CSHARP_STATUS_OK)
        {
            return 614;
        }
        NativeUtf8ResultHandle empty_string;
        NativeUtf8ResultHandle owner_string;
        if (jyppx_ocv_core_file_node_string(empty_word.value, &empty_string.value) != OPENCV_CSHARP_STATUS_OK ||
            !utf8_result_equals(empty_string.value, nullptr, 0) ||
            jyppx_ocv_core_file_node_string(map_owner.value, &owner_string.value) != OPENCV_CSHARP_STATUS_OK ||
            !utf8_result_equals(owner_string.value, reinterpret_cast<const unsigned char*>("native"), 6) ||
            jyppx_ocv_core_file_node_real(sequence_item.value, &numeric_value) != OPENCV_CSHARP_STATUS_OK || numeric_value != 13.0)
        {
            return 615;
        }

        NativeMatHandle decoded_matrix;
        int rows = 0;
        int cols = 0;
        int type = 0;
        unsigned char* decoded_bytes = nullptr;
        if (jyppx_ocv_mat_create_empty(decoded_matrix.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_file_node_mat(matrix_node.value, decoded_matrix.get()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_rows(decoded_matrix.get(), &rows) != OPENCV_CSHARP_STATUS_OK || rows != 2 ||
            jyppx_ocv_mat_cols(decoded_matrix.get(), &cols) != OPENCV_CSHARP_STATUS_OK || cols != 2 ||
            jyppx_ocv_mat_type(decoded_matrix.get(), &type) != OPENCV_CSHARP_STATUS_OK || type != 4 ||
            jyppx_ocv_mat_data(decoded_matrix.get(), &decoded_bytes) != OPENCV_CSHARP_STATUS_OK || decoded_bytes == nullptr ||
            std::memcmp(decoded_bytes, matrix_values, sizeof(matrix_values)) != 0)
        {
            return 616;
        }

        NativeFileNodeHandle invalid_child;
        if (jyppx_ocv_core_file_node_at(text_node.value, 0, &invalid_child.value) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT ||
            std::strlen(jyppx_ocv_get_last_error()) == 0)
        {
            return 617;
        }

        NativeFileNodeHandle stale_node;
        if (jyppx_ocv_core_file_storage_get_node(reader.value, reinterpret_cast<const unsigned char*>("score"), 5, &stale_node.value) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_file_storage_release(reader.value) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_file_node_real(stale_node.value, &numeric_value) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT ||
            std::strstr(jyppx_ocv_get_last_error(), "invalidated") == nullptr)
        {
            return 618;
        }

        NativeFileStorageHandle lifetime_reader;
        NativeFileNodeHandle surviving_node;
        if (jyppx_ocv_core_file_storage_create(&lifetime_reader.value) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_file_storage_open(lifetime_reader.value, document_data, static_cast<int>(document_size), storage_read_memory_yaml, nullptr, 0, &opened) != OPENCV_CSHARP_STATUS_OK || opened != 1 ||
            jyppx_ocv_core_file_storage_get_node(lifetime_reader.value, reinterpret_cast<const unsigned char*>("score"), 5, &surviving_node.value) != OPENCV_CSHARP_STATUS_OK)
        {
            return 619;
        }
        jyppx_ocv_core_file_storage_release_handle(lifetime_reader.value);
        lifetime_reader.value = nullptr;
        if (jyppx_ocv_core_file_node_real(surviving_node.value, &numeric_value) != OPENCV_CSHARP_STATUS_OK || numeric_value != 2.5)
        {
            return 620;
        }

        NativeFileNodeHandle empty_node;
        int empty = 0;
        if (jyppx_ocv_core_file_node_create(&empty_node.value) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_file_node_empty(empty_node.value, &empty) != OPENCV_CSHARP_STATUS_OK || empty != 1)
        {
            return 621;
        }

        NativeFileStorageHandle invalid_storage;
        const unsigned char invalid_utf8[] = { 0xc3, 0x28 };
        if (jyppx_ocv_core_file_storage_create(&invalid_storage.value) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_file_storage_open(invalid_storage.value, invalid_utf8, 2, storage_read_memory_yaml, nullptr, 0, &opened) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT ||
            jyppx_ocv_core_file_storage_open(invalid_storage.value, hint, 10, 3, nullptr, 0, &opened) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT ||
            jyppx_ocv_core_file_storage_start_write_struct(invalid_storage.value, reinterpret_cast<const unsigned char*>("bad"), 3, 21, nullptr, 0) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT ||
            std::strlen(jyppx_ocv_get_last_error()) == 0)
        {
            return 622;
        }

        return 0;
    }

    int run_core_numerical_collection_solver_smoke()
    {
        float scalar_value = 0.0F;
        if (jyppx_ocv_core_cube_root(-8.0F, &scalar_value) != OPENCV_CSHARP_STATUS_OK || scalar_value < -2.001F || scalar_value > -1.999F ||
            jyppx_ocv_core_fast_atan2(1.0F, 0.0F, &scalar_value) != OPENCV_CSHARP_STATUS_OK || scalar_value < 89.7F || scalar_value > 90.3F ||
            jyppx_ocv_core_cube_root(1.0F, nullptr) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT)
        {
            return 650;
        }

        NativeMatHandle src1;
        NativeMatHandle src2;
        NativeMatHandle distances;
        NativeMatHandle indices;
        if (jyppx_ocv_mat_create(2, 2, 5, src1.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create(2, 2, 5, src2.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(distances.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(indices.out()) != OPENCV_CSHARP_STATUS_OK)
        {
            return 651;
        }
        unsigned char* bytes = nullptr;
        const float src1_values[] = { 0.0F, 0.0F, 3.0F, 4.0F };
        const float src2_values[] = { 0.0F, 0.0F, 6.0F, 8.0F };
        if (jyppx_ocv_mat_data(src1.get(), &bytes) != OPENCV_CSHARP_STATUS_OK || bytes == nullptr)
        {
            return 652;
        }
        std::memcpy(bytes, src1_values, sizeof(src1_values));
        if (jyppx_ocv_mat_data(src2.get(), &bytes) != OPENCV_CSHARP_STATUS_OK || bytes == nullptr)
        {
            return 653;
        }
        std::memcpy(bytes, src2_values, sizeof(src2_values));
        if (jyppx_ocv_core_batch_distance(src1.get(), src2.get(), distances.get(), 5, indices.get(), 4, 1, nullptr, 0, 0) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_batch_distance(src1.get(), src2.get(), distances.get(), 5, nullptr, 4, 1, nullptr, 0, 0) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT)
        {
            return 654;
        }
        if (jyppx_ocv_mat_data(distances.get(), &bytes) != OPENCV_CSHARP_STATUS_OK || bytes == nullptr ||
            reinterpret_cast<float*>(bytes)[0] != 0.0F || reinterpret_cast<float*>(bytes)[1] < 4.999F || reinterpret_cast<float*>(bytes)[1] > 5.001F)
        {
            return 655;
        }

        NativeMatHandle nan_values;
        if (jyppx_ocv_mat_create(1, 2, 5, nan_values.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_data(nan_values.get(), &bytes) != OPENCV_CSHARP_STATUS_OK || bytes == nullptr)
        {
            return 656;
        }
        reinterpret_cast<float*>(bytes)[0] = std::numeric_limits<float>::quiet_NaN();
        reinterpret_cast<float*>(bytes)[1] = 2.0F;
        if (jyppx_ocv_core_patch_nans(nan_values.get(), -1.0) != OPENCV_CSHARP_STATUS_OK ||
            reinterpret_cast<float*>(bytes)[0] != -1.0F)
        {
            return 657;
        }
        int split_count = 0;
        jyppx_ocv_mat* split_values[1] = {};
        if (jyppx_ocv_core_split_count(nan_values.get(), &split_count) != OPENCV_CSHARP_STATUS_OK || split_count != 1 ||
            jyppx_ocv_core_split_fill(nan_values.get(), split_values, 1, &split_count) != OPENCV_CSHARP_STATUS_OK || split_values[0] == nullptr)
        {
            jyppx_ocv_mat_release(split_values[0]);
            return 658;
        }
        jyppx_ocv_mat_release(split_values[0]);

        NativeMatHandle data;
        NativeMatHandle covar;
        NativeMatHandle mean;
        NativeMatHandle eigenvectors;
        NativeMatHandle eigenvalues;
        NativeMatHandle projected;
        NativeMatHandle reconstructed;
        if (jyppx_ocv_mat_create(3, 2, 6, data.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(covar.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(mean.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(eigenvectors.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(eigenvalues.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(projected.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(reconstructed.out()) != OPENCV_CSHARP_STATUS_OK)
        {
            return 659;
        }
        const double pca_values[] = { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0 };
        if (jyppx_ocv_mat_data(data.get(), &bytes) != OPENCV_CSHARP_STATUS_OK || bytes == nullptr)
        {
            return 660;
        }
        std::memcpy(bytes, pca_values, sizeof(pca_values));
        if (jyppx_ocv_core_calc_covar_matrix(data.get(), covar.get(), mean.get(), 13, 6) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_pca_compute_max_components(data.get(), mean.get(), eigenvectors.get(), eigenvalues.get(), 1) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_pca_project(data.get(), mean.get(), eigenvectors.get(), projected.get()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_pca_back_project(projected.get(), mean.get(), eigenvectors.get(), reconstructed.get()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_pca_compute_retained_variance(data.get(), mean.get(), eigenvectors.get(), nullptr, 0.95) != OPENCV_CSHARP_STATUS_OK)
        {
            return 661;
        }

        NativeMatHandle svd_w;
        NativeMatHandle svd_u;
        NativeMatHandle svd_vt;
        NativeMatHandle rhs;
        NativeMatHandle solution;
        if (jyppx_ocv_mat_create_empty(svd_w.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(svd_u.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(svd_vt.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create(3, 1, 6, rhs.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(solution.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_svd_static_compute(data.get(), svd_w.get(), svd_u.get(), svd_vt.get(), 0) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_svd_static_back_subst(svd_w.get(), svd_u.get(), svd_vt.get(), rhs.get(), solution.get()) != OPENCV_CSHARP_STATUS_OK)
        {
            return 662;
        }

        NativeMatHandle random1;
        NativeMatHandle random2;
        NativeMatHandle low;
        NativeMatHandle high;
        if (jyppx_ocv_mat_create(1, 4, 5, random1.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create(1, 4, 5, random2.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_with_scalar(1, 1, 6, 0.0, 0.0, 0.0, 0.0, low.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_with_scalar(1, 1, 6, 1.0, 1.0, 1.0, 1.0, high.out()) != OPENCV_CSHARP_STATUS_OK)
        {
            return 663;
        }
        if (jyppx_ocv_core_set_rng_seed(123) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_randu_scalar(random1.get(), 0.0, 0.0, 0.0, 0.0, 1.0, 1.0, 1.0, 1.0) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_set_rng_seed(123) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_randu_mat(random2.get(), low.get(), high.get()) != OPENCV_CSHARP_STATUS_OK)
        {
            return 664;
        }
        unsigned char* random1_bytes = nullptr;
        unsigned char* random2_bytes = nullptr;
        if (jyppx_ocv_mat_data(random1.get(), &random1_bytes) != OPENCV_CSHARP_STATUS_OK || random1_bytes == nullptr ||
            jyppx_ocv_mat_data(random2.get(), &random2_bytes) != OPENCV_CSHARP_STATUS_OK || random2_bytes == nullptr ||
            std::memcmp(random1_bytes, random2_bytes, sizeof(float) * 4) != 0 ||
            jyppx_ocv_core_randn_scalar(random1.get(), 0.0, 0.0, 0.0, 0.0, 1.0, 1.0, 1.0, 1.0) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_randn_mat(random2.get(), low.get(), high.get()) != OPENCV_CSHARP_STATUS_OK)
        {
            return 665;
        }
        NativeRngHandle rng;
        if (jyppx_ocv_core_rng_create(123, &rng.value) != OPENCV_CSHARP_STATUS_OK || rng.value == nullptr ||
            jyppx_ocv_core_rand_shuffle(random1.get(), 1.0, rng.value) != OPENCV_CSHARP_STATUS_OK)
        {
            return 666;
        }

        NativeMatHandle objective;
        NativeMatHandle constraints;
        NativeMatHandle lp_solution;
        if (jyppx_ocv_mat_create(1, 2, 6, objective.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create(3, 3, 6, constraints.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(lp_solution.out()) != OPENCV_CSHARP_STATUS_OK)
        {
            return 667;
        }
        const double objective_values[] = { 1.0, 1.0 };
        const double constraint_values[] = { 1.0, 0.0, 2.0, 0.0, 1.0, 3.0, 1.0, 1.0, 4.0 };
        if (jyppx_ocv_mat_data(objective.get(), &bytes) != OPENCV_CSHARP_STATUS_OK || bytes == nullptr)
        {
            return 668;
        }
        std::memcpy(bytes, objective_values, sizeof(objective_values));
        if (jyppx_ocv_mat_data(constraints.get(), &bytes) != OPENCV_CSHARP_STATUS_OK || bytes == nullptr)
        {
            return 669;
        }
        std::memcpy(bytes, constraint_values, sizeof(constraint_values));
        int lp_result = -99;
        if (jyppx_ocv_core_solve_lp(objective.get(), constraints.get(), lp_solution.get(), 1e-12, &lp_result) != OPENCV_CSHARP_STATUS_OK || lp_result != 1 ||
            jyppx_ocv_core_solve_lp(objective.get(), constraints.get(), lp_solution.get(), 1e-12, nullptr) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT)
        {
            return 670;
        }
        return 0;
    }

    int run_core_runtime_diagnostics_timing_smoke()
    {
        NativeRuntimeStateRestore restore;
        if (jyppx_ocv_core_get_num_threads(&restore.thread_count) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_use_optimized(&restore.optimized) != OPENCV_CSHARP_STATUS_OK ||
            (restore.optimized != 0 && restore.optimized != 1))
        {
            return 680;
        }
        restore.initialized = true;

        int thread_number = 0;
        int cpu_count = 0;
        int hint = -1;
        int supported = -1;
        int64_t tick_before = 0;
        int64_t tick_after = 0;
        int64_t cpu_tick = 0;
        double tick_frequency = 0.0;
        if (jyppx_ocv_core_set_num_threads(1) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_get_num_threads(&thread_number) != OPENCV_CSHARP_STATUS_OK || thread_number < 1 ||
            jyppx_ocv_core_get_thread_num(&thread_number) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_get_tick_count(&tick_before) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_get_tick_frequency(&tick_frequency) != OPENCV_CSHARP_STATUS_OK || tick_frequency <= 0.0 ||
            jyppx_ocv_core_get_tick_count(&tick_after) != OPENCV_CSHARP_STATUS_OK || tick_after < tick_before ||
            jyppx_ocv_core_get_cpu_tick_count(&cpu_tick) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_get_number_of_cpus(&cpu_count) != OPENCV_CSHARP_STATUS_OK || cpu_count < 1 ||
            jyppx_ocv_core_get_default_algorithm_hint(&hint) != OPENCV_CSHARP_STATUS_OK || hint < 0 || hint > 2)
        {
            return 681;
        }

        NativeUtf8ResultHandle build_information;
        NativeUtf8ResultHandle feature_name;
        NativeUtf8ResultHandle feature_line;
        if (jyppx_ocv_core_get_build_information(&build_information.value) != OPENCV_CSHARP_STATUS_OK || build_information.value == nullptr ||
            jyppx_ocv_core_get_hardware_feature_name(3, &feature_name.value) != OPENCV_CSHARP_STATUS_OK || feature_name.value == nullptr ||
            jyppx_ocv_core_get_cpu_features_line(&feature_line.value) != OPENCV_CSHARP_STATUS_OK || feature_line.value == nullptr)
        {
            return 682;
        }
        size_t build_information_size = 0;
        size_t feature_name_size = 0;
        size_t feature_line_size = 0;
        if (jyppx_ocv_core_utf8_result_size(build_information.value, &build_information_size) != OPENCV_CSHARP_STATUS_OK || build_information_size == 0 ||
            jyppx_ocv_core_utf8_result_size(feature_name.value, &feature_name_size) != OPENCV_CSHARP_STATUS_OK || feature_name_size == 0 ||
            jyppx_ocv_core_utf8_result_size(feature_line.value, &feature_line_size) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_check_hardware_support(3, &supported) != OPENCV_CSHARP_STATUS_OK || (supported != 0 && supported != 1))
        {
            return 683;
        }

        if (jyppx_ocv_core_set_use_optimized(0) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_use_optimized(&supported) != OPENCV_CSHARP_STATUS_OK || supported != 0 ||
            jyppx_ocv_core_check_hardware_support(3, &supported) != OPENCV_CSHARP_STATUS_OK || supported != 0 ||
            jyppx_ocv_core_set_use_optimized(2) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT ||
            jyppx_ocv_core_check_hardware_support(-1, &supported) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT ||
            jyppx_ocv_core_get_hardware_feature_name(513, &feature_line.value) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT)
        {
            return 684;
        }
        jyppx_ocv_core_utf8_result_release(feature_line.value);
        feature_line.value = nullptr;

        NativeTickMeterHandle meter;
        int64_t time_ticks = 0;
        int64_t last_ticks = 0;
        int64_t counter = 0;
        double time_seconds = 0.0;
        double average_seconds = 0.0;
        double fps = 0.0;
        if (jyppx_ocv_core_tick_meter_create(&meter.value) != OPENCV_CSHARP_STATUS_OK || meter.value == nullptr ||
            jyppx_ocv_core_tick_meter_stop(meter.value) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_tick_meter_get_counter(meter.value, &counter) != OPENCV_CSHARP_STATUS_OK || counter != 0 ||
            jyppx_ocv_core_tick_meter_start(meter.value) != OPENCV_CSHARP_STATUS_OK)
        {
            return 685;
        }
        for (int i = 0; i < 1000; ++i)
        {
            jyppx_ocv_core_get_tick_count(&tick_after);
        }
        if (jyppx_ocv_core_tick_meter_stop(meter.value) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_tick_meter_get_time_ticks(meter.value, &time_ticks) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_tick_meter_get_last_time_ticks(meter.value, &last_ticks) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_tick_meter_get_counter(meter.value, &counter) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_tick_meter_get_time_sec(meter.value, &time_seconds) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_tick_meter_get_avg_time_sec(meter.value, &average_seconds) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_tick_meter_get_fps(meter.value, &fps) != OPENCV_CSHARP_STATUS_OK ||
            time_ticks <= 0 || last_ticks <= 0 || counter != 1 || time_seconds <= 0.0 || average_seconds <= 0.0 || fps <= 0.0 ||
            jyppx_ocv_core_tick_meter_start(nullptr) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT ||
            jyppx_ocv_core_tick_meter_reset(meter.value) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_tick_meter_get_counter(meter.value, &counter) != OPENCV_CSHARP_STATUS_OK || counter != 0)
        {
            return 686;
        }

        return 0;
    }

#if !defined(OPENCV_CSHARP_RUNTIME_PROFILE_MINI)
    int run_objdetect_structured_smoke()
    {
        NativeArucoDictionaryHandle dictionary4;
        NativeArucoDictionaryHandle dictionary5;
        NativeArucoDictionaryHandle extended;
        if (jyppx_ocv_aruco_dictionary_create_predefined(0, &dictionary4.value) != OPENCV_CSHARP_STATUS_OK || dictionary4.value == nullptr ||
            jyppx_ocv_aruco_dictionary_create_predefined(4, &dictionary5.value) != OPENCV_CSHARP_STATUS_OK || dictionary5.value == nullptr ||
            jyppx_ocv_aruco_dictionary_extend(55, 4, dictionary4.value, 12345, &extended.value) != OPENCV_CSHARP_STATUS_OK || extended.value == nullptr)
        {
            return 740;
        }

        const int object_offsets[] = { 0, 4 };
        const jyppx_ocv_point3f object_points[] = { { 0, 0, 0 }, { 1, 0, 0 }, { 1, 1, 0 }, { 0, 1, 0 } };
        const int board_ids[] = { 7 };
        NativeArucoBoardHandle board;
        if (jyppx_ocv_aruco_board_create(object_offsets, 1, object_points, 4, dictionary4.value, board_ids, 1, &board.value) != OPENCV_CSHARP_STATUS_OK || board.value == nullptr)
        {
            return 741;
        }

        int marker_count = 0;
        int point_count = 0;
        if (jyppx_ocv_aruco_board_get_object_points_count(board.value, &marker_count, &point_count) != OPENCV_CSHARP_STATUS_OK || marker_count != 1 || point_count != 4)
        {
            return 742;
        }
        int output_offsets[2] = {};
        jyppx_ocv_point3f output_points[4] = {};
        if (jyppx_ocv_aruco_board_get_object_points_fill(board.value, output_offsets, 2, output_points, 4, &marker_count, &point_count) != OPENCV_CSHARP_STATUS_OK ||
            output_offsets[0] != 0 || output_offsets[1] != 4 || output_points[2].x != 1.0F || output_points[2].y != 1.0F)
        {
            return 743;
        }

        NativeMatHandle board_image;
        if (jyppx_ocv_mat_create_empty(board_image.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_aruco_board_generate_image(board.value, 96, 96, board_image.get(), 4, 1) != OPENCV_CSHARP_STATUS_OK)
        {
            return 744;
        }

        jyppx_ocv_aruco_detector_params detector_params{};
        jyppx_ocv_aruco_refine_params refine_params{};
        if (jyppx_ocv_aruco_detector_default_params(&detector_params) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_aruco_refine_default_params(&refine_params) != OPENCV_CSHARP_STATUS_OK)
        {
            return 745;
        }
        const jyppx_ocv_aruco_dictionary* dictionaries[] = { dictionary4.value, dictionary5.value };
        NativeArucoDetectorHandle detector;
        if (jyppx_ocv_aruco_detector_create_multi_dictionary(dictionaries, 2, &detector_params, &refine_params, &detector.value) != OPENCV_CSHARP_STATUS_OK || detector.value == nullptr)
        {
            return 746;
        }
        int dictionary_count = 0;
        NativeArucoDictionaryHandle cloned_dictionary;
        if (jyppx_ocv_aruco_detector_get_dictionaries_count(detector.value, &dictionary_count) != OPENCV_CSHARP_STATUS_OK || dictionary_count != 2 ||
            jyppx_ocv_aruco_detector_get_dictionary_at(detector.value, 1, &cloned_dictionary.value) != OPENCV_CSHARP_STATUS_OK || cloned_dictionary.value == nullptr ||
            jyppx_ocv_aruco_detector_set_dictionaries(detector.value, dictionaries, 2) != OPENCV_CSHARP_STATUS_OK)
        {
            return 747;
        }

        const int corner_offsets[] = { 0, 4 };
        const jyppx_ocv_point2f corners[] = { { 8, 8 }, { 40, 8 }, { 40, 40 }, { 8, 40 } };
        if (jyppx_ocv_aruco_draw_detected_markers(board_image.get(), corner_offsets, 1, corners, 4, board_ids, 1, 0, 255, 0, 0) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_aruco_draw_detected_corners_charuco(board_image.get(), corners, 1, board_ids, 1, 255, 0, 0, 0) != OPENCV_CSHARP_STATUS_OK)
        {
            return 748;
        }

        const int diamond_ids[] = { 1, 2, 3, 4 };
        if (jyppx_ocv_aruco_draw_detected_diamonds(board_image.get(), corner_offsets, 1, corners, 4, diamond_ids, 4, 0, 0, 255, 0) != OPENCV_CSHARP_STATUS_OK)
        {
            return 749;
        }

        NativeCharucoBoardHandle charuco_board;
        if (jyppx_ocv_aruco_charuco_board_create(5, 7, 80.0F, 40.0F, dictionary4.value, nullptr, 0, &charuco_board.value) != OPENCV_CSHARP_STATUS_OK || charuco_board.value == nullptr)
        {
            return 750;
        }
        jyppx_ocv_aruco_charuco_params charuco_params{};
        if (jyppx_ocv_aruco_charuco_default_params(&charuco_params) != OPENCV_CSHARP_STATUS_OK)
        {
            return 751;
        }
        NativeCharucoDetectorHandle charuco_detector;
        if (jyppx_ocv_aruco_charuco_detector_create(charuco_board.value, &charuco_params, nullptr, nullptr, &detector_params, &refine_params, &charuco_detector.value) != OPENCV_CSHARP_STATUS_OK || charuco_detector.value == nullptr ||
            jyppx_ocv_aruco_charuco_detector_get_detector_parameters(charuco_detector.value, &detector_params) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_aruco_charuco_detector_set_detector_parameters(charuco_detector.value, &detector_params) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_aruco_charuco_detector_get_refine_parameters(charuco_detector.value, &refine_params) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_aruco_charuco_detector_set_refine_parameters(charuco_detector.value, &refine_params) != OPENCV_CSHARP_STATUS_OK)
        {
            return 752;
        }

        NativeQRCodeDetectorArucoHandle qr_detector;
        if (jyppx_ocv_qrcode_detector_aruco_create(&qr_detector.value) != OPENCV_CSHARP_STATUS_OK || qr_detector.value == nullptr ||
            jyppx_ocv_qrcode_detector_aruco_get_aruco_parameters(qr_detector.value, &detector_params) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_qrcode_detector_aruco_set_aruco_parameters(qr_detector.value, &detector_params) != OPENCV_CSHARP_STATUS_OK)
        {
            return 753;
        }

        NativeMccDetectorHandle mcc_detector;
        if (jyppx_ocv_mcc_checker_detector_create(&mcc_detector.value) != OPENCV_CSHARP_STATUS_OK || mcc_detector.value == nullptr ||
            jyppx_ocv_mcc_checker_detector_set_use_dnn_model(mcc_detector.value, 0) != OPENCV_CSHARP_STATUS_OK)
        {
            return 754;
        }
        int use_dnn = 1;
        if (jyppx_ocv_mcc_checker_detector_get_use_dnn_model(mcc_detector.value, &use_dnn) != OPENCV_CSHARP_STATUS_OK || use_dnn != 0 ||
            jyppx_ocv_aruco_board_get_ids_count(nullptr, &marker_count) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT ||
            jyppx_ocv_aruco_dictionary_extend(-1, 4, nullptr, 0, nullptr) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT)
        {
            return 755;
        }

        NativeMatHandle chessboard_image;
        NativeMatHandle chessboard_corners;
        NativeMatHandle chessboard_corners_with_meta;
        NativeMatHandle chessboard_meta;
        NativeMatHandle chessboard_sharpness;
        if (jyppx_ocv_mat_create_empty(chessboard_image.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(chessboard_corners.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(chessboard_corners_with_meta.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(chessboard_meta.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(chessboard_sharpness.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_aruco_charuco_board_generate_image(charuco_board.value, 600, 840, chessboard_image.get(), 40, 1) != OPENCV_CSHARP_STATUS_OK)
        {
            return 756;
        }

        int found_sb = 0;
        int found_sb_with_meta = 0;
        int found_quad = 0;
        double sharpness0 = 0.0;
        double sharpness1 = 0.0;
        double sharpness2 = 0.0;
        double sharpness3 = 0.0;
        if (jyppx_ocv_calib3d_find_chessboard_corners_sb(chessboard_image.get(), 4, 6, chessboard_corners.get(), 0, &found_sb) != OPENCV_CSHARP_STATUS_OK || found_sb != 1 ||
            jyppx_ocv_calib3d_find_chessboard_corners_sb_with_meta(chessboard_image.get(), 4, 6, chessboard_corners_with_meta.get(), 0, chessboard_meta.get(), &found_sb_with_meta) != OPENCV_CSHARP_STATUS_OK || found_sb_with_meta != 1 ||
            jyppx_ocv_calib3d_estimate_chessboard_sharpness(chessboard_image.get(), 4, 6, chessboard_corners.get(), 0.8F, 0, chessboard_sharpness.get(), &sharpness0, &sharpness1, &sharpness2, &sharpness3) != OPENCV_CSHARP_STATUS_OK || sharpness0 < 0.0 ||
            jyppx_ocv_calib3d_find_4_quad_corner_subpix(chessboard_image.get(), chessboard_corners.get(), 5, 5, &found_quad) != OPENCV_CSHARP_STATUS_OK || found_quad != 1)
        {
            return 757;
        }

        int meta_rows = 0;
        int meta_cols = 0;
        int sharpness_empty = 1;
        if (jyppx_ocv_mat_rows(chessboard_meta.get(), &meta_rows) != OPENCV_CSHARP_STATUS_OK || meta_rows != 6 ||
            jyppx_ocv_mat_cols(chessboard_meta.get(), &meta_cols) != OPENCV_CSHARP_STATUS_OK || meta_cols != 4 ||
            jyppx_ocv_mat_empty(chessboard_sharpness.get(), &sharpness_empty) != OPENCV_CSHARP_STATUS_OK || sharpness_empty != 0 ||
            jyppx_ocv_calib3d_find_chessboard_corners_sb(nullptr, 4, 6, chessboard_corners.get(), 0, &found_sb) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT)
        {
            return 758;
        }

        return 0;
    }

    int run_features_ann_index_smoke()
    {
        NativeAnnIndexHandle index;
        if (jyppx_ocv_features2d_ann_index_create(2, 0, &index.value) != OPENCV_CSHARP_STATUS_OK || index.value == nullptr ||
            jyppx_ocv_features2d_ann_index_set_seed(index.value, 1234) != OPENCV_CSHARP_STATUS_OK)
        {
            return 720;
        }

        NativeMatHandle features;
        NativeMatHandle query;
        NativeMatHandle indices;
        NativeMatHandle distances;
        if (jyppx_ocv_mat_create(4, 2, 5, features.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create(2, 2, 5, query.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(indices.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(distances.out()) != OPENCV_CSHARP_STATUS_OK)
        {
            return 721;
        }

        unsigned char* feature_bytes = nullptr;
        unsigned char* query_bytes = nullptr;
        if (jyppx_ocv_mat_data(features.get(), &feature_bytes) != OPENCV_CSHARP_STATUS_OK || feature_bytes == nullptr ||
            jyppx_ocv_mat_data(query.get(), &query_bytes) != OPENCV_CSHARP_STATUS_OK || query_bytes == nullptr)
        {
            return 722;
        }
        const float feature_values[] = { 0.0F, 0.0F, 10.0F, 10.0F, 2.0F, 2.0F, -2.0F, -2.0F };
        const float query_values[] = { 0.1F, 0.1F, 9.5F, 10.5F };
        std::memcpy(feature_bytes, feature_values, sizeof(feature_values));
        std::memcpy(query_bytes, query_values, sizeof(query_values));

        if (jyppx_ocv_features2d_ann_index_add_items(index.value, features.get()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_features2d_ann_index_build(index.value, 2) != OPENCV_CSHARP_STATUS_OK)
        {
            return 723;
        }

        int tree_number = 0;
        int item_number = 0;
        if (jyppx_ocv_features2d_ann_index_get_tree_number(index.value, &tree_number) != OPENCV_CSHARP_STATUS_OK || tree_number != 2 ||
            jyppx_ocv_features2d_ann_index_get_item_number(index.value, &item_number) != OPENCV_CSHARP_STATUS_OK || item_number != 4 ||
            jyppx_ocv_features2d_ann_index_knn_search(index.value, query.get(), indices.get(), distances.get(), 1, -1) != OPENCV_CSHARP_STATUS_OK)
        {
            return 724;
        }

        int rows = 0;
        int cols = 0;
        int type = -1;
        unsigned char* index_bytes = nullptr;
        if (jyppx_ocv_mat_rows(indices.get(), &rows) != OPENCV_CSHARP_STATUS_OK || rows != 2 ||
            jyppx_ocv_mat_cols(indices.get(), &cols) != OPENCV_CSHARP_STATUS_OK || cols != 1 ||
            jyppx_ocv_mat_type(indices.get(), &type) != OPENCV_CSHARP_STATUS_OK || type != 4 ||
            jyppx_ocv_mat_data(indices.get(), &index_bytes) != OPENCV_CSHARP_STATUS_OK || index_bytes == nullptr)
        {
            return 725;
        }
        const int* nearest = reinterpret_cast<const int*>(index_bytes);
        if (nearest[0] != 0 || nearest[1] != 1)
        {
            return 726;
        }

        const unsigned char filename[] = "ann-index-native-smoke.ann";
        std::remove(reinterpret_cast<const char*>(filename));
        if (jyppx_ocv_features2d_ann_index_save(index.value, filename, static_cast<int>(sizeof(filename) - 1), 0) != OPENCV_CSHARP_STATUS_OK)
        {
            return 727;
        }

        NativeAnnIndexHandle loaded;
        if (jyppx_ocv_features2d_ann_index_create(2, 0, &loaded.value) != OPENCV_CSHARP_STATUS_OK || loaded.value == nullptr ||
            jyppx_ocv_features2d_ann_index_load(loaded.value, filename, static_cast<int>(sizeof(filename) - 1), 0) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_features2d_ann_index_get_item_number(loaded.value, &item_number) != OPENCV_CSHARP_STATUS_OK || item_number != 4)
        {
            std::remove(reinterpret_cast<const char*>(filename));
            return 728;
        }
        std::remove(reinterpret_cast<const char*>(filename));

        jyppx_ocv_features2d_ann_index* invalid = nullptr;
        if (jyppx_ocv_features2d_ann_index_create(0, 0, &invalid) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT || invalid != nullptr ||
            jyppx_ocv_features2d_ann_index_create(2, 99, &invalid) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT || invalid != nullptr ||
            jyppx_ocv_features2d_ann_index_build(index.value, 0) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT ||
            jyppx_ocv_features2d_ann_index_knn_search(index.value, query.get(), query.get(), distances.get(), 1, -1) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT ||
            jyppx_ocv_features2d_ann_index_save(index.value, nullptr, 0, 0) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT)
        {
            return 729;
        }

        return 0;
    }

    int run_dnn_structured_smoke()
    {
        jyppx_ocv_dnn_net* net = nullptr;
        if (jyppx_ocv_dnn_net_create_empty(&net) != OPENCV_CSHARP_STATUS_OK || net == nullptr)
        {
            return 700;
        }

        int empty = 0;
        if (jyppx_ocv_dnn_net_empty(net, &empty) != OPENCV_CSHARP_STATUS_OK || empty != 1)
        {
            jyppx_ocv_dnn_net_release_handle(net);
            return 701;
        }
        jyppx_ocv_dnn_net_release_handle(net);
        net = nullptr;

        const unsigned char identity_onnx[] = {
            0x08, 0x08, 0x12, 0x11, 0x4f, 0x70, 0x65, 0x6e, 0x43, 0x56, 0x2d, 0x43, 0x53, 0x68, 0x61, 0x72,
            0x70, 0x2d, 0x41, 0x50, 0x49, 0x3a, 0x78, 0x0a, 0x23, 0x0a, 0x05, 0x69, 0x6e, 0x70, 0x75, 0x74,
            0x12, 0x06, 0x6f, 0x75, 0x74, 0x70, 0x75, 0x74, 0x1a, 0x08, 0x69, 0x64, 0x65, 0x6e, 0x74, 0x69,
            0x74, 0x79, 0x22, 0x08, 0x49, 0x64, 0x65, 0x6e, 0x74, 0x69, 0x74, 0x79, 0x12, 0x0e, 0x69, 0x64,
            0x65, 0x6e, 0x74, 0x69, 0x74, 0x79, 0x5f, 0x67, 0x72, 0x61, 0x70, 0x68, 0x5a, 0x1f, 0x0a, 0x05,
            0x69, 0x6e, 0x70, 0x75, 0x74, 0x12, 0x16, 0x0a, 0x14, 0x08, 0x01, 0x12, 0x10, 0x0a, 0x02, 0x08,
            0x01, 0x0a, 0x02, 0x08, 0x01, 0x0a, 0x02, 0x08, 0x02, 0x0a, 0x02, 0x08, 0x02, 0x62, 0x20, 0x0a,
            0x06, 0x6f, 0x75, 0x74, 0x70, 0x75, 0x74, 0x12, 0x16, 0x0a, 0x14, 0x08, 0x01, 0x12, 0x10, 0x0a,
            0x02, 0x08, 0x01, 0x0a, 0x02, 0x08, 0x01, 0x0a, 0x02, 0x08, 0x02, 0x0a, 0x02, 0x08, 0x02, 0x42,
            0x02, 0x10, 0x0d,
        };
        if (jyppx_ocv_dnn_read_net_from_onnx_buffer(identity_onnx, static_cast<int>(sizeof(identity_onnx)), 3, &net) != OPENCV_CSHARP_STATUS_OK || net == nullptr ||
            jyppx_ocv_dnn_net_set_tracing_mode(net, 1) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_dnn_net_release_handle(net);
            return 709;
        }

        int tracing_mode = 0;
        if (jyppx_ocv_dnn_net_get_tracing_mode(net, &tracing_mode) != OPENCV_CSHARP_STATUS_OK || tracing_mode != 1 ||
            jyppx_ocv_dnn_net_set_profiling_mode(net, 1) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_dnn_net_release_handle(net);
            return 702;
        }

        int profiling_mode = 0;
        int model_format = -1;
        if (jyppx_ocv_dnn_net_get_profiling_mode(net, &profiling_mode) != OPENCV_CSHARP_STATUS_OK || profiling_mode != 1 ||
            jyppx_ocv_dnn_net_get_model_format(net, &model_format) != OPENCV_CSHARP_STATUS_OK || model_format < 0 || model_format > 3 ||
            jyppx_ocv_dnn_net_enable_kv_cache(net) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_dnn_net_reset_kv_cache(net) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_dnn_net_disable_kv_cache(net) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_dnn_net_release_handle(net);
            return 703;
        }

        jyppx_ocv_core_utf8_result* dump = nullptr;
        if (jyppx_ocv_dnn_net_dump(net, &dump) != OPENCV_CSHARP_STATUS_OK || dump == nullptr)
        {
            jyppx_ocv_dnn_net_release_handle(net);
            return 704;
        }

        size_t dump_size = 0;
        const unsigned char* dump_data = nullptr;
        if (jyppx_ocv_core_utf8_result_size(dump, &dump_size) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_utf8_result_data(dump, &dump_data) != OPENCV_CSHARP_STATUS_OK ||
            (dump_size > 0 && dump_data == nullptr))
        {
            jyppx_ocv_core_utf8_result_release(dump);
            jyppx_ocv_dnn_net_release_handle(net);
            return 705;
        }
        jyppx_ocv_core_utf8_result_release(dump);

        int target_count = 0;
        if (jyppx_ocv_dnn_get_available_targets_count(3, &target_count) != OPENCV_CSHARP_STATUS_OK || target_count < 0 ||
            jyppx_ocv_dnn_get_available_targets_count(-1, &target_count) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT)
        {
            jyppx_ocv_dnn_net_release_handle(net);
            return 706;
        }
        if (target_count > 0)
        {
            std::vector<int> targets(static_cast<size_t>(target_count));
            int written = 0;
            if (jyppx_ocv_dnn_get_available_targets_fill(3, targets.data(), target_count, &written) != OPENCV_CSHARP_STATUS_OK || written != target_count)
            {
                jyppx_ocv_dnn_net_release_handle(net);
                return 707;
            }
        }

        const int invalid_offsets[] = { 1, 1 };
        const int values[] = { 1 };
        const int types[] = { 5 };
        int input_shape_count = 0;
        int input_value_count = 0;
        int output_shape_count = 0;
        int output_value_count = 0;
        if (jyppx_ocv_dnn_net_get_layer_shapes_count(
                net, invalid_offsets, 1, values, 1, types, 1, 0,
                &input_shape_count, &input_value_count, &output_shape_count, &output_value_count) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT ||
            jyppx_ocv_dnn_net_set_tracing_mode(net, 99) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT ||
            jyppx_ocv_dnn_net_set_profiling_mode(net, -1) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT)
        {
            jyppx_ocv_dnn_net_release_handle(net);
            return 708;
        }

        jyppx_ocv_dnn_net_release_handle(net);
        return 0;
    }

    int run_photo_hdr_smoke()
    {
        NativeMatHandle image0;
        NativeMatHandle image1;
        NativeMatHandle image2;
        NativeMatHandle aligned0;
        NativeMatHandle aligned1;
        NativeMatHandle aligned2;
        NativeMatHandle times;
        NativeMatHandle response;
        if (jyppx_ocv_mat_create(16, 16, 64, image0.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create(16, 16, 64, image1.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create(16, 16, 64, image2.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(aligned0.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(aligned1.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(aligned2.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create(3, 1, 5, times.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(response.out()) != OPENCV_CSHARP_STATUS_OK)
        {
            return 740;
        }

        unsigned char* bytes = nullptr;
        const unsigned char values[] = { 32, 96, 192 };
        NativeMatHandle* images[] = { &image0, &image1, &image2 };
        for (int i = 0; i < 3; ++i)
        {
            if (jyppx_ocv_mat_data(images[i]->get(), &bytes) != OPENCV_CSHARP_STATUS_OK || bytes == nullptr)
            {
                return 741;
            }
            std::memset(bytes, values[i], 16 * 16 * 3);
        }
        if (jyppx_ocv_mat_data(times.get(), &bytes) != OPENCV_CSHARP_STATUS_OK || bytes == nullptr)
        {
            return 742;
        }
        float* exposure_values = reinterpret_cast<float*>(bytes);
        exposure_values[0] = 0.25F;
        exposure_values[1] = 0.5F;
        exposure_values[2] = 1.0F;

        const jyppx_ocv_mat* src[] = { image0.get(), image1.get(), image2.get() };
        jyppx_ocv_mat* aligned[] = { aligned0.get(), aligned1.get(), aligned2.get() };
        NativeAlignMtbHandle align;
        if (jyppx_ocv_align_mtb_create(6, 4, 0, &align.value) != OPENCV_CSHARP_STATUS_OK || align.value == nullptr ||
            jyppx_ocv_align_mtb_process(align.value, src, aligned, 3, nullptr, nullptr, 0) != OPENCV_CSHARP_STATUS_OK)
        {
            return 743;
        }
        int max_bits = 0;
        int cut = 1;
        int rows = 0;
        int cols = 0;
        int type = 0;
        if (jyppx_ocv_align_mtb_set_max_bits(align.value, 5) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_align_mtb_get_max_bits(align.value, &max_bits) != OPENCV_CSHARP_STATUS_OK || max_bits != 5 ||
            jyppx_ocv_align_mtb_get_cut(align.value, &cut) != OPENCV_CSHARP_STATUS_OK || cut != 0 ||
            jyppx_ocv_mat_rows(aligned0.get(), &rows) != OPENCV_CSHARP_STATUS_OK || rows != 16 ||
            jyppx_ocv_mat_cols(aligned0.get(), &cols) != OPENCV_CSHARP_STATUS_OK || cols != 16 ||
            jyppx_ocv_mat_type(aligned0.get(), &type) != OPENCV_CSHARP_STATUS_OK || type != 64)
        {
            return 744;
        }

        NativeMatHandle gray0;
        NativeMatHandle gray1;
        NativeMatHandle shifted;
        NativeMatHandle threshold_bitmap;
        NativeMatHandle exclude_bitmap;
        if (jyppx_ocv_mat_create(16, 16, 0, gray0.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create(16, 16, 0, gray1.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(shifted.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(threshold_bitmap.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(exclude_bitmap.out()) != OPENCV_CSHARP_STATUS_OK)
        {
            return 745;
        }
        if (jyppx_ocv_mat_data(gray0.get(), &bytes) != OPENCV_CSHARP_STATUS_OK || bytes == nullptr)
        {
            return 746;
        }
        std::memset(bytes, 80, 16 * 16);
        if (jyppx_ocv_mat_data(gray1.get(), &bytes) != OPENCV_CSHARP_STATUS_OK || bytes == nullptr)
        {
            return 747;
        }
        std::memset(bytes, 80, 16 * 16);
        int shift_x = 0;
        int shift_y = 0;
        if (jyppx_ocv_align_mtb_calculate_shift(align.value, gray0.get(), gray1.get(), &shift_x, &shift_y) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_align_mtb_shift_mat(align.value, gray0.get(), shifted.get(), 1, -1) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_align_mtb_compute_bitmaps(align.value, gray0.get(), threshold_bitmap.get(), exclude_bitmap.get()) != OPENCV_CSHARP_STATUS_OK)
        {
            return 748;
        }

        NativeMatHandle debevec_response;
        NativeMatHandle robertson_response;
        NativeMatHandle radiance;
        if (jyppx_ocv_mat_create_empty(debevec_response.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(robertson_response.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(radiance.out()) != OPENCV_CSHARP_STATUS_OK)
        {
            return 749;
        }
        NativeCalibrateCrfHandle calibrate_debevec;
        NativeCalibrateCrfHandle calibrate_robertson;
        if (jyppx_ocv_calibrate_debevec_create(16, 10.0F, 0, &calibrate_debevec.value) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_calibrate_crf_process(calibrate_debevec.value, src, 3, debevec_response.get(), times.get()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_calibrate_robertson_create(1, 0.01F, &calibrate_robertson.value) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_calibrate_crf_process(calibrate_robertson.value, src, 3, robertson_response.get(), times.get()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_calibrate_robertson_get_radiance(calibrate_robertson.value, radiance.get()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_rows(debevec_response.get(), &rows) != OPENCV_CSHARP_STATUS_OK || rows != 256 ||
            jyppx_ocv_mat_type(debevec_response.get(), &type) != OPENCV_CSHARP_STATUS_OK || type != 69)
        {
            return 750;
        }

        NativeMatHandle merged_debevec;
        NativeMatHandle merged_mertens;
        NativeMatHandle merged_robertson;
        if (jyppx_ocv_mat_create_empty(merged_debevec.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(merged_mertens.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(merged_robertson.out()) != OPENCV_CSHARP_STATUS_OK)
        {
            return 751;
        }
        NativeMergeExposuresHandle merge_debevec;
        NativeMergeExposuresHandle merge_mertens;
        NativeMergeExposuresHandle merge_robertson;
        if (jyppx_ocv_merge_debevec_create(&merge_debevec.value) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_merge_mertens_create(1.0F, 1.0F, 0.0F, &merge_mertens.value) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_merge_robertson_create(&merge_robertson.value) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_merge_exposures_process(merge_debevec.value, src, 3, merged_debevec.get(), times.get(), nullptr, 1) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_merge_exposures_process(merge_mertens.value, src, 3, merged_mertens.get(), nullptr, nullptr, 0) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_merge_exposures_process(merge_robertson.value, src, 3, merged_robertson.get(), times.get(), nullptr, 1) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_type(merged_mertens.get(), &type) != OPENCV_CSHARP_STATUS_OK || type != 69)
        {
            return 752;
        }

        float weight = 0.0F;
        if (jyppx_ocv_merge_mertens_set_exposure_weight(merge_mertens.value, 0.2F) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_merge_mertens_get_exposure_weight(merge_mertens.value, &weight) != OPENCV_CSHARP_STATUS_OK ||
            weight < 0.199F || weight > 0.201F ||
            jyppx_ocv_align_mtb_process(align.value, nullptr, aligned, 3, nullptr, nullptr, 0) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT ||
            jyppx_ocv_calibrate_crf_process(calibrate_debevec.value, nullptr, 0, debevec_response.get(), times.get()) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT ||
            jyppx_ocv_merge_exposures_process(merge_mertens.value, src, 3, merged_mertens.get(), nullptr, nullptr, 3) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT)
        {
            return 753;
        }

        return 0;
    }

    int run_photo_ccm_smoke()
    {
        NativeMatHandle gamma_source;
        NativeMatHandle gamma_destination;
        NativeMatHandle samples;
        NativeMatHandle references;
        NativeMatHandle color_correction_matrix;
        NativeMatHandle copied_matrix;
        NativeMatHandle image;
        NativeMatHandle corrected;
        if (jyppx_ocv_mat_create(1, 3, 6, gamma_source.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(gamma_destination.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create(24, 1, 70, samples.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create(24, 1, 70, references.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(color_correction_matrix.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(copied_matrix.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create(2, 2, 64, image.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(corrected.out()) != OPENCV_CSHARP_STATUS_OK)
        {
            return 754;
        }

        unsigned char* bytes = nullptr;
        if (jyppx_ocv_mat_data(gamma_source.get(), &bytes) != OPENCV_CSHARP_STATUS_OK || bytes == nullptr)
        {
            return 755;
        }
        double* gamma_values = reinterpret_cast<double*>(bytes);
        gamma_values[0] = 0.25;
        gamma_values[1] = 0.5;
        gamma_values[2] = 1.0;
        if (jyppx_ocv_photo_ccm_gamma_correction(gamma_source.get(), gamma_destination.get(), 2.0) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_data(gamma_destination.get(), &bytes) != OPENCV_CSHARP_STATUS_OK || bytes == nullptr)
        {
            return 756;
        }
        const double* corrected_gamma = reinterpret_cast<const double*>(bytes);
        if (corrected_gamma[0] < 0.062499 || corrected_gamma[0] > 0.062501 ||
            corrected_gamma[1] < 0.249999 || corrected_gamma[1] > 0.250001 ||
            jyppx_ocv_photo_ccm_gamma_correction(gamma_source.get(), gamma_destination.get(), 0.0) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT)
        {
            return 757;
        }

        static const double sample_values[] =
        {
            0.8, 0.2, 0.1,
            0.1, 0.7, 0.2,
            0.2, 0.1, 0.8,
            0.6, 0.5, 0.2,
            0.3, 0.6, 0.7,
            0.5, 0.3, 0.6,
        };
        if (jyppx_ocv_mat_data(samples.get(), &bytes) != OPENCV_CSHARP_STATUS_OK || bytes == nullptr)
        {
            return 758;
        }
        double* sample_data = reinterpret_cast<double*>(bytes);
        for (int i = 0; i < 4; ++i)
        {
            std::memcpy(sample_data + i * 18, sample_values, sizeof(sample_values));
        }
        if (jyppx_ocv_mat_data(references.get(), &bytes) != OPENCV_CSHARP_STATUS_OK || bytes == nullptr)
        {
            return 759;
        }
        double* reference_data = reinterpret_cast<double*>(bytes);
        for (int i = 0; i < 4; ++i)
        {
            std::memcpy(reference_data + i * 18, sample_values, sizeof(sample_values));
        }
        if (jyppx_ocv_mat_data(image.get(), &bytes) != OPENCV_CSHARP_STATUS_OK || bytes == nullptr)
        {
            return 760;
        }
        std::memset(bytes, 96, 2 * 2 * 3);

        NativeColorCorrectionModelHandle unloaded;
        if (jyppx_ocv_photo_ccm_create(&unloaded.value) != OPENCV_CSHARP_STATUS_OK || unloaded.value == nullptr ||
            jyppx_ocv_photo_ccm_compute(unloaded.value, color_correction_matrix.get()) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT)
        {
            return 761;
        }

        NativeColorCorrectionModelHandle model;
        if (jyppx_ocv_photo_ccm_create_reference_colors(
                samples.get(), references.get(), 0, &model.value) != OPENCV_CSHARP_STATUS_OK || model.value == nullptr)
        {
            return 762;
        }
        double loss = 0.0;
        if (jyppx_ocv_photo_ccm_get_loss(model.value, &loss) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT ||
            jyppx_ocv_photo_ccm_set_color_space(model.value, 1) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT ||
            jyppx_ocv_photo_ccm_set_distance(model.value, 7) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_photo_ccm_set_max_count(model.value, 20) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_photo_ccm_set_epsilon(model.value, 0.01) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_photo_ccm_compute(model.value, color_correction_matrix.get()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_photo_ccm_get_color_correction_matrix(model.value, copied_matrix.get()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_photo_ccm_get_loss(model.value, &loss) != OPENCV_CSHARP_STATUS_OK ||
            !std::isfinite(loss) ||
            jyppx_ocv_photo_ccm_correct_image(model.value, image.get(), corrected.get(), 0) != OPENCV_CSHARP_STATUS_OK)
        {
            return 763;
        }

        int rows = 0;
        int cols = 0;
        int type = 0;
        if (jyppx_ocv_mat_rows(color_correction_matrix.get(), &rows) != OPENCV_CSHARP_STATUS_OK || rows != 3 ||
            jyppx_ocv_mat_cols(color_correction_matrix.get(), &cols) != OPENCV_CSHARP_STATUS_OK || cols != 3 ||
            jyppx_ocv_mat_type(color_correction_matrix.get(), &type) != OPENCV_CSHARP_STATUS_OK || type != 6 ||
            jyppx_ocv_mat_rows(corrected.get(), &rows) != OPENCV_CSHARP_STATUS_OK || rows != 2 ||
            jyppx_ocv_mat_type(corrected.get(), &type) != OPENCV_CSHARP_STATUS_OK || type != 64)
        {
            return 764;
        }

        return 0;
    }

    int run_photo_intelligent_scissors_smoke()
    {
        NativeIntelligentScissorsHandle scissors;
        NativeMatHandle image;
        NativeMatHandle contour;
        NativeMatHandle reverse_contour;
        NativeMatHandle non_edge;
        NativeMatHandle gradient_direction;
        NativeMatHandle gradient_magnitude;
        if (jyppx_ocv_photo_intelligent_scissors_create(&scissors.value) != OPENCV_CSHARP_STATUS_OK ||
            scissors.value == nullptr ||
            jyppx_ocv_mat_create(16, 16, 0, image.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(contour.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(reverse_contour.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create(16, 16, 0, non_edge.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create(16, 16, 37, gradient_direction.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create(16, 16, 5, gradient_magnitude.out()) != OPENCV_CSHARP_STATUS_OK)
        {
            return 765;
        }

        unsigned char* bytes = nullptr;
        if (jyppx_ocv_mat_data(image.get(), &bytes) != OPENCV_CSHARP_STATUS_OK || bytes == nullptr)
        {
            return 766;
        }
        std::memset(bytes, 0, 16 * 16);
        for (int x = 2; x <= 13; ++x)
        {
            bytes[2 * 16 + x] = 255;
            bytes[13 * 16 + x] = 255;
        }
        for (int y = 2; y <= 13; ++y)
        {
            bytes[y * 16 + 2] = 255;
            bytes[y * 16 + 13] = 255;
        }

        if (jyppx_ocv_photo_intelligent_scissors_build_map(scissors.value, 2, 2) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT ||
            jyppx_ocv_photo_intelligent_scissors_set_weights(scissors.value, 0.0F, 0.0F, 0.0F) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT ||
            jyppx_ocv_photo_intelligent_scissors_set_edge_feature_canny_parameters(scissors.value, 10.0, 30.0, 4, 0) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT ||
            jyppx_ocv_photo_intelligent_scissors_set_gradient_magnitude_max_limit(scissors.value, 10.0F) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_photo_intelligent_scissors_set_edge_feature_zero_crossing_parameters(scissors.value, 1.0F) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_photo_intelligent_scissors_apply_image(scissors.value, image.get()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_photo_intelligent_scissors_build_map(scissors.value, -1, 2) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT ||
            jyppx_ocv_photo_intelligent_scissors_build_map(scissors.value, 2, 2) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_photo_intelligent_scissors_get_contour(scissors.value, 13, 2, contour.get(), 0) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_photo_intelligent_scissors_get_contour(scissors.value, 13, 2, reverse_contour.get(), 1) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_photo_intelligent_scissors_get_contour(scissors.value, -1, 2, contour.get(), 0) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT)
        {
            return 767;
        }

        int rows = 0;
        int type = 0;
        if (jyppx_ocv_mat_rows(contour.get(), &rows) != OPENCV_CSHARP_STATUS_OK || rows < 2 ||
            jyppx_ocv_mat_type(contour.get(), &type) != OPENCV_CSHARP_STATUS_OK || type != 36 ||
            jyppx_ocv_mat_data(contour.get(), &bytes) != OPENCV_CSHARP_STATUS_OK || bytes == nullptr)
        {
            return 768;
        }
        const int* forward_points = reinterpret_cast<const int*>(bytes);
        if (forward_points[0] != 2 || forward_points[1] != 2 ||
            forward_points[(rows - 1) * 2] != 13 || forward_points[(rows - 1) * 2 + 1] != 2 ||
            jyppx_ocv_mat_data(reverse_contour.get(), &bytes) != OPENCV_CSHARP_STATUS_OK || bytes == nullptr)
        {
            return 769;
        }
        const int* reverse_points = reinterpret_cast<const int*>(bytes);
        if (reverse_points[0] != 13 || reverse_points[1] != 2 ||
            reverse_points[(rows - 1) * 2] != 2 || reverse_points[(rows - 1) * 2 + 1] != 2)
        {
            return 770;
        }

        if (jyppx_ocv_photo_intelligent_scissors_set_weights(scissors.value, 1.0F, 0.0F, 0.0F) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_photo_intelligent_scissors_get_contour(scissors.value, 13, 2, contour.get(), 0) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT ||
            jyppx_ocv_photo_intelligent_scissors_apply_image_features(
                scissors.value,
                non_edge.get(),
                gradient_direction.get(),
                gradient_magnitude.get(),
                nullptr) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_photo_intelligent_scissors_build_map(scissors.value, 2, 2) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_photo_intelligent_scissors_get_contour(scissors.value, 13, 2, contour.get(), 0) != OPENCV_CSHARP_STATUS_OK)
        {
            return 771;
        }

        return 0;
    }

    int run_photo_final_callables_smoke()
    {
        NativeMatHandle observation0;
        NativeMatHandle observation1;
        NativeMatHandle denoised;
        NativeMatHandle image;
        NativeMatHandle coefficients;
        NativeMatHandle corrected;
        NativeMatHandle loaded_coefficients;
        if (jyppx_ocv_mat_create(4, 4, 0, observation0.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create(4, 4, 0, observation1.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(denoised.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create(4, 4, 64, image.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create(4, 1, 5, coefficients.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(corrected.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(loaded_coefficients.out()) != OPENCV_CSHARP_STATUS_OK)
        {
            return 772;
        }

        if (jyppx_ocv_mat_set_to(observation0.get(), 80.0, 0.0, 0.0, 0.0) != OPENCV_CSHARP_STATUS_OK) { return 773; }
        if (jyppx_ocv_mat_set_to(observation1.get(), 96.0, 0.0, 0.0, 0.0) != OPENCV_CSHARP_STATUS_OK) { return 774; }
        if (jyppx_ocv_mat_set_to(image.get(), 20.0, 40.0, 80.0, 0.0) != OPENCV_CSHARP_STATUS_OK) { return 775; }
        if (jyppx_ocv_mat_set_to(coefficients.get(), 0.0, 0.0, 0.0, 0.0) != OPENCV_CSHARP_STATUS_OK) { return 776; }

        const jyppx_ocv_mat* observations[] = { observation0.get(), observation1.get() };
        int rows = 0;
        int cols = 0;
        int type = 0;
        if (jyppx_ocv_photo_denoise_tvl1(observations, 2, denoised.get(), 1.0, 2) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_rows(denoised.get(), &rows) != OPENCV_CSHARP_STATUS_OK || rows != 4 ||
            jyppx_ocv_mat_cols(denoised.get(), &cols) != OPENCV_CSHARP_STATUS_OK || cols != 4 ||
            jyppx_ocv_mat_type(denoised.get(), &type) != OPENCV_CSHARP_STATUS_OK || type != 0 ||
            jyppx_ocv_photo_denoise_tvl1(nullptr, 0, denoised.get(), 1.0, 2) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT ||
            jyppx_ocv_photo_denoise_tvl1(observations, 2, denoised.get(), 0.0, 2) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT)
        {
            return 777;
        }

        if (jyppx_ocv_photo_correct_chromatic_aberration(
                image.get(), coefficients.get(), corrected.get(), 4, 4, 0, -1) != OPENCV_CSHARP_STATUS_OK) { return 778; }
        if (jyppx_ocv_mat_rows(corrected.get(), &rows) != OPENCV_CSHARP_STATUS_OK || rows != 4) { return 779; }
        if (jyppx_ocv_mat_cols(corrected.get(), &cols) != OPENCV_CSHARP_STATUS_OK || cols != 4) { return 780; }
        if (jyppx_ocv_mat_type(corrected.get(), &type) != OPENCV_CSHARP_STATUS_OK || type != 64) { return 781; }
        if (jyppx_ocv_photo_correct_chromatic_aberration(
                image.get(), coefficients.get(), corrected.get(), 3, 4, 0, -1) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT) { return 782; }

        const char calibration[] =
            "%YAML:1.0\n"
            "image_width: 4\n"
            "image_height: 4\n"
            "red_channel:\n"
            "  coeffs_x: [0.]\n"
            "  coeffs_y: [0.]\n"
            "blue_channel:\n"
            "  coeffs_x: [0.]\n"
            "  coeffs_y: [0.]\n";
        NativeFileStorageHandle storage;
        NativeFileNodeHandle root;
        int opened = 0;
        int width = 0;
        int height = 0;
        int degree = -1;
        if (jyppx_ocv_core_file_storage_create(&storage.value) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_core_file_storage_open(
                storage.value,
                reinterpret_cast<const unsigned char*>(calibration),
                static_cast<int>(sizeof(calibration) - 1),
                4 | 16,
                nullptr,
                0,
                &opened) != OPENCV_CSHARP_STATUS_OK || opened != 1 ||
            jyppx_ocv_core_file_storage_root(storage.value, 0, &root.value) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_photo_load_chromatic_aberration_params(
                root.value, loaded_coefficients.get(), &width, &height, &degree) != OPENCV_CSHARP_STATUS_OK ||
            width != 4 || height != 4 || degree != 0 ||
            jyppx_ocv_mat_rows(loaded_coefficients.get(), &rows) != OPENCV_CSHARP_STATUS_OK || rows != 4 ||
            jyppx_ocv_mat_cols(loaded_coefficients.get(), &cols) != OPENCV_CSHARP_STATUS_OK || cols != 1 ||
            jyppx_ocv_mat_type(loaded_coefficients.get(), &type) != OPENCV_CSHARP_STATUS_OK || type != 5)
        {
            return 783;
        }

        if (jyppx_ocv_core_file_storage_release(storage.value) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_type(loaded_coefficients.get(), &type) != OPENCV_CSHARP_STATUS_OK || type != 5)
        {
            return 784;
        }

        return 0;
    }

    int run_ml_ann_mlp_smoke()
    {
        NativeMlModelHandle model;
        if (jyppx_ocv_ml_ann_mlp_create(&model.value) != OPENCV_CSHARP_STATUS_OK || model.value == nullptr)
        {
            return 800;
        }

        int train_method = -1;
        double value = 0.0;
        if (jyppx_ocv_ml_ann_mlp_get_int(model.value, 0, &train_method) != OPENCV_CSHARP_STATUS_OK || train_method != 1 ||
            jyppx_ocv_ml_ann_mlp_get_double(model.value, 2, &value) != OPENCV_CSHARP_STATUS_OK || std::fabs(value - 0.1) > 1e-12 ||
            jyppx_ocv_ml_ann_mlp_set_train_method(model.value, 1, 0.1, 1e-6) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_ml_ann_mlp_set_activation_function(model.value, 0, 0.0, 0.0) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_ml_ann_mlp_set_double(model.value, 7, 12.0) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_ml_ann_mlp_set_double(model.value, 8, 0.2) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_ml_ann_mlp_set_double(model.value, 9, 0.9) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_ml_ann_mlp_set_int(model.value, 1, 12) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_ml_ann_mlp_set_term_criteria(model.value, 3, 200, 1e-6) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_ml_ann_mlp_set_anneal_energy_seed(model.value, 0xffffffffULL) != OPENCV_CSHARP_STATUS_OK)
        {
            return 801;
        }

        NativeMatHandle layers;
        NativeMatHandle returned_layers;
        if (jyppx_ocv_mat_create(1, 3, 4, layers.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(returned_layers.out()) != OPENCV_CSHARP_STATUS_OK)
        {
            return 802;
        }
        unsigned char* bytes = nullptr;
        const int layer_values[] = { 2, 4, 1 };
        if (jyppx_ocv_mat_data(layers.get(), &bytes) != OPENCV_CSHARP_STATUS_OK || bytes == nullptr)
        {
            return 803;
        }
        std::memcpy(bytes, layer_values, sizeof(layer_values));
        if (jyppx_ocv_ml_ann_mlp_set_layer_sizes(model.value, layers.get()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_ml_ann_mlp_get_layer_sizes(model.value, returned_layers.get()) != OPENCV_CSHARP_STATUS_OK)
        {
            return 804;
        }

        int total_layers = 0;
        size_t returned_total = 0;
        if (jyppx_ocv_mat_total(returned_layers.get(), &returned_total) != OPENCV_CSHARP_STATUS_OK || returned_total != 3 ||
            jyppx_ocv_ml_ann_mlp_get_int(model.value, 1, &total_layers) != OPENCV_CSHARP_STATUS_OK || total_layers != 12)
        {
            return 805;
        }

        NativeMatHandle samples;
        NativeMatHandle responses;
        NativeMatHandle query;
        NativeMatHandle results;
        if (jyppx_ocv_mat_create(4, 2, 5, samples.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create(4, 1, 5, responses.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create(1, 2, 5, query.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(results.out()) != OPENCV_CSHARP_STATUS_OK)
        {
            return 806;
        }
        const float sample_values[] = { -1.0F, -1.0F, -1.0F, 1.0F, 1.0F, -1.0F, 1.0F, 1.0F };
        const float response_values[] = { -2.0F, 0.0F, 0.0F, 2.0F };
        const float query_values[] = { 0.25F, -0.5F };
        if (jyppx_ocv_mat_data(samples.get(), &bytes) != OPENCV_CSHARP_STATUS_OK || bytes == nullptr)
        {
            return 807;
        }
        std::memcpy(bytes, sample_values, sizeof(sample_values));
        if (jyppx_ocv_mat_data(responses.get(), &bytes) != OPENCV_CSHARP_STATUS_OK || bytes == nullptr)
        {
            return 808;
        }
        std::memcpy(bytes, response_values, sizeof(response_values));
        if (jyppx_ocv_mat_data(query.get(), &bytes) != OPENCV_CSHARP_STATUS_OK || bytes == nullptr)
        {
            return 809;
        }
        std::memcpy(bytes, query_values, sizeof(query_values));

        int trained = 0;
        float prediction = 0.0F;
        NativeMatHandle weights;
        if (jyppx_ocv_mat_create_empty(weights.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_ml_stat_model_train_samples(model.value, samples.get(), 0, responses.get(), &trained) != OPENCV_CSHARP_STATUS_OK || trained != 1 ||
            jyppx_ocv_ml_stat_model_predict(model.value, query.get(), results.get(), 0, &prediction) != OPENCV_CSHARP_STATUS_OK || !std::isfinite(prediction) ||
            jyppx_ocv_ml_ann_mlp_get_weights(model.value, 1, weights.get()) != OPENCV_CSHARP_STATUS_OK)
        {
            return 810;
        }
        size_t weight_count = 0;
        if (jyppx_ocv_mat_total(weights.get(), &weight_count) != OPENCV_CSHARP_STATUS_OK || weight_count == 0)
        {
            return 811;
        }

        return 0;
    }

    int run_ml_tree_models_smoke()
    {
        NativeMlModelHandle dtrees;
        NativeMlModelHandle rtrees;
        NativeMlModelHandle boost;
        if (jyppx_ocv_ml_dtrees_create(&dtrees.value) != OPENCV_CSHARP_STATUS_OK || dtrees.value == nullptr ||
            jyppx_ocv_ml_rtrees_create(&rtrees.value) != OPENCV_CSHARP_STATUS_OK || rtrees.value == nullptr ||
            jyppx_ocv_ml_boost_create(&boost.value) != OPENCV_CSHARP_STATUS_OK || boost.value == nullptr)
        {
            return 820;
        }

        int int_value = -1;
        double double_value = 0.0;
        if (jyppx_ocv_ml_dtrees_get_int(dtrees.value, 0, &int_value) != OPENCV_CSHARP_STATUS_OK || int_value != 10 ||
            jyppx_ocv_ml_dtrees_set_int(dtrees.value, 1, 4) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_ml_dtrees_set_int(dtrees.value, 2, 1) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_ml_dtrees_set_int(dtrees.value, 3, 0) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_ml_dtrees_set_regression_accuracy(dtrees.value, 0.02F) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_ml_rtrees_set_int(rtrees.value, 0, 1) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_ml_rtrees_set_int(rtrees.value, 1, 1) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_ml_rtrees_set_term_criteria(rtrees.value, 1, 8, 0.0) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_ml_boost_get_int(boost.value, 0, &int_value) != OPENCV_CSHARP_STATUS_OK || int_value != 1 ||
            jyppx_ocv_ml_boost_get_weight_trim_rate(boost.value, &double_value) != OPENCV_CSHARP_STATUS_OK || std::fabs(double_value - 0.95) > 1e-12 ||
            jyppx_ocv_ml_boost_set_int(boost.value, 0, 0) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_ml_boost_set_int(boost.value, 1, 8) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_ml_boost_set_weight_trim_rate(boost.value, 0.9) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_ml_dtrees_set_int(boost.value, 2, 1) != OPENCV_CSHARP_STATUS_OK)
        {
            return 821;
        }

        NativeMatHandle priors;
        NativeMatHandle copied_priors;
        NativeMatHandle samples;
        NativeMatHandle responses;
        NativeMatHandle query;
        NativeMatHandle results;
        NativeMatHandle votes;
        NativeMatHandle importance;
        if (jyppx_ocv_mat_create(1, 2, 5, priors.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(copied_priors.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create(6, 2, 5, samples.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create(6, 1, 4, responses.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create(1, 2, 5, query.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(results.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(votes.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(importance.out()) != OPENCV_CSHARP_STATUS_OK)
        {
            return 822;
        }

        unsigned char* bytes = nullptr;
        const float prior_values[] = { 1.0F, 1.0F };
        const float sample_values[] = {
            0.0F, 0.0F,
            0.0F, 1.0F,
            1.0F, 0.0F,
            5.0F, 5.0F,
            5.0F, 6.0F,
            6.0F, 5.0F
        };
        const int response_values[] = { 0, 0, 0, 1, 1, 1 };
        const float query_values[] = { 0.1F, 0.2F };
        if (jyppx_ocv_mat_data(priors.get(), &bytes) != OPENCV_CSHARP_STATUS_OK || bytes == nullptr)
        {
            return 823;
        }
        std::memcpy(bytes, prior_values, sizeof(prior_values));
        if (jyppx_ocv_mat_data(samples.get(), &bytes) != OPENCV_CSHARP_STATUS_OK || bytes == nullptr)
        {
            return 824;
        }
        std::memcpy(bytes, sample_values, sizeof(sample_values));
        if (jyppx_ocv_mat_data(responses.get(), &bytes) != OPENCV_CSHARP_STATUS_OK || bytes == nullptr)
        {
            return 825;
        }
        std::memcpy(bytes, response_values, sizeof(response_values));
        if (jyppx_ocv_mat_data(query.get(), &bytes) != OPENCV_CSHARP_STATUS_OK || bytes == nullptr)
        {
            return 826;
        }
        std::memcpy(bytes, query_values, sizeof(query_values));

        if (jyppx_ocv_ml_dtrees_set_priors(dtrees.value, priors.get()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_ml_dtrees_get_priors(dtrees.value, copied_priors.get()) != OPENCV_CSHARP_STATUS_OK)
        {
            return 827;
        }
        size_t total = 0;
        if (jyppx_ocv_mat_total(copied_priors.get(), &total) != OPENCV_CSHARP_STATUS_OK || total != 2)
        {
            return 828;
        }

        int trained = 0;
        float prediction = 0.0F;
        if (jyppx_ocv_ml_stat_model_train_samples(dtrees.value, samples.get(), 0, responses.get(), &trained) != OPENCV_CSHARP_STATUS_OK || trained != 1 ||
            jyppx_ocv_core_set_rng_seed(12345) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_ml_stat_model_train_samples(rtrees.value, samples.get(), 0, responses.get(), &trained) != OPENCV_CSHARP_STATUS_OK || trained != 1 ||
            jyppx_ocv_ml_stat_model_train_samples(boost.value, samples.get(), 0, responses.get(), &trained) != OPENCV_CSHARP_STATUS_OK || trained != 1 ||
            jyppx_ocv_ml_stat_model_predict(dtrees.value, query.get(), results.get(), 0, &prediction) != OPENCV_CSHARP_STATUS_OK || !std::isfinite(prediction) ||
            jyppx_ocv_ml_stat_model_predict(rtrees.value, query.get(), results.get(), 0, &prediction) != OPENCV_CSHARP_STATUS_OK || !std::isfinite(prediction) ||
            jyppx_ocv_ml_stat_model_predict(boost.value, query.get(), results.get(), 0, &prediction) != OPENCV_CSHARP_STATUS_OK || !std::isfinite(prediction))
        {
            return 829;
        }

        if (jyppx_ocv_ml_rtrees_get_votes(rtrees.value, query.get(), votes.get(), 512) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_ml_rtrees_get_var_importance(rtrees.value, importance.get()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_ml_rtrees_get_oob_error(rtrees.value, &double_value) != OPENCV_CSHARP_STATUS_OK || !std::isfinite(double_value))
        {
            return 830;
        }

        int rows = 0;
        int cols = 0;
        if (jyppx_ocv_mat_rows(votes.get(), &rows) != OPENCV_CSHARP_STATUS_OK || rows != 2 ||
            jyppx_ocv_mat_cols(votes.get(), &cols) != OPENCV_CSHARP_STATUS_OK || cols != 2 ||
            jyppx_ocv_mat_total(importance.get(), &total) != OPENCV_CSHARP_STATUS_OK || total != 2)
        {
            return 831;
        }

        return 0;
    }

    int run_ml_em_smoke()
    {
        NativeMlModelHandle model;
        NativeMlModelHandle e_model;
        NativeMlModelHandle m_model;
        if (jyppx_ocv_ml_em_create(&model.value) != OPENCV_CSHARP_STATUS_OK || model.value == nullptr ||
            jyppx_ocv_ml_em_create(&e_model.value) != OPENCV_CSHARP_STATUS_OK || e_model.value == nullptr ||
            jyppx_ocv_ml_em_create(&m_model.value) != OPENCV_CSHARP_STATUS_OK || m_model.value == nullptr)
        {
            return 833;
        }

        int int_value = -1;
        int criteria_type = 0;
        int criteria_count = 0;
        double criteria_epsilon = 0.0;
        if (jyppx_ocv_ml_em_get_int(model.value, 0, &int_value) != OPENCV_CSHARP_STATUS_OK || int_value != 5 ||
            jyppx_ocv_ml_em_get_int(model.value, 1, &int_value) != OPENCV_CSHARP_STATUS_OK || int_value != 1 ||
            jyppx_ocv_ml_em_get_term_criteria(model.value, &criteria_type, &criteria_count, &criteria_epsilon) != OPENCV_CSHARP_STATUS_OK ||
            criteria_type != 3 || criteria_count != 100 || std::fabs(criteria_epsilon - 1e-6) > 1e-15 ||
            jyppx_ocv_ml_em_set_int(model.value, 0, 2) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_ml_em_set_int(model.value, 1, 2) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_ml_em_set_term_criteria(model.value, 3, 100, 1e-8) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_ml_em_set_int(e_model.value, 0, 2) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_ml_em_set_int(e_model.value, 1, 2) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_ml_em_set_int(m_model.value, 0, 2) != OPENCV_CSHARP_STATUS_OK)
        {
            return 834;
        }

        NativeMatHandle samples;
        NativeMatHandle query;
        NativeMatHandle log_likelihoods;
        NativeMatHandle labels;
        NativeMatHandle probabilities;
        NativeMatHandle predict_probabilities;
        NativeMatHandle batch_probabilities;
        NativeMatHandle weights;
        NativeMatHandle means;
        NativeMatHandle covariance0;
        NativeMatHandle covariance1;
        NativeMatHandle initial_means;
        NativeMatHandle initial_weights;
        NativeMatHandle initial_probabilities;
        if (jyppx_ocv_mat_create(8, 2, 6, samples.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create(1, 2, 6, query.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(log_likelihoods.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(labels.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(probabilities.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(predict_probabilities.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(batch_probabilities.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(weights.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(means.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create(2, 2, 6, covariance0.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create(2, 2, 6, covariance1.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create(2, 2, 6, initial_means.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create(1, 2, 6, initial_weights.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create(8, 2, 6, initial_probabilities.out()) != OPENCV_CSHARP_STATUS_OK)
        {
            return 835;
        }

        const double sample_values[] = {
            -0.2, 0.1,
            0.1, -0.2,
            0.3, 0.2,
            -0.1, -0.3,
            4.8, 5.1,
            5.2, 4.9,
            5.3, 5.2,
            4.7, 4.8
        };
        const double query_values[] = { 0.0, 0.0 };
        const double mean_values[] = { 0.0, 0.0, 5.0, 5.0 };
        const double weight_values[] = { 0.5, 0.5 };
        const double covariance_values[] = { 1.0, 0.0, 0.0, 1.0 };
        const double initial_probability_values[] = {
            1.0, 0.0, 1.0, 0.0, 1.0, 0.0, 1.0, 0.0,
            0.0, 1.0, 0.0, 1.0, 0.0, 1.0, 0.0, 1.0
        };
        unsigned char* bytes = nullptr;
#define COPY_EM_VALUES(HANDLE, VALUES, CODE) \
        if (jyppx_ocv_mat_data((HANDLE).get(), &bytes) != OPENCV_CSHARP_STATUS_OK || bytes == nullptr) { return CODE; } \
        std::memcpy(bytes, VALUES, sizeof(VALUES))
        COPY_EM_VALUES(samples, sample_values, 836);
        COPY_EM_VALUES(query, query_values, 837);
        COPY_EM_VALUES(initial_means, mean_values, 838);
        COPY_EM_VALUES(initial_weights, weight_values, 839);
        COPY_EM_VALUES(covariance0, covariance_values, 840);
        COPY_EM_VALUES(covariance1, covariance_values, 841);
        COPY_EM_VALUES(initial_probabilities, initial_probability_values, 842);
#undef COPY_EM_VALUES

        int trained = 0;
        if (jyppx_ocv_core_set_rng_seed(20260731) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_ml_em_train_em(model.value, samples.get(), log_likelihoods.get(), labels.get(), probabilities.get(), &trained) != OPENCV_CSHARP_STATUS_OK || trained != 1)
        {
            return 843;
        }

        double log_likelihood = 0.0;
        int label = -1;
        float batch_prediction = -1.0F;
        if (jyppx_ocv_ml_em_predict2(model.value, query.get(), predict_probabilities.get(), &log_likelihood, &label) != OPENCV_CSHARP_STATUS_OK ||
            !std::isfinite(log_likelihood) || label < 0 || label > 1 ||
            jyppx_ocv_ml_stat_model_predict(model.value, query.get(), batch_probabilities.get(), 0, &batch_prediction) != OPENCV_CSHARP_STATUS_OK ||
            (batch_prediction != 0.0F && batch_prediction != 1.0F) ||
            jyppx_ocv_ml_em_get_weights(model.value, weights.get()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_ml_em_get_means(model.value, means.get()) != OPENCV_CSHARP_STATUS_OK)
        {
            return 844;
        }

        int covariance_count = 0;
        jyppx_ocv_mat* covariance_outputs[] = { covariance0.get(), covariance1.get() };
        if (jyppx_ocv_ml_em_get_covariances_count(model.value, &covariance_count) != OPENCV_CSHARP_STATUS_OK || covariance_count != 2 ||
            jyppx_ocv_ml_em_get_covariances_fill(model.value, covariance_outputs, 2, &covariance_count) != OPENCV_CSHARP_STATUS_OK || covariance_count != 2)
        {
            return 845;
        }

        int rows = 0;
        int cols = 0;
        if (jyppx_ocv_mat_rows(probabilities.get(), &rows) != OPENCV_CSHARP_STATUS_OK || rows != 8 ||
            jyppx_ocv_mat_cols(probabilities.get(), &cols) != OPENCV_CSHARP_STATUS_OK || cols != 2 ||
            jyppx_ocv_mat_rows(means.get(), &rows) != OPENCV_CSHARP_STATUS_OK || rows != 2 ||
            jyppx_ocv_mat_cols(means.get(), &cols) != OPENCV_CSHARP_STATUS_OK || cols != 2)
        {
            return 846;
        }

        const jyppx_ocv_mat* initial_covariances[] = { covariance0.get(), covariance1.get() };
        if (jyppx_ocv_ml_em_train_e(
                e_model.value,
                samples.get(),
                initial_means.get(),
                initial_covariances,
                2,
                initial_weights.get(),
                nullptr,
                nullptr,
                probabilities.get(),
                &trained) != OPENCV_CSHARP_STATUS_OK || trained != 1 ||
            jyppx_ocv_ml_em_train_m(
                m_model.value,
                samples.get(),
                initial_probabilities.get(),
                nullptr,
                nullptr,
                probabilities.get(),
                &trained) != OPENCV_CSHARP_STATUS_OK || trained != 1)
        {
            return 847;
        }

        return 0;
    }

    int run_ml_remaining_callables_smoke()
    {
        NativeMatHandle samples;
        NativeMatHandle logistic_responses;
        NativeMatHandle svmsgd_responses;
        NativeMatHandle negative_query;
        NativeMatHandle positive_query;
        NativeMatHandle variable_indices;
        NativeMatHandle sample_indices;
        NativeMatHandle results;
        NativeMatHandle thetas;
        NativeMatHandle weights;
        if (jyppx_ocv_mat_create(8, 2, 5, samples.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create(8, 1, 5, logistic_responses.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create(8, 1, 5, svmsgd_responses.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create(1, 2, 5, negative_query.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create(1, 2, 5, positive_query.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create(1, 1, 4, variable_indices.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create(1, 3, 4, sample_indices.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(results.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(thetas.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_mat_create_empty(weights.out()) != OPENCV_CSHARP_STATUS_OK)
        {
            return 849;
        }

        const float sample_values[] = {
            -4.0F, -1.0F, -3.0F, 0.0F, -2.0F, -0.5F, -1.0F, 0.5F,
             1.0F, -0.5F,  2.0F, 0.5F,  3.0F,  0.0F,  4.0F, 1.0F
        };
        const float logistic_values[] = { 0.0F, 0.0F, 0.0F, 0.0F, 1.0F, 1.0F, 1.0F, 1.0F };
        const float svmsgd_values[] = { -1.0F, -1.0F, -1.0F, -1.0F, 1.0F, 1.0F, 1.0F, 1.0F };
        const float negative_query_values[] = { -2.5F, 0.0F };
        const float positive_query_values[] = { 2.5F, 0.0F };
        const int variable_index_values[] = { 1 };
        const int sample_index_values[] = { 1, 4, 5 };
        unsigned char* bytes = nullptr;
#define COPY_ML_REMAINING_VALUES(HANDLE, VALUES, CODE) \
        if (jyppx_ocv_mat_data((HANDLE).get(), &bytes) != OPENCV_CSHARP_STATUS_OK || bytes == nullptr) { return CODE; } \
        std::memcpy(bytes, VALUES, sizeof(VALUES))
        COPY_ML_REMAINING_VALUES(samples, sample_values, 850);
        COPY_ML_REMAINING_VALUES(logistic_responses, logistic_values, 850);
        COPY_ML_REMAINING_VALUES(svmsgd_responses, svmsgd_values, 850);
        COPY_ML_REMAINING_VALUES(negative_query, negative_query_values, 850);
        COPY_ML_REMAINING_VALUES(positive_query, positive_query_values, 850);
        COPY_ML_REMAINING_VALUES(variable_indices, variable_index_values, 850);
        COPY_ML_REMAINING_VALUES(sample_indices, sample_index_values, 850);
#undef COPY_ML_REMAINING_VALUES

        NativeMlTrainDataHandle train_data;
        if (jyppx_ocv_ml_train_data_create(
                samples.get(), 0, logistic_responses.get(), nullptr, nullptr, nullptr, nullptr, &train_data.value) != OPENCV_CSHARP_STATUS_OK ||
            train_data.value == nullptr)
        {
            return 851;
        }

        int count = -1;
        float sample_buffer[2] = {};
        float value_buffer[3] = {};
        if (jyppx_ocv_ml_train_data_get_sample_count(train_data.value, nullptr, &count) != OPENCV_CSHARP_STATUS_OK || count != 2 ||
            jyppx_ocv_ml_train_data_get_sample_count(train_data.value, variable_indices.get(), &count) != OPENCV_CSHARP_STATUS_OK || count != 1 ||
            jyppx_ocv_ml_train_data_get_sample_fill(train_data.value, nullptr, 4, sample_buffer, 2, &count) != OPENCV_CSHARP_STATUS_OK ||
            count != 2 || std::fabs(sample_buffer[0] - 1.0F) > 1e-6F || std::fabs(sample_buffer[1] + 0.5F) > 1e-6F)
        {
            return 852;
        }
        if (jyppx_ocv_ml_train_data_get_values_count(train_data.value, nullptr, &count) != OPENCV_CSHARP_STATUS_OK || count != 8 ||
            jyppx_ocv_ml_train_data_get_values_count(train_data.value, sample_indices.get(), &count) != OPENCV_CSHARP_STATUS_OK || count != 3 ||
            jyppx_ocv_ml_train_data_get_values_fill(train_data.value, 0, sample_indices.get(), value_buffer, 3, &count) != OPENCV_CSHARP_STATUS_OK ||
            count != 3 || std::fabs(value_buffer[0] + 3.0F) > 1e-6F || std::fabs(value_buffer[1] - 1.0F) > 1e-6F ||
            std::fabs(value_buffer[2] - 2.0F) > 1e-6F)
        {
            return 853;
        }
        if (jyppx_ocv_ml_train_data_get_sample_fill(
                train_data.value, variable_indices.get(), 0, sample_buffer, 2, &count) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT)
        {
            return 854;
        }

        NativeMlModelHandle logistic;
        if (jyppx_ocv_ml_logistic_regression_create(&logistic.value) != OPENCV_CSHARP_STATUS_OK || logistic.value == nullptr)
        {
            return 855;
        }
        double learning_rate = 0.0;
        int int_value = -1;
        int criteria_type = 0;
        int criteria_count = 0;
        double criteria_epsilon = 0.0;
        if (jyppx_ocv_ml_logistic_regression_get_learning_rate(logistic.value, &learning_rate) != OPENCV_CSHARP_STATUS_OK ||
            std::fabs(learning_rate - 0.001) > 1e-15 ||
            jyppx_ocv_ml_logistic_regression_get_int(logistic.value, 0, &int_value) != OPENCV_CSHARP_STATUS_OK || int_value != 1000 ||
            jyppx_ocv_ml_logistic_regression_get_int(logistic.value, 1, &int_value) != OPENCV_CSHARP_STATUS_OK || int_value != 1 ||
            jyppx_ocv_ml_logistic_regression_get_int(logistic.value, 2, &int_value) != OPENCV_CSHARP_STATUS_OK || int_value != 0 ||
            jyppx_ocv_ml_logistic_regression_get_int(logistic.value, 3, &int_value) != OPENCV_CSHARP_STATUS_OK || int_value != 1 ||
            jyppx_ocv_ml_logistic_regression_get_term_criteria(
                logistic.value, &criteria_type, &criteria_count, &criteria_epsilon) != OPENCV_CSHARP_STATUS_OK ||
            criteria_type != 3 || criteria_count != 1000 || std::fabs(criteria_epsilon - 0.001) > 1e-15)
        {
            return 856;
        }
        if (jyppx_ocv_ml_logistic_regression_set_learning_rate(logistic.value, 0.05) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_ml_logistic_regression_set_int(logistic.value, 0, 1000) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_ml_logistic_regression_set_int(logistic.value, 1, -1) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_ml_logistic_regression_set_int(logistic.value, 2, 1) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_ml_logistic_regression_set_int(logistic.value, 3, 2) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_ml_logistic_regression_set_term_criteria(logistic.value, 3, 1000, 1e-6) != OPENCV_CSHARP_STATUS_OK)
        {
            return 857;
        }
        int trained = 0;
        float prediction = 0.0F;
        if (jyppx_ocv_ml_stat_model_train_samples(
                logistic.value, samples.get(), 0, logistic_responses.get(), &trained) != OPENCV_CSHARP_STATUS_OK || trained != 1 ||
            jyppx_ocv_ml_stat_model_predict(logistic.value, negative_query.get(), results.get(), 0, &prediction) != OPENCV_CSHARP_STATUS_OK || prediction != 0.0F ||
            jyppx_ocv_ml_stat_model_predict(logistic.value, positive_query.get(), results.get(), 0, &prediction) != OPENCV_CSHARP_STATUS_OK || prediction != 1.0F ||
            jyppx_ocv_ml_logistic_regression_get_learnt_thetas(logistic.value, thetas.get()) != OPENCV_CSHARP_STATUS_OK)
        {
            return 858;
        }
        size_t total = 0;
        if (jyppx_ocv_mat_total(thetas.get(), &total) != OPENCV_CSHARP_STATUS_OK || total == 0)
        {
            return 859;
        }

        const char* logistic_path = "opencv-csharp-native-logistic-regression-smoke.yml";
        std::remove(logistic_path);
        if (jyppx_ocv_ml_stat_model_save(logistic.value, logistic_path) != OPENCV_CSHARP_STATUS_OK)
        {
            return 860;
        }
        NativeMlModelHandle loaded_logistic;
        if (jyppx_ocv_ml_logistic_regression_load(logistic_path, nullptr, &loaded_logistic.value) != OPENCV_CSHARP_STATUS_OK ||
            loaded_logistic.value == nullptr ||
            jyppx_ocv_ml_stat_model_predict(loaded_logistic.value, positive_query.get(), results.get(), 0, &prediction) != OPENCV_CSHARP_STATUS_OK ||
            prediction != 1.0F)
        {
            std::remove(logistic_path);
            return 861;
        }
        std::remove(logistic_path);

        NativeMlModelHandle svmsgd;
        if (jyppx_ocv_ml_svmsgd_create(&svmsgd.value) != OPENCV_CSHARP_STATUS_OK || svmsgd.value == nullptr)
        {
            return 862;
        }
        float float_value = 0.0F;
        if (jyppx_ocv_ml_svmsgd_get_int(svmsgd.value, 0, &int_value) != OPENCV_CSHARP_STATUS_OK || int_value != 1 ||
            jyppx_ocv_ml_svmsgd_get_int(svmsgd.value, 1, &int_value) != OPENCV_CSHARP_STATUS_OK || int_value != 0 ||
            jyppx_ocv_ml_svmsgd_get_float(svmsgd.value, 0, &float_value) != OPENCV_CSHARP_STATUS_OK || std::fabs(float_value - 0.00001F) > 1e-9F ||
            jyppx_ocv_ml_svmsgd_get_float(svmsgd.value, 1, &float_value) != OPENCV_CSHARP_STATUS_OK || std::fabs(float_value - 0.05F) > 1e-7F ||
            jyppx_ocv_ml_svmsgd_get_float(svmsgd.value, 2, &float_value) != OPENCV_CSHARP_STATUS_OK || std::fabs(float_value - 0.75F) > 1e-7F ||
            jyppx_ocv_ml_svmsgd_get_term_criteria(
                svmsgd.value, &criteria_type, &criteria_count, &criteria_epsilon) != OPENCV_CSHARP_STATUS_OK ||
            criteria_type != 3 || criteria_count != 100000 || std::fabs(criteria_epsilon - 0.00001) > 1e-15)
        {
            return 863;
        }
        if (jyppx_ocv_ml_svmsgd_set_int(svmsgd.value, 0, 0) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_ml_svmsgd_set_int(svmsgd.value, 1, 1) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_ml_svmsgd_set_float(svmsgd.value, 0, 0.001F) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_ml_svmsgd_set_float(svmsgd.value, 1, 0.1F) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_ml_svmsgd_set_float(svmsgd.value, 2, 0.5F) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_ml_svmsgd_set_optimal_parameters(svmsgd.value, 1, 0) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_ml_svmsgd_set_term_criteria(svmsgd.value, 3, 10000, 1e-6) != OPENCV_CSHARP_STATUS_OK)
        {
            return 864;
        }
        float shift = 0.0F;
        if (jyppx_ocv_ml_stat_model_train_samples(
                svmsgd.value, samples.get(), 0, svmsgd_responses.get(), &trained) != OPENCV_CSHARP_STATUS_OK || trained != 1)
        {
            return 865;
        }
        if (jyppx_ocv_ml_stat_model_predict(
                svmsgd.value, negative_query.get(), nullptr, 0, &prediction) != OPENCV_CSHARP_STATUS_OK || prediction != -1.0F)
        {
            return 866;
        }
        if (jyppx_ocv_ml_stat_model_predict(
                svmsgd.value, positive_query.get(), nullptr, 0, &prediction) != OPENCV_CSHARP_STATUS_OK || prediction != 1.0F)
        {
            return 867;
        }
        if (jyppx_ocv_ml_svmsgd_get_weights(svmsgd.value, weights.get()) != OPENCV_CSHARP_STATUS_OK)
        {
            return 868;
        }
        if (jyppx_ocv_ml_svmsgd_get_shift(svmsgd.value, &shift) != OPENCV_CSHARP_STATUS_OK || !std::isfinite(shift))
        {
            return 869;
        }
        if (jyppx_ocv_mat_total(weights.get(), &total) != OPENCV_CSHARP_STATUS_OK || total != 2)
        {
            return 870;
        }

        const char* svmsgd_path = "opencv-csharp-native-svmsgd-smoke.yml";
        std::remove(svmsgd_path);
        if (jyppx_ocv_ml_stat_model_save(svmsgd.value, svmsgd_path) != OPENCV_CSHARP_STATUS_OK)
        {
            return 871;
        }
        NativeMlModelHandle loaded_svmsgd;
        if (jyppx_ocv_ml_svmsgd_load(svmsgd_path, nullptr, &loaded_svmsgd.value) != OPENCV_CSHARP_STATUS_OK ||
            loaded_svmsgd.value == nullptr ||
            jyppx_ocv_ml_stat_model_predict(loaded_svmsgd.value, positive_query.get(), nullptr, 0, &prediction) != OPENCV_CSHARP_STATUS_OK ||
            prediction != 1.0F)
        {
            std::remove(svmsgd_path);
            return 872;
        }
        std::remove(svmsgd_path);
        return 0;
    }

    int run_tracking_legacy_completion_smoke()
    {
        if (jyppx_ocv_tracking_legacy_tracker_boosting_get_default_params(nullptr) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT ||
            jyppx_ocv_tracking_legacy_upgrade(nullptr, nullptr) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT)
        {
            return 873;
        }

        jyppx_ocv_tracking_boosting_params boosting_params{};
        if (jyppx_ocv_tracking_legacy_tracker_boosting_get_default_params(&boosting_params) != OPENCV_CSHARP_STATUS_OK ||
            boosting_params.num_classifiers != 100 ||
            std::abs(boosting_params.sampler_overlap - 0.99F) > 0.000001F ||
            std::abs(boosting_params.sampler_search_factor - 1.8F) > 0.000001F ||
            boosting_params.iteration_init != 50 ||
            boosting_params.feature_set_num_features != 1050)
        {
            return 874;
        }

        jyppx_ocv_tracking_legacy_tracker_boosting* boosting = nullptr;
        jyppx_ocv_tracking_legacy_tracker_boosting* boosting_explicit = nullptr;
        jyppx_ocv_tracking_legacy_tracker_tld* tld = nullptr;
        jyppx_ocv_tracking_legacy_tracker_kcf* kcf = nullptr;
        jyppx_ocv_tracking_legacy_tracker_kcf* kcf_explicit = nullptr;
        jyppx_ocv_tracking_legacy_tracker_csrt* csrt = nullptr;
        jyppx_ocv_tracking_legacy_tracker_csrt* csrt_explicit = nullptr;
        jyppx_ocv_tracking_tracker* upgraded = nullptr;
        auto cleanup = [&]()
        {
            jyppx_ocv_tracking_legacy_tracker_release_handle(reinterpret_cast<jyppx_ocv_tracking_legacy_tracker*>(boosting));
            jyppx_ocv_tracking_legacy_tracker_release_handle(reinterpret_cast<jyppx_ocv_tracking_legacy_tracker*>(boosting_explicit));
            jyppx_ocv_tracking_legacy_tracker_release_handle(reinterpret_cast<jyppx_ocv_tracking_legacy_tracker*>(tld));
            jyppx_ocv_tracking_legacy_tracker_release_handle(reinterpret_cast<jyppx_ocv_tracking_legacy_tracker*>(kcf));
            jyppx_ocv_tracking_legacy_tracker_release_handle(reinterpret_cast<jyppx_ocv_tracking_legacy_tracker*>(kcf_explicit));
            jyppx_ocv_tracking_legacy_tracker_release_handle(reinterpret_cast<jyppx_ocv_tracking_legacy_tracker*>(csrt));
            jyppx_ocv_tracking_legacy_tracker_release_handle(reinterpret_cast<jyppx_ocv_tracking_legacy_tracker*>(csrt_explicit));
            jyppx_ocv_tracking_tracker_release_handle(upgraded);
        };

        jyppx_ocv_tracking_kcf_params kcf_params{};
        jyppx_ocv_tracking_csrt_params csrt_params{};
        if (jyppx_ocv_tracking_tracker_kcf_get_default_params(&kcf_params) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_tracking_tracker_csrt_get_default_params(&csrt_params) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_tracking_legacy_tracker_boosting_create_default(&boosting) != OPENCV_CSHARP_STATUS_OK || boosting == nullptr ||
            jyppx_ocv_tracking_legacy_tracker_boosting_create(&boosting_params, &boosting_explicit) != OPENCV_CSHARP_STATUS_OK || boosting_explicit == nullptr ||
            jyppx_ocv_tracking_legacy_tracker_tld_create(&tld) != OPENCV_CSHARP_STATUS_OK || tld == nullptr ||
            jyppx_ocv_tracking_legacy_tracker_kcf_create_default(&kcf) != OPENCV_CSHARP_STATUS_OK || kcf == nullptr ||
            jyppx_ocv_tracking_legacy_tracker_kcf_create(&kcf_params, &kcf_explicit) != OPENCV_CSHARP_STATUS_OK || kcf_explicit == nullptr ||
            jyppx_ocv_tracking_legacy_tracker_csrt_create_default(&csrt) != OPENCV_CSHARP_STATUS_OK || csrt == nullptr ||
            jyppx_ocv_tracking_legacy_tracker_csrt_create(&csrt_params, &csrt_explicit) != OPENCV_CSHARP_STATUS_OK || csrt_explicit == nullptr)
        {
            cleanup();
            return 875;
        }

        NativeMatHandle mask;
        if (jyppx_ocv_mat_create_with_scalar(32, 32, 0, 255.0, 0.0, 0.0, 0.0, mask.out()) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_tracking_legacy_tracker_csrt_set_initial_mask(csrt, nullptr) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT ||
            jyppx_ocv_tracking_legacy_tracker_csrt_set_initial_mask(csrt, mask.get()) != OPENCV_CSHARP_STATUS_OK)
        {
            cleanup();
            return 876;
        }

        if (jyppx_ocv_tracking_legacy_upgrade(reinterpret_cast<jyppx_ocv_tracking_legacy_tracker*>(kcf), &upgraded) != OPENCV_CSHARP_STATUS_OK || upgraded == nullptr)
        {
            cleanup();
            return 877;
        }

        jyppx_ocv_tracking_legacy_tracker_release_handle(reinterpret_cast<jyppx_ocv_tracking_legacy_tracker*>(kcf));
        kcf = nullptr;
        NativeMatHandle first;
        NativeMatHandle second;
        if (!create_video_tracking_frame(0, first) || !create_video_tracking_frame(2, second))
        {
            cleanup();
            return 878;
        }

        jyppx_ocv_tracking_rect box{ 7, 7, 16, 18 };
        int result = 0;
        if (jyppx_ocv_tracking_tracker_init(upgraded, first.get(), box) != OPENCV_CSHARP_STATUS_OK ||
            jyppx_ocv_tracking_tracker_update(upgraded, second.get(), &box, &result) != OPENCV_CSHARP_STATUS_OK ||
            box.width < 0 || box.height < 0)
        {
            cleanup();
            return 879;
        }

        cleanup();
        return 0;
    }
#endif
}

int main()
{
    if (jyppx_ocv_get_version_major() != 5)
    {
        return 1;
    }

    jyppx_ocv_clear_last_error();
    if (std::strlen(jyppx_ocv_get_last_error()) != 0)
    {
        return 2;
    }

    jyppx_ocv_mat* mat = nullptr;
    int status = jyppx_ocv_mat_create_empty(&mat);
    if (status == OPENCV_CSHARP_STATUS_OK)
    {
        if (mat == nullptr)
        {
            return 3;
        }

#if !defined(OPENCV_CSHARP_RUNTIME_PROFILE_MINI)
        int calib3d_smoke_status = run_calib3d_upstream_parity_smoke();
        if (calib3d_smoke_status != 0)
        {
            jyppx_ocv_mat_release(mat);
            return calib3d_smoke_status;
        }

        int dnn_structured_smoke_status = run_dnn_structured_smoke();
        if (dnn_structured_smoke_status != 0)
        {
            jyppx_ocv_mat_release(mat);
            return dnn_structured_smoke_status;
        }

        int features_ann_status = run_features_ann_index_smoke();
        if (features_ann_status != 0)
        {
            jyppx_ocv_mat_release(mat);
            return features_ann_status;
        }

        int objdetect_structured_status = run_objdetect_structured_smoke();
        if (objdetect_structured_status != 0)
        {
            jyppx_ocv_mat_release(mat);
            return objdetect_structured_status;
        }

        int photo_hdr_status = run_photo_hdr_smoke();
        if (photo_hdr_status != 0)
        {
            jyppx_ocv_mat_release(mat);
            return photo_hdr_status;
        }

        int photo_ccm_status = run_photo_ccm_smoke();
        if (photo_ccm_status != 0)
        {
            jyppx_ocv_mat_release(mat);
            return photo_ccm_status;
        }

        int photo_intelligent_scissors_status = run_photo_intelligent_scissors_smoke();
        if (photo_intelligent_scissors_status != 0)
        {
            jyppx_ocv_mat_release(mat);
            return photo_intelligent_scissors_status;
        }

        int photo_final_callables_status = run_photo_final_callables_smoke();
        if (photo_final_callables_status != 0)
        {
            jyppx_ocv_mat_release(mat);
            return photo_final_callables_status;
        }

        int ml_ann_mlp_status = run_ml_ann_mlp_smoke();
        if (ml_ann_mlp_status != 0)
        {
            jyppx_ocv_mat_release(mat);
            return ml_ann_mlp_status;
        }

        int ml_tree_models_status = run_ml_tree_models_smoke();
        if (ml_tree_models_status != 0)
        {
            jyppx_ocv_mat_release(mat);
            return ml_tree_models_status;
        }

        int ml_em_status = run_ml_em_smoke();
        if (ml_em_status != 0)
        {
            jyppx_ocv_mat_release(mat);
            return ml_em_status;
        }

        int ml_remaining_callables_status = run_ml_remaining_callables_smoke();
        if (ml_remaining_callables_status != 0)
        {
            jyppx_ocv_mat_release(mat);
            return ml_remaining_callables_status;
        }

        int tracking_legacy_completion_status = run_tracking_legacy_completion_smoke();
        if (tracking_legacy_completion_status != 0)
        {
            jyppx_ocv_mat_release(mat);
            return tracking_legacy_completion_status;
        }
#endif

        int empty = 0;
        if (jyppx_ocv_mat_empty(mat, &empty) != OPENCV_CSHARP_STATUS_OK || empty != 1)
        {
            jyppx_ocv_mat_release(mat);
            return 4;
        }

        jyppx_ocv_mat_release(mat);

        status = jyppx_ocv_mat_create(2, 3, 0, &mat);
        if (status != OPENCV_CSHARP_STATUS_OK || mat == nullptr)
        {
            return 6;
        }

        size_t total = 0;
        size_t elem_size = 0;
        size_t step = 0;
        unsigned char* data = nullptr;
        int is_continuous = 0;

        if (jyppx_ocv_mat_total(mat, &total) != OPENCV_CSHARP_STATUS_OK || total != 6)
        {
            jyppx_ocv_mat_release(mat);
            return 7;
        }

        if (jyppx_ocv_mat_elem_size(mat, &elem_size) != OPENCV_CSHARP_STATUS_OK || elem_size != 1)
        {
            jyppx_ocv_mat_release(mat);
            return 8;
        }

        if (jyppx_ocv_mat_step(mat, &step) != OPENCV_CSHARP_STATUS_OK || step < 3)
        {
            jyppx_ocv_mat_release(mat);
            return 9;
        }

        if (jyppx_ocv_mat_data(mat, &data) != OPENCV_CSHARP_STATUS_OK || data == nullptr)
        {
            jyppx_ocv_mat_release(mat);
            return 10;
        }

        if (jyppx_ocv_mat_is_continuous(mat, &is_continuous) != OPENCV_CSHARP_STATUS_OK || is_continuous != 1)
        {
            jyppx_ocv_mat_release(mat);
            return 11;
        }

        jyppx_ocv_mat_release(mat);
        mat = nullptr;

        int mat_object_status = run_mat_object_api_smoke();
        if (mat_object_status != 0)
        {
            return mat_object_status;
        }

        int core_upstream_status = run_core_upstream_parity_smoke();
        if (core_upstream_status != 0)
        {
            return core_upstream_status;
        }

        int core_persistence_status = run_core_persistence_smoke();
        if (core_persistence_status != 0)
        {
            return core_persistence_status;
        }

        int core_numerical_status = run_core_numerical_collection_solver_smoke();
        if (core_numerical_status != 0)
        {
            return core_numerical_status;
        }

        int core_runtime_status = run_core_runtime_diagnostics_timing_smoke();
        if (core_runtime_status != 0)
        {
            return core_runtime_status;
        }

        int videoio_status = run_videoio_smoke();
        if (videoio_status != 0)
        {
            return videoio_status;
        }

#if !defined(OPENCV_CSHARP_RUNTIME_PROFILE_MINI)
        int video_optical_flow_status = run_video_optical_flow_object_smoke();
        if (video_optical_flow_status != 0)
        {
            return video_optical_flow_status;
        }

        int video_ecc_tracker_status = run_video_ecc_tracker_mil_smoke();
        if (video_ecc_tracker_status != 0)
        {
            return video_ecc_tracker_status;
        }
#endif

        int filter_transform_status = run_imgproc_filter_transform_api_smoke();
        if (filter_transform_status != 0)
        {
            return filter_transform_status;
        }

        int upstream_parity_status = run_imgproc_upstream_parity_api_smoke();
        if (upstream_parity_status != 0)
        {
            return upstream_parity_status;
        }

        int remaining_parity_status = run_imgproc_remaining_parity_api_smoke();
        if (remaining_parity_status != 0)
        {
            return remaining_parity_status;
        }

#if !defined(OPENCV_CSHARP_RUNTIME_PROFILE_MINI)
        int imgcodecs_parity_status = run_imgcodecs_upstream_parity_api_smoke();
        if (imgcodecs_parity_status != 0)
        {
            return imgcodecs_parity_status;
        }
#endif

        int mini_excluded_features_status = run_mini_excluded_features_smoke();
        if (mini_excluded_features_status != 0)
        {
            return mini_excluded_features_status;
        }

        jyppx_ocv_mat* src_color = nullptr;
        jyppx_ocv_mat* gray = nullptr;
        jyppx_ocv_mat* resized = nullptr;
        jyppx_ocv_mat* thresholded = nullptr;
        jyppx_ocv_mat* blur_src = nullptr;
        jyppx_ocv_mat* blurred = nullptr;

        if (jyppx_ocv_mat_create(2, 2, 64, &src_color) != OPENCV_CSHARP_STATUS_OK)
        {
            return 14;
        }

        if (jyppx_ocv_mat_create_empty(&gray) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(src_color);
            return 15;
        }

        if (jyppx_ocv_imgproc_cvt_color(src_color, gray, 6, 0) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(src_color);
            jyppx_ocv_mat_release(gray);
            return 16;
        }

        int channels = 0;
        if (jyppx_ocv_mat_channels(gray, &channels) != OPENCV_CSHARP_STATUS_OK || channels != 1)
        {
            jyppx_ocv_mat_release(src_color);
            jyppx_ocv_mat_release(gray);
            return 17;
        }

        if (jyppx_ocv_mat_create_empty(&resized) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(src_color);
            jyppx_ocv_mat_release(gray);
            return 18;
        }

        if (jyppx_ocv_imgproc_resize(gray, resized, 4, 4, 0.0, 0.0, 1) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(src_color);
            jyppx_ocv_mat_release(gray);
            jyppx_ocv_mat_release(resized);
            return 19;
        }

        int rows = 0;
        int cols = 0;
        if (jyppx_ocv_mat_rows(resized, &rows) != OPENCV_CSHARP_STATUS_OK || rows != 4)
        {
            jyppx_ocv_mat_release(src_color);
            jyppx_ocv_mat_release(gray);
            jyppx_ocv_mat_release(resized);
            return 20;
        }

        if (jyppx_ocv_mat_cols(resized, &cols) != OPENCV_CSHARP_STATUS_OK || cols != 4)
        {
            jyppx_ocv_mat_release(src_color);
            jyppx_ocv_mat_release(gray);
            jyppx_ocv_mat_release(resized);
            return 21;
        }

        jyppx_ocv_mat_release(src_color);
        jyppx_ocv_mat_release(gray);
        jyppx_ocv_mat_release(resized);

        if (jyppx_ocv_mat_create(2, 3, 0, &mat) != OPENCV_CSHARP_STATUS_OK)
        {
            return 22;
        }

        if (jyppx_ocv_mat_data(mat, &data) != OPENCV_CSHARP_STATUS_OK || data == nullptr)
        {
            jyppx_ocv_mat_release(mat);
            return 23;
        }

        data[0] = 0;
        data[1] = 80;
        data[2] = 120;
        data[3] = 160;
        data[4] = 200;
        data[5] = 255;

        if (jyppx_ocv_mat_create_empty(&thresholded) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(mat);
            return 24;
        }

        double used_threshold = 0.0;
        if (jyppx_ocv_imgproc_threshold(mat, thresholded, 127.0, 255.0, 0, &used_threshold) != OPENCV_CSHARP_STATUS_OK || used_threshold != 127.0)
        {
            jyppx_ocv_mat_release(mat);
            jyppx_ocv_mat_release(thresholded);
            return 25;
        }

        if (jyppx_ocv_mat_data(thresholded, &data) != OPENCV_CSHARP_STATUS_OK || data == nullptr)
        {
            jyppx_ocv_mat_release(mat);
            jyppx_ocv_mat_release(thresholded);
            return 26;
        }

        if (data[0] != 0 || data[1] != 0 || data[2] != 0 || data[3] != 255 || data[4] != 255 || data[5] != 255)
        {
            jyppx_ocv_mat_release(mat);
            jyppx_ocv_mat_release(thresholded);
            return 27;
        }

        jyppx_ocv_mat_release(mat);
        jyppx_ocv_mat_release(thresholded);
        mat = nullptr;
        thresholded = nullptr;

        if (jyppx_ocv_mat_create(3, 3, 0, &blur_src) != OPENCV_CSHARP_STATUS_OK)
        {
            return 28;
        }

        if (jyppx_ocv_mat_data(blur_src, &data) != OPENCV_CSHARP_STATUS_OK || data == nullptr)
        {
            jyppx_ocv_mat_release(blur_src);
            return 29;
        }

        for (int i = 0; i < 9; ++i)
        {
            data[i] = 0;
        }

        data[4] = 255;

        if (jyppx_ocv_mat_create_empty(&blurred) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(blur_src);
            return 30;
        }

        if (jyppx_ocv_imgproc_gaussian_blur(blur_src, blurred, 3, 3, 0.0, 0.0, 4) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(blur_src);
            jyppx_ocv_mat_release(blurred);
            return 31;
        }

        if (jyppx_ocv_mat_data(blurred, &data) != OPENCV_CSHARP_STATUS_OK || data == nullptr)
        {
            jyppx_ocv_mat_release(blur_src);
            jyppx_ocv_mat_release(blurred);
            return 32;
        }

        if (data[4] >= 255 || data[4] == 0)
        {
            jyppx_ocv_mat_release(blur_src);
            jyppx_ocv_mat_release(blurred);
            return 33;
        }

        jyppx_ocv_mat_release(blur_src);
        jyppx_ocv_mat_release(blurred);

        jyppx_ocv_mat* kernel = nullptr;
        if (jyppx_ocv_imgproc_get_structuring_element(0, 3, 3, -1, -1, &kernel) != OPENCV_CSHARP_STATUS_OK || kernel == nullptr)
        {
            return 34;
        }

        if (jyppx_ocv_mat_rows(kernel, &rows) != OPENCV_CSHARP_STATUS_OK || rows != 3)
        {
            jyppx_ocv_mat_release(kernel);
            return 35;
        }

        if (jyppx_ocv_mat_cols(kernel, &cols) != OPENCV_CSHARP_STATUS_OK || cols != 3)
        {
            jyppx_ocv_mat_release(kernel);
            return 36;
        }

        int type = -1;
        if (jyppx_ocv_mat_type(kernel, &type) != OPENCV_CSHARP_STATUS_OK || type != 0)
        {
            jyppx_ocv_mat_release(kernel);
            return 37;
        }

        if (jyppx_ocv_mat_data(kernel, &data) != OPENCV_CSHARP_STATUS_OK || data == nullptr)
        {
            jyppx_ocv_mat_release(kernel);
            return 38;
        }

        for (int i = 0; i < 9; ++i)
        {
            if (data[i] != 1)
            {
                jyppx_ocv_mat_release(kernel);
                return 39;
            }
        }

        jyppx_ocv_mat_release(kernel);

        jyppx_ocv_mat* morphology_src = nullptr;
        jyppx_ocv_mat* eroded = nullptr;
        jyppx_ocv_mat* dilated = nullptr;
        if (jyppx_ocv_mat_create(5, 5, 0, &morphology_src) != OPENCV_CSHARP_STATUS_OK)
        {
            return 40;
        }

        if (jyppx_ocv_mat_data(morphology_src, &data) != OPENCV_CSHARP_STATUS_OK || data == nullptr)
        {
            jyppx_ocv_mat_release(morphology_src);
            return 41;
        }

        for (int i = 0; i < 25; ++i)
        {
            data[i] = 255;
        }

        if (jyppx_ocv_imgproc_get_structuring_element(0, 3, 3, -1, -1, &kernel) != OPENCV_CSHARP_STATUS_OK || kernel == nullptr)
        {
            jyppx_ocv_mat_release(morphology_src);
            return 42;
        }

        if (jyppx_ocv_mat_create_empty(&eroded) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(morphology_src);
            jyppx_ocv_mat_release(kernel);
            return 43;
        }

        if (jyppx_ocv_imgproc_erode(morphology_src, eroded, kernel, -1, -1, 1, 0, 0, 0.0, 0.0, 0.0, 0.0) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(morphology_src);
            jyppx_ocv_mat_release(kernel);
            jyppx_ocv_mat_release(eroded);
            return 44;
        }

        if (jyppx_ocv_mat_data(eroded, &data) != OPENCV_CSHARP_STATUS_OK || data == nullptr)
        {
            jyppx_ocv_mat_release(morphology_src);
            jyppx_ocv_mat_release(kernel);
            jyppx_ocv_mat_release(eroded);
            return 45;
        }

        for (int i = 0; i < 25; ++i)
        {
            if (data[i] != 255)
            {
                jyppx_ocv_mat_release(morphology_src);
                jyppx_ocv_mat_release(kernel);
                jyppx_ocv_mat_release(eroded);
                return 46;
            }
        }

        if (jyppx_ocv_mat_data(morphology_src, &data) != OPENCV_CSHARP_STATUS_OK || data == nullptr)
        {
            jyppx_ocv_mat_release(morphology_src);
            jyppx_ocv_mat_release(kernel);
            jyppx_ocv_mat_release(eroded);
            return 47;
        }

        for (int i = 0; i < 25; ++i)
        {
            data[i] = 0;
        }

        data[12] = 255;

        if (jyppx_ocv_mat_create_empty(&dilated) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(morphology_src);
            jyppx_ocv_mat_release(kernel);
            jyppx_ocv_mat_release(eroded);
            return 48;
        }

        if (jyppx_ocv_imgproc_dilate(morphology_src, dilated, kernel, -1, -1, 1, 0, 0, 0.0, 0.0, 0.0, 0.0) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(morphology_src);
            jyppx_ocv_mat_release(kernel);
            jyppx_ocv_mat_release(eroded);
            jyppx_ocv_mat_release(dilated);
            return 49;
        }

        if (jyppx_ocv_mat_data(dilated, &data) != OPENCV_CSHARP_STATUS_OK || data == nullptr)
        {
            jyppx_ocv_mat_release(morphology_src);
            jyppx_ocv_mat_release(kernel);
            jyppx_ocv_mat_release(eroded);
            jyppx_ocv_mat_release(dilated);
            return 50;
        }

        if (data[6] != 255 || data[7] != 255 || data[8] != 255 ||
            data[11] != 255 || data[12] != 255 || data[13] != 255 ||
            data[16] != 255 || data[17] != 255 || data[18] != 255)
        {
            jyppx_ocv_mat_release(morphology_src);
            jyppx_ocv_mat_release(kernel);
            jyppx_ocv_mat_release(eroded);
            jyppx_ocv_mat_release(dilated);
            return 51;
        }

        jyppx_ocv_mat_release(morphology_src);
        jyppx_ocv_mat_release(kernel);
        jyppx_ocv_mat_release(eroded);
        jyppx_ocv_mat_release(dilated);

        morphology_src = nullptr;
        kernel = nullptr;
        jyppx_ocv_mat* opened = nullptr;

        if (jyppx_ocv_mat_create(5, 5, 0, &morphology_src) != OPENCV_CSHARP_STATUS_OK)
        {
            return 52;
        }

        if (jyppx_ocv_mat_data(morphology_src, &data) != OPENCV_CSHARP_STATUS_OK || data == nullptr)
        {
            jyppx_ocv_mat_release(morphology_src);
            return 53;
        }

        for (int i = 0; i < 25; ++i)
        {
            data[i] = 0;
        }

        data[6] = 255;
        data[7] = 255;
        data[8] = 255;
        data[11] = 255;
        data[12] = 255;
        data[13] = 255;
        data[16] = 255;
        data[17] = 255;
        data[18] = 255;

        if (jyppx_ocv_imgproc_get_structuring_element(0, 3, 3, -1, -1, &kernel) != OPENCV_CSHARP_STATUS_OK || kernel == nullptr)
        {
            jyppx_ocv_mat_release(morphology_src);
            return 54;
        }

        if (jyppx_ocv_mat_create_empty(&opened) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(morphology_src);
            jyppx_ocv_mat_release(kernel);
            return 55;
        }

        if (jyppx_ocv_imgproc_morphology_ex(morphology_src, opened, 2, kernel, -1, -1, 1, 0, 0, 0.0, 0.0, 0.0, 0.0) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(morphology_src);
            jyppx_ocv_mat_release(kernel);
            jyppx_ocv_mat_release(opened);
            return 56;
        }

        if (jyppx_ocv_mat_data(opened, &data) != OPENCV_CSHARP_STATUS_OK || data == nullptr)
        {
            jyppx_ocv_mat_release(morphology_src);
            jyppx_ocv_mat_release(kernel);
            jyppx_ocv_mat_release(opened);
            return 57;
        }

        if (data[6] != 255 || data[7] != 255 || data[8] != 255 ||
            data[11] != 255 || data[12] != 255 || data[13] != 255 ||
            data[16] != 255 || data[17] != 255 || data[18] != 255)
        {
            jyppx_ocv_mat_release(morphology_src);
            jyppx_ocv_mat_release(kernel);
            jyppx_ocv_mat_release(opened);
            return 58;
        }

        jyppx_ocv_mat_release(morphology_src);
        jyppx_ocv_mat_release(kernel);
        jyppx_ocv_mat_release(opened);

        jyppx_ocv_mat* drawing = nullptr;
        if (jyppx_ocv_mat_create(5, 5, 0, &drawing) != OPENCV_CSHARP_STATUS_OK)
        {
            return 59;
        }

        if (jyppx_ocv_mat_data(drawing, &data) != OPENCV_CSHARP_STATUS_OK || data == nullptr)
        {
            jyppx_ocv_mat_release(drawing);
            return 60;
        }

        for (int i = 0; i < 25; ++i)
        {
            data[i] = 0;
        }

        if (jyppx_ocv_imgproc_line(drawing, 0, 0, 4, 4, 255.0, 0.0, 0.0, 0.0, 1, 8, 0) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(drawing);
            return 61;
        }

        if (jyppx_ocv_mat_data(drawing, &data) != OPENCV_CSHARP_STATUS_OK || data == nullptr)
        {
            jyppx_ocv_mat_release(drawing);
            return 62;
        }

        if (data[0] != 255 || data[6] != 255 || data[12] != 255 || data[18] != 255 || data[24] != 255)
        {
            jyppx_ocv_mat_release(drawing);
            return 63;
        }

        if (jyppx_ocv_imgproc_rectangle_by_rect(drawing, 1, 1, 3, 3, 128.0, 0.0, 0.0, 0.0, -1, 8, 0) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(drawing);
            return 64;
        }

        if (jyppx_ocv_mat_data(drawing, &data) != OPENCV_CSHARP_STATUS_OK || data == nullptr)
        {
            jyppx_ocv_mat_release(drawing);
            return 65;
        }

        if (data[6] != 128 || data[7] != 128 || data[8] != 128 ||
            data[11] != 128 || data[12] != 128 || data[13] != 128 ||
            data[16] != 128 || data[17] != 128 || data[18] != 128)
        {
            jyppx_ocv_mat_release(drawing);
            return 66;
        }

        for (int i = 0; i < 25; ++i)
        {
            data[i] = 0;
        }

        if (jyppx_ocv_imgproc_circle(drawing, 2, 2, 2, 255.0, 0.0, 0.0, 0.0, -1, 8, 0) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(drawing);
            return 67;
        }

        if (jyppx_ocv_mat_data(drawing, &data) != OPENCV_CSHARP_STATUS_OK || data == nullptr)
        {
            jyppx_ocv_mat_release(drawing);
            return 68;
        }

        if (data[2] != 255 || data[6] != 255 || data[7] != 255 || data[8] != 255 ||
            data[10] != 255 || data[11] != 255 || data[12] != 255 || data[13] != 255 || data[14] != 255 ||
            data[16] != 255 || data[17] != 255 || data[18] != 255 || data[22] != 255)
        {
            jyppx_ocv_mat_release(drawing);
            return 69;
        }

        for (int i = 0; i < 25; ++i)
        {
            data[i] = 0;
        }

        if (jyppx_ocv_imgproc_ellipse(drawing, 2, 2, 2, 1, 0.0, 0.0, 360.0, 128.0, 0.0, 0.0, 0.0, -1, 8, 0) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(drawing);
            return 70;
        }

        if (jyppx_ocv_mat_data(drawing, &data) != OPENCV_CSHARP_STATUS_OK || data == nullptr)
        {
            jyppx_ocv_mat_release(drawing);
            return 71;
        }

        if (data[10] != 128 || data[11] != 128 || data[12] != 128 || data[13] != 128 || data[14] != 128)
        {
            jyppx_ocv_mat_release(drawing);
            return 72;
        }

        jyppx_ocv_mat_release(drawing);
        drawing = nullptr;

        if (jyppx_ocv_mat_create(40, 80, 0, &drawing) != OPENCV_CSHARP_STATUS_OK)
        {
            return 73;
        }

        if (jyppx_ocv_mat_data(drawing, &data) != OPENCV_CSHARP_STATUS_OK || data == nullptr)
        {
            jyppx_ocv_mat_release(drawing);
            return 74;
        }

        for (int i = 0; i < 3200; ++i)
        {
            data[i] = 0;
        }

        if (jyppx_ocv_imgproc_put_text(drawing, "A", 4, 28, 0, 0.8, 255.0, 0.0, 0.0, 0.0, 1, 8, 0) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(drawing);
            return 75;
        }

        if (jyppx_ocv_mat_data(drawing, &data) != OPENCV_CSHARP_STATUS_OK || data == nullptr)
        {
            jyppx_ocv_mat_release(drawing);
            return 76;
        }

        bool has_text_pixels = false;
        for (int i = 0; i < 3200; ++i)
        {
            if (data[i] != 0)
            {
                has_text_pixels = true;
                break;
            }
        }

        if (!has_text_pixels)
        {
            jyppx_ocv_mat_release(drawing);
            return 77;
        }

        int text_width = 0;
        int text_height = 0;
        int text_base_line = 0;
        if (jyppx_ocv_imgproc_get_text_size("A", 0, 0.8, 1, &text_width, &text_height, &text_base_line) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(drawing);
            return 78;
        }

        if (text_width <= 0 || text_height <= 0 || text_base_line < 0)
        {
            jyppx_ocv_mat_release(drawing);
            return 79;
        }

        for (int i = 0; i < 3200; ++i)
        {
            data[i] = 0;
        }

        if (jyppx_ocv_imgproc_arrowed_line(drawing, 4, 20, 72, 20, 180.0, 0.0, 0.0, 0.0, 1, 8, 0, 0.2) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(drawing);
            return 80;
        }

        if (jyppx_ocv_mat_data(drawing, &data) != OPENCV_CSHARP_STATUS_OK || data == nullptr)
        {
            jyppx_ocv_mat_release(drawing);
            return 81;
        }

        if (data[20 * 80 + 4] != 180 || data[20 * 80 + 40] != 180 || data[20 * 80 + 72] != 180)
        {
            jyppx_ocv_mat_release(drawing);
            return 82;
        }

        int clip_pt1_x = -5;
        int clip_pt1_y = 2;
        int clip_pt2_x = 15;
        int clip_pt2_y = 2;
        int clip_intersects = 0;
        if (jyppx_ocv_imgproc_clip_line_rect(0, 0, 10, 10, &clip_pt1_x, &clip_pt1_y, &clip_pt2_x, &clip_pt2_y, &clip_intersects) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(drawing);
            return 83;
        }

        if (clip_intersects != 1 || clip_pt1_x != 0 || clip_pt1_y != 2 || clip_pt2_x != 9 || clip_pt2_y != 2)
        {
            jyppx_ocv_mat_release(drawing);
            return 84;
        }

        clip_pt1_x = -5;
        clip_pt1_y = -5;
        clip_pt2_x = -1;
        clip_pt2_y = -1;
        clip_intersects = 1;
        if (jyppx_ocv_imgproc_clip_line_rect(0, 0, 10, 10, &clip_pt1_x, &clip_pt1_y, &clip_pt2_x, &clip_pt2_y, &clip_intersects) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(drawing);
            return 85;
        }

        if (clip_intersects != 0)
        {
            jyppx_ocv_mat_release(drawing);
            return 86;
        }

        jyppx_ocv_mat_release(drawing);
        drawing = nullptr;

        if (jyppx_ocv_mat_create(10, 10, 0, &drawing) != OPENCV_CSHARP_STATUS_OK)
        {
            return 87;
        }

        if (jyppx_ocv_mat_data(drawing, &data) != OPENCV_CSHARP_STATUS_OK || data == nullptr)
        {
            jyppx_ocv_mat_release(drawing);
            return 88;
        }

        for (int i = 0; i < 100; ++i)
        {
            data[i] = 0;
        }

        const int polyline_points[] = { 1, 1, 8, 1, 8, 8 };
        if (jyppx_ocv_imgproc_polylines(drawing, polyline_points, 3, 1, 210.0, 0.0, 0.0, 0.0, 1, 8, 0) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(drawing);
            return 89;
        }

        if (jyppx_ocv_mat_data(drawing, &data) != OPENCV_CSHARP_STATUS_OK || data == nullptr)
        {
            jyppx_ocv_mat_release(drawing);
            return 90;
        }

        if (data[1 * 10 + 1] != 210 || data[1 * 10 + 8] != 210 || data[8 * 10 + 8] != 210)
        {
            jyppx_ocv_mat_release(drawing);
            return 91;
        }

        for (int i = 0; i < 100; ++i)
        {
            data[i] = 0;
        }

        const int fill_poly_points[] = { 2, 2, 7, 2, 7, 7, 2, 7 };
        if (jyppx_ocv_imgproc_fill_poly(drawing, fill_poly_points, 4, 160.0, 0.0, 0.0, 0.0, 8, 0, 0, 0) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(drawing);
            return 92;
        }

        if (jyppx_ocv_mat_data(drawing, &data) != OPENCV_CSHARP_STATUS_OK || data == nullptr)
        {
            jyppx_ocv_mat_release(drawing);
            return 93;
        }

        if (data[2 * 10 + 2] != 160 || data[4 * 10 + 4] != 160 || data[7 * 10 + 7] != 160)
        {
            jyppx_ocv_mat_release(drawing);
            return 94;
        }

        int ellipse_point_count = 0;
        if (jyppx_ocv_imgproc_ellipse2_poly_count(10, 10, 5, 3, 0, 0, 90, 30, &ellipse_point_count) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(drawing);
            return 95;
        }

        if (ellipse_point_count <= 0)
        {
            jyppx_ocv_mat_release(drawing);
            return 96;
        }

        int ellipse_points_xy[64] = {};
        int ellipse_written_count = 0;
        if (jyppx_ocv_imgproc_ellipse2_poly_fill(10, 10, 5, 3, 0, 0, 90, 30, ellipse_points_xy, 32, &ellipse_written_count) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(drawing);
            return 97;
        }

        if (ellipse_written_count != ellipse_point_count || ellipse_points_xy[0] != 15 || ellipse_points_xy[1] != 10)
        {
            jyppx_ocv_mat_release(drawing);
            return 98;
        }

        const int contour_area_points[] = { 0, 0, 4, 0, 4, 3, 0, 3 };
        double contour_area = 0.0;
        if (jyppx_ocv_imgproc_contour_area(contour_area_points, 4, 0, &contour_area) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(drawing);
            return 99;
        }

        if (contour_area != 12.0)
        {
            jyppx_ocv_mat_release(drawing);
            return 100;
        }

        const int arc_length_points[] = { 0, 0, 4, 0, 4, 3, 0, 3 };
        double arc_length_open = 0.0;
        double arc_length_closed = 0.0;
        if (jyppx_ocv_imgproc_arc_length(arc_length_points, 4, 0, &arc_length_open) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(drawing);
            return 101;
        }

        if (jyppx_ocv_imgproc_arc_length(arc_length_points, 4, 1, &arc_length_closed) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(drawing);
            return 102;
        }

        if (arc_length_open != 11.0 || arc_length_closed != 14.0)
        {
            jyppx_ocv_mat_release(drawing);
            return 103;
        }

        const int approx_poly_curve[] = { 0, 0, 2, 0, 4, 0, 4, 3, 0, 3 };
        int approx_poly_count = 0;
        if (jyppx_ocv_imgproc_approx_poly_dp_count(approx_poly_curve, 5, 0.5, 1, &approx_poly_count) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(drawing);
            return 104;
        }

        if (approx_poly_count != 4)
        {
            jyppx_ocv_mat_release(drawing);
            return 105;
        }

        int approx_poly_points_xy[8] = {};
        int approx_poly_written_count = 0;
        if (jyppx_ocv_imgproc_approx_poly_dp_fill(approx_poly_curve, 5, 0.5, 1, approx_poly_points_xy, 4, &approx_poly_written_count) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(drawing);
            return 106;
        }

        if (approx_poly_written_count != 4 ||
            approx_poly_points_xy[0] != 0 || approx_poly_points_xy[1] != 0 ||
            approx_poly_points_xy[2] != 4 || approx_poly_points_xy[3] != 0 ||
            approx_poly_points_xy[4] != 4 || approx_poly_points_xy[5] != 3 ||
            approx_poly_points_xy[6] != 0 || approx_poly_points_xy[7] != 3)
        {
            jyppx_ocv_mat_release(drawing);
            return 107;
        }

        int bounding_x = 0;
        int bounding_y = 0;
        int bounding_width = 0;
        int bounding_height = 0;
        if (jyppx_ocv_imgproc_bounding_rect(approx_poly_curve, 5, &bounding_x, &bounding_y, &bounding_width, &bounding_height) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(drawing);
            return 108;
        }

        if (bounding_x != 0 || bounding_y != 0 || bounding_width != 5 || bounding_height != 4)
        {
            jyppx_ocv_mat_release(drawing);
            return 109;
        }

        int is_convex = 0;
        if (jyppx_ocv_imgproc_is_contour_convex(approx_poly_points_xy, 4, &is_convex) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(drawing);
            return 110;
        }

        if (is_convex != 1)
        {
            jyppx_ocv_mat_release(drawing);
            return 111;
        }

        int hull_count = 0;
        if (jyppx_ocv_imgproc_convex_hull_count(approx_poly_curve, 5, 0, &hull_count) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(drawing);
            return 112;
        }

        if (hull_count != 4)
        {
            jyppx_ocv_mat_release(drawing);
            return 113;
        }

        int hull_points_xy[8] = {};
        int hull_written_count = 0;
        if (jyppx_ocv_imgproc_convex_hull_fill(approx_poly_curve, 5, 0, hull_points_xy, 4, &hull_written_count) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(drawing);
            return 114;
        }

        if (hull_written_count != hull_count)
        {
            jyppx_ocv_mat_release(drawing);
            return 115;
        }

        const int circle_points[] = { 0, 0, 4, 0 };
        float circle_center_x = 0.0F;
        float circle_center_y = 0.0F;
        float circle_radius = 0.0F;
        if (jyppx_ocv_imgproc_min_enclosing_circle(circle_points, 2, &circle_center_x, &circle_center_y, &circle_radius) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(drawing);
            return 116;
        }

        if (circle_center_x < 1.999F || circle_center_x > 2.001F ||
            circle_center_y < -0.001F || circle_center_y > 0.001F ||
            circle_radius < 1.999F || circle_radius > 2.001F)
        {
            jyppx_ocv_mat_release(drawing);
            return 117;
        }

        double polygon_inside = 0.0;
        double polygon_outside_distance = 0.0;
        if (jyppx_ocv_imgproc_point_polygon_test(contour_area_points, 4, 2.0F, 1.0F, 0, &polygon_inside) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(drawing);
            return 118;
        }

        if (jyppx_ocv_imgproc_point_polygon_test(contour_area_points, 4, 6.0F, 1.0F, 1, &polygon_outside_distance) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(drawing);
            return 119;
        }

        if (polygon_inside <= 0.0 || polygon_outside_distance >= -1.999 || polygon_outside_distance <= -2.001)
        {
            jyppx_ocv_mat_release(drawing);
            return 120;
        }

        double shape_distance_same = 1.0;
        double shape_distance_other = 0.0;
        const int triangle_points[] = { 0, 0, 4, 0, 2, 3 };
        if (jyppx_ocv_imgproc_match_shapes(contour_area_points, 4, contour_area_points, 4, 1, 0.0, &shape_distance_same) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(drawing);
            return 121;
        }

        if (jyppx_ocv_imgproc_match_shapes(contour_area_points, 4, triangle_points, 3, 1, 0.0, &shape_distance_other) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(drawing);
            return 122;
        }

        if (shape_distance_same < -0.000001 || shape_distance_same > 0.000001 || shape_distance_other <= shape_distance_same)
        {
            jyppx_ocv_mat_release(drawing);
            return 123;
        }

        float min_area_center_x = 0.0F;
        float min_area_center_y = 0.0F;
        float min_area_width = 0.0F;
        float min_area_height = 0.0F;
        float min_area_angle = 0.0F;
        if (jyppx_ocv_imgproc_min_area_rect(contour_area_points, 4, &min_area_center_x, &min_area_center_y, &min_area_width, &min_area_height, &min_area_angle) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(drawing);
            return 124;
        }

        if (min_area_center_x < 1.999F || min_area_center_x > 2.001F ||
            min_area_center_y < 1.499F || min_area_center_y > 1.501F ||
            min_area_width <= 0.0F || min_area_height <= 0.0F)
        {
            jyppx_ocv_mat_release(drawing);
            return 125;
        }

        float box_points_xy[8] = {};
        if (jyppx_ocv_imgproc_box_points(min_area_center_x, min_area_center_y, min_area_width, min_area_height, min_area_angle, box_points_xy, 4) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(drawing);
            return 126;
        }

        if (box_points_xy[0] == box_points_xy[2] && box_points_xy[1] == box_points_xy[3])
        {
            jyppx_ocv_mat_release(drawing);
            return 127;
        }

        const int ellipse_fit_points[] = { 0, 2, 1, 0, 3, 0, 4, 2, 3, 4, 1, 4 };
        float ellipse_center_x = 0.0F;
        float ellipse_center_y = 0.0F;
        float ellipse_width = 0.0F;
        float ellipse_height = 0.0F;
        float ellipse_angle = 0.0F;
        if (jyppx_ocv_imgproc_fit_ellipse(ellipse_fit_points, 6, &ellipse_center_x, &ellipse_center_y, &ellipse_width, &ellipse_height, &ellipse_angle) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(drawing);
            return 128;
        }

        if (ellipse_center_x < 1.5F || ellipse_center_x > 2.5F ||
            ellipse_center_y < 1.5F || ellipse_center_y > 2.5F ||
            ellipse_width <= 0.0F || ellipse_height <= 0.0F)
        {
            jyppx_ocv_mat_release(drawing);
            return 129;
        }

        if (jyppx_ocv_imgproc_fit_ellipse_ams(ellipse_fit_points, 6, &ellipse_center_x, &ellipse_center_y, &ellipse_width, &ellipse_height, &ellipse_angle) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(drawing);
            return 130;
        }

        if (ellipse_width <= 0.0F || ellipse_height <= 0.0F)
        {
            jyppx_ocv_mat_release(drawing);
            return 131;
        }

        if (jyppx_ocv_imgproc_fit_ellipse_direct(ellipse_fit_points, 6, &ellipse_center_x, &ellipse_center_y, &ellipse_width, &ellipse_height, &ellipse_angle) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(drawing);
            return 132;
        }

        if (ellipse_width <= 0.0F || ellipse_height <= 0.0F)
        {
            jyppx_ocv_mat_release(drawing);
            return 133;
        }

        int rr_intersection_type = 0;
        int rr_intersection_count = 0;
        if (jyppx_ocv_imgproc_rotated_rectangle_intersection_count(
            0.0F, 0.0F, 4.0F, 4.0F, 0.0F,
            1.0F, 0.0F, 4.0F, 4.0F, 0.0F,
            &rr_intersection_type,
            &rr_intersection_count) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(drawing);
            return 134;
        }

        if (rr_intersection_type != 1 || rr_intersection_count <= 0)
        {
            jyppx_ocv_mat_release(drawing);
            return 135;
        }

        float rr_intersection_points_xy[16] = {};
        int rr_intersection_written_count = 0;
        if (jyppx_ocv_imgproc_rotated_rectangle_intersection_fill(
            0.0F, 0.0F, 4.0F, 4.0F, 0.0F,
            1.0F, 0.0F, 4.0F, 4.0F, 0.0F,
            rr_intersection_points_xy,
            8,
            &rr_intersection_type,
            &rr_intersection_written_count) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(drawing);
            return 136;
        }

        if (rr_intersection_written_count != rr_intersection_count)
        {
            jyppx_ocv_mat_release(drawing);
            return 137;
        }

        float closest_ellipse_points_xy[12] = {};
        if (jyppx_ocv_imgproc_get_closest_ellipse_points(
            ellipse_center_x,
            ellipse_center_y,
            ellipse_width,
            ellipse_height,
            ellipse_angle,
            ellipse_fit_points,
            6,
            closest_ellipse_points_xy,
            6) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(drawing);
            return 138;
        }

        if (closest_ellipse_points_xy[0] == 0.0F && closest_ellipse_points_xy[1] == 0.0F)
        {
            jyppx_ocv_mat_release(drawing);
            return 139;
        }

        float enclosing_triangle_points_xy[6] = {};
        double enclosing_triangle_area = 0.0;
        if (jyppx_ocv_imgproc_min_enclosing_triangle(contour_area_points, 4, enclosing_triangle_points_xy, 3, &enclosing_triangle_area) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(drawing);
            return 140;
        }

        if (enclosing_triangle_area <= 0.0)
        {
            jyppx_ocv_mat_release(drawing);
            return 141;
        }

        float line_vx = 0.0F;
        float line_vy = 0.0F;
        float line_x0 = 0.0F;
        float line_y0 = 0.0F;
        const int fit_line_points[] = { 0, 1, 2, 5, 4, 9 };
        if (jyppx_ocv_imgproc_fit_line_2d(fit_line_points, 3, 2, 0.0, 0.01, 0.01, &line_vx, &line_vy, &line_x0, &line_y0) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(drawing);
            return 142;
        }

        if (line_vx <= 0.0F || line_vy <= 0.0F || line_y0 <= line_x0)
        {
            jyppx_ocv_mat_release(drawing);
            return 143;
        }

        int approx_poly_n_count = 0;
        if (jyppx_ocv_imgproc_approx_poly_n_count(approx_poly_curve, 5, 4, -1.0F, 1, &approx_poly_n_count) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(drawing);
            return 144;
        }

        if (approx_poly_n_count != 4)
        {
            jyppx_ocv_mat_release(drawing);
            return 145;
        }

        float approx_poly_n_points_xy[8] = {};
        int approx_poly_n_written_count = 0;
        if (jyppx_ocv_imgproc_approx_poly_n_fill(approx_poly_curve, 5, 4, -1.0F, 1, approx_poly_n_points_xy, 4, &approx_poly_n_written_count) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(drawing);
            return 146;
        }

        if (approx_poly_n_written_count != approx_poly_n_count)
        {
            jyppx_ocv_mat_release(drawing);
            return 147;
        }

        int hull_index_count = 0;
        if (jyppx_ocv_imgproc_convex_hull_indices_count(approx_poly_curve, 5, 0, &hull_index_count) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(drawing);
            return 148;
        }

        if (hull_index_count != 4)
        {
            jyppx_ocv_mat_release(drawing);
            return 149;
        }

        int hull_indices[8] = {};
        int hull_index_written_count = 0;
        if (jyppx_ocv_imgproc_convex_hull_indices_fill(approx_poly_curve, 5, 0, hull_indices, 8, &hull_index_written_count) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(drawing);
            return 150;
        }

        if (hull_index_written_count != hull_index_count)
        {
            jyppx_ocv_mat_release(drawing);
            return 151;
        }

        const int concave_points[] = { 0, 0, 4, 0, 4, 4, 2, 2, 0, 4 };
        int concave_hull_indices[8] = {};
        int concave_hull_index_count = 0;
        if (jyppx_ocv_imgproc_convex_hull_indices_fill(concave_points, 5, 0, concave_hull_indices, 8, &concave_hull_index_count) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(drawing);
            return 152;
        }

        int defect_count = 0;
        if (jyppx_ocv_imgproc_convexity_defects_count(concave_points, 5, concave_hull_indices, concave_hull_index_count, &defect_count) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(drawing);
            return 153;
        }

        if (defect_count <= 0)
        {
            jyppx_ocv_mat_release(drawing);
            return 154;
        }

        int defects[16] = {};
        int defect_written_count = 0;
        if (jyppx_ocv_imgproc_convexity_defects_fill(concave_points, 5, concave_hull_indices, concave_hull_index_count, defects, 4, &defect_written_count) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(drawing);
            return 155;
        }

        if (defect_written_count != defect_count || defects[3] <= 0)
        {
            jyppx_ocv_mat_release(drawing);
            return 156;
        }

        float enclosing_polygon_points_xy[8] = {};
        int enclosing_polygon_point_count = 0;
        double enclosing_polygon_area = 0.0;
        if (jyppx_ocv_imgproc_min_enclosing_convex_polygon(contour_area_points, 4, 4, enclosing_polygon_points_xy, 4, &enclosing_polygon_point_count, &enclosing_polygon_area) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(drawing);
            return 157;
        }

        if (enclosing_polygon_point_count != 4 || enclosing_polygon_area <= 0.0)
        {
            jyppx_ocv_mat_release(drawing);
            return 158;
        }

        enclosing_polygon_point_count = 0;
        enclosing_polygon_area = 0.0;
        if (jyppx_ocv_imgproc_min_enclosing_convex_polygon(approx_poly_curve, 5, 4, enclosing_polygon_points_xy, 4, &enclosing_polygon_point_count, &enclosing_polygon_area) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(drawing);
            return 344;
        }

        if (enclosing_polygon_point_count != 4 || enclosing_polygon_area <= 0.0)
        {
            jyppx_ocv_mat_release(drawing);
            return 345;
        }

        const int intersection_polygon1[] = { 0, 0, 4, 0, 4, 4, 0, 4 };
        const int intersection_polygon2[] = { 2, 0, 6, 0, 6, 4, 2, 4 };
        float intersection_area = 0.0F;
        int intersection_point_count = 0;
        if (jyppx_ocv_imgproc_intersect_convex_convex_count(intersection_polygon1, 4, intersection_polygon2, 4, 1, &intersection_area, &intersection_point_count) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(drawing);
            return 159;
        }

        if (intersection_area <= 7.999F || intersection_area >= 8.001F || intersection_point_count <= 0)
        {
            jyppx_ocv_mat_release(drawing);
            return 160;
        }

        float intersection_points_xy[16] = {};
        int intersection_written_count = 0;
        if (jyppx_ocv_imgproc_intersect_convex_convex_fill(intersection_polygon1, 4, intersection_polygon2, 4, 1, intersection_points_xy, 8, &intersection_area, &intersection_written_count) != OPENCV_CSHARP_STATUS_OK)
        {
            jyppx_ocv_mat_release(drawing);
            return 161;
        }

        if (intersection_written_count != intersection_point_count)
        {
            jyppx_ocv_mat_release(drawing);
            return 162;
        }

        jyppx_ocv_mat_release(drawing);
        return 0;
    }

    if (status == OPENCV_CSHARP_STATUS_NOT_LINKED)
    {
#if !defined(OPENCV_CSHARP_RUNTIME_PROFILE_MINI)
        jyppx_ocv_ml_model* ann = nullptr;
        if (jyppx_ocv_ml_ann_mlp_create(&ann) != OPENCV_CSHARP_STATUS_NOT_LINKED || ann != nullptr)
        {
            jyppx_ocv_ml_model_release_handle(ann);
            return 812;
        }
        jyppx_ocv_ml_model* dtrees = nullptr;
        jyppx_ocv_ml_model* rtrees = nullptr;
        jyppx_ocv_ml_model* boost = nullptr;
        if (jyppx_ocv_ml_dtrees_create(&dtrees) != OPENCV_CSHARP_STATUS_NOT_LINKED || dtrees != nullptr ||
            jyppx_ocv_ml_rtrees_create(&rtrees) != OPENCV_CSHARP_STATUS_NOT_LINKED || rtrees != nullptr ||
            jyppx_ocv_ml_boost_create(&boost) != OPENCV_CSHARP_STATUS_NOT_LINKED || boost != nullptr)
        {
            jyppx_ocv_ml_model_release_handle(dtrees);
            jyppx_ocv_ml_model_release_handle(rtrees);
            jyppx_ocv_ml_model_release_handle(boost);
            return 832;
        }
        jyppx_ocv_ml_model* em = nullptr;
        if (jyppx_ocv_ml_em_create(&em) != OPENCV_CSHARP_STATUS_NOT_LINKED || em != nullptr)
        {
            jyppx_ocv_ml_model_release_handle(em);
            return 848;
        }
        jyppx_ocv_ml_model* logistic = nullptr;
        jyppx_ocv_ml_model* svmsgd = nullptr;
        if (jyppx_ocv_ml_logistic_regression_create(&logistic) != OPENCV_CSHARP_STATUS_NOT_LINKED || logistic != nullptr ||
            jyppx_ocv_ml_svmsgd_create(&svmsgd) != OPENCV_CSHARP_STATUS_NOT_LINKED || svmsgd != nullptr)
        {
            jyppx_ocv_ml_model_release_handle(logistic);
            jyppx_ocv_ml_model_release_handle(svmsgd);
            return 870;
        }
        jyppx_ocv_tracking_legacy_tracker_tld* tld = nullptr;
        jyppx_ocv_tracking_legacy_tracker_kcf* kcf = nullptr;
        if (jyppx_ocv_tracking_legacy_tracker_tld_create(&tld) != OPENCV_CSHARP_STATUS_NOT_LINKED || tld != nullptr ||
            jyppx_ocv_tracking_legacy_tracker_kcf_create_default(&kcf) != OPENCV_CSHARP_STATUS_NOT_LINKED || kcf != nullptr)
        {
            jyppx_ocv_tracking_legacy_tracker_release_handle(reinterpret_cast<jyppx_ocv_tracking_legacy_tracker*>(tld));
            jyppx_ocv_tracking_legacy_tracker_release_handle(reinterpret_cast<jyppx_ocv_tracking_legacy_tracker*>(kcf));
            return 880;
        }
#endif
        const char* error = jyppx_ocv_get_last_error();
        return error != nullptr && std::strlen(error) > 0 ? 0 : 12;
    }

    return 13;
}


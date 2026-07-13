#pragma once

#include "open_cv_sharp/core/mat.h"
#include "open_cv_sharp/export.h"
#include "open_cv_sharp/status.h"

typedef struct jyppx_ocv_face_recognizer jyppx_ocv_face_recognizer;
typedef struct jyppx_ocv_face_basic_recognizer jyppx_ocv_face_basic_recognizer;
typedef struct jyppx_ocv_face_eigen_recognizer jyppx_ocv_face_eigen_recognizer;
typedef struct jyppx_ocv_face_fisher_recognizer jyppx_ocv_face_fisher_recognizer;
typedef struct jyppx_ocv_face_lbph_recognizer jyppx_ocv_face_lbph_recognizer;
typedef struct jyppx_ocv_face_standard_collector jyppx_ocv_face_standard_collector;
typedef struct jyppx_ocv_face_bif jyppx_ocv_face_bif;
typedef struct jyppx_ocv_face_facemark jyppx_ocv_face_facemark;
typedef struct jyppx_ocv_face_facemark_train jyppx_ocv_face_facemark_train;
typedef struct jyppx_ocv_face_facemark_lbf jyppx_ocv_face_facemark_lbf;
typedef struct jyppx_ocv_face_mace jyppx_ocv_face_mace;

typedef struct jyppx_ocv_face_prediction_result
{
    int label;
    double distance;
} jyppx_ocv_face_prediction_result;

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_face_recognizer_release_handle(
    jyppx_ocv_face_recognizer* recognizer);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_face_standard_collector_release_handle(
    jyppx_ocv_face_standard_collector* collector);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_face_bif_release_handle(
    jyppx_ocv_face_bif* bif);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_eigen_create(
    int num_components,
    double threshold,
    jyppx_ocv_face_eigen_recognizer** recognizer);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_fisher_create(
    int num_components,
    double threshold,
    jyppx_ocv_face_fisher_recognizer** recognizer);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_lbph_create(
    int radius,
    int neighbors,
    int grid_x,
    int grid_y,
    double threshold,
    jyppx_ocv_face_lbph_recognizer** recognizer);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_recognizer_train(
    jyppx_ocv_face_recognizer* recognizer,
    const jyppx_ocv_mat* const* images,
    int image_count,
    const int* labels,
    int label_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_recognizer_update(
    jyppx_ocv_face_recognizer* recognizer,
    const jyppx_ocv_mat* const* images,
    int image_count,
    const int* labels,
    int label_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_recognizer_predict_label(
    const jyppx_ocv_face_recognizer* recognizer,
    const jyppx_ocv_mat* image,
    int* label);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_recognizer_predict(
    const jyppx_ocv_face_recognizer* recognizer,
    const jyppx_ocv_mat* image,
    int* label,
    double* confidence);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_recognizer_predict_collect(
    const jyppx_ocv_face_recognizer* recognizer,
    const jyppx_ocv_mat* image,
    jyppx_ocv_face_standard_collector* collector);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_recognizer_read(
    jyppx_ocv_face_recognizer* recognizer,
    const char* path);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_recognizer_write(
    const jyppx_ocv_face_recognizer* recognizer,
    const char* path);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_recognizer_empty(
    const jyppx_ocv_face_recognizer* recognizer,
    int* empty);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_recognizer_set_label_info(
    jyppx_ocv_face_recognizer* recognizer,
    int label,
    const char* info);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_recognizer_get_label_info_length(
    const jyppx_ocv_face_recognizer* recognizer,
    int label,
    int* length);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_recognizer_get_label_info_fill(
    const jyppx_ocv_face_recognizer* recognizer,
    int label,
    char* buffer,
    int buffer_capacity,
    int* written);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_recognizer_get_labels_by_string_count(
    const jyppx_ocv_face_recognizer* recognizer,
    const char* substring,
    int* count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_recognizer_get_labels_by_string_fill(
    const jyppx_ocv_face_recognizer* recognizer,
    const char* substring,
    int* labels,
    int label_capacity,
    int* count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_recognizer_get_threshold(
    const jyppx_ocv_face_recognizer* recognizer,
    double* threshold);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_recognizer_set_threshold(
    jyppx_ocv_face_recognizer* recognizer,
    double threshold);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_basic_get_num_components(
    const jyppx_ocv_face_basic_recognizer* recognizer,
    int* num_components);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_basic_set_num_components(
    jyppx_ocv_face_basic_recognizer* recognizer,
    int num_components);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_basic_get_labels(
    const jyppx_ocv_face_basic_recognizer* recognizer,
    jyppx_ocv_mat** labels);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_basic_get_eigen_values(
    const jyppx_ocv_face_basic_recognizer* recognizer,
    jyppx_ocv_mat** eigen_values);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_basic_get_eigen_vectors(
    const jyppx_ocv_face_basic_recognizer* recognizer,
    jyppx_ocv_mat** eigen_vectors);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_basic_get_mean(
    const jyppx_ocv_face_basic_recognizer* recognizer,
    jyppx_ocv_mat** mean);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_basic_get_projections_count(
    const jyppx_ocv_face_basic_recognizer* recognizer,
    int* count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_basic_get_projections_fill(
    const jyppx_ocv_face_basic_recognizer* recognizer,
    jyppx_ocv_mat** projections,
    int projection_capacity,
    int* count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_lbph_get_radius(
    const jyppx_ocv_face_lbph_recognizer* recognizer,
    int* radius);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_lbph_set_radius(
    jyppx_ocv_face_lbph_recognizer* recognizer,
    int radius);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_lbph_get_neighbors(
    const jyppx_ocv_face_lbph_recognizer* recognizer,
    int* neighbors);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_lbph_set_neighbors(
    jyppx_ocv_face_lbph_recognizer* recognizer,
    int neighbors);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_lbph_get_grid_x(
    const jyppx_ocv_face_lbph_recognizer* recognizer,
    int* grid_x);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_lbph_set_grid_x(
    jyppx_ocv_face_lbph_recognizer* recognizer,
    int grid_x);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_lbph_get_grid_y(
    const jyppx_ocv_face_lbph_recognizer* recognizer,
    int* grid_y);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_lbph_set_grid_y(
    jyppx_ocv_face_lbph_recognizer* recognizer,
    int grid_y);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_lbph_get_labels(
    const jyppx_ocv_face_lbph_recognizer* recognizer,
    jyppx_ocv_mat** labels);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_lbph_get_histograms_count(
    const jyppx_ocv_face_lbph_recognizer* recognizer,
    int* count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_lbph_get_histograms_fill(
    const jyppx_ocv_face_lbph_recognizer* recognizer,
    jyppx_ocv_mat** histograms,
    int histogram_capacity,
    int* count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_standard_collector_create(
    double threshold,
    jyppx_ocv_face_standard_collector** collector);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_standard_collector_get_min_label(
    const jyppx_ocv_face_standard_collector* collector,
    int* label);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_standard_collector_get_min_dist(
    const jyppx_ocv_face_standard_collector* collector,
    double* distance);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_standard_collector_get_results_count(
    const jyppx_ocv_face_standard_collector* collector,
    int sorted,
    int* count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_standard_collector_get_results_fill(
    const jyppx_ocv_face_standard_collector* collector,
    int sorted,
    jyppx_ocv_face_prediction_result* results,
    int result_capacity,
    int* count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_bif_create(
    int num_bands,
    int num_rotations,
    jyppx_ocv_face_bif** bif);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_bif_get_num_bands(
    const jyppx_ocv_face_bif* bif,
    int* num_bands);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_bif_get_num_rotations(
    const jyppx_ocv_face_bif* bif,
    int* num_rotations);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_bif_compute(
    const jyppx_ocv_face_bif* bif,
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* features);

/* ====================================================================================
 *  Face round 2: Facemark base / FacemarkLBF / MACE
 *  All handles are opaque; cv::Ptr / STL / InputArray / OutputArray never leak
 *  across the ABI boundary.
 * ==================================================================================== */

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_face_facemark_release_handle(
    jyppx_ocv_face_facemark* facemark);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_face_mace_release_handle(
    jyppx_ocv_face_mace* mace);

/* Facemark base: load a trained model file and fit landmarks to detected faces. */
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_facemark_load_model(
    jyppx_ocv_face_facemark* facemark,
    const char* model_path);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_facemark_fit(
    jyppx_ocv_face_facemark* facemark,
    const jyppx_ocv_mat* image,
    const int* faces,
    int face_count,
    int* result);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_facemark_fit_landmarks_count(
    const jyppx_ocv_face_facemark* facemark,
    int* face_count,
    int* point_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_facemark_fit_landmarks_fill(
    const jyppx_ocv_face_facemark* facemark,
    int* landmark_offsets,
    int landmark_offset_capacity,
    float* landmarks_buffer,
    int landmark_point_capacity,
    int* face_count,
    int* point_count);

/* FacemarkTrain surface: add training sample, get faces from default detector. */
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_facemark_train_add_sample(
    jyppx_ocv_face_facemark_train* facemark,
    const jyppx_ocv_mat* image,
    const float* landmarks,
    int landmark_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_facemark_train_training(
    jyppx_ocv_face_facemark_train* facemark);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_facemark_train_get_faces_count(
    jyppx_ocv_face_facemark_train* facemark,
    const jyppx_ocv_mat* image,
    int* count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_facemark_train_get_faces_fill(
    jyppx_ocv_face_facemark_train* facemark,
    const jyppx_ocv_mat* image,
    int* faces_buffer,
    int face_capacity,
    int* count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_facemark_save(
    const jyppx_ocv_face_facemark* facemark,
    const char* path);

/* FacemarkLBF concrete factory. */
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_facemark_lbf_create(
    int n_landmarks,
    int init_shape_n,
    int stages_n,
    int tree_n,
    int tree_depth,
    double shape_offset,
    double bagging_overlap,
    int verbose,
    jyppx_ocv_face_facemark_lbf** facemark);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_facemark_lbf_create_ex(
    int n_landmarks,
    int init_shape_n,
    int stages_n,
    int tree_n,
    int tree_depth,
    double shape_offset,
    double bagging_overlap,
    int verbose,
    int save_model,
    unsigned int seed,
    const char* cascade_face,
    const char* model_filename,
    const int* feats_m,
    int feats_count,
    const double* radius_m,
    int radius_count,
    const int* left_pupil,
    int left_pupil_count,
    const int* right_pupil,
    int right_pupil_count,
    int detect_roi_x,
    int detect_roi_y,
    int detect_roi_width,
    int detect_roi_height,
    jyppx_ocv_face_facemark_lbf** facemark);

/* MACE: cancellable biometrical correlation filter authentication. */
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_mace_create(
    int imgsize,
    jyppx_ocv_face_mace** mace);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_mace_load(
    const char* filename,
    const char* objname,
    jyppx_ocv_face_mace** mace);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_mace_salt(
    jyppx_ocv_face_mace* mace,
    const char* passphrase);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_mace_train(
    jyppx_ocv_face_mace* mace,
    const jyppx_ocv_mat* const* images,
    int image_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_mace_same(
    const jyppx_ocv_face_mace* mace,
    const jyppx_ocv_mat* query,
    int* same);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_mace_save(
    const jyppx_ocv_face_mace* mace,
    const char* path);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_mace_empty(
    const jyppx_ocv_face_mace* mace,
    int* empty);

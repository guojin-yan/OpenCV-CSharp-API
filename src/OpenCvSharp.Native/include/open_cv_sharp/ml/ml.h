#pragma once

#include "open_cv_sharp/core/mat.h"
#include "open_cv_sharp/export.h"
#include "open_cv_sharp/status.h"

typedef struct jyppx_ocv_ml_train_data jyppx_ocv_ml_train_data;
typedef struct jyppx_ocv_ml_param_grid jyppx_ocv_ml_param_grid;
typedef struct jyppx_ocv_ml_model jyppx_ocv_ml_model;

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_param_grid_create(
    double min_val,
    double max_val,
    double log_step,
    jyppx_ocv_ml_param_grid** grid);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_ml_param_grid_release_handle(
    jyppx_ocv_ml_param_grid* grid);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_param_grid_get(
    const jyppx_ocv_ml_param_grid* grid,
    double* min_val,
    double* max_val,
    double* log_step);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_param_grid_set(
    jyppx_ocv_ml_param_grid* grid,
    double min_val,
    double max_val,
    double log_step);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_svm_get_default_grid(
    int param_id,
    jyppx_ocv_ml_param_grid** grid);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_train_data_create(
    const jyppx_ocv_mat* samples,
    int layout,
    const jyppx_ocv_mat* responses,
    const jyppx_ocv_mat* var_idx,
    const jyppx_ocv_mat* sample_idx,
    const jyppx_ocv_mat* sample_weights,
    const jyppx_ocv_mat* var_type,
    jyppx_ocv_ml_train_data** train_data);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_train_data_load_csv(
    const char* filename,
    int header_line_count,
    int response_start_idx,
    int response_end_idx,
    const char* var_type_spec,
    int delimiter,
    int missch,
    jyppx_ocv_ml_train_data** train_data);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_ml_train_data_release_handle(
    jyppx_ocv_ml_train_data* train_data);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_train_data_get_int(
    const jyppx_ocv_ml_train_data* train_data,
    int property_id,
    int argument,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_train_data_get_mat(
    const jyppx_ocv_ml_train_data* train_data,
    int property_id,
    int layout,
    int compress_samples,
    int compress_vars,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_train_data_set_train_test_split(
    jyppx_ocv_ml_train_data* train_data,
    int count,
    int shuffle);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_train_data_set_train_test_split_ratio(
    jyppx_ocv_ml_train_data* train_data,
    double ratio,
    int shuffle);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_train_data_shuffle_train_test(
    jyppx_ocv_ml_train_data* train_data);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_train_data_get_names_count(
    const jyppx_ocv_ml_train_data* train_data,
    int* string_count,
    int* byte_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_train_data_get_names_fill(
    const jyppx_ocv_ml_train_data* train_data,
    int* offsets,
    int offset_capacity,
    char* buffer,
    int buffer_capacity,
    int* string_count,
    int* byte_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_train_data_get_sub_vector(
    const jyppx_ocv_mat* vec,
    const jyppx_ocv_mat* idx,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_train_data_get_sub_matrix(
    const jyppx_ocv_mat* matrix,
    const jyppx_ocv_mat* idx,
    int layout,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_knearest_create(
    jyppx_ocv_ml_model** model);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_knearest_load(
    const char* filepath,
    jyppx_ocv_ml_model** model);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_svm_create(
    jyppx_ocv_ml_model** model);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_svm_load(
    const char* filepath,
    jyppx_ocv_ml_model** model);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_normal_bayes_classifier_create(
    jyppx_ocv_ml_model** model);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_normal_bayes_classifier_load(
    const char* filepath,
    const char* node_name,
    jyppx_ocv_ml_model** model);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_ann_mlp_create(
    jyppx_ocv_ml_model** model);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_ann_mlp_load(
    const char* filepath,
    jyppx_ocv_ml_model** model);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_dtrees_create(
    jyppx_ocv_ml_model** model);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_dtrees_load(
    const char* filepath,
    const char* node_name,
    jyppx_ocv_ml_model** model);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_rtrees_create(
    jyppx_ocv_ml_model** model);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_rtrees_load(
    const char* filepath,
    const char* node_name,
    jyppx_ocv_ml_model** model);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_boost_create(
    jyppx_ocv_ml_model** model);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_boost_load(
    const char* filepath,
    const char* node_name,
    jyppx_ocv_ml_model** model);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_em_create(
    jyppx_ocv_ml_model** model);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_em_load(
    const char* filepath,
    const char* node_name,
    jyppx_ocv_ml_model** model);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_ml_model_release_handle(
    jyppx_ocv_ml_model* model);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_stat_model_get_int(
    const jyppx_ocv_ml_model* model,
    int property_id,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_stat_model_train_data(
    jyppx_ocv_ml_model* model,
    const jyppx_ocv_ml_train_data* train_data,
    int flags,
    int* result);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_stat_model_train_samples(
    jyppx_ocv_ml_model* model,
    const jyppx_ocv_mat* samples,
    int layout,
    const jyppx_ocv_mat* responses,
    int* result);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_stat_model_predict(
    const jyppx_ocv_ml_model* model,
    const jyppx_ocv_mat* samples,
    jyppx_ocv_mat* results,
    int flags,
    float* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_stat_model_calc_error(
    const jyppx_ocv_ml_model* model,
    const jyppx_ocv_ml_train_data* data,
    int test,
    jyppx_ocv_mat* responses,
    float* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_stat_model_save(
    const jyppx_ocv_ml_model* model,
    const char* filepath);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_stat_model_clear(
    jyppx_ocv_ml_model* model);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_knearest_get_int(
    const jyppx_ocv_ml_model* model,
    int property_id,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_knearest_set_int(
    jyppx_ocv_ml_model* model,
    int property_id,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_knearest_find_nearest(
    const jyppx_ocv_ml_model* model,
    const jyppx_ocv_mat* samples,
    int k,
    jyppx_ocv_mat* results,
    jyppx_ocv_mat* neighbor_responses,
    jyppx_ocv_mat* dist,
    float* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_svm_get_int(
    const jyppx_ocv_ml_model* model,
    int property_id,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_svm_set_int(
    jyppx_ocv_ml_model* model,
    int property_id,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_svm_get_double(
    const jyppx_ocv_ml_model* model,
    int property_id,
    double* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_svm_set_double(
    jyppx_ocv_ml_model* model,
    int property_id,
    double value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_svm_get_term_criteria(
    const jyppx_ocv_ml_model* model,
    int* type,
    int* max_count,
    double* epsilon);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_svm_set_term_criteria(
    jyppx_ocv_ml_model* model,
    int type,
    int max_count,
    double epsilon);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_svm_get_class_weights(
    const jyppx_ocv_ml_model* model,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_svm_set_class_weights(
    jyppx_ocv_ml_model* model,
    const jyppx_ocv_mat* weights);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_svm_train_auto(
    jyppx_ocv_ml_model* model,
    const jyppx_ocv_mat* samples,
    int layout,
    const jyppx_ocv_mat* responses,
    int k_fold,
    const jyppx_ocv_ml_param_grid* c_grid,
    const jyppx_ocv_ml_param_grid* gamma_grid,
    const jyppx_ocv_ml_param_grid* p_grid,
    const jyppx_ocv_ml_param_grid* nu_grid,
    const jyppx_ocv_ml_param_grid* coeff_grid,
    const jyppx_ocv_ml_param_grid* degree_grid,
    int balanced,
    int* result);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_svm_get_support_vectors(
    const jyppx_ocv_ml_model* model,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_svm_get_uncompressed_support_vectors(
    const jyppx_ocv_ml_model* model,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_svm_get_decision_function(
    const jyppx_ocv_ml_model* model,
    int index,
    jyppx_ocv_mat* alpha,
    jyppx_ocv_mat* svidx,
    double* rho);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_normal_bayes_classifier_predict_prob(
    const jyppx_ocv_ml_model* model,
    const jyppx_ocv_mat* inputs,
    jyppx_ocv_mat* outputs,
    jyppx_ocv_mat* output_probs,
    int flags,
    float* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_ann_mlp_get_int(
    const jyppx_ocv_ml_model* model,
    int property_id,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_ann_mlp_set_int(
    jyppx_ocv_ml_model* model,
    int property_id,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_ann_mlp_get_double(
    const jyppx_ocv_ml_model* model,
    int property_id,
    double* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_ann_mlp_set_double(
    jyppx_ocv_ml_model* model,
    int property_id,
    double value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_ann_mlp_set_train_method(
    jyppx_ocv_ml_model* model,
    int method,
    double param1,
    double param2);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_ann_mlp_set_activation_function(
    jyppx_ocv_ml_model* model,
    int type,
    double param1,
    double param2);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_ann_mlp_get_layer_sizes(
    const jyppx_ocv_ml_model* model,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_ann_mlp_set_layer_sizes(
    jyppx_ocv_ml_model* model,
    const jyppx_ocv_mat* layer_sizes);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_ann_mlp_get_term_criteria(
    const jyppx_ocv_ml_model* model,
    int* type,
    int* max_count,
    double* epsilon);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_ann_mlp_set_term_criteria(
    jyppx_ocv_ml_model* model,
    int type,
    int max_count,
    double epsilon);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_ann_mlp_get_weights(
    const jyppx_ocv_ml_model* model,
    int layer_index,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_ann_mlp_set_anneal_energy_seed(
    jyppx_ocv_ml_model* model,
    unsigned long long seed);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_dtrees_get_int(
    const jyppx_ocv_ml_model* model,
    int property_id,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_dtrees_set_int(
    jyppx_ocv_ml_model* model,
    int property_id,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_dtrees_get_regression_accuracy(
    const jyppx_ocv_ml_model* model,
    float* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_dtrees_set_regression_accuracy(
    jyppx_ocv_ml_model* model,
    float value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_dtrees_get_priors(
    const jyppx_ocv_ml_model* model,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_dtrees_set_priors(
    jyppx_ocv_ml_model* model,
    const jyppx_ocv_mat* priors);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_rtrees_get_int(
    const jyppx_ocv_ml_model* model,
    int property_id,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_rtrees_set_int(
    jyppx_ocv_ml_model* model,
    int property_id,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_rtrees_get_term_criteria(
    const jyppx_ocv_ml_model* model,
    int* type,
    int* max_count,
    double* epsilon);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_rtrees_set_term_criteria(
    jyppx_ocv_ml_model* model,
    int type,
    int max_count,
    double epsilon);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_rtrees_get_var_importance(
    const jyppx_ocv_ml_model* model,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_rtrees_get_votes(
    const jyppx_ocv_ml_model* model,
    const jyppx_ocv_mat* samples,
    jyppx_ocv_mat* results,
    int flags);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_rtrees_get_oob_error(
    const jyppx_ocv_ml_model* model,
    double* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_boost_get_int(
    const jyppx_ocv_ml_model* model,
    int property_id,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_boost_set_int(
    jyppx_ocv_ml_model* model,
    int property_id,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_boost_get_weight_trim_rate(
    const jyppx_ocv_ml_model* model,
    double* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_boost_set_weight_trim_rate(
    jyppx_ocv_ml_model* model,
    double value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_em_get_int(
    const jyppx_ocv_ml_model* model,
    int property_id,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_em_set_int(
    jyppx_ocv_ml_model* model,
    int property_id,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_em_get_term_criteria(
    const jyppx_ocv_ml_model* model,
    int* type,
    int* max_count,
    double* epsilon);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_em_set_term_criteria(
    jyppx_ocv_ml_model* model,
    int type,
    int max_count,
    double epsilon);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_em_get_weights(
    const jyppx_ocv_ml_model* model,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_em_get_means(
    const jyppx_ocv_ml_model* model,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_em_get_covariances_count(
    const jyppx_ocv_ml_model* model,
    int* count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_em_get_covariances_fill(
    const jyppx_ocv_ml_model* model,
    jyppx_ocv_mat* const* covariances,
    int covariance_capacity,
    int* count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_em_predict2(
    const jyppx_ocv_ml_model* model,
    const jyppx_ocv_mat* sample,
    jyppx_ocv_mat* probabilities,
    double* log_likelihood,
    int* label);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_em_train_em(
    jyppx_ocv_ml_model* model,
    const jyppx_ocv_mat* samples,
    jyppx_ocv_mat* log_likelihoods,
    jyppx_ocv_mat* labels,
    jyppx_ocv_mat* probabilities,
    int* result);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_em_train_e(
    jyppx_ocv_ml_model* model,
    const jyppx_ocv_mat* samples,
    const jyppx_ocv_mat* initial_means,
    const jyppx_ocv_mat* const* initial_covariances,
    int initial_covariance_count,
    const jyppx_ocv_mat* initial_weights,
    jyppx_ocv_mat* log_likelihoods,
    jyppx_ocv_mat* labels,
    jyppx_ocv_mat* probabilities,
    int* result);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_em_train_m(
    jyppx_ocv_ml_model* model,
    const jyppx_ocv_mat* samples,
    const jyppx_ocv_mat* initial_probabilities,
    jyppx_ocv_mat* log_likelihoods,
    jyppx_ocv_mat* labels,
    jyppx_ocv_mat* probabilities,
    int* result);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_train_data_get_sample_count(
    const jyppx_ocv_ml_train_data* train_data,
    const jyppx_ocv_mat* variable_indices,
    int* count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_train_data_get_sample_fill(
    const jyppx_ocv_ml_train_data* train_data,
    const jyppx_ocv_mat* variable_indices,
    int sample_index,
    float* values,
    int value_capacity,
    int* count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_train_data_get_values_count(
    const jyppx_ocv_ml_train_data* train_data,
    const jyppx_ocv_mat* sample_indices,
    int* count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_train_data_get_values_fill(
    const jyppx_ocv_ml_train_data* train_data,
    int variable_index,
    const jyppx_ocv_mat* sample_indices,
    float* values,
    int value_capacity,
    int* count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_logistic_regression_create(
    jyppx_ocv_ml_model** model);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_logistic_regression_load(
    const char* filepath,
    const char* node_name,
    jyppx_ocv_ml_model** model);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_logistic_regression_get_learning_rate(
    const jyppx_ocv_ml_model* model,
    double* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_logistic_regression_set_learning_rate(
    jyppx_ocv_ml_model* model,
    double value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_logistic_regression_get_int(
    const jyppx_ocv_ml_model* model,
    int property_id,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_logistic_regression_set_int(
    jyppx_ocv_ml_model* model,
    int property_id,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_logistic_regression_get_term_criteria(
    const jyppx_ocv_ml_model* model,
    int* type,
    int* max_count,
    double* epsilon);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_logistic_regression_set_term_criteria(
    jyppx_ocv_ml_model* model,
    int type,
    int max_count,
    double epsilon);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_logistic_regression_get_learnt_thetas(
    const jyppx_ocv_ml_model* model,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_svmsgd_create(
    jyppx_ocv_ml_model** model);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_svmsgd_load(
    const char* filepath,
    const char* node_name,
    jyppx_ocv_ml_model** model);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_svmsgd_get_weights(
    const jyppx_ocv_ml_model* model,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_svmsgd_get_shift(
    const jyppx_ocv_ml_model* model,
    float* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_svmsgd_set_optimal_parameters(
    jyppx_ocv_ml_model* model,
    int svmsgd_type,
    int margin_type);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_svmsgd_get_int(
    const jyppx_ocv_ml_model* model,
    int property_id,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_svmsgd_set_int(
    jyppx_ocv_ml_model* model,
    int property_id,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_svmsgd_get_float(
    const jyppx_ocv_ml_model* model,
    int property_id,
    float* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_svmsgd_set_float(
    jyppx_ocv_ml_model* model,
    int property_id,
    float value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_svmsgd_get_term_criteria(
    const jyppx_ocv_ml_model* model,
    int* type,
    int* max_count,
    double* epsilon);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ml_svmsgd_set_term_criteria(
    jyppx_ocv_ml_model* model,
    int type,
    int max_count,
    double epsilon);

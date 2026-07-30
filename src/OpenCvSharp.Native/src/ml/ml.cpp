#include "open_cv_sharp/ml/ml.h"

#include "../core/mat_handle.h"
#include "../error_state.h"
#include "ml_handles.h"

#include <cstdint>
#include <cstring>
#include <limits>
#include <new>
#include <string>
#include <vector>

namespace
{
    constexpr int MODEL_KIND_KNEAREST = 1;
    constexpr int MODEL_KIND_SVM = 2;
    constexpr int MODEL_KIND_NORMAL_BAYES = 3;
    constexpr int MODEL_KIND_ANN_MLP = 4;
    constexpr int MODEL_KIND_DTREES = 5;
    constexpr int MODEL_KIND_RTREES = 6;
    constexpr int MODEL_KIND_BOOST = 7;
    constexpr int MODEL_KIND_EM = 8;

    constexpr int TRAIN_DATA_INT_LAYOUT = 0;
    constexpr int TRAIN_DATA_INT_N_TRAIN_SAMPLES = 1;
    constexpr int TRAIN_DATA_INT_N_TEST_SAMPLES = 2;
    constexpr int TRAIN_DATA_INT_N_SAMPLES = 3;
    constexpr int TRAIN_DATA_INT_N_VARS = 4;
    constexpr int TRAIN_DATA_INT_N_ALL_VARS = 5;
    constexpr int TRAIN_DATA_INT_RESPONSE_TYPE = 6;
    constexpr int TRAIN_DATA_INT_CAT_COUNT = 7;

    constexpr int TRAIN_DATA_MAT_SAMPLES = 0;
    constexpr int TRAIN_DATA_MAT_MISSING = 1;
    constexpr int TRAIN_DATA_MAT_TRAIN_SAMPLES = 2;
    constexpr int TRAIN_DATA_MAT_TRAIN_RESPONSES = 3;
    constexpr int TRAIN_DATA_MAT_TRAIN_NORM_CAT_RESPONSES = 4;
    constexpr int TRAIN_DATA_MAT_TEST_RESPONSES = 5;
    constexpr int TRAIN_DATA_MAT_TEST_NORM_CAT_RESPONSES = 6;
    constexpr int TRAIN_DATA_MAT_RESPONSES = 7;
    constexpr int TRAIN_DATA_MAT_NORM_CAT_RESPONSES = 8;
    constexpr int TRAIN_DATA_MAT_SAMPLE_WEIGHTS = 9;
    constexpr int TRAIN_DATA_MAT_TRAIN_SAMPLE_WEIGHTS = 10;
    constexpr int TRAIN_DATA_MAT_TEST_SAMPLE_WEIGHTS = 11;
    constexpr int TRAIN_DATA_MAT_VAR_IDX = 12;
    constexpr int TRAIN_DATA_MAT_VAR_TYPE = 13;
    constexpr int TRAIN_DATA_MAT_VAR_SYMBOL_FLAGS = 14;
    constexpr int TRAIN_DATA_MAT_TRAIN_SAMPLE_IDX = 15;
    constexpr int TRAIN_DATA_MAT_TEST_SAMPLE_IDX = 16;
    constexpr int TRAIN_DATA_MAT_DEFAULT_SUBST_VALUES = 17;
    constexpr int TRAIN_DATA_MAT_CLASS_LABELS = 18;
    constexpr int TRAIN_DATA_MAT_CAT_OFS = 19;
    constexpr int TRAIN_DATA_MAT_CAT_MAP = 20;
    constexpr int TRAIN_DATA_MAT_TEST_SAMPLES = 21;

    constexpr int STAT_MODEL_INT_VAR_COUNT = 0;
    constexpr int STAT_MODEL_INT_EMPTY = 1;
    constexpr int STAT_MODEL_INT_IS_TRAINED = 2;
    constexpr int STAT_MODEL_INT_IS_CLASSIFIER = 3;

    constexpr int KNEAREST_INT_DEFAULT_K = 0;
    constexpr int KNEAREST_INT_IS_CLASSIFIER = 1;
    constexpr int KNEAREST_INT_EMAX = 2;
    constexpr int KNEAREST_INT_ALGORITHM_TYPE = 3;

    constexpr int SVM_INT_TYPE = 0;
    constexpr int SVM_INT_KERNEL_TYPE = 1;

    constexpr int SVM_DOUBLE_GAMMA = 0;
    constexpr int SVM_DOUBLE_COEF0 = 1;
    constexpr int SVM_DOUBLE_DEGREE = 2;
    constexpr int SVM_DOUBLE_C = 3;
    constexpr int SVM_DOUBLE_NU = 4;
    constexpr int SVM_DOUBLE_P = 5;

    constexpr int ANN_MLP_INT_TRAIN_METHOD = 0;
    constexpr int ANN_MLP_INT_ANNEAL_ITE_PER_STEP = 1;

    constexpr int ANN_MLP_DOUBLE_BACKPROP_WEIGHT_SCALE = 0;
    constexpr int ANN_MLP_DOUBLE_BACKPROP_MOMENTUM_SCALE = 1;
    constexpr int ANN_MLP_DOUBLE_RPROP_DW0 = 2;
    constexpr int ANN_MLP_DOUBLE_RPROP_DW_PLUS = 3;
    constexpr int ANN_MLP_DOUBLE_RPROP_DW_MINUS = 4;
    constexpr int ANN_MLP_DOUBLE_RPROP_DW_MIN = 5;
    constexpr int ANN_MLP_DOUBLE_RPROP_DW_MAX = 6;
    constexpr int ANN_MLP_DOUBLE_ANNEAL_INITIAL_T = 7;
    constexpr int ANN_MLP_DOUBLE_ANNEAL_FINAL_T = 8;
    constexpr int ANN_MLP_DOUBLE_ANNEAL_COOLING_RATIO = 9;

    constexpr int DTREES_INT_MAX_CATEGORIES = 0;
    constexpr int DTREES_INT_MAX_DEPTH = 1;
    constexpr int DTREES_INT_MIN_SAMPLE_COUNT = 2;
    constexpr int DTREES_INT_CV_FOLDS = 3;
    constexpr int DTREES_INT_USE_SURROGATES = 4;
    constexpr int DTREES_INT_USE_1SE_RULE = 5;
    constexpr int DTREES_INT_TRUNCATE_PRUNED_TREE = 6;

    constexpr int RTREES_INT_CALCULATE_VAR_IMPORTANCE = 0;
    constexpr int RTREES_INT_ACTIVE_VAR_COUNT = 1;

    constexpr int BOOST_INT_TYPE = 0;
    constexpr int BOOST_INT_WEAK_COUNT = 1;

    constexpr int EM_INT_CLUSTERS_NUMBER = 0;
    constexpr int EM_INT_COVARIANCE_MATRIX_TYPE = 1;

    int validate_mat(const char* api_name, const jyppx_ocv_mat* mat, const char* argument_name)
    {
        return mat == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_train_data(const char* api_name, const jyppx_ocv_ml_train_data* train_data)
    {
        return train_data == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "train_data")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_grid(const char* api_name, const jyppx_ocv_ml_param_grid* grid)
    {
        return grid == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "grid")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_model(const char* api_name, const jyppx_ocv_ml_model* model)
    {
        return model == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "model")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_output_int(const char* api_name, const int* value, const char* argument_name)
    {
        return value == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_output_float(const char* api_name, const float* value, const char* argument_name)
    {
        return value == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_output_double(const char* api_name, const double* value, const char* argument_name)
    {
        return value == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_string(const char* api_name, const char* value, const char* argument_name)
    {
        if (value == nullptr || value[0] == '\0')
        {
            return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int validate_input_mat_array(
        const char* api_name,
        const jyppx_ocv_mat* const* values,
        int value_count,
        const char* argument_name)
    {
        if (value_count < 0 || (value_count > 0 && values == nullptr))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }

        for (int i = 0; i < value_count; ++i)
        {
            if (values[i] == nullptr)
            {
                return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
            }
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int validate_output_mat_array(
        const char* api_name,
        jyppx_ocv_mat* const* values,
        int value_count,
        const char* argument_name)
    {
        if (value_count < 0 || (value_count > 0 && values == nullptr))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }

        for (int i = 0; i < value_count; ++i)
        {
            if (values[i] == nullptr)
            {
                return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
            }
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
    cv::InputArray optional_input_array(const jyppx_ocv_mat* mat)
    {
        return mat == nullptr ? cv::noArray() : cv::InputArray(opencv_csharp_native::mat_value(mat));
    }

    cv::OutputArray optional_output_array(jyppx_ocv_mat* mat)
    {
        return mat == nullptr ? cv::noArray() : cv::OutputArray(opencv_csharp_native::mat_value(mat));
    }

    int create_grid_handle(const char* api_name, const cv::ml::ParamGrid& native, jyppx_ocv_ml_param_grid** grid)
    {
        if (grid == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "grid");
        }

        *grid = nullptr;
        jyppx_ocv_ml_param_grid* created = new (std::nothrow) jyppx_ocv_ml_param_grid();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = native;
        *grid = created;
        return OPENCV_CSHARP_STATUS_OK;
    }

    int create_train_data_handle(const char* api_name, const cv::Ptr<cv::ml::TrainData>& native, jyppx_ocv_ml_train_data** train_data)
    {
        if (train_data == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "train_data");
        }

        *train_data = nullptr;
        jyppx_ocv_ml_train_data* created = new (std::nothrow) jyppx_ocv_ml_train_data();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = native;
        *train_data = created;
        return OPENCV_CSHARP_STATUS_OK;
    }

    int create_model_handle(const char* api_name, const cv::Ptr<cv::ml::StatModel>& native, int kind, jyppx_ocv_ml_model** model)
    {
        if (model == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "model");
        }

        *model = nullptr;
        jyppx_ocv_ml_model* created = new (std::nothrow) jyppx_ocv_ml_model();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = native;
        created->kind = kind;
        *model = created;
        return OPENCV_CSHARP_STATUS_OK;
    }

    cv::Ptr<cv::ml::KNearest> as_knearest_ptr(const jyppx_ocv_ml_model* model)
    {
        return model == nullptr ? cv::Ptr<cv::ml::KNearest>() : model->value.dynamicCast<cv::ml::KNearest>();
    }

    cv::Ptr<cv::ml::SVM> as_svm_ptr(const jyppx_ocv_ml_model* model)
    {
        return model == nullptr ? cv::Ptr<cv::ml::SVM>() : model->value.dynamicCast<cv::ml::SVM>();
    }

    cv::Ptr<cv::ml::NormalBayesClassifier> as_normal_bayes_ptr(const jyppx_ocv_ml_model* model)
    {
        return model == nullptr ? cv::Ptr<cv::ml::NormalBayesClassifier>() : model->value.dynamicCast<cv::ml::NormalBayesClassifier>();
    }

    cv::Ptr<cv::ml::ANN_MLP> as_ann_mlp_ptr(const jyppx_ocv_ml_model* model)
    {
        return model == nullptr ? cv::Ptr<cv::ml::ANN_MLP>() : model->value.dynamicCast<cv::ml::ANN_MLP>();
    }

    cv::Ptr<cv::ml::DTrees> as_dtrees_ptr(const jyppx_ocv_ml_model* model)
    {
        return model == nullptr ? cv::Ptr<cv::ml::DTrees>() : model->value.dynamicCast<cv::ml::DTrees>();
    }

    cv::Ptr<cv::ml::RTrees> as_rtrees_ptr(const jyppx_ocv_ml_model* model)
    {
        return model == nullptr ? cv::Ptr<cv::ml::RTrees>() : model->value.dynamicCast<cv::ml::RTrees>();
    }

    cv::Ptr<cv::ml::Boost> as_boost_ptr(const jyppx_ocv_ml_model* model)
    {
        return model == nullptr ? cv::Ptr<cv::ml::Boost>() : model->value.dynamicCast<cv::ml::Boost>();
    }

    cv::Ptr<cv::ml::EM> as_em_ptr(const jyppx_ocv_ml_model* model)
    {
        return model == nullptr ? cv::Ptr<cv::ml::EM>() : model->value.dynamicCast<cv::ml::EM>();
    }

    cv::Ptr<cv::ml::ParamGrid> make_grid_ptr(const jyppx_ocv_ml_param_grid* grid, int param_id)
    {
        if (grid != nullptr)
        {
            return cv::ml::ParamGrid::create(grid->value.minVal, grid->value.maxVal, grid->value.logStep);
        }

        return cv::ml::SVM::getDefaultGridPtr(param_id);
    }

    void copy_mat_to_output(const cv::Mat& source, jyppx_ocv_mat* dst)
    {
        source.copyTo(opencv_csharp_native::mat_value(dst));
    }

    int checked_int_size(const char* api_name, size_t size, const char* argument_name)
    {
        if (size > static_cast<size_t>(std::numeric_limits<int>::max()))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    void collect_string_array_metrics(const std::vector<cv::String>& values, int* string_count, int* byte_count)
    {
        *string_count = static_cast<int>(values.size());
        int bytes = 0;
        for (const cv::String& value : values)
        {
            bytes += static_cast<int>(value.size());
        }

        *byte_count = bytes;
    }

    int fill_string_array_output(
        const char* api_name,
        const std::vector<cv::String>& values,
        int* offsets,
        int offset_capacity,
        char* buffer,
        int buffer_capacity,
        int* string_count,
        int* byte_count)
    {
        int status = checked_int_size(api_name, values.size(), "string_count");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        collect_string_array_metrics(values, string_count, byte_count);
        if (offsets == nullptr || offset_capacity < *string_count + 1)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "offsets");
        }

        if (*byte_count > 0 && (buffer == nullptr || buffer_capacity < *byte_count))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "buffer");
        }

        int cursor = 0;
        offsets[0] = 0;
        for (int i = 0; i < *string_count; ++i)
        {
            const cv::String& value = values[static_cast<size_t>(i)];
            if (!value.empty())
            {
                std::memcpy(buffer + cursor, value.data(), value.size());
            }

            cursor += static_cast<int>(value.size());
            offsets[i + 1] = cursor;
        }

        return OPENCV_CSHARP_STATUS_OK;
    }
#endif
}

int jyppx_ocv_ml_param_grid_create(double min_val, double max_val, double log_step, jyppx_ocv_ml_param_grid** grid)
{
    constexpr const char* api_name = "jyppx_ocv_ml_param_grid_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (grid == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "grid");
        }

        *grid = nullptr;
        jyppx_ocv_ml_param_grid* created = new (std::nothrow) jyppx_ocv_ml_param_grid();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        created->value = cv::ml::ParamGrid(min_val, max_val, log_step);
#else
        created->min_val = min_val;
        created->max_val = max_val;
        created->log_step = log_step;
#endif
        *grid = created;
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_ml_param_grid_release_handle(jyppx_ocv_ml_param_grid* grid)
{
    delete grid;
}

int jyppx_ocv_ml_param_grid_get(const jyppx_ocv_ml_param_grid* grid, double* min_val, double* max_val, double* log_step)
{
    constexpr const char* api_name = "jyppx_ocv_ml_param_grid_get";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_grid(api_name, grid);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_double(api_name, min_val, "min_val");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_double(api_name, max_val, "max_val");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_double(api_name, log_step, "log_step");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        *min_val = grid->value.minVal;
        *max_val = grid->value.maxVal;
        *log_step = grid->value.logStep;
#else
        *min_val = grid->min_val;
        *max_val = grid->max_val;
        *log_step = grid->log_step;
#endif
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_param_grid_set(jyppx_ocv_ml_param_grid* grid, double min_val, double max_val, double log_step)
{
    constexpr const char* api_name = "jyppx_ocv_ml_param_grid_set";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_grid(api_name, grid);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        grid->value.minVal = min_val;
        grid->value.maxVal = max_val;
        grid->value.logStep = log_step;
#else
        grid->min_val = min_val;
        grid->max_val = max_val;
        grid->log_step = log_step;
#endif
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_svm_get_default_grid(int param_id, jyppx_ocv_ml_param_grid** grid)
{
    constexpr const char* api_name = "jyppx_ocv_ml_svm_get_default_grid";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        return create_grid_handle(api_name, cv::ml::SVM::getDefaultGrid(param_id), grid);
#else
        if (grid != nullptr) { *grid = nullptr; }
        (void)param_id;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_train_data_create(
    const jyppx_ocv_mat* samples,
    int layout,
    const jyppx_ocv_mat* responses,
    const jyppx_ocv_mat* var_idx,
    const jyppx_ocv_mat* sample_idx,
    const jyppx_ocv_mat* sample_weights,
    const jyppx_ocv_mat* var_type,
    jyppx_ocv_ml_train_data** train_data)
{
    constexpr const char* api_name = "jyppx_ocv_ml_train_data_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, samples, "samples");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, responses, "responses");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::TrainData> native = cv::ml::TrainData::create(
            opencv_csharp_native::mat_value(samples),
            layout,
            opencv_csharp_native::mat_value(responses),
            optional_input_array(var_idx),
            optional_input_array(sample_idx),
            optional_input_array(sample_weights),
            optional_input_array(var_type));
        return create_train_data_handle(api_name, native, train_data);
#else
        (void)layout; (void)var_idx; (void)sample_idx; (void)sample_weights; (void)var_type;
        if (train_data != nullptr) { *train_data = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_train_data_load_csv(
    const char* filename,
    int header_line_count,
    int response_start_idx,
    int response_end_idx,
    const char* var_type_spec,
    int delimiter,
    int missch,
    jyppx_ocv_ml_train_data** train_data)
{
    constexpr const char* api_name = "jyppx_ocv_ml_train_data_load_csv";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_string(api_name, filename, "filename");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::TrainData> native = cv::ml::TrainData::loadFromCSV(
            filename,
            header_line_count,
            response_start_idx,
            response_end_idx,
            var_type_spec == nullptr ? cv::String() : cv::String(var_type_spec),
            static_cast<char>(delimiter),
            static_cast<char>(missch));
        return create_train_data_handle(api_name, native, train_data);
#else
        (void)header_line_count; (void)response_start_idx; (void)response_end_idx; (void)var_type_spec; (void)delimiter; (void)missch;
        if (train_data != nullptr) { *train_data = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_ml_train_data_release_handle(jyppx_ocv_ml_train_data* train_data)
{
    delete train_data;
}

int jyppx_ocv_ml_train_data_get_int(const jyppx_ocv_ml_train_data* train_data, int property_id, int argument, int* value)
{
    constexpr const char* api_name = "jyppx_ocv_ml_train_data_get_int";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_train_data(api_name, train_data);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        switch (property_id)
        {
        case TRAIN_DATA_INT_LAYOUT: *value = train_data->value->getLayout(); return OPENCV_CSHARP_STATUS_OK;
        case TRAIN_DATA_INT_N_TRAIN_SAMPLES: *value = train_data->value->getNTrainSamples(); return OPENCV_CSHARP_STATUS_OK;
        case TRAIN_DATA_INT_N_TEST_SAMPLES: *value = train_data->value->getNTestSamples(); return OPENCV_CSHARP_STATUS_OK;
        case TRAIN_DATA_INT_N_SAMPLES: *value = train_data->value->getNSamples(); return OPENCV_CSHARP_STATUS_OK;
        case TRAIN_DATA_INT_N_VARS: *value = train_data->value->getNVars(); return OPENCV_CSHARP_STATUS_OK;
        case TRAIN_DATA_INT_N_ALL_VARS: *value = train_data->value->getNAllVars(); return OPENCV_CSHARP_STATUS_OK;
        case TRAIN_DATA_INT_RESPONSE_TYPE: *value = train_data->value->getResponseType(); return OPENCV_CSHARP_STATUS_OK;
        case TRAIN_DATA_INT_CAT_COUNT: *value = train_data->value->getCatCount(argument); return OPENCV_CSHARP_STATUS_OK;
        default: return opencv_csharp_native::set_invalid_argument(api_name, "property_id");
        }
#else
        (void)property_id; (void)argument;
        *value = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_train_data_get_mat(
    const jyppx_ocv_ml_train_data* train_data,
    int property_id,
    int layout,
    int compress_samples,
    int compress_vars,
    jyppx_ocv_mat* dst)
{
    constexpr const char* api_name = "jyppx_ocv_ml_train_data_get_mat";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_train_data(api_name, train_data);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Mat result;
        switch (property_id)
        {
        case TRAIN_DATA_MAT_SAMPLES: result = train_data->value->getSamples(); break;
        case TRAIN_DATA_MAT_MISSING: result = train_data->value->getMissing(); break;
        case TRAIN_DATA_MAT_TRAIN_SAMPLES: result = train_data->value->getTrainSamples(layout, compress_samples != 0, compress_vars != 0); break;
        case TRAIN_DATA_MAT_TRAIN_RESPONSES: result = train_data->value->getTrainResponses(); break;
        case TRAIN_DATA_MAT_TRAIN_NORM_CAT_RESPONSES: result = train_data->value->getTrainNormCatResponses(); break;
        case TRAIN_DATA_MAT_TEST_RESPONSES: result = train_data->value->getTestResponses(); break;
        case TRAIN_DATA_MAT_TEST_NORM_CAT_RESPONSES: result = train_data->value->getTestNormCatResponses(); break;
        case TRAIN_DATA_MAT_RESPONSES: result = train_data->value->getResponses(); break;
        case TRAIN_DATA_MAT_NORM_CAT_RESPONSES: result = train_data->value->getNormCatResponses(); break;
        case TRAIN_DATA_MAT_SAMPLE_WEIGHTS: result = train_data->value->getSampleWeights(); break;
        case TRAIN_DATA_MAT_TRAIN_SAMPLE_WEIGHTS: result = train_data->value->getTrainSampleWeights(); break;
        case TRAIN_DATA_MAT_TEST_SAMPLE_WEIGHTS: result = train_data->value->getTestSampleWeights(); break;
        case TRAIN_DATA_MAT_VAR_IDX: result = train_data->value->getVarIdx(); break;
        case TRAIN_DATA_MAT_VAR_TYPE: result = train_data->value->getVarType(); break;
        case TRAIN_DATA_MAT_VAR_SYMBOL_FLAGS: result = train_data->value->getVarSymbolFlags(); break;
        case TRAIN_DATA_MAT_TRAIN_SAMPLE_IDX: result = train_data->value->getTrainSampleIdx(); break;
        case TRAIN_DATA_MAT_TEST_SAMPLE_IDX: result = train_data->value->getTestSampleIdx(); break;
        case TRAIN_DATA_MAT_DEFAULT_SUBST_VALUES: result = train_data->value->getDefaultSubstValues(); break;
        case TRAIN_DATA_MAT_CLASS_LABELS: result = train_data->value->getClassLabels(); break;
        case TRAIN_DATA_MAT_CAT_OFS: result = train_data->value->getCatOfs(); break;
        case TRAIN_DATA_MAT_CAT_MAP: result = train_data->value->getCatMap(); break;
        case TRAIN_DATA_MAT_TEST_SAMPLES: result = train_data->value->getTestSamples(); break;
        default: return opencv_csharp_native::set_invalid_argument(api_name, "property_id");
        }

        copy_mat_to_output(result, dst);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)property_id; (void)layout; (void)compress_samples; (void)compress_vars;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_train_data_set_train_test_split(jyppx_ocv_ml_train_data* train_data, int count, int shuffle)
{
    constexpr const char* api_name = "jyppx_ocv_ml_train_data_set_train_test_split";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_train_data(api_name, train_data);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        train_data->value->setTrainTestSplit(count, shuffle != 0);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)count; (void)shuffle;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_train_data_set_train_test_split_ratio(jyppx_ocv_ml_train_data* train_data, double ratio, int shuffle)
{
    constexpr const char* api_name = "jyppx_ocv_ml_train_data_set_train_test_split_ratio";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_train_data(api_name, train_data);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        train_data->value->setTrainTestSplitRatio(ratio, shuffle != 0);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)ratio; (void)shuffle;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_train_data_shuffle_train_test(jyppx_ocv_ml_train_data* train_data)
{
    constexpr const char* api_name = "jyppx_ocv_ml_train_data_shuffle_train_test";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_train_data(api_name, train_data);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        train_data->value->shuffleTrainTest();
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_train_data_get_names_count(const jyppx_ocv_ml_train_data* train_data, int* string_count, int* byte_count)
{
    constexpr const char* api_name = "jyppx_ocv_ml_train_data_get_names_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_train_data(api_name, train_data);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, string_count, "string_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, byte_count, "byte_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        std::vector<cv::String> names;
        train_data->value->getNames(names);
        status = checked_int_size(api_name, names.size(), "names");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        collect_string_array_metrics(names, string_count, byte_count);
        return OPENCV_CSHARP_STATUS_OK;
#else
        *string_count = 0;
        *byte_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_train_data_get_names_fill(
    const jyppx_ocv_ml_train_data* train_data,
    int* offsets,
    int offset_capacity,
    char* buffer,
    int buffer_capacity,
    int* string_count,
    int* byte_count)
{
    constexpr const char* api_name = "jyppx_ocv_ml_train_data_get_names_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_train_data(api_name, train_data);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, string_count, "string_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, byte_count, "byte_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        std::vector<cv::String> names;
        train_data->value->getNames(names);
        return fill_string_array_output(api_name, names, offsets, offset_capacity, buffer, buffer_capacity, string_count, byte_count);
#else
        (void)offsets; (void)offset_capacity; (void)buffer; (void)buffer_capacity;
        *string_count = 0;
        *byte_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_train_data_get_sub_vector(const jyppx_ocv_mat* vec, const jyppx_ocv_mat* idx, jyppx_ocv_mat* dst)
{
    constexpr const char* api_name = "jyppx_ocv_ml_train_data_get_sub_vector";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, vec, "vec");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, idx, "idx");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        copy_mat_to_output(cv::ml::TrainData::getSubVector(opencv_csharp_native::mat_value(vec), opencv_csharp_native::mat_value(idx)), dst);
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_train_data_get_sub_matrix(const jyppx_ocv_mat* matrix, const jyppx_ocv_mat* idx, int layout, jyppx_ocv_mat* dst)
{
    constexpr const char* api_name = "jyppx_ocv_ml_train_data_get_sub_matrix";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, matrix, "matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, idx, "idx");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        copy_mat_to_output(cv::ml::TrainData::getSubMatrix(opencv_csharp_native::mat_value(matrix), opencv_csharp_native::mat_value(idx), layout), dst);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)layout;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_knearest_create(jyppx_ocv_ml_model** model)
{
    constexpr const char* api_name = "jyppx_ocv_ml_knearest_create";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        return create_model_handle(api_name, cv::ml::KNearest::create(), MODEL_KIND_KNEAREST, model);
#else
        if (model != nullptr) { *model = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_knearest_load(const char* filepath, jyppx_ocv_ml_model** model)
{
    constexpr const char* api_name = "jyppx_ocv_ml_knearest_load";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_string(api_name, filepath, "filepath");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        return create_model_handle(api_name, cv::ml::KNearest::load(filepath), MODEL_KIND_KNEAREST, model);
#else
        if (model != nullptr) { *model = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_svm_create(jyppx_ocv_ml_model** model)
{
    constexpr const char* api_name = "jyppx_ocv_ml_svm_create";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        return create_model_handle(api_name, cv::ml::SVM::create(), MODEL_KIND_SVM, model);
#else
        if (model != nullptr) { *model = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_svm_load(const char* filepath, jyppx_ocv_ml_model** model)
{
    constexpr const char* api_name = "jyppx_ocv_ml_svm_load";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_string(api_name, filepath, "filepath");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        return create_model_handle(api_name, cv::ml::SVM::load(filepath), MODEL_KIND_SVM, model);
#else
        if (model != nullptr) { *model = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_normal_bayes_classifier_create(jyppx_ocv_ml_model** model)
{
    constexpr const char* api_name = "jyppx_ocv_ml_normal_bayes_classifier_create";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        return create_model_handle(api_name, cv::ml::NormalBayesClassifier::create(), MODEL_KIND_NORMAL_BAYES, model);
#else
        if (model != nullptr) { *model = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_normal_bayes_classifier_load(const char* filepath, const char* node_name, jyppx_ocv_ml_model** model)
{
    constexpr const char* api_name = "jyppx_ocv_ml_normal_bayes_classifier_load";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_string(api_name, filepath, "filepath");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        return create_model_handle(api_name, cv::ml::NormalBayesClassifier::load(filepath, node_name == nullptr ? cv::String() : cv::String(node_name)), MODEL_KIND_NORMAL_BAYES, model);
#else
        (void)node_name;
        if (model != nullptr) { *model = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_ann_mlp_create(jyppx_ocv_ml_model** model)
{
    constexpr const char* api_name = "jyppx_ocv_ml_ann_mlp_create";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        return create_model_handle(api_name, cv::ml::ANN_MLP::create(), MODEL_KIND_ANN_MLP, model);
#else
        if (model != nullptr) { *model = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_ann_mlp_load(const char* filepath, jyppx_ocv_ml_model** model)
{
    constexpr const char* api_name = "jyppx_ocv_ml_ann_mlp_load";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_string(api_name, filepath, "filepath");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        return create_model_handle(api_name, cv::ml::ANN_MLP::load(filepath), MODEL_KIND_ANN_MLP, model);
#else
        if (model != nullptr) { *model = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_dtrees_create(jyppx_ocv_ml_model** model)
{
    constexpr const char* api_name = "jyppx_ocv_ml_dtrees_create";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        return create_model_handle(api_name, cv::ml::DTrees::create(), MODEL_KIND_DTREES, model);
#else
        if (model != nullptr) { *model = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_dtrees_load(const char* filepath, const char* node_name, jyppx_ocv_ml_model** model)
{
    constexpr const char* api_name = "jyppx_ocv_ml_dtrees_load";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_string(api_name, filepath, "filepath");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        return create_model_handle(
            api_name,
            cv::ml::DTrees::load(filepath, node_name == nullptr ? cv::String() : cv::String(node_name)),
            MODEL_KIND_DTREES,
            model);
#else
        (void)node_name;
        if (model != nullptr) { *model = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_rtrees_create(jyppx_ocv_ml_model** model)
{
    constexpr const char* api_name = "jyppx_ocv_ml_rtrees_create";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        return create_model_handle(api_name, cv::ml::RTrees::create(), MODEL_KIND_RTREES, model);
#else
        if (model != nullptr) { *model = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_rtrees_load(const char* filepath, const char* node_name, jyppx_ocv_ml_model** model)
{
    constexpr const char* api_name = "jyppx_ocv_ml_rtrees_load";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_string(api_name, filepath, "filepath");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        return create_model_handle(
            api_name,
            cv::ml::RTrees::load(filepath, node_name == nullptr ? cv::String() : cv::String(node_name)),
            MODEL_KIND_RTREES,
            model);
#else
        (void)node_name;
        if (model != nullptr) { *model = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_boost_create(jyppx_ocv_ml_model** model)
{
    constexpr const char* api_name = "jyppx_ocv_ml_boost_create";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        return create_model_handle(api_name, cv::ml::Boost::create(), MODEL_KIND_BOOST, model);
#else
        if (model != nullptr) { *model = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_boost_load(const char* filepath, const char* node_name, jyppx_ocv_ml_model** model)
{
    constexpr const char* api_name = "jyppx_ocv_ml_boost_load";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_string(api_name, filepath, "filepath");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        return create_model_handle(
            api_name,
            cv::ml::Boost::load(filepath, node_name == nullptr ? cv::String() : cv::String(node_name)),
            MODEL_KIND_BOOST,
            model);
#else
        (void)node_name;
        if (model != nullptr) { *model = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_em_create(jyppx_ocv_ml_model** model)
{
    constexpr const char* api_name = "jyppx_ocv_ml_em_create";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        return create_model_handle(api_name, cv::ml::EM::create(), MODEL_KIND_EM, model);
#else
        if (model != nullptr) { *model = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_em_load(const char* filepath, const char* node_name, jyppx_ocv_ml_model** model)
{
    constexpr const char* api_name = "jyppx_ocv_ml_em_load";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_string(api_name, filepath, "filepath");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        return create_model_handle(
            api_name,
            cv::ml::EM::load(filepath, node_name == nullptr ? cv::String() : cv::String(node_name)),
            MODEL_KIND_EM,
            model);
#else
        (void)node_name;
        if (model != nullptr) { *model = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_ml_model_release_handle(jyppx_ocv_ml_model* model)
{
    delete model;
}

int jyppx_ocv_ml_stat_model_get_int(const jyppx_ocv_ml_model* model, int property_id, int* value)
{
    constexpr const char* api_name = "jyppx_ocv_ml_stat_model_get_int";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        switch (property_id)
        {
        case STAT_MODEL_INT_VAR_COUNT: *value = model->value->getVarCount(); return OPENCV_CSHARP_STATUS_OK;
        case STAT_MODEL_INT_EMPTY: *value = model->value->empty() ? 1 : 0; return OPENCV_CSHARP_STATUS_OK;
        case STAT_MODEL_INT_IS_TRAINED: *value = model->value->isTrained() ? 1 : 0; return OPENCV_CSHARP_STATUS_OK;
        case STAT_MODEL_INT_IS_CLASSIFIER: *value = model->value->isClassifier() ? 1 : 0; return OPENCV_CSHARP_STATUS_OK;
        default: return opencv_csharp_native::set_invalid_argument(api_name, "property_id");
        }
#else
        (void)property_id;
        *value = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_stat_model_train_data(jyppx_ocv_ml_model* model, const jyppx_ocv_ml_train_data* train_data, int flags, int* result)
{
    constexpr const char* api_name = "jyppx_ocv_ml_stat_model_train_data";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_train_data(api_name, train_data);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, result, "result");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        *result = model->value->train(train_data->value, flags) ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)flags;
        *result = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_stat_model_train_samples(jyppx_ocv_ml_model* model, const jyppx_ocv_mat* samples, int layout, const jyppx_ocv_mat* responses, int* result)
{
    constexpr const char* api_name = "jyppx_ocv_ml_stat_model_train_samples";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, samples, "samples");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, responses, "responses");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, result, "result");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        *result = model->value->train(opencv_csharp_native::mat_value(samples), layout, opencv_csharp_native::mat_value(responses)) ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)layout;
        *result = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_stat_model_predict(const jyppx_ocv_ml_model* model, const jyppx_ocv_mat* samples, jyppx_ocv_mat* results, int flags, float* value)
{
    constexpr const char* api_name = "jyppx_ocv_ml_stat_model_predict";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, samples, "samples");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_float(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        *value = model->value->predict(opencv_csharp_native::mat_value(samples), optional_output_array(results), flags);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)results; (void)flags;
        *value = 0.0F;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_stat_model_calc_error(const jyppx_ocv_ml_model* model, const jyppx_ocv_ml_train_data* data, int test, jyppx_ocv_mat* responses, float* value)
{
    constexpr const char* api_name = "jyppx_ocv_ml_stat_model_calc_error";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_train_data(api_name, data);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_float(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        *value = model->value->calcError(data->value, test != 0, optional_output_array(responses));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)test; (void)responses;
        *value = 0.0F;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_stat_model_save(const jyppx_ocv_ml_model* model, const char* filepath)
{
    constexpr const char* api_name = "jyppx_ocv_ml_stat_model_save";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_string(api_name, filepath, "filepath");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        model->value->save(filepath);
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_stat_model_clear(jyppx_ocv_ml_model* model)
{
    constexpr const char* api_name = "jyppx_ocv_ml_stat_model_clear";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        model->value->clear();
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_knearest_get_int(const jyppx_ocv_ml_model* model, int property_id, int* value)
{
    constexpr const char* api_name = "jyppx_ocv_ml_knearest_get_int";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::KNearest> native = as_knearest_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        switch (property_id)
        {
        case KNEAREST_INT_DEFAULT_K: *value = native->getDefaultK(); return OPENCV_CSHARP_STATUS_OK;
        case KNEAREST_INT_IS_CLASSIFIER: *value = native->getIsClassifier() ? 1 : 0; return OPENCV_CSHARP_STATUS_OK;
        case KNEAREST_INT_EMAX: *value = native->getEmax(); return OPENCV_CSHARP_STATUS_OK;
        case KNEAREST_INT_ALGORITHM_TYPE: *value = native->getAlgorithmType(); return OPENCV_CSHARP_STATUS_OK;
        default: return opencv_csharp_native::set_invalid_argument(api_name, "property_id");
        }
#else
        (void)property_id;
        *value = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_knearest_set_int(jyppx_ocv_ml_model* model, int property_id, int value)
{
    constexpr const char* api_name = "jyppx_ocv_ml_knearest_set_int";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::KNearest> native = as_knearest_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        switch (property_id)
        {
        case KNEAREST_INT_DEFAULT_K: native->setDefaultK(value); return OPENCV_CSHARP_STATUS_OK;
        case KNEAREST_INT_IS_CLASSIFIER: native->setIsClassifier(value != 0); return OPENCV_CSHARP_STATUS_OK;
        case KNEAREST_INT_EMAX: native->setEmax(value); return OPENCV_CSHARP_STATUS_OK;
        case KNEAREST_INT_ALGORITHM_TYPE: native->setAlgorithmType(value); return OPENCV_CSHARP_STATUS_OK;
        default: return opencv_csharp_native::set_invalid_argument(api_name, "property_id");
        }
#else
        (void)property_id; (void)value;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_knearest_find_nearest(
    const jyppx_ocv_ml_model* model,
    const jyppx_ocv_mat* samples,
    int k,
    jyppx_ocv_mat* results,
    jyppx_ocv_mat* neighbor_responses,
    jyppx_ocv_mat* dist,
    float* value)
{
    constexpr const char* api_name = "jyppx_ocv_ml_knearest_find_nearest";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, samples, "samples");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_float(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::KNearest> native = as_knearest_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        *value = native->findNearest(
            opencv_csharp_native::mat_value(samples),
            k,
            optional_output_array(results),
            optional_output_array(neighbor_responses),
            optional_output_array(dist));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)k; (void)results; (void)neighbor_responses; (void)dist;
        *value = 0.0F;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_svm_get_int(const jyppx_ocv_ml_model* model, int property_id, int* value)
{
    constexpr const char* api_name = "jyppx_ocv_ml_svm_get_int";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::SVM> native = as_svm_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        switch (property_id)
        {
        case SVM_INT_TYPE: *value = native->getType(); return OPENCV_CSHARP_STATUS_OK;
        case SVM_INT_KERNEL_TYPE: *value = native->getKernelType(); return OPENCV_CSHARP_STATUS_OK;
        default: return opencv_csharp_native::set_invalid_argument(api_name, "property_id");
        }
#else
        (void)property_id;
        *value = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_svm_set_int(jyppx_ocv_ml_model* model, int property_id, int value)
{
    constexpr const char* api_name = "jyppx_ocv_ml_svm_set_int";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::SVM> native = as_svm_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        switch (property_id)
        {
        case SVM_INT_TYPE: native->setType(value); return OPENCV_CSHARP_STATUS_OK;
        case SVM_INT_KERNEL_TYPE: native->setKernel(value); return OPENCV_CSHARP_STATUS_OK;
        default: return opencv_csharp_native::set_invalid_argument(api_name, "property_id");
        }
#else
        (void)property_id; (void)value;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_svm_get_double(const jyppx_ocv_ml_model* model, int property_id, double* value)
{
    constexpr const char* api_name = "jyppx_ocv_ml_svm_get_double";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_double(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::SVM> native = as_svm_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        switch (property_id)
        {
        case SVM_DOUBLE_GAMMA: *value = native->getGamma(); return OPENCV_CSHARP_STATUS_OK;
        case SVM_DOUBLE_COEF0: *value = native->getCoef0(); return OPENCV_CSHARP_STATUS_OK;
        case SVM_DOUBLE_DEGREE: *value = native->getDegree(); return OPENCV_CSHARP_STATUS_OK;
        case SVM_DOUBLE_C: *value = native->getC(); return OPENCV_CSHARP_STATUS_OK;
        case SVM_DOUBLE_NU: *value = native->getNu(); return OPENCV_CSHARP_STATUS_OK;
        case SVM_DOUBLE_P: *value = native->getP(); return OPENCV_CSHARP_STATUS_OK;
        default: return opencv_csharp_native::set_invalid_argument(api_name, "property_id");
        }
#else
        (void)property_id;
        *value = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_svm_set_double(jyppx_ocv_ml_model* model, int property_id, double value)
{
    constexpr const char* api_name = "jyppx_ocv_ml_svm_set_double";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::SVM> native = as_svm_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        switch (property_id)
        {
        case SVM_DOUBLE_GAMMA: native->setGamma(value); return OPENCV_CSHARP_STATUS_OK;
        case SVM_DOUBLE_COEF0: native->setCoef0(value); return OPENCV_CSHARP_STATUS_OK;
        case SVM_DOUBLE_DEGREE: native->setDegree(value); return OPENCV_CSHARP_STATUS_OK;
        case SVM_DOUBLE_C: native->setC(value); return OPENCV_CSHARP_STATUS_OK;
        case SVM_DOUBLE_NU: native->setNu(value); return OPENCV_CSHARP_STATUS_OK;
        case SVM_DOUBLE_P: native->setP(value); return OPENCV_CSHARP_STATUS_OK;
        default: return opencv_csharp_native::set_invalid_argument(api_name, "property_id");
        }
#else
        (void)property_id; (void)value;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_svm_get_term_criteria(const jyppx_ocv_ml_model* model, int* type, int* max_count, double* epsilon)
{
    constexpr const char* api_name = "jyppx_ocv_ml_svm_get_term_criteria";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, type, "type");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, max_count, "max_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_double(api_name, epsilon, "epsilon");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::SVM> native = as_svm_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        cv::TermCriteria criteria = native->getTermCriteria();
        *type = criteria.type;
        *max_count = criteria.maxCount;
        *epsilon = criteria.epsilon;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *type = 0;
        *max_count = 0;
        *epsilon = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_svm_set_term_criteria(jyppx_ocv_ml_model* model, int type, int max_count, double epsilon)
{
    constexpr const char* api_name = "jyppx_ocv_ml_svm_set_term_criteria";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::SVM> native = as_svm_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        native->setTermCriteria(cv::TermCriteria(type, max_count, epsilon));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)type; (void)max_count; (void)epsilon;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_svm_get_class_weights(const jyppx_ocv_ml_model* model, jyppx_ocv_mat* dst)
{
    constexpr const char* api_name = "jyppx_ocv_ml_svm_get_class_weights";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::SVM> native = as_svm_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        copy_mat_to_output(native->getClassWeights(), dst);
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_svm_set_class_weights(jyppx_ocv_ml_model* model, const jyppx_ocv_mat* weights)
{
    constexpr const char* api_name = "jyppx_ocv_ml_svm_set_class_weights";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, weights, "weights");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::SVM> native = as_svm_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        native->setClassWeights(opencv_csharp_native::mat_value(weights));
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_svm_train_auto(
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
    int* result)
{
    constexpr const char* api_name = "jyppx_ocv_ml_svm_train_auto";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, samples, "samples");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, responses, "responses");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, result, "result");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::SVM> native = as_svm_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        *result = native->trainAuto(
            opencv_csharp_native::mat_value(samples),
            layout,
            opencv_csharp_native::mat_value(responses),
            k_fold,
            make_grid_ptr(c_grid, cv::ml::SVM::C),
            make_grid_ptr(gamma_grid, cv::ml::SVM::GAMMA),
            make_grid_ptr(p_grid, cv::ml::SVM::P),
            make_grid_ptr(nu_grid, cv::ml::SVM::NU),
            make_grid_ptr(coeff_grid, cv::ml::SVM::COEF),
            make_grid_ptr(degree_grid, cv::ml::SVM::DEGREE),
            balanced != 0) ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)layout; (void)k_fold; (void)c_grid; (void)gamma_grid; (void)p_grid; (void)nu_grid; (void)coeff_grid; (void)degree_grid; (void)balanced;
        *result = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_svm_get_support_vectors(const jyppx_ocv_ml_model* model, jyppx_ocv_mat* dst)
{
    constexpr const char* api_name = "jyppx_ocv_ml_svm_get_support_vectors";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::SVM> native = as_svm_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        copy_mat_to_output(native->getSupportVectors(), dst);
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_svm_get_uncompressed_support_vectors(const jyppx_ocv_ml_model* model, jyppx_ocv_mat* dst)
{
    constexpr const char* api_name = "jyppx_ocv_ml_svm_get_uncompressed_support_vectors";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::SVM> native = as_svm_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        copy_mat_to_output(native->getUncompressedSupportVectors(), dst);
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_svm_get_decision_function(const jyppx_ocv_ml_model* model, int index, jyppx_ocv_mat* alpha, jyppx_ocv_mat* svidx, double* rho)
{
    constexpr const char* api_name = "jyppx_ocv_ml_svm_get_decision_function";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_double(api_name, rho, "rho");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::SVM> native = as_svm_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        *rho = native->getDecisionFunction(index, optional_output_array(alpha), optional_output_array(svidx));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)index; (void)alpha; (void)svidx;
        *rho = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_normal_bayes_classifier_predict_prob(
    const jyppx_ocv_ml_model* model,
    const jyppx_ocv_mat* inputs,
    jyppx_ocv_mat* outputs,
    jyppx_ocv_mat* output_probs,
    int flags,
    float* value)
{
    constexpr const char* api_name = "jyppx_ocv_ml_normal_bayes_classifier_predict_prob";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, inputs, "inputs");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, outputs, "outputs");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, output_probs, "output_probs");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_float(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::NormalBayesClassifier> native = as_normal_bayes_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        *value = native->predictProb(
            opencv_csharp_native::mat_value(inputs),
            cv::OutputArray(opencv_csharp_native::mat_value(outputs)),
            cv::OutputArray(opencv_csharp_native::mat_value(output_probs)),
            flags);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)flags;
        *value = 0.0F;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_ann_mlp_get_int(const jyppx_ocv_ml_model* model, int property_id, int* value)
{
    constexpr const char* api_name = "jyppx_ocv_ml_ann_mlp_get_int";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::ANN_MLP> native = as_ann_mlp_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        switch (property_id)
        {
        case ANN_MLP_INT_TRAIN_METHOD: *value = native->getTrainMethod(); return OPENCV_CSHARP_STATUS_OK;
        case ANN_MLP_INT_ANNEAL_ITE_PER_STEP: *value = native->getAnnealItePerStep(); return OPENCV_CSHARP_STATUS_OK;
        default: return opencv_csharp_native::set_invalid_argument(api_name, "property_id");
        }
#else
        (void)property_id;
        *value = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_ann_mlp_set_int(jyppx_ocv_ml_model* model, int property_id, int value)
{
    constexpr const char* api_name = "jyppx_ocv_ml_ann_mlp_set_int";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::ANN_MLP> native = as_ann_mlp_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        switch (property_id)
        {
        case ANN_MLP_INT_ANNEAL_ITE_PER_STEP: native->setAnnealItePerStep(value); return OPENCV_CSHARP_STATUS_OK;
        default: return opencv_csharp_native::set_invalid_argument(api_name, "property_id");
        }
#else
        (void)property_id; (void)value;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_ann_mlp_get_double(const jyppx_ocv_ml_model* model, int property_id, double* value)
{
    constexpr const char* api_name = "jyppx_ocv_ml_ann_mlp_get_double";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_double(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::ANN_MLP> native = as_ann_mlp_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        switch (property_id)
        {
        case ANN_MLP_DOUBLE_BACKPROP_WEIGHT_SCALE: *value = native->getBackpropWeightScale(); return OPENCV_CSHARP_STATUS_OK;
        case ANN_MLP_DOUBLE_BACKPROP_MOMENTUM_SCALE: *value = native->getBackpropMomentumScale(); return OPENCV_CSHARP_STATUS_OK;
        case ANN_MLP_DOUBLE_RPROP_DW0: *value = native->getRpropDW0(); return OPENCV_CSHARP_STATUS_OK;
        case ANN_MLP_DOUBLE_RPROP_DW_PLUS: *value = native->getRpropDWPlus(); return OPENCV_CSHARP_STATUS_OK;
        case ANN_MLP_DOUBLE_RPROP_DW_MINUS: *value = native->getRpropDWMinus(); return OPENCV_CSHARP_STATUS_OK;
        case ANN_MLP_DOUBLE_RPROP_DW_MIN: *value = native->getRpropDWMin(); return OPENCV_CSHARP_STATUS_OK;
        case ANN_MLP_DOUBLE_RPROP_DW_MAX: *value = native->getRpropDWMax(); return OPENCV_CSHARP_STATUS_OK;
        case ANN_MLP_DOUBLE_ANNEAL_INITIAL_T: *value = native->getAnnealInitialT(); return OPENCV_CSHARP_STATUS_OK;
        case ANN_MLP_DOUBLE_ANNEAL_FINAL_T: *value = native->getAnnealFinalT(); return OPENCV_CSHARP_STATUS_OK;
        case ANN_MLP_DOUBLE_ANNEAL_COOLING_RATIO: *value = native->getAnnealCoolingRatio(); return OPENCV_CSHARP_STATUS_OK;
        default: return opencv_csharp_native::set_invalid_argument(api_name, "property_id");
        }
#else
        (void)property_id;
        *value = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_ann_mlp_set_double(jyppx_ocv_ml_model* model, int property_id, double value)
{
    constexpr const char* api_name = "jyppx_ocv_ml_ann_mlp_set_double";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::ANN_MLP> native = as_ann_mlp_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        switch (property_id)
        {
        case ANN_MLP_DOUBLE_BACKPROP_WEIGHT_SCALE: native->setBackpropWeightScale(value); return OPENCV_CSHARP_STATUS_OK;
        case ANN_MLP_DOUBLE_BACKPROP_MOMENTUM_SCALE: native->setBackpropMomentumScale(value); return OPENCV_CSHARP_STATUS_OK;
        case ANN_MLP_DOUBLE_RPROP_DW0: native->setRpropDW0(value); return OPENCV_CSHARP_STATUS_OK;
        case ANN_MLP_DOUBLE_RPROP_DW_PLUS: native->setRpropDWPlus(value); return OPENCV_CSHARP_STATUS_OK;
        case ANN_MLP_DOUBLE_RPROP_DW_MINUS: native->setRpropDWMinus(value); return OPENCV_CSHARP_STATUS_OK;
        case ANN_MLP_DOUBLE_RPROP_DW_MIN: native->setRpropDWMin(value); return OPENCV_CSHARP_STATUS_OK;
        case ANN_MLP_DOUBLE_RPROP_DW_MAX: native->setRpropDWMax(value); return OPENCV_CSHARP_STATUS_OK;
        case ANN_MLP_DOUBLE_ANNEAL_INITIAL_T: native->setAnnealInitialT(value); return OPENCV_CSHARP_STATUS_OK;
        case ANN_MLP_DOUBLE_ANNEAL_FINAL_T: native->setAnnealFinalT(value); return OPENCV_CSHARP_STATUS_OK;
        case ANN_MLP_DOUBLE_ANNEAL_COOLING_RATIO: native->setAnnealCoolingRatio(value); return OPENCV_CSHARP_STATUS_OK;
        default: return opencv_csharp_native::set_invalid_argument(api_name, "property_id");
        }
#else
        (void)property_id; (void)value;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_ann_mlp_set_train_method(jyppx_ocv_ml_model* model, int method, double param1, double param2)
{
    constexpr const char* api_name = "jyppx_ocv_ml_ann_mlp_set_train_method";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::ANN_MLP> native = as_ann_mlp_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        native->setTrainMethod(method, param1, param2);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)method; (void)param1; (void)param2;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_ann_mlp_set_activation_function(jyppx_ocv_ml_model* model, int type, double param1, double param2)
{
    constexpr const char* api_name = "jyppx_ocv_ml_ann_mlp_set_activation_function";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::ANN_MLP> native = as_ann_mlp_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        native->setActivationFunction(type, param1, param2);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)type; (void)param1; (void)param2;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_ann_mlp_get_layer_sizes(const jyppx_ocv_ml_model* model, jyppx_ocv_mat* dst)
{
    constexpr const char* api_name = "jyppx_ocv_ml_ann_mlp_get_layer_sizes";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::ANN_MLP> native = as_ann_mlp_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        copy_mat_to_output(native->getLayerSizes(), dst);
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_ann_mlp_set_layer_sizes(jyppx_ocv_ml_model* model, const jyppx_ocv_mat* layer_sizes)
{
    constexpr const char* api_name = "jyppx_ocv_ml_ann_mlp_set_layer_sizes";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, layer_sizes, "layer_sizes");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::ANN_MLP> native = as_ann_mlp_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        native->setLayerSizes(opencv_csharp_native::mat_value(layer_sizes));
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_ann_mlp_get_term_criteria(const jyppx_ocv_ml_model* model, int* type, int* max_count, double* epsilon)
{
    constexpr const char* api_name = "jyppx_ocv_ml_ann_mlp_get_term_criteria";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, type, "type");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, max_count, "max_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_double(api_name, epsilon, "epsilon");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::ANN_MLP> native = as_ann_mlp_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        cv::TermCriteria criteria = native->getTermCriteria();
        *type = criteria.type;
        *max_count = criteria.maxCount;
        *epsilon = criteria.epsilon;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *type = 0; *max_count = 0; *epsilon = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_ann_mlp_set_term_criteria(jyppx_ocv_ml_model* model, int type, int max_count, double epsilon)
{
    constexpr const char* api_name = "jyppx_ocv_ml_ann_mlp_set_term_criteria";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::ANN_MLP> native = as_ann_mlp_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        native->setTermCriteria(cv::TermCriteria(type, max_count, epsilon));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)type; (void)max_count; (void)epsilon;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_ann_mlp_get_weights(const jyppx_ocv_ml_model* model, int layer_index, jyppx_ocv_mat* dst)
{
    constexpr const char* api_name = "jyppx_ocv_ml_ann_mlp_get_weights";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::ANN_MLP> native = as_ann_mlp_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        copy_mat_to_output(native->getWeights(layer_index), dst);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)layer_index;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_ann_mlp_set_anneal_energy_seed(jyppx_ocv_ml_model* model, unsigned long long seed)
{
    constexpr const char* api_name = "jyppx_ocv_ml_ann_mlp_set_anneal_energy_seed";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::ANN_MLP> native = as_ann_mlp_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        native->setAnnealEnergyRNG(cv::RNG(static_cast<uint64_t>(seed)));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)seed;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_dtrees_get_int(const jyppx_ocv_ml_model* model, int property_id, int* value)
{
    constexpr const char* api_name = "jyppx_ocv_ml_dtrees_get_int";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::DTrees> native = as_dtrees_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        switch (property_id)
        {
        case DTREES_INT_MAX_CATEGORIES: *value = native->getMaxCategories(); return OPENCV_CSHARP_STATUS_OK;
        case DTREES_INT_MAX_DEPTH: *value = native->getMaxDepth(); return OPENCV_CSHARP_STATUS_OK;
        case DTREES_INT_MIN_SAMPLE_COUNT: *value = native->getMinSampleCount(); return OPENCV_CSHARP_STATUS_OK;
        case DTREES_INT_CV_FOLDS: *value = native->getCVFolds(); return OPENCV_CSHARP_STATUS_OK;
        case DTREES_INT_USE_SURROGATES: *value = native->getUseSurrogates() ? 1 : 0; return OPENCV_CSHARP_STATUS_OK;
        case DTREES_INT_USE_1SE_RULE: *value = native->getUse1SERule() ? 1 : 0; return OPENCV_CSHARP_STATUS_OK;
        case DTREES_INT_TRUNCATE_PRUNED_TREE: *value = native->getTruncatePrunedTree() ? 1 : 0; return OPENCV_CSHARP_STATUS_OK;
        default: return opencv_csharp_native::set_invalid_argument(api_name, "property_id");
        }
#else
        (void)property_id;
        *value = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_dtrees_set_int(jyppx_ocv_ml_model* model, int property_id, int value)
{
    constexpr const char* api_name = "jyppx_ocv_ml_dtrees_set_int";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::DTrees> native = as_dtrees_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        switch (property_id)
        {
        case DTREES_INT_MAX_CATEGORIES: native->setMaxCategories(value); return OPENCV_CSHARP_STATUS_OK;
        case DTREES_INT_MAX_DEPTH: native->setMaxDepth(value); return OPENCV_CSHARP_STATUS_OK;
        case DTREES_INT_MIN_SAMPLE_COUNT: native->setMinSampleCount(value); return OPENCV_CSHARP_STATUS_OK;
        case DTREES_INT_CV_FOLDS: native->setCVFolds(value); return OPENCV_CSHARP_STATUS_OK;
        case DTREES_INT_USE_SURROGATES: native->setUseSurrogates(value != 0); return OPENCV_CSHARP_STATUS_OK;
        case DTREES_INT_USE_1SE_RULE: native->setUse1SERule(value != 0); return OPENCV_CSHARP_STATUS_OK;
        case DTREES_INT_TRUNCATE_PRUNED_TREE: native->setTruncatePrunedTree(value != 0); return OPENCV_CSHARP_STATUS_OK;
        default: return opencv_csharp_native::set_invalid_argument(api_name, "property_id");
        }
#else
        (void)property_id; (void)value;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_dtrees_get_regression_accuracy(const jyppx_ocv_ml_model* model, float* value)
{
    constexpr const char* api_name = "jyppx_ocv_ml_dtrees_get_regression_accuracy";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_float(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::DTrees> native = as_dtrees_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        *value = native->getRegressionAccuracy();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *value = 0.0F;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_dtrees_set_regression_accuracy(jyppx_ocv_ml_model* model, float value)
{
    constexpr const char* api_name = "jyppx_ocv_ml_dtrees_set_regression_accuracy";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::DTrees> native = as_dtrees_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        native->setRegressionAccuracy(value);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)value;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_dtrees_get_priors(const jyppx_ocv_ml_model* model, jyppx_ocv_mat* dst)
{
    constexpr const char* api_name = "jyppx_ocv_ml_dtrees_get_priors";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::DTrees> native = as_dtrees_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        copy_mat_to_output(native->getPriors(), dst);
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_dtrees_set_priors(jyppx_ocv_ml_model* model, const jyppx_ocv_mat* priors)
{
    constexpr const char* api_name = "jyppx_ocv_ml_dtrees_set_priors";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, priors, "priors");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::DTrees> native = as_dtrees_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        native->setPriors(opencv_csharp_native::mat_value(priors));
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_rtrees_get_int(const jyppx_ocv_ml_model* model, int property_id, int* value)
{
    constexpr const char* api_name = "jyppx_ocv_ml_rtrees_get_int";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::RTrees> native = as_rtrees_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        switch (property_id)
        {
        case RTREES_INT_CALCULATE_VAR_IMPORTANCE: *value = native->getCalculateVarImportance() ? 1 : 0; return OPENCV_CSHARP_STATUS_OK;
        case RTREES_INT_ACTIVE_VAR_COUNT: *value = native->getActiveVarCount(); return OPENCV_CSHARP_STATUS_OK;
        default: return opencv_csharp_native::set_invalid_argument(api_name, "property_id");
        }
#else
        (void)property_id;
        *value = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_rtrees_set_int(jyppx_ocv_ml_model* model, int property_id, int value)
{
    constexpr const char* api_name = "jyppx_ocv_ml_rtrees_set_int";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::RTrees> native = as_rtrees_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        switch (property_id)
        {
        case RTREES_INT_CALCULATE_VAR_IMPORTANCE: native->setCalculateVarImportance(value != 0); return OPENCV_CSHARP_STATUS_OK;
        case RTREES_INT_ACTIVE_VAR_COUNT: native->setActiveVarCount(value); return OPENCV_CSHARP_STATUS_OK;
        default: return opencv_csharp_native::set_invalid_argument(api_name, "property_id");
        }
#else
        (void)property_id; (void)value;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_rtrees_get_term_criteria(const jyppx_ocv_ml_model* model, int* type, int* max_count, double* epsilon)
{
    constexpr const char* api_name = "jyppx_ocv_ml_rtrees_get_term_criteria";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, type, "type");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, max_count, "max_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_double(api_name, epsilon, "epsilon");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::RTrees> native = as_rtrees_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        cv::TermCriteria criteria = native->getTermCriteria();
        *type = criteria.type;
        *max_count = criteria.maxCount;
        *epsilon = criteria.epsilon;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *type = 0; *max_count = 0; *epsilon = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_rtrees_set_term_criteria(jyppx_ocv_ml_model* model, int type, int max_count, double epsilon)
{
    constexpr const char* api_name = "jyppx_ocv_ml_rtrees_set_term_criteria";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::RTrees> native = as_rtrees_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        native->setTermCriteria(cv::TermCriteria(type, max_count, epsilon));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)type; (void)max_count; (void)epsilon;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_rtrees_get_var_importance(const jyppx_ocv_ml_model* model, jyppx_ocv_mat* dst)
{
    constexpr const char* api_name = "jyppx_ocv_ml_rtrees_get_var_importance";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::RTrees> native = as_rtrees_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        copy_mat_to_output(native->getVarImportance(), dst);
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_rtrees_get_votes(
    const jyppx_ocv_ml_model* model,
    const jyppx_ocv_mat* samples,
    jyppx_ocv_mat* results,
    int flags)
{
    constexpr const char* api_name = "jyppx_ocv_ml_rtrees_get_votes";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, samples, "samples");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, results, "results");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::RTrees> native = as_rtrees_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        native->getVotes(
            opencv_csharp_native::mat_value(samples),
            opencv_csharp_native::mat_value(results),
            flags);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)flags;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_rtrees_get_oob_error(const jyppx_ocv_ml_model* model, double* value)
{
    constexpr const char* api_name = "jyppx_ocv_ml_rtrees_get_oob_error";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_double(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::RTrees> native = as_rtrees_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        *value = native->getOOBError();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *value = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_boost_get_int(const jyppx_ocv_ml_model* model, int property_id, int* value)
{
    constexpr const char* api_name = "jyppx_ocv_ml_boost_get_int";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::Boost> native = as_boost_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        switch (property_id)
        {
        case BOOST_INT_TYPE: *value = native->getBoostType(); return OPENCV_CSHARP_STATUS_OK;
        case BOOST_INT_WEAK_COUNT: *value = native->getWeakCount(); return OPENCV_CSHARP_STATUS_OK;
        default: return opencv_csharp_native::set_invalid_argument(api_name, "property_id");
        }
#else
        (void)property_id;
        *value = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_boost_set_int(jyppx_ocv_ml_model* model, int property_id, int value)
{
    constexpr const char* api_name = "jyppx_ocv_ml_boost_set_int";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::Boost> native = as_boost_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        switch (property_id)
        {
        case BOOST_INT_TYPE: native->setBoostType(value); return OPENCV_CSHARP_STATUS_OK;
        case BOOST_INT_WEAK_COUNT: native->setWeakCount(value); return OPENCV_CSHARP_STATUS_OK;
        default: return opencv_csharp_native::set_invalid_argument(api_name, "property_id");
        }
#else
        (void)property_id; (void)value;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_boost_get_weight_trim_rate(const jyppx_ocv_ml_model* model, double* value)
{
    constexpr const char* api_name = "jyppx_ocv_ml_boost_get_weight_trim_rate";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_double(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::Boost> native = as_boost_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        *value = native->getWeightTrimRate();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *value = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_boost_set_weight_trim_rate(jyppx_ocv_ml_model* model, double value)
{
    constexpr const char* api_name = "jyppx_ocv_ml_boost_set_weight_trim_rate";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::Boost> native = as_boost_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        native->setWeightTrimRate(value);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)value;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_em_get_int(const jyppx_ocv_ml_model* model, int property_id, int* value)
{
    constexpr const char* api_name = "jyppx_ocv_ml_em_get_int";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::EM> native = as_em_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        switch (property_id)
        {
        case EM_INT_CLUSTERS_NUMBER: *value = native->getClustersNumber(); return OPENCV_CSHARP_STATUS_OK;
        case EM_INT_COVARIANCE_MATRIX_TYPE: *value = native->getCovarianceMatrixType(); return OPENCV_CSHARP_STATUS_OK;
        default: return opencv_csharp_native::set_invalid_argument(api_name, "property_id");
        }
#else
        (void)property_id;
        *value = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_em_set_int(jyppx_ocv_ml_model* model, int property_id, int value)
{
    constexpr const char* api_name = "jyppx_ocv_ml_em_set_int";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::EM> native = as_em_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        switch (property_id)
        {
        case EM_INT_CLUSTERS_NUMBER: native->setClustersNumber(value); return OPENCV_CSHARP_STATUS_OK;
        case EM_INT_COVARIANCE_MATRIX_TYPE: native->setCovarianceMatrixType(value); return OPENCV_CSHARP_STATUS_OK;
        default: return opencv_csharp_native::set_invalid_argument(api_name, "property_id");
        }
#else
        (void)property_id; (void)value;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_em_get_term_criteria(const jyppx_ocv_ml_model* model, int* type, int* max_count, double* epsilon)
{
    constexpr const char* api_name = "jyppx_ocv_ml_em_get_term_criteria";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, type, "type");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, max_count, "max_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_double(api_name, epsilon, "epsilon");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::EM> native = as_em_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        const cv::TermCriteria criteria = native->getTermCriteria();
        *type = criteria.type;
        *max_count = criteria.maxCount;
        *epsilon = criteria.epsilon;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *type = 0; *max_count = 0; *epsilon = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_em_set_term_criteria(jyppx_ocv_ml_model* model, int type, int max_count, double epsilon)
{
    constexpr const char* api_name = "jyppx_ocv_ml_em_set_term_criteria";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::EM> native = as_em_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        native->setTermCriteria(cv::TermCriteria(type, max_count, epsilon));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)type; (void)max_count; (void)epsilon;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_em_get_weights(const jyppx_ocv_ml_model* model, jyppx_ocv_mat* dst)
{
    constexpr const char* api_name = "jyppx_ocv_ml_em_get_weights";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::EM> native = as_em_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        copy_mat_to_output(native->getWeights(), dst);
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_em_get_means(const jyppx_ocv_ml_model* model, jyppx_ocv_mat* dst)
{
    constexpr const char* api_name = "jyppx_ocv_ml_em_get_means";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::EM> native = as_em_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        copy_mat_to_output(native->getMeans(), dst);
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_em_get_covariances_count(const jyppx_ocv_ml_model* model, int* count)
{
    constexpr const char* api_name = "jyppx_ocv_ml_em_get_covariances_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, count, "count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::EM> native = as_em_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        std::vector<cv::Mat> covariances;
        native->getCovs(covariances);
        status = checked_int_size(api_name, covariances.size(), "count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *count = static_cast<int>(covariances.size());
        return OPENCV_CSHARP_STATUS_OK;
#else
        *count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_em_get_covariances_fill(
    const jyppx_ocv_ml_model* model,
    jyppx_ocv_mat* const* covariances,
    int covariance_capacity,
    int* count)
{
    constexpr const char* api_name = "jyppx_ocv_ml_em_get_covariances_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, count, "count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (covariance_capacity < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "covariance_capacity");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::EM> native = as_em_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        std::vector<cv::Mat> native_covariances;
        native->getCovs(native_covariances);
        status = checked_int_size(api_name, native_covariances.size(), "count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *count = static_cast<int>(native_covariances.size());
        if (covariance_capacity < *count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "covariance_capacity");
        }
        status = validate_output_mat_array(api_name, covariances, *count, "covariances");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        for (int i = 0; i < *count; ++i)
        {
            copy_mat_to_output(native_covariances[static_cast<size_t>(i)], covariances[i]);
        }
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)covariances; (void)covariance_capacity;
        *count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_em_predict2(
    const jyppx_ocv_ml_model* model,
    const jyppx_ocv_mat* sample,
    jyppx_ocv_mat* probabilities,
    double* log_likelihood,
    int* label)
{
    constexpr const char* api_name = "jyppx_ocv_ml_em_predict2";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, sample, "sample");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_double(api_name, log_likelihood, "log_likelihood");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, label, "label");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::EM> native = as_em_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        const cv::Vec2d prediction = native->predict2(
            opencv_csharp_native::mat_value(sample),
            optional_output_array(probabilities));
        *log_likelihood = prediction[0];
        *label = static_cast<int>(prediction[1]);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)probabilities;
        *log_likelihood = 0.0; *label = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_em_train_em(
    jyppx_ocv_ml_model* model,
    const jyppx_ocv_mat* samples,
    jyppx_ocv_mat* log_likelihoods,
    jyppx_ocv_mat* labels,
    jyppx_ocv_mat* probabilities,
    int* result)
{
    constexpr const char* api_name = "jyppx_ocv_ml_em_train_em";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, samples, "samples");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, result, "result");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::EM> native = as_em_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        *result = native->trainEM(
            opencv_csharp_native::mat_value(samples),
            optional_output_array(log_likelihoods),
            optional_output_array(labels),
            optional_output_array(probabilities)) ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)log_likelihoods; (void)labels; (void)probabilities;
        *result = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_em_train_e(
    jyppx_ocv_ml_model* model,
    const jyppx_ocv_mat* samples,
    const jyppx_ocv_mat* initial_means,
    const jyppx_ocv_mat* const* initial_covariances,
    int initial_covariance_count,
    const jyppx_ocv_mat* initial_weights,
    jyppx_ocv_mat* log_likelihoods,
    jyppx_ocv_mat* labels,
    jyppx_ocv_mat* probabilities,
    int* result)
{
    constexpr const char* api_name = "jyppx_ocv_ml_em_train_e";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, samples, "samples");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, initial_means, "initial_means");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat_array(api_name, initial_covariances, initial_covariance_count, "initial_covariances");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, result, "result");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::EM> native = as_em_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        std::vector<cv::Mat> native_covariances;
        native_covariances.reserve(static_cast<size_t>(initial_covariance_count));
        for (int i = 0; i < initial_covariance_count; ++i)
        {
            native_covariances.push_back(opencv_csharp_native::mat_value(initial_covariances[i]));
        }

        bool trained;
        if (native_covariances.empty())
        {
            trained = native->trainE(
                opencv_csharp_native::mat_value(samples),
                opencv_csharp_native::mat_value(initial_means),
                cv::noArray(),
                optional_input_array(initial_weights),
                optional_output_array(log_likelihoods),
                optional_output_array(labels),
                optional_output_array(probabilities));
        }
        else
        {
            trained = native->trainE(
                opencv_csharp_native::mat_value(samples),
                opencv_csharp_native::mat_value(initial_means),
                native_covariances,
                optional_input_array(initial_weights),
                optional_output_array(log_likelihoods),
                optional_output_array(labels),
                optional_output_array(probabilities));
        }
        *result = trained ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)initial_weights; (void)log_likelihoods; (void)labels; (void)probabilities;
        *result = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ml_em_train_m(
    jyppx_ocv_ml_model* model,
    const jyppx_ocv_mat* samples,
    const jyppx_ocv_mat* initial_probabilities,
    jyppx_ocv_mat* log_likelihoods,
    jyppx_ocv_mat* labels,
    jyppx_ocv_mat* probabilities,
    int* result)
{
    constexpr const char* api_name = "jyppx_ocv_ml_em_train_m";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, samples, "samples");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, initial_probabilities, "initial_probabilities");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, result, "result");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ML)
        cv::Ptr<cv::ml::EM> native = as_em_ptr(model);
        if (native.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "model"); }
        *result = native->trainM(
            opencv_csharp_native::mat_value(samples),
            opencv_csharp_native::mat_value(initial_probabilities),
            optional_output_array(log_likelihoods),
            optional_output_array(labels),
            optional_output_array(probabilities)) ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)log_likelihoods; (void)labels; (void)probabilities;
        *result = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}


#include "open_cv_sharp/xphoto/xphoto.h"

#include "../core/mat_handle.h"
#include "../error_state.h"
#include "xphoto_handles.h"

#include <new>

namespace
{
    constexpr int SIMPLE_WB_PROPERTY_INPUT_MIN = 0;
    constexpr int SIMPLE_WB_PROPERTY_INPUT_MAX = 1;
    constexpr int SIMPLE_WB_PROPERTY_OUTPUT_MIN = 2;
    constexpr int SIMPLE_WB_PROPERTY_OUTPUT_MAX = 3;
    constexpr int SIMPLE_WB_PROPERTY_P = 4;

    constexpr int LEARNING_BASED_WB_PROPERTY_RANGE_MAX_VAL = 0;
    constexpr int LEARNING_BASED_WB_PROPERTY_HIST_BIN_NUM = 1;

    int validate_mat(const char* api_name, const jyppx_ocv_mat* mat, const char* argument_name)
    {
        return mat == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_white_balancer(const char* api_name, const jyppx_ocv_xphoto_white_balancer* white_balancer)
    {
        return white_balancer == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "white_balancer")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_output_float(const char* api_name, const float* value, const char* argument_name)
    {
        return value == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_output_int(const char* api_name, const int* value, const char* argument_name)
    {
        return value == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XPHOTO)
    int create_white_balancer_handle(
        const char* api_name,
        const cv::Ptr<cv::xphoto::WhiteBalancer>& native,
        jyppx_ocv_xphoto_white_balancer** white_balancer)
    {
        if (white_balancer == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "white_balancer");
        }

        *white_balancer = nullptr;
        jyppx_ocv_xphoto_white_balancer* created = new (std::nothrow) jyppx_ocv_xphoto_white_balancer();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = native;
        *white_balancer = created;
        return OPENCV_CSHARP_STATUS_OK;
    }

    cv::xphoto::SimpleWB* as_simple_wb(jyppx_ocv_xphoto_white_balancer* white_balancer)
    {
        return dynamic_cast<cv::xphoto::SimpleWB*>(white_balancer->value.get());
    }

    const cv::xphoto::SimpleWB* as_simple_wb(const jyppx_ocv_xphoto_white_balancer* white_balancer)
    {
        return dynamic_cast<const cv::xphoto::SimpleWB*>(white_balancer->value.get());
    }

    cv::xphoto::GrayworldWB* as_grayworld_wb(jyppx_ocv_xphoto_white_balancer* white_balancer)
    {
        return dynamic_cast<cv::xphoto::GrayworldWB*>(white_balancer->value.get());
    }

    const cv::xphoto::GrayworldWB* as_grayworld_wb(const jyppx_ocv_xphoto_white_balancer* white_balancer)
    {
        return dynamic_cast<const cv::xphoto::GrayworldWB*>(white_balancer->value.get());
    }

    cv::xphoto::LearningBasedWB* as_learning_based_wb(jyppx_ocv_xphoto_white_balancer* white_balancer)
    {
        return dynamic_cast<cv::xphoto::LearningBasedWB*>(white_balancer->value.get());
    }

    const cv::xphoto::LearningBasedWB* as_learning_based_wb(const jyppx_ocv_xphoto_white_balancer* white_balancer)
    {
        return dynamic_cast<const cv::xphoto::LearningBasedWB*>(white_balancer->value.get());
    }
#endif
}

int jyppx_ocv_xphoto_simple_wb_create(jyppx_ocv_xphoto_white_balancer** white_balancer)
{
    constexpr const char* api_name = "jyppx_ocv_xphoto_simple_wb_create";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XPHOTO)
        return create_white_balancer_handle(api_name, cv::xphoto::createSimpleWB(), white_balancer);
#else
        if (white_balancer != nullptr) { *white_balancer = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_xphoto_grayworld_wb_create(jyppx_ocv_xphoto_white_balancer** white_balancer)
{
    constexpr const char* api_name = "jyppx_ocv_xphoto_grayworld_wb_create";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XPHOTO)
        return create_white_balancer_handle(api_name, cv::xphoto::createGrayworldWB(), white_balancer);
#else
        if (white_balancer != nullptr) { *white_balancer = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_xphoto_learning_based_wb_create(const char* model_path, jyppx_ocv_xphoto_white_balancer** white_balancer)
{
    constexpr const char* api_name = "jyppx_ocv_xphoto_learning_based_wb_create";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XPHOTO)
        return create_white_balancer_handle(api_name, cv::xphoto::createLearningBasedWB(model_path == nullptr ? cv::String() : cv::String(model_path)), white_balancer);
#else
        (void)model_path;
        if (white_balancer != nullptr) { *white_balancer = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_xphoto_white_balancer_release_handle(jyppx_ocv_xphoto_white_balancer* white_balancer)
{
    delete white_balancer;
}

int jyppx_ocv_xphoto_white_balancer_balance_white(jyppx_ocv_xphoto_white_balancer* white_balancer, const jyppx_ocv_mat* src, jyppx_ocv_mat* dst)
{
    constexpr const char* api_name = "jyppx_ocv_xphoto_white_balancer_balance_white";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_white_balancer(api_name, white_balancer);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XPHOTO)
        white_balancer->value->balanceWhite(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst));
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

int jyppx_ocv_xphoto_simple_wb_get_property(const jyppx_ocv_xphoto_white_balancer* white_balancer, int property_id, float* value)
{
    constexpr const char* api_name = "jyppx_ocv_xphoto_simple_wb_get_property";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_white_balancer(api_name, white_balancer);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_float(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XPHOTO)
        const cv::xphoto::SimpleWB* simple = as_simple_wb(white_balancer);
        if (simple == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "white_balancer"); }

        switch (property_id)
        {
        case SIMPLE_WB_PROPERTY_INPUT_MIN: *value = simple->getInputMin(); return OPENCV_CSHARP_STATUS_OK;
        case SIMPLE_WB_PROPERTY_INPUT_MAX: *value = simple->getInputMax(); return OPENCV_CSHARP_STATUS_OK;
        case SIMPLE_WB_PROPERTY_OUTPUT_MIN: *value = simple->getOutputMin(); return OPENCV_CSHARP_STATUS_OK;
        case SIMPLE_WB_PROPERTY_OUTPUT_MAX: *value = simple->getOutputMax(); return OPENCV_CSHARP_STATUS_OK;
        case SIMPLE_WB_PROPERTY_P: *value = simple->getP(); return OPENCV_CSHARP_STATUS_OK;
        default: return opencv_csharp_native::set_invalid_argument(api_name, "property_id");
        }
#else
        (void)property_id;
        *value = 0.0F;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_xphoto_simple_wb_set_property(jyppx_ocv_xphoto_white_balancer* white_balancer, int property_id, float value)
{
    constexpr const char* api_name = "jyppx_ocv_xphoto_simple_wb_set_property";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_white_balancer(api_name, white_balancer);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XPHOTO)
        cv::xphoto::SimpleWB* simple = as_simple_wb(white_balancer);
        if (simple == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "white_balancer"); }

        switch (property_id)
        {
        case SIMPLE_WB_PROPERTY_INPUT_MIN: simple->setInputMin(value); return OPENCV_CSHARP_STATUS_OK;
        case SIMPLE_WB_PROPERTY_INPUT_MAX: simple->setInputMax(value); return OPENCV_CSHARP_STATUS_OK;
        case SIMPLE_WB_PROPERTY_OUTPUT_MIN: simple->setOutputMin(value); return OPENCV_CSHARP_STATUS_OK;
        case SIMPLE_WB_PROPERTY_OUTPUT_MAX: simple->setOutputMax(value); return OPENCV_CSHARP_STATUS_OK;
        case SIMPLE_WB_PROPERTY_P: simple->setP(value); return OPENCV_CSHARP_STATUS_OK;
        default: return opencv_csharp_native::set_invalid_argument(api_name, "property_id");
        }
#else
        (void)property_id;
        (void)value;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_xphoto_grayworld_wb_get_saturation_threshold(const jyppx_ocv_xphoto_white_balancer* white_balancer, float* value)
{
    constexpr const char* api_name = "jyppx_ocv_xphoto_grayworld_wb_get_saturation_threshold";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_white_balancer(api_name, white_balancer);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_float(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XPHOTO)
        const cv::xphoto::GrayworldWB* grayworld = as_grayworld_wb(white_balancer);
        if (grayworld == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "white_balancer"); }

        *value = grayworld->getSaturationThreshold();
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

int jyppx_ocv_xphoto_grayworld_wb_set_saturation_threshold(jyppx_ocv_xphoto_white_balancer* white_balancer, float value)
{
    constexpr const char* api_name = "jyppx_ocv_xphoto_grayworld_wb_set_saturation_threshold";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_white_balancer(api_name, white_balancer);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XPHOTO)
        cv::xphoto::GrayworldWB* grayworld = as_grayworld_wb(white_balancer);
        if (grayworld == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "white_balancer"); }

        grayworld->setSaturationThreshold(value);
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

int jyppx_ocv_xphoto_learning_based_wb_get_int_property(const jyppx_ocv_xphoto_white_balancer* white_balancer, int property_id, int* value)
{
    constexpr const char* api_name = "jyppx_ocv_xphoto_learning_based_wb_get_int_property";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_white_balancer(api_name, white_balancer);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XPHOTO)
        const cv::xphoto::LearningBasedWB* learning = as_learning_based_wb(white_balancer);
        if (learning == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "white_balancer"); }

        switch (property_id)
        {
        case LEARNING_BASED_WB_PROPERTY_RANGE_MAX_VAL: *value = learning->getRangeMaxVal(); return OPENCV_CSHARP_STATUS_OK;
        case LEARNING_BASED_WB_PROPERTY_HIST_BIN_NUM: *value = learning->getHistBinNum(); return OPENCV_CSHARP_STATUS_OK;
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

int jyppx_ocv_xphoto_learning_based_wb_set_int_property(jyppx_ocv_xphoto_white_balancer* white_balancer, int property_id, int value)
{
    constexpr const char* api_name = "jyppx_ocv_xphoto_learning_based_wb_set_int_property";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_white_balancer(api_name, white_balancer);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XPHOTO)
        cv::xphoto::LearningBasedWB* learning = as_learning_based_wb(white_balancer);
        if (learning == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "white_balancer"); }

        switch (property_id)
        {
        case LEARNING_BASED_WB_PROPERTY_RANGE_MAX_VAL: learning->setRangeMaxVal(value); return OPENCV_CSHARP_STATUS_OK;
        case LEARNING_BASED_WB_PROPERTY_HIST_BIN_NUM: learning->setHistBinNum(value); return OPENCV_CSHARP_STATUS_OK;
        default: return opencv_csharp_native::set_invalid_argument(api_name, "property_id");
        }
#else
        (void)property_id;
        (void)value;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_xphoto_learning_based_wb_get_saturation_threshold(const jyppx_ocv_xphoto_white_balancer* white_balancer, float* value)
{
    constexpr const char* api_name = "jyppx_ocv_xphoto_learning_based_wb_get_saturation_threshold";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_white_balancer(api_name, white_balancer);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_float(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XPHOTO)
        const cv::xphoto::LearningBasedWB* learning = as_learning_based_wb(white_balancer);
        if (learning == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "white_balancer"); }

        *value = learning->getSaturationThreshold();
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

int jyppx_ocv_xphoto_learning_based_wb_set_saturation_threshold(jyppx_ocv_xphoto_white_balancer* white_balancer, float value)
{
    constexpr const char* api_name = "jyppx_ocv_xphoto_learning_based_wb_set_saturation_threshold";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_white_balancer(api_name, white_balancer);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XPHOTO)
        cv::xphoto::LearningBasedWB* learning = as_learning_based_wb(white_balancer);
        if (learning == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "white_balancer"); }

        learning->setSaturationThreshold(value);
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

int jyppx_ocv_xphoto_learning_based_wb_extract_simple_features(jyppx_ocv_xphoto_white_balancer* white_balancer, const jyppx_ocv_mat* src, jyppx_ocv_mat* dst)
{
    constexpr const char* api_name = "jyppx_ocv_xphoto_learning_based_wb_extract_simple_features";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_white_balancer(api_name, white_balancer);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XPHOTO)
        cv::xphoto::LearningBasedWB* learning = as_learning_based_wb(white_balancer);
        if (learning == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "white_balancer"); }

        learning->extractSimpleFeatures(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst));
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

int jyppx_ocv_xphoto_apply_channel_gains(const jyppx_ocv_mat* src, jyppx_ocv_mat* dst, float gain_b, float gain_g, float gain_r)
{
    constexpr const char* api_name = "jyppx_ocv_xphoto_apply_channel_gains";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XPHOTO)
        cv::xphoto::applyChannelGains(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), gain_b, gain_g, gain_r);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)gain_b;
        (void)gain_g;
        (void)gain_r;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_xphoto_dct_denoising(const jyppx_ocv_mat* src, jyppx_ocv_mat* dst, double sigma, int psize)
{
    constexpr const char* api_name = "jyppx_ocv_xphoto_dct_denoising";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XPHOTO)
        cv::xphoto::dctDenoising(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), sigma, psize);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)sigma;
        (void)psize;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_xphoto_bm3d_denoising(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    float h,
    int template_window_size,
    int search_window_size,
    int block_matching_step1,
    int block_matching_step2,
    int group_size,
    int sliding_step,
    float beta,
    int norm_type,
    int step,
    int transform_type)
{
    constexpr const char* api_name = "jyppx_ocv_xphoto_bm3d_denoising";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XPHOTO)
        cv::xphoto::bm3dDenoising(
            opencv_csharp_native::mat_value(src),
            opencv_csharp_native::mat_value(dst),
            h,
            template_window_size,
            search_window_size,
            block_matching_step1,
            block_matching_step2,
            group_size,
            sliding_step,
            beta,
            norm_type,
            step,
            transform_type);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)h;
        (void)template_window_size;
        (void)search_window_size;
        (void)block_matching_step1;
        (void)block_matching_step2;
        (void)group_size;
        (void)sliding_step;
        (void)beta;
        (void)norm_type;
        (void)step;
        (void)transform_type;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_xphoto_bm3d_denoising_steps(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst_step1,
    jyppx_ocv_mat* dst_step2,
    float h,
    int template_window_size,
    int search_window_size,
    int block_matching_step1,
    int block_matching_step2,
    int group_size,
    int sliding_step,
    float beta,
    int norm_type,
    int step,
    int transform_type)
{
    constexpr const char* api_name = "jyppx_ocv_xphoto_bm3d_denoising_steps";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst_step1, "dst_step1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst_step2, "dst_step2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XPHOTO)
        cv::xphoto::bm3dDenoising(
            opencv_csharp_native::mat_value(src),
            opencv_csharp_native::mat_value(dst_step1),
            opencv_csharp_native::mat_value(dst_step2),
            h,
            template_window_size,
            search_window_size,
            block_matching_step1,
            block_matching_step2,
            group_size,
            sliding_step,
            beta,
            norm_type,
            step,
            transform_type);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)h;
        (void)template_window_size;
        (void)search_window_size;
        (void)block_matching_step1;
        (void)block_matching_step2;
        (void)group_size;
        (void)sliding_step;
        (void)beta;
        (void)norm_type;
        (void)step;
        (void)transform_type;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_xphoto_oil_painting(const jyppx_ocv_mat* src, jyppx_ocv_mat* dst, int size, int dyn_ratio, int code, int use_code)
{
    constexpr const char* api_name = "jyppx_ocv_xphoto_oil_painting";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XPHOTO)
        if (use_code != 0)
        {
            cv::xphoto::oilPainting(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), size, dyn_ratio, code);
        }
        else
        {
            cv::xphoto::oilPainting(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), size, dyn_ratio);
        }

        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)size;
        (void)dyn_ratio;
        (void)code;
        (void)use_code;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}



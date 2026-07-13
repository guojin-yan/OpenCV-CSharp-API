#include "open_cv_sharp/xstereo/xstereo.h"

#include "../core/mat_handle.h"
#include "../error_state.h"
#include "xstereo_handles.h"

#include <cstring>
#include <new>
#include <string>
#include <vector>

namespace
{
    int validate_mat(const char* api_name, const jyppx_ocv_mat* mat, const char* argument_name)
    {
        return mat == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_output_pointer(const char* api_name, const void* value, const char* argument_name)
    {
        return value == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_positive_size(const char* api_name, int width, int height)
    {
        return width > 0 && height > 0
            ? OPENCV_CSHARP_STATUS_OK
            : opencv_csharp_native::set_invalid_argument(api_name, "size");
    }

    std::string to_string_or_empty(const unsigned char* value)
    {
        return value == nullptr ? std::string() : std::string(reinterpret_cast<const char*>(value));
    }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XSTEREO)
    int validate_bm(const char* api_name, const jyppx_ocv_xstereo_binary_bm* matcher)
    {
        return matcher == nullptr || matcher->value.empty()
            ? opencv_csharp_native::set_invalid_argument(api_name, "matcher")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_sgbm(const char* api_name, const jyppx_ocv_xstereo_binary_sgbm* matcher)
    {
        return matcher == nullptr || matcher->value.empty()
            ? opencv_csharp_native::set_invalid_argument(api_name, "matcher")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_quasi_dense(const char* api_name, const jyppx_ocv_xstereo_quasi_dense_stereo* stereo)
    {
        return stereo == nullptr || stereo->value.empty()
            ? opencv_csharp_native::set_invalid_argument(api_name, "stereo")
            : OPENCV_CSHARP_STATUS_OK;
    }

    cv::InputArray optional_input_array(const jyppx_ocv_mat* mat)
    {
        return mat == nullptr ? cv::noArray() : cv::InputArray(opencv_csharp_native::mat_value(mat));
    }

    void fill_match(const cv::stereo::MatchQuasiDense& source, jyppx_ocv_xstereo_match_quasi_dense* destination)
    {
        destination->p0_x = source.p0.x;
        destination->p0_y = source.p0.y;
        destination->p1_x = source.p1.x;
        destination->p1_y = source.p1.y;
        destination->corr = source.corr;
    }

    void fill_parameters(
        const cv::stereo::PropagationParameters& source,
        jyppx_ocv_xstereo_propagation_parameters* destination)
    {
        destination->corr_win_size_x = source.corrWinSizeX;
        destination->corr_win_size_y = source.corrWinSizeY;
        destination->border_x = source.borderX;
        destination->border_y = source.borderY;
        destination->correlation_threshold = source.correlationThreshold;
        destination->textrure_threshold = source.textrureThreshold;
        destination->neighborhood_size = source.neighborhoodSize;
        destination->disparity_gradient = source.disparityGradient;
        destination->lk_template_size = source.lkTemplateSize;
        destination->lk_pyr_lvl = source.lkPyrLvl;
        destination->lk_term_param1 = source.lkTermParam1;
        destination->lk_term_param2 = source.lkTermParam2;
        destination->gft_quality_thres = source.gftQualityThres;
        destination->gft_min_seperation_dist = source.gftMinSeperationDist;
        destination->gft_max_num_features = source.gftMaxNumFeatures;
    }

    cv::stereo::PropagationParameters to_native_parameters(const jyppx_ocv_xstereo_propagation_parameters& source)
    {
        cv::stereo::PropagationParameters result;
        result.corrWinSizeX = source.corr_win_size_x;
        result.corrWinSizeY = source.corr_win_size_y;
        result.borderX = source.border_x;
        result.borderY = source.border_y;
        result.correlationThreshold = source.correlation_threshold;
        result.textrureThreshold = source.textrure_threshold;
        result.neighborhoodSize = source.neighborhood_size;
        result.disparityGradient = source.disparity_gradient;
        result.lkTemplateSize = source.lk_template_size;
        result.lkPyrLvl = source.lk_pyr_lvl;
        result.lkTermParam1 = source.lk_term_param1;
        result.lkTermParam2 = source.lk_term_param2;
        result.gftQualityThres = source.gft_quality_thres;
        result.gftMinSeperationDist = source.gft_min_seperation_dist;
        result.gftMaxNumFeatures = source.gft_max_num_features;
        return result;
    }

    int fill_matches_result(
        const char* api_name,
        const std::vector<cv::stereo::MatchQuasiDense>& native_matches,
        jyppx_ocv_xstereo_match_quasi_dense* matches,
        int match_capacity,
        int* count)
    {
        int status = validate_output_pointer(api_name, count, "count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (match_capacity < 0 || (match_capacity > 0 && matches == nullptr))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "matches");
        }

        *count = static_cast<int>(native_matches.size());
        if (match_capacity < *count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "match_capacity");
        }

        for (int i = 0; i < *count; ++i)
        {
            fill_match(native_matches[static_cast<size_t>(i)], &matches[i]);
        }

        return OPENCV_CSHARP_STATUS_OK;
    }
#else
    int validate_bm(const char*, const jyppx_ocv_xstereo_binary_bm*) { return OPENCV_CSHARP_STATUS_OK; }
    int validate_sgbm(const char*, const jyppx_ocv_xstereo_binary_sgbm*) { return OPENCV_CSHARP_STATUS_OK; }
    int validate_quasi_dense(const char*, const jyppx_ocv_xstereo_quasi_dense_stereo*) { return OPENCV_CSHARP_STATUS_OK; }
#endif
}

int jyppx_ocv_xstereo_census_transform(const jyppx_ocv_mat* image, int kernel_size, jyppx_ocv_mat* dist, int type)
{
    constexpr const char* api_name = "jyppx_ocv_xstereo_census_transform";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dist, "dist");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XSTEREO)
        cv::stereo::censusTransform(opencv_csharp_native::mat_value(image), kernel_size, opencv_csharp_native::mat_value(dist), type);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)kernel_size; (void)type;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_xstereo_census_transform_pair(
    const jyppx_ocv_mat* image1,
    const jyppx_ocv_mat* image2,
    int kernel_size,
    jyppx_ocv_mat* dist1,
    jyppx_ocv_mat* dist2,
    int type)
{
    constexpr const char* api_name = "jyppx_ocv_xstereo_census_transform_pair";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, image1, "image1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image2, "image2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dist1, "dist1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dist2, "dist2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XSTEREO)
        cv::stereo::censusTransform(
            opencv_csharp_native::mat_value(image1),
            opencv_csharp_native::mat_value(image2),
            kernel_size,
            opencv_csharp_native::mat_value(dist1),
            opencv_csharp_native::mat_value(dist2),
            type);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)kernel_size; (void)type;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_xstereo_modified_census_transform(
    const jyppx_ocv_mat* image,
    int kernel_size,
    jyppx_ocv_mat* dist,
    int type,
    int t,
    const jyppx_ocv_mat* integral_image)
{
    constexpr const char* api_name = "jyppx_ocv_xstereo_modified_census_transform";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dist, "dist");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XSTEREO)
        cv::stereo::modifiedCensusTransform(
            opencv_csharp_native::mat_value(image),
            kernel_size,
            opencv_csharp_native::mat_value(dist),
            type,
            t,
            optional_input_array(integral_image).getMat());
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)kernel_size; (void)type; (void)t; (void)integral_image;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_xstereo_modified_census_transform_pair(
    const jyppx_ocv_mat* image1,
    const jyppx_ocv_mat* image2,
    int kernel_size,
    jyppx_ocv_mat* dist1,
    jyppx_ocv_mat* dist2,
    int type,
    int t,
    const jyppx_ocv_mat* integral_image1,
    const jyppx_ocv_mat* integral_image2)
{
    constexpr const char* api_name = "jyppx_ocv_xstereo_modified_census_transform_pair";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, image1, "image1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image2, "image2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dist1, "dist1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dist2, "dist2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XSTEREO)
        cv::stereo::modifiedCensusTransform(
            opencv_csharp_native::mat_value(image1),
            opencv_csharp_native::mat_value(image2),
            kernel_size,
            opencv_csharp_native::mat_value(dist1),
            opencv_csharp_native::mat_value(dist2),
            type,
            t,
            optional_input_array(integral_image1).getMat(),
            optional_input_array(integral_image2).getMat());
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)kernel_size; (void)type; (void)t; (void)integral_image1; (void)integral_image2;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_xstereo_symetric_census_transform(
    const jyppx_ocv_mat* image,
    int kernel_size,
    jyppx_ocv_mat* dist,
    int type)
{
    constexpr const char* api_name = "jyppx_ocv_xstereo_symetric_census_transform";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dist, "dist");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XSTEREO)
        cv::stereo::symetricCensusTransform(opencv_csharp_native::mat_value(image), kernel_size, opencv_csharp_native::mat_value(dist), type);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)kernel_size; (void)type;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_xstereo_symetric_census_transform_pair(
    const jyppx_ocv_mat* image1,
    const jyppx_ocv_mat* image2,
    int kernel_size,
    jyppx_ocv_mat* dist1,
    jyppx_ocv_mat* dist2,
    int type)
{
    constexpr const char* api_name = "jyppx_ocv_xstereo_symetric_census_transform_pair";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, image1, "image1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image2, "image2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dist1, "dist1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dist2, "dist2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XSTEREO)
        cv::stereo::symetricCensusTransform(
            opencv_csharp_native::mat_value(image1),
            opencv_csharp_native::mat_value(image2),
            kernel_size,
            opencv_csharp_native::mat_value(dist1),
            opencv_csharp_native::mat_value(dist2),
            type);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)kernel_size; (void)type;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_xstereo_star_census_transform(const jyppx_ocv_mat* image, int kernel_size, jyppx_ocv_mat* dist)
{
    constexpr const char* api_name = "jyppx_ocv_xstereo_star_census_transform";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dist, "dist");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XSTEREO)
        cv::stereo::starCensusTransform(opencv_csharp_native::mat_value(image), kernel_size, opencv_csharp_native::mat_value(dist));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)kernel_size;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_xstereo_star_census_transform_pair(
    const jyppx_ocv_mat* image1,
    const jyppx_ocv_mat* image2,
    int kernel_size,
    jyppx_ocv_mat* dist1,
    jyppx_ocv_mat* dist2)
{
    constexpr const char* api_name = "jyppx_ocv_xstereo_star_census_transform_pair";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, image1, "image1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image2, "image2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dist1, "dist1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dist2, "dist2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XSTEREO)
        cv::stereo::starCensusTransform(
            opencv_csharp_native::mat_value(image1),
            opencv_csharp_native::mat_value(image2),
            kernel_size,
            opencv_csharp_native::mat_value(dist1),
            opencv_csharp_native::mat_value(dist2));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)kernel_size;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_xstereo_binary_bm_create(int num_disparities, int block_size, jyppx_ocv_xstereo_binary_bm** matcher)
{
    constexpr const char* api_name = "jyppx_ocv_xstereo_binary_bm_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_pointer(api_name, matcher, "matcher");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *matcher = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XSTEREO)
        jyppx_ocv_xstereo_binary_bm* created = new (std::nothrow) jyppx_ocv_xstereo_binary_bm();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = cv::stereo::StereoBinaryBM::create(num_disparities, block_size);
        if (created->value.empty())
        {
            delete created;
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        *matcher = created;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)num_disparities; (void)block_size;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_xstereo_binary_bm_release(jyppx_ocv_xstereo_binary_bm* matcher)
{
    delete matcher;
}

int jyppx_ocv_xstereo_binary_bm_compute(
    jyppx_ocv_xstereo_binary_bm* matcher,
    const jyppx_ocv_mat* left,
    const jyppx_ocv_mat* right,
    jyppx_ocv_mat* disparity)
{
    constexpr const char* api_name = "jyppx_ocv_xstereo_binary_bm_compute";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, left, "left");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, right, "right");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, disparity, "disparity");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XSTEREO)
        status = validate_bm(api_name, matcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        matcher->value->compute(opencv_csharp_native::mat_value(left), opencv_csharp_native::mat_value(right), opencv_csharp_native::mat_value(disparity));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)matcher;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_xstereo_binary_sgbm_create(
    int min_disparity,
    int num_disparities,
    int block_size,
    int p1,
    int p2,
    int disp12_max_diff,
    int pre_filter_cap,
    int uniqueness_ratio,
    int speckle_window_size,
    int speckle_range,
    int mode,
    jyppx_ocv_xstereo_binary_sgbm** matcher)
{
    constexpr const char* api_name = "jyppx_ocv_xstereo_binary_sgbm_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_pointer(api_name, matcher, "matcher");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *matcher = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XSTEREO)
        jyppx_ocv_xstereo_binary_sgbm* created = new (std::nothrow) jyppx_ocv_xstereo_binary_sgbm();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = cv::stereo::StereoBinarySGBM::create(
            min_disparity,
            num_disparities,
            block_size,
            p1,
            p2,
            disp12_max_diff,
            pre_filter_cap,
            uniqueness_ratio,
            speckle_window_size,
            speckle_range,
            mode);
        if (created->value.empty())
        {
            delete created;
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        *matcher = created;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)min_disparity; (void)num_disparities; (void)block_size; (void)p1; (void)p2; (void)disp12_max_diff;
        (void)pre_filter_cap; (void)uniqueness_ratio; (void)speckle_window_size; (void)speckle_range; (void)mode;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_xstereo_binary_sgbm_release(jyppx_ocv_xstereo_binary_sgbm* matcher)
{
    delete matcher;
}

int jyppx_ocv_xstereo_binary_sgbm_compute(
    jyppx_ocv_xstereo_binary_sgbm* matcher,
    const jyppx_ocv_mat* left,
    const jyppx_ocv_mat* right,
    jyppx_ocv_mat* disparity)
{
    constexpr const char* api_name = "jyppx_ocv_xstereo_binary_sgbm_compute";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, left, "left");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, right, "right");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, disparity, "disparity");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XSTEREO)
        status = validate_sgbm(api_name, matcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        matcher->value->compute(opencv_csharp_native::mat_value(left), opencv_csharp_native::mat_value(right), opencv_csharp_native::mat_value(disparity));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)matcher;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XSTEREO)
#define OCV_CSHARP_XSTEREO_BM_GET_INT(native_name, method_name) \
    int native_name(const jyppx_ocv_xstereo_binary_bm* matcher, int* value) \
    { \
        constexpr const char* api_name = #native_name; \
        try \
        { \
            opencv_csharp_native::clear_last_error(); \
            int status = validate_output_pointer(api_name, value, "value"); \
            if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
            *value = 0; \
            status = validate_bm(api_name, matcher); \
            if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
            *value = matcher->value->method_name(); \
            return OPENCV_CSHARP_STATUS_OK; \
        } \
        catch (...) \
        { \
            return opencv_csharp_native::translate_current_exception(api_name); \
        } \
    }
#define OCV_CSHARP_XSTEREO_BM_SET_INT(native_name, method_name) \
    int native_name(jyppx_ocv_xstereo_binary_bm* matcher, int value) \
    { \
        constexpr const char* api_name = #native_name; \
        try \
        { \
            opencv_csharp_native::clear_last_error(); \
            int status = validate_bm(api_name, matcher); \
            if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
            matcher->value->method_name(value); \
            return OPENCV_CSHARP_STATUS_OK; \
        } \
        catch (...) \
        { \
            return opencv_csharp_native::translate_current_exception(api_name); \
        } \
    }
#define OCV_CSHARP_XSTEREO_SGBM_GET_INT(native_name, method_name) \
    int native_name(const jyppx_ocv_xstereo_binary_sgbm* matcher, int* value) \
    { \
        constexpr const char* api_name = #native_name; \
        try \
        { \
            opencv_csharp_native::clear_last_error(); \
            int status = validate_output_pointer(api_name, value, "value"); \
            if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
            *value = 0; \
            status = validate_sgbm(api_name, matcher); \
            if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
            *value = matcher->value->method_name(); \
            return OPENCV_CSHARP_STATUS_OK; \
        } \
        catch (...) \
        { \
            return opencv_csharp_native::translate_current_exception(api_name); \
        } \
    }
#define OCV_CSHARP_XSTEREO_SGBM_SET_INT(native_name, method_name) \
    int native_name(jyppx_ocv_xstereo_binary_sgbm* matcher, int value) \
    { \
        constexpr const char* api_name = #native_name; \
        try \
        { \
            opencv_csharp_native::clear_last_error(); \
            int status = validate_sgbm(api_name, matcher); \
            if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
            matcher->value->method_name(value); \
            return OPENCV_CSHARP_STATUS_OK; \
        } \
        catch (...) \
        { \
            return opencv_csharp_native::translate_current_exception(api_name); \
        } \
    }
#else
#define OCV_CSHARP_XSTEREO_BM_GET_INT(native_name, method_name) \
    int native_name(const jyppx_ocv_xstereo_binary_bm*, int* value) \
    { \
        if (value != nullptr) { *value = 0; } \
        return opencv_csharp_native::set_not_linked(#native_name); \
    }
#define OCV_CSHARP_XSTEREO_BM_SET_INT(native_name, method_name) \
    int native_name(jyppx_ocv_xstereo_binary_bm*, int) \
    { \
        return opencv_csharp_native::set_not_linked(#native_name); \
    }
#define OCV_CSHARP_XSTEREO_SGBM_GET_INT(native_name, method_name) \
    int native_name(const jyppx_ocv_xstereo_binary_sgbm*, int* value) \
    { \
        if (value != nullptr) { *value = 0; } \
        return opencv_csharp_native::set_not_linked(#native_name); \
    }
#define OCV_CSHARP_XSTEREO_SGBM_SET_INT(native_name, method_name) \
    int native_name(jyppx_ocv_xstereo_binary_sgbm*, int) \
    { \
        return opencv_csharp_native::set_not_linked(#native_name); \
    }
#endif

OCV_CSHARP_XSTEREO_BM_GET_INT(jyppx_ocv_xstereo_binary_bm_get_min_disparity, getMinDisparity)
OCV_CSHARP_XSTEREO_BM_SET_INT(jyppx_ocv_xstereo_binary_bm_set_min_disparity, setMinDisparity)
OCV_CSHARP_XSTEREO_BM_GET_INT(jyppx_ocv_xstereo_binary_bm_get_num_disparities, getNumDisparities)
OCV_CSHARP_XSTEREO_BM_SET_INT(jyppx_ocv_xstereo_binary_bm_set_num_disparities, setNumDisparities)
OCV_CSHARP_XSTEREO_BM_GET_INT(jyppx_ocv_xstereo_binary_bm_get_block_size, getBlockSize)
OCV_CSHARP_XSTEREO_BM_SET_INT(jyppx_ocv_xstereo_binary_bm_set_block_size, setBlockSize)
OCV_CSHARP_XSTEREO_BM_GET_INT(jyppx_ocv_xstereo_binary_bm_get_speckle_window_size, getSpeckleWindowSize)
OCV_CSHARP_XSTEREO_BM_SET_INT(jyppx_ocv_xstereo_binary_bm_set_speckle_window_size, setSpeckleWindowSize)
OCV_CSHARP_XSTEREO_BM_GET_INT(jyppx_ocv_xstereo_binary_bm_get_speckle_range, getSpeckleRange)
OCV_CSHARP_XSTEREO_BM_SET_INT(jyppx_ocv_xstereo_binary_bm_set_speckle_range, setSpeckleRange)
OCV_CSHARP_XSTEREO_BM_GET_INT(jyppx_ocv_xstereo_binary_bm_get_disp12_max_diff, getDisp12MaxDiff)
OCV_CSHARP_XSTEREO_BM_SET_INT(jyppx_ocv_xstereo_binary_bm_set_disp12_max_diff, setDisp12MaxDiff)
OCV_CSHARP_XSTEREO_BM_GET_INT(jyppx_ocv_xstereo_binary_bm_get_pre_filter_type, getPreFilterType)
OCV_CSHARP_XSTEREO_BM_SET_INT(jyppx_ocv_xstereo_binary_bm_set_pre_filter_type, setPreFilterType)
OCV_CSHARP_XSTEREO_BM_GET_INT(jyppx_ocv_xstereo_binary_bm_get_pre_filter_size, getPreFilterSize)
OCV_CSHARP_XSTEREO_BM_SET_INT(jyppx_ocv_xstereo_binary_bm_set_pre_filter_size, setPreFilterSize)
OCV_CSHARP_XSTEREO_BM_GET_INT(jyppx_ocv_xstereo_binary_bm_get_pre_filter_cap, getPreFilterCap)
OCV_CSHARP_XSTEREO_BM_SET_INT(jyppx_ocv_xstereo_binary_bm_set_pre_filter_cap, setPreFilterCap)
OCV_CSHARP_XSTEREO_BM_GET_INT(jyppx_ocv_xstereo_binary_bm_get_texture_threshold, getTextureThreshold)
OCV_CSHARP_XSTEREO_BM_SET_INT(jyppx_ocv_xstereo_binary_bm_set_texture_threshold, setTextureThreshold)
OCV_CSHARP_XSTEREO_BM_GET_INT(jyppx_ocv_xstereo_binary_bm_get_uniqueness_ratio, getUniquenessRatio)
OCV_CSHARP_XSTEREO_BM_SET_INT(jyppx_ocv_xstereo_binary_bm_set_uniqueness_ratio, setUniquenessRatio)
OCV_CSHARP_XSTEREO_BM_GET_INT(jyppx_ocv_xstereo_binary_bm_get_smaller_block_size, getSmallerBlockSize)
OCV_CSHARP_XSTEREO_BM_SET_INT(jyppx_ocv_xstereo_binary_bm_set_smaller_block_size, setSmallerBlockSize)
OCV_CSHARP_XSTEREO_BM_GET_INT(jyppx_ocv_xstereo_binary_bm_get_scalle_factor, getScalleFactor)
OCV_CSHARP_XSTEREO_BM_SET_INT(jyppx_ocv_xstereo_binary_bm_set_scalle_factor, setScalleFactor)
OCV_CSHARP_XSTEREO_BM_GET_INT(jyppx_ocv_xstereo_binary_bm_get_spekle_removal_technique, getSpekleRemovalTechnique)
OCV_CSHARP_XSTEREO_BM_SET_INT(jyppx_ocv_xstereo_binary_bm_set_spekle_removal_technique, setSpekleRemovalTechnique)
OCV_CSHARP_XSTEREO_BM_GET_INT(jyppx_ocv_xstereo_binary_bm_get_use_prefilter, getUsePrefilter)
OCV_CSHARP_XSTEREO_BM_SET_INT(jyppx_ocv_xstereo_binary_bm_set_use_prefilter, setUsePrefilter)
OCV_CSHARP_XSTEREO_BM_GET_INT(jyppx_ocv_xstereo_binary_bm_get_binary_kernel_type, getBinaryKernelType)
OCV_CSHARP_XSTEREO_BM_SET_INT(jyppx_ocv_xstereo_binary_bm_set_binary_kernel_type, setBinaryKernelType)
OCV_CSHARP_XSTEREO_BM_GET_INT(jyppx_ocv_xstereo_binary_bm_get_agregation_window_size, getAgregationWindowSize)
OCV_CSHARP_XSTEREO_BM_SET_INT(jyppx_ocv_xstereo_binary_bm_set_agregation_window_size, setAgregationWindowSize)

OCV_CSHARP_XSTEREO_SGBM_GET_INT(jyppx_ocv_xstereo_binary_sgbm_get_min_disparity, getMinDisparity)
OCV_CSHARP_XSTEREO_SGBM_SET_INT(jyppx_ocv_xstereo_binary_sgbm_set_min_disparity, setMinDisparity)
OCV_CSHARP_XSTEREO_SGBM_GET_INT(jyppx_ocv_xstereo_binary_sgbm_get_num_disparities, getNumDisparities)
OCV_CSHARP_XSTEREO_SGBM_SET_INT(jyppx_ocv_xstereo_binary_sgbm_set_num_disparities, setNumDisparities)
OCV_CSHARP_XSTEREO_SGBM_GET_INT(jyppx_ocv_xstereo_binary_sgbm_get_block_size, getBlockSize)
OCV_CSHARP_XSTEREO_SGBM_SET_INT(jyppx_ocv_xstereo_binary_sgbm_set_block_size, setBlockSize)
OCV_CSHARP_XSTEREO_SGBM_GET_INT(jyppx_ocv_xstereo_binary_sgbm_get_speckle_window_size, getSpeckleWindowSize)
OCV_CSHARP_XSTEREO_SGBM_SET_INT(jyppx_ocv_xstereo_binary_sgbm_set_speckle_window_size, setSpeckleWindowSize)
OCV_CSHARP_XSTEREO_SGBM_GET_INT(jyppx_ocv_xstereo_binary_sgbm_get_speckle_range, getSpeckleRange)
OCV_CSHARP_XSTEREO_SGBM_SET_INT(jyppx_ocv_xstereo_binary_sgbm_set_speckle_range, setSpeckleRange)
OCV_CSHARP_XSTEREO_SGBM_GET_INT(jyppx_ocv_xstereo_binary_sgbm_get_disp12_max_diff, getDisp12MaxDiff)
OCV_CSHARP_XSTEREO_SGBM_SET_INT(jyppx_ocv_xstereo_binary_sgbm_set_disp12_max_diff, setDisp12MaxDiff)
OCV_CSHARP_XSTEREO_SGBM_GET_INT(jyppx_ocv_xstereo_binary_sgbm_get_pre_filter_cap, getPreFilterCap)
OCV_CSHARP_XSTEREO_SGBM_SET_INT(jyppx_ocv_xstereo_binary_sgbm_set_pre_filter_cap, setPreFilterCap)
OCV_CSHARP_XSTEREO_SGBM_GET_INT(jyppx_ocv_xstereo_binary_sgbm_get_uniqueness_ratio, getUniquenessRatio)
OCV_CSHARP_XSTEREO_SGBM_SET_INT(jyppx_ocv_xstereo_binary_sgbm_set_uniqueness_ratio, setUniquenessRatio)
OCV_CSHARP_XSTEREO_SGBM_GET_INT(jyppx_ocv_xstereo_binary_sgbm_get_p1, getP1)
OCV_CSHARP_XSTEREO_SGBM_SET_INT(jyppx_ocv_xstereo_binary_sgbm_set_p1, setP1)
OCV_CSHARP_XSTEREO_SGBM_GET_INT(jyppx_ocv_xstereo_binary_sgbm_get_p2, getP2)
OCV_CSHARP_XSTEREO_SGBM_SET_INT(jyppx_ocv_xstereo_binary_sgbm_set_p2, setP2)
OCV_CSHARP_XSTEREO_SGBM_GET_INT(jyppx_ocv_xstereo_binary_sgbm_get_mode, getMode)
OCV_CSHARP_XSTEREO_SGBM_SET_INT(jyppx_ocv_xstereo_binary_sgbm_set_mode, setMode)
OCV_CSHARP_XSTEREO_SGBM_GET_INT(jyppx_ocv_xstereo_binary_sgbm_get_spekle_removal_technique, getSpekleRemovalTechnique)
OCV_CSHARP_XSTEREO_SGBM_SET_INT(jyppx_ocv_xstereo_binary_sgbm_set_spekle_removal_technique, setSpekleRemovalTechnique)
OCV_CSHARP_XSTEREO_SGBM_GET_INT(jyppx_ocv_xstereo_binary_sgbm_get_binary_kernel_type, getBinaryKernelType)
OCV_CSHARP_XSTEREO_SGBM_SET_INT(jyppx_ocv_xstereo_binary_sgbm_set_binary_kernel_type, setBinaryKernelType)
OCV_CSHARP_XSTEREO_SGBM_GET_INT(jyppx_ocv_xstereo_binary_sgbm_get_sub_pixel_interpolation_method, getSubPixelInterpolationMethod)
OCV_CSHARP_XSTEREO_SGBM_SET_INT(jyppx_ocv_xstereo_binary_sgbm_set_sub_pixel_interpolation_method, setSubPixelInterpolationMethod)

#undef OCV_CSHARP_XSTEREO_BM_GET_INT
#undef OCV_CSHARP_XSTEREO_BM_SET_INT
#undef OCV_CSHARP_XSTEREO_SGBM_GET_INT
#undef OCV_CSHARP_XSTEREO_SGBM_SET_INT

int jyppx_ocv_xstereo_quasi_dense_create(
    int width,
    int height,
    const unsigned char* parameter_file_path,
    jyppx_ocv_xstereo_quasi_dense_stereo** stereo)
{
    constexpr const char* api_name = "jyppx_ocv_xstereo_quasi_dense_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_pointer(api_name, stereo, "stereo");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *stereo = nullptr;
        status = validate_positive_size(api_name, width, height);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XSTEREO)
        jyppx_ocv_xstereo_quasi_dense_stereo* created = new (std::nothrow) jyppx_ocv_xstereo_quasi_dense_stereo();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = cv::stereo::QuasiDenseStereo::create(cv::Size(width, height), to_string_or_empty(parameter_file_path));
        if (created->value.empty())
        {
            delete created;
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        *stereo = created;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)parameter_file_path;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_xstereo_quasi_dense_release(jyppx_ocv_xstereo_quasi_dense_stereo* stereo)
{
    delete stereo;
}

int jyppx_ocv_xstereo_quasi_dense_load_parameters(
    jyppx_ocv_xstereo_quasi_dense_stereo* stereo,
    const unsigned char* parameter_file_path,
    int* result)
{
    constexpr const char* api_name = "jyppx_ocv_xstereo_quasi_dense_load_parameters";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_pointer(api_name, result, "result");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *result = 0;
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XSTEREO)
        status = validate_quasi_dense(api_name, stereo);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *result = stereo->value->loadParameters(to_string_or_empty(parameter_file_path));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)stereo; (void)parameter_file_path;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_xstereo_quasi_dense_save_parameters(
    jyppx_ocv_xstereo_quasi_dense_stereo* stereo,
    const unsigned char* parameter_file_path,
    int* result)
{
    constexpr const char* api_name = "jyppx_ocv_xstereo_quasi_dense_save_parameters";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_pointer(api_name, result, "result");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *result = 0;
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XSTEREO)
        status = validate_quasi_dense(api_name, stereo);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *result = stereo->value->saveParameters(to_string_or_empty(parameter_file_path));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)stereo; (void)parameter_file_path;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_xstereo_quasi_dense_process(
    jyppx_ocv_xstereo_quasi_dense_stereo* stereo,
    const jyppx_ocv_mat* left,
    const jyppx_ocv_mat* right)
{
    constexpr const char* api_name = "jyppx_ocv_xstereo_quasi_dense_process";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, left, "left");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, right, "right");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XSTEREO)
        status = validate_quasi_dense(api_name, stereo);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        stereo->value->process(opencv_csharp_native::mat_value(left), opencv_csharp_native::mat_value(right));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)stereo;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XSTEREO)
#define OCV_CSHARP_XSTEREO_QD_COUNT(native_name, method_name) \
    int native_name(jyppx_ocv_xstereo_quasi_dense_stereo* stereo, int* count) \
    { \
        constexpr const char* api_name = #native_name; \
        try \
        { \
            opencv_csharp_native::clear_last_error(); \
            int status = validate_output_pointer(api_name, count, "count"); \
            if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
            *count = 0; \
            status = validate_quasi_dense(api_name, stereo); \
            if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
            std::vector<cv::stereo::MatchQuasiDense> matches; \
            stereo->value->method_name(matches); \
            *count = static_cast<int>(matches.size()); \
            return OPENCV_CSHARP_STATUS_OK; \
        } \
        catch (...) \
        { \
            return opencv_csharp_native::translate_current_exception(api_name); \
        } \
    }
#define OCV_CSHARP_XSTEREO_QD_FILL(native_name, method_name) \
    int native_name(jyppx_ocv_xstereo_quasi_dense_stereo* stereo, jyppx_ocv_xstereo_match_quasi_dense* matches, int match_capacity, int* count) \
    { \
        constexpr const char* api_name = #native_name; \
        try \
        { \
            opencv_csharp_native::clear_last_error(); \
            int status = validate_quasi_dense(api_name, stereo); \
            if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
            std::vector<cv::stereo::MatchQuasiDense> native_matches; \
            stereo->value->method_name(native_matches); \
            return fill_matches_result(api_name, native_matches, matches, match_capacity, count); \
        } \
        catch (...) \
        { \
            return opencv_csharp_native::translate_current_exception(api_name); \
        } \
    }
#else
#define OCV_CSHARP_XSTEREO_QD_COUNT(native_name, method_name) \
    int native_name(jyppx_ocv_xstereo_quasi_dense_stereo*, int* count) \
    { \
        if (count != nullptr) { *count = 0; } \
        return opencv_csharp_native::set_not_linked(#native_name); \
    }
#define OCV_CSHARP_XSTEREO_QD_FILL(native_name, method_name) \
    int native_name(jyppx_ocv_xstereo_quasi_dense_stereo*, jyppx_ocv_xstereo_match_quasi_dense*, int, int* count) \
    { \
        if (count != nullptr) { *count = 0; } \
        return opencv_csharp_native::set_not_linked(#native_name); \
    }
#endif

OCV_CSHARP_XSTEREO_QD_COUNT(jyppx_ocv_xstereo_quasi_dense_get_sparse_matches_count, getSparseMatches)
OCV_CSHARP_XSTEREO_QD_FILL(jyppx_ocv_xstereo_quasi_dense_get_sparse_matches_fill, getSparseMatches)
OCV_CSHARP_XSTEREO_QD_COUNT(jyppx_ocv_xstereo_quasi_dense_get_dense_matches_count, getDenseMatches)
OCV_CSHARP_XSTEREO_QD_FILL(jyppx_ocv_xstereo_quasi_dense_get_dense_matches_fill, getDenseMatches)

#undef OCV_CSHARP_XSTEREO_QD_COUNT
#undef OCV_CSHARP_XSTEREO_QD_FILL

int jyppx_ocv_xstereo_quasi_dense_get_match(
    jyppx_ocv_xstereo_quasi_dense_stereo* stereo,
    int x,
    int y,
    float* match_x,
    float* match_y)
{
    constexpr const char* api_name = "jyppx_ocv_xstereo_quasi_dense_get_match";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_pointer(api_name, match_x, "match_x");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_pointer(api_name, match_y, "match_y");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *match_x = 0.0F;
        *match_y = 0.0F;
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XSTEREO)
        status = validate_quasi_dense(api_name, stereo);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        cv::Point2f match = stereo->value->getMatch(x, y);
        *match_x = match.x;
        *match_y = match.y;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)stereo; (void)x; (void)y;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_xstereo_quasi_dense_get_disparity(
    jyppx_ocv_xstereo_quasi_dense_stereo* stereo,
    jyppx_ocv_mat* disparity)
{
    constexpr const char* api_name = "jyppx_ocv_xstereo_quasi_dense_get_disparity";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, disparity, "disparity");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XSTEREO)
        status = validate_quasi_dense(api_name, stereo);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        opencv_csharp_native::mat_value(disparity) = stereo->value->getDisparity();
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)stereo;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_xstereo_quasi_dense_get_parameters(
    const jyppx_ocv_xstereo_quasi_dense_stereo* stereo,
    jyppx_ocv_xstereo_propagation_parameters* parameters)
{
    constexpr const char* api_name = "jyppx_ocv_xstereo_quasi_dense_get_parameters";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_pointer(api_name, parameters, "parameters");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XSTEREO)
        status = validate_quasi_dense(api_name, stereo);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        fill_parameters(stereo->value->Param, parameters);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)stereo;
        std::memset(parameters, 0, sizeof(*parameters));
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_xstereo_quasi_dense_set_parameters(
    jyppx_ocv_xstereo_quasi_dense_stereo* stereo,
    const jyppx_ocv_xstereo_propagation_parameters* parameters)
{
    constexpr const char* api_name = "jyppx_ocv_xstereo_quasi_dense_set_parameters";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_pointer(api_name, parameters, "parameters");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XSTEREO)
        status = validate_quasi_dense(api_name, stereo);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        stereo->value->Param = to_native_parameters(*parameters);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)stereo;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}


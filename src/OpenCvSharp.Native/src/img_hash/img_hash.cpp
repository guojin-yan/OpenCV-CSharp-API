#include "open_cv_sharp/img_hash/img_hash.h"

#include "../core/mat_handle.h"
#include "../error_state.h"
#include "img_hash_handles.h"

#include <new>

namespace
{
    constexpr int IMG_HASH_KIND_AVERAGE = 1;
    constexpr int IMG_HASH_KIND_PHASH = 2;
    constexpr int IMG_HASH_KIND_BLOCK_MEAN = 3;
    constexpr int IMG_HASH_KIND_COLOR_MOMENT = 4;
    constexpr int IMG_HASH_KIND_MARR_HILDRETH = 5;
    constexpr int IMG_HASH_KIND_RADIAL_VARIANCE = 6;

    int validate_mat(const char* api_name, const jyppx_ocv_mat* mat, const char* argument_name)
    {
        return mat == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_hash(const char* api_name, const jyppx_ocv_img_hash* hash)
    {
        return hash == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "hash")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_output_int(const char* api_name, const int* value, const char* argument_name)
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

    int validate_output_float(const char* api_name, const float* value, const char* argument_name)
    {
        return value == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_IMG_HASH)
    int create_hash_handle(const char* api_name, const cv::Ptr<cv::img_hash::ImgHashBase>& native, int kind, jyppx_ocv_img_hash** hash)
    {
        if (hash == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "hash");
        }

        *hash = nullptr;
        jyppx_ocv_img_hash* created = new (std::nothrow) jyppx_ocv_img_hash();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = native;
        created->kind = kind;
        *hash = created;
        return OPENCV_CSHARP_STATUS_OK;
    }

    cv::img_hash::BlockMeanHash* as_block_mean(jyppx_ocv_img_hash* hash)
    {
        return dynamic_cast<cv::img_hash::BlockMeanHash*>(hash->value.get());
    }

    const cv::img_hash::BlockMeanHash* as_block_mean(const jyppx_ocv_img_hash* hash)
    {
        return dynamic_cast<const cv::img_hash::BlockMeanHash*>(hash->value.get());
    }

    cv::img_hash::MarrHildrethHash* as_marr_hildreth(jyppx_ocv_img_hash* hash)
    {
        return dynamic_cast<cv::img_hash::MarrHildrethHash*>(hash->value.get());
    }

    const cv::img_hash::MarrHildrethHash* as_marr_hildreth(const jyppx_ocv_img_hash* hash)
    {
        return dynamic_cast<const cv::img_hash::MarrHildrethHash*>(hash->value.get());
    }

    cv::img_hash::RadialVarianceHash* as_radial_variance(jyppx_ocv_img_hash* hash)
    {
        return dynamic_cast<cv::img_hash::RadialVarianceHash*>(hash->value.get());
    }

    const cv::img_hash::RadialVarianceHash* as_radial_variance(const jyppx_ocv_img_hash* hash)
    {
        return dynamic_cast<const cv::img_hash::RadialVarianceHash*>(hash->value.get());
    }
#endif
}

int jyppx_ocv_img_hash_average_create(jyppx_ocv_img_hash** hash)
{
    constexpr const char* api_name = "jyppx_ocv_img_hash_average_create";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_IMG_HASH)
        return create_hash_handle(api_name, cv::img_hash::AverageHash::create(), IMG_HASH_KIND_AVERAGE, hash);
#else
        if (hash != nullptr) { *hash = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_img_hash_phash_create(jyppx_ocv_img_hash** hash)
{
    constexpr const char* api_name = "jyppx_ocv_img_hash_phash_create";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_IMG_HASH)
        return create_hash_handle(api_name, cv::img_hash::PHash::create(), IMG_HASH_KIND_PHASH, hash);
#else
        if (hash != nullptr) { *hash = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_img_hash_block_mean_create(int mode, jyppx_ocv_img_hash** hash)
{
    constexpr const char* api_name = "jyppx_ocv_img_hash_block_mean_create";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_IMG_HASH)
        return create_hash_handle(api_name, cv::img_hash::BlockMeanHash::create(mode), IMG_HASH_KIND_BLOCK_MEAN, hash);
#else
        (void)mode;
        if (hash != nullptr) { *hash = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_img_hash_color_moment_create(jyppx_ocv_img_hash** hash)
{
    constexpr const char* api_name = "jyppx_ocv_img_hash_color_moment_create";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_IMG_HASH)
        return create_hash_handle(api_name, cv::img_hash::ColorMomentHash::create(), IMG_HASH_KIND_COLOR_MOMENT, hash);
#else
        if (hash != nullptr) { *hash = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_img_hash_marr_hildreth_create(float alpha, float scale, jyppx_ocv_img_hash** hash)
{
    constexpr const char* api_name = "jyppx_ocv_img_hash_marr_hildreth_create";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_IMG_HASH)
        return create_hash_handle(api_name, cv::img_hash::MarrHildrethHash::create(alpha, scale), IMG_HASH_KIND_MARR_HILDRETH, hash);
#else
        (void)alpha; (void)scale;
        if (hash != nullptr) { *hash = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_img_hash_radial_variance_create(double sigma, int num_of_angle_line, jyppx_ocv_img_hash** hash)
{
    constexpr const char* api_name = "jyppx_ocv_img_hash_radial_variance_create";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_IMG_HASH)
        return create_hash_handle(api_name, cv::img_hash::RadialVarianceHash::create(sigma, num_of_angle_line), IMG_HASH_KIND_RADIAL_VARIANCE, hash);
#else
        (void)sigma; (void)num_of_angle_line;
        if (hash != nullptr) { *hash = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_img_hash_release_handle(jyppx_ocv_img_hash* hash)
{
    delete hash;
}

int jyppx_ocv_img_hash_compute(jyppx_ocv_img_hash* hash, const jyppx_ocv_mat* input, jyppx_ocv_mat* output)
{
    constexpr const char* api_name = "jyppx_ocv_img_hash_compute";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_hash(api_name, hash);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, input, "input");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, output, "output");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_IMG_HASH)
        hash->value->compute(opencv_csharp_native::mat_value(input), opencv_csharp_native::mat_value(output));
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

int jyppx_ocv_img_hash_compare(const jyppx_ocv_img_hash* hash, const jyppx_ocv_mat* hash_one, const jyppx_ocv_mat* hash_two, double* value)
{
    constexpr const char* api_name = "jyppx_ocv_img_hash_compare";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_hash(api_name, hash);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, hash_one, "hash_one");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, hash_two, "hash_two");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_double(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_IMG_HASH)
        *value = hash->value->compare(opencv_csharp_native::mat_value(hash_one), opencv_csharp_native::mat_value(hash_two));
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

int jyppx_ocv_img_hash_block_mean_set_mode(jyppx_ocv_img_hash* hash, int mode)
{
    constexpr const char* api_name = "jyppx_ocv_img_hash_block_mean_set_mode";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_hash(api_name, hash);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_IMG_HASH)
        cv::img_hash::BlockMeanHash* native = as_block_mean(hash);
        if (native == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "hash"); }
        native->setMode(mode);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)mode;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_img_hash_block_mean_get_mean_count(const jyppx_ocv_img_hash* hash, int* count)
{
    constexpr const char* api_name = "jyppx_ocv_img_hash_block_mean_get_mean_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_hash(api_name, hash);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, count, "count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_IMG_HASH)
        const cv::img_hash::BlockMeanHash* native = as_block_mean(hash);
        if (native == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "hash"); }
        *count = static_cast<int>(native->getMean().size());
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

int jyppx_ocv_img_hash_block_mean_get_mean_fill(const jyppx_ocv_img_hash* hash, double* values, int value_capacity, int* count)
{
    constexpr const char* api_name = "jyppx_ocv_img_hash_block_mean_get_mean_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_hash(api_name, hash);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, count, "count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_IMG_HASH)
        const cv::img_hash::BlockMeanHash* native = as_block_mean(hash);
        if (native == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "hash"); }
        std::vector<double> mean = native->getMean();
        *count = static_cast<int>(mean.size());
        if (values == nullptr || value_capacity < *count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "values");
        }

        for (int i = 0; i < *count; ++i)
        {
            values[i] = mean[static_cast<size_t>(i)];
        }

        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)values; (void)value_capacity;
        *count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_img_hash_marr_hildreth_get(const jyppx_ocv_img_hash* hash, float* alpha, float* scale)
{
    constexpr const char* api_name = "jyppx_ocv_img_hash_marr_hildreth_get";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_hash(api_name, hash);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_float(api_name, alpha, "alpha");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_float(api_name, scale, "scale");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_IMG_HASH)
        const cv::img_hash::MarrHildrethHash* native = as_marr_hildreth(hash);
        if (native == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "hash"); }
        *alpha = native->getAlpha();
        *scale = native->getScale();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *alpha = 0.0F;
        *scale = 0.0F;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_img_hash_marr_hildreth_set_kernel_param(jyppx_ocv_img_hash* hash, float alpha, float scale)
{
    constexpr const char* api_name = "jyppx_ocv_img_hash_marr_hildreth_set_kernel_param";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_hash(api_name, hash);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_IMG_HASH)
        cv::img_hash::MarrHildrethHash* native = as_marr_hildreth(hash);
        if (native == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "hash"); }
        native->setKernelParam(alpha, scale);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)alpha; (void)scale;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_img_hash_radial_variance_get(const jyppx_ocv_img_hash* hash, double* sigma, int* num_of_angle_line)
{
    constexpr const char* api_name = "jyppx_ocv_img_hash_radial_variance_get";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_hash(api_name, hash);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_double(api_name, sigma, "sigma");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, num_of_angle_line, "num_of_angle_line");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_IMG_HASH)
        const cv::img_hash::RadialVarianceHash* native = as_radial_variance(hash);
        if (native == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "hash"); }
        *sigma = native->getSigma();
        *num_of_angle_line = native->getNumOfAngleLine();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *sigma = 0.0;
        *num_of_angle_line = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_img_hash_radial_variance_set_sigma(jyppx_ocv_img_hash* hash, double sigma)
{
    constexpr const char* api_name = "jyppx_ocv_img_hash_radial_variance_set_sigma";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_hash(api_name, hash);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_IMG_HASH)
        cv::img_hash::RadialVarianceHash* native = as_radial_variance(hash);
        if (native == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "hash"); }
        native->setSigma(sigma);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)sigma;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_img_hash_radial_variance_set_num_of_angle_line(jyppx_ocv_img_hash* hash, int num_of_angle_line)
{
    constexpr const char* api_name = "jyppx_ocv_img_hash_radial_variance_set_num_of_angle_line";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_hash(api_name, hash);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_IMG_HASH)
        cv::img_hash::RadialVarianceHash* native = as_radial_variance(hash);
        if (native == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "hash"); }
        native->setNumOfAngleLine(num_of_angle_line);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)num_of_angle_line;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_img_hash_average_compute_static(const jyppx_ocv_mat* input, jyppx_ocv_mat* output)
{
    constexpr const char* api_name = "jyppx_ocv_img_hash_average_compute_static";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, input, "input");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, output, "output");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_IMG_HASH)
        cv::img_hash::averageHash(opencv_csharp_native::mat_value(input), opencv_csharp_native::mat_value(output));
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

int jyppx_ocv_img_hash_phash_compute_static(const jyppx_ocv_mat* input, jyppx_ocv_mat* output)
{
    constexpr const char* api_name = "jyppx_ocv_img_hash_phash_compute_static";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, input, "input");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, output, "output");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_IMG_HASH)
        cv::img_hash::pHash(opencv_csharp_native::mat_value(input), opencv_csharp_native::mat_value(output));
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

int jyppx_ocv_img_hash_block_mean_compute_static(const jyppx_ocv_mat* input, jyppx_ocv_mat* output, int mode)
{
    constexpr const char* api_name = "jyppx_ocv_img_hash_block_mean_compute_static";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, input, "input");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, output, "output");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_IMG_HASH)
        cv::img_hash::blockMeanHash(opencv_csharp_native::mat_value(input), opencv_csharp_native::mat_value(output), mode);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)mode;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_img_hash_color_moment_compute_static(const jyppx_ocv_mat* input, jyppx_ocv_mat* output)
{
    constexpr const char* api_name = "jyppx_ocv_img_hash_color_moment_compute_static";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, input, "input");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, output, "output");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_IMG_HASH)
        cv::img_hash::colorMomentHash(opencv_csharp_native::mat_value(input), opencv_csharp_native::mat_value(output));
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

int jyppx_ocv_img_hash_marr_hildreth_compute_static(const jyppx_ocv_mat* input, jyppx_ocv_mat* output, float alpha, float scale)
{
    constexpr const char* api_name = "jyppx_ocv_img_hash_marr_hildreth_compute_static";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, input, "input");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, output, "output");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_IMG_HASH)
        cv::img_hash::marrHildrethHash(opencv_csharp_native::mat_value(input), opencv_csharp_native::mat_value(output), alpha, scale);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)alpha; (void)scale;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_img_hash_radial_variance_compute_static(const jyppx_ocv_mat* input, jyppx_ocv_mat* output, double sigma, int num_of_angle_line)
{
    constexpr const char* api_name = "jyppx_ocv_img_hash_radial_variance_compute_static";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, input, "input");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, output, "output");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_IMG_HASH)
        cv::img_hash::radialVarianceHash(opencv_csharp_native::mat_value(input), opencv_csharp_native::mat_value(output), sigma, num_of_angle_line);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)sigma; (void)num_of_angle_line;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}



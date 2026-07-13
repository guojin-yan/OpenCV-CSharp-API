#include "open_cv_sharp/bioinspired/bioinspired.h"

#include "../core/mat_handle.h"
#include "../error_state.h"
#include "bioinspired_handles.h"

#include <cstring>
#include <new>
#include <string>

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

    int fill_string_result(const char* api_name, const std::string& text, unsigned char* buffer, int buffer_capacity, int* written)
    {
        int status = validate_output_pointer(api_name, written, "written");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *written = 0;
        if (buffer_capacity < 0 || (buffer_capacity > 0 && buffer == nullptr))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "buffer");
        }

        const int byte_count = static_cast<int>(text.size());
        if (buffer_capacity < byte_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "buffer_capacity");
        }

        if (byte_count > 0)
        {
            std::memcpy(buffer, text.data(), static_cast<size_t>(byte_count));
        }

        *written = byte_count;
        return OPENCV_CSHARP_STATUS_OK;
    }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BIOINSPIRED)
    int validate_retina(const char* api_name, const jyppx_ocv_bioinspired_retina* retina)
    {
        return retina == nullptr || retina->value.empty()
            ? opencv_csharp_native::set_invalid_argument(api_name, "retina")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_tone_mapping(const char* api_name, const jyppx_ocv_bioinspired_retina_fast_tone_mapping* tone_mapping)
    {
        return tone_mapping == nullptr || tone_mapping->value.empty()
            ? opencv_csharp_native::set_invalid_argument(api_name, "tone_mapping")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_segmentation(const char* api_name, const jyppx_ocv_bioinspired_transient_areas_segmentation_module* segmentation)
    {
        return segmentation == nullptr || segmentation->value.empty()
            ? opencv_csharp_native::set_invalid_argument(api_name, "segmentation")
            : OPENCV_CSHARP_STATUS_OK;
    }

    void fill_parvo_parameters(
        const cv::bioinspired::RetinaParameters::OPLandIplParvoParameters& source,
        jyppx_ocv_bioinspired_retina_parvo_parameters* destination)
    {
        destination->color_mode = source.colorMode ? 1 : 0;
        destination->normalise_output = source.normaliseOutput ? 1 : 0;
        destination->photoreceptors_local_adaptation_sensitivity = source.photoreceptorsLocalAdaptationSensitivity;
        destination->photoreceptors_temporal_constant = source.photoreceptorsTemporalConstant;
        destination->photoreceptors_spatial_constant = source.photoreceptorsSpatialConstant;
        destination->horizontal_cells_gain = source.horizontalCellsGain;
        destination->hcells_temporal_constant = source.hcellsTemporalConstant;
        destination->hcells_spatial_constant = source.hcellsSpatialConstant;
        destination->ganglion_cells_sensitivity = source.ganglionCellsSensitivity;
    }

    void fill_magno_parameters(
        const cv::bioinspired::RetinaParameters::IplMagnoParameters& source,
        jyppx_ocv_bioinspired_retina_magno_parameters* destination)
    {
        destination->normalise_output = source.normaliseOutput ? 1 : 0;
        destination->parasol_cells_beta = source.parasolCells_beta;
        destination->parasol_cells_tau = source.parasolCells_tau;
        destination->parasol_cells_k = source.parasolCells_k;
        destination->amacrin_cells_temporal_cut_frequency = source.amacrinCellsTemporalCutFrequency;
        destination->v0_compression_parameter = source.V0CompressionParameter;
        destination->local_adapt_integration_tau = source.localAdaptintegration_tau;
        destination->local_adapt_integration_k = source.localAdaptintegration_k;
    }

    cv::bioinspired::RetinaParameters::OPLandIplParvoParameters to_native_parvo(
        const jyppx_ocv_bioinspired_retina_parvo_parameters& source)
    {
        cv::bioinspired::RetinaParameters::OPLandIplParvoParameters result;
        result.colorMode = source.color_mode != 0;
        result.normaliseOutput = source.normalise_output != 0;
        result.photoreceptorsLocalAdaptationSensitivity = source.photoreceptors_local_adaptation_sensitivity;
        result.photoreceptorsTemporalConstant = source.photoreceptors_temporal_constant;
        result.photoreceptorsSpatialConstant = source.photoreceptors_spatial_constant;
        result.horizontalCellsGain = source.horizontal_cells_gain;
        result.hcellsTemporalConstant = source.hcells_temporal_constant;
        result.hcellsSpatialConstant = source.hcells_spatial_constant;
        result.ganglionCellsSensitivity = source.ganglion_cells_sensitivity;
        return result;
    }

    cv::bioinspired::RetinaParameters::IplMagnoParameters to_native_magno(
        const jyppx_ocv_bioinspired_retina_magno_parameters& source)
    {
        cv::bioinspired::RetinaParameters::IplMagnoParameters result;
        result.normaliseOutput = source.normalise_output != 0;
        result.parasolCells_beta = source.parasol_cells_beta;
        result.parasolCells_tau = source.parasol_cells_tau;
        result.parasolCells_k = source.parasol_cells_k;
        result.amacrinCellsTemporalCutFrequency = source.amacrin_cells_temporal_cut_frequency;
        result.V0CompressionParameter = source.v0_compression_parameter;
        result.localAdaptintegration_tau = source.local_adapt_integration_tau;
        result.localAdaptintegration_k = source.local_adapt_integration_k;
        return result;
    }

    void fill_segmentation_parameters(
        const cv::bioinspired::SegmentationParameters& source,
        jyppx_ocv_bioinspired_segmentation_parameters* destination)
    {
        destination->threshold_on = source.thresholdON;
        destination->threshold_off = source.thresholdOFF;
        destination->local_energy_temporal_constant = source.localEnergy_temporalConstant;
        destination->local_energy_spatial_constant = source.localEnergy_spatialConstant;
        destination->neighborhood_energy_temporal_constant = source.neighborhoodEnergy_temporalConstant;
        destination->neighborhood_energy_spatial_constant = source.neighborhoodEnergy_spatialConstant;
        destination->context_energy_temporal_constant = source.contextEnergy_temporalConstant;
        destination->context_energy_spatial_constant = source.contextEnergy_spatialConstant;
    }

    cv::bioinspired::SegmentationParameters to_native_segmentation(
        const jyppx_ocv_bioinspired_segmentation_parameters& source)
    {
        cv::bioinspired::SegmentationParameters result;
        result.thresholdON = source.threshold_on;
        result.thresholdOFF = source.threshold_off;
        result.localEnergy_temporalConstant = source.local_energy_temporal_constant;
        result.localEnergy_spatialConstant = source.local_energy_spatial_constant;
        result.neighborhoodEnergy_temporalConstant = source.neighborhood_energy_temporal_constant;
        result.neighborhoodEnergy_spatialConstant = source.neighborhood_energy_spatial_constant;
        result.contextEnergy_temporalConstant = source.context_energy_temporal_constant;
        result.contextEnergy_spatialConstant = source.context_energy_spatial_constant;
        return result;
    }
#else
    int validate_retina(const char*, const jyppx_ocv_bioinspired_retina*) { return OPENCV_CSHARP_STATUS_OK; }
    int validate_tone_mapping(const char*, const jyppx_ocv_bioinspired_retina_fast_tone_mapping*) { return OPENCV_CSHARP_STATUS_OK; }
    int validate_segmentation(const char*, const jyppx_ocv_bioinspired_transient_areas_segmentation_module*) { return OPENCV_CSHARP_STATUS_OK; }
#endif
}

int jyppx_ocv_bioinspired_retina_create(
    int width,
    int height,
    int color_mode,
    int color_sampling_method,
    int use_retina_log_sampling,
    float reduction_factor,
    float sampling_strength,
    jyppx_ocv_bioinspired_retina** retina)
{
    constexpr const char* api_name = "jyppx_ocv_bioinspired_retina_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_pointer(api_name, retina, "retina");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *retina = nullptr;
        status = validate_positive_size(api_name, width, height);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BIOINSPIRED)
        jyppx_ocv_bioinspired_retina* created = new (std::nothrow) jyppx_ocv_bioinspired_retina();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = cv::bioinspired::Retina::create(
            cv::Size(width, height),
            color_mode != 0,
            color_sampling_method,
            use_retina_log_sampling != 0,
            reduction_factor,
            sampling_strength);
        if (created->value.empty())
        {
            delete created;
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        *retina = created;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)color_mode; (void)color_sampling_method; (void)use_retina_log_sampling; (void)reduction_factor; (void)sampling_strength;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_bioinspired_retina_release(jyppx_ocv_bioinspired_retina* retina)
{
    delete retina;
}

int jyppx_ocv_bioinspired_retina_get_input_size(const jyppx_ocv_bioinspired_retina* retina, int* width, int* height)
{
    constexpr const char* api_name = "jyppx_ocv_bioinspired_retina_get_input_size";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_pointer(api_name, width, "width");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_pointer(api_name, height, "height");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *width = 0;
        *height = 0;
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BIOINSPIRED)
        status = validate_retina(api_name, retina);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        cv::Size size = retina->value->getInputSize();
        *width = size.width;
        *height = size.height;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)retina;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_bioinspired_retina_get_output_size(const jyppx_ocv_bioinspired_retina* retina, int* width, int* height)
{
    constexpr const char* api_name = "jyppx_ocv_bioinspired_retina_get_output_size";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_pointer(api_name, width, "width");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_pointer(api_name, height, "height");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *width = 0;
        *height = 0;
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BIOINSPIRED)
        status = validate_retina(api_name, retina);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        cv::Size size = retina->value->getOutputSize();
        *width = size.width;
        *height = size.height;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)retina;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_bioinspired_retina_setup(
    jyppx_ocv_bioinspired_retina* retina,
    const unsigned char* retina_parameter_file,
    int apply_default_setup_on_failure)
{
    constexpr const char* api_name = "jyppx_ocv_bioinspired_retina_setup";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BIOINSPIRED)
        int status = validate_retina(api_name, retina);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        retina->value->setup(to_string_or_empty(retina_parameter_file), apply_default_setup_on_failure != 0);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)retina; (void)retina_parameter_file; (void)apply_default_setup_on_failure;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_bioinspired_retina_setup_parvo(
    jyppx_ocv_bioinspired_retina* retina,
    const jyppx_ocv_bioinspired_retina_parvo_parameters* parameters)
{
    constexpr const char* api_name = "jyppx_ocv_bioinspired_retina_setup_parvo";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_pointer(api_name, parameters, "parameters");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BIOINSPIRED)
        status = validate_retina(api_name, retina);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        cv::bioinspired::RetinaParameters::OPLandIplParvoParameters native = to_native_parvo(*parameters);
        retina->value->setupOPLandIPLParvoChannel(
            native.colorMode,
            native.normaliseOutput,
            native.photoreceptorsLocalAdaptationSensitivity,
            native.photoreceptorsTemporalConstant,
            native.photoreceptorsSpatialConstant,
            native.horizontalCellsGain,
            native.hcellsTemporalConstant,
            native.hcellsSpatialConstant,
            native.ganglionCellsSensitivity);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)retina;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_bioinspired_retina_setup_magno(
    jyppx_ocv_bioinspired_retina* retina,
    const jyppx_ocv_bioinspired_retina_magno_parameters* parameters)
{
    constexpr const char* api_name = "jyppx_ocv_bioinspired_retina_setup_magno";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_pointer(api_name, parameters, "parameters");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BIOINSPIRED)
        status = validate_retina(api_name, retina);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        cv::bioinspired::RetinaParameters::IplMagnoParameters native = to_native_magno(*parameters);
        retina->value->setupIPLMagnoChannel(
            native.normaliseOutput,
            native.parasolCells_beta,
            native.parasolCells_tau,
            native.parasolCells_k,
            native.amacrinCellsTemporalCutFrequency,
            native.V0CompressionParameter,
            native.localAdaptintegration_tau,
            native.localAdaptintegration_k);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)retina;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_bioinspired_retina_get_parameters(
    const jyppx_ocv_bioinspired_retina* retina,
    jyppx_ocv_bioinspired_retina_parvo_parameters* parvo,
    jyppx_ocv_bioinspired_retina_magno_parameters* magno)
{
    constexpr const char* api_name = "jyppx_ocv_bioinspired_retina_get_parameters";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_pointer(api_name, parvo, "parvo");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_pointer(api_name, magno, "magno");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BIOINSPIRED)
        status = validate_retina(api_name, retina);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        cv::bioinspired::RetinaParameters parameters = retina->value->getParameters();
        fill_parvo_parameters(parameters.OPLandIplParvo, parvo);
        fill_magno_parameters(parameters.IplMagno, magno);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)retina;
        std::memset(parvo, 0, sizeof(*parvo));
        std::memset(magno, 0, sizeof(*magno));
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_bioinspired_retina_run(jyppx_ocv_bioinspired_retina* retina, const jyppx_ocv_mat* input)
{
    constexpr const char* api_name = "jyppx_ocv_bioinspired_retina_run";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, input, "input");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BIOINSPIRED)
        status = validate_retina(api_name, retina);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        retina->value->run(opencv_csharp_native::mat_value(input));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)retina;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_bioinspired_retina_apply_fast_tone_mapping(
    jyppx_ocv_bioinspired_retina* retina,
    const jyppx_ocv_mat* input,
    jyppx_ocv_mat* output)
{
    constexpr const char* api_name = "jyppx_ocv_bioinspired_retina_apply_fast_tone_mapping";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, input, "input");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, output, "output");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BIOINSPIRED)
        status = validate_retina(api_name, retina);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        retina->value->applyFastToneMapping(opencv_csharp_native::mat_value(input), opencv_csharp_native::mat_value(output));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)retina;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_bioinspired_retina_get_parvo(jyppx_ocv_bioinspired_retina* retina, jyppx_ocv_mat* output)
{
    constexpr const char* api_name = "jyppx_ocv_bioinspired_retina_get_parvo";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, output, "output");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BIOINSPIRED)
        status = validate_retina(api_name, retina);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        retina->value->getParvo(opencv_csharp_native::mat_value(output));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)retina;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_bioinspired_retina_get_parvo_raw(jyppx_ocv_bioinspired_retina* retina, jyppx_ocv_mat* output)
{
    constexpr const char* api_name = "jyppx_ocv_bioinspired_retina_get_parvo_raw";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, output, "output");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BIOINSPIRED)
        status = validate_retina(api_name, retina);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        retina->value->getParvoRAW(opencv_csharp_native::mat_value(output));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)retina;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_bioinspired_retina_get_magno(jyppx_ocv_bioinspired_retina* retina, jyppx_ocv_mat* output)
{
    constexpr const char* api_name = "jyppx_ocv_bioinspired_retina_get_magno";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, output, "output");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BIOINSPIRED)
        status = validate_retina(api_name, retina);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        retina->value->getMagno(opencv_csharp_native::mat_value(output));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)retina;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_bioinspired_retina_get_magno_raw(jyppx_ocv_bioinspired_retina* retina, jyppx_ocv_mat* output)
{
    constexpr const char* api_name = "jyppx_ocv_bioinspired_retina_get_magno_raw";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, output, "output");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BIOINSPIRED)
        status = validate_retina(api_name, retina);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        retina->value->getMagnoRAW(opencv_csharp_native::mat_value(output));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)retina;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_bioinspired_retina_set_color_saturation(
    jyppx_ocv_bioinspired_retina* retina,
    int saturate_colors,
    float color_saturation_value)
{
    constexpr const char* api_name = "jyppx_ocv_bioinspired_retina_set_color_saturation";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BIOINSPIRED)
        int status = validate_retina(api_name, retina);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        retina->value->setColorSaturation(saturate_colors != 0, color_saturation_value);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)retina; (void)saturate_colors; (void)color_saturation_value;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_bioinspired_retina_clear_buffers(jyppx_ocv_bioinspired_retina* retina)
{
    constexpr const char* api_name = "jyppx_ocv_bioinspired_retina_clear_buffers";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BIOINSPIRED)
        int status = validate_retina(api_name, retina);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        retina->value->clearBuffers();
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)retina;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_bioinspired_retina_activate_moving_contours_processing(jyppx_ocv_bioinspired_retina* retina, int activate)
{
    constexpr const char* api_name = "jyppx_ocv_bioinspired_retina_activate_moving_contours_processing";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BIOINSPIRED)
        int status = validate_retina(api_name, retina);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        retina->value->activateMovingContoursProcessing(activate != 0);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)retina; (void)activate;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_bioinspired_retina_activate_contours_processing(jyppx_ocv_bioinspired_retina* retina, int activate)
{
    constexpr const char* api_name = "jyppx_ocv_bioinspired_retina_activate_contours_processing";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BIOINSPIRED)
        int status = validate_retina(api_name, retina);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        retina->value->activateContoursProcessing(activate != 0);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)retina; (void)activate;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_bioinspired_retina_print_setup_length(jyppx_ocv_bioinspired_retina* retina, int* length)
{
    constexpr const char* api_name = "jyppx_ocv_bioinspired_retina_print_setup_length";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_pointer(api_name, length, "length");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *length = 0;
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BIOINSPIRED)
        status = validate_retina(api_name, retina);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *length = static_cast<int>(retina->value->printSetup().size());
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)retina;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_bioinspired_retina_print_setup_fill(
    jyppx_ocv_bioinspired_retina* retina,
    unsigned char* buffer,
    int buffer_capacity,
    int* written)
{
    constexpr const char* api_name = "jyppx_ocv_bioinspired_retina_print_setup_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BIOINSPIRED)
        int status = validate_retina(api_name, retina);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        return fill_string_result(api_name, retina->value->printSetup(), buffer, buffer_capacity, written);
#else
        (void)retina; (void)buffer; (void)buffer_capacity;
        if (written != nullptr) { *written = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_bioinspired_retina_write(const jyppx_ocv_bioinspired_retina* retina, const unsigned char* path)
{
    constexpr const char* api_name = "jyppx_ocv_bioinspired_retina_write";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_pointer(api_name, path, "path");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BIOINSPIRED)
        status = validate_retina(api_name, retina);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        retina->value->write(to_string_or_empty(path));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)retina;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_bioinspired_retina_fast_tone_mapping_create(
    int width,
    int height,
    jyppx_ocv_bioinspired_retina_fast_tone_mapping** tone_mapping)
{
    constexpr const char* api_name = "jyppx_ocv_bioinspired_retina_fast_tone_mapping_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_pointer(api_name, tone_mapping, "tone_mapping");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *tone_mapping = nullptr;
        status = validate_positive_size(api_name, width, height);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BIOINSPIRED)
        jyppx_ocv_bioinspired_retina_fast_tone_mapping* created =
            new (std::nothrow) jyppx_ocv_bioinspired_retina_fast_tone_mapping();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = cv::bioinspired::RetinaFastToneMapping::create(cv::Size(width, height));
        if (created->value.empty())
        {
            delete created;
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        *tone_mapping = created;
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

void jyppx_ocv_bioinspired_retina_fast_tone_mapping_release(jyppx_ocv_bioinspired_retina_fast_tone_mapping* tone_mapping)
{
    delete tone_mapping;
}

int jyppx_ocv_bioinspired_retina_fast_tone_mapping_setup(
    jyppx_ocv_bioinspired_retina_fast_tone_mapping* tone_mapping,
    float photoreceptors_neighborhood_radius,
    float ganglion_cells_neighborhood_radius,
    float mean_luminance_modulator_k)
{
    constexpr const char* api_name = "jyppx_ocv_bioinspired_retina_fast_tone_mapping_setup";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BIOINSPIRED)
        int status = validate_tone_mapping(api_name, tone_mapping);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        tone_mapping->value->setup(photoreceptors_neighborhood_radius, ganglion_cells_neighborhood_radius, mean_luminance_modulator_k);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)tone_mapping; (void)photoreceptors_neighborhood_radius; (void)ganglion_cells_neighborhood_radius; (void)mean_luminance_modulator_k;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_bioinspired_retina_fast_tone_mapping_apply(
    jyppx_ocv_bioinspired_retina_fast_tone_mapping* tone_mapping,
    const jyppx_ocv_mat* input,
    jyppx_ocv_mat* output)
{
    constexpr const char* api_name = "jyppx_ocv_bioinspired_retina_fast_tone_mapping_apply";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, input, "input");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, output, "output");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BIOINSPIRED)
        status = validate_tone_mapping(api_name, tone_mapping);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        tone_mapping->value->applyFastToneMapping(opencv_csharp_native::mat_value(input), opencv_csharp_native::mat_value(output));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)tone_mapping;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_bioinspired_transient_areas_create(
    int width,
    int height,
    jyppx_ocv_bioinspired_transient_areas_segmentation_module** segmentation)
{
    constexpr const char* api_name = "jyppx_ocv_bioinspired_transient_areas_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_pointer(api_name, segmentation, "segmentation");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *segmentation = nullptr;
        status = validate_positive_size(api_name, width, height);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BIOINSPIRED)
        jyppx_ocv_bioinspired_transient_areas_segmentation_module* created =
            new (std::nothrow) jyppx_ocv_bioinspired_transient_areas_segmentation_module();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = cv::bioinspired::TransientAreasSegmentationModule::create(cv::Size(width, height));
        if (created->value.empty())
        {
            delete created;
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        *segmentation = created;
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

void jyppx_ocv_bioinspired_transient_areas_release(jyppx_ocv_bioinspired_transient_areas_segmentation_module* segmentation)
{
    delete segmentation;
}

int jyppx_ocv_bioinspired_transient_areas_get_size(
    const jyppx_ocv_bioinspired_transient_areas_segmentation_module* segmentation,
    int* width,
    int* height)
{
    constexpr const char* api_name = "jyppx_ocv_bioinspired_transient_areas_get_size";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_pointer(api_name, width, "width");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_pointer(api_name, height, "height");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *width = 0;
        *height = 0;
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BIOINSPIRED)
        status = validate_segmentation(api_name, segmentation);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        cv::Size size = segmentation->value->getSize();
        *width = size.width;
        *height = size.height;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)segmentation;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_bioinspired_transient_areas_setup(
    jyppx_ocv_bioinspired_transient_areas_segmentation_module* segmentation,
    const unsigned char* segmentation_parameter_file,
    int apply_default_setup_on_failure)
{
    constexpr const char* api_name = "jyppx_ocv_bioinspired_transient_areas_setup";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BIOINSPIRED)
        int status = validate_segmentation(api_name, segmentation);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        segmentation->value->setup(to_string_or_empty(segmentation_parameter_file), apply_default_setup_on_failure != 0);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)segmentation; (void)segmentation_parameter_file; (void)apply_default_setup_on_failure;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_bioinspired_transient_areas_setup_parameters(
    jyppx_ocv_bioinspired_transient_areas_segmentation_module* segmentation,
    const jyppx_ocv_bioinspired_segmentation_parameters* parameters)
{
    constexpr const char* api_name = "jyppx_ocv_bioinspired_transient_areas_setup_parameters";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_pointer(api_name, parameters, "parameters");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BIOINSPIRED)
        status = validate_segmentation(api_name, segmentation);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        segmentation->value->setup(to_native_segmentation(*parameters));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)segmentation;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_bioinspired_transient_areas_get_parameters(
    const jyppx_ocv_bioinspired_transient_areas_segmentation_module* segmentation,
    jyppx_ocv_bioinspired_segmentation_parameters* parameters)
{
    constexpr const char* api_name = "jyppx_ocv_bioinspired_transient_areas_get_parameters";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_pointer(api_name, parameters, "parameters");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BIOINSPIRED)
        status = validate_segmentation(api_name, segmentation);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        fill_segmentation_parameters(segmentation->value->getParameters(), parameters);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)segmentation;
        std::memset(parameters, 0, sizeof(*parameters));
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_bioinspired_transient_areas_run(
    jyppx_ocv_bioinspired_transient_areas_segmentation_module* segmentation,
    const jyppx_ocv_mat* input,
    int channel_index)
{
    constexpr const char* api_name = "jyppx_ocv_bioinspired_transient_areas_run";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, input, "input");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BIOINSPIRED)
        status = validate_segmentation(api_name, segmentation);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        segmentation->value->run(opencv_csharp_native::mat_value(input), channel_index);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)segmentation; (void)channel_index;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_bioinspired_transient_areas_get_segmentation_picture(
    jyppx_ocv_bioinspired_transient_areas_segmentation_module* segmentation,
    jyppx_ocv_mat* output)
{
    constexpr const char* api_name = "jyppx_ocv_bioinspired_transient_areas_get_segmentation_picture";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, output, "output");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BIOINSPIRED)
        status = validate_segmentation(api_name, segmentation);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        segmentation->value->getSegmentationPicture(opencv_csharp_native::mat_value(output));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)segmentation;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_bioinspired_transient_areas_clear_all_buffers(
    jyppx_ocv_bioinspired_transient_areas_segmentation_module* segmentation)
{
    constexpr const char* api_name = "jyppx_ocv_bioinspired_transient_areas_clear_all_buffers";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BIOINSPIRED)
        int status = validate_segmentation(api_name, segmentation);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        segmentation->value->clearAllBuffers();
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)segmentation;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_bioinspired_transient_areas_print_setup_length(
    jyppx_ocv_bioinspired_transient_areas_segmentation_module* segmentation,
    int* length)
{
    constexpr const char* api_name = "jyppx_ocv_bioinspired_transient_areas_print_setup_length";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_pointer(api_name, length, "length");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *length = 0;
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BIOINSPIRED)
        status = validate_segmentation(api_name, segmentation);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *length = static_cast<int>(segmentation->value->printSetup().size());
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)segmentation;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_bioinspired_transient_areas_print_setup_fill(
    jyppx_ocv_bioinspired_transient_areas_segmentation_module* segmentation,
    unsigned char* buffer,
    int buffer_capacity,
    int* written)
{
    constexpr const char* api_name = "jyppx_ocv_bioinspired_transient_areas_print_setup_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BIOINSPIRED)
        int status = validate_segmentation(api_name, segmentation);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        return fill_string_result(api_name, segmentation->value->printSetup(), buffer, buffer_capacity, written);
#else
        (void)segmentation; (void)buffer; (void)buffer_capacity;
        if (written != nullptr) { *written = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_bioinspired_transient_areas_write(
    const jyppx_ocv_bioinspired_transient_areas_segmentation_module* segmentation,
    const unsigned char* path)
{
    constexpr const char* api_name = "jyppx_ocv_bioinspired_transient_areas_write";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_pointer(api_name, path, "path");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BIOINSPIRED)
        status = validate_segmentation(api_name, segmentation);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        segmentation->value->write(to_string_or_empty(path));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)segmentation;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}


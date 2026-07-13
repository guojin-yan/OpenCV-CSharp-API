#include "open_cv_sharp/reg/reg.h"

#include "../core/mat_handle.h"
#include "../error_state.h"
#include "reg_handles.h"

#include <cmath>
#include <new>

namespace
{
    constexpr int MAPPER_KIND_GRAD_SHIFT = 1;
    constexpr int MAPPER_KIND_GRAD_EUCLID = 2;
    constexpr int MAPPER_KIND_GRAD_SIMILAR = 3;
    constexpr int MAPPER_KIND_GRAD_AFFINE = 4;
    constexpr int MAPPER_KIND_GRAD_PROJ = 5;
    constexpr int MAPPER_KIND_PYRAMID = 6;

    int validate_mat(const char* api_name, const jyppx_ocv_mat* mat, const char* argument_name)
    {
        return mat == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_map(const char* api_name, const jyppx_ocv_reg_map* map, const char* argument_name = "map")
    {
        return map == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_mapper(const char* api_name, const jyppx_ocv_reg_mapper* mapper, const char* argument_name = "mapper")
    {
        return mapper == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_output_pointer(const char* api_name, const void* value, const char* argument_name)
    {
        return value == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_finite_double(const char* api_name, double value, const char* argument_name)
    {
        return std::isfinite(value)
            ? OPENCV_CSHARP_STATUS_OK
            : opencv_csharp_native::set_invalid_argument(api_name, argument_name);
    }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_REG)
    int map_kind_from_value(const cv::Ptr<cv::reg::Map>& map)
    {
        if (map.empty())
        {
            return JYPPX_OCV_REG_MAP_KIND_UNKNOWN;
        }

        if (dynamic_cast<cv::reg::MapShift*>(map.get()) != nullptr)
        {
            return JYPPX_OCV_REG_MAP_KIND_SHIFT;
        }

        if (dynamic_cast<cv::reg::MapAffine*>(map.get()) != nullptr)
        {
            return JYPPX_OCV_REG_MAP_KIND_AFFINE;
        }

        if (dynamic_cast<cv::reg::MapProjec*>(map.get()) != nullptr)
        {
            return JYPPX_OCV_REG_MAP_KIND_PROJEC;
        }

        return JYPPX_OCV_REG_MAP_KIND_UNKNOWN;
    }

    int create_map_handle(const char* api_name, const cv::Ptr<cv::reg::Map>& native, jyppx_ocv_reg_map** map)
    {
        if (map == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "map");
        }

        *map = nullptr;
        if (native.empty())
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        jyppx_ocv_reg_map* created = new (std::nothrow) jyppx_ocv_reg_map();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = native;
        created->kind = map_kind_from_value(native);
        *map = created;
        return OPENCV_CSHARP_STATUS_OK;
    }

    int create_mapper_handle(
        const char* api_name,
        const cv::Ptr<cv::reg::Mapper>& native,
        int kind,
        jyppx_ocv_reg_mapper** mapper,
        const cv::Ptr<cv::reg::Mapper>& base_mapper = cv::Ptr<cv::reg::Mapper>())
    {
        if (mapper == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "mapper");
        }

        *mapper = nullptr;
        if (native.empty())
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        jyppx_ocv_reg_mapper* created = new (std::nothrow) jyppx_ocv_reg_mapper();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = native;
        created->base_mapper = base_mapper;
        created->kind = kind;
        *mapper = created;
        return OPENCV_CSHARP_STATUS_OK;
    }

    int require_map_kind(const char* api_name, const jyppx_ocv_reg_map* map, int kind)
    {
        int status = validate_map(api_name, map);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        return map->kind == kind
            ? OPENCV_CSHARP_STATUS_OK
            : opencv_csharp_native::set_invalid_argument(api_name, "map");
    }

    int require_pyramid_mapper(const char* api_name, const jyppx_ocv_reg_mapper* mapper)
    {
        int status = validate_mapper(api_name, mapper);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        return mapper->kind == MAPPER_KIND_PYRAMID
            ? OPENCV_CSHARP_STATUS_OK
            : opencv_csharp_native::set_invalid_argument(api_name, "mapper");
    }

    cv::Ptr<cv::reg::Map> clone_compatible_map(const cv::Ptr<cv::reg::Map>& map, int kind)
    {
        switch (kind)
        {
        case JYPPX_OCV_REG_MAP_KIND_SHIFT:
        {
            const auto* typed = dynamic_cast<const cv::reg::MapShift*>(map.get());
            return typed == nullptr ? cv::Ptr<cv::reg::Map>() : cv::makePtr<cv::reg::MapShift>(typed->getShift());
        }
        case JYPPX_OCV_REG_MAP_KIND_AFFINE:
        {
            const auto* typed = dynamic_cast<const cv::reg::MapAffine*>(map.get());
            return typed == nullptr ? cv::Ptr<cv::reg::Map>() : cv::makePtr<cv::reg::MapAffine>(typed->getLinTr(), typed->getShift());
        }
        case JYPPX_OCV_REG_MAP_KIND_PROJEC:
        {
            const auto* typed = dynamic_cast<const cv::reg::MapProjec*>(map.get());
            return typed == nullptr ? cv::Ptr<cv::reg::Map>() : cv::makePtr<cv::reg::MapProjec>(typed->getProjTr());
        }
        default:
            return cv::Ptr<cv::reg::Map>();
        }
    }
#endif
}

void jyppx_ocv_reg_map_release(jyppx_ocv_reg_map* map)
{
    delete map;
}

int jyppx_ocv_reg_map_get_kind(const jyppx_ocv_reg_map* map, int* kind)
{
    constexpr const char* api_name = "jyppx_ocv_reg_map_get_kind";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_map(api_name, map);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_pointer(api_name, kind, "kind");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_REG)
        *kind = map->kind;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *kind = JYPPX_OCV_REG_MAP_KIND_UNKNOWN;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_reg_map_warp(const jyppx_ocv_reg_map* map, const jyppx_ocv_mat* src, jyppx_ocv_mat* dst)
{
    constexpr const char* api_name = "jyppx_ocv_reg_map_warp";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_map(api_name, map);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_REG)
        map->value->warp(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst));
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

int jyppx_ocv_reg_map_inverse_warp(const jyppx_ocv_reg_map* map, const jyppx_ocv_mat* src, jyppx_ocv_mat* dst)
{
    constexpr const char* api_name = "jyppx_ocv_reg_map_inverse_warp";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_map(api_name, map);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_REG)
        map->value->inverseWarp(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst));
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

int jyppx_ocv_reg_map_inverse_map(const jyppx_ocv_reg_map* map, jyppx_ocv_reg_map** inverse_map)
{
    constexpr const char* api_name = "jyppx_ocv_reg_map_inverse_map";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_map(api_name, map);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (inverse_map == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "inverse_map");
        }

        *inverse_map = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_REG)
        return create_map_handle(api_name, map->value->inverseMap(), inverse_map);
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_reg_map_compose(jyppx_ocv_reg_map* map, const jyppx_ocv_reg_map* other)
{
    constexpr const char* api_name = "jyppx_ocv_reg_map_compose";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_map(api_name, map);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_map(api_name, other, "other");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_REG)
        if (map->kind != other->kind)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "other");
        }

        cv::Ptr<cv::reg::Map> other_clone = clone_compatible_map(other->value, other->kind);
        if (other_clone.empty())
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "other");
        }

        map->value->compose(other_clone);
        map->kind = map_kind_from_value(map->value);
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

int jyppx_ocv_reg_map_scale(jyppx_ocv_reg_map* map, double factor)
{
    constexpr const char* api_name = "jyppx_ocv_reg_map_scale";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_map(api_name, map);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_finite_double(api_name, factor, "factor");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_REG)
        map->value->scale(factor);
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

int jyppx_ocv_reg_map_shift_create(double shift_x, double shift_y, jyppx_ocv_reg_map** map)
{
    constexpr const char* api_name = "jyppx_ocv_reg_map_shift_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (map == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "map");
        }

        *map = nullptr;
        int status = validate_finite_double(api_name, shift_x, "shift_x");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_finite_double(api_name, shift_y, "shift_y");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_REG)
        cv::Vec<double, 2> shift(shift_x, shift_y);
        return create_map_handle(api_name, cv::makePtr<cv::reg::MapShift>(shift), map);
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_reg_map_shift_get(const jyppx_ocv_reg_map* map, double* shift_x, double* shift_y)
{
    constexpr const char* api_name = "jyppx_ocv_reg_map_shift_get";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_pointer(api_name, shift_x, "shift_x");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_pointer(api_name, shift_y, "shift_y");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_REG)
        status = require_map_kind(api_name, map, JYPPX_OCV_REG_MAP_KIND_SHIFT);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        const auto* typed = dynamic_cast<const cv::reg::MapShift*>(map->value.get());
        if (typed == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "map");
        }

        const cv::Vec<double, 2>& shift = typed->getShift();
        *shift_x = shift(0);
        *shift_y = shift(1);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)map;
        *shift_x = 0.0;
        *shift_y = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_reg_map_affine_create(
    double m00,
    double m01,
    double m10,
    double m11,
    double shift_x,
    double shift_y,
    jyppx_ocv_reg_map** map)
{
    constexpr const char* api_name = "jyppx_ocv_reg_map_affine_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (map == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "map");
        }

        *map = nullptr;
        const double values[] = { m00, m01, m10, m11, shift_x, shift_y };
        for (double value : values)
        {
            if (!std::isfinite(value))
            {
                return opencv_csharp_native::set_invalid_argument(api_name, "matrix");
            }
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_REG)
        cv::Matx<double, 2, 2> lin_tr(m00, m01, m10, m11);
        cv::Vec<double, 2> shift(shift_x, shift_y);
        return create_map_handle(api_name, cv::makePtr<cv::reg::MapAffine>(lin_tr, shift), map);
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_reg_map_affine_get(
    const jyppx_ocv_reg_map* map,
    double* m00,
    double* m01,
    double* m10,
    double* m11,
    double* shift_x,
    double* shift_y)
{
    constexpr const char* api_name = "jyppx_ocv_reg_map_affine_get";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (m00 == nullptr || m01 == nullptr || m10 == nullptr || m11 == nullptr || shift_x == nullptr || shift_y == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "output");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_REG)
        int status = require_map_kind(api_name, map, JYPPX_OCV_REG_MAP_KIND_AFFINE);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        const auto* typed = dynamic_cast<const cv::reg::MapAffine*>(map->value.get());
        if (typed == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "map");
        }

        const cv::Matx<double, 2, 2>& lin_tr = typed->getLinTr();
        const cv::Vec<double, 2>& shift = typed->getShift();
        *m00 = lin_tr(0, 0);
        *m01 = lin_tr(0, 1);
        *m10 = lin_tr(1, 0);
        *m11 = lin_tr(1, 1);
        *shift_x = shift(0);
        *shift_y = shift(1);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)map;
        *m00 = 1.0; *m01 = 0.0; *m10 = 0.0; *m11 = 1.0; *shift_x = 0.0; *shift_y = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_reg_map_projec_create(
    double m00,
    double m01,
    double m02,
    double m10,
    double m11,
    double m12,
    double m20,
    double m21,
    double m22,
    jyppx_ocv_reg_map** map)
{
    constexpr const char* api_name = "jyppx_ocv_reg_map_projec_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (map == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "map");
        }

        *map = nullptr;
        const double values[] = { m00, m01, m02, m10, m11, m12, m20, m21, m22 };
        for (double value : values)
        {
            if (!std::isfinite(value))
            {
                return opencv_csharp_native::set_invalid_argument(api_name, "matrix");
            }
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_REG)
        cv::Matx<double, 3, 3> proj_tr(m00, m01, m02, m10, m11, m12, m20, m21, m22);
        return create_map_handle(api_name, cv::makePtr<cv::reg::MapProjec>(proj_tr), map);
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_reg_map_projec_get(
    const jyppx_ocv_reg_map* map,
    double* m00,
    double* m01,
    double* m02,
    double* m10,
    double* m11,
    double* m12,
    double* m20,
    double* m21,
    double* m22)
{
    constexpr const char* api_name = "jyppx_ocv_reg_map_projec_get";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (m00 == nullptr || m01 == nullptr || m02 == nullptr ||
            m10 == nullptr || m11 == nullptr || m12 == nullptr ||
            m20 == nullptr || m21 == nullptr || m22 == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "output");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_REG)
        int status = require_map_kind(api_name, map, JYPPX_OCV_REG_MAP_KIND_PROJEC);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        const auto* typed = dynamic_cast<const cv::reg::MapProjec*>(map->value.get());
        if (typed == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "map");
        }

        const cv::Matx<double, 3, 3>& proj_tr = typed->getProjTr();
        *m00 = proj_tr(0, 0); *m01 = proj_tr(0, 1); *m02 = proj_tr(0, 2);
        *m10 = proj_tr(1, 0); *m11 = proj_tr(1, 1); *m12 = proj_tr(1, 2);
        *m20 = proj_tr(2, 0); *m21 = proj_tr(2, 1); *m22 = proj_tr(2, 2);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)map;
        *m00 = 1.0; *m01 = 0.0; *m02 = 0.0; *m10 = 0.0; *m11 = 1.0; *m12 = 0.0; *m20 = 0.0; *m21 = 0.0; *m22 = 1.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_reg_map_projec_normalize(jyppx_ocv_reg_map* map)
{
    constexpr const char* api_name = "jyppx_ocv_reg_map_projec_normalize";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_REG)
        int status = require_map_kind(api_name, map, JYPPX_OCV_REG_MAP_KIND_PROJEC);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        auto* typed = dynamic_cast<cv::reg::MapProjec*>(map->value.get());
        if (typed == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "map");
        }

        typed->normalize();
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)map;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_reg_mapper_release(jyppx_ocv_reg_mapper* mapper)
{
    delete mapper;
}

int jyppx_ocv_reg_mapper_grad_shift_create(jyppx_ocv_reg_mapper** mapper)
{
    constexpr const char* api_name = "jyppx_ocv_reg_mapper_grad_shift_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (mapper == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "mapper");
        }

        *mapper = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_REG)
        return create_mapper_handle(api_name, cv::makePtr<cv::reg::MapperGradShift>(), MAPPER_KIND_GRAD_SHIFT, mapper);
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_reg_mapper_grad_euclid_create(jyppx_ocv_reg_mapper** mapper)
{
    constexpr const char* api_name = "jyppx_ocv_reg_mapper_grad_euclid_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (mapper == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "mapper");
        }

        *mapper = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_REG)
        return create_mapper_handle(api_name, cv::makePtr<cv::reg::MapperGradEuclid>(), MAPPER_KIND_GRAD_EUCLID, mapper);
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_reg_mapper_grad_similar_create(jyppx_ocv_reg_mapper** mapper)
{
    constexpr const char* api_name = "jyppx_ocv_reg_mapper_grad_similar_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (mapper == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "mapper");
        }

        *mapper = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_REG)
        return create_mapper_handle(api_name, cv::makePtr<cv::reg::MapperGradSimilar>(), MAPPER_KIND_GRAD_SIMILAR, mapper);
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_reg_mapper_grad_affine_create(jyppx_ocv_reg_mapper** mapper)
{
    constexpr const char* api_name = "jyppx_ocv_reg_mapper_grad_affine_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (mapper == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "mapper");
        }

        *mapper = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_REG)
        return create_mapper_handle(api_name, cv::makePtr<cv::reg::MapperGradAffine>(), MAPPER_KIND_GRAD_AFFINE, mapper);
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_reg_mapper_grad_proj_create(jyppx_ocv_reg_mapper** mapper)
{
    constexpr const char* api_name = "jyppx_ocv_reg_mapper_grad_proj_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (mapper == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "mapper");
        }

        *mapper = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_REG)
        return create_mapper_handle(api_name, cv::makePtr<cv::reg::MapperGradProj>(), MAPPER_KIND_GRAD_PROJ, mapper);
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_reg_mapper_pyramid_create(const jyppx_ocv_reg_mapper* base_mapper, jyppx_ocv_reg_mapper** mapper)
{
    constexpr const char* api_name = "jyppx_ocv_reg_mapper_pyramid_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mapper(api_name, base_mapper, "base_mapper");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (mapper == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "mapper");
        }

        *mapper = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_REG)
        cv::Ptr<cv::reg::Mapper> retained_base = base_mapper->value;
        cv::Ptr<cv::reg::Mapper> pyramid = cv::makePtr<cv::reg::MapperPyramid>(retained_base);
        return create_mapper_handle(api_name, pyramid, MAPPER_KIND_PYRAMID, mapper, retained_base);
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_reg_mapper_calculate(
    const jyppx_ocv_reg_mapper* mapper,
    const jyppx_ocv_mat* img1,
    const jyppx_ocv_mat* img2,
    const jyppx_ocv_reg_map* init,
    jyppx_ocv_reg_map** map)
{
    constexpr const char* api_name = "jyppx_ocv_reg_mapper_calculate";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mapper(api_name, mapper);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, img1, "img1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, img2, "img2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (map == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "map");
        }

        *map = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_REG)
        cv::Ptr<cv::reg::Map> init_map;
        if (init != nullptr)
        {
            init_map = clone_compatible_map(init->value, init->kind);
            if (init_map.empty())
            {
                return opencv_csharp_native::set_invalid_argument(api_name, "init");
            }
        }

        return create_map_handle(
            api_name,
            mapper->value->calculate(opencv_csharp_native::mat_value(img1), opencv_csharp_native::mat_value(img2), init_map),
            map);
#else
        (void)init;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_reg_mapper_get_map(const jyppx_ocv_reg_mapper* mapper, jyppx_ocv_reg_map** map)
{
    constexpr const char* api_name = "jyppx_ocv_reg_mapper_get_map";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mapper(api_name, mapper);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (map == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "map");
        }

        *map = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_REG)
        cv::Ptr<cv::reg::Map> result = mapper->value->getMap();
        if (result.empty())
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "mapper");
        }

        return create_map_handle(api_name, result, map);
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_reg_mapper_pyramid_get_num_levels(const jyppx_ocv_reg_mapper* mapper, int* value)
{
    constexpr const char* api_name = "jyppx_ocv_reg_mapper_pyramid_get_num_levels";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_pointer(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_REG)
        status = require_pyramid_mapper(api_name, mapper);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        const auto* typed = dynamic_cast<const cv::reg::MapperPyramid*>(mapper->value.get());
        if (typed == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "mapper");
        }

        *value = typed->numLev_;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)mapper;
        *value = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_reg_mapper_pyramid_set_num_levels(jyppx_ocv_reg_mapper* mapper, int value)
{
    constexpr const char* api_name = "jyppx_ocv_reg_mapper_pyramid_set_num_levels";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (value <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "value");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_REG)
        int status = require_pyramid_mapper(api_name, mapper);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        auto* typed = dynamic_cast<cv::reg::MapperPyramid*>(mapper->value.get());
        if (typed == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "mapper");
        }

        typed->numLev_ = value;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)mapper;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_reg_mapper_pyramid_get_num_iterations_per_scale(const jyppx_ocv_reg_mapper* mapper, int* value)
{
    constexpr const char* api_name = "jyppx_ocv_reg_mapper_pyramid_get_num_iterations_per_scale";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_pointer(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_REG)
        status = require_pyramid_mapper(api_name, mapper);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        const auto* typed = dynamic_cast<const cv::reg::MapperPyramid*>(mapper->value.get());
        if (typed == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "mapper");
        }

        *value = typed->numIterPerScale_;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)mapper;
        *value = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_reg_mapper_pyramid_set_num_iterations_per_scale(jyppx_ocv_reg_mapper* mapper, int value)
{
    constexpr const char* api_name = "jyppx_ocv_reg_mapper_pyramid_set_num_iterations_per_scale";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (value <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "value");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_REG)
        int status = require_pyramid_mapper(api_name, mapper);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        auto* typed = dynamic_cast<cv::reg::MapperPyramid*>(mapper->value.get());
        if (typed == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "mapper");
        }

        typed->numIterPerScale_ = value;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)mapper;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}



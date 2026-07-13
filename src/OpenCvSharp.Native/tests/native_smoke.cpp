#include "open_cv_sharp/core/mat.h"
#include "open_cv_sharp/error.h"
#include "open_cv_sharp/imgproc.h"
#include "open_cv_sharp/status.h"
#include "open_cv_sharp/version.h"

#include <cstring>

namespace
{
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

        int filter_transform_status = run_imgproc_filter_transform_api_smoke();
        if (filter_transform_status != 0)
        {
            return filter_transform_status;
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
        const char* error = jyppx_ocv_get_last_error();
        return error != nullptr && std::strlen(error) > 0 ? 0 : 12;
    }

    return 13;
}


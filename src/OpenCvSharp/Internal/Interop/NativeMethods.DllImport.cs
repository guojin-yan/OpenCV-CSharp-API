#if !NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_get_last_error")]
        internal static extern IntPtr GetLastErrorPointer();

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_clear_last_error")]
        internal static extern void ClearLastError();

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_get_version_string")]
        internal static extern IntPtr GetVersionStringPointer();

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_create_empty")]
        internal static extern int MatCreateEmpty(out IntPtr mat);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_create")]
        internal static extern int MatCreate(int rows, int cols, int type, out IntPtr mat);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_create_with_scalar")]
        internal static extern int MatCreateWithScalar(int rows, int cols, int type, double v0, double v1, double v2, double v3, out IntPtr mat);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_create_in_place")]
        internal static extern int MatCreateInPlace(IntPtr mat, int rows, int cols, int type);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_zeros")]
        internal static extern int MatZeros(int rows, int cols, int type, out IntPtr mat);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_ones")]
        internal static extern int MatOnes(int rows, int cols, int type, out IntPtr mat);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_eye")]
        internal static extern int MatEye(int rows, int cols, int type, out IntPtr mat);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_release")]
        internal static extern void MatRelease(IntPtr mat);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_clone")]
        internal static extern int MatClone(IntPtr mat, out IntPtr clone);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_copy_to")]
        internal static extern int MatCopyTo(IntPtr src, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_convert_to")]
        internal static extern int MatConvertTo(IntPtr src, IntPtr dst, int rtype, double alpha, double beta);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_set_to")]
        internal static extern int MatSetTo(IntPtr mat, double v0, double v1, double v2, double v3);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_submat")]
        internal static extern int MatSubmat(IntPtr mat, int x, int y, int width, int height, out IntPtr submat);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_row_range")]
        internal static extern int MatRowRange(IntPtr mat, int startRow, int endRow, out IntPtr submat);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_col_range")]
        internal static extern int MatColRange(IntPtr mat, int startCol, int endCol, out IntPtr submat);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_reshape")]
        internal static extern int MatReshape(IntPtr mat, int channels, int rows, out IntPtr reshaped);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_empty")]
        internal static extern int MatEmpty(IntPtr mat, out int empty);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_dims")]
        internal static extern int MatDims(IntPtr mat, out int dims);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_rows")]
        internal static extern int MatRows(IntPtr mat, out int rows);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_cols")]
        internal static extern int MatCols(IntPtr mat, out int cols);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_channels")]
        internal static extern int MatChannels(IntPtr mat, out int channels);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_depth")]
        internal static extern int MatDepth(IntPtr mat, out int depth);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_type")]
        internal static extern int MatType(IntPtr mat, out int type);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_total")]
        internal static extern int MatTotal(IntPtr mat, out UIntPtr total);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_elem_size")]
        internal static extern int MatElemSize(IntPtr mat, out UIntPtr elemSize);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_elem_size1")]
        internal static extern int MatElemSize1(IntPtr mat, out UIntPtr elemSize1);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_step")]
        internal static extern int MatStep(IntPtr mat, out UIntPtr step);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_step1")]
        internal static extern int MatStep1(IntPtr mat, out UIntPtr step1);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_data")]
        internal static extern int MatData(IntPtr mat, out IntPtr data);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_is_continuous")]
        internal static extern int MatIsContinuous(IntPtr mat, out int isContinuous);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_is_submatrix")]
        internal static extern int MatIsSubmatrix(IntPtr mat, out int isSubmatrix);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_add")]
        internal static extern int CoreAdd(IntPtr src1, IntPtr src2, IntPtr dst, IntPtr mask, int dtype);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_add_scalar")]
        internal static extern int CoreAddScalar(IntPtr src, double v0, double v1, double v2, double v3, IntPtr dst, IntPtr mask, int dtype);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_subtract")]
        internal static extern int CoreSubtract(IntPtr src1, IntPtr src2, IntPtr dst, IntPtr mask, int dtype);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_subtract_scalar")]
        internal static extern int CoreSubtractScalar(IntPtr src, double v0, double v1, double v2, double v3, IntPtr dst, IntPtr mask, int dtype);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_multiply")]
        internal static extern int CoreMultiply(IntPtr src1, IntPtr src2, IntPtr dst, double scale, int dtype);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_divide")]
        internal static extern int CoreDivide(IntPtr src1, IntPtr src2, IntPtr dst, double scale, int dtype);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_scale_add")]
        internal static extern int CoreScaleAdd(IntPtr src1, double alpha, IntPtr src2, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_add_weighted")]
        internal static extern int CoreAddWeighted(IntPtr src1, double alpha, IntPtr src2, double beta, double gamma, IntPtr dst, int dtype);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_absdiff")]
        internal static extern int CoreAbsDiff(IntPtr src1, IntPtr src2, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_absdiff_scalar")]
        internal static extern int CoreAbsDiffScalar(IntPtr src, double v0, double v1, double v2, double v3, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_bitwise_and")]
        internal static extern int CoreBitwiseAnd(IntPtr src1, IntPtr src2, IntPtr dst, IntPtr mask);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_bitwise_or")]
        internal static extern int CoreBitwiseOr(IntPtr src1, IntPtr src2, IntPtr dst, IntPtr mask);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_bitwise_xor")]
        internal static extern int CoreBitwiseXor(IntPtr src1, IntPtr src2, IntPtr dst, IntPtr mask);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_bitwise_not")]
        internal static extern int CoreBitwiseNot(IntPtr src, IntPtr dst, IntPtr mask);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_compare")]
        internal static extern int CoreCompare(IntPtr src1, IntPtr src2, IntPtr dst, int cmpop);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_min")]
        internal static extern int CoreMin(IntPtr src1, IntPtr src2, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_max")]
        internal static extern int CoreMax(IntPtr src1, IntPtr src2, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_in_range")]
        internal static extern int CoreInRange(IntPtr src, double lowerV0, double lowerV1, double lowerV2, double lowerV3, double upperV0, double upperV1, double upperV2, double upperV3, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_patch_nans")]
        internal static extern int CorePatchNaNs(IntPtr src, double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_count_non_zero")]
        internal static extern int CoreCountNonZero(IntPtr src, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_mean")]
        internal static extern int CoreMean(IntPtr src, IntPtr mask, double[] values, int valuesLength);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_mean")]
        internal static extern unsafe int CoreMeanPtr(IntPtr src, IntPtr mask, double* values, int valuesLength);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_mean_std_dev")]
        internal static extern int CoreMeanStdDev(IntPtr src, IntPtr mask, double[] mean, int meanLength, double[] stddev, int stddevLength);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_mean_std_dev")]
        internal static extern unsafe int CoreMeanStdDevPtr(IntPtr src, IntPtr mask, double* mean, int meanLength, double* stddev, int stddevLength);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_min_max_loc")]
        internal static extern int CoreMinMaxLoc(IntPtr src, IntPtr mask, out double minVal, out double maxVal, out int minX, out int minY, out int maxX, out int maxY);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_norm")]
        internal static extern int CoreNorm(IntPtr src1, int normType, IntPtr mask, out double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_norm_diff")]
        internal static extern int CoreNormDiff(IntPtr src1, IntPtr src2, int normType, IntPtr mask, out double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_normalize")]
        internal static extern int CoreNormalize(IntPtr src, IntPtr dst, double alpha, double beta, int normType, int dtype, IntPtr mask);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_reduce")]
        internal static extern int CoreReduce(IntPtr src, IntPtr dst, int dim, int rtype, int dtype);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_sum")]
        internal static extern int CoreSum(IntPtr src, double[] values, int valuesLength);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_sum")]
        internal static extern unsafe int CoreSumPtr(IntPtr src, double* values, int valuesLength);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_trace")]
        internal static extern int CoreTrace(IntPtr src, double[] values, int valuesLength);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_trace")]
        internal static extern unsafe int CoreTracePtr(IntPtr src, double* values, int valuesLength);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_determinant")]
        internal static extern int CoreDeterminant(IntPtr src, out double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_invert")]
        internal static extern int CoreInvert(IntPtr src, IntPtr dst, int flags, out double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_solve")]
        internal static extern int CoreSolve(IntPtr src1, IntPtr src2, IntPtr dst, int flags, out int success);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_mahalanobis")]
        internal static extern int CoreMahalanobis(IntPtr v1, IntPtr v2, IntPtr icovar, out double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_split_count")]
        internal static extern int CoreSplitCount(IntPtr src, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_split_fill")]
        internal static extern int CoreSplitFill(IntPtr src, IntPtr[] dst, int dstCapacity, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_split_fill")]
        internal static unsafe extern int CoreSplitFillPtr(IntPtr src, IntPtr* dst, int dstCapacity, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_merge")]
        internal static extern int CoreMerge(IntPtr[] src, int srcCount, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_merge")]
        internal static extern unsafe int CoreMergePtr(IntPtr* src, int srcCount, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_hconcat")]
        internal static extern int CoreHConcat(IntPtr[] src, int srcCount, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_hconcat")]
        internal static extern unsafe int CoreHConcatPtr(IntPtr* src, int srcCount, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_vconcat")]
        internal static extern int CoreVConcat(IntPtr[] src, int srcCount, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_vconcat")]
        internal static extern unsafe int CoreVConcatPtr(IntPtr* src, int srcCount, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_extract_channel")]
        internal static extern int CoreExtractChannel(IntPtr src, IntPtr dst, int coi);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_insert_channel")]
        internal static extern int CoreInsertChannel(IntPtr src, IntPtr dst, int coi);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_mix_channels")]
        internal static extern int CoreMixChannels(IntPtr[] src, int srcCount, IntPtr[] dst, int dstCount, int[] fromTo, int pairCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_mix_channels")]
        internal static extern unsafe int CoreMixChannelsPtr(IntPtr* src, int srcCount, IntPtr* dst, int dstCount, int* fromTo, int pairCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_repeat")]
        internal static extern int CoreRepeat(IntPtr src, int ny, int nx, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_flip")]
        internal static extern int CoreFlip(IntPtr src, IntPtr dst, int flipCode);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_rotate")]
        internal static extern int CoreRotate(IntPtr src, IntPtr dst, int rotateCode);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_transpose")]
        internal static extern int CoreTranspose(IntPtr src, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_lut")]
        internal static extern int CoreLut(IntPtr src, IntPtr lut, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_convert_scale_abs")]
        internal static extern int CoreConvertScaleAbs(IntPtr src, IntPtr dst, double alpha, double beta);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_complete_symm")]
        internal static extern int CoreCompleteSymm(IntPtr mat, int lowerToUpper);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_set_identity")]
        internal static extern int CoreSetIdentity(IntPtr mat, double v0, double v1, double v2, double v3);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_kmeans")]
        internal static extern int CoreKMeans(IntPtr data, int k, IntPtr bestLabels, int criteriaType, int criteriaMaxCount, double criteriaEpsilon, int attempts, int flags, IntPtr centers, out double compactness);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_imencode", CharSet = CharSet.Ansi)]
        internal static extern int ImgCodecsImEncode(string ext, IntPtr image, out IntPtr buffer);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_imencode_with_params", CharSet = CharSet.Ansi)]
        internal static extern int ImgCodecsImEncodeWithParams(string ext, IntPtr image, int[] parameters, UIntPtr parametersLength, out IntPtr buffer);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_imencode_with_params", CharSet = CharSet.Ansi)]
        internal static extern unsafe int ImgCodecsImEncodeWithParams(string ext, IntPtr image, int* parameters, UIntPtr parametersLength, out IntPtr buffer);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_imdecode")]
        internal static extern int ImgCodecsImDecode(byte[] buffer, UIntPtr bufferLength, int flags, out IntPtr image);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_imdecode")]
        internal static extern unsafe int ImgCodecsImDecode(byte* buffer, UIntPtr bufferLength, int flags, out IntPtr image);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_imread")]
        internal static extern int ImgCodecsImRead(byte[] filename, int flags, out IntPtr image);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_imwrite")]
        internal static extern int ImgCodecsImWrite(byte[] filename, IntPtr image, out int written);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_imwrite_with_params")]
        internal static extern int ImgCodecsImWriteWithParams(byte[] filename, IntPtr image, int[] parameters, UIntPtr parametersLength, out int written);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_imwrite_with_params")]
        internal static extern unsafe int ImgCodecsImWriteWithParams(byte[] filename, IntPtr image, int* parameters, UIntPtr parametersLength, out int written);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_encoded_buffer_size")]
        internal static extern int EncodedBufferSize(IntPtr buffer, out UIntPtr size);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_encoded_buffer_data")]
        internal static extern int EncodedBufferData(IntPtr buffer, out IntPtr data);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_encoded_buffer_release")]
        internal static extern void EncodedBufferRelease(IntPtr buffer);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_cvt_color")]
        internal static extern int ImgProcCvtColor(IntPtr src, IntPtr dst, int code, int dstCn);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_resize")]
        internal static extern int ImgProcResize(
            IntPtr src,
            IntPtr dst,
            int width,
            int height,
            double fx,
            double fy,
            int interpolation);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_threshold")]
        internal static extern int ImgProcThreshold(
            IntPtr src,
            IntPtr dst,
            double thresh,
            double maxval,
            int type,
            out double threshold);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_adaptive_threshold")]
        internal static extern int ImgProcAdaptiveThreshold(
            IntPtr src,
            IntPtr dst,
            double maxValue,
            int adaptiveMethod,
            int thresholdType,
            int blockSize,
            double c);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_integral")]
        internal static extern int ImgProcIntegral(
            IntPtr src,
            IntPtr sum,
            int sdepth);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_integral2")]
        internal static extern int ImgProcIntegral2(
            IntPtr src,
            IntPtr sum,
            IntPtr sqsum,
            int sdepth,
            int sqdepth);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_integral3")]
        internal static extern int ImgProcIntegral3(
            IntPtr src,
            IntPtr sum,
            IntPtr sqsum,
            IntPtr tilted,
            int sdepth,
            int sqdepth);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_distance_transform")]
        internal static extern int ImgProcDistanceTransform(
            IntPtr src,
            IntPtr dst,
            int distanceType,
            int maskSize,
            int dstType);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_distance_transform_with_labels")]
        internal static extern int ImgProcDistanceTransformWithLabels(
            IntPtr src,
            IntPtr dst,
            IntPtr labels,
            int distanceType,
            int maskSize,
            int labelType);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_flood_fill")]
        internal static extern int ImgProcFloodFill(
            IntPtr image,
            int seedX,
            int seedY,
            double newValueV0,
            double newValueV1,
            double newValueV2,
            double newValueV3,
            out int rectX,
            out int rectY,
            out int rectWidth,
            out int rectHeight,
            double loDiffV0,
            double loDiffV1,
            double loDiffV2,
            double loDiffV3,
            double upDiffV0,
            double upDiffV1,
            double upDiffV2,
            double upDiffV3,
            int flags,
            out int filledCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_flood_fill_mask")]
        internal static extern int ImgProcFloodFillMask(
            IntPtr image,
            IntPtr mask,
            int seedX,
            int seedY,
            double newValueV0,
            double newValueV1,
            double newValueV2,
            double newValueV3,
            out int rectX,
            out int rectY,
            out int rectWidth,
            out int rectHeight,
            double loDiffV0,
            double loDiffV1,
            double loDiffV2,
            double loDiffV3,
            double upDiffV0,
            double upDiffV1,
            double upDiffV2,
            double upDiffV3,
            int flags,
            out int filledCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_connected_components")]
        internal static extern int ImgProcConnectedComponents(
            IntPtr image,
            IntPtr labels,
            int connectivity,
            int ltype,
            out int labelCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_connected_components_with_algorithm")]
        internal static extern int ImgProcConnectedComponentsWithAlgorithm(
            IntPtr image,
            IntPtr labels,
            int connectivity,
            int ltype,
            int ccltype,
            out int labelCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_connected_components_with_stats")]
        internal static extern int ImgProcConnectedComponentsWithStats(
            IntPtr image,
            IntPtr labels,
            IntPtr stats,
            IntPtr centroids,
            int connectivity,
            int ltype,
            out int labelCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_connected_components_with_stats_with_algorithm")]
        internal static extern int ImgProcConnectedComponentsWithStatsWithAlgorithm(
            IntPtr image,
            IntPtr labels,
            IntPtr stats,
            IntPtr centroids,
            int connectivity,
            int ltype,
            int ccltype,
            out int labelCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_equalize_hist")]
        internal static extern int ImgProcEqualizeHist(
            IntPtr src,
            IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_corner_harris")]
        internal static extern int ImgProcCornerHarris(
            IntPtr src,
            IntPtr dst,
            int blockSize,
            int ksize,
            double k,
            int borderType);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_corner_min_eigen_val")]
        internal static extern int ImgProcCornerMinEigenVal(
            IntPtr src,
            IntPtr dst,
            int blockSize,
            int ksize,
            int borderType);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_corner_eigen_vals_and_vecs")]
        internal static extern int ImgProcCornerEigenValsAndVecs(
            IntPtr src,
            IntPtr dst,
            int blockSize,
            int ksize,
            int borderType);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_pre_corner_detect")]
        internal static extern int ImgProcPreCornerDetect(
            IntPtr src,
            IntPtr dst,
            int ksize,
            int borderType);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_gaussian_blur")]
        internal static extern int ImgProcGaussianBlur(
            IntPtr src,
            IntPtr dst,
            int width,
            int height,
            double sigmaX,
            double sigmaY,
            int borderType);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_blur")]
        internal static extern int ImgProcBlur(
            IntPtr src,
            IntPtr dst,
            int width,
            int height,
            int anchorX,
            int anchorY,
            int borderType);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_box_filter")]
        internal static extern int ImgProcBoxFilter(
            IntPtr src,
            IntPtr dst,
            int ddepth,
            int width,
            int height,
            int anchorX,
            int anchorY,
            int normalize,
            int borderType);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_sqr_box_filter")]
        internal static extern int ImgProcSqrBoxFilter(
            IntPtr src,
            IntPtr dst,
            int ddepth,
            int width,
            int height,
            int anchorX,
            int anchorY,
            int normalize,
            int borderType);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_median_blur")]
        internal static extern int ImgProcMedianBlur(
            IntPtr src,
            IntPtr dst,
            int ksize);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_bilateral_filter")]
        internal static extern int ImgProcBilateralFilter(
            IntPtr src,
            IntPtr dst,
            int d,
            double sigmaColor,
            double sigmaSpace,
            int borderType);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_filter2d")]
        internal static extern int ImgProcFilter2D(
            IntPtr src,
            IntPtr dst,
            int ddepth,
            IntPtr kernel,
            int anchorX,
            int anchorY,
            double delta,
            int borderType);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_sep_filter2d")]
        internal static extern int ImgProcSepFilter2D(
            IntPtr src,
            IntPtr dst,
            int ddepth,
            IntPtr kernelX,
            IntPtr kernelY,
            int anchorX,
            int anchorY,
            double delta,
            int borderType);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_sobel")]
        internal static extern int ImgProcSobel(
            IntPtr src,
            IntPtr dst,
            int ddepth,
            int dx,
            int dy,
            int ksize,
            double scale,
            double delta,
            int borderType);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_scharr")]
        internal static extern int ImgProcScharr(
            IntPtr src,
            IntPtr dst,
            int ddepth,
            int dx,
            int dy,
            double scale,
            double delta,
            int borderType);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_laplacian")]
        internal static extern int ImgProcLaplacian(
            IntPtr src,
            IntPtr dst,
            int ddepth,
            int ksize,
            double scale,
            double delta,
            int borderType);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_canny")]
        internal static extern int ImgProcCanny(
            IntPtr image,
            IntPtr edges,
            double threshold1,
            double threshold2,
            int apertureSize,
            int l2Gradient);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_canny_derivatives")]
        internal static extern int ImgProcCannyDerivatives(
            IntPtr dx,
            IntPtr dy,
            IntPtr edges,
            double threshold1,
            double threshold2,
            int l2Gradient);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_get_gaussian_kernel")]
        internal static extern int ImgProcGetGaussianKernel(
            int ksize,
            double sigma,
            int ktype,
            out IntPtr kernel);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_get_deriv_kernels")]
        internal static extern int ImgProcGetDerivKernels(
            IntPtr kx,
            IntPtr ky,
            int dx,
            int dy,
            int ksize,
            int normalize,
            int ktype);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_get_gabor_kernel")]
        internal static extern int ImgProcGetGaborKernel(
            int width,
            int height,
            double sigma,
            double theta,
            double lambd,
            double gamma,
            double psi,
            int ktype,
            out IntPtr kernel);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_pyr_down")]
        internal static extern int ImgProcPyrDown(
            IntPtr src,
            IntPtr dst,
            int width,
            int height,
            int borderType);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_pyr_up")]
        internal static extern int ImgProcPyrUp(
            IntPtr src,
            IntPtr dst,
            int width,
            int height,
            int borderType);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_warp_affine")]
        internal static extern int ImgProcWarpAffine(
            IntPtr src,
            IntPtr dst,
            IntPtr transform,
            int width,
            int height,
            int flags,
            int borderMode,
            double borderValueV0,
            double borderValueV1,
            double borderValueV2,
            double borderValueV3);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_warp_perspective")]
        internal static extern int ImgProcWarpPerspective(
            IntPtr src,
            IntPtr dst,
            IntPtr transform,
            int width,
            int height,
            int flags,
            int borderMode,
            double borderValueV0,
            double borderValueV1,
            double borderValueV2,
            double borderValueV3);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_get_rotation_matrix2d")]
        internal static extern int ImgProcGetRotationMatrix2D(
            float centerX,
            float centerY,
            double angle,
            double scale,
            out IntPtr transform);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_get_affine_transform")]
        internal static extern unsafe int ImgProcGetAffineTransform(
            float* srcXy,
            float* dstXy,
            out IntPtr transform);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_get_perspective_transform")]
        internal static extern unsafe int ImgProcGetPerspectiveTransform(
            float* srcXy,
            float* dstXy,
            int solveMethod,
            out IntPtr transform);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_invert_affine_transform")]
        internal static extern int ImgProcInvertAffineTransform(
            IntPtr transform,
            IntPtr inverseTransform);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_remap")]
        internal static extern int ImgProcRemap(
            IntPtr src,
            IntPtr dst,
            IntPtr map1,
            IntPtr map2,
            int interpolation,
            int borderMode,
            double borderValueV0,
            double borderValueV1,
            double borderValueV2,
            double borderValueV3);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_convert_maps")]
        internal static extern int ImgProcConvertMaps(
            IntPtr map1,
            IntPtr map2,
            IntPtr dstmap1,
            IntPtr dstmap2,
            int dstmap1type,
            int nninterpolation);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_get_structuring_element")]
        internal static extern int ImgProcGetStructuringElement(
            int shape,
            int width,
            int height,
            int anchorX,
            int anchorY,
            out IntPtr element);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_erode")]
        internal static extern int ImgProcErode(
            IntPtr src,
            IntPtr dst,
            IntPtr kernel,
            int anchorX,
            int anchorY,
            int iterations,
            int borderType,
            int hasBorderValue,
            double borderValueV0,
            double borderValueV1,
            double borderValueV2,
            double borderValueV3);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_dilate")]
        internal static extern int ImgProcDilate(
            IntPtr src,
            IntPtr dst,
            IntPtr kernel,
            int anchorX,
            int anchorY,
            int iterations,
            int borderType,
            int hasBorderValue,
            double borderValueV0,
            double borderValueV1,
            double borderValueV2,
            double borderValueV3);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_morphology_ex")]
        internal static extern int ImgProcMorphologyEx(
            IntPtr src,
            IntPtr dst,
            int op,
            IntPtr kernel,
            int anchorX,
            int anchorY,
            int iterations,
            int borderType,
            int hasBorderValue,
            double borderValueV0,
            double borderValueV1,
            double borderValueV2,
            double borderValueV3);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_line")]
        internal static extern int ImgProcLine(
            IntPtr img,
            int x1,
            int y1,
            int x2,
            int y2,
            double colorV0,
            double colorV1,
            double colorV2,
            double colorV3,
            int thickness,
            int lineType,
            int shift);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_arrowed_line")]
        internal static extern int ImgProcArrowedLine(
            IntPtr img,
            int x1,
            int y1,
            int x2,
            int y2,
            double colorV0,
            double colorV1,
            double colorV2,
            double colorV3,
            int thickness,
            int lineType,
            int shift,
            double tipLength);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_clip_line_rect")]
        internal static extern int ImgProcClipLineRect(
            int rectX,
            int rectY,
            int rectWidth,
            int rectHeight,
            ref int pt1X,
            ref int pt1Y,
            ref int pt2X,
            ref int pt2Y,
            out int intersects);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_polylines")]
        internal static extern int ImgProcPolylines(
            IntPtr img,
            int[] pointsXy,
            int pointCount,
            int isClosed,
            double colorV0,
            double colorV1,
            double colorV2,
            double colorV3,
            int thickness,
            int lineType,
            int shift);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_fill_poly")]
        internal static extern int ImgProcFillPoly(
            IntPtr img,
            int[] pointsXy,
            int pointCount,
            double colorV0,
            double colorV1,
            double colorV2,
            double colorV3,
            int lineType,
            int shift,
            int offsetX,
            int offsetY);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_ellipse2_poly_count")]
        internal static extern int ImgProcEllipse2PolyCount(
            int centerX,
            int centerY,
            int axesWidth,
            int axesHeight,
            int angle,
            int arcStart,
            int arcEnd,
            int delta,
            out int pointCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_ellipse2_poly_fill")]
        internal static extern int ImgProcEllipse2PolyFill(
            int centerX,
            int centerY,
            int axesWidth,
            int axesHeight,
            int angle,
            int arcStart,
            int arcEnd,
            int delta,
            int[] pointsXy,
            int pointCapacity,
            out int pointCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_contour_area")]
        internal static extern int ImgProcContourArea(
            int[] pointsXy,
            int pointCount,
            int oriented,
            out double area);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_contour_area")]
        internal static unsafe extern int ImgProcContourAreaPtr(
            int* pointsXy,
            int pointCount,
            int oriented,
            out double area);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_find_contours_count")]
        internal static extern int ImgProcFindContoursCount(
            IntPtr image,
            int mode,
            int method,
            int offsetX,
            int offsetY,
            out int contourCount,
            out int totalPointCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_find_contours_fill")]
        internal static extern int ImgProcFindContoursFill(
            IntPtr image,
            int mode,
            int method,
            int offsetX,
            int offsetY,
            int[] contoursXy,
            int pointCapacity,
            int[] contourLengths,
            int contourCapacity,
            int[] hierarchy,
            int hierarchyCapacity,
            out int contourCount,
            out int totalPointCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_draw_contours")]
        internal static extern int ImgProcDrawContours(
            IntPtr image,
            int[] contoursXy,
            int[] contourLengths,
            int contourCount,
            int contourIndex,
            double colorV0,
            double colorV1,
            double colorV2,
            double colorV3,
            int thickness,
            int lineType,
            int[] hierarchy,
            int hasHierarchy,
            int maxLevel,
            int offsetX,
            int offsetY);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_moments_points")]
        internal static extern int ImgProcMomentsPoints(
            int[] pointsXy,
            int pointCount,
            int binaryImage,
            double[] values,
            int valueCapacity);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_moments_points")]
        internal static unsafe extern int ImgProcMomentsPointsPtr(
            int* pointsXy,
            int pointCount,
            int binaryImage,
            double* values,
            int valueCapacity);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_moments_mat")]
        internal static extern int ImgProcMomentsMat(
            IntPtr array,
            int binaryImage,
            double[] values,
            int valueCapacity);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_hu_moments")]
        internal static extern int ImgProcHuMoments(
            double[] momentsValues,
            int valueCount,
            double[] huValues,
            int huCapacity);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_arc_length")]
        internal static extern int ImgProcArcLength(
            int[] pointsXy,
            int pointCount,
            int closed,
            out double length);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_arc_length")]
        internal static unsafe extern int ImgProcArcLengthPtr(
            int* pointsXy,
            int pointCount,
            int closed,
            out double length);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_approx_poly_dp_count")]
        internal static extern int ImgProcApproxPolyDPCount(
            int[] curveXy,
            int pointCount,
            double epsilon,
            int closed,
            out int approxPointCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_approx_poly_dp_count")]
        internal static unsafe extern int ImgProcApproxPolyDPCountPtr(
            int* curveXy,
            int pointCount,
            double epsilon,
            int closed,
            out int approxPointCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_approx_poly_dp_fill")]
        internal static extern int ImgProcApproxPolyDPFill(
            int[] curveXy,
            int pointCount,
            double epsilon,
            int closed,
            int[] approxPointsXy,
            int approxPointCapacity,
            out int approxPointCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_approx_poly_dp_fill")]
        internal static unsafe extern int ImgProcApproxPolyDPFillPtr(
            int* curveXy,
            int pointCount,
            double epsilon,
            int closed,
            int* approxPointsXy,
            int approxPointCapacity,
            out int approxPointCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_approx_poly_n_count")]
        internal static extern int ImgProcApproxPolyNCount(
            int[] curveXy,
            int pointCount,
            int nsides,
            float epsilonPercentage,
            int ensureConvex,
            out int approxPointCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_approx_poly_n_count")]
        internal static unsafe extern int ImgProcApproxPolyNCountPtr(
            int* curveXy,
            int pointCount,
            int nsides,
            float epsilonPercentage,
            int ensureConvex,
            out int approxPointCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_approx_poly_n_fill")]
        internal static extern int ImgProcApproxPolyNFill(
            int[] curveXy,
            int pointCount,
            int nsides,
            float epsilonPercentage,
            int ensureConvex,
            float[] approxPointsXy,
            int approxPointCapacity,
            out int approxPointCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_approx_poly_n_fill")]
        internal static unsafe extern int ImgProcApproxPolyNFillPtr(
            int* curveXy,
            int pointCount,
            int nsides,
            float epsilonPercentage,
            int ensureConvex,
            float* approxPointsXy,
            int approxPointCapacity,
            out int approxPointCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_bounding_rect")]
        internal static extern int ImgProcBoundingRect(
            int[] pointsXy,
            int pointCount,
            out int x,
            out int y,
            out int width,
            out int height);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_bounding_rect")]
        internal static unsafe extern int ImgProcBoundingRectPtr(
            int* pointsXy,
            int pointCount,
            out int x,
            out int y,
            out int width,
            out int height);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_is_contour_convex")]
        internal static extern int ImgProcIsContourConvex(
            int[] pointsXy,
            int pointCount,
            out int isConvex);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_is_contour_convex")]
        internal static unsafe extern int ImgProcIsContourConvexPtr(
            int* pointsXy,
            int pointCount,
            out int isConvex);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_convex_hull_count")]
        internal static extern int ImgProcConvexHullCount(
            int[] pointsXy,
            int pointCount,
            int clockwise,
            out int hullPointCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_convex_hull_count")]
        internal static unsafe extern int ImgProcConvexHullCountPtr(
            int* pointsXy,
            int pointCount,
            int clockwise,
            out int hullPointCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_convex_hull_fill")]
        internal static extern int ImgProcConvexHullFill(
            int[] pointsXy,
            int pointCount,
            int clockwise,
            int[] hullPointsXy,
            int hullPointCapacity,
            out int hullPointCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_convex_hull_fill")]
        internal static unsafe extern int ImgProcConvexHullFillPtr(
            int* pointsXy,
            int pointCount,
            int clockwise,
            int* hullPointsXy,
            int hullPointCapacity,
            out int hullPointCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_convex_hull_indices_count")]
        internal static extern int ImgProcConvexHullIndicesCount(
            int[] pointsXy,
            int pointCount,
            int clockwise,
            out int hullIndexCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_convex_hull_indices_count")]
        internal static unsafe extern int ImgProcConvexHullIndicesCountPtr(
            int* pointsXy,
            int pointCount,
            int clockwise,
            out int hullIndexCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_convex_hull_indices_fill")]
        internal static extern int ImgProcConvexHullIndicesFill(
            int[] pointsXy,
            int pointCount,
            int clockwise,
            int[] hullIndices,
            int hullIndexCapacity,
            out int hullIndexCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_convex_hull_indices_fill")]
        internal static unsafe extern int ImgProcConvexHullIndicesFillPtr(
            int* pointsXy,
            int pointCount,
            int clockwise,
            int* hullIndices,
            int hullIndexCapacity,
            out int hullIndexCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_convexity_defects_count")]
        internal static extern int ImgProcConvexityDefectsCount(
            int[] contourXy,
            int contourPointCount,
            int[] hullIndices,
            int hullIndexCount,
            out int defectCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_convexity_defects_fill")]
        internal static extern int ImgProcConvexityDefectsFill(
            int[] contourXy,
            int contourPointCount,
            int[] hullIndices,
            int hullIndexCount,
            int[] defects,
            int defectCapacity,
            out int defectCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_min_enclosing_circle")]
        internal static extern int ImgProcMinEnclosingCircle(
            int[] pointsXy,
            int pointCount,
            out float centerX,
            out float centerY,
            out float radius);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_min_enclosing_circle")]
        internal static unsafe extern int ImgProcMinEnclosingCirclePtr(
            int* pointsXy,
            int pointCount,
            out float centerX,
            out float centerY,
            out float radius);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_point_polygon_test")]
        internal static extern int ImgProcPointPolygonTest(
            int[] contourXy,
            int pointCount,
            float pointX,
            float pointY,
            int measureDist,
            out double result);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_point_polygon_test")]
        internal static unsafe extern int ImgProcPointPolygonTestPtr(
            int* contourXy,
            int pointCount,
            float pointX,
            float pointY,
            int measureDist,
            out double result);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_match_shapes")]
        internal static extern int ImgProcMatchShapes(
            int[] contour1Xy,
            int contour1PointCount,
            int[] contour2Xy,
            int contour2PointCount,
            int method,
            double parameter,
            out double result);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_match_shapes")]
        internal static unsafe extern int ImgProcMatchShapesPtr(
            int* contour1Xy,
            int contour1PointCount,
            int* contour2Xy,
            int contour2PointCount,
            int method,
            double parameter,
            out double result);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_min_area_rect")]
        internal static extern int ImgProcMinAreaRect(
            int[] pointsXy,
            int pointCount,
            out float centerX,
            out float centerY,
            out float width,
            out float height,
            out float angle);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_min_area_rect")]
        internal static unsafe extern int ImgProcMinAreaRectPtr(
            int* pointsXy,
            int pointCount,
            out float centerX,
            out float centerY,
            out float width,
            out float height,
            out float angle);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_box_points")]
        internal static extern int ImgProcBoxPoints(
            float centerX,
            float centerY,
            float width,
            float height,
            float angle,
            float[] pointsXy,
            int pointCapacity);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_fit_ellipse")]
        internal static extern int ImgProcFitEllipse(
            int[] pointsXy,
            int pointCount,
            out float centerX,
            out float centerY,
            out float width,
            out float height,
            out float angle);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_fit_ellipse")]
        internal static unsafe extern int ImgProcFitEllipsePtr(
            int* pointsXy,
            int pointCount,
            out float centerX,
            out float centerY,
            out float width,
            out float height,
            out float angle);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_fit_ellipse_ams")]
        internal static extern int ImgProcFitEllipseAMS(
            int[] pointsXy,
            int pointCount,
            out float centerX,
            out float centerY,
            out float width,
            out float height,
            out float angle);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_fit_ellipse_ams")]
        internal static unsafe extern int ImgProcFitEllipseAMSPtr(
            int* pointsXy,
            int pointCount,
            out float centerX,
            out float centerY,
            out float width,
            out float height,
            out float angle);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_fit_ellipse_direct")]
        internal static extern int ImgProcFitEllipseDirect(
            int[] pointsXy,
            int pointCount,
            out float centerX,
            out float centerY,
            out float width,
            out float height,
            out float angle);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_fit_ellipse_direct")]
        internal static unsafe extern int ImgProcFitEllipseDirectPtr(
            int* pointsXy,
            int pointCount,
            out float centerX,
            out float centerY,
            out float width,
            out float height,
            out float angle);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_rotated_rectangle_intersection_count")]
        internal static extern int ImgProcRotatedRectangleIntersectionCount(
            float rect1CenterX,
            float rect1CenterY,
            float rect1Width,
            float rect1Height,
            float rect1Angle,
            float rect2CenterX,
            float rect2CenterY,
            float rect2Width,
            float rect2Height,
            float rect2Angle,
            out int intersectionType,
            out int pointCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_rotated_rectangle_intersection_fill")]
        internal static extern int ImgProcRotatedRectangleIntersectionFill(
            float rect1CenterX,
            float rect1CenterY,
            float rect1Width,
            float rect1Height,
            float rect1Angle,
            float rect2CenterX,
            float rect2CenterY,
            float rect2Width,
            float rect2Height,
            float rect2Angle,
            float[] pointsXy,
            int pointCapacity,
            out int intersectionType,
            out int pointCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_get_closest_ellipse_points")]
        internal static extern int ImgProcGetClosestEllipsePoints(
            float centerX,
            float centerY,
            float width,
            float height,
            float angle,
            int[] pointsXy,
            int pointCount,
            float[] closestPointsXy,
            int closestPointCapacity);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_get_closest_ellipse_points")]
        internal static unsafe extern int ImgProcGetClosestEllipsePointsPtr(
            float centerX,
            float centerY,
            float width,
            float height,
            float angle,
            int* pointsXy,
            int pointCount,
            float* closestPointsXy,
            int closestPointCapacity);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_min_enclosing_triangle")]
        internal static extern int ImgProcMinEnclosingTriangle(
            int[] pointsXy,
            int pointCount,
            float[] trianglePointsXy,
            int trianglePointCapacity,
            out double area);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_min_enclosing_triangle")]
        internal static unsafe extern int ImgProcMinEnclosingTrianglePtr(
            int* pointsXy,
            int pointCount,
            float* trianglePointsXy,
            int trianglePointCapacity,
            out double area);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_min_enclosing_convex_polygon")]
        internal static extern int ImgProcMinEnclosingConvexPolygon(
            int[] pointsXy,
            int pointCount,
            int k,
            float[] polygonPointsXy,
            int polygonPointCapacity,
            out int polygonPointCount,
            out double area);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_min_enclosing_convex_polygon")]
        internal static unsafe extern int ImgProcMinEnclosingConvexPolygonPtr(
            int* pointsXy,
            int pointCount,
            int k,
            float* polygonPointsXy,
            int polygonPointCapacity,
            out int polygonPointCount,
            out double area);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_intersect_convex_convex_count")]
        internal static extern int ImgProcIntersectConvexConvexCount(
            int[] polygon1Xy,
            int polygon1PointCount,
            int[] polygon2Xy,
            int polygon2PointCount,
            int handleNested,
            out float area,
            out int intersectingPointCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_intersect_convex_convex_count")]
        internal static unsafe extern int ImgProcIntersectConvexConvexCountPtr(
            int* polygon1Xy,
            int polygon1PointCount,
            int* polygon2Xy,
            int polygon2PointCount,
            int handleNested,
            out float area,
            out int intersectingPointCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_intersect_convex_convex_fill")]
        internal static extern int ImgProcIntersectConvexConvexFill(
            int[] polygon1Xy,
            int polygon1PointCount,
            int[] polygon2Xy,
            int polygon2PointCount,
            int handleNested,
            float[] intersectingPointsXy,
            int intersectingPointCapacity,
            out float area,
            out int intersectingPointCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_intersect_convex_convex_fill")]
        internal static unsafe extern int ImgProcIntersectConvexConvexFillPtr(
            int* polygon1Xy,
            int polygon1PointCount,
            int* polygon2Xy,
            int polygon2PointCount,
            int handleNested,
            float* intersectingPointsXy,
            int intersectingPointCapacity,
            out float area,
            out int intersectingPointCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_fit_line_2d")]
        internal static extern int ImgProcFitLine2D(
            int[] pointsXy,
            int pointCount,
            int distType,
            double param,
            double reps,
            double aeps,
            out float vx,
            out float vy,
            out float x0,
            out float y0);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_fit_line_2d")]
        internal static unsafe extern int ImgProcFitLine2DPtr(
            int* pointsXy,
            int pointCount,
            int distType,
            double param,
            double reps,
            double aeps,
            out float vx,
            out float vy,
            out float x0,
            out float y0);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_rectangle")]
        internal static extern int ImgProcRectangle(
            IntPtr img,
            int x1,
            int y1,
            int x2,
            int y2,
            double colorV0,
            double colorV1,
            double colorV2,
            double colorV3,
            int thickness,
            int lineType,
            int shift);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_rectangle_by_rect")]
        internal static extern int ImgProcRectangleByRect(
            IntPtr img,
            int x,
            int y,
            int width,
            int height,
            double colorV0,
            double colorV1,
            double colorV2,
            double colorV3,
            int thickness,
            int lineType,
            int shift);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_circle")]
        internal static extern int ImgProcCircle(
            IntPtr img,
            int centerX,
            int centerY,
            int radius,
            double colorV0,
            double colorV1,
            double colorV2,
            double colorV3,
            int thickness,
            int lineType,
            int shift);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_ellipse")]
        internal static extern int ImgProcEllipse(
            IntPtr img,
            int centerX,
            int centerY,
            int axesWidth,
            int axesHeight,
            double angle,
            double startAngle,
            double endAngle,
            double colorV0,
            double colorV1,
            double colorV2,
            double colorV3,
            int thickness,
            int lineType,
            int shift);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_put_text")]
        internal static extern int ImgProcPutText(
            IntPtr img,
            byte[] text,
            int orgX,
            int orgY,
            int fontFace,
            double fontScale,
            double colorV0,
            double colorV1,
            double colorV2,
            double colorV3,
            int thickness,
            int lineType,
            int bottomLeftOrigin);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_get_text_size")]
        internal static extern int ImgProcGetTextSize(
            byte[] text,
            int fontFace,
            double fontScale,
            int thickness,
            out int width,
            out int height,
            out int baseLine);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_clahe_create")]
        internal static extern int ImgProcClaheCreate(double clipLimit, int tilesGridWidth, int tilesGridHeight, out IntPtr clahe);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_clahe_release")]
        internal static extern void ImgProcClaheRelease(IntPtr clahe);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_clahe_apply")]
        internal static extern int ImgProcClaheApply(IntPtr clahe, IntPtr src, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_clahe_get_clip_limit")]
        internal static extern int ImgProcClaheGetClipLimit(IntPtr clahe, out double clipLimit);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_clahe_set_clip_limit")]
        internal static extern int ImgProcClaheSetClipLimit(IntPtr clahe, double clipLimit);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_clahe_get_tiles_grid_size")]
        internal static extern int ImgProcClaheGetTilesGridSize(IntPtr clahe, out int width, out int height);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_clahe_set_tiles_grid_size")]
        internal static extern int ImgProcClaheSetTilesGridSize(IntPtr clahe, int width, int height);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_clahe_get_bit_shift")]
        internal static extern int ImgProcClaheGetBitShift(IntPtr clahe, out int bitShift);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_clahe_set_bit_shift")]
        internal static extern int ImgProcClaheSetBitShift(IntPtr clahe, int bitShift);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_clahe_collect_garbage")]
        internal static extern int ImgProcClaheCollectGarbage(IntPtr clahe);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_corner_sub_pix")]
        internal static extern int ImgProcCornerSubPix(IntPtr image, float[] cornersXy, int cornerCount, int winWidth, int winHeight, int zeroZoneWidth, int zeroZoneHeight, int criteriaType, int criteriaMaxCount, double criteriaEpsilon);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_corner_sub_pix")]
        internal static unsafe extern int ImgProcCornerSubPixPtr(IntPtr image, float* cornersXy, int cornerCount, int winWidth, int winHeight, int zeroZoneWidth, int zeroZoneHeight, int criteriaType, int criteriaMaxCount, double criteriaEpsilon);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_good_features_to_track_count")]
        internal static extern int ImgProcGoodFeaturesToTrackCount(IntPtr image, IntPtr mask, int maxCorners, double qualityLevel, double minDistance, int blockSize, int gradientSize, int useHarrisDetector, double k, out int cornerCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_good_features_to_track_fill")]
        internal static extern int ImgProcGoodFeaturesToTrackFill(IntPtr image, IntPtr mask, int maxCorners, double qualityLevel, double minDistance, int blockSize, int gradientSize, int useHarrisDetector, double k, float[] cornersXy, int cornerCapacity, out int cornerCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_hough_lines_count")]
        internal static extern int ImgProcHoughLinesCount(IntPtr image, double rho, double theta, int threshold, double srn, double stn, double minTheta, double maxTheta, int useEdgeval, out int lineCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_hough_lines_fill")]
        internal static extern int ImgProcHoughLinesFill(IntPtr image, double rho, double theta, int threshold, double srn, double stn, double minTheta, double maxTheta, int useEdgeval, float[] lines, int lineCapacity, out int lineCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_hough_lines_p_count")]
        internal static extern int ImgProcHoughLinesPCount(IntPtr image, double rho, double theta, int threshold, double minLineLength, double maxLineGap, out int lineCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_hough_lines_p_fill")]
        internal static extern int ImgProcHoughLinesPFill(IntPtr image, double rho, double theta, int threshold, double minLineLength, double maxLineGap, int[] lines, int lineCapacity, out int lineCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_hough_lines_point_set_count")]
        internal static extern int ImgProcHoughLinesPointSetCount(int[] pointsXy, int pointCount, int linesMax, int threshold, double minRho, double maxRho, double rhoStep, double minTheta, double maxTheta, double thetaStep, out int lineCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_hough_lines_point_set_count")]
        internal static unsafe extern int ImgProcHoughLinesPointSetCountPtr(int* pointsXy, int pointCount, int linesMax, int threshold, double minRho, double maxRho, double rhoStep, double minTheta, double maxTheta, double thetaStep, out int lineCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_hough_lines_point_set_fill")]
        internal static extern int ImgProcHoughLinesPointSetFill(int[] pointsXy, int pointCount, int linesMax, int threshold, double minRho, double maxRho, double rhoStep, double minTheta, double maxTheta, double thetaStep, double[] lines, int lineCapacity, out int lineCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_hough_lines_point_set_fill")]
        internal static unsafe extern int ImgProcHoughLinesPointSetFillPtr(int* pointsXy, int pointCount, int linesMax, int threshold, double minRho, double maxRho, double rhoStep, double minTheta, double maxTheta, double thetaStep, double[] lines, int lineCapacity, out int lineCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_hough_circles_count")]
        internal static extern int ImgProcHoughCirclesCount(IntPtr image, int method, double dp, double minDist, double param1, double param2, int minRadius, int maxRadius, out int circleCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_hough_circles_fill")]
        internal static extern int ImgProcHoughCirclesFill(IntPtr image, int method, double dp, double minDist, double param1, double param2, int minRadius, int maxRadius, float[] circles, int circleCapacity, out int circleCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_calc_hist_uniform")]
        internal static extern int ImgProcCalcHistUniform(IntPtr image, IntPtr mask, int[] channels, int channelCount, IntPtr hist, int[] histSize, int histDims, float[] ranges, int rangeCount, int accumulate);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_calc_back_project_uniform")]
        internal static extern int ImgProcCalcBackProjectUniform(IntPtr image, int[] channels, int channelCount, IntPtr hist, IntPtr backProject, float[] ranges, int rangeCount, double scale);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_compare_hist")]
        internal static extern int ImgProcCompareHist(IntPtr h1, IntPtr h2, int method, out double result);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_line_segment_detector_create")]
        internal static extern int ImgProcLineSegmentDetectorCreate(int refine, double scale, double sigmaScale, double quant, double angTh, double logEps, double densityTh, int nBins, out IntPtr detector);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_line_segment_detector_release")]
        internal static extern void ImgProcLineSegmentDetectorRelease(IntPtr detector);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_line_segment_detector_detect")]
        internal static extern int ImgProcLineSegmentDetectorDetect(IntPtr detector, IntPtr image, IntPtr lines, IntPtr width, IntPtr prec, IntPtr nfa);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_line_segment_detector_detect_count")]
        internal static extern int ImgProcLineSegmentDetectorDetectCount(IntPtr detector, IntPtr image, out int lineCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_line_segment_detector_detect_fill")]
        internal static extern int ImgProcLineSegmentDetectorDetectFill(IntPtr detector, IntPtr image, float[] lines, int lineCapacity, out int lineCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_line_segment_detector_draw_segments")]
        internal static extern int ImgProcLineSegmentDetectorDrawSegments(IntPtr detector, IntPtr image, IntPtr lines);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_line_segment_detector_draw_segments_array")]
        internal static extern int ImgProcLineSegmentDetectorDrawSegmentsArray(IntPtr detector, IntPtr image, float[] lines, int lineCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_line_segment_detector_compare_segments")]
        internal static extern int ImgProcLineSegmentDetectorCompareSegments(IntPtr detector, int width, int height, IntPtr lines1, IntPtr lines2, IntPtr image, out int mismatchCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_line_segment_detector_compare_segments_array")]
        internal static extern int ImgProcLineSegmentDetectorCompareSegmentsArray(IntPtr detector, int width, int height, float[] lines1, int line1Count, float[] lines2, int line2Count, IntPtr image, out int mismatchCount);
    }
}
#endif

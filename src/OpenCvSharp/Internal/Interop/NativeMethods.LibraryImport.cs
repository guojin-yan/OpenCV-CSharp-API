#if NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_get_last_error")]
        internal static partial IntPtr GetLastErrorPointer();

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_clear_last_error")]
        internal static partial void ClearLastError();

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_get_version_string")]
        internal static partial IntPtr GetVersionStringPointer();

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_get_version_major")]
        internal static partial int GetVersionMajor();

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_get_version_minor")]
        internal static partial int GetVersionMinor();

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_get_version_revision")]
        internal static partial int GetVersionRevision();

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_create_empty")]
        internal static partial int MatCreateEmpty(out IntPtr mat);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_create")]
        internal static partial int MatCreate(int rows, int cols, int type, out IntPtr mat);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_create_with_scalar")]
        internal static partial int MatCreateWithScalar(int rows, int cols, int type, double v0, double v1, double v2, double v3, out IntPtr mat);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_create_in_place")]
        internal static partial int MatCreateInPlace(IntPtr mat, int rows, int cols, int type);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_zeros")]
        internal static partial int MatZeros(int rows, int cols, int type, out IntPtr mat);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_ones")]
        internal static partial int MatOnes(int rows, int cols, int type, out IntPtr mat);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_eye")]
        internal static partial int MatEye(int rows, int cols, int type, out IntPtr mat);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_release")]
        internal static partial void MatRelease(IntPtr mat);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_clone")]
        internal static partial int MatClone(IntPtr mat, out IntPtr clone);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_copy_to")]
        internal static partial int MatCopyTo(IntPtr src, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_convert_to")]
        internal static partial int MatConvertTo(IntPtr src, IntPtr dst, int rtype, double alpha, double beta);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_set_to")]
        internal static partial int MatSetTo(IntPtr mat, double v0, double v1, double v2, double v3);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_submat")]
        internal static partial int MatSubmat(IntPtr mat, int x, int y, int width, int height, out IntPtr submat);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_row_range")]
        internal static partial int MatRowRange(IntPtr mat, int startRow, int endRow, out IntPtr submat);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_col_range")]
        internal static partial int MatColRange(IntPtr mat, int startCol, int endCol, out IntPtr submat);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_reshape")]
        internal static partial int MatReshape(IntPtr mat, int channels, int rows, out IntPtr reshaped);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_empty")]
        internal static partial int MatEmpty(IntPtr mat, out int empty);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_dims")]
        internal static partial int MatDims(IntPtr mat, out int dims);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_rows")]
        internal static partial int MatRows(IntPtr mat, out int rows);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_cols")]
        internal static partial int MatCols(IntPtr mat, out int cols);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_channels")]
        internal static partial int MatChannels(IntPtr mat, out int channels);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_depth")]
        internal static partial int MatDepth(IntPtr mat, out int depth);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_type")]
        internal static partial int MatType(IntPtr mat, out int type);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_total")]
        internal static partial int MatTotal(IntPtr mat, out UIntPtr total);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_elem_size")]
        internal static partial int MatElemSize(IntPtr mat, out UIntPtr elemSize);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_elem_size1")]
        internal static partial int MatElemSize1(IntPtr mat, out UIntPtr elemSize1);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_step")]
        internal static partial int MatStep(IntPtr mat, out UIntPtr step);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_step1")]
        internal static partial int MatStep1(IntPtr mat, out UIntPtr step1);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_data")]
        internal static partial int MatData(IntPtr mat, out IntPtr data);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_is_continuous")]
        internal static partial int MatIsContinuous(IntPtr mat, out int isContinuous);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mat_is_submatrix")]
        internal static partial int MatIsSubmatrix(IntPtr mat, out int isSubmatrix);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_add")]
        internal static partial int CoreAdd(IntPtr src1, IntPtr src2, IntPtr dst, IntPtr mask, int dtype);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_add_scalar")]
        internal static partial int CoreAddScalar(IntPtr src, double v0, double v1, double v2, double v3, IntPtr dst, IntPtr mask, int dtype);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_subtract")]
        internal static partial int CoreSubtract(IntPtr src1, IntPtr src2, IntPtr dst, IntPtr mask, int dtype);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_subtract_scalar")]
        internal static partial int CoreSubtractScalar(IntPtr src, double v0, double v1, double v2, double v3, IntPtr dst, IntPtr mask, int dtype);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_multiply")]
        internal static partial int CoreMultiply(IntPtr src1, IntPtr src2, IntPtr dst, double scale, int dtype);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_divide")]
        internal static partial int CoreDivide(IntPtr src1, IntPtr src2, IntPtr dst, double scale, int dtype);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_scale_add")]
        internal static partial int CoreScaleAdd(IntPtr src1, double alpha, IntPtr src2, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_add_weighted")]
        internal static partial int CoreAddWeighted(IntPtr src1, double alpha, IntPtr src2, double beta, double gamma, IntPtr dst, int dtype);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_absdiff")]
        internal static partial int CoreAbsDiff(IntPtr src1, IntPtr src2, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_absdiff_scalar")]
        internal static partial int CoreAbsDiffScalar(IntPtr src, double v0, double v1, double v2, double v3, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_bitwise_and")]
        internal static partial int CoreBitwiseAnd(IntPtr src1, IntPtr src2, IntPtr dst, IntPtr mask);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_bitwise_or")]
        internal static partial int CoreBitwiseOr(IntPtr src1, IntPtr src2, IntPtr dst, IntPtr mask);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_bitwise_xor")]
        internal static partial int CoreBitwiseXor(IntPtr src1, IntPtr src2, IntPtr dst, IntPtr mask);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_bitwise_not")]
        internal static partial int CoreBitwiseNot(IntPtr src, IntPtr dst, IntPtr mask);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_compare")]
        internal static partial int CoreCompare(IntPtr src1, IntPtr src2, IntPtr dst, int cmpop);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_min")]
        internal static partial int CoreMin(IntPtr src1, IntPtr src2, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_max")]
        internal static partial int CoreMax(IntPtr src1, IntPtr src2, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_in_range")]
        internal static partial int CoreInRange(IntPtr src, double lowerV0, double lowerV1, double lowerV2, double lowerV3, double upperV0, double upperV1, double upperV2, double upperV3, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_patch_nans")]
        internal static partial int CorePatchNaNs(IntPtr src, double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_cube_root")]
        internal static partial int CoreCubeRoot(float value, out float result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_fast_atan2")]
        internal static partial int CoreFastAtan2(float y, float x, out float degrees);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_batch_distance")]
        internal static partial int CoreBatchDistance(IntPtr src1, IntPtr src2, IntPtr distances, int dtype, IntPtr indices, int normType, int k, IntPtr mask, int update, int crosscheck);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_count_non_zero")]
        internal static partial int CoreCountNonZero(IntPtr src, out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_mean")]
        internal static partial int CoreMean(IntPtr src, IntPtr mask, double[] values, int valuesLength);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_mean")]
        internal static unsafe partial int CoreMeanPtr(IntPtr src, IntPtr mask, double* values, int valuesLength);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_mean_std_dev")]
        internal static partial int CoreMeanStdDev(IntPtr src, IntPtr mask, double[] mean, int meanLength, double[] stddev, int stddevLength);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_mean_std_dev")]
        internal static unsafe partial int CoreMeanStdDevPtr(IntPtr src, IntPtr mask, double* mean, int meanLength, double* stddev, int stddevLength);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_min_max_loc")]
        internal static partial int CoreMinMaxLoc(IntPtr src, IntPtr mask, out double minVal, out double maxVal, out int minX, out int minY, out int maxX, out int maxY);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_norm")]
        internal static partial int CoreNorm(IntPtr src1, int normType, IntPtr mask, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_norm_diff")]
        internal static partial int CoreNormDiff(IntPtr src1, IntPtr src2, int normType, IntPtr mask, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_normalize")]
        internal static partial int CoreNormalize(IntPtr src, IntPtr dst, double alpha, double beta, int normType, int dtype, IntPtr mask);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_reduce")]
        internal static partial int CoreReduce(IntPtr src, IntPtr dst, int dim, int rtype, int dtype);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_sum")]
        internal static partial int CoreSum(IntPtr src, double[] values, int valuesLength);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_sum")]
        internal static unsafe partial int CoreSumPtr(IntPtr src, double* values, int valuesLength);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_trace")]
        internal static partial int CoreTrace(IntPtr src, double[] values, int valuesLength);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_trace")]
        internal static unsafe partial int CoreTracePtr(IntPtr src, double* values, int valuesLength);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_determinant")]
        internal static partial int CoreDeterminant(IntPtr src, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_invert")]
        internal static partial int CoreInvert(IntPtr src, IntPtr dst, int flags, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_solve")]
        internal static partial int CoreSolve(IntPtr src1, IntPtr src2, IntPtr dst, int flags, out int success);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_mahalanobis")]
        internal static partial int CoreMahalanobis(IntPtr v1, IntPtr v2, IntPtr icovar, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_split_count")]
        internal static partial int CoreSplitCount(IntPtr src, out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_split_fill")]
        internal static partial int CoreSplitFill(IntPtr src, IntPtr[] dst, int dstCapacity, out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_split_fill")]
        internal static unsafe partial int CoreSplitFillPtr(IntPtr src, IntPtr* dst, int dstCapacity, out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_merge")]
        internal static partial int CoreMerge(IntPtr[] src, int srcCount, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_merge")]
        internal static unsafe partial int CoreMergePtr(IntPtr* src, int srcCount, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_hconcat")]
        internal static partial int CoreHConcat(IntPtr[] src, int srcCount, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_hconcat")]
        internal static unsafe partial int CoreHConcatPtr(IntPtr* src, int srcCount, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_vconcat")]
        internal static partial int CoreVConcat(IntPtr[] src, int srcCount, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_vconcat")]
        internal static unsafe partial int CoreVConcatPtr(IntPtr* src, int srcCount, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_extract_channel")]
        internal static partial int CoreExtractChannel(IntPtr src, IntPtr dst, int coi);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_insert_channel")]
        internal static partial int CoreInsertChannel(IntPtr src, IntPtr dst, int coi);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_mix_channels")]
        internal static partial int CoreMixChannels(IntPtr[] src, int srcCount, IntPtr[] dst, int dstCount, int[] fromTo, int pairCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_mix_channels")]
        internal static unsafe partial int CoreMixChannelsPtr(IntPtr* src, int srcCount, IntPtr* dst, int dstCount, int* fromTo, int pairCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_repeat")]
        internal static partial int CoreRepeat(IntPtr src, int ny, int nx, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_flip")]
        internal static partial int CoreFlip(IntPtr src, IntPtr dst, int flipCode);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_rotate")]
        internal static partial int CoreRotate(IntPtr src, IntPtr dst, int rotateCode);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_transpose")]
        internal static partial int CoreTranspose(IntPtr src, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_lut")]
        internal static partial int CoreLut(IntPtr src, IntPtr lut, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_convert_scale_abs")]
        internal static partial int CoreConvertScaleAbs(IntPtr src, IntPtr dst, double alpha, double beta);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_complete_symm")]
        internal static partial int CoreCompleteSymm(IntPtr mat, int lowerToUpper);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_set_identity")]
        internal static partial int CoreSetIdentity(IntPtr mat, double v0, double v1, double v2, double v3);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_kmeans")]
        internal static partial int CoreKMeans(IntPtr data, int k, IntPtr bestLabels, int criteriaType, int criteriaMaxCount, double criteriaEpsilon, int attempts, int flags, IntPtr centers, out double compactness);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_imencode", StringMarshalling = StringMarshalling.Utf8)]
        internal static partial int ImgCodecsImEncode(string ext, IntPtr image, out IntPtr buffer);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_imencode_with_params", StringMarshalling = StringMarshalling.Utf8)]
        internal static partial int ImgCodecsImEncodeWithParams(string ext, IntPtr image, int[] parameters, UIntPtr parametersLength, out IntPtr buffer);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_imencode_with_params", StringMarshalling = StringMarshalling.Utf8)]
        internal static unsafe partial int ImgCodecsImEncodeWithParams(string ext, IntPtr image, int* parameters, UIntPtr parametersLength, out IntPtr buffer);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_imdecode")]
        internal static partial int ImgCodecsImDecode(byte[] buffer, UIntPtr bufferLength, int flags, out IntPtr image);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_imdecode")]
        internal static unsafe partial int ImgCodecsImDecode(byte* buffer, UIntPtr bufferLength, int flags, out IntPtr image);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_imread")]
        internal static partial int ImgCodecsImRead(byte[] filename, int flags, out IntPtr image);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_imwrite")]
        internal static partial int ImgCodecsImWrite(byte[] filename, IntPtr image, out int written);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_imwrite_with_params")]
        internal static partial int ImgCodecsImWriteWithParams(byte[] filename, IntPtr image, int[] parameters, UIntPtr parametersLength, out int written);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_imwrite_with_params")]
        internal static unsafe partial int ImgCodecsImWriteWithParams(byte[] filename, IntPtr image, int* parameters, UIntPtr parametersLength, out int written);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_imread_into")]
        internal static partial int ImgCodecsImReadInto(byte[] filename, int flags, IntPtr image);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_imread_multi")]
        internal static partial int ImgCodecsImReadMulti(byte[] filename, int flags, int hasRange, int start, int count, out IntPtr images, out int success);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_imdecode_multi")]
        internal static partial int ImgCodecsImDecodeMulti(byte[] buffer, UIntPtr bufferLength, int flags, int hasRange, int start, int end, out IntPtr images, out int success);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_imwrite_multi")]
        internal static partial int ImgCodecsImWriteMulti(byte[] filename, IntPtr images, UIntPtr imageCount, IntPtr parameters, UIntPtr parametersLength, out int written);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_imencode_multi", StringMarshalling = StringMarshalling.Utf8)]
        internal static partial int ImgCodecsImEncodeMulti(string ext, IntPtr images, UIntPtr imageCount, IntPtr parameters, UIntPtr parametersLength, out IntPtr buffer);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_imcount")]
        internal static partial int ImgCodecsImCount(byte[] filename, int flags, out UIntPtr count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_have_image_reader")]
        internal static partial int ImgCodecsHaveImageReader(byte[] filename, out int available);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_have_image_writer")]
        internal static partial int ImgCodecsHaveImageWriter(byte[] filenameOrExtension, out int available);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_mat_vector_count")]
        internal static partial int ImgCodecsMatVectorCount(IntPtr images, out UIntPtr count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_mat_vector_clone_at")]
        internal static partial int ImgCodecsMatVectorCloneAt(IntPtr images, UIntPtr index, out IntPtr image);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_mat_vector_release")]
        internal static partial void ImgCodecsMatVectorRelease(IntPtr images);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_imread_with_metadata")]
        internal static partial int ImgCodecsImReadWithMetadata(byte[] filename, int flags, out IntPtr result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_imdecode_with_metadata")]
        internal static partial int ImgCodecsImDecodeWithMetadata(byte[] buffer, UIntPtr bufferLength, int flags, out IntPtr result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_imwrite_with_metadata")]
        internal static partial int ImgCodecsImWriteWithMetadata(byte[] filename, IntPtr image, IntPtr metadataTypes, IntPtr metadata, UIntPtr metadataCount, IntPtr parameters, UIntPtr parametersLength, out int written);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_imencode_with_metadata", StringMarshalling = StringMarshalling.Utf8)]
        internal static partial int ImgCodecsImEncodeWithMetadata(string ext, IntPtr image, IntPtr metadataTypes, IntPtr metadata, UIntPtr metadataCount, IntPtr parameters, UIntPtr parametersLength, out IntPtr buffer);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_metadata_result_image_clone")]
        internal static partial int ImgCodecsMetadataResultImageClone(IntPtr result, out IntPtr image);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_metadata_result_count")]
        internal static partial int ImgCodecsMetadataResultCount(IntPtr result, out UIntPtr count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_metadata_result_clone_at")]
        internal static partial int ImgCodecsMetadataResultCloneAt(IntPtr result, UIntPtr index, out int type, out IntPtr metadata);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_metadata_result_release")]
        internal static partial void ImgCodecsMetadataResultRelease(IntPtr result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_animation_create")]
        internal static partial int ImgCodecsAnimationCreate(int loopCount, double bg0, double bg1, double bg2, double bg3, out IntPtr animation);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_animation_release")]
        internal static partial void ImgCodecsAnimationRelease(IntPtr animation);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_animation_get_loop_count")]
        internal static partial int ImgCodecsAnimationGetLoopCount(IntPtr animation, out int loopCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_animation_set_loop_count")]
        internal static partial int ImgCodecsAnimationSetLoopCount(IntPtr animation, int loopCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_animation_get_background_color")]
        internal static partial int ImgCodecsAnimationGetBackgroundColor(IntPtr animation, out double bg0, out double bg1, out double bg2, out double bg3);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_animation_set_background_color")]
        internal static partial int ImgCodecsAnimationSetBackgroundColor(IntPtr animation, double bg0, double bg1, double bg2, double bg3);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_animation_set_frames")]
        internal static partial int ImgCodecsAnimationSetFrames(IntPtr animation, IntPtr frames, IntPtr durations, UIntPtr frameCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_animation_frame_count")]
        internal static partial int ImgCodecsAnimationFrameCount(IntPtr animation, out UIntPtr count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_animation_frame_clone_at")]
        internal static partial int ImgCodecsAnimationFrameCloneAt(IntPtr animation, UIntPtr index, out IntPtr frame, out int duration);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_animation_still_image_clone")]
        internal static partial int ImgCodecsAnimationStillImageClone(IntPtr animation, out IntPtr image);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_animation_set_still_image")]
        internal static partial int ImgCodecsAnimationSetStillImage(IntPtr animation, IntPtr image);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_imread_animation")]
        internal static partial int ImgCodecsImReadAnimation(byte[] filename, int start, int count, IntPtr animation, out int success);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_imdecode_animation")]
        internal static partial int ImgCodecsImDecodeAnimation(byte[] buffer, UIntPtr bufferLength, int start, int count, IntPtr animation, out int success);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_imwrite_animation")]
        internal static partial int ImgCodecsImWriteAnimation(byte[] filename, IntPtr animation, IntPtr parameters, UIntPtr parametersLength, out int written);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_imencode_animation", StringMarshalling = StringMarshalling.Utf8)]
        internal static partial int ImgCodecsImEncodeAnimation(string ext, IntPtr animation, IntPtr parameters, UIntPtr parametersLength, out IntPtr buffer);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_image_collection_create")]
        internal static partial int ImgCodecsImageCollectionCreate(out IntPtr collection);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_image_collection_create_file")]
        internal static partial int ImgCodecsImageCollectionCreateFile(byte[] filename, int flags, out IntPtr collection);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_image_collection_release")]
        internal static partial void ImgCodecsImageCollectionRelease(IntPtr collection);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_image_collection_init")]
        internal static partial int ImgCodecsImageCollectionInit(IntPtr collection, byte[] filename, int flags);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_image_collection_size")]
        internal static partial int ImgCodecsImageCollectionSize(IntPtr collection, out UIntPtr size);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_image_collection_clone_at")]
        internal static partial int ImgCodecsImageCollectionCloneAt(IntPtr collection, int index, out IntPtr image);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgcodecs_image_collection_release_cache")]
        internal static partial int ImgCodecsImageCollectionReleaseCache(IntPtr collection, int index);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_encoded_buffer_size")]
        internal static partial int EncodedBufferSize(IntPtr buffer, out UIntPtr size);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_encoded_buffer_data")]
        internal static partial int EncodedBufferData(IntPtr buffer, out IntPtr data);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_encoded_buffer_release")]
        internal static partial void EncodedBufferRelease(IntPtr buffer);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_cvt_color")]
        internal static partial int ImgProcCvtColor(IntPtr src, IntPtr dst, int code, int dstCn);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_resize")]
        internal static partial int ImgProcResize(
            IntPtr src,
            IntPtr dst,
            int width,
            int height,
            double fx,
            double fy,
            int interpolation);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_threshold")]
        internal static partial int ImgProcThreshold(
            IntPtr src,
            IntPtr dst,
            double thresh,
            double maxval,
            int type,
            out double threshold);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_adaptive_threshold")]
        internal static partial int ImgProcAdaptiveThreshold(
            IntPtr src,
            IntPtr dst,
            double maxValue,
            int adaptiveMethod,
            int thresholdType,
            int blockSize,
            double c);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_integral")]
        internal static partial int ImgProcIntegral(
            IntPtr src,
            IntPtr sum,
            int sdepth);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_integral2")]
        internal static partial int ImgProcIntegral2(
            IntPtr src,
            IntPtr sum,
            IntPtr sqsum,
            int sdepth,
            int sqdepth);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_integral3")]
        internal static partial int ImgProcIntegral3(
            IntPtr src,
            IntPtr sum,
            IntPtr sqsum,
            IntPtr tilted,
            int sdepth,
            int sqdepth);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_distance_transform")]
        internal static partial int ImgProcDistanceTransform(
            IntPtr src,
            IntPtr dst,
            int distanceType,
            int maskSize,
            int dstType);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_distance_transform_with_labels")]
        internal static partial int ImgProcDistanceTransformWithLabels(
            IntPtr src,
            IntPtr dst,
            IntPtr labels,
            int distanceType,
            int maskSize,
            int labelType);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_flood_fill")]
        internal static partial int ImgProcFloodFill(
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

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_flood_fill_mask")]
        internal static partial int ImgProcFloodFillMask(
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

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_connected_components")]
        internal static partial int ImgProcConnectedComponents(
            IntPtr image,
            IntPtr labels,
            int connectivity,
            int ltype,
            out int labelCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_connected_components_with_algorithm")]
        internal static partial int ImgProcConnectedComponentsWithAlgorithm(
            IntPtr image,
            IntPtr labels,
            int connectivity,
            int ltype,
            int ccltype,
            out int labelCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_connected_components_with_stats")]
        internal static partial int ImgProcConnectedComponentsWithStats(
            IntPtr image,
            IntPtr labels,
            IntPtr stats,
            IntPtr centroids,
            int connectivity,
            int ltype,
            out int labelCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_connected_components_with_stats_with_algorithm")]
        internal static partial int ImgProcConnectedComponentsWithStatsWithAlgorithm(
            IntPtr image,
            IntPtr labels,
            IntPtr stats,
            IntPtr centroids,
            int connectivity,
            int ltype,
            int ccltype,
            out int labelCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_equalize_hist")]
        internal static partial int ImgProcEqualizeHist(
            IntPtr src,
            IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_corner_harris")]
        internal static partial int ImgProcCornerHarris(
            IntPtr src,
            IntPtr dst,
            int blockSize,
            int ksize,
            double k,
            int borderType);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_corner_min_eigen_val")]
        internal static partial int ImgProcCornerMinEigenVal(
            IntPtr src,
            IntPtr dst,
            int blockSize,
            int ksize,
            int borderType);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_corner_eigen_vals_and_vecs")]
        internal static partial int ImgProcCornerEigenValsAndVecs(
            IntPtr src,
            IntPtr dst,
            int blockSize,
            int ksize,
            int borderType);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_pre_corner_detect")]
        internal static partial int ImgProcPreCornerDetect(
            IntPtr src,
            IntPtr dst,
            int ksize,
            int borderType);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_gaussian_blur")]
        internal static partial int ImgProcGaussianBlur(
            IntPtr src,
            IntPtr dst,
            int width,
            int height,
            double sigmaX,
            double sigmaY,
            int borderType);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_blur")]
        internal static partial int ImgProcBlur(
            IntPtr src,
            IntPtr dst,
            int width,
            int height,
            int anchorX,
            int anchorY,
            int borderType);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_box_filter")]
        internal static partial int ImgProcBoxFilter(
            IntPtr src,
            IntPtr dst,
            int ddepth,
            int width,
            int height,
            int anchorX,
            int anchorY,
            int normalize,
            int borderType);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_sqr_box_filter")]
        internal static partial int ImgProcSqrBoxFilter(
            IntPtr src,
            IntPtr dst,
            int ddepth,
            int width,
            int height,
            int anchorX,
            int anchorY,
            int normalize,
            int borderType);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_median_blur")]
        internal static partial int ImgProcMedianBlur(
            IntPtr src,
            IntPtr dst,
            int ksize);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_bilateral_filter")]
        internal static partial int ImgProcBilateralFilter(
            IntPtr src,
            IntPtr dst,
            int d,
            double sigmaColor,
            double sigmaSpace,
            int borderType);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_filter2d")]
        internal static partial int ImgProcFilter2D(
            IntPtr src,
            IntPtr dst,
            int ddepth,
            IntPtr kernel,
            int anchorX,
            int anchorY,
            double delta,
            int borderType);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_sep_filter2d")]
        internal static partial int ImgProcSepFilter2D(
            IntPtr src,
            IntPtr dst,
            int ddepth,
            IntPtr kernelX,
            IntPtr kernelY,
            int anchorX,
            int anchorY,
            double delta,
            int borderType);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_sobel")]
        internal static partial int ImgProcSobel(
            IntPtr src,
            IntPtr dst,
            int ddepth,
            int dx,
            int dy,
            int ksize,
            double scale,
            double delta,
            int borderType);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_scharr")]
        internal static partial int ImgProcScharr(
            IntPtr src,
            IntPtr dst,
            int ddepth,
            int dx,
            int dy,
            double scale,
            double delta,
            int borderType);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_laplacian")]
        internal static partial int ImgProcLaplacian(
            IntPtr src,
            IntPtr dst,
            int ddepth,
            int ksize,
            double scale,
            double delta,
            int borderType);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_canny")]
        internal static partial int ImgProcCanny(
            IntPtr image,
            IntPtr edges,
            double threshold1,
            double threshold2,
            int apertureSize,
            int l2Gradient);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_canny_derivatives")]
        internal static partial int ImgProcCannyDerivatives(
            IntPtr dx,
            IntPtr dy,
            IntPtr edges,
            double threshold1,
            double threshold2,
            int l2Gradient);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_get_gaussian_kernel")]
        internal static partial int ImgProcGetGaussianKernel(
            int ksize,
            double sigma,
            int ktype,
            out IntPtr kernel);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_get_deriv_kernels")]
        internal static partial int ImgProcGetDerivKernels(
            IntPtr kx,
            IntPtr ky,
            int dx,
            int dy,
            int ksize,
            int normalize,
            int ktype);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_get_gabor_kernel")]
        internal static partial int ImgProcGetGaborKernel(
            int width,
            int height,
            double sigma,
            double theta,
            double lambd,
            double gamma,
            double psi,
            int ktype,
            out IntPtr kernel);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_pyr_down")]
        internal static partial int ImgProcPyrDown(
            IntPtr src,
            IntPtr dst,
            int width,
            int height,
            int borderType);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_pyr_up")]
        internal static partial int ImgProcPyrUp(
            IntPtr src,
            IntPtr dst,
            int width,
            int height,
            int borderType);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_warp_affine")]
        internal static partial int ImgProcWarpAffine(
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

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_warp_perspective")]
        internal static partial int ImgProcWarpPerspective(
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

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_get_rotation_matrix2d")]
        internal static partial int ImgProcGetRotationMatrix2D(
            float centerX,
            float centerY,
            double angle,
            double scale,
            out IntPtr transform);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_get_affine_transform")]
        internal static unsafe partial int ImgProcGetAffineTransform(
            float* srcXy,
            float* dstXy,
            out IntPtr transform);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_get_perspective_transform")]
        internal static unsafe partial int ImgProcGetPerspectiveTransform(
            float* srcXy,
            float* dstXy,
            int solveMethod,
            out IntPtr transform);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_invert_affine_transform")]
        internal static partial int ImgProcInvertAffineTransform(
            IntPtr transform,
            IntPtr inverseTransform);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_remap")]
        internal static partial int ImgProcRemap(
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

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_convert_maps")]
        internal static partial int ImgProcConvertMaps(
            IntPtr map1,
            IntPtr map2,
            IntPtr dstmap1,
            IntPtr dstmap2,
            int dstmap1type,
            int nninterpolation);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_get_structuring_element")]
        internal static partial int ImgProcGetStructuringElement(
            int shape,
            int width,
            int height,
            int anchorX,
            int anchorY,
            out IntPtr element);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_erode")]
        internal static partial int ImgProcErode(
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

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_dilate")]
        internal static partial int ImgProcDilate(
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

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_morphology_ex")]
        internal static partial int ImgProcMorphologyEx(
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

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_line")]
        internal static partial int ImgProcLine(
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

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_arrowed_line")]
        internal static partial int ImgProcArrowedLine(
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

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_clip_line_rect")]
        internal static partial int ImgProcClipLineRect(
            int rectX,
            int rectY,
            int rectWidth,
            int rectHeight,
            ref int pt1X,
            ref int pt1Y,
            ref int pt2X,
            ref int pt2Y,
            out int intersects);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_polylines")]
        internal static partial int ImgProcPolylines(
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

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_fill_poly")]
        internal static partial int ImgProcFillPoly(
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

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_ellipse2_poly_count")]
        internal static partial int ImgProcEllipse2PolyCount(
            int centerX,
            int centerY,
            int axesWidth,
            int axesHeight,
            int angle,
            int arcStart,
            int arcEnd,
            int delta,
            out int pointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_ellipse2_poly_fill")]
        internal static partial int ImgProcEllipse2PolyFill(
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

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_contour_area")]
        internal static partial int ImgProcContourArea(
            int[] pointsXy,
            int pointCount,
            int oriented,
            out double area);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_contour_area")]
        internal static unsafe partial int ImgProcContourAreaPtr(
            int* pointsXy,
            int pointCount,
            int oriented,
            out double area);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_find_contours_count")]
        internal static partial int ImgProcFindContoursCount(
            IntPtr image,
            int mode,
            int method,
            int offsetX,
            int offsetY,
            out int contourCount,
            out int totalPointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_find_contours_fill")]
        internal static partial int ImgProcFindContoursFill(
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

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_draw_contours")]
        internal static partial int ImgProcDrawContours(
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

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_moments_points")]
        internal static partial int ImgProcMomentsPoints(
            int[] pointsXy,
            int pointCount,
            int binaryImage,
            double[] values,
            int valueCapacity);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_moments_points")]
        internal static unsafe partial int ImgProcMomentsPointsPtr(
            int* pointsXy,
            int pointCount,
            int binaryImage,
            double* values,
            int valueCapacity);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_moments_mat")]
        internal static partial int ImgProcMomentsMat(
            IntPtr array,
            int binaryImage,
            double[] values,
            int valueCapacity);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_hu_moments")]
        internal static partial int ImgProcHuMoments(
            double[] momentsValues,
            int valueCount,
            double[] huValues,
            int huCapacity);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_arc_length")]
        internal static partial int ImgProcArcLength(
            int[] pointsXy,
            int pointCount,
            int closed,
            out double length);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_arc_length")]
        internal static unsafe partial int ImgProcArcLengthPtr(
            int* pointsXy,
            int pointCount,
            int closed,
            out double length);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_approx_poly_dp_count")]
        internal static partial int ImgProcApproxPolyDPCount(
            int[] curveXy,
            int pointCount,
            double epsilon,
            int closed,
            out int approxPointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_approx_poly_dp_count")]
        internal static unsafe partial int ImgProcApproxPolyDPCountPtr(
            int* curveXy,
            int pointCount,
            double epsilon,
            int closed,
            out int approxPointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_approx_poly_dp_fill")]
        internal static partial int ImgProcApproxPolyDPFill(
            int[] curveXy,
            int pointCount,
            double epsilon,
            int closed,
            int[] approxPointsXy,
            int approxPointCapacity,
            out int approxPointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_approx_poly_dp_fill")]
        internal static unsafe partial int ImgProcApproxPolyDPFillPtr(
            int* curveXy,
            int pointCount,
            double epsilon,
            int closed,
            int* approxPointsXy,
            int approxPointCapacity,
            out int approxPointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_approx_poly_n_count")]
        internal static partial int ImgProcApproxPolyNCount(
            int[] curveXy,
            int pointCount,
            int nsides,
            float epsilonPercentage,
            int ensureConvex,
            out int approxPointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_approx_poly_n_count")]
        internal static unsafe partial int ImgProcApproxPolyNCountPtr(
            int* curveXy,
            int pointCount,
            int nsides,
            float epsilonPercentage,
            int ensureConvex,
            out int approxPointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_approx_poly_n_fill")]
        internal static partial int ImgProcApproxPolyNFill(
            int[] curveXy,
            int pointCount,
            int nsides,
            float epsilonPercentage,
            int ensureConvex,
            float[] approxPointsXy,
            int approxPointCapacity,
            out int approxPointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_approx_poly_n_fill")]
        internal static unsafe partial int ImgProcApproxPolyNFillPtr(
            int* curveXy,
            int pointCount,
            int nsides,
            float epsilonPercentage,
            int ensureConvex,
            float* approxPointsXy,
            int approxPointCapacity,
            out int approxPointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_bounding_rect")]
        internal static partial int ImgProcBoundingRect(
            int[] pointsXy,
            int pointCount,
            out int x,
            out int y,
            out int width,
            out int height);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_bounding_rect")]
        internal static unsafe partial int ImgProcBoundingRectPtr(
            int* pointsXy,
            int pointCount,
            out int x,
            out int y,
            out int width,
            out int height);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_is_contour_convex")]
        internal static partial int ImgProcIsContourConvex(
            int[] pointsXy,
            int pointCount,
            out int isConvex);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_is_contour_convex")]
        internal static unsafe partial int ImgProcIsContourConvexPtr(
            int* pointsXy,
            int pointCount,
            out int isConvex);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_convex_hull_count")]
        internal static partial int ImgProcConvexHullCount(
            int[] pointsXy,
            int pointCount,
            int clockwise,
            out int hullPointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_convex_hull_count")]
        internal static unsafe partial int ImgProcConvexHullCountPtr(
            int* pointsXy,
            int pointCount,
            int clockwise,
            out int hullPointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_convex_hull_fill")]
        internal static partial int ImgProcConvexHullFill(
            int[] pointsXy,
            int pointCount,
            int clockwise,
            int[] hullPointsXy,
            int hullPointCapacity,
            out int hullPointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_convex_hull_fill")]
        internal static unsafe partial int ImgProcConvexHullFillPtr(
            int* pointsXy,
            int pointCount,
            int clockwise,
            int* hullPointsXy,
            int hullPointCapacity,
            out int hullPointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_convex_hull_indices_count")]
        internal static partial int ImgProcConvexHullIndicesCount(
            int[] pointsXy,
            int pointCount,
            int clockwise,
            out int hullIndexCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_convex_hull_indices_count")]
        internal static unsafe partial int ImgProcConvexHullIndicesCountPtr(
            int* pointsXy,
            int pointCount,
            int clockwise,
            out int hullIndexCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_convex_hull_indices_fill")]
        internal static partial int ImgProcConvexHullIndicesFill(
            int[] pointsXy,
            int pointCount,
            int clockwise,
            int[] hullIndices,
            int hullIndexCapacity,
            out int hullIndexCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_convex_hull_indices_fill")]
        internal static unsafe partial int ImgProcConvexHullIndicesFillPtr(
            int* pointsXy,
            int pointCount,
            int clockwise,
            int* hullIndices,
            int hullIndexCapacity,
            out int hullIndexCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_convexity_defects_count")]
        internal static partial int ImgProcConvexityDefectsCount(
            int[] contourXy,
            int contourPointCount,
            int[] hullIndices,
            int hullIndexCount,
            out int defectCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_convexity_defects_fill")]
        internal static partial int ImgProcConvexityDefectsFill(
            int[] contourXy,
            int contourPointCount,
            int[] hullIndices,
            int hullIndexCount,
            int[] defects,
            int defectCapacity,
            out int defectCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_min_enclosing_circle")]
        internal static partial int ImgProcMinEnclosingCircle(
            int[] pointsXy,
            int pointCount,
            out float centerX,
            out float centerY,
            out float radius);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_min_enclosing_circle")]
        internal static unsafe partial int ImgProcMinEnclosingCirclePtr(
            int* pointsXy,
            int pointCount,
            out float centerX,
            out float centerY,
            out float radius);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_point_polygon_test")]
        internal static partial int ImgProcPointPolygonTest(
            int[] contourXy,
            int pointCount,
            float pointX,
            float pointY,
            int measureDist,
            out double result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_point_polygon_test")]
        internal static unsafe partial int ImgProcPointPolygonTestPtr(
            int* contourXy,
            int pointCount,
            float pointX,
            float pointY,
            int measureDist,
            out double result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_match_shapes")]
        internal static partial int ImgProcMatchShapes(
            int[] contour1Xy,
            int contour1PointCount,
            int[] contour2Xy,
            int contour2PointCount,
            int method,
            double parameter,
            out double result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_match_shapes")]
        internal static unsafe partial int ImgProcMatchShapesPtr(
            int* contour1Xy,
            int contour1PointCount,
            int* contour2Xy,
            int contour2PointCount,
            int method,
            double parameter,
            out double result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_min_area_rect")]
        internal static partial int ImgProcMinAreaRect(
            int[] pointsXy,
            int pointCount,
            out float centerX,
            out float centerY,
            out float width,
            out float height,
            out float angle);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_min_area_rect")]
        internal static unsafe partial int ImgProcMinAreaRectPtr(
            int* pointsXy,
            int pointCount,
            out float centerX,
            out float centerY,
            out float width,
            out float height,
            out float angle);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_box_points")]
        internal static partial int ImgProcBoxPoints(
            float centerX,
            float centerY,
            float width,
            float height,
            float angle,
            float[] pointsXy,
            int pointCapacity);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_fit_ellipse")]
        internal static partial int ImgProcFitEllipse(
            int[] pointsXy,
            int pointCount,
            out float centerX,
            out float centerY,
            out float width,
            out float height,
            out float angle);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_fit_ellipse")]
        internal static unsafe partial int ImgProcFitEllipsePtr(
            int* pointsXy,
            int pointCount,
            out float centerX,
            out float centerY,
            out float width,
            out float height,
            out float angle);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_fit_ellipse_ams")]
        internal static partial int ImgProcFitEllipseAMS(
            int[] pointsXy,
            int pointCount,
            out float centerX,
            out float centerY,
            out float width,
            out float height,
            out float angle);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_fit_ellipse_ams")]
        internal static unsafe partial int ImgProcFitEllipseAMSPtr(
            int* pointsXy,
            int pointCount,
            out float centerX,
            out float centerY,
            out float width,
            out float height,
            out float angle);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_fit_ellipse_direct")]
        internal static partial int ImgProcFitEllipseDirect(
            int[] pointsXy,
            int pointCount,
            out float centerX,
            out float centerY,
            out float width,
            out float height,
            out float angle);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_fit_ellipse_direct")]
        internal static unsafe partial int ImgProcFitEllipseDirectPtr(
            int* pointsXy,
            int pointCount,
            out float centerX,
            out float centerY,
            out float width,
            out float height,
            out float angle);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_rotated_rectangle_intersection_count")]
        internal static partial int ImgProcRotatedRectangleIntersectionCount(
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

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_rotated_rectangle_intersection_fill")]
        internal static partial int ImgProcRotatedRectangleIntersectionFill(
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

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_get_closest_ellipse_points")]
        internal static partial int ImgProcGetClosestEllipsePoints(
            float centerX,
            float centerY,
            float width,
            float height,
            float angle,
            int[] pointsXy,
            int pointCount,
            float[] closestPointsXy,
            int closestPointCapacity);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_get_closest_ellipse_points")]
        internal static unsafe partial int ImgProcGetClosestEllipsePointsPtr(
            float centerX,
            float centerY,
            float width,
            float height,
            float angle,
            int* pointsXy,
            int pointCount,
            float* closestPointsXy,
            int closestPointCapacity);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_min_enclosing_triangle")]
        internal static partial int ImgProcMinEnclosingTriangle(
            int[] pointsXy,
            int pointCount,
            float[] trianglePointsXy,
            int trianglePointCapacity,
            out double area);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_min_enclosing_triangle")]
        internal static unsafe partial int ImgProcMinEnclosingTrianglePtr(
            int* pointsXy,
            int pointCount,
            float* trianglePointsXy,
            int trianglePointCapacity,
            out double area);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_min_enclosing_convex_polygon")]
        internal static partial int ImgProcMinEnclosingConvexPolygon(
            int[] pointsXy,
            int pointCount,
            int k,
            float[] polygonPointsXy,
            int polygonPointCapacity,
            out int polygonPointCount,
            out double area);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_min_enclosing_convex_polygon")]
        internal static unsafe partial int ImgProcMinEnclosingConvexPolygonPtr(
            int* pointsXy,
            int pointCount,
            int k,
            float* polygonPointsXy,
            int polygonPointCapacity,
            out int polygonPointCount,
            out double area);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_intersect_convex_convex_count")]
        internal static partial int ImgProcIntersectConvexConvexCount(
            int[] polygon1Xy,
            int polygon1PointCount,
            int[] polygon2Xy,
            int polygon2PointCount,
            int handleNested,
            out float area,
            out int intersectingPointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_intersect_convex_convex_count")]
        internal static unsafe partial int ImgProcIntersectConvexConvexCountPtr(
            int* polygon1Xy,
            int polygon1PointCount,
            int* polygon2Xy,
            int polygon2PointCount,
            int handleNested,
            out float area,
            out int intersectingPointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_intersect_convex_convex_fill")]
        internal static partial int ImgProcIntersectConvexConvexFill(
            int[] polygon1Xy,
            int polygon1PointCount,
            int[] polygon2Xy,
            int polygon2PointCount,
            int handleNested,
            float[] intersectingPointsXy,
            int intersectingPointCapacity,
            out float area,
            out int intersectingPointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_intersect_convex_convex_fill")]
        internal static unsafe partial int ImgProcIntersectConvexConvexFillPtr(
            int* polygon1Xy,
            int polygon1PointCount,
            int* polygon2Xy,
            int polygon2PointCount,
            int handleNested,
            float* intersectingPointsXy,
            int intersectingPointCapacity,
            out float area,
            out int intersectingPointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_fit_line_2d")]
        internal static partial int ImgProcFitLine2D(
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

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_fit_line_2d")]
        internal static unsafe partial int ImgProcFitLine2DPtr(
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

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_rectangle")]
        internal static partial int ImgProcRectangle(
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

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_rectangle_by_rect")]
        internal static partial int ImgProcRectangleByRect(
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

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_circle")]
        internal static partial int ImgProcCircle(
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

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_ellipse")]
        internal static partial int ImgProcEllipse(
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

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_put_text")]
        internal static partial int ImgProcPutText(
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

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_get_text_size")]
        internal static partial int ImgProcGetTextSize(
            byte[] text,
            int fontFace,
            double fontScale,
            int thickness,
            out int width,
            out int height,
            out int baseLine);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_clahe_create")]
        internal static partial int ImgProcClaheCreate(double clipLimit, int tilesGridWidth, int tilesGridHeight, out IntPtr clahe);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_clahe_release")]
        internal static partial void ImgProcClaheRelease(IntPtr clahe);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_clahe_apply")]
        internal static partial int ImgProcClaheApply(IntPtr clahe, IntPtr src, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_clahe_get_clip_limit")]
        internal static partial int ImgProcClaheGetClipLimit(IntPtr clahe, out double clipLimit);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_clahe_set_clip_limit")]
        internal static partial int ImgProcClaheSetClipLimit(IntPtr clahe, double clipLimit);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_clahe_get_tiles_grid_size")]
        internal static partial int ImgProcClaheGetTilesGridSize(IntPtr clahe, out int width, out int height);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_clahe_set_tiles_grid_size")]
        internal static partial int ImgProcClaheSetTilesGridSize(IntPtr clahe, int width, int height);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_clahe_get_bit_shift")]
        internal static partial int ImgProcClaheGetBitShift(IntPtr clahe, out int bitShift);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_clahe_set_bit_shift")]
        internal static partial int ImgProcClaheSetBitShift(IntPtr clahe, int bitShift);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_clahe_collect_garbage")]
        internal static partial int ImgProcClaheCollectGarbage(IntPtr clahe);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_corner_sub_pix")]
        internal static partial int ImgProcCornerSubPix(IntPtr image, float[] cornersXy, int cornerCount, int winWidth, int winHeight, int zeroZoneWidth, int zeroZoneHeight, int criteriaType, int criteriaMaxCount, double criteriaEpsilon);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_corner_sub_pix")]
        internal static unsafe partial int ImgProcCornerSubPixPtr(IntPtr image, float* cornersXy, int cornerCount, int winWidth, int winHeight, int zeroZoneWidth, int zeroZoneHeight, int criteriaType, int criteriaMaxCount, double criteriaEpsilon);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_good_features_to_track_count")]
        internal static partial int ImgProcGoodFeaturesToTrackCount(IntPtr image, IntPtr mask, int maxCorners, double qualityLevel, double minDistance, int blockSize, int gradientSize, int useHarrisDetector, double k, out int cornerCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_good_features_to_track_fill")]
        internal static partial int ImgProcGoodFeaturesToTrackFill(IntPtr image, IntPtr mask, int maxCorners, double qualityLevel, double minDistance, int blockSize, int gradientSize, int useHarrisDetector, double k, float[] cornersXy, int cornerCapacity, out int cornerCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_hough_lines_count")]
        internal static partial int ImgProcHoughLinesCount(IntPtr image, double rho, double theta, int threshold, double srn, double stn, double minTheta, double maxTheta, int useEdgeval, out int lineCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_hough_lines_fill")]
        internal static partial int ImgProcHoughLinesFill(IntPtr image, double rho, double theta, int threshold, double srn, double stn, double minTheta, double maxTheta, int useEdgeval, float[] lines, int lineCapacity, out int lineCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_hough_lines_p_count")]
        internal static partial int ImgProcHoughLinesPCount(IntPtr image, double rho, double theta, int threshold, double minLineLength, double maxLineGap, out int lineCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_hough_lines_p_fill")]
        internal static partial int ImgProcHoughLinesPFill(IntPtr image, double rho, double theta, int threshold, double minLineLength, double maxLineGap, int[] lines, int lineCapacity, out int lineCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_hough_lines_point_set_count")]
        internal static partial int ImgProcHoughLinesPointSetCount(int[] pointsXy, int pointCount, int linesMax, int threshold, double minRho, double maxRho, double rhoStep, double minTheta, double maxTheta, double thetaStep, out int lineCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_hough_lines_point_set_count")]
        internal static unsafe partial int ImgProcHoughLinesPointSetCountPtr(int* pointsXy, int pointCount, int linesMax, int threshold, double minRho, double maxRho, double rhoStep, double minTheta, double maxTheta, double thetaStep, out int lineCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_hough_lines_point_set_fill")]
        internal static partial int ImgProcHoughLinesPointSetFill(int[] pointsXy, int pointCount, int linesMax, int threshold, double minRho, double maxRho, double rhoStep, double minTheta, double maxTheta, double thetaStep, double[] lines, int lineCapacity, out int lineCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_hough_lines_point_set_fill")]
        internal static unsafe partial int ImgProcHoughLinesPointSetFillPtr(int* pointsXy, int pointCount, int linesMax, int threshold, double minRho, double maxRho, double rhoStep, double minTheta, double maxTheta, double thetaStep, double[] lines, int lineCapacity, out int lineCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_hough_circles_count")]
        internal static partial int ImgProcHoughCirclesCount(IntPtr image, int method, double dp, double minDist, double param1, double param2, int minRadius, int maxRadius, out int circleCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_hough_circles_fill")]
        internal static partial int ImgProcHoughCirclesFill(IntPtr image, int method, double dp, double minDist, double param1, double param2, int minRadius, int maxRadius, float[] circles, int circleCapacity, out int circleCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_calc_hist_uniform")]
        internal static partial int ImgProcCalcHistUniform(IntPtr image, IntPtr mask, int[] channels, int channelCount, IntPtr hist, int[] histSize, int histDims, float[] ranges, int rangeCount, int accumulate);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_calc_back_project_uniform")]
        internal static partial int ImgProcCalcBackProjectUniform(IntPtr image, int[] channels, int channelCount, IntPtr hist, IntPtr backProject, float[] ranges, int rangeCount, double scale);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_compare_hist")]
        internal static partial int ImgProcCompareHist(IntPtr h1, IntPtr h2, int method, out double result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_line_segment_detector_create")]
        internal static partial int ImgProcLineSegmentDetectorCreate(int refine, double scale, double sigmaScale, double quant, double angTh, double logEps, double densityTh, int nBins, out IntPtr detector);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_line_segment_detector_release")]
        internal static partial void ImgProcLineSegmentDetectorRelease(IntPtr detector);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_line_segment_detector_detect")]
        internal static partial int ImgProcLineSegmentDetectorDetect(IntPtr detector, IntPtr image, IntPtr lines, IntPtr width, IntPtr prec, IntPtr nfa);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_line_segment_detector_detect_count")]
        internal static partial int ImgProcLineSegmentDetectorDetectCount(IntPtr detector, IntPtr image, out int lineCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_line_segment_detector_detect_fill")]
        internal static partial int ImgProcLineSegmentDetectorDetectFill(IntPtr detector, IntPtr image, float[] lines, int lineCapacity, out int lineCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_line_segment_detector_draw_segments")]
        internal static partial int ImgProcLineSegmentDetectorDrawSegments(IntPtr detector, IntPtr image, IntPtr lines);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_line_segment_detector_draw_segments_array")]
        internal static partial int ImgProcLineSegmentDetectorDrawSegmentsArray(IntPtr detector, IntPtr image, float[] lines, int lineCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_line_segment_detector_compare_segments")]
        internal static partial int ImgProcLineSegmentDetectorCompareSegments(IntPtr detector, int width, int height, IntPtr lines1, IntPtr lines2, IntPtr image, out int mismatchCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_line_segment_detector_compare_segments_array")]
        internal static partial int ImgProcLineSegmentDetectorCompareSegmentsArray(IntPtr detector, int width, int height, float[] lines1, int line1Count, float[] lines2, int line2Count, IntPtr image, out int mismatchCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_generalized_hough_ballard_create")]
        internal static partial int ImgProcGeneralizedHoughBallardCreate(out IntPtr hough);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_generalized_hough_guil_create")]
        internal static partial int ImgProcGeneralizedHoughGuilCreate(out IntPtr hough);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_generalized_hough_release")]
        internal static partial void ImgProcGeneralizedHoughRelease(IntPtr hough);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_generalized_hough_set_template")]
        internal static partial int ImgProcGeneralizedHoughSetTemplate(IntPtr hough, IntPtr templ, int centerX, int centerY);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_generalized_hough_set_template_edges")]
        internal static partial int ImgProcGeneralizedHoughSetTemplateEdges(IntPtr hough, IntPtr edges, IntPtr dx, IntPtr dy, int centerX, int centerY);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_generalized_hough_detect")]
        internal static partial int ImgProcGeneralizedHoughDetect(IntPtr hough, IntPtr image, IntPtr positions, IntPtr votes);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_generalized_hough_detect_edges")]
        internal static partial int ImgProcGeneralizedHoughDetectEdges(IntPtr hough, IntPtr edges, IntPtr dx, IntPtr dy, IntPtr positions, IntPtr votes);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_generalized_hough_get_int_property")]
        internal static partial int ImgProcGeneralizedHoughGetIntProperty(IntPtr hough, int property, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_generalized_hough_set_int_property")]
        internal static partial int ImgProcGeneralizedHoughSetIntProperty(IntPtr hough, int property, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_generalized_hough_get_double_property")]
        internal static partial int ImgProcGeneralizedHoughGetDoubleProperty(IntPtr hough, int property, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_generalized_hough_set_double_property")]
        internal static partial int ImgProcGeneralizedHoughSetDoubleProperty(IntPtr hough, int property, double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_cvt_color_two_plane")]
        internal static partial int ImgProcCvtColorTwoPlane(IntPtr src1, IntPtr src2, IntPtr dst, int code);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_demosaicing")]
        internal static partial int ImgProcDemosaicing(IntPtr src, IntPtr dst, int code, int dstCn);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_apply_color_map")]
        internal static partial int ImgProcApplyColorMap(IntPtr src, IntPtr dst, int colormap);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_apply_color_map_user")]
        internal static partial int ImgProcApplyColorMapUser(IntPtr src, IntPtr dst, IntPtr userColor);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_blend_linear")]
        internal static partial int ImgProcBlendLinear(IntPtr src1, IntPtr src2, IntPtr weights1, IntPtr weights2, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_stack_blur")]
        internal static partial int ImgProcStackBlur(IntPtr src, IntPtr dst, int width, int height);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_spatial_gradient")]
        internal static partial int ImgProcSpatialGradient(IntPtr src, IntPtr dx, IntPtr dy, int ksize, int borderType);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_threshold_with_mask")]
        internal static partial int ImgProcThresholdWithMask(IntPtr src, IntPtr dst, IntPtr mask, double thresh, double maxval, int type, out double result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_draw_marker")]
        internal static partial int ImgProcDrawMarker(IntPtr image, int positionX, int positionY, double colorV0, double colorV1, double colorV2, double colorV3, int markerType, int markerSize, int thickness, int lineType);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_fill_convex_poly")]
        internal static partial int ImgProcFillConvexPoly(IntPtr image, int[] pointsXy, int pointCount, double colorV0, double colorV1, double colorV2, double colorV3, int lineType, int shift);

#if NETCOREAPP3_1_OR_GREATER
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_fill_convex_poly")]
        internal static unsafe partial int ImgProcFillConvexPolyPtr(IntPtr image, int* pointsXy, int pointCount, double colorV0, double colorV1, double colorV2, double colorV3, int lineType, int shift);
#endif

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_get_font_scale_from_height")]
        internal static partial int ImgProcGetFontScaleFromHeight(int fontFace, int pixelHeight, int thickness, out double result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_undistort")]
        internal static partial int ImgProcUndistort(IntPtr src, IntPtr dst, IntPtr cameraMatrix, IntPtr distCoeffs, IntPtr newCameraMatrix);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_init_inverse_rectification_map")]
        internal static partial int ImgProcInitInverseRectificationMap(IntPtr cameraMatrix, IntPtr distCoeffs, IntPtr r, IntPtr newCameraMatrix, int sizeWidth, int sizeHeight, int m1type, IntPtr map1, IntPtr map2);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_fisheye_undistort_image")]
        internal static partial int ImgProcFisheyeUndistortImage(IntPtr distorted, IntPtr undistorted, IntPtr cameraMatrix, IntPtr distCoeffs, IntPtr newCameraMatrix, int newSizeWidth, int newSizeHeight);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_get_rect_sub_pix")]
        internal static partial int ImgProcGetRectSubPix(IntPtr image, int patchWidth, int patchHeight, float centerX, float centerY, IntPtr patch, int patchType);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_warp_polar")]
        internal static partial int ImgProcWarpPolar(IntPtr src, IntPtr dst, int width, int height, float centerX, float centerY, double maxRadius, int flags);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_accumulate")]
        internal static partial int ImgProcAccumulate(IntPtr src, IntPtr dst, IntPtr mask);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_accumulate_square")]
        internal static partial int ImgProcAccumulateSquare(IntPtr src, IntPtr dst, IntPtr mask);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_accumulate_product")]
        internal static partial int ImgProcAccumulateProduct(IntPtr src1, IntPtr src2, IntPtr dst, IntPtr mask);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_accumulate_weighted")]
        internal static partial int ImgProcAccumulateWeighted(IntPtr src, IntPtr dst, double alpha, IntPtr mask);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_phase_correlate")]
        internal static partial int ImgProcPhaseCorrelate(IntPtr src1, IntPtr src2, IntPtr window, out double shiftX, out double shiftY, out double response);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_phase_correlate_iterative")]
        internal static partial int ImgProcPhaseCorrelateIterative(IntPtr src1, IntPtr src2, int l2Size, int maxIters, out double shiftX, out double shiftY);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_create_hanning_window")]
        internal static partial int ImgProcCreateHanningWindow(IntPtr dst, int width, int height, int type);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_emd")]
        internal static partial int ImgProcEmd(IntPtr signature1, IntPtr signature2, int distanceType, IntPtr cost, int hasLowerBound, ref float lowerBound, IntPtr flow, out float distance);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_watershed")]
        internal static partial int ImgProcWatershed(IntPtr image, IntPtr markers);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_pyr_mean_shift_filtering")]
        internal static partial int ImgProcPyrMeanShiftFiltering(IntPtr src, IntPtr dst, double spatialRadius, double colorRadius, int maxLevel, int criteriaType, int criteriaMaxCount, double criteriaEpsilon);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_grab_cut")]
        internal static partial int ImgProcGrabCut(IntPtr image, IntPtr mask, int rectX, int rectY, int rectWidth, int rectHeight, IntPtr backgroundModel, IntPtr foregroundModel, int iterationCount, int mode);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_match_template")]
        internal static partial int ImgProcMatchTemplate(IntPtr image, IntPtr templ, IntPtr result, int method, IntPtr mask);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_find_contours_link_runs_count")]
        internal static partial int ImgProcFindContoursLinkRunsCount(IntPtr image, int includeHierarchy, out int contourCount, out int totalPointCount, out int hierarchyCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_find_contours_link_runs_fill")]
        internal static partial int ImgProcFindContoursLinkRunsFill(IntPtr image, int includeHierarchy, int[] contoursXy, int pointCapacity, int[] contourLengths, int contourCapacity, int[] hierarchyValues, int hierarchyCapacity, out int writtenContourCount, out int writtenPointCount, out int writtenHierarchyCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_draw_frame_axes")]
        internal static partial int ImgProcDrawFrameAxes(IntPtr image, IntPtr cameraMatrix, IntPtr distCoeffs, IntPtr rotationVector, IntPtr translationVector, float length, int thickness);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_font_face_create_default")]
        internal static partial int ImgProcFontFaceCreateDefault(out IntPtr fontFace);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_font_face_create")]
        internal static partial int ImgProcFontFaceCreate(byte[] fontPathOrNameUtf8, out IntPtr fontFace);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_font_face_release")]
        internal static partial void ImgProcFontFaceRelease(IntPtr fontFace);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_font_face_set")]
        internal static partial int ImgProcFontFaceSet(IntPtr fontFace, byte[] fontPathOrNameUtf8, out int result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_font_face_get_name_size")]
        internal static partial int ImgProcFontFaceGetNameSize(IntPtr fontFace, out int byteCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_font_face_get_name_fill")]
        internal static partial int ImgProcFontFaceGetNameFill(IntPtr fontFace, byte[] buffer, int bufferCapacity, out int bytesWritten);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_font_face_set_instance")]
        internal static partial int ImgProcFontFaceSetInstance(IntPtr fontFace, int[] parameters, int parameterCount, out int result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_font_face_get_instance_count")]
        internal static partial int ImgProcFontFaceGetInstanceCount(IntPtr fontFace, out int parameterCount, out int result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_font_face_get_instance_fill")]
        internal static partial int ImgProcFontFaceGetInstanceFill(IntPtr fontFace, int[] parameters, int parameterCapacity, out int parametersWritten, out int result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_put_text_font_face")]
        internal static partial int ImgProcPutTextFontFace(IntPtr image, byte[] textUtf8, int originX, int originY, double colorV0, double colorV1, double colorV2, double colorV3, IntPtr fontFace, int size, int weight, int flags, int hasWrap, int wrapStart, int wrapEnd, out int nextX, out int nextY);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_imgproc_get_text_size_font_face")]
        internal static partial int ImgProcGetTextSizeFontFace(int imageWidth, int imageHeight, byte[] textUtf8, int originX, int originY, IntPtr fontFace, int size, int weight, int flags, int hasWrap, int wrapStart, int wrapEnd, out int resultX, out int resultY, out int resultWidth, out int resultHeight);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_border_interpolate")]
        internal static partial int CoreBorderInterpolate(int p, int len, int borderType, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_copy_make_border")]
        internal static partial int CoreCopyMakeBorder(IntPtr src, IntPtr dst, int top, int bottom, int left, int right, int borderType, double v0, double v1, double v2, double v3);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_has_non_zero")]
        internal static partial int CoreHasNonZero(IntPtr src, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_find_non_zero")]
        internal static partial int CoreFindNonZero(IntPtr src, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_psnr")]
        internal static partial int CorePsnr(IntPtr src1, IntPtr src2, double maxValue, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_reduce_arg_min")]
        internal static partial int CoreReduceArgMin(IntPtr src, IntPtr dst, int axis, int lastIndex);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_reduce_arg_max")]
        internal static partial int CoreReduceArgMax(IntPtr src, IntPtr dst, int axis, int lastIndex);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_flip_nd")]
        internal static partial int CoreFlipNd(IntPtr src, IntPtr dst, int axis);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_broadcast")]
        internal static partial int CoreBroadcast(IntPtr src, IntPtr shape, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_copy_to_mask")]
        internal static partial int CoreCopyToMask(IntPtr src, IntPtr dst, IntPtr mask);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_check_range")]
        internal static partial int CoreCheckRange(IntPtr src, double minValue, double maxValue, out int valid, out int x, out int y);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_finite_mask")]
        internal static partial int CoreFiniteMask(IntPtr src, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_transpose_nd")]
        internal static partial int CoreTransposeNd(IntPtr src, int[] order, int orderCount, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_sort")]
        internal static partial int CoreSort(IntPtr src, IntPtr dst, int flags);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_sort_idx")]
        internal static partial int CoreSortIdx(IntPtr src, IntPtr dst, int flags);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_file_storage_create")]
        internal static partial int CoreFileStorageCreate(out IntPtr storage);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_file_storage_release_handle")]
        internal static partial void CoreFileStorageReleaseHandle(IntPtr storage);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_file_storage_open")]
        internal static partial int CoreFileStorageOpen(IntPtr storage, byte[] sourceUtf8, int sourceByteLength, int flags, byte[] encodingUtf8, int encodingByteLength, out int opened);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_file_storage_is_opened")]
        internal static partial int CoreFileStorageIsOpened(IntPtr storage, out int opened);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_file_storage_release")]
        internal static partial int CoreFileStorageRelease(IntPtr storage);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_file_storage_release_and_get_string")]
        internal static partial int CoreFileStorageReleaseAndGetString(IntPtr storage, out IntPtr result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_file_storage_get_first_top_level_node")]
        internal static partial int CoreFileStorageGetFirstTopLevelNode(IntPtr storage, out IntPtr node);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_file_storage_root")]
        internal static partial int CoreFileStorageRoot(IntPtr storage, int streamIndex, out IntPtr node);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_file_storage_get_node")]
        internal static partial int CoreFileStorageGetNode(IntPtr storage, byte[] nameUtf8, int nameByteLength, out IntPtr node);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_file_storage_write_int")]
        internal static partial int CoreFileStorageWriteInt(IntPtr storage, byte[] nameUtf8, int nameByteLength, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_file_storage_write_bool")]
        internal static partial int CoreFileStorageWriteBool(IntPtr storage, byte[] nameUtf8, int nameByteLength, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_file_storage_write_int64")]
        internal static partial int CoreFileStorageWriteInt64(IntPtr storage, byte[] nameUtf8, int nameByteLength, long value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_file_storage_write_double")]
        internal static partial int CoreFileStorageWriteDouble(IntPtr storage, byte[] nameUtf8, int nameByteLength, double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_file_storage_write_string")]
        internal static partial int CoreFileStorageWriteString(IntPtr storage, byte[] nameUtf8, int nameByteLength, byte[] valueUtf8, int valueByteLength);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_file_storage_write_mat")]
        internal static partial int CoreFileStorageWriteMat(IntPtr storage, byte[] nameUtf8, int nameByteLength, IntPtr value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_file_storage_write_string_vector")]
        internal static partial int CoreFileStorageWriteStringVector(IntPtr storage, byte[] nameUtf8, int nameByteLength, byte[] valuesUtf8, int valuesByteLength, int[] valueOffsets, int[] valueLengths, int valueCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_file_storage_write_comment")]
        internal static partial int CoreFileStorageWriteComment(IntPtr storage, byte[] commentUtf8, int commentByteLength, int append);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_file_storage_start_write_struct")]
        internal static partial int CoreFileStorageStartWriteStruct(IntPtr storage, byte[] nameUtf8, int nameByteLength, int flags, byte[] typeNameUtf8, int typeNameByteLength);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_file_storage_end_write_struct")]
        internal static partial int CoreFileStorageEndWriteStruct(IntPtr storage);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_file_storage_get_format")]
        internal static partial int CoreFileStorageGetFormat(IntPtr storage, out int format);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_file_node_create")]
        internal static partial int CoreFileNodeCreate(out IntPtr node);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_file_node_release")]
        internal static partial void CoreFileNodeRelease(IntPtr node);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_file_node_get_node")]
        internal static partial int CoreFileNodeGetNode(IntPtr node, byte[] nameUtf8, int nameByteLength, out IntPtr child);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_file_node_at")]
        internal static partial int CoreFileNodeAt(IntPtr node, int index, out IntPtr child);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_file_node_keys")]
        internal static partial int CoreFileNodeKeys(IntPtr node, out IntPtr keys);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_file_node_type")]
        internal static partial int CoreFileNodeType(IntPtr node, out int type);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_file_node_empty")]
        internal static partial int CoreFileNodeEmpty(IntPtr node, out int empty);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_file_node_name")]
        internal static partial int CoreFileNodeName(IntPtr node, out IntPtr result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_file_node_size")]
        internal static partial int CoreFileNodeSize(IntPtr node, out UIntPtr size);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_file_node_raw_size")]
        internal static partial int CoreFileNodeRawSize(IntPtr node, out UIntPtr size);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_file_node_real")]
        internal static partial int CoreFileNodeReal(IntPtr node, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_file_node_string")]
        internal static partial int CoreFileNodeString(IntPtr node, out IntPtr result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_file_node_mat")]
        internal static partial int CoreFileNodeMat(IntPtr node, IntPtr mat);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_string_list_count")]
        internal static partial int CoreStringListCount(IntPtr values, out UIntPtr count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_string_list_get")]
        internal static partial int CoreStringListGet(IntPtr values, UIntPtr index, out IntPtr result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_string_list_release")]
        internal static partial void CoreStringListRelease(IntPtr values);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_utf8_result_size")]
        internal static partial int CoreUtf8ResultSize(IntPtr result, out UIntPtr size);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_utf8_result_data")]
        internal static partial int CoreUtf8ResultData(IntPtr result, out IntPtr data);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_utf8_result_release")]
        internal static partial void CoreUtf8ResultRelease(IntPtr result);
    }
}
#endif

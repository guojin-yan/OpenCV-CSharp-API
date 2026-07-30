#if NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal static unsafe partial class NativeMethods
    {
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_create")]
        internal static partial int QRCodeDetectorCreate(out IntPtr detector);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_release_handle")]
        internal static partial void QRCodeDetectorReleaseHandle(IntPtr detector);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_set_eps_x")]
        internal static partial int QRCodeDetectorSetEpsX(IntPtr detector, double epsX);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_set_eps_y")]
        internal static partial int QRCodeDetectorSetEpsY(IntPtr detector, double epsY);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_set_use_alignment_markers")]
        internal static partial int QRCodeDetectorSetUseAlignmentMarkers(IntPtr detector, int useAlignmentMarkers);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_detect")]
        internal static partial int QRCodeDetectorDetect(IntPtr detector, IntPtr image, IntPtr points, out int detected);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_decode_length")]
        internal static partial int QRCodeDetectorDecodeLength(IntPtr detector, IntPtr image, IntPtr points, IntPtr straightQRCode, out int length);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_decode_fill")]
        internal static partial int QRCodeDetectorDecodeFill(IntPtr detector, IntPtr image, IntPtr points, IntPtr straightQRCode, byte* buffer, int bufferCapacity, out int written);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_detect_and_decode_length")]
        internal static partial int QRCodeDetectorDetectAndDecodeLength(IntPtr detector, IntPtr image, IntPtr points, IntPtr straightQRCode, out int length);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_detect_and_decode_fill")]
        internal static partial int QRCodeDetectorDetectAndDecodeFill(IntPtr detector, IntPtr image, IntPtr points, IntPtr straightQRCode, byte* buffer, int bufferCapacity, out int written);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_decode_curved_length")]
        internal static partial int QRCodeDetectorDecodeCurvedLength(IntPtr detector, IntPtr image, IntPtr points, IntPtr straightQRCode, out int length);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_decode_curved_fill")]
        internal static partial int QRCodeDetectorDecodeCurvedFill(IntPtr detector, IntPtr image, IntPtr points, IntPtr straightQRCode, byte* buffer, int bufferCapacity, out int written);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_detect_and_decode_curved_length")]
        internal static partial int QRCodeDetectorDetectAndDecodeCurvedLength(IntPtr detector, IntPtr image, IntPtr points, IntPtr straightQRCode, out int length);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_detect_and_decode_curved_fill")]
        internal static partial int QRCodeDetectorDetectAndDecodeCurvedFill(IntPtr detector, IntPtr image, IntPtr points, IntPtr straightQRCode, byte* buffer, int bufferCapacity, out int written);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_detect_multi")]
        internal static partial int QRCodeDetectorDetectMulti(IntPtr detector, IntPtr image, IntPtr points, out int detected);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_decode_multi_count")]
        internal static partial int QRCodeDetectorDecodeMultiCount(IntPtr detector, IntPtr image, IntPtr points, out int decoded, out int stringCount, out int byteCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_decode_multi_fill")]
        internal static partial int QRCodeDetectorDecodeMultiFill(IntPtr detector, IntPtr image, IntPtr points, int* offsets, int offsetCapacity, byte* buffer, int bufferCapacity, out int decoded, out int stringCount, out int byteCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_detect_and_decode_multi_count")]
        internal static partial int QRCodeDetectorDetectAndDecodeMultiCount(IntPtr detector, IntPtr image, IntPtr points, out int decoded, out int stringCount, out int byteCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_detect_and_decode_multi_fill")]
        internal static partial int QRCodeDetectorDetectAndDecodeMultiFill(IntPtr detector, IntPtr image, IntPtr points, int* offsets, int offsetCapacity, byte* buffer, int bufferCapacity, out int decoded, out int stringCount, out int byteCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_get_encoding")]
        internal static partial int QRCodeDetectorGetEncoding(IntPtr detector, int codeIndex, out int encoding);

        [StructLayout(LayoutKind.Sequential)]
        internal struct QRCodeDetectorArucoParamsNative
        {
            internal float MinModuleSizeInPyramid;
            internal float MaxRotation;
            internal float MaxModuleSizeMismatch;
            internal float MaxTimingPatternMismatch;
            internal float MaxPenalties;
            internal float MaxColorsMismatch;
            internal float ScaleTimingPatternScore;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct QRCodeEncoderParamsNative
        {
            internal int Version;
            internal int CorrectionLevel;
            internal int Mode;
            internal int StructureNumber;
        }

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_barcode_detector_create")]
        internal static partial int BarcodeDetectorCreate(out IntPtr detector);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_barcode_detector_create_with_super_resolution")]
        internal static partial int BarcodeDetectorCreateWithSuperResolution(byte[] superResolutionModelPath, out IntPtr detector);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_barcode_detector_release_handle")]
        internal static partial void BarcodeDetectorReleaseHandle(IntPtr detector);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_barcode_detector_detect")]
        internal static partial int BarcodeDetectorDetect(IntPtr detector, IntPtr image, IntPtr points, out int detected);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_barcode_detector_decode_count")]
        internal static partial int BarcodeDetectorDecodeCount(IntPtr detector, IntPtr image, IntPtr points, out int decoded, out int stringCount, out int byteCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_barcode_detector_decode_fill")]
        internal static unsafe partial int BarcodeDetectorDecodeFill(IntPtr detector, IntPtr image, IntPtr points, int* offsets, int offsetCapacity, byte* buffer, int bufferCapacity, out int decoded, out int stringCount, out int byteCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_barcode_detector_decode_with_type_count")]
        internal static partial int BarcodeDetectorDecodeWithTypeCount(IntPtr detector, IntPtr image, IntPtr points, out int decoded, out int infoCount, out int infoByteCount, out int typeCount, out int typeByteCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_barcode_detector_decode_with_type_fill")]
        internal static unsafe partial int BarcodeDetectorDecodeWithTypeFill(IntPtr detector, IntPtr image, IntPtr points, int* infoOffsets, int infoOffsetCapacity, byte* infoBuffer, int infoBufferCapacity, int* typeOffsets, int typeOffsetCapacity, byte* typeBuffer, int typeBufferCapacity, out int decoded, out int infoCount, out int infoByteCount, out int typeCount, out int typeByteCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_barcode_detector_detect_and_decode_count")]
        internal static partial int BarcodeDetectorDetectAndDecodeCount(IntPtr detector, IntPtr image, IntPtr points, out int decoded, out int stringCount, out int byteCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_barcode_detector_detect_and_decode_fill")]
        internal static unsafe partial int BarcodeDetectorDetectAndDecodeFill(IntPtr detector, IntPtr image, IntPtr points, int* offsets, int offsetCapacity, byte* buffer, int bufferCapacity, out int decoded, out int stringCount, out int byteCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_barcode_detector_detect_and_decode_with_type_count")]
        internal static partial int BarcodeDetectorDetectAndDecodeWithTypeCount(IntPtr detector, IntPtr image, IntPtr points, out int decoded, out int infoCount, out int infoByteCount, out int typeCount, out int typeByteCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_barcode_detector_detect_and_decode_with_type_fill")]
        internal static unsafe partial int BarcodeDetectorDetectAndDecodeWithTypeFill(IntPtr detector, IntPtr image, IntPtr points, int* infoOffsets, int infoOffsetCapacity, byte* infoBuffer, int infoBufferCapacity, int* typeOffsets, int typeOffsetCapacity, byte* typeBuffer, int typeBufferCapacity, out int decoded, out int infoCount, out int infoByteCount, out int typeCount, out int typeByteCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_barcode_detector_get_downsampling_threshold")]
        internal static partial int BarcodeDetectorGetDownsamplingThreshold(IntPtr detector, out double threshold);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_barcode_detector_set_downsampling_threshold")]
        internal static partial int BarcodeDetectorSetDownsamplingThreshold(IntPtr detector, double threshold);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_barcode_detector_get_gradient_threshold")]
        internal static partial int BarcodeDetectorGetGradientThreshold(IntPtr detector, out double threshold);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_barcode_detector_set_gradient_threshold")]
        internal static partial int BarcodeDetectorSetGradientThreshold(IntPtr detector, double threshold);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_barcode_detector_get_detector_scales_count")]
        internal static partial int BarcodeDetectorGetDetectorScalesCount(IntPtr detector, out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_barcode_detector_get_detector_scales_fill")]
        internal static partial int BarcodeDetectorGetDetectorScalesFill(IntPtr detector, float[] scales, int scaleCapacity, out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_barcode_detector_set_detector_scales")]
        internal static partial int BarcodeDetectorSetDetectorScales(IntPtr detector, float[] scales, int scaleCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_barcode_detector_set_detector_scales")]
        internal static unsafe partial int BarcodeDetectorSetDetectorScales(IntPtr detector, float* scales, int scaleCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_aruco_default_params")]
        internal static partial int QRCodeDetectorArucoDefaultParams(out QRCodeDetectorArucoParamsNative parameters);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_aruco_create")]
        internal static partial int QRCodeDetectorArucoCreate(out IntPtr detector);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_aruco_create_with_params")]
        internal static partial int QRCodeDetectorArucoCreateWithParams(ref QRCodeDetectorArucoParamsNative parameters, out IntPtr detector);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_aruco_release_handle")]
        internal static partial void QRCodeDetectorArucoReleaseHandle(IntPtr detector);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_aruco_get_detector_parameters")]
        internal static partial int QRCodeDetectorArucoGetDetectorParameters(IntPtr detector, out QRCodeDetectorArucoParamsNative parameters);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_aruco_set_detector_parameters")]
        internal static partial int QRCodeDetectorArucoSetDetectorParameters(IntPtr detector, ref QRCodeDetectorArucoParamsNative parameters);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_aruco_get_aruco_parameters")]
        internal static partial int QRCodeDetectorArucoGetArucoParameters(IntPtr detector, out ArucoDetectorParamsNative parameters);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_aruco_set_aruco_parameters")]
        internal static partial int QRCodeDetectorArucoSetArucoParameters(IntPtr detector, ref ArucoDetectorParamsNative parameters);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_aruco_detect")]
        internal static partial int QRCodeDetectorArucoDetect(IntPtr detector, IntPtr image, IntPtr points, out int detected);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_aruco_decode_length")]
        internal static partial int QRCodeDetectorArucoDecodeLength(IntPtr detector, IntPtr image, IntPtr points, IntPtr straightQRCode, out int length);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_aruco_decode_fill")]
        internal static unsafe partial int QRCodeDetectorArucoDecodeFill(IntPtr detector, IntPtr image, IntPtr points, IntPtr straightQRCode, byte* buffer, int bufferCapacity, out int written);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_aruco_detect_and_decode_length")]
        internal static partial int QRCodeDetectorArucoDetectAndDecodeLength(IntPtr detector, IntPtr image, IntPtr points, IntPtr straightQRCode, out int length);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_aruco_detect_and_decode_fill")]
        internal static unsafe partial int QRCodeDetectorArucoDetectAndDecodeFill(IntPtr detector, IntPtr image, IntPtr points, IntPtr straightQRCode, byte* buffer, int bufferCapacity, out int written);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_aruco_detect_multi")]
        internal static partial int QRCodeDetectorArucoDetectMulti(IntPtr detector, IntPtr image, IntPtr points, out int detected);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_aruco_decode_multi_count")]
        internal static partial int QRCodeDetectorArucoDecodeMultiCount(IntPtr detector, IntPtr image, IntPtr points, out int decoded, out int stringCount, out int byteCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_aruco_decode_multi_fill")]
        internal static unsafe partial int QRCodeDetectorArucoDecodeMultiFill(IntPtr detector, IntPtr image, IntPtr points, int* offsets, int offsetCapacity, byte* buffer, int bufferCapacity, out int decoded, out int stringCount, out int byteCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_aruco_detect_and_decode_multi_count")]
        internal static partial int QRCodeDetectorArucoDetectAndDecodeMultiCount(IntPtr detector, IntPtr image, IntPtr points, out int decoded, out int stringCount, out int byteCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_aruco_detect_and_decode_multi_fill")]
        internal static unsafe partial int QRCodeDetectorArucoDetectAndDecodeMultiFill(IntPtr detector, IntPtr image, IntPtr points, int* offsets, int offsetCapacity, byte* buffer, int bufferCapacity, out int decoded, out int stringCount, out int byteCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_encoder_default_params")]
        internal static partial int QRCodeEncoderDefaultParams(out QRCodeEncoderParamsNative parameters);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_encoder_create")]
        internal static partial int QRCodeEncoderCreate(ref QRCodeEncoderParamsNative parameters, out IntPtr encoder);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_encoder_release_handle")]
        internal static partial void QRCodeEncoderReleaseHandle(IntPtr encoder);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_encoder_encode")]
        internal static partial int QRCodeEncoderEncode(IntPtr encoder, byte[] encodedInfo, IntPtr qrcode);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_encoder_encode_structured_append_count")]
        internal static partial int QRCodeEncoderEncodeStructuredAppendCount(IntPtr encoder, byte[] encodedInfo, out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_encoder_encode_structured_append_fill")]
        internal static partial int QRCodeEncoderEncodeStructuredAppendFill(IntPtr encoder, byte[] encodedInfo, IntPtr[] qrcodes, int qrcodeCapacity, out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_detector_yn_create")]
        internal static partial int FaceDetectorYNCreate(byte[] model, byte[] config, int inputWidth, int inputHeight, float scoreThreshold, float nmsThreshold, int topK, int backendId, int targetId, out IntPtr detector);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_detector_yn_create_from_buffer")]
        internal static partial int FaceDetectorYNCreateFromBuffer(byte[] framework, byte* modelBuffer, int modelBufferLength, byte* configBuffer, int configBufferLength, int inputWidth, int inputHeight, float scoreThreshold, float nmsThreshold, int topK, int backendId, int targetId, out IntPtr detector);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_detector_yn_release_handle")]
        internal static partial void FaceDetectorYNReleaseHandle(IntPtr detector);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_detector_yn_set_input_size")]
        internal static partial int FaceDetectorYNSetInputSize(IntPtr detector, int width, int height);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_detector_yn_get_input_size")]
        internal static partial int FaceDetectorYNGetInputSize(IntPtr detector, out int width, out int height);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_detector_yn_set_score_threshold")]
        internal static partial int FaceDetectorYNSetScoreThreshold(IntPtr detector, float scoreThreshold);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_detector_yn_get_score_threshold")]
        internal static partial int FaceDetectorYNGetScoreThreshold(IntPtr detector, out float scoreThreshold);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_detector_yn_set_nms_threshold")]
        internal static partial int FaceDetectorYNSetNMSThreshold(IntPtr detector, float nmsThreshold);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_detector_yn_get_nms_threshold")]
        internal static partial int FaceDetectorYNGetNMSThreshold(IntPtr detector, out float nmsThreshold);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_detector_yn_set_top_k")]
        internal static partial int FaceDetectorYNSetTopK(IntPtr detector, int topK);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_detector_yn_get_top_k")]
        internal static partial int FaceDetectorYNGetTopK(IntPtr detector, out int topK);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_detector_yn_detect")]
        internal static partial int FaceDetectorYNDetect(IntPtr detector, IntPtr image, IntPtr faces, out int result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_recognizer_sf_create")]
        internal static partial int FaceRecognizerSFCreate(byte[] model, byte[] config, int backendId, int targetId, out IntPtr recognizer);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_recognizer_sf_create_from_buffer")]
        internal static partial int FaceRecognizerSFCreateFromBuffer(byte[] framework, byte* modelBuffer, int modelBufferLength, byte* configBuffer, int configBufferLength, int backendId, int targetId, out IntPtr recognizer);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_recognizer_sf_release_handle")]
        internal static partial void FaceRecognizerSFReleaseHandle(IntPtr recognizer);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_recognizer_sf_align_crop")]
        internal static partial int FaceRecognizerSFAlignCrop(IntPtr recognizer, IntPtr sourceImage, IntPtr faceBox, IntPtr alignedImage);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_recognizer_sf_feature")]
        internal static partial int FaceRecognizerSFFeature(IntPtr recognizer, IntPtr alignedImage, IntPtr faceFeature);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_recognizer_sf_match")]
        internal static partial int FaceRecognizerSFMatch(IntPtr recognizer, IntPtr faceFeature1, IntPtr faceFeature2, int distanceType, out double result);
    }
}
#endif

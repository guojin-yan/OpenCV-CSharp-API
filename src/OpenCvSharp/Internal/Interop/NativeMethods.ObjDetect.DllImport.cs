#if !NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal static unsafe partial class NativeMethods
    {
        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_create")]
        internal static extern int QRCodeDetectorCreate(out IntPtr detector);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_release_handle")]
        internal static extern void QRCodeDetectorReleaseHandle(IntPtr detector);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_set_eps_x")]
        internal static extern int QRCodeDetectorSetEpsX(IntPtr detector, double epsX);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_set_eps_y")]
        internal static extern int QRCodeDetectorSetEpsY(IntPtr detector, double epsY);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_set_use_alignment_markers")]
        internal static extern int QRCodeDetectorSetUseAlignmentMarkers(IntPtr detector, int useAlignmentMarkers);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_detect")]
        internal static extern int QRCodeDetectorDetect(IntPtr detector, IntPtr image, IntPtr points, out int detected);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_decode_length")]
        internal static extern int QRCodeDetectorDecodeLength(IntPtr detector, IntPtr image, IntPtr points, IntPtr straightQRCode, out int length);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_decode_fill")]
        internal static extern int QRCodeDetectorDecodeFill(IntPtr detector, IntPtr image, IntPtr points, IntPtr straightQRCode, byte* buffer, int bufferCapacity, out int written);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_detect_and_decode_length")]
        internal static extern int QRCodeDetectorDetectAndDecodeLength(IntPtr detector, IntPtr image, IntPtr points, IntPtr straightQRCode, out int length);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_detect_and_decode_fill")]
        internal static extern int QRCodeDetectorDetectAndDecodeFill(IntPtr detector, IntPtr image, IntPtr points, IntPtr straightQRCode, byte* buffer, int bufferCapacity, out int written);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_decode_curved_length")]
        internal static extern int QRCodeDetectorDecodeCurvedLength(IntPtr detector, IntPtr image, IntPtr points, IntPtr straightQRCode, out int length);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_decode_curved_fill")]
        internal static extern int QRCodeDetectorDecodeCurvedFill(IntPtr detector, IntPtr image, IntPtr points, IntPtr straightQRCode, byte* buffer, int bufferCapacity, out int written);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_detect_and_decode_curved_length")]
        internal static extern int QRCodeDetectorDetectAndDecodeCurvedLength(IntPtr detector, IntPtr image, IntPtr points, IntPtr straightQRCode, out int length);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_detect_and_decode_curved_fill")]
        internal static extern int QRCodeDetectorDetectAndDecodeCurvedFill(IntPtr detector, IntPtr image, IntPtr points, IntPtr straightQRCode, byte* buffer, int bufferCapacity, out int written);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_detect_multi")]
        internal static extern int QRCodeDetectorDetectMulti(IntPtr detector, IntPtr image, IntPtr points, out int detected);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_decode_multi_count")]
        internal static extern int QRCodeDetectorDecodeMultiCount(IntPtr detector, IntPtr image, IntPtr points, out int decoded, out int stringCount, out int byteCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_decode_multi_fill")]
        internal static extern int QRCodeDetectorDecodeMultiFill(IntPtr detector, IntPtr image, IntPtr points, int* offsets, int offsetCapacity, byte* buffer, int bufferCapacity, out int decoded, out int stringCount, out int byteCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_detect_and_decode_multi_count")]
        internal static extern int QRCodeDetectorDetectAndDecodeMultiCount(IntPtr detector, IntPtr image, IntPtr points, out int decoded, out int stringCount, out int byteCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_detect_and_decode_multi_fill")]
        internal static extern int QRCodeDetectorDetectAndDecodeMultiFill(IntPtr detector, IntPtr image, IntPtr points, int* offsets, int offsetCapacity, byte* buffer, int bufferCapacity, out int decoded, out int stringCount, out int byteCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_get_encoding")]
        internal static extern int QRCodeDetectorGetEncoding(IntPtr detector, int codeIndex, out int encoding);

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

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_barcode_detector_create")]
        internal static extern int BarcodeDetectorCreate(out IntPtr detector);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_barcode_detector_create_with_super_resolution")]
        internal static extern int BarcodeDetectorCreateWithSuperResolution(byte[] superResolutionModelPath, out IntPtr detector);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_barcode_detector_release_handle")]
        internal static extern void BarcodeDetectorReleaseHandle(IntPtr detector);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_barcode_detector_detect")]
        internal static extern int BarcodeDetectorDetect(IntPtr detector, IntPtr image, IntPtr points, out int detected);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_barcode_detector_decode_count")]
        internal static extern int BarcodeDetectorDecodeCount(IntPtr detector, IntPtr image, IntPtr points, out int decoded, out int stringCount, out int byteCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_barcode_detector_decode_fill")]
        internal static extern int BarcodeDetectorDecodeFill(IntPtr detector, IntPtr image, IntPtr points, int* offsets, int offsetCapacity, byte* buffer, int bufferCapacity, out int decoded, out int stringCount, out int byteCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_barcode_detector_decode_with_type_count")]
        internal static extern int BarcodeDetectorDecodeWithTypeCount(IntPtr detector, IntPtr image, IntPtr points, out int decoded, out int infoCount, out int infoByteCount, out int typeCount, out int typeByteCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_barcode_detector_decode_with_type_fill")]
        internal static extern int BarcodeDetectorDecodeWithTypeFill(IntPtr detector, IntPtr image, IntPtr points, int* infoOffsets, int infoOffsetCapacity, byte* infoBuffer, int infoBufferCapacity, int* typeOffsets, int typeOffsetCapacity, byte* typeBuffer, int typeBufferCapacity, out int decoded, out int infoCount, out int infoByteCount, out int typeCount, out int typeByteCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_barcode_detector_detect_and_decode_count")]
        internal static extern int BarcodeDetectorDetectAndDecodeCount(IntPtr detector, IntPtr image, IntPtr points, out int decoded, out int stringCount, out int byteCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_barcode_detector_detect_and_decode_fill")]
        internal static extern int BarcodeDetectorDetectAndDecodeFill(IntPtr detector, IntPtr image, IntPtr points, int* offsets, int offsetCapacity, byte* buffer, int bufferCapacity, out int decoded, out int stringCount, out int byteCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_barcode_detector_detect_and_decode_with_type_count")]
        internal static extern int BarcodeDetectorDetectAndDecodeWithTypeCount(IntPtr detector, IntPtr image, IntPtr points, out int decoded, out int infoCount, out int infoByteCount, out int typeCount, out int typeByteCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_barcode_detector_detect_and_decode_with_type_fill")]
        internal static extern int BarcodeDetectorDetectAndDecodeWithTypeFill(IntPtr detector, IntPtr image, IntPtr points, int* infoOffsets, int infoOffsetCapacity, byte* infoBuffer, int infoBufferCapacity, int* typeOffsets, int typeOffsetCapacity, byte* typeBuffer, int typeBufferCapacity, out int decoded, out int infoCount, out int infoByteCount, out int typeCount, out int typeByteCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_barcode_detector_get_downsampling_threshold")]
        internal static extern int BarcodeDetectorGetDownsamplingThreshold(IntPtr detector, out double threshold);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_barcode_detector_set_downsampling_threshold")]
        internal static extern int BarcodeDetectorSetDownsamplingThreshold(IntPtr detector, double threshold);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_barcode_detector_get_gradient_threshold")]
        internal static extern int BarcodeDetectorGetGradientThreshold(IntPtr detector, out double threshold);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_barcode_detector_set_gradient_threshold")]
        internal static extern int BarcodeDetectorSetGradientThreshold(IntPtr detector, double threshold);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_barcode_detector_get_detector_scales_count")]
        internal static extern int BarcodeDetectorGetDetectorScalesCount(IntPtr detector, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_barcode_detector_get_detector_scales_fill")]
        internal static extern int BarcodeDetectorGetDetectorScalesFill(IntPtr detector, float[] scales, int scaleCapacity, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_barcode_detector_set_detector_scales")]
        internal static extern int BarcodeDetectorSetDetectorScales(IntPtr detector, float[] scales, int scaleCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_barcode_detector_set_detector_scales")]
        internal static extern int BarcodeDetectorSetDetectorScales(IntPtr detector, float* scales, int scaleCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_aruco_default_params")]
        internal static extern int QRCodeDetectorArucoDefaultParams(out QRCodeDetectorArucoParamsNative parameters);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_aruco_create")]
        internal static extern int QRCodeDetectorArucoCreate(out IntPtr detector);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_aruco_create_with_params")]
        internal static extern int QRCodeDetectorArucoCreateWithParams(ref QRCodeDetectorArucoParamsNative parameters, out IntPtr detector);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_aruco_release_handle")]
        internal static extern void QRCodeDetectorArucoReleaseHandle(IntPtr detector);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_aruco_get_detector_parameters")]
        internal static extern int QRCodeDetectorArucoGetDetectorParameters(IntPtr detector, out QRCodeDetectorArucoParamsNative parameters);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_aruco_set_detector_parameters")]
        internal static extern int QRCodeDetectorArucoSetDetectorParameters(IntPtr detector, ref QRCodeDetectorArucoParamsNative parameters);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_aruco_get_aruco_parameters")]
        internal static extern int QRCodeDetectorArucoGetArucoParameters(IntPtr detector, out ArucoDetectorParamsNative parameters);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_aruco_set_aruco_parameters")]
        internal static extern int QRCodeDetectorArucoSetArucoParameters(IntPtr detector, ref ArucoDetectorParamsNative parameters);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_aruco_detect")]
        internal static extern int QRCodeDetectorArucoDetect(IntPtr detector, IntPtr image, IntPtr points, out int detected);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_aruco_decode_length")]
        internal static extern int QRCodeDetectorArucoDecodeLength(IntPtr detector, IntPtr image, IntPtr points, IntPtr straightQRCode, out int length);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_aruco_decode_fill")]
        internal static extern int QRCodeDetectorArucoDecodeFill(IntPtr detector, IntPtr image, IntPtr points, IntPtr straightQRCode, byte* buffer, int bufferCapacity, out int written);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_aruco_detect_and_decode_length")]
        internal static extern int QRCodeDetectorArucoDetectAndDecodeLength(IntPtr detector, IntPtr image, IntPtr points, IntPtr straightQRCode, out int length);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_aruco_detect_and_decode_fill")]
        internal static extern int QRCodeDetectorArucoDetectAndDecodeFill(IntPtr detector, IntPtr image, IntPtr points, IntPtr straightQRCode, byte* buffer, int bufferCapacity, out int written);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_aruco_detect_multi")]
        internal static extern int QRCodeDetectorArucoDetectMulti(IntPtr detector, IntPtr image, IntPtr points, out int detected);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_aruco_decode_multi_count")]
        internal static extern int QRCodeDetectorArucoDecodeMultiCount(IntPtr detector, IntPtr image, IntPtr points, out int decoded, out int stringCount, out int byteCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_aruco_decode_multi_fill")]
        internal static extern int QRCodeDetectorArucoDecodeMultiFill(IntPtr detector, IntPtr image, IntPtr points, int* offsets, int offsetCapacity, byte* buffer, int bufferCapacity, out int decoded, out int stringCount, out int byteCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_aruco_detect_and_decode_multi_count")]
        internal static extern int QRCodeDetectorArucoDetectAndDecodeMultiCount(IntPtr detector, IntPtr image, IntPtr points, out int decoded, out int stringCount, out int byteCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_detector_aruco_detect_and_decode_multi_fill")]
        internal static extern int QRCodeDetectorArucoDetectAndDecodeMultiFill(IntPtr detector, IntPtr image, IntPtr points, int* offsets, int offsetCapacity, byte* buffer, int bufferCapacity, out int decoded, out int stringCount, out int byteCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_encoder_default_params")]
        internal static extern int QRCodeEncoderDefaultParams(out QRCodeEncoderParamsNative parameters);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_encoder_create")]
        internal static extern int QRCodeEncoderCreate(ref QRCodeEncoderParamsNative parameters, out IntPtr encoder);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_encoder_release_handle")]
        internal static extern void QRCodeEncoderReleaseHandle(IntPtr encoder);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_encoder_encode")]
        internal static extern int QRCodeEncoderEncode(IntPtr encoder, byte[] encodedInfo, IntPtr qrcode);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_encoder_encode_structured_append_count")]
        internal static extern int QRCodeEncoderEncodeStructuredAppendCount(IntPtr encoder, byte[] encodedInfo, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_qrcode_encoder_encode_structured_append_fill")]
        internal static extern int QRCodeEncoderEncodeStructuredAppendFill(IntPtr encoder, byte[] encodedInfo, IntPtr[] qrcodes, int qrcodeCapacity, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_detector_yn_create")]
        internal static extern int FaceDetectorYNCreate(byte[] model, byte[] config, int inputWidth, int inputHeight, float scoreThreshold, float nmsThreshold, int topK, int backendId, int targetId, out IntPtr detector);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_detector_yn_create_from_buffer")]
        internal static extern int FaceDetectorYNCreateFromBuffer(byte[] framework, byte* modelBuffer, int modelBufferLength, byte* configBuffer, int configBufferLength, int inputWidth, int inputHeight, float scoreThreshold, float nmsThreshold, int topK, int backendId, int targetId, out IntPtr detector);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_detector_yn_release_handle")]
        internal static extern void FaceDetectorYNReleaseHandle(IntPtr detector);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_detector_yn_set_input_size")]
        internal static extern int FaceDetectorYNSetInputSize(IntPtr detector, int width, int height);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_detector_yn_get_input_size")]
        internal static extern int FaceDetectorYNGetInputSize(IntPtr detector, out int width, out int height);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_detector_yn_set_score_threshold")]
        internal static extern int FaceDetectorYNSetScoreThreshold(IntPtr detector, float scoreThreshold);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_detector_yn_get_score_threshold")]
        internal static extern int FaceDetectorYNGetScoreThreshold(IntPtr detector, out float scoreThreshold);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_detector_yn_set_nms_threshold")]
        internal static extern int FaceDetectorYNSetNMSThreshold(IntPtr detector, float nmsThreshold);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_detector_yn_get_nms_threshold")]
        internal static extern int FaceDetectorYNGetNMSThreshold(IntPtr detector, out float nmsThreshold);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_detector_yn_set_top_k")]
        internal static extern int FaceDetectorYNSetTopK(IntPtr detector, int topK);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_detector_yn_get_top_k")]
        internal static extern int FaceDetectorYNGetTopK(IntPtr detector, out int topK);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_detector_yn_detect")]
        internal static extern int FaceDetectorYNDetect(IntPtr detector, IntPtr image, IntPtr faces, out int result);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_recognizer_sf_create")]
        internal static extern int FaceRecognizerSFCreate(byte[] model, byte[] config, int backendId, int targetId, out IntPtr recognizer);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_recognizer_sf_create_from_buffer")]
        internal static extern int FaceRecognizerSFCreateFromBuffer(byte[] framework, byte* modelBuffer, int modelBufferLength, byte* configBuffer, int configBufferLength, int backendId, int targetId, out IntPtr recognizer);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_recognizer_sf_release_handle")]
        internal static extern void FaceRecognizerSFReleaseHandle(IntPtr recognizer);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_recognizer_sf_align_crop")]
        internal static extern int FaceRecognizerSFAlignCrop(IntPtr recognizer, IntPtr sourceImage, IntPtr faceBox, IntPtr alignedImage);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_recognizer_sf_feature")]
        internal static extern int FaceRecognizerSFFeature(IntPtr recognizer, IntPtr alignedImage, IntPtr faceFeature);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_recognizer_sf_match")]
        internal static extern int FaceRecognizerSFMatch(IntPtr recognizer, IntPtr faceFeature1, IntPtr faceFeature2, int distanceType, out double result);
    }
}
#endif

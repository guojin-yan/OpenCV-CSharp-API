#if NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct FacePredictionResultNative
        {
            internal int Label;
            internal double Distance;
        }

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_recognizer_release_handle")]
        internal static partial void FaceRecognizerReleaseHandle(IntPtr recognizer);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_standard_collector_release_handle")]
        internal static partial void FaceStandardCollectorReleaseHandle(IntPtr collector);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_bif_release_handle")]
        internal static partial void FaceBIFReleaseHandle(IntPtr bif);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_facemark_release_handle")]
        internal static partial void FaceFacemarkReleaseHandle(IntPtr facemark);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_mace_release_handle")]
        internal static partial void FaceMaceReleaseHandle(IntPtr mace);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_eigen_create")]
        internal static partial int FaceEigenCreate(int numComponents, double threshold, out IntPtr recognizer);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_fisher_create")]
        internal static partial int FaceFisherCreate(int numComponents, double threshold, out IntPtr recognizer);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_lbph_create")]
        internal static partial int FaceLBPHCreate(int radius, int neighbors, int gridX, int gridY, double threshold, out IntPtr recognizer);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_recognizer_train")]
        internal static partial int FaceRecognizerTrain(IntPtr recognizer, IntPtr[] images, int imageCount, int[] labels, int labelCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_recognizer_update")]
        internal static partial int FaceRecognizerUpdate(IntPtr recognizer, IntPtr[] images, int imageCount, int[] labels, int labelCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_recognizer_predict_label")]
        internal static partial int FaceRecognizerPredictLabel(IntPtr recognizer, IntPtr image, out int label);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_recognizer_predict")]
        internal static partial int FaceRecognizerPredict(IntPtr recognizer, IntPtr image, out int label, out double confidence);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_recognizer_predict_collect")]
        internal static partial int FaceRecognizerPredictCollect(IntPtr recognizer, IntPtr image, IntPtr collector);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_recognizer_read")]
        internal static partial int FaceRecognizerRead(IntPtr recognizer, byte[] path);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_recognizer_write")]
        internal static partial int FaceRecognizerWrite(IntPtr recognizer, byte[] path);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_recognizer_empty")]
        internal static partial int FaceRecognizerEmpty(IntPtr recognizer, out int empty);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_recognizer_set_label_info")]
        internal static partial int FaceRecognizerSetLabelInfo(IntPtr recognizer, int label, byte[] info);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_recognizer_get_label_info_length")]
        internal static partial int FaceRecognizerGetLabelInfoLength(IntPtr recognizer, int label, out int length);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_recognizer_get_label_info_fill")]
        internal static unsafe partial int FaceRecognizerGetLabelInfoFill(IntPtr recognizer, int label, byte* buffer, int bufferCapacity, out int written);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_recognizer_get_labels_by_string_count")]
        internal static partial int FaceRecognizerGetLabelsByStringCount(IntPtr recognizer, byte[] substring, out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_recognizer_get_labels_by_string_fill")]
        internal static partial int FaceRecognizerGetLabelsByStringFill(IntPtr recognizer, byte[] substring, int[] labels, int labelCapacity, out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_recognizer_get_threshold")]
        internal static partial int FaceRecognizerGetThreshold(IntPtr recognizer, out double threshold);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_recognizer_set_threshold")]
        internal static partial int FaceRecognizerSetThreshold(IntPtr recognizer, double threshold);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_basic_get_num_components")]
        internal static partial int FaceBasicGetNumComponents(IntPtr recognizer, out int numComponents);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_basic_set_num_components")]
        internal static partial int FaceBasicSetNumComponents(IntPtr recognizer, int numComponents);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_basic_get_labels")]
        internal static partial int FaceBasicGetLabels(IntPtr recognizer, out IntPtr labels);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_basic_get_eigen_values")]
        internal static partial int FaceBasicGetEigenValues(IntPtr recognizer, out IntPtr eigenValues);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_basic_get_eigen_vectors")]
        internal static partial int FaceBasicGetEigenVectors(IntPtr recognizer, out IntPtr eigenVectors);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_basic_get_mean")]
        internal static partial int FaceBasicGetMean(IntPtr recognizer, out IntPtr mean);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_basic_get_projections_count")]
        internal static partial int FaceBasicGetProjectionsCount(IntPtr recognizer, out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_basic_get_projections_fill")]
        internal static partial int FaceBasicGetProjectionsFill(IntPtr recognizer, IntPtr[] projections, int projectionCapacity, out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_lbph_get_radius")]
        internal static partial int FaceLBPHGetRadius(IntPtr recognizer, out int radius);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_lbph_set_radius")]
        internal static partial int FaceLBPHSetRadius(IntPtr recognizer, int radius);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_lbph_get_neighbors")]
        internal static partial int FaceLBPHGetNeighbors(IntPtr recognizer, out int neighbors);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_lbph_set_neighbors")]
        internal static partial int FaceLBPHSetNeighbors(IntPtr recognizer, int neighbors);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_lbph_get_grid_x")]
        internal static partial int FaceLBPHGetGridX(IntPtr recognizer, out int gridX);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_lbph_set_grid_x")]
        internal static partial int FaceLBPHSetGridX(IntPtr recognizer, int gridX);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_lbph_get_grid_y")]
        internal static partial int FaceLBPHGetGridY(IntPtr recognizer, out int gridY);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_lbph_set_grid_y")]
        internal static partial int FaceLBPHSetGridY(IntPtr recognizer, int gridY);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_lbph_get_labels")]
        internal static partial int FaceLBPHGetLabels(IntPtr recognizer, out IntPtr labels);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_lbph_get_histograms_count")]
        internal static partial int FaceLBPHGetHistogramsCount(IntPtr recognizer, out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_lbph_get_histograms_fill")]
        internal static partial int FaceLBPHGetHistogramsFill(IntPtr recognizer, IntPtr[] histograms, int histogramCapacity, out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_standard_collector_create")]
        internal static partial int FaceStandardCollectorCreate(double threshold, out IntPtr collector);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_standard_collector_get_min_label")]
        internal static partial int FaceStandardCollectorGetMinLabel(IntPtr collector, out int label);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_standard_collector_get_min_dist")]
        internal static partial int FaceStandardCollectorGetMinDist(IntPtr collector, out double distance);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_standard_collector_get_results_count")]
        internal static partial int FaceStandardCollectorGetResultsCount(IntPtr collector, int sorted, out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_standard_collector_get_results_fill")]
        internal static partial int FaceStandardCollectorGetResultsFill(IntPtr collector, int sorted, FacePredictionResultNative[] results, int resultCapacity, out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_bif_create")]
        internal static partial int FaceBIFCreate(int numBands, int numRotations, out IntPtr bif);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_bif_get_num_bands")]
        internal static partial int FaceBIFGetNumBands(IntPtr bif, out int numBands);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_bif_get_num_rotations")]
        internal static partial int FaceBIFGetNumRotations(IntPtr bif, out int numRotations);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_bif_compute")]
        internal static partial int FaceBIFCompute(IntPtr bif, IntPtr image, IntPtr features);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_facemark_load_model")]
        internal static partial int FaceFacemarkLoadModel(IntPtr facemark, byte[] modelPath);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_facemark_fit")]
        internal static partial int FaceFacemarkFit(IntPtr facemark, IntPtr image, int[] faces, int faceCount, out int result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_facemark_fit_landmarks_count")]
        internal static partial int FaceFacemarkFitLandmarksCount(IntPtr facemark, out int faceCount, out int pointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_facemark_fit_landmarks_fill")]
        internal static partial int FaceFacemarkFitLandmarksFill(IntPtr facemark, int[] landmarkOffsets, int landmarkOffsetCapacity, float[] landmarksBuffer, int landmarkPointCapacity, out int faceCount, out int pointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_facemark_train_add_sample")]
        internal static partial int FaceFacemarkTrainAddSample(IntPtr facemark, IntPtr image, float[] landmarks, int landmarkCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_facemark_train_training")]
        internal static partial int FaceFacemarkTrainTraining(IntPtr facemark);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_facemark_train_get_faces_count")]
        internal static partial int FaceFacemarkTrainGetFacesCount(IntPtr facemark, IntPtr image, out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_facemark_train_get_faces_fill")]
        internal static partial int FaceFacemarkTrainGetFacesFill(IntPtr facemark, IntPtr image, int[] faces, int faceCapacity, out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_facemark_save")]
        internal static partial int FaceFacemarkSave(IntPtr facemark, byte[] path);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_facemark_lbf_create")]
        internal static partial int FaceFacemarkLBFCreate(int nLandmarks, int initShapeN, int stagesN, int treeN, int treeDepth, double shapeOffset, double baggingOverlap, int verbose, out IntPtr facemark);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_facemark_lbf_create_ex")]
        internal static partial int FaceFacemarkLBFCreateEx(int nLandmarks, int initShapeN, int stagesN, int treeN, int treeDepth, double shapeOffset, double baggingOverlap, int verbose, int saveModel, uint seed, byte[] cascadeFace, byte[] modelFilename, int[] featsM, int featsCount, double[] radiusM, int radiusCount, int[] leftPupil, int leftPupilCount, int[] rightPupil, int rightPupilCount, int detectRoiX, int detectRoiY, int detectRoiWidth, int detectRoiHeight, out IntPtr facemark);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_mace_create")]
        internal static partial int FaceMaceCreate(int imgSize, out IntPtr mace);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_mace_load")]
        internal static partial int FaceMaceLoad(byte[] filename, byte[] objname, out IntPtr mace);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_mace_salt")]
        internal static partial int FaceMaceSalt(IntPtr mace, byte[] passphrase);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_mace_train")]
        internal static partial int FaceMaceTrain(IntPtr mace, IntPtr[] images, int imageCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_mace_same")]
        internal static partial int FaceMaceSame(IntPtr mace, IntPtr query, out int same);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_mace_save")]
        internal static partial int FaceMaceSave(IntPtr mace, byte[] path);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_mace_empty")]
        internal static partial int FaceMaceEmpty(IntPtr mace, out int empty);
    }
}
#endif

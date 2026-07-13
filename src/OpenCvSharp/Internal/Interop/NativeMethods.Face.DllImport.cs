#if !NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct FacePredictionResultNative
        {
            internal int Label;
            internal double Distance;
        }

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_recognizer_release_handle")]
        internal static extern void FaceRecognizerReleaseHandle(IntPtr recognizer);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_standard_collector_release_handle")]
        internal static extern void FaceStandardCollectorReleaseHandle(IntPtr collector);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_bif_release_handle")]
        internal static extern void FaceBIFReleaseHandle(IntPtr bif);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_facemark_release_handle")]
        internal static extern void FaceFacemarkReleaseHandle(IntPtr facemark);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_mace_release_handle")]
        internal static extern void FaceMaceReleaseHandle(IntPtr mace);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_eigen_create")]
        internal static extern int FaceEigenCreate(int numComponents, double threshold, out IntPtr recognizer);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_fisher_create")]
        internal static extern int FaceFisherCreate(int numComponents, double threshold, out IntPtr recognizer);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_lbph_create")]
        internal static extern int FaceLBPHCreate(int radius, int neighbors, int gridX, int gridY, double threshold, out IntPtr recognizer);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_recognizer_train")]
        internal static extern int FaceRecognizerTrain(IntPtr recognizer, IntPtr[] images, int imageCount, int[] labels, int labelCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_recognizer_update")]
        internal static extern int FaceRecognizerUpdate(IntPtr recognizer, IntPtr[] images, int imageCount, int[] labels, int labelCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_recognizer_predict_label")]
        internal static extern int FaceRecognizerPredictLabel(IntPtr recognizer, IntPtr image, out int label);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_recognizer_predict")]
        internal static extern int FaceRecognizerPredict(IntPtr recognizer, IntPtr image, out int label, out double confidence);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_recognizer_predict_collect")]
        internal static extern int FaceRecognizerPredictCollect(IntPtr recognizer, IntPtr image, IntPtr collector);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_recognizer_read")]
        internal static extern int FaceRecognizerRead(IntPtr recognizer, byte[] path);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_recognizer_write")]
        internal static extern int FaceRecognizerWrite(IntPtr recognizer, byte[] path);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_recognizer_empty")]
        internal static extern int FaceRecognizerEmpty(IntPtr recognizer, out int empty);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_recognizer_set_label_info")]
        internal static extern int FaceRecognizerSetLabelInfo(IntPtr recognizer, int label, byte[] info);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_recognizer_get_label_info_length")]
        internal static extern int FaceRecognizerGetLabelInfoLength(IntPtr recognizer, int label, out int length);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_recognizer_get_label_info_fill")]
        internal static extern unsafe int FaceRecognizerGetLabelInfoFill(IntPtr recognizer, int label, byte* buffer, int bufferCapacity, out int written);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_recognizer_get_labels_by_string_count")]
        internal static extern int FaceRecognizerGetLabelsByStringCount(IntPtr recognizer, byte[] substring, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_recognizer_get_labels_by_string_fill")]
        internal static extern int FaceRecognizerGetLabelsByStringFill(IntPtr recognizer, byte[] substring, int[] labels, int labelCapacity, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_recognizer_get_threshold")]
        internal static extern int FaceRecognizerGetThreshold(IntPtr recognizer, out double threshold);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_recognizer_set_threshold")]
        internal static extern int FaceRecognizerSetThreshold(IntPtr recognizer, double threshold);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_basic_get_num_components")]
        internal static extern int FaceBasicGetNumComponents(IntPtr recognizer, out int numComponents);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_basic_set_num_components")]
        internal static extern int FaceBasicSetNumComponents(IntPtr recognizer, int numComponents);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_basic_get_labels")]
        internal static extern int FaceBasicGetLabels(IntPtr recognizer, out IntPtr labels);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_basic_get_eigen_values")]
        internal static extern int FaceBasicGetEigenValues(IntPtr recognizer, out IntPtr eigenValues);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_basic_get_eigen_vectors")]
        internal static extern int FaceBasicGetEigenVectors(IntPtr recognizer, out IntPtr eigenVectors);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_basic_get_mean")]
        internal static extern int FaceBasicGetMean(IntPtr recognizer, out IntPtr mean);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_basic_get_projections_count")]
        internal static extern int FaceBasicGetProjectionsCount(IntPtr recognizer, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_basic_get_projections_fill")]
        internal static extern int FaceBasicGetProjectionsFill(IntPtr recognizer, IntPtr[] projections, int projectionCapacity, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_lbph_get_radius")]
        internal static extern int FaceLBPHGetRadius(IntPtr recognizer, out int radius);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_lbph_set_radius")]
        internal static extern int FaceLBPHSetRadius(IntPtr recognizer, int radius);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_lbph_get_neighbors")]
        internal static extern int FaceLBPHGetNeighbors(IntPtr recognizer, out int neighbors);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_lbph_set_neighbors")]
        internal static extern int FaceLBPHSetNeighbors(IntPtr recognizer, int neighbors);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_lbph_get_grid_x")]
        internal static extern int FaceLBPHGetGridX(IntPtr recognizer, out int gridX);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_lbph_set_grid_x")]
        internal static extern int FaceLBPHSetGridX(IntPtr recognizer, int gridX);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_lbph_get_grid_y")]
        internal static extern int FaceLBPHGetGridY(IntPtr recognizer, out int gridY);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_lbph_set_grid_y")]
        internal static extern int FaceLBPHSetGridY(IntPtr recognizer, int gridY);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_lbph_get_labels")]
        internal static extern int FaceLBPHGetLabels(IntPtr recognizer, out IntPtr labels);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_lbph_get_histograms_count")]
        internal static extern int FaceLBPHGetHistogramsCount(IntPtr recognizer, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_lbph_get_histograms_fill")]
        internal static extern int FaceLBPHGetHistogramsFill(IntPtr recognizer, IntPtr[] histograms, int histogramCapacity, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_standard_collector_create")]
        internal static extern int FaceStandardCollectorCreate(double threshold, out IntPtr collector);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_standard_collector_get_min_label")]
        internal static extern int FaceStandardCollectorGetMinLabel(IntPtr collector, out int label);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_standard_collector_get_min_dist")]
        internal static extern int FaceStandardCollectorGetMinDist(IntPtr collector, out double distance);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_standard_collector_get_results_count")]
        internal static extern int FaceStandardCollectorGetResultsCount(IntPtr collector, int sorted, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_standard_collector_get_results_fill")]
        internal static extern int FaceStandardCollectorGetResultsFill(IntPtr collector, int sorted, FacePredictionResultNative[] results, int resultCapacity, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_bif_create")]
        internal static extern int FaceBIFCreate(int numBands, int numRotations, out IntPtr bif);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_bif_get_num_bands")]
        internal static extern int FaceBIFGetNumBands(IntPtr bif, out int numBands);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_bif_get_num_rotations")]
        internal static extern int FaceBIFGetNumRotations(IntPtr bif, out int numRotations);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_bif_compute")]
        internal static extern int FaceBIFCompute(IntPtr bif, IntPtr image, IntPtr features);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_facemark_load_model")]
        internal static extern int FaceFacemarkLoadModel(IntPtr facemark, byte[] modelPath);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_facemark_fit")]
        internal static extern int FaceFacemarkFit(IntPtr facemark, IntPtr image, int[] faces, int faceCount, out int result);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_facemark_fit_landmarks_count")]
        internal static extern int FaceFacemarkFitLandmarksCount(IntPtr facemark, out int faceCount, out int pointCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_facemark_fit_landmarks_fill")]
        internal static extern int FaceFacemarkFitLandmarksFill(IntPtr facemark, int[] landmarkOffsets, int landmarkOffsetCapacity, float[] landmarksBuffer, int landmarkPointCapacity, out int faceCount, out int pointCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_facemark_train_add_sample")]
        internal static extern int FaceFacemarkTrainAddSample(IntPtr facemark, IntPtr image, float[] landmarks, int landmarkCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_facemark_train_training")]
        internal static extern int FaceFacemarkTrainTraining(IntPtr facemark);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_facemark_train_get_faces_count")]
        internal static extern int FaceFacemarkTrainGetFacesCount(IntPtr facemark, IntPtr image, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_facemark_train_get_faces_fill")]
        internal static extern int FaceFacemarkTrainGetFacesFill(IntPtr facemark, IntPtr image, int[] faces, int faceCapacity, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_facemark_save")]
        internal static extern int FaceFacemarkSave(IntPtr facemark, byte[] path);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_facemark_lbf_create")]
        internal static extern int FaceFacemarkLBFCreate(int nLandmarks, int initShapeN, int stagesN, int treeN, int treeDepth, double shapeOffset, double baggingOverlap, int verbose, out IntPtr facemark);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_facemark_lbf_create_ex")]
        internal static extern int FaceFacemarkLBFCreateEx(int nLandmarks, int initShapeN, int stagesN, int treeN, int treeDepth, double shapeOffset, double baggingOverlap, int verbose, int saveModel, uint seed, byte[] cascadeFace, byte[] modelFilename, int[] featsM, int featsCount, double[] radiusM, int radiusCount, int[] leftPupil, int leftPupilCount, int[] rightPupil, int rightPupilCount, int detectRoiX, int detectRoiY, int detectRoiWidth, int detectRoiHeight, out IntPtr facemark);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_mace_create")]
        internal static extern int FaceMaceCreate(int imgSize, out IntPtr mace);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_mace_load")]
        internal static extern int FaceMaceLoad(byte[] filename, byte[] objname, out IntPtr mace);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_mace_salt")]
        internal static extern int FaceMaceSalt(IntPtr mace, byte[] passphrase);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_mace_train")]
        internal static extern int FaceMaceTrain(IntPtr mace, IntPtr[] images, int imageCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_mace_same")]
        internal static extern int FaceMaceSame(IntPtr mace, IntPtr query, out int same);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_mace_save")]
        internal static extern int FaceMaceSave(IntPtr mace, byte[] path);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_face_mace_empty")]
        internal static extern int FaceMaceEmpty(IntPtr mace, out int empty);
    }
}
#endif

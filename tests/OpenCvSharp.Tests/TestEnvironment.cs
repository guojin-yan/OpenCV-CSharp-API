using System;

namespace JYPPX.OpenCvSharp.Tests
{
    internal static class TestEnvironment
    {
        internal const string NativeSmokeVariable = "OPENCV_CSHARP_NATIVE_SMOKE";
        internal const string UnstableNativeSmokeVariable = "OPENCV_CSHARP_UNSTABLE_NATIVE_SMOKE";
        internal const string HighGuiSmokeVariable = "OPENCV_CSHARP_HIGHGUI_SMOKE";
        internal const string FaceCascadeVariable = "OPENCV_CSHARP_FACE_CASCADE";
        internal const string FaceDetectorModelVariable = "OPENCV_CSHARP_FACE_DETECTOR_MODEL";
        internal const string FaceRecognizerModelVariable = "OPENCV_CSHARP_FACE_RECOGNIZER_MODEL";
        internal const string BrisqueModelVariable = "OPENCV_CSHARP_BRISQUE_MODEL";
        internal const string BrisqueRangeVariable = "OPENCV_CSHARP_BRISQUE_RANGE";
        internal const string MlModelDirVariable = "OPENCV_CSHARP_ML_MODEL_DIR";

        internal static string? GetNativeSmokeVariable()
        {
            return GetVariable(NativeSmokeVariable);
        }

        internal static bool IsNativeSmokeEnabled()
        {
            return IsFlagValueEnabled(GetNativeSmokeVariable());
        }

        internal static string? GetUnstableNativeSmokeVariable()
        {
            return GetVariable(UnstableNativeSmokeVariable);
        }

        internal static bool IsUnstableNativeSmokeEnabled()
        {
            return IsFlagValueEnabled(GetUnstableNativeSmokeVariable());
        }

        internal static string? GetHighGuiSmokeVariable()
        {
            return GetVariable(HighGuiSmokeVariable);
        }

        internal static bool IsHighGuiSmokeEnabled()
        {
            return IsFlagValueEnabled(GetHighGuiSmokeVariable());
        }

        internal static string? GetFaceCascadeVariable()
        {
            return GetVariable(FaceCascadeVariable);
        }

        internal static string? GetFaceDetectorModelVariable()
        {
            return GetVariable(FaceDetectorModelVariable);
        }

        internal static string? GetFaceRecognizerModelVariable()
        {
            return GetVariable(FaceRecognizerModelVariable);
        }

        internal static string? GetBrisqueModelVariable()
        {
            return GetVariable(BrisqueModelVariable);
        }

        internal static string? GetBrisqueRangeVariable()
        {
            return GetVariable(BrisqueRangeVariable);
        }

        internal static string? GetMlModelDirVariable()
        {
            return GetVariable(MlModelDirVariable);
        }

        internal static string? GetVariable(string name)
        {
            return Environment.GetEnvironmentVariable(name);
        }

        internal static bool IsFlagValueEnabled(string? value)
        {
            return string.Equals(value, "1", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
        }
    }
}

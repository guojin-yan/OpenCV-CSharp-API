using System;

namespace OpenCvSharp.Tests
{
    internal static class TestEnvironment
    {
        internal const string NativeSmokeVariable = "OPENCV_CSHARP_NATIVE_SMOKE";
        internal const string CompatibilityNativeSmokeAlias = "OPENCV5SHARP_NATIVE_SMOKE";
        internal const string UnstableNativeSmokeVariable = "OPENCV_CSHARP_UNSTABLE_NATIVE_SMOKE";
        internal const string CompatibilityUnstableNativeSmokeAlias = "OPENCV5SHARP_UNSTABLE_NATIVE_SMOKE";
        internal const string HighGuiSmokeVariable = "OPENCV_CSHARP_HIGHGUI_SMOKE";
        internal const string CompatibilityHighGuiSmokeAlias = "OPENCV5SHARP_HIGHGUI_SMOKE";
        internal const string FaceCascadeVariable = "OPENCV_CSHARP_FACE_CASCADE";
        internal const string CompatibilityFaceCascadeAlias = "OPENCV5SHARP_FACE_CASCADE";
        internal const string FaceDetectorModelVariable = "OPENCV_CSHARP_FACE_DETECTOR_MODEL";
        internal const string CompatibilityFaceDetectorModelAlias = "OPENCV5SHARP_FACE_DETECTOR_MODEL";
        internal const string FaceRecognizerModelVariable = "OPENCV_CSHARP_FACE_RECOGNIZER_MODEL";
        internal const string CompatibilityFaceRecognizerModelAlias = "OPENCV5SHARP_FACE_RECOGNIZER_MODEL";
        internal const string BrisqueModelVariable = "OPENCV_CSHARP_BRISQUE_MODEL";
        internal const string CompatibilityBrisqueModelAlias = "OPENCV5SHARP_BRISQUE_MODEL";
        internal const string BrisqueRangeVariable = "OPENCV_CSHARP_BRISQUE_RANGE";
        internal const string CompatibilityBrisqueRangeAlias = "OPENCV5SHARP_BRISQUE_RANGE";
        internal const string MlModelDirVariable = "OPENCV_CSHARP_ML_MODEL_DIR";
        internal const string CompatibilityMlModelDirAlias = "OPENCV5SHARP_ML_MODEL_DIR";

        internal static string? GetNativeSmokeVariable()
        {
            return GetVariable(NativeSmokeVariable, CompatibilityNativeSmokeAlias);
        }

        internal static bool IsNativeSmokeEnabled()
        {
            return IsFlagValueEnabled(GetNativeSmokeVariable());
        }

        internal static string? GetUnstableNativeSmokeVariable()
        {
            return GetVariable(UnstableNativeSmokeVariable, CompatibilityUnstableNativeSmokeAlias);
        }

        internal static bool IsUnstableNativeSmokeEnabled()
        {
            return IsFlagValueEnabled(GetUnstableNativeSmokeVariable());
        }

        internal static string? GetHighGuiSmokeVariable()
        {
            return GetVariable(HighGuiSmokeVariable, CompatibilityHighGuiSmokeAlias);
        }

        internal static bool IsHighGuiSmokeEnabled()
        {
            return IsFlagValueEnabled(GetHighGuiSmokeVariable());
        }

        internal static string? GetFaceCascadeVariable()
        {
            return GetVariable(FaceCascadeVariable, CompatibilityFaceCascadeAlias);
        }

        internal static string? GetFaceDetectorModelVariable()
        {
            return GetVariable(FaceDetectorModelVariable, CompatibilityFaceDetectorModelAlias);
        }

        internal static string? GetFaceRecognizerModelVariable()
        {
            return GetVariable(FaceRecognizerModelVariable, CompatibilityFaceRecognizerModelAlias);
        }

        internal static string? GetBrisqueModelVariable()
        {
            return GetVariable(BrisqueModelVariable, CompatibilityBrisqueModelAlias);
        }

        internal static string? GetBrisqueRangeVariable()
        {
            return GetVariable(BrisqueRangeVariable, CompatibilityBrisqueRangeAlias);
        }

        internal static string? GetMlModelDirVariable()
        {
            return GetVariable(MlModelDirVariable, CompatibilityMlModelDirAlias);
        }

        internal static string? GetVariable(string neutralName, string compatibilityAliasName)
        {
            string? neutralValue = Environment.GetEnvironmentVariable(neutralName);
            return string.IsNullOrEmpty(neutralValue) ? Environment.GetEnvironmentVariable(compatibilityAliasName) : neutralValue;
        }

        internal static string? ChooseVariableValue(string? neutralValue, string? compatibilityAliasValue)
        {
            return string.IsNullOrEmpty(neutralValue) ? compatibilityAliasValue : neutralValue;
        }

        internal static bool IsFlagValueEnabled(string? value)
        {
            return string.Equals(value, "1", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
        }
    }
}

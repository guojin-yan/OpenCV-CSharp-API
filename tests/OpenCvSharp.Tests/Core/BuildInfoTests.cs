using System;
using JYPPX.OpenCvSharp;
using JYPPX.OpenCvSharp.Core;

namespace JYPPX.OpenCvSharp.Tests.Core
{
    public class BuildInfoTests
    {
        [Fact]
        public void BuildInfoContainsOpenCvVersion()
        {
            string value = OpenCvSharpBuildInfo.GetDisplayString();

            Assert.Contains("5.0.0", value);
            Assert.Equal("5.0.0", OpenCvSharpBuildInfo.OpenCvVersion);
            Assert.Equal("5.0.0.0", OpenCvSharpBuildInfo.PackageVersion);
            Assert.Equal("JYPPX.OpenCV.CSharp.API", OpenCvSharpBuildInfo.ManagedPackageId);
            Assert.Equal("JYPPX.OpenCV.runtime", OpenCvSharpBuildInfo.RuntimePackageIdPrefix);
            Assert.Equal("JYPPX.OpenCV.Native", OpenCvSharpBuildInfo.CurrentNativeLibraryName);
        }

        [Fact]
        public void PrimaryManagedIdentityIsVersionNeutral()
        {
            Assert.Equal("JYPPX.OpenCV.CSharp.API", typeof(OpenCvSharpBuildInfo).Assembly.GetName().Name);
            Assert.Equal("JYPPX.OpenCvSharp", typeof(OpenCvSharpBuildInfo).Namespace);
            Assert.Equal("JYPPX.OpenCvSharp.Core", typeof(Mat).Namespace);
        }

        [Fact]
        public void NativeOpenCvVersionMatchesManagedTargetWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Assert.Equal(OpenCvSharpBuildInfo.OpenCvVersion, OpenCvSharpBuildInfo.GetNativeOpenCvVersion());
            Assert.True(OpenCvSharpBuildInfo.IsNativeOpenCvVersionCompatible());
            OpenCvSharpBuildInfo.VerifyNativeOpenCvVersionCompatibility();

        }

        [Fact]
        public void TestEnvironmentPrefersNeutralVariableNames()
        {
            Assert.Equal("OPENCV_CSHARP_NATIVE_SMOKE", TestEnvironment.NativeSmokeVariable);
            Assert.Equal("OPENCV_CSHARP_UNSTABLE_NATIVE_SMOKE", TestEnvironment.UnstableNativeSmokeVariable);
            Assert.Equal("OPENCV_CSHARP_HIGHGUI_SMOKE", TestEnvironment.HighGuiSmokeVariable);
            Assert.Equal("OPENCV_CSHARP_FACE_CASCADE", TestEnvironment.FaceCascadeVariable);
            Assert.Equal("OPENCV_CSHARP_FACE_DETECTOR_MODEL", TestEnvironment.FaceDetectorModelVariable);
            Assert.Equal("OPENCV_CSHARP_FACE_RECOGNIZER_MODEL", TestEnvironment.FaceRecognizerModelVariable);
            Assert.Equal("OPENCV_CSHARP_BRISQUE_MODEL", TestEnvironment.BrisqueModelVariable);
            Assert.Equal("OPENCV_CSHARP_BRISQUE_RANGE", TestEnvironment.BrisqueRangeVariable);
            Assert.Equal("OPENCV_CSHARP_ML_MODEL_DIR", TestEnvironment.MlModelDirVariable);
            Assert.True(TestEnvironment.IsFlagValueEnabled("1"));
            Assert.True(TestEnvironment.IsFlagValueEnabled("true"));
            Assert.False(TestEnvironment.IsFlagValueEnabled("0"));
            Assert.Equal(TestEnvironment.IsFlagValueEnabled(TestEnvironment.GetNativeSmokeVariable()), TestEnvironment.IsNativeSmokeEnabled());
            Assert.Equal(TestEnvironment.IsFlagValueEnabled(TestEnvironment.GetUnstableNativeSmokeVariable()), TestEnvironment.IsUnstableNativeSmokeEnabled());
            Assert.Equal(TestEnvironment.IsFlagValueEnabled(TestEnvironment.GetHighGuiSmokeVariable()), TestEnvironment.IsHighGuiSmokeEnabled());
        }

        [Fact]
        public void BufferCopyCopiesBytes()
        {
            byte[] source = new byte[] { 1, 2, 3, 4 };
            byte[] destination = new byte[4];

            CvBuffer.Copy(source, destination);

            Assert.Equal(source, destination);
        }

        [Fact]
        public void MatTypeMakeTypeMatchesOpenCvEncoding()
        {
            Assert.Equal(128, MatType.ChannelMax);
            Assert.Equal(5, MatType.ChannelShift);
            Assert.Equal(32, MatType.DepthMax);
            Assert.Equal(31, MatType.DepthMask);
            Assert.Equal(4064, MatType.ChannelMask);
            Assert.Equal(4095, MatType.MatrixTypeMask);

            Assert.Equal(64, MatType.CV_8UC3);
            Assert.Equal(96, MatType.CV_8UC4);
            Assert.Equal(MatType.CV_8UC3, MatType.MakeType(0, 3));
            Assert.Equal(MatType.CV_32FC1, MatType.MakeType(5, 1));
            Assert.Equal(MatType.CV_64FC4, MatType.MakeType(MatType.CV_64F, 4));
            Assert.Equal(MatType.CV_32UC4, MatType.MakeType(MatType.CV_32U, 4));
        }

        [Fact]
        public void MatTypeCanDecodeDepthChannelsAndTypeMask()
        {
            Assert.Equal(MatType.CV_8U, MatType.Depth(MatType.CV_8UC3));
            Assert.Equal(3, MatType.Channels(MatType.CV_8UC3));
            Assert.Equal(MatType.CV_32F, MatType.Depth(MatType.CV_32FC4));
            Assert.Equal(4, MatType.Channels(MatType.CV_32FC4));
            Assert.Equal(MatType.CV_8UC3, MatType.TypeMask(MatType.CV_8UC3 | 16384));
        }

        [Fact]
        public void MatTypeMakeTypeRejectsOutOfRangeValues()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => MatType.MakeType(-1, 1));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => MatType.MakeType(MatType.DepthMax, 1));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => MatType.MakeType(MatType.CV_8U, 0));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => MatType.MakeType(MatType.CV_8U, MatType.ChannelMax + 1));
        }
    }
}

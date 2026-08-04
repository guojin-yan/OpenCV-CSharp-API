using System;
using System.Globalization;
using System.Runtime.InteropServices;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Reg;

namespace JYPPX.OpenCvSharp.Tests.Reg
{
    public sealed class RegTests
    {
        [Fact]
        public void ValueTypesExposeExpectedValues()
        {
            var affine = new AffineTransform2D(1, 2, 3, 4, 5, 6);
            var sameAffine = new AffineTransform2D(1, 2, 3, 4, 5, 6);
            var differentAffine = new AffineTransform2D(1, 2, 3, 4, 5, 7);
            Assert.Equal(new double[] { 1, 2, 3, 4, 5, 6 }, affine.ToArray());
            Assert.Equal(affine, AffineTransform2D.FromArray(new double[] { 1, 2, 3, 4, 5, 6 }));
            Assert.Equal(AffineTransform2D.Identity, new AffineTransform2D(1, 0, 0, 1, 0, 0));
            Assert.True(affine == sameAffine);
            Assert.False(affine != sameAffine);
            Assert.True(affine != differentAffine);
            Assert.False(affine.Equals("not a transform"));
            Assert.Equal(sameAffine.GetHashCode(), affine.GetHashCode());

            var projective = new ProjectiveTransform2D(1, 2, 3, 4, 5, 6, 7, 8, 9);
            var sameProjective = new ProjectiveTransform2D(1, 2, 3, 4, 5, 6, 7, 8, 9);
            var differentProjective = new ProjectiveTransform2D(1, 2, 3, 4, 5, 6, 7, 8, 10);
            Assert.Equal(new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, projective.ToArray());
            Assert.Equal(projective, ProjectiveTransform2D.FromArray(new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }));
            Assert.Equal(ProjectiveTransform2D.Identity, new ProjectiveTransform2D(1, 0, 0, 0, 1, 0, 0, 0, 1));
            Assert.True(projective == sameProjective);
            Assert.False(projective != sameProjective);
            Assert.True(projective != differentProjective);
            Assert.False(projective.Equals("not a transform"));
            Assert.Equal(sameProjective.GetHashCode(), projective.GetHashCode());
            Assert.Equal(RegMapKind.Shift, (RegMapKind)1);
            Assert.Equal(RegMapKind.Affine, (RegMapKind)2);
            Assert.Equal(RegMapKind.Projec, (RegMapKind)3);
        }

        [Fact]
        public void TransformValueTypesRejectInvalidArrayInputs()
        {
            Assert.Throws<ArgumentNullException>(() => AffineTransform2D.FromArray(null!));
            ArgumentException affineException = Assert.Throws<ArgumentException>(() =>
                AffineTransform2D.FromArray(new double[] { 1, 2, 3, 4, 5 }));
            Assert.Equal("values", affineException.ParamName);

            Assert.Throws<ArgumentNullException>(() => ProjectiveTransform2D.FromArray(null!));
            ArgumentException projectiveException = Assert.Throws<ArgumentException>(() =>
                ProjectiveTransform2D.FromArray(new double[] { 1, 2, 3, 4, 5, 6, 7, 8 }));
            Assert.Equal("values", projectiveException.ParamName);
        }

        [Fact]
        public void TransformValueTypesFormatInvariantly()
        {
            CultureInfo originalCulture = CultureInfo.CurrentCulture;
            CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
                CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");

                Assert.Equal(
                    "{M00=1.25,M01=2.5,M10=3.75,M11=4.125,ShiftX=5.25,ShiftY=6.5}",
                    new AffineTransform2D(1.25, 2.5, 3.75, 4.125, 5.25, 6.5).ToString());
                Assert.Equal(
                    "{M00=1.25,M01=2.5,M02=3.75,M10=4.125,M11=5.25,M12=6.5,M20=7.75,M21=8.875,M22=9.125}",
                    new ProjectiveTransform2D(1.25, 2.5, 3.75, 4.125, 5.25, 6.5, 7.75, 8.875, 9.125).ToString());
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUiCulture;
            }
        }

        [Fact]
        public void TransformValueTypesHaveSequentialOpenCvCompatibleLayout()
        {
            Assert.Equal(48, Marshal.SizeOf<AffineTransform2D>());
            Assert.Equal(72, Marshal.SizeOf<ProjectiveTransform2D>());

            Assert.Equal(0, FieldOffset<AffineTransform2D>("<M00>k__BackingField"));
            Assert.Equal(8, FieldOffset<AffineTransform2D>("<M01>k__BackingField"));
            Assert.Equal(16, FieldOffset<AffineTransform2D>("<M10>k__BackingField"));
            Assert.Equal(24, FieldOffset<AffineTransform2D>("<M11>k__BackingField"));
            Assert.Equal(32, FieldOffset<AffineTransform2D>("<ShiftX>k__BackingField"));
            Assert.Equal(40, FieldOffset<AffineTransform2D>("<ShiftY>k__BackingField"));

            Assert.Equal(0, FieldOffset<ProjectiveTransform2D>("<M00>k__BackingField"));
            Assert.Equal(8, FieldOffset<ProjectiveTransform2D>("<M01>k__BackingField"));
            Assert.Equal(16, FieldOffset<ProjectiveTransform2D>("<M02>k__BackingField"));
            Assert.Equal(24, FieldOffset<ProjectiveTransform2D>("<M10>k__BackingField"));
            Assert.Equal(32, FieldOffset<ProjectiveTransform2D>("<M11>k__BackingField"));
            Assert.Equal(40, FieldOffset<ProjectiveTransform2D>("<M12>k__BackingField"));
            Assert.Equal(48, FieldOffset<ProjectiveTransform2D>("<M20>k__BackingField"));
            Assert.Equal(56, FieldOffset<ProjectiveTransform2D>("<M21>k__BackingField"));
            Assert.Equal(64, FieldOffset<ProjectiveTransform2D>("<M22>k__BackingField"));
        }

        [Fact]
        public void MapValidationAndDisposedStateRunWhenNativeObjectIsAvailable()
        {
            using (MapShift? map = TryCreateMapShift())
            {
                if (map == null)
                {
                    return;
                }

                using (Mat image = CreateGrayImage())
                using (Mat output = new Mat())
                {
                    Assert.Equal(RegMapKind.Shift, map.Kind);
                    Assert.Throws<ArgumentNullException>(() => map.Warp(null!, output));
                    Assert.Throws<ArgumentNullException>(() => map.Warp(image, null!));
                    Assert.Throws<ArgumentNullException>(() => map.Warp(null!));
                    Assert.Throws<ArgumentNullException>(() => map.InverseWarp(null!, output));
                    Assert.Throws<ArgumentNullException>(() => map.InverseWarp(image, null!));
                    Assert.Throws<ArgumentNullException>(() => map.InverseWarp(null!));
                    Assert.Throws<ArgumentNullException>(() => map.Compose(null!));
                    Assert.Throws<ArgumentOutOfRangeException>(() => map.Scale(double.NaN));
                    map.GetShift(out double shiftX, out double shiftY);
                    Assert.Equal(1.0, shiftX);
                    Assert.Equal(2.0, shiftY);

                    using (MapAffine affine = RegCv2.CreateMapAffine(new AffineTransform2D(1.0, 0.25, 0.5, 1.0, 2.0, 3.0)))
                    using (MapProjec projective = RegCv2.CreateMapProjec(new ProjectiveTransform2D(2.0, 0.0, 4.0, 0.0, 2.0, 6.0, 0.0, 0.0, 2.0)))
                    {
                        Assert.Equal(new AffineTransform2D(1.0, 0.25, 0.5, 1.0, 2.0, 3.0), affine.Transform);
                        Assert.Equal(new ProjectiveTransform2D(2.0, 0.0, 4.0, 0.0, 2.0, 6.0, 0.0, 0.0, 2.0), projective.Transform);

                        projective.Normalize();
                        Assert.Equal(new ProjectiveTransform2D(1.0, 0.0, 2.0, 0.0, 1.0, 3.0, 0.0, 0.0, 1.0), projective.Transform);
                    }

                    map.Dispose();
                    Assert.True(map.IsDisposed);
                    Assert.Throws<ObjectDisposedException>(() => map.Warp(image, output));
                    Assert.Throws<ObjectDisposedException>(() => map.Warp(image));
                    Assert.Throws<ObjectDisposedException>(() => map.InverseWarp(image, output));
                    Assert.Throws<ObjectDisposedException>(() => map.InverseWarp(image));
                    Assert.Throws<ObjectDisposedException>(() => map.InverseMap());
                    Assert.Throws<ObjectDisposedException>(() => map.Kind);
                }
            }
        }

        [Fact]
        public void MapperValidationAndDisposedStateRunWhenNativeObjectIsAvailable()
        {
            using (MapperGradShift? mapper = TryCreateMapperGradShift())
            {
                if (mapper == null)
                {
                    return;
                }

                using (Mat image = CreateGrayImage())
                {
                    Assert.Throws<ArgumentNullException>(() => mapper.Calculate(null!, image));
                    Assert.Throws<ArgumentNullException>(() => mapper.Calculate(image, null!));

                    using (MapperPyramid pyramid = RegCv2.CreateMapperPyramid(mapper))
                    {
                        Assert.True(pyramid.NumLevels > 0);
                        Assert.True(pyramid.NumIterationsPerScale > 0);
                        Assert.Throws<ArgumentOutOfRangeException>(() => pyramid.NumLevels = 0);
                        Assert.Throws<ArgumentOutOfRangeException>(() => pyramid.NumIterationsPerScale = 0);
                    }

                    mapper.Dispose();
                    Assert.True(mapper.IsDisposed);
                    Assert.Throws<ObjectDisposedException>(() => mapper.Calculate(image, image));
                    Assert.Throws<ObjectDisposedException>(() => mapper.GetMap());
                }
            }
        }

        [Fact]
        public void LinkedSmokeRunsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            try
            {
                using (Mat image = CreateGrayImage())
                using (MapShift shiftMap = RegCv2.CreateMapShift(1.0, 0.0))
                using (Mat shifted = shiftMap.InverseWarp(image))
                using (MapperGradShift mapper = RegCv2.CreateMapperGradShift())
                using (RegMap result = mapper.Calculate(image, shifted))
                using (Mat warped = result.Warp(shifted))
                using (RegMap inverse = result.InverseMap())
                using (Mat restored = inverse.Warp(image))
                {
                    Assert.Equal(RegMapKind.Shift, result.Kind);
                    Assert.False(warped.Empty);
                    Assert.Equal(image.Rows, warped.Rows);
                    Assert.Equal(image.Cols, warped.Cols);
                    Assert.False(restored.Empty);
                    Assert.Equal(image.Rows, restored.Rows);
                    Assert.Equal(image.Cols, restored.Cols);
                }
            }
            catch (OpenCvException ex) when (IsRegModuleMissing(ex))
            {
                Assert.True(IsRegModuleMissing(ex), ex.Message);
            }
        }

        private static MapShift? TryCreateMapShift()
        {
            try
            {
                return RegCv2.CreateMapShift(1.0, 2.0);
            }
            catch (OpenCvException ex) when (IsRegModuleMissing(ex))
            {
                return null;
            }
            catch (DllNotFoundException)
            {
                return null;
            }
            catch (EntryPointNotFoundException)
            {
                return null;
            }
        }

        private static MapperGradShift? TryCreateMapperGradShift()
        {
            try
            {
                return RegCv2.CreateMapperGradShift();
            }
            catch (OpenCvException ex) when (IsRegModuleMissing(ex))
            {
                return null;
            }
            catch (DllNotFoundException)
            {
                return null;
            }
            catch (EntryPointNotFoundException)
            {
                return null;
            }
        }

        private static Mat CreateGrayImage()
        {
            var mat = new Mat(32, 32, MatType.CV_8UC1);
            var values = new byte[32 * 32];
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    values[y * 32 + x] = (byte)((x >= 8 && x < 24 && y >= 8 && y < 24) ? 220 : 30);
                }
            }

            mat.CopyFrom(values);
            return mat;
        }

        private static bool IsRegModuleMissing(OpenCvException exception)
        {
            return exception.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exception.Message.IndexOf("linked with OpenCV", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exception.Message.IndexOf("reg", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static int FieldOffset<T>(string fieldName)
        {
            return Marshal.OffsetOf<T>(fieldName).ToInt32();
        }
    }
}

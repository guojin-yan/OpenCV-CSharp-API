using System;
using JYPPX.OpenCvSharp.Calib3D;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Geometry;
using JYPPX.OpenCvSharp.ImgProc;
using Calib3DCv2 = JYPPX.OpenCvSharp.Calib3D.Cv2;
using ImgProcCv2 = JYPPX.OpenCvSharp.ImgProc.Cv2;

namespace JYPPX.OpenCvSharp.Tests.ImgProc
{
    public class ImgProcRemainingParityTests
    {
        [Fact]
        public void RemainingParitySurfaceValidatesManagedArguments()
        {
            using (Mat mat = new Mat())
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => ImgProcCv2.GetRectSubPix(mat, new Size(), new Point2f(), mat));
                Assert.Throws<ArgumentOutOfRangeException>(() => ImgProcCv2.WarpPolar(mat, mat, new Size(), new Point2f(), 0, InterpolationFlags.Linear));
                Assert.Throws<ArgumentOutOfRangeException>(() => ImgProcCv2.AccumulateWeighted(mat, mat, 2));
                Assert.Throws<ArgumentOutOfRangeException>(() => ImgProcCv2.PhaseCorrelateIterative(mat, mat, 0));
                Assert.Throws<ArgumentOutOfRangeException>(() => ImgProcCv2.CreateHanningWindow(mat, new Size(1, 8), MatType.CV_32F));
                Assert.Throws<ArgumentOutOfRangeException>(() => ImgProcCv2.EMD(mat, mat, (DistanceTypes)0));
                Assert.Throws<ArgumentOutOfRangeException>(() => ImgProcCv2.PyrMeanShiftFiltering(mat, mat, 0, 1));
                Assert.Throws<ArgumentOutOfRangeException>(() => ImgProcCv2.GrabCut(mat, mat, new Rect(), mat, mat, -1));
                Assert.Throws<ArgumentOutOfRangeException>(() => ImgProcCv2.MatchTemplate(mat, mat, mat, (TemplateMatchModes)99));
                Assert.Throws<ArgumentOutOfRangeException>(() => new TextWrapRange(5, 4));
                Assert.Throws<ArgumentOutOfRangeException>(() => Calib3DCv2.DrawFrameAxes(mat, mat, mat, mat, mat, 0));
            }
        }

        [Fact]
        public void CalibrationSamplingAndAxesExecuteWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat color = CreateColorFixture(16, 16))
            using (Mat camera = CreateCameraMatrix())
            using (Mat distortion = Mat.Zeros(new Size(5, 1), MatType.CV_64FC1))
            using (Mat fisheyeDistortion = Mat.Zeros(new Size(4, 1), MatType.CV_64FC1))
            using (Mat rectification = Mat.Eye(3, 3, MatType.CV_64FC1))
            using (Mat undistorted = Calib3DCv2.Undistort(color, camera, distortion, camera))
            using (Mat fisheye = Calib3DCv2.FisheyeUndistortImage(color, camera, fisheyeDistortion, camera, new Size(16, 16)))
            using (Mat patch = ImgProcCv2.GetRectSubPix(color, new Size(5, 5), new Point2f(8, 8)))
            using (Mat polar = ImgProcCv2.WarpPolar(color, new Size(16, 16), new Point2f(8, 8), 7, InterpolationFlags.Linear))
            using (Mat rvec = Mat.Zeros(new Size(1, 3), MatType.CV_64FC1))
            using (Mat tvec = Mat.Zeros(new Size(1, 3), MatType.CV_64FC1))
            {
                UndistortRectifyMapResult maps = Calib3DCv2.InitInverseRectificationMap(
                    camera,
                    distortion,
                    rectification,
                    camera,
                    new Size(16, 16),
                    MatType.CV_32FC1);
                using (maps.Map1)
                using (maps.Map2)
                {
                    tvec.CopyFrom<double>(new[] { 0.0, 0.0, 2.0 });
                    Calib3DCv2.DrawFrameAxes(color, camera, distortion, rvec, tvec, 0.5F, 1);

                    Assert.Equal(16, undistorted.Rows);
                    Assert.Equal(16, fisheye.Cols);
                    Assert.Equal(5, patch.Rows);
                    Assert.Equal(16, polar.Cols);
                    Assert.Equal(16, maps.Rows);
                    Assert.Equal(MatType.CV_32FC1, maps.Map1.Type);
                }
            }
        }

        [Fact]
        public void AccumulationPhaseCorrelationAndEmdExecuteWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat src1 = Mat.Zeros(new Size(8, 8), MatType.CV_32FC1))
            using (Mat src2 = Mat.Zeros(new Size(8, 8), MatType.CV_32FC1))
            using (Mat accumulator = Mat.Zeros(new Size(8, 8), MatType.CV_32FC1))
            using (Mat window = ImgProcCv2.CreateHanningWindow(new Size(8, 8), MatType.CV_32F))
            using (Mat signature1 = new Mat(2, 2, MatType.CV_32FC1))
            using (Mat signature2 = new Mat(2, 2, MatType.CV_32FC1))
            using (Mat flow = new Mat())
            {
                float[] values1 = new float[64];
                float[] values2 = new float[64];
                values1[2 * 8 + 2] = 1;
                values2[3 * 8 + 4] = 1;
                src1.CopyFrom<float>(values1);
                src2.CopyFrom<float>(values2);

                ImgProcCv2.Accumulate(src1, accumulator);
                ImgProcCv2.AccumulateSquare(src1, accumulator);
                ImgProcCv2.AccumulateProduct(src1, src2, accumulator);
                ImgProcCv2.AccumulateWeighted(src1, accumulator, 0.25);
                Point2d shift = ImgProcCv2.PhaseCorrelate(src1, src2, window, out double response);
                Point2d iterativeShift = ImgProcCv2.PhaseCorrelateIterative(src1, src2);

                signature1.CopyFrom<float>(new[] { 1.0F, 0.0F, 1.0F, 1.0F });
                signature2.CopyFrom<float>(new[] { 1.0F, 0.25F, 1.0F, 1.25F });
                float lowerBound = 0;
                float distance = ImgProcCv2.EMD(signature1, signature2, DistanceTypes.L2, ref lowerBound, flow: flow);

                Assert.Equal(MatType.CV_32FC1, window.Type);
                Assert.True(double.IsFinite(shift.X));
                Assert.True(double.IsFinite(iterativeShift.Y));
                Assert.True(double.IsFinite(response));
                Assert.True(distance > 0);
                Assert.True(lowerBound >= 0);
                Assert.Equal(2, flow.Rows);
            }
        }

        [Fact]
        public void SegmentationMatchingAndLinkRunsExecuteWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat color = CreateColorFixture(16, 16))
            using (Mat filtered = ImgProcCv2.PyrMeanShiftFiltering(color, 2, 8, 0))
            using (Mat markers = Mat.Zeros(new Size(16, 16), MatType.CV_32SC1))
            using (Mat grabMask = Mat.Zeros(new Size(16, 16), MatType.CV_8UC1))
            using (Mat backgroundModel = new Mat())
            using (Mat foregroundModel = new Mat())
            using (Mat gray = Mat.Zeros(new Size(16, 16), MatType.CV_8UC1))
            using (Mat template = Mat.Zeros(new Size(3, 3), MatType.CV_8UC1))
            {
                int[] markerValues = new int[256];
                markerValues[2 * 16 + 2] = 1;
                markerValues[13 * 16 + 13] = 2;
                markers.CopyFrom<int>(markerValues);
                ImgProcCv2.Watershed(color, markers);
                ImgProcCv2.GrabCut(color, grabMask, new Rect(2, 2, 12, 12), backgroundModel, foregroundModel, 1, GrabCutModes.InitWithRect);

                ImgProcCv2.Rectangle(gray, new Rect(3, 3, 8, 8), new Scalar(255), -1);
                using (Mat response = ImgProcCv2.MatchTemplate(gray, template, TemplateMatchModes.CCoeffNormed))
                {
                    ImgProcCv2.FindContoursLinkRuns(gray, out Point[][] contours, out Vec4i[] hierarchy);
                    Point[][] contoursWithoutHierarchy = ImgProcCv2.FindContoursLinkRuns(gray);

                    Assert.Equal(16, filtered.Rows);
                    Assert.Equal(14, response.Rows);
                    Assert.NotEmpty(contours);
                    Assert.Equal(contours.Length, hierarchy.Length);
                    Assert.Equal(contours.Length, contoursWithoutHierarchy.Length);
                    Assert.Equal(1, backgroundModel.Rows);
                    Assert.Equal(1, foregroundModel.Rows);
                }
            }
        }

        [Fact]
        public void FontFaceLifecycleAndRenderingExecuteWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            var font = new FontFace("sans");
            using (Mat canvas = Mat.Zeros(new Size(120, 40), MatType.CV_8UC3))
            {
                Assert.Equal("sans", font.Name);
                Assert.True(font.Set("sans"));
                Assert.Throws<ArgumentException>(() => font.SetInstance(new[] { 1 }));
                Assert.Throws<ArgumentOutOfRangeException>(() => ImgProcCv2.PutText(canvas, "x", new Point(), new Scalar(), font, 0));
                Assert.True(font.SetInstance(Array.Empty<int>()));
                Assert.True(font.GetInstance(out int[] parameters));
                Assert.Equal(0, parameters.Length % 2);

                Point next = ImgProcCv2.PutText(canvas, "OpenCV", new Point(2, 24), new Scalar(255, 255, 255), font, 16);
                Rect bounds = ImgProcCv2.GetTextSize(new Size(120, 40), "OpenCV", new Point(2, 24), font, 16);
                Point unicodeNext = ImgProcCv2.PutText(canvas, "中文", new Point(2, 38), new Scalar(255, 255, 255), font, 12);

                Assert.True(next.X > 2);
                Assert.True(unicodeNext.X > 2);
                Assert.True(bounds.Width > 0);
                Assert.True(bounds.Height > 0);
                Assert.False(font.IsDisposed);
            }

            font.Dispose();
            Assert.True(font.IsDisposed);
            Assert.Throws<ObjectDisposedException>(() => font.GetName());
            Assert.Throws<ObjectDisposedException>(() => font.Set("sans"));
        }

        private static Mat CreateCameraMatrix()
        {
            var camera = new Mat(3, 3, MatType.CV_64FC1);
            camera.CopyFrom<double>(new[]
            {
                12.0, 0.0, 8.0,
                0.0, 12.0, 8.0,
                0.0, 0.0, 1.0
            });
            return camera;
        }

        private static Mat CreateColorFixture(int rows, int cols)
        {
            var image = new Mat(rows, cols, MatType.CV_8UC3);
            var pixels = new byte[rows * cols * 3];
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    bool inside = x >= 3 && x < cols - 3 && y >= 3 && y < rows - 3;
                    int offset = (y * cols + x) * 3;
                    pixels[offset] = (byte)(inside ? 180 + x % 5 : 10 + x);
                    pixels[offset + 1] = (byte)(inside ? 40 + y % 7 : 15 + y);
                    pixels[offset + 2] = (byte)(inside ? 60 + (x + y) % 9 : 20 + (x + y) % 5);
                }
            }
            image.CopyFrom(pixels);
            return image;
        }
    }
}

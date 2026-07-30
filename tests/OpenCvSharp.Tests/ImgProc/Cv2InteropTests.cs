using System;
using OpenCvSharp.Core;
using OpenCvSharp.Geometry;
using OpenCvSharp.ImgProc;
using ImgProcCv2 = OpenCvSharp.ImgProc.Cv2;

namespace OpenCvSharp.Tests.ImgProc
{
    public class Cv2InteropTests
    {
        [Fact]
        public void CvtColorAndResizeProduceExpectedPixelsWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            byte[] bgrPixels = new byte[]
            {
                0, 0, 0,
                10, 20, 30,
                50, 100, 150,
                255, 255, 255
            };

            using (Mat src = new Mat(2, 2, MatType.CV_8UC3))
            using (Mat gray = new Mat())
            using (Mat resized = new Mat())
            {
                Assert.Equal(MatType.CV_8UC3, src.Type);
                Assert.Equal(3, src.Channels);
                Assert.Equal((UIntPtr)3, src.ElemSize);

                src.CopyFrom(bgrPixels);
                ImgProcCv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);

                Assert.False(gray.Empty);
                Assert.Equal(2, gray.Rows);
                Assert.Equal(2, gray.Cols);
                Assert.Equal(1, gray.Channels);

                byte[] grayPixels = new byte[gray.ByteLength];
                gray.CopyTo(grayPixels);
                Assert.Equal(new byte[] { 0, 22, 109, 255 }, grayPixels);

                ImgProcCv2.Resize(gray, resized, new Size(4, 4), interpolation: InterpolationFlags.Nearest);

                Assert.False(resized.Empty);
                Assert.Equal(4, resized.Rows);
                Assert.Equal(4, resized.Cols);
                Assert.Equal(1, resized.Channels);

                byte[] resizedPixels = new byte[resized.ByteLength];
                resized.CopyTo(resizedPixels);
                Assert.Equal(
                    new byte[]
                    {
                        0, 0, 22, 22,
                        0, 0, 22, 22,
                        109, 109, 255, 255,
                        109, 109, 255, 255
                    },
                    resizedPixels);
            }
        }

        [Fact]
        public void ThresholdProducesExpectedPixelsWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat src = new Mat(2, 3, MatType.CV_8UC1))
            using (Mat dst = new Mat())
            {
                src.CopyFrom(new byte[] { 0, 80, 120, 160, 200, 255 });

                double threshold = ImgProcCv2.Threshold(src, dst, 127, 255, ThresholdTypes.Binary);

                Assert.Equal(127, threshold);
                Assert.Equal(2, dst.Rows);
                Assert.Equal(3, dst.Cols);
                Assert.Equal(MatType.CV_8UC1, dst.Type);

                byte[] pixels = new byte[dst.ByteLength];
                dst.CopyTo(pixels);
                Assert.Equal(new byte[] { 0, 0, 0, 255, 255, 255 }, pixels);
            }
        }

        [Fact]
        public void GaussianBlurSpreadsCenterPixelWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat src = new Mat(5, 5, MatType.CV_8UC1))
            using (Mat dst = new Mat())
            {
                src.CopyFrom(new byte[]
                {
                    0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0,
                    0, 0, 255, 0, 0,
                    0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0
                });

                ImgProcCv2.GaussianBlur(src, dst, new Size(3, 3), 0, 0, BorderTypes.Default);

                Assert.Equal(5, dst.Rows);
                Assert.Equal(5, dst.Cols);
                Assert.Equal(MatType.CV_8UC1, dst.Type);

                byte[] pixels = new byte[dst.ByteLength];
                dst.CopyTo(pixels);

                Assert.Equal(
                    new byte[]
                    {
                        0, 0, 0, 0, 0,
                        0, 16, 32, 16, 0,
                        0, 32, 64, 32, 0,
                        0, 16, 32, 16, 0,
                        0, 0, 0, 0, 0
                    },
                    pixels);
            }
        }

        [Fact]
        public void FilteringBatchProducesExpectedShapesAndPixelsWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat src = new Mat(5, 5, MatType.CV_8UC1))
            using (Mat blurred = new Mat())
            using (Mat boxed = new Mat())
            using (Mat squared = new Mat())
            using (Mat median = new Mat())
            using (Mat bilateral = new Mat())
            using (Mat identityKernel = new Mat(1, 1, MatType.CV_32FC1))
            using (Mat filtered = new Mat())
            using (Mat gaussianKernel = ImgProcCv2.GetGaussianKernel(3, 0, MatType.CV_64F))
            using (Mat sepFiltered = new Mat())
            using (Mat gaborKernel = ImgProcCv2.GetGaborKernel(new Size(3, 3), 1.0, 0.0, 2.0, 0.5))
            {
                src.CopyFrom(new byte[]
                {
                    0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0,
                    0, 0, 255, 0, 0,
                    0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0
                });
                identityKernel.CopyFrom<float>(new float[] { 1.0F });

                ImgProcCv2.Blur(src, blurred, new Size(3, 3));
                ImgProcCv2.BoxFilter(src, boxed, -1, new Size(3, 3), normalize: false);
                ImgProcCv2.SqrBoxFilter(src, squared, MatType.CV_32F, new Size(3, 3), normalize: false);
                ImgProcCv2.MedianBlur(src, median, 3);
                ImgProcCv2.BilateralFilter(src, bilateral, 3, 25.0, 25.0);
                ImgProcCv2.Filter2D(src, filtered, -1, identityKernel);
                ImgProcCv2.SepFilter2D(src, sepFiltered, -1, gaussianKernel, gaussianKernel);

                byte[] blurredPixels = blurred.ToBytes();
                byte[] boxedPixels = boxed.ToBytes();
                byte[] medianPixels = median.ToBytes();
                byte[] filteredPixels = filtered.ToBytes();
                double[] gaussianValues = new double[3];
                gaussianKernel.CopyTo<double>(gaussianValues);

                Assert.Equal(5, blurred.Rows);
                Assert.Equal(5, blurred.Cols);
                Assert.Equal(28, blurredPixels[12]);
                Assert.Equal(255, boxedPixels[12]);
                Assert.Equal(MatType.CV_32FC1, squared.Type);
                Assert.Equal(0, medianPixels[12]);
                Assert.Equal(src.ToBytes(), filteredPixels);
                Assert.Equal(0.25, gaussianValues[0], 6);
                Assert.Equal(0.5, gaussianValues[1], 6);
                Assert.Equal(0.25, gaussianValues[2], 6);
                Assert.Equal(3, gaborKernel.Rows);
                Assert.Equal(3, gaborKernel.Cols);
                Assert.Equal(5, sepFiltered.Rows);
                Assert.Equal(5, sepFiltered.Cols);
                Assert.Equal(5, bilateral.Rows);
                Assert.Equal(5, bilateral.Cols);
            }
        }

        [Fact]
        public void DerivativeAndCannyBatchDetectEdgesWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat src = new Mat(5, 5, MatType.CV_8UC1))
            using (Mat sobelX = new Mat())
            using (Mat sobelY = new Mat())
            using (Mat scharrX = new Mat())
            using (Mat laplacian = new Mat())
            using (Mat edges = new Mat())
            using (Mat derivativeEdges = new Mat())
            using (Mat kx = new Mat())
            using (Mat ky = new Mat())
            {
                src.CopyFrom(new byte[]
                {
                    0, 0, 255, 255, 255,
                    0, 0, 255, 255, 255,
                    0, 0, 255, 255, 255,
                    0, 0, 255, 255, 255,
                    0, 0, 255, 255, 255
                });

                ImgProcCv2.Sobel(src, sobelX, MatType.CV_16S, 1, 0);
                ImgProcCv2.Sobel(src, sobelY, MatType.CV_16S, 0, 1);
                ImgProcCv2.Scharr(src, scharrX, MatType.CV_16S, 1, 0);
                ImgProcCv2.Laplacian(src, laplacian, MatType.CV_16S);
                ImgProcCv2.Canny(src, edges, 40.0, 120.0);
                ImgProcCv2.Canny(sobelX, sobelY, derivativeEdges, 40.0, 120.0);
                ImgProcCv2.GetDerivKernels(kx, ky, 1, 0, 3);

                short[] sobelValues = new short[25];
                short[] scharrValues = new short[25];
                byte[] edgePixels = edges.ToBytes();
                byte[] derivativeEdgePixels = derivativeEdges.ToBytes();
                sobelX.CopyTo<short>(sobelValues);
                scharrX.CopyTo<short>(scharrValues);

                Assert.Contains(sobelValues, value => value > 0);
                Assert.Contains(scharrValues, value => value > 0);
                Assert.Contains(edgePixels, value => value == 255);
                Assert.Contains(derivativeEdgePixels, value => value == 255);
                Assert.Equal(MatType.CV_16SC1, laplacian.Type);
                Assert.Equal(3, kx.Rows);
                Assert.Equal(1, kx.Cols);
                Assert.Equal(3, ky.Rows);
                Assert.Equal(1, ky.Cols);
            }
        }

        [Fact]
        public void PyramidAndWarpBatchPreserveIdentityTransformsWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            byte[] sourcePixels = new byte[]
            {
                1, 2, 3, 4,
                5, 6, 7, 8,
                9, 10, 11, 12,
                13, 14, 15, 16
            };

            using (Mat src = new Mat(4, 4, MatType.CV_8UC1))
            using (Mat down = new Mat())
            using (Mat up = new Mat())
            using (Mat rotation = ImgProcCv2.GetRotationMatrix2D(new Point2f(0.0F, 0.0F), 0.0, 1.0))
            using (Mat warpedAffine = new Mat())
            using (Mat inverseRotation = new Mat())
            using (Mat affine = ImgProcCv2.GetAffineTransform(
                new Point2f[]
                {
                    new Point2f(0.0F, 0.0F),
                    new Point2f(3.0F, 0.0F),
                    new Point2f(0.0F, 3.0F)
                },
                new Point2f[]
                {
                    new Point2f(0.0F, 0.0F),
                    new Point2f(3.0F, 0.0F),
                    new Point2f(0.0F, 3.0F)
                }))
            using (Mat warpedAffineFromPoints = new Mat())
            using (Mat perspective = ImgProcCv2.GetPerspectiveTransform(
                new Point2f[]
                {
                    new Point2f(0.0F, 0.0F),
                    new Point2f(3.0F, 0.0F),
                    new Point2f(3.0F, 3.0F),
                    new Point2f(0.0F, 3.0F)
                },
                new Point2f[]
                {
                    new Point2f(0.0F, 0.0F),
                    new Point2f(3.0F, 0.0F),
                    new Point2f(3.0F, 3.0F),
                    new Point2f(0.0F, 3.0F)
                }))
            using (Mat warpedPerspective = new Mat())
            {
                src.CopyFrom(sourcePixels);

                ImgProcCv2.PyrDown(src, down);
                ImgProcCv2.PyrUp(down, up, new Size(4, 4));
                ImgProcCv2.WarpAffine(src, warpedAffine, rotation, new Size(4, 4), InterpolationFlags.Nearest);
                ImgProcCv2.InvertAffineTransform(rotation, inverseRotation);
                ImgProcCv2.WarpAffine(src, warpedAffineFromPoints, affine, new Size(4, 4), InterpolationFlags.Nearest);
                ImgProcCv2.WarpPerspective(src, warpedPerspective, perspective, new Size(4, 4), InterpolationFlags.Nearest);

                Assert.Equal(2, down.Rows);
                Assert.Equal(2, down.Cols);
                Assert.Equal(4, up.Rows);
                Assert.Equal(4, up.Cols);
                Assert.Equal(2, rotation.Rows);
                Assert.Equal(3, rotation.Cols);
                Assert.Equal(2, inverseRotation.Rows);
                Assert.Equal(3, inverseRotation.Cols);
                Assert.Equal(sourcePixels, warpedAffine.ToBytes());
                Assert.Equal(sourcePixels, warpedAffineFromPoints.ToBytes());
                Assert.Equal(sourcePixels, warpedPerspective.ToBytes());
            }
        }

        [Fact]
        public void RemapAndConvertMapsPreserveIdentityMapWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            byte[] sourcePixels = new byte[]
            {
                10, 20, 30,
                40, 50, 60
            };

            using (Mat src = new Mat(2, 3, MatType.CV_8UC1))
            using (Mat mapX = new Mat(2, 3, MatType.CV_32FC1))
            using (Mat mapY = new Mat(2, 3, MatType.CV_32FC1))
            using (Mat remapped = new Mat())
            using (Mat convertedMap1 = new Mat())
            using (Mat convertedMap2 = new Mat())
            using (Mat remappedFromConvertedMaps = new Mat())
            {
                src.CopyFrom(sourcePixels);
                mapX.CopyFrom<float>(new float[] { 0, 1, 2, 0, 1, 2 });
                mapY.CopyFrom<float>(new float[] { 0, 0, 0, 1, 1, 1 });

                ImgProcCv2.Remap(src, remapped, mapX, mapY, InterpolationFlags.Nearest);
                ImgProcCv2.ConvertMaps(mapX, mapY, convertedMap1, convertedMap2, MatType.CV_16SC2, nninterpolation: true);
                ImgProcCv2.Remap(src, remappedFromConvertedMaps, convertedMap1, convertedMap2, InterpolationFlags.Nearest);

                Assert.Equal(sourcePixels, remapped.ToBytes());
                Assert.Equal(sourcePixels, remappedFromConvertedMaps.ToBytes());
                Assert.Equal(MatType.CV_16SC2, convertedMap1.Type);
            }
        }

        [Fact]
        public void GetStructuringElementReturnsExpectedRectKernelWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat kernel = ImgProcCv2.GetStructuringElement(MorphShapes.Rect, new Size(3, 3)))
            {
                Assert.Equal(3, kernel.Rows);
                Assert.Equal(3, kernel.Cols);
                Assert.Equal(MatType.CV_8UC1, kernel.Type);

                byte[] pixels = new byte[kernel.ByteLength];
                kernel.CopyTo(pixels);
                Assert.Equal(new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, pixels);
            }
        }

        [Fact]
        public void MorphologyEnumsValidateManagedArguments()
        {
            Assert.Equal(0, (int)MorphShapes.Rect);
            Assert.Equal(1, (int)MorphShapes.Cross);
            Assert.Equal(2, (int)MorphShapes.Ellipse);
            Assert.Equal(3, (int)MorphShapes.Diamond);

            Assert.Equal(0, (int)MorphTypes.Erode);
            Assert.Equal(1, (int)MorphTypes.Dilate);
            Assert.Equal(7, (int)MorphTypes.HitMiss);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                ImgProcCv2.GetStructuringElement((MorphShapes)99, new Size(3, 3)));

            using (Mat src = new Mat())
            using (Mat dst = new Mat())
            using (Mat kernel = new Mat())
            {
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    ImgProcCv2.MorphologyEx(src, dst, (MorphTypes)99, kernel));
            }
        }

        [Fact]
        public void ErodeAndDilateUseStructuringElementsAndDefaultMorphologyBorderValueWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat kernel = ImgProcCv2.GetStructuringElement(MorphShapes.Rect, new Size(3, 3)))
            using (Mat eroded = new Mat())
            using (Mat dilated = new Mat())
            using (Mat src = new Mat(5, 5, MatType.CV_8UC1))
            {
                src.CopyFrom(new byte[]
                {
                    255, 255, 255, 255, 255,
                    255, 255, 255, 255, 255,
                    255, 255, 255, 255, 255,
                    255, 255, 255, 255, 255,
                    255, 255, 255, 255, 255
                });

                ImgProcCv2.Erode(src, eroded, kernel);

                Assert.Equal(5, eroded.Rows);
                Assert.Equal(5, eroded.Cols);
                Assert.Equal(MatType.CV_8UC1, eroded.Type);

                byte[] erodedPixels = new byte[eroded.ByteLength];
                eroded.CopyTo(erodedPixels);
                Assert.Equal(
                    new byte[]
                    {
                        255, 255, 255, 255, 255,
                        255, 255, 255, 255, 255,
                        255, 255, 255, 255, 255,
                        255, 255, 255, 255, 255,
                        255, 255, 255, 255, 255
                    },
                    erodedPixels);

                src.CopyFrom(new byte[]
                {
                    0, 0, 0, 0, 0,
                    0, 255, 255, 255, 0,
                    0, 255, 255, 255, 0,
                    0, 255, 255, 255, 0,
                    0, 0, 0, 0, 0
                });

                ImgProcCv2.Dilate(src, dilated, kernel);

                Assert.Equal(5, dilated.Rows);
                Assert.Equal(5, dilated.Cols);
                Assert.Equal(MatType.CV_8UC1, dilated.Type);

                byte[] dilatedPixels = new byte[dilated.ByteLength];
                dilated.CopyTo(dilatedPixels);
                Assert.Equal(
                    new byte[]
                    {
                        255, 255, 255, 255, 255,
                        255, 255, 255, 255, 255,
                        255, 255, 255, 255, 255,
                        255, 255, 255, 255, 255,
                        255, 255, 255, 255, 255
                    },
                    dilatedPixels);
            }
        }

        [Fact]
        public void MorphologyExOpenKeepsLargeRegionAndExplicitBorderValueAffectsErosionWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat kernel = ImgProcCv2.GetStructuringElement(MorphShapes.Rect, new Size(3, 3)))
            using (Mat opened = new Mat())
            using (Mat erodedWithExplicitBorder = new Mat())
            using (Mat src = new Mat(5, 5, MatType.CV_8UC1))
            using (Mat borderSrc = new Mat(3, 3, MatType.CV_8UC1))
            {
                src.CopyFrom(new byte[]
                {
                    0, 0, 0, 0, 0,
                    0, 255, 255, 255, 0,
                    0, 255, 255, 255, 0,
                    0, 255, 255, 255, 0,
                    0, 0, 0, 0, 0
                });

                ImgProcCv2.MorphologyEx(src, opened, MorphTypes.Open, kernel);

                byte[] openedPixels = new byte[opened.ByteLength];
                opened.CopyTo(openedPixels);
                Assert.Equal(
                    new byte[]
                    {
                        0, 0, 0, 0, 0,
                        0, 255, 255, 255, 0,
                        0, 255, 255, 255, 0,
                        0, 255, 255, 255, 0,
                        0, 0, 0, 0, 0
                    },
                    openedPixels);

                borderSrc.CopyFrom(new byte[]
                {
                    255, 255, 255,
                    255, 255, 255,
                    255, 255, 255
                });

                ImgProcCv2.Erode(borderSrc, erodedWithExplicitBorder, kernel, borderValue: new Scalar(0));

                byte[] erodedPixels = new byte[erodedWithExplicitBorder.ByteLength];
                erodedWithExplicitBorder.CopyTo(erodedPixels);
                Assert.Equal(
                    new byte[]
                    {
                        0, 0, 0,
                        0, 255, 0,
                        0, 0, 0
                    },
                    erodedPixels);
            }
        }

        [Fact]
        public void LineAndRectangleDrawExpectedPixelsWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat image = new Mat(5, 5, MatType.CV_8UC1))
            {
                image.CopyFrom(new byte[]
                {
                    0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0
                });

                ImgProcCv2.Line(image, new Point(0, 0), new Point(4, 4), new Scalar(255));

                byte[] pixels = new byte[image.ByteLength];
                image.CopyTo(pixels);
                Assert.Equal(255, pixels[0]);
                Assert.Equal(255, pixels[6]);
                Assert.Equal(255, pixels[12]);
                Assert.Equal(255, pixels[18]);
                Assert.Equal(255, pixels[24]);

                ImgProcCv2.Rectangle(image, new Rect(1, 1, 3, 3), new Scalar(128), -1);

                image.CopyTo(pixels);
                Assert.Equal(
                    new byte[]
                    {
                        255, 0, 0, 0, 0,
                        0, 128, 128, 128, 0,
                        0, 128, 128, 128, 0,
                        0, 128, 128, 128, 0,
                        0, 0, 0, 0, 255
                    },
                    pixels);

                ImgProcCv2.Rectangle(image, new Point(0, 4), new Point(4, 4), new Scalar(64));

                image.CopyTo(pixels);
                Assert.Equal(64, pixels[20]);
                Assert.Equal(64, pixels[21]);
                Assert.Equal(64, pixels[22]);
                Assert.Equal(64, pixels[23]);
                Assert.Equal(64, pixels[24]);
            }
        }

        [Fact]
        public void ArrowedLineDrawsExpectedPixelsWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat image = new Mat(40, 80, MatType.CV_8UC1))
            {
                image.CopyFrom(new byte[image.ByteLength]);

                ImgProcCv2.ArrowedLine(image, new Point(4, 20), new Point(72, 20), new Scalar(180), tipLength: 0.2);

                byte[] pixels = new byte[image.ByteLength];
                image.CopyTo(pixels);

                Assert.Equal(180, pixels[20 * 80 + 4]);
                Assert.Equal(180, pixels[20 * 80 + 40]);
                Assert.Equal(180, pixels[20 * 80 + 72]);
            }
        }

        [Fact]
        public void ClipLineUpdatesEndpointsAndReturnsIntersectionWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Point pt1 = new Point(-5, 2);
            Point pt2 = new Point(15, 2);
            bool intersects = ImgProcCv2.ClipLine(new Rect(0, 0, 10, 10), ref pt1, ref pt2);

            Assert.True(intersects);
            Assert.Equal(new Point(0, 2), pt1);
            Assert.Equal(new Point(9, 2), pt2);

            pt1 = new Point(-5, -5);
            pt2 = new Point(-1, -1);
            intersects = ImgProcCv2.ClipLine(new Rect(0, 0, 10, 10), ref pt1, ref pt2);

            Assert.False(intersects);
        }

        [Fact]
        public void PolylinesDrawsClosedPolylineWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat image = new Mat(10, 10, MatType.CV_8UC1))
            {
                image.CopyFrom(new byte[image.ByteLength]);

                ImgProcCv2.Polylines(
                    image,
                    new Point[]
                    {
                        new Point(1, 1),
                        new Point(8, 1),
                        new Point(8, 8)
                    },
                    true,
                    new Scalar(210));

                byte[] pixels = new byte[image.ByteLength];
                image.CopyTo(pixels);

                Assert.Equal(210, pixels[1 * 10 + 1]);
                Assert.Equal(210, pixels[1 * 10 + 8]);
                Assert.Equal(210, pixels[8 * 10 + 8]);
            }
        }

        [Fact]
        public void FillPolyFillsPolygonWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat image = new Mat(10, 10, MatType.CV_8UC1))
            {
                image.CopyFrom(new byte[image.ByteLength]);

                ImgProcCv2.FillPoly(
                    image,
                    new Point[]
                    {
                        new Point(2, 2),
                        new Point(7, 2),
                        new Point(7, 7),
                        new Point(2, 7)
                    },
                    new Scalar(160));

                byte[] pixels = new byte[image.ByteLength];
                image.CopyTo(pixels);

                Assert.Equal(160, pixels[2 * 10 + 2]);
                Assert.Equal(160, pixels[4 * 10 + 4]);
                Assert.Equal(160, pixels[7 * 10 + 7]);
                Assert.Equal(0, pixels[1 * 10 + 1]);
            }
        }

        [Fact]
        public void Ellipse2PolyReturnsVerticesUsableByPolylinesWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Point[] points = ImgProcCv2.Ellipse2Poly(new Point(10, 10), new Size(5, 3), 0, 0, 90, 30);

            Assert.NotEmpty(points);
            Assert.Equal(new Point(15, 10), points[0]);

            using (Mat image = new Mat(20, 20, MatType.CV_8UC1))
            {
                image.CopyFrom(new byte[image.ByteLength]);
                ImgProcCv2.Polylines(image, points, false, new Scalar(200));

                byte[] pixels = new byte[image.ByteLength];
                image.CopyTo(pixels);
                Assert.Equal(200, pixels[10 * 20 + 15]);
            }
        }

        [Fact]
        public void ContourAreaReturnsExpectedAreaWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Point[] contour = new Point[]
            {
                new Point(0, 0),
                new Point(4, 0),
                new Point(4, 3),
                new Point(0, 3)
            };

            double area = ImgProcCv2.ContourArea(contour);
            double orientedArea = ImgProcCv2.ContourArea(contour, true);

            Assert.Equal(12.0, area);
            Assert.Equal(12.0, orientedArea);
        }

        [Fact]
        public void ArcLengthReturnsExpectedLengthWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Point[] curve = new Point[]
            {
                new Point(0, 0),
                new Point(4, 0),
                new Point(4, 3),
                new Point(0, 3)
            };

            double openLength = ImgProcCv2.ArcLength(curve, false);
            double closedLength = ImgProcCv2.ArcLength(curve, true);

            Assert.Equal(11.0, openLength);
            Assert.Equal(14.0, closedLength);
        }

        [Fact]
        public void ApproxPolyDPReturnsExpectedVerticesWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Point[] curve = new Point[]
            {
                new Point(0, 0),
                new Point(2, 0),
                new Point(4, 0),
                new Point(4, 3),
                new Point(0, 3)
            };

            Point[] approx = ImgProcCv2.ApproxPolyDP(curve, 0.5, true);

            Assert.Equal(
                new Point[]
                {
                    new Point(0, 0),
                    new Point(4, 0),
                    new Point(4, 3),
                    new Point(0, 3)
                },
                approx);
        }

        [Fact]
        public void BoundingRectAndIsContourConvexReturnExpectedValuesWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Point[] convexContour = new Point[]
            {
                new Point(1, 2),
                new Point(6, 2),
                new Point(6, 5),
                new Point(1, 5)
            };
            Point[] concaveContour = new Point[]
            {
                new Point(0, 0),
                new Point(4, 0),
                new Point(2, 1),
                new Point(4, 3),
                new Point(0, 3)
            };

            Rect rect = ImgProcCv2.BoundingRect(convexContour);
            bool convex = ImgProcCv2.IsContourConvex(convexContour);
            bool concave = ImgProcCv2.IsContourConvex(concaveContour);

            Assert.Equal(new Rect(1, 2, 6, 4), rect);
            Assert.True(convex);
            Assert.False(concave);
        }

        [Fact]
        public void ConvexHullAndMinEnclosingCircleReturnExpectedValuesWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Point[] points = new Point[]
            {
                new Point(0, 0),
                new Point(2, 1),
                new Point(4, 0),
                new Point(4, 3),
                new Point(0, 3)
            };

            Point[] hull = ImgProcCv2.ConvexHull(points);
            ImgProcCv2.MinEnclosingCircle(
                new Point[]
                {
                    new Point(0, 0),
                    new Point(4, 0)
                },
                out Point2f center,
                out float radius);

            Assert.Contains(new Point(0, 0), hull);
            Assert.Contains(new Point(4, 0), hull);
            Assert.Contains(new Point(4, 3), hull);
            Assert.Contains(new Point(0, 3), hull);
            Assert.Equal(4, hull.Length);
            Assert.Equal(2.0F, center.X, 3);
            Assert.Equal(0.0F, center.Y, 3);
            Assert.Equal(2.0F, radius, 3);
        }

        [Fact]
        public void ConvexHullIndicesAndConvexityDefectsReturnExpectedValuesWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Point[] contour = new Point[]
            {
                new Point(0, 0),
                new Point(4, 0),
                new Point(4, 4),
                new Point(2, 2),
                new Point(0, 4)
            };

            int[] hullIndices = ImgProcCv2.ConvexHullIndices(contour);
            Vec4i[] defects = ImgProcCv2.ConvexityDefects(contour, hullIndices);

            Assert.Equal(4, hullIndices.Length);
            Assert.Contains(0, hullIndices);
            Assert.Contains(1, hullIndices);
            Assert.Contains(2, hullIndices);
            Assert.Contains(4, hullIndices);
            Assert.NotEmpty(defects);
            Assert.Contains(defects, defect => defect.V2 == 3 && defect.V3 > 0);
        }

        [Fact]
        public void PointPolygonTestAndMatchShapesReturnExpectedValuesWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Point[] rectangle = new Point[]
            {
                new Point(0, 0),
                new Point(4, 0),
                new Point(4, 3),
                new Point(0, 3)
            };
            Point[] triangle = new Point[]
            {
                new Point(0, 0),
                new Point(4, 0),
                new Point(2, 3)
            };

            double inside = ImgProcCv2.PointPolygonTest(rectangle, new Point2f(2.0F, 1.0F), false);
            double outsideDistance = ImgProcCv2.PointPolygonTest(rectangle, new Point2f(6.0F, 1.0F), true);
            double sameDistance = ImgProcCv2.MatchShapes(rectangle, rectangle, ShapeMatchModes.I1);
            double otherDistance = ImgProcCv2.MatchShapes(rectangle, triangle, ShapeMatchModes.I1);

            Assert.True(inside > 0);
            Assert.Equal(-2.0, outsideDistance, 3);
            Assert.Equal(0.0, sameDistance, 6);
            Assert.True(otherDistance > sameDistance);
        }

        [Fact]
        public void MatchShapesModeValidatesManagedArgument()
        {
            Point[] contour = new Point[]
            {
                new Point(0, 0),
                new Point(4, 0),
                new Point(4, 3),
                new Point(0, 3)
            };

            Assert.Throws<ArgumentOutOfRangeException>(() => ImgProcCv2.MatchShapes(contour, contour, (ShapeMatchModes)99));
        }

        [Fact]
        public void MinAreaRectAndBoxPointsReturnExpectedValuesWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Point[] rectangle = new Point[]
            {
                new Point(0, 0),
                new Point(4, 0),
                new Point(4, 3),
                new Point(0, 3)
            };

            RotatedRect box = ImgProcCv2.MinAreaRect(rectangle);
            Point2f[] vertices = ImgProcCv2.BoxPoints(box);

            Assert.Equal(2.0F, box.Center.X, 3);
            Assert.Equal(1.5F, box.Center.Y, 3);
            Assert.True(box.Size.Width > 0);
            Assert.True(box.Size.Height > 0);
            Assert.Equal(4, vertices.Length);
            Assert.Contains(vertices, p => Math.Abs(p.X - 0.0F) < 0.001F && Math.Abs(p.Y - 0.0F) < 0.001F);
            Assert.Contains(vertices, p => Math.Abs(p.X - 4.0F) < 0.001F && Math.Abs(p.Y - 3.0F) < 0.001F);
        }

        [Fact]
        public void FitEllipseVariantsReturnPositiveSizeWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Point[] points = new Point[]
            {
                new Point(0, 2),
                new Point(1, 0),
                new Point(3, 0),
                new Point(4, 2),
                new Point(3, 4),
                new Point(1, 4)
            };

            RotatedRect ellipse = ImgProcCv2.FitEllipse(points);
            RotatedRect ellipseAms = ImgProcCv2.FitEllipseAMS(points);
            RotatedRect ellipseDirect = ImgProcCv2.FitEllipseDirect(points);

            AssertEllipseLooksReasonable(ellipse);
            AssertEllipseLooksReasonable(ellipseAms);
            AssertEllipseLooksReasonable(ellipseDirect);
        }

        [Fact]
        public void FitEllipseRejectsTooFewPointsBeforeNativeCall()
        {
            Point[] points = new Point[]
            {
                new Point(0, 0),
                new Point(1, 0),
                new Point(1, 1),
                new Point(0, 1)
            };

            Assert.Throws<ArgumentException>(() => ImgProcCv2.FitEllipse(points));
            Assert.Throws<ArgumentException>(() => ImgProcCv2.FitEllipseAMS(points));
            Assert.Throws<ArgumentException>(() => ImgProcCv2.FitEllipseDirect(points));
        }

        [Fact]
        public void RotatedRectangleIntersectionReturnsRegionWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            RotatedRect rect1 = new RotatedRect(new Point2f(0.0F, 0.0F), new Size2f(4.0F, 4.0F), 0.0F);
            RotatedRect rect2 = new RotatedRect(new Point2f(1.0F, 0.0F), new Size2f(4.0F, 4.0F), 0.0F);
            RotatedRect rect3 = new RotatedRect(new Point2f(10.0F, 10.0F), new Size2f(2.0F, 2.0F), 0.0F);

            RectanglesIntersectTypes intersection = ImgProcCv2.RotatedRectangleIntersection(rect1, rect2, out Point2f[] region);
            RectanglesIntersectTypes none = ImgProcCv2.RotatedRectangleIntersection(rect1, rect3, out Point2f[] emptyRegion);

            Assert.Equal(RectanglesIntersectTypes.IntersectPartial, intersection);
            Assert.True(region.Length > 0);
            Assert.Equal(RectanglesIntersectTypes.IntersectNone, none);
            Assert.Empty(emptyRegion);
        }

        [Fact]
        public void ApproxPolyNAndConvexPolygonOperationsReturnExpectedValuesWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Point[] polygon = new Point[]
            {
                new Point(0, 0),
                new Point(2, 0),
                new Point(4, 0),
                new Point(4, 4),
                new Point(0, 4)
            };
            Point[] polygon1 = new Point[]
            {
                new Point(0, 0),
                new Point(4, 0),
                new Point(4, 4),
                new Point(0, 4)
            };
            Point[] polygon2 = new Point[]
            {
                new Point(2, 0),
                new Point(6, 0),
                new Point(6, 4),
                new Point(2, 4)
            };

            Point2f[] approx = ImgProcCv2.ApproxPolyN(polygon, 4);
            double enclosingArea = ImgProcCv2.MinEnclosingConvexPolygon(polygon1, 4, out Point2f[] enclosingPolygon);
            float intersectArea = ImgProcCv2.IntersectConvexConvex(polygon1, polygon2, out Point2f[] intersectingRegion);

            Assert.Equal(4, approx.Length);
            Assert.Equal(4, enclosingPolygon.Length);
            Assert.True(enclosingArea > 0.0);
            Assert.Equal(8.0F, intersectArea, 3);
            Assert.True(intersectingRegion.Length >= 4);
        }

        [Fact]
        public void GetClosestEllipsePointsAndMinEnclosingTriangleReturnExpectedValuesWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Point[] points = new Point[]
            {
                new Point(0, 2),
                new Point(1, 0),
                new Point(3, 0),
                new Point(4, 2),
                new Point(3, 4),
                new Point(1, 4)
            };

            RotatedRect ellipse = ImgProcCv2.FitEllipse(points);
            Point2f[] closestPoints = ImgProcCv2.GetClosestEllipsePoints(ellipse, points);
            double triangleArea = ImgProcCv2.MinEnclosingTriangle(points, out Point2f[] triangle);

            Assert.Equal(points.Length, closestPoints.Length);
            Assert.Equal(3, triangle.Length);
            Assert.True(triangleArea > 0.0);
        }

        [Fact]
        public void FitLineReturnsExpectedDirectionWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Point[] points = new Point[]
            {
                new Point(0, 1),
                new Point(2, 5),
                new Point(4, 9)
            };

            Vec4f line = ImgProcCv2.FitLine(points, DistanceTypes.L2, 0.0, 0.01, 0.01);

            Assert.True(line.V0 > 0.0F);
            Assert.True(line.V1 > 0.0F);
            Assert.Equal(2.0F, line.V1 / line.V0, 2);
            Assert.True(line.V3 > line.V2);
        }

        [Fact]
        public void SpanPointFastPathsMatchArrayResultsWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Point[] contour = new Point[]
            {
                new Point(0, 0),
                new Point(4, 0),
                new Point(4, 3),
                new Point(0, 3)
            };
            ReadOnlySpan<Point> contourSpan = contour.AsSpan();

            Assert.Equal(ImgProcCv2.ContourArea(contour), ImgProcCv2.ContourArea(contourSpan));
            Assert.Equal(ImgProcCv2.ArcLength(contour, true), ImgProcCv2.ArcLength(contourSpan, true));
            Assert.Equal(ImgProcCv2.BoundingRect(contour), ImgProcCv2.BoundingRect(contourSpan));
            Assert.Equal(ImgProcCv2.IsContourConvex(contour), ImgProcCv2.IsContourConvex(contourSpan));
            Assert.Equal(ImgProcCv2.PointPolygonTest(contour, new Point2f(2.0F, 1.0F), true), ImgProcCv2.PointPolygonTest(contourSpan, new Point2f(2.0F, 1.0F), true));
            Assert.Equal(ImgProcCv2.MatchShapes(contour, contour, ShapeMatchModes.I1), ImgProcCv2.MatchShapes(contourSpan, contourSpan, ShapeMatchModes.I1));

            ImgProcCv2.MinEnclosingCircle(contour, out Point2f arrayCenter, out float arrayRadius);
            ImgProcCv2.MinEnclosingCircle(contourSpan, out Point2f spanCenter, out float spanRadius);

            Assert.Equal(arrayCenter, spanCenter);
            Assert.Equal(arrayRadius, spanRadius);
        }

        [Fact]
        public void SpanPointOutputFastPathsMatchArrayResultsWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Point[] points = new Point[]
            {
                new Point(0, 0),
                new Point(2, 1),
                new Point(4, 0),
                new Point(4, 3),
                new Point(0, 3)
            };
            ReadOnlySpan<Point> pointsSpan = points.AsSpan();

            Assert.Equal(ImgProcCv2.ApproxPolyDP(points, 0.5, true), ImgProcCv2.ApproxPolyDP(pointsSpan, 0.5, true));
            Assert.Equal(ImgProcCv2.ConvexHull(points), ImgProcCv2.ConvexHull(pointsSpan));
            Assert.Equal(ImgProcCv2.ConvexHullIndices(points), ImgProcCv2.ConvexHullIndices(pointsSpan));
            Assert.Equal(ImgProcCv2.MinAreaRect(points), ImgProcCv2.MinAreaRect(pointsSpan));
            Assert.Equal(ImgProcCv2.FitLine(points, DistanceTypes.L2, 0.0, 0.01, 0.01), ImgProcCv2.FitLine(pointsSpan, DistanceTypes.L2, 0.0, 0.01, 0.01));
        }

        [Fact]
        public void SpanPointAdvancedGeometryFastPathsMatchArrayResultsWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Point[] polygon = new Point[]
            {
                new Point(0, 0),
                new Point(2, 0),
                new Point(4, 0),
                new Point(4, 4),
                new Point(0, 4)
            };
            Point[] otherPolygon = new Point[]
            {
                new Point(2, 0),
                new Point(6, 0),
                new Point(6, 4),
                new Point(2, 4)
            };
            Point[] ellipsePoints = new Point[]
            {
                new Point(0, 2),
                new Point(1, 0),
                new Point(3, 0),
                new Point(4, 2),
                new Point(3, 4),
                new Point(1, 4)
            };
            ReadOnlySpan<Point> polygonSpan = polygon.AsSpan();
            ReadOnlySpan<Point> otherPolygonSpan = otherPolygon.AsSpan();
            ReadOnlySpan<Point> ellipseSpan = ellipsePoints.AsSpan();

            Assert.Equal(ImgProcCv2.ApproxPolyN(polygon, 4), ImgProcCv2.ApproxPolyN(polygonSpan, 4));
            AssertEllipseLooksReasonable(ImgProcCv2.FitEllipse(ellipsePoints));
            AssertEllipseLooksReasonable(ImgProcCv2.FitEllipse(ellipseSpan));
            AssertEllipseLooksReasonable(ImgProcCv2.FitEllipseAMS(ellipsePoints));
            AssertEllipseLooksReasonable(ImgProcCv2.FitEllipseAMS(ellipseSpan));
            AssertEllipseLooksReasonable(ImgProcCv2.FitEllipseDirect(ellipsePoints));
            AssertEllipseLooksReasonable(ImgProcCv2.FitEllipseDirect(ellipseSpan));

            RotatedRect ellipse = ImgProcCv2.FitEllipse(ellipseSpan);
            Assert.Equal(
                ImgProcCv2.GetClosestEllipsePoints(ellipse, ellipsePoints),
                ImgProcCv2.GetClosestEllipsePoints(ellipse, ellipseSpan));

            double arrayTriangleArea = ImgProcCv2.MinEnclosingTriangle(polygon, out Point2f[] arrayTriangle);
            double spanTriangleArea = ImgProcCv2.MinEnclosingTriangle(polygonSpan, out Point2f[] spanTriangle);
            Assert.Equal(arrayTriangleArea, spanTriangleArea);
            Assert.Equal(arrayTriangle, spanTriangle);

            double arrayPolygonArea = ImgProcCv2.MinEnclosingConvexPolygon(polygon, 4, out Point2f[] arrayEnclosingPolygon);
            double spanPolygonArea = ImgProcCv2.MinEnclosingConvexPolygon(polygonSpan, 4, out Point2f[] spanEnclosingPolygon);
            Assert.Equal(arrayPolygonArea, spanPolygonArea);
            Assert.Equal(arrayEnclosingPolygon, spanEnclosingPolygon);

            float arrayIntersectionArea = ImgProcCv2.IntersectConvexConvex(polygon, otherPolygon, out Point2f[] arrayIntersection);
            float spanIntersectionArea = ImgProcCv2.IntersectConvexConvex(polygonSpan, otherPolygonSpan, out Point2f[] spanIntersection);
            Assert.Equal(arrayIntersectionArea, spanIntersectionArea);
            Assert.Equal(arrayIntersection, spanIntersection);
        }

        [Fact]
        public void SpanPointFastPathsRejectEmptyInputs()
        {
            Assert.Throws<ArgumentException>(CallContourAreaWithEmptySpan);
            Assert.Throws<ArgumentException>(CallArcLengthWithEmptySpan);
            Assert.Throws<ArgumentException>(CallApproxPolyDPWithEmptySpan);
            Assert.Throws<ArgumentException>(CallBoundingRectWithEmptySpan);
            Assert.Throws<ArgumentException>(CallIsContourConvexWithEmptySpan);
            Assert.Throws<ArgumentException>(CallConvexHullWithEmptySpan);
            Assert.Throws<ArgumentException>(CallConvexHullIndicesWithEmptySpan);
            Assert.Throws<ArgumentException>(CallMinEnclosingCircleWithEmptySpan);
            Assert.Throws<ArgumentException>(CallPointPolygonTestWithEmptySpan);
            Assert.Throws<ArgumentException>(CallMatchShapesWithEmptySpan);
            Assert.Throws<ArgumentException>(CallMinAreaRectWithEmptySpan);
            Assert.Throws<ArgumentException>(CallFitLineWithEmptySpan);
            Assert.Throws<ArgumentException>(CallApproxPolyNWithEmptySpan);
            Assert.Throws<ArgumentException>(CallFitEllipseWithEmptySpan);
            Assert.Throws<ArgumentException>(CallFitEllipseAmsWithEmptySpan);
            Assert.Throws<ArgumentException>(CallFitEllipseDirectWithEmptySpan);
            Assert.Throws<ArgumentException>(CallGetClosestEllipsePointsWithEmptySpan);
            Assert.Throws<ArgumentException>(CallMinEnclosingTriangleWithEmptySpan);
            Assert.Throws<ArgumentException>(CallMinEnclosingConvexPolygonWithEmptySpan);
            Assert.Throws<ArgumentException>(CallIntersectConvexConvexWithFirstEmptySpan);
            Assert.Throws<ArgumentException>(CallIntersectConvexConvexWithSecondEmptySpan);
        }

        [Fact]
        public void SpanPointAdvancedGeometryFastPathsValidateScalarAndMinimumPointArguments()
        {
            Point[] fourPoints = new Point[]
            {
                new Point(0, 0),
                new Point(1, 0),
                new Point(1, 1),
                new Point(0, 1)
            };

            Assert.Throws<ArgumentOutOfRangeException>(() => CallApproxPolyNWithInvalidSideCount(fourPoints));
            Assert.Throws<ArgumentOutOfRangeException>(() => CallMinEnclosingConvexPolygonWithInvalidVertexCount(fourPoints));
            Assert.Throws<ArgumentException>(() => CallFitEllipseWithTooFewPoints(fourPoints));
            Assert.Throws<ArgumentException>(() => CallFitEllipseAmsWithTooFewPoints(fourPoints));
            Assert.Throws<ArgumentException>(() => CallFitEllipseDirectWithTooFewPoints(fourPoints));
        }

        [Fact]
        public void CircleAndEllipseDrawExpectedPixelsWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat image = new Mat(5, 5, MatType.CV_8UC1))
            {
                image.CopyFrom(new byte[]
                {
                    0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0
                });

                ImgProcCv2.Circle(image, new Point(2, 2), 2, new Scalar(255), -1);

                byte[] pixels = new byte[image.ByteLength];
                image.CopyTo(pixels);
                Assert.Equal(255, pixels[2]);
                Assert.Equal(255, pixels[6]);
                Assert.Equal(255, pixels[7]);
                Assert.Equal(255, pixels[8]);
                Assert.Equal(255, pixels[10]);
                Assert.Equal(255, pixels[11]);
                Assert.Equal(255, pixels[12]);
                Assert.Equal(255, pixels[13]);
                Assert.Equal(255, pixels[14]);
                Assert.Equal(255, pixels[16]);
                Assert.Equal(255, pixels[17]);
                Assert.Equal(255, pixels[18]);
                Assert.Equal(255, pixels[22]);

                image.CopyFrom(new byte[]
                {
                    0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0
                });

                ImgProcCv2.Ellipse(image, new Point(2, 2), new Size(2, 1), 0, 0, 360, new Scalar(128), -1);

                image.CopyTo(pixels);
                Assert.Equal(128, pixels[10]);
                Assert.Equal(128, pixels[11]);
                Assert.Equal(128, pixels[12]);
                Assert.Equal(128, pixels[13]);
                Assert.Equal(128, pixels[14]);
            }
        }

        [Fact]
        public void PutTextDrawsTextPixelsWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat image = new Mat(40, 80, MatType.CV_8UC1))
            {
                byte[] zeros = new byte[image.ByteLength];
                image.CopyFrom(zeros);

                ImgProcCv2.PutText(
                    image,
                    "A",
                    new Point(4, 28),
                    HersheyFonts.HersheySimplex,
                    0.8,
                    new Scalar(255));

                byte[] pixels = new byte[image.ByteLength];
                image.CopyTo(pixels);

                bool hasTextPixels = false;
                for (int i = 0; i < pixels.Length; i++)
                {
                    if (pixels[i] != 0)
                    {
                        hasTextPixels = true;
                        break;
                    }
                }

                Assert.True(hasTextPixels);
            }
        }

        [Fact]
        public void GetTextSizeReturnsPositiveSizeAndBaselineWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            int baseLine;
            Size textSize = ImgProcCv2.GetTextSize("OpenCV", HersheyFonts.HersheySimplex, 0.5, 1, out baseLine);

            Assert.True(textSize.Width > 0);
            Assert.True(textSize.Height > 0);
            Assert.True(baseLine >= 0);

            using (Mat image = new Mat(textSize.Height + baseLine + 8, textSize.Width + 8, MatType.CV_8UC1))
            {
                image.CopyFrom(new byte[image.ByteLength]);
                ImgProcCv2.PutText(
                    image,
                    "OpenCV",
                    new Point(4, textSize.Height + 4),
                    HersheyFonts.HersheySimplex,
                    0.5,
                    new Scalar(255));

                byte[] pixels = new byte[image.ByteLength];
                image.CopyTo(pixels);

                bool hasTextPixels = false;
                for (int i = 0; i < pixels.Length; i++)
                {
                    if (pixels[i] != 0)
                    {
                        hasTextPixels = true;
                        break;
                    }
                }

                Assert.True(hasTextPixels);
            }
        }

        [Fact]
        public void TextHelpersValidateHersheyFontFaceManagedArgument()
        {
            Assert.Equal(0, (int)HersheyFonts.HersheySimplex);
            Assert.Equal(1, (int)HersheyFonts.HersheyPlain);
            Assert.Equal(2, (int)HersheyFonts.HersheyDuplex);
            Assert.Equal(3, (int)HersheyFonts.HersheyComplex);
            Assert.Equal(4, (int)HersheyFonts.HersheyTriplex);
            Assert.Equal(5, (int)HersheyFonts.HersheyComplexSmall);
            Assert.Equal(6, (int)HersheyFonts.HersheyScriptSimplex);
            Assert.Equal(7, (int)HersheyFonts.HersheyScriptComplex);
            Assert.Equal(16, (int)HersheyFonts.Italic);

            using (Mat image = new Mat())
            {
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    ImgProcCv2.PutText(image, "A", new Point(0, 0), (HersheyFonts)8, 1.0, new Scalar(255)));

                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    ImgProcCv2.GetTextSize("A", (HersheyFonts)8, 1.0, 1, out _));

                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    ImgProcCv2.PutText(image, "A", new Point(0, 0), (HersheyFonts)24, 1.0, new Scalar(255)));

                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    ImgProcCv2.GetTextSize("A", (HersheyFonts)24, 1.0, 1, out _));
            }
        }

        private static void AssertEllipseLooksReasonable(RotatedRect ellipse)
        {
            Assert.InRange(ellipse.Center.X, 1.0F, 3.0F);
            Assert.InRange(ellipse.Center.Y, 1.0F, 3.0F);
            Assert.True(ellipse.Size.Width > 0.0F);
            Assert.True(ellipse.Size.Height > 0.0F);
        }

        private static void CallContourAreaWithEmptySpan()
        {
            ImgProcCv2.ContourArea(ReadOnlySpan<Point>.Empty);
        }

        private static void CallArcLengthWithEmptySpan()
        {
            ImgProcCv2.ArcLength(ReadOnlySpan<Point>.Empty, true);
        }

        private static void CallApproxPolyDPWithEmptySpan()
        {
            ImgProcCv2.ApproxPolyDP(ReadOnlySpan<Point>.Empty, 0.5, true);
        }

        private static void CallBoundingRectWithEmptySpan()
        {
            ImgProcCv2.BoundingRect(ReadOnlySpan<Point>.Empty);
        }

        private static void CallIsContourConvexWithEmptySpan()
        {
            ImgProcCv2.IsContourConvex(ReadOnlySpan<Point>.Empty);
        }

        private static void CallConvexHullWithEmptySpan()
        {
            ImgProcCv2.ConvexHull(ReadOnlySpan<Point>.Empty);
        }

        private static void CallConvexHullIndicesWithEmptySpan()
        {
            ImgProcCv2.ConvexHullIndices(ReadOnlySpan<Point>.Empty);
        }

        private static void CallMinEnclosingCircleWithEmptySpan()
        {
            ImgProcCv2.MinEnclosingCircle(ReadOnlySpan<Point>.Empty, out _, out _);
        }

        private static void CallPointPolygonTestWithEmptySpan()
        {
            ImgProcCv2.PointPolygonTest(ReadOnlySpan<Point>.Empty, new Point2f(), false);
        }

        private static void CallMatchShapesWithEmptySpan()
        {
            ImgProcCv2.MatchShapes(ReadOnlySpan<Point>.Empty, ReadOnlySpan<Point>.Empty, ShapeMatchModes.I1);
        }

        private static void CallMinAreaRectWithEmptySpan()
        {
            ImgProcCv2.MinAreaRect(ReadOnlySpan<Point>.Empty);
        }

        private static void CallFitLineWithEmptySpan()
        {
            ImgProcCv2.FitLine(ReadOnlySpan<Point>.Empty, DistanceTypes.L2, 0.0, 0.01, 0.01);
        }

        private static void CallApproxPolyNWithEmptySpan()
        {
            ImgProcCv2.ApproxPolyN(ReadOnlySpan<Point>.Empty, 3);
        }

        private static void CallFitEllipseWithEmptySpan()
        {
            ImgProcCv2.FitEllipse(ReadOnlySpan<Point>.Empty);
        }

        private static void CallFitEllipseAmsWithEmptySpan()
        {
            ImgProcCv2.FitEllipseAMS(ReadOnlySpan<Point>.Empty);
        }

        private static void CallFitEllipseDirectWithEmptySpan()
        {
            ImgProcCv2.FitEllipseDirect(ReadOnlySpan<Point>.Empty);
        }

        private static void CallGetClosestEllipsePointsWithEmptySpan()
        {
            ImgProcCv2.GetClosestEllipsePoints(default, ReadOnlySpan<Point>.Empty);
        }

        private static void CallMinEnclosingTriangleWithEmptySpan()
        {
            ImgProcCv2.MinEnclosingTriangle(ReadOnlySpan<Point>.Empty, out _);
        }

        private static void CallMinEnclosingConvexPolygonWithEmptySpan()
        {
            ImgProcCv2.MinEnclosingConvexPolygon(ReadOnlySpan<Point>.Empty, 3, out _);
        }

        private static void CallIntersectConvexConvexWithFirstEmptySpan()
        {
            Point[] polygon = new Point[] { new Point(0, 0), new Point(1, 0), new Point(0, 1) };
            ImgProcCv2.IntersectConvexConvex(ReadOnlySpan<Point>.Empty, polygon.AsSpan(), out _);
        }

        private static void CallIntersectConvexConvexWithSecondEmptySpan()
        {
            Point[] polygon = new Point[] { new Point(0, 0), new Point(1, 0), new Point(0, 1) };
            ImgProcCv2.IntersectConvexConvex(polygon.AsSpan(), ReadOnlySpan<Point>.Empty, out _);
        }

        private static void CallApproxPolyNWithInvalidSideCount(Point[] points)
        {
            ImgProcCv2.ApproxPolyN(points.AsSpan(), 2);
        }

        private static void CallMinEnclosingConvexPolygonWithInvalidVertexCount(Point[] points)
        {
            ImgProcCv2.MinEnclosingConvexPolygon(points.AsSpan(), 2, out _);
        }

        private static void CallFitEllipseWithTooFewPoints(Point[] points)
        {
            ImgProcCv2.FitEllipse(points.AsSpan());
        }

        private static void CallFitEllipseAmsWithTooFewPoints(Point[] points)
        {
            ImgProcCv2.FitEllipseAMS(points.AsSpan());
        }

        private static void CallFitEllipseDirectWithTooFewPoints(Point[] points)
        {
            ImgProcCv2.FitEllipseDirect(points.AsSpan());
        }
    }
}

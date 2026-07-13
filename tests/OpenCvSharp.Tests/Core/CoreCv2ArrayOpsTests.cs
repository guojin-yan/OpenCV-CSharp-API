using System;
using OpenCvSharp.Core;

namespace OpenCvSharp.Tests.Core
{
    public class CoreCv2ArrayOpsTests
    {
        [Fact]
        public void ArithmeticBitwiseAndComparisonWorkWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat a = new Mat(2, 3, MatType.CV_8UC1))
            using (Mat b = new Mat(2, 3, MatType.CV_8UC1))
            using (Mat add = new Mat())
            using (Mat subtract = new Mat())
            using (Mat multiply = new Mat())
            using (Mat divide = new Mat())
            using (Mat weighted = new Mat())
            using (Mat absDiff = new Mat())
            using (Mat bitwiseAnd = new Mat())
            using (Mat bitwiseOr = new Mat())
            using (Mat bitwiseXor = new Mat())
            using (Mat bitwiseNot = new Mat())
            using (Mat compare = new Mat())
            using (Mat min = new Mat())
            using (Mat max = new Mat())
            using (Mat inRange = new Mat())
            {
                a.CopyFrom(new byte[] { 1, 2, 3, 4, 5, 6 });
                b.CopyFrom(new byte[] { 6, 5, 4, 3, 2, 1 });

                Cv2.Add(a, b, add);
                Cv2.Subtract(b, a, subtract);
                Cv2.Multiply(a, b, multiply);
                Cv2.Divide(b, a, divide);
                Cv2.AddWeighted(a, 0.5, b, 0.5, 1.0, weighted);
                Cv2.AbsDiff(a, b, absDiff);
                Cv2.BitwiseAnd(a, b, bitwiseAnd);
                Cv2.BitwiseOr(a, b, bitwiseOr);
                Cv2.BitwiseXor(a, b, bitwiseXor);
                Cv2.BitwiseNot(a, bitwiseNot);
                Cv2.Compare(a, b, compare, CmpTypes.LT);
                Cv2.Min(a, b, min);
                Cv2.Max(a, b, max);
                Cv2.InRange(a, new Scalar(2), new Scalar(5), inRange);

                Assert.Equal(new byte[] { 7, 7, 7, 7, 7, 7 }, add.ToBytes());
                Assert.Equal(new byte[] { 5, 3, 1, 0, 0, 0 }, subtract.ToBytes());
                Assert.Equal(new byte[] { 6, 10, 12, 12, 10, 6 }, multiply.ToBytes());
                Assert.Equal(6, divide.ByteLength);
                Assert.Equal(new byte[] { 5, 3, 1, 1, 3, 5 }, absDiff.ToBytes());
                Assert.Equal(new byte[] { 0, 0, 0, 0, 0, 0 }, bitwiseAnd.ToBytes());
                Assert.Equal(new byte[] { 7, 7, 7, 7, 7, 7 }, bitwiseOr.ToBytes());
                Assert.Equal(new byte[] { 7, 7, 7, 7, 7, 7 }, bitwiseXor.ToBytes());
                Assert.Equal(new byte[] { 255, 255, 255, 0, 0, 0 }, compare.ToBytes());
                Assert.Equal(new byte[] { 1, 2, 3, 3, 2, 1 }, min.ToBytes());
                Assert.Equal(new byte[] { 6, 5, 4, 4, 5, 6 }, max.ToBytes());
                Assert.Equal(new byte[] { 0, 255, 255, 255, 255, 0 }, inRange.ToBytes());

                using (Mat returnedAdd = Cv2.Add(a, b))
                using (Mat returnedAddScalar = Cv2.Add(a, new Scalar(1)))
                using (Mat returnedSubtract = Cv2.Subtract(b, a))
                using (Mat returnedSubtractScalar = Cv2.Subtract(b, new Scalar(1)))
                using (Mat returnedMultiply = Cv2.Multiply(a, b))
                using (Mat returnedDivide = Cv2.Divide(b, a))
                using (Mat returnedScaleAdd = Cv2.ScaleAdd(a, 2.0, b))
                using (Mat returnedWeighted = Cv2.AddWeighted(a, 0.5, b, 0.5, 1.0))
                using (Mat returnedAbsDiff = Cv2.AbsDiff(a, b))
                using (Mat returnedAbsDiffScalar = Cv2.AbsDiff(a, new Scalar(1)))
                using (Mat returnedBitwiseAnd = Cv2.BitwiseAnd(a, b))
                using (Mat returnedBitwiseOr = Cv2.BitwiseOr(a, b))
                using (Mat returnedBitwiseXor = Cv2.BitwiseXor(a, b))
                using (Mat returnedBitwiseNot = Cv2.BitwiseNot(a))
                using (Mat returnedCompare = Cv2.Compare(a, b, CmpTypes.LT))
                using (Mat returnedMin = Cv2.Min(a, b))
                using (Mat returnedMax = Cv2.Max(a, b))
                using (Mat returnedInRange = Cv2.InRange(a, new Scalar(2), new Scalar(5)))
                {
                    Assert.Equal(add.ToBytes(), returnedAdd.ToBytes());
                    Assert.Equal(new byte[] { 2, 3, 4, 5, 6, 7 }, returnedAddScalar.ToBytes());
                    Assert.Equal(subtract.ToBytes(), returnedSubtract.ToBytes());
                    Assert.Equal(new byte[] { 5, 4, 3, 2, 1, 0 }, returnedSubtractScalar.ToBytes());
                    Assert.Equal(multiply.ToBytes(), returnedMultiply.ToBytes());
                    Assert.Equal(divide.ByteLength, returnedDivide.ByteLength);
                    Assert.Equal(new byte[] { 8, 9, 10, 11, 12, 13 }, returnedScaleAdd.ToBytes());
                    Assert.Equal(weighted.ToBytes(), returnedWeighted.ToBytes());
                    Assert.Equal(absDiff.ToBytes(), returnedAbsDiff.ToBytes());
                    Assert.Equal(new byte[] { 0, 1, 2, 3, 4, 5 }, returnedAbsDiffScalar.ToBytes());
                    Assert.Equal(bitwiseAnd.ToBytes(), returnedBitwiseAnd.ToBytes());
                    Assert.Equal(bitwiseOr.ToBytes(), returnedBitwiseOr.ToBytes());
                    Assert.Equal(bitwiseXor.ToBytes(), returnedBitwiseXor.ToBytes());
                    Assert.Equal(bitwiseNot.ToBytes(), returnedBitwiseNot.ToBytes());
                    Assert.Equal(compare.ToBytes(), returnedCompare.ToBytes());
                    Assert.Equal(min.ToBytes(), returnedMin.ToBytes());
                    Assert.Equal(max.ToBytes(), returnedMax.ToBytes());
                    Assert.Equal(inRange.ToBytes(), returnedInRange.ToBytes());
                }
            }
        }

        [Fact]
        public void StatisticsLinearAlgebraAndResultObjectsWorkWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat values = new Mat(2, 3, MatType.CV_8UC1))
            using (Mat normalized = new Mat())
            using (Mat reduced = new Mat())
            using (Mat matrix = new Mat(2, 2, MatType.CV_64FC1))
            using (Mat rhs = new Mat(2, 1, MatType.CV_64FC1))
            using (Mat inverse = new Mat())
            using (Mat solution = new Mat())
            using (Mat identity = Mat.Eye(2, 2, MatType.CV_64FC1))
            {
                values.CopyFrom(new byte[] { 1, 2, 3, 4, 5, 6 });
                matrix.CopyFrom<double>(new double[] { 1.0, 2.0, 3.0, 4.0 });
                rhs.CopyFrom<double>(new double[] { 5.0, 11.0 });

                Scalar sum = Cv2.Sum(values);
                Scalar mean = Cv2.Mean(values);
                MeanStdDevResult meanStdDev = Cv2.MeanStdDev(values);
                MinMaxLocResult minMax = Cv2.MinMaxLoc(values);
                double norm = Cv2.Norm(values, NormTypes.L2);
                Cv2.Normalize(values, normalized, 0.0, 255.0, NormTypes.MinMax);
                Cv2.Reduce(values, reduced, 0, ReduceTypes.Sum, MatType.CV_32S);
                Scalar trace = Cv2.Trace(matrix);
                double determinant = Cv2.Determinant(matrix);
                double invertQuality = Cv2.Invert(matrix, inverse);
                bool solved = Cv2.Solve(matrix, rhs, solution);
                double mahalanobis = Cv2.Mahalanobis(rhs, rhs, identity);

                Assert.Equal(21.0, sum.V0);
                Assert.Equal(3.5, mean.V0);
                Assert.Equal(3.5, meanStdDev.Mean.V0);
                Assert.True(meanStdDev.StdDev.V0 > 1.0);
                Assert.Equal(1.0, minMax.MinVal);
                Assert.Equal(6.0, minMax.MaxVal);
                Assert.Equal(new Point(0, 0).ToString(), minMax.MinLoc.ToString());
                Assert.Equal(new Point(2, 1).ToString(), minMax.MaxLoc.ToString());
                Assert.True(norm > 9.0);
                Assert.Equal(1, reduced.Rows);
                Assert.Equal(3, reduced.Cols);
                Assert.Equal(5.0, trace.V0);
                Assert.Equal(-2.0, determinant, 6);
                Assert.NotEqual(0.0, invertQuality);
                Assert.True(solved);
                Assert.Equal(0.0, mahalanobis, 6);
                Assert.Contains("MinVal=1", minMax.ToString(), StringComparison.Ordinal);
                Assert.Equal("{Mean=" + meanStdDev.Mean + ",StdDev=" + meanStdDev.StdDev + "}", meanStdDev.ToString());
            }
        }

        [Fact]
        public void TraceRejectsMultiDimensionalSourceContract()
        {
            using (var image = new Mat(4, 4, MatType.CV_8UC3, new Scalar(1, 2, 3)))
            using (Mat blob = OpenCvSharp.Dnn.Cv2.BlobFromImage(image, 1.0, new Size(4, 4)))
            {
                Assert.Equal(4, blob.Dims);
                ArgumentException exception = Assert.Throws<ArgumentException>(() => Cv2.Trace(blob));
                Assert.Equal("src", exception.ParamName);
            }
        }

        [Fact]
        public void ChannelAndLayoutOperationsWorkWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat color = new Mat(2, 2, MatType.CV_8UC3))
            using (Mat inserted = new Mat(2, 2, MatType.CV_8UC3))
            using (Mat gray = new Mat(2, 2, MatType.CV_8UC1))
            using (Mat repeated = new Mat())
            using (Mat flipped = new Mat())
            using (Mat rotated = new Mat())
            using (Mat transposed = new Mat())
            using (Mat lut = new Mat(1, 256, MatType.CV_8UC1))
            using (Mat lutResult = new Mat())
            using (Mat scaledAbs = new Mat())
            using (Mat symm = new Mat(2, 2, MatType.CV_64FC1))
            using (Mat identity = new Mat(2, 2, MatType.CV_64FC1))
            {
                color.CopyFrom(new byte[]
                {
                    1, 10, 100,
                    2, 20, 110,
                    3, 30, 120,
                    4, 40, 130
                });
                gray.CopyFrom(new byte[] { 9, 8, 7, 6 });
                lut.CopyFrom(CreateIdentityLut());
                symm.CopyFrom<double>(new double[] { 1.0, 2.0, 0.0, 4.0 });
                inserted.SetTo(new Scalar(0));

                Mat[] channels = Cv2.Split(color);
                try
                {
                    using (Mat merged = Cv2.Merge(channels))
                    using (Mat extracted = Cv2.ExtractChannel(color, 1))
                    using (Mat insertedChannel = new Mat())
                    using (Mat returnedRepeated = Cv2.Repeat(gray, 2, 1))
                    using (Mat returnedFlipped = Cv2.Flip(gray, 1))
                    using (Mat returnedRotated = Cv2.Rotate(gray, RotateFlags.Rotate90Clockwise))
                    using (Mat returnedTransposed = Cv2.Transpose(gray))
                    using (Mat returnedLut = Cv2.Lut(gray, lut))
                    using (Mat returnedScaledAbs = Cv2.ConvertScaleAbs(gray, 2.0))
                    {
                        Cv2.InsertChannel(gray, inserted, 1);
                        Cv2.ExtractChannel(inserted, insertedChannel, 1);
                        Cv2.Repeat(gray, 2, 1, repeated);
                        Cv2.Flip(gray, flipped, 1);
                        Cv2.Rotate(gray, rotated, RotateFlags.Rotate90Clockwise);
                        Cv2.Transpose(gray, transposed);
                        Cv2.Lut(gray, lut, lutResult);
                        Cv2.ConvertScaleAbs(gray, scaledAbs, 2.0);
                        Cv2.CompleteSymm(symm);
                        Cv2.SetIdentity(identity);

                        Assert.Equal(3, channels.Length);
                        Assert.Equal(new byte[] { 1, 2, 3, 4 }, channels[0].ToBytes());
                        Assert.Equal(color.ToBytes(), merged.ToBytes());
                        Assert.Equal(new byte[] { 10, 20, 30, 40 }, extracted.ToBytes());
                        Assert.Equal(gray.ToBytes(), insertedChannel.ToBytes());
                        Assert.Equal(4, repeated.Rows);
                        Assert.Equal(2, repeated.Cols);
                        Assert.Equal(new byte[] { 8, 9, 6, 7 }, flipped.ToBytes());
                        Assert.Equal(2, rotated.Rows);
                        Assert.Equal(2, rotated.Cols);
                        Assert.Equal(2, transposed.Rows);
                        Assert.Equal(2, transposed.Cols);
                        Assert.Equal(gray.ToBytes(), lutResult.ToBytes());
                        Assert.Equal(new byte[] { 18, 16, 14, 12 }, scaledAbs.ToBytes());
                        Assert.Equal(returnedRepeated.ToBytes(), repeated.ToBytes());
                        Assert.Equal(returnedFlipped.ToBytes(), flipped.ToBytes());
                        Assert.Equal(returnedRotated.ToBytes(), rotated.ToBytes());
                        Assert.Equal(returnedTransposed.ToBytes(), transposed.ToBytes());
                        Assert.Equal(returnedLut.ToBytes(), lutResult.ToBytes());
                        Assert.Equal(returnedScaledAbs.ToBytes(), scaledAbs.ToBytes());
                        Assert.Equal(0.0, symm.ToArray<double>()[1]);
                        Assert.Equal(new double[] { 1.0, 0.0, 0.0, 1.0 }, identity.ToArray<double>());
                    }
                }
                finally
                {
                    for (int i = 0; i < channels.Length; i++)
                    {
                        channels[i].Dispose();
                    }
                }
            }
        }

        [Fact]
        public void MergeRejectsInvalidSourceContract()
        {
            using (var first = new Mat(2, 2, MatType.CV_8UC1))
            using (var sizeMismatch = new Mat(1, 2, MatType.CV_8UC1))
            using (var depthMismatch = new Mat(2, 2, MatType.CV_32FC1))
            using (var empty = new Mat())
            using (var dst = new Mat())
            {
                ArgumentException emptyException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Merge(new[] { empty, first }, dst));
                ArgumentException sizeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Merge(new[] { first, sizeMismatch }, dst));
                ArgumentException depthException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Merge(new[] { first, depthMismatch }, dst));
                ArgumentException returningException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Merge(new[] { first, sizeMismatch }));

                Assert.Equal("src", emptyException.ParamName);
                Assert.Equal("src", sizeException.ParamName);
                Assert.Equal("src", depthException.ParamName);
                Assert.Equal("src", returningException.ParamName);
            }
        }

        [Fact]
        public void MergeRejectsExcessiveChannelCountContract()
        {
            Mat[] sources = new Mat[MatType.ChannelMax + 1];
            try
            {
                for (int i = 0; i < sources.Length; i++)
                {
                    sources[i] = new Mat(1, 1, MatType.CV_8UC1);
                }

                using (var dst = new Mat())
                {
                    ArgumentException exception = Assert.Throws<ArgumentException>(() => Cv2.Merge(sources, dst));
                    Assert.Equal("src", exception.ParamName);
                }
            }
            finally
            {
                for (int i = 0; i < sources.Length; i++)
                {
                    sources[i]?.Dispose();
                }
            }
        }

        [Fact]
        public void ManagedValidationRejectsInvalidCoreArguments()
        {
            Assert.Throws<ArgumentNullException>(() => Cv2.Add(null!, null!, null!));
            Assert.Throws<ArgumentNullException>(() => Cv2.Add(null!, new Scalar(1)));
            Assert.Throws<ArgumentNullException>(() => Cv2.Subtract(null!, new Scalar(1)));
            Assert.Throws<ArgumentNullException>(() => Cv2.Multiply(null!, null!));
            Assert.Throws<ArgumentNullException>(() => Cv2.Divide(null!, null!));
            Assert.Throws<ArgumentNullException>(() => Cv2.ScaleAdd(null!, 1.0, null!));
            Assert.Throws<ArgumentNullException>(() => Cv2.AddWeighted(null!, 1.0, null!, 1.0, 0.0));
            Assert.Throws<ArgumentNullException>(() => Cv2.AbsDiff(null!, null!));
            Assert.Throws<ArgumentNullException>(() => Cv2.AbsDiff(null!, new Scalar(1)));
            Assert.Throws<ArgumentNullException>(() => Cv2.BitwiseAnd(null!, null!));
            Assert.Throws<ArgumentNullException>(() => Cv2.BitwiseOr(null!, null!));
            Assert.Throws<ArgumentNullException>(() => Cv2.BitwiseXor(null!, null!));
            Assert.Throws<ArgumentNullException>(() => Cv2.BitwiseNot(null!));
            Assert.Throws<ArgumentNullException>(() => Cv2.Compare(null!, null!, CmpTypes.EQ));
            Assert.Throws<ArgumentOutOfRangeException>(() => Cv2.Compare(new Mat(), new Mat(), new Mat(), (CmpTypes)99));
            Assert.Throws<ArgumentOutOfRangeException>(() => Cv2.Compare(new Mat(), new Mat(), (CmpTypes)99));
            Assert.Throws<ArgumentNullException>(() => Cv2.Min(null!, null!));
            Assert.Throws<ArgumentNullException>(() => Cv2.Max(null!, null!));
            Assert.Throws<ArgumentNullException>(() => Cv2.InRange(null!, new Scalar(0), new Scalar(1)));
            Assert.Throws<ArgumentException>(() => Cv2.Merge(Array.Empty<Mat>()));
            Assert.Throws<ArgumentNullException>(() => Cv2.ExtractChannel(null!, 0));
            Assert.Throws<ArgumentNullException>(() => Cv2.Repeat(null!, 1, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => Cv2.Repeat(new Mat(), 0, 1));
            Assert.Throws<ArgumentNullException>(() => Cv2.Flip(null!, 1));
            Assert.Throws<ArgumentNullException>(() => Cv2.Rotate(null!, RotateFlags.Rotate90Clockwise));
            Assert.Throws<ArgumentOutOfRangeException>(() => Cv2.Rotate(new Mat(), new Mat(), (RotateFlags)99));
            Assert.Throws<ArgumentOutOfRangeException>(() => Cv2.Rotate(new Mat(), (RotateFlags)99));
            Assert.Throws<ArgumentNullException>(() => Cv2.Transpose(null!));
            Assert.Throws<ArgumentNullException>(() => Cv2.Lut(null!, null!));
            Assert.Throws<ArgumentNullException>(() => Cv2.ConvertScaleAbs(null!));
            Assert.Throws<ArgumentNullException>(() => Cv2.Mean(null!));
            Assert.Throws<ArgumentOutOfRangeException>(() => Cv2.Reduce(new Mat(), new Mat(), 0, (ReduceTypes)99));
            Assert.Throws<ArgumentOutOfRangeException>(() => Cv2.Invert(new Mat(), new Mat(), DecompTypes.QR));
            Assert.Throws<ArgumentOutOfRangeException>(() => Cv2.Invert(new Mat(), new Mat(), DecompTypes.Normal | DecompTypes.LU));
            Assert.Throws<ArgumentOutOfRangeException>(() => Cv2.Solve(new Mat(), new Mat(), new Mat(), (DecompTypes)99));
            Assert.Throws<ArgumentException>(() => Cv2.Merge(Array.Empty<Mat>(), null!));
            Assert.Throws<ArgumentException>(() => Cv2.MixChannels(Array.Empty<Mat>(), Array.Empty<Mat>(), Array.Empty<int>()));
        }

        [Fact]
        public void ArithmeticAndBitwiseRejectInvalidMaskContract()
        {
            using (var src1 = new Mat(2, 2, MatType.CV_8UC1))
            using (var src2 = new Mat(2, 2, MatType.CV_8UC1))
            using (var dst = new Mat())
            using (var colorMask = new Mat(2, 2, MatType.CV_8UC3))
            using (var smallMask = new Mat(1, 2, MatType.CV_8UC1))
            using (var signedMask = new Mat(2, 2, MatType.CV_8SC1))
            {
                ArgumentException addMaskTypeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Add(src1, src2, dst, colorMask));
                ArgumentException addMaskSizeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Add(src1, new Scalar(1), dst, smallMask));
                ArgumentException bitwiseMaskTypeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.BitwiseAnd(src1, src2, dst, colorMask));
                ArgumentException bitwiseMaskSizeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.BitwiseNot(src1, dst, smallMask));

                Cv2.Subtract(src1, src2, dst, signedMask);
                Cv2.BitwiseOr(src1, src2, dst, signedMask);

                Assert.Equal("mask", addMaskTypeException.ParamName);
                Assert.Equal("mask", addMaskSizeException.ParamName);
                Assert.Equal("mask", bitwiseMaskTypeException.ParamName);
                Assert.Equal("mask", bitwiseMaskSizeException.ParamName);
            }
        }

        [Fact]
        public void CompareRejectsMismatchedEmptySourceContract()
        {
            using (var empty = new Mat())
            using (var nonEmpty = new Mat(1, 1, MatType.CV_8UC1))
            using (var dst = new Mat())
            {
                ArgumentException emptyFirstException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Compare(empty, nonEmpty, dst, CmpTypes.EQ));
                ArgumentException emptySecondException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Compare(nonEmpty, empty, dst, CmpTypes.EQ));
                ArgumentException returningException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Compare(nonEmpty, empty, CmpTypes.EQ));

                Assert.Equal("src1", emptyFirstException.ParamName);
                Assert.Equal("src2", emptySecondException.ParamName);
                Assert.Equal("src2", returningException.ParamName);
            }
        }

        [Fact]
        public void CompareRejectsMismatchedSourceSizeAndTypeUnlessScalarContract()
        {
            using (var src = new Mat(2, 2, MatType.CV_8UC3, new Scalar(1, 2, 3)))
            using (var sizeMismatch = new Mat(1, 2, MatType.CV_8UC3))
            using (var typeMismatch = new Mat(2, 2, MatType.CV_16UC3))
            using (var scalarRow = new Mat(1, 3, MatType.CV_8UC1, new Scalar(2)))
            using (var scalarColumn64 = new Mat(4, 1, MatType.CV_64FC1, new Scalar(2)))
            using (var dst = new Mat())
            {
                ArgumentException sizeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Compare(src, sizeMismatch, dst, CmpTypes.EQ));
                ArgumentException typeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Compare(src, typeMismatch, dst, CmpTypes.EQ));
                ArgumentException returningException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Compare(src, sizeMismatch, CmpTypes.EQ));

                Cv2.Compare(src, scalarRow, dst, CmpTypes.GE);
                using (Mat returned = Cv2.Compare(scalarColumn64, src, CmpTypes.LE))
                {
                    Assert.Equal(src.Size, dst.Size);
                    Assert.Equal(src.Size, returned.Size);
                    Assert.Equal(MatType.CV_8UC3, dst.Type);
                    Assert.Equal(MatType.CV_8UC3, returned.Type);
                }

                Assert.Equal("src2", sizeException.ParamName);
                Assert.Equal("src2", typeException.ParamName);
                Assert.Equal("src2", returningException.ParamName);
            }
        }

        [Fact]
        public void AddRejectsInvalidArrayShapeAndRequiresDtypeUnlessScalarContract()
        {
            using (var src = new Mat(2, 2, MatType.CV_8UC3, new Scalar(1, 2, 3)))
            using (var sizeMismatch = new Mat(1, 2, MatType.CV_8UC3))
            using (var channelMismatch = new Mat(2, 2, MatType.CV_8UC1))
            using (var depthMismatch = new Mat(2, 2, MatType.CV_16UC3))
            using (var scalarRow = new Mat(1, 3, MatType.CV_64FC1, new Scalar(2)))
            using (var dst = new Mat())
            {
                ArgumentException sizeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Add(src, sizeMismatch, dst));
                ArgumentException channelException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Add(src, channelMismatch, dst, dtype: MatType.CV_32F));
                ArgumentException dtypeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Add(src, depthMismatch, dst));
                ArgumentException returningException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Add(src, sizeMismatch));

                Cv2.Add(src, depthMismatch, dst, dtype: MatType.CV_32F);
                Assert.Equal(MatType.CV_32FC3, dst.Type);

                Cv2.Add(src, scalarRow, dst);
                using (Mat returned = Cv2.Add(src, scalarRow))
                {
                    Assert.Equal(src.Size, dst.Size);
                    Assert.Equal(src.Size, returned.Size);
                    Assert.Equal(src.Type, dst.Type);
                    Assert.Equal(src.Type, returned.Type);
                }

                Assert.Equal("src2", sizeException.ParamName);
                Assert.Equal("src2", channelException.ParamName);
                Assert.Equal("dtype", dtypeException.ParamName);
                Assert.Equal("src2", returningException.ParamName);
            }
        }

        [Fact]
        public void SubtractRejectsInvalidArrayShapeAndRequiresDtypeUnlessScalarContract()
        {
            using (var src = new Mat(2, 2, MatType.CV_8UC3, new Scalar(10, 20, 30)))
            using (var sizeMismatch = new Mat(1, 2, MatType.CV_8UC3))
            using (var channelMismatch = new Mat(2, 2, MatType.CV_8UC1))
            using (var depthMismatch = new Mat(2, 2, MatType.CV_16UC3))
            using (var scalarRow = new Mat(1, 3, MatType.CV_64FC1, new Scalar(2)))
            using (var dst = new Mat())
            {
                ArgumentException sizeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Subtract(src, sizeMismatch, dst));
                ArgumentException channelException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Subtract(src, channelMismatch, dst, dtype: MatType.CV_32F));
                ArgumentException dtypeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Subtract(src, depthMismatch, dst));
                ArgumentException returningException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Subtract(src, sizeMismatch));

                Cv2.Subtract(src, depthMismatch, dst, dtype: MatType.CV_32F);
                Assert.Equal(MatType.CV_32FC3, dst.Type);

                Cv2.Subtract(src, scalarRow, dst);
                using (Mat returned = Cv2.Subtract(scalarRow, src))
                {
                    Assert.Equal(src.Size, dst.Size);
                    Assert.Equal(src.Size, returned.Size);
                    Assert.Equal(src.Type, dst.Type);
                    Assert.Equal(src.Type, returned.Type);
                }

                Assert.Equal("src2", sizeException.ParamName);
                Assert.Equal("src2", channelException.ParamName);
                Assert.Equal("dtype", dtypeException.ParamName);
                Assert.Equal("src2", returningException.ParamName);
            }
        }

        [Fact]
        public void ArithmeticRejectsMismatchedEmptySourceContract()
        {
            using (var empty = new Mat())
            using (var nonEmpty = new Mat(1, 1, MatType.CV_8UC1))
            using (var dst = new Mat())
            {
                ArgumentException addException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Add(empty, nonEmpty, dst));
                ArgumentException subtractException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Subtract(nonEmpty, empty, dst));
                ArgumentException divideException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Divide(empty, nonEmpty, dst));
                ArgumentException addWeightedException = Assert.Throws<ArgumentException>(() =>
                    Cv2.AddWeighted(nonEmpty, 0.5, empty, 0.5, 0.0, dst));
                ArgumentException absDiffException = Assert.Throws<ArgumentException>(() =>
                    Cv2.AbsDiff(empty, nonEmpty));

                Assert.Equal("src1", addException.ParamName);
                Assert.Equal("src2", subtractException.ParamName);
                Assert.Equal("src1", divideException.ParamName);
                Assert.Equal("src2", addWeightedException.ParamName);
                Assert.Equal("src1", absDiffException.ParamName);
            }
        }

        [Fact]
        public void BitwiseRejectsMismatchedSourceSizeAndTypeContract()
        {
            using (var src = new Mat(2, 2, MatType.CV_8UC1))
            using (var sizeMismatch = new Mat(1, 2, MatType.CV_8UC1))
            using (var typeMismatch = new Mat(2, 2, MatType.CV_16UC1))
            using (var dst = new Mat())
            {
                ArgumentException andSizeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.BitwiseAnd(src, sizeMismatch, dst));
                ArgumentException orTypeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.BitwiseOr(src, typeMismatch, dst));
                ArgumentException xorSizeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.BitwiseXor(src, sizeMismatch));

                Assert.Equal("src2", andSizeException.ParamName);
                Assert.Equal("src2", orTypeException.ParamName);
                Assert.Equal("src2", xorSizeException.ParamName);
            }
        }

        [Fact]
        public void MinMaxRejectMismatchedSourceSizeAndTypeContract()
        {
            using (var src = new Mat(2, 2, MatType.CV_8UC1))
            using (var sizeMismatch = new Mat(1, 2, MatType.CV_8UC1))
            using (var typeMismatch = new Mat(2, 2, MatType.CV_16UC1))
            using (var dst = new Mat())
            {
                ArgumentException minSizeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Min(src, sizeMismatch, dst));
                ArgumentException maxTypeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Max(src, typeMismatch, dst));
                ArgumentException returningMinException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Min(src, sizeMismatch));
                ArgumentException returningMaxException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Max(src, typeMismatch));

                Assert.Equal("src2", minSizeException.ParamName);
                Assert.Equal("src2", maxTypeException.ParamName);
                Assert.Equal("src2", returningMinException.ParamName);
                Assert.Equal("src2", returningMaxException.ParamName);
            }
        }

        [Fact]
        public void ScaleAddRejectsMismatchedSourceSizeAndTypeContract()
        {
            using (var src = new Mat(2, 2, MatType.CV_32FC1))
            using (var sizeMismatch = new Mat(1, 2, MatType.CV_32FC1))
            using (var typeMismatch = new Mat(2, 2, MatType.CV_64FC1))
            using (var dst = new Mat())
            {
                ArgumentException sizeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.ScaleAdd(src, 2.0, sizeMismatch, dst));
                ArgumentException typeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.ScaleAdd(src, 2.0, typeMismatch, dst));
                ArgumentException returningException = Assert.Throws<ArgumentException>(() =>
                    Cv2.ScaleAdd(src, 2.0, sizeMismatch));

                Assert.Equal("src2", sizeException.ParamName);
                Assert.Equal("src2", typeException.ParamName);
                Assert.Equal("src2", returningException.ParamName);
            }
        }

        [Fact]
        public void AbsDiffRejectsMismatchedSourceSizeAndTypeContract()
        {
            using (var src = new Mat(2, 2, MatType.CV_8UC1))
            using (var sizeMismatch = new Mat(1, 2, MatType.CV_8UC1))
            using (var typeMismatch = new Mat(2, 2, MatType.CV_16UC1))
            using (var dst = new Mat())
            {
                ArgumentException sizeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.AbsDiff(src, sizeMismatch, dst));
                ArgumentException typeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.AbsDiff(src, typeMismatch, dst));
                ArgumentException returningException = Assert.Throws<ArgumentException>(() =>
                    Cv2.AbsDiff(src, sizeMismatch));

                Assert.Equal("src2", sizeException.ParamName);
                Assert.Equal("src2", typeException.ParamName);
                Assert.Equal("src2", returningException.ParamName);
            }
        }

        [Fact]
        public void MultiplyRejectsInvalidArrayShapeAndRequiresDtypeForDepthMismatch()
        {
            using (var src = new Mat(2, 2, MatType.CV_8UC1))
            using (var sizeMismatch = new Mat(1, 2, MatType.CV_8UC1))
            using (var channelMismatch = new Mat(2, 2, MatType.CV_8UC3))
            using (var depthMismatch = new Mat(2, 2, MatType.CV_16UC1))
            using (var dst = new Mat())
            {
                ArgumentException sizeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Multiply(src, sizeMismatch, dst));
                ArgumentException channelException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Multiply(src, channelMismatch, dst, dtype: MatType.CV_32F));
                ArgumentException dtypeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Multiply(src, depthMismatch, dst));
                ArgumentException returningException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Multiply(src, sizeMismatch));

                Cv2.Multiply(src, depthMismatch, dst, dtype: MatType.CV_32F);
                using (Mat returned = Cv2.Multiply(src, depthMismatch, dtype: MatType.CV_32F))
                {
                    Assert.Equal(MatType.CV_32FC1, dst.Type);
                    Assert.Equal(MatType.CV_32FC1, returned.Type);
                }

                Assert.Equal("src2", sizeException.ParamName);
                Assert.Equal("src2", channelException.ParamName);
                Assert.Equal("dtype", dtypeException.ParamName);
                Assert.Equal("src2", returningException.ParamName);
            }
        }

        [Fact]
        public void DivideRejectsInvalidArrayShapeAndRequiresDtypeForDepthMismatch()
        {
            using (var src = new Mat(2, 2, MatType.CV_8UC1))
            using (var sizeMismatch = new Mat(1, 2, MatType.CV_8UC1))
            using (var channelMismatch = new Mat(2, 2, MatType.CV_8UC3))
            using (var depthMismatch = new Mat(2, 2, MatType.CV_16UC1))
            using (var dst = new Mat())
            {
                ArgumentException sizeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Divide(src, sizeMismatch, dst));
                ArgumentException channelException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Divide(src, channelMismatch, dst, dtype: MatType.CV_32F));
                ArgumentException dtypeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Divide(src, depthMismatch, dst));
                ArgumentException returningException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Divide(src, sizeMismatch));

                Cv2.Divide(src, depthMismatch, dst, dtype: MatType.CV_32F);
                using (Mat returned = Cv2.Divide(src, depthMismatch, dtype: MatType.CV_32F))
                {
                    Assert.Equal(MatType.CV_32FC1, dst.Type);
                    Assert.Equal(MatType.CV_32FC1, returned.Type);
                }

                Assert.Equal("src2", sizeException.ParamName);
                Assert.Equal("src2", channelException.ParamName);
                Assert.Equal("dtype", dtypeException.ParamName);
                Assert.Equal("src2", returningException.ParamName);
            }
        }

        [Fact]
        public void AddWeightedRejectsInvalidArrayShapeAndRequiresDtypeForDepthMismatch()
        {
            using (var src = new Mat(2, 2, MatType.CV_8UC1))
            using (var sizeMismatch = new Mat(1, 2, MatType.CV_8UC1))
            using (var channelMismatch = new Mat(2, 2, MatType.CV_8UC3))
            using (var depthMismatch = new Mat(2, 2, MatType.CV_16UC1))
            using (var dst = new Mat())
            {
                ArgumentException sizeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.AddWeighted(src, 0.5, sizeMismatch, 0.5, 0.0, dst));
                ArgumentException channelException = Assert.Throws<ArgumentException>(() =>
                    Cv2.AddWeighted(src, 0.5, channelMismatch, 0.5, 0.0, dst, dtype: MatType.CV_32F));
                ArgumentException dtypeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.AddWeighted(src, 0.5, depthMismatch, 0.5, 0.0, dst));
                ArgumentException returningException = Assert.Throws<ArgumentException>(() =>
                    Cv2.AddWeighted(src, 0.5, sizeMismatch, 0.5, 0.0));

                Cv2.AddWeighted(src, 0.5, depthMismatch, 0.5, 0.0, dst, dtype: MatType.CV_32F);
                using (Mat returned = Cv2.AddWeighted(src, 0.5, depthMismatch, 0.5, 0.0, dtype: MatType.CV_32F))
                {
                    Assert.Equal(MatType.CV_32FC1, dst.Type);
                    Assert.Equal(MatType.CV_32FC1, returned.Type);
                }

                Assert.Equal("src2", sizeException.ParamName);
                Assert.Equal("src2", channelException.ParamName);
                Assert.Equal("dtype", dtypeException.ParamName);
                Assert.Equal("src2", returningException.ParamName);
            }
        }

        [Fact]
        public void NormalizeRejectsUnsupportedNormTypeContract()
        {
            using (var src = new Mat(1, 3, MatType.CV_8UC1))
            using (var dst = new Mat())
            {
                ArgumentOutOfRangeException l2SqrException = Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Cv2.Normalize(src, dst, normType: NormTypes.L2Sqr));
                ArgumentOutOfRangeException hammingException = Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Cv2.Normalize(src, dst, normType: NormTypes.Hamming));
                ArgumentOutOfRangeException relativeException = Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Cv2.Normalize(src, dst, normType: NormTypes.Relative | NormTypes.L2));
                ArgumentOutOfRangeException unknownException = Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Cv2.Normalize(src, dst, normType: (NormTypes)99));

                Assert.Equal("normType", l2SqrException.ParamName);
                Assert.Equal("normType", hammingException.ParamName);
                Assert.Equal("normType", relativeException.ParamName);
                Assert.Equal("normType", unknownException.ParamName);
            }
        }

        [Fact]
        public void InRangeRejectsEmptySourceContract()
        {
            using (var empty = new Mat())
            using (var dst = new Mat())
            {
                ArgumentException voidException = Assert.Throws<ArgumentException>(() =>
                    Cv2.InRange(empty, new Scalar(0), new Scalar(1), dst));
                ArgumentException returningException = Assert.Throws<ArgumentException>(() =>
                    Cv2.InRange(empty, new Scalar(0), new Scalar(1)));

                Assert.Equal("src", voidException.ParamName);
                Assert.Equal("src", returningException.ParamName);
            }
        }

        [Fact]
        public void ReduceRejectsInvalidDimensionContract()
        {
            using (var src = new Mat(2, 3, MatType.CV_8UC1))
            using (var dst = new Mat())
            {
                ArgumentOutOfRangeException negativeException = Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Cv2.Reduce(src, dst, -1, ReduceTypes.Sum));
                ArgumentOutOfRangeException tooLargeException = Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Cv2.Reduce(src, dst, 2, ReduceTypes.Sum));

                Assert.Equal("dim", negativeException.ParamName);
                Assert.Equal("dim", tooLargeException.ParamName);
            }
        }

        [Fact]
        public void RepeatRejectsSourceWithMoreThanTwoDimensions()
        {
            using (var image = new Mat(4, 4, MatType.CV_8UC3, new Scalar(1, 2, 3)))
            using (Mat blob = OpenCvSharp.Dnn.Cv2.BlobFromImage(image, 1.0, new Size(4, 4)))
            using (var dst = new Mat())
            {
                Assert.Equal(4, blob.Dims);

                ArgumentException exception = Assert.Throws<ArgumentException>(() =>
                    Cv2.Repeat(blob, 2, 2, dst));

                Assert.Equal("src", exception.ParamName);
            }
        }

        [Fact]
        public void FlipRejectsSourceWithMoreThanTwoDimensions()
        {
            using (var image = new Mat(4, 4, MatType.CV_8UC3, new Scalar(1, 2, 3)))
            using (Mat blob = OpenCvSharp.Dnn.Cv2.BlobFromImage(image, 1.0, new Size(4, 4)))
            using (var dst = new Mat())
            {
                Assert.Equal(4, blob.Dims);

                ArgumentException exception = Assert.Throws<ArgumentException>(() =>
                    Cv2.Flip(blob, dst, 0));

                Assert.Equal("src", exception.ParamName);
            }
        }

        [Fact]
        public void RotateRejectsSourceWithMoreThanTwoDimensions()
        {
            using (var image = new Mat(4, 4, MatType.CV_8UC3, new Scalar(1, 2, 3)))
            using (Mat blob = OpenCvSharp.Dnn.Cv2.BlobFromImage(image, 1.0, new Size(4, 4)))
            using (var dst = new Mat())
            {
                Assert.Equal(4, blob.Dims);

                ArgumentException exception = Assert.Throws<ArgumentException>(() =>
                    Cv2.Rotate(blob, dst, RotateFlags.Rotate90Clockwise));

                Assert.Equal("src", exception.ParamName);
            }
        }

        [Fact]
        public void TransposeRejectsSourceWithMoreThanTwoDimensions()
        {
            using (var image = new Mat(4, 4, MatType.CV_8UC3, new Scalar(1, 2, 3)))
            using (Mat blob = OpenCvSharp.Dnn.Cv2.BlobFromImage(image, 1.0, new Size(4, 4)))
            using (var dst = new Mat())
            {
                Assert.Equal(4, blob.Dims);

                ArgumentException exception = Assert.Throws<ArgumentException>(() =>
                    Cv2.Transpose(blob, dst));

                Assert.Equal("src", exception.ParamName);
            }
        }

        [Fact]
        public void ReduceRejectsInvalidSourceAndDtypeContract()
        {
            using (var image = new Mat(4, 4, MatType.CV_8UC3, new Scalar(1, 2, 3)))
            using (Mat blob = OpenCvSharp.Dnn.Cv2.BlobFromImage(image, 1.0, new Size(4, 4)))
            using (var threeChannel = new Mat(2, 2, MatType.CV_8UC3))
            using (var dst = new Mat())
            {
                Assert.Equal(4, blob.Dims);

                ArgumentException dimsException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Reduce(blob, dst, 0, ReduceTypes.Sum, MatType.CV_32FC1));
                ArgumentException dtypeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Reduce(threeChannel, dst, 0, ReduceTypes.Sum, MatType.CV_32FC1));

                Assert.Equal("src", dimsException.ParamName);
                Assert.Equal("dtype", dtypeException.ParamName);
            }
        }

        [Fact]
        public void InvertRejectsInvalidSourceContract()
        {
            using (var unsupportedType = new Mat(2, 2, MatType.CV_8UC1))
            using (var multiChannel = new Mat(2, 2, MatType.CV_32FC2))
            using (var rectangular = new Mat(2, 3, MatType.CV_64FC1))
            using (var dst = new Mat())
            {
                ArgumentException unsupportedTypeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Invert(unsupportedType, dst));
                ArgumentException multiChannelException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Invert(multiChannel, dst));
                ArgumentException rectangularException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Invert(rectangular, dst));

                Assert.Equal("src", unsupportedTypeException.ParamName);
                Assert.Equal("src", multiChannelException.ParamName);
                Assert.Equal("src", rectangularException.ParamName);
            }
        }

        [Fact]
        public void InvertAllowsRectangularSourceWithSvd()
        {
            using (var rectangular = new Mat(2, 3, MatType.CV_64FC1))
            using (var dst = new Mat())
            {
                rectangular.CopyFrom<double>(new[] { 1.0, 0.0, 0.0, 0.0, 1.0, 0.0 });

                double quality = Cv2.Invert(rectangular, dst, DecompTypes.SVD);

                Assert.True(quality >= 0.0);
                Assert.Equal(3, dst.Rows);
                Assert.Equal(2, dst.Cols);
                Assert.Equal(MatType.CV_64FC1, dst.Type);
            }
        }

        [Fact]
        public void SolveRejectsInvalidInputContract()
        {
            using (var coefficients = new Mat(2, 2, MatType.CV_64FC1))
            using (var rhs = new Mat(2, 1, MatType.CV_64FC1))
            using (var unsupportedType = new Mat(2, 2, MatType.CV_8UC1))
            using (var rhsTypeMismatch = new Mat(2, 1, MatType.CV_32FC1))
            using (var overdetermined = new Mat(3, 2, MatType.CV_64FC1))
            using (var underdetermined = new Mat(2, 3, MatType.CV_64FC1))
            using (var rhsRowMismatch = new Mat(3, 1, MatType.CV_64FC1))
            using (var dst = new Mat())
            {
                ArgumentException unsupportedTypeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Solve(unsupportedType, rhs, dst));
                ArgumentException rhsTypeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Solve(coefficients, rhsTypeMismatch, dst));
                ArgumentException nonSquareLuException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Solve(overdetermined, rhsRowMismatch, dst, DecompTypes.LU));
                ArgumentException underdeterminedException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Solve(underdetermined, rhs, dst, DecompTypes.SVD));
                ArgumentException rhsRowsException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Solve(coefficients, rhsRowMismatch, dst));

                Assert.Equal("src1", unsupportedTypeException.ParamName);
                Assert.Equal("src2", rhsTypeException.ParamName);
                Assert.Equal("src1", nonSquareLuException.ParamName);
                Assert.Equal("src1", underdeterminedException.ParamName);
                Assert.Equal("src2", rhsRowsException.ParamName);
            }
        }

        [Fact]
        public void SolveAllowsOverdeterminedSourceWithQrAndNormalLu()
        {
            using (var coefficients = new Mat(3, 2, MatType.CV_64FC1))
            using (var rhs = new Mat(3, 1, MatType.CV_64FC1))
            using (var qrSolution = new Mat())
            using (var normalSolution = new Mat())
            {
                coefficients.CopyFrom<double>(new[] { 1.0, 0.0, 0.0, 1.0, 1.0, 1.0 });
                rhs.CopyFrom<double>(new[] { 1.0, 2.0, 3.0 });

                bool qrSolved = Cv2.Solve(coefficients, rhs, qrSolution, DecompTypes.QR);
                bool normalSolved = Cv2.Solve(coefficients, rhs, normalSolution, DecompTypes.Normal | DecompTypes.LU);

                Assert.True(qrSolved);
                Assert.True(normalSolved);
                Assert.Equal(2, qrSolution.Rows);
                Assert.Equal(1, qrSolution.Cols);
                Assert.Equal(MatType.CV_64FC1, qrSolution.Type);
                Assert.Equal(2, normalSolution.Rows);
                Assert.Equal(1, normalSolution.Cols);
                Assert.Equal(MatType.CV_64FC1, normalSolution.Type);
            }
        }

        [Fact]
        public void MixChannelsRejectsOddLengthChannelMapping()
        {
            using (var src = new Mat(1, 1, MatType.CV_8UC3))
            using (var dst = new Mat(1, 1, MatType.CV_8UC3))
            {
                ArgumentException exception = Assert.Throws<ArgumentException>(() =>
                    Cv2.MixChannels(new[] { src }, new[] { dst }, new[] { 0, 0, 1 }));

                Assert.Equal("fromTo", exception.ParamName);
            }
        }

        [Fact]
        public void MixChannelsRejectsInvalidSourceDestinationContract()
        {
            using (var src = new Mat(2, 2, MatType.CV_8UC3))
            using (var secondSource = new Mat(2, 2, MatType.CV_8UC1))
            using (var emptySource = new Mat())
            using (var sourceSizeMismatch = new Mat(1, 2, MatType.CV_8UC1))
            using (var sourceDepthMismatch = new Mat(2, 2, MatType.CV_32FC1))
            using (var dst = new Mat(2, 2, MatType.CV_8UC3))
            using (var emptyDestination = new Mat())
            using (var destinationSizeMismatch = new Mat(1, 2, MatType.CV_8UC3))
            using (var destinationDepthMismatch = new Mat(2, 2, MatType.CV_32FC3))
            {
                ArgumentException emptySourceException = Assert.Throws<ArgumentException>(() =>
                    Cv2.MixChannels(new[] { emptySource }, new[] { dst }, new[] { 0, 0 }));
                ArgumentException sourceSizeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.MixChannels(new[] { src, sourceSizeMismatch }, new[] { dst }, new[] { 0, 0 }));
                ArgumentException sourceDepthException = Assert.Throws<ArgumentException>(() =>
                    Cv2.MixChannels(new[] { src, sourceDepthMismatch }, new[] { dst }, new[] { 0, 0 }));
                ArgumentException emptyDestinationException = Assert.Throws<ArgumentException>(() =>
                    Cv2.MixChannels(new[] { src }, new[] { emptyDestination }, new[] { 0, 0 }));
                ArgumentException destinationSizeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.MixChannels(new[] { src }, new[] { destinationSizeMismatch }, new[] { 0, 0 }));
                ArgumentException destinationDepthException = Assert.Throws<ArgumentException>(() =>
                    Cv2.MixChannels(new[] { src }, new[] { destinationDepthMismatch }, new[] { 0, 0 }));
                ArgumentOutOfRangeException sourceIndexException = Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Cv2.MixChannels(new[] { src }, new[] { dst }, new[] { 3, 0 }));
                ArgumentOutOfRangeException destinationNegativeIndexException = Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Cv2.MixChannels(new[] { src }, new[] { dst }, new[] { 0, -1 }));
                ArgumentOutOfRangeException destinationIndexException = Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Cv2.MixChannels(new[] { src }, new[] { dst }, new[] { 0, 3 }));

                Cv2.MixChannels(new[] { src, secondSource }, new[] { dst }, new[] { 3, 2, -1, 1 });

                Assert.Equal("src", emptySourceException.ParamName);
                Assert.Equal("src", sourceSizeException.ParamName);
                Assert.Equal("src", sourceDepthException.ParamName);
                Assert.Equal("dst", emptyDestinationException.ParamName);
                Assert.Equal("dst", destinationSizeException.ParamName);
                Assert.Equal("dst", destinationDepthException.ParamName);
                Assert.Equal("fromTo", sourceIndexException.ParamName);
                Assert.Equal("fromTo", destinationNegativeIndexException.ParamName);
                Assert.Equal("fromTo", destinationIndexException.ParamName);
            }
        }

        [Fact]
        public void CountNonZeroRejectsMultiChannelSource()
        {
            using (var src = new Mat(1, 1, MatType.CV_8UC3))
            {
                ArgumentException exception = Assert.Throws<ArgumentException>(() =>
                    Cv2.CountNonZero(src));

                Assert.Equal("src", exception.ParamName);
            }
        }

        [Fact]
        public void SumRejectsMoreThanFourChannels()
        {
            using (var src = new Mat(1, 1, MatType.MakeType(MatType.CV_8U, 5)))
            {
                ArgumentException exception = Assert.Throws<ArgumentException>(() =>
                    Cv2.Sum(src));

                Assert.Equal("src", exception.ParamName);
            }
        }

        [Fact]
        public void MeanRejectsInvalidSourceAndMaskContract()
        {
            using (var src = new Mat(2, 2, MatType.CV_8UC1))
            using (var fiveChannelSource = new Mat(1, 1, MatType.MakeType(MatType.CV_8U, 5)))
            using (var colorMask = new Mat(2, 2, MatType.CV_8UC3))
            using (var smallMask = new Mat(1, 2, MatType.CV_8UC1))
            {
                ArgumentException channelException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Mean(fiveChannelSource));
                ArgumentException maskTypeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Mean(src, colorMask));
                ArgumentException maskSizeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Mean(src, smallMask));

                Assert.Equal("src", channelException.ParamName);
                Assert.Equal("mask", maskTypeException.ParamName);
                Assert.Equal("mask", maskSizeException.ParamName);
            }
        }

        [Fact]
        public void MeanStdDevRejectsInvalidSourceAndMaskContract()
        {
            using (var src = new Mat(2, 2, MatType.CV_8UC1))
            using (var colorMask = new Mat(2, 2, MatType.CV_8UC3))
            using (var smallMask = new Mat(1, 2, MatType.CV_8UC1))
            {
                ArgumentException emptySourceException = Assert.Throws<ArgumentException>(() =>
                    Cv2.MeanStdDev(new Mat()));
                ArgumentException maskTypeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.MeanStdDev(src, colorMask));
                ArgumentException maskSizeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.MeanStdDev(src, smallMask));

                Assert.Equal("src", emptySourceException.ParamName);
                Assert.Equal("mask", maskTypeException.ParamName);
                Assert.Equal("mask", maskSizeException.ParamName);
            }
        }

        [Fact]
        public void MinMaxLocRejectsInvalidSourceAndMaskContract()
        {
            using (var src = new Mat(2, 2, MatType.CV_8UC1))
            using (var colorSource = new Mat(2, 2, MatType.CV_8UC3))
            using (var colorMask = new Mat(2, 2, MatType.CV_8UC3))
            using (var smallMask = new Mat(1, 2, MatType.CV_8UC1))
            {
                ArgumentException channelException = Assert.Throws<ArgumentException>(() =>
                    Cv2.MinMaxLoc(colorSource));
                ArgumentException maskTypeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.MinMaxLoc(src, colorMask));
                ArgumentException maskSizeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.MinMaxLoc(src, smallMask));

                Assert.Equal("src", channelException.ParamName);
                Assert.Equal("mask", maskTypeException.ParamName);
                Assert.Equal("mask", maskSizeException.ParamName);
            }
        }

        [Fact]
        public void NormRejectsMismatchedInputTypeAndSize()
        {
            using (var src1 = new Mat(2, 2, MatType.CV_8UC1))
            using (var typeMismatch = new Mat(2, 2, MatType.CV_32FC1))
            using (var sizeMismatch = new Mat(1, 2, MatType.CV_8UC1))
            {
                ArgumentException typeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Norm(src1, typeMismatch));
                ArgumentException sizeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Norm(src1, sizeMismatch));

                Assert.Equal("src2", typeException.ParamName);
                Assert.Equal("src2", sizeException.ParamName);
            }
        }

        [Fact]
        public void NormRejectsInvalidTypeAndMaskContract()
        {
            using (var src8U = new Mat(2, 2, MatType.CV_8UC1))
            using (var other8U = new Mat(2, 2, MatType.CV_8UC1))
            using (var src32F = new Mat(2, 2, MatType.CV_32FC1))
            using (var other32F = new Mat(2, 2, MatType.CV_32FC1))
            using (var colorMask = new Mat(2, 2, MatType.CV_8UC3))
            using (var smallMask = new Mat(1, 2, MatType.CV_8UC1))
            {
                ArgumentOutOfRangeException singleMinMaxException = Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Cv2.Norm(src8U, NormTypes.MinMax));
                ArgumentOutOfRangeException singleUnknownException = Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Cv2.Norm(src8U, (NormTypes)99));
                ArgumentException singleHammingTypeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Norm(src32F, NormTypes.Hamming));
                ArgumentException singleMaskTypeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Norm(src8U, NormTypes.L2, colorMask));
                ArgumentException singleMaskSizeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Norm(src8U, NormTypes.L2, smallMask));
                ArgumentOutOfRangeException diffMinMaxException = Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Cv2.Norm(src8U, other8U, NormTypes.MinMax));
                ArgumentException diffHammingTypeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Norm(src32F, other32F, NormTypes.Hamming));
                ArgumentException diffMaskTypeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Norm(src8U, other8U, NormTypes.L2, colorMask));
                ArgumentException diffMaskSizeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Norm(src8U, other8U, NormTypes.L2, smallMask));

                Assert.Equal("normType", singleMinMaxException.ParamName);
                Assert.Equal("normType", singleUnknownException.ParamName);
                Assert.Equal("src1", singleHammingTypeException.ParamName);
                Assert.Equal("mask", singleMaskTypeException.ParamName);
                Assert.Equal("mask", singleMaskSizeException.ParamName);
                Assert.Equal("normType", diffMinMaxException.ParamName);
                Assert.Equal("src1", diffHammingTypeException.ParamName);
                Assert.Equal("mask", diffMaskTypeException.ParamName);
                Assert.Equal("mask", diffMaskSizeException.ParamName);
            }
        }

        [Fact]
        public void DeterminantRejectsInvalidInputContract()
        {
            using (var empty = new Mat())
            using (var nonSquare = new Mat(2, 3, MatType.CV_64FC1))
            using (var unsupportedType = new Mat(2, 2, MatType.CV_8UC1))
            {
                ArgumentException emptyException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Determinant(empty));
                ArgumentException shapeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Determinant(nonSquare));
                ArgumentException typeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Determinant(unsupportedType));

                Assert.Equal("src", emptyException.ParamName);
                Assert.Equal("src", shapeException.ParamName);
                Assert.Equal("src", typeException.ParamName);
            }
        }

        [Fact]
        public void MahalanobisRejectsInvalidInputContract()
        {
            using (var v1 = new Mat(2, 1, MatType.CV_64FC1))
            using (var typeMismatchVector = new Mat(2, 1, MatType.CV_32FC1))
            using (var sizeMismatchVector = new Mat(3, 1, MatType.CV_64FC1))
            using (var typeMismatchCovariance = new Mat(2, 2, MatType.CV_32FC1))
            using (var shapeMismatchCovariance = new Mat(3, 3, MatType.CV_64FC1))
            {
                ArgumentException vectorTypeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Mahalanobis(v1, typeMismatchVector, shapeMismatchCovariance));
                ArgumentException vectorSizeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Mahalanobis(v1, sizeMismatchVector, shapeMismatchCovariance));
                ArgumentException covarianceTypeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Mahalanobis(v1, v1, typeMismatchCovariance));
                ArgumentException covarianceShapeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Mahalanobis(v1, v1, shapeMismatchCovariance));

                Assert.Equal("v2", vectorTypeException.ParamName);
                Assert.Equal("v2", vectorSizeException.ParamName);
                Assert.Equal("icovar", covarianceTypeException.ParamName);
                Assert.Equal("icovar", covarianceShapeException.ParamName);
            }
        }

        [Fact]
        public void ExtractChannelRejectsChannelIndexOutsideSourceRange()
        {
            using (var src = new Mat(1, 1, MatType.CV_8UC3))
            using (var dst = new Mat())
            {
                ArgumentOutOfRangeException negativeException = Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Cv2.ExtractChannel(src, dst, -1));
                ArgumentOutOfRangeException equalChannelCountException = Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Cv2.ExtractChannel(src, dst, 3));
                ArgumentOutOfRangeException returnedMatException = Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Cv2.ExtractChannel(src, 3));

                Assert.Equal("coi", negativeException.ParamName);
                Assert.Equal("coi", equalChannelCountException.ParamName);
                Assert.Equal("coi", returnedMatException.ParamName);
            }
        }

        [Fact]
        public void InsertChannelRejectsInvalidSourceDestinationContract()
        {
            using (var gray = new Mat(2, 2, MatType.CV_8UC1))
            using (var color = new Mat(2, 2, MatType.CV_8UC3))
            using (var multiChannelSource = new Mat(2, 2, MatType.CV_8UC3))
            using (var sizeMismatch = new Mat(1, 2, MatType.CV_8UC3))
            using (var depthMismatch = new Mat(2, 2, MatType.CV_32FC3))
            {
                ArgumentException sizeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.InsertChannel(gray, sizeMismatch, 0));
                ArgumentException depthException = Assert.Throws<ArgumentException>(() =>
                    Cv2.InsertChannel(gray, depthMismatch, 0));
                ArgumentException channelException = Assert.Throws<ArgumentException>(() =>
                    Cv2.InsertChannel(multiChannelSource, color, 0));
                ArgumentOutOfRangeException negativeException = Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Cv2.InsertChannel(gray, color, -1));
                ArgumentOutOfRangeException equalChannelCountException = Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Cv2.InsertChannel(gray, color, 3));

                Assert.Equal("dst", sizeException.ParamName);
                Assert.Equal("dst", depthException.ParamName);
                Assert.Equal("src", channelException.ParamName);
                Assert.Equal("coi", negativeException.ParamName);
                Assert.Equal("coi", equalChannelCountException.ParamName);
            }
        }

        [Fact]
        public void LutRejectsInvalidLookupTableContract()
        {
            using (var gray = new Mat(1, 1, MatType.CV_8UC1))
            using (var color = new Mat(1, 1, MatType.CV_8UC3))
            using (var singleChannelLut = new Mat(1, 256, MatType.CV_8UC1))
            using (var twoChannelLut = new Mat(1, 256, MatType.CV_8UC2))
            using (var shortLut = new Mat(1, 128, MatType.CV_8UC1))
            using (var wideLut = new Mat(2, 256, MatType.CV_8UC1))
            using (var nonContinuousLut = wideLut.ColRange(0, 128))
            using (var dst = new Mat())
            {
                ArgumentException channelException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Lut(color, twoChannelLut, dst));
                ArgumentException lengthException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Lut(gray, shortLut, dst));
                ArgumentException continuousException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Lut(gray, nonContinuousLut, dst));
                ArgumentException returnedMatException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Lut(color, twoChannelLut));

                Assert.Equal("lut", channelException.ParamName);
                Assert.Equal("lut", lengthException.ParamName);
                Assert.Equal("lut", continuousException.ParamName);
                Assert.Equal("lut", returnedMatException.ParamName);
            }
        }

        [Fact]
        public void CompleteSymmRejectsInvalidMatrixContract()
        {
            using (var nonSquare = new Mat(2, 3, MatType.CV_64FC1))
            using (var image = new Mat(4, 4, MatType.CV_8UC3, new Scalar(1, 2, 3)))
            using (Mat blob = OpenCvSharp.Dnn.Cv2.BlobFromImage(image, 1.0, new Size(4, 4)))
            {
                Assert.Equal(4, blob.Dims);

                ArgumentException dimsException = Assert.Throws<ArgumentException>(() =>
                    Cv2.CompleteSymm(blob));
                ArgumentException squareException = Assert.Throws<ArgumentException>(() =>
                    Cv2.CompleteSymm(nonSquare));

                Assert.Equal("mat", dimsException.ParamName);
                Assert.Equal("mat", squareException.ParamName);
            }
        }

        [Fact]
        public void PatchNaNsRejectsUnsupportedSourceDepth()
        {
            using (var integer = new Mat(2, 2, MatType.CV_8UC1))
            {
                ArgumentException exception = Assert.Throws<ArgumentException>(() =>
                    Cv2.PatchNaNs(integer));

                Assert.Equal("src", exception.ParamName);
            }
        }

        [Fact]
        public void SetIdentityRejectsMatrixWithMoreThanTwoDimensions()
        {
            using (var image = new Mat(4, 4, MatType.CV_8UC3, new Scalar(1, 2, 3)))
            using (Mat blob = OpenCvSharp.Dnn.Cv2.BlobFromImage(image, 1.0, new Size(4, 4)))
            {
                Assert.Equal(4, blob.Dims);

                ArgumentException exception = Assert.Throws<ArgumentException>(() =>
                    Cv2.SetIdentity(blob));

                Assert.Equal("mat", exception.ParamName);
            }
        }

        private static byte[] CreateIdentityLut()
        {
            byte[] values = new byte[256];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = (byte)i;
            }

            return values;
        }

    }
}

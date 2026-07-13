using System;
using OpenCvSharp.Core;

namespace OpenCvSharp.Tests.Core
{
    public class SvdTests
    {
        [Fact]
        public void SvdObjectComputesValuesAndBackSubstitutionWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat matrix = new Mat(2, 2, MatType.CV_64FC1))
            using (Mat rhs = new Mat(2, 1, MatType.CV_64FC1))
            using (Svd svd = new Svd(matrix))
            using (Mat solution = new Mat())
            {
                matrix.CopyFrom<double>(new double[] { 1.0, 0.0, 0.0, 2.0 });
                rhs.CopyFrom<double>(new double[] { 3.0, 8.0 });

                svd.Compute(matrix);

                using (Mat w = svd.W)
                using (Mat u = svd.U)
                using (Mat vt = svd.Vt)
                {
                    Assert.False(w.Empty);
                    Assert.False(u.Empty);
                    Assert.False(vt.Empty);
                    Assert.Equal("{Disposed=False}", svd.ToString());
                }

                svd.BackSubst(rhs, solution);
                using (Mat returnedSolution = svd.BackSubst(rhs))
                {
                    Assert.False(returnedSolution.Empty);
                    double[] returnedValues = returnedSolution.ToArray<double>();
                    Assert.Equal(3.0, returnedValues[0], 6);
                    Assert.Equal(4.0, returnedValues[1], 6);
                }

                double[] values = solution.ToArray<double>();
                Assert.Equal(3.0, values[0], 6);
                Assert.Equal(4.0, values[1], 6);
            }
        }

        [Fact]
        public void SvdStaticComputeAndSolveZWorkWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat matrix = new Mat(2, 3, MatType.CV_64FC1))
            using (Mat w = new Mat())
            using (Mat u = new Mat())
            using (Mat vt = new Mat())
            using (Mat z = new Mat())
            {
                matrix.CopyFrom<double>(new double[]
                {
                    1.0, 0.0, 0.0,
                    0.0, 1.0, 0.0
                });

                Svd.Compute(matrix, w, u, vt, SvdFlags.FullUv);
                Svd.SolveZ(matrix, z);

                Assert.False(w.Empty);
                Assert.False(u.Empty);
                Assert.False(vt.Empty);
                Assert.Equal(3, z.ValueCount);
                Assert.True(Math.Abs(Cv2.Norm(z, NormTypes.L2) - 1.0) < 1e-6);

                using (Mat valuesOnly = Svd.ComputeValues(matrix))
                using (Mat returnedZ = Svd.SolveZ(matrix))
                {
                    Assert.False(valuesOnly.Empty);
                    Assert.Equal(3, returnedZ.ValueCount);
                    Assert.True(Math.Abs(Cv2.Norm(returnedZ, NormTypes.L2) - 1.0) < 1e-6);
                }
            }
        }

        [Fact]
        public void SvdReturningHelpersValidateManagedArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new Svd(null!));
            Assert.Throws<ArgumentNullException>(() => Svd.Compute(null!, new Mat(), new Mat(), new Mat()));
            Assert.Throws<ArgumentNullException>(() => Svd.Compute(new Mat(), null!, new Mat(), new Mat()));
            Assert.Throws<ArgumentNullException>(() => Svd.Compute(new Mat(), new Mat(), null!, new Mat()));
            Assert.Throws<ArgumentNullException>(() => Svd.Compute(new Mat(), new Mat(), new Mat(), null!));
            Assert.Throws<ArgumentNullException>(() => Svd.Compute(null!, new Mat()));
            Assert.Throws<ArgumentNullException>(() => Svd.Compute(new Mat(), null!));
            Assert.Throws<ArgumentNullException>(() => Svd.ComputeValues(null!));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Svd(new Mat(), (SvdFlags)8));
            Assert.Throws<ArgumentOutOfRangeException>(() => Svd.Compute(new Mat(), new Mat(), new Mat(), new Mat(), (SvdFlags)8));
            Assert.Throws<ArgumentOutOfRangeException>(() => Svd.Compute(new Mat(), new Mat(), (SvdFlags)8));
            Assert.Throws<ArgumentOutOfRangeException>(() => Svd.ComputeValues(new Mat(), (SvdFlags)8));
            Assert.Throws<ArgumentNullException>(() => Svd.BackSubst(null!, new Mat(), new Mat(), new Mat(), new Mat()));
            Assert.Throws<ArgumentNullException>(() => Svd.BackSubst(new Mat(), null!, new Mat(), new Mat(), new Mat()));
            Assert.Throws<ArgumentNullException>(() => Svd.BackSubst(new Mat(), new Mat(), null!, new Mat(), new Mat()));
            Assert.Throws<ArgumentNullException>(() => Svd.BackSubst(new Mat(), new Mat(), new Mat(), null!, new Mat()));
            Assert.Throws<ArgumentNullException>(() => Svd.BackSubst(new Mat(), new Mat(), new Mat(), new Mat(), null!));
            Assert.Throws<ArgumentNullException>(() => Svd.SolveZ(null!, new Mat()));
            Assert.Throws<ArgumentNullException>(() => Svd.SolveZ(new Mat(), null!));
            Assert.Throws<ArgumentNullException>(() => Svd.SolveZ(null!));

            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var svd = new Svd())
            {
                Assert.Throws<ArgumentNullException>(() => svd.Compute(null!));
                Assert.Throws<ArgumentOutOfRangeException>(() => svd.Compute(new Mat(), (SvdFlags)8));
                Assert.Throws<ArgumentNullException>(() => svd.BackSubst(null!, new Mat()));
                Assert.Throws<ArgumentNullException>(() => svd.BackSubst(new Mat(), null!));
                Assert.Throws<ArgumentNullException>(() => svd.BackSubst(null!));
            }
        }

        [Fact]
        public void SvdDisposedObjectRejectsCalls()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat matrix = new Mat(2, 2, MatType.CV_64FC1))
            {
                matrix.CopyFrom<double>(new double[] { 1.0, 0.0, 0.0, 1.0 });
                var svd = new Svd(matrix);
                svd.Dispose();

                Assert.True(svd.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => svd.Compute(matrix));
                Assert.Throws<ObjectDisposedException>(() => svd.BackSubst(matrix, new Mat()));
                Assert.Throws<ObjectDisposedException>(() => svd.BackSubst(matrix));
                Assert.Throws<ObjectDisposedException>(() => svd.U.Dispose());
                Assert.Throws<ObjectDisposedException>(() => svd.Vt.Dispose());
                Assert.Throws<ObjectDisposedException>(() => svd.W.Dispose());
                Assert.Equal("{Disposed=True}", svd.ToString());
            }
        }

    }
}

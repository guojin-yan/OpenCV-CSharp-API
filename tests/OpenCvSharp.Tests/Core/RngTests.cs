using System;
using OpenCvSharp.Core;

namespace OpenCvSharp.Tests.Core
{
    public class RngTests
    {
        [Fact]
        public void RngStateAndScalarGenerationWorkWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Rng rng = new Rng(123UL))
            {
                ulong initialState = rng.State;
                uint first = rng.Next();
                rng.State = initialState;
                uint repeated = rng.Next();

                Assert.Equal(first, repeated);
                Assert.InRange(rng.Uniform(1, 10), 1, 9);
                Assert.InRange(rng.Uniform(0.0f, 1.0f), 0.0f, 1.0f);
                Assert.InRange(rng.Uniform(0.0, 1.0), 0.0, 1.0);
                Assert.True(Math.Abs(rng.Gaussian(1.0)) < 10.0);
                Assert.Equal("{State=" + rng.State + "}", rng.ToString());
            }
        }

        [Fact]
        public void RngFillUniformAndNormalWorkWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Rng rng = new Rng(42UL))
            using (Mat uniform = new Mat(2, 3, MatType.CV_32SC1))
            using (Mat normal = new Mat(2, 3, MatType.CV_64FC1))
            {
                rng.FillUniform(uniform, new Scalar(0), new Scalar(10));
                int[] values = uniform.ToArray<int>();
                for (int i = 0; i < values.Length; i++)
                {
                    Assert.InRange(values[i], 0, 9);
                }

                rng.FillNormal(normal, new Scalar(0.0), new Scalar(1.0));
                Assert.Equal(6, normal.ToArray<double>().Length);
            }
        }

        [Fact]
        public void RngDisposedObjectRejectsCalls()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat mat = new Mat(2, 2, MatType.CV_32SC1))
            {
                var rng = new Rng(9UL);
                rng.Dispose();

                Assert.True(rng.IsDisposed);
                Assert.Equal("{Disposed=True}", rng.ToString());
                Assert.Throws<ObjectDisposedException>(() => rng.State);
                Assert.Throws<ObjectDisposedException>(() => rng.State = 123UL);
                Assert.Throws<ObjectDisposedException>(() => rng.Next());
                Assert.Throws<ObjectDisposedException>(() => rng.Uniform(1, 10));
                Assert.Throws<ObjectDisposedException>(() => rng.Uniform(0.0F, 1.0F));
                Assert.Throws<ObjectDisposedException>(() => rng.Uniform(0.0, 1.0));
                Assert.Throws<ObjectDisposedException>(() => rng.Gaussian(1.0));
                Assert.Throws<ObjectDisposedException>(() => rng.Fill(mat, RngDistributionTypes.Uniform, new Scalar(0), new Scalar(1)));
                Assert.Throws<ObjectDisposedException>(() => rng.FillUniform(mat, new Scalar(0), new Scalar(1)));
                Assert.Throws<ObjectDisposedException>(() => rng.FillNormal(mat, new Scalar(0), new Scalar(1)));
            }
        }

        [Fact]
        public void RngFillValidatesManagedArguments()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            var rng = new Rng(9UL);
            try
            {
                Assert.Throws<ArgumentNullException>(() => rng.Fill(null!, RngDistributionTypes.Uniform, new Scalar(0), new Scalar(1)));
                Assert.Throws<ArgumentNullException>(() => rng.FillUniform(null!, new Scalar(0), new Scalar(1)));
                Assert.Throws<ArgumentNullException>(() => rng.FillNormal(null!, new Scalar(0), new Scalar(1)));
            }
            finally
            {
                rng.Dispose();
            }
        }

    }
}

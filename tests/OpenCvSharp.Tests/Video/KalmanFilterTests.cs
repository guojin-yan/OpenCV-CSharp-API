using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Video;

namespace JYPPX.OpenCvSharp.Tests.Video
{
    public sealed class KalmanFilterTests
    {
        [Fact]
        public void KalmanMatrixEnumMatchesNativeIds()
        {
            Assert.Equal(0, (int)KalmanFilterMatrix.StatePre);
            Assert.Equal(4, (int)KalmanFilterMatrix.MeasurementMatrix);
            Assert.Equal(9, (int)KalmanFilterMatrix.ErrorCovPost);
        }

        [Fact]
        public void KalmanFilterValidatesDisposedStateWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var measurement = new Mat(1, 1, MatType.CV_32FC1, new Scalar(1.0)))
            using (var output = new Mat())
            using (var liveFilter = new KalmanFilter(2, 1))
            {
                Assert.Throws<ArgumentNullException>(() => liveFilter.Predict(null!, control: null));
                Assert.Throws<ArgumentNullException>(() => liveFilter.Correct(null!, output));
                Assert.Throws<ArgumentNullException>(() => liveFilter.Correct(measurement, null!));
                Assert.Throws<ArgumentNullException>(() => liveFilter.Correct(null!));
                Assert.Throws<ArgumentNullException>(() => liveFilter.GetMatrix(KalmanFilterMatrix.StatePre, null!));
                Assert.Throws<ArgumentNullException>(() => liveFilter.SetMatrix(KalmanFilterMatrix.StatePre, null!));
            }

            var filter = new KalmanFilter(2, 1);
            filter.Dispose();

            Assert.True(filter.IsDisposed);
            Assert.Throws<ObjectDisposedException>(() => filter.Init(2, 1));
            Assert.Throws<ObjectDisposedException>(() => filter.Predict(new Mat()));
            Assert.Throws<ObjectDisposedException>(() => filter.Predict(new Mat(), new Mat()));
            Assert.Throws<ObjectDisposedException>(() => filter.Predict());
            Assert.Throws<ObjectDisposedException>(() => filter.Correct(new Mat(1, 1, MatType.CV_32FC1)));
            Assert.Throws<ObjectDisposedException>(() => filter.Correct(new Mat(1, 1, MatType.CV_32FC1), new Mat()));
            Assert.Throws<ObjectDisposedException>(() => filter.GetMatrix(KalmanFilterMatrix.StatePre, new Mat()));
            Assert.Throws<ObjectDisposedException>(() => filter.GetMatrix(KalmanFilterMatrix.StatePre));
            Assert.Throws<ObjectDisposedException>(() => filter.SetMatrix(KalmanFilterMatrix.StatePre, new Mat()));
            Assert.Throws<ObjectDisposedException>(() => filter.StatePre);
            Assert.Throws<ObjectDisposedException>(() => filter.StatePre = new Mat());
            Assert.Throws<ObjectDisposedException>(() => filter.StatePost);
            Assert.Throws<ObjectDisposedException>(() => filter.StatePost = new Mat());
            Assert.Throws<ObjectDisposedException>(() => filter.TransitionMatrix);
            Assert.Throws<ObjectDisposedException>(() => filter.TransitionMatrix = new Mat());
            Assert.Throws<ObjectDisposedException>(() => filter.ControlMatrix);
            Assert.Throws<ObjectDisposedException>(() => filter.ControlMatrix = new Mat());
            Assert.Throws<ObjectDisposedException>(() => filter.MeasurementMatrix);
            Assert.Throws<ObjectDisposedException>(() => filter.MeasurementMatrix = new Mat());
            Assert.Throws<ObjectDisposedException>(() => filter.ProcessNoiseCov);
            Assert.Throws<ObjectDisposedException>(() => filter.ProcessNoiseCov = new Mat());
            Assert.Throws<ObjectDisposedException>(() => filter.MeasurementNoiseCov);
            Assert.Throws<ObjectDisposedException>(() => filter.MeasurementNoiseCov = new Mat());
            Assert.Throws<ObjectDisposedException>(() => filter.ErrorCovPre);
            Assert.Throws<ObjectDisposedException>(() => filter.ErrorCovPre = new Mat());
            Assert.Throws<ObjectDisposedException>(() => filter.Gain);
            Assert.Throws<ObjectDisposedException>(() => filter.Gain = new Mat());
            Assert.Throws<ObjectDisposedException>(() => filter.ErrorCovPost);
            Assert.Throws<ObjectDisposedException>(() => filter.ErrorCovPost = new Mat());
        }

        [Fact]
        public void KalmanFilterPredictsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var filter = new KalmanFilter(2, 1))
            using (var prediction = filter.Predict())
            {
                Assert.Equal(2, prediction.Rows);
                Assert.Equal(1, prediction.Cols);
            }
        }

        [Fact]
        public void KalmanFilterMatricesRoundTripWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var filter = new KalmanFilter(2, 1, 1))
            using (var statePre = new Mat(2, 1, MatType.CV_32FC1, new Scalar(1.0)))
            using (var statePost = new Mat(2, 1, MatType.CV_32FC1, new Scalar(2.0)))
            using (var transition = new Mat(2, 2, MatType.CV_32FC1, new Scalar(3.0)))
            using (var control = new Mat(2, 1, MatType.CV_32FC1, new Scalar(4.0)))
            using (var measurement = new Mat(1, 2, MatType.CV_32FC1, new Scalar(5.0)))
            using (var processNoise = new Mat(2, 2, MatType.CV_32FC1, new Scalar(6.0)))
            using (var measurementNoise = new Mat(1, 1, MatType.CV_32FC1, new Scalar(7.0)))
            using (var errorPre = new Mat(2, 2, MatType.CV_32FC1, new Scalar(8.0)))
            using (var gain = new Mat(2, 1, MatType.CV_32FC1, new Scalar(9.0)))
            using (var errorPost = new Mat(2, 2, MatType.CV_32FC1, new Scalar(10.0)))
            {
                filter.StatePre = statePre;
                filter.StatePost = statePost;
                filter.TransitionMatrix = transition;
                filter.ControlMatrix = control;
                filter.MeasurementMatrix = measurement;
                filter.ProcessNoiseCov = processNoise;
                filter.MeasurementNoiseCov = measurementNoise;
                filter.ErrorCovPre = errorPre;
                filter.Gain = gain;
                filter.ErrorCovPost = errorPost;

                AssertMatrixEquals(statePre, filter.StatePre);
                AssertMatrixEquals(statePost, filter.StatePost);
                AssertMatrixEquals(transition, filter.TransitionMatrix);
                AssertMatrixEquals(control, filter.ControlMatrix);
                AssertMatrixEquals(measurement, filter.MeasurementMatrix);
                AssertMatrixEquals(processNoise, filter.ProcessNoiseCov);
                AssertMatrixEquals(measurementNoise, filter.MeasurementNoiseCov);
                AssertMatrixEquals(errorPre, filter.ErrorCovPre);
                AssertMatrixEquals(gain, filter.Gain);
                AssertMatrixEquals(errorPost, filter.ErrorCovPost);
            }
        }

        private static void AssertMatrixEquals(Mat expected, Mat actual)
        {
            using (actual)
            {
                Assert.Equal(expected.Rows, actual.Rows);
                Assert.Equal(expected.Cols, actual.Cols);
                Assert.Equal(expected.Type, actual.Type);
                Assert.Equal(expected.ToArray<float>(), actual.ToArray<float>());
            }
        }

    }
}

using System;
using OpenCvSharp.Core;
using OpenCvSharp.Dnn;

namespace OpenCvSharp.Tests.Dnn
{
    public sealed class DnnNetAdvancedTests
    {
        [Fact]
        public void DnnPerfProfileStoresValues()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new DnnPerfProfile(-1, Array.Empty<double>()));

            var timings = new[] { 1.0, 2.0 };
            var profile = new DnnPerfProfile(12, timings);
            timings[0] = 9.0;

            Assert.Equal(12, profile.TickCount);
            Assert.Equal(2, profile.LayerCount);
            Assert.Equal(2, profile.LayerTimings.Length);
            Assert.Equal(new[] { 1.0, 2.0 }, profile.LayerTimings);

            double[] profileTimings = profile.LayerTimings;
            profileTimings[1] = 8.0;
            Assert.Equal(new[] { 1.0, 2.0 }, profile.LayerTimings);

            Assert.Equal(new DnnPerfProfile(12, new[] { 1.0, 2.0 }), profile);
            Assert.True(profile == new DnnPerfProfile(12, new[] { 1.0, 2.0 }));
            Assert.True(profile != new DnnPerfProfile(12, new[] { 2.0, 1.0 }));
            Assert.Equal(new DnnPerfProfile(12, new[] { 1.0, 2.0 }).GetHashCode(), profile.GetHashCode());
            Assert.Equal("{TickCount=12,LayerTimings=2}", profile.ToString());

            var nullTimings = new DnnPerfProfile(3, null!);
            Assert.Equal(0, nullTimings.LayerCount);
            Assert.Empty(nullTimings.LayerTimings);
            Assert.Equal(new DnnPerfProfile(3, Array.Empty<double>()), nullTimings);

            var defaultProfile = default(DnnPerfProfile);
            Assert.Equal(0, defaultProfile.LayerCount);
            Assert.Empty(defaultProfile.LayerTimings);
            Assert.Equal(new DnnPerfProfile(0, Array.Empty<double>()), defaultProfile);
            Assert.Equal(new DnnPerfProfile(0, Array.Empty<double>()).GetHashCode(), defaultProfile.GetHashCode());
            Assert.Equal("{TickCount=0,LayerTimings=0}", defaultProfile.ToString());
        }

        [Fact]
        public void AdvancedManagedValidationThrows()
        {
            Assert.Throws<ArgumentNullException>(() => Net.ReadNetFromOnnx(null!));
            Assert.Throws<ArgumentNullException>(() => Net.ReadNetFromTensorflow(null!));
            Assert.Throws<ArgumentNullException>(() => Net.ReadNetFromTFLite(null!));
            Assert.Throws<ArgumentNullException>(() => Net.ReadNetFromModelOptimizer(null!));
            Assert.Throws<ArgumentNullException>(() => Net.ReadNet((string)null!));
            Assert.Throws<ArgumentNullException>(() => Net.ReadNet(null!, Array.Empty<byte>()));
            Assert.Throws<ArgumentNullException>(() => Net.ReadNet("onnx", (byte[])null!));

            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var net = Net.CreateEmpty())
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => net.SetPreferableBackend((DnnBackend)99));
                Assert.Throws<ArgumentOutOfRangeException>(() => net.SetPreferableTarget((DnnTarget)99));
                Assert.Throws<ArgumentNullException>(() => net.Forward((string[])null!));
                Assert.Throws<ArgumentException>(() => net.Forward(Array.Empty<string>()));
                RunOrAcceptOpenCvBoundary(() => net.Forward());
                Assert.Throws<ArgumentNullException>(() => net.SetInputsNames(null!));
                Assert.Throws<ArgumentNullException>(() => net.SetInputsNames(new[] { "data", null! }));
                Assert.Throws<ArgumentNullException>(() => net.SetInputShape("data", null!));
                Assert.Throws<ArgumentNullException>(() => net.GetFLOPS(null!));
                Assert.Throws<ArgumentNullException>(() => net.GetLayerFLOPS(0, null!));
                Assert.Throws<ArgumentNullException>(() => net.GetLayerId(null!));
                Assert.Throws<ArgumentNullException>(() => net.GetLayersCountByType(null!));
            }
        }

        [Fact]
        public void EmptyNetMetadataCallsRunWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var net = Net.CreateEmpty())
            {
                Assert.True(net.Empty);
                Assert.Empty(net.GetLayerNames());
                string[] outLayerNames = net.GetUnconnectedOutLayersNames();
                Assert.All(outLayerNames, name => Assert.False(string.IsNullOrWhiteSpace(name)));
                int[] outLayerIds = net.GetUnconnectedOutLayers();
                Assert.All(outLayerIds, id => Assert.True(id >= 0));
                string[] layerTypes = net.GetLayerTypes();
                Assert.All(layerTypes, layerType => Assert.False(string.IsNullOrWhiteSpace(layerType)));
                Assert.Equal(0, net.GetLayersCountByType("Convolution"));
                Assert.Equal(-1, net.GetLayerId("missing"));

                net.SetInputsNames(Array.Empty<string>());

                RunOrAcceptOpenCvBoundary(() => net.SetInputShape(string.Empty, new[] { 1, 3, 4, 4 }));
                RunOrAcceptOpenCvBoundary(() => Assert.True(net.GetFLOPS(new[] { 1, 3, 4, 4 }, MatType.CV_32F) >= 0));
            }
        }

        [Fact]
        public void EmptyNetThrowsAfterDisposeWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var output = new Mat())
            {
                Net net = Net.CreateEmpty();
                net.Dispose();

                Assert.True(net.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => net.Forward(output));
                Assert.Throws<ObjectDisposedException>(() => net.Forward());
                Assert.Throws<ObjectDisposedException>(() => net.Forward(new[] { "prob" }));
                Assert.Throws<ObjectDisposedException>(() => net.SetInput(output));
                Assert.Throws<ObjectDisposedException>(() => net.SetPreferableBackend(DnnBackend.OpenCV));
                Assert.Throws<ObjectDisposedException>(() => net.SetPreferableTarget(DnnTarget.Cpu));
                Assert.Throws<ObjectDisposedException>(() => net.GetUnconnectedOutLayers());
                Assert.Throws<ObjectDisposedException>(() => net.SetInputsNames(Array.Empty<string>()));
                Assert.Throws<ObjectDisposedException>(() => net.SetInputShape("data", new[] { 1, 3, 4, 4 }));
                Assert.Throws<ObjectDisposedException>(() => net.GetFLOPS(new[] { 1, 3, 4, 4 }));
                Assert.Throws<ObjectDisposedException>(() => net.GetLayerFLOPS(0, new[] { 1, 3, 4, 4 }));
                Assert.Throws<ObjectDisposedException>(() => net.GetPerfProfile());
                Assert.Throws<ObjectDisposedException>(() => net.GetLayerNames());
                Assert.Throws<ObjectDisposedException>(() => net.GetUnconnectedOutLayersNames());
                Assert.Throws<ObjectDisposedException>(() => net.GetLayerTypes());
                Assert.Throws<ObjectDisposedException>(() => net.GetLayerId("missing"));
                Assert.Throws<ObjectDisposedException>(() => net.GetLayersCountByType("Convolution"));
            }
        }

        private static void RunOrAcceptOpenCvBoundary(Action action)
        {
            try
            {
                action();
            }
            catch (OpenCvException ex)
            {
                Assert.False(string.IsNullOrWhiteSpace(ex.Message));
            }
        }

    }
}

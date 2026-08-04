using System;
using System.IO;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Dnn;
using DnnCv2 = JYPPX.OpenCvSharp.Dnn.Cv2;

namespace JYPPX.OpenCvSharp.Tests.Dnn
{
    public sealed class DnnStructuredParityTests
    {
        [Fact]
        public void StructuredEnumsAndParametersMatchOpenCvValues()
        {
            Assert.Equal(0, (int)DnnTracingMode.None);
            Assert.Equal(2, (int)DnnTracingMode.Operation);
            Assert.Equal(0, (int)DnnProfilingMode.None);
            Assert.Equal(2, (int)DnnProfilingMode.Detailed);
            Assert.Equal(3, (int)DnnModelFormat.TensorFlowLite);
            Assert.Equal(2, (int)DnnDataLayout.Nchw);
            Assert.Equal(4, (int)DnnDataLayout.Nhwc);
            Assert.Equal(2, (int)DnnImagePaddingMode.Letterbox);

            var parameters = new Image2BlobParams(
                new Scalar(0.5, 0.25, 0.125, 1.0),
                new Size(8, 6),
                new Scalar(1, 2, 3, 4),
                true,
                MatType.CV_32F,
                DnnDataLayout.Nhwc,
                DnnImagePaddingMode.Letterbox,
                new Scalar(9, 8, 7, 6));
            Assert.Equal(new Size(8, 6), parameters.Size);
            Assert.True(parameters.SwapRB);
            Assert.Equal(DnnDataLayout.Nhwc, parameters.DataLayout);
            var mappingParameters = new Image2BlobParams(new Scalar(1), new Size(8, 6), dataLayout: DnnDataLayout.Nhwc);
            Assert.Equal(new Rect(4, 4, 8, 4), mappingParameters.BlobRectToImageRect(new Rect(2, 2, 4, 2), new Size(16, 12)));
            Assert.Equal(
                new[] { new Rect(4, 4, 8, 4), new Rect(0, 0, 4, 4) },
                mappingParameters.BlobRectsToImageRects(new[] { new Rect(2, 2, 4, 2), new Rect(0, 0, 2, 2) }, new Size(16, 12)));

            Assert.Throws<ArgumentOutOfRangeException>(() => new Image2BlobParams(new Scalar(1), new Size(0, 2)));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Image2BlobParams(new Scalar(1), ddepth: MatType.CV_64F));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Image2BlobParams(new Scalar(1), dataLayout: (DnnDataLayout)99));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Image2BlobParams(new Scalar(1), paddingMode: (DnnImagePaddingMode)99));
            Assert.Throws<ArgumentException>(() => new Image2BlobParams(new Scalar(2), ddepth: MatType.CV_8U));
            Assert.Throws<ArgumentNullException>(() => parameters.BlobRectsToImageRects(null!, new Size(8, 6)));
            Assert.Throws<ArgumentOutOfRangeException>(() => parameters.BlobRectToImageRect(new Rect(), new Size(0, 6)));
        }

        [Fact]
        public void StructuredManagedValidationFailsClosed()
        {
            Assert.Throws<ArgumentNullException>(() => Net.ReadNetFromOnnx((byte[])null!));
            Assert.Throws<ArgumentException>(() => Net.ReadNetFromOnnx(Array.Empty<byte>()));
            Assert.Throws<ArgumentNullException>(() => Net.ReadNetFromTensorflow((byte[])null!));
            Assert.Throws<ArgumentException>(() => Net.ReadNetFromTensorflow(Array.Empty<byte>()));
            Assert.Throws<ArgumentNullException>(() => Net.ReadNetFromTFLite((byte[])null!));
            Assert.Throws<ArgumentException>(() => Net.ReadNetFromTFLite(Array.Empty<byte>()));
            Assert.Throws<ArgumentNullException>(() => Net.ReadNetFromModelOptimizer((byte[])null!, new byte[] { 1 }));
            Assert.Throws<ArgumentException>(() => Net.ReadNetFromModelOptimizer(Array.Empty<byte>(), new byte[] { 1 }));
            Assert.Throws<ArgumentException>(() => Net.ReadNetFromOnnx("bad\0path"));
            Assert.Throws<ArgumentOutOfRangeException>(() => DnnCv2.GetAvailableTargets((DnnBackend)99));

            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using (Net net = Net.CreateEmpty())
            using (var value = new Mat())
            {
                Assert.Throws<ArgumentException>(() => net.ForwardAndRetrieve(Array.Empty<string>()));
                Assert.Throws<ArgumentException>(() => net.ForwardAndRetrieve(new[] { "bad\0name" }));
                Assert.Throws<ArgumentException>(() => net.Connect("bad\0pin", "input"));
                Assert.Throws<ArgumentOutOfRangeException>(() => net.GetLayer(-1));
                Assert.Throws<ArgumentOutOfRangeException>(() => net.RegisterOutput("output", -1, 0));
                Assert.Throws<ArgumentOutOfRangeException>(() => net.RegisterOutput("output", 0, -1));
                Assert.Throws<ArgumentOutOfRangeException>(() => net.SetTracingMode((DnnTracingMode)99));
                Assert.Throws<ArgumentOutOfRangeException>(() => net.SetProfilingMode((DnnProfilingMode)99));
                Assert.Throws<ArgumentOutOfRangeException>(() => net.SetParam(-1, 0, value));
                Assert.Throws<ArgumentOutOfRangeException>(() => net.SetParam(0, -1, value));
                Assert.Throws<ArgumentOutOfRangeException>(() => net.GetParam(0, -1));
                Assert.Throws<ArgumentNullException>(() => net.GetLayerShapes(null!, new[] { MatType.CV_32F }, 0));
                Assert.Throws<ArgumentException>(() => net.GetLayerShapes(Array.Empty<int[]>(), Array.Empty<int>(), 0));
                Assert.Throws<ArgumentException>(() => net.GetLayerShapes(new[] { new[] { 1 } }, Array.Empty<int>(), 0));
                Assert.Throws<ArgumentNullException>(() => net.GetLayerShapes(new int[][] { null! }, new[] { MatType.CV_32F }, 0));
                Assert.Throws<ArgumentException>(() => net.GetLayerShapes(new[] { new int[11] }, new[] { MatType.CV_32F }, 0));
                Assert.Throws<ArgumentOutOfRangeException>(() => net.GetFLOPS(new[] { new[] { 1 } }, new[] { -1 }));
                Assert.Throws<ArgumentNullException>(() => net.GetMemoryConsumption(null!, Array.Empty<int>()));
            }
        }

        [Fact]
        public void IdentityOnnxExercisesStructuredNetworkSurface()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            byte[] model = DnnFixture.ReadIdentityOnnx();
            Assert.Equal(147, model.Length);

            string tempRoot = Path.Combine(Path.GetTempPath(), "opencv-csharp-dnn-structured-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempRoot);
            try
            {
                string modelPath = Path.Combine(tempRoot, "identity-model.onnx");
                File.WriteAllBytes(modelPath, model);
                using (Net pathNet = Net.ReadNetFromOnnx(modelPath, DnnEngine.Classic))
                    Assert.Equal(DnnModelFormat.Generic, pathNet.ModelFormat);

                using (Net net = Net.ReadNetFromOnnx(model, DnnEngine.Classic))
                using (var image = new Mat(2, 2, MatType.CV_32FC1))
                {
                    image.CopyFrom(new[] { 1.0F, 2.0F, 3.0F, 4.0F });
                    using (Mat refreshedBlob = DnnCv2.BlobFromImage(image, new Image2BlobParams()))
                    {
                        net.SetPreferableBackend(DnnBackend.OpenCV)
                            .SetPreferableTarget(DnnTarget.Cpu)
                            .SetTracingMode(DnnTracingMode.None)
                            .SetProfilingMode(DnnProfilingMode.Detailed)
                            .EnableFusion(true)
                            .EnableWinograd(true);
                        Assert.Equal(DnnTracingMode.None, net.GetTracingMode());
                        Assert.Equal(DnnProfilingMode.Detailed, net.GetProfilingMode());
                        Assert.Equal(DnnModelFormat.Generic, net.ModelFormat);
                        Assert.Contains(DnnTarget.Cpu, DnnCv2.GetAvailableTargets(DnnBackend.OpenCV));
                        net.SetInput(refreshedBlob, "input");

                        string[] layerNames = net.GetLayerNames();
                        string[] outputNames = net.GetUnconnectedOutLayersNames();
                        Assert.NotEmpty(layerNames);
                        Assert.NotEmpty(outputNames);

                        int layerId = net.GetLayerId(layerNames[0]);
                        Assert.True(layerId >= 0);
                        Assert.True(net.RegisterOutput("registered_identity", layerId, 0) >= 0);
                        Assert.Contains("identity", net.Dump(), StringComparison.OrdinalIgnoreCase);
                        string dumpPath = Path.Combine(tempRoot, "network-dump.txt");
                        string pbtxtPath = Path.Combine(tempRoot, "network-dump.pbtxt");
                        net.DumpToFile(dumpPath);
                        net.DumpToPbtxt(pbtxtPath);
                        Assert.True(new FileInfo(dumpPath).Length > 0);
                        Assert.True(new FileInfo(pbtxtPath).Length > 0);
                        using (Layer byId = net.GetLayer(layerId))
                        using (Layer byName = net.GetLayer(layerNames[0]))
                        {
                            Assert.False(byId.IsDisposed);
                            Assert.False(byName.IsDisposed);
                        }
                        Assert.Throws<OpenCvException>(() => net.GetParam(layerId));
                        Assert.Throws<OpenCvException>(() => net.SetParam(layerId, 0, refreshedBlob));

                        int[][] inputShapes = { new[] { 1, 1, 2, 2 } };
                        int[] inputTypes = { MatType.CV_32F };
                        DnnLayerShapes shapes = net.GetLayerShapes(inputShapes, inputTypes, layerId);
                        Assert.NotEmpty(shapes.InputShapes);
                        Assert.NotEmpty(shapes.OutputShapes);
                        Assert.True(net.GetFLOPS(inputShapes, inputTypes) >= 0);
                        DnnMemoryConsumption memory = net.GetMemoryConsumption(inputShapes, inputTypes);
                        Assert.True(memory.WeightsBytes + memory.BlobBytes > 0);

                        net.FinalizeNetwork();
                        using (Mat output = net.Forward(outputNames[0]))
                        {
                            Assert.Equal((UIntPtr)4U, output.Total);
                            Assert.Equal(new[] { 1.0F, 2.0F, 3.0F, 4.0F }, output.ToArray<float>());
                        }

                        Mat[][] nested = net.ForwardAndRetrieve(outputNames);
                        try
                        {
                            Assert.Equal(outputNames.Length, nested.Length);
                            Assert.All(nested, group => Assert.NotEmpty(group));
                        }
                        finally
                        {
                            Dispose(nested);
                        }

                        DnnPerfProfile profile = net.GetPerfProfile();
                        Assert.True(profile.TickCount >= 0);
                        DnnDetailedPerfProfile detailed = net.GetDetailedPerfProfile();
                        Assert.Equal(detailed.Names.Length, detailed.Times.Length);
                        Assert.Equal(detailed.Names.Length, detailed.InvocationCounts.Length);

                        Assert.Throws<OpenCvException>(() => net.EnableKvCache());
                    }
                }

                using (Net newEngineNet = Net.ReadNetFromOnnx(model, DnnEngine.New))
                    newEngineNet.EnableKvCache().ResetKvCache().DisableKvCache();
            }
            finally
            {
                Directory.Delete(tempRoot, true);
            }
        }

        [Fact]
        public void LayerReferenceOutlivesParentNetAndDisposesExactlyOnce()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            Layer layer;
            using (Net net = Net.ReadNetFromOnnx(DnnFixture.ReadIdentityOnnx(), DnnEngine.Classic))
                layer = net.GetLayer(net.GetLayerNames()[0]);

            Assert.False(layer.IsDisposed);
            RunOrAcceptOpenCvBoundary(() => layer.OutputNameToIndex("output"));
            layer.Dispose();
            layer.Dispose();
            Assert.True(layer.IsDisposed);
            Assert.Throws<ObjectDisposedException>(() => layer.OutputNameToIndex("output"));
        }

        private static void Dispose(Mat[][] groups)
        {
            for (int i = 0; i < groups.Length; i++)
                for (int j = 0; j < groups[i].Length; j++)
                    groups[i][j].Dispose();
        }

        private static void RunOrAcceptOpenCvBoundary(Action action)
        {
            try { action(); }
            catch (OpenCvException exception) { Assert.False(string.IsNullOrWhiteSpace(exception.Message)); }
        }
    }
}

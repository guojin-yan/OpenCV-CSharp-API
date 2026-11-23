using System;
using System.IO;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Dnn;

namespace JYPPX.OpenCvSharp.Tests.Dnn
{
    public class DnnUpstream51CompatibilityTests
    {
        [Theory]
        [InlineData(DnnEngine.Auto)]
        [InlineData(DnnEngine.Classic)]
        [InlineData(DnnEngine.New)]
        [InlineData(DnnEngine.Ort)]
        public void OnnxImportPathsKeepAbiEngineValues(DnnEngine engine)
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;
            byte[] model = DnnFixture.ReadIdentityOnnx();
            string path = Path.Combine(Path.GetTempPath(), "opencv-dnn-51-" + Guid.NewGuid().ToString("N") + ".onnx");
            File.WriteAllBytes(path, model);
            try
            {
                using (Net net = Net.ReadNetFromOnnx(path, engine)) CheckInference(net, DnnModelFormat.Onnx, true);
                using (Net net = Net.ReadNetFromOnnx(model, engine)) CheckInference(net, DnnModelFormat.Onnx, true);
                using (Net net = Net.ReadNetFromOnnx(new ReadOnlySpan<byte>(model), engine)) CheckInference(net, DnnModelFormat.Onnx, true);
                using (Net net = Net.ReadNet(path, engine: engine)) CheckInference(net, DnnModelFormat.Onnx, true);
                using (Net net = Net.ReadNet("onnx", model, engine: engine)) CheckInference(net, DnnModelFormat.Onnx, true);
            }
            finally { File.Delete(path); }
        }

        [Theory]
        [InlineData(DnnEngine.Auto)]
        [InlineData(DnnEngine.Classic)]
        [InlineData(DnnEngine.New)]
        [InlineData(DnnEngine.Ort)]
        public void TFLiteImportReportsStableFormatAndRunsInference(DnnEngine engine)
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;
            byte[] model = DnnFixture.ReadAddZeroTFLite();
            Assert.Equal(408, model.Length);
            string path = Path.Combine(Path.GetTempPath(), "opencv-dnn-51-" + Guid.NewGuid().ToString("N") + ".tflite");
            File.WriteAllBytes(path, model);
            try
            {
                using (Net net = Net.ReadNetFromTFLite(path, engine)) CheckInference(net, DnnModelFormat.TensorFlowLite, false);
                using (Net net = Net.ReadNetFromTFLite(model, engine)) CheckInference(net, DnnModelFormat.TensorFlowLite, false);
                using (Net net = Net.ReadNetFromTFLite(new ReadOnlySpan<byte>(model), engine)) CheckInference(net, DnnModelFormat.TensorFlowLite, false);
                using (Net net = Net.ReadNet(path, engine: engine)) CheckInference(net, DnnModelFormat.TensorFlowLite, false);
                using (Net net = Net.ReadNet("tflite", model, engine: engine)) CheckInference(net, DnnModelFormat.TensorFlowLite, false);
            }
            finally { File.Delete(path); }
        }

        private static void CheckInference(Net net, DnnModelFormat format, bool onnx)
        {
            Assert.Equal(format, net.ModelFormat);
            using (var input = new Mat(2, 2, MatType.CV_32FC1))
            {
                float[] values = { 1, 2, 3, 4 };
                input.CopyFrom(values);
                var parameters = new Image2BlobParams(new Scalar(1),
                    dataLayout: onnx ? DnnDataLayout.Nchw : DnnDataLayout.Nhwc);
                using (Mat blob = JYPPX.OpenCvSharp.Dnn.Cv2.BlobFromImage(input, parameters))
                {
                    net.SetInput(blob, "input");
                    using (Mat output = net.Forward())
                    {
                        Assert.Equal(values, output.ToArray<float>());
                    }
                }
            }
        }

        [Fact]
        public void TensorFlowImportersRetainApiAndReportUpstreamRemoval()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;
            Assert.Equal("5.1.0", OpenCvSharpBuildInfo.GetNativeOpenCvVersion());
            byte[] model = { 1, 2, 3 };
            Action[] imports =
            {
                () => Net.ReadNetFromTensorflow("missing.pb"),
                () => Net.ReadNetFromTensorflow("missing.pb", "missing.pbtxt", DnnEngine.Auto, new[] { "output" }),
                () => Net.ReadNetFromTensorflow(model),
                () => Net.ReadNetFromTensorflow(model, new byte[] { 4 }, DnnEngine.Auto, new[] { "output" }),
                () => Net.ReadNetFromTensorflow(new ReadOnlySpan<byte>(model), ReadOnlySpan<byte>.Empty),
                () => Net.ReadNetFromTensorflow(new ReadOnlySpan<byte>(model), new ReadOnlySpan<byte>(model), DnnEngine.Auto, new[] { "output" })
            };
            foreach (Action import in imports)
            {
                OpenCvException error = Assert.Throws<OpenCvException>(import);
                Assert.Contains("TensorFlow importer was removed in OpenCV 5.1", error.Message);
                Assert.Contains("ONNX", error.Message);
            }
        }
    }
}

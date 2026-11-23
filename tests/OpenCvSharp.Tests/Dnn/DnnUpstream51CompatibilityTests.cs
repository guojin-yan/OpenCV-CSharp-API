using System;
using JYPPX.OpenCvSharp.Dnn;

namespace JYPPX.OpenCvSharp.Tests.Dnn
{
    public class DnnUpstream51CompatibilityTests
    {
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

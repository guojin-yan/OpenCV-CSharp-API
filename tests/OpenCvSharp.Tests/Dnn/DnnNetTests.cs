using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Dnn;
using DnnCv2 = JYPPX.OpenCvSharp.Dnn.Cv2;

namespace JYPPX.OpenCvSharp.Tests.Dnn
{
    public sealed class DnnNetTests
    {
        [Fact]
        public void DnnEnumsMatchOpenCvValues()
        {
            Assert.Equal(0, (int)DnnBackend.Default);
            Assert.Equal(3, (int)DnnBackend.OpenCV);
            Assert.Equal(5, (int)DnnBackend.Cuda);
            Assert.Equal(0, (int)DnnTarget.Cpu);
            Assert.Equal(10, (int)DnnTarget.CpuFp16);
            Assert.Equal(3, (int)DnnEngine.Auto);
            Assert.Equal(1, (int)DnnEngine.Classic);
            Assert.Equal(2, (int)DnnEngine.New);
            Assert.Equal(4, (int)DnnEngine.Ort);
        }

        [Fact]
        public void BlobHelpersValidateManagedArguments()
        {
            using (var image = new Mat())
            using (var blob = new Mat())
            {
                Assert.Throws<ArgumentNullException>(() => DnnCv2.BlobFromImage(null!, blob));
                Assert.Throws<ArgumentNullException>(() => DnnCv2.BlobFromImage(null!));
                Assert.Throws<ArgumentNullException>(() => DnnCv2.BlobFromImage(image, null!));
                Assert.Throws<ArgumentNullException>(() => DnnCv2.BlobFromImages(null!, blob));
                Assert.Throws<ArgumentNullException>(() => DnnCv2.BlobFromImages(null!));
                Assert.Throws<ArgumentException>(() => DnnCv2.BlobFromImages(Array.Empty<Mat>(), blob));
                Assert.Throws<ArgumentException>(() => DnnCv2.BlobFromImages(Array.Empty<Mat>()));
                Assert.Throws<ArgumentNullException>(() => DnnCv2.BlobFromImages(new Mat[] { null! }, blob));
                Assert.Throws<ArgumentNullException>(() => DnnCv2.BlobFromImages(new Mat[] { null! }));
#if NETCOREAPP3_1_OR_GREATER
                Assert.Throws<ArgumentNullException>(() => DnnCv2.BlobFromImages(new Mat[] { null! }.AsSpan(), blob));
#endif
                Assert.Throws<ArgumentNullException>(() => DnnCv2.ImagesFromBlob(null!));
            }
        }

        [Fact]
        public void ReadNetValidatesManagedArguments()
        {
            Assert.Throws<ArgumentNullException>(() => Net.ReadNet(null!));
            Assert.Throws<ArgumentNullException>(() => Net.ReadNet("onnx", (byte[])null!));
            Assert.Throws<ArgumentException>(() => Net.ReadNet("onnx", Array.Empty<byte>()));
            Assert.Throws<ArgumentOutOfRangeException>(() => Net.ReadNet("missing.onnx", engine: (DnnEngine)99));
            Assert.Throws<ArgumentOutOfRangeException>(() => Net.ReadNetFromOnnx("missing.onnx", (DnnEngine)99));
            Assert.Throws<ArgumentOutOfRangeException>(() => Net.ReadNetFromTensorflow("missing.pb", engine: (DnnEngine)99));
            Assert.Throws<ArgumentOutOfRangeException>(() => Net.ReadNetFromTFLite("missing.tflite", (DnnEngine)99));
            Assert.Throws<ArgumentOutOfRangeException>(() => Net.ReadNet("onnx", new byte[] { 1 }, engine: (DnnEngine)99));
#if NETCOREAPP3_1_OR_GREATER
            Assert.Throws<ArgumentOutOfRangeException>(() => ReadNetFromInvalidEngineSpan());
            Assert.Throws<ArgumentException>(() => ReadNetFromEmptySpan());
#endif
        }

#if NETCOREAPP3_1_OR_GREATER
        private static void ReadNetFromInvalidEngineSpan()
        {
            Net.ReadNet("onnx", new byte[] { 1 }.AsSpan(), ReadOnlySpan<byte>.Empty, (DnnEngine)99);
        }

        private static void ReadNetFromEmptySpan()
        {
            Net.ReadNet("onnx", ReadOnlySpan<byte>.Empty, ReadOnlySpan<byte>.Empty);
        }
#endif

        [Fact]
        public void DnnBlobRunsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var image = new Mat(4, 4, MatType.CV_8UC3, new Scalar(1, 2, 3)))
            using (Mat blob = DnnCv2.BlobFromImage(image, 1.0, new Size(4, 4)))
            {
                Assert.False(blob.Empty);
                Assert.Equal(4, blob.Dims);
            }
        }

    }
}

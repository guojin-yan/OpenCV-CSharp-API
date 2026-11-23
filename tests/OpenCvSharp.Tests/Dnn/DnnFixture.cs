using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace JYPPX.OpenCvSharp.Tests.Dnn
{
    internal static class DnnFixture
    {
        internal const string IdentityOnnxSha256 = "326793cdb2fc2da739a715c3f3ff71d09779b389ad29e56bbfccc4313e900744";

        internal static byte[] ReadIdentityOnnx()
        {
            string path = Path.Combine(AppContext.BaseDirectory, "Dnn", "Fixtures", "identity-opset13.onnx.base64");
            string encoded = File.ReadAllText(path, Encoding.ASCII).Trim();
            byte[] model = Convert.FromBase64String(encoded);
            using (SHA256 hasher = SHA256.Create())
            {
                string hash = BitConverter.ToString(hasher.ComputeHash(model)).Replace("-", string.Empty).ToLowerInvariant();
                if (!string.Equals(hash, IdentityOnnxSha256, StringComparison.Ordinal))
                    throw new InvalidOperationException("The deterministic DNN fixture hash does not match its declared contract.");
            }
            return model;
        }

        internal static byte[] ReadAddZeroTFLite()
        {
            string path = Path.Combine(AppContext.BaseDirectory, "Dnn", "Fixtures", "add-zero.tflite.base64");
            byte[] model = Convert.FromBase64String(File.ReadAllText(path, Encoding.ASCII).Trim());
            using (SHA256 hasher = SHA256.Create())
            {
                string hash = BitConverter.ToString(hasher.ComputeHash(model)).Replace("-", string.Empty).ToLowerInvariant();
                if (hash != "bfec1c8a625eabcb718538f14221ad821f4614e32d8e8031b3c179d3502c56c1")
                    throw new InvalidOperationException("The deterministic TFLite fixture hash does not match its declared contract.");
            }
            return model;
        }
    }
}

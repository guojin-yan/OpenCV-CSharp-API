using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace OpenCvSharp.Tests.Dnn
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
    }
}

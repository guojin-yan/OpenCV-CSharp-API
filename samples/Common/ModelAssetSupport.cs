using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;

namespace JYPPX.OpenCvSharp.Samples.Common
{
    internal static class ModelAssetSupport
    {
        public static string ResolveFile(string fileId, string? requestedRoot = null)
        {
            AssetManifest manifest = LoadManifest();
            AssetFile file = manifest.Files.SingleOrDefault(candidate =>
                string.Equals(candidate.Id, fileId, StringComparison.Ordinal))
                ?? throw new InvalidDataException("Unknown sample asset id: " + fileId);

            string root = ResolveCacheRoot(requestedRoot);
            string path = ResolveContainedPath(root, file.RelativePath);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(
                    "The verified sample asset is missing. Run scripts/Get-SampleModelAssets.ps1 first.", path);
            }

            var info = new FileInfo(path);
            if (info.Length != file.SizeBytes)
            {
                throw new InvalidDataException("Sample asset size mismatch: " + path);
            }

            string hash;
            using (FileStream stream = File.OpenRead(path))
            using (SHA256 sha256 = SHA256.Create())
            {
                hash = Convert.ToHexString(sha256.ComputeHash(stream)).ToLowerInvariant();
            }

            if (!string.Equals(hash, file.Sha256, StringComparison.Ordinal))
            {
                throw new InvalidDataException("Sample asset SHA256 mismatch: " + path);
            }

            return path;
        }

        public static string ResolveCacheRoot(string? requestedRoot = null)
        {
            string? configured = string.IsNullOrWhiteSpace(requestedRoot)
                ? Environment.GetEnvironmentVariable("OPENCV_CSHARP_SAMPLE_ASSET_ROOT")
                : requestedRoot;
            string root = string.IsNullOrWhiteSpace(configured)
                ? Path.Combine(Environment.CurrentDirectory, "samples", "assets", "models", "cache")
                : configured;
            return Path.GetFullPath(root);
        }

        private static AssetManifest LoadManifest()
        {
            string path = Path.Combine(AppContext.BaseDirectory, "Assets", "model-assets.json");
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("The sample model asset manifest was not copied to the output.", path);
            }

            AssetManifest? manifest = JsonSerializer.Deserialize<AssetManifest>(
                File.ReadAllText(path),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (manifest == null || manifest.SchemaVersion != 1 || manifest.Files.Count == 0)
            {
                throw new InvalidDataException("The sample model asset manifest is invalid.");
            }

            return manifest;
        }

        private static string ResolveContainedPath(string root, string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath) || Path.IsPathRooted(relativePath))
            {
                throw new InvalidDataException("Sample asset path must be relative.");
            }

            string normalized = relativePath.Replace('/', Path.DirectorySeparatorChar);
            string path = Path.GetFullPath(Path.Combine(root, normalized));
            string prefix = root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
            if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException("Sample asset path escapes the configured cache root.");
            }

            return path;
        }

        private sealed class AssetManifest
        {
            public int SchemaVersion { get; set; }
            public List<AssetFile> Files { get; set; } = new List<AssetFile>();
        }

        private sealed class AssetFile
        {
            public string Id { get; set; } = string.Empty;
            public string RelativePath { get; set; } = string.Empty;
            public long SizeBytes { get; set; }
            public string Sha256 { get; set; } = string.Empty;
        }
    }
}

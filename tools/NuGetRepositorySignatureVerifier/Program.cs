using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using NuGet.Packaging;
using NuGet.Packaging.Signing;

return await ProgramEntry.RunAsync(args);

internal static class ProgramEntry
{
    private const string SignatureEntry = ".signature.p7s";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = null,
    };

    public static async Task<int> RunAsync(string[] args)
    {
        try
        {
            var options = ParseArguments(args);
            var unsignedPath = GetRequiredPath(options, "unsigned");
            var signedPath = GetRequiredPath(options, "signed");
            var packageId = GetRequired(options, "package-id");
            var packageVersion = GetRequired(options, "package-version");
            var expectedOwner = GetRequired(options, "expected-owner");
            var serviceIndex = GetRequired(options, "service-index");
            var verifiedAt = ParseUtc(GetRequired(options, "verified-at"));
            options.TryGetValue("output", out var outputPath);
            var check = options.ContainsKey("check");

            var report = await VerifyAsync(
                unsignedPath,
                signedPath,
                packageId,
                packageVersion,
                expectedOwner,
                serviceIndex,
                verifiedAt);

            var json = JsonSerializer.Serialize(report, JsonOptions) + "\n";
            if (!string.IsNullOrWhiteSpace(outputPath))
            {
                var fullOutputPath = Path.GetFullPath(outputPath);
                if (check)
                {
                    if (!File.Exists(fullOutputPath))
                    {
                        throw new InvalidOperationException($"Check output does not exist: {fullOutputPath}");
                    }

                    var actual = File.ReadAllText(fullOutputPath, Encoding.UTF8).Replace("\r\n", "\n");
                    if (!string.Equals(actual, json, StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException($"Repository-signing report drifted: {fullOutputPath}");
                    }
                }
                else
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(fullOutputPath)!);
                    File.WriteAllText(fullOutputPath, json, new UTF8Encoding(false));
                }
            }
            else if (check)
            {
                throw new InvalidOperationException("--check requires --output.");
            }

            Console.WriteLine(
                $"NUGET_REPOSITORY_SIGNATURE_STRUCTURE_OK package={packageId}/{packageVersion} " +
                $"owner={expectedOwner} entries={report.Payload.EntryCount} " +
                $"unsigned_sha256={report.UnsignedPackage.Sha256} signed_sha256={report.RepositorySignedPackage.Sha256}");
            if (!string.IsNullOrWhiteSpace(outputPath))
            {
                Console.WriteLine($"Report: {Path.GetFullPath(outputPath)}");
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"NUGET_REPOSITORY_SIGNATURE_STRUCTURE_FAILED: {ex.Message}");
            return 1;
        }
    }

    private static async Task<VerificationReport> VerifyAsync(
        string unsignedPath,
        string signedPath,
        string packageId,
        string packageVersion,
        string expectedOwner,
        string serviceIndex,
        DateTimeOffset verifiedAt)
    {
        var unsignedEntries = ReadEntries(unsignedPath);
        var signedEntries = ReadEntries(signedPath);
        if (unsignedEntries.ContainsKey(SignatureEntry))
        {
            throw new InvalidOperationException("The frozen candidate must be unsigned and must not contain .signature.p7s.");
        }

        if (!signedEntries.TryGetValue(SignatureEntry, out var signatureEntry) || signatureEntry.Length == 0)
        {
            throw new InvalidOperationException("The downloaded package does not contain a non-empty .signature.p7s entry.");
        }

        var signedPayload = signedEntries
            .Where(item => !string.Equals(item.Key, SignatureEntry, StringComparison.Ordinal))
            .ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);
        AssertPayloadEqual(unsignedEntries, signedPayload);

        using var unsignedReader = new PackageArchiveReader(unsignedPath);
        using var signedReader = new PackageArchiveReader(signedPath);
        AssertPackageIdentity(unsignedReader, packageId, packageVersion, "unsigned");
        AssertPackageIdentity(signedReader, packageId, packageVersion, "repository-signed");

        if (await unsignedReader.IsSignedAsync(CancellationToken.None))
        {
            throw new InvalidOperationException("The frozen candidate unexpectedly reports a primary signature.");
        }

        var primarySignature = await signedReader.GetPrimarySignatureAsync(CancellationToken.None)
            ?? throw new InvalidOperationException("The downloaded package has no parseable primary signature.");
        if (primarySignature is not RepositoryPrimarySignature repositorySignature ||
            primarySignature.Type != SignatureType.Repository)
        {
            throw new InvalidOperationException($"Expected a repository primary signature, got {primarySignature.Type}.");
        }

        await signedReader.ValidateIntegrityAsync(primarySignature.SignatureContent, CancellationToken.None);

        var actualServiceIndex = repositorySignature.V3ServiceIndexUrl.AbsoluteUri;
        if (!string.Equals(actualServiceIndex, serviceIndex, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Repository signature service index mismatch: actual={actualServiceIndex} expected={serviceIndex}");
        }

        var packageOwners = repositorySignature.PackageOwners ?? Array.Empty<string>();
        if (!packageOwners.Contains(expectedOwner, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Repository signature owners do not contain '{expectedOwner}': {string.Join(",", packageOwners)}");
        }

        if (!string.Equals(primarySignature.SignatureContent.HashAlgorithm.ToString(), "SHA256", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Repository signature content hash must use SHA256, got {primarySignature.SignatureContent.HashAlgorithm}.");
        }

        var certificate = primarySignature.SignerInfo.Certificate
            ?? throw new InvalidOperationException("Repository signature does not carry a signer certificate.");
        if (!certificate.Subject.Contains("CN=NuGet.org Repository by Microsoft", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Unexpected repository signer subject: {certificate.Subject}");
        }

        if (primarySignature.Timestamps.Count == 0)
        {
            throw new InvalidOperationException("Repository signature has no trusted timestamp token.");
        }

        var payloadFacts = unsignedEntries.Values.OrderBy(item => item.Path, StringComparer.Ordinal).ToArray();
        var payloadSha256 = ComputeCanonicalPayloadHash(payloadFacts);
        var timestamps = primarySignature.Timestamps
            .Select(timestamp => timestamp.GeneralizedTime.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss'Z'"))
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();

        return new VerificationReport(
            SchemaVersion: 1,
            RecordKind: "nuget-repository-signature-verification",
            Strategy: "nuget.org-repository-signing",
            VerifiedAt: verifiedAt.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'"),
            Package: new PackageIdentity(packageId, packageVersion),
            UnsignedPackage: GetPackageFileFact(unsignedPath, unsignedEntries.Count),
            RepositorySignedPackage: GetPackageFileFact(signedPath, signedEntries.Count),
            RepositorySignature: new RepositorySignatureFact(
                Type: primarySignature.Type.ToString(),
                ServiceIndex: actualServiceIndex,
                Owners: packageOwners.OrderBy(value => value, StringComparer.Ordinal).ToArray(),
                ContentHashAlgorithm: primarySignature.SignatureContent.HashAlgorithm.ToString(),
                ContentHash: primarySignature.SignatureContent.HashValue,
                CertificateSubject: certificate.Subject,
                CertificateIssuer: certificate.Issuer,
                CertificateSha256: certificate.GetCertHashString(HashAlgorithmName.SHA256).ToLowerInvariant(),
                TimestampCount: timestamps.Length,
                Timestamps: timestamps),
            Payload: new PayloadFact(
                EntryCount: payloadFacts.Length,
                CanonicalSha256: payloadSha256,
                SignatureEntry: SignatureEntry,
                SignatureEntryBytes: signatureEntry.Length,
                MismatchCount: 0),
            SensitiveMaterialPresent: false,
            VerificationResult: "payload-and-repository-signature-structure-passed");
    }

    private static SortedDictionary<string, EntryFact> ReadEntries(string path)
    {
        using var archive = ZipFile.OpenRead(path);
        var result = new SortedDictionary<string, EntryFact>(StringComparer.Ordinal);
        var caseInsensitivePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in archive.Entries.Where(entry => !entry.FullName.EndsWith('/')))
        {
            var normalizedPath = entry.FullName.Replace('\\', '/');
            if (normalizedPath.StartsWith('/') ||
                normalizedPath.Split('/').Any(segment => segment is "" or "." or ".."))
            {
                throw new InvalidOperationException($"Unsafe package entry path: {entry.FullName}");
            }

            if (!caseInsensitivePaths.Add(normalizedPath))
            {
                throw new InvalidOperationException($"Duplicate or case-colliding package entry: {normalizedPath}");
            }

            using var stream = entry.Open();
            using var memory = new MemoryStream();
            stream.CopyTo(memory);
            var bytes = memory.ToArray();
            result.Add(normalizedPath, new EntryFact(normalizedPath, bytes.LongLength, Sha256(bytes)));
        }

        return result;
    }

    private static void AssertPayloadEqual(
        IReadOnlyDictionary<string, EntryFact> expected,
        IReadOnlyDictionary<string, EntryFact> actual)
    {
        var expectedPaths = expected.Keys.OrderBy(value => value, StringComparer.Ordinal).ToArray();
        var actualPaths = actual.Keys.OrderBy(value => value, StringComparer.Ordinal).ToArray();
        if (!expectedPaths.SequenceEqual(actualPaths, StringComparer.Ordinal))
        {
            var missing = expectedPaths.Except(actualPaths, StringComparer.Ordinal);
            var extra = actualPaths.Except(expectedPaths, StringComparer.Ordinal);
            throw new InvalidOperationException(
                $"Repository-signed payload closure drifted; missing=[{string.Join(",", missing)}] extra=[{string.Join(",", extra)}].");
        }

        foreach (var path in expectedPaths)
        {
            var expectedFact = expected[path];
            var actualFact = actual[path];
            if (expectedFact.Length != actualFact.Length ||
                !string.Equals(expectedFact.Sha256, actualFact.Sha256, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Repository-signed payload entry drifted: {path} expected={expectedFact.Length}/{expectedFact.Sha256} actual={actualFact.Length}/{actualFact.Sha256}");
            }
        }
    }

    private static void AssertPackageIdentity(
        PackageArchiveReader reader,
        string expectedId,
        string expectedVersion,
        string label)
    {
        var actualId = reader.NuspecReader.GetId();
        var actualVersion = reader.NuspecReader.GetVersion().ToNormalizedString();
        if (!string.Equals(actualId, expectedId, StringComparison.Ordinal) ||
            !string.Equals(actualVersion, expectedVersion, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"{label} package identity mismatch: actual={actualId}/{actualVersion} expected={expectedId}/{expectedVersion}");
        }
    }

    private static PackageFileFact GetPackageFileFact(string path, int entryCount)
    {
        var file = new FileInfo(path);
        return new PackageFileFact(file.Name, file.Length, FileSha256(path), entryCount);
    }

    private static string ComputeCanonicalPayloadHash(IEnumerable<EntryFact> entries)
    {
        var text = string.Join("\n", entries.Select(item => $"{item.Path}\0{item.Length}\0{item.Sha256}")) + "\n";
        return Sha256(Encoding.UTF8.GetBytes(text));
    }

    private static string FileSha256(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static string Sha256(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static DateTimeOffset ParseUtc(string value)
    {
        if (!DateTimeOffset.TryParseExact(
                value,
                "yyyy-MM-dd'T'HH:mm:ss'Z'",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal,
                out var result))
        {
            throw new ArgumentException($"--verified-at must be a whole-second UTC timestamp: {value}");
        }

        return result;
    }

    private static Dictionary<string, string> ParseArguments(string[] args)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        for (var index = 0; index < args.Length; index++)
        {
            var token = args[index];
            if (!token.StartsWith("--", StringComparison.Ordinal) || token.Length == 2)
            {
                throw new ArgumentException($"Unexpected argument: {token}");
            }

            var name = token[2..];
            if (name == "check")
            {
                if (!result.TryAdd(name, "true"))
                {
                    throw new ArgumentException("Duplicate argument: --check");
                }
                continue;
            }

            if (index + 1 >= args.Length || args[index + 1].StartsWith("--", StringComparison.Ordinal))
            {
                throw new ArgumentException($"Argument requires a value: {token}");
            }

            if (!result.TryAdd(name, args[++index]))
            {
                throw new ArgumentException($"Duplicate argument: {token}");
            }
        }

        var allowed = new HashSet<string>(new[]
        {
            "unsigned", "signed", "package-id", "package-version", "expected-owner",
            "service-index", "verified-at", "output", "check",
        }, StringComparer.Ordinal);
        var unknown = result.Keys.Where(name => !allowed.Contains(name)).OrderBy(value => value, StringComparer.Ordinal).ToArray();
        if (unknown.Length > 0)
        {
            throw new ArgumentException($"Unknown argument(s): {string.Join(",", unknown)}");
        }

        return result;
    }

    private static string GetRequired(IReadOnlyDictionary<string, string> options, string name)
    {
        if (!options.TryGetValue(name, out var value) || string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"Missing required argument: --{name}");
        }

        return value;
    }

    private static string GetRequiredPath(IReadOnlyDictionary<string, string> options, string name)
    {
        var value = Path.GetFullPath(GetRequired(options, name));
        if (!File.Exists(value))
        {
            throw new FileNotFoundException($"Package does not exist: {value}", value);
        }

        return value;
    }

    private sealed record EntryFact(string Path, long Length, string Sha256);
    private sealed record PackageIdentity(string Id, string Version);
    private sealed record PackageFileFact(string FileName, long Bytes, string Sha256, int EntryCount);
    private sealed record RepositorySignatureFact(
        string Type,
        string ServiceIndex,
        string[] Owners,
        string ContentHashAlgorithm,
        string ContentHash,
        string CertificateSubject,
        string CertificateIssuer,
        string CertificateSha256,
        int TimestampCount,
        string[] Timestamps);
    private sealed record PayloadFact(
        int EntryCount,
        string CanonicalSha256,
        string SignatureEntry,
        long SignatureEntryBytes,
        int MismatchCount);
    private sealed record VerificationReport(
        int SchemaVersion,
        string RecordKind,
        string Strategy,
        string VerifiedAt,
        PackageIdentity Package,
        PackageFileFact UnsignedPackage,
        PackageFileFact RepositorySignedPackage,
        RepositorySignatureFact RepositorySignature,
        PayloadFact Payload,
        bool SensitiveMaterialPresent,
        string VerificationResult);
}

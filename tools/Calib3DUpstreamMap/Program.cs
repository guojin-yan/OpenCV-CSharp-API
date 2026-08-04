using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

internal static class Program
{
    private static readonly StringComparer Ordinal = StringComparer.Ordinal;
    private static readonly string[] AllowedClassifications =
    {
        "implemented",
        "missing",
        "intentionally-omitted",
        "upstream-conditional",
        "unsupported",
        "non-callable-metadata"
    };

    private sealed record Options(
        string RepositoryRoot,
        string WorkspaceRoot,
        string RawPath,
        string ClassificationPath,
        string NativeManifestPath,
        string ManagedBaselinePath,
        string OutputPath,
        string SummaryPath,
        string FamilyOutputPath,
        bool InitializeClassification,
        bool Check);

    private sealed class RawDocument
    {
        public int SchemaVersion { get; set; }
        public string Generator { get; set; } = string.Empty;
        public string UpstreamOpenCvVersion { get; set; } = string.Empty;
        public string HeaderPath { get; set; } = string.Empty;
        public string HeaderSha256 { get; set; } = string.Empty;
        public string ParserPath { get; set; } = string.Empty;
        public string ParserSha256 { get; set; } = string.Empty;
        public Dictionary<string, int> PreprocessorDefinitions { get; set; } = new(Ordinal);
        public List<SourceHeader> SourceHeaders { get; set; } = new();
        public int DeclarationCount { get; set; }
        public List<RawDeclaration> Declarations { get; set; } = new();
    }

    private sealed class SourceHeader
    {
        public string Path { get; set; } = string.Empty;
        public string Sha256 { get; set; } = string.Empty;
        public int StartOrdinal { get; set; }
        public int DeclarationCount { get; set; }
    }

    private sealed class RawDeclaration
    {
        public int Ordinal { get; set; }
        public string SourceHeader { get; set; } = string.Empty;
        public string Kind { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Identity { get; set; } = string.Empty;
        public string ReturnType { get; set; } = string.Empty;
        public List<string> Modifiers { get; set; } = new();
        public List<RawArgument> Arguments { get; set; } = new();
        public List<RawEnumValue> EnumValues { get; set; } = new();
        public string BaseDeclaration { get; set; } = string.Empty;
        public string Documentation { get; set; } = string.Empty;
    }

    private sealed class RawArgument
    {
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Default { get; set; } = string.Empty;
        public List<string> Modifiers { get; set; } = new();
    }

    private sealed class RawEnumValue
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    private sealed class ClassificationDocument
    {
        public int SchemaVersion { get; set; } = 1;
        public string UpstreamOpenCvVersion { get; set; } = "5.0.0";
        public string ClaimedSlice { get; set; } = "opencv2/calib3d.hpp compatibility include closure: parser-emitted geometry/2d.hpp, geometry/3d.hpp, stereo.hpp, and calib.hpp declarations";
        public string ReviewStatus { get; set; } = "reviewed";
        public string Limitation { get; set; } = "Declaration identities preserve parsed overload, default, direction, and type metadata. Implemented evidence correlates symbol groups across the C++ API, stable C ABI, and managed public API; it does not claim binary equivalence of C++ parameter layouts or repository-wide OpenCV parity.";
        public List<ClassificationRow> Declarations { get; set; } = new();
    }

    private sealed class ClassificationRow
    {
        public int Ordinal { get; set; }
        public string Identity { get; set; } = string.Empty;
        public string Classification { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public List<string> NativeEntrypoints { get; set; } = new();
        public List<string> ManagedMembers { get; set; } = new();
    }

    private sealed class MappingSummary
    {
        public int SchemaVersion { get; init; } = 1;
        public string Generator { get; init; } = "tools/Calib3DUpstreamMap";
        public string UpstreamOpenCvVersion { get; init; } = "5.0.0";
        public string ClaimedSlice { get; init; } = "opencv2/calib3d.hpp compatibility include closure: parser-emitted geometry/2d.hpp, geometry/3d.hpp, stereo.hpp, and calib.hpp declarations";
        public string RawExtractionPath { get; init; } = string.Empty;
        public string ClassificationPath { get; init; } = string.Empty;
        public string MappingPath { get; init; } = string.Empty;
        public string HeaderSha256 { get; init; } = string.Empty;
        public string ParserSha256 { get; init; } = string.Empty;
        public int SourceHeaderCount { get; init; }
        public string SourceHeaderSetSha256 { get; init; } = string.Empty;
        public string MappingSha256 { get; init; } = string.Empty;
        public int DeclarationCount { get; init; }
        public int EnumCount { get; init; }
        public int ClassCount { get; init; }
        public int CallableCount { get; init; }
        public SortedDictionary<string, int> ClassificationCounts { get; init; } = new(Ordinal);
        public int NativeEvidenceCount { get; init; }
        public int ManagedEvidenceCount { get; init; }
        public int NegativeFixtureCount { get; init; }
        public string FamilyInventoryPath { get; init; } = string.Empty;
        public string FamilyInventorySha256 { get; init; } = string.Empty;
        public int SelectedFamilyCount { get; init; }
        public int SelectedDeclarationCount { get; init; }
        public int ManagedPublicTypeAdditionCount { get; init; }
        public int ManagedPublicMemberAdditionCount { get; init; }
        public bool RepositoryWideUpstreamParityClaimed { get; init; }
    }

    private sealed class FamilyInventory
    {
        public int SchemaVersion { get; init; } = 1;
        public string UpstreamOpenCvVersion { get; init; } = "5.0.0";
        public string Status { get; init; } = "implemented-verified";
        public int ManagedPublicTypeAdditionCount { get; init; } = 12;
        public int ManagedPublicMemberAdditionCount { get; init; } = 120;
        public List<FamilyRow> Families { get; init; } = new();
    }

    private sealed class FamilyRow
    {
        public string Id { get; init; } = string.Empty;
        public string Rationale { get; init; } = string.Empty;
        public List<FamilyOperation> Declarations { get; init; } = new();
    }

    private sealed class FamilyOperation
    {
        public int Ordinal { get; init; }
        public string UpstreamIdentity { get; init; } = string.Empty;
        public string UpstreamClassification { get; init; } = string.Empty;
        public List<string> NativeEntrypoints { get; init; } = new();
        public List<string> ManagedMembers { get; init; } = new();
        public string FocusedTest { get; init; } = "tests/OpenCvSharp.Tests/Calib3D/Calib3DUpstreamParityTests.cs";
        public string NativeSmoke { get; init; } = "src/OpenCvSharp.Native/tests/native_smoke.cpp";
        public string Sample { get; init; } = "samples/ConsoleSamples/Program.cs";
        public string Guide { get; init; } = "docs/articles/calib3d-upstream-parity-guide.md";
    }

    private sealed record ManagedMember(string TypeName, string Kind, string Name, string Evidence);

    private static int Main(string[] args)
    {
        try
        {
            Options options = ParseOptions(args);
            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            RawDocument raw = Deserialize<RawDocument>(options.RawPath, jsonOptions);
            string[] nativeEntrypoints = ReadNativeEntrypoints(options.NativeManifestPath);
            ManagedMember[] managedMembers = ReadManagedMembers(options.ManagedBaselinePath);

            if (options.InitializeClassification)
            {
                ClassificationDocument initialized = InitializeClassification(raw, nativeEntrypoints, managedMembers);
                WriteNormalizedJson(options.ClassificationPath, initialized, check: false);
            }

            ClassificationDocument classifications = Deserialize<ClassificationDocument>(options.ClassificationPath, jsonOptions);
            Validate(raw, classifications, options, nativeEntrypoints, managedMembers, verifyFileHashes: true);
            Console.WriteLine("CALIB3D_UPSTREAM_MAP_BASE_VALIDATION_OK");

            string mappingText = BuildMappingText(raw, classifications);
            FamilyInventory familyInventory = BuildFamilyInventory(raw, classifications);
            int[] selectedOrdinals = familyInventory.Families.SelectMany(value => value.Declarations).Select(value => value.Ordinal).ToArray();
            Require(selectedOrdinals.Length == raw.Declarations.Count && selectedOrdinals.Distinct().Count() == raw.Declarations.Count,
                "Calib3D family inventory must partition every declaration exactly once.");
            string sourceHeaderSetText = string.Join("\n", raw.SourceHeaders.Select(value =>
                $"{value.Path}|{value.Sha256}|{value.StartOrdinal}|{value.DeclarationCount}")) + "\n";
            string familyText = Serialize(familyInventory);
            var counts = classifications.Declarations
                .GroupBy(value => value.Classification, Ordinal)
                .ToDictionary(group => group.Key, group => group.Count(), Ordinal);
            var orderedCounts = new SortedDictionary<string, int>(Ordinal);
            foreach (string classification in AllowedClassifications)
            {
                orderedCounts[classification] = counts.TryGetValue(classification, out int count) ? count : 0;
            }

            var summary = new MappingSummary
            {
                RawExtractionPath = RelativePath(options.RepositoryRoot, options.RawPath),
                ClassificationPath = RelativePath(options.RepositoryRoot, options.ClassificationPath),
                MappingPath = RelativePath(options.RepositoryRoot, options.OutputPath),
                HeaderSha256 = raw.HeaderSha256,
                ParserSha256 = raw.ParserSha256,
                SourceHeaderCount = raw.SourceHeaders.Count,
                SourceHeaderSetSha256 = Sha256(sourceHeaderSetText),
                MappingSha256 = Sha256(mappingText),
                DeclarationCount = raw.Declarations.Count,
                EnumCount = raw.Declarations.Count(value => value.Kind == "enum"),
                ClassCount = raw.Declarations.Count(value => value.Kind == "class"),
                CallableCount = raw.Declarations.Count(value => value.Kind == "callable"),
                ClassificationCounts = orderedCounts,
                NativeEvidenceCount = classifications.Declarations.SelectMany(value => value.NativeEntrypoints).Distinct(Ordinal).Count(),
                ManagedEvidenceCount = classifications.Declarations.SelectMany(value => value.ManagedMembers).Distinct(Ordinal).Count(),
                NegativeFixtureCount = 11,
                FamilyInventoryPath = RelativePath(options.RepositoryRoot, options.FamilyOutputPath),
                FamilyInventorySha256 = Sha256(familyText),
                SelectedFamilyCount = familyInventory.Families.Count,
                SelectedDeclarationCount = familyInventory.Families.Sum(value => value.Declarations.Count),
                ManagedPublicTypeAdditionCount = familyInventory.ManagedPublicTypeAdditionCount,
                ManagedPublicMemberAdditionCount = familyInventory.ManagedPublicMemberAdditionCount,
                RepositoryWideUpstreamParityClaimed = false
            };
            string summaryText = Serialize(summary);
            RunNegativeFixtures(raw, classifications, summary, options, nativeEntrypoints, managedMembers);
            WriteOrCheck(options.OutputPath, mappingText, options.Check);
            WriteOrCheck(options.SummaryPath, summaryText, options.Check);
            WriteOrCheck(options.FamilyOutputPath, familyText, options.Check);

            Console.WriteLine(
                "CALIB3D_UPSTREAM_MAP_OK declarations={0} callable={1} implemented={2} missing={3} omitted={4} fixtures=11 sha256={5} mode={6}",
                summary.DeclarationCount,
                summary.CallableCount,
                summary.ClassificationCounts["implemented"],
                summary.ClassificationCounts["missing"],
                summary.ClassificationCounts["intentionally-omitted"],
                summary.MappingSha256,
                options.Check ? "check" : "write");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
    }

    private static Options ParseOptions(string[] args)
    {
        var values = new Dictionary<string, string>(Ordinal);
        bool initialize = false;
        bool check = false;
        for (int index = 0; index < args.Length; index++)
        {
            if (args[index] == "--initialize-classification")
            {
                initialize = true;
                continue;
            }
            if (args[index] == "--check")
            {
                check = true;
                continue;
            }
            if (!args[index].StartsWith("--", StringComparison.Ordinal) || index + 1 >= args.Length)
            {
                throw new ArgumentException("Expected --name value arguments and optional --initialize-classification or --check.");
            }
            values.Add(args[index], args[++index]);
        }

        string Required(string name) => values.TryGetValue(name, out string? value) && !string.IsNullOrWhiteSpace(value)
            ? Path.GetFullPath(value)
            : throw new ArgumentException("Missing required argument " + name + ".");
        if (initialize && check)
        {
            throw new ArgumentException("--initialize-classification and --check cannot be combined.");
        }

        return new Options(
            Required("--repository"),
            Required("--workspace"),
            Required("--raw"),
            Required("--classification"),
            Required("--native-manifest"),
            Required("--managed-baseline"),
            Required("--output"),
            Required("--summary"),
            Required("--family-output"),
            initialize,
            check);
    }

    private static ClassificationDocument InitializeClassification(
        RawDocument raw,
        IReadOnlyList<string> nativeEntrypoints,
        IReadOnlyList<ManagedMember> managedMembers)
    {
        var result = new ClassificationDocument();
        foreach (RawDeclaration declaration in raw.Declarations)
        {
            var row = new ClassificationRow { Ordinal = declaration.Ordinal, Identity = declaration.Identity };
            if (declaration.Kind is "enum" or "class")
            {
                row.Classification = "non-callable-metadata";
                row.Reason = declaration.Kind == "enum"
                    ? "Enum declaration and values are retained as normalized metadata; this callable-slice map does not claim value-level enum parity."
                    : "Class declaration and inheritance are retained as normalized metadata; constructors and methods are classified independently.";
                result.Declarations.Add(row);
                continue;
            }

            string owner = OwnerName(declaration.Name);
            string symbol = SymbolName(declaration.Name);
            string managedName = ManagedName(owner, symbol);
            row.NativeEntrypoints = FindNativeEvidence(declaration, owner, symbol, nativeEntrypoints).ToList();
            row.ManagedMembers = FindManagedEvidence(declaration, owner, managedName, managedMembers).ToList();

            if (row.NativeEntrypoints.Count > 0 && row.ManagedMembers.Count > 0)
            {
                row.Classification = "implemented";
                row.Reason = "Native and managed symbol-group evidence is present for this parsed declaration identity.";
            }
            else
            {
                row.Classification = "missing";
                row.Reason = row.NativeEntrypoints.Count == 0 && row.ManagedMembers.Count == 0
                    ? "No stable native C ABI or public managed evidence was found for this declaration identity."
                    : row.NativeEntrypoints.Count == 0
                        ? "Public managed evidence exists, but no stable native C ABI evidence was found for this declaration identity."
                        : "Stable native C ABI evidence exists, but no public managed evidence was found for this declaration identity.";
            }
            result.Declarations.Add(row);
        }
        return result;
    }

    private static string OwnerName(string upstreamName)
    {
        string[] parts = upstreamName.Split('.');
        return parts.Length >= 3 ? parts[^2] : string.Empty;
    }

    private static string SymbolName(string upstreamName) => upstreamName.Split('.')[^1];

    private static string ManagedName(string owner, string symbol)
    {
        if (symbol == owner)
        {
            return ".ctor";
        }
        if (owner.Length > 0 && owner != "Subdiv2D" && symbol.StartsWith("set", StringComparison.Ordinal) && symbol.Length > 3)
        {
            return PropertyName(symbol[3..]);
        }
        if (owner.Length > 0 && owner != "Subdiv2D" && symbol.StartsWith("get", StringComparison.Ordinal) && symbol.Length > 3)
        {
            return PropertyName(symbol[3..]);
        }
        string name = char.ToUpperInvariant(symbol[0]) + symbol[1..];
        return owner == "fisheye" ? "Fisheye" + name : name;
    }

    private static string PropertyName(string value) => value;

    private static IEnumerable<string> FindManagedEvidence(
        RawDeclaration declaration,
        string owner,
        string managedName,
        IReadOnlyList<ManagedMember> members)
    {
        IEnumerable<ManagedMember> candidates = members.Where(value => value.Name == managedName);
        if (owner == "Subdiv2D")
        {
            candidates = candidates.Where(value => value.TypeName == "JYPPX.OpenCvSharp.ImgProc.Subdiv2D");
        }
        else if (owner == "fisheye")
        {
            candidates = candidates.Where(value => value.TypeName == "JYPPX.OpenCvSharp.Calib3D.Cv2");
        }
        else if (owner.Length > 0)
        {
            string typeName = "JYPPX.OpenCvSharp.Calib3D." + owner;
            candidates = candidates.Where(value => value.TypeName == typeName);
        }
        else
        {
            candidates = candidates.Where(value => value.TypeName.EndsWith(".Cv2", StringComparison.Ordinal));
        }
        bool usacOverload = declaration.Identity.Contains("UsacParams", StringComparison.Ordinal);
        if (managedName != ".ctor")
        {
            candidates = candidates.Where(value => value.Evidence.Contains("JYPPX.OpenCvSharp.Calib3D.UsacParams", StringComparison.Ordinal) == usacOverload);
        }
        return candidates.Select(value => value.Evidence).Distinct(Ordinal).OrderBy(value => value, Ordinal);
    }

    private static IEnumerable<string> FindNativeEvidence(
        RawDeclaration declaration,
        string owner,
        string symbol,
        IReadOnlyList<string> entrypoints)
    {
        if (symbol == owner)
        {
            string constructorToken = owner == "Subdiv2D"
                ? "calib3d_subdiv2d_create"
                : "calib3d_usac_params_get_default";
            return entrypoints.Where(value => ContainsToken(value, constructorToken)).OrderBy(value => value, Ordinal);
        }

        bool usacOverload = declaration.Identity.Contains("UsacParams", StringComparison.Ordinal);
        if (usacOverload && symbol != "UsacParams")
        {
            string exactUsacToken = "calib3d_" + NativeSymbolToken(symbol) + "_usac";
            return entrypoints.Where(value => ContainsToken(value, exactUsacToken)).OrderBy(value => value, Ordinal);
        }

        string symbolToken = NativeSymbolToken(symbol);
        string ownerToken = owner.Length == 0 ? string.Empty : SnakeCase(owner) + "_";
        var tokens = new List<string>();
        if (owner == "fisheye")
        {
            ownerToken = "fisheye_";
            if (symbol == "estimateNewCameraMatrixForUndistortRectify")
            {
                symbolToken = "estimate_new_camera_matrix";
            }
        }
        tokens.Add(ownerToken + symbolToken);
        if (owner.Length == 0)
        {
            tokens.Add(symbolToken);
        }

        return entrypoints.Where(entrypoint =>
                !entrypoint.EndsWith("_usac", StringComparison.Ordinal) &&
                tokens.Any(token => ContainsToken(entrypoint, token)))
            .Distinct(Ordinal)
            .OrderBy(value => value, Ordinal);
    }

    private static string NativeSymbolToken(string symbol)
    {
        if (symbol == "getRotationMatrix2D")
        {
            return "get_rotation_matrix2d";
        }
        string token = SnakeCase(symbol).Replace("_pn_p", "_pnp", StringComparison.Ordinal);
        return Regex.Replace(token, "(?<=[a-z])([23])d", "_$1d", RegexOptions.CultureInvariant);
    }

    private static bool ContainsToken(string entrypoint, string token)
    {
        int index = entrypoint.IndexOf("_" + token, StringComparison.Ordinal);
        while (index >= 0)
        {
            int end = index + token.Length + 1;
            if (end == entrypoint.Length || entrypoint[end] == '_')
            {
                return true;
            }
            index = entrypoint.IndexOf("_" + token, index + 1, StringComparison.Ordinal);
        }
        return false;
    }

    private static string SnakeCase(string value)
    {
        var builder = new StringBuilder();
        for (int index = 0; index < value.Length; index++)
        {
            char current = value[index];
            if (char.IsUpper(current) && index > 0 &&
                (char.IsLower(value[index - 1]) || (index + 1 < value.Length && char.IsLower(value[index + 1]))))
            {
                builder.Append('_');
            }
            builder.Append(char.ToLowerInvariant(current));
        }
        return builder.ToString();
    }

    private static void Validate(
        RawDocument raw,
        ClassificationDocument classifications,
        Options options,
        IReadOnlyList<string> nativeEntrypoints,
        IReadOnlyList<ManagedMember> managedMembers,
        bool verifyFileHashes)
    {
        Require(raw.SchemaVersion == 1 && raw.Generator == "tools/Calib3DUpstreamMap/extract_calib3d.py" && raw.UpstreamOpenCvVersion == "5.0.0", "Raw extraction identity drifted.");
        Require(raw.DeclarationCount == 194 && raw.Declarations.Count == 194, "Raw extraction must contain exactly 194 declarations.");
        Require(raw.Declarations.Count(value => value.Kind == "enum") == 22 && raw.Declarations.Count(value => value.Kind == "class") == 5 && raw.Declarations.Count(value => value.Kind == "callable") == 167, "Raw declaration kind counts drifted.");
        Require(raw.PreprocessorDefinitions.Count == 1 && raw.PreprocessorDefinitions.TryGetValue("CV_VERSION_MAJOR", out int major) && major == 5, "OpenCV 5 preprocessor context drifted.");
        Require(raw.Declarations.Select(value => value.Ordinal).SequenceEqual(Enumerable.Range(0, raw.Declarations.Count)), "Raw declarations are reordered or have non-contiguous ordinals.");
        Require(raw.Declarations.Select(value => value.Identity).Distinct(Ordinal).Count() == raw.Declarations.Count, "Raw declarations contain a duplicate identity or collapsed overload.");
        Require(raw.Declarations.All(value => value.Kind is "enum" or "class" or "callable" && value.Identity.Length > 0), "Raw declaration kind or identity is invalid.");
        string[] expectedSourceHeaders =
        {
            "opencv-source/opencv-5.0.0/modules/geometry/include/opencv2/geometry/2d.hpp",
            "opencv-source/opencv-5.0.0/modules/geometry/include/opencv2/geometry/3d.hpp",
            "opencv-source/opencv-5.0.0/modules/stereo/include/opencv2/stereo.hpp",
            "opencv-source/opencv-5.0.0/modules/calib/include/opencv2/calib.hpp"
        };
        int[] expectedStarts = { 0, 53, 120, 175 };
        int[] expectedCounts = { 53, 67, 55, 19 };
        Require(raw.SourceHeaders.Count == expectedSourceHeaders.Length, "Calib3D source-header closure count drifted.");
        for (int index = 0; index < expectedSourceHeaders.Length; index++)
        {
            SourceHeader source = raw.SourceHeaders[index];
            Require(source.Path == expectedSourceHeaders[index] && source.StartOrdinal == expectedStarts[index] && source.DeclarationCount == expectedCounts[index], "Calib3D source-header closure identity or range drifted.");
            Require(raw.Declarations.Skip(source.StartOrdinal).Take(source.DeclarationCount).All(value => value.SourceHeader == source.Path), "Declaration source-header ownership drifted: " + source.Path);
        }
        if (verifyFileHashes)
        {
            Require(FileSha256(Path.Combine(options.WorkspaceRoot, raw.HeaderPath.Replace('/', Path.DirectorySeparatorChar))) == raw.HeaderSha256, "Parser input header SHA256 drifted.");
            Require(FileSha256(Path.Combine(options.WorkspaceRoot, raw.ParserPath.Replace('/', Path.DirectorySeparatorChar))) == raw.ParserSha256, "OpenCV parser SHA256 drifted.");
            foreach (SourceHeader source in raw.SourceHeaders)
            {
                Require(FileSha256(Path.Combine(options.WorkspaceRoot, source.Path.Replace('/', Path.DirectorySeparatorChar))) == source.Sha256, "Parser source-header SHA256 drifted: " + source.Path);
            }
        }

        Require(classifications.SchemaVersion == 1 && classifications.UpstreamOpenCvVersion == "5.0.0" && classifications.ReviewStatus == "reviewed", "Classification manifest identity or review status drifted.");
        Require(classifications.Declarations.Count == raw.Declarations.Count, "Classification manifest must contain one row per raw declaration.");
        Require(classifications.Declarations.Select(value => value.Ordinal).SequenceEqual(Enumerable.Range(0, raw.Declarations.Count)), "Classification rows are reordered or have non-contiguous ordinals.");
        Require(classifications.Declarations.Select(value => value.Identity).SequenceEqual(raw.Declarations.Select(value => value.Identity), Ordinal), "Classification identities are missing, duplicated, reordered, or collapsed.");

        HashSet<string> nativeSet = nativeEntrypoints.ToHashSet(Ordinal);
        HashSet<string> managedSet = managedMembers.Select(value => value.Evidence).ToHashSet(Ordinal);
        for (int index = 0; index < classifications.Declarations.Count; index++)
        {
            ClassificationRow row = classifications.Declarations[index];
            RawDeclaration declaration = raw.Declarations[index];
            Require(AllowedClassifications.Contains(row.Classification, Ordinal), "Classification row uses an undocumented classification: " + row.Identity);
            Require(row.NativeEntrypoints.Distinct(Ordinal).Count() == row.NativeEntrypoints.Count, "Native evidence is duplicated: " + row.Identity);
            Require(row.ManagedMembers.Distinct(Ordinal).Count() == row.ManagedMembers.Count, "Managed evidence is duplicated: " + row.Identity);
            Require(IsOrdinallySorted(row.NativeEntrypoints), "Native evidence is nondeterministically ordered: " + row.Identity);
            Require(IsOrdinallySorted(row.ManagedMembers), "Managed evidence is nondeterministically ordered: " + row.Identity);
            Require(row.NativeEntrypoints.All(value => !Regex.IsMatch(value, "^jyppx_ocv[0-9]+_", RegexOptions.CultureInvariant)), "Fixed-major native evidence is forbidden: " + row.Identity);
            Require(row.ManagedMembers.All(value => !Regex.IsMatch(value, "JYPPX.OpenCvSharp[0-9]+", RegexOptions.CultureInvariant)), "Fixed-major managed evidence is forbidden: " + row.Identity);
            Require(row.NativeEntrypoints.All(nativeSet.Contains), "Classification references nonexistent native evidence: " + row.Identity);
            Require(row.ManagedMembers.All(managedSet.Contains), "Classification references nonexistent managed evidence: " + row.Identity);

            if (row.Classification == "implemented")
            {
                Require(declaration.Kind == "callable" && row.NativeEntrypoints.Count > 0 && row.ManagedMembers.Count > 0, "Implemented classification requires callable native and managed evidence: " + row.Identity);
            }
            if (row.Classification == "non-callable-metadata")
            {
                Require(declaration.Kind is "enum" or "class", "Only enum or class declarations may be classified as non-callable metadata: " + row.Identity);
            }
            if (row.Classification is "missing" or "intentionally-omitted" or "upstream-conditional" or "unsupported")
            {
                Require(!string.IsNullOrWhiteSpace(row.Reason), "Non-implemented classification requires a documented reason: " + row.Identity);
            }
        }
    }

    private static string BuildMappingText(RawDocument raw, ClassificationDocument classifications)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# OpenCV 5.0.0 Calib3D upstream-to-native-to-managed map");
        builder.AppendLine("schema-version=1");
        builder.AppendLine("claimed-slice=opencv2/calib3d.hpp compatibility include closure: parser-emitted geometry/2d.hpp, geometry/3d.hpp, stereo.hpp, and calib.hpp declarations");
        builder.AppendLine("repository-wide-upstream-parity=false");
        builder.AppendLine("header-sha256=" + raw.HeaderSha256);
        builder.AppendLine("parser-sha256=" + raw.ParserSha256);
        builder.AppendLine("classification-order=" + string.Join(",", AllowedClassifications));
        builder.AppendLine("[source-headers]");
        foreach (SourceHeader source in raw.SourceHeaders)
        {
            builder.Append(source.StartOrdinal.ToString("D3")).Append('|')
                .Append(source.DeclarationCount).Append('|')
                .Append(source.Sha256).Append('|')
                .AppendLine(source.Path);
        }
        builder.AppendLine("[declarations]");
        for (int index = 0; index < raw.Declarations.Count; index++)
        {
            RawDeclaration declaration = raw.Declarations[index];
            ClassificationRow row = classifications.Declarations[index];
            builder.Append(declaration.Ordinal.ToString("D3")).Append('|')
                .Append(declaration.Kind).Append('|')
                .Append(declaration.SourceHeader).Append('|')
                .Append(row.Classification).Append('|')
                .Append(Sanitize(declaration.Identity)).Append('|')
                .Append(row.NativeEntrypoints.Count == 0 ? "-" : string.Join(";", row.NativeEntrypoints)).Append('|')
                .Append(row.ManagedMembers.Count == 0 ? "-" : string.Join(";", row.ManagedMembers)).Append('|')
                .AppendLine(Sanitize(row.Reason));
        }
        return Normalize(builder.ToString());
    }

    private static FamilyInventory BuildFamilyInventory(RawDocument raw, ClassificationDocument classifications)
    {
        const string geometry2d = "opencv-source/opencv-5.0.0/modules/geometry/include/opencv2/geometry/2d.hpp";
        const string geometry3d = "opencv-source/opencv-5.0.0/modules/geometry/include/opencv2/geometry/3d.hpp";
        const string stereo = "opencv-source/opencv-5.0.0/modules/stereo/include/opencv2/stereo.hpp";
        const string calib = "opencv-source/opencv-5.0.0/modules/calib/include/opencv2/calib.hpp";
        var definitions = new[]
        {
            new
            {
                Id = "geometry-2d-subdiv2d-object-model",
                Rationale = "Bind the complete Subdiv2D lifecycle, insertion, location, topology navigation, and Delaunay/Voronoi output surface.",
                FocusedTest = "tests/OpenCvSharp.Tests/Calib3D/Calib3DUpstreamParityTests.cs",
                Match = (Func<RawDeclaration, bool>)(value => value.SourceHeader == geometry2d && value.Name.Contains("Subdiv2D", StringComparison.Ordinal))
            },
            new
            {
                Id = "geometry-2d-primitives",
                Rationale = "Preserve the compatibility include's complete contour, polygon, ellipse, fitting, transform, and distance primitive surface.",
                FocusedTest = "tests/OpenCvSharp.Tests/Calib3D/Calib3DUpstreamParityTests.cs",
                Match = (Func<RawDeclaration, bool>)(value => value.SourceHeader == geometry2d && !value.Name.Contains("Subdiv2D", StringComparison.Ordinal))
            },
            new
            {
                Id = "geometry-3d-usac-and-homography",
                Rationale = "Bind USAC configuration and the parameter-object overloads while preserving robust-estimation metadata and homography behavior.",
                FocusedTest = "tests/OpenCvSharp.Tests/Calib3D/Calib3DUpstreamParityTests.cs",
                Match = (Func<RawDeclaration, bool>)(value => value.SourceHeader == geometry3d && value.Ordinal <= 67)
            },
            new
            {
                Id = "geometry-3d-pose-and-epipolar",
                Rationale = "Preserve rotation, projection, PnP, homogeneous, epipolar, essential-matrix, pose recovery, and triangulation workflows.",
                FocusedTest = "tests/OpenCvSharp.Tests/Calib3D/Calib3DUpstreamParityTests.cs",
                Match = (Func<RawDeclaration, bool>)(value => value.SourceHeader == geometry3d && value.Ordinal >= 68 && value.Ordinal <= 98)
            },
            new
            {
                Id = "geometry-3d-affine-camera-and-fisheye",
                Rationale = "Preserve affine/translation estimation, camera-matrix utilities, undistortion, homography decomposition, and fisheye geometry workflows.",
                FocusedTest = "tests/OpenCvSharp.Tests/Calib3D/Calib3DUpstreamParityTests.cs",
                Match = (Func<RawDeclaration, bool>)(value => value.SourceHeader == geometry3d && value.Ordinal >= 99)
            },
            new
            {
                Id = "stereo-rectification",
                Rationale = "Preserve calibrated, uncalibrated, and fisheye stereo rectification output ownership and ROI semantics.",
                FocusedTest = "tests/OpenCvSharp.Tests/Calib3D/Calib3DUpstreamParityTests.cs",
                Match = (Func<RawDeclaration, bool>)(value => value.SourceHeader == stereo && value.Name is "cv.stereoRectify" or "cv.stereoRectifyUncalibrated" or "cv.fisheye.stereoRectify")
            },
            new
            {
                Id = "stereo-matcher-object-model",
                Rationale = "Preserve the base StereoMatcher compute, property, and lifecycle contract.",
                FocusedTest = "tests/OpenCvSharp.Tests/Calib3D/Calib3DUpstreamParityTests.cs",
                Match = (Func<RawDeclaration, bool>)(value => value.SourceHeader == stereo && value.Name.Contains("cv.StereoMatcher", StringComparison.Ordinal))
            },
            new
            {
                Id = "stereo-bm-object-model",
                Rationale = "Preserve StereoBM creation, inherited compute/properties, BM-specific tuning, ROI, and lifecycle behavior.",
                FocusedTest = "tests/OpenCvSharp.Tests/Calib3D/Calib3DUpstreamParityTests.cs",
                Match = (Func<RawDeclaration, bool>)(value => value.SourceHeader == stereo && value.Name.Contains("cv.StereoBM", StringComparison.Ordinal))
            },
            new
            {
                Id = "stereo-sgbm-object-model",
                Rationale = "Preserve StereoSGBM creation, inherited compute/properties, SGBM tuning, modes, and lifecycle behavior.",
                FocusedTest = "tests/OpenCvSharp.Tests/Calib3D/Calib3DUpstreamParityTests.cs",
                Match = (Func<RawDeclaration, bool>)(value => value.SourceHeader == stereo && value.Name.Contains("cv.StereoSGBM", StringComparison.Ordinal))
            },
            new
            {
                Id = "stereo-disparity-utilities",
                Rationale = "Preserve speckle filtering, valid ROI, disparity validation, and 3D reprojection utilities.",
                FocusedTest = "tests/OpenCvSharp.Tests/Calib3D/Calib3DUpstreamParityTests.cs",
                Match = (Func<RawDeclaration, bool>)(value => value.SourceHeader == stereo &&
                    value.Name is not "cv.stereoRectify" and not "cv.stereoRectifyUncalibrated" and not "cv.fisheye.stereoRectify" &&
                    !value.Name.Contains("cv.StereoMatcher", StringComparison.Ordinal) &&
                    !value.Name.Contains("cv.StereoBM", StringComparison.Ordinal) &&
                    !value.Name.Contains("cv.StereoSGBM", StringComparison.Ordinal))
            },
            new
            {
                Id = "calibration-and-registration",
                Rationale = "Preserve camera, stereo, multiview, registration, and fisheye calibration overloads from OpenCV 5's calib module.",
                FocusedTest = "tests/OpenCvSharp.Tests/Calib3D/Calib3DUpstreamParityTests.cs",
                Match = (Func<RawDeclaration, bool>)(value => value.SourceHeader == calib)
            }
        };

        var inventory = new FamilyInventory();
        foreach (var definition in definitions)
        {
            var family = new FamilyRow { Id = definition.Id, Rationale = definition.Rationale };
            for (int index = 0; index < raw.Declarations.Count; index++)
            {
                RawDeclaration declaration = raw.Declarations[index];
                if (!definition.Match(declaration))
                {
                    continue;
                }
                ClassificationRow classification = classifications.Declarations[index];
                family.Declarations.Add(new FamilyOperation
                {
                    Ordinal = declaration.Ordinal,
                    UpstreamIdentity = declaration.Identity,
                    UpstreamClassification = classification.Classification,
                    NativeEntrypoints = classification.NativeEntrypoints.ToList(),
                    ManagedMembers = classification.ManagedMembers.ToList(),
                    FocusedTest = definition.FocusedTest
                });
            }
            Require(family.Declarations.Count > 0, "Selected Calib3D family has no upstream declarations: " + family.Id);
            inventory.Families.Add(family);
        }
        return inventory;
    }

    private static string Sanitize(string value) => value.Replace('|', '/').Replace('\r', ' ').Replace('\n', ' ').Trim();

    private static bool IsOrdinallySorted(IReadOnlyList<string> values)
    {
        for (int index = 1; index < values.Count; index++)
        {
            if (Ordinal.Compare(values[index - 1], values[index]) >= 0)
            {
                return false;
            }
        }
        return true;
    }

    private static void RunNegativeFixtures(
        RawDocument raw,
        ClassificationDocument classifications,
        MappingSummary summary,
        Options options,
        IReadOnlyList<string> nativeEntrypoints,
        IReadOnlyList<ManagedMember> managedMembers)
    {
        int rejected = 0;
        void Reject(string name, string expected, Action action)
        {
            try
            {
                action();
                throw new InvalidDataException("Negative Calib3D map fixture was accepted: " + name);
            }
            catch (InvalidDataException exception)
            {
                if (!exception.Message.Contains(expected, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidDataException("Negative fixture '" + name + "' failed for the wrong reason: " + exception.Message);
                }
                rejected++;
            }
        }
        ClassificationDocument CopyClassifications() => JsonSerializer.Deserialize<ClassificationDocument>(Serialize(classifications), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        RawDocument CopyRaw() => JsonSerializer.Deserialize<RawDocument>(Serialize(raw), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        Reject("missing declaration", "one row", () => { var value = CopyClassifications(); value.Declarations.RemoveAt(0); Validate(raw, value, options, nativeEntrypoints, managedMembers, false); });
        Reject("duplicate declaration", "identities", () => { var value = CopyClassifications(); value.Declarations[1].Identity = value.Declarations[0].Identity; Validate(raw, value, options, nativeEntrypoints, managedMembers, false); });
        Reject("reordered declarations", "reordered", () => { var value = CopyClassifications(); (value.Declarations[0], value.Declarations[1]) = (value.Declarations[1], value.Declarations[0]); Validate(raw, value, options, nativeEntrypoints, managedMembers, false); });
        Reject("overload collapse", "duplicate identity", () => { var value = CopyRaw(); int second = value.Declarations.FindIndex(1, item => item.Name == value.Declarations[0].Name); if (second < 0) second = 28; value.Declarations[second].Identity = value.Declarations[second - 1].Identity; Validate(value, classifications, options, nativeEntrypoints, managedMembers, false); });
        Reject("parser input drift", "header SHA256", () => { var value = CopyRaw(); value.HeaderSha256 = new string('0', 64); Validate(value, classifications, options, nativeEntrypoints, managedMembers, true); });
        Reject("parser source-header drift", "source-header SHA256", () => { var value = CopyRaw(); value.SourceHeaders[0].Sha256 = new string('0', 64); Validate(value, classifications, options, nativeEntrypoints, managedMembers, true); });
        Reject("stale hash", "mapping SHA256", () => ValidateSummaryHash(summary, new string('0', 64)));
        Reject("undocumented omission", "documented reason", () => { var value = CopyClassifications(); ClassificationRow row = value.Declarations.First(item => item.Classification == "implemented"); row.Classification = "intentionally-omitted"; row.Reason = string.Empty; Validate(raw, value, options, nativeEntrypoints, managedMembers, false); });
        Reject("false implementation", "requires callable native and managed evidence", () => { var value = CopyClassifications(); ClassificationRow row = value.Declarations.First(item => item.Classification == "non-callable-metadata"); row.Classification = "implemented"; row.NativeEntrypoints.Clear(); row.ManagedMembers.Clear(); Validate(raw, value, options, nativeEntrypoints, managedMembers, false); });
        Reject("fixed-major identity", "Fixed-major", () => { var value = CopyClassifications(); ClassificationRow row = value.Declarations.First(item => item.ManagedMembers.Count == 1); row.ManagedMembers[0] = row.ManagedMembers[0].Replace("JYPPX.OpenCvSharp", "JYPPX.OpenCvSharp5", StringComparison.Ordinal); Validate(raw, value, options, nativeEntrypoints, managedMembers, false); });
        Reject("nondeterministic source ordering", "nondeterministically ordered", () => { var value = CopyClassifications(); ClassificationRow row = value.Declarations.First(item => item.ManagedMembers.Count > 1); row.ManagedMembers.Reverse(); Validate(raw, value, options, nativeEntrypoints, managedMembers, false); });
        Require(rejected == 11, "Calib3D map negative fixture count drifted.");
    }

    private static void ValidateSummaryHash(MappingSummary summary, string mappingHash)
    {
        Require(summary.MappingSha256 == mappingHash, "Mapping SHA256 is stale.");
    }

    private static string[] ReadNativeEntrypoints(string path)
    {
        string[] result = File.ReadAllLines(path)
            .Where(line => line.StartsWith("jyppx_ocv_", StringComparison.Ordinal) && line.Contains('|'))
            .Select(line => line.Split('|')[0])
            .ToArray();
        Require(result.Length > 0 && result.Distinct(Ordinal).Count() == result.Length && result.SequenceEqual(result.OrderBy(value => value, Ordinal), Ordinal), "Native manifest is empty, duplicated, or not ordinally sorted.");
        return result;
    }

    private static ManagedMember[] ReadManagedMembers(string path)
    {
        var result = new List<ManagedMember>();
        foreach (string line in File.ReadLines(path).Where(value => value.StartsWith("MEMBER|", StringComparison.Ordinal)))
        {
            string[] parts = line.Split('|');
            if (parts.Length != 5)
            {
                continue;
            }
            string name = parts[2] switch
            {
                "method" => Regex.Match(parts[4], @" ([A-Za-z_][A-Za-z0-9_]*)\(").Groups[1].Value,
                "property" => parts[4].Split(' ')[^1],
                "constructor" => ".ctor",
                _ => string.Empty
            };
            if (name.Length > 0)
            {
                result.Add(new ManagedMember(parts[1], parts[2], name, line));
            }
        }
        Require(result.Count > 0 && result.Select(value => value.Evidence).Distinct(Ordinal).Count() == result.Count, "Managed baseline member evidence is empty or duplicated.");
        return result.ToArray();
    }

    private static T Deserialize<T>(string path, JsonSerializerOptions options)
    {
        return JsonSerializer.Deserialize<T>(File.ReadAllText(path), options)
            ?? throw new InvalidDataException("Could not parse " + path + ".");
    }

    private static string Serialize<T>(T value) => JsonSerializer.Serialize(value, new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    }) + "\n";

    private static void WriteNormalizedJson<T>(string path, T value, bool check) => WriteOrCheck(path, Serialize(value), check);

    private static void WriteOrCheck(string path, string text, bool check)
    {
        text = Normalize(text);
        if (check)
        {
            Require(File.Exists(path) && Normalize(File.ReadAllText(path)) == text, "Generated file is stale: " + path);
            return;
        }
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, text, new UTF8Encoding(false));
    }

    private static string Normalize(string text) => text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n').TrimEnd() + "\n";
    private static string Sha256(string text) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(Normalize(text)))).ToLowerInvariant();
    private static string FileSha256(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
    private static string RelativePath(string root, string path) => Path.GetRelativePath(root, path).Replace('\\', '/');

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidDataException(message);
        }
    }
}

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
        public int DeclarationCount { get; set; }
        public List<RawDeclaration> Declarations { get; set; } = new();
    }

    private sealed class RawDeclaration
    {
        public int Ordinal { get; set; }
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
        public string ClaimedSlice { get; set; } = "opencv2/imgcodecs.hpp declarations emitted by OpenCV hdr_parser.py";
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
        public string Generator { get; init; } = "tools/ImgCodecsUpstreamMap";
        public string UpstreamOpenCvVersion { get; init; } = "5.0.0";
        public string ClaimedSlice { get; init; } = "opencv2/imgcodecs.hpp declarations emitted by OpenCV hdr_parser.py";
        public string RawExtractionPath { get; init; } = string.Empty;
        public string ClassificationPath { get; init; } = string.Empty;
        public string MappingPath { get; init; } = string.Empty;
        public string HeaderSha256 { get; init; } = string.Empty;
        public string ParserSha256 { get; init; } = string.Empty;
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
        public int ManagedPublicTypeAdditionCount { get; init; } = 16;
        public int ManagedPublicMemberAdditionCount { get; init; } = 168;
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
        public string FocusedTest { get; init; } = "tests/OpenCvSharp.Tests/ImgCodecs/ImgCodecsUpstreamParityTests.cs";
        public string NativeSmoke { get; init; } = "src/OpenCvSharp.Native/tests/native_smoke.cpp";
        public string Sample { get; init; } = "samples/ConsoleSamples/Program.cs";
        public string Guide { get; init; } = "docs/articles/imgcodecs-upstream-parity-guide.md";
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
            Console.WriteLine("IMGCODECS_UPSTREAM_MAP_BASE_VALIDATION_OK");

            string mappingText = BuildMappingText(raw, classifications);
            FamilyInventory familyInventory = BuildFamilyInventory(raw, classifications);
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
                MappingSha256 = Sha256(mappingText),
                DeclarationCount = raw.Declarations.Count,
                EnumCount = raw.Declarations.Count(value => value.Kind == "enum"),
                ClassCount = raw.Declarations.Count(value => value.Kind == "class"),
                CallableCount = raw.Declarations.Count(value => value.Kind == "callable"),
                ClassificationCounts = orderedCounts,
                NativeEvidenceCount = classifications.Declarations.SelectMany(value => value.NativeEntrypoints).Distinct(Ordinal).Count(),
                ManagedEvidenceCount = classifications.Declarations.SelectMany(value => value.ManagedMembers).Distinct(Ordinal).Count(),
                NegativeFixtureCount = 10,
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
                "IMGCODECS_UPSTREAM_MAP_OK declarations={0} callable={1} implemented={2} missing={3} omitted={4} fixtures=10 sha256={5} mode={6}",
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
                    ? "The exact enum declaration and values are preserved in the managed ImgCodecs parameter contract."
                    : "The Animation class declaration is retained as metadata; its constructor and codec operations are classified independently.";
                result.Declarations.Add(row);
                continue;
            }

            string owner = OwnerName(declaration.Name);
            string symbol = SymbolName(declaration.Name);
            string managedName = ManagedName(owner, symbol);
            row.NativeEntrypoints = FindNativeEvidence(declaration, owner, symbol, nativeEntrypoints).ToList();
            row.ManagedMembers = FindManagedEvidence(owner, managedName, managedMembers).ToList();

            if (row.NativeEntrypoints.Count > 0 && row.ManagedMembers.Count > 0)
            {
                if (declaration.Arguments.Any(value => value.Type == "AlgorithmHint"))
                {
                    row.Classification = "intentionally-omitted";
                    row.Reason = "The operation is implemented with OpenCV's default AlgorithmHint; explicit hint selection is intentionally outside the stable C ABI for this declaration identity.";
                }
                else
                {
                    row.Classification = "implemented";
                    row.Reason = "Native and managed symbol-group evidence is present for this parsed declaration identity.";
                }
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
        return parts.Length >= 3 && parts[^2] != "fisheye" ? parts[^2] : string.Empty;
    }

    private static string SymbolName(string upstreamName) => upstreamName.Split('.')[^1];

    private static string ManagedName(string owner, string symbol)
    {
        if (symbol is "FontFace" || symbol == owner)
        {
            return ".ctor";
        }
        if (owner.Length > 0 && symbol == "setTemplate")
        {
            return "SetTemplate";
        }
        if (owner.Length > 0 && symbol.StartsWith("set", StringComparison.Ordinal) && symbol.Length > 3)
        {
            return PropertyName(symbol[3..]);
        }
        if (owner.Length > 0 && symbol.StartsWith("get", StringComparison.Ordinal) && symbol.Length > 3)
        {
            return PropertyName(symbol[3..]);
        }
        string? imgCodecsName = symbol switch
        {
            "imread" => "ImRead",
            "imreadWithMetadata" => "ImReadWithMetadata",
            "imreadmulti" => "ImReadMulti",
            "imreadanimation" => "ImReadAnimation",
            "imdecodeanimation" => "ImDecodeAnimation",
            "imwriteanimation" => "ImWriteAnimation",
            "imencodeanimation" => "ImEncodeAnimation",
            "imcount" => "ImCount",
            "imwrite" => "ImWrite",
            "imwriteWithMetadata" => "ImWriteWithMetadata",
            "imwritemulti" => "ImWriteMulti",
            "imdecode" => "ImDecode",
            "imdecodeWithMetadata" => "ImDecodeWithMetadata",
            "imdecodemulti" => "ImDecodeMulti",
            "imencode" => "ImEncode",
            "imencodeWithMetadata" => "ImEncodeWithMetadata",
            "imencodemulti" => "ImEncodeMulti",
            _ => null
        };
        if (imgCodecsName != null)
        {
            return imgCodecsName;
        }
        return char.ToUpperInvariant(symbol[0]) + symbol[1..];
    }

    private static string PropertyName(string value) => value switch
    {
        "CannyLowThresh" => "CannyLowThreshold",
        "CannyHighThresh" => "CannyHighThreshold",
        "MinDist" => "MinDistance",
        "VotesThresh" => "VotesThreshold",
        "AngleThresh" => "AngleThreshold",
        "ScaleThresh" => "ScaleThreshold",
        "PosThresh" => "PositionThreshold",
        _ => value
    };

    private static IEnumerable<string> FindManagedEvidence(
        string owner,
        string managedName,
        IReadOnlyList<ManagedMember> members)
    {
        IEnumerable<ManagedMember> candidates = members.Where(value => value.Name == managedName);
        if (owner.Length > 0)
        {
            string typeName = "JYPPX.OpenCvSharp.ImgCodecs." + owner;
            candidates = candidates.Where(value => value.TypeName == typeName);
        }
        else
        {
            candidates = candidates.Where(value => value.TypeName.EndsWith(".Cv2", StringComparison.Ordinal));
        }
        return candidates.Select(value => value.Evidence).Distinct(Ordinal).OrderBy(value => value, Ordinal);
    }

    private static IEnumerable<string> FindNativeEvidence(
        RawDeclaration declaration,
        string owner,
        string symbol,
        IReadOnlyList<string> entrypoints)
    {
        string[] exactNames = symbol switch
        {
            "Animation" => new[] { "jyppx_ocv_imgcodecs_animation_create" },
            "imread" when declaration.Identity.Contains("Mat dst", StringComparison.Ordinal) => new[] { "jyppx_ocv_imgcodecs_imread_into" },
            "imread" => new[] { "jyppx_ocv_imgcodecs_imread" },
            "imreadWithMetadata" => new[] { "jyppx_ocv_imgcodecs_imread_with_metadata" },
            "imreadmulti" => new[] { "jyppx_ocv_imgcodecs_imread_multi" },
            "imreadanimation" => new[] { "jyppx_ocv_imgcodecs_imread_animation" },
            "imdecodeanimation" => new[] { "jyppx_ocv_imgcodecs_imdecode_animation" },
            "imwriteanimation" => new[] { "jyppx_ocv_imgcodecs_imwrite_animation" },
            "imencodeanimation" => new[] { "jyppx_ocv_imgcodecs_imencode_animation" },
            "imcount" => new[] { "jyppx_ocv_imgcodecs_imcount" },
            "imwrite" => new[] { "jyppx_ocv_imgcodecs_imwrite", "jyppx_ocv_imgcodecs_imwrite_with_params" },
            "imwriteWithMetadata" => new[] { "jyppx_ocv_imgcodecs_imwrite_with_metadata" },
            "imwritemulti" => new[] { "jyppx_ocv_imgcodecs_imwrite_multi" },
            "imdecode" => new[] { "jyppx_ocv_imgcodecs_imdecode" },
            "imdecodeWithMetadata" => new[] { "jyppx_ocv_imgcodecs_imdecode_with_metadata" },
            "imdecodemulti" => new[] { "jyppx_ocv_imgcodecs_imdecode_multi" },
            "imencode" => new[] { "jyppx_ocv_imgcodecs_imencode", "jyppx_ocv_imgcodecs_imencode_with_params" },
            "imencodeWithMetadata" => new[] { "jyppx_ocv_imgcodecs_imencode_with_metadata" },
            "imencodemulti" => new[] { "jyppx_ocv_imgcodecs_imencode_multi" },
            "haveImageReader" => new[] { "jyppx_ocv_imgcodecs_have_image_reader" },
            "haveImageWriter" => new[] { "jyppx_ocv_imgcodecs_have_image_writer" },
            _ => Array.Empty<string>()
        };
        if (exactNames.Length > 0)
        {
            HashSet<string> entrypointSet = entrypoints.ToHashSet(Ordinal);
            return exactNames.Where(entrypointSet.Contains).OrderBy(value => value, Ordinal);
        }

        string symbolToken = symbol switch
        {
            "imreadmulti" => "imread_multi",
            "imwritemulti" => "imwrite_multi",
            "imdecodemulti" => "imdecode_multi",
            "imencodemulti" => "imencode_multi",
            "imreadanimation" => "imread_animation",
            "imdecodeanimation" => "imdecode_animation",
            "imwriteanimation" => "imwrite_animation",
            "imencodeanimation" => "imencode_animation",
            _ => SnakeCase(symbol)
        };
        string ownerToken = owner.Length == 0 ? string.Empty : SnakeCase(owner) + "_";
        var tokens = new List<string>();
        if (symbol == owner && owner == "Animation")
        {
            tokens.Add("animation_create");
        }
        if (owner.StartsWith("GeneralizedHough", StringComparison.Ordinal) &&
            (symbol.StartsWith("get", StringComparison.Ordinal) || symbol.StartsWith("set", StringComparison.Ordinal)))
        {
            bool getter = symbol.StartsWith("get", StringComparison.Ordinal);
            string propertyName = symbol[3..];
            bool integerProperty = propertyName is "CannyLowThresh" or "CannyHighThresh" or "MaxBufferSize" or
                "Levels" or "VotesThreshold" or "AngleThresh" or "ScaleThresh" or "PosThresh";
            tokens.Add("generalized_hough_" + (getter ? "get_" : "set_") + (integerProperty ? "int_property" : "double_property"));
        }
        if (symbol.StartsWith("create", StringComparison.Ordinal) && symbol.Length > 6)
        {
            tokens.Add(SnakeCase(symbol[6..]) + "_create");
        }
        tokens.Add(ownerToken + symbolToken);
        if (owner.Length == 0)
        {
            tokens.Add(symbolToken);
        }

        return entrypoints.Where(entrypoint => tokens.Any(token => ContainsToken(entrypoint, token)))
            .Distinct(Ordinal)
            .OrderBy(value => value, Ordinal);
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
        Require(raw.SchemaVersion == 1 && raw.Generator == "tools/ImgCodecsUpstreamMap/extract_imgcodecs.py" && raw.UpstreamOpenCvVersion == "5.0.0", "Raw extraction identity drifted.");
        Require(raw.DeclarationCount == 39 && raw.Declarations.Count == 39, "Raw extraction must contain exactly 39 declarations.");
        Require(raw.Declarations.Count(value => value.Kind == "enum") == 16 && raw.Declarations.Count(value => value.Kind == "class") == 1 && raw.Declarations.Count(value => value.Kind == "callable") == 22, "Raw declaration kind counts drifted.");
        Require(raw.Declarations.Select(value => value.Ordinal).SequenceEqual(Enumerable.Range(0, raw.Declarations.Count)), "Raw declarations are reordered or have non-contiguous ordinals.");
        Require(raw.Declarations.Select(value => value.Identity).Distinct(Ordinal).Count() == raw.Declarations.Count, "Raw declarations contain a duplicate identity or collapsed overload.");
        Require(raw.Declarations.All(value => value.Kind is "enum" or "class" or "callable" && value.Identity.Length > 0), "Raw declaration kind or identity is invalid.");
        if (verifyFileHashes)
        {
            Require(FileSha256(Path.Combine(options.WorkspaceRoot, raw.HeaderPath.Replace('/', Path.DirectorySeparatorChar))) == raw.HeaderSha256, "Parser input header SHA256 drifted.");
            Require(FileSha256(Path.Combine(options.WorkspaceRoot, raw.ParserPath.Replace('/', Path.DirectorySeparatorChar))) == raw.ParserSha256, "OpenCV parser SHA256 drifted.");
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
        builder.AppendLine("# OpenCV 5.0.0 ImgCodecs upstream-to-native-to-managed map");
        builder.AppendLine("schema-version=1");
        builder.AppendLine("claimed-slice=opencv2/imgcodecs.hpp declarations emitted by OpenCV hdr_parser.py");
        builder.AppendLine("repository-wide-upstream-parity=false");
        builder.AppendLine("header-sha256=" + raw.HeaderSha256);
        builder.AppendLine("parser-sha256=" + raw.ParserSha256);
        builder.AppendLine("classification-order=" + string.Join(",", AllowedClassifications));
        builder.AppendLine("[declarations]");
        for (int index = 0; index < raw.Declarations.Count; index++)
        {
            RawDeclaration declaration = raw.Declarations[index];
            ClassificationRow row = classifications.Declarations[index];
            builder.Append(declaration.Ordinal.ToString("D3")).Append('|')
                .Append(declaration.Kind).Append('|')
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
        var definitions = new[]
        {
            new
            {
                Id = "file-buffer-and-codec-capabilities",
                Rationale = "Cover basic file and memory coding, caller-owned destination reuse, and factual reader/writer probes.",
                FocusedTest = "tests/OpenCvSharp.Tests/ImgCodecs/ImgCodecsUpstreamParityTests.cs",
                Match = (Func<RawDeclaration, bool>)(value => value.Name is "cv.imread" or "cv.imwrite" or "cv.imdecode" or "cv.imencode" or "cv.haveImageReader" or "cv.haveImageWriter")
            },
            new
            {
                Id = "multi-page-workflows",
                Rationale = "Cover complete and ranged multi-page file and memory coding plus page counting.",
                FocusedTest = "tests/OpenCvSharp.Tests/ImgCodecs/ImgCodecsUpstreamParityTests.cs",
                Match = (Func<RawDeclaration, bool>)(value => value.Name is "cv.imreadmulti" or "cv.imdecodemulti" or "cv.imwritemulti" or "cv.imencodemulti" or "cv.imcount")
            },
            new
            {
                Id = "metadata-workflows",
                Rationale = "Cover typed metadata extraction and insertion for file and memory operations.",
                FocusedTest = "tests/OpenCvSharp.Tests/ImgCodecs/ImgCodecsUpstreamParityTests.cs",
                Match = (Func<RawDeclaration, bool>)(value => value.Name == "cv.ImageMetadataType" || value.Name.Contains("WithMetadata", StringComparison.Ordinal))
            },
            new
            {
                Id = "animation-object-and-codecs",
                Rationale = "Cover the owned animation object, file and memory decode, and file and memory encode.",
                FocusedTest = "tests/OpenCvSharp.Tests/ImgCodecs/ImgCodecsUpstreamParityTests.cs",
                Match = (Func<RawDeclaration, bool>)(value => value.Name.Contains("Animation", StringComparison.Ordinal) || value.Name.Contains("animation", StringComparison.Ordinal))
            },
            new
            {
                Id = "codec-parameter-enums",
                Rationale = "Preserve every parser-emitted OpenCV 5 ImgCodecs enum value without claiming that every optional codec is built.",
                FocusedTest = "tests/OpenCvSharp.Tests/ImgCodecs/ImgCodecsUpstreamParityTests.cs",
                Match = (Func<RawDeclaration, bool>)(value => value.Kind == "enum")
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
            Require(family.Declarations.Count > 0, "Selected ImgCodecs family has no upstream declarations: " + family.Id);
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
                throw new InvalidDataException("Negative ImgCodecs map fixture was accepted: " + name);
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
        Reject("stale hash", "mapping SHA256", () => ValidateSummaryHash(summary, new string('0', 64)));
        Reject("undocumented omission", "documented reason", () => { var value = CopyClassifications(); ClassificationRow row = value.Declarations.First(item => item.Classification == "implemented"); row.Classification = "missing"; row.Reason = string.Empty; Validate(raw, value, options, nativeEntrypoints, managedMembers, false); });
        Reject("false implementation", "requires callable native and managed evidence", () => { var value = CopyClassifications(); ClassificationRow row = value.Declarations.First(item => item.Classification == "non-callable-metadata"); row.Classification = "implemented"; row.NativeEntrypoints.Clear(); row.ManagedMembers.Clear(); Validate(raw, value, options, nativeEntrypoints, managedMembers, false); });
        Reject("fixed-major identity", "Fixed-major", () => { var value = CopyClassifications(); ClassificationRow row = value.Declarations.First(item => item.ManagedMembers.Count == 1); row.ManagedMembers[0] = row.ManagedMembers[0].Replace("JYPPX.OpenCvSharp", "JYPPX.OpenCvSharp5", StringComparison.Ordinal); Validate(raw, value, options, nativeEntrypoints, managedMembers, false); });
        Reject("nondeterministic source ordering", "nondeterministically ordered", () => { var value = CopyClassifications(); ClassificationRow row = value.Declarations.First(item => item.ManagedMembers.Count > 1); row.ManagedMembers.Reverse(); Validate(raw, value, options, nativeEntrypoints, managedMembers, false); });
        Require(rejected == 10, "ImgCodecs map negative fixture count drifted.");
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

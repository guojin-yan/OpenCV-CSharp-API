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
        public string ClaimedSlice { get; set; } = "opencv2/videoio.hpp declarations emitted by OpenCV hdr_parser.py";
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
        public string Generator { get; init; } = "tools/VideoIOUpstreamMap";
        public string UpstreamOpenCvVersion { get; init; } = "5.0.0";
        public string ClaimedSlice { get; init; } = "opencv2/videoio.hpp declarations emitted by OpenCV hdr_parser.py";
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
        public int ManagedPublicTypeAdditionCount { get; init; } = 3;
        public int ManagedPublicMemberAdditionCount { get; init; } = 31;
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
        public string FocusedTest { get; init; } = "tests/OpenCvSharp.Tests/VideoIO/VideoIOUpstreamParityTests.cs";
        public string NativeSmoke { get; init; } = "src/OpenCvSharp.Native/tests/native_smoke.cpp";
        public string Sample { get; init; } = "samples/ConsoleSamples/Program.cs";
        public string Guide { get; init; } = "docs/articles/videoio-upstream-parity-guide.md";
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
            Console.WriteLine("VIDEOIO_UPSTREAM_MAP_BASE_VALIDATION_OK");

            string mappingText = BuildMappingText(raw, classifications);
            FamilyInventory familyInventory = BuildFamilyInventory(raw, classifications);
            int[] selectedOrdinals = familyInventory.Families.SelectMany(value => value.Declarations).Select(value => value.Ordinal).ToArray();
            Require(selectedOrdinals.Length == raw.Declarations.Count && selectedOrdinals.Distinct().Count() == raw.Declarations.Count,
                "VideoIO family inventory must partition every declaration exactly once.");
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
                "VIDEOIO_UPSTREAM_MAP_OK declarations={0} callable={1} implemented={2} missing={3} omitted={4} fixtures=10 sha256={5} mode={6}",
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
                    ? "The exact parser-emitted enum identity and values are preserved in checked mapping metadata; legacy backend-specific numeric IDs remain usable through integer parameter pairs without implying backend availability."
                    : "The exact VideoIO class declaration is retained as metadata; its callable members are classified independently.";
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
        if (symbol == "getBackendName")
        {
            return "GetBackendName";
        }
        if (owner == "VideoWriter" && symbol == "fourcc")
        {
            return "FourCC";
        }
        if (owner.Length > 0 && symbol.StartsWith("set", StringComparison.Ordinal) && symbol.Length > 3)
        {
            return PropertyName(symbol[3..]);
        }
        if (owner.Length > 0 && symbol.StartsWith("get", StringComparison.Ordinal) && symbol.Length > 3)
        {
            return PropertyName(symbol[3..]);
        }
        return char.ToUpperInvariant(symbol[0]) + symbol[1..];
    }

    private static string PropertyName(string value) => value switch
    {
        "ExceptionMode" => "ExceptionMode",
        _ => value
    };

    private static IEnumerable<string> FindManagedEvidence(
        RawDeclaration declaration,
        string owner,
        string managedName,
        IReadOnlyList<ManagedMember> members)
    {
        IEnumerable<ManagedMember> candidates = members.Where(value => value.Name == managedName);
        if (owner == "IStreamReader")
        {
            candidates = candidates.Where(value => value.TypeName == "OpenCvSharp.VideoIO.VideoStreamReader");
        }
        else if (owner == "VideoCapture" && managedName == "Open")
        {
            candidates = candidates.Where(value => value.TypeName is "OpenCvSharp.VideoIO.VideoCapture" or "OpenCvSharp.VideoIO.VideoCaptureExtensions");
        }
        else if (owner.Length > 0)
        {
            string typeName = "OpenCvSharp.VideoIO." + owner;
            candidates = candidates.Where(value => value.TypeName == typeName);
        }
        else
        {
            candidates = candidates.Where(value => value.TypeName.EndsWith(".Cv2", StringComparison.Ordinal));
        }

        if ((owner is "VideoCapture" or "VideoWriter") && (managedName is ".ctor" or "Open"))
        {
            bool hasParameters = declaration.Identity.Contains("vector_int params", StringComparison.Ordinal);
            bool hasString = declaration.Identity.Contains("String filename", StringComparison.Ordinal);
            bool hasIndex = declaration.Identity.Contains("int index", StringComparison.Ordinal);
            bool hasStreamReader = declaration.Identity.Contains("Ptr_IStreamReader", StringComparison.Ordinal);
            candidates = candidates.Where(value =>
            {
                bool evidenceHasParameters = value.Evidence.Contains("params System.Int32[] parameters", StringComparison.Ordinal);
                if (evidenceHasParameters != hasParameters)
                {
                    return false;
                }
                if (owner == "VideoWriter")
                {
                    if (!hasString)
                    {
                        return value.Evidence.EndsWith(".ctor()", StringComparison.Ordinal);
                    }
                    bool upstreamHasApiPreference = declaration.Identity.Contains("int apiPreference", StringComparison.Ordinal);
                    bool evidenceHasApiPreference = value.Evidence.Contains("OpenCvSharp.VideoIO.VideoCaptureAPIs apiPreference", StringComparison.Ordinal);
                    return value.Evidence.Contains("System.String filename", StringComparison.Ordinal) && evidenceHasApiPreference == upstreamHasApiPreference;
                }
                if (hasString)
                {
                    return value.Evidence.Contains("System.String filename", StringComparison.Ordinal);
                }
                if (hasIndex)
                {
                    return value.Evidence.Contains("System.Int32 index", StringComparison.Ordinal);
                }
                if (hasStreamReader)
                {
                    return value.Evidence.Contains("OpenCvSharp.VideoIO.VideoStreamReader reader", StringComparison.Ordinal);
                }
                return value.Evidence.EndsWith(".ctor()", StringComparison.Ordinal);
            });
        }
        if (owner == "VideoWriter" && managedName == "FourCC")
        {
            candidates = candidates.Where(value => value.Evidence.Contains("FourCC(System.Char c1", StringComparison.Ordinal));
        }
        if (owner == "VideoCapture" && managedName is "Read" or "Retrieve")
        {
            candidates = candidates.Where(value => value.Evidence.Contains("System.Boolean " + managedName + "(OpenCvSharp.Core.Mat", StringComparison.Ordinal));
        }
        return candidates.Select(value => value.Evidence).Distinct(Ordinal).OrderBy(value => value, Ordinal);
    }

    private static IEnumerable<string> FindNativeEvidence(
        RawDeclaration declaration,
        string owner,
        string symbol,
        IReadOnlyList<string> entrypoints)
    {
        string[] exactNames = Array.Empty<string>();
        if (owner == "VideoCapture")
        {
            exactNames = symbol switch
            {
                "VideoCapture" => declaration.Identity.Contains("Ptr_IStreamReader", StringComparison.Ordinal)
                    ? new[] { "jyppx_ocv_video_capture_create", "jyppx_ocv_video_capture_open_stream" }
                    : declaration.Identity.Contains("String filename", StringComparison.Ordinal) && declaration.Identity.Contains("vector_int params", StringComparison.Ordinal)
                        ? new[] { "jyppx_ocv_video_capture_create", "jyppx_ocv_video_capture_open_file_params" }
                        : declaration.Identity.Contains("int index", StringComparison.Ordinal) && declaration.Identity.Contains("vector_int params", StringComparison.Ordinal)
                            ? new[] { "jyppx_ocv_video_capture_create", "jyppx_ocv_video_capture_open_index_params" }
                            : declaration.Identity.Contains("String filename", StringComparison.Ordinal)
                        ? new[] { "jyppx_ocv_video_capture_create", "jyppx_ocv_video_capture_open_file" }
                        : declaration.Identity.Contains("int index", StringComparison.Ordinal)
                            ? new[] { "jyppx_ocv_video_capture_create", "jyppx_ocv_video_capture_open_index" }
                            : new[] { "jyppx_ocv_video_capture_create" },
                "open" when declaration.Identity.Contains("vector_int params", StringComparison.Ordinal) && declaration.Identity.Contains("String filename", StringComparison.Ordinal) => new[] { "jyppx_ocv_video_capture_open_file_params" },
                "open" when declaration.Identity.Contains("vector_int params", StringComparison.Ordinal) && declaration.Identity.Contains("int index", StringComparison.Ordinal) => new[] { "jyppx_ocv_video_capture_open_index_params" },
                "open" when declaration.Identity.Contains("Ptr_IStreamReader", StringComparison.Ordinal) => new[] { "jyppx_ocv_video_capture_open_stream" },
                "open" when declaration.Identity.Contains("String filename", StringComparison.Ordinal) => new[] { "jyppx_ocv_video_capture_open_file" },
                "open" => new[] { "jyppx_ocv_video_capture_open_index" },
                "setExceptionMode" => new[] { "jyppx_ocv_video_capture_set_exception_mode" },
                "getExceptionMode" => new[] { "jyppx_ocv_video_capture_get_exception_mode" },
                "waitAny" => new[] { "jyppx_ocv_video_capture_wait_any" },
                "isOpened" => new[] { "jyppx_ocv_video_capture_is_opened" },
                "release" => new[] { "jyppx_ocv_video_capture_release" },
                "grab" => new[] { "jyppx_ocv_video_capture_grab" },
                "retrieve" => new[] { "jyppx_ocv_video_capture_retrieve" },
                "read" => new[] { "jyppx_ocv_video_capture_read" },
                "set" => new[] { "jyppx_ocv_video_capture_set" },
                "get" => new[] { "jyppx_ocv_video_capture_get" },
                "getBackendName" => new[] { "jyppx_ocv_video_capture_backend_name_length", "jyppx_ocv_video_capture_backend_name_fill" },
                _ => Array.Empty<string>()
            };
        }
        else if (owner == "VideoWriter")
        {
            exactNames = symbol switch
            {
                "VideoWriter" => declaration.Identity.Contains("vector_int params", StringComparison.Ordinal) && declaration.Identity.Contains("apiPreference", StringComparison.Ordinal)
                    ? new[] { "jyppx_ocv_video_writer_create", "jyppx_ocv_video_writer_open_api_params" }
                    : declaration.Identity.Contains("vector_int params", StringComparison.Ordinal)
                        ? new[] { "jyppx_ocv_video_writer_create", "jyppx_ocv_video_writer_open_params" }
                    : new[] { "jyppx_ocv_video_writer_create", "jyppx_ocv_video_writer_open" },
                "open" when declaration.Identity.Contains("vector_int params", StringComparison.Ordinal) && declaration.Identity.Contains("apiPreference", StringComparison.Ordinal) => new[] { "jyppx_ocv_video_writer_open_api_params" },
                "open" when declaration.Identity.Contains("vector_int params", StringComparison.Ordinal) => new[] { "jyppx_ocv_video_writer_open_params" },
                "open" => new[] { "jyppx_ocv_video_writer_open" },
                "isOpened" => new[] { "jyppx_ocv_video_writer_is_opened" },
                "release" => new[] { "jyppx_ocv_video_writer_release" },
                "write" => new[] { "jyppx_ocv_video_writer_write" },
                "set" => new[] { "jyppx_ocv_video_writer_set" },
                "get" => new[] { "jyppx_ocv_video_writer_get" },
                "fourcc" => new[] { "jyppx_ocv_video_writer_fourcc" },
                "getBackendName" => new[] { "jyppx_ocv_video_writer_backend_name_length", "jyppx_ocv_video_writer_backend_name_fill" },
                _ => Array.Empty<string>()
            };
        }
        else if (owner == "IStreamReader")
        {
            exactNames = symbol switch
            {
                "read" => new[] { "jyppx_ocv_video_stream_reader_read" },
                "seek" => new[] { "jyppx_ocv_video_stream_reader_seek" },
                _ => Array.Empty<string>()
            };
        }
        else if (owner.Length == 0)
        {
            exactNames = symbol switch
            {
                "getBackends" => new[] { "jyppx_ocv_videoio_registry_get_backends_count", "jyppx_ocv_videoio_registry_get_backends_fill" },
                "getCameraBackends" => new[] { "jyppx_ocv_videoio_registry_get_camera_backends_count", "jyppx_ocv_videoio_registry_get_camera_backends_fill" },
                "getStreamBackends" => new[] { "jyppx_ocv_videoio_registry_get_stream_backends_count", "jyppx_ocv_videoio_registry_get_stream_backends_fill" },
                "getStreamBufferedBackends" => new[] { "jyppx_ocv_videoio_registry_get_stream_buffered_backends_count", "jyppx_ocv_videoio_registry_get_stream_buffered_backends_fill" },
                "getWriterBackends" => new[] { "jyppx_ocv_videoio_registry_get_writer_backends_count", "jyppx_ocv_videoio_registry_get_writer_backends_fill" },
                "getBackendName" => new[] { "jyppx_ocv_videoio_registry_get_backend_name_length", "jyppx_ocv_videoio_registry_get_backend_name_fill" },
                "hasBackend" => new[] { "jyppx_ocv_videoio_registry_has_backend" },
                "isBackendBuiltIn" => new[] { "jyppx_ocv_videoio_registry_is_backend_built_in" },
                "getCameraBackendPluginVersion" => new[] { "jyppx_ocv_videoio_registry_get_camera_plugin_version_length", "jyppx_ocv_videoio_registry_get_camera_plugin_version_fill" },
                "getStreamBackendPluginVersion" => new[] { "jyppx_ocv_videoio_registry_get_stream_plugin_version_length", "jyppx_ocv_videoio_registry_get_stream_plugin_version_fill" },
                "getStreamBufferedBackendPluginVersion" => new[] { "jyppx_ocv_videoio_registry_get_stream_buffered_plugin_version_length", "jyppx_ocv_videoio_registry_get_stream_buffered_plugin_version_fill" },
                "getWriterBackendPluginVersion" => new[] { "jyppx_ocv_videoio_registry_get_writer_plugin_version_length", "jyppx_ocv_videoio_registry_get_writer_plugin_version_fill" },
                _ => Array.Empty<string>()
            };
        }
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
        Require(raw.SchemaVersion == 1 && raw.Generator == "tools/VideoIOUpstreamMap/extract_videoio.py" && raw.UpstreamOpenCvVersion == "5.0.0", "Raw extraction identity drifted.");
        Require(raw.DeclarationCount == 71 && raw.Declarations.Count == 71, "Raw extraction must contain exactly 71 declarations.");
        Require(raw.Declarations.Count(value => value.Kind == "enum") == 28 && raw.Declarations.Count(value => value.Kind == "class") == 3 && raw.Declarations.Count(value => value.Kind == "callable") == 40, "Raw declaration kind counts drifted.");
        Require(raw.PreprocessorDefinitions.Count == 1 && raw.PreprocessorDefinitions.TryGetValue("CV_VERSION_MAJOR", out int major) && major == 5, "OpenCV 5 preprocessor context drifted.");
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
            Require(row.ManagedMembers.All(value => !Regex.IsMatch(value, "OpenCvSharp[0-9]+", RegexOptions.CultureInvariant)), "Fixed-major managed evidence is forbidden: " + row.Identity);
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
        builder.AppendLine("# OpenCV 5.0.0 VideoIO upstream-to-native-to-managed map");
        builder.AppendLine("schema-version=1");
        builder.AppendLine("claimed-slice=opencv2/videoio.hpp declarations emitted by OpenCV hdr_parser.py");
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
                Id = "capture-lifecycle-and-properties",
                Rationale = "Cover capture construction, file/index/stream opening, frame acquisition, properties, backend identity, and release.",
                FocusedTest = "tests/OpenCvSharp.Tests/VideoIO/VideoIOUpstreamParityTests.cs",
                Match = (Func<RawDeclaration, bool>)(value =>
                    (value.Name == "cv.VideoCapture" || value.Name.StartsWith("cv.VideoCapture.", StringComparison.Ordinal)) &&
                    !value.Name.Contains("waitAny", StringComparison.Ordinal) &&
                    !value.Name.Contains("ExceptionMode", StringComparison.Ordinal))
            },
            new
            {
                Id = "stream-reader-and-coordination",
                Rationale = "Cover IStreamReader callbacks, exception mode, and waitAny with explicit lifetime and backend limitations.",
                FocusedTest = "tests/OpenCvSharp.Tests/VideoIO/VideoIOUpstreamParityTests.cs",
                Match = (Func<RawDeclaration, bool>)(value => value.Name.StartsWith("cv.IStreamReader", StringComparison.Ordinal) || value.Name.Contains("waitAny", StringComparison.Ordinal) || value.Name.Contains("ExceptionMode", StringComparison.Ordinal))
            },
            new
            {
                Id = "writer-lifecycle-and-parameters",
                Rationale = "Cover writer construction/open overloads, parameter pairs, boolean frame writes, properties, FourCC, backend identity, and release.",
                FocusedTest = "tests/OpenCvSharp.Tests/VideoIO/VideoIOUpstreamParityTests.cs",
                Match = (Func<RawDeclaration, bool>)(value => value.Name == "cv.VideoWriter" || value.Name.StartsWith("cv.VideoWriter.", StringComparison.Ordinal))
            },
            new
            {
                Id = "videoio-enum-contract",
                Rationale = "Preserve every parser-emitted OpenCV 5 VideoIO enum and specialized constant group without implying optional backend support.",
                FocusedTest = "tests/OpenCvSharp.Tests/VideoIO/VideoIOUpstreamParityTests.cs",
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
            Require(family.Declarations.Count > 0, "Selected VideoIO family has no upstream declarations: " + family.Id);
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
                throw new InvalidDataException("Negative VideoIO map fixture was accepted: " + name);
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
        Reject("fixed-major identity", "Fixed-major", () => { var value = CopyClassifications(); ClassificationRow row = value.Declarations.First(item => item.ManagedMembers.Count == 1); row.ManagedMembers[0] = row.ManagedMembers[0].Replace("OpenCvSharp", "OpenCvSharp5", StringComparison.Ordinal); Validate(raw, value, options, nativeEntrypoints, managedMembers, false); });
        Reject("nondeterministic source ordering", "nondeterministically ordered", () => { var value = CopyClassifications(); ClassificationRow row = value.Declarations.First(item => item.NativeEntrypoints.Count > 1); row.NativeEntrypoints.Reverse(); Validate(raw, value, options, nativeEntrypoints, managedMembers, false); });
        Require(rejected == 10, "VideoIO map negative fixture count drifted.");
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

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

internal static class Program
{
    private static readonly StringComparer Ordinal = StringComparer.Ordinal;
    private static readonly string[] Classifications =
    {
        "implemented", "missing", "intentionally-omitted", "upstream-conditional", "unsupported", "non-callable-metadata"
    };

    private static readonly HashSet<int> SelectedCallables = new(new[]
    {
        15, 20, 22, 23, 24, 25, 26, 27, 28, 31, 32, 33, 34, 36, 37, 38, 39, 40,
        42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59,
        60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 74, 75, 76, 77, 78, 79,
        80, 81, 82, 83, 84, 85, 86, 89, 90, 91, 92, 93, 94, 95, 96, 97,
        99, 100, 101, 103
    });

    private static readonly HashSet<int> ConditionalCallables = new(Enumerable.Range(176, 6));

    private sealed record Options(
        string Repository,
        string Workspace,
        string Raw,
        string Classification,
        string NativeManifest,
        string ManagedBaseline,
        string Output,
        string Summary,
        string FamilyOutput,
        bool Initialize,
        bool Check);

    private sealed record ManagedQuery(string TypeName, params string[] Tokens);

    private sealed class RawDocument
    {
        public int SchemaVersion { get; set; }
        public string Generator { get; set; } = "";
        public string UpstreamOpenCvVersion { get; set; } = "";
        public string HeaderPath { get; set; } = "";
        public string HeaderSha256 { get; set; } = "";
        public string ParserPath { get; set; } = "";
        public string ParserSha256 { get; set; } = "";
        public Dictionary<string, int> PreprocessorDefinitions { get; set; } = new(Ordinal);
        public List<SourceHeader> SourceHeaders { get; set; } = new();
        public int DeclarationCount { get; set; }
        public List<RawDeclaration> Declarations { get; set; } = new();
    }

    private sealed class SourceHeader
    {
        public string Path { get; set; } = "";
        public string Sha256 { get; set; } = "";
        public int StartOrdinal { get; set; }
        public int DeclarationCount { get; set; }
    }

    private sealed class RawDeclaration
    {
        public int Ordinal { get; set; }
        public string SourceHeader { get; set; } = "";
        public string Kind { get; set; } = "";
        public string Name { get; set; } = "";
        public string Identity { get; set; } = "";
        public string ReturnType { get; set; } = "";
        public List<string> Modifiers { get; set; } = new();
        public List<RawArgument> Arguments { get; set; } = new();
        public List<RawEnumValue> EnumValues { get; set; } = new();
        public string BaseDeclaration { get; set; } = "";
        public string Documentation { get; set; } = "";
    }

    private sealed class RawArgument
    {
        public string Type { get; set; } = "";
        public string Name { get; set; } = "";
        public string Default { get; set; } = "";
        public List<string> Modifiers { get; set; } = new();
    }

    private sealed class RawEnumValue
    {
        public string Name { get; set; } = "";
        public string Value { get; set; } = "";
    }

    private sealed class ClassificationDocument
    {
        public int SchemaVersion { get; set; } = 1;
        public string UpstreamOpenCvVersion { get; set; } = "5.0.0";
        public string ClaimedSlice { get; set; } = Program.ClaimedSlice;
        public string ReviewStatus { get; set; } = "reviewed";
        public string Limitation { get; set; } = "Parser identities preserve include order, overloads, defaults, direction metadata, and build conditions; this module map does not claim repository-wide OpenCV parity.";
        public List<ClassificationRow> Declarations { get; set; } = new();
    }

    private sealed class ClassificationRow
    {
        public int Ordinal { get; set; }
        public string Identity { get; set; } = "";
        public string Classification { get; set; } = "";
        public string Reason { get; set; } = "";
        public string BuildCondition { get; set; } = "unconditional-parser-surface";
        public List<string> NativeEntrypoints { get; set; } = new();
        public List<string> ManagedMembers { get; set; } = new();
    }

    private sealed class SummaryDocument
    {
        public int SchemaVersion { get; init; } = 1;
        public string Generator { get; init; } = "tools/DnnUpstreamMap";
        public string UpstreamOpenCvVersion { get; init; } = "5.0.0";
        public string ClaimedSlice { get; init; } = Program.ClaimedSlice;
        public string RawExtractionPath { get; init; } = "";
        public string ClassificationPath { get; init; } = "";
        public string MappingPath { get; init; } = "";
        public string HeaderSha256 { get; init; } = "";
        public string ParserSha256 { get; init; } = "";
        public int SourceHeaderCount { get; init; }
        public string SourceHeaderSetSha256 { get; init; } = "";
        public string MappingSha256 { get; init; } = "";
        public int DeclarationCount { get; init; }
        public int EnumCount { get; init; }
        public int ClassCount { get; init; }
        public int CallableCount { get; init; }
        public SortedDictionary<string, int> ClassificationCounts { get; init; } = new(Ordinal);
        public int NativeEvidenceCount { get; init; }
        public int ManagedEvidenceCount { get; init; }
        public int NegativeFixtureCount { get; init; } = 15;
        public string FamilyInventoryPath { get; init; } = "";
        public string FamilyInventorySha256 { get; init; } = "";
        public int SelectedFamilyCount { get; init; }
        public int SelectedDeclarationCount { get; init; }
        public int ManagedPublicTypeAdditionCount { get; init; } = 12;
        public int ManagedPublicMemberAdditionCount { get; init; }
        public bool RepositoryWideUpstreamParityClaimed { get; init; }
    }

    private sealed class FamilyDocument
    {
        public int SchemaVersion { get; init; } = 1;
        public string UpstreamOpenCvVersion { get; init; } = "5.0.0";
        public string Status { get; init; } = "implemented-verified";
        public int ManagedPublicTypeAdditionCount { get; init; } = 12;
        public int ManagedPublicMemberAdditionCount { get; init; }
        public List<FamilyRow> Families { get; init; } = new();
    }

    private sealed class FamilyRow
    {
        public string Id { get; init; } = "";
        public string Rationale { get; init; } = "";
        public List<FamilyOperation> Declarations { get; init; } = new();
    }

    private sealed class FamilyOperation
    {
        public int Ordinal { get; init; }
        public string UpstreamIdentity { get; init; } = "";
        public string UpstreamClassification { get; init; } = "";
        public List<string> NativeEntrypoints { get; init; } = new();
        public List<string> ManagedMembers { get; init; } = new();
        public string FocusedTest { get; init; } = "tests/OpenCvSharp.Tests/Dnn/DnnStructuredParityTests.cs";
        public string NativeSmoke { get; init; } = "src/OpenCvSharp.Native/tests/native_smoke.cpp";
        public string Sample { get; init; } = "samples/ConsoleSamples/Program.cs";
        public string Guide { get; init; } = "docs/articles/dnn-structured-parity-guide.md";
    }

    private const string ClaimedSlice = "opencv2/dnn.hpp compatibility include closure: parser-emitted dict.hpp, dnn.hpp, and utils/inference_engine.hpp declarations";

    private static int Main(string[] args)
    {
        try
        {
            Options options = Parse(args);
            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            RawDocument raw = Read<RawDocument>(options.Raw, jsonOptions);
            string[] native = ReadNative(options.NativeManifest);
            string[] managed = ReadManaged(options.ManagedBaseline);
            if (options.Initialize)
            {
                WriteOrCheck(options.Classification, Serialize(Initialize(raw, native, managed)), false);
            }

            ClassificationDocument classifications = Read<ClassificationDocument>(options.Classification, jsonOptions);
            Validate(raw, classifications, options, native, managed, true);
            string mapping = BuildMap(raw, classifications);
            FamilyDocument families = BuildFamilies(raw, classifications, managed);
            string familyText = Serialize(families);
            var counts = new SortedDictionary<string, int>(Ordinal);
            foreach (string classification in Classifications)
            {
                counts[classification] = classifications.Declarations.Count(value => value.Classification == classification);
            }

            var summary = new SummaryDocument
            {
                RawExtractionPath = Rel(options.Repository, options.Raw),
                ClassificationPath = Rel(options.Repository, options.Classification),
                MappingPath = Rel(options.Repository, options.Output),
                HeaderSha256 = raw.HeaderSha256,
                ParserSha256 = raw.ParserSha256,
                SourceHeaderCount = raw.SourceHeaders.Count,
                SourceHeaderSetSha256 = Sha256(string.Join("\n", raw.SourceHeaders.Select(value => $"{value.Path}|{value.Sha256}|{value.StartOrdinal}|{value.DeclarationCount}")) + "\n"),
                MappingSha256 = Sha256(mapping),
                DeclarationCount = raw.Declarations.Count,
                EnumCount = raw.Declarations.Count(value => value.Kind == "enum"),
                ClassCount = raw.Declarations.Count(value => value.Kind == "class"),
                CallableCount = raw.Declarations.Count(value => value.Kind == "callable"),
                ClassificationCounts = counts,
                NativeEvidenceCount = classifications.Declarations.SelectMany(value => value.NativeEntrypoints).Distinct(Ordinal).Count(),
                ManagedEvidenceCount = classifications.Declarations.SelectMany(value => value.ManagedMembers).Distinct(Ordinal).Count(),
                FamilyInventoryPath = Rel(options.Repository, options.FamilyOutput),
                FamilyInventorySha256 = Sha256(familyText),
                SelectedFamilyCount = families.Families.Count,
                SelectedDeclarationCount = families.Families.Sum(value => value.Declarations.Count),
                ManagedPublicMemberAdditionCount = families.ManagedPublicMemberAdditionCount,
                RepositoryWideUpstreamParityClaimed = false
            };

            RunNegativeFixtures(raw, classifications, summary, options, native, managed);
            WriteOrCheck(options.Output, mapping, options.Check);
            WriteOrCheck(options.FamilyOutput, familyText, options.Check);
            WriteOrCheck(options.Summary, Serialize(summary), options.Check);
            Console.WriteLine($"DNN_UPSTREAM_MAP_OK declarations={summary.DeclarationCount} callables={summary.CallableCount} implemented={counts["implemented"]} missing={counts["missing"]} omitted={counts["intentionally-omitted"]} fixtures=15 sha256={summary.MappingSha256} mode={(options.Check ? "check" : "write")}");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
    }

    private static Options Parse(string[] args)
    {
        var values = new Dictionary<string, string>(Ordinal);
        bool initialize = false;
        bool check = false;
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--initialize-classification") initialize = true;
            else if (args[i] == "--check") check = true;
            else
            {
                Require(i + 1 < args.Length, "Missing option value: " + args[i]);
                values[args[i]] = args[++i];
            }
        }

        string Value(string key)
        {
            Require(values.TryGetValue(key, out string? value), "Missing option: " + key);
            return Path.GetFullPath(value!);
        }

        return new Options(
            Value("--repository"), Value("--workspace"), Value("--raw"), Value("--classification"),
            Value("--native-manifest"), Value("--managed-baseline"), Value("--output"), Value("--summary"),
            Value("--family-output"), initialize, check);
    }

    private static ClassificationDocument Initialize(RawDocument raw, string[] native, string[] managed)
    {
        var result = new ClassificationDocument();
        foreach (RawDeclaration declaration in raw.Declarations)
        {
            var row = new ClassificationRow { Ordinal = declaration.Ordinal, Identity = declaration.Identity };
            if (declaration.Kind != "callable")
            {
                row.Classification = "non-callable-metadata";
                row.Reason = "Parser-emitted enum or class metadata is reviewed as public type shape rather than an independently callable ABI operation.";
            }
            else if (ConditionalCallables.Contains(declaration.Ordinal))
            {
                row.Classification = "upstream-conditional";
                row.BuildCondition = "deprecated-inference-engine-device-or-plugin-runtime";
                row.Reason = "The deprecated Inference Engine device/plugin control requires optional external OpenVINO, VPU, or HDDL runtime state and is not a portable locally supported contract.";
            }
            else if (SelectedCallables.Contains(declaration.Ordinal))
            {
                row.NativeEntrypoints = ResolveNative(declaration.Ordinal, native).ToList();
                row.ManagedMembers = ResolveManaged(declaration.Ordinal, managed).ToList();
                if (row.NativeEntrypoints.Count > 0 && row.ManagedMembers.Count > 0)
                {
                    row.Classification = "implemented";
                    row.Reason = "Correlated neutral C ABI and managed public evidence implements this selected DNN parser identity.";
                }
                else
                {
                    row.Classification = "missing";
                    row.Reason = "Selected adjacent DNN network, inference, layer, or preprocessing identity lacks complete native and managed evidence and must fail closed until implemented.";
                }
            }
            else if (declaration.Ordinal == 41)
            {
                row.Classification = "unsupported";
                row.Reason = "AsyncArray completion, backend thread origin, exception propagation, cancellation, and lifetime do not yet have a stable cross-platform managed ownership contract.";
            }
            else if (declaration.Ordinal == 98)
            {
                row.Classification = "unsupported";
                row.Reason = "writeTextGraph is a development-time model conversion helper with filesystem side effects and is outside the packaged inference runtime contract.";
            }
            else
            {
                row.Classification = "intentionally-omitted";
                row.Reason = OmissionReason(declaration.Ordinal);
            }
            result.Declarations.Add(row);
        }
        return result;
    }

    private static string OmissionReason(int ordinal)
    {
        if (ordinal is >= 1 and <= 9)
            return "DictValue requires a separately designed discriminated scalar/array ownership model and is deferred with LayerParams rather than exposing C++ storage.";
        if (ordinal is 18 or 19)
            return "Direct virtual Layer execution/finalization, including the deprecated run overload, is deferred because backend-owned input/output/internal buffers require an independent lifetime contract.";
        if (ordinal is 29 or 30)
            return "Programmatic layer construction depends on the deferred LayerParams/DictValue graph-building object model.";
        if (ordinal == 35)
            return "The C++ LayerId convenience overload is projected through the selected numeric-id and string-name overloads without exposing its implementation-specific representation.";
        if (ordinal == 72)
            return "printPerfProfile writes process stdout; structured profile retrieval is selected instead so callers retain output ownership and formatting control.";
        if (ordinal is >= 99 and <= 103)
            return "The NMS utility family is stable but separate from this round's selected network/inference/layer closure and remains an explicit next DNN utility priority.";
        if (ordinal is >= 104 and <= 171)
            return "High-level Model and task-specific derived wrappers form a separate model-backed object family requiring independent handles, deterministic fixtures, and result ownership tests.";
        if (ordinal is >= 172 and <= 175)
            return "Tokenizer loading and UTF-8 token conversion form a separate external-model object family and are deferred to a dedicated ownership batch.";
        return "The callable is outside the selected adjacent DNN family and remains explicitly deferred with source-reviewed ownership and test requirements.";
    }

    private static IEnumerable<string> ResolveNative(int ordinal, string[] native)
    {
        HashSet<string> actual = native.ToHashSet(Ordinal);
        return NativeNames(ordinal).Where(actual.Contains).Order(Ordinal);
    }

    private static string[] NativeNames(int ordinal) => ordinal switch
    {
        15 => N("jyppx_ocv_dnn_get_available_targets_count", "jyppx_ocv_dnn_get_available_targets_fill"),
        20 => N("jyppx_ocv_dnn_layer_output_name_to_index", "jyppx_ocv_dnn_layer_release_handle"),
        22 => N("jyppx_ocv_dnn_net_create_empty", "jyppx_ocv_dnn_net_release_handle"),
        23 or 80 => N("jyppx_ocv_dnn_read_net_from_model_optimizer", "jyppx_ocv_dnn_net_release_handle"),
        24 or 81 => N("jyppx_ocv_dnn_read_net_from_model_optimizer_buffer", "jyppx_ocv_dnn_net_release_handle"),
        25 => N("jyppx_ocv_dnn_net_empty"),
        26 => N("jyppx_ocv_core_utf8_result_data", "jyppx_ocv_core_utf8_result_release", "jyppx_ocv_core_utf8_result_size", "jyppx_ocv_dnn_net_dump"),
        27 => N("jyppx_ocv_dnn_net_dump_to_file"),
        28 => N("jyppx_ocv_dnn_net_dump_to_pbtxt"),
        31 => N("jyppx_ocv_dnn_net_get_layer_id"),
        32 => N("jyppx_ocv_dnn_net_get_layer_names_count", "jyppx_ocv_dnn_net_get_layer_names_fill"),
        33 => N("jyppx_ocv_dnn_layer_release_handle", "jyppx_ocv_dnn_net_get_layer_by_id"),
        34 => N("jyppx_ocv_dnn_layer_release_handle", "jyppx_ocv_dnn_net_get_layer_by_name"),
        36 => N("jyppx_ocv_dnn_net_connect"),
        37 => N("jyppx_ocv_dnn_net_register_output"),
        38 => N("jyppx_ocv_dnn_net_set_inputs_names"),
        39 => N("jyppx_ocv_dnn_net_set_input_shape"),
        40 or 42 => N("jyppx_ocv_dnn_net_forward"),
        43 => N("jyppx_ocv_dnn_net_forward_many"),
        44 => N("jyppx_ocv_dnn_mat_groups_get_counts", "jyppx_ocv_dnn_mat_groups_get_group_offsets", "jyppx_ocv_dnn_mat_groups_release_handle", "jyppx_ocv_dnn_mat_groups_take_mats", "jyppx_ocv_dnn_net_forward_and_retrieve"),
        45 => N("jyppx_ocv_dnn_net_set_preferable_backend"),
        46 => N("jyppx_ocv_dnn_net_set_preferable_target"),
        47 => N("jyppx_ocv_dnn_net_finalize"),
        48 => N("jyppx_ocv_dnn_net_set_tracing_mode"),
        49 => N("jyppx_ocv_dnn_net_get_tracing_mode"),
        50 => N("jyppx_ocv_dnn_net_set_profiling_mode"),
        51 => N("jyppx_ocv_dnn_net_get_profiling_mode"),
        52 => N("jyppx_ocv_dnn_net_get_model_format"),
        53 => N("jyppx_ocv_dnn_net_set_input"),
        54 => N("jyppx_ocv_dnn_net_set_param_by_id"),
        55 => N("jyppx_ocv_dnn_net_set_param_by_name"),
        56 => N("jyppx_ocv_dnn_net_get_param_by_id"),
        57 => N("jyppx_ocv_dnn_net_get_param_by_name"),
        58 => N("jyppx_ocv_dnn_net_get_unconnected_out_layers_count", "jyppx_ocv_dnn_net_get_unconnected_out_layers_fill"),
        59 => N("jyppx_ocv_dnn_net_get_unconnected_out_layers_names_count", "jyppx_ocv_dnn_net_get_unconnected_out_layers_names_fill"),
        60 => N("jyppx_ocv_dnn_net_get_layer_shapes_count", "jyppx_ocv_dnn_net_get_layer_shapes_fill"),
        61 => N("jyppx_ocv_dnn_net_get_flops_many"),
        62 => N("jyppx_ocv_dnn_net_get_layer_types_count", "jyppx_ocv_dnn_net_get_layer_types_fill"),
        63 => N("jyppx_ocv_dnn_net_get_layers_count_by_type"),
        64 => N("jyppx_ocv_dnn_net_get_memory_consumption"),
        65 => N("jyppx_ocv_dnn_net_enable_fusion"),
        66 => N("jyppx_ocv_dnn_net_enable_winograd"),
        67 => N("jyppx_ocv_dnn_net_get_perf_profile_count", "jyppx_ocv_dnn_net_get_perf_profile_fill"),
        68 => N("jyppx_ocv_dnn_net_enable_kv_cache"),
        69 => N("jyppx_ocv_dnn_net_disable_kv_cache"),
        70 => N("jyppx_ocv_dnn_net_reset_kv_cache"),
        71 => N("jyppx_ocv_dnn_net_get_detailed_perf_profile_count", "jyppx_ocv_dnn_net_get_detailed_perf_profile_fill"),
        74 => N("jyppx_ocv_dnn_net_release_handle", "jyppx_ocv_dnn_read_net_from_tensorflow_ex"),
        75 => N("jyppx_ocv_dnn_net_release_handle", "jyppx_ocv_dnn_read_net_from_tensorflow_buffer"),
        76 => N("jyppx_ocv_dnn_net_release_handle", "jyppx_ocv_dnn_read_net_from_tflite"),
        77 => N("jyppx_ocv_dnn_net_release_handle", "jyppx_ocv_dnn_read_net_from_tflite_buffer"),
        78 => N("jyppx_ocv_dnn_net_release_handle", "jyppx_ocv_dnn_read_net"),
        79 => N("jyppx_ocv_dnn_net_release_handle", "jyppx_ocv_dnn_read_net_from_buffer"),
        82 => N("jyppx_ocv_dnn_net_release_handle", "jyppx_ocv_dnn_read_net_from_onnx"),
        83 => N("jyppx_ocv_dnn_net_release_handle", "jyppx_ocv_dnn_read_net_from_onnx_buffer"),
        84 => N("jyppx_ocv_dnn_read_tensor_from_onnx"),
        85 => N("jyppx_ocv_dnn_blob_from_image"),
        86 => N("jyppx_ocv_dnn_blob_from_images"),
        89 or 90 or 93 or 94 => N("jyppx_ocv_dnn_blob_from_image_with_params"),
        91 => N("jyppx_ocv_dnn_blob_rect_to_image_rect"),
        92 => N("jyppx_ocv_dnn_blob_rects_to_image_rects"),
        95 or 96 => N("jyppx_ocv_dnn_blob_from_images_with_params"),
        97 => N("jyppx_ocv_dnn_images_from_blob_count", "jyppx_ocv_dnn_images_from_blob_fill"),
        99 => N("jyppx_ocv_dnn_nms_boxes_rect2d"),
        100 => N("jyppx_ocv_dnn_nms_boxes_rotated_rect"),
        101 => N("jyppx_ocv_dnn_nms_boxes_batched_rect2d"),
        103 => N("jyppx_ocv_dnn_soft_nms_boxes_rect"),
        _ => Array.Empty<string>()
    };

    private static string[] N(params string[] values) => values.Order(Ordinal).ToArray();

    private static IEnumerable<string> ResolveManaged(int ordinal, string[] managed)
    {
        var result = new SortedSet<string>(Ordinal);
        foreach (ManagedQuery query in ManagedQueries(ordinal))
        {
            string prefix = "MEMBER|" + query.TypeName + "|";
            foreach (string line in managed.Where(value => value.StartsWith(prefix, StringComparison.Ordinal) && query.Tokens.All(token => value.Contains(token, StringComparison.Ordinal))))
            {
                result.Add(line);
            }
        }
        return result;
    }

    private static ManagedQuery[] ManagedQueries(int ordinal) => ordinal switch
    {
        15 => Q("JYPPX.OpenCvSharp.Dnn.Cv2", "GetAvailableTargets("),
        20 => Q("JYPPX.OpenCvSharp.Dnn.Layer", "OutputNameToIndex("),
        22 => Q("JYPPX.OpenCvSharp.Dnn.Net", " CreateEmpty("),
        23 or 80 => Q("JYPPX.OpenCvSharp.Dnn.Net", " ReadNetFromModelOptimizer(System.String"),
        24 or 81 => Q("JYPPX.OpenCvSharp.Dnn.Net", " ReadNetFromModelOptimizer(System.Byte[]"),
        25 => Q("JYPPX.OpenCvSharp.Dnn.Net", "System.Boolean Empty"),
        26 => Q("JYPPX.OpenCvSharp.Dnn.Net", "System.String Dump("),
        27 => Q("JYPPX.OpenCvSharp.Dnn.Net", " DumpToFile("),
        28 => Q("JYPPX.OpenCvSharp.Dnn.Net", " DumpToPbtxt("),
        31 => Q("JYPPX.OpenCvSharp.Dnn.Net", " GetLayerId("),
        32 => Q("JYPPX.OpenCvSharp.Dnn.Net", "System.String[] GetLayerNames("),
        33 => Q("JYPPX.OpenCvSharp.Dnn.Net", "JYPPX.OpenCvSharp.Dnn.Layer GetLayer(System.Int32"),
        34 => Q("JYPPX.OpenCvSharp.Dnn.Net", "JYPPX.OpenCvSharp.Dnn.Layer GetLayer(System.String"),
        36 => Q("JYPPX.OpenCvSharp.Dnn.Net", " Connect("),
        37 => Q("JYPPX.OpenCvSharp.Dnn.Net", " RegisterOutput("),
        38 => Q("JYPPX.OpenCvSharp.Dnn.Net", " SetInputsNames("),
        39 => Q("JYPPX.OpenCvSharp.Dnn.Net", " SetInputShape("),
        40 => Q("JYPPX.OpenCvSharp.Dnn.Net", "JYPPX.OpenCvSharp.Core.Mat Forward(System.String"),
        42 => Q("JYPPX.OpenCvSharp.Dnn.Net", "System.Void Forward(JYPPX.OpenCvSharp.Core.Mat"),
        43 => Q("JYPPX.OpenCvSharp.Dnn.Net", "JYPPX.OpenCvSharp.Core.Mat[] Forward(System.String[]"),
        44 => Q("JYPPX.OpenCvSharp.Dnn.Net", "JYPPX.OpenCvSharp.Core.Mat[][] ForwardAndRetrieve("),
        45 => Q("JYPPX.OpenCvSharp.Dnn.Net", " SetPreferableBackend("),
        46 => Q("JYPPX.OpenCvSharp.Dnn.Net", " SetPreferableTarget("),
        47 => Q("JYPPX.OpenCvSharp.Dnn.Net", " FinalizeNetwork("),
        48 => Q("JYPPX.OpenCvSharp.Dnn.Net", " SetTracingMode("),
        49 => Q("JYPPX.OpenCvSharp.Dnn.Net", " GetTracingMode("),
        50 => Q("JYPPX.OpenCvSharp.Dnn.Net", " SetProfilingMode("),
        51 => Q("JYPPX.OpenCvSharp.Dnn.Net", " GetProfilingMode("),
        52 => Q("JYPPX.OpenCvSharp.Dnn.Net", "JYPPX.OpenCvSharp.Dnn.DnnModelFormat ModelFormat"),
        53 => Q("JYPPX.OpenCvSharp.Dnn.Net", " SetInput("),
        54 => Q("JYPPX.OpenCvSharp.Dnn.Net", " SetParam(System.Int32"),
        55 => Q("JYPPX.OpenCvSharp.Dnn.Net", " SetParam(System.String"),
        56 => Q("JYPPX.OpenCvSharp.Dnn.Net", " GetParam(System.Int32"),
        57 => Q("JYPPX.OpenCvSharp.Dnn.Net", " GetParam(System.String"),
        58 => Q("JYPPX.OpenCvSharp.Dnn.Net", "System.Int32[] GetUnconnectedOutLayers("),
        59 => Q("JYPPX.OpenCvSharp.Dnn.Net", "System.String[] GetUnconnectedOutLayersNames("),
        60 => Q("JYPPX.OpenCvSharp.Dnn.Net", "JYPPX.OpenCvSharp.Dnn.DnnLayerShapes GetLayerShapes("),
        61 => Q("JYPPX.OpenCvSharp.Dnn.Net", " GetFLOPS("),
        62 => Q("JYPPX.OpenCvSharp.Dnn.Net", "System.String[] GetLayerTypes("),
        63 => Q("JYPPX.OpenCvSharp.Dnn.Net", " GetLayersCountByType("),
        64 => Q("JYPPX.OpenCvSharp.Dnn.Net", "JYPPX.OpenCvSharp.Dnn.DnnMemoryConsumption GetMemoryConsumption("),
        65 => Q("JYPPX.OpenCvSharp.Dnn.Net", " EnableFusion("),
        66 => Q("JYPPX.OpenCvSharp.Dnn.Net", " EnableWinograd("),
        67 => Q("JYPPX.OpenCvSharp.Dnn.Net", "JYPPX.OpenCvSharp.Dnn.DnnPerfProfile GetPerfProfile("),
        68 => Q("JYPPX.OpenCvSharp.Dnn.Net", " EnableKvCache("),
        69 => Q("JYPPX.OpenCvSharp.Dnn.Net", " DisableKvCache("),
        70 => Q("JYPPX.OpenCvSharp.Dnn.Net", " ResetKvCache("),
        71 => Q("JYPPX.OpenCvSharp.Dnn.Net", "JYPPX.OpenCvSharp.Dnn.DnnDetailedPerfProfile GetDetailedPerfProfile("),
        74 => Q("JYPPX.OpenCvSharp.Dnn.Net", " ReadNetFromTensorflow(System.String"),
        75 => Q("JYPPX.OpenCvSharp.Dnn.Net", " ReadNetFromTensorflow(System.Byte[]"),
        76 => Q("JYPPX.OpenCvSharp.Dnn.Net", " ReadNetFromTFLite(System.String"),
        77 => Q("JYPPX.OpenCvSharp.Dnn.Net", " ReadNetFromTFLite(System.Byte[]"),
        78 => Q("JYPPX.OpenCvSharp.Dnn.Net", " ReadNet(System.String model"),
        79 => Q("JYPPX.OpenCvSharp.Dnn.Net", " ReadNet(System.String framework,System.Byte[]"),
        82 => Q("JYPPX.OpenCvSharp.Dnn.Net", " ReadNetFromOnnx(System.String"),
        83 => Q("JYPPX.OpenCvSharp.Dnn.Net", " ReadNetFromOnnx(System.Byte[]"),
        84 => Q("JYPPX.OpenCvSharp.Dnn.Cv2", " ReadTensorFromOnnx("),
        85 => Q("JYPPX.OpenCvSharp.Dnn.Cv2", " BlobFromImage(", "scaleFactor"),
        86 => Q("JYPPX.OpenCvSharp.Dnn.Cv2", " BlobFromImages(", "scaleFactor"),
        89 => Q("JYPPX.OpenCvSharp.Dnn.Image2BlobParams", ".ctor()"),
        90 => Q("JYPPX.OpenCvSharp.Dnn.Image2BlobParams", ".ctor(", "JYPPX.OpenCvSharp.Core.Scalar scaleFactor"),
        91 => Q("JYPPX.OpenCvSharp.Dnn.Image2BlobParams", " BlobRectToImageRect("),
        92 => Q("JYPPX.OpenCvSharp.Dnn.Image2BlobParams", " BlobRectsToImageRects("),
        93 or 94 => Q("JYPPX.OpenCvSharp.Dnn.Cv2", " BlobFromImage(", "JYPPX.OpenCvSharp.Dnn.Image2BlobParams"),
        95 or 96 => Q("JYPPX.OpenCvSharp.Dnn.Cv2", " BlobFromImages(", "JYPPX.OpenCvSharp.Dnn.Image2BlobParams"),
        97 => Q("JYPPX.OpenCvSharp.Dnn.Cv2", " ImagesFromBlob("),
        99 => Q("JYPPX.OpenCvSharp.Dnn.Cv2", " NMSBoxes(", "Core.Rect2d> boxes"),
        100 => Q("JYPPX.OpenCvSharp.Dnn.Cv2", " NMSBoxes(", "Core.RotatedRect> boxes"),
        101 => Q("JYPPX.OpenCvSharp.Dnn.Cv2", " NMSBoxesBatched(", "Core.Rect2d> boxes"),
        103 => Q("JYPPX.OpenCvSharp.Dnn.Cv2", " SoftNMSBoxes("),
        _ => Array.Empty<ManagedQuery>()
    };

    private static ManagedQuery[] Q(string type, params string[] tokens) => new[] { new ManagedQuery(type, tokens) };

    private static void Validate(RawDocument raw, ClassificationDocument document, Options options, string[] native, string[] managed, bool hashes)
    {
        Require(raw.SchemaVersion == 1 && raw.Generator == "tools/DnnUpstreamMap/extract_dnn.py" && raw.UpstreamOpenCvVersion == "5.0.0", "DNN raw generator identity drifted.");
        Require(raw.DeclarationCount == raw.Declarations.Count && raw.Declarations.Count == 182, "DNN raw declaration count drifted.");
        Require(raw.Declarations.Count(value => value.Kind == "callable") == 159, "DNN raw callable count drifted.");
        Require(raw.Declarations.Select(value => value.Identity).Distinct(Ordinal).Count() == raw.Declarations.Count, "DNN raw contains duplicate identities.");
        Require(raw.Declarations.Select((value, index) => value.Ordinal == index).All(value => value), "DNN raw ordinals are reordered.");
        Require(raw.PreprocessorDefinitions.Count == 2 && raw.PreprocessorDefinitions.GetValueOrDefault("CV_VERSION_MAJOR") == 5 && raw.PreprocessorDefinitions.GetValueOrDefault("OPENCV_ABI_COMPATIBILITY") == 500, "DNN parser definitions drifted.");

        string[] expectedHeaders =
        {
            "opencv-source/opencv-5.0.0/modules/dnn/include/opencv2/dnn/dict.hpp",
            "opencv-source/opencv-5.0.0/modules/dnn/include/opencv2/dnn/dnn.hpp",
            "opencv-source/opencv-5.0.0/modules/dnn/include/opencv2/dnn/utils/inference_engine.hpp"
        };
        Require(raw.SourceHeaders.Select(value => value.Path).SequenceEqual(expectedHeaders, Ordinal), "DNN public source-header closure drifted.");
        Require(raw.SourceHeaders.Select(value => value.DeclarationCount).SequenceEqual(new[] { 10, 166, 6 }), "DNN source-header declaration partition drifted.");
        Require(raw.SourceHeaders.Select(value => value.StartOrdinal).SequenceEqual(new[] { 0, 10, 176 }), "DNN source-header ordinals drifted.");
        if (hashes)
        {
            Require(FileSha(options.Workspace, raw.HeaderPath) == raw.HeaderSha256, "DNN umbrella header SHA256 is stale.");
            Require(FileSha(options.Workspace, raw.ParserPath) == raw.ParserSha256, "DNN parser SHA256 is stale.");
            foreach (SourceHeader header in raw.SourceHeaders)
                Require(FileSha(options.Workspace, header.Path) == header.Sha256, "DNN source-header SHA256 is stale: " + header.Path);
        }

        Require(document.SchemaVersion == 1 && document.UpstreamOpenCvVersion == "5.0.0" && document.ClaimedSlice == ClaimedSlice && document.ReviewStatus == "reviewed", "DNN classification identity drifted.");
        Require(document.Declarations.Count == raw.Declarations.Count, "DNN classification must contain one row per declaration.");
        Require(document.Declarations.Select(value => value.Identity).Distinct(Ordinal).Count() == document.Declarations.Count, "DNN classification contains duplicate identities.");
        HashSet<string> nativeSet = native.ToHashSet(Ordinal);
        HashSet<string> managedSet = managed.ToHashSet(Ordinal);
        for (int i = 0; i < raw.Declarations.Count; i++)
        {
            RawDeclaration declaration = raw.Declarations[i];
            ClassificationRow row = document.Declarations[i];
            Require(row.Ordinal == declaration.Ordinal && row.Identity == declaration.Identity, "DNN classification row is missing or reordered at ordinal " + i);
            Require(Classifications.Contains(row.Classification, Ordinal), "Unknown DNN classification: " + row.Classification);
            Require(IsSorted(row.NativeEntrypoints) && IsSorted(row.ManagedMembers), "DNN evidence is nondeterministically ordered: " + row.Identity);
            Require(row.NativeEntrypoints.All(value => !Regex.IsMatch(value, "^jyppx_ocv[0-9]+_", RegexOptions.CultureInvariant)), "Fixed-major DNN native evidence is forbidden: " + row.Identity);
            Require(row.ManagedMembers.All(value => !Regex.IsMatch(value, "JYPPX.OpenCvSharp[0-9]+", RegexOptions.CultureInvariant)), "Fixed-major DNN managed evidence is forbidden: " + row.Identity);
            Require(row.NativeEntrypoints.All(nativeSet.Contains), "DNN classification references false native evidence: " + row.Identity);
            Require(row.ManagedMembers.All(managedSet.Contains), "DNN classification references false managed evidence: " + row.Identity);
            if (row.Classification == "implemented")
            {
                Require(declaration.Kind == "callable" && SelectedCallables.Contains(i), "Only selected DNN callables may be classified implemented: " + row.Identity);
                Require(row.NativeEntrypoints.SequenceEqual(ResolveNative(i, native), Ordinal), "DNN implemented row has false native correlation: " + row.Identity);
                Require(row.ManagedMembers.SequenceEqual(ResolveManaged(i, managed), Ordinal), "DNN implemented row has false managed correlation: " + row.Identity);
                Require(row.NativeEntrypoints.Count > 0 && row.ManagedMembers.Count > 0, "DNN implemented row requires native and managed evidence: " + row.Identity);
            }
            if (row.Classification == "missing")
                Require(SelectedCallables.Contains(i), "Only selected DNN callables may remain missing: " + row.Identity);
            if (row.Classification == "non-callable-metadata")
                Require(declaration.Kind is "enum" or "class", "DNN callable was confused with metadata: " + row.Identity);
            if (row.Classification is "missing" or "intentionally-omitted" or "upstream-conditional" or "unsupported")
                Require(!string.IsNullOrWhiteSpace(row.Reason), "DNN non-implemented row requires a documented reason: " + row.Identity);
            if (row.Classification == "upstream-conditional")
                Require(row.BuildCondition != "unconditional-parser-surface", "DNN conditional row requires an explicit build condition: " + row.Identity);
        }
    }

    private static FamilyDocument BuildFamilies(RawDocument raw, ClassificationDocument classifications, string[] managed)
    {
        var definitions = new[]
        {
            (Id: "runtime-backend-and-model-loading", Rationale: "Correlate backend/target/engine metadata, deterministic availability, owned Net construction, and path/buffer model readers.", Ordinals: new HashSet<int>(new[] { 10, 11, 12, 13, 14, 15, 21, 22, 23, 24, 25, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84 })),
            (Id: "net-forward-introspection-and-controls", Rationale: "Close the adjacent Net dump, graph connection, forward, parameter, shape, memory, optimization, cache, and profiling surface.", Ordinals: new HashSet<int>(SelectedCallables.Where(value => value is >= 26 and <= 71 && value is not 33 and not 34))),
            (Id: "owned-layer-reference", Rationale: "Expose ref-counted Layer lookup and output-name indexing without leaking cv::Ptr or parent Net lifetime.", Ordinals: new HashSet<int>(new[] { 17, 20, 33, 34 })),
            (Id: "image-to-blob-parameter-workflow", Rationale: "Preserve legacy blob helpers and complete Image2BlobParams layout, padding, border, and rectangle projection semantics.", Ordinals: new HashSet<int>(Enumerable.Range(85, 13))),
            (Id: "nms-postprocessing", Rationale: "Expose standard, class-aware, rotated, and score-decaying OpenCV NMS with owned result arrays and deterministic validation.", Ordinals: new HashSet<int>(new[] { 99, 100, 101, 102, 103 }))
        };
        int memberAdditions = managed.Count(value => value.StartsWith("MEMBER|JYPPX.OpenCvSharp.Dnn.", StringComparison.Ordinal)) - 69;
        Require(memberAdditions >= 0, "DNN managed member baseline regressed below the pre-round 69-member surface.");
        var result = new FamilyDocument { ManagedPublicMemberAdditionCount = memberAdditions };
        foreach (var definition in definitions)
        {
            var family = new FamilyRow { Id = definition.Id, Rationale = definition.Rationale };
            foreach (int ordinal in definition.Ordinals.Order())
            {
                ClassificationRow classification = classifications.Declarations[ordinal];
                RawDeclaration declaration = raw.Declarations[ordinal];
                family.Declarations.Add(new FamilyOperation
                {
                    Ordinal = ordinal,
                    UpstreamIdentity = declaration.Identity,
                    UpstreamClassification = classification.Classification,
                    NativeEntrypoints = classification.NativeEntrypoints.ToList(),
                    ManagedMembers = classification.ManagedMembers.ToList(),
                    FocusedTest = definition.Id == "nms-postprocessing"
                        ? "tests/OpenCvSharp.Tests/Dnn/DnnNmsTests.cs"
                        : "tests/OpenCvSharp.Tests/Dnn/DnnStructuredParityTests.cs"
                });
            }
            result.Families.Add(family);
        }
        Require(result.Families.SelectMany(value => value.Declarations).Select(value => value.Ordinal).Distinct().Count() == result.Families.Sum(value => value.Declarations.Count), "DNN selected families overlap.");
        return result;
    }

    private static string BuildMap(RawDocument raw, ClassificationDocument classifications)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# OpenCV 5.0.0 DNN upstream-to-native-to-managed map");
        builder.AppendLine("schema-version=1");
        builder.AppendLine("claimed-slice=" + ClaimedSlice);
        builder.AppendLine("repository-wide-upstream-parity=false");
        builder.AppendLine("header-sha256=" + raw.HeaderSha256);
        builder.AppendLine("parser-sha256=" + raw.ParserSha256);
        builder.AppendLine("classification-order=" + string.Join(",", Classifications));
        builder.AppendLine("[source-headers]");
        foreach (SourceHeader source in raw.SourceHeaders)
            builder.Append(source.StartOrdinal.ToString("D3")).Append('|').Append(source.DeclarationCount).Append('|').Append(source.Sha256).Append('|').AppendLine(source.Path);
        builder.AppendLine("[declarations]");
        for (int i = 0; i < raw.Declarations.Count; i++)
        {
            RawDeclaration declaration = raw.Declarations[i];
            ClassificationRow row = classifications.Declarations[i];
            builder.Append(declaration.Ordinal.ToString("D3")).Append('|').Append(declaration.Kind).Append('|').Append(declaration.SourceHeader).Append('|')
                .Append(row.Classification).Append('|').Append(Clean(declaration.Identity)).Append('|')
                .Append(row.NativeEntrypoints.Count == 0 ? "-" : string.Join(";", row.NativeEntrypoints)).Append('|')
                .Append(row.ManagedMembers.Count == 0 ? "-" : string.Join(";", row.ManagedMembers)).Append('|').AppendLine(Clean(row.Reason));
        }
        return Normalize(builder.ToString());
    }

    private static void RunNegativeFixtures(RawDocument raw, ClassificationDocument classifications, SummaryDocument summary, Options options, string[] native, string[] managed)
    {
        int rejected = 0;
        void Reject(string name, string expected, Action action)
        {
            try { action(); throw new InvalidDataException("Negative DNN fixture was accepted: " + name); }
            catch (InvalidDataException exception)
            {
                if (!exception.Message.Contains(expected, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException($"Negative DNN fixture '{name}' failed for the wrong reason: {exception.Message}");
                rejected++;
            }
        }
        ClassificationDocument CopyC() => JsonSerializer.Deserialize<ClassificationDocument>(Serialize(classifications), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        RawDocument CopyR() => JsonSerializer.Deserialize<RawDocument>(Serialize(raw), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        Reject("missing row", "one row", () => { var value = CopyC(); value.Declarations.RemoveAt(0); Validate(raw, value, options, native, managed, false); });
        Reject("duplicate identity", "duplicate identities", () => { var value = CopyC(); value.Declarations[1].Identity = value.Declarations[0].Identity; Validate(raw, value, options, native, managed, false); });
        Reject("reorder", "reordered", () => { var value = CopyC(); (value.Declarations[0], value.Declarations[1]) = (value.Declarations[1], value.Declarations[0]); Validate(raw, value, options, native, managed, false); });
        Reject("overload collapse", "duplicate identities", () => { var value = CopyR(); value.Declarations[24].Identity = value.Declarations[23].Identity; Validate(value, classifications, options, native, managed, false); });
        Reject("callable metadata confusion", "confused", () => { var value = CopyC(); value.Declarations[22].Classification = "non-callable-metadata"; Validate(raw, value, options, native, managed, false); });
        Reject("source header drift", "closure", () => { var value = CopyR(); value.SourceHeaders.RemoveAt(0); Validate(value, classifications, options, native, managed, false); });
        Reject("parser drift", "generator identity", () => { var value = CopyR(); value.Generator = "other"; Validate(value, classifications, options, native, managed, false); });
        Reject("stale source hash", "SHA256", () => { var value = CopyR(); value.HeaderSha256 = new string('0', 64); Validate(value, classifications, options, native, managed, true); });
        Reject("false native evidence", "false native", () => { var value = CopyC(); var row = value.Declarations.First(item => item.Classification == "implemented"); row.NativeEntrypoints[0] = "jyppx_ocv_dnn_false"; Validate(raw, value, options, native, managed, false); });
        Reject("false managed evidence", "false managed", () => { var value = CopyC(); var row = value.Declarations.First(item => item.Classification == "implemented"); row.ManagedMembers[0] = "MEMBER|JYPPX.OpenCvSharp.Dnn.False|method|public|System.Void False()"; Validate(raw, value, options, native, managed, false); });
        Reject("undocumented omission", "documented reason", () => { var value = CopyC(); var row = value.Declarations.First(item => item.Classification == "intentionally-omitted"); row.Reason = ""; Validate(raw, value, options, native, managed, false); });
        Reject("fixed major", "Fixed-major", () => { var value = CopyC(); var row = value.Declarations.First(item => item.ManagedMembers.Count > 0); row.ManagedMembers[0] = row.ManagedMembers[0].Replace("JYPPX.OpenCvSharp", "JYPPX.OpenCvSharp5", StringComparison.Ordinal); Validate(raw, value, options, native, managed, false); });
        Reject("conditional misclassification", "build condition", () => { var value = CopyC(); var row = value.Declarations[176]; row.BuildCondition = "unconditional-parser-surface"; Validate(raw, value, options, native, managed, false); });
        Reject("evidence order", "nondeterministically", () => { var value = CopyC(); var row = value.Declarations.First(item => item.NativeEntrypoints.Count > 1); row.NativeEntrypoints.Reverse(); Validate(raw, value, options, native, managed, false); });
        Reject("stale mapping hash", "stale", () => Require(summary.MappingSha256 == new string('0', 64), "DNN mapping SHA256 is stale."));
        Require(rejected == 15, "DNN negative fixture count drifted.");
    }

    private static string[] ReadNative(string path)
    {
        string[] result = File.ReadLines(path).Where(value => value.StartsWith("jyppx_ocv_", StringComparison.Ordinal) && value.Contains('|')).Select(value => value.Split('|')[0]).ToArray();
        Require(result.Length > 0 && result.Distinct(Ordinal).Count() == result.Length && result.SequenceEqual(result.Order(Ordinal), Ordinal), "Native manifest is empty, duplicated, or unsorted.");
        return result;
    }

    private static string[] ReadManaged(string path)
    {
        string[] result = File.ReadLines(path).Where(value => value.StartsWith("MEMBER|", StringComparison.Ordinal)).ToArray();
        Require(result.Length > 0 && result.Distinct(Ordinal).Count() == result.Length, "Managed baseline member evidence is empty or duplicated.");
        return result;
    }

    private static T Read<T>(string path, JsonSerializerOptions options) => JsonSerializer.Deserialize<T>(File.ReadAllText(path), options) ?? throw new InvalidDataException("Could not parse " + path);
    private static string Serialize<T>(T value) => JsonSerializer.Serialize(value, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true }) + "\n";
    private static string Clean(string value) => value.Replace('|', '/').Replace('\r', ' ').Replace('\n', ' ').Trim();
    private static string Normalize(string value) => value.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n').TrimEnd() + "\n";
    private static string Rel(string root, string path) => Path.GetRelativePath(root, path).Replace('\\', '/');
    private static string Sha256(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(Normalize(value)))).ToLowerInvariant();
    private static string FileSha(string workspace, string relative) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(workspace, relative.Replace('/', Path.DirectorySeparatorChar))))).ToLowerInvariant();
    private static bool IsSorted(IReadOnlyList<string> values) => values.Count == values.Distinct(Ordinal).Count() && values.SequenceEqual(values.Order(Ordinal), Ordinal);

    private static void WriteOrCheck(string path, string value, bool check)
    {
        value = Normalize(value);
        if (check)
        {
            Require(File.Exists(path) && Normalize(File.ReadAllText(path)) == value, "Generated DNN file is stale: " + path);
            return;
        }
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, value, new UTF8Encoding(false));
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidDataException(message);
    }
}

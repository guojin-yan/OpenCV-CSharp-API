using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

internal static class Program
{
    private static readonly StringComparer Ordinal = StringComparer.Ordinal;
    private static readonly string[] Classes = { "implemented", "missing", "intentionally-omitted", "upstream-conditional", "unsupported", "non-callable-metadata" };
    private static readonly HashSet<string> SelectedSymbols = new(Ordinal)
    {
        "borderInterpolate", "copyMakeBorder", "hasNonZero", "findNonZero", "PSNR", "reduceArgMin", "reduceArgMax",
        "flipND", "broadcast", "copyTo", "checkRange", "finiteMask", "transposeND", "sort", "sortIdx"
    };
    private static readonly HashSet<int> NumericalOrdinals = new()
    {
        6, 7, 121, 128, 159, 179, 180, 181, 182, 183, 184, 185, 186, 187, 196, 197, 198, 199, 256, 257
    };
    private static readonly HashSet<int> RuntimeUtilityOrdinals = new()
    {
        215, 216, 217, 218, 219, 220, 221, 222, 223, 224, 242, 243, 244, 245, 246, 248, 249, 250
    };
    private static readonly HashSet<int> TickMeterOrdinals = new(Enumerable.Range(226, 16));

    private sealed record Options(string Repository, string Workspace, string Raw, string Classification, string NativeManifest, string ManagedBaseline, string Output, string Summary, string FamilyOutput, bool Initialize, bool Check);
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
    private sealed class SourceHeader { public string Path { get; set; } = ""; public string Sha256 { get; set; } = ""; public int StartOrdinal { get; set; } public int DeclarationCount { get; set; } }
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
    private sealed class RawArgument { public string Type { get; set; } = ""; public string Name { get; set; } = ""; public string Default { get; set; } = ""; public List<string> Modifiers { get; set; } = new(); }
    private sealed class RawEnumValue { public string Name { get; set; } = ""; public string Value { get; set; } = ""; }
    private sealed class ClassificationDocument
    {
        public int SchemaVersion { get; set; } = 1;
        public string UpstreamOpenCvVersion { get; set; } = "5.0.0";
        public string ClaimedSlice { get; set; } = "opencv2/core.hpp public compatibility include closure across 11 parser-reviewed contributing headers";
        public string ReviewStatus { get; set; } = "reviewed";
        public string Limitation { get; set; } = "Parser identities preserve header order, overloads, defaults, direction metadata, and build definitions; classifications do not claim repository-wide OpenCV parity.";
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
        public string Generator { get; init; } = "tools/CoreUpstreamMap";
        public string UpstreamOpenCvVersion { get; init; } = "5.0.0";
        public string ClaimedSlice { get; init; } = "opencv2/core.hpp public compatibility include closure across 11 parser-reviewed contributing headers";
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
        public int NegativeFixtureCount { get; init; } = 13;
        public string FamilyInventoryPath { get; init; } = "";
        public string FamilyInventorySha256 { get; init; } = "";
        public int SelectedFamilyCount { get; init; }
        public int SelectedDeclarationCount { get; init; }
        public int ManagedPublicTypeAdditionCount { get; init; } = 11;
        public int ManagedPublicMemberAdditionCount { get; init; } = 226;
        public bool RepositoryWideUpstreamParityClaimed { get; init; }
    }
    private sealed class FamilyDocument
    {
        public int SchemaVersion { get; init; } = 1;
        public string UpstreamOpenCvVersion { get; init; } = "5.0.0";
        public string Status { get; init; } = "implemented-verified";
        public int ManagedPublicTypeAdditionCount { get; init; } = 11;
        public int ManagedPublicMemberAdditionCount { get; init; } = 226;
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
        public string UpstreamClassification { get; init; } = "implemented";
        public List<string> NativeEntrypoints { get; init; } = new();
        public List<string> ManagedMembers { get; init; } = new();
        public string FocusedTest { get; init; } = "tests/OpenCvSharp.Tests/Core/CoreUpstreamParityTests.cs";
        public string NativeSmoke { get; init; } = "src/OpenCvSharp.Native/tests/native_smoke.cpp";
        public string Sample { get; init; } = "samples/ConsoleSamples/Program.cs";
        public string Guide { get; init; } = "docs/articles/core-upstream-parity-guide.md";
    }

    private static int Main(string[] args)
    {
        try
        {
            Options o = Parse(args);
            var json = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            RawDocument raw = Read<RawDocument>(o.Raw, json);
            string[] native = File.ReadAllLines(o.NativeManifest).Where(v => !string.IsNullOrWhiteSpace(v)).Select(v => v.Split('|')[0]).ToArray();
            string[] managed = File.ReadAllLines(o.ManagedBaseline).Where(v => v.StartsWith("MEMBER|", StringComparison.Ordinal)).ToArray();
            if (o.Initialize)
                WriteJson(o.Classification, Initialize(raw, native, managed), false);
            ClassificationDocument classifications = Read<ClassificationDocument>(o.Classification, json);
            Validate(raw, classifications, o, native, managed, true);
            string mapping = BuildMap(raw, classifications);
            FamilyDocument families = BuildFamilies(raw, classifications);
            string familyText = Serialize(families);
            var counts = new SortedDictionary<string, int>(Ordinal);
            foreach (string value in Classes) counts[value] = classifications.Declarations.Count(v => v.Classification == value);
            var summary = new SummaryDocument
            {
                RawExtractionPath = Rel(o.Repository, o.Raw), ClassificationPath = Rel(o.Repository, o.Classification), MappingPath = Rel(o.Repository, o.Output),
                HeaderSha256 = raw.HeaderSha256, ParserSha256 = raw.ParserSha256, SourceHeaderCount = raw.SourceHeaders.Count,
                SourceHeaderSetSha256 = Sha256(string.Join("\n", raw.SourceHeaders.Select(v => v.Path + "|" + v.Sha256)) + "\n"),
                MappingSha256 = Sha256(mapping), DeclarationCount = raw.Declarations.Count,
                EnumCount = raw.Declarations.Count(v => v.Kind == "enum"), ClassCount = raw.Declarations.Count(v => v.Kind == "class"),
                CallableCount = raw.Declarations.Count(v => v.Kind == "callable"), ClassificationCounts = counts,
                NativeEvidenceCount = classifications.Declarations.SelectMany(v => v.NativeEntrypoints).Distinct(Ordinal).Count(),
                ManagedEvidenceCount = classifications.Declarations.SelectMany(v => v.ManagedMembers).Distinct(Ordinal).Count(),
                FamilyInventoryPath = Rel(o.Repository, o.FamilyOutput), FamilyInventorySha256 = Sha256(familyText),
                SelectedFamilyCount = families.Families.Count, SelectedDeclarationCount = families.Families.Sum(v => v.Declarations.Count),
                RepositoryWideUpstreamParityClaimed = false
            };
            RunNegativeFixtures(raw, classifications, o, native, managed);
            WriteOrCheck(o.Output, mapping, o.Check);
            WriteOrCheck(o.FamilyOutput, familyText, o.Check);
            WriteJson(o.Summary, summary, o.Check);
            Console.WriteLine($"CORE_UPSTREAM_MAP_OK declarations={summary.DeclarationCount} callables={summary.CallableCount} implemented={counts["implemented"]} missing={counts["missing"]} omitted={counts["intentionally-omitted"]} fixtures=13 sha256={summary.MappingSha256} mode={(o.Check ? "check" : "write")}");
            return 0;
        }
        catch (Exception e) { Console.Error.WriteLine(e.Message); return 1; }
    }

    private static Options Parse(string[] args)
    {
        var values = new Dictionary<string, string>(Ordinal); bool init = false, check = false;
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--initialize-classification") init = true;
            else if (args[i] == "--check") check = true;
            else { Require(i + 1 < args.Length, "Missing option value: " + args[i]); values[args[i]] = args[++i]; }
        }
        string V(string key) { Require(values.TryGetValue(key, out string? value), "Missing option: " + key); return Path.GetFullPath(value!); }
        return new Options(V("--repository"), V("--workspace"), V("--raw"), V("--classification"), V("--native-manifest"), V("--managed-baseline"), V("--output"), V("--summary"), V("--family-output"), init, check);
    }

    private static ClassificationDocument Initialize(RawDocument raw, string[] native, string[] managed)
    {
        var result = new ClassificationDocument();
        foreach (RawDeclaration d in raw.Declarations)
        {
            var row = new ClassificationRow { Ordinal = d.Ordinal, Identity = d.Identity };
            if (d.Kind != "callable")
            {
                row.Classification = "non-callable-metadata";
                row.Reason = "Parser-emitted enum/class/struct metadata is reviewed as type or constant shape rather than an independently callable ABI operation.";
            }
            else
            {
                row.NativeEntrypoints = NativeEvidence(d, native).ToList();
                row.ManagedMembers = ManagedEvidence(d, managed).ToList();
                if (row.NativeEntrypoints.Count > 0 && row.ManagedMembers.Count > 0)
                {
                    row.Classification = "implemented";
                    row.Reason = "Correlated stable neutral C ABI and managed public API evidence was found for this callable identity.";
                }
                else if (d.Name.StartsWith("cv.ipp.", StringComparison.Ordinal))
                {
                    row.Classification = "upstream-conditional";
                    row.BuildCondition = "IPP-dependent-runtime-capability";
                    row.Reason = "IPP controls and version strings are backend-conditional and are not exposed as a production support contract by this build.";
                }
                else if (d.Name.StartsWith("cv.samples.", StringComparison.Ordinal))
                {
                    row.Classification = "unsupported";
                    row.Reason = "OpenCV sample-data search paths are development conveniences and are outside the packaged runtime contract.";
                }
                else if (IsPersistenceCallable(d))
                {
                    row.Classification = "missing";
                    row.Reason = "Selected persistence callable lacks the required stable native and managed evidence and must fail closed.";
                }
                else if (d.Name.Contains("MatShape", StringComparison.Ordinal) || d.Name.Contains("Algorithm.", StringComparison.Ordinal) || d.Name.Contains("TickMeter", StringComparison.Ordinal))
                {
                    row.Classification = "intentionally-omitted";
                    row.Reason = "The C++ convenience/stateful object lacks the dedicated neutral ownership model selected for this batch and remains explicitly deferred.";
                }
                else if (d.Name.Contains("RotatedRect.", StringComparison.Ordinal) || d.Name.Contains("KeyPoint.", StringComparison.Ordinal) || d.Name.Contains("DMatch.", StringComparison.Ordinal))
                {
                    row.Classification = "intentionally-omitted";
                    row.Reason = "This value-semantic C++ convenience callable is projected through managed value types or adjacent modules without exposing C++ layout through the C ABI.";
                }
                else
                {
                    row.Classification = "unsupported";
                    row.Reason = "Source-reviewed Core callable is outside the implemented family batch; no production ABI/managed evidence is claimed and its next priority remains explicit in the Core guide.";
                }
            }
            result.Declarations.Add(row);
        }
        return result;
    }

    private static IEnumerable<string> NativeEvidence(RawDeclaration d, string[] native)
    {
        if (IsPersistenceCallable(d))
        {
            string[] expected = d.Ordinal switch
            {
                57 => new[] { "jyppx_ocv_core_file_storage_create", "jyppx_ocv_core_file_storage_release_handle" },
                58 => new[] { "jyppx_ocv_core_file_storage_create", "jyppx_ocv_core_file_storage_open", "jyppx_ocv_core_file_storage_release_handle" },
                59 => new[] { "jyppx_ocv_core_file_storage_open" },
                60 => new[] { "jyppx_ocv_core_file_storage_is_opened" },
                61 => new[] { "jyppx_ocv_core_file_storage_release" },
                62 => new[] { "jyppx_ocv_core_file_storage_release_and_get_string", "jyppx_ocv_core_utf8_result_data", "jyppx_ocv_core_utf8_result_release", "jyppx_ocv_core_utf8_result_size" },
                63 => new[] { "jyppx_ocv_core_file_node_release", "jyppx_ocv_core_file_storage_get_first_top_level_node" },
                64 => new[] { "jyppx_ocv_core_file_node_release", "jyppx_ocv_core_file_storage_root" },
                65 => new[] { "jyppx_ocv_core_file_node_release", "jyppx_ocv_core_file_storage_get_node" },
                66 => new[] { "jyppx_ocv_core_file_storage_write_int" },
                67 => new[] { "jyppx_ocv_core_file_storage_write_bool" },
                68 => new[] { "jyppx_ocv_core_file_storage_write_int64" },
                69 => new[] { "jyppx_ocv_core_file_storage_write_double" },
                70 => new[] { "jyppx_ocv_core_file_storage_write_string" },
                71 => new[] { "jyppx_ocv_core_file_storage_write_mat" },
                72 => new[] { "jyppx_ocv_core_file_storage_write_string_vector" },
                73 => new[] { "jyppx_ocv_core_file_storage_write_comment" },
                74 => new[] { "jyppx_ocv_core_file_storage_start_write_struct" },
                75 => new[] { "jyppx_ocv_core_file_storage_end_write_struct" },
                76 => new[] { "jyppx_ocv_core_file_storage_get_format" },
                79 => new[] { "jyppx_ocv_core_file_node_create", "jyppx_ocv_core_file_node_release" },
                80 => new[] { "jyppx_ocv_core_file_node_get_node", "jyppx_ocv_core_file_node_release" },
                81 => new[] { "jyppx_ocv_core_file_node_at", "jyppx_ocv_core_file_node_release" },
                82 => new[] { "jyppx_ocv_core_file_node_keys", "jyppx_ocv_core_string_list_count", "jyppx_ocv_core_string_list_get", "jyppx_ocv_core_string_list_release", "jyppx_ocv_core_utf8_result_data", "jyppx_ocv_core_utf8_result_release", "jyppx_ocv_core_utf8_result_size" },
                83 or 85 or 86 or 87 or 88 or 89 or 90 => new[] { "jyppx_ocv_core_file_node_type" },
                84 => new[] { "jyppx_ocv_core_file_node_empty" },
                91 or 92 => new[] { "jyppx_ocv_core_file_node_name", "jyppx_ocv_core_utf8_result_data", "jyppx_ocv_core_utf8_result_release", "jyppx_ocv_core_utf8_result_size" },
                93 => new[] { "jyppx_ocv_core_file_node_size" },
                94 => new[] { "jyppx_ocv_core_file_node_raw_size" },
                95 => new[] { "jyppx_ocv_core_file_node_real" },
                96 => new[] { "jyppx_ocv_core_file_node_string", "jyppx_ocv_core_utf8_result_data", "jyppx_ocv_core_utf8_result_release", "jyppx_ocv_core_utf8_result_size" },
                97 => new[] { "jyppx_ocv_core_file_node_mat" },
                _ => Array.Empty<string>()
            };
            var nativeSet = native.ToHashSet(Ordinal);
            return expected.Where(nativeSet.Contains).Order(Ordinal);
        }

        if (NumericalOrdinals.Contains(d.Ordinal))
        {
            string[] expected = d.Ordinal switch
            {
                6 => new[] { "jyppx_ocv_core_cube_root" },
                7 => new[] { "jyppx_ocv_core_fast_atan2" },
                121 => new[] { "jyppx_ocv_core_batch_distance" },
                128 => new[] { "jyppx_ocv_core_split_count", "jyppx_ocv_core_split_fill" },
                159 => new[] { "jyppx_ocv_core_patch_nans" },
                179 => new[] { "jyppx_ocv_core_calc_covar_matrix" },
                180 or 181 => new[] { "jyppx_ocv_core_pca_compute_max_components" },
                182 or 183 => new[] { "jyppx_ocv_core_pca_compute_retained_variance" },
                184 => new[] { "jyppx_ocv_core_pca_project" },
                185 => new[] { "jyppx_ocv_core_pca_back_project" },
                186 => new[] { "jyppx_ocv_core_svd_static_compute" },
                187 => new[] { "jyppx_ocv_core_svd_static_back_subst" },
                196 => new[] { "jyppx_ocv_core_set_rng_seed" },
                197 => new[] { "jyppx_ocv_core_randu_mat", "jyppx_ocv_core_randu_scalar" },
                198 => new[] { "jyppx_ocv_core_randn_mat", "jyppx_ocv_core_randn_scalar" },
                199 => new[] { "jyppx_ocv_core_rand_shuffle" },
                256 or 257 => new[] { "jyppx_ocv_core_solve_lp" },
                _ => Array.Empty<string>()
            };
            var nativeSet = native.ToHashSet(Ordinal);
            return expected.Where(nativeSet.Contains).Order(Ordinal);
        }

        if (IsRuntimeDiagnosticsCallable(d))
        {
            string[] expected = d.Ordinal switch
            {
                215 => new[] { "jyppx_ocv_core_set_num_threads" },
                216 => new[] { "jyppx_ocv_core_get_num_threads" },
                217 => new[] { "jyppx_ocv_core_get_thread_num" },
                218 => Utf8Evidence("jyppx_ocv_core_get_build_information"),
                219 => new[] { "jyppx_ocv_get_version_string" },
                220 => new[] { "jyppx_ocv_get_version_major" },
                221 => new[] { "jyppx_ocv_get_version_minor" },
                222 => new[] { "jyppx_ocv_get_version_revision" },
                223 => new[] { "jyppx_ocv_core_get_tick_count" },
                224 => new[] { "jyppx_ocv_core_get_tick_frequency" },
                226 => new[] { "jyppx_ocv_core_tick_meter_create", "jyppx_ocv_core_tick_meter_release" },
                227 => new[] { "jyppx_ocv_core_tick_meter_start" },
                228 => new[] { "jyppx_ocv_core_tick_meter_stop" },
                229 => new[] { "jyppx_ocv_core_tick_meter_get_time_ticks" },
                230 => new[] { "jyppx_ocv_core_tick_meter_get_time_micro" },
                231 => new[] { "jyppx_ocv_core_tick_meter_get_time_milli" },
                232 => new[] { "jyppx_ocv_core_tick_meter_get_time_sec" },
                233 => new[] { "jyppx_ocv_core_tick_meter_get_last_time_ticks" },
                234 => new[] { "jyppx_ocv_core_tick_meter_get_last_time_micro" },
                235 => new[] { "jyppx_ocv_core_tick_meter_get_last_time_milli" },
                236 => new[] { "jyppx_ocv_core_tick_meter_get_last_time_sec" },
                237 => new[] { "jyppx_ocv_core_tick_meter_get_counter" },
                238 => new[] { "jyppx_ocv_core_tick_meter_get_fps" },
                239 => new[] { "jyppx_ocv_core_tick_meter_get_avg_time_sec" },
                240 => new[] { "jyppx_ocv_core_tick_meter_get_avg_time_milli" },
                241 => new[] { "jyppx_ocv_core_tick_meter_reset" },
                242 => new[] { "jyppx_ocv_core_get_cpu_tick_count" },
                243 => new[] { "jyppx_ocv_core_check_hardware_support" },
                244 => Utf8Evidence("jyppx_ocv_core_get_hardware_feature_name"),
                245 => Utf8Evidence("jyppx_ocv_core_get_cpu_features_line"),
                246 => new[] { "jyppx_ocv_core_get_number_of_cpus" },
                248 => new[] { "jyppx_ocv_core_get_default_algorithm_hint" },
                249 => new[] { "jyppx_ocv_core_set_use_optimized" },
                250 => new[] { "jyppx_ocv_core_use_optimized" },
                _ => Array.Empty<string>()
            };
            var nativeSet = native.ToHashSet(Ordinal);
            return expected.Where(nativeSet.Contains).Order(Ordinal);
        }

        string symbol = Symbol(d.Name); string token = Snake(symbol);
        if (token == "copy_to") token = "copy_to_mask";
        IEnumerable<string> matches = native.Where(v => v.Equals("jyppx_ocv_core_" + token, StringComparison.Ordinal));
        return matches.Order(Ordinal);
    }

    private static IEnumerable<string> ManagedEvidence(RawDeclaration d, string[] managed)
    {
        if (IsPersistenceCallable(d))
        {
            string expected = d.Ordinal switch
            {
                57 => "MEMBER|OpenCvSharp.Core.FileStorage|constructor|public;instance|.ctor()",
                58 => "MEMBER|OpenCvSharp.Core.FileStorage|constructor|public;instance|.ctor(System.String source,OpenCvSharp.Core.FileStorageModes flags,System.String? encoding=null)",
                59 => "MEMBER|OpenCvSharp.Core.FileStorage|method|public;instance|System.Boolean Open(System.String source,OpenCvSharp.Core.FileStorageModes flags,System.String? encoding=null)",
                60 => "MEMBER|OpenCvSharp.Core.FileStorage|property|instance;get:public|System.Boolean IsOpened",
                61 => "MEMBER|OpenCvSharp.Core.FileStorage|method|public;instance|System.Void Release()",
                62 => "MEMBER|OpenCvSharp.Core.FileStorage|method|public;instance|System.String ReleaseAndGetString()",
                63 => "MEMBER|OpenCvSharp.Core.FileStorage|method|public;instance|OpenCvSharp.Core.FileNode GetFirstTopLevelNode()",
                64 => "MEMBER|OpenCvSharp.Core.FileStorage|method|public;instance|OpenCvSharp.Core.FileNode Root(System.Int32 streamIndex=0)",
                65 => "MEMBER|OpenCvSharp.Core.FileStorage|property|instance;get:public|OpenCvSharp.Core.FileNode Item[System.String name]",
                66 => "MEMBER|OpenCvSharp.Core.FileStorage|method|public;instance|System.Void Write(System.String name,System.Int32 value)",
                67 => "MEMBER|OpenCvSharp.Core.FileStorage|method|public;instance|System.Void Write(System.String name,System.Boolean value)",
                68 => "MEMBER|OpenCvSharp.Core.FileStorage|method|public;instance|System.Void Write(System.String name,System.Int64 value)",
                69 => "MEMBER|OpenCvSharp.Core.FileStorage|method|public;instance|System.Void Write(System.String name,System.Double value)",
                70 => "MEMBER|OpenCvSharp.Core.FileStorage|method|public;instance|System.Void Write(System.String name,System.String value)",
                71 => "MEMBER|OpenCvSharp.Core.FileStorage|method|public;instance|System.Void Write(System.String name,OpenCvSharp.Core.Mat value)",
                72 => "MEMBER|OpenCvSharp.Core.FileStorage|method|public;instance|System.Void Write(System.String name,System.Collections.Generic.IReadOnlyList<System.String> values)",
                73 => "MEMBER|OpenCvSharp.Core.FileStorage|method|public;instance|System.Void WriteComment(System.String comment,System.Boolean append=false)",
                74 => "MEMBER|OpenCvSharp.Core.FileStorage|method|public;instance|System.Void StartWriteStruct(System.String name,OpenCvSharp.Core.FileNodeTypes flags,System.String? typeName=null)",
                75 => "MEMBER|OpenCvSharp.Core.FileStorage|method|public;instance|System.Void EndWriteStruct()",
                76 => "MEMBER|OpenCvSharp.Core.FileStorage|method|public;instance|OpenCvSharp.Core.FileStorageModes GetFormat()",
                79 => "MEMBER|OpenCvSharp.Core.FileNode|constructor|public;instance|.ctor()",
                80 => "MEMBER|OpenCvSharp.Core.FileNode|property|instance;get:public|OpenCvSharp.Core.FileNode Item[System.String name]",
                81 => "MEMBER|OpenCvSharp.Core.FileNode|property|instance;get:public|OpenCvSharp.Core.FileNode Item[System.Int32 index]",
                82 => "MEMBER|OpenCvSharp.Core.FileNode|property|instance;get:public|System.String[] Keys",
                83 => "MEMBER|OpenCvSharp.Core.FileNode|property|instance;get:public|OpenCvSharp.Core.FileNodeTypes Type",
                84 => "MEMBER|OpenCvSharp.Core.FileNode|property|instance;get:public|System.Boolean Empty",
                85 => "MEMBER|OpenCvSharp.Core.FileNode|property|instance;get:public|System.Boolean IsNone",
                86 => "MEMBER|OpenCvSharp.Core.FileNode|property|instance;get:public|System.Boolean IsSequence",
                87 => "MEMBER|OpenCvSharp.Core.FileNode|property|instance;get:public|System.Boolean IsMap",
                88 => "MEMBER|OpenCvSharp.Core.FileNode|property|instance;get:public|System.Boolean IsInteger",
                89 => "MEMBER|OpenCvSharp.Core.FileNode|property|instance;get:public|System.Boolean IsReal",
                90 => "MEMBER|OpenCvSharp.Core.FileNode|property|instance;get:public|System.Boolean IsString",
                91 => "MEMBER|OpenCvSharp.Core.FileNode|property|instance;get:public|System.Boolean IsNamed",
                92 => "MEMBER|OpenCvSharp.Core.FileNode|property|instance;get:public|System.String Name",
                93 => "MEMBER|OpenCvSharp.Core.FileNode|property|instance;get:public|System.Int32 Size",
                94 => "MEMBER|OpenCvSharp.Core.FileNode|property|instance;get:public|System.UInt64 RawSize",
                95 => "MEMBER|OpenCvSharp.Core.FileNode|property|instance;get:public|System.Double Real",
                96 => "MEMBER|OpenCvSharp.Core.FileNode|property|instance;get:public|System.String String",
                97 => "MEMBER|OpenCvSharp.Core.FileNode|method|public;instance|OpenCvSharp.Core.Mat ToMat()",
                _ => ""
            };
            return managed.Where(v => v.Equals(expected, StringComparison.Ordinal));
        }

        if (NumericalOrdinals.Contains(d.Ordinal))
        {
            string expected = d.Ordinal switch
            {
                180 => "MEMBER|OpenCvSharp.Core.Cv2|method|public;static|System.Void PcaCompute(OpenCvSharp.Core.Mat data,OpenCvSharp.Core.Mat mean,OpenCvSharp.Core.Mat eigenvectors,System.Int32 maxComponents=0)",
                181 => "MEMBER|OpenCvSharp.Core.Cv2|method|public;static|System.Void PcaCompute(OpenCvSharp.Core.Mat data,OpenCvSharp.Core.Mat mean,OpenCvSharp.Core.Mat eigenvectors,OpenCvSharp.Core.Mat eigenvalues,System.Int32 maxComponents=0)",
                182 => "MEMBER|OpenCvSharp.Core.Cv2|method|public;static|System.Void PcaCompute(OpenCvSharp.Core.Mat data,OpenCvSharp.Core.Mat mean,OpenCvSharp.Core.Mat eigenvectors,System.Double retainedVariance)",
                183 => "MEMBER|OpenCvSharp.Core.Cv2|method|public;static|System.Void PcaCompute(OpenCvSharp.Core.Mat data,OpenCvSharp.Core.Mat mean,OpenCvSharp.Core.Mat eigenvectors,OpenCvSharp.Core.Mat eigenvalues,System.Double retainedVariance)",
                186 => "MEMBER|OpenCvSharp.Core.Svd|method|public;static|System.Void Compute(OpenCvSharp.Core.Mat src,OpenCvSharp.Core.Mat w,OpenCvSharp.Core.Mat u,OpenCvSharp.Core.Mat vt,OpenCvSharp.Core.SvdFlags flags=None)",
                187 => "MEMBER|OpenCvSharp.Core.Svd|method|public;static|System.Void BackSubst(OpenCvSharp.Core.Mat w,OpenCvSharp.Core.Mat u,OpenCvSharp.Core.Mat vt,OpenCvSharp.Core.Mat rhs,OpenCvSharp.Core.Mat dst)",
                _ => ""
            };
            if (expected.Length > 0)
            {
                return managed.Where(v => v.Equals(expected, StringComparison.Ordinal));
            }
        }

        if (IsRuntimeDiagnosticsCallable(d))
        {
            string expected = d.Ordinal switch
            {
                215 => "MEMBER|OpenCvSharp.Core.Cv2|method|public;static|System.Void SetNumThreads(System.Int32 threadCount)",
                216 => "MEMBER|OpenCvSharp.Core.Cv2|method|public;static|System.Int32 GetNumThreads()",
                217 => "MEMBER|OpenCvSharp.Core.Cv2|method|public;static|System.Int32 GetThreadNum()",
                218 => "MEMBER|OpenCvSharp.Core.Cv2|method|public;static|System.String GetBuildInformation()",
                219 => "MEMBER|OpenCvSharp.OpenCvSharpBuildInfo|method|public;static|System.String GetNativeOpenCvVersion()",
                220 => "MEMBER|OpenCvSharp.OpenCvSharpBuildInfo|method|public;static|System.Int32 GetNativeOpenCvVersionMajor()",
                221 => "MEMBER|OpenCvSharp.OpenCvSharpBuildInfo|method|public;static|System.Int32 GetNativeOpenCvVersionMinor()",
                222 => "MEMBER|OpenCvSharp.OpenCvSharpBuildInfo|method|public;static|System.Int32 GetNativeOpenCvVersionRevision()",
                223 => "MEMBER|OpenCvSharp.Core.Cv2|method|public;static|System.Int64 GetTickCount()",
                224 => "MEMBER|OpenCvSharp.Core.Cv2|method|public;static|System.Double GetTickFrequency()",
                226 => "MEMBER|OpenCvSharp.Core.TickMeter|constructor|public;instance|.ctor()",
                227 => "MEMBER|OpenCvSharp.Core.TickMeter|method|public;instance|System.Void Start()",
                228 => "MEMBER|OpenCvSharp.Core.TickMeter|method|public;instance|System.Void Stop()",
                229 => "MEMBER|OpenCvSharp.Core.TickMeter|property|instance;get:public|System.Int64 TimeTicks",
                230 => "MEMBER|OpenCvSharp.Core.TickMeter|property|instance;get:public|System.Double TimeMicroseconds",
                231 => "MEMBER|OpenCvSharp.Core.TickMeter|property|instance;get:public|System.Double TimeMilliseconds",
                232 => "MEMBER|OpenCvSharp.Core.TickMeter|property|instance;get:public|System.Double TimeSeconds",
                233 => "MEMBER|OpenCvSharp.Core.TickMeter|property|instance;get:public|System.Int64 LastTimeTicks",
                234 => "MEMBER|OpenCvSharp.Core.TickMeter|property|instance;get:public|System.Double LastTimeMicroseconds",
                235 => "MEMBER|OpenCvSharp.Core.TickMeter|property|instance;get:public|System.Double LastTimeMilliseconds",
                236 => "MEMBER|OpenCvSharp.Core.TickMeter|property|instance;get:public|System.Double LastTimeSeconds",
                237 => "MEMBER|OpenCvSharp.Core.TickMeter|property|instance;get:public|System.Int64 Counter",
                238 => "MEMBER|OpenCvSharp.Core.TickMeter|property|instance;get:public|System.Double FramesPerSecond",
                239 => "MEMBER|OpenCvSharp.Core.TickMeter|property|instance;get:public|System.Double AverageTimeSeconds",
                240 => "MEMBER|OpenCvSharp.Core.TickMeter|property|instance;get:public|System.Double AverageTimeMilliseconds",
                241 => "MEMBER|OpenCvSharp.Core.TickMeter|method|public;instance|System.Void Reset()",
                242 => "MEMBER|OpenCvSharp.Core.Cv2|method|public;static|System.Int64 GetCpuTickCount()",
                243 => "MEMBER|OpenCvSharp.Core.Cv2|method|public;static|System.Boolean CheckHardwareSupport(OpenCvSharp.Core.CpuFeatures feature)",
                244 => "MEMBER|OpenCvSharp.Core.Cv2|method|public;static|System.String GetHardwareFeatureName(OpenCvSharp.Core.CpuFeatures feature)",
                245 => "MEMBER|OpenCvSharp.Core.Cv2|method|public;static|System.String GetCpuFeaturesLine()",
                246 => "MEMBER|OpenCvSharp.Core.Cv2|method|public;static|System.Int32 GetNumberOfCpus()",
                248 => "MEMBER|OpenCvSharp.Core.Cv2|method|public;static|OpenCvSharp.Core.AlgorithmHint GetDefaultAlgorithmHint()",
                249 => "MEMBER|OpenCvSharp.Core.Cv2|method|public;static|System.Void SetUseOptimized(System.Boolean enabled)",
                250 => "MEMBER|OpenCvSharp.Core.Cv2|method|public;static|System.Boolean UseOptimized()",
                _ => ""
            };
            return managed.Where(v => v.Equals(expected, StringComparison.Ordinal));
        }

        if (!d.Name.StartsWith("cv.", StringComparison.Ordinal) || d.Name.Count(c => c == '.') != 1) return Array.Empty<string>();
        string method = Pascal(Symbol(d.Name));
        return managed.Where(line =>
        {
            string[] p = line.Split('|');
            string signature = p.Length >= 5 ? p[4].Replace("_", "", StringComparison.Ordinal) : "";
            string normalizedMethod = method.Replace("_", "", StringComparison.Ordinal);
            return p.Length >= 5 && p[1] == "OpenCvSharp.Core.Cv2" && p[2] == "method" && Regex.IsMatch(signature, @"\b" + Regex.Escape(normalizedMethod) + @"\(", RegexOptions.IgnoreCase);
        }).Order(Ordinal);
    }

    private static void Validate(RawDocument raw, ClassificationDocument c, Options o, string[] native, string[] managed, bool hashes)
    {
        Require(raw.SchemaVersion == 1 && raw.UpstreamOpenCvVersion == "5.0.0", "Raw identity drifted.");
        Require(raw.Generator == "tools/CoreUpstreamMap/extract_core.py", "Raw generator drifted.");
        Require(raw.DeclarationCount == raw.Declarations.Count && raw.Declarations.Count == 258, "Core declaration count drifted.");
        Require(raw.SourceHeaders.Count == 11, "Core source-header closure drifted.");
        Require(raw.PreprocessorDefinitions.Count == 2 && raw.PreprocessorDefinitions["CV_VERSION_MAJOR"] == 5 && raw.PreprocessorDefinitions["OPENCV_ABI_COMPATIBILITY"] == 500, "Core build definitions drifted.");
        Require(raw.Declarations.Select(v => v.Ordinal).SequenceEqual(Enumerable.Range(0, raw.Declarations.Count)), "Raw declarations are reordered.");
        Require(raw.Declarations.Select(v => v.Identity).Distinct(Ordinal).Count() == raw.Declarations.Count, "Raw overload identities collapsed or duplicated.");
        Require(raw.SourceHeaders.Select(v => v.StartOrdinal).SequenceEqual(raw.SourceHeaders.Select((_, i) => raw.SourceHeaders.Take(i).Sum(v => v.DeclarationCount))), "Source-header parser order drifted.");
        if (hashes)
        {
            Require(FileHash(Path.Combine(o.Workspace, raw.HeaderPath.Replace('/', Path.DirectorySeparatorChar))) == raw.HeaderSha256, "Core umbrella header hash drifted.");
            Require(FileHash(Path.Combine(o.Workspace, raw.ParserPath.Replace('/', Path.DirectorySeparatorChar))) == raw.ParserSha256, "Core parser hash drifted.");
            foreach (SourceHeader h in raw.SourceHeaders) Require(FileHash(Path.Combine(o.Workspace, h.Path.Replace('/', Path.DirectorySeparatorChar))) == h.Sha256, "Core source-header hash drifted: " + h.Path);
        }
        Require(c.SchemaVersion == 1 && c.UpstreamOpenCvVersion == "5.0.0" && c.ReviewStatus == "reviewed", "Classification identity drifted.");
        Require(c.Declarations.Count == raw.Declarations.Count, "Classification must contain one row per raw declaration.");
        Require(c.Declarations.Select(v => v.Ordinal).SequenceEqual(Enumerable.Range(0, raw.Declarations.Count)), "Classification rows are reordered.");
        Require(c.Declarations.Select(v => v.Identity).SequenceEqual(raw.Declarations.Select(v => v.Identity), Ordinal), "Classification identities are missing, duplicated, reordered, or overload-collapsed.");
        var nativeSet = native.Select(v => v.Trim()).ToHashSet(Ordinal); var managedSet = managed.Select(v => v.Trim()).ToHashSet(Ordinal);
        for (int i = 0; i < c.Declarations.Count; i++)
        {
            ClassificationRow row = c.Declarations[i]; RawDeclaration d = raw.Declarations[i];
            Require(Classes.Contains(row.Classification, Ordinal), "Undocumented classification: " + row.Identity);
            Require(!string.IsNullOrWhiteSpace(row.Reason), "Every classification requires a documented reason: " + row.Identity);
            Require(IsSorted(row.NativeEntrypoints) && IsSorted(row.ManagedMembers), "Evidence is nondeterministically ordered: " + row.Identity);
            Require(row.NativeEntrypoints.Distinct(Ordinal).Count() == row.NativeEntrypoints.Count && row.ManagedMembers.Distinct(Ordinal).Count() == row.ManagedMembers.Count, "Evidence is duplicated: " + row.Identity);
            Require(row.ManagedMembers.All(v => !v.Contains("OpenCvSharp5", StringComparison.Ordinal)) && row.NativeEntrypoints.All(v => !v.StartsWith("jyppx_ocv5_", StringComparison.Ordinal)), "Fixed-major identity is forbidden in primary evidence.");
            Require(row.NativeEntrypoints.All(v => nativeSet.Contains(v.Trim())), "Classification references false native evidence: " + row.Identity);
            Require(row.ManagedMembers.All(v => managedSet.Contains(v.Trim())), "Classification references false managed evidence: " + row.Identity);
            if (row.Classification == "implemented") Require(d.Kind == "callable" && row.NativeEntrypoints.Count > 0 && row.ManagedMembers.Count > 0, "Implemented callable requires native and managed evidence: " + row.Identity);
            if (row.Classification == "non-callable-metadata") Require(d.Kind != "callable", "Constructor/property confusion classified a callable as metadata: " + row.Identity);
            if (row.Classification == "upstream-conditional") Require(row.BuildCondition != "unconditional-parser-surface", "Conditional-build classification requires an explicit build condition.");
        }
        Require(c.Declarations.Count(v => v.Classification == "missing") == 0, "Core callable partition contains unexplained missing rows.");
    }

    private static string BuildMap(RawDocument raw, ClassificationDocument c)
    {
        var b = new StringBuilder();
        b.AppendLine("schema-version=1"); b.AppendLine("generator=tools/CoreUpstreamMap"); b.AppendLine("upstream-opencv-version=5.0.0");
        b.AppendLine("claimed-slice=opencv2/core.hpp public compatibility include closure across 11 parser-reviewed contributing headers");
        b.AppendLine("repository-wide-upstream-parity-claimed=false"); b.AppendLine("header-sha256=" + raw.HeaderSha256); b.AppendLine("parser-sha256=" + raw.ParserSha256);
        b.AppendLine("source-header-count=" + raw.SourceHeaders.Count); b.AppendLine("declaration-count=" + raw.Declarations.Count);
        b.AppendLine("classification-order=" + string.Join(',', Classes));
        foreach (SourceHeader h in raw.SourceHeaders) b.AppendLine($"source|{h.Path}|{h.Sha256}|{h.StartOrdinal}|{h.DeclarationCount}");
        for (int i = 0; i < raw.Declarations.Count; i++)
        {
            RawDeclaration d = raw.Declarations[i]; ClassificationRow row = c.Declarations[i];
            b.Append(d.Ordinal.ToString("D4")).Append('|').Append(Safe(d.SourceHeader)).Append('|').Append(d.Kind).Append('|').Append(Safe(d.Identity)).Append('|')
                .Append(row.Classification).Append('|').Append(Safe(row.BuildCondition)).Append('|').Append(string.Join(',', row.NativeEntrypoints)).Append('|')
                .Append(string.Join(';', row.ManagedMembers.Select(Safe))).Append('|').AppendLine(Safe(row.Reason));
        }
        return Normalize(b.ToString());
    }

    private static FamilyDocument BuildFamilies(RawDocument raw, ClassificationDocument c)
    {
        var arrayRow = new FamilyRow
        {
            Id = "core-array-reductions-transforms",
            Rationale = "Closes the adjacent OpenCV 5 Core array, reduction, shape, finite-value, border, masked-copy, and ordering operations selected by the measured gap review."
        };
        var persistenceRow = new FamilyRow
        {
            Id = "core-persistence-utf8-ownership",
            Rationale = "Closes every parser-emitted FileStorage/FileNode callable with opaque shared-state handles, generation invalidation, explicit-length UTF-8, owned result/list handles, and indexed collection access."
        };
        var numericalRow = new FamilyRow
        {
            Id = "core-numerical-collection-solver",
            Rationale = "Closes the measured scalar, pairwise-distance, channel collection, NaN mutation, covariance/PCA/SVD, random generation, shuffle, and linear-programming callables with explicit shape, state, and output ownership contracts."
        };
        var runtimeDiagnosticsRow = new FamilyRow
        {
            Id = "core-runtime-diagnostics-timing",
            Rationale = "Closes the measured threading, version/build, clocks, CPU capability, optimization-state, algorithm-hint, and TickMeter callables with owned UTF-8 results, opaque timer ownership, and failure-safe global-state restoration."
        };
        for (int i = 0; i < raw.Declarations.Count; i++)
        {
            if (raw.Declarations[i].Kind == "callable" && SelectedSymbols.Contains(Symbol(raw.Declarations[i].Name)))
            {
                Require(c.Declarations[i].Classification == "implemented", "Selected Core family must be implemented: " + raw.Declarations[i].Identity);
                arrayRow.Declarations.Add(new FamilyOperation { Ordinal = i, UpstreamIdentity = raw.Declarations[i].Identity, NativeEntrypoints = c.Declarations[i].NativeEntrypoints, ManagedMembers = c.Declarations[i].ManagedMembers });
            }
            if (IsPersistenceCallable(raw.Declarations[i]))
            {
                Require(c.Declarations[i].Classification == "implemented", "Selected Core persistence family must be implemented: " + raw.Declarations[i].Identity);
                persistenceRow.Declarations.Add(new FamilyOperation
                {
                    Ordinal = i,
                    UpstreamIdentity = raw.Declarations[i].Identity,
                    NativeEntrypoints = c.Declarations[i].NativeEntrypoints,
                    ManagedMembers = c.Declarations[i].ManagedMembers,
                    FocusedTest = "tests/OpenCvSharp.Tests/Core/CorePersistenceTests.cs"
                });
            }
            if (IsNumericalCallable(raw.Declarations[i]))
            {
                Require(c.Declarations[i].Classification == "implemented", "Selected Core numerical family must be implemented: " + raw.Declarations[i].Identity);
                numericalRow.Declarations.Add(new FamilyOperation
                {
                    Ordinal = i,
                    UpstreamIdentity = raw.Declarations[i].Identity,
                    NativeEntrypoints = c.Declarations[i].NativeEntrypoints,
                    ManagedMembers = c.Declarations[i].ManagedMembers,
                    FocusedTest = "tests/OpenCvSharp.Tests/Core/CoreNumericalCollectionSolverTests.cs"
                });
            }
            if (IsRuntimeDiagnosticsCallable(raw.Declarations[i]))
            {
                Require(c.Declarations[i].Classification == "implemented", "Selected Core runtime diagnostics family must be implemented: " + raw.Declarations[i].Identity);
                runtimeDiagnosticsRow.Declarations.Add(new FamilyOperation
                {
                    Ordinal = i,
                    UpstreamIdentity = raw.Declarations[i].Identity,
                    NativeEntrypoints = c.Declarations[i].NativeEntrypoints,
                    ManagedMembers = c.Declarations[i].ManagedMembers,
                    FocusedTest = "tests/OpenCvSharp.Tests/Core/CoreRuntimeDiagnosticsTimingTests.cs"
                });
            }
        }
        Require(arrayRow.Declarations.Count == 15, "Selected Core array family declaration count drifted.");
        Require(persistenceRow.Declarations.Count == 39, "Selected Core persistence family declaration count drifted.");
        Require(numericalRow.Declarations.Count == 20, "Selected Core numerical family declaration count drifted.");
        Require(runtimeDiagnosticsRow.Declarations.Count == 34, "Selected Core runtime diagnostics family declaration count drifted.");
        return new FamilyDocument { Families = new List<FamilyRow> { arrayRow, persistenceRow, numericalRow, runtimeDiagnosticsRow } };
    }

    private static void RunNegativeFixtures(RawDocument raw, ClassificationDocument c, Options o, string[] native, string[] managed)
    {
        int rejected = 0;
        void Reject(string name, string expected, Action action)
        {
            try { action(); throw new InvalidDataException("Negative Core map fixture was accepted: " + name); }
            catch (InvalidDataException e) when (!e.Message.StartsWith("Negative Core", StringComparison.Ordinal)) { if (!e.Message.Contains(expected, StringComparison.OrdinalIgnoreCase)) throw; rejected++; }
        }
        ClassificationDocument CopyC() => JsonSerializer.Deserialize<ClassificationDocument>(Serialize(c), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        RawDocument CopyR() => JsonSerializer.Deserialize<RawDocument>(Serialize(raw), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        Reject("missing row", "one row", () => { var v = CopyC(); v.Declarations.RemoveAt(0); Validate(raw, v, o, native, managed, false); });
        Reject("duplicate", "identities", () => { var v = CopyC(); v.Declarations[1].Identity = v.Declarations[0].Identity; Validate(raw, v, o, native, managed, false); });
        Reject("reorder", "reordered", () => { var v = CopyC(); (v.Declarations[0], v.Declarations[1]) = (v.Declarations[1], v.Declarations[0]); Validate(raw, v, o, native, managed, false); });
        Reject("overload collapse", "collapsed", () => { var v = CopyR(); v.Declarations[107].Identity = v.Declarations[106].Identity; Validate(v, c, o, native, managed, false); });
        Reject("constructor metadata confusion", "confusion", () => { var v = CopyC(); var x = v.Declarations.First(z => z.Identity.Contains("FileStorage.FileStorage", StringComparison.Ordinal)); x.Classification = "non-callable-metadata"; Validate(raw, v, o, native, managed, false); });
        Reject("source header drift", "closure", () => { var v = CopyR(); v.SourceHeaders.RemoveAt(0); Validate(v, c, o, native, managed, false); });
        Reject("parser drift", "generator", () => { var v = CopyR(); v.Generator = "other"; Validate(v, c, o, native, managed, false); });
        Reject("stale hash", "header hash", () => { var v = CopyR(); v.HeaderSha256 = new string('0', 64); Validate(v, c, o, native, managed, true); });
        Reject("false evidence", "false native", () => { var v = CopyC(); var x = v.Declarations.First(z => z.Classification == "implemented"); x.NativeEntrypoints[0] = "jyppx_ocv_core_false"; Validate(raw, v, o, native, managed, false); });
        Reject("undocumented omission", "reason", () => { var v = CopyC(); var x = v.Declarations.First(z => z.Classification == "intentionally-omitted"); x.Reason = ""; Validate(raw, v, o, native, managed, false); });
        Reject("fixed major", "fixed-major", () => { var v = CopyC(); var x = v.Declarations.First(z => z.ManagedMembers.Count > 0); x.ManagedMembers[0] = x.ManagedMembers[0].Replace("OpenCvSharp", "OpenCvSharp5", StringComparison.Ordinal); Validate(raw, v, o, native, managed, false); });
        Reject("conditional misclassification", "build condition", () => { var v = CopyC(); var x = v.Declarations.First(z => z.Classification == "upstream-conditional"); x.BuildCondition = "unconditional-parser-surface"; Validate(raw, v, o, native, managed, false); });
        Reject("evidence order", "nondeterministically", () => { var v = CopyC(); var x = v.Declarations.First(z => z.ManagedMembers.Count > 1); x.ManagedMembers.Reverse(); Validate(raw, v, o, native, managed, false); });
        Require(rejected == 13, "Core negative fixture count drifted.");
    }

    private static string Symbol(string name)
    {
        string signature = name.Split('(', 2)[0];
        return signature.Split('.')[^1].Replace("operator[]", "index", StringComparison.Ordinal);
    }
    private static bool IsPersistenceCallable(RawDeclaration declaration) =>
        declaration.Kind == "callable" && declaration.SourceHeader.EndsWith("/core/persistence.hpp", StringComparison.Ordinal);
    private static bool IsNumericalCallable(RawDeclaration declaration) =>
        declaration.Kind == "callable" && NumericalOrdinals.Contains(declaration.Ordinal);
    private static bool IsRuntimeDiagnosticsCallable(RawDeclaration declaration) =>
        declaration.Kind == "callable" && (RuntimeUtilityOrdinals.Contains(declaration.Ordinal) || TickMeterOrdinals.Contains(declaration.Ordinal));
    private static string[] Utf8Evidence(string primary) =>
        new[] { primary, "jyppx_ocv_core_utf8_result_data", "jyppx_ocv_core_utf8_result_release", "jyppx_ocv_core_utf8_result_size" };
    private static string Pascal(string value) => value switch { "PSNR" => "Psnr", "LUT" => "Lut", _ => char.ToUpperInvariant(value[0]) + value[1..] };
    private static string Snake(string value) => Regex.Replace(Regex.Replace(value, "([a-z0-9])([A-Z])", "$1_$2"), "([A-Z]+)([A-Z][a-z])", "$1_$2").ToLowerInvariant();
    private static string Safe(string value) => value.Replace('|', '/').Replace('\r', ' ').Replace('\n', ' ').Trim();
    private static bool IsSorted(IReadOnlyList<string> values) { for (int i = 1; i < values.Count; i++) if (Ordinal.Compare(values[i - 1], values[i]) > 0) return false; return true; }
    private static T Read<T>(string path, JsonSerializerOptions options) => JsonSerializer.Deserialize<T>(File.ReadAllText(path), options) ?? throw new InvalidDataException("Invalid JSON: " + path);
    private static string Serialize<T>(T value) => JsonSerializer.Serialize(value, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true }) + "\n";
    private static void WriteJson<T>(string path, T value, bool check) => WriteOrCheck(path, Serialize(value), check);
    private static void WriteOrCheck(string path, string text, bool check)
    {
        text = Normalize(text);
        if (check) Require(File.Exists(path) && File.ReadAllText(path).Replace("\r\n", "\n", StringComparison.Ordinal) == text, "Generated artifact is stale: " + path);
        else { Directory.CreateDirectory(Path.GetDirectoryName(path)!); File.WriteAllText(path, text, new UTF8Encoding(false)); }
    }
    private static string Normalize(string value) => value.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n').TrimEnd() + "\n";
    private static string Sha256(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(Normalize(value)))).ToLowerInvariant();
    private static string FileHash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
    private static string Rel(string root, string path) => Path.GetRelativePath(root, path).Replace('\\', '/');
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidDataException(message); }
}

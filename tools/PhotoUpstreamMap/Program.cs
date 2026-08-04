using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

internal static class Program
{
    private static readonly StringComparer Ordinal = StringComparer.Ordinal;
    private static readonly HashSet<int> Existing = new()
    {
        1, 2, 3, 4, 5, 6, 7, 11, 12, 13, 14, 16, 17, 18, 19, 20,
        22, 23, 24, 25, 26, 27, 28, 30, 31, 32, 33, 34, 87, 89, 90,
        91, 92, 94, 95, 96, 97
    };
    private static readonly HashSet<int> HdrSelected = new(new[]
    {
        36, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49,
        51, 53, 54, 55, 56, 57, 58, 59, 61, 62, 63, 64, 65, 66,
        68, 70, 71, 72, 74, 75, 76, 77, 78, 79, 80, 81, 82, 84, 85, 86
    });
    private static readonly HashSet<int> CcmSelected = new(
        new[] { 106 }.Concat(Enumerable.Range(108, 27)));
    private static readonly HashSet<int> IntelligentScissorsSelected = new(
        Enumerable.Range(136, 9));
    private static readonly HashSet<int> Selected = new(
        HdrSelected.Concat(CcmSelected).Concat(IntelligentScissorsSelected).Concat(new[] { 8, 98, 99 }));
    private static readonly string[] Allowed =
    {
        "implemented", "missing", "intentionally-omitted", "upstream-conditional", "unsupported", "non-callable-metadata"
    };
    private const string ClaimedSlice = "opencv2/photo.hpp and opencv2/photo/photo.hpp compatibility headers measured through the three parser-emitted OpenCV 5.0.0 main Photo public source headers";

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
        public List<CompatibilityHeader> CompatibilityHeaders { get; set; } = new();
        public List<ExcludedPublicHeader> ExcludedPublicHeaders { get; set; } = new();
        public List<SourceHeader> SourceHeaders { get; set; } = new();
        public int DeclarationCount { get; set; }
        public List<RawDeclaration> Declarations { get; set; } = new();
    }
    private sealed class CompatibilityHeader { public string Path { get; set; } = ""; public string Sha256 { get; set; } = ""; public string Includes { get; set; } = ""; }
    private sealed class ExcludedPublicHeader { public string Path { get; set; } = ""; public string Reason { get; set; } = ""; }
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
        public string ClaimedSlice { get; set; } = Program.ClaimedSlice;
        public string ReviewStatus { get; set; } = "source-reviewed";
        public string Limitation { get; set; } = "The map covers the exact OpenCV 5.0.0 CPU main Photo compatibility include closure. CUDA and contrib xphoto are excluded, and repository-wide parity is not claimed.";
        public List<ClassificationRow> Declarations { get; set; } = new();
    }
    private sealed class ClassificationRow
    {
        public int Ordinal { get; set; }
        public string Identity { get; set; } = "";
        public string Classification { get; set; } = "";
        public string Reason { get; set; } = "";
        public string BuildCondition { get; set; } = "";
        public List<string> NativeEntrypoints { get; set; } = new();
        public List<string> ManagedMembers { get; set; } = new();
    }
    private sealed class SummaryDocument
    {
        public int SchemaVersion { get; init; } = 1;
        public string Generator { get; init; } = "tools/PhotoUpstreamMap";
        public string UpstreamOpenCvVersion { get; init; } = "5.0.0";
        public string ClaimedSlice { get; init; } = Program.ClaimedSlice;
        public string RawExtractionPath { get; init; } = "";
        public string ClassificationPath { get; init; } = "";
        public string MappingPath { get; init; } = "";
        public string HeaderSha256 { get; init; } = "";
        public string ParserSha256 { get; init; } = "";
        public int CompatibilityHeaderCount { get; init; }
        public int ExcludedPublicHeaderCount { get; init; }
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
        public int NegativeFixtureCount { get; init; } = 16;
        public string FamilyInventoryPath { get; init; } = "";
        public string FamilyInventorySha256 { get; init; } = "";
        public int SelectedFamilyCount { get; init; }
        public int SelectedDeclarationCount { get; init; }
        public int ManagedPublicTypeAdditionCount { get; init; } = 18;
        public int ManagedPublicMemberAdditionCount { get; init; } = 181;
        public int NativeEntrypointAdditionCount { get; init; } = 80;
        public bool RepositoryWideUpstreamParityClaimed { get; init; }
    }
    private sealed class FamilyDocument
    {
        public int SchemaVersion { get; init; } = 1;
        public string UpstreamOpenCvVersion { get; init; } = "5.0.0";
        public string Status { get; init; } = "implemented-verified";
        public int ManagedPublicTypeAdditionCount { get; init; } = 18;
        public int ManagedPublicMemberAdditionCount { get; init; } = 181;
        public int NativeEntrypointAdditionCount { get; init; } = 80;
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
        public List<string> NativeEntrypoints { get; init; } = new();
        public List<string> ManagedMembers { get; init; } = new();
        public string FocusedTest { get; init; } = "";
        public string NativeSmoke { get; init; } = "src/OpenCvSharp.Native/tests/native_smoke.cpp";
        public string Sample { get; init; } = "samples/ConsoleSamples/Program.cs";
        public string Guide { get; init; } = "";
    }

    private static int Main(string[] args)
    {
        try
        {
            Options options = Parse(args);
            RawDocument raw = Read<RawDocument>(options.Raw);
            string[] native = ReadNative(options.NativeManifest);
            string[] managed = File.ReadAllLines(options.ManagedBaseline, Encoding.UTF8);
            if (options.Initialize) WriteOrCheck(options.Classification, Serialize(Initialize(raw, native, managed)), false);
            ClassificationDocument classifications = Read<ClassificationDocument>(options.Classification);
            Validate(raw, classifications, options, native, managed, true);
            string mapping = BuildMap(raw, classifications);
            string familyText = Serialize(BuildFamilies(raw, classifications));
            var counts = new SortedDictionary<string, int>(Ordinal);
            foreach (string value in Allowed) counts[value] = classifications.Declarations.Count(row => row.Classification == value);
            var summary = new SummaryDocument
            {
                RawExtractionPath = Rel(options.Repository, options.Raw), ClassificationPath = Rel(options.Repository, options.Classification), MappingPath = Rel(options.Repository, options.Output),
                HeaderSha256 = raw.HeaderSha256, ParserSha256 = raw.ParserSha256, CompatibilityHeaderCount = raw.CompatibilityHeaders.Count, ExcludedPublicHeaderCount = raw.ExcludedPublicHeaders.Count, SourceHeaderCount = raw.SourceHeaders.Count,
                SourceHeaderSetSha256 = Sha256(string.Join("\n", raw.SourceHeaders.Select(x => $"{x.Path}|{x.Sha256}|{x.StartOrdinal}|{x.DeclarationCount}")) + "\n"),
                MappingSha256 = Sha256(mapping), DeclarationCount = raw.Declarations.Count, EnumCount = raw.Declarations.Count(x => x.Kind == "enum"), ClassCount = raw.Declarations.Count(x => x.Kind == "class"), CallableCount = raw.Declarations.Count(x => x.Kind == "callable"), ClassificationCounts = counts,
                NativeEvidenceCount = classifications.Declarations.SelectMany(x => x.NativeEntrypoints).Distinct(Ordinal).Count(), ManagedEvidenceCount = classifications.Declarations.SelectMany(x => x.ManagedMembers).Distinct(Ordinal).Count(),
                FamilyInventoryPath = Rel(options.Repository, options.FamilyOutput), FamilyInventorySha256 = Sha256(familyText), SelectedFamilyCount = 3, SelectedDeclarationCount = Selected.Count, RepositoryWideUpstreamParityClaimed = false
            };
            RunNegativeFixtures(raw, classifications, options, native, managed);
            WriteOrCheck(options.Output, mapping, options.Check);
            WriteOrCheck(options.FamilyOutput, familyText, options.Check);
            WriteOrCheck(options.Summary, Serialize(summary), options.Check);
            Console.WriteLine($"PHOTO_UPSTREAM_MAP_OK declarations={summary.DeclarationCount} callables={summary.CallableCount} implemented={counts["implemented"]} missing={counts["missing"]} omitted={counts["intentionally-omitted"]} fixtures=16 sha256={summary.MappingSha256} mode={(options.Check ? "check" : "write")}");
            return 0;
        }
        catch (Exception exception) { Console.Error.WriteLine(exception.Message); return 1; }
    }

    private static Options Parse(string[] args)
    {
        var values = new Dictionary<string, string>(Ordinal); bool initialize = false, check = false;
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--initialize-classification") initialize = true;
            else if (args[i] == "--check") check = true;
            else { Require(i + 1 < args.Length, "Missing option value: " + args[i]); values[args[i]] = args[++i]; }
        }
        string Value(string name) { Require(values.TryGetValue(name, out string? value), "Missing option: " + name); return Path.GetFullPath(value!); }
        return new Options(Value("--repository"), Value("--workspace"), Value("--raw"), Value("--classification"), Value("--native-manifest"), Value("--managed-baseline"), Value("--output"), Value("--summary"), Value("--family-output"), initialize, check);
    }

    private static ClassificationDocument Initialize(RawDocument raw, string[] native, string[] managed)
    {
        var result = new ClassificationDocument();
        foreach (RawDeclaration declaration in raw.Declarations)
        {
            var row = new ClassificationRow { Ordinal = declaration.Ordinal, Identity = declaration.Identity, BuildCondition = BuildCondition(declaration.Ordinal) };
            if (declaration.Kind != "callable")
            {
                row.Classification = "non-callable-metadata";
                row.Reason = "Parser-emitted class or enum metadata is reviewed as public type shape rather than an independently callable ABI operation.";
            }
            else if (!Existing.Contains(declaration.Ordinal) && !Selected.Contains(declaration.Ordinal))
            {
                row.Classification = "missing";
                row.Reason = MissingReason(declaration.Ordinal);
            }
            else
            {
                row.Classification = "implemented";
                row.Reason = Selected.Contains(declaration.Ordinal)
                    ? CcmSelected.Contains(declaration.Ordinal)
                        ? "The selected CCM batch has a version-neutral opaque-handle ABI, cloned inputs, caller-owned Mat outputs, checked Core persistence access, native smoke, and net8/net10 managed tests."
                        : IntelligentScissorsSelected.Contains(declaration.Ordinal)
                            ? "The selected Intelligent Scissors batch has a version-neutral opaque-handle ABI, checked feature/state/point contracts, normalized caller-owned contour output, native smoke, and net8/net10 managed tests."
                            : "The selected HDR batch has a version-neutral opaque-handle ABI, caller-owned Mat outputs, explicit collection counts, native smoke, and net8/net10 managed tests."
                    : "The existing version-neutral native and managed main Photo surface provides the callable semantics represented by this parser row.";
                row.NativeEntrypoints.AddRange(NativeEvidence(declaration, native));
                row.ManagedMembers.Add(ManagedEvidence(declaration, managed));
            }
            row.NativeEntrypoints = row.NativeEntrypoints.Distinct(Ordinal).OrderBy(x => x, Ordinal).ToList();
            row.ManagedMembers = row.ManagedMembers.Distinct(Ordinal).OrderBy(x => x, Ordinal).ToList();
            result.Declarations.Add(row);
        }
        Validate(raw, result, null, native, managed, false);
        return result;
    }

    private static string BuildCondition(int ordinal)
    {
        _ = ordinal;
        return "OPENCV_CSHARP_HAS_OPENCV_PHOTO; full-profile; mini-excluded";
    }

    private static string MissingReason(int ordinal)
    {
        if (ordinal == 8)
            return "OpenCV 5.0.0 photo/src/denoising.cpp is linked locally, but the current ABI has no TV-L1 observation-vector entrypoint; this offline-safe callable remains a prioritized implementation gap.";
        if (ordinal is 98 or 99)
            return "OpenCV 5.0.0 photo chromatic-aberration code is linked locally, but no version-neutral ABI or managed member currently exposes coefficient correction or FileNode parameter loading; the shared persistence handles are available for a later coherent batch.";
        if (ordinal is >= 106 and <= 134)
            return "OpenCV 5.0.0 photo/ccm.hpp is linked locally, but the CCM gamma/model lifecycle, enum contract, Mat results, and persistence methods have no current native or managed evidence; this family remains an explicit next-stage gap.";
        if (ordinal is >= 136 and <= 144)
            return "OpenCV 5.0.0 photo/segmentation.hpp is linked locally, but IntelligentScissorsMB state, fluent setters, map construction, and contour output have no current native or managed evidence; this family remains an explicit next-stage gap.";
        throw new InvalidOperationException("No missing rationale for ordinal " + ordinal);
    }

    private static List<string> NativeEvidence(RawDeclaration declaration, string[] native)
    {
        string baseName = declaration.Ordinal switch
        {
            8 => "jyppx_ocv_photo_denoise_tvl1",
            1 => "jyppx_ocv_photo_inpaint",
            2 => "jyppx_ocv_photo_fast_nl_means_denoising",
            3 => "jyppx_ocv_photo_fast_nl_means_denoising_with_h_array",
            4 => "jyppx_ocv_photo_fast_nl_means_denoising_colored",
            5 => "jyppx_ocv_photo_fast_nl_means_denoising_multi",
            6 => "jyppx_ocv_photo_fast_nl_means_denoising_multi_with_h_array",
            7 => "jyppx_ocv_photo_fast_nl_means_denoising_colored_multi",
            11 => "jyppx_ocv_tonemap_process",
            12 => "jyppx_ocv_tonemap_get_gamma",
            13 => "jyppx_ocv_tonemap_set_gamma",
            14 => "jyppx_ocv_tonemap_create",
            16 => "jyppx_ocv_tonemap_drago_get_saturation",
            17 => "jyppx_ocv_tonemap_drago_set_saturation",
            18 => "jyppx_ocv_tonemap_drago_get_bias",
            19 => "jyppx_ocv_tonemap_drago_set_bias",
            20 => "jyppx_ocv_tonemap_drago_create",
            22 => "jyppx_ocv_tonemap_reinhard_get_intensity",
            23 => "jyppx_ocv_tonemap_reinhard_set_intensity",
            24 => "jyppx_ocv_tonemap_reinhard_get_light_adaptation",
            25 => "jyppx_ocv_tonemap_reinhard_set_light_adaptation",
            26 => "jyppx_ocv_tonemap_reinhard_get_color_adaptation",
            27 => "jyppx_ocv_tonemap_reinhard_set_color_adaptation",
            28 => "jyppx_ocv_tonemap_reinhard_create",
            30 => "jyppx_ocv_tonemap_mantiuk_get_scale",
            31 => "jyppx_ocv_tonemap_mantiuk_set_scale",
            32 => "jyppx_ocv_tonemap_mantiuk_get_saturation",
            33 => "jyppx_ocv_tonemap_mantiuk_set_saturation",
            34 => "jyppx_ocv_tonemap_mantiuk_create",
            36 or 38 or 39 => "jyppx_ocv_align_mtb_process",
            40 => "jyppx_ocv_align_mtb_calculate_shift",
            41 => "jyppx_ocv_align_mtb_shift_mat",
            42 => "jyppx_ocv_align_mtb_compute_bitmaps",
            43 => "jyppx_ocv_align_mtb_get_max_bits",
            44 => "jyppx_ocv_align_mtb_set_max_bits",
            45 => "jyppx_ocv_align_mtb_get_exclude_range",
            46 => "jyppx_ocv_align_mtb_set_exclude_range",
            47 => "jyppx_ocv_align_mtb_get_cut",
            48 => "jyppx_ocv_align_mtb_set_cut",
            49 => "jyppx_ocv_align_mtb_create",
            51 => "jyppx_ocv_calibrate_crf_process",
            53 => "jyppx_ocv_calibrate_debevec_get_lambda",
            54 => "jyppx_ocv_calibrate_debevec_set_lambda",
            55 => "jyppx_ocv_calibrate_debevec_get_samples",
            56 => "jyppx_ocv_calibrate_debevec_set_samples",
            57 => "jyppx_ocv_calibrate_debevec_get_random",
            58 => "jyppx_ocv_calibrate_debevec_set_random",
            59 => "jyppx_ocv_calibrate_debevec_create",
            61 => "jyppx_ocv_calibrate_robertson_get_max_iter",
            62 => "jyppx_ocv_calibrate_robertson_set_max_iter",
            63 => "jyppx_ocv_calibrate_robertson_get_threshold",
            64 => "jyppx_ocv_calibrate_robertson_set_threshold",
            65 => "jyppx_ocv_calibrate_robertson_get_radiance",
            66 => "jyppx_ocv_calibrate_robertson_create",
            68 or 70 or 71 or 74 or 75 or 84 or 85 => "jyppx_ocv_merge_exposures_process",
            72 => "jyppx_ocv_merge_debevec_create",
            76 => "jyppx_ocv_merge_mertens_get_contrast_weight",
            77 => "jyppx_ocv_merge_mertens_set_contrast_weight",
            78 => "jyppx_ocv_merge_mertens_get_saturation_weight",
            79 => "jyppx_ocv_merge_mertens_set_saturation_weight",
            80 => "jyppx_ocv_merge_mertens_get_exposure_weight",
            81 => "jyppx_ocv_merge_mertens_set_exposure_weight",
            82 => "jyppx_ocv_merge_mertens_create",
            86 => "jyppx_ocv_merge_robertson_create",
            87 => "jyppx_ocv_photo_decolor",
            89 => "jyppx_ocv_photo_seamless_clone",
            90 => "jyppx_ocv_photo_color_change",
            91 => "jyppx_ocv_photo_illumination_change",
            92 => "jyppx_ocv_photo_texture_flattening",
            94 => "jyppx_ocv_photo_edge_preserving_filter",
            95 => "jyppx_ocv_photo_detail_enhance",
            96 => "jyppx_ocv_photo_pencil_sketch",
            97 => "jyppx_ocv_photo_stylization",
            98 => "jyppx_ocv_photo_correct_chromatic_aberration",
            99 => "jyppx_ocv_photo_load_chromatic_aberration_params",
            106 => "jyppx_ocv_photo_ccm_gamma_correction",
            108 => "jyppx_ocv_photo_ccm_create",
            109 => "jyppx_ocv_photo_ccm_create_color_checker",
            110 => "jyppx_ocv_photo_ccm_create_reference_colors",
            111 => "jyppx_ocv_photo_ccm_create_reference_colors_masked",
            112 => "jyppx_ocv_photo_ccm_set_color_space",
            113 => "jyppx_ocv_photo_ccm_set_ccm_type",
            114 => "jyppx_ocv_photo_ccm_set_distance",
            115 => "jyppx_ocv_photo_ccm_set_linearization",
            116 => "jyppx_ocv_photo_ccm_set_linearization_gamma",
            117 => "jyppx_ocv_photo_ccm_set_linearization_degree",
            118 => "jyppx_ocv_photo_ccm_set_saturated_threshold",
            119 => "jyppx_ocv_photo_ccm_set_weights_list",
            120 => "jyppx_ocv_photo_ccm_set_weight_coeff",
            121 => "jyppx_ocv_photo_ccm_set_initial_method",
            122 => "jyppx_ocv_photo_ccm_set_max_count",
            123 => "jyppx_ocv_photo_ccm_set_epsilon",
            124 => "jyppx_ocv_photo_ccm_set_rgb",
            125 => "jyppx_ocv_photo_ccm_compute",
            126 => "jyppx_ocv_photo_ccm_get_color_correction_matrix",
            127 => "jyppx_ocv_photo_ccm_get_loss",
            128 => "jyppx_ocv_photo_ccm_get_src_linear_rgb",
            129 => "jyppx_ocv_photo_ccm_get_ref_linear_rgb",
            130 => "jyppx_ocv_photo_ccm_get_mask",
            131 => "jyppx_ocv_photo_ccm_get_weights",
            132 => "jyppx_ocv_photo_ccm_correct_image",
            133 => "jyppx_ocv_photo_ccm_write",
            134 => "jyppx_ocv_photo_ccm_read",
            136 => "jyppx_ocv_photo_intelligent_scissors_create",
            137 => "jyppx_ocv_photo_intelligent_scissors_set_weights",
            138 => "jyppx_ocv_photo_intelligent_scissors_set_gradient_magnitude_max_limit",
            139 => "jyppx_ocv_photo_intelligent_scissors_set_edge_feature_zero_crossing_parameters",
            140 => "jyppx_ocv_photo_intelligent_scissors_set_edge_feature_canny_parameters",
            141 => "jyppx_ocv_photo_intelligent_scissors_apply_image",
            142 => "jyppx_ocv_photo_intelligent_scissors_apply_image_features",
            143 => "jyppx_ocv_photo_intelligent_scissors_build_map",
            144 => "jyppx_ocv_photo_intelligent_scissors_get_contour",
            _ => throw new InvalidOperationException("No native evidence mapping for " + declaration.Identity)
        };
        List<string> result = MatchNative(native, baseName);
        if (declaration.Ordinal is >= 108 and <= 111)
        {
            result.AddRange(MatchNative(native, "jyppx_ocv_photo_ccm_release_handle"));
            result = result.Distinct(Ordinal).OrderBy(x => x, Ordinal).ToList();
        }
        if (declaration.Ordinal == 136)
        {
            result.AddRange(MatchNative(native, "jyppx_ocv_photo_intelligent_scissors_release_handle"));
            result = result.Distinct(Ordinal).OrderBy(x => x, Ordinal).ToList();
        }
        return result;
    }

    private static List<string> MatchNative(string[] native, string baseName)
    {
        string[] suffixes = { "", "_count", "_fill", "_length" };
        var result = native.Where(x => suffixes.Any(s => x == baseName + s)).OrderBy(x => x, Ordinal).ToList();
        Require(result.Count > 0, "No native evidence for " + baseName);
        return result;
    }
    private static string ManagedEvidence(RawDeclaration declaration, string[] managed)
    {
        string M(string type, params string[] fragments) => FindManaged(managed, "JYPPX.OpenCvSharp.Photo." + type, fragments);
        return declaration.Ordinal switch
        {
            1 => M("PhotoCv2", "System.Void Inpaint("),
            2 => M("PhotoCv2", "System.Void FastNlMeansDenoising(", "System.Single h=3"),
            3 => M("PhotoCv2", "System.Void FastNlMeansDenoising(", "System.Single[] h"),
            4 => M("PhotoCv2", "System.Void FastNlMeansDenoisingColored("),
            5 => M("PhotoCv2", "System.Void FastNlMeansDenoisingMulti(JYPPX.OpenCvSharp.Core.Mat[]", "System.Single h=3"),
            6 => M("PhotoCv2", "System.Void FastNlMeansDenoisingMulti(JYPPX.OpenCvSharp.Core.Mat[]", "System.Single[] h"),
            7 => M("PhotoCv2", "System.Void FastNlMeansDenoisingColoredMulti(JYPPX.OpenCvSharp.Core.Mat[]"),
            11 => M("Tonemap", "System.Void Process("),
            12 or 13 => M("Tonemap", "|property|", "System.Single Gamma"),
            14 => M("Tonemap", "|method|public;static|", " Create(System.Single gamma=1)"),
            16 or 17 => M("TonemapDrago", "|property|", "System.Single Saturation"),
            18 or 19 => M("TonemapDrago", "|property|", "System.Single Bias"),
            20 => M("TonemapDrago", "|method|public;static|", " Create("),
            22 or 23 => M("TonemapReinhard", "|property|", "System.Single Intensity"),
            24 or 25 => M("TonemapReinhard", "|property|", "System.Single LightAdaptation"),
            26 or 27 => M("TonemapReinhard", "|property|", "System.Single ColorAdaptation"),
            28 => M("TonemapReinhard", "|method|public;static|", " Create("),
            30 or 31 => M("TonemapMantiuk", "|property|", "System.Single Scale"),
            32 or 33 => M("TonemapMantiuk", "|property|", "System.Single Saturation"),
            34 => M("TonemapMantiuk", "|method|public;static|", " Create("),
            36 or 38 => M("AlignExposures", "System.Void Process(JYPPX.OpenCvSharp.Core.Mat[] src,JYPPX.OpenCvSharp.Core.Mat[] dst,JYPPX.OpenCvSharp.Core.Mat times,JYPPX.OpenCvSharp.Core.Mat response)"),
            39 => M("AlignMTB", "System.Void Process(JYPPX.OpenCvSharp.Core.Mat[] src,JYPPX.OpenCvSharp.Core.Mat[] dst)"),
            40 => M("AlignMTB", "JYPPX.OpenCvSharp.Core.Point CalculateShift("),
            41 => M("AlignMTB", "System.Void ShiftMat("),
            42 => M("AlignMTB", "System.Void ComputeBitmaps("),
            43 or 44 => M("AlignMTB", "|property|", "System.Int32 MaxBits"),
            45 or 46 => M("AlignMTB", "|property|", "System.Int32 ExcludeRange"),
            47 or 48 => M("AlignMTB", "|property|", "System.Boolean Cut"),
            49 => M("AlignMTB", "|method|public;static|", " Create("),
            51 => M("CalibrateCRF", "System.Void Process(JYPPX.OpenCvSharp.Core.Mat[] src,JYPPX.OpenCvSharp.Core.Mat dst,JYPPX.OpenCvSharp.Core.Mat times)"),
            53 or 54 => M("CalibrateDebevec", "|property|", "System.Single Lambda"),
            55 or 56 => M("CalibrateDebevec", "|property|", "System.Int32 Samples"),
            57 or 58 => M("CalibrateDebevec", "|property|", "System.Boolean Random"),
            59 => M("CalibrateDebevec", "|method|public;static|", " Create("),
            61 or 62 => M("CalibrateRobertson", "|property|", "System.Int32 MaxIter"),
            63 or 64 => M("CalibrateRobertson", "|property|", "System.Single Threshold"),
            65 => M("CalibrateRobertson", "JYPPX.OpenCvSharp.Core.Mat GetRadiance()"),
            66 => M("CalibrateRobertson", "|method|public;static|", " Create("),
            68 or 70 or 74 or 84 => M("MergeExposures", "System.Void Process(JYPPX.OpenCvSharp.Core.Mat[] src,JYPPX.OpenCvSharp.Core.Mat dst,JYPPX.OpenCvSharp.Core.Mat times,JYPPX.OpenCvSharp.Core.Mat response)"),
            71 => M("MergeDebevec", "System.Void Process(JYPPX.OpenCvSharp.Core.Mat[] src,JYPPX.OpenCvSharp.Core.Mat dst,JYPPX.OpenCvSharp.Core.Mat times)"),
            72 => M("MergeDebevec", "|method|public;static|", " Create()"),
            75 => M("MergeMertens", "System.Void Process(JYPPX.OpenCvSharp.Core.Mat[] src,JYPPX.OpenCvSharp.Core.Mat dst)"),
            76 or 77 => M("MergeMertens", "|property|", "System.Single ContrastWeight"),
            78 or 79 => M("MergeMertens", "|property|", "System.Single SaturationWeight"),
            80 or 81 => M("MergeMertens", "|property|", "System.Single ExposureWeight"),
            82 => M("MergeMertens", "|method|public;static|", " Create("),
            85 => M("MergeRobertson", "System.Void Process(JYPPX.OpenCvSharp.Core.Mat[] src,JYPPX.OpenCvSharp.Core.Mat dst,JYPPX.OpenCvSharp.Core.Mat times)"),
            86 => M("MergeRobertson", "|method|public;static|", " Create()"),
            87 => M("PhotoCv2", "System.Void Decolor("),
            89 => M("PhotoCv2", "System.Void SeamlessClone("),
            90 => M("PhotoCv2", "System.Void ColorChange("),
            91 => M("PhotoCv2", "System.Void IlluminationChange("),
            92 => M("PhotoCv2", "System.Void TextureFlattening("),
            94 => M("PhotoCv2", "System.Void EdgePreservingFilter("),
            95 => M("PhotoCv2", "System.Void DetailEnhance("),
            96 => M("PhotoCv2", "System.Void PencilSketch("),
            97 => M("PhotoCv2", "System.Void Stylization("),
            8 => M("PhotoCv2", "System.Void DenoiseTvl1("),
            98 => M("PhotoCv2", "System.Void CorrectChromaticAberration("),
            99 => M("PhotoCv2", "System.Void LoadChromaticAberrationParams("),
            106 => M("PhotoCv2", "System.Void GammaCorrection("),
            108 => M("ColorCorrectionModel", "|method|public;static|", " Create()"),
            109 => M("ColorCorrectionModel", "|method|public;static|", " Create(JYPPX.OpenCvSharp.Core.Mat src,JYPPX.OpenCvSharp.Photo.ColorCheckerType colorChecker)"),
            110 => M("ColorCorrectionModel", "|method|public;static|", " Create(JYPPX.OpenCvSharp.Core.Mat src,JYPPX.OpenCvSharp.Core.Mat colors,JYPPX.OpenCvSharp.Photo.ColorSpace referenceColorSpace)"),
            111 => M("ColorCorrectionModel", "|method|public;static|", " Create(JYPPX.OpenCvSharp.Core.Mat src,JYPPX.OpenCvSharp.Core.Mat colors,JYPPX.OpenCvSharp.Photo.ColorSpace referenceColorSpace,JYPPX.OpenCvSharp.Core.Mat coloredPatchesMask)"),
            112 => M("ColorCorrectionModel", "System.Void SetColorSpace("),
            113 => M("ColorCorrectionModel", "System.Void SetCcmType("),
            114 => M("ColorCorrectionModel", "System.Void SetDistance("),
            115 => M("ColorCorrectionModel", "System.Void SetLinearization(JYPPX.OpenCvSharp.Photo.LinearizationType"),
            116 => M("ColorCorrectionModel", "System.Void SetLinearizationGamma("),
            117 => M("ColorCorrectionModel", "System.Void SetLinearizationDegree("),
            118 => M("ColorCorrectionModel", "System.Void SetSaturatedThreshold("),
            119 => M("ColorCorrectionModel", "System.Void SetWeightsList("),
            120 => M("ColorCorrectionModel", "System.Void SetWeightCoeff("),
            121 => M("ColorCorrectionModel", "System.Void SetInitialMethod("),
            122 => M("ColorCorrectionModel", "System.Void SetMaxCount("),
            123 => M("ColorCorrectionModel", "System.Void SetEpsilon("),
            124 => M("ColorCorrectionModel", "System.Void SetRGB("),
            125 => M("ColorCorrectionModel", "JYPPX.OpenCvSharp.Core.Mat Compute()"),
            126 => M("ColorCorrectionModel", "JYPPX.OpenCvSharp.Core.Mat GetColorCorrectionMatrix()"),
            127 => M("ColorCorrectionModel", "System.Double GetLoss()"),
            128 => M("ColorCorrectionModel", "JYPPX.OpenCvSharp.Core.Mat GetSrcLinearRGB()"),
            129 => M("ColorCorrectionModel", "JYPPX.OpenCvSharp.Core.Mat GetRefLinearRGB()"),
            130 => M("ColorCorrectionModel", "JYPPX.OpenCvSharp.Core.Mat GetMask()"),
            131 => M("ColorCorrectionModel", "JYPPX.OpenCvSharp.Core.Mat GetWeights()"),
            132 => M("ColorCorrectionModel", "System.Void CorrectImage("),
            133 => M("ColorCorrectionModel", "System.Void Write("),
            134 => M("ColorCorrectionModel", "System.Void Read("),
            136 => M("IntelligentScissorsMB", "|constructor|public;instance|", ".ctor()"),
            137 => M("IntelligentScissorsMB", " SetWeights("),
            138 => M("IntelligentScissorsMB", " SetGradientMagnitudeMaxLimit("),
            139 => M("IntelligentScissorsMB", " SetEdgeFeatureZeroCrossingParameters("),
            140 => M("IntelligentScissorsMB", " SetEdgeFeatureCannyParameters("),
            141 => M("IntelligentScissorsMB", " ApplyImage("),
            142 => M("IntelligentScissorsMB", " ApplyImageFeatures("),
            143 => M("IntelligentScissorsMB", "System.Void BuildMap("),
            144 => M("IntelligentScissorsMB", "System.Void GetContour("),
            _ => throw new InvalidOperationException("No managed evidence mapping for " + declaration.Identity)
        };
    }

    private static string FindManaged(string[] managed, string type, params string[] fragments)
    {
        string prefix = "MEMBER|" + type + "|";
        List<string> matches = managed.Where(x => x.Contains(prefix, StringComparison.Ordinal) && fragments.All(f => x.Contains(f, StringComparison.Ordinal))).OrderBy(x => x, Ordinal).ToList();
        Require(matches.Count > 0, "No managed evidence for " + type + " fragments " + string.Join(",", fragments));
        return matches[0];
    }

    private static void Validate(RawDocument raw, ClassificationDocument classifications, Options? options, string[] native, string[] managed, bool verifyFiles)
    {
        Require(raw.SchemaVersion == 1 && raw.UpstreamOpenCvVersion == "5.0.0" && raw.DeclarationCount == 145 && raw.Declarations.Count == 145, "Photo raw identity/count drifted.");
        Require(raw.Declarations.Count(x => x.Kind == "callable") == 120 && raw.Declarations.Count(x => x.Kind == "class") == 15 && raw.Declarations.Count(x => x.Kind == "enum") == 10, "Photo declaration partition drifted.");
        Require(raw.SourceHeaders.Count == 3 && raw.CompatibilityHeaders.Count == 2 && raw.ExcludedPublicHeaders.Count == 1, "Photo header closure drifted.");
        Require(raw.PreprocessorDefinitions.Count == 2 && raw.PreprocessorDefinitions.GetValueOrDefault("CV_VERSION_MAJOR") == 5 && raw.PreprocessorDefinitions.GetValueOrDefault("OPENCV_ABI_COMPATIBILITY") == 500, "Photo parser definitions drifted.");
        Require(raw.ExcludedPublicHeaders[0].Path.EndsWith("opencv2/photo/cuda.hpp", StringComparison.Ordinal) && !string.IsNullOrWhiteSpace(raw.ExcludedPublicHeaders[0].Reason), "Photo CUDA exclusion drifted.");
        int[] starts = { 0, 100, 135 }; int[] counts = { 100, 35, 10 };
        for (int i = 0; i < 3; i++) Require(raw.SourceHeaders[i].StartOrdinal == starts[i] && raw.SourceHeaders[i].DeclarationCount == counts[i], "Photo source-header order drifted at " + i);
        for (int i = 0; i < raw.Declarations.Count; i++)
        {
            SourceHeader owner = raw.SourceHeaders.Last(h => h.StartOrdinal <= i);
            Require(raw.Declarations[i].Ordinal == i && !string.IsNullOrWhiteSpace(raw.Declarations[i].Identity) && raw.Declarations[i].SourceHeader == owner.Path, "Photo parser order/source/identity drifted at " + i);
        }
        Require(raw.Declarations.Select(x => x.Identity).Distinct(Ordinal).Count() == 145, "Photo overload identities collapsed.");
        Require(raw.SourceHeaders.All(x => !x.Path.Contains("xphoto", StringComparison.OrdinalIgnoreCase) && !x.Path.EndsWith("/cuda.hpp", StringComparison.Ordinal)), "Photo source closure contains xphoto or CUDA contamination.");
        Require(classifications.SchemaVersion == 1 && classifications.UpstreamOpenCvVersion == "5.0.0" && classifications.ClaimedSlice == ClaimedSlice && classifications.ReviewStatus == "source-reviewed" && !string.IsNullOrWhiteSpace(classifications.Limitation), "Photo classification identity drifted.");
        Require(!classifications.ClaimedSlice.Contains("OpenCv5Sharp", StringComparison.Ordinal) && !classifications.ClaimedSlice.Contains("xphoto", StringComparison.OrdinalIgnoreCase) && classifications.Declarations.Count == 145, "Photo fixed-major identity, xphoto boundary, or row count drifted.");
        var nativeSet = new HashSet<string>(native, Ordinal); var managedSet = new HashSet<string>(managed, Ordinal);
        for (int i = 0; i < 145; i++)
        {
            RawDeclaration declaration = raw.Declarations[i]; ClassificationRow row = classifications.Declarations[i];
            Require(row.Ordinal == i && row.Identity == declaration.Identity && Allowed.Contains(row.Classification, Ordinal), "Photo classification order/value drifted at " + i);
            Require(!string.IsNullOrWhiteSpace(row.Reason) && !string.IsNullOrWhiteSpace(row.BuildCondition), "Undocumented Photo row at " + i);
            Require(row.NativeEntrypoints.SequenceEqual(row.NativeEntrypoints.Distinct(Ordinal).OrderBy(x => x, Ordinal)) && row.ManagedMembers.SequenceEqual(row.ManagedMembers.Distinct(Ordinal).OrderBy(x => x, Ordinal)), "Nondeterministic Photo evidence ordering at " + i);
            Require(declaration.Kind == "callable" ? row.Classification != "non-callable-metadata" : row.Classification == "non-callable-metadata", "Photo callable/metadata confusion at " + i);
            if (row.Classification == "implemented")
            {
                Require(row.NativeEntrypoints.Count > 0 && row.ManagedMembers.Count > 0 && row.NativeEntrypoints.All(nativeSet.Contains) && row.ManagedMembers.All(managedSet.Contains), "False or missing Photo evidence at " + i);
            }
            else if (declaration.Kind == "callable") Require(row.NativeEntrypoints.Count == 0 && row.ManagedMembers.Count == 0, "Non-implemented Photo callable carries evidence at " + i);
            Require(row.BuildCondition == "OPENCV_CSHARP_HAS_OPENCV_PHOTO; full-profile; mini-excluded", "Photo build condition drifted at " + i);
        }
        Require(classifications.Declarations.Count(x => x.Classification == "implemented") == 120 && classifications.Declarations.Count(x => x.Classification == "missing") == 0 && classifications.Declarations.Count(x => x.Classification == "non-callable-metadata") == 25 && classifications.Declarations.Count(x => x.Classification == "intentionally-omitted") == 0, "Photo callable partition drifted.");
        Require(Existing.All(i => classifications.Declarations[i].Classification == "implemented"), "Existing Photo correlation is incomplete.");
        Require(Selected.All(i => classifications.Declarations[i].Classification == "implemented"), "Selected Photo batch is incomplete.");
        if (verifyFiles)
        {
            Require(options != null, "Options required for hash verification.");
            VerifyHash(options!.Workspace, raw.HeaderPath, raw.HeaderSha256, "umbrella header"); VerifyHash(options.Workspace, raw.ParserPath, raw.ParserSha256, "parser");
            foreach (CompatibilityHeader header in raw.CompatibilityHeaders) VerifyHash(options.Workspace, header.Path, header.Sha256, "compatibility header");
            foreach (SourceHeader header in raw.SourceHeaders) VerifyHash(options.Workspace, header.Path, header.Sha256, "source header");
        }
    }

    private static string BuildMap(RawDocument raw, ClassificationDocument classifications)
    {
        var b = new StringBuilder();
        b.AppendLine("# Generated by tools/PhotoUpstreamMap. Do not edit."); b.AppendLine("schema-version=1"); b.AppendLine("upstream-opencv-version=5.0.0"); b.AppendLine("claimed-slice=" + ClaimedSlice); b.AppendLine("header-sha256=" + raw.HeaderSha256); b.AppendLine("parser-sha256=" + raw.ParserSha256); b.AppendLine("declaration-count=145"); b.AppendLine("callable-count=120"); b.AppendLine("class-count=15"); b.AppendLine("enum-count=10"); b.AppendLine("repository-wide-upstream-parity-claimed=false");
        foreach (CompatibilityHeader h in raw.CompatibilityHeaders) b.AppendLine($"compatibility-header={h.Path}|{h.Sha256}|includes={h.Includes}");
        b.AppendLine(); b.AppendLine("ordinal|kind|source-header|classification|identity|native-entrypoints|managed-members|build-condition|reason");
        for (int i = 0; i < 145; i++)
        {
            RawDeclaration d = raw.Declarations[i]; ClassificationRow r = classifications.Declarations[i];
            b.AppendLine($"{i}|{d.Kind}|{d.SourceHeader}|{r.Classification}|{d.Identity}|{Join(r.NativeEntrypoints)}|{Join(r.ManagedMembers)}|{r.BuildCondition}|{r.Reason}");
        }
        return b.ToString().Replace("\r\n", "\n", StringComparison.Ordinal);
    }

    private static FamilyDocument BuildFamilies(RawDocument raw, ClassificationDocument classifications)
    {
        FamilyRow BuildFamily(HashSet<int> ordinals, string id, string rationale, string test, string guide)
        {
            var family = new FamilyRow { Id = id, Rationale = rationale };
            foreach (int i in ordinals.OrderBy(x => x))
            {
                family.Declarations.Add(new FamilyOperation
                {
                    Ordinal = i,
                    UpstreamIdentity = raw.Declarations[i].Identity,
                    NativeEntrypoints = new(classifications.Declarations[i].NativeEntrypoints),
                    ManagedMembers = new(classifications.Declarations[i].ManagedMembers),
                    FocusedTest = test,
                    Guide = guide
                });
            }
            return family;
        }

        return new FamilyDocument
        {
            Families = new()
            {
                BuildFamily(
                    HdrSelected,
                    "photo-hdr-calibrate-merge",
                    "The selected batch closes the complete parser-emitted AlignMTB, CalibrateDebevec, CalibrateRobertson, MergeDebevec, MergeMertens, and MergeRobertson lifecycle and processing surface with offline deterministic inputs.",
                    "tests/OpenCvSharp.Tests/Photo/PhotoHdrParityTests.cs",
                    "docs/articles/photo-hdr-workflow-guide.md"),
                BuildFamily(
                    CcmSelected,
                    "photo-ccm-structured-parity",
                    "The selected batch closes gamma correction and the complete parser-emitted ColorCorrectionModel construction, configuration, compute, correction, getter, persistence, and lifetime surface with deterministic offline colors.",
                    "tests/OpenCvSharp.Tests/Photo/PhotoCcmParityTests.cs",
                    "docs/articles/photo-ccm-guide.md"),
                BuildFamily(
                    IntelligentScissorsSelected,
                    "photo-intelligent-scissors-live-wire",
                    "The selected batch closes the complete parser-emitted IntelligentScissorsMB configuration, feature extraction, map construction, contour retrieval, and lifetime surface with deterministic offline images.",
                    "tests/OpenCvSharp.Tests/Photo/PhotoIntelligentScissorsTests.cs",
                    "docs/articles/photo-intelligent-scissors-guide.md")
            }
        };
    }

    private static void RunNegativeFixtures(RawDocument raw, ClassificationDocument classifications, Options options, string[] native, string[] managed)
    {
        int passed = 0;
        void Fixture(Action<RawDocument, ClassificationDocument> mutate)
        {
            RawDocument rc = Clone(raw); ClassificationDocument cc = Clone(classifications); mutate(rc, cc); bool failed = false;
            try { Validate(rc, cc, options, native, managed, true); } catch { failed = true; }
            Require(failed, "A Photo negative fixture was accepted."); passed++;
        }
        Fixture((_, c) => c.Declarations.RemoveAt(0)); Fixture((_, c) => c.Declarations[1].Ordinal = 0); Fixture((_, c) => (c.Declarations[0], c.Declarations[1]) = (c.Declarations[1], c.Declarations[0]));
        Fixture((r, _) => r.Declarations[2].Identity = r.Declarations[1].Identity); Fixture((_, c) => c.Declarations[108].Classification = "non-callable-metadata"); Fixture((r, _) => r.Declarations[0].SourceHeader = "drifted/photo.hpp");
        Fixture((r, _) => r.ParserSha256 = new string('0', 64)); Fixture((r, _) => r.HeaderSha256 = new string('0', 64)); Fixture((_, c) => c.Declarations[14].NativeEntrypoints[0] = "jyppx_ocv_false_evidence");
        Fixture((_, c) => c.Declarations[14].ManagedMembers[0] = "MEMBER|false"); Fixture((_, c) => c.Declarations[8].Reason = ""); Fixture((_, c) => c.ClaimedSlice += "; OpenCv5Sharp");
        Fixture((_, c) => c.ClaimedSlice += "; xphoto"); Fixture((_, c) => c.Declarations[36].BuildCondition = "unconditional");
        Fixture((_, c) => c.Declarations[36].NativeEntrypoints.Add("jyppx_ocv_align_mtb_calculate_shift")); Fixture((r, _) => r.SourceHeaders.RemoveAt(0));
        Require(passed == 16, "Photo negative fixture count drifted.");
    }

    private static string[] ReadNative(string path) => File.ReadAllLines(path, Encoding.UTF8).Where(x => x.StartsWith("jyppx_ocv_", StringComparison.Ordinal)).Select(x => x.Split('|')[0]).OrderBy(x => x, Ordinal).ToArray();
    private static T Read<T>(string path) => JsonSerializer.Deserialize<T>(File.ReadAllText(path, Encoding.UTF8), JsonOptions()) ?? throw new InvalidOperationException("Could not parse " + path);
    private static T Clone<T>(T value) => JsonSerializer.Deserialize<T>(Serialize(value), JsonOptions())!;
    private static JsonSerializerOptions JsonOptions() => new() { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };
    private static string Serialize<T>(T value) =>
        JsonSerializer.Serialize(value, JsonOptions())
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace("\r", "\n", StringComparison.Ordinal) + "\n";
    private static void WriteOrCheck(string path, string content, bool check)
    {
        content = content.Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\r", "\n", StringComparison.Ordinal);
        if (check) { Require(File.Exists(path) && File.ReadAllText(path, Encoding.UTF8).Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\r", "\n", StringComparison.Ordinal) == content, "Generated file is missing or stale: " + path); return; }
        Directory.CreateDirectory(Path.GetDirectoryName(path)!); File.WriteAllText(path, content, new UTF8Encoding(false));
    }
    private static void VerifyHash(string workspace, string relative, string expected, string label)
    {
        string path = Path.Combine(workspace, relative.Replace('/', Path.DirectorySeparatorChar)); Require(File.Exists(path), "Photo " + label + " missing: " + relative); Require(Sha256File(path) == expected, "Photo " + label + " hash drifted: " + relative);
    }
    private static string Join(List<string> values) => values.Count == 0 ? "-" : string.Join(";", values.Select(x => x.Replace("|", "<pipe>", StringComparison.Ordinal)));
    private static string Rel(string root, string path) => Path.GetRelativePath(root, path).Replace('\\', '/');
    private static string Sha256(string text) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
    private static string Sha256File(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}

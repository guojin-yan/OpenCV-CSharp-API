using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

internal static class Program
{
    private static readonly StringComparer Ordinal = StringComparer.Ordinal;
    private static readonly HashSet<int> Existing = new(
        new[] { 1, 2, 3, 4, 5, 16, 17, 18, 19, 20, 121, 122, 123 }
            .Concat(Enumerable.Range(125, 27))
            .Concat(Enumerable.Range(153, 15)));
    private static readonly HashSet<int> FarnebackSelected = new(Enumerable.Range(27, 17));
    private static readonly HashSet<int> VariationalSelected = new(Enumerable.Range(45, 16));
    private static readonly HashSet<int> DisSelected = new(Enumerable.Range(63, 25));
    private static readonly HashSet<int> SparsePyrLkSelected = new(Enumerable.Range(89, 11));
    private static readonly HashSet<int> EccSelected = new() { 7, 8, 9, 10, 12, 13 };
    private static readonly HashSet<int> TrackerMilSelected = new() { 101, 102, 103, 106, 107 };
    private static readonly HashSet<int> Selected = new(
        new[] { 22, 23, 25 }
            .Concat(FarnebackSelected)
            .Concat(VariationalSelected)
            .Concat(DisSelected)
            .Concat(SparsePyrLkSelected)
            .Concat(EccSelected)
            .Concat(TrackerMilSelected));
    private static readonly HashSet<int> Omitted = new() { 15, 110, 111, 114, 115, 118, 119 };
    private static readonly string[] Allowed =
    {
        "implemented", "missing", "intentionally-omitted", "upstream-conditional", "unsupported", "non-callable-metadata"
    };
    private const string ClaimedSlice = "opencv2/video.hpp compatibility include measured through the two parser-emitted OpenCV 5.0.0 main Video public source headers";
    private const int ManagedTypeAdditions = 13;
    private const int ManagedMemberAdditions = 110;
    private const int NativeEntrypointAdditions = 45;

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
        public string Limitation { get; set; } = "The map covers the exact OpenCV 5.0.0 main Video compatibility include closure. VideoIO, contrib tracking, private detail headers, legacy C metadata, and repository-wide parity are excluded.";
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
        public string Generator { get; init; } = "tools/VideoUpstreamMap";
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
        public int NegativeFixtureCount { get; init; } = 17;
        public string FamilyInventoryPath { get; init; } = "";
        public string FamilyInventorySha256 { get; init; } = "";
        public int SelectedFamilyCount { get; init; }
        public int SelectedDeclarationCount { get; init; }
        public int ManagedPublicTypeAdditionCount { get; init; } = ManagedTypeAdditions;
        public int ManagedPublicMemberAdditionCount { get; init; } = ManagedMemberAdditions;
        public int NativeEntrypointAdditionCount { get; init; } = NativeEntrypointAdditions;
        public bool RepositoryWideUpstreamParityClaimed { get; init; }
    }
    private sealed class FamilyDocument
    {
        public int SchemaVersion { get; init; } = 1;
        public string UpstreamOpenCvVersion { get; init; } = "5.0.0";
        public string Status { get; init; } = "implemented-verified";
        public int ManagedPublicTypeAdditionCount { get; init; } = ManagedTypeAdditions;
        public int ManagedPublicMemberAdditionCount { get; init; } = ManagedMemberAdditions;
        public int NativeEntrypointAdditionCount { get; init; } = NativeEntrypointAdditions;
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
        public string FocusedTest { get; init; } = "tests/OpenCvSharp.Tests/Video/VideoOpticalFlowObjectTests.cs";
        public string NativeSmoke { get; init; } = "src/OpenCvSharp.Native/tests/native_smoke.cpp";
        public string Sample { get; init; } = "samples/ConsoleSamples/Program.cs";
        public string Guide { get; init; } = "docs/articles/video-upstream-parity-guide.md";
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
            Console.WriteLine($"VIDEO_UPSTREAM_MAP_OK declarations={summary.DeclarationCount} callables={summary.CallableCount} implemented={counts["implemented"]} missing={counts["missing"]} omitted={counts["intentionally-omitted"]} fixtures=17 sha256={summary.MappingSha256} mode={(options.Check ? "check" : "write")}");
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
            var row = new ClassificationRow { Ordinal = declaration.Ordinal, Identity = declaration.Identity, BuildCondition = "OPENCV_CSHARP_HAS_OPENCV_VIDEO; full-profile; mini-excluded" };
            if (declaration.Kind != "callable")
            {
                row.Classification = "non-callable-metadata";
                row.Reason = "Parser-emitted class, struct, or enum metadata is reviewed as public type shape rather than an independently callable ABI operation.";
            }
            else if (Omitted.Contains(declaration.Ordinal))
            {
                row.Classification = "intentionally-omitted";
                row.Reason = OmittedReason(declaration.Ordinal);
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
                    ? SelectedReason(declaration.Ordinal)
                    : "The existing version-neutral native and managed main Video surface provides the callable semantics represented by this parser row.";
                row.NativeEntrypoints.AddRange(NativeEvidence(declaration.Ordinal, native));
                row.ManagedMembers.Add(ManagedEvidence(declaration.Ordinal, managed));
            }
            row.NativeEntrypoints = row.NativeEntrypoints.Distinct(Ordinal).OrderBy(x => x, Ordinal).ToList();
            row.ManagedMembers = row.ManagedMembers.Distinct(Ordinal).OrderBy(x => x, Ordinal).ToList();
            result.Declarations.Add(row);
        }
        Validate(raw, result, null, native, managed, false);
        return result;
    }

    private static string MissingReason(int ordinal)
    {
        if (ordinal is >= 7 and <= 13)
            return "ECC registration and its multiscale parameter/result contract are linked locally but remain a separate coherent matrix, mask, iteration-schedule, and optional-output batch without current native or managed evidence.";
        if (ordinal is >= 101 and <= 107)
            return "The main Video Tracker base and model-free TrackerMIL parameter/factory lifecycle are distinct from contrib tracking and remain an explicit future opaque-handle batch.";
        throw new InvalidOperationException("No missing rationale for ordinal " + ordinal);
    }

    private static string SelectedReason(int ordinal)
    {
        if (EccSelected.Contains(ordinal))
            return "The selected ECC batch provides validated image, mask, warp, criteria, multiscale schedule, and independently owned result semantics with native smoke and net8/net10 managed tests.";
        if (TrackerMilSelected.Contains(ordinal))
            return "The selected main Video TrackerMIL batch provides copied parameters, opaque owned tracker state, guarded initialization/update semantics, and deterministic disposal with native smoke and net8/net10 managed tests.";
        return "The selected main Video optical-flow object batch has opaque SafeHandle ownership, caller-owned Mat or point-array outputs, typed properties, native smoke, and net8/net10 managed tests.";
    }

    private static string OmittedReason(int ordinal)
    {
        if (ordinal == 15)
            return "The default KalmanFilter constructor creates an uninitialized object with no useful managed operation; the dimensioned constructor and explicit Init lifecycle are implemented instead.";
        return "This tracker constructor or factory requires external DNN model files that are not present as deterministic repository-local test assets; it remains outside the supported offline main Video slice.";
    }

    private static List<string> NativeEvidence(int ordinal, string[] native)
    {
        string[] Names(params string[] values) => values;
        string[] names = ordinal switch
        {
            1 => Names("jyppx_ocv_video_cam_shift"),
            2 => Names("jyppx_ocv_video_mean_shift"),
            3 => Names("jyppx_ocv_video_build_optical_flow_pyramid_count", "jyppx_ocv_video_build_optical_flow_pyramid_fill"),
            4 => Names("jyppx_ocv_video_calc_optical_flow_pyr_lk"),
            5 => Names("jyppx_ocv_video_calc_optical_flow_farneback"),
            7 => Names("jyppx_ocv_video_compute_ecc"),
            8 or 9 => Names("jyppx_ocv_video_find_transform_ecc"),
            10 => Names("jyppx_ocv_video_find_transform_ecc_with_mask"),
            12 => Names("jyppx_ocv_video_ecc_parameters_get_default"),
            13 => Names("jyppx_ocv_video_find_transform_ecc_multi_scale"),
            16 => Names("jyppx_ocv_kalman_filter_create", "jyppx_ocv_kalman_filter_release_handle"),
            17 => Names("jyppx_ocv_kalman_filter_predict"),
            18 => Names("jyppx_ocv_kalman_filter_correct"),
            19 => Names("jyppx_ocv_video_read_optical_flow"),
            20 => Names("jyppx_ocv_video_write_optical_flow"),
            22 => Names("jyppx_ocv_dense_optical_flow_calc"),
            23 => Names("jyppx_ocv_dense_optical_flow_collect_garbage"),
            25 => Names("jyppx_ocv_sparse_optical_flow_calc"),
            27 or 33 or 35 or 37 or 41 => Names("jyppx_ocv_farneback_optical_flow_get_int_property"),
            28 or 34 or 36 or 38 or 42 => Names("jyppx_ocv_farneback_optical_flow_set_int_property"),
            29 or 39 => Names("jyppx_ocv_farneback_optical_flow_get_double_property"),
            30 or 40 => Names("jyppx_ocv_farneback_optical_flow_set_double_property"),
            31 => Names("jyppx_ocv_farneback_optical_flow_get_bool_property"),
            32 => Names("jyppx_ocv_farneback_optical_flow_set_bool_property"),
            43 => Names("jyppx_ocv_farneback_optical_flow_create", "jyppx_ocv_dense_optical_flow_release_handle"),
            45 => Names("jyppx_ocv_variational_refinement_calc_uv"),
            46 or 48 => Names("jyppx_ocv_variational_refinement_get_int_property"),
            47 or 49 => Names("jyppx_ocv_variational_refinement_set_int_property"),
            >= 50 and <= 59 when ordinal % 2 == 0 => Names("jyppx_ocv_variational_refinement_get_float_property"),
            >= 50 and <= 59 => Names("jyppx_ocv_variational_refinement_set_float_property"),
            60 => Names("jyppx_ocv_variational_refinement_create", "jyppx_ocv_dense_optical_flow_release_handle"),
            63 or 65 or 67 or 69 or 71 or 73 => Names("jyppx_ocv_dis_optical_flow_get_int_property"),
            64 or 66 or 68 or 70 or 72 or 74 => Names("jyppx_ocv_dis_optical_flow_set_int_property"),
            75 or 77 or 79 or 81 => Names("jyppx_ocv_dis_optical_flow_get_float_property"),
            76 or 78 or 80 or 82 => Names("jyppx_ocv_dis_optical_flow_set_float_property"),
            83 or 85 => Names("jyppx_ocv_dis_optical_flow_get_bool_property"),
            84 or 86 => Names("jyppx_ocv_dis_optical_flow_set_bool_property"),
            87 => Names("jyppx_ocv_dis_optical_flow_create", "jyppx_ocv_dense_optical_flow_release_handle"),
            89 => Names("jyppx_ocv_sparse_pyr_lk_optical_flow_get_size_property"),
            90 => Names("jyppx_ocv_sparse_pyr_lk_optical_flow_set_size_property"),
            91 or 95 => Names("jyppx_ocv_sparse_pyr_lk_optical_flow_get_int_property"),
            92 or 96 => Names("jyppx_ocv_sparse_pyr_lk_optical_flow_set_int_property"),
            93 => Names("jyppx_ocv_sparse_pyr_lk_optical_flow_get_term_criteria"),
            94 => Names("jyppx_ocv_sparse_pyr_lk_optical_flow_set_term_criteria"),
            97 => Names("jyppx_ocv_sparse_pyr_lk_optical_flow_get_min_eig_threshold"),
            98 => Names("jyppx_ocv_sparse_pyr_lk_optical_flow_set_min_eig_threshold"),
            99 => Names("jyppx_ocv_sparse_pyr_lk_optical_flow_create", "jyppx_ocv_sparse_optical_flow_release_handle"),
            101 => Names("jyppx_ocv_video_tracker_init"),
            102 => Names("jyppx_ocv_video_tracker_update"),
            103 => Names("jyppx_ocv_video_tracker_get_tracking_score"),
            106 => Names("jyppx_ocv_video_tracker_mil_get_default_params"),
            107 => Names("jyppx_ocv_video_tracker_mil_create", "jyppx_ocv_video_tracker_release_handle"),
            121 or 149 => Names("jyppx_ocv_background_subtractor_apply"),
            122 or 150 => Names("jyppx_ocv_background_subtractor_apply_with_known_foreground"),
            123 => Names("jyppx_ocv_background_subtractor_get_background_image"),
            125 => Names("jyppx_ocv_background_subtractor_mog2_get_history"),
            126 => Names("jyppx_ocv_background_subtractor_mog2_set_history"),
            127 => Names("jyppx_ocv_background_subtractor_mog2_get_n_mixtures"),
            128 => Names("jyppx_ocv_background_subtractor_mog2_set_n_mixtures"),
            129 or 131 or 133 or 135 or 137 or 139 or 141 or 147 => Names("jyppx_ocv_background_subtractor_mog2_get_double_property"),
            130 or 132 or 134 or 136 or 138 or 140 or 142 or 148 => Names("jyppx_ocv_background_subtractor_mog2_set_double_property"),
            143 => Names("jyppx_ocv_background_subtractor_mog2_get_detect_shadows"),
            144 => Names("jyppx_ocv_background_subtractor_mog2_set_detect_shadows"),
            145 => Names("jyppx_ocv_background_subtractor_mog2_get_int_property"),
            146 => Names("jyppx_ocv_background_subtractor_mog2_set_int_property"),
            151 => Names("jyppx_ocv_background_subtractor_mog2_create", "jyppx_ocv_background_subtractor_release_handle"),
            153 => Names("jyppx_ocv_background_subtractor_knn_get_history"),
            154 => Names("jyppx_ocv_background_subtractor_knn_set_history"),
            155 => Names("jyppx_ocv_background_subtractor_knn_get_n_samples"),
            156 => Names("jyppx_ocv_background_subtractor_knn_set_n_samples"),
            157 or 165 => Names("jyppx_ocv_background_subtractor_knn_get_double_property"),
            158 or 166 => Names("jyppx_ocv_background_subtractor_knn_set_double_property"),
            159 or 163 => Names("jyppx_ocv_background_subtractor_knn_get_int_property"),
            160 or 164 => Names("jyppx_ocv_background_subtractor_knn_set_int_property"),
            161 => Names("jyppx_ocv_background_subtractor_knn_get_detect_shadows"),
            162 => Names("jyppx_ocv_background_subtractor_knn_set_detect_shadows"),
            167 => Names("jyppx_ocv_background_subtractor_knn_create", "jyppx_ocv_background_subtractor_release_handle"),
            _ => throw new InvalidOperationException("No native evidence mapping for ordinal " + ordinal)
        };
        return names.SelectMany(name => MatchNative(native, name)).ToList();
    }

    private static IEnumerable<string> MatchNative(string[] native, string name)
    {
        string[] matches = native.Where(x => x == name).ToArray();
        Require(matches.Length == 1, "Native evidence not found exactly once: " + name);
        return matches;
    }

    private static string ManagedEvidence(int ordinal, string[] managed)
    {
        string M(string type, params string[] fragments) => FindManaged(managed, "OpenCvSharp.Video." + type, fragments);
        string Property(string type, string name) => M(type, "|property|", " " + name);
        return ordinal switch
        {
            1 => M("Cv2", " CamShift("), 2 => M("Cv2", " MeanShift("), 3 => M("Cv2", " BuildOpticalFlowPyramid("),
            4 => M("Cv2", " CalcOpticalFlowPyrLK("), 5 => M("Cv2", " CalcOpticalFlowFarneback("),
            7 => M("Cv2", " ComputeECC("), 8 or 9 => M("Cv2", " FindTransformECC(", "warpMatrix"),
            10 => M("Cv2", " FindTransformECCWithMask(", "warpMatrix"), 12 => M("ECCParameters", "|constructor|public;instance|"),
            13 => M("Cv2", " FindTransformECCMultiScale(", "warpMatrix"),
            16 => M("KalmanFilter", "|constructor|public;instance|"), 17 => M("KalmanFilter", " Predict("), 18 => M("KalmanFilter", " Correct("),
            19 => M("Cv2", " ReadOpticalFlow("), 20 => M("Cv2", " WriteOpticalFlow("),
            22 => M("DenseOpticalFlow", "System.Void Calc("), 23 => M("DenseOpticalFlow", " CollectGarbage("), 25 => M("SparseOpticalFlow", " Calc("),
            >= 27 and <= 42 => Property("FarnebackOpticalFlow", FarnebackProperty(ordinal)), 43 => M("FarnebackOpticalFlow", "|method|public;static|", " Create("),
            45 => M("VariationalRefinement", " CalcUV("), >= 46 and <= 59 => Property("VariationalRefinement", VariationalProperty(ordinal)), 60 => M("VariationalRefinement", "|method|public;static|", " Create("),
            >= 63 and <= 86 => Property("DisOpticalFlow", DisProperty(ordinal)), 87 => M("DisOpticalFlow", "|method|public;static|", " Create("),
            >= 89 and <= 98 => Property("SparsePyrLkOpticalFlow", SparsePyrProperty(ordinal)), 99 => M("SparsePyrLkOpticalFlow", "|method|public;static|", " Create("),
            101 => M("Tracker", " Init("), 102 => M("Tracker", " Update("), 103 => Property("Tracker", "TrackingScore"),
            106 => Property("TrackerMILParams", "Default"), 107 => M("TrackerMIL", "|method|public;static|", " Create("),
            121 => M("BackgroundSubtractor", "System.Void Apply(", "fgmask"), 122 => M("BackgroundSubtractor", "System.Void Apply(", "knownForegroundMask"), 123 => M("BackgroundSubtractor", "System.Void GetBackgroundImage("),
            >= 125 and <= 128 => Property("BackgroundSubtractorMOG2", ordinal is 125 or 126 ? "History" : "NMixtures"),
            >= 129 and <= 148 => Property("BackgroundSubtractorMOG2", Mog2Property(ordinal)),
            149 => M("BackgroundSubtractor", "System.Void Apply(", "fgmask"), 150 => M("BackgroundSubtractor", "System.Void Apply(", "knownForegroundMask"), 151 => M("BackgroundSubtractorMOG2", "|method|public;static|", " Create("),
            >= 153 and <= 166 => Property("BackgroundSubtractorKNN", KnnProperty(ordinal)), 167 => M("BackgroundSubtractorKNN", "|method|public;static|", " Create("),
            _ => throw new InvalidOperationException("No managed evidence mapping for ordinal " + ordinal)
        };
    }

    private static string FarnebackProperty(int ordinal) => ordinal switch { 27 or 28 => "NumLevels", 29 or 30 => "PyrScale", 31 or 32 => "FastPyramids", 33 or 34 => "WinSize", 35 or 36 => "NumIterations", 37 or 38 => "PolyN", 39 or 40 => "PolySigma", 41 or 42 => "Flags", _ => throw new InvalidOperationException() };
    private static string VariationalProperty(int ordinal) => ordinal switch { 46 or 47 => "FixedPointIterations", 48 or 49 => "SorIterations", 50 or 51 => "Omega", 52 or 53 => "Alpha", 54 or 55 => "Delta", 56 or 57 => "Gamma", 58 or 59 => "Epsilon", _ => throw new InvalidOperationException() };
    private static string DisProperty(int ordinal) => ordinal switch { 63 or 64 => "FinestScale", 65 or 66 => "CoarsestScale", 67 or 68 => "PatchSize", 69 or 70 => "PatchStride", 71 or 72 => "GradientDescentIterations", 73 or 74 => "VariationalRefinementIterations", 75 or 76 => "VariationalRefinementAlpha", 77 or 78 => "VariationalRefinementDelta", 79 or 80 => "VariationalRefinementGamma", 81 or 82 => "VariationalRefinementEpsilon", 83 or 84 => "UseMeanNormalization", 85 or 86 => "UseSpatialPropagation", _ => throw new InvalidOperationException() };
    private static string SparsePyrProperty(int ordinal) => ordinal switch { 89 or 90 => "WinSize", 91 or 92 => "MaxLevel", 93 or 94 => "Criteria", 95 or 96 => "Flags", 97 or 98 => "MinEigThreshold", _ => throw new InvalidOperationException() };
    private static string Mog2Property(int ordinal) => ordinal switch { 129 or 130 => "BackgroundRatio", 131 or 132 => "VarThreshold", 133 or 134 => "VarThresholdGen", 135 or 136 => "VarInit", 137 or 138 => "VarMin", 139 or 140 => "VarMax", 141 or 142 => "ComplexityReductionThreshold", 143 or 144 => "DetectShadows", 145 or 146 => "ShadowValue", 147 or 148 => "ShadowThreshold", _ => throw new InvalidOperationException() };
    private static string KnnProperty(int ordinal) => ordinal switch { 153 or 154 => "History", 155 or 156 => "NSamples", 157 or 158 => "Dist2Threshold", 159 or 160 => "KnnSamples", 161 or 162 => "DetectShadows", 163 or 164 => "ShadowValue", 165 or 166 => "ShadowThreshold", _ => throw new InvalidOperationException() };

    private static string FindManaged(string[] managed, string type, params string[] fragments)
    {
        string prefix = "MEMBER|" + type + "|";
        List<string> matches = managed.Where(x => x.Contains(prefix, StringComparison.Ordinal) && fragments.All(f => x.Contains(f, StringComparison.Ordinal))).OrderBy(x => x, Ordinal).ToList();
        Require(matches.Count > 0, "No managed evidence for " + type + " fragments " + string.Join(",", fragments));
        return matches[0];
    }

    private static void Validate(RawDocument raw, ClassificationDocument classifications, Options? options, string[] native, string[] managed, bool verifyFiles)
    {
        Require(raw.SchemaVersion == 1 && raw.UpstreamOpenCvVersion == "5.0.0" && raw.DeclarationCount == 168 && raw.Declarations.Count == 168, "Video raw identity/count drifted.");
        Require(raw.Declarations.Count(x => x.Kind == "callable") == 145 && raw.Declarations.Count(x => x.Kind == "class") == 20 && raw.Declarations.Count(x => x.Kind == "enum") == 3, "Video declaration partition drifted.");
        Require(raw.SourceHeaders.Count == 2 && raw.CompatibilityHeaders.Count == 2 && raw.ExcludedPublicHeaders.Count == 2, "Video header closure drifted.");
        Require(raw.PreprocessorDefinitions.Count == 2 && raw.PreprocessorDefinitions.GetValueOrDefault("CV_VERSION_MAJOR") == 5 && raw.PreprocessorDefinitions.GetValueOrDefault("OPENCV_ABI_COMPATIBILITY") == 500, "Video parser definitions drifted.");
        Require(raw.ExcludedPublicHeaders.Any(x => x.Path.EndsWith("video/legacy/constants_c.h", StringComparison.Ordinal)) && raw.ExcludedPublicHeaders.Any(x => x.Path.EndsWith("video/detail/tracking.private.hpp", StringComparison.Ordinal)), "Video legacy/private exclusions drifted.");
        int[] starts = { 0, 120 }; int[] counts = { 120, 48 };
        for (int i = 0; i < 2; i++) Require(raw.SourceHeaders[i].StartOrdinal == starts[i] && raw.SourceHeaders[i].DeclarationCount == counts[i], "Video source-header order drifted at " + i);
        for (int i = 0; i < raw.Declarations.Count; i++)
        {
            SourceHeader owner = raw.SourceHeaders.Last(h => h.StartOrdinal <= i);
            Require(raw.Declarations[i].Ordinal == i && !string.IsNullOrWhiteSpace(raw.Declarations[i].Identity) && raw.Declarations[i].SourceHeader == owner.Path, "Video parser order/source/identity drifted at " + i);
        }
        Require(raw.Declarations.Select(x => x.Identity).Distinct(Ordinal).Count() == 168, "Video overload identities collapsed.");
        Require(raw.SourceHeaders.All(x => !x.Path.Contains("videoio", StringComparison.OrdinalIgnoreCase) && !x.Path.Contains("modules/contrib", StringComparison.OrdinalIgnoreCase) && !x.Path.Contains("private", StringComparison.OrdinalIgnoreCase)), "Video source closure contains VideoIO, contrib, or private contamination.");
        Require(classifications.SchemaVersion == 1 && classifications.UpstreamOpenCvVersion == "5.0.0" && classifications.ClaimedSlice == ClaimedSlice && classifications.ReviewStatus == "source-reviewed" && !string.IsNullOrWhiteSpace(classifications.Limitation), "Video classification identity drifted.");
        Require(!classifications.ClaimedSlice.Contains("OpenCv5Sharp", StringComparison.Ordinal) && !classifications.ClaimedSlice.Contains("VideoIO", StringComparison.Ordinal) && classifications.Declarations.Count == 168, "Video fixed-major identity, module boundary, or row count drifted.");
        var nativeSet = new HashSet<string>(native, Ordinal); var managedSet = new HashSet<string>(managed, Ordinal);
        for (int i = 0; i < 168; i++)
        {
            RawDeclaration declaration = raw.Declarations[i]; ClassificationRow row = classifications.Declarations[i];
            Require(row.Ordinal == i && row.Identity == declaration.Identity && Allowed.Contains(row.Classification, Ordinal), "Video classification order/value drifted at " + i);
            Require(!string.IsNullOrWhiteSpace(row.Reason) && !string.IsNullOrWhiteSpace(row.BuildCondition), "Undocumented Video row at " + i);
            Require(row.NativeEntrypoints.SequenceEqual(row.NativeEntrypoints.Distinct(Ordinal).OrderBy(x => x, Ordinal)) && row.ManagedMembers.SequenceEqual(row.ManagedMembers.Distinct(Ordinal).OrderBy(x => x, Ordinal)), "Nondeterministic Video evidence ordering at " + i);
            Require(declaration.Kind == "callable" ? row.Classification != "non-callable-metadata" : row.Classification == "non-callable-metadata", "Video callable/metadata confusion at " + i);
            if (row.Classification == "implemented")
                Require(row.NativeEntrypoints.Count > 0 && row.ManagedMembers.Count > 0 && row.NativeEntrypoints.All(nativeSet.Contains) && row.ManagedMembers.All(managedSet.Contains), "False or missing Video evidence at " + i);
            else if (declaration.Kind == "callable") Require(row.NativeEntrypoints.Count == 0 && row.ManagedMembers.Count == 0, "Non-implemented Video callable carries evidence at " + i);
            Require(row.BuildCondition == "OPENCV_CSHARP_HAS_OPENCV_VIDEO; full-profile; mini-excluded", "Video build condition drifted at " + i);
        }
        Require(classifications.Declarations.Count(x => x.Classification == "implemented") == 138 && classifications.Declarations.Count(x => x.Classification == "missing") == 0 && classifications.Declarations.Count(x => x.Classification == "intentionally-omitted") == 7 && classifications.Declarations.Count(x => x.Classification == "non-callable-metadata") == 23, "Video callable partition drifted.");
        Require(Existing.All(i => classifications.Declarations[i].Classification == "implemented"), "Existing Video correlation is incomplete.");
        Require(Selected.All(i => classifications.Declarations[i].Classification == "implemented"), "Selected Video batch is incomplete.");
        Require(Omitted.All(i => classifications.Declarations[i].Classification == "intentionally-omitted"), "Video intentional-omission partition drifted.");
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
        b.AppendLine("# Generated by tools/VideoUpstreamMap. Do not edit."); b.AppendLine("schema-version=1"); b.AppendLine("upstream-opencv-version=5.0.0"); b.AppendLine("claimed-slice=" + ClaimedSlice); b.AppendLine("header-sha256=" + raw.HeaderSha256); b.AppendLine("parser-sha256=" + raw.ParserSha256); b.AppendLine("declaration-count=168"); b.AppendLine("callable-count=145"); b.AppendLine("class-count=20"); b.AppendLine("enum-count=3"); b.AppendLine("repository-wide-upstream-parity-claimed=false");
        foreach (CompatibilityHeader h in raw.CompatibilityHeaders) b.AppendLine($"compatibility-header={h.Path}|{h.Sha256}|includes={h.Includes}");
        b.AppendLine(); b.AppendLine("ordinal|kind|source-header|classification|identity|native-entrypoints|managed-members|build-condition|reason");
        for (int i = 0; i < 168; i++)
        {
            RawDeclaration d = raw.Declarations[i]; ClassificationRow r = classifications.Declarations[i];
            b.AppendLine($"{i}|{d.Kind}|{d.SourceHeader}|{r.Classification}|{d.Identity}|{Join(r.NativeEntrypoints)}|{Join(r.ManagedMembers)}|{r.BuildCondition}|{r.Reason}");
        }
        return b.ToString().Replace("\r\n", "\n", StringComparison.Ordinal);
    }

    private static FamilyDocument BuildFamilies(RawDocument raw, ClassificationDocument classifications)
    {
        FamilyRow BuildFamily(string id, string rationale, IEnumerable<int> ordinals, string focusedTest)
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
                    FocusedTest = focusedTest
                });
            }
            return family;
        }

        IEnumerable<int> opticalFlow = Selected.Except(EccSelected).Except(TrackerMilSelected);
        return new FamilyDocument
        {
            Families = new()
            {
                BuildFamily(
                    "video-optical-flow-objects",
                    "The selected batch closes the parser-emitted Dense/Sparse OpticalFlow bases plus Farneback, VariationalRefinement, DIS, and SparsePyrLK factories, operations, typed properties, and owned lifetimes with deterministic offline inputs.",
                    opticalFlow,
                    "tests/OpenCvSharp.Tests/Video/VideoOpticalFlowObjectTests.cs"),
                BuildFamily(
                    "video-ecc-registration",
                    "The ECC batch closes correlation, single- and dual-mask registration, multiscale parameters, caller-owned warp updates, and independently owned result matrices.",
                    EccSelected,
                    "tests/OpenCvSharp.Tests/Video/VideoEccTrackerMilTests.cs"),
                BuildFamily(
                    "video-tracker-mil",
                    "The main Video TrackerMIL batch closes default and copied parameters, factory ownership, initialization, update, tracking-score, and disposal semantics independently of contrib Tracking wrappers.",
                    TrackerMilSelected,
                    "tests/OpenCvSharp.Tests/Video/VideoEccTrackerMilTests.cs")
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
            Require(failed, "A Video negative fixture was accepted."); passed++;
        }
        Fixture((_, c) => c.Declarations.RemoveAt(0));
        Fixture((_, c) => c.Declarations[1].Ordinal = 0);
        Fixture((_, c) => (c.Declarations[0], c.Declarations[1]) = (c.Declarations[1], c.Declarations[0]));
        Fixture((r, _) => r.Declarations[2].Identity = r.Declarations[1].Identity);
        Fixture((_, c) => c.Declarations[15].Classification = "non-callable-metadata");
        Fixture((r, _) => r.Declarations[0].SourceHeader = "drifted/video.hpp");
        Fixture((r, _) => r.ParserSha256 = new string('0', 64));
        Fixture((r, _) => r.HeaderSha256 = new string('0', 64));
        Fixture((_, c) => c.Declarations[1].NativeEntrypoints[0] = "jyppx_ocv_false_evidence");
        Fixture((_, c) => c.Declarations[1].ManagedMembers[0] = "MEMBER|false");
        Fixture((_, c) => c.Declarations[7].Reason = "");
        Fixture((_, c) => c.ClaimedSlice += "; OpenCv5Sharp");
        Fixture((_, c) => c.Declarations[27].BuildCondition = "unconditional");
        Fixture((_, c) => c.ClaimedSlice += "; VideoIO; contrib Tracking");
        Fixture((_, c) => c.Declarations[3].NativeEntrypoints.Reverse());
        Fixture((_, c) => c.Declarations[111].Classification = "missing");
        Fixture((r, _) => r.SourceHeaders.RemoveAt(0));
        Require(passed == 17, "Video negative fixture count drifted.");
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
        string path = Path.Combine(workspace, relative.Replace('/', Path.DirectorySeparatorChar)); Require(File.Exists(path), "Video " + label + " missing: " + relative); Require(Sha256File(path) == expected, "Video " + label + " hash drifted: " + relative);
    }
    private static string Join(List<string> values) => values.Count == 0 ? "-" : string.Join(";", values.Select(x => x.Replace("|", "<pipe>", StringComparison.Ordinal)));
    private static string Rel(string root, string path) => Path.GetRelativePath(root, path).Replace('\\', '/');
    private static string Sha256(string text) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
    private static string Sha256File(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}

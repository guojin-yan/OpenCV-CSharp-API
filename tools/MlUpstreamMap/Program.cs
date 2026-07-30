using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

internal static class Program
{
    private static readonly StringComparer Ordinal = StringComparer.Ordinal;
    private static readonly HashSet<int> Existing = new(
        new[] { 4 }
            .Concat(Enumerable.Range(6, 39).Where(i => i is not 12 and not 31))
            .Concat(Enumerable.Range(47, 8))
            .Concat(Enumerable.Range(56, 3))
            .Concat(Enumerable.Range(60, 9))
            .Concat(new[] { 70, 71 })
            .Concat(Enumerable.Range(73, 20))
            .Concat(Enumerable.Range(96, 7)));
    private static readonly HashSet<int> AnnMlpSelected = new(
        Enumerable.Range(168, 29).Concat(Enumerable.Range(199, 3)));
    private static readonly HashSet<int> EmSelected = new(Enumerable.Range(107, 16));
    private static readonly HashSet<int> TreeModelsSelected = new(
        Enumerable.Range(125, 20)
            .Concat(Enumerable.Range(146, 10))
            .Concat(Enumerable.Range(157, 6))
            .Concat(Enumerable.Range(164, 2)));
    private static readonly HashSet<int> Selected = new(AnnMlpSelected.Concat(EmSelected).Concat(TreeModelsSelected));
    private static readonly string[] Allowed =
    {
        "implemented", "missing", "intentionally-omitted", "upstream-conditional", "unsupported", "non-callable-metadata"
    };
    private const string ClaimedSlice = "opencv2/ml/ml.hpp compatibility include measured through the parser-emitted OpenCV 5.0.0 contrib ML public source header";
    private const int NegativeFixtureCount = 17;
    private const int ManagedTypeAdditions = 12;
    private const int ManagedMemberAdditions = 108;
    private const int NativeEntrypointAdditions = 51;

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
        public string Limitation { get; set; } = "The map covers only the exact OpenCV 5.0.0 contrib ML compatibility include closure. Missing algorithms remain explicit gaps, and repository-wide parity is not claimed.";
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
        public string Generator { get; init; } = "tools/MlUpstreamMap";
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
        public int NegativeFixtureCount { get; init; } = Program.NegativeFixtureCount;
        public string FamilyInventoryPath { get; init; } = "";
        public string FamilyInventorySha256 { get; init; } = "";
        public int SelectedFamilyCount { get; init; }
        public int SelectedDeclarationCount { get; init; }
        public int SourceReviewedExtensionCount { get; init; } = 2;
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
        public List<SourceReviewedExtension> SourceReviewedExtensions { get; init; } = new();
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
        public string FocusedTest { get; init; } = "tests/OpenCvSharp.Tests/ML/MLTests.cs";
        public string NativeSmoke { get; init; } = "src/OpenCvSharp.Native/tests/native_smoke.cpp";
        public string Sample { get; init; } = "samples/ConsoleSamples/Program.cs";
        public string Guide { get; init; } = "docs/articles/ml-guide.md";
    }
    private sealed class SourceReviewedExtension
    {
        public string UpstreamIdentity { get; init; } = "";
        public string SourceHeader { get; init; } = "opencv-source/opencv_contrib-5.0.0/modules/ml/include/opencv2/ml.hpp";
        public string Adaptation { get; init; } = "";
        public List<string> NativeEntrypoints { get; init; } = new();
        public List<string> ManagedMembers { get; init; } = new();
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
            string familyText = Serialize(BuildFamilies(raw, classifications, managed));
            var counts = new SortedDictionary<string, int>(Ordinal);
            foreach (string value in Allowed) counts[value] = classifications.Declarations.Count(row => row.Classification == value);
            var summary = new SummaryDocument
            {
                RawExtractionPath = Rel(options.Repository, options.Raw),
                ClassificationPath = Rel(options.Repository, options.Classification),
                MappingPath = Rel(options.Repository, options.Output),
                HeaderSha256 = raw.HeaderSha256,
                ParserSha256 = raw.ParserSha256,
                CompatibilityHeaderCount = raw.CompatibilityHeaders.Count,
                ExcludedPublicHeaderCount = raw.ExcludedPublicHeaders.Count,
                SourceHeaderCount = raw.SourceHeaders.Count,
                SourceHeaderSetSha256 = Sha256(string.Join("\n", raw.SourceHeaders.Select(x => $"{x.Path}|{x.Sha256}|{x.StartOrdinal}|{x.DeclarationCount}")) + "\n"),
                MappingSha256 = Sha256(mapping),
                DeclarationCount = raw.Declarations.Count,
                EnumCount = raw.Declarations.Count(x => x.Kind == "enum"),
                ClassCount = raw.Declarations.Count(x => x.Kind == "class"),
                CallableCount = raw.Declarations.Count(x => x.Kind == "callable"),
                ClassificationCounts = counts,
                NativeEvidenceCount = classifications.Declarations.SelectMany(x => x.NativeEntrypoints).Distinct(Ordinal).Count(),
                ManagedEvidenceCount = classifications.Declarations.SelectMany(x => x.ManagedMembers).Distinct(Ordinal).Count(),
                FamilyInventoryPath = Rel(options.Repository, options.FamilyOutput),
                FamilyInventorySha256 = Sha256(familyText),
                SelectedFamilyCount = 3,
                SelectedDeclarationCount = Selected.Count,
                RepositoryWideUpstreamParityClaimed = false
            };
            RunNegativeFixtures(raw, classifications, options, native, managed);
            WriteOrCheck(options.Output, mapping, options.Check);
            WriteOrCheck(options.FamilyOutput, familyText, options.Check);
            WriteOrCheck(options.Summary, Serialize(summary), options.Check);
            Console.WriteLine($"ML_UPSTREAM_MAP_OK declarations={summary.DeclarationCount} callables={summary.CallableCount} implemented={counts["implemented"]} missing={counts["missing"]} fixtures={NegativeFixtureCount} sha256={summary.MappingSha256} mode={(options.Check ? "check" : "write")}");
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
        string Value(string name)
        {
            Require(values.TryGetValue(name, out string? value), "Missing option: " + name);
            return Path.GetFullPath(value!);
        }
        return new Options(Value("--repository"), Value("--workspace"), Value("--raw"), Value("--classification"), Value("--native-manifest"), Value("--managed-baseline"), Value("--output"), Value("--summary"), Value("--family-output"), initialize, check);
    }

    private static ClassificationDocument Initialize(RawDocument raw, string[] native, string[] managed)
    {
        var result = new ClassificationDocument();
        foreach (RawDeclaration declaration in raw.Declarations)
        {
            var row = new ClassificationRow
            {
                Ordinal = declaration.Ordinal,
                Identity = declaration.Identity,
                BuildCondition = "OPENCV_CSHARP_HAS_OPENCV_ML; full-profile; mini-excluded"
            };
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
                    ? SelectedReason(declaration.Ordinal)
                    : "The existing version-neutral native and managed ML surface provides the callable semantics represented by this parser row.";
                row.NativeEntrypoints.AddRange(NativeEvidence(declaration.Ordinal, native));
                row.ManagedMembers.AddRange(ManagedEvidence(declaration.Ordinal, managed));
            }
            row.NativeEntrypoints = row.NativeEntrypoints.Distinct(Ordinal).OrderBy(x => x, Ordinal).ToList();
            row.ManagedMembers = row.ManagedMembers.Distinct(Ordinal).OrderBy(x => x, Ordinal).ToList();
            result.Declarations.Add(row);
        }
        Validate(raw, result, null, native, managed, false);
        return result;
    }

    private static string SelectedReason(int ordinal)
    {
        if (AnnMlpSelected.Contains(ordinal))
            return "The selected ANN_MLP batch has one owned StatModel handle, copied Mat outputs, typed configuration, deterministic seed adaptation, native smoke, net8/net10 managed tests, persistence, and an offline sample.";
        if (EmSelected.Contains(ordinal))
            return "The selected EM batch has one owned StatModel handle, copied Mat and covariance outputs, typed configuration, caller-owned optional outputs, native smoke, net8/net10 managed tests, persistence, and an offline sample.";
        if (TreeModelsSelected.Contains(ordinal))
            return "The selected DTrees, RTrees, and Boost batch preserves upstream inheritance, typed tree configuration, copied matrix outputs, native smoke, net8/net10 managed tests, persistence, and an offline sample.";
        throw new InvalidOperationException("No selected-family rationale for ordinal " + ordinal);
    }

    private static string MissingReason(int ordinal)
    {
        if (ordinal is 12 or 31)
            return "This TrainData pointer-buffer callable has no current stable span or caller-buffer ABI contract and remains an explicit full-profile gap.";
        if (ordinal is >= 203 and <= 220)
            return "The LogisticRegression model lifecycle, optimizer configuration, learned-theta output, and prediction surface have no current native or managed evidence and remain an explicit full-profile family gap.";
        if (ordinal is >= 224 and <= 240)
            return "The SVMSGD model lifecycle, margin configuration, weight output, and termination surface have no current native or managed evidence and remain an explicit full-profile family gap.";
        throw new InvalidOperationException("No missing rationale for ordinal " + ordinal);
    }

    private static List<string> NativeEvidence(int ordinal, string[] native)
    {
        string[] names = ordinal switch
        {
            4 => N("jyppx_ocv_ml_param_grid_create"),
            >= 6 and <= 11 or 28 or 33 => N("jyppx_ocv_ml_train_data_get_int"),
            >= 13 and <= 30 or 32 or >= 34 and <= 36 or 40 => N("jyppx_ocv_ml_train_data_get_mat"),
            37 => N("jyppx_ocv_ml_train_data_set_train_test_split"),
            38 => N("jyppx_ocv_ml_train_data_set_train_test_split_ratio"),
            39 => N("jyppx_ocv_ml_train_data_shuffle_train_test"),
            41 => N("jyppx_ocv_ml_train_data_get_names_count", "jyppx_ocv_ml_train_data_get_names_fill"),
            42 => N("jyppx_ocv_ml_train_data_get_sub_vector"),
            43 => N("jyppx_ocv_ml_train_data_get_sub_matrix"),
            44 => N("jyppx_ocv_ml_train_data_create"),
            >= 47 and <= 50 => N("jyppx_ocv_ml_stat_model_get_int"),
            51 => N("jyppx_ocv_ml_stat_model_train_data"),
            52 => N("jyppx_ocv_ml_stat_model_train_samples"),
            53 => N("jyppx_ocv_ml_stat_model_calc_error"),
            54 => N("jyppx_ocv_ml_stat_model_predict"),
            56 => N("jyppx_ocv_ml_normal_bayes_classifier_predict_prob"),
            57 => N("jyppx_ocv_ml_normal_bayes_classifier_create"),
            58 => N("jyppx_ocv_ml_normal_bayes_classifier_load"),
            60 or 62 or 64 or 66 => N("jyppx_ocv_ml_knearest_get_int"),
            61 or 63 or 65 or 67 => N("jyppx_ocv_ml_knearest_set_int"),
            68 => N("jyppx_ocv_ml_knearest_find_nearest"),
            70 => N("jyppx_ocv_ml_knearest_create"),
            71 => N("jyppx_ocv_ml_knearest_load"),
            73 or 91 => N("jyppx_ocv_ml_svm_get_int"),
            74 or 92 => N("jyppx_ocv_ml_svm_set_int"),
            75 or 77 or 79 or 81 or 83 or 85 => N("jyppx_ocv_ml_svm_get_double"),
            76 or 78 or 80 or 82 or 84 or 86 => N("jyppx_ocv_ml_svm_set_double"),
            87 => N("jyppx_ocv_ml_svm_get_class_weights"),
            88 => N("jyppx_ocv_ml_svm_set_class_weights"),
            89 => N("jyppx_ocv_ml_svm_get_term_criteria"),
            90 => N("jyppx_ocv_ml_svm_set_term_criteria"),
            96 => N("jyppx_ocv_ml_svm_train_auto"),
            97 => N("jyppx_ocv_ml_svm_get_support_vectors"),
            98 => N("jyppx_ocv_ml_svm_get_uncompressed_support_vectors"),
            99 => N("jyppx_ocv_ml_svm_get_decision_function"),
            100 => N("jyppx_ocv_ml_svm_get_default_grid"),
            101 => N("jyppx_ocv_ml_svm_create"),
            102 => N("jyppx_ocv_ml_svm_load"),
            107 or 109 => N("jyppx_ocv_ml_em_get_int"),
            108 or 110 => N("jyppx_ocv_ml_em_set_int"),
            111 => N("jyppx_ocv_ml_em_get_term_criteria"),
            112 => N("jyppx_ocv_ml_em_set_term_criteria"),
            113 => N("jyppx_ocv_ml_em_get_weights"),
            114 => N("jyppx_ocv_ml_em_get_means"),
            115 => N("jyppx_ocv_ml_em_get_covariances_count", "jyppx_ocv_ml_em_get_covariances_fill"),
            116 => N("jyppx_ocv_ml_stat_model_predict"),
            117 => N("jyppx_ocv_ml_em_predict2"),
            118 => N("jyppx_ocv_ml_em_train_em"),
            119 => N("jyppx_ocv_ml_em_train_e"),
            120 => N("jyppx_ocv_ml_em_train_m"),
            121 => N("jyppx_ocv_ml_em_create"),
            122 => N("jyppx_ocv_ml_em_load"),
            125 or 127 or 129 or 131 or 133 or 135 or 137 => N("jyppx_ocv_ml_dtrees_get_int"),
            126 or 128 or 130 or 132 or 134 or 136 or 138 => N("jyppx_ocv_ml_dtrees_set_int"),
            139 => N("jyppx_ocv_ml_dtrees_get_regression_accuracy"),
            140 => N("jyppx_ocv_ml_dtrees_set_regression_accuracy"),
            141 => N("jyppx_ocv_ml_dtrees_get_priors"),
            142 => N("jyppx_ocv_ml_dtrees_set_priors"),
            143 => N("jyppx_ocv_ml_dtrees_create"),
            144 => N("jyppx_ocv_ml_dtrees_load"),
            146 or 148 => N("jyppx_ocv_ml_rtrees_get_int"),
            147 or 149 => N("jyppx_ocv_ml_rtrees_set_int"),
            150 => N("jyppx_ocv_ml_rtrees_get_term_criteria"),
            151 => N("jyppx_ocv_ml_rtrees_set_term_criteria"),
            152 => N("jyppx_ocv_ml_rtrees_get_var_importance"),
            153 => N("jyppx_ocv_ml_rtrees_get_votes"),
            154 => N("jyppx_ocv_ml_rtrees_create"),
            155 => N("jyppx_ocv_ml_rtrees_load"),
            157 or 159 => N("jyppx_ocv_ml_boost_get_int"),
            158 or 160 => N("jyppx_ocv_ml_boost_set_int"),
            161 => N("jyppx_ocv_ml_boost_get_weight_trim_rate"),
            162 => N("jyppx_ocv_ml_boost_set_weight_trim_rate"),
            164 => N("jyppx_ocv_ml_boost_create"),
            165 => N("jyppx_ocv_ml_boost_load"),
            168 => N("jyppx_ocv_ml_ann_mlp_set_train_method"),
            169 or 195 => N("jyppx_ocv_ml_ann_mlp_get_int"),
            170 => N("jyppx_ocv_ml_ann_mlp_set_activation_function"),
            171 => N("jyppx_ocv_ml_ann_mlp_set_layer_sizes"),
            172 => N("jyppx_ocv_ml_ann_mlp_get_layer_sizes"),
            173 => N("jyppx_ocv_ml_ann_mlp_get_term_criteria"),
            174 => N("jyppx_ocv_ml_ann_mlp_set_term_criteria"),
            175 or 177 or 179 or 181 or 183 or 185 or 187 or 189 or 191 or 193 => N("jyppx_ocv_ml_ann_mlp_get_double"),
            176 or 178 or 180 or 182 or 184 or 186 or 188 or 190 or 192 or 194 => N("jyppx_ocv_ml_ann_mlp_set_double"),
            196 => N("jyppx_ocv_ml_ann_mlp_set_int"),
            199 => N("jyppx_ocv_ml_ann_mlp_get_weights"),
            200 => N("jyppx_ocv_ml_ann_mlp_create"),
            201 => N("jyppx_ocv_ml_ann_mlp_load"),
            _ => throw new InvalidOperationException("No native evidence mapping for ordinal " + ordinal)
        };
        var nativeSet = new HashSet<string>(native, Ordinal);
        Require(names.All(nativeSet.Contains), "Native evidence is absent for ordinal " + ordinal);
        return names.OrderBy(x => x, Ordinal).ToList();
    }

    private static List<string> ManagedEvidence(int ordinal, string[] managed)
    {
        string member = ordinal switch
        {
            4 => M(managed, "ParamGrid", "|constructor|", ".ctor("),
            6 => M(managed, "TrainData", "|property|", " Layout"),
            7 => M(managed, "TrainData", "|property|", " NTrainSamples"),
            8 => M(managed, "TrainData", "|property|", " NTestSamples"),
            9 => M(managed, "TrainData", "|property|", " NSamples"),
            10 => M(managed, "TrainData", "|property|", " NVars"),
            11 => M(managed, "TrainData", "|property|", " NAllVars"),
            13 => M(managed, "TrainData", " GetSamples("),
            14 => M(managed, "TrainData", " GetMissing("),
            15 => M(managed, "TrainData", " GetTrainSamples("),
            16 => M(managed, "TrainData", " GetTrainResponses("),
            17 => M(managed, "TrainData", " GetTrainNormCatResponses("),
            18 => M(managed, "TrainData", " GetTestResponses("),
            19 => M(managed, "TrainData", " GetTestNormCatResponses("),
            20 => M(managed, "TrainData", " GetResponses("),
            21 => M(managed, "TrainData", " GetNormCatResponses("),
            22 => M(managed, "TrainData", " GetSampleWeights("),
            23 => M(managed, "TrainData", " GetTrainSampleWeights("),
            24 => M(managed, "TrainData", " GetTestSampleWeights("),
            25 => M(managed, "TrainData", " GetVarIdx("),
            26 => M(managed, "TrainData", " GetVarType("),
            27 => M(managed, "TrainData", " GetVarSymbolFlags("),
            28 => M(managed, "TrainData", "|property|", " ResponseType"),
            29 => M(managed, "TrainData", " GetTrainSampleIdx("),
            30 => M(managed, "TrainData", " GetTestSampleIdx("),
            32 => M(managed, "TrainData", " GetDefaultSubstValues("),
            33 => M(managed, "TrainData", " GetCatCount("),
            34 => M(managed, "TrainData", " GetClassLabels("),
            35 => M(managed, "TrainData", " GetCatOfs("),
            36 => M(managed, "TrainData", " GetCatMap("),
            37 => M(managed, "TrainData", " SetTrainTestSplit("),
            38 => M(managed, "TrainData", " SetTrainTestSplitRatio("),
            39 => M(managed, "TrainData", " ShuffleTrainTest("),
            40 => M(managed, "TrainData", " GetTestSamples("),
            41 => M(managed, "TrainData", " GetNames("),
            42 => M(managed, "TrainData", " GetSubVector(", "|method|public;static|OpenCvSharp.Core.Mat"),
            43 => M(managed, "TrainData", " GetSubMatrix(", "|method|public;static|OpenCvSharp.Core.Mat"),
            44 => M(managed, "TrainData", "|method|public;static|OpenCvSharp.ML.TrainData Create("),
            47 => M(managed, "StatModel", "|property|", " VarCount"),
            48 => M(managed, "StatModel", "|property|", " Empty"),
            49 => M(managed, "StatModel", "|property|", " IsTrained"),
            50 => M(managed, "StatModel", "|property|", " IsClassifier"),
            51 => M(managed, "StatModel", " Train(OpenCvSharp.ML.TrainData"),
            52 => M(managed, "StatModel", " Train(OpenCvSharp.Core.Mat"),
            53 => M(managed, "StatModel", " CalcError("),
            54 => M(managed, "StatModel", " Predict("),
            56 => M(managed, "NormalBayesClassifier", " PredictProb("),
            57 => M(managed, "NormalBayesClassifier", " Create("),
            58 => M(managed, "NormalBayesClassifier", " Load("),
            60 or 61 => M(managed, "KNearest", "|property|", " DefaultK"),
            62 or 63 => M(managed, "KNearest", "|property|", " IsClassifierModel"),
            64 or 65 => M(managed, "KNearest", "|property|", " Emax"),
            66 or 67 => M(managed, "KNearest", "|property|", " AlgorithmType"),
            68 => M(managed, "KNearest", " FindNearest("),
            70 => M(managed, "KNearest", " Create("),
            71 => M(managed, "KNearest", " Load("),
            73 or 74 => M(managed, "SVM", "|property|", " Type"),
            75 or 76 => M(managed, "SVM", "|property|", " Gamma"),
            77 or 78 => M(managed, "SVM", "|property|", " Coef0"),
            79 or 80 => M(managed, "SVM", "|property|", " Degree"),
            81 or 82 => Exact(managed, "MEMBER|OpenCvSharp.ML.SVM|property|instance;get:public;set:public|System.Double C"),
            83 or 84 => M(managed, "SVM", "|property|", " Nu"),
            85 or 86 => M(managed, "SVM", "|property|", " P"),
            87 => Exact(managed, "MEMBER|OpenCvSharp.ML.SVM|method|public;instance|OpenCvSharp.Core.Mat GetClassWeights()"),
            88 => M(managed, "SVM", " SetClassWeights("),
            89 or 90 => M(managed, "SVM", "|property|", " TermCriteria"),
            91 => M(managed, "SVM", "|property|", " KernelType"),
            92 => M(managed, "SVM", " SetKernel("),
            96 => M(managed, "SVM", " TrainAuto("),
            97 => M(managed, "SVM", " GetSupportVectors("),
            98 => M(managed, "SVM", " GetUncompressedSupportVectors("),
            99 => M(managed, "SVM", " GetDecisionFunction("),
            100 => M(managed, "SVM", " GetDefaultGrid("),
            101 => M(managed, "SVM", " Create("),
            102 => M(managed, "SVM", " Load("),
            107 or 108 => M(managed, "EM", "|property|", " ClustersNumber"),
            109 or 110 => M(managed, "EM", "|property|", " CovarianceMatrixType"),
            111 or 112 => M(managed, "EM", "|property|", " TermCriteria"),
            113 => Exact(managed, "MEMBER|OpenCvSharp.ML.EM|method|public;instance|OpenCvSharp.Core.Mat GetWeights()"),
            114 => Exact(managed, "MEMBER|OpenCvSharp.ML.EM|method|public;instance|OpenCvSharp.Core.Mat GetMeans()"),
            115 => M(managed, "EM", " GetCovariances()"),
            116 => M(managed, "StatModel", " Predict("),
            117 => M(managed, "EM", " Predict2("),
            118 => M(managed, "EM", " TrainEM("),
            119 => M(managed, "EM", " TrainE("),
            120 => M(managed, "EM", " TrainM("),
            121 => M(managed, "EM", "|method|public;static|", " Create("),
            122 => M(managed, "EM", "|method|public;static|", " Load("),
            125 or 126 => M(managed, "DTrees", "|property|", " MaxCategories"),
            127 or 128 => M(managed, "DTrees", "|property|", " MaxDepth"),
            129 or 130 => M(managed, "DTrees", "|property|", " MinSampleCount"),
            131 or 132 => M(managed, "DTrees", "|property|", " CVFolds"),
            133 or 134 => M(managed, "DTrees", "|property|", " UseSurrogates"),
            135 or 136 => M(managed, "DTrees", "|property|", " Use1SERule"),
            137 or 138 => M(managed, "DTrees", "|property|", " TruncatePrunedTree"),
            139 or 140 => M(managed, "DTrees", "|property|", " RegressionAccuracy"),
            141 => Exact(managed, "MEMBER|OpenCvSharp.ML.DTrees|method|public;instance|OpenCvSharp.Core.Mat GetPriors()"),
            142 => M(managed, "DTrees", " SetPriors("),
            143 => M(managed, "DTrees", "|method|public;static|", " Create("),
            144 => M(managed, "DTrees", "|method|public;static|", " Load("),
            146 or 147 => M(managed, "RTrees", "|property|", " CalculateVarImportance"),
            148 or 149 => M(managed, "RTrees", "|property|", " ActiveVarCount"),
            150 or 151 => M(managed, "RTrees", "|property|", " TermCriteria"),
            152 => Exact(managed, "MEMBER|OpenCvSharp.ML.RTrees|method|public;instance|OpenCvSharp.Core.Mat GetVarImportance()"),
            153 => M(managed, "RTrees", "|method|public;instance|System.Void", " GetVotes("),
            154 => M(managed, "RTrees", "|method|public;static|", " Create("),
            155 => M(managed, "RTrees", "|method|public;static|", " Load("),
            157 or 158 => M(managed, "Boost", "|property|", " BoostType"),
            159 or 160 => M(managed, "Boost", "|property|", " WeakCount"),
            161 or 162 => M(managed, "Boost", "|property|", " WeightTrimRate"),
            164 => M(managed, "Boost", "|method|public;static|", " Create("),
            165 => M(managed, "Boost", "|method|public;static|", " Load("),
            168 => M(managed, "ANN_MLP", " SetTrainMethod("),
            169 => M(managed, "ANN_MLP", "|property|", " TrainingMethod"),
            170 => M(managed, "ANN_MLP", " SetActivationFunction("),
            171 => M(managed, "ANN_MLP", " SetLayerSizes("),
            172 => M(managed, "ANN_MLP", " GetLayerSizes()"),
            173 or 174 => M(managed, "ANN_MLP", "|property|", " TermCriteria"),
            175 or 176 => M(managed, "ANN_MLP", "|property|", " BackpropWeightScale"),
            177 or 178 => M(managed, "ANN_MLP", "|property|", " BackpropMomentumScale"),
            179 or 180 => M(managed, "ANN_MLP", "|property|", " RpropDW0"),
            181 or 182 => M(managed, "ANN_MLP", "|property|", " RpropDWPlus"),
            183 or 184 => M(managed, "ANN_MLP", "|property|", " RpropDWMinus"),
            185 or 186 => Exact(managed, "MEMBER|OpenCvSharp.ML.ANN_MLP|property|instance;get:public;set:public|System.Double RpropDWMin"),
            187 or 188 => M(managed, "ANN_MLP", "|property|", " RpropDWMax"),
            189 or 190 => M(managed, "ANN_MLP", "|property|", " AnnealInitialT"),
            191 or 192 => M(managed, "ANN_MLP", "|property|", " AnnealFinalT"),
            193 or 194 => M(managed, "ANN_MLP", "|property|", " AnnealCoolingRatio"),
            195 or 196 => M(managed, "ANN_MLP", "|property|", " AnnealIterationsPerStep"),
            199 => M(managed, "ANN_MLP", " GetWeights(System.Int32 layerIndex)"),
            200 => M(managed, "ANN_MLP", " Create("),
            201 => M(managed, "ANN_MLP", " Load("),
            _ => throw new InvalidOperationException("No managed evidence mapping for ordinal " + ordinal)
        };
        return new() { member };
    }

    private static string M(string[] managed, string type, params string[] fragments)
    {
        string prefix = "MEMBER|OpenCvSharp.ML." + type + "|";
        List<string> matches = managed.Where(x => x.Contains(prefix, StringComparison.Ordinal) && fragments.All(f => x.Contains(f, StringComparison.Ordinal))).OrderBy(x => x, Ordinal).ToList();
        Require(matches.Count == 1, $"Expected one managed evidence row for {type} fragments {string.Join(",", fragments)}; found {matches.Count}.");
        return matches[0];
    }

    private static string Exact(string[] managed, string value)
    {
        Require(managed.Contains(value, Ordinal), "Managed evidence is absent: " + value);
        return value;
    }

    private static void Validate(RawDocument raw, ClassificationDocument classifications, Options? options, string[] native, string[] managed, bool verifyFiles)
    {
        Require(raw.SchemaVersion == 1 && raw.Generator == "tools/MlUpstreamMap/extract_ml.py" && raw.UpstreamOpenCvVersion == "5.0.0", "ML raw identity drifted.");
        Require(raw.DeclarationCount == 241 && raw.Declarations.Count == 241, "ML declaration count drifted.");
        Require(raw.Declarations.Count(x => x.Kind == "callable") == 208 && raw.Declarations.Count(x => x.Kind == "class") == 13 && raw.Declarations.Count(x => x.Kind == "enum") == 20, "ML declaration partition drifted.");
        Require(raw.SourceHeaders.Count == 1 && raw.CompatibilityHeaders.Count == 1 && raw.ExcludedPublicHeaders.Count == 1, "ML header closure drifted.");
        Require(raw.SourceHeaders[0].StartOrdinal == 0 && raw.SourceHeaders[0].DeclarationCount == 241 && raw.SourceHeaders[0].Path.EndsWith("opencv_contrib-5.0.0/modules/ml/include/opencv2/ml.hpp", StringComparison.Ordinal), "ML source header identity drifted.");
        Require(raw.CompatibilityHeaders[0].Path == raw.HeaderPath && raw.CompatibilityHeaders[0].Includes == "opencv2/ml.hpp", "ML compatibility header identity drifted.");
        Require(raw.ExcludedPublicHeaders[0].Path.EndsWith("opencv2/ml/ml.inl.hpp", StringComparison.Ordinal) && !string.IsNullOrWhiteSpace(raw.ExcludedPublicHeaders[0].Reason), "ML inl exclusion drifted.");
        Require(raw.PreprocessorDefinitions.Count == 2 && raw.PreprocessorDefinitions.GetValueOrDefault("CV_VERSION_MAJOR") == 5 && raw.PreprocessorDefinitions.GetValueOrDefault("OPENCV_ABI_COMPATIBILITY") == 500, "ML parser definitions drifted.");
        for (int i = 0; i < raw.Declarations.Count; i++)
        {
            Require(raw.Declarations[i].Ordinal == i && raw.Declarations[i].SourceHeader == raw.SourceHeaders[0].Path && !string.IsNullOrWhiteSpace(raw.Declarations[i].Identity), "ML parser order, source, or identity drifted at " + i);
        }
        Require(raw.Declarations.Select(x => x.Identity).Distinct(Ordinal).Count() == 241, "ML overload identities collapsed.");
        Require(classifications.SchemaVersion == 1 && classifications.UpstreamOpenCvVersion == "5.0.0" && classifications.ClaimedSlice == ClaimedSlice && classifications.ReviewStatus == "source-reviewed" && !string.IsNullOrWhiteSpace(classifications.Limitation), "ML classification identity drifted.");
        Require(!classifications.ClaimedSlice.Contains("OpenCv5Sharp", StringComparison.Ordinal) && classifications.Declarations.Count == 241, "ML fixed-major identity or row count drifted.");
        var nativeSet = new HashSet<string>(native, Ordinal);
        var managedSet = new HashSet<string>(managed, Ordinal);
        for (int i = 0; i < 241; i++)
        {
            RawDeclaration declaration = raw.Declarations[i];
            ClassificationRow row = classifications.Declarations[i];
            Require(row.Ordinal == i && row.Identity == declaration.Identity && Allowed.Contains(row.Classification, Ordinal), "ML classification order or value drifted at " + i);
            Require(!string.IsNullOrWhiteSpace(row.Reason) && row.BuildCondition == "OPENCV_CSHARP_HAS_OPENCV_ML; full-profile; mini-excluded", "ML rationale or build condition drifted at " + i);
            Require(row.NativeEntrypoints.SequenceEqual(row.NativeEntrypoints.Distinct(Ordinal).OrderBy(x => x, Ordinal)) && row.ManagedMembers.SequenceEqual(row.ManagedMembers.Distinct(Ordinal).OrderBy(x => x, Ordinal)), "Nondeterministic ML evidence ordering at " + i);
            Require(declaration.Kind == "callable" ? row.Classification != "non-callable-metadata" : row.Classification == "non-callable-metadata", "ML callable and metadata classification confused at " + i);
            if (row.Classification == "implemented")
            {
                Require(row.NativeEntrypoints.Count > 0 && row.ManagedMembers.Count > 0 && row.NativeEntrypoints.All(nativeSet.Contains) && row.ManagedMembers.All(managedSet.Contains), "False or missing ML evidence at " + i);
            }
            else if (declaration.Kind == "callable")
            {
                Require(row.NativeEntrypoints.Count == 0 && row.ManagedMembers.Count == 0, "Non-implemented ML callable carries evidence at " + i);
            }
        }
        Require(classifications.Declarations.Count(x => x.Classification == "implemented") == 173 && classifications.Declarations.Count(x => x.Classification == "missing") == 35 && classifications.Declarations.Count(x => x.Classification == "non-callable-metadata") == 33, "ML callable partition drifted.");
        Require(classifications.Declarations.Count(x => x.Classification is "intentionally-omitted" or "upstream-conditional" or "unsupported") == 0, "Unexpected ML classifications were introduced.");
        Require(Existing.All(i => classifications.Declarations[i].Classification == "implemented") && Selected.All(i => classifications.Declarations[i].Classification == "implemented"), "Existing or selected ML correlation is incomplete.");
        if (verifyFiles)
        {
            Require(options != null, "Options required for hash verification.");
            VerifyHash(options!.Workspace, raw.HeaderPath, raw.HeaderSha256, "compatibility header");
            VerifyHash(options.Workspace, raw.ParserPath, raw.ParserSha256, "parser");
            foreach (CompatibilityHeader header in raw.CompatibilityHeaders) VerifyHash(options.Workspace, header.Path, header.Sha256, "compatibility header");
            foreach (SourceHeader header in raw.SourceHeaders) VerifyHash(options.Workspace, header.Path, header.Sha256, "source header");
        }
    }

    private static string BuildMap(RawDocument raw, ClassificationDocument classifications)
    {
        var b = new StringBuilder();
        b.AppendLine("# Generated by tools/MlUpstreamMap. Do not edit.");
        b.AppendLine("schema-version=1");
        b.AppendLine("upstream-opencv-version=5.0.0");
        b.AppendLine("claimed-slice=" + ClaimedSlice);
        b.AppendLine("header-sha256=" + raw.HeaderSha256);
        b.AppendLine("parser-sha256=" + raw.ParserSha256);
        b.AppendLine("declaration-count=241");
        b.AppendLine("callable-count=208");
        b.AppendLine("class-count=13");
        b.AppendLine("enum-count=20");
        b.AppendLine("source-reviewed-extension-count=2");
        b.AppendLine("repository-wide-upstream-parity-claimed=false");
        foreach (CompatibilityHeader h in raw.CompatibilityHeaders) b.AppendLine($"compatibility-header={h.Path}|{h.Sha256}|includes={h.Includes}");
        foreach (ExcludedPublicHeader h in raw.ExcludedPublicHeaders) b.AppendLine($"excluded-public-header={h.Path}|reason={h.Reason}");
        b.AppendLine();
        b.AppendLine("ordinal|kind|source-header|classification|identity|native-entrypoints|managed-members|build-condition|reason");
        for (int i = 0; i < 241; i++)
        {
            RawDeclaration d = raw.Declarations[i];
            ClassificationRow r = classifications.Declarations[i];
            b.AppendLine($"{i}|{d.Kind}|{d.SourceHeader}|{r.Classification}|{d.Identity}|{Join(r.NativeEntrypoints)}|{Join(r.ManagedMembers)}|{r.BuildCondition}|{r.Reason}");
        }
        b.AppendLine();
        b.AppendLine("source-reviewed-extension|cv.ml.ANN_MLP.setAnnealEnergyRNG(const RNG& rng)->void|jyppx_ocv_ml_ann_mlp_set_anneal_energy_seed|OpenCvSharp.ML.ANN_MLP.SetAnnealEnergySeed(System.UInt64)|excluded-from-parser-derived-counts");
        b.AppendLine("source-reviewed-extension|cv.ml.RTrees.getOOBError()->double|jyppx_ocv_ml_rtrees_get_oob_error|OpenCvSharp.ML.RTrees.OobError|excluded-from-parser-derived-counts");
        return b.ToString().Replace("\r\n", "\n", StringComparison.Ordinal);
    }

    private static FamilyDocument BuildFamilies(RawDocument raw, ClassificationDocument classifications, string[] managed)
    {
        var annMlpFamily = new FamilyRow
        {
            Id = "ml-ann-mlp",
            Rationale = "The selected batch closes all 32 parser-emitted ANN_MLP callables with offline training, prediction, copied topology and weight matrices, typed configuration, persistence, and deterministic disposal."
        };
        foreach (int i in AnnMlpSelected.OrderBy(x => x))
        {
            annMlpFamily.Declarations.Add(new FamilyOperation
            {
                Ordinal = i,
                UpstreamIdentity = raw.Declarations[i].Identity,
                NativeEntrypoints = new(classifications.Declarations[i].NativeEntrypoints),
                ManagedMembers = new(classifications.Declarations[i].ManagedMembers)
            });
        }
        var emFamily = new FamilyRow
        {
            Id = "ml-em",
            Rationale = "The selected batch closes all 16 parser-emitted EM callables with typed configuration, automatic and explicit E/M-step training, copied parameter and covariance outputs, optional caller-owned result matrices, prediction, persistence, and deterministic disposal."
        };
        foreach (int i in EmSelected.OrderBy(x => x))
        {
            emFamily.Declarations.Add(new FamilyOperation
            {
                Ordinal = i,
                UpstreamIdentity = raw.Declarations[i].Identity,
                NativeEntrypoints = new(classifications.Declarations[i].NativeEntrypoints),
                ManagedMembers = new(classifications.Declarations[i].ManagedMembers)
            });
        }
        var treeModelsFamily = new FamilyRow
        {
            Id = "ml-tree-models",
            Rationale = "The selected batch closes all 38 parser-emitted DTrees, RTrees, and Boost callables with upstream-compatible inheritance, typed configuration, copied matrix outputs, deterministic training coverage, persistence, and disposal validation."
        };
        foreach (int i in TreeModelsSelected.OrderBy(x => x))
        {
            treeModelsFamily.Declarations.Add(new FamilyOperation
            {
                Ordinal = i,
                UpstreamIdentity = raw.Declarations[i].Identity,
                NativeEntrypoints = new(classifications.Declarations[i].NativeEntrypoints),
                ManagedMembers = new(classifications.Declarations[i].ManagedMembers)
            });
        }
        var annMlpExtension = new SourceReviewedExtension
        {
            UpstreamIdentity = "cv.ml.ANN_MLP.setAnnealEnergyRNG(const RNG& rng)->void",
            Adaptation = "The non-CV_WRAP C++ RNG parameter is represented as a stable unsigned 64-bit seed; it is intentionally excluded from parser-derived counts.",
            NativeEntrypoints = new() { "jyppx_ocv_ml_ann_mlp_set_anneal_energy_seed" },
            ManagedMembers = new() { M(managed, "ANN_MLP", " SetAnnealEnergySeed(") }
        };
        var rtreesOobExtension = new SourceReviewedExtension
        {
            UpstreamIdentity = "cv.ml.RTrees.getOOBError()->double",
            Adaptation = "OpenCV 5.0.0 keeps this public virtual method outside CV_WRAP; the binding exposes its scalar return without changing ownership or training semantics.",
            NativeEntrypoints = new() { "jyppx_ocv_ml_rtrees_get_oob_error" },
            ManagedMembers = new() { M(managed, "RTrees", "|property|", " OobError") }
        };
        return new FamilyDocument
        {
            Families = new() { annMlpFamily, emFamily, treeModelsFamily },
            SourceReviewedExtensions = new() { annMlpExtension, rtreesOobExtension }
        };
    }

    private static void RunNegativeFixtures(RawDocument raw, ClassificationDocument classifications, Options options, string[] native, string[] managed)
    {
        int passed = 0;
        void Fixture(Action<RawDocument, ClassificationDocument> mutate)
        {
            RawDocument rawClone = Clone(raw);
            ClassificationDocument classificationClone = Clone(classifications);
            mutate(rawClone, classificationClone);
            bool failed = false;
            try { Validate(rawClone, classificationClone, options, native, managed, true); }
            catch { failed = true; }
            Require(failed, "An ML negative fixture was accepted.");
            passed++;
        }
        Fixture((_, c) => c.Declarations.RemoveAt(0));
        Fixture((_, c) => c.Declarations[1].Ordinal = 0);
        Fixture((_, c) => (c.Declarations[0], c.Declarations[1]) = (c.Declarations[1], c.Declarations[0]));
        Fixture((r, _) => r.Declarations[2].Identity = r.Declarations[1].Identity);
        Fixture((_, c) => c.Declarations[168].Classification = "non-callable-metadata");
        Fixture((r, _) => r.Declarations[0].SourceHeader = "drifted/ml.hpp");
        Fixture((r, _) => r.ParserSha256 = new string('0', 64));
        Fixture((r, _) => r.HeaderSha256 = new string('0', 64));
        Fixture((_, c) => c.Declarations[168].NativeEntrypoints[0] = "jyppx_ocv_false_evidence");
        Fixture((_, c) => c.Declarations[168].ManagedMembers[0] = "MEMBER|false");
        Fixture((_, c) => c.Declarations[12].Reason = "");
        Fixture((_, c) => c.ClaimedSlice += "; OpenCv5Sharp");
        Fixture((_, c) => c.Limitation = "");
        Fixture((_, c) => c.Declarations[168].BuildCondition = "unconditional");
        Fixture((_, c) => c.Declarations[168].NativeEntrypoints.Add("jyppx_ocv_ml_ann_mlp_set_train_method"));
        Fixture((r, _) => r.SourceHeaders.RemoveAt(0));
        Fixture((r, _) => r.ExcludedPublicHeaders.RemoveAt(0));
        Require(passed == NegativeFixtureCount, "ML negative fixture count drifted.");
    }

    private static string[] N(params string[] values) => values;
    private static string[] ReadNative(string path) => File.ReadAllLines(path, Encoding.UTF8).Where(x => x.StartsWith("jyppx_ocv_", StringComparison.Ordinal)).Select(x => x.Split('|')[0]).OrderBy(x => x, Ordinal).ToArray();
    private static T Read<T>(string path) => JsonSerializer.Deserialize<T>(File.ReadAllText(path, Encoding.UTF8), JsonOptions()) ?? throw new InvalidOperationException("Could not parse " + path);
    private static T Clone<T>(T value) => JsonSerializer.Deserialize<T>(Serialize(value), JsonOptions())!;
    private static JsonSerializerOptions JsonOptions() => new() { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };
    private static string Serialize<T>(T value) => JsonSerializer.Serialize(value, JsonOptions()).Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\r", "\n", StringComparison.Ordinal) + "\n";
    private static void WriteOrCheck(string path, string content, bool check)
    {
        content = content.Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\r", "\n", StringComparison.Ordinal);
        if (check)
        {
            Require(File.Exists(path) && File.ReadAllText(path, Encoding.UTF8).Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\r", "\n", StringComparison.Ordinal) == content, "Generated file is missing or stale: " + path);
            return;
        }
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content, new UTF8Encoding(false));
    }
    private static void VerifyHash(string workspace, string relative, string expected, string label)
    {
        string path = Path.Combine(workspace, relative.Replace('/', Path.DirectorySeparatorChar));
        Require(File.Exists(path), "ML " + label + " missing: " + relative);
        Require(Sha256File(path) == expected, "ML " + label + " hash drifted: " + relative);
    }
    private static string Join(List<string> values) => values.Count == 0 ? "-" : string.Join(";", values.Select(x => x.Replace("|", "<pipe>", StringComparison.Ordinal)));
    private static string Rel(string root, string path) => Path.GetRelativePath(root, path).Replace('\\', '/');
    private static string Sha256(string text) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
    private static string Sha256File(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}

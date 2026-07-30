using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

internal static class Program
{
    private static readonly StringComparer Ordinal = StringComparer.Ordinal;
    private static readonly int[] OmittedCallables =
    {
        2, 7, 12, 13, 14, 17,
        113, 114, 115, 116, 117, 118, 119, 120, 121,
        132, 133,
        148, 149, 150, 154,
        162, 163, 164, 165,
        169
    };
    private static readonly HashSet<int> Omitted = new(OmittedCallables);
    private static readonly HashSet<int> ModelBacked = new(
        Enumerable.Range(113, 9).Concat(new[] { 132, 133 }).Concat(Enumerable.Range(162, 4)));
    private static readonly HashSet<int> Selected = new(Enumerable.Range(171, 12));
    private static readonly string[] AllowedClassifications =
    {
        "implemented", "missing", "intentionally-omitted", "upstream-conditional", "unsupported", "non-callable-metadata"
    };
    private const string ClaimedSlice = "opencv2/features2d.hpp and opencv2/features/features.hpp compatibility headers implemented by parser-emitted opencv2/features.hpp declarations";

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
        string ExtensionOutput,
        bool Initialize,
        bool Check);

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
        public List<SourceHeader> SourceHeaders { get; set; } = new();
        public int DeclarationCount { get; set; }
        public List<RawDeclaration> Declarations { get; set; } = new();
    }

    private sealed class CompatibilityHeader
    {
        public string Path { get; set; } = "";
        public string Sha256 { get; set; } = "";
        public string Includes { get; set; } = "";
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
        public string Limitation { get; set; } = "The map covers the main OpenCV 5 Features compatibility header closure only. Optional xfeatures2d and source-reviewed non-parser declarations remain separate, and repository-wide parity is not claimed.";
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
        public string Generator { get; init; } = "tools/FeaturesUpstreamMap";
        public string UpstreamOpenCvVersion { get; init; } = "5.0.0";
        public string ClaimedSlice { get; init; } = Program.ClaimedSlice;
        public string RawExtractionPath { get; init; } = "";
        public string ClassificationPath { get; init; } = "";
        public string MappingPath { get; init; } = "";
        public string HeaderSha256 { get; init; } = "";
        public string ParserSha256 { get; init; } = "";
        public int CompatibilityHeaderCount { get; init; }
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
        public string SourceReviewedExtensionPath { get; init; } = "";
        public string SourceReviewedExtensionSha256 { get; init; } = "";
        public int SourceReviewedExtensionDeclarationCount { get; init; }
        public int SelectedFamilyCount { get; init; }
        public int SelectedDeclarationCount { get; init; }
        public int ManagedPublicTypeAdditionCount { get; init; } = 2;
        public int ManagedPublicMemberAdditionCount { get; init; } = 18;
        public bool RepositoryWideUpstreamParityClaimed { get; init; }
    }

    private sealed class FamilyDocument
    {
        public int SchemaVersion { get; init; } = 1;
        public string UpstreamOpenCvVersion { get; init; } = "5.0.0";
        public string Status { get; init; } = "implemented-verified";
        public int ManagedPublicTypeAdditionCount { get; init; } = 2;
        public int ManagedPublicMemberAdditionCount { get; init; } = 18;
        public List<FamilyRow> Families { get; init; } = new();
    }

    private sealed class FamilyRow
    {
        public string Id { get; init; } = "features-ann-index";
        public string Rationale { get; init; } = "The complete ANNIndex family is offline-safe, deterministic with a fixed seed, and adds the largest adjacent unimplemented main-module object family without external model assets.";
        public List<FamilyOperation> Declarations { get; init; } = new();
    }

    private sealed class FamilyOperation
    {
        public int Ordinal { get; init; }
        public string UpstreamIdentity { get; init; } = "";
        public string UpstreamClassification { get; init; } = "";
        public List<string> NativeEntrypoints { get; init; } = new();
        public List<string> ManagedMembers { get; init; } = new();
        public string FocusedTest { get; init; } = "tests/OpenCvSharp.Tests/Features2D/ANNIndexTests.cs";
        public string NativeSmoke { get; init; } = "src/OpenCvSharp.Native/tests/native_smoke.cpp";
        public string Sample { get; init; } = "samples/ConsoleSamples/Program.cs";
        public string Guide { get; init; } = "docs/articles/features-upstream-parity-guide.md";
    }

    private sealed class ExtensionDocument
    {
        public int SchemaVersion { get; init; } = 1;
        public string UpstreamOpenCvVersion { get; init; } = "5.0.0";
        public string SourceHeader { get; init; } = "";
        public string SourceHeaderSha256 { get; init; } = "";
        public bool ParserDerived { get; init; }
        public string ReviewStatus { get; init; } = "source-reviewed-not-implemented";
        public string Reason { get; init; } = "KeyPointsFilter is public C++ source but lacks wrapper annotations, so the official hdr_parser.py does not emit it. It remains outside parser-derived counts and is deferred as a separate collection-mutation extension.";
        public List<string> Declarations { get; init; } = new();
    }

    private static int Main(string[] args)
    {
        try
        {
            Options options = Parse(args);
            RawDocument raw = Read<RawDocument>(options.Raw);
            string[] native = ReadNative(options.NativeManifest);
            string[] managed = File.ReadAllLines(options.ManagedBaseline, Encoding.UTF8);
            if (options.Initialize)
            {
                WriteOrCheck(options.Classification, Serialize(Initialize(raw, native, managed)), false);
            }

            ClassificationDocument classifications = Read<ClassificationDocument>(options.Classification);
            Validate(raw, classifications, options, native, managed, true);
            string mapping = BuildMap(raw, classifications);
            FamilyDocument families = BuildFamilies(raw, classifications);
            string familyText = Serialize(families);
            ExtensionDocument extensions = BuildExtensions(raw);
            string extensionText = Serialize(extensions);
            var counts = new SortedDictionary<string, int>(Ordinal);
            foreach (string value in AllowedClassifications)
            {
                counts[value] = classifications.Declarations.Count(row => row.Classification == value);
            }

            var summary = new SummaryDocument
            {
                RawExtractionPath = Rel(options.Repository, options.Raw),
                ClassificationPath = Rel(options.Repository, options.Classification),
                MappingPath = Rel(options.Repository, options.Output),
                HeaderSha256 = raw.HeaderSha256,
                ParserSha256 = raw.ParserSha256,
                CompatibilityHeaderCount = raw.CompatibilityHeaders.Count,
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
                SourceReviewedExtensionPath = Rel(options.Repository, options.ExtensionOutput),
                SourceReviewedExtensionSha256 = Sha256(extensionText),
                SourceReviewedExtensionDeclarationCount = extensions.Declarations.Count,
                SelectedFamilyCount = families.Families.Count,
                SelectedDeclarationCount = families.Families.Sum(value => value.Declarations.Count),
                RepositoryWideUpstreamParityClaimed = false
            };

            RunNegativeFixtures(raw, classifications, options, native, managed);
            WriteOrCheck(options.Output, mapping, options.Check);
            WriteOrCheck(options.FamilyOutput, familyText, options.Check);
            WriteOrCheck(options.ExtensionOutput, extensionText, options.Check);
            WriteOrCheck(options.Summary, Serialize(summary), options.Check);
            Console.WriteLine($"FEATURES_UPSTREAM_MAP_OK declarations={summary.DeclarationCount} callables={summary.CallableCount} implemented={counts["implemented"]} missing={counts["missing"]} omitted={counts["intentionally-omitted"]} fixtures=15 sha256={summary.MappingSha256} mode={(options.Check ? "check" : "write")}");
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
        for (int index = 0; index < args.Length; index++)
        {
            if (args[index] == "--initialize-classification") initialize = true;
            else if (args[index] == "--check") check = true;
            else
            {
                Require(index + 1 < args.Length, "Missing option value: " + args[index]);
                values[args[index]] = args[++index];
            }
        }

        string Value(string name)
        {
            Require(values.TryGetValue(name, out string? value), "Missing option: " + name);
            return Path.GetFullPath(value!);
        }

        return new Options(
            Value("--repository"), Value("--workspace"), Value("--raw"), Value("--classification"),
            Value("--native-manifest"), Value("--managed-baseline"), Value("--output"), Value("--summary"),
            Value("--family-output"), Value("--extension-output"), initialize, check);
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
                BuildCondition = BuildCondition(declaration.Ordinal)
            };
            if (declaration.Kind != "callable")
            {
                row.Classification = "non-callable-metadata";
                row.Reason = "Parser-emitted class or enum metadata is reviewed as public type shape rather than an independently callable ABI operation.";
                if (declaration.Ordinal == 171)
                    row.ManagedMembers.Add(FindManaged(managed, "TYPE|class|public;sealed|OpenCvSharp.Features2D.ANNIndex|"));
                else if (declaration.Ordinal == 172)
                    row.ManagedMembers.Add(FindManaged(managed, "TYPE|enum|public|OpenCvSharp.Features2D.ANNIndexDistance|"));
            }
            else if (Omitted.Contains(declaration.Ordinal))
            {
                row.Classification = "intentionally-omitted";
                row.Reason = OmissionReason(declaration.Ordinal);
            }
            else
            {
                row.Classification = "implemented";
                row.Reason = declaration.Ordinal >= 173
                    ? "The selected ANNIndex family has a version-neutral opaque C ABI, SafeHandle ownership, caller-owned Mat outputs, strict validation, UTF-8 persistence, native smoke, and net8/net10 tests."
                    : "The existing version-neutral native and managed Features2D surface provides the callable semantics represented by this parser row.";
                row.NativeEntrypoints.AddRange(NativeEvidence(declaration));
                row.ManagedMembers.Add(ManagedEvidence(declaration, managed));
            }
            row.NativeEntrypoints = row.NativeEntrypoints.Distinct(Ordinal).OrderBy(value => value, Ordinal).ToList();
            row.ManagedMembers = row.ManagedMembers.Distinct(Ordinal).OrderBy(value => value, Ordinal).ToList();
            result.Declarations.Add(row);
        }

        Validate(raw, result, null, native, managed, false);
        return result;
    }

    private static string BuildCondition(int ordinal)
    {
        if (ModelBacked.Contains(ordinal)) return "HAVE_OPENCV_DNN=1; external-model-evidence-required; full-profile";
        if (ordinal >= 158 && ordinal <= 160) return "HAVE_OPENCV_FLANN=1; OPENCV_CSHARP_HAS_OPENCV_FEATURES2D; full-profile";
        if (ordinal <= 2) return "OPENCV_CSHARP_HAS_OPENCV_FEATURES; full-profile; mini-returns-NOT_LINKED";
        return "OPENCV_CSHARP_HAS_OPENCV_FEATURES2D; full-profile";
    }

    private static string OmissionReason(int ordinal)
    {
        if (ordinal == 2) return "The cornersQuality output overload is not exposed by the current managed point-array result and is deferred until a paired quality result model is added.";
        if (ordinal == 7) return "Batch compute mutates nested keypoint collections and returns multiple descriptor matrices; it is deferred to a dedicated nested ownership result model while single-image compute remains implemented.";
        if (ordinal is 12 or 13 or 14 or 17 or 148 or 149 or 150 or 154) return "Polymorphic Algorithm persistence is deferred until every concrete Feature2D or DescriptorMatcher handle can safely participate in one FileStorage and FileNode ownership contract.";
        if (ModelBacked.Contains(ordinal)) return "DISK, ALIKED, and LightGlue require external DNN model artifacts and model-specific shape evidence; they are kept outside the offline deterministic success path.";
        if (ordinal == 169) return "The matchesThickness drawing overload is a convenience variant not present in the current managed drawing contract; the standard flat and nested match overloads remain implemented.";
        throw new InvalidOperationException("No omission reason for ordinal " + ordinal);
    }

    private static List<string> NativeEvidence(RawDeclaration declaration)
    {
        int ordinal = declaration.Ordinal;
        string method = declaration.Name.Split('.').Last();
        if (ordinal is 0 or 1) return Names("jyppx_ocv_imgproc_good_features_to_track_count", "jyppx_ocv_imgproc_good_features_to_track_fill");
        if (ordinal is 4 or 5) return Names("jyppx_ocv_features2d_orb_detect_count", "jyppx_ocv_features2d_orb_detect_fill");
        if (ordinal == 6) return Names("jyppx_ocv_features2d_orb_compute");
        if (ordinal == 8) return Names("jyppx_ocv_features2d_orb_detect_and_compute_count", "jyppx_ocv_features2d_orb_detect_and_compute_fill");
        if (ordinal is >= 9 and <= 16) return FeatureMetadataNative("orb", method);
        if (ordinal is >= 19 and <= 22)
        {
            return method switch
            {
                "create" => Names("jyppx_ocv_features2d_affine_create_from_orb"),
                "setViewParams" => Names("jyppx_ocv_features2d_affine_set_view_params"),
                "getViewParams" => Names("jyppx_ocv_features2d_affine_get_view_params_count", "jyppx_ocv_features2d_affine_get_view_params_fill"),
                "getDefaultName" => Names("jyppx_ocv_features2d_affine_default_name_length", "jyppx_ocv_features2d_affine_default_name_fill"),
                _ => throw new InvalidOperationException(declaration.Identity)
            };
        }
        if (ordinal is >= 24 and <= 36) return StandardFeatureNative("sift", method, new Dictionary<string, string>(Ordinal) { ["NFeatures"] = "nfeatures", ["NOctaveLayers"] = "n_octave_layers", ["ContrastThreshold"] = "contrast_threshold", ["EdgeThreshold"] = "edge_threshold", ["Sigma"] = "sigma" });
        if (ordinal is >= 39 and <= 58) return StandardFeatureNative("orb", method, new Dictionary<string, string>(Ordinal) { ["MaxFeatures"] = "max_features", ["ScaleFactor"] = "scale_factor", ["NLevels"] = "nlevels", ["EdgeThreshold"] = "edge_threshold", ["FirstLevel"] = "first_level", ["WTA_K"] = "wta_k", ["ScoreType"] = "score_type", ["PatchSize"] = "patch_size", ["FastThreshold"] = "fast_threshold" });
        if (ordinal is >= 60 and <= 82)
        {
            if (method == "detectRegions") return Names("jyppx_ocv_features2d_mser_detect_regions_count", "jyppx_ocv_features2d_mser_detect_regions_fill");
            return StandardFeatureNative("mser", method, new Dictionary<string, string>(Ordinal) { ["Delta"] = "delta", ["MinArea"] = "min_area", ["MaxArea"] = "max_area", ["MaxVariation"] = "max_variation", ["MinDiversity"] = "min_diversity", ["MaxEvolution"] = "max_evolution", ["AreaThreshold"] = "area_threshold", ["MinMargin"] = "min_margin", ["EdgeBlurSize"] = "edge_blur_size", ["Pass2Only"] = "pass2_only" });
        }
        if (ordinal is >= 86 and <= 93) return StandardFeatureNative("fast", method, new Dictionary<string, string>(Ordinal) { ["Threshold"] = "threshold", ["NonmaxSuppression"] = "nonmax_suppression", ["Type"] = "type" });
        if (ordinal is >= 95 and <= 111) return StandardFeatureNative("gftt", method, new Dictionary<string, string>(Ordinal) { ["MaxFeatures"] = "max_features", ["QualityLevel"] = "quality_level", ["MinDistance"] = "min_distance", ["BlockSize"] = "block_size", ["GradientSize"] = "gradient_size", ["HarrisDetector"] = "harris_detector", ["K"] = "k" });
        if (ordinal is >= 124 and <= 129)
        {
            return ordinal switch
            {
                124 => Names("jyppx_ocv_features2d_simple_blob_create_default"),
                125 => Names("jyppx_ocv_features2d_simple_blob_create", "jyppx_ocv_features2d_simple_blob_create_default"),
                126 => Names("jyppx_ocv_features2d_simple_blob_set_params"),
                127 => Names("jyppx_ocv_features2d_simple_blob_get_params"),
                128 => Names("jyppx_ocv_features2d_simple_blob_default_name_length", "jyppx_ocv_features2d_simple_blob_default_name_fill"),
                129 => Names("jyppx_ocv_features2d_simple_blob_get_blob_contours_count", "jyppx_ocv_features2d_simple_blob_get_blob_contours_fill"),
                _ => throw new InvalidOperationException(declaration.Identity)
            };
        }
        if (ordinal is >= 136 and <= 153)
        {
            return ordinal switch
            {
                136 => Names("jyppx_ocv_features2d_descriptor_matcher_add"),
                137 => Names("jyppx_ocv_features2d_descriptor_matcher_get_train_descriptors_count", "jyppx_ocv_features2d_descriptor_matcher_get_train_descriptor_clone"),
                138 => Names("jyppx_ocv_features2d_descriptor_matcher_clear"),
                139 => Names("jyppx_ocv_features2d_descriptor_matcher_empty"),
                140 => Names("jyppx_ocv_features2d_descriptor_matcher_is_mask_supported"),
                141 => Names("jyppx_ocv_features2d_descriptor_matcher_train"),
                142 => Names("jyppx_ocv_features2d_descriptor_matcher_match_count", "jyppx_ocv_features2d_descriptor_matcher_match_fill"),
                143 => Names("jyppx_ocv_features2d_descriptor_matcher_knn_match_count", "jyppx_ocv_features2d_descriptor_matcher_knn_match_fill"),
                144 => Names("jyppx_ocv_features2d_descriptor_matcher_radius_match_count", "jyppx_ocv_features2d_descriptor_matcher_radius_match_fill"),
                145 => Names("jyppx_ocv_features2d_descriptor_matcher_match_train_count", "jyppx_ocv_features2d_descriptor_matcher_match_train_fill"),
                146 => Names("jyppx_ocv_features2d_descriptor_matcher_knn_match_train_count", "jyppx_ocv_features2d_descriptor_matcher_knn_match_train_fill"),
                147 => Names("jyppx_ocv_features2d_descriptor_matcher_radius_match_train_count", "jyppx_ocv_features2d_descriptor_matcher_radius_match_train_fill"),
                151 => Names("jyppx_ocv_features2d_descriptor_matcher_clone"),
                152 => Names("jyppx_ocv_features2d_descriptor_matcher_create_by_name"),
                153 => Names("jyppx_ocv_features2d_descriptor_matcher_create_by_type"),
                _ => throw new InvalidOperationException(declaration.Identity)
            };
        }
        if (ordinal is 156 or 157) return Names("jyppx_ocv_features2d_bf_matcher_create");
        if (ordinal is 159 or 160) return Names("jyppx_ocv_features2d_flann_matcher_create");
        if (ordinal == 167) return Names("jyppx_ocv_features2d_draw_keypoints");
        if (ordinal == 168) return Names("jyppx_ocv_features2d_draw_matches");
        if (ordinal == 170) return Names("jyppx_ocv_features2d_draw_matches_knn");
        if (ordinal is >= 173 and <= 182)
        {
            string suffix = ordinal switch
            {
                173 => "add_items", 174 => "build", 175 => "knn_search", 176 => "save", 177 => "load",
                178 => "get_tree_number", 179 => "get_item_number", 180 => "set_on_disk_build", 181 => "set_seed", 182 => "create",
                _ => throw new InvalidOperationException(declaration.Identity)
            };
            return Names("jyppx_ocv_features2d_ann_index_" + suffix);
        }
        throw new InvalidOperationException("No native evidence rule for " + declaration.Identity);
    }

    private static List<string> FeatureMetadataNative(string prefix, string method)
    {
        return method switch
        {
            "descriptorSize" => Names($"jyppx_ocv_features2d_{prefix}_descriptor_size"),
            "descriptorType" => Names($"jyppx_ocv_features2d_{prefix}_descriptor_type"),
            "defaultNorm" => Names($"jyppx_ocv_features2d_{prefix}_default_norm"),
            "empty" => Names($"jyppx_ocv_features2d_{prefix}_empty"),
            "getDefaultName" => Names($"jyppx_ocv_features2d_{prefix}_default_name_length", $"jyppx_ocv_features2d_{prefix}_default_name_fill"),
            _ => throw new InvalidOperationException("No feature metadata evidence rule for " + method)
        };
    }

    private static List<string> StandardFeatureNative(string prefix, string method, Dictionary<string, string> properties)
    {
        if (method == "create") return Names($"jyppx_ocv_features2d_{prefix}_create");
        if (method == "getDefaultName") return Names($"jyppx_ocv_features2d_{prefix}_default_name_length", $"jyppx_ocv_features2d_{prefix}_default_name_fill");
        if (method.StartsWith("get", StringComparison.Ordinal) || method.StartsWith("set", StringComparison.Ordinal))
        {
            string direction = method.Substring(0, 3);
            string property = method.Substring(3);
            Require(properties.TryGetValue(property, out string? suffix), $"Unknown {prefix} property {property}");
            return Names($"jyppx_ocv_features2d_{prefix}_{direction}_{suffix}");
        }
        throw new InvalidOperationException($"No standard feature evidence for {prefix}.{method}");
    }

    private static string ManagedEvidence(RawDeclaration declaration, string[] managed)
    {
        int ordinal = declaration.Ordinal;
        string method = declaration.Name.Split('.').Last();
        if (ordinal is 0 or 1) return FindManaged(managed, "MEMBER|OpenCvSharp.ImgProc.Cv2|", " GoodFeaturesToTrack(");
        if (ordinal == 4) return FindManaged(managed, "MEMBER|OpenCvSharp.Features2D.Feature2D|", " Detect(OpenCvSharp.Core.Mat image");
        if (ordinal == 5) return FindManaged(managed, "MEMBER|OpenCvSharp.Features2D.Feature2D|", " Detect(OpenCvSharp.Core.Mat[] images,OpenCvSharp.Core.Mat[]? masks)");
        if (ordinal == 6) return FindManaged(managed, "MEMBER|OpenCvSharp.Features2D.ORB|", " Compute(OpenCvSharp.Core.Mat image,ref ");
        if (ordinal == 8) return FindManaged(managed, "MEMBER|OpenCvSharp.Features2D.ORB|", " DetectAndCompute(OpenCvSharp.Core.Mat image,");
        if (ordinal is >= 9 and <= 16) return FindFeatureManaged(managed, "Feature2D", method, new());
        if (ordinal is >= 19 and <= 22) return FindFeatureManaged(managed, "AffineFeature", method, new());
        if (ordinal is >= 24 and <= 36) return FindFeatureManaged(managed, "SIFT", method, new Dictionary<string, string>(Ordinal) { ["NFeatures"] = "NFeatures", ["NOctaveLayers"] = "NOctaveLayers", ["ContrastThreshold"] = "ContrastThreshold", ["EdgeThreshold"] = "EdgeThreshold", ["Sigma"] = "Sigma" });
        if (ordinal is >= 39 and <= 58) return FindFeatureManaged(managed, "ORB", method, new Dictionary<string, string>(Ordinal) { ["MaxFeatures"] = "MaxFeatures", ["ScaleFactor"] = "ScaleFactor", ["NLevels"] = "NLevels", ["EdgeThreshold"] = "EdgeThreshold", ["FirstLevel"] = "FirstLevel", ["WTA_K"] = "WtaK", ["ScoreType"] = "ScoreType", ["PatchSize"] = "PatchSize", ["FastThreshold"] = "FastThreshold" });
        if (ordinal is >= 60 and <= 82) return FindFeatureManaged(managed, "MSER", method, new Dictionary<string, string>(Ordinal) { ["Delta"] = "Delta", ["MinArea"] = "MinArea", ["MaxArea"] = "MaxArea", ["MaxVariation"] = "MaxVariation", ["MinDiversity"] = "MinDiversity", ["MaxEvolution"] = "MaxEvolution", ["AreaThreshold"] = "AreaThreshold", ["MinMargin"] = "MinMargin", ["EdgeBlurSize"] = "EdgeBlurSize", ["Pass2Only"] = "Pass2Only" });
        if (ordinal is >= 86 and <= 93) return FindFeatureManaged(managed, "FastFeatureDetector", method, new Dictionary<string, string>(Ordinal) { ["Threshold"] = "Threshold", ["NonmaxSuppression"] = "NonmaxSuppression", ["Type"] = "Type" });
        if (ordinal is >= 95 and <= 111) return FindFeatureManaged(managed, "GFTTDetector", method, new Dictionary<string, string>(Ordinal) { ["MaxFeatures"] = "MaxFeatures", ["QualityLevel"] = "QualityLevel", ["MinDistance"] = "MinDistance", ["BlockSize"] = "BlockSize", ["GradientSize"] = "GradientSize", ["HarrisDetector"] = "HarrisDetector", ["K"] = "K" });
        if (ordinal == 124) return FindManaged(managed, "MEMBER|OpenCvSharp.Features2D.SimpleBlobDetectorParams|constructor|");
        if (ordinal == 125) return FindManaged(managed, "MEMBER|OpenCvSharp.Features2D.SimpleBlobDetector|", " Create(OpenCvSharp.Features2D.SimpleBlobDetectorParams parameters)");
        if (ordinal is 126 or 127) return FindManaged(managed, "MEMBER|OpenCvSharp.Features2D.SimpleBlobDetector|", "|property|", " Parameters");
        if (ordinal == 128) return FindManaged(managed, "MEMBER|OpenCvSharp.Features2D.SimpleBlobDetector|", "|property|", " DefaultName");
        if (ordinal == 129) return FindManaged(managed, "MEMBER|OpenCvSharp.Features2D.SimpleBlobDetector|", " GetBlobContours(");
        if (ordinal is >= 136 and <= 147)
        {
            if (ordinal == 139) return FindManaged(managed, "MEMBER|OpenCvSharp.Features2D.DescriptorMatcher|", "|property|", " Empty");
            if (ordinal == 140) return FindManaged(managed, "MEMBER|OpenCvSharp.Features2D.DescriptorMatcher|", "|property|", " IsMaskSupported");
            string managedMethod = method switch { "knnMatch" => "KnnMatch", "radiusMatch" => "RadiusMatch", _ => char.ToUpperInvariant(method[0]) + method.Substring(1) };
            return FindManaged(managed, "MEMBER|OpenCvSharp.Features2D.DescriptorMatcher|", " " + managedMethod + "(");
        }
        if (ordinal == 151) return FindManaged(managed, "MEMBER|OpenCvSharp.Features2D.DescriptorMatcher|", " Clone(");
        if (ordinal == 152) return FindManaged(managed, "MEMBER|OpenCvSharp.Features2D.DescriptorMatcher|", " Create(System.String matcherName)");
        if (ordinal == 153) return FindManaged(managed, "MEMBER|OpenCvSharp.Features2D.DescriptorMatcher|", " Create(OpenCvSharp.Features2D.DescriptorMatcherType matcherType)");
        if (ordinal is 156 or 157) return FindManaged(managed, "MEMBER|OpenCvSharp.Features2D.BFMatcher|", " Create(");
        if (ordinal is 159 or 160) return FindManaged(managed, "MEMBER|OpenCvSharp.Features2D.FlannBasedMatcher|", " Create()");
        if (ordinal == 167) return FindManaged(managed, "MEMBER|OpenCvSharp.Features2D.Cv2|", " DrawKeypoints(");
        if (ordinal == 168) return FindManaged(managed, "MEMBER|OpenCvSharp.Features2D.Cv2|", " DrawMatches(");
        if (ordinal == 170) return FindManaged(managed, "MEMBER|OpenCvSharp.Features2D.Cv2|", " DrawMatchesKnn(");
        if (ordinal is >= 173 and <= 182)
        {
            string token = ordinal switch
            {
                173 => " AddItems(", 174 => " Build(", 175 => " KnnSearch(", 176 => " Save(", 177 => " Load(",
                178 => " TreeNumber", 179 => " ItemNumber", 180 => " SetOnDiskBuild(", 181 => " SetSeed(", 182 => " Create(",
                _ => throw new InvalidOperationException(declaration.Identity)
            };
            return FindManaged(managed, "MEMBER|OpenCvSharp.Features2D.ANNIndex|", token);
        }
        throw new InvalidOperationException("No managed evidence rule for " + declaration.Identity);
    }

    private static string FindFeatureManaged(string[] managed, string owner, string method, Dictionary<string, string> properties)
    {
        string prefix = $"MEMBER|OpenCvSharp.Features2D.{owner}|";
        if (method == "create") return FindManaged(managed, prefix, " Create(");
        if (method == "detectRegions") return FindManaged(managed, prefix, " DetectRegions(");
        if (method == "setViewParams") return FindManaged(managed, prefix, " SetViewParams(");
        if (method == "getViewParams") return FindManaged(managed, prefix, " GetViewParams(");
        if (method == "getDefaultName") return FindManaged(managed, prefix, "|property|", " DefaultName");
        if (method == "descriptorSize") return FindManaged(managed, prefix, "|property|", " DescriptorSize");
        if (method == "descriptorType") return FindManaged(managed, prefix, "|property|", " DescriptorType");
        if (method == "defaultNorm") return FindManaged(managed, prefix, "|property|", " DefaultNorm");
        if (method == "empty") return FindManaged(managed, prefix, "|property|", " Empty");
        if (method.StartsWith("get", StringComparison.Ordinal) || method.StartsWith("set", StringComparison.Ordinal))
        {
            string property = method.Substring(3);
            Require(properties.TryGetValue(property, out string? managedProperty), $"Unknown managed {owner} property {property}");
            return FindManaged(managed, prefix, "|property|", " " + managedProperty);
        }
        throw new InvalidOperationException($"No managed feature evidence for {owner}.{method}");
    }

    private static string FindManaged(string[] managed, params string[] tokens)
    {
        string[] matches = managed.Where(line => tokens.All(token => line.Contains(token, StringComparison.Ordinal))).OrderBy(line => line, Ordinal).ToArray();
        Require(matches.Length > 0, "Managed evidence was not found: " + string.Join(" + ", tokens));
        return matches[0];
    }

    private static List<string> Names(params string[] values) => values.ToList();

    private static void Validate(RawDocument raw, ClassificationDocument classifications, Options? options, string[] native, string[] managed, bool verifyFiles)
    {
        Require(raw.SchemaVersion == 1 && raw.Generator == "tools/FeaturesUpstreamMap/extract_features.py" && raw.UpstreamOpenCvVersion == "5.0.0", "Features raw identity drifted.");
        Require(raw.HeaderPath.EndsWith("modules/features/include/opencv2/features2d.hpp", StringComparison.Ordinal), "Features compatibility header path drifted.");
        Require(raw.DeclarationCount == 183 && raw.Declarations.Count == 183, "Features declaration count drifted.");
        Require(raw.Declarations.Count(value => value.Kind == "callable") == 160, "Features callable count drifted.");
        Require(raw.Declarations.Count(value => value.Kind == "class") == 17, "Features class count drifted.");
        Require(raw.Declarations.Count(value => value.Kind == "enum") == 6, "Features enum count drifted.");
        Require(raw.SourceHeaders.Count == 1 && raw.SourceHeaders[0].StartOrdinal == 0 && raw.SourceHeaders[0].DeclarationCount == 183, "Features source-header closure drifted.");
        Require(raw.CompatibilityHeaders.Count == 2, "Features compatibility-header count drifted.");
        Require(raw.CompatibilityHeaders[0].Path.EndsWith("opencv2/features2d.hpp", StringComparison.Ordinal) && raw.CompatibilityHeaders[1].Path.EndsWith("opencv2/features/features.hpp", StringComparison.Ordinal), "Features compatibility-header order drifted.");
        Require(raw.CompatibilityHeaders.All(value => value.Includes == "opencv2/features.hpp"), "Features compatibility forwarding target drifted.");
        Require(raw.PreprocessorDefinitions.Count == 4 && raw.PreprocessorDefinitions.GetValueOrDefault("CV_VERSION_MAJOR") == 5 && raw.PreprocessorDefinitions.GetValueOrDefault("OPENCV_ABI_COMPATIBILITY") == 500 && raw.PreprocessorDefinitions.GetValueOrDefault("HAVE_OPENCV_FLANN") == 1 && raw.PreprocessorDefinitions.GetValueOrDefault("HAVE_OPENCV_DNN") == 1, "Features parser definitions drifted.");

        for (int index = 0; index < raw.Declarations.Count; index++)
        {
            RawDeclaration declaration = raw.Declarations[index];
            Require(declaration.Ordinal == index, "Features parser order drifted at ordinal " + index);
            Require(declaration.SourceHeader == raw.SourceHeaders[0].Path, "Features declaration source-header drifted at ordinal " + index);
            Require(!string.IsNullOrWhiteSpace(declaration.Identity), "Features identity is empty at ordinal " + index);
        }
        Require(raw.Declarations.Select(value => value.Identity).Distinct(Ordinal).Count() == 183, "Features parser identities are duplicated or overloads collapsed.");

        Require(classifications.SchemaVersion == 1 && classifications.UpstreamOpenCvVersion == "5.0.0" && classifications.ClaimedSlice == ClaimedSlice && classifications.ReviewStatus == "reviewed", "Features classification identity drifted.");
        Require(!classifications.ClaimedSlice.Contains("OpenCv5Sharp", StringComparison.Ordinal) && !classifications.ClaimedSlice.Contains("jyppx_ocv5_", StringComparison.Ordinal), "Features classification uses a fixed-major identity.");
        Require(!string.IsNullOrWhiteSpace(classifications.Limitation), "Features classification limitation is missing.");
        Require(classifications.Declarations.Count == raw.Declarations.Count, "Features classification row count drifted.");

        var nativeSet = new HashSet<string>(native, Ordinal);
        var managedSet = new HashSet<string>(managed, Ordinal);
        for (int index = 0; index < classifications.Declarations.Count; index++)
        {
            RawDeclaration declaration = raw.Declarations[index];
            ClassificationRow row = classifications.Declarations[index];
            Require(row.Ordinal == index && row.Identity == declaration.Identity, "Features classification order or identity drifted at ordinal " + index);
            Require(AllowedClassifications.Contains(row.Classification, Ordinal), "Unknown Features classification at ordinal " + index);
            Require(!string.IsNullOrWhiteSpace(row.Reason) && !string.IsNullOrWhiteSpace(row.BuildCondition), "Undocumented Features classification at ordinal " + index);
            Require(row.NativeEntrypoints.SequenceEqual(row.NativeEntrypoints.Distinct(Ordinal).OrderBy(value => value, Ordinal)), "Nondeterministic native evidence ordering at ordinal " + index);
            Require(row.ManagedMembers.SequenceEqual(row.ManagedMembers.Distinct(Ordinal).OrderBy(value => value, Ordinal)), "Nondeterministic managed evidence ordering at ordinal " + index);
            if (declaration.Kind == "callable")
            {
                Require(row.Classification != "non-callable-metadata", "Callable classified as metadata at ordinal " + index);
            }
            else
            {
                Require(row.Classification == "non-callable-metadata", "Metadata classified as callable at ordinal " + index);
            }
            if (row.Classification == "implemented")
            {
                Require(row.NativeEntrypoints.Count > 0 && row.ManagedMembers.Count > 0, "Implemented Features row lacks evidence at ordinal " + index);
                Require(row.NativeEntrypoints.All(nativeSet.Contains), "False native evidence at ordinal " + index);
                Require(row.ManagedMembers.All(managedSet.Contains), "False managed evidence at ordinal " + index);
            }
            else if (declaration.Kind == "callable")
            {
                Require(row.NativeEntrypoints.Count == 0 && row.ManagedMembers.Count == 0, "Non-implemented callable carries implementation evidence at ordinal " + index);
            }
            if (ModelBacked.Contains(index))
            {
                Require(row.Classification == "intentionally-omitted" && row.BuildCondition.Contains("HAVE_OPENCV_DNN=1", StringComparison.Ordinal) && row.BuildCondition.Contains("external-model-evidence-required", StringComparison.Ordinal), "Model-backed Features row is misclassified at ordinal " + index);
            }
            if (index is 159 or 160)
            {
                Require(row.BuildCondition.Contains("HAVE_OPENCV_FLANN=1", StringComparison.Ordinal), "FLANN build condition is missing at ordinal " + index);
            }
        }
        Require(classifications.Declarations.All(row => row.Classification != "missing"), "Features classification contains unexplained missing rows.");
        Require(Selected.All(index => classifications.Declarations[index].Classification == (index <= 172 ? "non-callable-metadata" : "implemented")), "Selected ANNIndex family is incomplete.");

        if (verifyFiles)
        {
            Require(options != null, "Options are required for source verification.");
            VerifyHash(options!.Workspace, raw.HeaderPath, raw.HeaderSha256, "compatibility header");
            VerifyHash(options.Workspace, raw.ParserPath, raw.ParserSha256, "parser");
            foreach (CompatibilityHeader header in raw.CompatibilityHeaders) VerifyHash(options.Workspace, header.Path, header.Sha256, "compatibility header");
            foreach (SourceHeader header in raw.SourceHeaders) VerifyHash(options.Workspace, header.Path, header.Sha256, "source header");
        }
    }

    private static string BuildMap(RawDocument raw, ClassificationDocument classifications)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Generated by tools/FeaturesUpstreamMap. Do not edit.");
        builder.AppendLine("schema-version=1");
        builder.AppendLine("upstream-opencv-version=5.0.0");
        builder.AppendLine("claimed-slice=" + ClaimedSlice);
        builder.AppendLine("header-sha256=" + raw.HeaderSha256);
        builder.AppendLine("parser-sha256=" + raw.ParserSha256);
        builder.AppendLine("declaration-count=183");
        builder.AppendLine("callable-count=160");
        builder.AppendLine("class-count=17");
        builder.AppendLine("enum-count=6");
        builder.AppendLine("repository-wide-upstream-parity-claimed=false");
        foreach (CompatibilityHeader header in raw.CompatibilityHeaders)
            builder.AppendLine($"compatibility-header={header.Path}|{header.Sha256}|includes={header.Includes}");
        builder.AppendLine();
        builder.AppendLine("ordinal|kind|source-header|classification|identity|native-entrypoints|managed-members|build-condition|reason");
        for (int index = 0; index < raw.Declarations.Count; index++)
        {
            RawDeclaration declaration = raw.Declarations[index];
            ClassificationRow row = classifications.Declarations[index];
            builder.AppendLine($"{index}|{declaration.Kind}|{declaration.SourceHeader}|{row.Classification}|{declaration.Identity}|{JoinEvidence(row.NativeEntrypoints)}|{JoinEvidence(row.ManagedMembers)}|{row.BuildCondition}|{row.Reason}");
        }
        return builder.ToString().Replace("\r\n", "\n", StringComparison.Ordinal);
    }

    private static FamilyDocument BuildFamilies(RawDocument raw, ClassificationDocument classifications)
    {
        var family = new FamilyRow();
        foreach (int ordinal in Selected.OrderBy(value => value))
        {
            family.Declarations.Add(new FamilyOperation
            {
                Ordinal = ordinal,
                UpstreamIdentity = raw.Declarations[ordinal].Identity,
                UpstreamClassification = classifications.Declarations[ordinal].Classification,
                NativeEntrypoints = new List<string>(classifications.Declarations[ordinal].NativeEntrypoints),
                ManagedMembers = new List<string>(classifications.Declarations[ordinal].ManagedMembers)
            });
        }
        return new FamilyDocument { Families = new List<FamilyRow> { family } };
    }

    private static ExtensionDocument BuildExtensions(RawDocument raw)
    {
        return new ExtensionDocument
        {
            SourceHeader = raw.SourceHeaders[0].Path,
            SourceHeaderSha256 = raw.SourceHeaders[0].Sha256,
            ParserDerived = false,
            Declarations = new List<string>
            {
                "class cv.KeyPointsFilter",
                "cv.KeyPointsFilter.KeyPointsFilter()",
                "cv.KeyPointsFilter.runByImageBorder(vector_KeyPoint keypoints[/IO];Size imageSize;int borderSize)->void",
                "cv.KeyPointsFilter.runByKeypointSize(vector_KeyPoint keypoints[/IO];float minSize;float maxSize=FLT_MAX)->void",
                "cv.KeyPointsFilter.runByPixelsMask(vector_KeyPoint keypoints[/IO];Mat mask)->void",
                "cv.KeyPointsFilter.runByPixelsMask2VectorPoint(vector_KeyPoint keypoints[/IO];vector_vector_Point removeFrom[/IO];Mat mask)->void",
                "cv.KeyPointsFilter.removeDuplicated(vector_KeyPoint keypoints[/IO])->void",
                "cv.KeyPointsFilter.removeDuplicatedSorted(vector_KeyPoint keypoints[/IO])->void",
                "cv.KeyPointsFilter.retainBest(vector_KeyPoint keypoints[/IO];int npoints)->void"
            }
        };
    }

    private static void RunNegativeFixtures(RawDocument raw, ClassificationDocument classifications, Options options, string[] native, string[] managed)
    {
        int passed = 0;
        void Fixture(Action<RawDocument, ClassificationDocument> mutation)
        {
            RawDocument rawCopy = Clone(raw);
            ClassificationDocument classCopy = Clone(classifications);
            mutation(rawCopy, classCopy);
            bool failed = false;
            try { Validate(rawCopy, classCopy, options, native, managed, true); }
            catch { failed = true; }
            Require(failed, "A Features negative fixture was accepted.");
            passed++;
        }

        Fixture((_, value) => value.Declarations.RemoveAt(0));
        Fixture((_, value) => value.Declarations[1].Ordinal = 0);
        Fixture((_, value) => (value.Declarations[0], value.Declarations[1]) = (value.Declarations[1], value.Declarations[0]));
        Fixture((value, _) => value.Declarations[1].Identity = value.Declarations[0].Identity);
        Fixture((_, value) => value.Declarations[3].Classification = "implemented");
        Fixture((value, _) => value.Declarations[0].SourceHeader = "drifted/features.hpp");
        Fixture((value, _) => value.ParserSha256 = new string('0', 64));
        Fixture((value, _) => value.HeaderSha256 = new string('0', 64));
        Fixture((_, value) => value.Declarations[173].NativeEntrypoints[0] = "jyppx_ocv_features2d_false_evidence");
        Fixture((_, value) => value.Declarations[173].ManagedMembers[0] = "MEMBER|false");
        Fixture((_, value) => value.Declarations[2].Reason = "");
        Fixture((_, value) => value.ClaimedSlice += "; OpenCv5Sharp fixed-major");
        Fixture((_, value) => value.Declarations[113].BuildCondition = "unconditional");
        Fixture((_, value) => value.Declarations[4].NativeEntrypoints.Reverse());
        Fixture((value, _) => value.CompatibilityHeaders.RemoveAt(1));
        Require(passed == 15, "Features negative fixture count drifted.");
    }

    private static T Clone<T>(T value) => JsonSerializer.Deserialize<T>(Serialize(value), JsonOptions())!;
    private static T Read<T>(string path) => JsonSerializer.Deserialize<T>(File.ReadAllText(path, Encoding.UTF8), JsonOptions()) ?? throw new InvalidOperationException("Could not parse " + path);

    private static string[] ReadNative(string path)
    {
        return File.ReadAllLines(path, Encoding.UTF8)
            .Where(line => line.StartsWith("jyppx_ocv_", StringComparison.Ordinal))
            .Select(line => line.Split('|')[0])
            .OrderBy(value => value, Ordinal)
            .ToArray();
    }

    private static JsonSerializerOptions JsonOptions() => new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private static string Serialize<T>(T value) => JsonSerializer.Serialize(value, JsonOptions()) + "\n";

    private static void WriteOrCheck(string path, string content, bool check)
    {
        content = content.Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\r", "\n", StringComparison.Ordinal);
        if (check)
        {
            Require(File.Exists(path), "Generated file is missing: " + path);
            string current = File.ReadAllText(path, Encoding.UTF8).Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\r", "\n", StringComparison.Ordinal);
            Require(current == content, "Generated file is out of date: " + path);
            return;
        }
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content, new UTF8Encoding(false));
    }

    private static void VerifyHash(string workspace, string relativePath, string expected, string label)
    {
        string path = Path.Combine(workspace, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Require(File.Exists(path), $"Features {label} is missing: {relativePath}");
        Require(Sha256File(path) == expected, $"Features {label} hash drifted: {relativePath}");
    }

    private static string JoinEvidence(List<string> values) => values.Count == 0 ? "-" : string.Join(";", values.Select(Escape));
    private static string Escape(string value) => value.Replace("|", "<pipe>", StringComparison.Ordinal);
    private static string Rel(string root, string path) => Path.GetRelativePath(root, path).Replace('\\', '/');
    private static string Sha256(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
    private static string Sha256File(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}

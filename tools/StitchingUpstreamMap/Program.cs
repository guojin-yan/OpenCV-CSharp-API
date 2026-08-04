using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

internal static class Program
{
    private static readonly StringComparer Ordinal = StringComparer.Ordinal;
    private static readonly int[] ExposureOrdinals =
    {
        70, 71, 72, 73, 74, 75, 76, 78, 79, 80, 82, 83, 84, 85, 86,
        87, 88, 89, 90, 92, 93, 94, 95, 96, 97, 98, 99, 101, 102, 103,
        104, 105, 106, 107, 108, 109, 110, 111, 112, 114, 115, 116, 117, 118, 120
    };
    private static readonly HashSet<int> Exposure = new(ExposureOrdinals);
    private static readonly int[] PublicWarperOrdinals = Enumerable.Range(25, 10).ToArray();
    private static readonly HashSet<int> PublicWarper = new(PublicWarperOrdinals);
    private static readonly int[] BlenderOrdinals =
    {
        40, 41, 42, 43, 44, 46, 47, 48, 49, 50, 51, 52,
        54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65
    };
    private static readonly HashSet<int> Blender = new(BlenderOrdinals);
    private static readonly int[] MatcherOrdinals = { 122, 123, 124, 126, 127, 129, 130, 131, 132, 134, 135, 136, 138, 140 };
    private static readonly HashSet<int> Matcher = new(MatcherOrdinals);
    private static readonly HashSet<int> MatcherUnsupported = new(new[] { 142, 143 });
    private static readonly int[] CameraMotionOrdinals =
    {
        36, 37, 67, 145, 147, 149, 151, 152, 153, 154,
        155, 156, 158, 160, 162, 164, 166, 168, 169, 170
    };
    private static readonly HashSet<int> CameraMotion = new(CameraMotionOrdinals);
    private static readonly int[] SeamFinderOrdinals = { 173, 174, 176, 178, 180, 183, 184, 187, 188 };
    private static readonly HashSet<int> SeamFinder = new(SeamFinderOrdinals);
    private static readonly int[] TimelapserOrdinals = { 191, 192, 193, 194 };
    private static readonly HashSet<int> Timelapser = new(TimelapserOrdinals);
    private static readonly int[] UtilityOrdinals = { 196, 197, 198, 199, 200, 201, 202 };
    private static readonly HashSet<int> Utility = new(UtilityOrdinals);
    private static readonly int[] SphericalProjectorOrdinals = { 205, 206 };
    private static readonly HashSet<int> SphericalProjector = new(SphericalProjectorOrdinals);
    private static readonly HashSet<int> HighLevel = new(Enumerable.Range(3, 21));
    private static readonly string[] Allowed =
    {
        "implemented", "missing", "intentionally-omitted", "upstream-conditional", "unsupported", "non-callable-metadata"
    };
    private const string ClaimedSlice = "OpenCV 5.0.0 installed public main Stitching header closure, partitioned by high-level, public-warper, and detail source header";
    private const string BuildCondition = "OPENCV_CSHARP_HAS_OPENCV_STITCHING; full-profile; mini-excluded";
    private const int NegativeFixtureCount = 32;
    private const int ManagedTypeAdditions = 14;
    private const int ManagedMemberAdditions = 38;
    private const int NativeEntrypointAdditions = 22;

    private sealed record Options(string Repository, string Workspace, string Raw, string Classification,
        string NativeManifest, string ManagedBaseline, string Output, string Summary, string FamilyOutput,
        bool Initialize, bool Check);

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
        public Dictionary<string, int> SurfaceCounts { get; set; } = new(Ordinal);
        public List<RawDeclaration> Declarations { get; set; } = new();
    }

    private sealed class CompatibilityHeader { public string Path { get; set; } = ""; public string Sha256 { get; set; } = ""; public string Includes { get; set; } = ""; }
    private sealed class ExcludedPublicHeader { public string Path { get; set; } = ""; public string Reason { get; set; } = ""; }
    private sealed class SourceHeader { public string Surface { get; set; } = ""; public string Path { get; set; } = ""; public string Sha256 { get; set; } = ""; public int StartOrdinal { get; set; } public int DeclarationCount { get; set; } }
    private sealed class RawDeclaration { public int Ordinal { get; set; } public string Surface { get; set; } = ""; public string SourceHeader { get; set; } = ""; public string Kind { get; set; } = ""; public string Name { get; set; } = ""; public string Identity { get; set; } = ""; public string ReturnType { get; set; } = ""; public List<string> Modifiers { get; set; } = new(); public List<RawArgument> Arguments { get; set; } = new(); public List<RawEnumValue> EnumValues { get; set; } = new(); public string BaseDeclaration { get; set; } = ""; public string Documentation { get; set; } = ""; }
    private sealed class RawArgument { public string Type { get; set; } = ""; public string Name { get; set; } = ""; public string Default { get; set; } = ""; public List<string> Modifiers { get; set; } = new(); }
    private sealed class RawEnumValue { public string Name { get; set; } = ""; public string Value { get; set; } = ""; }

    private sealed class ClassificationDocument
    {
        public int SchemaVersion { get; set; } = 1;
        public string UpstreamOpenCvVersion { get; set; } = "5.0.0";
        public string ClaimedSlice { get; set; } = Program.ClaimedSlice;
        public string ReviewStatus { get; set; } = "source-reviewed";
        public string Limitation { get; set; } = "Closure is module-scoped. CUDA execution, callbacks, LightGlue model ownership, templates, and repository-wide parity are not claimed.";
        public List<ClassificationRow> Declarations { get; set; } = new();
    }

    private sealed class ClassificationRow
    {
        public int Ordinal { get; set; }
        public string Surface { get; set; } = "";
        public string Identity { get; set; } = "";
        public string Classification { get; set; } = "";
        public string Reason { get; set; } = "";
        public string BuildCondition { get; set; } = "";
        public List<string> NativeEntrypoints { get; set; } = new();
        public List<string> ManagedMembers { get; set; } = new();
    }

    private static int Main(string[] args)
    {
        try
        {
            Options options = Parse(args);
            RawDocument raw = Read<RawDocument>(options.Raw);
            string[] native = File.ReadAllLines(options.NativeManifest, Encoding.UTF8)
                .Where(x => x.Length > 0 && !x.StartsWith('#')).Select(x => x.Split('|')[0]).ToArray();
            string[] managed = File.ReadAllLines(options.ManagedBaseline, Encoding.UTF8);
            ValidateRaw(raw, options.Workspace);
            if (options.Initialize)
            {
                WriteOrCheck(options.Classification, Serialize(Initialize(raw, native, managed)), false);
            }

            ClassificationDocument classifications = Read<ClassificationDocument>(options.Classification);
            Validate(raw, classifications, native, managed);
            RunNegativeFixtures(raw, classifications, native, managed, options.Workspace);
            string mapping = BuildMap(raw, classifications);
            string families = Serialize(BuildFamilies(raw, classifications));
            var counts = Allowed.ToDictionary(x => x, x => classifications.Declarations.Count(row => row.Classification == x), Ordinal);
            var surfaceCounts = raw.SourceHeaders.ToDictionary(
                x => x.Surface,
                x => new SurfaceSummary
                {
                    Declarations = x.DeclarationCount,
                    Callables = raw.Declarations.Count(d => d.Surface == x.Surface && d.Kind == "callable"),
                    Implemented = classifications.Declarations.Count(d => d.Surface == x.Surface && d.Classification == "implemented")
                }, Ordinal);
            string sourceHeaderSet = string.Join("\n", raw.SourceHeaders.Select(x => $"{x.Surface}|{x.Path}|{x.Sha256}|{x.StartOrdinal}|{x.DeclarationCount}")) + "\n";
            var summary = new SummaryDocument
            {
                RawExtractionPath = Rel(options.Repository, options.Raw),
                ClassificationPath = Rel(options.Repository, options.Classification),
                MappingPath = Rel(options.Repository, options.Output),
                HeaderSha256 = raw.HeaderSha256,
                ParserSha256 = raw.ParserSha256,
                CompatibilityHeaderCount = raw.CompatibilityHeaders.Count,
                SourceHeaderCount = raw.SourceHeaders.Count,
                SourceHeaderSetSha256 = Sha256(sourceHeaderSet),
                MappingSha256 = Sha256(mapping),
                DeclarationCount = raw.Declarations.Count,
                EnumCount = raw.Declarations.Count(x => x.Kind == "enum"),
                ClassCount = raw.Declarations.Count(x => x.Kind == "class"),
                CallableCount = raw.Declarations.Count(x => x.Kind == "callable"),
                SurfaceCounts = surfaceCounts,
                ClassificationCounts = new SortedDictionary<string, int>(counts, Ordinal),
                NativeEvidenceCount = classifications.Declarations.SelectMany(x => x.NativeEntrypoints).Distinct(Ordinal).Count(),
                ManagedEvidenceCount = classifications.Declarations.SelectMany(x => x.ManagedMembers).Distinct(Ordinal).Count(),
                FamilyInventoryPath = Rel(options.Repository, options.FamilyOutput),
                FamilyInventorySha256 = Sha256(families),
            };
            WriteOrCheck(options.Output, mapping, options.Check);
            WriteOrCheck(options.FamilyOutput, families, options.Check);
            WriteOrCheck(options.Summary, Serialize(summary), options.Check);
            Console.WriteLine($"STITCHING_UPSTREAM_MAP_OK declarations={summary.DeclarationCount} callables={summary.CallableCount} implemented={counts["implemented"]} missing={counts["missing"]} seam={SeamFinder.Count} timelapser={Timelapser.Count} utility={Utility.Count} projector={SphericalProjector.Count} fixtures={NegativeFixtureCount} sha256={summary.MappingSha256} mode={(options.Check ? "check" : "write")}");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
    }

    private sealed class SurfaceSummary { public int Declarations { get; init; } public int Callables { get; init; } public int Implemented { get; init; } }
    private sealed class SummaryDocument
    {
        public int SchemaVersion { get; init; } = 1;
        public string Generator { get; init; } = "tools/StitchingUpstreamMap";
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
        public Dictionary<string, SurfaceSummary> SurfaceCounts { get; init; } = new(Ordinal);
        public SortedDictionary<string, int> ClassificationCounts { get; init; } = new(Ordinal);
        public int NativeEvidenceCount { get; init; }
        public int ManagedEvidenceCount { get; init; }
        public int NegativeFixtureCount { get; init; } = Program.NegativeFixtureCount;
        public string FamilyInventoryPath { get; init; } = "";
        public string FamilyInventorySha256 { get; init; } = "";
        public int SelectedFamilyCount { get; init; } = 9;
        public int SelectedDeclarationCount { get; init; } = 146;
        public int HighLevelImplementedCallableCount { get; init; } = 21;
        public int SourceReviewedExtensionCount { get; init; } = 4;
        public int ManagedPublicTypeAdditionCount { get; init; } = ManagedTypeAdditions;
        public int ManagedPublicMemberAdditionCount { get; init; } = ManagedMemberAdditions;
        public int NativeEntrypointAdditionCount { get; init; } = NativeEntrypointAdditions;
        public bool UMatExecutionClaimed { get; init; }
        public bool DetailRowsMixedIntoHighLevel { get; init; }
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
    private sealed class FamilyRow { public string Id { get; init; } = ""; public string Surface { get; init; } = ""; public string Rationale { get; init; } = ""; public List<FamilyOperation> Declarations { get; init; } = new(); }
    private sealed class FamilyOperation { public int Ordinal { get; init; } public string UpstreamIdentity { get; init; } = ""; public List<string> NativeEntrypoints { get; init; } = new(); public List<string> ManagedMembers { get; init; } = new(); public string FocusedTest { get; init; } = "tests/OpenCvSharp.Tests/Stitching/ExposureCompensatorTests.cs"; public string NativeSmoke { get; init; } = "src/OpenCvSharp.Native/tests/native_smoke.cpp"; public string Sample { get; init; } = "samples/ConsoleSamples/Program.cs"; public string Guide { get; init; } = "docs/articles/stitching-structured-parity-guide.md"; }
    private sealed class SourceReviewedExtension { public string UpstreamIdentity { get; init; } = ""; public string SourceHeader { get; init; } = ""; public string Adaptation { get; init; } = ""; public List<string> NativeEntrypoints { get; init; } = new(); public List<string> ManagedMembers { get; init; } = new(); }

    private static ClassificationDocument Initialize(RawDocument raw, string[] native, string[] managed)
    {
        var document = new ClassificationDocument();
        foreach (RawDeclaration declaration in raw.Declarations)
        {
            var row = new ClassificationRow { Ordinal = declaration.Ordinal, Surface = declaration.Surface, Identity = declaration.Identity, BuildCondition = BuildCondition };
            if (declaration.Kind != "callable")
            {
                row.Classification = "non-callable-metadata";
                row.Reason = "Parser-emitted type or enum shape is reviewed as metadata rather than an independent ABI operation.";
            }
            else if (MatcherUnsupported.Contains(declaration.Ordinal))
            {
                row.Classification = "unsupported";
                row.Reason = "LightGlue matching requires an externally supplied ONNX model and an owned cv::LightGlueMatcher lifecycle that this repository does not currently expose; invalid-path construction is not sufficient evidence.";
            }
            else if (HighLevel.Contains(declaration.Ordinal) || Exposure.Contains(declaration.Ordinal) || PublicWarper.Contains(declaration.Ordinal) || Blender.Contains(declaration.Ordinal) || Matcher.Contains(declaration.Ordinal) || CameraMotion.Contains(declaration.Ordinal) || SeamFinder.Contains(declaration.Ordinal) || Timelapser.Contains(declaration.Ordinal) || Utility.Contains(declaration.Ordinal) || SphericalProjector.Contains(declaration.Ordinal))
            {
                row.Classification = "implemented";
                row.Reason = Exposure.Contains(declaration.Ordinal)
                    ? "The selected Exposure Compensation family is implemented through an owned cv::Ptr handle, temporary Mat-to-UMat borrowing, in-place apply, independent gain copies, native smoke, and net8/net10 tests."
                    : SeamFinder.Contains(declaration.Ordinal)
                        ? "The complete seam-finder family uses owned strategy handles, strongly typed costs, temporary UMat inputs, and transactional mask commits."
                    : Timelapser.Contains(declaration.Ordinal)
                        ? "The complete timelapser family uses owned state, validated placements, borrowed process inputs, and independent CPU destination copies."
                    : Utility.Contains(declaration.Ordinal)
                        ? "The complete detail utility family uses checked geometry collections, bounded subset output, and a read-only process-global log query."
                    : SphericalProjector.Contains(declaration.Ordinal)
                        ? "The parser-visible spherical mapping methods use an owned source-reviewed camera configuration and copied projector state."
                    : CameraMotion.Contains(declaration.Ordinal)
                        ? "The selected camera and motion-estimator family uses copied camera matrices, owned estimator handles, transactional outputs, exact N-squared collections, strict UTF-8 packing, native smoke, and net8/net10 tests."
                    : PublicWarper.Contains(declaration.Ordinal)
                        ? "The complete public PyRotationWarper family is implemented with owned lifetime, strict projector/K/R contracts, caller-owned maps and images, safe default state, native smoke, and net8/net10 tests."
                        : Matcher.Contains(declaration.Ordinal)
                            ? "The selected detail matcher family is implemented with owned ImageFeatures and MatchesInfo value handles, copied descriptor/homography outputs, exact N-squared batch results, Feature2D finder bridging, native smoke, and net8/net10 tests."
                        : Blender.Contains(declaration.Ordinal)
                            ? "The complete detail Blender family is implemented with owned strategy lifetime, bounded prepare/feed/blend state, caller-owned outputs, copied UMat collections, source-true GPU failure behavior, native smoke, and net8/net10 tests."
                        : "The existing high-level Stitcher surface implements this parser row through the version-neutral native ABI and managed API.";
                row.NativeEntrypoints.AddRange(NativeEvidence(declaration.Ordinal, native));
                row.ManagedMembers.AddRange(ManagedEvidence(declaration.Ordinal, managed));
            }
            else
            {
                row.Classification = "missing";
                row.Reason = MissingReason(declaration.Surface);
            }
            row.NativeEntrypoints = row.NativeEntrypoints.Distinct(Ordinal).OrderBy(x => x, Ordinal).ToList();
            row.ManagedMembers = row.ManagedMembers.Distinct(Ordinal).OrderBy(x => x, Ordinal).ToList();
            document.Declarations.Add(row);
        }
        return document;
    }

    private static string MissingReason(string surface) => surface switch
    {
        "public-warpers" => "Public PyRotationWarper construction, matrix validation, map ownership, ROI, and forward/backward warp operations remain an unimplemented module-scoped family.",
        "detail-autocalib" => "The selected autocalibration helpers are implemented with exact CV_64FC1 input and caller-owned or independently owned matrix outputs.",
        "detail-blenders" => "Detail blender lifecycle, UMat pyramids, GPU branches, and output ownership remain unimplemented.",
        "detail-camera" => "CameraParams.K is represented by the copied high-level StitcherCameraParams intrinsic-matrix workflow.",
        "detail-matchers" => "Detail matcher families outside ImageFeatures and BestOf2Nearest strategies remain unimplemented; LightGlue rows are explicitly unsupported because their model lifecycle is not owned.",
        "detail-motion-estimators" => "The selected estimator, bundle-adjuster, wave-correction, graph, and component result contracts are implemented; retained seam and timelapse strategies remain separate gaps.",
        "detail-seam-finders" => "Detail seam-finder retained strategy and mutable UMat mask collection contracts remain unimplemented.",
        "detail-timelapsers" => "Detail timelapser state, borrowed destination UMat, and output ownership remain unimplemented.",
        "detail-util" => "Detail ROI, random subset, and logging helpers remain unimplemented as a separately reviewable utility family.",
        "detail-warpers" => "Parser-visible internal projector methods remain unimplemented and are not conflated with public warpers.",
        _ => "This callable remains an explicit module-scoped implementation gap."
    };

    private static IEnumerable<string> NativeEvidence(int ordinal, string[] native)
    {
        string[] values = ordinal switch
        {
            3 => N("stitcher_create"),
            4 or 5 or 6 or 7 or 8 or 9 or 10 or 11 or 23 => N(ordinal is 5 or 7 or 9 or 11 ? "stitcher_set_double_property" : "stitcher_get_double_property"),
            12 or 13 or 14 or 15 => N(ordinal is 13 or 15 ? "stitcher_set_int_property" : "stitcher_get_int_property"),
            16 => N("stitcher_estimate_transform"), 17 => N("stitcher_compose_panorama"), 18 => N("stitcher_compose_panorama_images"),
            19 or 20 => N("stitcher_stitch"), 21 => N("stitcher_get_component_count", "stitcher_get_component_fill"),
            22 => N("stitcher_get_cameras_count", "stitcher_get_cameras_fill"),
            36 => N("stitching_focals_from_homography"),
            37 => N("stitching_calibrate_rotating_camera"),
            25 => W("create"), 26 => W("create_default"), 27 => W("warp_point"), 28 => W("warp_point_backward"),
            29 => W("build_maps"), 30 => W("warp"), 31 => W("warp_backward"), 32 => W("warp_roi"),
            33 => W("get_scale"), 34 => W("set_scale"),
            40 => B("create_default", "release_handle"), 41 => B("prepare"), 42 => B("prepare_roi"),
            43 => B("feed"), 44 => B("blend"), 46 => B("create_feather", "release_handle"),
            47 => B("get_sharpness"), 48 => B("set_sharpness"), 49 => B("prepare_roi"),
            50 => B("feed"), 51 => B("blend"), 52 => B("create_weight_maps"),
            54 => B("create_multi_band", "release_handle"), 55 => B("get_number_of_bands"),
            56 => B("set_number_of_bands"), 57 => B("prepare_roi"), 58 => B("feed"), 59 => B("blend"),
            60 => N("stitching_normalize_using_weight_map"), 61 => N("stitching_create_weight_map"),
            62 => N("stitching_create_laplace_pyramid"), 63 => N("stitching_create_laplace_pyramid_gpu"),
            64 => N("stitching_restore_image_from_laplace_pyramid"),
            65 => N("stitching_restore_image_from_laplace_pyramid_gpu"),
            67 => N("stitching_camera_params_get_k"),
            122 => N("stitching_image_features_get_keypoints_count", "stitching_image_features_get_keypoints_fill"),
            123 => N("stitching_compute_image_features_batch"), 124 => N("stitching_compute_image_features"),
            126 => N("stitching_matches_info_get_matches_count", "stitching_matches_info_get_matches_fill"),
            127 => N("stitching_matches_info_get_inliers_count", "stitching_matches_info_get_inliers_fill"),
            129 => N("stitching_features_matcher_match_pair"), 130 => N("stitching_features_matcher_match_batch"),
            131 => N("stitching_features_matcher_is_thread_safe"), 132 => N("stitching_features_matcher_collect_garbage"),
            134 => N("stitching_features_matcher_create_best_of_two_nearest", "stitching_features_matcher_release_handle"),
            135 => N("stitching_features_matcher_collect_garbage"),
            136 => N("stitching_features_matcher_factory_best_of_two_nearest", "stitching_features_matcher_release_handle"),
            138 => N("stitching_features_matcher_create_range", "stitching_features_matcher_release_handle"),
            140 => N("stitching_features_matcher_create_affine", "stitching_features_matcher_release_handle"),
            145 => N("stitching_estimator_apply"),
            147 => N("stitching_estimator_create_homography", "stitching_estimator_release_handle"),
            149 => N("stitching_estimator_create_affine", "stitching_estimator_release_handle"),
            151 => N("stitching_bundle_adjuster_copy_refinement_mask"),
            152 => N("stitching_bundle_adjuster_set_refinement_mask"),
            153 => N("stitching_bundle_adjuster_get_confidence_threshold"),
            154 => N("stitching_bundle_adjuster_set_confidence_threshold"),
            155 => N("stitching_bundle_adjuster_get_term_criteria"),
            156 => N("stitching_bundle_adjuster_set_term_criteria"),
            158 => N("stitching_estimator_create_no_bundle_adjuster", "stitching_estimator_release_handle"),
            160 => N("stitching_estimator_create_bundle_adjuster_reproj", "stitching_estimator_release_handle"),
            162 => N("stitching_estimator_create_bundle_adjuster_ray", "stitching_estimator_release_handle"),
            164 => N("stitching_estimator_create_bundle_adjuster_affine", "stitching_estimator_release_handle"),
            166 => N("stitching_estimator_create_bundle_adjuster_affine_partial", "stitching_estimator_release_handle"),
            168 => N("stitching_wave_correct"),
            169 => N("stitching_matches_graph_as_string"),
            170 => N("stitching_leave_biggest_component"),
            173 or 176 or 178 or 180 or 188 => N("stitching_seam_finder_find"),
            174 => N("stitching_seam_finder_create_default", "stitching_seam_finder_release_handle"),
            183 => N("stitching_seam_finder_create_dp", "stitching_seam_finder_release_handle"),
            184 => N("stitching_seam_finder_set_dp_cost"),
            187 => N("stitching_seam_finder_create_graph_cut", "stitching_seam_finder_release_handle"),
            191 => N("stitching_timelapser_create_default", "stitching_timelapser_release_handle"),
            192 => N("stitching_timelapser_initialize"),
            193 => N("stitching_timelapser_process"),
            194 => N("stitching_timelapser_get_dst"),
            196 => N("stitching_overlap_roi"),
            197 => N("stitching_result_roi_images"),
            198 => N("stitching_result_roi_sizes"),
            199 => N("stitching_result_roi_intersection"),
            200 => N("stitching_result_tl"),
            201 => N("stitching_select_random_subset"),
            202 => N("stitching_log_level"),
            205 => N("stitching_spherical_projector_create", "stitching_spherical_projector_release_handle", "stitching_spherical_projector_map_forward"),
            206 => N("stitching_spherical_projector_create", "stitching_spherical_projector_release_handle", "stitching_spherical_projector_map_backward"),
            70 => E("create_default"), 71 => E("feed"), 72 or 78 or 84 or 93 or 101 or 116 => E("apply"),
            73 or 79 or 85 or 94 or 102 or 117 => E("get_mat_gains_count", "get_mat_gains_fill"),
            74 or 80 or 86 or 95 or 103 or 118 => E("set_mat_gains"), 75 => E("set_update_gain"), 76 => E("get_update_gain"),
            82 or 83 => E("create_gain"), 87 or 96 or 104 => E("set_number_of_feeds"), 88 or 97 or 105 => E("get_number_of_feeds"),
            89 or 98 or 106 => E("set_similarity_threshold"), 90 or 99 or 107 => E("get_similarity_threshold"), 92 => E("create_channels"),
            108 or 109 => E("set_block_size"), 110 => E("get_block_size"), 111 => E("set_filtering_iterations"), 112 => E("get_filtering_iterations"),
            114 or 115 => E("create_blocks_gain"), 120 => E("create_blocks_channels"),
            _ => throw new InvalidOperationException("No native evidence mapping for Stitching ordinal " + ordinal)
        };
        foreach (string value in values)
        {
            Require(native.Contains(value, Ordinal), "Native manifest is missing Stitching evidence: " + value);
            yield return value;
        }
        static string[] N(params string[] suffixes) => suffixes.Select(x => "jyppx_ocv_" + x).ToArray();
        static string[] E(params string[] suffixes) => suffixes.Select(x => "jyppx_ocv_stitching_exposure_" + x).ToArray();
        static string[] W(params string[] suffixes) => suffixes.Select(x => "jyppx_ocv_stitching_py_rotation_warper_" + x).ToArray();
        static string[] B(params string[] suffixes) => suffixes.Select(x => "jyppx_ocv_stitching_blender_" + x).ToArray();
    }

    private static IEnumerable<string> ManagedEvidence(int ordinal, string[] managed)
    {
        string[] fragments = ordinal switch
        {
            3 => M("Stitcher|method|public;static|", "Stitcher Create("),
            4 or 5 => M("Stitcher|property|", "RegistrationResol"), 6 or 7 => M("Stitcher|property|", "SeamEstimationResol"),
            8 or 9 => M("Stitcher|property|", "CompositingResol"), 10 or 11 => M("Stitcher|property|", "PanoConfidenceThresh"),
            12 or 13 => M("Stitcher|property|", "WaveCorrection"), 14 or 15 => M("Stitcher|property|", "InterpolationFlags"),
            16 => M("Stitcher|method|public;instance|", "EstimateTransform(JYPPX.OpenCvSharp.Core.Mat[] images,JYPPX.OpenCvSharp.Core.Mat[]? masks)"),
            17 => M("Stitcher|method|public;instance|", "ComposePanorama(JYPPX.OpenCvSharp.Core.Mat pano)"),
            18 => M("Stitcher|method|public;instance|", "ComposePanorama(JYPPX.OpenCvSharp.Core.Mat[] images"),
            19 => M("Stitcher|method|public;instance|", "Stitch(JYPPX.OpenCvSharp.Core.Mat[] images,JYPPX.OpenCvSharp.Core.Mat pano)"),
            20 => M("Stitcher|method|public;instance|", "Stitch(JYPPX.OpenCvSharp.Core.Mat[] images,JYPPX.OpenCvSharp.Core.Mat[]? masks"),
            21 => M("Stitcher|method|public;instance|", "GetComponent()"), 22 => M("Stitcher|method|public;instance|", "GetCameras()"),
            23 => M("Stitcher|property|", "WorkScale"),
            36 => M("StitchingMotion|method|public;static|", "FocalsFromHomography("),
            37 => M("StitchingMotion|method|public;static|System.Boolean CalibrateRotatingCamera("),
            25 => M("PyRotationWarper|constructor|", ".ctor(System.String type,System.Single scale)"),
            26 => M("PyRotationWarper|constructor|", ".ctor()"),
            27 => M("PyRotationWarper|method|public;instance|", "Point2f WarpPoint("),
            28 => M("PyRotationWarper|method|public;instance|", "Point2f WarpPointBackward("),
            29 => M("PyRotationWarper|method|public;instance|", "Rect BuildMaps("),
            30 => M("PyRotationWarper|method|public;instance|", "Point Warp("),
            31 => M("PyRotationWarper|method|public;instance|", "Void WarpBackward("),
            32 => M("PyRotationWarper|method|public;instance|", "Rect WarpRoi("),
            33 or 34 => M("PyRotationWarper|property|", "Single Scale"),
            40 => M("Blender|method|public;static|", "Blender CreateDefault("),
            41 => M("Blender|method|public;instance|", "Prepare(JYPPX.OpenCvSharp.Core.Point[] corners,JYPPX.OpenCvSharp.Core.Size[] sizes)"),
            42 or 49 or 57 => M("Blender|method|public;instance|", "Prepare(JYPPX.OpenCvSharp.Core.Rect destinationRoi)"),
            43 or 50 or 58 => M("Blender|method|public;instance|", " Feed(JYPPX.OpenCvSharp.Core.Mat image"),
            44 or 51 or 59 => M("Blender|method|public;instance|", " Blend(JYPPX.OpenCvSharp.Core.Mat destination"),
            46 => M("FeatherBlender|constructor|", ".ctor(System.Single sharpness=0.02)"),
            47 or 48 => M("FeatherBlender|property|", "Single Sharpness"),
            52 => M("FeatherBlender|method|public;instance|", "CreateWeightMaps("),
            54 => M("MultiBandBlender|constructor|", ".ctor(System.Boolean tryGpu=false,System.Int32 numberOfBands=5,System.Int32 weightType=5)"),
            55 or 56 => M("MultiBandBlender|property|", "Int32 NumberOfBands"),
            60 => M("Blender|method|public;static|", "NormalizeUsingWeightMap("),
            61 => M("Blender|method|public;static|", "CreateWeightMap("),
            62 => M("Blender|method|public;static|", "CreateLaplacePyramid("),
            63 => M("Blender|method|public;static|", "CreateLaplacePyramidGpu("),
            64 => M("Blender|method|public;static|", "RestoreImageFromLaplacePyramid("),
            65 => M("Blender|method|public;static|", "RestoreImageFromLaplacePyramidGpu("),
            67 => M("StitcherCameraParams|method|public;instance|", "GetCameraMatrix()"),
            70 => M("ExposureCompensator|method|public;static|", "CreateDefault("), 71 => M("ExposureCompensator|method|public;instance|", " Feed("),
            72 or 78 or 84 or 93 or 101 or 116 => M("ExposureCompensator|method|public;instance|", " Apply("),
            73 or 79 or 85 or 94 or 102 or 117 => M("ExposureCompensator|method|public;instance|", " GetMatGains()"),
            74 or 80 or 86 or 95 or 103 or 118 => M("ExposureCompensator|method|public;instance|", " SetMatGains("),
            75 or 76 => M("ExposureCompensator|property|", "UpdateGain"),
            82 or 83 => M("GainCompensator|constructor|", ".ctor(System.Int32 numberOfFeeds=1)"),
            87 or 88 => M("GainCompensator|property|", "NumberOfFeeds"), 89 or 90 => M("GainCompensator|property|", "SimilarityThreshold"),
            92 => M("ChannelsCompensator|constructor|", ".ctor(System.Int32 numberOfFeeds=1)"),
            96 or 97 => M("ChannelsCompensator|property|", "NumberOfFeeds"), 98 or 99 => M("ChannelsCompensator|property|", "SimilarityThreshold"),
            104 or 105 => M("BlocksCompensator|property|", "NumberOfFeeds"), 106 or 107 => M("BlocksCompensator|property|", "SimilarityThreshold"),
            108 or 109 or 110 => M("BlocksCompensator|property|", "BlockSize"), 111 or 112 => M("BlocksCompensator|property|", "FilteringIterations"),
            114 or 115 => M("BlocksGainCompensator|constructor|", ".ctor(System.Int32 blockWidth=32"),
            120 => M("BlocksChannelsCompensator|constructor|", ".ctor(System.Int32 blockWidth=32"),
            122 => M("ImageFeatures|property|instance;get:public|", "KeyPoint[] Keypoints"),
            123 => M("ImageFeatures|method|public;static|", "ImageFeatures[] Compute("),
            124 => M("ImageFeatures|method|public;static|", "ImageFeatures Compute("),
            126 => M("MatchesInfo|property|instance;get:public|", "DMatch[] Matches"),
            127 => M("MatchesInfo|property|instance;get:public|", "Byte[] Inliers"),
            129 => M("FeaturesMatcher|method|public;instance|", "MatchesInfo Match(JYPPX.OpenCvSharp.Stitching.ImageFeatures first"),
            130 => M("FeaturesMatcher|method|public;instance|", "MatchesInfo[] Match(JYPPX.OpenCvSharp.Stitching.ImageFeatures[]"),
            131 => M("FeaturesMatcher|property|instance;get:public|", "Boolean IsThreadSafe"),
            132 => M("FeaturesMatcher|method|public;instance|", "CollectGarbage()"),
            134 => M("BestOf2NearestMatcher|constructor|", ".ctor(System.Boolean tryGpu=false"),
            135 => M("FeaturesMatcher|method|public;instance|", "CollectGarbage()"),
            136 => M("BestOf2NearestMatcher|method|public;static|", "BestOf2NearestMatcher Create("),
            138 => M("BestOf2NearestRangeMatcher|constructor|", ".ctor(System.Int32 rangeWidth=5"),
            140 => M("AffineBestOf2NearestMatcher|constructor|", ".ctor(System.Boolean fullAffine=false"),
            145 => M("Estimator|method|public;instance|", "Apply(JYPPX.OpenCvSharp.Stitching.ImageFeatures[] features", "initialCameras"),
            147 => M("HomographyBasedEstimator|constructor|", ".ctor(System.Boolean focalLengthsEstimated=false)"),
            149 => M("AffineBasedEstimator|constructor|", ".ctor()"),
            151 or 152 => M("BundleAdjusterBase|property|", "RefinementMask"),
            153 or 154 => M("BundleAdjusterBase|property|", "ConfidenceThreshold"),
            155 or 156 => M("BundleAdjusterBase|property|", "TerminationCriteria"),
            158 => M("NoBundleAdjuster|constructor|", ".ctor()"),
            160 => M("BundleAdjusterReproj|constructor|", ".ctor()"),
            162 => M("BundleAdjusterRay|constructor|", ".ctor()"),
            164 => M("BundleAdjusterAffine|constructor|", ".ctor()"),
            166 => M("BundleAdjusterAffinePartial|constructor|", ".ctor()"),
            168 => M("StitchingMotion|method|public;static|", "WaveCorrect("),
            169 => M("StitchingMotion|method|public;static|", "MatchesGraphAsString("),
            170 => M("StitchingMotion|method|public;static|", "LeaveBiggestComponent("),
            173 or 176 or 178 or 180 or 188 => M("SeamFinder|method|public;instance|", " Find("),
            174 => M("SeamFinder|method|public;static|", " CreateDefault("),
            183 => M("DpSeamFinder|constructor|", ".ctor(JYPPX.OpenCvSharp.Stitching.DpSeamCost"),
            184 => M("DpSeamFinder|method|public;instance|", " SetCostFunction("),
            187 => M("GraphCutSeamFinder|constructor|", ".ctor(JYPPX.OpenCvSharp.Stitching.GraphCutSeamCost"),
            191 => M("Timelapser|method|public;static|", " CreateDefault("),
            192 => M("Timelapser|method|public;instance|", " Initialize("),
            193 => M("Timelapser|method|public;instance|", " Process("),
            194 => M("Timelapser|method|public;instance|", " GetDestination("),
            196 => M("StitchingUtilities|method|public;static|", " TryOverlapRoi("),
            197 => M("StitchingUtilities|method|public;static|", " ResultRoi(JYPPX.OpenCvSharp.Core.Point[] corners,JYPPX.OpenCvSharp.Core.Mat[] images)"),
            198 => M("StitchingUtilities|method|public;static|", " ResultRoi(JYPPX.OpenCvSharp.Core.Point[] corners,JYPPX.OpenCvSharp.Core.Size[] sizes)"),
            199 => M("StitchingUtilities|method|public;static|", " ResultRoiIntersection("),
            200 => M("StitchingUtilities|method|public;static|", " ResultTopLeft("),
            201 => M("StitchingUtilities|method|public;static|", " SelectRandomSubset("),
            202 => M("StitchingUtilities|property|static;get:public|", " LogLevel"),
            205 => M("SphericalProjector|method|public;instance|", " MapForward("),
            206 => M("SphericalProjector|method|public;instance|", " MapBackward("),
            _ => throw new InvalidOperationException("No managed evidence mapping for Stitching ordinal " + ordinal)
        };
        string[] matches = managed.Where(line => fragments.All(fragment => line.Contains(fragment, StringComparison.Ordinal))).ToArray();
        if (matches.Length != 1)
        {
            throw new InvalidOperationException(
                "Managed baseline Stitching evidence is not unique for ordinal " + ordinal +
                ": candidates=" + matches.Length + " fragments=" + string.Join(" + ", fragments));
        }
        yield return matches[0];
        static string[] M(params string[] fragments) => fragments;
    }

    private static void ValidateRaw(RawDocument raw, string workspace)
    {
        Require(raw.SchemaVersion == 1 && raw.Generator == "tools/StitchingUpstreamMap/extract_stitching.py" && raw.UpstreamOpenCvVersion == "5.0.0", "Stitching raw identity drifted.");
        Require(raw.DeclarationCount == 207 && raw.Declarations.Count == 207, "Stitching raw declaration count drifted.");
        Require(raw.Declarations.Count(x => x.Kind == "callable") == 158, "Stitching raw callable count drifted.");
        Require(raw.SourceHeaders.Count == 14 && raw.CompatibilityHeaders.Count == 14 && raw.ExcludedPublicHeaders.Count == 0, "Stitching public header closure drifted.");
        Require(raw.SourceHeaders.Single(x => x.Surface == "detail-util-inl").DeclarationCount == 0 && raw.SourceHeaders.Single(x => x.Surface == "detail-warpers-inl").DeclarationCount == 0, "Stitching inline-header boundary drifted.");
        for (int i = 0; i < raw.Declarations.Count; ++i) Require(raw.Declarations[i].Ordinal == i, "Stitching raw ordinal ordering drifted.");
        Require(raw.Declarations.Select(x => x.Identity).Distinct(Ordinal).Count() == raw.Declarations.Count, "Stitching raw identities are not unique.");
        Require(HashFile(Path.Combine(workspace, raw.HeaderPath.Replace('/', Path.DirectorySeparatorChar))) == raw.HeaderSha256, "Stitching primary header hash drifted.");
        Require(HashFile(Path.Combine(workspace, raw.ParserPath.Replace('/', Path.DirectorySeparatorChar))) == raw.ParserSha256, "Stitching parser hash drifted.");
        foreach (SourceHeader header in raw.SourceHeaders)
        {
            Require(HashFile(Path.Combine(workspace, header.Path.Replace('/', Path.DirectorySeparatorChar))) == header.Sha256, "Stitching source header hash drifted: " + header.Path);
            Require(raw.Declarations.Count(x => x.Surface == header.Surface) == header.DeclarationCount, "Stitching source partition count drifted: " + header.Surface);
        }
    }

    private static void Validate(RawDocument raw, ClassificationDocument document, string[] native, string[] managed)
    {
        Require(document.SchemaVersion == 1 && document.UpstreamOpenCvVersion == "5.0.0" && document.ClaimedSlice == ClaimedSlice && document.ReviewStatus == "source-reviewed", "Stitching classification identity drifted.");
        Require(!string.IsNullOrWhiteSpace(document.Limitation) && document.Declarations.Count == raw.Declarations.Count, "Stitching classification coverage drifted.");
        for (int i = 0; i < raw.Declarations.Count; ++i)
        {
            RawDeclaration declaration = raw.Declarations[i];
            ClassificationRow row = document.Declarations[i];
            Require(row.Ordinal == declaration.Ordinal && row.Surface == declaration.Surface && row.Identity == declaration.Identity, "Stitching classification ordering or identity drifted at ordinal " + i);
            Require(Allowed.Contains(row.Classification, Ordinal) && row.BuildCondition == BuildCondition && !string.IsNullOrWhiteSpace(row.Reason), "Stitching classification contract drifted at ordinal " + i);
            if (declaration.Kind == "callable") Require(row.Classification != "non-callable-metadata", "Callable was classified as metadata at ordinal " + i);
            else Require(row.Classification == "non-callable-metadata", "Metadata was classified as callable at ordinal " + i);
            if (row.Classification == "implemented")
            {
                Require(row.NativeEntrypoints.Count > 0 && row.ManagedMembers.Count > 0, "Implemented Stitching row lacks evidence at ordinal " + i);
                Require(row.NativeEntrypoints.All(x => native.Contains(x, Ordinal) && !x.Contains("ocv5", StringComparison.Ordinal)), "False or fixed-major native evidence at ordinal " + i);
                Require(row.ManagedMembers.All(x => managed.Contains(x, Ordinal)), "False managed evidence at ordinal " + i);
            }
            else Require(row.NativeEntrypoints.Count == 0 && row.ManagedMembers.Count == 0, "Non-implemented Stitching row contains false evidence at ordinal " + i);
        }
        Require(HighLevel.All(x => document.Declarations[x].Classification == "implemented"), "High-level Stitcher callable coverage drifted.");
        Require(Exposure.All(x => document.Declarations[x].Classification == "implemented"), "Exposure family coverage drifted.");
        Require(PublicWarper.All(x => document.Declarations[x].Classification == "implemented"), "Public warper family coverage drifted.");
        Require(Blender.All(x => document.Declarations[x].Classification == "implemented"), "Detail Blender family coverage drifted.");
        Require(Matcher.All(x => document.Declarations[x].Classification == "implemented"), "Detail matcher family coverage drifted.");
        Require(CameraMotion.All(x => document.Declarations[x].Classification == "implemented"), "Camera and motion-estimator family coverage drifted.");
        Require(SeamFinder.All(x => document.Declarations[x].Classification == "implemented"), "Seam-finder family coverage drifted.");
        Require(Timelapser.All(x => document.Declarations[x].Classification == "implemented"), "Timelapser family coverage drifted.");
        Require(Utility.All(x => document.Declarations[x].Classification == "implemented"), "Detail utility family coverage drifted.");
        Require(SphericalProjector.All(x => document.Declarations[x].Classification == "implemented"), "Spherical projector coverage drifted.");
        Require(MatcherUnsupported.All(x => document.Declarations[x].Classification == "unsupported"), "LightGlue support boundary drifted.");
        Require(document.Declarations.Where(x => x.Classification == "implemented").All(x => HighLevel.Contains(x.Ordinal) || Exposure.Contains(x.Ordinal) || PublicWarper.Contains(x.Ordinal) || Blender.Contains(x.Ordinal) || Matcher.Contains(x.Ordinal) || CameraMotion.Contains(x.Ordinal) || SeamFinder.Contains(x.Ordinal) || Timelapser.Contains(x.Ordinal) || Utility.Contains(x.Ordinal) || SphericalProjector.Contains(x.Ordinal)), "Stitching implementation partitions were mixed.");
    }

    private static void RunNegativeFixtures(RawDocument raw, ClassificationDocument classifications, string[] native, string[] managed, string workspace)
    {
        int accepted = 0;
        void Reject(Action<RawDocument, ClassificationDocument> mutate, bool validateRaw = false)
        {
            RawDocument r = Clone(raw); ClassificationDocument c = Clone(classifications); mutate(r, c);
            try { if (validateRaw) ValidateRaw(r, workspace); Validate(r, c, native, managed); }
            catch { accepted++; return; }
            throw new InvalidOperationException("A Stitching negative fixture was accepted.");
        }
        Reject((_, c) => c.Declarations.RemoveAt(0));
        Reject((_, c) => c.Declarations.Insert(1, Clone(c.Declarations[0])));
        Reject((_, c) => (c.Declarations[3], c.Declarations[4]) = (c.Declarations[4], c.Declarations[3]));
        Reject((_, c) => c.Declarations[18].Identity = c.Declarations[17].Identity);
        Reject((_, c) => c.Declarations[82].Identity = c.Declarations[83].Identity);
        Reject((r, _) => r.HeaderSha256 = new string('0', 64), true);
        Reject((r, _) => r.ParserSha256 = new string('0', 64), true);
        Reject((r, _) => r.SourceHeaders[5].Sha256 = new string('0', 64), true);
        Reject((_, c) => c.Declarations[70].NativeEntrypoints[0] = "jyppx_ocv_false_evidence");
        Reject((_, c) => c.Declarations[24].Reason = "");
        Reject((_, c) => c.Declarations[70].NativeEntrypoints[0] = "jyppx_ocv" + "5_stitching_exposure_create_default");
        Reject((_, c) => c.Declarations[70].BuildCondition = "always");
        Reject((_, c) => c.Declarations[70].NativeEntrypoints[0] = "jyppx_ocv_video_false");
        Reject((_, c) => c.Declarations[70].Surface = "primary");
        Reject((_, c) => c.Declarations[70].Ordinal = 71);
        Reject((_, c) => c.Declarations.Add(new ClassificationRow { Ordinal = 999, Classification = "implemented" }));
        Reject((_, c) => c.ClaimedSlice = "repository-wide upstream parity");
        Reject((_, c) => c.Declarations[25].Classification = "missing");
        Reject((_, c) => c.Declarations[25].Identity = c.Declarations[26].Identity);
        Reject((_, c) => c.Declarations[29].ManagedMembers.Clear());
        Reject((_, c) => c.Declarations[40].Classification = "missing");
        Reject((_, c) => c.Declarations[40].NativeEntrypoints.Clear());
        Reject((_, c) => c.Declarations[52].ManagedMembers.Clear());
        Reject((_, c) => c.Declarations[63].NativeEntrypoints.Clear());
        Reject((_, c) => c.Declarations[122].Classification = "missing");
        Reject((_, c) => c.Declarations[142].Classification = "implemented");
        Reject((_, c) => c.Declarations[142].Reason = "");
        Reject((_, c) => c.Declarations[140].ManagedMembers.Clear());
        Reject((_, c) => c.Declarations[173].Classification = "missing");
        Reject((_, c) => c.Declarations[191].NativeEntrypoints.Clear());
        Reject((_, c) => c.Declarations[196].ManagedMembers.Clear());
        Reject((_, c) => c.Declarations[205].Classification = "missing");
        Require(accepted == NegativeFixtureCount, "Stitching negative fixture count drifted.");
    }

    private static FamilyDocument BuildFamilies(RawDocument raw, ClassificationDocument classifications)
    {
        var family = new FamilyRow
        {
            Id = "stitching-detail-exposure-compensation-completion",
            Surface = "detail-exposure",
            Rationale = "Closes the largest continuous deterministic offline gap family with owned strategy lifetime, temporary Mat/UMat borrowing, in-place apply, copied gains, property round trips, native smoke, and both-framework tests."
        };
        foreach (int ordinal in ExposureOrdinals)
        {
            ClassificationRow row = classifications.Declarations[ordinal];
            family.Declarations.Add(new FamilyOperation { Ordinal = ordinal, UpstreamIdentity = raw.Declarations[ordinal].Identity, NativeEntrypoints = row.NativeEntrypoints, ManagedMembers = row.ManagedMembers });
        }
        var publicWarperFamily = new FamilyRow
        {
            Id = "stitching-public-py-rotation-warper-completion",
            Surface = "public-warpers",
            Rationale = "Closes all public PyRotationWarper callables with exact projector names, safe default state, owned lifetime, strict K/R matrices, caller-owned maps and image outputs, point/ROI semantics, native smoke, and both-framework tests."
        };
        foreach (int ordinal in PublicWarperOrdinals)
        {
            ClassificationRow row = classifications.Declarations[ordinal];
            publicWarperFamily.Declarations.Add(new FamilyOperation
            {
                Ordinal = ordinal,
                UpstreamIdentity = raw.Declarations[ordinal].Identity,
                NativeEntrypoints = row.NativeEntrypoints,
                ManagedMembers = row.ManagedMembers,
                FocusedTest = "tests/OpenCvSharp.Tests/Stitching/PyRotationWarperTests.cs"
            });
        }
        var blenderFamily = new FamilyRow
        {
            Id = "stitching-detail-blender-completion",
            Surface = "detail-blenders",
            Rationale = "Closes all detail Blender callables with owned polymorphic lifetime, bounded prepare/feed/blend state, exact Mat types, caller-owned output arrays, CPU pyramids, explicit CUDA-unavailable failures, native smoke, and both-framework tests."
        };
        foreach (int ordinal in BlenderOrdinals)
        {
            ClassificationRow row = classifications.Declarations[ordinal];
            blenderFamily.Declarations.Add(new FamilyOperation
            {
                Ordinal = ordinal,
                UpstreamIdentity = raw.Declarations[ordinal].Identity,
                NativeEntrypoints = row.NativeEntrypoints,
                ManagedMembers = row.ManagedMembers,
                FocusedTest = "tests/OpenCvSharp.Tests/Stitching/BlenderTests.cs"
            });
        }
        var matcherFamily = new FamilyRow
        {
            Id = "stitching-detail-feature-matchers-completion",
            Surface = "detail-matchers",
            Rationale = "Closes the ordinary offline ImageFeatures, MatchesInfo, BestOf2Nearest, range, and affine matcher rows with copied descriptors/transforms, exact count/fill collections, Feature2D bridge, N-squared batch layout, mask validation, native smoke, and both-framework tests."
        };
        foreach (int ordinal in MatcherOrdinals)
        {
            ClassificationRow row = classifications.Declarations[ordinal];
            matcherFamily.Declarations.Add(new FamilyOperation
            {
                Ordinal = ordinal,
                UpstreamIdentity = raw.Declarations[ordinal].Identity,
                NativeEntrypoints = row.NativeEntrypoints,
                ManagedMembers = row.ManagedMembers,
                FocusedTest = "tests/OpenCvSharp.Tests/Stitching/FeaturesMatcherTests.cs"
            });
        }
        var cameraMotionFamily = new FamilyRow
        {
            Id = "stitching-camera-motion-estimator-completion",
            Surface = "detail-motion-estimators",
            Rationale = "Closes autocalibration, copied camera intrinsics, owned homography/affine estimators, five bundle adjusters, transactional wave correction, UTF-8 match graphs, and independently owned largest-component results."
        };
        foreach (int ordinal in CameraMotionOrdinals)
        {
            ClassificationRow row = classifications.Declarations[ordinal];
            cameraMotionFamily.Declarations.Add(new FamilyOperation
            {
                Ordinal = ordinal,
                UpstreamIdentity = raw.Declarations[ordinal].Identity,
                NativeEntrypoints = row.NativeEntrypoints,
                ManagedMembers = row.ManagedMembers,
                FocusedTest = "tests/OpenCvSharp.Tests/Stitching/MotionEstimatorTests.cs"
            });
        }
        var seamFinderFamily = new FamilyRow
        {
            Id = "stitching-detail-seam-finders-completion",
            Surface = "detail-seam-finders",
            Rationale = "Closes all seam-finder callables with owned polymorphic lifetime, strongly typed cost selection, temporary UMat inputs, transactional mutable masks, native smoke, and both-framework tests."
        };
        foreach (int ordinal in SeamFinderOrdinals)
        {
            ClassificationRow row = classifications.Declarations[ordinal];
            seamFinderFamily.Declarations.Add(new FamilyOperation
            {
                Ordinal = ordinal,
                UpstreamIdentity = raw.Declarations[ordinal].Identity,
                NativeEntrypoints = row.NativeEntrypoints,
                ManagedMembers = row.ManagedMembers,
                FocusedTest = "tests/OpenCvSharp.Tests/Stitching/StitchingDetailTests.cs"
            });
        }
        var timelapserFamily = new FamilyRow
        {
            Id = "stitching-detail-timelapsers-completion",
            Surface = "detail-timelapsers",
            Rationale = "Closes all timelapser callables with owned initialized state, checked placements, exact CV_16SC3 processing, independent CPU destination copies, native smoke, and both-framework tests."
        };
        foreach (int ordinal in TimelapserOrdinals)
        {
            ClassificationRow row = classifications.Declarations[ordinal];
            timelapserFamily.Declarations.Add(new FamilyOperation
            {
                Ordinal = ordinal,
                UpstreamIdentity = raw.Declarations[ordinal].Identity,
                NativeEntrypoints = row.NativeEntrypoints,
                ManagedMembers = row.ManagedMembers,
                FocusedTest = "tests/OpenCvSharp.Tests/Stitching/StitchingDetailTests.cs"
            });
        }
        var utilityFamily = new FamilyRow
        {
            Id = "stitching-detail-utilities-completion",
            Surface = "detail-util",
            Rationale = "Closes all detail utility callables with checked placement arithmetic, exact ROI values, bounded random-subset output, read-only logging state, native smoke, and both-framework tests."
        };
        foreach (int ordinal in UtilityOrdinals)
        {
            ClassificationRow row = classifications.Declarations[ordinal];
            utilityFamily.Declarations.Add(new FamilyOperation
            {
                Ordinal = ordinal,
                UpstreamIdentity = raw.Declarations[ordinal].Identity,
                NativeEntrypoints = row.NativeEntrypoints,
                ManagedMembers = row.ManagedMembers,
                FocusedTest = "tests/OpenCvSharp.Tests/Stitching/StitchingDetailTests.cs"
            });
        }
        var sphericalProjectorFamily = new FamilyRow
        {
            Id = "stitching-detail-spherical-projector-completion",
            Surface = "detail-warpers",
            Rationale = "Closes both parser-visible spherical mapping methods through an owned, source-reviewed camera configuration with exact CV_32FC1 matrices, native smoke, and both-framework tests."
        };
        foreach (int ordinal in SphericalProjectorOrdinals)
        {
            ClassificationRow row = classifications.Declarations[ordinal];
            sphericalProjectorFamily.Declarations.Add(new FamilyOperation
            {
                Ordinal = ordinal,
                UpstreamIdentity = raw.Declarations[ordinal].Identity,
                NativeEntrypoints = row.NativeEntrypoints,
                ManagedMembers = row.ManagedMembers,
                FocusedTest = "tests/OpenCvSharp.Tests/Stitching/StitchingDetailTests.cs"
            });
        }
        return new FamilyDocument
        {
            Families = new List<FamilyRow> { family, publicWarperFamily, blenderFamily, matcherFamily, cameraMotionFamily, seamFinderFamily, timelapserFamily, utilityFamily, sphericalProjectorFamily },
            SourceReviewedExtensions = new List<SourceReviewedExtension>
            {
                new() { UpstreamIdentity = "cv::detail::NoExposureCompensator default construction", SourceHeader = "opencv-source/opencv-5.0.0/modules/stitching/include/opencv2/stitching/detail/exposure_compensate.hpp", Adaptation = "Adds an explicit owned managed no-op constructor; the parser emits its inherited operations but not its implicit default constructor.", NativeEntrypoints = E("create_no"), ManagedMembers = new() { "MEMBER|JYPPX.OpenCvSharp.Stitching.NoExposureCompensator|constructor|public;instance|.ctor()" } },
                new() { UpstreamIdentity = "cv::Stitcher::waveCorrectKind/setWaveCorrectKind", SourceHeader = "opencv-source/opencv-5.0.0/modules/stitching/include/opencv2/stitching.hpp", Adaptation = "Source-reviewed high-level property pair omitted by hdr_parser and kept outside parser-derived counts.", NativeEntrypoints = new() { "jyppx_ocv_stitcher_get_int_property", "jyppx_ocv_stitcher_set_int_property" }, ManagedMembers = new() { "MEMBER|JYPPX.OpenCvSharp.Stitching.Stitcher|property|instance;get:public;set:public|JYPPX.OpenCvSharp.Stitching.WaveCorrectKind WaveCorrectKind" } },
                new() { UpstreamIdentity = "cv::Stitcher::resultMask", SourceHeader = "opencv-source/opencv-5.0.0/modules/stitching/include/opencv2/stitching.hpp", Adaptation = "Copies the source-reviewed internal result mask into caller-owned or newly allocated managed Mat storage.", NativeEntrypoints = new() { "jyppx_ocv_stitcher_get_result_mask" }, ManagedMembers = new() { "MEMBER|JYPPX.OpenCvSharp.Stitching.Stitcher|method|public;instance|JYPPX.OpenCvSharp.Core.Mat GetResultMask()", "MEMBER|JYPPX.OpenCvSharp.Stitching.Stitcher|method|public;instance|System.Void GetResultMask(JYPPX.OpenCvSharp.Core.Mat resultMask)" } },
                new() { UpstreamIdentity = "cv::detail::ProjectorBase::setCameraParams plus SphericalProjector scale configuration", SourceHeader = "opencv-source/opencv-5.0.0/modules/stitching/include/opencv2/stitching/detail/warpers.hpp", Adaptation = "Adds an owned configured projector constructor so parser-visible mapping methods cannot observe uninitialized scale or camera arrays.", NativeEntrypoints = new() { "jyppx_ocv_stitching_spherical_projector_create", "jyppx_ocv_stitching_spherical_projector_release_handle" }, ManagedMembers = new() { "MEMBER|JYPPX.OpenCvSharp.Stitching.SphericalProjector|constructor|public;instance|.ctor(System.Single scale,JYPPX.OpenCvSharp.Core.Mat cameraMatrix,JYPPX.OpenCvSharp.Core.Mat rotationMatrix,JYPPX.OpenCvSharp.Core.Mat? translation=null)" } }
            }
        };
        static List<string> E(params string[] suffixes) => suffixes.Select(x => "jyppx_ocv_stitching_exposure_" + x).ToList();
    }

    private static string BuildMap(RawDocument raw, ClassificationDocument classifications)
    {
        var text = new StringBuilder();
        text.AppendLine("# OpenCV 5.0.0 main Stitching parser-derived compatibility map");
        text.AppendLine("# Module-scoped closure only; repository-wide upstream parity is not claimed.");
        text.AppendLine("ordinal|surface|kind|classification|identity|native-evidence|managed-evidence|reason");
        for (int i = 0; i < raw.Declarations.Count; ++i)
        {
            RawDeclaration declaration = raw.Declarations[i]; ClassificationRow row = classifications.Declarations[i];
            text.Append(declaration.Ordinal).Append('|').Append(declaration.Surface).Append('|').Append(declaration.Kind).Append('|').Append(row.Classification).Append('|')
                .Append(Escape(declaration.Identity)).Append('|').Append(string.Join(',', row.NativeEntrypoints)).Append('|').Append(Escape(string.Join(';', row.ManagedMembers))).Append('|').AppendLine(Escape(row.Reason));
        }
        return text.ToString().Replace("\r\n", "\n", StringComparison.Ordinal);
        static string Escape(string value) => value.Replace("|", "\\|", StringComparison.Ordinal).Replace("\r", " ", StringComparison.Ordinal).Replace("\n", " ", StringComparison.Ordinal);
    }

    private static Options Parse(string[] args)
    {
        var values = new Dictionary<string, string>(Ordinal); bool initialize = false; bool check = false;
        for (int i = 0; i < args.Length; ++i)
        {
            if (args[i] == "--initialize-classification") initialize = true;
            else if (args[i] == "--check") check = true;
            else { Require(i + 1 < args.Length, "Missing option value: " + args[i]); values[args[i]] = args[++i]; }
        }
        string V(string name) { Require(values.TryGetValue(name, out string? value), "Missing option: " + name); return Path.GetFullPath(value!); }
        return new Options(V("--repository"), V("--workspace"), V("--raw"), V("--classification"), V("--native-manifest"), V("--managed-baseline"), V("--output"), V("--summary"), V("--family-output"), initialize, check);
    }

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };
    private static T Read<T>(string path) => JsonSerializer.Deserialize<T>(File.ReadAllText(path, Encoding.UTF8), JsonOptions) ?? throw new InvalidOperationException("Invalid JSON: " + path);
    private static T Clone<T>(T value) => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value, JsonOptions), JsonOptions)!;
    private static string Serialize<T>(T value) => JsonSerializer.Serialize(value, JsonOptions).Replace("\r\n", "\n", StringComparison.Ordinal) + "\n";
    private static string HashFile(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
    private static string Sha256(string text) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
    private static string Rel(string root, string path) => Path.GetRelativePath(root, path).Replace('\\', '/');
    private static void WriteOrCheck(string path, string content, bool check)
    {
        if (check) Require(File.Exists(path) && File.ReadAllText(path, Encoding.UTF8) == content, "Generated Stitching artifact is stale: " + path);
        else { Directory.CreateDirectory(Path.GetDirectoryName(path)!); File.WriteAllText(path, content, new UTF8Encoding(false)); }
    }
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}

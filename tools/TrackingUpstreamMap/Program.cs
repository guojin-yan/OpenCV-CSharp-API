using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

internal static class Program
{
    private static readonly StringComparer Ordinal = StringComparer.Ordinal;
    private static readonly HashSet<int> Selected = new(new[] { 16, 20, 22, 32, 33, 34 });
    private static readonly string[] Allowed =
    {
        "implemented", "missing", "intentionally-omitted", "upstream-conditional", "unsupported", "non-callable-metadata"
    };
    private const string ClaimedSlice = "opencv2/tracking.hpp primary contrib tracker declarations plus separately partitioned public opencv2/tracking/tracking_legacy.hpp declarations";
    private const int NegativeFixtureCount = 17;
    private const int ManagedTypeAdditions = 5;
    private const int ManagedMemberAdditions = 23;
    private const int NativeEntrypointAdditions = 10;

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

    private sealed class CompatibilityHeader
    {
        public string Path { get; set; } = "";
        public string Sha256 { get; set; } = "";
        public string Includes { get; set; } = "";
    }

    private sealed class ExcludedPublicHeader
    {
        public string Path { get; set; } = "";
        public string Reason { get; set; } = "";
    }

    private sealed class SourceHeader
    {
        public string Surface { get; set; } = "";
        public string Path { get; set; } = "";
        public string Sha256 { get; set; } = "";
        public int StartOrdinal { get; set; }
        public int DeclarationCount { get; set; }
    }

    private sealed class RawDeclaration
    {
        public int Ordinal { get; set; }
        public string Surface { get; set; } = "";
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
        public string ReviewStatus { get; set; } = "source-reviewed";
        public string Limitation { get; set; } = "Primary contrib and public legacy declarations are counted separately. Main Video, external-model, callback, and detail/internal surfaces are not claimed.";
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

    private sealed class SummaryDocument
    {
        public int SchemaVersion { get; init; } = 1;
        public string Generator { get; init; } = "tools/TrackingUpstreamMap";
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
        public int PrimaryDeclarationCount { get; init; }
        public int PrimaryCallableCount { get; init; }
        public int LegacyDeclarationCount { get; init; }
        public int LegacyCallableCount { get; init; }
        public SortedDictionary<string, int> ClassificationCounts { get; init; } = new(Ordinal);
        public int NativeEvidenceCount { get; init; }
        public int ManagedEvidenceCount { get; init; }
        public int NegativeFixtureCount { get; init; } = Program.NegativeFixtureCount;
        public string FamilyInventoryPath { get; init; } = "";
        public string FamilyInventorySha256 { get; init; } = "";
        public int SelectedFamilyCount { get; init; }
        public int SelectedDeclarationCount { get; init; }
        public int SourceReviewedExtensionCount { get; init; }
        public int ManagedPublicTypeAdditionCount { get; init; } = ManagedTypeAdditions;
        public int ManagedPublicMemberAdditionCount { get; init; } = ManagedMemberAdditions;
        public int NativeEntrypointAdditionCount { get; init; } = NativeEntrypointAdditions;
        public bool MainVideoRowsDoubleCounted { get; init; }
        public bool LegacyRowsMixedIntoPrimary { get; init; }
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
        public string Surface { get; init; } = "";
        public string Rationale { get; init; } = "";
        public List<FamilyOperation> Declarations { get; init; } = new();
    }

    private sealed class FamilyOperation
    {
        public int Ordinal { get; init; }
        public string UpstreamIdentity { get; init; } = "";
        public List<string> NativeEntrypoints { get; init; } = new();
        public List<string> ManagedMembers { get; init; } = new();
        public string FocusedTest { get; init; } = "tests/OpenCvSharp.Tests/Tracking/TrackingTests.cs";
        public string NativeSmoke { get; init; } = "src/OpenCvSharp.Native/tests/native_smoke.cpp";
        public string Sample { get; init; } = "samples/ConsoleSamples/Program.cs";
        public string Guide { get; init; } = "docs/articles/tracking-guide.md";
    }

    private sealed class SourceReviewedExtension
    {
        public string UpstreamIdentity { get; init; } = "";
        public string SourceHeader { get; init; } = "opencv-source/opencv_contrib-5.0.0/modules/tracking/include/opencv2/tracking/tracking_legacy.hpp";
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
            if (options.Initialize)
            {
                WriteOrCheck(options.Classification, Serialize(Initialize(raw, native, managed)), false);
            }

            ClassificationDocument classifications = Read<ClassificationDocument>(options.Classification);
            Validate(raw, classifications, options, native, managed, true, false);
            string mapping = BuildMap(raw, classifications);
            string familyText = Serialize(BuildFamilies(raw, classifications, managed));
            var counts = new SortedDictionary<string, int>(Ordinal);
            foreach (string value in Allowed)
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
                ExcludedPublicHeaderCount = raw.ExcludedPublicHeaders.Count,
                SourceHeaderCount = raw.SourceHeaders.Count,
                SourceHeaderSetSha256 = Sha256(string.Join("\n", raw.SourceHeaders.Select(x => $"{x.Surface}|{x.Path}|{x.Sha256}|{x.StartOrdinal}|{x.DeclarationCount}")) + "\n"),
                MappingSha256 = Sha256(mapping),
                DeclarationCount = raw.Declarations.Count,
                EnumCount = raw.Declarations.Count(x => x.Kind == "enum"),
                ClassCount = raw.Declarations.Count(x => x.Kind == "class"),
                CallableCount = raw.Declarations.Count(x => x.Kind == "callable"),
                PrimaryDeclarationCount = raw.Declarations.Count(x => x.Surface == "primary"),
                PrimaryCallableCount = raw.Declarations.Count(x => x.Surface == "primary" && x.Kind == "callable"),
                LegacyDeclarationCount = raw.Declarations.Count(x => x.Surface == "legacy"),
                LegacyCallableCount = raw.Declarations.Count(x => x.Surface == "legacy" && x.Kind == "callable"),
                ClassificationCounts = counts,
                NativeEvidenceCount = classifications.Declarations.SelectMany(x => x.NativeEntrypoints).Distinct(Ordinal).Count(),
                ManagedEvidenceCount = classifications.Declarations.SelectMany(x => x.ManagedMembers).Distinct(Ordinal).Count(),
                FamilyInventoryPath = Rel(options.Repository, options.FamilyOutput),
                FamilyInventorySha256 = Sha256(familyText),
                SelectedFamilyCount = 1,
                SelectedDeclarationCount = Selected.Count,
                SourceReviewedExtensionCount = 4,
                MainVideoRowsDoubleCounted = false,
                LegacyRowsMixedIntoPrimary = false,
                RepositoryWideUpstreamParityClaimed = false
            };
            RunNegativeFixtures(raw, classifications, options, native, managed);
            WriteOrCheck(options.Output, mapping, options.Check);
            WriteOrCheck(options.FamilyOutput, familyText, options.Check);
            WriteOrCheck(options.Summary, Serialize(summary), options.Check);
            Console.WriteLine($"TRACKING_UPSTREAM_MAP_OK declarations={summary.DeclarationCount} primary={summary.PrimaryDeclarationCount}/{summary.PrimaryCallableCount} legacy={summary.LegacyDeclarationCount}/{summary.LegacyCallableCount} implemented={counts["implemented"]} missing={counts["missing"]} fixtures={NegativeFixtureCount} sha256={summary.MappingSha256} mode={(options.Check ? "check" : "write")}");
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

        return new Options(
            Value("--repository"),
            Value("--workspace"),
            Value("--raw"),
            Value("--classification"),
            Value("--native-manifest"),
            Value("--managed-baseline"),
            Value("--output"),
            Value("--summary"),
            Value("--family-output"),
            initialize,
            check);
    }

    private static ClassificationDocument Initialize(RawDocument raw, string[] native, string[] managed)
    {
        var result = new ClassificationDocument();
        foreach (RawDeclaration declaration in raw.Declarations)
        {
            var row = new ClassificationRow
            {
                Ordinal = declaration.Ordinal,
                Surface = declaration.Surface,
                Identity = declaration.Identity,
                BuildCondition = "OPENCV_CSHARP_HAS_OPENCV_TRACKING; full-profile; mini-excluded"
            };
            if (declaration.Kind != "callable")
            {
                row.Classification = "non-callable-metadata";
                row.Reason = "Parser-emitted class, struct, or enum metadata is reviewed as public type shape rather than an independent ABI operation.";
            }
            else
            {
                row.Classification = "implemented";
                row.Reason = Selected.Contains(declaration.Ordinal)
                    ? "The selected public legacy model-free completion batch provides opaque ownership, guarded lifecycle behavior, native smoke, and net8/net10 managed tests."
                    : "The existing version-neutral contrib Tracking native and managed surface implements this parser row.";
                row.NativeEntrypoints.AddRange(NativeEvidence(declaration.Ordinal, native));
                row.ManagedMembers.AddRange(ManagedEvidence(declaration.Ordinal, managed));
            }

            row.NativeEntrypoints = row.NativeEntrypoints.Distinct(Ordinal).OrderBy(x => x, Ordinal).ToList();
            row.ManagedMembers = row.ManagedMembers.Distinct(Ordinal).OrderBy(x => x, Ordinal).ToList();
            result.Declarations.Add(row);
        }

        Validate(raw, result, null, native, managed, false, false);
        return result;
    }

    private static IEnumerable<string> NativeEvidence(int ordinal, string[] native)
    {
        string[] names = ordinal switch
        {
            2 => new[] { "jyppx_ocv_tracking_tracker_csrt_get_default_params" },
            3 => new[] { "jyppx_ocv_tracking_tracker_csrt_create", "jyppx_ocv_tracking_tracker_csrt_create_default" },
            4 => new[] { "jyppx_ocv_tracking_tracker_csrt_set_initial_mask" },
            8 => new[] { "jyppx_ocv_tracking_tracker_kcf_get_default_params" },
            9 => new[] { "jyppx_ocv_tracking_tracker_kcf_create", "jyppx_ocv_tracking_tracker_kcf_create_default" },
            11 => new[] { "jyppx_ocv_tracking_legacy_tracker_init" },
            12 => new[] { "jyppx_ocv_tracking_legacy_tracker_update" },
            14 => new[] { "jyppx_ocv_tracking_legacy_tracker_mil_create_default" },
            16 => new[] { "jyppx_ocv_tracking_legacy_tracker_boosting_create_default" },
            18 => new[] { "jyppx_ocv_tracking_legacy_tracker_median_flow_create_default" },
            20 => new[] { "jyppx_ocv_tracking_legacy_tracker_tld_create" },
            22 => new[] { "jyppx_ocv_tracking_legacy_tracker_kcf_create_default" },
            24 => new[] { "jyppx_ocv_tracking_legacy_tracker_mosse_create" },
            26 or 30 => new[] { "jyppx_ocv_tracking_legacy_multi_tracker_create" },
            27 => new[] { "jyppx_ocv_tracking_legacy_multi_tracker_add" },
            28 => new[] { "jyppx_ocv_tracking_legacy_multi_tracker_update_count", "jyppx_ocv_tracking_legacy_multi_tracker_update_fill" },
            29 => new[] { "jyppx_ocv_tracking_legacy_multi_tracker_get_objects_count", "jyppx_ocv_tracking_legacy_multi_tracker_get_objects_fill" },
            32 => new[] { "jyppx_ocv_tracking_legacy_tracker_csrt_create_default" },
            33 => new[] { "jyppx_ocv_tracking_legacy_tracker_csrt_set_initial_mask" },
            34 => new[] { "jyppx_ocv_tracking_legacy_upgrade" },
            _ => throw new InvalidOperationException("No native evidence mapping for callable ordinal " + ordinal)
        };
        foreach (string name in names)
        {
            Require(native.Contains(name, Ordinal), "Native manifest is missing Tracking evidence: " + name);
            yield return name;
        }
    }

    private static IEnumerable<string> ManagedEvidence(int ordinal, string[] managed)
    {
        string[] fragments = ordinal switch
        {
            2 => new[] { "MEMBER|JYPPX.OpenCvSharp.Tracking.TrackerCSRTParams|constructor|" },
            3 => new[] { "MEMBER|JYPPX.OpenCvSharp.Tracking.TrackerCSRT|method|public;static|JYPPX.OpenCvSharp.Tracking.TrackerCSRT Create()" },
            4 => new[] { "MEMBER|JYPPX.OpenCvSharp.Tracking.TrackerCSRT|method|public;instance|System.Void SetInitialMask" },
            8 => new[] { "MEMBER|JYPPX.OpenCvSharp.Tracking.TrackerKCFParams|constructor|" },
            9 => new[] { "MEMBER|JYPPX.OpenCvSharp.Tracking.TrackerKCF|method|public;static|JYPPX.OpenCvSharp.Tracking.TrackerKCF Create()" },
            11 => new[] { "MEMBER|JYPPX.OpenCvSharp.Tracking.Legacy.LegacyTracker|method|public;instance|System.Void Init" },
            12 => new[] { "MEMBER|JYPPX.OpenCvSharp.Tracking.Legacy.LegacyTracker|method|public;instance|System.Boolean Update" },
            14 => new[] { "MEMBER|JYPPX.OpenCvSharp.Tracking.Legacy.TrackerMIL|method|public;static|JYPPX.OpenCvSharp.Tracking.Legacy.TrackerMIL Create()" },
            16 => new[] { "MEMBER|JYPPX.OpenCvSharp.Tracking.Legacy.TrackerBoosting|method|public;static|JYPPX.OpenCvSharp.Tracking.Legacy.TrackerBoosting Create()" },
            18 => new[] { "MEMBER|JYPPX.OpenCvSharp.Tracking.Legacy.TrackerMedianFlow|method|public;static|JYPPX.OpenCvSharp.Tracking.Legacy.TrackerMedianFlow Create()" },
            20 => new[] { "MEMBER|JYPPX.OpenCvSharp.Tracking.Legacy.TrackerTLD|method|public;static|JYPPX.OpenCvSharp.Tracking.Legacy.TrackerTLD Create()" },
            22 => new[] { "MEMBER|JYPPX.OpenCvSharp.Tracking.Legacy.TrackerKCF|method|public;static|JYPPX.OpenCvSharp.Tracking.Legacy.TrackerKCF Create()" },
            24 => new[] { "MEMBER|JYPPX.OpenCvSharp.Tracking.Legacy.TrackerMOSSE|method|public;static|JYPPX.OpenCvSharp.Tracking.Legacy.TrackerMOSSE Create()" },
            26 or 30 => new[] { "MEMBER|JYPPX.OpenCvSharp.Tracking.Legacy.MultiTracker|method|public;static|JYPPX.OpenCvSharp.Tracking.Legacy.MultiTracker Create()" },
            27 => new[] { "MEMBER|JYPPX.OpenCvSharp.Tracking.Legacy.MultiTracker|method|public;instance|System.Boolean Add" },
            28 => new[] { "MEMBER|JYPPX.OpenCvSharp.Tracking.Legacy.MultiTracker|method|public;instance|JYPPX.OpenCvSharp.Tracking.Legacy.LegacyMultiTrackerUpdateResult Update" },
            29 => new[] { "MEMBER|JYPPX.OpenCvSharp.Tracking.Legacy.MultiTracker|method|public;instance|JYPPX.OpenCvSharp.Core.Rect2d[] GetObjects()" },
            32 => new[] { "MEMBER|JYPPX.OpenCvSharp.Tracking.Legacy.TrackerCSRT|method|public;static|JYPPX.OpenCvSharp.Tracking.Legacy.TrackerCSRT Create()" },
            33 => new[] { "MEMBER|JYPPX.OpenCvSharp.Tracking.Legacy.TrackerCSRT|method|public;instance|System.Void SetInitialMask" },
            34 => new[] { "MEMBER|JYPPX.OpenCvSharp.Tracking.Legacy.LegacyTracker|method|public;instance|JYPPX.OpenCvSharp.Tracking.Tracker Upgrade()" },
            _ => throw new InvalidOperationException("No managed evidence mapping for callable ordinal " + ordinal)
        };
        foreach (string fragment in fragments)
        {
            string[] matches = managed.Where(line => line.Contains(fragment, StringComparison.Ordinal)).ToArray();
            Require(matches.Length == 1, $"Managed Tracking evidence must match exactly once: {fragment}; matches={matches.Length}");
            yield return matches[0];
        }
    }

    private static FamilyDocument BuildFamilies(RawDocument raw, ClassificationDocument classifications, string[] managed)
    {
        var selected = new FamilyRow
        {
            Id = "tracking-public-legacy-model-free-completion",
            Surface = "legacy",
            Rationale = "Closes every remaining parser-emitted public legacy model-free callable: Boosting, TLD, KCF, CSRT mask/lifecycle, and the ownership-preserving modern API adapter."
        };
        foreach (int ordinal in Selected.OrderBy(x => x))
        {
            RawDeclaration declaration = raw.Declarations.Single(x => x.Ordinal == ordinal);
            ClassificationRow row = classifications.Declarations.Single(x => x.Ordinal == ordinal);
            selected.Declarations.Add(new FamilyOperation
            {
                Ordinal = ordinal,
                UpstreamIdentity = declaration.Identity,
                NativeEntrypoints = row.NativeEntrypoints.ToList(),
                ManagedMembers = row.ManagedMembers.ToList()
            });
        }

        string Managed(string fragment)
        {
            string[] matches = managed.Where(line => line.Contains(fragment, StringComparison.Ordinal)).ToArray();
            Require(matches.Length == 1, "Source-reviewed managed evidence must match exactly once: " + fragment);
            return matches[0];
        }

        return new FamilyDocument
        {
            Families = new List<FamilyRow> { selected },
            SourceReviewedExtensions = new List<SourceReviewedExtension>
            {
                new()
                {
                    UpstreamIdentity = "cv::legacy::TrackerBoosting::Params and create(const Params&)",
                    Adaptation = "Copies five primitive parameter fields through a flat C value and exposes exact native defaults without passing the C++ parameter layout.",
                    NativeEntrypoints = new List<string> { "jyppx_ocv_tracking_legacy_tracker_boosting_create", "jyppx_ocv_tracking_legacy_tracker_boosting_get_default_params" },
                    ManagedMembers = new List<string> { Managed("MEMBER|JYPPX.OpenCvSharp.Tracking.Legacy.TrackerBoosting|method|public;static|JYPPX.OpenCvSharp.Tracking.Legacy.TrackerBoosting Create(JYPPX.OpenCvSharp.Tracking.Legacy.TrackerBoostingParams") }
                },
                new()
                {
                    UpstreamIdentity = "cv::legacy::TrackerKCF::create(const Params&)",
                    Adaptation = "Reuses the reviewed modern KCF managed value shape and copies it into the derived legacy C++ parameter object.",
                    NativeEntrypoints = new List<string> { "jyppx_ocv_tracking_legacy_tracker_kcf_create" },
                    ManagedMembers = new List<string> { Managed("MEMBER|JYPPX.OpenCvSharp.Tracking.Legacy.TrackerKCF|method|public;static|JYPPX.OpenCvSharp.Tracking.Legacy.TrackerKCF Create(JYPPX.OpenCvSharp.Tracking.TrackerKCFParams") }
                },
                new()
                {
                    UpstreamIdentity = "cv::legacy::TrackerCSRT::create(const Params&)",
                    Adaptation = "Reuses reviewed CSRT values, pins strict UTF-8 only for the call, and copies every field into the derived legacy C++ parameter object.",
                    NativeEntrypoints = new List<string> { "jyppx_ocv_tracking_legacy_tracker_csrt_create" },
                    ManagedMembers = new List<string> { Managed("MEMBER|JYPPX.OpenCvSharp.Tracking.Legacy.TrackerCSRT|method|public;static|JYPPX.OpenCvSharp.Tracking.Legacy.TrackerCSRT Create(JYPPX.OpenCvSharp.Tracking.TrackerCSRTParams") }
                },
                new()
                {
                    UpstreamIdentity = "cv::legacy::TrackerKCF::setFeatureExtractor(callback,bool)",
                    Adaptation = "Intentionally not exposed: callback lifetime, reentrancy, exception capture, thread origin, and native retention require a separate callback contract.",
                    NativeEntrypoints = new List<string>(),
                    ManagedMembers = new List<string>()
                }
            }
        };
    }

    private static void Validate(
        RawDocument raw,
        ClassificationDocument classifications,
        Options? options,
        string[] native,
        string[] managed,
        bool validateFiles,
        bool repositoryWideClaim)
    {
        Require(raw.SchemaVersion == 1 && raw.UpstreamOpenCvVersion == "5.0.0", "Tracking raw schema/version mismatch.");
        Require(raw.DeclarationCount == raw.Declarations.Count, "Tracking raw declaration count mismatch.");
        Require(raw.Declarations.Select(x => x.Ordinal).SequenceEqual(Enumerable.Range(0, raw.Declarations.Count)), "Tracking parser ordinals are not contiguous and ordered.");
        Require(raw.Declarations.Select(x => x.Identity).Distinct(Ordinal).Count() == raw.Declarations.Count, "Tracking parser identities are not unique.");
        Require(raw.HeaderSha256.Length == 64 && raw.ParserSha256.Length == 64, "Tracking header/parser SHA256 is invalid.");
        Require(raw.CompatibilityHeaders.Count == 2 && raw.SourceHeaders.Count == 2, "Tracking primary/legacy header boundary mismatch.");
        Require(raw.CompatibilityHeaders.All(x => x.Sha256.Length == 64) && raw.SourceHeaders.All(x => x.Sha256.Length == 64), "Tracking compatibility/source-header SHA256 is invalid.");
        Require(raw.SourceHeaders.Select(x => x.Surface).SequenceEqual(new[] { "primary", "legacy" }), "Tracking source-header surface order drifted.");
        Require(raw.SurfaceCounts.TryGetValue("primary", out int primary) && primary == 10, "Tracking primary declaration count drifted.");
        Require(raw.SurfaceCounts.TryGetValue("legacy", out int legacy) && legacy == 25, "Tracking legacy declaration count drifted.");
        Require(raw.Declarations.Count(x => x.Surface == "primary") == primary && raw.Declarations.Count(x => x.Surface == "legacy") == legacy, "Tracking surface partition mismatch.");
        Require(raw.Declarations.All(x => x.Surface is "primary" or "legacy"), "Tracking declaration has an unknown surface.");
        Require(raw.ExcludedPublicHeaders.Count == 8 && raw.ExcludedPublicHeaders.All(x => !string.IsNullOrWhiteSpace(x.Reason)), "Tracking excluded public-header evidence drifted.");
        Require(classifications.SchemaVersion == 1 && classifications.ClaimedSlice == ClaimedSlice, "Tracking classification schema/slice mismatch.");
        Require(classifications.Declarations.Count == raw.Declarations.Count, "Tracking classification row count mismatch.");
        Require(classifications.Declarations.Select(x => x.Ordinal).SequenceEqual(raw.Declarations.Select(x => x.Ordinal)), "Tracking classification order drifted.");
        Require(!repositoryWideClaim, "Tracking contract cannot claim repository-wide parity.");

        for (int i = 0; i < raw.Declarations.Count; i++)
        {
            RawDeclaration declaration = raw.Declarations[i];
            ClassificationRow row = classifications.Declarations[i];
            Require(row.Identity == declaration.Identity, "Tracking classification identity drift at ordinal " + i);
            Require(row.Surface == declaration.Surface, "Tracking classification surface drift at ordinal " + i);
            Require(Allowed.Contains(row.Classification, Ordinal), "Unknown Tracking classification at ordinal " + i);
            Require(!string.IsNullOrWhiteSpace(row.Reason), "Tracking classification reason is empty at ordinal " + i);
            Require(!row.Identity.Contains("OpenCv5Sharp", StringComparison.Ordinal), "Fixed-major managed identity entered Tracking evidence.");
            Require(IsSortedDistinct(row.NativeEntrypoints) && IsSortedDistinct(row.ManagedMembers), "Tracking evidence ordering is nondeterministic at ordinal " + i);
            if (declaration.Kind == "callable")
            {
                Require(row.Classification == "implemented", "Every measured Tracking callable must remain implemented; ordinal " + i);
                Require(row.NativeEntrypoints.Count > 0 && row.ManagedMembers.Count > 0, "Implemented Tracking row lacks evidence at ordinal " + i);
            }
            else
            {
                Require(row.Classification == "non-callable-metadata", "Tracking metadata classification mismatch at ordinal " + i);
                Require(row.NativeEntrypoints.Count == 0 && row.ManagedMembers.Count == 0, "Tracking metadata row carries callable evidence at ordinal " + i);
            }

            foreach (string entrypoint in row.NativeEntrypoints)
            {
                Require(native.Contains(entrypoint, Ordinal), "False Tracking native evidence: " + entrypoint);
            }
            foreach (string member in row.ManagedMembers)
            {
                Require(managed.Contains(member, Ordinal), "False Tracking managed evidence: " + member);
            }
        }

        if (validateFiles)
        {
            Require(options != null, "Tracking validation options are missing.");
            foreach (CompatibilityHeader header in raw.CompatibilityHeaders)
            {
                ValidateHash(options!.Workspace, header.Path, header.Sha256);
            }
            foreach (SourceHeader header in raw.SourceHeaders)
            {
                ValidateHash(options!.Workspace, header.Path, header.Sha256);
            }
            ValidateHash(options!.Workspace, raw.ParserPath, raw.ParserSha256);
            Require(!raw.Declarations.Any(x => x.Identity.Contains("cv.TrackerMIL", StringComparison.Ordinal) || x.Identity.Contains("DaSiamRPN", StringComparison.Ordinal) || x.Identity.Contains("TrackerNano", StringComparison.Ordinal) || x.Identity.Contains("TrackerVit", StringComparison.Ordinal)), "Main Video or external-model rows were double-counted in contrib Tracking.");
        }
    }

    private static void RunNegativeFixtures(RawDocument raw, ClassificationDocument source, Options options, string[] native, string[] managed)
    {
        int fixtures = 0;
        void F(Action<RawDocument, ClassificationDocument> mutate, bool claim = false)
        {
            RawDocument r = Clone(raw);
            ClassificationDocument c = Clone(source);
            mutate(r, c);
            ExpectFailure(() => Validate(r, c, options, native, managed, false, claim));
            fixtures++;
        }

        F((_, c) => c.Declarations.Single(x => x.Ordinal == 16).Classification = "missing");
        F((_, c) => c.Declarations[1].Ordinal = c.Declarations[0].Ordinal);
        F((_, c) => (c.Declarations[0], c.Declarations[1]) = (c.Declarations[1], c.Declarations[0]));
        F((_, c) => c.Declarations.Single(x => x.Ordinal == 3).Identity = c.Declarations.Single(x => x.Ordinal == 9).Identity);
        F((_, c) => c.Declarations.Single(x => x.Ordinal == 16).Surface = "primary");
        F((r, _) => r.HeaderSha256 = "bad");
        F((r, _) => r.ParserSha256 = "bad");
        F((r, _) => r.SourceHeaders[0].Sha256 = "bad");
        F((_, c) => c.Declarations.Single(x => x.Ordinal == 16).NativeEntrypoints[0] = "jyppx_ocv_tracking_false");
        F((_, c) => c.Declarations.Single(x => x.Ordinal == 16).ManagedMembers[0] = "MEMBER|false");
        F((_, c) => { ClassificationRow row = c.Declarations.Single(x => x.Ordinal == 16); row.Classification = "intentionally-omitted"; row.Reason = ""; });
        F((_, c) => c.Declarations.Single(x => x.Ordinal == 16).Classification = "conditional-ish");
        F((_, c) => c.Declarations.Single(x => x.Ordinal == 16).Identity = "OpenCv5Sharp.Tracking.Legacy.TrackerBoosting.create");
        F((_, _) => { }, true);
        F((_, c) => c.Declarations.Single(x => x.Ordinal == 3).NativeEntrypoints.Reverse());
        F((r, _) => r.SourceHeaders.Reverse());
        F((r, _) => r.SurfaceCounts["primary"] = 11);
        Require(fixtures == NegativeFixtureCount, "Tracking negative fixture count mismatch.");
    }

    private static string BuildMap(RawDocument raw, ClassificationDocument classifications)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# OpenCV 5.0.0 Contrib Tracking Upstream Map");
        builder.AppendLine("# Primary and public legacy surfaces are counted separately; main Video and repository-wide parity are not claimed.");
        builder.AppendLine();
        foreach (RawDeclaration declaration in raw.Declarations)
        {
            ClassificationRow row = classifications.Declarations.Single(x => x.Ordinal == declaration.Ordinal);
            builder.Append(declaration.Ordinal.ToString("D3"));
            builder.Append(" | ").Append(declaration.Surface);
            builder.Append(" | ").Append(declaration.Kind);
            builder.Append(" | ").Append(row.Classification);
            builder.Append(" | ").Append(declaration.Identity);
            builder.Append(" | native=").Append(string.Join(",", row.NativeEntrypoints));
            builder.Append(" | managed=").Append(string.Join(",", row.ManagedMembers));
            builder.AppendLine();
        }
        return builder.ToString().Replace("\r\n", "\n", StringComparison.Ordinal);
    }

    private static string[] ReadNative(string path)
    {
        return File.ReadAllLines(path, Encoding.UTF8)
            .Select(line => line.Split('|')[0].Trim())
            .Where(line => line.StartsWith("jyppx_ocv_", StringComparison.Ordinal))
            .Distinct(Ordinal)
            .OrderBy(line => line, Ordinal)
            .ToArray();
    }

    private static bool IsSortedDistinct(List<string> values)
    {
        return values.SequenceEqual(values.Distinct(Ordinal).OrderBy(x => x, Ordinal));
    }

    private static void ValidateHash(string workspace, string relativePath, string expected)
    {
        string fullPath = Path.Combine(workspace, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Require(File.Exists(fullPath), "Tracking source evidence file is missing: " + relativePath);
        Require(Sha256(File.ReadAllBytes(fullPath)) == expected, "Tracking source evidence hash drifted: " + relativePath);
    }

    private static string Rel(string root, string path)
    {
        return Path.GetRelativePath(root, path).Replace('\\', '/');
    }

    private static T Read<T>(string path)
    {
        return JsonSerializer.Deserialize<T>(File.ReadAllText(path, Encoding.UTF8), JsonOptions())
            ?? throw new InvalidOperationException("Could not parse " + path);
    }

    private static T Clone<T>(T value)
    {
        return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value, JsonOptions()), JsonOptions())!;
    }

    private static string Serialize<T>(T value)
    {
        return JsonSerializer.Serialize(value, JsonOptions()) + "\n";
    }

    private static JsonSerializerOptions JsonOptions()
    {
        return new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };
    }

    private static string Sha256(string text)
    {
        return Sha256(Encoding.UTF8.GetBytes(text.Replace("\r\n", "\n", StringComparison.Ordinal)));
    }

    private static string Sha256(byte[] bytes)
    {
        return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    }

    private static void WriteOrCheck(string path, string content, bool check)
    {
        content = content.Replace("\r\n", "\n", StringComparison.Ordinal);
        if (check)
        {
            Require(File.Exists(path), "Generated Tracking artifact is missing: " + path);
            Require(File.ReadAllText(path, Encoding.UTF8).Replace("\r\n", "\n", StringComparison.Ordinal) == content, "Generated Tracking artifact is stale: " + path);
        }
        else
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, content, new UTF8Encoding(false));
        }
    }

    private static void ExpectFailure(Action action)
    {
        try
        {
            action();
        }
        catch (Exception)
        {
            return;
        }
        throw new InvalidOperationException("Tracking negative fixture unexpectedly passed.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}

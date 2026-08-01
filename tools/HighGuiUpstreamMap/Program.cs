using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

internal static class Program
{
    private static readonly StringComparer Ordinal = StringComparer.Ordinal;
    private static readonly HashSet<int> Implemented = new(new[]
    {
        7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 26, 27, 28, 29
    });
    private static readonly HashSet<int> Omitted = new() { 23, 24, 25 };
    private static readonly HashSet<int> Conditional = new() { 30, 31, 32 };
    private static readonly string[] Allowed =
    {
        "implemented", "missing", "intentionally-omitted", "upstream-conditional", "unsupported", "non-callable-metadata"
    };
    private const string ClaimedSlice = "opencv2/highgui.hpp exact OpenCV 5.0.0 official-parser surface with compatibility and WinRT headers separately source-reviewed";
    private const int NegativeFixtureCount = 17;
    private const int ManagedTypeAdditions = 0;
    private const int ManagedMemberAdditions = 6;
    private const int NativeEntrypointAdditions = 10;

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
        public string Limitation { get; set; } = "Interactive ROI selection is intentionally omitted from automation; Qt-only calls and WinRT are conditional; callback APIs omitted by the official parser are recorded as source-reviewed extensions; repository-wide parity is not claimed.";
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
        public string Generator { get; init; } = "tools/HighGuiUpstreamMap";
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
        public int SourceReviewedExtensionCount { get; init; }
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
        public string FocusedTest { get; init; } = "tests/OpenCvSharp.Tests/HighGui/HighGuiTests.cs";
        public string NativeSmoke { get; init; } = "src/OpenCvSharp.Native/tests/native_smoke.cpp";
        public string Sample { get; init; } = "samples/ConsoleSamples/Program.cs";
        public string Guide { get; init; } = "docs/articles/highgui-interaction-guide.md";
    }
    private sealed class SourceReviewedExtension
    {
        public string UpstreamIdentity { get; init; } = "";
        public string SourceHeader { get; init; } = "opencv-source/opencv-5.0.0/modules/highgui/include/opencv2/highgui.hpp";
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
            FamilyDocument families = BuildFamilies(raw, classifications, native, managed);
            ValidateFamilies(families, native, managed);
            string familyText = Serialize(families);
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
                SelectedFamilyCount = families.Families.Count,
                SelectedDeclarationCount = families.Families.Sum(x => x.Declarations.Count),
                SourceReviewedExtensionCount = families.SourceReviewedExtensions.Count,
                RepositoryWideUpstreamParityClaimed = false
            };
            RunNegativeFixtures(raw, classifications, options, native, managed);
            WriteOrCheck(options.Output, mapping, options.Check);
            WriteOrCheck(options.FamilyOutput, familyText, options.Check);
            WriteOrCheck(options.Summary, Serialize(summary), options.Check);
            Console.WriteLine($"HIGHGUI_UPSTREAM_MAP_OK declarations={summary.DeclarationCount} callables={summary.CallableCount} implemented={counts["implemented"]} omitted={counts["intentionally-omitted"]} conditional={counts["upstream-conditional"]} missing={counts["missing"]} extensions={summary.SourceReviewedExtensionCount} fixtures={NegativeFixtureCount} sha256={summary.MappingSha256} mode={(options.Check ? "check" : "write")}");
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
        bool initialize = false, check = false;
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
        string Value(string name) { Require(values.TryGetValue(name, out string? value), "Missing option: " + name); return Path.GetFullPath(value!); }
        return new Options(Value("--repository"), Value("--workspace"), Value("--raw"), Value("--classification"), Value("--native-manifest"), Value("--managed-baseline"), Value("--output"), Value("--summary"), Value("--family-output"), initialize, check);
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
                row.BuildCondition = "metadata; parser-emitted";
                row.Reason = "Parser-emitted HighGui enum metadata is reviewed as public constant shape rather than an independently callable ABI operation.";
            }
            else if (Omitted.Contains(declaration.Ordinal))
            {
                row.Classification = "intentionally-omitted";
                row.BuildCondition = "interactive-user-input; automation-excluded";
                row.Reason = "Interactive ROI selection owns a blocking window/event loop and temporarily replaces the window mouse callback; it is intentionally excluded from deterministic unattended binding workflows.";
            }
            else if (Conditional.Contains(declaration.Ordinal))
            {
                row.Classification = "upstream-conditional";
                row.BuildCondition = "HAVE_QT; Qt-backend-only";
                row.Reason = "This text/overlay/status operation is implemented only by the Qt HighGui backend; the verified full runtime uses WIN32 and must not advertise Qt-only behavior.";
            }
            else
            {
                Require(Implemented.Contains(declaration.Ordinal), "Unclassified HighGui callable ordinal " + declaration.Ordinal);
                row.Classification = "implemented";
                row.BuildCondition = "OPENCV_CSHARP_HAS_OPENCV; full-profile; highgui-required";
                row.Reason = "The version-neutral native and managed HighGui surface preserves this callable's backend, value, UTF-8, and full-profile semantics.";
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

    private static List<string> NativeEvidence(int ordinal, string[] native)
    {
        string[] names = ordinal switch
        {
            7 => new[] { "jyppx_ocv_highgui_named_window" },
            8 => new[] { "jyppx_ocv_highgui_destroy_window" },
            9 => new[] { "jyppx_ocv_highgui_destroy_all_windows" },
            10 => new[] { "jyppx_ocv_highgui_current_ui_framework_length", "jyppx_ocv_highgui_current_ui_framework_fill" },
            11 => new[] { "jyppx_ocv_highgui_start_window_thread" },
            12 => new[] { "jyppx_ocv_highgui_wait_key_ex" },
            13 => new[] { "jyppx_ocv_highgui_wait_key" },
            14 => new[] { "jyppx_ocv_highgui_poll_key" },
            15 => new[] { "jyppx_ocv_highgui_imshow" },
            16 or 17 => new[] { "jyppx_ocv_highgui_resize_window" },
            18 => new[] { "jyppx_ocv_highgui_move_window" },
            19 => new[] { "jyppx_ocv_highgui_set_window_property" },
            20 => new[] { "jyppx_ocv_highgui_set_window_title" },
            21 => new[] { "jyppx_ocv_highgui_get_window_property" },
            22 => new[] { "jyppx_ocv_highgui_get_window_image_rect" },
            26 => new[] { "jyppx_ocv_highgui_get_trackbar_pos" },
            27 => new[] { "jyppx_ocv_highgui_set_trackbar_pos" },
            28 => new[] { "jyppx_ocv_highgui_set_trackbar_max" },
            29 => new[] { "jyppx_ocv_highgui_set_trackbar_min" },
            _ => throw new InvalidOperationException("No HighGui native evidence mapping for ordinal " + ordinal)
        };
        return names.Select(name => FindNative(native, name)).OrderBy(x => x, Ordinal).ToList();
    }

    private static string ManagedEvidence(int ordinal, string[] managed)
    {
        string[] fragments = ordinal switch
        {
            7 => new[] { "System.Void NamedWindow(" },
            8 => new[] { "System.Void DestroyWindow(" },
            9 => new[] { "System.Void DestroyAllWindows()" },
            10 => new[] { "System.String GetCurrentUIFramework()" },
            11 => new[] { "System.Int32 StartWindowThread()" },
            12 => new[] { "System.Int32 WaitKeyEx(" },
            13 => new[] { "System.Int32 WaitKey(" },
            14 => new[] { "System.Int32 PollKey()" },
            15 => new[] { "System.Void ImShow(" },
            16 => new[] { "System.Void ResizeWindow(System.String winname,System.Int32 width,System.Int32 height)" },
            17 => new[] { "System.Void ResizeWindow(System.String winname,OpenCvSharp.Core.Size size)" },
            18 => new[] { "System.Void MoveWindow(" },
            19 => new[] { "System.Void SetWindowProperty(" },
            20 => new[] { "System.Void SetWindowTitle(" },
            21 => new[] { "System.Double GetWindowProperty(" },
            22 => new[] { "OpenCvSharp.Core.Rect GetWindowImageRect(" },
            26 => new[] { "System.Int32 GetTrackbarPos(" },
            27 => new[] { "System.Void SetTrackbarPos(" },
            28 => new[] { "System.Void SetTrackbarMax(" },
            29 => new[] { "System.Void SetTrackbarMin(" },
            _ => throw new InvalidOperationException("No HighGui managed evidence mapping for ordinal " + ordinal)
        };
        return FindManaged(managed, fragments);
    }

    private static FamilyDocument BuildFamilies(RawDocument raw, ClassificationDocument classifications, string[] native, string[] managed)
    {
        FamilyRow Family(string id, string rationale, IEnumerable<int> ordinals, string test)
        {
            var result = new FamilyRow { Id = id, Rationale = rationale };
            foreach (int ordinal in ordinals.OrderBy(x => x))
            {
                result.Declarations.Add(new FamilyOperation
                {
                    Ordinal = ordinal,
                    UpstreamIdentity = raw.Declarations[ordinal].Identity,
                    NativeEntrypoints = new(classifications.Declarations[ordinal].NativeEntrypoints),
                    ManagedMembers = new(classifications.Declarations[ordinal].ManagedMembers),
                    FocusedTest = test
                });
            }
            return result;
        }

        SourceReviewedExtension Extension(string identity, string adaptation, IEnumerable<string> nativeNames, params string[] managedFragments)
        {
            return new SourceReviewedExtension
            {
                UpstreamIdentity = identity,
                Adaptation = adaptation,
                NativeEntrypoints = nativeNames.Select(name => FindNative(native, name)).Distinct(Ordinal).OrderBy(x => x, Ordinal).ToList(),
                ManagedMembers = managedFragments.Select(fragment => FindManaged(managed, fragment)).Distinct(Ordinal).OrderBy(x => x, Ordinal).ToList()
            };
        }

        return new FamilyDocument
        {
            Families = new()
            {
                Family("highgui-window-lifecycle-backend", "Window creation/destruction, backend identity, and backend-specific thread startup retain explicit full-profile and event-loop behavior.", Enumerable.Range(7, 5), "tests/OpenCvSharp.Tests/HighGui/HighGuiTests.cs"),
                Family("highgui-event-image-window", "Key/event, image display, resize/move, property, title, and rendering-rectangle operations retain borrowed Mat and backend-specific result semantics.", Enumerable.Range(12, 11), "tests/OpenCvSharp.Tests/HighGui/HighGuiTests.cs"),
                Family("highgui-trackbar-controls", "Trackbar position and range operations preserve exact window/name association and source-defined integer state.", Enumerable.Range(26, 4), "tests/OpenCvSharp.Tests/HighGui/HighGuiInteractionTests.cs")
            },
            SourceReviewedExtensions = new()
            {
                Extension(
                    "cv.createTrackbar(String trackbarname;String winname;int* value;int count;TrackbarCallback onChange=0;void* userdata=0)->int",
                    "A ref-counted opaque registration keeps the value pointer and callback state alive until both the managed SafeHandle and owning window release their references; explicit-length UTF-8 rejects embedded null and invalid text.",
                    new[] { "jyppx_ocv_highgui_create_trackbar", "jyppx_ocv_highgui_create_trackbar_utf8", "jyppx_ocv_highgui_trackbar_release_handle" },
                    "HighGuiTrackbar CreateTrackbar("),
                Extension(
                    "cv.setMouseCallback(String winname;MouseCallback onMouse;void* userdata=0)->void",
                    "Per-window registrations replace and unregister native callbacks before managed delegates are released; callback exceptions are captured and rethrown only through an explicit managed observation call.",
                    new[] { "jyppx_ocv_highgui_set_mouse_callback", "jyppx_ocv_highgui_mouse_callback_create_utf8", "jyppx_ocv_highgui_mouse_callback_clear_utf8", "jyppx_ocv_highgui_callback_registration_release_handle" },
                    "System.Void SetMouseCallback(", "System.Void ThrowPendingCallbackException()"),
                Extension(
                    "cv.getMouseWheelDelta(int flags)->int",
                    "The signed high-word delta is exposed as a pure full-profile helper with exact integer output and no retained state.",
                    new[] { "jyppx_ocv_highgui_get_mouse_wheel_delta" },
                    "System.Int32 GetMouseWheelDelta("),
                Extension(
                    "cv.createButton(String bar_name;ButtonCallback on_change;void* userdata=0;int type=QT_PUSH_BUTTON;bool initial_button_state=false)->int",
                    "Qt-only button callbacks use a native ref-counted trampoline and remain rooted until DestroyAllWindows; unavailable Qt backends report the upstream NativeException without leaking state.",
                    new[] { "jyppx_ocv_highgui_create_button", "jyppx_ocv_highgui_button_callback_create_utf8", "jyppx_ocv_highgui_callback_registration_release_handle" },
                    "System.Void CreateButton(", "System.Void ThrowPendingCallbackException()")
            }
        };
    }

    private static void Validate(RawDocument raw, ClassificationDocument classifications, Options? options, string[] native, string[] managed, bool verifyFiles)
    {
        Require(raw.SchemaVersion == 1 && raw.UpstreamOpenCvVersion == "5.0.0" && raw.DeclarationCount == 33 && raw.Declarations.Count == 33, "HighGui raw identity/count drifted.");
        Require(raw.Declarations.Count(x => x.Kind == "callable") == 26 && raw.Declarations.Count(x => x.Kind == "enum") == 7 && raw.Declarations.Count(x => x.Kind == "class") == 0, "HighGui declaration partition drifted.");
        Require(raw.SourceHeaders.Count == 1 && raw.SourceHeaders[0].StartOrdinal == 0 && raw.SourceHeaders[0].DeclarationCount == 33 && raw.CompatibilityHeaders.Count == 3 && raw.ExcludedPublicHeaders.Count == 0, "HighGui header closure drifted.");
        Require(raw.PreprocessorDefinitions.Count == 2 && raw.PreprocessorDefinitions.GetValueOrDefault("CV_VERSION_MAJOR") == 5 && raw.PreprocessorDefinitions.GetValueOrDefault("OPENCV_ABI_COMPATIBILITY") == 500, "HighGui parser definitions drifted.");
        Require(raw.CompatibilityHeaders.Any(x => x.Path.EndsWith("highgui/highgui_winrt.hpp", StringComparison.Ordinal)), "HighGui WinRT conditional header evidence is missing.");
        for (int i = 0; i < raw.Declarations.Count; i++)
            Require(raw.Declarations[i].Ordinal == i && raw.Declarations[i].SourceHeader == raw.SourceHeaders[0].Path && !string.IsNullOrWhiteSpace(raw.Declarations[i].Identity), "HighGui parser order/source/identity drifted at " + i);
        Require(raw.Declarations.Select(x => x.Identity).Distinct(Ordinal).Count() == 33, "HighGui overload identities collapsed.");
        Require(classifications.SchemaVersion == 1 && classifications.UpstreamOpenCvVersion == "5.0.0" && classifications.ClaimedSlice == ClaimedSlice && classifications.ReviewStatus == "source-reviewed" && classifications.Declarations.Count == 33, "HighGui classification identity drifted.");
        Require(!classifications.ClaimedSlice.Contains("OpenCv5Sharp", StringComparison.Ordinal) && !classifications.ClaimedSlice.Contains("repository-wide", StringComparison.OrdinalIgnoreCase), "HighGui fixed-major or repository-wide claim drifted.");
        var nativeSet = new HashSet<string>(native, Ordinal);
        var managedSet = new HashSet<string>(managed, Ordinal);
        for (int i = 0; i < 33; i++)
        {
            RawDeclaration declaration = raw.Declarations[i];
            ClassificationRow row = classifications.Declarations[i];
            Require(row.Ordinal == i && row.Identity == declaration.Identity && Allowed.Contains(row.Classification, Ordinal), "HighGui classification order/value drifted at " + i);
            Require(!string.IsNullOrWhiteSpace(row.Reason) && !string.IsNullOrWhiteSpace(row.BuildCondition), "Undocumented HighGui row at " + i);
            Require(row.NativeEntrypoints.SequenceEqual(row.NativeEntrypoints.Distinct(Ordinal).OrderBy(x => x, Ordinal)) && row.ManagedMembers.SequenceEqual(row.ManagedMembers.Distinct(Ordinal).OrderBy(x => x, Ordinal)), "Nondeterministic HighGui evidence ordering at " + i);
            Require(declaration.Kind == "callable" ? row.Classification != "non-callable-metadata" : row.Classification == "non-callable-metadata", "HighGui callable/metadata confusion at " + i);
            if (row.Classification == "implemented")
                Require(row.NativeEntrypoints.Count > 0 && row.ManagedMembers.Count > 0 && row.NativeEntrypoints.All(nativeSet.Contains) && row.ManagedMembers.All(managedSet.Contains), "False or missing HighGui evidence at " + i);
            else if (declaration.Kind == "callable")
                Require(row.NativeEntrypoints.Count == 0 && row.ManagedMembers.Count == 0, "Non-implemented HighGui callable carries evidence at " + i);
            string expectedCondition = row.Classification switch
            {
                "implemented" => "OPENCV_CSHARP_HAS_OPENCV; full-profile; highgui-required",
                "intentionally-omitted" => "interactive-user-input; automation-excluded",
                "upstream-conditional" => "HAVE_QT; Qt-backend-only",
                _ => "metadata; parser-emitted"
            };
            Require(row.BuildCondition == expectedCondition, "HighGui build condition drifted at " + i);
        }
        Require(classifications.Declarations.Count(x => x.Classification == "implemented") == 20 && classifications.Declarations.Count(x => x.Classification == "missing") == 0 && classifications.Declarations.Count(x => x.Classification == "intentionally-omitted") == 3 && classifications.Declarations.Count(x => x.Classification == "upstream-conditional") == 3 && classifications.Declarations.Count(x => x.Classification == "unsupported") == 0 && classifications.Declarations.Count(x => x.Classification == "non-callable-metadata") == 7, "HighGui classification partition drifted.");
        Require(Implemented.All(i => classifications.Declarations[i].Classification == "implemented") && Omitted.All(i => classifications.Declarations[i].Classification == "intentionally-omitted") && Conditional.All(i => classifications.Declarations[i].Classification == "upstream-conditional"), "HighGui fixed ordinal partitions drifted.");
        if (verifyFiles)
        {
            Require(options != null, "Options required for HighGui hash verification.");
            VerifyHash(options!.Workspace, raw.HeaderPath, raw.HeaderSha256, "main header");
            VerifyHash(options.Workspace, raw.ParserPath, raw.ParserSha256, "parser");
            foreach (CompatibilityHeader header in raw.CompatibilityHeaders) VerifyHash(options.Workspace, header.Path, header.Sha256, "compatibility header");
            foreach (SourceHeader header in raw.SourceHeaders) VerifyHash(options.Workspace, header.Path, header.Sha256, "source header");
        }
    }

    private static void ValidateFamilies(FamilyDocument families, string[] native, string[] managed)
    {
        Require(families.SchemaVersion == 1 && families.Status == "implemented-verified" && families.Families.Count == 3 && families.Families.Sum(x => x.Declarations.Count) == 20 && families.SourceReviewedExtensions.Count == 4, "HighGui family/extension partition drifted.");
        Require(families.Families.SelectMany(x => x.Declarations).Select(x => x.Ordinal).OrderBy(x => x).SequenceEqual(Implemented.OrderBy(x => x)), "HighGui family ordinals drifted.");
        var nativeSet = new HashSet<string>(native, Ordinal);
        var managedSet = new HashSet<string>(managed, Ordinal);
        foreach (SourceReviewedExtension extension in families.SourceReviewedExtensions)
        {
            Require(!string.IsNullOrWhiteSpace(extension.UpstreamIdentity) && !string.IsNullOrWhiteSpace(extension.Adaptation) && extension.NativeEntrypoints.Count > 0 && extension.ManagedMembers.Count > 0, "HighGui source-reviewed extension lacks evidence.");
            Require(extension.NativeEntrypoints.SequenceEqual(extension.NativeEntrypoints.Distinct(Ordinal).OrderBy(x => x, Ordinal)) && extension.ManagedMembers.SequenceEqual(extension.ManagedMembers.Distinct(Ordinal).OrderBy(x => x, Ordinal)) && extension.NativeEntrypoints.All(nativeSet.Contains) && extension.ManagedMembers.All(managedSet.Contains), "HighGui extension evidence is false or nondeterministic.");
        }
        Require(families.SourceReviewedExtensions.Any(x => x.UpstreamIdentity.Contains("setMouseCallback", StringComparison.Ordinal) && x.NativeEntrypoints.Any(v => v.EndsWith("mouse_callback_clear_utf8", StringComparison.Ordinal)) && x.NativeEntrypoints.Any(v => v.EndsWith("callback_registration_release_handle", StringComparison.Ordinal))), "HighGui mouse callback ownership evidence is incomplete.");
    }

    private static string BuildMap(RawDocument raw, ClassificationDocument classifications)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Generated by tools/HighGuiUpstreamMap. Do not edit.");
        builder.AppendLine("schema-version=1");
        builder.AppendLine("upstream-opencv-version=5.0.0");
        builder.AppendLine("claimed-slice=" + ClaimedSlice);
        builder.AppendLine("header-sha256=" + raw.HeaderSha256);
        builder.AppendLine("parser-sha256=" + raw.ParserSha256);
        builder.AppendLine("declaration-count=33");
        builder.AppendLine("callable-count=26");
        builder.AppendLine("class-count=0");
        builder.AppendLine("enum-count=7");
        builder.AppendLine("repository-wide-upstream-parity-claimed=false");
        foreach (CompatibilityHeader header in raw.CompatibilityHeaders)
            builder.AppendLine($"compatibility-header={header.Path}|{header.Sha256}|includes={header.Includes}");
        builder.AppendLine();
        builder.AppendLine("ordinal|kind|source-header|classification|identity|native-entrypoints|managed-members|build-condition|reason");
        for (int i = 0; i < 33; i++)
        {
            RawDeclaration declaration = raw.Declarations[i];
            ClassificationRow row = classifications.Declarations[i];
            builder.AppendLine($"{i}|{declaration.Kind}|{declaration.SourceHeader}|{row.Classification}|{declaration.Identity}|{Join(row.NativeEntrypoints)}|{Join(row.ManagedMembers)}|{row.BuildCondition}|{row.Reason}");
        }
        return builder.ToString().Replace("\r\n", "\n", StringComparison.Ordinal);
    }

    private static void RunNegativeFixtures(RawDocument raw, ClassificationDocument classifications, Options options, string[] native, string[] managed)
    {
        int passed = 0;
        void Fixture(Action<RawDocument, ClassificationDocument> mutate)
        {
            RawDocument rawCopy = Clone(raw);
            ClassificationDocument classificationCopy = Clone(classifications);
            mutate(rawCopy, classificationCopy);
            ExpectFailure(() => Validate(rawCopy, classificationCopy, options, native, managed, true));
            passed++;
        }
        Fixture((_, c) => c.Declarations.RemoveAt(0));
        Fixture((_, c) => c.Declarations[1].Ordinal = 0);
        Fixture((_, c) => (c.Declarations[0], c.Declarations[1]) = (c.Declarations[1], c.Declarations[0]));
        Fixture((r, _) => r.Declarations[17].Identity = r.Declarations[16].Identity);
        Fixture((_, c) => c.Declarations[7].Classification = "non-callable-metadata");
        Fixture((r, _) => r.Declarations[0].SourceHeader = "drifted/highgui.hpp");
        Fixture((r, _) => r.ParserSha256 = new string('0', 64));
        Fixture((r, _) => r.HeaderSha256 = new string('0', 64));
        Fixture((_, c) => c.Declarations[7].NativeEntrypoints[0] = "jyppx_ocv_false_evidence");
        Fixture((_, c) => c.Declarations[7].ManagedMembers[0] = "MEMBER|false");
        Fixture((_, c) => c.Declarations[23].Reason = "");
        Fixture((_, c) => c.ClaimedSlice += "; OpenCv5Sharp");
        Fixture((_, c) => c.Declarations[7].BuildCondition = "unconditional");
        Fixture((_, c) => c.Declarations[30].Classification = "intentionally-omitted");
        string[] missingCallbackEvidence = native.Where(x => x != "jyppx_ocv_highgui_mouse_callback_clear_utf8").ToArray();
        ExpectFailure(() => BuildFamilies(raw, classifications, missingCallbackEvidence, managed));
        passed++;
        Fixture((_, c) => c.Declarations[10].NativeEntrypoints.Reverse());
        Fixture((r, _) => r.CompatibilityHeaders.RemoveAt(0));
        Require(passed == NegativeFixtureCount, "HighGui negative fixture count drifted.");
    }

    private static string FindNative(string[] native, string name)
    {
        Require(native.Contains(name, Ordinal), "No HighGui native evidence for " + name);
        return name;
    }

    private static string FindManaged(string[] managed, params string[] fragments)
    {
        const string prefix = "MEMBER|OpenCvSharp.HighGui.Cv2|";
        List<string> matches = managed.Where(x => x.Contains(prefix, StringComparison.Ordinal) && fragments.All(fragment => x.Contains(fragment, StringComparison.Ordinal))).OrderBy(x => x, Ordinal).ToList();
        Require(matches.Count == 1, "Expected one HighGui managed evidence row for " + string.Join(",", fragments) + "; found " + matches.Count);
        return matches[0];
    }

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
        Require(File.Exists(path), "HighGui " + label + " missing: " + relative);
        Require(Sha256File(path) == expected, "HighGui " + label + " hash drifted: " + relative);
    }
    private static void ExpectFailure(Action action)
    {
        bool failed = false;
        try { action(); } catch { failed = true; }
        Require(failed, "A HighGui negative fixture was accepted.");
    }
    private static string Join(List<string> values) => values.Count == 0 ? "-" : string.Join(";", values.Select(x => x.Replace("|", "<pipe>", StringComparison.Ordinal)));
    private static string Rel(string root, string path) => Path.GetRelativePath(root, path).Replace('\\', '/');
    private static string Sha256(string text) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
    private static string Sha256File(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}

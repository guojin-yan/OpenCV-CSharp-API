using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

internal static class Program
{
    private static readonly StringComparer Ordinal = StringComparer.Ordinal;
    private static readonly HashSet<int> Omitted = new() { 3, 4, 40, 41, 44, 45, 61, 62, 145, 147 };
    private static readonly HashSet<int> Selected = new(new[]
    {
        14, 16, 17, 18, 19, 20, 21, 22, 48, 52, 55, 56, 63, 72, 73, 74, 75,
        100, 105, 106, 133, 134, 139, 140, 141, 142, 176, 177, 178, 179, 181, 182, 183
    });
    private static readonly string[] Allowed =
    {
        "implemented", "missing", "intentionally-omitted", "upstream-conditional", "unsupported", "non-callable-metadata"
    };
    private const string ClaimedSlice = "opencv2/objdetect.hpp and opencv2/objdetect/objdetect.hpp compatibility headers implemented by the nine parser-emitted OpenCV 5.0.0 ObjDetect public source headers";

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
        public List<SourceHeader> SourceHeaders { get; set; } = new();
        public int DeclarationCount { get; set; }
        public List<RawDeclaration> Declarations { get; set; } = new();
    }
    private sealed class CompatibilityHeader { public string Path { get; set; } = ""; public string Sha256 { get; set; } = ""; public string Includes { get; set; } = ""; }
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
        public string Limitation { get; set; } = "The map covers the exact OpenCV 5.0.0 ObjDetect compatibility include closure. Two advanced CirclesGridFinderParameters callables and eight polymorphic persistence callables are intentionally omitted; optional contrib modules and repository-wide parity are not claimed.";
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
        public string Generator { get; init; } = "tools/ObjDetectUpstreamMap";
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
        public int SelectedFamilyCount { get; init; }
        public int SelectedDeclarationCount { get; init; }
        public int ManagedPublicTypeAdditionCount { get; init; } = 4;
        public int ManagedPublicMemberAdditionCount { get; init; } = 55;
        public int NativeEntrypointAdditionCount { get; init; } = 35;
        public bool RepositoryWideUpstreamParityClaimed { get; init; }
    }
    private sealed class FamilyDocument
    {
        public int SchemaVersion { get; init; } = 1;
        public string UpstreamOpenCvVersion { get; init; } = "5.0.0";
        public string Status { get; init; } = "implemented-verified";
        public int ManagedPublicTypeAdditionCount { get; init; } = 4;
        public int ManagedPublicMemberAdditionCount { get; init; } = 55;
        public int NativeEntrypointAdditionCount { get; init; } = 35;
        public List<FamilyRow> Families { get; init; } = new();
    }
    private sealed class FamilyRow
    {
        public string Id { get; init; } = "objdetect-structured-parity";
        public string Rationale { get; init; } = "The selected batch closes all remaining offline-safe ArUco object workflows, byte-preserving graphical-code overloads, MCC DNN controls, and advanced chessboard helpers without external models or polymorphic persistence handles.";
        public List<FamilyOperation> Declarations { get; init; } = new();
    }
    private sealed class FamilyOperation
    {
        public int Ordinal { get; init; }
        public string UpstreamIdentity { get; init; } = "";
        public List<string> NativeEntrypoints { get; init; } = new();
        public List<string> ManagedMembers { get; init; } = new();
        public string FocusedTest { get; init; } = "tests/OpenCvSharp.Tests/ObjDetect/ObjDetectStructuredParityTests.cs";
        public string NativeSmoke { get; init; } = "src/OpenCvSharp.Native/tests/native_smoke.cpp";
        public string Sample { get; init; } = "samples/ConsoleSamples/Program.cs";
        public string Guide { get; init; } = "docs/articles/objdetect-structured-parity-guide.md";
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
                HeaderSha256 = raw.HeaderSha256, ParserSha256 = raw.ParserSha256, CompatibilityHeaderCount = raw.CompatibilityHeaders.Count, SourceHeaderCount = raw.SourceHeaders.Count,
                SourceHeaderSetSha256 = Sha256(string.Join("\n", raw.SourceHeaders.Select(x => $"{x.Path}|{x.Sha256}|{x.StartOrdinal}|{x.DeclarationCount}")) + "\n"),
                MappingSha256 = Sha256(mapping), DeclarationCount = raw.Declarations.Count, EnumCount = raw.Declarations.Count(x => x.Kind == "enum"), ClassCount = raw.Declarations.Count(x => x.Kind == "class"), CallableCount = raw.Declarations.Count(x => x.Kind == "callable"), ClassificationCounts = counts,
                NativeEvidenceCount = classifications.Declarations.SelectMany(x => x.NativeEntrypoints).Distinct(Ordinal).Count(), ManagedEvidenceCount = classifications.Declarations.SelectMany(x => x.ManagedMembers).Distinct(Ordinal).Count(),
                FamilyInventoryPath = Rel(options.Repository, options.FamilyOutput), FamilyInventorySha256 = Sha256(familyText), SelectedFamilyCount = 1, SelectedDeclarationCount = Selected.Count, RepositoryWideUpstreamParityClaimed = false
            };
            RunNegativeFixtures(raw, classifications, options, native, managed);
            WriteOrCheck(options.Output, mapping, options.Check);
            WriteOrCheck(options.FamilyOutput, familyText, options.Check);
            WriteOrCheck(options.Summary, Serialize(summary), options.Check);
            Console.WriteLine($"OBJDETECT_UPSTREAM_MAP_OK declarations={summary.DeclarationCount} callables={summary.CallableCount} implemented={counts["implemented"]} missing={counts["missing"]} omitted={counts["intentionally-omitted"]} fixtures=15 sha256={summary.MappingSha256} mode={(options.Check ? "check" : "write")}");
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
            else if (Omitted.Contains(declaration.Ordinal))
            {
                row.Classification = "intentionally-omitted";
                row.Reason = OmissionReason(declaration.Ordinal);
            }
            else
            {
                row.Classification = "implemented";
                row.Reason = Selected.Contains(declaration.Ordinal)
                    ? "The selected structured-parity batch has a version-neutral native ABI or exact existing buffer ABI, deterministic ownership, native smoke, and net8/net10 managed tests."
                    : "The existing version-neutral native and managed ObjDetect or Calib3D surface provides the callable semantics represented by this parser row.";
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
        if (ordinal == 100 || ordinal is >= 149 and <= 167) return "HAVE_OPENCV_DNN=1; OPENCV_CSHARP_HAS_OPENCV_OBJDETECT; full-profile; mini-excluded";
        if (ordinal is >= 137 and <= 148) return "OPENCV_CSHARP_HAS_OPENCV_CALIB3D; OPENCV_CSHARP_HAS_OPENCV_OBJDETECT; full-profile; mini-excluded";
        return "OPENCV_CSHARP_HAS_OPENCV_OBJDETECT; full-profile; mini-excluded";
    }

    private static string OmissionReason(int ordinal)
    {
        if (ordinal is 3 or 4 or 40 or 41 or 44 or 45 or 61 or 62)
            return "Polymorphic Algorithm persistence is deferred until ObjDetect objects can consume the shared FileStorage and FileNode ownership contract without casting private module handles.";
        if (ordinal is 145 or 147)
            return "CirclesGridFinderParameters and its dedicated overload require a complete parameter value object plus Feature2D blob-detector ownership; the existing flags overload is not treated as equivalent.";
        throw new InvalidOperationException("No omission reason for ordinal " + ordinal);
    }

    private static List<string> NativeEvidence(RawDeclaration declaration, string[] native)
    {
        int ordinal = declaration.Ordinal;
        string method = declaration.Name.Split('.').Last();
        string baseName;
        if (ordinal == 1) baseName = "jyppx_ocv_aruco_dictionary_create_default";
        else if (ordinal == 2) baseName = "jyppx_ocv_aruco_dictionary_create_from_bytes_list";
        else if (ordinal == 6) baseName = "jyppx_ocv_aruco_dictionary_identify_with_threshold";
        else if (ordinal == 13) baseName = "jyppx_ocv_aruco_dictionary_create_predefined";
        else if (ordinal == 14) baseName = "jyppx_ocv_aruco_dictionary_extend";
        else if (ordinal == 16) baseName = "jyppx_ocv_aruco_board_create";
        else if (ordinal == 18) baseName = "jyppx_ocv_aruco_board_get_object_points";
        else if (ordinal == 24) baseName = "jyppx_ocv_aruco_grid_board_create";
        else if (ordinal == 29) baseName = "jyppx_ocv_aruco_charuco_board_create";
        else if (ordinal == 35) baseName = "jyppx_ocv_aruco_charuco_board_get_chessboard_corners";
        else if (ordinal == 36) baseName = "jyppx_ocv_aruco_charuco_board_check_corners_collinear";
        else if (ordinal == 39) baseName = "jyppx_ocv_aruco_detector_default_params";
        else if (ordinal == 43) baseName = "jyppx_ocv_aruco_refine_default_params";
        else if (ordinal == 47) baseName = "jyppx_ocv_aruco_detector_create";
        else if (ordinal == 48) baseName = "jyppx_ocv_aruco_detector_create_multi_dictionary";
        else if (ordinal == 49) baseName = "jyppx_ocv_aruco_detector_detect_markers";
        else if (ordinal == 50) baseName = "jyppx_ocv_aruco_detector_detect_markers_with_confidence";
        else if (ordinal == 51) baseName = "jyppx_ocv_aruco_detector_refine_detected_markers";
        else if (ordinal == 52) baseName = "jyppx_ocv_aruco_detector_detect_markers_multi_dictionary";
        else if (ordinal == 55) return Names(native, "jyppx_ocv_aruco_detector_get_dictionaries_count", "jyppx_ocv_aruco_detector_get_dictionary_at");
        else if (ordinal == 63) baseName = "jyppx_ocv_aruco_draw_detected_markers";
        else if (ordinal == 64) baseName = "jyppx_ocv_aruco_dictionary_generate_image_marker";
        else if (ordinal is >= 66 and <= 75) baseName = GraphicalNativeBase(ordinal);
        else if (ordinal == 78) baseName = "jyppx_ocv_mcc_checker_create";
        else if (ordinal == 82) baseName = "jyppx_ocv_mcc_checker_set_charts_ycbcr";
        else if (ordinal == 89) baseName = "jyppx_ocv_mcc_checker_get_charts_ycbcr";
        else if (ordinal == 93) baseName = "jyppx_ocv_mcc_detector_default_params";
        else if (ordinal == 95) baseName = "jyppx_ocv_mcc_checker_detector_process_with_roi";
        else if (ordinal == 96) baseName = "jyppx_ocv_mcc_checker_detector_process";
        else if (ordinal == 99) baseName = "jyppx_ocv_mcc_checker_detector_create";
        else if (ordinal == 100) baseName = "jyppx_ocv_mcc_checker_detector_create_from_net";
        else if (ordinal == 114) baseName = "jyppx_ocv_qrcode_encoder_default_params";
        else if (ordinal == 115) baseName = "jyppx_ocv_qrcode_encoder_create";
        else if (ordinal == 117) baseName = "jyppx_ocv_qrcode_encoder_encode_structured_append";
        else if (ordinal == 119) baseName = "jyppx_ocv_qrcode_detector_create";
        else if (ordinal == 127) baseName = "jyppx_ocv_qrcode_detector_aruco_create";
        else if (ordinal == 129) baseName = "jyppx_ocv_qrcode_detector_aruco_default_params";
        else if (ordinal == 130) baseName = "jyppx_ocv_qrcode_detector_aruco_create_with_params";
        else if (ordinal == 137) baseName = "jyppx_ocv_calib3d_find_chessboard_corners";
        else if (ordinal == 138) baseName = "jyppx_ocv_calib3d_check_chessboard";
        else if (ordinal == 139) baseName = "jyppx_ocv_calib3d_find_chessboard_corners_sb_with_meta";
        else if (ordinal == 140) baseName = "jyppx_ocv_calib3d_find_chessboard_corners_sb";
        else if (ordinal == 141) baseName = "jyppx_ocv_calib3d_estimate_chessboard_sharpness";
        else if (ordinal == 142) baseName = "jyppx_ocv_calib3d_find_4_quad_corner_subpix";
        else if (ordinal == 143) baseName = "jyppx_ocv_calib3d_draw_chessboard_corners";
        else if (ordinal == 148) baseName = "jyppx_ocv_calib3d_find_circles_grid";
        else if (ordinal == 159) baseName = "jyppx_ocv_face_detector_yn_create";
        else if (ordinal == 160) baseName = "jyppx_ocv_face_detector_yn_create_from_buffer";
        else if (ordinal == 166) baseName = "jyppx_ocv_face_recognizer_sf_create";
        else if (ordinal == 167) baseName = "jyppx_ocv_face_recognizer_sf_create_from_buffer";
        else if (ordinal == 169) baseName = "jyppx_ocv_aruco_charuco_default_params";
        else if (ordinal == 171) baseName = "jyppx_ocv_aruco_charuco_detector_create";
        else if (ordinal == 180) baseName = "jyppx_ocv_aruco_charuco_detector_detect_board";
        else if (ordinal == 181) baseName = "jyppx_ocv_aruco_charuco_detector_detect_diamonds";
        else if (ordinal == 182) baseName = "jyppx_ocv_aruco_draw_detected_corners_charuco";
        else if (ordinal == 183) baseName = "jyppx_ocv_aruco_draw_detected_diamonds";
        else if (ordinal == 185) baseName = "jyppx_ocv_barcode_detector_create";
        else if (ordinal == 186) baseName = "jyppx_ocv_barcode_detector_create_with_super_resolution";
        else baseName = NativePrefix(declaration.Name) + ToSnake(method.StartsWith("get", StringComparison.Ordinal) || method.StartsWith("set", StringComparison.Ordinal) ? method : method);
        return MatchNative(native, baseName);
    }

    private static string GraphicalNativeBase(int ordinal) => ordinal switch
    {
        66 => "jyppx_ocv_qrcode_detector_detect", 67 or 73 => "jyppx_ocv_qrcode_detector_decode", 68 or 72 => "jyppx_ocv_qrcode_detector_detect_and_decode",
        69 => "jyppx_ocv_qrcode_detector_detect_multi", 70 or 74 => "jyppx_ocv_qrcode_detector_decode_multi", 71 or 75 => "jyppx_ocv_qrcode_detector_detect_and_decode_multi", _ => throw new InvalidOperationException()
    };

    private static string NativePrefix(string name)
    {
        if (name.StartsWith("cv.aruco.Dictionary.", StringComparison.Ordinal)) return "jyppx_ocv_aruco_dictionary_";
        if (name.StartsWith("cv.aruco.Board.", StringComparison.Ordinal)) return "jyppx_ocv_aruco_board_";
        if (name.StartsWith("cv.aruco.GridBoard.", StringComparison.Ordinal)) return "jyppx_ocv_aruco_grid_board_";
        if (name.StartsWith("cv.aruco.CharucoBoard.", StringComparison.Ordinal)) return "jyppx_ocv_aruco_charuco_board_";
        if (name.StartsWith("cv.aruco.ArucoDetector.", StringComparison.Ordinal)) return "jyppx_ocv_aruco_detector_";
        if (name.StartsWith("cv.mcc.CCheckerDetector.", StringComparison.Ordinal)) return "jyppx_ocv_mcc_checker_detector_";
        if (name.StartsWith("cv.mcc.CChecker.", StringComparison.Ordinal)) return "jyppx_ocv_mcc_checker_";
        if (name.StartsWith("cv.QRCodeEncoder.", StringComparison.Ordinal)) return "jyppx_ocv_qrcode_encoder_";
        if (name.StartsWith("cv.QRCodeDetector.", StringComparison.Ordinal)) return "jyppx_ocv_qrcode_detector_";
        if (name.StartsWith("cv.QRCodeDetectorAruco.", StringComparison.Ordinal)) return "jyppx_ocv_qrcode_detector_aruco_";
        if (name.StartsWith("cv.FaceDetectorYN.", StringComparison.Ordinal)) return "jyppx_ocv_face_detector_yn_";
        if (name.StartsWith("cv.FaceRecognizerSF.", StringComparison.Ordinal)) return "jyppx_ocv_face_recognizer_sf_";
        if (name.StartsWith("cv.aruco.CharucoDetector.", StringComparison.Ordinal)) return "jyppx_ocv_aruco_charuco_detector_";
        if (name.StartsWith("cv.barcode.BarcodeDetector.", StringComparison.Ordinal)) return "jyppx_ocv_barcode_detector_";
        throw new InvalidOperationException("No native owner for " + name);
    }

    private static List<string> MatchNative(string[] native, string baseName)
    {
        string[] suffixes = { "", "_count", "_fill", "_length" };
        var result = native.Where(x => suffixes.Any(s => x == baseName + s)).OrderBy(x => x, Ordinal).ToList();
        Require(result.Count > 0, "No native evidence for " + baseName);
        return result;
    }
    private static List<string> Names(string[] native, params string[] names)
    {
        foreach (string name in names) Require(native.Contains(name, Ordinal), "No native evidence for " + name);
        return names.OrderBy(x => x, Ordinal).ToList();
    }

    private static string ManagedEvidence(RawDeclaration declaration, string[] managed)
    {
        int ordinal = declaration.Ordinal;
        string type = ManagedType(declaration.Name);
        if (ordinal == 2) return FindManaged(managed, type, "|constructor|", "OpenCvSharp.Core.Mat bytesList");
        if (ordinal == 48) return FindManaged(managed, type, " Create(OpenCvSharp.ObjDetect.ArucoDictionary[] dictionaries");
        if (ordinal == 72) return FindManaged(managed, type, " DetectAndDecodeBytes(");
        if (ordinal == 73) return FindManaged(managed, type, " DecodeBytes(");
        if (ordinal == 74) return FindManaged(managed, type, " DecodeBytesMulti(");
        if (ordinal == 75) return FindManaged(managed, type, " DetectAndDecodeBytesMulti(");
        if (ordinal == 78) return FindManaged(managed, type, "|constructor|");
        if (ordinal == 95) return FindManaged(managed, type, " Process(", "regionsOfInterest");
        if (ordinal == 96) return FindManaged(managed, type, " Process(OpenCvSharp.Core.Mat image,System.Int32 nc=1)");
        if (ordinal == 99) return FindManaged(managed, type, "|constructor|", ".ctor()");
        if (ordinal == 100) return FindManaged(managed, type, "|constructor|", "OpenCvSharp.Dnn.Net net");
        if (ordinal == 127) return FindManaged(managed, type, "|constructor|", ".ctor()");
        if (ordinal == 130) return FindManaged(managed, type, "|constructor|", "QRCodeDetectorArucoParams parameters");
        if (ordinal == 139) return FindManaged(managed, type, " FindChessboardCornersSB(", "OpenCvSharp.Core.Mat meta");
        if (ordinal == 140) return managed.Where(x => x.Contains("MEMBER|" + type + "|", StringComparison.Ordinal) && x.Contains(" FindChessboardCornersSB(", StringComparison.Ordinal) && !x.Contains("OpenCvSharp.Core.Mat meta", StringComparison.Ordinal)).Single();
        if (ordinal == 159 || ordinal == 166) return FindManaged(managed, type, " Create(System.String model");
        if (ordinal == 160 || ordinal == 167) return FindManaged(managed, type, " Create(System.String framework,System.Byte[] modelBuffer");
        if (ordinal == 185) return FindManaged(managed, type, "|constructor|", ".ctor()");
        if (ordinal == 186) return FindManaged(managed, type, "|constructor|", "System.String superResolutionModelPath");

        string method = declaration.Name.Split('.').Last();
        if (method == declaration.Name.Split('.').Reverse().Skip(1).FirstOrDefault()) return FindManaged(managed, type, "|constructor|");
        if (declaration.Name.EndsWith("." + method, StringComparison.Ordinal) && IsConstructor(declaration.Name, method)) return FindManaged(managed, type, "|constructor|");
        string managedName = ManagedName(ordinal, method);
        string typePrefix = "MEMBER|" + type + "|";
        List<string> candidates = managed.Where(x => x.Contains(typePrefix, StringComparison.Ordinal) && (x.Contains(" " + managedName + "(", StringComparison.Ordinal) || (x.Contains("|property|", StringComparison.Ordinal) && x.EndsWith(" " + managedName, StringComparison.Ordinal)))).ToList();
        if (candidates.Count == 0 && (method.StartsWith("get", StringComparison.Ordinal) || method.StartsWith("set", StringComparison.Ordinal)))
        {
            string property = method.Substring(3);
            candidates = managed.Where(x => x.Contains(typePrefix, StringComparison.Ordinal) && x.Contains("|property|", StringComparison.Ordinal) && x.EndsWith(" " + property, StringComparison.Ordinal)).ToList();
        }
        Require(candidates.Count > 0, "No managed evidence for " + declaration.Identity + " using " + type + "." + managedName);
        return candidates.OrderBy(x => x, Ordinal).First();
    }

    private static bool IsConstructor(string name, string method)
    {
        string[] parts = name.Split('.');
        return parts.Length >= 2 && parts[^2] == method;
    }

    private static string ManagedType(string name)
    {
        if (name.StartsWith("cv.aruco.Dictionary", StringComparison.Ordinal) || name == "cv.aruco.getPredefinedDictionary" || name == "cv.aruco.extendDictionary" || name == "cv.aruco.generateImageMarker") return "OpenCvSharp.ObjDetect.ArucoDictionary";
        if (name.StartsWith("cv.aruco.Board", StringComparison.Ordinal)) return "OpenCvSharp.ObjDetect.ArucoBoard";
        if (name.StartsWith("cv.aruco.GridBoard", StringComparison.Ordinal)) return "OpenCvSharp.ObjDetect.ArucoGridBoard";
        if (name.StartsWith("cv.aruco.CharucoBoard", StringComparison.Ordinal)) return "OpenCvSharp.ObjDetect.CharucoBoard";
        if (name.StartsWith("cv.aruco.DetectorParameters", StringComparison.Ordinal)) return "OpenCvSharp.ObjDetect.ArucoDetectorParameters";
        if (name.StartsWith("cv.aruco.RefineParameters", StringComparison.Ordinal)) return "OpenCvSharp.ObjDetect.ArucoRefineParameters";
        if (name.StartsWith("cv.aruco.ArucoDetector", StringComparison.Ordinal) || name == "cv.aruco.drawDetectedMarkers") return "OpenCvSharp.ObjDetect.ArucoDetector";
        if (name.StartsWith("cv.GraphicalCodeDetector", StringComparison.Ordinal) || name.StartsWith("cv.QRCodeDetector.", StringComparison.Ordinal)) return "OpenCvSharp.ObjDetect.QRCodeDetector";
        if (name.StartsWith("cv.mcc.CCheckerDetector", StringComparison.Ordinal)) return "OpenCvSharp.ObjDetect.CCheckerDetector";
        if (name.StartsWith("cv.mcc.CChecker", StringComparison.Ordinal)) return "OpenCvSharp.ObjDetect.CChecker";
        if (name.StartsWith("cv.mcc.DetectorParametersMCC", StringComparison.Ordinal)) return "OpenCvSharp.ObjDetect.DetectorParametersMCC";
        if (name.StartsWith("cv.QRCodeEncoder.Params", StringComparison.Ordinal)) return "OpenCvSharp.ObjDetect.QRCodeEncoderParams";
        if (name.StartsWith("cv.QRCodeEncoder", StringComparison.Ordinal)) return "OpenCvSharp.ObjDetect.QRCodeEncoder";
        if (name.StartsWith("cv.QRCodeDetectorAruco.Params", StringComparison.Ordinal)) return "OpenCvSharp.ObjDetect.QRCodeDetectorArucoParams";
        if (name.StartsWith("cv.QRCodeDetectorAruco", StringComparison.Ordinal)) return "OpenCvSharp.ObjDetect.QRCodeDetectorAruco";
        if (name.StartsWith("cv.find", StringComparison.Ordinal) || name.StartsWith("cv.checkChessboard", StringComparison.Ordinal) || name.StartsWith("cv.estimateChessboard", StringComparison.Ordinal) || name.StartsWith("cv.drawChessboard", StringComparison.Ordinal)) return "OpenCvSharp.Calib3D.Cv2";
        if (name.StartsWith("cv.FaceDetectorYN", StringComparison.Ordinal)) return "OpenCvSharp.ObjDetect.FaceDetectorYN";
        if (name.StartsWith("cv.FaceRecognizerSF", StringComparison.Ordinal)) return "OpenCvSharp.ObjDetect.FaceRecognizerSF";
        if (name.StartsWith("cv.aruco.CharucoParameters", StringComparison.Ordinal)) return "OpenCvSharp.ObjDetect.CharucoParameters";
        if (name.StartsWith("cv.aruco.CharucoDetector", StringComparison.Ordinal) || name.StartsWith("cv.aruco.drawDetectedCornersCharuco", StringComparison.Ordinal) || name.StartsWith("cv.aruco.drawDetectedDiamonds", StringComparison.Ordinal)) return "OpenCvSharp.ObjDetect.CharucoDetector";
        if (name.StartsWith("cv.barcode.BarcodeDetector", StringComparison.Ordinal)) return "OpenCvSharp.ObjDetect.BarcodeDetector";
        throw new InvalidOperationException("No managed owner for " + name);
    }

    private static string ManagedName(int ordinal, string method)
    {
        return ordinal switch
        {
            13 => "GetPredefinedDictionary", 14 => "Extend", 18 => "ObjectPoints", 19 => "Ids", 20 => "RightBottomCorner",
            25 => "GridSize", 26 or 34 => "MarkerLength", 27 => "MarkerSeparation", 30 or 31 => "LegacyPattern", 32 => "ChessboardSize", 33 => "SquareLength", 35 => "GetChessboardCorners", 36 => "CheckCharucoCornersCollinear",
            52 => "DetectMarkersMultiDictionary", 55 => "GetDictionaries", 63 => "DrawDetectedMarkers", 64 => "GenerateImageMarker",
            66 => "Detect", 67 => "Decode", 68 => "DetectAndDecode", 69 => "DetectMulti", 70 => "DecodeMulti", 71 => "DetectAndDecodeMulti",
            79 or 85 => "Target", 80 or 86 => method.StartsWith("get") ? "GetBox" : "SetBox", 81 or 88 => method.StartsWith("get") ? "GetChartsRGB" : "SetChartsRGB", 82 or 89 => method.StartsWith("get") ? "GetChartsYCbCr" : "SetChartsYCbCr", 83 or 90 => "Cost", 84 or 91 => "Center", 87 => "GetColorCharts",
            97 => "GetBestColorChecker", 98 => "GetListColorChecker", 101 => "Draw", 102 => "GetRefColors", 103 or 107 => method.StartsWith("get") ? "GetDetectionParams" : "SetDetectionParams", 104 or 108 => "ColorChartType", 105 or 106 => "UseDnnModel",
            137 => "FindChessboardCorners", 138 => "CheckChessboard", 141 => "EstimateChessboardSharpness", 142 => "Find4QuadCornerSubpix", 143 => "DrawChessboardCorners", 148 => "FindCirclesGrid",
            150 or 151 => "InputSize", 152 or 153 => "ScoreThreshold", 154 or 155 => "NMSThreshold", 156 or 157 => "TopK",
            182 => "DrawDetectedCorners", 183 => "DrawDetectedDiamonds", 189 or 190 => "DownsamplingThreshold", 191 => "GetDetectorScales", 192 => "SetDetectorScales", 193 or 194 => "GradientThreshold",
            _ => char.ToUpperInvariant(method[0]) + method.Substring(1)
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
        Require(raw.SchemaVersion == 1 && raw.UpstreamOpenCvVersion == "5.0.0" && raw.DeclarationCount == 195 && raw.Declarations.Count == 195, "ObjDetect raw identity/count drifted.");
        Require(raw.Declarations.Count(x => x.Kind == "callable") == 163 && raw.Declarations.Count(x => x.Kind == "class") == 22 && raw.Declarations.Count(x => x.Kind == "enum") == 10, "ObjDetect declaration partition drifted.");
        Require(raw.SourceHeaders.Count == 9 && raw.CompatibilityHeaders.Count == 2, "ObjDetect header closure drifted.");
        Require(raw.PreprocessorDefinitions.Count == 3 && raw.PreprocessorDefinitions.GetValueOrDefault("CV_VERSION_MAJOR") == 5 && raw.PreprocessorDefinitions.GetValueOrDefault("OPENCV_ABI_COMPATIBILITY") == 500 && raw.PreprocessorDefinitions.GetValueOrDefault("HAVE_OPENCV_DNN") == 1, "ObjDetect parser definitions drifted.");
        int[] starts = { 0, 15, 37, 65, 76, 109, 149, 168, 184 }; int[] counts = { 15, 22, 28, 11, 33, 40, 19, 16, 11 };
        for (int i = 0; i < 9; i++) Require(raw.SourceHeaders[i].StartOrdinal == starts[i] && raw.SourceHeaders[i].DeclarationCount == counts[i], "ObjDetect source-header order drifted at " + i);
        for (int i = 0; i < raw.Declarations.Count; i++)
        {
            SourceHeader owner = raw.SourceHeaders.Last(h => h.StartOrdinal <= i);
            Require(raw.Declarations[i].Ordinal == i && !string.IsNullOrWhiteSpace(raw.Declarations[i].Identity) && raw.Declarations[i].SourceHeader == owner.Path, "ObjDetect parser order/source/identity drifted at " + i);
        }
        Require(raw.Declarations.Select(x => x.Identity).Distinct(Ordinal).Count() == 195, "ObjDetect overload identities collapsed.");
        Require(classifications.SchemaVersion == 1 && classifications.UpstreamOpenCvVersion == "5.0.0" && classifications.ClaimedSlice == ClaimedSlice && classifications.ReviewStatus == "source-reviewed" && !string.IsNullOrWhiteSpace(classifications.Limitation), "ObjDetect classification identity drifted.");
        Require(!classifications.ClaimedSlice.Contains("OpenCv5Sharp", StringComparison.Ordinal) && classifications.Declarations.Count == 195, "ObjDetect fixed-major identity or row count drifted.");
        var nativeSet = new HashSet<string>(native, Ordinal); var managedSet = new HashSet<string>(managed, Ordinal);
        for (int i = 0; i < 195; i++)
        {
            RawDeclaration declaration = raw.Declarations[i]; ClassificationRow row = classifications.Declarations[i];
            Require(row.Ordinal == i && row.Identity == declaration.Identity && Allowed.Contains(row.Classification, Ordinal), "ObjDetect classification order/value drifted at " + i);
            Require(!string.IsNullOrWhiteSpace(row.Reason) && !string.IsNullOrWhiteSpace(row.BuildCondition), "Undocumented ObjDetect row at " + i);
            Require(row.NativeEntrypoints.SequenceEqual(row.NativeEntrypoints.Distinct(Ordinal).OrderBy(x => x, Ordinal)) && row.ManagedMembers.SequenceEqual(row.ManagedMembers.Distinct(Ordinal).OrderBy(x => x, Ordinal)), "Nondeterministic ObjDetect evidence ordering at " + i);
            Require(declaration.Kind == "callable" ? row.Classification != "non-callable-metadata" : row.Classification == "non-callable-metadata", "ObjDetect callable/metadata confusion at " + i);
            if (row.Classification == "implemented")
            {
                Require(row.NativeEntrypoints.Count > 0 && row.ManagedMembers.Count > 0 && row.NativeEntrypoints.All(nativeSet.Contains) && row.ManagedMembers.All(managedSet.Contains), "False or missing ObjDetect evidence at " + i);
            }
            else if (declaration.Kind == "callable") Require(row.NativeEntrypoints.Count == 0 && row.ManagedMembers.Count == 0, "Non-implemented ObjDetect callable carries evidence at " + i);
            if (Omitted.Contains(i)) Require(row.Classification == "intentionally-omitted", "ObjDetect omission drifted at " + i);
            if (i == 100 || i is >= 149 and <= 167) Require(row.BuildCondition.Contains("HAVE_OPENCV_DNN=1", StringComparison.Ordinal), "ObjDetect DNN build condition missing at " + i);
        }
        Require(classifications.Declarations.Count(x => x.Classification == "implemented") == 153 && classifications.Declarations.Count(x => x.Classification == "intentionally-omitted") == 10 && classifications.Declarations.All(x => x.Classification != "missing"), "ObjDetect callable partition drifted.");
        Require(Selected.All(i => classifications.Declarations[i].Classification == "implemented"), "Selected ObjDetect batch is incomplete.");
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
        b.AppendLine("# Generated by tools/ObjDetectUpstreamMap. Do not edit."); b.AppendLine("schema-version=1"); b.AppendLine("upstream-opencv-version=5.0.0"); b.AppendLine("claimed-slice=" + ClaimedSlice); b.AppendLine("header-sha256=" + raw.HeaderSha256); b.AppendLine("parser-sha256=" + raw.ParserSha256); b.AppendLine("declaration-count=195"); b.AppendLine("callable-count=163"); b.AppendLine("class-count=22"); b.AppendLine("enum-count=10"); b.AppendLine("repository-wide-upstream-parity-claimed=false");
        foreach (CompatibilityHeader h in raw.CompatibilityHeaders) b.AppendLine($"compatibility-header={h.Path}|{h.Sha256}|includes={h.Includes}");
        b.AppendLine(); b.AppendLine("ordinal|kind|source-header|classification|identity|native-entrypoints|managed-members|build-condition|reason");
        for (int i = 0; i < 195; i++)
        {
            RawDeclaration d = raw.Declarations[i]; ClassificationRow r = classifications.Declarations[i];
            b.AppendLine($"{i}|{d.Kind}|{d.SourceHeader}|{r.Classification}|{d.Identity}|{Join(r.NativeEntrypoints)}|{Join(r.ManagedMembers)}|{r.BuildCondition}|{r.Reason}");
        }
        return b.ToString().Replace("\r\n", "\n", StringComparison.Ordinal);
    }

    private static FamilyDocument BuildFamilies(RawDocument raw, ClassificationDocument classifications)
    {
        var family = new FamilyRow();
        foreach (int i in Selected.OrderBy(x => x)) family.Declarations.Add(new FamilyOperation { Ordinal = i, UpstreamIdentity = raw.Declarations[i].Identity, NativeEntrypoints = new(classifications.Declarations[i].NativeEntrypoints), ManagedMembers = new(classifications.Declarations[i].ManagedMembers) });
        return new FamilyDocument { Families = new() { family } };
    }

    private static void RunNegativeFixtures(RawDocument raw, ClassificationDocument classifications, Options options, string[] native, string[] managed)
    {
        int passed = 0;
        void Fixture(Action<RawDocument, ClassificationDocument> mutate)
        {
            RawDocument rc = Clone(raw); ClassificationDocument cc = Clone(classifications); mutate(rc, cc); bool failed = false;
            try { Validate(rc, cc, options, native, managed, true); } catch { failed = true; }
            Require(failed, "An ObjDetect negative fixture was accepted."); passed++;
        }
        Fixture((_, c) => c.Declarations.RemoveAt(0)); Fixture((_, c) => c.Declarations[1].Ordinal = 0); Fixture((_, c) => (c.Declarations[0], c.Declarations[1]) = (c.Declarations[1], c.Declarations[0]));
        Fixture((r, _) => r.Declarations[2].Identity = r.Declarations[1].Identity); Fixture((_, c) => c.Declarations[15].Classification = "implemented"); Fixture((r, _) => r.Declarations[0].SourceHeader = "drifted/objdetect.hpp");
        Fixture((r, _) => r.ParserSha256 = new string('0', 64)); Fixture((r, _) => r.HeaderSha256 = new string('0', 64)); Fixture((_, c) => c.Declarations[14].NativeEntrypoints[0] = "jyppx_ocv_false_evidence");
        Fixture((_, c) => c.Declarations[14].ManagedMembers[0] = "MEMBER|false"); Fixture((_, c) => c.Declarations[3].Reason = ""); Fixture((_, c) => c.ClaimedSlice += "; OpenCv5Sharp");
        Fixture((_, c) => c.Declarations[100].BuildCondition = "unconditional"); Fixture((_, c) => c.Declarations[18].NativeEntrypoints.Reverse()); Fixture((r, _) => r.SourceHeaders.RemoveAt(0));
        Require(passed == 15, "ObjDetect negative fixture count drifted.");
    }

    private static string ToSnake(string value)
    {
        var b = new StringBuilder();
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];
            if (char.IsUpper(c) && i > 0 && (char.IsLower(value[i - 1]) || (i + 1 < value.Length && char.IsLower(value[i + 1])))) b.Append('_');
            b.Append(char.ToLowerInvariant(c));
        }
        return b.ToString();
    }
    private static string[] ReadNative(string path) => File.ReadAllLines(path, Encoding.UTF8).Where(x => x.StartsWith("jyppx_ocv_", StringComparison.Ordinal)).Select(x => x.Split('|')[0]).OrderBy(x => x, Ordinal).ToArray();
    private static T Read<T>(string path) => JsonSerializer.Deserialize<T>(File.ReadAllText(path, Encoding.UTF8), JsonOptions()) ?? throw new InvalidOperationException("Could not parse " + path);
    private static T Clone<T>(T value) => JsonSerializer.Deserialize<T>(Serialize(value), JsonOptions())!;
    private static JsonSerializerOptions JsonOptions() => new() { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };
    private static string Serialize<T>(T value) => JsonSerializer.Serialize(value, JsonOptions()) + "\n";
    private static void WriteOrCheck(string path, string content, bool check)
    {
        content = content.Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\r", "\n", StringComparison.Ordinal);
        if (check) { Require(File.Exists(path) && File.ReadAllText(path, Encoding.UTF8).Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\r", "\n", StringComparison.Ordinal) == content, "Generated file is missing or stale: " + path); return; }
        Directory.CreateDirectory(Path.GetDirectoryName(path)!); File.WriteAllText(path, content, new UTF8Encoding(false));
    }
    private static void VerifyHash(string workspace, string relative, string expected, string label)
    {
        string path = Path.Combine(workspace, relative.Replace('/', Path.DirectorySeparatorChar)); Require(File.Exists(path), "ObjDetect " + label + " missing: " + relative); Require(Sha256File(path) == expected, "ObjDetect " + label + " hash drifted: " + relative);
    }
    private static string Join(List<string> values) => values.Count == 0 ? "-" : string.Join(";", values.Select(x => x.Replace("|", "<pipe>", StringComparison.Ordinal)));
    private static string Rel(string root, string path) => Path.GetRelativePath(root, path).Replace('\\', '/');
    private static string Sha256(string text) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
    private static string Sha256File(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}

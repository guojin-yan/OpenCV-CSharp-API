using System.Text;
using System.Text.Json;
using System.Security.Cryptography;

internal static partial class Program
{
    private const string RefreshRoot = "compatibility/opencv-5.1.0";
    private const string RefreshCommit = "e4f78fbcf1a773fbed69e14500e8b734963eb585";
    private const string RefreshPrefix = "opencv-source/opencv-5.1.0/";

    private sealed record RefreshDecision(int? HistoricalOrdinal, string Classification, string Reason);

    private sealed class RefreshDelta
    {
        public string FromVersion { get; init; } = "5.0.0";
        public string ToVersion { get; init; } = "5.1.0";
        public List<string> Added { get; init; } = new();
        public List<string> Removed { get; init; } = new();
        public List<object> Changed { get; init; } = new();
        public int RetainedIdentityCount { get; set; }
    }

    private static int Refresh51(string[] args)
    {
        Options options = Parse(args);
        var json = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        RawDocument historical = Read<RawDocument>(Path.Combine(options.Repository, "compatibility/dnn-upstream-raw.json"), json);
        ClassificationDocument previous = Read<ClassificationDocument>(Path.Combine(options.Repository, "compatibility/dnn-upstream-classifications.json"), json);
        RawDocument raw = Read<RawDocument>(options.Raw, json);
        string[] native = ReadNative(options.NativeManifest);
        string[] managed = ReadManaged(options.ManagedBaseline);
        Require(FileSha(options.Repository, "compatibility/dnn-upstream-raw.json") == "1a9d121e7e06a8059612e368f9dfc83466a23b2ceafd3f505bb0aecb08be784a" &&
            FileSha(options.Repository, "compatibility/dnn-upstream-classifications.json") == "64fec89890aba7733af902139cde29f3547cb07c99ecb1cd6f7f81228a85156c",
            "The reviewed historical DNN input snapshot drifted.");
        Validate(historical, previous, options, native, managed, true);
        Validate51Raw(raw, options);
        ClassificationDocument expected = Classify51(raw, historical, previous, native, managed);
        if (options.Initialize) WriteOrCheck(options.Classification, Serialize(expected), false);
        ClassificationDocument classified = Read<ClassificationDocument>(options.Classification, json);
        Validate51Classification(raw, classified, historical, previous, native, managed);
        RefreshDelta delta = Delta51(historical, raw);
        Run51Fixtures(raw, classified, historical, previous, native, managed, options, delta);

        string map = Build51Map(raw, classified);
        string families = Serialize(new
        {
            schemaVersion = 1,
            upstreamOpenCvVersion = "5.1.0",
            status = "reviewed-parser-slice",
            implemented = classified.Declarations.Where(value => value.Classification == "implemented"),
            limitations = new[]
            {
                "This is the three-header parser slice, not the complete DNN C++ surface or an ORT runtime certification.",
                "Legacy image-to-blob APIs use INTER_LINEAR; the new interpolation argument/property is not exposed.",
                "The historical 5.0.0 artifacts retain their original source and version."
            }
        });
        var counts = new SortedDictionary<string, int>(Ordinal);
        foreach (string value in Classifications) counts[value] = classified.Declarations.Count(row => row.Classification == value);
        string deltaPath = Path.Combine(options.Repository, RefreshRoot, "dnn-upstream-delta.json");
        string deltaText = Serialize(delta);
        string summary = Serialize(new
        {
            schemaVersion = 1,
            generator = "tools/DnnUpstreamMap --refresh-5.1",
            upstreamOpenCvVersion = "5.1.0",
            upstreamCommit = RefreshCommit,
            sourceHashPolicy = "UTF-8 source with CRLF/CR normalized to LF; no trimming",
            claimedSlice = ClaimedSlice,
            rawExtractionPath = Rel(options.Repository, options.Raw),
            classificationPath = Rel(options.Repository, options.Classification),
            mappingPath = Rel(options.Repository, options.Output),
            familyInventoryPath = Rel(options.Repository, options.FamilyOutput),
            deltaPath = Rel(options.Repository, deltaPath),
            mappingSha256 = Sha256(map),
            familyInventorySha256 = Sha256(families),
            deltaSha256 = Sha256(deltaText),
            rawSha256 = FileSha(options.Repository, Rel(options.Repository, options.Raw)),
            declarationSnapshotSha256 = Sha256(Serialize(raw)),
            historicalRawSha256 = FileSha(options.Repository, "compatibility/dnn-upstream-raw.json"),
            historicalClassificationSha256 = FileSha(options.Repository, "compatibility/dnn-upstream-classifications.json"),
            declarationCount = raw.Declarations.Count,
            callableCount = raw.Declarations.Count(value => value.Kind == "callable"),
            classCount = raw.Declarations.Count(value => value.Kind == "class"),
            enumCount = raw.Declarations.Count(value => value.Kind == "enum"),
            sourceHeaderCount = raw.SourceHeaders.Count,
            classificationCounts = counts,
            nativeEvidenceCount = classified.Declarations.SelectMany(value => value.NativeEntrypoints).Distinct(Ordinal).Count(),
            managedEvidenceCount = classified.Declarations.SelectMany(value => value.ManagedMembers).Distinct(Ordinal).Count(),
            addedIdentityCount = delta.Added.Count,
            removedIdentityCount = delta.Removed.Count,
            retainedIdentityCount = delta.RetainedIdentityCount,
            changedRetainedIdentityCount = delta.Changed.Count,
            negativeFixtureCount = 22,
            repositoryWideUpstreamParityClaimed = false,
            completeDnnCppParityClaimed = false,
            ortRuntimeValidated = false,
            abiEngineProjection = new[]
            {
                new { name = "Auto", abiValue = 3, upstreamValue = 0 },
                new { name = "Classic", abiValue = 1, upstreamValue = 1 },
                new { name = "New", abiValue = 2, upstreamValue = 1 },
                new { name = "Ort", abiValue = 4, upstreamValue = 2 }
            },
            abiModelFormatProjection = new[]
            {
                new { name = "Generic", upstreamValue = 0, abiValue = 0 },
                new { name = "Onnx", upstreamValue = 1, abiValue = 1 },
                new { name = "TensorFlowLite", upstreamValue = 2, abiValue = 3 }
            }
        });
        WriteOrCheck(options.Output, map, options.Check);
        WriteOrCheck(options.FamilyOutput, families, options.Check);
        WriteOrCheck(deltaPath, deltaText, options.Check);
        WriteOrCheck(options.Summary, summary, options.Check);
        Console.WriteLine($"DNN_510_UPSTREAM_MAP_OK declarations={raw.Declarations.Count} callables=161 implemented={counts["implemented"]} omitted={counts["intentionally-omitted"]} unsupported={counts["unsupported"]} missing={counts["missing"]} added={delta.Added.Count} removed={delta.Removed.Count} fixtures=22 mode={(options.Check ? "check" : "write")}");
        return 0;
    }

    private static void Validate51Raw(RawDocument raw, Options options)
    {
        Require(raw.SchemaVersion == 1 && raw.UpstreamOpenCvVersion == "5.1.0" && raw.Generator == "tools/DnnUpstreamMap/extract_dnn.py", "5.1 DNN raw identity drifted.");
        Require(raw.HeaderPath == RefreshPrefix + "modules/dnn/include/opencv2/dnn.hpp" &&
            raw.ParserPath == RefreshPrefix + "modules/python/src2/hdr_parser.py", "5.1 DNN source scope drifted.");
        Require(raw.DeclarationCount == 185 && raw.Declarations.Count == 185 && raw.Declarations.Count(value => value.Kind == "callable") == 161,
            "5.1 DNN declaration partition drifted.");
        Require(raw.PreprocessorDefinitions.Count == 2 && raw.PreprocessorDefinitions.GetValueOrDefault("CV_VERSION_MAJOR") == 5 &&
            raw.PreprocessorDefinitions.GetValueOrDefault("OPENCV_ABI_COMPATIBILITY") == 500, "5.1 DNN parser definitions drifted.");
        string[] headers = { "dict.hpp", "dnn.hpp", "utils/inference_engine.hpp" };
        Require(raw.SourceHeaders.Select(value => value.Path).SequenceEqual(headers.Select(value => RefreshPrefix + "modules/dnn/include/opencv2/dnn/" + value)),
            "5.1 DNN source closure drifted.");
        Require(raw.SourceHeaders.Select(value => value.StartOrdinal).SequenceEqual(new[] { 0, 10, 179 }) &&
            raw.SourceHeaders.Select(value => value.DeclarationCount).SequenceEqual(new[] { 10, 169, 6 }), "5.1 DNN source partition drifted.");
        Require(raw.Declarations.Select(value => value.Identity).Distinct(Ordinal).Count() == raw.Declarations.Count, "5.1 DNN duplicate identity.");
        for (int i = 0; i < raw.Declarations.Count; ++i)
        {
            RawDeclaration row = raw.Declarations[i];
            Require(row.Ordinal == i, "5.1 DNN ordinal drifted.");
            SourceHeader header = raw.SourceHeaders.Single(value => i >= value.StartOrdinal && i < value.StartOrdinal + value.DeclarationCount);
            Require(row.SourceHeader == header.Path, "5.1 DNN declaration source drifted.");
            string identity = row.Kind == "enum" ? $"enum {row.Name}[{string.Join(";", row.EnumValues.Select(value => $"{value.Name}={value.Value}"))}]" :
                row.Kind == "class" ? "class " + row.Name : $"{row.Name}({string.Join(";", row.Arguments.Select(value => value.Type + " " + value.Name +
                    (value.Default.Length > 0 ? "=" + value.Default : "") + (value.Modifiers.Count > 0 ? "[" + string.Join(",", value.Modifiers) + "]" : "")))})->{row.ReturnType}";
            Require(row.Identity == identity, "5.1 DNN signature identity drifted.");
        }
        Require(SourceSha51(options.Workspace, raw.HeaderPath) == raw.HeaderSha256 && SourceSha51(options.Workspace, raw.ParserPath) == raw.ParserSha256,
            "5.1 DNN source SHA256 drifted.");
        foreach (SourceHeader header in raw.SourceHeaders)
            Require(SourceSha51(options.Workspace, header.Path) == header.Sha256, "5.1 DNN header SHA256 drifted.");
        // Pin the official inputs, independently of mutable JSON source hashes.
        Require(raw.HeaderSha256 == "f2012cb1f4980d641f9dcacba7694c5b129af5589391154e2b2f00477ac31309" &&
            SourceSha51(options.Workspace, RefreshPrefix + "modules/core/include/opencv2/core/version.hpp") == "bc436bd455e88946523bd83d441320b44ecc9cdd6e65718cd4fc78f4cd6defb7" &&
            raw.ParserSha256 == "867e3de5ad45fd3996e8c9097ffc47f13c8651e20dda2318d70e273262c8dcee" &&
            raw.SourceHeaders.Select(value => value.Sha256).SequenceEqual(new[]
            {
                "5e2d14d8c9bf1d44a755efa6ab259c38ea2971979a5870920cd2c2c0bbd326b0",
                "24e86c0f2fd8f9ca68beb787526496032c1a17c97db6671083ec3372cb624658",
                "0deb20d27d8dbe6ca41cbc6e49fd082d8a7f3108c181be905e480cc5fa257047"
            }), "5.1 DNN official source drifted.");
        Require(Sha256(Serialize(raw)) == "c8e93297cee63d76eddfb37db94f4e85ccf5f61a58629cfd81784507cdfbb44a",
            "5.1 DNN reviewed declaration snapshot drifted.");
    }

    private static string SourceSha51(string workspace, string relative) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
        File.ReadAllText(Path.Combine(workspace, relative)).Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n')))).ToLowerInvariant();

    private static ClassificationDocument Classify51(RawDocument raw, RawDocument old, ClassificationDocument previous, string[] native, string[] managed)
    {
        Dictionary<string, RawDeclaration> historical = old.Declarations.ToDictionary(value => value.Identity, Ordinal);
        var result = new ClassificationDocument { UpstreamOpenCvVersion = "5.1.0" };
        foreach (RawDeclaration declaration in raw.Declarations)
        {
            RefreshDecision decision = Decision51(declaration, historical, previous);
            var row = new ClassificationRow
            {
                Ordinal = declaration.Ordinal,
                Identity = declaration.Identity,
                Classification = decision.Classification,
                Reason = decision.Reason
            };
            if (decision.HistoricalOrdinal is int index)
            {
                if (row.Classification == "implemented")
                {
                    row.NativeEntrypoints = ResolveNative(index, native).ToList();
                    row.ManagedMembers = ResolveManaged(index, managed).ToList();
                    Require(row.NativeEntrypoints.Count > 0 && row.ManagedMembers.Count > 0, "5.1 DNN implemented evidence missing.");
                }
                if (row.Classification == "upstream-conditional") row.BuildCondition = previous.Declarations[index].BuildCondition;
            }
            result.Declarations.Add(row);
        }
        return result;
    }

    private static RefreshDecision Decision51(RawDeclaration row, Dictionary<string, RawDeclaration> historical, ClassificationDocument previous)
    {
        if (row.Kind != "callable") return new(null, "non-callable-metadata", "5.1 parser type shape; changed EngineType/ModelFormat values and inherited LayerInfo properties are recorded in the source delta. This is not a new managed type or a direct enum cast contract.");
        if (row.Name == "cv.dnn.Net.dumpToPbtxt") return new(null, "unsupported", "The retained API rejects 5.1 ONNX PBTXT export before the upstream exporter can clear the graph; DumpToFile remains usable. No ONNX PBTXT parity is claimed.");
        if (historical.TryGetValue(row.Identity, out RawDeclaration? old))
        {
            ClassificationRow prior = previous.Declarations[old.Ordinal];
            return new(old.Ordinal, prior.Classification, prior.Reason);
        }
        if (row.Identity == "cv.dnn.LayerInfo.outputNameToIndex(String outputName[/C,/Ref])->int")
            return new(20, "implemented", "The method moved from Layer to its LayerInfo base; the owned cv::Ptr<Layer> wrapper calls the inherited method and retains the parent-independent lifetime contract.");
        if (row.Name is "cv.dnn.blobFromImage" or "cv.dnn.blobFromImages" && row.Arguments.Count == 8 && row.Arguments[^1].Name == "interpolation" && row.Arguments[^1].Default == "cv::INTER_LINEAR")
            return new(null, "intentionally-omitted", "The 5.1 signature adds interpolation. The existing managed/C ABI helper retains INTER_LINEAR through the upstream default; non-default interpolation is not exposed and the full new signature is not classified implemented.");
        if (row.Name == "cv.dnn.Image2BlobParams.Image2BlobParams" && row.Arguments.Count == 9 && row.Arguments[^1].Name == "interpolation" && row.Arguments[^1].Default == "cv::INTER_LINEAR")
            return new(null, "intentionally-omitted", "The new constructor argument and interpolation property are not in the immutable managed parameters or ABI 1 transport; existing eight-argument construction retains INTER_LINEAR.");
        if (row.Identity is "cv.dnn.Net.enableTF32(bool useTF32)->void" or "cv.dnn.Model.enableTF32(bool useTF32)->Model")
            return new(null, "intentionally-omitted", "TF32 control is new in 5.1 and needs an additive native entrypoint, managed method, backend policy and numeric accuracy evidence; no such wrapper is currently provided.");
        if (row.Identity == "cv.dnn.Net.reserveKVCache(int maxSequenceLength)->void")
            return new(null, "intentionally-omitted", "Explicit KV-cache capacity reservation is distinct from the existing enable/reset/disable workflow and needs its own allocation and model-backed lifetime evidence.");
        if (row.Identity == "cv.dnn.readNetFromONNX(String onnxFile[/PATH,/C,/Ref];int engine;bool mapExternalData)->Net")
            return new(null, "intentionally-omitted", "Memory-mapped external ONNX data requires files to remain present and unmodified throughout Net lifetime; the existing path reader eagerly reads external data and does not offer this flag.");
        if (row.Identity == "cv.dnn.Tokenizer.encode(vector_string textChunks[/C,/Ref])->std::vector<int>")
            return new(null, "intentionally-omitted", "Multi-chunk tokenization requires the separately deferred Tokenizer model, UTF-8 ownership and post-processor pair-template contract.");
        throw new InvalidDataException("Unreviewed 5.1 DNN callable: " + row.Identity);
    }

    private static void Validate51Classification(RawDocument raw, ClassificationDocument value, RawDocument historical, ClassificationDocument previous, string[] native, string[] managed)
    {
        Require(value.UpstreamOpenCvVersion == "5.1.0" && value.SchemaVersion == 1 && value.ClaimedSlice == ClaimedSlice && value.ReviewStatus == "reviewed", "5.1 DNN classification identity drifted.");
        Require(value.Declarations.Count == raw.Declarations.Count, "5.1 DNN classification requires one row per declaration.");
        Require(value.Declarations.Select(row => row.Identity).Distinct(Ordinal).Count() == value.Declarations.Count, "5.1 DNN duplicate classification identity.");
        ClassificationDocument expected = Classify51(raw, historical, previous, native, managed);
        for (int i = 0; i < value.Declarations.Count; ++i)
        {
            Require(value.Declarations[i].Ordinal == i && value.Declarations[i].Identity == raw.Declarations[i].Identity, "5.1 DNN classification order drifted.");
            Require(Serialize(value.Declarations[i]) == Serialize(expected.Declarations[i]), "5.1 DNN reviewed classification/evidence drifted: " + raw.Declarations[i].Identity);
        }
    }

    private static RefreshDelta Delta51(RawDocument historical, RawDocument raw)
    {
        Dictionary<string, RawDeclaration> old = historical.Declarations.ToDictionary(value => value.Identity, Ordinal);
        Dictionary<string, RawDeclaration> current = raw.Declarations.ToDictionary(value => value.Identity, Ordinal);
        var result = new RefreshDelta();
        result.Added.AddRange(raw.Declarations.Where(value => !old.ContainsKey(value.Identity)).Select(value => value.Identity));
        result.Removed.AddRange(historical.Declarations.Where(value => !current.ContainsKey(value.Identity)).Select(value => value.Identity));
        foreach (RawDeclaration row in raw.Declarations)
        {
            if (!old.TryGetValue(row.Identity, out RawDeclaration? prior)) continue;
            result.RetainedIdentityCount++;
            string[] fields = new[] { "arguments", "modifiers", "enumValues", "baseDeclaration", "documentation" }
                .Where(field => Field51(prior, field) != Field51(row, field)).ToArray();
            if (fields.Length > 0) result.Changed.Add(new { identity = row.Identity, historicalOrdinal = prior.Ordinal, currentOrdinal = row.Ordinal, fields,
                before = prior, after = row });
        }
        return result;
    }

    private static string Field51(RawDeclaration row, string field) => field switch
    {
        "arguments" => Serialize(row.Arguments), "modifiers" => Serialize(row.Modifiers), "enumValues" => Serialize(row.EnumValues),
        "baseDeclaration" => row.BaseDeclaration, "documentation" => row.Documentation, _ => throw new ArgumentException(field)
    };

    private static string Build51Map(RawDocument raw, ClassificationDocument classified)
    {
        var result = new StringBuilder("# OpenCV 5.1.0 DNN upstream-to-native-to-managed map\n");
        result.AppendLine("claimed-slice=" + ClaimedSlice);
        result.AppendLine("upstream-commit=" + RefreshCommit);
        result.AppendLine("repository-wide-upstream-parity=false");
        foreach (ClassificationRow row in classified.Declarations)
            result.AppendLine($"{row.Ordinal:D3}|{raw.Declarations[row.Ordinal].Kind}|{row.Identity}|{row.Classification}|{row.BuildCondition}|{Clean(row.Reason)}|native={string.Join(';', row.NativeEntrypoints)}|managed={string.Join(';', row.ManagedMembers)}");
        return Normalize(result.ToString());
    }

    private static void Run51Fixtures(RawDocument raw, ClassificationDocument classified, RawDocument historical, ClassificationDocument previous, string[] native, string[] managed, Options options, RefreshDelta delta)
    {
        int rejected = 0;
        void Reject(string name, Action action)
        {
            bool failed = false;
            try { action(); } catch (InvalidDataException) { failed = true; }
            Require(failed, "5.1 DNN fixture accepted: " + name);
            rejected++;
        }
        T Copy<T>(T value) => JsonSerializer.Deserialize<T>(Serialize(value), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        void Raw(string name, Action<RawDocument> mutate) => Reject(name, () => { var value = Copy(raw); mutate(value); Validate51Raw(value, options); });
        void Classification(string name, Action<ClassificationDocument> mutate) => Reject(name, () => { var value = Copy(classified); mutate(value); Validate51Classification(raw, value, historical, previous, native, managed); });
        Raw("version relabel", value => value.UpstreamOpenCvVersion = "5.0.0");
        Raw("removed declaration", value => value.Declarations.RemoveAt(0));
        Raw("duplicate overload", value => value.Declarations[24].Identity = value.Declarations[23].Identity);
        Raw("wrong source", value => value.Declarations[10].SourceHeader = raw.SourceHeaders[0].Path);
        Raw("header closure", value => value.SourceHeaders.RemoveAt(0));
        Raw("source hash", value => value.HeaderSha256 = new string('0', 64));
        Raw("parser condition", value => value.PreprocessorDefinitions["OPENCV_ABI_COMPATIBILITY"] = 400);
        Raw("signature default", value => value.Declarations.First(row => row.Name == "cv.dnn.blobFromImage").Arguments[^1].Default = "cv::INTER_NEAREST");
        Raw("lost inherited properties", value => value.Declarations.First(row => row.Name == "cv.dnn.LayerInfo").Arguments.Clear());
        Raw("lost interpolation property", value => value.Declarations.First(row => row.Name == "cv.dnn.Image2BlobParams").Arguments.RemoveAt(8));
        Classification("missing row", value => value.Declarations.RemoveAt(0));
        Classification("reordered rows", value => (value.Declarations[0], value.Declarations[1]) = (value.Declarations[1], value.Declarations[0]));
        Classification("metadata as callable", value => value.Declarations[0].Classification = "implemented");
        Classification("interpolation promoted", value => value.Declarations.First(row => row.Identity.StartsWith("cv.dnn.blobFromImage(", StringComparison.Ordinal)).Classification = "implemented");
        Classification("ORT silently claimed", value => value.Declarations.First(row => row.Classification == "implemented").Reason = "All backends certified");
        Classification("false native correlation", value => value.Declarations.First(row => row.NativeEntrypoints.Count > 0).NativeEntrypoints[0] = "jyppx_ocv_dnn_net_empty");
        Classification("false managed correlation", value => value.Declarations.First(row => row.ManagedMembers.Count > 0).ManagedMembers[0] = managed[0]);
        Classification("lost conditional", value => value.Declarations.First(row => row.Classification == "upstream-conditional").BuildCondition = "unconditional-parser-surface");
        Classification("omission without review", value => value.Declarations.First(row => row.Classification == "intentionally-omitted").Reason = "");
        Classification("PBTXT promoted", value => value.Declarations.First(row => row.Identity.StartsWith("cv.dnn.Net.dumpToPbtxt(", StringComparison.Ordinal)).Classification = "implemented");
        Reject("unreviewed callable", () => { var value = Copy(raw.Declarations.First(row => row.Kind == "callable")); value.Identity = "cv.dnn.Unknown()->void"; value.Name = "cv.dnn.Unknown"; Decision51(value, historical.Declarations.ToDictionary(row => row.Identity, Ordinal), previous); });
        Reject("lost delta", () => { var value = Copy(delta); value.Removed.RemoveAt(0); Require(Serialize(value) == Serialize(Delta51(historical, raw)), "5.1 DNN delta drifted."); });
        Require(rejected == 22, "5.1 DNN fixture count drifted.");
    }
}

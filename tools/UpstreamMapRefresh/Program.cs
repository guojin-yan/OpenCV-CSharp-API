using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

internal static class Program
{
    private const string Version = "5.1.0";
    private const string Commit = "e4f78fbcf1a773fbed69e14500e8b734963eb585";
    private const string Prefix = "opencv-source/opencv-5.1.0/";
    private const string ContribPrefix = "opencv-source/opencv_contrib-5.1.0/";
    private const string ContribCommit = "b67a855188a3d683813731010f6d39ac614733a4";
    private const string CatalogPath = "compatibility/opencv-5.1.0/upstream-map-refresh-catalog.json";
    // Updating a reviewed slice requires reviewing its source and decisions together.
    private const string ReviewedCatalogSha256 = "2597e3ed13c8aba5ee0a88be9d9b2ebb8d3c84ad6207d22875f343ed51074e9d";
    private static readonly string[] Classes = { "implemented", "intentionally-omitted", "missing", "non-callable-metadata", "unsupported", "upstream-conditional" };
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };
    private sealed record Options(string Repository, string Workspace, string Output, bool Check);

    private static int Main(string[] args)
    {
        try { Run(Parse(args)); return 0; }
        catch (Exception error) { Console.Error.WriteLine(error.Message); return 1; }
    }

    private static void Run(Options options)
    {
        JsonObject catalog = Read(Path.Combine(options.Repository, CatalogPath));
        Require(FileSha(Path.Combine(options.Repository, CatalogPath)) == ReviewedCatalogSha256, "Reviewed 5.1 catalog hash drifted.");
        Require(Int(catalog, "schemaVersion") == 1 && Str(catalog, "upstreamOpenCvVersion") == Version && Str(catalog, "upstreamCommit") == Commit,
            "Catalog upstream identity drifted.");
        Require(Str(catalog, "upstreamContribCommit") == ContribCommit, "Catalog contrib identity drifted.");
        Require(!Bool(catalog, "repositoryWideUpstreamParityClaimed") && !Bool(catalog, "completeCppParityClaimed"), "Catalog overstates coverage.");
        foreach (string key in new[] { "parser", "versionHeader" }) Source(options, Obj(catalog, key));
        Require(Str(Obj(catalog, "parser"), "sha256") == "867e3de5ad45fd3996e8c9097ffc47f13c8651e20dda2318d70e273262c8dcee" &&
            Str(Obj(catalog, "versionHeader"), "sha256") == "bc436bd455e88946523bd83d441320b44ecc9cdd6e65718cd4fc78f4cd6defb7", "Official parser/version changed.");
        Require(FileSha(Path.Combine(options.Repository, Str(catalog, "historicalCatalogPath"))) == Str(catalog, "historicalCatalogSha256"), "Historical catalog hash drifted.");
        JsonObject historicalCatalog = Read(Path.Combine(options.Repository, Str(catalog, "historicalCatalogPath")));
        Require(Str(historicalCatalog, "upstreamOpenCvVersion") == "5.0.0", "Historical catalog was relabeled.");
        string[] historicalModules = Arr(historicalCatalog, "maps").Select(row => Str(row!, "id")).ToArray();
        var native = Regex.Matches(File.ReadAllText(Path.Combine(options.Repository, "src/OpenCvSharp.Native/generated/native_abi_manifest.txt")), @"\bjyppx_ocv_[A-Za-z0-9_]+\b")
            .Select(match => match.Value).ToHashSet(StringComparer.Ordinal);
        var managed = File.ReadAllLines(Path.Combine(options.Repository, "compatibility/managed-public-api.txt")).Select(line => line.Trim()).ToHashSet(StringComparer.Ordinal);
        var slices = new List<JsonObject>();
        int fixtureTotal = 0;
        foreach (JsonNode? external in Arr(catalog, "externalSlices"))
        {
            string path = Path.Combine(options.Repository, Str(external!, "summaryPath"));
            Require(FileSha(path) == Str(external!, "sha256"), "External reviewed slice drifted.");
            JsonObject summary = Read(path);
            Require(Str(summary, "upstreamOpenCvVersion") == Version && Str(summary, "upstreamCommit") == Commit &&
                !Bool(summary, "repositoryWideUpstreamParityClaimed") && !Bool(summary, "completeDnnCppParityClaimed"), "External slice identity or scope drifted.");
            slices.Add(new JsonObject { ["id"] = Str(external!, "id"), ["summaryPath"] = Str(external!, "summaryPath"), ["sha256"] = FileSha(path),
                ["declarationCount"] = Int(summary, "declarationCount"), ["completeCppParityClaimed"] = false });
        }
        foreach (JsonNode? node in Arr(catalog, "modules"))
        {
            JsonObject module = node!.AsObject();
            string id = Str(module, "id");
            string stem = id + "-upstream-";
            string rawPath = Path.Combine(options.Output, stem + "raw.json");
            Require(FileSha(rawPath) == Str(module, "rawSha256"), "Reviewed raw snapshot hash drifted: " + id);
            JsonObject raw = Read(rawPath);
            JsonObject trusted = Copy(raw);
            foreach (string key in new[] { "historicalRaw", "historicalClassification" })
                Require(FileSha(Path.Combine(options.Repository, Str(module, key + "Path"))) == Str(module, key + "Sha256"), "Historical input hash drifted: " + id);
            JsonObject old = Read(Path.Combine(options.Repository, Str(module, "historicalRawPath")));
            JsonObject previous = Read(Path.Combine(options.Repository, Str(module, "historicalClassificationPath")));
            Require(Str(old, "upstreamOpenCvVersion") == "5.0.0" && Str(previous, "upstreamOpenCvVersion") == "5.0.0", "Historical slice was relabeled.");
            ValidateRaw(raw, module, catalog, trusted);
            Source(options, Obj(module, "primaryHeader"));
            foreach (JsonNode? header in Arr(module, "sourceHeaders")) Source(options, header!.AsObject());
            if (module["compatibilityHeaders"] is JsonArray compatibilityHeaders)
                foreach (JsonNode? header in compatibilityHeaders) Source(options, header!.AsObject());
            if (module["bindingSourceEvidence"] is JsonArray evidence)
                foreach (JsonNode? input in evidence)
                {
                    string path = Str(input!, "path");
                    Require((path.StartsWith("src/", StringComparison.Ordinal) || path.StartsWith("tests/", StringComparison.Ordinal)) &&
                        !path.Contains("..", StringComparison.Ordinal) && !path.Contains('\\'), "Binding source path is outside the repository evidence roots.");
                    Require(Sha(File.ReadAllText(Path.Combine(options.Repository, path)).Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n')) == Str(input!, "sha256"), "Binding source evidence drifted: " + path);
                }
            ValidateReviews(raw, old, module);
            JsonObject classified = Classify(raw, old, previous, module);
            ValidateClassification(raw, classified, old, previous, module, native, managed);
            JsonObject delta = Delta(raw, old, module);
            int fixtures = Fixtures(raw, old, previous, module, catalog, trusted, classified, delta, native, managed);
            fixtureTotal += fixtures;
            string classificationText = Serialize(classified), deltaText = Serialize(delta);
            var map = new StringBuilder($"# OpenCV {Version} {id} upstream-to-native-to-managed map\n");
            map.AppendLine("claimed-slice=" + Str(module, "claimedSlice"));
            map.AppendLine("upstream-commit=" + Commit);
            map.AppendLine("repository-wide-upstream-parity=false");
            foreach (JsonNode? row in Arr(classified, "declarations"))
                map.AppendLine($"{Int(row!, "ordinal"):D3}|{Clean(Str(row!, "identity"))}|{Str(row!, "classification")}|{Clean(Str(row!, "buildCondition"))}|{Clean(Str(row!, "reason"))}|review={Clean(Str(row!, "refreshReview"))}|native={string.Join(';', Strings(row!, "nativeEntrypoints"))}|managed={string.Join(';', Strings(row!, "managedMembers"))}");
            string mapping = map.ToString().Replace("\r\n", "\n", StringComparison.Ordinal);
            var family = new JsonObject { ["schemaVersion"] = 1, ["upstreamOpenCvVersion"] = Version, ["module"] = id,
                ["claimedSlice"] = Str(module, "claimedSlice"), ["completeCppParityClaimed"] = false,
                ["implemented"] = Array(Arr(classified, "declarations").Where(row => Str(row!, "classification") == "implemented")),
                ["limitation"] = "Implemented means the recorded managed/C ABI binding exists, subject to its documented validation. It does not certify wider 5.1 input domains, optional codecs/backends, or complete C++ coverage." };
            string familyText = Serialize(family);
            var counts = new JsonObject();
            foreach (string label in Classes) counts[label] = Arr(classified, "declarations").Count(row => Str(row!, "classification") == label);
            var summary = new JsonObject { ["schemaVersion"] = 1, ["generator"] = "tools/UpstreamMapRefresh", ["upstreamOpenCvVersion"] = Version,
                ["upstreamCommit"] = Commit, ["module"] = id, ["claimedSlice"] = Str(module, "claimedSlice"), ["sourceHashPolicy"] = Str(catalog, "sourceHashPolicy"),
                ["rawExtractionPath"] = Relative(id + "-upstream-raw.json"), ["classificationPath"] = Relative(id + "-upstream-classifications.json"),
                ["mappingPath"] = Relative(id + "-upstream-map.txt"), ["deltaPath"] = Relative(id + "-upstream-delta.json"),
                ["familyInventoryPath"] = Relative(id + "-implemented-families.json"), ["rawSha256"] = FileSha(rawPath),
                ["classificationSha256"] = Sha(classificationText), ["mappingSha256"] = Sha(mapping), ["deltaSha256"] = Sha(deltaText), ["familyInventorySha256"] = Sha(familyText),
                ["historicalRawSha256"] = Str(module, "historicalRawSha256"), ["historicalClassificationSha256"] = Str(module, "historicalClassificationSha256"),
                ["sourceHeaderCount"] = Arr(raw, "sourceHeaders").Count, ["declarationCount"] = Int(raw, "declarationCount"), ["callableCount"] = Int(module, "callableCount"),
                ["classCount"] = Arr(raw, "declarations").Count(row => Str(row!, "kind") == "class"), ["enumCount"] = Arr(raw, "declarations").Count(row => Str(row!, "kind") == "enum"),
                ["classificationCounts"] = counts, ["deltaCounts"] = Obj(module, "expectedDelta").DeepClone(), ["negativeFixtureCount"] = fixtures,
                ["nativeEvidenceCount"] = Arr(classified, "declarations").SelectMany(row => Strings(row!, "nativeEntrypoints")).Distinct(StringComparer.Ordinal).Count(),
                ["managedEvidenceCount"] = Arr(classified, "declarations").SelectMany(row => Strings(row!, "managedMembers")).Distinct(StringComparer.Ordinal).Count(),
                ["repositoryWideUpstreamParityClaimed"] = false, ["completeCppParityClaimed"] = false, ["newRuntimeBehaviorCertified"] = false };
            if (module["bindingSourceEvidence"] is JsonArray)
                summary["reviewedExistingBindingCount"] = Arr(module, "addedReviews").Count(row => Str(row!, "classification") == "implemented");
            if (module["sourceRepository"] is not null)
            {
                summary["sourceRepository"] = module["sourceRepository"]!.DeepClone();
                if (Str(module, "sourceRepository") == "opencv_contrib") summary["upstreamContribCommit"] = ContribCommit;
            }
            string summaryText = Serialize(summary);
            Write(options, stem + "classifications.json", classificationText);
            Write(options, stem + "delta.json", deltaText);
            Write(options, stem + "map.txt", mapping);
            Write(options, id + "-implemented-families.json", familyText);
            Write(options, stem + "summary.json", summaryText);
            slices.Add(new JsonObject { ["id"] = id, ["summaryPath"] = Relative(stem + "summary.json"), ["sha256"] = Sha(summaryText),
                ["declarationCount"] = Int(raw, "declarationCount"), ["completeCppParityClaimed"] = false });
            Console.WriteLine($"UPSTREAM_510_REFRESH_OK module={id} declarations={Int(raw, "declarationCount")} implemented={Int(counts, "implemented")} omitted={Int(counts, "intentionally-omitted")} missing=0 fixtures={fixtures} mode={(options.Check ? "check" : "write")}");
        }
        string[] refreshed = slices.Select(row => Str(row, "id")).ToArray();
        Require(refreshed.Distinct(StringComparer.Ordinal).Count() == refreshed.Length && refreshed.All(historicalModules.Contains), "Refreshed slice membership drifted.");
        var aggregate = new JsonObject { ["schemaVersion"] = 1, ["upstreamOpenCvVersion"] = Version, ["upstreamCommit"] = Commit,
            ["upstreamContribCommit"] = ContribCommit,
            ["catalogPath"] = CatalogPath, ["catalogSha256"] = ReviewedCatalogSha256, ["historicalCatalogRevision"] = "5.0.0", ["historicalCatalogMapCount"] = historicalModules.Length,
            ["refreshedParserSliceCount"] = refreshed.Length, ["pendingCatalogMapCount"] = historicalModules.Length - refreshed.Length,
            ["slices"] = Array(slices.OrderBy(row => Str(row, "id"), StringComparer.Ordinal)),
            ["pendingCatalogMaps"] = Array(historicalModules.Except(refreshed, StringComparer.Ordinal).Order(StringComparer.Ordinal).Select(value => JsonValue.Create(value))),
            ["sharedGeneratorNegativeFixtureCount"] = fixtureTotal, ["repositoryWideUpstreamParityClaimed"] = false, ["completeCppParityClaimed"] = false,
            ["limitation"] = "Only the recorded parser slices have been reviewed. Historical maps, complete C++/template surfaces and runtime/platform certification retain separate evidence and version identities." };
        Write(options, "upstream-map-refresh-summary.json", Serialize(aggregate));
        Console.WriteLine($"UPSTREAM_510_CATALOG_OK slices={refreshed.Length} pending={historicalModules.Length - refreshed.Length} shared_generator_fixtures={fixtureTotal} repository_wide_parity=false");
    }

    private static void ValidateRaw(JsonObject raw, JsonObject module, JsonObject catalog, JsonObject trusted)
    {
        Require(Int(raw, "schemaVersion") == 1 && Str(raw, "upstreamOpenCvVersion") == Version, "Raw version drifted.");
        Require(Str(raw, "generator") == "tools/UpstreamMapRefresh/extract.py" && Str(raw, "module") == Str(module, "id"), "Raw generator/module drifted.");
        Require(JsonNode.DeepEquals(raw["sourceRepository"], module["sourceRepository"]), "Raw source repository drifted.");
        if (module["sourceRepository"] is not null)
        {
            string repository = Str(module, "sourceRepository");
            Require((repository is "opencv" or "opencv_contrib") &&
                Str(Obj(module, "primaryHeader"), "path").StartsWith(repository == "opencv" ? Prefix : ContribPrefix, StringComparison.Ordinal), "Raw source repository root drifted.");
            Require(repository != "opencv_contrib" || Str(raw, "upstreamContribCommit") == ContribCommit, "Raw contrib identity drifted.");
        }
        Require(JsonNode.DeepEquals(raw["compatibilityHeaders"], module["compatibilityHeaders"]), "Raw compatibility headers drifted.");
        Require(Str(raw, "headerPath") == Str(Obj(module, "primaryHeader"), "path") && Str(raw, "headerSha256") == Str(Obj(module, "primaryHeader"), "sha256") &&
            Str(raw, "parserPath") == Str(Obj(catalog, "parser"), "path") && Str(raw, "parserSha256") == Str(Obj(catalog, "parser"), "sha256"), "Raw source binding drifted.");
        Require(JsonNode.DeepEquals(raw["preprocessorDefinitions"], module["preprocessorDefinitions"]), "Raw parser defines drifted.");
        JsonArray headers = Arr(raw, "sourceHeaders"), expectedHeaders = Arr(module, "sourceHeaders"), rows = Arr(raw, "declarations");
        Require(headers.Count == expectedHeaders.Count, "Raw header closure drifted.");
        int next = 0;
        for (int i = 0; i < headers.Count; i++)
        {
            Require(Str(headers[i]!, "path") == Str(expectedHeaders[i]!, "path") && Str(headers[i]!, "sha256") == Str(expectedHeaders[i]!, "sha256"), "Raw header closure drifted.");
            Require(Int(headers[i]!, "startOrdinal") == next && Int(headers[i]!, "declarationCount") >= 0, "Raw header partition drifted.");
            next += Int(headers[i]!, "declarationCount");
        }
        Require(next == rows.Count && rows.Count == Int(module, "declarationCount") && rows.Count == Int(raw, "declarationCount"), "Raw declaration count drifted.");
        Require(rows.Select(row => Str(row!, "identity")).Distinct(StringComparer.Ordinal).Count() == rows.Count, "Raw duplicate identities.");
        for (int i = 0; i < rows.Count; i++)
        {
            JsonNode row = rows[i]!;
            Require(Int(row, "ordinal") == i, "Raw ordinals drifted.");
            JsonNode header = headers.Single(h => i >= Int(h!, "startOrdinal") && i < Int(h!, "startOrdinal") + Int(h!, "declarationCount"))!;
            Require(Str(row, "sourceHeader") == Str(header, "path"), "Raw declaration source scope drifted.");
            Require(JsonNode.DeepEquals(row["surface"], header["surface"]), "Raw declaration surface drifted.");
            Require(Str(row, "identity") == Identity(row, Str(module, "identityPolicy")), "Raw signature identity drifted.");
        }
        Require(rows.Count(row => Str(row!, "kind") == "callable") == Int(module, "callableCount"), "Raw callable partition drifted.");
        Require(JsonNode.DeepEquals(raw, trusted), "Raw reviewed snapshot drifted.");
    }

    private static string Identity(JsonNode row, string policy)
    {
        string kind = Str(row, "kind"), name = Str(row, "name");
        if (kind == "enum" && policy == "full-enum") return $"enum {name}[{string.Join(';', Arr(row, "enumValues").Select(v => Str(v!, "name") + "=" + Str(v!, "value")))}]";
        if (kind != "callable") { Require(kind is "class" or "enum", "Unknown declaration kind."); return kind + " " + name; }
        return name + "(" + string.Join(';', Arr(row, "arguments").Select(a => Str(a!, "type") + " " + Str(a!, "name") +
            (Str(a!, "default").Length > 0 ? "=" + Str(a!, "default") : "") + (Arr(a!, "modifiers").Count > 0 ? "[" + string.Join(',', Strings(a!, "modifiers")) + "]" : ""))) + ")->" + Str(row, "returnType");
    }

    private static string[] ChangedFields(JsonNode before, JsonNode after) => new[] { "kind", "name", "identity", "returnType", "modifiers", "arguments", "enumValues", "baseDeclaration", "documentation" }
        .Where(key => !JsonNode.DeepEquals(before[key], after[key])).ToArray();

    private static void ValidateReviews(JsonObject raw, JsonObject old, JsonObject module)
    {
        var before = Rows(old); var after = Rows(raw);
        var sets = new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["added"] = after.Keys.Except(before.Keys, StringComparer.Ordinal).ToArray(),
            ["removed"] = before.Keys.Except(after.Keys, StringComparer.Ordinal).ToArray(),
            ["changed"] = after.Keys.Intersect(before.Keys, StringComparer.Ordinal).Where(id => ChangedFields(before[id], after[id]).Length > 0).ToArray()
        };
        foreach (var set in sets)
        {
            JsonArray reviews = Arr(module, set.Key + "Reviews");
            Require(reviews.Count == set.Value.Length && reviews.Select(row => Str(row!, "identity")).SequenceEqual(set.Value), "Review identity closure drifted: " + set.Key);
            Require(Int(Obj(module, "expectedDelta"), set.Key) == reviews.Count, "Review delta count drifted.");
            foreach (JsonNode? review in reviews)
            {
                Require(!string.IsNullOrWhiteSpace(Str(review!, "reason")), "Review reason is missing.");
                string id = Str(review!, "identity");
                if (set.Key == "changed") Require(Strings(review!, "fields").SequenceEqual(ChangedFields(before[id], after[id])), "Review changed fields drifted.");
                if (set.Key == "added")
                {
                    string label = Str(review!, "classification");
                    if (Str(after[id], "kind") != "callable") Require(label == "non-callable-metadata", "Unimplemented addition was promoted.");
                    else if (label == "implemented")
                    {
                        string kind = Str(review!, "bindingReviewKind");
                        Require(kind is "argument-rename" or "existing-wrapper", "Existing binding review kind drifted.");
                        Require(Strings(review!, "nativeEntrypoints").Length > 0 && Strings(review!, "managedMembers").Length > 0 &&
                            module["bindingSourceEvidence"] is JsonArray inputs && inputs.Count >= 3, "Existing binding review lacks evidence.");
                        if (kind == "argument-rename")
                        {
                            string priorId = Str(review!, "renamedFromIdentity");
                            Require(before.ContainsKey(priorId) && !after.ContainsKey(priorId), "Argument rename historical identity drifted.");
                            JsonObject prior = Copy(before[priorId].AsObject()), current = Copy(after[id].AsObject());
                            Require(!JsonNode.DeepEquals(prior["arguments"], current["arguments"]), "Argument rename has no renamed parameter.");
                            foreach (JsonNode? arg in Arr(prior, "arguments")) arg!["name"] = "";
                            foreach (JsonNode? arg in Arr(current, "arguments")) arg!["name"] = "";
                            Require(ChangedFields(prior, current).All(field => field is "identity" or "documentation"), "Argument rename changed a non-name signature field.");
                        }
                    }
                    else Require(label == "intentionally-omitted", "Unimplemented addition was promoted.");
                }
            }
        }
        Require(Int(Obj(module, "expectedDelta"), "retained") == after.Keys.Intersect(before.Keys, StringComparer.Ordinal).Count(), "Review retained count drifted.");
    }

    private static JsonObject Classify(JsonObject raw, JsonObject old, JsonObject previous, JsonObject module)
    {
        var historical = Rows(old); JsonArray prior = Arr(previous, "declarations"); var rows = new JsonArray();
        Require(prior.Count == historical.Count, "Historical classification closure drifted.");
        foreach (JsonNode? declaration in Arr(raw, "declarations"))
        {
            string id = Str(declaration!, "identity"); JsonObject row;
            if (historical.TryGetValue(id, out JsonNode? original))
            {
                int ordinal = Int(original, "ordinal");
                Require(Str(prior[ordinal]!, "identity") == id && Int(prior[ordinal]!, "ordinal") == ordinal, "Historical classification identity drifted.");
                row = Copy(prior[ordinal]!.AsObject()); row["historicalOrdinal"] = ordinal;
                row["buildCondition"] ??= "unconditional-parser-surface";
                JsonNode? changed = Arr(module, "changedReviews").FirstOrDefault(item => Str(item!, "identity") == id);
                row["refreshReview"] = changed is null ? "Exact historical identity retained; evidence and managed validation remain the recorded binding scope." : Str(changed, "reason");
                if (changed is not null && Str(declaration!, "kind") != "callable") row["reason"] = Str(changed, "reason");
            }
            else
            {
                JsonNode? review = Arr(module, "addedReviews").SingleOrDefault(item => Str(item!, "identity") == id);
                Require(review is not null, "Unreviewed added declaration: " + id);
                row = new JsonObject { ["identity"] = id, ["classification"] = Str(review!, "classification"), ["reason"] = Str(review!, "reason"),
                    ["buildCondition"] = "unconditional-parser-surface", ["nativeEntrypoints"] = new JsonArray(), ["managedMembers"] = new JsonArray(),
                    ["historicalOrdinal"] = null, ["refreshReview"] = Str(review!, "reason") };
                if (Str(review!, "classification") == "implemented")
                {
                    row["nativeEntrypoints"] = review!["nativeEntrypoints"]!.DeepClone();
                    row["managedMembers"] = review["managedMembers"]!.DeepClone();
                    row["bindingReviewKind"] = Str(review, "bindingReviewKind");
                    if (Str(review, "bindingReviewKind") == "argument-rename")
                    {
                        int ordinal = Int(historical[Str(review, "renamedFromIdentity")], "ordinal");
                        Require(Str(prior[ordinal]!, "classification") == "implemented" &&
                            JsonNode.DeepEquals(prior[ordinal]!["nativeEntrypoints"], review["nativeEntrypoints"]) &&
                            JsonNode.DeepEquals(prior[ordinal]!["managedMembers"], review["managedMembers"]), "Argument rename binding evidence drifted.");
                        row["historicalOrdinal"] = ordinal;
                        row["renamedFromIdentity"] = Str(review, "renamedFromIdentity");
                    }
                }
            }
            row["ordinal"] = Int(declaration!, "ordinal"); rows.Add(row);
        }
        return new JsonObject { ["schemaVersion"] = 1, ["upstreamOpenCvVersion"] = Version, ["module"] = Str(module, "id"),
            ["claimedSlice"] = Str(module, "claimedSlice"), ["reviewStatus"] = "reviewed-parser-slice", ["declarations"] = rows };
    }

    private static void ValidateClassification(JsonObject raw, JsonObject value, JsonObject old, JsonObject previous, JsonObject module, HashSet<string> native, HashSet<string> managed)
    {
        Require(JsonNode.DeepEquals(value, Classify(raw, old, previous, module)), "Classification decisions/evidence drifted.");
        foreach (JsonNode? row in Arr(value, "declarations"))
        {
            string label = Str(row!, "classification");
            Require(Classes.Contains(label) && label != "missing" && !string.IsNullOrWhiteSpace(Str(row!, "reason")), "Classification is unexplained.");
            string[] n = Strings(row!, "nativeEntrypoints"), m = Strings(row!, "managedMembers");
            Require(n.All(native.Contains) && m.All(managed.Contains), "Classification references missing public evidence.");
            Require(n.SequenceEqual(n.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)) && m.SequenceEqual(m.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)), "Classification evidence is nondeterministic.");
            Require(label != "implemented" || n.Length > 0 && m.Length > 0 && Str(Arr(raw, "declarations")[Int(row!, "ordinal")]!, "kind") == "callable", "Implemented declaration has no binding evidence.");
        }
    }

    private static JsonObject Delta(JsonObject raw, JsonObject old, JsonObject module)
    {
        var before = Rows(old); var after = Rows(raw);
        var result = new JsonObject { ["fromVersion"] = "5.0.0", ["toVersion"] = Version, ["module"] = Str(module, "id"),
            ["retainedIdentityCount"] = after.Keys.Intersect(before.Keys, StringComparer.Ordinal).Count() };
        foreach (string key in new[] { "added", "removed", "changed" })
        {
            result[key] = Array(Arr(module, key + "Reviews").Select(review =>
            {
                string id = Str(review!, "identity"); var row = new JsonObject { ["identity"] = id, ["review"] = Str(review!, "reason") };
                if (before.TryGetValue(id, out JsonNode? prior)) row["before"] = prior.DeepClone();
                if (after.TryGetValue(id, out JsonNode? current)) row["after"] = current.DeepClone();
                if (key == "changed") row["fields"] = review!["fields"]!.DeepClone();
                if (review!["bindingReviewKind"] is not null) row["bindingReview"] = review.DeepClone();
                return row;
            }));
        }
        return result;
    }

    private static int Fixtures(JsonObject raw, JsonObject old, JsonObject previous, JsonObject module, JsonObject catalog, JsonObject trusted, JsonObject classified, JsonObject delta, HashSet<string> native, HashSet<string> managed)
    {
        int count = 0;
        void Reject(string name, string issue, Action action)
        {
            bool rejected = false;
            try { action(); } catch (InvalidDataException error) when (error.Message.Contains(issue, StringComparison.Ordinal)) { rejected = true; }
            Require(rejected, "Negative fixture accepted or failed for another reason: " + name); count++;
        }
        void Raw(string name, string issue, Action<JsonObject> mutate) => Reject(name, issue, () => { var copy = Copy(raw); mutate(copy); ValidateRaw(copy, module, catalog, trusted); });
        void Classification(string name, Action<JsonObject> mutate) => Reject(name, "Classification decisions/evidence", () => { var copy = Copy(classified); mutate(copy); ValidateClassification(raw, copy, old, previous, module, native, managed); });
        Raw("version relabel", "Raw version", v => v["upstreamOpenCvVersion"] = "5.0.0");
        Raw("other extractor", "Raw generator", v => v["generator"] = "other");
        Raw("other module", "Raw generator/module", v => v["module"] = "other");
        Raw("header hash", "Raw source binding", v => v["headerSha256"] = new string('0', 64));
        Raw("parser root", "Raw source binding", v => v["parserPath"] = "opencv-source/opencv-5.0.0/modules/python/src2/hdr_parser.py");
        Raw("parser defines", "Raw parser defines", v => Obj(v, "preprocessorDefinitions")["OPENCV_ABI_COMPATIBILITY"] = 400);
        Raw("lost header", "Raw header closure", v => Arr(v, "sourceHeaders").RemoveAt(0));
        Raw("header partition", "Raw header partition", v => Arr(v, "sourceHeaders")[0]!["startOrdinal"] = 1);
        Raw("lost declaration", "Raw declaration count", v => Arr(v, "declarations").RemoveAt(0));
        Raw("ordinal", "Raw ordinals", v => Arr(v, "declarations")[0]!["ordinal"] = 1);
        Raw("collapsed overload", "Raw duplicate identities", v => Arr(v, "declarations")[1]!["identity"] = Str(Arr(v, "declarations")[0]!, "identity"));
        Raw("wrong source", "Raw declaration source scope", v => Arr(v, "declarations")[0]!["sourceHeader"] = "outside.hpp");
        Raw("signature", "Raw signature identity", v => Arr(v, "declarations").First(row => Str(row!, "kind") == "callable")!["returnType"] = "unreviewed");
        Raw("lost documentation", "Raw reviewed snapshot", v => Arr(v, "declarations").First(row => Str(row!, "documentation").Length > 0)!["documentation"] = "");
        if (Arr(raw, "declarations").Any(row => Str(row!, "kind") == "class" && Arr(row!, "arguments").Count > 0))
            Raw("lost class property", "Raw reviewed snapshot", v => Arr(Arr(v, "declarations").First(row => Str(row!, "kind") == "class" && Arr(row!, "arguments").Count > 0)!, "arguments").RemoveAt(0));
        else
            Raw("false declaration qualifier", "Raw reviewed snapshot", v => Arr(Arr(v, "declarations").First(row => Str(row!, "kind") == "callable")!, "modifiers").Add("/unreviewed"));
        Raw("lost enum metadata", Str(module, "identityPolicy") == "full-enum" ? "Raw signature identity" : "Raw reviewed snapshot", v => Arr(Arr(v, "declarations").First(row => Arr(row!, "enumValues").Count > 0)!, "enumValues").RemoveAt(0));
        Classification("missing classification", v => Arr(v, "declarations").RemoveAt(0));
        Classification("wrong order", v => Arr(v, "declarations")[0]!["ordinal"] = 1);
        Classification("false native", v => Arr(Arr(v, "declarations").First(row => Arr(row!, "nativeEntrypoints").Count > 0)!, "nativeEntrypoints")[0] = "jyppx_ocv_core_false");
        Classification("false managed", v => Arr(Arr(v, "declarations").First(row => Arr(row!, "managedMembers").Count > 0)!, "managedMembers")[0] = "false");
        Classification("metadata promoted", v => Arr(v, "declarations").First(row => Str(row!, "classification") == "non-callable-metadata")!["classification"] = "implemented");
        Classification("binding erased", v => Arr(v, "declarations").First(row => Str(row!, "classification") == "implemented")!["classification"] = "intentionally-omitted");
        Classification("missing reason", v => Arr(v, "declarations")[0]!["reason"] = "");
        Classification("lost build condition", v => Arr(v, "declarations")[0]!["buildCondition"] = "unreviewed");
        // Unchanged slices still exercise the review contract with a controlled documentation delta.
        (JsonObject Raw, JsonObject Module) ReviewCase()
        {
            var r = Copy(raw); var m = Copy(module);
            if (Arr(m, "changedReviews").Count == 0)
            {
                var historical = Rows(old);
                JsonNode row = Arr(r, "declarations").First(item => historical.ContainsKey(Str(item!, "identity")))!;
                row["documentation"] = Str(row, "documentation") + "\nreview fixture";
                Arr(m, "changedReviews").Add(new JsonObject { ["identity"] = Str(row, "identity"), ["fields"] = new JsonArray("documentation"), ["reason"] = "Controlled review fixture." });
                Obj(m, "expectedDelta")["changed"] = 1;
            }
            ValidateReviews(r, old, m);
            return (r, m);
        }
        Reject("review omitted", "Review identity closure", () => { var v = ReviewCase(); Arr(v.Module, "changedReviews").RemoveAt(0); ValidateReviews(v.Raw, old, v.Module); });
        Reject("review empty", "Review reason", () => { var v = ReviewCase(); Arr(v.Module, "changedReviews")[0]!["reason"] = ""; ValidateReviews(v.Raw, old, v.Module); });
        Reject("lost enum/property change review", "Review changed fields", () => { var v = ReviewCase(); Arr(v.Module, "changedReviews")[0]!["fields"] = new JsonArray(); ValidateReviews(v.Raw, old, v.Module); });
        Reject("unreviewed removal", "Review identity closure", () => { var v = Copy(module); Arr(v, "removedReviews").Add(new JsonObject { ["identity"] = "unknown", ["reason"] = "unknown" }); ValidateReviews(raw, old, v); });
        Reject("unreviewed new callable", "Unreviewed added declaration", () => { var v = Copy(raw); Arr(v, "declarations").First(row => Str(row!, "kind") == "callable")!["identity"] = "cv.unreviewed()->void"; Classify(v, old, previous, module); });
        Reject("lost delta", "Delta drift", () => { var v = Copy(delta); v["retainedIdentityCount"] = Int(v, "retainedIdentityCount") + 1; Require(JsonNode.DeepEquals(v, Delta(raw, old, module)), "Delta drift."); });
        if (module["sourceRepository"] is not null)
            Raw("false source repository", "Raw source repository", v => v["sourceRepository"] = "other");
        if (module["sourceRepository"] is not null && Str(module, "sourceRepository") == "opencv_contrib")
            Raw("false contrib revision", "Raw contrib identity", v => v["upstreamContribCommit"] = new string('0', 40));
        if (module["compatibilityHeaders"] is JsonArray { Count: > 0 })
            Raw("lost compatibility header", "Raw compatibility headers", v => Arr(v, "compatibilityHeaders").RemoveAt(0));
        if (Arr(module, "sourceHeaders").Any(header => header!["surface"] is not null))
            Raw("false declaration surface", "Raw declaration surface", v => Arr(v, "declarations")[0]!["surface"] = "unreviewed");
        if (Arr(module, "addedReviews").Any(row => Str(row!, "classification") == "implemented"))
        {
            JsonNode Binding(JsonObject v) => Arr(v, "addedReviews").First(row => Str(row!, "classification") == "implemented")!;
            Reject("false binding review kind", "Existing binding review kind", () => { var v = Copy(module); Binding(v)["bindingReviewKind"] = "unreviewed"; ValidateReviews(raw, old, v); });
            Reject("empty binding review", "Existing binding review lacks evidence", () => { var v = Copy(module); Binding(v)["nativeEntrypoints"] = new JsonArray(); ValidateReviews(raw, old, v); });
            Reject("false new native evidence", "Classification references missing public evidence", () => { var v = Copy(module); Binding(v)["bindingReviewKind"] = "existing-wrapper"; Binding(v)["nativeEntrypoints"] = new JsonArray("jyppx_ocv_false"); var result = Classify(raw, old, previous, v); ValidateClassification(raw, result, old, previous, v, native, managed); });
            Reject("false new managed evidence", "Classification references missing public evidence", () => { var v = Copy(module); Binding(v)["bindingReviewKind"] = "existing-wrapper"; Binding(v)["managedMembers"] = new JsonArray("false"); var result = Classify(raw, old, previous, v); ValidateClassification(raw, result, old, previous, v, native, managed); });
            Reject("false rename source", "Argument rename historical identity", () => { var v = Copy(module); Binding(v)["bindingReviewKind"] = "argument-rename"; Binding(v)["renamedFromIdentity"] = "unknown"; ValidateReviews(raw, old, v); });
            Reject("rename signature mutation", "Argument rename changed a non-name signature field", () => { var v = Copy(raw); Arr(v, "declarations").Single(row => Str(row!, "identity") == Str(Binding(module), "identity"))!["returnType"] = "unreviewed"; ValidateReviews(v, old, module); });
            Reject("rename evidence mutation", "Argument rename binding evidence", () => { var v = Copy(module); Binding(v)["nativeEntrypoints"] = new JsonArray("jyppx_ocv_false"); Classify(raw, old, previous, v); });
        }
        return count;
    }

    private static Options Parse(string[] args)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal); bool check = false;
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--check") { check = true; continue; }
            Require(args[i] is "--repository" or "--workspace" or "--output-directory" && i + 1 < args.Length, "Invalid refresh option.");
            string key = args[i]; Require(values.TryAdd(key, args[++i]), "Duplicate refresh option.");
        }
        string repo = Path.GetFullPath(values.GetValueOrDefault("--repository", Directory.GetCurrentDirectory()));
        return new(repo, Path.GetFullPath(values.GetValueOrDefault("--workspace", Path.Combine(repo, ".."))),
            Path.GetFullPath(values.GetValueOrDefault("--output-directory", Path.Combine(repo, "compatibility/opencv-5.1.0"))), check);
    }
    private static void Source(Options options, JsonObject input)
    {
        string path = Str(input, "path"); Require((path.StartsWith(Prefix, StringComparison.Ordinal) || path.StartsWith(ContribPrefix, StringComparison.Ordinal)) &&
            !path.Contains("..", StringComparison.Ordinal) && !path.Contains('\\'), "Source path is outside the pinned root.");
        string text = new UTF8Encoding(false, true).GetString(File.ReadAllBytes(Path.Combine(options.Workspace, path)));
        Require(Sha(text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n')) == Str(input, "sha256"), "Official source hash drifted: " + path);
    }
    private static void Write(Options options, string name, string text)
    {
        string path = Path.Combine(options.Output, name);
        byte[] expected = new UTF8Encoding(false).GetBytes(text);
        if (options.Check) Require(File.Exists(path) && File.ReadAllBytes(path).SequenceEqual(expected), "Generated artifact is stale or noncanonical: " + name);
        else { Directory.CreateDirectory(options.Output); File.WriteAllBytes(path, expected); }
    }
    private static Dictionary<string, JsonNode> Rows(JsonObject raw) => Arr(raw, "declarations").ToDictionary(row => Str(row!, "identity"), row => row!, StringComparer.Ordinal);
    private static JsonObject Read(string path) => JsonNode.Parse(File.ReadAllText(path))!.AsObject();
    private static JsonObject Copy(JsonObject value) => value.DeepClone().AsObject();
    private static JsonArray Arr(JsonNode value, string key) => value[key]!.AsArray();
    private static JsonObject Obj(JsonNode value, string key) => value[key]!.AsObject();
    private static string Str(JsonNode value, string key) => value[key]?.GetValue<string>() ?? throw new InvalidDataException("Missing string field: " + key);
    private static int Int(JsonNode value, string key) => value[key]!.GetValue<int>();
    private static bool Bool(JsonNode value, string key) => value[key]!.GetValue<bool>();
    private static string[] Strings(JsonNode value, string key) => Arr(value, key).Select(node => node!.GetValue<string>()).ToArray();
    private static JsonArray Array(IEnumerable<JsonNode?> nodes) => new(nodes.Select(node => node?.DeepClone()).ToArray());
    private static string Serialize(JsonNode value) => value.ToJsonString(Json).Replace("\r\n", "\n", StringComparison.Ordinal) + "\n";
    private static string FileSha(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
    private static string Sha(string text) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
    private static string Relative(string name) => "compatibility/opencv-5.1.0/" + name;
    private static string Clean(string value) => value.Replace('|', '/').Replace('\r', ' ').Replace('\n', ' ');
    private static void Require(bool condition, string issue) { if (!condition) throw new InvalidDataException(issue); }
}

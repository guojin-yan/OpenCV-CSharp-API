using System.Text;
using System.Text.Json.Nodes;

internal static partial class Program
{
    private static void ValidateStrategy(JsonObject raw)
    {
        if (raw["parserEvidence"] is not JsonObject evidence) return;
        JsonArray rows = Arr(raw, "declarations");
        string id = Str(raw, "module");
        switch (id)
        {
            case "cannops":
                Require(Arr(evidence, "qualifiedImplementationClasses").Count == 2 &&
                    Arr(evidence, "inlineDefinitionIdentities").Count == 16 &&
                    Arr(evidence, "excludedPrivateInlineDefinitions").Count == 2 &&
                    Int(evidence, "bindingOnlyPlaceholderBlockCount") == 7, "CANN inline/implementation evidence drifted.");
                Require(Strings(evidence, "inlineDefinitionIdentities").All(identity => rows.Any(row => Str(row!, "identity") == identity)) &&
                    Strings(evidence, "excludedPrivateInlineDefinitions").All(identity => rows.All(row => Str(row!, "identity") != identity)), "CANN inline reconciliation drifted.");
                break;
            case "fastcv":
                Require(rows.Count(row => Bool(row!, "upstreamWrapperAnnotated")) == Int(evidence, "upstreamWrapperAnnotatedDeclarationCount") &&
                    rows.Count(row => Str(row!, "kind") == "callable" && Bool(row!, "upstreamWrapperAnnotated")) == Int(evidence, "upstreamWrapperAnnotatedCallableCount") &&
                    Arr(evidence, "umbrellaIncludes").Count == Arr(raw, "sourceHeaders").Count &&
                    Strings(evidence, "umbrellaIncludes").Order(StringComparer.Ordinal).SequenceEqual(Arr(raw, "sourceHeaders").Select(row => Str(row!, "path")).Order(StringComparer.Ordinal)), "FastCV wrapper annotation/closure drifted.");
                break;
            case "sfm":
                Require(rows.Count(row => Bool(row!, "requiresCeres")) == Int(evidence, "ceresDeclarationCount") &&
                    rows.Count(row => !Bool(row!, "requiresCeres")) == Int(evidence, "baseDeclarationCount") &&
                    rows.Count(row => Str(row!, "kind") == "callable" && Bool(row!, "requiresCeres")) == Int(evidence, "ceresCallableCount") &&
                    Arr(evidence, "ceresHeaderGuardProbes").Select(row => Int(row!, "reconstructHeaderDeclarationCount")).SequenceEqual(new[] { 0, 4, 4 }) &&
                    Int(evidence, "affineReconstructionNotImplementedBranchCount") == 4, "SFM Ceres/placeholder evidence drifted.");
                break;
            case "viz":
                Require(Strings(evidence, "parserDuplicateIdentities").SequenceEqual(new[] { "cv.viz.Color.Color()->", "cv.viz.Color.Color(const Scalar& color)->" }) &&
                    Arr(evidence, "parserHeaderPartitionsBeforeExactDeduplication").Sum(row => Int(row!, "declarationCount")) == rows.Count + 2, "Viz inline duplicate evidence drifted.");
                break;
            case "cudev":
                Require(!Bool(evidence, "completeCallableCoverageClaimed") && !Bool(evidence, "cudaExecutionVerified") &&
                    Arr(evidence, "staticParserNameRepairs").Count == 13 &&
                    Arr(evidence, "parserConfigurationProbes").Select(row => Int(Obj(row!, "preprocessorDefinitions"), "CV_CUDEV_ARCH")).SequenceEqual(new[] { 0, 120, 200, 300, 750 }), "Cudev parser strategy/scope drifted.");
                break;
            case "matlab":
                Require(!Bool(evidence, "completeCallableCoverageClaimed") && !Bool(evidence, "matlabExecutionVerified") &&
                    !Bool(evidence, "generatedBindingSurfaceIncluded") && Arr(evidence, "parserNameRepairs").Count == 14 &&
                    Arr(evidence, "reconciledInlineDefinitions").Count == 2 &&
                    Arr(evidence, "parserConfigurationProbes").Select(row => Int(row!, "normalizedMxarrayDeclarationCount")).SequenceEqual(new[] { 35, 36, 36 }) &&
                    Int(Obj(evidence, "upstreamPlaceholderBoundaries"), "noOpAssignmentDefinitionCount") == 48, "MATLAB parser strategy/scope drifted.");
                break;
            default: Require(false, "Unknown specialized parser strategy."); break;
        }
    }

    private static int ValidateSourceInventories(Options options, JsonObject raw, JsonObject module)
    {
        ValidateStrategy(raw);
        int fixtures = 0;
        void Reject(string issue, Action action)
        {
            bool rejected = false;
            try { action(); } catch (InvalidDataException error) when (error.Message.Contains(issue, StringComparison.Ordinal)) { rejected = true; }
            Require(rejected, "Source inventory negative fixture accepted or failed for another reason: " + issue); fixtures++;
        }
        if (raw["parserEvidence"] is JsonObject)
        {
            string id = Str(raw, "module");
            string key = id switch { "cannops" => "bindingOnlyPlaceholderBlockCount", "fastcv" => "upstreamWrapperAnnotatedCallableCount", "sfm" => "affineReconstructionNotImplementedBranchCount", "viz" => "parserDuplicateIdentities", "cudev" => "cudaExecutionVerified", _ => "matlabExecutionVerified" };
            string issue = id switch { "cannops" => "CANN inline/implementation", "fastcv" => "FastCV wrapper annotation/closure", "sfm" => "SFM Ceres/placeholder", "viz" => "Viz inline duplicate", "cudev" => "Cudev parser strategy/scope", _ => "MATLAB parser strategy/scope" };
            Reject(issue, () =>
            {
                var v = Copy(raw); JsonObject e = Obj(v, "parserEvidence");
                if (e[key] is JsonArray a) a.Clear();
                else if (id is "cudev" or "matlab") e[key] = true;
                else e[key] = -1;
                ValidateStrategy(v);
            });
        }
        if (module["sourceInventories"] is not JsonArray specs) return fixtures;
        Require(specs.Count == 2 && Str(raw, "module") is "cudev" or "matlab", "Unexpected source inventory registration.");
        var documents = new Dictionary<string, JsonObject>(StringComparer.Ordinal);
        foreach (JsonNode? spec in specs)
        {
            string kind = Str(spec!, "kind"), path = Str(spec!, "path");
            Require(kind is "lexical" or "callables" && path == Relative(Str(raw, "module") + (kind == "lexical" ? "-source-reviewed-extensions.json" : "-source-callables.json")), "Source inventory path drifted.");
            string local = Path.Combine(options.Output, Path.GetFileName(path));
            byte[] bytes = File.ReadAllBytes(local);
            Require(FileSha(local) == Str(spec!, "sha256"), "Reviewed source inventory hash drifted: " + path);
            Require(bytes.Length > 0 && bytes[^1] == 10 && !bytes.Contains((byte)13) && !(bytes.Length >= 3 && bytes[0] == 239 && bytes[1] == 187 && bytes[2] == 191), "Source inventory text is noncanonical.");
            JsonObject value = Read(local);
            Require(JsonNode.DeepEquals(value["counts"], spec!["counts"]), "Reviewed source inventory counts drifted.");
            Require(documents.TryAdd(kind, value), "Duplicate source inventory registration.");
        }
        JsonObject lexical = documents["lexical"], callables = documents["callables"];
        void Validate(JsonObject lex, JsonObject syntax) => ValidateInventoryDocuments(options, raw, lex, syntax);
        Validate(lexical, callables);
        void Lex(string issue, Action<JsonObject> mutate) => Reject(issue, () => { var v = Copy(lexical); mutate(v); Validate(v, callables); });
        void Syntax(string issue, Action<JsonObject> mutate) => Reject(issue, () => { var v = Copy(callables); mutate(v); Validate(lexical, v); });
        Lex("Source inventory revision/scope", v => v["upstreamOpenCvVersion"] = "5.0.0");
        Lex("Source inventory revision/scope", v => v["completeCallableCoverageClaimed"] = true);
        Lex("Source inventory header closure", v => Arr(v, "headers").RemoveAt(0));
        Lex("Source lexical count", v => Obj(v, "counts")["templatesCount"] = -1);
        Lex("Source lexical span/hash", v => Arr(Arr(v, "headers").First(row => Arr(row!, "templates").Count > 0)!, "templates")[0]!["sourceSha256"] = new string('0', 64));
        Syntax("Source inventory revision/scope", v => v["concreteTemplateInstantiationsClaimed"] = true);
        Syntax("Source syntax parser", v => Obj(v, "syntaxParser")["cppGrammarVersion"] = "other");
        Syntax("Source callable ordinal/head", v => Arr(Arr(v, "headers")[0]!, "callables")[0]!["ordinal"] = -1);
        Syntax("Source callable count", v => Obj(v, "counts")["callableDeclarationCount"] = -1);
        Syntax("Source parser reconciliation", v => Arr(v, "parserCallableReconciliation").RemoveAt(0));
        Syntax("Source parser reconciliation", v => Arr(v, "parserCallableReconciliation")[0]!["identity"] = "cv.unreviewed()->void");
        Syntax("Source callable count", v => Arr(Arr(v, "headers")[0]!, "callables")[0]!["kind"] = "invalid");
        Console.WriteLine($"UPSTREAM_510_SOURCE_INVENTORY_OK module={Str(raw, "module")} headers={Arr(lexical, "headers").Count} source_callables={Int(Obj(callables, "counts"), "callableDeclarationCount")} fixtures={fixtures}");
        return fixtures;
    }

    private static void ValidateInventoryDocuments(Options options, JsonObject raw, JsonObject lexical, JsonObject syntax)
    {
        bool cudev = Str(raw, "module") == "cudev";
        foreach (JsonObject document in new[] { lexical, syntax })
        {
            Require(Int(document, "schemaVersion") == 1 && Str(document, "upstreamOpenCvVersion") == Version &&
                !string.IsNullOrWhiteSpace(Str(document, "limitation")), "Source inventory revision/scope drifted.");
            foreach (string flag in new[] { "completeCallableCoverageClaimed", "macroExpansionClaimed", "templateInstantiationCoverageClaimed", "concreteTemplateInstantiationsClaimed", "externalDeclarationClosureClaimed", "cudaExecutionVerified", "matlabExecutionVerified", "generatedBindingSurfaceIncluded" })
                if (document[flag] is not null) Require(!Bool(document, flag), "Source inventory revision/scope drifted.");
            Require(Arr(document, "headers").Count == Arr(raw, "sourceHeaders").Count, "Source inventory header closure drifted.");
            for (int i = 0; i < Arr(document, "headers").Count; i++)
            {
                JsonNode header = Arr(document, "headers")[i]!, source = Arr(raw, "sourceHeaders")[i]!;
                Require(Str(header, "path") == Str(source, "path") && Str(header, "sha256") == Str(source, "sha256"), "Source inventory header closure drifted.");
            }
        }
        Require(Str(Obj(syntax, "syntaxParser"), "treeSitterVersion") == "0.25.2" && Str(Obj(syntax, "syntaxParser"), "cppGrammarVersion") == "0.23.4", "Source syntax parser drifted.");
        foreach (string field in new[] { "templates", "templateParameters", "macroDefinitions", "conditionalDirectives" }.Concat(cudev ? new[] { "macroInvocations" } : System.Array.Empty<string>()))
            Require(Int(Obj(lexical, "counts"), field + "Count") == Arr(lexical, "headers").Sum(row => Arr(row!, field).Count), "Source lexical count drifted.");
        foreach (JsonNode? header in Arr(lexical, "headers"))
        {
            string text = File.ReadAllText(Path.Combine(options.Workspace, Str(header!, "path"))).Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
            string[] points = text.EnumerateRunes().Select(rune => rune.ToString()).ToArray();
            foreach (string field in new[] { "templates", "templateParameters", "macroDefinitions" }.Concat(cudev ? new[] { "macroInvocations" } : System.Array.Empty<string>()))
                foreach (JsonNode? unit in Arr(header!, field))
                {
                    int start = Int(unit!, "startOffset"), end = Int(unit!, "endOffset");
                    Require(start >= 0 && end > start && end <= points.Length && Sha(string.Concat(points[start..end])) == Str(unit!, "sourceSha256"), "Source lexical span/hash drifted.");
                }
        }
        var sourceRows = Arr(syntax, "headers").SelectMany(header => Arr(header!, "callables").Select(row => (Header: header!, Row: row!))).ToArray();
        for (int i = 0; i < sourceRows.Length; i++)
            // Head hashes bind original whitespace, while head is compacted.
            // The independent PowerShell source guards reconstruct both direct
            // and macro-expanded source and validate the full head witnesses.
            Require(Int(sourceRows[i].Row, "ordinal") == i, "Source callable ordinal/head drifted.");
        JsonObject counts = Obj(syntax, "counts");
        var actual = new Dictionary<string, int> {
            ["callableDeclarationCount"] = sourceRows.Length,
            ["directCallableCount"] = sourceRows.Count(row => Str(Obj(row.Row, "origin"), "kind") == "direct"),
            ["templateContextCallableCount"] = sourceRows.Count(row => Arr(row.Row, "templateContext").Count > 0),
            ["definitionCount"] = sourceRows.Count(row => Str(row.Row, "kind") == "definition"),
            ["declarationOnlyCount"] = sourceRows.Count(row => Str(row.Row, "kind") == "declaration"),
            ["declarationErrorCount"] = Arr(syntax, "headers").Sum(header => Int(header!, "declarationErrorCount")),
            ["bodyParseDiagnosticCount"] = Arr(syntax, "headers").Sum(header => Arr(header!, "bodyParseDiagnostics").Count)
        };
        if (cudev)
        {
            actual["macroGeneratedCallableCount"] = sourceRows.Count(row => Str(Obj(row.Row, "origin"), "kind") == "macro-expansion");
            actual["expandedMacroInvocationCount"] = Arr(syntax, "headers").Sum(header => Arr(header!, "macroExpansions").Count);
            actual["nonCallableExpansionCount"] = Arr(syntax, "headers").Sum(header => Arr(header!, "macroExpansions").Count(row => Int(row!, "callableCount") == 0));
        }
        else
        {
            actual["friendFunctionCount"] = sourceRows.Count(row => Bool(row.Row, "friendDeclared"));
            actual["constructorInitializerCount"] = sourceRows.Count(row => row.Row["constructorInitializer"] is not null);
            actual["conditionalScopeBoundaryCount"] = Arr(syntax, "headers").Sum(header => Arr(header!, "conditionalScopeBoundaries").Count);
        }
        foreach (var count in actual) Require(Int(counts, count.Key) == count.Value, "Source callable count drifted: " + count.Key);
        Require(actual["declarationErrorCount"] == 0, "Source callable declaration syntax errors.");
        JsonNode[] parserRows = Arr(raw, "declarations").Where(row => Str(row!, "kind") == "callable").Select(row => row!).ToArray();
        JsonArray reconciled = Arr(syntax, "parserCallableReconciliation");
        Require(reconciled.Count == parserRows.Length && Int(counts, "reconciledParserCallableCount") == parserRows.Length, "Source parser reconciliation drifted.");
        for (int i = 0; i < parserRows.Length; i++)
        {
            JsonNode row = reconciled[i]!, parser = parserRows[i];
            Require(Int(row, "parserOrdinal") == Int(parser, "ordinal") && Str(row, "identity") == Str(parser, "identity") &&
                Arr(row, "sourceOrdinals").Count > 0 && Arr(row, "sourceOrdinals").All(ordinal => ordinal!.GetValue<int>() >= 0 && ordinal.GetValue<int>() < sourceRows.Length &&
                    Str(sourceRows[ordinal.GetValue<int>()].Header, "path") == Str(parser, "sourceHeader")), "Source parser reconciliation drifted.");
        }
        JsonObject evidence = Obj(raw, "parserEvidence");
        Require(JsonNode.DeepEquals(evidence["sourceInventoryCounts"], lexical["counts"]) &&
            JsonNode.DeepEquals(Obj(evidence, "sourceCallableInventory")["counts"], syntax["counts"]), "Source inventory linked counts drifted.");
    }
}

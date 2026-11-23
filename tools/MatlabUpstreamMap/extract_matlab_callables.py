#!/usr/bin/env python3
"""Retain all-branch, uninstantiated MATLAB helper function occurrences.

Reuse the pinned C++ CST walker, then retain MATLAB-specific friend scope,
storage qualifiers, constructor initializers and definition-body witnesses.
No preprocessor, external includes, C++ compiler or MATLAB runtime is used.
"""

import argparse
import importlib.metadata
import importlib.util
import json
import re
import sys
from pathlib import Path

sys.path[:0] = [str(Path(__file__).parents[1] / "CudevUpstreamMap"),
                str(Path(__file__).parents[1] / "XImgProcUpstreamMap")]
from extract_source_callables import inventory_callables
from source_inventory import DIRECTIVE, blank, compact, digest, inventory, mask
from extract_ximgproc import convert, sha256
from extract_matlab import write_json

LIMITATION = (
    "Counts are uninstantiated source function declaration occurrences, including repeated declarations, "
    "private helpers, class-template members, specializations and inactive branches. Preprocessor directive "
    "lines are blanked without changing byte positions; every source branch is retained, not evaluated. "
    "Lexical class placement is separate from namespace scope for friend functions. External includes/macros, "
    "imported using/alias targets, concrete template instantiations, generated +cv gateways, C++ type checking, "
    "linking and MATLAB/MEX execution remain outside this syntax inventory. Body parser diagnostics are "
    "retained rather than interpreted as compiler results. No package wrapper or complete callable parity is claimed."
)


def extract_header(parser, source, lexical):
    raw = source.encode("utf-8")
    normalized = list(source)
    directives = list(DIRECTIVE.finditer(mask(source, literals=False)))
    for directive in directives:
        normalized[directive.start():directive.end()] = blank(source[directive.start():directive.end()])
    normalized = "".join(normalized).encode("utf-8")
    if len(raw) != len(normalized):
        raise ValueError("MATLAB directive normalization moved UTF-8 coordinates")
    segments = [{"start": 0, "end": len(raw), "sourceStart": 0, "expansion": None}]
    rows, diagnostics, contexts, counts = inventory_callables(parser, raw, normalized, segments, [], source, lexical)
    definitions = {}

    def walk(node):
        if node.type == "function_definition":
            declarator = node.child_by_field_name("declarator")
            definitions[(declarator.start_byte, declarator.end_byte)] = node
            return
        for child in node.named_children:
            walk(child)

    walk(parser.parse(normalized).root_node)
    for row in rows:
        definition = definitions.get((row["declaratorStartByte"], row["declaratorEndByte"]))
        row["qualifiedName"] = re.sub(r"(^|::)operator\s*", r"\1operator ", row["qualifiedName"])
        row["lexicalQualifiedName"] = row["qualifiedName"]
        row["friendDeclared"] = definition is not None and definition.parent.type == "friend_declaration"
        if row["friendDeclared"]:
            start = definition.parent.start_byte
            row["expandedStartByte"] = start
            row["origin"]["startOffset"] = len(raw[:start].decode("utf-8"))
            namespaces = [contexts[i]["value"] for i in row["contextIndices"]
                          if contexts[i]["kind"] == "namespace_definition"]
            name = raw[row["declaratorStartByte"]:row["parametersStartByte"]].decode("utf-8").strip()
            row["qualifiedName"] = "::".join([*namespaces, name])
        start, end = row["expandedStartByte"], row["expandedEndByte"]
        head = raw[start:end].decode("utf-8")
        row["head"], row["headSha256"] = compact(head), digest(head)
        row["origin"]["line"] = source[:row["origin"]["startOffset"]].count("\n") + 1
        prefix = raw[start:row["declaratorStartByte"]].decode("utf-8")
        row["storageSpecifiers"] = re.findall(r"\b(?:friend|static|inline|virtual|explicit)\b", prefix)
        row["methodConst"] = bool(re.search(r"\)\s*const\s*$", row["declarator"]))
        del row["cudaAttributes"]
        row["definitionBody"] = None
        row["constructorInitializer"] = None
        if definition is not None:
            body = definition.child_by_field_name("body")
            if body is None:
                raise ValueError("MATLAB definition without a body requires explicit support")
            row["definitionBody"] = {"startByte": body.start_byte, "endByte": body.end_byte,
                                     "sha256": digest(raw[body.start_byte:body.end_byte].decode("utf-8"))}
            tail = raw[end:body.start_byte].decode("utf-8")
            if tail.strip():
                if not tail.lstrip().startswith(":"):
                    raise ValueError("Unexpected MATLAB declaration tail")
                row["constructorInitializer"] = {"startByte": end, "endByte": body.start_byte,
                                                 "text": compact(tail), "sha256": digest(tail)}
    _, _, span = inventory(source)
    conditional_scopes = []
    for index, context in enumerate(contexts):
        if context["kind"] not in {"namespace_definition", "class_specifier", "struct_specifier"}:
            continue
        opening = len(raw[:context["startByte"]].decode("utf-8"))
        closing = len(raw[:context["endByte"] - 1].decode("utf-8"))
        opening_conditions = span(opening, opening + 1)["conditions"]
        closing_conditions = span(closing, closing + 1)["conditions"]
        if opening_conditions != closing_conditions:
            conditional_scopes.append({"contextIndex": index, "openingConditions": opening_conditions,
                                       "closingConditions": closing_conditions})
    return {"path": lexical["path"], "sha256": lexical.get("sha256", digest(source)),
            "normalizedInputSha256": digest(normalized.decode("utf-8")),
            "normalization": "blank-preprocessor-directives-retain-all-branches", "directiveCount": len(directives),
            "contexts": contexts, "conditionalScopeBoundaries": conditional_scopes,
            "syntaxNodeCounts": counts, "callables": rows,
            "declarationErrorCount": 0, "bodyParseDiagnostics": diagnostics}


def self_test(parser):
    source = '''// Unicode source coordinates: 中文
namespace example {
#if CPP11
template<class T> using Map = Alias<T>;
#else
template<class T> class Map { public: T& at(int i) const { return values[i]; } };
#endif
class Owner {
  friend void swap(Owner& a, Owner& b) { a.value = b.value; }
  int value;
public:
  explicit Owner(int x) : value(x) {}
  operator const int*() const { return &value; }
  static void variadic(int first, ...);
};
template<> int specialized<int>(int x) { return x; }
using Callback = void (*)(int);
}'''
    lexical, _, _ = inventory(source)
    lexical["path"] = "fixture.hpp"
    result = extract_header(parser, source, lexical)
    rows = result["callables"]
    assert len(rows) == 6 and not result["bodyParseDiagnostics"]
    assert rows[0]["templateContext"] == ["<class T>"] and rows[0]["methodConst"]
    assert rows[0]["conditions"] == ["!((CPP11))"]
    assert rows[1]["qualifiedName"] == "example::swap" and rows[1]["friendDeclared"]
    assert rows[1]["lexicalQualifiedName"] == "example::Owner::swap" and rows[1]["access"] == "private"
    assert rows[1]["head"].startswith("friend void swap") and rows[1]["storageSpecifiers"] == ["friend"]
    assert rows[2]["storageSpecifiers"] == ["explicit"] and rows[2]["constructorInitializer"]["text"] == ": value(x)"
    assert rows[3]["qualifiedName"] == "example::Owner::operator const int*" and rows[3]["methodConst"]
    assert rows[4]["variadic"] and rows[4]["parameterCount"] == 2 and rows[4]["definitionBody"] is None
    assert rows[5]["templateContext"] == ["<>"] and rows[5]["qualifiedName"] == "example::specialized<int>"
    assert result["syntaxNodeCounts"]["excludedTypeDeclaratorCount"] > 0
    assert not result["conditionalScopeBoundaries"]
    for row in rows:
        origin = row["origin"]
        assert digest(source[origin["startOffset"]:origin["endOffset"]]) == row["headSha256"]
        assert len(source[:origin["startOffset"]].encode()) == row["expandedStartByte"]
    conditional = 'namespace fallback {\n#if CPP11\nusing Value = int;\n#else\nvoid f();\n}\n#endif\n'
    lexical, _, _ = inventory(conditional)
    lexical["path"] = "fixture.hpp"
    boundaries = extract_header(parser, conditional, lexical)["conditionalScopeBoundaries"]
    assert len(boundaries) == 1 and not boundaries[0]["openingConditions"]
    assert boundaries[0]["closingConditions"] == ["!((CPP11))"]
    for malformed in ("int broken( {", "template<class T> int bad(;"):
        lexical, _, _ = inventory("int placeholder();")
        lexical["path"] = "fixture.hpp"
        try:
            extract_header(parser, malformed, lexical)
        except ValueError:
            pass
        else:
            raise AssertionError("Malformed MATLAB declaration was accepted")
    print("MATLAB_SOURCE_CALLABLE_SELF_TEST_OK")


def extract(workspace, output, syntax_root, upstream_version="5.0.0"):
    sys.path.insert(0, str(syntax_root))
    from tree_sitter import Language, Parser
    import tree_sitter
    import tree_sitter_cpp
    for module in (tree_sitter, tree_sitter_cpp):
        if not Path(module.__file__).resolve().is_relative_to(syntax_root):
            raise RuntimeError("Syntax parser must load from its explicit isolated target")
    for name, expected in (("tree-sitter", "0.25.2"), ("tree-sitter-cpp", "0.23.4")):
        if importlib.metadata.version(name) != expected:
            raise RuntimeError("Unpinned syntax parser dependency: " + name)
    language = Language(tree_sitter_cpp.language())
    parser = Parser(language)
    self_test(parser)
    raw_path = output.parent / "matlab-upstream-raw.json"
    lexical_path = output.parent / "matlab-source-reviewed-extensions.json"
    raw_map, lexical = (json.loads(path.read_text(encoding="utf-8")) for path in (raw_path, lexical_path))
    headers, ordinal = [], 0
    for header in lexical["headers"]:
        path = workspace / header["path"]
        if sha256(path) != header["sha256"] or header["macroDefinitions"]:
            raise RuntimeError("MATLAB source hash/local macro scope drifted")
        result = extract_header(parser, path.read_text(encoding="utf-8"), header)
        for row in result["callables"]:
            row["ordinal"] = ordinal
            ordinal += 1
        headers.append(result)
    spec = importlib.util.spec_from_file_location("matlab_callable_hdr_parser", workspace / raw_map["parserPath"])
    hdr = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(hdr)
    candidates = {}
    for header in headers:
        for row in header["callables"]:
            if row["templateContext"] or row["friendDeclared"]:
                continue
            head = re.sub(r"\binline\b", "", row["head"]).strip().rstrip(";")
            value = hdr.CppHeaderParser().parse_func_decl_no_wrap(head)
            name = re.sub(r"operator\s+([=\[\]()+*/!<>&|^-])", r"operator\1", row["qualifiedName"])
            if "::operator " in name:
                owner, target = name.rsplit("::operator ", 1)
                value[0] = owner.replace("::", ".") + ".operator " + target
            else:
                value[0] = name.replace("::", ".")
            identity = convert(value, 0, header["path"])["identity"]
            candidates.setdefault((header["path"], identity), []).append(row["ordinal"])
    reconciled = []
    for row in raw_map["declarations"]:
        if row["kind"] != "callable":
            continue
        matches = candidates.get((row["sourceHeader"], row["identity"]), [])
        if not matches:
            raise RuntimeError("MATLAB parser callable absent from source inventory: " + row["identity"])
        reconciled.append({"parserOrdinal": row["ordinal"], "identity": row["identity"], "sourceOrdinals": matches})
    rows = [row for header in headers for row in header["callables"]]
    counts = {"callableDeclarationCount": len(rows), "directCallableCount": len(rows),
              "templateContextCallableCount": sum(bool(row["templateContext"]) for row in rows),
              "definitionCount": sum(row["kind"] == "definition" for row in rows),
              "declarationOnlyCount": sum(row["kind"] == "declaration" for row in rows),
              "friendFunctionCount": sum(row["friendDeclared"] for row in rows),
              "conditionalScopeBoundaryCount": sum(len(header["conditionalScopeBoundaries"]) for header in headers),
              "constructorInitializerCount": sum(row["constructorInitializer"] is not None for row in rows),
              "reconciledParserCallableCount": len(reconciled), "declarationErrorCount": 0,
              "bodyParseDiagnosticCount": sum(len(header["bodyParseDiagnostics"]) for header in headers)}
    result = {"schemaVersion": 1, "upstreamOpenCvVersion": upstream_version,
              "generator": "tools/MatlabUpstreamMap/extract_matlab_callables.py",
              "syntaxParser": {"treeSitterVersion": "0.25.2", "cppGrammarVersion": "0.23.4", "languageAbi": language.abi_version},
              "sourceInventoryPath": ("compatibility/" if upstream_version == "5.0.0" else "compatibility/opencv-5.1.0/") + "matlab-source-reviewed-extensions.json",
              "reviewDisposition": "intentionally-omitted-unwrapped-matlab-source",
              "completeCallableCoverageClaimed": False, "concreteTemplateInstantiationsClaimed": False,
              "externalDeclarationClosureClaimed": False, "generatedBindingSurfaceIncluded": False,
              "matlabExecutionVerified": False, "limitation": LIMITATION,
              "counts": counts, "parserCallableReconciliation": reconciled, "headers": headers}
    write_json(output, result)
    lexical["sourceCallableInventory"] = {"path": ("compatibility/" if upstream_version == "5.0.0" else "compatibility/opencv-5.1.0/") + "matlab-source-callables.json", "sha256": sha256(output), "counts": counts}
    write_json(lexical_path, lexical)
    raw_map["sourceReviewedExtensionSha256"] = sha256(lexical_path)
    write_json(raw_path, raw_map)
    print("MATLAB_SOURCE_CALLABLE_EXTRACTION_OK " + json.dumps(counts, separators=(",", ":")))


if __name__ == "__main__":
    cli = argparse.ArgumentParser()
    cli.add_argument("--workspace", required=True)
    cli.add_argument("--output", required=True)
    cli.add_argument("--syntax-parser-root", required=True)
    cli.add_argument("--upstream-version", choices=("5.0.0", "5.1.0"), default="5.0.0")
    args = cli.parse_args()
    extract(*(Path(value).resolve() for value in (args.workspace, args.output, args.syntax_parser_root)), upstream_version=args.upstream_version)

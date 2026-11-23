#!/usr/bin/env python3
"""Inventory uninstantiated Cudev callable declarations with a pinned C++ CST.

Expand only the 44 local declaration generators. Keep every source branch,
source occurrence and template context; never invent concrete instantiations.
No external include, macro environment, compiler or CUDA device is used.
"""

import argparse
import bisect
import importlib.metadata
import importlib.util
import json
import re
import sys
from pathlib import Path

from source_inventory import compact, digest
from extract_cudev import ATTRIBUTES, write_json
from extract_ximgproc import convert, sha256

RUNTIME_MACROS = {"CV_CUDEV_SAFE_CALL", "CV_CUDEV_DESCALE", "__shfl", "__shfl_xor", "__shfl_down"}
NAMESPACE_MACROS = re.compile(r"\b(?:_CCCL_BEGIN_NAMESPACE_CUDA_STD|_CCCL_END_NAMESPACE_CUDA_STD|_LIBCUDACXX_BEGIN_NAMESPACE_STD|_LIBCUDACXX_END_NAMESPACE_STD)\b")
TRAIT_NORMALIZATIONS = {
    "sizeof(A) >= sizeof(B)": "(sizeof(A)>=sizeof(B))",
    "unsigned(VecTraits<A>::cn) != unsigned(VecTraits<B>::cn)": "((unsigned)VecTraits<A>::cn!=(unsigned)VecTraits<B>::cn)",
}
LIMITATION = ("Counts are uninstantiated source declaration occurrences, including repeated declarations and inactive branches. "
              "Local declaration macros are expanded using their recorded source definitions. External includes/macros, imported using targets, "
              "concrete template instantiations, C++ type checking, linking and CUDA execution remain outside this syntax inventory. "
              "The four external namespace-wrapper macro markers occur only around tuple trait specializations with no callables and are blanked for CST parsing. "
              "Source head attributes, qualifiers, access, template context and conditional provenance are retained; no package wrapper parity is claimed.")


def expand_macro(definition, call):
    # The audited declaration-generator arguments contain no commas inside
    # parentheses or strings. Fail if that pinned source contract changes.
    args = [part.strip() for part in call["arguments"].split(",")]
    if len(args) != len(definition["parameters"]) or any(re.search(r'[()"\']', part) for part in args):
        raise ValueError("Declaration macro requires a new argument parser: " + call["name"])
    lookup = dict(zip(definition["parameters"], args))
    body = re.sub(r"\b[A-Za-z_]\w*\b", lambda match: lookup.get(match.group(), match.group()), definition["replacement"])
    while "##" in body:
        replaced = re.sub(r"(\w+)\s*##\s*(\w+)", lambda match: match[1] + match[2], body)
        if replaced == body:
            raise ValueError("Unsupported declaration macro token paste")
        body = replaced
    return body


def prepare(text, header, definitions):
    chunks, segments, expansions, cursor, length = [], [], [], 0, 0
    declaration_names = set(definitions) - RUNTIME_MACROS

    def append(value, start, expansion=None):
        nonlocal length
        encoded = value.encode("utf-8")
        chunks.append(encoded)
        segments.append({"start": length, "end": length + len(encoded), "sourceStart": start, "expansion": expansion})
        length += len(encoded)

    for call in header["macroInvocations"]:
        if call["name"] in RUNTIME_MACROS:
            continue
        definition = definitions[call["name"]]
        if len(call["sourceDefinitionIds"]) != 1 or call["sourceDefinitionIds"][0] != definition["id"]:
            raise ValueError("Ambiguous declaration macro source")
        if definition["sourceHeader"] != header["path"]:
            raise ValueError("Cross-header declaration macro requires include-order evaluation")
        if call["startOffset"] < cursor:
            raise ValueError("Overlapping declaration macro invocations")
        append(text[cursor:call["startOffset"]], cursor)
        body = expand_macro(definition, call)
        if any(re.search(r"\b" + re.escape(name) + r"\s*\(", body) for name in declaration_names):
            raise ValueError("Nested declaration macro expansion requires explicit support")
        if "#" in body:
            raise ValueError("Declaration macro stringification is unsupported")
        expansion = {"name": call["name"], "definitionId": definition["id"], "startOffset": call["startOffset"],
                     "endOffset": call["endOffset"], "sourceSha256": call["sourceSha256"], "conditions": call["conditions"],
                     "expandedSha256": digest(body), "callableCount": 0}
        append(body, call["startOffset"], len(expansions))
        expansions.append(expansion)
        cursor = call["endOffset"]
    append(text[cursor:], cursor)
    raw = b"".join(chunks)
    normalized = raw.decode("utf-8")
    normalizations = []
    for pattern, replacement in TRAIT_NORMALIZATIONS.items():
        if len(pattern.encode()) != len(replacement.encode()):
            raise ValueError("Trait normalization must preserve byte positions")
        count = normalized.count(pattern)
        if count:
            expected_header = "/util/detail/type_traits.hpp" if pattern.startswith("sizeof") else "/util/type_traits.hpp"
            if not header["path"].endswith(expected_header) or count != 1:
                raise ValueError("Non-callable trait normalization scope drifted")
            normalizations.append({"kind": "equivalent-non-callable-trait-expression", "source": pattern, "replacement": replacement, "count": count})
            normalized = normalized.replace(pattern, replacement)
    marker_count = len(NAMESPACE_MACROS.findall(normalized))
    if marker_count:
        if not header["path"].endswith("/ptr2d/zip.hpp") or marker_count != 4:
            raise ValueError("External namespace macro marker scope drifted")
        normalizations.append({"kind": "external-namespace-markers-around-non-callable-tuple-traits", "count": marker_count})
        normalized = NAMESPACE_MACROS.sub(lambda match: " " * len(match.group()), normalized)
    normalized = ATTRIBUTES.sub(lambda match: " " * len(match.group()), normalized).encode("utf-8")
    if len(raw) != len(normalized):
        raise ValueError("CST normalization changed byte coordinates")
    return raw, normalized, segments, expansions, normalizations


def inventory_callables(parser, raw, normalized, segments, expansions, source_text, lexical):
    tree = parser.parse(normalized)
    rows, diagnostics, contexts, consumed = [], [], [], set()
    segment_starts = [segment["start"] for segment in segments]
    # Source branch contexts are taken from the already independently checked
    # lexical inventory, not inferred from a preprocessed active configuration.
    # Rebuild branch events for non-template members and ordinary declarations.
    from source_inventory import inventory
    _, source_tokens, source_span = inventory(source_text)
    offsets = [token.start() for token in source_tokens]

    def origin(start, end):
        segment_index = bisect.bisect_right(segment_starts, start) - 1
        segment = segments[segment_index]
        if end > segment["end"]:
            raise ValueError("Callable head crosses a declaration expansion boundary")
        relative_start, relative_end = start - segment["start"], end - segment["start"]
        if segment["expansion"] is not None:
            expansion = expansions[segment["expansion"]]
            return {"kind": "macro-expansion", "expansionIndex": segment["expansion"],
                    "startOffset": relative_start, "endOffset": relative_end}, expansion["conditions"]
        before = raw[segment["start"]:start].decode("utf-8")
        content = raw[start:end].decode("utf-8")
        source_start = segment["sourceStart"] + len(before)
        source_end = source_start + len(content)
        if source_text[source_start:source_end] != content:
            raise ValueError("Direct callable source mapping drifted")
        token_index = bisect.bisect_left(offsets, source_start)
        if token_index == len(offsets) or offsets[token_index] != source_start:
            raise ValueError("Callable head does not start at a source token")
        return {"kind": "direct", "startOffset": source_start, "endOffset": source_end}, source_span(source_start, source_end)["conditions"]

    def text(node):
        return compact(raw[node.start_byte:node.end_byte].decode("utf-8"))

    def function_parts(declarator):
        def inner(node):
            child = node.child_by_field_name("declarator")
            if child is None and node.type in {"reference_declarator", "abstract_reference_declarator"}:
                children = [part for part in node.named_children if part.type != "comment"]
                if len(children) != 1:
                    raise ValueError("Ambiguous reference declarator")
                child = children[0]
            return child

        while declarator is not None:
            if declarator.type == "qualified_identifier":
                name = declarator.child_by_field_name("name")
                parts = function_parts(name)
                if parts is None:
                    return None
                prefix = compact(raw[declarator.start_byte:name.start_byte].decode("utf-8"))
                return prefix + parts[0], parts[1]
            if declarator.type in {"function_declarator", "abstract_function_declarator"}:
                name = declarator.child_by_field_name("declarator")
                if name is None or name.type == "parenthesized_declarator":
                    return None  # Function-pointer field/alias, not a callable.
                value = text(name)
                if value.startswith("operator"):
                    value = "operator " + value[len("operator"):].strip()
                return value, declarator
            if declarator.type == "operator_cast":
                leaf = declarator.child_by_field_name("declarator")
                while leaf is not None and leaf.type != "abstract_function_declarator":
                    leaf = inner(leaf)
                if leaf is None:
                    raise ValueError("Conversion operator lost its parameter declarator")
                name = compact(raw[declarator.start_byte:leaf.start_byte].decode("utf-8"))
                return name, leaf
            declarator = inner(declarator)
        return None

    def record(node, declarator, scopes, templates, access, context_indices):
        parts = function_parts(declarator)
        if parts is None:
            return False
        name, function = parts
        key = (function.start_byte, function.end_byte)
        if key in consumed:
            raise ValueError("Callable syntax node was recorded twice")
        consumed.add(key)
        body = node.child_by_field_name("body")
        end = body.start_byte if body is not None else node.end_byte
        for child in node.named_children:
            if child.type == "field_initializer_list":
                end = min(end, child.start_byte)
        start = node.start_byte
        # Blanked CUDA attributes can precede the CST node's first token.
        prefix = raw[:start].decode("utf-8")
        attrs = re.search(r"(?:(?:__host__|__device__|__global__|__forceinline__|__shared__|__constant__)\s*)+$", prefix)
        if attrs:
            start = len(prefix[:attrs.start()].encode("utf-8"))
        head = raw[start:end].decode("utf-8")
        if ATTRIBUTES.sub(lambda match: " " * len(match.group()), head).encode("utf-8") != normalized[start:end]:
            raise ValueError("A non-attribute normalization touched a callable head")
        source, conditions = origin(start, end)
        if source["kind"] == "macro-expansion":
            expansions[source["expansionIndex"]]["callableCount"] += 1
        parameters = function.child_by_field_name("parameters")
        if parameters is None:
            raise ValueError("Callable has no parameter list")
        parameter_rows = [child for child in parameters.named_children if child.type != "comment"]
        ellipsis = any(child.type == "..." for child in parameters.children)
        arity = 0 if len(parameter_rows) == 1 and text(parameter_rows[0]) == "void" else len(parameter_rows) + int(ellipsis)
        rows.append({"qualifiedName": "::".join([*scopes, name]), "head": compact(head), "headSha256": digest(head),
                     "declarator": text(declarator), "templateContext": templates, "access": access,
                     "cudaAttributes": list(dict.fromkeys(ATTRIBUTES.findall(head))), "parameterCount": arity,
                     "variadic": ellipsis or any(child.type == "variadic_parameter_declaration" for child in parameter_rows),
                     "kind": "definition" if node.type == "function_definition" else "declaration",
                     "contextIndices": list(context_indices), "expandedStartByte": start, "expandedEndByte": end,
                     "declaratorStartByte": declarator.start_byte, "declaratorEndByte": declarator.end_byte,
                     "parametersStartByte": parameters.start_byte, "parametersEndByte": parameters.end_byte,
                     "origin": source, "conditions": conditions})
        return True

    def context(node, value, body_start):
        index = len(contexts)
        contexts.append({"kind": node.type, "startByte": node.start_byte, "endByte": node.end_byte,
                         "bodyStartByte": body_start, "value": text(value),
                         "valueStartByte": value.start_byte, "valueEndByte": value.end_byte})
        return index

    def walk(node, scopes=(), templates=(), access="namespace", context_indices=()):
        if node.type == "function_definition":
            if not record(node, node.child_by_field_name("declarator"), scopes, list(templates), access, context_indices):
                raise ValueError(f"Unrecognized function definition declarator in {lexical['path']}: {node.child_by_field_name('declarator')}; {text(node)[:500]}")
            return
        if node.type in {"declaration", "field_declaration"}:
            declarations = node.children_by_field_name("declarator")
            found = sum(record(node, declarator, scopes, list(templates), access, context_indices) for declarator in declarations)
            if found:
                if found != 1:
                    raise ValueError("Multiple callable declarators need separate source spans")
                return
        if node.type in {"type_definition", "alias_declaration"}:
            return
        if node.type == "namespace_definition":
            name = node.child_by_field_name("name")
            if name is None:
                raise ValueError("Anonymous namespace requires a source identity")
            body = node.child_by_field_name("body")
            index = context(node, name, body.start_byte)
            walk(body, (*scopes, text(name)), templates, "namespace", (*context_indices, index))
            return
        if node.type in {"class_specifier", "struct_specifier"}:
            name, body = node.child_by_field_name("name"), node.child_by_field_name("body")
            if body is None:
                return
            if name is None:
                raise ValueError("Anonymous class requires a source identity")
            member_access = "private" if node.type == "class_specifier" else "public"
            index = context(node, name, body.start_byte)
            for child in body.named_children:
                if child.type == "access_specifier":
                    member_access = text(child)
                else:
                    walk(child, (*scopes, text(name)), templates, member_access, (*context_indices, index))
            return
        if node.type == "template_declaration":
            parameters = node.child_by_field_name("parameters")
            if parameters is None:
                raise ValueError("Template declaration lost its parameters")
            index = context(node, parameters, parameters.end_byte)
            for child in node.named_children:
                if child != parameters:
                    walk(child, scopes, (*templates, text(parameters)), access, (*context_indices, index))
            return
        for child in node.named_children:
            walk(child, scopes, templates, access, context_indices)

    def check(node, in_body=False):
        if node.type == "ERROR" or node.is_missing:
            if not in_body:
                raise ValueError(f"CST declaration error at line {node.start_point.row + 1}: {text(node)}")
            context_start, context_end = max(0, node.start_byte - 50), min(len(raw), node.end_byte + 65)
            diagnostics.append({"kind": node.type, "missing": node.is_missing,
                                "expandedStartByte": node.start_byte, "expandedEndByte": node.end_byte,
                                "text": text(node), "sourceSha256": digest(raw[node.start_byte:node.end_byte].decode("utf-8")),
                                "contextStartByte": context_start, "contextEndByte": context_end,
                                "context": compact(raw[context_start:context_end].decode("utf-8"))})
        for child in node.children:
            check(child, in_body or (node.type == "function_definition" and child == node.child_by_field_name("body")))

    check(tree.root_node)
    walk(tree.root_node)
    markers = list(NAMESPACE_MACROS.finditer(raw.decode("utf-8")))
    if markers:
        marker_start = len(raw.decode("utf-8")[:markers[0].start()].encode("utf-8"))
        if any(row["expandedStartByte"] >= marker_start for row in rows):
            raise ValueError("External namespace markers must enclose no callables")
    syntax_counts = {"surfaceFunctionDefinitionCount": 0, "surfaceFunctionDeclaratorCount": 0,
                     "surfaceConversionDeclaratorCount": 0, "bodyFunctionDeclaratorCount": 0,
                     "bodyAbstractFunctionDeclaratorCount": 0, "excludedTypeDeclaratorCount": 0}

    def audit(node, in_body=False, in_alias=False):
        in_alias = in_alias or node.type in {"type_definition", "alias_declaration"}
        if node.type == "function_definition" and not in_body and not in_alias:
            syntax_counts["surfaceFunctionDefinitionCount"] += 1
        if node.type in {"function_declarator", "abstract_function_declarator"}:
            if in_body:
                syntax_counts["bodyFunctionDeclaratorCount" if node.type == "function_declarator" else "bodyAbstractFunctionDeclaratorCount"] += 1
            elif in_alias or (node.type == "function_declarator" and node.child_by_field_name("declarator").type == "parenthesized_declarator"):
                syntax_counts["excludedTypeDeclaratorCount"] += 1
            else:
                if (node.start_byte, node.end_byte) not in consumed:
                    raise ValueError("Unclassified surface function syntax node: " + str(node))
                syntax_counts["surfaceFunctionDeclaratorCount" if node.type == "function_declarator" else "surfaceConversionDeclaratorCount"] += 1
        for child in node.children:
            audit(child, in_body or (node.type == "function_definition" and child == node.child_by_field_name("body")), in_alias)

    audit(tree.root_node)
    if syntax_counts["surfaceFunctionDeclaratorCount"] + syntax_counts["surfaceConversionDeclaratorCount"] != len(rows):
        raise ValueError("Surface callable syntax partition drifted")
    return rows, diagnostics, contexts, syntax_counts


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--workspace", required=True)
    parser.add_argument("--output", required=True)
    parser.add_argument("--syntax-parser-root", required=True)
    parser.add_argument("--upstream-version", choices=("5.0.0", "5.1.0"), default="5.0.0")
    args = parser.parse_args()
    workspace, output, syntax_root = (Path(value).resolve() for value in (args.workspace, args.output, args.syntax_parser_root))
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
    syntax_parser = Parser(language)
    from test_source_callables import self_test
    self_test(syntax_parser)
    raw_path = output.parent / "cudev-upstream-raw.json"
    lexical_path = output.parent / "cudev-source-reviewed-extensions.json"
    raw_map, lexical = (json.loads(path.read_text(encoding="utf-8")) for path in (raw_path, lexical_path))
    definitions = {row["name"]: {**row, "sourceHeader": header["path"]} for header in lexical["headers"] for row in header["macroDefinitions"]}
    if len(definitions) != 49 or len(set(definitions) - RUNTIME_MACROS) != 44:
        raise RuntimeError("Cudev local macro inventory drifted")
    headers, ordinal = [], 0
    for header in lexical["headers"]:
        path = workspace / header["path"]
        source_text = path.read_text(encoding="utf-8")
        if sha256(path) != header["sha256"]:
            raise RuntimeError("Cudev installed source hash drifted")
        raw, normalized, segments, expansions, normalizations = prepare(source_text, header, definitions)
        callables, diagnostics, contexts, syntax_counts = inventory_callables(syntax_parser, raw, normalized, segments, expansions, source_text, header)
        for row in callables:
            row["ordinal"] = ordinal
            ordinal += 1
        headers.append({"path": header["path"], "sha256": header["sha256"], "normalizedInputSha256": digest(normalized.decode("utf-8")),
                        "normalizations": normalizations, "macroExpansions": expansions, "contexts": contexts,
                        "syntaxNodeCounts": syntax_counts, "callables": callables,
                        "declarationErrorCount": 0, "bodyParseDiagnostics": diagnostics})
    parser_path = workspace / raw_map["parserPath"]
    spec = importlib.util.spec_from_file_location("cudev_pinned_hdr_parser", parser_path)
    hdr = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(hdr)
    candidates = {}
    pinned_parser = hdr.CppHeaderParser()
    for header in headers:
        for row in header["callables"]:
            if row["templateContext"]:
                continue
            head = ATTRIBUTES.sub("", row["head"]).strip().rstrip(";")
            head = re.sub(r"\binline\b", "", head).strip()
            value = pinned_parser.parse_func_decl_no_wrap(head)
            value[0] = row["qualifiedName"].replace("::", ".")
            identity = convert(value, 0, header["path"])["identity"]
            candidates.setdefault((header["path"], identity), []).append(row["ordinal"])
    reconciled = []
    for row in raw_map["declarations"]:
        if row["kind"] != "callable":
            continue
        matches = candidates.get((row["sourceHeader"], row["identity"]), [])
        if not matches:
            raise RuntimeError("Pinned parser callable absent from source CST inventory: " + row["identity"])
        reconciled.append({"parserOrdinal": row["ordinal"], "sourceOrdinals": matches, "identity": row["identity"]})
    all_rows = [row for header in headers for row in header["callables"]]
    counts = {"callableDeclarationCount": len(all_rows), "directCallableCount": sum(row["origin"]["kind"] == "direct" for row in all_rows),
              "macroGeneratedCallableCount": sum(row["origin"]["kind"] == "macro-expansion" for row in all_rows),
              "templateContextCallableCount": sum(bool(row["templateContext"]) for row in all_rows),
              "definitionCount": sum(row["kind"] == "definition" for row in all_rows),
              "declarationOnlyCount": sum(row["kind"] == "declaration" for row in all_rows),
              "declarationMacroDefinitionCount": 44, "expandedMacroInvocationCount": sum(len(header["macroExpansions"]) for header in headers),
              "nonCallableExpansionCount": sum(expansion["callableCount"] == 0 for header in headers for expansion in header["macroExpansions"]),
              "reconciledParserCallableCount": len(reconciled), "declarationErrorCount": 0,
              "bodyParseDiagnosticCount": sum(len(header["bodyParseDiagnostics"]) for header in headers)}
    result = {"schemaVersion": 1, "upstreamOpenCvVersion": args.upstream_version, "generator": "tools/CudevUpstreamMap/extract_source_callables.py",
              "syntaxParser": {"treeSitterVersion": "0.25.2", "cppGrammarVersion": "0.23.4", "languageAbi": language.abi_version},
              "sourceInventoryPath": ("compatibility/" if args.upstream_version == "5.0.0" else "compatibility/opencv-5.1.0/") + "cudev-source-reviewed-extensions.json", "reviewDisposition": "intentionally-omitted-unwrapped-device-source",
              "localDeclarationMacroExpansionClaimed": True, "concreteTemplateInstantiationsClaimed": False,
              "externalDeclarationClosureClaimed": False, "cudaExecutionVerified": False, "limitation": LIMITATION,
              "counts": counts, "parserCallableReconciliation": reconciled, "headers": headers}
    write_json(output, result)
    lexical["sourceCallableInventory"] = {"path": ("compatibility/" if args.upstream_version == "5.0.0" else "compatibility/opencv-5.1.0/") + "cudev-source-callables.json", "sha256": sha256(output), "counts": counts}
    write_json(lexical_path, lexical)
    raw_map["sourceReviewedExtensionSha256"] = sha256(lexical_path)
    write_json(raw_path, raw_map)
    print("CUDEV_SOURCE_CALLABLE_EXTRACTION_OK " + json.dumps(counts, separators=(",", ":")))


if __name__ == "__main__":
    main()

"""Conservative C++ source inventory, not a compiler or an instantiation parser.

Keep every branch, exclude comments/literals/directives from token discovery,
and fail on an unbalanced source construct. Coordinates use LF-normalized text.
Nested template-template parameters are counted separately from declarations.
"""

import hashlib
import re


LEXEME = re.compile(r'//[^\n]*|/\*[\s\S]*?\*/|"(?:\\[\s\S]|[^"\\])*"|\'(?:\\[\s\S]|[^\'\\])*\'')
TOKEN = re.compile(r"[A-Za-z_]\w*|\d+(?:\.\d+)?|::|##|\.\.\.|[^\s]")
DIRECTIVE = re.compile(r"^[ \t]*#(?:[^\n]*\\\n)*[^\n]*(?:\n|$)", re.MULTILINE)


def digest(text):
    return hashlib.sha256(text.encode("utf-8")).hexdigest()


def blank(text):
    return "".join("\n" if char == "\n" else " " for char in text)


def mask(text, literals=True):
    def replace(match):
        value = match.group()
        return blank(value) if literals or value.startswith(("//", "/*")) else value
    return LEXEME.sub(replace, text)


def compact(text):
    return " ".join(mask(text, literals=False).split())


def matching(tokens, start, opening="(", closing=")"):
    depth = 0
    for index in range(start, len(tokens)):
        word = tokens[index].group()
        if word == opening:
            depth += 1
        elif word == closing:
            depth -= 1
            if depth == 0:
                return index
    raise ValueError(f"Unbalanced {opening}{closing} at offset {tokens[start].start()}")


def template_end(tokens, start):
    # Relational/shift operators in parenthesized default arguments must not
    # close a template parameter list. Nested template-template lists do.
    depth, groups = 0, []
    pairs = {"(": ")", "[": "]", "{": "}"}
    for index in range(start, len(tokens)):
        word = tokens[index].group()
        if word in pairs:
            groups.append(pairs[word])
        elif groups and word == groups[-1]:
            groups.pop()
        elif not groups and word == "<":
            depth += 1
        elif not groups and word == ">":
            depth -= 1
            if depth == 0:
                return index
    raise ValueError(f"Unbalanced template parameters at offset {tokens[start].start()}")


def inventory(text):
    text = text.replace("\r\n", "\n")
    comments_removed = mask(text, literals=False)
    masked = mask(text)
    directives = list(DIRECTIVE.finditer(comments_removed))
    code = list(masked)
    conditions, stack, events = {}, [], []
    definitions, condition_rows = [], []
    for directive in directives:
        logical = re.sub(r"\\\n", " ", directive.group()).strip()
        match = re.match(r"#\s*(\w+)\s*(.*)", logical)
        if not match:
            raise ValueError("Unrecognized preprocessor directive")
        command, rest = match.groups()
        line = text.count("\n", 0, directive.start()) + 1
        before = [item["current"] for item in stack]
        if command in {"if", "ifdef", "ifndef"}:
            expression = rest if command == "if" else f"{'!' if command == 'ifndef' else ''}defined({rest})"
            stack.append({"seen": [expression], "current": expression, "else": False})
        elif command in {"elif", "else"}:
            if not stack or stack[-1]["else"]:
                raise ValueError("Unmatched or repeated conditional branch")
            item = stack[-1]
            prior = " || ".join(f"({part})" for part in item["seen"])
            item["current"] = f"!({prior})" + (f" && ({rest})" if command == "elif" else "")
            item["seen"].append(rest)
            item["else"] = command == "else"
        elif command == "endif":
            if not stack:
                raise ValueError("Unmatched endif")
            stack.pop()
        if command in {"if", "ifdef", "ifndef", "elif", "else", "endif"}:
            condition_rows.append({"line": line, "directive": logical})
        if command == "define":
            define = re.match(r"([A-Za-z_]\w*)\(([^)]*)\)([\s\S]*)", rest)
            if define:
                name, parameters, body = define.groups()
                definitions.append({"name": name, "parameters": [part.strip() for part in parameters.split(",")] if parameters else [],
                                    "startOffset": directive.start(), "endOffset": directive.end(), "line": line,
                                    "sourceSha256": digest(text[directive.start():directive.end()]),
                                    "replacement": compact(body), "templateKeywordCount": len(re.findall(r"\btemplate\s*<", mask(body))),
                                    "conditions": before})
        events.append((directive.end(), [item["current"] for item in stack]))
        code[directive.start():directive.end()] = blank(directive.group())
    if stack:
        raise ValueError("Unclosed conditional branch")
    event_index, active = 0, []
    tokens = list(TOKEN.finditer("".join(code)))
    for token in tokens:
        while event_index < len(events) and events[event_index][0] <= token.start():
            active = events[event_index][1]
            event_index += 1
        conditions[token.start()] = list(active)

    def span(start, end):
        return {"startOffset": start, "endOffset": end, "line": text.count("\n", 0, start) + 1,
                "sourceSha256": digest(text[start:end]), "conditions": conditions[start]}

    templates, parameters, ranges = [], [], []
    for index, token in enumerate(tokens):
        if token.group() != "template" or index + 1 == len(tokens) or tokens[index + 1].group() != "<":
            continue
        close = template_end(tokens, index + 1)
        enclosing = next((row for row in reversed(ranges) if row[0] < index < row[1]), None)
        if enclosing:
            parameters.append({**span(token.start(), tokens[close].end()), "enclosingTemplateOffset": tokens[enclosing[0]].start()})
            continue
        ranges.append((index, close))
        cursor, groups = close + 1, []
        while cursor < len(tokens):
            word = tokens[cursor].group()
            if not groups and word in {"{", ";"}:
                break
            if word in {"(", "["}:
                groups.append(")" if word == "(" else "]")
            elif groups and word == groups[-1]:
                groups.pop()
            cursor += 1
        if cursor == len(tokens):
            raise ValueError("Template declaration has no body or semicolon")
        head = compact(text[tokens[close].end():tokens[cursor].start()])
        class_match = re.match(r"(class|struct)\s+([A-Za-z_]\w*)", head)
        kind = "class-template" if class_match else "function-template"
        if not class_match and "(" not in head:
            raise ValueError(f"Unrecognized template declaration: {head}")
        end = matching(tokens, cursor, "{", "}") if tokens[cursor].group() == "{" else cursor
        if class_match and end + 1 < len(tokens) and tokens[end + 1].group() == ";":
            end += 1
        templates.append({**span(token.start(), tokens[end].end()), "kind": kind,
                          "templateParameters": compact(text[tokens[index + 1].start():tokens[close].end()]),
                          "declarationHead": head, "headEndOffset": tokens[cursor].start()})
    return {"templates": templates, "templateParameters": parameters, "macroDefinitions": definitions,
            "conditionalDirectives": condition_rows}, tokens, span


def macro_invocations(text, tokens, span, definitions):
    rows = []
    for index, token in enumerate(tokens):
        name = token.group()
        if name not in definitions or index + 1 == len(tokens) or tokens[index + 1].group() != "(":
            continue
        close = matching(tokens, index + 1)
        rows.append({**span(token.start(), tokens[close].end()), "name": name,
                     "arguments": compact(text[tokens[index + 1].end():tokens[close].start()]),
                     "sourceDefinitionIds": definitions[name]})
    return rows


def self_test():
    sample = '''#ifndef FIXTURE
#define FIXTURE
// template<class Ghost> struct Ghost {};
#define MAKE(T) template<class U> struct T { const char* s = "template<class Fake>"; };
#if ARCH >= 300
template<class T, template<class> class C, int N = (8 >> 1)> struct A {
  template<typename U> __device__ U f(U a = U()) { return a; }
  const char* s = "{ template<class Fake> }";
};
MAKE(Visible)
#elif ARCH >= 200
template<> struct A<int, Box, 4> {};
#else
template<class T> void host(T value) { /* } */ }
#endif
#endif
'''
    result, tokens, span = inventory(sample)
    calls = macro_invocations(sample, tokens, span, {"MAKE": ["fixture:4"]})
    assert len(result["templates"]) == 4 and len(result["templateParameters"]) == 1
    assert len(result["macroDefinitions"]) == 1 and result["macroDefinitions"][0]["templateKeywordCount"] == 1
    assert len(calls) == 1 and calls[0]["arguments"] == "Visible"
    assert result["templates"][0]["declarationHead"] == "struct A"
    assert result["templates"][1]["kind"] == "function-template"
    assert "!((ARCH >= 300)) && (ARCH >= 200)" in result["templates"][2]["conditions"]
    assert "!((ARCH >= 300) || (ARCH >= 200))" in result["templates"][3]["conditions"]
    for broken in ("template<class T> struct Broken {", "#if X\ntemplate<class T> struct A {};", "#else\n", "#if X\n#else\n#else\n#endif\n"):
        try:
            inventory(broken)
        except ValueError:
            pass
        else:
            raise AssertionError("Invalid source inventory was accepted")


if __name__ == "__main__":
    self_test()
    print("CPP_SOURCE_INVENTORY_SELF_TEST_OK")

"""Focused syntax/provenance regressions; called during source generation."""

from extract_source_callables import prepare, inventory_callables, expand_macro
from source_inventory import inventory, macro_invocations, digest


def self_test(parser):
    def extract(source):
        lexical, tokens, span = inventory(source)
        lexical['path'] = 'fixture.hpp'
        definitions = {}
        for index, definition in enumerate(lexical['macroDefinitions']):
            definition['id'] = f'fixture:{index}'
            definitions[definition['name']] = {**definition, 'sourceHeader': 'fixture.hpp'}
        refs = {name: [definition['id']] for name, definition in definitions.items()}
        lexical['macroInvocations'] = macro_invocations(source, tokens, span, refs)
        raw, normalized, segments, expansions, _ = prepare(source, lexical, definitions)
        rows, diagnostics, contexts, counts = inventory_callables(parser, raw, normalized, segments, expansions, source, lexical)
        return rows, diagnostics, contexts, counts, expansions

    # Class-template non-template members, access, constructor initializers,
    # operators/conversions, reference returns, nested member templates and
    # defaulted/deleted definitions each exercise distinct CST shapes.
    rows, diagnostics, contexts, counts, _ = extract('''namespace cv { namespace cudev {
template<class T> class Box {
  int value;
  __host__ Box() : value(0) {}
public:
  __device__ T& get(T& x) { return x; }
  __device__ operator const T*() const { return 0; }
  __device__ T operator()(int x, int y) const { return T(); }
  template<class U> U convert(U x) { return x; }
  Box(const Box&) = delete;
  Box& operator=(const Box&) = default;
};
template<class T> Box<T>::operator const T*() const { return 0; }
}}''')
    assert len(rows) == 8 and not diagnostics
    assert rows[0]['qualifiedName'] == 'cv::cudev::Box::Box' and rows[0]['access'] == 'private'
    assert rows[1]['qualifiedName'].endswith('::get') and rows[1]['parameterCount'] == 1
    assert rows[1]['access'] == 'public' and rows[1]['cudaAttributes'] == ['__device__']
    assert rows[2]['qualifiedName'].endswith('::operator const T*')
    assert rows[3]['qualifiedName'].endswith('::operator ()') and rows[3]['parameterCount'] == 2
    assert rows[4]['templateContext'] == ['<class T>', '<class U>']
    assert rows[5]['kind'] == rows[6]['kind'] == 'definition'
    assert rows[7]['qualifiedName'] == 'cv::cudev::Box<T>::operator const T*'
    assert counts['surfaceFunctionDefinitionCount'] == 8
    assert counts['surfaceConversionDeclaratorCount'] == 2
    assert all(contexts[i]['startByte'] <= row['expandedStartByte'] < contexts[i]['endByte'] for row in rows for i in row['contextIndices'])

    rows, _, _, counts, _ = extract('''namespace cv {
typedef void (*Callback)(int);
using Other = int (*)(double);
struct Owner { void (*field)(int); int run(void); };
int use(int (*callback)(int));
}''')
    assert [row['qualifiedName'] for row in rows] == ['cv::Owner::run', 'cv::use']
    assert rows[0]['parameterCount'] == 0 and counts['excludedTypeDeclaratorCount'] == 4

    rows, _, _, _, _ = extract('void only(...); void mixed(int count, ...); template<class... T> void pack(T... values); template<class... T> void expanded(const tuple<T...>& values);')
    assert [row['parameterCount'] for row in rows] == [1, 2, 1, 1]
    assert [row['variadic'] for row in rows] == [True, True, True, False]

    rows, _, _, _, expansions = extract('''#define MAKE(t, suffix) template<class U> struct Name##suffix { __device__ t call(t x) const { return x; } };
namespace cv {
#if DEVICE
MAKE(int, A)
#else
MAKE(float, B)
#endif
}''')
    assert len(rows) == len(expansions) == 2
    assert rows[0]['qualifiedName'] == 'cv::NameA::call' and rows[1]['qualifiedName'] == 'cv::NameB::call'
    assert rows[0]['origin']['kind'] == 'macro-expansion' and rows[0]['conditions'] == ['DEVICE']
    assert rows[1]['conditions'] == ['!((DEVICE))']
    assert all(expansion['callableCount'] == 1 for expansion in expansions)

    # Unicode comments must not shift direct Unicode or expanded UTF-8 offsets;
    # attributes before the CST's first token must remain in the original head.
    source = '// 中文 provenance\n__host__ int f(int x = 1) { return x; }\n'
    rows, _, _, _, _ = extract(source)
    origin = rows[0]['origin']
    assert source[origin['startOffset']:origin['endOffset']].startswith('__host__')
    assert rows[0]['headSha256'] == digest(source[origin['startOffset']:origin['endOffset']])
    assert rows[0]['expandedStartByte'] == len(source[:origin['startOffset']].encode())

    rows, diagnostics, _, counts, _ = extract('void host() { kernel<<<1, 32>>>(); }')
    assert len(rows) == 1 and diagnostics and counts['bodyFunctionDeclaratorCount'] >= 0
    assert all(row['missing'] or row['kind'] == 'ERROR' for row in diagnostics)
    for malformed in ('int broken( {', 'template<class T> int broken(;', 'namespace cv { int broken( );'):
        try:
            extract(malformed)
        except ValueError:
            pass
        else:
            raise AssertionError('Malformed declaration was accepted: ' + malformed)
    definition = {'parameters': ['x'], 'replacement': 'x##tail'}
    assert expand_macro(definition, {'arguments': 'head', 'name': 'MAKE'}) == 'headtail'
    for arguments in ('a,b', 'nested(x)', '"literal"'):
        try:
            expand_macro(definition, {'arguments': arguments, 'name': 'MAKE'})
        except ValueError:
            pass
        else:
            raise AssertionError('Unsupported macro arguments were accepted')
    try:
        expand_macro({'parameters': ['x'], 'replacement': 'x##+'}, {'arguments': 'x', 'name': 'MAKE'})
    except ValueError:
        pass
    else:
        raise AssertionError('Invalid token paste was accepted')
    print('CUDEV_SOURCE_CALLABLE_SELF_TEST_OK')

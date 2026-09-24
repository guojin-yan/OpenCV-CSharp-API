# DNN Tokenizer And Text ADR / DNN Tokenizer 与文本 ADR

## Status

Deferred from the 5.0.1 stable surface. Tokenizer, text detection, and text recognition wrappers remain research/preview work until model provenance, vocabulary ownership, and output schemas are closed.

## Scope and provenance

Text models require more than a native `Net`: tokenizer vocabulary/merges, normalization rules, locale, model bytes, config bytes, license/source, and preprocessing revision all affect results. Every future wrapper must carry a reproducible model/vocabulary provenance record with SHA256 and license identity. The library must not download vocabularies or silently apply locale-specific normalization.

文本模型不只是 native `Net`：tokenizer vocabulary/merges、normalization 规则、locale、模型与 config 字节、license/source 和 preprocessing revision 都会影响结果。未来 wrapper 必须携带可复现的 model/vocabulary provenance、SHA256 和 license identity。库不能下载 vocabulary，也不能静默应用 locale-specific normalization。

## Owner and encoding rules

- UTF-8 input strings are copied for the native call and never retained by native code unless the upstream object explicitly documents ownership.
- Vocabulary and merge tables are caller-owned inputs at construction, then cloned or pinned only for the documented model lifetime.
- Returned token ids, offsets, confidence arrays, and text strings are independently owned managed results. Nested results dispose all child buffers on partial failure.
- Byte-preserving APIs must expose bytes or explicit encoding instead of silently decoding invalid UTF-8.

## Output contracts

Tokenizer output must distinguish token ids, attention masks, offsets, and special-token flags. Text detection must distinguish boxes, polygons, scores, and source image coordinates. Text recognition must distinguish decoded text, byte text, confidence per token/character, and alignment offsets. A single dictionary or loosely typed `object[]` result is not an acceptable stable contract.

Dynamic batch/sequence lengths require checked limits. Unknown shape, vocabulary mismatch, invalid UTF-8, and output-count mismatch fail closed before a result object is returned. Model execution success does not prove text accuracy.

## Stop and promotion criteria

Keep this family outside the 5.0.1 stable API unless the repository owns a small deterministic fixture, a reviewed model/vocabulary license, fixed preprocessing, CPU execution evidence, malformed input/output tests, nested ownership tests, and a cross-TFM report. ONNX Runtime or tokenizer-specific third-party dependencies remain separate adapter packages. Without these facts, consumers use `Net` and own the text pipeline.

在仓库拥有小型 deterministic fixture、审核过的 model/vocabulary license、固定 preprocessing、CPU 执行证据、malformed 输入输出测试、nested ownership 测试和跨 TFM report 之前，该 family 不进入 5.0.1 稳定 API。ONNX Runtime 或 tokenizer-specific 第三方依赖保持独立 adapter package。

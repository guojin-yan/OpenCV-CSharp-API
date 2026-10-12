# Signal Upstream Map / Signal 上游映射

This guide records the scoped OpenCV 5.0.0 contrib `signal` evidence map. It is a parser/header boundary record, not a claim of repository-wide contrib parity.

本指南记录固定 OpenCV 5.0.0 contrib `signal` 的 parser/header 证据范围，不宣称整个 contrib 仓库已经 parity。

## Scope / 范围

The map covers the parser-emitted closure from `opencv-source/opencv_contrib-5.0.0/modules/signal/include/opencv2/signal.hpp`. The umbrella includes one parser-emitted callable from `signal_resample.hpp`: `cv.signal.resampleSignal`.

映射覆盖 `opencv-source/opencv_contrib-5.0.0/modules/signal/include/opencv2/signal.hpp` 的 parser closure。该 umbrella 由 `signal_resample.hpp` 发出一个 callable：`cv.signal.resampleSignal`。

The callable is `intentionally-omitted`. The current package has no native Signal wrapper entrypoint or managed public owner, so this map records the optional module boundary without inferring a native ABI, managed API, or runtime parity contract.

该 callable 标记为 `intentionally-omitted`。当前包没有 Signal native wrapper entrypoint 或 managed public owner，因此本 map 只记录可选模块边界，不推断 native ABI、managed API 或 runtime parity 契约。

## Verification / 验证

```powershell
pwsh -NoProfile -File .\scripts\Test-SignalUpstreamMap.ps1
pwsh -NoProfile -File .\scripts\Test-UpstreamMapCatalog.ps1
```

The focused guard verifies the pinned umbrella/include header and parser hashes, the one-callable closure, one intentional omission, zero native/managed evidence, and the prohibition on repository-wide parity claims.

专项 guard 会检查固定 umbrella/include header 与 parser hash、单一 callable closure、一个 intentional omission、native/managed 证据为零，以及禁止 repository-wide parity 声明。

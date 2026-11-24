# OpenCV 5.1 Performance Baseline

The [5.1 baseline](https://github.com/guojin-yan/OpenCV-CSharp-API/blob/opencv5.x/packaging/performance/perf-001-5.1.0-baseline.json) measures the current Windows x64 Full package, including all 43 native DLLs, from clean benchmark source `0440b7daece3cf59dec554673b4fc6463fd41a08`. It retains twelve exact harness outputs: codec on net8.0, typed Mat on net8.0/net10.0, and Video/DNN on net10.0, each repeated in three independent processes with 100 warmed iterations. Fourteen metric groups use the median of the three measurements, including the reused caller-owned codec writer.

The runtime archive is `JYPPX.OpenCV.runtime.win-x64.5.1.0.nupkg`, SHA256 `fe58985b2909978a6e2d80cc6dec3df5e0e08c5f015019359f0d77ffd611f921`. The record binds the local package/SBOM evidence, every DLL hash, managed and harness assembly hashes, fixed workloads, Windows/CPU identity, .NET SDK and installed runtimes, and stopwatch frequency. Each child uses DLLs extracted directly from that archive, a restricted PATH and no OpenCV or .NET runtime overrides. The source set includes the managed implementation, three harnesses, model fixture and measurement helpers.

| Scenario | Managed bytes per 100 iterations | Sampled peak working set |
| --- | ---: | ---: |
| PNG byte-array encode | 37,640 | — |
| PNG encode with a new writer | 40,840 | — |
| PNG encode with a reused writer | 3,240 | — |
| Byte-array decode | 7,240 | — |
| Span decode with preflight | 60,840 | — |
| Stream decode with preflight | 8,296,040 | — |
| MJPG file VideoIO | 1,306,496 | 42,856,448 |
| Identity ONNX inference | 383,104 | 45,223,936 |

Typed Mat retains the same input checksum across both frameworks and all three access paths. Per 100 iterations, the row accessor allocates 40 bytes, `MatView` 10,440 bytes, and `ReadOnlyMatView` 12,840 bytes. These results retain the preview status and the allocation tradeoff of the view APIs.

The OpenCV 5.1 unified ONNX engine reports one profile layer, zero weight bytes and 16 blob bytes for this fixed 147-byte model; the historical 5.0 engine reported two layers and 48 blob bytes. This diagnostic change is recorded explicitly. The output checksum remains 1,000,000. The upstream warning that the new graph engine does not support target selection is retained in the captured stderr; this CPU-only fixture establishes no accelerator support.

To create a fresh measurement from a clean checkout with the recorded local Full package available:

```powershell
./scripts/Measure-Performance51Evidence.ps1 -OutputPath artifacts/performance-5.1.0/candidate.json
./scripts/Test-Performance51Evidence.ps1 -RequirePackageArtifacts
./scripts/Test-Performance51Evidence.ps1 -CandidatePath artifacts/performance-5.1.0/candidate.json
```

The committed baseline is immutable. The guard verifies the exact run inventory, source and package bindings, output bytes, checksums, median derivation and release boundaries. Candidate comparison requires a descendant source commit and identical runner, SDK/runtimes, workloads and runtime bytes. Limits remain elapsed ticks +20%, managed allocation +10% and sampled peak working set +10%; comparison rejects incompatible inputs before applying these limits. The original [5.0 PERF-001 report](perf-001-regression-baseline.md) remains historical and is not a comparator for this different native runtime. Initializing this baseline makes no claim about a measured 5.0-to-5.1 performance regression.

These are small, runner-specific workloads. Video covers a three-frame MJPG AVI, rather than camera/network backends; DNN covers the identity model on CPU. `WorkingSet64` is sampled between operations and is not the native allocator high-water mark. Cross-RID performance, large images/models and stable API promotion remain open.

5.1 性能证据绑定当前 Windows x64 Full 包的全部 43 个 DLL、干净源码提交、程序集、固定输入和 runner。四组 harness 各在三个独立进程中执行 100 次，保存全部十二份原始输出，并从中重算十四组中位数指标；证据校验已接入 project invariants 与 release closeout。复用 writer 的托管分配为每 100 次 3,240 bytes，Stream preflight 为 8,296,040 bytes，Typed Mat view 仍有明确的分配开销。5.1 ONNX 新引擎的 profile/memory 诊断变化与上游 target warning 均保留；CPU 推理 checksum 正确不代表 GPU 支持。旧 5.0 报告保留为历史，不能直接作为新 runtime 的性能比较基线；本次建立 5.1 初始基线，不宣称已完成跨版本性能回归评估或跨平台验收。

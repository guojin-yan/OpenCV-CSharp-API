# NativeAOT, Trim, And Single-File Smoke ADR / NativeAOT、裁剪与单文件 Smoke ADR

## Status

Accepted as a 5.0.1 evidence boundary. The repository does not set `IsAotCompatible` or `IsTrimmable` on the managed package from this smoke. The proof is a separate `tools/AotSmoke` consumer.

## Consumer contract

`tools/AotSmoke/AotSmoke.csproj` targets `net10.0`, publishes self-contained for `win-x64` or `win-arm64`, enables `PublishAot`, `PublishTrimmed`, `TrimMode=partial`, and `PublishSingleFile`, and references the built net10.0 managed assembly without changing the package project metadata. The program calls the version/ABI probes and has code paths for `Mat`, ImgProc, and ImgCodecs. With a factual native runtime directory it executes the native path; without one it reports `native_smoke=skipped-native-runtime-missing` and still proves that the published executable starts and returns `AOT_SMOKE_OK`.

Run the bounded guard:

```powershell
pwsh -NoProfile -File .\scripts\Test-NativeAotSmoke.ps1 -RuntimeIdentifier win-x64
```

The guard first builds the managed `net10.0` assembly, enters the Visual Studio `VsDevCmd.bat` environment required by the Windows NativeAOT linker, publishes the single executable, rejects unexpected native payloads when no runtime input was supplied, runs it, and removes the temporary publish directory. Non-Windows hosts report a platform skip because this evidence contract currently uses the Windows linker.

## Current evidence and warning ledger

- `win-x64` publish and executable run passed on the local Windows/.NET 10.0.401 toolchain.
- The executable was a single `AotSmoke.exe`; no native runtime files were copied without an explicit runtime input.
- The managed package reported OpenCV `5.0.0`, ABI `1`, and package version `5.0.0` before the native probe.
- The native probe was explicitly skipped because this checkout has no factual `JYPPX.OpenCV.Native` runtime directory.
- With the factual Windows x64 runtime artifact supplied, the same smoke produced 21 published files (18 native payload files), verified the ABI/OpenCV probes, and encoded a 2x2 image to 71 PNG bytes. The structured record is [`native-aot-smoke-evidence.json`](../../packaging/performance/native-aot-smoke-evidence.json).
- The current-source consumer was rerun against the exact cached full `win-x64` runtime package (`07de65af...2544e`), verified the OpenCV DNN CPU target, and reproduced the 71-byte PNG output. `Measure-NativeAotSmokeEvidence.ps1` records the source commit, managed assembly, package, and all 18 DLL hashes; `Test-NativeAotSmokeEvidence.ps1` cross-checks those payload hashes against the existing native package evidence.
- The AOT analysis warning from `Marshal.SizeOf(typeof(T))` in the pixel traits registry was removed by using the generic `Marshal.SizeOf<T>()` path. Subsequent publish produced no IL3050 warning for that code path.
- The current managed-only `win-x64` consumer run records its compiler and NativeAOT publish diagnostics in [`native-aot-trim-warning-ledger.json`](../../packaging/performance/native-aot-trim-warning-ledger.json). The ledger is generated with `Test-NativeAotSmoke.ps1 -WarningLedgerPath ...`, requires zero unclassified warnings, and is checked by `Test-NativeAotTrimWarningLedger.ps1`.
- A local `win-arm64` attempt stopped before publish because `VsDevCmd.bat -arch=arm64 -host_arch=x64` returned 255; the machine does not have the Visual Studio C++ ARM64 build tools. The guard reports this as an explicit toolchain failure rather than treating a cross-build as ARM64 evidence.
- This record does not claim native runtime execution, ARM64 execution, or stable package-wide AOT/trim compatibility. Those require a matching runtime payload and an independent consumer on each promoted RID/profile.

## Boundaries

NativeAOT does not change the C ABI or managed API. Native loader search, runtime version mismatch, optional module availability, and single-file extraction with a factual native payload remain deployment evidence. A future `win-arm64` run may reuse the same consumer but must record its own SDK, linker, runtime payload, and executable result.

NativeAOT 不改变 C ABI 或 managed API。native loader 查找、runtime 版本不匹配、可选模块可用性以及带真实 native payload 的单文件解压仍需独立部署证据。未来 `win-arm64` 运行必须记录自己的 SDK、linker、runtime payload 和可执行结果。

当前 managed-only `win-x64` consumer 运行会把编译和 NativeAOT publish diagnostics 写入 [`native-aot-trim-warning-ledger.json`](../../packaging/performance/native-aot-trim-warning-ledger.json)。该 ledger 由 `Test-NativeAotSmoke.ps1 -WarningLedgerPath ...` 生成，并由 `Test-NativeAotTrimWarningLedger.ps1` 校验；当前记录要求没有未分类 warning。它不能替代真实 native runtime payload 或 ARM64 硬件证据。

当前源码的真实 `win-x64` AOT native consumer 使用缓存中完整 Full runtime 包重跑，native DNN CPU target 与 2x2 PNG 编码均成功；独立 evidence verifier 绑定 package 和全部 18 个 native DLL 的 hash。它仍不构成 `win-arm64` 证据。

## OpenCV 5.1.0 local Linux package consumers

[`opencv-5.1.0-local-linux-aot-evidence.json`](https://github.com/guojin-yan/OpenCV-CSharp-API/blob/opencv5.x/packaging/runtime/opencv-5.1.0-local-linux-aot-evidence.json) records a separate package-only `tools/LinuxAotPackageSmoke` consumer. Full and Mini restore the existing managed/Linux preview 5.1.0 packages into separate projects and caches in Ubuntu 22.04. SDK 10.0.401 drives NativeAOT with runtime, platform ILCompiler and ILLink 10.0.15; all restore/publish compiler, NuGet, AOT and trim diagnostics are checked. Both publishes completed with zero warnings. The managed package's compatibility flags retain their existing scope.

The producer uses clang 14 and a verified SDK/PowerShell kit. NativeAOT's runtime is supplied by the platform compiler/runtime packs. The record retains the actual restored assets, nine cached archives per profile, signed archive SHA256/SHA512, NuGet signature-normalized content hashes and feed metadata. NuGet content hashes for signed Microsoft packages are distinct from hashes of the complete signed archives; both are validated separately.

Each publish contains one native `LinuxAotConsumer` ELF plus the original package's adjacent OpenCV shared libraries: 53 Full files or 20 Mini files. `PublishSingleFile=true` applies to the managed application; the 52/19 native package files remain adjacent deployment inputs. Direct dependencies of the executable are `libm.so.6`, `libc.so.6` and `ld-linux-x86-64.so.2`; its maximum required GLIBC version is 2.34. The combined OpenCV deployment retains the separately measured 2.35 baseline.

Three independent official-image containers (Ubuntu 22.04, Debian 12, Fedora 44) each run both native executables. They receive a read-only publish/script kit and separate writable volumes, with system runtime libraries only. They have no .NET, PowerShell, compiler, producer directories or native loader overrides. All six executions report both dynamic-code flags false, runtime 10.0.15 and no loaded external CLR/JIT. Loaded OpenCV paths must resolve inside the application's directory and every published file must match the producer and original package.

All six verify exact PNG pixels and a three-frame MJPG AVI roundtrip, including frame shape, pixels and EOF. Full executes two fixed identity ONNX named-input/output inferences per distribution with exact values; Mini actually invokes the importer and verifies its missing entry point. The existing OpenCV 5.1 `setPreferableTarget` graph-engine warning is retained verbatim and narrowly accepted in runtime stderr; it is separate from the zero-warning publish result.

Reproduce with `Initialize-LocalLinux51AotProducer.sh` and `Build-LocalLinux51AotConsumer.ps1` in the pinned producer, then `Initialize-LocalLinux51AotConsumer.sh` and `Invoke-LocalLinux51AotConsumer.sh` in each fresh matching runtime container. Export `/producer/artifacts` and `/consumer/artifacts`, collect with `Measure-LocalLinux51AotEvidence.ps1`, and verify with `Test-LocalLinux51AotEvidence.ps1 -RequireArtifacts`. The guard binds twelve source inputs and rejects 32 negative fixtures. The original Windows AOT and Linux package measurements retain their original bytes and scope. This evidence covers these local x64 consumers; ARM64, hosted execution, arbitrary models/backends and package-wide AOT compatibility remain separate work.

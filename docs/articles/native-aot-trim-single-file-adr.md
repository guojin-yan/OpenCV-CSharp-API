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
- The AOT analysis warning from `Marshal.SizeOf(typeof(T))` in the pixel traits registry was removed by using the generic `Marshal.SizeOf<T>()` path. Subsequent publish produced no IL3050 warning for that code path.
- A local `win-arm64` attempt stopped before publish because `VsDevCmd.bat -arch=arm64 -host_arch=x64` returned 255; the machine does not have the Visual Studio C++ ARM64 build tools. The guard reports this as an explicit toolchain failure rather than treating a cross-build as ARM64 evidence.
- This record does not claim native runtime execution, ARM64 execution, or stable package-wide AOT/trim compatibility. Those require a matching runtime payload and an independent consumer on each promoted RID/profile.

## Boundaries

NativeAOT does not change the C ABI or managed API. Native loader search, runtime version mismatch, optional module availability, and single-file extraction with a factual native payload remain deployment evidence. A future `win-arm64` run may reuse the same consumer but must record its own SDK, linker, runtime payload, and executable result.

NativeAOT 不改变 C ABI 或 managed API。native loader 查找、runtime 版本不匹配、可选模块可用性以及带真实 native payload 的单文件解压仍需独立部署证据。未来 `win-arm64` 运行必须记录自己的 SDK、linker、runtime payload 和可执行结果。

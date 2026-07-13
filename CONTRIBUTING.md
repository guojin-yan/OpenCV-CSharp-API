# Contributing / 贡献指南

Thanks for helping build OpenCV CSharp API.

感谢参与 OpenCV CSharp API 项目。

## Development Rules / 开发规则

- Keep all public managed APIs under the `OpenCvSharp` namespace.
- Keep fixed-major public managed names out of current APIs; `OpenCv5SharpBuildInfo` is the only documented/tested existing-caller compatibility facade.
- Keep samples, docs, and snippets on `OpenCvSharp.*`, `JYPPX.OpenCV.*`, and `OPENCV_CSHARP_*`; fixed-major names in consumer-facing files must be explicitly labelled as compatibility or legacy aliases.
- Keep install commands neutral-first: `dotnet add package JYPPX.OpenCV.CSharp.API --version <four-part-version>` plus the matching `JYPPX.OpenCV.runtime.<rid>` runtime package on the same four-part package version metadata.
- When install snippets show `JYPPX.OpenCV.runtime.win-x64`, label it as the current Windows x64 example and also mention choosing `JYPPX.OpenCV.runtime.<rid>` for the consumer's target RID when available; it is not the only supported runtime package.
- For availability docs, describe the current runtime package matrix from `packaging/runtime/runtime-package-matrix.json`: full packages use `JYPPX.OpenCV.runtime.<rid>`, mini packages use `JYPPX.OpenCV.runtime.<rid>.mini`, and real publishing requires native wrapper plus OpenCV runtime outputs for the selected RID/profile.
- If no matching runtime package is available yet, keep the local native runtime fallback visible: run `Build-OpenCV.ps1`, run `Stage-Runtime.ps1`, use `OpenCvNativeRuntimeDir`, and package with `Pack-Runtime.ps1 -StageRuntime -OpenCvNativeRuntimeDir <runtime-native-dir>` when needed.
- Keep sample/test local runtime copy targets on `OpenCvNativeRuntimeDir`; `OpenCv5SharpNativeRuntimeDir` may appear only as an explicit compatibility alias bridge for existing build scripts, and copy-target dry-runs must use synthetic DLLs outside project `bin`/`obj`.
- Keep runtime package docs cross-linked across README, Quick Start, linked runtime build/smoke guides, smoke profiles, runtime licenses, issue templates, and runtime package README files so users can find package selection, fallback, validation, and license guidance without treating `win-x64` as the only/final package identity.
- Keep package metadata version-neutral: managed package ID and assembly name stay `JYPPX.OpenCV.CSharp.API`, runtime package IDs stay `JYPPX.OpenCV.runtime.<rid>`, and OpenCV runtime identity belongs in package versions.
- Keep release package artifact labels neutral: package output stays under `artifacts/packages`, workflow uploads use `nupkg`, and `.nupkg` filenames are derived from neutral package IDs plus package version metadata.
- Keep managed package artifact dry-runs isolated: use temporary target framework, build output, restore cache, and package output paths, then verify the neutral nuspec, root README, and `lib/net8.0` assembly without creating repo `bin`/`obj` or package outputs.
- Keep standalone managed package consumer dry-runs isolated: restore/build temporary consumers from a local package source with only `JYPPX.OpenCV.CSharp.API`, compile representative managed API references across core and selected module namespaces, and avoid requiring runtime packages or native assets for compile-only usage.
- Keep pack-stage dry-runs isolated: `Pack-Runtime.ps1 -StageRuntime` must forward the selected runtime project directory and `StageOutputRoot` to staging, package from temporary inputs when tested, and avoid creating repo runtime/package mirrors.
- Keep local runtime package consumer dry-runs isolated: restore/build temporary consumers from local package sources and temporary NuGet caches, verify RID native assets are selected, and remove all consumer/package outputs afterwards.
- Keep managed/runtime package-pair dry-runs version-aligned: temporary consumers must reference `JYPPX.OpenCV.CSharp.API` and `JYPPX.OpenCV.runtime.<rid>` with matching four-part package version metadata and isolated build/cache outputs.
- Keep pack workflow release availability honest: `rid=all` and `runtime_profile=all` validate the configured full/mini multi-RID package matrix; synthetic runtime inputs are package-surface validation only and must not be published.
- Keep real runtime pack inputs honest: `pack.yml` does not currently build or download real runtime inputs, real input paths must already exist on the selected runner when `validate_synthetic_runtime=false`, and real publishable runtime packages require `SyntheticRuntimeInputs=false` provenance plus release preflight.
- Keep full-matrix pack workflow artifacts self-validated: `pack.yml` should download current-run `nupkg-*` artifacts and run `Test-GitHubPackArtifactMatrixSurface.ps1` whenever `rid=all` and `runtime_profile=all`.
- Keep full-matrix pack workflow consumer restore validated: after artifact self-validation, `pack.yml` should run `Test-GitHubPackConsumerRestoreSurface.ps1` against the same downloaded artifacts so temporary consumers restore/build the managed package plus every configured full/mini runtime package pair.
- Keep GitHub pack artifact validation reproducible: download successful `pack.yml` artifacts with `gh run download <run-id> -D <artifact-root>`, then run `Test-GitHubPackArtifactMatrixSurface.ps1 -ArtifactRoot <artifact-root>` before treating a full/mini RID matrix run as release evidence.
- Keep DocFX configuration and generated API documentation surfaces on `OpenCvSharp.*`, `src/OpenCvSharp/OpenCvSharp.csproj`, `docs/api`, and `docs/_site`.
- Keep managed, pack, docs, and native CI workflows wired to `scripts/Test-ProjectInvariants.ps1` before restore, build, pack, DocFX, or CMake work begins.
- Keep the `build-managed` workflow native-free: its aggregate invariant step runs the representative managed package consumer compile guard before build outputs exist, and the workflow must not run the full native-dependent `dotnet test` suite unless native runtime assets are explicitly staged.
- Keep new generic build variables, package metadata, and docs version-neutral unless they describe a concrete runtime fact.
- Keep new repository paths version-neutral; only the generated `src/OpenCvSharp.Native/include/open_cv_5_sharp` compatibility include tree may keep a fixed-major path name.
- Keep current native source and examples on `#include "open_cv_sharp/..."`; `#include "open_cv_5_sharp/..."` belongs only to generated compatibility wrappers and the legacy source-compatibility smoke test.
- Runtime packages must remain header-free unless a separately reviewed native header SDK is introduced; document `src/OpenCvSharp.Native/include/open_cv_sharp` as the current source-tree header surface and `src/OpenCvSharp.Native/include/open_cv_5_sharp` only as compatibility.
- Keep the native CMake wrapper source-tree build only unless a public native SDK is deliberately designed and tested; do not add `install(`, `export(`, CMake package config generation, or install-interface include paths. Keep `JYPPX.OpenCV.Native` as the primary CMake target and `OpenCv5Sharp.Native` only as a compatibility alias.
- Keep native CTest and local build-output names neutral-first: use `JYPPX.OpenCV.NativeSmoke`, `JYPPX.OpenCV.NativeCompatibilitySourceSmoke`, and other `JYPPX.OpenCV.Native*` test names, with the `OpenCv5Sharp.Native` loader file only as a compatibility copy.
- Keep native runtime-root/PATH copy neutral-first: use `OPENCV_CSHARP_OPENCV_RUNTIME_ROOT`, put the neutral target output directory first in CTest `PATH`, and treat factual upstream `opencv*.dll` files as copied runtime artifacts rather than project identities.
- Keep fixed-version source/install/cache path text explicitly labelled as factual, upstream, cache, fallback, or compatibility context.
- Keep runtime packages and staging scripts neutral-first: `JYPPX.OpenCV.runtime.<rid>` and `JYPPX.OpenCV.Native.dll` are primary, while `OpenCv5Sharp.Native.dll` is only an explicit compatibility copy.
- Keep C# public API names close to OpenCV C++ names while following .NET naming conventions.
- Do not copy source code from OpenCvSharp, Emgu CV, or other projects.
- Do not use top-level statements in applications, samples, tests, or tools.
- Add bilingual XML documentation for public APIs.
- Keep old framework compatibility and modern .NET fast paths separated with conditional compilation or partial implementations.
- Keep managed P/Invoke declarations on `NativeLibraryNames.CurrentNativeLibrary` with neutral `jyppx_ocv_*` entry points.

## Native ABI / Native 接口

- Export primary C functions with the version-neutral `jyppx_ocv_` prefix.
- Keep the generated `jyppx_ocv5_` forwarding ABI only for already-compiled binaries and existing native source includes.
- Catch all C++ exceptions at the native boundary.
- Use opaque handles for C++ objects.
- Avoid exposing STL types across the C ABI.

## Tests / 测试

Before submitting a pull request, run:

```powershell
pwsh -NoProfile -File .\scripts\Test-ProjectInvariants.ps1
dotnet restore .\OpenCV-CSharp-API.slnx
dotnet build .\OpenCV-CSharp-API.slnx -c Release
dotnet test .\OpenCV-CSharp-API.slnx -c Release --no-build
```

The GitHub `build-managed` workflow intentionally relies on the native-free representative package consumer compile guard inside `scripts/Test-ProjectInvariants.ps1`. Run full `dotnet test` locally or in CI only with matching native runtime assets staged.

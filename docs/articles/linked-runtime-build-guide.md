# Linked Runtime Build Guide

The repository can validate ABI shape without OpenCV, but real image/video/DNN/PtCloud/Quality/XPhoto/ML/ImgHash/XImgProc/OptFlow/BgSegm/Tracking/Face/Saliency/Plot/Shape/LineDescriptor/PhaseUnwrapping/StructuredLight/IntensityTransform/Fuzzy/HFS/Reg/SurfaceMatching/Rapid/AlphaMat/BioInspired/XStereo behavior requires a linked OpenCV runtime. The current packaged OpenCV runtime identity is OpenCV 5.0.0. The default build list includes the main modules used by the managed API and adds optional contrib surfaces when `-WithContrib` is set.

仓库可以在不链接 OpenCV 的情况下验证 ABI 形状，但真实图像、视频、DNN、PtCloud、Quality、XPhoto、ML、ImgHash、XImgProc、OptFlow、BgSegm、Tracking、Face、Saliency、Plot、Shape、LineDescriptor、PhaseUnwrapping、StructuredLight、IntensityTransform、Fuzzy、HFS、Reg、SurfaceMatching、Rapid、AlphaMat、BioInspired 和 XStereo 行为需要 linked OpenCV runtime。当前打包的 OpenCV runtime 身份为 OpenCV 5.0.0。默认 build list 包含 managed API 使用的主线模块，并在设置 `-WithContrib` 时加入可选 contrib 接口面。

## Build And Stage / 构建与暂存

```powershell
pwsh -NoProfile -File ./scripts/Build-OpenCV.ps1 -WithContrib -Build
pwsh -NoProfile -File ./scripts/Stage-Runtime.ps1
```

The scripts accept version-neutral path variables such as `-OpenCvSourceRoot`, `-OpenCvInstallRoot`, and `-OpenCvRuntimeVersionSuffix`. When `-OpenCvSourceRoot` is omitted, the default source root is the version-neutral `opencv-source` workspace directory. Existing local major-version source directories derived from `-OpenCvVersion` are used only when that older checkout path already exists.

脚本接受 `-OpenCvSourceRoot`、`-OpenCvInstallRoot` 和 `-OpenCvRuntimeVersionSuffix` 等版本中立路径变量。省略 `-OpenCvSourceRoot` 时，默认 source root 是 workspace 下版本中立的 `opencv-source` 目录；只有既有本地 major-version 源码目录已经存在时，才会使用由 `-OpenCvVersion` 推导出的旧 checkout 路径。

Use `Build-OpenCV.ps1 -Rid <rid> -DescribeOnly` to inspect the real OpenCV build plan for a runtime package RID before configuring CMake. The plan is derived from `packaging/runtime/runtime-package-matrix.json`: Windows RIDs use Visual Studio multi-config builds and select `x64`, `Win32`, or `ARM64`; Linux RIDs use Ninja single-config builds on the matching Linux host; Android RIDs use Ninja plus the Android NDK toolchain and require `-AndroidNdkRoot` for real configuration. This build-plan evidence is separate from synthetic package-shape validation: synthetic CI proves package layout, while real release candidates still require actual native wrapper and OpenCV runtime inputs.

请使用 `Build-OpenCV.ps1 -Rid <rid> -DescribeOnly` 在配置 CMake 前检查某个 runtime package RID 的真实 OpenCV build plan。该计划来自 `packaging/runtime/runtime-package-matrix.json`：Windows RID 使用 Visual Studio multi-config 构建并选择 `x64`、`Win32` 或 `ARM64`；Linux RID 在匹配的 Linux host 上使用 Ninja single-config 构建；Android RID 使用 Ninja 与 Android NDK toolchain，真实配置时必须提供 `-AndroidNdkRoot`。这个 build-plan 证据与 synthetic package-shape validation 分离：synthetic CI 证明包布局，真实 release candidate 仍必须提供实际 native wrapper 与 OpenCV runtime 输入。

Linux NuGet package IDs should stay on portable .NET glibc RIDs such as `JYPPX.OpenCV.runtime.linux-x64`; do not encode Ubuntu, Debian, Fedora, or RHEL release names into the package identity unless the project deliberately adds a custom RID graph. Distribution coverage belongs in the build/smoke validation matrix: build from an agreed baseline, run linked smoke on common glibc distributions, and publish only after provenance plus distro smoke are both green. Alpine and other musl-based systems are not covered by `linux-x64`; they require separate `linux-musl-*` runtime packages.

Linux NuGet package ID 应保持 .NET portable glibc RID，例如 `JYPPX.OpenCV.runtime.linux-x64`；除非项目明确引入 custom RID graph，否则不要把 Ubuntu、Debian、Fedora 或 RHEL release 名写入 package identity。发行版覆盖应放在 build/smoke validation matrix：从约定 baseline 构建，在常见 glibc 发行版上跑 linked smoke，且 provenance 与 distro smoke 都通过后再发布。Alpine 等 musl-based 系统不属于 `linux-x64` 覆盖范围，需要单独的 `linux-musl-*` runtime 包。

Runtime package template project: `packaging/runtime/JYPPX.OpenCV.runtime`. Full packages use `JYPPX.OpenCV.runtime.<rid>` and mini packages use `JYPPX.OpenCV.runtime.<rid>.mini` for the configured runtime package matrix. If no matching published runtime package is available yet, this guide is the local native runtime fallback: run `Build-OpenCV.ps1`, run `Stage-Runtime.ps1`, and pass the staged native output through `OpenCvNativeRuntimeDir` or `Pack-Runtime.ps1 -StageRuntime -OpenCvNativeRuntimeDir <runtime-native-dir>`.

runtime package 模板项目为 `packaging/runtime/JYPPX.OpenCV.runtime`。full package 使用 `JYPPX.OpenCV.runtime.<rid>`，mini package 使用 `JYPPX.OpenCV.runtime.<rid>.mini`，并覆盖配置好的 runtime matrix。如果 no matching published runtime package is available yet，本指南就是 local native runtime fallback：运行 `Build-OpenCV.ps1`，运行 `Stage-Runtime.ps1`，并通过 `OpenCvNativeRuntimeDir` 或 `Pack-Runtime.ps1 -StageRuntime -OpenCvNativeRuntimeDir <runtime-native-dir>` 使用暂存的 native 输出。

## Pack Runtime / 打包 Runtime

If runtime files are already staged in the runtime package project, pack them directly:

```powershell
pwsh -NoProfile -File ./scripts/Pack-Runtime.ps1 -Rid win-x64 -OpenCvVersion 5.0.0 -PackageRevision 0
```

Runtime package `.csproj` files and package README files are trackable metadata. The runtime project `runtimes/`, `licenses/`, and `build/` directories are ignored generated mirrors; `Stage-Runtime.ps1` regenerates them from current runtime inputs before packaging, and those mirror contents should not be committed or manually edited.

runtime package 的 `.csproj` 和包 README 是可跟踪元数据。runtime project 下的 `runtimes/`、`licenses/` 与 `build/` 目录是被忽略的生成镜像；`Stage-Runtime.ps1` 会在打包前根据当前 runtime 输入重新生成它们，不应提交或手动编辑这些镜像内容。

`Pack-Runtime.ps1 -Rid <rid>` derives full runtime package IDs as `JYPPX.OpenCV.runtime.<rid>`; `-RuntimeProfile mini` derives mini package IDs as `JYPPX.OpenCV.runtime.<rid>.mini`. The script forwards `RuntimePackageRid` and `RuntimePackageProfile` into the generic runtime package project.

`Stage-Runtime.ps1 -RuntimeProject` is a repository-relative or absolute runtime package directory used for generated `runtimes/<rid>/native` and `licenses/` mirrors. `Pack-Runtime.ps1 -RuntimeProject` is a repository-relative or absolute runtime package `.csproj` file path used by `dotnet pack`. When `Pack-Runtime.ps1 -StageRuntime` is used, the selected `.csproj` directory is forwarded to `Stage-Runtime.ps1`; use `-StageOutputRoot` to forward a separate staging root without changing package `-OutputDir`.

The pack workflow exposes `rid` and `runtime_profile` inputs. `all` runs the configured full/mini multi-RID matrix; targeted RID/profile packaging is supported. The workflow uploads neutral `nupkg-*` artifacts from `artifacts/packages`, self-validates the full matrix artifacts with `Test-GitHubPackArtifactMatrixSurface.ps1`, verifies consumer restore/build behavior with `Test-GitHubPackConsumerRestoreSurface.ps1`, validates package shape with synthetic runtime inputs by default, rejects publishing when synthetic inputs are enabled, and passes `-RequireReleasePreflight` to `Pack-Runtime.ps1` before any publish-capable runtime package push.

`pack.yml` does not build real runtime inputs. When `validate_synthetic_runtime=false`, real input paths must already exist on the selected runner or come from `real_runtime_artifact_run_id`; that run must expose a neutral `runtime-input-<rid>-<profile>` artifact. The artifact layout is `native-wrapper/` for `JYPPX.OpenCV.Native`, `opencv-runtime/` for OpenCV runtime binaries, `opencv-source/` for source/license evidence, and optional `opencv-install/` for install-root license mirrors. Synthetic runtime inputs are package-surface validation only; real publishable runtime packages require `SyntheticRuntimeInputs=false` provenance and release preflight. The next real-GitHub build step should produce that artifact before disabling synthetic validation for publish-capable packages.

`runtime-input.yml` is the first real producer workflow for this handoff. It currently produces only `runtime-input-linux-x64-full`: the job fetches factual OpenCV source, builds OpenCV with the full profile build list, links/tests `JYPPX.OpenCV.Native`, and uploads `runtime-input-<rid>-<profile>` with `native-wrapper/`, `opencv-runtime/`, `opencv-source/`, and optional `opencv-install/`. Mini and additional RID producers remain intentionally disabled until their linked-component boundaries are implemented and verified. After a successful producer run, pass its run id to `pack.yml` through `real_runtime_artifact_run_id` with `validate_synthetic_runtime=false` and `publish_github_packages=false`.

`Test-RuntimeReleaseCandidatePreflight.ps1` validates release-candidate runtime staging before packing/publishing. It checks `build/JYPPX.OpenCV.runtime.provenance.json`, rejects `SyntheticRuntimeInputs=true` unless explicitly allowed for non-release diagnostics, verifies package ID/version, OpenCV version/RID, runtime profile modules, native loader names, runtime file source paths, license evidence, and ensures generated `runtimes/<rid>/native`, `licenses/`, and `build/` mirrors match provenance without stale files. `Test-RuntimeReleaseCandidatePreflightGuard.ps1` also runs `Pack-Runtime.ps1 -StageRuntime -RequireReleasePreflight` in an isolated temporary runtime package project, verifies the resulting package, and proves `-SyntheticRuntimeInputs -RequireReleasePreflight` does not produce a package.

After a successful GitHub `pack.yml` run, download the workflow artifacts and validate the package matrix offline before using the run as release evidence:

```powershell
gh run download <run-id> -D <artifact-root>
pwsh -NoProfile -File ./scripts/Test-GitHubPackArtifactMatrixSurface.ps1 -ArtifactRoot <artifact-root>
pwsh -NoProfile -File ./scripts/Test-GitHubPackConsumerRestoreSurface.ps1 -ArtifactRoot <artifact-root>
```

The artifact guard checks the managed package plus every configured full/mini runtime RID package, including neutral package IDs, normalized package filenames, `runtimes/<rid>/native` payload paths, full versus mini module counts, and `build/JYPPX.OpenCV.runtime.provenance.json`. The provenance manifest records package ID/version, OpenCV version, RID/profile, loader names, required and optional module evidence, runtime and license file sources, input/output roots, and `SyntheticRuntimeInputs`; synthetic validation manifests are non-release evidence, while real release candidates must be marked `SyntheticRuntimeInputs=false`. The consumer restore guard uses the downloaded packages as a temporary local NuGet source, restores/builds temporary consumers for every configured RID/profile pair, and verifies managed compile assets plus selected RID native assets without executing native binaries. Keep downloaded artifacts outside the repository or remove them after validation.

`Pack-Runtime.ps1 -Rid <rid>` 会把 full runtime package ID 推导为 `JYPPX.OpenCV.runtime.<rid>`；`-RuntimeProfile mini` 会推导 mini package ID `JYPPX.OpenCV.runtime.<rid>.mini`。脚本会把 `RuntimePackageRid` 和 `RuntimePackageProfile` 转发给通用 runtime package project。

`Stage-Runtime.ps1 -RuntimeProject` 是仓库相对或绝对 runtime package 目录，用于生成 `runtimes/<rid>/native` 和 `licenses/` 镜像；`Pack-Runtime.ps1 -RuntimeProject` 是供 `dotnet pack` 使用的仓库相对或绝对 runtime package `.csproj` 文件路径。使用 `Pack-Runtime.ps1 -StageRuntime` 时，所选 `.csproj` 所在目录会转发给 `Stage-Runtime.ps1`；可用 `-StageOutputRoot` 转发单独的 staging root，且不改变 package `-OutputDir`。

pack workflow 暴露 `rid` 和 `runtime_profile` 输入。`all` 会运行配置好的 full/mini 多 RID 矩阵；也支持定向 RID/profile 打包。workflow 从 `artifacts/packages` 上传中性的 `nupkg-*` 产物，先用 `Test-GitHubPackArtifactMatrixSurface.ps1` 自检 full matrix artifacts，再用 `Test-GitHubPackConsumerRestoreSurface.ps1` 验证 consumer restore/build；默认用 synthetic runtime inputs 验证 package shape，并在启用 synthetic inputs 时拒绝发布。

`pack.yml` 当前不会构建真实 runtime 输入。当 `validate_synthetic_runtime=false` 时，真实输入路径必须已经存在于 selected runner，或来自 `real_runtime_artifact_run_id`；该 run 必须暴露中性的 `runtime-input-<rid>-<profile>` artifact。artifact layout 为：`native-wrapper/` 存放 `JYPPX.OpenCV.Native`，`opencv-runtime/` 存放 OpenCV runtime binaries，`opencv-source/` 存放源码/license evidence，`opencv-install/` 可选用于 install-root license mirrors。synthetic runtime inputs 只用于 package-surface validation；真实可发布 runtime 包必须带有 `SyntheticRuntimeInputs=false` provenance 并通过 release preflight。下一步真实 GitHub build step 应在对 publish-capable package 关闭 synthetic validation 前生成该 artifact。

`runtime-input.yml` 是这条 handoff 的第一条真实 producer workflow。当前它只生产 `runtime-input-linux-x64-full`：job 会获取事实性 OpenCV 源码，按 full profile build list 构建 OpenCV，链接/测试 `JYPPX.OpenCV.Native`，并上传带有 `native-wrapper/`、`opencv-runtime/`、`opencv-source/` 与可选 `opencv-install/` 的 `runtime-input-<rid>-<profile>`。mini 和其它 RID producer 会保持明确禁用，直到对应 linked-component 边界完成并通过验证。producer run 成功后，把它的 run id 通过 `real_runtime_artifact_run_id` 传给 `pack.yml`，并使用 `validate_synthetic_runtime=false` 与 `publish_github_packages=false`。

artifact guard 会检查 managed package 以及每个已配置 full/mini runtime RID 包，包括中性 package ID、规范化 package 文件名、`runtimes/<rid>/native` payload 路径、full 与 mini module count，以及 `build/JYPPX.OpenCV.runtime.provenance.json`。provenance manifest 会记录 package ID/version、OpenCV version、RID/profile、loader 名称、required/optional module 证据、runtime 和 license 文件来源、输入/输出根目录，以及 `SyntheticRuntimeInputs`；synthetic validation manifest 不是发布证据，真实 release candidate 必须标记为 `SyntheticRuntimeInputs=false`。`Test-RuntimeReleaseCandidatePreflight.ps1` 会在发布前校验这些字段，并确保 generated `runtimes/<rid>/native`、`licenses/` 与 `build/` 镜像和 provenance 完全一致、没有陈旧文件。`Test-RuntimeReleaseCandidatePreflightGuard.ps1` 还会在隔离 temporary runtime package project 中运行 `Pack-Runtime.ps1 -StageRuntime -RequireReleasePreflight`，校验生成的 package，并证明 `-SyntheticRuntimeInputs -RequireReleasePreflight` 不会产出 package。

To stage fresh runtime files and then pack in one command, use `-StageRuntime` and provide the native wrapper output directory:

```powershell
pwsh -NoProfile -File ./scripts/Pack-Runtime.ps1 -Rid win-x64 -OpenCvVersion 5.0.0 -PackageRevision 0 -StageRuntime -OpenCvNativeRuntimeDir ./build/native-opencv-core/Release
```

The `./build/native-opencv-core/Release` path in the example is the current local native wrapper build-output location, not a package identity or a new generic naming pattern.

`Pack-Runtime.ps1` derives the four-part package version from `-OpenCvVersion` plus `-PackageRevision`; `-PackageVersion` remains accepted only as an explicit version-metadata compatibility override, not as a package identity or naming surface. The script uses `-OpenCvNativeRuntimeDir` as the preferred version-neutral native wrapper runtime path/staging parameter; the older `-NativeRuntimeDir` remains accepted only as an existing-packaging-script compatibility alias. It forwards version-neutral OpenCV path parameters such as `-OpenCvRuntimeDir`, `-OpenCvInstallDir`, `-OpenCvSourceDir`, `-OpenCvSourceRoot`, `-OpenCvInstallRoot`, and `-OpenCvRuntimeVersionSuffix` to `Stage-Runtime.ps1` when `-StageRuntime` is set.

`-RuntimeProject` accepts repository-relative or absolute runtime project paths. An absolute `-RuntimeProject` is accepted as-is and can point outside the repository when the caller chooses that layout.

`-OutputDir` accepts repository-relative or absolute package output directories. An absolute `-OutputDir` is accepted as-is and can write package artifacts outside the repository when the caller chooses that path.

The managed and runtime package IDs stay version-neutral; the default package output remains `artifacts\packages`, and release checks treat normalized `.nupkg` files in that directory as the package artifacts.

示例中的 `./build/native-opencv-core/Release` 路径只是当前本地 native wrapper build-output 位置，不是 package 身份或新的通用命名模式。

如果 runtime package project 中已经存在暂存文件，可以直接打包。若要先暂存最新 runtime 文件再打包，请使用 `-StageRuntime` 并提供 native wrapper 输出目录。`Pack-Runtime.ps1` 会从 `-OpenCvVersion` 和 `-PackageRevision` 推导四段 package version；`-PackageVersion` 仍仅作为版本元数据的显式兼容覆盖口接受，不作为包身份或命名面。脚本使用 `-OpenCvNativeRuntimeDir` 作为首选版本中立 native wrapper runtime path/staging 参数；旧的 `-NativeRuntimeDir` 仍仅作为既有 packaging script 的兼容别名接受。设置 `-StageRuntime` 时，`Pack-Runtime.ps1` 会把 `-OpenCvRuntimeDir`、`-OpenCvInstallDir`、`-OpenCvSourceDir`、`-OpenCvSourceRoot`、`-OpenCvInstallRoot` 和 `-OpenCvRuntimeVersionSuffix` 等版本中立 OpenCV 路径参数转发给 `Stage-Runtime.ps1`。

`-RuntimeProject` 可以使用仓库相对或绝对 runtime 项目路径。绝对 `-RuntimeProject` 会按原样接受；如果调用方选择仓库外项目路径，也可以指向仓库外。

`-OutputDir` 可以使用仓库相对或绝对包输出目录。绝对 `-OutputDir` 会按原样接受；如果调用方选择仓库外路径，包产物也会写到仓库外。

managed 与 runtime package IDs 保持版本中立；默认包输出目录保持 `artifacts\packages`，release 检查把该目录中规范化后的 `.nupkg` 文件视为包产物证据。

The runtime package ID stays version-neutral; the OpenCV runtime identity is expressed through package version metadata derived from `-OpenCvVersion` plus `-PackageRevision`. The pack scripts keep that package version metadata contract while checking the normalized `.nupkg` artifact file name that NuGet writes, for example `5.0.0.0` becomes `5.0.0` in the package file name.

Runtime package ID 保持版本中立；OpenCV runtime 身份通过由 `-OpenCvVersion` 加 `-PackageRevision` 推导出的 package version 元数据表达。打包脚本保持该 package version 元数据契约，同时检查 NuGet 实际写出的规范化 `.nupkg` 产物文件名；例如 `5.0.0.0` 在包文件名中会变成 `5.0.0`。

Use the managed package `JYPPX.OpenCV.CSharp.API` and the matching runtime package `JYPPX.OpenCV.runtime.<rid>` together on the same four-part package version metadata so their OpenCV runtime identity stays aligned.

managed 主包 `JYPPX.OpenCV.CSharp.API` 和匹配的 runtime 包 `JYPPX.OpenCV.runtime.<rid>` 应使用相同的四段 package version 元数据，以保持 OpenCV runtime 身份一致。

Start with the [Quick Start](quick-start.md) for consumer install commands. After building or staging a local runtime, validate it with the [Linked Runtime Smoke Guide](linked-runtime-smoke-guide.md) and [Smoke Profiles Guide](smoke-profiles-guide.md). Runtime package license layout is covered by [Runtime Licenses](runtime-licenses.md) and the [runtime package README](../../packaging/runtime/JYPPX.OpenCV.runtime/README.md).

consumer 安装命令先看 [Quick Start](quick-start.md)。构建或暂存 local runtime 后，请用 [Linked Runtime Smoke Guide](linked-runtime-smoke-guide.md) 和 [Smoke Profiles Guide](smoke-profiles-guide.md) 验证。runtime package license 布局见 [Runtime Licenses](runtime-licenses.md) 和[runtime package README](../../packaging/runtime/JYPPX.OpenCV.runtime/README.md)。

Before packing, and before invoking `dotnet pack`, the scripts remove the expected normalized package file from the output directory when it already exists. This avoids accepting stale package artifacts from earlier runs as release evidence.

在调用 `dotnet pack` 前，脚本会删除输出目录中已存在的预期规范化包文件，避免把早先运行遗留的旧包误当成本轮发布证据。

The optional `-NoBuild` and `-NoRestore` switches only forward `--no-build` and `--no-restore` to `dotnet pack`; stale artifact removal and normalized `.nupkg` verification still run.

可选的 `-NoBuild` 与 `-NoRestore` 开关只会把 `--no-build` 和 `--no-restore` 转发给 `dotnet pack`；旧产物删除和规范化 `.nupkg` 验证仍会执行。

The default main-module list is:

默认主模块列表为：

```text
core,imgproc,imgcodecs,videoio,flann,geometry,calib,stereo,dnn,objdetect,photo,features,video,highgui,stitching,ptcloud
```

The default build list includes main modules used by the current wrapper, including `ptcloud`.

默认 build list 包含当前 wrapper 使用的主线模块，包括 `ptcloud`。

With contrib enabled, the build also requests `xfeatures2d`, `xobjdetect`, `quality`, `xphoto`, `ml`, `img_hash`, `ximgproc`, `optflow`, `bgsegm`, `tracking`, `face`, `saliency`, `plot`, `shape`, `line_descriptor`, `phase_unwrapping`, `structured_light`, `intensity_transform`, `fuzzy`, `hfs`, `reg`, `surface_matching`, `rapid`, `alphamat`, `bioinspired`, and `xstereo`. In the local OpenCV 5.0.0 source layout, these optional modules are under contrib, and `optflow` can also use the staged `ximgproc` module for related algorithms. `FastBilateralSolverFilter` and `IntensityTransformCv2.Bimef` call paths additionally depend on OpenCV being built with EIGEN support.

启用 contrib 时，构建还会请求 `xfeatures2d`、`xobjdetect`、`quality`、`xphoto`、`ml`、`img_hash`、`ximgproc`、`optflow`、`bgsegm`、`tracking`、`face`、`saliency`、`plot`、`shape`、`line_descriptor`、`phase_unwrapping`、`structured_light`、`intensity_transform`、`fuzzy`、`hfs`、`reg`、`surface_matching`、`rapid`、`alphamat`、`bioinspired` 和 `xstereo`。在当前本地 OpenCV 5.0.0 源码布局中，这些可选模块位于 contrib，且 `optflow` 的相关算法也可以使用已暂存的 `ximgproc` 模块。`FastBilateralSolverFilter` 与 `IntensityTransformCv2.Bimef` 调用路径还取决于 OpenCV 是否启用 EIGEN 支持。

## Linked Native Smoke / Linked Native 验证

```powershell
cmake -S ./src/OpenCvSharp.Native -B ./build/native-codex-opencv -DOPENCV_CSHARP_OPENCV_DIR=<OpenCVConfig.cmake directory>
cmake --build ./build/native-codex-opencv --config Release
cmake --build ./build/native-codex-opencv --target RUN_TESTS --config Release
```

`Build-OpenCV.ps1` prints the expected `OpenCVConfig.cmake` directory and staged `bin` directory after configuration/build. Use that config directory for the version-neutral `OPENCV_CSHARP_OPENCV_DIR` CMake variable. The older `OPENCV5SHARP_OPENCV_DIR` name remains accepted only as an existing-build-script compatibility alias.

`Build-OpenCV.ps1` 会在配置/构建后打印预期 `OpenCVConfig.cmake` 目录和暂存 `bin` 目录。版本中立的 CMake 变量 `OPENCV_CSHARP_OPENCV_DIR` 应指向该 config 目录。旧的 `OPENCV5SHARP_OPENCV_DIR` 名称仍仅作为既有构建脚本的兼容别名接受。

## Required DLL Highlights / 关键 DLL

The `500` suffix in the DLL names below is the upstream OpenCV 5.0.0 binary naming fact for the current packaged runtime identity, not a project naming pattern for new generic APIs.

下列 DLL 名中的 `500` 后缀是当前打包 runtime 身份所使用的 OpenCV 5.0.0 上游二进制命名事实，不是新增通用 API 的项目命名模式。

- factual OpenCV 5.0.0 runtime artifact `opencv_video500.dll`: `OpenCvSharp.Video` optical flow, mean-shift/CamShift, and `KalmanFilter`.
- factual OpenCV 5.0.0 runtime artifact `opencv_dnn500.dll`: `OpenCvSharp.Dnn.Net`, blob helpers, model loading, and forward passes.
- factual OpenCV 5.0.0 runtime artifact `opencv_highgui500.dll`: `OpenCvSharp.HighGui` window and key helpers.
- factual OpenCV 5.0.0 runtime artifact `opencv_videoio500.dll`: capture/writer and backend registry.
- factual OpenCV 5.0.0 runtime artifact `opencv_objdetect500.dll`: QR, barcode, ArUco, ChArUco, MCC, and face wrappers.
- factual OpenCV 5.0.0 runtime artifact `opencv_photo500.dll`: photo denoise, editing, and tonemap wrappers.
- factual OpenCV 5.0.0 runtime artifact `opencv_ptcloud500.dll`: depth/RGB-D helper surface.
- factual OpenCV 5.0.0 runtime artifact `opencv_ml500.dll`: `OpenCvSharp.ML` and model support used by contrib quality BRISQUE.
- factual OpenCV 5.0.0 runtime artifact `opencv_img_hash500.dll`: `OpenCvSharp.ImgHash` perceptual hash objects and one-shot helpers.
- factual OpenCV 5.0.0 runtime artifact `opencv_ximgproc500.dll`: `OpenCvSharp.XImgProc` local thresholding, edge-aware filters, superpixels, fast line detection, disparity WLS helpers, sparse interpolation, EdgeDrawing, EdgeBoxes, ridge/gradient utilities, Fourier descriptors, run-length morphology, ScanSegment, GraphSegmentation, Selective Search, and covariance estimation.
- factual OpenCV 5.0.0 runtime artifact `opencv_optflow500.dll`: `OpenCvSharp.OptFlow` dense/sparse flow, RLOF, SimpleFlow/SparseToDense, and motion-template helpers.
- factual OpenCV 5.0.0 runtime artifact `opencv_bgsegm500.dll`: `OpenCvSharp.BgSegm` MOG/GMG/CNT background subtractors and synthetic sequence generator.
- factual OpenCV 5.0.0 runtime artifact `opencv_tracking500.dll`: `OpenCvSharp.Tracking` modern KCF/CSRT, legacy MOSSE/MIL/MedianFlow, and `MultiTracker`.
- factual OpenCV 5.0.0 runtime artifact `opencv_face500.dll`: `OpenCvSharp.Face` traditional Eigen/Fisher/LBPH recognizers, `StandardCollector`, `BIF`, `FacemarkLBF`, and `MACE`.
- factual OpenCV 5.0.0 runtime artifact `opencv_saliency500.dll`: `OpenCvSharp.Saliency` static spectral-residual/fine-grained saliency, BinWang motion saliency, and `ObjectnessBING`.
- factual OpenCV 5.0.0 runtime artifact `opencv_plot500.dll`: `OpenCvSharp.Plot` `Plot2d` render paths.
- factual OpenCV 5.0.0 runtime artifact `opencv_shape500.dll`: `OpenCvSharp.Shape` `EMDL1`, histogram cost extractors, and shape distance extractors.
- factual OpenCV 5.0.0 runtime artifact `opencv_line_descriptor500.dll`: `OpenCvSharp.LineDescriptor` binary descriptor, matcher, and drawing helper paths.
- factual OpenCV 5.0.0 runtime artifact `opencv_phase_unwrapping500.dll`: `OpenCvSharp.PhaseUnwrapping` histogram phase-unwrapping wrappers.
- factual OpenCV 5.0.0 runtime artifact `opencv_structured_light500.dll`: `OpenCvSharp.StructuredLight` Gray-code and sinusoidal structured-light wrappers.
- factual OpenCV 5.0.0 runtime artifact `opencv_intensity_transform500.dll`: `OpenCvSharp.IntensityTransform` log/gamma/autoscale/contrast/BIMEF wrappers; BIMEF also needs EIGEN support.
- factual OpenCV 5.0.0 runtime artifact `opencv_fuzzy500.dll`: `OpenCvSharp.Fuzzy` kernel, inpaint/filter, and F-transform helpers.
- factual OpenCV 5.0.0 runtime artifact `opencv_hfs500.dll`: `OpenCvSharp.Hfs` HFS segmentation wrappers.
- factual OpenCV 5.0.0 runtime artifact `opencv_reg500.dll`: `OpenCvSharp.Reg` registration map and mapper wrappers.
- factual OpenCV 5.0.0 runtime artifact `opencv_surface_matching500.dll`: `OpenCvSharp.SurfaceMatching` ICP and PPF 3D detector wrappers.
- factual OpenCV 5.0.0 runtime artifact `opencv_rapid500.dll`: `OpenCvSharp.Rapid` RAPID helper and tracker wrappers.
- factual OpenCV 5.0.0 runtime artifact `opencv_alphamat500.dll`: `OpenCvSharp.AlphaMat` information-flow alpha matting wrapper.
- factual OpenCV 5.0.0 runtime artifact `opencv_bioinspired500.dll`: `OpenCvSharp.BioInspired` Retina, fast tone mapping, and transient segmentation wrappers.
- factual OpenCV 5.0.0 runtime artifact `opencv_xstereo500.dll`: `OpenCvSharp.XStereo` census, binary stereo, and quasi-dense stereo wrappers.
- factual OpenCV 5.0.0 runtime artifacts `opencv_xfeatures2d500.dll`, `opencv_xobjdetect500.dll`, `opencv_quality500.dll`, and `opencv_xphoto500.dll`: optional contrib surfaces.

- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_video500.dll`：`OpenCvSharp.Video` 光流、`.flo` 光流文件读写、mean-shift/CamShift 和 `KalmanFilter`。
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_dnn500.dll`：`OpenCvSharp.Dnn.Net`、blob 辅助、模型加载和 forward。
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_highgui500.dll`：`OpenCvSharp.HighGui` 窗口和按键辅助。
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_videoio500.dll`：capture/writer 和 backend registry。
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_objdetect500.dll`：QR、条形码、ArUco、ChArUco、MCC 和 face wrapper。
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_photo500.dll`：photo 去噪、editing 和 tonemap wrapper。
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_ptcloud500.dll`：depth/RGB-D helper 接口面。
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_ml500.dll`：`OpenCvSharp.ML` 以及 contrib quality BRISQUE 使用的模型支持模块。
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_img_hash500.dll`：`OpenCvSharp.ImgHash` 感知哈希对象和一次性 helper。
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_ximgproc500.dll`：`OpenCvSharp.XImgProc` 局部阈值、edge-aware filter、超像素、快速线段检测、disparity WLS helper、稀疏插值、EdgeDrawing、EdgeBoxes、ridge/gradient 工具、Fourier descriptor、run-length morphology、ScanSegment、GraphSegmentation、Selective Search 和 covariance estimation。
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_optflow500.dll`：`OpenCvSharp.OptFlow` dense/sparse 光流、RLOF、SimpleFlow/SparseToDense 和 motion-template helper。
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_bgsegm500.dll`：`OpenCvSharp.BgSegm` MOG/GMG/CNT 背景减除器和 synthetic sequence generator。
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_tracking500.dll`：`OpenCvSharp.Tracking` modern KCF/CSRT、legacy MOSSE/MIL/MedianFlow 和 `MultiTracker`。
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_face500.dll`：`OpenCvSharp.Face` 传统 Eigen/Fisher/LBPH 识别器、`StandardCollector`、`BIF`、`FacemarkLBF` 和 `MACE`。
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_saliency500.dll`：`OpenCvSharp.Saliency` 静态 spectral-residual/fine-grained 显著性、BinWang 运动显著性和 `ObjectnessBING`。
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_plot500.dll`：`OpenCvSharp.Plot` 的 `Plot2d` 渲染路径。
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_shape500.dll`：`OpenCvSharp.Shape` 的 `EMDL1`、直方图代价提取器和形状距离提取器。
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_line_descriptor500.dll`：`OpenCvSharp.LineDescriptor` 二进制描述子、匹配器和绘图 helper 路径。
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_phase_unwrapping500.dll`：`OpenCvSharp.PhaseUnwrapping` histogram phase-unwrapping wrapper。
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_structured_light500.dll`：`OpenCvSharp.StructuredLight` Gray-code 和正弦 structured-light wrapper。
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_intensity_transform500.dll`：`OpenCvSharp.IntensityTransform` log/gamma/autoscale/contrast/BIMEF wrapper；BIMEF 还需要 EIGEN 支持。
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_fuzzy500.dll`：`OpenCvSharp.Fuzzy` kernel、inpaint/filter 与 F-transform helper。
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_hfs500.dll`：`OpenCvSharp.Hfs` HFS 分割 wrapper。
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_reg500.dll`：`OpenCvSharp.Reg` registration map 与 mapper wrapper。
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_surface_matching500.dll`：`OpenCvSharp.SurfaceMatching` ICP 与 PPF 3D detector wrapper。
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_rapid500.dll`：`OpenCvSharp.Rapid` RAPID helper 与 tracker wrapper。
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_alphamat500.dll`：`OpenCvSharp.AlphaMat` 信息流 alpha matting wrapper。
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_bioinspired500.dll`：`OpenCvSharp.BioInspired` Retina、fast tone mapping 和 transient segmentation wrapper。
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_xstereo500.dll`：`OpenCvSharp.XStereo` census、binary stereo 和 quasi-dense stereo wrapper。
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_xfeatures2d500.dll`、`opencv_xobjdetect500.dll`、`opencv_quality500.dll` 与 `opencv_xphoto500.dll`：可选 contrib 接口面。

## Failure Records / 失败记录

If a full OpenCV build takes too long or fails, record the exact command, failing phase, visible DLLs, and missing DLLs in the diary. Keep the no-OpenCV configure/build/test path as the authoritative ABI validation for that round.

如果完整 OpenCV 构建耗时过长或失败，应在开发日记中记录确切命令、失败阶段、可见 DLL 和缺失 DLL。该轮仍以 no-OpenCV configure/build/test 路径作为权威 ABI 验证。

# Headless Runtime Profile ADR

Status: local OpenCV 5.1.0 Full/Mini candidates verified on three x64 distributions; candidate-only and not publishable.

## Decision

The repository will model a headless runtime as an isolated profile contract before adding a NuGet identity to the active runtime matrix. The proposed identity is `JYPPX.OpenCV.runtime.<rid>.headless`; it must remain outside `runtime-package-matrix.json` until the producer/package/consumer gates below have completed. The existing `full` and `mini` package identities are unchanged.

Headless is a packaging and runtime-behaviour contract, not a promise that every OpenCV backend is available without a display server. The first candidate is based on the existing full module set with `highgui` removed. `core`, `imgproc`, `imgcodecs`, `videoio`, `flann`, `geometry`, `calib`, `stereo`, `dnn`, `ml`, `objdetect`, `photo`, `features`, `video`, `stitching`, and `ptcloud` remain candidates, subject to the dependency and smoke gates. Optional contrib modules remain optional and do not silently widen the profile.

## ABI and managed API boundary

The headless profile adds no native entrypoints and changes no managed public type or method signatures. Existing HighGui methods remain callable at the managed compatibility surface, but when the profile omits the native HighGui module they must fail deterministically through the repository's existing `NOT_LINKED` path. A headless package must not ship a second loader, a profile-specific managed assembly, or a compatibility shim that dynamically loads GUI libraries.

The profile's native ABI manifest must be generated from the profile's selected wrapper sources. Its ABI count is therefore evidence, not a hand-maintained constant. Any difference from the manifest is a promotion blocker; the profile cannot reuse a full-profile manifest by assertion.

## Dependency policy

The candidate must be usable on a machine with no X11 display and no GTK, Qt, Cocoa, or Win32 GUI development/runtime libraries installed. ELF/PE dependency closure must contain only the selected OpenCV modules, the neutral loader, the C/C++ runtime, libc, the bundled image codecs/protobuf policy already used by the matching base profile, and explicitly approved system libraries. `libgtk*`, `libgdk*`, `libQt*`, `libX11*`, `libXext*`, `libwayland*`, and display-server sockets are forbidden unless a future profile explicitly opts into them.

`videoio` is retained as a separate risk boundary: camera and network backends can pull platform services even when HighGui is absent. A headless promotion therefore requires an explicit backend result (available, unavailable, or disabled) for every tested distro. It is not sufficient to prove that `imread` and `imwrite` work.

## Required negative behaviour

The candidate consumer must run with `DISPLAY` and `WAYLAND_DISPLAY` unset and with no GUI packages installed. It must also run without `LD_LIBRARY_PATH` or `OPENCV_CSHARP_OPENCV_RUNTIME_ROOT`; those variables are useful for local linked-runtime development but are forbidden as evidence shortcuts for the headless package. It must prove all of the following:

- `Cv2.ImShow`, `NamedWindow`, `DestroyWindow`, and trackbar/window APIs take the deterministic `NOT_LINKED`/unsupported path and do not hang.
- Image codec operations still execute and preserve the existing codec preflight/size-limit policy.
- A VideoIO open attempt reports the tested backend result and exits within the configured timeout; it must not leave a worker, socket, or native handle behind.
- DNN and the selected full-profile smoke continue to load through the same neutral loader when their modules are present.
- A second invocation after a failed GUI call remains usable, proving that the negative path did not corrupt global native state.

These are consumer evidence requirements, not promises that GUI calls are silently ignored. An exception or `NOT_LINKED` result is acceptable when it is deterministic, documented, and covered by the profile guard.

## Promotion gate

Before creating `JYPPX.OpenCV.runtime.<rid>.headless` in the active matrix, the following evidence is required for at least Ubuntu 22.04 and Debian 12 (and one additional target selected by the support plan):

1. Full and headless producers use the same OpenCV source/version and reproducible toolchain, with profile-specific provenance and ABI manifests.
2. Package inspection proves no GUI backend payload or forbidden GUI SONAME, and every shipped native file has a matching producer hash.
3. Native smoke proves codec, DNN, and the documented VideoIO backend result with no display server.
4. The managed consumer restores only the headless candidate package, invokes the negative HighGui checks, and records OS, libc, architecture, SDK, package hash, and loader result.
5. The support contract, release SBOM, closeout record, and rollback fixture all classify the package as candidate-only until every target/profile row is complete.

Failure of any gate leaves the profile as an ADR and temporary artifact. It must not change fallback precedence for the existing distro-specific packages or generic Linux previews.

## Rollback and future work

Rollback is deletion of the candidate artifact and its evidence plus removal of the candidate-only matrix row; no active package identity or existing lockfile is rewritten. A future GUI-enabled profile, such as `headful`, must be a separate decision and cannot be inferred by negating this ADR. A future musl/headless combination also requires an independent libc and dependency audit.

## Consequences

Consumers can target servers without installing a GUI stack while retaining the existing managed API shape. The cost is an additional profile-specific ABI, dependency, and VideoIO evidence chain. Until that chain is automated and independently reproduced, the repository intentionally documents the profile without publishing it.

The existing Mini runtime provides a bounded candidate negative smoke on Windows x64: with `DISPLAY`, `WAYLAND_DISPLAY`, `LD_LIBRARY_PATH`, and the repository runtime-root variable unset, HighGui `NamedWindow`, `DestroyWindow`, `ImShow`, trackbar creation, and backend-query calls take the deterministic missing-entrypoint/`NOT_LINKED` path, PNG encode remains usable afterward, a missing-file VideoIO open returns false and disposes cleanly, and a DNN target query is deterministically unavailable because Mini omits dnn on net8.0 and net10.0. The evidence is recorded in [`runtime-headless-candidate-evidence.json`](../../packaging/runtime/runtime-headless-candidate-evidence.json). It is partial evidence only; a Full headless candidate requires a wrapper rebuilt without HighGui and still needs camera/network VideoIO, DNN, package, and multi-distro consumer gates.

## OpenCV 5.1.0 local candidate results

The separate [Linux headless evidence](https://github.com/guojin-yan/OpenCV-CSharp-API/blob/opencv5.x/packaging/runtime/opencv-5.1.0-local-linux-headless-evidence.json) records a rebuilt Full wrapper with HighGui disabled and a matching Mini build. The earlier Windows Mini 5.0 measurement retains its original scope and hashes. The new packages reuse the existing `.linux-x64.preview` and `.linux-x64.preview.mini` identities with the isolated version `5.1.0-headless.2963ce5`; no `.headless` identity is added to the active matrix.

The producer reuses the previously measured official OpenCV/contrib 5.1.0 Ubuntu 22.04 installation, including the repository's CCM patch. It stages only the selected 16 Full or six Mini upstream modules and rebuilds the corresponding 49 or nine wrapper sources. This is not a new minimal upstream build. A copied source overlay disables the 30 HighGui implementation conditions without changing `mat_handle.h`'s OpenCV definition. It also removes the exported `ptcloud` CMake interface edge to HighGui; the actual reused `ptcloud` ELF has no such dependency. Each packaged ELF is audited independently for its complete dependency closure, `$ORIGIN`, SONAME, glibc/libstdc++ floors and absence of GUI libraries.

Both ABI manifests are regenerated after deleting their copied outputs. Generated manifests, actual exported symbols and six successful CTest results agree: Full retains 2,669 entrypoints, including deterministic HighGui `NOT_LINKED` stubs; Mini retains 527 entrypoints and its existing omitted HighGui/DNN entrypoints. Full contains 49 native package files and Mini 19, including the SONAME/file aliases.

Three new independent containers use pinned official Ubuntu 22.04, Ubuntu 24.04 and Debian 12 images, separate volumes and a read-only package/tool kit. They have no compiler, producer mounts, display server, GUI runtime packages or native loader overrides. Each restores only the managed 5.1.0 package and its selected candidate from the local feed into a fresh cache, then builds and executes net8.0 and net10.0: twelve executions and 72 negative GUI calls passed. Every GUI failure is followed by a successful native Mat operation. All runs verify PNG pixels and three MJPG AVI frames, checksums and EOF. Full executes the fixed identity ONNX model twice per run; Mini verifies the absent importer.

The explicit `CV_MJPEG` missing-file open returns false twice per run. The second open/dispose preserves worker and socket counts and leaves no file handle. `CV_IMAGES` and `CV_MJPEG` are available built-in backends; FFmpeg, GStreamer, Intel MFX and uEye are unavailable. A 60-second process timeout bounds each consumer execution. No run timed out; camera/network backend attempts and their timeout recovery remain unvalidated. The original OpenCV missing-file warnings and Full CPU-target warning are retained separately from the zero-warning restore/build results. The first .NET 10 SDK welcome message is also retained in raw stderr.

The candidate SPDX inventories for [Full](https://github.com/guojin-yan/OpenCV-CSharp-API/blob/opencv5.x/packaging/release/opencv-5.1.0-local-headless-sbom/JYPPX.OpenCV.runtime.linux-x64.preview.5.1.0-headless.2963ce5.spdx.json) and [Mini](https://github.com/guojin-yan/OpenCV-CSharp-API/blob/opencv5.x/packaging/release/opencv-5.1.0-local-headless-sbom/JYPPX.OpenCV.runtime.linux-x64.preview.mini.5.1.0-headless.2963ce5.spdx.json) bind every archive entry and the package verification code. They describe these local packages; a complete static third-party inventory, hosted reproduction, ARM64 and final release approval remain open. Active support status and fallback precedence retain their existing values.

Reproduce the producer in the measured Ubuntu 22.04 toolchain using a fresh workspace:

```sh
python3 /repo-input/scripts/Build-Linux51HeadlessCandidate.py \
  --repo /repo-input --workspace /workspace/headless-candidate-new \
  --install /workspace/artifacts/opencv-install/opencv-5.1.0-ubuntu.22.04-x64 \
  --baseline /workspace/linux51-output
```

Create a small read-only kit containing the three package archives and their `packages.json` hashes, both verified SDK archives, PowerShell, the CA bundle, `tools/LinuxHeadlessPackageSmoke` sources, the existing ONNX fixture and the two consumer scripts. Bootstrap each fresh matching container with `Initialize-Linux51HeadlessConsumer.sh`, then execute `Invoke-Linux51HeadlessConsumer.ps1`. Export its logs and `/consumer/artifacts` without sharing a producer volume. `Measure-Linux51HeadlessEvidence.ps1` collects package, source, raw log, image, mount, SBOM and execution evidence; `Test-Linux51HeadlessEvidence.ps1 -RequireArtifacts` verifies the retained artifacts and semantic negative fixtures.

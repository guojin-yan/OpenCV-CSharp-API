# Headless Runtime Profile ADR

Status: proposed for v5.0.1; candidate-only and not publishable.

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

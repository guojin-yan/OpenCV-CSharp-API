# Generic linux-musl-x64/arm64 Feasibility ADR

Status: research-only for v5.0.1; no generic musl package identity is publishable.

## Scope and decision

`linux-musl-x64` and `linux-musl-arm64` remain feasibility targets, not active package identities. The existing `alpine.3.20-x64` package surface is a separately classified compatibility target and does not imply that a single generic musl package works on every Alpine release, CPU, or native backend. A future generic package would need an explicit identity and a new promotion review; it cannot be introduced by changing RID fallback alone.

The first reference userspace is official Alpine Linux. x64 has an existing compatibility-only matrix row. ARM64 is research-only until a native AArch64 producer and consumer are available. QEMU-only evidence is insufficient for promotion because it cannot validate the host kernel, CPU features, filesystem ABI, or camera/video backends.

## RID and fallback boundary

The repository RID graph keeps `linux-musl-x64` importing `linux-musl`, and `linux-musl` importing `linux`. The same graph may later add `linux-musl-arm64`, but exact RID assets must always win over imported generic assets. Managed package references remain explicit: the managed package never silently chooses a musl runtime package, and a glibc package must never be selected as a musl fallback.

## Native and dependency risks

Musl is a libc boundary, not merely a distro label. Every candidate ELF needs a musl loader/interpreter audit, symbol/version audit, `$ORIGIN` closure, and producer-to-package hash comparison. The dependency policy must classify musl libc, libstdc++, libgcc, bundled codecs/protobuf, and any approved system runtime separately from optional OpenCV modules.

FFmpeg, GStreamer, GTK, Qt, X11, Wayland, font, and camera backends are not assumed available. A candidate must record each backend as available, unavailable, or disabled. The presence of `opencv_videoio` alone is not evidence that a headless or network camera scenario works. GUI dependencies must be explicit; they cannot enter a generic musl package transitively through an unreviewed system package.

## Required evidence

- Native x64 Alpine producer and independent consumer with exact image digest, musl version, kernel/runner identity, source commit, toolchain, and profile-specific provenance.
- Native AArch64 Alpine producer and consumer, or a documented stop decision if only emulation is available.
- Full and Mini profile parity, including distinct ABI manifests and package payloads.
- ELF interpreter/symbol/dependency closure for every shipped file; no glibc loader, glibc-only symbol, producer path, or undeclared GUI/backend SONAME.
- Codec read/write smoke, DNN smoke where the profile contains DNN, and explicit VideoIO backend results with timeout and cleanup evidence.
- RID selection tests proving exact musl assets precede `linux-musl` and `linux`; glibc assets are rejected as candidates.

## Stop and promotion rules

Stop at research-only when a native ARM64 runner is unavailable, when musl-specific toolchain or OpenCV source provenance cannot be reproduced, or when a required backend depends on an unbounded external system package. Do not create `JYPPX.OpenCV.runtime.linux-musl-*` identities for incomplete evidence.

Promotion requires at least two independently consumed musl userspaces, both x64 and ARM64 evidence for any architecture-specific identity, profile-specific ABI/provenance, deterministic package hashes, and a release/SBOM/rollback update. Existing Alpine compatibility packages and glibc distro packages remain the rollback path.

## Consequences

This boundary preserves a useful Alpine compatibility path while preventing accidental claims that all musl distributions are interchangeable. It also leaves room for a future headless-musl profile, but that profile would need to satisfy this libc audit and the separate headless negative-behaviour contract.


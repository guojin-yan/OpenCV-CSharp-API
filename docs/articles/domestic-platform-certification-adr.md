# Domestic C# Platform Certification ADR

Status: research and certification plan for v5.0.1. No domestic distribution-specific NuGet identity is approved by this ADR.

## Decision

openEuler, 银河麒麟/Kylin, 统信 UOS, and openKylin are treated as Linux/glibc certification targets. They may reuse `linux-x64` or `linux-arm64` only after the corresponding generic Linux producer/package/consumer chain and target OS evidence close. A vendor distribution name alone does not justify a new runtime package ID. LoongArch64 is a separate blocked-research track because it requires a supported C#/.NET runtime, native OpenCV toolchain, and real hardware; it cannot inherit an x64 or ARM64 package.

The machine-readable matrix at `packaging/platform/domestic-platform-certification-matrix.json` records release, architecture, .NET requirement, generic RID reuse, evidence level, GUI/driver risks, official reference links, and stop conditions. The generic linux RID policy is reuse-only after evidence. It deliberately distinguishes `certification-candidate`, `research-only`, and `blocked-research`.

## What “supports C#” means

The minimum is a supported .NET 8 runtime/SDK and a successful C# restore/build/run on the exact vendor image. Mono-only results, a vendor-patched CLR without provenance, or a process that starts but cannot load the audited native wrapper are not release evidence. The report must include `dotnet --info`, SDK acquisition/hash, runtime identifier, libc/OpenSSL/ICU facts, and package restore logs.

## Reuse of generic Linux packages

The project should not create `runtime.openEuler`, `runtime.kylin`, or `runtime.uos` identities during certification. If the OS consumes the generic glibc package, the evidence must prove the exact `linux-x64` or `linux-arm64` asset, loader, ELF symbol floor, dependency closure, and native smoke. If a vendor image needs a special patch, system library, or compiler flag, that result is a platform-specific certification note and does not silently change the generic package contract.

## Evidence stages

1. Runtime: native architecture, OS release/kernel, .NET 8/10 SDK and C# hello-world restore/build/run.
2. Native: neutral loader, core/imgproc/imgcodecs smoke, full dependency closure, and package hash comparison.
3. Headless: unset `DISPLAY` and `WAYLAND_DISPLAY`; prove deterministic HighGui negative behavior, codec/DNN smoke, and explicit VideoIO backend result/cleanup.
4. Desktop: only when requested, record GTK/Qt/X11/Wayland/display-server/font/camera/codec dependencies and run HighGui interaction smoke.
5. Performance: CPU model/features, fixed-input benchmark, peak RSS, and backend limitations. Hygon, Zhaoxin, Kunpeng, and Phytium are CPU evidence dimensions, not new package architectures.

QEMU-only evidence is useful for research but cannot promote an ARM64 platform. Real hardware or a trusted native AArch64 runner is required for native loader, VideoIO, driver, timing, and cleanup claims.

## Platform-specific risks

- openEuler has x86_64 and AArch64 certification value, but release/support and vendor package channels must be recorded per LTS image; OpenCL/Vulkan/VA-API and camera backends remain device-specific.
- Kylin V10 SP1 and UOS 20 may have different OpenSSL, ICU, GUI, codec, and vendor-driver packaging from Debian/Ubuntu. A portable .NET process is not enough to claim package support.
- openKylin is a separate distribution and needs its own image, SDK source, and native evidence even when its userland resembles Debian.
- LoongArch64 must stop if a supported .NET JIT/AOT and OpenCV C++ ABI cannot be reproduced; a QEMU demo is not a package commitment.

## Promotion and rollback

Promotion requires runtime, native, headless/desktop scope, and rollback evidence for every architecture/release row. Failure keeps the platform at research/certification status and leaves existing generic/distro-specific package identities unchanged. Rollback is removal of the certification record/evidence and any preview artifact; no active package identity or RID fallback is rewritten.

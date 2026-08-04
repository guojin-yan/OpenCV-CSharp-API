# Support And Lifecycle Policy

This document is the authoritative support classification for runtime packages. A package RID/profile name is a packaging surface, not a production-support claim. The machine-readable source is `packaging/runtime/runtime-support-contract.json`, and every release review must bind its SHA256 and counts.

## Current Classification

| Classification | Count | Meaning |
| --- | ---: | --- |
| `real-supported` | 28 | Non-synthetic native producer, package handoff, and package consumer evidence exists for the exact RID/profile. |
| `hosted-evidence-pending` | 1 | `win-x86/full` is locally feasible, but hosted producer, same-run pack, independent artifact audit, and X86 consumer evidence are still missing. |
| `android-evidence-pending` | 4 | Android ARM/ARM64 still require ABI-matched physical-device loading evidence. |
| `excluded` | 1 | `win-x86/mini` is not production support. |
| outside matrix | 1 | macOS is intentionally not declared. |

The 28 real-supported entries comprise twelve full and twelve mini desktop/server targets plus Android x64/x86 Full and Mini. Fedora 40 and Alpine 3.20 are retained as explicit compatibility targets: their historical userspace, image identity, lifecycle state, and exact native/package evidence must remain recorded. An ended distro is never silently represented as a current-lifecycle promise.

## Promotion Evidence

A target can move into `real-supported` only when the exact target has all of the following:

1. Source-traceable native producer evidence on the target userspace and architecture.
2. ABI/export, loader, dependency, payload, license, and provenance evidence for the selected full or mini profile.
3. A same-run package handoff whose files and hashes match the producer.
4. An independent package consumer process on the target without producer search-path overrides.
5. Profile-specific native smoke and negative `NOT_LINKED` behavior where applicable.
6. Documentation and release records that name the exact target without broadening the support contract.

Synthetic runtime inputs, unaudited cross-builds, relabeled binaries, PE/ELF headers alone, or package presence alone cannot promote a target. Android NDK cross-production must be paired with ABI-specific ELF/package audits and a real APK consumer that executes a native OpenCV call on an ABI-matched emulator or device.

## Pending And Excluded Targets

`win-x86/full` requires one quota-authorized hosted chain in this order: hosted producer, neutral artifact handoff, same-run non-synthetic pack, independent artifact/PE audit, and an actual X86 consumer process. Until then it remains pending. `win-x86/mini` is excluded and must not be inferred from the full profile.

Android x64/x86 Full and Mini are real-supported after the single neutral loader was rebuilt, packaged, consumed by an APK, and loaded on authoritative hosted emulators. The current evidence is under `verified`, while retired dual-loader records remain under `superseded` in `packaging/runtime/android-runtime-evidence.json`. Android ARM/ARM64 remain android-evidence-pending until the same APK consumer succeeds on ABI-matched physical devices. macOS requires an explicit matrix decision followed by native build and package-consumer evidence; it is currently outside the matrix.

## Retirement And Review

Every release candidate reviews image digests, distro lifecycle, compiler/toolchain availability, security advisories, test freshness, and consumer evidence. A target is retired or moved to compatibility-only status when its userspace is no longer reproducible, its security/lifecycle policy is unacceptable, or its evidence can no longer be rerun. Retirement updates the support contract and documentation together; it does not silently leave a stale package claim.

The current local candidate is unsigned, unapproved, unpublished, and read-only-feed verified. It is not a release and does not change any support classification.

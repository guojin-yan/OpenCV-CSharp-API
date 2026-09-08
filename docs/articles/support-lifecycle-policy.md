# Support And Lifecycle Policy

This document is the authoritative support classification for runtime packages. A package RID/profile name is a packaging surface, not a production-support claim. The machine-readable source is `packaging/runtime/runtime-support-contract.json`; `packaging/runtime/runtime-support-contract.schema.json` defines and validates its v2 structure. Every release review must bind the contract SHA256 and classification counts.

## Current Classification

| Classification | Count | Meaning |
| --- | ---: | --- |
| package surface | 34 | The RID/profile exists in the package matrix and can be selected for controlled production; this is not a support promise. |
| `real-supported` | 25 | Non-synthetic native producer, package handoff, package consumer evidence, and a current lifecycle exist for the exact RID/profile. |
| `compatibility-only` | 4 | Fedora 40 and Alpine 3.20 Full/Mini retain exact reproducible historical evidence for existing users but are excluded from the current release candidate. |
| `android-evidence-pending` | 4 | Android ARM/ARM64 still require ABI-matched physical-device loading evidence. |
| `excluded` | 1 | `win-x86/mini` is not production support. |
| outside matrix | 1 | macOS is intentionally not declared. |

Schema v2 records all 34 package-surface entries independently from their support classification. The 25 real-supported entries comprise eleven full and ten mini desktop/server targets plus Android x64/x86 Full and Mini. Fedora 40 and Alpine 3.20 Full/Mini are the four explicit compatibility-only entries: their historical userspace, pinned image identity, lifecycle state, and exact native/package evidence remain reproducible, but they are not included in the current release candidate. An ended distro is never silently represented as a current-lifecycle promise.

## Promotion Evidence

A target can move into `real-supported` only when the exact target has all of the following:

1. Source-traceable native producer evidence on the target userspace and architecture.
2. ABI/export, loader, dependency, payload, license, and provenance evidence for the selected full or mini profile.
3. A same-run package handoff whose files and hashes match the producer.
4. An independent package consumer process on the target without producer search-path overrides.
5. Profile-specific native smoke and negative `NOT_LINKED` behavior where applicable.
6. Documentation and release records that name the exact target without broadening the support contract.

Synthetic runtime inputs, unaudited cross-builds, relabeled binaries, PE/ELF headers alone, or package presence alone cannot promote a target. Android NDK cross-production must be paired with ABI-specific ELF/package audits and a real APK consumer that executes a native OpenCV call on an ABI-matched emulator or device.

A `compatibility-only` target remains selectable by the runtime-input workflow so its historical evidence can be reproduced. It cannot enter the publication manifest unless a later lifecycle review promotes it to `real-supported`; the manifest derives its exact package closure only from `realSupport`.

## Pending And Excluded Targets

`win-x86/full` is real-supported after its hosted producer, neutral artifact handoff, same-run non-synthetic pack, independent artifact/PE audit, WoW64 probe, and actual X86 consumer process all passed. The exact source commit, run IDs, artifact identities and digests, and hosted audit hashes are retained in `runtime-support-contract.json`. `win-x86/mini` remains excluded and must not be inferred from the full profile.

Android x64/x86 Full and Mini are real-supported after the single neutral loader was rebuilt, packaged, consumed by an APK, and loaded on authoritative hosted emulators. The current evidence is under `verified`, while retired dual-loader records remain under `superseded` in `packaging/runtime/android-runtime-evidence.json`. Android ARM/ARM64 remain android-evidence-pending until the same APK consumer succeeds on ABI-matched physical devices. macOS requires an explicit matrix decision followed by native build and package-consumer evidence; it is currently outside the matrix.

## Retirement And Review

Every release candidate reviews image digests, distro lifecycle, compiler/toolchain availability, security advisories, test freshness, and consumer evidence. A target is retired or moved to compatibility-only status when its current lifecycle has ended but its fixed userspace remains reproducible. It is excluded when its userspace or evidence can no longer be reproduced, or its security policy is unacceptable even for compatibility use. Classification changes update the support contract, publication closure, closeout evidence, and documentation together.

The current local candidate is unsigned, unapproved, unpublished, and read-only-feed verified. It is not a release and does not change any support classification.

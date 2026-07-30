# Support And Lifecycle Policy

This document is the authoritative support classification for runtime packages. A package RID/profile name is a packaging surface, not a production-support claim. The machine-readable source is `packaging/runtime/runtime-support-contract.json`, and every release review must bind its SHA256 and counts.

## Current Classification

| Classification | Count | Meaning |
| --- | ---: | --- |
| `real-supported` | 24 | Non-synthetic native producer, package handoff, and package consumer evidence exists for the exact RID/profile. |
| `hosted-evidence-pending` | 1 | `win-x86/full` is locally feasible, but hosted producer, same-run pack, independent artifact audit, and X86 consumer evidence are still missing. |
| `excluded` | 9 | `win-x86/mini` and all Android full/mini profiles are not production support. |
| outside matrix | 1 | macOS is intentionally not declared. |

The 24 real-supported entries are the twelve full and twelve mini desktop/server targets listed in the support contract. Fedora 40 and Alpine 3.20 are retained as explicit compatibility targets: their historical userspace, image identity, lifecycle state, and exact native/package evidence must remain recorded. An ended distro is never silently represented as a current-lifecycle promise.

## Promotion Evidence

A target can move into `real-supported` only when the exact target has all of the following:

1. Source-traceable native producer evidence on the target userspace and architecture.
2. ABI/export, loader, dependency, payload, license, and provenance evidence for the selected full or mini profile.
3. A same-run package handoff whose files and hashes match the producer.
4. An independent package consumer process on the target without producer search-path overrides.
5. Profile-specific native smoke and negative `NOT_LINKED` behavior where applicable.
6. Documentation and release records that name the exact target without broadening the support contract.

Synthetic runtime inputs, cross-builds, emulation, relabeled binaries, PE/ELF headers alone, or package presence alone cannot promote a target.

## Pending And Excluded Targets

`win-x86/full` requires one quota-authorized hosted chain in this order: hosted producer, neutral artifact handoff, same-run non-synthetic pack, independent artifact/PE audit, and an actual X86 consumer process. Until then it remains pending. `win-x86/mini` is excluded and must not be inferred from the full profile.

Android requires a real native build, Android package consumer, and device or emulator loading evidence for each profile before any Android RID is promoted. macOS requires an explicit matrix decision followed by native build and package-consumer evidence; it is currently outside the matrix.

## Retirement And Review

Every release candidate reviews image digests, distro lifecycle, compiler/toolchain availability, security advisories, test freshness, and consumer evidence. A target is retired or moved to compatibility-only status when its userspace is no longer reproducible, its security/lifecycle policy is unacceptable, or its evidence can no longer be rerun. Retirement updates the support contract and documentation together; it does not silently leave a stale package claim.

The current local candidate is unsigned, unapproved, unpublished, and read-only-feed verified. It is not a release and does not change any support classification.

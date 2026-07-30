# Release Candidate Closeout

The current candidate is a local, deterministic preflight only. The generated record is `packaging/release/local-release-candidate-closeout.json`; `scripts/Test-ReleaseCandidateFinalCloseout.ps1` verifies its source-set digest, evidence hashes, API/ABI baselines, support contract, and fail-closed release states. Candidate identity is derived only from the normalized source-set SHA-256, so committing an already verified source set does not invalidate the record through a changing Git HEAD.

## Local Checklist

- [x] Managed package and runtime package identities remain version-neutral.
- [x] Native full/mini ABI manifests and managed API baseline are generated and checked for deterministic drift.
- [x] The bounded OpenCV 5.0.0 ImgProc map classifies all 203 parsed declarations, with 161 implemented callables, zero measured gaps, 6 intentional omissions, and no repository-wide parity claim.
- [x] Runtime support is classified as 24 real, 1 pending, 9 excluded, with macOS outside the matrix.
- [x] Deterministic package normalization runs before any future signing input.
- [x] Provenance, license, runtime payload, change-control, rollback, and read-only public-feed evidence are locally validated.
- [x] Exact SDK `10.0.302`, DocFX `2.78.5`, workflow syntax, actionlint, and aggregate invariant checks are local gates.
- [x] Main Video ECC and TrackerMIL callables are closed, with 138 implemented callables, zero measured gaps, and 7 intentional omissions in the bounded Video partition.
- [ ] Hosted `win-x86/full` producer-to-X86-consumer evidence after quota restoration.
- [ ] External signing and SPDX-2.3 SBOM inputs with immutable public references.
- [ ] Independent reviewer/approver acceptance of exact normalized/post-signing bytes.
- [ ] Publication authorization, preview release, and public installation verification.

## Stop Conditions

Stop immediately on source or artifact hash drift, synthetic runtime inputs in a publish-capable path, incomplete PE/ELF closure, producer/search-path overrides, missing license or provenance evidence, mutable certificate/feed references, unapproved signing/SBOM state, remote mutation, or any attempt to publish before the complete hosted and approval chain succeeds.

## Blocker Ledger

The local candidate remains `locally-validated`, `not-approved`, unsigned, and unpublished. Remaining blockers are hosted CI compatibility and `win-x86/full`, signing/SBOM/approval/publication inputs, Android and macOS support decisions, repository-wide API gap implementation, and still-unmeasured upstream surfaces outside the bounded module partitions. These blockers are external or future implementation work; they are not silently converted into support claims.

# Upstream Map Toolchain

The upstream map catalog centralizes the source scope, parser identity, classification policy, output files, and semantic check command for the 14 parser-backed maps already in the repository. Its schema is [`upstream-map-catalog.schema.json`](../../compatibility/upstream-map-catalog.schema.json), and the checked catalog is [`upstream-map-catalog.json`](../../compatibility/upstream-map-catalog.json).

Regenerate the catalog after an intentional map-surface update:

```powershell
pwsh -NoProfile -File .\scripts\New-UpstreamMapCatalog.ps1
```

Run the configured suite:

```powershell
pwsh -NoProfile -File .\scripts\Test-UpstreamMapCatalog.ps1
```

The suite validates the catalog schema, pinned OpenCV 5.0.0 source-header/parser identities, raw declaration/classification ordinal alignment, output and family hashes, shared classification rules, and zero unexplained missing declarations. It then invokes each configured module guard, which preserves that map's existing semantic fixtures. Specialized extractors and generators remain per module because include closures and source-reviewed exceptions differ. The catalog does not claim repository-wide OpenCV parity.

`MAP-002..004` can extend the same catalog after each contrib module's exact headers, optional dependency boundaries, source-review exclusions, and classification rules have been reviewed.

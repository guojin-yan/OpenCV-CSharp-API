# Upstream Map Toolchain

The upstream map catalog centralizes the source scope, parser identity, classification policy, output files, and semantic check command for 23 parser-backed maps. Its schema is [`upstream-map-catalog.schema.json`](../../compatibility/upstream-map-catalog.schema.json), and the checked catalog is [`upstream-map-catalog.json`](../../compatibility/upstream-map-catalog.json).

Regenerate the catalog after an intentional map-surface update:

```powershell
pwsh -NoProfile -File .\scripts\New-UpstreamMapCatalog.ps1
```

Run the configured suite:

```powershell
pwsh -NoProfile -File .\scripts\Test-UpstreamMapCatalog.ps1
```

The suite validates the catalog schema, pinned OpenCV 5.0.0 source-header/parser identities, raw declaration/classification ordinal alignment, output and family hashes, shared classification rules, and zero unexplained missing declarations. It then invokes each configured module guard, which preserves that map's semantic fixtures. Face, Quality, ImgHash, LineDescriptor, and FreeType reuse the shared contrib parser extractor; each map keeps its own module scope and semantic guard. FreeType remains fully omitted because the package has no wrapper and upstream requires external FreeType2/HarfBuzz dependencies. The catalog does not claim repository-wide OpenCV parity.

`MAP-002..004` are represented in the catalog. MAP-005 incrementally adds remaining contrib modules after each module's exact headers, optional dependency boundaries, source-review exclusions, and classification rules have been reviewed.

# Upstream Map Toolchain

The upstream map catalog centralizes the source scope, parser identity, classification policy, output files, and semantic check command for 32 parser-backed maps. Its schema is [`upstream-map-catalog.schema.json`](../../compatibility/upstream-map-catalog.schema.json), and the checked catalog is [`upstream-map-catalog.json`](../../compatibility/upstream-map-catalog.json).

Regenerate the catalog after an intentional map-surface update:

```powershell
pwsh -NoProfile -File .\scripts\New-UpstreamMapCatalog.ps1
```

Run the configured suite:

```powershell
pwsh -NoProfile -File .\scripts\Test-UpstreamMapCatalog.ps1
```

The suite validates the catalog schema, pinned OpenCV 5.0.0 source-header/parser identities, raw declaration/classification ordinal alignment, output and family hashes, shared classification rules, and zero unexplained missing declarations. It then invokes each configured module guard, which preserves that map's semantic fixtures. AlphaMat, Face, Quality, ImgHash, LineDescriptor, FreeType, IntensityTransform, Plot, BioInspired, PhaseUnwrapping, HFS, and Fuzzy reuse the shared contrib parser extractor; each map keeps its own module scope and semantic guard. FreeType remains fully omitted because the package has no wrapper and upstream requires external FreeType2/HarfBuzz dependencies. AlphaMat binds its single parser declaration to the existing native symbol and both managed overloads. IntensityTransform binds six parser callables to six native symbols and twelve managed overloads; BIMEF still requires an EIGEN-enabled OpenCV build at runtime. Plot maps all 20 parser callables to declaration-specific native symbols and 21 Plot2d managed members; its release-handle ABI and `PlotCv2` convenience factories remain explicit wrapper-only surface, not additional parser callables. BioInspired maps the pinned public-header closure's 32 callables to 31 distinct ABI symbols and 31 exact managed members; its Retina channel setup functions are represented by parameter-struct managed overloads. PhaseUnwrapping maps three callables to three native symbols and three exact managed members; the upstream `Params` value constructor is represented by a default parameter value and is not treated as a native callable. Its release ABI and `PhaseUnwrappingCv2` factories remain wrapper-only. The catalog does not claim repository-wide OpenCV parity.

HFS maps all 17 callable declarations in the pinned `opencv2/hfs.hpp` parser closure to seven distinct native ABI symbols and ten managed members. The existing HFS smoke coverage exercises CPU segmentation only when native smoke is enabled; GPU segmentation is an optional runtime capability and is not required by this map's test evidence.

Fuzzy maps all 16 callable declarations in the pinned `opencv2/fuzzy.hpp` closure (four parser-emitted public headers) to 16 exact native ABI symbols and output-oriented managed members; its two enum declarations retain their pinned values. The module is optional at runtime, and the existing focused tests remain the behavioral boundary rather than a claim of image-quality parity.

Rapid maps the pinned `opencv2/rapid.hpp` closure's 12 existing callable wrappers to 12 exact native ABI symbols and managed members. The upstream `GOSTracker.create` factory is recorded as an intentional omission because the current wrapper exposes only the basic Rapid and OLS trackers; tracker-release ABI remains wrapper-only and outside parser callable scope.

Shape maps the pinned `opencv2/shape.hpp` parser closure's 20 callable declarations to 18 distinct native ABI symbols and 19 managed members. Both Norm and EMD norm-flag declarations bind to the shared managed base property and native accessors; unwrapped ShapeContext configuration and ShapeTransformer APIs remain explicit omissions. The `emd_l1` helper and two native handle-release exports remain outside this parser closure.

`MAP-002..004` are represented in the catalog. MAP-005 incrementally adds remaining contrib modules after each module's exact headers, optional dependency boundaries, source-review exclusions, and classification rules have been reviewed; ten MAP-005 modules currently have zero unexplained missing declarations.

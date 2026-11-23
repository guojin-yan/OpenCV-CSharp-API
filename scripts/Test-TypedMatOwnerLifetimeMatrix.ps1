param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$evidenceRel = 'packaging/performance/typed-mat-owner-lifetime-matrix.json'
$schemaRel = 'packaging/performance/typed-mat-owner-lifetime-matrix.schema.json'
$evidencePath = Join-Path $repo ($evidenceRel -replace '/', [IO.Path]::DirectorySeparatorChar)
$schemaPath = Join-Path $repo ($schemaRel -replace '/', [IO.Path]::DirectorySeparatorChar)
foreach ($path in @($evidencePath,$schemaPath)) { if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Typed Mat owner/lifetime matrix input is missing: $path" } }
if (-not (Test-Json -LiteralPath $evidencePath -SchemaFile $schemaPath -ErrorAction Stop)) { throw 'Typed Mat owner/lifetime matrix failed JSON Schema validation.' }
$evidence = Get-Content -LiteralPath $evidencePath -Raw | ConvertFrom-Json
if ([string]$evidence.status -cne 'reviewed' -or [string]$evidence.sourceCommit -notmatch '^[0-9a-f]{40}$') { throw 'Typed Mat owner/lifetime matrix identity is invalid.' }
$commitType = ((& git -C $repo cat-file -t ([string]$evidence.sourceCommit) 2>$null) | Select-Object -First 1).Trim()
if ($LASTEXITCODE -ne 0 -or $commitType -cne 'commit') { throw 'Typed Mat owner/lifetime source commit is not present in the repository.' }
& git -C $repo merge-base --is-ancestor ([string]$evidence.sourceCommit) HEAD 2>$null
if ($LASTEXITCODE -ne 0) { throw 'Typed Mat owner/lifetime source commit is not an ancestor of HEAD.' }
& git -C $repo diff --quiet ([string]$evidence.sourceCommit) -- src/OpenCvSharp tests/OpenCvSharp.Tests
if ($LASTEXITCODE -ne 0) { throw 'Typed Mat managed source or tests differ from the native lifetime evidence.' }
$frameworks = @('net46','net461','net462','net47','net471','net472','net48','net481','netcoreapp3.1','net5.0','net6.0','net7.0','net8.0','net9.0','net10.0')
$rows = @($evidence.frameworks)
if ($rows.Count -ne 15 -or (@($rows | ForEach-Object { [string]$_.framework }) -join '|') -cne ($frameworks -join '|')) { throw 'Typed Mat owner/lifetime matrix framework order drifted.' }
if ([bool]$evidence.policy.stablePromotionAllowed -or [bool]$evidence.summary.stablePromotionAllowed) { throw 'Typed Mat owner/lifetime matrix must not allow stable promotion.' }
$legacy = @('net46','net461','net462','net47','net471','net472','net48','net481')
$verified = @('net8.0','net9.0','net10.0')
$compileOnly = @('netcoreapp3.1','net5.0','net6.0','net7.0')
foreach ($row in $rows) {
    $framework = [string]$row.framework
    if ($legacy -contains $framework) {
        if ([string]$row.surface -cne 'legacy-excluded' -or [string]$row.ownerReview -cne 'not-applicable' -or [string]$row.lifetimeEvidence -cne 'not-applicable') { throw "Legacy Typed Mat policy drifted for $framework." }
    } elseif ($verified -contains $framework) {
        if ([string]$row.surface -cne 'span-preview' -or [string]$row.ownerReview -cne 'native-verified' -or [string]$row.lifetimeEvidence -cne 'native-verified') { throw "Native-verified Typed Mat policy drifted for $framework." }
    } elseif ($compileOnly -contains $framework) {
        if ([string]$row.surface -cne 'span-preview' -or [string]$row.ownerReview -cne 'reviewed' -or [string]$row.lifetimeEvidence -cne 'compile-only') { throw "Compile-only Typed Mat policy drifted for $framework." }
    } else { throw "Unexpected Typed Mat framework: $framework" }
    if ([string]$row.compileEvidence -cne 'release-build' -or [string]::IsNullOrWhiteSpace([string]$row.rationale)) { throw "Typed Mat evidence row is incomplete for $framework." }
}
$native51Path = Join-Path $repo 'packaging/performance/typed-mat-native-lifetime-evidence-5.1.0.json'
$native51SchemaPath = Join-Path $repo 'packaging/performance/typed-mat-native-lifetime-evidence-5.1.0.schema.json'
foreach ($path in @($native51Path,$native51SchemaPath)) { if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "OpenCV 5.1 Typed Mat lifetime evidence is missing: $path" } }
if (-not (Test-Json -LiteralPath $native51Path -SchemaFile $native51SchemaPath -ErrorAction Stop)) { throw 'OpenCV 5.1 Typed Mat lifetime evidence failed schema validation.' }
$native51 = Get-Content -LiteralPath $native51Path -Raw | ConvertFrom-Json
if ([string]$native51.sourceCommit -cne [string]$evidence.sourceCommit -or [string]$native51.packageVersion -cne '5.1.0' -or [string]$native51.runtime.openCvVersion -cne '5.1.0' -or [int]$native51.runtime.nativeAbiVersion -ne 1 -or [int]$native51.runtime.nativeModuleFiles -ne 42 -or -not [bool]$native51.test.nativeSmokeEnabled) { throw 'OpenCV 5.1 Typed Mat runtime identity drifted.' }
$measuredFrameworks = @($native51.test.targetFrameworks)
if ($measuredFrameworks.Count -ne 3 -or (@($measuredFrameworks | ForEach-Object { [string]$_.framework }) -join '|') -cne 'net8.0|net9.0|net10.0') { throw 'OpenCV 5.1 Typed Mat native lifetime framework scope drifted.' }
foreach ($measured in $measuredFrameworks) {
    if ([int]$measured.total -ne 9 -or [int]$measured.executed -ne 9 -or [int]$measured.passed -ne 9 -or [int]$measured.failed -ne 0 -or [int]$measured.skipped -ne 0 -or [string]$measured.trx.sha256 -notmatch '^[0-9a-f]{64}$') { throw "OpenCV 5.1 Typed Mat native tests are incomplete for $($measured.framework)." }
}
$sourceBuildPath = Join-Path $repo 'packaging/runtime/opencv-5.1.0-source-build-evidence.json'
$sourceBuild = Get-Content -LiteralPath $sourceBuildPath -Raw | ConvertFrom-Json
$full = @($sourceBuild.wrapperProfiles | Where-Object profile -CEQ 'full')
if ([string]$sourceBuild.openCvVersion -cne '5.1.0' -or $full.Count -ne 1 -or @($native51.runtime.nativePayload).Count -ne 43 -or @($full[0].dlls).Count -ne 43) { throw 'OpenCV 5.1 Typed Mat payload inventory scope drifted.' }
if ((@($native51.runtime.nativePayload | ForEach-Object { [string]$_.name } | Sort-Object) -join '|') -cne (@($full[0].dlls | ForEach-Object { [string]$_.name } | Sort-Object) -join '|')) { throw 'OpenCV 5.1 Typed Mat payload names are missing, duplicated or unexpected.' }
foreach ($payload in $native51.runtime.nativePayload) {
    $recorded = @($full[0].dlls | Where-Object name -CEQ ([string]$payload.name))
    if ($recorded.Count -ne 1 -or [int64]$payload.bytes -ne [int64]$recorded[0].length -or [string]$payload.sha256 -cne [string]$recorded[0].sha256) { throw "Typed Mat native payload is not bound to the source-build evidence: $($payload.name)." }
}
foreach ($binding in @(
    @{ field='wrapperLoaderSha256'; name='JYPPX.OpenCV.Native.dll' },
    @{ field='opencvCoreSha256'; name='opencv_core510.dll' },
    @{ field='opencvImgProcSha256'; name='opencv_imgproc510.dll' },
    @{ field='opencvImgCodecsSha256'; name='opencv_imgcodecs510.dll' }
)) {
    $payload = @($native51.runtime.nativePayload | Where-Object name -CEQ $binding.name)[0]
    if ([string]$native51.runtime.($binding.field) -cne [string]$payload.sha256) { throw "Typed Mat runtime digest binding drifted: $($binding.field)." }
}
Write-Host "TYPED_MAT_OWNER_LIFETIME_MATRIX_OK source_commit=$($evidence.sourceCommit) frameworks=15 legacy_excluded=8 compile_only=4 native_verified=3 stable_promotion_allowed=false"

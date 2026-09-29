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
$frameworks = @('net46','net461','net462','net47','net471','net472','net48','net481','netcoreapp3.1','net5.0','net6.0','net7.0','net8.0','net9.0','net10.0')
$rows = @($evidence.frameworks)
if ($rows.Count -ne 15 -or (@($rows | ForEach-Object { [string]$_.framework }) -join '|') -cne ($frameworks -join '|')) { throw 'Typed Mat owner/lifetime matrix framework order drifted.' }
if ([bool]$evidence.policy.stablePromotionAllowed -or [bool]$evidence.summary.stablePromotionAllowed) { throw 'Typed Mat owner/lifetime matrix must not allow stable promotion.' }
$legacy = @('net46','net461','net462','net47','net471','net472','net48','net481')
$verified = @('net8.0','net10.0')
$compileOnly = @('netcoreapp3.1','net5.0','net6.0','net7.0','net9.0')
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
Write-Host "TYPED_MAT_OWNER_LIFETIME_MATRIX_OK source_commit=$($evidence.sourceCommit) frameworks=15 legacy_excluded=8 compile_only=5 native_verified=2 stable_promotion_allowed=false"

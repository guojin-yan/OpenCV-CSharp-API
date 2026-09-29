param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [string]$OutputPath = 'packaging/performance/typed-mat-owner-lifetime-matrix.json'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$status = @(& git -C $repo status --porcelain 2>$null)
if ($LASTEXITCODE -ne 0 -or $status.Count -ne 0) { throw 'Typed Mat owner/lifetime matrix measurement requires a clean repository.' }
$sourceCommit = ((& git -C $repo rev-parse HEAD 2>$null) | Select-Object -First 1).Trim().ToLowerInvariant()
if ($LASTEXITCODE -ne 0 -or $sourceCommit -notmatch '^[0-9a-f]{40}$') { throw 'Unable to resolve the Typed Mat owner/lifetime source commit.' }

$frameworks = @('net46','net461','net462','net47','net471','net472','net48','net481','netcoreapp3.1','net5.0','net6.0','net7.0','net8.0','net9.0','net10.0')
$legacy = @('net46','net461','net462','net47','net471','net472','net48','net481')
$verified = @('net8.0','net10.0')
$compileOnly = @('netcoreapp3.1','net5.0','net6.0','net7.0','net9.0')
$targetEvidencePath = Join-Path $repo 'packaging/performance/typed-mat-target-framework-evidence.json'
$lifetimeEvidencePath = Join-Path $repo 'packaging/performance/typed-mat-native-lifetime-evidence.json'
$benchmarkEvidencePath = Join-Path $repo 'packaging/performance/codec-typed-mat-benchmark-evidence.json'
foreach ($path in @($targetEvidencePath,$lifetimeEvidencePath,$benchmarkEvidencePath)) { if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Typed Mat evidence input is missing: $path" } }
$targetEvidence = Get-Content -LiteralPath $targetEvidencePath -Raw | ConvertFrom-Json
$lifetimeEvidence = Get-Content -LiteralPath $lifetimeEvidencePath -Raw | ConvertFrom-Json
if ((@($targetEvidence.frameworks) -join '|') -cne ($frameworks -join '|')) { throw 'Typed Mat compilation evidence framework set drifted.' }
$lifetimeFrameworks = @($lifetimeEvidence.test.targetFrameworks | ForEach-Object { [string]$_.framework })
if ((@($lifetimeFrameworks | Sort-Object) -join '|') -cne (@($verified | Sort-Object) -join '|')) { throw 'Typed Mat native lifetime evidence framework set drifted.' }

$rows = [System.Collections.Generic.List[object]]::new()
foreach ($framework in $frameworks) {
    if ($legacy -contains $framework) {
        $rows.Add([ordered]@{ framework = $framework; surface = 'legacy-excluded'; compileEvidence = 'release-build'; ownerReview = 'not-applicable'; lifetimeEvidence = 'not-applicable'; rationale = 'MatView and ReadOnlyMatView are excluded by NETCOREAPP3_1_OR_GREATER; existing byte and typed row APIs remain available.' })
    } elseif ($verified -contains $framework) {
        $rows.Add([ordered]@{ framework = $framework; surface = 'span-preview'; compileEvidence = 'release-build'; ownerReview = 'native-verified'; lifetimeEvidence = 'native-verified'; rationale = 'Conditional Span preview with native owner/header lifetime cases verified by MatViewTests on the exact full runtime package.' })
    } elseif ($compileOnly -contains $framework) {
        $rows.Add([ordered]@{ framework = $framework; surface = 'span-preview'; compileEvidence = 'release-build'; ownerReview = 'reviewed'; lifetimeEvidence = 'compile-only'; rationale = 'Conditional Span preview compiles on this TFM; native owner/lifetime execution remains unmeasured and cannot promote the preview.' })
    } else { throw "Unhandled Typed Mat framework: $framework" }
}

$evidence = [ordered]@{
    '$schema' = 'typed-mat-owner-lifetime-matrix.schema.json'
    schemaVersion = 1
    status = 'reviewed'
    sourceCommit = $sourceCommit
    policy = [ordered]@{
        previewStatus = 'conditional-preview'
        stablePromotionAllowed = $false
        ownerModel = 'borrowed-view-retains-mat-owner'
        nativeLifetimeEvidenceFrameworks = @($verified)
        compileOnlyFrameworks = @($compileOnly)
        legacyExcludedFrameworks = @($legacy)
    }
    frameworks = @($rows)
    summary = [ordered]@{
        frameworkCount = 15
        legacyExcludedCount = 8
        compileOnlyCount = 5
        nativeVerifiedCount = 2
        stablePromotionAllowed = $false
    }
    evidence = [ordered]@{
        targetFrameworkCompilation = 'packaging/performance/typed-mat-target-framework-evidence.json'
        nativeLifetime = 'packaging/performance/typed-mat-native-lifetime-evidence.json'
        benchmark = 'packaging/performance/codec-typed-mat-benchmark-evidence.json'
    }
    limitations = @(
        'All 15 package target frameworks have Release compilation evidence; legacy frameworks intentionally exclude the Span preview surface.',
        'Native owner/lifetime execution is verified only on Windows x64 net8.0 and net10.0 with the exact full runtime package.',
        'The matrix keeps MatView and ReadOnlyMatView conditional preview contracts; stable promotion remains false until the unmeasured preview TFMs and broader runner review are closed.'
    )
}
$outputFullPath = if ([IO.Path]::IsPathRooted($OutputPath)) { [IO.Path]::GetFullPath($OutputPath) } else { [IO.Path]::GetFullPath((Join-Path $repo ($OutputPath -replace '/', [IO.Path]::DirectorySeparatorChar))) }
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $outputFullPath) | Out-Null
[IO.File]::WriteAllText($outputFullPath, (($evidence | ConvertTo-Json -Depth 12) + [Environment]::NewLine), [Text.UTF8Encoding]::new($false))
Write-Host "TYPED_MAT_OWNER_LIFETIME_MATRIX_MEASURED source_commit=$sourceCommit frameworks=15 legacy_excluded=8 compile_only=5 native_verified=2 output=$outputFullPath"

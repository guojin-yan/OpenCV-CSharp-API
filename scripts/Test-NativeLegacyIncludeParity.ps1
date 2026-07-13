param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$nativeRoot = Join-Path $repo "src/OpenCvSharp.Native"
$includeRoot = Join-Path $nativeRoot "include"
$neutralRoot = Join-Path $includeRoot "open_cv_sharp"
$legacyRoot = Join-Path $includeRoot "open_cv_5_sharp"
$legacyNamesPath = Join-Path $legacyRoot "legacy_names.h"
$sourceSmokePath = Join-Path $nativeRoot "tests/legacy_source_compat_smoke.cpp"

foreach ($requiredPath in @($neutralRoot, $legacyRoot, $legacyNamesPath, $sourceSmokePath)) {
    if (-not (Test-Path -LiteralPath $requiredPath)) {
        throw "Required native compatibility path was not found: $requiredPath"
    }
}

function Get-RelativeHeaderPaths {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Root,
        [switch]$ExcludeLegacyNames
    )

    Get-ChildItem -LiteralPath $Root -Recurse -File -Filter "*.h" |
        Where-Object { -not ($ExcludeLegacyNames -and $_.Name -eq "legacy_names.h") } |
        ForEach-Object {
            [System.IO.Path]::GetRelativePath($Root, $_.FullName).Replace("\", "/")
        } |
        Sort-Object
}

$neutralHeaders = @(Get-RelativeHeaderPaths -Root $neutralRoot)
$legacyHeaders = @(Get-RelativeHeaderPaths -Root $legacyRoot -ExcludeLegacyNames)
$sourceSmokeText = [System.IO.File]::ReadAllText($sourceSmokePath)
$violations = [System.Collections.Generic.List[object]]::new()

foreach ($relativeHeader in $neutralHeaders) {
    if ($relativeHeader -notin $legacyHeaders) {
        $violations.Add([pscustomobject]@{
            Path = $relativeHeader
            Issue = "Missing legacy wrapper header"
        })
    }
}

foreach ($relativeHeader in $legacyHeaders) {
    if ($relativeHeader -notin $neutralHeaders) {
        $violations.Add([pscustomobject]@{
            Path = $relativeHeader
            Issue = "Legacy wrapper has no neutral header counterpart"
        })
    }
}

foreach ($relativeHeader in $legacyHeaders) {
    $legacyHeaderPath = Join-Path $legacyRoot ($relativeHeader -replace "/", [System.IO.Path]::DirectorySeparatorChar)
    $text = [System.IO.File]::ReadAllText($legacyHeaderPath)
    $neutralInclude = "#include `"open_cv_sharp/$relativeHeader`""
    $legacyNamesInclude = "#include `"open_cv_5_sharp/legacy_names.h`""

    if (-not $text.Contains("#pragma once")) {
        $violations.Add([pscustomobject]@{
            Path = $relativeHeader
            Issue = "Legacy wrapper is missing #pragma once"
        })
    }

    $neutralIndex = $text.IndexOf($neutralInclude, [System.StringComparison]::Ordinal)
    if ($neutralIndex -lt 0) {
        $violations.Add([pscustomobject]@{
            Path = $relativeHeader
            Issue = "Legacy wrapper is missing neutral include '$neutralInclude'"
        })
    }

    $legacyNamesIndex = $text.IndexOf($legacyNamesInclude, [System.StringComparison]::Ordinal)
    if ($legacyNamesIndex -lt 0) {
        $violations.Add([pscustomobject]@{
            Path = $relativeHeader
            Issue = "Legacy wrapper is missing generated aliases include"
        })
    }

    if ($neutralIndex -ge 0 -and $legacyNamesIndex -ge 0 -and $neutralIndex -gt $legacyNamesIndex) {
        $violations.Add([pscustomobject]@{
            Path = $relativeHeader
            Issue = "Legacy wrapper must include the neutral header before generated aliases"
        })
    }

    $sourceSmokeInclude = "#include `"open_cv_5_sharp/$relativeHeader`""
    if (-not $sourceSmokeText.Contains($sourceSmokeInclude)) {
        $violations.Add([pscustomobject]@{
            Path = $relativeHeader
            Issue = "Legacy source compatibility smoke is missing '$sourceSmokeInclude'"
        })
    }
}

if ($violations.Count -gt 0) {
    Write-Host "Native legacy include parity guard failed with $($violations.Count) issue(s)."
    $violations |
        Sort-Object Path, Issue |
        Format-Table Path, Issue -AutoSize
    exit 1
}

Write-Host (
    "Native legacy include parity guard passed. " +
    "Neutral headers: $($neutralHeaders.Count); " +
    "legacy wrappers: $($legacyHeaders.Count); " +
    "source-smoke includes: $($legacyHeaders.Count).")

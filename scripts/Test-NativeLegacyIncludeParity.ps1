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
$nativeSmokePath = Join-Path $nativeRoot "tests/native_smoke.cpp"
$legacyAbiPath = Join-Path $nativeRoot "generated/legacy_abi.cpp"
$readmePath = Join-Path $repo "README.md"
$contributingPath = Join-Path $repo "CONTRIBUTING.md"
$namingGuidePath = Join-Path $repo "docs/articles/version-neutral-naming-guide.md"

foreach ($requiredPath in @(
        $neutralRoot,
        $legacyRoot,
        $legacyNamesPath,
        $sourceSmokePath,
        $nativeSmokePath,
        $legacyAbiPath,
        $readmePath,
        $contributingPath,
        $namingGuidePath)) {
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
$nativeSmokeText = [System.IO.File]::ReadAllText($nativeSmokePath)
$legacyAbiText = [System.IO.File]::ReadAllText($legacyAbiPath)
$violations = [System.Collections.Generic.List[object]]::new()
$neutralIncludeDirectivePattern = '(?m)^\s*#\s*include\s*[<"]open_cv_sharp/'
$legacyIncludeDirectivePattern = '(?m)^\s*#\s*include\s*[<"]open_cv_5_sharp/'

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

if (-not [regex]::IsMatch($nativeSmokeText, $neutralIncludeDirectivePattern)) {
    $violations.Add([pscustomobject]@{
        Path = [System.IO.Path]::GetRelativePath($repo, $nativeSmokePath).Replace("\", "/")
        Issue = "Native smoke must include current wrapper headers through open_cv_sharp"
    })
}

if ([regex]::IsMatch($nativeSmokeText, $legacyIncludeDirectivePattern)) {
    $violations.Add([pscustomobject]@{
        Path = [System.IO.Path]::GetRelativePath($repo, $nativeSmokePath).Replace("\", "/")
        Issue = "Native smoke must not include compatibility headers through open_cv_5_sharp"
    })
}

if (-not [regex]::IsMatch($legacyAbiText, $neutralIncludeDirectivePattern)) {
    $violations.Add([pscustomobject]@{
        Path = [System.IO.Path]::GetRelativePath($repo, $legacyAbiPath).Replace("\", "/")
        Issue = "Generated ABI forwarding unit must include neutral headers through open_cv_sharp"
    })
}

if ([regex]::IsMatch($legacyAbiText, $legacyIncludeDirectivePattern)) {
    $violations.Add([pscustomobject]@{
        Path = [System.IO.Path]::GetRelativePath($repo, $legacyAbiPath).Replace("\", "/")
        Issue = "Generated ABI forwarding unit must not include compatibility headers through open_cv_5_sharp"
    })
}

$nativeCodeExtensions = @(".c", ".cc", ".cpp", ".cxx", ".h", ".hh", ".hpp", ".hxx")
$primaryIncludeRoots = @(
    (Join-Path $nativeRoot "src")
    (Join-Path $nativeRoot "generated")
    (Join-Path $nativeRoot "tests")
)
$primaryNativeFiles = @(
    foreach ($root in $primaryIncludeRoots) {
        Get-ChildItem -LiteralPath $root -Recurse -File |
            Where-Object { $_.Extension -in $nativeCodeExtensions }
    }
)

foreach ($file in $primaryNativeFiles) {
    if ($file.FullName -eq $sourceSmokePath) {
        continue
    }

    $text = [System.IO.File]::ReadAllText($file.FullName)
    if ([regex]::IsMatch($text, $legacyIncludeDirectivePattern)) {
        $violations.Add([pscustomobject]@{
            Path = [System.IO.Path]::GetRelativePath($repo, $file.FullName).Replace("\", "/")
            Issue = "Current native source/test/generated code must include open_cv_sharp; open_cv_5_sharp is legacy-source smoke only"
        })
    }
}

$documentationExpectations = @(
    [pscustomobject]@{
        Path = $readmePath
        Name = "README"
    },
    [pscustomobject]@{
        Path = $contributingPath
        Name = "CONTRIBUTING"
    },
    [pscustomobject]@{
        Path = $namingGuidePath
        Name = "version-neutral naming guide"
    }
)

foreach ($doc in $documentationExpectations) {
    $text = [System.IO.File]::ReadAllText($doc.Path)
    if (-not $text.Contains("open_cv_sharp")) {
        $violations.Add([pscustomobject]@{
            Path = [System.IO.Path]::GetRelativePath($repo, $doc.Path).Replace("\", "/")
            Issue = "$($doc.Name) must document open_cv_sharp as the primary native include tree"
        })
    }

    if (-not ($text.Contains("open_cv_5_sharp") -and
            $text -match "(?is)(open_cv_5_sharp.{0,200}compatib|compatib.{0,200}open_cv_5_sharp)")) {
        $violations.Add([pscustomobject]@{
            Path = [System.IO.Path]::GetRelativePath($repo, $doc.Path).Replace("\", "/")
            Issue = "$($doc.Name) must document open_cv_5_sharp only in compatibility context"
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
    "source-smoke includes: $($legacyHeaders.Count); " +
    "primary native files scanned: $($primaryNativeFiles.Count).")

param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$fallbackPackCommand = "Pack-Runtime.ps1 -StageRuntime -OpenCvNativeRuntimeDir <runtime-native-dir>"
$runtimePackageShape = "JYPPX.OpenCV.runtime.<rid>"
$runtimeMiniPackageShape = "JYPPX.OpenCV.runtime.<rid>.mini"
$currentRuntimeProject = "packaging/runtime/JYPPX.OpenCV.runtime"
$oldRootIdentity = "OpenCV-CSharp-API-opencv" + "5.x"

function Add-Violation {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.List[object]]$Violations,
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [int]$Line = 0,
        [Parameter(Mandatory = $true)]
        [string]$Issue,
        [string]$Text = ""
    )

    $Violations.Add([pscustomobject]@{
        Path = $Path
        Line = $Line
        Issue = $Issue
        Text = $Text.Trim()
    })
}

function Read-RequiredText {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RelativePath
    )

    $path = Join-Path $repo $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Required runtime fallback command/path file was not found: $RelativePath"
    }

    return [System.IO.File]::ReadAllText($path)
}

function Assert-Contains {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.List[object]]$Violations,
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [Parameter(Mandatory = $true)]
        [string]$Text,
        [Parameter(Mandatory = $true)]
        [string]$Needle,
        [Parameter(Mandatory = $true)]
        [string]$Issue
    )

    if ($Text.IndexOf($Needle, [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
        Add-Violation -Violations $Violations -Path $Path -Issue $Issue -Text $Needle
    }
}

$auditedPaths = @(
    "README.md",
    "docs/articles/quick-start.md",
    "docs/articles/linked-runtime-build-guide.md",
    "docs/articles/linked-runtime-smoke-guide.md",
    "docs/articles/smoke-profiles-guide.md",
    "docs/articles/runtime-licenses.md",
    "docs/articles/native-module-boundary.md",
    "docs/articles/version-neutral-naming-guide.md",
    "packaging/runtime/JYPPX.OpenCV.runtime/README.md",
    ".github/ISSUE_TEMPLATE/bug_report.yml",
    "CONTRIBUTING.md",
    "scripts/Build-OpenCV.ps1",
    "scripts/Stage-Runtime.ps1",
    "scripts/Pack-Runtime.ps1",
    "scripts/Test-ProjectInvariants.ps1"
)

$texts = @{}
foreach ($path in $auditedPaths) {
    $texts[$path] = Read-RequiredText -RelativePath $path
}

$violations = [System.Collections.Generic.List[object]]::new()

$localFallbackDocs = @(
    "README.md",
    "docs/articles/quick-start.md",
    "docs/articles/linked-runtime-build-guide.md",
    "docs/articles/linked-runtime-smoke-guide.md",
    "docs/articles/smoke-profiles-guide.md",
    "packaging/runtime/JYPPX.OpenCV.runtime/README.md",
    ".github/ISSUE_TEMPLATE/bug_report.yml",
    "CONTRIBUTING.md"
)

foreach ($path in $localFallbackDocs) {
    Assert-Contains -Violations $violations -Path $path -Text $texts[$path] -Needle "no matching" -Issue "$path must preserve the no-matching-runtime-package fallback trigger"
    Assert-Contains -Violations $violations -Path $path -Text $texts[$path] -Needle "local native runtime" -Issue "$path must describe the fallback as a local native runtime route"
    Assert-Contains -Violations $violations -Path $path -Text $texts[$path] -Needle "Build-OpenCV.ps1" -Issue "$path must keep Build-OpenCV.ps1 in the local fallback route"
    Assert-Contains -Violations $violations -Path $path -Text $texts[$path] -Needle "Stage-Runtime.ps1" -Issue "$path must keep Stage-Runtime.ps1 in the local fallback route"
    Assert-Contains -Violations $violations -Path $path -Text $texts[$path] -Needle "OpenCvNativeRuntimeDir" -Issue "$path must keep OpenCvNativeRuntimeDir as the preferred local fallback property"
}

$packFallbackDocs = @(
    "README.md",
    "docs/articles/quick-start.md",
    "docs/articles/linked-runtime-build-guide.md",
    "packaging/runtime/JYPPX.OpenCV.runtime/README.md",
    "CONTRIBUTING.md",
    "docs/articles/version-neutral-naming-guide.md"
)

foreach ($path in $packFallbackDocs) {
    Assert-Contains -Violations $violations -Path $path -Text $texts[$path] -Needle $fallbackPackCommand -Issue "$path must keep the complete neutral pack fallback command"
}

foreach ($path in @(
        "README.md",
        "docs/articles/quick-start.md",
        "docs/articles/linked-runtime-build-guide.md",
        "docs/articles/runtime-licenses.md",
        "packaging/runtime/JYPPX.OpenCV.runtime/README.md")) {
    Assert-Contains -Violations $violations -Path $path -Text $texts[$path] -Needle $currentRuntimeProject -Issue "$path must identify the generic runtime package project before fallback"
    Assert-Contains -Violations $violations -Path $path -Text $texts[$path] -Needle $runtimePackageShape -Issue "$path must keep the generic full runtime package shape visible"
    Assert-Contains -Violations $violations -Path $path -Text $texts[$path] -Needle $runtimeMiniPackageShape -Issue "$path must keep the generic mini runtime package shape visible"
}

Assert-Contains -Violations $violations -Path "scripts/Build-OpenCV.ps1" -Text $texts["scripts/Build-OpenCV.ps1"] -Needle 'Join-Path $WorkspaceRoot "opencv-source"' -Issue "Build-OpenCV.ps1 must prefer the version-neutral opencv-source workspace root"
Assert-Contains -Violations $violations -Path "scripts/Build-OpenCV.ps1" -Text $texts["scripts/Build-OpenCV.ps1"] -Needle 'return Join-Path $WorkspaceRoot "opencv-source"' -Issue "Build-OpenCV.ps1 must use only the version-neutral opencv-source workspace root"
Assert-Contains -Violations $violations -Path "scripts/Stage-Runtime.ps1" -Text $texts["scripts/Stage-Runtime.ps1"] -Needle 'Join-Path $WorkspaceRoot "opencv-source"' -Issue "Stage-Runtime.ps1 must prefer the version-neutral opencv-source workspace root"
Assert-Contains -Violations $violations -Path "scripts/Stage-Runtime.ps1" -Text $texts["scripts/Stage-Runtime.ps1"] -Needle 'return Join-Path $WorkspaceRoot "opencv-source"' -Issue "Stage-Runtime.ps1 must use only the version-neutral opencv-source workspace root"
Assert-Contains -Violations $violations -Path "scripts/Stage-Runtime.ps1" -Text $texts["scripts/Stage-Runtime.ps1"] -Needle '[string]$OpenCvNativeRuntimeDir = ""' -Issue "Stage-Runtime.ps1 must expose OpenCvNativeRuntimeDir as the neutral runtime input"
Assert-Contains -Violations $violations -Path "scripts/Stage-Runtime.ps1" -Text $texts["scripts/Stage-Runtime.ps1"] -Needle 'build\native-opencv-core\Release' -Issue "Stage-Runtime.ps1 must keep the neutral local native wrapper output fallback path"
Assert-Contains -Violations $violations -Path "scripts/Pack-Runtime.ps1" -Text $texts["scripts/Pack-Runtime.ps1"] -Needle '[string]$OpenCvNativeRuntimeDir = ""' -Issue "Pack-Runtime.ps1 must expose OpenCvNativeRuntimeDir as the neutral runtime input"
Assert-Contains -Violations $violations -Path "scripts/Pack-Runtime.ps1" -Text $texts["scripts/Pack-Runtime.ps1"] -Needle 'OpenCvNativeRuntimeDir is required when StageRuntime is set' -Issue "Pack-Runtime.ps1 must require OpenCvNativeRuntimeDir when StageRuntime is set"
Assert-Contains -Violations $violations -Path "scripts/Test-ProjectInvariants.ps1" -Text $texts["scripts/Test-ProjectInvariants.ps1"] -Needle "Test-RuntimeFallbackCommandPathConsistency.ps1" -Issue "Aggregate invariant suite must include runtime fallback command/path consistency guard"

foreach ($path in $auditedPaths) {
    $lineNumber = 0
    foreach ($line in [System.IO.File]::ReadLines((Join-Path $repo $path))) {
        $lineNumber++

        if ($line.IndexOf($oldRootIdentity, [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
            Add-Violation -Violations $violations -Path $path -Line $lineNumber -Issue "Runtime fallback surfaces must not use the old fixed-major repository root" -Text $line
        }

        if ($line.IndexOf("OpenCv5SharpNativeRuntimeDir", [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
            Add-Violation -Violations $violations -Path $path -Line $lineNumber -Issue "Retired fixed-major runtime property must not remain" -Text $line
        }

        if ($line.IndexOf("OPENCV5SHARP_", [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
            Add-Violation -Violations $violations -Path $path -Line $lineNumber -Issue "Retired fixed-major environment variable must not remain" -Text $line
        }

        if ($line -match "(?<!OpenCv)-NativeRuntimeDir") {
            Add-Violation -Violations $violations -Path $path -Line $lineNumber -Issue "Retired NativeRuntimeDir parameter alias must not remain" -Text $line
        }
    }
}

if ($violations.Count -gt 0) {
    Write-Host "Runtime fallback command/path consistency guard failed with $($violations.Count) violation(s)."
    $violations |
        Sort-Object Path, Line, Issue |
        Format-Table Path, Line, Issue, Text -AutoSize
    exit 1
}

Write-Host "Runtime fallback command/path consistency guard passed."
Write-Host "Local fallback docs checked: $($localFallbackDocs.Count)."
Write-Host "Pack fallback docs checked: $($packFallbackDocs.Count)."
Write-Host "Neutral pack fallback command: $fallbackPackCommand."

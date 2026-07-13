param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$runtimePackagePrefix = "JYPPX.OpenCV.runtime"
$runtimePackageShape = "$runtimePackagePrefix.<rid>"
$currentRuntimePackage = "$runtimePackagePrefix.win-x64"

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
        throw "Required runtime package availability/fallback file was not found: $RelativePath"
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
        Add-Violation -Violations $Violations -Path $Path -Issue $Issue
    }
}

$violations = [System.Collections.Generic.List[object]]::new()

$runtimeRoot = Join-Path $repo "packaging/runtime"
if (-not (Test-Path -LiteralPath $runtimeRoot -PathType Container)) {
    throw "Runtime package root was not found: packaging/runtime"
}

$runtimePackageProjects = @(
    Get-ChildItem -LiteralPath $runtimeRoot -Directory |
        Where-Object { $_.Name.StartsWith("$runtimePackagePrefix.", [System.StringComparison]::OrdinalIgnoreCase) } |
        Select-Object -ExpandProperty Name
)
if ($runtimePackageProjects.Count -eq 0) {
    Add-Violation -Violations $violations -Path "packaging/runtime" -Issue "At least one tracked runtime package project must exist before claiming runtime package availability"
}
if ($runtimePackageProjects -notcontains $currentRuntimePackage) {
    Add-Violation -Violations $violations -Path "packaging/runtime" -Issue "Current Windows x64 runtime package project must remain tracked until availability docs change"
}

$readmePath = "README.md"
$contributingPath = "CONTRIBUTING.md"
$quickStartPath = "docs/articles/quick-start.md"
$linkedRuntimeBuildGuidePath = "docs/articles/linked-runtime-build-guide.md"
$linkedRuntimeSmokeGuidePath = "docs/articles/linked-runtime-smoke-guide.md"
$smokeProfilesGuidePath = "docs/articles/smoke-profiles-guide.md"
$runtimeLicensesPath = "docs/articles/runtime-licenses.md"
$versionNeutralGuidePath = "docs/articles/version-neutral-naming-guide.md"
$bugTemplatePath = ".github/ISSUE_TEMPLATE/bug_report.yml"
$runtimeReadmePath = "packaging/runtime/JYPPX.OpenCV.runtime.win-x64/README.md"

$readmeText = Read-RequiredText -RelativePath $readmePath
$contributingText = Read-RequiredText -RelativePath $contributingPath
$quickStartText = Read-RequiredText -RelativePath $quickStartPath
$linkedRuntimeBuildGuideText = Read-RequiredText -RelativePath $linkedRuntimeBuildGuidePath
$linkedRuntimeSmokeGuideText = Read-RequiredText -RelativePath $linkedRuntimeSmokeGuidePath
$smokeProfilesGuideText = Read-RequiredText -RelativePath $smokeProfilesGuidePath
$runtimeLicensesText = Read-RequiredText -RelativePath $runtimeLicensesPath
$versionNeutralGuideText = Read-RequiredText -RelativePath $versionNeutralGuidePath
$bugTemplateText = Read-RequiredText -RelativePath $bugTemplatePath
$runtimeReadmeText = Read-RequiredText -RelativePath $runtimeReadmePath

foreach ($doc in @(
        [pscustomobject]@{ Path = $readmePath; Text = $readmeText },
        [pscustomobject]@{ Path = $quickStartPath; Text = $quickStartText },
        [pscustomobject]@{ Path = $linkedRuntimeBuildGuidePath; Text = $linkedRuntimeBuildGuideText },
        [pscustomobject]@{ Path = $runtimeLicensesPath; Text = $runtimeLicensesText },
        [pscustomobject]@{ Path = $runtimeReadmePath; Text = $runtimeReadmeText })) {
    Assert-Contains -Violations $violations -Path $doc.Path -Text $doc.Text -Needle "Currently tracked runtime package project" -Issue "$($doc.Path) must identify the currently tracked runtime package project"
    Assert-Contains -Violations $violations -Path $doc.Path -Text $doc.Text -Needle $currentRuntimePackage -Issue "$($doc.Path) must name the current Windows x64 runtime package"
    Assert-Contains -Violations $violations -Path $doc.Path -Text $doc.Text -Needle $runtimePackageShape -Issue "$($doc.Path) must keep the generic runtime package shape visible"
}

foreach ($doc in @(
        [pscustomobject]@{ Path = $readmePath; Text = $readmeText },
        [pscustomobject]@{ Path = $quickStartPath; Text = $quickStartText },
        [pscustomobject]@{ Path = $linkedRuntimeBuildGuidePath; Text = $linkedRuntimeBuildGuideText },
        [pscustomobject]@{ Path = $linkedRuntimeSmokeGuidePath; Text = $linkedRuntimeSmokeGuideText },
        [pscustomobject]@{ Path = $smokeProfilesGuidePath; Text = $smokeProfilesGuideText },
        [pscustomobject]@{ Path = $bugTemplatePath; Text = $bugTemplateText })) {
    Assert-Contains -Violations $violations -Path $doc.Path -Text $doc.Text -Needle "If no matching runtime package is available yet" -Issue "$($doc.Path) must provide a no-matching-runtime-package fallback"
    Assert-Contains -Violations $violations -Path $doc.Path -Text $doc.Text -Needle "local native runtime" -Issue "$($doc.Path) must describe the fallback as local native runtime usage"
    Assert-Contains -Violations $violations -Path $doc.Path -Text $doc.Text -Needle "Build-OpenCV.ps1" -Issue "$($doc.Path) must point fallback users to Build-OpenCV.ps1"
    Assert-Contains -Violations $violations -Path $doc.Path -Text $doc.Text -Needle "Stage-Runtime.ps1" -Issue "$($doc.Path) must point fallback users to Stage-Runtime.ps1"
    Assert-Contains -Violations $violations -Path $doc.Path -Text $doc.Text -Needle "OpenCvNativeRuntimeDir" -Issue "$($doc.Path) must prefer OpenCvNativeRuntimeDir for local runtime fallback"
}

foreach ($doc in @(
        [pscustomobject]@{ Path = $readmePath; Text = $readmeText },
        [pscustomobject]@{ Path = $quickStartPath; Text = $quickStartText },
        [pscustomobject]@{ Path = $linkedRuntimeBuildGuidePath; Text = $linkedRuntimeBuildGuideText },
        [pscustomobject]@{ Path = $contributingPath; Text = $contributingText },
        [pscustomobject]@{ Path = $versionNeutralGuidePath; Text = $versionNeutralGuideText })) {
    Assert-Contains -Violations $violations -Path $doc.Path -Text $doc.Text -Needle "-OpenCvNativeRuntimeDir" -Issue "$($doc.Path) must document the neutral pack/stage fallback parameter"
}

Assert-Contains -Violations $violations -Path $contributingPath -Text $contributingText -Needle "do not describe future RID packages as published" -Issue "CONTRIBUTING must prevent overclaiming future RID package availability"
Assert-Contains -Violations $violations -Path $contributingPath -Text $contributingText -Needle "package project and release artifact exist" -Issue "CONTRIBUTING must require package project and release artifact evidence before future RID availability claims"
Assert-Contains -Violations $violations -Path $versionNeutralGuidePath -Text $versionNeutralGuideText -Needle "Test-RuntimePackageAvailabilityFallbackGuidance.ps1" -Issue "Version-neutral naming guide must list the availability/fallback guard"

$availabilityFiles = @(
    $readmePath,
    $contributingPath,
    $quickStartPath,
    $linkedRuntimeBuildGuidePath,
    $linkedRuntimeSmokeGuidePath,
    $smokeProfilesGuidePath,
    $runtimeLicensesPath,
    $versionNeutralGuidePath,
    $bugTemplatePath,
    $runtimeReadmePath
)
$futureRidRegex = [System.Text.RegularExpressions.Regex]::new(
    "\b(?:linux-x64|linux-arm64|osx-x64|osx-arm64|win-arm64)\b",
    [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
$futureRidContextRegex = [System.Text.RegularExpressions.Regex]::new(
    "future|planned|when available|when added|not currently tracked|no matching runtime package|target RID|未来|计划|可用时|尚未|目标",
    [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)

foreach ($relativePath in $availabilityFiles) {
    $path = Join-Path $repo $relativePath
    $lineNumber = 0
    foreach ($line in [System.IO.File]::ReadLines($path)) {
        $lineNumber++
        if ($futureRidRegex.IsMatch($line) -and -not $futureRidContextRegex.IsMatch($line)) {
            Add-Violation `
                -Violations $violations `
                -Path $relativePath `
                -Line $lineNumber `
                -Issue "Non-win-x64 RID mentions must be future/planned/when-available or target-RID scoped unless a runtime package project exists" `
                -Text $line
        }
    }
}

if ($violations.Count -gt 0) {
    Write-Host "Runtime package availability/fallback guidance guard failed with $($violations.Count) violation(s)."
    $violations |
        Sort-Object Path, Line, Issue |
        Format-Table Path, Line, Issue, Text -AutoSize
    exit 1
}

Write-Host "Runtime package availability/fallback guidance guard passed."
Write-Host "Tracked runtime package projects: $($runtimePackageProjects -join ', ')."
Write-Host "Runtime package shape: $runtimePackageShape."
Write-Host "Current Windows x64 runtime package: $currentRuntimePackage."

param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path

function Add-Violation {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.List[object]]$Violations,
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [Parameter(Mandatory = $true)]
        [string]$Issue,
        [string]$Text = ""
    )

    $Violations.Add([pscustomobject]@{
        Path = $Path
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
        throw "Required runtime package documentation file was not found: $RelativePath"
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

$violations = [System.Collections.Generic.List[object]]::new()

$readmePath = "README.md"
$quickStartPath = "docs/articles/quick-start.md"
$linkedRuntimeBuildGuidePath = "docs/articles/linked-runtime-build-guide.md"
$linkedRuntimeSmokeGuidePath = "docs/articles/linked-runtime-smoke-guide.md"
$smokeProfilesGuidePath = "docs/articles/smoke-profiles-guide.md"
$runtimeLicensesPath = "docs/articles/runtime-licenses.md"
$nativeModuleBoundaryPath = "docs/articles/native-module-boundary.md"
$versionNeutralGuidePath = "docs/articles/version-neutral-naming-guide.md"
$tocPath = "docs/toc.yml"
$runtimeReadmePath = "packaging/runtime/JYPPX.OpenCV.runtime/README.md"
$bugTemplatePath = ".github/ISSUE_TEMPLATE/bug_report.yml"
$contributingPath = "CONTRIBUTING.md"

$texts = @{}
foreach ($path in @(
        $readmePath,
        $quickStartPath,
        $linkedRuntimeBuildGuidePath,
        $linkedRuntimeSmokeGuidePath,
        $smokeProfilesGuidePath,
        $runtimeLicensesPath,
        $nativeModuleBoundaryPath,
        $versionNeutralGuidePath,
        $tocPath,
        $runtimeReadmePath,
        $bugTemplatePath,
        $contributingPath)) {
    $texts[$path] = Read-RequiredText -RelativePath $path
}

$runtimePackageShape = "JYPPX.OpenCV.runtime.<rid>"
$runtimeMiniPackageShape = "JYPPX.OpenCV.runtime.<rid>.mini"
$currentRuntimeProject = "packaging/runtime/JYPPX.OpenCV.runtime"

foreach ($entry in @(
        [pscustomobject]@{ Text = "href: articles/quick-start.md"; Issue = "Docs TOC must expose Quick Start" },
        [pscustomobject]@{ Text = "href: articles/linked-runtime-build-guide.md"; Issue = "Docs TOC must expose linked runtime build/fallback guidance" },
        [pscustomobject]@{ Text = "href: articles/linked-runtime-smoke-guide.md"; Issue = "Docs TOC must expose linked runtime smoke guidance" },
        [pscustomobject]@{ Text = "href: articles/smoke-profiles-guide.md"; Issue = "Docs TOC must expose smoke profiles guidance" },
        [pscustomobject]@{ Text = "href: articles/runtime-licenses.md"; Issue = "Docs TOC must expose runtime license guidance" },
        [pscustomobject]@{ Text = "href: articles/native-module-boundary.md"; Issue = "Docs TOC must expose native module boundary guidance" },
        [pscustomobject]@{ Text = "href: articles/version-neutral-naming-guide.md"; Issue = "Docs TOC must expose version-neutral naming guidance" })) {
    Assert-Contains -Violations $violations -Path $tocPath -Text $texts[$tocPath] -Needle $entry.Text -Issue $entry.Issue
}

foreach ($entry in @(
        [pscustomobject]@{ Path = $readmePath; Needles = @("docs/articles/quick-start.md", "docs/articles/linked-runtime-build-guide.md", "docs/articles/linked-runtime-smoke-guide.md", "docs/articles/smoke-profiles-guide.md", "docs/articles/runtime-licenses.md", "packaging/runtime/JYPPX.OpenCV.runtime/README.md") },
        [pscustomobject]@{ Path = $quickStartPath; Needles = @("linked-runtime-build-guide.md", "linked-runtime-smoke-guide.md", "smoke-profiles-guide.md", "runtime-licenses.md", "https://github.com/guojin-yan/OpenCV-CSharp-API/blob/opencv5.x/packaging/runtime/JYPPX.OpenCV.runtime/README.md") },
        [pscustomobject]@{ Path = $linkedRuntimeBuildGuidePath; Needles = @("quick-start.md", "linked-runtime-smoke-guide.md", "smoke-profiles-guide.md", "runtime-licenses.md", "https://github.com/guojin-yan/OpenCV-CSharp-API/blob/opencv5.x/packaging/runtime/JYPPX.OpenCV.runtime/README.md") },
        [pscustomobject]@{ Path = $linkedRuntimeSmokeGuidePath; Needles = @("linked-runtime-build-guide.md", "smoke-profiles-guide.md", "runtime-licenses.md") },
        [pscustomobject]@{ Path = $smokeProfilesGuidePath; Needles = @("linked-runtime-build-guide.md", "linked-runtime-smoke-guide.md") },
        [pscustomobject]@{ Path = $runtimeLicensesPath; Needles = @("quick-start.md", "linked-runtime-build-guide.md", "linked-runtime-smoke-guide.md", "https://github.com/guojin-yan/OpenCV-CSharp-API/blob/opencv5.x/packaging/runtime/JYPPX.OpenCV.runtime/README.md") },
        [pscustomobject]@{ Path = $nativeModuleBoundaryPath; Needles = @("linked-runtime-build-guide.md", "linked-runtime-smoke-guide.md", "runtime-licenses.md", "https://github.com/guojin-yan/OpenCV-CSharp-API/blob/opencv5.x/packaging/runtime/JYPPX.OpenCV.runtime/README.md") },
        [pscustomobject]@{ Path = $runtimeReadmePath; Needles = @("../../../docs/articles/quick-start.md", "../../../docs/articles/linked-runtime-build-guide.md", "../../../docs/articles/linked-runtime-smoke-guide.md", "../../../docs/articles/smoke-profiles-guide.md", "../../../docs/articles/runtime-licenses.md") },
        [pscustomobject]@{ Path = $versionNeutralGuidePath; Needles = @("Test-RuntimePackageDocsDiscoverability.ps1", "linked-runtime-build-guide.md", "linked-runtime-smoke-guide.md", "runtime-licenses.md") })) {
    foreach ($needle in $entry.Needles) {
        Assert-Contains -Violations $violations -Path $entry.Path -Text $texts[$entry.Path] -Needle $needle -Issue "$($entry.Path) must cross-link runtime package guidance through $needle"
    }
}

foreach ($entry in @(
        [pscustomobject]@{ Path = $quickStartPath },
        [pscustomobject]@{ Path = $linkedRuntimeBuildGuidePath },
        [pscustomobject]@{ Path = $linkedRuntimeSmokeGuidePath },
        [pscustomobject]@{ Path = $smokeProfilesGuidePath },
        [pscustomobject]@{ Path = $runtimeLicensesPath },
        [pscustomobject]@{ Path = $runtimeReadmePath })) {
    Assert-Contains -Violations $violations -Path $entry.Path -Text $texts[$entry.Path] -Needle $runtimePackageShape -Issue "$($entry.Path) must keep generic full runtime package shape discoverable"
    Assert-Contains -Violations $violations -Path $entry.Path -Text $texts[$entry.Path] -Needle $runtimeMiniPackageShape -Issue "$($entry.Path) must keep generic mini runtime package shape discoverable"
}

foreach ($entry in @(
        [pscustomobject]@{ Path = $quickStartPath },
        [pscustomobject]@{ Path = $linkedRuntimeBuildGuidePath },
        [pscustomobject]@{ Path = $runtimeLicensesPath },
        [pscustomobject]@{ Path = $runtimeReadmePath })) {
    Assert-Contains -Violations $violations -Path $entry.Path -Text $texts[$entry.Path] -Needle $currentRuntimeProject -Issue "$($entry.Path) must identify the generic runtime package project"
}

foreach ($entry in @(
        [pscustomobject]@{ Path = $quickStartPath },
        [pscustomobject]@{ Path = $linkedRuntimeBuildGuidePath },
        [pscustomobject]@{ Path = $linkedRuntimeSmokeGuidePath },
        [pscustomobject]@{ Path = $smokeProfilesGuidePath },
        [pscustomobject]@{ Path = $bugTemplatePath })) {
    Assert-Contains -Violations $violations -Path $entry.Path -Text $texts[$entry.Path] -Needle "no matching" -Issue "$($entry.Path) must preserve local native runtime fallback discoverability"
    Assert-Contains -Violations $violations -Path $entry.Path -Text $texts[$entry.Path] -Needle "Build-OpenCV.ps1" -Issue "$($entry.Path) must point fallback users to Build-OpenCV.ps1"
    Assert-Contains -Violations $violations -Path $entry.Path -Text $texts[$entry.Path] -Needle "Stage-Runtime.ps1" -Issue "$($entry.Path) must point fallback users to Stage-Runtime.ps1"
    Assert-Contains -Violations $violations -Path $entry.Path -Text $texts[$entry.Path] -Needle "OpenCvNativeRuntimeDir" -Issue "$($entry.Path) must point fallback users to OpenCvNativeRuntimeDir"
}

Assert-Contains -Violations $violations -Path $bugTemplatePath -Text $texts[$bugTemplatePath] -Needle "docs/articles/linked-runtime-build-guide.md" -Issue "Bug template must point reporters to fallback guidance"
Assert-Contains -Violations $violations -Path $bugTemplatePath -Text $texts[$bugTemplatePath] -Needle "docs/articles/linked-runtime-smoke-guide.md" -Issue "Bug template must point reporters to linked smoke guidance"
Assert-Contains -Violations $violations -Path $contributingPath -Text $texts[$contributingPath] -Needle "runtime package docs cross-linked" -Issue "CONTRIBUTING must keep runtime package docs discoverability as a maintenance rule"

if ($violations.Count -gt 0) {
    Write-Host "Runtime package docs discoverability guard failed with $($violations.Count) violation(s)."
    $violations |
        Sort-Object Path, Issue |
        Format-Table Path, Issue, Text -AutoSize
    exit 1
}

Write-Host "Runtime package docs discoverability guard passed."
Write-Host "Runtime docs entry points checked: $($texts.Count)."
Write-Host "Runtime package shape: $runtimePackageShape."
Write-Host "Mini runtime package shape: $runtimeMiniPackageShape."

param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$runtimePackagePrefix = "JYPPX.OpenCV.runtime"
$runtimePackageShape = "$runtimePackagePrefix.<rid>"
$runtimeMiniPackageShape = "$runtimePackagePrefix.<rid>.mini"
$currentRuntimeProject = "packaging/runtime/JYPPX.OpenCV.runtime"
$runtimePackageMatrixPath = "packaging/runtime/runtime-package-matrix.json"
$runtimeMatrixPhrase = "runtime package matrix"

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
        Add-Violation -Violations $Violations -Path $Path -Issue $Issue -Text $Needle
    }
}

$violations = [System.Collections.Generic.List[object]]::new()

$runtimeProjectFullPath = Join-Path $repo $currentRuntimeProject
if (-not (Test-Path -LiteralPath $runtimeProjectFullPath -PathType Container)) {
    Add-Violation -Violations $violations -Path $currentRuntimeProject -Issue "Generic runtime package project must exist before claiming runtime package availability"
}

$runtimeMatrixText = Read-RequiredText -RelativePath $runtimePackageMatrixPath
$runtimeMatrix = $null
try {
    $runtimeMatrix = $runtimeMatrixText | ConvertFrom-Json
}
catch {
    Add-Violation -Violations $violations -Path $runtimePackageMatrixPath -Issue "Runtime package matrix must be valid JSON" -Text $_.Exception.Message
}

if ($null -ne $runtimeMatrix) {
    foreach ($requiredRid in @("win-x64", "win-x86", "win-arm64", "ubuntu.22.04-x64", "ubuntu.24.04-x64", "debian.12-x64", "fedora.40-x64", "rhel.9-x64", "rocky.9-x64", "alpine.3.20-x64", "android-arm64", "android-arm", "android-x64", "android-x86")) {
        $ridSpec = @($runtimeMatrix.rids | Where-Object { $_.rid -eq $requiredRid } | Select-Object -First 1)
        if ($ridSpec.Count -eq 0) {
            Add-Violation -Violations $violations -Path $runtimePackageMatrixPath -Issue "Runtime package matrix must include RID $requiredRid"
        }
    }

    foreach ($requiredProfile in @("full", "mini")) {
        $profileSpec = @($runtimeMatrix.profiles | Where-Object { $_.name -eq $requiredProfile } | Select-Object -First 1)
        if ($profileSpec.Count -eq 0) {
            Add-Violation -Violations $violations -Path $runtimePackageMatrixPath -Issue "Runtime package matrix must include profile $requiredProfile"
        }
    }
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
$runtimeReadmePath = "packaging/runtime/JYPPX.OpenCV.runtime/README.md"

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
    Assert-Contains -Violations $violations -Path $doc.Path -Text $doc.Text -Needle $currentRuntimeProject -Issue "$($doc.Path) must identify the generic runtime package project"
    Assert-Contains -Violations $violations -Path $doc.Path -Text $doc.Text -Needle $runtimePackageShape -Issue "$($doc.Path) must keep the generic full runtime package shape visible"
    Assert-Contains -Violations $violations -Path $doc.Path -Text $doc.Text -Needle $runtimeMiniPackageShape -Issue "$($doc.Path) must keep the generic mini runtime package shape visible"
}

foreach ($doc in @(
        [pscustomobject]@{ Path = $readmePath; Text = $readmeText },
        [pscustomobject]@{ Path = $contributingPath; Text = $contributingText },
        [pscustomobject]@{ Path = $linkedRuntimeBuildGuidePath; Text = $linkedRuntimeBuildGuideText },
        [pscustomobject]@{ Path = $runtimeReadmePath; Text = $runtimeReadmeText })) {
    Assert-Contains -Violations $violations -Path $doc.Path -Text $doc.Text -Needle $runtimeMatrixPhrase -Issue "$($doc.Path) must describe the runtime package matrix"
}

foreach ($doc in @(
        [pscustomobject]@{ Path = $readmePath; Text = $readmeText },
        [pscustomobject]@{ Path = $quickStartPath; Text = $quickStartText },
        [pscustomobject]@{ Path = $linkedRuntimeBuildGuidePath; Text = $linkedRuntimeBuildGuideText },
        [pscustomobject]@{ Path = $linkedRuntimeSmokeGuidePath; Text = $linkedRuntimeSmokeGuideText },
        [pscustomobject]@{ Path = $smokeProfilesGuidePath; Text = $smokeProfilesGuideText },
        [pscustomobject]@{ Path = $bugTemplatePath; Text = $bugTemplateText })) {
    Assert-Contains -Violations $violations -Path $doc.Path -Text $doc.Text -Needle "no matching" -Issue "$($doc.Path) must provide a no-matching-runtime-package fallback"
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

Assert-Contains -Violations $violations -Path $contributingPath -Text $contributingText -Needle "runtime package matrix" -Issue "CONTRIBUTING must require runtime availability docs to track the runtime package matrix"
Assert-Contains -Violations $violations -Path $contributingPath -Text $contributingText -Needle "real publishing requires native wrapper plus OpenCV runtime outputs" -Issue "CONTRIBUTING must require real runtime output evidence before publishing claims"
Assert-Contains -Violations $violations -Path $contributingPath -Text $contributingText -Needle "synthetic runtime inputs are package-surface validation only" -Issue "CONTRIBUTING must prevent publishing synthetic runtime validation artifacts"
Assert-Contains -Violations $violations -Path $versionNeutralGuidePath -Text $versionNeutralGuideText -Needle "Test-RuntimePackageAvailabilityFallbackGuidance.ps1" -Issue "Version-neutral naming guide must list the availability/fallback guard"

foreach ($relativePath in @(
        $readmePath,
        $contributingPath,
        $quickStartPath,
        $linkedRuntimeBuildGuidePath,
        $linkedRuntimeSmokeGuidePath,
        $smokeProfilesGuidePath,
        $runtimeLicensesPath,
        $versionNeutralGuidePath,
        $bugTemplatePath,
        $runtimeReadmePath)) {
    $path = Join-Path $repo $relativePath
    $lineNumber = 0
    foreach ($line in [System.IO.File]::ReadLines($path)) {
        $lineNumber++
        $oldFixedRidRuntimeProjectPath = "packaging/runtime/JYPPX.OpenCV.runtime." + "win-x64"
        if ($line.IndexOf($oldFixedRidRuntimeProjectPath, [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
            Add-Violation -Violations $violations -Path $relativePath -Line $lineNumber -Issue "Runtime docs must not point to the old fixed-RID runtime project directory" -Text $line
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
Write-Host "Runtime package matrix: $runtimePackageMatrixPath."
Write-Host "Runtime package shape: $runtimePackageShape."
Write-Host "Mini runtime package shape: $runtimeMiniPackageShape."

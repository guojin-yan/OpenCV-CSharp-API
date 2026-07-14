param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$producerWorkflowPath = ".github/workflows/runtime-input.yml"
$runtimeInputScriptPath = "scripts/New-RuntimeInputArtifact.ps1"
$packWorkflowPath = ".github/workflows/pack.yml"
$readmePath = "README.md"
$linkedRuntimeBuildGuidePath = "docs/articles/linked-runtime-build-guide.md"
$versionNeutralGuidePath = "docs/articles/version-neutral-naming-guide.md"

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
    param([Parameter(Mandatory = $true)][string]$RelativePath)

    $path = Join-Path $repo $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Required real runtime input producer file was not found: $RelativePath"
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

function Assert-NotContains {
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

    if ($Text.IndexOf($Needle, [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
        Add-Violation -Violations $Violations -Path $Path -Issue $Issue -Text $Needle
    }
}

function Assert-TextOrder {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.List[object]]$Violations,
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [Parameter(Mandatory = $true)]
        [string]$Text,
        [Parameter(Mandatory = $true)]
        [string]$Earlier,
        [Parameter(Mandatory = $true)]
        [string]$Later,
        [Parameter(Mandatory = $true)]
        [string]$Issue
    )

    $earlierIndex = $Text.IndexOf($Earlier, [System.StringComparison]::OrdinalIgnoreCase)
    $laterIndex = $Text.IndexOf($Later, [System.StringComparison]::OrdinalIgnoreCase)
    if ($earlierIndex -lt 0 -or $laterIndex -lt 0 -or $earlierIndex -ge $laterIndex) {
        Add-Violation -Violations $Violations -Path $Path -Issue $Issue -Text "$Earlier before $Later"
    }
}

$violations = [System.Collections.Generic.List[object]]::new()

$producerWorkflowText = Read-RequiredText -RelativePath $producerWorkflowPath
$runtimeInputScriptText = Read-RequiredText -RelativePath $runtimeInputScriptPath
$packWorkflowText = Read-RequiredText -RelativePath $packWorkflowPath
$readmeText = Read-RequiredText -RelativePath $readmePath
$linkedRuntimeBuildGuideText = Read-RequiredText -RelativePath $linkedRuntimeBuildGuidePath
$versionNeutralGuideText = Read-RequiredText -RelativePath $versionNeutralGuidePath

foreach ($required in @(
        [pscustomobject]@{ Needle = "name: runtime-input"; Issue = "Producer workflow must have a neutral runtime-input name" },
        [pscustomobject]@{ Needle = "workflow_dispatch:"; Issue = "Producer workflow must be manually dispatched until real build cost is proven" },
        [pscustomobject]@{ Needle = "default: linux-x64"; Issue = "Producer workflow must start with the first real linux-x64 target" },
        [pscustomobject]@{ Needle = "default: full"; Issue = "Producer workflow must start with the full profile until mini linked-component support is split" },
        [pscustomobject]@{ Needle = "Check project invariants"; Issue = "Producer workflow must run project invariants before building runtime inputs" },
        [pscustomobject]@{ Needle = "runtime-input.yml currently produces only runtime-input-linux-x64-full"; Issue = "Producer workflow must explicitly reject unsupported real producer targets" },
        [pscustomobject]@{ Needle = "git -c advice.detachedHead=false clone --depth 1 --branch"; Issue = "Producer workflow must fetch factual OpenCV source for real runtime inputs" },
        [pscustomobject]@{ Needle = "https://github.com/opencv/opencv.git"; Issue = "Producer workflow must fetch OpenCV from the upstream source repository" },
        [pscustomobject]@{ Needle = "./scripts/Build-OpenCV.ps1"; Issue = "Producer workflow must build OpenCV runtime inputs" },
        [pscustomobject]@{ Needle = "-Build"; Issue = "Producer workflow must run the OpenCV build/install target, not only describe it" },
        [pscustomobject]@{ Needle = "OPENCV_CSHARP_OPENCV_DIR"; Issue = "Producer workflow must link native wrapper against produced OpenCV config" },
        [pscustomobject]@{ Needle = "cmake --build build/native-linked"; Issue = "Producer workflow must build the linked native wrapper" },
        [pscustomobject]@{ Needle = "ctest --test-dir build/native-linked"; Issue = "Producer workflow must test the linked native wrapper" },
        [pscustomobject]@{ Needle = "./scripts/New-RuntimeInputArtifact.ps1"; Issue = "Producer workflow must assemble the agreed handoff layout" },
        [pscustomobject]@{ Needle = 'runtime-input-${{ inputs.rid }}-${{ inputs.runtime_profile }}'; Issue = "Producer workflow must upload neutral runtime-input artifact names" },
        [pscustomobject]@{ Needle = 'artifacts/runtime-inputs/${{ inputs.rid }}-${{ inputs.runtime_profile }}'; Issue = "Producer workflow must upload the agreed runtime-input layout root" })) {
    Assert-Contains -Violations $violations -Path $producerWorkflowPath -Text $producerWorkflowText -Needle $required.Needle -Issue $required.Issue
}

Assert-TextOrder -Violations $violations -Path $producerWorkflowPath -Text $producerWorkflowText -Earlier "Build OpenCV runtime" -Later "Configure linked native wrapper" -Issue "Producer workflow must build OpenCV before configuring the linked native wrapper"
Assert-TextOrder -Violations $violations -Path $producerWorkflowPath -Text $producerWorkflowText -Earlier "Build linked native wrapper" -Later "Create runtime input artifact layout" -Issue "Producer workflow must build native wrapper before assembling the artifact"
Assert-TextOrder -Violations $violations -Path $producerWorkflowPath -Text $producerWorkflowText -Earlier "Create runtime input artifact layout" -Later "Upload runtime input artifact" -Issue "Producer workflow must assemble the artifact before upload"

Assert-NotContains -Violations $violations -Path $producerWorkflowPath -Text $producerWorkflowText -Needle "New-SyntheticRuntimeInputs.ps1" -Issue "Producer workflow must not use synthetic runtime input generation"
Assert-NotContains -Violations $violations -Path $producerWorkflowPath -Text $producerWorkflowText -Needle "publish_github_packages" -Issue "Producer workflow must not publish packages"
Assert-NotContains -Violations $violations -Path $producerWorkflowPath -Text $producerWorkflowText -Needle "dotnet nuget push" -Issue "Producer workflow must not push packages"

foreach ($required in @(
        [pscustomobject]@{ Needle = '[string]$OutputRoot = "artifacts/runtime-inputs"'; Issue = "Runtime input artifact script must use a neutral generated output root" },
        [pscustomobject]@{ Needle = "native-wrapper"; Issue = "Runtime input artifact script must create native-wrapper layout" },
        [pscustomobject]@{ Needle = "opencv-runtime"; Issue = "Runtime input artifact script must create opencv-runtime layout" },
        [pscustomobject]@{ Needle = "opencv-source"; Issue = "Runtime input artifact script must create opencv-source layout" },
        [pscustomobject]@{ Needle = "opencv-install"; Issue = "Runtime input artifact script must create optional opencv-install layout" },
        [pscustomobject]@{ Needle = "SyntheticRuntimeInputs = `$false"; Issue = "Runtime input artifact provenance must mark produced handoff as non-synthetic" },
        [pscustomobject]@{ Needle = "runtime-input.provenance.json"; Issue = "Runtime input artifact script must write handoff provenance" },
        [pscustomobject]@{ Needle = "JYPPX.OpenCV.Native"; Issue = "Runtime input artifact script must require the neutral native loader" },
        [pscustomobject]@{ Needle = '"Open" + "Cv5Sharp.Native" # compatibility loader for already-compiled consumers'; Issue = "Runtime input artifact script must keep compatibility loader explicitly scoped" },
        [pscustomobject]@{ Needle = "OpenCV source LICENSE was not found"; Issue = "Runtime input artifact script must require OpenCV source license evidence" },
        [pscustomobject]@{ Needle = "Runtime input artifact name: runtime-input-`$Rid-`$RuntimeProfile"; Issue = "Runtime input artifact script must print the neutral artifact name" })) {
    Assert-Contains -Violations $violations -Path $runtimeInputScriptPath -Text $runtimeInputScriptText -Needle $required.Needle -Issue $required.Issue
}

Assert-NotContains -Violations $violations -Path $runtimeInputScriptPath -Text $runtimeInputScriptText -Needle "New-SyntheticRuntimeInputs" -Issue "Runtime input artifact script must not call synthetic input generation"

Assert-Contains -Violations $violations -Path $packWorkflowPath -Text $packWorkflowText -Needle "real_runtime_artifact_run_id" -Issue "Pack workflow must keep consuming producer run ids"
Assert-Contains -Violations $violations -Path $packWorkflowPath -Text $packWorkflowText -Needle 'runtime-input-${{ matrix.rid }}-${{ matrix.profile }}' -Issue "Pack workflow must consume the same neutral producer artifact names"

foreach ($doc in @(
        [pscustomobject]@{ Path = $readmePath; Text = $readmeText },
        [pscustomobject]@{ Path = $linkedRuntimeBuildGuidePath; Text = $linkedRuntimeBuildGuideText })) {
    foreach ($needle in @(
            '`runtime-input.yml`',
            '`runtime-input-linux-x64-full`',
            '`runtime-input-<rid>-<profile>`',
            '`native-wrapper/`',
            '`opencv-runtime/`',
            '`opencv-source/`')) {
        Assert-Contains -Violations $violations -Path $doc.Path -Text $doc.Text -Needle $needle -Issue "$($doc.Path) must document real runtime input producer text '$needle'"
    }
}

Assert-Contains -Violations $violations -Path $versionNeutralGuidePath -Text $versionNeutralGuideText -Needle "Test-RealRuntimeInputProducerSurface.ps1" -Issue "Version-neutral guide must list the real runtime input producer guard"

if ($violations.Count -gt 0) {
    Write-Host "Real runtime input producer surface guard failed with $($violations.Count) violation(s)."
    $violations |
        Sort-Object Path, Issue |
        Format-Table Path, Issue, Text -AutoSize
    exit 1
}

Write-Host "Real runtime input producer surface guard passed."
Write-Host "First producer artifact: runtime-input-linux-x64-full."
Write-Host "Producer handoff layout: native-wrapper, opencv-runtime, opencv-source, optional opencv-install."

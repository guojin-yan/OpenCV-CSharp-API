param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$packWorkflowPath = ".github/workflows/pack.yml"
$packRuntimePath = "scripts/Pack-Runtime.ps1"
$stageRuntimePath = "scripts/Stage-Runtime.ps1"
$preflightPath = "scripts/Test-RuntimeReleaseCandidatePreflight.ps1"
$preflightGuardPath = "scripts/Test-RuntimeReleaseCandidatePreflightGuard.ps1"
$readmePath = "README.md"
$contributingPath = "CONTRIBUTING.md"
$linkedRuntimeBuildGuidePath = "docs/articles/linked-runtime-build-guide.md"
$versionNeutralGuidePath = "docs/articles/version-neutral-naming-guide.md"
$runtimePackageReadmePath = "packaging/runtime/JYPPX.OpenCV.runtime/README.md"

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
        throw "Required real runtime pack input boundary file was not found: $RelativePath"
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

$packWorkflowText = Read-RequiredText -RelativePath $packWorkflowPath
$packRuntimeText = Read-RequiredText -RelativePath $packRuntimePath
$stageRuntimeText = Read-RequiredText -RelativePath $stageRuntimePath
$preflightText = Read-RequiredText -RelativePath $preflightPath
$preflightGuardText = Read-RequiredText -RelativePath $preflightGuardPath
$readmeText = Read-RequiredText -RelativePath $readmePath
$contributingText = Read-RequiredText -RelativePath $contributingPath
$linkedRuntimeBuildGuideText = Read-RequiredText -RelativePath $linkedRuntimeBuildGuidePath
$versionNeutralGuideText = Read-RequiredText -RelativePath $versionNeutralGuidePath
$runtimePackageReadmeText = Read-RequiredText -RelativePath $runtimePackageReadmePath

foreach ($required in @(
        [pscustomobject]@{
            Needle = "Existing real native wrapper runtime directory on the selected runner when synthetic validation is false"
            Issue = "pack.yml must describe native_runtime_dir as an existing real runner directory"
        },
        [pscustomobject]@{
            Needle = "Existing real OpenCV runtime directory on the selected runner when synthetic validation is false"
            Issue = "pack.yml must describe opencv_runtime_dir as an existing real runner directory"
        },
        [pscustomobject]@{
            Needle = "Existing real OpenCV source directory on the selected runner when synthetic validation is false"
            Issue = "pack.yml must describe opencv_source_dir as an existing real runner directory"
        },
        [pscustomobject]@{
            Needle = "Optional existing real OpenCV install directory on the selected runner when synthetic validation is false"
            Issue = "pack.yml must describe opencv_install_dir as an optional existing real runner directory"
        },
        [pscustomobject]@{
            Needle = "Reject synthetic publish inputs"
            Issue = "pack.yml must reject synthetic publish requests before packaging"
        },
        [pscustomobject]@{
            Needle = "Validate real runtime input paths"
            Issue = "pack.yml must validate real runtime input paths before real packaging"
        },
        [pscustomobject]@{
            Needle = "real_runtime_artifact_run_id"
            Issue = "pack.yml must expose a real runtime artifact handoff run-id input"
        },
        [pscustomobject]@{
            Needle = "Download real runtime input artifact"
            Issue = "pack.yml must download explicit real runtime input handoff artifacts"
        },
        [pscustomobject]@{
            Needle = 'runtime-input-${{ matrix.rid }}-${{ matrix.profile }}'
            Issue = "pack.yml must use deterministic neutral real runtime input artifact names"
        },
        [pscustomobject]@{
            Needle = 'artifacts/real-runtime-inputs/${{ matrix.rid }}-${{ matrix.profile }}'
            Issue = "pack.yml must download real runtime input artifacts into an isolated generated root"
        },
        [pscustomobject]@{
            Needle = 'pack.yml does not build real runtime inputs; provide paths that already exist on the selected runner or set real_runtime_artifact_run_id to download runtime-input-${{ matrix.rid }}-${{ matrix.profile }} before disabling synthetic validation.'
            Issue = "pack.yml must explain the real-runtime input artifact handoff boundary"
        },
        [pscustomobject]@{
            Needle = "Real runtime packaging requires native_runtime_dir to be an existing directory on the selected runner"
            Issue = "pack.yml must fail early when native_runtime_dir is missing for real packaging"
        },
        [pscustomobject]@{
            Needle = "Real runtime packaging requires opencv_runtime_dir to be an existing directory on the selected runner"
            Issue = "pack.yml must fail early when opencv_runtime_dir is missing for real packaging"
        },
        [pscustomobject]@{
            Needle = "Real runtime packaging requires opencv_source_dir to be an existing directory on the selected runner"
            Issue = "pack.yml must fail early when opencv_source_dir is missing for real packaging"
        },
        [pscustomobject]@{
            Needle = "Real runtime packaging requires opencv_install_dir to be an existing directory when provided"
            Issue = "pack.yml must validate optional opencv_install_dir when provided"
        },
        [pscustomobject]@{
            Needle = "inputs.validate_synthetic_runtime != 'true'"
            Issue = "pack.yml must separate real runtime path validation from synthetic package-shape validation"
        },
        [pscustomobject]@{
            Needle = 'if (''${{ inputs.validate_synthetic_runtime }}'' -ne ''true'')'
            Issue = "pack.yml must switch to workflow-dispatch real runtime path inputs when synthetic validation is disabled"
        },
        [pscustomobject]@{
            Needle = 'steps.real.outputs.native_runtime_dir'
            Issue = "pack.yml must pass resolved real input paths from validation to Pack-Runtime"
        },
        [pscustomobject]@{
            Needle = "'-RequireReleasePreflight'"
            Issue = "pack.yml must pass release preflight before publish-capable runtime package pushes"
        })) {
    Assert-Contains -Violations $violations -Path $packWorkflowPath -Text $packWorkflowText -Needle $required.Needle -Issue $required.Issue
}

Assert-TextOrder `
    -Violations $violations `
    -Path $packWorkflowPath `
    -Text $packWorkflowText `
    -Earlier "Download real runtime input artifact" `
    -Later "Validate real runtime input paths" `
    -Issue "pack.yml must download real runtime input artifacts before validating real runtime paths"

Assert-TextOrder `
    -Violations $violations `
    -Path $packWorkflowPath `
    -Text $packWorkflowText `
    -Earlier "Validate real runtime input paths" `
    -Later "Pack runtime package" `
    -Issue "pack.yml must validate real runtime paths before invoking Pack-Runtime.ps1"

Assert-TextOrder `
    -Violations $violations `
    -Path $packWorkflowPath `
    -Text $packWorkflowText `
    -Earlier "Reject synthetic publish inputs" `
    -Later "pack-managed:" `
    -Issue "pack.yml must reject synthetic publish before managed package jobs can publish"

foreach ($requiredPackRuntimeText in @(
        'if ($RequireReleasePreflight -and $SyntheticRuntimeInputs)',
        'Release-candidate runtime packages require real native runtime inputs; synthetic runtime inputs validate package shape only.',
        'if ($RequireReleasePreflight)',
        'Test-RuntimeReleaseCandidatePreflight.ps1',
        'SyntheticRuntimeInputs')) {
    Assert-Contains `
        -Violations $violations `
        -Path $packRuntimePath `
        -Text $packRuntimeText `
        -Needle $requiredPackRuntimeText `
        -Issue "$packRuntimePath must keep release preflight and synthetic validation separated"
}

foreach ($requiredPreflightText in @(
        '[switch]$AllowSyntheticRuntimeInputs',
        'Release candidate preflight rejects synthetic runtime inputs',
        'Runtime project native mirror must match provenance exactly and contain no stale files',
        'Runtime project license mirror must match provenance exactly and contain no stale files',
        'Runtime provenance input root',
        'SyntheticRuntimeInputs')) {
    Assert-Contains `
        -Violations $violations `
        -Path $preflightPath `
        -Text $preflightText `
        -Needle $requiredPreflightText `
        -Issue "$preflightPath must reject synthetic/stale release candidates by default"
}

foreach ($requiredStageText in @(
        'SyntheticRuntimeInputs = [bool]$SyntheticRuntimeInputs.IsPresent',
        'NativeWrapperRuntimeDir = $nativeRuntimePath.Path',
        'OpenCvRuntimeDir = $openCvRuntimePath.Path',
        'OpenCvSourceDir = $openCvSourcePath.Path')) {
    Assert-Contains `
        -Violations $violations `
        -Path $stageRuntimePath `
        -Text $stageRuntimeText `
        -Needle $requiredStageText `
        -Issue "$stageRuntimePath must record real/synthetic runtime provenance roots"
}

foreach ($requiredGuardText in @(
        'Pack-Runtime -RequireReleasePreflight produces a package only for non-synthetic staged inputs.',
        'Synthetic manifests and stale mirrors are rejected by default.',
        '-SyntheticRuntimeInputs',
        '-RequireReleasePreflight')) {
    Assert-Contains `
        -Violations $violations `
        -Path $preflightGuardPath `
        -Text $preflightGuardText `
        -Needle $requiredGuardText `
        -Issue "$preflightGuardPath must prove release preflight rejects synthetic publish-capable paths"
}

$docNeedles = @(
    '`pack.yml` does not build real runtime inputs',
    'real input paths must already exist on the selected runner or come from `real_runtime_artifact_run_id`',
    '`runtime-input-<rid>-<profile>`',
    'synthetic runtime inputs are package-surface validation only',
    'real publishable runtime packages require `SyntheticRuntimeInputs=false`'
)

foreach ($doc in @(
        [pscustomobject]@{ Path = $readmePath; Text = $readmeText },
        [pscustomobject]@{ Path = $contributingPath; Text = $contributingText },
        [pscustomobject]@{ Path = $linkedRuntimeBuildGuidePath; Text = $linkedRuntimeBuildGuideText },
        [pscustomobject]@{ Path = $runtimePackageReadmePath; Text = $runtimePackageReadmeText })) {
    foreach ($needle in $docNeedles) {
        Assert-Contains `
            -Violations $violations `
            -Path $doc.Path `
            -Text $doc.Text `
            -Needle $needle `
            -Issue "$($doc.Path) must document real runtime input boundary text '$needle'"
    }
}

Assert-Contains `
    -Violations $violations `
    -Path $versionNeutralGuidePath `
    -Text $versionNeutralGuideText `
    -Needle "Test-RealRuntimePackInputBoundary.ps1" `
    -Issue "$versionNeutralGuidePath must list the real runtime pack input boundary guard"

if ($violations.Count -gt 0) {
    Write-Host "Real runtime pack input boundary guard failed with $($violations.Count) violation(s)."
    $violations |
        Sort-Object Path, Issue |
        Format-Table Path, Issue, Text -AutoSize
    exit 1
}

Write-Host "Real runtime pack input boundary guard passed."
Write-Host "Synthetic validation remains non-publishing package-surface evidence only."
Write-Host "Real runtime package inputs must already exist on the selected runner or come from an explicit artifact handoff."

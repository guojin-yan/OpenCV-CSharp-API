param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$violations = [System.Collections.Generic.List[object]]::new()
$contractRelativePath = 'packaging/runtime/runtime-support-contract.json'
$matrixRelativePath = 'packaging/runtime/runtime-package-matrix.json'

function Add-Violation {
    param([Parameter(Mandatory = $true)][string]$Path,[Parameter(Mandatory = $true)][string]$Issue,[string]$Text = '')
    $violations.Add([pscustomobject]@{ Path = $Path; Issue = $Issue; Text = $Text.Trim() })
}

function Assert-True {
    param([Parameter(Mandatory = $true)][bool]$Condition,[Parameter(Mandatory = $true)][string]$Path,[Parameter(Mandatory = $true)][string]$Issue,[string]$Text = '')
    if (-not $Condition) { Add-Violation -Path $Path -Issue $Issue -Text $Text }
}

function Read-RequiredJson {
    param([Parameter(Mandatory = $true)][string]$RelativePath)
    $path = Join-Path $repo $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Required support contract file was not found: $RelativePath" }
    return [pscustomobject]@{ Path = $path; RelativePath = $RelativePath; Value = ([IO.File]::ReadAllText($path) | ConvertFrom-Json) }
}

function Get-TargetSet {
    param([Parameter(Mandatory = $true)][object[]]$Items,[Parameter(Mandatory = $true)][string]$Property)
    return @($Items | ForEach-Object { [string]$_.$Property } | Sort-Object)
}

function Assert-ExactSet {
    param([Parameter(Mandatory = $true)][string]$Path,[Parameter(Mandatory = $true)][string]$Issue,[Parameter(Mandatory = $true)][string[]]$Expected,[Parameter(Mandatory = $true)][string[]]$Actual)
    $expectedText = [string]::Join("`n", @($Expected | Sort-Object))
    $actualText = [string]::Join("`n", @($Actual | Sort-Object))
    Assert-True -Condition ($expectedText -eq $actualText) -Path $Path -Issue $Issue -Text "expected=$expectedText actual=$actualText"
}

function Get-WorkflowJobText {
    param([Parameter(Mandatory = $true)][string]$Text,[Parameter(Mandatory = $true)][string]$JobName)
    $pattern = "(?ms)^  $([regex]::Escape($JobName)):\r?\n.*?(?=^  [A-Za-z0-9_-]+:\r?\n|\z)"
    $match = [regex]::Match($Text, $pattern)
    if (-not $match.Success) { Add-Violation -Path '.github/workflows/runtime-input.yml' -Issue 'Required workflow job was not found' -Text $JobName; return '' }
    return $match.Value
}

try {
    $contract = Read-RequiredJson -RelativePath $contractRelativePath
    $matrix = Read-RequiredJson -RelativePath $matrixRelativePath
    $c = $contract.Value
    $m = $matrix.Value

    Assert-True -Condition ([int]$c.schemaVersion -eq 1) -Path $contract.RelativePath -Issue 'Support contract schema version must be 1'
    Assert-True -Condition ([string]$c.packageMatrix -eq $matrixRelativePath) -Path $contract.RelativePath -Issue 'Support contract must identify the package matrix'
    Assert-True -Condition ($c.policy.packageSurfaceIsSupport -eq $false) -Path $contract.RelativePath -Issue 'Package surface must not be treated as real support'
    Assert-True -Condition ([string]$c.policy.syntheticRuntimeInputs -eq 'package-shape-only; never real support') -Path $contract.RelativePath -Issue 'Synthetic runtime policy drifted'
    Assert-True -Condition ([string]$c.policy.publication -match 'blocked until') -Path $contract.RelativePath -Issue 'Support contract must keep publication blocked until all release gates pass'

    $matrixTargets = @($m.rids | ForEach-Object { $rid = [string]$_.rid; foreach ($profile in @($m.profiles)) { "$rid/$([string]$profile.name)" } } | Sort-Object)
    $realTargets = @($c.realSupport | ForEach-Object { [string]$_ } | Sort-Object)
    $pendingTargets = @(Get-TargetSet -Items @($c.pending) -Property 'target')
    $excludedTargets = @(Get-TargetSet -Items @($c.excluded) -Property 'target')
    $classifiedTargets = @($realTargets + $pendingTargets + $excludedTargets | Sort-Object)

    Assert-ExactSet -Path $contract.RelativePath -Issue 'Support contract must partition every package matrix RID/profile pair exactly once' -Expected $matrixTargets -Actual $classifiedTargets
    Assert-True -Condition (@($realTargets).Count -eq 24) -Path $contract.RelativePath -Issue 'Real support target count must remain 24'
    Assert-True -Condition (@($pendingTargets).Count -eq 1 -and $pendingTargets[0] -eq 'win-x86/full') -Path $contract.RelativePath -Issue 'Only win-x86/full may remain pending'
    Assert-True -Condition (@($excludedTargets).Count -eq 9 -and $excludedTargets -contains 'win-x86/mini') -Path $contract.RelativePath -Issue 'Excluded support target set drifted'
    Assert-True -Condition (@($c.outsideMatrix | Where-Object { $_.platform -eq 'macOS' -and $_.status -eq 'not-supported' }).Count -eq 1) -Path $contract.RelativePath -Issue 'macOS must remain explicitly outside support'

    foreach ($entry in @($c.pending)) {
        Assert-True -Condition ([string]$entry.status -eq 'hosted-evidence-pending') -Path $contract.RelativePath -Issue 'Pending support target must remain hosted-evidence-pending' -Text $entry.target
        Assert-ExactSet -Path $contract.RelativePath -Issue "Pending target requirements drifted for $($entry.target)" -Expected @('artifact-handoff','hosted-producer','independent-artifact-audit','same-run-pack','x86-consumer') -Actual @($entry.requires)
    }
    foreach ($entry in @($c.excluded)) {
        Assert-True -Condition ([string]$entry.status -eq 'excluded' -and -not [string]::IsNullOrWhiteSpace([string]$entry.reason)) -Path $contract.RelativePath -Issue 'Excluded target must carry an explicit reason' -Text $entry.target
    }

    $runtimeInputPath = Join-Path $repo '.github/workflows/runtime-input.yml'
    $runtimeInputText = [IO.File]::ReadAllText($runtimeInputPath)
    $selectedTargetMatch = [regex]::Match($runtimeInputText, '(?ms)\$supportedTargets\s*=\s*@\((.*?)\)')
    Assert-True -Condition $selectedTargetMatch.Success -Path '.github/workflows/runtime-input.yml' -Issue 'runtime-input.yml supported-target allowlist is missing'
    if ($selectedTargetMatch.Success) {
        $producerTargets = @([regex]::Matches($selectedTargetMatch.Groups[1].Value, "'([^']+)'" ) | ForEach-Object { $_.Groups[1].Value } | Sort-Object)
        Assert-ExactSet -Path '.github/workflows/runtime-input.yml' -Issue 'Real producer allowlist must equal real support plus the one pending hosted target' -Expected @($realTargets + $pendingTargets) -Actual $producerTargets
        Assert-True -Condition (@($producerTargets | Where-Object { $excludedTargets -contains $_ }).Count -eq 0) -Path '.github/workflows/runtime-input.yml' -Issue 'Producer allowlist must not include excluded targets'
    }

    $packWorkflowPath = Join-Path $repo '.github/workflows/pack.yml'
    $packWorkflowText = [IO.File]::ReadAllText($packWorkflowPath)
    $runtimeJobText = Get-WorkflowJobText -Text $packWorkflowText -JobName 'pack-runtime'
    Assert-True -Condition ($runtimeJobText.Contains('validate_synthetic_runtime')) -Path '.github/workflows/pack.yml' -Issue 'Pack workflow must retain explicit synthetic-input gating'
    Assert-True -Condition ($runtimeJobText.Contains("publish_github_packages == 'true'")) -Path '.github/workflows/pack.yml' -Issue 'Pack workflow must retain explicit publication gating'
    Assert-True -Condition ($runtimeJobText.Contains('real_runtime_artifact_run_id')) -Path '.github/workflows/pack.yml' -Issue 'Pack workflow must retain real artifact handoff input'
    Assert-True -Condition (-not $runtimeJobText.Contains('-SelectedRid android-')) -Path '.github/workflows/pack.yml' -Issue 'Pack workflow must not claim Android real verification'

    $readmePath = Join-Path $repo 'packaging/runtime/JYPPX.OpenCV.runtime/README.md'
    $readmeText = [IO.File]::ReadAllText($readmePath)
    Assert-True -Condition ($readmeText.Contains('runtime-support-contract.json')) -Path 'packaging/runtime/JYPPX.OpenCV.runtime/README.md' -Issue 'Runtime README must link the support contract'
    Assert-True -Condition ($readmeText.Contains('Windows x86 remains synthetic-only') -and $readmeText.Contains('Android')) -Path 'packaging/runtime/JYPPX.OpenCV.runtime/README.md' -Issue 'Runtime README must preserve x86 and Android non-support wording'

    $guidePath = Join-Path $repo 'docs/articles/linked-runtime-build-guide.md'
    $guideText = [IO.File]::ReadAllText($guidePath)
    Assert-True -Condition ($guideText.Contains('runtime-support-contract.json')) -Path 'docs/articles/linked-runtime-build-guide.md' -Issue 'Linked runtime guide must link the support contract'
    Assert-True -Condition ($guideText.Contains('Windows x86 remains synthetic-only') -and $guideText.Contains('Android')) -Path 'docs/articles/linked-runtime-build-guide.md' -Issue 'Linked runtime guide must preserve x86 and Android non-support wording'

    Write-Host "RELEASE_SUPPORT_CONTRACT_OK matrix_entries=$($matrixTargets.Count) real=$($realTargets.Count) pending=$($pendingTargets.Count) excluded=$($excludedTargets.Count) outside_matrix=macOS package_surface_support=false"
}
catch {
    Add-Violation -Path $contractRelativePath -Issue 'Release support contract execution failed' -Text $_.Exception.Message
}

if ($violations.Count -gt 0) {
    Write-Host "Release support contract failed with $($violations.Count) violation(s)."
    $violations | Sort-Object Path, Issue | Format-Table Path, Issue, Text -AutoSize
    exit 1
}

Write-Host 'Release support contract passed.'
Write-Host 'Package-matrix surface is explicitly separated from real support; Android/macOS and Windows x86 mini remain excluded, while Windows x86 full remains hosted-evidence-pending.'

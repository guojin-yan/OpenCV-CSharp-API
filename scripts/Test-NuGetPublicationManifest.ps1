[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$ManifestPath,
    [Parameter(Mandatory = $true)][ValidatePattern('^[0-9a-f]{40}$')][string]$SourceCommit,
    [Parameter(Mandatory = $true)][string]$PackageVersion,
    [string]$SupportContractPath = (Join-Path $PSScriptRoot "../packaging/runtime/runtime-support-contract.json"),
    [string]$OutputPath = "",
    [switch]$Check
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Assert-ExactPropertySet {
    param([object]$Value,[string[]]$Expected,[string]$Context)
    $actual = @($Value.PSObject.Properties.Name | Sort-Object)
    $wanted = @($Expected | Sort-Object)
    if (($actual -join "`n") -cne ($wanted -join "`n")) {
        throw "$Context property set mismatch. actual=$($actual -join ',') expected=$($wanted -join ',')"
    }
}

if ($PackageVersion -cne '5.0.0-preview.1') {
    throw "The first-preview publication manifest requires package version 5.0.0-preview.1. Actual: $PackageVersion"
}

$resolvedManifest = (Resolve-Path -LiteralPath $ManifestPath).Path
$resolvedSupport = (Resolve-Path -LiteralPath $SupportContractPath).Path
$input = Get-Content -LiteralPath $resolvedManifest -Raw | ConvertFrom-Json
$support = Get-Content -LiteralPath $resolvedSupport -Raw | ConvertFrom-Json

$isNormalized = $input.PSObject.Properties.Name -contains 'RecordKind'
if ($isNormalized) {
    Assert-ExactPropertySet -Value $input -Expected @('SchemaVersion', 'RecordKind', 'SourceRevision', 'PackageVersion', 'SupportContractSha256', 'PackageCount', 'RuntimePackageCount', 'Packages', 'Deterministic') -Context 'Normalized publication manifest'
    if ([string]$input.RecordKind -cne 'nuget-publication-input-manifest' -or -not [bool]$input.Deterministic) {
        throw 'Normalized publication manifest kind or deterministic state is invalid.'
    }
}
else {
    Assert-ExactPropertySet -Value $input -Expected @('SchemaVersion', 'SourceRevision', 'PackageVersion', 'Packages') -Context 'Publication manifest'
}
if ([int]$input.SchemaVersion -ne 1 -or [string]$input.SourceRevision -cne $SourceCommit -or [string]$input.PackageVersion -cne $PackageVersion) {
    throw 'Publication manifest source or version identity mismatch.'
}

$realTargets = @($support.realSupport | ForEach-Object { [string]$_ } | Sort-Object)
if ($realTargets.Count -ne 24 -or @($realTargets | Sort-Object -Unique).Count -ne 24) {
    throw "Runtime support contract must contain exactly 24 unique real-supported targets. Actual: $($realTargets.Count)"
}
$supportHash = (Get-FileHash -LiteralPath $resolvedSupport -Algorithm SHA256).Hash.ToLowerInvariant()
if ($isNormalized -and ([string]$input.SupportContractSha256 -cne $supportHash -or [int]$input.PackageCount -ne 25 -or [int]$input.RuntimePackageCount -ne 24)) {
    throw 'Normalized publication manifest support binding or package counts drifted.'
}

$expected = [Collections.Generic.List[object]]::new()
$expected.Add([pscustomobject]@{
        Kind = 'managed'
        Rid = ''
        RuntimeProfile = ''
        PackageId = 'JYPPX.OpenCV.CSharp.API'
        ArtifactName = 'nupkg-managed'
        PackageFile = "JYPPX.OpenCV.CSharp.API.$PackageVersion.nupkg"
        SbomFile = 'managed.spdx.json'
    })
foreach ($target in $realTargets) {
    $parts = @($target -split '/')
    if ($parts.Count -ne 2 -or $parts[1] -notin @('full', 'mini')) {
        throw "Invalid real-supported target: $target"
    }
    $rid = $parts[0]
    $profile = $parts[1]
    $suffix = if ($profile -eq 'mini') { '.mini' } else { '' }
    $packageId = "JYPPX.OpenCV.runtime.$rid$suffix"
    $expected.Add([pscustomobject]@{
            Kind = 'runtime'
            Rid = $rid
            RuntimeProfile = $profile
            PackageId = $packageId
            ArtifactName = "nupkg-$rid-$profile"
            PackageFile = "$packageId.$PackageVersion.nupkg"
            SbomFile = "runtime-$rid-$profile.spdx.json"
        })
}

$inputPackages = @($input.Packages)
if ($inputPackages.Count -ne $expected.Count) {
    throw "Publication manifest must contain exactly $($expected.Count) packages. Actual: $($inputPackages.Count)"
}
if (@($inputPackages | Group-Object { [string]$_.PackageId } | Where-Object Count -ne 1).Count -ne 0) {
    throw 'Publication manifest package IDs must be unique.'
}

$normalized = [Collections.Generic.List[object]]::new()
foreach ($definition in $expected) {
    $matches = @($inputPackages | Where-Object { [string]$_.PackageId -ceq $definition.PackageId })
    if ($matches.Count -ne 1) { throw "Publication manifest is missing exact package: $($definition.PackageId)" }
    $item = $matches[0]
    $expectedProperties = @('Kind', 'Rid', 'RuntimeProfile', 'PackageId', 'ArtifactName', 'RunId', 'Sha256')
    if ($isNormalized) { $expectedProperties += @('PackageFile', 'SbomFile') }
    Assert-ExactPropertySet -Value $item -Expected $expectedProperties -Context "Package $($definition.PackageId)"
    $runId = [string]$item.RunId
    $hash = [string]$item.Sha256
    if ([string]$item.Kind -cne $definition.Kind -or
        [string]$item.Rid -cne $definition.Rid -or
        [string]$item.RuntimeProfile -cne $definition.RuntimeProfile -or
        [string]$item.ArtifactName -cne $definition.ArtifactName) {
        throw "Publication manifest metadata mismatch for $($definition.PackageId)."
    }
    if ($isNormalized -and ([string]$item.PackageFile -cne $definition.PackageFile -or [string]$item.SbomFile -cne $definition.SbomFile)) {
        throw "Normalized publication filenames drifted for $($definition.PackageId)."
    }
    if ($runId -notmatch '^[1-9][0-9]*$') { throw "Invalid pack run ID for $($definition.PackageId): $runId" }
    if ($hash -cnotmatch '^[0-9a-f]{64}$') { throw "Invalid lowercase package SHA256 for $($definition.PackageId): $hash" }
    $normalized.Add([ordered]@{
            Kind = $definition.Kind
            Rid = $definition.Rid
            RuntimeProfile = $definition.RuntimeProfile
            PackageId = $definition.PackageId
            ArtifactName = $definition.ArtifactName
            RunId = $runId
            Sha256 = $hash
            PackageFile = $definition.PackageFile
            SbomFile = $definition.SbomFile
        })
}

$record = [ordered]@{
    SchemaVersion = 1
    RecordKind = 'nuget-publication-input-manifest'
    SourceRevision = $SourceCommit
    PackageVersion = $PackageVersion
    SupportContractSha256 = $supportHash
    PackageCount = $normalized.Count
    RuntimePackageCount = @($normalized | Where-Object Kind -eq 'runtime').Count
    Packages = @($normalized | Sort-Object PackageId)
    Deterministic = $true
}
$json = ((($record | ConvertTo-Json -Depth 8) -replace "`r`n", "`n") -replace "`r", "`n").TrimEnd() + "`n"

if (-not [string]::IsNullOrWhiteSpace($OutputPath)) {
    $fullOutput = [IO.Path]::GetFullPath($OutputPath)
    if ($Check) {
        if (-not (Test-Path -LiteralPath $fullOutput -PathType Leaf)) { throw "Publication manifest check output does not exist: $fullOutput" }
        $actual = ([IO.File]::ReadAllText($fullOutput) -replace "`r`n", "`n") -replace "`r", "`n"
        if ($actual -cne $json) { throw "NuGet publication input manifest drifted: $fullOutput" }
    }
    else {
        New-Item -ItemType Directory -Force -Path (Split-Path -Parent $fullOutput) | Out-Null
        [IO.File]::WriteAllText($fullOutput, $json, [Text.UTF8Encoding]::new($false))
    }
}
elseif ($Check) { throw '-Check requires -OutputPath.' }

Write-Host "NUGET_PUBLICATION_MANIFEST_OK packages=$($normalized.Count) runtimes=$(@($normalized | Where-Object Kind -eq 'runtime').Count) source=$SourceCommit version=$PackageVersion"
if (-not [string]::IsNullOrWhiteSpace($OutputPath)) { Write-Host "Record: $([IO.Path]::GetFullPath($OutputPath))" }

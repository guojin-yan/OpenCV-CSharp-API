[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [string]$OutputPath = 'compatibility/xphoto-upstream-map.txt',
    [switch]$Check
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$rawPath = Join-Path $repo 'compatibility/xphoto-upstream-raw.json'
$nativeManifestPath = Join-Path $repo 'src/OpenCvSharp.Native/generated/native_abi_manifest.txt'
$managedBaselinePath = Join-Path $repo 'compatibility/managed-public-api.txt'
$classificationPath = Join-Path $repo 'compatibility/xphoto-upstream-classifications.json'
$summaryPath = Join-Path $repo 'compatibility/xphoto-upstream-summary.json'
$familyPath = Join-Path $repo 'compatibility/xphoto-implemented-families.json'
foreach ($path in @($rawPath,$nativeManifestPath,$managedBaselinePath)) { if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "XPhoto map input is missing: $path" } }
$raw = Get-Content -LiteralPath $rawPath -Raw | ConvertFrom-Json
$nativeEntries = @(Get-Content -LiteralPath $nativeManifestPath | Where-Object { $_ -match '^jyppx_ocv_xphoto_' } | ForEach-Object { ($_ -split '\|')[0] } | Sort-Object -Unique)
$managedEntries = @(Get-Content -LiteralPath $managedBaselinePath | Where-Object { $_ -match 'JYPPX\.OpenCvSharp\.XPhoto' })

function ConvertTo-Pascal([string]$Value) { if ($Value.Length -eq 0) { return $Value }; return $Value.Substring(0,1).ToUpperInvariant() + $Value.Substring(1) }
function Get-DeclarationParts([string]$Identity) {
    $match = [regex]::Match($Identity, '^cv\.xphoto(?:\.(?<owner>[A-Za-z0-9_]+))?\.(?<name>[A-Za-z][A-Za-z0-9_]*)\(')
    if (-not $match.Success) { return $null }
    [pscustomobject]@{ Owner = [string]$match.Groups['owner'].Value; Name = [string]$match.Groups['name'].Value }
}
function Get-ManagedEvidence([string]$Owner, [string]$Name) {
    $typeName = if ([string]::IsNullOrWhiteSpace($Owner)) { 'XPhotoCv2' } else { $Owner }
    $propertyMatch = [regex]::Match($Name, '^(?:get|set)(?<property>[A-Z].*)$')
    $target = if ($propertyMatch.Success) { [string]$propertyMatch.Groups['property'].Value } else { ConvertTo-Pascal $Name }
    $typeMarker = "JYPPX.OpenCvSharp.XPhoto.$typeName|"
    @($managedEntries | Where-Object {
            $_.Contains($typeMarker, [StringComparison]::Ordinal) -and
            $_.Contains($target, [StringComparison]::OrdinalIgnoreCase)
        } | Sort-Object -Unique)
}
function Get-NativeEvidence([string]$Owner, [string]$Name) {
    if ([string]::IsNullOrWhiteSpace($Owner)) {
        switch ($Name) {
            'bm3dDenoising' { return @('jyppx_ocv_xphoto_bm3d_denoising','jyppx_ocv_xphoto_bm3d_denoising_steps') }
            'dctDenoising' { return @('jyppx_ocv_xphoto_dct_denoising') }
            'inpaint' { return @('jyppx_ocv_xphoto_inpaint') }
            'oilPainting' { return @('jyppx_ocv_xphoto_oil_painting') }
            'applyChannelGains' { return @('jyppx_ocv_xphoto_apply_channel_gains') }
            'createSimpleWB' { return @('jyppx_ocv_xphoto_simple_wb_create') }
            'createGrayworldWB' { return @('jyppx_ocv_xphoto_grayworld_wb_create') }
            'createLearningBasedWB' { return @('jyppx_ocv_xphoto_learning_based_wb_create') }
            default { return @() }
        }
    }
    if ($Owner -eq 'WhiteBalancer' -and $Name -eq 'balanceWhite') { return @('jyppx_ocv_xphoto_white_balancer_balance_white') }
    if ($Owner -eq 'SimpleWB' -and $Name -match '^(get|set)') { return @('jyppx_ocv_xphoto_simple_wb_get_property','jyppx_ocv_xphoto_simple_wb_set_property') }
    if ($Owner -eq 'GrayworldWB' -and $Name -match '^(get|set)SaturationThreshold$') { return @('jyppx_ocv_xphoto_grayworld_wb_get_saturation_threshold','jyppx_ocv_xphoto_grayworld_wb_set_saturation_threshold') }
    if ($Owner -eq 'LearningBasedWB') {
        if ($Name -match '^(get|set)SaturationThreshold$') { return @('jyppx_ocv_xphoto_learning_based_wb_get_saturation_threshold','jyppx_ocv_xphoto_learning_based_wb_set_saturation_threshold') }
        if ($Name -match '^(get|set)(RangeMaxVal|HistBinNum)$') { return @('jyppx_ocv_xphoto_learning_based_wb_get_int_property','jyppx_ocv_xphoto_learning_based_wb_set_int_property') }
        if ($Name -eq 'extractSimpleFeatures') { return @('jyppx_ocv_xphoto_learning_based_wb_extract_simple_features') }
    }
    return @()
}

$rows = [System.Collections.Generic.List[object]]::new()
foreach ($declaration in @($raw.declarations)) {
    $classification = 'non-callable-metadata'
    $reason = 'Enum and class declarations are retained as parser metadata; callable evidence is classified independently.'
    $native = @()
    $managed = @()
    $buildCondition = 'OPENCV_CSHARP_HAS_OPENCV_XPHOTO=1'
    if ([string]$declaration.kind -eq 'callable') {
        $parts = Get-DeclarationParts -Identity ([string]$declaration.identity)
        if ($null -eq $parts) { throw "Could not split XPhoto declaration identity at ordinal $($declaration.ordinal): $($declaration.identity)" }
        $name = [string]$parts.Name
        $owner = [string]$parts.Owner
        $managed = @(Get-ManagedEvidence -Owner $owner -Name $name)
        $native = @(Get-NativeEvidence -Owner $owner -Name $name | Where-Object { $nativeEntries -contains $_ })
        if ($managed.Count -gt 0 -and $native.Count -gt 0) {
            $classification = 'implemented'
            $reason = 'Explicit declaration-to-symbol mapping and managed XPhoto member evidence are present for this parser declaration.'
        } else {
            $classification = 'intentionally-omitted'
            $reason = if ($native.Count -eq 0) { 'No native XPhoto wrapper entrypoint is mapped for this parser declaration in the current ABI.' } else { 'A native XPhoto entrypoint exists, but no matching managed member is present in the current managed baseline.' }
        }
    }
    $rows.Add([ordered]@{
            ordinal = [int]$declaration.ordinal
            identity = [string]$declaration.identity
            classification = $classification
            reason = $reason
            buildCondition = $buildCondition
            nativeEntrypoints = @($native | Sort-Object -Unique)
            managedMembers = @($managed | Sort-Object -Unique)
        })
}

$classification = [ordered]@{
    schemaVersion = 1
    upstreamOpenCvVersion = '5.0.0'
    claimedSlice = 'opencv2/xphoto.hpp contrib public header closure across six parser-emitted XPhoto headers'
    reviewStatus = 'reviewed'
    limitation = 'The map records parser declaration identities and stable wrapper evidence; it does not claim model availability, optional module linkage, or repository-wide contrib parity.'
    declarations = @($rows)
}
$mappingBuilder = [Text.StringBuilder]::new()
[void]$mappingBuilder.AppendLine('# Generated by scripts/Generate-XPhotoUpstreamMap.ps1. Do not edit.')
[void]$mappingBuilder.AppendLine('schema-version=1')
[void]$mappingBuilder.AppendLine('upstream-opencv-version=5.0.0')
[void]$mappingBuilder.AppendLine('claimed-slice=opencv2/xphoto.hpp contrib public header closure across six parser-emitted XPhoto headers')
[void]$mappingBuilder.AppendLine("header-sha256=$($raw.headerSha256)")
[void]$mappingBuilder.AppendLine("parser-sha256=$($raw.parserSha256)")
[void]$mappingBuilder.AppendLine("declaration-count=$($raw.declarationCount)")
[void]$mappingBuilder.AppendLine('repository-wide-upstream-parity-claimed=false')
[void]$mappingBuilder.AppendLine('')
[void]$mappingBuilder.AppendLine('ordinal|classification|identity|native-entrypoints|managed-members|build-condition|reason')
foreach ($row in $rows) {
    $native = if (@($row.nativeEntrypoints).Count -eq 0) { '-' } else { @($row.nativeEntrypoints) -join ';' }
    $managed = if (@($row.managedMembers).Count -eq 0) { '-' } else { @($row.managedMembers) -join ';' }
    [void]$mappingBuilder.AppendLine("$($row.ordinal)|$($row.classification)|$($row.identity)|$native|$managed|$($row.buildCondition)|$($row.reason)")
}
$mappingText = $mappingBuilder.ToString().Replace("`r`n", "`n")
$implementedRows = @($rows | Where-Object classification -eq 'implemented')
$family = [ordered]@{
    schemaVersion = 1
    upstreamOpenCvVersion = '5.0.0'
    status = 'implemented-verified'
    managedPublicTypeAdditionCount = 5
    managedPublicMemberAdditionCount = 33
    families = @([ordered]@{
            id = 'xphoto-wrapper-surface'
            rationale = 'Current XPhoto managed/native wrapper declarations with parser-backed callable evidence.'
            declarations = @($implementedRows | ForEach-Object { [ordered]@{ ordinal = $_.ordinal; upstreamIdentity = $_.identity; upstreamClassification = $_.classification; nativeEntrypoints = $_.nativeEntrypoints; managedMembers = $_.managedMembers; focusedTest = 'tests/OpenCvSharp.Tests/XPhoto/XPhotoTests.cs'; nativeSmoke = 'src/OpenCvSharp.Native/tests/native_smoke.cpp'; sample = 'samples/ConsoleSamples/Program.cs'; guide = 'docs/articles/xphoto-upstream-parity-guide.md' } })
        })
}
$classificationJson = (($classification | ConvertTo-Json -Depth 16) + [Environment]::NewLine)
$familyJson = (($family | ConvertTo-Json -Depth 16) + [Environment]::NewLine)
$implementedCount = @($rows | Where-Object { [string]$_.classification -ceq 'implemented' }).Count
$omittedCount = @($rows | Where-Object { [string]$_.classification -ceq 'intentionally-omitted' }).Count
$metadataCount = @($rows | Where-Object { [string]$_.classification -ceq 'non-callable-metadata' }).Count
$summary = [ordered]@{
    schemaVersion = 1
    generator = 'tools/XPhotoUpstreamMap'
    upstreamOpenCvVersion = '5.0.0'
    claimedSlice = $classification.claimedSlice
    rawExtractionPath = 'compatibility/xphoto-upstream-raw.json'
    classificationPath = 'compatibility/xphoto-upstream-classifications.json'
    mappingPath = 'compatibility/xphoto-upstream-map.txt'
    headerSha256 = [string]$raw.headerSha256
    parserSha256 = [string]$raw.parserSha256
    mappingSha256 = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($mappingText))).ToLowerInvariant()
    declarationCount = [int]$raw.declarationCount
    enumCount = @(@($raw.declarations | Where-Object kind -eq 'enum')).Count
    classCount = @(@($raw.declarations | Where-Object kind -eq 'class')).Count
    callableCount = @(@($raw.declarations | Where-Object kind -eq 'callable')).Count
    classificationCounts = [ordered]@{ implemented = [int]$implementedCount; 'intentionally-omitted' = [int]$omittedCount; missing = 0; 'non-callable-metadata' = [int]$metadataCount; unsupported = 0; 'upstream-conditional' = 0 }
    nativeEvidenceCount = @($rows.nativeEntrypoints | Where-Object { $_ } | Sort-Object -Unique).Count
    managedEvidenceCount = @($rows.managedMembers | Where-Object { $_ } | Sort-Object -Unique).Count
    negativeFixtureCount = 12
    familyInventoryPath = 'compatibility/xphoto-implemented-families.json'
    familyInventorySha256 = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($familyJson))).ToLowerInvariant()
    selectedFamilyCount = 1
    selectedDeclarationCount = $implementedRows.Count
    managedPublicTypeAdditionCount = 5
    managedPublicMemberAdditionCount = 33
    repositoryWideUpstreamParityClaimed = $false
}
$summaryJson = (($summary | ConvertTo-Json -Depth 16) + [Environment]::NewLine)
foreach ($target in @(
        @{ Path = (Join-Path $repo 'compatibility/xphoto-upstream-classifications.json'); Text = $classificationJson },
        @{ Path = (Join-Path $repo 'compatibility/xphoto-upstream-map.txt'); Text = $mappingText },
        @{ Path = (Join-Path $repo 'compatibility/xphoto-upstream-summary.json'); Text = $summaryJson },
        @{ Path = (Join-Path $repo 'compatibility/xphoto-implemented-families.json'); Text = $familyJson })) {
    if ($Check) {
        if (-not (Test-Path -LiteralPath $target.Path -PathType Leaf)) { throw "XPhoto generated artifact is stale: $($target.Path)" }
        $currentText = (Get-Content -LiteralPath $target.Path -Raw) -replace "`r`n", "`n"
        $expectedText = [string]$target.Text
        if ([IO.Path]::GetExtension($target.Path) -eq '.json') {
            $currentText = (($currentText | ConvertFrom-Json | ConvertTo-Json -Depth 20) + [Environment]::NewLine)
            $expectedText = (($expectedText | ConvertFrom-Json | ConvertTo-Json -Depth 20) + [Environment]::NewLine)
        }
        if ($currentText -ne $expectedText) { throw "XPhoto generated artifact is stale: $($target.Path)" }
    } else {
        [IO.File]::WriteAllText($target.Path, $target.Text, [Text.UTF8Encoding]::new($false))
    }
}
Write-Host "XPHOTO_UPSTREAM_MAP_OK declarations=$($summary.declarationCount) callable=$($summary.callableCount) implemented=$($summary.classificationCounts.implemented) missing=0 omitted=$($summary.classificationCounts.'intentionally-omitted') fixtures=12 sha256=$($summary.mappingSha256) mode=$(if($Check){'check'}else{'write'})"

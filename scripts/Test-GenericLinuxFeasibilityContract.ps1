param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$contractRelativePath = 'packaging/runtime/runtime-generic-linux-feasibility.json'
$schemaRelativePath = 'packaging/runtime/runtime-generic-linux-feasibility.schema.json'
$matrixRelativePath = 'packaging/runtime/runtime-package-matrix.json'
$graphRelativePath = 'packaging/runtime/runtime-distro-rid-graph.json'
$adrRelativePath = 'docs/articles/generic-linux-rid-adr.md'
$contractPath = Join-Path $repo ($contractRelativePath -replace '/', [IO.Path]::DirectorySeparatorChar)
$schemaPath = Join-Path $repo ($schemaRelativePath -replace '/', [IO.Path]::DirectorySeparatorChar)
$matrixPath = Join-Path $repo ($matrixRelativePath -replace '/', [IO.Path]::DirectorySeparatorChar)
$graphPath = Join-Path $repo ($graphRelativePath -replace '/', [IO.Path]::DirectorySeparatorChar)
$adrPath = Join-Path $repo ($adrRelativePath -replace '/', [IO.Path]::DirectorySeparatorChar)

foreach ($path in @($contractPath, $schemaPath, $matrixPath, $graphPath, $adrPath)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Generic Linux feasibility surface was not found: $path"
    }
}

$contract = Get-Content -LiteralPath $contractPath -Raw | ConvertFrom-Json
$matrix = Get-Content -LiteralPath $matrixPath -Raw | ConvertFrom-Json
$graph = Get-Content -LiteralPath $graphPath -Raw | ConvertFrom-Json
$adr = [IO.File]::ReadAllText($adrPath)

if (-not (Test-Json -LiteralPath $contractPath -SchemaFile $schemaPath -ErrorAction Stop)) {
    throw 'Generic Linux feasibility contract failed JSON Schema validation.'
}

$expectedTargets = @('debian.12-x64', 'fedora.40-x64', 'ubuntu.22.04-x64')
$actualTargets = @($contract.referenceTargets | ForEach-Object { [string]$_.rid } | Sort-Object)
if ((@($actualTargets) -join '|') -cne (@($expectedTargets | Sort-Object) -join '|')) {
    throw "Generic Linux feasibility reference target set drifted: $($actualTargets -join ', ')"
}
if ([string]$contract.targetRid -cne 'linux-x64' -or
    [string]$contract.status -cne 'preview-only' -or
    [bool]$contract.packageIdentityAllowed -or
    [string]$contract.activePackageMatrix -cne $matrixRelativePath -or
    [string]$contract.proposedGlibcBaseline -cne '2.35') {
    throw 'Generic Linux feasibility must remain preview-only, non-publishable, and bound to the active package matrix.'
}

$matrixRids = @($matrix.rids | ForEach-Object { [string]$_.rid })
foreach ($target in $expectedTargets) {
    if ($matrixRids -notcontains $target) {
        throw "Generic Linux feasibility target is missing from the active factual matrix: $target"
    }
    $targetRow = @($contract.referenceTargets | Where-Object { [string]$_.rid -ceq $target })[0]
    $matrixRow = @($matrix.rids | Where-Object { [string]$_.rid -ceq $target })[0]
    if ((@($targetRow.profiles | Sort-Object) -join '|') -cne 'full|mini' -or
        [string]$targetRow.distro -cne [string]$matrixRow.distro -or
        [string]$targetRow.distroVersion -cne [string]$matrixRow.distroVersion -or
        [string]$matrixRow.platformFamily -cne 'linux' -or
        [string]$matrixRow.nativeExtension -cne '.so' -or
        ((@($matrixRow.producer.profiles | Sort-Object) -join '|') -cne 'full|mini')) {
        throw "Generic Linux feasibility target/profile contract drifted: $target"
    }
}

if ($null -eq $graph.runtimes.PSObject.Properties['linux-x64'] -or
    $null -eq $graph.runtimes.PSObject.Properties['linux']) {
    throw 'The repository RID graph must retain generic linux-x64/linux nodes for feasibility analysis.'
}
if (@($matrix.rids | Where-Object { [string]$_.rid -ceq 'linux-x64' }).Count -ne 0) {
    throw 'Generic linux-x64 must not enter the active runtime package matrix during feasibility.'
}
if ($null -ne $contract.PSObject.Properties['packageId'] -or
    $null -ne $contract.PSObject.Properties['producer']) {
    throw 'Generic linux-x64 feasibility must not contain a publishable package identity or producer surface.'
}
if ([string]$contract.ridSelection.graphDirection -cne 'distro-specific RIDs import linux-x64; linux-x64 imports linux' -or
    [string]$contract.ridSelection.exactAssetPriority -cne 'exact RID assets are preferred before imported fallback assets' -or
    [string]$contract.ridSelection.packageReferencePolicy -cne 'package IDs are explicit; managed package does not auto-select a runtime package' -or
    [string]$contract.ridSelection.previewPolicy -cne 'linux-x64 preview may be referenced only by explicit preview consumers') {
    throw 'Generic linux-x64 RID selection or fallback policy drifted.'
}
if (@($contract.referenceTargets | Where-Object { [string]$_.role -ceq 'baseline' }).Count -ne 1 -or
    @($contract.referenceTargets | Where-Object { [string]$_.role -ceq 'cross-distro' }).Count -ne 2) {
    throw 'Generic linux-x64 feasibility requires exactly one baseline and two cross-distro references.'
}

foreach ($token in @(
        'preview-only',
        'packageIdentityAllowed=false',
        'glibc',
        '2.35',
        'exact RID assets are preferred before imported fallback assets',
        'package IDs are explicit; managed package does not auto-select a runtime package',
        'Full and Mini',
        'LD_LIBRARY_PATH',
        'producer `PATH`',
        'no package identity, RID graph entry, support-contract entry, or publication manifest is changed implicitly',
        'runtime-generic-linux-feasibility.json')) {
    if ($adr.IndexOf($token, [StringComparison]::OrdinalIgnoreCase) -lt 0) {
        throw "$adrRelativePath is missing the required generic Linux feasibility boundary: $token"
    }
}

$invalid = $contract | ConvertTo-Json -Depth 20 | ConvertFrom-Json
$invalid.status = 'real-supported'
if (Test-Json -Json ($invalid | ConvertTo-Json -Depth 20) -SchemaFile $schemaPath -ErrorAction SilentlyContinue) {
    throw 'Generic Linux schema accepted a promoted status fixture.'
}

$invalid = $contract | ConvertTo-Json -Depth 20 | ConvertFrom-Json
$invalid.packageIdentityAllowed = $true
if (Test-Json -Json ($invalid | ConvertTo-Json -Depth 20) -SchemaFile $schemaPath -ErrorAction SilentlyContinue) {
    throw 'Generic Linux schema accepted a publishable package identity fixture.'
}

$invalid = $contract | ConvertTo-Json -Depth 20 | ConvertFrom-Json
$invalid.referenceTargets = @($invalid.referenceTargets | Select-Object -First 2)
if (Test-Json -Json ($invalid | ConvertTo-Json -Depth 20) -SchemaFile $schemaPath -ErrorAction SilentlyContinue) {
    throw 'Generic Linux schema accepted a two-distribution evidence fixture.'
}

Write-Host "GENERIC_LINUX_FEASIBILITY_OK target=$($contract.targetRid) status=$($contract.status) references=$(@($contract.referenceTargets).Count) profiles=2 proposed_glibc=$($contract.proposedGlibcBaseline) package_identity_allowed=false"
Write-Host 'Generic linux-x64 remains preview-only; distro-specific package identities and rollback boundaries are unchanged.'

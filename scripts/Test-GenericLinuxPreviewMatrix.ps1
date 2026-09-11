param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$matrixRelativePath = 'packaging/runtime/runtime-generic-linux-preview-matrix.json'
$schemaRelativePath = 'packaging/runtime/runtime-generic-linux-preview-matrix.schema.json'
$feasibilityRelativePath = 'packaging/runtime/runtime-generic-linux-feasibility.json'
$activeMatrixRelativePath = 'packaging/runtime/runtime-package-matrix.json'
$matrixPath = Join-Path $repo ($matrixRelativePath -replace '/', [IO.Path]::DirectorySeparatorChar)
$schemaPath = Join-Path $repo ($schemaRelativePath -replace '/', [IO.Path]::DirectorySeparatorChar)
$feasibilityPath = Join-Path $repo ($feasibilityRelativePath -replace '/', [IO.Path]::DirectorySeparatorChar)
$activeMatrixPath = Join-Path $repo ($activeMatrixRelativePath -replace '/', [IO.Path]::DirectorySeparatorChar)

foreach ($path in @($matrixPath, $schemaPath, $feasibilityPath, $activeMatrixPath)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Generic Linux preview surface was not found: $path"
    }
}

$matrix = Get-Content -LiteralPath $matrixPath -Raw | ConvertFrom-Json
$feasibility = Get-Content -LiteralPath $feasibilityPath -Raw | ConvertFrom-Json
$active = Get-Content -LiteralPath $activeMatrixPath -Raw | ConvertFrom-Json

if (-not (Test-Json -LiteralPath $matrixPath -SchemaFile $schemaPath -ErrorAction Stop)) {
    throw 'Generic Linux preview matrix failed JSON Schema validation.'
}

if ([string]$matrix.status -cne 'preview-only' -or
    [string]$matrix.targetRid -cne 'linux-x64' -or
    [bool]$matrix.activePackageIdentityAllowed -or
    -not [bool]$matrix.previewArtifactIdentityAllowed -or
    [bool]$matrix.publicationAllowed -or
    [string]$matrix.activePackageMatrix -cne $activeMatrixRelativePath -or
    [string]$matrix.feasibilityContract -cne $feasibilityRelativePath) {
    throw 'Generic Linux preview matrix must remain preview-only, non-publishable, and bound to the active matrix/feasibility contract.'
}

if ([string]$matrix.producer.sourceRid -cne 'ubuntu.22.04-x64' -or
    [string]$matrix.producer.runner -cne 'ubuntu-22.04' -or
    [string]$matrix.producer.glibcBaseline -cne '2.35' -or
    (@($matrix.producer.profiles | Sort-Object) -join '|') -cne 'full|mini') {
    throw 'Generic Linux preview producer must be the Ubuntu 22.04 x64 glibc 2.35 Full/Mini baseline.'
}

if ([string]$feasibility.targetRid -cne 'linux-x64' -or
    [string]$feasibility.status -cne 'preview-only' -or
    [string]$feasibility.proposedGlibcBaseline -cne [string]$matrix.producer.glibcBaseline) {
    throw 'Generic Linux preview matrix is not bound to the RT-004 feasibility contract.'
}

$previewRid = @($matrix.rids | Where-Object { [string]$_.rid -ceq 'linux-x64' })
if ($previewRid.Count -ne 1 -or
    [string]$previewRid[0].platformFamily -cne 'linux' -or
    [string]$previewRid[0].opencvRid -cne 'ubuntu.22.04-x64' -or
    [string]$previewRid[0].nativeExtension -cne '.so' -or
    [string]$previewRid[0].producer.kind -cne 'linux' -or
    ((@($previewRid[0].producer.profiles | Sort-Object) -join '|') -cne 'full|mini')) {
    throw 'Generic Linux preview RID row must expose one linux-x64 producer mapped to the Ubuntu baseline source RID.'
}

foreach ($profileName in @('full', 'mini')) {
    $previewProfile = @($matrix.profiles | Where-Object { [string]$_.name -ceq $profileName })
    $activeProfile = @($active.profiles | Where-Object { [string]$_.name -ceq $profileName })
    if ($previewProfile.Count -ne 1 -or $activeProfile.Count -ne 1 -or
        [string]$previewProfile[0].buildList -cne [string]$activeProfile[0].buildList -or
        ((@($previewProfile[0].modules) -join '|') -cne (@($activeProfile[0].modules) -join '|')) -or
        ((@($previewProfile[0].optionalModules) -join '|') -cne (@($activeProfile[0].optionalModules) -join '|'))) {
        throw "Generic Linux preview profile must match the active Full/Mini build contract: $profileName"
    }
}

$expectedConsumers = @('ubuntu.22.04-x64', 'debian.12-x64', 'fedora.40-x64')
$actualConsumers = @($matrix.consumers | ForEach-Object { [string]$_.rid } | Sort-Object)
if ((@($actualConsumers) -join '|') -cne (@($expectedConsumers | Sort-Object) -join '|')) {
    throw "Generic Linux preview consumer set drifted: $($actualConsumers -join ', ')"
}
if (@($matrix.consumers | Where-Object { [string]$_.role -ceq 'baseline' }).Count -ne 1 -or
    @($matrix.consumers | Where-Object { [string]$_.role -ceq 'cross-distro' }).Count -ne 2) {
    throw 'Generic Linux preview requires exactly one baseline and two cross-distro consumers.'
}

foreach ($consumer in @($matrix.consumers)) {
    if ((@($consumer.profiles | Sort-Object) -join '|') -cne 'full|mini') {
        throw "Generic Linux preview consumer must cover Full and Mini independently: $($consumer.rid)"
    }
    if ([string]$consumer.execution -ceq 'container' -and [string]::IsNullOrWhiteSpace([string]$consumer.containerImage)) {
        throw "Container consumer is missing an immutable image: $($consumer.rid)"
    }
}

if (@($active.rids | Where-Object { [string]$_.rid -ceq 'linux-x64' }).Count -ne 0) {
    throw 'Generic Linux preview must not enter the active runtime package matrix.'
}
if (@($matrix.profiles).Count -ne 2 -or
    @($matrix.profiles | Where-Object { [string]$_.name -ceq 'full' -and [string]$_.packageId -ceq 'JYPPX.OpenCV.runtime.linux-x64.preview' }).Count -ne 1 -or
    @($matrix.profiles | Where-Object { [string]$_.name -ceq 'mini' -and [string]$_.packageId -ceq 'JYPPX.OpenCV.runtime.linux-x64.preview.mini' }).Count -ne 1) {
    throw 'Generic Linux preview package identities must remain explicit preview-only identities.'
}

$invalid = $matrix | ConvertTo-Json -Depth 20 | ConvertFrom-Json
$invalid.status = 'real-supported'
if (Test-Json -Json ($invalid | ConvertTo-Json -Depth 20) -SchemaFile $schemaPath -ErrorAction SilentlyContinue) {
    throw 'Generic Linux preview schema accepted a promoted status fixture.'
}
$invalid = $matrix | ConvertTo-Json -Depth 20 | ConvertFrom-Json
$invalid.publicationAllowed = $true
if (Test-Json -Json ($invalid | ConvertTo-Json -Depth 20) -SchemaFile $schemaPath -ErrorAction SilentlyContinue) {
    throw 'Generic Linux preview schema accepted a publication-enabled fixture.'
}
$invalid = $matrix | ConvertTo-Json -Depth 20 | ConvertFrom-Json
$invalid.consumers = @($invalid.consumers | Select-Object -First 2)
if (Test-Json -Json ($invalid | ConvertTo-Json -Depth 20) -SchemaFile $schemaPath -ErrorAction SilentlyContinue) {
    throw 'Generic Linux preview schema accepted a two-consumer fixture.'
}

Write-Host "GENERIC_LINUX_PREVIEW_MATRIX_OK target=$($matrix.targetRid) status=$($matrix.status) producer=$($matrix.producer.sourceRid) consumers=$(@($matrix.consumers).Count) profiles=$(@($matrix.profiles).Count) publication_allowed=false"
Write-Host 'Generic linux-x64 preview package identities are candidate-only and remain outside active publication surfaces.'

param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$matrixPath = Join-Path $repo 'packaging/runtime/runtime-generic-linux-arm64-preview-matrix.json'
$schemaPath = Join-Path $repo 'packaging/runtime/runtime-generic-linux-arm64-preview-matrix.schema.json'
$activePath = Join-Path $repo 'packaging/runtime/runtime-package-matrix.json'
$graphPath = Join-Path $repo 'packaging/runtime/runtime-distro-rid-graph.json'
foreach ($path in @($matrixPath, $schemaPath, $activePath, $graphPath)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Generic linux-arm64 preview surface was not found: $path" }
}

$matrix = Get-Content -LiteralPath $matrixPath -Raw | ConvertFrom-Json
$active = Get-Content -LiteralPath $activePath -Raw | ConvertFrom-Json
$graph = Get-Content -LiteralPath $graphPath -Raw | ConvertFrom-Json
if (-not (Test-Json -LiteralPath $matrixPath -SchemaFile $schemaPath -ErrorAction Stop)) { throw 'Generic linux-arm64 preview matrix failed JSON Schema validation.' }
if ([string]$matrix.status -cne 'preview-only' -or [string]$matrix.targetRid -cne 'linux-arm64' -or [bool]$matrix.activePackageIdentityAllowed -or -not [bool]$matrix.previewArtifactIdentityAllowed -or [bool]$matrix.publicationAllowed) { throw 'Generic linux-arm64 preview must remain preview-only and non-publishable.' }
if ([string]$matrix.producer.sourceRid -cne 'ubuntu.22.04-arm64' -or [string]$matrix.producer.runner -cne 'ubuntu-24.04-arm' -or [string]$matrix.producer.execution -cne 'container-on-native-aarch64' -or [string]$matrix.producer.glibcBaseline -cne '2.35') { throw 'Generic linux-arm64 producer must use the Ubuntu 22.04 ARM64 glibc baseline on native AArch64.' }

$expectedConsumers = @('ubuntu.22.04-arm64', 'ubuntu.24.04-arm64', 'debian.12-arm64')
$actualConsumers = @($matrix.consumers | ForEach-Object { [string]$_.rid } | Sort-Object)
if ((@($actualConsumers) -join '|') -cne (@($expectedConsumers | Sort-Object) -join '|')) { throw "Generic linux-arm64 consumer set drifted: $($actualConsumers -join ', ')" }
if (@($matrix.consumers | Where-Object { [string]$_.role -ceq 'baseline' }).Count -ne 1 -or @($matrix.consumers | Where-Object { [string]$_.role -ceq 'cross-distro' }).Count -ne 1 -or @($matrix.consumers | Where-Object { [string]$_.role -ceq 'forward-distro' }).Count -ne 1) { throw 'Generic linux-arm64 requires baseline, forward-distro, and cross-distro consumers.' }
if ($null -eq $graph.runtimes.PSObject.Properties['linux-arm64'] -or @($active.rids | Where-Object { [string]$_.rid -ceq 'linux-arm64' }).Count -ne 0) { throw 'Generic linux-arm64 must exist in the RID graph but remain outside the active package matrix.' }

$activeProfiles = @($active.profiles)
foreach ($name in @('full', 'mini')) {
    $preview = @($matrix.profiles | Where-Object { [string]$_.name -ceq $name })
    $activeProfile = @($activeProfiles | Where-Object { [string]$_.name -ceq $name })
    if ($preview.Count -ne 1 -or $activeProfile.Count -ne 1 -or [string]$preview[0].buildList -cne [string]$activeProfile[0].buildList -or ((@($preview[0].modules) -join '|') -cne (@($activeProfile[0].modules) -join '|'))) { throw "Generic linux-arm64 preview profile drifted from active $name contract." }
}
$invalid = $matrix | ConvertTo-Json -Depth 20 | ConvertFrom-Json
$invalid.status = 'real-supported'
if (Test-Json -Json ($invalid | ConvertTo-Json -Depth 20) -SchemaFile $schemaPath -ErrorAction SilentlyContinue) { throw 'Generic linux-arm64 schema accepted a promoted status fixture.' }
$invalid = $matrix | ConvertTo-Json -Depth 20 | ConvertFrom-Json
$invalid.publicationAllowed = $true
if (Test-Json -Json ($invalid | ConvertTo-Json -Depth 20) -SchemaFile $schemaPath -ErrorAction SilentlyContinue) { throw 'Generic linux-arm64 schema accepted a publication-enabled fixture.' }

Write-Host "GENERIC_LINUX_ARM64_PREVIEW_MATRIX_OK target=$($matrix.targetRid) status=$($matrix.status) producer=$($matrix.producer.sourceRid) consumers=$(@($matrix.consumers).Count) profiles=$(@($matrix.profiles).Count) publication_allowed=false native_aarch64_required=true"

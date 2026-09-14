param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
function Read-Required([string]$relativePath) {
    $path = Join-Path $repo ($relativePath -replace '/', [IO.Path]::DirectorySeparatorChar)
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Required arm64 preview surface was not found: $relativePath" }
    return [IO.File]::ReadAllText($path)
}
$matrixText = Read-Required 'packaging/runtime/runtime-generic-linux-arm64-preview-matrix.json'
$schemaText = Read-Required 'packaging/runtime/runtime-generic-linux-arm64-preview-matrix.schema.json'
$packageText = Read-Required 'scripts/New-GenericLinuxPreviewPackage.ps1'
$consumerText = Read-Required 'scripts/Test-GenericLinuxPreviewConsumer.ps1'
$runtimeInputText = Read-Required '.github/workflows/runtime-input.yml'
$packText = Read-Required '.github/workflows/pack.yml'
$arm64WorkflowText = Read-Required '.github/workflows/generic-linux-arm64-preview.yml'
$matrix = $matrixText | ConvertFrom-Json
if ($matrix.targetRid -cne 'linux-arm64' -or $matrix.status -cne 'preview-only' -or [bool]$matrix.publicationAllowed -or [bool]$matrix.activePackageIdentityAllowed -or $matrix.producer.sourceRid -cne 'ubuntu.22.04-arm64') { throw 'Generic linux-arm64 preview must be isolated, non-publishable, and Ubuntu 22.04 ARM64-produced.' }
foreach ($token in @('linux-arm64', 'ubuntu.22.04-arm64', 'ubuntu.24.04-arm64', 'debian.12-arm64', 'container-on-native-aarch64', 'publicationAllowed')) {
    if ($matrixText.IndexOf($token, [StringComparison]::OrdinalIgnoreCase) -lt 0) { throw "Arm64 preview matrix is missing required token: $token" }
}
foreach ($token in @("[ValidateSet('linux-x64', 'linux-arm64')]", '$TargetRid =', '-p:RuntimePackageRid=$TargetRid', 'RuntimeAssetRid = $targetRid')) {
    if ($packageText.IndexOf($token, [StringComparison]::OrdinalIgnoreCase) -lt 0 -and $consumerText.IndexOf($token, [StringComparison]::OrdinalIgnoreCase) -lt 0) { throw "Arm64 preview package/consumer scripts are missing required token: $token" }
}
if ($matrixText.IndexOf('JYPPX.OpenCV.runtime.linux-arm64.preview', [StringComparison]::OrdinalIgnoreCase) -lt 0) { throw 'Arm64 preview matrix is missing its isolated package identity.' }
if ($arm64WorkflowText.IndexOf('runtime-generic-linux-arm64-preview-matrix.json', [StringComparison]::OrdinalIgnoreCase) -lt 0) { throw 'Arm64 preview workflow is missing its dedicated matrix path.' }
foreach ($token in @('$env:RID_INPUT -notin @(', "matrix.rid == 'ubuntu.22.04-arm64'", 'generic-linux-arm64-preview-runtime-input', 'linux-arm64')) {
    if ($runtimeInputText.IndexOf($token, [StringComparison]::OrdinalIgnoreCase) -lt 0 -and $packText.IndexOf($token, [StringComparison]::OrdinalIgnoreCase) -lt 0 -and $arm64WorkflowText.IndexOf($token, [StringComparison]::OrdinalIgnoreCase) -lt 0) { throw "Arm64 preview workflow boundary is missing required token: $token" }
}
if ($schemaText -notmatch 'linux-arm64' -or $schemaText -notmatch 'container-on-native-aarch64') { throw 'Arm64 preview schema does not enforce target and native-AArch64 producer.' }
Write-Host "GENERIC_LINUX_ARM64_PREVIEW_PACKAGE_SURFACE_OK producer=$($matrix.producer.sourceRid) consumers=$(@($matrix.consumers).Count) profiles=$(@($matrix.profiles).Count) publication_allowed=false native_aarch64_required=true"

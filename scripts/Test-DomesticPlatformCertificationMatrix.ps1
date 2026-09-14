param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
function Resolve-RepoPath([string]$relativePath) { Join-Path $repo ($relativePath -replace '/', [IO.Path]::DirectorySeparatorChar) }
$matrixRelativePath = 'packaging/platform/domestic-platform-certification-matrix.json'
$schemaRelativePath = 'packaging/platform/domestic-platform-certification-matrix.schema.json'
$adrRelativePath = 'docs/articles/domestic-platform-certification-adr.md'
$matrixPath = Resolve-RepoPath $matrixRelativePath
$schemaPath = Resolve-RepoPath $schemaRelativePath
$adrPath = Resolve-RepoPath $adrRelativePath
foreach ($path in @($matrixPath, $schemaPath, $adrPath)) { if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Domestic platform certification surface was not found: $path" } }
$matrix = Get-Content -LiteralPath $matrixPath -Raw | ConvertFrom-Json
$adr = [IO.File]::ReadAllText($adrPath)
if (-not (Test-Json -LiteralPath $matrixPath -SchemaFile $schemaPath -ErrorAction Stop)) { throw 'Domestic platform matrix failed JSON Schema validation.' }
if ([string]$matrix.status -cne 'research-and-certification' -or [string]$matrix.packageIdentityPolicy -notmatch 'reuse generic linux RID' -or @($matrix.platforms).Count -lt 6) { throw 'Domestic platform matrix must remain research/certification-only with generic RID reuse policy.' }
$ids = @($matrix.platforms | ForEach-Object { [string]$_.id })
foreach ($requiredId in @('openEuler-22.03-LTS-SP3', 'openEuler-24.03-LTS', 'kylin-V10-SP1', 'UOS-20', 'openKylin-2.0', 'Loongnix-LoongArch64')) { if ($ids -notcontains $requiredId) { throw "Domestic platform matrix is missing required platform: $requiredId" } }
$openEuler = @($matrix.platforms | Where-Object { [string]$_.vendor -ceq 'openEuler' })
if ($openEuler.Count -ne 2 -or @($openEuler | Where-Object { @($_.architectures) -contains 'aarch64' }).Count -ne 2) { throw 'openEuler must cover both listed LTS rows with AArch64.' }
$loong = @($matrix.platforms | Where-Object { [string]$_.id -ceq 'Loongnix-LoongArch64' })[0]
if ([string]$loong.evidenceLevel -cne 'blocked-research' -or @($loong.genericRids).Count -ne 0) { throw 'LoongArch64 must remain blocked research without a generic RID.' }
foreach ($platform in @($matrix.platforms)) {
    if (-not [bool]$platform.realHardwareRequired) { throw "Real hardware requirement was relaxed for $($platform.id)." }
    if ([string]$platform.csharpRuntime.minimum -cne '.NET 8' -or [string]$platform.csharpRuntime.preferred -cne '.NET 10') { throw "C# runtime floor drifted for $($platform.id)." }
}
foreach ($token in @('supported .NET 8', 'Mono-only', 'QEMU-only', 'generic linux RID', 'openEuler', '银河麒麟', 'UOS', 'openKylin', 'LoongArch64', 'DISPLAY', 'WAYLAND_DISPLAY', 'VideoIO', 'OpenCL', 'Vulkan', 'VA-API', 'rollback')) { if ($adr.IndexOf($token, [StringComparison]::OrdinalIgnoreCase) -lt 0) { throw "$adrRelativePath is missing the required certification boundary: $token" } }
$invalid = $matrix | ConvertTo-Json -Depth 30 | ConvertFrom-Json
$invalid.status = 'real-supported'
if (Test-Json -Json ($invalid | ConvertTo-Json -Depth 30) -SchemaFile $schemaPath -ErrorAction SilentlyContinue) { throw 'Domestic platform schema accepted a promoted status fixture.' }
$invalid = $matrix | ConvertTo-Json -Depth 30 | ConvertFrom-Json
$invalid.platforms[0].realHardwareRequired = $false
if (Test-Json -Json ($invalid | ConvertTo-Json -Depth 30) -SchemaFile $schemaPath -ErrorAction SilentlyContinue) { throw 'Domestic platform schema accepted a no-real-hardware fixture.' }
$invalid = $matrix | ConvertTo-Json -Depth 30 | ConvertFrom-Json
$invalid.platforms[0].genericRids = @('runtime.openEuler')
if (Test-Json -Json ($invalid | ConvertTo-Json -Depth 30) -ErrorAction SilentlyContinue -SchemaFile $schemaPath) { throw 'Domestic platform schema accepted a distro-specific package identity fixture.' }
Write-Host "DOMESTIC_PLATFORM_CERTIFICATION_MATRIX_OK platforms=$(@($matrix.platforms).Count) candidate=$(@($matrix.platforms | Where-Object evidenceLevel -eq 'certification-candidate').Count) research=$(@($matrix.platforms | Where-Object evidenceLevel -eq 'research-only').Count) blocked=$(@($matrix.platforms | Where-Object evidenceLevel -eq 'blocked-research').Count) package_identity_policy=generic-linux-only"

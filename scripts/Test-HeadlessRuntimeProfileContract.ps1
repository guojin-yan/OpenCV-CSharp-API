param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$contractRelativePath = 'packaging/runtime/runtime-headless-profile-contract.json'
$schemaRelativePath = 'packaging/runtime/runtime-headless-profile-contract.schema.json'
$matrixRelativePath = 'packaging/runtime/runtime-package-matrix.json'
$adrRelativePath = 'docs/articles/headless-runtime-profile-adr.md'
function Resolve-RepoPath([string]$relativePath) { Join-Path $repo ($relativePath -replace '/', [IO.Path]::DirectorySeparatorChar) }
$contractPath = Resolve-RepoPath $contractRelativePath
$schemaPath = Resolve-RepoPath $schemaRelativePath
$matrixPath = Resolve-RepoPath $matrixRelativePath
$adrPath = Resolve-RepoPath $adrRelativePath
foreach ($path in @($contractPath, $schemaPath, $matrixPath, $adrPath)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Headless profile contract surface was not found: $path" }
}
$contract = Get-Content -LiteralPath $contractPath -Raw | ConvertFrom-Json
$matrix = Get-Content -LiteralPath $matrixPath -Raw | ConvertFrom-Json
$adr = [IO.File]::ReadAllText($adrPath)
if (-not (Test-Json -LiteralPath $contractPath -SchemaFile $schemaPath -ErrorAction Stop)) { throw 'Headless profile contract failed JSON Schema validation.' }
if ([string]$contract.status -cne 'candidate-only' -or [bool]$contract.packageIdentityAllowed -or [string]$contract.candidatePackageSuffix -cne '.headless') { throw 'Headless profile must remain candidate-only and non-publishable.' }
if ([string]$contract.activePackageMatrix -cne $matrixRelativePath -or [string]$contract.baseProfile -cne 'full') { throw 'Headless profile active matrix or base profile drifted.' }
if ((@($contract.omittedModules) -join '|') -cne 'highgui' -or @($contract.requiredModules) -contains 'highgui') { throw 'Headless profile must omit highgui without claiming it as a required module.' }
if (@($contract.profiles | Sort-Object) -join '|' -cne 'full|mini') { throw 'Headless profile must retain Full/Mini parity.' }
$targetRids = @($contract.targetRids | ForEach-Object { [string]$_.rid })
if ((@($targetRids | Sort-Object) -join '|') -cne 'debian.12-x64|ubuntu.22.04-x64|ubuntu.24.04-x64') { throw "Headless target RID set drifted: $($targetRids -join ', ')" }
foreach ($target in $targetRids) {
    $active = @($matrix.rids | Where-Object { [string]$_.rid -ceq $target })
    if ($active.Count -ne 1 -or [string]$active[0].platformFamily -cne 'linux') { throw "Headless target is not backed by one active Linux base row: $target" }
}
$matrixText = [IO.File]::ReadAllText($matrixPath)
if ($matrixText.IndexOf('headless', [StringComparison]::OrdinalIgnoreCase) -ge 0) { throw 'Headless candidate must not be represented in the active package matrix.' }
foreach ($pattern in @('libgtk*', 'libgdk*', 'libQt*', 'libX11*', 'libXext*', 'libwayland*')) {
    if (@($contract.dependencyPolicy.forbiddenSonamePatterns) -notcontains $pattern) { throw "Headless forbidden GUI SONAME policy is missing: $pattern" }
}
foreach ($token in @('candidate-only', 'JYPPX.OpenCV.runtime.<rid>.headless', 'highgui', 'NOT_LINKED', 'DISPLAY', 'WAYLAND_DISPLAY', 'LD_LIBRARY_PATH', 'VideoIO', 'promotion gate', 'rollback', 'active matrix')) {
    if ($adr.IndexOf($token, [StringComparison]::OrdinalIgnoreCase) -lt 0) { throw "$adrRelativePath is missing the required headless boundary: $token" }
}
$invalid = $contract | ConvertTo-Json -Depth 30 | ConvertFrom-Json
$invalid.status = 'real-supported'
if (Test-Json -Json ($invalid | ConvertTo-Json -Depth 30) -SchemaFile $schemaPath -ErrorAction SilentlyContinue) { throw 'Headless schema accepted a promoted status fixture.' }
$invalid = $contract | ConvertTo-Json -Depth 30 | ConvertFrom-Json
$invalid.packageIdentityAllowed = $true
if (Test-Json -Json ($invalid | ConvertTo-Json -Depth 30) -SchemaFile $schemaPath -ErrorAction SilentlyContinue) { throw 'Headless schema accepted a publishable identity fixture.' }
$invalid = $contract | ConvertTo-Json -Depth 30 | ConvertFrom-Json
$invalid.omittedModules = @()
if (Test-Json -Json ($invalid | ConvertTo-Json -Depth 30) -SchemaFile $schemaPath -ErrorAction SilentlyContinue) { throw 'Headless schema accepted a missing highgui omission fixture.' }
Write-Host "HEADLESS_RUNTIME_PROFILE_CONTRACT_OK status=$($contract.status) suffix=$($contract.candidatePackageSuffix) targets=$(@($contract.targetRids).Count) profiles=$(@($contract.profiles).Count) omitted=highgui package_identity_allowed=false"

param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
function Resolve-RepoPath([string]$relativePath) { Join-Path $repo ($relativePath -replace '/', [IO.Path]::DirectorySeparatorChar) }

$contractRelativePath = 'packaging/runtime/runtime-opencl-tapi-umat-contract.json'
$schemaRelativePath = 'packaging/runtime/runtime-opencl-tapi-umat-contract.schema.json'
$matrixRelativePath = 'packaging/runtime/runtime-package-matrix.json'
$adrRelativePath = 'docs/articles/opencl-tapi-umat-adr.md'
$contractPath = Resolve-RepoPath $contractRelativePath
$schemaPath = Resolve-RepoPath $schemaRelativePath
$matrixPath = Resolve-RepoPath $matrixRelativePath
$adrPath = Resolve-RepoPath $adrRelativePath
foreach ($path in @($contractPath, $schemaPath, $matrixPath, $adrPath)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "OpenCL/T-API/UMat contract surface was not found: $path" }
}

$contract = Get-Content -LiteralPath $contractPath -Raw | ConvertFrom-Json
$matrix = Get-Content -LiteralPath $matrixPath -Raw | ConvertFrom-Json
$adr = [IO.File]::ReadAllText($adrPath)
if (-not (Test-Json -LiteralPath $contractPath -SchemaFile $schemaPath -ErrorAction Stop)) { throw 'OpenCL/T-API/UMat contract failed JSON Schema validation.' }
if ([string]$contract.status -cne 'research-only' -or [string]$contract.runtimePolicy -cne 'cpu-only') { throw 'OpenCL/T-API/UMat contract must remain research-only and CPU-first.' }
foreach ($field in @('openclExecutionClaimed', 'gpuAccelerationClaimed', 'cudaExecutionClaimed', 'packageIdentityAllowed')) {
    if ([bool]$contract.$field) { throw "OpenCL/T-API/UMat contract must keep $field=false." }
}
if ([string]$contract.activePackageMatrix -cne $matrixRelativePath) { throw 'OpenCL contract active package matrix drifted.' }
if ((@($contract.capabilityModel.states) -join '|') -cne 'Declared|Available|Verified') { throw 'Capability state order drifted.' }
if ([string]$contract.capabilityModel.cuda.status -cne 'not-claimed') { throw 'CUDA must remain a separate non-claimed backend.' }
if ([string]$contract.contextPolicy.globalMutableContext -cne 'forbidden') { throw 'Global mutable OpenCL context must remain forbidden.' }
if ([string]$contract.fallbackPolicy.silentFallbackClaim -cne 'forbidden') { throw 'Silent CPU fallback cannot be reported as GPU execution.' }
if ([string]$contract.synchronizationPolicy.finishBoundary -notmatch 'Finish') { throw 'OpenCL Finish boundary is missing.' }
if (@($contract.conversionPolicy.preserve).Count -lt 6) { throw 'Mat/UMat conversion policy lost layout invariants.' }
if (-not [bool]$contract.benchmarkProtocol.required -or @($contract.benchmarkProtocol.runs).Count -ne 3) { throw 'OpenCL benchmark protocol must require cold, warm, and independent-consumer runs.' }

$matrixText = [IO.File]::ReadAllText($matrixPath)
foreach ($forbidden in @('runtime-opencl', 'opencl-runtime', 'gpu-runtime', 'cuda-runtime', 'UMat')) {
    if ($matrixText.IndexOf($forbidden, [StringComparison]::OrdinalIgnoreCase) -ge 0) { throw "Active package matrix contains a forbidden GPU/OpenCL package identity token: $forbidden" }
}
foreach ($token in @('CPU-first', 'Declared', 'Available', 'Verified', 'thread-local', 'Finish', 'fallback', 'Mat', 'UMat', 'DNN', 'CUDA', 'benchmark', 'transfer', 'ICD', 'rollback', 'Unsupported')) {
    if ($adr.IndexOf($token, [StringComparison]::OrdinalIgnoreCase) -lt 0) { throw "$adrRelativePath is missing the required OpenCL boundary: $token" }
}

$invalid = $contract | ConvertTo-Json -Depth 40 | ConvertFrom-Json
$invalid.status = 'real-supported'
if (Test-Json -Json ($invalid | ConvertTo-Json -Depth 40) -SchemaFile $schemaPath -ErrorAction SilentlyContinue) { throw 'OpenCL schema accepted a promoted status fixture.' }
$invalid = $contract | ConvertTo-Json -Depth 40 | ConvertFrom-Json
$invalid.gpuAccelerationClaimed = $true
if (Test-Json -Json ($invalid | ConvertTo-Json -Depth 40) -SchemaFile $schemaPath -ErrorAction SilentlyContinue) { throw 'OpenCL schema accepted a GPU-claimed fixture.' }
$invalid = $contract | ConvertTo-Json -Depth 40 | ConvertFrom-Json
$invalid.packageIdentityAllowed = $true
if (Test-Json -Json ($invalid | ConvertTo-Json -Depth 40) -SchemaFile $schemaPath -ErrorAction SilentlyContinue) { throw 'OpenCL schema accepted a publishable package identity fixture.' }
$invalid = $contract | ConvertTo-Json -Depth 40 | ConvertFrom-Json
$invalid.contextPolicy.globalMutableContext = 'allowed'
if (Test-Json -Json ($invalid | ConvertTo-Json -Depth 40) -SchemaFile $schemaPath -ErrorAction SilentlyContinue) { throw 'OpenCL schema accepted a global mutable context fixture.' }
$invalid = $contract | ConvertTo-Json -Depth 40 | ConvertFrom-Json
$invalid.fallbackPolicy.silentFallbackClaim = 'allowed'
if (Test-Json -Json ($invalid | ConvertTo-Json -Depth 40) -SchemaFile $schemaPath -ErrorAction SilentlyContinue) { throw 'OpenCL schema accepted a silent fallback claim fixture.' }

Write-Host "OPENCL_TAPI_UMAT_CONTRACT_OK status=$($contract.status) runtime_policy=$($contract.runtimePolicy) states=$(@($contract.capabilityModel.states).Count) package_identity_allowed=false gpu_claimed=false"
Write-Host 'OpenCL/T-API/UMat remains research-only; current published runtime is CPU-only and CUDA is separately unclaimed.'

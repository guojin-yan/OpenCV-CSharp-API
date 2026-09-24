param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$contractRel = 'packaging/dnn/runtime-dnn-diagnostics-contract.json'
$schemaRel = 'packaging/dnn/runtime-dnn-diagnostics-contract.schema.json'
$adrRel = 'docs/articles/dnn-quantization-backend-diagnostics-adr.md'
$contract = Join-Path $repo ($contractRel -replace '/', [IO.Path]::DirectorySeparatorChar)
$schema = Join-Path $repo ($schemaRel -replace '/', [IO.Path]::DirectorySeparatorChar)
$adr = Join-Path $repo ($adrRel -replace '/', [IO.Path]::DirectorySeparatorChar)
foreach ($path in @($contract,$schema,$adr)) { if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "DNN diagnostics contract file missing: $path" } }
if (-not (Test-Json -LiteralPath $contract -SchemaFile $schema -ErrorAction Stop)) { throw 'DNN diagnostics contract failed JSON Schema validation.' }
$value = Get-Content -LiteralPath $contract -Raw | ConvertFrom-Json
$adrText = [IO.File]::ReadAllText($adr)
foreach ($token in @('Declared','Available','Verified','Model failure','scale','zero-point','CPU fallback','promotion')) { if ($adrText.IndexOf($token, [StringComparison]::OrdinalIgnoreCase) -lt 0) { throw "DNN diagnostics ADR is missing boundary token: $token" } }
if ([string]$value.status -cne 'design-only' -or [bool]$value.packageIdentityAllowed -or [bool]$value.publicApiPromoted) { throw 'DNN diagnostics contract must remain design-only and non-promoted.' }
$invalid = $value | ConvertTo-Json -Depth 20 | ConvertFrom-Json; $invalid.publicApiPromoted = $true
if (Test-Json -Json ($invalid | ConvertTo-Json -Depth 20) -SchemaFile $schema -ErrorAction SilentlyContinue) { throw 'DNN diagnostics schema accepted a promoted API fixture.' }
$invalid = $value | ConvertTo-Json -Depth 20 | ConvertFrom-Json; $invalid.executionClaims.silentFallback = 'allowed'
if (Test-Json -Json ($invalid | ConvertTo-Json -Depth 20) -SchemaFile $schema -ErrorAction SilentlyContinue) { throw 'DNN diagnostics schema accepted a silent fallback fixture.' }
Write-Host "DNN_DIAGNOSTICS_CONTRACT_OK status=$($value.status) states=$(@($value.states).Count) promotion_gates=$(@($value.promotionGates).Count) package_identity_allowed=false"

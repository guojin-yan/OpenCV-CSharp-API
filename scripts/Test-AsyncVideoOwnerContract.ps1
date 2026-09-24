param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$contractRel = 'packaging/video/runtime-async-video-contract.json'
$schemaRel = 'packaging/video/runtime-async-video-contract.schema.json'
$adrRel = 'docs/articles/async-video-owner-adr.md'
$contract = Join-Path $repo ($contractRel -replace '/', [IO.Path]::DirectorySeparatorChar)
$schema = Join-Path $repo ($schemaRel -replace '/', [IO.Path]::DirectorySeparatorChar)
$adr = Join-Path $repo ($adrRel -replace '/', [IO.Path]::DirectorySeparatorChar)
foreach ($path in @($contract,$schema,$adr)) { if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Async video contract file missing: $path" } }
if (-not (Test-Json -LiteralPath $contract -SchemaFile $schema -ErrorAction Stop)) { throw 'Async video owner contract failed JSON Schema validation.' }
$value = Get-Content -LiteralPath $contract -Raw | ConvertFrom-Json
$adrText = [IO.File]::ReadAllText($adr)
foreach ($token in @('bounded','Block','DropOldest','DropNewest','dispose','cancellation','EOF','BackendDisconnected','IAsyncEnumerable','synchronous')) { if ($adrText.IndexOf($token, [StringComparison]::OrdinalIgnoreCase) -lt 0) { throw "Async video ADR is missing boundary token: $token" } }
if ([string]$value.status -cne 'design-only' -or [bool]$value.packageIdentityAllowed -or [bool]$value.publicApiPromoted) { throw 'Async video contract must remain design-only and non-promoted.' }
$invalid = $value | ConvertTo-Json -Depth 20 | ConvertFrom-Json; $invalid.publicApiPromoted = $true
if (Test-Json -Json ($invalid | ConvertTo-Json -Depth 20) -SchemaFile $schema -ErrorAction SilentlyContinue) { throw 'Async video schema accepted a promoted public API fixture.' }
$invalid = $value | ConvertTo-Json -Depth 20 | ConvertFrom-Json; $invalid.queuePolicy.capacity = 'unbounded'
if (Test-Json -Json ($invalid | ConvertTo-Json -Depth 20) -SchemaFile $schema -ErrorAction SilentlyContinue) { throw 'Async video schema accepted an unbounded queue fixture.' }
Write-Host "ASYNC_VIDEO_OWNER_CONTRACT_OK status=$($value.status) outcomes=$(@($value.outcomes).Count) queue_policies=$(@($value.queuePolicy.allowed).Count) package_identity_allowed=false"

param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
function Resolve-RepoPath([string]$relativePath) { Join-Path $repo ($relativePath -replace '/', [IO.Path]::DirectorySeparatorChar) }
$contractRelativePath = 'packaging/runtime/runtime-generic-linux-musl-feasibility.json'
$schemaRelativePath = 'packaging/runtime/runtime-generic-linux-musl-feasibility.schema.json'
$matrixRelativePath = 'packaging/runtime/runtime-package-matrix.json'
$graphRelativePath = 'packaging/runtime/runtime-distro-rid-graph.json'
$adrRelativePath = 'docs/articles/generic-linux-musl-feasibility-adr.md'
$contractPath = Resolve-RepoPath $contractRelativePath
$schemaPath = Resolve-RepoPath $schemaRelativePath
$matrixPath = Resolve-RepoPath $matrixRelativePath
$graphPath = Resolve-RepoPath $graphRelativePath
$adrPath = Resolve-RepoPath $adrRelativePath
foreach ($path in @($contractPath, $schemaPath, $matrixPath, $graphPath, $adrPath)) { if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Generic musl feasibility surface was not found: $path" } }
$contract = Get-Content -LiteralPath $contractPath -Raw | ConvertFrom-Json
$matrix = Get-Content -LiteralPath $matrixPath -Raw | ConvertFrom-Json
$graph = Get-Content -LiteralPath $graphPath -Raw | ConvertFrom-Json
$adr = [IO.File]::ReadAllText($adrPath)
if (-not (Test-Json -LiteralPath $contractPath -SchemaFile $schemaPath -ErrorAction Stop)) { throw 'Generic musl feasibility contract failed JSON Schema validation.' }
if ([string]$contract.status -cne 'research-only' -or [bool]$contract.packageIdentityAllowed -or [string]$contract.activePackageMatrix -cne $matrixRelativePath) { throw 'Generic musl feasibility must remain research-only and non-publishable.' }
if ((@($contract.targetRids) -join '|') -cne 'linux-musl-x64|linux-musl-arm64') { throw 'Generic musl target RID set drifted.' }
if ($null -eq $graph.runtimes.PSObject.Properties['linux-musl-x64'] -or $null -eq $graph.runtimes.PSObject.Properties['linux-musl']) { throw 'RID graph lost linux-musl-x64/linux-musl feasibility nodes.' }
if ($null -eq $graph.runtimes.PSObject.Properties['linux-musl-arm64']) { throw 'RID graph must make the ARM64 musl research gap explicit.' }
if (@($matrix.rids | Where-Object { [string]$_.rid -match '^linux-musl' }).Count -ne 0) { throw 'Generic musl RIDs must not enter the active package matrix during feasibility.' }
$alpine = @($matrix.rids | Where-Object { [string]$_.rid -ceq 'alpine.3.20-x64' })
if ($alpine.Count -ne 1 -or [string]$alpine[0].platformFamily -cne 'linux' -or [string]$alpine[0].producer.containerImage -notmatch '@sha256:') { throw 'Alpine x64 compatibility reference drifted or lost digest provenance.' }
if (@($contract.referenceTargets | Where-Object { [string]$_.status -ceq 'compatibility-only' }).Count -ne 1 -or @($contract.referenceTargets | Where-Object { [string]$_.status -ceq 'research-only' }).Count -ne 1) { throw 'Generic musl references must distinguish compatibility-only x64 from research-only ARM64.' }
foreach ($token in @('research-only', 'linux-musl-x64', 'linux-musl-arm64', 'musl', 'glibc', 'QEMU-only', 'native AArch64', 'FFmpeg', 'GStreamer', 'GTK', 'Qt', 'X11', 'Wayland', 'VideoIO', 'rollback', 'exact RID assets')) { if ($adr.IndexOf($token, [StringComparison]::OrdinalIgnoreCase) -lt 0) { throw "$adrRelativePath is missing the required musl boundary: $token" } }
$invalid = $contract | ConvertTo-Json -Depth 30 | ConvertFrom-Json
$invalid.status = 'real-supported'
if (Test-Json -Json ($invalid | ConvertTo-Json -Depth 30) -SchemaFile $schemaPath -ErrorAction SilentlyContinue) { throw 'Generic musl schema accepted a promoted status fixture.' }
$invalid = $contract | ConvertTo-Json -Depth 30 | ConvertFrom-Json
$invalid.packageIdentityAllowed = $true
if (Test-Json -Json ($invalid | ConvertTo-Json -Depth 30) -SchemaFile $schemaPath -ErrorAction SilentlyContinue) { throw 'Generic musl schema accepted a publishable identity fixture.' }
$invalid = $contract | ConvertTo-Json -Depth 30 | ConvertFrom-Json
$invalid.dependencyPolicy.libc = 'glibc-compatible'
if (Test-Json -Json ($invalid | ConvertTo-Json -Depth 30) -SchemaFile $schemaPath -ErrorAction SilentlyContinue) { throw 'Generic musl schema accepted a glibc libc policy fixture.' }
Write-Host "GENERIC_LINUX_MUSL_FEASIBILITY_OK status=$($contract.status) targets=$(@($contract.targetRids).Count) references=$(@($contract.referenceTargets).Count) package_identity_allowed=false arm64_native_required=true"
Write-Host 'Generic musl remains research-only; Alpine compatibility and glibc package rollback identities are unchanged.'

param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$ledgerRel = 'compatibility/v5.0.1-change-ledger.json'
$schemaRel = 'compatibility/v5.0.1-change-ledger.schema.json'
$ledgerPath = Join-Path $repo ($ledgerRel -replace '/', [IO.Path]::DirectorySeparatorChar)
$schemaPath = Join-Path $repo ($schemaRel -replace '/', [IO.Path]::DirectorySeparatorChar)
foreach ($path in @($ledgerPath,$schemaPath)) { if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Change ledger file missing: $path" } }
if (-not (Test-Json -LiteralPath $ledgerPath -SchemaFile $schemaPath -ErrorAction Stop)) { throw 'v5.0.1 change ledger failed JSON Schema validation.' }
$ledger = Get-Content -LiteralPath $ledgerPath -Raw | ConvertFrom-Json
foreach ($entry in @($ledger.entries)) {
    foreach ($evidence in @($entry.evidence)) {
        $path = Join-Path $repo ($evidence -replace '/', [IO.Path]::DirectorySeparatorChar)
        if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Change ledger evidence is missing for $($entry.id): $evidence" }
    }
    if ([string]$entry.surface -eq 'managed-api' -and [string]$entry.category -notin @('additive-safe','tfm-conditional-preview','behavior-fix')) { throw "Managed API ledger entry has invalid category: $($entry.id)" }
}
if ([int]$ledger.nativeAbiVersion -ne 1 -or [string]$ledger.openCvVersion -cne '5.0.0') { throw 'Change ledger OpenCV/ABI identity drifted.' }
Write-Host "V501_CHANGE_LEDGER_OK entries=$(@($ledger.entries).Count) from=$($ledger.fromVersion) to=$($ledger.toVersion) native_abi=$($ledger.nativeAbiVersion)"

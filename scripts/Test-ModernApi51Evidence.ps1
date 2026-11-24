[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [string]$EvidencePath = 'packaging/runtime/modern-api-5.1.0-evidence.json'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'ModernApi51Evidence.Common.ps1')
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$evidenceFullPath = Resolve-ModernApi51Path -RepositoryRoot $repo -Path $EvidencePath
$schemaPath = Join-Path $repo 'packaging/runtime/modern-api-5.1.0-evidence.schema.json'
$snapshotSchemaPath = Join-Path $repo 'packaging/runtime/runtime-capabilities-json.schema.json'
$recordJson = Get-Content -LiteralPath $evidenceFullPath -Raw
if (-not (Test-Json -Json $recordJson -SchemaFile $schemaPath -ErrorAction Stop)) { throw 'Modern API 5.1 evidence schema validation failed.' }
$record = $recordJson | ConvertFrom-Json
$expectedSource = Get-ModernApi51SourceSet -RepositoryRoot $repo
$expectedBindings = Get-ModernApi51PackageBindings -RepositoryRoot $repo -SourceCommit ([string]$record.sourceCommit)

function Assert-ModernApi51Record {
    param([object]$Record)
    $json = $Record | ConvertTo-Json -Depth 30
    if (-not (Test-Json -Json $json -SchemaFile $schemaPath -ErrorAction Stop)) { throw 'Modern API 5.1 evidence schema validation failed.' }
    & git -C $repo merge-base --is-ancestor ([string]$Record.sourceCommit) HEAD 2>$null
    if ($LASTEXITCODE -ne 0) { throw 'Modern API evidence source is not an ancestor of HEAD.' }
    if (($Record.sourceInputSet | ConvertTo-Json -Depth 5 -Compress) -cne ($expectedSource | ConvertTo-Json -Depth 5 -Compress)) { throw 'Modern API source input set drifted.' }
    if (($Record.packageConsumerEvidence | ConvertTo-Json -Compress) -cne ($expectedBindings.reference | ConvertTo-Json -Compress) -or
        ($Record.runtimePackages | ConvertTo-Json -Depth 8 -Compress) -cne ($expectedBindings.packages | ConvertTo-Json -Depth 8 -Compress)) {
        throw 'Modern API runtime package identity or DLL payload differs from the verified 5.1 package evidence.'
    }
    if ((@($Record.capabilityProfiles.profile) -join '|') -cne 'none|full|mini') { throw 'Modern API snapshot profile set drifted.' }
    foreach ($row in @($Record.capabilityProfiles) + @($Record.testRuns)) {
        $expectedDllCount = if ($row.profile -ceq 'full') { 43 } elseif ($row.profile -ceq 'mini') { 7 } else { 0 }
        if ([int]$row.nativeDllCount -ne $expectedDllCount -or
            [bool]$row.isolation.nativeSmokeEnabled -ne ($row.profile -cne 'none') -or
            [bool]$row.isolation.headlessSmokeEnabled -ne ($row.profile -ceq 'mini')) { throw "Modern API isolation flags or DLL count drifted for $($row.profile)." }
    }
    foreach ($row in @($Record.capabilityProfiles)) {
        $snapshotJson = [string]$row.snapshotJson
        if (-not (Test-Json -Json $snapshotJson -SchemaFile $snapshotSchemaPath -ErrorAction Stop)) { throw 'Modern API capability snapshot schema validation failed.' }
        $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.UTF8Encoding]::new($false).GetBytes($snapshotJson))).ToLowerInvariant()
        if ($hash -cne $row.snapshotSha256) { throw 'Modern API capability snapshot hash drifted.' }
        Assert-ModernApi51Snapshot -Snapshot ($snapshotJson | ConvertFrom-Json) -Profile $row.profile
        if ($snapshotJson -match '[A-Za-z]:[\\/]|LD_LIBRARY_PATH|OPENCV_CSHARP_OPENCV_RUNTIME_ROOT') { throw 'Capability snapshot exposed a machine path or environment variable.' }
    }
    $expectedKeys = @('none/net8.0', 'full/net8.0', 'mini/net8.0', 'none/net10.0', 'full/net10.0', 'mini/net10.0')
    $actualKeys = @($Record.testRuns | ForEach-Object { "$($_.profile)/$($_.targetFramework)" })
    if (($actualKeys -join '|') -cne ($expectedKeys -join '|')) { throw 'Modern API test run matrix drifted.' }
    foreach ($row in @($Record.testRuns)) {
        $cases = @(Get-ModernApi51TestCases -Profile $row.profile)
        if ([int]$row.total -ne $cases.Count -or [int]$row.passed -ne $cases.Count -or (@($row.testCases) -join '|') -cne ($cases -join '|')) {
            throw "Modern API exact test inventory/counters drifted for $($row.profile)/$($row.targetFramework)."
        }
        if ($row.profile -ceq 'mini') {
            foreach ($marker in @('HEADLESS_DNN_STATUS=omitted-deterministic-unavailable', 'HEADLESS_VIDEOIO_MISSING_FILE_OPEN=returned-false')) {
                if (-not ([string]$row.headlessOutput).Contains($marker, [StringComparison]::Ordinal)) { throw "Mini headless evidence marker is missing: $marker" }
            }
        }
        elseif ([string]$row.headlessOutput -cne '') { throw 'Headless output was incorrectly attributed to another profile.' }
    }
    foreach ($framework in @('net8.0', 'net10.0')) {
        $rows = @($Record.testRuns | Where-Object targetFramework -CEQ $framework)
        if (@($rows.apiAssemblySha256 | Sort-Object -Unique).Count -ne 1 -or @($rows.testAssemblySha256 | Sort-Object -Unique).Count -ne 1) {
            throw "Modern API profile runs did not use the same current-source assemblies on $framework."
        }
    }
    if (@($Record.capabilityProfiles.apiAssemblySha256 | Sort-Object -Unique).Count -ne 1 -or
        @($Record.capabilityProfiles.sampleAssemblySha256 | Sort-Object -Unique).Count -ne 1) { throw 'Capability profiles did not use identical sample/API assemblies.' }
}

Assert-ModernApi51Record -Record $record
$negativeCases = [ordered]@{
    'source input drift' = { param($r) $r.sourceInputSet.sha256 = '0' * 64 }
    'package hash drift' = { param($r) $r.runtimePackages[0].packageSha256 = '0' * 64 }
    'native DLL hash drift' = { param($r) $r.runtimePackages[0].payload[0].sha256 = '0' * 64 }
    'native DLL omitted' = { param($r) $r.runtimePackages[0].payload = @($r.runtimePackages[0].payload | Select-Object -Skip 1) }
    'false native smoke flag' = { param($r) $r.testRuns[1].isolation.nativeSmokeEnabled = $false }
    'unrestricted loader path' = { param($r) $r.testRuns[1].isolation.pathRestricted = $false }
    'duplicate test run' = { param($r) $r.testRuns[2] = $r.testRuns[1] }
    'test inventory substitution' = { param($r) $r.testRuns[1].testCases[0] = 'UnrelatedTest' }
    'skipped test' = { param($r) $r.testRuns[1].skipped = 1 }
    'false pass count' = { param($r) $r.testRuns[1].passed = 14 }
    'headless execution marker missing' = { param($r) $r.testRuns[2].headlessOutput = '' }
    'preview promotion' = { param($r) $r.limits.stableCodecWriterPromoted = $true }
    'snapshot hash drift' = { param($r) $r.capabilityProfiles[1].snapshotSha256 = '0' * 64 }
    'false no-runtime promotion' = {
        param($r)
        $s = $r.capabilityProfiles[0].snapshotJson | ConvertFrom-Json
        $s.nativeRuntime.state = 'Verified'
        $r.capabilityProfiles[0].snapshotJson = $s | ConvertTo-Json -Depth 20 -Compress
        $r.capabilityProfiles[0].snapshotSha256 = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.UTF8Encoding]::new($false).GetBytes($r.capabilityProfiles[0].snapshotJson))).ToLowerInvariant()
    }
    'optional module overclaim' = {
        param($r)
        $s = $r.capabilityProfiles[1].snapshotJson | ConvertFrom-Json
        $s.optionalModules[0].state = 'Verified'
        $r.capabilityProfiles[1].snapshotJson = $s | ConvertTo-Json -Depth 20 -Compress
        $r.capabilityProfiles[1].snapshotSha256 = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.UTF8Encoding]::new($false).GetBytes($r.capabilityProfiles[1].snapshotJson))).ToLowerInvariant()
    }
}
foreach ($fixture in $negativeCases.GetEnumerator()) {
    $mutant = $recordJson | ConvertFrom-Json
    & $fixture.Value $mutant
    $rejected = $false
    try { Assert-ModernApi51Record -Record $mutant } catch { $rejected = $true }
    if (-not $rejected) { throw "Modern API evidence guard accepted negative fixture: $($fixture.Key)" }
}
Write-Host "MODERN_API_51_EVIDENCE_OK source_commit=$($record.sourceCommit) full_dlls=43 mini_dlls=7 snapshots=3 test_runs=6 passed=76 negative_fixtures=$($negativeCases.Count) preview_promotion=false"

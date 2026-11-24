[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [switch]$RequirePackageArtifacts
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'LocalPackageSbom51.Common.ps1')
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$recordPath = Join-Path $repo 'packaging/release/opencv-5.1.0-local-sbom-evidence.json'
$schemaPath = Join-Path $repo 'packaging/release/opencv-5.1.0-local-sbom-evidence.schema.json'
$recordJson = Get-Content -LiteralPath $recordPath -Raw
$record = ConvertFrom-LocalPackageSbom51Json -Json $recordJson
$expectedSource = Get-LocalPackageSbom51SourceSet -RepositoryRoot $repo
# This immutable Windows measurement used the consumer verifier at sourceCommit.
# The verifier can evolve for Linux SONAMEs without changing any package bytes.
# Reconstruct that one historical input from its real Git object; all production
# and packaging inputs must still match the measured commit.
$historicalConsumer = 'scripts/Test-GitHubPackConsumerRestoreSurface.ps1'
$changedInputs = @(& git -C $repo diff --name-only ([string]$record.sourceCommit) -- @($expectedSource.paths))
if ($LASTEXITCODE -ne 0) { throw 'Cannot inspect measured SBOM source inputs.' }
if (($changedInputs -join '|') -ceq $historicalConsumer) {
    $historicalLines = @(& git -C $repo show "$($record.sourceCommit):$historicalConsumer")
    if ($LASTEXITCODE -ne 0) { throw 'Cannot read historical Windows consumer verifier.' }
    $paths = [string[]]@(& git -C $repo ls-files --cached --others --exclude-standard -- @($expectedSource.paths))
    if ($LASTEXITCODE -ne 0) { throw 'Cannot enumerate measured SBOM source inputs.' }
    [Array]::Sort($paths, [StringComparer]::Ordinal)
    $rows = foreach ($inputPath in $paths) {
        $inputBytes = if ($inputPath -ceq $historicalConsumer) {
            [Text.Encoding]::UTF8.GetBytes((($historicalLines -join "`n").TrimEnd()) + "`n")
        } elseif ([IO.Path]::GetExtension($inputPath) -ceq '.jpg') { [IO.File]::ReadAllBytes((Join-Path $repo $inputPath)) }
        else { [Text.Encoding]::UTF8.GetBytes((([IO.File]::ReadAllText((Join-Path $repo $inputPath)) -replace "`r`n", "`n") -replace "`r", "`n").TrimEnd() + "`n") }
        "$inputPath|$($inputBytes.Length)|$([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($inputBytes)).ToLowerInvariant())"
    }
    $expectedSource.sha256 = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes(($rows -join "`n") + "`n"))).ToLowerInvariant()
}
$bindings = Get-ModernApi51PackageBindings -RepositoryRoot $repo -SourceCommit $record.sourceCommit
$documents = @{}
foreach ($package in $record.packages) {
    if ($package.sbom.path -cne "packaging/release/opencv-5.1.0-local-sbom/$($package.packageId).5.1.0.spdx.json") { throw 'Local SBOM document path is outside its expected scope.' }
    $documents[$package.role] = Get-Content -LiteralPath (Join-Path $repo $package.sbom.path) -Raw
}

function Assert-LocalSbomRecord {
    param([object]$Record, [hashtable]$Documents)
    if (-not (Test-Json -Json ($Record | ConvertTo-Json -Depth 25) -SchemaFile $schemaPath -ErrorAction Stop)) { throw 'Local package SBOM evidence schema failed.' }
    & git -C $repo merge-base --is-ancestor ([string]$Record.sourceCommit) HEAD 2>$null
    if ($LASTEXITCODE -ne 0) { throw 'Local SBOM source is not an ancestor of HEAD.' }
    & git -C $repo diff --quiet ([string]$Record.sourceCommit) -- @($expectedSource.paths) ":(exclude)$historicalConsumer"
    if ($LASTEXITCODE -ne 0) { throw 'Local SBOM source inputs differ from the measured commit.' }
    if (($Record.sourceInputSet | ConvertTo-Json -Depth 5 -Compress) -cne ($expectedSource | ConvertTo-Json -Depth 5 -Compress) -or
        ($Record.nativePackageConsumerEvidence | ConvertTo-Json -Compress) -cne ($bindings.reference | ConvertTo-Json -Compress)) { throw 'Local SBOM source inputs/native evidence binding drifted.' }
    if ($Record.artifactRoot -cne "artifacts/opencv-5.1.0-local-candidate/$($Record.sourceCommit)" -or
        (@($Record.packages.role) -join '|') -cne 'managed|full|mini') { throw 'Local SBOM archive root or package inventory drifted.' }
    foreach ($package in @($Record.packages)) {
        Assert-LocalPackageSbom51Payload -Package $package -Bindings $bindings
        $names = [string[]]@($package.entries.path)
        $sorted = [string[]]$names.Clone(); [Array]::Sort($sorted, [StringComparer]::Ordinal)
        if (($names -join '|') -cne ($sorted -join '|') -or @($names | Sort-Object -Unique).Count -ne $names.Count -or
            @($names | Where-Object { $_ -match '(^|/)\.\.?(/|$)|^/|\\|:' -or $_ -ieq '.signature.p7s' }).Count -ne 0) { throw 'Local SBOM entry path/order/uniqueness drifted.' }
        $folder = if ($package.role -ceq 'managed') { 'nupkg-managed' } else { "nupkg-win-x64-$($package.role)" }
        if ($package.artifactPath -cne "$($Record.artifactRoot)/$folder/$($package.fileName)" -or
            $package.sbom.path -cne "packaging/release/opencv-5.1.0-local-sbom/$($package.packageId).5.1.0.spdx.json") { throw 'Local package/SBOM evidence path drifted.' }
        $json = [string]$Documents[$package.role]
        $bytes = [Text.UTF8Encoding]::new($false).GetBytes($json)
        $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
        if ($hash -cne $package.sbom.sha256 -or [int64]$bytes.Length -ne [int64]$package.sbom.bytes) { throw 'Local SPDX document bytes drifted.' }
        Assert-LocalPackageSbom51Document -Document (ConvertFrom-LocalPackageSbom51Json -Json $json) -Package $package -SourceCommit $Record.sourceCommit -Created $Record.created
    }
    $keys = @($Record.consumers | ForEach-Object { "$($_.profile)/$($_.targetFramework)" })
    if (($keys -join '|') -cne 'full/net8.0|mini/net8.0|full/net10.0|mini/net10.0') { throw 'Local package consumer matrix drifted.' }
    foreach ($consumer in @($Record.consumers)) {
        $marker = if ($consumer.profile -ceq 'full') { 'TARGETED_NATIVE_SMOKE_OK core,imgproc,imgcodecs,videoio,dnn,ml_knn profile=full' } else { 'TARGETED_NATIVE_SMOKE_OK core,imgproc,imgcodecs,videoio,not_linked profile=mini' }
        if ($consumer.marker -cne $marker) { throw 'Local package consumer execution marker drifted.' }
    }
}

Assert-LocalSbomRecord -Record $record -Documents $documents
$artifactCount = 0
foreach ($package in $record.packages) {
    $packagePath = Join-Path $repo $package.artifactPath
    if (-not (Test-Path -LiteralPath $packagePath -PathType Leaf)) {
        if ($RequirePackageArtifacts) { throw "Required measured package artifact is missing: $($package.fileName)" }
        continue
    }
    if ((Get-ModernApi51Sha256 -Path $packagePath) -cne $package.sha256 -or (Get-Item -LiteralPath $packagePath).Length -ne $package.bytes -or
        ((Get-LocalPackageSbom51Manifest -PackagePath $packagePath) | ConvertTo-Json -Depth 5 -Compress) -cne ($package.entries | ConvertTo-Json -Depth 5 -Compress)) { throw 'Local package archive differs from its measured SPDX manifest.' }
    & (Join-Path $repo 'scripts/New-ReleasePackageSbom.ps1') -PackagePath $packagePath -SourceCommit $record.sourceCommit -Created $record.created -OpenCvVersion '5.1.0' -OutputPath (Join-Path $repo $package.sbom.path) -Check
    $artifactCount++
}
$consumerLogsChecked = 0
foreach ($consumer in $record.consumers) {
    $logPath = Join-Path (Join-Path $repo $record.artifactRoot) "logs/consumer-$($consumer.profile)-$($consumer.targetFramework).log"
    if (-not (Test-Path -LiteralPath $logPath -PathType Leaf)) {
        if ($RequirePackageArtifacts) { throw 'Required measured package consumer log is missing.' }
        continue
    }
    $log = [IO.File]::ReadAllText($logPath)
    if ((Get-ModernApi51Sha256 -Path $logPath) -cne $consumer.logSha256 -or
        -not $log.Contains($consumer.marker, [StringComparison]::Ordinal) -or
        -not $log.Contains('Consumer package pairs checked: 1.', [StringComparison]::Ordinal)) { throw 'Local consumer log hash or execution marker drifted.' }
    $consumerLogsChecked++
}

$recordFixtures = [ordered]@{
    'source input drift' = { param($r) $r.sourceInputSet.sha256 = '0' * 64 }
    'archive checksum drift' = { param($r) $r.packages[0].sha256 = '0' * 64 }
    'native DLL substitution' = { param($r) $e = @($r.packages[1].entries | Where-Object path -Like 'runtimes/*')[0]; $e.sha256 = '0' * 64 }
    'missing target framework' = { param($r) $r.packages[0].entries = @($r.packages[0].entries | Where-Object path -NotLike 'lib/net46/*') }
    'missing license' = { param($r) $r.packages[1].entries = @($r.packages[1].entries | Where-Object path -NotLike 'licenses/*') }
    'duplicate package' = { param($r) $r.packages[2] = $r.packages[1] }
    'unsigned-state promotion' = { param($r) $r.limits.signed = $true }
    'approval promotion' = { param($r) $r.limits.approved = $true }
    'release-set promotion' = { param($r) $r.limits.finalReleaseSetComplete = $true }
    'transitive component overclaim' = { param($r) $r.limits.transitiveComponentInventoryComplete = $true }
    'consumer missing' = { param($r) $r.consumers = @($r.consumers | Select-Object -Skip 1) }
    'consumer skipped' = { param($r) $r.consumers[0].nativeSmoke = 'skipped' }
    'false smoke marker' = { param($r) $r.consumers[1].marker = 'TARGETED_NATIVE_SMOKE_OK' }
    'document bytes drift' = { param($r) $r.packages[0].sbom.sha256 = '0' * 64 }
}
foreach ($fixture in $recordFixtures.GetEnumerator()) {
    $mutant = ConvertFrom-LocalPackageSbom51Json -Json $recordJson; & $fixture.Value $mutant
    $rejected = $false
    try { Assert-LocalSbomRecord -Record $mutant -Documents $documents } catch { $rejected = $true }
    if (-not $rejected) { throw "Local SBOM guard accepted negative record fixture: $($fixture.Key)" }
}
$documentFixtures = [ordered]@{
    'file checksum drift' = { param($d) $d.files[0].checksums[1].checksumValue = '0' * 64 }
    'package verification code drift' = { param($d) $d.packages[0].packageVerificationCode.packageVerificationCodeValue = '0' * 40 }
    'file omitted' = { param($d) $d.files = @($d.files | Select-Object -Skip 1) }
    'relationship omitted' = { param($d) $d.relationships = @($d.relationships | Select-Object -Skip 1) }
    'upstream version drift' = { param($d) $d.packages[1].versionInfo = '5.0.0' }
    'source provenance drift' = { param($d) $d.packages[0].sourceInfo = 'unbound source' }
    'package license drift' = { param($d) $d.packages[0].licenseDeclared = 'NOASSERTION' }
    'creation timestamp drift' = { param($d) $d.creationInfo.created = '2000-01-01T00:00:00Z' }
}
foreach ($fixture in $documentFixtures.GetEnumerator()) {
    $document = ConvertFrom-LocalPackageSbom51Json -Json $documents.managed; & $fixture.Value $document
    $rejected = $false
    try { Assert-LocalPackageSbom51Document -Document $document -Package $record.packages[0] -SourceCommit $record.sourceCommit -Created $record.created } catch { $rejected = $true }
    if (-not $rejected) { throw "Local SBOM guard accepted negative SPDX fixture: $($fixture.Key)" }
}
Write-Host "LOCAL_SBOM_51_EVIDENCE_OK packages=3 documents=3 managed_tfms=15 native_dlls=50 consumers=4 artifacts_checked=$artifactCount logs_checked=$consumerLogsChecked negative_fixtures=$($recordFixtures.Count + $documentFixtures.Count) final_release_set=false approved=false"

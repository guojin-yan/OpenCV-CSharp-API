[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [string]$EvidencePath = 'packaging/runtime/opencv-5.1.0-local-linux-distro-consumer-evidence.json',
    [switch]$RequirePackageArtifacts
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'LocalLinux51Distro.Common.ps1')
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$path = if ([IO.Path]::IsPathRooted($EvidencePath)) { $EvidencePath } else { Join-Path $repo $EvidencePath }
$json = [IO.File]::ReadAllText($path)
if (-not (Test-Json -Json $json -SchemaFile (Join-Path $repo 'packaging/runtime/opencv-5.1.0-local-linux-distro-consumer-evidence.schema.json'))) { throw 'Distro evidence schema failed.' }
$record = $json | ConvertFrom-Json
$baselineRelativePath = 'packaging/runtime/opencv-5.1.0-local-linux-preview-evidence.json'
$baselinePath = Join-Path $repo $baselineRelativePath
$baseline = Get-Content $baselinePath -Raw | ConvertFrom-Json
& (Join-Path $repo 'scripts/Test-LocalLinux51Evidence.ps1') -RepositoryRoot $repo -RequirePackageArtifacts:$RequirePackageArtifacts
$sourceInputs = @(Get-LocalLinux51DistroSourceInputs $repo)
$matrixRelativePath = 'packaging/runtime/runtime-generic-linux-5.1-local-consumer-matrix.json'
$matrixPath = Join-Path $repo $matrixRelativePath
$matrixJson = [IO.File]::ReadAllText($matrixPath)
if (-not (Test-Json -Json $matrixJson -SchemaFile (Join-Path $repo 'packaging/runtime/runtime-generic-linux-preview-matrix.schema.json'))) { throw 'Local consumer matrix schema failed.' }
$matrix = $matrixJson | ConvertFrom-Json
$canonicalMatrix = Get-Content (Join-Path $repo 'packaging/runtime/runtime-generic-linux-preview-matrix.json') -Raw | ConvertFrom-Json
foreach ($field in @('$schema', 'schemaVersion', 'status', 'targetRid', 'activePackageIdentityAllowed', 'previewArtifactIdentityAllowed', 'publicationAllowed', 'activePackageMatrix', 'feasibilityContract', 'producer', 'rids', 'profiles')) {
    if (($matrix.$field | ConvertTo-Json -Depth 12 -Compress) -cne ($canonicalMatrix.$field | ConvertTo-Json -Depth 12 -Compress)) { throw 'Local consumer overlay changed the producer/package contract.' }
}
$consumerSpecs = @(
    @('ubuntu.22.04-x64', 'ubuntu', '22.04', 'ubuntu:22.04@sha256:0e0a0fc6d18feda9db1590da249ac93e8d5abfea8f4c3c0c849ce512b5ef8982', 'glibc 2.35'),
    @('debian.12-x64', 'debian', '12', 'debian:12@sha256:9344f8b8992482f80cba753f323adeaf17690076c095ccff6cc9536be98185dc', 'glibc 2.36'),
    @('fedora.44-x64', 'fedora', '44', 'fedora:44@sha256:43b29f65a41eb9c35e1cd5323e3bdf3b655c2357a9f4f1ff2f9c2798e5045d80', 'glibc 2.43')
)
if (($matrix.consumers.rid -join '|') -cne (($consumerSpecs | ForEach-Object { $_[0] }) -join '|')) { throw 'Local consumer distro scope drifted.' }
foreach ($spec in $consumerSpecs) {
    $c = @($matrix.consumers | Where-Object rid -CEQ $spec[0])[0]
    if ($c.distro -cne $spec[1] -or $c.distroVersion -cne $spec[2] -or $c.containerImage -cne $spec[3] -or
        $c.execution -cne 'container' -or $c.runner -cne 'local-docker' -or ($c.profiles -join '|') -cne 'full|mini') { throw 'Local consumer matrix identity drifted.' }
}
$packages = @([pscustomobject]@{
        folder = 'nupkg-managed'; fileName = 'JYPPX.OpenCV.CSharp.API.5.1.0.nupkg'
        sha256 = $baseline.managedPackage.sha256; bytes = $baseline.managedPackage.bytes
    }) + @($baseline.profiles | ForEach-Object { [pscustomobject]@{
        folder = "nupkg-linux-x64-preview-$($_.name)"; fileName = "$($_.packageId).5.1.0.nupkg"
        sha256 = $_.packageSha256; bytes = $_.packageBytes
    } })
$kitSources = @('Directory.Build.props', 'scripts/PackageVersion.ps1', 'scripts/Test-GitHubPackConsumerRestoreSurface.ps1',
    'scripts/Test-GenericLinuxPreviewConsumer.ps1', 'scripts/Initialize-LocalLinux51DistroConsumer.sh', 'scripts/Invoke-LocalLinux51DistroConsumer.ps1',
    'packaging/runtime/runtime-distro-rid-graph.json', $matrixRelativePath)
$expectedKitPaths = @($kitSources | ForEach-Object { "repository/$_" }) + @('toolchain/dotnet.tar.gz', 'toolchain/powershell.tar.gz', 'toolchain/ubuntu-ca-certificates.crt') +
    @($packages | ForEach-Object { "packages/$($_.folder)/$($_.fileName)" })
$graphText = [IO.File]::ReadAllText((Join-Path $repo 'packaging/runtime/runtime-distro-rid-graph.json')).Replace("`r`n", "`n").Replace("`r", "`n").TrimEnd() + "`n"

function Assert-LocalLinux51DistroEvidence {
    param([object]$Record)
    if ($Record.schemaVersion -ne 1 -or $Record.evidenceKind -cne 'local-linux-5.1-three-distro-preview-consumers' -or
        $Record.status -cne 'verified-local-three-distro-x64-preview' -or $Record.openCvVersion -cne '5.1.0' -or $Record.packageVersion -cne '5.1.0' -or
        $Record.execution -cne 'local-docker' -or -not $Record.worktreeChangesIncluded -or $Record.hosted -or $Record.publicationAllowed -or $Record.genericStablePromoted -or
        $Record.dockerHost.platform -cne 'linux' -or $Record.dockerHost.architecture -cne 'x86_64') { throw 'Local distro execution/publication boundary drifted.' }
    & git -C $repo merge-base --is-ancestor $Record.sourceBaseCommit HEAD 2>$null
    if ($LASTEXITCODE -ne 0) { throw 'Distro consumer source is not an ancestor of HEAD.' }
    if ($Record.baselineEvidence.path -cne $baselineRelativePath -or
        $Record.baselineEvidence.sha256 -cne (Get-FileHash -LiteralPath $baselinePath -Algorithm SHA256).Hash.ToLowerInvariant() -or
        $Record.matrix.path -cne $matrixRelativePath -or $Record.matrix.sha256 -cne (Get-FileHash -LiteralPath $matrixPath -Algorithm SHA256).Hash.ToLowerInvariant()) { throw 'Original producer/Ubuntu evidence or local overlay binding drifted.' }
    if (($Record.sourceInputs | ConvertTo-Json -Depth 5 -Compress) -cne ($sourceInputs | ConvertTo-Json -Depth 5 -Compress)) { throw 'Distro consumer source inputs drifted.' }
    if ($Record.baseGraphJson -cne $graphText -or $Record.baseGraphSha256 -cne (Get-LocalLinux51TextHash $graphText)) { throw 'Canonical RID graph was changed or misrepresented.' }
    if (@($Record.kitManifest).Count -ne 14 -or @($Record.kitManifest.path | Select-Object -Unique).Count -ne 14 -or
        @(Compare-Object $expectedKitPaths @($Record.kitManifest.path) -CaseSensitive).Count -ne 0) { throw 'Independent kit contains unexpected source, toolchain or producer inputs.' }
    foreach ($file in $kitSources) {
        $kitFile = @($Record.kitManifest | Where-Object path -CEQ "repository/$file")[0]
        $sourceFile = @($sourceInputs | Where-Object path -CEQ $file)[0]
        if ($kitFile.sha256 -cne $sourceFile.sha256 -or $kitFile.bytes -ne $sourceFile.bytes) { throw 'Executed validation kit differs from source inputs.' }
    }
    foreach ($package in $packages) {
        $kitFile = @($Record.kitManifest | Where-Object path -CEQ "packages/$($package.folder)/$($package.fileName)")[0]
        if ($kitFile.sha256 -cne $package.sha256 -or $kitFile.bytes -ne $package.bytes) { throw 'Consumer kit package differs from original measured bytes.' }
    }
    $toolSpecs = @(
        @('powershell.tar.gz', 72497272, 'dcfe6060fc86abcb859ce1f8f80843ce50bab0585396de56380ed9f25176ac6d'),
        @('dotnet.tar.gz', 240059572, '137268c8ad939c064ff1ee2a6fdf0899d8725377114ea012fbd1ad5fa2550418'),
        @('ubuntu-ca-certificates.crt', 180744, '64e7c24b0536b525fb19b145768bd5e6e12ced0df3db837c34553ea789acd143')
    )
    foreach ($spec in $toolSpecs) {
        $file = @($Record.kitManifest | Where-Object path -CEQ "toolchain/$($spec[0])")[0]
        if ($file.bytes -ne $spec[1] -or $file.sha256 -cne $spec[2]) { throw 'Unverified toolchain/CA archive bytes.' }
    }
    if ($Record.toolchain.powerShellArchiveSha256 -cne $toolSpecs[0][2] -or $Record.toolchain.bootstrapCaSha256 -cne $toolSpecs[2][2] -or
        $Record.toolchain.dotNetArchiveSha512 -cne '51c8b999af9e8dd9998c9edc5944e19a90788862068acd38694e098889054ce8c23d4f0c5cccfa16bf187d044562359e5ee69a9f8ad0bbe913ba90311fbce25b') { throw 'Toolchain digest evidence drifted.' }
    $coverage = $Record.coverage
    if (($coverage.consumerRids -join '|') -cne 'ubuntu.22.04-x64|debian.12-x64|fedora.44-x64' -or ($coverage.profiles -join '|') -cne 'full|mini' -or
        $coverage.totalProfilePairs -ne 6 -or $coverage.newProfilePairs -ne 4 -or $coverage.baselineProfilePairs -ne 2 -or -not $coverage.localThreeDistroConsumerGateComplete -or
        -not $coverage.baselineUbuntuEvidenceReused -or $coverage.newProducerBuildExecuted -or $coverage.baselineProducerSourceCommit -cne $baseline.sourceBaseCommit -or
        $coverage.hostedValidated -or $coverage.arm64Validated -or $coverage.dnnModelInferenceValidated -or $coverage.videoFileOrDeviceValidated -or $coverage.finalReleaseApproved) { throw 'Three-distro coverage overstates actual execution.' }
    if (($Record.consumers.rid -join '|') -cne 'debian.12-x64|fedora.44-x64') { throw 'New consumer evidence matrix is incomplete.' }
    $identities = @($Record.consumers.identity.id) + @($baseline.producerIdentity.id, $baseline.consumerIdentity.id)
    if (@($identities | Select-Object -Unique).Count -ne 4) { throw 'Consumer and producer container identities must be distinct.' }
    $volumes = @($Record.consumers.identity.mounts | Where-Object destination -CEQ '/consumer' | ForEach-Object source)
    if (@($volumes | Select-Object -Unique).Count -ne 2) { throw 'Consumers share a writable data volume.' }
    foreach ($consumer in $Record.consumers) {
        $spec = @($consumerSpecs | Where-Object { $_[0] -ceq $consumer.rid })[0]
        $d = $consumer.identity
        if ($d.platform -cne 'linux' -or $d.image -cne $spec[3] -or $d.imageArchitecture -cne 'amd64' -or $d.imageOs -cne 'linux' -or
            $d.imageId -notmatch '^sha256:[a-f0-9]{64}$' -or $d.id -notmatch '^[a-f0-9]{64}$' -or $d.mounts.Count -ne 2 -or
            @($d.mounts | Where-Object { $_.destination -ceq '/kit' -and $_.type -ceq 'bind' -and -not $_.writable }).Count -ne 1 -or
            @($d.mounts | Where-Object { $_.destination -ceq '/consumer' -and $_.type -ceq 'volume' -and $_.writable }).Count -ne 1 -or
            @($d.mounts | Where-Object source -CIn (@($baseline.producerIdentity.mounts.source) + @($baseline.consumerIdentity.mounts.source))).Count -gt 0) { throw 'Consumer image/architecture/mount isolation drifted.' }
        foreach ($fields in @(@('bootstrapOutput', 'bootstrapOutputSha256'), @('driverOutput', 'driverOutputSha256'), @('preflightJson', 'preflightSha256'))) {
            if ((Get-LocalLinux51TextHash $consumer.($fields[0])) -cne $consumer.($fields[1])) { throw 'Raw consumer evidence bytes changed.' }
        }
        if (-not $consumer.bootstrapOutput.Contains('/kit/toolchain/powershell.tar.gz: OK') -or -not $consumer.bootstrapOutput.Contains('/kit/toolchain/dotnet.tar.gz: OK') -or
            -not $consumer.bootstrapOutput.Contains("LOCAL_LINUX_51_DISTRO_CONSUMER_READY rid=$($consumer.rid)") -or
            -not $consumer.driverOutput.Contains("LOCAL_LINUX_51_DISTRO_CONSUMER_OK rid=$($consumer.rid) compiler_absent=true producer_paths_absent=true loader_overrides_absent=true")) { throw 'Missing actual bootstrap or independent execution marker.' }
        $p = $consumer.preflightJson | ConvertFrom-Json
        if ($p.consumerRid -cne $consumer.rid -or $p.distro -cne $spec[1] -or $p.distroVersion -cne $spec[2] -or $p.libc -cne $spec[4] -or $p.architecture -cne 'x86_64' -or
            $p.execution -cne 'local-docker' -or $p.hosted -or $p.publicationAllowed -or $p.powerShell -cne '7.4.17' -or $p.dotNetSdk -cne '10.0.401' -or
            -not $p.compilerAbsent -or -not $p.producerPathsAbsent -or -not $p.loaderOverridesAbsent -or $p.matrixSha256 -cne $Record.matrix.sha256 -or
            $p.graphBeforeSha256 -cne $Record.baseGraphSha256 -or $p.graphAfterSha256 -cne (Get-LocalLinux51TextHash $p.graphJson)) { throw 'Consumer preflight environment/graph evidence drifted.' }
        $expectedGraph = $graphText | ConvertFrom-Json
        if ($consumer.rid -ceq 'fedora.44-x64') {
            $expectedGraph.runtimes | Add-Member -NotePropertyName 'fedora.44-x64' -NotePropertyValue ([pscustomobject]@{ '#import' = @('linux-x64') })
            if (($p.addedGraphRids -join '|') -cne 'fedora.44-x64') { throw 'Fedora candidate RID overlay is missing.' }
        }
        elseif ($p.addedGraphRids.Count -ne 0 -or $p.graphAfterSha256 -cne $p.graphBeforeSha256) { throw 'Debian consumer unexpectedly changed its RID graph.' }
        $actualGraph = $p.graphJson | ConvertFrom-Json
        if (($actualGraph | ConvertTo-Json -Depth 8 -Compress) -cne ($expectedGraph | ConvertTo-Json -Depth 8 -Compress)) { throw 'Consumer RID graph contains an unrelated change.' }
        if ($consumer.packages.Count -ne 3) { throw 'Independent consumer package set is incomplete.' }
        foreach ($package in $packages) {
            $files = @($consumer.packages | Where-Object { $_.artifactPath.EndsWith("/$($package.folder)/$($package.fileName)", [StringComparison]::Ordinal) })
            if ($files.Count -ne 1 -or $files[0].sha256 -cne $package.sha256 -or $files[0].bytes -ne $package.bytes -or
                -not $files[0].artifactPath.StartsWith("$($consumer.outputDirectory)/", [StringComparison]::Ordinal) -or $files[0].artifactPath.Contains('..')) { throw 'Independent consumer package identity/bytes drifted.' }
        }
        if (($consumer.profiles.name -join '|') -cne 'full|mini') { throw 'Independent consumer profile matrix is incomplete.' }
        foreach ($profile in $consumer.profiles) {
            $original = @($baseline.profiles | Where-Object name -CEQ $profile.name)[0]
            if ((Get-LocalLinux51TextHash $profile.consumerEvidenceJson) -cne $profile.consumerEvidenceSha256 -or
                (Get-LocalLinux51TextHash $profile.consumerOutput) -cne $profile.consumerOutputSha256) { throw 'Raw profile output/evidence changed.' }
            $e = $profile.consumerEvidenceJson | ConvertFrom-Json
            if ($e.SchemaVersion -ne 1 -or $e.EvidenceKind -cne 'generic-linux-preview-package-consumer-native-smoke' -or $e.Status -cne 'preview-only' -or $e.PublicationAllowed -or
                $e.ConsumerRid -cne $consumer.rid -or $e.ConsumerDistro -cne $spec[1] -or $e.ConsumerDistroVersion -cne $spec[2] -or $e.RuntimeProfile -cne $profile.name -or
                $e.PackageId -cne $original.packageId -or $e.PackageVersion -cne '5.1.0.0' -or $e.PreviewPackageSha256 -cne $original.packageSha256 -or
                $e.ManagedPackageSha256 -cne $baseline.managedPackage.sha256 -or -not $e.NativeSmokeExecuted -or $e.RuntimeAssetRid -cne 'linux-x64') { throw 'Consumer package evidence does not prove actual selected native execution.' }
            $marker = if ($profile.name -ceq 'full') { 'TARGETED_NATIVE_SMOKE_OK core,imgproc,imgcodecs,videoio,dnn,ml_knn profile=full' }
                else { 'TARGETED_NATIVE_SMOKE_OK core,imgproc,imgcodecs,videoio,not_linked profile=mini' }
            if (-not $profile.consumerOutput.Contains($marker) -or -not $profile.consumerOutput.Contains('Consumer package pairs checked: 1.') -or
                -not $profile.consumerOutput.Contains("GENERIC_LINUX_PREVIEW_CONSUMER_OK consumer=$($consumer.rid) profile=$($profile.name) package=$($original.packageId) runtime_asset_rid=linux-x64 native_smoke_executed=True publication_allowed=false")) { throw 'Actual restore/build/native smoke success output is missing.' }
        }
    }
}
Assert-LocalLinux51DistroEvidence $record
if ($RequirePackageArtifacts) {
    foreach ($file in $record.consumers.packages) {
        $path = Join-Path $repo $file.artifactPath
        if ((Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant() -cne $file.sha256 -or (Get-Item -LiteralPath $path).Length -ne $file.bytes) { throw 'Retained consumer package artifacts changed or are missing.' }
    }
}

$fixtures = @(
    { param($r) $r.hosted = $true },
    { param($r) $r.publicationAllowed = $true },
    { param($r) $r.genericStablePromoted = $true },
    { param($r) $r.coverage.arm64Validated = $true },
    { param($r) $r.coverage.dnnModelInferenceValidated = $true },
    { param($r) $r.coverage.videoFileOrDeviceValidated = $true },
    { param($r) $r.coverage.totalProfilePairs = 8 },
    { param($r) $r.coverage.baselineProducerSourceCommit = '0' * 40 },
    { param($r) $r.baselineEvidence.sha256 = '0' * 64 },
    { param($r) $r.sourceInputs[0].sha256 = '0' * 64 },
    { param($r) $r.kitManifest += $r.kitManifest[0] },
    { param($r) $r.kitManifest[0].sha256 = '0' * 64 },
    { param($r) $r.dockerHost.architecture = 'aarch64' },
    { param($r) $r.consumers = @($r.consumers[0]) },
    { param($r) $r.consumers[0].identity.image = 'debian:12' },
    { param($r) $r.consumers[0].identity.id = $r.consumers[1].identity.id },
    { param($r) $r.consumers[0].identity.mounts[0].writable = $true },
    { param($r) $r.consumers[0].identity.mounts[1].source = $r.consumers[1].identity.mounts[1].source },
    { param($r) $r.consumers[0].identity.mounts[1].destination = '/workspace' },
    { param($r) $r.consumers[0].profiles = @($r.consumers[0].profiles[0]) },
    { param($r) $r.consumers[0].profiles[0].consumerOutput = 'Skipped'; $r.consumers[0].profiles[0].consumerOutputSha256 = Get-LocalLinux51TextHash 'Skipped' },
    { param($r) $p = $r.consumers[0].preflightJson | ConvertFrom-Json; $p.compilerAbsent = $false; $r.consumers[0].preflightJson = $p | ConvertTo-Json -Depth 8; $r.consumers[0].preflightSha256 = Get-LocalLinux51TextHash $r.consumers[0].preflightJson },
    { param($r) $p = $r.consumers[1].preflightJson | ConvertFrom-Json; $p.graphJson = $r.baseGraphJson; $p.graphAfterSha256 = $r.baseGraphSha256; $r.consumers[1].preflightJson = $p | ConvertTo-Json -Depth 8; $r.consumers[1].preflightSha256 = Get-LocalLinux51TextHash $r.consumers[1].preflightJson },
    { param($r) $p = $r.consumers[0].profiles[0].consumerEvidenceJson | ConvertFrom-Json; $p.NativeSmokeExecuted = $false; $r.consumers[0].profiles[0].consumerEvidenceJson = $p | ConvertTo-Json -Depth 8; $r.consumers[0].profiles[0].consumerEvidenceSha256 = Get-LocalLinux51TextHash $r.consumers[0].profiles[0].consumerEvidenceJson },
    { param($r) $p = $r.consumers[1].profiles[0].consumerEvidenceJson | ConvertFrom-Json; $p.PreviewPackageSha256 = '0' * 64; $r.consumers[1].profiles[0].consumerEvidenceJson = $p | ConvertTo-Json -Depth 8; $r.consumers[1].profiles[0].consumerEvidenceSha256 = Get-LocalLinux51TextHash $r.consumers[1].profiles[0].consumerEvidenceJson }
)
foreach ($fixture in $fixtures) {
    $mutant = $json | ConvertFrom-Json
    & $fixture $mutant
    $rejected = $false
    try { Assert-LocalLinux51DistroEvidence $mutant } catch { $rejected = $true }
    if (-not $rejected) { throw 'Distro evidence guard accepted a negative fixture.' }
}
Write-Host "LOCAL_LINUX_51_DISTRO_EVIDENCE_OK distros=3 profiles=2 baseline_pairs=2 new_pairs=4 negative_fixtures=$($fixtures.Count) hosted=false publication_allowed=false"

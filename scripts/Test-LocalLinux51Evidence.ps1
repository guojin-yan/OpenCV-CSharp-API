[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [string]$EvidencePath = 'packaging/runtime/opencv-5.1.0-local-linux-preview-evidence.json',
    [switch]$RequirePackageArtifacts
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'LocalLinux51.Common.ps1')
$repo = (Resolve-Path $RepositoryRoot).Path
$path = if ([IO.Path]::IsPathRooted($EvidencePath)) { $EvidencePath } else { Join-Path $repo $EvidencePath }
$json = [IO.File]::ReadAllText($path)
$record = $json | ConvertFrom-Json
if (-not (Test-Json -Json $json -SchemaFile (Join-Path $repo 'packaging/runtime/opencv-5.1.0-local-linux-preview-evidence.schema.json') -ErrorAction Stop)) { throw 'Linux evidence schema failed.' }
$source = Get-LocalLinux51SourceSet $repo
$matrix = Get-Content (Join-Path $repo 'packaging/runtime/runtime-generic-linux-preview-matrix.json') -Raw | ConvertFrom-Json
$consumerAst = [System.Management.Automation.Language.Parser]::ParseFile((Join-Path $repo 'scripts/Test-GitHubPackConsumerRestoreSurface.ps1'), [ref]$null, [ref]$null)
$nameFunction = $consumerAst.Find({ param($a) $a -is [System.Management.Automation.Language.FunctionDefinitionAst] -and $a.Name -ceq 'Get-NativeFileNames' }, $true)
if ($null -eq $nameFunction) { throw 'Package consumer naming function missing.' }
. ([scriptblock]::Create($nameFunction.Extent.Text))
foreach ($case in @(@('4.10.0', '410', '4100'), @('5.0.0', '500', '500'), @('5.1.0', '501', '510'), @('5.1.2', '501', '512'))) {
    $names = Get-NativeFileNames -Rid linux-x64 -PlatformFamily linux -Modules @('core') -OpenCvVersion $case[0] -OpenCvBinarySuffix $case[2] -SyntheticRuntimeInputs $false
    if (($names.Modules -join '|') -cne "libopencv_core.so|libopencv_core.so.$($case[1])|libopencv_core.so.$($case[0])") { throw 'ELF SONAME derivation regression.' }
    $windows = Get-NativeFileNames -Rid win-x64 -PlatformFamily windows -Modules @('core') -OpenCvVersion $case[0] -OpenCvBinarySuffix $case[2] -SyntheticRuntimeInputs $false
    if (($windows.Modules -join '|') -cne "opencv_core$($case[2]).dll") { throw 'Windows DLL suffix regression.' }
}
function Assert-LocalLinux51 {
    param([object]$Record)
    if ($Record.schemaVersion -ne 1 -or $Record.evidenceKind -cne 'local-linux-5.1-preview-package-consumer' -or
        $Record.status -cne 'verified-local-ubuntu22-x64-preview' -or $Record.openCvVersion -cne '5.1.0' -or $Record.packageVersion -cne '5.1.0' -or
        $Record.execution -cne 'local-docker' -or $Record.hosted -or $Record.publicationAllowed -or $Record.genericStablePromoted -or -not $Record.worktreeChangesIncluded) { throw 'Linux preview identity or release boundary drifted.' }
    if (($Record.sourceInputSet | ConvertTo-Json -Depth 5 -Compress) -cne ($source | ConvertTo-Json -Depth 5 -Compress)) { throw 'Linux measurement source inputs drifted.' }
    & git -C $repo merge-base --is-ancestor $Record.sourceBaseCommit HEAD 2>$null
    if ($LASTEXITCODE -ne 0) { throw 'Linux measurement base is not an ancestor of HEAD.' }
    $limits = $Record.limits
    if (($limits.consumerDistros -join '|') -cne 'ubuntu.22.04-x64' -or $limits.threeDistroGateComplete -or $limits.arm64Validated -or $limits.hostedValidated -or $limits.finalReleaseApproved) { throw 'Linux evidence overstates measured coverage.' }
    $image = 'ubuntu:22.04@sha256:0e0a0fc6d18feda9db1590da249ac93e8d5abfea8f4c3c0c849ce512b5ef8982'
    foreach ($d in @($Record.producerIdentity, $Record.consumerIdentity)) {
        if ($d.id -notmatch '^[a-f0-9]{64}$' -or $d.image -cne $image -or $d.imageId -notmatch '^sha256:[a-f0-9]{64}$' -or $d.platform -cne 'linux') { throw 'Unpinned or invalid Docker identity.' }
    }
    if ($Record.producerIdentity.id -ceq $Record.consumerIdentity.id -or
        (@($Record.consumerIdentity.mounts.destination) -join '|') -notmatch '^(/kit\|/consumer|/consumer\|/kit)$' -or
        @($Record.consumerIdentity.mounts | Where-Object { $_.destination -ceq '/kit' -and $_.writable }).Count -ne 0 -or
        @($Record.consumerIdentity.mounts | Where-Object source -CIn @($Record.producerIdentity.mounts.source)).Count -gt 0) { throw 'Consumer shares producer inputs or writable kit.' }
    if ((Get-LocalLinux51TextHash $Record.producerJson) -cne $Record.producerSha256) { throw 'Producer output bytes changed.' }
    if ((Get-LocalLinux51TextHash $Record.consumerDriverOutput) -cne $Record.consumerDriverOutputSha256 -or
        -not $Record.consumerDriverOutput.Contains('LOCAL_LINUX_51_INDEPENDENT_CONSUMER_OK compiler_absent=true producer_paths_absent=true loader_overrides_absent=true')) { throw 'Independent consumer isolation checks were not executed.' }
    $producer = $Record.producerJson | ConvertFrom-Json
    if ($producer.sourceCommit -cne $Record.sourceBaseCommit -or $producer.openCvVersion -cne '5.1.0' -or $producer.execution -cne 'local-docker' -or
        $producer.hosted -or $producer.publicationAllowed -or $producer.genericStablePromoted -or $producer.image -cne $image -or
        $producer.imageId -cne $Record.producerIdentity.imageId -or $producer.architecture -cne 'x86_64' -or $producer.libc -cne 'glibc 2.35' -or
        $producer.openCvCommit -cne 'e4f78fbcf1a773fbed69e14500e8b734963eb585' -or $producer.contribCommit -cne 'b67a855188a3d683813731010f6d39ac614733a4' -or
        ($producer.modifiedOpenCvFiles -join '|') -cne 'modules/photo/src/ccm/ccm.cpp' -or -not $producer.contribClean) { throw 'Official source or producer identity drifted.' }
    if ($producer.toolchain.powerShell -cne '7.4.17' -or $producer.toolchain.powerShellArchiveSha256 -cne 'dcfe6060fc86abcb859ce1f8f80843ce50bab0585396de56380ed9f25176ac6d' -or
        $producer.toolchain.dotNetSdk -cne '10.0.401' -or $producer.toolchain.dotNetArchiveSha512 -cne '51c8b999af9e8dd9998c9edc5944e19a90788862068acd38694e098889054ce8c23d4f0c5cccfa16bf187d044562359e5ee69a9f8ad0bbe913ba90311fbce25b') { throw 'Unverified toolchain archive.' }
    if ($Record.managedPackage.sha256 -cne '0081cd584da74aac7e51187985fbe4ceff3406f960525734633a3ba32768fd80' -or $Record.managedPackage.bytes -ne 11924069) { throw 'Managed package binding changed.' }
    if (($Record.profiles.name -join '|') -cne 'full|mini' -or ($producer.profiles.name -join '|') -cne 'full|mini') { throw 'Missing/duplicate Linux profile.' }
    foreach ($p in $Record.profiles) {
        $name = [string]$p.name
        $spec = @($matrix.profiles | Where-Object name -CEQ $name)[0]
        $build = @($producer.profiles | Where-Object name -CEQ $name)[0]
        if ($p.packageId -cne $spec.packageId -or $p.packageVersion -cne '5.1.0' -or $p.packageSha256 -notmatch '^[a-f0-9]{64}$' -or
            $p.packageBytes -le 100000 -or $p.artifactPath -notmatch '^artifacts/[^:]+\.nupkg$' -or $p.artifactPath -match '(^|/)\.\.(/|$)') { throw 'Linux package identity/path drifted.' }
        if ($build.buildList -cne $spec.buildList -or ($build.modules -join '|') -cne ($spec.modules -join '|') -or
            $build.independentMinimalUpstreamBuild -or $build.upstreamBuildProfile -cne 'full' -or
            $build.wrapperSourceCount -ne $(if ($name -ceq 'full') { 49 } else { 9 }) -or
            $build.wrapperSources.Count -ne $build.wrapperSourceCount -or
            @($build.wrapperSources | Select-Object -Unique).Count -ne $build.wrapperSources.Count -or
            $build.ctestOutput -notmatch '100% tests passed, 0 tests failed out of 3') { throw 'Linux build/CTest scope drifted.' }
        foreach ($field in @('producerProvenance', 'packageEvidence', 'consumerEvidence')) {
            if ((Get-LocalLinux51TextHash $p."${field}Json") -cne $p."${field}Sha256") { throw "Changed raw evidence bytes: $field" }
        }
        if ((Get-LocalLinux51TextHash $p.consumerOutput) -cne $p.consumerOutputSha256) { throw 'Changed consumer output bytes.' }
        $provenance = $p.producerProvenanceJson | ConvertFrom-Json
        if ($provenance.SyntheticRuntimeInputs -or -not $provenance.GenericLinuxPreview -or $provenance.OpenCvVersion -cne '5.1.0' -or
            $provenance.RuntimeProfile -cne $name -or $provenance.Rid -cne 'ubuntu.22.04-x64' -or $provenance.HostedRunner -cne '' -or
            $provenance.ContainerImage -cne $image -or $provenance.ContainerImageId -cne $producer.imageId -or
            $provenance.GenericLinuxDependencyPolicy -cne 'bundled-codecs-bundled-protobuf-glibc-cxx-system-only-v2' -or
            ($provenance.RequiredModules -join '|') -cne ($spec.modules -join '|')) { throw 'Producer provenance scope drifted.' }
        $pack = $p.packageEvidenceJson | ConvertFrom-Json
        $consumer = $p.consumerEvidenceJson | ConvertFrom-Json
        if ($pack.PublicationAllowed -or $pack.Status -cne 'preview-only' -or $pack.NativeSmokeExecuted -or -not $pack.ConsumerEvidencePending -or
            $pack.PackageSha256 -cne $p.packageSha256 -or $pack.PackageBytes -ne $p.packageBytes -or
            $pack.ProducerInputProvenanceSha256 -cne $p.producerProvenanceSha256 -or
            $consumer.PublicationAllowed -or $consumer.Status -cne 'preview-only' -or -not $consumer.NativeSmokeExecuted -or
            $consumer.ConsumerRid -cne 'ubuntu.22.04-x64' -or $consumer.RuntimeProfile -cne $name -or
            $consumer.PreviewPackageSha256 -cne $p.packageSha256 -or $consumer.ManagedPackageSha256 -cne $Record.managedPackage.sha256) { throw 'Package and consumer evidence mismatch.' }
        $marker = if ($name -ceq 'full') { 'TARGETED_NATIVE_SMOKE_OK core,imgproc,imgcodecs,videoio,dnn,ml_knn profile=full' }
            else { 'TARGETED_NATIVE_SMOKE_OK core,imgproc,imgcodecs,videoio,not_linked profile=mini' }
        if (-not $p.consumerOutput.Contains($marker) -or -not $p.consumerOutput.Contains('GENERIC_LINUX_PREVIEW_CONSUMER_OK') -or
            -not $p.consumerOutput.Contains('Consumer package pairs checked: 1.')) { throw 'Actual package consumer success marker missing.' }
        $elf = $build.elf
        if ($elf.runtimeFileCount -ne (1 + 3 * $spec.modules.Count) -or $elf.files.Count -ne (1 + $spec.modules.Count) -or
            $elf.exportCount -ne $(if ($name -ceq 'full') { 2669 } else { 527 }) -or $elf.exportNamesSha256 -notmatch '^[a-f0-9]{64}$') { throw 'ELF or ABI count drifted.' }
        $expectedCanonical = @('libJYPPX.OpenCV.Native.so') + @($spec.modules | ForEach-Object { "libopencv_$_.so.5.1.0" })
        if ((@($elf.files.name | Sort-Object) -join '|') -cne (@($expectedCanonical | Sort-Object) -join '|')) { throw 'Canonical ELF inventory drifted.' }
        $allowed = @('libstdc++.so.6', 'libm.so.6', 'libgcc_s.so.1', 'libc.so.6', 'libpthread.so.0', 'libdl.so.2', 'librt.so.1', 'ld-linux-x86-64.so.2')
        $moduleSonames = @($spec.modules | ForEach-Object { "libopencv_$_.so.501" })
        $native = @($p.entries | Where-Object path -Clike 'runtimes/linux-x64/native/*')
        if ($native.Count -ne $elf.runtimeFileCount -or @($native.path | Select-Object -Unique).Count -ne $native.Count) { throw 'NuGet native closure count drifted.' }
        foreach ($f in $elf.files) {
            if (($f.runtimePaths -join '|') -cne '$ORIGIN' -or $f.lddOutput -match 'not found' -or
                @($f.needed | Where-Object { $_ -cnotin ($allowed + $moduleSonames) }).Count -ne 0 -or
                [version]$f.symbolFloors.GLIBC -gt [version]'2.35' -or [version]$f.symbolFloors.GLIBCXX -gt [version]'3.4.30' -or
                [version]$f.symbolFloors.CXXABI -gt [version]'1.3.13') { throw 'ELF loader/dependency/symbol floor drifted.' }
            $names = if ($f.name -ceq 'libJYPPX.OpenCV.Native.so') { @($f.name) }
                else { @($f.name, ($f.name -replace '\.5\.1\.0$', '.501'), ($f.name -replace '\.5\.1\.0$', '')) }
            foreach ($fileName in $names) {
                $entry = @($native | Where-Object path -CEQ "runtimes/linux-x64/native/$fileName")
                if ($entry.Count -ne 1 -or $entry[0].sha256 -cne $f.sha256 -or $entry[0].bytes -ne $f.bytes) { throw 'NuGet ELF bytes differ from audited runtime.' }
            }
            if ($f.name -ceq 'libJYPPX.OpenCV.Native.so' -and @($f.needed | Where-Object { $_ -cin $moduleSonames }).Count -ne $spec.modules.Count) { throw 'Missing direct OpenCV dependency.' }
        }
    }
}
Assert-LocalLinux51 $record
if ($RequirePackageArtifacts) {
    foreach ($p in @($record.profiles) + @($record.managedPackage)) {
        $package = Join-Path $repo $p.artifactPath
        $hash = (Get-FileHash $package -Algorithm SHA256).Hash.ToLowerInvariant()
        $expectedHash = if ($p.PSObject.Properties['packageSha256']) { $p.packageSha256 } else { $p.sha256 }
        if ($hash -cne $expectedHash) { throw 'Measured package archive changed or is missing.' }
        if ($p.PSObject.Properties['entries'] -and
            ((@(Get-LocalLinux51PackageManifest $package) | ConvertTo-Json -Depth 5 -Compress) -cne ($p.entries | ConvertTo-Json -Depth 5 -Compress))) { throw 'Measured archive entries changed.' }
    }
}
$fixtures = @(
    { param($r) $r.publicationAllowed = $true },
    { param($r) $r.hosted = $true },
    { param($r) $r.limits.threeDistroGateComplete = $true },
    { param($r) $r.sourceInputSet.sha256 = '0' * 64 },
    { param($r) $r.consumerIdentity.id = $r.producerIdentity.id },
    { param($r) $r.consumerIdentity.mounts[0].destination = '/workspace' },
    { param($r) $r.profiles = @($r.profiles[0]) },
    { param($r) $r.profiles[0].entries = @($r.profiles[0].entries | Where-Object path -CNE 'runtimes/linux-x64/native/libJYPPX.OpenCV.Native.so') },
    { param($r) $r.profiles[0].packageSha256 = '0' * 64 },
    { param($r) $r.profiles[0].consumerOutput = 'Skipped'; $r.profiles[0].consumerOutputSha256 = Get-LocalLinux51TextHash 'Skipped' },
    { param($r) $p = $r.producerJson | ConvertFrom-Json; $p.profiles[0].elf.files[0].runtimePaths = @('/workspace/lib'); $r.producerJson = $p | ConvertTo-Json -Depth 18; $r.producerSha256 = Get-LocalLinux51TextHash $r.producerJson },
    { param($r) $p = $r.producerJson | ConvertFrom-Json; $p.profiles[0].elf.files[0].needed += 'libprotobuf.so.23'; $r.producerJson = $p | ConvertTo-Json -Depth 18; $r.producerSha256 = Get-LocalLinux51TextHash $r.producerJson }
)
foreach ($fixture in $fixtures) {
    $mutant = $json | ConvertFrom-Json
    & $fixture $mutant
    $rejected = $false
    try { Assert-LocalLinux51 $mutant } catch { $rejected = $true }
    if (-not $rejected) { throw 'Linux evidence guard accepted a negative fixture.' }
}
Write-Host "LOCAL_LINUX_51_EVIDENCE_OK profiles=2 consumer_distros=1 negative_fixtures=$($fixtures.Count) publication_allowed=false"

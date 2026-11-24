[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [string]$EvidencePath = 'packaging/runtime/opencv-5.1.0-local-linux-aot-evidence.json',
    [switch]$RequireArtifacts
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'LocalLinux51Aot.Common.ps1')
$repo = (Resolve-Path $RepositoryRoot).Path
$path = if ([IO.Path]::IsPathRooted($EvidencePath)) { $EvidencePath } else { Join-Path $repo $EvidencePath }
$json = [IO.File]::ReadAllText($path)
$schema = Join-Path $repo 'packaging/runtime/opencv-5.1.0-local-linux-aot-evidence.schema.json'
$sourceInputs = @(Get-LocalLinux51AotSourceInputs $repo)
$specs = @(Get-LocalLinux51AotSpecs)
$referencePaths = @('packaging/runtime/opencv-5.1.0-local-linux-preview-evidence.json',
    'packaging/runtime/opencv-5.1.0-local-linux-module-consumer-evidence.json',
    'packaging/performance/native-aot-smoke-evidence.json',
    'packaging/release/opencv-5.1.0-local-sbom/JYPPX.OpenCV.CSharp.API.5.1.0.spdx.json')
$baseline = Get-Content (Join-Path $repo $referencePaths[0]) -Raw | ConvertFrom-Json
$previous = Get-Content (Join-Path $repo $referencePaths[1]) -Raw | ConvertFrom-Json
$sbom = Get-Content (Join-Path $repo $referencePaths[3]) -Raw | ConvertFrom-Json
$producerSources = [ordered]@{
    'source/Program.cs' = 'tools/LinuxAotPackageSmoke/Program.cs'
    'source/consumer.csproj.in' = 'tools/LinuxAotPackageSmoke/consumer.csproj.in'
    'scripts/Initialize-LocalLinux51AotProducer.sh' = 'scripts/Initialize-LocalLinux51AotProducer.sh'
    'scripts/Build-LocalLinux51AotConsumer.ps1' = 'scripts/Build-LocalLinux51AotConsumer.ps1'
}
$runtimeSources = [ordered]@{
    'scripts/Initialize-LocalLinux51AotConsumer.sh' = 'scripts/Initialize-LocalLinux51AotConsumer.sh'
    'scripts/Invoke-LocalLinux51AotConsumer.sh' = 'scripts/Invoke-LocalLinux51AotConsumer.sh'
}
$toolFiles = @(
    @('toolchain/dotnet.tar.gz', 240059572, '137268c8ad939c064ff1ee2a6fdf0899d8725377114ea012fbd1ad5fa2550418'),
    @('toolchain/powershell.tar.gz', 72497272, 'dcfe6060fc86abcb859ce1f8f80843ce50bab0585396de56380ed9f25176ac6d'),
    @('toolchain/ubuntu-ca-certificates.crt', 180744, '64e7c24b0536b525fb19b145768bd5e6e12ced0df3db837c34553ea789acd143')
)
$packages = @([pscustomobject]@{ path = 'packages/JYPPX.OpenCV.CSharp.API.5.1.0.nupkg'; bytes = $baseline.managedPackage.bytes; sha256 = $baseline.managedPackage.sha256 }) +
    @($baseline.profiles | ForEach-Object { [pscustomobject]@{ path = "packages/$($_.packageId).5.1.0.nupkg"; bytes = $_.packageBytes; sha256 = $_.packageSha256 } })

function Get-AotText([object[]]$Blobs, [string]$Name) {
    $items = @($Blobs | Where-Object name -CEQ $Name)
    if ($items.Count -ne 1) { throw "Missing/duplicate AOT blob: $Name" }
    $blob = $items[0]
    if ((Get-LocalLinux51TextHash $blob.text) -cne $blob.sha256 -or [Text.Encoding]::UTF8.GetByteCount($blob.text) -ne $blob.bytes) { throw 'Raw AOT evidence bytes drifted.' }
    $blob.text
}
function Assert-AotText([string]$Actual, [string]$Expected) {
    if ($Actual.Replace("`r`n", "`n").TrimEnd() -cne $Expected.Replace("`r`n", "`n").TrimEnd()) { throw 'AOT output or configuration differs from the executed contract.' }
}
function Assert-AotManifest([object[]]$Actual, [object[]]$Expected) {
    if ($Actual.Count -ne $Expected.Count -or @($Actual.path | Select-Object -Unique).Count -ne $Expected.Count) { throw 'AOT manifest is incomplete or duplicated.' }
    foreach ($entry in $Expected) {
        $item = @($Actual | Where-Object path -CEQ $entry.path)
        if ($item.Count -ne 1 -or $item[0].bytes -ne $entry.bytes -or $item[0].sha256 -cne $entry.sha256) { throw "AOT manifest payload drifted: $($entry.path)" }
    }
}
function Assert-AotIdentity([object]$Identity, [object]$Spec, [string]$VolumePath, [long]$Memory) {
    if ($Identity.image -cne $Spec.image -or $Identity.memoryBytes -ne $Memory -or
        @($Identity.mounts | Where-Object { $_.destination -ceq '/kit' -and $_.type -ceq 'bind' -and -not $_.writable }).Count -ne 1 -or
        @($Identity.mounts | Where-Object { $_.destination -ceq $VolumePath -and $_.type -ceq 'volume' -and $_.writable }).Count -ne 1) { throw 'AOT image or mount isolation drifted.' }
}

function Assert-LocalLinux51AotEvidence([object]$Record) {
    if (-not (Test-Json -Json ($Record | ConvertTo-Json -Depth 26 -Compress) -SchemaFile $schema -ErrorAction SilentlyContinue)) { throw 'AOT evidence schema failed.' }
    & git -C $repo merge-base --is-ancestor $Record.sourceBaseCommit HEAD 2>$null
    if ($LASTEXITCODE -ne 0) { throw 'AOT source base is not an ancestor of HEAD.' }
    if (($Record.sourceInputs | ConvertTo-Json -Compress) -cne ($sourceInputs | ConvertTo-Json -Compress)) { throw 'AOT source inputs drifted.' }
    for ($i = 0; $i -lt $referencePaths.Count; $i++) {
        if ($Record.references[$i].path -cne $referencePaths[$i] -or $Record.references[$i].sha256 -cne (Get-FileHash (Join-Path $repo $referencePaths[$i]) -Algorithm SHA256).Hash.ToLowerInvariant()) { throw 'Original package, Windows AOT or functional evidence was relabeled.' }
    }
    $expectedProducerKit = @($packages) + @(foreach ($key in $producerSources.Keys) {
        $entry = @($sourceInputs | Where-Object path -CEQ $producerSources[$key])[0]
        [pscustomobject]@{ path = $key; bytes = $entry.bytes; sha256 = $entry.sha256 }
    }) + @($toolFiles | ForEach-Object { [pscustomobject]@{ path = $_[0]; bytes = $_[1]; sha256 = $_[2] } })
    Assert-AotManifest $Record.producerKitManifest $expectedProducerKit
    $producer = $Record.producer
    Assert-AotIdentity $producer.identity $specs[0] '/producer' 3221225472
    if ($producer.outputDirectory -cnotmatch '^artifacts/opencv-5\.1\.0-local-linux-aot/[0-9a-f]{40}/producer$') { throw 'Unsafe AOT producer artifact path.' }
    $bootstrap = Get-AotText $producer.blobs 'bootstrap.log'
    foreach ($marker in @('/kit/toolchain/powershell.tar.gz: OK', '/kit/toolchain/dotnet.tar.gz: OK', 'LOCAL_LINUX_51_AOT_PRODUCER_READY')) {
        if (-not $bootstrap.Contains($marker)) { throw 'AOT producer archive verification/bootstrap was not executed.' }
    }
    Assert-AotText (Get-AotText $producer.blobs 'driver.log') "LOCAL_LINUX_51_AOT_PUBLISH_OK profile=full files=53 warnings=0`nLOCAL_LINUX_51_AOT_PUBLISH_OK profile=mini files=20 warnings=0`nLOCAL_LINUX_51_AOT_PRODUCER_OK profiles=2 warnings=0`n"
    $build = (Get-AotText $producer.blobs 'producer.json') | ConvertFrom-Json
    if ($build.schemaVersion -ne 1 -or $build.sdk -cne '10.0.401' -or $build.runtimeFrameworkVersion -cne '10.0.15' -or
        $build.targetFramework -cne 'net10.0' -or $build.runtimeIdentifier -cne 'linux-x64' -or $build.libc -cne 'glibc 2.35' -or
        $build.architecture -cne 'x86_64' -or $build.clang -notmatch '^Ubuntu clang version 14\.0\.0-' -or
        -not $build.publishAot -or -not $build.publishTrimmed -or $build.trimMode -cne 'partial' -or -not $build.publishSingleFile -or
        -not $build.selfContained -or -not $build.nugetAuditEnabled -or -not $build.nu1903TreatedAsError) { throw 'AOT producer toolchain/settings drifted.' }
    Assert-AotManifest $build.sourceFiles @($expectedProducerKit | Where-Object { $_.path -cin $producerSources.Keys })
    if (($producer.profiles.name -join '|') -cne 'full|mini' -or ($build.profiles.name -join '|') -cne 'full|mini') { throw 'AOT publish profile matrix drifted.' }
    $runtimeKit = @(foreach ($key in $runtimeSources.Keys) {
        $entry = @($sourceInputs | Where-Object path -CEQ $runtimeSources[$key])[0]
        [pscustomobject]@{ path = $key; bytes = $entry.bytes; sha256 = $entry.sha256 }
    }) + @([pscustomobject]@{ path = 'ubuntu-ca-certificates.crt'; bytes = $toolFiles[2][1]; sha256 = $toolFiles[2][2] })
    foreach ($profile in @('full', 'mini')) {
        $native = @($baseline.profiles | Where-Object name -CEQ $profile)[0]
        $published = @($build.profiles | Where-Object name -CEQ $profile)[0]
        $raw = @($producer.profiles | Where-Object name -CEQ $profile)[0]
        if (-not $published.freshProjectAndCache -or $published.warningCount -ne 0) { throw 'AOT publish skipped fresh restoration or contains warnings.' }
        $managedFile = @($sbom.files | Where-Object fileName -CEQ './lib/net10.0/JYPPX.OpenCV.CSharp.API.dll')[0]
        if ($published.managedAssemblySha256 -cne @($managedFile.checksums | Where-Object algorithm -CEQ 'SHA256')[0].checksumValue) { throw 'AOT compiled an assembly outside the managed package.' }
        $expectedNative = @($native.entries | Where-Object path -CLike 'runtimes/linux-x64/native/*.so*' | ForEach-Object {
            [pscustomobject]@{ path = [IO.Path]::GetFileName($_.path); bytes = $_.bytes; sha256 = $_.sha256 }
        })
        $publishedManifest = @($published.publishedFiles | ForEach-Object { [pscustomobject]@{ path = $_.name; bytes = $_.bytes; sha256 = $_.sha256 } })
        Assert-AotManifest @($publishedManifest | Where-Object path -CNE 'LinuxAotConsumer') $expectedNative
        $exe = @($publishedManifest | Where-Object path -CEQ 'LinuxAotConsumer')
        if ($exe.Count -ne 1 -or $exe[0].bytes -lt 1000000 -or $exe[0].sha256 -cnotmatch '^[0-9a-f]{64}$') { throw 'NativeAOT executable is missing.' }
        $runtimeKit += @($publishedManifest | ForEach-Object { [pscustomobject]@{ path = "publish/$profile/$($_.path)"; bytes = $_.bytes; sha256 = $_.sha256 } })
        $template = [IO.File]::ReadAllText((Join-Path $repo 'tools/LinuxAotPackageSmoke/consumer.csproj.in')).Replace('__RUNTIME_PACKAGE__', $native.packageId)
        Assert-AotText (Get-AotText $raw.blobs 'Consumer.csproj') $template
        $config = Get-AotText $raw.blobs 'NuGet.Config'
        if ($config -notmatch '<add key="local" value="/producer/feed"/>' -or $config -notmatch '<add key="nuget" value="https://api.nuget.org/v3/index.json"/>' -or
            $config -match 'disableTLSCertificateValidation|allowInsecureConnections|http:') { throw 'AOT restore feed trust drifted.' }
        $assets = (Get-AotText $raw.blobs 'project.assets.json') | ConvertFrom-Json
        $toolIds = @('Microsoft.DotNet.ILCompiler', 'Microsoft.NET.ILLink.Tasks', 'runtime.linux-x64.Microsoft.DotNet.ILCompiler')
        $expectedLibraries = @('JYPPX.OpenCV.CSharp.API/5.1.0', "$($native.packageId)/5.1.0") + @($toolIds | ForEach-Object { "$_/10.0.15" })
        if (@(Compare-Object $expectedLibraries @($assets.libraries.PSObject.Properties.Name) -CaseSensitive).Count -or
            $assets.project.restore.packagesPath -cne "/producer/build-$profile/nuget" -or
            @($assets.project.restore.sources.PSObject.Properties.Name).Count -ne 2 -or
            @($assets.project.restore.sources.PSObject.Properties.Name | Where-Object { $_ -cnotin @('/producer/feed', 'https://api.nuget.org/v3/index.json') }).Count) { throw 'AOT assets restored a wrong package/toolchain/source/cache.' }
        if ($published.packageArchives.Count -ne 9 -or @($published.packageArchives.path | Select-Object -Unique).Count -ne 9) { throw 'AOT cached package inventory is incomplete.' }
        $metadata = @((Get-AotText $raw.blobs 'nuget-metadata.json') | ConvertFrom-Json)
        if ($metadata.Count -ne 9 -or @($metadata.path | Select-Object -Unique).Count -ne 9) { throw 'AOT NuGet restore metadata is incomplete.' }
        foreach ($archive in $published.packageArchives) {
            if ($archive.bytes -le 0 -or $archive.sha256 -cnotmatch '^[0-9a-f]{64}$' -or $archive.sha512 -cnotmatch '^[0-9a-f]{128}$') { throw 'Missing restored tool archive bytes.' }
            $cacheMetadata = @($metadata | Where-Object path -CEQ $archive.path)
            if ($cacheMetadata.Count -ne 1 -or $cacheMetadata[0].metadata.version -ne 2 -or
                [Convert]::ToHexString([Convert]::FromBase64String($cacheMetadata[0].archiveSha512Base64)).ToLowerInvariant() -cne $archive.sha512 -or
                [Convert]::FromBase64String($cacheMetadata[0].metadata.contentHash).Length -ne 64) { throw 'NuGet restore metadata differs from the signed archive hash.' }
            if ($archive.path.StartsWith('jyppx.', [StringComparison]::Ordinal)) {
                $package = @($packages | Where-Object { [IO.Path]::GetFileName($_.path).ToLowerInvariant() -ceq [IO.Path]::GetFileName($archive.path) })
                if ($package.Count -ne 1 -or $package[0].sha256 -cne $archive.sha256 -or $package[0].bytes -ne $archive.bytes -or $cacheMetadata[0].metadata.source -cne '/producer/feed') { throw 'Restored AOT input package drifted.' }
            } elseif ($archive.path -cnotmatch '^(microsoft\.(aspnetcore\.app\.runtime|dotnet\.ilcompiler|net\.illink\.tasks|netcore\.app\.(host|runtime|runtime\.nativeaot))|runtime\.linux-x64\.microsoft\.dotnet\.ilcompiler)(\.linux-x64)?/10\.0\.15/[^/]+\.10\.0\.15\.nupkg$' -or $cacheMetadata[0].metadata.source -cne 'https://api.nuget.org/v3/index.json') { throw 'AOT cache contains an unexpected runtime/tool version/source.' }
        }
        foreach ($id in $expectedLibraries) {
            $cache = @($metadata | Where-Object { $_.path.StartsWith($id.ToLowerInvariant() + '/', [StringComparison]::Ordinal) })
            if ($cache.Count -ne 1 -or $assets.libraries.$id.sha512 -cne $cache[0].metadata.contentHash) { throw 'Restored assets content hash differs from NuGet restore metadata.' }
        }
        foreach ($step in @('restore', 'publish')) {
            $out = Get-AotText $raw.blobs "$step.stdout.log"; $err = Get-AotText $raw.blobs "$step.stderr.log"
            if (($out + $err) -match '(?im)\b(warning|error)\s+(IL|NU|CS|NETSDK)[0-9]+\s*:' -or $err.Trim().Length) { throw 'AOT restore/publish has unreviewed diagnostics.' }
            if ($step -ceq 'restore' -and $out -notmatch 'Restored /producer/build-' -or $step -ceq 'publish' -and $out -notmatch 'Generating native code') { throw 'AOT restore/native compilation was skipped.' }
        }
        foreach ($step in @('elf-header', 'elf-dynamic', 'elf-versions', 'ldd')) { if ((Get-AotText $raw.blobs "$step.stderr.log").Trim().Length) { throw 'ELF audit failed.' } }
        $header = Get-AotText $raw.blobs 'elf-header.stdout.log'
        $dynamic = Get-AotText $raw.blobs 'elf-dynamic.stdout.log'
        $versions = Get-AotText $raw.blobs 'elf-versions.stdout.log'
        $ldd = Get-AotText $raw.blobs 'ldd.stdout.log'
        if ($header -notmatch 'Class:\s+ELF64' -or $header -notmatch 'Machine:\s+Advanced Micro Devices X86-64' -or $header -notmatch 'Type:\s+DYN' -or
            $dynamic -match 'RPATH|RUNPATH|coreclr|hostfxr|clrjit' -or $ldd -match 'not found|coreclr|hostfxr|clrjit') { throw 'AOT output is not an independently loadable x64 ELF.' }
        $needed = @([regex]::Matches($dynamic, '\(NEEDED\).*\[([^\]]+)\]') | ForEach-Object { $_.Groups[1].Value })
        if (($needed -join '|') -cne 'libm.so.6|libc.so.6|ld-linux-x86-64.so.2') { throw 'AOT executable gained an external dependency.' }
        $floors = @([regex]::Matches($versions, 'GLIBC_([0-9]+\.[0-9]+(?:\.[0-9]+)?)') | ForEach-Object { [version]$_.Groups[1].Value } | Sort-Object -Descending)
        if (-not $floors.Count -or $floors[0] -gt [version]'2.35') { throw 'AOT ELF exceeds the measured glibc floor.' }
    }
    Assert-AotManifest $Record.runtimeKitManifest $runtimeKit
    $identities = @($producer.identity) + @($Record.consumers.identity)
    $oldIdentities = @($baseline.producerIdentity, $baseline.consumerIdentity) + @($previous.consumers.identity)
    if (@($identities.id | Select-Object -Unique).Count -ne 4 -or @($identities.id | Where-Object { $_ -cin $oldIdentities.id }).Count) { throw 'AOT producer/consumers reuse a measured container identity.' }
    $volumes = @($identities.mounts | Where-Object type -CEQ 'volume' | ForEach-Object source)
    if (@($volumes | Select-Object -Unique).Count -ne 4 -or @($volumes | Where-Object { $_ -cin $oldIdentities.mounts.source }).Count) { throw 'AOT consumers share writable producer/consumer data.' }
    $runtimeKits = @($Record.consumers.identity.mounts | Where-Object destination -CEQ '/kit' | ForEach-Object source | Select-Object -Unique)
    if ($runtimeKits.Count -ne 1 -or $runtimeKits[0] -ceq @($producer.identity.mounts | Where-Object destination -CEQ '/kit')[0].source) { throw 'AOT execution kit contains producer tools or inputs.' }
    if (($Record.consumers.rid -join '|') -cne ($specs.rid -join '|')) { throw 'AOT distro matrix is incomplete or duplicated.' }
    for ($i = 0; $i -lt 3; $i++) {
        $consumer = $Record.consumers[$i]; $spec = $specs[$i]
        Assert-AotIdentity $consumer.identity $spec '/consumer' 2147483648
        if ($consumer.outputDirectory -cne ($producer.outputDirectory -replace '/producer$', "/$($spec.rid)")) { throw 'Unsafe AOT runtime artifact path.' }
        $bootstrap = Get-AotText $consumer.blobs 'bootstrap.log'
        if (-not $bootstrap.Contains("LOCAL_LINUX_51_AOT_CONSUMER_READY distro=$($spec.distro) version=$($spec.version)")) { throw 'AOT runtime bootstrap was skipped.' }
        Assert-AotText (Get-AotText $consumer.blobs 'environment.log') "rid=$($spec.rid)`ndistro=$($spec.distro)`nversion=$($spec.version)`narchitecture=x86_64`nlibc=$($spec.libc)`ndotnetAbsent=true`ncompilerAbsent=true`nproducerPathsAbsent=true`nloaderOverridesAbsent=true`n"
        Assert-AotText (Get-AotText $consumer.blobs 'driver.log') "LOCAL_LINUX_51_AOT_RUN_OK rid=$($spec.rid) profile=full`nLOCAL_LINUX_51_AOT_RUN_OK rid=$($spec.rid) profile=mini`nLOCAL_LINUX_51_AOT_CONSUMER_OK rid=$($spec.rid) runs=2`n"
        $scripts = @(foreach ($key in $runtimeSources.Keys) { $entry = @($Record.runtimeKitManifest | Where-Object path -CEQ $key)[0]; "$($entry.sha256)  /kit/$key" })
        Assert-AotText (Get-AotText $consumer.blobs 'scripts.sha256') (($scripts -join "`n") + "`n")
        if (($consumer.runs.profile -join '|') -cne 'full|mini') { throw 'AOT executable profile matrix drifted.' }
        foreach ($run in $consumer.runs) {
            $profile = $run.profile
            $publish = @($build.profiles | Where-Object name -CEQ $profile)[0]
            Assert-AotText (Get-AotText $run.blobs 'exit-code.log') '0'
            $fileNames = [string[]]@($publish.publishedFiles.name)
            [Array]::Sort($fileNames, [StringComparer]::Ordinal)
            $hashLines = @(foreach ($name in $fileNames) { $file = @($publish.publishedFiles | Where-Object name -CEQ $name)[0]; "$($file.sha256)  ./$name" })
            Assert-AotText (Get-AotText $run.blobs 'publish.sha256') (($hashLines -join "`n") + "`n")
            Assert-AotText (Get-AotText $run.blobs 'video.sha256') "$($run.video.sha256)  /consumer/artifacts/$profile/three-frames.avi"
            $dnn = if ($profile -ceq 'full') { 'OpenCV-CPU-identity-values-exact' } else { 'entrypoint-absent' }
            $inferences = if ($profile -ceq 'full') { 2 } else { 0 }
            $expected = "AOT_LINUX_SMOKE_START`nprofile=$profile`nopenCvVersion=5.1.0`nmanagedPackageVersion=5.1.0`nnativeAbi=1`nruntimeVersion=10.0.15`ndynamicCodeSupported=false`ndynamicCodeCompiled=false`npngBytes=71`npngSha256=e62349c3276dddb6c0fb3717badb2b37866ad6e92b8b8556bf0de86917da74a5`npngPixelsExact=true`nvideoBackend=CV_MJPEG`nvideoFrames=3`nvideoChecksum=527760`nvideoMaxError=2`nvideoEof=true`nvideoSha256=35f06c4c8ddfd2f0ac4265353ba17449a576a28fb35c9bc342108950e0963449`nmodelSha256=326793cdb2fc2da739a715c3f3ff71d09779b389ad29e56bbfccc4313e900744`ndnn=$dnn`ndnnInferenceCount=$inferences`n"
            $loaded = [string[]](@('libJYPPX.OpenCV.Native.so') + @($publish.publishedFiles.name | Where-Object { $_ -clike 'libopencv_*.so.501' }))
            [Array]::Sort($loaded, [StringComparer]::Ordinal)
            if ($loaded.Count -ne $(if ($profile -ceq 'full') { 18 } else { 7 })) { throw 'Wrong loaded module closure.' }
            $expected += (($loaded | ForEach-Object { "nativeLoaded=/consumer/publish/$profile/$_" }) -join "`n") + "`nexternalClrLoaded=false`nAOT_LINUX_SMOKE_OK`n"
            Assert-AotText (Get-AotText $run.blobs 'execute.stdout.log') $expected
            $stderr = (Get-AotText $run.blobs 'execute.stderr.log').TrimEnd()
            if ($stderr.Length -and ($profile -cne 'full' -or $stderr -cnotmatch '^\[ WARN:0@[0-9.]+\] global net_impl_backend.cpp:388 setPreferableTarget Targets are not supported by the new graph engine for now$')) { throw 'Unreviewed NativeAOT execution diagnostic.' }
        }
    }
}

function Set-AotBlob([object]$Blob, [string]$Text) {
    $Blob.text = $Text; $Blob.bytes = [Text.Encoding]::UTF8.GetByteCount($Text); $Blob.sha256 = Get-LocalLinux51TextHash $Text
}
function Edit-AotExecution([object]$Record, [string]$Old, [string]$New) {
    $blob = @($Record.consumers[0].runs[0].blobs | Where-Object name -CEQ 'execute.stdout.log')[0]
    Set-AotBlob $blob ($blob.text.Replace($Old, $New))
}
function Edit-AotProducer([object]$Record, [scriptblock]$Mutation) {
    $blob = @($Record.producer.blobs | Where-Object name -CEQ 'producer.json')[0]
    $value = $blob.text | ConvertFrom-Json; & $Mutation $value
    Set-AotBlob $blob (($value | ConvertTo-Json -Depth 20) + "`n")
}
$fixtures = @(
    { param($r) $r.hosted = $true },
    { param($r) $r.publicationAllowed = $true },
    { param($r) $r.coverage.packageWideAotCompatibilityClaimed = $true },
    { param($r) $r.coverage.arm64Validated = $true },
    { param($r) $r.coverage.nativeAotExecutions = 4 },
    { param($r) $r.references[2].sha256 = '0' * 64 },
    { param($r) $r.sourceInputs[0].sha256 = '0' * 64 },
    { param($r) $r.producerKitManifest[0].sha256 = '0' * 64 },
    { param($r) $r.runtimeKitManifest += $r.runtimeKitManifest[0] },
    { param($r) $r.runtimeKitManifest[0].sha256 = '0' * 64 },
    { param($r) $r.consumers[0].identity.id = $r.producer.identity.id },
    { param($r) @($r.consumers[0].identity.mounts | Where-Object destination -CEQ '/consumer')[0].source = @($r.producer.identity.mounts | Where-Object destination -CEQ '/producer')[0].source },
    { param($r) @($r.consumers[0].identity.mounts | Where-Object destination -CEQ '/kit')[0].writable = $true },
    { param($r) $r.consumers[0].identity.oomKilled = $true },
    { param($r) $r.consumers[0].runs[1].profile = 'full' },
    { param($r) Edit-AotProducer $r { param($v) $v.runtimeFrameworkVersion = '10.0.12' } },
    { param($r) Edit-AotProducer $r { param($v) $v.publishAot = $false } },
    { param($r) Edit-AotProducer $r { param($v) $v.profiles[0].publishedFiles[1].sha256 = '0' * 64 } },
    { param($r) Edit-AotProducer $r { param($v) $v.profiles[0].managedAssemblySha256 = '0' * 64 } },
    { param($r) Edit-AotProducer $r { param($v) $v.profiles[0].packageArchives[3].sha512 = '0' * 128 } },
    { param($r) Edit-AotExecution $r 'dynamicCodeSupported=false' 'dynamicCodeSupported=true' },
    { param($r) Edit-AotExecution $r 'externalClrLoaded=false' 'externalClrLoaded=true' },
    { param($r) Edit-AotExecution $r 'AOT_LINUX_SMOKE_OK' 'skipped' },
    { param($r) Edit-AotExecution $r 'profile=full' 'profile=mini' },
    { param($r) Edit-AotExecution $r 'videoChecksum=527760' 'videoChecksum=0' },
    { param($r) Edit-AotExecution $r 'videoEof=true' 'videoEof=false' },
    { param($r) Edit-AotExecution $r 'dnnInferenceCount=2' 'dnnInferenceCount=0' },
    { param($r) Edit-AotExecution $r '/consumer/publish/full/libopencv_core' '/producer/libopencv_core' },
    { param($r) $b = @($r.producer.profiles[0].blobs | Where-Object name -CEQ 'publish.stdout.log')[0]; Set-AotBlob $b '' },
    { param($r) $b = @($r.consumers[0].runs[0].blobs | Where-Object name -CEQ 'execute.stderr.log')[0]; Set-AotBlob $b 'unexpected loader failure' },
    { param($r) $b = @($r.consumers[0].blobs | Where-Object name -CEQ 'environment.log')[0]; Set-AotBlob $b ($b.text.Replace('dotnetAbsent=true', 'dotnetAbsent=false')) },
    { param($r) $b = @($r.producer.profiles[0].blobs | Where-Object name -CEQ 'elf-dynamic.stdout.log')[0]; Set-AotBlob $b ($b.text + "`n (NEEDED) Shared library: [libcoreclr.so]`n") }
)
$record = $json | ConvertFrom-Json
Assert-LocalLinux51AotEvidence $record
foreach ($fixture in $fixtures) {
    $mutant = $json | ConvertFrom-Json; & $fixture $mutant
    $rejected = $false
    try { Assert-LocalLinux51AotEvidence $mutant } catch { $rejected = $true }
    if (-not $rejected) { throw 'AOT evidence guard accepted a negative fixture.' }
}
if ($RequireArtifacts) {
    $groups = @([pscustomobject]@{ directory = $record.producer.outputDirectory; blobs = $record.producer.blobs })
    $groups += @($record.producer.profiles | ForEach-Object { [pscustomobject]@{ directory = "$($record.producer.outputDirectory)/$($_.name)"; blobs = $_.blobs } })
    foreach ($consumer in $record.consumers) {
        $groups += [pscustomobject]@{ directory = $consumer.outputDirectory; blobs = $consumer.blobs }
        foreach ($run in $consumer.runs) {
            $groups += [pscustomobject]@{ directory = "$($consumer.outputDirectory)/$($run.profile)"; blobs = $run.blobs }
            $avi = Join-Path $repo "$($consumer.outputDirectory)/$($run.profile)/three-frames.avi"
            if ((Get-Item $avi).Length -ne $run.video.bytes -or (Get-FileHash $avi -Algorithm SHA256).Hash.ToLowerInvariant() -cne $run.video.sha256) { throw 'Retained AOT video bytes drifted.' }
        }
    }
    foreach ($group in $groups) {
        foreach ($blob in $group.blobs) {
            if ((Get-FileHash (Join-Path $repo "$($group.directory)/$($blob.name)") -Algorithm SHA256).Hash.ToLowerInvariant() -cne $blob.sha256) { throw 'Retained AOT raw evidence drifted.' }
        }
    }
    $build = (Get-AotText $record.producer.blobs 'producer.json') | ConvertFrom-Json
    foreach ($profile in $build.profiles) {
        foreach ($file in $profile.publishedFiles) {
            $actual = Join-Path $repo "$($record.producer.outputDirectory)/$($profile.name)/publish/$($file.name)"
            if ((Get-Item $actual).Length -ne $file.bytes -or (Get-FileHash $actual -Algorithm SHA256).Hash.ToLowerInvariant() -cne $file.sha256) { throw 'Retained NativeAOT publish payload drifted.' }
        }
    }
}
Write-Host "LOCAL_LINUX_51_AOT_EVIDENCE_OK publishes=2 executions=6 native_runtime=10.0.15 distros=3 negative_fixtures=$($fixtures.Count) hosted=false publication_allowed=false"

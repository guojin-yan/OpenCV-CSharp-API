[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [string]$EvidencePath = 'packaging/release/opencv-5.1.0-linux-thirdparty-evidence.json',
    [switch]$RequireArtifacts
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path $RepositoryRoot).Path
$path = if ([IO.Path]::IsPathRooted($EvidencePath)) { $EvidencePath } else { Join-Path $repo $EvidencePath }
$json = [IO.File]::ReadAllText($path)
$schema = Join-Path $repo 'packaging/release/opencv-5.1.0-linux-thirdparty-evidence.schema.json'
function Assert-TP([bool]$Condition, [string]$Message) { if (-not $Condition) { throw $Message } }
function Get-TPHash([byte[]]$Bytes, [string]$Algorithm = 'SHA256') {
    $hasher = [Security.Cryptography.HashAlgorithm]::Create($Algorithm)
    try { [Convert]::ToHexString($hasher.ComputeHash($Bytes)).ToLowerInvariant() } finally { $hasher.Dispose() }
}
function Get-TPTextHash([AllowEmptyString()][string]$Text) { Get-TPHash ([Text.Encoding]::UTF8.GetBytes($Text)) }
function Get-TPBlob([object[]]$Blobs, [string]$Name) {
    $matches = @($Blobs | Where-Object name -CEQ $Name)
    Assert-TP ($matches.Count -eq 1) "Missing or duplicated raw evidence: $Name"
    [string]$matches[0].text
}
function Test-TPBlobs([object[]]$Blobs) {
    Assert-TP (@($Blobs.name | Select-Object -Unique).Count -eq $Blobs.Count) 'Duplicate raw evidence names.'
    foreach ($blob in $Blobs) {
        Assert-TP ($blob.bytes -eq [Text.Encoding]::UTF8.GetByteCount($blob.text) -and $blob.sha256 -ceq (Get-TPTextHash $blob.text)) 'Raw evidence bytes/hash drifted.'
    }
}
function Get-TPTable([object[]]$Rows, [string]$Key = 'path') {
    $table = [Collections.Generic.Dictionary[string,object]]::new([StringComparer]::Ordinal)
    foreach ($row in $Rows) { Assert-TP ($table.TryAdd([string]$row.$Key, $row)) "Duplicate inventory key: $($row.$Key)" }
    return ,$table
}
function Get-TPSorted([string[]]$Values) { [Array]::Sort($Values, [StringComparer]::Ordinal); $Values }
function Test-TPFile([object]$File) {
    $absolute = Join-Path $repo $File.path
    Assert-TP ((Get-Item -LiteralPath $absolute).Length -eq $File.bytes -and (Get-FileHash -LiteralPath $absolute).Hash.ToLowerInvariant() -ceq $File.sha256) "File bytes drifted: $($File.path)"
}
$sourcePaths = @(
    'scripts/Collect-Linux51ThirdPartyBuild.py', 'scripts/New-Linux51ThirdPartyCandidates.py',
    'scripts/Measure-Linux51ThirdPartyEvidence.py', 'scripts/Invoke-Linux51ThirdPartyConsumer.ps1',
    'scripts/Test-Linux51ThirdPartyEvidence.ps1', 'scripts/Initialize-Linux51HeadlessConsumer.sh',
    'tools/LinuxPackageModuleSmoke/Program.cs', 'tools/LinuxHeadlessPackageSmoke/Program.cs',
    'tools/LinuxHeadlessPackageSmoke/consumer.csproj.in', 'tests/OpenCvSharp.Tests/Dnn/Fixtures/identity-opset13.onnx.base64',
    'packaging/release/opencv-5.1.0-linux-thirdparty-evidence.schema.json')
[Array]::Sort($sourcePaths, [StringComparer]::Ordinal)
$sourceInputs = @($sourcePaths | ForEach-Object {
    $text = ([IO.File]::ReadAllText((Join-Path $repo $_)).Replace("`r`n", "`n").Replace("`r", "`n")).TrimEnd() + "`n"
    [pscustomobject]@{ path = $_; bytes = [Text.Encoding]::UTF8.GetByteCount($text); sha256 = Get-TPTextHash $text }
})
$preview = Get-Content (Join-Path $repo 'packaging/runtime/opencv-5.1.0-local-linux-preview-evidence.json') -Raw | ConvertFrom-Json
$headless = Get-Content (Join-Path $repo 'packaging/runtime/opencv-5.1.0-local-linux-headless-evidence.json') -Raw | ConvertFrom-Json
$managedHash = '0081cd584da74aac7e51187985fbe4ceff3406f960525734633a3ba32768fd80'
function Assert-TPEvidence([object]$r) {
    Assert-TP (Test-Json -Json ($r | ConvertTo-Json -Depth 60 -Compress) -SchemaFile $schema -ErrorAction Stop) 'Third-party evidence schema failed.'
    Assert-TP (($r.sourceInputs.path -join '|') -ceq ($sourcePaths -join '|')) 'Source input set drifted.'
    for ($i = 0; $i -lt $sourceInputs.Count; $i++) {
        Assert-TP ($r.sourceInputs[$i].bytes -eq $sourceInputs[$i].bytes -and $r.sourceInputs[$i].sha256 -ceq $sourceInputs[$i].sha256) 'Source input hash drifted.'
    }
    Assert-TP (($r.references.path -join '|') -ceq 'packaging/runtime/opencv-5.1.0-local-linux-preview-evidence.json|packaging/runtime/opencv-5.1.0-local-linux-headless-evidence.json') 'Original evidence reference set drifted.'
    foreach ($ref in $r.references) { Test-TPFile $ref }
    Test-TPBlobs @($r.producer.buildEvidence)
    $b = $r.producer.buildEvidence.text | ConvertFrom-Json
    Assert-TP ($b.openCvVersion -ceq '5.1.0' -and $b.upstreamCommit -ceq 'e4f78fbcf1a773fbed69e14500e8b734963eb585' -and
        -not $b.fullSourceLicenseReviewComplete -and $b.embeddedLeadingNotices -eq 1920 -and
        ($b.modifiedOpenCvFiles -join '|') -ceq 'modules/photo/src/ccm/ccm.cpp') 'Official build provenance or source-review scope drifted.'
    Assert-TP ($b.modules.Count -eq 17 -and $b.archives.Count -eq 8 -and $b.objects.Count -eq 1201 -and $b.compilerDependencyRecords -eq 1201 -and
        $b.observedFileManifest.Count -eq 3079 -and $b.upstreamGitObjects.Count -eq 2054) 'Measured compiler/linker closure is incomplete.'
    $modules = Get-TPTable $b.modules 'module'; $archives = Get-TPTable $b.archives; $objects = Get-TPTable $b.objects
    $observed = Get-TPTable $b.observedFileManifest; $gitObjects = Get-TPTable $b.upstreamGitObjects
    $components = Get-TPTable $b.components 'id'
    Assert-TP (($b.components.id -join '|') -ceq 'Eigen|annoy|flann|flatbuffers|harfbuzz|libjpeg-turbo|libpng|libtiff|libwebp|mlas|mscr|openjpeg|protobuf|softfloat-fdlibm|zlib') 'Unknown, missing or duplicated third-party component.'
    foreach ($a in $b.archives) {
        Assert-TP ($a.objectBytesVerified -and $a.objects.Count -gt 0 -and $a.objects.Count -eq $a.archiveMembers.Count -and $a.path -notmatch 'ade') 'Static archive member-byte evidence missing.'
        $memberNames = @(Get-TPSorted @($a.objects | ForEach-Object { [IO.Path]::GetFileName($_) }))
        Assert-TP (($memberNames -join '|') -ceq ((Get-TPSorted $a.archiveMembers) -join '|')) 'Static archive member closure drifted.'
        foreach ($o in $a.objects) { Assert-TP ($objects.ContainsKey($o)) 'Static archive object missing from the measured object set.' }
    }
    foreach ($c in $b.components) {
        Assert-TP ($c.observedInputs.Count -gt 0 -and $c.notices.Count -gt 0 -and $c.version -ne '' -and $c.sourceRevision -ne '') 'Component input/license provenance missing.'
        foreach ($inputFile in $c.observedInputs) {
            Assert-TP ($observed.ContainsKey($inputFile.path) -and $observed[$inputFile.path].sha256 -ceq $inputFile.sha256 -and $observed[$inputFile.path].bytes -eq $inputFile.bytes) 'Component does not match observed compiler inputs.'
        }
        foreach ($notice in $c.notices) {
            Assert-TP ($notice.bytes -gt 0 -and $notice.sha256 -match '^[a-f0-9]{64}$' -and $notice.path.StartsWith("licenses/third-party/$($c.id)/", [StringComparison]::Ordinal)) 'Component license notice missing.'
            if ($c.id -cne 'Eigen') { Assert-TP ($gitObjects.ContainsKey($notice.source.path)) 'Notice source lacks official Git object provenance.' }
        }
    }
    Assert-TP ($components['Eigen'].observedInputs.Count -eq 343 -and $components['Eigen'].sourceRevision -ceq '3.4.0-2ubuntu2' -and $components['Eigen'].version -ceq '3.4.0') 'Corresponding Eigen source set drifted.'
    foreach ($m in $b.modules) {
        Assert-TP ($m.runpathOnlyInstallChange -and $m.buildElf.bytes -eq $m.installedElf.bytes -and $m.objects.Count -gt 0) 'Installed ELF/build object binding missing.'
        foreach ($a in $m.archives) { Assert-TP ($archives.ContainsKey($a)) 'Unmeasured linked archive.' }
        foreach ($o in $m.objects) { Assert-TP ($objects.ContainsKey($o)) 'Unmeasured linked object.' }
        foreach ($property in $m.componentInputs.PSObject.Properties) {
            Assert-TP ($components.ContainsKey($property.Name) -and $property.Value.Count -gt 0) 'Unknown linked component.'
            foreach ($inputPath in $property.Value) { Assert-TP ($observed.ContainsKey($inputPath)) 'Unobserved linked component input.' }
        }
    }
    Assert-TP (($r.packages | ForEach-Object { "$($_.flavor)/$($_.profile)" }) -join '|' -ceq 'preview/full|preview/mini|headless/full|headless/mini') 'Supplement package matrix incomplete.'
    $kit = Get-TPTable $r.kitManifest
    $kitSources = @{
        'source/preview/Program.cs' = 'tools/LinuxPackageModuleSmoke/Program.cs'; 'source/headless/Program.cs' = 'tools/LinuxHeadlessPackageSmoke/Program.cs'
        'source/consumer.csproj.in' = 'tools/LinuxHeadlessPackageSmoke/consumer.csproj.in'; 'source/identity-opset13.onnx.base64' = 'tests/OpenCvSharp.Tests/Dnn/Fixtures/identity-opset13.onnx.base64'
        'scripts/Invoke-Linux51ThirdPartyConsumer.ps1' = 'scripts/Invoke-Linux51ThirdPartyConsumer.ps1'; 'scripts/Initialize-Linux51HeadlessConsumer.sh' = 'scripts/Initialize-Linux51HeadlessConsumer.sh'
    }
    Assert-TP ($kit.Count -eq 16) 'Consumer kit inventory drifted.'
    foreach ($name in $kitSources.Keys) {
        $actual = [IO.File]::ReadAllText((Join-Path $repo $kitSources[$name]))
        Assert-TP ($kit.ContainsKey($name) -and $kit[$name].sha256 -ceq (Get-TPTextHash $actual)) 'Executed consumer source does not match current source.'
    }
    Assert-TP ($kit['packages/JYPPX.OpenCV.CSharp.API.5.1.0.nupkg'].sha256 -ceq $managedHash) 'Managed package drifted.'
    foreach ($tool in @(
        @('dotnet8.tar.gz', '6f8b64ef9644b11bcab6748ebfec088d2ae4868df26204bb2354c3ada0f09cbe'),
        @('dotnet10.tar.gz', '137268c8ad939c064ff1ee2a6fdf0899d8725377114ea012fbd1ad5fa2550418'),
        @('powershell.tar.gz', 'dcfe6060fc86abcb859ce1f8f80843ce50bab0585396de56380ed9f25176ac6d'),
        @('ubuntu-ca-certificates.crt', '64e7c24b0536b525fb19b145768bd5e6e12ced0df3db837c34553ea789acd143'))) {
        Assert-TP ($kit["toolchain/$($tool[0])"].sha256 -ceq $tool[1]) 'Consumer toolchain provenance drifted.'
    }
    foreach ($p in $r.packages) {
        $version = '5.1.0-' + $(if ($p.flavor -ceq 'headless') {'headless-'} else {''}) + 'licenses.' + $r.sourceBaseCommit.Substring(0, 7)
        $id = 'JYPPX.OpenCV.runtime.linux-x64.preview' + $(if ($p.profile -ceq 'mini') {'.mini'} else {''})
        Assert-TP ($p.packageId -ceq $id -and $p.packageVersion -ceq $version -and $p.sourceBaseCommit -ceq $r.sourceBaseCommit -and $p.buildEvidenceSha256 -ceq $r.producer.buildEvidence.sha256) 'Supplement identity/build binding drifted.'
        $original = if ($p.flavor -ceq 'headless') { @($headless.packages | Where-Object profile -CEQ $p.profile)[0] } else { @($preview.profiles | Where-Object name -CEQ $p.profile)[0] }
        $originalHash = if ($p.flavor -ceq 'headless') {$original.sha256} else {$original.packageSha256}
        $originalVersion = if ($p.flavor -ceq 'headless') {$original.version} else {$original.packageVersion}
        Assert-TP ($p.originalPackageSha256 -ceq $originalHash -and $p.originalPackageVersion -ceq $originalVersion -and $p.originalArtifactPath -ceq $original.artifactPath) 'Original candidate provenance drifted.'
        $entries = Get-TPTable $p.entries; $oldEntries = Get-TPTable $original.entries
        Assert-TP ($entries.Count -eq ($oldEntries.Count + $p.addedEntries.Count)) 'Supplement lost or duplicated archive entries.'
        Assert-TP ($kit[('packages/' + [IO.Path]::GetFileName($p.artifactPath))].sha256 -ceq $p.sha256) 'Consumer feed package bytes drifted.'
        $native = @($p.entries | Where-Object path -CLike 'runtimes/*')
        foreach ($entry in $native) { Assert-TP ($oldEntries.ContainsKey($entry.path) -and $oldEntries[$entry.path].sha256 -ceq $entry.sha256 -and $oldEntries[$entry.path].bytes -eq $entry.bytes) 'Supplement changed original native payload.' }
        Assert-TP ($native.Count -eq @($original.entries | Where-Object path -CLike 'runtimes/*').Count) 'Supplement native closure changed.'
        $selected = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
        $moduleNames = @($native.path | Where-Object { $_ -cmatch '/libopencv_(.+)\.so\.5\.1\.0$' } | ForEach-Object { [void]($_ -cmatch '/libopencv_(.+)\.so\.5\.1\.0$'); $Matches[1] } | Sort-Object -CaseSensitive)
        Assert-TP (($p.modules -join '|') -ceq ($moduleNames -join '|')) 'Packaged module inventory drifted.'
        foreach ($name in $p.modules) {
            $m = $modules[$name]
            Assert-TP ($entries["runtimes/linux-x64/native/libopencv_$name.so.5.1.0"].sha256 -ceq $m.installedElf.sha256) 'Packaged ELF differs from measured compiler/linker build.'
            foreach ($component in $m.componentInputs.PSObject.Properties.Name) { [void]$selected.Add($component) }
        }
        Assert-TP (($p.components -join '|') -ceq ((Get-TPSorted @($selected)) -join '|') -and $p.components.Count -eq $(if ($p.profile -ceq 'full') {15} else {10})) 'Package component closure is incomplete.'
        foreach ($name in $p.components) { foreach ($notice in $components[$name].notices) {
            Assert-TP ($entries.ContainsKey($notice.path) -and $entries[$notice.path].sha256 -ceq $notice.sha256 -and $entries[$notice.path].bytes -eq $notice.bytes -and $p.addedEntries -ccontains $notice.path) 'Package omitted or changed a component license.'
        } }
        foreach ($inputFile in $components['Eigen'].observedInputs) {
            $entry = 'sources/eigen3/' + $inputFile.path.Substring('/usr/include/eigen3/'.Length)
            Assert-TP ($entries.ContainsKey($entry) -and $entries[$entry].sha256 -ceq $inputFile.sha256 -and $entries[$entry].bytes -eq $inputFile.bytes) 'Package omitted corresponding Eigen source.'
        }
        Assert-TP (@($p.entries | Where-Object path -CLike 'sources/eigen3/*').Count -eq 343 -and $entries.ContainsKey('THIRD_PARTY_NOTICES.txt') -and $entries.ContainsKey('licenses/embedded-source-notices.txt')) 'Required source notices or corresponding source missing.'
        Test-TPFile $p.sbom
        $sbom = Get-Content (Join-Path $repo $p.sbom.path) -Raw | ConvertFrom-Json
        Assert-TP ($sbom.spdxVersion -ceq 'SPDX-2.3' -and $sbom.packages.Count -eq ($p.components.Count + 1) -and $sbom.files.Count -eq $p.entries.Count) 'SPDX package/file/component inventory drifted.'
        $spdxFiles = Get-TPTable $sbom.files 'fileName'; $nodes = Get-TPTable $sbom.packages 'name'
        $sha1s = [string[]]@($p.entries.sha1); [Array]::Sort($sha1s, [StringComparer]::Ordinal)
        Assert-TP ($sbom.packages[0].versionInfo -ceq $p.packageVersion -and $sbom.packages[0].checksums[0].checksumValue -ceq $p.sha256 -and
            $sbom.packages[0].packageVerificationCode.packageVerificationCodeValue -ceq (Get-TPHash ([Text.Encoding]::UTF8.GetBytes(($sha1s -join ''))) 'SHA1')) 'SPDX archive binding or verification code drifted.'
        foreach ($entry in $p.entries) {
            Assert-TP ($spdxFiles.ContainsKey('./' + $entry.path)) 'SPDX file omitted.'
            $file = $spdxFiles['./' + $entry.path]; $checks = Get-TPTable $file.checksums 'algorithm'
            Assert-TP ($checks['SHA256'].checksumValue -ceq $entry.sha256 -and $checks['SHA1'].checksumValue -ceq $entry.sha1) 'SPDX file bytes drifted.'
        }
        foreach ($name in $p.components) { Assert-TP ($nodes.ContainsKey($name) -and $nodes[$name].versionInfo -ceq $components[$name].version -and $nodes[$name].licenseDeclared -ceq $components[$name].licenseDeclared) 'SPDX component version/license drifted.' }
        $expectedLinks = @()
        foreach ($name in $p.modules) { foreach ($component in $modules[$name].componentInputs.PSObject.Properties.Name) {
            $relation = if ($component -cin @('zlib','libjpeg-turbo','libwebp','libpng','libtiff','openjpeg','harfbuzz','protobuf','mlas')) {'STATIC_LINK'} else {'GENERATED_FROM'}
            $expectedLinks += "$($spdxFiles["./runtimes/linux-x64/native/libopencv_$name.so.5.1.0"].SPDXID)|$relation|$($nodes[$component].SPDXID)"
        } }
        $actualLinks = @($sbom.relationships | Where-Object relationshipType -CIn @('STATIC_LINK','GENERATED_FROM') | ForEach-Object { "$($_.spdxElementId)|$($_.relationshipType)|$($_.relatedSpdxElement)" })
        Assert-TP (($expectedLinks -join "`n") -ceq ($actualLinks -join "`n")) 'SPDX compiler/linker relationships drifted.'
    }
    $specs = @(
        @('ubuntu.22.04-x64','ubuntu:22.04@sha256:0e0a0fc6d18feda9db1590da249ac93e8d5abfea8f4c3c0c849ce512b5ef8982','glibc 2.35'),
        @('ubuntu.24.04-x64','ubuntu:24.04@sha256:562111b88caa79c71391c0c4d3c2114a7d5c35915c20561fd893cb1e07929e6b','glibc 2.39'),
        @('debian.12-x64','debian:12@sha256:9344f8b8992482f80cba753f323adeaf17690076c095ccff6cc9536be98185dc','glibc 2.36'))
    $ids = @($r.producer.identity.id); $volumes = @()
    for ($index = 0; $index -lt 3; $index++) {
        $c = $r.consumers[$index]; $spec = $specs[$index]; $identity = $c.identity
        Assert-TP ($c.rid -ceq $spec[0] -and $identity.image -ceq $spec[1] -and $identity.architecture -ceq 'amd64' -and $identity.os -ceq 'linux' -and
            -not $identity.oomKilled -and $identity.nanoCpus -eq 2000000000 -and $identity.memoryBytes -eq 2147483648 -and $identity.id -cnotin $ids) 'Consumer identity/resources drifted.'
        $ids += $identity.id
        $mount = @($identity.mounts | Where-Object destination -CEQ '/kit'); $volume = @($identity.mounts | Where-Object destination -CEQ '/consumer')
        Assert-TP ($identity.mounts.Count -eq 2 -and $mount.Count -eq 1 -and -not $mount[0].writable -and $volume.Count -eq 1 -and $volume[0].writable -and
            $volume[0].type -ceq 'volume' -and $volume[0].source -cnotin $volumes -and @($r.producer.identity.mounts | Where-Object source -CEQ $volume[0].source).Count -eq 0) 'Consumer shares producer volume or writable kit.'
        $volumes += $volume[0].source
        Test-TPBlobs $c.blobs
        $environment = (Get-TPBlob $c.blobs 'environment.json') | ConvertFrom-Json
        Assert-TP ($environment.consumerRid -ceq $c.rid -and $environment.libc -ceq $spec[2] -and $environment.compilerAbsent -and $environment.producerPathsAbsent -and $environment.loaderOverridesAbsent) 'Consumer isolation facts drifted.'
        $driver = Get-TPBlob $c.blobs 'driver.log'
        Assert-TP ($driver.Contains("THIRD_PARTY_51_CONSUMER_OK rid=$($c.rid) runs=8") -and (Get-TPBlob $c.blobs 'bootstrap.log').Contains('HEADLESS_51_CONSUMER_READY')) 'Consumer did not finish.'
        $pairs = @($c.runs | ForEach-Object { "$($_.targetFramework)-$($_.flavor)-$($_.profile)" })
        Assert-TP (($pairs -join '|') -ceq 'net8.0-preview-full|net8.0-preview-mini|net8.0-headless-full|net8.0-headless-mini|net10.0-preview-full|net10.0-preview-mini|net10.0-headless-full|net10.0-headless-mini') 'Consumer TFM/flavor/profile matrix incomplete.'
        foreach ($run in $c.runs) {
            Test-TPBlobs $run.blobs
            $p = @($r.packages | Where-Object { $_.flavor -ceq $run.flavor -and $_.profile -ceq $run.profile })[0]
            $tfm = $run.targetFramework; $runtime = if ($tfm -ceq 'net8.0') {'8.0.34'} else {'10.0.12'}; $sdk = if ($tfm -ceq 'net8.0') {'8.0.428'} else {'10.0.401'}
            Assert-TP ($run.packageId -ceq $p.packageId -and $run.packageVersion -ceq $p.packageVersion -and $run.runtimeVersion -ceq $runtime -and $run.sdkVersion -ceq $sdk -and $run.freshPackageCache -and $run.resultDirectory -ceq "$tfm-$($run.flavor)-$($run.profile)") 'Consumer package/runtime selection drifted.'
            $v = (Get-TPBlob $run.blobs 'result.json') | ConvertFrom-Json
            Assert-TP ($v.status -ceq 'passed' -and $v.profile -ceq $run.profile -and $v.targetFramework -ceq $tfm -and $v.runtimeVersion -ceq $runtime -and $v.openCvVersion -ceq '5.1.0' -and $v.nativeAbi -eq 1) 'Actual native execution identity drifted.'
            Assert-TP ((Get-TPBlob $run.blobs 'execute.stdout.log').Trim() -ceq (Get-TPBlob $run.blobs 'result.json').Trim()) 'Native stdout/result mismatch.'
            $native = @($p.entries | Where-Object path -CLike 'runtimes/*'); $nativeFiles = Get-TPTable $v.nativeFiles 'name'
            Assert-TP ($nativeFiles.Count -eq $native.Count) 'Executed native payload incomplete.'
            foreach ($entry in $native) { $file = $nativeFiles[[IO.Path]::GetFileName($entry.path)]; Assert-TP ($null -ne $file -and $file.sha256 -ceq $entry.sha256 -and $file.bytes -eq $entry.bytes) 'Consumer native payload differs from package.' }
            $payload = @((Get-TPBlob $run.blobs 'restored-payload.json') | ConvertFrom-Json); $payloadTable = Get-TPTable $payload
            $expectedPayload = @($p.entries | Where-Object { $_.path.StartsWith('runtimes/') -or $p.addedEntries -ccontains $_.path })
            Assert-TP ($payload.Count -eq $expectedPayload.Count -and $run.restoredPayloadFiles -eq $payload.Count) 'Restored license/source/native inventory incomplete.'
            foreach ($entry in $expectedPayload) { Assert-TP ($payloadTable.ContainsKey($entry.path) -and $payloadTable[$entry.path].sha256 -ceq $entry.sha256 -and $payloadTable[$entry.path].bytes -eq $entry.bytes) 'Restored notice/source/native bytes drifted.' }
            $assets = (Get-TPBlob $run.blobs 'project.assets.json') | ConvertFrom-Json; $libraries = @($assets.libraries.PSObject.Properties.Name)
            Assert-TP ($libraries.Count -eq 2 -and $libraries -ccontains 'JYPPX.OpenCV.CSharp.API/5.1.0' -and $libraries -ccontains "$($p.packageId)/$($p.packageVersion)" -and ($assets.project.restore.sources.PSObject.Properties.Name -join '|') -ceq '/consumer/feed') 'NuGet did not resolve the isolated package pair.'
            $cached = @((Get-TPBlob $run.blobs 'cache-packages.json') | ConvertFrom-Json)
            Assert-TP ($cached.Count -eq 2 -and @($cached | Where-Object sha256 -CEQ $managedHash).Count -eq 1 -and @($cached | Where-Object sha256 -CEQ $p.sha256).Count -eq 1) 'Restored nupkg bytes drifted.'
            $project = [IO.File]::ReadAllText((Join-Path $repo 'tools/LinuxHeadlessPackageSmoke/consumer.csproj.in')).Replace('__TFM__',$tfm).Replace('__RUNTIME_VERSION__',$runtime).Replace('__RUNTIME_PACKAGE__',$p.packageId).Replace('__CANDIDATE_VERSION__',$p.packageVersion)
            Assert-TP ((Get-TPBlob $run.blobs 'Consumer.csproj') -ceq $project) 'Consumer project/runtime roll-forward drifted.'
            $buildLog = Get-TPBlob $run.blobs 'build.stdout.log'
            Assert-TP ($buildLog.Contains('0 Warning(s)') -and $buildLog.Contains('0 Error(s)') -and (Get-TPBlob $run.blobs 'build.stderr.log') -ceq '' -and (Get-TPBlob $run.blobs 'restore.stdout.log') -notmatch '(?i)warning|error :') 'Consumer restore/build failed or emitted warnings.'
            Assert-TP ($v.image.exactPixelRoundtrip -and $v.image.pngSha256 -ceq '85c0160486a1c381e8c7adc062e03491aef8133428ad99be492be0302fe7ada7') 'PNG pixel roundtrip failed.'
            Assert-TP ($v.video.frameCount -eq 3 -and $v.video.endOfFileVerified -and $v.video.fileSha256 -ceq '35f06c4c8ddfd2f0ac4265353ba17449a576a28fb35c9bc342108950e0963449' -and $run.video.sha256 -ceq $v.video.fileSha256 -and $run.video.bytes -eq 6288) 'MJPG file roundtrip failed.'
            if ($run.profile -ceq 'full') {
                Assert-TP ($v.dnn.status -ceq 'passed' -and $v.dnn.inferenceExecuted -and $v.dnn.inferences.Count -eq 2 -and ($v.dnn.inferences[0].output -join '|') -ceq '1|2|3|4' -and ($v.dnn.inferences[1].output -join '|') -ceq '-4|-3|-2|-1') 'Full ONNX inference failed.'
            } else { Assert-TP ($v.dnn.status -ceq 'omitted' -and -not $v.dnn.inferenceExecuted -and $v.dnn.reasonCode -ceq 'entrypoint-absent') 'Mini DNN omission not exercised.' }
            if ($run.flavor -ceq 'headless') {
                Assert-TP ($v.guiLibrariesAbsent -and $v.displayUnset -and $v.nativeStateUsableAfterEachGuiFailure -and $v.highgui.checks.Count -eq 6) 'Headless recovery checks missing.'
                foreach ($check in $v.highgui.checks) { Assert-TP ($check.nativeRecovery -and $check.status -ceq $(if ($run.profile -ceq 'full') {'NOT_LINKED'} else {'entrypoint-absent'})) 'Headless GUI call unexpectedly succeeded or failed recovery.' }
            }
        }
    }
}
function Set-TPBlob([object]$Blob, [string]$Text) { $Blob.text = $Text; $Blob.bytes = [Text.Encoding]::UTF8.GetByteCount($Text); $Blob.sha256 = Get-TPTextHash $Text }
function Edit-TPBuild([object]$Record, [scriptblock]$Edit) {
    $build = $Record.producer.buildEvidence.text | ConvertFrom-Json; & $Edit $build
    Set-TPBlob $Record.producer.buildEvidence (($build | ConvertTo-Json -Depth 40 -Compress) + "`n")
    foreach ($p in $Record.packages) { $p.buildEvidenceSha256 = $Record.producer.buildEvidence.sha256 }
}
function Edit-TPResult([object]$Record, [scriptblock]$Edit) {
    $blobs = $Record.consumers[0].runs[0].blobs; $v = (Get-TPBlob $blobs 'result.json') | ConvertFrom-Json; & $Edit $v
    $text = ($v | ConvertTo-Json -Depth 30 -Compress) + "`n"
    Set-TPBlob @($blobs | Where-Object name -CEQ 'result.json')[0] $text
    Set-TPBlob @($blobs | Where-Object name -CEQ 'execute.stdout.log')[0] $text
}
$record = $json | ConvertFrom-Json
Assert-TPEvidence $record
$fixtures = @(
    {param($r) $r.publicationAllowed = $true}, {param($r) $r.candidateOnly = $false},
    {param($r) $r.activeIdentityAdded = $true}, {param($r) $r.coverage.completeSourceLicenseReview = $true},
    {param($r) $r.coverage.vulnerabilityReviewComplete = $true}, {param($r) $r.coverage.freshUpstreamBuild = $true},
    {param($r) $r.sourceInputs[0].sha256 = '0' * 64}, {param($r) $r.references[0].sha256 = '0' * 64},
    {param($r) Edit-TPBuild $r {param($b) $b.components[0].id = 'unknown-component'}},
    {param($r) Edit-TPBuild $r {param($b) $b.components[1].id = $b.components[0].id}},
    {param($r) Edit-TPBuild $r {param($b) $b.components[0].notices = @()}},
    {param($r) Edit-TPBuild $r {param($b) $b.archives[0].objectBytesVerified = $false}},
    {param($r) Edit-TPBuild $r {param($b) $b.archives[0].archiveMembers = @()}},
    {param($r) Edit-TPBuild $r {param($b) $b.modules[0].runpathOnlyInstallChange = $false}},
    {param($r) $r.packages[0].packageVersion = '5.1.0'},
    {param($r) $r.packages[0].components = @('Eigen')},
    {param($r) @($r.packages[0].entries | Where-Object path -CLike 'runtimes/*')[0].sha256 = '0' * 64},
    {param($r) @($r.packages[0].entries | Where-Object path -CLike 'licenses/third-party/*')[0].sha256 = '0' * 64},
    {param($r) @($r.packages[0].entries | Where-Object path -CLike 'sources/eigen3/*')[0].sha256 = '0' * 64},
    {param($r) $r.packages[0].sbom.sha256 = '0' * 64},
    {param($r) $r.kitManifest[0].sha256 = '0' * 64},
    {param($r) $r.consumers = @($r.consumers[0],$r.consumers[1])},
    {param($r) $r.consumers[1].identity.id = $r.consumers[0].identity.id},
    {param($r) @($r.consumers[0].identity.mounts | Where-Object destination -CEQ '/kit')[0].writable = $true},
    {param($r) $r.consumers[0].runs[0].runtimeVersion = '10.0.12'},
    {param($r) Edit-TPResult $r {param($v) $v.image.exactPixelRoundtrip = $false}},
    {param($r) Edit-TPResult $r {param($v) $v.dnn.inferences[0].output[0] = 99}},
    {param($r) $blobs=$r.consumers[0].runs[0].blobs; $v=@((Get-TPBlob $blobs 'restored-payload.json') | ConvertFrom-Json); $v[0].sha256='0'*64; Set-TPBlob @($blobs|Where-Object name -CEQ 'restored-payload.json')[0] ($v|ConvertTo-Json -Depth 8)}
)
$fixtureIndex = 0
foreach ($fixture in $fixtures) {
    $fixtureIndex++; $mutant = $json | ConvertFrom-Json; & $fixture $mutant; $rejected = $false
    try { Assert-TPEvidence $mutant } catch { $rejected = $true }
    Assert-TP $rejected "Third-party guard accepted negative fixture $fixtureIndex."
}
if ($RequireArtifacts) {
    foreach ($p in $record.packages) {
        Test-TPFile ([pscustomobject]@{ path=$p.artifactPath; bytes=$p.bytes; sha256=$p.sha256 })
        $archive = [IO.Compression.ZipFile]::OpenRead((Join-Path $repo $p.artifactPath))
        try {
            Assert-TP ($archive.Entries.Count -eq $p.entries.Count) 'Retained ZIP inventory drifted.'
            foreach ($entry in $p.entries) {
                $actual = $archive.GetEntry($entry.path); Assert-TP ($null -ne $actual -and $actual.Length -eq $entry.bytes) 'Retained ZIP entry missing.'
                $stream = $actual.Open(); $memory = [IO.MemoryStream]::new()
                try { $stream.CopyTo($memory); $data = $memory.ToArray() } finally { $stream.Dispose(); $memory.Dispose() }
                Assert-TP ((Get-TPHash $data) -ceq $entry.sha256 -and (Get-TPHash $data 'SHA1') -ceq $entry.sha1) 'Retained ZIP entry bytes drifted.'
                if ($entry.path -ceq 'THIRD_PARTY_NOTICES.txt') { Assert-TP ([Text.Encoding]::UTF8.GetString($data).Contains('This software is based in part on the work of the Independent JPEG Group.')) 'JPEG acknowledgement missing.' }
                if ($entry.path.EndsWith('.nuspec') -or $entry.path.EndsWith('.psmdcp')) {
                    $metadata = [xml][Text.Encoding]::UTF8.GetString($data)
                    $versions = @($metadata.DocumentElement.GetElementsByTagName('version', $metadata.DocumentElement.NamespaceURI))
                    Assert-TP ($versions.Count -eq 1 -and $versions[0].InnerText -ceq $p.packageVersion) 'NuGet manifest/core-properties version drifted.'
                }
            }
        } finally { $archive.Dispose() }
    }
    $groups = @([pscustomobject]@{ directory=$record.producer.outputDirectory; blobs=@($record.producer.buildEvidence) })
    foreach ($consumer in $record.consumers) {
        $groups += [pscustomobject]@{ directory=$consumer.outputDirectory; blobs=$consumer.blobs }
        foreach ($run in $consumer.runs) {
            $directory = "$($consumer.outputDirectory)/$($run.resultDirectory)"; $groups += [pscustomobject]@{directory=$directory;blobs=$run.blobs}
            Test-TPFile ([pscustomobject]@{path="$directory/three-frames.avi";bytes=$run.video.bytes;sha256=$run.video.sha256})
        }
    }
    foreach ($group in $groups) { foreach ($blob in $group.blobs) { Test-TPFile ([pscustomobject]@{path="$($group.directory)/$($blob.name)";bytes=$blob.bytes;sha256=$blob.sha256}) } }
}
Write-Host "LINUX_51_THIRD_PARTY_EVIDENCE_OK components=15 archives=8 objects=1201 packages=4 distros=3 executions=24 eigen_source_files=343 negative_fixtures=$($fixtures.Count) candidate_only=true"

[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [string]$EvidencePath = 'packaging/runtime/opencv-5.1.0-local-fedora44-evidence.json',
    [switch]$RequireArtifacts
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'LocalFedora51.Common.ps1')
$repo = (Resolve-Path $RepositoryRoot).Path
$path = if ([IO.Path]::IsPathRooted($EvidencePath)) { $EvidencePath } else { Join-Path $repo $EvidencePath }
$json = [IO.File]::ReadAllText($path)
$schema = Join-Path $repo 'packaging/runtime/opencv-5.1.0-local-fedora44-evidence.schema.json'
$sourceInputs = @(Get-LocalFedora51SourceInputs $repo)
$matrix = Get-Content (Join-Path $repo 'packaging/runtime/runtime-lifecycle-refresh-matrix.json') -Raw | ConvertFrom-Json
$sbomPath = 'packaging/release/opencv-5.1.0-local-sbom/JYPPX.OpenCV.CSharp.API.5.1.0.spdx.json'
$sbom = Get-Content (Join-Path $repo $sbomPath) -Raw | ConvertFrom-Json
function Assert-F51([bool]$Condition, [string]$Message) { if (-not $Condition) { throw $Message } }
function Get-F51Json($Value) { ConvertTo-Json -InputObject $Value -Depth 50 -Compress }
function Get-F51Blob([object[]]$Blobs, [string]$Name) {
    $rows = @($Blobs | Where-Object name -CEQ $Name)
    Assert-F51 ($rows.Count -eq 1) "Missing or duplicate raw Fedora evidence: $Name"
    $row = $rows[0]
    Assert-F51 ($row.bytes -eq [Text.Encoding]::UTF8.GetByteCount($row.text) -and $row.sha256 -ceq (Get-LocalLinux51TextHash $row.text)) 'Raw Fedora evidence bytes drifted.'
    [string]$row.text
}
function Get-F51Table([object[]]$Rows, [string]$Key = 'path') {
    $table = [Collections.Generic.Dictionary[string,object]]::new([StringComparer]::Ordinal)
    foreach ($row in $Rows) { Assert-F51 ($table.TryAdd([string]$row.$Key,$row)) "Duplicate Fedora inventory entry: $($row.$Key)" }
    return ,$table
}
function Test-F51File($File) {
    Assert-F51 (-not $File.path.Contains('..') -and -not [IO.Path]::IsPathRooted($File.path)) 'Artifact path escaped the repository.'
    $absolute = Join-Path $repo $File.path
    Assert-F51 ((Get-Item $absolute).Length -eq $File.bytes -and (Get-FileHash $absolute).Hash.ToLowerInvariant() -ceq $File.sha256) "Retained Fedora file differs: $($File.path)"
}
$kitSources = [ordered]@{
    'source/Program.cs' = 'tools/LinuxPackageModuleSmoke/Program.cs'
    'source/fedora-consumer.csproj.in' = 'tools/LinuxPackageModuleSmoke/fedora-consumer.csproj.in'
    'source/candidate-rid-graph.json' = 'packaging/runtime/runtime-fedora-5.1-local-consumer-rid-graph.json'
    'source/identity-opset13.onnx.base64' = 'tests/OpenCvSharp.Tests/Dnn/Fixtures/identity-opset13.onnx.base64'
    'scripts/Initialize-LocalLinux51ModuleConsumer.sh' = 'scripts/Initialize-LocalLinux51ModuleConsumer.sh'
    'scripts/Invoke-LocalFedora51Consumer.ps1' = 'scripts/Invoke-LocalFedora51Consumer.ps1'
}
$toolHashes = @{
    'toolchain/powershell.tar.gz' = 'dcfe6060fc86abcb859ce1f8f80843ce50bab0585396de56380ed9f25176ac6d'
    'toolchain/dotnet8.tar.gz' = '6f8b64ef9644b11bcab6748ebfec088d2ae4868df26204bb2354c3ada0f09cbe'
    'toolchain/dotnet10.tar.gz' = '137268c8ad939c064ff1ee2a6fdf0899d8725377114ea012fbd1ad5fa2550418'
    'toolchain/ubuntu-ca-certificates.crt' = '64e7c24b0536b525fb19b145768bd5e6e12ced0df3db837c34553ea789acd143'
}
function Assert-F51Evidence($r) {
    Assert-F51 (Test-Json -Json (Get-F51Json $r) -SchemaFile $schema -ErrorAction SilentlyContinue) 'Fedora evidence schema failed.'
    Assert-F51 ((Get-F51Json $r.sourceInputs) -ceq (Get-F51Json $sourceInputs)) 'Fedora source inputs drifted.'
    foreach ($group in @($r.producer,$r.consumer) + @($r.producer.profiles) + @($r.consumer.runs)) {
        $null = Get-F51Table $group.blobs 'name'
        foreach ($blob in $group.blobs) { $null = Get-F51Blob $group.blobs $blob.name }
    }
    & git -C $repo merge-base --is-ancestor $r.sourceBaseCommit HEAD 2>$null
    Assert-F51 ($LASTEXITCODE -eq 0) 'Fedora build revision is not an ancestor of HEAD.'
    $buildDrift = @(& git -C $repo diff --name-only $r.sourceBaseCommit -- 'src/OpenCvSharp.Native' 'scripts/Build-OpenCV.ps1' 'scripts/Apply-OpenCvSourcePatches.ps1' 'packaging/runtime/runtime-lifecycle-refresh-matrix.json')
    Assert-F51 ($LASTEXITCODE -eq 0 -and $buildDrift.Count -eq 0) 'Fedora committed native build inputs differ from the producer revision.'
    $kit = Get-F51Table $r.kitManifest
    $expectedKit = @($kitSources.Keys) + @($toolHashes.Keys) + @('packages.json','packages/JYPPX.OpenCV.CSharp.API.5.1.0.nupkg') + @($r.packages | ForEach-Object { "packages/$($_.packageId).$($_.version).nupkg" })
    Assert-F51 (@(Compare-Object $expectedKit @($kit.Keys) -CaseSensitive).Count -eq 0) 'Unexpected file in the Fedora consumer kit.'
    foreach ($key in $kitSources.Keys) {
        $inputFile = @($sourceInputs | Where-Object path -CEQ $kitSources[$key])[0]
        Assert-F51 ($kit[$key].bytes -eq $inputFile.bytes -and $kit[$key].sha256 -ceq $inputFile.sha256) 'Executed Fedora consumer source differs from the repository.'
    }
    foreach ($key in $toolHashes.Keys) { Assert-F51 ($kit[$key].sha256 -ceq $toolHashes[$key]) 'Unverified consumer toolchain archive.' }
    Assert-F51 ($r.managedSbom.path -ceq $sbomPath -and $r.managedSbom.sha256 -ceq (Get-FileHash (Join-Path $repo $sbomPath)).Hash.ToLowerInvariant()) 'Managed SBOM binding drifted.'
    $managed = $kit['packages/JYPPX.OpenCV.CSharp.API.5.1.0.nupkg']
    Assert-F51 ($managed.bytes -eq 11924069 -and $managed.sha256 -ceq '0081cd584da74aac7e51187985fbe4ceff3406f960525734633a3ba32768fd80') 'The Fedora consumer used another managed package.'
    $identities = @($r.producer.identity,$r.consumer.identity)
    Assert-F51 ($identities[0].id -cne $identities[1].id) 'Producer and consumer must be independent containers.'
    foreach ($identity in $identities) {
        Assert-F51 ($identity.id -cmatch '^[0-9a-f]{64}$' -and $identity.imageId -ceq 'sha256:43b29f65a41eb9c35e1cd5323e3bdf3b655c2357a9f4f1ff2f9c2798e5045d80' -and
            $identity.imageArchitecture -ceq 'amd64' -and $identity.imageOs -ceq 'linux' -and $identity.platform -ceq 'linux' -and -not $identity.oomKilled) 'Wrong Fedora image, architecture or OOM evidence.'
    }
    $producerMounts = Get-F51Table $identities[0].mounts 'destination'
    $consumerMounts = Get-F51Table $identities[1].mounts 'destination'
    Assert-F51 ($producerMounts.Count -eq 4 -and $producerMounts['/workspace'].type -ceq 'volume' -and $producerMounts['/workspace'].writable -and
        -not $producerMounts['/repo-input'].writable -and -not $producerMounts['/upstream-input'].writable -and -not $producerMounts['/toolchain'].writable -and
        $identities[0].memoryBytes -eq 6442450944 -and $identities[0].nanoCpus -eq 4000000000) 'Producer mount or resource isolation drifted.'
    Assert-F51 ($consumerMounts.Count -eq 2 -and $consumerMounts['/consumer'].type -ceq 'volume' -and $consumerMounts['/consumer'].writable -and
        $consumerMounts['/kit'].type -ceq 'bind' -and -not $consumerMounts['/kit'].writable -and
        $consumerMounts['/consumer'].source -cne $producerMounts['/workspace'].source -and
        $identities[1].memoryBytes -eq 2147483648 -and $identities[1].nanoCpus -eq 2000000000) 'Consumer has access to producer files or lacks a fresh volume.'
    $p = $r.producer.measurement
    Assert-F51 ((Get-F51Json $p) -ceq (Get-F51Json ((Get-F51Blob $r.producer.blobs 'producer.json') | ConvertFrom-Json))) 'Parsed producer data differs from its raw measurement.'
    Assert-F51 ($p.sourceBaseCommit -ceq $r.sourceBaseCommit -and $p.openCvVersion -ceq '5.1.0' -and $p.rid -ceq 'fedora.44-x64' -and
        $p.execution -ceq 'local-docker' -and $p.candidateOnly -and -not $p.publicationAllowed -and -not $p.hostedValidated -and -not $p.activeIdentityPromoted -and
        $p.upstreamCommit -ceq 'e4f78fbcf1a773fbed69e14500e8b734963eb585' -and $p.contribCommit -ceq 'b67a855188a3d683813731010f6d39ac614733a4' -and
        ($p.modifiedOpenCvFiles -join '|') -ceq 'modules/photo/src/ccm/ccm.cpp' -and $p.glibc -ceq 'glibc 2.43' -and $p.compilerTarget -ceq 'x86_64-redhat-linux' -and
        $p.compiler -match '16\.2\.1' -and $p.powerShell -ceq '7.4.17') 'The producer is not the official 5.1 Fedora native build.'
    $rpm = Get-F51Blob $r.producer.blobs 'installed-packages.txt'
    Assert-F51 ($rpm -match '(?m)^gcc-c\+\+\|16\.2\.1-3\.fc44\|x86_64$' -and $rpm -match '(?m)^eigen3-devel\|5\.0\.1-3\.fc44\|noarch$') 'The recorded producer compiler/Eigen RPMs are incomplete.'
    Assert-F51 ((Get-F51Blob $r.producer.blobs 'bootstrap.log').Contains('FEDORA_51_PRODUCER_READY') -and
        (Get-F51Blob $r.producer.blobs 'driver.log').Contains('FEDORA_51_PRODUCER_OK profiles=2 native_ctest=6')) 'Producer did not finish.'
    Assert-F51 (($p.profiles.profile -join '|') -ceq 'full|mini' -and ($r.producer.profiles.profile -join '|') -ceq 'full|mini' -and ($r.packages.profile -join '|') -ceq 'full|mini') 'Full/Mini producer/package matrix is incomplete.'
    foreach ($index in 0..1) {
        $profile = $p.profiles[$index]; $name = $profile.profile; $spec = @($matrix.profiles | Where-Object name -CEQ $name)[0]
        $blobs = $r.producer.profiles[$index].blobs; $package = $r.packages[$index]
        $plan = (Get-F51Blob $blobs 'build-plan.json') | ConvertFrom-Json
        Assert-F51 ($plan.PackageRid -ceq 'fedora.44-x64' -and $plan.OpenCvVersion -ceq '5.1.0' -and $plan.BuildList -ceq $spec.buildList -and
            $plan.BuildDir -ceq $profile.buildDirectory -and $plan.InstallRoot -ceq $profile.installDirectory -and -not $plan.GenericLinuxPreview) 'Actual build plan differs from the Fedora profile.'
        $expectedCount = if ($name -ceq 'full') { 49 } else { 9 }
        Assert-F51 ($profile.independentUpstreamBuild -and $profile.buildList -ceq $spec.buildList -and
            ($profile.modules -join '|') -ceq ($spec.modules -join '|') -and $profile.wrapperSourceCount -eq $expectedCount -and
            @($profile.wrapperSources | Select-Object -Unique).Count -eq $expectedCount -and
            $profile.buildDirectory -ceq "/workspace/producer-$name/artifacts/opencv-build/opencv-5.1.0-fedora.44-x64" -and
            $profile.installDirectory -ceq "/workspace/producer-$name/artifacts/opencv-install/opencv-5.1.0-fedora.44-x64") 'Independent upstream profile build evidence drifted.'
        $ctest = Get-F51Blob $blobs 'ctest.log'
        Assert-F51 ($ctest.Contains('100% tests passed, 0 tests failed out of 3') -and ([regex]::Matches($ctest,'(?m)^\s*[123]/3 Test #[123]: .*Passed')).Count -eq 3) 'Native CTest did not pass all three tests.'
        $cache = Get-F51Blob $blobs 'opencv-CMakeCache.txt'
        $built = [regex]::Match($cache,'(?m)^OPENCV_MODULES_BUILD:INTERNAL=(.+)$').Groups[1].Value.Trim().Split(';') | ForEach-Object { $_ -replace '^opencv_','' } | Sort-Object -CaseSensitive
        Assert-F51 (($built -join '|') -ceq (($spec.modules | Sort-Object -CaseSensitive) -join '|') -and $cache.Contains('CMAKE_BUILD_TYPE:STRING=Release')) 'The actual upstream CMake module set differs from the profile.'
        $wrapperCache = Get-F51Blob $blobs 'wrapper-CMakeCache.txt'
        Assert-F51 ($wrapperCache.Contains('CMAKE_BUILD_TYPE:STRING=Release') -and $wrapperCache.Contains("OPENCV_CSHARP_RUNTIME_PROFILE:STRING=$name") -and
            $wrapperCache.Contains("OPENCV_CSHARP_OPENCV_BUILD_LIST:STRING=$($spec.buildList)")) 'Wrapper configuration differs from the native profile.'
        $commands = (Get-F51Blob $blobs 'compile_commands.json') | ConvertFrom-Json
        $sources = [string[]]@($commands | Where-Object { $_.command.Contains('CMakeFiles/JYPPX.OpenCV.Native.dir/') } | ForEach-Object { $_.file.Replace('/workspace/repo/','') })
        [Array]::Sort($sources,[StringComparer]::Ordinal)
        Assert-F51 (($sources -join '|') -ceq ($profile.wrapperSources -join '|')) 'Wrapper sources differ from actual compiler commands.'
        $elf = (Get-F51Blob $blobs 'elf.json') | ConvertFrom-Json
        Assert-F51 ((Get-F51Json $elf) -ceq (Get-F51Json $profile.elf) -and $elf.profile -ceq $name -and $elf.rid -ceq 'fedora.44-x64' -and
            $elf.exactAbiManifestMatched -and -not $elf.genericLinuxBaselineClaimed -and $elf.glibc -ceq 'glibc 2.43') 'ELF audit was replaced or misclassified as generic Linux.'
        $manifestName = if ($name -ceq 'full') { 'native_abi_manifest.txt' } else { 'native_abi_mini_manifest.txt' }
        $exports = [string[]]@(Get-Content (Join-Path $repo "src/OpenCvSharp.Native/generated/$manifestName") | Where-Object { $_.StartsWith('jyppx_ocv_') -and $_.Contains('|') } | ForEach-Object { $_.Split('|')[0] })
        [Array]::Sort($exports,[StringComparer]::Ordinal)
        Assert-F51 ($elf.exportCount -eq $exports.Count -and $elf.exportNamesSha256 -ceq (Get-LocalLinux51TextHash (($exports -join "`n") + "`n"))) 'The ELF wrapper does not match the exact ABI manifest.'
        $native = Get-F51Table $profile.runtimeFiles
        $expectedNames = @('libJYPPX.OpenCV.Native.so') + @($spec.modules | ForEach-Object { "libopencv_$_.so"; "libopencv_$_.so.501"; "libopencv_$_.so.5.1.0" })
        Assert-F51 ($native.Count -eq $expectedNames.Count -and $elf.runtimeFileCount -eq $native.Count -and $elf.files.Count -eq ($spec.modules.Count + 1)) 'Native closure contains missing or extra files.'
        foreach ($module in $spec.modules) {
            foreach ($suffix in @('', '.501')) { Assert-F51 ($native["libopencv_$module.so$suffix"].sha256 -ceq $native["libopencv_$module.so.5.1.0"].sha256) 'OpenCV SONAME aliases differ.' }
        }
        foreach ($file in $elf.files) {
            Assert-F51 ($native[$file.name].sha256 -ceq $file.sha256 -and $native[$file.name].bytes -eq $file.bytes -and
                ($file.runtimePaths -join '|') -ceq '$ORIGIN' -and -not $file.lddOutput.Contains('not found')) 'ELF closure hash, loader path or dependencies drifted.'
            foreach ($family in @('GLIBC','GLIBCXX','CXXABI')) { Assert-F51 ($file.symbolFloors.$family -match '^\d+(\.\d+)*$') 'Actual Fedora symbol floors are missing.' }
            $expectedSoname = $file.name.Replace('.5.1.0','.501')
            Assert-F51 ($file.soname -ceq $expectedSoname) 'OpenCV 5.1 SONAME drifted.'
            foreach ($dependency in $file.resolvedDependencies) {
                if ($dependency.name.StartsWith('libopencv_')) {
                    Assert-F51 ($dependency.path -ceq "/workspace/output/$name/runtime/$($dependency.name)" -and $dependency.sha256 -ceq $native[$dependency.name].sha256) 'OpenCV dependency escaped the staged profile.'
                } else { Assert-F51 ($dependency.path -match '^/(usr/)?lib(64)?/' -and $dependency.sha256 -cmatch '^[0-9a-f]{64}$') 'An external dependency is not a system library.' }
            }
        }
        $id = 'JYPPX.OpenCV.runtime.fedora.44-x64' + $(if ($name -ceq 'mini') { '.mini' } else { '' })
        Assert-F51 ($package.packageId -ceq $id -and $package.file.path -ceq "$($r.artifactDirectory)/packages/$id.5.1.0-fedora44.b7f8180.nupkg") 'Fedora candidate package identity drifted.'
        $kitPackage = $kit["packages/$id.5.1.0-fedora44.b7f8180.nupkg"]
        Assert-F51 ($package.file.sha256 -ceq $kitPackage.sha256 -and $package.file.bytes -eq $kitPackage.bytes) 'Consumer kit does not contain the newly built package.'
        $entries = Get-F51Table $package.entries
        $nativeEntries = @($package.entries | Where-Object path -CLike 'runtimes/fedora.44-x64/native/*')
        Assert-F51 ($nativeEntries.Count -eq $expectedNames.Count) 'Package native asset closure drifted.'
        foreach ($fileName in $expectedNames) {
            $entry = $entries["runtimes/fedora.44-x64/native/$fileName"]
            Assert-F51 ($entry.sha256 -ceq $native[$fileName].sha256 -and $entry.bytes -eq $native[$fileName].bytes) 'Packaged native bytes differ from this Fedora build.'
        }
        $installedLicenses = @($r.artifactManifest | Where-Object path -CLike "$($r.artifactDirectory)/producer/$name/install/etc/licenses/*")
        Assert-F51 ($installedLicenses.Count -ge 10) 'Installed upstream license export is missing.'
        foreach ($license in $installedLicenses) {
            $entry = $entries["licenses/opencv-3rdparty/$([IO.Path]::GetFileName($license.path))"]
            Assert-F51 ($entry.sha256 -ceq $license.sha256 -and $entry.bytes -eq $license.bytes) 'Packaged installed license bytes differ from the producer export.'
        }
    }
    $c = $r.consumer
    $environment = (Get-F51Blob $c.blobs 'environment.json') | ConvertFrom-Json
    $packageManifestText = (ConvertTo-Json -InputObject @($environment.packages) -Depth 5) + "`n"
    Assert-F51 ($kit['packages.json'].bytes -eq [Text.Encoding]::UTF8.GetByteCount($packageManifestText) -and
        $kit['packages.json'].sha256 -ceq (Get-LocalLinux51TextHash $packageManifestText)) 'The executed package feed manifest differs from the read-only kit.'
    Assert-F51 ($c.rid -ceq 'fedora.44-x64' -and $environment.consumerRid -ceq $c.rid -and $environment.distro -ceq 'fedora' -and $environment.distroVersion -ceq '44' -and
        $environment.architecture -ceq 'x86_64' -and $environment.libc -ceq 'glibc 2.43' -and $environment.compilerAbsent -and $environment.producerPathsAbsent -and $environment.nativeLoaderOverridesAbsent) 'Consumer was not isolated Fedora 44 x64.'
    foreach ($manifest in @(@{ rows = $environment.sourceManifest; prefix = 'source' },@{ rows = $environment.scriptManifest; prefix = 'scripts' })) {
        foreach ($file in $manifest.rows) { Assert-F51 ($file.sha256 -ceq $kit["$($manifest.prefix)/$($file.name)"].sha256) 'Executed consumer file differs from the read-only kit.' }
    }
    Assert-F51 ($environment.sourceManifest.Count -eq 4 -and $environment.scriptManifest.Count -eq 2 -and $environment.packages.Count -eq 3) 'Consumer inputs are incomplete.'
    foreach ($package in $environment.packages) { Assert-F51 ($package.sha256 -ceq $kit["packages/$($package.name)"].sha256 -and $package.bytes -eq $kit["packages/$($package.name)"].bytes) 'Consumer restored another feed.' }
    $driver = Get-F51Blob $c.blobs 'driver.log'
    Assert-F51 ($driver.Contains('FEDORA_51_CONSUMER_OK rid=fedora.44-x64 runs=4') -and (Get-F51Blob $c.blobs 'bootstrap.log').Contains('CONSUMER_READY distro=fedora version=44')) 'Independent consumer did not finish.'
    $runs = (Get-F51Blob $c.blobs 'runs.json') | ConvertFrom-Json
    Assert-F51 ((($c.runs | ForEach-Object { "$($_.targetFramework)/$($_.profile)" }) -join '|') -ceq 'net8.0/full|net8.0/mini|net10.0/full|net10.0/mini') 'Consumer TFM/profile matrix is incomplete.'
    Assert-F51 ($runs.Count -eq 4) 'Consumer run record is incomplete.'
    foreach ($run in $c.runs) {
        $tfm = $run.targetFramework; $name = $run.profile
        $runtime = if ($tfm -ceq 'net8.0') { '8.0.34' } else { '10.0.12' }
        $sdk = if ($tfm -ceq 'net8.0') { '8.0.428' } else { '10.0.401' }
        $execution = @($runs | Where-Object { $_.targetFramework -ceq $tfm -and $_.profile -ceq $name })
        Assert-F51 ($execution.Count -eq 1 -and $execution[0].sdkVersion -ceq $sdk -and $execution[0].runtimeVersion -ceq $runtime -and $execution[0].freshPackageCache) 'SDK/runtime or fresh-cache execution drifted.'
        $raw = Get-F51Blob $run.blobs 'result.json'; $result = $raw | ConvertFrom-Json
        Assert-F51 ((Get-F51Blob $run.blobs 'execute.stdout.log').TrimEnd() -ceq $raw.TrimEnd() -and $result.status -ceq 'passed' -and $result.profile -ceq $name -and
            $result.openCvVersion -ceq '5.1.0' -and $result.packageVersion -ceq '5.1.0' -and $result.nativeAbi -eq 1 -and $result.targetFramework -ceq $tfm -and
            $result.runtimeVersion -ceq $runtime -and $result.architecture -ceq 'X64') 'The consumer did not execute the expected native runtime.'
        $package = @($r.packages | Where-Object profile -CEQ $name)[0]
        $native = @($package.entries | Where-Object path -CLike 'runtimes/fedora.44-x64/native/*')
        $actual = Get-F51Table $result.nativeFiles 'name'
        Assert-F51 ($actual.Count -eq $native.Count) 'Consumer output native closure is incomplete.'
        foreach ($file in $native) {
            $row = $actual[[IO.Path]::GetFileName($file.path)]
            Assert-F51 ($row.bytes -eq $file.bytes -and $row.sha256 -ceq $file.sha256) 'Executed native bytes differ from the new Fedora package.'
        }
        $assembly = @($sbom.files | Where-Object fileName -CEQ "./lib/$tfm/JYPPX.OpenCV.CSharp.API.dll")[0]
        Assert-F51 ($result.managedAssemblySha256 -ceq @($assembly.checksums | Where-Object algorithm -CEQ 'SHA256')[0].checksumValue) 'Managed assembly differs from the package SBOM.'
        Assert-F51 ($result.loadedOpenCvPaths.Count -eq $(if ($name -ceq 'full') { 18 } else { 7 }) -and @($result.loadedOpenCvPaths | Select-Object -Unique).Count -eq $result.loadedOpenCvPaths.Count) 'Loaded native module set drifted.'
        foreach ($loaded in $result.loadedOpenCvPaths) {
            Assert-F51 ([IO.Path]::GetDirectoryName($loaded).Replace('\','/') -ceq "/consumer/runs/$tfm-$name/bin/Release/$tfm/fedora.44-x64" -and $actual.ContainsKey([IO.Path]::GetFileName($loaded))) 'A module loaded from a producer or system OpenCV directory.'
        }
        Assert-F51 ($result.image.exactPixelRoundtrip -and $result.image.pngBytes -gt 0 -and $result.image.pngSha256 -cmatch '^[0-9a-f]{64}$') 'PNG exact pixel roundtrip did not pass.'
        $video = $result.video
        Assert-F51 ($video.codec -ceq 'MJPG' -and $video.writerBackend -ceq 'CV_MJPEG' -and $video.readerBackend -ceq 'CV_MJPEG' -and
            $video.frameCount -eq 3 -and $video.endOfFileVerified -and $video.frames.Count -eq 3 -and $video.pixelTolerance -eq 5 -and
            $run.video.bytes -eq $video.fileBytes -and $run.video.sha256 -ceq $video.fileSha256 -and
            $run.video.artifactPath -ceq "$($c.outputDirectory)/$tfm-$name/three-frames.avi") 'MJPG video, EOF or retained artifact evidence drifted.'
        for ($index = 0; $index -lt 3; $index++) {
            $frame = $video.frames[$index]
            $expectedSum = (200 + 30*$index)*32*24
            Assert-F51 ($frame.index -eq $index -and $frame.rows -eq 24 -and $frame.columns -eq 32 -and $frame.channels -eq 3 -and
                $frame.maxAbsoluteError -ge 0 -and $frame.maxAbsoluteError -le 5 -and
                [Math]::Abs($frame.checksum - $expectedSum) -le 5*32*24*3) 'Decoded frame pixels or sequence drifted.'
        }
        $dnn = $result.dnn
        Assert-F51 ($dnn.modelBytes -eq 147 -and $dnn.modelSha256 -ceq '326793cdb2fc2da739a715c3f3ff71d09779b389ad29e56bbfccc4313e900744') 'ONNX model provenance drifted.'
        if ($name -ceq 'full') {
            Assert-F51 ($dnn.status -ceq 'passed' -and $dnn.inferenceExecuted -and $dnn.engine -ceq 'Classic' -and $dnn.backend -ceq 'OpenCV' -and
                $dnn.target -ceq 'Cpu' -and $dnn.inputName -ceq 'input' -and $dnn.outputName -ceq 'output' -and $dnn.inferences.Count -eq 2) 'Full ONNX inference did not execute.'
            foreach ($index in 0..1) {
                $expected = if ($index -eq 0) { '1|2|3|4' } else { '-4|-3|-2|-1' }
                Assert-F51 (($dnn.inferences[$index].input -join '|') -ceq $expected -and ($dnn.inferences[$index].output -join '|') -ceq $expected -and $dnn.inferences[$index].maxAbsoluteError -eq 0) 'Full ONNX numerical output drifted.'
            }
        } else { Assert-F51 ($dnn.status -ceq 'omitted' -and -not $dnn.inferenceExecuted -and $dnn.reasonCode -ceq 'entrypoint-absent' -and $dnn.entrypoint -ceq 'jyppx_ocv_dnn_read_net_from_onnx_buffer') 'Mini importer omission was not actually exercised.' }
        Assert-F51 ($driver.Contains("FEDORA_51_CONSUMER_RUN_OK rid=fedora.44-x64 tfm=$tfm profile=$name video_frames=3 dnn=$($dnn.status)")) 'An actual execution marker is missing.'
        $project = (Get-Content (Join-Path $repo 'tools/LinuxPackageModuleSmoke/fedora-consumer.csproj.in') -Raw).Replace("`r`n","`n").Replace('__TFM__',$tfm).Replace('__RUNTIME_VERSION__',$runtime).Replace('__RUNTIME_PACKAGE__',$package.packageId)
        Assert-F51 ((Get-F51Blob $run.blobs 'Consumer.csproj') -ceq $project -and (Get-F51Blob $run.blobs 'candidate-rid-graph.json') -ceq [IO.File]::ReadAllText((Join-Path $repo 'packaging/runtime/runtime-fedora-5.1-local-consumer-rid-graph.json'))) 'Temporary consumer project or candidate RID graph drifted.'
        $assets = (Get-F51Blob $run.blobs 'project.assets.json') | ConvertFrom-Json
        $libraries = @($assets.libraries.PSObject.Properties.Name)
        Assert-F51 ($libraries.Count -eq 2 -and $libraries -ccontains 'JYPPX.OpenCV.CSharp.API/5.1.0' -and $libraries -ccontains "$($package.packageId)/$($package.version)" -and
            @($assets.targets.PSObject.Properties.Name) -ccontains "$tfm/fedora.44-x64" -and
            ($assets.project.restore.sources.PSObject.Properties.Name -join '|') -ceq '/consumer/feed' -and
            $assets.project.restore.packagesPath -ceq "/consumer/runs/$tfm-$name/nuget") 'NuGet did not restore the exact package pair from the isolated feed/cache.'
        foreach ($step in @('restore','build','execute')) { $null = Get-F51Blob $run.blobs "$step.stderr.log" }
        Assert-F51 ((Get-F51Blob $run.blobs 'restore.stdout.log').Contains('Restored') -and
            (Get-F51Blob $run.blobs 'build.stdout.log').Contains('0 Warning(s)') -and (Get-F51Blob $run.blobs 'build.stdout.log').Contains('0 Error(s)')) 'Consumer restore/build did not complete cleanly.'
    }
    $null = Get-F51Table $r.artifactManifest
}

function Set-F51Blob($Blob, [string]$Text) {
    $Blob.text = $Text; $Blob.bytes = [Text.Encoding]::UTF8.GetByteCount($Text); $Blob.sha256 = Get-LocalLinux51TextHash $Text
}
function Update-F51Result($Record, [int]$Index, [scriptblock]$Change) {
    $run = $Record.consumer.runs[$Index]; $blob = @($run.blobs | Where-Object name -CEQ 'result.json')[0]
    $result = $blob.text | ConvertFrom-Json; & $Change $result
    $text = (Get-F51Json $result) + "`n"; Set-F51Blob $blob $text
    Set-F51Blob @($run.blobs | Where-Object name -CEQ 'execute.stdout.log')[0] $text
}
function Update-F51Producer($Record, [scriptblock]$Change) {
    & $Change $Record.producer.measurement
    Set-F51Blob @($Record.producer.blobs | Where-Object name -CEQ 'producer.json')[0] ((Get-F51Json $Record.producer.measurement) + "`n")
}
function Update-F51Elf($Record, [scriptblock]$Change) {
    & $Change $Record.producer.measurement.profiles[0].elf
    Set-F51Blob @($Record.producer.profiles[0].blobs | Where-Object name -CEQ 'elf.json')[0] ((Get-F51Json $Record.producer.measurement.profiles[0].elf) + "`n")
    Update-F51Producer $Record { param($p) }
}
$record = $json | ConvertFrom-Json
Assert-F51Evidence $record
$fixtures = @(
    { param($r) $r.publicationAllowed = $true }, { param($r) $r.hostedValidated = $true },
    { param($r) $r.activeIdentityPromoted = $true }, { param($r) $r.coverage.genericLinuxBaselineClaimed = $true },
    { param($r) $r.coverage.independentUpstreamBuilds = 1 }, { param($r) $r.coverage.releaseApproved = $true },
    { param($r) $r.sourceInputs[0].sha256 = '0'*64 }, { param($r) $r.kitManifest[0].sha256 = '0'*64 },
    { param($r) $r.consumer.identity.id = $r.producer.identity.id },
    { param($r) @($r.consumer.identity.mounts | Where-Object destination -CEQ '/kit')[0].writable = $true },
    { param($r) Update-F51Producer $r { param($p) $p.profiles[1].independentUpstreamBuild = $false } },
    { param($r) Update-F51Producer $r { param($p) $p.upstreamCommit = '0'*40 } },
    { param($r) Update-F51Elf $r { param($e) $e.exportNamesSha256 = '0'*64 } },
    { param($r) Update-F51Elf $r { param($e) $e.files[0].runtimePaths = @('/workspace/install/lib64') } },
    { param($r) Update-F51Elf $r { param($e) $e.files[0].soname = 'libopencv_core.so.500' } },
    { param($r) $r.producer.profiles[0].blobs[0].text = 'changed' },
    { param($r) $r.packages[0].entries[0].sha256 = '0'*64; $r.packages[0].file.sha256 = '0'*64 },
    { param($r) $r.consumer.runs = @($r.consumer.runs[0]) },
    { param($r) Update-F51Result $r 0 { param($v) $v.runtimeVersion = '10.0.12' } },
    { param($r) Update-F51Result $r 0 { param($v) $v.nativeFiles[0].sha256 = '0'*64 } },
    { param($r) Update-F51Result $r 0 { param($v) $v.loadedOpenCvPaths[0] = '/workspace/producer/libopencv_core.so.501' } },
    { param($r) Update-F51Result $r 0 { param($v) $v.managedAssemblySha256 = '0'*64 } },
    { param($r) Update-F51Result $r 0 { param($v) $v.image.exactPixelRoundtrip = $false } },
    { param($r) Update-F51Result $r 0 { param($v) $v.video.endOfFileVerified = $false } },
    { param($r) Update-F51Result $r 0 { param($v) $v.video.frames[1].checksum = 0 } },
    { param($r) Update-F51Result $r 0 { param($v) $v.dnn.inferences[1].output[0] = 99 } },
    { param($r) Update-F51Result $r 1 { param($v) $v.dnn.reasonCode = 'assumed' } },
    { param($r) $b = @($r.consumer.runs[0].blobs | Where-Object name -CEQ 'project.assets.json')[0]; $a = $b.text | ConvertFrom-Json; $a.project.restore.packagesPath = '/shared/cache'; Set-F51Blob $b (Get-F51Json $a) },
    { param($r) $b = @($r.consumer.runs[0].blobs | Where-Object name -CEQ 'build.stdout.log')[0]; Set-F51Blob $b '' }
)
$fixtureIndex = 0
foreach ($fixture in $fixtures) {
    $fixtureIndex++
    $mutant = $json | ConvertFrom-Json; & $fixture $mutant; $rejected = $false
    try { Assert-F51Evidence $mutant } catch { $rejected = $true }
    Assert-F51 $rejected "Fedora evidence guard accepted negative fixture $fixtureIndex."
}
if ($RequireArtifacts) {
    foreach ($file in $record.artifactManifest) { Test-F51File $file }
    foreach ($package in $record.packages) {
        Test-F51File $package.file
        Assert-F51 ((Get-F51Json @(Get-LocalLinux51PackageManifest (Join-Path $repo $package.file.path))) -ceq (Get-F51Json $package.entries)) 'Retained NuGet archive entries differ from the evidence.'
    }
}
Write-Host "FEDORA_51_EVIDENCE_OK upstream_builds=2 native_ctest=6 consumers=4 negative_fixtures=$($fixtures.Count) hosted=false publication_allowed=false"

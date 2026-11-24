[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [string]$EvidencePath = 'packaging/runtime/opencv-5.1.0-local-linux-module-consumer-evidence.json',
    [switch]$RequireArtifacts
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'LocalLinux51Module.Common.ps1')
$repo = (Resolve-Path $RepositoryRoot).Path
$path = if ([IO.Path]::IsPathRooted($EvidencePath)) { $EvidencePath } else { Join-Path $repo $EvidencePath }
$json = [IO.File]::ReadAllText($path)
$schema = Join-Path $repo 'packaging/runtime/opencv-5.1.0-local-linux-module-consumer-evidence.schema.json'
$record = $json | ConvertFrom-Json
$specs = @(Get-LocalLinux51ModuleSpecs)
$sourceInputs = @(Get-LocalLinux51ModuleSourceInputs $repo)
$baselinePath = 'packaging/runtime/opencv-5.1.0-local-linux-preview-evidence.json'
$distroPath = 'packaging/runtime/opencv-5.1.0-local-linux-distro-consumer-evidence.json'
$sbomPath = 'packaging/release/opencv-5.1.0-local-sbom/JYPPX.OpenCV.CSharp.API.5.1.0.spdx.json'
$baseline = Get-Content (Join-Path $repo $baselinePath) -Raw | ConvertFrom-Json
$distro = Get-Content (Join-Path $repo $distroPath) -Raw | ConvertFrom-Json
$sbom = Get-Content (Join-Path $repo $sbomPath) -Raw | ConvertFrom-Json
$kitSources = [ordered]@{
    'source/Program.cs' = 'tools/LinuxPackageModuleSmoke/Program.cs'
    'source/consumer.csproj.in' = 'tools/LinuxPackageModuleSmoke/consumer.csproj.in'
    'source/identity-opset13.onnx.base64' = 'tests/OpenCvSharp.Tests/Dnn/Fixtures/identity-opset13.onnx.base64'
    'scripts/Initialize-LocalLinux51ModuleConsumer.sh' = 'scripts/Initialize-LocalLinux51ModuleConsumer.sh'
    'scripts/Invoke-LocalLinux51ModuleConsumer.ps1' = 'scripts/Invoke-LocalLinux51ModuleConsumer.ps1'
}
$toolFiles = @(
    @('toolchain/dotnet8.tar.gz', 217266728, '6f8b64ef9644b11bcab6748ebfec088d2ae4868df26204bb2354c3ada0f09cbe'),
    @('toolchain/dotnet10.tar.gz', 240059572, '137268c8ad939c064ff1ee2a6fdf0899d8725377114ea012fbd1ad5fa2550418'),
    @('toolchain/powershell.tar.gz', 72497272, 'dcfe6060fc86abcb859ce1f8f80843ce50bab0585396de56380ed9f25176ac6d'),
    @('toolchain/ubuntu-ca-certificates.crt', 180744, '64e7c24b0536b525fb19b145768bd5e6e12ced0df3db837c34553ea789acd143')
)
$packages = @([pscustomobject]@{ path = 'packages/JYPPX.OpenCV.CSharp.API.5.1.0.nupkg'; bytes = $baseline.managedPackage.bytes; sha256 = $baseline.managedPackage.sha256 }) +
    @($baseline.profiles | ForEach-Object { [pscustomobject]@{ path = "packages/$($_.packageId).5.1.0.nupkg"; bytes = $_.packageBytes; sha256 = $_.packageSha256 } })
$toolchainSpecs = @(
    @('net8.0', '8.0.428', '8.0.34', 'toolchain/dotnet8.tar.gz', '33212bb99a2bf003973d41106c7e8fe94208a12958ff601d4a476be379403c85a7dc9183a6ec64dfb44ded7102965be09f35add1758e68e9251bae7891d7b7bf'),
    @('net10.0', '10.0.401', '10.0.12', 'toolchain/dotnet10.tar.gz', '51c8b999af9e8dd9998c9edc5944e19a90788862068acd38694e098889054ce8c23d4f0c5cccfa16bf187d044562359e5ee69a9f8ad0bbe913ba90311fbce25b')
)

function Get-ModuleBlobText {
    param([object[]]$Blobs, [string]$Name)
    $matches = @($Blobs | Where-Object name -CEQ $Name)
    if ($matches.Count -ne 1) { throw "Missing or duplicate module evidence blob: $Name" }
    $blob = $matches[0]
    if ((Get-LocalLinux51TextHash $blob.text) -cne $blob.sha256 -or [Text.Encoding]::UTF8.GetByteCount($blob.text) -ne $blob.bytes) { throw 'Raw module evidence bytes drifted.' }
    $blob.text
}

function Assert-LocalLinux51ModuleEvidence {
    param([object]$Record)
    if (-not (Test-Json -Json ($Record | ConvertTo-Json -Depth 24 -Compress) -SchemaFile $schema -ErrorAction SilentlyContinue)) { throw 'Module consumer evidence schema failed.' }
    & git -C $repo merge-base --is-ancestor $Record.sourceBaseCommit HEAD 2>$null
    if ($LASTEXITCODE -ne 0) { throw 'Module source base is not an ancestor of HEAD.' }
    if (($Record.sourceInputs | ConvertTo-Json -Compress) -cne ($sourceInputs | ConvertTo-Json -Compress)) { throw 'Module source inputs drifted.' }
    foreach ($reference in @(@('baselineEvidence', $baselinePath), @('originalDistroEvidence', $distroPath), @('managedSbomEvidence', $sbomPath))) {
        $value = $Record.($reference[0])
        if ($value.path -cne $reference[1] -or $value.sha256 -cne (Get-FileHash (Join-Path $repo $reference[1]) -Algorithm SHA256).Hash.ToLowerInvariant()) { throw 'Original evidence or managed SBOM binding drifted.' }
    }
    if (@($sbom.packages[0].checksums | Where-Object { $_.algorithm -ceq 'SHA256' -and $_.checksumValue -ceq $baseline.managedPackage.sha256 }).Count -ne 1) { throw 'Managed SBOM does not describe the consumer package.' }
    $expectedKitPaths = @($kitSources.Keys) + @($toolFiles | ForEach-Object { $_[0] }) + @($packages.path)
    if (@($Record.kitManifest.path | Select-Object -Unique).Count -ne 12 -or @(Compare-Object $expectedKitPaths @($Record.kitManifest.path) -CaseSensitive).Count) { throw 'Unexpected module consumer kit content.' }
    foreach ($key in $kitSources.Keys) {
        $file = @($Record.kitManifest | Where-Object path -CEQ $key)[0]
        $sourceFile = @($sourceInputs | Where-Object path -CEQ $kitSources[$key])[0]
        if ($file.bytes -ne $sourceFile.bytes -or $file.sha256 -cne $sourceFile.sha256) { throw 'Executed source kit differs from measured inputs.' }
    }
    foreach ($spec in $toolFiles) {
        $file = @($Record.kitManifest | Where-Object path -CEQ $spec[0])[0]
        if ($file.bytes -ne $spec[1] -or $file.sha256 -cne $spec[2]) { throw 'Unverified toolchain archive or CA bundle.' }
    }
    foreach ($package in $packages) {
        $file = @($Record.kitManifest | Where-Object path -CEQ $package.path)[0]
        if ($file.bytes -ne $package.bytes -or $file.sha256 -cne $package.sha256) { throw 'Consumer kit package bytes differ from the original producer.' }
    }
    for ($index = 0; $index -lt 2; $index++) {
        $tool = $Record.toolchains[$index]; $spec = $toolchainSpecs[$index]
        if ($tool.targetFramework -cne $spec[0] -or $tool.sdkVersion -cne $spec[1] -or $tool.runtimeVersion -cne $spec[2] -or
            $tool.archive -cne $spec[3] -or $tool.archiveSha512 -cne $spec[4] -or
            $tool.officialUrl -cne "https://builds.dotnet.microsoft.com/dotnet/Sdk/$($spec[1])/dotnet-sdk-$($spec[1])-linux-x64.tar.gz") { throw 'SDK/runtime selection or official toolchain binding drifted.' }
    }
    if (($Record.consumers.rid -join '|') -cne ($specs.rid -join '|')) { throw 'Module consumer distro matrix is incomplete.' }
    $oldIds = @($baseline.producerIdentity.id, $baseline.consumerIdentity.id) + @($distro.consumers.identity.id)
    if (@($Record.consumers.identity.id | Select-Object -Unique).Count -ne 3 -or @($Record.consumers.identity.id | Where-Object { $_ -cin $oldIds }).Count) { throw 'Functional consumers must have independent container identities.' }
    $volumes = @($Record.consumers.identity.mounts | Where-Object destination -CEQ '/consumer' | ForEach-Object source)
    $oldMounts = @($baseline.producerIdentity.mounts.source) + @($baseline.consumerIdentity.mounts.source) + @($distro.consumers.identity.mounts.source)
    if (@($volumes | Select-Object -Unique).Count -ne 3 -or @($volumes | Where-Object { $_ -cin $oldMounts }).Count) { throw 'Functional consumers share a writable volume or producer input.' }
    $kitMounts = @($Record.consumers.identity.mounts | Where-Object destination -CEQ '/kit' | ForEach-Object source | Select-Object -Unique)
    if ($kitMounts.Count -ne 1) { throw 'Expected the same verified read-only kit.' }
    foreach ($consumer in $Record.consumers) {
        $spec = @($specs | Where-Object rid -CEQ $consumer.rid)[0]
        $identity = $consumer.identity
        if ($identity.image -cne $spec.image -or
            @($identity.mounts | Where-Object { $_.destination -ceq '/kit' -and $_.type -ceq 'bind' -and -not $_.writable }).Count -ne 1 -or
            @($identity.mounts | Where-Object { $_.destination -ceq '/consumer' -and $_.type -ceq 'volume' -and $_.writable }).Count -ne 1) { throw 'Consumer image or mount isolation drifted.' }
        if (-not $consumer.outputDirectory.StartsWith('artifacts/opencv-5.1.0-local-linux-module-consumers/', [StringComparison]::Ordinal) -or $consumer.outputDirectory.Contains('..')) { throw 'Unsafe or unrelated retained artifact path.' }
        $bootstrap = Get-ModuleBlobText $consumer.blobs 'bootstrap.log'
        $driver = Get-ModuleBlobText $consumer.blobs 'driver.log'
        $environment = (Get-ModuleBlobText $consumer.blobs 'environment.json') | ConvertFrom-Json
        $runs = @((Get-ModuleBlobText $consumer.blobs 'runs.json') | ConvertFrom-Json)
        foreach ($archive in @('powershell', 'dotnet8', 'dotnet10')) { if (-not $bootstrap.Contains("/kit/toolchain/$archive.tar.gz: OK")) { throw 'Toolchain archive verification was not executed.' } }
        if (-not $bootstrap.Contains("LOCAL_LINUX_51_MODULE_CONSUMER_READY distro=$($spec.distro) version=$($spec.version)") -or
            -not $driver.Contains("LOCAL_LINUX_51_MODULE_CONSUMER_OK rid=$($spec.rid) runs=4")) { throw 'Missing actual consumer bootstrap or completion output.' }
        if ($environment.consumerRid -cne $spec.rid -or $environment.distro -cne $spec.distro -or $environment.distroVersion -cne $spec.version -or
            $environment.architecture -cne 'x86_64' -or $environment.libc -cne $spec.libc -or $environment.powerShellVersion -cne '7.4.17' -or
            -not $environment.compilerAbsent -or -not $environment.producerPathsAbsent -or -not $environment.nativeLoaderOverridesAbsent) { throw 'Independent consumer environment drifted.' }
        foreach ($manifestSpec in @(@('sourceManifest', 'source/', 3), @('scriptManifest', 'scripts/', 2))) {
            $manifest = @($environment.($manifestSpec[0]))
            if ($manifest.Count -ne $manifestSpec[2] -or @($manifest.name | Select-Object -Unique).Count -ne $manifestSpec[2]) { throw 'Executed consumer source/script manifest is incomplete.' }
            foreach ($file in $manifest) {
                $expected = @($Record.kitManifest | Where-Object path -CEQ "$($manifestSpec[1])$($file.name)")
                if ($expected.Count -ne 1 -or $file.sha256 -cne $expected[0].sha256 -or $file.bytes -ne $expected[0].bytes) { throw 'Executed consumer files differ from the kit.' }
            }
        }
        if ($environment.packages.Count -ne 3) { throw 'Consumer feed is incomplete.' }
        foreach ($package in $packages) {
            $feedEntry = @($environment.packages | Where-Object name -CEQ ([IO.Path]::GetFileName($package.path)))
            if ($feedEntry.Count -ne 1 -or $feedEntry[0].sha256 -cne $package.sha256) { throw 'Actual consumer feed package bytes drifted.' }
        }
        $pairs = @($consumer.runs | ForEach-Object { "$($_.targetFramework)/$($_.profile)" })
        if (($pairs -join '|') -cne 'net8.0/full|net8.0/mini|net10.0/full|net10.0/mini' -or $runs.Count -ne 4) { throw 'Functional TFM/profile matrix is incomplete or duplicated.' }
        foreach ($run in $consumer.runs) {
            $tfm = $run.targetFramework; $profile = $run.profile
            $tool = @($Record.toolchains | Where-Object targetFramework -CEQ $tfm)[0]
            $execution = @($runs | Where-Object { $_.targetFramework -ceq $tfm -and $_.profile -ceq $profile })
            if ($execution.Count -ne 1 -or $execution[0].sdkVersion -cne $tool.sdkVersion -or $execution[0].runtimeVersion -cne $tool.runtimeVersion -or
                -not $execution[0].freshPackageCache -or $execution[0].resultDirectory -cne "$tfm-$profile") { throw 'Selected toolchain or fresh-cache execution evidence drifted.' }
            $resultJson = Get-ModuleBlobText $run.blobs 'result.json'
            $result = $resultJson | ConvertFrom-Json
            $stdout = Get-ModuleBlobText $run.blobs 'execute.stdout.log'
            $null = Get-ModuleBlobText $run.blobs 'execute.stderr.log'
            if ($stdout.TrimEnd() -cne $resultJson.TrimEnd() -or $result.status -cne 'passed' -or $result.profile -cne $profile -or
                $result.openCvVersion -cne '5.1.0' -or $result.packageVersion -cne '5.1.0' -or $result.nativeAbi -ne 1 -or
                $result.targetFramework -cne $tfm -or $result.runtimeVersion -cne $tool.runtimeVersion -or $result.frameworkDescription -cne ".NET $($tool.runtimeVersion)" -or
                $result.architecture -cne 'X64') { throw 'Native execution was skipped or used the wrong runtime/profile.' }
            $managedFile = @($sbom.files | Where-Object fileName -CEQ "./lib/$tfm/JYPPX.OpenCV.CSharp.API.dll")[0]
            $assemblyHash = @($managedFile.checksums | Where-Object algorithm -CEQ 'SHA256')[0].checksumValue
            if ($result.managedAssemblySha256 -cne $assemblyHash) { throw 'Executed managed assembly differs from the package SBOM.' }
            $nativeProfile = @($baseline.profiles | Where-Object name -CEQ $profile)[0]
            $nativeEntries = @($nativeProfile.entries | Where-Object path -CLike 'runtimes/linux-x64/native/*.so*')
            if ($result.nativeFiles.Count -ne $nativeEntries.Count -or @($result.nativeFiles.name | Select-Object -Unique).Count -ne $nativeEntries.Count) { throw 'Executed native payload is incomplete or mixed.' }
            foreach ($entry in $nativeEntries) {
                $actual = @($result.nativeFiles | Where-Object name -CEQ ([IO.Path]::GetFileName($entry.path)))
                if ($actual.Count -ne 1 -or $actual[0].bytes -ne $entry.bytes -or $actual[0].sha256 -cne $entry.sha256) { throw 'Executed native file bytes differ from the original package.' }
            }
            $baseDirectory = "/consumer/runs/$tfm-$profile/bin/Release/$tfm/linux-x64/"
            if ($result.loadedOpenCvPaths.Count -ne $(if ($profile -ceq 'full') { 18 } else { 7 }) -or
                @($result.loadedOpenCvPaths | Select-Object -Unique).Count -ne $result.loadedOpenCvPaths.Count) { throw 'Loaded native module closure is incomplete.' }
            foreach ($loaded in $result.loadedOpenCvPaths) {
                if (-not $loaded.StartsWith($baseDirectory, [StringComparison]::Ordinal) -or $loaded.Contains('..') -or
                    @($result.nativeFiles | Where-Object name -CEQ $loaded.Substring($baseDirectory.Length)).Count -ne 1) { throw 'OpenCV loaded outside the verified consumer output.' }
            }
            if (-not $result.image.exactPixelRoundtrip -or $result.image.pngBytes -ne 146 -or $result.image.pngSha256 -cne '85c0160486a1c381e8c7adc062e03491aef8133428ad99be492be0302fe7ada7') { throw 'PNG pixel roundtrip did not match the fixture.' }
            $video = $result.video
            if ($video.codec -cne 'MJPG' -or $video.container -cne 'AVI' -or $video.writerBackend -cne 'CV_MJPEG' -or $video.readerBackend -cne 'CV_MJPEG' -or
                $video.frameCount -ne 3 -or -not $video.endOfFileVerified -or $video.pixelTolerance -ne 5 -or $video.frames.Count -ne 3 -or
                $video.fileBytes -ne 6288 -or $video.fileSha256 -cne '35f06c4c8ddfd2f0ac4265353ba17449a576a28fb35c9bc342108950e0963449' -or
                $run.video.bytes -ne $video.fileBytes -or $run.video.sha256 -cne $video.fileSha256 -or
                $run.video.artifactPath -cne "$($consumer.outputDirectory)/$tfm-$profile/three-frames.avi") { throw 'Actual AVI write/read/EOF evidence drifted.' }
            for ($index = 0; $index -lt 3; $index++) {
                $frame = $video.frames[$index]
                if ($frame.index -ne $index -or $frame.rows -ne 24 -or $frame.columns -ne 32 -or $frame.channels -ne 3 -or
                    $frame.maxAbsoluteError -ne 2 -or $frame.checksum -ne (152880 + 23040 * $index)) { throw 'Decoded video frame pixels or sequence drifted.' }
            }
            $dnn = $result.dnn
            if ($dnn.modelBytes -ne 147 -or $dnn.modelSha256 -cne '326793cdb2fc2da739a715c3f3ff71d09779b389ad29e56bbfccc4313e900744') { throw 'ONNX fixture provenance drifted.' }
            if ($profile -ceq 'full') {
                if ($dnn.status -cne 'passed' -or -not $dnn.inferenceExecuted -or $dnn.engine -cne 'Classic' -or $dnn.backend -cne 'OpenCV' -or $dnn.target -cne 'Cpu' -or
                    $dnn.inputName -cne 'input' -or $dnn.outputName -cne 'output' -or $dnn.inferences.Count -ne 2) { throw 'Full ONNX inference was not executed.' }
                for ($index = 0; $index -lt 2; $index++) {
                    $expectedValues = if ($index -eq 0) { '1|2|3|4' } else { '-4|-3|-2|-1' }
                    $inference = $dnn.inferences[$index]
                    if (($inference.input -join '|') -cne $expectedValues -or ($inference.output -join '|') -cne $expectedValues -or $inference.maxAbsoluteError -ne 0) { throw 'Full ONNX output values drifted.' }
                }
            }
            elseif ($dnn.status -cne 'omitted' -or $dnn.inferenceExecuted -or $dnn.reasonCode -cne 'entrypoint-absent' -or
                $dnn.entrypoint -cne 'jyppx_ocv_dnn_read_net_from_onnx_buffer' -or -not $dnn.reason.Contains('jyppx_ocv_dnn_read_net_from_onnx_buffer')) { throw 'Mini DNN omission was not actually exercised.' }
            $marker = "LOCAL_LINUX_51_MODULE_RUN_OK rid=$($consumer.rid) tfm=$tfm profile=$profile video_frames=3 dnn=$($dnn.status)"
            if (-not $driver.Contains($marker)) { throw 'Actual run completion marker is missing.' }
            $runtimeId = if ($profile -ceq 'full') { 'JYPPX.OpenCV.runtime.linux-x64.preview' } else { 'JYPPX.OpenCV.runtime.linux-x64.preview.mini' }
            $project = Get-ModuleBlobText $run.blobs 'Consumer.csproj'
            $expectedProject = (Get-Content (Join-Path $repo 'tools/LinuxPackageModuleSmoke/consumer.csproj.in') -Raw).Replace('__TFM__', $tfm).Replace('__RUNTIME_VERSION__', $tool.runtimeVersion).Replace('__RUNTIME_PACKAGE__', $runtimeId)
            if ($project -cne $expectedProject) { throw 'Consumer project uses an unexpected package, TFM or runtime.' }
            $assets = (Get-ModuleBlobText $run.blobs 'project.assets.json') | ConvertFrom-Json
            $libraryNames = @($assets.libraries.PSObject.Properties.Name)
            if ($libraryNames.Count -ne 2 -or $libraryNames -cnotcontains 'JYPPX.OpenCV.CSharp.API/5.1.0' -or $libraryNames -cnotcontains "$runtimeId/5.1.0" -or
                @($assets.targets.PSObject.Properties.Name | Where-Object { $_ -ceq "$tfm/linux-x64" }).Count -ne 1) { throw 'Actual NuGet resolution did not select the exact isolated package pair.' }
            if (($assets.project.restore.sources.PSObject.Properties.Name -join '|') -cne '/consumer/feed' -or
                $assets.project.restore.packagesPath -cne "/consumer/runs/$tfm-$profile/nuget" -or
                ($assets.packageFolders.PSObject.Properties.Name -join '|') -cne "/consumer/runs/$tfm-$profile/nuget") { throw 'Restore used an external feed or a shared package cache.' }
            foreach ($step in @('restore', 'build')) {
                $log = Get-ModuleBlobText $run.blobs "$step.stdout.log"
                $null = Get-ModuleBlobText $run.blobs "$step.stderr.log"
                if ([string]::IsNullOrWhiteSpace($log) -or $log -match '(?m)^.*\berror (NU|CS|MSB|NETSDK)[0-9]+:') { throw 'Restore/build did not complete successfully.' }
            }
        }
    }
}

function Set-ModuleBlobText {
    param([object]$Blob, [string]$Text)
    $Blob.text = $Text; $Blob.bytes = [Text.Encoding]::UTF8.GetByteCount($Text); $Blob.sha256 = Get-LocalLinux51TextHash $Text
}
function Update-ModuleResult {
    param([object]$Record, [int]$RunIndex, [scriptblock]$Mutation)
    $run = $Record.consumers[0].runs[$RunIndex]
    $blob = @($run.blobs | Where-Object name -CEQ 'result.json')[0]
    $result = $blob.text | ConvertFrom-Json
    & $Mutation $result
    $text = ($result | ConvertTo-Json -Depth 12 -Compress) + "`n"
    Set-ModuleBlobText $blob $text
    Set-ModuleBlobText @($run.blobs | Where-Object name -CEQ 'execute.stdout.log')[0] $text
}
$fixtures = @(
    { param($r) $r.hosted = $true },
    { param($r) $r.publicationAllowed = $true },
    { param($r) $r.genericStablePromoted = $true },
    { param($r) $r.coverage.arm64Validated = $true },
    { param($r) $r.coverage.cameraOrNetworkValidated = $true },
    { param($r) $r.coverage.arbitraryDnnModelsValidated = $true },
    { param($r) $r.coverage.nativeRuns = 8 },
    { param($r) $r.sourceInputs[0].sha256 = '0' * 64 },
    { param($r) $r.kitManifest += $r.kitManifest[0] },
    { param($r) $r.kitManifest[0].sha256 = '0' * 64 },
    { param($r) $r.toolchains[1].runtimeVersion = '10.0.15' },
    { param($r) $r.toolchains[0].archiveSha512 = '0' * 128 },
    { param($r) $r.originalDistroEvidence.sha256 = '0' * 64 },
    { param($r) $r.managedSbomEvidence.sha256 = '0' * 64 },
    { param($r) $r.consumers = @($r.consumers[0], $r.consumers[1]) },
    { param($r) $r.consumers[0].identity.id = $r.consumers[1].identity.id },
    { param($r) @($r.consumers[0].identity.mounts | Where-Object destination -CEQ '/kit')[0].writable = $true },
    { param($r) @($r.consumers[0].identity.mounts | Where-Object destination -CEQ '/consumer')[0].source = @($r.consumers[1].identity.mounts | Where-Object destination -CEQ '/consumer')[0].source },
    { param($r) $r.consumers[0].runs[1].profile = 'full' },
    { param($r) $r.consumers[0].runs[0].blobs[0].text = 'changed' },
    { param($r) Update-ModuleResult $r 0 { param($v) $v.status = 'skipped' } },
    { param($r) Update-ModuleResult $r 0 { param($v) $v.dnn.inferenceExecuted = $false } },
    { param($r) Update-ModuleResult $r 0 { param($v) $v.dnn.inferences[1].output[0] = 99 } },
    { param($r) Update-ModuleResult $r 0 { param($v) $v.video.endOfFileVerified = $false } },
    { param($r) Update-ModuleResult $r 0 { param($v) $v.video.frames[1].checksum = 152880 } },
    { param($r) Update-ModuleResult $r 0 { param($v) $v.video.frames[0].maxAbsoluteError = 8 } },
    { param($r) Update-ModuleResult $r 0 { param($v) $v.nativeFiles[0].sha256 = '0' * 64 } },
    { param($r) Update-ModuleResult $r 0 { param($v) $v.loadedOpenCvPaths[0] = '/opt/producer/libJYPPX.OpenCV.Native.so' } },
    { param($r) Update-ModuleResult $r 0 { param($v) $v.managedAssemblySha256 = '0' * 64 } },
    { param($r) Update-ModuleResult $r 1 { param($v) $v.dnn.reasonCode = 'assumed' } },
    { param($r) $blob = @($r.consumers[0].runs[0].blobs | Where-Object name -CEQ 'project.assets.json')[0]; $a = $blob.text | ConvertFrom-Json; $a.project.restore.packagesPath = '/shared/nuget'; Set-ModuleBlobText $blob ($a | ConvertTo-Json -Depth 20) },
    { param($r) $blob = @($r.consumers[0].runs[0].blobs | Where-Object name -CEQ 'build.stdout.log')[0]; Set-ModuleBlobText $blob '' }
)
foreach ($fixture in $fixtures) {
    $mutant = $json | ConvertFrom-Json
    & $fixture $mutant
    $rejected = $false
    try { Assert-LocalLinux51ModuleEvidence $mutant } catch { $rejected = $true }
    if (-not $rejected) { throw 'Module consumer evidence guard accepted a negative fixture.' }
}
Write-Host "LOCAL_LINUX_51_MODULE_EVIDENCE_OK distros=3 native_runs=12 full_onnx_runs=6 mini_omission_runs=6 avi_runs=12 negative_fixtures=$($fixtures.Count) hosted=false publication_allowed=false"

Assert-LocalLinux51ModuleEvidence $record
if ($RequireArtifacts) {
    foreach ($consumer in $record.consumers) {
        foreach ($blob in $consumer.blobs) {
            $file = Join-Path $repo "$($consumer.outputDirectory)/$($blob.name)"
            if ((Get-FileHash $file -Algorithm SHA256).Hash.ToLowerInvariant() -cne $blob.sha256) { throw 'Retained consumer artifact bytes drifted.' }
        }
        foreach ($run in $consumer.runs) {
            foreach ($blob in $run.blobs) {
                $file = Join-Path $repo "$($consumer.outputDirectory)/$($run.targetFramework)-$($run.profile)/$($blob.name)"
                if ((Get-FileHash $file -Algorithm SHA256).Hash.ToLowerInvariant() -cne $blob.sha256) { throw 'Retained run artifact bytes drifted.' }
            }
            $file = Join-Path $repo $run.video.artifactPath
            if ((Get-FileHash $file -Algorithm SHA256).Hash.ToLowerInvariant() -cne $run.video.sha256 -or (Get-Item $file).Length -ne $run.video.bytes) { throw 'Retained AVI bytes drifted.' }
        }
    }
}

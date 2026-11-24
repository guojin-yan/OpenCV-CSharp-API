[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [string]$EvidencePath = 'packaging/runtime/opencv-5.1.0-local-linux-headless-evidence.json',
    [switch]$RequireArtifacts
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'Linux51Headless.Common.ps1')
$repo = (Resolve-Path $RepositoryRoot).Path
$path = if ([IO.Path]::IsPathRooted($EvidencePath)) { $EvidencePath } else { Join-Path $repo $EvidencePath }
$json = [IO.File]::ReadAllText($path)
$schema = Join-Path $repo 'packaging/runtime/opencv-5.1.0-local-linux-headless-evidence.schema.json'
$inputs = @(Get-Headless51Inputs $repo)
$specs = @(Get-Headless51Specs)
$baseline = Get-Content (Join-Path $repo 'packaging/runtime/opencv-5.1.0-local-linux-preview-evidence.json') -Raw | ConvertFrom-Json
function Assert-HD([bool]$Condition, [string]$Message) { if (-not $Condition) { throw $Message } }
function Get-HDText([object[]]$Blobs, [string]$Name) {
    $selected = @($Blobs | Where-Object name -CEQ $Name)
    Assert-HD ($selected.Count -eq 1) "Missing or duplicated raw evidence: $Name"
    [string]$selected[0].text
}
function Test-HDBlobs([object[]]$Blobs) {
    Assert-HD (@($Blobs.name | Select-Object -Unique).Count -eq $Blobs.Count) 'Duplicate raw evidence names.'
    foreach ($blob in $Blobs) {
        Assert-HD ($blob.bytes -eq [Text.Encoding]::UTF8.GetByteCount($blob.text) -and $blob.sha256 -ceq (Get-LocalLinux51TextHash $blob.text)) "Raw evidence bytes/hash drift: $($blob.name)"
    }
}
function Get-HDSignature([object[]]$Rows) { (@($Rows | ForEach-Object { "$($_.path)|$($_.bytes)|$($_.sha256)" }) -join "`n") }
function Assert-HDEvidence([object]$r) {
    Assert-HD (Test-Json -Json ($r | ConvertTo-Json -Depth 40 -Compress) -SchemaFile $schema -ErrorAction Stop) 'Headless schema failed.'
    Assert-HD ((Get-HDSignature $r.sourceInputs) -ceq (Get-HDSignature $inputs)) 'Headless source inputs drifted.'
    Assert-HD ($r.packageVersion -ceq ('5.1.0-headless.' + $r.sourceBaseCommit.Substring(0, 7))) 'Candidate version/source identity drifted.'
    Assert-HD (($r.references.path -join '|') -ceq 'packaging/runtime/runtime-headless-profile-contract.json|packaging/runtime/runtime-headless-candidate-evidence.json|packaging/runtime/opencv-5.1.0-local-linux-preview-evidence.json') 'Historical and current evidence references drifted.'
    foreach ($reference in $r.references) { Assert-HD ($reference.sha256 -ceq (Get-FileHash (Join-Path $repo $reference.path)).Hash.ToLowerInvariant()) 'Evidence reference hash drifted.' }
    $active = Get-Content (Join-Path $repo 'packaging/runtime/runtime-package-matrix.json') -Raw
    Assert-HD ($active -notmatch '(?i)headless') 'Headless candidate entered the active package matrix.'
    $contract = Get-Content (Join-Path $repo 'packaging/runtime/runtime-headless-profile-contract.json') -Raw | ConvertFrom-Json
    Assert-HD ($contract.status -ceq 'candidate-only' -and -not $contract.packageIdentityAllowed) 'Headless contract was promoted.'
    Assert-HD (($r.kitManifest.path | Select-Object -Unique).Count -eq $r.kitManifest.Count -and $r.kitManifest.Count -eq 13) 'Isolated consumer kit inventory drifted.'
    $kitSources = @{
        'source/Program.cs' = 'tools/LinuxHeadlessPackageSmoke/Program.cs'; 'source/consumer.csproj.in' = 'tools/LinuxHeadlessPackageSmoke/consumer.csproj.in'
        'source/identity-opset13.onnx.base64' = 'tests/OpenCvSharp.Tests/Dnn/Fixtures/identity-opset13.onnx.base64'
        'scripts/Initialize-Linux51HeadlessConsumer.sh' = 'scripts/Initialize-Linux51HeadlessConsumer.sh'
        'scripts/Invoke-Linux51HeadlessConsumer.ps1' = 'scripts/Invoke-Linux51HeadlessConsumer.ps1'
    }
    foreach ($key in $kitSources.Keys) {
        $file = @($r.kitManifest | Where-Object path -CEQ $key)
        $actual = [IO.File]::ReadAllText((Join-Path $repo $kitSources[$key]))
        Assert-HD ($file.Count -eq 1 -and $file[0].sha256 -ceq (Get-LocalLinux51TextHash $actual)) 'Executed consumer kit/source mismatch.'
    }
    foreach ($tool in @(
        @('toolchain/dotnet10.tar.gz', '137268c8ad939c064ff1ee2a6fdf0899d8725377114ea012fbd1ad5fa2550418'),
        @('toolchain/dotnet8.tar.gz', '6f8b64ef9644b11bcab6748ebfec088d2ae4868df26204bb2354c3ada0f09cbe'),
        @('toolchain/powershell.tar.gz', 'dcfe6060fc86abcb859ce1f8f80843ce50bab0585396de56380ed9f25176ac6d'),
        @('toolchain/ubuntu-ca-certificates.crt', '64e7c24b0536b525fb19b145768bd5e6e12ced0df3db837c34553ea789acd143'))) {
        Assert-HD (@($r.kitManifest | Where-Object { $_.path -ceq $tool[0] -and $_.sha256 -ceq $tool[1] }).Count -eq 1) 'Consumer tool provenance drifted.'
    }
    Test-HDBlobs $r.producer.blobs
    $build = (Get-HDText $r.producer.blobs 'producer.json') | ConvertFrom-Json
    Assert-HD ($build.status -ceq 'candidate-only' -and $build.openCvVersion -ceq '5.1.0' -and $build.sourceBaseCommit -ceq $r.sourceBaseCommit -and $build.worktreeChangesIncluded -and $build.upstreamBuildReused) 'Producer scope or source binding drifted.'
    Assert-HD ($build.upstreamProducer.openCvCommit -ceq 'e4f78fbcf1a773fbed69e14500e8b734963eb585' -and $build.upstreamProducer.contribCommit -ceq 'b67a855188a3d683813731010f6d39ac614733a4' -and $build.upstreamProducer.openCvVersion -ceq '5.1.0') 'Official upstream provenance drifted.'
    Assert-HD (($build.overlayEdits.path -join '|') -ceq 'CMakeLists.txt|src/highgui/highgui.cpp|tests/native_smoke.cpp') 'Unexpected candidate source overlay.'
    foreach ($edit in $build.overlayEdits) {
        Assert-HD ($edit.derivedSha256 -ceq (Get-LocalLinux51TextHash (Get-HDText $r.producer.blobs ('overlay/' + $edit.path)))) 'Derived source hash drifted.'
    }
    Assert-HD (($build.profiles.profile -join '|') -ceq 'full|mini' -and ($r.packages.profile -join '|') -ceq 'full|mini') 'Producer/package profile matrix incomplete.'
    foreach ($profile in @('full', 'mini')) {
        $p = @($build.profiles | Where-Object profile -CEQ $profile)[0]
        $package = @($r.packages | Where-Object profile -CEQ $profile)[0]
        $expectedModules = if ($profile -ceq 'full') { @($contract.requiredModules) } else { @('core', 'imgproc', 'imgcodecs', 'videoio', 'geometry', 'flann') }
        Assert-HD ((($p.modules | Sort-Object -CaseSensitive) -join '|') -ceq (($expectedModules | Sort-Object -CaseSensitive) -join '|')) 'Selected headless module list drifted.'
        Assert-HD ($p.wrapperSourceCount -eq $(if ($profile -ceq 'full') {49} else {9})) 'Actual wrapper source count drifted.'
        Assert-HD ($package.id -ceq ('JYPPX.OpenCV.runtime.linux-x64.preview' + $(if ($profile -ceq 'mini') {'.mini'} else {''})) -and
            $package.version -ceq $r.packageVersion -and $package.sha256 -ceq $p.package.sha256 -and $package.bytes -eq $p.package.bytes) 'Local package identity/hash drifted.'
        $kitPackage = @($r.kitManifest | Where-Object path -CEQ ('packages/' + [IO.Path]::GetFileName($package.artifactPath)))
        Assert-HD ($kitPackage.Count -eq 1 -and $kitPackage[0].sha256 -ceq $package.sha256 -and $kitPackage[0].bytes -eq $package.bytes) 'Candidate package feed bytes drifted.'
        Assert-HD (@($package.entries.path | Select-Object -Unique).Count -eq $package.entries.Count) 'Duplicate package entries.'
        $native = @($package.entries | Where-Object path -CLike 'runtimes/linux-x64/native/*')
        Assert-HD ($native.Count -eq $(if ($profile -ceq 'full') {49} else {19}) -and @($native | Where-Object path -Match 'highgui').Count -eq 0) 'Candidate native closure contains GUI or misses files.'
        foreach ($entry in $native) {
            $file = @($p.runtimeFiles | Where-Object path -CEQ ([IO.Path]::GetFileName($entry.path)))
            Assert-HD ($file.Count -eq 1 -and $file[0].sha256 -ceq $entry.sha256 -and $file[0].bytes -eq $entry.bytes) 'Package bytes differ from the measured producer.'
            if ($entry.path -notmatch 'JYPPX') {
                $original = @(@($baseline.profiles | Where-Object name -CEQ $profile)[0].entries | Where-Object path -CEQ $entry.path)
                Assert-HD ($original.Count -eq 1 -and $original[0].sha256 -ceq $entry.sha256) 'Reused upstream module differs from baseline.'
            }
        }
        $elf = (Get-HDText $r.producer.blobs "$profile-elf.json") | ConvertFrom-Json
        $manifestName = if ($profile -ceq 'full') { 'native_abi_manifest.txt' } else { 'native_abi_mini_manifest.txt' }
        $abi = Get-HDText $r.producer.blobs "generated-abi/$manifestName"
        $exports = @($abi -split "`n" | Where-Object { $_ -match '^jyppx_ocv_.*\|' } | ForEach-Object { ($_ -split '\|')[0] })
        Assert-HD ($p.abi.generatedIndependently -and $p.abi.manifestSha256 -ceq (Get-LocalLinux51TextHash $abi) -and
            $exports.Count -eq $elf.exportCount -and $p.abi.exportCount -eq $exports.Count -and
            (Get-LocalLinux51TextHash (($exports -join "`n") + "`n")) -ceq $elf.exportNamesSha256 -and $elf.exportNamesSha256 -ceq $p.abi.exportNamesSha256) 'Independent manifest and actual exports disagree.'
        Assert-HD ($elf.runtimeFileCount -eq $native.Count -and $elf.files.Count -eq ($expectedModules.Count + 1)) 'ELF closure size drifted.'
        foreach ($file in $elf.files) {
            Assert-HD (($file.runtimePaths -join '|') -ceq '$ORIGIN' -and $file.lddOutput -notmatch 'not found|/repo-input|/upstream-input|/artifacts/opencv-install/' -and
                ($file.needed -join '|') -notmatch 'lib(gtk|gdk|Qt|X11|Xext|wayland|opencv_highgui)') 'GUI, unresolved dependency or producer path in actual ELF closure.'
            $nativeFile = @($native | Where-Object path -CEQ ('runtimes/linux-x64/native/' + $file.name))[0]
            Assert-HD ($nativeFile.sha256 -ceq $file.sha256 -and $nativeFile.bytes -eq $file.bytes) 'ELF audit differs from packaged bytes.'
            Assert-HD ([version]$file.symbolFloors.GLIBC -le [version]'2.35' -and [version]$file.symbolFloors.GLIBCXX -le [version]'3.4.30' -and [version]$file.symbolFloors.CXXABI -le [version]'1.3.13') 'ELF runtime floor exceeds baseline.'
        }
        $ctest = Get-HDText $r.producer.blobs "$profile-ctest.log"
        Assert-HD ($ctest.Contains('100% tests passed, 0 tests failed out of 3') -and ([regex]::Matches($ctest, '\d/3 Test #\d:.*Passed')).Count -eq 3) 'Actual CTest execution incomplete.'
        $compile = (Get-HDText $r.producer.blobs "$profile-compile-commands.json") | ConvertFrom-Json
        $sources = @($compile | Where-Object command -Match 'CMakeFiles/JYPPX\.OpenCV\.Native\.dir/')
        Assert-HD ($sources.Count -eq $p.wrapperSourceCount -and @($sources | Where-Object command -NotMatch 'OPENCV_CSHARP_HEADLESS=1').Count -eq 0) 'Headless overlay was not actually compiled.'
        $sbomPath = Join-Path $repo $package.sbom.path
        Assert-HD ($package.sbom.sha256 -ceq (Get-FileHash $sbomPath).Hash.ToLowerInvariant()) 'Candidate SBOM hash drifted.'
        $sbom = Get-Content $sbomPath -Raw | ConvertFrom-Json
        Assert-HD ($sbom.spdxVersion -ceq 'SPDX-2.3' -and $sbom.packages.Count -eq 1 -and $sbom.packages[0].versionInfo -ceq $package.version -and
            $sbom.packages[0].checksums[0].checksumValue -ceq $package.sha256 -and $sbom.files.Count -eq $package.entries.Count -and $sbom.comment.Contains('Candidate-only')) 'SBOM identity or scope drifted.'
        foreach ($entry in $package.entries) {
            $file = @($sbom.files | Where-Object fileName -CEQ ('./' + $entry.path))
            Assert-HD ($file.Count -eq 1 -and @($file[0].checksums | Where-Object { $_.algorithm -ceq 'SHA256' -and $_.checksumValue -ceq $entry.sha256 }).Count -eq 1 -and
                @($file[0].checksums | Where-Object { $_.algorithm -ceq 'SHA1' -and $_.checksumValue -ceq $entry.sha1 }).Count -eq 1) 'SBOM file inventory differs from package.'
        }
        $verification = [Convert]::ToHexString([Security.Cryptography.SHA1]::HashData([Text.Encoding]::UTF8.GetBytes((($package.entries.sha1 | Sort-Object -CaseSensitive) -join '')))).ToLowerInvariant()
        Assert-HD ($sbom.packages[0].packageVerificationCode.packageVerificationCodeValue -ceq $verification) 'SBOM verification code drifted.'
    }
    Assert-HD ($r.managedPackage.sha256 -ceq '0081cd584da74aac7e51187985fbe4ceff3406f960525734633a3ba32768fd80' -and $r.managedPackage.bytes -eq 11924069) 'Managed package bytes drifted.'
    Assert-HD (@($r.kitManifest | Where-Object { $_.path -ceq 'packages/JYPPX.OpenCV.CSharp.API.5.1.0.nupkg' -and $_.sha256 -ceq $r.managedPackage.sha256 -and $_.bytes -eq $r.managedPackage.bytes }).Count -eq 1) 'Managed package feed bytes drifted.'
    $feedRecords = @([ordered]@{ id = 'JYPPX.OpenCV.CSharp.API'; version = '5.1.0'; name = 'JYPPX.OpenCV.CSharp.API.5.1.0.nupkg'; sha256 = $r.managedPackage.sha256 })
    $feedRecords += @($r.packages | ForEach-Object { [ordered]@{ id = $_.id; version = $_.version; name = [IO.Path]::GetFileName($_.artifactPath); sha256 = $_.sha256 } })
    $feedText = ($feedRecords | ConvertTo-Json -Depth 6) + "`n"
    Assert-HD (@($r.kitManifest | Where-Object { $_.path -ceq 'packages.json' -and $_.sha256 -ceq (Get-LocalLinux51TextHash $feedText) }).Count -eq 1) 'Consumer feed manifest differs from the measured packages.'
    $ids = @($r.producer.identity.id); $volumes = @(); $runCount = 0
    for ($i = 0; $i -lt 3; $i++) {
        $c = $r.consumers[$i]; $spec = $specs[$i]; $id = $c.identity
        Assert-HD ($c.rid -ceq $spec.rid -and $id.image -ceq $spec.image -and $id.imageArchitecture -ceq 'amd64' -and $id.imageOs -ceq 'linux' -and -not $id.oomKilled) 'Consumer distro/image/architecture drifted.'
        Assert-HD ($id.id -cnotin $ids) 'Consumer is not independent.'; $ids += $id.id
        $kitMount = @($id.mounts | Where-Object destination -CEQ '/kit'); $volume = @($id.mounts | Where-Object destination -CEQ '/consumer')
        Assert-HD ($id.mounts.Count -eq 2 -and $kitMount.Count -eq 1 -and -not $kitMount[0].writable -and $volume.Count -eq 1 -and $volume[0].type -ceq 'volume' -and $volume[0].source -cnotin $volumes -and
            @($r.producer.identity.mounts | Where-Object source -CEQ $volume[0].source).Count -eq 0) 'Consumer shares producer paths or writable kit.'
        $volumes += $volume[0].source
        Test-HDBlobs $c.blobs
        $envFacts = (Get-HDText $c.blobs 'environment.json') | ConvertFrom-Json
        Assert-HD ($envFacts.consumerRid -ceq $spec.rid -and $envFacts.libc -ceq $spec.libc -and $envFacts.architecture -ceq 'x86_64' -and
            $envFacts.compilerAbsent -and $envFacts.producerPathsAbsent -and $envFacts.nativeLoaderOverridesAbsent) 'Consumer environment is not independent.'
        Assert-HD ((Get-HDText $c.blobs 'bootstrap.log').Contains('HEADLESS_51_CONSUMER_READY') -and (Get-HDText $c.blobs 'driver.log').Contains("HEADLESS_51_CONSUMER_OK rid=$($spec.rid) runs=4")) 'Consumer bootstrap or driver did not finish.'
        Assert-HD (($c.runs | ForEach-Object { "$($_.targetFramework)-$($_.profile)" }) -join '|' -ceq 'net8.0-full|net8.0-mini|net10.0-full|net10.0-mini') 'Consumer TFM/profile matrix incomplete.'
        foreach ($run in $c.runs) {
            $runCount++; Test-HDBlobs $run.blobs
            $v = (Get-HDText $run.blobs 'result.json') | ConvertFrom-Json
            $tfm = $run.targetFramework; $profile = $run.profile
            $package = @($r.packages | Where-Object profile -CEQ $profile)[0]
            Assert-HD ($v.status -ceq 'passed' -and $v.targetFramework -ceq $tfm -and $v.profile -ceq $profile -and $v.openCvVersion -ceq '5.1.0' -and $v.packageVersion -ceq '5.1.0' -and $v.nativeAbi -eq 1 -and
                $v.runtimeVersion -ceq $(if ($tfm -ceq 'net8.0') {'8.0.34'} else {'10.0.12'})) 'Actual managed/native execution identity drifted.'
            $managed = @($r.managedPackage.assemblies | Where-Object path -CLike "lib/$tfm/*")[0]
            Assert-HD ($v.managedAssemblySha256 -ceq $managed.sha256 -and $v.displayUnset -and $v.guiLibrariesAbsent -and $v.nativeStateUsableAfterEachGuiFailure) 'Managed assembly or headless runtime facts drifted.'
            Assert-HD ($v.highgui.status -ceq 'passed' -and ($v.highgui.checks.name -join '|') -ceq 'NamedWindow|DestroyWindow|ImShow|CreateTrackbar|GetCurrentUIFramework|WaitKey') 'GUI negative coverage missing.'
            foreach ($check in $v.highgui.checks) {
                Assert-HD ($check.nativeRecovery -and $check.status -ceq $(if ($profile -ceq 'full') {'NOT_LINKED'} else {'entrypoint-absent'})) 'GUI path was not deterministic or native recovery failed.'
            }
            $missing = $v.missingVideo
            Assert-HD ($missing.status -ceq 'returned-false' -and $missing.backend -ceq 'CV_MJPEG' -and $missing.attempts -eq 2 -and $missing.tasksBefore -eq $missing.tasksAfter -and
                $missing.socketsBefore -eq $missing.socketsAfter -and -not $missing.fileHandleLeaked) 'VideoIO negative path leaked or did not finish.'
            Assert-HD ($v.image.exactPixelRoundtrip -and $v.image.pngBytes -eq 146 -and $v.image.pngSha256 -ceq '85c0160486a1c381e8c7adc062e03491aef8133428ad99be492be0302fe7ada7') 'PNG codec pixels drifted.'
            $video = $v.video
            Assert-HD ($video.frameCount -eq 3 -and $video.endOfFileVerified -and $video.writerBackend -ceq 'CV_MJPEG' -and $video.readerBackend -ceq 'CV_MJPEG' -and
                $video.fileBytes -eq 6288 -and $video.fileSha256 -ceq $run.video.sha256 -and $run.video.bytes -eq $video.fileBytes) 'Video write/read/EOF drifted.'
            for ($frame = 0; $frame -lt 3; $frame++) { Assert-HD ($video.frames[$frame].index -eq $frame -and $video.frames[$frame].checksum -eq (152880 + 23040 * $frame) -and $video.frames[$frame].maxAbsoluteError -eq 2) 'Actual video frame pixels drifted.' }
            Assert-HD ($v.dnn.modelBytes -eq 147 -and $v.dnn.modelSha256 -ceq '326793cdb2fc2da739a715c3f3ff71d09779b389ad29e56bbfccc4313e900744') 'ONNX fixture drifted.'
            if ($profile -ceq 'full') {
                Assert-HD ($v.dnn.status -ceq 'passed' -and $v.dnn.inferenceExecuted -and $v.dnn.inferences.Count -eq 2 -and $v.dnn.backend -ceq 'OpenCV' -and $v.dnn.target -ceq 'Cpu') 'Full DNN was not executed.'
                for ($inference = 0; $inference -lt 2; $inference++) {
                    $expected = if ($inference -eq 0) {'1|2|3|4'} else {'-4|-3|-2|-1'}
                    Assert-HD (($v.dnn.inferences[$inference].input -join '|') -ceq $expected -and ($v.dnn.inferences[$inference].output -join '|') -ceq $expected) 'Actual ONNX output values drifted.'
                }
            } else { Assert-HD ($v.dnn.status -ceq 'omitted' -and -not $v.dnn.inferenceExecuted -and $v.dnn.reasonCode -ceq 'entrypoint-absent') 'Mini DNN omission was not exercised.' }
            Assert-HD (@($v.backends | Where-Object { $_.name -ceq 'CV_MJPEG' -and $_.available -and $_.builtIn }).Count -eq 1 -and
                @($v.backends | Where-Object { $_.name -in @('FFMPEG', 'GSTREAMER', 'INTEL_MFX', 'UEYE') -and $_.available }).Count -eq 0) 'VideoIO backend facts drifted.'
            $nativeEntries = @($package.entries | Where-Object path -CLike 'runtimes/linux-x64/native/*')
            Assert-HD ($v.nativeFiles.Count -eq $nativeEntries.Count -and @($v.nativeFiles.name | Select-Object -Unique).Count -eq $v.nativeFiles.Count) 'Executed native payload is incomplete.'
            foreach ($entry in $nativeEntries) {
                $file = @($v.nativeFiles | Where-Object name -CEQ ([IO.Path]::GetFileName($entry.path)))
                Assert-HD ($file.Count -eq 1 -and $file[0].sha256 -ceq $entry.sha256 -and $file[0].bytes -eq $entry.bytes) 'Consumer native bytes differ from candidate package.'
            }
            $loaded = @($v.loadedOpenCvPaths)
            Assert-HD ($loaded.Count -eq $(if ($profile -ceq 'full') {17} else {7}) -and @($loaded | Select-Object -Unique).Count -eq $loaded.Count) 'Loaded module closure drifted.'
            foreach ($item in $loaded) { Assert-HD ($item.StartsWith("/consumer/runs/$tfm-$profile/bin/Release/$tfm/linux-x64/", [StringComparison]::Ordinal) -and $item -notmatch 'highgui|\.\.') 'Native module loaded outside the package consumer.' }
            $assets = (Get-HDText $run.blobs 'project.assets.json') | ConvertFrom-Json
            $libraries = @($assets.libraries.PSObject.Properties.Name)
            Assert-HD ($libraries.Count -eq 2 -and $libraries -ccontains 'JYPPX.OpenCV.CSharp.API/5.1.0' -and $libraries -ccontains "$($package.id)/$($package.version)" -and
                ($assets.project.restore.sources.PSObject.Properties.Name -join '|') -ceq '/consumer/feed') 'Package-only restore used the wrong feed or packages.'
            $cached = @((Get-HDText $run.blobs 'cache-packages.json') | ConvertFrom-Json)
            Assert-HD ($cached.Count -eq 2 -and @($cached | Where-Object sha256 -CEQ $package.sha256).Count -eq 1 -and @($cached | Where-Object sha256 -CEQ $r.managedPackage.sha256).Count -eq 1) 'Restored archive bytes differ from the measured feed.'
            $buildLog = Get-HDText $run.blobs 'build.stdout.log'
            $restoreError = Get-HDText $run.blobs 'restore.stderr.log'
            $knownSdkWelcome = $tfm -ceq 'net10.0' -and $profile -ceq 'full' -and
                (Get-LocalLinux51TextHash $restoreError) -ceq 'f67e92fd2ed736b2c538849898ed20ec576f9488f5df16cf95e895f564f1b215'
            Assert-HD ($buildLog.Contains('0 Warning(s)') -and $buildLog.Contains('0 Error(s)') -and (Get-HDText $run.blobs 'build.stderr.log') -ceq '' -and
                ($restoreError -ceq '' -or $knownSdkWelcome) -and (Get-HDText $run.blobs 'restore.stdout.log') -notmatch '(?i)warning') 'Consumer restore/build warnings or errors.'
            $errors = @((Get-HDText $run.blobs 'execute.stderr.log') -split "`n" | Where-Object { $_ })
            Assert-HD ($errors.Count -eq $(if ($profile -ceq 'full') {3} else {2})) 'Unexpected native stderr count.'
            foreach ($line in $errors) { Assert-HD ($line -match '^\[ WARN:0@[0-9.]+\] global (cap.cpp:212 open VIDEOIO\(CV_MJPEG\): backend is generally available but can.t be used to capture by name|net_impl_backend.cpp:388 setPreferableTarget Targets are not supported by the new graph engine for now)$') 'Unexpected native stderr diagnostic.' }
            Assert-HD ((Get-HDText $run.blobs 'execute.stdout.log').Trim() -ceq (Get-HDText $run.blobs 'result.json').Trim()) 'Execution stdout and result evidence disagree.'
        }
    }
    Assert-HD ($runCount -eq 12) 'Headless consumer execution count drifted.'
}
function Set-HDBlob([object]$Blob, [string]$Text) { $Blob.text = $Text; $Blob.bytes = [Text.Encoding]::UTF8.GetByteCount($Text); $Blob.sha256 = Get-LocalLinux51TextHash $Text }
function Edit-HDResult([object]$Record, [scriptblock]$Mutation) {
    $blobs = $Record.consumers[0].runs[0].blobs
    $result = (Get-HDText $blobs 'result.json') | ConvertFrom-Json
    & $Mutation $result
    $text = ($result | ConvertTo-Json -Depth 20 -Compress) + "`n"
    Set-HDBlob @($blobs | Where-Object name -CEQ 'result.json')[0] $text
    Set-HDBlob @($blobs | Where-Object name -CEQ 'execute.stdout.log')[0] $text
}
$record = $json | ConvertFrom-Json
Assert-HDEvidence $record
$fixtures = @(
    {param($r) $r.publicationAllowed = $true}, {param($r) $r.candidateOnly = $false},
    {param($r) $r.activePackageIdentityAdded = $true}, {param($r) $r.coverage.freshMinimalUpstreamBuild = $true},
    {param($r) $r.coverage.cameraOrNetworkValidated = $true}, {param($r) $r.references[1].sha256 = '0' * 64},
    {param($r) $r.sourceInputs[0].sha256 = '0' * 64}, {param($r) $r.kitManifest[0].sha256 = '0' * 64},
    {param($r) $r.packages[0].sha256 = '0' * 64}, {param($r) $r.packages[0].sbom.sha256 = '0' * 64},
    {param($r) $r.consumers[0].identity.id = $r.producer.identity.id}, {param($r) $r.consumers[1].rid = $r.consumers[0].rid},
    {param($r) $r.consumers[0].identity.oomKilled = $true},
    {param($r) @($r.consumers[0].identity.mounts | Where-Object destination -CEQ '/kit')[0].writable = $true},
    {param($r) $r.consumers[0].runs[0].profile = 'mini'},
    {param($r) Edit-HDResult $r {param($v) $v.highgui.checks[0].status = 'succeeded'}},
    {param($r) Edit-HDResult $r {param($v) $v.highgui.checks[1].nativeRecovery = $false}},
    {param($r) Edit-HDResult $r {param($v) $v.missingVideo.tasksAfter++}},
    {param($r) Edit-HDResult $r {param($v) $v.missingVideo.socketsAfter++}},
    {param($r) Edit-HDResult $r {param($v) $v.image.exactPixelRoundtrip = $false}},
    {param($r) Edit-HDResult $r {param($v) $v.video.frameCount = 0}},
    {param($r) Edit-HDResult $r {param($v) $v.dnn.inferences[0].output[0] = 99}},
    {param($r) Edit-HDResult $r {param($v) $v.guiLibrariesAbsent = $false}},
    {param($r) Edit-HDResult $r {param($v) $v.nativeFiles[0].sha256 = '0' * 64}},
    {param($r) Edit-HDResult $r {param($v) $v.loadedOpenCvPaths[0] = '/workspace/libopencv_core.so.501'}},
    {param($r) Set-HDBlob @($r.producer.blobs | Where-Object name -CEQ 'full-ctest.log')[0] 'skipped'},
    {param($r) Set-HDBlob @($r.consumers[0].runs[0].blobs | Where-Object name -CEQ 'execute.stderr.log')[0] 'unexpected failure'}
)
$fixtureIndex = 0
foreach ($fixture in $fixtures) {
    $fixtureIndex++
    $mutant = $json | ConvertFrom-Json; & $fixture $mutant
    $rejected = $false
    try { Assert-HDEvidence $mutant } catch { $rejected = $true }
    Assert-HD $rejected "Headless evidence guard accepted negative fixture $fixtureIndex."
}
if ($RequireArtifacts) {
    $groups = @([pscustomobject]@{ directory = $record.producer.outputDirectory; blobs = $record.producer.blobs })
    foreach ($c in $record.consumers) {
        $groups += [pscustomobject]@{ directory = $c.outputDirectory; blobs = $c.blobs }
        foreach ($run in $c.runs) {
            $dir = "$($c.outputDirectory)/$($run.targetFramework)-$($run.profile)"
            $groups += [pscustomobject]@{ directory = $dir; blobs = $run.blobs }
            Assert-HD ((Get-FileHash (Join-Path $repo "$dir/three-frames.avi")).Hash.ToLowerInvariant() -ceq $run.video.sha256) 'Retained video bytes drifted.'
        }
    }
    foreach ($group in $groups) { foreach ($blob in $group.blobs) {
        Assert-HD ((Get-FileHash (Join-Path $repo "$($group.directory)/$($blob.name)")).Hash.ToLowerInvariant() -ceq $blob.sha256) 'Retained raw evidence drifted.'
    } }
    foreach ($p in $record.packages) {
        $package = Join-Path $repo $p.artifactPath
        Assert-HD ((Get-FileHash $package).Hash.ToLowerInvariant() -ceq $p.sha256 -and (Get-Item $package).Length -eq $p.bytes) 'Retained candidate package drifted.'
        $actualEntries = @(Get-LocalLinux51PackageManifest $package)
        Assert-HD ($actualEntries.Count -eq $p.entries.Count) 'Retained package entry count drifted.'
        foreach ($entry in $p.entries) {
            $actualEntry = @($actualEntries | Where-Object path -CEQ $entry.path)
            Assert-HD ($actualEntry.Count -eq 1 -and $actualEntry[0].bytes -eq $entry.bytes -and $actualEntry[0].sha256 -ceq $entry.sha256) 'Retained package entry bytes drifted.'
        }
    }
}
Write-Host "HEADLESS_51_EVIDENCE_OK profiles=2 distros=3 executions=12 gui_negative_calls=72 native_ctest=6 negative_fixtures=$($fixtures.Count) candidate_only=true publication_allowed=false"

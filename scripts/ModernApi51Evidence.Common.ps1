function Resolve-ModernApi51Path {
    param([string]$RepositoryRoot, [string]$Path)
    if ([IO.Path]::IsPathRooted($Path)) { return [IO.Path]::GetFullPath($Path) }
    return [IO.Path]::GetFullPath((Join-Path $RepositoryRoot $Path))
}

function Get-ModernApi51Sha256 {
    param([string]$Path)
    return (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
}

function Get-ModernApi51SourceSet {
    param([string]$RepositoryRoot)
    $scopes = @('Directory.Build.props', 'src/OpenCvSharp', 'tests/OpenCvSharp.Tests', 'samples/ConsoleSamples',
        'scripts/ModernApi51Evidence.Common.ps1', 'scripts/Measure-ModernApi51Evidence.ps1')
    $paths = [string[]]@(& git -C $RepositoryRoot ls-files --cached --others --exclude-standard -- @scopes)
    if ($LASTEXITCODE -ne 0 -or $paths.Count -eq 0) { throw 'Cannot enumerate modern API evidence source inputs.' }
    [Array]::Sort($paths, [StringComparer]::Ordinal)
    $rows = foreach ($path in $paths) {
        $content = [IO.File]::ReadAllText((Join-Path $RepositoryRoot $path))
        $bytes = [Text.UTF8Encoding]::new($false).GetBytes((($content -replace "`r`n", "`n") -replace "`r", "`n").TrimEnd() + "`n")
        $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
        "$path|$($bytes.Length)|$hash"
    }
    $manifestBytes = [Text.UTF8Encoding]::new($false).GetBytes(($rows -join "`n") + "`n")
    return [ordered]@{
        paths = $scopes
        fileCount = $paths.Count
        sha256 = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($manifestBytes)).ToLowerInvariant()
    }
}

function Get-ModernApi51PackageBindings {
    param([string]$RepositoryRoot, [string]$SourceCommit)
    $relativePath = 'packaging/runtime/opencv-5.1.0-package-consumer-evidence.json'
    $path = Join-Path $RepositoryRoot $relativePath
    $record = Get-Content -LiteralPath $path -Raw | ConvertFrom-Json
    if ($record.recordKind -cne 'opencv-5.1.0-package-consumer-evidence' -or $record.status -cne 'verified-local-windows-x64' -or
        $record.openCvVersion -cne '5.1.0' -or $record.packageVersion -cne '5.1.0.0' -or $record.rid -cne 'win-x64') {
        throw 'Modern API evidence requires verified OpenCV 5.1.0 Windows x64 package-consumer inputs.'
    }
    foreach ($revision in @($record.sourceCommit, $record.nativeBuildSourceCommit)) {
        if ([string]$revision -notmatch '^[0-9a-f]{40}$') { throw 'Package evidence source revision is malformed.' }
        & git -C $RepositoryRoot merge-base --is-ancestor $revision $SourceCommit 2>$null
        if ($LASTEXITCODE -ne 0) { throw "Package source revision is not an ancestor of the measured source: $revision" }
    }
    $packages = foreach ($profile in @('full', 'mini')) {
        $rows = @($record.packages | Where-Object label -CEQ $profile)
        $packageId = if ($profile -ceq 'full') { 'JYPPX.OpenCV.runtime.win-x64' } else { 'JYPPX.OpenCV.runtime.win-x64.mini' }
        $expectedCount = if ($profile -ceq 'full') { 43 } else { 7 }
        if ($rows.Count -ne 1 -or $rows[0].packageId -cne $packageId -or $rows[0].nugetVersion -cne '5.1.0' -or
            $rows[0].packageVersion -cne '5.1.0.0' -or $rows[0].sha256 -notmatch '^[0-9a-f]{64}$' -or
            [int]$rows[0].nativeFiles -ne $expectedCount -or @($rows[0].nativeFilesSha256).Count -ne $expectedCount) {
            throw "Invalid OpenCV 5.1 $profile package input."
        }
        $payload = @($rows[0].nativeFilesSha256 | Sort-Object path | ForEach-Object {
            if ($_.path -cnotmatch '^runtimes/win-x64/native/(JYPPX\.OpenCV\.Native|opencv_[a-z0-9_]+510)\.dll$' -or
                [int64]$_.length -le 0 -or $_.sha256 -notmatch '^[0-9a-f]{64}$') { throw "Invalid $profile native DLL evidence row." }
            [pscustomobject][ordered]@{ name = [IO.Path]::GetFileName([string]$_.path); bytes = [int64]$_.length; sha256 = [string]$_.sha256 }
        })
        if (@($payload | Group-Object name | Where-Object Count -ne 1).Count -ne 0) { throw "Duplicate $profile native DLL evidence." }
        [pscustomobject][ordered]@{ profile = $profile; packageFileName = ($packageId + '.5.1.0.nupkg').ToLowerInvariant(); packageSha256 = [string]$rows[0].sha256; payload = $payload }
    }
    return [ordered]@{
        reference = [ordered]@{ path = $relativePath; sha256 = Get-ModernApi51Sha256 -Path $path }
        packages = @($packages)
    }
}

function Assert-ModernApi51Payload {
    param([string]$Directory, [object[]]$Expected)
    $files = @(Get-ChildItem -LiteralPath $Directory -File -Recurse -Filter '*.dll' |
        Where-Object { $_.Name -match '^JYPPX\.OpenCV\.Native\.dll$|^opencv_.*\.dll$' } | Sort-Object Name)
    if ($files.Count -ne $Expected.Count) { throw "Isolated runtime DLL count differs: expected=$($Expected.Count) actual=$($files.Count)." }
    for ($index = 0; $index -lt $files.Count; $index++) {
        if ($files[$index].DirectoryName -cne $Directory -or $files[$index].Name -cne $Expected[$index].name -or
            [int64]$files[$index].Length -ne [int64]$Expected[$index].bytes -or
            (Get-ModernApi51Sha256 -Path $files[$index].FullName) -cne $Expected[$index].sha256) {
            throw "Isolated runtime DLL differs from the 5.1 package evidence: $($files[$index].Name)"
        }
    }
}

function Get-ModernApi51TestCases {
    param([ValidateSet('none', 'full', 'mini')][string]$Profile)
    $prefix = 'JYPPX.OpenCvSharp.Tests.'
    $cases = @(
        'Core.OpenCvCapabilitiesTests.ProbeResultHasStableStateAndAvailabilitySemantics',
        'Core.OpenCvCapabilitiesTests.SnapshotIsSafeToReadWithoutNativeSmoke',
        'Core.OpenCvCapabilitiesTests.SnapshotDoesNotExposeMutableCollections',
        'Core.OpenCvCapabilitiesTests.SnapshotJsonIsDeterministicAndOmitsMachinePaths',
        'ImgCodecs.Cv2InteropTests.ImEncodeRejectsOddParameterCount',
        'ImgCodecs.Cv2InteropTests.ImEncodeRejectsManagedInvalidInputsBeforeNativeCall',
        'ImgCodecs.Cv2InteropTests.ImDecodeRejectsManagedInvalidInputsBeforeNativeCall'
    )
    if ($Profile -cne 'none') {
        $cases += @(
            'Core.OpenCvCapabilitiesTests.NativeSmokeSnapshotVerifiesExistingProbes',
            'ImgCodecs.Cv2InteropTests.ImEncodeAndImDecodeRoundTripPngWhenNativeRuntimeIsAvailable',
            'ImgCodecs.Cv2InteropTests.ImEncodeToWriterAdvancesOnceAndDoesNotRetryWriterFaultsWhenNativeRuntimeIsAvailable',
            'ImgCodecs.Cv2InteropTests.ImEncodeToWritesOneExactCallerOwnedSegmentWhenNativeRuntimeIsAvailable',
            'ImgCodecs.Cv2InteropTests.ImEncodeAcceptsPngAndJpegParametersWhenNativeRuntimeIsAvailable',
            'ImgCodecs.Cv2InteropTests.TypeSafeEncodingParametersRoundTripPngWhenNativeRuntimeIsAvailable',
            'ImgCodecs.CodecWriterAllocationTests.ReusedCallerOwnedWriterMaintainsPayloadAcrossNativeEncodes',
            'ImgCodecs.CodecWriterAllocationTests.AdvanceFaultCanBeRepeatedWithoutPoisoningTheNextEncode'
        )
    }
    if ($Profile -ceq 'mini') { $cases += 'Headless.HeadlessRuntimeCandidateTests.MiniProfileRejectsHighGuiAndKeepsCodecUsable' }
    return @($cases | ForEach-Object { $prefix + $_ } | Sort-Object)
}

function Assert-ModernApi51Snapshot {
    param([object]$Snapshot, [ValidateSet('none', 'full', 'mini')][string]$Profile)
    if ($Snapshot.managedPackageVersion -cne '5.1.0' -or $Snapshot.openCvVersion -cne '5.1.0' -or
        [int]$Snapshot.nativeAbiVersion -ne 1 -or [int]$Snapshot.processBitness -ne 64 -or $Snapshot.processArchitecture -cne 'X64') {
        throw "Capability snapshot version/ABI/architecture drifted for $Profile."
    }
    if ((@($Snapshot.modules.name) -join '|') -cne 'core|imgproc|imgcodecs|videoio' -or
        @($Snapshot.optionalModules).Count -ne 25 -or @($Snapshot.optionalModules | Where-Object state -CNE 'Declared').Count -ne 0 -or
        (@($Snapshot.accelerators.name) -join '|') -cne 'opencl-tapi|cuda' -or
        @($Snapshot.accelerators | Where-Object state -CNE 'Unknown').Count -ne 0) { throw "Capability declaration/accelerator boundaries drifted for $Profile." }
    if ((@($Snapshot.codecs.extension) -join '|') -cne '.png|.jpg|.webp|.tiff|.bmp|.gif|.exr|.jp2' -or
        @($Snapshot.codecs | Where-Object { $_.reader.state -cne 'Declared' }).Count -ne 0 -or
        (@($Snapshot.dnnBackends.backend) -join '|') -cne 'Default|InferenceEngine|OpenCV|VkCom|Cuda|WebNN|TimVx|Cann') {
        throw "Capability codec/backend inventory drifted for $Profile."
    }
    if ($Profile -ceq 'none') {
        if ($Snapshot.nativeRuntime.state -cne 'Unknown' -or $Snapshot.nativeOpenCvVersion -cne '' -or
            $null -ne $Snapshot.loadedNativeAbiVersion -or $Snapshot.guiBackend.state -cne 'Unavailable' -or
            @($Snapshot.modules | Where-Object state -CNE 'Unknown').Count -ne 0 -or @($Snapshot.warnings).Count -eq 0 -or
            @($Snapshot.codecs | Where-Object { $_.writer.state -cne 'Unavailable' }).Count -ne 0) { throw 'No-runtime capability negative path was not verified.' }
    }
    else {
        if ($Snapshot.nativeRuntime.state -cne 'Verified' -or $Snapshot.nativeOpenCvVersion -cne '5.1.0' -or
            [int]$Snapshot.loadedNativeAbiVersion -ne 1 -or @($Snapshot.modules | Where-Object state -CNE 'Verified').Count -ne 0 -or
            [int]$Snapshot.logicalCpuCount -le 0 -or [string]::IsNullOrWhiteSpace([string]$Snapshot.cpuFeaturesLine)) { throw "$Profile capability positive path was not verified." }
        $writers = @($Snapshot.codecs | ForEach-Object { $_.writer.state })
        if (($writers -join '|') -cne 'Verified|Verified|Verified|Verified|Verified|Verified|Unavailable|Verified') { throw "$Profile codec writer states drifted." }
        if ($Profile -ceq 'full' -and ($Snapshot.guiBackend.state -cne 'Verified' -or @($Snapshot.warnings).Count -ne 0)) { throw 'Full GUI capability evidence drifted.' }
        if ($Profile -ceq 'mini' -and ($Snapshot.guiBackend.state -cne 'Unavailable' -or @($Snapshot.warnings).Count -eq 0)) { throw 'Mini GUI omission boundary drifted.' }
    }
    foreach ($backend in @($Snapshot.dnnBackends)) {
        $cpuExpected = $Profile -ceq 'full' -and $backend.backend -cin @('Default', 'OpenCV')
        $expectedState = if ($cpuExpected) { 'Verified' } else { 'Unavailable' }
        $expectedTargets = if ($cpuExpected) { 'Cpu' } else { '' }
        if ($backend.state -cne $expectedState -or (@($backend.targets) -join '|') -cne $expectedTargets) { throw "$Profile DNN capability evidence drifted for $($backend.backend)." }
    }
}

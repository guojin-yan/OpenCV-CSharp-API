param(
    [string]$OpenCvVersion = "5.0.0",
    [string]$WorkspaceRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path,
    [string]$OpenCvSourceRoot = "",
    [string]$OpenCvInstallRoot = "",
    [string]$OpenCvRuntimeVersionSuffix = "",
    [string]$Rid = "win-x64",
    [string]$Configuration = "Release",
    [string]$Generator = "",
    [string]$Platform = "",
    [string]$BuildList = "core,imgproc,imgcodecs,videoio,flann,geometry,calib,stereo,dnn,objdetect,photo,features,video,highgui,stitching,ptcloud",
    [string]$ExtraCMakeArgs = "",
    [string]$EigenIncludePath = "",
    [string]$RuntimePackageMatrix = "packaging/runtime/runtime-package-matrix.json",
    [string]$AndroidNdkRoot = "",
    [string]$AndroidApiLevel = "24",
    [switch]$WithContrib,
    [switch]$Build,
    [switch]$DescribeOnly
)

$ErrorActionPreference = "Stop"
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path

function Invoke-CheckedCommand {
    param(
        [Parameter(Mandatory = $true)]
        [string]$FilePath,
        [Parameter(ValueFromRemainingArguments = $true)]
        [string[]]$Arguments
    )

    & $FilePath @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "$FilePath failed with exit code $LASTEXITCODE."
    }
}

function Add-BuildListModule {
    param(
        [Parameter(Mandatory = $true)]
        [string]$CurrentBuildList,
        [Parameter(Mandatory = $true)]
        [string]$ModuleName
    )

    $modules = @($CurrentBuildList.Split(",", [System.StringSplitOptions]::RemoveEmptyEntries) | ForEach-Object { $_.Trim() })
    if ($modules -notcontains $ModuleName) {
        $modules += $ModuleName
    }

    return [string]::Join(",", $modules)
}

function Get-DefaultOpenCvSourceRoot {
    param(
        [Parameter(Mandatory = $true)]
        [string]$WorkspaceRoot,
        [Parameter(Mandatory = $true)]
        [string]$OpenCvVersion
    )

    # Prefer the version-neutral workspace source root when it exists.
    $neutralSourceRoot = Join-Path $WorkspaceRoot "opencv-source"
    if (Test-Path $neutralSourceRoot) {
        return $neutralSourceRoot
    }

    $versionMatch = [regex]::Match($OpenCvVersion, "^(\d+)(?:\.|$)")
    if (-not $versionMatch.Success) {
        throw "OpenCvVersion must start with a numeric major version: $OpenCvVersion"
    }

    # Use the major-version source directory only when an existing local checkout still uses that older layout.
    $legacyMajorSourceRoot = Join-Path $WorkspaceRoot "opencv$($versionMatch.Groups[1].Value)-source code"
    if (Test-Path $legacyMajorSourceRoot) {
        return $legacyMajorSourceRoot
    }

    return $neutralSourceRoot
}

function Get-RuntimeMatrix {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RepositoryRoot,
        [Parameter(Mandatory = $true)]
        [string]$MatrixPath
    )

    $matrixCandidate = if ([System.IO.Path]::IsPathRooted($MatrixPath)) {
        $MatrixPath
    }
    else {
        Join-Path $RepositoryRoot $MatrixPath
    }

    if (-not (Test-Path -LiteralPath $matrixCandidate -PathType Leaf)) {
        throw "Runtime package matrix was not found: $matrixCandidate"
    }

    return Get-Content -LiteralPath $matrixCandidate -Raw | ConvertFrom-Json
}

function Get-AndroidAbi {
    param([Parameter(Mandatory = $true)][string]$PackageRid)

    switch ($PackageRid) {
        "android-arm64" { return "arm64-v8a" }
        "android-arm" { return "armeabi-v7a" }
        "android-x64" { return "x86_64" }
        "android-x86" { return "x86" }
        default { return "" }
    }
}

function Get-WindowsOpenCvArchFolder {
    param([Parameter(Mandatory = $true)][string]$PackageRid)

    switch ($PackageRid) {
        "win-x64" { return "x64" }
        "win-x86" { return "x86" }
        "win-arm64" { return "ARM64" }
        default { return "" }
    }
}

function Get-OpenCvBuildTarget {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RequestedRid,
        [Parameter(Mandatory = $true)]
        [object]$Matrix,
        [Parameter(Mandatory = $true)]
        [string]$Configuration,
        [Parameter(Mandatory = $true)]
        [string]$OpenCvVersion,
        [string]$GeneratorOverride = "",
        [string]$PlatformOverride = ""
    )

    $ridSpec = @($Matrix.rids | Where-Object {
            $_.rid -eq $RequestedRid -or $_.opencvRid -eq $RequestedRid
        } | Select-Object -First 1)
    if ($ridSpec.Count -eq 0) {
        throw "RID '$RequestedRid' was not found in runtime package matrix."
    }

    $ridDefinition = $ridSpec[0]
    $packageRid = [string]$ridDefinition.rid
    $openCvRid = [string]$ridDefinition.opencvRid
    $platformFamily = if (
        $null -ne $ridDefinition.PSObject.Properties["platformFamily"] -and
        -not [string]::IsNullOrWhiteSpace([string]$ridDefinition.platformFamily)) {
        [string]$ridDefinition.platformFamily
    }
    elseif ($packageRid.StartsWith("win-", [System.StringComparison]::OrdinalIgnoreCase)) {
        "windows"
    }
    elseif ($packageRid.StartsWith("linux-", [System.StringComparison]::OrdinalIgnoreCase)) {
        "linux"
    }
    elseif ($packageRid.StartsWith("android-", [System.StringComparison]::OrdinalIgnoreCase)) {
        "android"
    }
    else {
        throw "RID '$packageRid' is not mapped to a real OpenCV build target family."
    }

    $platformFamily = $platformFamily.ToLowerInvariant()
    if (@("windows", "linux", "android") -notcontains $platformFamily) {
        throw "RID '$packageRid' has unsupported platformFamily '$platformFamily'."
    }

    $resolvedGenerator = $GeneratorOverride
    $resolvedPlatform = $PlatformOverride
    $buildSystem = "single-config"
    $installTarget = "install"
    $androidAbi = ""
    $requiresAndroidNdk = $false
    $openCvMajorMatch = [regex]::Match($OpenCvVersion, "^(\d+)(?:\.|$)")
    if (-not $openCvMajorMatch.Success) {
        throw "OpenCvVersion must start with a numeric major version: $OpenCvVersion"
    }

    $openCvMajor = $openCvMajorMatch.Groups[1].Value
    $openCvConfigCandidates = @("lib/OpenCVConfig.cmake", "OpenCVConfig.cmake")
    $runtimeDirCandidates = @("bin", "lib")

    if ($platformFamily -eq "windows") {
        if ([string]::IsNullOrWhiteSpace($resolvedGenerator)) {
            $resolvedGenerator = "Visual Studio 18 2026"
        }

        if ([string]::IsNullOrWhiteSpace($resolvedPlatform)) {
            $resolvedPlatform = switch ($packageRid) {
                "win-x64" { "x64" }
                "win-x86" { "Win32" }
                "win-arm64" { "ARM64" }
            }
        }

        $archFolder = Get-WindowsOpenCvArchFolder -PackageRid $packageRid
        $buildSystem = "multi-config"
        $installTarget = "INSTALL"
        $openCvConfigCandidates = @(
            "$archFolder/vc18/lib/OpenCVConfig.cmake",
            "lib/OpenCVConfig.cmake",
            "OpenCVConfig.cmake"
        )
        $runtimeDirCandidates = @(
            "$archFolder/vc18/bin",
            "$archFolder/vc18/bin/$Configuration",
            "bin",
            "bin/$Configuration"
        )
    }
    elseif ($platformFamily -eq "linux") {
        if ([string]::IsNullOrWhiteSpace($resolvedGenerator)) {
            $resolvedGenerator = "Ninja"
        }

        $openCvConfigCandidates = @(
            "lib/cmake/opencv$openCvMajor/OpenCVConfig.cmake",
            "lib64/cmake/opencv$openCvMajor/OpenCVConfig.cmake",
            "lib/OpenCVConfig.cmake",
            "lib64/OpenCVConfig.cmake",
            "OpenCVConfig.cmake"
        )
        $runtimeDirCandidates = @("lib", "lib64", "bin")
    }
    elseif ($platformFamily -eq "android") {
        if ([string]::IsNullOrWhiteSpace($resolvedGenerator)) {
            $resolvedGenerator = "Ninja"
        }

        $androidAbi = Get-AndroidAbi -PackageRid $packageRid
        $requiresAndroidNdk = $true
        $openCvConfigCandidates = @(
            "sdk/native/jni/OpenCVConfig.cmake",
            "lib/$androidAbi/OpenCVConfig.cmake",
            "OpenCVConfig.cmake"
        )
        $runtimeDirCandidates = @(
            "sdk/native/libs/$androidAbi",
            "lib/$androidAbi",
            "lib"
        )
    }

    return [pscustomobject]@{
        PackageRid = $packageRid
        OpenCvRid = $openCvRid
        PlatformFamily = $platformFamily
        Generator = $resolvedGenerator
        Platform = $resolvedPlatform
        BuildSystem = $buildSystem
        InstallTarget = $installTarget
        AndroidAbi = $androidAbi
        RequiresAndroidNdk = $requiresAndroidNdk
        OpenCvConfigCandidates = @($openCvConfigCandidates)
        RuntimeDirCandidates = @($runtimeDirCandidates)
    }
}

$runtimeMatrix = Get-RuntimeMatrix -RepositoryRoot $repoRoot -MatrixPath $RuntimePackageMatrix
$buildTarget = Get-OpenCvBuildTarget `
    -RequestedRid $Rid `
    -Matrix $runtimeMatrix `
    -Configuration $Configuration `
    -OpenCvVersion $OpenCvVersion `
    -GeneratorOverride $Generator `
    -PlatformOverride $Platform
$Rid = $buildTarget.PackageRid
$openCvRid = $buildTarget.OpenCvRid
$Generator = $buildTarget.Generator
$Platform = $buildTarget.Platform

if ([string]::IsNullOrWhiteSpace($OpenCvSourceRoot)) {
    $OpenCvSourceRoot = Get-DefaultOpenCvSourceRoot -WorkspaceRoot $WorkspaceRoot -OpenCvVersion $OpenCvVersion
}

if ([string]::IsNullOrWhiteSpace($OpenCvInstallRoot)) {
    $OpenCvInstallRoot = Join-Path $WorkspaceRoot "artifacts\opencv-install"
}

if ([string]::IsNullOrWhiteSpace($OpenCvRuntimeVersionSuffix)) {
    # The suffix carries factual local build/install artifact identity, not a package ID or generic project naming surface.
    $OpenCvRuntimeVersionSuffix = "$OpenCvVersion-$openCvRid"
}

# Upstream source leaf directories include the selected OpenCV version as factual source artifact identity.
$opencvSource = Join-Path $OpenCvSourceRoot "opencv-$OpenCvVersion"
$contribSource = Join-Path $OpenCvSourceRoot "opencv_contrib-$OpenCvVersion"
$depsRoot = Join-Path $WorkspaceRoot "artifacts\deps"
$buildRoot = Join-Path $WorkspaceRoot "artifacts\opencv-build"
# Local OpenCV build/install leaf directories include the selected runtime identity as factual artifact metadata only.
$installRoot = Join-Path $OpenCvInstallRoot "opencv-$OpenCvRuntimeVersionSuffix"
$buildDir = Join-Path $buildRoot "opencv-$OpenCvRuntimeVersionSuffix"

$cmakeArgs = @(
    "-S", $opencvSource,
    "-B", $buildDir,
    "-G", $Generator,
    "-DCMAKE_INSTALL_PREFIX=$installRoot",
    "-DBUILD_SHARED_LIBS=ON",
    "-DBUILD_TESTS=OFF",
    "-DBUILD_PERF_TESTS=OFF",
    "-DBUILD_EXAMPLES=OFF",
    "-DBUILD_DOCS=OFF",
    "-DBUILD_opencv_apps=OFF",
    "-DBUILD_JAVA=OFF",
    "-DBUILD_opencv_python_bindings_generator=OFF",
    "-DBUILD_opencv_python_tests=OFF",
    "-DWITH_IPP=OFF",
    "-DWITH_OPENCL=OFF",
    "-DWITH_FFMPEG=OFF"
)

if (-not [string]::IsNullOrWhiteSpace($Platform)) {
    $cmakeArgs += @("-A", $Platform)
}

if ($buildTarget.BuildSystem -eq "multi-config") {
    $cmakeArgs += "-DCMAKE_CONFIGURATION_TYPES=$Configuration"
}
else {
    $cmakeArgs += "-DCMAKE_BUILD_TYPE=$Configuration"
}

if ($buildTarget.PlatformFamily -eq "windows") {
    $cmakeArgs += @("-DWITH_MSMF=OFF", "-DWITH_DSHOW=OFF")
}

if ($buildTarget.PlatformFamily -eq "android") {
    if ([string]::IsNullOrWhiteSpace($AndroidNdkRoot)) {
        if (-not $DescribeOnly) {
            throw "Android OpenCV builds require -AndroidNdkRoot for RID '$Rid'. Use -DescribeOnly to inspect the required plan without configuring."
        }

        $AndroidNdkRoot = "<ANDROID_NDK_ROOT>"
    }

    $androidToolchainFile = Join-Path $AndroidNdkRoot "build\cmake\android.toolchain.cmake"
    if (-not $DescribeOnly -and -not (Test-Path -LiteralPath $androidToolchainFile -PathType Leaf)) {
        throw "Android NDK toolchain file was not found: $androidToolchainFile"
    }

    $cmakeArgs += @(
        "-DCMAKE_TOOLCHAIN_FILE=$androidToolchainFile",
        "-DANDROID_ABI=$($buildTarget.AndroidAbi)",
        "-DANDROID_PLATFORM=android-$AndroidApiLevel"
    )
}

if ($WithContrib) {
    if (-not (Test-Path $contribSource)) {
        if (-not $DescribeOnly) {
            throw "OpenCV contrib source directory was not found: $contribSource"
        }
    }

    if ([string]::IsNullOrWhiteSpace($EigenIncludePath)) {
        $eigenCandidates = @(
            (Join-Path $depsRoot "eigen-3.4.0"),
            (Join-Path $depsRoot "eigen-3.4.0\eigen-3.4.0"),
            (Join-Path $depsRoot "eigen"),
            (Join-Path $depsRoot "eigen\eigen")
        )

        foreach ($candidate in $eigenCandidates) {
            if (Test-Path (Join-Path $candidate "Eigen\Core")) {
                $EigenIncludePath = $candidate
                break
            }
        }
    }

    if (-not [string]::IsNullOrWhiteSpace($EigenIncludePath)) {
        if (-not (Test-Path (Join-Path $EigenIncludePath "Eigen\Core"))) {
            throw "Eigen include path does not contain Eigen\Core: $EigenIncludePath"
        }

        $resolvedEigenIncludePath = (Resolve-Path $EigenIncludePath).Path.Replace("\", "/")
        $cmakeArgs += "-DEIGEN_INCLUDE_PATH=$resolvedEigenIncludePath"
        Write-Host "Eigen include path: $resolvedEigenIncludePath"
    }
    else {
        Write-Warning "Eigen headers were not found under artifacts\deps. OpenCV alphamat will be disabled unless Eigen is available through the system CMake search path."
    }

    $BuildList = Add-BuildListModule -CurrentBuildList $BuildList -ModuleName "xfeatures2d"
    $BuildList = Add-BuildListModule -CurrentBuildList $BuildList -ModuleName "xobjdetect"
    $BuildList = Add-BuildListModule -CurrentBuildList $BuildList -ModuleName "quality"
    $BuildList = Add-BuildListModule -CurrentBuildList $BuildList -ModuleName "xphoto"
    $BuildList = Add-BuildListModule -CurrentBuildList $BuildList -ModuleName "ml"
    $BuildList = Add-BuildListModule -CurrentBuildList $BuildList -ModuleName "img_hash"
    $BuildList = Add-BuildListModule -CurrentBuildList $BuildList -ModuleName "ximgproc"
    $BuildList = Add-BuildListModule -CurrentBuildList $BuildList -ModuleName "optflow"
    $BuildList = Add-BuildListModule -CurrentBuildList $BuildList -ModuleName "bgsegm"
    $BuildList = Add-BuildListModule -CurrentBuildList $BuildList -ModuleName "tracking"
    $BuildList = Add-BuildListModule -CurrentBuildList $BuildList -ModuleName "face"
    $BuildList = Add-BuildListModule -CurrentBuildList $BuildList -ModuleName "saliency"
    $BuildList = Add-BuildListModule -CurrentBuildList $BuildList -ModuleName "plot"
    $BuildList = Add-BuildListModule -CurrentBuildList $BuildList -ModuleName "shape"
    $BuildList = Add-BuildListModule -CurrentBuildList $BuildList -ModuleName "line_descriptor"
    $BuildList = Add-BuildListModule -CurrentBuildList $BuildList -ModuleName "phase_unwrapping"
    $BuildList = Add-BuildListModule -CurrentBuildList $BuildList -ModuleName "structured_light"
    $BuildList = Add-BuildListModule -CurrentBuildList $BuildList -ModuleName "intensity_transform"
    $BuildList = Add-BuildListModule -CurrentBuildList $BuildList -ModuleName "fuzzy"
    $BuildList = Add-BuildListModule -CurrentBuildList $BuildList -ModuleName "hfs"
    $BuildList = Add-BuildListModule -CurrentBuildList $BuildList -ModuleName "reg"
    $BuildList = Add-BuildListModule -CurrentBuildList $BuildList -ModuleName "surface_matching"
    $BuildList = Add-BuildListModule -CurrentBuildList $BuildList -ModuleName "rapid"
    $BuildList = Add-BuildListModule -CurrentBuildList $BuildList -ModuleName "alphamat"
    $BuildList = Add-BuildListModule -CurrentBuildList $BuildList -ModuleName "bioinspired"
    $BuildList = Add-BuildListModule -CurrentBuildList $BuildList -ModuleName "xstereo"
    $cmakeArgs += "-DOPENCV_EXTRA_MODULES_PATH=$(Join-Path $contribSource "modules")"
}

$cmakeArgs += "-DBUILD_LIST=$BuildList"

if (-not [string]::IsNullOrWhiteSpace($ExtraCMakeArgs)) {
    $cmakeArgs += $ExtraCMakeArgs.Split(" ", [System.StringSplitOptions]::RemoveEmptyEntries)
}

$expectedConfigCandidates = @($buildTarget.OpenCvConfigCandidates | ForEach-Object {
        Join-Path $installRoot $_
    })
$expectedConfigCandidates += (Join-Path $buildDir "OpenCVConfig.cmake")
$expectedRuntimeDirCandidates = @($buildTarget.RuntimeDirCandidates | ForEach-Object {
        Join-Path $installRoot $_
    })

if ($DescribeOnly) {
    [pscustomobject]@{
        PackageRid = $buildTarget.PackageRid
        OpenCvRid = $buildTarget.OpenCvRid
        PlatformFamily = $buildTarget.PlatformFamily
        Generator = $Generator
        Platform = $Platform
        BuildSystem = $buildTarget.BuildSystem
        InstallTarget = $buildTarget.InstallTarget
        AndroidAbi = $buildTarget.AndroidAbi
        RequiresAndroidNdk = [bool]$buildTarget.RequiresAndroidNdk
        AndroidApiLevel = if ($buildTarget.PlatformFamily -eq "android") { $AndroidApiLevel } else { "" }
        OpenCvVersion = $OpenCvVersion
        RuntimeVersionSuffix = $OpenCvRuntimeVersionSuffix
        BuildList = $BuildList
        Source = $opencvSource
        ContribSource = $contribSource
        BuildDir = $buildDir
        InstallRoot = $installRoot
        ExpectedOpenCvConfigCMake = @($expectedConfigCandidates)
        ExpectedRuntimeDirs = @($expectedRuntimeDirCandidates)
        CMakeArgs = @($cmakeArgs)
    } | ConvertTo-Json -Depth 6
    return
}

if (-not (Test-Path $opencvSource)) {
    throw "OpenCV source directory was not found: $opencvSource"
}

New-Item -ItemType Directory -Force $buildDir | Out-Null
New-Item -ItemType Directory -Force $installRoot | Out-Null

Write-Host "Configuring OpenCV $OpenCvVersion"
Write-Host "RID:     $Rid ($($buildTarget.OpenCvRid))"
Write-Host "Target:  $($buildTarget.PlatformFamily)"
Write-Host "Source:  $opencvSource"
Write-Host "Build:   $buildDir"
Write-Host "Install: $installRoot"
Write-Host "Modules: $BuildList"
Write-Host "Expected OpenCVConfig.cmake candidates:"
foreach ($candidate in $expectedConfigCandidates) {
    Write-Host " - $candidate"
}
Write-Host "Expected runtime directory candidates:"
foreach ($candidate in $expectedRuntimeDirCandidates) {
    Write-Host " - $candidate"
}

Invoke-CheckedCommand cmake @cmakeArgs

if ($Build) {
    Write-Host "Building OpenCV $OpenCvVersion ($Configuration)"
    Invoke-CheckedCommand cmake --build $buildDir --config $Configuration --target $buildTarget.InstallTarget
}

foreach ($candidate in $expectedConfigCandidates) {
    if (Test-Path $candidate) {
        Write-Host "OpenCVConfig.cmake: $candidate"
        return
    }
}

Write-Warning "OpenCVConfig.cmake was not found yet. Run with -Build to install OpenCV."

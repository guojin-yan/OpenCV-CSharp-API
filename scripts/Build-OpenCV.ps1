param(
    [string]$OpenCvVersion = "5.0.0",
    [string]$WorkspaceRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path,
    [string]$OpenCvSourceRoot = "",
    [string]$OpenCvInstallRoot = "",
    [string]$OpenCvRuntimeVersionSuffix = "",
    [string]$Rid = "windows-x64",
    [string]$Configuration = "Release",
    [string]$Generator = "Visual Studio 18 2026",
    [string]$Platform = "x64",
    [string]$BuildList = "core,imgproc,imgcodecs,videoio,flann,geometry,calib,stereo,dnn,objdetect,photo,features,video,highgui,stitching,ptcloud",
    [string]$ExtraCMakeArgs = "",
    [string]$EigenIncludePath = "",
    [switch]$WithContrib,
    [switch]$Build
)

$ErrorActionPreference = "Stop"

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

if ([string]::IsNullOrWhiteSpace($OpenCvSourceRoot)) {
    $OpenCvSourceRoot = Get-DefaultOpenCvSourceRoot -WorkspaceRoot $WorkspaceRoot -OpenCvVersion $OpenCvVersion
}

if ([string]::IsNullOrWhiteSpace($OpenCvInstallRoot)) {
    $OpenCvInstallRoot = Join-Path $WorkspaceRoot "artifacts\opencv-install"
}

if ([string]::IsNullOrWhiteSpace($OpenCvRuntimeVersionSuffix)) {
    # The suffix carries factual local build/install artifact identity, not a package ID or generic project naming surface.
    $OpenCvRuntimeVersionSuffix = "$OpenCvVersion-$Rid"
}

# Upstream source leaf directories include the selected OpenCV version as factual source artifact identity.
$opencvSource = Join-Path $OpenCvSourceRoot "opencv-$OpenCvVersion"
$contribSource = Join-Path $OpenCvSourceRoot "opencv_contrib-$OpenCvVersion"
$depsRoot = Join-Path $WorkspaceRoot "artifacts\deps"
$buildRoot = Join-Path $WorkspaceRoot "artifacts\opencv-build"
# Local OpenCV build/install leaf directories include the selected runtime identity as factual artifact metadata only.
$installRoot = Join-Path $OpenCvInstallRoot "opencv-$OpenCvRuntimeVersionSuffix"
$buildDir = Join-Path $buildRoot "opencv-$OpenCvRuntimeVersionSuffix"

if (-not (Test-Path $opencvSource)) {
    throw "OpenCV source directory was not found: $opencvSource"
}

New-Item -ItemType Directory -Force $buildDir | Out-Null
New-Item -ItemType Directory -Force $installRoot | Out-Null

$cmakeArgs = @(
    "-S", $opencvSource,
    "-B", $buildDir,
    "-G", $Generator,
    "-A", $Platform,
    "-DCMAKE_INSTALL_PREFIX=$installRoot",
    "-DCMAKE_CONFIGURATION_TYPES=$Configuration",
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
    "-DWITH_FFMPEG=OFF",
    "-DWITH_MSMF=OFF",
    "-DWITH_DSHOW=OFF"
)

if ($WithContrib) {
    if (-not (Test-Path $contribSource)) {
        throw "OpenCV contrib source directory was not found: $contribSource"
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

Write-Host "Configuring OpenCV $OpenCvVersion"
Write-Host "Source:  $opencvSource"
Write-Host "Build:   $buildDir"
Write-Host "Install: $installRoot"
Write-Host "Modules: $BuildList"
Write-Host "Expected OpenCVConfig.cmake: $(Join-Path $installRoot "x64\vc18\lib\OpenCVConfig.cmake")"
Write-Host "Expected runtime bin:      $(Join-Path $installRoot "x64\vc18\bin")"

Invoke-CheckedCommand cmake @cmakeArgs

if ($Build) {
    Write-Host "Building OpenCV $OpenCvVersion ($Configuration)"
    Invoke-CheckedCommand cmake --build $buildDir --config $Configuration --target INSTALL
}

$configCandidates = @(
    (Join-Path $installRoot "x64\vc18\lib\OpenCVConfig.cmake"),
    (Join-Path $installRoot "lib\OpenCVConfig.cmake"),
    (Join-Path $buildDir "OpenCVConfig.cmake")
)

foreach ($candidate in $configCandidates) {
    if (Test-Path $candidate) {
        Write-Host "OpenCVConfig.cmake: $candidate"
        return
    }
}

Write-Warning "OpenCVConfig.cmake was not found yet. Run with -Build to install OpenCV."

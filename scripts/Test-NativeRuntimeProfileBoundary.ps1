param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$cmakePath = Join-Path $repo "src/OpenCvSharp.Native/CMakeLists.txt"
$nativeSourceRoot = Join-Path $repo "src/OpenCvSharp.Native/src"
$imgprocSourcePath = Join-Path $nativeSourceRoot "imgproc.cpp"
$nativeSmokePath = Join-Path $repo "src/OpenCvSharp.Native/tests/native_smoke.cpp"
$generatorPath = Join-Path $repo "scripts/Generate-NativeAbiCompatibility.ps1"
$buildNativePath = Join-Path $repo "scripts/Build-Native.ps1"
$workflowPath = Join-Path $repo ".github/workflows/runtime-input.yml"
$matrixPath = Join-Path $repo "packaging/runtime/runtime-package-matrix.json"
$miniAbiPath = Join-Path $repo "src/OpenCvSharp.Native/generated/legacy_abi_mini.cpp"
$miniManifestPath = Join-Path $repo "src/OpenCvSharp.Native/generated/legacy_abi_mini_manifest.txt"
$fullManifestPath = Join-Path $repo "src/OpenCvSharp.Native/generated/legacy_abi_manifest.txt"

$requiredPaths = @(
    $cmakePath,
    $nativeSourceRoot,
    $imgprocSourcePath,
    $nativeSmokePath,
    $generatorPath,
    $buildNativePath,
    $workflowPath,
    $matrixPath,
    $miniAbiPath,
    $miniManifestPath,
    $fullManifestPath
)
foreach ($path in $requiredPaths) {
    if (-not (Test-Path -LiteralPath $path)) {
        throw "Required native runtime profile path was not found: $path"
    }
}

$violations = [System.Collections.Generic.List[string]]::new()
function Add-Violation {
    param([Parameter(Mandatory)][string]$Message)
    $violations.Add($Message)
}

function Assert-Contains {
    param(
        [Parameter(Mandatory)][string]$Text,
        [Parameter(Mandatory)][string]$Needle,
        [Parameter(Mandatory)][string]$Description
    )

    if (-not $Text.Contains($Needle)) {
        Add-Violation "$Description (missing: $Needle)"
    }
}

function Get-CMakeSourceList {
    param(
        [Parameter(Mandatory)][string]$Text,
        [Parameter(Mandatory)][string]$VariableName
    )

    $pattern = "(?ms)^set\($([regex]::Escape($VariableName))\s*(?<body>.*?)^\)\s*$"
    $match = [regex]::Match($Text, $pattern)
    if (-not $match.Success) {
        Add-Violation "CMake source list was not found: $VariableName"
        return @()
    }

    return @(
        [regex]::Matches($match.Groups["body"].Value, "(?m)^\s*(src/[A-Za-z0-9_./-]+\.cpp)\s*$") |
            ForEach-Object { $_.Groups[1].Value }
    )
}

function Get-ManifestRows {
    param([Parameter(Mandatory)][string]$Path)

    return @(
        Get-Content -LiteralPath $Path |
            Where-Object { $_ -and -not $_.StartsWith("#") -and -not $_.StartsWith("[") -and $_.Contains("|") } |
            ForEach-Object {
                $parts = $_.Split("|")
                if ($parts.Count -ne 5) {
                    Add-Violation "Malformed ABI manifest row in $Path`: $_"
                    return
                }

                [pscustomobject]@{
                    Primary = $parts[0]
                    Legacy = $parts[1]
                    ReturnType = $parts[2]
                    ParameterCount = [int]$parts[3]
                    Header = $parts[4]
                }
            }
    )
}

$matrix = Get-Content -LiteralPath $matrixPath -Raw | ConvertFrom-Json
$miniProfile = @($matrix.profiles | Where-Object name -eq "mini")
$fullProfile = @($matrix.profiles | Where-Object name -eq "full")
if ($miniProfile.Count -ne 1 -or $fullProfile.Count -ne 1) {
    Add-Violation "Runtime matrix must contain exactly one full and one mini profile."
}
else {
    $expectedMiniModules = @("core", "imgproc", "imgcodecs", "videoio", "geometry", "flann")
    if ([string]$miniProfile[0].buildList -cne ($expectedMiniModules -join ",")) {
        Add-Violation "Mini runtime buildList must be exactly $($expectedMiniModules -join ',')."
    }
    if ((Compare-Object -ReferenceObject $expectedMiniModules -DifferenceObject @($miniProfile[0].modules) -SyncWindow 0)) {
        Add-Violation "Mini runtime modules must remain exactly core,imgproc,imgcodecs,videoio,geometry,flann in order."
    }
    if (@($miniProfile[0].optionalModules).Count -ne 0) {
        Add-Violation "Mini runtime profile must not declare optional full-only modules."
    }
}

$cmakeText = [System.IO.File]::ReadAllText($cmakePath)
$generatorText = [System.IO.File]::ReadAllText($generatorPath)
$buildNativeText = [System.IO.File]::ReadAllText($buildNativePath)
$workflowText = [System.IO.File]::ReadAllText($workflowPath)
$miniAbiText = [System.IO.File]::ReadAllText($miniAbiPath)
$imgprocSourceText = [System.IO.File]::ReadAllText($imgprocSourcePath)
$nativeSmokeText = [System.IO.File]::ReadAllText($nativeSmokePath)

foreach ($expectation in @(
        @($cmakeText, 'set(OPENCV_CSHARP_RUNTIME_PROFILE "full" CACHE STRING', "CMake must expose a version-neutral runtime profile input"),
        @($cmakeText, 'set_property(CACHE OPENCV_CSHARP_RUNTIME_PROFILE PROPERTY STRINGS full mini)', "CMake must constrain runtime profile values"),
        @($cmakeText, 'set(OPENCV_CSHARP_MINI_OPENCV_BUILD_LIST "core,imgproc,imgcodecs,videoio,geometry,flann")', "CMake must pin the mini OpenCV build list including transitive runtime dependencies"),
        @($cmakeText, 'find_package(OpenCV REQUIRED COMPONENTS core imgproc imgcodecs videoio geometry flann)', "Mini CMake must require only mini OpenCV components plus the OpenCV 5 geometry/flann dependency chain"),
        @($cmakeText, 'OPENCV_CSHARP_HAS_OPENCV_GEOMETRY=1', "Mini/full CMake must expose OpenCV 5 geometry-backed imgproc APIs"),
        @($cmakeText, 'OPENCV_CSHARP_HAS_OPENCV_FEATURES=1', "Full CMake must explicitly expose OpenCV 5 features-backed imgproc APIs"),
        @($cmakeText, 'set(OPENCV_CSHARP_NATIVE_ABI_SOURCE generated/legacy_abi_mini.cpp)', "Mini CMake must select the reduced compatibility ABI"),
        @($cmakeText, 'set(OPENCV_CSHARP_NATIVE_ABI_MANIFEST generated/legacy_abi_mini_manifest.txt)', "Mini CTest must select the reduced export manifest"),
        @($cmakeText, 'BUILD_WITH_INSTALL_RPATH TRUE', "Linked Linux native wrapper must use its package runtime RPATH during the producer build"),
        @($cmakeText, 'INSTALL_RPATH "\$ORIGIN"', "Linked Linux native wrapper must resolve packaged OpenCV dependencies beside the loader"),
        @($cmakeText, 'target_link_options(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE "LINKER:--no-as-needed")', "Linked Linux mini wrapper must keep the six-module runtime closure as direct loader dependencies"),
        @($cmakeText, 'LD_LIBRARY_PATH=${OPENCV_CSHARP_OPENCV_RUNTIME_DIRECTORY}', "Producer CTest must resolve OpenCV from the factual install tree without changing the packaged loader RPATH"),
        @($generatorText, 'generated/legacy_abi_mini.cpp', "ABI generator must own the mini forwarding unit"),
        @($generatorText, 'generated/legacy_abi_mini_manifest.txt', "ABI generator must own the mini manifest"),
        @($buildNativeText, '[ValidateSet("full", "mini")]', "Build-Native must expose full/mini profiles"),
        @($buildNativeText, '"-DOPENCV_CSHARP_RUNTIME_PROFILE=$RuntimeProfile"', "Build-Native must pass runtime profile to CMake"),
        @($workflowText, "'ubuntu.24.04-x64/mini'", "Runtime producer must explicitly allow the first mini target"),
        @($workflowText, 'profile: mini', "Runtime producer matrix must include a mini row"),
        @($workflowText, '"-DOPENCV_CSHARP_RUNTIME_PROFILE=${{ matrix.profile }}"', "Hosted producer must pass runtime profile to CMake"),
        @($workflowText, '"-DOPENCV_CSHARP_RUNTIME_PROFILE=$RUNTIME_PROFILE"', "Container producer must pass runtime profile to CMake"))) {
    Assert-Contains -Text $expectation[0] -Needle $expectation[1] -Description $expectation[2]
}

Assert-Contains -Text $imgprocSourceText -Needle '#if defined(OPENCV_CSHARP_HAS_OPENCV_GEOMETRY)' -Description "imgproc wrapper must guard OpenCV 5 geometry-backed calls"
Assert-Contains -Text $imgprocSourceText -Needle '#if defined(OPENCV_CSHARP_HAS_OPENCV_FEATURES)' -Description "imgproc wrapper must guard OpenCV 5 features-backed calls"
Assert-Contains -Text $nativeSmokeText -Needle 'run_mini_excluded_features_smoke' -Description "Native smoke must verify the mini excluded-features boundary"
Assert-Contains -Text $nativeSmokeText -Needle 'status != OPENCV_CSHARP_STATUS_NOT_LINKED' -Description "Mini smoke must require NOT_LINKED for excluded features APIs"
if ($imgprocSourceText.Contains('__has_include(<opencv2/features.hpp>)')) {
    Add-Violation "imgproc wrapper must not infer features linkage from header presence; CMake target linkage is authoritative."
}

$miniSources = @(Get-CMakeSourceList -Text $cmakeText -VariableName "OPENCV_CSHARP_MINI_NATIVE_SOURCES")
$fullOnlySources = @(Get-CMakeSourceList -Text $cmakeText -VariableName "OPENCV_CSHARP_FULL_ONLY_NATIVE_SOURCES")
$expectedMiniSources = @(
    "src/error_state.cpp",
    "src/version.cpp",
    "src/core/mat.cpp",
    "src/core/decomp.cpp",
    "src/core/operations.cpp",
    "src/videoio/videoio.cpp",
    "src/imgcodecs.cpp",
    "src/imgproc.cpp"
)
if (Compare-Object -ReferenceObject $expectedMiniSources -DifferenceObject $miniSources) {
    Add-Violation "CMake mini native sources must remain common infrastructure plus core/imgproc/imgcodecs/videoio wrappers; geometry and flann are OpenCV dependencies without separate wrapper sources."
}

$diskSources = @(
    Get-ChildItem -LiteralPath $nativeSourceRoot -Recurse -File -Filter "*.cpp" |
        ForEach-Object {
            "src/" + [System.IO.Path]::GetRelativePath($nativeSourceRoot, $_.FullName).Replace("\", "/")
        } |
        Sort-Object
)
$configuredFullSources = @($miniSources + $fullOnlySources | Sort-Object -Unique)
if (Compare-Object -ReferenceObject $diskSources -DifferenceObject $configuredFullSources) {
    Add-Violation "Full CMake source union must include every native source file exactly once."
}
if (@($miniSources | Where-Object { $_ -in $fullOnlySources }).Count -gt 0) {
    Add-Violation "Mini and full-only native source lists must not overlap."
}

$miniRows = @(Get-ManifestRows -Path $miniManifestPath)
$fullRows = @(Get-ManifestRows -Path $fullManifestPath)
$miniIdentifierCountLine = @(Get-Content -LiteralPath $miniManifestPath | Where-Object { $_.StartsWith("identifier-count=") } | Select-Object -First 1)
$fullIdentifierCountLine = @(Get-Content -LiteralPath $fullManifestPath | Where-Object { $_.StartsWith("identifier-count=") } | Select-Object -First 1)
$allowedMiniHeaders = @(
    "core/decomp.h",
    "core/mat.h",
    "core/operations.h",
    "error.h",
    "imgcodecs.h",
    "imgproc.h",
    "version.h",
    "videoio/videoio.h"
)
if ($miniRows.Count -eq 0 -or $miniRows.Count -ge $fullRows.Count) {
    Add-Violation "Mini ABI manifest must be non-empty and smaller than the full manifest."
}
if ($miniIdentifierCountLine.Count -ne 1 -or $fullIdentifierCountLine.Count -ne 1 -or
    [int]$miniIdentifierCountLine[0].Substring("identifier-count=".Length) -ge [int]$fullIdentifierCountLine[0].Substring("identifier-count=".Length)) {
    Add-Violation "Mini ABI identifier count must be present and smaller than the full ABI identifier count."
}
$unexpectedMiniHeaders = @($miniRows.Header | Sort-Object -Unique | Where-Object { $_ -notin $allowedMiniHeaders })
if ($unexpectedMiniHeaders.Count -gt 0) {
    Add-Violation "Mini ABI manifest contains full-only headers: $($unexpectedMiniHeaders -join ', ')"
}
$missingManifestHeaders = @($allowedMiniHeaders | Where-Object { $_ -notin @($miniRows.Header | Sort-Object -Unique) })
if ($missingManifestHeaders.Count -gt 0) {
    Add-Violation "Mini ABI manifest is missing supported headers: $($missingManifestHeaders -join ', ')"
}

foreach ($row in $miniRows) {
    if (-not $miniAbiText.Contains("$($row.Legacy)(")) {
        Add-Violation "Mini compatibility ABI is missing legacy export $($row.Legacy)."
    }
    if (-not $miniAbiText.Contains("$($row.Primary)(")) {
        Add-Violation "Mini compatibility ABI is missing forwarding call $($row.Primary)."
    }
}

$fixedMajorDnnPrefix = "jyppx_ocv" + "5_dnn_" # compatibility ABI check; keep fixed-major token out of new project identity text
if ($miniAbiText.Contains('open_cv_sharp/dnn/dnn.h') -or $miniAbiText.Contains($fixedMajorDnnPrefix)) {
    Add-Violation "Mini compatibility ABI must not include or forward DNN entrypoints."
}

if ($violations.Count -gt 0) {
    Write-Host "Native runtime profile boundary guard failed with $($violations.Count) violation(s)."
    $violations | Sort-Object | ForEach-Object { Write-Host "- $_" }
    exit 1
}

Write-Host "Native runtime profile boundary guard passed."
Write-Host "Mini native sources: $($miniSources.Count); full native sources: $($configuredFullSources.Count)."
Write-Host "Mini ABI functions: $($miniRows.Count); full ABI functions: $($fullRows.Count)."

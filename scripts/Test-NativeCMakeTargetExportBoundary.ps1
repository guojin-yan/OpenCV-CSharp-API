param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$cmakePath = Join-Path $repo "src/OpenCvSharp.Native/CMakeLists.txt"
$buildScriptPath = Join-Path $repo "scripts/Build-Native.ps1"
$workflowPath = Join-Path $repo ".github/workflows/build-native.yml"
$violations = [System.Collections.Generic.List[string]]::new()

foreach ($path in @($cmakePath, $buildScriptPath, $workflowPath)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Required native build boundary file was not found: $path"
    }
}

$cmake = [System.IO.File]::ReadAllText($cmakePath)
$buildScript = [System.IO.File]::ReadAllText($buildScriptPath)
$workflow = [System.IO.File]::ReadAllText($workflowPath)

foreach ($forbiddenPattern in @(
        '(?im)^\s*install\s*\(',
        '(?im)^\s*export\s*\(',
        '(?i)configure_package_config_file\s*\(',
        '(?i)write_basic_package_version_file\s*\(',
        '(?i)CMakePackageConfigHelpers',
        '\$<INSTALL_INTERFACE:')) {
    if ($cmake -match $forbiddenPattern) {
        $violations.Add("Native CMake project must remain source-tree build only: $forbiddenPattern")
    }
}

foreach ($required in @(
        'set(OPENCV_CSHARP_NATIVE_TARGET "JYPPX.OpenCV.Native")',
        'add_library(${OPENCV_CSHARP_NATIVE_TARGET} SHARED',
        'OUTPUT_NAME "${OPENCV_CSHARP_NATIVE_TARGET}"',
        '"LINKER:-z,max-page-size=16384"',
        '"LINKER:-z,common-page-size=16384"')) {
    if (-not $cmake.Contains($required, [StringComparison]::Ordinal)) {
        $violations.Add("Native CMake boundary is missing: $required")
    }
}

foreach ($surface in @(
        [pscustomobject]@{ Name = "CMake"; Text = $cmake },
        [pscustomobject]@{ Name = "Build-Native"; Text = $buildScript },
        [pscustomobject]@{ Name = "build-native workflow"; Text = $workflow })) {
    if ($surface.Text -match 'OpenCv[0-9]+Sharp|OPENCV[0-9]+SHARP') {
        $violations.Add("$($surface.Name) contains a fixed-major native identity.")
    }
}

foreach ($required in @("OPENCV_CSHARP_OPENCV_DIR", "OPENCV_CSHARP_OPENCV_BUILD_LIST")) {
    if (-not $cmake.Contains($required, [StringComparison]::Ordinal) -or
        -not $workflow.Contains($required, [StringComparison]::Ordinal)) {
        $violations.Add("Native build and workflow must use $required.")
    }
}

if ($violations.Count -gt 0) {
    Write-Host "Native CMake target/export boundary guard failed with $($violations.Count) violation(s)."
    $violations | Sort-Object | ForEach-Object { Write-Host "- $_" }
    exit 1
}

Write-Host "Native CMake target/export boundary guard passed."
Write-Host "Native CMake target: JYPPX.OpenCV.Native."
Write-Host "CMake package install/export surface: absent."

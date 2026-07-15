param(
    [string]$Configuration = "Release",
    [string]$OpenCvDir = "",
    [string]$NativeWrapperSourceDir = "src\OpenCvSharp.Native",
    [string]$BuildDir = "build\native",
    [string]$Generator = "Visual Studio 18 2026",
    [string]$Platform = "x64",
    [ValidateSet("full", "mini")]
    [string]$RuntimeProfile = "full"
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

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
# NativeWrapperSourceDir is the version-neutral native wrapper source-path input.
# Its default is the version-neutral src\OpenCvSharp.Native source directory.
$nativeSourceCandidate = if ([System.IO.Path]::IsPathRooted($NativeWrapperSourceDir)) {
    $NativeWrapperSourceDir
}
else {
    Join-Path $repoRoot $NativeWrapperSourceDir
}

if (-not (Test-Path -LiteralPath $nativeSourceCandidate -PathType Container)) {
    throw "Native wrapper source directory was not found: $nativeSourceCandidate"
}

$nativeSource = (Resolve-Path -LiteralPath $nativeSourceCandidate).Path
# BuildDir is the version-neutral native build-output path input.
# Its default remains the existing build\native compatibility directory.
$nativeBuildCandidate = if ([System.IO.Path]::IsPathRooted($BuildDir)) {
    $BuildDir
}
else {
    Join-Path $repoRoot $BuildDir
}

$nativeBuild = [System.IO.Path]::GetFullPath($nativeBuildCandidate)
$runtimeMatrixPath = Join-Path $repoRoot "packaging/runtime/runtime-package-matrix.json"
$runtimeMatrix = Get-Content -LiteralPath $runtimeMatrixPath -Raw | ConvertFrom-Json
$profileSpec = @($runtimeMatrix.profiles | Where-Object { $_.name -eq $RuntimeProfile } | Select-Object -First 1)
if ($profileSpec.Count -eq 0) {
    throw "Runtime profile was not found in ${runtimeMatrixPath}: $RuntimeProfile"
}

$cmakeArgs = @(
    "-S", $nativeSource,
    "-B", $nativeBuild,
    "-G", $Generator,
    "-A", $Platform,
    "-DOPENCV_CSHARP_RUNTIME_PROFILE=$RuntimeProfile",
    "-DOPENCV_CSHARP_OPENCV_BUILD_LIST=$($profileSpec[0].buildList)"
)

if (-not [string]::IsNullOrWhiteSpace($OpenCvDir)) {
    # OPENCV_CSHARP_* variables are the current smoke-test/build environment knobs; fixed-major OPENCV5SHARP_* aliases are not primary build inputs here.
    $cmakeArgs += "-DOPENCV_CSHARP_OPENCV_DIR=$OpenCvDir"
}

Invoke-CheckedCommand cmake @cmakeArgs
Invoke-CheckedCommand cmake --build $nativeBuild --config $Configuration
Invoke-CheckedCommand ctest --test-dir $nativeBuild -C $Configuration --output-on-failure

Write-Host "Native runtime directory: $(Join-Path $nativeBuild $Configuration)"

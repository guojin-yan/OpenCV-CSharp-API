param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$cmakePath = Join-Path $repo "src/OpenCvSharp.Native/CMakeLists.txt"
$smokePath = Join-Path $repo "src/OpenCvSharp.Native/tests/native_smoke.cpp"
$workflowPath = Join-Path $repo ".github/workflows/build-native.yml"
$violations = [System.Collections.Generic.List[string]]::new()

foreach ($path in @($cmakePath, $smokePath, $workflowPath)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Required native CTest boundary file was not found: $path"
    }
}

$cmake = [System.IO.File]::ReadAllText($cmakePath)
$smoke = [System.IO.File]::ReadAllText($smokePath)
$workflow = [System.IO.File]::ReadAllText($workflowPath)

foreach ($required in @(
        'set(OPENCV_CSHARP_NATIVE_SMOKE_TARGET "${OPENCV_CSHARP_NATIVE_TARGET}Smoke")',
        'add_executable(${OPENCV_CSHARP_NATIVE_SMOKE_TARGET}',
        'add_test(NAME ${OPENCV_CSHARP_NATIVE_SMOKE_TARGET} COMMAND ${OPENCV_CSHARP_NATIVE_SMOKE_TARGET})',
        'NAME ${OPENCV_CSHARP_NATIVE_TARGET}AbiGeneratedCheck',
        'NAME ${OPENCV_CSHARP_NATIVE_ABI_EXPORT_TEST}')) {
    if (-not $cmake.Contains($required, [StringComparison]::Ordinal)) {
        $violations.Add("Native CTest boundary is missing: $required")
    }
}

if (-not $workflow.Contains('ctest --test-dir build/native -C Release --output-on-failure', [StringComparison]::Ordinal)) {
    $violations.Add("Native workflow must execute CTest with failure output.")
}

foreach ($surface in @(
        [pscustomobject]@{ Name = "CMake"; Text = $cmake },
        [pscustomobject]@{ Name = "native smoke"; Text = $smoke })) {
    if ($surface.Text -match 'OpenCv[0-9]+Sharp|OPENCV[0-9]+SHARP|open_cv_[0-9]+_sharp|jyppx_ocv[0-9]+_') {
        $violations.Add("$($surface.Name) contains a fixed-major test or output identity.")
    }
}

if ($violations.Count -gt 0) {
    Write-Host "Native CTest/output naming boundary guard failed with $($violations.Count) violation(s)."
    $violations | Sort-Object | ForEach-Object { Write-Host "- $_" }
    exit 1
}

Write-Host "Native CTest/output naming boundary guard passed."
Write-Host "Native smoke target: JYPPX.OpenCV.NativeSmoke."

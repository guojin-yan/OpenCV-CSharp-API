param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$runtimeProjectPath = Join-Path $repo "packaging/runtime/JYPPX.OpenCV.runtime/JYPPX.OpenCV.runtime.csproj"
$headerRoot = Join-Path $repo "src/OpenCvSharp.Native/include"
$neutralHeaderRoot = Join-Path $headerRoot "open_cv_sharp"
$violations = [System.Collections.Generic.List[string]]::new()

if (-not (Test-Path -LiteralPath $runtimeProjectPath -PathType Leaf)) {
    throw "Runtime package project was not found: $runtimeProjectPath"
}
if (-not (Test-Path -LiteralPath $neutralHeaderRoot -PathType Container)) {
    $violations.Add("Version-neutral native header tree is missing: open_cv_sharp")
}

$fixedMajorHeaderDirs = @(
    Get-ChildItem -LiteralPath $headerRoot -Directory -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -match '^open_cv_[0-9]+_sharp$' })
if ($fixedMajorHeaderDirs.Count -gt 0) {
    $violations.Add("Fixed-major native header trees must be absent: $($fixedMajorHeaderDirs.Name -join ', ')")
}

[xml]$project = [System.IO.File]::ReadAllText($runtimeProjectPath)
$packedItems = @($project.SelectNodes("//*[@Pack='true']"))
foreach ($item in $packedItems) {
    $include = [string]$item.Attributes["Include"].Value
    $packagePath = if ($null -eq $item.Attributes["PackagePath"]) { "" } else { [string]$item.Attributes["PackagePath"].Value }
    if ("$include/$packagePath" -match '(?i)(^|[/\\])(include|headers?)([/\\]|$)|open_cv_(?:[0-9]+_)?sharp') {
        $violations.Add("Runtime package must not distribute native headers: Include=$include PackagePath=$packagePath")
    }
}

if ($violations.Count -gt 0) {
    Write-Host "Native header package boundary guard failed with $($violations.Count) violation(s)."
    $violations | Sort-Object | ForEach-Object { Write-Host "- $_" }
    exit 1
}

Write-Host "Native header package boundary guard passed."
Write-Host "Source-tree header surface: src/OpenCvSharp.Native/include/open_cv_sharp."
Write-Host "Runtime package header payload: absent."

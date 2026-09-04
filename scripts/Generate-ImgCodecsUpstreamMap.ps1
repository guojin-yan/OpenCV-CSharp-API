[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path,
    [string]$DotNetPath = "",
    [string]$PythonPath = "",
    [switch]$RegenerateRaw,
    [switch]$InitializeClassification,
    [switch]$Check
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$workspace = (Resolve-Path -LiteralPath (Join-Path $repo "..")).Path
$opencvRoot = Join-Path $workspace "opencv-source/opencv-5.0.0"
if (-not (Test-Path -LiteralPath $opencvRoot -PathType Container)) {
    throw "OpenCV 5.0.0 source root was not found: $opencvRoot"
}

if ([string]::IsNullOrWhiteSpace($DotNetPath)) {
    $dotnetCommand = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($null -eq $dotnetCommand) {
        throw "dotnet was not found. ImgCodecs upstream-map generation requires a .NET 10 SDK."
    }
    $DotNetPath = $dotnetCommand.Source
}
$resolvedDotNet = (Resolve-Path -LiteralPath $DotNetPath).Path
$sdkVersion = (& $resolvedDotNet --version).Trim()
if ($LASTEXITCODE -ne 0 -or $sdkVersion -notmatch '^10\.') {
    throw "ImgCodecs upstream-map generation requires a .NET 10 SDK; resolved '$sdkVersion' from $resolvedDotNet."
}

$rawPath = Join-Path $repo "compatibility/imgcodecs-upstream-raw.json"
$classificationPath = Join-Path $repo "compatibility/imgcodecs-upstream-classifications.json"
$mappingPath = Join-Path $repo "compatibility/imgcodecs-upstream-map.txt"
$summaryPath = Join-Path $repo "compatibility/imgcodecs-upstream-summary.json"
$familyOutputPath = Join-Path $repo "compatibility/imgcodecs-implemented-families.json"

if ($RegenerateRaw) {
    if ([string]::IsNullOrWhiteSpace($PythonPath)) {
        throw "-RegenerateRaw requires -PythonPath pointing to a working Python 3 executable. No machine-specific Python path is embedded in the repository."
    }
    $resolvedPython = (Resolve-Path -LiteralPath $PythonPath).Path
    & $resolvedPython (Join-Path $repo "tools/ImgCodecsUpstreamMap/extract_imgcodecs.py") `
        --workspace $workspace `
        --opencv-root $opencvRoot `
        --output $rawPath
    if ($LASTEXITCODE -ne 0) {
        throw "OpenCV hdr_parser.py ImgCodecs extraction failed with exit code $LASTEXITCODE."
    }
}

if (-not (Test-Path -LiteralPath $rawPath -PathType Leaf)) {
    throw "Checked ImgCodecs raw extraction was not found: $rawPath. Use -RegenerateRaw with an explicit working -PythonPath."
}

$arguments = @(
    "run",
    "--project", (Join-Path $repo "tools/ImgCodecsUpstreamMap/ImgCodecsUpstreamMap.csproj"),
    "--configuration", "Release",
    "--framework", "net10.0",
    "--",
    "--repository", $repo,
    "--workspace", $workspace,
    "--raw", $rawPath,
    "--classification", $classificationPath,
    "--native-manifest", (Join-Path $repo "src/OpenCvSharp.Native/generated/native_abi_manifest.txt"),
    "--managed-baseline", (Join-Path $repo "compatibility/managed-public-api.txt"),
    "--output", $mappingPath,
    "--summary", $summaryPath,
    "--family-output", $familyOutputPath
)
if ($InitializeClassification) {
    $arguments += "--initialize-classification"
}
if ($Check) {
    $arguments += "--check"
}

$previousTelemetry = $env:DOTNET_CLI_TELEMETRY_OPTOUT
$previousNoLogo = $env:DOTNET_NOLOGO
try {
    $env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"
    $env:DOTNET_NOLOGO = "1"
    & $resolvedDotNet @arguments
    if ($LASTEXITCODE -ne 0) {
        throw "ImgCodecs upstream-map generator failed with exit code $LASTEXITCODE."
    }
}
finally {
    $env:DOTNET_CLI_TELEMETRY_OPTOUT = $previousTelemetry
    $env:DOTNET_NOLOGO = $previousNoLogo
}

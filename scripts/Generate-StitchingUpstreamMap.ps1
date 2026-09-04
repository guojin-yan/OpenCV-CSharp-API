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
$opencv = Join-Path $workspace "opencv-source/opencv-5.0.0"
if (-not (Test-Path -LiteralPath $opencv -PathType Container)) { throw "OpenCV 5.0.0 source root was not found: $opencv" }
if ([string]::IsNullOrWhiteSpace($DotNetPath)) { $DotNetPath = (Get-Command dotnet -ErrorAction Stop).Source }
$dotnet = (Resolve-Path -LiteralPath $DotNetPath).Path
$sdkVersion = (& $dotnet --version).Trim()
if ($LASTEXITCODE -ne 0 -or $sdkVersion -notmatch '^10\.') { throw "Stitching map generation requires a .NET 10 SDK; resolved '$sdkVersion' from $dotnet." }

$raw = Join-Path $repo "compatibility/stitching-upstream-raw.json"
if ($RegenerateRaw) {
    if ([string]::IsNullOrWhiteSpace($PythonPath)) { throw "-RegenerateRaw requires an explicit -PythonPath." }
    $python = (Resolve-Path -LiteralPath $PythonPath).Path
    & $python (Join-Path $repo "tools/StitchingUpstreamMap/extract_stitching.py") --workspace $workspace --opencv-root $opencv --output $raw
    if ($LASTEXITCODE -ne 0) { throw "Stitching raw extraction failed with exit code $LASTEXITCODE." }
}
if (-not (Test-Path -LiteralPath $raw -PathType Leaf)) { throw "Checked Stitching raw artifact is missing: $raw" }

$arguments = @(
    "run", "--project", (Join-Path $repo "tools/StitchingUpstreamMap/StitchingUpstreamMap.csproj"),
    "--configuration", "Release", "--framework", "net10.0", "--",
    "--repository", $repo, "--workspace", $workspace, "--raw", $raw,
    "--classification", (Join-Path $repo "compatibility/stitching-upstream-classifications.json"),
    "--native-manifest", (Join-Path $repo "src/OpenCvSharp.Native/generated/native_abi_manifest.txt"),
    "--managed-baseline", (Join-Path $repo "compatibility/managed-public-api.txt"),
    "--output", (Join-Path $repo "compatibility/stitching-upstream-map.txt"),
    "--summary", (Join-Path $repo "compatibility/stitching-upstream-summary.json"),
    "--family-output", (Join-Path $repo "compatibility/stitching-implemented-families.json")
)
if ($InitializeClassification) { $arguments += "--initialize-classification" }
if ($Check) { $arguments += "--check" }

$previousTelemetry = $env:DOTNET_CLI_TELEMETRY_OPTOUT
$previousNoLogo = $env:DOTNET_NOLOGO
try {
    $env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"
    $env:DOTNET_NOLOGO = "1"
    & $dotnet @arguments
    if ($LASTEXITCODE -ne 0) { throw "Stitching map generator failed with exit code $LASTEXITCODE." }
}
finally {
    $env:DOTNET_CLI_TELEMETRY_OPTOUT = $previousTelemetry
    $env:DOTNET_NOLOGO = $previousNoLogo
}

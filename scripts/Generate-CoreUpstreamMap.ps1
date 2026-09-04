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
if ([string]::IsNullOrWhiteSpace($DotNetPath)) { $DotNetPath = (Get-Command dotnet -ErrorAction Stop).Source }
$dotnet = (Resolve-Path -LiteralPath $DotNetPath).Path
if ((& $dotnet --version).Trim() -notmatch '^10\.') { throw "Core map generation requires a .NET 10 SDK." }
$raw = Join-Path $repo "compatibility/core-upstream-raw.json"
if ($RegenerateRaw) {
    if ([string]::IsNullOrWhiteSpace($PythonPath)) { throw "-RegenerateRaw requires an explicit -PythonPath." }
    & (Resolve-Path $PythonPath).Path (Join-Path $repo "tools/CoreUpstreamMap/extract_core.py") --workspace $workspace --opencv-root $opencv --output $raw
    if ($LASTEXITCODE -ne 0) { throw "Core raw extraction failed." }
}
if (-not (Test-Path -LiteralPath $raw -PathType Leaf)) { throw "Core raw artifact is missing." }
$arguments = @("run", "--project", (Join-Path $repo "tools/CoreUpstreamMap/CoreUpstreamMap.csproj"), "--configuration", "Release", "--framework", "net10.0", "--", "--repository", $repo, "--workspace", $workspace, "--raw", $raw, "--classification", (Join-Path $repo "compatibility/core-upstream-classifications.json"), "--native-manifest", (Join-Path $repo "src/OpenCvSharp.Native/generated/native_abi_manifest.txt"), "--managed-baseline", (Join-Path $repo "compatibility/managed-public-api.txt"), "--output", (Join-Path $repo "compatibility/core-upstream-map.txt"), "--summary", (Join-Path $repo "compatibility/core-upstream-summary.json"), "--family-output", (Join-Path $repo "compatibility/core-implemented-families.json"))
if ($InitializeClassification) { $arguments += "--initialize-classification" }
if ($Check) { $arguments += "--check" }
$oldTelemetry = $env:DOTNET_CLI_TELEMETRY_OPTOUT; $oldNoLogo = $env:DOTNET_NOLOGO
try { $env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"; $env:DOTNET_NOLOGO = "1"; & $dotnet @arguments; if ($LASTEXITCODE -ne 0) { throw "Core map generator failed." } }
finally { $env:DOTNET_CLI_TELEMETRY_OPTOUT = $oldTelemetry; $env:DOTNET_NOLOGO = $oldNoLogo }

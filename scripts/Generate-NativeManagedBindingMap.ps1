[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path,
    [string]$DotNetPath = "",
    [switch]$Check
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
if ([string]::IsNullOrWhiteSpace($DotNetPath)) {
    $dotnetCommand = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($null -eq $dotnetCommand) {
        throw "dotnet was not found. Native/managed binding-map generation requires SDK 10.0.302."
    }
    $DotNetPath = $dotnetCommand.Source
}

$resolvedDotNet = (Resolve-Path -LiteralPath $DotNetPath).Path
$sdkVersion = (& $resolvedDotNet --version).Trim()
if ($LASTEXITCODE -ne 0 -or $sdkVersion -ne "10.0.302") {
    throw "Native/managed binding-map generation requires exact SDK 10.0.302; resolved '$sdkVersion' from $resolvedDotNet."
}

$managedProject = Join-Path $repo "src/OpenCvSharp/OpenCvSharp.csproj"
$managedAssembly = Join-Path $repo "src/OpenCvSharp/bin/Release/net10.0/JYPPX.OpenCV.CSharp.API.dll"
$toolProject = Join-Path $repo "tools/NativeManagedBindingMap/NativeManagedBindingMap.csproj"
$manifestPath = Join-Path $repo "src/OpenCvSharp.Native/generated/native_abi_manifest.txt"
$sourceRoot = Join-Path $repo "src/OpenCvSharp/Internal/Interop"
$outputPath = Join-Path $repo "compatibility/native-managed-binding-map.txt"
$summaryPath = Join-Path $repo "compatibility/native-managed-binding-summary.json"

$previousTelemetry = $env:DOTNET_CLI_TELEMETRY_OPTOUT
$previousNoLogo = $env:DOTNET_NOLOGO
try {
    $env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"
    $env:DOTNET_NOLOGO = "1"

    & $resolvedDotNet build $managedProject --configuration Release --framework net10.0 --nologo
    if ($LASTEXITCODE -ne 0) {
        throw "Managed assembly build for binding-map generation failed with exit code $LASTEXITCODE."
    }

    $arguments = @(
        "run",
        "--project", $toolProject,
        "--configuration", "Release",
        "--framework", "net10.0",
        "--",
        "--repository", $repo,
        "--assembly", $managedAssembly,
        "--manifest", $manifestPath,
        "--source-root", $sourceRoot,
        "--output", $outputPath,
        "--summary", $summaryPath
    )
    if ($Check) {
        $arguments += "--check"
    }

    & $resolvedDotNet @arguments
    if ($LASTEXITCODE -ne 0) {
        throw "Native/managed binding-map generator failed with exit code $LASTEXITCODE."
    }
}
finally {
    $env:DOTNET_CLI_TELEMETRY_OPTOUT = $previousTelemetry
    $env:DOTNET_NOLOGO = $previousNoLogo
}

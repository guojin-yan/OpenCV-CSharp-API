[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path,
    [string]$DotNetPath = "",
    [switch]$Check
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$projectPath = Join-Path $repo "tools/ManagedApiBaseline/ManagedApiBaseline.csproj"
$outputPath = Join-Path $repo "compatibility/managed-public-api.txt"
$summaryPath = Join-Path $repo "compatibility/managed-public-api-summary.json"

if ([string]::IsNullOrWhiteSpace($DotNetPath)) {
    $dotnetCommand = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($null -eq $dotnetCommand) {
        throw "dotnet was not found. Managed API baseline generation requires SDK 10.0.302."
    }
    $DotNetPath = $dotnetCommand.Source
}

$resolvedDotNet = (Resolve-Path -LiteralPath $DotNetPath).Path
$sdkVersion = (& $resolvedDotNet --version).Trim()
if ($LASTEXITCODE -ne 0 -or $sdkVersion -ne "10.0.302") {
    throw "Managed API baseline generation requires exact SDK 10.0.302; resolved '$sdkVersion' from $resolvedDotNet."
}

$arguments = @(
    "run",
    "--project", $projectPath,
    "--configuration", "Release",
    "--framework", "net8.0",
    "--",
    "--output", $outputPath,
    "--summary", $summaryPath
)
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
        throw "Managed API baseline generator failed with exit code $LASTEXITCODE."
    }
}
finally {
    $env:DOTNET_CLI_TELEMETRY_OPTOUT = $previousTelemetry
    $env:DOTNET_NOLOGO = $previousNoLogo
}

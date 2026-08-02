[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$UnsignedPackagePath,
    [Parameter(Mandatory = $true)]
    [string]$RepositorySignedPackagePath,
    [Parameter(Mandatory = $true)]
    [string]$PackageId,
    [Parameter(Mandatory = $true)]
    [string]$PackageVersion,
    [string]$ExpectedOwner = "GuojinYan",
    [string]$ServiceIndex = "https://api.nuget.org/v3/index.json",
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}Z$')]
    [string]$VerifiedAt,
    [string]$OutputPath = "",
    [switch]$Check,
    [string]$DotNetPath = "dotnet"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$unsigned = (Resolve-Path -LiteralPath $UnsignedPackagePath).Path
$signed = (Resolve-Path -LiteralPath $RepositorySignedPackagePath).Path
$tool = Join-Path $repo "tools/NuGetRepositorySignatureVerifier/NuGetRepositorySignatureVerifier.csproj"
$temporaryRoot = Join-Path ([IO.Path]::GetTempPath()) ("opencv-nuget-repository-verify-" + [guid]::NewGuid().ToString("N"))

try {
    New-Item -ItemType Directory -Force -Path $temporaryRoot | Out-Null
    $verifyOutputPath = Join-Path $temporaryRoot "dotnet-nuget-verify.txt"
    $previousLanguage = $env:DOTNET_CLI_UI_LANGUAGE
    try {
        $env:DOTNET_CLI_UI_LANGUAGE = "en-US"
        $verifyOutput = @(& $DotNetPath nuget verify --all --verbosity detailed $signed 2>&1)
        $verifyExitCode = $LASTEXITCODE
    }
    finally {
        $env:DOTNET_CLI_UI_LANGUAGE = $previousLanguage
    }
    [IO.File]::WriteAllLines($verifyOutputPath, @($verifyOutput | ForEach-Object { [string]$_ }), [Text.UTF8Encoding]::new($false))
    if ($verifyExitCode -ne 0) {
        throw "dotnet nuget verify failed with exit code $verifyExitCode.`n$($verifyOutput -join [Environment]::NewLine)"
    }

    $verifyText = $verifyOutput -join "`n"
    if ($verifyText -notmatch '(?m)^Signature type:\s*Repository\s*$') {
        throw "dotnet nuget verify did not report a Repository signature."
    }
    if ($verifyText -notmatch 'CN=NuGet\.org Repository by Microsoft') {
        throw "dotnet nuget verify did not report the expected NuGet.org repository signer."
    }

    $arguments = @(
        "run", "--project", $tool, "--configuration", "Release", "--framework", "net8.0", "--",
        "--unsigned", $unsigned,
        "--signed", $signed,
        "--package-id", $PackageId,
        "--package-version", $PackageVersion,
        "--expected-owner", $ExpectedOwner,
        "--service-index", $ServiceIndex,
        "--verified-at", $VerifiedAt
    )
    if (-not [string]::IsNullOrWhiteSpace($OutputPath)) {
        $arguments += @("--output", [IO.Path]::GetFullPath($OutputPath))
    }
    if ($Check) { $arguments += "--check" }

    & $DotNetPath @arguments
    if ($LASTEXITCODE -ne 0) {
        throw "NuGet repository-signature structured verification failed with exit code $LASTEXITCODE."
    }

    $verifyOutputSha256 = (Get-FileHash -LiteralPath $verifyOutputPath -Algorithm SHA256).Hash.ToLowerInvariant()
    Write-Host "NUGET_REPOSITORY_SIGNED_PACKAGE_OK package=$PackageId/$PackageVersion cryptographic_verification=passed signature_type=Repository owner=$ExpectedOwner verify_output_sha256=$verifyOutputSha256"
}
finally {
    if (Test-Path -LiteralPath $temporaryRoot -PathType Container) {
        [IO.Directory]::Delete((Resolve-Path -LiteralPath $temporaryRoot).Path, $true)
    }
}

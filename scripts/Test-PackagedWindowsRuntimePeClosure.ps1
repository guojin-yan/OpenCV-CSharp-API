param(
    [Parameter(Mandatory = $true)]
    [string]$PackagePath,
    [ValidateSet("win-x64", "win-x86", "win-arm64")]
    [string]$Rid = "win-x64",
    [ValidateSet("full", "mini")]
    [string]$RuntimeProfile = "full",
    [string]$OpenCvVersion = "5.1.0",
    [string]$RuntimePackageMatrix = "packaging/runtime/runtime-package-matrix.json",
    [string]$DumpbinPath = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..")).Path
$package = if ([System.IO.Path]::IsPathRooted($PackagePath)) { $PackagePath } else { Join-Path $repoRoot $PackagePath }
if (-not (Test-Path -LiteralPath $package -PathType Leaf)) {
    throw "Package was not found: $package"
}
$package = (Resolve-Path -LiteralPath $package).Path
if ([System.IO.Path]::GetExtension($package) -ine ".nupkg") {
    throw "Package must be a .nupkg file: $package"
}

$matrixPath = if ([System.IO.Path]::IsPathRooted($RuntimePackageMatrix)) { $RuntimePackageMatrix } else { Join-Path $repoRoot $RuntimePackageMatrix }
if (-not (Test-Path -LiteralPath $matrixPath -PathType Leaf)) {
    throw "Runtime package matrix was not found: $matrixPath"
}
$matrix = Get-Content -LiteralPath $matrixPath -Raw | ConvertFrom-Json
$ridSpec = @($matrix.rids | Where-Object { [string]$_.rid -ceq $Rid })
$profileSpec = @($matrix.profiles | Where-Object { [string]$_.name -ceq $RuntimeProfile })
if ($ridSpec.Count -ne 1 -or $profileSpec.Count -ne 1) {
    throw "RID/profile was not found exactly once in the runtime package matrix: $Rid / $RuntimeProfile"
}
if ([string]$ridSpec[0].platformFamily -ine "windows") {
    throw "Packaged Windows PE closure requires a Windows runtime matrix row: $Rid"
}

$version = [System.Version]::Parse($OpenCvVersion)
$binarySuffix = "$($version.Major)$($version.Minor)$($version.Build)"
$expectedPackageId = "JYPPX.OpenCV.runtime.$Rid"
if ($RuntimeProfile -ceq "mini") { $expectedPackageId += ".mini" }
$expectedNativePrefix = "runtimes/$Rid/native/"
$temporaryRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("opencv-packaged-pe-" + [System.Guid]::NewGuid().ToString("N"))
$nativeDirectory = Join-Path $temporaryRoot "native"

function Assert-SafeZipEntry {
    param([Parameter(Mandatory = $true)][string]$Name)

    $normalized = $Name.Replace("\", "/")
    if ([System.IO.Path]::IsPathRooted($normalized) -or $normalized.Contains("../") -or $normalized.StartsWith("../", [System.StringComparison]::Ordinal)) {
        throw "Package contains an unsafe ZIP entry path: $Name"
    }
}

function Get-ZipEntrySha256 {
    param([Parameter(Mandatory = $true)][System.IO.Compression.ZipArchiveEntry]$Entry)

    $stream = $Entry.Open()
    try {
        return ([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))).ToLowerInvariant()
    }
    finally {
        $stream.Dispose()
    }
}

try {
    New-Item -ItemType Directory -Force -Path $nativeDirectory | Out-Null
    $archive = [System.IO.Compression.ZipFile]::OpenRead($package)
    try {
        foreach ($entry in $archive.Entries) { Assert-SafeZipEntry -Name $entry.FullName }

        $nuspecs = @($archive.Entries | Where-Object { $_.FullName -match '^[^/]+\.nuspec$' })
        if ($nuspecs.Count -ne 1) { throw "Package must contain exactly one root nuspec: $package" }
        $nuspecReader = [System.IO.StreamReader]::new($nuspecs[0].Open())
        try { $nuspecText = $nuspecReader.ReadToEnd() } finally { $nuspecReader.Dispose() }
        if ($nuspecText -notmatch "<id>\s*$([regex]::Escape($expectedPackageId))\s*</id>" -or
            $nuspecText -notmatch "<version>\s*$([regex]::Escape($version.ToString()))(?:\.0)?\s*</version>") {
            throw "Package nuspec identity does not match $expectedPackageId/$($OpenCvVersion): $package"
        }

        $nativeEntries = @($archive.Entries | Where-Object {
                $_.FullName.StartsWith($expectedNativePrefix, [System.StringComparison]::Ordinal) -and
                $_.FullName.EndsWith(".dll", [System.StringComparison]::OrdinalIgnoreCase)
            })
        if ($nativeEntries.Count -eq 0) { throw "Package contains no Windows native runtime DLLs: $package" }
        foreach ($entry in $nativeEntries) {
            $relative = $entry.FullName.Substring($expectedNativePrefix.Length)
            if ($relative.Contains("/") -or [string]::IsNullOrWhiteSpace($relative)) {
                throw "Native package entries must be direct DLL files: $($entry.FullName)"
            }
            $destination = Join-Path $nativeDirectory $relative
            [System.IO.Compression.ZipFileExtensions]::ExtractToFile($entry, $destination, $true)
        }
    }
    finally {
        $archive.Dispose()
    }

    $auditScript = Join-Path $repoRoot "scripts/Test-WindowsRuntimePeClosure.ps1"
    $auditOutput = @(& $auditScript -RuntimeDirectory $nativeDirectory -Rid $Rid -RuntimeProfile $RuntimeProfile -OpenCvVersion $OpenCvVersion -RuntimePackageMatrix $matrixPath -DumpbinPath $DumpbinPath 2>&1)
    if ($LASTEXITCODE -ne 0) {
        throw "Packaged Windows PE closure audit failed: $($auditOutput -join [Environment]::NewLine)"
    }
    $packageHash = (Get-FileHash -LiteralPath $package -Algorithm SHA256).Hash.ToLowerInvariant()
    $nativeHashRows = @(
        Get-ChildItem -LiteralPath $nativeDirectory -File -Filter "*.dll" |
            Sort-Object Name |
            ForEach-Object {
                [pscustomobject]@{
                    path = "$expectedNativePrefix$($_.Name)"
                    length = $_.Length
                    sha256 = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
                }
            }
    )

    Write-Host "PACKAGED_WINDOWS_PE_AUDIT_OK package=$([System.IO.Path]::GetFileName($package)) rid=$Rid profile=$RuntimeProfile package_sha256=$packageHash native_files=$($nativeHashRows.Count)"
}
finally {
    if (Test-Path -LiteralPath $temporaryRoot -PathType Container) {
        Remove-Item -LiteralPath $temporaryRoot -Recurse -Force
    }
}

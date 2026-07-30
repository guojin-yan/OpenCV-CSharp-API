[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$workspace = (Resolve-Path -LiteralPath (Join-Path $repo "..")).Path
$openCvCommit = "40738fb16ceddb5fb3fea747585f7ce6abb0605b"
$sourcePrefix = "opencv-source/opencv-5.0.0/"
$rawNames = @("imgproc", "imgcodecs", "videoio", "calib3d", "core", "dnn", "features", "objdetect", "photo", "video")
$required = [System.Collections.Generic.Dictionary[string, string]]::new([StringComparer]::Ordinal)

function Add-RequiredFile {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$Sha256
    )

    if (-not $Path.StartsWith($sourcePrefix, [StringComparison]::Ordinal)) {
        throw "Upstream map evidence path is outside the exact OpenCV source root: $Path"
    }
    if ($Sha256 -notmatch "^[0-9a-f]{64}$") {
        throw "Upstream map evidence SHA256 is malformed for $Path."
    }
    $existingSha256 = ""
    if ($required.TryGetValue($Path, [ref]$existingSha256)) {
        if ($existingSha256 -ne $Sha256) {
            throw "Upstream map evidence assigns conflicting SHA256 values to $Path."
        }
        return
    }
    $required.Add($Path, $Sha256)
}

foreach ($name in $rawNames) {
    $rawPath = Join-Path $repo "compatibility/$name-upstream-raw.json"
    if (-not (Test-Path -LiteralPath $rawPath -PathType Leaf)) {
        throw "Upstream raw evidence was not found: $rawPath"
    }
    $raw = Get-Content -LiteralPath $rawPath -Raw | ConvertFrom-Json
    Add-RequiredFile -Path ([string]$raw.headerPath) -Sha256 ([string]$raw.headerSha256)
    Add-RequiredFile -Path ([string]$raw.parserPath) -Sha256 ([string]$raw.parserSha256)
    foreach ($propertyName in @("compatibilityHeaders", "sourceHeaders")) {
        $property = $raw.PSObject.Properties[$propertyName]
        if ($null -eq $property) {
            continue
        }
        foreach ($header in @($property.Value)) {
            Add-RequiredFile -Path ([string]$header.path) -Sha256 ([string]$header.sha256)
        }
    }
}

$registryPath = Join-Path $repo "compatibility/videoio-registry-surface.json"
$registry = Get-Content -LiteralPath $registryPath -Raw | ConvertFrom-Json
Add-RequiredFile -Path ([string]$registry.headerPath) -Sha256 ([string]$registry.headerSha256)

$downloaded = 0
foreach ($entry in @($required.GetEnumerator() | Sort-Object Key)) {
    $relativePath = $entry.Key.Substring($sourcePrefix.Length)
    $destination = Join-Path $workspace ($entry.Key -replace '/', [IO.Path]::DirectorySeparatorChar)
    if (Test-Path -LiteralPath $destination -PathType Leaf) {
        $actualSha256 = (Get-FileHash -LiteralPath $destination -Algorithm SHA256).Hash.ToLowerInvariant()
        if ($actualSha256 -eq $entry.Value) {
            continue
        }
    }

    [IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($destination)) | Out-Null
    $downloadPath = "$destination.download"
    try {
        $url = "https://raw.githubusercontent.com/opencv/opencv/$openCvCommit/$relativePath"
        Invoke-WebRequest -Uri $url -OutFile $downloadPath -MaximumRedirection 0
        $actualSha256 = (Get-FileHash -LiteralPath $downloadPath -Algorithm SHA256).Hash.ToLowerInvariant()
        if ($actualSha256 -ne $entry.Value) {
            throw "Downloaded upstream evidence SHA256 drifted for $relativePath. Expected $($entry.Value), found $actualSha256."
        }
        Move-Item -LiteralPath $downloadPath -Destination $destination -Force
        $downloaded++
    }
    finally {
        if (Test-Path -LiteralPath $downloadPath -PathType Leaf) {
            Remove-Item -LiteralPath $downloadPath -Force
        }
    }
}

Write-Host "UPSTREAM_MAP_SOURCE_EVIDENCE_OK commit=$openCvCommit files=$($required.Count) downloaded=$downloaded root=$(Join-Path $workspace 'opencv-source/opencv-5.0.0')"

[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$workspace = (Resolve-Path -LiteralPath (Join-Path $repo "..")).Path
$surfacePath = Join-Path $repo "compatibility/videoio-registry-surface.json"
$manifestPath = Join-Path $repo "src/OpenCvSharp.Native/generated/legacy_abi_manifest.txt"
$managedPath = Join-Path $repo "compatibility/managed-public-api.txt"
$surface = Get-Content -LiteralPath $surfacePath -Raw | ConvertFrom-Json

if ($surface.schemaVersion -ne 1 -or $surface.upstreamOpenCvVersion -ne "5.0.0" -or $surface.reviewStatus -ne "implemented-verified") {
    throw "VideoIO registry surface identity or review status drifted."
}

$headerPath = Join-Path $workspace $surface.headerPath
if (-not (Test-Path -LiteralPath $headerPath -PathType Leaf)) {
    throw "VideoIO registry upstream header was not found: $headerPath"
}
$headerHash = (Get-FileHash -LiteralPath $headerPath -Algorithm SHA256).Hash.ToLowerInvariant()
if ($headerHash -ne $surface.headerSha256) {
    throw "VideoIO registry header SHA256 drifted: expected=$($surface.headerSha256) actual=$headerHash."
}

$headerText = Get-Content -LiteralPath $headerPath -Raw
$sourceNames = @([regex]::Matches(
    $headerText,
    'CV_EXPORTS_W\s+(?:cv::String|std::vector<VideoCaptureAPIs>|bool|std::string)\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*\(',
    [System.Text.RegularExpressions.RegexOptions]::CultureInvariant) | ForEach-Object { $_.Groups['name'].Value })
$reviewedNames = @($surface.operations | ForEach-Object { $_.upstreamName })
if ($sourceNames.Count -ne 12 -or ($sourceNames -join "`n") -ne ($reviewedNames -join "`n")) {
    throw "VideoIO registry source declarations no longer match the 12 reviewed operations."
}

$nativeSet = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
Get-Content -LiteralPath $manifestPath | ForEach-Object {
    if ($_ -match '^(jyppx_ocv_[^|]+)\|') { [void]$nativeSet.Add($Matches[1]) }
}
$managedSet = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
Get-Content -LiteralPath $managedPath | Where-Object { $_.StartsWith('MEMBER|', [StringComparison]::Ordinal) } | ForEach-Object { [void]$managedSet.Add($_) }

$nativeEvidence = [System.Collections.Generic.List[string]]::new()
$managedEvidence = [System.Collections.Generic.List[string]]::new()
foreach ($operation in $surface.operations) {
    $entrypoints = @($operation.nativeEntrypoints)
    $sortedEntrypoints = @($entrypoints | Sort-Object -CaseSensitive)
    if ($entrypoints.Count -eq 0 -or ($sortedEntrypoints -join "`n") -ne ($entrypoints -join "`n")) {
        throw "VideoIO registry native evidence is empty or nondeterministically ordered: $($operation.upstreamName)"
    }
    foreach ($entrypoint in $entrypoints) {
        if (-not $nativeSet.Contains($entrypoint)) { throw "Missing VideoIO registry native evidence: $entrypoint" }
        if ($entrypoint -match '^jyppx_ocv[0-9]+_') { throw "Fixed-major VideoIO registry native evidence is forbidden: $entrypoint" }
        $nativeEvidence.Add($entrypoint)
    }
    if (-not $managedSet.Contains($operation.managedMember)) {
        throw "Missing VideoIO registry managed evidence: $($operation.managedMember)"
    }
    $managedEvidence.Add($operation.managedMember)
}

if (($nativeEvidence | Select-Object -Unique).Count -ne 22 -or ($managedEvidence | Select-Object -Unique).Count -ne 12) {
    throw "VideoIO registry evidence counts drifted: native=$(($nativeEvidence | Select-Object -Unique).Count) managed=$(($managedEvidence | Select-Object -Unique).Count)."
}

$surfaceHash = (Get-FileHash -LiteralPath $surfacePath -Algorithm SHA256).Hash.ToLowerInvariant()
Write-Host "VIDEOIO_REGISTRY_SURFACE_OK operations=12 native=22 managed=12 headerSha256=$headerHash surfaceSha256=$surfaceHash"

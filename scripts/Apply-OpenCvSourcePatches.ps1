param(
    [Parameter(Mandatory = $true)]
    [string]$OpenCvSourceDir,
    [string]$OpenCvVersion = "5.0.0"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..")).Path
$sourceDir = (Resolve-Path -LiteralPath $OpenCvSourceDir).Path
$patchRelativePath = "packaging/runtime/patches/opencv-5.0.0-photo-ccm-instance-color-space.patch"
$patchPath = Join-Path $repoRoot $patchRelativePath
$targetRelativePath = "modules/photo/src/ccm/ccm.cpp"
$targetPath = Join-Path $sourceDir $targetRelativePath

if ($OpenCvVersion -ne "5.0.0") {
    throw "The audited photo CCM source patch is only defined for OpenCV 5.0.0, not '$OpenCvVersion'."
}
if (-not (Test-Path -LiteralPath $patchPath -PathType Leaf)) {
    throw "OpenCV source patch was not found: $patchPath"
}
if (-not (Test-Path -LiteralPath $targetPath -PathType Leaf)) {
    throw "OpenCV photo CCM source file was not found: $targetPath"
}

$text = [System.IO.File]::ReadAllText($targetPath)
$original = "    RGBBase_& cs;"
$patched = "    RGBBase_ cs;"
$originalCount = ([regex]::Matches($text, [regex]::Escape($original))).Count
$patchedCount = ([regex]::Matches($text, [regex]::Escape($patched))).Count

if ($originalCount -eq 1 -and $patchedCount -eq 0) {
    $text = $text.Replace($original, $patched)
    [System.IO.File]::WriteAllText($targetPath, $text, [System.Text.UTF8Encoding]::new($false))
}
elseif ($originalCount -eq 0 -and $patchedCount -eq 1) {
    # Idempotent for a source tree reused by a local build or a workflow retry.
}
else {
    throw "OpenCV photo CCM source patch precondition failed: original=$originalCount patched=$patchedCount."
}

$finalText = [System.IO.File]::ReadAllText($targetPath)
if (([regex]::Matches($finalText, [regex]::Escape($original))).Count -ne 0 -or
    ([regex]::Matches($finalText, [regex]::Escape($patched))).Count -ne 1) {
    throw "OpenCV photo CCM source patch verification failed: $targetRelativePath"
}

$patchSha256 = (Get-FileHash -LiteralPath $patchPath -Algorithm SHA256).Hash
$evidence = [ordered]@{
    Path = $patchRelativePath
    Sha256 = $patchSha256
    Target = $targetRelativePath
    Reason = "keep ColorCorrectionModel color-space state per instance"
}
$evidenceJson = $evidence | ConvertTo-Json -Compress
Write-Output $evidenceJson
Write-Host "OPENCV_SOURCE_PATCH_OK path=$patchRelativePath sha256=$patchSha256 target=$targetRelativePath"

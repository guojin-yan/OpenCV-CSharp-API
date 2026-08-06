param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$violations = [System.Collections.Generic.List[string]]::new()

function Add-Violation {
    param([string]$Message)
    $violations.Add($Message)
}

function Read-RequiredFile {
    param([string]$RelativePath)
    $path = Join-Path $repo $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        Add-Violation "Required release-notes file is missing: $RelativePath"
        return ""
    }

    return Get-Content -LiteralPath $path -Raw
}

$changelog = Read-RequiredFile "CHANGELOG.md"
$index = Read-RequiredFile "docs/releases/README.md"
$readme = Read-RequiredFile "README.md"
$readmeCn = Read-RequiredFile "README_cn.md"
$contributing = Read-RequiredFile "CONTRIBUTING.md"

$versionMatches = [regex]::Matches($changelog, '(?m)^## \[(?<version>[0-9]+\.[0-9]+\.[0-9]+(?:-[0-9A-Za-z.-]+)?)\]')
if ($versionMatches.Count -eq 0) {
    Add-Violation "CHANGELOG.md must start with at least one normalized SemVer version heading."
    $currentVersion = ""
}
else {
    $currentVersion = $versionMatches[0].Groups['version'].Value
}

if (-not [string]::IsNullOrWhiteSpace($currentVersion)) {
    $currentRelativePath = "docs/releases/$currentVersion.md"
    $currentDetail = Read-RequiredFile $currentRelativePath
    $currentLink = [regex]::Escape($currentRelativePath)
    if ($readme -notmatch $currentLink -or $readme -notmatch "What's New In $([regex]::Escape($currentVersion))") {
        Add-Violation "README.md must summarize and link the current release note: $currentRelativePath"
    }
    if ($readmeCn -notmatch $currentLink -or $readmeCn -notmatch [regex]::Escape("$currentVersion 本次更新")) {
        Add-Violation "README_cn.md must summarize and link the current release note: $currentRelativePath"
    }
    if ($index -notmatch [regex]::Escape("$currentVersion.md")) {
        Add-Violation "docs/releases/README.md must index the current release note: $currentRelativePath"
    }
    if ($currentDetail -notmatch "(?m)^Status:\s*" -or $currentDetail -notmatch "(?m)^## Summary / 概要" -or
        $currentDetail -notmatch "(?m)^## Managed API / 托管接口" -or
        $currentDetail -notmatch "(?m)^## Native ABI and runtimes / Native ABI 与运行时" -or
        $currentDetail -notmatch "(?m)^## Compatibility and migration / 兼容与迁移" -or
        $currentDetail -notmatch "(?m)^## Validation / 验证") {
        Add-Violation "Current detailed release note is missing one or more required sections: $currentRelativePath"
    }
}

foreach ($requiredRule in @(
    'CHANGELOG.md',
    'docs/releases/<version>.md',
    'docs/releases/README.md',
    'README.md',
    'README_cn.md',
    'NuGet.org',
    'GitHub Packages',
    'GitHub Release'
)) {
    if ($contributing -notmatch [regex]::Escape($requiredRule)) {
        Add-Violation "CONTRIBUTING.md is missing the release-notes rule token: $requiredRule"
    }
}

$detailRoot = Join-Path $repo "docs/releases"
if (Test-Path -LiteralPath $detailRoot -PathType Container) {
    $detailFiles = @(Get-ChildItem -LiteralPath $detailRoot -Filter "*.md" -File | Where-Object { $_.Name -ne "README.md" })
    if ($detailFiles.Count -eq 0) {
        Add-Violation "docs/releases must contain at least one version detail file."
    }

    foreach ($detailFile in $detailFiles) {
        $version = [IO.Path]::GetFileNameWithoutExtension($detailFile.Name)
        if ($version -notmatch '^[0-9]+\.[0-9]+\.[0-9]+(?:-[0-9A-Za-z.-]+)?$') {
            Add-Violation "Release detail filename is not a normalized version: docs/releases/$($detailFile.Name)"
            continue
        }

        $link = [regex]::Escape("$version.md")
        if ($changelog -notmatch $link -or $index -notmatch $link) {
            Add-Violation "Release detail is missing from CHANGELOG.md or docs/releases/README.md: $version"
        }
    }
}

if ($violations.Count -gt 0) {
    $violations | ForEach-Object { Write-Error $_ }
    exit 1
}

Write-Host "RELEASE_NOTES_CONTRACT_OK current=$currentVersion versions=$($versionMatches.Count)"

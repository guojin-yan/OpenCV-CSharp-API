param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$violations = [System.Collections.Generic.List[string]]::new()

function Add-Violation {
    param([Parameter(Mandatory)][string]$Text)
    $violations.Add($Text)
}

function Read-RequiredText {
    param([Parameter(Mandatory)][string]$RelativePath)

    $path = Join-Path $repo ($RelativePath -replace "/", [IO.Path]::DirectorySeparatorChar)
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Required file was not found: $RelativePath"
    }
    return [IO.File]::ReadAllText($path)
}

function Get-ProjectFiles {
    $files = [System.Collections.Generic.List[string]]::new()

    $sharedProps = Join-Path $repo "Directory.Build.props"
    if (Test-Path -LiteralPath $sharedProps -PathType Leaf) {
        $files.Add($sharedProps)
    }

    foreach ($project in Get-ChildItem -LiteralPath $repo -Recurse -Filter "*.csproj" -File) {
        $text = [IO.File]::ReadAllText($project.FullName)
        if ($text -match "<IsPackable>\s*true\s*</IsPackable>") {
            $files.Add($project.FullName)
        }
    }

    return @($files | Sort-Object -Unique)
}

function Get-RelativePath {
    param([Parameter(Mandatory)][string]$Path)
    return ([IO.Path]::GetRelativePath($repo, $Path)) -replace "\\", "/"
}

$adrPath = "docs/articles/image-adapter-selection-adr.md"
$adr = Read-RequiredText -RelativePath $adrPath
foreach ($requiredText in @(
        "No adapter package is added to the core package",
        "core managed/native package must keep zero new third-party image dependencies",
        "ImageSharp remains opt-in",
        "Promotion requires color/alpha/stride tests")) {
    if (-not $adr.Contains($requiredText, [StringComparison]::Ordinal)) {
        Add-Violation "$adrPath is missing the required license or promotion boundary: $requiredText"
    }
}

$forbiddenPackagePattern = "(?i)(SixLabors\.ImageSharp|SkiaSharp|NetVips|Magick\.NET|Avalonia|PresentationCore|System\.Drawing\.Common)"
foreach ($path in Get-ProjectFiles) {
    $text = [IO.File]::ReadAllText($path)
    $matches = [regex]::Matches($text, '<PackageReference\s+[^>]*Include="([^"]+)"[^>]*>', [Text.RegularExpressions.RegexOptions]::IgnoreCase)
    foreach ($match in $matches) {
        $packageId = [string]$match.Groups[1].Value
        if ($packageId -match $forbiddenPackagePattern) {
            Add-Violation "Core packable project $(Get-RelativePath -Path $path) references forbidden adapter/UI package '$packageId'."
        }
    }
}

if ($violations.Count -gt 0) {
    $violations | ForEach-Object { Write-Error $_ }
    exit 1
}

Write-Host "Image adapter license gate passed: core packable projects contain no adapter/UI image dependencies."

param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$git = Get-Command git -ErrorAction SilentlyContinue
if ($null -eq $git) {
    throw "git was not found. Compatibility-surface validation requires the repository file inventory."
}

$relativePaths = @(& $git.Source -C $repo ls-files --cached --others --exclude-standard)
if ($LASTEXITCODE -ne 0) {
    throw "git ls-files failed while collecting the repository file inventory."
}

$forbiddenPattern = [System.Text.RegularExpressions.Regex]::new(
    'open_cv_5_sharp|jyppx_ocv5_|OpenCv5Sharp|OPENCV5SHARP_|legacy_abi|legacy_names\.h|NativeCompatibilitySourceSmoke|CompatibilityNativeLoaderName|Test-NativeLegacyIncludeParity|Generate-NativeAbiCompatibility',
    [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
$textExtensions = [System.Collections.Generic.HashSet[string]]::new(
    [string[]]@('.c', '.cc', '.cmake', '.cpp', '.cs', '.csproj', '.h', '.hpp', '.json', '.md', '.props', '.ps1', '.psm1', '.targets', '.txt', '.yaml', '.yml'),
    [System.StringComparer]::OrdinalIgnoreCase)
$violations = [System.Collections.Generic.List[object]]::new()

foreach ($relativePathValue in $relativePaths) {
    $relativePath = ([string]$relativePathValue) -replace '\\', '/'
    if ([string]::IsNullOrWhiteSpace($relativePath) -or
        $relativePath -eq 'scripts/Test-NoUnpublishedCompatibilitySurface.ps1' -or
        $relativePath -like 'scripts/Test-*.ps1' -or
        $relativePath.StartsWith('tools/', [System.StringComparison]::OrdinalIgnoreCase) -or
        $relativePath.StartsWith('docs/api/', [System.StringComparison]::OrdinalIgnoreCase) -or
        $relativePath.StartsWith('docs/_site/', [System.StringComparison]::OrdinalIgnoreCase)) {
        continue
    }

    if ($forbiddenPattern.IsMatch($relativePath)) {
        $violations.Add([pscustomobject]@{
            Path = $relativePath
            Line = 0
            Text = 'Retired compatibility identity remains in a repository path.'
        })
        continue
    }

    $fullPath = Join-Path $repo ($relativePath -replace '/', [IO.Path]::DirectorySeparatorChar)
    if (-not (Test-Path -LiteralPath $fullPath -PathType Leaf)) {
        continue
    }

    $extension = [IO.Path]::GetExtension($fullPath)
    if (-not $textExtensions.Contains($extension) -and
        -not ([IO.Path]::GetFileName($fullPath)).Equals('CMakeLists.txt', [System.StringComparison]::OrdinalIgnoreCase)) {
        continue
    }

    $lineNumber = 0
    foreach ($line in [IO.File]::ReadLines($fullPath)) {
        $lineNumber++
        if ($forbiddenPattern.IsMatch($line)) {
            $violations.Add([pscustomobject]@{
                Path = $relativePath
                Line = $lineNumber
                Text = $line.Trim()
            })
        }
    }
}

if ($violations.Count -gt 0) {
    Write-Host "Unpublished compatibility surface guard failed with $($violations.Count) violation(s)."
    $violations | Sort-Object Path, Line | Format-Table Path, Line, Text -AutoSize
    exit 1
}

Write-Host "Unpublished compatibility surface guard passed."
Write-Host "Repository files inspected: $($relativePaths.Count)."

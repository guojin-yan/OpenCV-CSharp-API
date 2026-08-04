param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$sourceRoot = Join-Path $repo "src/OpenCvSharp"
$directoryBuildPropsPath = Join-Path $repo "Directory.Build.props"
if (-not (Test-Path -LiteralPath $sourceRoot -PathType Container)) {
    throw "Managed source root was not found: $sourceRoot"
}
if (-not (Test-Path -LiteralPath $directoryBuildPropsPath -PathType Leaf)) {
    throw "Directory.Build.props was not found: $directoryBuildPropsPath"
}

$violations = [System.Collections.Generic.List[string]]::new()
$files = @(
    Get-ChildItem -LiteralPath $sourceRoot -Recurse -File -Filter "*.cs" |
        Where-Object { $_.FullName -notmatch '\\(?:bin|obj)\\' } |
        Sort-Object FullName)
$namespaceCount = 0
$publicTypeCount = 0
$repositoryNamespaceCount = 0

$directoryBuildProps = [System.IO.File]::ReadAllText($directoryBuildPropsPath)
if (-not $directoryBuildProps.Contains('<OpenCvCSharpRootNamespace>JYPPX.OpenCvSharp</OpenCvCSharpRootNamespace>', [StringComparison]::Ordinal)) {
    $violations.Add('Directory.Build.props must define JYPPX.OpenCvSharp as the exact managed root namespace.')
}

$repositoryCodeFiles = @(
    foreach ($relativeRoot in @('src', 'samples', 'tests', 'tools')) {
        Get-ChildItem -LiteralPath (Join-Path $repo $relativeRoot) -Recurse -File -Filter '*.cs' |
            Where-Object { $_.FullName -notmatch '\\(?:bin|obj)\\' }
    }
) | Sort-Object FullName -Unique

foreach ($file in $repositoryCodeFiles) {
    $relativePath = [System.IO.Path]::GetRelativePath($repo, $file.FullName).Replace("\", "/")
    $lineNumber = 0
    foreach ($line in [System.IO.File]::ReadLines($file.FullName)) {
        $lineNumber++
        $namespaceMatch = [regex]::Match($line, '^\s*namespace\s+(?<name>[A-Za-z_][A-Za-z0-9_.]*)')
        if ($namespaceMatch.Success) {
            $repositoryNamespaceCount++
            $name = $namespaceMatch.Groups['name'].Value
            if ($name -ne 'JYPPX.OpenCvSharp' -and -not $name.StartsWith('JYPPX.OpenCvSharp.', [StringComparison]::Ordinal)) {
                $violations.Add("$relativePath`:$lineNumber namespace must remain under JYPPX.OpenCvSharp: $name")
            }
        }

        if ($line -match '^\s*(?:global\s+)?using\s+(?:static\s+)?OpenCvSharp(?:\.|\s*;)' -or $line -match 'global::OpenCvSharp\.') {
            $violations.Add("$relativePath`:$lineNumber references the retired unprefixed OpenCvSharp namespace.")
        }
    }
}

foreach ($file in $files) {
    $relativePath = [System.IO.Path]::GetRelativePath($repo, $file.FullName).Replace("\", "/")
    $lineNumber = 0
    foreach ($line in [System.IO.File]::ReadLines($file.FullName)) {
        $lineNumber++
        $namespaceMatch = [regex]::Match($line, '^\s*namespace\s+(?<name>[A-Za-z_][A-Za-z0-9_.]*)')
        if ($namespaceMatch.Success) {
            $namespaceCount++
            $name = $namespaceMatch.Groups['name'].Value
            if ($name -ne 'JYPPX.OpenCvSharp' -and -not $name.StartsWith('JYPPX.OpenCvSharp.', [StringComparison]::Ordinal)) {
                $violations.Add("$relativePath`:$lineNumber namespace must remain under JYPPX.OpenCvSharp: $name")
            }
        }

        if ($line -match '^\s*public\s+(?:(?:new|abstract|sealed|static|partial|readonly|unsafe)\s+)*(?:class|struct|enum|interface|record|delegate)\b') {
            $publicTypeCount++
        }
        if ($line -match '\bOpenCv[0-9]+Sharp\b') {
            $violations.Add("$relativePath`:$lineNumber contains a fixed-major managed identity.")
        }
    }
}

if ($violations.Count -gt 0) {
    Write-Host "Public API namespace neutrality guard failed with $($violations.Count) violation(s)."
    $violations | Sort-Object | ForEach-Object { Write-Host "- $_" }
    exit 1
}

Write-Host "Public API namespace neutrality guard passed."
Write-Host "Managed source files scanned: $($files.Count)."
Write-Host "Repository C# files scanned: $($repositoryCodeFiles.Count)."
Write-Host "Namespaces scanned: $namespaceCount."
Write-Host "Repository namespace declarations scanned: $repositoryNamespaceCount."
Write-Host "Public type declarations scanned: $publicTypeCount."

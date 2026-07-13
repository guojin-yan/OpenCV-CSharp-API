param(
    [string]$RepoRoot = ""
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = Join-Path $PSScriptRoot ".."
}

$repo = (Resolve-Path -LiteralPath $RepoRoot).Path

$ignoredDirectoryNames = @(
    ".git",
    "bin",
    "obj",
    "artifacts",
    "packages"
)

$ignoredRelativePrefixes = @(
    "docs/_site",
    "docs/api"
)

$checks = @(
    [pscustomobject]@{
        Name = "Managed package ID must stay version-neutral"
        Pattern = "<PackageId>\s*OpenCv5Sharp"
    },
    [pscustomobject]@{
        Name = "Managed assembly name must stay version-neutral"
        Pattern = "<AssemblyName>\s*OpenCv5Sharp"
    },
    [pscustomobject]@{
        Name = "Package references must use neutral package identity"
        Pattern = ("Package" + "Reference.*Open" + "Cv5Sharp")
    },
    [pscustomobject]@{
        Name = "Install docs must not recommend fixed-major OpenCv5Sharp package identity"
        Pattern = "dotnet\s+add\s+package\s+OpenCv5Sharp"
    },
    [pscustomobject]@{
        Name = "Runtime package identity must stay version-neutral"
        Pattern = "OpenCv5Sharp\.runtime|opencv5sharp\.runtime"
    },
    [pscustomobject]@{
        Name = "Repository/workspace paths must not use the retired fixed-major root"
        Pattern = "OpenCV-CSharp-API-opencv5\.x"
    }
)

$pathChecks = @(
    [pscustomobject]@{
        Name = "Repository paths must not use fixed-major managed identity"
        Pattern = "(^|/)OpenCv5Sharp($|[._/-])"
    },
    [pscustomobject]@{
        Name = "Repository paths must not use fixed-major OpenCV 5 shorthand"
        Pattern = "(^|/)opencv5(?:\.x)?($|[._/-])"
    },
    [pscustomobject]@{
        Name = "Repository paths must not use fixed-major native include identity"
        Pattern = "(^|/)open_cv_5_sharp($|/)"
    },
    [pscustomobject]@{
        Name = "Repository paths must not use fixed-major native ABI identity"
        Pattern = "(^|/)jyppx_ocv5[^/]*($|/)"
    }
)

$allowedFixedMajorRelativePathPrefixes = @(
    # Source-compatible include tree for existing native code that includes old wrapper headers.
    "src/OpenCvSharp.Native/include/open_cv_5_sharp"
)

$contentScanIgnoredRelativePrefixes = @(
    # Generated compatibility files are covered by ABI-specific tests and can contain many intentional legacy names.
    "src/OpenCvSharp.Native/generated",
    "src/OpenCvSharp.Native/include/open_cv_5_sharp"
)

function Get-RepositoryRelativePath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    $relative = [System.IO.Path]::GetRelativePath($repo, $Path)
    return $relative -replace "\\", "/"
}

function Test-IsIgnoredPath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    $relative = Get-RepositoryRelativePath -Path $Path
    $segments = $relative -split "/"
    foreach ($segment in $segments) {
        if ($ignoredDirectoryNames -contains $segment) {
            return $true
        }
    }

    foreach ($prefix in $ignoredRelativePrefixes) {
        if ($relative.StartsWith($prefix + "/", [System.StringComparison]::OrdinalIgnoreCase) -or
            $relative.Equals($prefix, [System.StringComparison]::OrdinalIgnoreCase)) {
            return $true
        }
    }

    return $false
}

function Test-IsAllowedFixedMajorPath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RelativePath
    )

    foreach ($prefix in $allowedFixedMajorRelativePathPrefixes) {
        if ($RelativePath.StartsWith($prefix + "/", [System.StringComparison]::OrdinalIgnoreCase) -or
            $RelativePath.Equals($prefix, [System.StringComparison]::OrdinalIgnoreCase)) {
            return $true
        }
    }

    return $false
}

function Test-IsIgnoredContentPath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RelativePath
    )

    foreach ($prefix in $contentScanIgnoredRelativePrefixes) {
        if ($RelativePath.StartsWith($prefix + "/", [System.StringComparison]::OrdinalIgnoreCase) -or
            $RelativePath.Equals($prefix, [System.StringComparison]::OrdinalIgnoreCase)) {
            return $true
        }
    }

    return $false
}

$violations = [System.Collections.Generic.List[object]]::new()
$regexOptions = [System.Text.RegularExpressions.RegexOptions]::IgnoreCase
$compiledChecks = foreach ($check in $checks) {
    [pscustomobject]@{
        Name = $check.Name
        Regex = [System.Text.RegularExpressions.Regex]::new($check.Pattern, $regexOptions)
    }
}

$compiledPathChecks = foreach ($check in $pathChecks) {
    [pscustomobject]@{
        Name = $check.Name
        Regex = [System.Text.RegularExpressions.Regex]::new($check.Pattern, $regexOptions)
    }
}

function Get-ScannableFiles {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Directory
    )

    foreach ($item in Get-ChildItem -LiteralPath $Directory -Force) {
        if ($item.PSIsContainer) {
            if (-not (Test-IsIgnoredPath -Path $item.FullName)) {
                Get-ScannableFiles -Directory $item.FullName
            }

            continue
        }

        if (-not (Test-IsIgnoredPath -Path $item.FullName)) {
            $item
        }
    }
}

function Get-ScannablePathItems {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Directory
    )

    foreach ($item in Get-ChildItem -LiteralPath $Directory -Force) {
        if (Test-IsIgnoredPath -Path $item.FullName) {
            continue
        }

        $item

        if ($item.PSIsContainer) {
            Get-ScannablePathItems -Directory $item.FullName
        }
    }
}

$pathItems = Get-ScannablePathItems -Directory $repo
foreach ($item in $pathItems) {
    $relativePath = Get-RepositoryRelativePath -Path $item.FullName
    if (Test-IsAllowedFixedMajorPath -RelativePath $relativePath) {
        continue
    }

    foreach ($check in $compiledPathChecks) {
        $pathMatch = $check.Regex.Match($relativePath)
        if ($pathMatch.Success) {
            $violations.Add([pscustomobject]@{
                Path = $relativePath
                Line = 0
                Match = $pathMatch.Value
                Text = "$($check.Name): $relativePath"
            })
        }
    }
}

$files = Get-ScannableFiles -Directory $repo

foreach ($file in $files) {
    $relativePath = Get-RepositoryRelativePath -Path $file.FullName
    if (Test-IsIgnoredContentPath -RelativePath $relativePath) {
        continue
    }

    $lineNumber = 0
    try {
        foreach ($line in [System.IO.File]::ReadLines($file.FullName)) {
            $lineNumber++
            foreach ($check in $compiledChecks) {
                $lineMatch = $check.Regex.Match($line)
                if ($lineMatch.Success) {
                    $violations.Add([pscustomobject]@{
                        Path = $relativePath
                        Line = $lineNumber
                        Match = $lineMatch.Value
                        Text = "$($check.Name): $($line.Trim())"
                    })
                }
            }
        }
    }
    catch [System.Text.DecoderFallbackException] {
        continue
    }
    catch [System.IO.IOException] {
        continue
    }
    catch [System.UnauthorizedAccessException] {
        continue
    }
}

if ($violations.Count -gt 0) {
    Write-Host "Version-neutral naming guard failed with $($violations.Count) suspicious fixed-major identity occurrence(s)."
    $violations |
        Sort-Object Path, Line, Match |
        Format-Table Path, Line, Match, Text -AutoSize
    exit 1
}

Write-Host "Version-neutral naming guard passed. Suspicious fixed-major content/path identity patterns: 0."

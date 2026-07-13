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

$fixedMajorManagedIdentity = "OpenCv" + "5Sharp"
$fixedMajorOpenCvShorthand = "opencv" + "5"
$fixedMajorRetiredRootPattern = "OpenCV-CSharp-API-opencv" + "5\.x"
$fixedMajorIncludeTree = "open_cv_" + "5_sharp"
$fixedMajorAbiPrefix = "jyppx_ocv" + "5_"

$checks = @(
    [pscustomobject]@{
        Name = "Managed package ID must stay version-neutral"
        Pattern = "<PackageId>\s*$fixedMajorManagedIdentity"
    },
    [pscustomobject]@{
        Name = "Managed assembly name must stay version-neutral"
        Pattern = "<AssemblyName>\s*$fixedMajorManagedIdentity"
    },
    [pscustomobject]@{
        Name = "Package references must use neutral package identity"
        Pattern = "PackageReference.*$fixedMajorManagedIdentity"
    },
    [pscustomobject]@{
        Name = "Install docs must not recommend fixed-major OpenCv5Sharp package identity"
        Pattern = "dotnet\s+add\s+package\s+$fixedMajorManagedIdentity"
    },
    [pscustomobject]@{
        Name = "Runtime package identity must stay version-neutral"
        Pattern = "$fixedMajorManagedIdentity\.runtime|$fixedMajorOpenCvShorthand" + "sharp\.runtime"
    },
    [pscustomobject]@{
        Name = "Repository/workspace paths must not use the retired fixed-major root"
        Pattern = $fixedMajorRetiredRootPattern
    }
)

$activeContentChecks = @(
    [pscustomobject]@{
        Name = "Active content must not import the retired OpenCv5Sharp namespace"
        Pattern = "(^|[`'\s])using\s+$fixedMajorManagedIdentity(?:[.;])"
        AllowCompatibilityPaths = $false
    },
    [pscustomobject]@{
        Name = "Active content must not declare the retired OpenCv5Sharp namespace"
        Pattern = "(^|[`'\s])namespace\s+$fixedMajorManagedIdentity(?:[.;\s{])"
        AllowCompatibilityPaths = $false
    },
    [pscustomobject]@{
        Name = "Active content must not reference retired OpenCv5Sharp module namespaces"
        Pattern = "$fixedMajorManagedIdentity\.(Core|ImgProc|ImgCodecs|Videoio|VideoIO|HighGui)\b"
        AllowCompatibilityPaths = $false
    },
    [pscustomobject]@{
        Name = "Active content must not introduce concrete fixed-major native ABI calls outside compatibility generation/tests"
        Pattern = "\b$fixedMajorAbiPrefix[A-Za-z0-9_]+"
        AllowCompatibilityPaths = $true
    }
)

$contextualFixedMajorContentChecks = @(
    [pscustomobject]@{
        Name = "Fixed-major native include tree references must be compatibility/generated/include-tree context only"
        Pattern = [System.Text.RegularExpressions.Regex]::Escape($fixedMajorIncludeTree)
    }
)

$pathChecks = @(
    [pscustomobject]@{
        Name = "Repository paths must not use fixed-major managed identity"
        Pattern = "(^|/)$fixedMajorManagedIdentity($|[._/-])"
    },
    [pscustomobject]@{
        Name = "Repository paths must not use fixed-major OpenCV 5 shorthand"
        Pattern = "(^|/)$fixedMajorOpenCvShorthand(?:\.x)?($|[._/-])"
    },
    [pscustomobject]@{
        Name = "Repository paths must not use fixed-major native include identity"
        Pattern = "(^|/)" + [System.Text.RegularExpressions.Regex]::Escape($fixedMajorIncludeTree) + "($|/)"
    },
    [pscustomobject]@{
        Name = "Repository paths must not use fixed-major native ABI identity"
        Pattern = "(^|/)$fixedMajorAbiPrefix" + "[^/]*($|/)"
    }
)

$allowedFixedMajorRelativePathPrefixes = @(
    # Source-compatible include tree for existing native code that includes old wrapper headers.
    "src/OpenCvSharp.Native/include/$fixedMajorIncludeTree"
)

$contentScanIgnoredRelativePrefixes = @(
    # Generated compatibility files are covered by ABI-specific tests and can contain many intentional legacy names.
    "src/OpenCvSharp.Native/generated",
    "src/OpenCvSharp.Native/include/$fixedMajorIncludeTree"
)

$allowedFixedMajorContentRelativePaths = @(
    # Compatibility generator and verification code intentionally contains legacy ABI/include spellings.
    "scripts/Generate-NativeAbiCompatibility.ps1",
    "scripts/Test-ManagedNativeInteropNeutrality.ps1",
    "scripts/Test-NativeAbiExports.ps1",
    "scripts/Test-NativeLegacyIncludeParity.ps1",
    "src/OpenCvSharp.Native/tests/legacy_source_compat_smoke.cpp"
)

$fixedMajorCompatibilityContextPattern = "compatib|legacy|existing|older|already-compiled|source-compatible|historical|retired|alias|facade|generated|include tree|allowlist|保留|兼容|旧|既有|已编译|历史|别名|生成"
$fixedMajorCompatibilityContextRegex = [System.Text.RegularExpressions.Regex]::new($fixedMajorCompatibilityContextPattern, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)

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

function Test-IsAllowedFixedMajorContentPath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RelativePath
    )

    foreach ($allowedPath in $allowedFixedMajorContentRelativePaths) {
        if ($RelativePath.Equals($allowedPath, [System.StringComparison]::OrdinalIgnoreCase)) {
            return $true
        }
    }

    return $false
}

function Test-HasFixedMajorCompatibilityContext {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Line
    )

    return $fixedMajorCompatibilityContextRegex.IsMatch($Line)
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

$compiledActiveContentChecks = foreach ($check in $activeContentChecks) {
    [pscustomobject]@{
        Name = $check.Name
        Regex = [System.Text.RegularExpressions.Regex]::new($check.Pattern, $regexOptions)
        AllowCompatibilityPaths = $check.AllowCompatibilityPaths
    }
}

$compiledContextualFixedMajorContentChecks = foreach ($check in $contextualFixedMajorContentChecks) {
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
$activeContentMatchCount = 0
$contextualFixedMajorContentReferenceCount = 0

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

            foreach ($check in $compiledActiveContentChecks) {
                $lineMatch = $check.Regex.Match($line)
                if (-not $lineMatch.Success) {
                    continue
                }

                $activeContentMatchCount++
                if ($check.AllowCompatibilityPaths -and (Test-IsAllowedFixedMajorContentPath -RelativePath $relativePath)) {
                    continue
                }

                $violations.Add([pscustomobject]@{
                    Path = $relativePath
                    Line = $lineNumber
                    Match = $lineMatch.Value
                    Text = "$($check.Name): $($line.Trim())"
                })
            }

            foreach ($check in $compiledContextualFixedMajorContentChecks) {
                $lineMatch = $check.Regex.Match($line)
                if (-not $lineMatch.Success) {
                    continue
                }

                $contextualFixedMajorContentReferenceCount++
                if ((Test-IsAllowedFixedMajorContentPath -RelativePath $relativePath) -or
                    (Test-HasFixedMajorCompatibilityContext -Line $line)) {
                    continue
                }

                $violations.Add([pscustomobject]@{
                    Path = $relativePath
                    Line = $lineNumber
                    Match = $lineMatch.Value
                    Text = "$($check.Name): $($line.Trim())"
                })
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
Write-Host "Allowed compatibility fixed-major active-content references: $activeContentMatchCount."
Write-Host "Contextual fixed-major include-tree references: $contextualFixedMajorContentReferenceCount."

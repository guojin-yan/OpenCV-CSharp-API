param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$preferredRuntimeProperty = "OpenCvNativeRuntimeDir"
$compatibilityRuntimeProperty = "Open" + "Cv5SharpNativeRuntimeDir"
$copyTargetName = "CopyOpenCvNativeRuntime"
$copyItemName = "OpenCvNativeRuntimeFiles"
$primaryNativeLoader = "JYPPX.OpenCV.Native.dll"
$compatibilityNativeLoader = "OpenCv5Sharp.Native.dll"

function Add-Violation {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.List[object]]$Violations,
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [Parameter(Mandatory = $true)]
        [string]$Issue,
        [string]$Text = ""
    )

    $Violations.Add([pscustomobject]@{
        Path = $Path
        Issue = $Issue
        Text = $Text.Trim()
    })
}

function Get-RepositoryRelativePath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    return [System.IO.Path]::GetRelativePath($repo, $Path).Replace("\", "/")
}

function Read-RequiredText {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RelativePath
    )

    $path = Join-Path $repo $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Required runtime native copy property file was not found: $RelativePath"
    }

    return [System.IO.File]::ReadAllText($path)
}

function Assert-Contains {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.List[object]]$Violations,
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [Parameter(Mandatory = $true)]
        [string]$Text,
        [Parameter(Mandatory = $true)]
        [string]$Needle,
        [Parameter(Mandatory = $true)]
        [string]$Issue
    )

    if ($Text.IndexOf($Needle, [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
        Add-Violation -Violations $Violations -Path $Path -Issue $Issue -Text $Needle
    }
}

function Get-ProjectAttribute {
    param(
        [Parameter(Mandatory = $true)]
        [System.Xml.XmlElement]$Element,
        [Parameter(Mandatory = $true)]
        [string]$Name
    )

    if ($Element.HasAttribute($Name)) {
        return $Element.GetAttribute($Name)
    }

    return ""
}

function Test-ContextualLegacyLine {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Line
    )

    return $Line -match "compatibility|alias|existing|older|legacy|already-compiled|兼容|别名|既有|旧|已编译"
}

function Test-PreferredLegacyLine {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Line
    )

    return $Line -match "prefer|preferred|current|primary|new build|new runtime|首选|优先|当前|主"
}

function Get-ProjectTargetFramework {
    param(
        [Parameter(Mandatory = $true)]
        [xml]$ProjectXml
    )

    $targetFramework = $ProjectXml.SelectSingleNode("//TargetFramework")
    if ($null -ne $targetFramework -and -not [string]::IsNullOrWhiteSpace($targetFramework.InnerText)) {
        return $targetFramework.InnerText.Trim()
    }

    $targetFrameworks = $ProjectXml.SelectSingleNode("//TargetFrameworks")
    if ($null -ne $targetFrameworks -and -not [string]::IsNullOrWhiteSpace($targetFrameworks.InnerText)) {
        $frameworks = @($targetFrameworks.InnerText.Split(";", [System.StringSplitOptions]::RemoveEmptyEntries) | ForEach-Object { $_.Trim() })
        if ($frameworks.Count -gt 0) {
            return $frameworks[0]
        }
    }

    return ""
}

function Test-IsPathUnder {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [Parameter(Mandatory = $true)]
        [string]$Root
    )

    $fullPath = [System.IO.Path]::GetFullPath($Path)
    $fullRoot = [System.IO.Path]::GetFullPath($Root).TrimEnd(
        [System.IO.Path]::DirectorySeparatorChar,
        [System.IO.Path]::AltDirectorySeparatorChar)
    $rootPrefix = $fullRoot + [System.IO.Path]::DirectorySeparatorChar

    return (
        $fullPath.Equals($fullRoot, [System.StringComparison]::OrdinalIgnoreCase) -or
        $fullPath.StartsWith($rootPrefix, [System.StringComparison]::OrdinalIgnoreCase))
}

function Remove-DirectoryIfSafe {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [Parameter(Mandatory = $true)]
        [string]$AllowedRoot
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Container)) {
        return
    }

    if (-not (Test-IsPathUnder -Path $Path -Root $AllowedRoot)) {
        throw "Refusing to remove path outside allowed root. Path: $Path; allowed root: $AllowedRoot"
    }

    Remove-Item -LiteralPath $Path -Recurse -Force
}

$violations = [System.Collections.Generic.List[object]]::new()
$validatedProjectFiles = [System.Collections.Generic.List[object]]::new()
$validatedAndroidProjectCount = 0

$projectFiles = @()
foreach ($root in @("samples", "tests")) {
    $rootPath = Join-Path $repo $root
    if (Test-Path -LiteralPath $rootPath -PathType Container) {
        $projectFiles += Get-ChildItem -LiteralPath $rootPath -Recurse -File -Filter "*.csproj" |
            Where-Object { $_.FullName -notmatch "\\(bin|obj)\\" }
    }
}
$projectFiles = @($projectFiles | Sort-Object FullName)

if ($projectFiles.Count -eq 0) {
    throw "No sample/test project files were found under samples/ or tests/."
}

foreach ($projectFile in $projectFiles) {
    $relativePath = Get-RepositoryRelativePath -Path $projectFile.FullName
    $text = [System.IO.File]::ReadAllText($projectFile.FullName)

    try {
        [xml]$xml = $text
    }
    catch {
        Add-Violation -Violations $violations -Path $relativePath -Issue "Project file must be valid XML" -Text $_.Exception.Message
        continue
    }

    $targetFramework = Get-ProjectTargetFramework -ProjectXml $xml
    if ($targetFramework.EndsWith("-android", [System.StringComparison]::OrdinalIgnoreCase)) {
        $androidNativeLibraries = @($xml.SelectNodes("//AndroidNativeLibrary"))
        $localRuntimeLibraries = @(
            $androidNativeLibraries |
                Where-Object {
                    ($_ -is [System.Xml.XmlElement]) -and
                    (Get-ProjectAttribute -Element $_ -Name "Include") -eq "`$($preferredRuntimeProperty)\*.so" -and
                    (Get-ProjectAttribute -Element $_ -Name "Condition") -eq "'`$($preferredRuntimeProperty)' != ''"
                })
        if ($localRuntimeLibraries.Count -ne 1) {
            Add-Violation -Violations $violations -Path $relativePath -Issue "Android sample must map local unversioned .so files through AndroidNativeLibrary and OpenCvNativeRuntimeDir"
        }
        else {
            $abis = @($localRuntimeLibraries[0].SelectNodes("./Abi") | ForEach-Object { $_.InnerText.Trim() } | Sort-Object -Unique)
            $expectedAbis = @("arm64-v8a", "armeabi-v7a", "x86", "x86_64")
            if (($abis -join ",") -ne ($expectedAbis -join ",")) {
                Add-Violation -Violations $violations -Path $relativePath -Issue "Android sample must map all four supported Android ABIs" -Text ($abis -join ",")
            }
        }
        if (@($xml.SelectNodes("//Target[@Name='$copyTargetName']")).Count -ne 0) {
            Add-Violation -Violations $violations -Path $relativePath -Issue "Android sample must use AndroidNativeLibrary packaging rather than the desktop CopyOpenCvNativeRuntime target"
        }
        $validatedAndroidProjectCount++
        continue
    }

    Assert-Contains -Violations $violations -Path $relativePath -Text $text -Needle "$preferredRuntimeProperty is the version-neutral runtime copy property" -Issue "$relativePath must document OpenCvNativeRuntimeDir as version-neutral"
    Assert-Contains -Violations $violations -Path $relativePath -Text $text -Needle "$compatibilityRuntimeProperty remains only as an existing-" -Issue "$relativePath must document OpenCv5SharpNativeRuntimeDir as existing compatibility only"
    Assert-Contains -Violations $violations -Path $relativePath -Text $text -Needle "compatibility alias bridge" -Issue "$relativePath must label the legacy property bridge as a compatibility alias bridge"

    $propertyNodes = @($xml.SelectNodes("//$preferredRuntimeProperty"))
    $aliasBridgeNodes = @(
        $propertyNodes |
            Where-Object {
                $condition = ""
                if ($_ -is [System.Xml.XmlElement]) {
                    $condition = Get-ProjectAttribute -Element $_ -Name "Condition"
                }

                $condition -eq "'`$($preferredRuntimeProperty)' == '' and '`$($compatibilityRuntimeProperty)' != ''" -and
                $_.InnerText -eq "`$($compatibilityRuntimeProperty)"
            }
    )

    if ($aliasBridgeNodes.Count -ne 1) {
        Add-Violation -Violations $violations -Path $relativePath -Issue "$relativePath must bridge OpenCv5SharpNativeRuntimeDir to OpenCvNativeRuntimeDir only when the preferred property is empty"
    }

    $copyTargets = @($xml.SelectNodes("//Target[@Name='$copyTargetName']"))

    if ($copyTargets.Count -ne 1) {
        Add-Violation -Violations $violations -Path $relativePath -Issue "$relativePath must contain exactly one $copyTargetName target"
        continue
    }

    $validatedProjectFiles.Add([pscustomobject]@{
        Path = $projectFile.FullName
        RelativePath = $relativePath
        TargetFramework = Get-ProjectTargetFramework -ProjectXml $xml
    })

    $target = $copyTargets[0]
    if ((Get-ProjectAttribute -Element $target -Name "AfterTargets") -ne "Build") {
        Add-Violation -Violations $violations -Path $relativePath -Issue "$copyTargetName must run AfterTargets=Build"
    }

    $targetCondition = Get-ProjectAttribute -Element $target -Name "Condition"
    foreach ($requiredCondition in @("'`$($preferredRuntimeProperty)' != ''", "'`$(TargetDir)' != ''")) {
        if ($targetCondition.IndexOf($requiredCondition, [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
            Add-Violation -Violations $violations -Path $relativePath -Issue "$copyTargetName condition must require $requiredCondition" -Text $targetCondition
        }
    }

    if ($targetCondition.IndexOf($compatibilityRuntimeProperty, [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
        Add-Violation -Violations $violations -Path $relativePath -Issue "$copyTargetName target condition must use only the preferred OpenCvNativeRuntimeDir property"
    }

    $runtimeFileItems = @($target.SelectNodes(".//$copyItemName"))
    $matchingRuntimeItems = @(
        $runtimeFileItems |
            Where-Object {
                ($_ -is [System.Xml.XmlElement]) -and
                (Get-ProjectAttribute -Element $_ -Name "Include") -eq "`$($preferredRuntimeProperty)\*.dll"
            }
    )
    if ($matchingRuntimeItems.Count -ne 1) {
        Add-Violation -Violations $violations -Path $relativePath -Issue "$copyTargetName must include runtime DLLs from `$($preferredRuntimeProperty)\*.dll"
    }

    $copyNodes = @($target.SelectNodes(".//Copy"))
    $matchingCopyNodes = @(
        $copyNodes |
            Where-Object {
                ($_ -is [System.Xml.XmlElement]) -and
                (Get-ProjectAttribute -Element $_ -Name "SourceFiles") -eq "@($copyItemName)" -and
                (Get-ProjectAttribute -Element $_ -Name "DestinationFolder") -eq "`$(TargetDir)"
            }
    )
    if ($matchingCopyNodes.Count -ne 1) {
        Add-Violation -Violations $violations -Path $relativePath -Issue "$copyTargetName must copy @($copyItemName) into `$(TargetDir)"
    }

    $lineNumber = 0
    foreach ($line in [System.IO.File]::ReadLines($projectFile.FullName)) {
        $lineNumber++
        if ($line.IndexOf($compatibilityRuntimeProperty, [System.StringComparison]::OrdinalIgnoreCase) -ge 0 -and
            -not (Test-ContextualLegacyLine -Line $line)) {
            Add-Violation -Violations $violations -Path $relativePath -Issue "$compatibilityRuntimeProperty on line $lineNumber must be compatibility-only" -Text $line
        }

        if ($line.IndexOf($compatibilityRuntimeProperty, [System.StringComparison]::OrdinalIgnoreCase) -ge 0 -and
            (Test-PreferredLegacyLine -Line $line) -and
            -not (Test-ContextualLegacyLine -Line $line)) {
            Add-Violation -Violations $violations -Path $relativePath -Issue "$compatibilityRuntimeProperty on line $lineNumber must not be described as preferred/current" -Text $line
        }
    }
}

$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if ($null -eq $dotnet) {
    Add-Violation -Violations $violations -Path "dotnet" -Issue "dotnet was not found; runtime native copy dry-run validation cannot execute MSBuild target"
}
else {
    $tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("opencv-csharp-runtime-copy-dry-run-" + [System.Guid]::NewGuid().ToString("N"))
    $nativeRuntimeDir = Join-Path $tempRoot "native-runtime"
    $syntheticDllNames = @(
        $primaryNativeLoader,
        $compatibilityNativeLoader,
        "opencv_core500.dll"
    )
    $nonDllFileName = "do-not-copy.txt"

    try {
        New-Item -ItemType Directory -Force -Path $nativeRuntimeDir | Out-Null
        foreach ($dllName in $syntheticDllNames) {
            [System.IO.File]::WriteAllBytes((Join-Path $nativeRuntimeDir $dllName), [byte[]](0x4D, 0x5A, 0x00, 0x00))
        }
        [System.IO.File]::WriteAllText((Join-Path $nativeRuntimeDir $nonDllFileName), "not a runtime dll")

        foreach ($project in $validatedProjectFiles) {
            $projectDirectory = Split-Path -Parent $project.Path
            $binPath = Join-Path $projectDirectory "bin"
            $objPath = Join-Path $projectDirectory "obj"
            $binExistedBefore = Test-Path -LiteralPath $binPath
            $objExistedBefore = Test-Path -LiteralPath $objPath
            $projectOutputDir = Join-Path $tempRoot ("out-" + ($project.RelativePath -replace "[^A-Za-z0-9_.-]", "_"))
            New-Item -ItemType Directory -Force -Path $projectOutputDir | Out-Null
            $targetDir = $projectOutputDir.TrimEnd("\", "/") + [System.IO.Path]::DirectorySeparatorChar

            $arguments = @(
                "msbuild",
                $project.Path,
                "/nologo",
                "/t:$copyTargetName",
                "/p:$preferredRuntimeProperty=$nativeRuntimeDir",
                "/p:TargetDir=$targetDir",
                "/p:Configuration=Release",
                "/v:minimal"
            )

            if (-not [string]::IsNullOrWhiteSpace($project.TargetFramework)) {
                $arguments += "/p:TargetFramework=$($project.TargetFramework)"
            }

            $msbuildOutput = & $dotnet.Source @arguments 2>&1
            if ($LASTEXITCODE -ne 0) {
                Add-Violation -Violations $violations -Path $project.RelativePath -Issue "MSBuild dry-run target $copyTargetName failed" -Text ($msbuildOutput -join " ")
                continue
            }

            foreach ($dllName in $syntheticDllNames) {
                if (-not (Test-Path -LiteralPath (Join-Path $projectOutputDir $dllName) -PathType Leaf)) {
                    Add-Violation -Violations $violations -Path $project.RelativePath -Issue "MSBuild dry-run did not copy synthetic DLL $dllName"
                }
            }

            if (Test-Path -LiteralPath (Join-Path $projectOutputDir $nonDllFileName) -PathType Leaf) {
                Add-Violation -Violations $violations -Path $project.RelativePath -Issue "MSBuild dry-run copied non-DLL file $nonDllFileName"
            }

            if (-not $binExistedBefore -and (Test-Path -LiteralPath $binPath)) {
                Add-Violation -Violations $violations -Path $project.RelativePath -Issue "MSBuild dry-run unexpectedly created project bin directory" -Text $binPath
                Remove-DirectoryIfSafe -Path $binPath -AllowedRoot $projectDirectory
            }

            if (-not $objExistedBefore -and (Test-Path -LiteralPath $objPath)) {
                Add-Violation -Violations $violations -Path $project.RelativePath -Issue "MSBuild dry-run unexpectedly created project obj directory" -Text $objPath
                Remove-DirectoryIfSafe -Path $objPath -AllowedRoot $projectDirectory
            }
        }
    }
    finally {
        Remove-DirectoryIfSafe -Path $tempRoot -AllowedRoot ([System.IO.Path]::GetTempPath())
    }
}

$runUnstablePath = "scripts/Run-UnstableSmoke.ps1"
$runUnstableText = Read-RequiredText -RelativePath $runUnstablePath
Assert-Contains -Violations $violations -Path $runUnstablePath -Text $runUnstableText -Needle "# OpenCvNativeRuntimeDir is the preferred version-neutral runtime path/build property passed through to MSBuild." -Issue "Run-UnstableSmoke must document OpenCvNativeRuntimeDir as the preferred build property"
Assert-Contains -Violations $violations -Path $runUnstablePath -Text $runUnstableText -Needle '[string]$OpenCvNativeRuntimeDir = ""' -Issue "Run-UnstableSmoke must expose OpenCvNativeRuntimeDir"
Assert-Contains -Violations $violations -Path $runUnstablePath -Text $runUnstableText -Needle '$buildArguments += "/p:OpenCvNativeRuntimeDir=$OpenCvNativeRuntimeDir"' -Issue "Run-UnstableSmoke must pass OpenCvNativeRuntimeDir through to dotnet build"
Assert-Contains -Violations $violations -Path $runUnstablePath -Text $runUnstableText -Needle 'OpenCvNativeRuntimeDir is only applied during the build step; -NoBuild assumes the test output is already staged.' -Issue "Run-UnstableSmoke must warn when -NoBuild prevents applying OpenCvNativeRuntimeDir"
Assert-Contains -Violations $violations -Path $runUnstablePath -Text $runUnstableText -Needle '"--no-build"' -Issue "Run-UnstableSmoke must run dotnet test with --no-build after the build/property propagation step"
if ($runUnstableText.IndexOf($compatibilityRuntimeProperty, [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
    Add-Violation -Violations $violations -Path $runUnstablePath -Issue "Run-UnstableSmoke must not expose the legacy runtime copy property as a script parameter"
}

foreach ($doc in @(
        [pscustomobject]@{ Path = "README.md"; Text = Read-RequiredText -RelativePath "README.md"; Needle = 'point local samples/tests at it with `OpenCvNativeRuntimeDir`' },
        [pscustomobject]@{ Path = "docs/articles/quick-start.md"; Text = Read-RequiredText -RelativePath "docs/articles/quick-start.md"; Needle = 'use `OpenCvNativeRuntimeDir` for local builds' },
        [pscustomobject]@{ Path = "docs/articles/linked-runtime-build-guide.md"; Text = Read-RequiredText -RelativePath "docs/articles/linked-runtime-build-guide.md"; Needle = 'pass the staged native output through `OpenCvNativeRuntimeDir`' },
        [pscustomobject]@{ Path = "docs/articles/smoke-profiles-guide.md"; Text = Read-RequiredText -RelativePath "docs/articles/smoke-profiles-guide.md"; Needle = 'tests or samples with `OpenCvNativeRuntimeDir`' },
        [pscustomobject]@{ Path = "docs/articles/linked-runtime-smoke-guide.md"; Text = Read-RequiredText -RelativePath "docs/articles/linked-runtime-smoke-guide.md"; Needle = "/p:OpenCvNativeRuntimeDir=<runtime-native-dir>" })) {
    Assert-Contains -Violations $violations -Path $doc.Path -Text $doc.Text -Needle $doc.Needle -Issue "$($doc.Path) must align local native runtime fallback docs with the MSBuild copy property"
}

$linkedRuntimeSmokeText = Read-RequiredText -RelativePath "docs/articles/linked-runtime-smoke-guide.md"
Assert-Contains -Violations $violations -Path "docs/articles/linked-runtime-smoke-guide.md" -Text $linkedRuntimeSmokeText -Needle "/p:OpenCv5SharpNativeRuntimeDir=<runtime-native-dir>" -Issue "Linked runtime smoke guide must document the legacy property only as compatibility"
Assert-Contains -Violations $violations -Path "docs/articles/linked-runtime-smoke-guide.md" -Text $linkedRuntimeSmokeText -Needle "existing-test-build-script compatibility alias" -Issue "Linked runtime smoke guide must label OpenCv5SharpNativeRuntimeDir as compatibility-only"
Assert-Contains -Violations $violations -Path "docs/articles/version-neutral-naming-guide.md" -Text (Read-RequiredText -RelativePath "docs/articles/version-neutral-naming-guide.md") -Needle "Test-RuntimeNativeCopyPropertyPropagation.ps1" -Issue "Version-neutral naming guide must list runtime native copy property propagation guard"
Assert-Contains -Violations $violations -Path "scripts/Test-ProjectInvariants.ps1" -Text (Read-RequiredText -RelativePath "scripts/Test-ProjectInvariants.ps1") -Needle "Test-RuntimeNativeCopyPropertyPropagation.ps1" -Issue "Aggregate invariant suite must include runtime native copy property propagation guard"

foreach ($doc in @(
        [pscustomobject]@{ Path = "README.md"; Text = Read-RequiredText -RelativePath "README.md" },
        [pscustomobject]@{ Path = "docs/articles/linked-runtime-smoke-guide.md"; Text = $linkedRuntimeSmokeText },
        [pscustomobject]@{ Path = "docs/articles/version-neutral-naming-guide.md"; Text = Read-RequiredText -RelativePath "docs/articles/version-neutral-naming-guide.md" },
        [pscustomobject]@{ Path = "CONTRIBUTING.md"; Text = Read-RequiredText -RelativePath "CONTRIBUTING.md" })) {
    foreach ($line in $doc.Text -split "\r?\n") {
        if ($line.IndexOf($compatibilityNativeLoader, [System.StringComparison]::OrdinalIgnoreCase) -ge 0 -and
            -not (Test-ContextualLegacyLine -Line $line)) {
            Add-Violation -Violations $violations -Path $doc.Path -Issue "$compatibilityNativeLoader mentions must be explicitly compatibility-scoped" -Text $line
        }
    }
}

if ($violations.Count -gt 0) {
    Write-Host "Runtime native copy property propagation guard failed with $($violations.Count) violation(s)."
    $violations |
        Sort-Object Path, Issue |
        Format-Table Path, Issue, Text -AutoSize
    exit 1
}

Write-Host "Runtime native copy property propagation guard passed."
Write-Host "Sample/test project files checked: $($projectFiles.Count)."
Write-Host "MSBuild copy dry-run project files checked: $($validatedProjectFiles.Count)."
Write-Host "AndroidNativeLibrary project files checked: $validatedAndroidProjectCount."
Write-Host "Preferred runtime copy property: $preferredRuntimeProperty."
Write-Host "Compatibility runtime copy property: $compatibilityRuntimeProperty."

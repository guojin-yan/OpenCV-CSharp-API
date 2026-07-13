param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$primaryManagedPackageId = "JYPPX.OpenCV.CSharp.API"
$runtimePackagePrefix = "JYPPX.OpenCV.runtime"
$primaryNativeLoader = "JYPPX.OpenCV.Native.dll"
$compatibilityNativeLoader = "OpenCv5Sharp.Native.dll"
$preferredRuntimeCopyProperty = "OpenCvNativeRuntimeDir"
$compatibilityRuntimeCopyProperty = "OpenCv5SharpNativeRuntimeDir"

function Get-RepositoryRelativePath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    return [System.IO.Path]::GetRelativePath($repo, $Path).Replace("\", "/")
}

function Add-Violation {
    param(
        [System.Collections.Generic.List[object]]$Violations,
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [Parameter(Mandatory = $true)]
        [string]$Issue
    )

    $Violations.Add([pscustomobject]@{
        Path = $Path
        Issue = $Issue
    })
}

function Read-RequiredText {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RelativePath
    )

    $path = Join-Path $repo $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Required runtime packaging file was not found: $path"
    }

    return [System.IO.File]::ReadAllText($path)
}

function Test-Contains {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Text,
        [Parameter(Mandatory = $true)]
        [string]$Needle
    )

    return $Text.IndexOf($Needle, [System.StringComparison]::Ordinal) -ge 0
}

$violations = [System.Collections.Generic.List[object]]::new()

$stageRuntimePath = "scripts/Stage-Runtime.ps1"
$packRuntimePath = "scripts/Pack-Runtime.ps1"
$packManagedPath = "scripts/Pack-Managed.ps1"
$stageRuntimeText = Read-RequiredText -RelativePath $stageRuntimePath
$packRuntimeText = Read-RequiredText -RelativePath $packRuntimePath
$packManagedText = Read-RequiredText -RelativePath $packManagedPath

if (-not (Test-Contains -Text $stageRuntimeText -Needle "`"$primaryNativeLoader`"")) {
    Add-Violation $violations $stageRuntimePath "Stage-Runtime must name $primaryNativeLoader as the Windows primary loader"
}

if (-not (Test-Contains -Text $stageRuntimeText -Needle '"Cv5Sharp.Native" # compatibility loader')) {
    Add-Violation $violations $stageRuntimePath "Stage-Runtime must keep $compatibilityNativeLoader only as an explicit compatibility copy"
}

if (-not (Test-Contains -Text $stageRuntimeText -Needle '"libJYPPX.OpenCV.Native.so"')) {
    Add-Violation $violations $stageRuntimePath "Stage-Runtime must name libJYPPX.OpenCV.Native.so as the non-Windows primary loader"
}

foreach ($needle in @(
        "(Join-Path `$nativeRuntimePath `$primaryNativeLoaderFileName)",
        "(Join-Path `$nativeRuntimePath `$compatibilityNativeLoaderCopyFileName)")) {
    if (-not (Test-Contains -Text $stageRuntimeText -Needle $needle)) {
        Add-Violation $violations $stageRuntimePath "Stage-Runtime runtimeFiles must include '$needle'"
    }
}

if ($stageRuntimeText -match "OpenCv5Sharp\.runtime|opencv5sharp\.runtime") {
    Add-Violation $violations $stageRuntimePath "Stage-Runtime must not use a fixed-major runtime package identity"
}

if (-not (Test-Contains -Text $packRuntimeText -Needle '$runtimePackageId = "$runtimePackagePrefix.$Rid$runtimePackageSuffix"')) {
    Add-Violation $violations $packRuntimePath "Pack-Runtime must derive runtime package ID from $runtimePackagePrefix"
}

if (-not (Test-Contains -Text $packRuntimeText -Needle "`"-p:PackageId=`$runtimePackageId`"")) {
    Add-Violation $violations $packRuntimePath "Pack-Runtime must pass the derived neutral runtime package ID to dotnet pack"
}

if ($packRuntimeText -match "OpenCv5Sharp\.runtime|opencv5sharp\.runtime") {
    Add-Violation $violations $packRuntimePath "Pack-Runtime must not use a fixed-major runtime package identity"
}

if (-not (Test-Contains -Text $packManagedText -Needle "OpenCvCSharpManagedPackageId") -or
    -not (Test-Contains -Text $packManagedText -Needle "`$managedPackageId = Get-RequiredDirectoryBuildProperty")) {
    Add-Violation $violations $packManagedPath "Pack-Managed must derive the version-neutral managed package ID from Directory.Build.props"
}

$runtimeProjectFiles = @(
    Get-ChildItem -LiteralPath (Join-Path $repo "packaging/runtime") -Recurse -File -Filter "*.csproj" |
        Where-Object { $_.FullName -notmatch "\\(bin|obj)\\" } |
        Sort-Object FullName
)

if ($runtimeProjectFiles.Count -eq 0) {
    throw "No runtime package project files were found under packaging/runtime"
}

foreach ($projectFile in $runtimeProjectFiles) {
    $relativePath = Get-RepositoryRelativePath -Path $projectFile.FullName
    $text = [System.IO.File]::ReadAllText($projectFile.FullName)

    if ($text -notmatch "<PackageId>\s*(?:JYPPX\.OpenCV\.runtime|\$\(OpenCvCSharpRuntimePackageIdPrefix\))\.\$\(RuntimePackageRid\)\$\(RuntimePackageProfileSuffix\)\s*</PackageId>") {
        Add-Violation $violations $relativePath "Runtime package project PackageId must be JYPPX.OpenCV.runtime.`$(RuntimePackageRid)`$(RuntimePackageProfileSuffix) or `$(OpenCvCSharpRuntimePackageIdPrefix).`$(RuntimePackageRid)`$(RuntimePackageProfileSuffix)"
    }

    if ($text -notmatch "<RuntimePackageRid\b[^>]*>\s*win-x64\s*</RuntimePackageRid>") {
        Add-Violation $violations $relativePath "Runtime package project should define a default RuntimePackageRid"
    }

    if ($text -notmatch "runtimes/\$\(RuntimePackageRid\)/native/\*\*/\*") {
        Add-Violation $violations $relativePath "Runtime package project must pack runtimes/`$(RuntimePackageRid)/native/**/*"
    }

    if ($text -match "OpenCv5Sharp|opencv5sharp") {
        Add-Violation $violations $relativePath "Runtime package project metadata must not contain fixed-major OpenCv5Sharp identities"
    }
}

$runtimeReadmeFiles = @(
    Get-ChildItem -LiteralPath (Join-Path $repo "packaging/runtime") -Recurse -File -Filter "README.md" |
        Where-Object { $_.FullName -notmatch "\\(bin|obj)\\" } |
        Sort-Object FullName
)

foreach ($readmeFile in $runtimeReadmeFiles) {
    $relativePath = Get-RepositoryRelativePath -Path $readmeFile.FullName
    $text = [System.IO.File]::ReadAllText($readmeFile.FullName)

    foreach ($requiredText in @($runtimePackagePrefix, $primaryNativeLoader, $compatibilityNativeLoader)) {
        if (-not (Test-Contains -Text $text -Needle $requiredText)) {
            Add-Violation $violations $relativePath "Runtime README must mention $requiredText"
        }
    }

    if ($text -match "OpenCv5Sharp\.runtime|opencv5sharp\.runtime") {
        Add-Violation $violations $relativePath "Runtime README must not describe a fixed-major runtime package identity"
    }
}

$copyProjectFiles = @(
    Get-ChildItem -LiteralPath (Join-Path $repo "samples") -Recurse -File -Filter "*.csproj" |
        Where-Object { $_.FullName -notmatch "\\(bin|obj)\\" }
    Get-ChildItem -LiteralPath (Join-Path $repo "tests") -Recurse -File -Filter "*.csproj" |
        Where-Object { $_.FullName -notmatch "\\(bin|obj)\\" }
) | Sort-Object FullName

foreach ($projectFile in $copyProjectFiles) {
    $relativePath = Get-RepositoryRelativePath -Path $projectFile.FullName
    $text = [System.IO.File]::ReadAllText($projectFile.FullName)

    if ($text.Contains($compatibilityRuntimeCopyProperty)) {
        if (-not $text.Contains($preferredRuntimeCopyProperty)) {
            Add-Violation $violations $relativePath "Compatibility runtime copy property requires preferred $preferredRuntimeCopyProperty"
        }

        if ($text -notmatch "$([regex]::Escape($compatibilityRuntimeCopyProperty)).*(compatibility alias|兼容别名|compatibility)") {
            Add-Violation $violations $relativePath "$compatibilityRuntimeCopyProperty must be documented as a compatibility alias"
        }
    }
}

$loaderMentionFiles = @(
    Get-ChildItem -LiteralPath $repo -Recurse -File |
        Where-Object {
            $_.FullName -notmatch "\\(\.git|bin|obj|artifacts|packages)\\" -and
            $_.FullName -notmatch "\\src\\OpenCvSharp\.Native\\generated\\" -and
            $_.FullName -notmatch "\\src\\OpenCvSharp\.Native\\include\\open_cv_5_sharp\\" # generated compatibility include tree
        } |
        Sort-Object FullName
)

foreach ($file in $loaderMentionFiles) {
    $relativePath = Get-RepositoryRelativePath -Path $file.FullName
    try {
        $lines = [System.IO.File]::ReadAllLines($file.FullName)
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

    for ($index = 0; $index -lt $lines.Count; $index++) {
        $line = $lines[$index]
        if ($line.Contains($compatibilityNativeLoader) -and
            $line -notmatch "compatibility|兼容|already-compiled|已编译|Compatibility") {
            Add-Violation $violations $relativePath "$compatibilityNativeLoader mention on line $($index + 1) must be explicitly compatibility-scoped"
        }
    }
}

$stagedNativeDirs = @(
    Get-ChildItem -LiteralPath $repo -Recurse -Directory -Force |
        Where-Object {
            $_.FullName -notmatch "\\(\.git|bin|obj|packages)\\" -and
            $_.Name -eq "native" -and
            $_.Parent -and
            $_.Parent.Parent -and
            $_.Parent.Parent.Name -eq "runtimes"
        }
)

foreach ($nativeDir in $stagedNativeDirs) {
    $dlls = @(Get-ChildItem -LiteralPath $nativeDir.FullName -File -Filter "*.dll")
    if ($dlls.Count -eq 0) {
        continue
    }

    $names = [System.Collections.Generic.HashSet[string]]::new(
        [System.StringComparer]::OrdinalIgnoreCase)
    foreach ($dll in $dlls) {
        [void]$names.Add($dll.Name)
    }

    $relativePath = Get-RepositoryRelativePath -Path $nativeDir.FullName
    if (-not $names.Contains($primaryNativeLoader)) {
        Add-Violation $violations $relativePath "Staged runtime native directory with DLLs must include primary loader $primaryNativeLoader"
    }

    if (-not $names.Contains($compatibilityNativeLoader)) {
        Add-Violation $violations $relativePath "Staged runtime native directory with DLLs must include explicit compatibility loader copy $compatibilityNativeLoader"
    }
}

if ($violations.Count -gt 0) {
    Write-Host "Runtime package neutrality guard failed with $($violations.Count) issue(s)."
    $violations |
        Sort-Object Path, Issue |
        Format-Table Path, Issue -AutoSize
    exit 1
}

Write-Host (
    "Runtime package neutrality guard passed. " +
    "Runtime projects: $($runtimeProjectFiles.Count); " +
    "runtime README files: $($runtimeReadmeFiles.Count); " +
    "sample/test copy projects: $($copyProjectFiles.Count); " +
    "staged native dirs with DLLs checked: $(@($stagedNativeDirs | Where-Object { @(Get-ChildItem -LiteralPath $_.FullName -File -Filter '*.dll').Count -gt 0 }).Count).")

param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$nativeCMakePath = "src/OpenCvSharp.Native/CMakeLists.txt"
$nativeRootPath = "src/OpenCvSharp.Native"
$buildNativeScriptPath = "scripts/Build-Native.ps1"
$nativeWorkflowPath = ".github/workflows/build-native.yml"
$readmePath = "README.md"
$contributingPath = "CONTRIBUTING.md"
$versionNeutralGuidePath = "docs/articles/version-neutral-naming-guide.md"
$nativeModuleBoundaryPath = "docs/articles/native-module-boundary.md"
$regexOptions = [System.Text.RegularExpressions.RegexOptions]::IgnoreCase -bor [System.Text.RegularExpressions.RegexOptions]::Multiline

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

function Read-RequiredText {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RelativePath
    )

    $path = Join-Path $repo $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Required native CMake boundary file was not found: $RelativePath"
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

function Assert-Matches {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.List[object]]$Violations,
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [Parameter(Mandatory = $true)]
        [string]$Text,
        [Parameter(Mandatory = $true)]
        [string]$Pattern,
        [Parameter(Mandatory = $true)]
        [string]$Issue
    )

    if (-not [regex]::IsMatch($Text, $Pattern, $regexOptions)) {
        Add-Violation -Violations $Violations -Path $Path -Issue $Issue -Text $Pattern
    }
}

$violations = [System.Collections.Generic.List[object]]::new()

$nativeCMakeText = Read-RequiredText -RelativePath $nativeCMakePath
$buildNativeScriptText = Read-RequiredText -RelativePath $buildNativeScriptPath
$nativeWorkflowText = Read-RequiredText -RelativePath $nativeWorkflowPath
$readmeText = Read-RequiredText -RelativePath $readmePath
$contributingText = Read-RequiredText -RelativePath $contributingPath
$versionNeutralGuideText = Read-RequiredText -RelativePath $versionNeutralGuidePath
$nativeModuleBoundaryText = Read-RequiredText -RelativePath $nativeModuleBoundaryPath

foreach ($forbidden in @(
        [pscustomobject]@{
            Pattern = "^\s*install\s*\("
            Issue = "Native CMake project must not install files or targets until a public CMake package/SDK boundary is deliberately designed"
        },
        [pscustomobject]@{
            Pattern = "^\s*export\s*\("
            Issue = "Native CMake project must not export targets until a public CMake package/SDK boundary is deliberately designed"
        },
        [pscustomobject]@{
            Pattern = "configure_package_config_file\s*\("
            Issue = "Native CMake project must not generate package config files today"
        },
        [pscustomobject]@{
            Pattern = "write_basic_package_version_file\s*\("
            Issue = "Native CMake project must not generate package version files today"
        },
        [pscustomobject]@{
            Pattern = "CMakePackageConfigHelpers"
            Issue = "Native CMake project must not include CMake package config helpers today"
        },
        [pscustomobject]@{
            Pattern = "\$<INSTALL_INTERFACE:"
            Issue = "Native CMake include directories must not advertise an install interface today"
        })) {
    if ([regex]::IsMatch($nativeCMakeText, $forbidden.Pattern, $regexOptions)) {
        Add-Violation -Violations $violations -Path $nativeCMakePath -Issue $forbidden.Issue -Text $forbidden.Pattern
    }
}

$nativeRoot = Join-Path $repo $nativeRootPath
if (-not (Test-Path -LiteralPath $nativeRoot -PathType Container)) {
    Add-Violation -Violations $violations -Path $nativeRootPath -Issue "Native source root must exist"
}
else {
    $cmakePackageFiles = @(
        Get-ChildItem -LiteralPath $nativeRoot -Recurse -File |
            Where-Object {
                $_.Name -match "(?i)(Config\.cmake|ConfigVersion\.cmake|Targets\.cmake|\.cmake\.in$)"
            } |
            ForEach-Object { ([System.IO.Path]::GetRelativePath($repo, $_.FullName)) -replace "\\", "/" }
    )

    foreach ($packageFile in $cmakePackageFiles) {
        Add-Violation `
            -Violations $violations `
            -Path $packageFile `
            -Issue "Native source tree must not contain project-owned CMake package config/export files today"
    }
}

foreach ($required in @(
        [pscustomobject]@{
            Needle = 'set(OPENCV_CSHARP_NATIVE_TARGET "JYPPX.OpenCV.Native")'
            Issue = "Native CMake target must keep the version-neutral primary target name"
        },
        [pscustomobject]@{
            Needle = 'set(OPENCV_CSHARP_COMPATIBILITY_NATIVE_TARGET "OpenCv5Sharp.Native")'
            Issue = "Native CMake compatibility target alias must remain explicit"
        },
        [pscustomobject]@{
            Needle = 'add_library(${OPENCV_CSHARP_COMPATIBILITY_NATIVE_TARGET} ALIAS ${OPENCV_CSHARP_NATIVE_TARGET})'
            Issue = "Compatibility target must be an alias to the primary native target, not a second implementation"
        },
        [pscustomobject]@{
            Needle = "JYPPX.OpenCV.Native is the version-neutral primary native target/output"
            Issue = "Primary native target comment must document neutral-first status"
        },
        [pscustomobject]@{
            Needle = "OpenCv5Sharp.Native remains only as a compatibility target"
            Issue = "Compatibility target comment must document compatibility-only status"
        },
        [pscustomobject]@{
            Needle = "OPENCV_CSHARP_* variables primary and OPENCV5SHARP_* variables only as existing-build-script compatibility aliases"
            Issue = "Native CMake comments must document primary and compatibility variable roles"
        },
        [pscustomobject]@{
            Needle = '"LINKER:-z,max-page-size=16384"'
            Issue = "Android native loader must retain 16 KB maximum page-size alignment"
        },
        [pscustomobject]@{
            Needle = '"LINKER:-z,common-page-size=16384"'
            Issue = "Android native loader must retain 16 KB common page-size alignment"
        })) {
    Assert-Contains `
        -Violations $violations `
        -Path $nativeCMakePath `
        -Text $nativeCMakeText `
        -Needle $required.Needle `
        -Issue $required.Issue
}

foreach ($requiredPattern in @(
        [pscustomobject]@{
            Pattern = 'set\(OPENCV5SHARP_BUILD_WITH_OPENCV\s+"\$\{OPENCV_CSHARP_BUILD_WITH_OPENCV\}"\s+CACHE\s+BOOL\s+"Existing-build-script compatibility alias'
            Issue = "OPENCV5SHARP_BUILD_WITH_OPENCV must remain only an existing-build-script compatibility alias"
        },
        [pscustomobject]@{
            Pattern = 'set\(OPENCV5SHARP_OPENCV_DIR\s+"\$\{OPENCV_CSHARP_OPENCV_DIR\}"\s+CACHE\s+PATH\s+"Existing-build-script compatibility alias'
            Issue = "OPENCV5SHARP_OPENCV_DIR must remain only an existing-build-script compatibility alias"
        },
        [pscustomobject]@{
            Pattern = 'set\(OPENCV5SHARP_OPENCV_BUILD_LIST\s+"\$\{OPENCV_CSHARP_OPENCV_BUILD_LIST\}"\s+CACHE\s+STRING\s+"Existing-build-script compatibility alias'
            Issue = "OPENCV5SHARP_OPENCV_BUILD_LIST must remain only an existing-build-script compatibility alias"
        })) {
    Assert-Matches `
        -Violations $violations `
        -Path $nativeCMakePath `
        -Text $nativeCMakeText `
        -Pattern $requiredPattern.Pattern `
        -Issue $requiredPattern.Issue
}

foreach ($requiredScriptNeedle in @(
        "OPENCV_CSHARP_OPENCV_DIR",
        "OPENCV_CSHARP_* variables are the current smoke-test/build environment knobs",
        "fixed-major OPENCV5SHARP_* aliases are not primary build inputs")) {
    Assert-Contains `
        -Violations $violations `
        -Path $buildNativeScriptPath `
        -Text $buildNativeScriptText `
        -Needle $requiredScriptNeedle `
        -Issue "$buildNativeScriptPath must keep native build entry points neutral-first"
}

foreach ($requiredWorkflowNeedle in @(
        "OPENCV_CSHARP_OPENCV_BUILD_LIST",
        "OPENCV_CSHARP_OPENCV_DIR")) {
    Assert-Contains `
        -Violations $violations `
        -Path $nativeWorkflowPath `
        -Text $nativeWorkflowText `
        -Needle $requiredWorkflowNeedle `
        -Issue "$nativeWorkflowPath must keep native workflow CMake variables neutral-first"
}

foreach ($doc in @(
        [pscustomobject]@{
            Path = $readmePath
            Text = $readmeText
            Required = @(
                "The native CMake project is currently source-tree build only",
                "does not currently install or export a reusable CMake package or SDK target",
                'The `JYPPX.OpenCV.Native` CMake target is primary',
                '`OpenCv5Sharp.Native` remains only a compatibility alias'
            )
        },
        [pscustomobject]@{
            Path = $contributingPath
            Text = $contributingText
            Required = @(
                "Keep the native CMake wrapper source-tree build only",
                'do not add `install(`, `export(`, CMake package config generation, or install-interface include paths',
                'Keep `JYPPX.OpenCV.Native` as the primary CMake target',
                '`OpenCv5Sharp.Native` only as a compatibility alias'
            )
        },
        [pscustomobject]@{
            Path = $versionNeutralGuidePath
            Text = $versionNeutralGuideText
            Required = @(
                "Native CMake target",
                "source-tree build only",
                "not a public install/export package surface",
                "Test-NativeCMakeTargetExportBoundary.ps1"
            )
        },
        [pscustomobject]@{
            Path = $nativeModuleBoundaryPath
            Text = $nativeModuleBoundaryText
            Required = @(
                "CMake target/export boundary",
                "source-tree build only",
                "does not install or export a reusable CMake package or SDK target today",
                '`OpenCv5Sharp.Native` CMake target name is only a compatibility alias'
            )
        })) {
    foreach ($requiredText in $doc.Required) {
        Assert-Contains `
            -Violations $violations `
            -Path $doc.Path `
            -Text $doc.Text `
            -Needle $requiredText `
            -Issue "$($doc.Path) must document native CMake target/export boundary text '$requiredText'"
    }
}

if ($violations.Count -gt 0) {
    Write-Host "Native CMake target/export boundary guard failed with $($violations.Count) violation(s)."
    $violations |
        Sort-Object Path, Issue |
        Format-Table Path, Issue, Text -AutoSize
    exit 1
}

Write-Host "Native CMake target/export boundary guard passed."
Write-Host "Primary native CMake target: JYPPX.OpenCV.Native."
Write-Host "Compatibility native CMake alias: OpenCv5Sharp.Native."
Write-Host "CMake package install/export surface: absent."

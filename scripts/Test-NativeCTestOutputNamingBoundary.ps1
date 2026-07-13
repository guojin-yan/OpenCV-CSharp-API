param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$nativeCMakePath = "src/OpenCvSharp.Native/CMakeLists.txt"
$nativeSmokePath = "src/OpenCvSharp.Native/tests/native_smoke.cpp"
$compatibilitySourceSmokePath = "src/OpenCvSharp.Native/tests/legacy_source_compat_smoke.cpp"
$buildNativeScriptPath = "scripts/Build-Native.ps1"
$nativeWorkflowPath = ".github/workflows/build-native.yml"
$readmePath = "README.md"
$contributingPath = "CONTRIBUTING.md"
$versionNeutralGuidePath = "docs/articles/version-neutral-naming-guide.md"
$nativeModuleBoundaryPath = "docs/articles/native-module-boundary.md"
$regexOptions = [System.Text.RegularExpressions.RegexOptions]::IgnoreCase -bor [System.Text.RegularExpressions.RegexOptions]::Multiline
$fixedMajorCTestDeclarationPattern = "add_(?:executable|test)\s*\([^)]*(?:OpenCv" + "5Sharp|OPENCV" + "5SHARP|open_cv_" + "5_sharp|opencv" + "5)"
$compatibilityOnlyNativeTokenPattern = "open_cv_" + "5_sharp|OPENCV" + "5SHARP_|jyppx_ocv" + "5_"
$compatibilityCoreMatIncludeNeedle = '#include "open_cv_' + '5_sharp/core/mat.h"'

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
        throw "Required native CTest/output boundary file was not found: $RelativePath"
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

function Assert-NotMatches {
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

    $matches = [regex]::Matches($Text, $Pattern, $regexOptions)
    foreach ($match in $matches) {
        Add-Violation -Violations $Violations -Path $Path -Issue $Issue -Text $match.Value
    }
}

$violations = [System.Collections.Generic.List[object]]::new()

$nativeCMakeText = Read-RequiredText -RelativePath $nativeCMakePath
$nativeSmokeText = Read-RequiredText -RelativePath $nativeSmokePath
$compatibilitySourceSmokeText = Read-RequiredText -RelativePath $compatibilitySourceSmokePath
$buildNativeScriptText = Read-RequiredText -RelativePath $buildNativeScriptPath
$nativeWorkflowText = Read-RequiredText -RelativePath $nativeWorkflowPath
$readmeText = Read-RequiredText -RelativePath $readmePath
$contributingText = Read-RequiredText -RelativePath $contributingPath
$versionNeutralGuideText = Read-RequiredText -RelativePath $versionNeutralGuidePath
$nativeModuleBoundaryText = Read-RequiredText -RelativePath $nativeModuleBoundaryPath

foreach ($required in @(
        [pscustomobject]@{
            Needle = 'set(OPENCV_CSHARP_NATIVE_SMOKE_TARGET "${OPENCV_CSHARP_NATIVE_TARGET}Smoke")'
            Issue = "Native smoke target name must derive from the neutral primary native target"
        },
        [pscustomobject]@{
            Needle = 'set(OPENCV_CSHARP_COMPATIBILITY_SOURCE_SMOKE_TARGET "${OPENCV_CSHARP_NATIVE_TARGET}CompatibilitySourceSmoke")'
            Issue = "Compatibility source smoke target name must derive from the neutral primary native target"
        },
        [pscustomobject]@{
            Needle = 'OUTPUT_NAME "${OPENCV_CSHARP_NATIVE_TARGET}"'
            Issue = "Native library output name must remain the neutral primary target name"
        },
        [pscustomobject]@{
            Needle = '${CMAKE_SHARED_LIBRARY_PREFIX}${OPENCV_CSHARP_COMPATIBILITY_NATIVE_TARGET}${CMAKE_SHARED_LIBRARY_SUFFIX}'
            Issue = "Compatibility loader filename must be derived only from the compatibility target alias"
        },
        [pscustomobject]@{
            Needle = 'Creating native loader compatibility copy ${OPENCV_CSHARP_COMPATIBILITY_NATIVE_FILE_NAME}'
            Issue = "Compatibility loader copy must stay explicitly labelled as compatibility"
        },
        [pscustomobject]@{
            Needle = 'Smoke/CTest target names derive from the neutral primary target'
            Issue = "Native CMake file must document neutral-first CTest target naming"
        },
        [pscustomobject]@{
            Needle = 'Compatibility loader copy keeps existing binary consumers working'
            Issue = "Native CMake file must document compatibility-loader copy scope"
        },
        [pscustomobject]@{
            Needle = 'add_executable(${OPENCV_CSHARP_NATIVE_SMOKE_TARGET}'
            Issue = "Primary native smoke executable must use the neutral smoke target variable"
        },
        [pscustomobject]@{
            Needle = 'add_test(NAME ${OPENCV_CSHARP_NATIVE_SMOKE_TARGET} COMMAND ${OPENCV_CSHARP_NATIVE_SMOKE_TARGET})'
            Issue = "Primary native smoke CTest name must use the neutral smoke target variable"
        },
        [pscustomobject]@{
            Needle = 'add_executable(${OPENCV_CSHARP_COMPATIBILITY_SOURCE_SMOKE_TARGET}'
            Issue = "Compatibility source smoke executable must use the neutral-derived compatibility smoke target variable"
        },
        [pscustomobject]@{
            Needle = 'NAME ${OPENCV_CSHARP_COMPATIBILITY_SOURCE_SMOKE_TARGET}'
            Issue = "Compatibility source smoke CTest name must use the neutral-derived compatibility smoke target variable"
        },
        [pscustomobject]@{
            Needle = 'NAME ${OPENCV_CSHARP_NATIVE_TARGET}AbiGeneratedCheck'
            Issue = "ABI generated-file check CTest name must derive from the neutral primary native target"
        },
        [pscustomobject]@{
            Needle = 'NAME ${OPENCV_CSHARP_NATIVE_TARGET}LegacyIncludeParity'
            Issue = "Legacy include parity CTest name must derive from the neutral primary native target"
        },
        [pscustomobject]@{
            Needle = 'set(OPENCV_CSHARP_NATIVE_ABI_EXPORT_TEST "${OPENCV_CSHARP_NATIVE_TARGET}AbiExportAudit")'
            Issue = "ABI export audit CTest name must derive from the neutral primary native target"
        })) {
    Assert-Contains `
        -Violations $violations `
        -Path $nativeCMakePath `
        -Text $nativeCMakeText `
        -Needle $required.Needle `
        -Issue $required.Issue
}

Assert-NotMatches `
    -Violations $violations `
    -Path $nativeCMakePath `
    -Text $nativeCMakeText `
    -Pattern $fixedMajorCTestDeclarationPattern `
    -Issue "Native CTest/executable declarations must not introduce fixed-major target names"

Assert-Contains `
    -Violations $violations `
    -Path $nativeSmokePath `
    -Text $nativeSmokeText `
    -Needle '#include "open_cv_sharp/core/mat.h"' `
    -Issue "Primary native smoke source must include the neutral wrapper header tree"

Assert-NotMatches `
    -Violations $violations `
    -Path $nativeSmokePath `
    -Text $nativeSmokeText `
    -Pattern $compatibilityOnlyNativeTokenPattern `
    -Issue "Primary native smoke source must not use compatibility-only include, status, or ABI names"

Assert-Contains `
    -Violations $violations `
    -Path $compatibilitySourceSmokePath `
    -Text $compatibilitySourceSmokeText `
    -Needle $compatibilityCoreMatIncludeNeedle `
    -Issue "Compatibility source smoke must explicitly cover the compatibility include tree"

Assert-Contains `
    -Violations $violations `
    -Path $compatibilitySourceSmokePath `
    -Text $compatibilitySourceSmokeText `
    -Needle "Compatibility Reg unknown kind alias must remain source-compatible." `
    -Issue "Compatibility source smoke must label fixed-major aliases as compatibility/source-compatible"

foreach ($requiredScriptNeedle in @(
        'BuildDir is the version-neutral native build-output path input',
        'Invoke-CheckedCommand ctest --test-dir $nativeBuild -C $Configuration --output-on-failure',
        'Native runtime directory:')) {
    Assert-Contains `
        -Violations $violations `
        -Path $buildNativeScriptPath `
        -Text $buildNativeScriptText `
        -Needle $requiredScriptNeedle `
        -Issue "$buildNativeScriptPath must keep CTest/build-output entry points neutral-first"
}

Assert-NotMatches `
    -Violations $violations `
    -Path $buildNativeScriptPath `
    -Text $buildNativeScriptText `
    -Pattern "OpenCv5Sharp\.Native(?:Smoke|Abi|Legacy)|OpenCv5Sharp\.Native\.dll\s+is\s+primary|primary\s+OPENCV5SHARP_|OPENCV5SHARP_.*\bis\s+primary" `
    -Issue "$buildNativeScriptPath must not treat compatibility native names as primary CTest/output names"

foreach ($requiredWorkflowNeedle in @(
        'ctest --test-dir build/native -C Release --output-on-failure',
        'OPENCV_CSHARP_OPENCV_BUILD_LIST',
        'OPENCV_CSHARP_OPENCV_DIR')) {
    Assert-Contains `
        -Violations $violations `
        -Path $nativeWorkflowPath `
        -Text $nativeWorkflowText `
        -Needle $requiredWorkflowNeedle `
        -Issue "$nativeWorkflowPath must keep native CTest workflow entry points neutral-first"
}

foreach ($doc in @(
        [pscustomobject]@{
            Path = $readmePath
            Text = $readmeText
            Required = @(
                "Native CTest and local build output names are neutral-first",
                '`JYPPX.OpenCV.NativeSmoke`',
                '`JYPPX.OpenCV.NativeCompatibilitySourceSmoke`',
                '`OpenCv5Sharp.Native` loader file remains only a compatibility copy'
            )
        },
        [pscustomobject]@{
            Path = $contributingPath
            Text = $contributingText
            Required = @(
                "Keep native CTest and local build-output names neutral-first",
                '`JYPPX.OpenCV.NativeSmoke`',
                '`JYPPX.OpenCV.NativeCompatibilitySourceSmoke`',
                '`OpenCv5Sharp.Native` loader file only as a compatibility copy'
            )
        },
        [pscustomobject]@{
            Path = $versionNeutralGuidePath
            Text = $versionNeutralGuideText
            Required = @(
                "Native CTest/output names",
                "neutral-first",
                '`JYPPX.OpenCV.NativeSmoke`',
                "Test-NativeCTestOutputNamingBoundary.ps1"
            )
        },
        [pscustomobject]@{
            Path = $nativeModuleBoundaryPath
            Text = $nativeModuleBoundaryText
            Required = @(
                "CTest/Output Naming Boundary",
                "neutral-first",
                '`JYPPX.OpenCV.NativeSmoke`',
                '`OpenCv5Sharp.Native` loader file remains only the compatibility copy'
            )
        })) {
    foreach ($requiredText in $doc.Required) {
        Assert-Contains `
            -Violations $violations `
            -Path $doc.Path `
            -Text $doc.Text `
            -Needle $requiredText `
            -Issue "$($doc.Path) must document native CTest/output naming boundary text '$requiredText'"
    }
}

if ($violations.Count -gt 0) {
    Write-Host "Native CTest/output naming boundary guard failed with $($violations.Count) violation(s)."
    $violations |
        Sort-Object Path, Issue |
        Format-Table Path, Issue, Text -AutoSize
    exit 1
}

Write-Host "Native CTest/output naming boundary guard passed."
Write-Host "Primary native smoke target: JYPPX.OpenCV.NativeSmoke."
Write-Host "Compatibility source smoke target: JYPPX.OpenCV.NativeCompatibilitySourceSmoke."
Write-Host "Compatibility loader copy: OpenCv5Sharp.Native."

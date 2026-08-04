param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$nativeCMakePath = "src/OpenCvSharp.Native/CMakeLists.txt"
$buildNativeScriptPath = "scripts/Build-Native.ps1"
$stageRuntimeScriptPath = "scripts/Stage-Runtime.ps1"
$readmePath = "README.md"
$contributingPath = "CONTRIBUTING.md"
$versionNeutralGuidePath = "docs/articles/version-neutral-naming-guide.md"
$nativeModuleBoundaryPath = "docs/articles/native-module-boundary.md"
$regexOptions = [System.Text.RegularExpressions.RegexOptions]::IgnoreCase -bor [System.Text.RegularExpressions.RegexOptions]::Multiline
$fixedMajorRuntimeRootPattern = "(OPENCV" + "5SHARP_[A-Z0-9_]*(?:RUNTIME|PATH|COPY|ROOT)[A-Z0-9_]*)|(OpenCv" + "5Sharp[A-Za-z0-9_]*(?:Runtime|Path|Copy|Root)[A-Za-z0-9_]*)"

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
        throw "Required native runtime-root/PATH copy boundary file was not found: $RelativePath"
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
$buildNativeScriptText = Read-RequiredText -RelativePath $buildNativeScriptPath
$stageRuntimeScriptText = Read-RequiredText -RelativePath $stageRuntimeScriptPath
$readmeText = Read-RequiredText -RelativePath $readmePath
$contributingText = Read-RequiredText -RelativePath $contributingPath
$versionNeutralGuideText = Read-RequiredText -RelativePath $versionNeutralGuidePath
$nativeModuleBoundaryText = Read-RequiredText -RelativePath $nativeModuleBoundaryPath

foreach ($required in @(
        [pscustomobject]@{
            Needle = 'set(OPENCV_CSHARP_OPENCV_RUNTIME_ROOT "")'
            Issue = "Native CMake runtime root must use the neutral OPENCV_CSHARP runtime-root variable"
        },
        [pscustomobject]@{
            Needle = 'set(OPENCV_CSHARP_OPENCV_RUNTIME_ROOT_CANDIDATES'
            Issue = "Native CMake runtime-root candidates must use neutral OPENCV_CSHARP naming"
        },
        [pscustomobject]@{
            Needle = 'foreach(OPENCV_CSHARP_CANDIDATE IN LISTS OPENCV_CSHARP_OPENCV_RUNTIME_ROOT_CANDIDATES)'
            Issue = "Native CMake runtime-root probe loop must use neutral candidate variables"
        },
        [pscustomobject]@{
            Needle = '# Discover runtime DLLs from OpenCV_DIR using version-neutral project variables.'
            Issue = "Native CMake runtime discovery must document version-neutral project variables"
        },
        [pscustomobject]@{
            Needle = '# The copied opencv*.dll names remain factual upstream artifacts for the linked OpenCV build.'
            Issue = "Native CMake runtime copy must label opencv*.dll names as factual upstream artifacts"
        },
        [pscustomobject]@{
            Needle = 'file(GLOB OPENCV_CSHARP_CANDIDATE_FLAT_DLLS'
            Issue = "Native CMake must collect flat OpenCV DLL candidates through neutral variables"
        },
        [pscustomobject]@{
            Needle = '"${OPENCV_CSHARP_CANDIDATE}/opencv*.dll"'
            Issue = "Native CMake must keep factual upstream opencv*.dll runtime probe"
        },
        [pscustomobject]@{
            Needle = 'file(GLOB OPENCV_CSHARP_CANDIDATE_RELEASE_DLLS'
            Issue = "Native CMake must collect Release OpenCV DLL candidates through neutral variables"
        },
        [pscustomobject]@{
            Needle = '"${OPENCV_CSHARP_CANDIDATE}/Release/opencv*.dll"'
            Issue = "Native CMake must keep factual upstream Release/opencv*.dll runtime probe"
        },
        [pscustomobject]@{
            Needle = 'message(STATUS "OpenCV runtime root: ${OPENCV_CSHARP_OPENCV_RUNTIME_ROOT}")'
            Issue = "Native CMake runtime-root diagnostic must report the neutral runtime-root variable"
        },
        [pscustomobject]@{
            Needle = 'COMMAND ${CMAKE_COMMAND} -E copy_directory'
            Issue = "Native CMake linked Windows builds must copy the discovered OpenCV runtime directory"
        },
        [pscustomobject]@{
            Needle = '"${OPENCV_CSHARP_OPENCV_RUNTIME_ROOT}"'
            Issue = "Native CMake runtime copy source must be the neutral runtime-root variable"
        },
        [pscustomobject]@{
            Needle = '"$<TARGET_FILE_DIR:${OPENCV_CSHARP_NATIVE_TARGET}>"'
            Issue = "Native CMake runtime copy destination must be the neutral native target output directory"
        },
        [pscustomobject]@{
            Needle = 'OpenCV runtime directory was not found. Native smoke tests may need PATH to include OpenCV binaries.'
            Issue = "Native CMake missing-runtime diagnostic must stay generic and OpenCV-focused"
        },
        [pscustomobject]@{
            Needle = 'ENVIRONMENT "PATH=$<TARGET_FILE_DIR:${OPENCV_CSHARP_NATIVE_TARGET}>;${OPENCV_CSHARP_OPENCV_RUNTIME_ROOT};$ENV{PATH}"'
            Issue = "Native CTest PATH must include target output first and the neutral OpenCV runtime root second"
        },
        [pscustomobject]@{
            Needle = 'ENVIRONMENT "PATH=$<TARGET_FILE_DIR:${OPENCV_CSHARP_NATIVE_TARGET}>;$ENV{PATH}"'
            Issue = "Native CTest PATH fallback must still include the neutral target output directory first"
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
    -Pattern $fixedMajorRuntimeRootPattern `
    -Issue "Native CMake runtime-root/PATH/copy variables must not introduce fixed-major runtime names"

foreach ($requiredScriptNeedle in @(
        'BuildDir is the version-neutral native build-output path input',
        '"-DOPENCV_CSHARP_OPENCV_DIR=$OpenCvDir"',
        'Invoke-CheckedCommand ctest --test-dir $nativeBuild -C $Configuration --output-on-failure',
        'Write-Host "Native runtime directory: $(Join-Path $nativeBuild $Configuration)"')) {
    Assert-Contains `
        -Violations $violations `
        -Path $buildNativeScriptPath `
        -Text $buildNativeScriptText `
        -Needle $requiredScriptNeedle `
        -Issue "$buildNativeScriptPath must keep native runtime-root and CTest entry points neutral-first"
}

Assert-NotMatches `
    -Violations $violations `
    -Path $buildNativeScriptPath `
    -Text $buildNativeScriptText `
    -Pattern $fixedMajorRuntimeRootPattern `
    -Issue "$buildNativeScriptPath must not introduce fixed-major runtime-root/PATH/copy variables"

foreach ($requiredStageNeedle in @(
        '[string]$OpenCvNativeRuntimeDir = ""',
        'Derived only for factual upstream OpenCV runtime probe names such as opencv_core500.dll or libopencv_core.so.',
        'Derived only for factual upstream OpenCV runtime names such as opencv_core500.dll or libopencv_core.so.5.0.0.',
        '$nativeRuntimePath = Resolve-RepoPath $OpenCvNativeRuntimeDir')) {
    Assert-Contains `
        -Violations $violations `
        -Path $stageRuntimeScriptPath `
        -Text $stageRuntimeScriptText `
        -Needle $requiredStageNeedle `
        -Issue "$stageRuntimeScriptPath must keep runtime staging inputs neutral-first and factual runtime names scoped"
}

foreach ($doc in @(
        [pscustomobject]@{
            Path = $readmePath
            Text = $readmeText
            Required = @(
                "Native CMake runtime-root/PATH copy is neutral-first",
                '`OPENCV_CSHARP_OPENCV_RUNTIME_ROOT`',
                'put that target output directory first in CTest `PATH`',
                'the copied `opencv*.dll` names remain factual upstream artifacts'
            )
        },
        [pscustomobject]@{
            Path = $contributingPath
            Text = $contributingText
            Required = @(
                "Keep native runtime-root/PATH copy neutral-first",
                '`OPENCV_CSHARP_OPENCV_RUNTIME_ROOT`',
                'CTest `PATH`',
                'factual upstream `opencv*.dll`'
            )
        },
        [pscustomobject]@{
            Path = $versionNeutralGuidePath
            Text = $versionNeutralGuideText
            Required = @(
                "Native runtime-root/PATH copy",
                '`OPENCV_CSHARP_OPENCV_RUNTIME_ROOT`',
                'target output directory first on CTest `PATH`',
                "Test-NativeRuntimeRootPathCopyBoundary.ps1"
            )
        },
        [pscustomobject]@{
            Path = $nativeModuleBoundaryPath
            Text = $nativeModuleBoundaryText
            Required = @(
                "Runtime Root/PATH Copy Boundary",
                '`OPENCV_CSHARP_OPENCV_RUNTIME_ROOT`',
                'target output directory first in CTest `PATH`',
                '`opencv*.dll` names remain factual upstream artifacts'
            )
        })) {
    foreach ($requiredText in $doc.Required) {
        Assert-Contains `
            -Violations $violations `
            -Path $doc.Path `
            -Text $doc.Text `
            -Needle $requiredText `
            -Issue "$($doc.Path) must document native runtime-root/PATH copy boundary text '$requiredText'"
    }
}

if ($violations.Count -gt 0) {
    Write-Host "Native runtime-root/PATH copy boundary guard failed with $($violations.Count) violation(s)."
    $violations |
        Sort-Object Path, Issue |
        Format-Table Path, Issue, Text -AutoSize
    exit 1
}

Write-Host "Native runtime-root/PATH copy boundary guard passed."
Write-Host "Neutral runtime root: OPENCV_CSHARP_OPENCV_RUNTIME_ROOT."
Write-Host "Native CTest PATH starts with the primary target output directory."
Write-Host "Copied opencv*.dll files remain factual upstream artifacts."

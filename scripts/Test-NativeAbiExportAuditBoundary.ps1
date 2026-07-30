param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$abiScriptRelativePath = "scripts/Test-NativeAbiExports.ps1"
$cmakeRelativePath = "src/OpenCvSharp.Native/CMakeLists.txt"
$workflowRelativePaths = @(
    ".github/workflows/runtime-input.yml",
    ".github/workflows/pack.yml"
)

function Read-RequiredText {
    param([Parameter(Mandatory)][string]$RelativePath)

    $path = Join-Path $repo $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Required ABI export audit boundary file was not found: $RelativePath"
    }

    return [System.IO.File]::ReadAllText($path)
}

function Add-Violation {
    param(
        [Parameter(Mandatory)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$Violations,
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$Issue,
        [string]$Text = ""
    )

    $Violations.Add([pscustomobject]@{
        Path = $Path
        Issue = $Issue
        Text = $Text.Trim()
    })
}

function Assert-Contains {
    param(
        [Parameter(Mandatory)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$Violations,
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$Text,
        [Parameter(Mandatory)][string]$Needle,
        [Parameter(Mandatory)][string]$Issue
    )

    if ($Text.IndexOf($Needle, [System.StringComparison]::Ordinal) -lt 0) {
        Add-Violation -Violations $Violations -Path $Path -Issue $Issue -Text $Needle
    }
}

function Assert-Order {
    param(
        [Parameter(Mandatory)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$Violations,
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$Text,
        [Parameter(Mandatory)][string]$Earlier,
        [Parameter(Mandatory)][string]$Later,
        [Parameter(Mandatory)][string]$Issue
    )

    $earlierIndex = $Text.IndexOf($Earlier, [System.StringComparison]::Ordinal)
    $laterIndex = $Text.IndexOf($Later, [System.StringComparison]::Ordinal)
    if ($earlierIndex -lt 0 -or $laterIndex -lt 0 -or $earlierIndex -ge $laterIndex) {
        Add-Violation -Violations $Violations -Path $Path -Issue $Issue -Text "$Earlier before $Later"
    }
}

$violations = [System.Collections.Generic.List[object]]::new()
$abiPath = Join-Path $repo $abiScriptRelativePath
$cmakeText = Read-RequiredText -RelativePath $cmakeRelativePath
$abiText = Read-RequiredText -RelativePath $abiScriptRelativePath

foreach ($required in @(
        "`[System.Runtime.InteropServices.RuntimeInformation`]::ProcessArchitecture.ToString()",
        "Get-PeMachine",
        "requiresStaticExportAudit",
        "Cross-architecture Windows ABI export audit requires ExportToolPath",
        "Resolve-Path -LiteralPath `$ExportToolPath",
        "GetFileName(`$resolvedExportToolPath) -ne `"dumpbin.exe`"",
        "`$resolvedExportToolPath /nologo /exports `$resolvedLibraryPath",
        "Assert-ManifestExports -ExportNames `$exportNames -Rows `$manifestRows",
        "`[System.Runtime.InteropServices.NativeLibrary`]::Load",
        "Verified cross-architecture Windows export table")) {
    Assert-Contains `
        -Violations $violations `
        -Path $abiScriptRelativePath `
        -Text $abiText `
        -Needle $required `
        -Issue "ABI export audit must retain the explicit cross-architecture PE export path"
}

Assert-Order `
    -Violations $violations `
    -Path $abiScriptRelativePath `
    -Text $abiText `
    -Earlier "`$requiresStaticExportAudit = `$libraryMachine -ne `$processMachine" `
    -Later "`[System.Runtime.InteropServices.NativeLibrary`]::Load" `
    -Issue "Cross-architecture detection must occur before any dynamic library load"

foreach ($required in @(
        "set(OPENCV_CSHARP_NATIVE_ABI_EXPORT_TOOL_ARGUMENTS)",
        "if(WIN32 AND MSVC AND CMAKE_LINKER)",
        'get_filename_component(OPENCV_CSHARP_MSVC_TOOL_DIRECTORY "${CMAKE_LINKER}" DIRECTORY)',
        'set(OPENCV_CSHARP_DUMPBIN_EXECUTABLE "${OPENCV_CSHARP_MSVC_TOOL_DIRECTORY}/dumpbin.exe")',
        'if(EXISTS "${OPENCV_CSHARP_DUMPBIN_EXECUTABLE}")',
        "OPENCV_CSHARP_NATIVE_ABI_EXPORT_TOOL_ARGUMENTS",
        "-ExportToolPath")) {
    Assert-Contains `
        -Violations $violations `
        -Path $cmakeRelativePath `
        -Text $cmakeText `
        -Needle $required `
        -Issue "CMake must derive and forward the selected MSVC dumpbin path to ABI export audit"
}

Assert-Order `
    -Violations $violations `
    -Path $cmakeRelativePath `
    -Text $cmakeText `
    -Earlier "get_filename_component(OPENCV_CSHARP_MSVC_TOOL_DIRECTORY" `
    -Later 'NAME ${OPENCV_CSHARP_NATIVE_ABI_EXPORT_TEST}' `
    -Issue "CMake must resolve the ABI export tool before declaring the audit test"

foreach ($workflowRelativePath in $workflowRelativePaths) {
    $workflowText = Read-RequiredText -RelativePath $workflowRelativePath
    Assert-Contains `
        -Violations $violations `
        -Path $workflowRelativePath `
        -Text $workflowText `
        -Needle "-DumpbinPath" `
        -Issue "Windows workflow must pass the explicit target dumpbin path into PE auditing"
}

if ($violations.Count -gt 0) {
    Write-Host "Native ABI export audit boundary guard failed with $($violations.Count) violation(s)."
    $violations | Sort-Object Path, Issue | Format-Table Path, Issue, Text -AutoSize
    exit 1
}

Write-Host "Native ABI export audit boundary guard passed."
Write-Host "Cross-architecture Windows audit uses dumpbin export tables; same-architecture audit retains dynamic loading."

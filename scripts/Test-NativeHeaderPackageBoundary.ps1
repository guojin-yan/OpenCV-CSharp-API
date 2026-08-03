param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$runtimeProjectPath = "packaging/runtime/JYPPX.OpenCV.runtime/JYPPX.OpenCV.runtime.csproj"
$runtimeReadmePath = "packaging/runtime/JYPPX.OpenCV.runtime/README.md"
$readmePath = "README.md"
$contributingPath = "CONTRIBUTING.md"
$versionNeutralGuidePath = "docs/articles/version-neutral-naming-guide.md"
$primaryHeaderTree = "src/OpenCvSharp.Native/include/open_cv_sharp"
$compatibilityHeaderTree = "src/OpenCvSharp.Native/include/open_cv_5_sharp"
$primaryHeaderTreeLeaf = "open_cv_sharp"
$compatibilityHeaderTreeLeaf = "open_cv_5_sharp"

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
        throw "Required native header package boundary file was not found: $RelativePath"
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

$violations = [System.Collections.Generic.List[object]]::new()

$runtimeProjectText = Read-RequiredText -RelativePath $runtimeProjectPath
$runtimeReadmeText = Read-RequiredText -RelativePath $runtimeReadmePath
$readmeText = Read-RequiredText -RelativePath $readmePath
$contributingText = Read-RequiredText -RelativePath $contributingPath
$versionNeutralGuideText = Read-RequiredText -RelativePath $versionNeutralGuidePath

$runtimeProjectXml = [xml]$runtimeProjectText
$packedItems = @($runtimeProjectXml.SelectNodes("//*[@Pack='true']"))
$allowedPackedIncludes = [System.Collections.Generic.HashSet[string]]::new(
    [System.StringComparer]::OrdinalIgnoreCase)
foreach ($allowed in @(
        "README.md",
        "../runtime-distro-rid-graph.json",
        "build/JYPPX.OpenCV.runtime.provenance.json",
        "buildTransitive/JYPPX.OpenCV.runtime.targets",
        "runtimes/`$(RuntimePackageRid)/native/**/*",
        "licenses/**/*")) {
    [void]$allowedPackedIncludes.Add($allowed)
}

if ($packedItems.Count -eq 0) {
    Add-Violation -Violations $violations -Path $runtimeProjectPath -Issue "Runtime package project must explicitly list packed items"
}

foreach ($item in $packedItems) {
    $include = $item.Attributes["Include"].Value
    $packagePath = ""
    if ($null -ne $item.Attributes["PackagePath"]) {
        $packagePath = $item.Attributes["PackagePath"].Value
    }

    $normalizedInclude = $include.Replace("\", "/")
    $normalizedPackagePath = $packagePath.Replace("\", "/")
    if (-not $allowedPackedIncludes.Contains($normalizedInclude)) {
        Add-Violation `
            -Violations $violations `
            -Path $runtimeProjectPath `
            -Issue "Runtime package project must not add unreviewed packed payloads outside runtime binaries, licenses, provenance, README, and the audited Android buildTransitive target" `
            -Text $include
    }

    $forbiddenHeaderPayloadPattern = (
        "(^|/)(include|headers?)(/|$)|" +
        [regex]::Escape($primaryHeaderTreeLeaf) + "|" +
        [regex]::Escape($compatibilityHeaderTreeLeaf) + "|" +
        "OpenCvSharp\.Native/include")
    if ("$normalizedInclude/$normalizedPackagePath" -match $forbiddenHeaderPayloadPattern) {
        Add-Violation `
            -Violations $violations `
            -Path $runtimeProjectPath `
            -Issue "Runtime package project must not package native C headers or include trees" `
            -Text "Include=$include PackagePath=$packagePath"
    }
}

$androidBuildTargetItems = @(
    $packedItems |
        Where-Object { $_.Attributes["Include"].Value -eq "buildTransitive/JYPPX.OpenCV.runtime.targets" })
if ($androidBuildTargetItems.Count -ne 1) {
    Add-Violation -Violations $violations -Path $runtimeProjectPath -Issue "Runtime package project must declare exactly one audited Android buildTransitive target"
}
else {
    $androidItem = $androidBuildTargetItems[0]
    $condition = $androidItem.Attributes["Condition"].Value
    $packagePath = $androidItem.Attributes["PackagePath"].Value
    foreach ($androidRid in @("android-arm64", "android-arm", "android-x64", "android-x86")) {
        if ($condition.IndexOf("'`$(RuntimePackageRid)' == '$androidRid'", [System.StringComparison]::Ordinal) -lt 0) {
            Add-Violation -Violations $violations -Path $runtimeProjectPath -Issue "Android buildTransitive target condition must be restricted to every declared Android RID" -Text $androidRid
        }
    }
    if ($packagePath -ne "buildTransitive/`$(PackageId).targets") {
        Add-Violation -Violations $violations -Path $runtimeProjectPath -Issue "Android buildTransitive target must use NuGet's exact package-ID import name" -Text $packagePath
    }
}

foreach ($forbiddenPattern in @(
        "contentFiles",
        "src/OpenCvSharp.Native/include",
        $primaryHeaderTreeLeaf,
        $compatibilityHeaderTreeLeaf,
        "PackagePath\s*=\s*[`"'][^`"']*(?:include|header)")) {
    if ($runtimeProjectText -match $forbiddenPattern) {
        Add-Violation `
            -Violations $violations `
            -Path $runtimeProjectPath `
            -Issue "Runtime package project must not imply a native header SDK payload" `
            -Text $forbiddenPattern
    }
}

foreach ($doc in @(
        [pscustomobject]@{
            Path = $runtimeReadmePath
            Text = $runtimeReadmeText
            Required = @(
                "Runtime packages do not currently distribute native C headers",
                "not native C header SDK",
                $primaryHeaderTree,
                $compatibilityHeaderTree,
                "compatibility"
            )
        },
        [pscustomobject]@{
            Path = $readmePath
            Text = $readmeText
            Required = @(
                "Runtime NuGet packages do not currently distribute native C headers",
                $primaryHeaderTree,
                $compatibilityHeaderTree,
                "compatibility"
            )
        },
        [pscustomobject]@{
            Path = $contributingPath
            Text = $contributingText
            Required = @(
                "Runtime packages must remain header-free",
                $primaryHeaderTree,
                $compatibilityHeaderTree,
                "compatibility"
            )
        },
        [pscustomobject]@{
            Path = $versionNeutralGuidePath
            Text = $versionNeutralGuideText
            Required = @(
                "Native headers are currently a source-tree",
                "runtime NuGet packages do not distribute native C headers",
                $primaryHeaderTree,
                $compatibilityHeaderTree,
                "Test-NativeHeaderPackageBoundary.ps1"
            )
        })) {
    foreach ($requiredText in $doc.Required) {
        Assert-Contains `
            -Violations $violations `
            -Path $doc.Path `
            -Text $doc.Text `
            -Needle $requiredText `
            -Issue "$($doc.Path) must document native header package boundary text '$requiredText'"
    }
}

if ($violations.Count -gt 0) {
    Write-Host "Native header package boundary guard failed with $($violations.Count) violation(s)."
    $violations |
        Sort-Object Path, Issue |
        Format-Table Path, Issue, Text -AutoSize
    exit 1
}

Write-Host "Native header package boundary guard passed."
Write-Host "Runtime package packed items checked: $($packedItems.Count)."
Write-Host "Primary source-tree header surface: $primaryHeaderTree."
Write-Host "Compatibility source-tree header surface: $compatibilityHeaderTree."

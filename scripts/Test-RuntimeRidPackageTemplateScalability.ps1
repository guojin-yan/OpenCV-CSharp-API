param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$runtimePackagePrefix = "JYPPX.OpenCV.runtime"
$runtimePackageMiniShape = "$runtimePackagePrefix.<rid>.mini"
$currentExampleRid = "win-x64"
$currentRuntimeProject = "packaging/runtime/JYPPX.OpenCV.runtime"
$currentRuntimeProjectFile = "$currentRuntimeProject/JYPPX.OpenCV.runtime.csproj"
$runtimePackageMatrixPath = "packaging/runtime/runtime-package-matrix.json"

function Add-Violation {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.List[object]]$Violations,
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [int]$Line = 0,
        [Parameter(Mandatory = $true)]
        [string]$Issue,
        [string]$Text = ""
    )

    $Violations.Add([pscustomobject]@{
        Path = $Path
        Line = $Line
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
        throw "Required runtime RID package template file was not found: $RelativePath"
    }

    return [System.IO.File]::ReadAllText($path)
}

function Test-ContainsText {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Text,
        [Parameter(Mandatory = $true)]
        [string]$Needle,
        [switch]$NormalizeSlashes
    )

    if ($NormalizeSlashes) {
        $Text = $Text.Replace("\", "/")
        $Needle = $Needle.Replace("\", "/")
    }

    return $Text.IndexOf($Needle, [System.StringComparison]::OrdinalIgnoreCase) -ge 0
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
        [string]$Issue,
        [switch]$NormalizeSlashes
    )

    if (-not (Test-ContainsText -Text $Text -Needle $Needle -NormalizeSlashes:$NormalizeSlashes)) {
        Add-Violation -Violations $Violations -Path $Path -Issue $Issue
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

    if ($Text -notmatch $Pattern) {
        Add-Violation -Violations $Violations -Path $Path -Issue $Issue
    }
}

$violations = [System.Collections.Generic.List[object]]::new()

$packRuntimePath = "scripts/Pack-Runtime.ps1"
$stageRuntimePath = "scripts/Stage-Runtime.ps1"
$packWorkflowPath = ".github/workflows/pack.yml"
$runtimeProjectPath = $currentRuntimeProjectFile
$runtimeReadmePath = "$currentRuntimeProject/README.md"
$runtimeMatrixPath = $runtimePackageMatrixPath
$gitignorePath = ".gitignore"
$readmePath = "README.md"
$linkedRuntimeGuidePath = "docs/articles/linked-runtime-build-guide.md"
$runtimeLicensesPath = "docs/articles/runtime-licenses.md"
$nativeBoundaryPath = "docs/articles/native-module-boundary.md"
$versionNeutralGuidePath = "docs/articles/version-neutral-naming-guide.md"

$packRuntimeText = Read-RequiredText -RelativePath $packRuntimePath
$stageRuntimeText = Read-RequiredText -RelativePath $stageRuntimePath
$packWorkflowText = Read-RequiredText -RelativePath $packWorkflowPath
$runtimeProjectText = Read-RequiredText -RelativePath $runtimeProjectPath
$runtimeReadmeText = Read-RequiredText -RelativePath $runtimeReadmePath
$runtimeMatrixText = Read-RequiredText -RelativePath $runtimeMatrixPath
$gitignoreText = Read-RequiredText -RelativePath $gitignorePath
$readmeText = Read-RequiredText -RelativePath $readmePath
$linkedRuntimeGuideText = Read-RequiredText -RelativePath $linkedRuntimeGuidePath
$runtimeLicensesText = Read-RequiredText -RelativePath $runtimeLicensesPath
$nativeBoundaryText = Read-RequiredText -RelativePath $nativeBoundaryPath
$versionNeutralGuideText = Read-RequiredText -RelativePath $versionNeutralGuidePath

Assert-Contains -Violations $violations -Path $packRuntimePath -Text $packRuntimeText -Needle '[string]$Rid = "win-x64"' -Issue "Pack-Runtime may keep win-x64 only as the current default RID example"
Assert-Contains -Violations $violations -Path $packRuntimePath -Text $packRuntimeText -Needle '$runtimePackageId = "$runtimePackagePrefix.$Rid$runtimePackageSuffix"' -Issue "Pack-Runtime must derive runtime package ID from -Rid and runtime profile suffix"
Assert-Contains -Violations $violations -Path $packRuntimePath -Text $packRuntimeText -Needle '"-p:RuntimePackageRid=$Rid"' -Issue "Pack-Runtime must pass RuntimePackageRid from -Rid"
Assert-Contains -Violations $violations -Path $packRuntimePath -Text $packRuntimeText -Needle '"-p:RuntimePackageProfile=$RuntimeProfile"' -Issue "Pack-Runtime must pass RuntimePackageProfile from -RuntimeProfile"
Assert-Contains -Violations $violations -Path $packRuntimePath -Text $packRuntimeText -Needle '"-p:PackageId=$runtimePackageId"' -Issue "Pack-Runtime must pass the derived RID package ID"
Assert-Contains -Violations $violations -Path $packRuntimePath -Text $packRuntimeText -Needle '[System.IO.Path]::IsPathRooted($RuntimeProject)' -Issue "Pack-Runtime -RuntimeProject must accept absolute project paths"
Assert-Contains -Violations $violations -Path $packRuntimePath -Text $packRuntimeText -Needle 'Join-Path $repoRoot $RuntimeProject' -Issue "Pack-Runtime -RuntimeProject must accept repository-relative project paths"

Assert-Contains -Violations $violations -Path $stageRuntimePath -Text $stageRuntimeText -Needle '[string]$Rid = "win-x64"' -Issue "Stage-Runtime may keep win-x64 only as the current default RID example"
Assert-Contains -Violations $violations -Path $stageRuntimePath -Text $stageRuntimeText -Needle '$stagingNativeDir = Join-Path (Join-Path $outputRootFullPath $Rid) "native"' -Issue "Stage-Runtime staging output must be driven by -Rid"
Assert-Contains -Violations $violations -Path $stageRuntimePath -Text $stageRuntimeText -Needle '$runtimeProjectNativeDir = Join-Path (Join-Path (Join-Path $runtimeProjectRootFullPath "runtimes") $Rid) "native"' -Issue "Stage-Runtime runtime package mirror must be driven by -Rid"
Assert-Contains -Violations $violations -Path $stageRuntimePath -Text $stageRuntimeText -Needle '[string]$RuntimeProject = "packaging/runtime/JYPPX.OpenCV.runtime"' -Issue "Stage-Runtime may keep win-x64 only as the current concrete runtime project default"
Assert-Contains -Violations $violations -Path $stageRuntimePath -Text $stageRuntimeText -Needle '[string]$RuntimeProfile = "full"' -Issue "Stage-Runtime must expose a runtime profile input"
Assert-Contains -Violations $violations -Path $stageRuntimePath -Text $stageRuntimeText -Needle 'Resolve-RepoPath' -Issue "Stage-Runtime must keep runtime input path resolution generic"

Assert-Contains -Violations $violations -Path $packWorkflowPath -Text $packWorkflowText -Needle "rid:" -Issue "Pack workflow must expose runtime identifier input"
Assert-Contains -Violations $violations -Path $packWorkflowPath -Text $packWorkflowText -Needle "runtime_profile:" -Issue "Pack workflow must expose runtime profile input"
Assert-Contains -Violations $violations -Path $packWorkflowPath -Text $packWorkflowText -Needle "default: all" -Issue "Pack workflow must default to the configured runtime matrix"
Assert-Contains -Violations $violations -Path $packWorkflowPath -Text $packWorkflowText -Needle "'-Rid', '`${{ matrix.rid }}'" -Issue "Pack workflow must pass matrix RID to Pack-Runtime"
Assert-Contains -Violations $violations -Path $packWorkflowPath -Text $packWorkflowText -Needle "'-RuntimeProfile', '`${{ matrix.profile }}'" -Issue "Pack workflow must pass matrix runtime profile to Pack-Runtime"
Assert-Contains -Violations $violations -Path $packWorkflowPath -Text $packWorkflowText -Needle "validate_synthetic_runtime" -Issue "Pack workflow must expose synthetic runtime validation mode"
Assert-Contains -Violations $violations -Path $packWorkflowPath -Text $packWorkflowText -Needle "Reject synthetic publish" -Issue "Pack workflow must reject publishing synthetic runtime validation packages"

foreach ($requiredRid in @("win-x64", "win-x86", "win-arm64", "linux-x64", "linux-arm64", "android-arm64", "android-arm", "android-x64", "android-x86")) {
    Assert-Contains -Violations $violations -Path $runtimeMatrixPath -Text $runtimeMatrixText -Needle "`"rid`": `"$requiredRid`"" -Issue "Runtime package matrix must include RID $requiredRid"
    Assert-Contains -Violations $violations -Path $packWorkflowPath -Text $packWorkflowText -Needle "rid: $requiredRid" -Issue "Pack workflow matrix must include RID $requiredRid"
}

foreach ($requiredProfile in @("full", "mini")) {
    Assert-Contains -Violations $violations -Path $runtimeMatrixPath -Text $runtimeMatrixText -Needle "`"name`": `"$requiredProfile`"" -Issue "Runtime package matrix must include profile $requiredProfile"
    Assert-Contains -Violations $violations -Path $packWorkflowPath -Text $packWorkflowText -Needle "profile: $requiredProfile" -Issue "Pack workflow matrix must include profile $requiredProfile"
}

Assert-Matches -Violations $violations -Path $runtimeProjectPath -Text $runtimeProjectText -Pattern "<RuntimePackageRid\b[^>]*>\s*$currentExampleRid\s*</RuntimePackageRid>" -Issue "Runtime package project may keep win-x64 only as its current default RuntimePackageRid"
Assert-Matches -Violations $violations -Path $runtimeProjectPath -Text $runtimeProjectText -Pattern "<RuntimePackageProfile\b[^>]*>\s*full\s*</RuntimePackageProfile>" -Issue "Runtime package project must define RuntimePackageProfile"
Assert-Matches -Violations $violations -Path $runtimeProjectPath -Text $runtimeProjectText -Pattern "<PackageId>\s*(?:JYPPX\.OpenCV\.runtime|\$\(OpenCvCSharpRuntimePackageIdPrefix\))\.\$\(RuntimePackageRid\)\$\(RuntimePackageProfileSuffix\)\s*</PackageId>" -Issue "Runtime package project PackageId must be derived from RuntimePackageRid and RuntimePackageProfile"
Assert-Contains -Violations $violations -Path $runtimeProjectPath -Text $runtimeProjectText -Needle 'Include="runtimes/$(RuntimePackageRid)/native/**/*"' -Issue "Runtime package project must pack RID-driven native payloads"
Assert-Contains -Violations $violations -Path $runtimeProjectPath -Text $runtimeProjectText -Needle 'PackagePath="runtimes/$(RuntimePackageRid)/native"' -Issue "Runtime package project PackagePath must be RID-driven"

foreach ($requiredText in @(
        "packaging/runtime/JYPPX.OpenCV.runtime/runtimes/",
        "packaging/runtime/JYPPX.OpenCV.runtime/licenses/")) {
    Assert-Contains -Violations $violations -Path $gitignorePath -Text $gitignoreText -Needle $requiredText -Issue ".gitignore must ignore generated mirrors for every runtime RID package project"
}

foreach ($doc in @(
        [pscustomobject]@{ Path = $readmePath; Text = $readmeText },
        [pscustomobject]@{ Path = $linkedRuntimeGuidePath; Text = $linkedRuntimeGuideText },
        [pscustomobject]@{ Path = $runtimeReadmePath; Text = $runtimeReadmeText },
        [pscustomobject]@{ Path = $nativeBoundaryPath; Text = $nativeBoundaryText },
        [pscustomobject]@{ Path = $runtimeLicensesPath; Text = $runtimeLicensesText })) {
    Assert-Contains -Violations $violations -Path $doc.Path -Text $doc.Text -Needle "$runtimePackagePrefix.<rid>" -Issue "$($doc.Path) must describe runtime packages generically as $runtimePackagePrefix.<rid>"
    Assert-Contains -Violations $violations -Path $doc.Path -Text $doc.Text -Needle $runtimePackageMiniShape -Issue "$($doc.Path) must describe mini runtime packages generically as $runtimePackageMiniShape"
}

Assert-Contains -Violations $violations -Path $readmePath -Text $readmeText -Needle "runtime package matrix" -Issue "README must describe the runtime package matrix"
Assert-Contains -Violations $violations -Path $linkedRuntimeGuidePath -Text $linkedRuntimeGuideText -Needle "multi-RID matrix" -Issue "Linked runtime guide must describe the workflow runtime matrix"
Assert-Contains -Violations $violations -Path $runtimeReadmePath -Text $runtimeReadmeText -Needle "runtime-package-matrix.json" -Issue "Runtime README must link the matrix definition"

Assert-Contains -Violations $violations -Path $runtimeReadmePath -Text $runtimeReadmeText -Needle "runtimes/<rid>/native" -Issue "Runtime README must document the generic RID-native package layout"
Assert-Contains -Violations $violations -Path $runtimeReadmePath -Text $runtimeReadmeText -Needle "RuntimePackageRid" -Issue "Runtime README must document the RuntimePackageRid-driven package project"
Assert-Contains -Violations $violations -Path $runtimeReadmePath -Text $runtimeReadmeText -Needle "RuntimePackageProfile" -Issue "Runtime README must document the RuntimePackageProfile-driven package project"
Assert-Contains -Violations $violations -Path $linkedRuntimeGuidePath -Text $linkedRuntimeGuideText -Needle "RuntimePackageRid" -Issue "Linked runtime build guide must document RuntimePackageRid-driven runtime projects"
Assert-Contains -Violations $violations -Path $linkedRuntimeGuidePath -Text $linkedRuntimeGuideText -Needle "RuntimePackageProfile" -Issue "Linked runtime build guide must document RuntimePackageProfile-driven runtime projects"
Assert-Contains -Violations $violations -Path $versionNeutralGuidePath -Text $versionNeutralGuideText -Needle "$runtimePackagePrefix.<rid>" -Issue "Version-neutral naming guide must document generic runtime package IDs"

$ridSurfaceFiles = @(
    $packRuntimePath,
    $stageRuntimePath,
    $packWorkflowPath,
    $runtimeProjectPath,
    $runtimeReadmePath,
    $readmePath,
    $linkedRuntimeGuidePath,
    $runtimeLicensesPath,
    $nativeBoundaryPath,
    $versionNeutralGuidePath
)

$fixedMajorContextFiles = @(
    $runtimeReadmePath,
    $readmePath,
    $linkedRuntimeGuidePath,
    $runtimeLicensesPath,
    $nativeBoundaryPath,
    $versionNeutralGuidePath
)

$winRuntimePackagePattern = [System.Text.RegularExpressions.Regex]::new(
    "JYPPX\.OpenCV\.runtime\.win-x64",
    [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
$currentExampleContextPattern = [System.Text.RegularExpressions.Regex]::new(
    "current|concrete|example|default|Windows x64|win-x64 runtime package README|当前|具体|示例|默认",
    [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)

foreach ($relativePath in $fixedMajorContextFiles) {
    $path = Join-Path $repo $relativePath
    $lineNumber = 0
    foreach ($line in [System.IO.File]::ReadLines($path)) {
        $lineNumber++
        if ($winRuntimePackagePattern.IsMatch($line) -and -not $currentExampleContextPattern.IsMatch($line)) {
            Add-Violation `
                -Violations $violations `
                -Path $relativePath `
                -Line $lineNumber `
                -Issue "win-x64 runtime package IDs/paths must be labelled as current concrete examples or defaults" `
                -Text $line
        }
    }
}

if ($violations.Count -gt 0) {
    Write-Host "Runtime RID package template scalability guard failed with $($violations.Count) violation(s)."
    $violations |
        Sort-Object Path, Line, Issue |
        Format-Table Path, Line, Issue, Text -AutoSize
    exit 1
}

Write-Host "Runtime RID package template scalability guard passed."
Write-Host "RID/package files checked: $($ridSurfaceFiles.Count)."
Write-Host "Runtime package matrix checked: full and mini profiles across configured RIDs."

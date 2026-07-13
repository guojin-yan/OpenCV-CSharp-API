param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$managedPackageId = "JYPPX.OpenCV.CSharp.API"
$runtimePackagePrefix = "JYPPX.OpenCV.runtime"
$primaryNativeLoader = "JYPPX.OpenCV.Native.dll"
$compatibilityNativeLoader = "OpenCv5Sharp.Native.dll"
$packageOutputRoot = "artifacts/packages"
$runtimeStagingRoot = "artifacts/runtime"
$uploadArtifactName = "nupkg"

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
        throw "Required release/package artifact surface file was not found: $RelativePath"
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

$packWorkflowPath = ".github/workflows/pack.yml"
$packManagedPath = "scripts/Pack-Managed.ps1"
$packRuntimePath = "scripts/Pack-Runtime.ps1"
$stageRuntimePath = "scripts/Stage-Runtime.ps1"
$runtimeProjectPath = "packaging/runtime/JYPPX.OpenCV.runtime/JYPPX.OpenCV.runtime.csproj"
$runtimeReadmePath = "packaging/runtime/JYPPX.OpenCV.runtime/README.md"
$readmePath = "README.md"
$linkedRuntimeGuidePath = "docs/articles/linked-runtime-build-guide.md"
$runtimeLicensesPath = "docs/articles/runtime-licenses.md"
$gitignorePath = ".gitignore"

$packWorkflowText = Read-RequiredText -RelativePath $packWorkflowPath
$packManagedText = Read-RequiredText -RelativePath $packManagedPath
$packRuntimeText = Read-RequiredText -RelativePath $packRuntimePath
$stageRuntimeText = Read-RequiredText -RelativePath $stageRuntimePath
$runtimeProjectText = Read-RequiredText -RelativePath $runtimeProjectPath
$runtimeReadmeText = Read-RequiredText -RelativePath $runtimeReadmePath
$readmeText = Read-RequiredText -RelativePath $readmePath
$linkedRuntimeGuideText = Read-RequiredText -RelativePath $linkedRuntimeGuidePath
$runtimeLicensesText = Read-RequiredText -RelativePath $runtimeLicensesPath
$gitignoreText = Read-RequiredText -RelativePath $gitignorePath

foreach ($check in @(
        [pscustomobject]@{ Needle = "scripts/Pack-Managed.ps1"; Issue = "Pack workflow must invoke the managed pack script" },
        [pscustomobject]@{ Needle = "scripts/Pack-Runtime.ps1"; Issue = "Pack workflow must invoke the runtime pack script" },
        [pscustomobject]@{ Needle = "name: $uploadArtifactName"; Issue = "Pack workflow upload artifact name must stay neutral" },
        [pscustomobject]@{ Needle = "path: $packageOutputRoot/*.nupkg"; Issue = "Pack workflow must upload neutral package output artifacts" },
        [pscustomobject]@{ Needle = "actions/download-artifact@v4"; Issue = "Pack workflow must download package artifacts for full-matrix self-validation" },
        [pscustomobject]@{ Needle = "scripts/Test-GitHubPackArtifactMatrixSurface.ps1"; Issue = "Pack workflow must verify downloaded package artifacts with the offline artifact guard" },
        [pscustomobject]@{ Needle = "dotnet nuget push ./artifacts/packages/*.nupkg"; Issue = "Pack workflow publish step must push from neutral package output root" })) {
    Assert-Contains `
        -Violations $violations `
        -Path $packWorkflowPath `
        -Text $packWorkflowText `
        -Needle $check.Needle `
        -Issue $check.Issue `
        -NormalizeSlashes
}

Assert-Contains -Violations $violations -Path $packManagedPath -Text $packManagedText -Needle '[string]$OutputDir = "artifacts\packages"' -Issue "Pack-Managed default output directory must be artifacts\packages"
Assert-Contains -Violations $violations -Path $packManagedPath -Text $packManagedText -Needle '[string]$ProjectPath = "src\OpenCvSharp\OpenCvSharp.csproj"' -Issue "Pack-Managed default project path must be the neutral managed project"
Assert-Contains -Violations $violations -Path $packManagedPath -Text $packManagedText -Needle "OpenCvCSharpManagedPackageId" -Issue "Pack-Managed must derive the neutral managed package ID from Directory.Build.props"
Assert-Contains -Violations $violations -Path $packManagedPath -Text $packManagedText -Needle "`$managedPackageId = Get-RequiredDirectoryBuildProperty" -Issue "Pack-Managed must assign the neutral managed package ID from the central metadata property"
Assert-Contains -Violations $violations -Path $packManagedPath -Text $packManagedText -Needle '$packagePath = Join-Path $outputFullPath "$managedPackageId.$packageFileVersion.nupkg"' -Issue "Pack-Managed package artifact file must be derived from neutral package ID plus normalized version"
Assert-Contains -Violations $violations -Path $packManagedPath -Text $packManagedText -Needle "Remove-Item -LiteralPath `$packagePath -Force" -Issue "Pack-Managed must remove stale expected package artifacts before packing"
Assert-Contains -Violations $violations -Path $packManagedPath -Text $packManagedText -Needle "Managed package artifact was not found" -Issue "Pack-Managed must verify the expected package artifact after packing"
Assert-Contains -Violations $violations -Path $packManagedPath -Text $packManagedText -Needle "PackageVersion carries OpenCV runtime identity as version metadata" -Issue "Pack-Managed must document PackageVersion as metadata, not package identity"

Assert-Contains -Violations $violations -Path $packRuntimePath -Text $packRuntimeText -Needle '[string]$OutputDir = "artifacts/packages"' -Issue "Pack-Runtime default output directory must be artifacts/packages"
Assert-Contains -Violations $violations -Path $packRuntimePath -Text $packRuntimeText -Needle '[string]$RuntimeProject = "packaging/runtime/JYPPX.OpenCV.runtime/JYPPX.OpenCV.runtime.csproj"' -Issue "Pack-Runtime default project path must be the neutral runtime package project"
Assert-Contains -Violations $violations -Path $packRuntimePath -Text $packRuntimeText -Needle '$runtimePackageId = "$runtimePackagePrefix.$Rid$runtimePackageSuffix"' -Issue "Pack-Runtime package ID must be derived from neutral runtime package prefix, RID, and profile suffix"
Assert-Contains -Violations $violations -Path $packRuntimePath -Text $packRuntimeText -Needle '$packagePath = Join-Path $outputFullPath "$runtimePackageId.$packageFileVersion.nupkg"' -Issue "Pack-Runtime package artifact file must be derived from neutral package ID plus normalized version"
Assert-Contains -Violations $violations -Path $packRuntimePath -Text $packRuntimeText -Needle '"-p:PackageId=$runtimePackageId"' -Issue "Pack-Runtime must pass the derived neutral package ID to dotnet pack"
Assert-Contains -Violations $violations -Path $packRuntimePath -Text $packRuntimeText -Needle "Remove-Item -LiteralPath `$packagePath -Force" -Issue "Pack-Runtime must remove stale expected package artifacts before packing"
Assert-Contains -Violations $violations -Path $packRuntimePath -Text $packRuntimeText -Needle "Runtime package artifact was not found" -Issue "Pack-Runtime must verify the expected package artifact after packing"
Assert-Contains -Violations $violations -Path $packRuntimePath -Text $packRuntimeText -Needle "PackageVersion carries OpenCV runtime identity as version metadata" -Issue "Pack-Runtime must document PackageVersion as metadata, not package identity"

Assert-Contains -Violations $violations -Path $stageRuntimePath -Text $stageRuntimeText -Needle '[string]$OutputRoot = "artifacts/runtime"' -Issue "Stage-Runtime default staging output root must be artifacts/runtime"
Assert-Contains -Violations $violations -Path $stageRuntimePath -Text $stageRuntimeText -Needle '[string]$RuntimeProject = "packaging/runtime/JYPPX.OpenCV.runtime"' -Issue "Stage-Runtime default runtime project root must use the neutral runtime package identity"
Assert-Contains -Violations $violations -Path $stageRuntimePath -Text $stageRuntimeText -Needle "JYPPX.OpenCV.Native.dll" -Issue "Stage-Runtime must stage the neutral primary native loader"
Assert-Contains -Violations $violations -Path $stageRuntimePath -Text $stageRuntimeText -Needle '$compatibilityNativeLoaderBaseName = "Open" + "Cv5Sharp.Native" # compatibility loader for already-compiled consumers' -Issue "Stage-Runtime must keep the fixed-major native loader only as a compatibility copy"
Assert-Contains -Violations $violations -Path $stageRuntimePath -Text $stageRuntimeText -Needle "compatibility loader copy for already-compiled consumers" -Issue "Stage-Runtime must label the fixed-major loader copy as compatibility-scoped"
Assert-Contains -Violations $violations -Path $stageRuntimePath -Text $stageRuntimeText -Needle "Runtime staging directory:" -Issue "Stage-Runtime must print staging directory evidence"
Assert-Contains -Violations $violations -Path $stageRuntimePath -Text $stageRuntimeText -Needle "Runtime package project directory:" -Issue "Stage-Runtime must print runtime package mirror evidence"

Assert-Matches -Violations $violations -Path $runtimeProjectPath -Text $runtimeProjectText -Pattern "<PackageId>\s*(?:JYPPX\.OpenCV\.runtime|\$\(OpenCvCSharpRuntimePackageIdPrefix\))\.\$\(RuntimePackageRid\)\$\(RuntimePackageProfileSuffix\)\s*</PackageId>" -Issue "Runtime package project PackageId must stay RID/profile-derived and version-neutral"
Assert-Contains -Violations $violations -Path $runtimeProjectPath -Text $runtimeProjectText -Needle "<PackageReadmeFile>README.md</PackageReadmeFile>" -Issue "Runtime package project must package README.md"
Assert-Contains -Violations $violations -Path $runtimeProjectPath -Text $runtimeProjectText -Needle 'Include="runtimes/$(RuntimePackageRid)/native/**/*"' -Issue "Runtime package project must include RID native runtime files"
Assert-Contains -Violations $violations -Path $runtimeProjectPath -Text $runtimeProjectText -Needle 'Include="licenses/**/*"' -Issue "Runtime package project must include generated license files"

foreach ($requiredText in @(
        "The package ID is version-neutral",
        $primaryNativeLoader,
        $compatibilityNativeLoader,
        "compatibility loader copy",
        "factual OpenCV 5.0.0 runtime artifacts",
        "not a naming pattern for new project concepts")) {
    Assert-Contains -Violations $violations -Path $runtimeReadmePath -Text $runtimeReadmeText -Needle $requiredText -Issue "Runtime package README must document '$requiredText'"
}

foreach ($doc in @(
        [pscustomobject]@{ Path = $readmePath; Text = $readmeText },
        [pscustomobject]@{ Path = $linkedRuntimeGuidePath; Text = $linkedRuntimeGuideText })) {
    foreach ($requiredText in @(
            'package IDs stay version-neutral',
            'artifacts\packages',
            'normalized `.nupkg`',
            'Before packing',
            '-PackageVersion',
            '-OpenCvNativeRuntimeDir')) {
        Assert-Contains -Violations $violations -Path $doc.Path -Text $doc.Text -Needle $requiredText -Issue "$($doc.Path) must document release package artifact rule '$requiredText'"
    }
}

foreach ($requiredText in @(
        "packaging/runtime/JYPPX.OpenCV.runtime/licenses/opencv-3rdparty",
        "CI packaging should always stage from the produced OpenCV install tree")) {
    Assert-Contains -Violations $violations -Path $runtimeLicensesPath -Text $runtimeLicensesText -Needle $requiredText -Issue "Runtime license guide must document '$requiredText'"
}

foreach ($requiredText in @(
        "artifacts/",
        "*.nupkg",
        "*.snupkg",
        "packaging/runtime/JYPPX.OpenCV.runtime/runtimes/",
        "packaging/runtime/JYPPX.OpenCV.runtime/licenses/")) {
    Assert-Contains -Violations $violations -Path $gitignorePath -Text $gitignoreText -Needle $requiredText -Issue ".gitignore must ignore generated package/release artifact path '$requiredText'"
}

$fixedMajorManagedIdentity = "Open" + "Cv5Sharp"
$fixedMajorRuntimeIdentity = $fixedMajorManagedIdentity + "\.runtime"
$fixedMajorRuntimeIdentityLower = "opencv" + "5sharp\.runtime"
$retiredFixedMajorRoot = "OpenCV-CSharp-API-opencv" + "5\.x"
$activeLeakRegex = [System.Text.RegularExpressions.Regex]::new(
    "$fixedMajorRuntimeIdentity|$fixedMajorRuntimeIdentityLower|dotnet\s+add\s+package\s+$fixedMajorManagedIdentity\b|Package" + "Reference[^\r\n]*$fixedMajorManagedIdentity|$retiredFixedMajorRoot",
    [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
$compatibilityNameRegex = [System.Text.RegularExpressions.Regex]::new(
    "OpenCv5Sharp|opencv5sharp",
    [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
$compatibilityContextRegex = [System.Text.RegularExpressions.Regex]::new(
    "compatib|legacy|existing|already-compiled|kept stable|explicit|兼容|既有|已编译|保留|明确",
    [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)

$releaseSurfaceFiles = @(
    $packWorkflowPath,
    $packManagedPath,
    $packRuntimePath,
    $stageRuntimePath,
    $runtimeProjectPath,
    $runtimeReadmePath,
    $readmePath,
    "CONTRIBUTING.md",
    "scripts/Test-GitHubPackArtifactMatrixSurface.ps1",
    $linkedRuntimeGuidePath,
    $runtimeLicensesPath
)

foreach ($relativePath in $releaseSurfaceFiles) {
    $path = Join-Path $repo $relativePath
    $lineNumber = 0
    foreach ($line in [System.IO.File]::ReadLines($path)) {
        $lineNumber++
        if ($activeLeakRegex.IsMatch($line)) {
            Add-Violation `
                -Violations $violations `
                -Path $relativePath `
                -Line $lineNumber `
                -Issue "Release/package surfaces must not use fixed-major package, install, or repository identities" `
                -Text $line
        }

        if ($compatibilityNameRegex.IsMatch($line) -and -not $compatibilityContextRegex.IsMatch($line)) {
            Add-Violation `
                -Violations $violations `
                -Path $relativePath `
                -Line $lineNumber `
                -Issue "Fixed-major loader/build-info mentions in release/package surfaces must be explicitly compatibility-scoped" `
                -Text $line
        }
    }
}

if ($violations.Count -gt 0) {
    Write-Host "Release package artifact surface guard failed with $($violations.Count) violation(s)."
    $violations |
        Sort-Object Path, Line, Issue |
        Format-Table Path, Line, Issue, Text -AutoSize
    exit 1
}

Write-Host "Release package artifact surface guard passed."
Write-Host "Release/package files checked: $($releaseSurfaceFiles.Count)."
Write-Host "Package output root: $packageOutputRoot."
Write-Host "Runtime staging root: $runtimeStagingRoot."
Write-Host "Upload artifact name: $uploadArtifactName."

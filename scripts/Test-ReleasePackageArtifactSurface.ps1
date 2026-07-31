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
$runtimeReleasePreflightPath = "scripts/Test-RuntimeReleaseCandidatePreflight.ps1"
$runtimeReleasePreflightGuardPath = "scripts/Test-RuntimeReleaseCandidatePreflightGuard.ps1"
$releaseCandidateProvenancePath = "scripts/Test-ReleaseCandidateProvenance.ps1"
$releaseReadinessContractPath = "scripts/Test-ReleaseReadinessContract.ps1"
$releaseSigningBoundaryPath = "scripts/Test-ReleaseSigningBoundary.ps1"
$releaseSupportContractPath = "scripts/Test-ReleaseSupportContract.ps1"
$publicFeedVerificationContractPath = "scripts/Test-PublicFeedVerificationContract.ps1"
$releaseChangeControlPath = "scripts/Test-ReleaseChangeControlRecord.ps1"
$apiAbiBaselinePath = "scripts/Test-ApiAbiBaselineContract.ps1"
$bindingMapGuardPath = "scripts/Test-NativeManagedBindingMap.ps1"
$bindingMapGeneratorPath = "scripts/Generate-NativeManagedBindingMap.ps1"
$bindingMapToolProjectPath = "tools/NativeManagedBindingMap/NativeManagedBindingMap.csproj"
$bindingMapToolProgramPath = "tools/NativeManagedBindingMap/Program.cs"
$bindingMapPath = "compatibility/native-managed-binding-map.txt"
$bindingMapSummaryPath = "compatibility/native-managed-binding-summary.json"
$imgProcMapGuardPath = "scripts/Test-ImgProcUpstreamMap.ps1"
$imgProcMapGeneratorPath = "scripts/Generate-ImgProcUpstreamMap.ps1"
$imgProcMapToolPath = "tools/ImgProcUpstreamMap/Program.cs"
$imgProcMapPath = "compatibility/imgproc-upstream-map.txt"
$imgProcMapSummaryPath = "compatibility/imgproc-upstream-summary.json"
$imgProcFamilyPath = "compatibility/imgproc-implemented-families.json"
$imgCodecsMapGuardPath = "scripts/Test-ImgCodecsUpstreamMap.ps1"
$imgCodecsMapGeneratorPath = "scripts/Generate-ImgCodecsUpstreamMap.ps1"
$imgCodecsMapToolPath = "tools/ImgCodecsUpstreamMap/Program.cs"
$imgCodecsMapPath = "compatibility/imgcodecs-upstream-map.txt"
$imgCodecsMapSummaryPath = "compatibility/imgcodecs-upstream-summary.json"
$imgCodecsFamilyPath = "compatibility/imgcodecs-implemented-families.json"
$imgCodecsExtensionsPath = "compatibility/imgcodecs-source-reviewed-extensions.json"
$finalCloseoutPath = "scripts/Test-ReleaseCandidateFinalCloseout.ps1"
$finalCloseoutRecordPath = "packaging/release/local-release-candidate-closeout.json"
$runtimeProjectPath = "packaging/runtime/JYPPX.OpenCV.runtime/JYPPX.OpenCV.runtime.csproj"
$runtimeReadmePath = "packaging/runtime/JYPPX.OpenCV.runtime/README.md"
$readmePath = "README.md"
$linkedRuntimeGuidePath = "docs/articles/linked-runtime-build-guide.md"
$apiAbiPolicyPath = "docs/articles/api-abi-compatibility-policy.md"
$supportLifecyclePolicyPath = "docs/articles/support-lifecycle-policy.md"
$releaseCloseoutDocPath = "docs/articles/release-candidate-closeout.md"
$runtimeLicensesPath = "docs/articles/runtime-licenses.md"
$githubPackArtifactGuardPath = "scripts/Test-GitHubPackArtifactMatrixSurface.ps1"
$githubPackConsumerGuardPath = "scripts/Test-GitHubPackConsumerRestoreSurface.ps1"
$gitignorePath = ".gitignore"

$packWorkflowText = Read-RequiredText -RelativePath $packWorkflowPath
$packManagedText = Read-RequiredText -RelativePath $packManagedPath
$packRuntimeText = Read-RequiredText -RelativePath $packRuntimePath
$stageRuntimeText = Read-RequiredText -RelativePath $stageRuntimePath
$runtimeReleasePreflightText = Read-RequiredText -RelativePath $runtimeReleasePreflightPath
$runtimeReleasePreflightGuardText = Read-RequiredText -RelativePath $runtimeReleasePreflightGuardPath
$releaseCandidateProvenanceText = Read-RequiredText -RelativePath $releaseCandidateProvenancePath
$releaseReadinessContractText = Read-RequiredText -RelativePath $releaseReadinessContractPath
$releaseSigningBoundaryText = Read-RequiredText -RelativePath $releaseSigningBoundaryPath
$releaseSupportContractText = Read-RequiredText -RelativePath $releaseSupportContractPath
$publicFeedVerificationContractText = Read-RequiredText -RelativePath $publicFeedVerificationContractPath
$releaseChangeControlText = Read-RequiredText -RelativePath $releaseChangeControlPath
$apiAbiBaselineText = Read-RequiredText -RelativePath $apiAbiBaselinePath
$bindingMapGuardText = Read-RequiredText -RelativePath $bindingMapGuardPath
$bindingMapGeneratorText = Read-RequiredText -RelativePath $bindingMapGeneratorPath
$bindingMapToolProjectText = Read-RequiredText -RelativePath $bindingMapToolProjectPath
$bindingMapToolProgramText = Read-RequiredText -RelativePath $bindingMapToolProgramPath
$bindingMapText = Read-RequiredText -RelativePath $bindingMapPath
$bindingMapSummaryText = Read-RequiredText -RelativePath $bindingMapSummaryPath
$imgProcMapGuardText = Read-RequiredText -RelativePath $imgProcMapGuardPath
$imgProcMapGeneratorText = Read-RequiredText -RelativePath $imgProcMapGeneratorPath
$imgProcMapToolText = Read-RequiredText -RelativePath $imgProcMapToolPath
$imgProcMapText = Read-RequiredText -RelativePath $imgProcMapPath
$imgProcMapSummaryText = Read-RequiredText -RelativePath $imgProcMapSummaryPath
$imgProcFamilyText = Read-RequiredText -RelativePath $imgProcFamilyPath
$imgCodecsMapGuardText = Read-RequiredText -RelativePath $imgCodecsMapGuardPath
$imgCodecsMapGeneratorText = Read-RequiredText -RelativePath $imgCodecsMapGeneratorPath
$imgCodecsMapToolText = Read-RequiredText -RelativePath $imgCodecsMapToolPath
$imgCodecsMapText = Read-RequiredText -RelativePath $imgCodecsMapPath
$imgCodecsMapSummaryText = Read-RequiredText -RelativePath $imgCodecsMapSummaryPath
$imgCodecsFamilyText = Read-RequiredText -RelativePath $imgCodecsFamilyPath
$imgCodecsExtensionsText = Read-RequiredText -RelativePath $imgCodecsExtensionsPath
$finalCloseoutText = Read-RequiredText -RelativePath $finalCloseoutPath
$finalCloseoutRecordText = Read-RequiredText -RelativePath $finalCloseoutRecordPath
$runtimeProjectText = Read-RequiredText -RelativePath $runtimeProjectPath
$runtimeReadmeText = Read-RequiredText -RelativePath $runtimeReadmePath
$readmeText = Read-RequiredText -RelativePath $readmePath
$linkedRuntimeGuideText = Read-RequiredText -RelativePath $linkedRuntimeGuidePath
$apiAbiPolicyText = Read-RequiredText -RelativePath $apiAbiPolicyPath
$supportLifecyclePolicyText = Read-RequiredText -RelativePath $supportLifecyclePolicyPath
$releaseCloseoutDocText = Read-RequiredText -RelativePath $releaseCloseoutDocPath
$runtimeLicensesText = Read-RequiredText -RelativePath $runtimeLicensesPath
$githubPackArtifactGuardText = Read-RequiredText -RelativePath $githubPackArtifactGuardPath
$gitignoreText = Read-RequiredText -RelativePath $gitignorePath

foreach ($check in @(
        [pscustomobject]@{ Needle = "scripts/Pack-Managed.ps1"; Issue = "Pack workflow must invoke the managed pack script" },
        [pscustomobject]@{ Needle = "scripts/Pack-Runtime.ps1"; Issue = "Pack workflow must invoke the runtime pack script" },
        [pscustomobject]@{ Needle = "name: $uploadArtifactName"; Issue = "Pack workflow upload artifact name must stay neutral" },
        [pscustomobject]@{ Needle = "path: $packageOutputRoot/*.nupkg"; Issue = "Pack workflow must upload neutral package output artifacts" },
        [pscustomobject]@{ Needle = "uses: actions/download-artifact@"; Issue = "Pack workflow must download package artifacts for full-matrix self-validation" },
        [pscustomobject]@{ Needle = "scripts/Test-GitHubPackArtifactMatrixSurface.ps1"; Issue = "Pack workflow must verify downloaded package artifacts with the offline artifact guard" },
        [pscustomobject]@{ Needle = "scripts/Test-GitHubPackConsumerRestoreSurface.ps1"; Issue = "Pack workflow must verify downloaded package artifacts with the offline consumer restore guard" },
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
Assert-Contains -Violations $violations -Path $apiAbiBaselinePath -Text $apiAbiBaselineText -Needle 'managed-public-api.txt' -Issue "Release artifact surface must register the managed API baseline"
Assert-Contains -Violations $violations -Path $apiAbiBaselinePath -Text $apiAbiBaselineText -Needle 'legacy_abi_mini_manifest.txt' -Issue "Release artifact surface must register the mini ABI baseline"
Assert-Contains -Violations $violations -Path $bindingMapGuardPath -Text $bindingMapGuardText -Needle 'Generate-NativeManagedBindingMap.ps1' -Issue "Release artifact surface must freshness-check the native-to-managed binding map"
Assert-Contains -Violations $violations -Path $bindingMapGuardPath -Text $bindingMapGuardText -Needle 'native=2545 bound=2545 unbound=0 managed_only=0' -Issue "Native-to-managed binding guard must emit complete parity evidence"
Assert-Contains -Violations $violations -Path $bindingMapGeneratorPath -Text $bindingMapGeneratorText -Needle 'tools/NativeManagedBindingMap/NativeManagedBindingMap.csproj' -Issue "Binding-map generator must invoke its checked-in tool project"
Assert-Contains -Violations $violations -Path $bindingMapToolProjectPath -Text $bindingMapToolProjectText -Needle '<TargetFramework>net10.0</TargetFramework>' -Issue "Binding-map tool must retain its exact modern target framework"
Assert-Contains -Violations $violations -Path $bindingMapToolProgramPath -Text $bindingMapToolProgramText -Needle 'NATIVE_MANAGED_BINDING_MAP_OK' -Issue "Binding-map tool must emit deterministic parity evidence"
Assert-Contains -Violations $violations -Path $bindingMapPath -Text $bindingMapText -Needle '[managed-only]' -Issue "Release artifact surface must include the complete native-to-managed mapping output"
Assert-Contains -Violations $violations -Path $bindingMapSummaryPath -Text $bindingMapSummaryText -Needle '"managedBoundCount": 2545' -Issue "Binding-map summary must record all native functions as managed-bound"
Assert-Contains -Violations $violations -Path $bindingMapSummaryPath -Text $bindingMapSummaryText -Needle '"unboundCount": 0' -Issue "Binding-map summary must record zero unbound native functions"
Assert-Contains -Violations $violations -Path $imgProcMapGuardPath -Text $imgProcMapGuardText -Needle 'Generate-ImgProcUpstreamMap.ps1' -Issue "Release artifact surface must freshness-check the ImgProc upstream map"
Assert-Contains -Violations $violations -Path $imgProcMapGeneratorPath -Text $imgProcMapGeneratorText -Needle '-RegenerateRaw requires -PythonPath' -Issue "ImgProc raw extraction must require an explicit Python path"
Assert-Contains -Violations $violations -Path $imgProcMapToolPath -Text $imgProcMapToolText -Needle 'RepositoryWideUpstreamParityClaimed = false' -Issue "ImgProc map tool must retain the exact non-repository-wide claim boundary"
Assert-Contains -Violations $violations -Path $imgProcMapPath -Text $imgProcMapText -Needle 'repository-wide-upstream-parity=false' -Issue "Release artifact surface must include the bounded ImgProc mapping output"
Assert-Contains -Violations $violations -Path $imgProcMapSummaryPath -Text $imgProcMapSummaryText -Needle '"declarationCount": 203' -Issue "ImgProc map summary must record the exact declaration count"
Assert-Contains -Violations $violations -Path $imgProcMapSummaryPath -Text $imgProcMapSummaryText -Needle '"implemented": 161' -Issue "ImgProc map summary must record implemented callable evidence"
Assert-Contains -Violations $violations -Path $imgProcFamilyPath -Text $imgProcFamilyText -Needle '"managedPublicMemberAdditionCount": 174' -Issue "ImgProc family inventory must record the reviewed managed API addition count"
Assert-Contains -Violations $violations -Path $imgCodecsMapGuardPath -Text $imgCodecsMapGuardText -Needle 'Generate-ImgCodecsUpstreamMap.ps1' -Issue "Release artifact surface must freshness-check the ImgCodecs upstream map"
Assert-Contains -Violations $violations -Path $imgCodecsMapGeneratorPath -Text $imgCodecsMapGeneratorText -Needle '-RegenerateRaw requires -PythonPath' -Issue "ImgCodecs raw extraction must require an explicit Python path"
Assert-Contains -Violations $violations -Path $imgCodecsMapToolPath -Text $imgCodecsMapToolText -Needle 'RepositoryWideUpstreamParityClaimed = false' -Issue "ImgCodecs map tool must retain the exact non-repository-wide claim boundary"
Assert-Contains -Violations $violations -Path $imgCodecsMapPath -Text $imgCodecsMapText -Needle 'repository-wide-upstream-parity=false' -Issue "Release artifact surface must include the bounded ImgCodecs mapping output"
Assert-Contains -Violations $violations -Path $imgCodecsMapSummaryPath -Text $imgCodecsMapSummaryText -Needle '"declarationCount": 39' -Issue "ImgCodecs map summary must record the exact declaration count"
Assert-Contains -Violations $violations -Path $imgCodecsMapSummaryPath -Text $imgCodecsMapSummaryText -Needle '"implemented": 22' -Issue "ImgCodecs map summary must record all callable evidence"
Assert-Contains -Violations $violations -Path $imgCodecsFamilyPath -Text $imgCodecsFamilyText -Needle '"managedPublicMemberAdditionCount": 168' -Issue "ImgCodecs family inventory must record the reviewed managed API addition count"
Assert-Contains -Violations $violations -Path $imgCodecsExtensionsPath -Text $imgCodecsExtensionsText -Needle '"identity": "cv::ImageCollection"' -Issue "ImgCodecs source-reviewed extension inventory must retain ImageCollection evidence"
Assert-Contains -Violations $violations -Path $finalCloseoutPath -Text $finalCloseoutText -Needle 'local-release-candidate-closeout.json' -Issue "Release artifact surface must register the final closeout record"
Assert-Contains -Violations $violations -Path $finalCloseoutRecordPath -Text $finalCloseoutRecordText -Needle 'local-release-candidate-closeout' -Issue "Final closeout record must identify its record kind"
Assert-Contains -Violations $violations -Path $apiAbiPolicyPath -Text $apiAbiPolicyText -Needle 'compatibility/api-gap-inventory.json' -Issue "API/ABI policy must expose the gap inventory"
Assert-Contains -Violations $violations -Path $supportLifecyclePolicyPath -Text $supportLifecyclePolicyText -Needle '24' -Issue "Support lifecycle policy must expose the real-support count"
Assert-Contains -Violations $violations -Path $releaseCloseoutDocPath -Text $releaseCloseoutDocText -Needle 'locally-validated' -Issue "Release closeout documentation must expose local validation state"
Assert-Contains -Violations $violations -Path $packRuntimePath -Text $packRuntimeText -Needle '[string]$RuntimeProject = "packaging/runtime/JYPPX.OpenCV.runtime/JYPPX.OpenCV.runtime.csproj"' -Issue "Pack-Runtime default project path must be the neutral runtime package project"
Assert-Contains -Violations $violations -Path $packRuntimePath -Text $packRuntimeText -Needle '$runtimePackageId = "$runtimePackagePrefix.$Rid$runtimePackageSuffix"' -Issue "Pack-Runtime package ID must be derived from neutral runtime package prefix, RID, and profile suffix"
Assert-Contains -Violations $violations -Path $packRuntimePath -Text $packRuntimeText -Needle '$packagePath = Join-Path $outputFullPath "$runtimePackageId.$packageFileVersion.nupkg"' -Issue "Pack-Runtime package artifact file must be derived from neutral package ID plus normalized version"
Assert-Contains -Violations $violations -Path $packRuntimePath -Text $packRuntimeText -Needle '"-p:PackageId=$runtimePackageId"' -Issue "Pack-Runtime must pass the derived neutral package ID to dotnet pack"
Assert-Contains -Violations $violations -Path $packRuntimePath -Text $packRuntimeText -Needle "Remove-Item -LiteralPath `$packagePath -Force" -Issue "Pack-Runtime must remove stale expected package artifacts before packing"
Assert-Contains -Violations $violations -Path $packRuntimePath -Text $packRuntimeText -Needle "Runtime package artifact was not found" -Issue "Pack-Runtime must verify the expected package artifact after packing"
Assert-Contains -Violations $violations -Path $packRuntimePath -Text $packRuntimeText -Needle "PackageVersion carries OpenCV runtime identity as version metadata" -Issue "Pack-Runtime must document PackageVersion as metadata, not package identity"
Assert-Contains -Violations $violations -Path $packRuntimePath -Text $packRuntimeText -Needle '[switch]$SyntheticRuntimeInputs' -Issue "Pack-Runtime must expose synthetic runtime provenance marking"
Assert-Contains -Violations $violations -Path $packRuntimePath -Text $packRuntimeText -Needle '[switch]$RequireReleasePreflight' -Issue "Pack-Runtime must expose release-candidate runtime preflight"
Assert-Contains -Violations $violations -Path $packRuntimePath -Text $packRuntimeText -Needle 'Test-RuntimeReleaseCandidatePreflight.ps1' -Issue "Pack-Runtime must invoke release-candidate runtime preflight when requested"
Assert-Contains -Violations $violations -Path $packRuntimePath -Text $packRuntimeText -Needle 'RuntimePackageId = $runtimePackageId' -Issue "Pack-Runtime must forward derived package ID to runtime staging provenance"
Assert-Contains -Violations $violations -Path $packRuntimePath -Text $packRuntimeText -Needle 'PackageVersion = $PackageVersion' -Issue "Pack-Runtime must forward package version metadata to runtime staging provenance"
Assert-Contains -Violations $violations -Path $packRuntimePath -Text $packRuntimeText -Needle '$stageParameters.SyntheticRuntimeInputs = $true' -Issue "Pack-Runtime must forward synthetic runtime input status to staging provenance"

Assert-Contains -Violations $violations -Path $stageRuntimePath -Text $stageRuntimeText -Needle '[string]$OutputRoot = "artifacts/runtime"' -Issue "Stage-Runtime default staging output root must be artifacts/runtime"
Assert-Contains -Violations $violations -Path $stageRuntimePath -Text $stageRuntimeText -Needle '[string]$RuntimeProject = "packaging/runtime/JYPPX.OpenCV.runtime"' -Issue "Stage-Runtime default runtime project root must use the neutral runtime package identity"
Assert-Contains -Violations $violations -Path $stageRuntimePath -Text $stageRuntimeText -Needle "JYPPX.OpenCV.Native.dll" -Issue "Stage-Runtime must stage the neutral primary native loader"
Assert-Contains -Violations $violations -Path $stageRuntimePath -Text $stageRuntimeText -Needle '$compatibilityNativeLoaderBaseName = "Open" + "Cv5Sharp.Native" # compatibility loader for already-compiled consumers' -Issue "Stage-Runtime must keep the fixed-major native loader only as a compatibility copy"
Assert-Contains -Violations $violations -Path $stageRuntimePath -Text $stageRuntimeText -Needle "compatibility loader copy for already-compiled consumers" -Issue "Stage-Runtime must label the fixed-major loader copy as compatibility-scoped"
Assert-Contains -Violations $violations -Path $stageRuntimePath -Text $stageRuntimeText -Needle "Runtime staging directory:" -Issue "Stage-Runtime must print staging directory evidence"
Assert-Contains -Violations $violations -Path $stageRuntimePath -Text $stageRuntimeText -Needle "Runtime package project directory:" -Issue "Stage-Runtime must print runtime package mirror evidence"
Assert-Contains -Violations $violations -Path $stageRuntimePath -Text $stageRuntimeText -Needle 'JYPPX.OpenCV.runtime.provenance.json' -Issue "Stage-Runtime must generate a durable runtime provenance manifest"
Assert-Contains -Violations $violations -Path $stageRuntimePath -Text $stageRuntimeText -Needle 'SyntheticRuntimeInputs = [bool]$SyntheticRuntimeInputs.IsPresent' -Issue "Stage-Runtime provenance manifest must distinguish synthetic validation inputs"
Assert-Contains -Violations $violations -Path $stageRuntimePath -Text $stageRuntimeText -Needle 'RequiredModules = @($OpenCvModules)' -Issue "Stage-Runtime provenance manifest must record required OpenCV modules"
Assert-Contains -Violations $violations -Path $stageRuntimePath -Text $stageRuntimeText -Needle 'Runtime provenance manifest:' -Issue "Stage-Runtime must print runtime provenance manifest evidence"

Assert-Contains -Violations $violations -Path $runtimeReleasePreflightPath -Text $runtimeReleasePreflightText -Needle 'Release candidate preflight rejects synthetic runtime inputs' -Issue "Runtime release preflight must reject synthetic runtime inputs by default"
Assert-Contains -Violations $violations -Path $runtimeReleasePreflightPath -Text $runtimeReleasePreflightText -Needle 'contain no stale files' -Issue "Runtime release preflight must reject stale native/license/build mirrors"
Assert-Contains -Violations $violations -Path $runtimeReleasePreflightPath -Text $runtimeReleasePreflightText -Needle 'Runtime provenance required modules must match selected profile' -Issue "Runtime release preflight must validate profile module provenance"
Assert-Contains -Violations $violations -Path $runtimeReleasePreflightGuardPath -Text $runtimeReleasePreflightGuardText -Needle 'Pack-Runtime -RequireReleasePreflight integration should pass for release-shaped staged inputs' -Issue "Runtime release preflight guard must exercise the actual Pack-Runtime -RequireReleasePreflight path"
Assert-Contains -Violations $violations -Path $runtimeReleasePreflightGuardPath -Text $runtimeReleasePreflightGuardText -Needle 'Synthetic release-preflight negative path must not produce a runtime package' -Issue "Runtime release preflight guard must prove synthetic preflight integration does not produce packages"
Assert-Contains -Violations $violations -Path $runtimeReleasePreflightGuardPath -Text $runtimeReleasePreflightGuardText -Needle 'Pack-Runtime -RequireReleasePreflight produces a package only for non-synthetic staged inputs.' -Issue "Runtime release preflight guard must cover positive and negative pack integration cases"
Assert-Contains -Violations $violations -Path $releaseCandidateProvenancePath -Text $releaseCandidateProvenanceText -Needle 'Deterministic package manifest' -Issue "Release candidate provenance guard must produce deterministic package manifest evidence"
Assert-Contains -Violations $violations -Path $releaseCandidateProvenancePath -Text $releaseCandidateProvenanceText -Needle "SignatureStatus = 'not-ready'" -Issue "Release candidate provenance guard must keep local signing readiness explicit"
Assert-Contains -Violations $violations -Path $releaseCandidateProvenancePath -Text $releaseCandidateProvenanceText -Needle "SbomStatus = 'not-ready'" -Issue "Release candidate provenance guard must keep local SBOM readiness explicit"
Assert-Contains -Violations $violations -Path $releaseCandidateProvenancePath -Text $releaseCandidateProvenanceText -Needle 'PublicationAllowed = $false' -Issue "Release candidate provenance guard must remain non-publishing"
Assert-Contains -Violations $violations -Path $releaseCandidateProvenancePath -Text $releaseCandidateProvenanceText -Needle "local-preflight-only; no remote mutation" -Issue "Release candidate provenance guard must record rollback abort metadata"
Assert-Contains -Violations $violations -Path $releaseReadinessContractPath -Text $releaseReadinessContractText -Needle 'Signature/SBOM state machine' -Issue "Release readiness contract must validate signing and SBOM state transitions"
Assert-Contains -Violations $violations -Path $releaseReadinessContractPath -Text $releaseReadinessContractText -Needle 'Private key material must never be present' -Issue "Release readiness contract must reject private key material"
Assert-Contains -Violations $violations -Path $releaseReadinessContractPath -Text $releaseReadinessContractText -Needle 'pending-hosted-evidence' -Issue "Release readiness contract must keep hosted Windows x86 promotion pending"
Assert-Contains -Violations $violations -Path $releaseReadinessContractPath -Text $releaseReadinessContractText -Needle 'read-only-fixture' -Issue "Release readiness contract must keep public feed verification read-only"
Assert-Contains -Violations $violations -Path $releaseSigningBoundaryPath -Text $releaseSigningBoundaryText -Needle 'RELEASE_SIGNING_BOUNDARY_OK' -Issue "Release signing boundary must emit normalized-input evidence"
Assert-Contains -Violations $violations -Path $releaseSigningBoundaryPath -Text $releaseSigningBoundaryText -Needle 'PostSigningPackageSha256' -Issue "Release signing boundary must bind post-signing package bytes"
Assert-Contains -Violations $violations -Path $releaseSigningBoundaryPath -Text $releaseSigningBoundaryText -Needle 'SPDX-2.3' -Issue "Release signing boundary must bind SPDX-2.3 SBOM provenance"
Assert-Contains -Violations $violations -Path $releaseSigningBoundaryPath -Text $releaseSigningBoundaryText -Needle 'RemoteMutationAllowed' -Issue "Release signing boundary must keep approval unable to mutate remote state"
Assert-Contains -Violations $violations -Path $releaseSigningBoundaryPath -Text $releaseSigningBoundaryText -Needle 'dotnet nuget sign' -Issue "Release signing boundary must reject direct workflow signing"
Assert-Contains -Violations $violations -Path $releaseSupportContractPath -Text $releaseSupportContractText -Needle 'RELEASE_SUPPORT_CONTRACT_OK' -Issue "Release support contract must emit explicit matrix/support classification"
Assert-Contains -Violations $violations -Path $releaseSupportContractPath -Text $releaseSupportContractText -Needle 'packageSurfaceIsSupport' -Issue "Release support contract must separate package surface from real support"
Assert-Contains -Violations $violations -Path $publicFeedVerificationContractPath -Text $publicFeedVerificationContractText -Needle 'NUGET_PUBLIC_FEED_READ_ONLY_OK' -Issue "Public feed contract must emit read-only verification evidence"
Assert-Contains -Violations $violations -Path $publicFeedVerificationContractPath -Text $publicFeedVerificationContractText -Needle 'https_only=true' -Issue "Public feed contract must require HTTPS-only verification"
Assert-Contains -Violations $violations -Path $publicFeedVerificationContractPath -Text $publicFeedVerificationContractText -Needle 'upload_attempted=false' -Issue "Public feed contract must reject upload during verification"
Assert-Contains -Violations $violations -Path $publicFeedVerificationContractPath -Text $publicFeedVerificationContractText -Needle 'api.nuget.org/v3-flatcontainer' -Issue "Public feed contract must use the exact NuGet flat-container path"
Assert-Contains -Violations $violations -Path $releaseCandidateProvenancePath -Text $releaseCandidateProvenanceText -Needle 'SigningHandoff' -Issue "Release provenance must carry the signing handoff contract"
Assert-Contains -Violations $violations -Path $releaseCandidateProvenancePath -Text $releaseCandidateProvenanceText -Needle 'SbomHandoff' -Issue "Release provenance must carry the SBOM handoff contract"
Assert-Contains -Violations $violations -Path $releaseChangeControlPath -Text $releaseChangeControlText -Needle 'RELEASE_CHANGE_CONTROL_OK' -Issue "Release change-control guard must emit deterministic review evidence"
Assert-Contains -Violations $violations -Path $releaseChangeControlPath -Text $releaseChangeControlText -Needle "Decision = 'do-not-publish'" -Issue "Release change-control guard must default to non-publishing"
Assert-Contains -Violations $violations -Path $releaseChangeControlPath -Text $releaseChangeControlText -Needle "Status = 'not-approved'" -Issue "Release change-control guard must require explicit approval"
Assert-Contains -Violations $violations -Path $releaseChangeControlPath -Text $releaseChangeControlText -Needle 'hosted-evidence-pending' -Issue "Release change-control guard must keep win-x86/full pending"

Assert-Matches -Violations $violations -Path $runtimeProjectPath -Text $runtimeProjectText -Pattern "<PackageId>\s*(?:JYPPX\.OpenCV\.runtime|\$\(OpenCvCSharpRuntimePackageIdPrefix\))\.\$\(RuntimePackageRid\)\$\(RuntimePackageProfileSuffix\)\s*</PackageId>" -Issue "Runtime package project PackageId must stay RID/profile-derived and version-neutral"
Assert-Contains -Violations $violations -Path $runtimeProjectPath -Text $runtimeProjectText -Needle "<PackageReadmeFile>README.md</PackageReadmeFile>" -Issue "Runtime package project must package README.md"
Assert-Contains -Violations $violations -Path $runtimeProjectPath -Text $runtimeProjectText -Needle 'Include="runtimes/$(RuntimePackageRid)/native/**/*"' -Issue "Runtime package project must include RID native runtime files"
Assert-Contains -Violations $violations -Path $runtimeProjectPath -Text $runtimeProjectText -Needle 'Include="licenses/**/*"' -Issue "Runtime package project must include generated license files"
Assert-Contains -Violations $violations -Path $runtimeProjectPath -Text $runtimeProjectText -Needle 'Include="build/JYPPX.OpenCV.runtime.provenance.json"' -Issue "Runtime package project must include the generated provenance manifest"

Assert-Contains -Violations $violations -Path $packWorkflowPath -Text $packWorkflowText -Needle '-SyntheticRuntimeInputs' -Issue "Pack workflow must mark synthetic runtime validation packages in provenance"
Assert-Contains -Violations $violations -Path $packWorkflowPath -Text $packWorkflowText -Needle '-RequireReleasePreflight' -Issue "Pack workflow must require runtime release preflight before publish-capable runtime package pushes"
Assert-Contains -Violations $violations -Path $packWorkflowPath -Text $packWorkflowText -Needle '-ExpectedPackageVersion $packageVersion' -Issue "Pack workflow artifact verifier must validate workflow-derived package version metadata"
Assert-Contains -Violations $violations -Path $packWorkflowPath -Text $packWorkflowText -Needle '-ExpectedSyntheticRuntimeInputs $expectedSyntheticRuntimeInputs' -Issue "Pack workflow artifact verifier must validate synthetic runtime provenance status"

Assert-Contains -Violations $violations -Path $githubPackArtifactGuardPath -Text $githubPackArtifactGuardText -Needle 'JYPPX.OpenCV.runtime.provenance.json' -Issue "GitHub artifact matrix guard must inspect runtime provenance manifests"
Assert-Contains -Violations $violations -Path $githubPackArtifactGuardPath -Text $githubPackArtifactGuardText -Needle '$ExpectedSyntheticRuntimeInputs' -Issue "GitHub artifact matrix guard must validate expected synthetic runtime provenance status"
Assert-Contains -Violations $violations -Path $githubPackArtifactGuardPath -Text $githubPackArtifactGuardText -Needle 'PrimaryNativeLoaderName' -Issue "GitHub artifact matrix guard must validate provenance native loader names"

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
        "packaging/runtime/JYPPX.OpenCV.runtime/licenses/",
        "packaging/runtime/JYPPX.OpenCV.runtime/build/")) {
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
    $runtimeReleasePreflightPath,
    $runtimeReleasePreflightGuardPath,
    $apiAbiBaselinePath,
    $bindingMapGuardPath,
    $bindingMapGeneratorPath,
    $bindingMapToolProjectPath,
    $bindingMapToolProgramPath,
    $finalCloseoutPath,
    $finalCloseoutRecordPath,
    $githubPackArtifactGuardPath,
    $githubPackConsumerGuardPath,
    $linkedRuntimeGuidePath,
    $runtimeLicensesPath,
    $apiAbiPolicyPath,
    $supportLifecyclePolicyPath,
    $releaseCloseoutDocPath
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

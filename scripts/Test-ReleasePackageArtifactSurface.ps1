param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$managedPackageId = "JYPPX.OpenCV.CSharp.API"
$runtimePackagePrefix = "JYPPX.OpenCV.runtime"
$primaryNativeLoader = "JYPPX.OpenCV.Native.dll"
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
$runtimeInputWorkflowPath = ".github/workflows/runtime-input.yml"
$publishNugetWorkflowPath = ".github/workflows/publish-nuget.yml"
$packManagedPath = "scripts/Pack-Managed.ps1"
$packRuntimePath = "scripts/Pack-Runtime.ps1"
$stageRuntimePath = "scripts/Stage-Runtime.ps1"
$runtimeReleasePreflightPath = "scripts/Test-RuntimeReleaseCandidatePreflight.ps1"
$runtimeReleasePreflightGuardPath = "scripts/Test-RuntimeReleaseCandidatePreflightGuard.ps1"
$releaseCandidateProvenancePath = "scripts/Test-ReleaseCandidateProvenance.ps1"
$releaseReadinessContractPath = "scripts/Test-ReleaseReadinessContract.ps1"
$releasePackageSbomGeneratorPath = "scripts/New-ReleasePackageSbom.ps1"
$releasePackageSbomGuardPath = "scripts/Test-ReleasePackageSbom.ps1"
$releaseSigningBoundaryPath = "scripts/Test-ReleaseSigningBoundary.ps1"
$nugetRepositorySigningBoundaryPath = "scripts/Test-NuGetRepositorySigningBoundary.ps1"
$nugetRepositorySignedPackagePath = "scripts/Test-NuGetRepositorySignedPackage.ps1"
$nugetPublicationBundlePath = "scripts/New-NuGetPublicationBundle.ps1"
$nugetPublicationManifestPath = "scripts/Test-NuGetPublicationManifest.ps1"
$nugetRepositoryVerifierProjectPath = "tools/NuGetRepositorySignatureVerifier/NuGetRepositorySignatureVerifier.csproj"
$nugetRepositoryVerifierProgramPath = "tools/NuGetRepositorySignatureVerifier/Program.cs"
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
$highGuiMapGuardPath = "scripts/Test-HighGuiUpstreamMap.ps1"
$highGuiMapGeneratorPath = "scripts/Generate-HighGuiUpstreamMap.ps1"
$highGuiMapToolPath = "tools/HighGuiUpstreamMap/Program.cs"
$highGuiMapPath = "compatibility/highgui-upstream-map.txt"
$highGuiMapSummaryPath = "compatibility/highgui-upstream-summary.json"
$highGuiFamilyPath = "compatibility/highgui-implemented-families.json"
$finalCloseoutPath = "scripts/Test-ReleaseCandidateFinalCloseout.ps1"
$finalCloseoutRecordPath = "packaging/release/local-release-candidate-closeout.json"
$runtimeProjectPath = "packaging/runtime/JYPPX.OpenCV.runtime/JYPPX.OpenCV.runtime.csproj"
$androidRuntimeTargetsPath = "packaging/runtime/JYPPX.OpenCV.runtime/buildTransitive/JYPPX.OpenCV.runtime.targets"
$androidRuntimeProducerPath = "scripts/Build-AndroidRuntimeInput.ps1"
$androidSmokeProjectPath = "samples/AndroidSmoke/AndroidSmoke.csproj"
$androidSmokeActivityPath = "samples/AndroidSmoke/MainActivity.cs"
$runtimeReadmePath = "packaging/runtime/JYPPX.OpenCV.runtime/README.md"
$readmePath = "README.md"
$chineseReadmePath = "README_cn.md"
$runtimeSupportContractPath = "packaging/runtime/runtime-support-contract.json"
$linkedRuntimeGuidePath = "docs/articles/linked-runtime-build-guide.md"
$apiAbiPolicyPath = "docs/articles/api-abi-compatibility-policy.md"
$supportLifecyclePolicyPath = "docs/articles/support-lifecycle-policy.md"
$releaseCloseoutDocPath = "docs/articles/release-candidate-closeout.md"
$releaseNotesPath = "docs/articles/release-notes.md"
$nugetRepositorySigningGuidePath = "docs/articles/nuget-repository-signing-guide.md"
$runtimeLicensesPath = "docs/articles/runtime-licenses.md"
$consoleSampleProgramPath = "samples/ConsoleSamples/Program.cs"
$tutorialRunnerPath = "samples/ConsoleSamples/ShowcaseRunner.cs"
$fontFacePath = "src/OpenCvSharp/ImgProc/FontFace.cs"
$putTextApiPath = "src/OpenCvSharp/ImgProc/Cv2.RemainingParity.cs"
$nativeImgProcPath = "src/OpenCvSharp.Native/src/imgproc.cpp"
$tutorialSeriesPath = "docs/articles/tutorial-series.md"
$chinesePutTextTutorialPath = "docs/articles/tutorial-02-chinese-puttext.md"
$androidTutorialPath = "docs/articles/tutorial-07-android-runtime.md"
$docsTocPath = "docs/toc.yml"
$githubPackArtifactGuardPath = "scripts/Test-GitHubPackArtifactMatrixSurface.ps1"
$githubPackConsumerGuardPath = "scripts/Test-GitHubPackConsumerRestoreSurface.ps1"
$gitignorePath = ".gitignore"

$packWorkflowText = Read-RequiredText -RelativePath $packWorkflowPath
$runtimeInputWorkflowText = Read-RequiredText -RelativePath $runtimeInputWorkflowPath
$publishNugetWorkflowText = Read-RequiredText -RelativePath $publishNugetWorkflowPath
$packManagedText = Read-RequiredText -RelativePath $packManagedPath
$packRuntimeText = Read-RequiredText -RelativePath $packRuntimePath
$stageRuntimeText = Read-RequiredText -RelativePath $stageRuntimePath
$runtimeReleasePreflightText = Read-RequiredText -RelativePath $runtimeReleasePreflightPath
$runtimeReleasePreflightGuardText = Read-RequiredText -RelativePath $runtimeReleasePreflightGuardPath
$releaseCandidateProvenanceText = Read-RequiredText -RelativePath $releaseCandidateProvenancePath
$releaseReadinessContractText = Read-RequiredText -RelativePath $releaseReadinessContractPath
$releasePackageSbomGeneratorText = Read-RequiredText -RelativePath $releasePackageSbomGeneratorPath
$releasePackageSbomGuardText = Read-RequiredText -RelativePath $releasePackageSbomGuardPath
$releaseSigningBoundaryText = Read-RequiredText -RelativePath $releaseSigningBoundaryPath
$nugetRepositorySigningBoundaryText = Read-RequiredText -RelativePath $nugetRepositorySigningBoundaryPath
$nugetRepositorySignedPackageText = Read-RequiredText -RelativePath $nugetRepositorySignedPackagePath
$nugetPublicationBundleText = Read-RequiredText -RelativePath $nugetPublicationBundlePath
$nugetPublicationManifestText = Read-RequiredText -RelativePath $nugetPublicationManifestPath
$nugetRepositoryVerifierProjectText = Read-RequiredText -RelativePath $nugetRepositoryVerifierProjectPath
$nugetRepositoryVerifierProgramText = Read-RequiredText -RelativePath $nugetRepositoryVerifierProgramPath
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
$highGuiMapGuardText = Read-RequiredText -RelativePath $highGuiMapGuardPath
$highGuiMapGeneratorText = Read-RequiredText -RelativePath $highGuiMapGeneratorPath
$highGuiMapToolText = Read-RequiredText -RelativePath $highGuiMapToolPath
$highGuiMapText = Read-RequiredText -RelativePath $highGuiMapPath
$highGuiMapSummaryText = Read-RequiredText -RelativePath $highGuiMapSummaryPath
$highGuiFamilyText = Read-RequiredText -RelativePath $highGuiFamilyPath
$finalCloseoutText = Read-RequiredText -RelativePath $finalCloseoutPath
$finalCloseoutRecordText = Read-RequiredText -RelativePath $finalCloseoutRecordPath
$runtimeProjectText = Read-RequiredText -RelativePath $runtimeProjectPath
$androidRuntimeTargetsText = Read-RequiredText -RelativePath $androidRuntimeTargetsPath
$androidRuntimeProducerText = Read-RequiredText -RelativePath $androidRuntimeProducerPath
$androidSmokeProjectText = Read-RequiredText -RelativePath $androidSmokeProjectPath
$androidSmokeActivityText = Read-RequiredText -RelativePath $androidSmokeActivityPath
$runtimeReadmeText = Read-RequiredText -RelativePath $runtimeReadmePath
$readmeText = Read-RequiredText -RelativePath $readmePath
$chineseReadmeText = Read-RequiredText -RelativePath $chineseReadmePath
$runtimeSupportContract = Read-RequiredText -RelativePath $runtimeSupportContractPath | ConvertFrom-Json
$linkedRuntimeGuideText = Read-RequiredText -RelativePath $linkedRuntimeGuidePath
$apiAbiPolicyText = Read-RequiredText -RelativePath $apiAbiPolicyPath
$supportLifecyclePolicyText = Read-RequiredText -RelativePath $supportLifecyclePolicyPath
$releaseCloseoutDocText = Read-RequiredText -RelativePath $releaseCloseoutDocPath
$releaseNotesText = Read-RequiredText -RelativePath $releaseNotesPath
$nugetRepositorySigningGuideText = Read-RequiredText -RelativePath $nugetRepositorySigningGuidePath
$runtimeLicensesText = Read-RequiredText -RelativePath $runtimeLicensesPath
$consoleSampleProgramText = Read-RequiredText -RelativePath $consoleSampleProgramPath
$tutorialRunnerText = Read-RequiredText -RelativePath $tutorialRunnerPath
$fontFaceText = Read-RequiredText -RelativePath $fontFacePath
$putTextApiText = Read-RequiredText -RelativePath $putTextApiPath
$nativeImgProcText = Read-RequiredText -RelativePath $nativeImgProcPath
$tutorialSeriesText = Read-RequiredText -RelativePath $tutorialSeriesPath
$chinesePutTextTutorialText = Read-RequiredText -RelativePath $chinesePutTextTutorialPath
$androidTutorialText = Read-RequiredText -RelativePath $androidTutorialPath
$docsTocText = Read-RequiredText -RelativePath $docsTocPath
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
Assert-Contains -Violations $violations -Path $publishNugetWorkflowPath -Text $publishNugetWorkflowText -Needle 'environment: nuget-production' -Issue "NuGet.org publication must require the protected production Environment"
Assert-Contains -Violations $violations -Path $publishNugetWorkflowPath -Text $publishNugetWorkflowText -Needle 'scripts/New-NuGetPublicationBundle.ps1' -Issue "NuGet.org publication must generate and recheck the exact reviewed bundle"
Assert-Contains -Violations $violations -Path $publishNugetWorkflowPath -Text $publishNugetWorkflowText -Needle 'scripts/Test-NuGetRepositorySignedPackage.ps1' -Issue "NuGet.org publication must verify public Repository signatures and payload equality"
Assert-Contains -Violations $violations -Path $publishNugetWorkflowPath -Text $publishNugetWorkflowText -Needle 'publish_authorization' -Issue "NuGet.org publication must require the exact dry-run authorization token"
Assert-Contains -Violations $violations -Path $packManagedPath -Text $packManagedText -Needle 'ValidateStableApiCompatibility' -Issue "Stable managed packaging must expose the package API compatibility gate"
Assert-Contains -Violations $violations -Path $packManagedPath -Text $packManagedText -Needle 'PackageValidationBaselineVersion=5.0.0-preview.1' -Issue "Stable managed packaging must compare against the published preview.1 API baseline"
Assert-Contains -Violations $violations -Path $packWorkflowPath -Text $packWorkflowText -Needle '-ValidateStableApiCompatibility' -Issue "The formal stable pack workflow must enable managed package API compatibility validation"
Assert-Contains -Violations $violations -Path $publishNugetWorkflowPath -Text $publishNugetWorkflowText -Needle 'single_maintainer_exception' -Issue "Stable publication must expose the explicit single-maintainer exception input"
Assert-Contains -Violations $violations -Path $publishNugetWorkflowPath -Text $publishNugetWorkflowText -Needle 'single-maintainer-stable-5.0.0-exception' -Issue "Stable publication must record its version-bounded single-maintainer approval mode"
Assert-Contains -Violations $violations -Path $publishNugetWorkflowPath -Text $publishNugetWorkflowText -Needle 'explicit-owner-authorized-stable-5.0.0-no-independent-reviewer' -Issue "Stable publication must record explicit owner risk acceptance"
Assert-Contains -Violations $violations -Path $publishNugetWorkflowPath -Text $publishNugetWorkflowText -Needle 'The single-maintainer exception is restricted to the exact stable 5.0.0 release.' -Issue "Single-maintainer exception must remain bounded to exact stable 5.0.0"
Assert-Contains -Violations $violations -Path $publishNugetWorkflowPath -Text $publishNugetWorkflowText -Needle 'The single-maintainer exception must be dispatched by guojin-yan.' -Issue "Single-maintainer exception must be owner-dispatched"
Assert-Contains -Violations $violations -Path $publishNugetWorkflowPath -Text $publishNugetWorkflowText -Needle 'The single-maintainer exception requires designated_publisher=Guojin Yan.' -Issue "Single-maintainer exception must preserve the declared publisher identity"
Assert-Contains -Violations $violations -Path $publishNugetWorkflowPath -Text $publishNugetWorkflowText -Needle 'The single-maintainer exception requires independent_approver=not-available.' -Issue "Single-maintainer exception must not invent an independent approver"
Assert-Contains -Violations $violations -Path $publishNugetWorkflowPath -Text $publishNugetWorkflowText -Needle 'publication_manifest_json' -Issue "NuGet.org publication must bind the complete real-supported package manifest"
Assert-Contains -Violations $violations -Path $publishNugetWorkflowPath -Text $publishNugetWorkflowText -Needle 'gh run download' -Issue "NuGet.org publication must download exact manifest-bound authoritative pack artifacts"
Assert-Contains -Violations $violations -Path $nugetRepositorySigningBoundaryPath -Text $nugetRepositorySigningBoundaryText -Needle 'NUGET_REPOSITORY_SIGNING_BOUNDARY_OK' -Issue "Release artifact surface must register repository-signing negative fixtures"
Assert-Contains -Violations $violations -Path $nugetRepositorySignedPackagePath -Text $nugetRepositorySignedPackageText -Needle 'dotnet nuget verify' -Issue "Repository-signed package verifier must invoke NuGet cryptographic verification"
Assert-Contains -Violations $violations -Path $nugetPublicationBundlePath -Text $nugetPublicationBundleText -Needle 'publish-nuget:sha256:' -Issue "Publication bundle must emit a candidate-specific authorization token"
Assert-Contains -Violations $violations -Path $nugetPublicationBundlePath -Text $nugetPublicationBundleText -Needle 'PublicationManifestPath' -Issue "Publication bundle must bind the normalized package manifest"
Assert-Contains -Violations $violations -Path $nugetPublicationBundlePath -Text $nugetPublicationBundleText -Needle 'Where-Object { [string]$_.PackageId -ceq [string]$package.Id }' -Issue "Publication bundle must compare change-control package identities without relying on culture-sensitive sorting"
Assert-Contains -Violations $violations -Path $nugetPublicationBundlePath -Text $nugetPublicationBundleText -Needle 'Change-control package hash mismatch' -Issue "Publication bundle must compare each exact package hash with actionable diagnostics"
Assert-Contains -Violations $violations -Path $nugetPublicationManifestPath -Text $nugetPublicationManifestText -Needle '$realTargets.Count -ne 28' -Issue "Publication manifest must require all 28 currently real-supported runtime targets"
Assert-Contains -Violations $violations -Path $nugetPublicationManifestPath -Text $nugetPublicationManifestText -Needle 'Publication manifest must contain exactly' -Issue "Publication manifest must reject incomplete package closure"
Assert-Contains -Violations $violations -Path $nugetRepositoryVerifierProjectPath -Text $nugetRepositoryVerifierProjectText -Needle '<PackageReference Include="NuGet.Packaging" Version="7.6.0" />' -Issue "Structured repository-signature verifier must pin the audited NuGet.Packaging version"
Assert-Contains -Violations $violations -Path $nugetRepositoryVerifierProgramPath -Text $nugetRepositoryVerifierProgramText -Needle 'RepositoryPrimarySignature' -Issue "Structured verifier must require a repository primary signature"
Assert-Contains -Violations $violations -Path $packManagedPath -Text $packManagedText -Needle '[string]$ProjectPath = "src\OpenCvSharp\OpenCvSharp.csproj"' -Issue "Pack-Managed default project path must be the neutral managed project"
Assert-Contains -Violations $violations -Path $packManagedPath -Text $packManagedText -Needle "OpenCvCSharpManagedPackageId" -Issue "Pack-Managed must derive the neutral managed package ID from Directory.Build.props"
Assert-Contains -Violations $violations -Path $packManagedPath -Text $packManagedText -Needle "`$managedPackageId = Get-RequiredDirectoryBuildProperty" -Issue "Pack-Managed must assign the neutral managed package ID from the central metadata property"
Assert-Contains -Violations $violations -Path $packManagedPath -Text $packManagedText -Needle '$packagePath = Join-Path $outputFullPath "$managedPackageId.$($packageVersionRecord.NuGetVersion).nupkg"' -Issue "Pack-Managed package artifact file must be derived from neutral package ID plus normalized version"
Assert-Contains -Violations $violations -Path $packManagedPath -Text $packManagedText -Needle "Remove-Item -LiteralPath `$candidatePath -Force" -Issue "Pack-Managed must remove stale SDK and canonical package artifacts before packing"
Assert-Contains -Violations $violations -Path $packManagedPath -Text $packManagedText -Needle "Managed package artifact was not found" -Issue "Pack-Managed must verify the expected package artifact after packing"
Assert-Contains -Violations $violations -Path $packManagedPath -Text $packManagedText -Needle "PackageVersion carries OpenCV runtime identity as version metadata" -Issue "Pack-Managed must document PackageVersion as metadata, not package identity"

Assert-Contains -Violations $violations -Path $packRuntimePath -Text $packRuntimeText -Needle '[string]$OutputDir = "artifacts/packages"' -Issue "Pack-Runtime default output directory must be artifacts/packages"
Assert-Contains -Violations $violations -Path $apiAbiBaselinePath -Text $apiAbiBaselineText -Needle 'managed-public-api.txt' -Issue "Release artifact surface must register the managed API baseline"
Assert-Contains -Violations $violations -Path $apiAbiBaselinePath -Text $apiAbiBaselineText -Needle 'native_abi_mini_manifest.txt' -Issue "Release artifact surface must register the mini ABI baseline"
Assert-Contains -Violations $violations -Path $bindingMapGuardPath -Text $bindingMapGuardText -Needle 'Generate-NativeManagedBindingMap.ps1' -Issue "Release artifact surface must freshness-check the native-to-managed binding map"
Assert-Contains -Violations $violations -Path $bindingMapGuardPath -Text $bindingMapGuardText -Needle 'native=2663 bound=2663 unbound=0 managed_only=0' -Issue "Native-to-managed binding guard must emit complete parity evidence"
Assert-Contains -Violations $violations -Path $bindingMapGeneratorPath -Text $bindingMapGeneratorText -Needle 'tools/NativeManagedBindingMap/NativeManagedBindingMap.csproj' -Issue "Binding-map generator must invoke its checked-in tool project"
Assert-Contains -Violations $violations -Path $bindingMapToolProjectPath -Text $bindingMapToolProjectText -Needle '<TargetFramework>net10.0</TargetFramework>' -Issue "Binding-map tool must retain its exact modern target framework"
Assert-Contains -Violations $violations -Path $bindingMapToolProgramPath -Text $bindingMapToolProgramText -Needle 'NATIVE_MANAGED_BINDING_MAP_OK' -Issue "Binding-map tool must emit deterministic parity evidence"
Assert-Contains -Violations $violations -Path $bindingMapPath -Text $bindingMapText -Needle '[managed-only]' -Issue "Release artifact surface must include the complete native-to-managed mapping output"
Assert-Contains -Violations $violations -Path $bindingMapSummaryPath -Text $bindingMapSummaryText -Needle '"managedBoundCount": 2663' -Issue "Binding-map summary must record all native functions as managed-bound"
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
Assert-Contains -Violations $violations -Path $highGuiMapGuardPath -Text $highGuiMapGuardText -Needle 'Generate-HighGuiUpstreamMap.ps1' -Issue "Release artifact surface must freshness-check the HighGui upstream map"
Assert-Contains -Violations $violations -Path $highGuiMapGeneratorPath -Text $highGuiMapGeneratorText -Needle '-RegenerateRaw requires an explicit -PythonPath.' -Issue "HighGui raw extraction must require an explicit Python path"
Assert-Contains -Violations $violations -Path $highGuiMapToolPath -Text $highGuiMapToolText -Needle 'RepositoryWideUpstreamParityClaimed = false' -Issue "HighGui map tool must retain the exact non-repository-wide claim boundary"
Assert-Contains -Violations $violations -Path $highGuiMapPath -Text $highGuiMapText -Needle 'repository-wide-upstream-parity-claimed=false' -Issue "Release artifact surface must include the bounded HighGui mapping output"
Assert-Contains -Violations $violations -Path $highGuiMapSummaryPath -Text $highGuiMapSummaryText -Needle '"declarationCount": 33' -Issue "HighGui map summary must record the exact declaration count"
Assert-Contains -Violations $violations -Path $highGuiMapSummaryPath -Text $highGuiMapSummaryText -Needle '"implemented": 20' -Issue "HighGui map summary must record all locally supported callables"
Assert-Contains -Violations $violations -Path $highGuiFamilyPath -Text $highGuiFamilyText -Needle '"nativeEntrypointAdditionCount": 10' -Issue "HighGui family inventory must record the reviewed native ABI addition count"
Assert-Contains -Violations $violations -Path $finalCloseoutPath -Text $finalCloseoutText -Needle 'local-release-candidate-closeout.json' -Issue "Release artifact surface must register the final closeout record"
Assert-Contains -Violations $violations -Path $finalCloseoutRecordPath -Text $finalCloseoutRecordText -Needle 'local-release-candidate-closeout' -Issue "Final closeout record must identify its record kind"
Assert-Contains -Violations $violations -Path $apiAbiPolicyPath -Text $apiAbiPolicyText -Needle 'compatibility/api-gap-inventory.json' -Issue "API/ABI policy must expose the gap inventory"
Assert-Contains -Violations $violations -Path $supportLifecyclePolicyPath -Text $supportLifecyclePolicyText -Needle '| `real-supported` | 28 |' -Issue "Support lifecycle policy must expose the real-support count"
Assert-Contains -Violations $violations -Path $releaseCloseoutDocPath -Text $releaseCloseoutDocText -Needle 'locally-validated' -Issue "Release closeout documentation must expose local validation state"
Assert-Contains -Violations $violations -Path $releaseCloseoutDocPath -Text $releaseCloseoutDocText -Needle 'New-ReleasePackageSbom.ps1' -Issue "Release closeout documentation must register deterministic SPDX generation"
Assert-Contains -Violations $violations -Path $releaseCloseoutDocPath -Text $releaseCloseoutDocText -Needle 'repository-signing-pending' -Issue "Release closeout documentation must expose the confirmed NuGet.org signing strategy"
Assert-Contains -Violations $violations -Path $releaseCloseoutDocPath -Text $releaseCloseoutDocText -Needle 'Test-NuGetRepositorySignedPackage.ps1' -Issue "Release closeout documentation must register post-publication signature verification"
Assert-Contains -Violations $violations -Path $releaseNotesPath -Text $releaseNotesText -Needle '5.0.0' -Issue "Stable release notes must identify the exact normalized public candidate version"
Assert-Contains -Violations $violations -Path $releaseNotesPath -Text $releaseNotesText -Needle 'dotnet remove package JYPPX.OpenCV.CSharp.API' -Issue "Stable release notes must provide managed package uninstall guidance"
Assert-Contains -Violations $violations -Path $releaseNotesPath -Text $releaseNotesText -Needle 'Known Limitations' -Issue "Stable release notes must expose known limitations"
Assert-Contains -Violations $violations -Path $releaseNotesPath -Text $releaseNotesText -Needle '5.0.0-preview.1' -Issue "Stable rollback guidance must name the previous public package explicitly"
Assert-Contains -Violations $violations -Path $releaseNotesPath -Text $releaseNotesText -Needle 'Do not reference full and mini runtime packages together' -Issue "Stable release notes must prevent ambiguous full/mini runtime selection"
Assert-Contains -Violations $violations -Path $publishNugetWorkflowPath -Text $publishNugetWorkflowText -Needle '--latest' -Issue "Stable GitHub Release creation must mark 5.0.0 as latest"
Assert-Contains -Violations $violations -Path $publishNugetWorkflowPath -Text $publishNugetWorkflowText -Needle '--notes-file ./docs/articles/release-notes.md' -Issue "Stable GitHub Release must use the stable release notes"
Assert-Contains -Violations $violations -Path $nugetRepositorySigningGuidePath -Text $nugetRepositorySigningGuideText -Needle 'RepositoryPrimarySignature' -Issue "Repository-signing guide must explain the structured NuGet signature type"
Assert-Contains -Violations $violations -Path $nugetRepositorySigningGuidePath -Text $nugetRepositorySigningGuideText -Needle 'publish-nuget:sha256:' -Issue "Repository-signing guide must explain the dry-run authorization token"
Assert-Contains -Violations $violations -Path $nugetRepositorySigningGuidePath -Text $nugetRepositorySigningGuideText -Needle 'dotnet nuget verify --all' -Issue "Repository-signing guide must expose the consumer verification command"

foreach ($needle in @('"tutorial"', '"showcase"', 'ShowcaseRunner.Run')) {
    Assert-Contains -Violations $violations -Path $consoleSampleProgramPath -Text $consoleSampleProgramText -Needle $needle -Issue "ConsoleSamples must retain the tutorial entrypoint and showcase compatibility alias"
}
foreach ($needle in @(
        'command != "text"',
        'command != "contours"',
        'new FontFace(fontPath)',
        'ImgProcCv2.PutText(panel, headline',
        'ImgProcCv2.GetTextSize(panel.Size, headline',
        'OPENCV_CSHARP_CJK_FONT',
        'tutorial [all|image|text|contours|features|template|ml]')) {
    Assert-Contains -Violations $violations -Path $tutorialRunnerPath -Text $tutorialRunnerText -Needle $needle -Issue "Tutorial runner must retain the six-case series and OpenCV Chinese putText path"
}
Assert-Matches -Violations $violations -Path $tutorialRunnerPath -Text $tutorialRunnerText -Pattern 'const string headline = "OpenCV \u4e2d\u6587\u5199\u5b57"' -Issue "Tutorial runner must retain a real Chinese UTF-8 rendering input"
Assert-Contains -Violations $violations -Path $fontFacePath -Text $fontFaceText -Needle 'including Chinese when the selected font contains the required glyphs' -Issue "FontFace API documentation must state the Chinese glyph requirement"
Assert-Contains -Violations $violations -Path $putTextApiPath -Text $putTextApiText -Needle 'Renders UTF-8 Unicode text with OpenCV <c>putText</c>' -Issue "Managed PutText API documentation must identify the UTF-8 OpenCV path"
foreach ($needle in @('jyppx_ocv_imgproc_put_text_font_face', 'const cv::Point next = cv::putText(')) {
    Assert-Contains -Violations $violations -Path $nativeImgProcPath -Text $nativeImgProcText -Needle $needle -Issue "Native ImgProc implementation must route FontFace text through OpenCV cv::putText"
}
foreach ($needle in @(
        'tutorial-01-image-pipeline.md',
        'tutorial-02-chinese-puttext.md',
        'tutorial-03-contours.md',
        'tutorial-04-orb-features.md',
        'tutorial-05-template-matching.md',
        'tutorial-06-knn-classification.md',
        'tutorial-07-android-runtime.md')) {
    Assert-Contains -Violations $violations -Path $tutorialSeriesPath -Text $tutorialSeriesText -Needle $needle -Issue "Tutorial overview must link every numbered technical article"
    Assert-Contains -Violations $violations -Path $docsTocPath -Text $docsTocText -Needle $needle -Issue "DocFX navigation must expose every numbered technical article"
}
foreach ($needle in @('OpenCV 5 adds a `FontFace` overload of `putText`', 'does not use GDI, Skia', 'ImgProcCv2.PutText(', 'OPENCV_CSHARP_CJK_FONT')) {
    Assert-Contains -Violations $violations -Path $chinesePutTextTutorialPath -Text $chinesePutTextTutorialText -Needle $needle -Issue "Chinese tutorial must document and demonstrate the direct OpenCV putText path"
}
foreach ($needle in @('dotnet add package JYPPX.OpenCV.runtime.android-x64.mini', 'Cv2.Sum(image)', 'PASS version=5.0.0 sum=448', 'Android ARM/ARM64 remain `android-evidence-pending`')) {
    Assert-Contains -Violations $violations -Path $androidTutorialPath -Text $androidTutorialText -Needle $needle -Issue "Android tutorial must retain version-neutral package installation, native loading, and truthful support boundaries"
}
foreach ($readme in @(
        [pscustomobject]@{ Path = $readmePath; Text = $readmeText },
        [pscustomobject]@{ Path = $chineseReadmePath; Text = $chineseReadmeText })) {
    foreach ($needle in @('docs/articles/tutorial-series.md', 'docs/articles/tutorial-02-chinese-puttext.md', '-- tutorial all')) {
        Assert-Contains -Violations $violations -Path $readme.Path -Text $readme.Text -Needle $needle -Issue "$($readme.Path) must expose the tutorial series and Chinese putText example"
    }
}

$requiredTutorialImages = @(
    "source.png",
    "image-pipeline.png",
    "chinese-text.png",
    "contours.png",
    "orb-features.png",
    "template-match.png",
    "knn-classification.png",
    "showcase-overview.png"
)
foreach ($imageName in $requiredTutorialImages) {
    $relativeImagePath = "docs/images/showcase/$imageName"
    $imagePath = Join-Path $repo $relativeImagePath
    if (-not (Test-Path -LiteralPath $imagePath -PathType Leaf)) {
        Add-Violation -Violations $violations -Path $relativeImagePath -Issue "Tutorial output image is missing"
        continue
    }
    if ((Get-Item -LiteralPath $imagePath).Length -lt 1024) {
        Add-Violation -Violations $violations -Path $relativeImagePath -Issue "Tutorial output image is unexpectedly small"
    }
}

$runtimeTargets = @($runtimeSupportContract.realSupport)
if ($runtimeTargets.Count -ne 28) {
    Add-Violation -Violations $violations -Path $runtimeSupportContractPath -Issue "README package table guard requires exactly 28 currently real-supported runtime targets" -Text "actual=$($runtimeTargets.Count)"
}
foreach ($readme in @(
        [pscustomobject]@{ Path = $readmePath; Text = $readmeText },
        [pscustomobject]@{ Path = $chineseReadmePath; Text = $chineseReadmeText })) {
    foreach ($needle in @(
            'https://img.shields.io/nuget/v/JYPPX.OpenCV.CSharp.API.svg?label=version',
            'https://www.nuget.org/packages/JYPPX.OpenCV.CSharp.API/',
            'https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.CSharp.API',
            'https://github.com/guojin-yan/OpenCV-CSharp-API/releases')) {
        Assert-Contains -Violations $violations -Path $readme.Path -Text $readme.Text -Needle $needle -Issue "$($readme.Path) must expose the live managed package version badge and formal public release surfaces"
    }
    foreach ($needle in @(
            'dotnet add package JYPPX.OpenCV.CSharp.API',
            'dotnet add package JYPPX.OpenCV.runtime.win-x64')) {
        Assert-Contains -Violations $violations -Path $readme.Path -Text $readme.Text -Needle $needle -Issue "$($readme.Path) install examples must follow the stable channel without a fixed version"
    }
    if ($readme.Text -match '(?im)^\s*dotnet\s+add\s+package\b[^\r\n]*\s--version\s') {
        Add-Violation -Violations $violations -Path $readme.Path -Issue "README install examples must not pin a version"
    }
    if ($readme.Text.IndexOf('5.0.0-preview.1', [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
        Add-Violation -Violations $violations -Path $readme.Path -Issue "README package versions must come from live NuGet badges instead of fixed text"
    }
    foreach ($target in $runtimeTargets) {
        $parts = ([string]$target).Split('/')
        $packageId = "$runtimePackagePrefix.$($parts[0])$(if ($parts[1] -eq 'mini') { '.mini' } else { '' })"
        foreach ($needle in @(
                "https://img.shields.io/nuget/v/$packageId.svg?label=version",
                "https://www.nuget.org/packages/$packageId/",
                "https://github.com/users/guojin-yan/packages/nuget/package/$packageId")) {
            Assert-Contains -Violations $violations -Path $readme.Path -Text $readme.Text -Needle $needle -Issue "$($readme.Path) must list every real-supported runtime package, live NuGet version badge, and both registry links"
        }
    }
}

Assert-Contains -Violations $violations -Path $linkedRuntimeGuidePath -Text $linkedRuntimeGuideText -Needle 'Test-ReleasePackageSbom.ps1' -Issue "Linked runtime documentation must register the release package SBOM guard"
Assert-Contains -Violations $violations -Path $packRuntimePath -Text $packRuntimeText -Needle '[string]$RuntimeProject = "packaging/runtime/JYPPX.OpenCV.runtime/JYPPX.OpenCV.runtime.csproj"' -Issue "Pack-Runtime default project path must be the neutral runtime package project"
Assert-Contains -Violations $violations -Path $packRuntimePath -Text $packRuntimeText -Needle '$runtimePackageId = "$runtimePackagePrefix.$Rid$runtimePackageSuffix"' -Issue "Pack-Runtime package ID must be derived from neutral runtime package prefix, RID, and profile suffix"
Assert-Contains -Violations $violations -Path $packRuntimePath -Text $packRuntimeText -Needle '$packagePath = Join-Path $outputFullPath "$runtimePackageId.$($packageVersionRecord.NuGetVersion).nupkg"' -Issue "Pack-Runtime package artifact file must be derived from neutral package ID plus normalized version"
Assert-Contains -Violations $violations -Path $packRuntimePath -Text $packRuntimeText -Needle '"-p:PackageId=$runtimePackageId"' -Issue "Pack-Runtime must pass the derived neutral package ID to dotnet pack"
Assert-Contains -Violations $violations -Path $packRuntimePath -Text $packRuntimeText -Needle "Remove-Item -LiteralPath `$candidatePath -Force" -Issue "Pack-Runtime must remove stale SDK and canonical package artifacts before packing"
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
Assert-Contains -Violations $violations -Path $stageRuntimePath -Text $stageRuntimeText -Needle "Runtime staging directory:" -Issue "Stage-Runtime must print staging directory evidence"
Assert-Contains -Violations $violations -Path $stageRuntimePath -Text $stageRuntimeText -Needle "Runtime package project directory:" -Issue "Stage-Runtime must print runtime package mirror evidence"
Assert-Contains -Violations $violations -Path $stageRuntimePath -Text $stageRuntimeText -Needle 'JYPPX.OpenCV.runtime.provenance.json' -Issue "Stage-Runtime must generate a durable runtime provenance manifest"
Assert-Contains -Violations $violations -Path $stageRuntimePath -Text $stageRuntimeText -Needle 'SyntheticRuntimeInputs = [bool]$SyntheticRuntimeInputs.IsPresent' -Issue "Stage-Runtime provenance manifest must distinguish synthetic validation inputs"
Assert-Contains -Violations $violations -Path $stageRuntimePath -Text $stageRuntimeText -Needle 'RequiredModules = @($OpenCvModules)' -Issue "Stage-Runtime provenance manifest must record required OpenCV modules"
Assert-Contains -Violations $violations -Path $stageRuntimePath -Text $stageRuntimeText -Needle 'Runtime provenance manifest:' -Issue "Stage-Runtime must print runtime provenance manifest evidence"

Assert-Contains -Violations $violations -Path $runtimeProjectPath -Text $runtimeProjectText -Needle 'PackagePath="buildTransitive/$(PackageId).targets"' -Issue "Runtime package must rename the Android buildTransitive target to the exact dynamic package ID"
Assert-Contains -Violations $violations -Path $runtimeProjectPath -Text $runtimeProjectText -Needle "'`$(RuntimePackageRid)' == 'android-arm64'" -Issue "Runtime package must include buildTransitive integration only for declared Android RIDs"
foreach ($androidAbi in @(
        [pscustomobject]@{ Rid = 'android-arm64'; Abi = 'arm64-v8a' },
        [pscustomobject]@{ Rid = 'android-arm'; Abi = 'armeabi-v7a' },
        [pscustomobject]@{ Rid = 'android-x64'; Abi = 'x86_64' },
        [pscustomobject]@{ Rid = 'android-x86'; Abi = 'x86' })) {
    Assert-Contains -Violations $violations -Path $androidRuntimeTargetsPath -Text $androidRuntimeTargetsText -Needle "runtimes/$($androidAbi.Rid)/native/*.so" -Issue "Android runtime target must map $($androidAbi.Rid) native payload" -NormalizeSlashes
    Assert-Contains -Violations $violations -Path $androidRuntimeTargetsPath -Text $androidRuntimeTargetsText -Needle "<Abi>$($androidAbi.Abi)</Abi>" -Issue "Android runtime target must declare ABI metadata for $($androidAbi.Rid)"
}
Assert-Contains -Violations $violations -Path $androidRuntimeTargetsPath -Text $androidRuntimeTargetsText -Needle 'AndroidNativeLibrary' -Issue "Android runtime target must use .NET for Android AndroidNativeLibrary items"
Assert-Contains -Violations $violations -Path $androidRuntimeTargetsPath -Text $androidRuntimeTargetsText -Needle "'`$(TargetPlatformIdentifier)' == 'android' Or '`$(TargetFrameworkIdentifier)' == 'MonoAndroid'" -Issue "Android runtime target must activate for modern and legacy .NET for Android TFMs while remaining inert for desktop consumers"
foreach ($androidProducerNeedle in @(
        '28.2.13676358',
        'ANDROID_SUPPORT_FLEXIBLE_PAGE_SIZES=ON',
        'libopencv_$_.so',
        '^libopencv_.+\.so\..+$',
        'Get-ExpectedElfIdentity',
        'Android ELF e_machine audit failed',
        '@(& $ReadElf -h $Path 2>&1)',
        '@(& $ReadElf -lW $Path 2>&1)',
        '@(& $readElf -dW $file 2>&1)',
        'Android ELF must retain at least 16 KB LOAD segment alignment',
        'libc++_shared.so',
        'New-RuntimeInputArtifact.ps1',
        'ANDROID_RUNTIME_INPUT_OK')) {
    Assert-Contains -Violations $violations -Path $androidRuntimeProducerPath -Text $androidRuntimeProducerText -Needle $androidProducerNeedle -Issue "Android runtime producer must retain '$androidProducerNeedle'"
}
Assert-Contains -Violations $violations -Path $androidSmokeProjectPath -Text $androidSmokeProjectText -Needle '<TargetFramework>net10.0-android</TargetFramework>' -Issue "Android smoke project must target .NET 10 for Android"
Assert-Contains -Violations $violations -Path $androidSmokeProjectPath -Text $androidSmokeProjectText -Needle '<PackageReference Include="$(AndroidRuntimePackageId)"' -Issue "Android smoke project must support consuming the selected runtime package"
Assert-Contains -Violations $violations -Path $androidSmokeActivityPath -Text $androidSmokeActivityText -Needle 'using JYPPX.OpenCvSharp.Core;' -Issue "Android smoke activity must import the core API namespace"
Assert-Contains -Violations $violations -Path $androidSmokeActivityPath -Text $androidSmokeActivityText -Needle 'OpenCvSharpBuildInfo.GetNativeOpenCvVersion()' -Issue "Android smoke activity must load and report the native OpenCV runtime"
Assert-Contains -Violations $violations -Path $androidSmokeActivityPath -Text $androidSmokeActivityText -Needle 'Cv2.Sum(image)' -Issue "Android smoke activity must execute a real native OpenCV operation"
foreach ($workflowNeedle in @(
        'produce-android:',
        'scripts/Build-AndroidRuntimeInput.ps1',
        'dotnet workload install android --skip-manifest-update',
        'artifacts/android-proof/consumer-packages',
        '-p:RestorePackagesPath=$consumerPackages',
        'Restored Android managed assembly did not match the same-run package payload.',
        'system-images;android-35;default;x86_64',
        'system-images;android-29;default;x86',
        "matrix.rid == 'android-x64' || matrix.rid == 'android-x86'",
        'timeout 20m bash -c',
        'export ANDROID_AVD_HOME="$RUNNER_TEMP/android-avd"',
        'opencv-csharp-smoke.ini',
        'timeout 3m "$adb" wait-for-device',
        'emulator-logcat.txt',
        'ANDROID_PACKAGE_CONSUMER_OK',
        'ANDROID_EMULATOR_LOADING_OK')) {
    Assert-Contains -Violations $violations -Path $runtimeInputWorkflowPath -Text $runtimeInputWorkflowText -Needle $workflowNeedle -Issue "Android runtime workflow must retain '$workflowNeedle'"
}

Assert-Contains -Violations $violations -Path $runtimeReleasePreflightPath -Text $runtimeReleasePreflightText -Needle 'Release candidate preflight rejects synthetic runtime inputs' -Issue "Runtime release preflight must reject synthetic runtime inputs by default"
Assert-Contains -Violations $violations -Path $runtimeReleasePreflightPath -Text $runtimeReleasePreflightText -Needle 'contain no stale files' -Issue "Runtime release preflight must reject stale native/license/build mirrors"
Assert-Contains -Violations $violations -Path $runtimeReleasePreflightPath -Text $runtimeReleasePreflightText -Needle 'Runtime provenance required modules must match selected profile' -Issue "Runtime release preflight must validate profile module provenance"
Assert-Contains -Violations $violations -Path $runtimeReleasePreflightGuardPath -Text $runtimeReleasePreflightGuardText -Needle 'Pack-Runtime -RequireReleasePreflight integration should pass for release-shaped staged inputs' -Issue "Runtime release preflight guard must exercise the actual Pack-Runtime -RequireReleasePreflight path"
Assert-Contains -Violations $violations -Path $runtimeReleasePreflightGuardPath -Text $runtimeReleasePreflightGuardText -Needle 'Synthetic release-preflight negative path must not produce a runtime package' -Issue "Runtime release preflight guard must prove synthetic preflight integration does not produce packages"
Assert-Contains -Violations $violations -Path $runtimeReleasePreflightGuardPath -Text $runtimeReleasePreflightGuardText -Needle 'Pack-Runtime -RequireReleasePreflight produces a package only for non-synthetic staged inputs.' -Issue "Runtime release preflight guard must cover positive and negative pack integration cases"
Assert-Contains -Violations $violations -Path $releaseCandidateProvenancePath -Text $releaseCandidateProvenanceText -Needle 'Deterministic package manifest' -Issue "Release candidate provenance guard must produce deterministic package manifest evidence"
Assert-Contains -Violations $violations -Path $releaseCandidateProvenancePath -Text $releaseCandidateProvenanceText -Needle "SignatureStatus = 'repository-signing-pending'" -Issue "Release candidate provenance guard must keep NuGet.org repository-signing readiness explicit"
Assert-Contains -Violations $violations -Path $releaseCandidateProvenancePath -Text $releaseCandidateProvenanceText -Needle "SbomStatus = 'not-ready'" -Issue "Release candidate provenance guard must keep local SBOM readiness explicit"
Assert-Contains -Violations $violations -Path $releaseCandidateProvenancePath -Text $releaseCandidateProvenanceText -Needle 'PublicationAllowed = $false' -Issue "Release candidate provenance guard must remain non-publishing"
Assert-Contains -Violations $violations -Path $releaseCandidateProvenancePath -Text $releaseCandidateProvenanceText -Needle "local-preflight-only; no remote mutation" -Issue "Release candidate provenance guard must record rollback abort metadata"
Assert-Contains -Violations $violations -Path $releaseReadinessContractPath -Text $releaseReadinessContractText -Needle 'repository-signature/SBOM state machine' -Issue "Release readiness contract must validate repository signing and SBOM state transitions"
Assert-Contains -Violations $violations -Path $releaseReadinessContractPath -Text $releaseReadinessContractText -Needle 'Private key material must never be present' -Issue "Release readiness contract must reject private key material"
Assert-Contains -Violations $violations -Path $releaseReadinessContractPath -Text $releaseReadinessContractText -Needle 'pending-hosted-evidence' -Issue "Release readiness contract must keep hosted Windows x86 promotion pending"
Assert-Contains -Violations $violations -Path $releaseReadinessContractPath -Text $releaseReadinessContractText -Needle 'read-only-fixture' -Issue "Release readiness contract must keep public feed verification read-only"
Assert-Contains -Violations $violations -Path $releaseSigningBoundaryPath -Text $releaseSigningBoundaryText -Needle 'RELEASE_SIGNING_BOUNDARY_OK' -Issue "Release signing boundary must emit normalized-input evidence"
Assert-Contains -Violations $violations -Path $releaseSigningBoundaryPath -Text $releaseSigningBoundaryText -Needle 'PostSigningPackageSha256' -Issue "Release signing boundary must bind post-signing package bytes"
Assert-Contains -Violations $violations -Path $releaseSigningBoundaryPath -Text $releaseSigningBoundaryText -Needle 'SPDX-2.3' -Issue "Release signing boundary must bind SPDX-2.3 SBOM provenance"
Assert-Contains -Violations $violations -Path $releaseSigningBoundaryPath -Text $releaseSigningBoundaryText -Needle 'RemoteMutationAllowed' -Issue "Release signing boundary must keep approval unable to mutate remote state"
Assert-Contains -Violations $violations -Path $releaseSigningBoundaryPath -Text $releaseSigningBoundaryText -Needle 'dotnet nuget sign' -Issue "Release signing boundary must reject direct workflow signing"

Assert-Contains -Violations $violations -Path $releasePackageSbomGeneratorPath -Text $releasePackageSbomGeneratorText -Needle "[switch]`$Check" -Issue "Release package SBOM generator must expose byte-for-byte check mode"
Assert-Contains -Violations $violations -Path $releasePackageSbomGeneratorPath -Text $releasePackageSbomGeneratorText -Needle "spdxVersion = 'SPDX-2.3'" -Issue "Release package SBOM generator must emit SPDX-2.3"
Assert-Contains -Violations $violations -Path $releasePackageSbomGeneratorPath -Text $releasePackageSbomGeneratorText -Needle 'normalized unsigned package before signing' -Issue "Release package SBOM generator must require normalized unsigned package input"
Assert-Contains -Violations $violations -Path $releasePackageSbomGeneratorPath -Text $releasePackageSbomGeneratorText -Needle '$nuspecRepositoryCommit -ne $SourceCommit' -Issue "Release package SBOM generator must bind the exact source commit"
Assert-Contains -Violations $violations -Path $releasePackageSbomGeneratorPath -Text $releasePackageSbomGeneratorText -Needle 'SyntheticRuntimeInputs' -Issue "Release package SBOM generator must reject synthetic runtime provenance"
Assert-Contains -Violations $violations -Path $releasePackageSbomGeneratorPath -Text $releasePackageSbomGeneratorText -Needle '[Linq.Enumerable]::SequenceEqual($outputBytes, $actualBytes)' -Issue "Release package SBOM check mode must compare exact deterministic bytes"
Assert-Contains -Violations $violations -Path $releasePackageSbomGuardPath -Text $releasePackageSbomGuardText -Needle 'RELEASE_PACKAGE_SBOM_OK format=SPDX-2.3 deterministic=true' -Issue "Release package SBOM guard must emit deterministic SPDX evidence"
Assert-Contains -Violations $violations -Path $releasePackageSbomGuardPath -Text $releasePackageSbomGuardText -Needle 'private_keys=false remote_mutation=false' -Issue "Release package SBOM guard must remain local and private-key free"
$actualSbomNegativeFixtureCount = [regex]::Matches($releasePackageSbomGuardText, '(?m)^\s{4}Assert-Rejected\s+-Name\s+').Count
if ($actualSbomNegativeFixtureCount -ne 17) {
    Add-Violation -Violations $violations -Path $releasePackageSbomGuardPath -Issue "Release package SBOM guard must retain exactly 17 negative fixtures" -Text "actual=$actualSbomNegativeFixtureCount"
}
Assert-Contains -Violations $violations -Path $releaseSupportContractPath -Text $releaseSupportContractText -Needle 'RELEASE_SUPPORT_CONTRACT_OK' -Issue "Release support contract must emit explicit matrix/support classification"
Assert-Contains -Violations $violations -Path $releaseSupportContractPath -Text $releaseSupportContractText -Needle 'packageSurfaceIsSupport' -Issue "Release support contract must separate package surface from real support"
Assert-Contains -Violations $violations -Path $publicFeedVerificationContractPath -Text $publicFeedVerificationContractText -Needle 'NUGET_PUBLIC_FEED_READ_ONLY_OK' -Issue "Public feed contract must emit read-only verification evidence"
Assert-Contains -Violations $violations -Path $publicFeedVerificationContractPath -Text $publicFeedVerificationContractText -Needle 'https_only=true' -Issue "Public feed contract must require HTTPS-only verification"
Assert-Contains -Violations $violations -Path $publicFeedVerificationContractPath -Text $publicFeedVerificationContractText -Needle 'upload_attempted=false' -Issue "Public feed contract must reject upload during verification"
Assert-Contains -Violations $violations -Path $publicFeedVerificationContractPath -Text $publicFeedVerificationContractText -Needle 'api.nuget.org/v3-flatcontainer' -Issue "Public feed contract must use the exact NuGet flat-container path"
Assert-Contains -Violations $violations -Path $publicFeedVerificationContractPath -Text $publicFeedVerificationContractText -Needle "`$packageVersion = '5.0.0'" -Issue "Public feed contract must verify the exact stable candidate version"
Assert-Contains -Violations $violations -Path $releaseCandidateProvenancePath -Text $releaseCandidateProvenanceText -Needle 'SigningHandoff' -Issue "Release provenance must carry the signing handoff contract"
Assert-Contains -Violations $violations -Path $releaseCandidateProvenancePath -Text $releaseCandidateProvenanceText -Needle 'SbomHandoff' -Issue "Release provenance must carry the SBOM handoff contract"
Assert-Contains -Violations $violations -Path $releaseChangeControlPath -Text $releaseChangeControlText -Needle 'RELEASE_CHANGE_CONTROL_OK' -Issue "Release change-control guard must emit deterministic review evidence"
Assert-Contains -Violations $violations -Path $releaseChangeControlPath -Text $releaseChangeControlText -Needle "Decision = 'do-not-publish'" -Issue "Release change-control guard must default to non-publishing"
Assert-Contains -Violations $violations -Path $releaseChangeControlPath -Text $releaseChangeControlText -Needle "Status = 'not-approved'" -Issue "Release change-control guard must require explicit approval"
Assert-Contains -Violations $violations -Path $releaseChangeControlPath -Text $releaseChangeControlText -Needle 'hosted-evidence-pending' -Issue "Release change-control guard must keep win-x86/full pending"
Assert-Contains -Violations $violations -Path $releaseChangeControlPath -Text $releaseChangeControlText -Needle '[string]$SbomRoot = ""' -Issue "Release change-control guard must accept package-bound SBOM inputs"
Assert-Contains -Violations $violations -Path $releaseChangeControlPath -Text $releaseChangeControlText -Needle '[string]$OutputPath = ""' -Issue "Release change-control guard must support durable output"
Assert-Contains -Violations $violations -Path $releaseChangeControlPath -Text $releaseChangeControlText -Needle '[switch]$Check' -Issue "Release change-control guard must support byte-for-byte output checks"
Assert-Contains -Violations $violations -Path $releaseChangeControlPath -Text $releaseChangeControlText -Needle 'current-unsigned-candidate' -Issue "Release change-control guard must distinguish current unsigned candidates from historical artifacts"
Assert-Contains -Violations $violations -Path $releaseChangeControlPath -Text $releaseChangeControlText -Needle 'generated-unapproved' -Issue "Release change-control guard must distinguish generated SBOMs from external approval"
Assert-Contains -Violations $violations -Path $releaseChangeControlPath -Text $releaseChangeControlText -Needle 'RequiredApprovalInputs' -Issue "Release change-control guard must retain the explicit signing/approval input checklist"
Assert-Contains -Violations $violations -Path $releaseChangeControlPath -Text $releaseChangeControlText -Needle '[Linq.Enumerable]::SequenceEqual($outputBytes' -Issue "Release change-control check mode must compare exact deterministic bytes"

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
    "compatib|legacy|existing|already-compiled|kept stable|explicit|\u517c\u5bb9|\u65e2\u6709|\u5df2\u7f16\u8bd1|\u4fdd\u7559|\u660e\u786e",
    [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)

$releaseSurfaceFiles = @(
    $packWorkflowPath,
    $runtimeInputWorkflowPath,
    $packManagedPath,
    $packRuntimePath,
    $stageRuntimePath,
    $runtimeProjectPath,
    $androidRuntimeTargetsPath,
    $androidRuntimeProducerPath,
    $androidSmokeProjectPath,
    $androidSmokeActivityPath,
    $runtimeReadmePath,
    $readmePath,
    $chineseReadmePath,
    "CONTRIBUTING.md",
    $runtimeReleasePreflightPath,
    $runtimeReleasePreflightGuardPath,
    $releasePackageSbomGeneratorPath,
    $releasePackageSbomGuardPath,
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
    $releaseCloseoutDocPath,
    $releaseNotesPath
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

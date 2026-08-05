param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$recordRelativePath = "packaging/release/local-release-candidate-closeout.json"
$recordPath = Join-Path $repo ($recordRelativePath -replace '/', [IO.Path]::DirectorySeparatorChar)
$violations = [System.Collections.Generic.List[object]]::new()
$textExtensions = @(
    ".c", ".cc", ".cpp", ".cs", ".cmake", ".csproj", ".h", ".hpp", ".json", ".md", ".props", ".ps1", ".slnx", ".targets", ".txt", ".yml", ".yaml", ".xml"
)
$textFileNames = @("CMakeLists.txt", ".gitignore", "global.json")

function Add-Violation {
    param(
        [Parameter(Mandatory)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$List,
        [Parameter(Mandatory)][string]$Issue,
        [string]$Text = ""
    )

    $List.Add([pscustomobject]@{ Path = $recordRelativePath; Issue = $Issue; Text = $Text.Trim() })
}

function Assert-True {
    param(
        [Parameter(Mandatory)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$List,
        [Parameter(Mandatory)][bool]$Condition,
        [Parameter(Mandatory)][string]$Issue,
        [string]$Text = ""
    )

    if (-not $Condition) { Add-Violation -List $List -Issue $Issue -Text $Text }
}

function Normalize-Text {
    param([Parameter(Mandatory)][AllowEmptyString()][string]$Text)
    return (($Text -replace "`r`n", "`n") -replace "`r", "`n").TrimEnd() + "`n"
}

function Get-LogicalFileEvidence {
    param([Parameter(Mandatory)][string]$Path)

    $extension = [IO.Path]::GetExtension($Path).ToLowerInvariant()
    $name = [IO.Path]::GetFileName($Path)
    if ($extension -in $textExtensions -or $name -in $textFileNames) {
        $bytes = [Text.UTF8Encoding]::new($false).GetBytes((Normalize-Text ([IO.File]::ReadAllText($Path))))
    }
    else {
        $bytes = [IO.File]::ReadAllBytes($Path)
    }

    return [pscustomobject]@{
        Sha256 = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
        Length = $bytes.Length
    }
}

function Get-OrdinalSorted {
    param([Parameter(Mandatory)][AllowEmptyCollection()][string[]]$Values)
    $copy = [string[]]$Values.Clone()
    [Array]::Sort($copy, [StringComparer]::Ordinal)
    return $copy
}

function Get-ExpectedEvidencePaths {
    $paths = @(
        ".github/workflows/pack.yml",
        ".github/workflows/publish-nuget.yml",
        ".github/workflows/runtime-input.yml",
        "compatibility/api-gap-inventory.json",
        "compatibility/calib3d-implemented-families.json",
        "compatibility/calib3d-upstream-classifications.json",
        "compatibility/calib3d-upstream-map.txt",
        "compatibility/calib3d-upstream-raw.json",
        "compatibility/calib3d-upstream-summary.json",
        "compatibility/core-implemented-families.json",
        "compatibility/core-upstream-classifications.json",
        "compatibility/core-upstream-map.txt",
        "compatibility/core-upstream-raw.json",
        "compatibility/core-upstream-summary.json",
        "compatibility/dnn-implemented-families.json",
        "compatibility/dnn-upstream-classifications.json",
        "compatibility/dnn-upstream-map.txt",
        "compatibility/dnn-upstream-raw.json",
        "compatibility/dnn-upstream-summary.json",
        "compatibility/features-implemented-families.json",
        "compatibility/features-source-reviewed-extensions.json",
        "compatibility/features-upstream-classifications.json",
        "compatibility/features-upstream-map.txt",
        "compatibility/features-upstream-raw.json",
        "compatibility/features-upstream-summary.json",
        "compatibility/highgui-implemented-families.json",
        "compatibility/highgui-upstream-classifications.json",
        "compatibility/highgui-upstream-map.txt",
        "compatibility/highgui-upstream-raw.json",
        "compatibility/highgui-upstream-summary.json",
        "compatibility/imgcodecs-implemented-families.json",
        "compatibility/imgcodecs-source-reviewed-extensions.json",
        "compatibility/imgcodecs-upstream-classifications.json",
        "compatibility/imgcodecs-upstream-map.txt",
        "compatibility/imgcodecs-upstream-raw.json",
        "compatibility/imgcodecs-upstream-summary.json",
        "compatibility/imgproc-implemented-families.json",
        "compatibility/imgproc-point-set-span-family.json",
        "compatibility/imgproc-upstream-classifications.json",
        "compatibility/imgproc-upstream-map.txt",
        "compatibility/imgproc-upstream-raw.json",
        "compatibility/imgproc-upstream-summary.json",
        "compatibility/managed-public-api-summary.json",
        "compatibility/managed-public-api.txt",
        "compatibility/ml-implemented-families.json",
        "compatibility/ml-upstream-classifications.json",
        "compatibility/ml-upstream-map.txt",
        "compatibility/ml-upstream-raw.json",
        "compatibility/ml-upstream-summary.json",
        "compatibility/native-managed-binding-map.txt",
        "compatibility/native-managed-binding-summary.json",
        "compatibility/objdetect-implemented-families.json",
        "compatibility/objdetect-upstream-classifications.json",
        "compatibility/objdetect-upstream-map.txt",
        "compatibility/objdetect-upstream-raw.json",
        "compatibility/objdetect-upstream-summary.json",
        "compatibility/photo-implemented-families.json",
        "compatibility/photo-upstream-classifications.json",
        "compatibility/photo-upstream-map.txt",
        "compatibility/photo-upstream-raw.json",
        "compatibility/photo-upstream-summary.json",
        "compatibility/stitching-implemented-families.json",
        "compatibility/stitching-upstream-classifications.json",
        "compatibility/stitching-upstream-map.txt",
        "compatibility/stitching-upstream-raw.json",
        "compatibility/stitching-upstream-summary.json",
        "compatibility/tracking-implemented-families.json",
        "compatibility/tracking-upstream-classifications.json",
        "compatibility/tracking-upstream-map.txt",
        "compatibility/tracking-upstream-raw.json",
        "compatibility/tracking-upstream-summary.json",
        "compatibility/video-implemented-families.json",
        "compatibility/video-upstream-classifications.json",
        "compatibility/video-upstream-map.txt",
        "compatibility/video-upstream-raw.json",
        "compatibility/video-upstream-summary.json",
        "compatibility/videoio-implemented-families.json",
        "compatibility/videoio-registry-surface.json",
        "compatibility/videoio-upstream-classifications.json",
        "compatibility/videoio-upstream-map.txt",
        "compatibility/videoio-upstream-raw.json",
        "compatibility/videoio-upstream-summary.json",
        "docs/articles/api-abi-compatibility-policy.md",
        "docs/articles/calib3d-upstream-parity-guide.md",
        "docs/articles/core-upstream-parity-guide.md",
        "docs/articles/dnn-structured-parity-guide.md",
        "docs/articles/features-upstream-parity-guide.md",
        "docs/articles/highgui-interaction-guide.md",
        "docs/articles/imgcodecs-upstream-parity-guide.md",
        "docs/articles/imgproc-geometry-guide.md",
        "docs/articles/imgproc-upstream-parity-guide.md",
        "docs/articles/ml-guide.md",
        "docs/articles/objdetect-structured-parity-guide.md",
        "docs/articles/photo-ccm-guide.md",
        "docs/articles/photo-hdr-workflow-guide.md",
        "docs/articles/photo-intelligent-scissors-guide.md",
        "docs/articles/photo-tvl1-chromatic-aberration-guide.md",
        "docs/articles/point-set-marshalling-guide.md",
        "docs/articles/release-candidate-closeout.md",
        "docs/articles/stitching-structured-parity-guide.md",
        "docs/articles/support-lifecycle-policy.md",
        "docs/articles/tutorial-series.md",
        "docs/articles/tutorial-01-image-pipeline.md",
        "docs/articles/tutorial-02-chinese-puttext.md",
        "docs/articles/tutorial-03-contours.md",
        "docs/articles/tutorial-04-orb-features.md",
        "docs/articles/tutorial-05-template-matching.md",
        "docs/articles/tutorial-06-knn-classification.md",
        "docs/articles/tutorial-07-android-runtime.md",
        "docs/images/showcase/chinese-text.png",
        "docs/images/showcase/contours.png",
        "docs/images/showcase/image-pipeline.png",
        "docs/images/showcase/knn-classification.png",
        "docs/images/showcase/orb-features.png",
        "docs/images/showcase/showcase-overview.png",
        "docs/images/showcase/source.png",
        "docs/images/showcase/template-match.png",
        "docs/index.md",
        "docs/toc.yml",
        "docs/articles/tracking-guide.md",
        "docs/articles/video-upstream-parity-guide.md",
        "docs/articles/videoio-upstream-parity-guide.md",
        "nuget/logo.jpg",
        "packaging/runtime/android-runtime-evidence.json",
        "packaging/runtime/runtime-support-contract.json",
        "samples/AndroidSmoke/AndroidSmoke.csproj",
        "samples/AndroidSmoke/MainActivity.cs",
        "samples/ConsoleSamples/Program.cs",
        "samples/ConsoleSamples/ShowcaseRunner.cs",
        "scripts/Generate-Calib3DUpstreamMap.ps1",
        "scripts/Generate-CoreUpstreamMap.ps1",
        "scripts/Generate-DnnUpstreamMap.ps1",
        "scripts/Generate-FeaturesUpstreamMap.ps1",
        "scripts/Generate-HighGuiUpstreamMap.ps1",
        "scripts/Generate-ImgCodecsUpstreamMap.ps1",
        "scripts/Generate-ImgProcUpstreamMap.ps1",
        "scripts/Generate-ManagedPublicApiBaseline.ps1",
        "scripts/Generate-MlUpstreamMap.ps1",
        "scripts/Generate-NativeAbiManifest.ps1",
        "scripts/Generate-NativeManagedBindingMap.ps1",
        "scripts/Test-NoUnpublishedCompatibilitySurface.ps1",
        "scripts/Generate-ObjDetectUpstreamMap.ps1",
        "scripts/Generate-PhotoUpstreamMap.ps1",
        "scripts/Generate-StitchingUpstreamMap.ps1",
        "scripts/Generate-TrackingUpstreamMap.ps1",
        "scripts/Generate-VideoUpstreamMap.ps1",
        "packaging/runtime/JYPPX.OpenCV.runtime/buildTransitive/JYPPX.OpenCV.runtime.targets",
        "scripts/Build-AndroidRuntimeInput.ps1",
        "scripts/New-ReleaseCandidateFinalCloseout.ps1",
        "scripts/New-NuGetPublicationBundle.ps1",
        "scripts/Test-NuGetPublicationManifest.ps1",
        "scripts/New-ReleasePackageSbom.ps1",
        "scripts/Test-ApiAbiBaselineContract.ps1",
        "scripts/Test-Calib3DUpstreamMap.ps1",
        "scripts/Test-CoreUpstreamMap.ps1",
        "scripts/Test-DnnUpstreamMap.ps1",
        "scripts/Test-FeaturesUpstreamMap.ps1",
        "scripts/Test-HighGuiUpstreamMap.ps1",
        "scripts/Test-ImgCodecsUpstreamMap.ps1",
        "scripts/Test-ImgProcUpstreamMap.ps1",
        "scripts/Test-GitHubPackArtifactMatrixSurface.ps1",
        "scripts/Test-ManagedPackageIsolatedArtifactSurface.ps1",
        "scripts/Test-ManagedPackageStandaloneLocalConsumerCompile.ps1",
        "scripts/Test-MlUpstreamMap.ps1",
        "scripts/Test-NativeManagedBindingMap.ps1",
        "scripts/Test-ObjDetectUpstreamMap.ps1",
        "scripts/Test-PhotoUpstreamMap.ps1",
        "scripts/Test-ReleaseCandidateFinalCloseout.ps1",
        "scripts/Test-ReleaseCandidateProvenance.ps1",
        "scripts/Test-ReleaseChangeControlRecord.ps1",
        "scripts/Test-ReleasePackageReproducibility.ps1",
        "scripts/Test-ReleasePackageSbom.ps1",
        "scripts/Test-ReleaseReadinessContract.ps1",
        "scripts/Test-ReleaseSigningBoundary.ps1",
        "scripts/Test-NuGetRepositorySignedPackage.ps1",
        "scripts/Test-NuGetRepositorySigningBoundary.ps1",
        "scripts/Test-PackageMetadataNeutrality.ps1",
        "scripts/Test-ReleaseSupportContract.ps1",
        "scripts/Test-StitchingUpstreamMap.ps1",
        "scripts/Test-TrackingUpstreamMap.ps1",
        "scripts/Test-VideoIORegistrySurface.ps1",
        "scripts/Test-VideoIOUpstreamMap.ps1",
        "scripts/Test-VideoUpstreamMap.ps1",
        "scripts/Test-WorkflowInvariantCoverage.ps1",
        "src/OpenCvSharp.Native/generated/native_abi_manifest.txt",
        "src/OpenCvSharp.Native/generated/native_abi_mini_manifest.txt",
        "src/OpenCvSharp.Native/include/open_cv_sharp/highgui/highgui.h",
        "src/OpenCvSharp.Native/src/highgui/highgui.cpp",
        "src/OpenCvSharp.Native/include/open_cv_sharp/stitching/stitching.h",
        "src/OpenCvSharp.Native/src/stitching/stitching.cpp",
        "src/OpenCvSharp.Native/src/stitching/stitching_handles.h",
        "src/OpenCvSharp.Native/src/imgproc.cpp",
        "src/OpenCvSharp.Native/tests/native_smoke.cpp",
        "src/OpenCvSharp/ImgProc/Cv2.RemainingParity.cs",
        "src/OpenCvSharp/ImgProc/FontFace.cs",
        "src/OpenCvSharp/Internal/Interop/NativeBlenderHandle.cs",
        "src/OpenCvSharp/Internal/Interop/NativeEstimatorHandle.cs",
        "src/OpenCvSharp/Internal/Interop/NativeFeaturesMatcherHandle.cs",
        "src/OpenCvSharp/Internal/Interop/NativeHighGuiCallbackRegistrationHandle.cs",
        "src/OpenCvSharp/Internal/Interop/NativeMethods.HighGui.DllImport.cs",
        "src/OpenCvSharp/Internal/Interop/NativeMethods.HighGui.LibraryImport.cs",
        "src/OpenCvSharp/Internal/Interop/NativeImageFeaturesHandle.cs",
        "src/OpenCvSharp/Internal/Interop/NativeMatchesInfoHandle.cs",
        "src/OpenCvSharp/Internal/Interop/NativeMethods.Stitching.DllImport.cs",
        "src/OpenCvSharp/Internal/Interop/NativeMethods.Stitching.LibraryImport.cs",
        "src/OpenCvSharp/Stitching/AffineBestOf2NearestMatcher.cs",
        "src/OpenCvSharp/Stitching/BestOf2NearestMatcher.cs",
        "src/OpenCvSharp/Stitching/BestOf2NearestRangeMatcher.cs",
        "src/OpenCvSharp/Stitching/Blender.cs",
        "src/OpenCvSharp/Stitching/BlenderType.cs",
        "src/OpenCvSharp/Stitching/Blenders.cs",
        "src/OpenCvSharp/Stitching/BundleAdjusters.cs",
        "src/OpenCvSharp/Stitching/Estimator.cs",
        "src/OpenCvSharp/Stitching/FeaturesMatcher.cs",
        "src/OpenCvSharp/Stitching/ImageFeatures.cs",
        "src/OpenCvSharp/Stitching/MatchesInfo.cs",
        "src/OpenCvSharp/Stitching/StitcherCameraParams.cs",
        "src/OpenCvSharp/Stitching/StitchingMotion.cs",
        "src/OpenCvSharp/HighGui/Cv2.cs",
        "src/OpenCvSharp/HighGui/HighGuiStringConvert.cs",
        "src/OpenCvSharp/HighGui/HighGuiTrackbar.cs",
        "tests/OpenCvSharp.Tests/Stitching/BlenderTests.cs",
        "tests/OpenCvSharp.Tests/Stitching/FeaturesMatcherTests.cs",
        "tests/OpenCvSharp.Tests/Stitching/MotionEstimatorTests.cs",
        "tests/OpenCvSharp.Tests/HighGui/HighGuiInteractionTests.cs",
        "tests/OpenCvSharp.Tests/HighGui/HighGuiTests.cs",
        "tests/OpenCvSharp.Tests/ImgProc/ImgProcRemainingParityTests.cs",
        "tools/Calib3DUpstreamMap/Calib3DUpstreamMap.csproj",
        "tools/Calib3DUpstreamMap/Program.cs",
        "tools/Calib3DUpstreamMap/extract_calib3d.py",
        "tools/CoreUpstreamMap/CoreUpstreamMap.csproj",
        "tools/CoreUpstreamMap/Program.cs",
        "tools/CoreUpstreamMap/extract_core.py",
        "tools/DnnUpstreamMap/DnnUpstreamMap.csproj",
        "tools/DnnUpstreamMap/Program.cs",
        "tools/DnnUpstreamMap/extract_dnn.py",
        "tools/FeaturesUpstreamMap/FeaturesUpstreamMap.csproj",
        "tools/FeaturesUpstreamMap/Program.cs",
        "tools/FeaturesUpstreamMap/extract_features.py",
        "tools/HighGuiUpstreamMap/HighGuiUpstreamMap.csproj",
        "tools/HighGuiUpstreamMap/Program.cs",
        "tools/HighGuiUpstreamMap/extract_highgui.py",
        "tools/ImgCodecsUpstreamMap/ImgCodecsUpstreamMap.csproj",
        "tools/ImgCodecsUpstreamMap/Program.cs",
        "tools/ImgCodecsUpstreamMap/extract_imgcodecs.py",
        "tools/ImgProcUpstreamMap/ImgProcUpstreamMap.csproj",
        "tools/ImgProcUpstreamMap/Program.cs",
        "tools/ImgProcUpstreamMap/extract_imgproc.py",
        "tools/MlUpstreamMap/MlUpstreamMap.csproj",
        "tools/MlUpstreamMap/Program.cs",
        "tools/MlUpstreamMap/extract_ml.py",
        "tools/NativeManagedBindingMap/NativeManagedBindingMap.csproj",
        "tools/NativeManagedBindingMap/Program.cs",
        "tools/ObjDetectUpstreamMap/ObjDetectUpstreamMap.csproj",
        "tools/ObjDetectUpstreamMap/Program.cs",
        "tools/ObjDetectUpstreamMap/extract_objdetect.py",
        "tools/PhotoUpstreamMap/PhotoUpstreamMap.csproj",
        "tools/PhotoUpstreamMap/Program.cs",
        "tools/PhotoUpstreamMap/extract_photo.py",
        "tools/StitchingUpstreamMap/Program.cs",
        "tools/StitchingUpstreamMap/StitchingUpstreamMap.csproj",
        "tools/StitchingUpstreamMap/extract_stitching.py",
        "tools/TrackingUpstreamMap/Program.cs",
        "tools/TrackingUpstreamMap/TrackingUpstreamMap.csproj",
        "tools/TrackingUpstreamMap/extract_tracking.py",
        "tools/NuGetRepositorySignatureVerifier/NuGetRepositorySignatureVerifier.csproj",
        "tools/NuGetRepositorySignatureVerifier/Program.cs",
        "tools/VideoUpstreamMap/Program.cs",
        "tools/VideoUpstreamMap/VideoUpstreamMap.csproj",
        "tools/VideoUpstreamMap/extract_video.py"
    )
    return @(Get-OrdinalSorted -Values $paths)
}

function Test-Record {
    param(
        [Parameter(Mandatory)][object]$Record,
        [Parameter(Mandatory)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$List,
        [Parameter(Mandatory)][string]$ExpectedSourceHash
    )

    $required = @("SchemaVersion","RecordKind","CandidateId","SourceIdentity","OpenCvRevision","SourceSet","PackageMatrix","SupportContract","ApiAbiBaseline","EvidenceReferences","LocalValidation","Signing","Sbom","Approval","PublicFeed","Rollback","ReleaseApproval","ExternalBlockers","PrivateKeyMaterialPresent","SecretMaterialPresent","Deterministic")
    foreach ($field in $required) {
        Assert-True -List $List -Condition ($null -ne $Record.PSObject.Properties[$field]) -Issue "Final closeout record is missing required field" -Text $field
    }
    if ($null -eq $Record.SourceSet -or $null -eq $Record.SupportContract -or $null -eq $Record.ApiAbiBaseline -or $null -eq $Record.LocalValidation) { return }

    Assert-True -List $List -Condition ($Record.SchemaVersion -eq 2 -and $Record.RecordKind -eq "local-release-candidate-closeout" -and $Record.OpenCvRevision -eq "5.0.0") -Issue "Final closeout record identity drifted"
    $expectedCandidateId = "local-closeout/sha256/$(([string]$Record.SourceSet.Sha256).Substring(0, 16))"
    $expectedSourceIdentity = "sha256:$([string]$Record.SourceSet.Sha256)"
    Assert-True -List $List -Condition ($Record.CandidateId -eq $expectedCandidateId -and $Record.SourceIdentity -eq $expectedSourceIdentity) -Issue "Final closeout candidate identity is malformed or is not derived from the source-set digest"
    Assert-True -List $List -Condition ([int]$Record.SourceSet.FileCount -gt 0 -and $Record.SourceSet.Sha256 -match "^[0-9a-f]{64}$" -and $Record.SourceSet.HashPolicy -like "UTF-8 text normalized*") -Issue "Final closeout source-set digest is missing or malformed"
    Assert-True -List $List -Condition ($Record.SourceSet.Sha256 -eq $ExpectedSourceHash) -Issue "Final closeout source-set digest drifted" -Text "expected=$ExpectedSourceHash actual=$($Record.SourceSet.Sha256)"

    Assert-True -List $List -Condition ([int]$Record.PackageMatrix.RidCount -gt 0 -and [int]$Record.PackageMatrix.ProfileCount -eq 2 -and [int]$Record.PackageMatrix.EntryCount -eq 34 -and $Record.PackageMatrix.Sha256 -match "^[0-9a-f]{64}$") -Issue "Final closeout package matrix evidence drifted"
    Assert-True -List $List -Condition ($Record.SupportContract.MatrixEntryCount -eq 34 -and $Record.SupportContract.RealSupportCount -eq 28 -and $Record.SupportContract.PendingSupportCount -eq 5 -and $Record.SupportContract.ExcludedSupportCount -eq 1 -and $Record.SupportContract.OutsideMatrixCount -eq 1 -and $Record.SupportContract.WinX86FullStatus -eq "hosted-evidence-pending" -and $Record.SupportContract.WinX86MiniStatus -eq "excluded" -and -not [bool]$Record.SupportContract.PackageSurfaceDefinesSupport) -Issue "Final closeout support partition or policy drifted"

    Assert-True -List $List -Condition ($Record.ApiAbiBaseline.Managed.Sha256 -eq "8c11d1eea4a349ee782d7e4ede740113ecc40102d83a8a0860bdc6de100225fb" -and $Record.ApiAbiBaseline.Managed.TypeCount -eq 611 -and $Record.ApiAbiBaseline.Managed.MemberCount -eq 6300 -and $Record.ApiAbiBaseline.Managed.NamespaceCount -eq 41 -and $Record.ApiAbiBaseline.Managed.TargetFramework -eq "net8.0") -Issue "Final closeout managed API baseline evidence drifted"
    Assert-True -List $List -Condition ($Record.ApiAbiBaseline.NativeFull.Sha256 -eq "cc26f65ffa8e6c422b6db1e50fb6f29441907283349f2ac4b47762a69fb00473" -and $Record.ApiAbiBaseline.NativeFull.FunctionCount -eq 2656 -and $Record.ApiAbiBaseline.NativeMini.Sha256 -eq "db7cfee61317ade8765a21da370475e7832c0eb5f051ac6891598515d1b3e61d" -and $Record.ApiAbiBaseline.NativeMini.FunctionCount -eq 526) -Issue "Final closeout native ABI baseline evidence drifted"
    Assert-True -List $List -Condition ($Record.ApiAbiBaseline.NativeManagedBindingMap.Sha256 -eq "c3cc09873fa796e3190dd952ca8d5c054f8dc2e958c8de8e3ca1cc50b5c3bc6f" -and $Record.ApiAbiBaseline.NativeManagedBindingMap.NativeFunctionCount -eq 2656 -and $Record.ApiAbiBaseline.NativeManagedBindingMap.ManagedBoundCount -eq 2656 -and $Record.ApiAbiBaseline.NativeManagedBindingMap.UnboundCount -eq 0 -and $Record.ApiAbiBaseline.NativeManagedBindingMap.ManagedOnlyCount -eq 0) -Issue "Final closeout native-to-managed binding evidence drifted"
    Assert-True -List $List -Condition ($Record.ApiAbiBaseline.ImgProcUpstreamMap.Sha256 -eq "5b4bf38bfb2dd40868d8631b8d527bed1290c51f6a9f94bebc316f2526868b6a" -and $Record.ApiAbiBaseline.ImgProcUpstreamMap.DeclarationCount -eq 203 -and $Record.ApiAbiBaseline.ImgProcUpstreamMap.CallableCount -eq 167 -and $Record.ApiAbiBaseline.ImgProcUpstreamMap.ImplementedCount -eq 161 -and $Record.ApiAbiBaseline.ImgProcUpstreamMap.MissingCount -eq 0 -and $Record.ApiAbiBaseline.ImgProcUpstreamMap.IntentionallyOmittedCount -eq 6 -and $Record.ApiAbiBaseline.ImgProcUpstreamMap.SelectedFamilyCount -eq 8 -and $Record.ApiAbiBaseline.ImgProcUpstreamMap.SelectedDeclarationCount -eq 90 -and $Record.ApiAbiBaseline.ImgProcUpstreamMap.ManagedPublicMemberAdditionCount -eq 174 -and -not [bool]$Record.ApiAbiBaseline.ImgProcUpstreamMap.RepositoryWideParityClaimed) -Issue "Final closeout ImgProc upstream-map evidence drifted"
    Assert-True -List $List -Condition ($Record.ApiAbiBaseline.ImgCodecsUpstreamMap.Sha256 -eq "fedb1e9cc6713238cec6043af94b55d346ecdcf568b12c1da1b88ab49ae761f5" -and $Record.ApiAbiBaseline.ImgCodecsUpstreamMap.DeclarationCount -eq 39 -and $Record.ApiAbiBaseline.ImgCodecsUpstreamMap.CallableCount -eq 22 -and $Record.ApiAbiBaseline.ImgCodecsUpstreamMap.ImplementedCount -eq 22 -and $Record.ApiAbiBaseline.ImgCodecsUpstreamMap.MissingCount -eq 0 -and $Record.ApiAbiBaseline.ImgCodecsUpstreamMap.IntentionallyOmittedCount -eq 0 -and $Record.ApiAbiBaseline.ImgCodecsUpstreamMap.SelectedFamilyCount -eq 5 -and $Record.ApiAbiBaseline.ImgCodecsUpstreamMap.SelectedDeclarationCount -eq 40 -and $Record.ApiAbiBaseline.ImgCodecsUpstreamMap.ManagedPublicTypeAdditionCount -eq 16 -and $Record.ApiAbiBaseline.ImgCodecsUpstreamMap.ManagedPublicMemberAdditionCount -eq 168 -and -not [bool]$Record.ApiAbiBaseline.ImgCodecsUpstreamMap.RepositoryWideParityClaimed) -Issue "Final closeout ImgCodecs upstream-map evidence drifted"
    Assert-True -List $List -Condition ($Record.ApiAbiBaseline.VideoIOUpstreamMap.Sha256 -eq "b7605406830526c0b1afc4afc674aaa4a0cf307f1cb3652a489f8545a5cb8de8" -and $Record.ApiAbiBaseline.VideoIOUpstreamMap.RegistrySurfaceSha256 -eq "b629bec86d9bd6dbf428a4b524fd473c01454e6a9ece39aaadf672b4f93373f8" -and $Record.ApiAbiBaseline.VideoIOUpstreamMap.DeclarationCount -eq 71 -and $Record.ApiAbiBaseline.VideoIOUpstreamMap.CallableCount -eq 40 -and $Record.ApiAbiBaseline.VideoIOUpstreamMap.ImplementedCount -eq 40 -and $Record.ApiAbiBaseline.VideoIOUpstreamMap.MissingCount -eq 0 -and $Record.ApiAbiBaseline.VideoIOUpstreamMap.IntentionallyOmittedCount -eq 0 -and $Record.ApiAbiBaseline.VideoIOUpstreamMap.RegistryOperationCount -eq 12 -and -not [bool]$Record.ApiAbiBaseline.VideoIOUpstreamMap.RepositoryWideParityClaimed) -Issue "Final closeout VideoIO upstream-map evidence drifted"
    Assert-True -List $List -Condition ($Record.ApiAbiBaseline.Calib3DUpstreamMap.Sha256 -eq "d171ffc695c3eae70ffe415a3ef75c83cee21cdfe0add3d990b8304f17cb5ed5" -and $Record.ApiAbiBaseline.Calib3DUpstreamMap.DeclarationCount -eq 194 -and $Record.ApiAbiBaseline.Calib3DUpstreamMap.CallableCount -eq 167 -and $Record.ApiAbiBaseline.Calib3DUpstreamMap.ImplementedCount -eq 167 -and $Record.ApiAbiBaseline.Calib3DUpstreamMap.MissingCount -eq 0 -and $Record.ApiAbiBaseline.Calib3DUpstreamMap.IntentionallyOmittedCount -eq 0 -and $Record.ApiAbiBaseline.Calib3DUpstreamMap.SourceHeaderCount -eq 4 -and $Record.ApiAbiBaseline.Calib3DUpstreamMap.SelectedFamilyCount -eq 11 -and $Record.ApiAbiBaseline.Calib3DUpstreamMap.SelectedDeclarationCount -eq 194 -and $Record.ApiAbiBaseline.Calib3DUpstreamMap.ManagedPublicTypeAdditionCount -eq 12 -and $Record.ApiAbiBaseline.Calib3DUpstreamMap.ManagedPublicMemberAdditionCount -eq 120 -and -not [bool]$Record.ApiAbiBaseline.Calib3DUpstreamMap.RepositoryWideParityClaimed) -Issue "Final closeout Calib3D upstream-map evidence drifted"
    Assert-True -List $List -Condition ($Record.ApiAbiBaseline.CoreUpstreamMap.Sha256 -eq "cfaaed815e4c7f0173739f17fdb8b13fc5f995cee7c26542b52640a9fefcb1f1" -and $Record.ApiAbiBaseline.CoreUpstreamMap.DeclarationCount -eq 258 -and $Record.ApiAbiBaseline.CoreUpstreamMap.CallableCount -eq 215 -and $Record.ApiAbiBaseline.CoreUpstreamMap.ImplementedCount -eq 176 -and $Record.ApiAbiBaseline.CoreUpstreamMap.MissingCount -eq 0 -and $Record.ApiAbiBaseline.CoreUpstreamMap.IntentionallyOmittedCount -eq 29 -and $Record.ApiAbiBaseline.CoreUpstreamMap.UnsupportedCount -eq 5 -and $Record.ApiAbiBaseline.CoreUpstreamMap.UpstreamConditionalCount -eq 5 -and $Record.ApiAbiBaseline.CoreUpstreamMap.SourceHeaderCount -eq 11 -and $Record.ApiAbiBaseline.CoreUpstreamMap.SelectedFamilyCount -eq 4 -and $Record.ApiAbiBaseline.CoreUpstreamMap.SelectedDeclarationCount -eq 108 -and $Record.ApiAbiBaseline.CoreUpstreamMap.ManagedPublicTypeAdditionCount -eq 11 -and $Record.ApiAbiBaseline.CoreUpstreamMap.ManagedPublicMemberAdditionCount -eq 226 -and -not [bool]$Record.ApiAbiBaseline.CoreUpstreamMap.RepositoryWideParityClaimed) -Issue "Final closeout Core upstream-map evidence drifted"
    Assert-True -List $List -Condition ($Record.ApiAbiBaseline.DnnUpstreamMap.Sha256 -eq "4c482153942cbb32a6fc5b6382a06acdbc2329c177af4f78378e09d15f0f517d" -and $Record.ApiAbiBaseline.DnnUpstreamMap.DeclarationCount -eq 182 -and $Record.ApiAbiBaseline.DnnUpstreamMap.CallableCount -eq 159 -and $Record.ApiAbiBaseline.DnnUpstreamMap.ImplementedCount -eq 70 -and $Record.ApiAbiBaseline.DnnUpstreamMap.MissingCount -eq 0 -and $Record.ApiAbiBaseline.DnnUpstreamMap.IntentionallyOmittedCount -eq 81 -and $Record.ApiAbiBaseline.DnnUpstreamMap.UnsupportedCount -eq 2 -and $Record.ApiAbiBaseline.DnnUpstreamMap.UpstreamConditionalCount -eq 6 -and $Record.ApiAbiBaseline.DnnUpstreamMap.SourceHeaderCount -eq 3 -and $Record.ApiAbiBaseline.DnnUpstreamMap.SelectedFamilyCount -eq 4 -and $Record.ApiAbiBaseline.DnnUpstreamMap.SelectedDeclarationCount -eq 80 -and $Record.ApiAbiBaseline.DnnUpstreamMap.ManagedPublicTypeAdditionCount -eq 10 -and $Record.ApiAbiBaseline.DnnUpstreamMap.ManagedPublicMemberAdditionCount -eq 94 -and -not [bool]$Record.ApiAbiBaseline.DnnUpstreamMap.RepositoryWideParityClaimed) -Issue "Final closeout DNN upstream-map evidence drifted"
    Assert-True -List $List -Condition ($Record.ApiAbiBaseline.FeaturesUpstreamMap.Sha256 -eq "af315fa7f08a8f88a391a710297372fb11997b84c1084fab10adfcb19ce7e20b" -and $Record.ApiAbiBaseline.FeaturesUpstreamMap.DeclarationCount -eq 183 -and $Record.ApiAbiBaseline.FeaturesUpstreamMap.CallableCount -eq 160 -and $Record.ApiAbiBaseline.FeaturesUpstreamMap.ImplementedCount -eq 134 -and $Record.ApiAbiBaseline.FeaturesUpstreamMap.MissingCount -eq 0 -and $Record.ApiAbiBaseline.FeaturesUpstreamMap.IntentionallyOmittedCount -eq 26 -and $Record.ApiAbiBaseline.FeaturesUpstreamMap.UnsupportedCount -eq 0 -and $Record.ApiAbiBaseline.FeaturesUpstreamMap.UpstreamConditionalCount -eq 0 -and $Record.ApiAbiBaseline.FeaturesUpstreamMap.CompatibilityHeaderCount -eq 2 -and $Record.ApiAbiBaseline.FeaturesUpstreamMap.SourceHeaderCount -eq 1 -and $Record.ApiAbiBaseline.FeaturesUpstreamMap.SourceReviewedExtensionCount -eq 9 -and $Record.ApiAbiBaseline.FeaturesUpstreamMap.SelectedFamilyCount -eq 1 -and $Record.ApiAbiBaseline.FeaturesUpstreamMap.SelectedDeclarationCount -eq 12 -and $Record.ApiAbiBaseline.FeaturesUpstreamMap.ManagedPublicTypeAdditionCount -eq 2 -and $Record.ApiAbiBaseline.FeaturesUpstreamMap.ManagedPublicMemberAdditionCount -eq 18 -and -not [bool]$Record.ApiAbiBaseline.FeaturesUpstreamMap.RepositoryWideParityClaimed) -Issue "Final closeout Features upstream-map evidence drifted"
    Assert-True -List $List -Condition ($Record.ApiAbiBaseline.HighGuiUpstreamMap.Sha256 -eq "83d4e9b279217e6315176cb344dd56bcfb90ae4d6c9afbc645be72c162752961" -and $Record.ApiAbiBaseline.HighGuiUpstreamMap.RawSha256 -eq "8d21c928dc3edd89fd3f57ee8b43aafc415c55b59bccf8e817d01cc2c9b2ba82" -and $Record.ApiAbiBaseline.HighGuiUpstreamMap.DeclarationCount -eq 33 -and $Record.ApiAbiBaseline.HighGuiUpstreamMap.CallableCount -eq 26 -and $Record.ApiAbiBaseline.HighGuiUpstreamMap.ImplementedCount -eq 20 -and $Record.ApiAbiBaseline.HighGuiUpstreamMap.MissingCount -eq 0 -and $Record.ApiAbiBaseline.HighGuiUpstreamMap.IntentionallyOmittedCount -eq 3 -and $Record.ApiAbiBaseline.HighGuiUpstreamMap.UnsupportedCount -eq 0 -and $Record.ApiAbiBaseline.HighGuiUpstreamMap.UpstreamConditionalCount -eq 3 -and $Record.ApiAbiBaseline.HighGuiUpstreamMap.CompatibilityHeaderCount -eq 3 -and $Record.ApiAbiBaseline.HighGuiUpstreamMap.ExcludedPublicHeaderCount -eq 0 -and $Record.ApiAbiBaseline.HighGuiUpstreamMap.SourceHeaderCount -eq 1 -and $Record.ApiAbiBaseline.HighGuiUpstreamMap.SourceReviewedExtensionCount -eq 4 -and $Record.ApiAbiBaseline.HighGuiUpstreamMap.SelectedFamilyCount -eq 3 -and $Record.ApiAbiBaseline.HighGuiUpstreamMap.SelectedDeclarationCount -eq 20 -and $Record.ApiAbiBaseline.HighGuiUpstreamMap.ManagedPublicTypeAdditionCount -eq 0 -and $Record.ApiAbiBaseline.HighGuiUpstreamMap.ManagedPublicMemberAdditionCount -eq 6 -and $Record.ApiAbiBaseline.HighGuiUpstreamMap.NativeEntrypointAdditionCount -eq 10 -and -not [bool]$Record.ApiAbiBaseline.HighGuiUpstreamMap.RepositoryWideParityClaimed) -Issue "Final closeout HighGui upstream-map evidence drifted"
    Assert-True -List $List -Condition ($Record.ApiAbiBaseline.ObjDetectUpstreamMap.Sha256 -eq "7fa633737f1967bbb38f6d31c632a41f0b136f3d06f1e0f81bf984f5349097e7" -and $Record.ApiAbiBaseline.ObjDetectUpstreamMap.RawSha256 -eq "e61cc23d241ae1582e45b085ffaabf066f1996d11ee188fcda6180820cb147e0" -and $Record.ApiAbiBaseline.ObjDetectUpstreamMap.DeclarationCount -eq 195 -and $Record.ApiAbiBaseline.ObjDetectUpstreamMap.CallableCount -eq 163 -and $Record.ApiAbiBaseline.ObjDetectUpstreamMap.ImplementedCount -eq 153 -and $Record.ApiAbiBaseline.ObjDetectUpstreamMap.MissingCount -eq 0 -and $Record.ApiAbiBaseline.ObjDetectUpstreamMap.IntentionallyOmittedCount -eq 10 -and $Record.ApiAbiBaseline.ObjDetectUpstreamMap.UnsupportedCount -eq 0 -and $Record.ApiAbiBaseline.ObjDetectUpstreamMap.UpstreamConditionalCount -eq 0 -and $Record.ApiAbiBaseline.ObjDetectUpstreamMap.CompatibilityHeaderCount -eq 2 -and $Record.ApiAbiBaseline.ObjDetectUpstreamMap.SourceHeaderCount -eq 9 -and $Record.ApiAbiBaseline.ObjDetectUpstreamMap.SelectedFamilyCount -eq 1 -and $Record.ApiAbiBaseline.ObjDetectUpstreamMap.SelectedDeclarationCount -eq 33 -and $Record.ApiAbiBaseline.ObjDetectUpstreamMap.ManagedPublicTypeAdditionCount -eq 4 -and $Record.ApiAbiBaseline.ObjDetectUpstreamMap.ManagedPublicMemberAdditionCount -eq 55 -and $Record.ApiAbiBaseline.ObjDetectUpstreamMap.NativeEntrypointAdditionCount -eq 35 -and -not [bool]$Record.ApiAbiBaseline.ObjDetectUpstreamMap.RepositoryWideParityClaimed) -Issue "Final closeout ObjDetect upstream-map evidence drifted"
    Assert-True -List $List -Condition ($Record.ApiAbiBaseline.PhotoUpstreamMap.Sha256 -eq "34a940bdd523e26d508d6fd20896f5347e3c39d21ccd97d9c5f5909f55be8428" -and $Record.ApiAbiBaseline.PhotoUpstreamMap.RawSha256 -eq "ac887c44492de0fdd533555c11c043c418ce02698b2332ac0e93dda3d35f239a" -and $Record.ApiAbiBaseline.PhotoUpstreamMap.DeclarationCount -eq 145 -and $Record.ApiAbiBaseline.PhotoUpstreamMap.CallableCount -eq 120 -and $Record.ApiAbiBaseline.PhotoUpstreamMap.ImplementedCount -eq 120 -and $Record.ApiAbiBaseline.PhotoUpstreamMap.MissingCount -eq 0 -and $Record.ApiAbiBaseline.PhotoUpstreamMap.IntentionallyOmittedCount -eq 0 -and $Record.ApiAbiBaseline.PhotoUpstreamMap.UnsupportedCount -eq 0 -and $Record.ApiAbiBaseline.PhotoUpstreamMap.UpstreamConditionalCount -eq 0 -and $Record.ApiAbiBaseline.PhotoUpstreamMap.CompatibilityHeaderCount -eq 2 -and $Record.ApiAbiBaseline.PhotoUpstreamMap.ExcludedPublicHeaderCount -eq 1 -and $Record.ApiAbiBaseline.PhotoUpstreamMap.SourceHeaderCount -eq 3 -and $Record.ApiAbiBaseline.PhotoUpstreamMap.SelectedFamilyCount -eq 3 -and $Record.ApiAbiBaseline.PhotoUpstreamMap.SelectedDeclarationCount -eq 83 -and $Record.ApiAbiBaseline.PhotoUpstreamMap.ManagedPublicTypeAdditionCount -eq 18 -and $Record.ApiAbiBaseline.PhotoUpstreamMap.ManagedPublicMemberAdditionCount -eq 181 -and $Record.ApiAbiBaseline.PhotoUpstreamMap.NativeEntrypointAdditionCount -eq 80 -and -not [bool]$Record.ApiAbiBaseline.PhotoUpstreamMap.RepositoryWideParityClaimed) -Issue "Final closeout Photo upstream-map evidence drifted"
    Assert-True -List $List -Condition ($Record.ApiAbiBaseline.MlUpstreamMap.Sha256 -eq "e446feada2420463a08fd02934f9610ba6773ad757c131915000e3d8021fb2ee" -and $Record.ApiAbiBaseline.MlUpstreamMap.RawSha256 -eq "b7c850093e07b04739bff28a81b0248d3a646933f1d51bf6cc25fcab47ab1001" -and $Record.ApiAbiBaseline.MlUpstreamMap.DeclarationCount -eq 241 -and $Record.ApiAbiBaseline.MlUpstreamMap.CallableCount -eq 208 -and $Record.ApiAbiBaseline.MlUpstreamMap.ImplementedCount -eq 208 -and $Record.ApiAbiBaseline.MlUpstreamMap.MissingCount -eq 0 -and $Record.ApiAbiBaseline.MlUpstreamMap.IntentionallyOmittedCount -eq 0 -and $Record.ApiAbiBaseline.MlUpstreamMap.SourceHeaderCount -eq 1 -and $Record.ApiAbiBaseline.MlUpstreamMap.CompatibilityHeaderCount -eq 1 -and $Record.ApiAbiBaseline.MlUpstreamMap.ExcludedPublicHeaderCount -eq 1 -and $Record.ApiAbiBaseline.MlUpstreamMap.SourceReviewedExtensionCount -eq 2 -and $Record.ApiAbiBaseline.MlUpstreamMap.SelectedFamilyCount -eq 6 -and $Record.ApiAbiBaseline.MlUpstreamMap.SelectedDeclarationCount -eq 121 -and $Record.ApiAbiBaseline.MlUpstreamMap.ManagedPublicTypeAdditionCount -eq 18 -and $Record.ApiAbiBaseline.MlUpstreamMap.ManagedPublicMemberAdditionCount -eq 147 -and $Record.ApiAbiBaseline.MlUpstreamMap.NativeEntrypointAdditionCount -eq 75 -and -not [bool]$Record.ApiAbiBaseline.MlUpstreamMap.RepositoryWideParityClaimed) -Issue "Final closeout ML upstream-map evidence drifted"
    Assert-True -List $List -Condition ($Record.ApiAbiBaseline.TrackingUpstreamMap.Sha256 -eq "798f07fee6f6648a2c1faf74004b92e740f50c552685b48f97f73432c983a0b5" -and $Record.ApiAbiBaseline.TrackingUpstreamMap.RawSha256 -eq "b4eb3792e5b20d98922568555557f3c77b8a10cee06a6a1cf1d46d628728a899" -and $Record.ApiAbiBaseline.TrackingUpstreamMap.DeclarationCount -eq 35 -and $Record.ApiAbiBaseline.TrackingUpstreamMap.CallableCount -eq 21 -and $Record.ApiAbiBaseline.TrackingUpstreamMap.ImplementedCount -eq 21 -and $Record.ApiAbiBaseline.TrackingUpstreamMap.MissingCount -eq 0 -and $Record.ApiAbiBaseline.TrackingUpstreamMap.IntentionallyOmittedCount -eq 0 -and $Record.ApiAbiBaseline.TrackingUpstreamMap.PrimaryDeclarationCount -eq 10 -and $Record.ApiAbiBaseline.TrackingUpstreamMap.PrimaryCallableCount -eq 5 -and $Record.ApiAbiBaseline.TrackingUpstreamMap.LegacyDeclarationCount -eq 25 -and $Record.ApiAbiBaseline.TrackingUpstreamMap.LegacyCallableCount -eq 16 -and $Record.ApiAbiBaseline.TrackingUpstreamMap.SourceHeaderCount -eq 2 -and $Record.ApiAbiBaseline.TrackingUpstreamMap.CompatibilityHeaderCount -eq 2 -and $Record.ApiAbiBaseline.TrackingUpstreamMap.ExcludedPublicHeaderCount -eq 8 -and $Record.ApiAbiBaseline.TrackingUpstreamMap.SourceReviewedExtensionCount -eq 4 -and $Record.ApiAbiBaseline.TrackingUpstreamMap.SelectedFamilyCount -eq 1 -and $Record.ApiAbiBaseline.TrackingUpstreamMap.SelectedDeclarationCount -eq 6 -and $Record.ApiAbiBaseline.TrackingUpstreamMap.ManagedPublicTypeAdditionCount -eq 5 -and $Record.ApiAbiBaseline.TrackingUpstreamMap.ManagedPublicMemberAdditionCount -eq 23 -and $Record.ApiAbiBaseline.TrackingUpstreamMap.NativeEntrypointAdditionCount -eq 10 -and -not [bool]$Record.ApiAbiBaseline.TrackingUpstreamMap.MainVideoRowsDoubleCounted -and -not [bool]$Record.ApiAbiBaseline.TrackingUpstreamMap.LegacyRowsMixedIntoPrimary -and -not [bool]$Record.ApiAbiBaseline.TrackingUpstreamMap.RepositoryWideParityClaimed) -Issue "Final closeout Tracking upstream-map evidence drifted"
    $stitchingEvidenceMatches =
        $Record.ApiAbiBaseline.StitchingUpstreamMap.Sha256 -eq "dd89f9bac6104644aa040d7c7dbc77597f8b28597441487dbef02edd1b6b71f0" -and
        $Record.ApiAbiBaseline.StitchingUpstreamMap.RawSha256 -eq "e712d4faed827e0c3e35ed584cde22fe11c60a9ca7589c3d87f6b840321399ad" -and
        $Record.ApiAbiBaseline.StitchingUpstreamMap.DeclarationCount -eq 207 -and
        $Record.ApiAbiBaseline.StitchingUpstreamMap.CallableCount -eq 158 -and
        $Record.ApiAbiBaseline.StitchingUpstreamMap.ImplementedCount -eq 156 -and
        $Record.ApiAbiBaseline.StitchingUpstreamMap.MissingCount -eq 0 -and
        $Record.ApiAbiBaseline.StitchingUpstreamMap.IntentionallyOmittedCount -eq 0 -and
        $Record.ApiAbiBaseline.StitchingUpstreamMap.UnsupportedCount -eq 2 -and
        $Record.ApiAbiBaseline.StitchingUpstreamMap.UpstreamConditionalCount -eq 0 -and
        $Record.ApiAbiBaseline.StitchingUpstreamMap.HighLevelDeclarationCount -eq 24 -and
        $Record.ApiAbiBaseline.StitchingUpstreamMap.HighLevelCallableCount -eq 21 -and
        $Record.ApiAbiBaseline.StitchingUpstreamMap.HighLevelImplementedCount -eq 21 -and
        $Record.ApiAbiBaseline.StitchingUpstreamMap.PublicWarperDeclarationCount -eq 12 -and
        $Record.ApiAbiBaseline.StitchingUpstreamMap.PublicWarperCallableCount -eq 10 -and
        $Record.ApiAbiBaseline.StitchingUpstreamMap.PublicWarperImplementedCount -eq 10 -and
        $Record.ApiAbiBaseline.StitchingUpstreamMap.BlenderDeclarationCount -eq 28 -and
        $Record.ApiAbiBaseline.StitchingUpstreamMap.BlenderCallableCount -eq 24 -and
        $Record.ApiAbiBaseline.StitchingUpstreamMap.BlenderImplementedCount -eq 24 -and
        $Record.ApiAbiBaseline.StitchingUpstreamMap.ExposureDeclarationCount -eq 53 -and
        $Record.ApiAbiBaseline.StitchingUpstreamMap.ExposureCallableCount -eq 45 -and
        $Record.ApiAbiBaseline.StitchingUpstreamMap.ExposureImplementedCount -eq 45 -and
        $Record.ApiAbiBaseline.StitchingUpstreamMap.MatcherDeclarationCount -eq 23 -and
        $Record.ApiAbiBaseline.StitchingUpstreamMap.MatcherCallableCount -eq 16 -and
        $Record.ApiAbiBaseline.StitchingUpstreamMap.MatcherImplementedCount -eq 14 -and
        $Record.ApiAbiBaseline.StitchingUpstreamMap.SeamFinderDeclarationCount -eq 18 -and
        $Record.ApiAbiBaseline.StitchingUpstreamMap.SeamFinderCallableCount -eq 9 -and
        $Record.ApiAbiBaseline.StitchingUpstreamMap.SeamFinderImplementedCount -eq 9 -and
        $Record.ApiAbiBaseline.StitchingUpstreamMap.TimelapserDeclarationCount -eq 7 -and
        $Record.ApiAbiBaseline.StitchingUpstreamMap.TimelapserCallableCount -eq 4 -and
        $Record.ApiAbiBaseline.StitchingUpstreamMap.TimelapserImplementedCount -eq 4 -and
        $Record.ApiAbiBaseline.StitchingUpstreamMap.UtilityDeclarationCount -eq 7 -and
        $Record.ApiAbiBaseline.StitchingUpstreamMap.UtilityCallableCount -eq 7 -and
        $Record.ApiAbiBaseline.StitchingUpstreamMap.UtilityImplementedCount -eq 7 -and
        $Record.ApiAbiBaseline.StitchingUpstreamMap.DetailWarperDeclarationCount -eq 4 -and
        $Record.ApiAbiBaseline.StitchingUpstreamMap.DetailWarperCallableCount -eq 2 -and
        $Record.ApiAbiBaseline.StitchingUpstreamMap.DetailWarperImplementedCount -eq 2 -and
        $Record.ApiAbiBaseline.StitchingUpstreamMap.SourceHeaderCount -eq 14 -and
        $Record.ApiAbiBaseline.StitchingUpstreamMap.CompatibilityHeaderCount -eq 14 -and
        $Record.ApiAbiBaseline.StitchingUpstreamMap.SourceReviewedExtensionCount -eq 4 -and
        $Record.ApiAbiBaseline.StitchingUpstreamMap.SelectedFamilyCount -eq 9 -and
        $Record.ApiAbiBaseline.StitchingUpstreamMap.SelectedDeclarationCount -eq 146 -and
        $Record.ApiAbiBaseline.StitchingUpstreamMap.ManagedPublicTypeAdditionCount -eq 14 -and
        $Record.ApiAbiBaseline.StitchingUpstreamMap.ManagedPublicMemberAdditionCount -eq 38 -and
        $Record.ApiAbiBaseline.StitchingUpstreamMap.NativeEntrypointAdditionCount -eq 22 -and
        -not [bool]$Record.ApiAbiBaseline.StitchingUpstreamMap.UMatExecutionClaimed -and
        -not [bool]$Record.ApiAbiBaseline.StitchingUpstreamMap.DetailRowsMixedIntoHighLevel -and
        -not [bool]$Record.ApiAbiBaseline.StitchingUpstreamMap.RepositoryWideParityClaimed
    Assert-True -List $List -Condition $stitchingEvidenceMatches -Issue "Final closeout Stitching upstream-map evidence drifted"
    Assert-True -List $List -Condition ($Record.ApiAbiBaseline.VideoUpstreamMap.Sha256 -eq "3b0c00ec0f05ed3cf27b28f56a7679abf2f351d384d0c88910a03bb39d8627f0" -and $Record.ApiAbiBaseline.VideoUpstreamMap.RawSha256 -eq "67921346dfbe6f8f2d6a1c850cb88b34bf6eb46a89c4aa6c788ff893f74596af" -and $Record.ApiAbiBaseline.VideoUpstreamMap.DeclarationCount -eq 168 -and $Record.ApiAbiBaseline.VideoUpstreamMap.CallableCount -eq 145 -and $Record.ApiAbiBaseline.VideoUpstreamMap.ImplementedCount -eq 138 -and $Record.ApiAbiBaseline.VideoUpstreamMap.MissingCount -eq 0 -and $Record.ApiAbiBaseline.VideoUpstreamMap.IntentionallyOmittedCount -eq 7 -and $Record.ApiAbiBaseline.VideoUpstreamMap.UnsupportedCount -eq 0 -and $Record.ApiAbiBaseline.VideoUpstreamMap.UpstreamConditionalCount -eq 0 -and $Record.ApiAbiBaseline.VideoUpstreamMap.CompatibilityHeaderCount -eq 2 -and $Record.ApiAbiBaseline.VideoUpstreamMap.ExcludedPublicHeaderCount -eq 2 -and $Record.ApiAbiBaseline.VideoUpstreamMap.SourceHeaderCount -eq 2 -and $Record.ApiAbiBaseline.VideoUpstreamMap.SelectedFamilyCount -eq 3 -and $Record.ApiAbiBaseline.VideoUpstreamMap.SelectedDeclarationCount -eq 83 -and $Record.ApiAbiBaseline.VideoUpstreamMap.ManagedPublicTypeAdditionCount -eq 13 -and $Record.ApiAbiBaseline.VideoUpstreamMap.ManagedPublicMemberAdditionCount -eq 110 -and $Record.ApiAbiBaseline.VideoUpstreamMap.NativeEntrypointAdditionCount -eq 45 -and -not [bool]$Record.ApiAbiBaseline.VideoUpstreamMap.RepositoryWideParityClaimed) -Issue "Final closeout Video upstream-map evidence drifted"
    Assert-True -List $List -Condition ($Record.ApiAbiBaseline.GapInventoryStatus -eq "baseline-established-native-managed-parity-measured" -and -not [bool]$Record.ApiAbiBaseline.UpstreamParityMeasured -and [bool]$Record.ApiAbiBaseline.NativeToManagedParityMeasured -and $Record.ApiAbiBaseline.ImplementedSpanFamilyStatus -eq "implemented-verified") -Issue "Final closeout measured parity distinction or Span family status drifted"

    $expectedEvidence = @(Get-ExpectedEvidencePaths)
    $actualEvidence = @($Record.EvidenceReferences | ForEach-Object { [string]$_.Path })
    $sortedEvidence = @(Get-OrdinalSorted -Values $actualEvidence)
    Assert-True -List $List -Condition (($actualEvidence -join "`n") -ceq ($sortedEvidence -join "`n") -and ($actualEvidence -join "`n") -eq ($expectedEvidence -join "`n")) -Issue "Final closeout evidence references are missing, reordered, or unexpected"
    Assert-True -List $List -Condition ($actualEvidence.Count -eq @($actualEvidence | Sort-Object -Unique).Count) -Issue "Final closeout evidence references must be unique"
    foreach ($evidence in $Record.EvidenceReferences) {
        $path = Join-Path $repo ($evidence.Path -replace '/', [IO.Path]::DirectorySeparatorChar)
        Assert-True -List $List -Condition (Test-Path -LiteralPath $path -PathType Leaf) -Issue "Final closeout evidence file is missing" -Text $evidence.Path
        if (Test-Path -LiteralPath $path -PathType Leaf) {
            $logicalEvidence = Get-LogicalFileEvidence -Path $path
            Assert-True -List $List -Condition ($evidence.Sha256 -eq $logicalEvidence.Sha256 -and [int]$evidence.Length -eq $logicalEvidence.Length) -Issue "Final closeout evidence hash or length drifted" -Text $evidence.Path
        }
    }

    $expectedChecks = @("actionlint-1.7.12", "api-abi-baseline", "docfx-2.78.5", "git-diff-check", "repository-powershell-ast", "workflow-bash-syntax", "workflow-powershell-syntax")
    Assert-True -List $List -Condition ($Record.LocalValidation.Status -eq "locally-validated" -and $Record.LocalValidation.InvariantGuardCount -eq 76 -and $Record.LocalValidation.ExactSdk -eq "10.0.302" -and -not [bool]$Record.LocalValidation.PublicationAllowed -and (@($Record.LocalValidation.RequiredChecks) -join ",") -eq ($expectedChecks -join ",")) -Issue "Final closeout local validation state or check list drifted"
    Assert-True -List $List -Condition ($Record.Signing.Status -eq "repository-signing-pending" -and $Record.Signing.Strategy -eq "nuget.org-repository-signing" -and $Record.Signing.NormalizedInputRequired -and -not [bool]$Record.Signing.AuthorCertificateRequired -and -not [bool]$Record.Signing.PrivateKeyRequired -and -not [bool]$Record.Signing.PrivateKeyMaterialPresent -and $Record.Signing.ServiceIndex -eq "https://api.nuget.org/v3/index.json" -and $Record.Signing.ExpectedSignatureType -eq "Repository" -and $Record.Signing.ExpectedOwner -eq "GuojinYan" -and $Record.Signing.VerificationScript -eq "scripts/Test-NuGetRepositorySignedPackage.ps1" -and $Record.Signing.Verification -eq "post-publication-required") -Issue "Final closeout repository-signing state drifted"
    Assert-True -List $List -Condition ($Record.Sbom.Status -eq "not-ready" -and $Record.Sbom.Format -eq "SPDX-2.3" -and $Record.Sbom.Generator -eq "scripts/New-ReleasePackageSbom.ps1" -and $Record.Sbom.Guard -eq "scripts/Test-ReleasePackageSbom.ps1" -and [bool]$Record.Sbom.Deterministic -and -not [bool]$Record.Sbom.FinalCandidateDocumentGenerated -and $Record.Sbom.Verification -eq "generator-verified-final-candidate-not-generated") -Issue "Final closeout SBOM state must retain a verified generator without claiming final-candidate output"
    Assert-True -List $List -Condition ($Record.Approval.Status -eq "not-approved" -and $Record.Approval.Reviewer -eq "automated-local-preflight" -and $Record.Approval.Approver -eq "unassigned" -and $Record.Approval.EvidenceKind -eq "local-source-and-offline-fixture" -and -not [bool]$Record.Approval.RemoteMutationAllowed) -Issue "Final closeout approval state drifted"

    $expectedMethods = @("GET", "HEAD")
    Assert-True -List $List -Condition ($Record.PublicFeed.Mode -eq "read-only" -and $Record.PublicFeed.ServiceIndex -eq "https://api.nuget.org/v3/index.json" -and $Record.PublicFeed.GitHubPackagesServiceIndex -eq "https://nuget.pkg.github.com/guojin-yan/index.json" -and $Record.PublicFeed.GitHubPackagesRepository -eq "guojin-yan/OpenCV-CSharp-API" -and $Record.PublicFeed.RequiredPublicVisibility -eq "public" -and [int]$Record.PublicFeed.RequiredFeedCount -eq 2) -Issue "Final closeout dual public-feed boundary drifted"
    Assert-True -List $List -Condition ($Record.PublicFeed.CandidatePackage -eq "https://api.nuget.org/v3-flatcontainer/jyppx.opencv.csharp.api/5.0.0-preview.1/jyppx.opencv.csharp.api.5.0.0-preview.1.nupkg") -Issue "Final closeout public-feed candidate URL drifted" -Text $Record.PublicFeed.CandidatePackage
    Assert-True -List $List -Condition ((@($Record.PublicFeed.Methods) -join ",") -eq ($expectedMethods -join ",")) -Issue "Final closeout public-feed method list drifted"
    Assert-True -List $List -Condition (-not [bool]$Record.PublicFeed.Mutable -and $Record.PublicFeed.CandidateStatus -eq "not-published" -and -not [bool]$Record.PublicFeed.UploadAttempted) -Issue "Final closeout public-feed state is not immutable and not-published"
    Assert-True -List $List -Condition ($Record.Rollback.Status -eq "not-published" -and -not [bool]$Record.Rollback.PackageRemovalRequired -and $Record.ReleaseApproval.Status -eq "not-approved" -and -not [bool]$Record.ReleaseApproval.ReleaseReady -and $Record.ReleaseApproval.PublicationDecision -eq "do-not-publish") -Issue "Final closeout rollback or publication decision drifted"
    Assert-True -List $List -Condition (-not [bool]$Record.PrivateKeyMaterialPresent -and -not [bool]$Record.SecretMaterialPresent -and [bool]$Record.Deterministic) -Issue "Final closeout must exclude secrets and remain deterministic"

    $expectedBlockers = @("android-arm-device-evidence","api-gap-implementation","hosted-win-x86-full","macos-support-decision","nuget-production-environment","publication-authorization","release-approval","repository-signing-verification","sbom-inputs")
    $actualBlockers = @($Record.ExternalBlockers | ForEach-Object { [string]$_.Id })
    Assert-True -List $List -Condition (($actualBlockers -join ",") -eq ($expectedBlockers -join ",")) -Issue "Final closeout blocker ledger is missing, reordered, or incomplete"
    foreach ($blocker in $Record.ExternalBlockers) {
        Assert-True -List $List -Condition (-not [string]::IsNullOrWhiteSpace([string]$blocker.Status) -and -not [string]::IsNullOrWhiteSpace([string]$blocker.Evidence)) -Issue "Final closeout blocker lacks status/evidence" -Text $blocker.Id
    }
    $androidBlocker = @($Record.ExternalBlockers | Where-Object { $_.Id -eq "android-arm-device-evidence" })
    Assert-True -List $List -Condition ($androidBlocker.Count -eq 1 -and $androidBlocker[0].Status -eq "android-evidence-pending" -and $androidBlocker[0].Evidence -match "x64/x86 Full and Mini" -and $androidBlocker[0].Evidence -match "single-loader emulator loading evidence" -and $androidBlocker[0].Evidence -match "ARM/ARM64" -and $androidBlocker[0].Evidence -match "device loading evidence") -Issue "Final closeout Android blocker must distinguish verified x64/x86 emulators from pending ARM device evidence"
}

function Assert-FixtureRejected {
    param(
        [Parameter(Mandatory)][string]$Name,
        [Parameter(Mandatory)][scriptblock]$Action,
        [Parameter(Mandatory)][string]$ExpectedIssue
    )
    $fixtureViolations = [System.Collections.Generic.List[object]]::new()
    & $Action $fixtureViolations
    if ($fixtureViolations.Count -eq 0) { throw "Negative final closeout fixture was accepted: $Name" }
    if (-not @($fixtureViolations | Where-Object { $_.Issue -like "*$ExpectedIssue*" })) {
        throw "Negative final closeout fixture '$Name' failed for the wrong reason: $($fixtureViolations.Issue -join '; ')"
    }
}

if (-not (Test-Path -LiteralPath $recordPath -PathType Leaf)) {
    throw "Final release closeout record was not found: $recordPath"
}
& (Join-Path $repo "scripts/New-ReleaseCandidateFinalCloseout.ps1") -RepositoryRoot $repo -Check
if (-not $?) { throw "Final release closeout record freshness check failed." }

$recordText = [IO.File]::ReadAllText($recordPath)
$record = $recordText | ConvertFrom-Json
$expectedSourceHash = [string]$record.SourceSet.Sha256
Test-Record -Record $record -List $violations -ExpectedSourceHash $expectedSourceHash

Assert-FixtureRejected -Name "missing evidence" -ExpectedIssue "evidence references" -Action {
    param($list)
    $fixture = $record | ConvertTo-Json -Depth 30 | ConvertFrom-Json
    $fixture.EvidenceReferences = @($fixture.EvidenceReferences | Select-Object -Skip 1)
    Test-Record -Record $fixture -List $list -ExpectedSourceHash $expectedSourceHash
}
Assert-FixtureRejected -Name "source hash drift" -ExpectedIssue "source-set digest" -Action {
    param($list)
    $fixture = $record | ConvertTo-Json -Depth 30 | ConvertFrom-Json
    $fixture.SourceSet.Sha256 = "0" * 64
    Test-Record -Record $fixture -List $list -ExpectedSourceHash $expectedSourceHash
}
Assert-FixtureRejected -Name "source identity drift" -ExpectedIssue "candidate identity" -Action {
    param($list)
    $fixture = $record | ConvertTo-Json -Depth 30 | ConvertFrom-Json
    $fixture.CandidateId = "local-closeout/sha256/$('0' * 16)"
    Test-Record -Record $fixture -List $list -ExpectedSourceHash $expectedSourceHash
}
Assert-FixtureRejected -Name "support count drift" -ExpectedIssue "support partition" -Action {
    param($list)
    $fixture = $record | ConvertTo-Json -Depth 30 | ConvertFrom-Json
    $fixture.SupportContract.RealSupportCount = 29
    Test-Record -Record $fixture -List $list -ExpectedSourceHash $expectedSourceHash
}
Assert-FixtureRejected -Name "false hosted promotion" -ExpectedIssue "support partition" -Action {
    param($list)
    $fixture = $record | ConvertTo-Json -Depth 30 | ConvertFrom-Json
    $fixture.SupportContract.WinX86FullStatus = "real-supported"
    Test-Record -Record $fixture -List $list -ExpectedSourceHash $expectedSourceHash
}
Assert-FixtureRejected -Name "false signing readiness" -ExpectedIssue "signing state" -Action {
    param($list)
    $fixture = $record | ConvertTo-Json -Depth 30 | ConvertFrom-Json
    $fixture.Signing.Status = "verified"
    Test-Record -Record $fixture -List $list -ExpectedSourceHash $expectedSourceHash
}
Assert-FixtureRejected -Name "premature approval/publication" -ExpectedIssue "approval state" -Action {
    param($list)
    $fixture = $record | ConvertTo-Json -Depth 30 | ConvertFrom-Json
    $fixture.Approval.Status = "approved"
    $fixture.Approval.Approver = "external-reviewer"
    Test-Record -Record $fixture -List $list -ExpectedSourceHash $expectedSourceHash
}
Assert-FixtureRejected -Name "mutable public reference" -ExpectedIssue "public-feed state" -Action {
    param($list)
    $fixture = $record | ConvertTo-Json -Depth 30 | ConvertFrom-Json
    $fixture.PublicFeed.Mutable = $true
    $fixture.PublicFeed.CandidatePackage = "https://api.nuget.org/v3-flatcontainer/jyppx.opencv.csharp.api/latest/package.nupkg"
    Test-Record -Record $fixture -List $list -ExpectedSourceHash $expectedSourceHash
}
Assert-FixtureRejected -Name "nondeterministic evidence ordering" -ExpectedIssue "evidence references" -Action {
    param($list)
    $fixture = $record | ConvertTo-Json -Depth 30 | ConvertFrom-Json
    $first = $fixture.EvidenceReferences[0]
    $fixture.EvidenceReferences[0] = $fixture.EvidenceReferences[1]
    $fixture.EvidenceReferences[1] = $first
    Test-Record -Record $fixture -List $list -ExpectedSourceHash $expectedSourceHash
}

if ($violations.Count -gt 0) {
    Write-Host "Final release closeout guard failed with $($violations.Count) violation(s)."
    $violations | Sort-Object Issue, Text | Format-Table Path, Issue, Text -AutoSize
    exit 1
}

Write-Host "RELEASE_CANDIDATE_FINAL_CLOSEOUT_OK candidate=$($record.CandidateId) blockers=$(@($record.ExternalBlockers).Count) evidence=$(@($record.EvidenceReferences).Count) source_files=$($record.SourceSet.FileCount)"
Write-Host "Final release closeout guard passed."
Write-Host "Negative fixtures rejected: missing evidence, source hash drift, source identity drift, support-count drift, false hosted promotion, false signing readiness, premature approval/publication, mutable public reference, nondeterministic ordering."

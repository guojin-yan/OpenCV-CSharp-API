[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path,
    [string]$OutputPath = "",
    [switch]$Check
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$canonicalRecordRelativePath = "packaging/release/local-release-candidate-closeout.json"
if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $repo ($canonicalRecordRelativePath -replace '/', [IO.Path]::DirectorySeparatorChar)
}
$OutputPath = [IO.Path]::GetFullPath($OutputPath)

$textExtensions = @(
    ".c", ".cc", ".cpp", ".cs", ".cmake", ".csproj", ".h", ".hpp", ".json", ".md", ".props", ".ps1", ".slnx", ".targets", ".txt", ".yml", ".yaml", ".xml"
)
$textFileNames = @("CMakeLists.txt", ".gitignore", "global.json")

function Normalize-Text {
    param([Parameter(Mandatory)][AllowEmptyString()][string]$Text)

    return (($Text -replace "`r`n", "`n") -replace "`r", "`n").TrimEnd() + "`n"
}

function Get-Sha256Bytes {
    param([Parameter(Mandatory)][byte[]]$Bytes)

    return [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($Bytes)).ToLowerInvariant()
}

function Get-OrdinalSortedObjects {
    param(
        [Parameter(Mandatory)][AllowEmptyCollection()][object[]]$Values,
        [Parameter(Mandatory)][string]$Property
    )

    $copy = [object[]]$Values.Clone()
    $keys = [string[]]@($copy | ForEach-Object { [string]$_.$Property })
    [Array]::Sort[string, object]($keys, $copy, [StringComparer]::Ordinal)
    return $copy
}

function Get-LogicalFileRecord {
    param([Parameter(Mandatory)][string]$RelativePath)

    $normalizedPath = $RelativePath.Replace("\", "/")
    $absolutePath = Join-Path $repo ($normalizedPath -replace '/', [IO.Path]::DirectorySeparatorChar)
    if (-not (Test-Path -LiteralPath $absolutePath -PathType Leaf)) {
        throw "Source-set file is missing: $normalizedPath"
    }

    $extension = [IO.Path]::GetExtension($absolutePath).ToLowerInvariant()
    $name = [IO.Path]::GetFileName($absolutePath)
    if ($extension -in $textExtensions -or $name -in $textFileNames) {
        $logicalBytes = [Text.UTF8Encoding]::new($false).GetBytes(
            (Normalize-Text ([IO.File]::ReadAllText($absolutePath))))
    }
    else {
        $logicalBytes = [IO.File]::ReadAllBytes($absolutePath)
    }

    [pscustomobject]@{
        Path = $normalizedPath
        Length = $logicalBytes.Length
        Sha256 = Get-Sha256Bytes -Bytes $logicalBytes
    }
}

function Get-SourceSet {
    $paths = @(& git -C $repo ls-files --cached --others --exclude-standard)
    if ($LASTEXITCODE -ne 0) {
        throw "git ls-files failed while creating the release closeout source set."
    }

    $paths = @($paths | ForEach-Object { ([string]$_).Trim() } | Where-Object { $_ })
    $records = [System.Collections.Generic.List[object]]::new()
    foreach ($path in $paths) {
        $normalizedPath = $path.Replace("\", "/")
        if ($normalizedPath -eq $canonicalRecordRelativePath) {
            continue
        }
        $records.Add((Get-LogicalFileRecord -RelativePath $normalizedPath))
    }

    $sorted = @(Get-OrdinalSortedObjects -Values $records.ToArray() -Property "Path")
    $digestLines = @($sorted | ForEach-Object { "$($_.Path)|$($_.Length)|$($_.Sha256)" })
    $digest = Get-Sha256Bytes -Bytes ([Text.UTF8Encoding]::new($false).GetBytes((($digestLines -join "`n") + "`n")))
    [pscustomobject]@{
        FileCount = $sorted.Count
        Sha256 = $digest
        Files = $sorted
    }
}

function Get-FileEvidence {
    param([Parameter(Mandatory)][string]$RelativePath)

    $logicalRecord = Get-LogicalFileRecord -RelativePath $RelativePath
    return [pscustomobject]@{
        Path = $logicalRecord.Path
        Sha256 = $logicalRecord.Sha256
        Length = $logicalRecord.Length
    }
}

function Get-Record {
    $sourceSet = Get-SourceSet
    $support = Get-Content -LiteralPath (Join-Path $repo "packaging/runtime/runtime-support-contract.json") -Raw | ConvertFrom-Json
    $matrixPath = Join-Path $repo "packaging/runtime/runtime-package-matrix.json"
    $matrix = Get-Content -LiteralPath $matrixPath -Raw | ConvertFrom-Json
    $summary = Get-Content -LiteralPath (Join-Path $repo "compatibility/managed-public-api-summary.json") -Raw | ConvertFrom-Json
    $gapInventory = Get-Content -LiteralPath (Join-Path $repo "compatibility/api-gap-inventory.json") -Raw | ConvertFrom-Json
    $bindingSummary = Get-Content -LiteralPath (Join-Path $repo "compatibility/native-managed-binding-summary.json") -Raw | ConvertFrom-Json
    $spanFamily = Get-Content -LiteralPath (Join-Path $repo "compatibility/imgproc-point-set-span-family.json") -Raw | ConvertFrom-Json
    $imgProcSummary = Get-Content -LiteralPath (Join-Path $repo "compatibility/imgproc-upstream-summary.json") -Raw | ConvertFrom-Json
    $imgCodecsSummary = Get-Content -LiteralPath (Join-Path $repo "compatibility/imgcodecs-upstream-summary.json") -Raw | ConvertFrom-Json
    $videoIOSummary = Get-Content -LiteralPath (Join-Path $repo "compatibility/videoio-upstream-summary.json") -Raw | ConvertFrom-Json
    $videoIORegistry = Get-Content -LiteralPath (Join-Path $repo "compatibility/videoio-registry-surface.json") -Raw | ConvertFrom-Json
    $calib3DSummary = Get-Content -LiteralPath (Join-Path $repo "compatibility/calib3d-upstream-summary.json") -Raw | ConvertFrom-Json
    $coreSummary = Get-Content -LiteralPath (Join-Path $repo "compatibility/core-upstream-summary.json") -Raw | ConvertFrom-Json
    $dnnSummary = Get-Content -LiteralPath (Join-Path $repo "compatibility/dnn-upstream-summary.json") -Raw | ConvertFrom-Json
    $featuresSummary = Get-Content -LiteralPath (Join-Path $repo "compatibility/features-upstream-summary.json") -Raw | ConvertFrom-Json
    $highGuiSummary = Get-Content -LiteralPath (Join-Path $repo "compatibility/highgui-upstream-summary.json") -Raw | ConvertFrom-Json
    $objDetectSummary = Get-Content -LiteralPath (Join-Path $repo "compatibility/objdetect-upstream-summary.json") -Raw | ConvertFrom-Json
    $photoSummary = Get-Content -LiteralPath (Join-Path $repo "compatibility/photo-upstream-summary.json") -Raw | ConvertFrom-Json
    $mlSummary = Get-Content -LiteralPath (Join-Path $repo "compatibility/ml-upstream-summary.json") -Raw | ConvertFrom-Json
    $trackingSummary = Get-Content -LiteralPath (Join-Path $repo "compatibility/tracking-upstream-summary.json") -Raw | ConvertFrom-Json
    $stitchingSummary = Get-Content -LiteralPath (Join-Path $repo "compatibility/stitching-upstream-summary.json") -Raw | ConvertFrom-Json
    $videoSummary = Get-Content -LiteralPath (Join-Path $repo "compatibility/video-upstream-summary.json") -Raw | ConvertFrom-Json
    $evidencePaths = @(
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
        "compatibility/tracking-implemented-families.json",
        "compatibility/tracking-upstream-classifications.json",
        "compatibility/tracking-upstream-map.txt",
        "compatibility/tracking-upstream-raw.json",
        "compatibility/tracking-upstream-summary.json",
        "compatibility/stitching-implemented-families.json",
        "compatibility/stitching-upstream-classifications.json",
        "compatibility/stitching-upstream-map.txt",
        "compatibility/stitching-upstream-raw.json",
        "compatibility/stitching-upstream-summary.json",
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
        "docs/articles/tracking-guide.md",
        "docs/articles/stitching-structured-parity-guide.md",
        "docs/articles/objdetect-structured-parity-guide.md",
        "docs/articles/photo-ccm-guide.md",
        "docs/articles/photo-hdr-workflow-guide.md",
        "docs/articles/photo-intelligent-scissors-guide.md",
        "docs/articles/photo-tvl1-chromatic-aberration-guide.md",
        "docs/articles/point-set-marshalling-guide.md",
        "docs/articles/release-candidate-closeout.md",
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
        "docs/articles/video-upstream-parity-guide.md",
        "docs/articles/videoio-upstream-parity-guide.md",
        "nuget/logo.jpg",
        "packaging/runtime/JYPPX.OpenCV.runtime/buildTransitive/JYPPX.OpenCV.runtime.targets",
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
        "scripts/Generate-ImgProcUpstreamMap.ps1",
        "scripts/Generate-ImgCodecsUpstreamMap.ps1",
        "scripts/Generate-ManagedPublicApiBaseline.ps1",
        "scripts/Generate-MlUpstreamMap.ps1",
        "scripts/Generate-TrackingUpstreamMap.ps1",
        "scripts/Generate-StitchingUpstreamMap.ps1",
        "scripts/Generate-NativeAbiManifest.ps1",
        "scripts/Generate-NativeManagedBindingMap.ps1",
        "scripts/Test-NoUnpublishedCompatibilitySurface.ps1",
        "scripts/Generate-ObjDetectUpstreamMap.ps1",
        "scripts/Generate-PhotoUpstreamMap.ps1",
        "scripts/Generate-VideoUpstreamMap.ps1",
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
        "scripts/Test-ImgProcUpstreamMap.ps1",
        "scripts/Test-GitHubPackArtifactMatrixSurface.ps1",
        "scripts/Test-ManagedPackageIsolatedArtifactSurface.ps1",
        "scripts/Test-ManagedPackageStandaloneLocalConsumerCompile.ps1",
        "scripts/Test-MlUpstreamMap.ps1",
        "scripts/Test-TrackingUpstreamMap.ps1",
        "scripts/Test-StitchingUpstreamMap.ps1",
        "scripts/Test-ImgCodecsUpstreamMap.ps1",
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
        "src/OpenCvSharp/Internal/Interop/NativeFeaturesMatcherHandle.cs",
        "src/OpenCvSharp/Internal/Interop/NativeHighGuiCallbackRegistrationHandle.cs",
        "src/OpenCvSharp/Internal/Interop/NativeMethods.HighGui.DllImport.cs",
        "src/OpenCvSharp/Internal/Interop/NativeMethods.HighGui.LibraryImport.cs",
        "src/OpenCvSharp/Internal/Interop/NativeImageFeaturesHandle.cs",
        "src/OpenCvSharp/Internal/Interop/NativeMatchesInfoHandle.cs",
        "src/OpenCvSharp/Internal/Interop/NativeEstimatorHandle.cs",
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
        "tools/ImgProcUpstreamMap/ImgProcUpstreamMap.csproj",
        "tools/ImgProcUpstreamMap/Program.cs",
        "tools/ImgProcUpstreamMap/extract_imgproc.py",
        "tools/MlUpstreamMap/MlUpstreamMap.csproj",
        "tools/MlUpstreamMap/Program.cs",
        "tools/MlUpstreamMap/extract_ml.py",
        "tools/TrackingUpstreamMap/TrackingUpstreamMap.csproj",
        "tools/TrackingUpstreamMap/Program.cs",
        "tools/TrackingUpstreamMap/extract_tracking.py",
        "tools/StitchingUpstreamMap/StitchingUpstreamMap.csproj",
        "tools/StitchingUpstreamMap/Program.cs",
        "tools/StitchingUpstreamMap/extract_stitching.py",
        "tools/ImgCodecsUpstreamMap/ImgCodecsUpstreamMap.csproj",
        "tools/ImgCodecsUpstreamMap/Program.cs",
        "tools/ImgCodecsUpstreamMap/extract_imgcodecs.py",
        "tools/NativeManagedBindingMap/NativeManagedBindingMap.csproj",
        "tools/NativeManagedBindingMap/Program.cs",
        "tools/ObjDetectUpstreamMap/ObjDetectUpstreamMap.csproj",
        "tools/ObjDetectUpstreamMap/Program.cs",
        "tools/ObjDetectUpstreamMap/extract_objdetect.py",
        "tools/PhotoUpstreamMap/PhotoUpstreamMap.csproj",
        "tools/PhotoUpstreamMap/Program.cs",
        "tools/PhotoUpstreamMap/extract_photo.py",
        "tools/VideoUpstreamMap/VideoUpstreamMap.csproj",
        "tools/NuGetRepositorySignatureVerifier/NuGetRepositorySignatureVerifier.csproj",
        "tools/NuGetRepositorySignatureVerifier/Program.cs",
        "tools/VideoUpstreamMap/Program.cs",
        "tools/VideoUpstreamMap/extract_video.py"
    )
    $evidence = @(Get-OrdinalSortedObjects -Values @($evidencePaths | ForEach-Object { Get-FileEvidence -RelativePath $_ }) -Property "Path")

    $blockers = @(
        [ordered]@{ Id = "android-arm-device-evidence"; Status = "android-evidence-pending"; Evidence = "Android x64/x86 Full and Mini have authoritative single-loader emulator loading evidence. Android ARM/ARM64 NDK, ELF, same-run package, and APK evidence has passed; matching device loading evidence remains required before promotion." },
        [ordered]@{ Id = "api-gap-implementation"; Status = "open-local-follow-up"; Evidence = "The structured ImgProc, ImgCodecs, VideoIO, Calib3D, Core, DNN, Features, ObjDetect, main CPU Photo, and main Video slices are closed at zero missing callable declarations. Repository-wide upstream parity and prioritized ownership/marshalling work remain open." },
        [ordered]@{ Id = "hosted-win-x86-full"; Status = "quota-blocked"; Evidence = "Hosted producer, artifact handoff, same-run pack, independent audit, and X86 consumer evidence are absent." },
        [ordered]@{ Id = "macos-support-decision"; Status = "decision-deferred"; Evidence = "macOS is outside the declared matrix until an explicit decision and native/consumer evidence exist." },
        [ordered]@{ Id = "nuget-production-environment"; Status = "not-configured"; Evidence = "The authoritative nuget-production Environment and NUGET_API_KEY secret are not configured. The first preview may use only the workflow's explicit single-maintainer exception instead of a protected reviewer." },
        [ordered]@{ Id = "publication-authorization"; Status = "not-authorized"; Evidence = "No publish, tag, release, or mutable feed operation is authorized in the current quota state." },
        [ordered]@{ Id = "release-approval"; Status = "not-approved"; Evidence = "No exact candidate has been approved by either an independent reviewer or the explicit, version-bounded first-preview single-maintainer exception." },
        [ordered]@{ Id = "repository-signing-verification"; Status = "post-publication-required"; Evidence = "NuGet.org must add a Repository primary signature and pass exact payload comparison; GitHub Packages must be public, repository-linked, and byte-identical to the reviewed candidate." },
        [ordered]@{ Id = "sbom-inputs"; Status = "candidate-refresh-required"; Evidence = "The deterministic SPDX-2.3 generator and guard are provisioned; final package-bound documents must be regenerated from the final source commit and approved." }
    )

    [ordered]@{
        SchemaVersion = 2
        RecordKind = "local-release-candidate-closeout"
        CandidateId = "local-closeout/sha256/$($sourceSet.Sha256.Substring(0, 16))"
        SourceIdentity = "sha256:$($sourceSet.Sha256)"
        OpenCvRevision = "5.0.0"
        SourceSet = [ordered]@{
            FileCount = $sourceSet.FileCount
            Sha256 = $sourceSet.Sha256
            HashPolicy = "UTF-8 text normalized to LF; non-text bytes hashed raw; closeout record excluded"
        }
        PackageMatrix = [ordered]@{
            Path = "packaging/runtime/runtime-package-matrix.json"
            Sha256 = (Get-FileHash -LiteralPath $matrixPath -Algorithm SHA256).Hash.ToLowerInvariant()
            RidCount = @($matrix.rids).Count
            ProfileCount = @($matrix.profiles).Count
            EntryCount = @($matrix.rids).Count * @($matrix.profiles).Count
        }
        SupportContract = [ordered]@{
            Path = "packaging/runtime/runtime-support-contract.json"
            Sha256 = (Get-FileHash -LiteralPath (Join-Path $repo "packaging/runtime/runtime-support-contract.json") -Algorithm SHA256).Hash.ToLowerInvariant()
            MatrixEntryCount = @($support.realSupport).Count + @($support.pending).Count + @($support.excluded).Count
            RealSupportCount = @($support.realSupport).Count
            PendingSupportCount = @($support.pending).Count
            ExcludedSupportCount = @($support.excluded).Count
            OutsideMatrixCount = @($support.outsideMatrix).Count
            WinX86FullStatus = $support.pending[0].status
            WinX86MiniStatus = ($support.excluded | Where-Object { $_.target -eq "win-x86/mini" }).status
            PackageSurfaceDefinesSupport = [bool]$support.policy.packageSurfaceIsSupport
        }
        ApiAbiBaseline = [ordered]@{
            Managed = [ordered]@{
                Path = "compatibility/managed-public-api.txt"
                SummaryPath = "compatibility/managed-public-api-summary.json"
                Sha256 = $summary.baselineSha256
                TypeCount = [int]$summary.typeCount
                MemberCount = [int]$summary.memberCount
                NamespaceCount = [int]$summary.namespaceCount
                TargetFramework = $summary.targetFramework
            }
            NativeFull = [ordered]@{
                Path = "src/OpenCvSharp.Native/generated/native_abi_manifest.txt"
                Sha256 = (Get-FileHash -LiteralPath (Join-Path $repo "src/OpenCvSharp.Native/generated/native_abi_manifest.txt") -Algorithm SHA256).Hash.ToLowerInvariant()
                FunctionCount = 2656
            }
            NativeMini = [ordered]@{
                Path = "src/OpenCvSharp.Native/generated/native_abi_mini_manifest.txt"
                Sha256 = (Get-FileHash -LiteralPath (Join-Path $repo "src/OpenCvSharp.Native/generated/native_abi_mini_manifest.txt") -Algorithm SHA256).Hash.ToLowerInvariant()
                FunctionCount = 526
            }
            NativeManagedBindingMap = [ordered]@{
                Path = "compatibility/native-managed-binding-map.txt"
                SummaryPath = "compatibility/native-managed-binding-summary.json"
                Sha256 = $bindingSummary.mappingSha256
                NativeFunctionCount = [int]$bindingSummary.nativeFunctionCount
                ManagedBoundCount = [int]$bindingSummary.managedBoundCount
                UnboundCount = [int]$bindingSummary.unboundCount
                ManagedOnlyCount = [int]$bindingSummary.managedOnlyCount
            }
            ImgProcUpstreamMap = [ordered]@{
                Path = "compatibility/imgproc-upstream-map.txt"
                SummaryPath = "compatibility/imgproc-upstream-summary.json"
                FamilyInventoryPath = "compatibility/imgproc-implemented-families.json"
                Sha256 = $imgProcSummary.mappingSha256
                DeclarationCount = [int]$imgProcSummary.declarationCount
                CallableCount = [int]$imgProcSummary.callableCount
                ImplementedCount = [int]$imgProcSummary.classificationCounts.implemented
                MissingCount = [int]$imgProcSummary.classificationCounts.missing
                IntentionallyOmittedCount = [int]$imgProcSummary.classificationCounts.'intentionally-omitted'
                SelectedFamilyCount = [int]$imgProcSummary.selectedFamilyCount
                SelectedDeclarationCount = [int]$imgProcSummary.selectedDeclarationCount
                ManagedPublicMemberAdditionCount = [int]$imgProcSummary.managedPublicMemberAdditionCount
                RepositoryWideParityClaimed = [bool]$imgProcSummary.repositoryWideUpstreamParityClaimed
            }
            ImgCodecsUpstreamMap = [ordered]@{
                Path = "compatibility/imgcodecs-upstream-map.txt"
                SummaryPath = "compatibility/imgcodecs-upstream-summary.json"
                FamilyInventoryPath = "compatibility/imgcodecs-implemented-families.json"
                SourceReviewedExtensionsPath = "compatibility/imgcodecs-source-reviewed-extensions.json"
                Sha256 = $imgCodecsSummary.mappingSha256
                DeclarationCount = [int]$imgCodecsSummary.declarationCount
                CallableCount = [int]$imgCodecsSummary.callableCount
                ImplementedCount = [int]$imgCodecsSummary.classificationCounts.implemented
                MissingCount = [int]$imgCodecsSummary.classificationCounts.missing
                IntentionallyOmittedCount = [int]$imgCodecsSummary.classificationCounts.'intentionally-omitted'
                SelectedFamilyCount = [int]$imgCodecsSummary.selectedFamilyCount
                SelectedDeclarationCount = [int]$imgCodecsSummary.selectedDeclarationCount
                ManagedPublicTypeAdditionCount = [int]$imgCodecsSummary.managedPublicTypeAdditionCount
                ManagedPublicMemberAdditionCount = [int]$imgCodecsSummary.managedPublicMemberAdditionCount
                RepositoryWideParityClaimed = [bool]$imgCodecsSummary.repositoryWideUpstreamParityClaimed
            }
            VideoIOUpstreamMap = [ordered]@{
                Path = "compatibility/videoio-upstream-map.txt"
                SummaryPath = "compatibility/videoio-upstream-summary.json"
                FamilyInventoryPath = "compatibility/videoio-implemented-families.json"
                RegistrySurfacePath = "compatibility/videoio-registry-surface.json"
                Sha256 = $videoIOSummary.mappingSha256
                RegistrySurfaceSha256 = (Get-FileHash -LiteralPath (Join-Path $repo "compatibility/videoio-registry-surface.json") -Algorithm SHA256).Hash.ToLowerInvariant()
                DeclarationCount = [int]$videoIOSummary.declarationCount
                CallableCount = [int]$videoIOSummary.callableCount
                ImplementedCount = [int]$videoIOSummary.classificationCounts.implemented
                MissingCount = [int]$videoIOSummary.classificationCounts.missing
                IntentionallyOmittedCount = [int]$videoIOSummary.classificationCounts.'intentionally-omitted'
                RegistryOperationCount = @($videoIORegistry.operations).Count
                RepositoryWideParityClaimed = [bool]$videoIOSummary.repositoryWideUpstreamParityClaimed
            }
            Calib3DUpstreamMap = [ordered]@{
                Path = "compatibility/calib3d-upstream-map.txt"
                SummaryPath = "compatibility/calib3d-upstream-summary.json"
                FamilyInventoryPath = "compatibility/calib3d-implemented-families.json"
                Sha256 = $calib3DSummary.mappingSha256
                DeclarationCount = [int]$calib3DSummary.declarationCount
                CallableCount = [int]$calib3DSummary.callableCount
                ImplementedCount = [int]$calib3DSummary.classificationCounts.implemented
                MissingCount = [int]$calib3DSummary.classificationCounts.missing
                IntentionallyOmittedCount = [int]$calib3DSummary.classificationCounts.'intentionally-omitted'
                SourceHeaderCount = [int]$calib3DSummary.sourceHeaderCount
                SelectedFamilyCount = [int]$calib3DSummary.selectedFamilyCount
                SelectedDeclarationCount = [int]$calib3DSummary.selectedDeclarationCount
                ManagedPublicTypeAdditionCount = [int]$calib3DSummary.managedPublicTypeAdditionCount
                ManagedPublicMemberAdditionCount = [int]$calib3DSummary.managedPublicMemberAdditionCount
                RepositoryWideParityClaimed = [bool]$calib3DSummary.repositoryWideUpstreamParityClaimed
            }
            CoreUpstreamMap = [ordered]@{
                Path = "compatibility/core-upstream-map.txt"
                SummaryPath = "compatibility/core-upstream-summary.json"
                ClassificationPath = "compatibility/core-upstream-classifications.json"
                FamilyInventoryPath = "compatibility/core-implemented-families.json"
                Sha256 = $coreSummary.mappingSha256
                DeclarationCount = [int]$coreSummary.declarationCount
                CallableCount = [int]$coreSummary.callableCount
                ImplementedCount = [int]$coreSummary.classificationCounts.implemented
                MissingCount = [int]$coreSummary.classificationCounts.missing
                IntentionallyOmittedCount = [int]$coreSummary.classificationCounts.'intentionally-omitted'
                UnsupportedCount = [int]$coreSummary.classificationCounts.unsupported
                UpstreamConditionalCount = [int]$coreSummary.classificationCounts.'upstream-conditional'
                SourceHeaderCount = [int]$coreSummary.sourceHeaderCount
                SelectedFamilyCount = [int]$coreSummary.selectedFamilyCount
                SelectedDeclarationCount = [int]$coreSummary.selectedDeclarationCount
                ManagedPublicTypeAdditionCount = [int]$coreSummary.managedPublicTypeAdditionCount
                ManagedPublicMemberAdditionCount = [int]$coreSummary.managedPublicMemberAdditionCount
                RepositoryWideParityClaimed = [bool]$coreSummary.repositoryWideUpstreamParityClaimed
            }
            DnnUpstreamMap = [ordered]@{
                Path = "compatibility/dnn-upstream-map.txt"
                SummaryPath = "compatibility/dnn-upstream-summary.json"
                ClassificationPath = "compatibility/dnn-upstream-classifications.json"
                FamilyInventoryPath = "compatibility/dnn-implemented-families.json"
                Sha256 = $dnnSummary.mappingSha256
                DeclarationCount = [int]$dnnSummary.declarationCount
                CallableCount = [int]$dnnSummary.callableCount
                ImplementedCount = [int]$dnnSummary.classificationCounts.implemented
                MissingCount = [int]$dnnSummary.classificationCounts.missing
                IntentionallyOmittedCount = [int]$dnnSummary.classificationCounts.'intentionally-omitted'
                UnsupportedCount = [int]$dnnSummary.classificationCounts.unsupported
                UpstreamConditionalCount = [int]$dnnSummary.classificationCounts.'upstream-conditional'
                SourceHeaderCount = [int]$dnnSummary.sourceHeaderCount
                SelectedFamilyCount = [int]$dnnSummary.selectedFamilyCount
                SelectedDeclarationCount = [int]$dnnSummary.selectedDeclarationCount
                ManagedPublicTypeAdditionCount = [int]$dnnSummary.managedPublicTypeAdditionCount
                ManagedPublicMemberAdditionCount = [int]$dnnSummary.managedPublicMemberAdditionCount
                RepositoryWideParityClaimed = [bool]$dnnSummary.repositoryWideUpstreamParityClaimed
            }
            FeaturesUpstreamMap = [ordered]@{
                Path = "compatibility/features-upstream-map.txt"
                SummaryPath = "compatibility/features-upstream-summary.json"
                ClassificationPath = "compatibility/features-upstream-classifications.json"
                FamilyInventoryPath = "compatibility/features-implemented-families.json"
                SourceReviewedExtensionsPath = "compatibility/features-source-reviewed-extensions.json"
                Sha256 = $featuresSummary.mappingSha256
                DeclarationCount = [int]$featuresSummary.declarationCount
                CallableCount = [int]$featuresSummary.callableCount
                ImplementedCount = [int]$featuresSummary.classificationCounts.implemented
                MissingCount = [int]$featuresSummary.classificationCounts.missing
                IntentionallyOmittedCount = [int]$featuresSummary.classificationCounts.'intentionally-omitted'
                UnsupportedCount = [int]$featuresSummary.classificationCounts.unsupported
                UpstreamConditionalCount = [int]$featuresSummary.classificationCounts.'upstream-conditional'
                CompatibilityHeaderCount = [int]$featuresSummary.compatibilityHeaderCount
                SourceHeaderCount = [int]$featuresSummary.sourceHeaderCount
                SourceReviewedExtensionCount = [int]$featuresSummary.sourceReviewedExtensionDeclarationCount
                SelectedFamilyCount = [int]$featuresSummary.selectedFamilyCount
                SelectedDeclarationCount = [int]$featuresSummary.selectedDeclarationCount
                ManagedPublicTypeAdditionCount = [int]$featuresSummary.managedPublicTypeAdditionCount
                ManagedPublicMemberAdditionCount = [int]$featuresSummary.managedPublicMemberAdditionCount
                RepositoryWideParityClaimed = [bool]$featuresSummary.repositoryWideUpstreamParityClaimed
            }
            HighGuiUpstreamMap = [ordered]@{
                Path = "compatibility/highgui-upstream-map.txt"
                SummaryPath = "compatibility/highgui-upstream-summary.json"
                ClassificationPath = "compatibility/highgui-upstream-classifications.json"
                FamilyInventoryPath = "compatibility/highgui-implemented-families.json"
                RawExtractionPath = "compatibility/highgui-upstream-raw.json"
                Sha256 = $highGuiSummary.mappingSha256
                RawSha256 = (Get-FileHash -LiteralPath (Join-Path $repo "compatibility/highgui-upstream-raw.json") -Algorithm SHA256).Hash.ToLowerInvariant()
                DeclarationCount = [int]$highGuiSummary.declarationCount
                CallableCount = [int]$highGuiSummary.callableCount
                ImplementedCount = [int]$highGuiSummary.classificationCounts.implemented
                MissingCount = [int]$highGuiSummary.classificationCounts.missing
                IntentionallyOmittedCount = [int]$highGuiSummary.classificationCounts.'intentionally-omitted'
                UnsupportedCount = [int]$highGuiSummary.classificationCounts.unsupported
                UpstreamConditionalCount = [int]$highGuiSummary.classificationCounts.'upstream-conditional'
                CompatibilityHeaderCount = [int]$highGuiSummary.compatibilityHeaderCount
                ExcludedPublicHeaderCount = [int]$highGuiSummary.excludedPublicHeaderCount
                SourceHeaderCount = [int]$highGuiSummary.sourceHeaderCount
                SourceReviewedExtensionCount = [int]$highGuiSummary.sourceReviewedExtensionCount
                SelectedFamilyCount = [int]$highGuiSummary.selectedFamilyCount
                SelectedDeclarationCount = [int]$highGuiSummary.selectedDeclarationCount
                ManagedPublicTypeAdditionCount = [int]$highGuiSummary.managedPublicTypeAdditionCount
                ManagedPublicMemberAdditionCount = [int]$highGuiSummary.managedPublicMemberAdditionCount
                NativeEntrypointAdditionCount = [int]$highGuiSummary.nativeEntrypointAdditionCount
                RepositoryWideParityClaimed = [bool]$highGuiSummary.repositoryWideUpstreamParityClaimed
            }
            ObjDetectUpstreamMap = [ordered]@{
                Path = "compatibility/objdetect-upstream-map.txt"
                SummaryPath = "compatibility/objdetect-upstream-summary.json"
                ClassificationPath = "compatibility/objdetect-upstream-classifications.json"
                FamilyInventoryPath = "compatibility/objdetect-implemented-families.json"
                RawExtractionPath = "compatibility/objdetect-upstream-raw.json"
                Sha256 = $objDetectSummary.mappingSha256
                RawSha256 = (Get-FileHash -LiteralPath (Join-Path $repo "compatibility/objdetect-upstream-raw.json") -Algorithm SHA256).Hash.ToLowerInvariant()
                DeclarationCount = [int]$objDetectSummary.declarationCount
                CallableCount = [int]$objDetectSummary.callableCount
                ImplementedCount = [int]$objDetectSummary.classificationCounts.implemented
                MissingCount = [int]$objDetectSummary.classificationCounts.missing
                IntentionallyOmittedCount = [int]$objDetectSummary.classificationCounts.'intentionally-omitted'
                UnsupportedCount = [int]$objDetectSummary.classificationCounts.unsupported
                UpstreamConditionalCount = [int]$objDetectSummary.classificationCounts.'upstream-conditional'
                CompatibilityHeaderCount = [int]$objDetectSummary.compatibilityHeaderCount
                SourceHeaderCount = [int]$objDetectSummary.sourceHeaderCount
                SelectedFamilyCount = [int]$objDetectSummary.selectedFamilyCount
                SelectedDeclarationCount = [int]$objDetectSummary.selectedDeclarationCount
                ManagedPublicTypeAdditionCount = [int]$objDetectSummary.managedPublicTypeAdditionCount
                ManagedPublicMemberAdditionCount = [int]$objDetectSummary.managedPublicMemberAdditionCount
                NativeEntrypointAdditionCount = [int]$objDetectSummary.nativeEntrypointAdditionCount
                RepositoryWideParityClaimed = [bool]$objDetectSummary.repositoryWideUpstreamParityClaimed
            }
            PhotoUpstreamMap = [ordered]@{
                Path = "compatibility/photo-upstream-map.txt"
                SummaryPath = "compatibility/photo-upstream-summary.json"
                ClassificationPath = "compatibility/photo-upstream-classifications.json"
                FamilyInventoryPath = "compatibility/photo-implemented-families.json"
                RawExtractionPath = "compatibility/photo-upstream-raw.json"
                Sha256 = $photoSummary.mappingSha256
                RawSha256 = (Get-FileHash -LiteralPath (Join-Path $repo "compatibility/photo-upstream-raw.json") -Algorithm SHA256).Hash.ToLowerInvariant()
                DeclarationCount = [int]$photoSummary.declarationCount
                CallableCount = [int]$photoSummary.callableCount
                ImplementedCount = [int]$photoSummary.classificationCounts.implemented
                MissingCount = [int]$photoSummary.classificationCounts.missing
                IntentionallyOmittedCount = [int]$photoSummary.classificationCounts.'intentionally-omitted'
                UnsupportedCount = [int]$photoSummary.classificationCounts.unsupported
                UpstreamConditionalCount = [int]$photoSummary.classificationCounts.'upstream-conditional'
                CompatibilityHeaderCount = [int]$photoSummary.compatibilityHeaderCount
                ExcludedPublicHeaderCount = [int]$photoSummary.excludedPublicHeaderCount
                SourceHeaderCount = [int]$photoSummary.sourceHeaderCount
                SelectedFamilyCount = [int]$photoSummary.selectedFamilyCount
                SelectedDeclarationCount = [int]$photoSummary.selectedDeclarationCount
                ManagedPublicTypeAdditionCount = [int]$photoSummary.managedPublicTypeAdditionCount
                ManagedPublicMemberAdditionCount = [int]$photoSummary.managedPublicMemberAdditionCount
                NativeEntrypointAdditionCount = [int]$photoSummary.nativeEntrypointAdditionCount
                RepositoryWideParityClaimed = [bool]$photoSummary.repositoryWideUpstreamParityClaimed
            }
            MlUpstreamMap = [ordered]@{
                Path = "compatibility/ml-upstream-map.txt"
                SummaryPath = "compatibility/ml-upstream-summary.json"
                ClassificationPath = "compatibility/ml-upstream-classifications.json"
                FamilyInventoryPath = "compatibility/ml-implemented-families.json"
                RawExtractionPath = "compatibility/ml-upstream-raw.json"
                Sha256 = $mlSummary.mappingSha256
                RawSha256 = (Get-FileHash -LiteralPath (Join-Path $repo "compatibility/ml-upstream-raw.json") -Algorithm SHA256).Hash.ToLowerInvariant()
                DeclarationCount = [int]$mlSummary.declarationCount
                CallableCount = [int]$mlSummary.callableCount
                ImplementedCount = [int]$mlSummary.classificationCounts.implemented
                MissingCount = [int]$mlSummary.classificationCounts.missing
                IntentionallyOmittedCount = [int]$mlSummary.classificationCounts.'intentionally-omitted'
                UnsupportedCount = [int]$mlSummary.classificationCounts.unsupported
                UpstreamConditionalCount = [int]$mlSummary.classificationCounts.'upstream-conditional'
                CompatibilityHeaderCount = [int]$mlSummary.compatibilityHeaderCount
                ExcludedPublicHeaderCount = [int]$mlSummary.excludedPublicHeaderCount
                SourceHeaderCount = [int]$mlSummary.sourceHeaderCount
                SourceReviewedExtensionCount = [int]$mlSummary.sourceReviewedExtensionCount
                SelectedFamilyCount = [int]$mlSummary.selectedFamilyCount
                SelectedDeclarationCount = [int]$mlSummary.selectedDeclarationCount
                ManagedPublicTypeAdditionCount = [int]$mlSummary.managedPublicTypeAdditionCount
                ManagedPublicMemberAdditionCount = [int]$mlSummary.managedPublicMemberAdditionCount
                NativeEntrypointAdditionCount = [int]$mlSummary.nativeEntrypointAdditionCount
                RepositoryWideParityClaimed = [bool]$mlSummary.repositoryWideUpstreamParityClaimed
            }
            TrackingUpstreamMap = [ordered]@{
                Path = "compatibility/tracking-upstream-map.txt"
                SummaryPath = "compatibility/tracking-upstream-summary.json"
                ClassificationPath = "compatibility/tracking-upstream-classifications.json"
                FamilyInventoryPath = "compatibility/tracking-implemented-families.json"
                RawExtractionPath = "compatibility/tracking-upstream-raw.json"
                Sha256 = $trackingSummary.mappingSha256
                RawSha256 = (Get-FileHash -LiteralPath (Join-Path $repo "compatibility/tracking-upstream-raw.json") -Algorithm SHA256).Hash.ToLowerInvariant()
                DeclarationCount = [int]$trackingSummary.declarationCount
                CallableCount = [int]$trackingSummary.callableCount
                ImplementedCount = [int]$trackingSummary.classificationCounts.implemented
                MissingCount = [int]$trackingSummary.classificationCounts.missing
                IntentionallyOmittedCount = [int]$trackingSummary.classificationCounts.'intentionally-omitted'
                UnsupportedCount = [int]$trackingSummary.classificationCounts.unsupported
                UpstreamConditionalCount = [int]$trackingSummary.classificationCounts.'upstream-conditional'
                PrimaryDeclarationCount = [int]$trackingSummary.primaryDeclarationCount
                PrimaryCallableCount = [int]$trackingSummary.primaryCallableCount
                LegacyDeclarationCount = [int]$trackingSummary.legacyDeclarationCount
                LegacyCallableCount = [int]$trackingSummary.legacyCallableCount
                CompatibilityHeaderCount = [int]$trackingSummary.compatibilityHeaderCount
                ExcludedPublicHeaderCount = [int]$trackingSummary.excludedPublicHeaderCount
                SourceHeaderCount = [int]$trackingSummary.sourceHeaderCount
                SourceReviewedExtensionCount = [int]$trackingSummary.sourceReviewedExtensionCount
                SelectedFamilyCount = [int]$trackingSummary.selectedFamilyCount
                SelectedDeclarationCount = [int]$trackingSummary.selectedDeclarationCount
                ManagedPublicTypeAdditionCount = [int]$trackingSummary.managedPublicTypeAdditionCount
                ManagedPublicMemberAdditionCount = [int]$trackingSummary.managedPublicMemberAdditionCount
                NativeEntrypointAdditionCount = [int]$trackingSummary.nativeEntrypointAdditionCount
                MainVideoRowsDoubleCounted = [bool]$trackingSummary.mainVideoRowsDoubleCounted
                LegacyRowsMixedIntoPrimary = [bool]$trackingSummary.legacyRowsMixedIntoPrimary
                RepositoryWideParityClaimed = [bool]$trackingSummary.repositoryWideUpstreamParityClaimed
            }
            StitchingUpstreamMap = [ordered]@{
                Path = "compatibility/stitching-upstream-map.txt"
                SummaryPath = "compatibility/stitching-upstream-summary.json"
                ClassificationPath = "compatibility/stitching-upstream-classifications.json"
                FamilyInventoryPath = "compatibility/stitching-implemented-families.json"
                RawExtractionPath = "compatibility/stitching-upstream-raw.json"
                Sha256 = $stitchingSummary.mappingSha256
                RawSha256 = (Get-FileHash -LiteralPath (Join-Path $repo "compatibility/stitching-upstream-raw.json") -Algorithm SHA256).Hash.ToLowerInvariant()
                DeclarationCount = [int]$stitchingSummary.declarationCount
                CallableCount = [int]$stitchingSummary.callableCount
                ImplementedCount = [int]$stitchingSummary.classificationCounts.implemented
                MissingCount = [int]$stitchingSummary.classificationCounts.missing
                IntentionallyOmittedCount = [int]$stitchingSummary.classificationCounts.'intentionally-omitted'
                UnsupportedCount = [int]$stitchingSummary.classificationCounts.unsupported
                UpstreamConditionalCount = [int]$stitchingSummary.classificationCounts.'upstream-conditional'
                HighLevelDeclarationCount = [int]$stitchingSummary.surfaceCounts.primary.declarations
                HighLevelCallableCount = [int]$stitchingSummary.surfaceCounts.primary.callables
                HighLevelImplementedCount = [int]$stitchingSummary.surfaceCounts.primary.implemented
                PublicWarperDeclarationCount = [int]$stitchingSummary.surfaceCounts.'public-warpers'.declarations
                PublicWarperCallableCount = [int]$stitchingSummary.surfaceCounts.'public-warpers'.callables
                PublicWarperImplementedCount = [int]$stitchingSummary.surfaceCounts.'public-warpers'.implemented
                BlenderDeclarationCount = [int]$stitchingSummary.surfaceCounts.'detail-blenders'.declarations
                BlenderCallableCount = [int]$stitchingSummary.surfaceCounts.'detail-blenders'.callables
                BlenderImplementedCount = [int]$stitchingSummary.surfaceCounts.'detail-blenders'.implemented
                ExposureDeclarationCount = [int]$stitchingSummary.surfaceCounts.'detail-exposure'.declarations
                ExposureCallableCount = [int]$stitchingSummary.surfaceCounts.'detail-exposure'.callables
                ExposureImplementedCount = [int]$stitchingSummary.surfaceCounts.'detail-exposure'.implemented
                MatcherDeclarationCount = [int]$stitchingSummary.surfaceCounts.'detail-matchers'.declarations
                MatcherCallableCount = [int]$stitchingSummary.surfaceCounts.'detail-matchers'.callables
                MatcherImplementedCount = [int]$stitchingSummary.surfaceCounts.'detail-matchers'.implemented
                SeamFinderDeclarationCount = [int]$stitchingSummary.surfaceCounts.'detail-seam-finders'.declarations
                SeamFinderCallableCount = [int]$stitchingSummary.surfaceCounts.'detail-seam-finders'.callables
                SeamFinderImplementedCount = [int]$stitchingSummary.surfaceCounts.'detail-seam-finders'.implemented
                TimelapserDeclarationCount = [int]$stitchingSummary.surfaceCounts.'detail-timelapsers'.declarations
                TimelapserCallableCount = [int]$stitchingSummary.surfaceCounts.'detail-timelapsers'.callables
                TimelapserImplementedCount = [int]$stitchingSummary.surfaceCounts.'detail-timelapsers'.implemented
                UtilityDeclarationCount = [int]$stitchingSummary.surfaceCounts.'detail-util'.declarations
                UtilityCallableCount = [int]$stitchingSummary.surfaceCounts.'detail-util'.callables
                UtilityImplementedCount = [int]$stitchingSummary.surfaceCounts.'detail-util'.implemented
                DetailWarperDeclarationCount = [int]$stitchingSummary.surfaceCounts.'detail-warpers'.declarations
                DetailWarperCallableCount = [int]$stitchingSummary.surfaceCounts.'detail-warpers'.callables
                DetailWarperImplementedCount = [int]$stitchingSummary.surfaceCounts.'detail-warpers'.implemented
                CompatibilityHeaderCount = [int]$stitchingSummary.compatibilityHeaderCount
                SourceHeaderCount = [int]$stitchingSummary.sourceHeaderCount
                SourceReviewedExtensionCount = [int]$stitchingSummary.sourceReviewedExtensionCount
                SelectedFamilyCount = [int]$stitchingSummary.selectedFamilyCount
                SelectedDeclarationCount = [int]$stitchingSummary.selectedDeclarationCount
                ManagedPublicTypeAdditionCount = [int]$stitchingSummary.managedPublicTypeAdditionCount
                ManagedPublicMemberAdditionCount = [int]$stitchingSummary.managedPublicMemberAdditionCount
                NativeEntrypointAdditionCount = [int]$stitchingSummary.nativeEntrypointAdditionCount
                UMatExecutionClaimed = [bool]$stitchingSummary.uMatExecutionClaimed
                DetailRowsMixedIntoHighLevel = [bool]$stitchingSummary.detailRowsMixedIntoHighLevel
                RepositoryWideParityClaimed = [bool]$stitchingSummary.repositoryWideUpstreamParityClaimed
            }
            VideoUpstreamMap = [ordered]@{
                Path = "compatibility/video-upstream-map.txt"
                SummaryPath = "compatibility/video-upstream-summary.json"
                ClassificationPath = "compatibility/video-upstream-classifications.json"
                FamilyInventoryPath = "compatibility/video-implemented-families.json"
                RawExtractionPath = "compatibility/video-upstream-raw.json"
                Sha256 = $videoSummary.mappingSha256
                RawSha256 = (Get-FileHash -LiteralPath (Join-Path $repo "compatibility/video-upstream-raw.json") -Algorithm SHA256).Hash.ToLowerInvariant()
                DeclarationCount = [int]$videoSummary.declarationCount
                CallableCount = [int]$videoSummary.callableCount
                ImplementedCount = [int]$videoSummary.classificationCounts.implemented
                MissingCount = [int]$videoSummary.classificationCounts.missing
                IntentionallyOmittedCount = [int]$videoSummary.classificationCounts.'intentionally-omitted'
                UnsupportedCount = [int]$videoSummary.classificationCounts.unsupported
                UpstreamConditionalCount = [int]$videoSummary.classificationCounts.'upstream-conditional'
                CompatibilityHeaderCount = [int]$videoSummary.compatibilityHeaderCount
                ExcludedPublicHeaderCount = [int]$videoSummary.excludedPublicHeaderCount
                SourceHeaderCount = [int]$videoSummary.sourceHeaderCount
                SelectedFamilyCount = [int]$videoSummary.selectedFamilyCount
                SelectedDeclarationCount = [int]$videoSummary.selectedDeclarationCount
                ManagedPublicTypeAdditionCount = [int]$videoSummary.managedPublicTypeAdditionCount
                ManagedPublicMemberAdditionCount = [int]$videoSummary.managedPublicMemberAdditionCount
                NativeEntrypointAdditionCount = [int]$videoSummary.nativeEntrypointAdditionCount
                RepositoryWideParityClaimed = [bool]$videoSummary.repositoryWideUpstreamParityClaimed
            }
            GapInventoryPath = "compatibility/api-gap-inventory.json"
            GapInventoryStatus = $gapInventory.status
            UpstreamParityMeasured = [bool]$gapInventory.measurements.upstreamCppParityMeasured
            NativeToManagedParityMeasured = [bool]$gapInventory.measurements.nativeToManagedParityMeasured
            ImplementedSpanFamilyPath = "compatibility/imgproc-point-set-span-family.json"
            ImplementedSpanFamilyStatus = $spanFamily.status
        }
        EvidenceReferences = $evidence
        LocalValidation = [ordered]@{
            Status = "locally-validated"
            InvariantGuardCount = 76
            RequiredChecks = @("actionlint-1.7.12", "api-abi-baseline", "docfx-2.78.5", "git-diff-check", "repository-powershell-ast", "workflow-bash-syntax", "workflow-powershell-syntax")
            ExactSdk = "10.0.302"
            PublicationAllowed = $false
        }
        Signing = [ordered]@{
            Status = "repository-signing-pending"
            Strategy = "nuget.org-repository-signing"
            NormalizedInputRequired = $true
            AuthorCertificateRequired = $false
            PrivateKeyRequired = $false
            PrivateKeyMaterialPresent = $false
            ServiceIndex = "https://api.nuget.org/v3/index.json"
            ExpectedSignatureType = "Repository"
            ExpectedOwner = "GuojinYan"
            VerificationScript = "scripts/Test-NuGetRepositorySignedPackage.ps1"
            Verification = "post-publication-required"
        }
        Sbom = [ordered]@{
            Status = "not-ready"
            Format = "SPDX-2.3"
            Generator = "scripts/New-ReleasePackageSbom.ps1"
            Guard = "scripts/Test-ReleasePackageSbom.ps1"
            Deterministic = $true
            FinalCandidateDocumentGenerated = $false
            Verification = "generator-verified-final-candidate-not-generated"
        }
        Approval = [ordered]@{
            Status = "not-approved"
            Reviewer = "automated-local-preflight"
            Approver = "unassigned"
            EvidenceKind = "local-source-and-offline-fixture"
            RemoteMutationAllowed = $false
        }
        PublicFeed = [ordered]@{
            Mode = "read-only"
            ServiceIndex = "https://api.nuget.org/v3/index.json"
            GitHubPackagesServiceIndex = "https://nuget.pkg.github.com/guojin-yan/index.json"
            GitHubPackagesRepository = "guojin-yan/OpenCV-CSharp-API"
            RequiredPublicVisibility = "public"
            RequiredFeedCount = 2
            CandidatePackage = "https://api.nuget.org/v3-flatcontainer/jyppx.opencv.csharp.api/5.0.0-preview.1/jyppx.opencv.csharp.api.5.0.0-preview.1.nupkg"
            Methods = @("GET", "HEAD")
            Mutable = $false
            CandidateStatus = "not-published"
            UploadAttempted = $false
        }
        Rollback = [ordered]@{
            Status = "not-published"
            PriorKnownGood = ""
            PackageRemovalRequired = $false
        }
        ReleaseApproval = [ordered]@{
            Status = "not-approved"
            ReleaseReady = $false
            PublicationDecision = "do-not-publish"
        }
        ExternalBlockers = @(Get-OrdinalSortedObjects -Values $blockers -Property "Id")
        PrivateKeyMaterialPresent = $false
        SecretMaterialPresent = $false
        Deterministic = $true
    }
}

$record = Get-Record
$json = Normalize-Text ($record | ConvertTo-Json -Depth 30)
if ($Check) {
    if (-not (Test-Path -LiteralPath $OutputPath -PathType Leaf)) {
        throw "Final release closeout record is missing: $OutputPath"
    }
    $current = Normalize-Text ([IO.File]::ReadAllText($OutputPath))
    if ($current -cne $json) {
        throw "Final release closeout record is out of date: $OutputPath"
    }
}
else {
    [IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($OutputPath)) | Out-Null
    [IO.File]::WriteAllText($OutputPath, $json, [Text.UTF8Encoding]::new($false))
}

Write-Host "RELEASE_CANDIDATE_CLOSEOUT_RECORD_OK candidate=$($record.CandidateId) files=$($record.SourceSet.FileCount) source_sha256=$($record.SourceSet.Sha256) mode=$(if($Check){'check'}else{'write'})"

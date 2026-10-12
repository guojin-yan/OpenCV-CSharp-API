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
        "CHANGELOG.md",
        "CONTRIBUTING.md",
        "README.md",
        "README_cn.md",
        "compatibility/api-gap-inventory.json",
        "compatibility/upstream-map-catalog.json",
        "compatibility/upstream-map-catalog.schema.json",
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
        "compatibility/dnn_superres-implemented-families.json",
        "compatibility/dnn_superres-upstream-classifications.json",
        "compatibility/dnn_superres-upstream-map.txt",
        "compatibility/dnn_superres-upstream-raw.json",
        "compatibility/dnn_superres-upstream-summary.json",
        "compatibility/wechat_qrcode-implemented-families.json",
        "compatibility/wechat_qrcode-upstream-classifications.json",
        "compatibility/wechat_qrcode-upstream-map.txt",
        "compatibility/wechat_qrcode-upstream-raw.json",
        "compatibility/wechat_qrcode-upstream-summary.json",
        "compatibility/cnn_3dobj-implemented-families.json",
        "compatibility/cnn_3dobj-upstream-classifications.json",
        "compatibility/cnn_3dobj-upstream-map.txt",
        "compatibility/cnn_3dobj-upstream-raw.json",
        "compatibility/cnn_3dobj-upstream-summary.json",
        "compatibility/signal-implemented-families.json",
        "compatibility/signal-upstream-classifications.json",
        "compatibility/signal-upstream-map.txt",
        "compatibility/signal-upstream-raw.json",
        "compatibility/signal-upstream-summary.json",
        "compatibility/dpm-implemented-families.json",
        "compatibility/dpm-upstream-classifications.json",
        "compatibility/dpm-upstream-map.txt",
        "compatibility/dpm-upstream-raw.json",
        "compatibility/dpm-upstream-summary.json",
        "compatibility/hdf-implemented-families.json",
        "compatibility/hdf-upstream-classifications.json",
        "compatibility/hdf-upstream-map.txt",
        "compatibility/hdf-upstream-raw.json",
        "compatibility/hdf-upstream-summary.json",
        "compatibility/text-implemented-families.json",
        "compatibility/text-upstream-classifications.json",
        "compatibility/text-upstream-map.txt",
        "compatibility/text-upstream-raw.json",
        "compatibility/text-upstream-summary.json",
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
        "compatibility/xphoto-implemented-families.json",
        "compatibility/xphoto-upstream-classifications.json",
        "compatibility/xphoto-upstream-map.txt",
        "compatibility/xphoto-upstream-raw.json",
        "compatibility/xphoto-upstream-summary.json",
        "compatibility/ximgproc-implemented-families.json",
        "compatibility/ximgproc-upstream-classifications.json",
        "compatibility/ximgproc-upstream-map.txt",
        "compatibility/ximgproc-upstream-raw.json",
        "compatibility/ximgproc-upstream-summary.json",
        "compatibility/optflow-implemented-families.json",
        "compatibility/optflow-upstream-classifications.json",
        "compatibility/optflow-upstream-map.txt",
        "compatibility/optflow-upstream-raw.json",
        "compatibility/optflow-upstream-summary.json",
        "compatibility/bgsegm-implemented-families.json",
        "compatibility/bgsegm-upstream-classifications.json",
        "compatibility/bgsegm-upstream-map.txt",
        "compatibility/bgsegm-upstream-raw.json",
        "compatibility/bgsegm-upstream-summary.json",
        "compatibility/face-implemented-families.json",
        "compatibility/face-upstream-classifications.json",
        "compatibility/face-upstream-map.txt",
        "compatibility/face-upstream-raw.json",
        "compatibility/face-upstream-summary.json",
        "compatibility/quality-implemented-families.json",
        "compatibility/quality-upstream-classifications.json",
        "compatibility/quality-upstream-map.txt",
        "compatibility/quality-upstream-raw.json",
        "compatibility/quality-upstream-summary.json",
        "compatibility/img_hash-implemented-families.json",
        "compatibility/img_hash-upstream-classifications.json",
        "compatibility/img_hash-upstream-map.txt",
        "compatibility/img_hash-upstream-raw.json",
        "compatibility/img_hash-upstream-summary.json",
        "compatibility/line_descriptor-implemented-families.json",
        "compatibility/line_descriptor-upstream-classifications.json",
        "compatibility/line_descriptor-upstream-map.txt",
        "compatibility/line_descriptor-upstream-raw.json",
        "compatibility/line_descriptor-upstream-summary.json",
        "compatibility/freetype-implemented-families.json",
        "compatibility/freetype-upstream-classifications.json",
        "compatibility/freetype-upstream-map.txt",
        "compatibility/freetype-upstream-raw.json",
        "compatibility/freetype-upstream-summary.json",
        "compatibility/alphamat-implemented-families.json",
        "compatibility/alphamat-upstream-classifications.json",
        "compatibility/alphamat-upstream-map.txt",
        "compatibility/alphamat-upstream-raw.json",
        "compatibility/alphamat-upstream-summary.json",
        "compatibility/intensity_transform-implemented-families.json",
        "compatibility/intensity_transform-upstream-classifications.json",
        "compatibility/intensity_transform-upstream-map.txt",
        "compatibility/intensity_transform-upstream-raw.json",
        "compatibility/intensity_transform-upstream-summary.json",
        "compatibility/plot-implemented-families.json",
        "compatibility/plot-upstream-classifications.json",
        "compatibility/plot-upstream-map.txt",
        "compatibility/plot-upstream-raw.json",
        "compatibility/plot-upstream-summary.json",
        "compatibility/bioinspired-implemented-families.json",
        "compatibility/bioinspired-upstream-classifications.json",
        "compatibility/bioinspired-upstream-map.txt",
        "compatibility/bioinspired-upstream-raw.json",
        "compatibility/bioinspired-upstream-summary.json",
        "compatibility/phase_unwrapping-implemented-families.json",
        "compatibility/phase_unwrapping-upstream-classifications.json",
        "compatibility/phase_unwrapping-upstream-map.txt",
        "compatibility/phase_unwrapping-upstream-raw.json",
        "compatibility/phase_unwrapping-upstream-summary.json",
        "compatibility/hfs-implemented-families.json",
        "compatibility/hfs-upstream-classifications.json",
        "compatibility/hfs-upstream-map.txt",
        "compatibility/hfs-upstream-raw.json",
        "compatibility/hfs-upstream-summary.json",
        "compatibility/fuzzy-implemented-families.json",
        "compatibility/fuzzy-upstream-classifications.json",
        "compatibility/fuzzy-upstream-map.txt",
        "compatibility/fuzzy-upstream-raw.json",
        "compatibility/fuzzy-upstream-summary.json",
        "compatibility/rapid-implemented-families.json",
        "compatibility/rapid-upstream-classifications.json",
        "compatibility/rapid-upstream-map.txt",
        "compatibility/rapid-upstream-raw.json",
        "compatibility/rapid-upstream-summary.json",
        "compatibility/shape-implemented-families.json",
        "compatibility/shape-upstream-classifications.json",
        "compatibility/shape-upstream-map.txt",
        "compatibility/shape-upstream-raw.json",
        "compatibility/shape-upstream-summary.json",
        "compatibility/structured_light-implemented-families.json",
        "compatibility/structured_light-upstream-classifications.json",
        "compatibility/structured_light-upstream-map.txt",
        "compatibility/structured_light-upstream-raw.json",
        "compatibility/structured_light-upstream-summary.json",
        "compatibility/saliency-implemented-families.json",
        "compatibility/saliency-upstream-classifications.json",
        "compatibility/saliency-upstream-map.txt",
        "compatibility/saliency-upstream-raw.json",
        "compatibility/saliency-upstream-summary.json",
        "compatibility/surface_matching-implemented-families.json",
        "compatibility/surface_matching-upstream-classifications.json",
        "compatibility/surface_matching-upstream-map.txt",
        "compatibility/surface_matching-upstream-raw.json",
        "compatibility/surface_matching-upstream-summary.json",
        "compatibility/reg-implemented-families.json",
        "compatibility/reg-upstream-classifications.json",
        "compatibility/reg-upstream-map.txt",
        "compatibility/reg-upstream-raw.json",
        "compatibility/reg-upstream-summary.json",
        "compatibility/xstereo-implemented-families.json",
        "compatibility/xstereo-upstream-classifications.json",
        "compatibility/xstereo-upstream-map.txt",
        "compatibility/xstereo-upstream-raw.json",
        "compatibility/xstereo-upstream-summary.json",
        "compatibility/ptcloud-implemented-families.json",
        "compatibility/ptcloud-upstream-classifications.json",
        "compatibility/ptcloud-upstream-map.txt",
        "compatibility/ptcloud-upstream-raw.json",
        "compatibility/ptcloud-upstream-summary.json",
        "compatibility/xobjdetect-implemented-families.json",
        "compatibility/xobjdetect-upstream-classifications.json",
        "compatibility/xobjdetect-upstream-map.txt",
        "compatibility/xobjdetect-upstream-raw.json",
        "compatibility/xobjdetect-upstream-summary.json",
        "compatibility/v5.0.1-change-ledger.json",
        "compatibility/v5.0.1-change-ledger.schema.json",
        "docs/articles/api-abi-compatibility-policy.md",
        "docs/articles/upstream-map-toolchain.md",
        "docs/articles/signal-upstream-parity-guide.md",
        "docs/articles/dpm-upstream-parity-guide.md",
        "docs/articles/hdf-upstream-parity-guide.md",
        "docs/articles/text-upstream-parity-guide.md",
        "docs/articles/dnn-superres-upstream-parity-guide.md",
        "docs/articles/wechat-qrcode-upstream-parity-guide.md",
        "docs/articles/cnn-3dobj-upstream-parity-guide.md",
        "docs/articles/xphoto-upstream-parity-guide.md",
        "docs/articles/ximgproc-upstream-parity-guide.md",
        "docs/articles/optflow-bgsegm-upstream-parity-guide.md",
        "docs/articles/face-guide.md",
        "docs/articles/quality-guide.md",
        "docs/articles/img-hash-guide.md",
        "docs/articles/line-descriptor-guide.md",
        "docs/articles/intensity-transform-guide.md",
        "docs/articles/plot-guide.md",
        "docs/articles/bioinspired-guide.md",
        "docs/articles/phase-unwrapping-guide.md",
        "docs/articles/hfs-guide.md",
        "docs/articles/fuzzy-guide.md",
        "docs/articles/rapid-guide.md",
        "docs/articles/shape-guide.md",
        "docs/articles/structured-light-guide.md",
        "docs/articles/saliency-guide.md",
        "docs/articles/saliency-objectness-guide.md",
        "docs/articles/surface-matching-guide.md",
        "docs/articles/reg-guide.md",
        "docs/articles/xobjdetect-guide.md",
        "docs/articles/calib3d-upstream-parity-guide.md",
        "docs/articles/core-upstream-parity-guide.md",
        "docs/articles/dnn-structured-parity-guide.md",
        "docs/articles/dnn-named-io-dynamic-shape-adr.md",
        "docs/articles/dnn-model-wrapper-adr.md",
        "docs/articles/dnn-quantization-backend-diagnostics-adr.md",
        "docs/articles/dnn-tokenizer-text-adr.md",
        "docs/articles/features-upstream-parity-guide.md",
        "docs/articles/generic-linux-rid-adr.md",
        "docs/articles/headless-runtime-profile-adr.md",
        "docs/articles/generic-linux-musl-feasibility-adr.md",
        "docs/articles/domestic-platform-certification-adr.md",
        "docs/articles/opencl-tapi-umat-adr.md",
        "docs/articles/highgui-interaction-guide.md",
        "docs/articles/imgcodecs-upstream-parity-guide.md",
        "docs/articles/codec-buffer-lease-adr.md",
        "docs/articles/codec-buffer-and-owner-adr.md",
        "docs/articles/codec-benchmark-baseline.md",
        "docs/articles/image-adapter-selection-adr.md",
        "docs/articles/codec-preflight-and-limits.md",
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
        "docs/articles/sample-model-assets-guide.md",
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
        "docs/articles/async-video-owner-adr.md",
        "docs/articles/v501-modern-api-preview-guide.md",
        "docs/articles/video-dnn-benchmark-baseline.md",
        "docs/articles/perf-001-regression-baseline.md",
        "docs/articles/native-aot-trim-single-file-adr.md",
        "nuget/logo.jpg",
        "packaging/runtime/android-runtime-evidence.json",
        "packaging/runtime/runtime-generic-linux-feasibility.json",
        "packaging/runtime/runtime-generic-linux-feasibility.schema.json",
        "packaging/runtime/runtime-generic-linux-preview-matrix.json",
        "packaging/runtime/runtime-generic-linux-preview-matrix.schema.json",
        "packaging/runtime/runtime-generic-linux-arm64-preview-matrix.json",
        "packaging/runtime/runtime-generic-linux-arm64-preview-matrix.schema.json",
        "packaging/runtime/runtime-headless-profile-contract.json",
        "packaging/runtime/runtime-headless-profile-contract.schema.json",
        "packaging/runtime/runtime-headless-candidate-evidence.json",
        "packaging/runtime/runtime-headless-candidate-evidence.schema.json",
        "packaging/runtime/runtime-generic-linux-musl-feasibility.json",
        "packaging/runtime/runtime-generic-linux-musl-feasibility.schema.json",
        "packaging/runtime/runtime-distro-rid-graph.json",
        "packaging/platform/domestic-platform-certification-matrix.json",
        "packaging/platform/domestic-platform-certification-matrix.schema.json",
        "packaging/runtime/runtime-opencl-tapi-umat-contract.json",
        "packaging/runtime/runtime-opencl-tapi-umat-contract.schema.json",
        "packaging/runtime/runtime-capabilities-json.schema.json",
        "packaging/runtime/capability-profile-evidence.json",
        "packaging/runtime/capability-profile-evidence.schema.json",
        "packaging/codec/image-preflight-mutation-corpus.json",
        "packaging/codec/image-preflight-mutation-corpus.schema.json",
        "packaging/codec/image-preflight-fuzz-evidence.json",
        "packaging/codec/image-preflight-fuzz-evidence.schema.json",
        "packaging/codec/codec-native-mutation-evidence.json",
        "packaging/codec/codec-native-mutation-evidence.schema.json",
        "packaging/performance/codec-typed-mat-benchmark-evidence.json",
        "packaging/performance/codec-typed-mat-benchmark-evidence.schema.json",
        "packaging/performance/codec-writer-native-evidence.json",
        "packaging/performance/codec-writer-native-evidence.schema.json",
        "packaging/performance/codec-writer-allocation-evidence.json",
        "packaging/performance/codec-writer-allocation-evidence.schema.json",
        "packaging/performance/video-dnn-benchmark-evidence.json",
        "packaging/performance/video-dnn-benchmark-evidence.schema.json",
        "packaging/performance/perf-001-regression-baseline.json",
        "packaging/performance/perf-001-regression-baseline.schema.json",
        "packaging/performance/native-aot-smoke-evidence.json",
        "packaging/performance/native-aot-smoke-evidence.schema.json",
        "packaging/performance/native-aot-trim-warning-ledger.json",
        "packaging/performance/native-aot-trim-warning-ledger.schema.json",
        "packaging/performance/typed-mat-native-lifetime-evidence.json",
        "packaging/performance/typed-mat-native-lifetime-evidence.schema.json",
        "packaging/performance/typed-mat-owner-lifetime-matrix.json",
        "packaging/performance/typed-mat-owner-lifetime-matrix.schema.json",
        "packaging/performance/typed-mat-target-framework-evidence.json",
        "packaging/performance/typed-mat-target-framework-evidence.schema.json",
        "packaging/video/runtime-async-video-contract.json",
        "packaging/video/runtime-async-video-contract.schema.json",
        "packaging/dnn/runtime-dnn-diagnostics-contract.json",
        "packaging/dnn/runtime-dnn-diagnostics-contract.schema.json",
        "packaging/codec/image-format-policy.json",
        "packaging/codec/image-format-policy.schema.json",
        "packaging/runtime/runtime-support-contract.schema.json",
        "packaging/runtime/runtime-support-contract.json",
        "samples/AndroidSmoke/AndroidSmoke.csproj",
        "samples/AndroidSmoke/MainActivity.cs",
        "samples/Common/ModelAssetSupport.cs",
        "samples/ConsoleSamples/Program.cs",
        "samples/ConsoleSamples/ShowcaseRunner.cs",
        "samples/assets/models/model-assets.json",
        "scripts/Generate-Calib3DUpstreamMap.ps1",
        "scripts/Generate-CoreUpstreamMap.ps1",
        "scripts/Generate-DnnUpstreamMap.ps1",
        "scripts/Generate-DnnSuperresUpstreamMap.ps1",
        "scripts/Generate-WechatQRCodeUpstreamMap.ps1",
        "scripts/Generate-Cnn3DObjUpstreamMap.ps1",
        "scripts/Generate-SignalUpstreamMap.ps1",
        "scripts/Generate-DpmUpstreamMap.ps1",
        "scripts/Generate-HdfUpstreamMap.ps1",
        "scripts/Generate-TextUpstreamMap.ps1",
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
          "scripts/Build-OpenCV.ps1",
          "scripts/Apply-OpenCvSourcePatches.ps1",
          "scripts/Get-NativeRuntimeProfileEvidence.ps1",
          "scripts/Invoke-LifecycleRefreshContainerProducer.ps1",
          "scripts/Invoke-LifecycleRefreshContainerProducer.sh",
          "scripts/Test-LifecycleRefreshCandidateProducerSurface.ps1",
          "scripts/Test-LifecycleRefreshCandidatePackageSurface.ps1",
        "scripts/Test-LifecycleRefreshRuntimeInputArtifact.ps1",
        "scripts/Test-GenericLinuxFeasibilityContract.ps1",
        "scripts/Test-GenericLinuxPreviewMatrix.ps1",
        "scripts/New-GenericLinuxPreviewPackage.ps1",
        "scripts/Test-GenericLinuxPreviewPackageSurface.ps1",
        "scripts/Test-GenericLinuxPreviewConsumer.ps1",
        "scripts/Test-GenericLinuxArm64PreviewMatrix.ps1",
        "scripts/Test-GenericLinuxArm64PreviewPackageSurface.ps1",
        "scripts/Test-HeadlessRuntimeProfileContract.ps1",
        "scripts/Measure-HeadlessCandidateEvidence.ps1",
        "scripts/Test-HeadlessCandidateEvidence.ps1",
        "scripts/Test-GenericLinuxMuslFeasibilityContract.ps1",
        "scripts/Test-DomesticPlatformCertificationMatrix.ps1",
        "scripts/Test-OpenClTapiUmatContract.ps1",
        "scripts/Test-CapabilitiesJsonContract.ps1",
        "scripts/Measure-CapabilityProfileEvidence.ps1",
        "scripts/Test-CapabilityProfileEvidence.ps1",
        ".github/workflows/generic-linux-arm64-preview.yml",
        "scripts/Test-GitHubPackConsumerRestoreSurface.ps1",
          "packaging/runtime/patches/opencv-5.0.0-photo-ccm-instance-color-space.patch",
          "scripts/New-RuntimeInputArtifact.ps1",
        "scripts/Get-SampleModelAssets.ps1",
        "scripts/New-ReleaseCandidateFinalCloseout.ps1",
        "scripts/New-NuGetPublicationBundle.ps1",
        "scripts/Test-NuGetPublicationManifest.ps1",
        "scripts/New-ReleasePackageSbom.ps1",
        "scripts/Test-ApiAbiBaselineContract.ps1",
        "scripts/Test-Calib3DUpstreamMap.ps1",
        "scripts/Test-CoreUpstreamMap.ps1",
        "scripts/Test-DnnUpstreamMap.ps1",
        "scripts/Test-DnnSuperresUpstreamMap.ps1",
        "scripts/Test-WechatQRCodeUpstreamMap.ps1",
        "scripts/Test-Cnn3DObjUpstreamMap.ps1",
        "scripts/Test-SignalUpstreamMap.ps1",
        "scripts/Test-DpmUpstreamMap.ps1",
        "scripts/Test-HdfUpstreamMap.ps1",
        "scripts/Test-TextUpstreamMap.ps1",
        "scripts/Test-FeaturesUpstreamMap.ps1",
        "scripts/Test-HighGuiUpstreamMap.ps1",
        "scripts/Test-ImgCodecsUpstreamMap.ps1",
        "scripts/Test-ImgProcUpstreamMap.ps1",
        "scripts/Test-LifecycleRefreshRuntimeMatrix.ps1",
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
        "scripts/Test-ReleaseNotesContract.ps1",
        "scripts/Test-ReleaseSigningBoundary.ps1",
        "scripts/Test-SampleModelAssetContract.ps1",
        "scripts/Test-NuGetRepositorySignedPackage.ps1",
        "scripts/Test-NuGetRepositorySigningBoundary.ps1",
        "scripts/Test-ImageAdapterLicenseGate.ps1",
        "scripts/Test-PackageMetadataNeutrality.ps1",
        "scripts/Test-ReleaseSupportContract.ps1",
        "scripts/Test-StitchingUpstreamMap.ps1",
        "scripts/Test-TrackingUpstreamMap.ps1",
        "scripts/Test-VideoIORegistrySurface.ps1",
        "scripts/Test-VideoIOUpstreamMap.ps1",
        "scripts/Test-VideoUpstreamMap.ps1",
        "scripts/Test-WorkflowInvariantCoverage.ps1",
        "docs/releases/README.md",
        "docs/releases/5.0.0-preview.1.md",
        "docs/releases/5.0.0.md",
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
        "src/OpenCvSharp/Plot/Plot2d.cs",
        "src/OpenCvSharp/Plot/PlotCv2.cs",
        "src/OpenCvSharp/BioInspired/BioInspiredCv2.cs",
        "src/OpenCvSharp/BioInspired/BioInspiredStringConvert.cs",
        "src/OpenCvSharp/BioInspired/Retina.cs",
        "src/OpenCvSharp/BioInspired/RetinaColorSamplingMethod.cs",
        "src/OpenCvSharp/BioInspired/RetinaFastToneMapping.cs",
        "src/OpenCvSharp/BioInspired/RetinaMagnoParameters.cs",
        "src/OpenCvSharp/BioInspired/RetinaParameters.cs",
        "src/OpenCvSharp/BioInspired/RetinaParvoParameters.cs",
        "src/OpenCvSharp/BioInspired/SegmentationParameters.cs",
        "src/OpenCvSharp/BioInspired/TransientAreasSegmentationModule.cs",
        "src/OpenCvSharp.Native/src/bioinspired/bioinspired.cpp",
        "src/OpenCvSharp.Native/src/bioinspired/bioinspired_handles.h",
        "tests/OpenCvSharp.Tests/BioInspired/BioInspiredTests.cs",
        "src/OpenCvSharp/PhaseUnwrapping/HistogramPhaseUnwrapping.cs",
        "src/OpenCvSharp/PhaseUnwrapping/HistogramPhaseUnwrappingParams.cs",
        "src/OpenCvSharp/PhaseUnwrapping/PhaseUnwrappingCv2.cs",
        "src/OpenCvSharp/PhaseUnwrapping/PhaseUnwrappingObject.cs",
        "src/OpenCvSharp.Native/src/phase_unwrapping/phase_unwrapping.cpp",
        "src/OpenCvSharp.Native/src/phase_unwrapping/phase_unwrapping_handles.h",
        "tests/OpenCvSharp.Tests/PhaseUnwrapping/PhaseUnwrappingTests.cs",
        "src/OpenCvSharp/Hfs/HfsSegment.cs",
        "src/OpenCvSharp/Hfs/HfsSegmentParams.cs",
        "src/OpenCvSharp/Hfs/HfsCv2.cs",
        "src/OpenCvSharp/Internal/Interop/NativeHfsSegmentHandle.cs",
        "src/OpenCvSharp/Internal/Interop/NativeMethods.Hfs.DllImport.cs",
        "src/OpenCvSharp/Internal/Interop/NativeMethods.Hfs.LibraryImport.cs",
        "src/OpenCvSharp.Native/include/open_cv_sharp/hfs/hfs.h",
        "src/OpenCvSharp.Native/src/hfs/hfs.cpp",
        "src/OpenCvSharp.Native/src/hfs/hfs_handles.h",
        "tests/OpenCvSharp.Tests/Hfs/HfsTests.cs",
        "src/OpenCvSharp/Fuzzy/FuzzyCv2.cs",
        "src/OpenCvSharp/Fuzzy/FuzzyFunctionType.cs",
        "src/OpenCvSharp/Fuzzy/FuzzyInpaintAlgorithm.cs",
        "src/OpenCvSharp/Internal/Interop/NativeMethods.Fuzzy.DllImport.cs",
        "src/OpenCvSharp/Internal/Interop/NativeMethods.Fuzzy.LibraryImport.cs",
        "src/OpenCvSharp.Native/include/open_cv_sharp/fuzzy/fuzzy.h",
        "src/OpenCvSharp.Native/src/fuzzy/fuzzy.cpp",
        "tests/OpenCvSharp.Tests/Fuzzy/FuzzyTests.cs",
        "src/OpenCvSharp/Rapid/RapidCv2.cs",
        "src/OpenCvSharp/Rapid/RapidResult.cs",
        "src/OpenCvSharp/Rapid/RapidTracker.cs",
        "src/OpenCvSharp/Rapid/RapidSilhouetteTracker.cs",
        "src/OpenCvSharp/Rapid/OlsTracker.cs",
        "src/OpenCvSharp/Internal/Interop/NativeMethods.Rapid.DllImport.cs",
        "src/OpenCvSharp/Internal/Interop/NativeMethods.Rapid.LibraryImport.cs",
        "src/OpenCvSharp.Native/include/open_cv_sharp/rapid/rapid.h",
        "src/OpenCvSharp.Native/src/rapid/rapid.cpp",
        "src/OpenCvSharp.Native/src/rapid/rapid_handles.h",
        "tests/OpenCvSharp.Tests/Rapid/RapidTests.cs",
        "src/OpenCvSharp/Shape/ShapeCv2.cs",
        "src/OpenCvSharp/Shape/HistogramCostExtractor.cs",
        "src/OpenCvSharp/Shape/NormHistogramCostExtractor.cs",
        "src/OpenCvSharp/Shape/EMDHistogramCostExtractor.cs",
        "src/OpenCvSharp/Shape/ChiHistogramCostExtractor.cs",
        "src/OpenCvSharp/Shape/EMDL1HistogramCostExtractor.cs",
        "src/OpenCvSharp/Shape/ShapeDistanceExtractor.cs",
        "src/OpenCvSharp/Shape/ShapeContextDistanceExtractor.cs",
        "src/OpenCvSharp/Shape/HausdorffDistanceExtractor.cs",
        "src/OpenCvSharp/Internal/Interop/NativeMethods.Shape.DllImport.cs",
        "src/OpenCvSharp/Internal/Interop/NativeMethods.Shape.LibraryImport.cs",
        "src/OpenCvSharp.Native/include/open_cv_sharp/shape/shape.h",
        "src/OpenCvSharp.Native/src/shape/shape.cpp",
        "src/OpenCvSharp.Native/src/shape/shape_handles.h",
        "tests/OpenCvSharp.Tests/Shape/ShapeTests.cs",
        "src/OpenCvSharp/Internal/Interop/NativeBlenderHandle.cs",
        "src/OpenCvSharp/Internal/Interop/NativeEstimatorHandle.cs",
        "src/OpenCvSharp/Internal/Interop/NativeFeaturesMatcherHandle.cs",
        "src/OpenCvSharp/Internal/Interop/NativeHighGuiCallbackRegistrationHandle.cs",
        "src/OpenCvSharp/Internal/CodecBufferLease.cs",
        "scripts/Test-CodecBufferLeasePrototype.ps1",
        "src/OpenCvSharp/Properties/InternalsVisibleTo.cs",
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
        "tests/OpenCvSharp.Tests/Core/CodecBufferLeaseTests.cs",
        "tests/OpenCvSharp.Tests/ImgCodecs/Cv2InteropTests.cs",
        "tools/Calib3DUpstreamMap/Calib3DUpstreamMap.csproj",
        "tools/Calib3DUpstreamMap/Program.cs",
        "tools/Calib3DUpstreamMap/extract_calib3d.py",
        "tools/CoreUpstreamMap/CoreUpstreamMap.csproj",
        "tools/CoreUpstreamMap/Program.cs",
        "tools/CoreUpstreamMap/extract_core.py",
        "tools/DnnUpstreamMap/DnnUpstreamMap.csproj",
        "tools/DnnUpstreamMap/Program.cs",
        "tools/DnnUpstreamMap/extract_dnn.py",
        "tools/DnnSuperresUpstreamMap/DnnSuperresUpstreamMap.csproj",
        "tools/DnnSuperresUpstreamMap/Program.cs",
        "tools/DnnSuperresUpstreamMap/extract_dnn_superres.py",
        "tools/WechatQRCodeUpstreamMap/WechatQRCodeUpstreamMap.csproj",
        "tools/WechatQRCodeUpstreamMap/Program.cs",
        "tools/WechatQRCodeUpstreamMap/extract_wechat_qrcode.py",
        "tools/Cnn3DObjUpstreamMap/Cnn3DObjUpstreamMap.csproj",
        "tools/Cnn3DObjUpstreamMap/Program.cs",
        "tools/Cnn3DObjUpstreamMap/extract_cnn_3dobj.py",
        "tools/SignalUpstreamMap/SignalUpstreamMap.csproj",
        "tools/SignalUpstreamMap/Program.cs",
        "tools/SignalUpstreamMap/extract_signal.py",
        "tools/DpmUpstreamMap/DpmUpstreamMap.csproj",
        "tools/DpmUpstreamMap/Program.cs",
        "tools/DpmUpstreamMap/extract_dpm.py",
        "tools/HdfUpstreamMap/HdfUpstreamMap.csproj",
        "tools/HdfUpstreamMap/Program.cs",
        "tools/HdfUpstreamMap/extract_hdf.py",
        "tools/TextUpstreamMap/TextUpstreamMap.csproj",
        "tools/TextUpstreamMap/Program.cs",
        "tools/TextUpstreamMap/extract_text.py",
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
        "tools/VideoUpstreamMap/extract_video.py",
        "tools/XPhotoUpstreamMap/XPhotoUpstreamMap.csproj",
        "tools/XPhotoUpstreamMap/Program.cs",
        "tools/XPhotoUpstreamMap/extract_xphoto.py",
        "tools/XImgProcUpstreamMap/XImgProcUpstreamMap.csproj",
        "tools/XImgProcUpstreamMap/Program.cs",
        "tools/XImgProcUpstreamMap/extract_ximgproc.py",
        "tools/OptFlowUpstreamMap/OptFlowUpstreamMap.csproj",
        "tools/OptFlowUpstreamMap/Program.cs",
        "tools/OptFlowUpstreamMap/extract_optflow.py",
        "tools/BgSegmUpstreamMap/BgSegmUpstreamMap.csproj",
        "tools/BgSegmUpstreamMap/Program.cs",
        "tools/BgSegmUpstreamMap/extract_bgsegm.py",
        "tools/FaceUpstreamMap/FaceUpstreamMap.csproj",
        "tools/FaceUpstreamMap/Program.cs",
        "tools/FaceUpstreamMap/extract_face.py",
        "tools/QualityUpstreamMap/QualityUpstreamMap.csproj",
        "tools/QualityUpstreamMap/Program.cs",
        "tools/QualityUpstreamMap/extract_quality.py",
        "tools/ImgHashUpstreamMap/ImgHashUpstreamMap.csproj",
        "tools/ImgHashUpstreamMap/Program.cs",
        "tools/ImgHashUpstreamMap/extract_img_hash.py",
        "tools/LineDescriptorUpstreamMap/LineDescriptorUpstreamMap.csproj",
        "tools/LineDescriptorUpstreamMap/Program.cs",
        "tools/LineDescriptorUpstreamMap/extract_line_descriptor.py",
        "tools/FreetypeUpstreamMap/FreetypeUpstreamMap.csproj",
        "tools/FreetypeUpstreamMap/Program.cs",
        "tools/FreetypeUpstreamMap/extract_freetype.py",
        "tools/AlphaMatUpstreamMap/AlphaMatUpstreamMap.csproj",
        "tools/AlphaMatUpstreamMap/Program.cs",
        "tools/AlphaMatUpstreamMap/extract_alphamat.py",
        "tools/IntensityTransformUpstreamMap/IntensityTransformUpstreamMap.csproj",
        "tools/IntensityTransformUpstreamMap/Program.cs",
        "tools/IntensityTransformUpstreamMap/extract_intensity_transform.py",
        "tools/PlotUpstreamMap/PlotUpstreamMap.csproj",
        "tools/PlotUpstreamMap/Program.cs",
        "tools/PlotUpstreamMap/extract_plot.py",
        "tools/BioInspiredUpstreamMap/BioInspiredUpstreamMap.csproj",
        "tools/BioInspiredUpstreamMap/Program.cs",
        "tools/BioInspiredUpstreamMap/extract_bioinspired.py",
        "tools/PhaseUnwrappingUpstreamMap/PhaseUnwrappingUpstreamMap.csproj",
        "tools/PhaseUnwrappingUpstreamMap/Program.cs",
        "tools/PhaseUnwrappingUpstreamMap/extract_phase_unwrapping.py",
        "tools/HfsUpstreamMap/HfsUpstreamMap.csproj",
        "tools/HfsUpstreamMap/Program.cs",
        "tools/HfsUpstreamMap/extract_hfs.py",
        "tools/FuzzyUpstreamMap/FuzzyUpstreamMap.csproj",
        "tools/FuzzyUpstreamMap/Program.cs",
        "tools/FuzzyUpstreamMap/extract_fuzzy.py",
        "tools/RapidUpstreamMap/RapidUpstreamMap.csproj",
        "tools/RapidUpstreamMap/Program.cs",
        "tools/RapidUpstreamMap/extract_rapid.py",
        "tools/ShapeUpstreamMap/ShapeUpstreamMap.csproj",
        "tools/ShapeUpstreamMap/Program.cs",
        "tools/ShapeUpstreamMap/extract_shape.py",
        "tools/StructuredLightUpstreamMap/StructuredLightUpstreamMap.csproj",
        "tools/StructuredLightUpstreamMap/Program.cs",
        "tools/StructuredLightUpstreamMap/extract_structured_light.py",
        "tools/SaliencyUpstreamMap/SaliencyUpstreamMap.csproj",
        "tools/SaliencyUpstreamMap/Program.cs",
        "tools/SaliencyUpstreamMap/extract_saliency.py",
        "tools/SurfaceMatchingUpstreamMap/SurfaceMatchingUpstreamMap.csproj",
        "tools/SurfaceMatchingUpstreamMap/Program.cs",
        "tools/SurfaceMatchingUpstreamMap/extract_surface_matching.py",
        "tools/RegUpstreamMap/RegUpstreamMap.csproj",
        "tools/RegUpstreamMap/Program.cs",
        "tools/RegUpstreamMap/extract_reg.py",
        "tools/XStereoUpstreamMap/XStereoUpstreamMap.csproj",
        "tools/XStereoUpstreamMap/Program.cs",
        "tools/XStereoUpstreamMap/extract_xstereo.py",
        "tools/PtCloudUpstreamMap/PtCloudUpstreamMap.csproj",
        "tools/PtCloudUpstreamMap/Program.cs",
        "tools/PtCloudUpstreamMap/extract_ptcloud.py",
        "tools/XObjDetectUpstreamMap/XObjDetectUpstreamMap.csproj",
        "tools/XObjDetectUpstreamMap/Program.cs",
        "tools/XObjDetectUpstreamMap/extract_xobjdetect.py",
        "tools/CodecBenchmark/CodecBenchmark.csproj",
        "tools/CodecBenchmark/Program.cs",
        "scripts/Measure-CodecTypedMatBenchmark.ps1",
        "scripts/Test-CodecBenchmarkHarness.ps1",
        "scripts/Measure-CodecWriterNativeEvidence.ps1",
        "scripts/Test-CodecWriterNativeEvidence.ps1",
        "tools/ScenarioBenchmark/ScenarioBenchmark.csproj",
        "tools/ScenarioBenchmark/Program.cs",
        "scripts/Test-ScenarioBenchmarkHarness.ps1",
        "scripts/Measure-VideoDnnBenchmark.ps1",
        "scripts/Test-ImagePreflightMutationCorpus.ps1",
        "scripts/Measure-ImagePreflightFuzz.ps1",
        "scripts/Test-ImagePreflightFuzzEvidence.ps1",
        "scripts/Measure-CodecNativeMutationEvidence.ps1",
        "scripts/Test-CodecNativeMutationEvidence.ps1",
        "scripts/Test-NativeAotSmoke.ps1",
        "scripts/Measure-NativeAotSmokeEvidence.ps1",
        "scripts/Test-NativeAotSmokeEvidence.ps1",
        "scripts/Test-NativeAotTrimWarningLedger.ps1",
        "scripts/Test-PerformanceBenchmarkEvidence.ps1",
        "scripts/Test-VideoDnnBenchmarkEvidence.ps1",
        "scripts/New-PerformanceRegressionBaseline.ps1",
        "scripts/Test-PerformanceRegressionBaseline.ps1",
        "scripts/Test-TypedMatTargetFrameworks.ps1",
        "scripts/Measure-TypedMatNativeLifetime.ps1",
        "scripts/Test-TypedMatNativeLifetimeEvidence.ps1",
        "scripts/Measure-TypedMatOwnerLifetimeMatrix.ps1",
        "scripts/Test-TypedMatOwnerLifetimeMatrix.ps1",
        "scripts/Test-AsyncVideoOwnerContract.ps1",
        "scripts/Test-DnnDiagnosticsContract.ps1",
        "scripts/Test-ImageFormatPolicy.ps1",
        "scripts/Test-ModernApiPreviewSamples.ps1",
        "scripts/Test-PlatformProbeSample.ps1",
        "scripts/Test-V501ChangeLedger.ps1",
        "scripts/New-UpstreamMapCatalog.ps1",
        "scripts/Test-UpstreamMapCatalog.ps1",
        "scripts/Generate-XPhotoUpstreamMap.ps1",
        "scripts/Test-XPhotoUpstreamMap.ps1",
        "scripts/Generate-XImgProcUpstreamMap.ps1",
        "scripts/Test-XImgProcUpstreamMap.ps1",
        "scripts/Generate-ContribUpstreamMap.ps1",
        "scripts/Generate-OptFlowUpstreamMap.ps1",
        "scripts/Test-OptFlowUpstreamMap.ps1",
        "scripts/Generate-BgSegmUpstreamMap.ps1",
        "scripts/Test-BgSegmUpstreamMap.ps1",
        "scripts/Generate-FaceUpstreamMap.ps1",
        "scripts/Test-FaceUpstreamMap.ps1",
        "scripts/Generate-QualityUpstreamMap.ps1",
        "scripts/Test-QualityUpstreamMap.ps1",
        "scripts/Generate-ImgHashUpstreamMap.ps1",
        "scripts/Test-ImgHashUpstreamMap.ps1",
        "scripts/Generate-LineDescriptorUpstreamMap.ps1",
        "scripts/Test-LineDescriptorUpstreamMap.ps1",
        "scripts/Generate-FreetypeUpstreamMap.ps1",
        "scripts/Test-FreetypeUpstreamMap.ps1",
        "scripts/Generate-AlphaMatUpstreamMap.ps1",
        "scripts/Test-AlphaMatUpstreamMap.ps1",
        "scripts/Generate-IntensityTransformUpstreamMap.ps1",
        "scripts/Test-IntensityTransformUpstreamMap.ps1",
        "scripts/Generate-PlotUpstreamMap.ps1",
        "scripts/Test-PlotUpstreamMap.ps1",
        "scripts/Generate-BioInspiredUpstreamMap.ps1",
        "scripts/Test-BioInspiredUpstreamMap.ps1",
        "scripts/Generate-PhaseUnwrappingUpstreamMap.ps1",
        "scripts/Test-PhaseUnwrappingUpstreamMap.ps1",
        "scripts/Generate-HfsUpstreamMap.ps1",
        "scripts/Test-HfsUpstreamMap.ps1",
        "scripts/Generate-FuzzyUpstreamMap.ps1",
        "scripts/Test-FuzzyUpstreamMap.ps1",
        "scripts/Generate-RapidUpstreamMap.ps1",
        "scripts/Test-RapidUpstreamMap.ps1",
        "scripts/Generate-ShapeUpstreamMap.ps1",
        "scripts/Test-ShapeUpstreamMap.ps1",
        "scripts/Generate-StructuredLightUpstreamMap.ps1",
        "scripts/Test-StructuredLightUpstreamMap.ps1",
        "scripts/Generate-SaliencyUpstreamMap.ps1",
        "scripts/Test-SaliencyUpstreamMap.ps1",
        "scripts/Generate-SurfaceMatchingUpstreamMap.ps1",
        "scripts/Test-SurfaceMatchingUpstreamMap.ps1",
        "scripts/Generate-RegUpstreamMap.ps1",
        "scripts/Test-RegUpstreamMap.ps1",
        "scripts/Generate-XStereoUpstreamMap.ps1",
        "scripts/Test-XStereoUpstreamMap.ps1",
        "scripts/Generate-PtCloudUpstreamMap.ps1",
        "scripts/Test-PtCloudUpstreamMap.ps1",
        "scripts/Generate-XObjDetectUpstreamMap.ps1",
        "scripts/Test-XObjDetectUpstreamMap.ps1",
        "scripts/Initialize-UpstreamMapSourceEvidence.ps1",
        "tools/TypedMatBenchmark/TypedMatBenchmark.csproj",
        "tools/TypedMatBenchmark/Program.cs",
        "scripts/Test-TypedMatBenchmarkHarness.ps1",
        "tools/AotSmoke/AotSmoke.csproj",
        "tools/AotSmoke/Program.cs"
    )
    return @(Get-OrdinalSorted -Values $paths)
}

function Test-Record {
    param(
        [Parameter(Mandatory)][object]$Record,
        [Parameter(Mandatory)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$List,
        [Parameter(Mandatory)][string]$ExpectedSourceHash
    )

    $required = @("SchemaVersion","RecordKind","CandidateId","SourceIdentity","OpenCvRevision","SourceSet","PackageMatrix","LifecycleRefreshMatrix","SupportContract","ApiAbiBaseline","EvidenceReferences","LocalValidation","Signing","Sbom","Approval","PublicFeed","Rollback","ReleaseApproval","ExternalBlockers","PrivateKeyMaterialPresent","SecretMaterialPresent","Deterministic")
    foreach ($field in $required) {
        Assert-True -List $List -Condition ($null -ne $Record.PSObject.Properties[$field]) -Issue "Final closeout record is missing required field" -Text $field
    }
    if ($null -eq $Record.SourceSet -or $null -eq $Record.LifecycleRefreshMatrix -or $null -eq $Record.SupportContract -or $null -eq $Record.ApiAbiBaseline -or $null -eq $Record.LocalValidation) { return }

    Assert-True -List $List -Condition ($Record.SchemaVersion -eq 2 -and $Record.RecordKind -eq "local-release-candidate-closeout" -and $Record.OpenCvRevision -eq "5.0.0") -Issue "Final closeout record identity drifted"
    $expectedCandidateId = "local-closeout/sha256/$(([string]$Record.SourceSet.Sha256).Substring(0, 16))"
    $expectedSourceIdentity = "sha256:$([string]$Record.SourceSet.Sha256)"
    Assert-True -List $List -Condition ($Record.CandidateId -eq $expectedCandidateId -and $Record.SourceIdentity -eq $expectedSourceIdentity) -Issue "Final closeout candidate identity is malformed or is not derived from the source-set digest"
    Assert-True -List $List -Condition ([int]$Record.SourceSet.FileCount -gt 0 -and $Record.SourceSet.Sha256 -match "^[0-9a-f]{64}$" -and $Record.SourceSet.HashPolicy -like "UTF-8 text normalized*") -Issue "Final closeout source-set digest is missing or malformed"
    Assert-True -List $List -Condition ($Record.SourceSet.Sha256 -eq $ExpectedSourceHash) -Issue "Final closeout source-set digest drifted" -Text "expected=$ExpectedSourceHash actual=$($Record.SourceSet.Sha256)"

    Assert-True -List $List -Condition ([int]$Record.PackageMatrix.RidCount -gt 0 -and [int]$Record.PackageMatrix.ProfileCount -eq 2 -and [int]$Record.PackageMatrix.EntryCount -eq 34 -and $Record.PackageMatrix.Sha256 -match "^[0-9a-f]{64}$") -Issue "Final closeout package matrix evidence drifted"
    $lifecycleRefreshMatrixPath = Join-Path $repo 'packaging/runtime/runtime-lifecycle-refresh-matrix.json'
    $lifecycleRefreshMatrixSha256 = (Get-FileHash -LiteralPath $lifecycleRefreshMatrixPath -Algorithm SHA256).Hash.ToLowerInvariant()
    Assert-True -List $List -Condition (
        $Record.LifecycleRefreshMatrix.Path -eq 'packaging/runtime/runtime-lifecycle-refresh-matrix.json' -and
        $Record.LifecycleRefreshMatrix.Sha256 -eq $lifecycleRefreshMatrixSha256 -and
        $Record.LifecycleRefreshMatrix.Status -eq 'candidate-only' -and
        [int]$Record.LifecycleRefreshMatrix.RidCount -eq 2 -and
        [int]$Record.LifecycleRefreshMatrix.ProfileCount -eq 2 -and
        [int]$Record.LifecycleRefreshMatrix.TargetCount -eq 4 -and
        -not [bool]$Record.LifecycleRefreshMatrix.PublicationAllowed
    ) -Issue 'Final closeout lifecycle refresh matrix evidence drifted'
    Assert-True -List $List -Condition ($Record.SupportContract.MatrixEntryCount -eq 34 -and $Record.SupportContract.RealSupportCount -eq 25 -and $Record.SupportContract.CompatibilityOnlyCount -eq 4 -and $Record.SupportContract.PendingSupportCount -eq 4 -and $Record.SupportContract.ExcludedSupportCount -eq 1 -and $Record.SupportContract.OutsideMatrixCount -eq 1 -and $Record.SupportContract.LifecycleRefreshCandidateCount -eq 2 -and $Record.SupportContract.WinX86FullStatus -eq "real-supported" -and $Record.SupportContract.WinX86MiniStatus -eq "excluded" -and -not [bool]$Record.SupportContract.PackageSurfaceDefinesSupport) -Issue "Final closeout support partition or policy drifted"
    $lifecycleCandidates = @($Record.SupportContract.LifecycleRefreshCandidates)
    $expectedLifecycleCandidateDigests = @{
        'alpine.3.23-x64' = 'alpine:3.23@sha256:fd791d74b68913cbb027c6546007b3f0d3bc45125f797758156952bc2d6daf40'
        'fedora.44-x64' = 'fedora:44@sha256:43b29f65a41eb9c35e1cd5323e3bdf3b655c2357a9f4f1ff2f9c2798e5045d80'
    }
    foreach ($rid in @('alpine.3.23-x64', 'fedora.44-x64')) {
        $candidate = @($lifecycleCandidates | Where-Object { [string]$_.rid -ceq $rid })
        Assert-True -List $List -Condition ($candidate.Count -eq 1 -and [string]$candidate[0].status -ceq 'lifecycle-refresh-pending' -and [string]$candidate[0].containerImage -ceq $expectedLifecycleCandidateDigests[$rid]) -Issue 'Final closeout lifecycle refresh candidate drifted' -Text $rid
    }
    $hostedEvidence = $Record.SupportContract.HostedPromotionEvidence
    Assert-True -List $List -Condition ($null -ne $hostedEvidence -and $hostedEvidence.target -eq 'win-x86/full' -and $hostedEvidence.status -eq 'verified-hosted-evidence' -and $hostedEvidence.sourceCommit -eq '7f53e03e7d6ad5839711ba5ea32a0fcc02d8d5b8' -and [long]$hostedEvidence.runtimeInputRunId -eq 31162854992 -and [long]$hostedEvidence.packRunId -eq 31171822232 -and [long]$hostedEvidence.consumerRunId -eq 31171822232) -Issue 'Final closeout Windows x86 hosted promotion evidence drifted'
    Assert-True -List $List -Condition ($hostedEvidence.hostedCloseoutSha256 -eq '61f3ce0263fa41126c7ac857cde56ce6096147f0a1f9a3f3fa499fab1478bd81' -and $hostedEvidence.artifactDigestAuditSha256 -eq 'c0bf6886787fbbc105390ec8861acd6d608b60e9b86c63aef2c7456d436d9ded' -and $hostedEvidence.hostedPackageManifestSha256 -eq 'eedb212724a7f176bc913cc92b09ca086a4d7129f069a964c3195a3b8557c353' -and $hostedEvidence.hostedChangeControlSha256 -eq '69b1444692d389bb3452575a494035faddf5eb4c2f111bf3b60a686986841332') -Issue 'Final closeout Windows x86 hosted audit hashes drifted'

    Assert-True -List $List -Condition ($Record.ApiAbiBaseline.Managed.Sha256 -eq "d8de86430b08e158f0850c6a985c1b758b75d66296641617aeb58ab60cba1f3f" -and $Record.ApiAbiBaseline.Managed.TypeCount -eq 647 -and $Record.ApiAbiBaseline.Managed.MemberCount -eq 7010 -and $Record.ApiAbiBaseline.Managed.NamespaceCount -eq 41 -and $Record.ApiAbiBaseline.Managed.TargetFramework -eq "net8.0") -Issue "Final closeout managed API baseline evidence drifted"
    Assert-True -List $List -Condition ($Record.ApiAbiBaseline.NativeFull.Sha256 -eq "991e8b88349bc012805a2506c89681a572cc650f0ea90fb9d3f0d37de7e5ccb9" -and $Record.ApiAbiBaseline.NativeFull.FunctionCount -eq 2669 -and $Record.ApiAbiBaseline.NativeMini.Sha256 -eq "aac22280e4b88aa45bbc3710ecf0633ba0658e0ba40a0c80a031bb1cf287a9ec" -and $Record.ApiAbiBaseline.NativeMini.FunctionCount -eq 527) -Issue "Final closeout native ABI baseline evidence drifted"
    Assert-True -List $List -Condition ($Record.ApiAbiBaseline.NativeManagedBindingMap.Sha256 -eq "18f1b48a44860e06a1179db7020bd3dc93dd4b60c18cc3f1415b05a824d22733" -and $Record.ApiAbiBaseline.NativeManagedBindingMap.NativeFunctionCount -eq 2669 -and $Record.ApiAbiBaseline.NativeManagedBindingMap.ManagedBoundCount -eq 2669 -and $Record.ApiAbiBaseline.NativeManagedBindingMap.UnboundCount -eq 0 -and $Record.ApiAbiBaseline.NativeManagedBindingMap.ManagedOnlyCount -eq 0) -Issue "Final closeout native-to-managed binding evidence drifted"
    Assert-True -List $List -Condition ($Record.ApiAbiBaseline.ImgProcUpstreamMap.Sha256 -eq "5b4bf38bfb2dd40868d8631b8d527bed1290c51f6a9f94bebc316f2526868b6a" -and $Record.ApiAbiBaseline.ImgProcUpstreamMap.DeclarationCount -eq 203 -and $Record.ApiAbiBaseline.ImgProcUpstreamMap.CallableCount -eq 167 -and $Record.ApiAbiBaseline.ImgProcUpstreamMap.ImplementedCount -eq 161 -and $Record.ApiAbiBaseline.ImgProcUpstreamMap.MissingCount -eq 0 -and $Record.ApiAbiBaseline.ImgProcUpstreamMap.IntentionallyOmittedCount -eq 6 -and $Record.ApiAbiBaseline.ImgProcUpstreamMap.SelectedFamilyCount -eq 8 -and $Record.ApiAbiBaseline.ImgProcUpstreamMap.SelectedDeclarationCount -eq 90 -and $Record.ApiAbiBaseline.ImgProcUpstreamMap.ManagedPublicMemberAdditionCount -eq 174 -and -not [bool]$Record.ApiAbiBaseline.ImgProcUpstreamMap.RepositoryWideParityClaimed) -Issue "Final closeout ImgProc upstream-map evidence drifted"
    Assert-True -List $List -Condition ($Record.ApiAbiBaseline.ImgCodecsUpstreamMap.Sha256 -eq "fedb1e9cc6713238cec6043af94b55d346ecdcf568b12c1da1b88ab49ae761f5" -and $Record.ApiAbiBaseline.ImgCodecsUpstreamMap.DeclarationCount -eq 39 -and $Record.ApiAbiBaseline.ImgCodecsUpstreamMap.CallableCount -eq 22 -and $Record.ApiAbiBaseline.ImgCodecsUpstreamMap.ImplementedCount -eq 22 -and $Record.ApiAbiBaseline.ImgCodecsUpstreamMap.MissingCount -eq 0 -and $Record.ApiAbiBaseline.ImgCodecsUpstreamMap.IntentionallyOmittedCount -eq 0 -and $Record.ApiAbiBaseline.ImgCodecsUpstreamMap.SelectedFamilyCount -eq 5 -and $Record.ApiAbiBaseline.ImgCodecsUpstreamMap.SelectedDeclarationCount -eq 40 -and $Record.ApiAbiBaseline.ImgCodecsUpstreamMap.ManagedPublicTypeAdditionCount -eq 16 -and $Record.ApiAbiBaseline.ImgCodecsUpstreamMap.ManagedPublicMemberAdditionCount -eq 168 -and -not [bool]$Record.ApiAbiBaseline.ImgCodecsUpstreamMap.RepositoryWideParityClaimed) -Issue "Final closeout ImgCodecs upstream-map evidence drifted"
    Assert-True -List $List -Condition ($Record.ApiAbiBaseline.VideoIOUpstreamMap.Sha256 -eq "b7605406830526c0b1afc4afc674aaa4a0cf307f1cb3652a489f8545a5cb8de8" -and $Record.ApiAbiBaseline.VideoIOUpstreamMap.RegistrySurfaceSha256 -eq "b629bec86d9bd6dbf428a4b524fd473c01454e6a9ece39aaadf672b4f93373f8" -and $Record.ApiAbiBaseline.VideoIOUpstreamMap.DeclarationCount -eq 71 -and $Record.ApiAbiBaseline.VideoIOUpstreamMap.CallableCount -eq 40 -and $Record.ApiAbiBaseline.VideoIOUpstreamMap.ImplementedCount -eq 40 -and $Record.ApiAbiBaseline.VideoIOUpstreamMap.MissingCount -eq 0 -and $Record.ApiAbiBaseline.VideoIOUpstreamMap.IntentionallyOmittedCount -eq 0 -and $Record.ApiAbiBaseline.VideoIOUpstreamMap.RegistryOperationCount -eq 12 -and -not [bool]$Record.ApiAbiBaseline.VideoIOUpstreamMap.RepositoryWideParityClaimed) -Issue "Final closeout VideoIO upstream-map evidence drifted"
    Assert-True -List $List -Condition ($Record.ApiAbiBaseline.Calib3DUpstreamMap.Sha256 -eq "d171ffc695c3eae70ffe415a3ef75c83cee21cdfe0add3d990b8304f17cb5ed5" -and $Record.ApiAbiBaseline.Calib3DUpstreamMap.DeclarationCount -eq 194 -and $Record.ApiAbiBaseline.Calib3DUpstreamMap.CallableCount -eq 167 -and $Record.ApiAbiBaseline.Calib3DUpstreamMap.ImplementedCount -eq 167 -and $Record.ApiAbiBaseline.Calib3DUpstreamMap.MissingCount -eq 0 -and $Record.ApiAbiBaseline.Calib3DUpstreamMap.IntentionallyOmittedCount -eq 0 -and $Record.ApiAbiBaseline.Calib3DUpstreamMap.SourceHeaderCount -eq 4 -and $Record.ApiAbiBaseline.Calib3DUpstreamMap.SelectedFamilyCount -eq 11 -and $Record.ApiAbiBaseline.Calib3DUpstreamMap.SelectedDeclarationCount -eq 194 -and $Record.ApiAbiBaseline.Calib3DUpstreamMap.ManagedPublicTypeAdditionCount -eq 12 -and $Record.ApiAbiBaseline.Calib3DUpstreamMap.ManagedPublicMemberAdditionCount -eq 120 -and -not [bool]$Record.ApiAbiBaseline.Calib3DUpstreamMap.RepositoryWideParityClaimed) -Issue "Final closeout Calib3D upstream-map evidence drifted"
    Assert-True -List $List -Condition ($Record.ApiAbiBaseline.CoreUpstreamMap.Sha256 -eq "cfaaed815e4c7f0173739f17fdb8b13fc5f995cee7c26542b52640a9fefcb1f1" -and $Record.ApiAbiBaseline.CoreUpstreamMap.DeclarationCount -eq 258 -and $Record.ApiAbiBaseline.CoreUpstreamMap.CallableCount -eq 215 -and $Record.ApiAbiBaseline.CoreUpstreamMap.ImplementedCount -eq 176 -and $Record.ApiAbiBaseline.CoreUpstreamMap.MissingCount -eq 0 -and $Record.ApiAbiBaseline.CoreUpstreamMap.IntentionallyOmittedCount -eq 29 -and $Record.ApiAbiBaseline.CoreUpstreamMap.UnsupportedCount -eq 5 -and $Record.ApiAbiBaseline.CoreUpstreamMap.UpstreamConditionalCount -eq 5 -and $Record.ApiAbiBaseline.CoreUpstreamMap.SourceHeaderCount -eq 11 -and $Record.ApiAbiBaseline.CoreUpstreamMap.SelectedFamilyCount -eq 4 -and $Record.ApiAbiBaseline.CoreUpstreamMap.SelectedDeclarationCount -eq 108 -and $Record.ApiAbiBaseline.CoreUpstreamMap.ManagedPublicTypeAdditionCount -eq 11 -and $Record.ApiAbiBaseline.CoreUpstreamMap.ManagedPublicMemberAdditionCount -eq 226 -and -not [bool]$Record.ApiAbiBaseline.CoreUpstreamMap.RepositoryWideParityClaimed) -Issue "Final closeout Core upstream-map evidence drifted"
    Assert-True -List $List -Condition ($Record.ApiAbiBaseline.DnnUpstreamMap.Sha256 -eq "2c7674703c8a5104839e0bc80e610e2a7939e6b8842b0df742ad226de05a0d19" -and $Record.ApiAbiBaseline.DnnUpstreamMap.DeclarationCount -eq 182 -and $Record.ApiAbiBaseline.DnnUpstreamMap.CallableCount -eq 159 -and $Record.ApiAbiBaseline.DnnUpstreamMap.ImplementedCount -eq 74 -and $Record.ApiAbiBaseline.DnnUpstreamMap.MissingCount -eq 0 -and $Record.ApiAbiBaseline.DnnUpstreamMap.IntentionallyOmittedCount -eq 77 -and $Record.ApiAbiBaseline.DnnUpstreamMap.UnsupportedCount -eq 2 -and $Record.ApiAbiBaseline.DnnUpstreamMap.UpstreamConditionalCount -eq 6 -and $Record.ApiAbiBaseline.DnnUpstreamMap.SourceHeaderCount -eq 3 -and $Record.ApiAbiBaseline.DnnUpstreamMap.SelectedFamilyCount -eq 5 -and $Record.ApiAbiBaseline.DnnUpstreamMap.SelectedDeclarationCount -eq 85 -and $Record.ApiAbiBaseline.DnnUpstreamMap.ManagedPublicTypeAdditionCount -eq 12 -and $Record.ApiAbiBaseline.DnnUpstreamMap.ManagedPublicMemberAdditionCount -eq 106 -and -not [bool]$Record.ApiAbiBaseline.DnnUpstreamMap.RepositoryWideParityClaimed) -Issue "Final closeout DNN upstream-map evidence drifted"
    Assert-True -List $List -Condition ($Record.ApiAbiBaseline.DnnSuperresUpstreamMap.Sha256 -eq "5d8b06d467c592322f1c63513600b0fe0e53904c3978573cf95b55dc90667f9f" -and $Record.ApiAbiBaseline.DnnSuperresUpstreamMap.DeclarationCount -eq 10 -and $Record.ApiAbiBaseline.DnnSuperresUpstreamMap.CallableCount -eq 9 -and $Record.ApiAbiBaseline.DnnSuperresUpstreamMap.ImplementedCount -eq 0 -and $Record.ApiAbiBaseline.DnnSuperresUpstreamMap.MissingCount -eq 0 -and $Record.ApiAbiBaseline.DnnSuperresUpstreamMap.IntentionallyOmittedCount -eq 9 -and $Record.ApiAbiBaseline.DnnSuperresUpstreamMap.UnsupportedCount -eq 0 -and $Record.ApiAbiBaseline.DnnSuperresUpstreamMap.UpstreamConditionalCount -eq 0 -and $Record.ApiAbiBaseline.DnnSuperresUpstreamMap.SourceHeaderCount -eq 1 -and $Record.ApiAbiBaseline.DnnSuperresUpstreamMap.SelectedFamilyCount -eq 1 -and $Record.ApiAbiBaseline.DnnSuperresUpstreamMap.SelectedDeclarationCount -eq 0 -and $Record.ApiAbiBaseline.DnnSuperresUpstreamMap.ManagedPublicTypeAdditionCount -eq 0 -and $Record.ApiAbiBaseline.DnnSuperresUpstreamMap.ManagedPublicMemberAdditionCount -eq 0 -and $Record.ApiAbiBaseline.DnnSuperresUpstreamMap.NativeEntrypointAdditionCount -eq 0 -and -not [bool]$Record.ApiAbiBaseline.DnnSuperresUpstreamMap.RepositoryWideParityClaimed) -Issue "Final closeout DNN Super Resolution upstream-map evidence drifted"
    Assert-True -List $List -Condition ($Record.ApiAbiBaseline.WechatQRCodeUpstreamMap.Sha256 -eq "64a6a4fca4b75318a4cdf72784548c302872728033045be09068fd2f5957920c" -and $Record.ApiAbiBaseline.WechatQRCodeUpstreamMap.DeclarationCount -eq 5 -and $Record.ApiAbiBaseline.WechatQRCodeUpstreamMap.CallableCount -eq 4 -and $Record.ApiAbiBaseline.WechatQRCodeUpstreamMap.ImplementedCount -eq 0 -and $Record.ApiAbiBaseline.WechatQRCodeUpstreamMap.MissingCount -eq 0 -and $Record.ApiAbiBaseline.WechatQRCodeUpstreamMap.IntentionallyOmittedCount -eq 4 -and $Record.ApiAbiBaseline.WechatQRCodeUpstreamMap.UnsupportedCount -eq 0 -and $Record.ApiAbiBaseline.WechatQRCodeUpstreamMap.UpstreamConditionalCount -eq 0 -and $Record.ApiAbiBaseline.WechatQRCodeUpstreamMap.SourceHeaderCount -eq 1 -and $Record.ApiAbiBaseline.WechatQRCodeUpstreamMap.SelectedFamilyCount -eq 1 -and $Record.ApiAbiBaseline.WechatQRCodeUpstreamMap.SelectedDeclarationCount -eq 0 -and $Record.ApiAbiBaseline.WechatQRCodeUpstreamMap.ManagedPublicTypeAdditionCount -eq 0 -and $Record.ApiAbiBaseline.WechatQRCodeUpstreamMap.ManagedPublicMemberAdditionCount -eq 0 -and $Record.ApiAbiBaseline.WechatQRCodeUpstreamMap.NativeEntrypointAdditionCount -eq 0 -and -not [bool]$Record.ApiAbiBaseline.WechatQRCodeUpstreamMap.RepositoryWideParityClaimed) -Issue "Final closeout WeChat QRCode upstream-map evidence drifted"
    Assert-True -List $List -Condition ($Record.ApiAbiBaseline.Cnn3DObjUpstreamMap.Sha256 -eq "7f764a01806a9e8c730a2f0966e5b8156aeaa77d525191d87f1da04d1b7f170e" -and $Record.ApiAbiBaseline.Cnn3DObjUpstreamMap.DeclarationCount -eq 7 -and $Record.ApiAbiBaseline.Cnn3DObjUpstreamMap.CallableCount -eq 5 -and $Record.ApiAbiBaseline.Cnn3DObjUpstreamMap.ImplementedCount -eq 0 -and $Record.ApiAbiBaseline.Cnn3DObjUpstreamMap.MissingCount -eq 0 -and $Record.ApiAbiBaseline.Cnn3DObjUpstreamMap.IntentionallyOmittedCount -eq 5 -and $Record.ApiAbiBaseline.Cnn3DObjUpstreamMap.UnsupportedCount -eq 0 -and $Record.ApiAbiBaseline.Cnn3DObjUpstreamMap.UpstreamConditionalCount -eq 0 -and $Record.ApiAbiBaseline.Cnn3DObjUpstreamMap.SourceHeaderCount -eq 1 -and $Record.ApiAbiBaseline.Cnn3DObjUpstreamMap.SelectedFamilyCount -eq 1 -and $Record.ApiAbiBaseline.Cnn3DObjUpstreamMap.SelectedDeclarationCount -eq 0 -and $Record.ApiAbiBaseline.Cnn3DObjUpstreamMap.ManagedPublicTypeAdditionCount -eq 0 -and $Record.ApiAbiBaseline.Cnn3DObjUpstreamMap.ManagedPublicMemberAdditionCount -eq 0 -and $Record.ApiAbiBaseline.Cnn3DObjUpstreamMap.NativeEntrypointAdditionCount -eq 0 -and -not [bool]$Record.ApiAbiBaseline.Cnn3DObjUpstreamMap.RepositoryWideParityClaimed) -Issue "Final closeout Cnn3DObj upstream-map evidence drifted"
    Assert-True -List $List -Condition ($Record.ApiAbiBaseline.SignalUpstreamMap.Sha256 -eq "88662b9743a6825cad8695fe8fc788f99196c0430dddfd8b09f9b8908bfe7678" -and $Record.ApiAbiBaseline.SignalUpstreamMap.DeclarationCount -eq 1 -and $Record.ApiAbiBaseline.SignalUpstreamMap.CallableCount -eq 1 -and $Record.ApiAbiBaseline.SignalUpstreamMap.ImplementedCount -eq 0 -and $Record.ApiAbiBaseline.SignalUpstreamMap.MissingCount -eq 0 -and $Record.ApiAbiBaseline.SignalUpstreamMap.IntentionallyOmittedCount -eq 1 -and $Record.ApiAbiBaseline.SignalUpstreamMap.UnsupportedCount -eq 0 -and $Record.ApiAbiBaseline.SignalUpstreamMap.UpstreamConditionalCount -eq 0 -and $Record.ApiAbiBaseline.SignalUpstreamMap.SourceHeaderCount -eq 1 -and $Record.ApiAbiBaseline.SignalUpstreamMap.SelectedFamilyCount -eq 1 -and $Record.ApiAbiBaseline.SignalUpstreamMap.SelectedDeclarationCount -eq 0 -and $Record.ApiAbiBaseline.SignalUpstreamMap.ManagedPublicTypeAdditionCount -eq 0 -and $Record.ApiAbiBaseline.SignalUpstreamMap.ManagedPublicMemberAdditionCount -eq 0 -and $Record.ApiAbiBaseline.SignalUpstreamMap.NativeEntrypointAdditionCount -eq 0 -and -not [bool]$Record.ApiAbiBaseline.SignalUpstreamMap.RepositoryWideParityClaimed) -Issue "Final closeout Signal upstream-map evidence drifted"
    Assert-True -List $List -Condition ($Record.ApiAbiBaseline.DpmUpstreamMap.Sha256 -eq "b0e24a5485d5b3dbeb6add5228bd2ed450d91bcec4df4079fa6fddde6ea274d7" -and $Record.ApiAbiBaseline.DpmUpstreamMap.DeclarationCount -eq 2 -and $Record.ApiAbiBaseline.DpmUpstreamMap.CallableCount -eq 0 -and $Record.ApiAbiBaseline.DpmUpstreamMap.ImplementedCount -eq 0 -and $Record.ApiAbiBaseline.DpmUpstreamMap.MissingCount -eq 0 -and $Record.ApiAbiBaseline.DpmUpstreamMap.IntentionallyOmittedCount -eq 0 -and $Record.ApiAbiBaseline.DpmUpstreamMap.SourceHeaderCount -eq 1 -and $Record.ApiAbiBaseline.DpmUpstreamMap.SelectedFamilyCount -eq 1 -and $Record.ApiAbiBaseline.DpmUpstreamMap.SelectedDeclarationCount -eq 0 -and $Record.ApiAbiBaseline.DpmUpstreamMap.ManagedPublicTypeAdditionCount -eq 0 -and $Record.ApiAbiBaseline.DpmUpstreamMap.ManagedPublicMemberAdditionCount -eq 0 -and $Record.ApiAbiBaseline.DpmUpstreamMap.NativeEntrypointAdditionCount -eq 0 -and -not [bool]$Record.ApiAbiBaseline.DpmUpstreamMap.RepositoryWideParityClaimed) -Issue "Final closeout DPM upstream-map evidence drifted"
    Assert-True -List $List -Condition ($Record.ApiAbiBaseline.HdfUpstreamMap.Sha256 -eq "73c94abdcc189dca33b3b0e5eb191269ec5a34a90dd8b86c5d95a9edb626914a" -and $Record.ApiAbiBaseline.HdfUpstreamMap.DeclarationCount -eq 38 -and $Record.ApiAbiBaseline.HdfUpstreamMap.CallableCount -eq 37 -and $Record.ApiAbiBaseline.HdfUpstreamMap.ImplementedCount -eq 0 -and $Record.ApiAbiBaseline.HdfUpstreamMap.MissingCount -eq 0 -and $Record.ApiAbiBaseline.HdfUpstreamMap.IntentionallyOmittedCount -eq 37 -and $Record.ApiAbiBaseline.HdfUpstreamMap.UnsupportedCount -eq 0 -and $Record.ApiAbiBaseline.HdfUpstreamMap.UpstreamConditionalCount -eq 0 -and $Record.ApiAbiBaseline.HdfUpstreamMap.SourceHeaderCount -eq 1 -and $Record.ApiAbiBaseline.HdfUpstreamMap.SelectedFamilyCount -eq 1 -and $Record.ApiAbiBaseline.HdfUpstreamMap.SelectedDeclarationCount -eq 0 -and $Record.ApiAbiBaseline.HdfUpstreamMap.ManagedPublicTypeAdditionCount -eq 0 -and $Record.ApiAbiBaseline.HdfUpstreamMap.ManagedPublicMemberAdditionCount -eq 0 -and $Record.ApiAbiBaseline.HdfUpstreamMap.NativeEntrypointAdditionCount -eq 0 -and $Record.ApiAbiBaseline.HdfUpstreamMap.ExternalDataDependencyCount -eq 1 -and -not [bool]$Record.ApiAbiBaseline.HdfUpstreamMap.RepositoryWideParityClaimed) -Issue "Final closeout HDF upstream-map evidence drifted"
    Assert-True -List $List -Condition ($Record.ApiAbiBaseline.TextUpstreamMap.Sha256 -eq "1b804578982bb04e1d5728850db13de25dfd5e4d8fe6155f6f4302a5211a2513" -and $Record.ApiAbiBaseline.TextUpstreamMap.DeclarationCount -eq 47 -and $Record.ApiAbiBaseline.TextUpstreamMap.CallableCount -eq 30 -and $Record.ApiAbiBaseline.TextUpstreamMap.ImplementedCount -eq 0 -and $Record.ApiAbiBaseline.TextUpstreamMap.MissingCount -eq 0 -and $Record.ApiAbiBaseline.TextUpstreamMap.IntentionallyOmittedCount -eq 30 -and $Record.ApiAbiBaseline.TextUpstreamMap.UnsupportedCount -eq 0 -and $Record.ApiAbiBaseline.TextUpstreamMap.UpstreamConditionalCount -eq 0 -and $Record.ApiAbiBaseline.TextUpstreamMap.SourceHeaderCount -eq 4 -and $Record.ApiAbiBaseline.TextUpstreamMap.SelectedFamilyCount -eq 1 -and $Record.ApiAbiBaseline.TextUpstreamMap.SelectedDeclarationCount -eq 0 -and $Record.ApiAbiBaseline.TextUpstreamMap.ManagedPublicTypeAdditionCount -eq 0 -and $Record.ApiAbiBaseline.TextUpstreamMap.ManagedPublicMemberAdditionCount -eq 0 -and $Record.ApiAbiBaseline.TextUpstreamMap.NativeEntrypointAdditionCount -eq 0 -and $Record.ApiAbiBaseline.TextUpstreamMap.ExternalDataDependencyCount -eq 2 -and -not [bool]$Record.ApiAbiBaseline.TextUpstreamMap.RepositoryWideParityClaimed) -Issue "Final closeout Text upstream-map evidence drifted"
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
    Assert-True -List $List -Condition ($Record.ApiAbiBaseline.SurfaceMatchingUpstreamMap.Sha256 -eq "2f7f088ed4de9c554f46a79a7e08bb5deb5b11dbfd3e9480a6b5ff9ba2e7cbd2" -and $Record.ApiAbiBaseline.SurfaceMatchingUpstreamMap.RawSha256 -eq "96b62399b1af18fa485faeb452a177651b094e62dee03ec208823c0eec12603f" -and $Record.ApiAbiBaseline.SurfaceMatchingUpstreamMap.DeclarationCount -eq 27 -and $Record.ApiAbiBaseline.SurfaceMatchingUpstreamMap.CallableCount -eq 23 -and $Record.ApiAbiBaseline.SurfaceMatchingUpstreamMap.ImplementedCount -eq 7 -and $Record.ApiAbiBaseline.SurfaceMatchingUpstreamMap.MissingCount -eq 0 -and $Record.ApiAbiBaseline.SurfaceMatchingUpstreamMap.IntentionallyOmittedCount -eq 16 -and $Record.ApiAbiBaseline.SurfaceMatchingUpstreamMap.UnsupportedCount -eq 0 -and $Record.ApiAbiBaseline.SurfaceMatchingUpstreamMap.UpstreamConditionalCount -eq 0 -and $Record.ApiAbiBaseline.SurfaceMatchingUpstreamMap.SelectedFamilyCount -eq 1 -and $Record.ApiAbiBaseline.SurfaceMatchingUpstreamMap.SelectedDeclarationCount -eq 7 -and $Record.ApiAbiBaseline.SurfaceMatchingUpstreamMap.ManagedPublicTypeAdditionCount -eq 0 -and $Record.ApiAbiBaseline.SurfaceMatchingUpstreamMap.ManagedPublicMemberAdditionCount -eq 0 -and $Record.ApiAbiBaseline.SurfaceMatchingUpstreamMap.NativeEntrypointAdditionCount -eq 6 -and -not [bool]$Record.ApiAbiBaseline.SurfaceMatchingUpstreamMap.RepositoryWideParityClaimed) -Issue "Final closeout Surface Matching upstream-map evidence drifted"
    Assert-True -List $List -Condition ($Record.ApiAbiBaseline.RegUpstreamMap.Sha256 -eq "1dd55da90fd1d92c14ba492db33fbd727e97141180d1e4caff1a62f4ccec855f" -and $Record.ApiAbiBaseline.RegUpstreamMap.RawSha256 -eq "5b13f257cb33678389d15ea51f7650657be27e973fce347bda357bb3d016faf6" -and $Record.ApiAbiBaseline.RegUpstreamMap.DeclarationCount -eq 63 -and $Record.ApiAbiBaseline.RegUpstreamMap.CallableCount -eq 51 -and $Record.ApiAbiBaseline.RegUpstreamMap.ImplementedCount -eq 48 -and $Record.ApiAbiBaseline.RegUpstreamMap.MissingCount -eq 0 -and $Record.ApiAbiBaseline.RegUpstreamMap.IntentionallyOmittedCount -eq 3 -and $Record.ApiAbiBaseline.RegUpstreamMap.UnsupportedCount -eq 0 -and $Record.ApiAbiBaseline.RegUpstreamMap.UpstreamConditionalCount -eq 0 -and $Record.ApiAbiBaseline.RegUpstreamMap.SourceHeaderCount -eq 11 -and $Record.ApiAbiBaseline.RegUpstreamMap.SelectedFamilyCount -eq 1 -and $Record.ApiAbiBaseline.RegUpstreamMap.SelectedDeclarationCount -eq 48 -and $Record.ApiAbiBaseline.RegUpstreamMap.ManagedPublicTypeAdditionCount -eq 0 -and $Record.ApiAbiBaseline.RegUpstreamMap.ManagedPublicMemberAdditionCount -eq 0 -and $Record.ApiAbiBaseline.RegUpstreamMap.NativeEntrypointAdditionCount -eq 20 -and -not [bool]$Record.ApiAbiBaseline.RegUpstreamMap.RepositoryWideParityClaimed) -Issue "Final closeout Reg upstream-map evidence drifted"
    Assert-True -List $List -Condition ($Record.ApiAbiBaseline.XStereoUpstreamMap.Sha256 -eq "5878d163e02c9a20c65df5745e145184b8340ca1cd8fe95fe06a7dc3c28673a7" -and $Record.ApiAbiBaseline.XStereoUpstreamMap.DeclarationCount -eq 14 -and $Record.ApiAbiBaseline.XStereoUpstreamMap.CallableCount -eq 10 -and $Record.ApiAbiBaseline.XStereoUpstreamMap.ImplementedCount -eq 8 -and $Record.ApiAbiBaseline.XStereoUpstreamMap.MissingCount -eq 0 -and $Record.ApiAbiBaseline.XStereoUpstreamMap.IntentionallyOmittedCount -eq 2 -and $Record.ApiAbiBaseline.XStereoUpstreamMap.SourceHeaderCount -eq 2 -and $Record.ApiAbiBaseline.XStereoUpstreamMap.SelectedFamilyCount -eq 1 -and $Record.ApiAbiBaseline.XStereoUpstreamMap.SelectedDeclarationCount -eq 8 -and $Record.ApiAbiBaseline.XStereoUpstreamMap.NativeEntrypointAdditionCount -eq 10 -and -not [bool]$Record.ApiAbiBaseline.XStereoUpstreamMap.RepositoryWideParityClaimed) -Issue "Final closeout XStereo upstream-map evidence drifted"
    Assert-True -List $List -Condition ($Record.ApiAbiBaseline.PtCloudUpstreamMap.Sha256 -eq "ad6cb09f4de25b611e255fa5fa61b02ceea19cb8ac4d8aec34b1923452d0bc4c" -and $Record.ApiAbiBaseline.PtCloudUpstreamMap.DeclarationCount -eq 22 -and $Record.ApiAbiBaseline.PtCloudUpstreamMap.CallableCount -eq 19 -and $Record.ApiAbiBaseline.PtCloudUpstreamMap.ImplementedCount -eq 19 -and $Record.ApiAbiBaseline.PtCloudUpstreamMap.MissingCount -eq 0 -and $Record.ApiAbiBaseline.PtCloudUpstreamMap.IntentionallyOmittedCount -eq 0 -and $Record.ApiAbiBaseline.PtCloudUpstreamMap.SourceHeaderCount -eq 1 -and $Record.ApiAbiBaseline.PtCloudUpstreamMap.ExcludedPublicHeaderCount -eq 1 -and $Record.ApiAbiBaseline.PtCloudUpstreamMap.SelectedFamilyCount -eq 1 -and $Record.ApiAbiBaseline.PtCloudUpstreamMap.SelectedDeclarationCount -eq 19 -and $Record.ApiAbiBaseline.PtCloudUpstreamMap.NativeEntrypointAdditionCount -eq 13 -and -not [bool]$Record.ApiAbiBaseline.PtCloudUpstreamMap.RepositoryWideParityClaimed) -Issue "Final closeout PtCloud upstream-map evidence drifted"
    Assert-True -List $List -Condition ($Record.ApiAbiBaseline.XObjDetectUpstreamMap.Sha256 -eq "e95d33dbf2f2b35b5fdbb0aced3d926b9940d9cd843b02f2ebe0149839c161ab" -and $Record.ApiAbiBaseline.XObjDetectUpstreamMap.DeclarationCount -eq 34 -and $Record.ApiAbiBaseline.XObjDetectUpstreamMap.CallableCount -eq 27 -and $Record.ApiAbiBaseline.XObjDetectUpstreamMap.ImplementedCount -eq 20 -and $Record.ApiAbiBaseline.XObjDetectUpstreamMap.MissingCount -eq 0 -and $Record.ApiAbiBaseline.XObjDetectUpstreamMap.IntentionallyOmittedCount -eq 7 -and $Record.ApiAbiBaseline.XObjDetectUpstreamMap.SourceHeaderCount -eq 1 -and $Record.ApiAbiBaseline.XObjDetectUpstreamMap.SelectedFamilyCount -eq 1 -and $Record.ApiAbiBaseline.XObjDetectUpstreamMap.SelectedDeclarationCount -eq 20 -and $Record.ApiAbiBaseline.XObjDetectUpstreamMap.NativeEntrypointAdditionCount -eq 27 -and -not [bool]$Record.ApiAbiBaseline.XObjDetectUpstreamMap.RepositoryWideParityClaimed) -Issue "Final closeout XObjDetect upstream-map evidence drifted"
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
    $sdkPolicyValid = ($Record.LocalValidation.SdkPolicy -eq ".NET 10 (any installed feature band)") -or ($Record.LocalValidation.ExactSdk -match '^10\.0\.\d+$')
    Assert-True -List $List -Condition ($Record.LocalValidation.Status -eq "locally-validated" -and $Record.LocalValidation.InvariantGuardCount -eq 105 -and $sdkPolicyValid -and -not [bool]$Record.LocalValidation.PublicationAllowed -and (@($Record.LocalValidation.RequiredChecks) -join ",") -eq ($expectedChecks -join ",")) -Issue "Final closeout local validation state or check list drifted"
    Assert-True -List $List -Condition ($Record.Signing.Status -eq "repository-signing-pending" -and $Record.Signing.Strategy -eq "nuget.org-repository-signing" -and $Record.Signing.NormalizedInputRequired -and -not [bool]$Record.Signing.AuthorCertificateRequired -and -not [bool]$Record.Signing.PrivateKeyRequired -and -not [bool]$Record.Signing.PrivateKeyMaterialPresent -and $Record.Signing.ServiceIndex -eq "https://api.nuget.org/v3/index.json" -and $Record.Signing.ExpectedSignatureType -eq "Repository" -and $Record.Signing.ExpectedOwner -eq "GuojinYan" -and $Record.Signing.VerificationScript -eq "scripts/Test-NuGetRepositorySignedPackage.ps1" -and $Record.Signing.Verification -eq "post-publication-required") -Issue "Final closeout repository-signing state drifted"
    Assert-True -List $List -Condition ($Record.Sbom.Status -eq "not-ready" -and $Record.Sbom.Format -eq "SPDX-2.3" -and $Record.Sbom.Generator -eq "scripts/New-ReleasePackageSbom.ps1" -and $Record.Sbom.Guard -eq "scripts/Test-ReleasePackageSbom.ps1" -and [bool]$Record.Sbom.Deterministic -and -not [bool]$Record.Sbom.FinalCandidateDocumentGenerated -and $Record.Sbom.Verification -eq "generator-verified-final-candidate-not-generated") -Issue "Final closeout SBOM state must retain a verified generator without claiming final-candidate output"
    Assert-True -List $List -Condition ($Record.Approval.Status -eq "not-approved" -and $Record.Approval.Reviewer -eq "automated-local-preflight" -and $Record.Approval.Approver -eq "unassigned" -and $Record.Approval.EvidenceKind -eq "local-source-and-offline-fixture" -and -not [bool]$Record.Approval.RemoteMutationAllowed) -Issue "Final closeout approval state drifted"

    $expectedMethods = @("GET", "HEAD")
    Assert-True -List $List -Condition ($Record.PublicFeed.Mode -eq "read-only" -and $Record.PublicFeed.ServiceIndex -eq "https://api.nuget.org/v3/index.json" -and $Record.PublicFeed.GitHubPackagesServiceIndex -eq "https://nuget.pkg.github.com/guojin-yan/index.json" -and $Record.PublicFeed.GitHubPackagesRepository -eq "guojin-yan/OpenCV-CSharp-API" -and $Record.PublicFeed.RequiredPublicVisibility -eq "public" -and [int]$Record.PublicFeed.RequiredFeedCount -eq 2) -Issue "Final closeout dual public-feed boundary drifted"
    Assert-True -List $List -Condition ($Record.PublicFeed.CandidatePackage -eq "https://api.nuget.org/v3-flatcontainer/jyppx.opencv.csharp.api/5.0.0/jyppx.opencv.csharp.api.5.0.0.nupkg") -Issue "Final closeout public-feed candidate URL drifted" -Text $Record.PublicFeed.CandidatePackage
    Assert-True -List $List -Condition ((@($Record.PublicFeed.Methods) -join ",") -eq ($expectedMethods -join ",")) -Issue "Final closeout public-feed method list drifted"
    Assert-True -List $List -Condition (-not [bool]$Record.PublicFeed.Mutable -and $Record.PublicFeed.CandidateStatus -eq "not-published" -and -not [bool]$Record.PublicFeed.UploadAttempted) -Issue "Final closeout public-feed state is not immutable and not-published"
    Assert-True -List $List -Condition ($Record.Rollback.Status -eq "not-published" -and -not [bool]$Record.Rollback.PackageRemovalRequired -and $Record.ReleaseApproval.Status -eq "not-approved" -and -not [bool]$Record.ReleaseApproval.ReleaseReady -and $Record.ReleaseApproval.PublicationDecision -eq "do-not-publish") -Issue "Final closeout rollback or publication decision drifted"
    Assert-True -List $List -Condition (-not [bool]$Record.PrivateKeyMaterialPresent -and -not [bool]$Record.SecretMaterialPresent -and [bool]$Record.Deterministic) -Issue "Final closeout must exclude secrets and remain deterministic"

    $expectedBlockers = @("android-arm-device-evidence","api-gap-implementation","macos-support-decision","publication-authorization","release-approval","repository-signing-verification","sbom-inputs")
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
    $fixture.SupportContract.RealSupportCount = 24
    Test-Record -Record $fixture -List $list -ExpectedSourceHash $expectedSourceHash
}
Assert-FixtureRejected -Name "false hosted promotion" -ExpectedIssue "hosted promotion" -Action {
    param($list)
    $fixture = $record | ConvertTo-Json -Depth 30 | ConvertFrom-Json
    $fixture.SupportContract.HostedPromotionEvidence.status = "pending-hosted-evidence"
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

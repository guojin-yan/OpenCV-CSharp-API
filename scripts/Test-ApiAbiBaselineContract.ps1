param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path,
    [string]$DotNetPath = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$managedBaselinePath = Join-Path $repo "compatibility/managed-public-api.txt"
$managedSummaryPath = Join-Path $repo "compatibility/managed-public-api-summary.json"
$gapInventoryPath = Join-Path $repo "compatibility/api-gap-inventory.json"
$nativeFullPath = Join-Path $repo "src/OpenCvSharp.Native/generated/legacy_abi_manifest.txt"
$nativeMiniPath = Join-Path $repo "src/OpenCvSharp.Native/generated/legacy_abi_mini_manifest.txt"
$bindingMapPath = Join-Path $repo "compatibility/native-managed-binding-map.txt"
$bindingSummaryPath = Join-Path $repo "compatibility/native-managed-binding-summary.json"
$spanFamilyPath = Join-Path $repo "compatibility/imgproc-point-set-span-family.json"
$imgProcMapPath = Join-Path $repo "compatibility/imgproc-upstream-map.txt"
$imgProcSummaryPath = Join-Path $repo "compatibility/imgproc-upstream-summary.json"
$imgProcFamilyPath = Join-Path $repo "compatibility/imgproc-implemented-families.json"
$imgCodecsMapPath = Join-Path $repo "compatibility/imgcodecs-upstream-map.txt"
$imgCodecsSummaryPath = Join-Path $repo "compatibility/imgcodecs-upstream-summary.json"
$imgCodecsFamilyPath = Join-Path $repo "compatibility/imgcodecs-implemented-families.json"
$imgCodecsExtensionsPath = Join-Path $repo "compatibility/imgcodecs-source-reviewed-extensions.json"
$videoIOMapPath = Join-Path $repo "compatibility/videoio-upstream-map.txt"
$videoIOSummaryPath = Join-Path $repo "compatibility/videoio-upstream-summary.json"
$videoIOFamilyPath = Join-Path $repo "compatibility/videoio-implemented-families.json"
$videoIORegistryPath = Join-Path $repo "compatibility/videoio-registry-surface.json"
$calib3DMapPath = Join-Path $repo "compatibility/calib3d-upstream-map.txt"
$calib3DSummaryPath = Join-Path $repo "compatibility/calib3d-upstream-summary.json"
$calib3DFamilyPath = Join-Path $repo "compatibility/calib3d-implemented-families.json"
$coreMapPath = Join-Path $repo "compatibility/core-upstream-map.txt"
$coreSummaryPath = Join-Path $repo "compatibility/core-upstream-summary.json"
$coreFamilyPath = Join-Path $repo "compatibility/core-implemented-families.json"
$dnnMapPath = Join-Path $repo "compatibility/dnn-upstream-map.txt"
$dnnSummaryPath = Join-Path $repo "compatibility/dnn-upstream-summary.json"
$dnnFamilyPath = Join-Path $repo "compatibility/dnn-implemented-families.json"
$featuresMapPath = Join-Path $repo "compatibility/features-upstream-map.txt"
$featuresSummaryPath = Join-Path $repo "compatibility/features-upstream-summary.json"
$featuresFamilyPath = Join-Path $repo "compatibility/features-implemented-families.json"
$featuresExtensionsPath = Join-Path $repo "compatibility/features-source-reviewed-extensions.json"
$objDetectMapPath = Join-Path $repo "compatibility/objdetect-upstream-map.txt"
$objDetectSummaryPath = Join-Path $repo "compatibility/objdetect-upstream-summary.json"
$objDetectFamilyPath = Join-Path $repo "compatibility/objdetect-implemented-families.json"
$objDetectRawPath = Join-Path $repo "compatibility/objdetect-upstream-raw.json"
$photoMapPath = Join-Path $repo "compatibility/photo-upstream-map.txt"
$photoSummaryPath = Join-Path $repo "compatibility/photo-upstream-summary.json"
$photoRawPath = Join-Path $repo "compatibility/photo-upstream-raw.json"
$videoMapPath = Join-Path $repo "compatibility/video-upstream-map.txt"
$videoSummaryPath = Join-Path $repo "compatibility/video-upstream-summary.json"
$videoRawPath = Join-Path $repo "compatibility/video-upstream-raw.json"
$violations = [System.Collections.Generic.List[object]]::new()

function Add-Violation {
    param(
        [Parameter(Mandatory)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$List,
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$Issue,
        [string]$Text = ""
    )

    $List.Add([pscustomobject]@{ Path = $Path; Issue = $Issue; Text = $Text.Trim() })
}

function Assert-True {
    param(
        [Parameter(Mandatory)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$List,
        [Parameter(Mandatory)][bool]$Condition,
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$Issue,
        [string]$Text = ""
    )

    if (-not $Condition) {
        Add-Violation -List $List -Path $Path -Issue $Issue -Text $Text
    }
}

function Normalize-Text {
    param([Parameter(Mandatory)][AllowEmptyString()][string]$Text)

    return (($Text -replace "`r`n", "`n") -replace "`r", "`n").TrimEnd() + "`n"
}

function Get-TextSha256 {
    param([Parameter(Mandatory)][AllowEmptyString()][string]$Text)

    $bytes = [Text.UTF8Encoding]::new($false).GetBytes((Normalize-Text $Text))
    return [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
}

function Get-OrdinalSorted {
    param([Parameter(Mandatory)][AllowEmptyCollection()][string[]]$Values)

    $copy = [string[]]$Values.Clone()
    [Array]::Sort($copy, [StringComparer]::Ordinal)
    return $copy
}

function Test-ManagedBaselineDocument {
    param(
        [Parameter(Mandatory)][AllowEmptyString()][string]$Text,
        [Parameter(Mandatory)][object]$Summary,
        [Parameter(Mandatory)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$List,
        [Parameter(Mandatory)][string]$Path
    )

    $normalized = Normalize-Text $Text
    $lines = @($normalized.TrimEnd("`n").Split("`n"))
    $typesIndex = [Array]::IndexOf($lines, "[types]")
    $membersIndex = [Array]::IndexOf($lines, "[members]")
    Assert-True -List $List -Condition ($lines.Count -gt 10 -and $lines[0] -eq "# Generated by tools/ManagedApiBaseline. Do not edit.") -Path $Path -Issue "Managed API baseline generator header is missing"
    Assert-True -List $List -Condition ($typesIndex -gt 0 -and $membersIndex -gt $typesIndex) -Path $Path -Issue "Managed API baseline sections are missing or reordered"
    if ($typesIndex -lt 0 -or $membersIndex -lt 0 -or $membersIndex -le $typesIndex) { return }

    $metadata = @{}
    foreach ($line in $lines[1..($typesIndex - 1)]) {
        if ([string]::IsNullOrWhiteSpace($line)) { continue }
        $pair = $line.Split("=", 2)
        if ($pair.Count -ne 2) {
            Add-Violation -List $List -Path $Path -Issue "Managed API baseline metadata is malformed" -Text $line
            continue
        }
        $metadata[$pair[0]] = $pair[1]
    }

    $typeLines = @($lines[($typesIndex + 1)..($membersIndex - 1)] | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
    $memberLines = if ($membersIndex + 1 -lt $lines.Count) {
        @($lines[($membersIndex + 1)..($lines.Count - 1)] | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
    }
    else { @() }

    Assert-True -List $List -Condition ($metadata["schema-version"] -eq "1" -and $metadata["assembly"] -eq "JYPPX.OpenCV.CSharp.API" -and $metadata["target-framework"] -eq "net8.0" -and $metadata["nullable-shape"] -eq "included") -Path $Path -Issue "Managed API baseline identity or nullable-shape metadata drifted"
    Assert-True -List $List -Condition ($typeLines.Count -eq [int]$metadata["type-count"] -and $typeLines.Count -eq [int]$Summary.typeCount) -Path $Path -Issue "Managed API baseline type count drifted" -Text "actual=$($typeLines.Count) metadata=$($metadata['type-count']) summary=$($Summary.typeCount)"
    Assert-True -List $List -Condition ($memberLines.Count -eq [int]$metadata["member-count"] -and $memberLines.Count -eq [int]$Summary.memberCount) -Path $Path -Issue "Managed API baseline member count drifted" -Text "actual=$($memberLines.Count) metadata=$($metadata['member-count']) summary=$($Summary.memberCount)"
    Assert-True -List $List -Condition ([int]$metadata["namespace-count"] -eq [int]$Summary.namespaceCount) -Path $Path -Issue "Managed API baseline namespace count drifted"

    $sortedTypes = @(Get-OrdinalSorted -Values $typeLines)
    $sortedMembers = @(Get-OrdinalSorted -Values $memberLines)
    Assert-True -List $List -Condition (($typeLines -join "`n") -ceq ($sortedTypes -join "`n")) -Path $Path -Issue "Managed API baseline type entries must use ordinal ordering"
    Assert-True -List $List -Condition (($memberLines -join "`n") -ceq ($sortedMembers -join "`n")) -Path $Path -Issue "Managed API baseline member entries must use ordinal ordering"
    Assert-True -List $List -Condition ($typeLines.Count -eq @($typeLines | Sort-Object -Unique).Count) -Path $Path -Issue "Managed API baseline contains duplicate type entries"
    Assert-True -List $List -Condition ($memberLines.Count -eq @($memberLines | Sort-Object -Unique).Count) -Path $Path -Issue "Managed API baseline contains duplicate member entries"

    foreach ($line in $typeLines) {
        $parts = $line.Split([char]'|')
        Assert-True -List $List -Condition ($line.StartsWith("TYPE|", [StringComparison]::Ordinal) -and $parts.Count -in @(6, 7) -and ($parts.Count -eq 6 -or $parts[6].StartsWith("generic=", [StringComparison]::Ordinal))) -Path $Path -Issue "Managed API type entry is malformed" -Text $line
    }
    foreach ($line in $memberLines) {
        $parts = $line.Split([char]'|')
        Assert-True -List $List -Condition ($line.StartsWith("MEMBER|", [StringComparison]::Ordinal) -and $parts.Count -in @(5, 6) -and ($parts.Count -eq 5 -or $parts[5].StartsWith("generic=", [StringComparison]::Ordinal))) -Path $Path -Issue "Managed API member entry is malformed" -Text $line
    }

    foreach ($line in @($typeLines + $memberLines)) {
        $parts = $line.Split([char]'|')
        $identity = if ($line.StartsWith("TYPE|", [StringComparison]::Ordinal)) { $parts[3] } else { $parts[1] }
        $fixedMajorIdentityPattern = "OpenCv" + "[0-9]+Sharp"
        if ($identity -match $fixedMajorIdentityPattern -and $identity -ne ("OpenCvSharp.OpenCv" + "5SharpBuildInfo")) {
            Add-Violation -List $List -Path $Path -Issue "Managed API baseline contains an unexpected fixed-major public identity" -Text $identity
        }
    }

    $actualSha256 = Get-TextSha256 $normalized
    Assert-True -List $List -Condition ($Summary.schemaVersion -eq 1 -and $Summary.generator -eq "tools/ManagedApiBaseline" -and $Summary.baselinePath -eq "compatibility/managed-public-api.txt") -Path $Path -Issue "Managed API summary identity drifted"
    Assert-True -List $List -Condition ($Summary.baselineSha256 -eq $actualSha256) -Path $Path -Issue "Managed API baseline SHA256 does not match its summary" -Text "actual=$actualSha256 summary=$($Summary.baselineSha256)"
    Assert-True -List $List -Condition (@($Summary.namespaces).Count -eq [int]$Summary.namespaceCount -and @($Summary.namespaces | ForEach-Object { $_.name }).Count -eq @($Summary.namespaces | ForEach-Object { $_.name } | Sort-Object -Unique).Count) -Path $Path -Issue "Managed API namespace summary is incomplete or duplicated"
}

function Test-NativeManifestDocument {
    param(
        [Parameter(Mandatory)][AllowEmptyString()][string]$Text,
        [Parameter(Mandatory)][int]$ExpectedCount,
        [Parameter(Mandatory)][bool]$Mini,
        [Parameter(Mandatory)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$List,
        [Parameter(Mandatory)][string]$Path
    )

    $lines = @((Normalize-Text $Text).TrimEnd("`n").Split("`n"))
    $functionsIndex = [Array]::IndexOf($lines, "[functions]")
    Assert-True -List $List -Condition ($functionsIndex -gt 0) -Path $Path -Issue "Native ABI manifest functions section is missing"
    if ($functionsIndex -lt 0) { return }

    $metadata = @{}
    foreach ($line in $lines[1..($functionsIndex - 1)]) {
        if ([string]::IsNullOrWhiteSpace($line)) { continue }
        $pair = $line.Split("=", 2)
        if ($pair.Count -eq 2) { $metadata[$pair[0]] = $pair[1] }
    }
    $functionLines = @($lines[($functionsIndex + 1)..($lines.Count - 1)] | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
    Assert-True -List $List -Condition ($metadata["primary-prefix"] -eq "jyppx_ocv_" -and $metadata["legacy-prefix"] -eq "jyppx_ocv5_") -Path $Path -Issue "Native ABI manifest prefixes drifted"
    Assert-True -List $List -Condition ($functionLines.Count -eq $ExpectedCount -and $functionLines.Count -eq [int]$metadata["function-count"]) -Path $Path -Issue "Native ABI function count drifted" -Text "actual=$($functionLines.Count) expected=$ExpectedCount"
    Assert-True -List $List -Condition ((-not $Mini -and -not $metadata.ContainsKey("runtime-profile")) -or ($Mini -and $metadata["runtime-profile"] -eq "mini")) -Path $Path -Issue "Native ABI runtime profile metadata drifted"
    $functionNames = @($functionLines | ForEach-Object { $_.Split([char]'|')[0] })
    $sortedNames = @(Get-OrdinalSorted -Values $functionNames)
    Assert-True -List $List -Condition (($functionNames -join "`n") -ceq ($sortedNames -join "`n")) -Path $Path -Issue "Native ABI entries must use ordinal function-name ordering"
    Assert-True -List $List -Condition ($functionLines.Count -eq @($functionLines | Sort-Object -Unique).Count) -Path $Path -Issue "Native ABI manifest contains duplicate functions"

    foreach ($line in $functionLines) {
        $parts = $line.Split([char]'|')
        if ($parts.Count -ne 5) {
            Add-Violation -List $List -Path $Path -Issue "Native ABI manifest entry is malformed" -Text $line
            continue
        }
        $expectedLegacy = "jyppx_ocv5_" + $parts[0].Substring("jyppx_ocv_".Length)
        Assert-True -List $List -Condition ($parts[0].StartsWith("jyppx_ocv_", [StringComparison]::Ordinal) -and $parts[0] -notmatch "^jyppx_ocv[0-9]+_" -and $parts[1] -eq $expectedLegacy) -Path $Path -Issue "Native ABI primary/compatibility identity drifted" -Text $line
    }
}

function Test-GapInventory {
    param(
        [AllowNull()][object]$Inventory,
        [Parameter(Mandatory)][object]$Summary,
        [Parameter(Mandatory)][string]$ManagedHash,
        [Parameter(Mandatory)][string]$NativeFullHash,
        [Parameter(Mandatory)][string]$NativeMiniHash,
        [Parameter(Mandatory)][object]$BindingSummary,
        [Parameter(Mandatory)][string]$BindingMapHash,
        [Parameter(Mandatory)][object]$ImgProcSummary,
        [Parameter(Mandatory)][string]$ImgProcMapHash,
        [Parameter(Mandatory)][object]$ImgCodecsSummary,
        [Parameter(Mandatory)][string]$ImgCodecsMapHash,
        [Parameter(Mandatory)][object]$VideoIOSummary,
        [Parameter(Mandatory)][string]$VideoIOMapHash,
        [Parameter(Mandatory)][string]$VideoIORegistryHash,
        [Parameter(Mandatory)][object]$Calib3DSummary,
        [Parameter(Mandatory)][string]$Calib3DMapHash,
        [Parameter(Mandatory)][object]$CoreSummary,
        [Parameter(Mandatory)][string]$CoreMapHash,
        [Parameter(Mandatory)][object]$DnnSummary,
        [Parameter(Mandatory)][string]$DnnMapHash,
        [Parameter(Mandatory)][object]$FeaturesSummary,
        [Parameter(Mandatory)][string]$FeaturesMapHash,
        [Parameter(Mandatory)][object]$ObjDetectSummary,
        [Parameter(Mandatory)][string]$ObjDetectMapHash,
        [Parameter(Mandatory)][string]$ObjDetectRawHash,
        [Parameter(Mandatory)][object]$PhotoSummary,
        [Parameter(Mandatory)][string]$PhotoMapHash,
        [Parameter(Mandatory)][string]$PhotoRawHash,
        [Parameter(Mandatory)][object]$VideoSummary,
        [Parameter(Mandatory)][string]$VideoMapHash,
        [Parameter(Mandatory)][string]$VideoRawHash,
        [Parameter(Mandatory)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$List,
        [Parameter(Mandatory)][string]$Path
    )

    if ($null -eq $Inventory) {
        Add-Violation -List $List -Path $Path -Issue "API gap inventory is missing"
        return
    }
    if ($null -eq $Inventory.baselines -or $null -eq $Inventory.measurements -or $null -eq $Inventory.policy) {
        Add-Violation -List $List -Path $Path -Issue "API gap inventory is missing required sections"
        return
    }

    Assert-True -List $List -Condition ($Inventory.schemaVersion -eq 1 -and $Inventory.upstreamOpenCvVersion -eq "5.0.0" -and $Inventory.status -eq "baseline-established-native-managed-parity-measured") -Path $Path -Issue "API gap inventory identity/status drifted"
    Assert-True -List $List -Condition ($Inventory.baselines.managed.sha256 -eq $ManagedHash -and [int]$Inventory.baselines.managed.typeCount -eq [int]$Summary.typeCount -and [int]$Inventory.baselines.managed.memberCount -eq [int]$Summary.memberCount -and [int]$Inventory.baselines.managed.namespaceCount -eq [int]$Summary.namespaceCount) -Path $Path -Issue "API gap inventory managed baseline evidence drifted"
    Assert-True -List $List -Condition ($Inventory.baselines.nativeFull.sha256 -eq $NativeFullHash -and [int]$Inventory.baselines.nativeFull.functionCount -eq 2438 -and $Inventory.baselines.nativeFull.primaryPrefix -eq "jyppx_ocv_") -Path $Path -Issue "API gap inventory full native baseline evidence drifted"
    Assert-True -List $List -Condition ($Inventory.baselines.nativeMini.sha256 -eq $NativeMiniHash -and [int]$Inventory.baselines.nativeMini.functionCount -eq 526 -and $Inventory.baselines.nativeMini.primaryPrefix -eq "jyppx_ocv_") -Path $Path -Issue "API gap inventory mini native baseline evidence drifted"
    Assert-True -List $List -Condition ($Inventory.baselines.nativeManagedBindingMap.sha256 -eq $BindingMapHash -and $Inventory.baselines.nativeManagedBindingMap.path -eq "compatibility/native-managed-binding-map.txt" -and $Inventory.baselines.nativeManagedBindingMap.summaryPath -eq "compatibility/native-managed-binding-summary.json" -and [int]$Inventory.baselines.nativeManagedBindingMap.nativeFunctionCount -eq [int]$BindingSummary.nativeFunctionCount -and [int]$Inventory.baselines.nativeManagedBindingMap.managedBoundCount -eq [int]$BindingSummary.managedBoundCount -and [int]$Inventory.baselines.nativeManagedBindingMap.unboundCount -eq 0 -and [int]$Inventory.baselines.nativeManagedBindingMap.managedOnlyCount -eq 0) -Path $Path -Issue "API gap inventory native-to-managed binding evidence drifted"
    Assert-True -List $List -Condition ($Inventory.baselines.imgProcUpstreamMap.sha256 -eq $ImgProcMapHash -and $Inventory.baselines.imgProcUpstreamMap.summaryPath -eq "compatibility/imgproc-upstream-summary.json" -and $Inventory.baselines.imgProcUpstreamMap.familyInventoryPath -eq "compatibility/imgproc-implemented-families.json" -and [int]$Inventory.baselines.imgProcUpstreamMap.declarationCount -eq [int]$ImgProcSummary.declarationCount -and [int]$Inventory.baselines.imgProcUpstreamMap.implementedCount -eq [int]$ImgProcSummary.classificationCounts.implemented -and [int]$Inventory.baselines.imgProcUpstreamMap.missingCount -eq [int]$ImgProcSummary.classificationCounts.missing) -Path $Path -Issue "API gap inventory ImgProc upstream-map evidence drifted"
    Assert-True -List $List -Condition ($Inventory.baselines.imgCodecsUpstreamMap.sha256 -eq $ImgCodecsMapHash -and $Inventory.baselines.imgCodecsUpstreamMap.summaryPath -eq "compatibility/imgcodecs-upstream-summary.json" -and $Inventory.baselines.imgCodecsUpstreamMap.familyInventoryPath -eq "compatibility/imgcodecs-implemented-families.json" -and $Inventory.baselines.imgCodecsUpstreamMap.sourceReviewedExtensionsPath -eq "compatibility/imgcodecs-source-reviewed-extensions.json" -and [int]$Inventory.baselines.imgCodecsUpstreamMap.declarationCount -eq [int]$ImgCodecsSummary.declarationCount -and [int]$Inventory.baselines.imgCodecsUpstreamMap.implementedCount -eq [int]$ImgCodecsSummary.classificationCounts.implemented -and [int]$Inventory.baselines.imgCodecsUpstreamMap.missingCount -eq 0 -and [int]$Inventory.baselines.imgCodecsUpstreamMap.sourceReviewedExtensionCount -eq 1) -Path $Path -Issue "API gap inventory ImgCodecs upstream-map evidence drifted"
    Assert-True -List $List -Condition ($Inventory.baselines.videoIOUpstreamMap.sha256 -eq $VideoIOMapHash -and $Inventory.baselines.videoIOUpstreamMap.registrySurfaceSha256 -eq $VideoIORegistryHash -and $Inventory.baselines.videoIOUpstreamMap.summaryPath -eq "compatibility/videoio-upstream-summary.json" -and $Inventory.baselines.videoIOUpstreamMap.familyInventoryPath -eq "compatibility/videoio-implemented-families.json" -and $Inventory.baselines.videoIOUpstreamMap.registrySurfacePath -eq "compatibility/videoio-registry-surface.json" -and [int]$Inventory.baselines.videoIOUpstreamMap.declarationCount -eq [int]$VideoIOSummary.declarationCount -and [int]$Inventory.baselines.videoIOUpstreamMap.implementedCount -eq [int]$VideoIOSummary.classificationCounts.implemented -and [int]$Inventory.baselines.videoIOUpstreamMap.missingCount -eq 0 -and [int]$Inventory.baselines.videoIOUpstreamMap.registryOperationCount -eq 12) -Path $Path -Issue "API gap inventory VideoIO upstream-map evidence drifted"
    Assert-True -List $List -Condition ($Inventory.baselines.calib3DUpstreamMap.sha256 -eq $Calib3DMapHash -and $Inventory.baselines.calib3DUpstreamMap.summaryPath -eq "compatibility/calib3d-upstream-summary.json" -and $Inventory.baselines.calib3DUpstreamMap.familyInventoryPath -eq "compatibility/calib3d-implemented-families.json" -and [int]$Inventory.baselines.calib3DUpstreamMap.declarationCount -eq [int]$Calib3DSummary.declarationCount -and [int]$Inventory.baselines.calib3DUpstreamMap.implementedCount -eq [int]$Calib3DSummary.classificationCounts.implemented -and [int]$Inventory.baselines.calib3DUpstreamMap.missingCount -eq 0 -and [int]$Inventory.baselines.calib3DUpstreamMap.sourceHeaderCount -eq 4) -Path $Path -Issue "API gap inventory Calib3D upstream-map evidence drifted"
    $coreInventoryMatches =
        $Inventory.baselines.coreUpstreamMap.sha256 -eq $CoreMapHash -and
        $Inventory.baselines.coreUpstreamMap.summaryPath -eq "compatibility/core-upstream-summary.json" -and
        $Inventory.baselines.coreUpstreamMap.familyInventoryPath -eq "compatibility/core-implemented-families.json" -and
        [int]$Inventory.baselines.coreUpstreamMap.declarationCount -eq [int]$CoreSummary.declarationCount -and
        [int]$Inventory.baselines.coreUpstreamMap.callableCount -eq [int]$CoreSummary.callableCount -and
        [int]$Inventory.baselines.coreUpstreamMap.implementedCount -eq [int]$CoreSummary.classificationCounts.implemented -and
        [int]$Inventory.baselines.coreUpstreamMap.missingCount -eq [int]$CoreSummary.classificationCounts.missing -and
        [int]$Inventory.baselines.coreUpstreamMap.intentionallyOmittedCount -eq [int]$CoreSummary.classificationCounts.'intentionally-omitted' -and
        [int]$Inventory.baselines.coreUpstreamMap.unsupportedCount -eq [int]$CoreSummary.classificationCounts.unsupported -and
        [int]$Inventory.baselines.coreUpstreamMap.upstreamConditionalCount -eq [int]$CoreSummary.classificationCounts.'upstream-conditional' -and
        [int]$Inventory.baselines.coreUpstreamMap.selectedFamilyCount -eq [int]$CoreSummary.selectedFamilyCount -and
        [int]$Inventory.baselines.coreUpstreamMap.selectedDeclarationCount -eq [int]$CoreSummary.selectedDeclarationCount -and
        [int]$Inventory.baselines.coreUpstreamMap.sourceHeaderCount -eq [int]$CoreSummary.sourceHeaderCount
    Assert-True -List $List -Condition $coreInventoryMatches -Path $Path -Issue "API gap inventory Core upstream-map evidence drifted"
    $dnnInventoryMatches =
        $Inventory.baselines.dnnUpstreamMap.sha256 -eq $DnnMapHash -and
        $Inventory.baselines.dnnUpstreamMap.summaryPath -eq "compatibility/dnn-upstream-summary.json" -and
        $Inventory.baselines.dnnUpstreamMap.familyInventoryPath -eq "compatibility/dnn-implemented-families.json" -and
        [int]$Inventory.baselines.dnnUpstreamMap.declarationCount -eq [int]$DnnSummary.declarationCount -and
        [int]$Inventory.baselines.dnnUpstreamMap.callableCount -eq [int]$DnnSummary.callableCount -and
        [int]$Inventory.baselines.dnnUpstreamMap.implementedCount -eq [int]$DnnSummary.classificationCounts.implemented -and
        [int]$Inventory.baselines.dnnUpstreamMap.missingCount -eq [int]$DnnSummary.classificationCounts.missing -and
        [int]$Inventory.baselines.dnnUpstreamMap.intentionallyOmittedCount -eq [int]$DnnSummary.classificationCounts.'intentionally-omitted' -and
        [int]$Inventory.baselines.dnnUpstreamMap.unsupportedCount -eq [int]$DnnSummary.classificationCounts.unsupported -and
        [int]$Inventory.baselines.dnnUpstreamMap.upstreamConditionalCount -eq [int]$DnnSummary.classificationCounts.'upstream-conditional' -and
        [int]$Inventory.baselines.dnnUpstreamMap.selectedFamilyCount -eq [int]$DnnSummary.selectedFamilyCount -and
        [int]$Inventory.baselines.dnnUpstreamMap.selectedDeclarationCount -eq [int]$DnnSummary.selectedDeclarationCount -and
        [int]$Inventory.baselines.dnnUpstreamMap.sourceHeaderCount -eq [int]$DnnSummary.sourceHeaderCount
    Assert-True -List $List -Condition $dnnInventoryMatches -Path $Path -Issue "API gap inventory DNN upstream-map evidence drifted"
    $featuresInventoryMatches =
        $Inventory.baselines.featuresUpstreamMap.sha256 -eq $FeaturesMapHash -and
        $Inventory.baselines.featuresUpstreamMap.summaryPath -eq "compatibility/features-upstream-summary.json" -and
        $Inventory.baselines.featuresUpstreamMap.classificationPath -eq "compatibility/features-upstream-classifications.json" -and
        $Inventory.baselines.featuresUpstreamMap.familyInventoryPath -eq "compatibility/features-implemented-families.json" -and
        $Inventory.baselines.featuresUpstreamMap.sourceReviewedExtensionsPath -eq "compatibility/features-source-reviewed-extensions.json" -and
        [int]$Inventory.baselines.featuresUpstreamMap.declarationCount -eq [int]$FeaturesSummary.declarationCount -and
        [int]$Inventory.baselines.featuresUpstreamMap.callableCount -eq [int]$FeaturesSummary.callableCount -and
        [int]$Inventory.baselines.featuresUpstreamMap.implementedCount -eq [int]$FeaturesSummary.classificationCounts.implemented -and
        [int]$Inventory.baselines.featuresUpstreamMap.missingCount -eq [int]$FeaturesSummary.classificationCounts.missing -and
        [int]$Inventory.baselines.featuresUpstreamMap.intentionallyOmittedCount -eq [int]$FeaturesSummary.classificationCounts.'intentionally-omitted' -and
        [int]$Inventory.baselines.featuresUpstreamMap.unsupportedCount -eq [int]$FeaturesSummary.classificationCounts.unsupported -and
        [int]$Inventory.baselines.featuresUpstreamMap.upstreamConditionalCount -eq [int]$FeaturesSummary.classificationCounts.'upstream-conditional' -and
        [int]$Inventory.baselines.featuresUpstreamMap.selectedFamilyCount -eq [int]$FeaturesSummary.selectedFamilyCount -and
        [int]$Inventory.baselines.featuresUpstreamMap.selectedDeclarationCount -eq [int]$FeaturesSummary.selectedDeclarationCount -and
        [int]$Inventory.baselines.featuresUpstreamMap.sourceHeaderCount -eq [int]$FeaturesSummary.sourceHeaderCount -and
        [int]$Inventory.baselines.featuresUpstreamMap.compatibilityHeaderCount -eq [int]$FeaturesSummary.compatibilityHeaderCount -and
        [int]$Inventory.baselines.featuresUpstreamMap.sourceReviewedExtensionCount -eq [int]$FeaturesSummary.sourceReviewedExtensionDeclarationCount
    Assert-True -List $List -Condition $featuresInventoryMatches -Path $Path -Issue "API gap inventory Features upstream-map evidence drifted"
    $objDetectInventoryMatches =
        $Inventory.baselines.objDetectUpstreamMap.sha256 -eq $ObjDetectMapHash -and
        $Inventory.baselines.objDetectUpstreamMap.rawSha256 -eq $ObjDetectRawHash -and
        $Inventory.baselines.objDetectUpstreamMap.summaryPath -eq "compatibility/objdetect-upstream-summary.json" -and
        $Inventory.baselines.objDetectUpstreamMap.classificationPath -eq "compatibility/objdetect-upstream-classifications.json" -and
        $Inventory.baselines.objDetectUpstreamMap.familyInventoryPath -eq "compatibility/objdetect-implemented-families.json" -and
        $Inventory.baselines.objDetectUpstreamMap.rawExtractionPath -eq "compatibility/objdetect-upstream-raw.json" -and
        [int]$Inventory.baselines.objDetectUpstreamMap.declarationCount -eq [int]$ObjDetectSummary.declarationCount -and
        [int]$Inventory.baselines.objDetectUpstreamMap.callableCount -eq [int]$ObjDetectSummary.callableCount -and
        [int]$Inventory.baselines.objDetectUpstreamMap.implementedCount -eq [int]$ObjDetectSummary.classificationCounts.implemented -and
        [int]$Inventory.baselines.objDetectUpstreamMap.missingCount -eq [int]$ObjDetectSummary.classificationCounts.missing -and
        [int]$Inventory.baselines.objDetectUpstreamMap.intentionallyOmittedCount -eq [int]$ObjDetectSummary.classificationCounts.'intentionally-omitted' -and
        [int]$Inventory.baselines.objDetectUpstreamMap.unsupportedCount -eq [int]$ObjDetectSummary.classificationCounts.unsupported -and
        [int]$Inventory.baselines.objDetectUpstreamMap.upstreamConditionalCount -eq [int]$ObjDetectSummary.classificationCounts.'upstream-conditional' -and
        [int]$Inventory.baselines.objDetectUpstreamMap.selectedFamilyCount -eq [int]$ObjDetectSummary.selectedFamilyCount -and
        [int]$Inventory.baselines.objDetectUpstreamMap.selectedDeclarationCount -eq [int]$ObjDetectSummary.selectedDeclarationCount -and
        [int]$Inventory.baselines.objDetectUpstreamMap.sourceHeaderCount -eq [int]$ObjDetectSummary.sourceHeaderCount -and
        [int]$Inventory.baselines.objDetectUpstreamMap.compatibilityHeaderCount -eq [int]$ObjDetectSummary.compatibilityHeaderCount -and
        [int]$Inventory.baselines.objDetectUpstreamMap.managedPublicTypeAdditionCount -eq [int]$ObjDetectSummary.managedPublicTypeAdditionCount -and
        [int]$Inventory.baselines.objDetectUpstreamMap.managedPublicMemberAdditionCount -eq [int]$ObjDetectSummary.managedPublicMemberAdditionCount -and
        [int]$Inventory.baselines.objDetectUpstreamMap.nativeEntrypointAdditionCount -eq [int]$ObjDetectSummary.nativeEntrypointAdditionCount -and
        ((@($Inventory.baselines.objDetectUpstreamMap.omittedOrdinals) | ForEach-Object { [int]$_ }) -join ",") -eq "3,4,40,41,44,45,61,62,145,147"
    Assert-True -List $List -Condition $objDetectInventoryMatches -Path $Path -Issue "API gap inventory ObjDetect upstream-map evidence drifted"
    $photoInventoryMatches =
        $Inventory.baselines.photoUpstreamMap.sha256 -eq $PhotoMapHash -and
        $Inventory.baselines.photoUpstreamMap.rawSha256 -eq $PhotoRawHash -and
        $Inventory.baselines.photoUpstreamMap.summaryPath -eq "compatibility/photo-upstream-summary.json" -and
        $Inventory.baselines.photoUpstreamMap.classificationPath -eq "compatibility/photo-upstream-classifications.json" -and
        $Inventory.baselines.photoUpstreamMap.familyInventoryPath -eq "compatibility/photo-implemented-families.json" -and
        $Inventory.baselines.photoUpstreamMap.rawExtractionPath -eq "compatibility/photo-upstream-raw.json" -and
        [int]$Inventory.baselines.photoUpstreamMap.declarationCount -eq [int]$PhotoSummary.declarationCount -and
        [int]$Inventory.baselines.photoUpstreamMap.callableCount -eq [int]$PhotoSummary.callableCount -and
        [int]$Inventory.baselines.photoUpstreamMap.implementedCount -eq [int]$PhotoSummary.classificationCounts.implemented -and
        [int]$Inventory.baselines.photoUpstreamMap.missingCount -eq [int]$PhotoSummary.classificationCounts.missing -and
        [int]$Inventory.baselines.photoUpstreamMap.intentionallyOmittedCount -eq [int]$PhotoSummary.classificationCounts.'intentionally-omitted' -and
        [int]$Inventory.baselines.photoUpstreamMap.sourceHeaderCount -eq [int]$PhotoSummary.sourceHeaderCount -and
        [int]$Inventory.baselines.photoUpstreamMap.compatibilityHeaderCount -eq [int]$PhotoSummary.compatibilityHeaderCount -and
        [int]$Inventory.baselines.photoUpstreamMap.excludedPublicHeaderCount -eq [int]$PhotoSummary.excludedPublicHeaderCount -and
        [int]$Inventory.baselines.photoUpstreamMap.selectedFamilyCount -eq [int]$PhotoSummary.selectedFamilyCount -and
        [int]$Inventory.baselines.photoUpstreamMap.selectedDeclarationCount -eq [int]$PhotoSummary.selectedDeclarationCount -and
        [int]$Inventory.baselines.photoUpstreamMap.managedPublicTypeAdditionCount -eq [int]$PhotoSummary.managedPublicTypeAdditionCount -and
        [int]$Inventory.baselines.photoUpstreamMap.managedPublicMemberAdditionCount -eq [int]$PhotoSummary.managedPublicMemberAdditionCount -and
        [int]$Inventory.baselines.photoUpstreamMap.nativeEntrypointAdditionCount -eq [int]$PhotoSummary.nativeEntrypointAdditionCount
    Assert-True -List $List -Condition $photoInventoryMatches -Path $Path -Issue "API gap inventory Photo upstream-map evidence drifted"
    $videoInventoryMatches =
        $Inventory.baselines.videoUpstreamMap.sha256 -eq $VideoMapHash -and
        $Inventory.baselines.videoUpstreamMap.rawSha256 -eq $VideoRawHash -and
        $Inventory.baselines.videoUpstreamMap.summaryPath -eq "compatibility/video-upstream-summary.json" -and
        $Inventory.baselines.videoUpstreamMap.classificationPath -eq "compatibility/video-upstream-classifications.json" -and
        $Inventory.baselines.videoUpstreamMap.familyInventoryPath -eq "compatibility/video-implemented-families.json" -and
        $Inventory.baselines.videoUpstreamMap.rawExtractionPath -eq "compatibility/video-upstream-raw.json" -and
        [int]$Inventory.baselines.videoUpstreamMap.declarationCount -eq [int]$VideoSummary.declarationCount -and
        [int]$Inventory.baselines.videoUpstreamMap.callableCount -eq [int]$VideoSummary.callableCount -and
        [int]$Inventory.baselines.videoUpstreamMap.implementedCount -eq [int]$VideoSummary.classificationCounts.implemented -and
        [int]$Inventory.baselines.videoUpstreamMap.missingCount -eq [int]$VideoSummary.classificationCounts.missing -and
        [int]$Inventory.baselines.videoUpstreamMap.intentionallyOmittedCount -eq [int]$VideoSummary.classificationCounts.'intentionally-omitted' -and
        [int]$Inventory.baselines.videoUpstreamMap.unsupportedCount -eq [int]$VideoSummary.classificationCounts.unsupported -and
        [int]$Inventory.baselines.videoUpstreamMap.upstreamConditionalCount -eq [int]$VideoSummary.classificationCounts.'upstream-conditional' -and
        [int]$Inventory.baselines.videoUpstreamMap.sourceHeaderCount -eq [int]$VideoSummary.sourceHeaderCount -and
        [int]$Inventory.baselines.videoUpstreamMap.compatibilityHeaderCount -eq [int]$VideoSummary.compatibilityHeaderCount -and
        [int]$Inventory.baselines.videoUpstreamMap.excludedPublicHeaderCount -eq [int]$VideoSummary.excludedPublicHeaderCount -and
        [int]$Inventory.baselines.videoUpstreamMap.selectedFamilyCount -eq [int]$VideoSummary.selectedFamilyCount -and
        [int]$Inventory.baselines.videoUpstreamMap.selectedDeclarationCount -eq [int]$VideoSummary.selectedDeclarationCount -and
        [int]$Inventory.baselines.videoUpstreamMap.managedPublicTypeAdditionCount -eq [int]$VideoSummary.managedPublicTypeAdditionCount -and
        [int]$Inventory.baselines.videoUpstreamMap.managedPublicMemberAdditionCount -eq [int]$VideoSummary.managedPublicMemberAdditionCount -and
        [int]$Inventory.baselines.videoUpstreamMap.nativeEntrypointAdditionCount -eq [int]$VideoSummary.nativeEntrypointAdditionCount
    Assert-True -List $List -Condition $videoInventoryMatches -Path $Path -Issue "API gap inventory Video upstream-map evidence drifted"
    Assert-True -List $List -Condition ([bool]$Inventory.measurements.managedPublicSurfaceBaseline -and [bool]$Inventory.measurements.nativeCAbiBaseline -and -not [bool]$Inventory.measurements.upstreamCppParityMeasured -and [bool]$Inventory.measurements.imgProcHeaderSliceMeasured -and [bool]$Inventory.measurements.imgCodecsHeaderSliceMeasured -and [bool]$Inventory.measurements.videoIOHeaderSliceMeasured -and [bool]$Inventory.measurements.videoIORegistrySurfaceMeasured -and [bool]$Inventory.measurements.calib3DCompatibilityClosureMeasured -and [bool]$Inventory.measurements.coreCompatibilityClosureMeasured -and [bool]$Inventory.measurements.dnnCompatibilityClosureMeasured -and [bool]$Inventory.measurements.featuresCompatibilityClosureMeasured -and [bool]$Inventory.measurements.objDetectCompatibilityClosureMeasured -and [bool]$Inventory.measurements.photoCompatibilityClosureMeasured -and [bool]$Inventory.measurements.videoCompatibilityClosureMeasured -and [bool]$Inventory.measurements.nativeToManagedParityMeasured -and -not [bool]$Inventory.measurements.ownershipErrorMarshallingAuditComplete -and -not [bool]$Inventory.measurements.packageSurfaceDefinesSupport) -Path $Path -Issue "API gap inventory measured-parity or support claims drifted"
    Assert-True -List $List -Condition (-not [bool]$Inventory.policy.baselineIsParityClaim -and [bool]$Inventory.policy.baselineUpdateRequiresReview -and [bool]$Inventory.policy.breakingChangeRequiresExplicitDecision -and [bool]$Inventory.policy.supportRequiresRuntimeEvidence) -Path $Path -Issue "API gap inventory policy drifted"

    $expectedGapIds = @("managed-upstream-parity-map","native-to-managed-binding-map","ownership-error-marshalling-audit","public-api-compatibility-review","sample-and-guide-coverage-map")
    $actualGapIds = @($Inventory.knownGaps | ForEach-Object { [string]$_.id })
    $sortedGapIds = @(Get-OrdinalSorted -Values $actualGapIds)
    Assert-True -List $List -Condition (($actualGapIds -join ",") -ceq ($sortedGapIds -join ",") -and ($actualGapIds -join ",") -eq ($expectedGapIds -join ",")) -Path $Path -Issue "API gap inventory entries are missing, duplicated, or reordered"
    $expectedPriorityIds = @("managed-upstream-parity-map","ownership-error-marshalling-audit","public-api-compatibility-review","sample-and-guide-coverage-map")
    Assert-True -List $List -Condition ((@($Inventory.priorityOrder) -join ",") -eq ($expectedPriorityIds -join ",")) -Path $Path -Issue "API gap priority order drifted"
    foreach ($gap in $Inventory.knownGaps) {
        if ($gap.id -eq "native-to-managed-binding-map") {
            Assert-True -List $List -Condition ($gap.status -eq "closed" -and $gap.evidence -eq "compatibility/native-managed-binding-summary.json" -and -not [string]::IsNullOrWhiteSpace([string]$gap.result)) -Path $Path -Issue "Native-to-managed binding gap must retain closed evidence" -Text $gap.id
        }
        else {
            Assert-True -List $List -Condition ($gap.status -in @("open", "ongoing") -and -not [string]::IsNullOrWhiteSpace([string]$gap.nextEvidence)) -Path $Path -Issue "Open API gap entry must retain actionable next evidence" -Text $gap.id
        }
    }
}

function Assert-FixtureRejected {
    param(
        [Parameter(Mandatory)][string]$Name,
        [Parameter(Mandatory)][scriptblock]$Action,
        [Parameter(Mandatory)][string]$ExpectedIssue
    )

    $fixtureViolations = [System.Collections.Generic.List[object]]::new()
    & $Action $fixtureViolations
    if ($fixtureViolations.Count -eq 0) {
        throw "Negative API/ABI baseline fixture was accepted: $Name"
    }
    if (-not @($fixtureViolations | Where-Object { $_.Issue -like "*$ExpectedIssue*" })) {
        throw "Negative API/ABI baseline fixture '$Name' failed for the wrong reason: $($fixtureViolations.Issue -join '; ')"
    }
}

function Test-SpanFamilyInventory {
    param(
        [AllowNull()][object]$Inventory,
        [Parameter(Mandatory)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$List,
        [Parameter(Mandatory)][string]$Path
    )

    if ($null -eq $Inventory) {
        Add-Violation -List $List -Path $Path -Issue "ImgProc Span family inventory is missing"
        return
    }

    $expectedIds = @(
        "approx-poly-n",
        "fit-ellipse",
        "fit-ellipse-ams",
        "fit-ellipse-direct",
        "get-closest-ellipse-points",
        "intersect-convex-convex",
        "min-enclosing-convex-polygon",
        "min-enclosing-triangle")
    $actualIds = @($Inventory.operations | ForEach-Object { [string]$_.id })
    Assert-True -List $List -Condition ($Inventory.schemaVersion -eq 1 -and $Inventory.familyId -eq "imgproc-point-set-span-fast-paths" -and $Inventory.status -eq "implemented-verified" -and $Inventory.upstreamOpenCvVersion -eq "5.0.0") -Path $Path -Issue "ImgProc Span family inventory identity/status drifted"
    Assert-True -List $List -Condition ($Inventory.managedType -eq "OpenCvSharp.ImgProc.Cv2" -and $Inventory.inputShape -eq "ReadOnlySpan<OpenCvSharp.Core.Point>" -and -not [bool]$Inventory.nativeAbiChangeRequired) -Path $Path -Issue "ImgProc Span family API or ABI shape drifted"
    Assert-True -List $List -Condition (($actualIds -join ",") -eq ($expectedIds -join ",")) -Path $Path -Issue "ImgProc Span family operations are missing, duplicated, or reordered"
    foreach ($operation in $Inventory.operations) {
        Assert-True -List $List -Condition ($operation.classification -eq "implemented-verified" -and @($operation.nativeEntrypoints).Count -gt 0) -Path $Path -Issue "ImgProc Span operation must retain implemented evidence" -Text $operation.id
    }

    $requiredEvidence = @(
        "src/OpenCvSharp/ImgProc/Cv2.cs",
        "tests/OpenCvSharp.Tests/ImgProc/Cv2InteropTests.cs",
        "src/OpenCvSharp.Native/tests/native_smoke.cpp",
        "samples/ConsoleSamples/Program.cs",
        "docs/articles/imgproc-geometry-guide.md",
        "docs/articles/point-set-marshalling-guide.md")
    $actualEvidence = @(
        [string]$Inventory.evidence.implementation,
        [string]$Inventory.evidence.managedTests,
        [string]$Inventory.evidence.nativeRegressionTest,
        [string]$Inventory.evidence.sample,
        [string]$Inventory.evidence.guide,
        [string]$Inventory.evidence.marshallingGuide)
    Assert-True -List $List -Condition (($actualEvidence -join ",") -eq ($requiredEvidence -join ",")) -Path $Path -Issue "ImgProc Span family implementation/test/sample/doc evidence drifted"
}

function Test-ImgProcFamilyInventory {
    param(
        [AllowNull()][object]$Summary,
        [AllowNull()][object]$Inventory,
        [Parameter(Mandatory)][string]$MappingHash,
        [Parameter(Mandatory)][string]$InventoryHash,
        [Parameter(Mandatory)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$List,
        [Parameter(Mandatory)][string]$Path
    )

    if ($null -eq $Summary -or $null -eq $Inventory) {
        Add-Violation -List $List -Path $Path -Issue "ImgProc upstream summary or family inventory is missing"
        return
    }

    Assert-True -List $List -Condition ($Summary.schemaVersion -eq 1 -and $Summary.generator -eq "tools/ImgProcUpstreamMap" -and $Summary.upstreamOpenCvVersion -eq "5.0.0" -and -not [bool]$Summary.repositoryWideUpstreamParityClaimed) -Path $Path -Issue "ImgProc upstream summary identity or claim boundary drifted"
    Assert-True -List $List -Condition ([int]$Summary.declarationCount -eq 203 -and [int]$Summary.enumCount -eq 29 -and [int]$Summary.classCount -eq 7 -and [int]$Summary.callableCount -eq 167) -Path $Path -Issue "ImgProc upstream declaration counts drifted"
    Assert-True -List $List -Condition ([int]$Summary.classificationCounts.implemented -eq 161 -and [int]$Summary.classificationCounts.missing -eq 0 -and [int]$Summary.classificationCounts.'intentionally-omitted' -eq 6 -and [int]$Summary.classificationCounts.'non-callable-metadata' -eq 36) -Path $Path -Issue "ImgProc upstream classification counts drifted"
    Assert-True -List $List -Condition ($Summary.mappingSha256 -eq $MappingHash -and $Summary.familyInventorySha256 -eq $InventoryHash -and [int]$Summary.negativeFixtureCount -eq 10) -Path $Path -Issue "ImgProc upstream mapping/family hash or fixture count drifted"
    Assert-True -List $List -Condition ([int]$Summary.selectedFamilyCount -eq 8 -and [int]$Summary.selectedDeclarationCount -eq 90 -and [int]$Summary.managedPublicTypeAdditionCount -eq 11 -and [int]$Summary.managedPublicMemberAdditionCount -eq 174) -Path $Path -Issue "ImgProc selected-family or managed-addition counts drifted"

    $expectedIds = @(
        "generalized-hough-object-model",
        "color-conversion-and-visualization",
        "filter-gradient-and-masked-threshold",
        "drawing-markers-polygons-and-text-scale",
        "calibration-sampling-and-coordinate-workflows",
        "accumulation-registration-and-matching-workflows",
        "segmentation-and-link-runs-workflows",
        "font-face-object-and-rendering")
    $expectedCounts = @(47, 6, 3, 4, 6, 9, 5, 10)
    $expectedTests = @(
        "tests/OpenCvSharp.Tests/ImgProc/ImgProcUpstreamParityTests.cs",
        "tests/OpenCvSharp.Tests/ImgProc/ImgProcUpstreamParityTests.cs",
        "tests/OpenCvSharp.Tests/ImgProc/ImgProcUpstreamParityTests.cs",
        "tests/OpenCvSharp.Tests/ImgProc/ImgProcUpstreamParityTests.cs",
        "tests/OpenCvSharp.Tests/ImgProc/ImgProcRemainingParityTests.cs",
        "tests/OpenCvSharp.Tests/ImgProc/ImgProcRemainingParityTests.cs",
        "tests/OpenCvSharp.Tests/ImgProc/ImgProcRemainingParityTests.cs",
        "tests/OpenCvSharp.Tests/ImgProc/ImgProcRemainingParityTests.cs")
    $actualIds = @($Inventory.families | ForEach-Object { [string]$_.id })
    Assert-True -List $List -Condition ($Inventory.schemaVersion -eq 1 -and $Inventory.status -eq "implemented-verified" -and $Inventory.upstreamOpenCvVersion -eq "5.0.0" -and ($actualIds -join ",") -eq ($expectedIds -join ",")) -Path $Path -Issue "ImgProc family inventory identity, status, or order drifted"
    Assert-True -List $List -Condition ([int]$Inventory.managedPublicTypeAdditionCount -eq 11 -and [int]$Inventory.managedPublicMemberAdditionCount -eq 174) -Path $Path -Issue "ImgProc family managed addition counts drifted"
    for ($index = 0; $index -lt $expectedIds.Count; $index++) {
        $family = $Inventory.families[$index]
        Assert-True -List $List -Condition (@($family.declarations).Count -eq $expectedCounts[$index] -and -not [string]::IsNullOrWhiteSpace([string]$family.rationale)) -Path $Path -Issue "ImgProc family declaration count or rationale drifted" -Text $family.id
        foreach ($declaration in $family.declarations) {
            Assert-True -List $List -Condition ($declaration.upstreamClassification -in @("implemented", "intentionally-omitted", "non-callable-metadata")) -Path $Path -Issue "Selected ImgProc declaration has an unexpected classification" -Text $declaration.upstreamIdentity
            if ($declaration.upstreamClassification -in @("implemented", "intentionally-omitted")) {
                Assert-True -List $List -Condition (@($declaration.nativeEntrypoints).Count -gt 0 -and @($declaration.managedMembers).Count -gt 0) -Path $Path -Issue "Selected callable declaration is missing native or managed evidence" -Text $declaration.upstreamIdentity
            }
            Assert-True -List $List -Condition ($declaration.focusedTest -eq $expectedTests[$index] -and $declaration.nativeSmoke -eq "src/OpenCvSharp.Native/tests/native_smoke.cpp" -and $declaration.sample -eq "samples/ConsoleSamples/Program.cs" -and $declaration.guide -eq "docs/articles/imgproc-upstream-parity-guide.md") -Path $Path -Issue "Selected ImgProc declaration test/sample/guide evidence drifted" -Text $declaration.upstreamIdentity
        }
    }
}

function Test-Calib3DFamilyInventory {
    param(
        [AllowNull()][object]$Summary,
        [AllowNull()][object]$Inventory,
        [Parameter(Mandatory)][string]$MappingHash,
        [Parameter(Mandatory)][string]$InventoryHash,
        [Parameter(Mandatory)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$List,
        [Parameter(Mandatory)][string]$Path
    )

    if ($null -eq $Summary -or $null -eq $Inventory) {
        Add-Violation -List $List -Path $Path -Issue "Calib3D upstream summary or family inventory is missing"
        return
    }

    Assert-True -List $List -Condition ($Summary.schemaVersion -eq 1 -and $Summary.generator -eq "tools/Calib3DUpstreamMap" -and $Summary.upstreamOpenCvVersion -eq "5.0.0" -and -not [bool]$Summary.repositoryWideUpstreamParityClaimed) -Path $Path -Issue "Calib3D upstream summary identity or claim boundary drifted"
    Assert-True -List $List -Condition ([int]$Summary.declarationCount -eq 194 -and [int]$Summary.enumCount -eq 22 -and [int]$Summary.classCount -eq 5 -and [int]$Summary.callableCount -eq 167 -and [int]$Summary.sourceHeaderCount -eq 4) -Path $Path -Issue "Calib3D upstream declaration or source-header counts drifted"
    Assert-True -List $List -Condition ([int]$Summary.classificationCounts.implemented -eq 167 -and [int]$Summary.classificationCounts.missing -eq 0 -and [int]$Summary.classificationCounts.'intentionally-omitted' -eq 0 -and [int]$Summary.classificationCounts.'non-callable-metadata' -eq 27) -Path $Path -Issue "Calib3D upstream classification counts drifted"
    Assert-True -List $List -Condition ($Summary.mappingSha256 -eq $MappingHash -and $Summary.familyInventorySha256 -eq $InventoryHash -and [int]$Summary.negativeFixtureCount -eq 11) -Path $Path -Issue "Calib3D upstream mapping/family hash or fixture count drifted"
    Assert-True -List $List -Condition ([int]$Summary.selectedFamilyCount -eq 11 -and [int]$Summary.selectedDeclarationCount -eq 194 -and [int]$Summary.managedPublicTypeAdditionCount -eq 12 -and [int]$Summary.managedPublicMemberAdditionCount -eq 120) -Path $Path -Issue "Calib3D selected-family or managed-addition counts drifted"

    $expectedIds = @(
        "geometry-2d-subdiv2d-object-model",
        "geometry-2d-primitives",
        "geometry-3d-usac-and-homography",
        "geometry-3d-pose-and-epipolar",
        "geometry-3d-affine-camera-and-fisheye",
        "stereo-rectification",
        "stereo-matcher-object-model",
        "stereo-bm-object-model",
        "stereo-sgbm-object-model",
        "stereo-disparity-utilities",
        "calibration-and-registration")
    $expectedCounts = @(23, 30, 15, 31, 21, 3, 15, 19, 13, 5, 19)
    $actualIds = @($Inventory.families | ForEach-Object { [string]$_.id })
    Assert-True -List $List -Condition ($Inventory.schemaVersion -eq 1 -and $Inventory.status -eq "implemented-verified" -and $Inventory.upstreamOpenCvVersion -eq "5.0.0" -and ($actualIds -join ",") -eq ($expectedIds -join ",")) -Path $Path -Issue "Calib3D family inventory identity, status, or order drifted"
    Assert-True -List $List -Condition ([int]$Inventory.managedPublicTypeAdditionCount -eq 12 -and [int]$Inventory.managedPublicMemberAdditionCount -eq 120) -Path $Path -Issue "Calib3D family managed addition counts drifted"
    for ($index = 0; $index -lt $expectedIds.Count; $index++) {
        $family = $Inventory.families[$index]
        Assert-True -List $List -Condition (@($family.declarations).Count -eq $expectedCounts[$index] -and -not [string]::IsNullOrWhiteSpace([string]$family.rationale)) -Path $Path -Issue "Calib3D family declaration count or rationale drifted" -Text $family.id
        foreach ($declaration in $family.declarations) {
            Assert-True -List $List -Condition ($declaration.upstreamClassification -in @("implemented", "non-callable-metadata")) -Path $Path -Issue "Calib3D declaration has an unexpected classification" -Text $declaration.upstreamIdentity
            if ($declaration.upstreamClassification -eq "implemented") {
                Assert-True -List $List -Condition (@($declaration.nativeEntrypoints).Count -gt 0 -and @($declaration.managedMembers).Count -gt 0) -Path $Path -Issue "Calib3D callable declaration is missing native or managed evidence" -Text $declaration.upstreamIdentity
            }
            Assert-True -List $List -Condition ($declaration.focusedTest -eq "tests/OpenCvSharp.Tests/Calib3D/Calib3DUpstreamParityTests.cs" -and $declaration.nativeSmoke -eq "src/OpenCvSharp.Native/tests/native_smoke.cpp" -and $declaration.sample -eq "samples/ConsoleSamples/Program.cs" -and $declaration.guide -eq "docs/articles/calib3d-upstream-parity-guide.md") -Path $Path -Issue "Calib3D test/sample/guide evidence drifted" -Text $declaration.upstreamIdentity
        }
    }
}

function Test-FeaturesFamilyInventory {
    param(
        [AllowNull()][object]$Summary,
        [AllowNull()][object]$Inventory,
        [Parameter(Mandatory)][string]$MappingHash,
        [Parameter(Mandatory)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$List,
        [Parameter(Mandatory)][string]$Path
    )

    if ($null -eq $Summary -or $null -eq $Inventory) {
        Add-Violation -List $List -Path $Path -Issue "Features upstream summary or family inventory is missing"
        return
    }

    Assert-True -List $List -Condition ($Summary.schemaVersion -eq 1 -and $Summary.generator -eq "tools/FeaturesUpstreamMap" -and $Summary.upstreamOpenCvVersion -eq "5.0.0" -and -not [bool]$Summary.repositoryWideUpstreamParityClaimed) -Path $Path -Issue "Features upstream summary identity or claim boundary drifted"
    Assert-True -List $List -Condition ([int]$Summary.declarationCount -eq 183 -and [int]$Summary.enumCount -eq 6 -and [int]$Summary.classCount -eq 17 -and [int]$Summary.callableCount -eq 160) -Path $Path -Issue "Features upstream declaration counts drifted"
    Assert-True -List $List -Condition ([int]$Summary.classificationCounts.implemented -eq 134 -and [int]$Summary.classificationCounts.'intentionally-omitted' -eq 26 -and [int]$Summary.classificationCounts.missing -eq 0 -and [int]$Summary.classificationCounts.'non-callable-metadata' -eq 23 -and [int]$Summary.classificationCounts.unsupported -eq 0 -and [int]$Summary.classificationCounts.'upstream-conditional' -eq 0) -Path $Path -Issue "Features upstream classification counts drifted"
    Assert-True -List $List -Condition ($Summary.mappingSha256 -eq $MappingHash -and $Summary.familyInventorySha256 -match "^[0-9a-f]{64}$" -and $Summary.sourceReviewedExtensionSha256 -match "^[0-9a-f]{64}$" -and [int]$Summary.negativeFixtureCount -eq 15) -Path $Path -Issue "Features upstream mapping, inventory, extension, or fixture evidence drifted"
    Assert-True -List $List -Condition ([int]$Summary.compatibilityHeaderCount -eq 2 -and [int]$Summary.sourceHeaderCount -eq 1 -and [int]$Summary.sourceReviewedExtensionDeclarationCount -eq 9 -and [int]$Summary.selectedFamilyCount -eq 1 -and [int]$Summary.selectedDeclarationCount -eq 12 -and [int]$Summary.managedPublicTypeAdditionCount -eq 2 -and [int]$Summary.managedPublicMemberAdditionCount -eq 18) -Path $Path -Issue "Features source, selected-family, or managed-addition counts drifted"

    $families = @($Inventory.families)
    Assert-True -List $List -Condition ($Inventory.schemaVersion -eq 1 -and $Inventory.status -eq "implemented-verified" -and $Inventory.upstreamOpenCvVersion -eq "5.0.0" -and [int]$Inventory.managedPublicTypeAdditionCount -eq 2 -and [int]$Inventory.managedPublicMemberAdditionCount -eq 18 -and $families.Count -eq 1 -and $families[0].id -eq "features-ann-index") -Path $Path -Issue "Features family inventory identity, status, or order drifted"
    if ($families.Count -ne 1) { return }

    $family = $families[0]
    Assert-True -List $List -Condition (@($family.declarations).Count -eq 12 -and -not [string]::IsNullOrWhiteSpace([string]$family.rationale)) -Path $Path -Issue "Features ANNIndex declaration count or rationale drifted"
    foreach ($declaration in $family.declarations) {
        Assert-True -List $List -Condition ($declaration.upstreamClassification -in @("implemented", "non-callable-metadata")) -Path $Path -Issue "Features ANNIndex declaration has an unexpected classification" -Text $declaration.upstreamIdentity
        if ($declaration.upstreamClassification -eq "implemented") {
            Assert-True -List $List -Condition (@($declaration.nativeEntrypoints).Count -gt 0 -and @($declaration.managedMembers).Count -gt 0) -Path $Path -Issue "Features ANNIndex callable declaration is missing native or managed evidence" -Text $declaration.upstreamIdentity
        }
        Assert-True -List $List -Condition ($declaration.focusedTest -eq "tests/OpenCvSharp.Tests/Features2D/ANNIndexTests.cs" -and $declaration.nativeSmoke -eq "src/OpenCvSharp.Native/tests/native_smoke.cpp" -and $declaration.sample -eq "samples/ConsoleSamples/Program.cs" -and $declaration.guide -eq "docs/articles/features-upstream-parity-guide.md") -Path $Path -Issue "Features ANNIndex test/sample/guide evidence drifted" -Text $declaration.upstreamIdentity
    }
}

function Test-ObjDetectFamilyInventory {
    param(
        [AllowNull()][object]$Summary,
        [AllowNull()][object]$Inventory,
        [Parameter(Mandatory)][string]$MappingHash,
        [Parameter(Mandatory)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$List,
        [Parameter(Mandatory)][string]$Path
    )

    if ($null -eq $Summary -or $null -eq $Inventory) {
        Add-Violation -List $List -Path $Path -Issue "ObjDetect upstream summary or family inventory is missing"
        return
    }

    Assert-True -List $List -Condition ($Summary.schemaVersion -eq 1 -and $Summary.generator -eq "tools/ObjDetectUpstreamMap" -and $Summary.upstreamOpenCvVersion -eq "5.0.0" -and -not [bool]$Summary.repositoryWideUpstreamParityClaimed) -Path $Path -Issue "ObjDetect upstream summary identity or claim boundary drifted"
    Assert-True -List $List -Condition ([int]$Summary.declarationCount -eq 195 -and [int]$Summary.enumCount -eq 10 -and [int]$Summary.classCount -eq 22 -and [int]$Summary.callableCount -eq 163) -Path $Path -Issue "ObjDetect upstream declaration counts drifted"
    Assert-True -List $List -Condition ([int]$Summary.classificationCounts.implemented -eq 153 -and [int]$Summary.classificationCounts.'intentionally-omitted' -eq 10 -and [int]$Summary.classificationCounts.missing -eq 0 -and [int]$Summary.classificationCounts.'non-callable-metadata' -eq 32 -and [int]$Summary.classificationCounts.unsupported -eq 0 -and [int]$Summary.classificationCounts.'upstream-conditional' -eq 0) -Path $Path -Issue "ObjDetect upstream classification counts drifted"
    Assert-True -List $List -Condition ($Summary.mappingSha256 -eq $MappingHash -and $Summary.familyInventorySha256 -match "^[0-9a-f]{64}$" -and [int]$Summary.negativeFixtureCount -eq 15) -Path $Path -Issue "ObjDetect upstream mapping, inventory, or fixture evidence drifted"
    Assert-True -List $List -Condition ([int]$Summary.compatibilityHeaderCount -eq 2 -and [int]$Summary.sourceHeaderCount -eq 9 -and [int]$Summary.selectedFamilyCount -eq 1 -and [int]$Summary.selectedDeclarationCount -eq 33 -and [int]$Summary.managedPublicTypeAdditionCount -eq 4 -and [int]$Summary.managedPublicMemberAdditionCount -eq 55 -and [int]$Summary.nativeEntrypointAdditionCount -eq 35) -Path $Path -Issue "ObjDetect source, selected-family, or addition counts drifted"

    $families = @($Inventory.families)
    Assert-True -List $List -Condition ($Inventory.schemaVersion -eq 1 -and $Inventory.status -eq "implemented-verified" -and $Inventory.upstreamOpenCvVersion -eq "5.0.0" -and [int]$Inventory.managedPublicTypeAdditionCount -eq 4 -and [int]$Inventory.managedPublicMemberAdditionCount -eq 55 -and [int]$Inventory.nativeEntrypointAdditionCount -eq 35 -and $families.Count -eq 1 -and $families[0].id -eq "objdetect-structured-parity") -Path $Path -Issue "ObjDetect family inventory identity, status, or order drifted"
    if ($families.Count -ne 1) { return }

    $family = $families[0]
    Assert-True -List $List -Condition (@($family.declarations).Count -eq 33 -and -not [string]::IsNullOrWhiteSpace([string]$family.rationale)) -Path $Path -Issue "ObjDetect selected-family declaration count or rationale drifted"
    foreach ($declaration in $family.declarations) {
        Assert-True -List $List -Condition (@($declaration.nativeEntrypoints).Count -gt 0 -and @($declaration.managedMembers).Count -gt 0) -Path $Path -Issue "ObjDetect callable declaration is missing native or managed evidence" -Text $declaration.upstreamIdentity
        Assert-True -List $List -Condition ($declaration.focusedTest -eq "tests/OpenCvSharp.Tests/ObjDetect/ObjDetectStructuredParityTests.cs" -and $declaration.nativeSmoke -eq "src/OpenCvSharp.Native/tests/native_smoke.cpp" -and $declaration.sample -eq "samples/ConsoleSamples/Program.cs" -and $declaration.guide -eq "docs/articles/objdetect-structured-parity-guide.md") -Path $Path -Issue "ObjDetect test/sample/guide evidence drifted" -Text $declaration.upstreamIdentity
    }
}

foreach ($path in @($managedBaselinePath, $managedSummaryPath, $gapInventoryPath, $nativeFullPath, $nativeMiniPath, $bindingMapPath, $bindingSummaryPath, $spanFamilyPath, $imgProcMapPath, $imgProcSummaryPath, $imgProcFamilyPath, $imgCodecsMapPath, $imgCodecsSummaryPath, $imgCodecsFamilyPath, $imgCodecsExtensionsPath, $videoIOMapPath, $videoIOSummaryPath, $videoIOFamilyPath, $videoIORegistryPath, $calib3DMapPath, $calib3DSummaryPath, $calib3DFamilyPath, $coreMapPath, $coreSummaryPath, $coreFamilyPath, $dnnMapPath, $dnnSummaryPath, $dnnFamilyPath, $featuresMapPath, $featuresSummaryPath, $featuresFamilyPath, $featuresExtensionsPath, $objDetectMapPath, $objDetectSummaryPath, $objDetectFamilyPath, $objDetectRawPath)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Required API/ABI baseline file was not found: $path"
    }
}

$managedGeneratorArguments = @{
    RepositoryRoot = $repo
    Check = $true
}
if (-not [string]::IsNullOrWhiteSpace($DotNetPath)) {
    $managedGeneratorArguments.DotNetPath = $DotNetPath
}
& (Join-Path $repo "scripts/Generate-ManagedPublicApiBaseline.ps1") @managedGeneratorArguments
if (-not $?) { throw "Managed API generated-file freshness check failed." }
& (Join-Path $repo "scripts/Generate-NativeAbiCompatibility.ps1") -RepositoryRoot $repo -Check
if (-not $?) { throw "Native ABI generated-file freshness check failed." }
$imgProcGuardArguments = @{ RepositoryRoot = $repo }
if (-not [string]::IsNullOrWhiteSpace($DotNetPath)) { $imgProcGuardArguments.DotNetPath = $DotNetPath }
& (Join-Path $repo "scripts/Test-ImgProcUpstreamMap.ps1") @imgProcGuardArguments
if (-not $?) { throw "ImgProc upstream-map generated-file freshness check failed." }
$imgCodecsGuardArguments = @{ RepositoryRoot = $repo }
if (-not [string]::IsNullOrWhiteSpace($DotNetPath)) { $imgCodecsGuardArguments.DotNetPath = $DotNetPath }
& (Join-Path $repo "scripts/Test-ImgCodecsUpstreamMap.ps1") @imgCodecsGuardArguments
if (-not $?) { throw "ImgCodecs upstream-map generated-file freshness check failed." }
$videoIOGuardArguments = @{ RepositoryRoot = $repo }
if (-not [string]::IsNullOrWhiteSpace($DotNetPath)) { $videoIOGuardArguments.DotNetPath = $DotNetPath }
& (Join-Path $repo "scripts/Test-VideoIOUpstreamMap.ps1") @videoIOGuardArguments
if (-not $?) { throw "VideoIO upstream-map generated-file freshness check failed." }
& (Join-Path $repo "scripts/Test-VideoIORegistrySurface.ps1") -RepositoryRoot $repo
if (-not $?) { throw "VideoIO registry-surface check failed." }
$calib3DGuardArguments = @{ RepositoryRoot = $repo }
if (-not [string]::IsNullOrWhiteSpace($DotNetPath)) { $calib3DGuardArguments.DotNetPath = $DotNetPath }
& (Join-Path $repo "scripts/Test-Calib3DUpstreamMap.ps1") @calib3DGuardArguments
if (-not $?) { throw "Calib3D upstream-map generated-file freshness check failed." }
$coreGuardArguments = @{ RepositoryRoot = $repo }
if (-not [string]::IsNullOrWhiteSpace($DotNetPath)) { $coreGuardArguments.DotNetPath = $DotNetPath }
& (Join-Path $repo "scripts/Test-CoreUpstreamMap.ps1") @coreGuardArguments
if (-not $?) { throw "Core upstream-map generated-file freshness check failed." }
$dnnGuardArguments = @{ RepositoryRoot = $repo }
if (-not [string]::IsNullOrWhiteSpace($DotNetPath)) { $dnnGuardArguments.DotNetPath = $DotNetPath }
& (Join-Path $repo "scripts/Test-DnnUpstreamMap.ps1") @dnnGuardArguments
if (-not $?) { throw "DNN upstream-map generated-file freshness check failed." }
$featuresGuardArguments = @{ RepositoryRoot = $repo }
if (-not [string]::IsNullOrWhiteSpace($DotNetPath)) { $featuresGuardArguments.DotNetPath = $DotNetPath }
& (Join-Path $repo "scripts/Test-FeaturesUpstreamMap.ps1") @featuresGuardArguments
if (-not $?) { throw "Features upstream-map generated-file freshness check failed." }
$objDetectGuardArguments = @{ RepositoryRoot = $repo }
if (-not [string]::IsNullOrWhiteSpace($DotNetPath)) { $objDetectGuardArguments.DotNetPath = $DotNetPath }
& (Join-Path $repo "scripts/Test-ObjDetectUpstreamMap.ps1") @objDetectGuardArguments
if (-not $?) { throw "ObjDetect upstream-map generated-file freshness check failed." }
$photoGuardArguments = @{ RepositoryRoot = $repo }
if (-not [string]::IsNullOrWhiteSpace($DotNetPath)) { $photoGuardArguments.DotNetPath = $DotNetPath }
& (Join-Path $repo "scripts/Test-PhotoUpstreamMap.ps1") @photoGuardArguments
if (-not $?) { throw "Photo upstream-map generated-file freshness check failed." }
$videoGuardArguments = @{ RepositoryRoot = $repo }
if (-not [string]::IsNullOrWhiteSpace($DotNetPath)) { $videoGuardArguments.DotNetPath = $DotNetPath }
& (Join-Path $repo "scripts/Test-VideoUpstreamMap.ps1") @videoGuardArguments
if (-not $?) { throw "Video upstream-map generated-file freshness check failed." }

$managedText = [IO.File]::ReadAllText($managedBaselinePath)
$managedSummary = Get-Content -LiteralPath $managedSummaryPath -Raw | ConvertFrom-Json
$gapInventory = Get-Content -LiteralPath $gapInventoryPath -Raw | ConvertFrom-Json
$nativeFullText = [IO.File]::ReadAllText($nativeFullPath)
$nativeMiniText = [IO.File]::ReadAllText($nativeMiniPath)
$bindingMapText = [IO.File]::ReadAllText($bindingMapPath)
$bindingSummary = Get-Content -LiteralPath $bindingSummaryPath -Raw | ConvertFrom-Json
$spanFamily = Get-Content -LiteralPath $spanFamilyPath -Raw | ConvertFrom-Json
$imgProcMapText = [IO.File]::ReadAllText($imgProcMapPath)
$imgProcSummary = Get-Content -LiteralPath $imgProcSummaryPath -Raw | ConvertFrom-Json
$imgProcFamilyText = [IO.File]::ReadAllText($imgProcFamilyPath)
$imgProcFamily = $imgProcFamilyText | ConvertFrom-Json
$imgCodecsMapText = [IO.File]::ReadAllText($imgCodecsMapPath)
$imgCodecsSummary = Get-Content -LiteralPath $imgCodecsSummaryPath -Raw | ConvertFrom-Json
$videoIOMapText = [IO.File]::ReadAllText($videoIOMapPath)
$videoIOSummary = Get-Content -LiteralPath $videoIOSummaryPath -Raw | ConvertFrom-Json
$calib3DMapText = [IO.File]::ReadAllText($calib3DMapPath)
$calib3DSummary = Get-Content -LiteralPath $calib3DSummaryPath -Raw | ConvertFrom-Json
$calib3DFamilyText = [IO.File]::ReadAllText($calib3DFamilyPath)
$calib3DFamily = $calib3DFamilyText | ConvertFrom-Json
$coreMapText = [IO.File]::ReadAllText($coreMapPath)
$coreSummary = Get-Content -LiteralPath $coreSummaryPath -Raw | ConvertFrom-Json
$dnnMapText = [IO.File]::ReadAllText($dnnMapPath)
$dnnSummary = Get-Content -LiteralPath $dnnSummaryPath -Raw | ConvertFrom-Json
$featuresMapText = [IO.File]::ReadAllText($featuresMapPath)
$featuresSummary = Get-Content -LiteralPath $featuresSummaryPath -Raw | ConvertFrom-Json
$featuresFamilyText = [IO.File]::ReadAllText($featuresFamilyPath)
$featuresFamily = $featuresFamilyText | ConvertFrom-Json
$objDetectMapText = [IO.File]::ReadAllText($objDetectMapPath)
$objDetectSummary = Get-Content -LiteralPath $objDetectSummaryPath -Raw | ConvertFrom-Json
$objDetectFamilyText = [IO.File]::ReadAllText($objDetectFamilyPath)
$objDetectFamily = $objDetectFamilyText | ConvertFrom-Json
$photoMapText = [IO.File]::ReadAllText($photoMapPath)
$photoSummary = Get-Content -LiteralPath $photoSummaryPath -Raw | ConvertFrom-Json
$videoMapText = [IO.File]::ReadAllText($videoMapPath)
$videoSummary = Get-Content -LiteralPath $videoSummaryPath -Raw | ConvertFrom-Json
$managedHash = Get-TextSha256 $managedText
$nativeFullHash = (Get-FileHash -LiteralPath $nativeFullPath -Algorithm SHA256).Hash.ToLowerInvariant()
$nativeMiniHash = (Get-FileHash -LiteralPath $nativeMiniPath -Algorithm SHA256).Hash.ToLowerInvariant()
$bindingMapHash = Get-TextSha256 $bindingMapText
$imgProcMapHash = Get-TextSha256 $imgProcMapText
$imgProcFamilyHash = Get-TextSha256 $imgProcFamilyText
$imgCodecsMapHash = Get-TextSha256 $imgCodecsMapText
$videoIOMapHash = Get-TextSha256 $videoIOMapText
$videoIORegistryHash = (Get-FileHash -LiteralPath $videoIORegistryPath -Algorithm SHA256).Hash.ToLowerInvariant()
$calib3DMapHash = Get-TextSha256 $calib3DMapText
$calib3DFamilyHash = Get-TextSha256 $calib3DFamilyText
$coreMapHash = Get-TextSha256 $coreMapText
$dnnMapHash = Get-TextSha256 $dnnMapText
$featuresMapHash = Get-TextSha256 $featuresMapText
$objDetectMapHash = Get-TextSha256 $objDetectMapText
$objDetectRawHash = (Get-FileHash -LiteralPath $objDetectRawPath -Algorithm SHA256).Hash.ToLowerInvariant()
$photoMapHash = Get-TextSha256 $photoMapText
$photoRawHash = (Get-FileHash -LiteralPath $photoRawPath -Algorithm SHA256).Hash.ToLowerInvariant()
$videoMapHash = Get-TextSha256 $videoMapText
$videoRawHash = (Get-FileHash -LiteralPath $videoRawPath -Algorithm SHA256).Hash.ToLowerInvariant()

Test-ManagedBaselineDocument -Text $managedText -Summary $managedSummary -List $violations -Path "compatibility/managed-public-api.txt"
Test-NativeManifestDocument -Text $nativeFullText -ExpectedCount 2438 -Mini $false -List $violations -Path "src/OpenCvSharp.Native/generated/legacy_abi_manifest.txt"
Test-NativeManifestDocument -Text $nativeMiniText -ExpectedCount 526 -Mini $true -List $violations -Path "src/OpenCvSharp.Native/generated/legacy_abi_mini_manifest.txt"
Test-GapInventory -Inventory $gapInventory -Summary $managedSummary -ManagedHash $managedHash -NativeFullHash $nativeFullHash -NativeMiniHash $nativeMiniHash -BindingSummary $bindingSummary -BindingMapHash $bindingMapHash -ImgProcSummary $imgProcSummary -ImgProcMapHash $imgProcMapHash -ImgCodecsSummary $imgCodecsSummary -ImgCodecsMapHash $imgCodecsMapHash -VideoIOSummary $videoIOSummary -VideoIOMapHash $videoIOMapHash -VideoIORegistryHash $videoIORegistryHash -Calib3DSummary $calib3DSummary -Calib3DMapHash $calib3DMapHash -CoreSummary $coreSummary -CoreMapHash $coreMapHash -DnnSummary $dnnSummary -DnnMapHash $dnnMapHash -FeaturesSummary $featuresSummary -FeaturesMapHash $featuresMapHash -ObjDetectSummary $objDetectSummary -ObjDetectMapHash $objDetectMapHash -ObjDetectRawHash $objDetectRawHash -PhotoSummary $photoSummary -PhotoMapHash $photoMapHash -PhotoRawHash $photoRawHash -VideoSummary $videoSummary -VideoMapHash $videoMapHash -VideoRawHash $videoRawHash -List $violations -Path "compatibility/api-gap-inventory.json"
Test-SpanFamilyInventory -Inventory $spanFamily -List $violations -Path "compatibility/imgproc-point-set-span-family.json"
Test-ImgProcFamilyInventory -Summary $imgProcSummary -Inventory $imgProcFamily -MappingHash $imgProcMapHash -InventoryHash $imgProcFamilyHash -List $violations -Path "compatibility/imgproc-implemented-families.json"
Test-Calib3DFamilyInventory -Summary $calib3DSummary -Inventory $calib3DFamily -MappingHash $calib3DMapHash -InventoryHash $calib3DFamilyHash -List $violations -Path "compatibility/calib3d-implemented-families.json"
Test-FeaturesFamilyInventory -Summary $featuresSummary -Inventory $featuresFamily -MappingHash $featuresMapHash -List $violations -Path "compatibility/features-implemented-families.json"
Test-ObjDetectFamilyInventory -Summary $objDetectSummary -Inventory $objDetectFamily -MappingHash $objDetectMapHash -List $violations -Path "compatibility/objdetect-implemented-families.json"

$baselineLines = @((Normalize-Text $managedText).TrimEnd("`n").Split("`n"))
$typesIndex = [Array]::IndexOf($baselineLines, "[types]")
$membersIndex = [Array]::IndexOf($baselineLines, "[members]")

Assert-FixtureRejected -Name "missing managed type" -ExpectedIssue "type count" -Action {
    param($list)
    $fixture = [Collections.Generic.List[string]]::new()
    $fixture.AddRange([string[]]$baselineLines)
    $fixture.RemoveAt($typesIndex + 1)
    Test-ManagedBaselineDocument -Text (($fixture -join "`n") + "`n") -Summary $managedSummary -List $list -Path "fixture/missing-managed-type.txt"
}
Assert-FixtureRejected -Name "duplicate managed member" -ExpectedIssue "duplicate member" -Action {
    param($list)
    $fixture = [Collections.Generic.List[string]]::new()
    $fixture.AddRange([string[]]$baselineLines)
    $fixture.Insert($membersIndex + 2, $fixture[$membersIndex + 1])
    $summaryFixture = $managedSummary | ConvertTo-Json -Depth 20 | ConvertFrom-Json
    $summaryFixture.memberCount = [int]$summaryFixture.memberCount + 1
    Test-ManagedBaselineDocument -Text (($fixture -join "`n") + "`n") -Summary $summaryFixture -List $list -Path "fixture/duplicate-managed-member.txt"
}
Assert-FixtureRejected -Name "reordered managed types" -ExpectedIssue "ordinal ordering" -Action {
    param($list)
    $fixture = [string[]]$baselineLines.Clone()
    $temporary = $fixture[$typesIndex + 1]
    $fixture[$typesIndex + 1] = $fixture[$typesIndex + 2]
    $fixture[$typesIndex + 2] = $temporary
    Test-ManagedBaselineDocument -Text (($fixture -join "`n") + "`n") -Summary $managedSummary -List $list -Path "fixture/reordered-managed-types.txt"
}
Assert-FixtureRejected -Name "fixed-major managed identity" -ExpectedIssue "unexpected fixed-major" -Action {
    param($list)
    $fixture = [string[]]$baselineLines.Clone()
    $fixture[$typesIndex + 1] = $fixture[$typesIndex + 1] -replace "OpenCvSharp\.", "OpenCv6Sharp."
    Test-ManagedBaselineDocument -Text (($fixture -join "`n") + "`n") -Summary $managedSummary -List $list -Path "fixture/fixed-major-managed-identity.txt"
}
Assert-FixtureRejected -Name "stale managed hash" -ExpectedIssue "SHA256" -Action {
    param($list)
    $summaryFixture = $managedSummary | ConvertTo-Json -Depth 20 | ConvertFrom-Json
    $summaryFixture.baselineSha256 = "0" * 64
    Test-ManagedBaselineDocument -Text $managedText -Summary $summaryFixture -List $list -Path "fixture/stale-managed-hash.txt"
}
Assert-FixtureRejected -Name "reordered native ABI" -ExpectedIssue "ordinal function-name ordering" -Action {
    param($list)
    $fixture = @((Normalize-Text $nativeFullText).TrimEnd("`n").Split("`n"))
    $index = [Array]::IndexOf($fixture, "[functions]") + 1
    $temporary = $fixture[$index]
    $fixture[$index] = $fixture[$index + 1]
    $fixture[$index + 1] = $temporary
    Test-NativeManifestDocument -Text (($fixture -join "`n") + "`n") -ExpectedCount 2438 -Mini $false -List $list -Path "fixture/reordered-native-abi.txt"
}
Assert-FixtureRejected -Name "fixed-major primary native ABI" -ExpectedIssue "primary/compatibility identity" -Action {
    param($list)
    $fixture = @((Normalize-Text $nativeFullText).TrimEnd("`n").Split("`n"))
    $index = [Array]::IndexOf($fixture, "[functions]") + 1
    $fixture[$index] = $fixture[$index] -replace "^jyppx_ocv_", "jyppx_ocv6_"
    Test-NativeManifestDocument -Text (($fixture -join "`n") + "`n") -ExpectedCount 2438 -Mini $false -List $list -Path "fixture/fixed-major-primary-native-abi.txt"
}
Assert-FixtureRejected -Name "missing gap inventory" -ExpectedIssue "gap inventory is missing" -Action {
    param($list)
    Test-GapInventory -Inventory $null -Summary $managedSummary -ManagedHash $managedHash -NativeFullHash $nativeFullHash -NativeMiniHash $nativeMiniHash -BindingSummary $bindingSummary -BindingMapHash $bindingMapHash -ImgProcSummary $imgProcSummary -ImgProcMapHash $imgProcMapHash -ImgCodecsSummary $imgCodecsSummary -ImgCodecsMapHash $imgCodecsMapHash -VideoIOSummary $videoIOSummary -VideoIOMapHash $videoIOMapHash -VideoIORegistryHash $videoIORegistryHash -Calib3DSummary $calib3DSummary -Calib3DMapHash $calib3DMapHash -CoreSummary $coreSummary -CoreMapHash $coreMapHash -DnnSummary $dnnSummary -DnnMapHash $dnnMapHash -FeaturesSummary $featuresSummary -FeaturesMapHash $featuresMapHash -ObjDetectSummary $objDetectSummary -ObjDetectMapHash $objDetectMapHash -ObjDetectRawHash $objDetectRawHash -PhotoSummary $photoSummary -PhotoMapHash $photoMapHash -PhotoRawHash $photoRawHash -VideoSummary $videoSummary -VideoMapHash $videoMapHash -VideoRawHash $videoRawHash -List $list -Path "fixture/missing-gap-inventory.json"
}
Assert-FixtureRejected -Name "pending Span family" -ExpectedIssue "identity/status" -Action {
    param($list)
    $fixture = $spanFamily | ConvertTo-Json -Depth 20 | ConvertFrom-Json
    $fixture.status = "selected-implementation-pending"
    Test-SpanFamilyInventory -Inventory $fixture -List $list -Path "fixture/pending-span-family.json"
}

if ($violations.Count -gt 0) {
    Write-Host "API/ABI baseline contract failed with $($violations.Count) violation(s)."
    $violations | Sort-Object Path, Issue, Text | Format-Table Path, Issue, Text -AutoSize
    exit 1
}

Write-Host "API_ABI_BASELINE_OK managed_types=$($managedSummary.typeCount) managed_members=$($managedSummary.memberCount) namespaces=$($managedSummary.namespaceCount) native_full=2438 native_mini=526 imgproc_callables=167 imgproc_implemented=161 imgcodecs_callables=22 imgcodecs_implemented=22 videoio_callables=40 videoio_implemented=40 registry_operations=12 calib3d_callables=167 calib3d_implemented=167 core_callables=215 core_implemented=176 dnn_callables=159 dnn_implemented=70 features_callables=160 features_implemented=134 objdetect_callables=163 objdetect_implemented=153 photo_callables=120 photo_implemented=120 video_callables=145 video_implemented=138 video_missing=0 managed_sha256=$managedHash"
Write-Host "API/ABI baseline contract passed."
Write-Host "Negative fixtures rejected: missing type, duplicate member, reorder, fixed-major identity, stale hash, native reorder, native fixed-major primary prefix, missing gap inventory, pending Span family."

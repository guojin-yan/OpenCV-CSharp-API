[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [string]$OutputPath = 'compatibility/upstream-map-catalog.json'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$definitions = @(
    [pscustomobject]@{ Id = 'calib3d'; Tool = 'Calib3D'; Repository = 'opencv' },
    [pscustomobject]@{ Id = 'core'; Tool = 'Core'; Repository = 'opencv' },
    [pscustomobject]@{ Id = 'dnn'; Tool = 'Dnn'; Repository = 'opencv' },
    [pscustomobject]@{ Id = 'features'; Tool = 'Features'; Repository = 'opencv' },
    [pscustomobject]@{ Id = 'highgui'; Tool = 'HighGui'; Repository = 'opencv' },
    [pscustomobject]@{ Id = 'imgcodecs'; Tool = 'ImgCodecs'; Repository = 'opencv' },
    [pscustomobject]@{ Id = 'imgproc'; Tool = 'ImgProc'; Repository = 'opencv' },
    [pscustomobject]@{ Id = 'ml'; Tool = 'Ml'; Repository = 'opencv_contrib' },
    [pscustomobject]@{ Id = 'objdetect'; Tool = 'ObjDetect'; Repository = 'opencv' },
    [pscustomobject]@{ Id = 'photo'; Tool = 'Photo'; Repository = 'opencv' },
    [pscustomobject]@{ Id = 'stitching'; Tool = 'Stitching'; Repository = 'opencv' },
    [pscustomobject]@{ Id = 'tracking'; Tool = 'Tracking'; Repository = 'opencv_contrib' },
    [pscustomobject]@{ Id = 'video'; Tool = 'Video'; Repository = 'opencv' },
    [pscustomobject]@{ Id = 'videoio'; Tool = 'VideoIO'; Repository = 'opencv' },
    [pscustomobject]@{ Id = 'xphoto'; Tool = 'XPhoto'; Repository = 'opencv_contrib' }
)

function Get-HeaderRows {
    param([Parameter(Mandatory)][object]$Raw, [Parameter(Mandatory)][string]$PropertyName)
    $property = $Raw.PSObject.Properties[$PropertyName]
    if ($null -eq $property -or $null -eq $property.Value) { return @() }
    $rows = [System.Collections.Generic.List[object]]::new()
    foreach ($item in @($property.Value)) {
        if ($null -eq $item) { continue }
        if ($item -is [string]) {
            $rows.Add([ordered]@{ path = [string]$item; sha256 = '' })
        } else {
            $row = [ordered]@{ path = [string]$item.path; sha256 = '' }
            $hashProperty = $item.PSObject.Properties['sha256']
            if ($null -ne $hashProperty) { $row.sha256 = [string]$hashProperty.Value }
            foreach ($field in @('reason','includes','startOrdinal','declarationCount')) {
                $property = $item.PSObject.Properties[$field]
                if ($null -ne $property) { $row[$field] = $property.Value }
            }
            $rows.Add($row)
        }
    }
    return $rows.ToArray()
}

$mapRows = [System.Collections.Generic.List[object]]::new()
foreach ($definition in $definitions) {
    $id = [string]$definition.Id
    $rawRelative = "compatibility/$id-upstream-raw.json"
    $classificationRelative = "compatibility/$id-upstream-classifications.json"
    $summaryRelative = "compatibility/$id-upstream-summary.json"
    $rawPath = Join-Path $repo ($rawRelative -replace '/', [IO.Path]::DirectorySeparatorChar)
    $summaryPath = Join-Path $repo ($summaryRelative -replace '/', [IO.Path]::DirectorySeparatorChar)
    foreach ($path in @($rawPath,$summaryPath)) { if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Cannot construct upstream map catalog; required input missing: $path" } }
    $raw = Get-Content -LiteralPath $rawPath -Raw | ConvertFrom-Json
    $summary = Get-Content -LiteralPath $summaryPath -Raw | ConvertFrom-Json
    $tool = [string]$definition.Tool
    $generate = "scripts/Generate-$($tool)UpstreamMap.ps1"
    $verify = "scripts/Test-$($tool)UpstreamMap.ps1"
    $sourceHeaders = @(Get-HeaderRows -Raw $raw -PropertyName 'sourceHeaders')
    if (@($sourceHeaders).Count -eq 0) {
        $sourceHeaders = @([ordered]@{ path = [string]$raw.headerPath; sha256 = [string]$raw.headerSha256 })
    }
    $extensionPath = ''
    $extensionProperty = $summary.PSObject.Properties['sourceReviewedExtensionPath']
    if ($null -ne $extensionProperty) { $extensionPath = [string]$extensionProperty.Value }
    $mapRows.Add([ordered]@{
            id = $id
            repository = [string]$definition.Repository
            generatorProject = [string]$summary.generator
            extractorScript = [string]$raw.generator
            generationScript = $generate
            semanticGuard = $verify
            scope = [ordered]@{
                claimedSlice = [string]$summary.claimedSlice
                primaryHeader = [ordered]@{ path = [string]$raw.headerPath; sha256 = [string]$raw.headerSha256 }
                parser = [ordered]@{ path = [string]$raw.parserPath; sha256 = [string]$raw.parserSha256 }
                sourceHeaders = $sourceHeaders
                compatibilityHeaders = @(Get-HeaderRows -Raw $raw -PropertyName 'compatibilityHeaders')
                excludedPublicHeaders = @(Get-HeaderRows -Raw $raw -PropertyName 'excludedPublicHeaders')
            }
            artifacts = [ordered]@{
                raw = $rawRelative
                classifications = $classificationRelative
                mapping = [string]$summary.mappingPath
                summary = $summaryRelative
                familyInventory = [string]$summary.familyInventoryPath
                sourceReviewedExtensions = $extensionPath
            }
            evidenceContract = [ordered]@{
                declarations = [int]$summary.declarationCount
                negativeFixturesMinimum = 10
                mappingSha256 = [string]$summary.mappingSha256
                familyInventorySha256 = [string]$summary.familyInventorySha256
                sourceReviewedExtensionSha256 = if ($extensionPath.Length -gt 0) { [string]$summary.sourceReviewedExtensionSha256 } else { '' }
                zeroMissing = ([int]$summary.classificationCounts.missing -eq 0)
                repositoryWideParityClaimed = [bool]$summary.repositoryWideUpstreamParityClaimed
            }
        })
}

$catalog = [ordered]@{
    '$schema' = 'upstream-map-catalog.schema.json'
    schemaVersion = 1
    status = 'active'
    upstreamOpenCvVersion = '5.0.0'
    parser = [ordered]@{ path = 'opencv-source/opencv-5.0.0/modules/python/src2/hdr_parser.py'; sha256 = [string]((Get-Content (Join-Path $repo 'compatibility/imgproc-upstream-raw.json') -Raw | ConvertFrom-Json).parserSha256) }
    classificationPolicy = [ordered]@{
        allowed = @('implemented','missing','intentionally-omitted','upstream-conditional','unsupported','non-callable-metadata')
        requiredFields = @('ordinal','identity','classification','reason','nativeEntrypoints','managedMembers')
        implementedRequiresNativeAndManagedEvidence = $true
        unexplainedMissingAllowed = $false
    }
    moduleCount = $mapRows.Count
    maps = @($mapRows)
    limitations = @(
        'The catalog centralizes source/header scope, classification policy, artifact bindings, generator commands, and semantic guard commands for the existing parser-backed map set.',
        'Each module retains its specialized extraction and semantic implementation; catalog orchestration does not assert repository-wide OpenCV parity.',
        'Additional contrib modules are introduced only as explicit catalog rows after their source scope and classification evidence are reviewed.'
    )
}
$outputFullPath = if ([IO.Path]::IsPathRooted($OutputPath)) { [IO.Path]::GetFullPath($OutputPath) } else { [IO.Path]::GetFullPath((Join-Path $repo ($OutputPath -replace '/', [IO.Path]::DirectorySeparatorChar))) }
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $outputFullPath) | Out-Null
[IO.File]::WriteAllText($outputFullPath, (($catalog | ConvertTo-Json -Depth 20) + [Environment]::NewLine), [Text.UTF8Encoding]::new($false))
Write-Host "UPSTREAM_MAP_CATALOG_WRITTEN maps=$($mapRows.Count) output=$outputFullPath"

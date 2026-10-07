param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$workspace = (Resolve-Path -LiteralPath (Join-Path $repo '..')).Path
$catalogPath = Join-Path $repo 'compatibility/upstream-map-catalog.json'
$schemaPath = Join-Path $repo 'compatibility/upstream-map-catalog.schema.json'
foreach ($path in @($catalogPath,$schemaPath)) { if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Upstream map catalog input is missing: $path" } }
if (-not (Test-Json -LiteralPath $catalogPath -SchemaFile $schemaPath -ErrorAction Stop)) { throw 'Upstream map catalog failed JSON Schema validation.' }
$catalog = Get-Content -LiteralPath $catalogPath -Raw | ConvertFrom-Json
$pwsh = Get-Command pwsh -ErrorAction Stop
$expectedIds = @('calib3d','core','dnn','features','highgui','imgcodecs','imgproc','ml','objdetect','photo','stitching','tracking','video','videoio','xphoto','ximgproc','optflow','bgsegm','face','quality','img_hash','line_descriptor','freetype','alphamat','intensity_transform','plot','bioinspired','phase_unwrapping','hfs','fuzzy','rapid','shape')
$maps = @($catalog.maps)
if ($maps.Count -ne $expectedIds.Count) { throw 'Upstream map catalog module count drifted.' }
if ((@($maps | ForEach-Object { [string]$_.id }) -join '|') -cne ($expectedIds -join '|')) { throw 'Upstream map catalog IDs or stable ordering drifted.' }

function Assert-CatalogSchemaRejects {
    param([Parameter(Mandatory)][string]$Name, [Parameter(Mandatory)][scriptblock]$Mutation)
    $fixture = $catalog | ConvertTo-Json -Depth 30 | ConvertFrom-Json
    & $Mutation $fixture
    $json = $fixture | ConvertTo-Json -Depth 30
    if (Test-Json -Json $json -SchemaFile $schemaPath -ErrorAction SilentlyContinue) { throw "Upstream map catalog schema accepted negative fixture: $Name" }
}
Assert-CatalogSchemaRejects -Name 'promoted status' -Mutation { param($value) $value.status = 'complete' }
Assert-CatalogSchemaRejects -Name 'OpenCV version drift' -Mutation { param($value) $value.upstreamOpenCvVersion = '5.1.0' }
Assert-CatalogSchemaRejects -Name 'missing evidence requirement' -Mutation { param($value) $value.classificationPolicy.implementedRequiresNativeAndManagedEvidence = $false }
Assert-CatalogSchemaRejects -Name 'publishable parity claim' -Mutation { param($value) $value.maps[0].evidenceContract.repositoryWideParityClaimed = $true }

function Resolve-RepositoryPath {
    param([Parameter(Mandatory)][string]$RelativePath)
    if ([IO.Path]::IsPathRooted($RelativePath) -or $RelativePath -match '(^|[/\\])\.\.([/\\]|$)') { throw "Upstream catalog path must be repository-relative without traversal: $RelativePath" }
    [IO.Path]::GetFullPath((Join-Path $repo ($RelativePath -replace '/', [IO.Path]::DirectorySeparatorChar)))
}

function Assert-Hash {
    param([Parameter(Mandatory)][string]$Path, [Parameter(Mandatory)][string]$Expected, [Parameter(Mandatory)][string]$Description)
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) { throw "Upstream map $Description is missing: $Path" }
    $actual = (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
    if ($actual -cne $Expected) { throw "Upstream map $Description hash drifted. Expected $Expected, found ${actual}: $Path" }
}

function Assert-SourceHeader {
    param([Parameter(Mandatory)][object]$Header, [Parameter(Mandatory)][string]$Description, [switch]$AllowMissingExcluded)
    $relative = [string]$Header.path
    if ($relative -notmatch '^opencv-source/(opencv-5\.0\.0|opencv_contrib-5\.0\.0)/') { throw "Upstream map $Description header escaped the pinned source trees: $relative" }
    $path = [IO.Path]::GetFullPath((Join-Path $workspace ($relative -replace '/', [IO.Path]::DirectorySeparatorChar)))
    $expected = [string]$Header.sha256
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        if ($AllowMissingExcluded -and [string]::IsNullOrWhiteSpace($expected) -and -not [string]::IsNullOrWhiteSpace([string]$Header.reason)) { return }
        throw "Upstream map $Description source header is missing: $path"
    }
    if ([string]::IsNullOrWhiteSpace($expected)) {
        if ([string]::IsNullOrWhiteSpace([string]$Header.reason)) { throw "Unhashed upstream map header needs an explicit exclusion rationale: $relative" }
        return
    }
    Assert-Hash -Path $path -Expected $expected -Description "$Description source header"
}

function Assert-HeaderScopeSet {
    param(
        [Parameter(Mandatory)][object]$Raw,
        [Parameter(Mandatory)][AllowEmptyCollection()][object[]]$Configured,
        [Parameter(Mandatory)][string]$PropertyName,
        [Parameter(Mandatory)][string]$Id,
        [switch]$PrimaryFallback
    )
    $rawProperty = $Raw.PSObject.Properties[$PropertyName]
    $expected = @()
    if ($null -ne $rawProperty -and $null -ne $rawProperty.Value) { $expected = @($rawProperty.Value) }
    elseif ($PrimaryFallback) { $expected = @([ordered]@{ path = [string]$Raw.headerPath; sha256 = [string]$Raw.headerSha256 }) }
    if ($expected.Count -ne $Configured.Count) { throw "Upstream catalog $PropertyName scope count drifted for $Id." }
    foreach ($rawHeader in $expected) {
        $path = if ($rawHeader -is [string]) { [string]$rawHeader } else { [string]$rawHeader.path }
        $rows = @($Configured | Where-Object { [string]$_.path -ceq $path })
        if ($rows.Count -ne 1) { throw "Upstream catalog omitted or duplicated $PropertyName header for ${Id}: $path" }
        $configuredHeader = $rows[0]
        $rawHashProperty = if ($rawHeader -is [string]) { $null } else { $rawHeader.PSObject.Properties['sha256'] }
        $rawHash = if ($null -ne $rawHashProperty) { [string]$rawHashProperty.Value } elseif ($rawHeader -is [System.Collections.IDictionary] -and $rawHeader.Contains('sha256')) { [string]$rawHeader['sha256'] } else { '' }
        if ([string]$configuredHeader.sha256 -cne $rawHash) { throw "Upstream catalog $PropertyName header hash drifted for ${Id}: $path" }
        if ($rawHeader -isnot [string]) {
            foreach ($field in @('reason','includes','startOrdinal','declarationCount')) {
                $rawField = $rawHeader.PSObject.Properties[$field]
                if ($null -ne $rawField -and [string]$configuredHeader.$field -cne [string]$rawField.Value) { throw "Upstream catalog $PropertyName header metadata '$field' drifted for ${Id}: $path" }
            }
        }
    }
}

foreach ($map in $maps) {
    $id = [string]$map.id
    $artifactPaths = $map.artifacts
    $rawPath = Resolve-RepositoryPath -RelativePath ([string]$artifactPaths.raw)
    $classificationPath = Resolve-RepositoryPath -RelativePath ([string]$artifactPaths.classifications)
    $mappingPath = Resolve-RepositoryPath -RelativePath ([string]$artifactPaths.mapping)
    $summaryPath = Resolve-RepositoryPath -RelativePath ([string]$artifactPaths.summary)
    $familyPath = Resolve-RepositoryPath -RelativePath ([string]$artifactPaths.familyInventory)
    foreach ($path in @($rawPath,$classificationPath,$mappingPath,$summaryPath,$familyPath)) { if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Upstream map catalog artifact is missing for ${id}: $path" } }
    if (-not [string]::IsNullOrWhiteSpace([string]$artifactPaths.sourceReviewedExtensions)) {
        $extensionPath = Resolve-RepositoryPath -RelativePath ([string]$artifactPaths.sourceReviewedExtensions)
        if (-not (Test-Path -LiteralPath $extensionPath -PathType Leaf)) { throw "Upstream map source-reviewed extension inventory is missing for ${id}: $extensionPath" }
    }
    $raw = Get-Content -LiteralPath $rawPath -Raw | ConvertFrom-Json
    $classifications = Get-Content -LiteralPath $classificationPath -Raw | ConvertFrom-Json
    $summary = Get-Content -LiteralPath $summaryPath -Raw | ConvertFrom-Json
    if ([int]$raw.schemaVersion -ne 1 -or [string]$raw.upstreamOpenCvVersion -cne '5.0.0' -or [string]$summary.upstreamOpenCvVersion -cne '5.0.0') { throw "Upstream map version/schema drifted for $id." }
    if ([string]$raw.generator -cne [string]$map.extractorScript -or [string]$summary.generator -cne [string]$map.generatorProject) { throw "Upstream map extractor/generator identity drifted for $id." }
    $extractorPath = Resolve-RepositoryPath -RelativePath ([string]$map.extractorScript)
    if (-not (Test-Path -LiteralPath $extractorPath -PathType Leaf)) { throw "Upstream map extractor script is missing for ${id}: $extractorPath" }
    $generationScriptPath = Resolve-RepositoryPath -RelativePath ([string]$map.generationScript)
    $generatorProjectPath = Resolve-RepositoryPath -RelativePath ("$($map.generatorProject)/$([IO.Path]::GetFileName([string]$map.generatorProject)).csproj")
    if (-not (Test-Path -LiteralPath $generationScriptPath -PathType Leaf) -or -not (Test-Path -LiteralPath $generatorProjectPath -PathType Leaf)) { throw "Upstream map generator command surface is incomplete for $id." }
    if ([string]$summary.claimedSlice -cne [string]$map.scope.claimedSlice) { throw "Upstream map scope description drifted for $id." }
    if ([string]$raw.headerPath -cne [string]$map.scope.primaryHeader.path -or [string]$raw.headerSha256 -cne [string]$map.scope.primaryHeader.sha256) { throw "Upstream map primary header identity drifted for $id." }
    if ([string]$raw.parserPath -cne [string]$catalog.parser.path -or [string]$raw.parserSha256 -cne [string]$catalog.parser.sha256 -or [string]$raw.parserSha256 -cne [string]$map.scope.parser.sha256) { throw "Upstream map parser identity drifted for $id." }
    Assert-HeaderScopeSet -Raw $raw -Configured @($map.scope.sourceHeaders) -PropertyName 'sourceHeaders' -Id $id -PrimaryFallback
    Assert-HeaderScopeSet -Raw $raw -Configured @($map.scope.compatibilityHeaders) -PropertyName 'compatibilityHeaders' -Id $id
    Assert-HeaderScopeSet -Raw $raw -Configured @($map.scope.excludedPublicHeaders) -PropertyName 'excludedPublicHeaders' -Id $id
    if ([int]$raw.declarationCount -ne [int]$summary.declarationCount -or [int]$summary.declarationCount -ne [int]$map.evidenceContract.declarations) { throw "Upstream declaration count drifted for $id." }
    if ([string]$summary.rawExtractionPath -cne [string]$artifactPaths.raw -or [string]$summary.classificationPath -cne [string]$artifactPaths.classifications -or [string]$summary.mappingPath -cne [string]$artifactPaths.mapping -or [string]$summary.familyInventoryPath -cne [string]$artifactPaths.familyInventory) { throw "Upstream map artifact bindings drifted for $id." }
    if ([int]$summary.negativeFixtureCount -lt [int]$map.evidenceContract.negativeFixturesMinimum) { throw "Upstream map negative-fixture coverage regressed for $id." }
    if ([int]$summary.classificationCounts.missing -ne 0 -or -not [bool]$map.evidenceContract.zeroMissing) { throw "Upstream map contains unexplained missing declarations for $id." }
    if ([bool]$summary.repositoryWideUpstreamParityClaimed -or [bool]$map.evidenceContract.repositoryWideParityClaimed) { throw "Upstream map must not claim repository-wide parity for $id." }
    Assert-Hash -Path $mappingPath -Expected ([string]$map.evidenceContract.mappingSha256) -Description "$id mapping"
    Assert-Hash -Path $familyPath -Expected ([string]$map.evidenceContract.familyInventorySha256) -Description "$id family inventory"
    if (-not [string]::IsNullOrWhiteSpace([string]$artifactPaths.sourceReviewedExtensions)) {
        if ([string]$summary.sourceReviewedExtensionPath -cne [string]$artifactPaths.sourceReviewedExtensions -or [string]$summary.sourceReviewedExtensionSha256 -cne [string]$map.evidenceContract.sourceReviewedExtensionSha256) { throw "Upstream map source-reviewed extension binding drifted for $id." }
        Assert-Hash -Path $extensionPath -Expected ([string]$map.evidenceContract.sourceReviewedExtensionSha256) -Description "$id source-reviewed extensions"
    } elseif (-not [string]::IsNullOrWhiteSpace([string]$map.evidenceContract.sourceReviewedExtensionSha256)) { throw "Unexpected source-reviewed extension hash for $id." }

    $rawRows = @($raw.declarations)
    $classificationRows = @($classifications.declarations)
    if ($rawRows.Count -ne [int]$raw.declarationCount -or $classificationRows.Count -ne $rawRows.Count) { throw "Upstream map raw/classification row closure drifted for $id." }
    $allowed = @($catalog.classificationPolicy.allowed | ForEach-Object { [string]$_ })
    for ($index = 0; $index -lt $rawRows.Count; $index++) {
        $rawRow = $rawRows[$index]
        $classificationRow = $classificationRows[$index]
        if ([int]$rawRow.ordinal -ne $index -or [int]$classificationRow.ordinal -ne $index -or [string]$rawRow.identity -cne [string]$classificationRow.identity) { throw "Upstream map ordinal/identity alignment drifted for $id at $index." }
        if ($allowed -notcontains [string]$classificationRow.classification) { throw "Upstream map classification is outside shared policy for $id/$index." }
        if ([string]::IsNullOrWhiteSpace([string]$classificationRow.reason)) { throw "Upstream map classification rationale is missing for $id/$index." }
        if ([string]$classificationRow.classification -ceq 'implemented' -and (@($classificationRow.nativeEntrypoints).Count -eq 0 -or @($classificationRow.managedMembers).Count -eq 0)) { throw "Implemented upstream declaration lacks native/managed evidence for $id/$index." }
        if ([string]$classificationRow.classification -ceq 'missing') { throw "Upstream map has a missing declaration for $id/$index." }
    }

    Assert-SourceHeader -Header $map.scope.primaryHeader -Description "$id primary"
    Assert-SourceHeader -Header $map.scope.parser -Description "$id parser"
    foreach ($header in @($map.scope.sourceHeaders) + @($map.scope.compatibilityHeaders)) { Assert-SourceHeader -Header $header -Description "$id scoped" }
    foreach ($header in @($map.scope.excludedPublicHeaders)) { Assert-SourceHeader -Header $header -Description "$id excluded" -AllowMissingExcluded }
    $guardPath = Resolve-RepositoryPath -RelativePath ([string]$map.semanticGuard)
    if (-not (Test-Path -LiteralPath $guardPath -PathType Leaf)) { throw "Upstream map semantic guard is missing for ${id}: $guardPath" }
    Write-Host "UPSTREAM_MAP_CATALOG_ROW_OK module=$id declarations=$($rawRows.Count) negative_fixtures=$($summary.negativeFixtureCount) missing=0 generator=$($map.generatorProject)"
    & $pwsh.Source -NoProfile -File $guardPath -RepositoryRoot $repo
    if ($LASTEXITCODE -ne 0) { throw "Configured upstream map semantic guard failed for ${id}: $guardPath" }
}

Write-Host "UPSTREAM_MAP_CATALOG_OK modules=$($maps.Count) upstream=$($catalog.upstreamOpenCvVersion) classification_policy=shared semantic_guards=$($maps.Count) schema_negative_fixtures=4 repository_wide_parity_claimed=false"

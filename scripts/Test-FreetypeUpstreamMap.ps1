param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')))
Set-StrictMode -Version Latest;$ErrorActionPreference='Stop'
$repo=(Resolve-Path -LiteralPath $RepositoryRoot).Path
& (Join-Path $PSScriptRoot 'Generate-FreetypeUpstreamMap.ps1') -RepositoryRoot $repo -Check
$summary=Get-Content (Join-Path $repo 'compatibility/freetype-upstream-summary.json') -Raw|ConvertFrom-Json
$map=Get-Content (Join-Path $repo 'compatibility/freetype-upstream-classifications.json') -Raw|ConvertFrom-Json
$raw=Get-Content (Join-Path $repo 'compatibility/freetype-upstream-raw.json') -Raw|ConvertFrom-Json
$manifest=@(Get-Content (Join-Path $repo 'src/OpenCvSharp.Native/generated/native_abi_manifest.txt')|ForEach-Object {($_ -split '\|')[0]})
$managed=Get-Content (Join-Path $repo 'compatibility/managed-public-api.txt') -Raw
if([int]$summary.declarationCount -ne 7 -or [int]$summary.callableCount -ne 6 -or [int]$summary.classificationCounts.implemented -ne 0 -or [int]$summary.classificationCounts.'intentionally-omitted' -ne 6 -or [int]$summary.classificationCounts.'non-callable-metadata' -ne 1 -or [int]$summary.classificationCounts.missing -ne 0){throw 'FreeType upstream map partition drifted.'}
if(@($raw.sourceHeaders|Where-Object {$_.path -eq 'opencv-source/opencv_contrib-5.0.0/modules/freetype/include/opencv2/freetype.hpp'}).Count -ne 1){throw 'FreeType map must cover the pinned public umbrella header.'}
foreach($name in @('loadFontData','setSplitNumber','putText','getTextSize','createFreeType2')){if(@($map.declarations|Where-Object {$_.identity -match "\.$name\(" -and $_.classification -eq 'intentionally-omitted' -and $_.reason -match 'depends on external FreeType/Harfbuzz' -and $_.reason -match 'no native wrapper entrypoint'}).Count -lt 1){throw "FreeType declaration is not explicitly omitted with its dependency and wrapper boundary: $name"}}
if(@($manifest|Where-Object {$_ -match '^jyppx_ocv_freetype_'}).Count -ne 0 -or $managed -match 'JYPPX\.OpenCvSharp\.Freetype'){throw 'FreeType map falsely claims a current native or managed wrapper surface.'}
if(@($map.externalDataDependencies|Where-Object {$_ -match 'FreeType2 and HarfBuzz' -and $_ -match 'neither dependency is bundled'}).Count -ne 1){throw 'FreeType external dependency boundary drifted.'}
Write-Host "FREETYPE_UPSTREAM_MAP_CONTRACT_OK declarations=$($summary.declarationCount) callables=$($summary.callableCount) implemented=0 omitted=$($summary.classificationCounts.'intentionally-omitted') missing=0 optional_dependency=true sha256=$($summary.mappingSha256)"

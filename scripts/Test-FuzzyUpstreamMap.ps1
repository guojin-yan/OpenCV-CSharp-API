param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')))
Set-StrictMode -Version Latest;$ErrorActionPreference='Stop'
$repo=(Resolve-Path -LiteralPath $RepositoryRoot).Path
& (Join-Path $PSScriptRoot 'Generate-FuzzyUpstreamMap.ps1') -RepositoryRoot $repo -Check
$summary=Get-Content (Join-Path $repo 'compatibility/fuzzy-upstream-summary.json') -Raw|ConvertFrom-Json
$raw=Get-Content (Join-Path $repo 'compatibility/fuzzy-upstream-raw.json') -Raw|ConvertFrom-Json
$map=Get-Content (Join-Path $repo 'compatibility/fuzzy-upstream-classifications.json') -Raw|ConvertFrom-Json
$manifest=@(Get-Content (Join-Path $repo 'src/OpenCvSharp.Native/generated/native_abi_manifest.txt')|ForEach-Object {($_ -split '\|')[0]})
$managedBaseline=@(Get-Content (Join-Path $repo 'compatibility/managed-public-api.txt'))
$umbrella='opencv-source/opencv_contrib-5.0.0/modules/fuzzy/include/opencv2/fuzzy.hpp'
if([int]$summary.declarationCount -ne 18 -or [int]$summary.callableCount -ne 16 -or [int]$summary.classificationCounts.implemented -ne 16 -or [int]$summary.classificationCounts.'intentionally-omitted' -ne 0 -or [int]$summary.classificationCounts.missing -ne 0 -or [int]$summary.classificationCounts.'non-callable-metadata' -ne 2){throw 'Fuzzy upstream map partition drifted.'}
if([string]$raw.headerPath -cne $umbrella -or @($raw.sourceHeaders).Count -ne 4){throw 'Fuzzy map must retain the pinned umbrella and its four parser-emitted public headers.'}
if(@($raw.sourceHeaders|Where-Object {$_.path -match '/include/opencv2/fuzzy/[^/]+\.hpp$'}).Count -ne 4){throw 'Fuzzy parser closure lost a fuzzy public header.'}
$expectedCallables=@{
    'FT02D_components('='jyppx_ocv_fuzzy_ft02d_components|System\.Void FT02DComponents\(JYPPX\.OpenCvSharp\.Core\.Mat matrix,JYPPX\.OpenCvSharp\.Core\.Mat kernel,JYPPX\.OpenCvSharp\.Core\.Mat components,JYPPX\.OpenCvSharp\.Core\.Mat\? mask=null\)'
    'FT02D_inverseFT('='jyppx_ocv_fuzzy_ft02d_inverse_ft|System\.Void FT02DInverseFT\(JYPPX\.OpenCvSharp\.Core\.Mat components,JYPPX\.OpenCvSharp\.Core\.Mat kernel,JYPPX\.OpenCvSharp\.Core\.Mat output,System\.Int32 width,System\.Int32 height\)'
    'FT02D_process('='jyppx_ocv_fuzzy_ft02d_process|System\.Void FT02DProcess\(JYPPX\.OpenCvSharp\.Core\.Mat matrix,JYPPX\.OpenCvSharp\.Core\.Mat kernel,JYPPX\.OpenCvSharp\.Core\.Mat output,JYPPX\.OpenCvSharp\.Core\.Mat\? mask=null\)'
    'FT02D_iteration('='jyppx_ocv_fuzzy_ft02d_iteration|System\.Int32 FT02DIteration\(JYPPX\.OpenCvSharp\.Core\.Mat matrix,JYPPX\.OpenCvSharp\.Core\.Mat kernel,JYPPX\.OpenCvSharp\.Core\.Mat output,JYPPX\.OpenCvSharp\.Core\.Mat mask,JYPPX\.OpenCvSharp\.Core\.Mat\? maskOutput=null,System\.Boolean firstStop=true\)'
    'FT02D_FL_process('='jyppx_ocv_fuzzy_ft02d_fl_process|System\.Void FT02DFLProcess\(JYPPX\.OpenCvSharp\.Core\.Mat matrix,System\.Int32 radius,JYPPX\.OpenCvSharp\.Core\.Mat output\)'
    'FT02D_FL_process_float('='jyppx_ocv_fuzzy_ft02d_fl_process_float|System\.Void FT02DFLProcessFloat\(JYPPX\.OpenCvSharp\.Core\.Mat matrix,System\.Int32 radius,JYPPX\.OpenCvSharp\.Core\.Mat output\)'
    'FT12D_components('='jyppx_ocv_fuzzy_ft12d_components|System\.Void FT12DComponents\(JYPPX\.OpenCvSharp\.Core\.Mat matrix,JYPPX\.OpenCvSharp\.Core\.Mat kernel,JYPPX\.OpenCvSharp\.Core\.Mat components\)'
    'FT12D_polynomial('='jyppx_ocv_fuzzy_ft12d_polynomial|System\.Void FT12DPolynomial\(JYPPX\.OpenCvSharp\.Core\.Mat matrix,JYPPX\.OpenCvSharp\.Core\.Mat kernel,JYPPX\.OpenCvSharp\.Core\.Mat c00,JYPPX\.OpenCvSharp\.Core\.Mat c10,JYPPX\.OpenCvSharp\.Core\.Mat c01,JYPPX\.OpenCvSharp\.Core\.Mat components,JYPPX\.OpenCvSharp\.Core\.Mat\? mask=null\)'
    'FT12D_createPolynomMatrixVertical('='jyppx_ocv_fuzzy_ft12d_create_polynom_matrix_vertical|System\.Void FT12DCreatePolynomMatrixVertical\(System\.Int32 radius,JYPPX\.OpenCvSharp\.Core\.Mat matrix,System\.Int32 channels\)'
    'FT12D_createPolynomMatrixHorizontal('='jyppx_ocv_fuzzy_ft12d_create_polynom_matrix_horizontal|System\.Void FT12DCreatePolynomMatrixHorizontal\(System\.Int32 radius,JYPPX\.OpenCvSharp\.Core\.Mat matrix,System\.Int32 channels\)'
    'FT12D_process('='jyppx_ocv_fuzzy_ft12d_process|System\.Void FT12DProcess\(JYPPX\.OpenCvSharp\.Core\.Mat matrix,JYPPX\.OpenCvSharp\.Core\.Mat kernel,JYPPX\.OpenCvSharp\.Core\.Mat output,JYPPX\.OpenCvSharp\.Core\.Mat\? mask=null\)'
    'FT12D_inverseFT('='jyppx_ocv_fuzzy_ft12d_inverse_ft|System\.Void FT12DInverseFT\(JYPPX\.OpenCvSharp\.Core\.Mat components,JYPPX\.OpenCvSharp\.Core\.Mat kernel,JYPPX\.OpenCvSharp\.Core\.Mat output,System\.Int32 width,System\.Int32 height\)'
    'createKernel(Mat A;'='jyppx_ocv_fuzzy_create_kernel_from_functions|System\.Void CreateKernel\(JYPPX\.OpenCvSharp\.Core\.Mat functionX,JYPPX\.OpenCvSharp\.Core\.Mat functionY,JYPPX\.OpenCvSharp\.Core\.Mat kernel,System\.Int32 channels\)'
    'createKernel(int function;'='jyppx_ocv_fuzzy_create_kernel|System\.Void CreateKernel\(JYPPX\.OpenCvSharp\.Fuzzy\.FuzzyFunctionType functionType,System\.Int32 radius,JYPPX\.OpenCvSharp\.Core\.Mat kernel,System\.Int32 channels\)'
    'inpaint('='jyppx_ocv_fuzzy_inpaint|System\.Void Inpaint\(JYPPX\.OpenCvSharp\.Core\.Mat image,JYPPX\.OpenCvSharp\.Core\.Mat mask,JYPPX\.OpenCvSharp\.Core\.Mat output,System\.Int32 radius,JYPPX\.OpenCvSharp\.Fuzzy\.FuzzyFunctionType functionType,JYPPX\.OpenCvSharp\.Fuzzy\.FuzzyInpaintAlgorithm algorithm\)'
    'filter('='jyppx_ocv_fuzzy_filter|System\.Void Filter\(JYPPX\.OpenCvSharp\.Core\.Mat image,JYPPX\.OpenCvSharp\.Core\.Mat kernel,JYPPX\.OpenCvSharp\.Core\.Mat output\)'
}
$callables=@($map.declarations|Where-Object {$_.classification -eq 'implemented'})
if($callables.Count -ne 16 -or $expectedCallables.Count -ne 16){throw 'Fuzzy semantic callable coverage drifted.'}
foreach($row in $callables){
    $identity=[string]$row.identity
    $key=@($expectedCallables.Keys|Where-Object {$identity.StartsWith("cv.ft.$_",[StringComparison]::Ordinal)})
    if($key.Count -ne 1){throw "Unexpected Fuzzy callable identity: $identity"}
    $spec=([string]$expectedCallables[$key[0]] -split '\|',2);$symbol=$spec[0];$managedPattern=$spec[1]
    if(@($row.nativeEntrypoints).Count -ne 1 -or $row.nativeEntrypoints[0] -cne $symbol -or $manifest -notcontains $symbol){throw "Fuzzy declaration-to-native ABI mapping drifted: $identity"}
    if(@($row.managedMembers).Count -ne 1 -or $row.managedMembers[0] -notmatch ('JYPPX\.OpenCvSharp\.Fuzzy\.FuzzyCv2\|method\|public;static\|' + $managedPattern + '$') -or $managedBaseline -notcontains [string]$row.managedMembers[0]){throw "Fuzzy declaration-to-managed output overload mapping drifted: $identity"}
}
if([int]$summary.nativeEvidenceCount -ne 16 -or [int]$summary.managedEvidenceCount -ne 16){throw 'Fuzzy distinct evidence counts drifted.'}
$enumValues=@($raw.declarations|Where-Object kind -eq enum|ForEach-Object {@($_.enumValues|ForEach-Object {("$($_.name)" -replace '^const cv\.ft\.', '') + "=$($_.value)"}) -join ','})
if($enumValues.Count -ne 2 -or $enumValues[0] -cne 'LINEAR=1,SINUS=2' -or $enumValues[1] -cne 'ONE_STEP=1,MULTI_STEP=2,ITERATIVE=3'){throw 'Fuzzy enum metadata drifted.'}
Write-Host "FUZZY_UPSTREAM_MAP_CONTRACT_OK declarations=18 callables=16 implemented=16 omitted=0 missing=0 native=16 managed=16 metadata_enums=2 sha256=$($summary.mappingSha256)"

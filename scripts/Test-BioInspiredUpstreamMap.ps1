param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')))
Set-StrictMode -Version Latest;$ErrorActionPreference='Stop'
$repo=(Resolve-Path -LiteralPath $RepositoryRoot).Path
& (Join-Path $PSScriptRoot 'Generate-BioInspiredUpstreamMap.ps1') -RepositoryRoot $repo -Check
$summary=Get-Content (Join-Path $repo 'compatibility/bioinspired-upstream-summary.json') -Raw|ConvertFrom-Json
$raw=Get-Content (Join-Path $repo 'compatibility/bioinspired-upstream-raw.json') -Raw|ConvertFrom-Json
$map=Get-Content (Join-Path $repo 'compatibility/bioinspired-upstream-classifications.json') -Raw|ConvertFrom-Json
$manifest=@(Get-Content (Join-Path $repo 'src/OpenCvSharp.Native/generated/native_abi_manifest.txt')|ForEach-Object {($_ -split '\|')[0]})
$managedBaseline=@(Get-Content (Join-Path $repo 'compatibility/managed-public-api.txt'))
$expectedHeaders=@('opencv-source/opencv_contrib-5.0.0/modules/bioinspired/include/opencv2/bioinspired/bioinspired.hpp','opencv-source/opencv_contrib-5.0.0/modules/bioinspired/include/opencv2/bioinspired/retina.hpp','opencv-source/opencv_contrib-5.0.0/modules/bioinspired/include/opencv2/bioinspired/retinafasttonemapping.hpp','opencv-source/opencv_contrib-5.0.0/modules/bioinspired/include/opencv2/bioinspired/transientareassegmentationmodule.hpp')
if([int]$summary.declarationCount -ne 36 -or [int]$summary.callableCount -ne 32 -or [int]$summary.classificationCounts.implemented -ne 32 -or [int]$summary.classificationCounts.'intentionally-omitted' -ne 0 -or [int]$summary.classificationCounts.missing -ne 0){throw 'BioInspired upstream map partition drifted.'}
if(@($raw.sourceHeaders).Count -ne $expectedHeaders.Count){throw 'BioInspired parser source-header closure count drifted.'}
foreach($header in $expectedHeaders){if(@($raw.sourceHeaders|Where-Object {$_.path -ceq $header}).Count -ne 1){throw "BioInspired map source-header closure drifted: $header"}}
$callables=@($map.declarations|Where-Object {$_.classification -eq 'implemented'})
if($callables.Count -ne 32){throw 'BioInspired map must classify every scoped callable as implemented.'}
foreach($row in $callables){
    if($row.identity -notmatch '^cv\.bioinspired\.(?<owner>[A-Za-z][A-Za-z0-9_]*)\.(?<name>[A-Za-z][A-Za-z0-9_]*)\('){throw "Unexpected BioInspired callable identity: $($row.identity)"}
    $owner=[string]$Matches.owner;$name=[string]$Matches.name
    $expectedNative=@(switch("$owner.$name"){
        'Retina.getInputSize' {'jyppx_ocv_bioinspired_retina_get_input_size'}
        'Retina.getOutputSize' {'jyppx_ocv_bioinspired_retina_get_output_size'}
        'Retina.setup' {'jyppx_ocv_bioinspired_retina_setup'}
        'Retina.printSetup' {'jyppx_ocv_bioinspired_retina_print_setup_length';'jyppx_ocv_bioinspired_retina_print_setup_fill'}
        'Retina.write' {'jyppx_ocv_bioinspired_retina_write'}
        'Retina.setupOPLandIPLParvoChannel' {'jyppx_ocv_bioinspired_retina_setup_parvo'}
        'Retina.setupIPLMagnoChannel' {'jyppx_ocv_bioinspired_retina_setup_magno'}
        'Retina.run' {'jyppx_ocv_bioinspired_retina_run'}
        'Retina.applyFastToneMapping' {'jyppx_ocv_bioinspired_retina_apply_fast_tone_mapping'}
        'Retina.getParvo' {'jyppx_ocv_bioinspired_retina_get_parvo'}
        'Retina.getParvoRAW' {'jyppx_ocv_bioinspired_retina_get_parvo_raw'}
        'Retina.getMagno' {'jyppx_ocv_bioinspired_retina_get_magno'}
        'Retina.getMagnoRAW' {'jyppx_ocv_bioinspired_retina_get_magno_raw'}
        'Retina.setColorSaturation' {'jyppx_ocv_bioinspired_retina_set_color_saturation'}
        'Retina.clearBuffers' {'jyppx_ocv_bioinspired_retina_clear_buffers'}
        'Retina.activateMovingContoursProcessing' {'jyppx_ocv_bioinspired_retina_activate_moving_contours_processing'}
        'Retina.activateContoursProcessing' {'jyppx_ocv_bioinspired_retina_activate_contours_processing'}
        'Retina.create' {'jyppx_ocv_bioinspired_retina_create'}
        'RetinaFastToneMapping.applyFastToneMapping' {'jyppx_ocv_bioinspired_retina_fast_tone_mapping_apply'}
        'RetinaFastToneMapping.setup' {'jyppx_ocv_bioinspired_retina_fast_tone_mapping_setup'}
        'RetinaFastToneMapping.create' {'jyppx_ocv_bioinspired_retina_fast_tone_mapping_create'}
        'TransientAreasSegmentationModule.getSize' {'jyppx_ocv_bioinspired_transient_areas_get_size'}
        'TransientAreasSegmentationModule.setup' {'jyppx_ocv_bioinspired_transient_areas_setup'}
        'TransientAreasSegmentationModule.printSetup' {'jyppx_ocv_bioinspired_transient_areas_print_setup_length';'jyppx_ocv_bioinspired_transient_areas_print_setup_fill'}
        'TransientAreasSegmentationModule.write' {'jyppx_ocv_bioinspired_transient_areas_write'}
        'TransientAreasSegmentationModule.run' {'jyppx_ocv_bioinspired_transient_areas_run'}
        'TransientAreasSegmentationModule.getSegmentationPicture' {'jyppx_ocv_bioinspired_transient_areas_get_segmentation_picture'}
        'TransientAreasSegmentationModule.clearAllBuffers' {'jyppx_ocv_bioinspired_transient_areas_clear_all_buffers'}
        'TransientAreasSegmentationModule.create' {'jyppx_ocv_bioinspired_transient_areas_create'}
        default {throw "BioInspired callable has no reviewed native mapping: $($row.identity)"}
    })
    $actualNative=@($row.nativeEntrypoints|Sort-Object -Unique)
    if(($actualNative -join '|') -cne (($expectedNative|Sort-Object -Unique) -join '|') -or @($row.nativeEntrypoints).Count -ne $expectedNative.Count){throw "BioInspired declaration-to-native ABI mapping drifted: $($row.identity)"}
    foreach($symbol in $expectedNative){if($manifest -notcontains $symbol){throw "BioInspired ABI symbol is absent from the manifest: $symbol"}}
    $managedPattern=switch("$owner.$name"){
        'Retina.getInputSize' {'JYPPX\.OpenCvSharp\.BioInspired\.Retina\|property\|[^|]*\|.* InputSize$'}
        'Retina.getOutputSize' {'JYPPX\.OpenCvSharp\.BioInspired\.Retina\|property\|[^|]*\|.* OutputSize$'}
        'Retina.setup' {'JYPPX\.OpenCvSharp\.BioInspired\.Retina\|method\|[^|]*\|.*\bSetup\(System\.String'}
        'Retina.printSetup' {'JYPPX\.OpenCvSharp\.BioInspired\.Retina\|method\|[^|]*\|.*\bPrintSetup\('}
        'Retina.write' {'JYPPX\.OpenCvSharp\.BioInspired\.Retina\|method\|[^|]*\|.*\bWrite\('}
        'Retina.setupOPLandIPLParvoChannel' {'JYPPX\.OpenCvSharp\.BioInspired\.Retina\|method\|[^|]*\|.*\bSetup\(JYPPX\.OpenCvSharp\.BioInspired\.RetinaParvoParameters'}
        'Retina.setupIPLMagnoChannel' {'JYPPX\.OpenCvSharp\.BioInspired\.Retina\|method\|[^|]*\|.*\bSetup\(JYPPX\.OpenCvSharp\.BioInspired\.RetinaMagnoParameters'}
        'Retina.run' {'JYPPX\.OpenCvSharp\.BioInspired\.Retina\|method\|[^|]*\|.*\bRun\(JYPPX\.OpenCvSharp\.Core\.Mat input\)'}
        'Retina.applyFastToneMapping' {'JYPPX\.OpenCvSharp\.BioInspired\.Retina\|method\|[^|]*\|.*\bApplyFastToneMapping\(JYPPX\.OpenCvSharp\.Core\.Mat input,JYPPX\.OpenCvSharp\.Core\.Mat output\)'}
        'Retina.getParvo' {'JYPPX\.OpenCvSharp\.BioInspired\.Retina\|method\|[^|]*\|.*\bGetParvo\(JYPPX\.OpenCvSharp\.Core\.Mat output\)'}
        'Retina.getParvoRAW' {if($row.identity -match '\[/O\]'){'JYPPX\.OpenCvSharp\.BioInspired\.Retina\|method\|[^|]*\|.*\bGetParvoRaw\(JYPPX\.OpenCvSharp\.Core\.Mat output\)'}else{'JYPPX\.OpenCvSharp\.BioInspired\.Retina\|method\|[^|]*\|.*\bGetParvoRaw\(\)'}}
        'Retina.getMagno' {'JYPPX\.OpenCvSharp\.BioInspired\.Retina\|method\|[^|]*\|.*\bGetMagno\(JYPPX\.OpenCvSharp\.Core\.Mat output\)'}
        'Retina.getMagnoRAW' {if($row.identity -match '\[/O\]'){'JYPPX\.OpenCvSharp\.BioInspired\.Retina\|method\|[^|]*\|.*\bGetMagnoRaw\(JYPPX\.OpenCvSharp\.Core\.Mat output\)'}else{'JYPPX\.OpenCvSharp\.BioInspired\.Retina\|method\|[^|]*\|.*\bGetMagnoRaw\(\)'}}
        'Retina.setColorSaturation' {'JYPPX\.OpenCvSharp\.BioInspired\.Retina\|method\|[^|]*\|.*\bSetColorSaturation\('}
        'Retina.clearBuffers' {'JYPPX\.OpenCvSharp\.BioInspired\.Retina\|method\|[^|]*\|.*\bClearBuffers\(\)'}
        'Retina.activateMovingContoursProcessing' {'JYPPX\.OpenCvSharp\.BioInspired\.Retina\|method\|[^|]*\|.*\bActivateMovingContoursProcessing\('}
        'Retina.activateContoursProcessing' {'JYPPX\.OpenCvSharp\.BioInspired\.Retina\|method\|[^|]*\|.*\bActivateContoursProcessing\('}
        'Retina.create' {'JYPPX\.OpenCvSharp\.BioInspired\.BioInspiredCv2\|method\|[^|]*\|.*\bCreateRetina\('}
        'RetinaFastToneMapping.applyFastToneMapping' {'JYPPX\.OpenCvSharp\.BioInspired\.RetinaFastToneMapping\|method\|[^|]*\|.*\bApply\(JYPPX\.OpenCvSharp\.Core\.Mat input,JYPPX\.OpenCvSharp\.Core\.Mat output\)'}
        'RetinaFastToneMapping.setup' {'JYPPX\.OpenCvSharp\.BioInspired\.RetinaFastToneMapping\|method\|[^|]*\|.*\bSetup\('}
        'RetinaFastToneMapping.create' {'JYPPX\.OpenCvSharp\.BioInspired\.BioInspiredCv2\|method\|[^|]*\|.*\bCreateRetinaFastToneMapping\('}
        'TransientAreasSegmentationModule.getSize' {'JYPPX\.OpenCvSharp\.BioInspired\.TransientAreasSegmentationModule\|property\|[^|]*\|.* Size$'}
        'TransientAreasSegmentationModule.setup' {'JYPPX\.OpenCvSharp\.BioInspired\.TransientAreasSegmentationModule\|method\|[^|]*\|.*\bSetup\(System\.String'}
        'TransientAreasSegmentationModule.printSetup' {'JYPPX\.OpenCvSharp\.BioInspired\.TransientAreasSegmentationModule\|method\|[^|]*\|.*\bPrintSetup\('}
        'TransientAreasSegmentationModule.write' {'JYPPX\.OpenCvSharp\.BioInspired\.TransientAreasSegmentationModule\|method\|[^|]*\|.*\bWrite\('}
        'TransientAreasSegmentationModule.run' {'JYPPX\.OpenCvSharp\.BioInspired\.TransientAreasSegmentationModule\|method\|[^|]*\|.*\bRun\(JYPPX\.OpenCvSharp\.Core\.Mat input,'}
        'TransientAreasSegmentationModule.getSegmentationPicture' {'JYPPX\.OpenCvSharp\.BioInspired\.TransientAreasSegmentationModule\|method\|[^|]*\|.*\bGetSegmentationPicture\(JYPPX\.OpenCvSharp\.Core\.Mat output\)'}
        'TransientAreasSegmentationModule.clearAllBuffers' {'JYPPX\.OpenCvSharp\.BioInspired\.TransientAreasSegmentationModule\|method\|[^|]*\|.*\bClearAllBuffers\(\)'}
        'TransientAreasSegmentationModule.create' {'JYPPX\.OpenCvSharp\.BioInspired\.BioInspiredCv2\|method\|[^|]*\|.*\bCreateTransientAreasSegmentationModule\('}
        default {throw "BioInspired callable has no reviewed managed mapping: $($row.identity)"}
    }
    if(@($row.managedMembers).Count -ne 1 -or @($row.managedMembers|Where-Object {$_ -match $managedPattern}).Count -ne 1){throw "BioInspired declaration-to-managed member mapping drifted: $($row.identity)"}
    foreach($member in $row.managedMembers){if($managedBaseline -notcontains [string]$member){throw "BioInspired managed member is absent from the public API baseline: $member"}}
}
if([int]$summary.nativeEvidenceCount -ne 31 -or [int]$summary.managedEvidenceCount -ne 31){throw 'BioInspired unique evidence counts drifted.'}
Write-Host "BIOINSPIRED_UPSTREAM_MAP_CONTRACT_OK declarations=36 callables=32 implemented=32 omitted=0 missing=0 native=$($summary.nativeEvidenceCount) managed=$($summary.managedEvidenceCount) sha256=$($summary.mappingSha256)"

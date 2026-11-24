[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [switch]$RequirePackageArtifacts
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$helper = Join-Path $repo 'scripts/Get-OpenCvElfSoversion.ps1'
$bashCandidates = @(
    $(if ($IsWindows) { Join-Path $env:ProgramFiles 'Git/bin/bash.exe' }),
    $(if ($IsWindows) { Join-Path ${env:ProgramFiles(x86)} 'Git/bin/bash.exe' }),
    $(if (-not $IsWindows) { (Get-Command bash -ErrorAction Stop).Source })
) | Where-Object { $_ -and (Test-Path -LiteralPath $_ -PathType Leaf) }
if (@($bashCandidates).Count -eq 0) { throw 'Bash is required to exercise the workflow ELF audits.' }
$bash = @($bashCandidates)[0]

function Invoke-SonameShell {
    param([string]$Script, [switch]$ExpectFailure)
    $info = [Diagnostics.ProcessStartInfo]::new()
    $info.FileName = $bash
    $info.WorkingDirectory = $repo
    $info.UseShellExecute = $false
    $info.RedirectStandardInput = $true
    $info.RedirectStandardOutput = $true
    $info.RedirectStandardError = $true
    foreach ($arg in @('--noprofile', '--norc', '-s')) { $info.ArgumentList.Add($arg) }
    $process = [Diagnostics.Process]::Start($info)
    try {
        $stdout = $process.StandardOutput.ReadToEndAsync()
        $stderr = $process.StandardError.ReadToEndAsync()
        $process.StandardInput.Write(($Script -replace "`r`n", "`n") + "`n")
        $process.StandardInput.Close()
        $process.WaitForExit()
        $output = $stdout.GetAwaiter().GetResult().Trim()
        $diagnostic = $stderr.GetAwaiter().GetResult().Trim()
        if ($ExpectFailure) {
            if ($process.ExitCode -eq 0) { throw 'Invalid ELF version/dependency was accepted.' }
        }
        elseif ($process.ExitCode -ne 0) { throw "ELF audit fragment failed: $diagnostic" }
        return $output
    }
    finally { $process.Dispose() }
}

$cases = @(
    @('4.9.0', '409'), @('4.10.0', '410'), @('5.0.0', '500'),
    @('5.1.0', '501'), @('5.1.2', '501'), @('5.99.3', '599'), @('5.101.2', '501')
)
foreach ($case in $cases) {
    if ((& $helper -OpenCvVersion $case[0]) -cne $case[1]) { throw "PowerShell SONAME rule failed: $($case[0])" }
    $actual = Invoke-SonameShell "sh ./scripts/Get-OpenCvElfSoversion.sh '$($case[0])'"
    if ($actual -cne $case[1]) { throw "POSIX SONAME rule failed: $($case[0])" }
}
$invalid = @('', '5.1', '5.1.0.0', '05.1.0', '5.01.0', '5.1.00', '-5.1.0',
    '5.1.0-preview', '5.1.0+build', "5.1.0`n", "5.1.0`njunk", '5.1.0;echo BAD')
foreach ($version in $invalid) {
    $rejected = $false
    try { $null = & $helper -OpenCvVersion $version } catch { $rejected = $true }
    if (-not $rejected) { throw 'PowerShell accepted an invalid OpenCV version.' }
    $null = Invoke-SonameShell "sh ./scripts/Get-OpenCvElfSoversion.sh '$version'" -ExpectFailure
}

$pack = [IO.File]::ReadAllText((Join-Path $repo '.github/workflows/pack.yml'))
$producer = [IO.File]::ReadAllText((Join-Path $repo '.github/workflows/runtime-input.yml'))
$lifecycle = [IO.File]::ReadAllText((Join-Path $repo 'scripts/Invoke-LifecycleRefreshContainerProducer.sh'))
foreach ($text in @($pack, $producer, $lifecycle)) {
    if ($text -match 'libopencv_[^\r\n]*\.so\.[0-9]+') { throw 'A live workflow still pins a numeric ELF ABI suffix.' }
}

# Execute the actual nine provenance name blocks, with real inputs substituted.
$psBlocks = [regex]::Matches($pack, '(?ms)^ *(?<body>\$openCvElfSoversion = [^\r\n]+\r?\n *\$expectedRuntimeNames = @\(\$expectedModules \| ForEach-Object \{.*?^ *\}\))')
if ($psBlocks.Count -ne 9) { throw 'Expected nine Linux provenance name blocks.' }
foreach ($block in $psBlocks) {
    foreach ($case in $cases[1..4]) {
        $body = $block.Groups['body'].Value.Replace('${{ inputs.opencv_version }}', $case[0])
        $expectedModules = @('core', 'imgproc')
        $expectedRuntimeNames = @()
        Push-Location $repo
        try { . ([scriptblock]::Create($body)) } finally { Pop-Location }
        $expected = @($expectedModules | ForEach-Object { "libopencv_$_.so"; "libopencv_$_.so.$($case[1])"; "libopencv_$_.so.$($case[0])" })
        if (($expectedRuntimeNames -join '|') -cne ($expected -join '|')) { throw 'Workflow provenance SONAME names drifted.' }
    }
}

# Exercise each shell initializer and its name-generation loop independently.
$shellBlockCount = 0
foreach ($text in @($producer, $pack)) {
    $initializers = [regex]::Matches($text, '(?m)^ *opencv_elf_soversion=[^\r\n]+')
    foreach ($init in $initializers) {
        $body = $text.Substring($init.Index + $init.Length)
        $loop = [regex]::Match($body, '(?ms)^ *printf[^\r\n]*libJYPPX\.OpenCV\.Native\.so[^\r\n]*\r?\n *for module in [^\r\n]*; do\r?\n.*?^ *done\r?$')
        if (-not $loop.Success) { throw 'Workflow ELF name loop is missing.' }
        $initBody = $init.Value.Replace('/workspace/scripts/', './scripts/')
        foreach ($case in $cases[1..4]) {
            $script = @'
set -euo pipefail
OPENCV_VERSION='__VERSION__'
modules=(core imgproc)
required_modules=(core imgproc)
expected_names="$(mktemp)"
trap 'rm -f "$expected_names"' EXIT
'@
            if ($loop.Value.Contains('for module in $modules;')) { $script = $script.Replace('modules=(core imgproc)', 'modules="core imgproc"') }
            $script = $script.Replace('__VERSION__', $case[0]) + "`n" + $initBody + "`n" + $loop.Value + "`n" + 'LC_ALL=C sort "$expected_names"'
            $script = $script.Replace('${{ inputs.opencv_version }}', $case[0])
            $actual = Invoke-SonameShell $script
            $expected = [string[]](@('libJYPPX.OpenCV.Native.so') + @('core', 'imgproc' | ForEach-Object { "libopencv_$_.so"; "libopencv_$_.so.$($case[1])"; "libopencv_$_.so.$($case[0])" }))
            [Array]::Sort($expected, [StringComparer]::Ordinal)
            if (($actual -replace "`r", '') -cne ($expected -join "`n")) { throw 'Workflow shell SONAME names drifted.' }
        }
        $shellBlockCount++
    }
}
if ($shellBlockCount -ne 13) { throw 'Expected thirteen shell ELF filename blocks.' }

# Use readelf-format lines to catch patch-suffix, Windows-suffix and prefix matches.
$dependencyCount = 0
foreach ($text in @($producer, $pack, $lifecycle)) {
    foreach ($line in [regex]::Matches($text, '(?m)^ *(?<variable>[a-z_]*direct_opencv_dependencies)=[^\r\n]+')) {
        $fixture = @'
set -euo pipefail
native_dir=/unused
audit_dir=/unused
opencv_elf_soversion=501
readelf() {
  printf '%s\n' ' (NEEDED) Shared library: [libopencv_core.so.501]' ' (NEEDED) Shared library: [libopencv_imgproc.so.501]' ' (NEEDED) Shared library: [libopencv_videoio.so.5010]' ' (NEEDED) Shared library: [libopencv_core.so.500]' ' (NEEDED) Shared library: [libopencv_core.so.510]' ' (NEEDED) Shared library: [libopencv_core.so.5.1.0]' ' (NEEDED) Shared library: [libopencv_corexsox501]'
}
'@
        $script = $fixture + "`n" + $line.Value + "`n" + ('printf ''%s\n'' "${' + $line.Groups['variable'].Value + '}"')
        if ((Invoke-SonameShell $script) -cne '2') { throw 'Workflow NEEDED count matches an incorrect ABI name.' }
        $null = Invoke-SonameShell ($script.Replace('opencv_elf_soversion=501', 'opencv_elf_soversion=599')) -ExpectFailure
        $dependencyCount++
    }
}
if ($dependencyCount -ne 14) { throw 'Expected fourteen direct-OpenCV dependency audits.' }

function Get-SonameTextHash {
    param([string]$Text)
    [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($Text))).ToLowerInvariant()
}
function Normalize-SonameText {
    param([string]$Text)
    ($Text.Replace("`r`n", "`n").Replace("`r", "`n").TrimEnd()) + "`n"
}
$evidence = Get-Content (Join-Path $repo 'packaging/runtime/opencv-5.1.0-local-linux-preview-evidence.json') -Raw | ConvertFrom-Json
$recordPath = Join-Path $repo 'packaging/runtime/opencv-5.1.0-elf-workflow-evidence.json'
$recordJson = [IO.File]::ReadAllText($recordPath)
if (-not (Test-Json -Json $recordJson -SchemaFile (Join-Path $repo 'packaging/runtime/opencv-5.1.0-elf-workflow-evidence.schema.json'))) { throw 'ELF workflow evidence schema failed.' }
$record = $recordJson | ConvertFrom-Json
$inputPaths = @('.github/workflows/runtime-input.yml', '.github/workflows/pack.yml', 'scripts/Invoke-LifecycleRefreshContainerProducer.sh', 'scripts/Get-OpenCvElfSoversion.ps1', 'scripts/Get-OpenCvElfSoversion.sh')
if (($record.sourceInputs.path -join '|') -cne ($inputPaths -join '|')) { throw 'ELF workflow source input scope drifted.' }
foreach ($inputFile in $record.sourceInputs) {
    $hash = Get-SonameTextHash (Normalize-SonameText ([IO.File]::ReadAllText((Join-Path $repo $inputFile.path))))
    if ($hash -cne $inputFile.sha256) { throw "ELF workflow evidence requires remeasurement: $($inputFile.path)" }
}
& git -C $repo merge-base --is-ancestor $record.sourceBaseCommit HEAD 2>$null
if ($LASTEXITCODE -ne 0) { throw 'ELF workflow measurement source is not an ancestor of HEAD.' }
if ($record.producerContainerId -cne $evidence.producerIdentity.id -or $record.producerImage -cne $evidence.producerIdentity.image) { throw 'ELF audit producer identity differs from the measured runtime.' }
$fragmentMatch = [regex]::Match($producer, '(?ms)^      - name: Audit generic Linux preview ELF dependency policy\r?\n.*?^        run: \|\r?\n(?<body>.*?)(?=^      - name: Create runtime input artifact layout)')
if (-not $fragmentMatch.Success) { throw 'Measured ELF workflow step is missing.' }
$fragment = Normalize-SonameText ($fragmentMatch.Groups['body'].Value -replace '(?m)^ {10}', '')
if ((Get-SonameTextHash $fragment) -cne $record.workflowFragmentSha256) { throw 'Measured workflow fragment differs from current code.' }
$matrix = Get-Content (Join-Path $repo 'packaging/runtime/runtime-generic-linux-preview-matrix.json') -Raw | ConvertFrom-Json
if (($record.profiles.name -join '|') -cne 'full|mini') { throw 'ELF workflow evidence profile scope drifted.' }
foreach ($profile in $record.profiles) {
    $package = @($evidence.profiles | Where-Object name -CEQ $profile.name)[0]
    $definition = @($matrix.profiles | Where-Object name -CEQ $profile.name)[0]
    if ($profile.packageSha256 -cne $package.packageSha256 -or $profile.packageBytes -ne $package.packageBytes) { throw 'ELF workflow package binding drifted.' }
    foreach ($field in @('script', 'output', 'oldSuffixControlOutput')) {
        if ((Get-SonameTextHash $profile.$field) -cne $profile."$($field)Sha256") { throw 'ELF workflow raw script/output bytes changed.' }
    }
    $resolvedFragment = $fragment.Replace('${{ matrix.profile }}', $profile.name).Replace('${{ inputs.opencv_version }}', '5.1.0').Replace('${{ steps.profile.outputs.build_list }}', $definition.buildList).Replace('${{ steps.profile.outputs.opencv_rid }}', 'ubuntu.22.04-x64')
    if (-not $profile.script.EndsWith($resolvedFragment, [StringComparison]::Ordinal) -or $profile.runtimeDirectory -cne "/workspace/linux51-output/$($profile.name)-runtime") { throw 'Local execution changed the measured workflow audit body.' }
    $files = if ($profile.name -ceq 'full') { 18 } else { 7 }
    $entries = if ($profile.name -ceq 'full') { 52 } else { 19 }
    $modules = $files - 1
    $marker = "GENERIC_LINUX_PREVIEW_ELF_EVIDENCE profile=$($profile.name) files=$files runtime_files=$entries machine=X86-64 origin=$files producer_paths=0 direct_opencv=$modules missing_dependencies=0 external_distro_dependencies=0 policy=bundled-codecs-bundled-protobuf-glibc-cxx-system-only-v2"
    if (-not $profile.output.Contains($marker) -or -not $profile.oldSuffixControlOutput.Contains('-libopencv_core.so.500') -or -not $profile.oldSuffixControlOutput.Contains('+libopencv_core.so.501')) { throw 'ELF workflow positive/negative execution evidence is incomplete.' }
}

if ($RequirePackageArtifacts) {
    foreach ($profile in $evidence.profiles) {
        $packagePath = Join-Path $repo $profile.artifactPath
        if ((Get-FileHash -LiteralPath $packagePath -Algorithm SHA256).Hash.ToLowerInvariant() -cne $profile.packageSha256) { throw 'Local Linux package bytes changed.' }
        $modules = @($matrix.profiles | Where-Object name -CEQ $profile.name)[0].modules
        $expectedNames = @('libJYPPX.OpenCV.Native.so') + @($modules | ForEach-Object { "libopencv_$_.so"; "libopencv_$_.so.501"; "libopencv_$_.so.5.1.0" })
        $zip = [IO.Compression.ZipFile]::OpenRead($packagePath)
        try {
            $entries = @($zip.Entries | Where-Object FullName -CLike 'runtimes/linux-x64/native/*')
            $names = @($entries | ForEach-Object { $_.Name })
            if ($names.Count -ne $expectedNames.Count -or @(Compare-Object $expectedNames $names -CaseSensitive).Count -ne 0) { throw 'Actual 5.1 package differs from workflow names.' }
            foreach ($module in $modules) {
                $hashes = foreach ($suffix in @('', '.501', '.5.1.0')) {
                    $entry = $zip.GetEntry("runtimes/linux-x64/native/libopencv_$module.so$suffix")
                    $stream = $entry.Open()
                    try { [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)) }
                    finally { $stream.Dispose() }
                }
                if (@($hashes | Select-Object -Unique).Count -ne 1) { throw 'Actual package SONAME triplet bytes differ.' }
            }
            Write-Host "Actual $($profile.name) package passed: $($names.Count) native entries, all SONAME triplets equal, SHA256=$($profile.packageSha256)."
        }
        finally { $zip.Dispose() }
    }
}

Write-Host "ELF SONAME workflow guard passed: $($cases.Count) versions, $($invalid.Count) invalid versions, 9 PowerShell name blocks, $shellBlockCount shell name blocks, $dependencyCount dependency audits with negative controls."

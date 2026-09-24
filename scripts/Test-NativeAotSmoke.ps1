param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [string]$RuntimeIdentifier = 'win-x64',
    [string]$OpenCvNativeRuntimeDir = '',
    [int]$TimeoutSeconds = 180
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if (-not $IsWindows) {
    Write-Host 'NATIVE_AOT_SMOKE_SKIPPED platform=non-windows reason=windows-linker-contract'
    exit 0
}
if ($RuntimeIdentifier -notin @('win-x64', 'win-arm64')) { throw "Unsupported AOT smoke RID: $RuntimeIdentifier" }
if ($TimeoutSeconds -lt 30 -or $TimeoutSeconds -gt 900) { throw 'TimeoutSeconds must be between 30 and 900.' }

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$project = Join-Path $repo 'tools/AotSmoke/AotSmoke.csproj'
if (-not (Test-Path -LiteralPath $project -PathType Leaf)) { throw "AOT smoke project was not found: $project" }
$dotnet = Get-Command dotnet -ErrorAction Stop
$publishRoot = Join-Path ([IO.Path]::GetTempPath()) ('opencv-aot-smoke-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Force -Path $publishRoot | Out-Null

function Quote-CmdArgument {
    param([Parameter(Mandatory)][string]$Value)
    return '"' + ($Value -replace '"', '\"') + '"'
}

function Find-VsDevCmd {
    $candidates = @(
        'C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\Tools\VsDevCmd.bat',
        'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\Tools\VsDevCmd.bat')
    foreach ($candidate in $candidates) {
        if (Test-Path -LiteralPath $candidate -PathType Leaf) { return $candidate }
    }
    return ''
}

try {
    & $dotnet.Source build (Join-Path $repo 'src/OpenCvSharp/OpenCvSharp.csproj') -c Release -f net10.0 --no-restore
    if ($LASTEXITCODE -ne 0) { throw "Managed net10.0 build for AOT smoke failed with exit code $LASTEXITCODE." }
    $publishArgs = @(
        'publish', $project, '-c', 'Release', '-f', 'net10.0', '-r', $RuntimeIdentifier,
        '--self-contained', 'true', '-p:PublishAot=true', '-p:PublishTrimmed=true',
        '-p:TrimMode=partial', '-p:PublishSingleFile=true', '-p:InvariantGlobalization=true',
        '-p:IlcUseEnvironmentalTools=true',
        '-p:StripSymbols=true', '-o', $publishRoot)
    if (-not [string]::IsNullOrWhiteSpace($OpenCvNativeRuntimeDir)) {
        $publishArgs += @('-p:OpenCvNativeRuntimeDir=' + $OpenCvNativeRuntimeDir)
    }

    $vsDevCmd = Find-VsDevCmd
    if ([string]::IsNullOrWhiteSpace($vsDevCmd)) {
        throw 'NativeAOT Windows smoke requires a Visual Studio VsDevCmd.bat toolchain environment.'
    }
    $hostArchitecture = 'x64'
    $targetArchitecture = if ($RuntimeIdentifier -eq 'win-arm64') { 'arm64' } else { 'x64' }
    $publishCommand = 'dotnet ' + (($publishArgs | ForEach-Object { Quote-CmdArgument -Value ([string]$_) }) -join ' ')
    $developerCommand = (Quote-CmdArgument -Value $vsDevCmd) + " -arch=$targetArchitecture -host_arch=$hostArchitecture && " + $publishCommand
    $build = @(& cmd.exe /d /s /c $developerCommand 2>&1)
    $build | ForEach-Object { Write-Host ([string]$_) }
    if ($LASTEXITCODE -ne 0) { throw "NativeAOT publish failed with exit code $LASTEXITCODE." }

    $executable = Join-Path $publishRoot 'AotSmoke.exe'
    if (-not (Test-Path -LiteralPath $executable -PathType Leaf)) { throw "AOT executable was not produced: $executable" }
    $files = @(Get-ChildItem -LiteralPath $publishRoot -File)
    $nativePayload = @($files | Where-Object { $_.Name -match '^JYPPX\.OpenCV\.Native\.dll$|^opencv_.*\.dll$' })
    if ([string]::IsNullOrWhiteSpace($OpenCvNativeRuntimeDir) -and $nativePayload.Count -ne 0) {
        throw 'AOT smoke output unexpectedly contained native runtime payload without explicit runtime input.'
    }

    $process = Start-Process -FilePath $executable -WorkingDirectory $publishRoot -Wait -PassThru -NoNewWindow
    if ($process.ExitCode -ne 0) { throw "AOT smoke executable failed with exit code $($process.ExitCode)." }
    Write-Host "NATIVE_AOT_SMOKE_OK rid=$RuntimeIdentifier files=$($files.Count) native_payload=$($nativePayload.Count) native_runtime_supplied=$(-not [string]::IsNullOrWhiteSpace($OpenCvNativeRuntimeDir))"
}
finally {
    if (Test-Path -LiteralPath $publishRoot) { Remove-Item -LiteralPath $publishRoot -Recurse -Force }
}

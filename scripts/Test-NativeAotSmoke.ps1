param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [string]$RuntimeIdentifier = 'win-x64',
    [string]$OpenCvNativeRuntimeDir = '',
    [int]$TimeoutSeconds = 180,
    [string]$WarningLedgerPath = ''
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

function Get-ProcessArchitecture {
    $architecture = [string]$env:PROCESSOR_ARCHITECTURE
    if ($architecture -match '^(AMD64|x86_64)$') { return 'x64' }
    if ($architecture -match '^(ARM64|AARCH64)$') { return 'arm64' }
    if ($architecture -match '^(x86|X86)$') { return 'x86' }
    return $architecture.ToLowerInvariant()
}

function Get-SourceCommit {
    $commit = ((& git -C $repo rev-parse HEAD 2>$null) | Select-Object -First 1).Trim()
    if ($LASTEXITCODE -ne 0 -or $commit -notmatch '^[0-9a-f]{40}$') {
        throw "Could not resolve a repository source commit for the AOT warning ledger: $commit"
    }
    return $commit
}

function Resolve-OptionalOutputPath {
    param([Parameter(Mandatory)][AllowEmptyString()][string]$Path)
    if ([string]::IsNullOrWhiteSpace($Path)) { return '' }
    if ([IO.Path]::IsPathRooted($Path)) { return [IO.Path]::GetFullPath($Path) }
    return [IO.Path]::GetFullPath((Join-Path $repo ($Path -replace '/', [IO.Path]::DirectorySeparatorChar)))
}

try {
    $warningRecords = [System.Collections.Generic.List[object]]::new()
    $warningKeys = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    function Write-CommandOutput {
        param(
            [Parameter(Mandatory)][string]$Phase,
            [Parameter(Mandatory)][AllowEmptyCollection()][object[]]$Lines
        )
        foreach ($line in $Lines) {
            $text = [string]$line
            Write-Host $text
            if ($text -match '(?i)(?:\bwarning|警告)\s+(?<code>[A-Z][A-Z0-9]{2,8})\s*:\s*(?<message>.*)$') {
                $key = "$Phase|$($Matches.code)|$($Matches.message.Trim())"
                if ($warningKeys.Add($key)) {
                    [void]$warningRecords.Add([ordered]@{
                            phase = $Phase
                            code = $Matches.code.ToUpperInvariant()
                            message = $Matches.message.Trim()
                        })
                }
            }
        }
    }

    $managedBuild = @(& $dotnet.Source build (Join-Path $repo 'src/OpenCvSharp/OpenCvSharp.csproj') -c Release -f net10.0 --no-restore 2>&1)
    $managedBuildExitCode = $LASTEXITCODE
    Write-CommandOutput -Phase 'managed-build' -Lines $managedBuild
    if ($managedBuildExitCode -ne 0) { throw "Managed net10.0 build for AOT smoke failed with exit code $managedBuildExitCode." }
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
    $toolProbeCommand = (Quote-CmdArgument -Value $vsDevCmd) + " -arch=$targetArchitecture -host_arch=$hostArchitecture && where link.exe"
    $toolProbe = @(& cmd.exe /d /s /c $toolProbeCommand 2>&1)
    if ($LASTEXITCODE -ne 0) {
        if ($RuntimeIdentifier -eq 'win-arm64') {
            throw 'NativeAOT ARM64 smoke requires the Visual Studio C++ ARM64 build tools; VsDevCmd could not initialize the ARM64 linker environment.'
        }
        throw 'NativeAOT x64 smoke could not initialize the Visual Studio linker environment.'
    }
    $toolProbe | ForEach-Object { Write-Host ([string]$_) }
    $publishCommand = 'dotnet ' + (($publishArgs | ForEach-Object { Quote-CmdArgument -Value ([string]$_) }) -join ' ')
    $developerCommand = (Quote-CmdArgument -Value $vsDevCmd) + " -arch=$targetArchitecture -host_arch=$hostArchitecture && " + $publishCommand
    $build = @(& cmd.exe /d /s /c $developerCommand 2>&1)
    $publishExitCode = $LASTEXITCODE
    Write-CommandOutput -Phase 'native-publish' -Lines $build
    if ($publishExitCode -ne 0) { throw "NativeAOT publish failed with exit code $publishExitCode." }

    $executable = Join-Path $publishRoot 'AotSmoke.exe'
    if (-not (Test-Path -LiteralPath $executable -PathType Leaf)) { throw "AOT executable was not produced: $executable" }
    $files = @(Get-ChildItem -LiteralPath $publishRoot -File)
    $nativePayload = @($files | Where-Object { $_.Name -match '^JYPPX\.OpenCV\.Native\.dll$|^opencv_.*\.dll$' })
    if ([string]::IsNullOrWhiteSpace($OpenCvNativeRuntimeDir) -and $nativePayload.Count -ne 0) {
        throw 'AOT smoke output unexpectedly contained native runtime payload without explicit runtime input.'
    }

    $process = Start-Process -FilePath $executable -WorkingDirectory $publishRoot -Wait -PassThru -NoNewWindow
    if ($process.ExitCode -ne 0) { throw "AOT smoke executable failed with exit code $($process.ExitCode)." }

    $ledgerPath = Resolve-OptionalOutputPath -Path $WarningLedgerPath
    if (-not [string]::IsNullOrWhiteSpace($ledgerPath)) {
        $managedAssembly = Join-Path $repo 'src/OpenCvSharp/bin/Release/net10.0/JYPPX.OpenCV.CSharp.API.dll'
        if (-not (Test-Path -LiteralPath $managedAssembly -PathType Leaf)) { throw "Managed assembly was not found for the AOT warning ledger: $managedAssembly" }
        $ledger = [ordered]@{
            '$schema' = 'native-aot-trim-warning-ledger.schema.json'
            schemaVersion = 1
            status = if ($warningRecords.Count -eq 0) { 'verified' } else { 'warnings-observed' }
            sourceCommit = Get-SourceCommit
            runner = [ordered]@{
                os = [Environment]::OSVersion.VersionString
                architecture = Get-ProcessArchitecture
                dotnet = ((& $dotnet.Source --version 2>$null) | Select-Object -First 1).Trim()
                configuration = 'Release'
            }
            publish = [ordered]@{
                targetFramework = 'net10.0'
                runtimeIdentifier = $RuntimeIdentifier
                publishAot = $true
                publishTrimmed = $true
                trimMode = 'partial'
                publishSingleFile = $true
                managedAssemblySha256 = (Get-FileHash -LiteralPath $managedAssembly -Algorithm SHA256).Hash.ToLowerInvariant()
                publishedFileCount = $files.Count
                executableName = $executable | Split-Path -Leaf
                nativePayloadFileCount = $nativePayload.Count
                nativeRuntimeSupplied = (-not [string]::IsNullOrWhiteSpace($OpenCvNativeRuntimeDir))
                nativeSmoke = if ([string]::IsNullOrWhiteSpace($OpenCvNativeRuntimeDir)) { 'skipped-native-runtime-missing' } else { 'verified' }
            }
            warnings = @($warningRecords)
            warningCount = $warningRecords.Count
            limitations = @(
                'The ledger records compiler and NativeAOT publish diagnostics for this exact consumer and runner.',
                'The managed assembly hash identifies the assembly used by this measured publish; ordinary local rebuilds can change compiler output bytes.',
                'The current record has no trim or AOT warnings; it does not promote package-wide IsAotCompatible or IsTrimmable metadata.',
                'The managed-only run does not replace native runtime payload execution or ARM64 hardware evidence.'
            )
        }
        $ledgerDirectory = Split-Path -Parent $ledgerPath
        New-Item -ItemType Directory -Force -Path $ledgerDirectory | Out-Null
        [IO.File]::WriteAllText($ledgerPath, (($ledger | ConvertTo-Json -Depth 8) + [Environment]::NewLine), [Text.UTF8Encoding]::new($false))
        Write-Host "NATIVE_AOT_TRIM_WARNING_LEDGER_WRITTEN status=$($ledger.status) warnings=$($ledger.warningCount) output=$ledgerPath"
    }
    Write-Host "NATIVE_AOT_SMOKE_OK rid=$RuntimeIdentifier files=$($files.Count) native_payload=$($nativePayload.Count) native_runtime_supplied=$(-not [string]::IsNullOrWhiteSpace($OpenCvNativeRuntimeDir))"
}
finally {
    if (Test-Path -LiteralPath $publishRoot) { Remove-Item -LiteralPath $publishRoot -Recurse -Force }
}

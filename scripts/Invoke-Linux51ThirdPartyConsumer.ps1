[CmdletBinding()]
param([string]$ConsumerRoot = '/consumer')
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if (-not $IsLinux -or (& uname -m) -cne 'x86_64') { throw 'Expected native Linux x64.' }
foreach ($tool in @('gcc', 'g++', 'clang', 'clang++', 'cc', 'c++', 'cmake', 'ninja')) {
    if (Get-Command $tool -ErrorAction SilentlyContinue) { throw "Compiler present: $tool" }
}
foreach ($path in @('/workspace', '/repo-input', '/upstream-input', '/tmp/.X11-unix')) {
    if (Test-Path $path) { throw "Producer or display path visible: $path" }
}
if (@(Get-ChildItem Env: | Where-Object Name -Match '^OPENCV_|^OpenCvSharp|^LD_LIBRARY_PATH$|^LD_PRELOAD$|^LD_AUDIT$|^DISPLAY$|^WAYLAND_DISPLAY$').Count) { throw 'Loader or display override present.' }
function Get-TPHash([string]$Path) { (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant() }
function Write-TPText([string]$Path, [string]$Text) { [IO.File]::WriteAllText($Path, $Text, [Text.UTF8Encoding]::new($false)) }
function Invoke-TPTool([string]$Executable, [string[]]$Arguments, [string]$Directory, [string]$Prefix, [int]$Timeout = 300) {
    $start = [Diagnostics.ProcessStartInfo]::new($Executable)
    $start.WorkingDirectory = $Directory
    $start.UseShellExecute = $false; $start.RedirectStandardOutput = $true; $start.RedirectStandardError = $true
    foreach ($argument in $Arguments) { $start.ArgumentList.Add($argument) }
    $process = [Diagnostics.Process]::Start($start)
    $stdout = $process.StandardOutput.ReadToEndAsync(); $stderr = $process.StandardError.ReadToEndAsync()
    if (-not $process.WaitForExit($Timeout * 1000)) { $process.Kill($true); throw 'Consumer process timed out.' }
    Write-TPText "$Prefix.stdout.log" ($stdout.GetAwaiter().GetResult())
    Write-TPText "$Prefix.stderr.log" ($stderr.GetAwaiter().GetResult())
    $exitCode = $process.ExitCode; $process.Dispose()
    if ($exitCode -ne 0) { throw "Consumer process failed: $Executable/$exitCode, see $Prefix" }
}
$os = @{}
foreach ($line in Get-Content /etc/os-release) { if ($line -match '^([A-Z_]+)=(.*)$') { $os[$Matches[1]] = $Matches[2].Trim('"') } }
$rid = "$($os.ID).$($os.VERSION_ID)-x64"
if ($rid -cnotin @('ubuntu.22.04-x64', 'ubuntu.24.04-x64', 'debian.12-x64')) { throw 'Unexpected consumer distro.' }
$packages = @(Get-Content (Join-Path $ConsumerRoot 'packages.json') -Raw | ConvertFrom-Json)
$feed = Join-Path $ConsumerRoot 'feed'; $source = Join-Path $ConsumerRoot 'source'; $artifacts = Join-Path $ConsumerRoot 'artifacts'
if ($packages.Count -ne 5 -or @(Get-ChildItem $feed -File).Count -ne 5) { throw 'Expected one managed and four supplemented runtime packages.' }
foreach ($package in $packages) {
    $path = Join-Path $feed $package.fileName
    if ((Get-Item $path).Length -ne $package.bytes -or (Get-TPHash $path) -cne $package.sha256) { throw 'Feed archive bytes drifted.' }
}
$environment = [ordered]@{ consumerRid = $rid; architecture = (& uname -m); libc = (& getconf GNU_LIBC_VERSION)
    compilerAbsent = $true; producerPathsAbsent = $true; loaderOverridesAbsent = $true
    packages = @($packages | Select-Object id, version, fileName, sha256)
    powerShellVersion = $PSVersionTable.PSVersion.ToString() }
Write-TPText (Join-Path $artifacts 'environment.json') (($environment | ConvertTo-Json -Depth 8) + "`n")
$runs = @()
foreach ($tfm in @('net8.0', 'net10.0')) {
    $toolchain = if ($tfm -ceq 'net8.0') { '/opt/dotnet8' } else { '/opt/dotnet10' }
    $runtime = if ($tfm -ceq 'net8.0') { '8.0.34' } else { '10.0.12' }
    $sdk = if ($tfm -ceq 'net8.0') { '8.0.428' } else { '10.0.401' }
    $env:DOTNET_ROOT = $toolchain; $env:DOTNET_MULTILEVEL_LOOKUP = '0'; $env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
    $dotnet = "$toolchain/dotnet"
    if ((& $dotnet --version) -cne $sdk) { throw 'Unexpected SDK.' }
    foreach ($flavor in @('preview', 'headless')) {
        foreach ($profile in @('full', 'mini')) {
            $selected = @($packages | Where-Object { $_.flavor -ceq $flavor -and $_.profile -ceq $profile })
            if ($selected.Count -ne 1 -or $selected[0].version -notmatch '^5\.1\.0-(?:headless-)?licenses\.[a-f0-9]{7}$') { throw 'Unexpected candidate identity.' }
            $candidate = $selected[0]
            $name = "$tfm-$flavor-$profile"
            $run = Join-Path $ConsumerRoot "runs/$name"; $resultDir = Join-Path $artifacts $name
            if (Test-Path $run) { throw 'Fresh project and package cache required.' }
            [IO.Directory]::CreateDirectory($run) | Out-Null
            [IO.Directory]::CreateDirectory($resultDir) | Out-Null
            $env:NUGET_PACKAGES = Join-Path $run 'nuget'
            $project = (Get-Content (Join-Path $source 'consumer.csproj.in') -Raw).Replace('__TFM__', $tfm).Replace('__RUNTIME_VERSION__', $runtime).Replace('__RUNTIME_PACKAGE__', [string]$candidate.id).Replace('__CANDIDATE_VERSION__', [string]$candidate.version)
            Write-TPText (Join-Path $run 'Consumer.csproj') $project
            Write-TPText (Join-Path $run 'NuGet.Config') "<configuration><packageSources><clear/><add key=`"local`" value=`"$feed`" /></packageSources></configuration>`n"
            Copy-Item (Join-Path $source "$flavor/Program.cs") $run
            Copy-Item (Join-Path $source 'identity-opset13.onnx.base64') $run
            Invoke-TPTool $dotnet @('restore', 'Consumer.csproj', '--configfile', 'NuGet.Config', '--no-cache') $run (Join-Path $resultDir 'restore')
            Invoke-TPTool $dotnet @('build', 'Consumer.csproj', '--no-restore', '-c', 'Release') $run (Join-Path $resultDir 'build')
            $assembly = Join-Path $run "bin/Release/$tfm/linux-x64/Consumer.dll"
            Invoke-TPTool $dotnet @($assembly, $profile, $resultDir) $run (Join-Path $resultDir 'execute') 60
            $result = Get-Content (Join-Path $resultDir 'result.json') -Raw | ConvertFrom-Json
            if ($result.status -cne 'passed' -or $result.targetFramework -cne $tfm -or $result.runtimeVersion -cne $runtime) { throw 'Native execution did not use the selected framework.' }
            $cache = @(Get-ChildItem $env:NUGET_PACKAGES -Recurse -File -Filter '*.nupkg')
            if ($cache.Count -ne 2) { throw 'Unexpected restored dependency closure.' }
            $cacheRows = @($cache | Sort-Object FullName -CaseSensitive | ForEach-Object { [ordered]@{ path = [IO.Path]::GetRelativePath($env:NUGET_PACKAGES, $_.FullName); bytes = $_.Length; sha256 = Get-TPHash $_.FullName } })
            Write-TPText (Join-Path $resultDir 'cache-packages.json') (($cacheRows | ConvertTo-Json -Depth 5) + "`n")
            $packageRoot = Join-Path $env:NUGET_PACKAGES ($candidate.id.ToLowerInvariant() + '/' + $candidate.version)
            $restoredEntries = @()
            foreach ($entry in $candidate.entries | Where-Object { $_.path.StartsWith('runtimes/') -or $candidate.addedEntries -ccontains $_.path }) {
                $path = Join-Path $packageRoot $entry.path
                if ((Get-Item $path).Length -ne $entry.bytes -or (Get-TPHash $path) -cne $entry.sha256) { throw "Restored payload/notice/source drift: $($entry.path)" }
                $restoredEntries += [ordered]@{ path = $entry.path; bytes = $entry.bytes; sha256 = $entry.sha256 }
            }
            Write-TPText (Join-Path $resultDir 'restored-payload.json') (($restoredEntries | ConvertTo-Json -Depth 5) + "`n")
            foreach ($path in @('obj/project.assets.json', 'Consumer.csproj', 'NuGet.Config')) { Copy-Item (Join-Path $run $path) $resultDir }
            $runs += [ordered]@{ targetFramework = $tfm; flavor = $flavor; profile = $profile; packageId = $candidate.id
                packageVersion = $candidate.version; sdkVersion = $sdk; runtimeVersion = $runtime; resultDirectory = $name
                freshPackageCache = $true; restoredPayloadFiles = $restoredEntries.Count }
            Write-Host "THIRD_PARTY_51_RUN_OK rid=$rid tfm=$tfm flavor=$flavor profile=$profile notices_and_source_verified=true"
        }
    }
}
Write-TPText (Join-Path $artifacts 'runs.json') (($runs | ConvertTo-Json -Depth 7) + "`n")
Write-Host "THIRD_PARTY_51_CONSUMER_OK rid=$rid runs=8"

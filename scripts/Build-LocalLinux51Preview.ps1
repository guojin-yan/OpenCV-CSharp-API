[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$WorkspaceRoot,
    [Parameter(Mandatory)][string]$ContainerImage,
    [Parameter(Mandatory)][string]$ContainerImageId,
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [ValidateRange(1, 4)][int]$Parallel = 2
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path $RepositoryRoot).Path
$work = (Resolve-Path $WorkspaceRoot).Path
if (-not $IsLinux -or (& uname -m) -cne 'x86_64' -or (& getconf GNU_LIBC_VERSION) -cne 'glibc 2.35') {
    throw 'This local preview producer requires native Ubuntu 22.04 x86_64/glibc 2.35.'
}
$os = Get-Content /etc/os-release -Raw
if ($os -notmatch '(?m)^ID=ubuntu$' -or $os -notmatch '(?m)^VERSION_ID="22.04"$') { throw 'Unexpected producer distro.' }
if ($ContainerImage -cne 'ubuntu:22.04@sha256:0e0a0fc6d18feda9db1590da249ac93e8d5abfea8f4c3c0c849ce512b5ef8982' -or $ContainerImageId -notmatch '^sha256:[a-f0-9]{64}$') {
    throw 'A pinned official Ubuntu baseline and the actual Docker image ID are required.'
}
function Invoke-LocalChecked {
    param([string]$Executable, [string[]]$CommandArguments, [string]$Log)
    & $Executable @CommandArguments *> $Log
    if ($LASTEXITCODE -ne 0) { Get-Content $Log -Tail 30; throw "$Executable failed; see $Log" }
}
function Write-LocalJson {
    param([string]$Path, [object]$Value)
    [IO.File]::WriteAllText($Path, (($Value | ConvertTo-Json -Depth 18) -replace "`r`n", "`n") + "`n", [Text.UTF8Encoding]::new($false))
}
$sourceRoot = Join-Path $work 'opencv-source'
$opencv = Join-Path $sourceRoot 'opencv-5.1.0'
$contrib = Join-Path $sourceRoot 'opencv_contrib-5.1.0'
$expected = @{
    $opencv = 'e4f78fbcf1a773fbed69e14500e8b734963eb585'
    $contrib = 'b67a855188a3d683813731010f6d39ac614733a4'
}
foreach ($entry in $expected.GetEnumerator()) {
    $actual = & git -C $entry.Key rev-parse HEAD
    if ($LASTEXITCODE -ne 0 -or $actual -cne $entry.Value) { throw "Incorrect official source: $($entry.Key)" }
}
$sourceCommit = & git -C $repo rev-parse HEAD
& git -C $repo diff --exit-code -- src/OpenCvSharp.Native scripts/Build-OpenCV.ps1 scripts/Apply-OpenCvSourcePatches.ps1 packaging/runtime
if ($LASTEXITCODE -ne 0) { throw 'Producer implementation has uncommitted edits.' }
$matrix = Get-Content (Join-Path $repo 'packaging/runtime/runtime-generic-linux-preview-matrix.json') -Raw | ConvertFrom-Json
$out = Join-Path $work 'linux51-output'
New-Item -ItemType Directory -Force $out | Out-Null
$env:CMAKE_BUILD_PARALLEL_LEVEL = [string]$Parallel
$env:DOTNET_ROOT = '/opt/dotnet'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
Remove-Item Env:LD_LIBRARY_PATH -ErrorAction SilentlyContinue
foreach ($item in @(Get-ChildItem Env: | Where-Object Name -Match '^OPENCV_|^OpenCvSharp')) { Remove-Item "Env:$($item.Name)" }
$full = @($matrix.profiles | Where-Object name -CEQ full)[0]
$buildArgs = @('-NoProfile', '-File', (Join-Path $repo 'scripts/Build-OpenCV.ps1'), '-WorkspaceRoot', $work,
    '-OpenCvSourceRoot', $sourceRoot, '-Rid', 'ubuntu.22.04-x64', '-Generator', 'Ninja', '-EigenIncludePath', '/usr/include/eigen3',
    '-BuildList', $full.buildList, '-GenericLinuxPreview', '-Build')
Invoke-LocalChecked pwsh $buildArgs (Join-Path $out 'opencv-build.log')
$install = Join-Path $work 'artifacts/opencv-install/opencv-5.1.0-ubuntu.22.04-x64'
$upstreamBuild = Join-Path $work 'artifacts/opencv-build/opencv-5.1.0-ubuntu.22.04-x64'
Copy-Item (Join-Path $upstreamBuild 'CMakeCache.txt') (Join-Path $out 'opencv-CMakeCache.txt') -Force
$profiles = @()
foreach ($profile in $matrix.profiles) {
    $name = [string]$profile.name
    Write-Host "Building local Linux 5.1 $name wrapper"
    $build = Join-Path $work "native51-$name"
    $cmakeArgs = @('-S', (Join-Path $repo 'src/OpenCvSharp.Native'), '-B', $build, '-G', 'Ninja', '-DCMAKE_BUILD_TYPE=Release', '-DCMAKE_EXPORT_COMPILE_COMMANDS=ON',
        "-DOPENCV_CSHARP_OPENCV_DIR=$install/lib/cmake/opencv5", "-DOPENCV_CSHARP_RUNTIME_PROFILE=$name",
        "-DOPENCV_CSHARP_OPENCV_BUILD_LIST=$($profile.buildList)")
    Invoke-LocalChecked cmake $cmakeArgs (Join-Path $out "$name-configure.log")
    Invoke-LocalChecked cmake @('--build', $build, '--parallel', [string]$Parallel) (Join-Path $out "$name-build.log")
    $compileCommands = Get-Content (Join-Path $build 'compile_commands.json') -Raw | ConvertFrom-Json
    $wrapperSources = @($compileCommands | Where-Object command -Match 'CMakeFiles/JYPPX\.OpenCV\.Native\.dir/' |
        ForEach-Object { [IO.Path]::GetRelativePath($repo, [string]$_.file).Replace('\', '/') } | Sort-Object)
    if ($wrapperSources.Count -ne $(if ($name -ceq 'full') { 49 } else { 9 })) { throw 'Actual wrapper compilation input count drifted.' }
    Invoke-LocalChecked ctest @('--test-dir', $build, '--output-on-failure') (Join-Path $out "$name-ctest.log")
    if ((Get-Content (Join-Path $out "$name-ctest.log") -Raw) -notmatch '100% tests passed, 0 tests failed out of 3') { throw 'Incomplete CTest run.' }
    # Mini reuses six modules from the Full upstream build. Only those six are staged.
    $runtime = Join-Path $out "$name-runtime"
    New-Item -ItemType Directory -Force $runtime | Out-Null
    foreach ($module in $profile.modules) {
        foreach ($suffix in @('', '.501', '.5.1.0')) {
            Copy-Item -LiteralPath "$install/lib/libopencv_$module.so$suffix" -Destination $runtime -Force
        }
    }
    Copy-Item (Join-Path $build 'libJYPPX.OpenCV.Native.so') $runtime -Force
    $auditPath = Join-Path $out "$name-elf.json"
    Invoke-LocalChecked python3 @((Join-Path $repo 'scripts/Audit-Linux51Runtime.py'), '--directory', $runtime, '--profile', $name, '--output', $auditPath) (Join-Path $out "$name-elf.log")
    $audit = Get-Content $auditPath -Raw | ConvertFrom-Json
    $provenanceArgs = @{
        Rid = 'ubuntu.22.04-x64'; RuntimeProfile = $name; OpenCvVersion = '5.1.0'; GenericLinuxPreview = $true
        NativeRuntimeDir = $build; OpenCvRuntimeDir = $runtime; OpenCvSourceDir = $opencv
        OpenCvContribSourceDir = $contrib; OutputRoot = (Join-Path $out 'runtime-inputs')
        ContainerImage = $ContainerImage; ContainerImageId = $ContainerImageId
        ContainerImageDigest = ($ContainerImage -split '@')[1]
        ContainerDistro = 'ubuntu'; ContainerDistroVersion = '22.04'; ContainerArchitecture = 'x86_64'
        ContainerPackageArchitecture = 'amd64'; ContainerLibc = 'glibc 2.35'
        CMakeVersion = ((& cmake --version | Select-Object -First 1) -replace '^cmake version ', '')
        CMakeGenerator = 'Ninja'; BuildConfiguration = 'Release'; CompilerPath = '/usr/bin/g++'
        CompilerVersion = (& g++ -dumpfullversion); NinjaVersion = (& ninja --version); DotNetVersion = (& dotnet --version)
        PowerShellVersion = $PSVersionTable.PSVersion.ToString()
        PowerShellArchiveSha256 = 'dcfe6060fc86abcb859ce1f8f80843ce50bab0585396de56380ed9f25176ac6d'
        NativeWrapperSources = ($wrapperSources -join ';'); NativeWrapperSourceCount = [string]$wrapperSources.Count
        NativeAbiFunctionCount = [string]$audit.exportCount
        ElfAuditEvidence = "LOCAL_LINUX_51_ELF_OK profile=$name modules=$($profile.modules.Count) canonical=$($audit.files.Count) exports=$($audit.exportCount) missing_dependencies=0 producer_paths=0"
        OpenCvSourcePatchEvidence = 'Repository-owned OpenCV 5.1 photo CCM per-instance color-space patch applied and audited.'
    }
    & (Join-Path $repo 'scripts/New-RuntimeInputArtifact.ps1') @provenanceArgs *> (Join-Path $out "$name-input.log")
    $inputRoot = Join-Path $out "runtime-inputs/ubuntu.22.04-x64-$name"
    $packageArgs = @('-NoProfile', '-File', (Join-Path $repo 'scripts/New-GenericLinuxPreviewPackage.ps1'),
        '-RuntimeInputRoot', $inputRoot, '-RuntimeProfile', $name, '-OpenCvVersion', '5.1.0', '-PackageRevision', '0',
        '-PackageVersion', '5.1.0.0', '-OutputDir', (Join-Path $out $profile.artifactName), '-RepositoryRoot', $repo)
    Invoke-LocalChecked pwsh $packageArgs (Join-Path $out "$name-package.log")
    $profiles += [ordered]@{
        name = $name; modules = @($profile.modules); buildList = $profile.buildList
        independentMinimalUpstreamBuild = $false; upstreamBuildProfile = 'full'
        wrapperSourceCount = [int]$provenanceArgs.NativeWrapperSourceCount
        wrapperSources = $wrapperSources
        elf = $audit; ctestOutput = [IO.File]::ReadAllText((Join-Path $out "$name-ctest.log"))
    }
}
$modifiedSource = @(& git -C $opencv diff --name-only)
if (($modifiedSource -join '|') -cne 'modules/photo/src/ccm/ccm.cpp') { throw 'Unexpected OpenCV source changes.' }
& git -C $contrib diff --quiet
if ($LASTEXITCODE -ne 0) { throw 'Contrib source is modified.' }
Write-LocalJson (Join-Path $out 'producer.json') ([ordered]@{
    schemaVersion = 1; evidenceKind = 'local-linux-5.1-preview-producer'; openCvVersion = '5.1.0'; sourceCommit = $sourceCommit
    execution = 'local-docker'; hosted = $false; publicationAllowed = $false; genericStablePromoted = $false
    image = $ContainerImage; imageId = $ContainerImageId; architecture = 'x86_64'; libc = 'glibc 2.35'
    toolchain = [ordered]@{
        powerShell = $PSVersionTable.PSVersion.ToString()
        powerShellArchiveSha256 = (Get-FileHash (Join-Path $work 'toolchain/powershell-7.4.17-linux-x64.tar.gz') -Algorithm SHA256).Hash.ToLowerInvariant()
        dotNetSdk = (& dotnet --version)
        dotNetArchiveSha512 = ((Get-Content (Join-Path $work 'toolchain/dotnet-sdk.sha512') -Raw).Trim() -split '\s+')[0]
        compiler = (& g++ -dumpfullversion); cmake = ((& cmake --version | Select-Object -First 1) -replace '^cmake version ', '')
        ninja = (& ninja --version)
    }
    openCvCommit = $expected[$opencv]; contribCommit = $expected[$contrib]; profiles = $profiles
    modifiedOpenCvFiles = $modifiedSource; contribClean = $true
})
Write-Host "LOCAL_LINUX_51_PREVIEW_PRODUCER_OK output=$out"

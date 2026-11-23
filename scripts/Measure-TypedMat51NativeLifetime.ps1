[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [string]$OpenCvNativeRuntimeDir = 'artifacts/opencv-5.1.0-win-x64/full',
    [string]$PackageArtifactPath = 'artifacts/OpenCV-CSharp-API-5.1.0-win-x64-local.zip',
    [string]$OutputPath = 'packaging/performance/typed-mat-native-lifetime-evidence-5.1.0.json',
    [int]$TimeoutSeconds = 180
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if ($TimeoutSeconds -lt 10 -or $TimeoutSeconds -gt 600) { throw 'TimeoutSeconds must be between 10 and 600.' }
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$runtimePath = if ([IO.Path]::IsPathRooted($OpenCvNativeRuntimeDir)) { [IO.Path]::GetFullPath($OpenCvNativeRuntimeDir) } else { [IO.Path]::GetFullPath((Join-Path $repo $OpenCvNativeRuntimeDir)) }
$artifactPath = if ([IO.Path]::IsPathRooted($PackageArtifactPath)) { [IO.Path]::GetFullPath($PackageArtifactPath) } else { [IO.Path]::GetFullPath((Join-Path $repo $PackageArtifactPath)) }
$outputFullPath = if ([IO.Path]::IsPathRooted($OutputPath)) { [IO.Path]::GetFullPath($OutputPath) } else { [IO.Path]::GetFullPath((Join-Path $repo ($OutputPath -replace '/', [IO.Path]::DirectorySeparatorChar))) }
foreach ($path in @($runtimePath,$artifactPath)) { if (-not (Test-Path -LiteralPath $path)) { throw "Typed Mat 5.1 input is missing: $path" } }
if (-not [Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([Runtime.InteropServices.OSPlatform]::Windows) -or [Runtime.InteropServices.RuntimeInformation]::OSArchitecture -ne [Runtime.InteropServices.Architecture]::X64) { throw 'Typed Mat 5.1 evidence requires Windows x64.' }
$sourceChanges = @(& git -C $repo status --porcelain -- src/OpenCvSharp tests/OpenCvSharp.Tests)
if ($LASTEXITCODE -ne 0 -or $sourceChanges.Count -ne 0) { throw 'Commit managed source and test changes before measuring Typed Mat lifetime.' }
$sourceCommit = ((& git -C $repo rev-parse HEAD 2>$null) | Select-Object -First 1).Trim()
$sourceEvidence = Get-Content (Join-Path $repo 'packaging/runtime/opencv-5.1.0-source-build-evidence.json') -Raw | ConvertFrom-Json
if ([string]$sourceEvidence.openCvVersion -cne '5.1.0' -or [string]$sourceEvidence.packageVersion -cne '5.1.0') { throw 'Typed Mat source evidence is not bound to OpenCV 5.1.0.' }
$nativePayload = @(Get-ChildItem -LiteralPath $runtimePath -File -Filter '*.dll' | Sort-Object Name | ForEach-Object {
    [ordered]@{ name = $_.Name; bytes = [int64]$_.Length; sha256 = (Get-FileHash $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant() }
})
if ($nativePayload.Count -ne 43 -or @($sourceEvidence.wrapperProfiles | Where-Object profile -CEQ 'full')[0].dlls.Count -ne 43) { throw 'OpenCV 5.1 Full inventory must contain 43 DLLs.' }
foreach ($row in $nativePayload) {
    $recorded = @($sourceEvidence.wrapperProfiles | Where-Object profile -CEQ 'full')[0].dlls | Where-Object name -CEQ $row.name
    if ($recorded.Count -ne 1 -or [int64]$recorded[0].length -ne $row.bytes -or [string]$recorded[0].sha256 -cne $row.sha256) { throw "Runtime DLL differs from source-build evidence: $($row.name)" }
}
Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [IO.Compression.ZipFile]::OpenRead($artifactPath)
try {
    $packagePayload = @($archive.Entries | Where-Object { $_.FullName.StartsWith('opencv-5.1.0-win-x64/full/',[StringComparison]::Ordinal) -and $_.FullName.EndsWith('.dll',[StringComparison]::OrdinalIgnoreCase) } | Sort-Object FullName | ForEach-Object {
        $entry = $_; $stream = $entry.Open()
        try { $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)).ToLowerInvariant() } finally { $stream.Dispose() }
        [ordered]@{ name = [IO.Path]::GetFileName($entry.FullName); bytes = [int64]$entry.Length; sha256 = $hash }
    })
} finally { $archive.Dispose() }
if ($packagePayload.Count -ne 43) { throw 'Local 5.1 artifact must contain 43 Full DLLs.' }
for ($i=0; $i -lt 43; $i++) {
    if ($nativePayload[$i].name -cne $packagePayload[$i].name -or $nativePayload[$i].bytes -ne $packagePayload[$i].bytes -or $nativePayload[$i].sha256 -cne $packagePayload[$i].sha256) { throw "Local ZIP differs from Full runtime at $($nativePayload[$i].name)." }
}
$expectedCases = @(
    'TypedViewReadsWritesAndCopiesRegisteredPixelsWhenNativeRuntimeIsAvailable',
    'TypedViewSupportsRowsOfNonContinuousRoiWhenNativeRuntimeIsAvailable',
    'TypedViewRejectsUnregisteredAndMismatchedTypesWhenNativeRuntimeIsAvailable',
    'TypedViewAndOwnerDisposalInvalidateFutureAccessWhenNativeRuntimeIsAvailable',
    'TypedViewDoesNotAllowMatrixHeaderChangesWhenNativeRuntimeIsAvailable',
    'TypedViewCloneAndMatCopyPreserveTheViewedRoiWhenNativeRuntimeIsAvailable',
    'TypedViewCopyToValidatesDestinationBeforeNativeRuntimeWhenNativeRuntimeIsAvailable',
    'ReadOnlyTypedViewReadsAndCopiesWithoutWriteSurfaceWhenNativeRuntimeIsAvailable',
    'ReadOnlyTypedViewRejectsDisposedOwnerAndHeaderChangesWhenNativeRuntimeIsAvailable'
)
$dotnet = Get-Command dotnet -ErrorAction Stop
$project = Join-Path $repo 'tests/OpenCvSharp.Tests/OpenCvSharp.Tests.csproj'
$tempRoot = Join-Path ([IO.Path]::GetTempPath()) ('opencv-v510-typed-mat-lifetime-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Force -Path $tempRoot | Out-Null
$frameworkRows = [Collections.Generic.List[object]]::new()
$oldSmoke = $env:OPENCV_CSHARP_NATIVE_SMOKE
$env:OPENCV_CSHARP_NATIVE_SMOKE = '1'
try {
    foreach ($framework in @('net8.0','net9.0','net10.0')) {
        $trxName = "MatView-$framework.trx"
        $args = @('test',$project,'-c','Release','-f',$framework,'--filter','FullyQualifiedName~JYPPX.OpenCvSharp.Tests.Core.MatViewTests',
            ('-p:TargetFrameworks=' + $framework),'-p:RestoreIgnoreFailedSources=true',('-p:OpenCvNativeRuntimeDir=' + $runtimePath),
            '--results-directory',$tempRoot,'--logger',('trx;LogFileName=' + $trxName),'--logger','console;verbosity=minimal')
        $startInfo = [Diagnostics.ProcessStartInfo]::new()
        $startInfo.FileName = $dotnet.Source
        $startInfo.WorkingDirectory = $repo
        $startInfo.UseShellExecute = $false
        $startInfo.RedirectStandardOutput = $true
        $startInfo.RedirectStandardError = $true
        foreach ($argument in $args) { [void]$startInfo.ArgumentList.Add($argument) }
        $startInfo.Environment['OPENCV_CSHARP_NATIVE_SMOKE'] = '1'
        $process = [Diagnostics.Process]::new()
        $process.StartInfo = $startInfo
        if (-not $process.Start()) { throw "Could not start MatView tests on $framework." }
        $stdoutTask = $process.StandardOutput.ReadToEndAsync()
        $stderrTask = $process.StandardError.ReadToEndAsync()
        if (-not $process.WaitForExit($TimeoutSeconds * 1000)) {
            try { $process.Kill($true) } catch { }
            throw "MatView tests timed out on $framework after $TimeoutSeconds seconds."
        }
        $stdout = $stdoutTask.GetAwaiter().GetResult()
        $stderr = $stderrTask.GetAwaiter().GetResult()
        if (-not [string]::IsNullOrWhiteSpace($stdout)) { Write-Host $stdout.TrimEnd() }
        if (-not [string]::IsNullOrWhiteSpace($stderr)) { Write-Host $stderr.TrimEnd() }
        if ($process.ExitCode -ne 0) { throw "MatView tests failed on $framework." }
        $process.Dispose()
        $trxPath = Join-Path $tempRoot $trxName
        if (-not (Test-Path $trxPath)) { throw "MatView TRX is missing for $framework." }
        [xml]$trx = Get-Content $trxPath -Raw
        $c = $trx.SelectSingleNode("//*[local-name()='Counters']")
        $results = @($trx.SelectNodes("//*[local-name()='UnitTestResult']"))
        $cases = @($results | ForEach-Object { ([string]$_.testName -split '\.')[-1] } | Sort-Object)
        if ($null -eq $c -or [int]$c.total -ne 9 -or [int]$c.executed -ne 9 -or [int]$c.passed -ne 9 -or [int]$c.failed -ne 0 -or [int]$c.notExecuted -ne 0 -or ($cases -join '|') -cne (@($expectedCases | Sort-Object) -join '|') -or @($results | Where-Object outcome -cne 'Passed').Count -ne 0) { throw "MatView test inventory/counters differ for $framework." }
        $testDll = Join-Path $repo "tests/OpenCvSharp.Tests/bin/Release/$framework/OpenCvSharp.Tests.dll"
        $apiDll = Join-Path $repo "src/OpenCvSharp/bin/Release/$framework/JYPPX.OpenCV.CSharp.API.dll"
        foreach ($file in @($testDll,$apiDll)) { if (-not (Test-Path $file)) { throw "Release assembly missing for $framework." } }
        $frameworkRows.Add([ordered]@{
            framework = $framework; total = [int]$c.total; executed = [int]$c.executed; passed = [int]$c.passed; failed = [int]$c.failed; skipped = [int]$c.notExecuted
            testAssemblySha256 = (Get-FileHash $testDll -Algorithm SHA256).Hash.ToLowerInvariant()
            apiAssemblySha256 = (Get-FileHash $apiDll -Algorithm SHA256).Hash.ToLowerInvariant()
            trx = [ordered]@{ path = '%TEMP%/' + (Split-Path -Leaf $tempRoot) + '/' + $trxName; length = (Get-Item $trxPath).Length; sha256 = (Get-FileHash $trxPath -Algorithm SHA256).Hash.ToLowerInvariant() }
        })
        Write-Host "TYPED_MAT_NATIVE_TFM_OK framework=$framework total=9 passed=9 failed=0 trx=$trxPath"
    }
} finally {
    if ($null -eq $oldSmoke) { Remove-Item Env:OPENCV_CSHARP_NATIVE_SMOKE -ErrorAction SilentlyContinue } else { $env:OPENCV_CSHARP_NATIVE_SMOKE = $oldSmoke }
    # Per-framework test restores replace project.assets.json. Restore the project's
    # default target frameworks so subsequent --no-restore guards can execute both.
    & $dotnet.Source restore $project -p:RestoreIgnoreFailedSources=true
    if ($LASTEXITCODE -ne 0) { throw 'Could not restore the default test target frameworks after Typed Mat measurement.' }
}
$evidence = [ordered]@{
    '$schema'='typed-mat-native-lifetime-evidence-5.1.0.schema.json'; schemaVersion=1; status='verified'; sourceCommit=$sourceCommit; packageVersion='5.1.0'
    runner=[ordered]@{ os=[Runtime.InteropServices.RuntimeInformation]::OSDescription; dotnetSdk=((& $dotnet.Source --version 2>$null | Select-Object -First 1).Trim()); architecture='x64'; configuration='Release' }
    artifact=[ordered]@{ path='artifacts/OpenCV-CSharp-API-5.1.0-win-x64-local.zip'; bytes=(Get-Item $artifactPath).Length; sha256=(Get-FileHash $artifactPath -Algorithm SHA256).Hash.ToLowerInvariant(); fullProfileEntryPrefix='opencv-5.1.0-win-x64/full/' }
    runtime=[ordered]@{
        openCvVersion='5.1.0'; nativeAbiVersion=1; nativeModuleFiles=42; nativeRuntimeDirectory='artifacts/opencv-5.1.0-win-x64/full'; nativePayload=$nativePayload
        wrapperLoaderSha256=([string](@($nativePayload | Where-Object name -CEQ 'JYPPX.OpenCV.Native.dll')[0].sha256))
        opencvCoreSha256=([string](@($nativePayload | Where-Object name -CEQ 'opencv_core510.dll')[0].sha256))
        opencvImgProcSha256=([string](@($nativePayload | Where-Object name -CEQ 'opencv_imgproc510.dll')[0].sha256))
        opencvImgCodecsSha256=([string](@($nativePayload | Where-Object name -CEQ 'opencv_imgcodecs510.dll')[0].sha256))
    }
    test=[ordered]@{ class='JYPPX.OpenCvSharp.Tests.Core.MatViewTests'; filter='FullyQualifiedName~JYPPX.OpenCvSharp.Tests.Core.MatViewTests'; nativeSmokeEnabled=$true; targetFrameworks=$frameworkRows.ToArray(); verifiedCases=$expectedCases }
    limitations=@(
        'Exact OpenCV 5.1.0 Windows x64 Full artifact and all 43 native DLL hashes are recorded; this is not NuGet consumer or cross-RID evidence.',
        'Native lifetime cases are verified on net8.0, net9.0 and net10.0. netcoreapp3.1 and net5.0 through net7.0 remain compile-only; legacy .NET Framework targets exclude the preview API.',
        'MatView and ReadOnlyMatView remain conditional preview contracts. These results do not authorize stable cross-target promotion or prove long-lived leak absence.'
    )
}
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $outputFullPath) | Out-Null
[IO.File]::WriteAllText($outputFullPath,(($evidence | ConvertTo-Json -Depth 12) + [Environment]::NewLine),[Text.UTF8Encoding]::new($false))
Write-Host "TYPED_MAT_510_NATIVE_LIFETIME_MEASURED source_commit=$sourceCommit frameworks=net8.0,net9.0,net10.0 cases=9 output=$outputFullPath"

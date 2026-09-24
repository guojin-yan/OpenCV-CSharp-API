param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [int]$TimeoutSeconds = 120
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if ($TimeoutSeconds -lt 10 -or $TimeoutSeconds -gt 600) { throw 'TimeoutSeconds must be between 10 and 600.' }

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$dotnetCommand = Get-Command dotnet -ErrorAction Stop
$project = Join-Path $repo 'tests/OpenCvSharp.Tests/OpenCvSharp.Tests.csproj'
if (-not (Test-Path -LiteralPath $project -PathType Leaf)) { throw "Test project was not found: $project" }

$frameworks = @('net8.0', 'net10.0')
foreach ($framework in $frameworks) {
    $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $dotnetCommand.Source
    $startInfo.WorkingDirectory = $repo
    $startInfo.UseShellExecute = $false
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    foreach ($argument in @(
            'test', $project, '-c', 'Release', '-f', $framework, '--no-restore',
            '--filter', 'FullyQualifiedName~IdentifyDeterministicMutationCorpus',
            '--logger', 'console;verbosity=minimal')) {
        [void]$startInfo.ArgumentList.Add($argument)
    }

    $process = [System.Diagnostics.Process]::new()
    $process.StartInfo = $startInfo
    if (-not $process.Start()) { throw "Could not start the image preflight mutation corpus on $framework." }
    $stdoutTask = $process.StandardOutput.ReadToEndAsync()
    $stderrTask = $process.StandardError.ReadToEndAsync()
    if (-not $process.WaitForExit($TimeoutSeconds * 1000)) {
        try { $process.Kill($true) } catch { }
        throw "Image preflight mutation corpus timed out on $framework after $TimeoutSeconds seconds."
    }
    $stdout = $stdoutTask.GetAwaiter().GetResult()
    $stderr = $stderrTask.GetAwaiter().GetResult()
    if (-not [string]::IsNullOrWhiteSpace($stdout)) { Write-Host $stdout.TrimEnd() }
    if (-not [string]::IsNullOrWhiteSpace($stderr)) { Write-Host $stderr.TrimEnd() }
    if ($process.ExitCode -ne 0) { throw "Image preflight mutation corpus failed on $framework with exit code $($process.ExitCode)." }
    Write-Host "IMAGE_PREFLIGHT_MUTATION_FRAMEWORK_OK framework=$framework timeout_seconds=$TimeoutSeconds"
    $process.Dispose()
}

Write-Host 'IMAGE_PREFLIGHT_MUTATION_CORPUS_OK fixtures=13 replacements=5 frameworks=2 native_runtime_required=false'

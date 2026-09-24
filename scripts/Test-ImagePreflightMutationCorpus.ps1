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
$manifestPath = Join-Path $repo 'packaging/codec/image-preflight-mutation-corpus.json'
$schemaPath = Join-Path $repo 'packaging/codec/image-preflight-mutation-corpus.schema.json'
if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf) -or -not (Test-Path -LiteralPath $schemaPath -PathType Leaf)) { throw 'Image preflight mutation corpus manifest/schema is missing.' }
if (-not (Test-Json -LiteralPath $manifestPath -SchemaFile $schemaPath -ErrorAction Stop)) { throw 'Image preflight mutation corpus manifest failed JSON Schema validation.' }
$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
$testText = Get-Content -LiteralPath (Join-Path $repo 'tests/OpenCvSharp.Tests/ImgCodecs/ImagePreflightTests.cs') -Raw
foreach ($fixture in @($manifest.fixtures)) {
    if ($testText.IndexOf('Name = "' + [string]$fixture + '"', [StringComparison]::Ordinal) -lt 0) { throw "Mutation fixture is missing from the focused test: $fixture" }
}
if ($testText.IndexOf('IdentifyDeterministicMutationCorpusPreservesSeekAndConsumesNonSeekStreams', [StringComparison]::Ordinal) -lt 0) { throw 'Stream mutation test is missing from the focused test.' }
if (@($manifest.frameworks).Count -ne 2 -or @($manifest.replacementBytes).Count -ne 5 -or @($manifest.fixtures).Count -lt 13) { throw 'Mutation corpus manifest dimensions drifted.' }

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

Write-Host "IMAGE_PREFLIGHT_MUTATION_CORPUS_OK fixtures=$(@($manifest.fixtures).Count) replacements=$(@($manifest.replacementBytes).Count) frameworks=$(@($manifest.frameworks).Count) minimum_mutations=$($manifest.expectedMinimumMutations) minimum_stream_cases=$($manifest.expectedMinimumStreamCases) native_runtime_required=false"

param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [int]$TimeoutSeconds = 180
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$project = Join-Path $repo 'src/OpenCvSharp/OpenCvSharp.csproj'
$frameworks = @('net46','net461','net462','net47','net471','net472','net48','net481','netcoreapp3.1','net5.0','net6.0','net7.0','net8.0','net9.0','net10.0')
$dotnet = Get-Command dotnet -ErrorAction Stop
foreach ($framework in $frameworks) {
    $process = Start-Process -FilePath $dotnet.Source -ArgumentList @('build',$project,'-c','Release','-f',$framework,'--no-restore') -WorkingDirectory $repo -NoNewWindow -Wait -PassThru
    if ($process.ExitCode -ne 0) { throw "Typed Mat target-framework build failed for $framework with exit code $($process.ExitCode)." }
    $output = Join-Path $repo ("src/OpenCvSharp/bin/Release/$framework/JYPPX.OpenCV.CSharp.API.dll" -replace '/', [IO.Path]::DirectorySeparatorChar)
    if (-not (Test-Path -LiteralPath $output -PathType Leaf)) { throw "Managed output is missing for ${framework}: $output" }
    Write-Host "TYPED_MAT_TFM_BUILD_OK framework=$framework"
}

Write-Host "TYPED_MAT_TFM_MATRIX_OK frameworks=$($frameworks.Count) span_preview_tfms=netcoreapp3.1+ old_tfms=net46-net7"

. (Join-Path $PSScriptRoot 'LocalPackageSbom51.Common.ps1')

function Get-Performance51SourceSet {
    param([string]$RepositoryRoot)
    $scopes = @('Directory.Build.props', 'src/OpenCvSharp', 'tools/CodecBenchmark', 'tools/TypedMatBenchmark',
        'tools/ScenarioBenchmark', 'tests/OpenCvSharp.Tests/Dnn/Fixtures/identity-opset13.onnx.base64',
        'scripts/ModernApi51Evidence.Common.ps1', 'scripts/LocalPackageSbom51.Common.ps1',
        'scripts/Performance51.Common.ps1', 'scripts/Measure-Performance51Evidence.ps1')
    $paths = [string[]]@(& git -C $RepositoryRoot ls-files --cached --others --exclude-standard -- @scopes)
    if ($LASTEXITCODE -ne 0 -or $paths.Count -eq 0) { throw 'Cannot enumerate performance source inputs.' }
    [Array]::Sort($paths, [StringComparer]::Ordinal)
    $rows = foreach ($path in $paths) {
        $text = [IO.File]::ReadAllText((Join-Path $RepositoryRoot $path))
        $bytes = [Text.UTF8Encoding]::new($false).GetBytes((($text -replace "`r`n", "`n") -replace "`r", "`n").TrimEnd() + "`n")
        "$path|$($bytes.Length)|$(Get-Performance51TextHash -Text ([Text.Encoding]::UTF8.GetString($bytes)))"
    }
    return [ordered]@{ paths = $scopes; fileCount = $paths.Count; sha256 = Get-Performance51TextHash -Text (($rows -join "`n") + "`n") }
}

function Get-Performance51TextHash {
    param([AllowEmptyString()][string]$Text)
    return [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.UTF8Encoding]::new($false).GetBytes($Text))).ToLowerInvariant()
}

function Get-Performance51Package {
    param([string]$RepositoryRoot, [string]$SourceCommit)
    $relative = 'packaging/release/opencv-5.1.0-local-sbom-evidence.json'
    $record = Get-Content -LiteralPath (Join-Path $RepositoryRoot $relative) -Raw | ConvertFrom-Json
    if ($record.status -cne 'verified-local-windows-x64' -or $record.openCvVersion -cne '5.1.0' -or $record.packageVersion -cne '5.1.0') { throw 'Performance requires verified local 5.1 packages.' }
    & git -C $RepositoryRoot merge-base --is-ancestor ([string]$record.sourceCommit) $SourceCommit 2>$null
    if ($LASTEXITCODE -ne 0) { throw 'Measured package source is not an ancestor of the benchmark source.' }
    $bindings = Get-ModernApi51PackageBindings -RepositoryRoot $RepositoryRoot -SourceCommit $SourceCommit
    $full = @($record.packages | Where-Object role -CEQ 'full')
    $native = @($bindings.packages | Where-Object profile -CEQ 'full')
    if ($full.Count -ne 1 -or $full[0].packageId -cne 'JYPPX.OpenCV.runtime.win-x64' -or $full[0].sha256 -notmatch '^[0-9a-f]{64}$') { throw 'Full performance package identity is invalid.' }
    $entries = @($full[0].entries | Where-Object path -Clike 'runtimes/win-x64/native/*.dll' | Sort-Object path | ForEach-Object {
        [ordered]@{ name = [IO.Path]::GetFileName([string]$_.path); bytes = [int64]$_.bytes; sha256 = [string]$_.sha256 }
    })
    if (($entries | ConvertTo-Json -Depth 4 -Compress) -cne ($native[0].payload | ConvertTo-Json -Depth 4 -Compress)) { throw 'Performance package DLLs differ from verified native consumers.' }
    return [ordered]@{
        evidence = [ordered]@{ path = $relative; sha256 = Get-ModernApi51Sha256 -Path (Join-Path $RepositoryRoot $relative) }
        artifactPath = [string]$full[0].artifactPath; fileName = [string]$full[0].fileName
        sourceCommit = [string]$record.sourceCommit; bytes = [int64]$full[0].bytes; sha256 = [string]$full[0].sha256
        payload = $entries
    }
}

function Assert-Performance51Output {
    param([string]$Scenario, [string]$TargetFramework, [object]$Output)
    if ($Output.status -cne 'measured' -or $Output.targetFramework -cne $TargetFramework -or [int]$Output.iterations -ne 100) { throw 'Performance output was skipped or has wrong framework/iterations.' }
    if ($Scenario -cin @('codec', 'scenario') -and ($Output.packageVersion -cne '5.1.0' -or $Output.openCvVersion -cne '5.1.0')) { throw 'Performance output has wrong package/OpenCV version.' }
    if ($Scenario -ceq 'codec') {
        if ([int]$Output.rows -ne 64 -or [int]$Output.columns -ne 64 -or [int]$Output.inputBytes -ne 12288 -or
            $Output.inputSha256 -cne '518f05b99bd5822f768487e0c9c969b3f6b5ba24f1b1531df4231f674413a0af' -or
            [int]$Output.encodedBytes -le 0 -or $Output.encodedSha256 -notmatch '^[0-9a-f]{64}$') { throw 'Codec workload drifted.' }
        foreach ($name in @('encodeByteArray', 'encodeBufferWriter', 'encodeBufferWriterReused')) {
            if ([int]$Output.$name.encodedBytes -ne [int]$Output.encodedBytes) { throw 'Codec encoded lengths differ.' }
        }
        $writer = $Output.encodeBufferWriterReused
        if ([int]$writer.getSpanCalls -ne 100 -or [int]$writer.advanceCalls -ne 100 -or -not $writer.writerReused -or [int]$writer.writerCapacity -ne 4096) { throw 'Reusable writer contract drifted.' }
        foreach ($name in @('decodeByteArray', 'decodeSpanWithPreflight', 'decodeStreamWithPreflight')) {
            if ([int64]$Output.$name.checksum -ne 156672000 -or [int]$Output.$name.decodedBytes -ne 12288) { throw 'Codec decode checksum/length drifted.' }
        }
    }
    elseif ($Scenario -ceq 'typedMat') {
        if ([int]$Output.rows -ne 118 -or [int]$Output.columns -ne 122 -or
            $Output.inputSha256 -cne '70986737435b5f89e119558ba7d29e5bb8768dd18c16beaaaa09298bf586b275') { throw 'Typed Mat workload drifted.' }
        foreach ($name in @('rowAccessor', 'matView', 'readOnlyMatView')) {
            if ([int64]$Output.$name.checksum -ne 10077200) { throw 'Typed Mat checksum drifted.' }
        }
    }
    elseif ($Scenario -ceq 'scenario') {
        $video = $Output.video; $dnn = $Output.dnn
        if ($video.targetFramework -cne 'net10.0' -or [int]$video.iterations -ne 100 -or [int]$video.frameCount -ne 3 -or
            [int]$video.rows -ne 24 -or [int]$video.columns -ne 32 -or $video.codec -cne 'MJPG' -or
            [int]$video.fileBytes -ne 6288 -or $video.fileSha256 -cne '3137a5dc45d1dadf29aee22d2eb140a55c31fe240a80fe66dd4cc069fe27fd56' -or
            [int64]$video.metric.checksum -ne 46555200) { throw 'Video workload drifted.' }
        if ($dnn.targetFramework -cne 'net10.0' -or [int]$dnn.iterations -ne 100 -or [int]$dnn.modelBytes -ne 147 -or
            $dnn.modelSha256 -cne '326793cdb2fc2da739a715c3f3ff71d09779b389ad29e56bbfccc4313e900744' -or
            $dnn.outputName -cne 'output' -or [int64]$dnn.outputChecksum -ne 1000000 -or [int64]$dnn.metric.checksum -ne 1000000 -or
            [int64]$dnn.profileTickCount -lt 0 -or [int]$dnn.profileLayerCount -ne 1 -or [int64]$dnn.blobBytes -ne 16 -or [int64]$dnn.weightsBytes -ne 0) { throw 'DNN workload drifted.' }
        foreach ($metric in @($video.metric, $dnn.metric)) {
            if ([int64]$metric.baselineWorkingSetBytes -le 0 -or [int64]$metric.peakWorkingSetBytes -lt [int64]$metric.baselineWorkingSetBytes) { throw 'Peak working set is invalid.' }
        }
    }
    else { throw 'Unknown performance scenario.' }
    $metrics = if ($Scenario -ceq 'codec') { @($Output.encodeByteArray, $Output.encodeBufferWriter, $Output.encodeBufferWriterReused, $Output.decodeByteArray, $Output.decodeSpanWithPreflight, $Output.decodeStreamWithPreflight) }
        elseif ($Scenario -ceq 'typedMat') { @($Output.rowAccessor, $Output.matView, $Output.readOnlyMatView) }
        else { @($Output.video.metric, $Output.dnn.metric) }
    foreach ($metric in $metrics) {
        if ($null -eq $metric.managedAllocatedBytes -or [int64]$metric.managedAllocatedBytes -lt 0 -or $null -eq $metric.elapsedTicks -or [int64]$metric.elapsedTicks -le 0) { throw 'Missing/negative performance metric.' }
    }
}

function Get-Performance51Metrics {
    param([object[]]$Runs)
    $samples = [ordered]@{}
    foreach ($run in $Runs) {
        $output = $run.stdoutJson | ConvertFrom-Json
        $rows = [ordered]@{}
        if ($run.scenario -ceq 'codec') {
            foreach ($name in @('encodeByteArray', 'encodeBufferWriter', 'encodeBufferWriterReused', 'decodeByteArray', 'decodeSpanWithPreflight', 'decodeStreamWithPreflight')) { $rows["codec/$name"] = $output.$name }
        }
        elseif ($run.scenario -ceq 'typedMat') {
            foreach ($name in @('rowAccessor', 'matView', 'readOnlyMatView')) { $rows["typedMat/$($run.targetFramework)/$name"] = $output.$name }
        }
        else { foreach ($name in @('video', 'dnn')) { $rows["$name/net10.0"] = $output.$name.metric } }
        foreach ($name in $rows.Keys) {
            if (-not $samples.Contains($name)) { $samples[$name] = [Collections.Generic.List[object]]::new() }
            $samples[$name].Add($rows[$name])
        }
    }
    $summary = [ordered]@{}
    foreach ($name in $samples.Keys) {
        if ($samples[$name].Count -ne 3) { throw "Expected three measurements for $name." }
        $row = [ordered]@{}
        $fields = @('managedAllocatedBytes', 'elapsedTicks')
        if ($name -cin @('video/net10.0', 'dnn/net10.0')) { $fields += 'peakWorkingSetBytes' }
        foreach ($field in $fields) { $row[$field] = [int64](@($samples[$name] | ForEach-Object { [int64]$_.$field } | Sort-Object)[1]) }
        $summary[$name] = $row
    }
    return $summary
}

function Assert-Performance51Comparison {
    param([object]$Baseline, [object]$Candidate, [string]$RepositoryRoot)
    if (($Baseline.runner | ConvertTo-Json -Depth 5 -Compress) -cne ($Candidate.runner | ConvertTo-Json -Depth 5 -Compress) -or
        ($Baseline.runtimePackage | ConvertTo-Json -Depth 8 -Compress) -cne ($Candidate.runtimePackage | ConvertTo-Json -Depth 8 -Compress)) { throw 'Regression comparison requires the same runner and exact runtime package.' }
    & git -C $RepositoryRoot merge-base --is-ancestor ([string]$Baseline.sourceCommit) ([string]$Candidate.sourceCommit) 2>$null
    if ($LASTEXITCODE -ne 0) { throw 'Regression candidate must descend from the measured baseline.' }
    foreach ($property in $Baseline.metrics.PSObject.Properties) {
        foreach ($field in $property.Value.PSObject.Properties) {
            $threshold = switch ($field.Name) { 'elapsedTicks' { 20 } 'managedAllocatedBytes' { 10 } 'peakWorkingSetBytes' { 10 } }
            $value = $Candidate.metrics.($property.Name).($field.Name)
            if ($null -eq $value -or [decimal]$value -gt [decimal]$field.Value * (1 + [decimal]$threshold / 100)) { throw "Performance regression: $($property.Name)/$($field.Name)." }
        }
    }
}

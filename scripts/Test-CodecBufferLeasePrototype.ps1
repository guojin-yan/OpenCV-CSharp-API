param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path

$sourcePath = Join-Path $repo 'src/OpenCvSharp/Internal/CodecBufferLease.cs'
$testPath = Join-Path $repo 'tests/OpenCvSharp.Tests/Core/CodecBufferLeaseTests.cs'
$adrPath = Join-Path $repo 'docs/articles/codec-buffer-lease-adr.md'
$managedApiPath = Join-Path $repo 'compatibility/managed-public-api.txt'
$nativeManifestPath = Join-Path $repo 'src/OpenCvSharp.Native/generated/native_abi_manifest.txt'
foreach ($path in @($sourcePath, $testPath, $adrPath, $managedApiPath, $nativeManifestPath)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "CodecBufferLease prototype evidence is missing: $path" }
}

$source = Get-Content -LiteralPath $sourcePath -Raw
$tests = Get-Content -LiteralPath $testPath -Raw
$adr = Get-Content -LiteralPath $adrPath -Raw
$managedApi = Get-Content -LiteralPath $managedApiPath -Raw
$nativeManifest = Get-Content -LiteralPath $nativeManifestPath -Raw

if ($source -notmatch 'internal sealed class CodecBufferLease') { throw 'CodecBufferLease must remain internal.' }
if ($source -match '\bpublic\s+(?:sealed\s+)?class\s+CodecBufferLease') { throw 'CodecBufferLease leaked into the public API.' }
if ($source -notmatch 'static CodecBufferLease Pin\(' -or $source -notmatch 'static CodecBufferLease FromNativePointer\(') { throw 'Pinned-array and native-pointer prototype paths are incomplete.' }
if ($source -notmatch 'EnterOperation\(' -or $source -notmatch 'activeOperations' -or $source -notmatch 'Dispose\(') { throw 'CodecBufferLease operation guard/disposal boundary is incomplete.' }
if ($source -notmatch 'checked\(\(long\)\(rows - 1\) \* stepBytes\)') { throw 'CodecBufferLease final-row checked arithmetic is missing.' }
if ($source -notmatch 'releaseException' -or $source -notmatch 'releaseCallback\(\)') { throw 'CodecBufferLease callback exception isolation is missing.' }
if ($source -match 'DllImport|LibraryImport|NativeMethods|jyppx_ocv_') { throw 'CodecBufferLease prototype must not add native ABI bindings.' }
if ($managedApi -match 'CodecBufferLease|MatBufferLease') { throw 'CodecBufferLease prototype must not enter the managed public API baseline.' }
if ($nativeManifest -match 'codec_buffer_lease|mat_buffer_lease') { throw 'CodecBufferLease prototype must not add native ABI symbols.' }

$factCount = ([regex]::Matches($tests, '\[Fact\]')).Count
if ($factCount -ne 6) { throw "CodecBufferLease focused test count drifted: $factCount" }
foreach ($marker in @('InvalidOwnerAndLayoutFailBeforePinning','OperationGuardDefersReleaseAndDisposeIsIdempotent','ReleaseCallbackRunsOnceAndExceptionsAreCaptured','NativePointerLeaseRequiresOwnerCallbackAndReleasesOnce','PinAndReleaseStressKeepsExactlyOneCallbackPerLease')) {
    if ($tests -notmatch [regex]::Escape($marker)) { throw "CodecBufferLease focused test is missing: $marker" }
}
if ($adr -notmatch 'internal/P1 prototype' -or $adr -notmatch 'does not create a Mat header or native ABI entry') { throw 'CodecBufferLease ADR does not preserve the internal/no-ABI boundary.' }

Write-Host "CODEC_BUFFER_LEASE_PROTOTYPE_OK facts=$factCount public_api=absent native_abi=absent operation_guard=verified callback_exactly_once=verified"

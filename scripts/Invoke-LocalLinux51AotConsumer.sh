#!/bin/sh
set -eu
. /etc/os-release
case "$ID:$VERSION_ID:$(getconf GNU_LIBC_VERSION)" in
  'ubuntu:22.04:glibc 2.35') rid=ubuntu.22.04-x64 ;;
  'debian:12:glibc 2.36') rid=debian.12-x64 ;;
  'fedora:44:glibc 2.43') rid=fedora.44-x64 ;;
  *) echo 'Unexpected AOT consumer system' >&2; exit 1 ;;
esac
test "$(uname -m)" = x86_64
for forbidden in dotnet pwsh gcc g++ clang clang++ cc c++ cmake ninja; do
  if command -v "$forbidden"; then exit 9; fi
done
for forbidden in /producer /workspace /repo-input /upstream-input /opt/dotnet /opt/powershell; do
  test ! -e "$forbidden"
done
if env | awk -F= '$1 ~ /^(OPENCV_|OpenCvSharp|LD_LIBRARY_PATH$|LD_PRELOAD$|LD_AUDIT$)/ { found=1 } END { exit !found }'; then
  echo 'Native loader override found' >&2
  exit 9
fi
{
  printf 'rid=%s\ndistro=%s\nversion=%s\narchitecture=%s\nlibc=%s\n' "$rid" "$ID" "$VERSION_ID" "$(uname -m)" "$(getconf GNU_LIBC_VERSION)"
  printf 'dotnetAbsent=true\ncompilerAbsent=true\nproducerPathsAbsent=true\nloaderOverridesAbsent=true\n'
} > /consumer/artifacts/environment.log
sha256sum /kit/scripts/*.sh > /consumer/artifacts/scripts.sha256
for profile in full mini; do
  output="/consumer/artifacts/$profile"
  test ! -e "$output"
  mkdir "$output"
  (
    cd "/consumer/publish/$profile"
    sha256sum ./* | LC_ALL=C sort -k2 > "$output/publish.sha256"
  )
  code=0
  timeout 120 "/consumer/publish/$profile/LinuxAotConsumer" "$profile" "$output" > "$output/execute.stdout.log" 2> "$output/execute.stderr.log" || code=$?
  printf '%s\n' "$code" > "$output/exit-code.log"
  test "$code" -eq 0
  if [ "$profile" = full ]; then
    # OpenCV 5.1 emits this existing diagnostic when setting its CPU target.
    awk '$0 !~ /^\[ WARN:0@[0-9.]+\] global net_impl_backend.cpp:388 setPreferableTarget Targets are not supported by the new graph engine for now$/ {exit 1} END {if (NR > 1) exit 1}' "$output/execute.stderr.log"
  else
    test ! -s "$output/execute.stderr.log"
  fi
  tail -n 1 "$output/execute.stdout.log" | awk '$0 != "AOT_LINUX_SMOKE_OK" {exit 1}'
  sha256sum "$output/three-frames.avi" > "$output/video.sha256"
  echo "LOCAL_LINUX_51_AOT_RUN_OK rid=$rid profile=$profile"
done
echo "LOCAL_LINUX_51_AOT_CONSUMER_OK rid=$rid runs=2"

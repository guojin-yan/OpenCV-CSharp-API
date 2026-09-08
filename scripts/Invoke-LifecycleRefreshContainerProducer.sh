#!/bin/sh
set -eu

require_value() {
  variable_name="$1"
  eval "variable_value=\${$variable_name:-}"
  if [ -z "$variable_value" ]; then
    echo "Required lifecycle-refresh environment variable is empty: $variable_name" >&2
    exit 1
  fi
}

for variable_name in \
  LIFECYCLE_REFRESH_RID \
  LIFECYCLE_REFRESH_PROFILE \
  LIFECYCLE_REFRESH_OPENCV_VERSION \
  LIFECYCLE_REFRESH_CONTAINER_IMAGE \
  LIFECYCLE_REFRESH_CONTAINER_IMAGE_ID \
  LIFECYCLE_REFRESH_CONTAINER_IMAGE_DIGEST \
  LIFECYCLE_REFRESH_BUILD_LIST \
  LIFECYCLE_REFRESH_HOST_DOTNET_VERSION; do
  require_value "$variable_name"
done

case "$LIFECYCLE_REFRESH_PROFILE" in
  full)
    expected_opencv_count=17
    expected_canonical_count=18
    expected_runtime_file_count=52
    expected_direct_opencv=17
    abi_manifest="src/OpenCvSharp.Native/generated/native_abi_manifest.txt"
    ;;
  mini)
    expected_opencv_count=6
    expected_canonical_count=7
    expected_runtime_file_count=19
    expected_direct_opencv=6
    abi_manifest="src/OpenCvSharp.Native/generated/native_abi_mini_manifest.txt"
    ;;
  *)
    echo "Unsupported lifecycle-refresh profile: $LIFECYCLE_REFRESH_PROFILE" >&2
    exit 1
    ;;
esac

case "$LIFECYCLE_REFRESH_OPENCV_VERSION" in
  ""|*[!0-9A-Za-z.+-]*)
    echo "Unsafe lifecycle-refresh OpenCV version: $LIFECYCLE_REFRESH_OPENCV_VERSION" >&2
    exit 1
    ;;
esac

case "$LIFECYCLE_REFRESH_RID" in
  fedora.44-x64)
    expected_distro="fedora"
    expected_distro_version="44"
    powershell_version="7.4.17"
    powershell_archive_name="powershell-7.4.17-linux-x64.tar.gz"
    powershell_archive_sha256="dcfe6060fc86abcb859ce1ff80843ce50bab0585396de56380ed9f25176ac6d"
    dnf -y --setopt=install_weak_deps=False install \
      binutils ca-certificates cmake curl diffutils findutils gcc-c++ git gzip \
      libicu libunwind ninja-build openssl-libs tar util-linux which zlib
    dnf clean all
    package_architecture="$(rpm --eval '%{_arch}')"
    package_evidence="$(rpm -q binutils cmake gcc-c++ git ninja-build | tr '\n' ';')"
    container_libc="$(getconf GNU_LIBC_VERSION)"
    ;;
  alpine.3.23-x64)
    expected_distro="alpine"
    expected_distro_version="3.23"
    powershell_version="7.4.17"
    powershell_archive_name="powershell-7.4.17-linux-musl-x64.tar.gz"
    powershell_archive_sha256="143a1de65ea320c36a0b4bd1808fe65561e5ab12fd66d5f63f78b0b3d66b4397"
    apk add --no-cache \
      bash binutils build-base ca-certificates cmake curl diffutils findutils git gzip \
      icu-libs krb5-libs libgcc libintl libssl3 libstdc++ linux-headers samurai tar \
      util-linux-misc zlib
    if ! command -v ninja >/dev/null 2>&1; then
      ln -s /usr/bin/samu /usr/local/bin/ninja
    fi
    package_architecture="$(apk --print-arch)"
    package_evidence="$(apk info -e -v binutils build-base cmake git samurai | tr '\n' ';')"
    container_libc="$(ldd --version 2>&1 | sed -n '1p')"
    ;;
  *)
    echo "Unsupported lifecycle-refresh RID: $LIFECYCLE_REFRESH_RID" >&2
    exit 1
    ;;
esac

. /etc/os-release
test "${ID:-}" = "$expected_distro"
test "${VERSION_ID:-}" = "$expected_distro_version"
test "$(uname -m)" = "x86_64"
test "$package_architecture" = "x86_64"
test -n "$container_libc"
test -n "$package_evidence"

powershell_archive="/tmp/$powershell_archive_name"
powershell_url="https://github.com/PowerShell/PowerShell/releases/download/v$powershell_version/$powershell_archive_name"
curl --fail --location --retry 4 --retry-delay 2 --output "$powershell_archive" "$powershell_url"
printf '%s  %s\n' "$powershell_archive_sha256" "$powershell_archive" | sha256sum -c -
mkdir -p "/opt/microsoft/powershell/$powershell_version"
tar -xzf "$powershell_archive" -C "/opt/microsoft/powershell/$powershell_version"
chmod +x "/opt/microsoft/powershell/$powershell_version/pwsh"
ln -s "/opt/microsoft/powershell/$powershell_version/pwsh" /usr/local/bin/pwsh

installed_powershell_version="$(pwsh -NoProfile -Command '$PSVersionTable.PSVersion.ToString()' | tr -d '\r')"
process_architecture="$(pwsh -NoProfile -Command '[Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture.ToString()' | tr -d '\r')"
test "$installed_powershell_version" = "$powershell_version"
test "$process_architecture" = "X64"

compiler_path="$(command -v g++)"
compiler_version="$(g++ --version | sed -n '1p') target=$(g++ -dumpmachine)"
assembler_target="$(as --version | sed -n '$p' | tr -cd '[:alnum:]_.+-')"
assembler_version="$(as --version | sed -n '1p') target=$assembler_target"
cmake_version="$(cmake --version | sed -n '1p')"
ninja_version="$(ninja --version)"
cpu_model="$(lscpu | sed -n 's/^Model name:[[:space:]]*//p' | sed -n '1p')"
memory_bytes="$(( $(awk '/^MemTotal:/ { print $2 }' /proc/meminfo) * 1024 ))"
disk_available_bytes="$(df -P -B1 / | awk 'NR == 2 { print $4 }')"
test -x "$compiler_path"
test -n "$compiler_version"
test -n "$assembler_version"
test -n "$cmake_version"
test -n "$ninja_version"
test -n "$cpu_model"
test "$memory_bytes" -gt 0
test "$disk_available_bytes" -gt 0

echo "LIFECYCLE_REFRESH_CONTAINER_EVIDENCE target=$LIFECYCLE_REFRESH_RID/$LIFECYCLE_REFRESH_PROFILE distro=$ID version=$VERSION_ID architecture=$(uname -m) package_architecture=$package_architecture libc=$container_libc image=$LIFECYCLE_REFRESH_CONTAINER_IMAGE digest=$LIFECYCLE_REFRESH_CONTAINER_IMAGE_DIGEST"
echo "LIFECYCLE_REFRESH_TOOLCHAIN_EVIDENCE target=$LIFECYCLE_REFRESH_RID/$LIFECYCLE_REFRESH_PROFILE compiler=$compiler_path compiler_version=$compiler_version assembler=$assembler_version cmake=$cmake_version ninja=$ninja_version powershell=$installed_powershell_version powershell_sha256=$powershell_archive_sha256 host_dotnet=$LIFECYCLE_REFRESH_HOST_DOTNET_VERSION packages=$package_evidence"

candidate_matrix="packaging/runtime/runtime-lifecycle-refresh-matrix.json"
pwsh -NoProfile -File ./scripts/Test-LifecycleRefreshRuntimeMatrix.ps1
profile_evidence_path="/tmp/lifecycle-refresh-native-profile.json"
pwsh -NoProfile -File ./scripts/Get-NativeRuntimeProfileEvidence.ps1 \
  -RuntimeProfile "$LIFECYCLE_REFRESH_PROFILE" > "$profile_evidence_path"
native_wrapper_sources="$(pwsh -NoProfile -Command "(Get-Content -LiteralPath '$profile_evidence_path' -Raw | ConvertFrom-Json).Sources | ConvertTo-Json -Compress" | tr -d '\r\n')"
native_wrapper_source_count="$(pwsh -NoProfile -Command "(Get-Content -LiteralPath '$profile_evidence_path' -Raw | ConvertFrom-Json).SourceCount" | tr -d '\r\n')"
native_abi_function_count="$(pwsh -NoProfile -Command "(Get-Content -LiteralPath '$profile_evidence_path' -Raw | ConvertFrom-Json).AbiFunctionCount" | tr -d '\r\n')"
test -n "$native_wrapper_sources"
test -n "$native_wrapper_source_count"
test -n "$native_abi_function_count"
grep -Fq "function-count=$native_abi_function_count" "$abi_manifest"

source_root="/workspace/opencv-source"
source_dir="$source_root/opencv-$LIFECYCLE_REFRESH_OPENCV_VERSION"
contrib_source_dir=""
mkdir -p "$source_root"
case "$source_dir" in
  /workspace/opencv-source/opencv-*) ;;
  *) echo "OpenCV source reset target escaped its dedicated root: $source_dir" >&2; exit 1 ;;
esac
if [ -e "$source_dir" ]; then
  rm -rf -- "$source_dir"
fi
git -c advice.detachedHead=false clone --depth 1 --branch "$LIFECYCLE_REFRESH_OPENCV_VERSION" \
  https://github.com/opencv/opencv.git "$source_dir"

with_contrib_argument=""
if [ "$LIFECYCLE_REFRESH_PROFILE" = "full" ]; then
  contrib_source_dir="$source_root/opencv_contrib-$LIFECYCLE_REFRESH_OPENCV_VERSION"
  case "$contrib_source_dir" in
    /workspace/opencv-source/opencv_contrib-*) ;;
    *) echo "OpenCV contrib reset target escaped its dedicated root: $contrib_source_dir" >&2; exit 1 ;;
  esac
  if [ -e "$contrib_source_dir" ]; then
    rm -rf -- "$contrib_source_dir"
  fi
  git -c advice.detachedHead=false clone --depth 1 --branch "$LIFECYCLE_REFRESH_OPENCV_VERSION" \
    https://github.com/opencv/opencv_contrib.git "$contrib_source_dir"
  with_contrib_argument="-WithContrib"
fi

plan_path="/tmp/lifecycle-refresh-opencv-plan.json"
# shellcheck disable=SC2086
pwsh -NoProfile -File ./scripts/Build-OpenCV.ps1 \
  -OpenCvVersion "$LIFECYCLE_REFRESH_OPENCV_VERSION" \
  -WorkspaceRoot /workspace \
  -Rid "$LIFECYCLE_REFRESH_RID" \
  -BuildList "$LIFECYCLE_REFRESH_BUILD_LIST" \
  -ExtraCMakeArgs "${LIFECYCLE_REFRESH_EXTRA_CMAKE_ARGS:-}" \
  -RuntimePackageMatrix "$candidate_matrix" \
  $with_contrib_argument \
  -DescribeOnly > "$plan_path"
opencv_cmake_arguments="$(pwsh -NoProfile -Command "(Get-Content -LiteralPath '$plan_path' -Raw | ConvertFrom-Json).CMakeArgs -join ' '" | tr -d '\r\n')"
test -n "$opencv_cmake_arguments"

# shellcheck disable=SC2086
pwsh -NoProfile -File ./scripts/Build-OpenCV.ps1 \
  -OpenCvVersion "$LIFECYCLE_REFRESH_OPENCV_VERSION" \
  -WorkspaceRoot /workspace \
  -Rid "$LIFECYCLE_REFRESH_RID" \
  -BuildList "$LIFECYCLE_REFRESH_BUILD_LIST" \
  -ExtraCMakeArgs "${LIFECYCLE_REFRESH_EXTRA_CMAKE_ARGS:-}" \
  -RuntimePackageMatrix "$candidate_matrix" \
  $with_contrib_argument \
  -Build

open_cv_build_dir="/workspace/artifacts/opencv-build/opencv-$LIFECYCLE_REFRESH_OPENCV_VERSION-$LIFECYCLE_REFRESH_RID"
open_cv_install_dir="/workspace/artifacts/opencv-install/opencv-$LIFECYCLE_REFRESH_OPENCV_VERSION-$LIFECYCLE_REFRESH_RID"
open_cv_cache="$open_cv_build_dir/CMakeCache.txt"
open_cv_cpu_config="$(find "$open_cv_build_dir" -type f -name cv_cpu_config.h -print -quit)"
test -f "$open_cv_cache"
test -n "$open_cv_cpu_config"
opencv_cpu_configuration="$({
  grep -E '^(CPU_BASELINE|CPU_DISPATCH|CV_DISABLE_OPTIMIZATION):' "$open_cv_cache" || true
  grep -E '^#define CV_CPU_(COMPILE|BASELINE|DISPATCH).*' "$open_cv_cpu_config" || true
} | tr '\n' ';')"
printf '%s\n' "$opencv_cpu_configuration" | grep -Eq 'SSE|AVX'

open_cv_dir=""
for candidate in \
  "$open_cv_install_dir/lib/cmake/opencv5" \
  "$open_cv_install_dir/lib64/cmake/opencv5"; do
  if [ -f "$candidate/OpenCVConfig.cmake" ]; then
    open_cv_dir="$candidate"
    break
  fi
done
test -n "$open_cv_dir"

rm -rf build/native-lifecycle-refresh
cmake \
  -S src/OpenCvSharp.Native \
  -B build/native-lifecycle-refresh \
  -G Ninja \
  "-DOPENCV_VERSION=$LIFECYCLE_REFRESH_OPENCV_VERSION" \
  "-DOPENCV_CSHARP_OPENCV_DIR=$open_cv_dir" \
  "-DOPENCV_CSHARP_OPENCV_BUILD_LIST=$LIFECYCLE_REFRESH_BUILD_LIST" \
  "-DOPENCV_CSHARP_RUNTIME_PROFILE=$LIFECYCLE_REFRESH_PROFILE"
cmake --build build/native-lifecycle-refresh --config Release
ctest --test-dir build/native-lifecycle-refresh -C Release --output-on-failure | tee build/native-lifecycle-refresh/ctest-output.txt
grep -Fq '100% tests passed, 0 tests failed out of 3' build/native-lifecycle-refresh/ctest-output.txt

open_cv_runtime_dir=""
for candidate in "$open_cv_install_dir/lib" "$open_cv_install_dir/lib64"; do
  if [ -d "$candidate" ]; then
    open_cv_runtime_dir="$candidate"
    break
  fi
done
test -n "$open_cv_runtime_dir"

audit_dir="build/native-lifecycle-refresh-audit"
rm -rf "$audit_dir"
mkdir -p "$audit_dir"
cp -a "$open_cv_runtime_dir"/libopencv_*.so* "$audit_dir/"
cp build/native-lifecycle-refresh/libJYPPX.OpenCV.Native.so "$audit_dir/"

actual_runtime_file_count=0
for path in "$audit_dir"/*; do
  test -e "$path" || test -L "$path"
  actual_runtime_file_count=$((actual_runtime_file_count + 1))
done
test "$actual_runtime_file_count" -eq "$expected_runtime_file_count"

opencv_elf_count=0
machine_count=0
origin_count=0
producer_path_count=0
missing_dependency_count=0
for elf in \
  "$audit_dir/libJYPPX.OpenCV.Native.so" \
  "$audit_dir"/libopencv_*.so."$LIFECYCLE_REFRESH_OPENCV_VERSION"; do
  test -f "$elf"
  case "$(basename "$elf")" in libopencv_*) opencv_elf_count=$((opencv_elf_count + 1)) ;; esac
  readelf -h "$elf" | grep -q 'Machine:.*X86-64'
  machine_count=$((machine_count + 1))
  dynamic_paths="$(readelf -d "$elf" | grep -E '\((RPATH|RUNPATH)\)' || true)"
  printf '%s\n' "$dynamic_paths" | grep -Fq '\$ORIGIN'
  origin_count=$((origin_count + 1))
  if printf '%s\n' "$dynamic_paths" | grep -Eq '(\[|:)(/[A-Za-z0-9]|[A-Za-z]:)'; then
    producer_path_count=$((producer_path_count + 1))
  fi
  missing="$(ldd "$elf" | grep 'not found' || true)"
  if [ -n "$missing" ]; then
    printf '%s\n' "$missing" >&2
    missing_dependency_count=$((missing_dependency_count + 1))
  fi
done
direct_opencv_dependencies="$(readelf -d "$audit_dir/libJYPPX.OpenCV.Native.so" | grep NEEDED | grep -c 'libopencv_.*so.500')"
test "$opencv_elf_count" -eq "$expected_opencv_count"
test "$machine_count" -eq "$expected_canonical_count"
test "$origin_count" -eq "$expected_canonical_count"
test "$producer_path_count" -eq 0
test "$direct_opencv_dependencies" -eq "$expected_direct_opencv"
test "$missing_dependency_count" -eq 0
elf_audit_evidence="LIFECYCLE_REFRESH_PRODUCER_ELF_EVIDENCE target=$LIFECYCLE_REFRESH_RID/$LIFECYCLE_REFRESH_PROFILE files=$expected_canonical_count runtime_files=$expected_runtime_file_count machine=X86-64 origin=$expected_canonical_count producer_paths=0 direct_opencv=$expected_direct_opencv missing_dependencies=0"
echo "$elf_audit_evidence"

source_patch_evidence="$(pwsh -NoProfile -File ./scripts/Apply-OpenCvSourcePatches.ps1 \
  -OpenCvSourceDir "$source_dir" \
  -OpenCvVersion "$LIFECYCLE_REFRESH_OPENCV_VERSION" | tail -n 1 | tr -d '\r')"
test -n "$source_patch_evidence"

pwsh -NoProfile -File ./scripts/New-RuntimeInputArtifact.ps1 \
  -Rid "$LIFECYCLE_REFRESH_RID" \
  -RuntimeProfile "$LIFECYCLE_REFRESH_PROFILE" \
  -OpenCvVersion "$LIFECYCLE_REFRESH_OPENCV_VERSION" \
  -NativeRuntimeDir ./build/native-lifecycle-refresh \
  -OpenCvRuntimeDir "$open_cv_runtime_dir" \
  -OpenCvSourceDir "$source_dir" \
  -OpenCvContribSourceDir "$contrib_source_dir" \
  -OpenCvInstallDir "$open_cv_install_dir" \
  -HostedRunner "${LIFECYCLE_REFRESH_HOSTED_RUNNER:-}" \
  -RunnerImage "${LIFECYCLE_REFRESH_RUNNER_IMAGE:-}" \
  -RunnerImageVersion "${LIFECYCLE_REFRESH_RUNNER_IMAGE_VERSION:-}" \
  -HostedDistro "$ID" \
  -HostedDistroVersion "$VERSION_ID" \
  -HostedArchitecture "$(uname -m)" \
  -HostedPackageArchitecture "$package_architecture" \
  -HostedLibc "$container_libc" \
  -HostedCpuModel "$cpu_model" \
  -HostedMemoryBytes "$memory_bytes" \
  -HostedDiskAvailableBytes "$disk_available_bytes" \
  -HostedProcessArchitecture "$process_architecture" \
  -CMakeVersion "$cmake_version" \
  -CMakeGenerator Ninja \
  -BuildConfiguration Release \
  -CompilerPath "$compiler_path" \
  -CompilerVersion "$compiler_version" \
  -AssemblerVersion "$assembler_version" \
  -NinjaVersion "$ninja_version" \
  -DotNetVersion "$LIFECYCLE_REFRESH_HOST_DOTNET_VERSION" \
  -OpenCvCMakeArguments "$opencv_cmake_arguments" \
  -ElfAuditEvidence "$elf_audit_evidence" \
  -OpenCvCpuConfiguration "$opencv_cpu_configuration" \
  -NativeWrapperSources "$native_wrapper_sources" \
  -NativeWrapperSourceCount "$native_wrapper_source_count" \
  -NativeAbiFunctionCount "$native_abi_function_count" \
  -ContainerImage "$LIFECYCLE_REFRESH_CONTAINER_IMAGE" \
  -ContainerImageId "$LIFECYCLE_REFRESH_CONTAINER_IMAGE_ID" \
  -ContainerImageDigest "$LIFECYCLE_REFRESH_CONTAINER_IMAGE_DIGEST" \
  -ContainerDistro "$ID" \
  -ContainerDistroVersion "$VERSION_ID" \
  -ContainerArchitecture "$(uname -m)" \
  -ContainerPackageArchitecture "$package_architecture" \
  -ContainerLibc "$container_libc" \
  -PowerShellVersion "$installed_powershell_version" \
  -PowerShellArchiveSha256 "$powershell_archive_sha256" \
  -OpenCvExtraCMakeArgs "${LIFECYCLE_REFRESH_EXTRA_CMAKE_ARGS:-}" \
  -OpenCvSourcePatchEvidence "$source_patch_evidence" \
  -OutputRoot artifacts/lifecycle-refresh-runtime-inputs \
  -RuntimePackageMatrix "$candidate_matrix"

echo "LIFECYCLE_REFRESH_CONTAINER_PRODUCER_OK target=$LIFECYCLE_REFRESH_RID/$LIFECYCLE_REFRESH_PROFILE publication_allowed=false"

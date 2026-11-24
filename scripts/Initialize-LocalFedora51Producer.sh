#!/bin/sh
set -eu
. /etc/os-release
test "$ID:$VERSION_ID:$(uname -m)" = fedora:44:x86_64
if [ "${FEDORA_51_USE_CACHE:-0}" = 1 ]; then set -- --cacheonly; else set --; fi
dnf "$@" -y --setopt=timeout=30 --setopt=retries=2 --setopt=install_weak_deps=False install binutils ca-certificates cmake diffutils eigen3-devel findutils gcc-c++ git gzip libicu libunwind ninja-build openssl-libs python3 tar util-linux which zlib-ng-compat
printf '%s  %s\n' dcfe6060fc86abcb859ce1f8f80843ce50bab0585396de56380ed9f25176ac6d /toolchain/powershell.tar.gz | sha256sum -c -
mkdir -p /opt/powershell /workspace/opencv-source /workspace/output
tar -xzf /toolchain/powershell.tar.gz -C /opt/powershell
chmod +x /opt/powershell/pwsh
ln -s /opt/powershell/pwsh /usr/local/bin/pwsh
git -c advice.detachedHead=false clone --shared --no-checkout /repo-input /workspace/repo
git -C /workspace/repo checkout --detach b7f8180c3949aa341bbdf2be97432fb08d1e85e2
for tree in opencv-5.1.0 opencv_contrib-5.1.0; do
  git -c advice.detachedHead=false clone --shared --no-checkout "/upstream-input/$tree" "/workspace/opencv-source/$tree"
done
git -C /workspace/opencv-source/opencv-5.1.0 checkout --detach e4f78fbcf1a773fbed69e14500e8b734963eb585
git -C /workspace/opencv-source/opencv_contrib-5.1.0 checkout --detach b67a855188a3d683813731010f6d39ac614733a4
rpm -qa --qf '%{NAME}|%{VERSION}-%{RELEASE}|%{ARCH}\n' | sort > /workspace/output/installed-packages.txt
pwsh -NoProfile -Command '$PSVersionTable.PSVersion.ToString()'
getconf GNU_LIBC_VERSION
echo FEDORA_51_PRODUCER_READY

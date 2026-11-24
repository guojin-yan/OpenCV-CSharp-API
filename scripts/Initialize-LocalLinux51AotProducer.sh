#!/bin/sh
set -eu
. /etc/os-release
test "$ID:$VERSION_ID:$(uname -m)" = ubuntu:22.04:x86_64
if [ ! -s /etc/ssl/certs/ca-certificates.crt ]; then
  mkdir -p /etc/ssl/certs
  cp /kit/toolchain/ubuntu-ca-certificates.crt /etc/ssl/certs/ca-certificates.crt
fi
sed -i 's|http://archive.ubuntu.com|https://archive.ubuntu.com|g;s|http://security.ubuntu.com|https://security.ubuntu.com|g' /etc/apt/sources.list
export DEBIAN_FRONTEND=noninteractive
apt-get -o Acquire::Retries=3 update
apt-get -o Acquire::Retries=3 install -y --no-install-recommends ca-certificates clang build-essential zlib1g-dev binutils libicu70 libssl3 libunwind8 libgssapi-krb5-2 tar gzip
printf '%s  %s\n' dcfe6060fc86abcb859ce1f8f80843ce50bab0585396de56380ed9f25176ac6d /kit/toolchain/powershell.tar.gz | sha256sum -c -
printf '%s  %s\n' 51c8b999af9e8dd9998c9edc5944e19a90788862068acd38694e098889054ce8c23d4f0c5cccfa16bf187d044562359e5ee69a9f8ad0bbe913ba90311fbce25b /kit/toolchain/dotnet.tar.gz | sha512sum -c -
mkdir -p /opt/dotnet /opt/powershell /producer
cp -a /kit/source /producer/source
cp -a /kit/scripts /producer/scripts
cp -a /kit/packages /producer/feed
tar -xzf /kit/toolchain/dotnet.tar.gz -C /opt/dotnet
tar -xzf /kit/toolchain/powershell.tar.gz -C /opt/powershell
chmod +x /opt/powershell/pwsh
ln -s /opt/powershell/pwsh /usr/local/bin/pwsh
ln -s /opt/dotnet/dotnet /usr/local/bin/dotnet
echo LOCAL_LINUX_51_AOT_PRODUCER_READY

# Runtime Licenses

Runtime packages include this project's license, the OpenCV license, and third-party license files exported by the OpenCV install tree.

runtime 包包含本项目许可证、OpenCV 许可证，以及 OpenCV 安装树导出的第三方许可证文件。

The runtime package license expression is `Apache-2.0`: this project's native wrapper pieces and the OpenCV runtime are both covered by Apache-2.0.

runtime 包的 license expression 为 `Apache-2.0`：本项目 native wrapper 与 OpenCV runtime 均使用 Apache-2.0 许可。

## Staging Rule

`scripts/Stage-Runtime.ps1` copies OpenCV runtime binaries and license files into the runtime package project.

The script prints the copied runtime DLL names so CI logs can be used as packaging evidence.

脚本会打印已复制的 runtime DLL 名称，便于在 CI 日志中核对打包内容。

License source inputs are the repository `LICENSE`, the OpenCV source `LICENSE`, the OpenCV source `3rdparty/ippicv/readme.htm`, and third-party license files from the OpenCV install `etc/licenses` directory.

许可证源输入包括仓库 `LICENSE`、OpenCV 源码 `LICENSE`、OpenCV 源码 `3rdparty/ippicv/readme.htm`，以及 OpenCV install `etc/licenses` 目录中的第三方许可证文件。

Generated package license layout lives under `licenses/`; OpenCV third-party license files are copied into `licenses/opencv-3rdparty`. Full runtime package IDs are `JYPPX.OpenCV.runtime.<rid>` and mini runtime package IDs are `JYPPX.OpenCV.runtime.<rid>.mini`.

生成包的 license 布局位于 `licenses/`；OpenCV 第三方许可证文件会复制到 `licenses/opencv-3rdparty`。full runtime package ID 为 `JYPPX.OpenCV.runtime.<rid>`，mini runtime package ID 为 `JYPPX.OpenCV.runtime.<rid>.mini`。

Runtime package template project: `packaging/runtime/JYPPX.OpenCV.runtime`. Full and mini runtime packages keep the same generated license layout for every configured RID/profile.

runtime package 模板项目为 `packaging/runtime/JYPPX.OpenCV.runtime`。full 与 mini runtime packages 会为每个配置好的 RID/profile 保持相同的生成 license 布局。

For OpenCV install-tree third-party licenses, the script looks for:

对于 OpenCV install tree 的第三方许可证，脚本会查找：

```text
<OpenCV install dir>/etc/licenses
```

and stages those third-party license files under:

并将这些第三方许可证文件暂存到：

```text
packaging/runtime/JYPPX.OpenCV.runtime.<rid>/licenses/opencv-3rdparty
```

That generic path resolves inside the template project to `packaging/runtime/JYPPX.OpenCV.runtime/licenses/opencv-3rdparty` for the selected staged RID/profile.

For package-local license layout and source-input details, see the `packaging/runtime/JYPPX.OpenCV.runtime/README.md`.

包内 license 布局和源输入细节见`packaging/runtime/JYPPX.OpenCV.runtime/README.md`。

Consumer install and package selection start in the [Quick Start](quick-start.md). Local native runtime fallback and staging are covered by the [Linked Runtime Build Guide](linked-runtime-build-guide.md), linked validation is covered by the [Linked Runtime Smoke Guide](linked-runtime-smoke-guide.md), and package-local metadata is documented in the [runtime package README](https://github.com/guojin-yan/OpenCV-CSharp-API/blob/opencv5.x/packaging/runtime/JYPPX.OpenCV.runtime/README.md).

consumer 安装和 package 选择从 [Quick Start](quick-start.md) 开始。local native runtime fallback 与 staging 见 [Linked Runtime Build Guide](linked-runtime-build-guide.md)，linked 验证见 [Linked Runtime Smoke Guide](linked-runtime-smoke-guide.md)，包内元数据见[runtime package README](https://github.com/guojin-yan/OpenCV-CSharp-API/blob/opencv5.x/packaging/runtime/JYPPX.OpenCV.runtime/README.md)。

## Image Codec Dependencies

For the current Windows x64 OpenCV 5.0.0 build, image codec related third-party licenses include files for:

- libjpeg-turbo
- libpng
- libtiff
- libopenjp2
- zlib

The exact list is determined by the OpenCV build configuration. CI packaging should always stage from the produced OpenCV install tree instead of relying on a hard-coded list.

具体列表由 OpenCV 构建配置决定。CI 打包时应始终从产出的 OpenCV install tree 暂存许可证，而不是依赖硬编码列表。

## OpenCV 5.1 Linux candidate supplements

The retained Ubuntu 22.04 build's Ninja graph, valid compiler dependency records and actual static archive members identify 15 third-party components across the 17 upstream modules. The new [local supplement evidence](https://github.com/guojin-yan/OpenCV-CSharp-API/blob/opencv5.x/packaging/release/opencv-5.1.0-linux-thirdparty-evidence.json) records eight archives, 1,201 compiled objects, 3,079 observed input files and 2,054 checked official Git blobs. It also preserves 1,920 leading source notices. ADE is excluded because no linked archive or compiler input uses it in this build. System shared libraries remain distribution dependencies.

The four isolated preview/headless Full/Mini candidates add original component licenses, the Independent JPEG Group acknowledgement, leading source notices and all 343 Eigen headers observed by the compiler. Their native payloads match the original packages byte for byte. Full includes 15 named components; Mini includes the ten reached by its six upstream modules. Each package has an SPDX 2.3 inventory containing every ZIP entry and build-derived component relationships. The NuGet license metadata points to `THIRD_PARTY_NOTICES.txt`; the original OpenCV/contrib license files are retained under `licenses/`.

The components are Eigen 3.4.0, annoy, FLANN 1.6.10 OpenCV fork, FlatBuffers 25.9.23, HarfBuzz 14.2.1, libjpeg-turbo 3.1.2, libpng 1.6.57, libtiff 4.7.2, libwebp, MLAS, MSCR, OpenJPEG 2.5.4, Protobuf 3.19.1, SoftFloat/fdlibm and zlib 1.3.2. Versions without a vendored release declaration retain their source revision. License declarations are preliminary; libtiff remains `NOASSERTION` with its original license text. The shared leading-notice file also contains notices from source units outside an individual Mini/headless profile.

Reproduce collection with `Collect-Linux51ThirdPartyBuild.py` against the retained producer, then `New-Linux51ThirdPartyCandidates.py` against the original packages. The collector verifies archive member bytes against actual objects, compiler dependency timestamps, official source blobs and the installed ELF's RUNPATH-only transformation. `Invoke-Linux51ThirdPartyConsumer.ps1` checks every restored native/notice/source file in independent package caches. `Measure-Linux51ThirdPartyEvidence.py` writes the package-bound records and SPDX documents; `Test-Linux51ThirdPartyEvidence.ps1 -RequireArtifacts` verifies retained artifacts and rejects 28 negative evidence fixtures.

Ubuntu 22.04, Ubuntu 24.04 and Debian 12 complete 24 package-only executions over net8.0/net10.0, preview/headless and Full/Mini. PNG pixels, three-frame MJPG files, Full ONNX inference, Mini importer omission and headless GUI-failure recovery pass. The candidate versions are `5.1.0-licenses.53a4b3d` and `5.1.0-headless-licenses.53a4b3d`, using the existing preview package IDs. These local supplements do not update the active package matrix. Complete source-level license review, vulnerability assessment, final release SBOM promotion, hosted reproduction and publication remain open.

本轮根据实际编译与链接输入补齐四份 Linux 候选包的第三方原文、JPEG 致谢及 Eigen 对应源，原生库字节保持不变。三发行版 24 组真实还原、编译与运行全部通过；该证据不代表完整源码许可证审查、漏洞审查或正式发布已经完成。

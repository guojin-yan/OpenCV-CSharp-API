# Runtime Licenses

Runtime packages include this project's license, the OpenCV license, and third-party license files exported by the OpenCV install tree.

runtime 包包含本项目许可证、OpenCV 许可证，以及 OpenCV 安装树导出的第三方许可证文件。

The runtime package license expression is `MIT AND Apache-2.0`: this project's native wrapper pieces are covered by MIT, and the OpenCV runtime is covered by Apache-2.0.

runtime 包的 license expression 为 `MIT AND Apache-2.0`：本项目 native wrapper 使用 MIT 许可，OpenCV runtime 使用 Apache-2.0 许可。

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

Consumer install and package selection start in the [Quick Start](quick-start.md). Local native runtime fallback and staging are covered by the [Linked Runtime Build Guide](linked-runtime-build-guide.md), linked validation is covered by the [Linked Runtime Smoke Guide](linked-runtime-smoke-guide.md), and package-local metadata is documented in the [runtime package README](../../packaging/runtime/JYPPX.OpenCV.runtime/README.md).

consumer 安装和 package 选择从 [Quick Start](quick-start.md) 开始。local native runtime fallback 与 staging 见 [Linked Runtime Build Guide](linked-runtime-build-guide.md)，linked 验证见 [Linked Runtime Smoke Guide](linked-runtime-smoke-guide.md)，包内元数据见[runtime package README](../../packaging/runtime/JYPPX.OpenCV.runtime/README.md)。

## Image Codec Dependencies

For the current Windows x64 OpenCV 5.0.0 build, image codec related third-party licenses include files for:

- libjpeg-turbo
- libpng
- libtiff
- libopenjp2
- zlib

The exact list is determined by the OpenCV build configuration. CI packaging should always stage from the produced OpenCV install tree instead of relying on a hard-coded list.

具体列表由 OpenCV 构建配置决定。CI 打包时应始终从产出的 OpenCV install tree 暂存许可证，而不是依赖硬编码列表。

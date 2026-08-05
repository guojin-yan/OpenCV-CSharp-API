# Sample Model Assets / 案例模型资产

The model-backed examples use an auditable asset manifest instead of floating download links. Every model, label file, image, and license is pinned by source commit, byte length, and SHA-256 in `samples/assets/models/model-assets.json`.

模型案例不使用会随时间变化的下载链接。每个模型、标签文件、输入图片和许可证都在 `samples/assets/models/model-assets.json` 中固定来源提交、文件长度和 SHA-256。

## Download / 下载

Download only the bundle needed by the example:

```powershell
pwsh .\scripts\Get-SampleModelAssets.ps1 -Bundle classification-mobilenet-v2
pwsh .\scripts\Get-SampleModelAssets.ps1 -Bundle detection-nanodet
pwsh .\scripts\Get-SampleModelAssets.ps1 -Bundle segmentation-pphumanseg
```

只下载当前案例所需的 bundle。默认缓存目录是 `samples/assets/models/cache`，该目录被 Git 忽略，模型二进制不会进入源码仓库。

To use another cache location, pass `-OutputRoot` while downloading and pass the same path as the second sample argument, or set `OPENCV_CSHARP_SAMPLE_ASSET_ROOT`.

如需使用其他缓存目录，下载时传入 `-OutputRoot`，运行案例时把同一路径作为第二个参数，或者设置 `OPENCV_CSHARP_SAMPLE_ASSET_ROOT`。

## Provenance / 来源

| Bundle | Model | Source | License |
|---|---|---|---|
| `classification-mobilenet-v2` | MobileNetV2 ImageNet-1K | OpenCV Zoo commit `47534e27...` | Apache-2.0 |
| `detection-nanodet` | NanoDet object detection | OpenCV Zoo commit `47534e27...` | Apache-2.0 |
| `segmentation-pphumanseg` | PPHumanSeg portrait segmentation | OpenCV Zoo commit `47534e27...` | Apache-2.0 |

The OpenCV Zoo Git repository stores large models through Git LFS. The manifest records the immutable Zoo source commit and uses the official `opencv/*` Hugging Face repositories as the transport. The downloaded content hash must still equal the Zoo LFS object hash. The input image and common license are pinned to OpenCV commit `40738fb1...`.

OpenCV Zoo 通过 Git LFS 保存大模型。清单同时记录不可变的 Zoo 源提交，并使用 OpenCV 官方 `opencv/*` Hugging Face 仓库传输文件；下载内容仍必须与 Zoo LFS 对象哈希一致。输入图片和公共许可证固定到 OpenCV 提交 `40738fb1...`。

## Verification / 校验

The downloader validates containment, size, and SHA-256 before moving a temporary file into the cache. Each sample repeats the size and hash checks at runtime, so replacing a cached model silently is not possible.

下载器先校验路径边界、长度和 SHA-256，再把临时文件移动到缓存。每个案例在运行时会再次检查文件长度和哈希，因此缓存模型被替换后不会静默执行。

```powershell
pwsh .\scripts\Test-SampleModelAssetContract.ps1
pwsh .\scripts\Test-SampleModelAssetContract.ps1 -VerifyCache
```

The contract guard also rejects floating branches, non-HTTPS URLs, unknown repositories, missing license references, duplicate paths, tracked cache files, and credential handling in the downloader.

契约守卫还会拒绝浮动分支、非 HTTPS 地址、未知仓库、缺失许可证、重复路径、被 Git 跟踪的缓存文件，以及下载器中的凭据处理逻辑。

See [MobileNetV2 Classification](tutorial-18-mobilenet-classification.md), [NanoDet Detection](tutorial-19-nanodet-object-detection.md), and [PPHumanSeg Segmentation](tutorial-20-pphumanseg-segmentation.md).

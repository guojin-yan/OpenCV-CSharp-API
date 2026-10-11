# WeChat QRCode Upstream Parity Guide

This guide records the scoped OpenCV 5.0.0 contrib `wechat_qrcode` evidence map. It is a source-scope and wrapper-boundary record, not a claim of repository-wide upstream parity.

## Scope

The map covers the parser-emitted declarations from `opencv-source/opencv_contrib-5.0.0/modules/wechat_qrcode/include/opencv2/wechat_qrcode.hpp`: the `cv.wechat_qrcode.WeChatQRCode` class metadata row, its constructor, `detectAndDecode`, `setScaleFactor`, and `getScaleFactor`.

The pinned closure contains four callable declarations and one class metadata declaration. The callable rows are intentionally omitted: the current package has no WeChatQRCode native ABI entrypoint or managed public owner.

## Model and ownership boundary

OpenCV's implementation requires caller-selected detector and super-resolution ONNX model files. The repository bundles neither model asset, backend selection policy, nor a managed decoded-string ownership contract. Those dependencies are recorded in `wechat_qrcode-upstream-classifications.json` and are not converted into an unverified wrapper claim.

## Verification

Run the focused guard from the repository root:

```powershell
pwsh -NoProfile -File .\scripts\Test-WechatQRCodeUpstreamMap.ps1
```

The guard verifies the pinned header/parser hashes, five-declaration ordinal closure, four intentional omissions, zero missing declarations, zero native/managed evidence, the model dependency note, and the prohibition on repository-wide parity claims.

中文说明：本映射只覆盖固定的 `wechat_qrcode.hpp` parser 声明范围。由于当前包没有 WeChatQRCode 的 native ABI 或 managed owner，四个 callable 均明确标记为 intentionally-omitted；探测器/超分 ONNX 模型由调用方提供，仓库不宣称已封装或已验证模型执行。

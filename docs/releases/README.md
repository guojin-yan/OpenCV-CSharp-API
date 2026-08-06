# Release Notes Index / 版本说明索引

Every package iteration has one immutable detailed note in this directory. The newest entry appears first in the root [`CHANGELOG.md`](https://github.com/guojin-yan/OpenCV-CSharp-API/blob/opencv5.x/CHANGELOG.md) and is summarized in both root README files.

每次包版本迭代都必须在本目录保留一个独立的详细说明文件。最新版本同时位于根目录 [`CHANGELOG.md`](https://github.com/guojin-yan/OpenCV-CSharp-API/blob/opencv5.x/CHANGELOG.md) 首位，并在中英文 README 中提供简要摘要。

| Version / 版本 | Status / 状态 | Detailed notes / 详细说明 |
| --- | --- | --- |
| `5.0.0` | Stable / 稳定版 | [5.0.0](5.0.0.md) |
| `5.0.0-preview.1` | Published / 已发布 | [5.0.0-preview.1](5.0.0-preview.1.md) |

## Maintenance Rule / 维护规则

For every package-visible iteration:

1. Add `docs/releases/<normalized-version>.md` from the same change set.
2. Add the version to this index and to the top of `CHANGELOG.md`.
3. Replace the short current-update section in `README.md` and `README_cn.md`.
4. Record managed API, native ABI/runtime, compatibility/migration, and validation impact explicitly.
5. Do not mark an entry published until NuGet.org, GitHub Packages, and the matching GitHub Release are verified.

每次影响包使用者的版本迭代，都必须在同一变更中新增详细说明、更新两个索引和双语 README 摘要，并明确记录托管 API、native ABI/runtime、迁移影响与验证结果。只有 NuGet.org、GitHub Packages 和对应 GitHub Release 全部核验完成后，才能将状态改为“已发布”。

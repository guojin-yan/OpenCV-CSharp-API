# Published Package Samples

These projects consume the public package fixture defined in `PublishedPackageSamples.props` instead of a project reference:

- `JYPPX.OpenCV.CSharp.API`
- `JYPPX.OpenCV.runtime.win-x64`

Each case has its own executable, complete processing pipeline, and output directory. The only shared code is small deterministic fixture infrastructure for input generation, font discovery, PNG writing, and package metadata; the OpenCV algorithm chain is implemented in each case's `Program.cs`. The fixture version is intentionally pinned for reproducible release validation; normal applications should use the unpinned `dotnet add package ... --prerelease` commands in the main README.

| Case | Project | Module workflow |
|---|---|---|
| 01 | `Case01.ImagePipeline` | Core, ImgProc, ImgCodecs |
| 02 | `Case02.ChinesePutText` | ImgProc `FontFace` and `putText` |
| 03 | `Case03.Contours` | ImgProc threshold and contours |
| 04 | `Case04.OrbFeatures` | Features2D ORB |
| 05 | `Case05.TemplateMatching` | ImgProc template response |
| 06 | `Case06.KnnClassification` | ML KNN, or explicit `NOT_LINKED` capability output |

## Run One Case

```powershell
$env:OPENCV_CSHARP_CJK_FONT = "C:\Windows\Fonts\Deng.ttf"
dotnet run --project .\samples\PublishedPackageSamples\Case01.ImagePipeline\Case01.ImagePipeline.csproj -c Release -- .\artifacts\published-package-image
dotnet run --project .\samples\PublishedPackageSamples\Case02.ChinesePutText\Case02.ChinesePutText.csproj -c Release -- .\artifacts\published-package-text C:\Windows\Fonts\Deng.ttf
dotnet run --project .\samples\PublishedPackageSamples\Case03.Contours\Case03.Contours.csproj -c Release -- .\artifacts\published-package-contours
dotnet run --project .\samples\PublishedPackageSamples\Case04.OrbFeatures\Case04.OrbFeatures.csproj -c Release -- .\artifacts\published-package-features
dotnet run --project .\samples\PublishedPackageSamples\Case05.TemplateMatching\Case05.TemplateMatching.csproj -c Release -- .\artifacts\published-package-template
dotnet run --project .\samples\PublishedPackageSamples\Case06.KnnClassification\Case06.KnnClassification.csproj -c Release -- .\artifacts\published-package-ml
```

Each command runs a complete feature implementation and writes one focused PNG plus a summary containing the package fixture and native OpenCV versions. The Chinese text case accepts a TTF/TTC path as its second argument; the KNN case writes `NOT_LINKED` when the selected runtime was built without the optional ML module.

The root `PublishedPackageSamples` project remains as an optional aggregate gallery command for release regression checks:

```powershell
dotnet run --project .\samples\PublishedPackageSamples\PublishedPackageSamples.csproj -c Release -- tutorial all .\artifacts\published-package-tutorials C:\Windows\Fonts\Deng.ttf
```

# Native Module Boundary / Native 模块边界

The first native wrapper scope is intentionally small:

第一批 native 封装范围保持克制：

| Module | First APIs | Notes |
| --- | --- | --- |
| core | version, error, `Mat`, `Point`, `Size`, `Rect`, `Scalar` | ABI and lifetime model first |
| imgcodecs | `imencode`, `imdecode` | Uses opaque encoded buffer handles; file APIs are later |
| imgproc | `cvtColor`, resize, threshold, blur, morphology | Requires array and enum mapping |
| features | `ORB`, `SIFT`, `FAST`, `GFTT`, `MSER`, `SimpleBlobDetector`, matcher objects, draw helpers | Optional OpenCV target; exported ABI returns `NOT_LINKED` when unavailable |
| xfeatures2d | `BRISK`, `KAZE`, `AKAZE`, typed `AffineFeature` bridges | Optional contrib target layered on top of `features`; exported ABI returns `NOT_LINKED` when unavailable |
| highgui | named windows, image display, wait/poll key, move/resize, window properties, title/image rectangle, trackbar/mouse/button callbacks | Optional or platform-gated; default tests do not create windows |
| videoio | `VideoCapture`, `VideoWriter`, backend names, backend registry, FourCC, capture/writer properties | Runtime dependency and codec-backend sensitive |
| video | Lucas-Kanade/Farneback optical flow, optical-flow pyramid, mean-shift/CamShift, background subtraction, `KalmanFilter` | Requires `opencv_video`; exported ABI returns `NOT_LINKED` when unavailable |
| dnn | `Net`, model path/buffer loading, input, single/multi-output forward, layer names/metadata, profile/FLOPS, blob helpers | Requires `opencv_dnn`; real forward depends on user-supplied model files |
| stitching | `Stitcher`, mode/status, properties, stitch/estimate/compose, component/camera/result-mask output | Requires `opencv_stitching`; real success depends on image overlap and feature quality |
| objdetect | `QRCodeDetector`, `BarcodeDetector`, `QRCodeDetectorAruco`, `QRCodeEncoder`, ArUco dictionary/detector/grid board/ChArUco, MCC checker detector/checker, `FaceDetectorYN`, `FaceRecognizerSF` | Main OpenCV object-detection surface; face and future MCC DNN workflows require OpenCV DNN and external model files |
| photo | `Inpaint`, single-frame and multi-frame fast NLM denoise, decolor, seamless/editing, edge-preserving/sketch/stylization, tonemap/HDR objects, CPU CCM, and `IntelligentScissorsMB` | Main OpenCV photo surface; HDR, correction, contour, and editing behavior depends on input type, range, and state |
| calib | full camera calibration and stereo calibration | Current OpenCV 5.0.0 split calibration module, staged as the factual OpenCV 5.0.0 runtime artifact `opencv_calib500.dll` |
| xobjdetect | `CascadeClassifier`, `HOGDescriptor` | Optional contrib module; exported ABI returns `NOT_LINKED` when unavailable |
| ptcloud | depth/RGB-D functions and `RgbdNormals` | Main OpenCV module; depth behavior depends on intrinsics, depth units, and input type |
| quality | `QualityMSE`, `QualityPSNR`, `QualitySSIM`, `QualityGMSD`, `QualityBRISQUE` | Optional contrib module; BRISQUE also needs `opencv_ml` and user model/range files |
| xphoto | white balancers, channel gains, DCT/BM3D denoising, oil painting | Optional contrib module; algorithms are sensitive to channel count, depth, and parameter range |
| ml | `TrainData`, `ParamGrid`, `KNearest`, `SVM`, `SVMSGD`, `LogisticRegression`, `NormalBayesClassifier`, `EM`, `DTrees`, `RTrees`, `Boost`, `ANN_MLP` | Local OpenCV 5.0.0 exposes this through contrib; samples usually use `CV_32F` data and runtime needs the factual OpenCV 5.0.0 runtime artifact `opencv_ml500.dll` |
| img_hash | `ImgHashBase`, average/pHash/block mean/color moment/Marr-Hildreth/radial variance hashes | Optional contrib module; runtime needs the factual OpenCV 5.0.0 runtime artifact `opencv_img_hash500.dll`; compare semantics differ by algorithm |
| ximgproc | local thresholding, thinning, edge-aware filters, guided/FGS/FBS objects, SLIC/SEEDS/LSC superpixels, FastLineDetector, disparity WLS helpers, sparse match interpolation, EdgeDrawing, EdgeBoxes, ridge/gradient utilities, Fourier descriptors, run-length morphology, ScanSegment, GraphSegmentation, Selective Search, covariance estimation | Optional contrib module; runtime needs the factual OpenCV 5.0.0 runtime artifact `opencv_ximgproc500.dll`; tiny smoke checks call paths and output shape |
| optflow | dense/sparse optical-flow bases, `DualTVL1OpticalFlow`, RLOF parameters/flows, SimpleFlow/SparseToDense/RLOF helpers, motion templates | Optional contrib module; runtime needs the factual OpenCV 5.0.0 runtime artifact `opencv_optflow500.dll`; several algorithms can use the staged factual OpenCV 5.0.0 runtime artifact `opencv_ximgproc500.dll` module |
| bgsegm | `BackgroundSubtractorMOG`, `BackgroundSubtractorGMG`, `BackgroundSubtractorCNT`, `SyntheticSequenceGenerator` | Optional contrib module; runtime needs the factual OpenCV 5.0.0 runtime artifact `opencv_bgsegm500.dll`; useful models require multi-frame sequences |
| tracking | modern `TrackerKCF`/`TrackerCSRT`, legacy MOSSE/MIL/MedianFlow, `MultiTracker` | Optional contrib module; runtime needs the factual OpenCV 5.0.0 runtime artifact `opencv_tracking500.dll`; tiny smoke checks call paths, not tracking quality |
| face | `FaceRecognizer`, `BasicFaceRecognizer`, `EigenFaceRecognizer`, `FisherFaceRecognizer`, `LBPHFaceRecognizer`, `StandardCollector`, `BIF`, `FacemarkLBF`, `MACE` | Optional contrib module; runtime needs the factual OpenCV 5.0.0 runtime artifact `opencv_face500.dll`; facemark real fitting needs caller model/training data |
| saliency | `StaticSaliencySpectralResidual`, `StaticSaliencyFineGrained`, `MotionSaliencyBinWangApr2014`, `ObjectnessBING` | Optional contrib module; runtime needs the factual OpenCV 5.0.0 runtime artifact `opencv_saliency500.dll`; BING real proposals need caller training data |
| plot | `Plot2d` create/setter/render APIs | Optional contrib module; runtime needs the factual OpenCV 5.0.0 runtime artifact `opencv_plot500.dll`; renders caller-owned `Mat` output |
| shape | `EMDL1`, histogram cost extractors, shape-context and Hausdorff distance extractors | Optional contrib module; runtime needs the factual OpenCV 5.0.0 runtime artifact `opencv_shape500.dll`; tiny smoke checks generated signatures/descriptors/contours only |
| line_descriptor | `KeyLine`, `BinaryDescriptor`, `BinaryDescriptorMatcher`, draw helpers | Optional contrib module; runtime needs the factual OpenCV 5.0.0 runtime artifact `opencv_line_descriptor500.dll`; tiny smoke checks generated line images only |
| phase_unwrapping | `HistogramPhaseUnwrapping`, unwrap, inverse reliability map | Optional contrib module; runtime needs the factual OpenCV 5.0.0 runtime artifact `opencv_phase_unwrapping500.dll`; tiny smoke checks generated `CV_32FC1` phase maps only |
| structured_light | `GrayCodePattern`, `SinusoidalPattern`, pattern generation, shadow-mask images, selected phase helpers | Optional contrib module; runtime needs the factual OpenCV 5.0.0 runtime artifact `opencv_structured_light500.dll`; real projector-camera workflows need caller-captured images |
| intensity_transform | log transform, gamma correction, autoscaling, contrast stretching, BIMEF | Optional contrib module; runtime needs the factual OpenCV 5.0.0 runtime artifact `opencv_intensity_transform500.dll`; BIMEF also depends on OpenCV EIGEN support |
| fuzzy | kernel creation, inpaint, filter, F0/F1 transform helpers | Optional contrib module; runtime needs the factual OpenCV 5.0.0 runtime artifact `opencv_fuzzy500.dll`; tiny smoke checks call paths and output shape |
| hfs | `HfsSegment`, parameter get/set, CPU/GPU segmentation | Optional contrib module; runtime needs the factual OpenCV 5.0.0 runtime artifact `opencv_hfs500.dll`; default smoke uses CPU segmentation only |
| reg | registration maps and gradient mappers | Optional contrib module; runtime needs the factual OpenCV 5.0.0 runtime artifact `opencv_reg500.dll`; maps and mappers stay behind opaque handles |
| surface_matching | ICP and PPF 3D detector | Optional contrib module; runtime needs the factual OpenCV 5.0.0 runtime artifact `opencv_surface_matching500.dll`; pose vectors stay inside native and are flattened for managed code |
| rapid | RAPID helper calls and silhouette trackers | Optional contrib module; runtime needs the factual OpenCV 5.0.0 runtime artifact `opencv_rapid500.dll`; tiny smoke checks generated mesh/edge call paths |
| alphamat | information-flow alpha matting | Optional contrib module; runtime needs the factual OpenCV 5.0.0 runtime artifact `opencv_alphamat500.dll`; tiny smoke checks generated image/trimap output shape |
| bioinspired | Retina, fast tone mapping, transient segmentation | Optional contrib module; runtime needs the factual OpenCV 5.0.0 runtime artifact `opencv_bioinspired500.dll`; model objects stay behind opaque handles; algorithm smoke is unstable opt-in |
| xstereo | census descriptors, binary stereo matchers, quasi-dense stereo | Optional contrib module; runtime needs the factual OpenCV 5.0.0 runtime artifact `opencv_xstereo500.dll`; match vectors use count/fill flat structs |

Version naming note:

版本命名说明：

- `JYPPX.OpenCV.Native.dll` is the primary version-neutral loader selected by current managed interop declarations.
- `JYPPX.OpenCV.Native.dll`, `jyppx_ocv_*`, and `OPENCV_CSHARP_STATUS_*` are the primary native loader, ABI, and status names.
- `OpenCv5Sharp.Native.dll`, `jyppx_ocv5_*`, and `OPENCV5SHARP_STATUS_*` remain existing-binary compatibility contracts for already-compiled managed and native consumers.
- `OPENCV5SHARP_STATUS_NOT_LINKED` remains an alias to `OPENCV_CSHARP_STATUS_NOT_LINKED`.
- New generic build or documentation concepts should use version-neutral names and reserve `OpenCV 5.0.0` for current packaged runtime identity or factual upstream runtime artifacts.

- `JYPPX.OpenCV.Native.dll` 是当前 managed interop 声明选择的版本中立主 loader。
- `JYPPX.OpenCV.Native.dll`、`jyppx_ocv_*` 与 `OPENCV_CSHARP_STATUS_*` 是主 native loader、ABI 和状态名称。
- `OpenCv5Sharp.Native.dll`、`jyppx_ocv5_*` 与 `OPENCV5SHARP_STATUS_*` 继续作为供已编译 managed 和 native 消费者使用的既有二进制兼容契约保留。
- `OPENCV5SHARP_STATUS_NOT_LINKED` 继续作为 `OPENCV_CSHARP_STATUS_NOT_LINKED` 的别名。
- 新增的通用构建或文档概念应使用版本中立名称，只在描述当前打包 runtime 身份或事实性上游 runtime 产物时使用 `OpenCV 5.0.0`。

Rules:

- Primary C exports use the `jyppx_ocv_` prefix.
- Generated wrappers preserve matching `jyppx_ocv5_*` exports for already-compiled binaries.
- C++ objects are exposed as opaque handles.
- C++ exceptions are caught at the native boundary.
- STL containers never cross the C ABI.
- UTF-8 is the default string encoding.

## CMake Target/Export Boundary / CMake Target/Export 边界

The native CMake project is currently source-tree build only. It builds the primary `JYPPX.OpenCV.Native` target for local wrapper builds and does not install or export a reusable CMake package or SDK target today. The `OpenCv5Sharp.Native` CMake target name is only a compatibility alias to the primary target for existing build scripts and loaders.

native CMake 项目当前只作为 source-tree build surface 使用。它为本地 wrapper build 构建主 `JYPPX.OpenCV.Native` target，当前不 install 或 export 可复用的 CMake package / SDK target。`OpenCv5Sharp.Native` CMake target name 仅作为指向主目标的兼容 alias 保留给既有构建脚本和 loader。

## CTest/Output Naming Boundary / CTest/Output 命名边界

Native CTest and local build output names are neutral-first. The primary smoke and audit tests derive from `JYPPX.OpenCV.Native`, including `JYPPX.OpenCV.NativeSmoke`, `JYPPX.OpenCV.NativeCompatibilitySourceSmoke`, `JYPPX.OpenCV.NativeAbiGeneratedCheck`, `JYPPX.OpenCV.NativeLegacyIncludeParity`, and `JYPPX.OpenCV.NativeAbiExportAudit`. The `OpenCv5Sharp.Native` loader file remains only the compatibility copy for existing binary consumers.

native CTest 和本地 build output 名称保持 neutral-first。主 smoke 与 audit tests 从 `JYPPX.OpenCV.Native` 派生，包括 `JYPPX.OpenCV.NativeSmoke`、`JYPPX.OpenCV.NativeCompatibilitySourceSmoke`、`JYPPX.OpenCV.NativeAbiGeneratedCheck`、`JYPPX.OpenCV.NativeLegacyIncludeParity` 和 `JYPPX.OpenCV.NativeAbiExportAudit`。`OpenCv5Sharp.Native` loader file 仅作为既有 binary consumers 的兼容副本保留。

## Runtime Root/PATH Copy Boundary / Runtime Root/PATH Copy 边界

Windows linked CMake builds use `OPENCV_CSHARP_OPENCV_RUNTIME_ROOT` for the discovered OpenCV runtime directory, copy it into `$<TARGET_FILE_DIR:${OPENCV_CSHARP_NATIVE_TARGET}>`, and put that target output directory first in CTest `PATH`. The `opencv*.dll` names remain factual upstream artifacts for the linked OpenCV build, not project identities.

Windows linked CMake build 使用 `OPENCV_CSHARP_OPENCV_RUNTIME_ROOT` 表示已发现的 OpenCV runtime directory，把它复制到 `$<TARGET_FILE_DIR:${OPENCV_CSHARP_NATIVE_TARGET}>`，并把该 target output directory 放在 CTest `PATH` 首位。`opencv*.dll` 名称仍是 linked OpenCV build 的事实性上游产物，不是项目身份。

## OpenCV Linking / OpenCV 链接方式

The native project supports two build modes:

native 项目支持两种构建模式：

- Stub mode: default mode used when OpenCV has not been built or installed. ABI shape and smoke tests still build, but OpenCV-backed APIs return `OPENCV_CSHARP_STATUS_NOT_LINKED`.
- OpenCV mode: pass the version-neutral `-DOPENCV_CSHARP_OPENCV_DIR=<path-to-OpenCVConfig.cmake-directory>` CMake variable to link against the current OpenCV 5.0.0 build. The older `OPENCV5SHARP_OPENCV_DIR` variable remains accepted only as an existing-build-script compatibility alias.
- ObjDetect mode: OpenCV mode requires `opencv_objdetect` for QR, barcode, QR encoder, ArUco, GridBoard, ChArUco, and MCC checker APIs. `FaceDetectorYN`, `FaceRecognizerSF`, and future DNN-assisted MCC workflows also require `opencv_dnn` and user-supplied model files.
- Photo mode: OpenCV mode requires `opencv_photo` for inpainting, denoising, seamless/editing, edge-preserving/sketch/stylization, tonemap/HDR APIs, gamma correction, `ColorCorrectionModel`, and `IntelligentScissorsMB`.
- Calib mode: OpenCV mode requires `opencv_calib` for full `CalibrateCamera` and `StereoCalibrate` APIs. Existing stereo geometry still stages `opencv_stereo`.
- Video mode: OpenCV mode requires `opencv_video` for optical flow, `.flo` optical-flow file IO, mean-shift/CamShift, optical-flow pyramids, background subtractors, and `KalmanFilter`.
- DNN mode: OpenCV mode requires `opencv_dnn` for `Net`, blob helpers, model loading, single/multi-output forward passes, metadata/profile/FLOPS helpers, and real model execution.
- Stitching mode: OpenCV mode requires `opencv_stitching` for the high-level `Stitcher` pipeline. Default smoke checks status and ABI shape; real panorama success depends on input overlap and features.
- PtCloud mode: OpenCV mode requires `opencv_ptcloud` for depth registration, depth-to-3D conversion, plane finding, frame warping, and `RgbdNormals`.
- Quality mode: OpenCV mode requires contrib `opencv_quality`; BRISQUE also requires `opencv_ml` and caller-supplied model/range files.
- XPhoto mode: OpenCV mode requires contrib `opencv_xphoto` for white balance, channel gains, DCT/BM3D denoising, and oil painting.
- ML mode: in the local OpenCV 5.0.0 tree, `opencv_ml` comes from contrib and is required for `TrainData`, `KNearest`, `SVM`, `SVMSGD`, `LogisticRegression`, `NormalBayesClassifier`, `EM`, `DTrees`, `RTrees`, `Boost`, and `ANN_MLP`.
- ImgHash mode: OpenCV mode requires contrib `opencv_img_hash` for all `OpenCvSharp.ImgHash` objects and one-shot helpers.
- XImgProc mode: OpenCV mode requires contrib `opencv_ximgproc` for local thresholding, edge-aware filters, superpixels, FastLineDetector, disparity WLS helpers, sparse interpolation, EdgeDrawing, EdgeBoxes, ridge/gradient utilities, Fourier descriptors, run-length morphology, ScanSegment, GraphSegmentation, Selective Search, and covariance estimation.
- OptFlow mode: OpenCV mode requires contrib `opencv_optflow`; several first-batch algorithms can also use the staged contrib `opencv_ximgproc` module at runtime.
- BgSegm mode: OpenCV mode requires contrib `opencv_bgsegm`; tiny generated-frame smoke checks call paths, not stable background-model quality.
- Tracking mode: OpenCV mode requires contrib `opencv_tracking`; modern `cv::Tracker` and legacy `cv::legacy::Tracker` are separate opaque-handle boundaries.
- Face mode: OpenCV mode requires contrib `opencv_face`; recognizers, facemark, and MACE convert managed arrays to native vectors inside the ABI boundary.
- Saliency mode: OpenCV mode requires contrib `opencv_saliency`; static/motion saliency objects write caller-owned `Mat` outputs, and `ObjectnessBING` exposes boxes/values through count/fill arrays.
- Plot mode: OpenCV mode requires contrib `opencv_plot`; `Plot2d` stays behind an opaque handle and renders into caller-owned `Mat` outputs.
- Shape mode: OpenCV mode requires contrib `opencv_shape`; histogram cost extractors and shape distance extractors stay behind opaque handles, and descriptor/signature/contour matrices are caller-owned `Mat` values.
- LineDescriptor mode: OpenCV mode requires contrib `opencv_line_descriptor`; binary descriptor and matcher objects stay behind opaque handles, and keyline/match vectors are flattened inside the ABI boundary.
- PhaseUnwrapping mode: OpenCV mode requires contrib `opencv_phase_unwrapping`; `HistogramPhaseUnwrapping` stays behind an opaque handle, and phase/reliability maps are caller-owned `Mat` values.
- StructuredLight mode: OpenCV mode requires contrib `opencv_structured_light`; Gray-code and sinusoidal pattern objects stay behind opaque handles, and generated pattern images are returned as owned `Mat` handles through count/fill arrays.
- IntensityTransform mode: OpenCV mode requires contrib `opencv_intensity_transform`; static image-enhancement functions use caller-owned `Mat` inputs and outputs. BIMEF can still report an Eigen-required OpenCV exception when the OpenCV runtime was built without EIGEN.
- Fuzzy mode: OpenCV mode requires contrib `opencv_fuzzy`; kernel, inpaint/filter, and F0/F1 helpers use caller-owned `Mat` inputs and outputs, and optional masks are represented by nullable `Mat` handles.
- HFS mode: OpenCV mode requires contrib `opencv_hfs`; `cv::hfs::HfsSegment` stays behind an opaque handle, and segmentation writes caller-owned `Mat` outputs.
- Reg mode: OpenCV mode requires contrib `opencv_reg`; `cv::Ptr<cv::reg::Map>` and `cv::Ptr<cv::reg::Mapper>` stay behind opaque handles, and map operations use caller-owned `Mat` inputs and outputs.
- SurfaceMatching mode: OpenCV mode requires contrib `opencv_surface_matching`; ICP and PPF detector objects stay behind opaque handles, and PPF pose vectors are exposed through flat count/fill result structs.
- Rapid mode: OpenCV mode requires contrib `opencv_rapid`; helper functions use caller-owned `Mat` values, and tracker objects stay behind opaque handles.
- AlphaMat mode: OpenCV mode requires contrib `opencv_alphamat`; `infoFlow` uses caller-owned `Mat` image, trimap, and output values.
- BioInspired mode: OpenCV mode requires contrib `opencv_bioinspired`; Retina, fast tone-mapping, and transient segmentation objects stay behind opaque handles, and parameter groups are flattened deliberately. Ordinary native smoke skips linked BioInspired calls; object creation, metadata, and Retina/tone/transient algorithm execution are guarded by `OPENCV_CSHARP_UNSTABLE_NATIVE_SMOKE=1` because tiny linked setup/teardown and inputs can expose local runtime crashes.
- XStereo mode: OpenCV mode requires contrib `opencv_xstereo`; BinaryBM/BinarySGBM/QuasiDense objects stay behind opaque handles, and quasi-dense matches are exposed through count/fill flat structs.
- HighGUI mode: OpenCV mode requires `opencv_highgui` for windows, properties, trackbars, mouse callbacks, and optional Qt buttons; it may need platform GUI dependencies. Default tests keep window creation behind `OPENCV_CSHARP_HIGHGUI_SMOKE=1`; the older `OPENCV5SHARP_HIGHGUI_SMOKE=1` name remains accepted only as an existing-smoke-workflow compatibility alias.
- Optional module mode: `opencv_features` is linked when the installed OpenCV package exposes that target. If the target is missing, `features2d` exports still build and return `OPENCV_CSHARP_STATUS_NOT_LINKED`.
- Optional contrib mode: `opencv_xfeatures2d` and `opencv_xobjdetect` are linked when the installed OpenCV package exposes those targets. If a target is missing, the matching exports still build and return `OPENCV_CSHARP_STATUS_NOT_LINKED`.

Example:

```powershell
cmake -S src\OpenCvSharp.Native -B build\native-opencv -DOPENCV_CSHARP_OPENCV_DIR=<OpenCVConfig.cmake directory>
cmake --build build\native-opencv --config Release
ctest --test-dir build\native-opencv -C Release --output-on-failure
```

## Core Mat ABI / Core Mat ABI

The `core` module now has an object-level `Mat` boundary. `cv::Mat` stays hidden behind the opaque `jyppx_ocv_mat*` handle, and every operation returns a status code instead of throwing through the C ABI.

`core` 模块目前已经具备对象级 `Mat` 边界。`cv::Mat` 始终隐藏在 opaque `jyppx_ocv_mat*` 句柄之后，每个操作都通过状态码返回结果，而不会让异常穿透 C ABI。

Implemented `Mat` ABI groups:

已实现的 `Mat` ABI 分组：

- Construction: `jyppx_ocv_mat_create_empty`, `jyppx_ocv_mat_create`, `jyppx_ocv_mat_create_with_scalar`, `jyppx_ocv_mat_create_in_place`.
- Factories: `jyppx_ocv_mat_zeros`, `jyppx_ocv_mat_ones`, `jyppx_ocv_mat_eye`.
- Lifetime: `jyppx_ocv_mat_release`.
- Copy and fill: `jyppx_ocv_mat_clone`, `jyppx_ocv_mat_copy_to`, `jyppx_ocv_mat_set_to`.
- Views: `jyppx_ocv_mat_submat`, `jyppx_ocv_mat_row_range`, `jyppx_ocv_mat_col_range`, `jyppx_ocv_mat_reshape`.
- Shape and type queries: `jyppx_ocv_mat_empty`, `jyppx_ocv_mat_dims`, `jyppx_ocv_mat_rows`, `jyppx_ocv_mat_cols`, `jyppx_ocv_mat_channels`, `jyppx_ocv_mat_depth`, `jyppx_ocv_mat_type`, `jyppx_ocv_mat_total`.
- Storage queries: `jyppx_ocv_mat_elem_size`, `jyppx_ocv_mat_elem_size1`, `jyppx_ocv_mat_step`, `jyppx_ocv_mat_step1`, `jyppx_ocv_mat_data`, `jyppx_ocv_mat_is_continuous`, `jyppx_ocv_mat_is_submatrix`.
- Core operations: arithmetic, bitwise, compare, range, statistics, normalization, matrix solve/invert, SVD, RNG, generalized matrix multiplication, eigen/polynomial helpers, vector transforms, spectral transforms, channel split/merge, layout transforms, LUT, and identity/symmetric helpers.

- 构造：`jyppx_ocv_mat_create_empty`、`jyppx_ocv_mat_create`、`jyppx_ocv_mat_create_with_scalar`、`jyppx_ocv_mat_create_in_place`。
- 工厂方法：`jyppx_ocv_mat_zeros`、`jyppx_ocv_mat_ones`、`jyppx_ocv_mat_eye`。
- 生命周期：`jyppx_ocv_mat_release`。
- 拷贝与填充：`jyppx_ocv_mat_clone`、`jyppx_ocv_mat_copy_to`、`jyppx_ocv_mat_set_to`。
- 视图：`jyppx_ocv_mat_submat`、`jyppx_ocv_mat_row_range`、`jyppx_ocv_mat_col_range`、`jyppx_ocv_mat_reshape`。
- 形状与类型查询：`jyppx_ocv_mat_empty`、`jyppx_ocv_mat_dims`、`jyppx_ocv_mat_rows`、`jyppx_ocv_mat_cols`、`jyppx_ocv_mat_channels`、`jyppx_ocv_mat_depth`、`jyppx_ocv_mat_type`、`jyppx_ocv_mat_total`。
- 存储查询：`jyppx_ocv_mat_elem_size`、`jyppx_ocv_mat_elem_size1`、`jyppx_ocv_mat_step`、`jyppx_ocv_mat_step1`、`jyppx_ocv_mat_data`、`jyppx_ocv_mat_is_continuous`、`jyppx_ocv_mat_is_submatrix`。
- Core 运算：算术、位运算、比较、范围判断、统计、归一化、矩阵求解/求逆、SVD、RNG、广义矩阵乘法、特征值/多项式辅助、向量变换、频谱变换、通道拆合、布局变换、LUT，以及单位矩阵/对称矩阵辅助。

Ownership rules:

所有权规则：

- Functions returning `jyppx_ocv_mat**` transfer a new handle to the caller.
- `clone` and `copy_to` create independent storage; `submat`, `row_range`, `col_range`, and `reshape` can return views sharing the same OpenCV data.
- The managed wrapper owns each returned handle and must call `jyppx_ocv_mat_release`.
- Data pointers returned by `jyppx_ocv_mat_data` are borrowed and remain valid only while the owning `Mat` data remains alive and unchanged by OpenCV reallocation.

- 返回 `jyppx_ocv_mat**` 的函数会把新句柄所有权交给调用方。
- `clone` 和 `copy_to` 创建独立存储；`submat`、`row_range`、`col_range` 和 `reshape` 可以返回共享同一 OpenCV 数据的视图。
- managed 包装类拥有每个返回句柄，并负责调用 `jyppx_ocv_mat_release`。
- `jyppx_ocv_mat_data` 返回的数据指针是借用指针，只能在拥有该数据的 `Mat` 存活且未被 OpenCV 重新分配期间使用。

## ObjDetect ABI / ObjDetect ABI

The ObjDetect boundary follows the same opaque-handle rule and keeps OpenCV DNN objects behind native handles. It does not expose `cv::Ptr`, `std::vector`, `cv::InputArray`, or `cv::OutputArray` through exported signatures.

ObjDetect 边界遵循相同的不透明句柄规则，并将 OpenCV DNN 对象隐藏在 native 句柄之后。导出签名不会暴露 `cv::Ptr`、`std::vector`、`cv::InputArray` 或 `cv::OutputArray`。

Implemented ObjDetect ABI groups:

已实现的 ObjDetect ABI 分组：

- `QRCodeDetector`: create/release, epsilon setters, alignment-marker setter, single-code detect/decode/detect-and-decode, curved decode paths, multi-code detect/decode/detect-and-decode, and ECI encoding query.
- `BarcodeDetector`: create/release, super-resolution model path creation, detect/decode/detect-and-decode, type-aware decode variants, detector thresholds, and detector scale get/set.
- `QRCodeDetectorAruco`: create/release, flattened detector parameter get/set, single-code detect/decode/detect-and-decode, and multi-code detect/decode/detect-and-decode.
- `QRCodeEncoder`: create/release, flattened encoder parameters, encode string to `Mat`, and structured append encoding.
- `ArucoDictionary`: create/release, predefined dictionaries, byte-list get/set, marker size and correction-bit get/set, marker identification, distance-to-id, marker image generation, marker bits, and bit/byte-list conversion helpers.
- `ArucoDetector`: create/release, dictionary get/set, detector/refine parameter get/set, marker detection, confidence output, grouped point-array output, and `RefineDetectedMarkers` count/fill output.
- `ArucoGridBoard`: create/release, grid size/marker length/marker separation queries, and printable image generation.
- `CharucoBoard`: create/release, chessboard geometry queries, chessboard-corner output, legacy pattern get/set, collinearity checks, and printable image generation.
- `CharucoDetector`: create/release, board get/set, ChArUco parameter get/set, marker-corner input marshalling, and ChArUco/marker grouped point output.
- `DetectorParametersMCC`: flattened default MCC parameter retrieval.
- `CCheckerDetector`: create/release, process, process-with-ROI, best/list checker retrieval, draw, reference colors, detection parameter get/set, and chart type get/set.
- `CChecker`: create/release, target get/set, box get/set, color-chart centers, RGB/YCbCr Mat get/set, cost get/set, and center get/set.
- `FaceDetectorYN`: create from model path, create from model/config buffers, release, input-size get/set, score-threshold get/set, NMS-threshold get/set, topK get/set, and detect.
- `FaceRecognizerSF`: create from model path, create from model/config buffers, release, align/crop, feature extraction, and feature match.
- Structured parity additions: dictionary extension, generic `ArucoBoard` value ownership, multi-dictionary detection, marker/diamond drawing, QR byte-preserving decode, MCC `Net` creation and DNN controls, QR ArUco parameters, ChArUco detector/refine parameters, diamond detection, and advanced chessboard SB/meta/sharpness/4-quad helpers.
- String output uses length/fill pairs so the caller owns the UTF-8 buffer allocation.
- Multi-string output uses count/fill pairs with offset and byte buffers, keeping STL containers inside the native boundary.

- `QRCodeDetector`：创建/释放、epsilon setter、alignment marker setter、单二维码 detect/decode/detect-and-decode、曲面解码路径、多二维码 detect/decode/detect-and-decode，以及 ECI 编码查询。
- `BarcodeDetector`：创建/释放、超分辨率模型路径创建、detect/decode/detect-and-decode、带类型 decode 变体、检测器阈值和 detector scales get/set。
- `QRCodeDetectorAruco`：创建/释放、平铺检测器参数 get/set、单二维码 detect/decode/detect-and-decode，以及多二维码 detect/decode/detect-and-decode。
- `QRCodeEncoder`：创建/释放、平铺 encoder 参数、将字符串编码到 `Mat`，以及结构化追加编码。
- `ArucoDictionary`：创建/释放、预定义字典、byte-list get/set、marker size 和纠错 bit get/set、marker 识别、到指定 id 的距离、marker 图像生成、marker bits，以及 bit/byte-list 转换 helper。
- `ArucoDetector`：创建/释放、字典 get/set、检测/细化参数 get/set、marker 检测、置信度输出、分组点集数组输出，以及 `RefineDetectedMarkers` count/fill 输出。
- `ArucoGridBoard`：创建/释放、grid size/marker length/marker separation 查询，以及可打印图像生成。
- `CharucoBoard`：创建/释放、棋盘几何查询、棋盘角点输出、legacy pattern get/set、共线检查和可打印图像生成。
- `CharucoDetector`：创建/释放、board get/set、ChArUco 参数 get/set、marker-corner 输入封送，以及 ChArUco/marker 分组点集输出。
- `DetectorParametersMCC`：平铺 MCC 参数默认值读取。
- `CCheckerDetector`：创建/释放、process、process-with-ROI、最佳/列表 checker 获取、绘制、参考颜色、检测参数 get/set 和色卡类型 get/set。
- `CChecker`：创建/释放、target get/set、box get/set、色块中心、RGB/YCbCr Mat get/set、cost get/set 和 center get/set。
- `FaceDetectorYN`：通过模型路径创建、通过模型/配置缓冲创建、释放、输入尺寸 get/set、分数阈值 get/set、NMS 阈值 get/set、topK get/set 和 detect。
- `FaceRecognizerSF`：通过模型路径创建、通过模型/配置缓冲创建、释放、对齐裁剪、特征提取和特征匹配。
- 结构化 parity 增量：dictionary extension、generic `ArucoBoard` 值所有权、多字典检测、marker/diamond 绘制、QR 原始字节解码、MCC `Net` 创建与 DNN 控制、QR ArUco 参数、ChArUco detector/refine 参数、diamond 检测，以及高级 chessboard SB/meta/sharpness/4-quad helper。
- 字符串输出使用 length/fill 双阶段接口，由调用方持有 UTF-8 缓冲区分配。
- 多字符串输出使用 count/fill 双阶段接口和 offset/byte 缓冲区，STL 容器始终留在 native 边界内部。
- ArUco marker corners, ChArUco marker inputs/outputs, and MCC point arrays use group offsets or flat `Point2f` buffers. Managed code receives owned array copies.
- ArUco marker corners、ChArUco marker 输入/输出和 MCC 点数组使用分组偏移或扁平 `Point2f` 缓冲区。managed 层接收自己拥有的数组副本。

In the local OpenCV 5.0.0 tree, cascade and HOG APIs were found under contrib `xobjdetect`, not the main `objdetect` surface. They are exported through a separate optional `xobjdetect` ABI so the main `objdetect` wrapper keeps a clear dependency boundary.

在当前本地 OpenCV 5.0.0 源码树中，级联分类器和 HOG API 位于 contrib `xobjdetect`，不属于主线 `objdetect` 接口面。它们通过独立的可选 `xobjdetect` ABI 导出，以保持主线 `objdetect` wrapper 的依赖边界清晰。

## XObjDetect ABI / XObjDetect ABI

The contrib `xobjdetect` boundary is optional. Its exported C functions are always present in the native wrapper, but return `OPENCV_CSHARP_STATUS_NOT_LINKED` unless OpenCV was built with `opencv_xobjdetect`.

contrib `xobjdetect` 边界是可选的。native wrapper 始终导出对应 C 函数，但只有 OpenCV 构建包含 `opencv_xobjdetect` 时才会真正执行；否则返回 `OPENCV_CSHARP_STATUS_NOT_LINKED`。

Implemented XObjDetect ABI groups:

已实现的 XObjDetect ABI 分组：

- `CascadeClassifier`: create/release, file load, empty query, multi-scale detection variants, original window size, old-format query, and feature type query.
- `HOGDescriptor`: create/release, full-parameter creation, file creation, default detector vectors, SVM detector set, detector-size checks, descriptor/window sigma queries, major property get/set, and single/multi-scale detection.
- Array output uses count/fill pairs for rectangles, points, reject levels, weights, and detector coefficient vectors.

- `CascadeClassifier`：创建/释放、文件加载、empty 查询、多尺度检测变体、原始窗口尺寸、旧格式查询和特征类型查询。
- `HOGDescriptor`：创建/释放、完整参数创建、文件创建、默认 detector 向量、SVM detector 设置、detector size 检查、descriptor/window sigma 查询、主要属性 get/set，以及单尺度/多尺度检测。
- 数组输出通过 count/fill 双阶段接口传递矩形、点、reject levels、weights 和 detector 系数向量。

## Photo ABI / Photo ABI

The `photo` boundary is a main OpenCV module boundary and is linked through `opencv_photo`. The exported C functions are always present in stub builds and return `OPENCV_CSHARP_STATUS_NOT_LINKED` when the native wrapper is built without OpenCV.

`photo` 边界属于 OpenCV 主线模块边界，并通过 `opencv_photo` 链接。stub build 中导出的 C 函数始终存在；当 native wrapper 未链接 OpenCV 时返回 `OPENCV_CSHARP_STATUS_NOT_LINKED`。

Implemented Photo ABI groups:

已实现的 Photo ABI 分组：

- Functions: `Inpaint`, scalar-strength fast NLM denoising, per-channel fast NLM denoising, colored fast NLM denoising, multi-frame fast NLM denoising, multi-frame colored fast NLM denoising, `Decolor`, `SeamlessClone`, `ColorChange`, `IlluminationChange`, `TextureFlattening`, `EdgePreservingFilter`, `DetailEnhance`, `PencilSketch`, and `Stylization`.
- Tonemap objects: opaque base handle for `cv::Tonemap` plus dedicated create/get/set functions for Drago, Reinhard, and Mantiuk derived properties.
- HDR objects: opaque aligner, camera-response calibrator, and exposure-merger handles with caller-owned image and matrix outputs.
- CCM: a full-profile-only opaque `ColorCorrectionModel` handle, cloned constructor/setter inputs, caller-owned copied matrix outputs, checked persistence access through Core FileStorage/FileNode accessors, and explicit release.
- Intelligent Scissors: a full-profile-only opaque `IntelligentScissorsMB` handle with retained ref-counted custom-feature matrices, validated state transitions and coordinates, caller-owned contour output, stable `N x 1 CV_32SC2` contour layout, and explicit release.

- 函数：`Inpaint`、标量强度 fast NLM 去噪、按通道 fast NLM 去噪、彩色 fast NLM 去噪、多帧 fast NLM 去噪、多帧彩色 fast NLM 去噪、`Decolor`、`SeamlessClone`、`ColorChange`、`IlluminationChange`、`TextureFlattening`、`EdgePreservingFilter`、`DetailEnhance`、`PencilSketch` 和 `Stylization`。
- Tonemap 对象：用于 `cv::Tonemap` 的 opaque 基类句柄，以及 Drago、Reinhard 和 Mantiuk 派生属性的专用 create/get/set 函数。

## Video ABI / Video ABI

The `video` boundary is a main OpenCV module boundary linked through `opencv_video`. Functions accept opaque `Mat` handles and flat value buffers; `KalmanFilter` is held as an opaque object handle.

`video` 边界属于 OpenCV 主线模块边界，并通过 `opencv_video` 链接。函数接收 opaque `Mat` 句柄和平铺值缓冲；`KalmanFilter` 作为 opaque 对象句柄持有。

Implemented Video ABI groups:

已实现的 Video ABI 分组：

- `CalcOpticalFlowPyrLK` with flat `Point2f`, status, and error buffers.
- `CalcOpticalFlowFarneback` with caller-owned flow `Mat`.
- `ReadOpticalFlow` and `WriteOpticalFlow` for `.flo` files; flow matrices should be `CV_32FC2`.
- `BuildOpticalFlowPyramid` count/fill output returning owned `Mat` handles.
- `MeanShift` and `CamShift` with flattened `Rect` and `RotatedRect` values.
- `BackgroundSubtractor`: base apply, known-foreground apply, background-image output, and release.
- `BackgroundSubtractorMOG2`: create and history, mixture, shadow, threshold, variance, and complexity-reduction properties.
- `BackgroundSubtractorKNN`: create and history, sample, shadow, KNN-sample, and distance-threshold properties.
- `KalmanFilter`: create/release/init, predict, correct, and matrix get/set for state, transition, control, measurement, covariance, gain, and error matrices.

- `CalcOpticalFlowPyrLK`：使用平铺 `Point2f`、status 和 error 缓冲。
- `CalcOpticalFlowFarneback`：使用调用方持有的 flow `Mat`。
- `ReadOpticalFlow` 和 `WriteOpticalFlow`：用于 `.flo` 文件；flow 矩阵应为 `CV_32FC2`。
- `BuildOpticalFlowPyramid`：通过 count/fill 输出返回 owned `Mat` 句柄。
- `MeanShift` 与 `CamShift`：使用平铺 `Rect` 和 `RotatedRect` 值。
- `BackgroundSubtractor`：基类 apply、known-foreground apply、background-image 输出和释放。
- `BackgroundSubtractorMOG2`：创建以及 history、mixture、shadow、threshold、variance 和 complexity-reduction 属性。
- `BackgroundSubtractorKNN`：创建以及 history、sample、shadow、KNN-sample 和 distance-threshold 属性。
- `KalmanFilter`：创建/释放/init、predict、correct，以及 state、transition、control、measurement、covariance、gain、error 矩阵 get/set。

## DNN ABI / DNN ABI

The `dnn` boundary keeps `cv::dnn::Net`, ref-counted `cv::dnn::Layer`, and nested forward results behind separate opaque handles. Model buffers and strings are caller-owned, and variable outputs use exact count/fill pairs.

`dnn` 边界将 `cv::dnn::Net` 保持在 opaque 句柄之后。模型缓冲和字符串由调用方持有，数组输出使用 count/fill 双阶段接口。

Implemented DNN ABI groups:

已实现的 DNN ABI 分组：

- `Net`: path/buffer ONNX, TensorFlow, TFLite, OpenVINO, and general model loading; backend/target/finalization; input, single/multi/nested forward; dump/connect/register; layer lookup; parameter get/set; shapes, FLOPS, memory, tracing, profiling, fusion, Winograd, and KV cache controls.
- `Layer`: independently ref-counted lookup handles with explicit release and output-name indexing. Layer lifetime does not borrow the parent `Net`.
- Blob helpers: legacy and `Image2BlobParams` preprocessing, NCHW/NHWC, crop/letterbox, rectangle projection, and `ImagesFromBlob`.
- Strings use strict UTF-8 and owned Core result handles. String arrays, detailed profile columns, shapes, targets, and layer ids use exact count/fill or packed offset/value buffers.
- Nested forward results use a temporary opaque group handle. Each returned Mat becomes independently owned; partial native and managed conversion failures release all created handles.
- KV cache calls reject graphs without a New-engine `mainGraph` before entering OpenCV's cache manager, preventing the upstream Classic-engine null dereference.

- `Net`：创建空网络、通过模型/配置/framework 路径读取、通过模型/配置缓冲读取、ONNX/TensorFlow/TFLite/OpenVINO 便捷读取、释放、empty 查询、backend/target setter、input setter、单输出和多输出 forward、层名称、未连接输出层名称/id、输入名称/形状、layer type 元数据、profile 耗时和 FLOPS helper。
- Blob helper：`BlobFromImage`、`BlobFromImages` 和 `ImagesFromBlob`。
- 字符串数组使用 offsets 加 UTF-8 byte 缓冲；Mat 数组使用调用方提供的句柄缓冲。

## Stitching ABI / Stitching ABI

The `stitching` boundary keeps `cv::Stitcher` behind an opaque handle and links through `opencv_stitching`. It wraps the high-level pipeline only; detail matchers, estimators, seam finders, and blenders remain inside OpenCV for this round.

`stitching` 边界将 `cv::Stitcher` 保持在 opaque handle 后面，并通过 `opencv_stitching` 链接。本轮只封装高层 pipeline；detail matchers、estimators、seam finders 和 blenders 仍留在 OpenCV 内部。

Implemented Stitching ABI groups:

已实现的 Stitching ABI 分组：

- `Stitcher`: create/release, registration/seam/compositing resolution, confidence threshold, wave correction, interpolation flags, wave correction kind, and work scale.
- Pipeline calls: `EstimateTransform`, `ComposePanorama`, and `Stitch` with caller-owned `Mat` handle arrays for images and optional masks.
- Outputs: component indices via count/fill, camera parameters via count/fill with owned rotation/translation `Mat` handles, and result mask copy into caller-owned `Mat`.
- no-OpenCV builds export all functions and return `OPENCV_CSHARP_STATUS_NOT_LINKED`.

- `Stitcher`：创建/释放、registration/seam/compositing resolution、confidence threshold、wave correction、interpolation flags、wave correction kind 和 work scale。
- Pipeline 调用：`EstimateTransform`、`ComposePanorama` 和 `Stitch`，图像和可选 mask 使用调用方持有的 `Mat` handle 数组。
- 输出：component 索引使用 count/fill，相机参数使用 count/fill 并返回 owned rotation/translation `Mat` handle，result mask 拷贝到调用方持有的 `Mat`。
- no-OpenCV build 会导出所有函数并返回 `OPENCV_CSHARP_STATUS_NOT_LINKED`。

## HighGUI ABI / HighGUI ABI

The `highgui` boundary exposes only small UTF-8 string and `Mat` handle calls. It is intended for guarded local smoke tests and user applications with a GUI environment.

`highgui` 边界只暴露小型 UTF-8 字符串和 `Mat` 句柄调用。它用于受控本地 smoke 测试，以及具备 GUI 环境的用户应用。

Implemented HighGUI ABI groups:

已实现的 HighGUI ABI 分组：

- `NamedWindow`, `DestroyWindow`, `DestroyAllWindows`, `ImShow`, `WaitKey`, `PollKey`, `MoveWindow`, and `ResizeWindow`.
- Window property/title/image-rectangle helpers.
- Trackbar create/get/set/min/max with managed callback lifetime held by `HighGuiTrackbar`.
- Mouse and Qt button callback registration. The initial managed wrapper keeps one current callback slot per callback kind.
- no-OpenCV builds export all functions and return `OPENCV_CSHARP_STATUS_NOT_LINKED`.

- `NamedWindow`、`DestroyWindow`、`DestroyAllWindows`、`ImShow`、`WaitKey`、`PollKey`、`MoveWindow` 和 `ResizeWindow`。
- 窗口属性、标题和图像区域 helper。
- Trackbar create/get/set/min/max，并由 `HighGuiTrackbar` 保持 managed callback 生命周期。
- 鼠标和 Qt button 回调注册。当前初始 managed wrapper 对每种回调保留一个当前槽位。
- no-OpenCV build 会导出所有函数并返回 `OPENCV_CSHARP_STATUS_NOT_LINKED`。

## Calib ABI / Calib ABI

Local OpenCV 5.0.0 places full calibration functions in the `calib` module. The C ABI accepts grouped point arrays as offsets plus flat `Point2f` / `Point3f` buffers and returns pose arrays packed into owned `Mat` outputs.

本地 OpenCV 5.0.0 将完整标定函数放在 `calib` 模块中。C ABI 使用偏移表加扁平 `Point2f` / `Point3f` 缓冲区传递分组点数组，并将位姿数组打包到调用方拥有的 `Mat` 输出中。

Implemented Calib ABI groups:

已实现的 Calib ABI 分组：

- `CalibrateCamera` and `CalibrateCameraExtended`.
- `StereoCalibrate` and `StereoCalibrateExtended`.
- `Rectify3Collinear`.
- no-OpenCV builds keep all exported functions and return `OPENCV_CSHARP_STATUS_NOT_LINKED`.

- `CalibrateCamera` 和 `CalibrateCameraExtended`。
- `StereoCalibrate` 和 `StereoCalibrateExtended`。
- `Rectify3Collinear`。
- no-OpenCV 构建保留所有导出函数，并返回 `OPENCV_CSHARP_STATUS_NOT_LINKED`。

## ML ABI / ML ABI

The local OpenCV 5.0.0 `ml` headers are under the contrib tree. The native wrapper keeps `cv::ml::TrainData`, `cv::ml::ParamGrid`, and `cv::ml::StatModel`-derived objects behind opaque handles. No `cv::Ptr`, STL container, `InputArray`, or `OutputArray` crosses the exported ABI.

本地 OpenCV 5.0.0 的 `ml` 头文件位于 contrib 树。native wrapper 将 `cv::ml::TrainData`、`cv::ml::ParamGrid` 和 `cv::ml::StatModel` 派生对象保持在 opaque handle 后面。导出 ABI 不暴露 `cv::Ptr`、STL 容器、`InputArray` 或 `OutputArray`。

Implemented ML ABI groups:

已实现的 ML ABI 分组：

- `TrainData`: create from matrices, load from CSV, shape/state queries, Mat getters, train/test split, shuffle, names, and sub-vector/sub-matrix helpers.
- `ParamGrid`: create/release, value get/set, and SVM default-grid creation.
- `StatModel`: common state, train, predict, error calculation, save, and clear.
- `KNearest`: create/load, properties, and nearest-neighbor prediction with caller-owned output `Mat` objects.
- `SVM`: create/load, scalar properties, kernel, class weights, term criteria, TrainAuto, support vectors, uncompressed support vectors, and decision function.
- `NormalBayesClassifier`: create/load, common model calls, and probability prediction.

- `TrainData`：从矩阵创建、从 CSV 加载、形状/状态查询、Mat getter、训练/测试划分、shuffle、名称、sub-vector/sub-matrix helper。
- `ParamGrid`：创建/释放、值 get/set，以及 SVM 默认网格创建。
- `StatModel`：通用状态、训练、预测、误差计算、保存和清理。
- `KNearest`：创建/加载、属性，以及通过调用方持有的输出 `Mat` 执行最近邻预测。
- `SVM`：创建/加载、标量属性、核函数、类别权重、终止条件、TrainAuto、支持向量、未压缩支持向量和决策函数。
- `NormalBayesClassifier`：创建/加载、通用模型调用和概率预测。

## ImgHash ABI / ImgHash ABI

The contrib `img_hash` boundary holds each algorithm as an opaque handle to an OpenCV image-hash object. Static one-shot helpers and object methods all use caller-owned `Mat` handles for input and output.

contrib `img_hash` 边界将每个算法对象作为 opaque handle 持有。静态一次性 helper 和对象方法都使用调用方持有的 `Mat` handle 传入输入和输出。

Implemented ImgHash ABI groups:

已实现的 ImgHash ABI 分组：

- Object create/release for average hash, pHash, block mean, color moment, Marr-Hildreth, and radial variance hashes.
- Base compute/compare through opaque handles.
- Block mean mode and mean-vector count/fill output.
- Marr-Hildreth alpha/scale get/set.
- Radial variance sigma and angle-line get/set.
- Static one-shot compute helpers for all first-batch algorithms.

- Average hash、pHash、block mean、color moment、Marr-Hildreth 和 radial variance 哈希对象的创建/释放。
- 通过 opaque handle 进行基类 compute/compare。
- Block mean mode 与 mean vector count/fill 输出。
- Marr-Hildreth alpha/scale get/set。
- Radial variance sigma 与 angle-line get/set。
- 第一批所有算法的静态一次性 compute helper。

## Plot ABI / Plot ABI

The contrib `plot` boundary keeps `cv::plot::Plot2d` behind an opaque handle. Inputs and rendered output are caller-owned `Mat` handles; the C ABI exposes factory, setter, render, and release functions only.

contrib `plot` 边界将 `cv::plot::Plot2d` 保持在 opaque handle 后面。输入与渲染输出均为调用方持有的 `Mat` handle；C ABI 只暴露工厂、setter、render 和 release 函数。

Implemented Plot ABI groups:

已实现的 Plot ABI 分组：

- `Plot2d`: create from Y values, create from X/Y values, release, and render.
- Setters for min/max bounds, line width, line/grid/text visibility, plot/axis/grid/text colors, plot size, orientation, grid-line count, and point index text.

- `Plot2d`：从 Y 值创建、从 X/Y 值创建、释放和渲染。
- setter 覆盖 min/max 边界、线宽、线/网格/文本显示、曲线/坐标轴/网格/文本颜色、绘图尺寸、方向、网格线数量和点索引文本。

## Shape ABI / Shape ABI

The contrib `shape` boundary keeps histogram cost extractors and distance extractors behind opaque handles. Descriptor, signature, and contour data stays in caller-owned `Mat` values, and cost matrices are written to caller-owned outputs or returned as managed-owned `Mat` wrappers.

contrib `shape` 边界将 histogram cost extractor 与 distance extractor 保持在 opaque handle 后面。descriptor、signature 与 contour 数据保留在调用方持有的 `Mat` 中，cost matrix 写入调用方持有的输出或作为 managed 持有的 `Mat` wrapper 返回。

Implemented Shape ABI groups:

已实现的 Shape ABI 分组：

- Static `EMDL1`.
- `HistogramCostExtractor`: release, `BuildCostMatrix`, `NDummies`, and `DefaultCost`.
- `NormHistogramCostExtractor` and `EMDHistogramCostExtractor`: create plus `NormFlag` get/set.
- `ChiHistogramCostExtractor` and `EMDL1HistogramCostExtractor`: create plus shared histogram-cost settings.
- `ShapeDistanceExtractor`: release and `ComputeDistance`.
- `ShapeContextDistanceExtractor`: create.
- `HausdorffDistanceExtractor`: create plus `DistanceFlag` and `RankProportion` get/set.

- 静态 `EMDL1`。
- `HistogramCostExtractor`：释放、`BuildCostMatrix`、`NDummies` 与 `DefaultCost`。
- `NormHistogramCostExtractor` 与 `EMDHistogramCostExtractor`：创建以及 `NormFlag` get/set。
- `ChiHistogramCostExtractor` 与 `EMDL1HistogramCostExtractor`：创建以及共享 histogram-cost 设置。
- `ShapeDistanceExtractor`：释放与 `ComputeDistance`。
- `ShapeContextDistanceExtractor`：创建。
- `HausdorffDistanceExtractor`：创建以及 `DistanceFlag` 与 `RankProportion` get/set。

## LineDescriptor Boundary / LineDescriptor 边界

The contrib `line_descriptor` boundary keeps binary descriptor and matcher objects behind opaque handles. Keyline and match arrays are flattened into native POD buffers before crossing the ABI, while descriptor matrices and drawing outputs remain caller-owned `Mat` values.

contrib `line_descriptor` 边界将 binary descriptor 与 matcher 对象保持在 opaque handle 后面。KeyLine 与 match 数组进入 ABI 前会被平铺为 native POD buffer，descriptor 矩阵与绘图输出仍由调用方持有的 `Mat` 表示。

Implemented LineDescriptor ABI groups:

已实现的 LineDescriptor ABI 分组：

- `KeyLine` immutable value object with equality and start/end point helpers.
- `BinaryDescriptor`: create/release, clear, empty, descriptor metadata, parameter get/set, `Detect`, `Compute`, and `DetectAndCompute`.
- `BinaryDescriptorMatcher`: create/release, clear, empty, `Match`, and `KnnMatch`.
- Drawing helpers: `drawKeylines` and `drawLineMatches`.
- `DrawLinesMatchesFlags` enum matching OpenCV drawing flag values.

- `KeyLine` 不可变值对象，支持相等性和起止点 helper。
- `BinaryDescriptor`：创建/释放、clear、empty、descriptor 元数据、参数 get/set、`Detect`、`Compute` 与 `DetectAndCompute`。
- `BinaryDescriptorMatcher`：创建/释放、clear、empty、`Match` 与 `KnnMatch`。
- 绘图 helper：`drawKeylines` 与 `drawLineMatches`。
- `DrawLinesMatchesFlags` 枚举，对应 OpenCV 绘制标志值。

## PhaseUnwrapping Boundary / PhaseUnwrapping 边界

The contrib `phase_unwrapping` boundary keeps `cv::phase_unwrapping::PhaseUnwrapping` and `HistogramPhaseUnwrapping` behind an opaque handle. Wrapped, unwrapped, optional shadow-mask, and reliability-map values stay as caller-owned `Mat` handles.

contrib `phase_unwrapping` 边界将 `cv::phase_unwrapping::PhaseUnwrapping` 和 `HistogramPhaseUnwrapping` 保持在 opaque handle 后面。包裹相位图、展开输出、可选 shadow mask 与 reliability map 都保持为调用方持有的 `Mat` handle。

Implemented PhaseUnwrapping ABI groups:

已实现的 PhaseUnwrapping ABI 分组：

- `HistogramPhaseUnwrapping` creation from flat parameter values.
- Base release and `UnwrapPhaseMap`.
- `GetInverseReliabilityMap` output.

- 通过平铺参数值创建 `HistogramPhaseUnwrapping`。
- 基类释放和 `UnwrapPhaseMap`。
- `GetInverseReliabilityMap` 输出。

## StructuredLight Boundary / StructuredLight 边界

The contrib `structured_light` boundary keeps Gray-code and sinusoidal pattern objects behind one opaque pattern handle. Pattern image vectors are generated inside native code and returned through count/fill arrays of newly owned `Mat` handles; managed code owns and releases each returned image.

contrib `structured_light` 边界将 Gray-code 与正弦图案对象保持在一个 opaque pattern handle 后面。图案图像 vector 在 native 内部生成，并通过新持有 `Mat` handle 的 count/fill 数组返回；managed 代码拥有并释放每个返回图像。

Implemented StructuredLight ABI groups:

已实现的 StructuredLight ABI 分组：

- `GrayCodePattern`: create, pattern image count, threshold setters, shadow-mask image generation, and `GetProjPixel`.
- `SinusoidalPattern`: create from flat parameters and marker points, generate inherited from the base pattern, `ComputePhaseMap`, `UnwrapPhaseMap`, and `ComputeDataModulationTerm`.
- Base pattern release and `Generate` count/fill output.

- `GrayCodePattern`：创建、图案图像数量、阈值 setter、shadow-mask 图像生成和 `GetProjPixel`。
- `SinusoidalPattern`：通过平铺参数和 marker 点创建、继承基类 `Generate`、`ComputePhaseMap`、`UnwrapPhaseMap` 和 `ComputeDataModulationTerm`。
- 基类 pattern 释放和 `Generate` count/fill 输出。

## IntensityTransform Boundary / IntensityTransform 边界

The contrib `intensity_transform` boundary exposes static image-enhancement functions through caller-owned `Mat` handles. No OpenCV `InputArray` or `OutputArray` crosses the C ABI.

contrib `intensity_transform` 边界通过调用方持有的 `Mat` handle 暴露静态图像增强函数。OpenCV `InputArray` 或 `OutputArray` 不穿过 C ABI。

Implemented IntensityTransform ABI groups:

已实现的 IntensityTransform ABI 分组：

- Log transform, gamma correction, autoscaling, and contrast stretching.
- BIMEF with automatic exposure ratio and BIMEF with explicit exposure ratio.
- No-OpenCV and missing-module builds export the same functions and return `NOT_LINKED`.

- 对数变换、gamma 校正、自动缩放和对比度拉伸。
- 自动曝光比例 BIMEF 与显式曝光比例 BIMEF。
- no-OpenCV 和缺模块构建导出相同函数，并返回 `NOT_LINKED`。

BIMEF can also fail at runtime when OpenCV was built without EIGEN support. This is an OpenCV algorithm dependency rather than an ABI boundary failure.

当 OpenCV 未启用 EIGEN 支持时，BIMEF 也可能在运行时失败。这是 OpenCV 算法依赖，而不是 ABI 边界失败。

## Fuzzy Boundary / Fuzzy 边界

The contrib `fuzzy` boundary exposes kernel creation, inpaint/filter, and F-transform helpers through caller-owned `Mat` handles. Optional masks are represented as nullable `Mat` handles that become `cv::noArray()` inside native code.

contrib `fuzzy` 边界通过调用方持有的 `Mat` handle 暴露 kernel 创建、inpaint/filter 和 F-transform helper。可选 mask 使用可空 `Mat` handle 表示，并在 native 内部转换为 `cv::noArray()`。

Implemented Fuzzy ABI groups:

已实现的 Fuzzy ABI 分组：

- Kernel creation from function matrices and from predefined function/radius/channel values.
- `inpaint` and `filter`.
- F0 helpers: components, inverse transform, process, iteration, optimized linear process, and optimized linear float process.
- F1 helpers: components, polynomial components, vertical/horizontal polynomial matrix creation, inverse transform, and process.
- No-OpenCV and missing-module builds export the same functions and return `NOT_LINKED`.

- 通过函数矩阵以及预定义 function/radius/channel 值创建 kernel。
- `inpaint` 与 `filter`。
- F0 helper：components、inverse transform、process、iteration、optimized linear process 和 optimized linear float process。
- F1 helper：components、polynomial components、vertical/horizontal polynomial matrix 创建、inverse transform 和 process。
- no-OpenCV 和缺模块构建导出相同函数，并返回 `NOT_LINKED`。

## HFS Boundary / HFS 边界

The contrib `hfs` boundary keeps `cv::hfs::HfsSegment` behind an opaque handle. Segmenter construction uses flat parameter values, properties use numeric property ids inside native code, and segmentation writes caller-owned `Mat` outputs.

contrib `hfs` 边界将 `cv::hfs::HfsSegment` 保持在 opaque handle 后面。Segmenter 构造使用平铺参数值，属性在 native 内部使用数字 property id，分割结果写入调用方持有的 `Mat` 输出。

Implemented HFS ABI groups:

已实现的 HFS ABI 分组：

- `HfsSegment` create/release.
- Float properties: `SegEgbThresholdI`, `SegEgbThresholdII`, and `SpatialWeight`.
- Integer properties: `MinRegionSizeI`, `MinRegionSizeII`, `SlicSpixelSize`, and `NumSlicIter`.
- CPU segmentation and GPU segmentation entry points. Default smoke uses CPU only.
- No-OpenCV and missing-module builds export the same functions and return `NOT_LINKED`.

- `HfsSegment` 创建/释放。
- float 属性：`SegEgbThresholdI`、`SegEgbThresholdII` 和 `SpatialWeight`。
- int 属性：`MinRegionSizeI`、`MinRegionSizeII`、`SlicSpixelSize` 和 `NumSlicIter`。
- CPU 分割与 GPU 分割入口。默认 smoke 只使用 CPU。
- no-OpenCV 和缺模块构建导出相同函数，并返回 `NOT_LINKED`。

## Reg Boundary / Reg 边界

The contrib `reg` boundary keeps `cv::Ptr<cv::reg::Map>` and `cv::Ptr<cv::reg::Mapper>` behind opaque handles. Flat transform values are used for shift, affine, and projective maps. Caller-owned `Mat` handles carry image inputs and outputs.

contrib `reg` 边界将 `cv::Ptr<cv::reg::Map>` 与 `cv::Ptr<cv::reg::Mapper>` 保持在 opaque handle 后面。shift、affine 和 projective map 使用平铺变换值。图像输入输出通过调用方持有的 `Mat` handle 传递。

Implemented Reg ABI groups:

已实现的 Reg ABI 分组：

- Map release, kind query, warp, inverse warp, inverse map, compose, and scale.
- `MapShift`, `MapAffine`, and `MapProjec` creation plus flat parameter retrieval.
- Mapper creation for gradient shift, Euclidean, similarity, affine, and projective registration.
- `MapperPyramid` creation plus level and iteration property get/set.
- `Mapper.Calculate` and `Mapper.GetMap` return new opaque map handles.
- No-OpenCV and missing-module builds export the same functions and return `NOT_LINKED`.

- map 释放、类型查询、warp、inverse warp、inverse map、compose 和 scale。
- `MapShift`、`MapAffine` 与 `MapProjec` 创建，以及平铺参数读取。
- gradient shift、Euclidean、similarity、affine 和 projective registration mapper 创建。
- `MapperPyramid` 创建以及 level/iteration 属性 get/set。
- `Mapper.Calculate` 与 `Mapper.GetMap` 返回新的 opaque map handle。
- no-OpenCV 和缺模块构建导出相同函数，并返回 `NOT_LINKED`。

## SurfaceMatching Boundary / SurfaceMatching 边界

The contrib `surface_matching` boundary keeps `cv::ppf_match_3d::ICP`, `cv::ppf_match_3d::PPF3DDetector`, and OpenCV pose vectors inside native code. ICP returns a flat residual and row-major pose matrix. PPF matches use count/fill APIs with flat pose summaries.

contrib `surface_matching` 边界将 `cv::ppf_match_3d::ICP`、`cv::ppf_match_3d::PPF3DDetector` 和 OpenCV pose vector 保持在 native 内部。ICP 返回平铺 residual 与行优先 pose 矩阵。PPF match 使用 count/fill API 输出平铺 pose 摘要。

Implemented SurfaceMatching ABI groups:

已实现的 SurfaceMatching ABI 分组：

- ICP create/release and `RegisterModelToScene` with result code, residual, and 16-value pose output.
- PPF 3D detector create/release, search parameter setup, model training, match count, and match fill.
- `Pose3D` summaries flatten alpha, residual, model index, votes, angle, translation, quaternion, and 4x4 pose.
- No-OpenCV and missing-module builds export the same functions and return `NOT_LINKED`.

- ICP 创建/释放，以及带 result code、residual 和 16 值 pose 输出的 `RegisterModelToScene`。
- PPF 3D detector 创建/释放、search 参数设置、model training、match count 和 match fill。
- `Pose3D` 摘要平铺 alpha、residual、model index、votes、angle、translation、quaternion 和 4x4 pose。
- no-OpenCV 和缺模块构建导出相同函数，并返回 `NOT_LINKED`。

## Rapid Boundary / Rapid 边界

The contrib `rapid` boundary exposes stateless helper functions through caller-owned `Mat` handles and keeps `cv::rapid::Tracker` objects behind opaque handles. The local OpenCV 5.0.0 `GOSTracker::create` surface is not exposed in this batch because it returns the `OLSTracker` pointer type.

contrib `rapid` 边界通过调用方持有的 `Mat` handle 暴露无状态 helper，并将 `cv::rapid::Tracker` 对象保持在 opaque handle 后面。本地 OpenCV 5.0.0 `GOSTracker::create` 接口本批次未暴露，因为它返回 `OLSTracker` 指针类型。

Implemented Rapid ABI groups:

已实现的 Rapid ABI 分组：

- Static draw/extract/find/convert/run helper functions.
- `Rapid` and `OLSTracker` creation behind a shared tracker handle.
- Tracker release, `Compute`, and `ClearState`.
- No-OpenCV and missing-module builds export the same functions and return `NOT_LINKED`.

- 静态 draw/extract/find/convert/run helper。
- 通过共享 tracker handle 创建 `Rapid` 与 `OLSTracker`。
- tracker 释放、`Compute` 和 `ClearState`。
- no-OpenCV 和缺模块构建导出相同函数，并返回 `NOT_LINKED`。

## XImgProc ABI / XImgProc ABI

The contrib `ximgproc` boundary keeps edge-aware filter objects, superpixel segmenters, disparity filters, sparse interpolators, EdgeDrawing, EdgeBoxes, the fast line detector, ridge filters, contour fitting, ScanSegment, GraphSegmentation, and Selective Search objects behind opaque handles. Static helpers and object methods use caller-owned `Mat` inputs and outputs; vector-like outputs are exposed through flat count/fill arrays, grouped point arrays, `Point3i` run arrays, or existing managed value objects.

contrib `ximgproc` 边界将 edge-aware filter 对象、超像素分割器、disparity filter、稀疏插值器、EdgeDrawing、EdgeBoxes、快速线段检测器、ridge filter、轮廓拟合、ScanSegment、GraphSegmentation 和 Selective Search 对象保持在 opaque handle 后面。静态 helper 和对象方法使用调用方持有的 `Mat` 输入输出；类似 vector 的输出通过平铺 count/fill 数组、分组点集数组、`Point3i` run 数组或现有 managed 值对象暴露。

Implemented XImgProc ABI groups:

已实现的 XImgProc ABI 分组：

- Static helpers: NiBlack-family thresholding, thinning, anisotropic diffusion, joint bilateral, guided, rolling guidance, weighted median, domain-transform, adaptive-manifold, bilateral texture, edge-preserving, fast global smoother, L0 smooth, fast Hough, Hough point-to-line, and Pei-Lin normalization.
- Edge-aware objects: `GuidedFilter` and `FastGlobalSmootherFilter` create/release plus filter calls.
- Superpixels: `SuperpixelSLIC`, `SuperpixelSEEDS`, and `SuperpixelLSC` create/release, iterate, labels, contour masks, connectivity, and superpixel count.
- `FastLineDetector`: create/release, detect to `Mat`, detect count/fill to flat line segments, and draw segments from either representation.
- Disparity: `DisparityWLSFilter` generic creation, base filter call, lambda/sigma/LRC/depth-discontinuity properties, confidence map, ROI, disparity visualization, MSE, and bad-pixel percentage.
- Fast bilateral solver: reusable `FastBilateralSolverFilter` object and one-shot helper. Linked behavior can depend on OpenCV EIGEN support.
- Sparse interpolation: `SparseMatchInterpolator` base calls plus `EdgeAwareInterpolator` and `RICInterpolator` property groups, cost-map input, and `Mat`-based sparse point sets.
- Edge/proposal: `EdgeDrawing` parameters, edge/gradient images, grouped segment count/fill output, line and ellipse outputs, segment-index output, plus `EdgeBoxes` Rect/score proposal count/fill output.
- Ridge and gradients: `RidgeDetectionFilter` create/release plus Deriche and Paillou gradient helpers.
- Fourier descriptors: `fourierDescriptor`, `transformFD`, `contourSampling`, and `ContourFitting` create/release/estimate transformation calls.
- Run-length morphology: threshold, structuring element creation, dilate, erode, morphologyEx, paint, feasibility check, and `createRLEImage` through flat `Point3i` arrays.
- Segmentation: `ScanSegment`, `GraphSegmentation`, Selective Search segmentation objects, independently owned strategy handles, and Selective Search proposal Rect count/fill output.
- Covariance estimation: complex-matrix input and caller-owned `Mat` output.
- `StructuredEdgeDetection` is model-file dependent and not part of the default ABI smoke in this batch.

- 静态 helper：NiBlack 系列阈值、细化、各向异性扩散、joint bilateral、guided、rolling guidance、weighted median、domain-transform、adaptive-manifold、bilateral texture、edge-preserving、fast global smoother、L0 smooth、fast Hough、Hough point-to-line 和 Pei-Lin normalization。
- Edge-aware 对象：`GuidedFilter` 与 `FastGlobalSmootherFilter` 创建/释放和 filter 调用。
- 超像素：`SuperpixelSLIC`、`SuperpixelSEEDS` 与 `SuperpixelLSC` 创建/释放、iterate、labels、contour mask、connectivity 和 superpixel count。
- `FastLineDetector`：创建/释放、检测到 `Mat`、通过 count/fill 输出平铺线段，以及从两种表示绘制线段。
- Disparity：`DisparityWLSFilter` generic 创建、基类 filter 调用、lambda/sigma/LRC/depth-discontinuity 属性、confidence map、ROI、disparity 可视化、MSE 和 bad-pixel percentage。
- Fast bilateral solver：可复用 `FastBilateralSolverFilter` 对象和一次性 helper。linked 行为可能取决于 OpenCV EIGEN 支持。
- 稀疏插值：`SparseMatchInterpolator` 基类调用，以及 `EdgeAwareInterpolator` 和 `RICInterpolator` 属性组、cost map 输入、基于 `Mat` 的稀疏点集。
- Edge/proposal：`EdgeDrawing` 参数、edge/gradient 图、分组 segment count/fill 输出、line 和 ellipse 输出、segment index 输出，以及 `EdgeBoxes` Rect/score proposal count/fill 输出。
- Ridge 与梯度：`RidgeDetectionFilter` 创建/释放，以及 Deriche 和 Paillou 梯度 helper。
- Fourier descriptor：`fourierDescriptor`、`transformFD`、`contourSampling`，以及 `ContourFitting` 创建/释放/估计变换调用。
- Run-length morphology：threshold、structuring element 创建、dilate、erode、morphologyEx、paint、可行性检查，以及通过平铺 `Point3i` 数组调用 `createRLEImage`。
- 分割：`ScanSegment`、`GraphSegmentation`、Selective Search segmentation 对象、独立持有的 strategy 句柄，以及 Selective Search proposal Rect count/fill 输出。
- Covariance estimation：复数矩阵输入和调用方持有的 `Mat` 输出。
- `StructuredEdgeDetection` 依赖模型文件，本批次不进入默认 ABI smoke。

## OptFlow ABI / OptFlow ABI

The contrib `optflow` boundary keeps `cv::Ptr<cv::DenseOpticalFlow>`, `cv::Ptr<cv::SparseOpticalFlow>`, `cv::optflow::RLOFOpticalFlowParameter`, and concrete Dual TV-L1/RLOF objects behind opaque handles. Dense flow writes to caller-owned `Mat` outputs, commonly `CV_32FC2`. Sparse point sets are represented as caller-owned `Mat` values so no STL vector or `Point2f` container crosses the ABI.

contrib `optflow` 边界将 `cv::Ptr<cv::DenseOpticalFlow>`、`cv::Ptr<cv::SparseOpticalFlow>`、`cv::optflow::RLOFOpticalFlowParameter` 以及具体 Dual TV-L1/RLOF 对象保持在 opaque handle 后面。密集光流写入调用方持有的 `Mat` 输出，常见为 `CV_32FC2`。稀疏点集使用调用方持有的 `Mat` 表达，因此没有 STL vector 或 `Point2f` 容器穿过 ABI。

Implemented OptFlow ABI groups:

已实现的 OptFlow ABI 分组：

- Dense/sparse object release, `Calc`, and `CollectGarbage`.
- `DualTVL1OpticalFlow` creation plus scalar property get/set.
- `RLOFOpticalFlowParameter` creation plus solver/support-region and numeric property get/set.
- Dense and sparse RLOF object creation, parameter ownership transfer, and property get/set.
- Static SimpleFlow, SparseToDense, DenseRLOF, SparseRLOF, and motion-template helpers.
- `SegmentMotion` returns rectangles through count/fill style output.

- Dense/sparse 对象释放、`Calc` 和 `CollectGarbage`。
- `DualTVL1OpticalFlow` 创建以及标量属性 get/set。
- `RLOFOpticalFlowParameter` 创建以及 solver/support-region 和数值属性 get/set。
- Dense/Sparse RLOF 对象创建、参数所有权转移和属性 get/set。
- 静态 SimpleFlow、SparseToDense、DenseRLOF、SparseRLOF 和 motion-template helper。
- `SegmentMotion` 通过 count/fill 风格输出矩形。

## BgSegm ABI / BgSegm ABI

The contrib `bgsegm` boundary uses its own opaque background-subtractor handle instead of reusing the private video-module handle. This keeps module ownership explicit while still exposing the shared `Apply`, known-foreground `Apply`, and `GetBackgroundImage` surface.

contrib `bgsegm` 边界使用自己的 opaque background-subtractor handle，而不复用 video 模块的 private handle。这样可以明确模块所有权，同时仍暴露共享的 `Apply`、known-foreground `Apply` 和 `GetBackgroundImage` 接口面。

Implemented BgSegm ABI groups:

已实现的 BgSegm ABI 分组：

- Base background-subtractor release, `Apply`, known-foreground `Apply`, and `GetBackgroundImage`.
- `BackgroundSubtractorMOG` creation and history/mixture/background/noise properties.
- `BackgroundSubtractorGMG` creation and feature/learning/threshold/update properties.
- `BackgroundSubtractorCNT` creation and stability/history/parallel properties.
- `SyntheticSequenceGenerator` creation and `GetNextFrame`.

- 基类 background-subtractor 释放、`Apply`、known-foreground `Apply` 和 `GetBackgroundImage`。
- `BackgroundSubtractorMOG` 创建以及 history/mixture/background/noise 属性。
- `BackgroundSubtractorGMG` 创建以及 feature/learning/threshold/update 属性。
- `BackgroundSubtractorCNT` 创建以及 stability/history/parallel 属性。
- `SyntheticSequenceGenerator` 创建和 `GetNextFrame`。

## Tracking ABI / Tracking ABI

The contrib `tracking` boundary keeps modern `cv::Tracker` and legacy `cv::legacy::Tracker` objects as separate opaque handles. Concrete KCF/CSRT and legacy tracker handles derive only inside native code; exported C signatures use flat rectangles and parameter structs.

contrib `tracking` 边界将 modern `cv::Tracker` 和 legacy `cv::legacy::Tracker` 作为独立 opaque handle 持有。具体 KCF/CSRT 和 legacy tracker 句柄只在 native 内部派生；导出 C 签名使用平铺矩形和参数结构。

Implemented Tracking ABI groups:

已实现的 Tracking ABI 分组：

- Modern base: release, `Init(Mat, Rect)`, and `Update(Mat, Rect)`.
- `TrackerKCF`: default/parameter creation and flat default-parameter retrieval.
- `TrackerCSRT`: default/parameter creation, flat default-parameter retrieval, and `SetInitialMask(Mat)`.
- Legacy base: release, `Init(Mat, Rect2d)`, and `Update(Mat, Rect2d)`.
- Legacy trackers: MOSSE creation, MIL creation/default params, and MedianFlow creation/default params.
- `MultiTracker`: create/release, add legacy tracker, update count/fill output, and current-object count/fill output.

- modern 基类：释放、`Init(Mat, Rect)` 和 `Update(Mat, Rect)`。
- `TrackerKCF`：默认/参数创建和平铺默认参数读取。
- `TrackerCSRT`：默认/参数创建、平铺默认参数读取和 `SetInitialMask(Mat)`。
- legacy 基类：释放、`Init(Mat, Rect2d)` 和 `Update(Mat, Rect2d)`。
- legacy tracker：MOSSE 创建、MIL 创建/默认参数，以及 MedianFlow 创建/默认参数。
- `MultiTracker`：创建/释放、添加 legacy tracker、update count/fill 输出，以及当前对象 count/fill 输出。

## Face ABI / Face ABI

The contrib `face` boundary keeps `cv::face::FaceRecognizer`, concrete recognizers, `StandardCollector`, `BIF`, `Facemark`, `FacemarkLBF`, and `MACE` behind opaque handles. Managed `Mat[]`, label arrays, face rectangles, and landmark arrays are flattened before the call; native code builds the OpenCV vectors internally.

contrib `face` 边界将 `cv::face::FaceRecognizer`、具体识别器、`StandardCollector`、`BIF`、`Facemark`、`FacemarkLBF` 和 `MACE` 保持在 opaque handle 后面。managed `Mat[]`、标签数组、人脸矩形和关键点数组在调用前被平铺；native 内部再构造 OpenCV vector。

Implemented Face ABI groups:

已实现的 Face ABI 分组：

- `FaceRecognizer`: release, train/update, label prediction, label/confidence prediction, collector prediction, read/write, empty, label info, labels-by-string, and threshold.
- `BasicFaceRecognizer`: num components, labels, eigen values, eigen vectors, mean, and projection array count/fill output.
- Concrete recognizers: EigenFace, FisherFace, and LBPH creation plus LBPH radius/neighbors/grid/threshold, labels, and histogram array count/fill output.
- `StandardCollector`: create/release, min label, min distance, and result count/fill output using a flat label/distance struct.
- `BIF`: create/release, band/rotation queries, and `Compute(Mat, Mat)`.
- `Facemark`: release, load model, save, fit, and landmark count/fill output.
- `FacemarkTrain`: add training sample, training, and detected-face count/fill output.
- `FacemarkLBF`: default and expanded parameter creation.
- `MACE`: create, load, salt, train, same, save, and empty.

- `FaceRecognizer`：释放、train/update、标签预测、标签/置信度预测、collector 预测、read/write、empty、label info、labels-by-string 和 threshold。
- `BasicFaceRecognizer`：num components、labels、eigen values、eigen vectors、mean，以及 projection 数组 count/fill 输出。
- 具体识别器：EigenFace、FisherFace、LBPH 创建，以及 LBPH radius/neighbors/grid/threshold、labels 和 histogram 数组 count/fill 输出。
- `StandardCollector`：创建/释放、min label、min distance，以及使用平铺 label/distance 结构的 result count/fill 输出。
- `BIF`：创建/释放、band/rotation 查询和 `Compute(Mat, Mat)`。
- `Facemark`：释放、加载模型、保存、fit，以及 landmark count/fill 输出。
- `FacemarkTrain`：添加训练样本、training，以及检测人脸 count/fill 输出。
- `FacemarkLBF`：默认和展开参数创建。
- `MACE`：create、load、salt、train、same、save 和 empty。

## Saliency ABI / Saliency ABI

The contrib `saliency` boundary keeps static, motion, and objectness saliency objects behind opaque handles. Inputs and outputs are caller-owned `Mat` handles; static binary-map conversion writes to a caller-owned `Mat`, while `ObjectnessBING` returns cached boxes and scores through flat count/fill arrays.

contrib `saliency` 边界将静态、运动和 objectness 显著性对象保持在 opaque handle 后面。输入与输出都是调用方持有的 `Mat` handle；静态二值图转换写入调用方持有的 `Mat`，`ObjectnessBING` 通过平铺 count/fill 数组返回缓存候选框和分数。

Implemented Saliency ABI groups:

已实现的 Saliency ABI 分组：

- Base `Saliency`: release and `ComputeSaliency(Mat, Mat)`.
- `StaticSaliency`: `ComputeBinaryMap(Mat, Mat)`.
- `StaticSaliencySpectralResidual`: create plus image width/height get/set.
- `StaticSaliencyFineGrained`: create.
- `MotionSaliencyBinWangApr2014`: create, set image size, init, and image width/height get/set.
- `ObjectnessBING`: create, training path, BB result directory, `Base`, `NSS`, `W`, compute, box count/fill, and objectness-value count/fill.

- 基类 `Saliency`：释放和 `ComputeSaliency(Mat, Mat)`。
- `StaticSaliency`：`ComputeBinaryMap(Mat, Mat)`。
- `StaticSaliencySpectralResidual`：创建以及 image width/height get/set。
- `StaticSaliencyFineGrained`：创建。
- `MotionSaliencyBinWangApr2014`：创建、set image size、init，以及 image width/height get/set。
- `ObjectnessBING`：创建、training path、BB result directory、`Base`、`NSS`、`W`、compute、box count/fill 和 objectness-value count/fill。

## Runtime Staging / Runtime 分发暂存

Windows x64 runtime files can be staged with:

Windows x64 runtime 文件可通过以下脚本暂存：

```powershell
pwsh -NoProfile -File .\scripts\Stage-Runtime.ps1
```

Current staged native files:

当前暂存的 native 文件：

`JYPPX.OpenCV.Native.dll` is the primary loader. `OpenCv5Sharp.Native.dll` is the explicitly named compatibility loader copy kept stable for already-compiled consumers. The `opencv_*500.dll` entries are factual OpenCV 5.0.0 runtime artifacts.

`JYPPX.OpenCV.Native.dll` 是主 loader。`OpenCv5Sharp.Native.dll` 是为已编译消费者保持稳定的名称明确兼容 loader 副本。`opencv_*500.dll` 条目是 OpenCV 5.0.0 runtime 的事实性产物。

- `JYPPX.OpenCV.Native.dll` (primary loader / 主 loader)
- `OpenCv5Sharp.Native.dll` (explicit compatibility loader copy kept stable for already-compiled consumers / 为已编译消费者保持稳定的明确兼容 loader 副本)
- factual OpenCV 5.0.0 runtime artifact `opencv_core500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_imgcodecs500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_imgproc500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_videoio500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_flann500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_geometry500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_calib500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_stereo500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_dnn500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_objdetect500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_photo500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_video500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_dnn500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_highgui500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_stitching500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_ptcloud500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_ml500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_img_hash500.dll` when the OpenCV contrib build includes the optional `img_hash` module
- factual OpenCV 5.0.0 runtime artifact `opencv_ximgproc500.dll` when the OpenCV contrib build includes the optional `ximgproc` module
- factual OpenCV 5.0.0 runtime artifact `opencv_optflow500.dll` when the OpenCV contrib build includes the optional `optflow` module
- factual OpenCV 5.0.0 runtime artifact `opencv_bgsegm500.dll` when the OpenCV contrib build includes the optional `bgsegm` module
- factual OpenCV 5.0.0 runtime artifact `opencv_tracking500.dll` when the OpenCV contrib build includes the optional `tracking` module
- factual OpenCV 5.0.0 runtime artifact `opencv_face500.dll` when the OpenCV contrib build includes the optional `face` module
- factual OpenCV 5.0.0 runtime artifact `opencv_saliency500.dll` when the OpenCV contrib build includes the optional `saliency` module
- factual OpenCV 5.0.0 runtime artifact `opencv_plot500.dll` when the OpenCV contrib build includes the optional `plot` module
- factual OpenCV 5.0.0 runtime artifact `opencv_shape500.dll` when the OpenCV contrib build includes the optional `shape` module
- factual OpenCV 5.0.0 runtime artifact `opencv_line_descriptor500.dll` when the OpenCV contrib build includes the optional `line_descriptor` module
- factual OpenCV 5.0.0 runtime artifact `opencv_phase_unwrapping500.dll` when the OpenCV contrib build includes the optional `phase_unwrapping` module
- factual OpenCV 5.0.0 runtime artifact `opencv_structured_light500.dll` when the OpenCV contrib build includes the optional `structured_light` module
- factual OpenCV 5.0.0 runtime artifact `opencv_intensity_transform500.dll` when the OpenCV contrib build includes the optional `intensity_transform` module
- factual OpenCV 5.0.0 runtime artifact `opencv_fuzzy500.dll` when the OpenCV contrib build includes the optional `fuzzy` module
- factual OpenCV 5.0.0 runtime artifact `opencv_hfs500.dll` when the OpenCV contrib build includes the optional `hfs` module
- factual OpenCV 5.0.0 runtime artifact `opencv_reg500.dll` when the OpenCV contrib build includes the optional `reg` module
- factual OpenCV 5.0.0 runtime artifact `opencv_surface_matching500.dll` when the OpenCV contrib build includes the optional `surface_matching` module
- factual OpenCV 5.0.0 runtime artifact `opencv_rapid500.dll` when the OpenCV contrib build includes the optional `rapid` module
- factual OpenCV 5.0.0 runtime artifact `opencv_alphamat500.dll` when the OpenCV contrib build includes the optional `alphamat` module
- factual OpenCV 5.0.0 runtime artifact `opencv_bioinspired500.dll` when the OpenCV contrib build includes the optional `bioinspired` module
- factual OpenCV 5.0.0 runtime artifact `opencv_xstereo500.dll` when the OpenCV contrib build includes the optional `xstereo` module
- factual OpenCV 5.0.0 runtime artifact `opencv_features500.dll` when the OpenCV build includes the optional `features` module
- factual OpenCV 5.0.0 runtime artifact `opencv_xfeatures2d500.dll` when the OpenCV contrib build includes the optional `xfeatures2d` module
- factual OpenCV 5.0.0 runtime artifact `opencv_xobjdetect500.dll` when the OpenCV contrib build includes the optional `xobjdetect` module
- factual OpenCV 5.0.0 runtime artifact `opencv_quality500.dll` when the OpenCV contrib build includes the optional `quality` module
- factual OpenCV 5.0.0 runtime artifact `opencv_xphoto500.dll` when the OpenCV contrib build includes the optional `xphoto` module

Runtime packages are named generically as `JYPPX.OpenCV.runtime.<rid>` for full builds and `JYPPX.OpenCV.runtime.<rid>.mini` for mini builds. The current concrete runtime package skeleton is located at `packaging/runtime/JYPPX.OpenCV.runtime`.

runtime 包使用通用命名：full build 为 `JYPPX.OpenCV.runtime.<rid>`，mini build 为 `JYPPX.OpenCV.runtime.<rid>.mini`。当前具体 runtime 包骨架位于 `packaging/runtime/JYPPX.OpenCV.runtime`。

For runtime package selection, local native runtime fallback, linked validation, and license layout, see the [Linked Runtime Build Guide](linked-runtime-build-guide.md), [Linked Runtime Smoke Guide](linked-runtime-smoke-guide.md), [Runtime Licenses](runtime-licenses.md), and [runtime package README](https://github.com/guojin-yan/OpenCV-CSharp-API/blob/opencv5.x/packaging/runtime/JYPPX.OpenCV.runtime/README.md).

runtime package 选择、local native runtime fallback、linked 验证和 license 布局见 [Linked Runtime Build Guide](linked-runtime-build-guide.md)、[Linked Runtime Smoke Guide](linked-runtime-smoke-guide.md)、[Runtime Licenses](runtime-licenses.md) 以及[runtime package README](https://github.com/guojin-yan/OpenCV-CSharp-API/blob/opencv5.x/packaging/runtime/JYPPX.OpenCV.runtime/README.md)。

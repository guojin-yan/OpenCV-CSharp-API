# Datasets Upstream Map

This map records the OpenCV 5.0.0 contrib `opencv2/datasets/*.hpp` parser closure. It is declaration-map evidence only; it does not claim that this package ships dataset assets or a datasets wrapper.

The closure contains 163 parser declarations across 32 public headers: 83 callable declarations and 80 class/struct/enum metadata declarations. The current package has no datasets native ABI entrypoints or managed public owner, so every callable is an explicit intentional omission. Dataset files, directory trees, annotations, and evaluation paths remain caller-provided external data.

The guard binds each pinned public header and the module CMake dependency boundary (`opencv_core`, `opencv_imgcodecs`, `opencv_ml`, `opencv_flann`, optional `opencv_text`), verifies zero missing declarations and zero datasets native/managed evidence, and rejects any repository-wide parity claim.

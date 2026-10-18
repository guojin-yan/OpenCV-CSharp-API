# Viz Upstream Map

This map records the OpenCV 5.0.0 contrib `opencv2/viz.hpp` and `opencv2/viz/*.hpp` parser closure. It is declaration-map evidence only; it does not claim that this package ships VTK, a GUI backend, or a viz wrapper.

The closure contains 269 parser declarations across five public subheaders: 225 callable declarations and 44 class/struct/enum metadata declarations. The current package has no viz native ABI entrypoints or managed public owner, so every callable is an explicit intentional omission. VTK availability, GUI/window state, caller-owned mesh/cloud/trajectory files, and rendering object lifetime remain external to this package contract.

The guard binds each pinned public header and the module CMake dependency boundary (`opencv_core` plus external VTK, with the upstream static-VTK conflict exclusion), verifies zero missing declarations and zero viz native/managed evidence, and rejects any repository-wide parity claim.

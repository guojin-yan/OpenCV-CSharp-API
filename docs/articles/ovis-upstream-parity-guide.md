# OVIS Upstream Map

This map records the OpenCV 5.0.0 contrib `opencv2/ovis.hpp` parser closure. It is declaration-map evidence only; it does not claim that this package builds or ships OVIS, OGRE, or a rendering backend.

The OVIS module is disabled by upstream CMake unless an external OGRE package is found at version 1.11.5 or later. Its API covers scene/window creation, renderer state, mesh/material resources, and `Mat`-backed inputs and outputs. This package has no OVIS native ABI entrypoints or managed public owner, so callable declarations remain explicit intentional omissions while classes and enums are retained as parser metadata.

The guard binds the pinned public header and module CMake file, checks the full parser closure, rejects unexplained missing declarations, and verifies zero OVIS native/managed evidence. Optional OGRE and caller-provided scene resources are not treated as package runtime support.

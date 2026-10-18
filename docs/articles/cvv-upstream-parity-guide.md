# CVV Upstream Map

This map records the OpenCV 5.0.0 contrib `opencv2/cvv.hpp` parser closure across its public CVV headers. It is declaration-map evidence only; it does not claim that this package builds or ships CVV, Qt, or an interactive debug GUI.

Upstream disables CVV unless Qt 5 or later and C++11 are available. The declarations also carry debug-window, callback, thread-local flag, and RAII finalization behavior that are outside the current managed/native package contract. All callable declarations are therefore explicit intentional omissions, while class declarations remain parser metadata.

The guard binds all seven pinned public headers and the module CMake dependency boundary, checks overload coverage for debug-match/filter/image helpers, rejects unexplained missing declarations, and verifies zero CVV native/managed evidence.

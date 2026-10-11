#!/usr/bin/env python3
"""XStereo extraction entry point using the shared contrib header parser."""

from pathlib import Path
import runpy


if __name__ == "__main__":
    shared = Path(__file__).resolve().parents[1] / "XImgProcUpstreamMap" / "extract_ximgproc.py"
    runpy.run_path(str(shared), run_name="__main__")

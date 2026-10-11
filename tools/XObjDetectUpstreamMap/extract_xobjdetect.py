#!/usr/bin/env python3
"""XObjDetect extraction entry point using the shared contrib header parser."""

from pathlib import Path
import runpy
import sys


if __name__ == "__main__":
    shared = Path(__file__).resolve().parents[1] / "XImgProcUpstreamMap" / "extract_ximgproc.py"
    sys.argv.extend(["--module", "xobjdetect"])
    runpy.run_path(str(shared), run_name="__main__")

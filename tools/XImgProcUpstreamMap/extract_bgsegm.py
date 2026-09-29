import sys
from extract_ximgproc import main

if __name__ == "__main__":
    sys.argv.extend(["--module", "bgsegm"])
    main()

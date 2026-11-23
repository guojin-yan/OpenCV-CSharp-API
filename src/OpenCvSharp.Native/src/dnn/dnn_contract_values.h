#pragma once

namespace opencv_csharp_native
{
    // These are ABI 1 values, which must survive changes to upstream enums.
    constexpr int upstream_dnn_engine(int engine, bool unified_engine)
    {
        return !unified_engine ? engine : engine == 3 ? 0 : engine == 4 ? 2 : 1;
    }

    constexpr int contract_dnn_model_format(int format, bool unified_engine)
    {
        return unified_engine && format == 2 ? 3 : format;
    }
}

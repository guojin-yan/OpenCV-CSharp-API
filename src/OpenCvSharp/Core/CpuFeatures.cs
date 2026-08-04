namespace JYPPX.OpenCvSharp.Core
{
    /// <summary>Identifies an OpenCV CPU feature for runtime capability queries.</summary>
    public enum CpuFeatures
    {
        /// <summary>No CPU feature.</summary>
        None = 0,
        /// <summary>x86 MMX.</summary>
        Mmx = 1,
        /// <summary>x86 SSE.</summary>
        Sse = 2,
        /// <summary>x86 SSE2.</summary>
        Sse2 = 3,
        /// <summary>x86 SSE3.</summary>
        Sse3 = 4,
        /// <summary>x86 Supplemental SSE3.</summary>
        Ssse3 = 5,
        /// <summary>x86 SSE4.1.</summary>
        Sse41 = 6,
        /// <summary>x86 SSE4.2.</summary>
        Sse42 = 7,
        /// <summary>x86 population count.</summary>
        Popcnt = 8,
        /// <summary>Half-precision floating point instructions.</summary>
        Fp16 = 9,
        /// <summary>x86 AVX.</summary>
        Avx = 10,
        /// <summary>x86 AVX2.</summary>
        Avx2 = 11,
        /// <summary>x86 FMA3.</summary>
        Fma3 = 12,
        /// <summary>x86 AVX-512 foundation.</summary>
        Avx512F = 13,
        /// <summary>x86 AVX-512 byte and word instructions.</summary>
        Avx512Bw = 14,
        /// <summary>x86 AVX-512 conflict detection.</summary>
        Avx512Cd = 15,
        /// <summary>x86 AVX-512 doubleword and quadword instructions.</summary>
        Avx512Dq = 16,
        /// <summary>x86 AVX-512 exponential and reciprocal instructions.</summary>
        Avx512Er = 17,

        /// <summary>Deprecated OpenCV alias for <see cref="Avx512Ifma"/>.</summary>
        Avx512Ifma512 = 18,
        /// <summary>x86 AVX-512 integer fused multiply-add.</summary>
        Avx512Ifma = 18,
        /// <summary>x86 AVX-512 prefetch instructions.</summary>
        Avx512Pf = 19,
        /// <summary>x86 AVX-512 vector byte manipulation.</summary>
        Avx512Vbmi = 20,
        /// <summary>x86 AVX-512 vector length extensions.</summary>
        Avx512Vl = 21,
        /// <summary>x86 AVX-512 vector byte manipulation version 2.</summary>
        Avx512Vbmi2 = 22,
        /// <summary>x86 AVX-512 vector neural-network instructions.</summary>
        Avx512Vnni = 23,
        /// <summary>x86 AVX-512 bit algorithms.</summary>
        Avx512Bitalg = 24,
        /// <summary>x86 AVX-512 vector population count.</summary>
        Avx512Vpopcntdq = 25,
        /// <summary>x86 AVX-512 four-register neural-network instructions.</summary>
        Avx5124Vnniw = 26,
        /// <summary>x86 AVX-512 four-register fused multiply-add instructions.</summary>
        Avx5124Fmaps = 27,
        /// <summary>x86 AVX vector neural-network instructions.</summary>
        AvxVnni = 28,
        /// <summary>ARM NEON.</summary>
        Neon = 100,
        /// <summary>ARM NEON dot-product instructions.</summary>
        NeonDotprod = 101,
        /// <summary>ARM NEON half-precision instructions.</summary>
        NeonFp16 = 102,
        /// <summary>ARM NEON bfloat16 instructions.</summary>
        NeonBf16 = 103,
        /// <summary>ARM scalable vector extension.</summary>
        Sve = 104,
        /// <summary>MIPS SIMD architecture.</summary>
        Msa = 150,
        /// <summary>RISC-V vector extension.</summary>
        RiscvV = 170,
        /// <summary>PowerPC vector-scalar extension.</summary>
        Vsx = 200,
        /// <summary>PowerPC vector-scalar extension version 3.</summary>
        Vsx3 = 201,
        /// <summary>RISC-V vector extension group.</summary>
        Rvv = 210,
        /// <summary>RISC-V vector half-precision extension.</summary>
        RvvZvfh = 211,
        /// <summary>LoongArch SIMD extension.</summary>
        Lsx = 230,
        /// <summary>LoongArch advanced SIMD extension.</summary>
        Lasx = 231,
        /// <summary>x86 AVX-512 Skylake-X group.</summary>
        Avx512Skx = 256,
        /// <summary>x86 common AVX-512 group.</summary>
        Avx512Common = 257,
        /// <summary>x86 AVX-512 Knights Landing group.</summary>
        Avx512Knl = 258,
        /// <summary>x86 AVX-512 Knights Mill group.</summary>
        Avx512Knm = 259,
        /// <summary>x86 AVX-512 Cannon Lake group.</summary>
        Avx512Cnl = 260,
        /// <summary>x86 AVX-512 Cascade Lake-X group.</summary>
        Avx512Clx = 261,
        /// <summary>x86 AVX-512 Ice Lake client group.</summary>
        Avx512Icl = 262,

        /// <summary>The highest OpenCV CPU feature identifier accepted by the native API.</summary>
        MaxFeature = 512,
    }
}

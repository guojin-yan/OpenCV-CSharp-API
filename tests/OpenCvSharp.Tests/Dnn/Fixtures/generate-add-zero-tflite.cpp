// Compile with OpenCV 5.1's modules/dnn/misc/tflite and 3rdparty/flatbuffers/include.
// This is a synthetic, weight-free fixture; no TensorFlow installation is needed.
#include "schema_generated.h"
#include <fstream>

int main(int argc, char** argv)
{
    if (argc != 2) return 1;
    using namespace opencv_tflite;
    flatbuffers::FlatBufferBuilder builder;
    const auto shape = builder.CreateVector<int32_t>({1, 2, 2, 1});
    const auto input = CreateTensor(builder, shape, TensorType_FLOAT32, 0, builder.CreateString("input"));
    const auto zero = CreateTensor(builder, shape, TensorType_FLOAT32, 1, builder.CreateString("zero"));
    const auto output = CreateTensor(builder, shape, TensorType_FLOAT32, 0, builder.CreateString("output"));
    const auto options = CreateAddOptions(builder);
    const auto op = CreateOperator(builder, 0, builder.CreateVector<int32_t>({0, 1}),
        builder.CreateVector<int32_t>({2}), BuiltinOptions_AddOptions, options.Union());
    const auto graph = CreateSubGraph(builder,
        builder.CreateVector<flatbuffers::Offset<Tensor>>({input, zero, output}),
        builder.CreateVector<int32_t>({0}), builder.CreateVector<int32_t>({2}),
        builder.CreateVector<flatbuffers::Offset<Operator>>({op}), builder.CreateString("add_zero"));
    const auto code = CreateOperatorCode(builder);
    const auto empty = CreateBuffer(builder);
    const auto zeros = CreateBuffer(builder, builder.CreateVector<uint8_t>(std::vector<uint8_t>(16, 0)));
    const auto model = CreateModel(builder, 3,
        builder.CreateVector<flatbuffers::Offset<OperatorCode>>({code}),
        builder.CreateVector<flatbuffers::Offset<SubGraph>>({graph}), builder.CreateString("OpenCV-CSharp-API"),
        builder.CreateVector<flatbuffers::Offset<Buffer>>({empty, zeros}));
    FinishModelBuffer(builder, model);
    flatbuffers::Verifier verifier(builder.GetBufferPointer(), builder.GetSize());
    if (!VerifyModelBuffer(verifier)) return 2;
    std::ofstream stream(argv[1], std::ios::binary);
    stream.write(reinterpret_cast<const char*>(builder.GetBufferPointer()), builder.GetSize());
    return stream ? 0 : 3;
}

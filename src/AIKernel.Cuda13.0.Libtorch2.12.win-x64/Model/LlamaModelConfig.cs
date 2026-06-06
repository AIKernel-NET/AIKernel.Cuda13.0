namespace AIKernel.Cuda13.Libtorch2_12.WinX64.Model;

public sealed record LlamaModelConfig(
    string ModelPath,
    int MaxInputTokens = LlamaForwardRequest.MaxInputTokens,
    int MaxOutputTokens = 64,
    string Device = "cuda");

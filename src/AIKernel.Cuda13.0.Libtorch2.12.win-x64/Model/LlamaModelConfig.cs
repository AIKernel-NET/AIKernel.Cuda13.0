namespace AIKernel.Cuda13.Libtorch2_12.WinX64.Model;

/// <include file="docs.en.xml" path="doc/members/member[@name='T:AIKernel.Cuda13.Libtorch2_12.WinX64.Model.LlamaModelConfig']" />
/// <include file="docs.ja.xml" path="doc/members/member[@name='T:AIKernel.Cuda13.Libtorch2_12.WinX64.Model.LlamaModelConfig']" />
public sealed record LlamaModelConfig(
    string ModelPath,
    int MaxInputTokens = LlamaForwardRequest.MaxInputTokens,
    int MaxOutputTokens = 64,
    string Device = "cuda");

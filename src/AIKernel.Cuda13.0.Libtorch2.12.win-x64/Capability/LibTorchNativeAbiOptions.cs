namespace AIKernel.Cuda13.Libtorch2_12.WinX64.Capability;

/// <summary>
/// [EN] Options for probing the LibTorch/CUDA native ABI boundary without loading native libraries.
/// [JA] native library を load せず LibTorch/CUDA native ABI 境界を probe するための option です。
/// </summary>
public sealed record LibTorchNativeAbiOptions
{
    /// <summary>[EN] Loader JSON environment variable. [JA] loader JSON 用環境変数です。</summary>
    public const string DefaultLoaderEnvironmentVariable = "AIKERNEL_CUDA13_LIBTORCH2_12_WIN_X64_LOADER";

    /// <summary>[EN] External LibTorch runtime environment variable. [JA] 外部 LibTorch runtime 用環境変数です。</summary>
    public const string DefaultLibTorchPathEnvironmentVariable = "AIKERNEL_LIBTORCH_PATH";

    /// <summary>[EN] Optional explicit loader JSON path. [JA] 任意の明示 loader JSON path です。</summary>
    public string? LoaderPath { get; init; }

    /// <summary>[EN] Optional explicit LibTorch runtime path. [JA] 任意の明示 LibTorch runtime path です。</summary>
    public string? LibTorchPath { get; init; }

    /// <summary>[EN] Optional explicit native bridge path. [JA] 任意の明示 native bridge path です。</summary>
    public string? NativeBridgePath { get; init; }

    /// <summary>[EN] Environment variable used when LoaderPath is not specified. [JA] LoaderPath が指定されない場合に使う環境変数です。</summary>
    public string LoaderEnvironmentVariable { get; init; } = DefaultLoaderEnvironmentVariable;

    /// <summary>[EN] Environment variable used when LibTorchPath is not specified. [JA] LibTorchPath が指定されない場合に使う環境変数です。</summary>
    public string LibTorchPathEnvironmentVariable { get; init; } = DefaultLibTorchPathEnvironmentVariable;

    /// <summary>[EN] Native bridge library name. [JA] native bridge library 名です。</summary>
    public string NativeBridgeLibraryName { get; init; } = "libtorch_bridge.dll";

    /// <summary>[EN] True when native validation / diagnostic logging should be requested. [JA] native validation / diagnostic logging を要求する場合 true です。</summary>
    public bool EnableNativeValidation { get; init; }
}

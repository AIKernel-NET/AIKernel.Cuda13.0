namespace AIKernel.Cuda13.Libtorch2_12.WinX64.Capability;

/// <summary>
/// [EN] Probe result for the LibTorch/CUDA native ABI boundary.
/// [JA] LibTorch/CUDA native ABI 境界の probe 結果です。
/// </summary>
public sealed record LibTorchNativeAbiStatus
{
    /// <summary>[EN] True when the native bridge candidate exists. [JA] native bridge 候補が存在する場合 true です。</summary>
    public bool BridgeAvailable { get; init; }

    /// <summary>[EN] True when a LibTorch runtime path is configured and exists. [JA] LibTorch runtime path が設定され存在する場合 true です。</summary>
    public bool RuntimeAvailable { get; init; }

    /// <summary>[EN] Stable status reason. [JA] 安定した status reason です。</summary>
    public required string Reason { get; init; }

    /// <summary>[EN] Resolved native bridge path when available. [JA] 利用可能な場合の解決済み native bridge path です。</summary>
    public string? ResolvedNativeBridgePath { get; init; }

    /// <summary>[EN] Resolved LibTorch runtime path when available. [JA] 利用可能な場合の解決済み LibTorch runtime path です。</summary>
    public string? ResolvedLibTorchPath { get; init; }

    /// <summary>[EN] Metadata projection for descriptors, invocations, and diagnostics. [JA] descriptor、invocation、diagnostics 用 metadata projection です。</summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
}

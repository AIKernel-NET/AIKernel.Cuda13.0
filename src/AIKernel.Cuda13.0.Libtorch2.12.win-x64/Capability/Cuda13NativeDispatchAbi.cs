namespace AIKernel.Cuda13.Libtorch2_12.WinX64.Capability;

using System.Globalization;
using System.Runtime.InteropServices;

/// <summary>
/// [EN] Canonical Cuda13 native pass identifiers mirrored from libtorch_bridge.h.
/// [JA] libtorch_bridge.h と対応する canonical Cuda13 native pass identifier です。
/// </summary>
public enum Cuda13NativePassId : uint
{
    /// <summary>[EN] Unknown pass. [JA] unknown pass です。</summary>
    Unknown = 0,

    /// <summary>[EN] Generic compute dispatch pass. [JA] 汎用 compute dispatch pass です。</summary>
    ComputeDispatch = 1,

    /// <summary>[EN] Model load operation. [JA] model load operation です。</summary>
    LoadModel = 2,

    /// <summary>[EN] Model unload operation. [JA] model unload operation です。</summary>
    UnloadModel = 3,

    /// <summary>[EN] Forward operation. [JA] forward operation です。</summary>
    Forward = 4,

    /// <summary>[EN] Canonical GPU Aisthesis raw-frame pass. [JA] canonical GPU Aisthesis raw-frame pass です。</summary>
    Aisthesis = 5,

    /// <summary>[EN] Canonical GPU Spatial Reasoning pass. [JA] canonical GPU Spatial Reasoning pass です。</summary>
    SpatialReasoning = 6,

    /// <summary>[EN] Canonical GPU HUD Composite pass. [JA] canonical GPU HUD Composite pass です。</summary>
    HudComposite = 7
}

/// <summary>
/// [EN] Fail-closed Cuda13 native dispatch status.
/// [JA] fail-closed Cuda13 native dispatch status です。
/// </summary>
public enum Cuda13NativeDispatchStatus : uint
{
    /// <summary>[EN] Dispatch completed. [JA] dispatch が完了しました。</summary>
    Ok = 0,

    /// <summary>[EN] CPU fallback is active. [JA] CPU fallback が active です。</summary>
    CpuFallback = 1,

    /// <summary>[EN] Native device is unavailable. [JA] native device が unavailable です。</summary>
    DeviceUnavailable = 2,

    /// <summary>[EN] Native device was lost. [JA] native device lost です。</summary>
    DeviceLost = 3,

    /// <summary>[EN] Native dispatch is not initialized. [JA] native dispatch は初期化されていません。</summary>
    NotInitialized = 4
}

/// <summary>
/// [EN] Fail-closed Cuda13 native dispatch failure reason mirrored from libtorch_bridge.h.
/// [JA] libtorch_bridge.h と対応する fail-closed Cuda13 native dispatch failure reason です。
/// </summary>
public enum Cuda13NativeDispatchFailureReason : uint
{
    /// <summary>[EN] No failure. [JA] failure なしです。</summary>
    None = 0,

    /// <summary>[EN] Request length was too short. [JA] request length が不足しています。</summary>
    InvalidRequestLength = 1,

    /// <summary>[EN] ABI version or header size did not match. [JA] ABI version または header size が一致しません。</summary>
    InvalidAbi = 2,

    /// <summary>[EN] Pass id is not recognized. [JA] pass id が認識できません。</summary>
    UnknownPass = 3,

    /// <summary>[EN] One-way CPU fallback is active. [JA] one-way CPU fallback が active です。</summary>
    CpuFallback = 4,

    /// <summary>[EN] Native device lost was observed. [JA] native device lost が観測されました。</summary>
    DeviceLost = 5,

    /// <summary>[EN] Native device is not ready. [JA] native device が ready ではありません。</summary>
    DeviceUnavailable = 6,

    /// <summary>[EN] Real native command submission is intentionally disabled. [JA] real native command submission は意図的に disabled です。</summary>
    CommandSubmissionDisabled = 7
}

/// <summary>
/// [EN] Stable request header for Cuda13 native command submission staging.
/// [JA] Cuda13 native command submission staging 用の安定 request header です。
/// </summary>
public sealed record Cuda13NativeDispatchRequest
{
    /// <summary>[EN] Current dispatch ABI version. [JA] 現在の dispatch ABI version です。</summary>
    public const uint CurrentAbiVersion = 1;

    /// <summary>[EN] Native request header byte size. [JA] native request header byte size です。</summary>
    public const int HeaderByteSize = 40;

    /// <summary>[EN] Native response header byte size. [JA] native response header byte size です。</summary>
    public const int ResponseHeaderByteSize = 40;

    /// <summary>[EN] ABI version. [JA] ABI version です。</summary>
    public uint AbiVersion { get; init; } = CurrentAbiVersion;

    /// <summary>[EN] Header size in bytes. [JA] header size byte 数です。</summary>
    public uint HeaderSize { get; init; } = HeaderByteSize;

    /// <summary>[EN] Pass id. [JA] pass id です。</summary>
    public Cuda13NativePassId PassId { get; init; }

    /// <summary>[EN] Dispatch flags. [JA] dispatch flags です。</summary>
    public uint Flags { get; init; }

    /// <summary>[EN] Deterministic frame index. [JA] deterministic frame index です。</summary>
    public ulong FrameIndex { get; init; }

    /// <summary>[EN] Deterministic sample ticks. [JA] deterministic sample ticks です。</summary>
    public ulong SampleTicks { get; init; }

    /// <summary>[EN] Payload size after the header. [JA] header 後続 payload size です。</summary>
    public uint PayloadBytes { get; init; }

    /// <summary>
    /// [EN] Serializes the header to the native ABI byte layout.
    /// [JA] header を native ABI byte layout に serialize します。
    /// </summary>
    /// <returns>EN: Native bytes. JA: native bytes です。</returns>
    public byte[] ToNativeBytes()
    {
        var bytes = new byte[HeaderByteSize];
        var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        try
        {
            Marshal.StructureToPtr(
                new NativeRequestHeader
                {
                    AbiVersion = AbiVersion,
                    HeaderSize = HeaderSize,
                    PassId = (uint)PassId,
                    Flags = Flags,
                    FrameIndex = FrameIndex,
                    SampleTicks = SampleTicks,
                    PayloadBytes = PayloadBytes
                },
                handle.AddrOfPinnedObject(),
                fDeleteOld: false);
        }
        finally
        {
            handle.Free();
        }

        return bytes;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRequestHeader
    {
        public uint AbiVersion;
        public uint HeaderSize;
        public uint PassId;
        public uint Flags;
        public ulong FrameIndex;
        public ulong SampleTicks;
        public uint PayloadBytes;
        public uint Reserved;
    }
}

/// <summary>
/// [EN] Stable response header from Cuda13 native command submission staging.
/// [JA] Cuda13 native command submission staging から返る安定 response header です。
/// </summary>
public sealed record Cuda13NativeDispatchResponse
{
    /// <summary>[EN] Metadata key for native dispatch ABI version. [JA] native dispatch ABI version 用 metadata key です。</summary>
    public const string AbiVersionMetadataKey = "cuda.native.dispatch.abi_version";

    /// <summary>[EN] Metadata key for native dispatch status. [JA] native dispatch status 用 metadata key です。</summary>
    public const string StatusMetadataKey = "cuda.native.dispatch.status";

    /// <summary>[EN] Metadata key for native dispatch fail-closed reason. [JA] native dispatch fail-closed reason 用 metadata key です。</summary>
    public const string FailureReasonMetadataKey = "cuda.native.dispatch.failure_reason";

    /// <summary>[EN] Metadata key for echoed frame index. [JA] echo された frame index 用 metadata key です。</summary>
    public const string FrameIndexMetadataKey = "cuda.native.dispatch.frame_index";

    /// <summary>[EN] Metadata key for echoed sample ticks. [JA] echo された sample ticks 用 metadata key です。</summary>
    public const string SampleTicksMetadataKey = "cuda.native.dispatch.sample_ticks";

    /// <summary>[EN] Metadata key for diagnostics byte count. [JA] diagnostics byte count 用 metadata key です。</summary>
    public const string DiagnosticsBytesMetadataKey = "cuda.native.dispatch.diagnostics_bytes";

    /// <summary>[EN] ABI version. [JA] ABI version です。</summary>
    public uint AbiVersion { get; init; }

    /// <summary>[EN] Header size in bytes. [JA] header size byte 数です。</summary>
    public uint HeaderSize { get; init; }

    /// <summary>[EN] Native status. [JA] native status です。</summary>
    public Cuda13NativeDispatchStatus Status { get; init; }

    /// <summary>[EN] Fail-closed reason. [JA] fail-closed reason です。</summary>
    public Cuda13NativeDispatchFailureReason FailureReason { get; init; }

    /// <summary>[EN] Echoed frame index. [JA] echo された frame index です。</summary>
    public ulong FrameIndex { get; init; }

    /// <summary>[EN] Echoed sample ticks. [JA] echo された sample ticks です。</summary>
    public ulong SampleTicks { get; init; }

    /// <summary>[EN] Diagnostics payload byte count. [JA] diagnostics payload byte count です。</summary>
    public uint DiagnosticsBytes { get; init; }

    /// <summary>
    /// [EN] Reads a response header from a native pointer.
    /// [JA] native pointer から response header を読み取ります。
    /// </summary>
    /// <param name="responsePointer">EN: Response pointer. JA: response pointer です。</param>
    /// <returns>EN: Response projection. JA: response projection です。</returns>
    public static Cuda13NativeDispatchResponse FromNativePointer(IntPtr responsePointer)
    {
        if (responsePointer == IntPtr.Zero)
        {
            return new Cuda13NativeDispatchResponse
            {
                AbiVersion = Cuda13NativeDispatchRequest.CurrentAbiVersion,
                HeaderSize = Cuda13NativeDispatchRequest.ResponseHeaderByteSize,
                Status = Cuda13NativeDispatchStatus.NotInitialized,
                FailureReason = Cuda13NativeDispatchFailureReason.InvalidRequestLength
            };
        }

        var native = Marshal.PtrToStructure<NativeResponseHeader>(responsePointer);
        return new Cuda13NativeDispatchResponse
        {
            AbiVersion = native.AbiVersion,
            HeaderSize = native.HeaderSize,
            Status = Enum.IsDefined(typeof(Cuda13NativeDispatchStatus), native.Status)
                ? (Cuda13NativeDispatchStatus)native.Status
                : Cuda13NativeDispatchStatus.NotInitialized,
            FailureReason = Enum.IsDefined(typeof(Cuda13NativeDispatchFailureReason), native.FailureReason)
                ? (Cuda13NativeDispatchFailureReason)native.FailureReason
                : Cuda13NativeDispatchFailureReason.DeviceUnavailable,
            FrameIndex = native.FrameIndex,
            SampleTicks = native.SampleTicks,
            DiagnosticsBytes = native.DiagnosticsBytes
        };
    }

    /// <summary>
    /// [EN] Projects the response into deterministic native dispatch diagnostics metadata.
    /// [JA] response を deterministic native dispatch diagnostics metadata に投影します。
    /// </summary>
    /// <returns>EN: Metadata dictionary. JA: metadata dictionary です。</returns>
    public IReadOnlyDictionary<string, string> ToMetadata()
        => new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [AbiVersionMetadataKey] = AbiVersion.ToString(CultureInfo.InvariantCulture),
            [StatusMetadataKey] = Status.ToString(),
            [FailureReasonMetadataKey] = FailureReason.ToString(),
            [FrameIndexMetadataKey] = FrameIndex.ToString(CultureInfo.InvariantCulture),
            [SampleTicksMetadataKey] = SampleTicks.ToString(CultureInfo.InvariantCulture),
            [DiagnosticsBytesMetadataKey] = DiagnosticsBytes.ToString(CultureInfo.InvariantCulture)
        };

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeResponseHeader
    {
        public uint AbiVersion;
        public uint HeaderSize;
        public uint Status;
        public uint FailureReason;
        public ulong FrameIndex;
        public ulong SampleTicks;
        public uint DiagnosticsBytes;
        public uint Reserved;
    }
}

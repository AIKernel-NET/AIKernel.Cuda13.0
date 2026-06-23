namespace AIKernel.Cuda13.Libtorch2_12.WinX64.Capability;

using AIKernel.Dtos.Gpu;

/// <summary>
/// [EN] Maps canonical rev3 operation ids and frame tokens into the staged Cuda13 native dispatch ABI.
/// [JA] canonical rev3 operation id と frame token を staged Cuda13 native dispatch ABI に写像します。
/// </summary>
public static class Cuda13NativeDispatchRequestBuilder
{
    /// <summary>
    /// [EN] Creates a staged native dispatch request from a canonical operation id.
    /// [JA] canonical operation id から staged native dispatch request を作成します。
    /// </summary>
    /// <param name="operation">EN: Canonical or package operation id. JA: canonical または package operation id です。</param>
    /// <param name="frame">EN: Optional canonical frame token. JA: optional canonical frame token です。</param>
    /// <param name="flags">EN: Native dispatch flags. JA: native dispatch flags です。</param>
    /// <param name="payloadBytes">EN: Payload byte count after the header. JA: header 後続 payload byte count です。</param>
    /// <returns>EN: Native request header projection. JA: native request header projection です。</returns>
    public static Cuda13NativeDispatchRequest Create(
        string operation,
        GpuFrameToken? frame = null,
        uint flags = 0,
        uint payloadBytes = 0)
    {
        if (!TryMapOperation(operation, out var nativePassId))
        {
            throw new ArgumentException($"Unsupported Cuda13 native operation id: {operation}", nameof(operation));
        }

        var frameIndex = frame?.FrameIndex ?? 0;
        var sampleTicks = frame?.SampleTicks ?? 0;

        if (frameIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(frame), frameIndex, "FrameIndex must be non-negative for the native ABI.");
        }

        if (sampleTicks < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(frame), sampleTicks, "SampleTicks must be non-negative for the native ABI.");
        }

        return new Cuda13NativeDispatchRequest
        {
            PassId = nativePassId,
            Flags = flags,
            FrameIndex = checked((ulong)frameIndex),
            SampleTicks = checked((ulong)sampleTicks),
            PayloadBytes = payloadBytes
        };
    }

    /// <summary>
    /// [EN] Tries to map a canonical/package operation id to a Cuda13 native pass id.
    /// [JA] canonical/package operation id を Cuda13 native pass id へ写像します。
    /// </summary>
    /// <param name="operation">EN: Operation id. JA: operation id です。</param>
    /// <param name="nativePassId">EN: Native pass id. JA: native pass id です。</param>
    /// <returns>EN: True when mapped. JA: 写像できた場合 true です。</returns>
    public static bool TryMapOperation(string? operation, out Cuda13NativePassId nativePassId)
    {
        nativePassId = operation switch
        {
            GpuOperationNames.ComputeDispatch => Cuda13NativePassId.ComputeDispatch,
            GpuOperationNames.GpuAisthesisRawFrame => Cuda13NativePassId.Aisthesis,
            GpuOperationNames.GpuSpatialReasoning => Cuda13NativePassId.SpatialReasoning,
            GpuOperationNames.GpuHudComposite => Cuda13NativePassId.HudComposite,
            "load_model" => Cuda13NativePassId.LoadModel,
            "unload_model" => Cuda13NativePassId.UnloadModel,
            "forward" => Cuda13NativePassId.Forward,
            _ => Cuda13NativePassId.Unknown
        };

        return nativePassId != Cuda13NativePassId.Unknown;
    }
}

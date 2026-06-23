namespace AIKernel.Cuda13.Libtorch2_12.WinX64.Capability;

using System.Globalization;
using AIKernel.Abstractions.Gpu;
using AIKernel.Dtos.Gpu;
using AIKernel.Enums;

/// <summary>
/// [EN] Descriptor/probe-stage diagnostics provider for the LibTorch CUDA 13 native capability package.
/// [JA] LibTorch CUDA 13 native capability package 用の descriptor/probe-stage diagnostics provider です。
/// </summary>
public sealed class LibTorchDiagnosticsProvider : IGpuDiagnostics
{
    private readonly LibTorchNativeAbiOptions _nativeAbiOptions;

    /// <summary>
    /// [EN] Creates diagnostics backed by the deterministic LibTorch/CUDA native ABI probe.
    /// [JA] deterministic LibTorch/CUDA native ABI probe に基づく diagnostics を作成します。
    /// </summary>
    /// <param name="nativeAbiOptions">EN: Optional native ABI probe options. JA: 任意の native ABI probe option です。</param>
    public LibTorchDiagnosticsProvider(LibTorchNativeAbiOptions? nativeAbiOptions = null)
        => _nativeAbiOptions = nativeAbiOptions ?? new LibTorchNativeAbiOptions();

    /// <inheritdoc />
    public ValueTask<GpuFrameDiagnostics> CaptureFrameDiagnosticsAsync(
        GpuFrameToken? frame = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(CreateFrameDiagnostics(frame));
    }

    private GpuFrameDiagnostics CreateFrameDiagnostics(GpuFrameToken? frame)
        => new()
        {
            GamePath = CreateDiagnosticsPath(GpuRev3PathRoles.Game, frame),
            BonsaiPath = CreateDiagnosticsPath(GpuRev3PathRoles.Bonsai, frame),
            HudPath = CreateDiagnosticsPath(GpuRev3PathRoles.Hud, frame),
            SensorPath = CreateDiagnosticsPath(GpuRev3PathRoles.Sensor, frame)
        };

    private GpuDiagnosticsPathInfo CreateDiagnosticsPath(
        string passId,
        GpuFrameToken? frame)
    {
        var nativeStatus = LibTorchNativeAbiProbe.Probe(_nativeAbiOptions);
        var metadata = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in LibTorchCapabilityDescriptor.Create().Metadata)
        {
            metadata[pair.Key] = pair.Value;
        }

        foreach (var pair in nativeStatus.Metadata)
        {
            metadata[pair.Key] = pair.Value;
        }

        metadata[GpuDiagnosticsMetadataKeys.Rev3AuthoritativeReady] = "false";
        metadata[GpuDiagnosticsMetadataKeys.Rev3CandidateStreak] = "0";
        metadata[GpuDiagnosticsMetadataKeys.Rev3DiagnosticReady] = "true";
        metadata[GpuDiagnosticsMetadataKeys.Rev3DiagnosticStreak] = "1";
        metadata[GpuDiagnosticsMetadataKeys.Rev3ExecutionMode] = GpuRev3ExecutionModes.NativeCudaAbiProbe;
        metadata[GpuDiagnosticsMetadataKeys.Rev3FeatureMaskStorageTexture] = "false";
        metadata[GpuDiagnosticsMetadataKeys.Rev3FrameIndex] =
            (frame?.FrameIndex ?? 0).ToString(CultureInfo.InvariantCulture);
        metadata[GpuDiagnosticsMetadataKeys.Rev3PassId] = passId;
        metadata[GpuDiagnosticsMetadataKeys.Rev3PassReadiness] = "Passes.{Aisthesis,SpatialReasoning,HudComposite}:ShaderBound=false,PipelineCached=false,BuiltInExecutor=false,InjectedExecutor=false,ReadyForBuiltIn=false";
        metadata[GpuDiagnosticsMetadataKeys.Rev3PathRole] = ResolvePathRole(passId);
        var nativeAvailable = nativeStatus.BridgeAvailable && nativeStatus.RuntimeAvailable;
        AddNativeDispatchMetadata(metadata, nativeAvailable, frame);
        metadata[GpuDiagnosticsMetadataKeys.Rev3PilotState] = nativeAvailable
            ? GpuRev3PilotStates.ProbeNativeCandidate
            : GpuRev3PilotStates.ProbeReady;
        metadata[GpuDiagnosticsMetadataKeys.Rev3PromotionGate] = GpuRev3PromotionGates.NativeBridgeRequired;
        metadata[GpuDiagnosticsMetadataKeys.Rev3RequiredStreak] = "0";
        metadata[GpuDiagnosticsMetadataKeys.Rev3SampleTicks] =
            (frame?.SampleTicks ?? 0).ToString(CultureInfo.InvariantCulture);
        AddPromotionReadinessMetadata(metadata);
        ValidateCanonicalDiagnosticsMetadata(metadata, passId);

        return new GpuDiagnosticsPathInfo
        {
            Backend = GpuBackend.Cuda.ToString(),
            ZeroCopy = false,
            Readback = GpuReadbackPolicy.RequiredFallback,
            FallbackReason = nativeAvailable
                ? "cuda-native-dispatch-not-bound"
                : nativeStatus.Reason,
            FrameId = frame?.FrameId,
            PassId = passId,
            MemoryEstimate = EstimatePathMemory(frame, passId),
            Metadata = metadata
        };
    }

    private static void AddNativeDispatchMetadata(
        SortedDictionary<string, string> metadata,
        bool nativeAvailable,
        GpuFrameToken? frame)
    {
        var response = new Cuda13NativeDispatchResponse
        {
            AbiVersion = Cuda13NativeDispatchRequest.CurrentAbiVersion,
            HeaderSize = Cuda13NativeDispatchRequest.ResponseHeaderByteSize,
            Status = Cuda13NativeDispatchStatus.NotInitialized,
            FailureReason = nativeAvailable
                ? Cuda13NativeDispatchFailureReason.CommandSubmissionDisabled
                : Cuda13NativeDispatchFailureReason.DeviceUnavailable,
            FrameIndex = (ulong)Math.Max(frame?.FrameIndex ?? 0, 0),
            SampleTicks = (ulong)Math.Max(frame?.SampleTicks ?? 0, 0)
        };

        foreach (var pair in response.ToMetadata())
        {
            metadata[pair.Key] = pair.Value;
        }
    }

    private static void AddPromotionReadinessMetadata(SortedDictionary<string, string> metadata)
    {
        var readiness = GpuCanonicalValidation.EvaluateRev3PromotionReadiness(
            new Dictionary<string, string>(metadata, StringComparer.Ordinal));
        metadata[GpuDiagnosticsMetadataKeys.Rev3PromotionBlocked] = Flag(readiness.IsBlocked);
        metadata[GpuDiagnosticsMetadataKeys.Rev3PromotionCandidateReady] = Flag(readiness.IsPromotionCandidate);
        metadata[GpuDiagnosticsMetadataKeys.Rev3PromotionDiagnosticStable] = Flag(readiness.IsDiagnosticStable);
        metadata[GpuDiagnosticsMetadataKeys.Rev3PromotionReason] = readiness.Reason;
    }

    private static void ValidateCanonicalDiagnosticsMetadata(
        SortedDictionary<string, string> metadata,
        string passId)
    {
        var validation = GpuCanonicalValidation.ValidateRev3DiagnosticsMetadata(
            metadata,
            $"{passId}.metadata");
        if (validation.IsValid)
        {
            return;
        }

        var details = string.Join(
            "; ",
            validation.Errors.Select(issue => $"{issue.Code}:{issue.Path ?? "(metadata)"}"));
        throw new InvalidOperationException(
            $"LibTorch diagnostics metadata failed canonical rev3 validation for '{passId}': {details}");
    }

    private static string Flag(bool value) => value ? "true" : "false";

    private static long? EstimatePathMemory(
        GpuFrameToken? frame,
        string passId)
    {
        var target = string.Equals(passId, GpuRev3PathRoles.Hud, StringComparison.OrdinalIgnoreCase)
            ? frame?.HudTarget ?? frame?.RawTarget
            : frame?.RawTarget;
        if (target is null || target.Width <= 0 || target.Height <= 0)
        {
            return null;
        }

        var bytesPerPixel = target.PixelFormat switch
        {
            FramePixelFormat.Indexed8 or FramePixelFormat.Luminance8 => 1,
            FramePixelFormat.Rgb24 => 3,
            FramePixelFormat.Rgba32 or FramePixelFormat.Bgra32 => 4,
            _ => 4
        };

        return (long)target.Width * target.Height * bytesPerPixel;
    }

    private static string ResolvePathRole(string passId)
    {
        return GpuRev3PathRoles.TryResolveFromPassId(passId, out var role)
            ? role
            : "unknown";
    }
}

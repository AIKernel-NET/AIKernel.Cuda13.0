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
            GamePath = CreateDiagnosticsPath(GpuPathRoles.Game, frame),
            BonsaiPath = CreateDiagnosticsPath(GpuPathRoles.Bonsai, frame),
            HudPath = CreateDiagnosticsPath(GpuPathRoles.Hud, frame),
            SensorPath = CreateDiagnosticsPath(GpuPathRoles.Sensor, frame)
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

        metadata[GpuDiagnosticsMetadataKeys.CanonicalAuthoritativeReady] = "false";
        metadata[GpuDiagnosticsMetadataKeys.CanonicalCandidateStreak] = "0";
        metadata[GpuDiagnosticsMetadataKeys.CanonicalDiagnosticReady] = "true";
        metadata[GpuDiagnosticsMetadataKeys.CanonicalDiagnosticStreak] = "1";
        metadata[GpuDiagnosticsMetadataKeys.CanonicalExecutionMode] = GpuExecutionModes.NativeCudaAbiProbe;
        metadata[GpuDiagnosticsMetadataKeys.CanonicalFeatureMaskStorageTexture] = "false";
        metadata[GpuDiagnosticsMetadataKeys.CanonicalFrameIndex] =
            (frame?.FrameIndex ?? 0).ToString(CultureInfo.InvariantCulture);
        metadata[GpuDiagnosticsMetadataKeys.CanonicalPassId] = passId;
        metadata[GpuDiagnosticsMetadataKeys.CanonicalPassReadiness] = "Passes.{Aisthesis,SpatialReasoning,HudComposite}:ShaderBound=false,PipelineCached=false,BuiltInExecutor=false,InjectedExecutor=false,ReadyForBuiltIn=false";
        metadata[GpuDiagnosticsMetadataKeys.CanonicalPathRole] = ResolvePathRole(passId);
        var nativeAvailable = nativeStatus.BridgeAvailable && nativeStatus.RuntimeAvailable;
        AddNativeDispatchMetadata(metadata, nativeAvailable, frame);
        metadata[GpuDiagnosticsMetadataKeys.CanonicalPilotState] = nativeAvailable
            ? GpuCanonicalPilotStates.ProbeNativeCandidate
            : GpuCanonicalPilotStates.ProbeReady;
        metadata[GpuDiagnosticsMetadataKeys.CanonicalPromotionGate] = GpuPromotionGates.NativeBridgeRequired;
        metadata[GpuDiagnosticsMetadataKeys.CanonicalRequiredStreak] = "0";
        metadata[GpuDiagnosticsMetadataKeys.CanonicalSampleTicks] =
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
        var readiness = GpuCanonicalValidation.EvaluatePromotionReadiness(
            new Dictionary<string, string>(metadata, StringComparer.Ordinal));
        metadata[GpuDiagnosticsMetadataKeys.CanonicalPromotionBlocked] = Flag(readiness.IsBlocked);
        metadata[GpuDiagnosticsMetadataKeys.CanonicalPromotionCandidateReady] = Flag(readiness.IsPromotionCandidate);
        metadata[GpuDiagnosticsMetadataKeys.CanonicalPromotionDiagnosticStable] = Flag(readiness.IsDiagnosticStable);
        metadata[GpuDiagnosticsMetadataKeys.CanonicalPromotionReason] = readiness.Reason;
    }

    private static void ValidateCanonicalDiagnosticsMetadata(
        SortedDictionary<string, string> metadata,
        string passId)
    {
        var validation = GpuCanonicalValidation.ValidateDiagnosticsMetadata(
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
            $"LibTorch diagnostics metadata failed canonical validation for '{passId}': {details}");
    }

    private static string Flag(bool value) => value ? "true" : "false";

    private static long? EstimatePathMemory(
        GpuFrameToken? frame,
        string passId)
    {
        var target = string.Equals(passId, GpuPathRoles.Hud, StringComparison.OrdinalIgnoreCase)
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
        return GpuPathRoles.TryResolveFromPassId(passId, out var role)
            ? role
            : "unknown";
    }
}

namespace AIKernel.Cuda13.Libtorch2_12.WinX64.Capability;

using AIKernel.Dtos.Gpu;

/// <summary>
/// [EN] Deterministic probe for the LibTorch/CUDA native ABI boundary.
/// [JA] LibTorch/CUDA native ABI 境界の決定論的 probe です。
/// </summary>
public static class LibTorchNativeAbiProbe
{
    /// <summary>
    /// [EN] Probes configured LibTorch/CUDA paths without loading the native bridge or LibTorch libraries.
    /// [JA] native bridge や LibTorch library を load せず、設定された LibTorch/CUDA path を probe します。
    /// </summary>
    /// <param name="options">EN: Probe options. JA: probe option です。</param>
    /// <returns>EN: Probe status. JA: probe status です。</returns>
    public static LibTorchNativeAbiStatus Probe(LibTorchNativeAbiOptions? options = null)
    {
        options ??= new LibTorchNativeAbiOptions();
        var loaderEnv = NormalizeName(
            options.LoaderEnvironmentVariable,
            LibTorchNativeAbiOptions.DefaultLoaderEnvironmentVariable);
        var libTorchEnv = NormalizeName(
            options.LibTorchPathEnvironmentVariable,
            LibTorchNativeAbiOptions.DefaultLibTorchPathEnvironmentVariable);
        var loaderPath = NormalizePath(options.LoaderPath) ??
            NormalizePath(Environment.GetEnvironmentVariable(loaderEnv));
        var libTorchPath = NormalizePath(options.LibTorchPath) ??
            NormalizePath(Environment.GetEnvironmentVariable(libTorchEnv));
        var bridgePath = NormalizePath(options.NativeBridgePath);

        var bridgeCandidate = ResolveBridgeCandidate(
            bridgePath,
            loaderPath,
            options.NativeBridgeLibraryName);
        var bridgeAvailable = bridgeCandidate is not null && File.Exists(bridgeCandidate);
        var runtimeAvailable = libTorchPath is not null && Directory.Exists(libTorchPath);
        var reason = ResolveReason(loaderPath, libTorchPath, bridgeCandidate, bridgeAvailable, runtimeAvailable);

        var metadata = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [GpuNativeAbiMetadataKeys.CudaNativeAbiAvailable] = (bridgeAvailable && runtimeAvailable) ? "true" : "false",
            [GpuNativeAbiMetadataKeys.CudaNativeAbiReason] = reason,
            [GpuNativeAbiMetadataKeys.CudaNativeBridgeAvailable] = bridgeAvailable ? "true" : "false",
            [GpuNativeAbiMetadataKeys.CudaNativeBridgeLibrary] = options.NativeBridgeLibraryName,
            [GpuNativeAbiMetadataKeys.CudaNativeLoaderConfigured] = loaderPath is null ? "false" : "true",
            [GpuNativeAbiMetadataKeys.CudaNativeLoaderEnvironmentVariable] = loaderEnv,
            [GpuNativeAbiMetadataKeys.CudaNativeValidation] = options.EnableNativeValidation ? "requested" : "default",
            [GpuNativeAbiMetadataKeys.CudaLibTorchPathConfigured] = libTorchPath is null ? "false" : "true",
            [GpuNativeAbiMetadataKeys.CudaLibTorchPathAvailable] = runtimeAvailable ? "true" : "false",
            [GpuNativeAbiMetadataKeys.CudaLibTorchPathEnvironmentVariable] = libTorchEnv
        };

        if (loaderPath is not null)
        {
            metadata[GpuNativeAbiMetadataKeys.CudaNativeLoaderPath] = loaderPath;
        }

        if (bridgeCandidate is not null)
        {
            metadata[GpuNativeAbiMetadataKeys.CudaNativeBridgePath] = bridgeCandidate;
        }

        if (libTorchPath is not null)
        {
            metadata[GpuNativeAbiMetadataKeys.CudaLibTorchPath] = libTorchPath;
        }

        return new LibTorchNativeAbiStatus
        {
            BridgeAvailable = bridgeAvailable,
            RuntimeAvailable = runtimeAvailable,
            Reason = reason,
            ResolvedNativeBridgePath = bridgeAvailable ? bridgeCandidate : null,
            ResolvedLibTorchPath = runtimeAvailable ? libTorchPath : null,
            Metadata = metadata
        };
    }

    private static string? ResolveBridgeCandidate(
        string? explicitBridgePath,
        string? loaderPath,
        string nativeBridgeLibraryName)
    {
        if (explicitBridgePath is not null)
            return explicitBridgePath;

        if (loaderPath is null || !File.Exists(loaderPath))
            return null;

        var loaderDirectory = Path.GetDirectoryName(loaderPath);
        if (string.IsNullOrWhiteSpace(loaderDirectory))
            return null;

        return Path.Combine(
            loaderDirectory,
            "runtimes",
            "win-x64",
            "native",
            nativeBridgeLibraryName);
    }

    private static string ResolveReason(
        string? loaderPath,
        string? libTorchPath,
        string? bridgeCandidate,
        bool bridgeAvailable,
        bool runtimeAvailable)
    {
        if (bridgeAvailable && runtimeAvailable)
            return "available";

        if (loaderPath is null && bridgeCandidate is null)
            return "loader-not-configured";

        if (bridgeCandidate is null || !bridgeAvailable)
            return "native-bridge-not-found";

        if (libTorchPath is null)
            return "libtorch-path-not-configured";

        if (!runtimeAvailable)
            return "libtorch-path-not-found";

        return "unavailable";
    }

    private static string NormalizeName(string? value, string fallback)
        => string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    private static string? NormalizePath(string? path)
        => string.IsNullOrWhiteSpace(path) ? null : path.Trim();
}

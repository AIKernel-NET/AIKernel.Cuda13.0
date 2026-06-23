namespace AIKernel.Cuda13.Libtorch2_12.WinX64.Capability;

using AIKernel.Dtos.Capabilities;
using AIKernel.Dtos.Gpu;
using AIKernel.Enums;

/// <summary>[EN] Documents this public package API member. [JA] LibTorchCapabilityDescriptor を表します。</summary>
/// <include file="docs.en.xml" path="doc/members/member[@name='T:AIKernel.Cuda13.Libtorch2_12.WinX64.Capability.LibTorchCapabilityDescriptor']" />
/// <include file="docs.ja.xml" path="doc/members/member[@name='T:AIKernel.Cuda13.Libtorch2_12.WinX64.Capability.LibTorchCapabilityDescriptor']" />
public static class LibTorchCapabilityDescriptor
{
    /// <summary>[EN] Documents this public package API member. [JA] CapabilityId 定数を取得します。</summary>
    /// <include file="docs.en.xml" path="doc/members/member[@name='F:AIKernel.Cuda13.Libtorch2_12.WinX64.Capability.LibTorchCapabilityDescriptor.CapabilityId']" />
    /// <include file="docs.ja.xml" path="doc/members/member[@name='F:AIKernel.Cuda13.Libtorch2_12.WinX64.Capability.LibTorchCapabilityDescriptor.CapabilityId']" />
    public const string CapabilityId = "libtorch.llama.cuda13.0.libtorch2.12.win-x64";
    /// <summary>[EN] Documents this public package API member. [JA] Name 定数を取得します。</summary>
    /// <include file="docs.en.xml" path="doc/members/member[@name='F:AIKernel.Cuda13.Libtorch2_12.WinX64.Capability.LibTorchCapabilityDescriptor.Name']" />
    /// <include file="docs.ja.xml" path="doc/members/member[@name='F:AIKernel.Cuda13.Libtorch2_12.WinX64.Capability.LibTorchCapabilityDescriptor.Name']" />
    public const string Name = "LibTorch Llama CUDA 13.0";
    /// <summary>[EN] Documents this public package API member. [JA] Version 定数を取得します。</summary>
    /// <include file="docs.en.xml" path="doc/members/member[@name='F:AIKernel.Cuda13.Libtorch2_12.WinX64.Capability.LibTorchCapabilityDescriptor.Version']" />
    /// <include file="docs.ja.xml" path="doc/members/member[@name='F:AIKernel.Cuda13.Libtorch2_12.WinX64.Capability.LibTorchCapabilityDescriptor.Version']" />
    public const string Version = "2.12.0";

    /// <summary>[EN] Documents this public package API member. [JA] Create を実行します。</summary>
    /// <include file="docs.en.xml" path="doc/members/member[@name='M:AIKernel.Cuda13.Libtorch2_12.WinX64.Capability.LibTorchCapabilityDescriptor.Create']" />
    /// <include file="docs.ja.xml" path="doc/members/member[@name='M:AIKernel.Cuda13.Libtorch2_12.WinX64.Capability.LibTorchCapabilityDescriptor.Create']" />
    public static CapabilityModuleDescriptor Create()
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["abi.calling_convention"] = "cdecl",
            ["abi.library"] = "libtorch_bridge",
            [GpuProviderMetadataKeys.AdapterProfile] = "cuda13",
            [GpuProviderMetadataKeys.AotCompilerHooks] = "planned-gpu-native-execution",
            [GpuProviderMetadataKeys.Backend] = "cuda13.0",
            [GpuProviderMetadataKeys.DeterministicFrameSampling] = "host-frame-token-sample-ticks",
            [GpuProviderMetadataKeys.Fallback] = "fail-closed",
            [GpuProviderMetadataKeys.GpuBypass] = "native-cuda-buffer-dispatch",
            [GpuProviderMetadataKeys.GpuBackend] = GpuBackend.Cuda.ToString(),
            [GpuProviderMetadataKeys.GpuCapabilities] = (
                GpuProviderCapabilities.SupportsCompute |
                GpuProviderCapabilities.SupportsNativeValidation |
                GpuProviderCapabilities.SupportsFrameDiagnostics).ToString(),
            [GpuProviderMetadataKeys.NativeJsBridge] = "not-required-native-provider",
            [GpuProviderMetadataKeys.PassBridge] = "native-abi",
            [GpuProviderMetadataKeys.ProviderFamily] = "aikernel.gpu.rev3",
            [GpuProviderMetadataKeys.ProviderRole] = "cuda13-native-compute",
            [GpuProviderMetadataKeys.RawCaptureSource] = "none",
            [GpuProviderMetadataKeys.Rev3] = "true",
            [GpuProviderMetadataKeys.Version] = Version,
            [GpuProviderMetadataKeys.ZeroCopyBufferHandling] = "native-cuda-device-buffer",
            [GpuDiagnosticsMetadataKeys.Rev3ExecutionMode] = GpuRev3ExecutionModes.NativeCudaAbi,
            [GpuDiagnosticsMetadataKeys.Rev3PassId] = GpuOperationNames.ComputeDispatch,
            ["libtorch.version"] = "2.12.0",
            ["cuda.version"] = "13.0",
            ["os"] = "win",
            ["rid"] = "win-x64",
            ["package.id"] = "AIKernel.Cuda13.0.Libtorch2.12.win-x64",
            ["runtime.win-x64"] = "runtime/win-x64/libtorch",
            ["runtime.env"] = LibTorchNativeAbiOptions.DefaultLibTorchPathEnvironmentVariable,
            ["loader.config"] = "loader.json",
            ["loader.env"] = LibTorchNativeAbiOptions.DefaultLoaderEnvironmentVariable,
        };

        foreach (var pair in LibTorchNativeAbiProbe.Probe().Metadata)
        {
            metadata[pair.Key] = pair.Value;
        }

        return new CapabilityModuleDescriptor(
            CapabilityId: CapabilityId,
            Name: Name,
            Kind: CapabilityModuleKind.NativeLibrary,
            InvocationMode: CapabilityInvocationMode.NativeAbi,
            Version: Version,
            EntryPoint: "libtorch_bridge",
            ArtifactUri: null,
            ArtifactHash: null,
            ProvidedOperations:
            [
                GpuOperationNames.ComputeDispatch,
                "load_model",
                "unload_model",
                "forward"
            ],
            RequiredPermissions:
            [
                GpuPermissionNames.ComputeExecute,
                GpuPermissionNames.BufferRead,
                GpuPermissionNames.BufferWrite,
                "native-abi",
                "gpu.cuda",
                "filesystem.read"
            ],
            Metadata: metadata);
    }
}

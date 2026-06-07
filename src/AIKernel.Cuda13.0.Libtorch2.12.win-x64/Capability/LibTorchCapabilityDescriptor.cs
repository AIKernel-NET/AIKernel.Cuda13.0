namespace AIKernel.Cuda13.Libtorch2_12.WinX64.Capability;

using AIKernel.Dtos.Capabilities;
using AIKernel.Enums;

/// <include file="docs.en.xml" path="doc/members/member[@name='T:AIKernel.Cuda13.Libtorch2_12.WinX64.Capability.LibTorchCapabilityDescriptor']" />
/// <include file="docs.ja.xml" path="doc/members/member[@name='T:AIKernel.Cuda13.Libtorch2_12.WinX64.Capability.LibTorchCapabilityDescriptor']" />
public static class LibTorchCapabilityDescriptor
{
    /// <include file="docs.en.xml" path="doc/members/member[@name='F:AIKernel.Cuda13.Libtorch2_12.WinX64.Capability.LibTorchCapabilityDescriptor.CapabilityId']" />
    /// <include file="docs.ja.xml" path="doc/members/member[@name='F:AIKernel.Cuda13.Libtorch2_12.WinX64.Capability.LibTorchCapabilityDescriptor.CapabilityId']" />
    public const string CapabilityId = "libtorch.llama.cuda13.0.libtorch2.12.win-x64";
    /// <include file="docs.en.xml" path="doc/members/member[@name='F:AIKernel.Cuda13.Libtorch2_12.WinX64.Capability.LibTorchCapabilityDescriptor.Name']" />
    /// <include file="docs.ja.xml" path="doc/members/member[@name='F:AIKernel.Cuda13.Libtorch2_12.WinX64.Capability.LibTorchCapabilityDescriptor.Name']" />
    public const string Name = "LibTorch Llama CUDA 13.0";
    /// <include file="docs.en.xml" path="doc/members/member[@name='F:AIKernel.Cuda13.Libtorch2_12.WinX64.Capability.LibTorchCapabilityDescriptor.Version']" />
    /// <include file="docs.ja.xml" path="doc/members/member[@name='F:AIKernel.Cuda13.Libtorch2_12.WinX64.Capability.LibTorchCapabilityDescriptor.Version']" />
    public const string Version = "2.12.0";

    /// <include file="docs.en.xml" path="doc/members/member[@name='M:AIKernel.Cuda13.Libtorch2_12.WinX64.Capability.LibTorchCapabilityDescriptor.Create']" />
    /// <include file="docs.ja.xml" path="doc/members/member[@name='M:AIKernel.Cuda13.Libtorch2_12.WinX64.Capability.LibTorchCapabilityDescriptor.Create']" />
    public static CapabilityModuleDescriptor Create()
    {
        return new CapabilityModuleDescriptor(
            CapabilityId: CapabilityId,
            Name: Name,
            Kind: CapabilityModuleKind.NativeLibrary,
            InvocationMode: CapabilityInvocationMode.NativeAbi,
            Version: Version,
            EntryPoint: "libtorch_bridge",
            ArtifactUri: null,
            ArtifactHash: null,
            ProvidedOperations: ["load_model", "unload_model", "forward"],
            RequiredPermissions:
            [
                "native-abi",
                "gpu.cuda",
                "filesystem.read"
            ],
            Metadata: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["abi.calling_convention"] = "cdecl",
                ["abi.library"] = "libtorch_bridge",
                ["libtorch.version"] = "2.12.0",
                ["cuda.version"] = "13.0",
                ["os"] = "win",
                ["rid"] = "win-x64",
                ["package.id"] = "AIKernel.Cuda13.0.Libtorch2.12.win-x64",
                ["runtime.win-x64"] = "runtime/win-x64/libtorch",
                ["runtime.env"] = "AIKERNEL_LIBTORCH_PATH",
                ["loader.config"] = "loader.json",
                ["loader.env"] = "AIKERNEL_CUDA13_LIBTORCH2_12_WIN_X64_LOADER"
            });
    }
}

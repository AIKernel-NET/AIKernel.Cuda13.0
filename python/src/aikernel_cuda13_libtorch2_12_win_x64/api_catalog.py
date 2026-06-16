"""[EN]
Static public managed API catalog generated from the C# source tree.

[JA]
C# source tree から生成した public managed API の静的 catalog です。
"""

from __future__ import annotations

from dataclasses import dataclass


@dataclass(frozen=True)
class ManagedMemberDescriptor:
    """[EN] Describes one public managed member discovered from C# source.

    [JA] C# source から検出した public managed member を表します。
    """

    kind: str
    name: str
    signature: str


@dataclass(frozen=True)
class ManagedTypeDescriptor:
    """[EN] Describes one public managed type exposed by the package.

    [JA] package が公開する public managed type を表します。
    """

    namespace: str
    name: str
    kind: str
    assembly: str
    source: str
    members: tuple[ManagedMemberDescriptor, ...] = ()

    @property
    def full_name(self) -> str:
        """[EN] Return the namespace-qualified managed type name.

        [JA] namespace で修飾された managed type 名を返します。
        """
        return f"{self.namespace}.{self.name}" if self.namespace else self.name


_CATALOG: tuple[ManagedTypeDescriptor, ...] = (
    ManagedTypeDescriptor(
        namespace='AIKernel.Cuda13.Libtorch2_12.WinX64.Capability',
        name='LibTorchCapabilityDescriptor',
        kind='class',
        assembly='AIKernel.Cuda13.0.Libtorch2.12.win-x64',
        source='AIKernel.Cuda13.0/src/AIKernel.Cuda13.0.Libtorch2.12.win-x64/Capability/LibTorchCapabilityDescriptor.cs',
        members=(
            ManagedMemberDescriptor('method', 'Create', 'public static CapabilityModuleDescriptor Create()'),
            ManagedMemberDescriptor('method', 'CapabilityModuleDescriptor', 'return new CapabilityModuleDescriptor('),
        ),
    ),
    ManagedTypeDescriptor(
        namespace='AIKernel.Cuda13.Libtorch2_12.WinX64.Capability',
        name='LibTorchCapabilityInvoker',
        kind='class',
        assembly='AIKernel.Cuda13.0.Libtorch2.12.win-x64',
        source='AIKernel.Cuda13.0/src/AIKernel.Cuda13.0.Libtorch2.12.win-x64/Capability/LibTorchCapabilityInvoker.cs',
        members=(
            ManagedMemberDescriptor('method', 'InvokeAsync', 'public ValueTask<CapabilityInvocationResult> InvokeAsync('),
            ManagedMemberDescriptor('method', 'CapabilityInvocationResult', 'return new CapabilityInvocationResult('),
            ManagedMemberDescriptor('method', 'Success', 'public static MappedModelPath Success('),
            ManagedMemberDescriptor('method', 'Fail', 'public static MappedModelPath Fail('),
        ),
    ),
    ManagedTypeDescriptor(
        namespace='AIKernel.Cuda13.Libtorch2_12.WinX64.Interop',
        name='SafeLlamaModelHandle',
        kind='class',
        assembly='AIKernel.Cuda13.0.Libtorch2.12.win-x64',
        source='AIKernel.Cuda13.0/src/AIKernel.Cuda13.0.Libtorch2.12.win-x64/Interop/SafeHandles.cs',
        members=(
            ManagedMemberDescriptor('method', 'SetHandle', 'SetHandle(new IntPtr(nativeHandle));'),
            ManagedMemberDescriptor('method', 'ReleaseHandle', 'protected override bool ReleaseHandle()'),
        ),
    ),
    ManagedTypeDescriptor(
        namespace='AIKernel.Cuda13.Libtorch2_12.WinX64.Model',
        name='LlamaForwardRequest',
        kind='record',
        assembly='AIKernel.Cuda13.0.Libtorch2.12.win-x64',
        source='AIKernel.Cuda13.0/src/AIKernel.Cuda13.0.Libtorch2.12.win-x64/Model/LlamaForwardRequest.cs',
        members=(
            ManagedMemberDescriptor('method', 'TryCreate', 'public static LlamaForwardRequestParseResult TryCreate('),
            ManagedMemberDescriptor('method', 'LlamaForwardRequestParseResult', 'public sealed record LlamaForwardRequestParseResult('),
            ManagedMemberDescriptor('method', 'Success', 'public static LlamaForwardRequestParseResult Success('),
            ManagedMemberDescriptor('method', 'LlamaForwardRequestParseResult', 'return new LlamaForwardRequestParseResult(true, value, null);'),
            ManagedMemberDescriptor('method', 'Fail', 'public static LlamaForwardRequestParseResult Fail('),
            ManagedMemberDescriptor('method', 'LlamaForwardRequestParseResult', 'return new LlamaForwardRequestParseResult(false, null, errorMessage);'),
        ),
    ),
    ManagedTypeDescriptor(
        namespace='AIKernel.Cuda13.Libtorch2_12.WinX64.Model',
        name='LlamaForwardRequestParseResult',
        kind='record',
        assembly='AIKernel.Cuda13.0.Libtorch2.12.win-x64',
        source='AIKernel.Cuda13.0/src/AIKernel.Cuda13.0.Libtorch2.12.win-x64/Model/LlamaForwardRequest.cs',
        members=(
            ManagedMemberDescriptor('method', 'Success', 'public static LlamaForwardRequestParseResult Success('),
            ManagedMemberDescriptor('method', 'Fail', 'public static LlamaForwardRequestParseResult Fail('),
        ),
    ),
    ManagedTypeDescriptor(
        namespace='AIKernel.Cuda13.Libtorch2_12.WinX64.Model',
        name='LlamaForwardResult',
        kind='record',
        assembly='AIKernel.Cuda13.0.Libtorch2.12.win-x64',
        source='AIKernel.Cuda13.0/src/AIKernel.Cuda13.0.Libtorch2.12.win-x64/Model/LlamaForwardResult.cs',
        members=(
            ManagedMemberDescriptor('method', 'Hash', 'Hash(material));'),
        ),
    ),
    ManagedTypeDescriptor(
        namespace='AIKernel.Cuda13.Libtorch2_12.WinX64.Model',
        name='LlamaModelConfig',
        kind='record',
        assembly='AIKernel.Cuda13.0.Libtorch2.12.win-x64',
        source='AIKernel.Cuda13.0/src/AIKernel.Cuda13.0.Libtorch2.12.win-x64/Model/LlamaModelConfig.cs',
    ),
)


def managed_api_catalog() -> tuple[ManagedTypeDescriptor, ...]:
    """[EN] Return the generated public managed API catalog.

    [JA] 生成済み public managed API catalog を返します。
    """
    return _CATALOG


def managed_type_names() -> tuple[str, ...]:
    """[EN] Return all namespace-qualified managed type names.

    [JA] namespace 修飾済み managed type 名をすべて返します。
    """
    return tuple(item.full_name for item in _CATALOG)


def find_managed_type(full_name: str) -> ManagedTypeDescriptor | None:
    """[EN] Find a managed type descriptor by namespace-qualified name.

    [JA] namespace 修飾名から managed type descriptor を検索します。
    """
    for item in _CATALOG:
        if item.full_name == full_name:
            return item
    return None


def managed_api_summary() -> dict[str, int]:
    """[EN] Return public managed API counts by assembly.

    [JA] assembly ごとの public managed API 件数を返します。
    """
    summary: dict[str, int] = {}
    for item in _CATALOG:
        summary[item.assembly] = summary.get(item.assembly, 0) + 1
    return dict(sorted(summary.items()))


__all__ = [
    "ManagedMemberDescriptor",
    "ManagedTypeDescriptor",
    "find_managed_type",
    "managed_api_catalog",
    "managed_api_summary",
    "managed_type_names",
]

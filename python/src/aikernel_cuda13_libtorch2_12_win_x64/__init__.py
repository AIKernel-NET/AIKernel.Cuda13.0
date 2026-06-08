"""[EN]
Reference module for aikernel_cuda13_libtorch2_12_win_x64.__init__.

[JA]
aikernel_cuda13_libtorch2_12_win_x64.__init__ の参照モジュールです。
"""

from __future__ import annotations

from dataclasses import dataclass
from importlib.resources import files
from .loader import (
    LOADER_ENV,
    LoaderConfig,
    default_loader_json_path,
    load_loader_config,
)

__version__ = "0.1.0"

PACKAGE_NAME = "aikernel-cuda13-libtorch2-12-win-x64"
DEV_PACKAGE_NAME = "aikernel-cuda13-libtorch2-12-win-x64-dev"
IMPORT_NAME = "aikernel_cuda13_libtorch2_12_win_x64"
NUGET_PACKAGE_ID = "AIKernel.Cuda13.0.Libtorch2.12.win-x64"
CUDA_VERSION = "13.0"
LIBTORCH_VERSION = "2.12.0"
RID = "win-x64"
OS = "windows"
REPOSITORY_URL = "https://github.com/AIKernel-NET/AIKernel.Cuda13.0"
RELEASES_URL = f"{REPOSITORY_URL}/releases"


@dataclass(frozen=True)
class CapabilityPackage:
    """[EN]
    Represents the CapabilityPackage public Python API surface.
    
    [JA]
    CapabilityPackage の公開 Python API サーフェスを表します。
    """

    package_name: str
    dev_package_name: str
    import_name: str
    nuget_package_id: str
    version: str
    cuda_version: str
    libtorch_version: str
    rid: str
    os: str
    repository_url: str
    releases_url: str


def package() -> CapabilityPackage:
    """[EN]
    Executes the package operation.
    Returns:
        Result produced by the operation.
    
    [JA]
    package 操作を実行します。
    戻り値:
        操作によって生成される結果です。
    """

    return CapabilityPackage(
        package_name=PACKAGE_NAME,
        dev_package_name=DEV_PACKAGE_NAME,
        import_name=IMPORT_NAME,
        nuget_package_id=NUGET_PACKAGE_ID,
        version=__version__,
        cuda_version=CUDA_VERSION,
        libtorch_version=LIBTORCH_VERSION,
        rid=RID,
        os=OS,
        repository_url=REPOSITORY_URL,
        releases_url=RELEASES_URL,
    )


def capability_descriptor() -> dict[str, str]:
    """[EN]
    Executes the capability descriptor operation.
    Returns:
        Result produced by the operation.
    
    [JA]
    capability descriptor 操作を実行します。
    戻り値:
        操作によって生成される結果です。
    """

    info = package()
    return {
        "package_name": info.package_name,
        "dev_package_name": info.dev_package_name,
        "import_name": info.import_name,
        "nuget_package_id": info.nuget_package_id,
        "version": info.version,
        "cuda_version": info.cuda_version,
        "libtorch_version": info.libtorch_version,
        "rid": info.rid,
        "os": info.os,
        "repository_url": info.repository_url,
        "releases_url": info.releases_url,
        "loader_env": LOADER_ENV,
        "loader_json": default_loader_json_path(),
    }


def install_instructions(version: str | None = None) -> str:
    """[EN]
    Executes the install instructions operation.
    Args:
        version: Input value for install instructions.
    
    Returns:
        Result produced by the operation.
    
    [JA]
    install instructions 操作を実行します。
    引数:
        version: install instructions に渡す入力値です。
    
    戻り値:
        操作によって生成される結果です。
    """

    resolved_version = version or __version__
    return "\n".join(
        [
            "Install the lightweight C# NuGet package:",
            f"dotnet add package {NUGET_PACKAGE_ID} --version {resolved_version}",
            "",
            "Download and extract the matching runtime zip from:",
            RELEASES_URL,
            "",
            "If the archive is not extracted beside the application, set:",
            "AIKERNEL_CUDA13_LIBTORCH2_12_WIN_X64_LOADER=<path-to-loader.json>",
        ]
    )


def bundled_managed_assemblies() -> tuple[str, ...]:
    """[EN]
    Executes the bundled managed assemblies operation.
    Returns:
        Result produced by the operation.
    
    [JA]
    bundled managed assemblies 操作を実行します。
    戻り値:
        操作によって生成される結果です。
    """

    managed = files(__package__).joinpath("managed")
    if not managed.is_dir():
        return ()

    return tuple(str(path) for path in managed.iterdir() if path.name.lower().endswith(".dll"))


def bundled_native_libraries() -> tuple[str, ...]:
    """[EN]
    Executes the bundled native libraries operation.
    Returns:
        Result produced by the operation.
    
    [JA]
    bundled native libraries 操作を実行します。
    戻り値:
        操作によって生成される結果です。
    """

    native = files(__package__).joinpath("native", "win-x64")
    if not native.is_dir():
        return ()

    return tuple(str(path) for path in native.iterdir() if path.name.lower().endswith(".dll"))


__all__ = [
    "CUDA_VERSION",
    "DEV_PACKAGE_NAME",
    "IMPORT_NAME",
    "LIBTORCH_VERSION",
    "NUGET_PACKAGE_ID",
    "OS",
    "PACKAGE_NAME",
    "RELEASES_URL",
    "RID",
    "CapabilityPackage",
    "LOADER_ENV",
    "LoaderConfig",
    "__version__",
    "bundled_managed_assemblies",
    "bundled_native_libraries",
    "capability_descriptor",
    "default_loader_json_path",
    "install_instructions",
    "load_loader_config",
    "package",
]

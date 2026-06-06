"""AIKernel CUDA 13.0 LibTorch 2.12 win-x64 Capability metadata."""

from __future__ import annotations

from dataclasses import dataclass
from importlib.resources import files
from .loader import (
    LOADER_ENV,
    LoaderConfig,
    default_loader_json_path,
    load_loader_config,
)

__version__ = "0.0.5"

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
    """Stable package identity for the external CUDA Capability."""

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
    """Return the pip package and C# NuGet runtime identity."""

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
    """Return a serializable descriptor for Python tooling."""

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
    """Return CUDA package installation instructions for C# consumers."""

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
    """Return managed DLLs bundled into the pip wheel, if present."""

    managed = files(__package__).joinpath("managed")
    if not managed.is_dir():
        return ()

    return tuple(str(path) for path in managed.iterdir() if path.name.lower().endswith(".dll"))


def bundled_native_libraries() -> tuple[str, ...]:
    """Return native bridge DLLs bundled into the pip wheel, if present."""

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

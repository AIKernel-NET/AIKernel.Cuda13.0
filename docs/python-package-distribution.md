# Python Package Distribution

[日本語](python-package-distribution-ja.md)

This repository publishes a lightweight Python package for the CUDA Capability.
Python distribution is independent from NuGet: NuGet packages are for C#
consumers, while pip packages are for Python consumers.

The Python package is not embedded in the NuGet package. Release wheels carry
the same lightweight runtime surface as the NuGet package: managed Capability
DLL, native bridge DLL, and `loader.json`. They do not carry the CUDA runtime
payload.

## Stable Channel

Stable Python releases are published to PyPI.

| Field | Value |
| --- | --- |
| Distribution | `aikernel-cuda13-libtorch2-12-win-x64` |
| Import name | `aikernel_cuda13_libtorch2_12_win_x64` |
| Version line | `0.1.0 -> ...` |
| Contents | Capability metadata, managed Capability DLL, `libtorch_bridge.dll`, bundled `loader.json`, loader helpers, and installation guidance |

Install:

```bash
pip install aikernel-cuda13-libtorch2-12-win-x64
```

Use:

```python
import aikernel_cuda13_libtorch2_12_win_x64 as cuda_capability

print(cuda_capability.capability_descriptor())
print(cuda_capability.install_instructions())

config = cuda_capability.load_loader_config()
print(config.resolved_runtime_search_paths())
print(cuda_capability.bundled_managed_assemblies())
print(cuda_capability.bundled_native_libraries())
```

## Development Channel

Development Python wheels use the package identity
`aikernel-cuda13-libtorch2-12-win-x64-dev` and version numbers such as
`0.1.0.dev1`.

GitHub Packages does not provide a PyPI registry. For pip users, development
wheels are distributed as GitHub Release assets or installed directly from the
repository:

```bash
pip install git+https://github.com/AIKernel-NET/AIKernel.Cuda13.0.git#subdirectory=python
```

Use development wheels only for CI/CD and compatibility testing. Breaking
changes are allowed on the development channel.

## Runtime Payload Boundary

The heavy executable CUDA runtime remains outside the Python package and outside
the PyPI artifact:

- LibTorch CUDA DLLs
- CUDA runtime and cuDNN redistributables
- Full GitHub Release runtime archive

Those assets are attached to GitHub Releases as the full runtime `.zip`. The
PyPI package is a stable wrapper, discovery, and tooling package only. C#
consumers use NuGet; Python consumers use pip.

The PyPI package does include a `loader.json` template so Python tooling can
inspect and generate matching loader configuration. Release wheels also include
the managed Capability DLL and native bridge DLL. They do not include LibTorch,
CUDA, cuDNN, or cuBLAS runtime binaries.

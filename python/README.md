# aikernel-cuda13-libtorch2-12-win-x64

[日本語](README-ja.md)

Python wrapper package for the AIKernel CUDA 13.0 + LibTorch 2.12 Windows
`win-x64` Capability.

Install the stable package from PyPI:

```bash
pip install aikernel-cuda13-libtorch2-12-win-x64
```

Import it as:

```python
import aikernel_cuda13_libtorch2_12_win_x64 as cuda_capability

descriptor = cuda_capability.capability_descriptor()
print(descriptor["package_id"])
```

This Python package is intentionally lightweight and is distributed through
pip. Release wheels include the CUDA Capability managed assembly,
`libtorch_bridge.dll`, and `loader.json`. They do not include LibTorch, CUDA
runtime DLLs, cuDNN, or cuBLAS. The executable CUDA runtime snapshot is
distributed as the full GitHub Release runtime archive.

Use this package to discover the Capability identity, supported runtime, and
installation guidance from Python tooling. NuGet is reserved for C# consumers;
Python wrappers are not embedded in NuGet packages.

The package includes a bundled `loader.json` template and loader helpers:

```python
config = cuda_capability.load_loader_config()
print(config.native_library)
print(config.resolved_runtime_search_paths())
```

This is lightweight runtime support only. The pip package ships the AIKernel
managed Capability assembly and native bridge, but does not ship LibTorch,
CUDA, cuDNN, or cuBLAS runtime binaries.

Release wheels expose bundled paths when those binaries are present:

```python
print(cuda_capability.bundled_managed_assemblies())
print(cuda_capability.bundled_native_libraries())
```

Development wheels are published as GitHub Release assets, not to PyPI. PyPI is
reserved for stable releases.

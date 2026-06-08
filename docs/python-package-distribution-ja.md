# Python Package Distribution

[English](python-package-distribution.md)

この repository は CUDA Capability 用の lightweight Python package を公開します。
Python distribution は NuGet から独立しています。NuGet package は C# consumer 向け、
pip package は Python consumer 向けです。

Python package は NuGet package に埋め込みません。Release wheel は NuGet package と
同じ lightweight runtime surface を持ちます。managed Capability DLL、native bridge DLL、
`loader.json` を含みますが、CUDA runtime payload は含みません。

## Stable Channel

Stable Python release は PyPI に公開します。

| Field | Value |
| --- | --- |
| Distribution | `aikernel-cuda13-libtorch2-12-win-x64` |
| Import name | `aikernel_cuda13_libtorch2_12_win_x64` |
| Version line | `0.1.0 -> ...` |
| Contents | Capability metadata, managed Capability DLL, `libtorch_bridge.dll`, bundled `loader.json`, loader helpers, installation guidance |

Install:

```bash
pip install aikernel-cuda13-libtorch2-12-win-x64
```

Use:

```python
import aikernel_cuda13_libtorch2_12_win_x64 as cuda_capability

print(cuda_capability.capability_descriptor())
print(cuda_capability.install_instructions())
```

## Development Channel

Development Python wheel は `aikernel-cuda13-libtorch2-12-win-x64-dev` と
`0.1.0.dev1` のような version number を使います。

GitHub Packages は PyPI registry を提供しません。pip user 向け development wheel は
GitHub Release asset として配布するか、repository から直接 install します。

```bash
pip install git+https://github.com/AIKernel-NET/AIKernel.Cuda13.0.git#subdirectory=python
```

## Runtime Payload Boundary

重い executable CUDA runtime は Python package と PyPI artifact の外に置きます。

- LibTorch CUDA DLLs
- CUDA runtime and cuDNN redistributables
- Full GitHub Release runtime archive

PyPI package は stable wrapper、discovery、tooling package です。C# consumer は NuGet、
Python consumer は pip を使います。

PyPI package は `loader.json` template を含みます。release wheel は managed Capability
DLL と native bridge DLL も含みますが、LibTorch、CUDA、cuDNN、cuBLAS runtime binary は
含みません。

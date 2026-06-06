# AIKernel.Cuda13.0.Libtorch2.12.win-x64

Reference external AIKernel Capability module for one runtime combination:
Windows `win-x64`, LibTorch 2.12.0, and CUDA 13.0. This repository owns the
CUDA-specific managed invoker, native C ABI bridge, loader configuration, and
runtime metadata that were intentionally separated from AIKernel.Core.

AIKernel.Core is CUDA-free. Install this package only on trusted GPU hosts that
explicitly opt in to CUDA execution.

For the full split-distribution rules, see
[`docs/package-distribution.md`](docs/package-distribution.md).

## Package Model

This Capability has two C# runtime artifacts and one Python wrapper artifact:

- **Lightweight NuGet package**: published to NuGet.org for C# consumers. It
  contains the managed assembly, `libtorch_bridge.dll`, `loader.json`, and
  dynamic loading logic. It does not contain LibTorch, CUDA, cuDNN, cuBLAS, or
  other large runtime DLLs.
- **Full runtime archive**: published as a GitHub Release `.zip`. It includes
  LibTorch CUDA, CUDA Runtime, cuDNN, cuBLAS, `libtorch_bridge.dll`, and an
  auto-configured `loader.json`.
- **pip package**: published to PyPI for Python consumers. It is not embedded in
  the NuGet package.

NuGet is the C# distribution channel. pip is the Python distribution channel.
The GitHub Release archive carries the large CUDA runtime snapshot.

## Install The Lightweight NuGet Package

For C# consumers:

```powershell
dotnet add package AIKernel.Cuda13.0.Libtorch2.12.win-x64 --version 0.0.5
```

The managed package exposes:

- `LibTorchCapabilityDescriptor.Create()`
- `LibTorchCapabilityInvoker`
- C ABI operations: `load_model`, `unload_model`, `forward`

The NuGet package includes `loader.json`. Configure it by setting one of:

- `AIKERNEL_LIBTORCH_PATH`
- `AIKERNEL_CUDA13_LIBTORCH2_12_WIN_X64_HOME`
- `AIKERNEL_CUDA13_LIBTORCH2_12_WIN_X64_LOADER`

## Install The Full Runtime Archive

Download the full runtime archive from:

```text
https://github.com/AIKernel-NET/AIKernel.Cuda13.0/releases
```

Extract it beside the consuming application, or set
`AIKERNEL_CUDA13_LIBTORCH2_12_WIN_X64_LOADER` to the extracted `loader.json`.
The archive layout is designed so the default relative paths work after
extraction:

```text
loader.json
runtimes/win-x64/native/libtorch_bridge.dll
runtime/win-x64/libtorch/**
```

The public C ABI is stable. Do not expose LibTorch, CUDA, or C++ types across
the ABI boundary.

## Native Build

Windows/MSVC `win-x64` is the only supported native build target in this
repository revision. Do not add Linux, macOS, ARM64, or other CUDA/LibTorch
combinations to this package.

Place LibTorch 2.12.0 + CUDA 13.0 under one of:

- `runtime/win-x64/libtorch`
- `ref/libtorch-win-shared-with-deps-2.12.0+cu130/libtorch` in the parent
  workspace
- `AIKERNEL_LIBTORCH_PATH`

`native/CMakeLists.txt` also reads `../ref/env.txt` from the parent workspace
for `CUDA_PATH` when present.

```powershell
cmake -S native -B native/build/win-x64 -A x64
cmake --build native/build/win-x64 --config Release
```

The package project uses this default when packing the lightweight NuGet native
bridge:

- `AIKernelNativeBridgePath=native/build/win-x64/Release/libtorch_bridge.dll`

Override this MSBuild property if your CI places the native bridge elsewhere.

## Release Verification

Before publishing, verify both managed and native surfaces:

```powershell
dotnet test AIKernel.Cuda13.0.Libtorch2.12.win-x64.slnx -c Release --no-restore
dotnet pack AIKernel.Cuda13.0.Libtorch2.12.win-x64.slnx -c Release --no-restore
cmake --build native/build/win-x64 --config Release
```

The NuGet.org lightweight `.nupkg` should contain:

- `lib/net10.0/AIKernel.Cuda13.0.Libtorch2.12.win-x64.dll`
- `runtimes/win-x64/native/libtorch_bridge.dll`
- `loader.json`
- `LICENSE`
- `README.md`

It must not contain LibTorch, CUDA, cuDNN, cuBLAS, or other large runtime DLLs.

The GitHub Release runtime `.zip` should contain:

- `runtimes/win-x64/native/libtorch_bridge.dll`
- `runtime/win-x64/libtorch/**`
- `loader.json`
- `LICENSE`
- `README.md`
- `RELEASE_NOTES.md`
- `LICENSE-THIRD-PARTY/*`

## Release Workflow

Use `.github/workflows/release.yml` for the split publication flow:

1. Build and test the managed Capability.
2. Build `libtorch_bridge.dll` with Windows MSVC.
3. Pack the lightweight NuGet `.nupkg`.
4. Push the lightweight NuGet package to NuGet.org.
5. Build the full runtime `.zip`.
6. Upload the full runtime `.zip` to GitHub Release assets.

The full package job intentionally runs on a self-hosted Windows runner labeled
`Windows`, `X64`, and `CUDA13`. That runner must have:

- Visual Studio/MSVC C++ tools
- CMake
- CUDA Toolkit 13.0.x
- LibTorch 2.12.0 + CUDA 13.0 available at the workflow `libtorch_path`
  input, or another path passed through `AIKERNEL_LIBTORCH_PATH`

See [`docs/package-distribution.md`](docs/package-distribution.md) for the
publisher and consumer checklist.

## Python / pip

Python distribution is independent from NuGet. NuGet packages are for C#
consumers, while Python wrappers are published through pip.

The Python package follows the same channel policy as AIKernel.Core:

- Stable packages are published to PyPI.
- Development packages are attached to GitHub Releases for CI/CD and
  compatibility testing.

The stable Python package for this Capability is:

```bash
pip install aikernel-cuda13-libtorch2-12-win-x64
```

Import it as:

```python
import aikernel_cuda13_libtorch2_12_win_x64 as cuda_capability
```

This Python package is intentionally lightweight. It carries the CUDA
Capability identity, descriptor metadata, managed Capability DLL,
`libtorch_bridge.dll`, `loader.json`, and installation guidance. It does not
include LibTorch, CUDA runtime DLLs, cuDNN, or cuBLAS; those runtime assets
remain in the full GitHub Release runtime archive.

Development wheels use `aikernel-cuda13-libtorch2-12-win-x64-dev` and versions
such as `0.0.5.dev1`. GitHub Packages does not provide a PyPI registry, so dev
pip artifacts are distributed as GitHub Release assets or installed from the
repository with `pip install git+...#subdirectory=python`.

See [`docs/python-package-distribution.md`](docs/python-package-distribution.md)
for the Python package policy.

## Third-Party Notices

LibTorch/PyTorch runtime binaries are redistributed under the BSD 3-Clause
License and Additional Terms. CUDA Runtime and cuDNN DLLs included in the
LibTorch Windows CUDA package are NVIDIA redistributables.

The lightweight NuGet package does not redistribute LibTorch, CUDA, cuDNN, or
cuBLAS binaries.

The GitHub Release runtime archive includes:

- `LICENSE-THIRD-PARTY/pytorch-LICENSE.txt`
- `LICENSE-THIRD-PARTY/pytorch-NOTICE.txt`

Keep those files in the runtime archive whenever LibTorch CUDA binaries are
included.

## Fork Model

Create separate Capability repositories for other GPU, OS, RID, or runtime
targets. This repository remains the Windows `win-x64` CUDA 13.0 + LibTorch
2.12.0 package only.

- `AIKernel.Cuda13.0.Libtorch2.12.win-arm64`
- `AIKernel.Cuda13.0.Libtorch2.12.linux-x64`
- `AIKernel.Cuda12.4.Libtorch2.3.win-x64`
- `AIKernel.ROCm6.Libtorch2.12.linux-x64`
- `AIKernel.DirectML.win-x64`
- `AIKernel.Vulkan.linux-x64`

Fork this repository and update the runtime version metadata, native build
settings, and descriptor metadata while preserving AIKernel Capability
contracts.

## License

Apache License 2.0.

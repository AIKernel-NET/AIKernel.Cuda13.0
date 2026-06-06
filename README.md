# AIKernel.Cuda13.0.Libtorch2.12.win-x64

Reference external AIKernel Capability module for one runtime combination:
Windows `win-x64`, LibTorch 2.12.0, and CUDA 13.0. This repository owns the
CUDA-specific managed invoker, native C ABI bridge, redistributable
LibTorch/CUDA runtime binaries, and runtime metadata that were intentionally
separated from AIKernel.Core.

AIKernel.Core is CUDA-free. Install this package only on trusted GPU hosts that
explicitly opt in to CUDA execution.

## Package Model

This Capability has two package artifacts with the same package id and version:

- **Metadata package**: published to NuGet.org for discovery only. It contains
  no `lib/`, `runtimes/`, or native binaries. It carries the managed AIKernel
  dependency metadata for Core, Kernel, Abstractions, Dtos, and Enums.
- **Full runtime package**: published as a GitHub Release asset. It includes
  LibTorch, CUDA runtime DLLs, cuDNN redistributables, and `libtorch_bridge.dll`.

The full runtime package is too large for NuGet.org because NuGet.org limits
packages to about 250 MB. Do not install the NuGet.org metadata package when
you need CUDA execution. Download the matching full `.nupkg` from GitHub
Releases and install it from a local NuGet source.

## Install The Full Runtime Package

Download the full package from:

```text
https://github.com/AIKernel-NET/AIKernel.Cuda13.0/releases
```

Then add the folder that contains the downloaded `.nupkg` as a local source and
install from that source explicitly:

```powershell
$feed = "$HOME\.aikernel\cuda-packages"
New-Item -ItemType Directory -Force $feed | Out-Null

# Put AIKernel.Cuda13.0.Libtorch2.12.win-x64.0.0.5.nupkg in $feed first.
dotnet nuget add source $feed --name AIKernel-CUDA
dotnet add package AIKernel.Cuda13.0.Libtorch2.12.win-x64 --version 0.0.5 --source $feed
```

The managed package exposes:

- `LibTorchCapabilityDescriptor.Create()`
- `LibTorchCapabilityInvoker`
- C ABI operations: `load_model`, `unload_model`, `forward`

The full runtime package includes the Windows native bridge and redistributable
LibTorch CUDA DLLs under `runtimes/win-x64/native/`. The public C ABI is stable.
Do not expose LibTorch, CUDA, or C++ types across the ABI boundary.

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

The package project uses these defaults when packing runtime assets:

- `AIKernelLibTorchRedistPath=../ref/libtorch-win-shared-with-deps-2.12.0+cu130/libtorch`
- `AIKernelNativeBridgePath=native/build/win-x64/Release/libtorch_bridge.dll`

Override these MSBuild properties if your CI extracts LibTorch or native build
artifacts elsewhere.

## Release Verification

Before publishing, verify both managed and native surfaces:

```powershell
dotnet test AIKernel.Cuda13.0.Libtorch2.12.win-x64.slnx -c Release --no-restore
dotnet pack AIKernel.Cuda13.0.Libtorch2.12.win-x64.slnx -c Release --no-restore
cmake --build native/build/win-x64 --config Release
```

The metadata package is produced from
`packaging/meta/AIKernel.Cuda13.0.Libtorch2.12.win-x64.nuspec.template` by the
release workflow after replacing `$version$` with the release version. It does
not restore managed dependencies during packing; Core/Kernel package
availability is verified when the metadata package is pushed and consumed from
NuGet.org.

The GitHub Release full runtime `.nupkg` should contain:

- `lib/net10.0/AIKernel.Cuda13.0.Libtorch2.12.win-x64.dll`
- `runtimes/win-x64/native/libtorch_bridge.dll`
- LibTorch / CUDA runtime DLLs under `runtimes/win-x64/native/`
- `LICENSE`
- `README.md`
- `LICENSE-THIRD-PARTY/*`

It should not contain source-only `native/` files or `runtime/` placeholders.

The NuGet.org metadata `.nupkg` should contain metadata, managed AIKernel
dependencies, `README.md`, and `LICENSE`. It must not contain `lib/`,
`runtimes/`, or native binaries.

## Release Workflow

Use `.github/workflows/release.yml` for the split publication flow:

1. Build and test the managed Capability.
2. Build `libtorch_bridge.dll` with Windows MSVC.
3. Pack the full runtime `.nupkg`.
4. Upload the full runtime `.nupkg` to the GitHub Release assets.
5. Pack the metadata `.nupkg` with managed AIKernel dependencies only.
6. Push only the metadata package to NuGet.org.

The full package job intentionally runs on a self-hosted Windows runner labeled
`Windows`, `X64`, and `CUDA13`. That runner must have:

- Visual Studio/MSVC C++ tools
- CMake
- CUDA Toolkit 13.0.x
- LibTorch 2.12.0 + CUDA 13.0 available at the workflow `libtorch_path`
  input, or another path passed through `AIKERNEL_LIBTORCH_PATH`

The metadata package job can run on a hosted Windows runner because it contains
no CUDA, LibTorch, native binaries, or runtime payload.

The metadata package and the full package intentionally share the same package
id and version. Runtime users should install with `--source` pointing at the
local folder containing the downloaded full package so NuGet selects the full
runtime package instead of the NuGet.org metadata package.

## Python

The default `aikernel` Python package is CPU-only and lives with
AIKernel.Core. GPU-specific Python or native wrappers should be provided by the
matching external Capability repository if needed.

## Third-Party Notices

LibTorch/PyTorch runtime binaries are redistributed under the BSD 3-Clause
License and Additional Terms. CUDA Runtime and cuDNN DLLs included in the
LibTorch Windows CUDA package are NVIDIA redistributables.

This NuGet package includes:

- `LICENSE-THIRD-PARTY/pytorch-LICENSE.txt`
- `LICENSE-THIRD-PARTY/pytorch-NOTICE.txt`

Keep those files in the package whenever LibTorch CUDA binaries are included.

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

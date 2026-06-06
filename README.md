# AIKernel.Cuda13.0.Libtorch2.12.win-x64

Reference external AIKernel Capability module for one runtime combination:
Windows `win-x64`, LibTorch 2.12.0, and CUDA 13.0. This repository owns the
CUDA-specific managed invoker, native C ABI bridge, redistributable
LibTorch/CUDA runtime binaries, and runtime metadata that were intentionally
separated from AIKernel.Core.

AIKernel.Core is CUDA-free. Install this package only on trusted GPU hosts that
explicitly opt in to CUDA execution.

## Package

```powershell
dotnet add package AIKernel.Cuda13.0.Libtorch2.12.win-x64 --version 0.0.5
```

The managed package exposes:

- `LibTorchCapabilityDescriptor.Create()`
- `LibTorchCapabilityInvoker`
- C ABI operations: `load_model`, `unload_model`, `forward`

The NuGet package includes the Windows native bridge and redistributable
LibTorch CUDA DLLs under `runtimes/win-x64/native/`. The public C ABI is
stable. Do not expose LibTorch, CUDA, or C++ types across the ABI boundary.

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

The `.nupkg` should contain:

- `lib/net10.0/AIKernel.Cuda13.0.Libtorch2.12.win-x64.dll`
- `runtimes/win-x64/native/libtorch_bridge.dll`
- LibTorch / CUDA runtime DLLs under `runtimes/win-x64/native/`
- `LICENSE`
- `README.md`
- `LICENSE-THIRD-PARTY/*`

It should not contain source-only `native/` files or `runtime/` placeholders.

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

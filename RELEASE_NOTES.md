# AIKernel.Cuda13.0.Libtorch2.12.win-x64 Release Notes

## 0.0.5

Initial Windows `win-x64` CUDA Capability package for AIKernel.

This release uses split distribution:

- NuGet.org receives a metadata package only. It includes managed AIKernel
  dependencies for `AIKernel.Abstractions`, `AIKernel.Core`, `AIKernel.Kernel`,
  `AIKernel.Dtos`, and `AIKernel.Enums`, but no native payload.
- GitHub Releases receive the full runtime `.nupkg`. It includes the managed
  Capability assembly, `libtorch_bridge.dll`, LibTorch 2.12.0 CUDA 13.0
  runtime DLLs, CUDA redistributables, and PyTorch license notices.

Install the full runtime package by downloading the matching GitHub Release
asset, adding its folder as a local NuGet source, and installing this package id
with `--source` pointing at that local folder.

The public C ABI remains:

- `load_model`
- `unload_model`
- `forward`

AIKernel.Core remains CUDA-free. This repository is the external Capability
snapshot for Windows `win-x64`, CUDA 13.0, and LibTorch 2.12.0 only.

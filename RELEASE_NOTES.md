# AIKernel.Cuda13.0.Libtorch2.12.win-x64 Release Notes

[日本語](RELEASE_NOTES-ja.md)

## 0.1.2

**June 16th, 2026 - Preparing the CUDA Capability publication line.**
**2026年6月16日--CUDA Capability の公開ラインを準備する。**

AIKernel.Cuda13.0.Libtorch2.12.win-x64 v0.1.2 keeps the external Capability
model unchanged while aligning the package for the next public CUDA release:

- Bump the lightweight NuGet package identity to `0.1.2`.
- Align managed AIKernel package dependencies with the `0.1.1.1` contract and
  core library package line selected for the current validation train.
- Keep AIKernel.Core CUDA-free.
- Keep LibTorch, CUDA, cuDNN, cuBLAS, and other large runtime assets outside the
  lightweight NuGet package.
- Prepare the synchronized NuGet + PyPI publication path for the 0.1.2 canonical
  series.

Install the lightweight C# package from NuGet.org:

```powershell
dotnet add package AIKernel.Cuda13.0.Libtorch2.12.win-x64 --version 0.1.2
```

The public C ABI remains:

- `load_model`
- `unload_model`
- `forward`

## 0.1.1

**June 10th, 2026 - Validating the CUDA path.**
**2026年6月10日--CUDA パスを検証する。**

Validating the CUDA path: the LibTorch 2.12 surface synchronizes under guarded
Windows conditions. CUDA パスの検証--LibTorch 2.12 の面が Windows 限定のガード下で
同期される。

Public external CUDA Capability baseline for the AIKernel 0.1.1 package family:

- Add the NuGet package icon (`aikernel-logo128x128.png`) through
  `PackageIcon`.
- Keep the package model unchanged: lightweight NuGet / lightweight pip /
  GitHub Release runtime archive.
- Align managed package references with the AIKernel.Core and AIKernel.NET
  0.1.1 package family.
- Publish the Python wrapper as
  `aikernel-cuda13-libtorch2-12-win-x64` version `0.1.1`.

This release uses split distribution:

- NuGet.org receives a lightweight C# package. It includes the managed
  Capability assembly, `libtorch_bridge.dll`, `loader.json`, and dynamic
  runtime loading logic.
- The NuGet package does not include LibTorch, CUDA, cuDNN, cuBLAS, or other
  large runtime DLLs.
- GitHub Releases receive the full runtime `.zip`. It includes LibTorch 2.12.0
  CUDA 13.0 runtime files, CUDA redistributables, cuDNN, cuBLAS,
  `libtorch_bridge.dll`, `loader.json`, and PyTorch license notices.
- PyPI receives a lightweight Python wrapper package named
  `aikernel-cuda13-libtorch2-12-win-x64`.
- Development wheel changes are folded into the next public release note rather
  than being listed as separate package history entries.

Install the lightweight C# package from NuGet.org:

```powershell
dotnet add package AIKernel.Cuda13.0.Libtorch2.12.win-x64 --version 0.1.1
```

For a self-contained CUDA runtime snapshot, download and extract the matching
GitHub Release `.runtime.zip` asset. Extracting it beside the app allows the
default `loader.json` relative paths to work; otherwise set
`AIKERNEL_CUDA13_LIBTORCH2_12_WIN_X64_LOADER`.

See `docs/package-distribution.md` for the detailed split-distribution
checklist.

Install the Python wrapper package with:

```bash
pip install aikernel-cuda13-libtorch2-12-win-x64
```

Import it with:

```python
import aikernel_cuda13_libtorch2_12_win_x64
```

The Python package is distributed through pip and is not embedded in the NuGet
package. NuGet packages are for C# consumers; pip packages are for Python
consumers.

The public C ABI remains:

- `load_model`
- `unload_model`
- `forward`

AIKernel.Core remains CUDA-free. This repository is the external Capability
snapshot for Windows `win-x64`, CUDA 13.0, and LibTorch 2.12.0 only.

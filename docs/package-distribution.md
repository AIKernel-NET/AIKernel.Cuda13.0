# Package Distribution

[日本語](package-distribution-ja.md)

AIKernel.Cuda13.0.Libtorch2.12.win-x64 uses split distribution:

- NuGet.org receives a lightweight C# package.
- GitHub Releases receive a full CUDA runtime archive.
- PyPI receives the Python wrapper package.

This is part of the AIKernel 0.1.1 release validation line scheduled
for 2026-06-10. The package validates external GPU Capability distribution
without moving CUDA, LibTorch, or native runtime payloads into AIKernel.Core.

## Artifacts

| Artifact | Location | Contents | Purpose |
| --- | --- | --- | --- |
| Lightweight NuGet package | NuGet.org | Managed assembly, `libtorch_bridge.dll`, `loader.json`, dynamic loader logic | C# consumption |
| Full runtime archive | GitHub Release asset | LibTorch CUDA, CUDA runtime, cuDNN, cuBLAS, bridge DLL, configured `loader.json` | Extract-and-run CUDA runtime |
| pip package | PyPI | Capability descriptor and install guidance | Python consumption |
| pip dev wheel | GitHub Release asset | Development Capability wrapper wheel | CI/CD and compatibility testing |

The lightweight NuGet package and the full runtime archive share the same
Capability identity:

```text
AIKernel.Cuda13.0.Libtorch2.12.win-x64
```

## Lightweight NuGet Package

The NuGet package is for C# consumers. It includes:

- `lib/net10.0/AIKernel.Cuda13.0.Libtorch2.12.win-x64.dll`
- `runtimes/win-x64/native/libtorch_bridge.dll`
- `loader.json`
- dynamic runtime loading logic based on `NativeLibrary.SetDllImportResolver`
- `README.md`
- `RELEASE_NOTES.md`
- `LICENSE`

It intentionally does not include:

- LibTorch
- CUDA runtime DLLs
- cuDNN
- cuBLAS
- other large runtime DLLs

Users point `loader.json` at the runtime location with either:

- `AIKERNEL_LIBTORCH_PATH`
- `AIKERNEL_CUDA13_LIBTORCH2_12_WIN_X64_HOME`
- `AIKERNEL_CUDA13_LIBTORCH2_12_WIN_X64_LOADER`
- relative `runtime/win-x64/libtorch` folders beside `loader.json`

Install:

```powershell
dotnet add package AIKernel.Cuda13.0.Libtorch2.12.win-x64 --version 0.1.1
```

## loader.json

`loader.json` is copied to consumer output and controls dynamic native loading.
The default file searches:

- `%AIKERNEL_LIBTORCH_PATH%/lib`
- `%AIKERNEL_LIBTORCH_PATH%`
- `%AIKERNEL_CUDA13_LIBTORCH2_12_WIN_X64_HOME%/runtime/win-x64/libtorch/lib`
- `%AIKERNEL_CUDA13_LIBTORCH2_12_WIN_X64_HOME%/runtime/win-x64/libtorch`
- `runtime/win-x64/libtorch/lib`
- `runtime/win-x64/libtorch`

Set `AIKERNEL_CUDA13_LIBTORCH2_12_WIN_X64_LOADER` to an explicit loader file
when the runtime is outside the application directory.

## Full Runtime Archive

The full runtime archive is uploaded to the matching GitHub Release as:

```text
AIKernel.Cuda13.0.Libtorch2.12.win-x64.<version>.runtime.zip
```

It includes:

- `runtimes/win-x64/native/libtorch_bridge.dll`
- `runtime/win-x64/libtorch/**`
- LibTorch 2.12.0 CUDA 13.0 runtime DLLs
- CUDA runtime DLLs
- cuDNN and cuBLAS redistributables included with LibTorch
- configured `loader.json`
- `LICENSE`
- `README.md`
- `RELEASE_NOTES.md`
- `LICENSE-THIRD-PARTY/*`

Users can extract the archive beside the application so the default
`loader.json` relative paths work. If they extract elsewhere, they should point
`AIKERNEL_CUDA13_LIBTORCH2_12_WIN_X64_LOADER` at the extracted `loader.json`.

## Python Package

The Python package is separate from NuGet. NuGet packages are for C# consumers;
pip packages are for Python consumers. The Python package is never embedded in
the NuGet package.

Stable releases are published to PyPI:

```bash
pip install aikernel-cuda13-libtorch2-12-win-x64
```

Development wheels use `aikernel-cuda13-libtorch2-12-win-x64-dev` and are
attached to GitHub Releases. They are not published to PyPI.

See `docs/python-package-distribution.md` for details.

## Release Checklist

1. Publish AIKernel.NET and AIKernel.Core managed dependencies first.
   The CUDA release workflow's `managed_package_version` input must point to
   the already-published managed package family. During local validation this
   may be a cache-busting build such as `0.1.1.2`; for the public v0.1.1 release
   it should be `0.1.1` after AIKernel.NET/Core have been repacked and
   published with the final contract surface.
2. Ensure the self-hosted Windows runner has CUDA Toolkit 13.0.x and LibTorch
   2.12.0 CUDA 13.0.
3. Run the release workflow.
4. Verify the lightweight NuGet `.nupkg` is published to NuGet.org.
5. Verify the full runtime `.zip` is attached to the GitHub Release.
6. Build and verify the Python wheel from `python/`.
7. Publish the stable Python wheel to PyPI, or attach a dev wheel to GitHub
   Releases.
8. In a clean C# consumer project, install the NuGet package and configure
   `loader.json` or extract the runtime archive beside the app.

## Architecture Boundary

This repository is an external Capability snapshot for Windows `win-x64`, CUDA
13.0, and LibTorch 2.12.0. AIKernel.Core remains CUDA-free. Other CUDA,
LibTorch, OS, or RID combinations should be separate Capability repositories.

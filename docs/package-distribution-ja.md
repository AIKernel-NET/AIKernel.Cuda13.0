# Package Distribution

[English](package-distribution.md)

AIKernel.Cuda13.0.Libtorch2.12.win-x64 は split distribution を採用します。

- NuGet.org: lightweight C# package
- GitHub Releases: full CUDA runtime archive
- PyPI: Python wrapper package

これは 2026-06-10 予定の AIKernel 0.1.1 release validation line の一部です。
CUDA、LibTorch、native runtime payload を AIKernel.Core に移さず、外部 GPU Capability
distribution を検証します。

## Artifacts

| Artifact | Location | Contents | Purpose |
| --- | --- | --- | --- |
| Lightweight NuGet package | NuGet.org | Managed assembly, `libtorch_bridge.dll`, `loader.json`, dynamic loader logic | C# consumption |
| Full runtime archive | GitHub Release asset | LibTorch CUDA, CUDA runtime, cuDNN, cuBLAS, bridge DLL, configured `loader.json` | Extract-and-run CUDA runtime |
| pip package | PyPI | Capability descriptor and install guidance | Python consumption |
| pip dev wheel | GitHub Release asset | Development Capability wrapper wheel | CI/CD and compatibility testing |

Lightweight NuGet package と full runtime archive は同じ Capability identity を共有します。

```text
AIKernel.Cuda13.0.Libtorch2.12.win-x64
```

## Lightweight NuGet Package

NuGet package は C# consumer 向けです。含むもの:

- `lib/net10.0/AIKernel.Cuda13.0.Libtorch2.12.win-x64.dll`
- `runtimes/win-x64/native/libtorch_bridge.dll`
- `loader.json`
- `NativeLibrary.SetDllImportResolver` に基づく dynamic runtime loading logic
- `README.md`
- `RELEASE_NOTES.md`
- `LICENSE`

含めないもの:

- LibTorch
- CUDA runtime DLLs
- cuDNN
- cuBLAS
- other large runtime DLLs

Install:

```powershell
dotnet add package AIKernel.Cuda13.0.Libtorch2.12.win-x64 --version 0.1.1
```

## Full Runtime Archive

Full runtime archive は matching GitHub Release に次の形式で upload します。

```text
AIKernel.Cuda13.0.Libtorch2.12.win-x64.<version>.runtime.zip
```

含むもの:

- `runtimes/win-x64/native/libtorch_bridge.dll`
- `runtime/win-x64/libtorch/**`
- LibTorch 2.12.0 CUDA 13.0 runtime DLLs
- CUDA runtime DLLs
- LibTorch に含まれる cuDNN / cuBLAS redistributables
- configured `loader.json`
- `LICENSE`
- `README.md`
- `RELEASE_NOTES.md`
- `LICENSE-THIRD-PARTY/*`

## Python Package

Python package は NuGet から分離します。NuGet package は C# consumer 向け、pip package は
Python consumer 向けです。Python package は NuGet package に埋め込みません。

Stable release:

```bash
pip install aikernel-cuda13-libtorch2-12-win-x64
```

Development wheel は `aikernel-cuda13-libtorch2-12-win-x64-dev` を使い、GitHub Release に
添付します。PyPI には公開しません。

## Architecture Boundary

この repository は Windows `win-x64`、CUDA 13.0、LibTorch 2.12.0 の外部 Capability
snapshot です。AIKernel.Core は CUDA-free のままです。他の CUDA、LibTorch、OS、RID
combination は別 Capability repository としてください。

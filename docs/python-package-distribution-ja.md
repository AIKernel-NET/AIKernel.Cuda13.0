# Python Package Distribution

[English](python-package-distribution.md)

この repository は CUDA Capability 用の lightweight Python package を公開します。
Python distribution は NuGet から独立しています。NuGet package は C# consumer 向け、
pip package は Python consumer 向けです。

この page は v0.1.3 正典 GPU integration series の stable Python distribution
channel を説明します。integration 中は `0.1.3.dev{buildNumber}` のような
development wheel を使い、publication task が明示的に要求するまで stable
`0.1.3` package は作成しません。

Python package は NuGet package に埋め込みません。Release wheel は NuGet package と
同じ lightweight runtime surface を持ちます。managed Capability DLL、native bridge DLL、
`loader.json` を含みますが、CUDA runtime payload は含みません。

## Stable Channel

Stable Python release は PyPI に公開します。

| Field | Value |
| --- | --- |
| Distribution | `aikernel-cuda13-libtorch2-12-win-x64` |
| Import name | `aikernel_cuda13_libtorch2_12_win_x64` |
| Version line | `0.1.3 -> ...` |
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
print(cuda_capability.native_verification_commands())
```

## Development Channel

Development Python wheel は `0.1.3.dev1` のような version number を使います。pip user
向け development wheel は local artifact、GitHub Release asset、または repository
から直接 install します。

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
## Native Verification Boundary

Python wrapper は、tooling が canonical rev3 package / library verification command
を表示できるように `native_verification_commands()` を公開します。これらの command は
意図的に Python execution の外側に置きます。

- package mode は LibTorch / CUDA を load せず packaged loader/runtime entry を検証します。
- library mode は `aikernel_cuda13_dispatch` を load し、staged fail-closed dispatch ABI
  を検証します。

これにより Python distribution を lightweight に保ったまま、NuGet と native bridge
validation で使う `aik gpu verify-native` check へ安定して接続できます。
## Trusted Publisher 設定

aikernel-cuda13-libtorch2-12-win-x64 project の PyPI Trusted Publisher は、この repository が発行する GitHub OIDC claims と一致している必要があります。

| Field | Value |
| --- | --- |
| PyPI project | aikernel-cuda13-libtorch2-12-win-x64 |
| Owner | AIKernel-NET |
| Repository | AIKernel.Cuda13.0 |
| Workflow | publish-python.yml |
| Environment | pypi |

PyPI が `invalid-publisher` を返す場合、workflow を token credential 方式へ戻してはいけません。PyPI project 側の Trusted Publisher entry を上記の値に合わせて修正し、失敗した publish job を rerun します。

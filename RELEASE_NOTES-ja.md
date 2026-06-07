# AIKernel.Cuda13.0.Libtorch2.12.win-x64 リリースノート

[English](RELEASE_NOTES.md)

## 0.1.0

最初の stable external CUDA Capability baseline です。

- `PackageIcon` により NuGet package icon (`aikernel-logo128x128.png`) を追加しました。
- Package model は維持します: lightweight NuGet / lightweight pip /
  GitHub Release runtime archive。
- Managed package reference を AIKernel.Core / AIKernel.NET 0.1.0 package family に
  揃えました。
- Python wrapper を `aikernel-cuda13-libtorch2-12-win-x64` version `0.1.0` として公開します。

この release は split distribution を採用します。

- NuGet.org には lightweight C# package を公開します。managed Capability assembly、
  `libtorch_bridge.dll`、`loader.json`、dynamic runtime loading logic を含みます。
- NuGet package は LibTorch、CUDA、cuDNN、cuBLAS、その他巨大 runtime DLL を含みません。
- GitHub Releases には full runtime `.zip` を置きます。LibTorch 2.12.0 CUDA 13.0
  runtime files、CUDA redistributables、cuDNN、cuBLAS、`libtorch_bridge.dll`、
  `loader.json`、PyTorch license notices を含みます。
- PyPI には `aikernel-cuda13-libtorch2-12-win-x64` という lightweight Python wrapper
  package を公開します。
- GitHub Releases には CI/CD と compatibility testing 用の
  `aikernel-cuda13-libtorch2-12-win-x64-dev` development Python wheel を置きます。

Install:

```powershell
dotnet add package AIKernel.Cuda13.0.Libtorch2.12.win-x64 --version 0.1.0
```

Self-contained CUDA runtime snapshot が必要な場合は、matching GitHub Release の
`.runtime.zip` asset を download / extract してください。

Python wrapper:

```bash
pip install aikernel-cuda13-libtorch2-12-win-x64
```

```python
import aikernel_cuda13_libtorch2_12_win_x64
```

Python package は pip で配布し、NuGet package には含めません。NuGet は C# consumer
向け、pip は Python consumer 向けです。

Public C ABI:

- `load_model`
- `unload_model`
- `forward`

AIKernel.Core は CUDA-free のままです。この repository は Windows `win-x64`、
CUDA 13.0、LibTorch 2.12.0 専用の external Capability snapshot です。

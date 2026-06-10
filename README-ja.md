# AIKernel.Cuda13.0.Libtorch2.12.win-x64

[English README](README.md)

Windows `win-x64`、LibTorch 2.12.0、CUDA 13.0 という 1 つの runtime
combination に対応する AIKernel external Capability module です。

この repository は、CUDA-specific managed invoker、native C ABI bridge、
loader configuration、runtime metadata を所有します。これらは意図的に
AIKernel.Core から分離されています。

AIKernel.Core は CUDA-free です。この package は CUDA execution に明示的に opt-in
する trusted GPU host にのみ install してください。

AIOS SDK において、AIKernel.Cuda13.0 は optional な GPU backend layer です。
Windows CUDA 13.0 と LibTorch runtime の関心事を kernel runtime の外へ分離し、
native accelerator execution が必要な distribution だけが明示的に opt-in できます。

AIKernel には、公式 AIOS ディストリビューションである **AIKernel.Monolith** もあります。
Monolith は 0.1.x 系の安定化後に SDK layer を統合する標準 AIOS として
開発が開始されています。optional GPU backend は引き続き明示的な opt-in component です。

この repository は、2026-06-10 予定の AIKernel 0.1.1 release validation line
に参加します。Core は CUDA-free のまま、Windows `win-x64` CUDA 13.0 +
LibTorch 2.12.0 runtime boundary をこの repository が所有する split を検証します。

詳細な distribution rule は [docs/package-distribution-ja.md](docs/package-distribution-ja.md)
を参照してください。

## パッケージモデル

この Capability には 2 つの C# runtime artifact と 1 つの Python wrapper artifact があります。

- Lightweight NuGet package: C# consumer 向けに NuGet.org へ公開します。
  managed assembly、`libtorch_bridge.dll`、`loader.json`、dynamic loading logic を含みます。
  LibTorch、CUDA、cuDNN、cuBLAS などの巨大 runtime DLL は含みません。
- Full runtime archive: GitHub Release `.zip` として公開します。
  LibTorch CUDA、CUDA Runtime、cuDNN、cuBLAS、`libtorch_bridge.dll`、自動設定済み
  `loader.json` を含みます。
- pip package: Python consumer 向けに PyPI へ公開します。NuGet package には含めません。

NuGet は C# distribution channel、pip は Python distribution channel です。
GitHub Release archive が巨大 CUDA runtime snapshot を保持します。

## 安全な利用手順

まず lightweight NuGet package または pip package で、package identity、descriptor
metadata、loader configuration を確認してください。Full runtime archive は、CUDA 13.0 と
LibTorch 2.12.0 の実行を明示的に意図した trusted Windows GPU host でのみ取得します。

## Lightweight NuGet Package のインストール

C# consumer:

```powershell
dotnet add package AIKernel.Cuda13.0.Libtorch2.12.win-x64 --version 0.1.1
```

NuGet package は `loader.json` を含みます。次のいずれかで設定できます。

- `AIKERNEL_LIBTORCH_PATH`
- `AIKERNEL_CUDA13_LIBTORCH2_12_WIN_X64_HOME`
- `AIKERNEL_CUDA13_LIBTORCH2_12_WIN_X64_LOADER`

## Full Runtime Archive のインストール

Full runtime archive は GitHub Release から取得します。

```text
https://github.com/AIKernel-NET/AIKernel.Cuda13.0/releases
```

Consuming application の横に展開するか、`AIKERNEL_CUDA13_LIBTORCH2_12_WIN_X64_LOADER`
で展開済み `loader.json` を指定してください。

Public C ABI は安定境界です。LibTorch、CUDA、C++ 型を ABI 境界へ公開してはいけません。

## Python / pip の利用

Python distribution は NuGet から独立しています。NuGet は C# consumer 向け、
Python wrapper は pip 経由で公開します。

Stable package:

```bash
pip install aikernel-cuda13-libtorch2-12-win-x64
```

Import:

```python
import aikernel_cuda13_libtorch2_12_win_x64 as cuda_capability
```

Python package は lightweight です。Capability identity、descriptor metadata、
managed Capability DLL、`libtorch_bridge.dll`、`loader.json`、installation guidance を
含みます。LibTorch、CUDA runtime DLL、cuDNN、cuBLAS は含みません。

## Third-Party Notices / 第三者ライセンス

LibTorch / PyTorch runtime binary は BSD 3-Clause License と Additional Terms で
再配布されます。LibTorch Windows CUDA package に含まれる CUDA Runtime と cuDNN DLL は
NVIDIA redistributable です。

Lightweight NuGet package は LibTorch、CUDA、cuDNN、cuBLAS binary を再配布しません。
GitHub Release runtime archive に LibTorch CUDA binary を含める場合は、
`LICENSE-THIRD-PARTY/pytorch-LICENSE.txt` と
`LICENSE-THIRD-PARTY/pytorch-NOTICE.txt` を保持してください。

## Fork Model / 他 runtime への展開

他の GPU、OS、RID、runtime target は別 Capability repository として作成してください。
この repository は Windows `win-x64` CUDA 13.0 + LibTorch 2.12.0 package のみです。

## License / ライセンス

Apache License 2.0.

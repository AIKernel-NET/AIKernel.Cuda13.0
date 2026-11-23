# AIKernel.Cuda13.0.Libtorch2.12.win-x64

**保守終了。後継は `AIKernel.Kernel` 0.1.4 の CUDA Driver です。**

`AIKernel.Kernel` 0.1.4 は Windows x64 と Linux x64 向けの CUDA Driver を同梱します。実行時に必要なネイティブ依存は NVIDIA ドライバのみです。

[CUDA Driver 移行ガイド](https://github.com/AIKernel-NET/AIKernel.Providers/blob/dev/docs/CUDA-DRIVER-MIGRATION.md) · [English README](README.md)

このリポジトリは 0.1.3 までの旧 LibTorch 参照実装です。以降は既存環境の参照資料です。

## セキュリティに関する告知

以下は公開済みの旧 LibTorch 版に関する注意事項です。

- **CU-1** カレントディレクトリの `loader.json` を通じて意図しない DLL が読み込まれるおそれがあります。信頼できない作業ディレクトリでは使用しないでください。
- **CU-2** 旧ローダーは DLL の検索パスを変更し、そのプロセスで解決される依存 DLL に影響する場合があります。承認済みのランタイムディレクトリだけを使用してください。
- **CU-3** `load_model` は指定されたパスの TorchScript を読み込みます。信頼できないモデルファイルを渡さないでください。

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
dotnet add package AIKernel.Cuda13.0.Libtorch2.12.win-x64 --version 0.1.3
```

Managed package は `LibTorchCapabilityDescriptor.Create()`、`LibTorchCapabilityInvoker`、
`load_model` / `unload_model` / `forward` の C ABI 操作、そして canonical staged native
dispatch probe である `aikernel_cuda13_dispatch` を公開します。

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

canonical native dispatch entrypoint は、この line では意図的に fail-closed です。
canonical 40-byte request header を受け取り、frame index と sample ticks を保持し、
`InvalidRequestLength`、`UnknownPass`、`DeviceUnavailable`、
`CommandSubmissionDisabled` などの deterministic failure reason を返します。
実 CUDA queue / device-buffer submission は、missing-runtime、invalid-header、
unknown-operation、CPU fallback、device-lost、command-submission-disabled の native fixture
coverage が揃うまで無効のままです。

Native bridge を rebuild した後は package shape と staged dispatch ABI を確認してください。

```powershell
.\scripts\verify-native-package.ps1
.\scripts\verify-native-library.ps1 -LibTorchPath <libtorch-root> -CudaRuntimePath <cuda-runtime-root>
```

`verify-native-library.ps1` は明示 path なしでも実行できます。その場合は
`AIKERNEL_LIBTORCH_PATH`、repository 内 `runtime/win-x64/libtorch`、親 workspace
の `ref/libtorch-win-shared-with-deps-2.12.0+cu130/libtorch`、D ドライブ移行用の
`D:\AIKernel\runtime\win-x64\libtorch` / `D:\AIKernel\ref\...` を順に probe します。
CUDA runtime は `CUDA_PATH_V13_0`、`CUDA_PATH`、標準 CUDA 13.0 install path、
`D:\AIKernel\runtime\cuda\v13.0` を probe します。見つかった `bin` / `lib` folder は
一時的に `PATH` の先頭へ追加され、`dependency-probe` 行として診断出力されます。
strict verification では LibTorch または CUDA 13 が解決できない場合、native bridge を
load する前に失敗します。`-AllowLoadFailure` は missing-dependent-module 境界を
意図的に確認する diagnostics-only lane でのみ使用してください。

release lane では、packaged DLL が native bridge source より古い場合に失敗させます。

```powershell
.\scripts\verify-native-package.ps1 -RequireFreshNativeBridge
```

明示的な library check は次のような行を出力する必要があります。

```text
cuda-dispatch-response: abi=1 status=NotInitialized failure=CommandSubmissionDisabled frame=1 ticks=100 diagnostics=0
cuda-dispatch-invalid-length: failure=InvalidRequestLength
```

runtime dependency set がまだ配置されていない場合は、diagnostics-only mode
として次を利用できます。

```powershell
.\scripts\verify-native-library.ps1 -AllowLoadFailure
```

この mode は Windows loader の `0x8007007E` dependent-module failure を想定内として
扱い、LibTorch/CUDA runtime 不足の境界が CLI に明確に表示されることを確認します。

## Python / pip の利用

Python distribution は NuGet から独立しています。NuGet は C# consumer 向け、
Python wrapper は pip 経由で公開します。

0.1.3 local development line では、NuGet は `0.1.3-dev{build-number}`、Python wrapper は `0.1.3.dev{build-number}` として検証します。`r`n0.1.3 publication line では、この CUDA Capability も NuGet と PyPI の同期公開準備パスに乗せます。

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

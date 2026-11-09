# AIKernel.Cuda13.0 リリースノート

[English](RELEASE_NOTES.md)

## 0.1.3（公開準備中）

- `AIKernel.Cuda13.0.Libtorch2.12.win-x64` を 0.1.3 のマネージド契約に揃えます。対象は Windows x64、CUDA 13.0、LibTorch 2.12 で、利用側が明示的に有効化します。
- 完全なネイティブランタイムは軽量 NuGet／Python 配布物とは分離します。配布するブリッジを対応する LibTorch／CUDA と組み合わせ、Python 同梱物からの読み込みも確認します。
- staged dispatch ABI の検証は境界と想定した fail-closed の結果を確認するもので、モデル推論の動作保証ではありません。[ランタイムと検証の手順](README-ja.md) を参照してください。

## 0.1.2

**2026年6月16日 - CUDA descriptor package line。**

AIKernel.Cuda13.0 0.1.2 は CUDA package を AIKernel 0.1.2 dependency chain に揃えます。

- CUDA 13 / LibTorch 2.12 package として `AIKernel.Cuda13.0.Libtorch2.12.win-x64` を公開します。
- managed AIKernel dependency を AIKernel.NET / AIKernel.Core 0.1.2 に揃えます。
- CUDA runtime concern は Providers、Control、Core の package surface から隔離します。
- Python publishing task が開始された場合、同期 Python wrapper を 0.1.2 release flow で公開します。

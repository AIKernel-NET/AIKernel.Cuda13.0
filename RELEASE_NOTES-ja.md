# AIKernel.Cuda13.0 リリースノート

[English](RELEASE_NOTES.md)

## 0.1.2

**2026年6月16日 - CUDA descriptor package line。**

AIKernel.Cuda13.0 0.1.2 は CUDA package を AIKernel 0.1.2 dependency chain に揃えます。

- CUDA 13 / LibTorch 2.12 package として `AIKernel.Cuda13.0.Libtorch2.12.win-x64` を公開します。
- managed AIKernel dependency を AIKernel.NET / AIKernel.Core 0.1.2 に揃えます。
- CUDA runtime concern は Providers、Control、Core の package surface から隔離します。
- Python publishing task が開始された場合、同期 Python wrapper を 0.1.2 release flow で公開します。
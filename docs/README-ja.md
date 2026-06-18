# AIKernel.Cuda13.0 ドキュメント

[English](README.md)

AIKernel.Cuda13.0 は、AIKernel package family における optional native GPU backend
repository です。Windows `win-x64` CUDA 13.0 + LibTorch 2.12 runtime boundary を所有し、
CUDA-specific native execution を Core と generic Providers substrate の外へ分離します。

## リポジトリ横断整合

共有の repository boundary、v0.1.2 development versioning、依存関係順、
PyPI Trusted Publishing、Python wrapper scope は
[Package Release Alignment v0.1.2](https://github.com/AIKernel-NET/AIKernel.NET/blob/main/docs/development/package-release-alignment-v0.1.2-ja.md)
で定義します。履歴としての v0.1.1.1 validation rule は
[AIKernel Repository Alignment v0.1.1.1](https://github.com/AIKernel-NET/AIKernel.NET/blob/main/docs/development/repository-alignment-v0.1.1.1-ja.md)
に残します。
複数 repository をまたぐ変更を行う場合は、まず
[リポジトリ横断開発者ガイド v0.1.1.1](https://github.com/AIKernel-NET/AIKernel.NET/blob/main/docs/development/cross-repository-developer-guide-v0.1.1.1-ja.md)
を読んでください。

AIKernel.Cuda13.0 は明示 opt-in の CUDA runtime package を所有します。Core runtime
policy、generic provider routing、非 CUDA backend、cross-platform provider abstraction は
所有しません。

## Sections

- [Package Distribution](package-distribution-ja.md)
- [Python Package Distribution](python-package-distribution-ja.md)

## Release Scope

Version 0.1.2 は現在の canonical integration line です。local NuGet package reference
には `0.1.2-dev{build-number}`、local Python wheel validation には
`0.1.2.dev{build-number}` を使います。

stable package artifact は依存関係順に後で作成します。publication task が明示的に要求する
まで、stable `0.1.2` package は作成しません。

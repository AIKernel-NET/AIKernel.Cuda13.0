# AIKernel.Cuda13.0 ドキュメント

[English](README.md)

AIKernel.Cuda13.0 は、AIKernel package family における optional native GPU backend
repository です。Windows `win-x64` CUDA 13.0 + LibTorch 2.12 runtime boundary を所有し、
CUDA-specific native execution を Core と generic Providers substrate の外へ分離します。

## リポジトリ横断整合

共有の repository boundary、0.1.1.1 local NuGet versioning、この更新ラインでの
NuGet-only / no-PyPI rule は
[AIKernel Repository Alignment v0.1.1.1](https://github.com/AIKernel-NET/AIKernel.NET/blob/main/docs/development/repository-alignment-v0.1.1.1-ja.md)
で定義します。

AIKernel.Cuda13.0 は明示 opt-in の CUDA runtime package を所有します。Core runtime
policy、generic provider routing、非 CUDA backend、cross-platform provider abstraction は
所有しません。

## Sections

- [Package Distribution](package-distribution-ja.md)
- [Python Package Distribution](python-package-distribution-ja.md)

## Release Scope

stable 0.1.1 documentation は公開済み Python channel を説明する場合があります。
0.1.1.1 local development line では、Python release が明示的に予定されない限り
NuGet package のみを作成・消費します。

cross-repository validation では local NuGet package reference に
`0.1.1.1-dev{build-number}` を使います。


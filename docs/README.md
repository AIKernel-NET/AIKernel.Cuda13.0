# AIKernel.Cuda13.0 Documentation

[日本語](README-ja.md)

AIKernel.Cuda13.0 is the optional native GPU backend repository for the
AIKernel package family. It owns the Windows `win-x64` CUDA 13.0 + LibTorch
2.12 runtime boundary and keeps CUDA-specific native execution outside Core and
the generic Providers substrate.

## Cross-Repository Alignment

Shared repository boundaries, v0.1.3 development versioning, dependency order,
PyPI Trusted Publishing, and Python wrapper scope are defined by
[AIKernel GPU rev3 Migration v0.1.3](https://github.com/AIKernel-NET/AIKernel.NET/blob/main/docs/migration/v0.1.3-gpu-rev3-migration.md).
The historical v0.1.1.1 validation rules remain available in
[AIKernel Repository Alignment v0.1.1.1](https://github.com/AIKernel-NET/AIKernel.NET/blob/main/docs/development/repository-alignment-v0.1.1.1.md).
When a change crosses repositories, start with the
[Cross-Repository Developer Guide v0.1.1.1](https://github.com/AIKernel-NET/AIKernel.NET/blob/main/docs/development/cross-repository-developer-guide-v0.1.1.1.md).

AIKernel.Cuda13.0 owns an explicit opt-in CUDA runtime package. It must not own
Core runtime policy, generic provider routing, non-CUDA backends, or
cross-platform provider abstractions.

## Sections

- [Package Distribution](package-distribution.md)
- [Python Package Distribution](python-package-distribution.md)

## Release Scope

Version 0.1.3 is the current canonical integration line. Use
`0.1.3-dev{build-number}` for local NuGet package references and
`0.1.3.dev{build-number}` for local Python wheel validation.

Stable package artifacts are created later in dependency order. Do not create
stable `0.1.3` packages until the publication task explicitly requests them.

# AIKernel.Cuda13.0 Documentation

[日本語](README-ja.md)

AIKernel.Cuda13.0 is the optional native GPU backend repository for the
AIKernel package family. It owns the Windows `win-x64` CUDA 13.0 + LibTorch
2.12 runtime boundary and keeps CUDA-specific native execution outside Core and
the generic Providers substrate.

## Cross-Repository Alignment

Shared repository boundaries, 0.1.1.1 local NuGet versioning, and the
NuGet-only / no-PyPI rule for this update line are defined by
[AIKernel Repository Alignment v0.1.1.1](https://github.com/AIKernel-NET/AIKernel.NET/blob/main/docs/development/repository-alignment-v0.1.1.1.md).

AIKernel.Cuda13.0 owns an explicit opt-in CUDA runtime package. It must not own
Core runtime policy, generic provider routing, non-CUDA backends, or
cross-platform provider abstractions.

## Sections

- [Package Distribution](package-distribution.md)
- [Python Package Distribution](python-package-distribution.md)

## Release Scope

The stable 0.1.1 documentation may describe the published Python channel. For
the 0.1.1.1 local development line, create and consume NuGet packages only
unless a Python release is explicitly scheduled.

Use `0.1.1.1-dev{build-number}` for local NuGet package references during
cross-repository validation.


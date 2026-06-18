# AIKernel.Cuda13.0 Release Notes

[日本語](RELEASE_NOTES-ja.md)

## 0.1.2

**June 16th, 2026 - CUDA descriptor package line.**

AIKernel.Cuda13.0 0.1.2 aligns the CUDA package with the AIKernel 0.1.2 dependency chain.

- Publish `AIKernel.Cuda13.0.Libtorch2.12.win-x64` as the CUDA 13 / LibTorch 2.12 package.
- Align managed AIKernel dependencies with AIKernel.NET and AIKernel.Core 0.1.2.
- Keep CUDA runtime concerns isolated from Providers, Control, and Core package surfaces.
- Publish the synchronized Python wrapper through the 0.1.2 release flow when the release task opens Python publishing.
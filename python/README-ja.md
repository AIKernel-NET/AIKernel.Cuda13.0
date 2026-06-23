# aikernel-cuda13-libtorch2-12-win-x64

[English](README.md)

AIKernel CUDA 13.0 + LibTorch 2.12 Windows `win-x64` Capability の Python wrapper
package です。

Stable package を PyPI から install します。

```bash
pip install aikernel-cuda13-libtorch2-12-win-x64
```

Import:

```python
import aikernel_cuda13_libtorch2_12_win_x64 as cuda_capability

descriptor = cuda_capability.capability_descriptor()
print(descriptor["package_id"])
```

この Python package は lightweight で、pip 経由で配布します。Release wheel には
CUDA Capability managed assembly、`libtorch_bridge.dll`、`loader.json` が含まれます。
LibTorch、CUDA runtime DLL、cuDNN、cuBLAS は含まれません。実行可能な CUDA runtime
snapshot は full GitHub Release runtime archive として配布します。

この package は Python tooling から Capability identity、supported runtime、
installation guidance を発見するために使います。NuGet は C# consumer 専用であり、
Python wrapper は NuGet package に埋め込みません。

v0.1.3 package では generated managed API catalog も公開します。
`managed_api_catalog()`、`managed_api_summary()`、`managed_type_names()`、
`find_managed_type(full_name)` で確認できます。

Bundled `loader.json` template と loader helper を含みます。

```python
config = cuda_capability.load_loader_config()
print(config.native_library)
print(config.resolved_runtime_search_paths())
```

canonical rev3 GPU integration check 用に、Python wrapper から対応する
native verification command も取得できます。

```python
for command in cuda_capability.native_verification_commands():
    print(command)
```

package mode は LibTorch / CUDA を load せず、lightweight wrapper と runtime entry
を検証します。library mode は `aikernel_cuda13_dispatch` を load し、staged
fail-closed dispatch ABI を確認します。

Development wheel は GitHub Release asset として公開し、PyPI には公開しません。
PyPI は stable release 専用です。

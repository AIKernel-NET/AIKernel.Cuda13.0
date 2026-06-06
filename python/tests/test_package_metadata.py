import aikernel_cuda13_libtorch2_12_win_x64 as cuda_capability


def test_capability_descriptor_uses_stable_python_names():
    descriptor = cuda_capability.capability_descriptor()

    assert descriptor["package_name"] in {
        "aikernel-cuda13-libtorch2-12-win-x64",
        "aikernel-cuda13-libtorch2-12-win-x64-dev",
    }
    assert descriptor["dev_package_name"] == "aikernel-cuda13-libtorch2-12-win-x64-dev"
    assert descriptor["import_name"] == "aikernel_cuda13_libtorch2_12_win_x64"
    assert descriptor["nuget_package_id"] == "AIKernel.Cuda13.0.Libtorch2.12.win-x64"
    assert descriptor["version"] == cuda_capability.__version__
    assert descriptor["cuda_version"] == "13.0"
    assert descriptor["libtorch_version"] == "2.12.0"
    assert descriptor["rid"] == "win-x64"


def test_install_instructions_point_to_github_release_nuget_runtime():
    instructions = cuda_capability.install_instructions()

    assert "https://github.com/AIKernel-NET/AIKernel.Cuda13.0/releases" in instructions
    assert "dotnet add package AIKernel.Cuda13.0.Libtorch2.12.win-x64" in instructions
    assert "runtime zip" in instructions
    assert "AIKERNEL_CUDA13_LIBTORCH2_12_WIN_X64_LOADER" in instructions


def test_bundled_loader_json_matches_capability_identity():
    config = cuda_capability.load_loader_config()
    descriptor = cuda_capability.capability_descriptor()

    assert config.package_id == descriptor["nuget_package_id"]
    assert config.rid == "win-x64"
    assert config.native_library == "libtorch_bridge.dll"
    assert "AIKERNEL_LIBTORCH_PATH" in "".join(config.runtime_search_paths)
    assert descriptor["loader_env"] == cuda_capability.LOADER_ENV
    assert descriptor["loader_json"].endswith("loader.json")


def test_optional_bundled_binary_lists_are_null_safe():
    assert isinstance(cuda_capability.bundled_managed_assemblies(), tuple)
    assert isinstance(cuda_capability.bundled_native_libraries(), tuple)

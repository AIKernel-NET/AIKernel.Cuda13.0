namespace AIKernel.Cuda13.Libtorch2_12.WinX64.Tests;

using AIKernel.Abstractions.Gpu;
using AIKernel.Cuda13.Libtorch2_12.WinX64.Capability;
using AIKernel.Cuda13.Libtorch2_12.WinX64.Model;
using AIKernel.Common.Results;
using AIKernel.Core.Memory;
using AIKernel.Dtos.Capabilities;
using AIKernel.Dtos.Gpu;
using AIKernel.Enums;
using System.Runtime.InteropServices;
using CoreMemoryAccessMode = AIKernel.Core.Memory.MemoryAccessMode;

public sealed class LibTorchCapabilityModuleTests
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate uint Cuda13DispatchDelegate(
        IntPtr request,
        uint requestLength,
        IntPtr response,
        uint responseLength);

    [Fact]
    public void Create_ReturnsVersionedNativeAbiDescriptor()
    {
        var descriptor = LibTorchCapabilityDescriptor.Create();

        Assert.Equal("libtorch.llama.cuda13.0.libtorch2.12.win-x64", descriptor.CapabilityId);
        Assert.Equal("2.12.0", descriptor.Version);
        Assert.Equal(CapabilityModuleKind.NativeLibrary, descriptor.Kind);
        Assert.Equal(CapabilityInvocationMode.NativeAbi, descriptor.InvocationMode);
        Assert.Equal(
            [GpuOperationNames.ComputeDispatch, "forward", "load_model", "unload_model"],
            descriptor.ProvidedOperations.Order().ToArray());
        Assert.Contains(GpuPermissionNames.ComputeExecute, descriptor.RequiredPermissions);
        Assert.Contains(GpuPermissionNames.BufferRead, descriptor.RequiredPermissions);
        Assert.Contains(GpuPermissionNames.BufferWrite, descriptor.RequiredPermissions);
        Assert.Equal("cdecl", descriptor.Metadata["abi.calling_convention"]);
        Assert.Equal("libtorch_bridge", descriptor.Metadata["abi.library"]);
        Assert.Equal("true", descriptor.Metadata[GpuProviderMetadataKeys.Rev3]);
        Assert.Equal("cuda13.0", descriptor.Metadata[GpuProviderMetadataKeys.Backend]);
        Assert.Equal("native-abi", descriptor.Metadata[GpuProviderMetadataKeys.PassBridge]);
        Assert.Equal("native-cuda-buffer-dispatch", descriptor.Metadata[GpuProviderMetadataKeys.GpuBypass]);
        Assert.Equal("not-required-native-provider", descriptor.Metadata[GpuProviderMetadataKeys.NativeJsBridge]);
        Assert.Equal("planned-gpu-native-execution", descriptor.Metadata[GpuProviderMetadataKeys.AotCompilerHooks]);
        Assert.Equal("host-frame-token-sample-ticks", descriptor.Metadata[GpuProviderMetadataKeys.DeterministicFrameSampling]);
        Assert.Equal("native-cuda-device-buffer", descriptor.Metadata[GpuProviderMetadataKeys.ZeroCopyBufferHandling]);
        Assert.Equal(GpuBackend.Cuda.ToString(), descriptor.Metadata[GpuProviderMetadataKeys.GpuBackend]);
        Assert.Contains(nameof(GpuProviderCapabilities.SupportsCompute), descriptor.Metadata[GpuProviderMetadataKeys.GpuCapabilities]);
        Assert.Equal("aikernel.gpu.rev3", descriptor.Metadata[GpuProviderMetadataKeys.ProviderFamily]);
        Assert.Equal("cuda13-native-compute", descriptor.Metadata[GpuProviderMetadataKeys.ProviderRole]);
        Assert.Equal("native-cuda-abi", descriptor.Metadata[GpuDiagnosticsMetadataKeys.Rev3ExecutionMode]);
        Assert.Equal(GpuOperationNames.ComputeDispatch, descriptor.Metadata[GpuDiagnosticsMetadataKeys.Rev3PassId]);
        Assert.Equal("13.0", descriptor.Metadata["cuda.version"]);
        Assert.Equal("win-x64", descriptor.Metadata["rid"]);
        Assert.Equal("AIKernel.Cuda13.0.Libtorch2.12.win-x64", descriptor.Metadata["package.id"]);
        Assert.Equal("AIKERNEL_LIBTORCH_PATH", descriptor.Metadata["runtime.env"]);
        Assert.Equal("loader.json", descriptor.Metadata["loader.config"]);
        Assert.Equal(
            "AIKERNEL_CUDA13_LIBTORCH2_12_WIN_X64_LOADER",
            descriptor.Metadata["loader.env"]);
        Assert.True(descriptor.Metadata.ContainsKey(GpuNativeAbiMetadataKeys.CudaNativeAbiAvailable));
        Assert.True(descriptor.Metadata.ContainsKey(GpuNativeAbiMetadataKeys.CudaNativeAbiReason));
        Assert.Equal(
            LibTorchNativeAbiOptions.DefaultLoaderEnvironmentVariable,
            descriptor.Metadata[GpuNativeAbiMetadataKeys.CudaNativeLoaderEnvironmentVariable]);
        Assert.Equal(
            LibTorchNativeAbiOptions.DefaultLibTorchPathEnvironmentVariable,
            descriptor.Metadata[GpuNativeAbiMetadataKeys.CudaLibTorchPathEnvironmentVariable]);

        var probeValidation = GpuCanonicalValidation.ValidateNativeAbiProbeMetadata(
            GpuBackend.Cuda,
            descriptor.Metadata);
        var executionLayerValidation = GpuCanonicalValidation.ValidateRev3ExecutionLayerMetadata(
            descriptor.Metadata);

        Assert.True(probeValidation.IsValid);
        Assert.True(executionLayerValidation.IsValid);
    }

    [Fact]
    public async Task DiagnosticsProvider_ProjectsCanonicalRev3ProbeDiagnostics()
    {
        var diagnosticsProvider = Assert.IsAssignableFrom<IGpuDiagnostics>(new LibTorchDiagnosticsProvider());
        var frame = new GpuFrameToken
        {
            FrameId = "frame-1",
            FrameIndex = 1,
            SampleTicks = 100,
            RawTarget = new GpuFrameTarget
            {
                TargetId = "raw",
                Backend = GpuBackend.Cuda,
                Kind = GpuFrameTargetKind.RawFramebuffer,
                Width = 320,
                Height = 200,
                PixelFormat = FramePixelFormat.Indexed8
            },
            HudTarget = new GpuFrameTarget
            {
                TargetId = "hud",
                Backend = GpuBackend.Cuda,
                Kind = GpuFrameTargetKind.HudCompositeOffscreen,
                Width = 320,
                Height = 200,
                PixelFormat = FramePixelFormat.Rgba32
            }
        };

        var diagnostics = await diagnosticsProvider.CaptureFrameDiagnosticsAsync(
            frame,
            TestContext.Current.CancellationToken);

        Assert.Equal("frame-1", diagnostics.SensorPath.FrameId);
        Assert.Equal(GpuBackend.Cuda.ToString(), diagnostics.SensorPath.Backend);
        Assert.False(diagnostics.SensorPath.ZeroCopy);
        Assert.Equal(GpuReadbackPolicy.RequiredFallback, diagnostics.SensorPath.Readback);
        Assert.Equal("loader-not-configured", diagnostics.SensorPath.FallbackReason);
        Assert.Equal(320 * 200, diagnostics.SensorPath.MemoryEstimate);
        Assert.Equal(320 * 200 * 4, diagnostics.HudPath.MemoryEstimate);
        Assert.Equal("sensor", diagnostics.SensorPath.Metadata[GpuDiagnosticsMetadataKeys.Rev3PathRole]);
        Assert.Equal("sensor", diagnostics.SensorPath.Metadata[GpuDiagnosticsMetadataKeys.Rev3PassId]);
        Assert.Equal("1", diagnostics.SensorPath.Metadata[GpuDiagnosticsMetadataKeys.Rev3FrameIndex]);
        Assert.Equal("100", diagnostics.SensorPath.Metadata[GpuDiagnosticsMetadataKeys.Rev3SampleTicks]);
        Assert.Equal("native-cuda-abi-probe", diagnostics.SensorPath.Metadata[GpuDiagnosticsMetadataKeys.Rev3ExecutionMode]);
        Assert.Equal("1", diagnostics.SensorPath.Metadata[Cuda13NativeDispatchResponse.AbiVersionMetadataKey]);
        Assert.Equal(
            nameof(Cuda13NativeDispatchStatus.NotInitialized),
            diagnostics.SensorPath.Metadata[Cuda13NativeDispatchResponse.StatusMetadataKey]);
        Assert.Equal(
            nameof(Cuda13NativeDispatchFailureReason.DeviceUnavailable),
            diagnostics.SensorPath.Metadata[Cuda13NativeDispatchResponse.FailureReasonMetadataKey]);
        Assert.Equal("1", diagnostics.SensorPath.Metadata[Cuda13NativeDispatchResponse.FrameIndexMetadataKey]);
        Assert.Equal("100", diagnostics.SensorPath.Metadata[Cuda13NativeDispatchResponse.SampleTicksMetadataKey]);
        Assert.Equal("true", diagnostics.SensorPath.Metadata[GpuDiagnosticsMetadataKeys.Rev3PromotionBlocked]);
        Assert.Equal("false", diagnostics.SensorPath.Metadata[GpuDiagnosticsMetadataKeys.Rev3PromotionCandidateReady]);
        Assert.Equal("false", diagnostics.SensorPath.Metadata[GpuDiagnosticsMetadataKeys.Rev3PromotionDiagnosticStable]);
        Assert.Equal(
            GpuRev3PromotionGates.NativeBridgeRequired,
            diagnostics.SensorPath.Metadata[GpuDiagnosticsMetadataKeys.Rev3PromotionReason]);
        Assert.Equal("native-cuda-buffer-dispatch", diagnostics.SensorPath.Metadata[GpuProviderMetadataKeys.GpuBypass]);
        Assert.Equal("native-cuda-device-buffer", diagnostics.SensorPath.Metadata[GpuProviderMetadataKeys.ZeroCopyBufferHandling]);
        Assert.Contains("ReadyForBuiltIn=false", diagnostics.SensorPath.Metadata[GpuDiagnosticsMetadataKeys.Rev3PassReadiness]);
        Assert.True(GpuCanonicalValidation.EvaluateRev3PromotionReadiness(
            diagnostics.SensorPath.Metadata).IsBlocked);
        Assert.True(GpuCanonicalValidation.ValidateNativeAbiProbeMetadata(
            GpuBackend.Cuda,
            diagnostics.SensorPath.Metadata).IsValid);
        Assert.True(GpuCanonicalValidation.ValidateRev3ExecutionLayerMetadata(diagnostics.SensorPath.Metadata).IsValid);
        Assert.True(GpuCanonicalValidation.ValidateRev3DiagnosticsMetadata(
            diagnostics.SensorPath.Metadata,
            "sensor.metadata").IsValid);
        Assert.True(GpuCanonicalValidation.ValidateFrameDiagnostics(diagnostics).IsValid);
    }

    [Fact]
    public async Task DiagnosticsProvider_ProjectsDefaultFrameTokenMetadata()
    {
        var diagnosticsProvider = Assert.IsAssignableFrom<IGpuDiagnostics>(new LibTorchDiagnosticsProvider());

        var diagnostics = await diagnosticsProvider.CaptureFrameDiagnosticsAsync(
            frame: null,
            TestContext.Current.CancellationToken);

        Assert.Equal("0", diagnostics.SensorPath.Metadata[GpuDiagnosticsMetadataKeys.Rev3FrameIndex]);
        Assert.Equal("0", diagnostics.SensorPath.Metadata[GpuDiagnosticsMetadataKeys.Rev3SampleTicks]);
        Assert.Equal("0", diagnostics.SensorPath.Metadata[Cuda13NativeDispatchResponse.FrameIndexMetadataKey]);
        Assert.Equal("0", diagnostics.SensorPath.Metadata[Cuda13NativeDispatchResponse.SampleTicksMetadataKey]);
        Assert.Equal(
            GpuRev3PromotionGates.NativeBridgeRequired,
            diagnostics.SensorPath.Metadata[GpuDiagnosticsMetadataKeys.Rev3PromotionReason]);
        Assert.True(GpuCanonicalValidation.ValidateRev3DiagnosticsMetadata(
            diagnostics.SensorPath.Metadata,
            "sensor.metadata").IsValid);
        Assert.True(GpuCanonicalValidation.ValidateFrameDiagnostics(diagnostics).IsValid);
    }

    [Fact]
    public async Task InvokeAsync_ComputeDispatchUsesForwardPath()
    {
        var invoker = new LibTorchCapabilityInvoker();
        var request = new CapabilityInvocationRequest(
            InvocationId: "invoke-compute-dispatch-invalid",
            CapabilityId: LibTorchCapabilityDescriptor.CapabilityId,
            Operation: GpuOperationNames.ComputeDispatch,
            Arguments: new Dictionary<string, string>(),
            InputHash: null,
            ReplayLogHash: "sha256:replay",
            Metadata: new Dictionary<string, string>());

        var result = await invoker.InvokeAsync(
            request,
            TestContext.Current.CancellationToken);

        Assert.False(result.Succeeded);
        Assert.Equal("LIBTORCH_FORWARD_REQUEST_INVALID", result.ErrorCode);
        Assert.Equal(GpuOperationNames.ComputeDispatch, result.Metadata["operation"]);
        Assert.Equal("true", result.Metadata["fail_closed"]);
        Assert.Equal("true", result.Metadata[GpuProviderMetadataKeys.Rev3]);
        Assert.Equal(GpuBackend.Cuda.ToString(), result.Metadata[GpuProviderMetadataKeys.GpuBackend]);
        Assert.Equal("native-cuda-buffer-dispatch", result.Metadata[GpuProviderMetadataKeys.GpuBypass]);
        Assert.Equal("native-cuda-device-buffer", result.Metadata[GpuProviderMetadataKeys.ZeroCopyBufferHandling]);
        Assert.Equal("false", result.Metadata[GpuNativeAbiMetadataKeys.CudaNativeAbiAvailable]);
        Assert.Equal("1", result.Metadata[Cuda13NativeDispatchResponse.AbiVersionMetadataKey]);
        Assert.Equal(
            nameof(Cuda13NativeDispatchStatus.NotInitialized),
            result.Metadata[Cuda13NativeDispatchResponse.StatusMetadataKey]);
        Assert.Equal(
            nameof(Cuda13NativeDispatchFailureReason.DeviceUnavailable),
            result.Metadata[Cuda13NativeDispatchResponse.FailureReasonMetadataKey]);
        Assert.True(GpuCanonicalValidation.ValidateNativeAbiProbeMetadata(
            GpuBackend.Cuda,
            result.Metadata).IsValid);
        Assert.True(GpuCanonicalValidation.ValidateRev3ExecutionLayerMetadata(result.Metadata).IsValid);
    }

    [Fact]
    public async Task InvokeAsync_OverwritesSpoofedGpuExecutionMetadata()
    {
        var invoker = new LibTorchCapabilityInvoker();
        var request = new CapabilityInvocationRequest(
            InvocationId: "invoke-compute-dispatch-spoofed",
            CapabilityId: LibTorchCapabilityDescriptor.CapabilityId,
            Operation: GpuOperationNames.ComputeDispatch,
            Arguments: new Dictionary<string, string>(),
            InputHash: null,
            ReplayLogHash: "sha256:replay",
            Metadata: new Dictionary<string, string>
            {
                ["caller_trace"] = "preserve-me",
                [GpuProviderMetadataKeys.Backend] = GpuBackend.WebGpu.ToString(),
                [GpuProviderMetadataKeys.GpuBackend] = GpuBackend.WebGpu.ToString(),
                [GpuProviderMetadataKeys.GpuBypass] = "raw-texture-binding",
                [GpuProviderMetadataKeys.NativeJsBridge] = "rev3-envelope-bridge",
                [GpuProviderMetadataKeys.PassBridge] = "optional-native-or-js",
                [GpuProviderMetadataKeys.ZeroCopyBufferHandling] = "raw-framebuffer-texture"
            });

        var result = await invoker.InvokeAsync(
            request,
            TestContext.Current.CancellationToken);

        Assert.False(result.Succeeded);
        Assert.Equal("preserve-me", result.Metadata["caller_trace"]);
        Assert.Equal("cuda13.0", result.Metadata[GpuProviderMetadataKeys.Backend]);
        Assert.Equal(GpuBackend.Cuda.ToString(), result.Metadata[GpuProviderMetadataKeys.GpuBackend]);
        Assert.Equal("native-cuda-buffer-dispatch", result.Metadata[GpuProviderMetadataKeys.GpuBypass]);
        Assert.Equal("not-required-native-provider", result.Metadata[GpuProviderMetadataKeys.NativeJsBridge]);
        Assert.Equal("native-abi", result.Metadata[GpuProviderMetadataKeys.PassBridge]);
        Assert.Equal("native-cuda-device-buffer", result.Metadata[GpuProviderMetadataKeys.ZeroCopyBufferHandling]);
        Assert.True(GpuCanonicalValidation.ValidateNativeAbiProbeMetadata(
            GpuBackend.Cuda,
            result.Metadata).IsValid);
        Assert.True(GpuCanonicalValidation.ValidateRev3ExecutionLayerMetadata(result.Metadata).IsValid);
    }

    [Fact]
    public async Task InvokeAsync_ProjectsExplicitNativeAbiProbeMetadata()
    {
        var tempDirectory = Path.Combine(
            Path.GetTempPath(),
            "aikernel-cuda-probe-" + Guid.NewGuid().ToString("N"));
        var libTorchDirectory = Path.Combine(tempDirectory, "libtorch");
        var bridgePath = Path.Combine(tempDirectory, "libtorch_bridge.dll");

        try
        {
            Directory.CreateDirectory(libTorchDirectory);
            File.WriteAllText(bridgePath, "fake native bridge marker");

            var invoker = new LibTorchCapabilityInvoker(
                nativeAbiOptions: new LibTorchNativeAbiOptions
                {
                    LibTorchPath = libTorchDirectory,
                    NativeBridgePath = bridgePath,
                    EnableNativeValidation = true
                });
            var request = new CapabilityInvocationRequest(
                InvocationId: "invoke-compute-dispatch-probe",
                CapabilityId: LibTorchCapabilityDescriptor.CapabilityId,
                Operation: GpuOperationNames.ComputeDispatch,
                Arguments: new Dictionary<string, string>(),
                InputHash: null,
                ReplayLogHash: "sha256:replay",
                Metadata: new Dictionary<string, string>());

            var result = await invoker.InvokeAsync(
                request,
                TestContext.Current.CancellationToken);

            Assert.False(result.Succeeded);
            Assert.Equal("LIBTORCH_FORWARD_REQUEST_INVALID", result.ErrorCode);
            Assert.Equal("true", result.Metadata[GpuNativeAbiMetadataKeys.CudaNativeAbiAvailable]);
            Assert.Equal("available", result.Metadata[GpuNativeAbiMetadataKeys.CudaNativeAbiReason]);
            Assert.Equal("true", result.Metadata[GpuNativeAbiMetadataKeys.CudaNativeBridgeAvailable]);
            Assert.Equal("true", result.Metadata[GpuNativeAbiMetadataKeys.CudaLibTorchPathAvailable]);
            Assert.Equal("requested", result.Metadata[GpuNativeAbiMetadataKeys.CudaNativeValidation]);
            Assert.Equal(bridgePath, result.Metadata[GpuNativeAbiMetadataKeys.CudaNativeBridgePath]);
            Assert.Equal(libTorchDirectory, result.Metadata[GpuNativeAbiMetadataKeys.CudaLibTorchPath]);
            Assert.Equal(
                nameof(Cuda13NativeDispatchFailureReason.CommandSubmissionDisabled),
                result.Metadata[Cuda13NativeDispatchResponse.FailureReasonMetadataKey]);
            Assert.True(GpuCanonicalValidation.ValidateNativeAbiProbeMetadata(
                GpuBackend.Cuda,
                result.Metadata).IsValid);
            Assert.True(GpuCanonicalValidation.ValidateRev3ExecutionLayerMetadata(result.Metadata).IsValid);
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }
    }

    [Fact]
    public void NativeHeaderPinsCuda13DispatchAbi()
    {
        var repoRoot = ResolveRepositoryRoot();
        var headerPath = Path.Combine(repoRoot, "native", "libtorch_bridge.h");
        var header = File.ReadAllText(headerPath);

        Assert.Contains("AIKernelCuda13DispatchRequestHeader", header);
        Assert.Contains("AIKernelCuda13DispatchResponseHeader", header);
        Assert.Contains("AIKERNEL_CUDA13_PASS_COMPUTE_DISPATCH", header);
        Assert.Contains("AIKERNEL_CUDA13_PASS_AISTHESIS", header);
        Assert.Contains("AIKERNEL_CUDA13_PASS_SPATIAL_REASONING", header);
        Assert.Contains("AIKERNEL_CUDA13_PASS_HUD_COMPOSITE", header);
        Assert.Contains("AIKERNEL_CUDA13_FAILURE_COMMAND_SUBMISSION_DISABLED", header);
        Assert.Contains("aikernel_cuda13_dispatch", header);
    }

    [Fact]
    public void Cuda13NativeDispatchRequestBuilderMapsOperationsAndFrameToken()
    {
        var frame = new GpuFrameToken
        {
            FrameId = "frame-7",
            FrameIndex = 7,
            SampleTicks = 9001
        };

        var request = Cuda13NativeDispatchRequestBuilder.Create(
            GpuOperationNames.ComputeDispatch,
            frame,
            flags: 3,
            payloadBytes: 128);
        var bytes = request.ToNativeBytes();

        Assert.Equal(Cuda13NativeDispatchRequest.HeaderByteSize, bytes.Length);
        Assert.Equal(Cuda13NativeDispatchRequest.CurrentAbiVersion, BitConverter.ToUInt32(bytes, 0));
        Assert.Equal((uint)Cuda13NativeDispatchRequest.HeaderByteSize, BitConverter.ToUInt32(bytes, 4));
        Assert.Equal((uint)Cuda13NativePassId.ComputeDispatch, BitConverter.ToUInt32(bytes, 8));
        Assert.Equal(3u, BitConverter.ToUInt32(bytes, 12));
        Assert.Equal(7ul, BitConverter.ToUInt64(bytes, 16));
        Assert.Equal(9001ul, BitConverter.ToUInt64(bytes, 24));
        Assert.Equal(128u, BitConverter.ToUInt32(bytes, 32));
        Assert.Equal(0u, BitConverter.ToUInt32(bytes, 36));
        Assert.True(Cuda13NativeDispatchRequestBuilder.TryMapOperation("load_model", out var loadModelPass));
        Assert.Equal(Cuda13NativePassId.LoadModel, loadModelPass);
        Assert.True(Cuda13NativeDispatchRequestBuilder.TryMapOperation("unload_model", out var unloadModelPass));
        Assert.Equal(Cuda13NativePassId.UnloadModel, unloadModelPass);
        Assert.True(Cuda13NativeDispatchRequestBuilder.TryMapOperation("forward", out var forwardPass));
        Assert.Equal(Cuda13NativePassId.Forward, forwardPass);
        Assert.True(Cuda13NativeDispatchRequestBuilder.TryMapOperation(GpuOperationNames.GpuAisthesisRawFrame, out var aisthesisPass));
        Assert.Equal(Cuda13NativePassId.Aisthesis, aisthesisPass);
        Assert.True(Cuda13NativeDispatchRequestBuilder.TryMapOperation(GpuOperationNames.GpuSpatialReasoning, out var spatialPass));
        Assert.Equal(Cuda13NativePassId.SpatialReasoning, spatialPass);
        Assert.True(Cuda13NativeDispatchRequestBuilder.TryMapOperation(GpuOperationNames.GpuHudComposite, out var hudPass));
        Assert.Equal(Cuda13NativePassId.HudComposite, hudPass);
    }

    [Fact]
    public void Cuda13NativeDispatchResponseProjectsMetadata()
    {
        var responsePointer = Marshal.AllocHGlobal(Cuda13NativeDispatchRequest.ResponseHeaderByteSize);
        try
        {
            Marshal.WriteInt32(responsePointer, 0, unchecked((int)Cuda13NativeDispatchRequest.CurrentAbiVersion));
            Marshal.WriteInt32(responsePointer, 4, Cuda13NativeDispatchRequest.ResponseHeaderByteSize);
            Marshal.WriteInt32(responsePointer, 8, (int)Cuda13NativeDispatchStatus.NotInitialized);
            Marshal.WriteInt32(responsePointer, 12, (int)Cuda13NativeDispatchFailureReason.CommandSubmissionDisabled);
            Marshal.WriteInt64(responsePointer, 16, 11);
            Marshal.WriteInt64(responsePointer, 24, 1200);
            Marshal.WriteInt32(responsePointer, 32, 0);
            Marshal.WriteInt32(responsePointer, 36, 0);

            var response = Cuda13NativeDispatchResponse.FromNativePointer(responsePointer);
            var metadata = response.ToMetadata();

            Assert.Equal(Cuda13NativeDispatchStatus.NotInitialized, response.Status);
            Assert.Equal(Cuda13NativeDispatchFailureReason.CommandSubmissionDisabled, response.FailureReason);
            Assert.Equal("1", metadata[Cuda13NativeDispatchResponse.AbiVersionMetadataKey]);
            Assert.Equal("NotInitialized", metadata[Cuda13NativeDispatchResponse.StatusMetadataKey]);
            Assert.Equal("CommandSubmissionDisabled", metadata[Cuda13NativeDispatchResponse.FailureReasonMetadataKey]);
            Assert.Equal("11", metadata[Cuda13NativeDispatchResponse.FrameIndexMetadataKey]);
            Assert.Equal("1200", metadata[Cuda13NativeDispatchResponse.SampleTicksMetadataKey]);
            Assert.Equal("0", metadata[Cuda13NativeDispatchResponse.DiagnosticsBytesMetadataKey]);
        }
        finally
        {
            Marshal.FreeHGlobal(responsePointer);
        }
    }

    [Fact]
    public void NativeDispatch_WhenBuiltFixtureLoads_ReturnsFailClosedDispatchResponse()
    {
        var nativePath = TryGetBuiltCuda13BridgeLibraryPath();
        if (nativePath is null)
        {
            return;
        }

        IntPtr library = IntPtr.Zero;
        IntPtr requestPointer = IntPtr.Zero;
        var responsePointer = Marshal.AllocHGlobal(Cuda13NativeDispatchRequest.ResponseHeaderByteSize);
        try
        {
            if (!NativeLibrary.TryLoad(nativePath, out library) || library == IntPtr.Zero)
            {
                return;
            }

            Assert.True(NativeLibrary.TryGetExport(
                library,
                "aikernel_cuda13_dispatch",
                out var dispatchExport));
            var dispatch = Marshal.GetDelegateForFunctionPointer<Cuda13DispatchDelegate>(dispatchExport);
            requestPointer = AllocateNativeBytes(Cuda13NativeDispatchRequestBuilder.Create(
                GpuOperationNames.ComputeDispatch,
                new GpuFrameToken
                {
                    FrameId = "native-fixture-frame",
                    FrameIndex = 12,
                    SampleTicks = 345
                }).ToNativeBytes());

            var status = dispatch(
                requestPointer,
                Cuda13NativeDispatchRequest.HeaderByteSize,
                responsePointer,
                Cuda13NativeDispatchRequest.ResponseHeaderByteSize);
            var response = Cuda13NativeDispatchResponse.FromNativePointer(responsePointer);

            Assert.Equal((uint)Cuda13NativeDispatchStatus.NotInitialized, status);
            Assert.Equal(Cuda13NativeDispatchStatus.NotInitialized, response.Status);
            Assert.Equal(Cuda13NativeDispatchFailureReason.CommandSubmissionDisabled, response.FailureReason);
            Assert.Equal(12UL, response.FrameIndex);
            Assert.Equal(345UL, response.SampleTicks);

            status = dispatch(
                requestPointer,
                8,
                responsePointer,
                Cuda13NativeDispatchRequest.ResponseHeaderByteSize);
            response = Cuda13NativeDispatchResponse.FromNativePointer(responsePointer);

            Assert.Equal((uint)Cuda13NativeDispatchStatus.NotInitialized, status);
            Assert.Equal(Cuda13NativeDispatchFailureReason.InvalidRequestLength, response.FailureReason);
        }
        finally
        {
            if (requestPointer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(requestPointer);
            }

            Marshal.FreeHGlobal(responsePointer);
            if (library != IntPtr.Zero)
            {
                NativeLibrary.Free(library);
            }
        }
    }

    [Fact]
    public async Task InvokeAsync_ReturnsFailClosedForUnknownOperationWithoutLoadingNativeLibrary()
    {
        var invoker = new LibTorchCapabilityInvoker();
        var request = new CapabilityInvocationRequest(
            InvocationId: "invoke-unsupported",
            CapabilityId: LibTorchCapabilityDescriptor.CapabilityId,
            Operation: "unknown",
            Arguments: new Dictionary<string, string>(),
            InputHash: null,
            ReplayLogHash: "sha256:replay",
            Metadata: new Dictionary<string, string>());

        var result = await invoker.InvokeAsync(
            request,
            TestContext.Current.CancellationToken);

        Assert.False(result.Succeeded);
        Assert.Equal("LIBTORCH_UNSUPPORTED_OPERATION", result.ErrorCode);
        Assert.Equal("sha256:replay", result.ReplayLogHash);
        Assert.Equal("true", result.Metadata["fail_closed"]);
        Assert.Equal(
            nameof(Cuda13NativeDispatchFailureReason.UnknownPass),
            result.Metadata[Cuda13NativeDispatchResponse.FailureReasonMetadataKey]);
    }

    [Fact]
    public async Task InvokeAsync_ReturnsFailClosedForCapabilityIdMismatch()
    {
        var invoker = new LibTorchCapabilityInvoker();
        var request = new CapabilityInvocationRequest(
            InvocationId: "invoke-mismatched-capability",
            CapabilityId: "other.capability",
            Operation: "load_model",
            Arguments: new Dictionary<string, string>
            {
                ["path"] = "model.pt"
            },
            InputHash: null,
            ReplayLogHash: "sha256:replay",
            Metadata: new Dictionary<string, string>());

        var result = await invoker.InvokeAsync(
            request,
            TestContext.Current.CancellationToken);

        Assert.False(result.Succeeded);
        Assert.Equal("LIBTORCH_CAPABILITY_ID_MISMATCH", result.ErrorCode);
        Assert.Equal("sha256:replay", result.ReplayLogHash);
        Assert.Equal("true", result.Metadata["fail_closed"]);
    }

    [Fact]
    public async Task InvokeAsync_ReturnsFailClosedWhenNativeLoadCannotExecute()
    {
        var invoker = new LibTorchCapabilityInvoker();
        var request = new CapabilityInvocationRequest(
            InvocationId: "invoke-load-missing-native",
            CapabilityId: LibTorchCapabilityDescriptor.CapabilityId,
            Operation: "load_model",
            Arguments: new Dictionary<string, string>
            {
                ["path"] = "missing-model.pt"
            },
            InputHash: null,
            ReplayLogHash: "sha256:replay",
            Metadata: new Dictionary<string, string>());

        var result = await invoker.InvokeAsync(
            request,
            TestContext.Current.CancellationToken);

        Assert.False(result.Succeeded);
        Assert.Equal("sha256:replay", result.ReplayLogHash);
        Assert.Equal("true", result.Metadata["fail_closed"]);
        Assert.Equal("libtorch_native_abi", result.Metadata["failure_origin"]);
    }

    [Fact]
    public async Task InvokeAsync_RejectsInvalidUnloadModelHandle()
    {
        var invoker = new LibTorchCapabilityInvoker();
        var request = new CapabilityInvocationRequest(
            InvocationId: "invoke-unload-invalid-handle",
            CapabilityId: LibTorchCapabilityDescriptor.CapabilityId,
            Operation: "unload_model",
            Arguments: new Dictionary<string, string>
            {
                ["model_handle"] = "0"
            },
            InputHash: null,
            ReplayLogHash: "sha256:replay",
            Metadata: new Dictionary<string, string>());

        var result = await invoker.InvokeAsync(
            request,
            TestContext.Current.CancellationToken);

        Assert.False(result.Succeeded);
        Assert.Equal("LIBTORCH_MODEL_HANDLE_INVALID", result.ErrorCode);
        Assert.Equal("true", result.Metadata["fail_closed"]);
    }

    [Fact]
    public async Task InvokeAsync_ReturnsFailClosedWhenNativeUnloadCannotExecute()
    {
        var invoker = new LibTorchCapabilityInvoker();
        var request = new CapabilityInvocationRequest(
            InvocationId: "invoke-unload-missing-native",
            CapabilityId: LibTorchCapabilityDescriptor.CapabilityId,
            Operation: "unload_model",
            Arguments: new Dictionary<string, string>
            {
                ["model_handle"] = "42"
            },
            InputHash: null,
            ReplayLogHash: "sha256:replay",
            Metadata: new Dictionary<string, string>());

        var result = await invoker.InvokeAsync(
            request,
            TestContext.Current.CancellationToken);

        Assert.False(result.Succeeded);
        Assert.Equal("sha256:replay", result.ReplayLogHash);
        Assert.Equal("true", result.Metadata["fail_closed"]);
        Assert.Equal("libtorch_native_abi", result.Metadata["failure_origin"]);
    }

    [Fact]
    public void TryCreate_ParsesForwardRequestArguments()
    {
        var result = LlamaForwardRequest.TryCreate(
            new Dictionary<string, string>
            {
                ["model_handle"] = "42",
                ["input_ids"] = "1, 2, 3"
            });

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Value);
        Assert.Equal(42, result.Value.ModelHandle);
        Assert.Equal([1, 2, 3], result.Value.InputIds);
    }

    [Fact]
    public void TryCreate_RejectsInvalidInputIds()
    {
        var result = LlamaForwardRequest.TryCreate(
            new Dictionary<string, string>
            {
                ["model_handle"] = "42",
                ["input_ids"] = "1, nope"
            });

        Assert.False(result.Succeeded);
        Assert.Null(result.Value);
        Assert.Contains("input_ids", result.ErrorMessage);
    }

    [Fact]
    public void TryCreate_RejectsEmptyInputIds()
    {
        var result = LlamaForwardRequest.TryCreate(
            new Dictionary<string, string>
            {
                ["model_handle"] = "42",
                ["input_ids"] = ", ,"
            });

        Assert.False(result.Succeeded);
        Assert.Null(result.Value);
        Assert.Contains("at least one", result.ErrorMessage);
    }

    [Fact]
    public void TryCreate_RejectsInputIdsOverLimit()
    {
        var inputIds = string.Join(
            ",",
            Enumerable.Range(0, LlamaForwardRequest.MaxInputTokens + 1));

        var result = LlamaForwardRequest.TryCreate(
            new Dictionary<string, string>
            {
                ["model_handle"] = "42",
                ["input_ids"] = inputIds
            });

        Assert.False(result.Succeeded);
        Assert.Null(result.Value);
        Assert.Contains("at most", result.ErrorMessage);
    }

    private sealed class FailingMemoryMapper : IMemoryMapper
    {
        public IMemoryRegion Open(
            string path,
            CoreMemoryAccessMode accessMode = CoreMemoryAccessMode.Read)
            => throw new InvalidOperationException("mapped model unavailable");

        public Result<IMemoryRegion> OpenResult(
            string path,
            CoreMemoryAccessMode accessMode = CoreMemoryAccessMode.Read)
            => Result<IMemoryRegion>.Fail(new ErrorContext(
                "mapped model unavailable",
                "TEST_MEMORY_MAP_FAILED",
                false));
    }

    private sealed class SuccessfulMemoryMapper(
        string mappedPath,
        long length) : IMemoryMapper
    {
        public string? OpenedPath { get; private set; }

        public CoreMemoryAccessMode? OpenedAccessMode { get; private set; }

        public IMemoryRegion Open(
            string path,
            CoreMemoryAccessMode accessMode = CoreMemoryAccessMode.Read)
        {
            OpenedPath = path;
            OpenedAccessMode = accessMode;

            return new FakeMemoryRegion(
                new MemoryRegionInfo(
                    mappedPath,
                    length,
                    accessMode));
        }

        public Result<IMemoryRegion> OpenResult(
            string path,
            CoreMemoryAccessMode accessMode = CoreMemoryAccessMode.Read)
            => Result<IMemoryRegion>.Success(Open(path, accessMode));
    }

    private sealed class FakeMemoryRegion(
        MemoryRegionInfo info) : IMemoryRegion
    {
        public MemoryRegionInfo Info { get; } = info;

        public IntPtr Pointer { get; } = new(1);

        public long Length => Info.Length;

        public bool IsMapped { get; private set; } = true;

        public bool Unmap()
        {
            IsMapped = false;
            return true;
        }

        public void Dispose()
        {
            _ = Unmap();
        }
    }

    [Fact]
    public async Task InvokeAsync_ReturnsFailClosedWhenMemoryMapperCannotOpenModel()
    {
        var invoker = new LibTorchCapabilityInvoker(new FailingMemoryMapper());
        var request = new CapabilityInvocationRequest(
            InvocationId: "invoke-load-memory-map-failed",
            CapabilityId: LibTorchCapabilityDescriptor.CapabilityId,
            Operation: "load_model",
            Arguments: new Dictionary<string, string>
            {
                ["path"] = "missing-model.pt"
            },
            InputHash: null,
            ReplayLogHash: "sha256:replay",
            Metadata: new Dictionary<string, string>());

        var result = await invoker.InvokeAsync(
            request,
            TestContext.Current.CancellationToken);

        Assert.False(result.Succeeded);
        Assert.Equal("LIBTORCH_MEMORY_MAP_FAILED", result.ErrorCode);
        Assert.Equal("sha256:replay", result.ReplayLogHash);
        Assert.Equal("true", result.Metadata["fail_closed"]);
        Assert.Equal("libtorch_native_abi", result.Metadata["failure_origin"]);
    }

    [Fact]
    public async Task InvokeAsync_UsesMappedModelPathBeforeNativeLoad()
    {
        var mapper = new SuccessfulMemoryMapper(
            "mapped-model.pt",
            1234);
        var invoker = new LibTorchCapabilityInvoker(mapper);
        var request = new CapabilityInvocationRequest(
            InvocationId: "invoke-load-mapped-native-missing",
            CapabilityId: LibTorchCapabilityDescriptor.CapabilityId,
            Operation: "load_model",
            Arguments: new Dictionary<string, string>
            {
                ["path"] = "logical-model.pt"
            },
            InputHash: null,
            ReplayLogHash: "sha256:replay",
            Metadata: new Dictionary<string, string>());

        var result = await invoker.InvokeAsync(
            request,
            TestContext.Current.CancellationToken);

        Assert.Equal("logical-model.pt", mapper.OpenedPath);
        Assert.Equal(CoreMemoryAccessMode.Read, mapper.OpenedAccessMode);
        Assert.False(result.Succeeded);
        Assert.Equal("LIBTORCH_NATIVE_ABI_UNAVAILABLE", result.ErrorCode);
        Assert.Equal("sha256:replay", result.ReplayLogHash);
        Assert.Equal("true", result.Metadata["fail_closed"]);
    }

    private static string ResolveRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "native", "libtorch_bridge.h")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate AIKernel.Cuda13.0 repository root.");
    }

    private static string? TryGetBuiltCuda13BridgeLibraryPath()
    {
        var repoRoot = ResolveRepositoryRoot();
        var candidate = Path.Combine(
            repoRoot,
            "native",
            "build",
            "win-x64",
            "Release",
            "libtorch_bridge.dll");

        return OperatingSystem.IsWindows() && File.Exists(candidate)
            ? candidate
            : null;
    }

    private static IntPtr AllocateNativeBytes(byte[] bytes)
    {
        var pointer = Marshal.AllocHGlobal(bytes.Length);
        Marshal.Copy(bytes, 0, pointer, bytes.Length);
        return pointer;
    }
}

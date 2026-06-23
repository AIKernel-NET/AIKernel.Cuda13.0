namespace AIKernel.Cuda13.Libtorch2_12.WinX64.Capability;

using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using AIKernel.Abstractions.Capabilities;
using AIKernel.Cuda13.Libtorch2_12.WinX64.Interop;
using AIKernel.Cuda13.Libtorch2_12.WinX64.Model;
using AIKernel.Core.Memory;
using AIKernel.Dtos.Capabilities;
using AIKernel.Dtos.Gpu;
using AIKernel.Enums;
using CoreMemoryAccessMode = AIKernel.Core.Memory.MemoryAccessMode;

/// <summary>[EN] Documents this public package API member. [JA] LibTorchCapabilityInvoker を表します。</summary>
/// <include file="docs.en.xml" path="doc/members/member[@name='T:AIKernel.Cuda13.Libtorch2_12.WinX64.Capability.LibTorchCapabilityInvoker']" />
/// <include file="docs.ja.xml" path="doc/members/member[@name='T:AIKernel.Cuda13.Libtorch2_12.WinX64.Capability.LibTorchCapabilityInvoker']" />
public sealed class LibTorchCapabilityInvoker : ICapabilityModuleInvoker
{
    private readonly IMemoryMapper? _memoryMapper;
    private readonly LibTorchNativeAbiOptions _nativeAbiOptions;

    /// <summary>[EN] Documents this public package API member. [JA] LibTorchCapabilityInvoker を取得します。</summary>
    /// <include file="docs.en.xml" path="doc/members/member[@name='M:AIKernel.Cuda13.Libtorch2_12.WinX64.Capability.LibTorchCapabilityInvoker.#ctor']" />
    /// <include file="docs.ja.xml" path="doc/members/member[@name='M:AIKernel.Cuda13.Libtorch2_12.WinX64.Capability.LibTorchCapabilityInvoker.#ctor']" />
    public LibTorchCapabilityInvoker(
        IMemoryMapper? memoryMapper = null,
        LibTorchNativeAbiOptions? nativeAbiOptions = null)
    {
        _memoryMapper = memoryMapper;
        _nativeAbiOptions = nativeAbiOptions ?? new LibTorchNativeAbiOptions();
    }

    /// <summary>[EN] Documents this public package API member. [JA] InvokeAsync を取得します。</summary>
    /// <include file="docs.en.xml" path="doc/members/member[@name='M:AIKernel.Cuda13.Libtorch2_12.WinX64.Capability.LibTorchCapabilityInvoker.InvokeAsync']" />
    /// <include file="docs.ja.xml" path="doc/members/member[@name='M:AIKernel.Cuda13.Libtorch2_12.WinX64.Capability.LibTorchCapabilityInvoker.InvokeAsync']" />
    public ValueTask<CapabilityInvocationResult> InvokeAsync(
        CapabilityInvocationRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(request);

        if (!string.Equals(
            request.CapabilityId,
            LibTorchCapabilityDescriptor.CapabilityId,
            StringComparison.Ordinal))
        {
            return ValueTask.FromResult(Fail(
                request,
                "LIBTORCH_CAPABILITY_ID_MISMATCH",
                "The LibTorch invoker can only execute the LibTorch CUDA capability."));
        }

        return request.Operation switch
        {
            GpuOperationNames.ComputeDispatch => Forward(request, cancellationToken),
            "load_model" => LoadModel(request, cancellationToken),
            "unload_model" => UnloadModel(request, cancellationToken),
            "forward" => Forward(request, cancellationToken),
            _ => ValueTask.FromResult(Fail(
                request,
                "LIBTORCH_UNSUPPORTED_OPERATION",
                $"Unsupported LibTorch operation '{request.Operation}'."))
        };
    }

    private ValueTask<CapabilityInvocationResult> LoadModel(
        CapabilityInvocationRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!request.Arguments.TryGetValue("path", out var path) ||
            string.IsNullOrWhiteSpace(path))
        {
            return ValueTask.FromResult(Fail(
                request,
                "LIBTORCH_MODEL_PATH_REQUIRED",
                "Argument 'path' is required for load_model."));
        }

        var mappedPath = ResolveMappedModelPath(request, path);
        if (!mappedPath.Succeeded)
            return ValueTask.FromResult(mappedPath.Result!);

        int statusOrHandle;

        try
        {
            statusOrHandle = NativeMethods.LoadModel(mappedPath.Path!);
        }
        catch (Exception ex) when (IsNativeBoundaryException(ex))
        {
            return ValueTask.FromResult(NativeBoundaryFail(request, ex));
        }

        if (statusOrHandle <= 0)
        {
            return ValueTask.FromResult(Fail(
                request,
                "LIBTORCH_LOAD_MODEL_FAILED",
                $"Native load_model failed with status {statusOrHandle}."));
        }

        var metadata = CreateMetadata(request);
        metadata["model_handle"] = statusOrHandle.ToString(CultureInfo.InvariantCulture);
        metadata["model_path_hash"] = Hash(mappedPath.Path!);
        if (mappedPath.RegionLength is { } regionLength)
        {
            metadata["memory_region_length"] =
                regionLength.ToString(CultureInfo.InvariantCulture);
        }

        return ValueTask.FromResult(Success(
            request,
            outputHash: Hash($"load_model:{statusOrHandle}:{mappedPath.Path}"),
            metadata));
    }

    private ValueTask<CapabilityInvocationResult> UnloadModel(
        CapabilityInvocationRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!TryParsePositiveModelHandle(request, out var modelHandle, out var errorMessage))
        {
            return ValueTask.FromResult(Fail(
                request,
                "LIBTORCH_MODEL_HANDLE_INVALID",
                errorMessage));
        }

        int status;

        try
        {
            status = NativeMethods.UnloadModel(modelHandle);
        }
        catch (Exception ex) when (IsNativeBoundaryException(ex))
        {
            return ValueTask.FromResult(NativeBoundaryFail(request, ex));
        }

        if (status != NativeStatus.Success)
        {
            return ValueTask.FromResult(Fail(
                request,
                "LIBTORCH_UNLOAD_MODEL_FAILED",
                $"Native unload_model failed with status {status}."));
        }

        var metadata = CreateMetadata(request);
        metadata["model_handle"] = modelHandle.ToString(CultureInfo.InvariantCulture);

        return ValueTask.FromResult(Success(
            request,
            outputHash: Hash($"unload_model:{modelHandle}"),
            metadata));
    }

    private ValueTask<CapabilityInvocationResult> Forward(
        CapabilityInvocationRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var parse = LlamaForwardRequest.TryCreate(request.Arguments);

        if (!parse.Succeeded || parse.Value is null)
        {
            return ValueTask.FromResult(Fail(
                request,
                "LIBTORCH_FORWARD_REQUEST_INVALID",
                parse.ErrorMessage ?? "Invalid forward request."));
        }

        var forwardRequest = parse.Value;
        var nativeResult = new ForwardResultNative();
        int status;

        try
        {
            status = NativeMethods.Forward(
                forwardRequest.ModelHandle,
                forwardRequest.InputIds,
                forwardRequest.InputIds.Length,
                out nativeResult);
        }
        catch (Exception ex) when (IsNativeBoundaryException(ex))
        {
            return ValueTask.FromResult(NativeBoundaryFail(request, ex));
        }

        if (status != NativeStatus.Success)
        {
            return ValueTask.FromResult(Fail(
                request,
                "LIBTORCH_FORWARD_FAILED",
                $"Native forward failed with status {status}."));
        }

        var result = LlamaForwardResult.FromNative(nativeResult);
        var metadata = CreateMetadata(request);
        metadata["output_tokens"] = string.Join(",", result.OutputTokenIds);
        metadata["logits_count"] = result.Logits.Count.ToString(CultureInfo.InvariantCulture);

        return ValueTask.FromResult(Success(
            request,
            outputHash: result.OutputHash,
            metadata));
    }

    private static CapabilityInvocationResult Success(
        CapabilityInvocationRequest request,
        string outputHash,
        IReadOnlyDictionary<string, string> metadata)
    {
        return new CapabilityInvocationResult(
            request.InvocationId,
            request.CapabilityId,
            Succeeded: true,
            OutputHash: outputHash,
            ErrorCode: null,
            ErrorMessage: null,
            ReplayLogHash: request.ReplayLogHash,
            Metadata: metadata);
    }

    private CapabilityInvocationResult Fail(
        CapabilityInvocationRequest request,
        string errorCode,
        string errorMessage)
    {
        var metadata = CreateMetadata(request);
        metadata["fail_closed"] = "true";
        metadata["failure_origin"] = "libtorch_native_abi";

        return new CapabilityInvocationResult(
            request.InvocationId,
            request.CapabilityId,
            Succeeded: false,
            OutputHash: null,
            ErrorCode: errorCode,
            ErrorMessage: errorMessage,
            ReplayLogHash: request.ReplayLogHash,
            Metadata: metadata);
    }

    private CapabilityInvocationResult NativeBoundaryFail(
        CapabilityInvocationRequest request,
        Exception exception)
    {
        var metadata = CreateMetadata(request);
        metadata["fail_closed"] = "true";
        metadata["failure_origin"] = "libtorch_native_abi";
        metadata["native_exception"] = exception.GetType().Name;

        return new CapabilityInvocationResult(
            request.InvocationId,
            request.CapabilityId,
            Succeeded: false,
            OutputHash: null,
            ErrorCode: "LIBTORCH_NATIVE_ABI_UNAVAILABLE",
            ErrorMessage: "LibTorch native ABI invocation failed before execution.",
            ReplayLogHash: request.ReplayLogHash,
            Metadata: metadata);
    }

    private static bool IsNativeBoundaryException(
        Exception exception)
    {
        return exception is DllNotFoundException or
            EntryPointNotFoundException or
            BadImageFormatException or
            SEHException;
    }

    private static bool TryParsePositiveModelHandle(
        CapabilityInvocationRequest request,
        out int modelHandle,
        out string errorMessage)
    {
        if (request.Arguments.TryGetValue("model_handle", out var modelHandleText) &&
            int.TryParse(
                modelHandleText,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out modelHandle) &&
            modelHandle > 0)
        {
            errorMessage = string.Empty;
            return true;
        }

        modelHandle = 0;
        errorMessage = "Argument 'model_handle' must be a positive integer.";
        return false;
    }

    private Dictionary<string, string> CreateMetadata(
        CapabilityInvocationRequest request)
    {
        var metadata = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in request.Metadata)
        {
            metadata[pair.Key] = pair.Value;
        }

        foreach (var pair in LibTorchCapabilityDescriptor.Create().Metadata)
        {
            metadata[pair.Key] = pair.Value;
        }

        metadata["capability"] = LibTorchCapabilityDescriptor.CapabilityId;
        metadata["native_library"] = NativeMethods.LibraryName;
        metadata["operation"] = request.Operation;

        var nativeStatus = LibTorchNativeAbiProbe.Probe(_nativeAbiOptions);
        foreach (var pair in nativeStatus.Metadata)
        {
            metadata[pair.Key] = pair.Value;
        }

        AddNativeDispatchMetadata(metadata, request.Operation, nativeStatus);

        return new Dictionary<string, string>(metadata, StringComparer.Ordinal);
    }

    private static void AddNativeDispatchMetadata(
        SortedDictionary<string, string> metadata,
        string operation,
        LibTorchNativeAbiStatus nativeStatus)
    {
        var failureReason = ResolveDispatchFailureReason(operation, nativeStatus);
        var response = new Cuda13NativeDispatchResponse
        {
            AbiVersion = Cuda13NativeDispatchRequest.CurrentAbiVersion,
            HeaderSize = Cuda13NativeDispatchRequest.ResponseHeaderByteSize,
            Status = Cuda13NativeDispatchStatus.NotInitialized,
            FailureReason = failureReason
        };

        foreach (var pair in response.ToMetadata())
        {
            metadata[pair.Key] = pair.Value;
        }
    }

    private static Cuda13NativeDispatchFailureReason ResolveDispatchFailureReason(
        string operation,
        LibTorchNativeAbiStatus nativeStatus)
    {
        if (!Cuda13NativeDispatchRequestBuilder.TryMapOperation(operation, out _))
        {
            return Cuda13NativeDispatchFailureReason.UnknownPass;
        }

        if (!nativeStatus.BridgeAvailable || !nativeStatus.RuntimeAvailable)
        {
            return Cuda13NativeDispatchFailureReason.DeviceUnavailable;
        }

        return Cuda13NativeDispatchFailureReason.CommandSubmissionDisabled;
    }

    private static string Hash(
        string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return "sha256:" + Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private sealed record MappedModelPath(
        bool Succeeded,
        string? Path,
        long? RegionLength,
        CapabilityInvocationResult? Result)
    {
        /// <summary>[EN] Documents this public package API member. [JA] Success を取得します。</summary>
        /// <include file="docs.en.xml" path="doc/members/member[@name='M:AIKernel.Cuda13.Libtorch2_12.WinX64.Capability.LibTorchCapabilityInvoker.Success']" />
        /// <include file="docs.ja.xml" path="doc/members/member[@name='M:AIKernel.Cuda13.Libtorch2_12.WinX64.Capability.LibTorchCapabilityInvoker.Success']" />
        public static MappedModelPath Success(
            string path,
            long? regionLength)
            => new(true, path, regionLength, null);

        /// <summary>[EN] Documents this public package API member. [JA] Fail を取得します。</summary>
        /// <include file="docs.en.xml" path="doc/members/member[@name='M:AIKernel.Cuda13.Libtorch2_12.WinX64.Capability.LibTorchCapabilityInvoker.Fail']" />
        /// <include file="docs.ja.xml" path="doc/members/member[@name='M:AIKernel.Cuda13.Libtorch2_12.WinX64.Capability.LibTorchCapabilityInvoker.Fail']" />
        public static MappedModelPath Fail(
            CapabilityInvocationResult result)
            => new(false, null, null, result);
    }

    private MappedModelPath ResolveMappedModelPath(
        CapabilityInvocationRequest request,
        string path)
    {
        if (_memoryMapper is null)
            return MappedModelPath.Success(path, regionLength: null);

        IMemoryRegion region;
        try
        {
            region = _memoryMapper.Open(path, CoreMemoryAccessMode.Read);
        }
        catch (Exception ex)
        {
            return MappedModelPath.Fail(Fail(
                request,
                "LIBTORCH_MEMORY_MAP_FAILED",
                ex.Message));
        }

        using (region)
        {
            if (!region.IsMapped || region.Pointer == IntPtr.Zero)
            {
                return MappedModelPath.Fail(Fail(
                    request,
                    "LIBTORCH_MEMORY_MAP_FAILED",
                    "Memory mapper returned an unmapped model region."));
            }

            return MappedModelPath.Success(
                region.Info.Path,
                region.Length);
        }
    }
}

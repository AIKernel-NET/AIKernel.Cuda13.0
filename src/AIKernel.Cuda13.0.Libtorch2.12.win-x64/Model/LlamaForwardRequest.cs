namespace AIKernel.Cuda13.Libtorch2_12.WinX64.Model;

using System.Globalization;

/// <include file="docs.en.xml" path="doc/members/member[@name='T:AIKernel.Cuda13.Libtorch2_12.WinX64.Model.LlamaForwardRequest']" />
/// <include file="docs.ja.xml" path="doc/members/member[@name='T:AIKernel.Cuda13.Libtorch2_12.WinX64.Model.LlamaForwardRequest']" />
public sealed record LlamaForwardRequest(
    int ModelHandle,
    int[] InputIds)
{
    /// <include file="docs.en.xml" path="doc/members/member[@name='F:AIKernel.Cuda13.Libtorch2_12.WinX64.Model.LlamaForwardRequest.MaxInputTokens']" />
    /// <include file="docs.ja.xml" path="doc/members/member[@name='F:AIKernel.Cuda13.Libtorch2_12.WinX64.Model.LlamaForwardRequest.MaxInputTokens']" />
    public const int MaxInputTokens = 4096;

    /// <include file="docs.en.xml" path="doc/members/member[@name='M:AIKernel.Cuda13.Libtorch2_12.WinX64.Model.LlamaForwardRequest.TryCreate']" />
    /// <include file="docs.ja.xml" path="doc/members/member[@name='M:AIKernel.Cuda13.Libtorch2_12.WinX64.Model.LlamaForwardRequest.TryCreate']" />
    public static LlamaForwardRequestParseResult TryCreate(
        IReadOnlyDictionary<string, string> arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);

        if (!arguments.TryGetValue("model_handle", out var modelHandleText) ||
            !int.TryParse(
                modelHandleText,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var modelHandle) ||
            modelHandle <= 0)
        {
            return LlamaForwardRequestParseResult.Fail(
                "Argument 'model_handle' must be a positive integer.");
        }

        if (!arguments.TryGetValue("input_ids", out var inputIdsText) ||
            string.IsNullOrWhiteSpace(inputIdsText))
        {
            return LlamaForwardRequestParseResult.Fail(
                "Argument 'input_ids' is required.");
        }

        var tokens = inputIdsText
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (tokens.Length == 0)
        {
            return LlamaForwardRequestParseResult.Fail(
                "Argument 'input_ids' must contain at least one token id.");
        }

        if (tokens.Length > MaxInputTokens)
        {
            return LlamaForwardRequestParseResult.Fail(
                $"Argument 'input_ids' must contain at most {MaxInputTokens} token ids.");
        }

        var inputIds = new int[tokens.Length];

        for (var i = 0; i < tokens.Length; i++)
        {
            if (!int.TryParse(
                tokens[i],
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out inputIds[i]) ||
                inputIds[i] < 0)
            {
                return LlamaForwardRequestParseResult.Fail(
                    "Argument 'input_ids' must contain non-negative integer token ids.");
            }
        }

        return LlamaForwardRequestParseResult.Success(
            new LlamaForwardRequest(modelHandle, inputIds));
    }
}

/// <include file="docs.en.xml" path="doc/members/member[@name='T:AIKernel.Cuda13.Libtorch2_12.WinX64.Model.LlamaForwardRequestParseResult']" />
/// <include file="docs.ja.xml" path="doc/members/member[@name='T:AIKernel.Cuda13.Libtorch2_12.WinX64.Model.LlamaForwardRequestParseResult']" />
public sealed record LlamaForwardRequestParseResult(
    bool Succeeded,
    LlamaForwardRequest? Value,
    string? ErrorMessage)
{
    /// <include file="docs.en.xml" path="doc/members/member[@name='M:AIKernel.Cuda13.Libtorch2_12.WinX64.Model.LlamaForwardRequestParseResult.Success']" />
    /// <include file="docs.ja.xml" path="doc/members/member[@name='M:AIKernel.Cuda13.Libtorch2_12.WinX64.Model.LlamaForwardRequestParseResult.Success']" />
    public static LlamaForwardRequestParseResult Success(
        LlamaForwardRequest value)
    {
        return new LlamaForwardRequestParseResult(true, value, null);
    }

    /// <include file="docs.en.xml" path="doc/members/member[@name='M:AIKernel.Cuda13.Libtorch2_12.WinX64.Model.LlamaForwardRequestParseResult.Fail']" />
    /// <include file="docs.ja.xml" path="doc/members/member[@name='M:AIKernel.Cuda13.Libtorch2_12.WinX64.Model.LlamaForwardRequestParseResult.Fail']" />
    public static LlamaForwardRequestParseResult Fail(
        string errorMessage)
    {
        return new LlamaForwardRequestParseResult(false, null, errorMessage);
    }
}

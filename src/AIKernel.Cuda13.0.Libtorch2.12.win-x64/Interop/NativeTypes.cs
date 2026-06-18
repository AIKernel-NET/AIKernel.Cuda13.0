namespace AIKernel.Cuda13.Libtorch2_12.WinX64.Interop;

using System.Runtime.InteropServices;

internal static class NativeStatus
{
    internal const int Success = 0;
}

[StructLayout(LayoutKind.Sequential)]
internal struct ForwardResultNative
{
    /// <summary>
    /// EN: Gets Status.
    /// [EN] Documents this public package API member. [JA] Status を取得します。
    /// </summary>
    public int Status;
    /// <summary>
    /// EN: Gets OutputTokenCount.
    /// [EN] Documents this public package API member. [JA] OutputTokenCount を取得します。
    /// </summary>

    public int OutputTokenCount;
    /// <summary>
    /// EN: Gets OutputTokenIds.
    /// [EN] Documents this public package API member. [JA] OutputTokenIds を取得します。
    /// </summary>

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    public int[] OutputTokenIds;
    /// <summary>
    /// EN: Gets LogitCount.
    /// [EN] Documents this public package API member. [JA] LogitCount を取得します。
    /// </summary>

    public int LogitCount;
    /// <summary>
    /// EN: Gets Logits.
    /// [EN] Documents this public package API member. [JA] Logits を取得します。
    /// </summary>

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4096)]
    public float[] Logits;
}

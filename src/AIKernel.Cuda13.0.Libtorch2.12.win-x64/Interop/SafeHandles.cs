namespace AIKernel.Cuda13.Libtorch2_12.WinX64.Interop;

using Microsoft.Win32.SafeHandles;

/// <include file="docs.en.xml" path="doc/members/member[@name='T:AIKernel.Cuda13.Libtorch2_12.WinX64.Interop.SafeLlamaModelHandle']" />
/// <include file="docs.ja.xml" path="doc/members/member[@name='T:AIKernel.Cuda13.Libtorch2_12.WinX64.Interop.SafeLlamaModelHandle']" />
public sealed class SafeLlamaModelHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    /// <include file="docs.en.xml" path="doc/members/member[@name='M:AIKernel.Cuda13.Libtorch2_12.WinX64.Interop.SafeLlamaModelHandle.#ctor']" />
    /// <include file="docs.ja.xml" path="doc/members/member[@name='M:AIKernel.Cuda13.Libtorch2_12.WinX64.Interop.SafeLlamaModelHandle.#ctor']" />
    public SafeLlamaModelHandle()
        : base(ownsHandle: true)
    {
    }

    internal SafeLlamaModelHandle(
        int nativeHandle)
        : base(ownsHandle: true)
    {
        SetHandle(new IntPtr(nativeHandle));
    }

    /// <include file="docs.en.xml" path="doc/members/member[@name='M:AIKernel.Cuda13.Libtorch2_12.WinX64.Interop.SafeLlamaModelHandle.ToInt32']" />
    /// <include file="docs.ja.xml" path="doc/members/member[@name='M:AIKernel.Cuda13.Libtorch2_12.WinX64.Interop.SafeLlamaModelHandle.ToInt32']" />
    public int ModelHandle => handle.ToInt32();

    /// <include file="docs.en.xml" path="doc/members/member[@name='M:AIKernel.Cuda13.Libtorch2_12.WinX64.Interop.SafeLlamaModelHandle.ReleaseHandle']" />
    /// <include file="docs.ja.xml" path="doc/members/member[@name='M:AIKernel.Cuda13.Libtorch2_12.WinX64.Interop.SafeLlamaModelHandle.ReleaseHandle']" />
    protected override bool ReleaseHandle()
    {
        try
        {
            return NativeMethods.UnloadModel(ModelHandle) == NativeStatus.Success;
        }
        catch (DllNotFoundException)
        {
            return false;
        }
        catch (EntryPointNotFoundException)
        {
            return false;
        }
        catch (BadImageFormatException)
        {
            return false;
        }
    }
}

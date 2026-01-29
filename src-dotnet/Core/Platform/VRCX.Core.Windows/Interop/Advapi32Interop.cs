using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace VRCX.Core.Windows.Interop;

public partial class Advapi32Interop
{
    [LibraryImport("advapi32.dll", EntryPoint = "RegSetValueExW", StringMarshalling = StringMarshalling.Utf16)]
    internal static partial int RegSetValueEx(
        SafeRegistryHandle hKey,
        string lpValueName,
        int Reserved,
        int dwType,
        byte[] lpData,
        int cbData);
}
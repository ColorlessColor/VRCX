using System.Runtime.InteropServices;

namespace VRCX.Core.Windows.Interop;

public partial class Shell32Interop
{
    internal static Guid FolderIdLocalAppDataLow = new("A520A1A4-1780-4FF6-BD18-167343C5AF16");
    
    [LibraryImport("shell32.dll", SetLastError = false, StringMarshalling = StringMarshalling.Utf16)]
    internal static partial int SHGetKnownFolderPath(
        in Guid rfid,
        uint dwFlags,
        nint hToken,
        out string ppszPath);
}
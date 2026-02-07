using VRCX.Core.Services.Platform;

namespace VRCX.LegacyApp.ElectronGlue.Services.Platform;

public sealed class ElectronFileDialogSerivice : IFileDialogService
{
    public ValueTask<string> OpenFolderSelectorDialogAsync(string defaultPath = "")
    {
        throw new NotImplementedException();
    }

    public ValueTask<string> OpenFileSelectorDialogAsync(string defaultPath = "", string defaultExt = "",
        string defaultFilter = "All files (*.*)|*.*")
    {
        throw new NotImplementedException();
    }

    public ValueTask HighlightInFileExplorerAsync(string path)
    {
        throw new NotImplementedException();
    }
}
using System.Threading.Tasks;

namespace VRCX.Core.Services.Platform;

public interface IFileDialogService
{
    ValueTask<string> OpenFolderSelectorDialogAsync(string defaultPath = "");

    ValueTask<string> OpenFileSelectorDialogAsync(
        string defaultPath = "",
        string defaultExt = "",
        string defaultFilter = "All files (*.*)|*.*");

    ValueTask HighlightInFileExplorerAsync(string path);
}
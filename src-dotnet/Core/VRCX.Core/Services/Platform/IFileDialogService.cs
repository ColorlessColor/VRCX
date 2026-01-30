using System.Threading.Tasks;

namespace VRCX.Core.Services.Platform;

public interface IFileDialogService
{
    ValueTask<string> OpenFolderSelectorDialogAsync(string defaultPath = "");
}
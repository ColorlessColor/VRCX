using System;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using VRCX.Core.Services.Platform;

namespace VRCX.App.Services;

public sealed class FileDialogService(AppWindowService appWindowService) : IFileDialogService
{
    public async ValueTask<string> OpenFolderSelectorDialogAsync(string defaultPath = "")
    {
        var topLevel = appWindowService.GetTopLevel();
        if (topLevel == null)
            throw new InvalidOperationException("No top-level window available for file dialog.");

        var storageProvider = topLevel.StorageProvider;
        var initialFolder = await storageProvider.TryGetFolderFromPathAsync(defaultPath);

        var result = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            AllowMultiple = false,
            SuggestedStartLocation = initialFolder
        });

        if (result.Count == 0)
        {
            return defaultPath;
        }

        return result[0].TryGetLocalPath() ??
               throw new InvalidOperationException("Selected folder does not have a local path.");
    }
}
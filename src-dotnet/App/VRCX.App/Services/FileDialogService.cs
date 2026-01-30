using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using VRCX.Core.Services.Platform;

namespace VRCX.App.Services;

public sealed class FileDialogService(AppWindowService appWindowService) : IFileDialogService
{
    public async ValueTask<string> OpenFolderSelectorDialogAsync(string defaultPath = "")
    {
        var storageProvider = GetStorageProvider();
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

    public async ValueTask<string> OpenFileSelectorDialogAsync(string defaultPath = "", string defaultExt = "",
        string defaultFilter = "All files (*.*)|*.*")
    {
        var storageProvider = GetStorageProvider();
        var initialFolder = await storageProvider.TryGetFolderFromPathAsync(defaultPath);

        var filters = ParseFilter(defaultFilter);
        var defaultFileType = string.IsNullOrWhiteSpace(defaultExt)
            ? filters[0]
            : filters.FirstOrDefault(f => f.Patterns?.Contains($"*{defaultExt}") ?? false)
              ?? new FilePickerFileType(defaultExt)
              {
                  Patterns = [$"*{defaultExt}"]
              };

        var result = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = false,
            SuggestedStartLocation = initialFolder,
            SuggestedFileType = defaultFileType,
            FileTypeFilter = filters
        });

        if (result.Count == 0)
        {
            return "";
        }

        return result[0].TryGetLocalPath() ??
               throw new InvalidOperationException("Selected file does not have a local path.");
    }

    private FilePickerFileType[] ParseFilter(string filter)
    {
        // "PNG Files (*.png)|*.png"
        // "All files (*.*)|*.*"
        var parts = filter.Split('|', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0 || parts.Length % 2 != 0)
            throw new ArgumentException("Invalid filter format.", nameof(filter));

        var fileTypes = new FilePickerFileType[parts.Length / 2];
        for (var i = 0; i < parts.Length; i += 2)
        {
            var name = parts[i];
            var patterns = parts[i + 1].Split(';', StringSplitOptions.RemoveEmptyEntries);
            fileTypes[i / 2] = new FilePickerFileType(name)
            {
                Patterns = patterns
            };
        }

        return fileTypes;
    }

    private IStorageProvider GetStorageProvider()
    {
        var topLevel = appWindowService.GetTopLevel();
        if (topLevel == null)
            throw new InvalidOperationException("No top-level window available for file dialog.");

        return topLevel.StorageProvider;
    }
}
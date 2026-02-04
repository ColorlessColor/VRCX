using System.Diagnostics;
using NLog;
using VRCX.Core.Services.Platform;
using VRCX.LegacyApp.WinFormsCef.Cef;
using VRCX.LegacyApp.WinFormsCef.CoreGlue.Utils;

namespace VRCX.LegacyApp.WinFormsCef.CoreGlue.Services.Platform;

public class WinFormsFileDialogService(IGameFolderProvider gameFolderProvider) : IFileDialogService
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public async ValueTask<string> OpenFolderSelectorDialogAsync(string defaultPath = "")
    {
        return await StaThreadUtils.RunOnStaThread(() =>
        {
            using var openFolderDialog = new FolderBrowserDialog();
            openFolderDialog.InitialDirectory =
                Directory.Exists(defaultPath) ? defaultPath : gameFolderProvider.GetVRChatPhotosLocation();

            var dialogResult = openFolderDialog.ShowDialog(MainForm.nativeWindow);
            if (dialogResult == DialogResult.OK)
            {
                return openFolderDialog.SelectedPath;
            }

            return defaultPath;
        });
    }

    public async ValueTask<string> OpenFileSelectorDialogAsync(
        string defaultPath = "",
        string defaultExt = "",
        string defaultFilter = "All files (*.*)|*.*"
    )
    {
        return await StaThreadUtils.RunOnStaThread(() =>
        {
            using var openFileDialog = new OpenFileDialog();
            if (Directory.Exists(defaultPath))
            {
                openFileDialog.InitialDirectory = defaultPath;
            }

            openFileDialog.DefaultExt = defaultExt;
            openFileDialog.Filter = defaultFilter;

            var dialogResult = openFileDialog.ShowDialog(MainForm.nativeWindow);
            if (dialogResult == DialogResult.OK && !string.IsNullOrEmpty(openFileDialog.FileName))
            {
                return openFileDialog.FileName;
            }

            return "";
        });
    }

    public ValueTask HighlightInFileExplorerAsync(string path)
    {
        if (File.Exists(path))
        {
            Process.Start("explorer.exe", $"/select,\"{path}\"");
            return ValueTask.CompletedTask;
        }

        if (Directory.Exists(path))
        {
            Process.Start("explorer.exe", path);
            return ValueTask.CompletedTask;
        }

        _logger.Warn("Trying to highlight a path that does not exist: {FileOrDirectoryPath}", path);
        return ValueTask.CompletedTask;
    }
}
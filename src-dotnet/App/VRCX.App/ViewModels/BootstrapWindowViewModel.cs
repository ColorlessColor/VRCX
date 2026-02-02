using Avalonia.Threading;
using NLog;
using VRCX.App.Services;
using VRCX.App.Views;
using VRCX.Core;
using VRCX.Core.Services.Platform;

namespace VRCX.App.ViewModels;

public sealed class BootstrapWindowViewModel(
    MainWindowViewModel mainWindowViewModel,
    NativeMessageBoxService nativeMessageBoxService,
    BootstrapDelegate bootstrapDelegate
)
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public string Version
    {
        get
        {
            var versionString = AppBuildInfoService.Version;
            if (versionString.StartsWith("VRCX"))
            {
                versionString = versionString["VRCX ".Length..];
            }

            return versionString;
        }
    }

    public event EventHandler? RequestClose;

    public async Task BootstrapAsync()
    {
        try
        {
            await bootstrapDelegate();
        }
        catch (Exception ex)
        {
            _logger.Fatal(ex, "An error occurred during Bootstrap.");
            await nativeMessageBoxService.ShowAsync(
                ex.ToString(),
                "An error occurred during startup.",
                NativeMessageBoxIcon.Error);

            Dispatcher.UIThread.InvokeShutdown();
            return;
        }

        var mainWindow = new MainWindow()
        {
            DataContext = mainWindowViewModel
        };

        mainWindow.Show();
        mainWindow.Activate();

        RequestClose?.Invoke(this, EventArgs.Empty);
    }
}

public sealed class BootstrapWindowViewModelFactory(
    MainWindowViewModel mainWindowViewModel,
    NativeMessageBoxService nativeMessageBoxService
)
{
    public BootstrapWindowViewModel Create(BootstrapDelegate bootstrapDelegate) =>
        new(mainWindowViewModel, nativeMessageBoxService, bootstrapDelegate);
}

public delegate Task BootstrapDelegate();
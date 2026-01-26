using System;
using System.Threading.Tasks;
using VRCX.App.Views;

namespace VRCX.App.ViewModels;

public sealed class BootstrapWindowViewModel(
    MainWindowViewModel mainWindowViewModel,
    BootstrapDelegate bootstrapDelegate
)
{
    public event EventHandler? RequestClose;

    public async Task BootstrapAsync()
    {
        await bootstrapDelegate();

        var mainWindow = new MainWindow()
        {
            DataContext = mainWindowViewModel
        };

        mainWindow.Show();
        mainWindow.Activate();

        RequestClose?.Invoke(this, EventArgs.Empty);
    }
}

public sealed class BootstrapWindowViewModelFactory(MainWindowViewModel mainWindowViewModel)
{
    public BootstrapWindowViewModel Create(BootstrapDelegate bootstrapDelegate) =>
        new(mainWindowViewModel, bootstrapDelegate);
}

public delegate Task BootstrapDelegate();
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using VRCX.App.ViewModels;

namespace VRCX.App.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        // Notice: Running WebView initialization outside of UI thread will cause issues.
        Dispatcher.UIThread.InvokeAsync(Load);
    }

    private async Task Load()
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.SetMainWindow(this);
            await viewModel.LoadAsync().ConfigureAwait(true);
        }
    }
}
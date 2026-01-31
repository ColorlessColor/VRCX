using Avalonia.Controls;
using Avalonia.Interactivity;
using VRCX.App.ViewModels;

namespace VRCX.App.Views;

public partial class BootstrapWindow : Window
{
    public BootstrapWindow()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (DataContext is not BootstrapWindowViewModel bootstrapWindowViewModel) return;

        bootstrapWindowViewModel.RequestClose += OnRequestClose;
        _ = bootstrapWindowViewModel.BootstrapAsync();
    }

    private void OnRequestClose(object? sender, EventArgs e)
    {
        Close();
    }
}
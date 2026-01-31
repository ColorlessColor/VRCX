using Avalonia.Controls;
using Avalonia.Interactivity;

namespace VRCX.App.Views;

public partial class MessageBoxWindow : Window
{
    private readonly TaskCompletionSource _tcs = new();

    public MessageBoxWindow()
    {
        InitializeComponent();
    }

    public static async Task CreateAndShowAsync(string message, string title)
    {
        var msgBox = new MessageBoxWindow
        {
            Title = title,
            MessageBoxText =
            {
                Text = message
            }
        };

        msgBox.Show();
        await msgBox._tcs.Task;
    }

    private void OkButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
        _tcs.SetResult();
    }
}